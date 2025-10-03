// Scripts/Godot/BattleRoot.cs
using Godot;
using EngineGame = DiceArena.Engine.Core.Game;

namespace DiceArena.Godot
{
	public partial class BattleRoot : Control
	{
		// --- Inspector paths --------------------------------------------------
		[Export] public PackedScene HeroCardScene { get; set; } = default!;
		[Export] public NodePath FriendlyRootPath { get; set; } = new NodePath();
		[Export] public NodePath EnemyRootPath    { get; set; } = new NodePath();
		[Export] public NodePath LogRootPath      { get; set; } = new NodePath();

		// --- Cached nodes -----------------------------------------------------
		private Control? _friendlyRoot;
		private Control? _enemyRoot;
		private RichTextLabel? _log;

		public override void _Ready()
		{
			// Resolve nodes (null-safe; works even if paths are empty)
			_friendlyRoot = GetNodeOrNull<Control>(FriendlyRootPath);
			_enemyRoot    = GetNodeOrNull<Control>(EnemyRootPath);
			_log          = GetNodeOrNull<RichTextLabel>(LogRootPath);

			// Enable BBCode so we can colorize log lines
			if (_log != null)
				_log.BbcodeEnabled = true;

			var friendlyMissing = _friendlyRoot == null;
			var enemyMissing    = _enemyRoot == null;
			var logMissing      = _log == null;

			if (friendlyMissing || enemyMissing || logMissing)
				LogWarn($"[Battle] Missing roots: friendly? {friendlyMissing}, enemy? {enemyMissing}, log? {logMissing}");
			else
				LogInfo("[Battle] BattleRoot ready.");

			if (HeroCardScene == null)
			{
				LogError("[Battle] HeroCardScene is not set in the inspector!");
			}

			if (_friendlyRoot != null)
				_friendlyRoot.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			if (_enemyRoot != null)
				_enemyRoot.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		}

		public void ApplyFromGame(EngineGame game)
		{
			if (_friendlyRoot != null)
				ClearChildren(_friendlyRoot);
			if (_enemyRoot != null)
				ClearChildren(_enemyRoot);

			LogInfo("[Battle] Applying state from Engine.Core.Game...");

			if (HeroCardScene == null)
			{
				LogError("[Battle] Cannot instantiate heroes, HeroCardScene is null.");
				return;
			}
			
			if (game.Members.Count == 0)
			{
				LogWarn("[Battle] Game state received, but it contains no party members.");
			}

			// --- Hydrate Hero Cards ---
			foreach (var member in game.Members)
			{
				var cardInstance = HeroCardScene.Instantiate<HeroCard>();
				cardInstance.SetHero(member);
				_friendlyRoot?.AddChild(cardInstance);
			}

			LogInfo($"[Battle] Instantiated {game.Members.Count} hero cards. State applied.");
		}

		public void ApplyFromGame(DiceArena.Godot.Game game)
		{
			LogInfo("[Battle] ApplyFromGame(Godot.Game) called. (No-op placeholder)");
		}

		// --- Utility ----------------------------------------------------------
		public void ClearLog()
		{
			if (_log == null) return;
			_log.Clear();
		}

		public void AppendLog(string message) => LogInfo(message);

		private static void ClearChildren(Node parent)
		{
			foreach (var child in parent.GetChildren())
			{
				child.QueueFree();
			}
		}

		// --- Logging helpers (BBCode) ----------------------------------------
		private void LogInfo(string msg)
		{
			if (_log == null) return;
			_log.AppendText(msg + "\n");
		}

		private void LogWarn(string msg)
		{
			if (_log == null) return;
			_log.AppendText($"[color=yellow]{msg}[/color]\n");
		}

		private void LogError(string msg)
		{
			if (_log == null) return;
			_log.AppendText($"[color=red]{msg}[/color]\n");
		}
	}
}
