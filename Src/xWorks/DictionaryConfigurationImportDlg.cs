﻿// Copyright (c) 2017 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)


using System;
using System.Drawing;
using System.Windows.Forms;
using SIL.FieldWorks.Common.FwUtils;
using SIL.PlatformUtilities;
using XCore;

namespace SIL.FieldWorks.XWorks
{
	public partial class DictionaryConfigurationImportDlg : Form
	{
		private readonly IHelpTopicProvider m_helpTopicProvider;

		internal string HelpTopic { get; set; }

		public DictionaryConfigurationImportDlg(IHelpTopicProvider helpProvider)
		{
			InitializeComponent();
			m_helpTopicProvider = helpProvider;
			// Clear away example text
			explanationLabel.Text = string.Empty;

			if (Platform.IsUnix)
			{
				var optimalWidthOnMono = 582;
				MinimumSize = new Size(optimalWidthOnMono, MinimumSize.Height);
			}
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void helpButton_Click(object sender, EventArgs e)
		{
			ShowHelp.ShowHelpTopic(m_helpTopicProvider, HelpTopic);
		}
	}
}
