using System.Collections.Generic;

namespace DiceArena.Engine
{
	// A spell printed on a dice face or learned in loadout
	public class Spell
	{
		public int Tier { get; set; } = 0;
		public string Name { get; set; } = "blank";
		public string Description { get; set; } = "";
		public bool IsPhysical { get; set; } = false;
		public bool IgnoresArmor { get; set; } = false;
	}

	// A dice face outcome
	public class Face
	{
		// "spell", "upgrade", "classAbility", "blank"
		public string Kind { get; set; } = "blank";
		public Spell Spell { get; set; } = null; // only for Kind=="spell"
	}

	public class Player
	{
		public string ClassId { get; set; } = "thief";
		public string Name { get; set; } = "P1";
		public int HP { get; set; } = 20;
		public int Armor { get; set; } = 0;
		public List<Spell> Spells { get; set; } = new List<Spell>(); // 4 slots total (padded)
		public Dictionary<string,int> Stacks { get; set; } = new Dictionary<string, int> {{"psn",0},{"bmb",0}};
	}

	public class Enemy
	{
		public string Name { get; set; } = "Enemy";
		public int Tier { get; set; } = 1;
		public int HP { get; set; } = 8;
		public int Armor { get; set; } = 0;
		public bool IsBoss { get; set; } = false;
	}

	public class InitiativeEntry
	{
		public string Id { get; set; } = "P1"; // "P1", "E2", etc.
		public int Roll { get; set; } = 1;
	}
}
