using Godot;
using DiceArena.GodotUI;

namespace DiceArena.GodotApp
{
	/// <summary>
	/// Root game controller. Spawns heroes and enemies, logs actions,
	/// and manages the scene-level UI panels.
	/// </summary>
	public partial class Game : Control
	{
		private BattleLogPanel _logPanel = default!;

		public override void _Ready()
		{
			// Try unique-name lookup first (%BattleLogPanel is the shorthand).
			_logPanel =
				GetNodeOrNull<BattleLogPanel>("%BattleLogPanel") ??
				GetNodeOrNull<BattleLogPanel>("BattleLogPanel") ??
				null;

			// If the scene didn’t include one, create a temporary panel
			// so we never null-ref (useful for dev/testing).
			if (_logPanel == null)
			{
				_logPanel = new BattleLogPanel();
				AddChild(_logPanel);
			}

			_logPanel.Log("=== Game initialized ===");

			// Demo spawns (replace with your real logic).
			SpawnEnemies(3);
			SpawnTestHero();
		}

		/// <summary>
		/// Spawn a given number of test enemies.
		/// Replace with your real combat/enemy manager later.
		/// </summary>
		private void SpawnEnemies(int count)
		{
			for (int i = 0; i < count; i++)
			{
				// Example: log each spawn
				_logPanel?.Log($"Spawned enemy Goblin {i + 1}");
			}
		}

		/// <summary>
		/// Spawn a test hero (placeholder).
		/// Replace with your real hero management logic.
		/// </summary>
		private void SpawnTestHero()
		{
			_logPanel?.Log("Spawned test hero: Paladin with 4 spells.");
		}
	}
}
