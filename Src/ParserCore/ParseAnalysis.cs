// Copyright (c) 2014-2020 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SIL.FieldWorks.WordWorks.Parser
{
	public sealed class ParseAnalysis : IEquatable<ParseAnalysis>
	{
		internal ParseAnalysis(IEnumerable<ParseMorph> morphs)
		{
			Morphs = new ReadOnlyCollection<ParseMorph>(morphs.ToArray());
		}

		public ReadOnlyCollection<ParseMorph> Morphs { get; }

		internal bool IsValid
		{
			get { return Morphs.All(morph => morph.IsValid); }
		}

		public bool Equals(ParseAnalysis other)
		{
			return Morphs.SequenceEqual(other.Morphs);
		}

		public override bool Equals(object obj)
		{
			return obj is ParseAnalysis other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Morphs.Aggregate(23, (current, morph) => current * 31 + morph.GetHashCode());
		}
	}
}