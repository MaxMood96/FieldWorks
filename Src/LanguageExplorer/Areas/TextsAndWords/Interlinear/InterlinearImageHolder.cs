// Copyright (c) 2003-2018 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System.Windows.Forms;
using System.Diagnostics;

namespace LanguageExplorer.Areas.TextsAndWords.Interlinear
{
	/// <summary>
	/// Summary description for InterlinearImageHolder.
	/// </summary>
	public class InterlinearImageHolder : UserControl
	{
		private System.Windows.Forms.Button button1;
		public ImageList buttonImages;
		private System.ComponentModel.IContainer components;

		/// <summary>
		/// Initializes a new instance of the <see cref="InterlinearImageHolder"/> class.
		/// </summary>
		public InterlinearImageHolder()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitForm call

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			Debug.WriteLineIf(!disposing, "****************** Missing Dispose() call for " + GetType() + ". ******************");
			// Must not be run more than once.
			if (IsDisposed)
			{
				return;
			}

			if( disposing )
			{
				components?.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InterlinearImageHolder));
			this.button1 = new System.Windows.Forms.Button();
			this.buttonImages = new System.Windows.Forms.ImageList(this.components);
			this.SuspendLayout();
			//
			// button1
			//
			this.button1.ImageIndex = 0;
			this.button1.ImageList = this.buttonImages;
			this.button1.Location = new System.Drawing.Point(32, 40);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 56);
			this.button1.TabIndex = 0;
			this.button1.Text = "button1";
			this.button1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			//
			// buttonImages
			//
			this.buttonImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("buttonImages.ImageStream")));
			this.buttonImages.TransparentColor = System.Drawing.Color.Transparent;
			this.buttonImages.Images.SetKeyName(0, "Find Dictionary.ico");
			this.buttonImages.Images.SetKeyName(1, "Add a Literal Translation.ico");
			this.buttonImages.Images.SetKeyName(2, "Add Free Translation.ico");
			this.buttonImages.Images.SetKeyName(3, "Add New Text.ico");
			this.buttonImages.Images.SetKeyName(4, "Insert Note.ico");
			this.buttonImages.Images.SetKeyName(5, "Find and Replace Text.ico");
			this.buttonImages.Images.SetKeyName(6, "LinkWords.ico");
			this.buttonImages.Images.SetKeyName(7, "BreakPhrase.ico");
			this.buttonImages.Images.SetKeyName(8, "AddScripture.ico");
			this.buttonImages.Images.SetKeyName(9, "ApprAndMoveNext.ico");
			this.buttonImages.Images.SetKeyName(10, "BrowseAndMoveNext.ico");
			this.buttonImages.Images.SetKeyName(11, "ME.ico");
			this.buttonImages.Images.SetKeyName(12, "ZeroWidth.ico");
			this.buttonImages.Images.SetKeyName(13, "ApproveAll.ico");
			//
			// ImageHolder
			//
			this.Controls.Add(this.button1);
			this.Name = "InterlinearImageHolder";
			this.ResumeLayout(false);

		}
		#endregion
	}
}