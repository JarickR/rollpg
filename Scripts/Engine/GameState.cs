// res://Scripts/Engine/GameState.cs
using System;
using System.Collections.Generic;

namespace DiceArena.Engine
{
	public class GameState
	{
		public readonly List<Hero>  Players  = new();
		public readonly List<Enemy> Enemies  = new();
		public readonly List<string> Log     = new();

		public readonly List<InitiativeEntry> Initiative = new();
		public readonly Random Rng = new();

		public event Action<string>? OnLog;

		public void AddLog(string msg)
		{
			if (string.IsNullOrWhiteSpace(msg)) return;
			Log.Add(msg);
			if (Log.Count > 200) Log.RemoveRange(0, Log.Count - 200);
			OnLog?.Invoke(msg);
		}

		public void AddHero(Hero h)  { if (h == null) return; Players.Add(h);  AddLog($"Added hero: {h.Name} [{h.ClassId}]"); }
		public void AddPlayer(Hero h) => AddHero(h);
		public void AddEnemy(Enemy e) { if (e == null) return; Enemies.Add(e); AddLog($"Spawned enemy: {e.Name} (T{e.Tier})"); }

		public void ClearEncounter()
		{
			Enemies.Clear();
			Initiative.Clear();
			AddLog("Encounter cleared.");
		}

		public int RollD20() => Rng.Next(1, 21);
		public Spell RandomT1Spell() => Spells.Tier1Pool[Rng.Next(Spells.Tier1Pool.Count)];
		public Spell RandomT2Spell() => Spells.Tier2Pool[Rng.Next(Spells.Tier2Pool.Count)];

		public Spell RollUpgradeCandidate(Spell current)
		{
			if (current == null || current.Tier <= 1) return RandomT2Spell();
			return current;
		}

		public void TickAllEnemyStatuses()
		{
			foreach (var e in Enemies)
			{
				if (e.Hp <= 0) continue;

				if (e.PoisonStacks > 0)
				{
					e.Hp = Math.Max(0, e.Hp - e.PoisonStacks);
					if (Rng.Next(6) == 0) e.PoisonStacks = Math.Max(0, e.PoisonStacks - 1);
				}

				if (e.BombStacks > 0)
				{
					int dmg = 3 * e.BombStacks;
					e.Hp = Math.Max(0, e.Hp - dmg);
					e.BombStacks = 0;
				}
			}
		}

		public List<InitiativeEntry> BuildInitiativeDefault()
		{
			Initiative.Clear();
			foreach (var h in Players)
			{
				var r = RollD20();
				Initiative.Add(new InitiativeEntry(h.Id, false, r));
				AddLog($"{h.Name} (P) rolled initiative: {r}");
			}
			foreach (var e in Enemies)
			{
				var r = RollD20();
				Initiative.Add(new InitiativeEntry(e.Id, true, r));
				AddLog($"{e.Name} (E) rolled initiative: {r}");
			}
			Initiative.Sort((a,b) => {
				int cmp = b.Roll.CompareTo(a.Roll);
				if (cmp != 0) return cmp;
				if (a.IsEnemy != b.IsEnemy) return a.IsEnemy ? 1 : -1; // players first on tie
				return string.Compare(a.Id, b.Id, StringComparison.Ordinal);
			});
			return Initiative;
		}
	}

	public struct InitiativeEntry
	{
		public string Id { get; }
		public bool IsEnemy { get; }
		public int Roll { get; }

		public InitiativeEntry(string id, bool isEnemy, int roll)
		{
			Id = id; IsEnemy = isEnemy; Roll = roll;
		}

		public override string ToString() => $"{(IsEnemy ? "E" : "P")}:{Id}@{Roll}";
	}
}
