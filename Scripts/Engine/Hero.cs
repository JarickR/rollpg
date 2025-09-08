// res://Scripts/Engine/Hero.cs
#nullable enable
using System.Collections.Generic;

namespace DiceArena.Engine
{
	public enum DefenseType
	{
		None = 0,
		Disrupt = 1,     // cancel next spell targeting you
		Redirect = 2,    // next spell targeting you goes to another player
		Delay = 3,       // next spell is delayed to start of your next turn
		Counterspell = 4,// next spell: half damage to you and reflect half to caster (works vs AoE too)
		Dodge = 5,       // physical attack 1–3 dodge, 4–6 fail
		Smoke = 6        // next spell: 50% miss
	}

	/// <summary>Ephemeral defense applied by rolling the Defend face. Consumed by the next eligible incoming effect this turn.</summary>
	public sealed class DefenseToken
	{
		public DefenseType Type { get; set; } = DefenseType.None;

		/// <summary>Optional: for Redirect, who to redirect to (Id of another hero). If null, UI/logic may auto-pick or fail.</summary>
		public string? RedirectTargetId { get; set; }

		/// <summary>True if still available to trigger; when consumed set to false.</summary>
		public bool Active { get; set; } = true;

		public override string ToString() => Type.ToString();
	}

	public class Hero
	{
		// Identity
		public string Id { get; set; } = "P1";
		public string Name { get; set; } = "Hero";
		public string ClassId { get; set; } = "thief";

		// Alias expected by some UIs
		public string ClassKey { get => ClassId; set => ClassId = value; }

		// Stats
		public int MaxHp { get; set; } = 20;
		public int Hp { get; set; } = 20;
		public int Armor { get; set; } = 0;

		// Alias expected by some UIs
		public int CurrentHp { get => Hp; set => Hp = value; }

		// Status stacks
		public int PoisonStacks { get; set; } = 0;
		public int BombStacks { get; set; } = 0;
		public int ConcentrationStacks { get; set; } = 0;

		/// <summary>
		/// One-turn defensive token, granted when rolling a Defend face.
		/// Cleared at start of your next turn (or when consumed).
		/// </summary>
		public DefenseToken? Defense { get; set; }

		// Always 4 slots — pad with Defend faces as needed by the loadout picker
		public List<Spell> Loadout { get; set; } = new(4)
		{
			Spells.Blank(), Spells.Blank(), Spells.Blank(), Spells.Blank()
		};

		public Hero() { }

		// Minimal ctor used by some call sites
		public Hero(string id, int maxHp = 20)
		{
			Id = id; Name = id; ClassId = "thief";
			MaxHp = maxHp; Hp = maxHp;
		}

		// Main ctor
		public Hero(string id, string name, string classId, int maxHp = 20, int armor = 0, IEnumerable<Spell>? loadout = null)
		{
			Id = id; Name = name; ClassId = classId;
			MaxHp = maxHp; Hp = maxHp; Armor = armor;

			Loadout = new List<Spell>(4);
			if (loadout != null)
			{
				foreach (var s in loadout)
				{
					if (Loadout.Count == 4) break;
					Loadout.Add(s ?? Spells.Blank());
				}
			}
			while (Loadout.Count < 4) Loadout.Add(Spells.Blank());
		}

		// Compatibility ctor so named args like hp: 20 don’t blow up
		public Hero(string id, string name, string classId, int hp, int armor, bool usingHpNamedArg)
		{
			Id = id; Name = name; ClassId = classId;
			MaxHp = hp; Hp = hp; Armor = armor;
			Loadout = new List<Spell>(4) { Spells.Blank(), Spells.Blank(), Spells.Blank(), Spells.Blank() };
		}

		// Utilities
		public void AddArmor(int amt) => Armor = System.Math.Max(0, Armor + System.Math.Max(0, amt));
		public void Heal(int amt)      => Hp    = System.Math.Min(MaxHp, Hp + System.Math.Max(0, amt));

		public void Damage(int amt, bool bypassArmor)
		{
			amt = System.Math.Max(0, amt);
			if (!bypassArmor && Armor > 0)
			{
				var blocked = System.Math.Min(Armor, amt);
				Armor -= blocked;
				amt   -= blocked;
			}
			if (amt > 0) Hp = System.Math.Max(0, Hp - amt);
		}

		// Loadout helpers
		public void ClearLoadoutToBlanks()
		{
			Loadout = new List<Spell>(4) { Spells.Blank(), Spells.Blank(), Spells.Blank(), Spells.Blank() };
		}

		public void SetSlot(int slot, Spell spell)
		{
			if (slot < 0 || slot > 3) return;
			while (Loadout.Count < 4) Loadout.Add(Spells.Blank());
			Loadout[slot] = spell ?? Spells.Blank();
		}

		public void SetLoadout(IEnumerable<Spell> spells)
		{
			Loadout.Clear();
			if (spells != null)
			{
				foreach (var s in spells)
				{
					if (Loadout.Count == 4) break;
					Loadout.Add(s ?? Spells.Blank());
				}
			}
			while (Loadout.Count < 4) Loadout.Add(Spells.Blank());
		}

		public override string ToString() => $"{Id} {Name} [{ClassId}] HP {Hp}/{MaxHp} AR {Armor}";
	}
}
