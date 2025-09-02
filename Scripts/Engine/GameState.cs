using System;
using System.Collections.Generic;
using System.Linq;

namespace DiceArena.Engine
{
	public enum Phase { Loadout, Battle }

	public sealed class GameState
	{
		public Phase Phase { get; private set; } = Phase.Loadout;

		public List<Player> Players { get; } = new();
		public List<Enemy> Enemies { get; } = new();
		public List<InitiativeEntry> Initiative { get; } = new();
		public int TurnIndex { get; private set; } = 0;

		public List<string> Log { get; } = new();
		public int LogMax = 200;

		public int MaxSlotsPerPlayer = 4;

		public void Reset()
		{
			Phase = Phase.Loadout;
			Players.Clear();
			Enemies.Clear();
			Initiative.Clear();
			TurnIndex = 0;
			Log.Clear();
		}

		public void PushLog(string text)
		{
			Log.Add(text ?? "");
			if (Log.Count > LogMax) Log.RemoveAt(0);
		}

		// ------- Player creation -------
		public Player BuildPlayer(string classId, IEnumerable<Spell> t1, IEnumerable<Spell> t2, string name = null)
		{
			var p = new Player
			{
				ClassId = classId ?? "unknown",
				Name = name ?? classId ?? "Hero",
				HP = 20,
				Armor = 0,
				Spells = new List<Spell>()
			};

			if (t1 != null) p.Spells.AddRange(t1.Select(Clone));
			if (t2 != null) p.Spells.AddRange(t2.Select(Clone));

			while (p.Spells.Count < MaxSlotsPerPlayer) p.Spells.Add(MakeBlank());
			while (p.Spells.Count > MaxSlotsPerPlayer) p.Spells.RemoveAt(p.Spells.Count - 1);

			return p;
		}

		private static Spell MakeBlank() => new Spell { Tier = 0, Name = "blank", Description = "Empty slot" };
		private static Spell Clone(Spell s) => new Spell {
			Tier = s?.Tier ?? 0, Name = s?.Name ?? "blank", Description = s?.Description ?? "",
			IsPhysical = s?.IsPhysical ?? false, IgnoresArmor = s?.IgnoresArmor ?? false
		};

		// ------- Encounter setup -------
		public void StartEncounter(IEnumerable<Player> party, int tier)
		{
			Reset();
			Phase = Phase.Battle;

			if (party != null) Players.AddRange(party);

			Enemies.Clear();
			if (tier >= 3)
			{
				Enemies.Add(new Enemy { Name = "Boss", HP = 30, Armor = 0, Tier = 3, IsBoss = true });
			}
			else
			{
				int count = Math.Max(1, Players.Count);
				for (int i = 0; i < count; i++)
				{
					int hp = tier == 2 ? 14 : 8;
					Enemies.Add(new Enemy { Name = $"Enemy {i + 1}", Tier = tier, HP = hp, Armor = 0, IsBoss = false });
				}
			}

			RollInitiative();
			PushLog("Encounter begins.");
			AnnounceInitiative();
		}

		public void RollInitiative()
		{
			Initiative.Clear();
			for (int i = 0; i < Players.Count; i++)
				Initiative.Add(new InitiativeEntry { Id = $"P{i+1}", Roll = Dice.RollD20() });
			for (int j = 0; j < Enemies.Count; j++)
				Initiative.Add(new InitiativeEntry { Id = $"E{j+1}", Roll = Dice.RollD20() });

			Initiative.Sort((a,b) => b.Roll.CompareTo(a.Roll)); // high to low
			TurnIndex = 0;
		}

		private void AnnounceInitiative()
		{
			foreach (var it in Initiative)
				PushLog($"{it.Id}: {it.Roll}");
		}

		public InitiativeEntry GetActiveTurn() => Initiative.Count == 0 ? null : Initiative[TurnIndex % Initiative.Count];
		public void NextTurn() { if (Initiative.Count > 0) TurnIndex = (TurnIndex + 1) % Initiative.Count; }

		// ------- Combat helpers -------
		public static int DamageAfterArmor(int raw, int armor, bool bypassArmor)
			=> bypassArmor ? Math.Max(0, raw) : Math.Max(0, raw - Math.Max(0, armor));

		public void ApplyDamageToEnemy(int idx, int raw, bool bypassArmor)
		{
			if (idx < 0 || idx >= Enemies.Count) return;
			var e = Enemies[idx];
			int dealt = DamageAfterArmor(raw, e.Armor, bypassArmor);
			e.HP = Math.Max(0, e.HP - dealt);
			if (dealt > 0) PushLog($"Dealt {dealt} to E{idx+1}.");
			if (e.HP == 0) PushLog($"E{idx+1} defeated.");
		}

		public void ApplyDamageToPlayer(int idx, int raw, bool bypassArmor)
		{
			if (idx < 0 || idx >= Players.Count) return;
			var p = Players[idx];
			int dealt = DamageAfterArmor(raw, p.Armor, bypassArmor);
			p.HP = Math.Max(0, p.HP - dealt);
			if (dealt > 0) PushLog($"P{idx+1} took {dealt}.");
			if (p.HP == 0) PushLog($"P{idx+1} is down!");
		}

		public void HealPlayer(int idx, int amount)
		{
			if (idx < 0 || idx >= Players.Count) return;
			var p = Players[idx];
			int before = p.HP;
			p.HP = Math.Min(20, p.HP + Math.Max(0, amount));
			int healed = p.HP - before;
			if (healed > 0) PushLog($"P{idx+1} healed {healed}.");
		}

		public void AddArmorToPlayer(int idx, int amount)
		{
			if (idx < 0 || idx >= Players.Count) return;
			Players[idx].Armor = Math.Max(0, Players[idx].Armor + Math.Max(0, amount));
			PushLog($"P{idx+1} gains {amount} armor.");
		}

		// Very simple spell resolution — matches your earlier rules
		public void ResolvePlayerRoll(int pIndex, Face face)
		{
			if (pIndex < 0 || pIndex >= Players.Count) return;
			if (face == null) { NextTurn(); return; }

			if (face.Kind == "spell" && face.Spell != null)
			{
				var s = face.Spell;
				int tier = Math.Max(1, s.Tier);

				switch (s.Name)
				{
					case "attack":   if (Enemies.Count > 0) ApplyDamageToEnemy(0, 2 * tier, false); break;
					case "heal":     HealPlayer(pIndex, 1 * tier); break;
					case "armor":    AddArmorToPlayer(pIndex, 2 * tier); break;
					case "fireball": if (Enemies.Count > 0) ApplyDamageToEnemy(0, 2 * tier, true); break;
					case "poison":   PushLog($"E1 PSN +1."); break; // hook up stacks later
					case "bomb":     PushLog($"E1 BMB +1."); break;
					case "blank":    PushLog($"P{pIndex+1} rolled blank."); break;
					default:         PushLog($"P{pIndex+1} cast {s.Name} (T{tier})."); break;
				}
			}
			else if (face.Kind == "upgrade")
			{
				PushLog($"P{pIndex+1} rolled UPGRADE (UI decides slot and accept/decline).");
			}
			else if (face.Kind == "classAbility")
			{
				PushLog($"P{pIndex+1} class ability triggers.");
			}
			else
			{
				PushLog($"P{pIndex+1} rolled {face.Kind}.");
			}

			NextTurn();
		}
	}
}
