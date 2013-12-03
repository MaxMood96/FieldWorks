// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2013, SIL International. All Rights Reserved.
// <copyright from='2013' to='2013' company='SIL International'>
//		Copyright (c) 2013, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
// ---------------------------------------------------------------------------------------------
using System;
using System.Windows.Forms;
using Microsoft.Win32;
using SIL.FieldWorks.Common.FwUtils;
using SIL.FieldWorks.FDO.DomainServices.BackupRestore;
using SIL.FieldWorks.FdoUi.Dialogs;
using SIL.Utils;
using XCore;

namespace SIL.FieldWorks.FdoUi
{
	/// <summary>
	/// Implementation of IFdoUserAction which uses Windows Forms
	/// </summary>
	public class FdoUserActionWindowsForms : IFdoUserAction
	{
		private readonly IHelpTopicProvider m_helpTopicProvider;

		public FdoUserActionWindowsForms(IHelpTopicProvider helpTopicProvider)
		{
			m_helpTopicProvider = helpTopicProvider;
		}

		/// <summary>
		/// Check with user regarding conflicting changes
		/// </summary>
		/// <returns>True if user wishes to revert to saved state. False otherwise.</returns>
		public bool ConflictingSave()
		{
			using (var dlg = new ConflictingSaveDlg())
			{
				DialogResult result = dlg.ShowDialog(Form.ActiveForm);
				return result != DialogResult.OK;
			}
		}

		/// <summary>
		/// Inform the user of a lost connection
		/// </summary>
		/// <returns>True if user wishes to attempt reconnect.  False otherwise.</returns>
		public bool ConnectionLost()
		{
			using (var dlg = new ConnectionLostDlg())
			{
				return dlg.ShowDialog() == DialogResult.Yes;
			}
		}

		/// <summary>
		/// Check with user regarding which files to use
		/// </summary>
		/// <returns></returns>
		public FileSelection ChooseFilesToUse()
		{
			using (var dlg = new FilesToRestoreAreOlder(m_helpTopicProvider))
			{
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					if (dlg.fKeepFilesThatAreNewer)
					{
						return FileSelection.OkKeepNewer;
					}
					if (dlg.fOverWriteThatAreNewer)
					{
						return FileSelection.OkUseOlder;
					}
				}
				return FileSelection.Cancel;
			}
		}

		/// <summary>
		/// Check with user regarding restoring linked files in the project folder or original path
		/// </summary>
		/// <returns>True if user wishes to restore linked files in project folder. False to leave them in the original location.</returns>
		public bool RestoreLinkedFilesInProjectFolder()
		{
			using (var dlg = new RestoreLinkedFilesToProjectsFolder(m_helpTopicProvider))
			{
				return dlg.ShowDialog() == DialogResult.OK && dlg.fRestoreLinkedFilesToProjectFolder;
			}
		}

		/// <summary>
		/// Cannot restore linked files to original path.
		/// Check with user regarding restoring linked files in the project folder or not at all
		/// </summary>
		/// <returns>OkYes to restore to project folder, OkNo to skip restoring linked files, Cancel otherwise</returns>
		public YesNoCancel CannotRestoreLinkedFilesToOriginalLocation()
		{
			using (var dlgCantWriteFiles = new CantRestoreLinkedFilesToOriginalLocation(m_helpTopicProvider))
			{
				if (dlgCantWriteFiles.ShowDialog() == DialogResult.OK)
				{
					if (dlgCantWriteFiles.fRestoreLinkedFilesToProjectFolder)
					{
						return YesNoCancel.OkYes;
					}
					if (dlgCantWriteFiles.fDoNotRestoreLinkedFiles)
					{
						return YesNoCancel.OkNo;
					}
				}
				return YesNoCancel.Cancel;
			}
		}

		/// <summary>
		/// Displays information to the user
		/// </summary>
		public void MessageBox()
		{
			//System.Windows.Forms.MessageBox.Show();
		}

		/// <summary>
		/// show a dialog or output to the error log, as appropriate.
		/// </summary>
		/// <param name="error">the exception you want to report</param>
		/// <param name="isLethal">set to <c>true</c> if the error is lethal, otherwise
		/// <c>false</c>.</param>
		/// <returns>True if the error was lethal and the user chose to exit the application,
		/// false otherwise.</returns>
		public bool ReportException(Exception error, bool isLethal)
		{
			return ErrorReporter.ReportException(error, null, null, null, isLethal);
		}

		/// <summary>
		/// Reports duplicate guids to the user
		/// </summary>
		/// <param name="applicationKey">The application key.</param>
		/// <param name="emailAddress">The email address.</param>
		/// <param name="errorText">The error text.</param>
		public void ReportDuplicateGuids(RegistryKey applicationKey, string emailAddress, string errorText)
		{
			ErrorReporter.ReportDuplicateGuids(applicationKey, emailAddress, null, errorText);
		}
	}
}
