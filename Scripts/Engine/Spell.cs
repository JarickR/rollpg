// res://Scripts/Engine/Spell.cs
namespace DiceArena.Engine
{
	public enum SpellKind
	{
		Blank = 0,
		Attack,
		Heal,
		Armor,
		Sweep,
		Fireball,
		Poison,
		Bomb,
		Concentration
	}

	public class Spell
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public int Tier { get; set; }
		public SpellKind Kind { get; set; }
		public string Description { get; set; }
		public string IconKey { get; set; }

		public Spell(string id, string name, int tier, SpellKind kind, string description = "", string iconKey = "")
		{
			Id = id; Name = name; Tier = tier; Kind = kind;
			Description = description ?? string.Empty;
			IconKey = iconKey ?? string.Empty;
		}

		// Legacy/serializer-friendly
		public Spell()
		{
			Id = "blank"; Name = "Blank"; Tier = 0; Kind = SpellKind.Blank;
			Description = ""; IconKey = "";
		}

		public override string ToString() => $"{Name} (T{Tier}, {Kind})";
	}
}
