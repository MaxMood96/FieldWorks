// Copyright (c) 2005-2018 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System.Diagnostics;
using SIL.LCModel;
using SIL.FieldWorks.Common.FwUtils;

namespace LanguageExplorer.Controls.DetailControls
{
	/// <summary>
	/// Summary description for DerivMSAReferenceSlice.
	/// </summary>
	internal class DerivMSAReferenceSlice : AtomicReferenceSlice
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AtomicReferenceSlice"/> class.
		/// </summary>
		internal DerivMSAReferenceSlice(LcmCache cache, ICmObject obj, int flid)
			: base(cache, obj, flid)
		{
		}

		#region IDisposable override

		/// <summary>
		/// Executes in two distinct scenarios.
		///
		/// 1. If disposing is true, the method has been called directly
		/// or indirectly by a user's code via the Dispose method.
		/// Both managed and unmanaged resources can be disposed.
		///
		/// 2. If disposing is false, the method has been called by the
		/// runtime from inside the finalizer and you should not reference (access)
		/// other managed objects, as they already have been garbage collected.
		/// Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing"></param>
		/// <remarks>
		/// If any exceptions are thrown, that is fine.
		/// If the method is being done in a finalizer, it will be ignored.
		/// If it is thrown by client code calling Dispose,
		/// it needs to be handled by fixing the bug.
		///
		/// If subclasses override this method, they should call the base implementation.
		/// </remarks>
		protected override void Dispose(bool disposing)
		{
			// Must not be run more than once.
			if (IsDisposed)
			{
				return;
			}

			if (disposing)
			{
				// Dispose managed resources here.
				((AtomicReferenceLauncher)Control).ReferenceChanged -= OnReferenceChanged;
			}

			// Dispose unmanaged resources here, whether disposing is true or false.

			base.Dispose(disposing);
		}

		#endregion IDisposable override

		public override void FinishInit()
		{
			base.FinishInit();
			((AtomicReferenceLauncher)Control).ReferenceChanged += OnReferenceChanged;
		}

		#region Event handlers

		/// <summary>
		/// Handle interaction between to and from POS for a derivational affix MSA.
		/// </summary>
		/// <remarks>
		/// If the new value is zero, then set the other one's value to zero, as well.
		/// If the other one's value is zero, then set it to the new value.
		/// In all cases, set this one's value to the new value.
		/// </remarks>
		protected void OnReferenceChanged(object sender, FwObjectSelectionEventArgs e)
		{
			Debug.Assert(sender is AtomicReferenceLauncher);
			var source = (AtomicReferenceLauncher)sender;
			Debug.Assert(Control == source);
			Debug.Assert(Object is IMoDerivAffMsa);

			AtomicReferenceLauncher otherControl = null;
			var idxSender = ContainingDataTree.Slices.IndexOf(this);
			int otherFlid;
			var myIsFromPOS = true;
			if (m_flid == MoDerivAffMsaTags.kflidFromPartOfSpeech)
			{
				otherFlid = MoDerivAffMsaTags.kflidToPartOfSpeech;
			}
			else
			{
				otherFlid = MoDerivAffMsaTags.kflidFromPartOfSpeech;
				myIsFromPOS = false;
			}
			var otherHvo = 0;
			Slice otherSlice = null;
			int idxOther;
			if (idxSender > 0)
			{
				idxOther = idxSender - 1;
				while (otherSlice == null || (otherSlice.Indent == Indent && idxOther > 0 && otherSlice.Object == Object))
				{
					otherSlice = ContainingDataTree.Slices[idxOther--];
					if (otherSlice is AtomicReferenceSlice && (otherSlice as AtomicReferenceSlice).Flid == otherFlid)
					{
						break;
					}
				}

				if (otherSlice is AtomicReferenceSlice)
				{
					otherHvo = GetOtherHvo((AtomicReferenceSlice)otherSlice, otherFlid, myIsFromPOS, out otherControl);
				}
				else
				{
					otherSlice = null;
				}
			}
			if (otherControl == null && idxSender < ContainingDataTree.Slices.Count)
			{
				idxOther = idxSender + 1;
				while (otherSlice == null || (otherSlice.Indent == Indent && idxOther > 0 && otherSlice.Object == Object))
				{
					otherSlice = ContainingDataTree.Slices[idxOther++];
					if (otherSlice is AtomicReferenceSlice && ((AtomicReferenceSlice)otherSlice).Flid == otherFlid)
					{
						break;
					}
				}

				if (otherSlice is AtomicReferenceSlice)
				{
					otherHvo = GetOtherHvo((AtomicReferenceSlice)otherSlice, otherFlid, myIsFromPOS, out otherControl);
				}
			}

			var msa = Object as IMoDerivAffMsa;
			if (e.Hvo == 0 && otherHvo != 0)
			{
				if (otherControl != null)
				{
					if (m_flid == MoDerivAffMsaTags.kflidFromPartOfSpeech)
					{
						msa.ToPartOfSpeechRA = null;
					}
					else
					{
						msa.FromPartOfSpeechRA = null;
					}
				}
			}
			else if (otherHvo == 0 && e.Hvo > 0)
			{
				if (otherControl == null)
				{
					// The other one is not available (filtered out?),
					// so set it directly using the msa.
					if (m_flid == MoDerivAffMsaTags.kflidFromPartOfSpeech)
					{
						msa.ToPartOfSpeechRA = Cache.ServiceLocator.GetInstance<IPartOfSpeechRepository>().GetObject(e.Hvo);
					}
					else
					{
						msa.FromPartOfSpeechRA = Cache.ServiceLocator.GetInstance<IPartOfSpeechRepository>().GetObject(e.Hvo);
					}
				}
				else
				{
					otherControl.AddItem(Cache.ServiceLocator.GetObject(e.Hvo)); // Set the other guy to this value.
				}
			}
		}

		private int GetOtherHvo(AtomicReferenceSlice otherSlice, int otherFlid, bool myIsFromPOS, out AtomicReferenceLauncher otherOne)
		{
			otherOne = null;
			var otherHvo = 0;

			if (otherSlice != null)
			{

				var otherControl = otherSlice.Control as AtomicReferenceLauncher;
				if (otherSlice.Object == Object && (otherSlice.Flid == otherFlid))
				{
					otherOne = otherControl;
					otherHvo = myIsFromPOS ? ((IMoDerivAffMsa)Object).ToPartOfSpeechRA.Hvo : ((IMoDerivAffMsa)Object).FromPartOfSpeechRA.Hvo;
				}
			}
			return otherHvo;
		}

		#endregion Event handlers
	}
}