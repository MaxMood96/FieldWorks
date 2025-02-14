// Copyright (c) 2015-2022 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System;
using System.Diagnostics;
using System.Xml;

using SIL.LCModel;
using SIL.FieldWorks.Common.ViewsInterfaces;
using SIL.FieldWorks.Common.RootSites;
using System.Collections.Generic;
using SIL.LCModel.Core.Text;
using SIL.LCModel.Core.KernelInterfaces;
using SIL.FieldWorks.IText;

namespace SIL.FieldWorks.Discourse
{
	/// <summary>
	/// DiscourseExporter is an IVwEnv implementation which exports discourse data to an XmlWriter.
	/// Make one of these by creating an XmlTextWriter.
	/// Refactoring is probably in order to share more code with InterlinearExporter,
	/// or move common code down to CollectorEnv. This has been postponed in the interests
	/// of being able to release FW 5.2.1 without requiring changes to DLLs other than Discourse.
	/// </summary>
	public class DiscourseExporter : CollectorEnv, IDisposable
	{
		private readonly XmlWriter m_writer;
		private readonly LcmCache m_cache;
		private readonly IVwViewConstructor m_vc;
		private readonly HashSet<int> m_usedWritingSystems = new HashSet<int>();
		private readonly List<ITsString> m_glossesInCellCollector = new List<ITsString>();
		private readonly Stack<int> m_frags = new Stack<int>();
		private readonly IDsConstChart m_chart;
		private readonly IConstChartRowRepository m_rowRepo;
		private int m_titleRowCount;
		private bool m_fNextCellReversed;

		private readonly int m_wsLineNumber; // ws to use for line numbers. REVIEW (Hasso) 2022.02: use or lose?

		public DiscourseExporter(LcmCache cache, XmlWriter writer, int hvoRoot, IVwViewConstructor vc,
			int wsLineNumber)
			: base(null, cache.MainCacheAccessor, hvoRoot)
		{
			m_cache = cache;
			m_writer = writer;
			m_vc = vc;
			m_wsLineNumber = wsLineNumber;
			m_chart = m_cache.ServiceLocator.GetInstance<IDsConstChartRepository>().GetObject(hvoRoot);
			m_rowRepo = m_cache.ServiceLocator.GetInstance<IConstChartRowRepository>();
		}

		#region Disposable stuff
#if DEBUG
		~DiscourseExporter()
		{
			Dispose(false);
		}
#endif
		public bool IsDisposed { get; private set;}

		/// <summary/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary/>
		protected virtual void Dispose(bool fDisposing)
		{
			Debug.WriteLineIf(!fDisposing, "****** Missing Dispose() call for " + GetType().Name + ". ****** ");
			if (fDisposing && !IsDisposed)
			{
				// dispose managed and unmanaged objects
				m_writer.Dispose();
			}

			IsDisposed = true;
		}
		#endregion

		public void ExportDisplay()
		{
			m_writer.WriteStartDocument();
			m_writer.WriteStartElement("document");
			m_writer.WriteStartElement("chart");
			m_vc.Display(this, OpenObject, ConstChartVc.kfragPrintChart);
			m_writer.WriteEndElement(); // chart
			WriteLanguages();
			m_writer.WriteEndElement(); // document
			m_writer.WriteEndDocument();
		}

		/// <summary>
		/// Write out the languages element. This should be used in InterlinearExporter, too.
		/// </summary>
		void WriteLanguages()
		{
			m_writer.WriteStartElement("languages");
			foreach (var wsActual in m_usedWritingSystems)
			{
				var ws = m_cache.ServiceLocator.WritingSystemManager.Get(wsActual);
				m_writer.WriteStartElement("language");
				// we don't have enough context at this point to get all the possible writing system
				// information we may encounter in the word bundles.
				string wsId = ws.Id;
				m_writer.WriteAttributeString("lang", wsId);
				var fontName = ws.DefaultFontName;
				m_writer.WriteAttributeString("font", fontName);
				if (m_cache.ServiceLocator.WritingSystems.VernacularWritingSystems.Contains(ws))
				{
					m_writer.WriteAttributeString("vernacular", "true");
				}
				if (ws.RightToLeftScript)
					m_writer.WriteAttributeString("RightToLeft", "true");
				m_writer.WriteEndElement();
			}
			m_writer.WriteEndElement();	// languages
		}


		public override void AddStringProp(int tag, IVwViewConstructor _vwvc)
		{
			switch (tag)
			{
				case ConstChartRowTags.kflidNotes:
					WriteStringProp(tag, "note", 0);
					break;
				case ConstChartRowTags.kflidLabel:
					switch (TopFragment)
					{
						case ConstChartVc.kfragChartRow:
							WriteStringProp(tag, "rownum", 0);
							break;
						case ConstChartVc.kfragComment:
							WriteStringProp(tag, "clauseMkr", 0, "target", m_sda.get_StringProp(m_hvoCurr, tag).Text);
							break;
					}
					break;
			}
		}

