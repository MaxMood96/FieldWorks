// Copyright (c) 2005-2018 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System.Diagnostics;
using System.Drawing;
using LanguageExplorer.Controls.DetailControls;
using SIL.FieldWorks.Common.Widgets;
using SIL.LCModel.Core.KernelInterfaces;

namespace LanguageExplorer.Areas.Grammar.Tools.AdhocCoprohibEdit
{
	/// <summary>
	/// Summary description for AdhocCoProhibAtomicReferenceSlice.
	/// </summary>
	internal class AdhocCoProhibAtomicReferenceSlice : CustomAtomicReferenceSlice
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AdhocCoProhibAtomicReferenceSlice"/> class.
		/// </summary>
		public AdhocCoProhibAtomicReferenceSlice()
			: base(new AdhocCoProhibAtomicLauncher())
		{
		}

		/// <summary>
		/// Override method to add suitable control.
		/// </summary>
		public override void FinishInit()
		{
			Debug.Assert(Cache != null);

			base.FinishInit();

			//We need to set the Font so the height of this slice will be
			//set appropriately to fit the text.
			IVwStylesheet stylesheet = FontHeightAdjuster.StyleSheetFromPropertyTable(PropertyTable);
			var fontHeight = FontHeightAdjuster.GetFontHeightForStyle("Normal", stylesheet, Cache.DefaultVernWs, Cache.LanguageWritingSystemFactoryAccessor);
			Font = new Font(Cache.ServiceLocator.WritingSystems.DefaultVernacularWritingSystem.DefaultFontName, fontHeight / 1000f);
		}
	}
}