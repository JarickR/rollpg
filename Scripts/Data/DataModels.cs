// res://Scripts/Data/DataModels.cs
#nullable enable
using System.Collections.Generic;

namespace DiceArena.Data
{
	public class ClassDef
	{
		public string Id { get; set; } = "";
		public string Name { get; set; } = "";
		public string Trait { get; set; } = "";
		/// <summary>Display text of the Class Ability (multi-line allowed).</summary>
		public string HeroAction { get; set; } = "";
	}

	public class SpellDef
	{
		/// <summary>Stable id, e.g. "attack", "heal", "fireball".</summary>
		public string Id { get; set; } = "";
		/// <summary>UI display name, e.g. "Attack", "Heal", "Fireball".</summary>
		public string Name { get; set; } = "";
		/// <summary>1, 2, or 3.</summary>
		public int Tier { get; set; }
		/// <summary>Category/kind: "attack","sweep","heal","armor","fireball","poison","bomb","concentration"…</summary>
		public string Kind { get; set; } = "";
		/// <summary>Optional tag list, e.g. ["defensive"] for padding logic.</summary>
		public List<string> Tags { get; set; } = new();
		/// <summary>Optional short rules text for UI.</summary>
		public string? Text { get; set; }
	}
}
