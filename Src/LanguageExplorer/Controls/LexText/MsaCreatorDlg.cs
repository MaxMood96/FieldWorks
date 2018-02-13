// Copyright (c) 2004-2018 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using SIL.LCModel.Core.Text;
using SIL.LCModel.Core.KernelInterfaces;
using SIL.FieldWorks.Common.FwUtils;
using SIL.FieldWorks.Common.Widgets;
using SIL.LCModel;
using SIL.LCModel.DomainServices;
using SIL.Windows.Forms;

namespace LanguageExplorer.Controls.LexText
{
	/// <summary>
	/// Summary description for MsaCreatorDlg.
	/// </summary>
	public class MsaCreatorDlg : Form
	{
		#region Data Members

		private LcmCache m_cache;
		private IPropertyTable m_propertyTable;
		private IPublisher m_publisher;

		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnHelp;
		private System.Windows.Forms.Label label1;
		private SIL.FieldWorks.Common.Widgets.FwTextBox m_fwtbCitationForm;
		private System.Windows.Forms.Label label2;
		private SIL.FieldWorks.Common.Widgets.FwTextBox m_fwtbSenses;
		private LanguageExplorer.Controls.LexText.MSAGroupBox m_msaGroupBox;

		private string s_helpTopic = "khtpCreateNewGrammaticalFunction";
		private System.Windows.Forms.HelpProvider helpProvider;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		#endregion Data Members

		#region Properties

		public SandboxGenericMSA SandboxMSA => m_msaGroupBox.SandboxMSA;
		#endregion Properties

		#region Construction, Initialization, and Disposal

		/// <summary>
		/// Constructor.
		/// </summary>
		public MsaCreatorDlg()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			AccessibleName = GetType().Name;
			helpProvider = new HelpProvider();
		}

