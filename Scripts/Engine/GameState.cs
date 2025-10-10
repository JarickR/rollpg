// res://Scripts/Engine/GameState.cs
using System;
using System.Collections.Generic;

namespace RollPG.Engine
{
	/// <summary>
	/// Bridges your loadout UI to combat and also exposes the members
	/// expected by DiceArena.Engine.CombatSystem (Players, Enemies, Rng, AddLog).
	/// Also resolves class strings and chosen spells from ContentDB.
	/// </summary>
	public sealed class GameState
	{
		// ------------ Selections from the UI ------------
		public RollPG.Content.Loadout PlayerLoadout { get; set; } = new();

		// ------------ Resolved data from ContentDB ------------
		public string ClassName { get; private set; } = "";
		public string ClassTrait { get; private set; } = "";
		public string ClassHeroAction { get; private set; } = "";
		public List<RollPG.Content.SpellDef> ChosenSpells { get; } = new(); // fully-qualified to avoid ambiguity

		// ------------ Minimal combat surface (for CombatSystem) ------------
		// We reference DiceArena.Engine types so CombatSystem stays unchanged.
		public List<DiceArena.Engine.Hero> Players { get; } = new();
		public List<DiceArena.Engine.Enemy> Enemies { get; } = new();

		// Shared RNG used by some upgrade flows
		public Random Rng { get; } = new();

		// Simple log collector; wire to UI as needed.
		public List<string> Log { get; } = new();
		public void AddLog(string msg)
		{
			if (!string.IsNullOrWhiteSpace(msg))
				Log.Add(msg);
		}

		// Example baseline stats you can expand later
		public int CurrentHP { get; set; } = 10;
		public int CurrentArmor { get; set; } = 0;
		public Dictionary<string, int> Conditions { get; } = new();

		/// <summary>
		/// Call after PlayerLoadout is assigned. Resolves class/traits and chosen spells.
		/// </summary>
		public void InitializeFromContent()
		{
			ChosenSpells.Clear();

			var cls = RollPG.Content.ContentDB.GetClass(PlayerLoadout.ClassId);
			if (cls != null)
			{
				ClassName       = cls.Name;
				ClassTrait      = cls.Trait;
				ClassHeroAction = cls.HeroAction;
			}

			foreach (var sid in PlayerLoadout.SpellIds)
			{
				var s = RollPG.Content.ContentDB.GetSpell(sid);
				if (s != null) ChosenSpells.Add(s);
			}
		}
	}
}
