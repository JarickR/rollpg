using System;

namespace DiceArena.Engine
{
	public sealed class CombatSystem
	{
		private readonly GameState _gs;
		private readonly Random _rng = new();

		public CombatSystem(GameState gs) { _gs = gs ?? throw new ArgumentNullException(nameof(gs)); }

		public void StepTurn()
		{
			var turn = _gs.GetActiveTurn();
			if (turn == null) { _gs.PushLog("No initiative!"); return; }

			if (turn.Id.StartsWith("P"))
			{
				int pIndex = ParseIndex(turn.Id);
				_gs.PushLog($"It’s P{pIndex + 1}’s turn. (Roll in UI)");
				// UI should call _gs.ResolvePlayerRoll(pIndex, face)
			}
			else if (turn.Id.StartsWith("E"))
			{
				int eIndex = ParseIndex(turn.Id);
				DoEnemyAction(eIndex);
				_gs.NextTurn();
			}
		}

		private void DoEnemyAction(int enemyIndex)
		{
			if (enemyIndex < 0 || enemyIndex >= _gs.Enemies.Count) return;
			if (_gs.Players.Count == 0) return;

			int target = _rng.Next(_gs.Players.Count);
			int dmg = _gs.Enemies[enemyIndex].IsBoss ? 4 : 2;

			_gs.ApplyDamageToPlayer(target, dmg, bypassArmor:false);
			_gs.PushLog($"{_gs.Enemies[enemyIndex].Name} attacks P{target+1} for {dmg}.");
		}

		private int ParseIndex(string id) // "P3" -> 2
		{
			if (id.Length < 2) return -1;
			var tail = id.Substring(1);
			return int.TryParse(tail, out var n) ? (n - 1) : -1;
		}
	}
}
