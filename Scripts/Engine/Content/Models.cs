using System.Collections.Generic;

namespace DiceArena.Engine.Content
{
	// Keep enums light; your "kind" column can stay stringly-typed for now.
	public enum TargetType { Self, Ally, Enemy, AllAllies, AllEnemies, Any }

	public sealed class ClassDef
	{
		public string Id { get; set; } = "";          // e.g., "thief", "vampire"
		public string Name { get; set; } = "";        // "Thief"
		public string Trait { get; set; } = "";       // passive text (Backstab, Bulwark, etc.)
		public string HeroAction { get; set; } = "";  // multi-line action text
		public Dictionary<string, object?> Extra { get; set; } = new(); // future-proof
	}

	public sealed class SpellDef
	{
		public string Id { get; set; } = "";          // e.g., "fireball-1"
		public string Name { get; set; } = "";        // "Fireball"
		public int Tier { get; set; }                 // 1, 2, 3
		public string Kind { get; set; } = "";        // "attack", "heal", "fireball", ...
		public string Text { get; set; } = "";        // effect rules text
		public int Order { get; set; }                // for UI sorting
		public Dictionary<string, object?> Extra { get; set; } = new();
	}

	public sealed class ContentBundle
	{
		public List<ClassDef> Classes { get; set; } = new();
		public List<SpellDef> Spells  { get; set; } = new();
	}
}