		private int TopFragment => m_frags.Count == 0 ? 0 : m_frags.Peek();

		public override void AddStringAltMember(int tag, int ws, IVwViewConstructor _vwvc)
		{
			switch (tag)
			{
				case WfiWordformTags.kflidForm:
					if(m_frags.Contains(ConstChartVc.kfragMovedTextCellPart))
						WriteStringProp(tag, "word", ws, "moved", "true");
					else
						WriteStringProp(tag, "word", ws);
					break;
				case WfiGlossTags.kflidForm:
					m_glossesInCellCollector.Add(m_sda.get_MultiStringAlt(m_hvoCurr, tag, ws));
					break;
				case ConstChartTagTags.kflidTag:
					WriteStringProp(tag, "lit", ws); // missing marker.
					break;
				case CmPossibilityTags.kflidName:
				case CmPossibilityTags.kflidAbbreviation:
					WriteStringProp(tag, "listRef", ws);
					break;
				default:
					Debug.Assert(false, "Unknown field in AddStringAltMember!");
					break;
			}
		}

		private void WriteStringProp(int tag, string elementTag, int alt)
		{
			WriteStringProp(tag, elementTag, alt, null, null);
		}

		private void WriteStringProp(int tag, string elementTag, int alt, string extraAttr, string extraAttrVal)
		{
			ITsString tss;
			var ws = alt;
			if (ws == 0)
				tss = m_sda.get_StringProp(m_hvoCurr, tag);
			else
				tss = m_sda.get_MultiStringAlt(m_hvoCurr, tag, alt);
			if (elementTag == "word")
				WriteWordForm(elementTag, tss, ws, extraAttr);
			else
				WriteStringVal(elementTag, ws, tss, extraAttr, extraAttrVal);
		}

		private void WriteWordForm (string elementTag, ITsString tss, int ws, string extraAttr)
		{
			WriteStringVal(elementTag, ws, tss, extraAttr, (extraAttr == null) ? null : "true");
		}

		private void WriteStringVal(string elementTag, int ws, ITsString tss, string extraAttr, string extraAttrVal)
		{
			m_writer.WriteStartElement(elementTag);
			if (extraAttr != null)
				m_writer.WriteAttributeString(extraAttr, extraAttrVal);
			WriteLangAndContent(ws, tss);
			m_writer.WriteEndElement();
		}

		private static int GetWsFromTsString(ITsString tss)
		{
			ITsTextProps ttp = tss.get_PropertiesAt(0);
			return ttp.GetIntPropValues((int)FwTextPropType.ktptWs, out _);
		}

		public override void set_IntProperty(int tpt, int tpv, int nValue)
		{
			if (tpt == (int)FwTextPropType.ktptAlign && nValue == (int)FwTextAlign.ktalTrailing)
				m_fNextCellReversed = true;
			base.set_IntProperty(tpt, tpv, nValue);
		}

		/// <summary>
		/// Write a lang attribute identifying the string, then its content as the body of
		/// an element.
		/// </summary>
		/// <param name="ws1"></param>
		/// <param name="tss"></param>
		private void WriteLangAndContent(int ws1, ITsString tss)
		{
			int ws = ws1;
			if (ws == 0)
				ws = GetWsFromTsString(tss);
			UpdateWsList(ws);
			string icuCode = m_cache.WritingSystemFactory.GetStrFromWs(ws);
			m_writer.WriteAttributeString("lang", icuCode);
			m_writer.WriteString(GetText(tss));
		}

		void UpdateWsList(int ws)
		{
			// only add valid actual ws
			if (ws > 0)
				m_usedWritingSystems.Add(ws);
		}

		static string GetText(ITsString tss)
		{
			string result = tss.Text;
			if (result == null)
				return "";
			return result.Normalize();
		}

		public override void AddObj(int hvoItem, IVwViewConstructor vc, int frag)
		{
			m_frags.Push(frag);
			base.AddObj (hvoItem, vc, frag);
			m_frags.Pop();
		}

		public override void AddString(ITsString tss)
		{
			if (TopFragment == ConstChartVc.kfragMTMarker)
			{
				WriteMTMarker(tss);
				return;
			}
			// Ignore directionality markers on export. Also skip empty strings and single spaces...
			// we handle extra space with the stylesheet.
			string text = tss.Text;
			if (text == "\x200F" || text == "\x200E" || string.IsNullOrEmpty(text) || text == " ")
				return;
			if ((m_vc as InterlinVc)!= null && (m_vc as InterlinVc).IsDoingRealWordForm)
			{
				var ws = GetWsFromTsString(tss);
				WriteWordForm("word", tss, ws, m_frags.Contains(ConstChartVc.kfragMovedTextCellPart) ? "moved" : null);
			}
			else if (text == "***")
				m_glossesInCellCollector.Add(tss);
			else
			{
				m_writer.WriteStartElement("lit");
				MarkNeedsSpace(tss.Text);

				WriteLangAndContent(GetWsFromTsString(tss), tss);
				m_writer.WriteEndElement();
				base.AddString(tss);
			}
		}

