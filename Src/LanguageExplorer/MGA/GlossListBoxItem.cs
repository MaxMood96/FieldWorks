// Copyright (c) 2003-2018 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System.Xml;
using SIL.Code;
using SIL.LCModel;
using SIL.Xml;

namespace LanguageExplorer.MGA
{
	// NB: I'd prefer to subclass XmlNode and override its ToString() class.
	//     When I tried that, however, it appears that XmlNode is protected and one
	//     cannot inherit from it.
	public class GlossListBoxItem
	{
		private readonly LcmCache m_cache;

		#region Construction
		public GlossListBoxItem(LcmCache cache, XmlNode node, string sAfterSeparator, string sComplexNameSeparator, bool fComplexNameFirst)
		{
			Guard.AgainstNull(cache, nameof(cache));

			m_cache = cache;
			XmlNode = node;
			SetValues(node, sAfterSeparator, sComplexNameSeparator, fComplexNameFirst);
			MoGlossItem = m_cache.ServiceLocator.GetInstance<IMoGlossItemFactory>().Create();
		}

		private void SetValues(XmlNode node, string sAfterSeparator, string sComplexNameSeparator, bool fComplexNameFirst)
		{
			var xn = node.SelectSingleNode("term");
			Term = xn?.InnerText ?? MGAStrings.ksUnknownTerm;
			xn = node.SelectSingleNode("abbrev");
			Abbrev = xn?.InnerText ?? MGAStrings.ksUnknownTerm;
			var attr = XmlNode.Attributes.GetNamedItem("afterSeparator");
			AfterSeparator = attr == null ? sAfterSeparator : attr.Value;
			attr = XmlNode.Attributes.GetNamedItem("complexNameSeparator");
			ComplexNameSeparator = attr == null ? sComplexNameSeparator : attr.Value;
			attr = XmlNode.Attributes.GetNamedItem("complexNameFirst");
			ComplexNameFirst = attr == null ? fComplexNameFirst : XmlUtils.GetBooleanAttributeValue(attr.Value);
			SetType();
		}

		private void SetType()
		{
			var attr = XmlNode.Attributes.GetNamedItem("type");
			if (attr != null)
			{
				switch (attr.Value)
				{
					case "complex":
						IsComplex = true;
						IsValue = false;
						break;
					case "value":
						IsComplex = false;
						IsValue = true;
						break;
					default:
						IsComplex = false;
						IsValue = false;
						break;
				}
			}
			else
			{
				var itemDaughter = XmlNode.SelectSingleNode("item");
				if (itemDaughter == null)
				{
					IsComplex = false;
					IsValue = true;
				}
				else
				{
					IsComplex = false;
					IsValue = false;
				}
			}
		}
		#endregion
		#region properties
		/// <summary>
		/// Gets the abbreviation of the item.
		/// </summary>
		public string Abbrev { get; private set; }

		/// <summary>
		/// Gets default after separator character for glossing.
		/// </summary>
		public string AfterSeparator { get; private set; }

		/// <summary>
		/// Gets flag whether the name of the complex item comes first or not.
		/// </summary>
		public bool ComplexNameFirst { get; private set; }

		/// <summary>
		/// Gets default separator character to occur after a complex name in glossing.
		/// </summary>
		public string ComplexNameSeparator { get; private set; }

		/// <summary>
		/// Gets flag whether the item is complex or not.
		/// </summary>
		public bool IsComplex { get; private set; }

		/// <summary>
		/// Gets flag whether the item is a feature value or not.
		/// </summary>
		public bool IsValue { get; private set; }

		/// <summary>
		/// Gets the MoGlossItem of the item.
		/// </summary>
		public IMoGlossItem MoGlossItem { get; }

		/// <summary>
		/// Gets the term definition of the item.
		/// </summary>
		public string Term { get; private set; }

		/// <summary>
		/// Gets/sets the XmlNode of the item.
		/// </summary>
		public XmlNode XmlNode { get; set; }

