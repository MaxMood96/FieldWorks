// Copyright (c) 2005-2018 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using LanguageExplorer.Controls.DetailControls;
using LanguageExplorer.LcmUi;
using SIL.FieldWorks.Common.FwUtils;
using SIL.FieldWorks.Common.RootSites;
using SIL.LCModel;
using SIL.LCModel.Core.KernelInterfaces;
using SIL.LCModel.Core.Phonology;
using SIL.LCModel.DomainServices;

namespace LanguageExplorer.Areas
{
	internal class StringRepSliceView : RootSiteControl, INotifyControlInCurrentSlice
	{
		IPhEnvironment m_env;
		int m_hvoObj;
		StringRepSliceVc m_vc = null;
		private PhonEnvRecognizer m_validator;

		public StringRepSliceView(int hvo)
		{
			m_hvoObj = hvo;
		}

		public void ResetValidator()
		{
			m_validator = new PhonEnvRecognizer(m_cache.LangProject.PhonologicalDataOA.AllPhonemes().ToArray(), m_cache.LangProject.PhonologicalDataOA.AllNaturalClassAbbrs().ToArray());
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			// Must not be run more than once.
			if (IsDisposed)
				return;

			base.Dispose(disposing);

			if (disposing)
			{
			}

			m_env = null;
			m_vc = null;
			m_validator = null; // TODO: Make m_validator disposable?
		}

		#region INotifyControlInCurrentSlice implementation

		/// <summary>
		/// Adjust controls based on whether the slice is the current slice.
		/// </summary>
		public bool SliceIsCurrent
		{
			set
			{
				// SliceIsCurrent may be called in the process of deleting the object after the object
				// has been partially cleared out and thus would certainly fail the constraint
				// check, then try to instantiate an error annotation which wouldn't have an
				// owner, causing bad things to happen.
				if (DesignMode || m_rootb == null || !m_env.IsValidObject)
				{
					return;
				}

				if (!value)
				{
					DoValidation(true); // JohnT: do we really always want a Refresh? Trying to preserve the previous behavior...
				}
			}
		}

		#endregion INotifyControlInCurrentSlice implementation

		private void DoValidation(bool refresh)
		{
			var frm = FindForm();
			// frm may be null, if the record has been switched
			WaitCursor wc = null;
			try
			{
				if (frm != null)
				{
					wc = new WaitCursor(frm);
				}
				ConstraintFailure failure;
				m_env.CheckConstraints(PhEnvironmentTags.kflidStringRepresentation, true, out failure, /* adjust the squiggly line */ true);
				// This will make the record list update to the new value.
				if (refresh)
				{
					Publisher.Publish("Refresh", null);
				}
			}
			finally
			{
				wc?.Dispose();
			}
		}

		/// <summary>
		/// This method seems to get called when we are switching to another tool (or area, or slice) AND when the
		/// program is shutting down. This makes it a good point to check constraints, since in some of these
		/// cases, SliceIsCurrent may not get set false.
		/// </summary>
		protected override void OnValidating(System.ComponentModel.CancelEventArgs e)
		{
			base.OnValidating(e);
			// Only necessary to ensure that validation is done when window is going away. We don't need a Refresh then!
			// Also, in some cases (LT-15730) we come back through here on Undo when we have a deleted object.
			// Don't do validation then.
			if (m_env.IsValidObject)
			{
				DoValidation(false);
			}
		}

		public override void MakeRoot()
		{
			if (m_cache == null || DesignMode)
			{
				return;
			}

			// A crude way of making sure the property we want is loaded into the cache.
			m_env = m_cache.ServiceLocator.GetInstance<IPhEnvironmentRepository>().GetObject(m_hvoObj);
			m_vc = new StringRepSliceVc();

			base.MakeRoot();

			// And maybe this too, at least by default?
			m_rootb.DataAccess = m_cache.MainCacheAccessor;

			// arg3 is a meaningless initial fragment, since this VC only displays one thing.
			// arg4 could be used to supply a stylesheet.
			m_rootb.SetRootObject(m_hvoObj, m_vc, StringRepSliceVc.Flid, null);
		}

		internal bool CanShowEnvironmentError()
		{
			var s = m_env.StringRepresentation.Text;
			if (string.IsNullOrEmpty(s))
			{
				return false;
			}
			return (!m_validator.Recognize(s));
		}

		internal void ShowEnvironmentError()
		{
			var s = m_env.StringRepresentation.Text;
			if (string.IsNullOrEmpty(s))
			{
				return;
			}
			if (!m_validator.Recognize(s))
			{
				string sMsg;
				int pos;
				PhonEnvRecognizer.CreateErrorMessageFromXml(s, m_validator.ErrorMessage, out pos, out sMsg);
				MessageBox.Show(sMsg, AreaResources.ksErrorInEnvironment, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		internal bool CanInsertSlash
		{
			get
			{
				var s = m_env.StringRepresentation.Text;
				return string.IsNullOrEmpty(s) || s.IndexOf('/') < 0;
			}
		}

		private int GetSelectionEndPoint(bool fEnd)
		{
			var vwsel = m_rootb.Selection;
			if (vwsel == null)
			{
				return -1;
			}
			int ichEnd;
			ITsString tss;
			bool fAssocPrev;
			int hvo;
			int flid;
			int ws;
			vwsel.TextSelInfo(fEnd, out tss, out ichEnd, out fAssocPrev, out hvo, out flid, out ws);
			Debug.Assert(hvo == m_env.Hvo);
			Debug.Assert(flid == PhEnvironmentTags.kflidStringRepresentation);
			return ichEnd;
		}

		internal bool CanInsertEnvBar
		{
			get
			{
				var s = m_env.StringRepresentation.Text;
				if (string.IsNullOrEmpty(s))
				{
					return false;
				}
				var ichSlash = s.IndexOf('/');
				if (ichSlash < 0)
				{
					return false;
				}
				var ichEnd = GetSelectionEndPoint(true);
				if (ichEnd < 0)
				{
					return false;
				}
				var ichAnchor = GetSelectionEndPoint(false);
				if (ichAnchor < 0)
				{
					return false;
				}
				return (ichEnd > ichSlash) && (ichAnchor > ichSlash) && (s.IndexOf('_') < 0);
			}
		}

		internal bool CanInsertItem
		{
			get
			{
				var s = m_env.StringRepresentation.Text;
				if (string.IsNullOrEmpty(s))
				{
					return false;
				}
				var ichEnd = GetSelectionEndPoint(true);
				var ichAnchor = GetSelectionEndPoint(false);
				return PhonEnvRecognizer.CanInsertItem(s, ichEnd, ichAnchor);
			}
		}

		internal bool CanInsertHashMark
		{
			get
			{
				var s = m_env.StringRepresentation.Text;
				if (string.IsNullOrEmpty(s))
				{
					return false;
				}
				var ichEnd = GetSelectionEndPoint(true);
				var ichAnchor = GetSelectionEndPoint(false);
				return PhonEnvRecognizer.CanInsertHashMark(s, ichEnd, ichAnchor);
			}
		}

		#region Handle right click menu
		protected override bool OnRightMouseUp(Point pt, Rectangle rcSrcRoot, Rectangle rcDstRoot)
		{
			if (m_env == null)
			{
				return false;
			}
			// We need a CmObjectUi in order to call HandleRightClick().
			using (var ui = new CmObjectUi(m_env))
			{
				ui.InitializeFlexComponent(new FlexComponentParameters(PropertyTable, Publisher, Subscriber));
				return ui.HandleRightClick(this, true, "mnuEnvChoices");
			}
		}
		#endregion
	}
}