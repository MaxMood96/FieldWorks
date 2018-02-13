// Copyright (c) 2003-2018 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Linq;
using LanguageExplorer.Controls.DetailControls.Resources;
using LanguageExplorer.Controls.XMLViews;
using SIL.LCModel.Core.KernelInterfaces;
using SIL.FieldWorks.Common.FwUtils;
using SIL.LCModel;
using SIL.Xml;

namespace LanguageExplorer.Controls.DetailControls
{
	/// <summary>
	/// SimpleListChooser is (now) a trivial override of ReallySimpleListChooser, adding the one bit
	/// of functionality that couldn't be put in the XmlViews project because it depends on DLLs
	/// which XmlViews can't reference.
	/// </summary>
	public class SimpleListChooser : ReallySimpleListChooser
	{
		/// <summary>
		/// Constructor for use with designer
		/// </summary>
		public SimpleListChooser()
			: base()
		{
		}

		/// <summary>
		/// deprecated constructor for use with changing or setting a value
		/// </summary>
		public SimpleListChooser(LcmCache cache, IPersistenceProvider persistProvider, IHelpTopicProvider helpTopicProvider, IEnumerable<ObjectLabel> labels, ICmObject currentObj, string fieldName, string nullLabel)
			: base(cache, helpTopicProvider, persistProvider, labels, currentObj, fieldName, nullLabel)
		{
		}

		/// <summary>
		/// constructor for use with changing or setting a value
		/// </summary>
		public SimpleListChooser(LcmCache cache, IPersistenceProvider persistProvider, IHelpTopicProvider helpTopicProvider, IEnumerable<ObjectLabel> labels, ICmObject currentObj, string fieldName, string nullLabel, IVwStylesheet stylesheet)
			: base(cache, helpTopicProvider, persistProvider, labels, currentObj, fieldName, nullLabel, stylesheet)
		{
		}

		/// <summary>
		/// constructor for use with changing or setting a value
		/// </summary>
		public SimpleListChooser(LcmCache cache, IPersistenceProvider persistProvider, IHelpTopicProvider helpTopicProvider, IEnumerable<ObjectLabel> labels, ICmObject currentObj, string fieldName)
			: base(cache, helpTopicProvider, persistProvider, labels, currentObj, fieldName)
		{
		}

		/// <summary>
		/// constructor for use with adding a new value
		/// </summary>
		public SimpleListChooser(IPersistenceProvider persistProvider, IEnumerable<ObjectLabel> labels, string fieldName, IHelpTopicProvider helpTopicProvider)
			: base(persistProvider, labels, fieldName, helpTopicProvider)
		{
		}

		/// <summary>
		/// constructor for use with adding a new value
		/// </summary>
		public SimpleListChooser(IPersistenceProvider persistProvider, IEnumerable<ObjectLabel> labels, string fieldName, IVwStylesheet stylesheet, IHelpTopicProvider helpTopicProvider)
			: base(persistProvider, labels, fieldName, stylesheet, helpTopicProvider)
		{
		}

		/// <summary>
		/// constructor for use with changing or setting multiple values.
		/// </summary>
		public SimpleListChooser(IPersistenceProvider persistProvider, IEnumerable<ObjectLabel> labels, string fieldName, LcmCache cache, IEnumerable<ICmObject> chosenObjs, IHelpTopicProvider helpTopicProvider)
			: base(persistProvider, labels, fieldName, cache, chosenObjs, helpTopicProvider)
		{
		}

		protected override void AddSimpleLink(string sLabel, string sTool, XElement node)
		{
			switch (sTool)
			{
				case "MakeInflAffixSlotChooserCommand":
					{
						var sTarget = XmlUtils.GetOptionalAttributeValue(node, "target");
						var hvoPos = 0;
						var sTopPOS = DetailControlsStrings.ksQuestionable;
						if (sTarget == null || sTarget.ToLower() == "owner")
						{
							hvoPos = TextParamHvo;
							sTopPOS = TextParam;
						}
						else if (sTarget.ToLower() == "toppos")
						{
							hvoPos = GetHvoOfHighestPOS(TextParamHvo, out sTopPOS);
						}
						sLabel = string.Format(sLabel, sTopPOS);
						var fOptional = XmlUtils.GetOptionalBooleanAttributeValue(node, "optional", false);
						var sTitle = StringTable.Table.GetString(fOptional ? "OptionalSlot" : "ObligatorySlot", "Linguistics/Morphology/TemplateTable");
						AddLink(sLabel, LinkType.kSimpleLink, new MakeInflAffixSlotChooserCommand(Cache, true, sTitle, hvoPos, fOptional, m_propertyTable, m_publisher, m_subscriber));
					}
					break;
				default:
					base.AddSimpleLink(sLabel, sTool, node);
					break;
			}
		}

		/// <summary>
		/// Get tree view of the chooser
		/// </summary>
		public TreeView TreeView => m_labelsTreeView;
	}
}