		private void WriteMTMarker(ITsString tss)
		{
			var ws = GetWsFromTsString(tss);
			var newTss = TsStringUtils.MakeString(tss.Equals(((ConstChartVc)m_vc).m_sMovedTextBefore) ? "Preposed" : "Postposed", ws);
			var hvoTarget = m_sda.get_ObjectProp(m_hvoCurr,
					ConstChartMovedTextMarkerTags.kflidWordGroup); // the CCWordGroup we refer to
			if (ConstituentChartLogic.HasPreviousMovedItemOnLine(m_chart, hvoTarget))
				WriteStringVal("moveMkr", ws, newTss, "targetFirstOnLine", "false");
			else
				WriteStringVal("moveMkr", ws, newTss, null, null);
		}

		/// <summary>
		/// Add attributes to an element just started to indicate whether white space is
		/// needed before or after the specified literal.
		/// The current implementation is simplistic, based on the few separators actually
		/// used in the discourse chart.
		/// The default is nothing added, indicating that white space IS needed.
		/// </summary>
		private void MarkNeedsSpace(string lit)
		{
			if (lit.StartsWith("]") || lit.StartsWith(")"))
				m_writer.WriteAttributeString("noSpaceBefore", "true");
			if (lit.EndsWith("[") || lit.EndsWith("(") || lit.EndsWith("-"))
				m_writer.WriteAttributeString("noSpaceAfter", "true");
		}

		public override void AddObjProp(int tag, IVwViewConstructor vc, int frag)
		{
			m_frags.Push(frag);
			base.AddObjProp(tag, vc, frag);
			m_frags.Pop();
		}

		/// <summary>
		/// Here we build the main structure of the chart as a collection of cells. We have to be a bit tricky about
		/// generating the header.
		/// </summary>
		public override void OpenTableCell(int nRowSpan, int nColSpan)
		{
			m_writer.WriteStartElement("cell");
			if (m_fNextCellReversed)
			{
				m_fNextCellReversed = false;
				m_writer.WriteAttributeString("reversed", "true");
			}
			m_writer.WriteAttributeString("cols", nColSpan.ToString());
			m_writer.WriteStartElement("main");
			base.OpenTableCell(nRowSpan, nColSpan);
		}

		public override void CloseTableCell()
		{
			base.CloseTableCell();
			m_writer.WriteEndElement(); // the "main" element
			if (m_glossesInCellCollector.Count > 0)
			{
				m_writer.WriteStartElement("glosses");
				foreach (var gloss in m_glossesInCellCollector)
				{
					m_writer.WriteStartElement("gloss");
					var icuCode = m_cache.WritingSystemFactory.GetStrFromWs(gloss.get_WritingSystem(0));
					m_writer.WriteAttributeString("lang", icuCode);
					m_writer.WriteString(gloss.Text ?? string.Empty);
					m_writer.WriteEndElement(); // gloss
				}
				m_writer.WriteEndElement(); // glosses
				// Ready to start collecting for the next cell.
				m_glossesInCellCollector.Clear();
			}

			m_writer.WriteEndElement(); // cell
		}

		public override void OpenTableRow()
		{
			m_writer.WriteStartElement("row");

			switch (TopFragment)
			{
				case ConstChartVc.kfragChartRow:
					var row = m_rowRepo.GetObject(m_hvoCurr);
					if (row.EndParagraph)
						m_writer.WriteAttributeString("endPara", "true");
					else if (row.EndSentence)
						m_writer.WriteAttributeString("endSent", "true");
					var clauseType = ConstChartVc.GetRowStyleName(row);
					m_writer.WriteAttributeString("type", clauseType);
					var label = row.Label.Text;
					if (!string.IsNullOrEmpty(label))
						m_writer.WriteAttributeString("id", label);
					break;
				default:
					// Not a chart row; this must be a title row
					m_writer.WriteAttributeString("type", $"title{++m_titleRowCount}");
					break;
			}

			base.OpenTableRow();
		}

		public override void CloseTableRow()
		{
			base.CloseTableRow();
			m_writer.WriteEndElement(); // row
		}

		/// <summary>
		/// overridden to maintain the frags array.
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="vc"></param>
		/// <param name="frag"></param>
		public override void AddObjVecItems(int tag, IVwViewConstructor vc, int frag)
		{
			m_frags.Push(frag);
			base.AddObjVecItems (tag, vc, frag);
			m_frags.Pop();
		}
	}
}
