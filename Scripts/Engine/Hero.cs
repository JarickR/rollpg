#nullable enable
using System.Collections.Generic;
using DiceArena.Data; // ✅ add this for ClassData / SpellData

namespace DiceArena.Engine
{
	public enum DefenseType
	{
		None = 0,
		Disrupt = 1,
		Redirect = 2,
		Delay = 3,
		Counterspell = 4,
		Dodge = 5,
		Smoke = 6
	}

	public sealed class DefenseToken
	{
		public DefenseType Type { get; set; } = DefenseType.None;
		public string? RedirectTargetId { get; set; }
		public bool Active { get; set; } = true;
		public override string ToString() => Type.ToString();
	}

	public class Hero
	{
		// Identity
		public string Id { get; set; } = "P1";
		public string Name { get; set; } = "Hero";
		public string ClassId { get; set; } = "thief";

		public string ClassKey { get => ClassId; set => ClassId = value; }

		// Stats
		public int MaxHp { get; set; } = 20;
		public int Hp { get; set; } = 20;
		public int Armor { get; set; } = 0;

		public int CurrentHp { get => Hp; set => Hp = value; }

		// Status stacks
		public int PoisonStacks { get; set; } = 0;
		public int BombStacks { get; set; } = 0;
		public int ConcentrationStacks { get; set; } = 0;

		public int SpikedThorns { get; set; } = 0;
		public DefenseToken? Defense { get; set; }

		public List<Spell> Loadout { get; set; } = new(4)
		{
			Spells.Blank(), Spells.Blank(), Spells.Blank(), Spells.Blank()
		};

		public Hero() { }

		public Hero(string id, int maxHp = 20)
		{
			Id = id; Name = id; ClassId = "thief";
			MaxHp = maxHp; Hp = maxHp;
		}

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

		public Hero(string id, string name, string classId, int hp, int armor, bool usingHpNamedArg)
		{
			Id = id; Name = name; ClassId = classId;
			MaxHp = hp; Hp = hp; Armor = armor;
			Loadout = new List<Spell>(4)
			{
				Spells.Blank(), Spells.Blank(), Spells.Blank(), Spells.Blank()
			};
		}

		public void AddArmor(int amt) => Armor = System.Math.Max(0, Armor + System.Math.Max(0, amt));
		public void Heal(int amt) => Hp = System.Math.Min(MaxHp, Hp + System.Math.Max(0, amt));

		public void Damage(int amt, bool bypassArmor)
		{
			amt = System.Math.Max(0, amt);
			if (!bypassArmor && Armor > 0)
			{
				var blocked = System.Math.Min(Armor, amt);
				Armor -= blocked;
				amt -= blocked;
			}
			if (amt > 0) Hp = System.Math.Max(0, Hp - amt);
		}

		public void ClearLoadoutToBlanks()
		{
			Loadout = new List<Spell>(4)
			{
				Spells.Blank(), Spells.Blank(), Spells.Blank(), Spells.Blank()
			};
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

		// 🆕 Loadout metadata (for UI + LoadoutScreen)
		public ClassData? Class { get; set; }
		public SpellData? Tier1Spell { get; set; }
		public SpellData? Tier2Spell { get; set; }

		public override string ToString() => $"{Id} {Name} [{ClassId}] HP {Hp}/{MaxHp} AR {Armor}";
	}
}