		#endregion
		public override string ToString()
		{
			return string.Format(MGAStrings.ksX_Y, Term, Abbrev);
		}
#if UsingGlossSystem
		/// <summary>
		/// Add the item to the language database
		/// </summary>
		/// <param name="cache">LCM cache to use</param>
		public void AddToDataBase(LcmCache cache)
		{
			ILangProject lp=cache.LangProject;
			IMoMorphData md = lp.MorphologicalDataOA;
			IMoGlossSystem gs = md.GlossSystemOA;

			XmlNode parent = m_xmlNode.ParentNode;
			if (parent.Name != "item")
			{ // is a top level item; find it or add it
				IMoGlossItem giFound = gs.FindEmbeddedItem(Term, Abbrev, false);
				if (giFound == null)
				{ // not found, so add it
					gs.GlossesOC.Add(m_glossItem);
				}
			}
			else
			{ // not at top level; get parent and add it to parent;
				// also create any missing items between this node and the top
				IMoGlossItem giParent = GetMyParentGlossItem(cache, parent);
				giParent.GlossItemsOS.Append(m_glossItem);
			}
			FillInGlossItemBasedOnXmlNode(m_glossItem, m_xmlNode, this);
			CreateFeatStructFrag();
		}
		/// <summary>
		/// Get parent MoGlossItem and fill in any missing items between the parent and the top level.
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="node"></param>
		/// <returns>The MoGlossItem object which is or is to be the parent of this item.</returns>
		private IMoGlossItem GetMyParentGlossItem(LcmCache cache, XmlNode node)
		{
			ILangProject lp=cache.LangProject;
			IMoMorphData md = lp.MorphologicalDataOA;
			IMoGlossSystem gs = md.GlossSystemOA;

			System.Xml.XmlNode parent = node.ParentNode;
#if NUnitDebug
			Console.WriteLine("gmpgi: working on " + XmlUtils.GetAttributeValue(node, "id"));
#endif
			if (parent.Name != "item")
			{ // is a top level item; find it or add it
#if NUnitDebug
				Console.WriteLine("gmpgi: found top");
#endif
				MIoGlossItem giFound = gs.FindEmbeddedItem(XmlUtils.GetAttributeValue(node, "term"),
					XmlUtils.GetAttributeValue(node, "abbrev"), false);
				if (giFound == null)
				{ // not found; so add it
					IMoGlossItem gi = new MoGlossItem();
					gs.GlossesOC.Add(gi);
					FillInGlossItemBasedOnXmlNode(gi, node, this);
#if NUnitDebug
					Console.WriteLine("gmpgi, found top, made new, returning ", gi.Name.AnalysisDefaultWritingSystem);
#endif
					return gi;
				}
				else
				{ //found, so return it
#if NUnitDebug
					Console.WriteLine("gmpgi, found top, exists, returning ", giFound.Name.AnalysisDefaultWritingSystem);
#endif
					return giFound;
				}
			}
			else
			{  // not a top level item; get its parent and add it, if need be
#if NUnitDebug
				Console.WriteLine("gmpgi: calling parent of " + XmlUtils.GetAttributeValue(node, "id"));
#endif
				IMoGlossItem giParent = GetMyParentGlossItem(cache, parent);
				IMoGlossItem giFound = giParent.FindEmbeddedItem(XmlUtils.GetAttributeValue(node, "term"),
					XmlUtils.GetAttributeValue(node, "abbrev"), false);
				if (giFound == null)
				{ // not there, add it
#if NUnitDebug
					Console.WriteLine("gmpgi: adding a node");
#endif
					giFound = new MoGlossItem();
					giParent.GlossItemsOS.Append(giFound);
					FillInGlossItemBasedOnXmlNode(giFound, node, this);
				}
#if NUnitDebug
				Console.WriteLine("gmpgi, in middle, returning " + giFound.Name.AnalysisDefaultWritingSystem + " for node " + XmlUtils.GetAttributeValue(node, "id"));
#endif
				return giFound;
			}
		}
		/// <summary>
		/// Fill in the attributes of a MoGlossItem object based on its corresponding XML node in the etic gloss list tree
		/// </summary>
		/// <param name="gi"></param>
		/// <param name="xn"></param>
		/// <param name="glbi"></param>
		private void FillInGlossItemBasedOnXmlNode(MoGlossItem gi, XmlNode xn, GlossListBoxItem glbi)
		{
			XmlNode attr = xn.Attributes.GetNamedItem("term");
			if (attr == null)
				gi.Name.AnalysisDefaultWritingSystem = MGAStrings.ksUnknownTerm;
			else
				gi.Name.AnalysisDefaultWritingSystem = attr.Value;
			attr = xn.Attributes.GetNamedItem("abbrev");
			if (attr == null)
				gi.Abbreviation.AnalysisDefaultWritingSystem = MGAStrings.ksUnknownAbbreviation;
			else
				gi.Abbreviation.AnalysisDefaultWritingSystem = attr.Value;
			attr = xn.Attributes.GetNamedItem("afterSeparator");
			if (attr == null)
				gi.AfterSeparator = this.AfterSeparator;
			else
				gi.AfterSeparator = attr.Value;
			attr = xn.Attributes.GetNamedItem("complexNameSeparator");
			if (attr == null)
				gi.ComplexNameSeparator = this.ComplexNameSeparator;
			else
				gi.ComplexNameSeparator= attr.Value;
			attr = xn.Attributes.GetNamedItem("complexNameFirst");
			if (attr == null)
				gi.ComplexNameFirst = this.ComplexNameFirst;
			else
				gi.ComplexNameFirst = XmlUtils.GetBooleanAttributeValue(attr.Value);
			attr = xn.Attributes.GetNamedItem("type");
			attr = xn.Attributes.GetNamedItem("status");
			if (attr == null)
				gi.Status = true;
			else
			{
				if (attr.Value == "visible")
					gi.Status = true;
				else
					gi.Status = false;
			}
			attr = xn.Attributes.GetNamedItem("type");
			if (attr == null)
				gi.Type = (int)SIL.FieldWorks.FDO.Ling.MoGlossItem.ItemType.unknown;
			else
			{
				switch(attr.Value)
				{
					case "complex":
						gi.Type = (int)SIL.FieldWorks.FDO.Ling.MoGlossItem.ItemType.complex;
						break;
					case "deriv":
						gi.Type = (int)SIL.FieldWorks.FDO.Ling.MoGlossItem.ItemType.deriv;
						break;
					case "feature":
						gi.Type = (int)SIL.FieldWorks.FDO.Ling.MoGlossItem.ItemType.feature;
						break;
					case "fsType":
						gi.Type = (int)SIL.FieldWorks.FDO.Ling.MoGlossItem.ItemType.fsType;
						break;
					case "group":
						gi.Type = (int)SIL.FieldWorks.FDO.Ling.MoGlossItem.ItemType.group;
						break;
					case "value":
						gi.Type = (int)SIL.FieldWorks.FDO.Ling.MoGlossItem.ItemType.inflValue;
						break;
					case "xref":
						gi.Type = (int)SIL.FieldWorks.FDO.Ling.MoGlossItem.ItemType.xref;
						break;
					default:
						gi.Type = (int)SIL.FieldWorks.FDO.Ling.MoGlossItem.ItemType.unknown;
						break;
				}
			}
		}
		private void CreateFeatStructFrag()
		{

		}
#endif
	}
}