		/// <summary>
		/// Initialize the dialog before showing it.
		/// </summary>
		public void SetDlgInfo(LcmCache cache, IPersistenceProvider persistProvider, IPropertyTable propertyTable, IPublisher publisher, ILexEntry entry, SandboxGenericMSA sandboxMsa, int hvoOriginalMsa,
			bool useForEdit, string titleForEdit)
		{
			Debug.Assert(m_cache == null);
			m_cache = cache;
			m_propertyTable = propertyTable;
			m_publisher = publisher;

			if (useForEdit)
			{
				// Change the window title and the OK button text.
				Text = titleForEdit;
				s_helpTopic = "khtpEditGrammaticalFunction";
				btnOk.Text = LexTextControls.ks_OK;
			}
			var helpTopicProvider = m_propertyTable.GetValue<IHelpTopicProvider>("HelpTopicProvider");
			helpProvider.HelpNamespace = helpTopicProvider.HelpFile;
			helpProvider.SetHelpKeyword(this, helpTopicProvider.GetHelpString(s_helpTopic));
			helpProvider.SetHelpNavigator(this, HelpNavigator.Topic);

			// Set font, writing system factory, and code for the edit box.
			var fntSize = label1.Font.Size * 2.0F;
			var defVernWs = m_cache.ServiceLocator.WritingSystems.DefaultVernacularWritingSystem;
			m_fwtbCitationForm.Font = new Font(defVernWs.DefaultFontName, fntSize);
			m_fwtbCitationForm.WritingSystemFactory = m_cache.WritingSystemFactory;
			m_fwtbCitationForm.WritingSystemCode = defVernWs.Handle;
			m_fwtbCitationForm.AdjustForStyleSheet(this, null, m_propertyTable);
			m_fwtbCitationForm.AdjustStringHeight = false;
			m_fwtbCitationForm.Tss = entry.HeadWord;
			m_fwtbCitationForm.HasBorder = false;

			m_fwtbSenses.Font = new Font(defVernWs.DefaultFontName, fntSize);
			m_fwtbSenses.WritingSystemFactory = m_cache.WritingSystemFactory;
			m_fwtbSenses.WritingSystemCode = defVernWs.Handle;
			m_fwtbSenses.AdjustForStyleSheet(this, null, m_propertyTable);
			m_fwtbSenses.AdjustStringHeight = false;

			var tisb = TsStringUtils.MakeIncStrBldr();
			tisb.SetIntPropValues((int)FwTextPropType.ktptWs, 0, m_cache.DefaultAnalWs);
			var msaRepository = m_cache.ServiceLocator.GetInstance<IMoMorphSynAnalysisRepository>();
			if (hvoOriginalMsa != 0)
			{
				foreach (var sense in entry.AllSenses)
				{
					if (sense.MorphoSyntaxAnalysisRA != null)
					{
						if (sense.MorphoSyntaxAnalysisRA == msaRepository.GetObject(hvoOriginalMsa))
						{
							if (tisb.Text != null)
							{
								tisb.Append(", ");	// REVIEW: IS LOCALIZATION NEEDED FOR BUILDING THIS LIST?
							}
							tisb.AppendTsString(sense.ShortNameTSS);
						}
					}
				}
			}
			m_fwtbSenses.Tss = tisb.GetString();
			m_fwtbSenses.HasBorder = false;

			m_msaGroupBox.Initialize(m_cache, m_propertyTable, m_publisher, this, sandboxMsa);
			var oldHeight = m_msaGroupBox.Height;
			var newHeight = Math.Max(oldHeight, m_msaGroupBox.PreferredHeight);
			var delta = newHeight - oldHeight;
			if (delta > 0)
			{
				m_msaGroupBox.AdjustInternalControlsAndGrow();
				Debug.Assert(m_msaGroupBox.Height == m_msaGroupBox.PreferredHeight);
				FontHeightAdjuster.GrowDialogAndAdjustControls(this, delta, m_msaGroupBox);
			}

			// Reset window location.
			// Get location to the stored values, if any.
			Point dlgLocation;
			if (m_propertyTable.TryGetValue("msaCreatorDlgLocation", out dlgLocation))
			{
				// JohnT: this dialog can't be resized. So it doesn't make sense to
				// remember a size. If we do, we need to override OnLoad (as in SimpleListChooser)
				// to prevent the dialog growing every time at 120 dpi. But such an override
				// makes it too small to show all the controls at the default size.
				// It's better just to use the default size until it's resizeable for some reason.
				//var dlgSize = m_propertyTable.GetValue<Size>("msaCreatorDlgSize");
				var rect = new Rectangle(dlgLocation, Size);
				ScreenHelper.EnsureVisibleRect(ref rect);
				DesktopBounds = rect;
				StartPosition = FormStartPosition.Manual;
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			Debug.WriteLineIf(!disposing, "****** Missing Dispose() call for " + GetType().Name + ". ****** ");
			// Must not be run more than once.
			if (IsDisposed)
			{
				return;
			}

			if (disposing)
			{
				components?.Dispose();
			}
			m_cache = null;

			base.Dispose( disposing );
		}

		#endregion Construction, Initialization, and Disposal

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MsaCreatorDlg));
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnHelp = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.m_fwtbCitationForm = new SIL.FieldWorks.Common.Widgets.FwTextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.m_fwtbSenses = new SIL.FieldWorks.Common.Widgets.FwTextBox();
			this.m_msaGroupBox = new LanguageExplorer.Controls.LexText.MSAGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.m_fwtbCitationForm)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_fwtbSenses)).BeginInit();
			this.SuspendLayout();
			//
			// btnCancel
			//
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.Name = "btnCancel";
			//
			// btnOk
			//
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			resources.ApplyResources(this.btnOk, "btnOk");
			this.btnOk.Name = "btnOk";
			//
			// btnHelp
			//
			resources.ApplyResources(this.btnHelp, "btnHelp");
			this.btnHelp.Name = "btnHelp";
			this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
			//
			// label1
			//
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			//
			// m_fwtbCitationForm
			//
			this.m_fwtbCitationForm.AdjustStringHeight = true;
			this.m_fwtbCitationForm.BackColor = System.Drawing.SystemColors.Control;
			this.m_fwtbCitationForm.controlID = null;
			resources.ApplyResources(this.m_fwtbCitationForm, "m_fwtbCitationForm");
			this.m_fwtbCitationForm.Name = "m_fwtbCitationForm";
			this.m_fwtbCitationForm.SelectionLength = 0;
			this.m_fwtbCitationForm.SelectionStart = 0;
			//
			// label2
			//
			resources.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			//
			// m_fwtbSenses
			//
			this.m_fwtbSenses.AdjustStringHeight = true;
			this.m_fwtbSenses.BackColor = System.Drawing.SystemColors.Control;
			this.m_fwtbSenses.controlID = null;
			resources.ApplyResources(this.m_fwtbSenses, "m_fwtbSenses");
			this.m_fwtbSenses.Name = "m_fwtbSenses";
			this.m_fwtbSenses.SelectionLength = 0;
			this.m_fwtbSenses.SelectionStart = 0;
			//
			// m_msaGroupBox
			//
			resources.ApplyResources(this.m_msaGroupBox, "m_msaGroupBox");
			this.m_msaGroupBox.MSAType = MsaType.kNotSet;
			this.m_msaGroupBox.Name = "m_msaGroupBox";
			this.m_msaGroupBox.Slot = null;
			//
			// MsaCreatorDlg
			//
			this.AcceptButton = this.btnOk;
			resources.ApplyResources(this, "$this");
			this.CancelButton = this.btnCancel;
			this.Controls.Add(this.m_msaGroupBox);
			this.Controls.Add(this.m_fwtbSenses);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.m_fwtbCitationForm);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnHelp);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.btnCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MsaCreatorDlg";
			this.ShowInTaskbar = false;
			this.Closed += new System.EventHandler(this.MsaCreatorDlg_Closed);
			((System.ComponentModel.ISupportInitialize)(this.m_fwtbCitationForm)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_fwtbSenses)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		#region Event handlers

		private void MsaCreatorDlg_Closed(object sender, System.EventArgs e)
		{
			m_propertyTable?.SetProperty("msaCreatorDlgLocation", Location, true, true);
		}

		private void btnHelp_Click(object sender, EventArgs e)
		{
			ShowHelp.ShowHelpTopic(m_propertyTable.GetValue<IHelpTopicProvider>("HelpTopicProvider"), s_helpTopic);
		}

		#endregion Event handlers
	}
}
