// Copyright (c) 2015-2022 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using NUnit.Framework;
using SIL.LCModel.Core.WritingSystems;
using SIL.FieldWorks.Common.Controls;
using SIL.LCModel;
using SIL.WritingSystems;
using XCore;

namespace XMLViewsTests
{
	public class ConfiguredExportTests : MemoryOnlyBackendProviderRestoredForEachTestTestBase
	{
		private Mediator m_mediator;
		private PropertyTable m_propertyTable;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Override to start an undoable UOW.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public override void TestSetup()
		{
			base.TestSetup();

			m_mediator = new Mediator();
			m_propertyTable = new PropertyTable(m_mediator);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Override to end the undoable UOW, Undo everything, and 'commit',
		/// which will essentially clear out the Redo stack.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public override void TestTearDown()
		{
			m_mediator.Dispose();
			m_mediator = null;
			m_propertyTable.Dispose();
			m_propertyTable = null;

			base.TestTearDown();
		}

		[Test]
		public void BeginCssClassIfNeeded_UsesSafeClasses()
		{
			TestBeginCssClassForFlowType("para");
			TestBeginCssClassForFlowType("span");
		}

		[Test]
		public void XHTMLExportGetDigraphMapsFirstCharactersFromICUSortRules()
		{
			CoreWritingSystemDefinition ws = Cache.LangProject.DefaultVernacularWritingSystem;
			ws.DefaultCollation = new IcuRulesCollationDefinition("standard") {IcuRules = "&b < az << a < c <<< ch"};

			var exporter = new ConfiguredExport(null, null, 0);
			string output;
			using (var stream = new MemoryStream())
			{
				using (var writer = new StreamWriter(stream))
				{
					exporter.Initialize(Cache, m_propertyTable, writer, null, "xhtml", null, "dicBody");
					Dictionary<string, string> mapChars;
					ISet<string> ignoreSet;
					var data = exporter.GetDigraphs(ws, out mapChars, out ignoreSet);
					Assert.AreEqual(mapChars.Count, 2, "Too many characters found equivalents");
					Assert.AreEqual(mapChars["a"], "az");
					Assert.AreEqual(mapChars["ch"], "c");
				}
			}
		}

		[Test]
		public void XHTMLExportGetDigraphMapsFromICUSortRules_TestSecondaryTertiaryShouldNotGenerateHeader()
		{
			var ws = Cache.LangProject.DefaultVernacularWritingSystem;
			ws.DefaultCollation = new IcuRulesCollationDefinition("standard") { IcuRules = "&b << az / c <<< AZ / C" + Environment.NewLine + "&f << gz"};

			var exporter = new ConfiguredExport(null, null, 0);
			string output;
			using (var stream = new MemoryStream())
			{
				using (var writer = new StreamWriter(stream))
				{
					exporter.Initialize(Cache, m_propertyTable, writer, null, "xhtml", null, "dicBody");
					Dictionary<string, string> mapChars;
					ISet<string> ignoreSet;
					var data = exporter.GetDigraphs(ws, out mapChars, out ignoreSet);
					Assert.AreEqual(data.Count, 0, "Header created for two wedges");
					Assert.AreEqual(mapChars.Count, 3, "Too many characters found equivalents");
					Assert.AreEqual(mapChars["az"], "b");
					Assert.AreEqual(mapChars["AZ"], "b");
					// Rules following the '/' rule should not be skipped LT-18309
					Assert.AreEqual(mapChars["gz"], "f");
				}
			}
		}

		[Test]
		public void XHTMLExportGetDigraphMapsFromICUSortRules_TertiaryIgnorableDoesNotCrash()
		{
			CoreWritingSystemDefinition ws = Cache.LangProject.DefaultVernacularWritingSystem;
			ws.DefaultCollation = new IcuRulesCollationDefinition("standard") {IcuRules = "&[last tertiary ignorable] = \\"};

			var exporter = new ConfiguredExport(null, null, 0);
			string output;
			using(var stream = new MemoryStream())
			{
				using(var writer = new StreamWriter(stream))
				{
					exporter.Initialize(Cache, m_propertyTable, writer, null, "xhtml", null, "dicBody");
					Dictionary<string, string> mapChars = null;
					ISet<string> ignoreSet = null;
					ISet<string> data = null;
					Assert.DoesNotThrow(() => data = exporter.GetDigraphs(ws, out mapChars, out ignoreSet));
					// The second test catches the real world scenario, GetDigraphs is actually called many times, but the first time
					// is the only one that should trigger the algorithm, afterward the information is cached in the exporter.
					Assert.DoesNotThrow(() => data = exporter.GetDigraphs(ws, out mapChars, out ignoreSet));
					Assert.AreEqual(mapChars.Count, 0, "Too many characters found equivalents");
					Assert.AreEqual(ignoreSet.Count, 1, "Ignorable character not parsed from rule");
				}
			}
		}

		[Test]
		public void XHTMLExportGetDigraphMapsFromICUSortRules_UnicodeTertiaryIgnorableWorks()
		{
			CoreWritingSystemDefinition ws = Cache.LangProject.DefaultVernacularWritingSystem;
			ws.DefaultCollation = new IcuRulesCollationDefinition("standard") {IcuRules = "&[last tertiary ignorable] = \\uA78C"};

			var exporter = new ConfiguredExport(null, null, 0);
			string output;
			using(var stream = new MemoryStream())
			{
				using(var writer = new StreamWriter(stream))
				{
					exporter.Initialize(Cache, m_propertyTable, writer, null, "xhtml", null, "dicBody");
					Dictionary<string, string> mapChars = null;
					ISet<string> ignoreSet = null;
					ISet<string> data = null;
					Assert.DoesNotThrow(() => data = exporter.GetDigraphs(ws, out mapChars, out ignoreSet));
					Assert.AreEqual(mapChars.Count, 0, "Too many characters found equivalents");
					Assert.AreEqual(ignoreSet.Count, 1, "Ignorable character not parsed from rule");
					Assert.IsTrue(ignoreSet.Contains('\uA78C'.ToString(CultureInfo.InvariantCulture)));
				}
			}
		}

		[Test]
		public void XHTMLExportGetDigraphMapsFromICUSortRules_UnicodeTertiaryIgnorableWithNoSpacesWorks()
		{
			CoreWritingSystemDefinition ws = Cache.LangProject.DefaultVernacularWritingSystem;
			ws.DefaultCollation = new IcuRulesCollationDefinition("standard") { IcuRules = "&[last tertiary ignorable]=\\uA78C" };

			var exporter = new ConfiguredExport(null, null, 0);
			string output;
			using (var stream = new MemoryStream())
			{
				using (var writer = new StreamWriter(stream))
				{
					exporter.Initialize(Cache, m_propertyTable, writer, null, "xhtml", null, "dicBody");
					Dictionary<string, string> mapChars = null;
					ISet<string> ignoreSet = null;
					ISet<string> data = null;
					Assert.DoesNotThrow(() => data = exporter.GetDigraphs(ws, out mapChars, out ignoreSet));
					Assert.AreEqual(mapChars.Count, 0, "Too many characters found equivalents");
					Assert.AreEqual(ignoreSet.Count, 1, "Ignorable character not parsed from rule");
					Assert.IsTrue(ignoreSet.Contains('\uA78C'.ToString(CultureInfo.InvariantCulture)));
				}
			}
		}

		[Test]
		public void XHTMLExportGetDigraphMapsFromICUSortRules_TertiaryIgnorableMultipleLinesWorks()
		{
			CoreWritingSystemDefinition ws = Cache.LangProject.DefaultVernacularWritingSystem;
			ws.DefaultCollation = new IcuRulesCollationDefinition("standard") { IcuRules = "&[last tertiary ignorable] = '!'\r\n&[last tertiary ignorable]='?'" };

			var exporter = new ConfiguredExport(null, null, 0);
			string output;
			using (var stream = new MemoryStream())
			{
				using (var writer = new StreamWriter(stream))
				{
					exporter.Initialize(Cache, m_propertyTable, writer, null, "xhtml", null, "dicBody");
					Dictionary<string, string> mapChars = null;
					ISet<string> ignoreSet = null;
					ISet<string> data = null;
					Assert.DoesNotThrow(() => data = exporter.GetDigraphs(ws, out mapChars, out ignoreSet));
					Assert.AreEqual(mapChars.Count, 0, "Too many characters found equivalents");
					Assert.AreEqual(ignoreSet.Count, 2, "Ignorable character not parsed from rule");
					CollectionAssert.AreEquivalent(ignoreSet, new [] {"!", "?"});
				}
			}
		}

		[Test]
		public void XHTMLExportGetDigraphMapsFromICUSortRules_TertiaryIgnorableMultipleCharsWorks()
		{
			CoreWritingSystemDefinition ws = Cache.LangProject.DefaultVernacularWritingSystem;
			ws.DefaultCollation = new IcuRulesCollationDefinition("standard") { IcuRules = "&[last tertiary ignorable] ='eb-'='oba-'='ba-'" };

			var exporter = new ConfiguredExport(null, null, 0);
			string output;
			using (var stream = new MemoryStream())
			{
				using (var writer = new StreamWriter(stream))
				{
					exporter.Initialize(Cache, m_propertyTable, writer, null, "xhtml", null, "dicBody");
					Dictionary<string, string> mapChars = null;
					ISet<string> ignoreSet = null;
					ISet<string> data = null;
					Assert.DoesNotThrow(() => data = exporter.GetDigraphs(ws, out mapChars, out ignoreSet));
					Assert.AreEqual(mapChars.Count, 0, "Too many characters found equivalents");
					Assert.AreEqual(ignoreSet.Count, 3, "Ignorable character not parsed from rule");
					CollectionAssert.AreEquivalent(ignoreSet, new[] { "eb-", "oba-", "ba-" });
				}
			}
		}

		[Test]
		public void XHTMLExportGetDigraphMapsFromICUSortRules_TertiaryIgnorableMixedSpacingWorks()
		{
			CoreWritingSystemDefinition ws = Cache.LangProject.DefaultVernacularWritingSystem;
			ws.DefaultCollation = new IcuRulesCollationDefinition("standard") { IcuRules = "&[last tertiary ignorable]= '!'\r\n&[last tertiary ignorable] ='?'" };

			var exporter = new ConfiguredExport(null, null, 0);
			string output;
			using (var stream = new MemoryStream())
			{
				using (var writer = new StreamWriter(stream))
				{
					exporter.Initialize(Cache, m_propertyTable, writer, null, "xhtml", null, "dicBody");
					Dictionary<string, string> mapChars = null;
					ISet<string> ignoreSet = null;
					ISet<string> data = null;
					Assert.DoesNotThrow(() => data = exporter.GetDigraphs(ws, out mapChars, out ignoreSet));
					Assert.AreEqual(mapChars.Count, 0, "Too many characters found equivalents");
					Assert.AreEqual(ignoreSet.Count, 2, "Ignorable character not parsed from rule");
					CollectionAssert.AreEquivalent(ignoreSet, new[] { "!", "?" });
				}
			}
		}

		[Test]
		public void XHTMLExportGetDigraphMapsFromICUSortRules_BeforeRuleSecondaryIgnored()
		{
			CoreWritingSystemDefinition ws = Cache.LangProject.DefaultVernacularWritingSystem;
			ws.DefaultCollation = new IcuRulesCollationDefinition("standard") {IcuRules = "& [before 2] a < aa <<< Aa <<< AA"};

			var exporter = new ConfiguredExport(null, null, 0);
			string output;
			using(var stream = new MemoryStream())
			{
				using(var writer = new StreamWriter(stream))
				{
					exporter.Initialize(Cache, m_propertyTable, writer, null, "xhtml", null, "dicBody");
					Dictionary<string, string> mapChars = null;
					ISet<string> ignoreSet = null;
					ISet<string> data = null;
					Assert.DoesNotThrow(() => data = exporter.GetDigraphs(ws, out mapChars, out ignoreSet));
					Assert.AreEqual(data.Count, 0, "No characters should be generated by a before 2 rule");
					Assert.AreEqual(mapChars.Count, 0, "The rule should have been ignored, no characters ought to have been mapped");
					Assert.AreEqual(ignoreSet.Count, 0, "Ignorable character incorrectly parsed from rule");
				}
			}
		}

		[Test]
		public void XHTMLExportGetDigraphMapsFromICUSortRules_BeforeRuleCombinedWithNormalRuleWorks()
		{
			CoreWritingSystemDefinition ws = Cache.LangProject.DefaultVernacularWritingSystem;
			ws.DefaultCollation = new IcuRulesCollationDefinition("standard") {IcuRules = "& a < bb & [before 1] a < aa"};

			var exporter = new ConfiguredExport(null, null, 0);
			string output;
			using(var stream = new MemoryStream())
			{
				using(var writer = new StreamWriter(stream))
				{
					exporter.Initialize(Cache, m_propertyTable, writer, null, "xhtml", null, "dicBody");
					Dictionary<string, string> mapChars = null;
					ISet<string> ignoreSet = null;
					ISet<string> data = null;
					Assert.DoesNotThrow(() => data = exporter.GetDigraphs(ws, out mapChars, out ignoreSet));
					Assert.AreEqual(data.Count, 2, "The [before 1] rule should have added one additional character");
				}
			}
		}

		[Test]
		public void XHTMLExportGetDigraphMapsFromICUSortRules_BeforeRulePrimaryGetsADigraph()
		{
			CoreWritingSystemDefinition ws = Cache.LangProject.DefaultVernacularWritingSystem;
			ws.DefaultCollation = new IcuRulesCollationDefinition("standard") {IcuRules = "& [before 1] a < aa <<< Aa <<< AA"};

			var exporter = new ConfiguredExport(null, null, 0);
			string output;
			using(var stream = new MemoryStream())
			{
				using(var writer = new StreamWriter(stream))
				{
					exporter.Initialize(Cache, m_propertyTable, writer, null, "xhtml", null, "dicBody");
					Dictionary<string, string> mapChars = null;
					ISet<string> ignoreSet = null;
					ISet<string> data = null;
					Assert.DoesNotThrow(() => data = exporter.GetDigraphs(ws, out mapChars, out ignoreSet));
					Assert.AreEqual(data.Count, 1, "Wrong number of character mappings found");
					Assert.AreEqual(mapChars.Count, 2, "Wrong number of character mappings found");
					Assert.AreEqual(ignoreSet.Count, 0, "Ignorable character incorrectly parsed from rule");
				}
			}
		}

		[Test]
		public void XHTMLExportGetDigraphMapsFirstCharactersFromToolboxSortRules()
		{
			CoreWritingSystemDefinition ws = Cache.LangProject.DefaultVernacularWritingSystem;
			ws.DefaultCollation = new SimpleRulesCollationDefinition("standard") {SimpleRules = "b" + Environment.NewLine + "az a" + Environment.NewLine + "c ch"};

			var exporter = new ConfiguredExport(null, null, 0);
			string output;
			using (var stream = new MemoryStream())
			{
				using (var writer = new StreamWriter(stream))
				{
					exporter.Initialize(Cache, m_propertyTable, writer, null, "xhtml", null, "dicBody");
					Dictionary<string, string> mapChars;
					ISet<string> ignoreSet;
					var data = exporter.GetDigraphs(ws, out mapChars, out ignoreSet);
					Assert.AreEqual(mapChars.Count, 2, "Too many characters found equivalents");
					Assert.AreEqual(mapChars["a"], "az");
					Assert.AreEqual(mapChars["ch"], "c");
				}
			}
		}

		[Test]
		public void XHTMLExportGetDigraphMapsFirstCharactersFromSortRulesWithNoMapping()
		{
			var ws = Cache.LangProject.DefaultVernacularWritingSystem;
			ws.DefaultCollation = new SimpleRulesCollationDefinition("standard") { SimpleRules = "b" + Environment.NewLine + "ñe ñ"};

			var exporter = new ConfiguredExport(null, null, 0);
			string output;
			using (var stream = new MemoryStream())
			{
				using (var writer = new StreamWriter(stream))
				{
					exporter.Initialize(Cache, m_propertyTable, writer, null, "xhtml", null, "dicBody");
					Dictionary<string, string> mapChars;
					ISet<string> ignoreSet;
					var data = exporter.GetDigraphs(ws, out mapChars, out ignoreSet);
					Assert.AreEqual(data.Count, 2, "Two Digraphs should be returned");
					Assert.AreEqual(mapChars["ñ"], "ñe");
				}
			}
		}

		[Test]
		public void XHTMLExportGetLeadChar_SurrogatePairDoesNotCrash()
		{
			string data = null;
			Cache.ServiceLocator.WritingSystemManager.GetOrSet("ipo", out var wsDef);
			Cache.ServiceLocator.WritingSystems.AddToCurrentVernacularWritingSystems(wsDef);
			string entryLetter = "\U00016F00\U00016F51\U00016F61\U00016F90";
			Dictionary<string, Dictionary<string, ConfiguredExport.CollationLevel>> wsDigraphMap = new Dictionary<string, Dictionary<string, ConfiguredExport.CollationLevel>>();
			Dictionary<string, Dictionary<string, string>> wsCharEquivalentMap = new Dictionary<string, Dictionary<string, string>>();
			Dictionary<string, ISet<string>> wsIgnorableCharMap = new Dictionary<string, ISet<string>>();
			Assert.DoesNotThrow(() => data = ConfiguredExport.GetLeadChar(entryLetter, "ipo", wsDigraphMap, wsCharEquivalentMap, wsIgnorableCharMap, null, Cache));
			Assert.That(data.Length, Is.EqualTo(2), "Surrogate pair should contains 2 characters");
		}

		[Test]
		public void XHTMLExportGetLeadChar_MultigraphsInIgnoreListAreIgnored()
		{
			string data = null;
			Cache.ServiceLocator.WritingSystemManager.GetOrSet("guq", out var wsDef);
			wsDef.DefaultCollation = new IcuRulesCollationDefinition("standard") { IcuRules = "&[last tertiary ignorable] ='ig'='ignore-'='i'" };
			Cache.ServiceLocator.WritingSystems.AddToCurrentVernacularWritingSystems(wsDef);
			var wsDigraphMap = new Dictionary<string, Dictionary<string, ConfiguredExport.CollationLevel>>();
			var wsCharEquivalentMap = new Dictionary<string, Dictionary<string, string>>();
			var wsIgnorableCharMap = new Dictionary<string, ISet<string>>();
			// test for the longest of the ignore rules
			Assert.DoesNotThrow(() => data = ConfiguredExport.GetLeadChar("ignore-a", "guq", wsDigraphMap, wsCharEquivalentMap, wsIgnorableCharMap, null, Cache));
			Assert.That(data, Is.EqualTo("a"));
			// test for the shortest of the ignore rules
			Assert.DoesNotThrow(() => data = ConfiguredExport.GetLeadChar("ia", "guq", wsDigraphMap, wsCharEquivalentMap, wsIgnorableCharMap, null, Cache));
			Assert.That(data, Is.EqualTo("a"));
		}

		[Test]
		public void XHTMLExportGetLeadChar_PrimaryCollationProceedsSecondary()
		{
			string data = null;
			Cache.ServiceLocator.WritingSystemManager.GetOrSet("guq", out var wsDef);
			wsDef.DefaultCollation = new IcuRulesCollationDefinition("standard") { IcuRules = "&a << ha &c < ch " };
			Cache.ServiceLocator.WritingSystems.AddToCurrentVernacularWritingSystems(wsDef);
			var wsDigraphMap = new Dictionary<string, Dictionary<string, ConfiguredExport.CollationLevel>>();
			var wsCharEquivalentMap = new Dictionary<string, Dictionary<string, string>>();
			var wsIgnorableCharMap = new Dictionary<string, ISet<string>>();
			// test that the primary rule 'ch' has a higher priority than the secondary rule which replaces 'ha' with 'a'
			// (ie. confirm that 'ch' is returned instead of 'c')
			Assert.DoesNotThrow(() => data = ConfiguredExport.GetLeadChar("cha", "guq", wsDigraphMap, wsCharEquivalentMap, wsIgnorableCharMap, null, Cache));
			Assert.That(data, Is.EqualTo("ch"));
		}


		[Test]
		public void XHTMLExportGetLeadChar_UsesCaseAlias()
		{
			string data = null;
			Cache.ServiceLocator.WritingSystemManager.GetOrSet("tkr", out var wsDef);
			wsDef.CaseAlias = "az";
			Cache.ServiceLocator.WritingSystems.AddToCurrentVernacularWritingSystems(wsDef);
			const string headword = "Indebted";
			var wsDigraphMap = new Dictionary<string, Dictionary<string, ConfiguredExport.CollationLevel>>();
			var wsCharEquivalentMap = new Dictionary<string, Dictionary<string, string>>();
			var wsIgnorableCharMap = new Dictionary<string, ISet<string>>();
			Assert.DoesNotThrow(() => data = ConfiguredExport.GetLeadChar(headword, "tkr", wsDigraphMap, wsCharEquivalentMap, wsIgnorableCharMap, null, Cache));
			Assert.That(data, Is.EqualTo("\u0131"), "When using Azerbaijani casing, dotted and undotted I's are different letters.");
		}

		/// <summary>
		/// Test verifies minimal behavior added for sort rules other than Toolbox and ICU
		/// (which currently does something minimal, enough to prevent crashes).
		/// This test currently just verifies that, indeed, we don't crash.
		/// It may be desirable to do something more for some or all of the other cases,
		/// in which case this test will probably need to change.
		/// </summary>
		[Test]
		public void XHTMLExportGetDigraphMapsFirstCharactersFromOtherSortRules()
		{
			CoreWritingSystemDefinition ws = Cache.LangProject.DefaultVernacularWritingSystem;
			ws.DefaultCollation = new SystemCollationDefinition {LanguageTag = "fr"};

			var exporter = new ConfiguredExport(null, null, 0);
			string output;
			using (var stream = new MemoryStream())
			{
				using (var writer = new StreamWriter(stream))
				{
					exporter.Initialize(Cache, m_propertyTable, writer, null, "xhtml", null, "dicBody");
					exporter.GetDigraphs(ws, out var mapChars, out _);
					Assert.AreEqual(mapChars.Count, 0, "No equivalents expected");
				}
			}
		}

		private void TestBeginCssClassForFlowType(string flowType)
		{
			var exporter = new ConfiguredExport(null, null, 0);
			string output;
			using (var stream = new MemoryStream())
			{
				using (var writer = new StreamWriter(stream))
				{
					exporter.Initialize(Cache, m_propertyTable, writer, null, "xhtml", null, "dicBody");

					var frag = new XmlDocument();
					frag.LoadXml("<p css='some#style' flowType='" + flowType + "'/>");

					exporter.BeginCssClassIfNeeded(frag.DocumentElement);
					writer.Flush();
					stream.Seek(0, SeekOrigin.Begin);
					using (var reader = new StreamReader(stream))
					{
						output = reader.ReadToEnd();
					}
				}
			}
			Assert.That(output, Does.Contain("class=\"someNUMBER_SIGNstyle\""));
		}
	}
}
