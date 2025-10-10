// File: Scripts/Engine/Content/Models.cs
using System.Collections.Generic;
using System.Linq;

namespace DiceArena.Engine.Content
{
	public sealed class ClassDef
	{
		public string Id { get; set; } = "";
		public string Name { get; set; } = "";
		public string Trait { get; set; } = "";
		public string HeroAction { get; set; } = "";
	}

	public sealed class SpellDef
	{
		public string Id { get; set; } = "";
		public string Name { get; set; } = "";
		public int Tier { get; set; }
		public string Kind { get; set; } = "";
		public string Text { get; set; } = "";
		public int Order { get; set; }
	}

	/// <summary>
	/// Canonical bundle used by the game UI. Exposes tiered spell lists by name.
	/// </summary>
	public sealed class ContentBundle
	{
		public IReadOnlyList<ClassDef> Classes { get; }
		public IReadOnlyList<SpellDef> AllSpells { get; }
		public IReadOnlyList<SpellDef> SpellsTier1 { get; }
		public IReadOnlyList<SpellDef> SpellsTier2 { get; }

		public ContentBundle(IReadOnlyList<ClassDef> classes, IReadOnlyList<SpellDef> allSpells)
		{
			Classes = classes;
			AllSpells = allSpells;

			SpellsTier1 = allSpells.Where(s => s.Tier == 1)
								   .OrderBy(s => s.Order)
								   .ToList();
			SpellsTier2 = allSpells.Where(s => s.Tier == 2)
								   .OrderBy(s => s.Order)
								   .ToList();
		}
	}
}
