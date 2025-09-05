// res://Scripts/Engine/Spells.cs
using System;
using System.Collections.Generic;

namespace DiceArena.Engine
{
	/// <summary>Factory methods (so code can call Spells.Attack()), plus tier pools.</summary>
	public static class Spells
	{
		// Factories (methods — match Spells.Attack() style)
		public static Spell Blank()         => new Spell("blank", "Blank", 0, SpellKind.Blank, "No effect.");
		public static Spell Attack(int t=1) => new Spell($"attack_t{t}", "Attack", t, SpellKind.Attack, "Physical strike.");
		public static Spell Heal(int t=1)   => new Spell($"heal_t{t}",   "Heal",   t, SpellKind.Heal,   "Restore HP.");
		public static Spell Armor(int t=1)  => new Spell($"armor_t{t}",  "Armor",  t, SpellKind.Armor,  "Gain armor.");
		public static Spell Sweep(int t=1)  => new Spell($"sweep_t{t}",  "Sweep",  t, SpellKind.Sweep,  "Wide physical swing.");
		public static Spell Concentration(int t=1)
			=> new Spell($"concentration_t{t}", "Concentration", t, SpellKind.Concentration, "Empower next damage.");
		public static Spell Fireball(int t=2)
			=> new Spell($"fireball_t{t}", "Fireball", t, SpellKind.Fireball, "Bypasses armor.");
		public static Spell Poison(int t=2)
			=> new Spell($"poison_t{t}", "Poison", t, SpellKind.Poison, "Apply PSN; 1/6 cure.");
		public static Spell Bomb(int t=2)
			=> new Spell($"bomb_t{t}", "Bomb", t, SpellKind.Bomb, "Attach BMB; 2/3 pass, 1/3 explode.");

		// Tier pools
		public static readonly List<Spell> Tier1Pool = new()
		{
			Attack(1), Heal(1), Armor(1), Sweep(1), Concentration(1)
		};

		public static readonly List<Spell> Tier2Pool = new()
		{
			Fireball(2), Poison(2), Bomb(2)
		};

		// Helpers
		public static bool IsPhysical(Spell s) =>
			s.Kind == SpellKind.Attack || s.Kind == SpellKind.Sweep || s.Kind == SpellKind.Bomb;

		public static bool BypassesArmor(Spell s) =>
			s.Kind == SpellKind.Fireball || s.Kind == SpellKind.Poison;

		public static Spell RandomFromTier(int tier, Random? rng = null)
		{
			rng ??= new Random();
			if (tier <= 1) return Tier1Pool[rng.Next(Tier1Pool.Count)];
			if (tier == 2) return Tier2Pool[rng.Next(Tier2Pool.Count)];
			return Blank();
		}
	}
}
