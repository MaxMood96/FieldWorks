// Copyright (c) 2017-2018 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System.Windows.Forms;

namespace LanguageExplorer
{
	/// <summary>
	/// Class that helps tools get a main menu or a sub-menu of a main menu.
	/// </summary>
	internal static class MenuServices
	{
		private static ToolStripMenuItem GetMenu(ToolStripItemCollection dropDownItems, string menuName)
		{
			return (ToolStripMenuItem)dropDownItems[menuName];
		}

		private static ToolStripMenuItem GetMenu(ToolStrip menustrip, string menuName)
		{
			return (ToolStripMenuItem)menustrip.Items[menuName];
		}

		#region File menu

		internal static ToolStripMenuItem GetFileMenu(MenuStrip menustrip)
		{
			return GetMenu(menustrip, LanguageExplorerConstants.FileToolStripMenuItem);
		}

		internal static ToolStripMenuItem GetFilePrintMenu(MenuStrip menustrip)
		{
			return GetMenu(GetMenu(menustrip, LanguageExplorerConstants.FileToolStripMenuItem).DropDownItems, "printToolStripMenuItem");
		}

		internal static ToolStripMenuItem GetFileExportMenu(MenuStrip menustrip)
		{
			return GetMenu(GetMenu(menustrip, LanguageExplorerConstants.FileToolStripMenuItem).DropDownItems, "exportToolStripMenuItem");
		}

		internal static ToolStripMenuItem GetFileImportMenu(MenuStrip menustrip)
		{
			return GetMenu(GetMenu(menustrip, LanguageExplorerConstants.FileToolStripMenuItem).DropDownItems, "importToolStripMenuItem");
		}

		#endregion File menu

		#region Edit menu

		internal static ToolStripMenuItem GetEditMenu(MenuStrip menustrip)
		{
			return GetMenu(menustrip, LanguageExplorerConstants.EditToolStripMenuItem);
		}

		internal static ToolStripMenuItem GetEditDeleteMenu(MenuStrip menustrip)
		{
			return GetMenu(GetMenu(menustrip, LanguageExplorerConstants.EditToolStripMenuItem).DropDownItems, "deleteToolStripMenuItem");
		}

		internal static ToolStripMenuItem GetEditFindMenu(MenuStrip menustrip)
		{
			return GetMenu(GetMenu(menustrip, LanguageExplorerConstants.EditToolStripMenuItem).DropDownItems, "findToolStripMenuItem");
		}

		internal static ToolStripMenuItem GetEditFindAndReplaceMenu(MenuStrip menustrip)
		{
			return GetMenu(GetMenu(menustrip, LanguageExplorerConstants.EditToolStripMenuItem).DropDownItems, "replaceToolStripMenuItem");
		}

		#endregion Edit menu

		#region View menu

		internal static ToolStripMenuItem GetViewMenu(MenuStrip menustrip)
		{
			return GetMenu(menustrip, LanguageExplorerConstants.ViewToolStripMenuItem);
		}

		internal static ToolStripMenuItem GetViewRefreshMenu(MenuStrip menustrip)
		{
			return GetMenu(GetMenu(menustrip, LanguageExplorerConstants.ViewToolStripMenuItem).DropDownItems, "refreshToolStripMenuItem");
		}

		internal static ToolStripMenuItem GetViewFilterMenu(MenuStrip menustrip)
		{
			return GetMenu(GetMenu(menustrip, LanguageExplorerConstants.ViewToolStripMenuItem).DropDownItems, "filtersToolStripMenuItem");
		}

		#endregion View menu

		#region Data menu

		internal static ToolStripMenuItem GetDataMenu(MenuStrip menustrip)
		{
			return GetMenu(menustrip, LanguageExplorerConstants.DataToolStripMenuItem);
		}

		#endregion Data menu

		#region Insert menu

		internal static ToolStripMenuItem GetInsertMenu(MenuStrip menustrip)
		{
			return GetMenu(menustrip, LanguageExplorerConstants.InsertToolStripMenuItem);
		}

		#endregion Insert menu

		#region Format menu

		internal static ToolStripMenuItem GetFormatMenu(MenuStrip menustrip)
		{
			return GetMenu(menustrip, LanguageExplorerConstants.FormatToolStripMenuItem);
		}

		#endregion Format menu

		#region Tools menu

		internal static ToolStripMenuItem GetToolsMenu(MenuStrip menustrip)
		{
			return GetMenu(menustrip, LanguageExplorerConstants.ToolsToolStripMenuItem);
		}

		internal static ToolStripMenuItem GetToolsConfigureMenu(MenuStrip menustrip)
		{
			return GetMenu(GetMenu(menustrip, LanguageExplorerConstants.ToolsToolStripMenuItem).DropDownItems, "configureToolStripMenuItem");
		}

		#endregion Tools menu

		#region Help menu

		internal static ToolStripMenuItem GetHelpMenu(MenuStrip menustrip)
		{
			return GetMenu(menustrip, LanguageExplorerConstants.HelpToolStripMenuItem);
		}

		#endregion Help menu
	}
}