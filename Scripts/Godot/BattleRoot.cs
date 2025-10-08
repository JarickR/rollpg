// Scripts/Godot/BattleRoot.cs
using Godot;
using System;
using System.Reflection;
using EngineGame = DiceArena.Engine.Core.Game;
using DiceArena.Engine; // for Hero

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
			_friendlyRoot = GetNodeOrNull<Control>(FriendlyRootPath);
			_enemyRoot    = GetNodeOrNull<Control>(EnemyRootPath);
			_log          = GetNodeOrNull<RichTextLabel>(LogRootPath);

			if (_friendlyRoot == null) GD.PushWarning("[BattleRoot] FriendlyRoot not found.");
			if (_enemyRoot    == null) GD.PushWarning("[BattleRoot] EnemyRoot not found.");
			if (_log          == null) GD.PushWarning("[BattleRoot] LogRoot not found.");
		}

		/// <summary>
		/// Populate UI from an engine Game instance.
		/// Expects game.Members and game.Enemies collections.
		/// Each friendly member should either be a DiceArena.Engine.Hero OR have a .Hero property.
		/// </summary>
		public void ApplyFromGame(EngineGame game)
		{
			if (game == null)
			{
				GD.PushWarning("[BattleRoot] ApplyFromGame called with null game.");
				return;
			}

			// Clear existing cards
			if (_friendlyRoot != null) ClearChildren(_friendlyRoot);
			if (_enemyRoot != null)    ClearChildren(_enemyRoot);

			// --- Hydrate Hero Cards (friends) ---
			if (_friendlyRoot != null && HeroCardScene != null)
			{
				foreach (var member in game.Members)
				{
					var hero = ExtractHero(member);
					if (hero == null) {
						GD.PushWarning("[BattleRoot] Skipped member: could not extract Hero.");
						continue;
					}

					var cardInstance = HeroCardScene.Instantiate<HeroCard>();
					cardInstance.SetHero(hero);
					_friendlyRoot.AddChild(cardInstance);
				}
			}

			// (Optional) Enemies if you have an enemy card scene:
			// if (_enemyRoot != null && EnemyCardScene != null)
			// foreach (var enemy in game.Enemies) { ... }

			LogInfo("[Battle] UI hydrated from EngineGame.");
		}

		/// <summary>
		/// Try to get a DiceArena.Engine.Hero from member (either cast directly or via a Hero property).
		/// </summary>
		private static Hero? ExtractHero(object? member)
		{
			// 1) Already a Hero
			if (member is Hero h) return h;

			// 2) Look for a public 'Hero' property
			if (member != null)
			{
				try
				{
					PropertyInfo? p = member.GetType().GetProperty("Hero", BindingFlags.Instance | BindingFlags.Public);
					if (p != null)
					{
						var value = p.GetValue(member) as Hero;
						if (value != null) return value;
					}
				}
				catch (Exception) { /* ignore */ }
			}
			return null;
		}

		// --- Logging helpers --------------------------------------------------
		public void ClearLog()
		{
			if (_log == null) return;
			_log.Clear();
		}

		public void AppendLog(string message) => LogInfo(message);

		private void LogInfo(string msg)
		{
			if (_log == null) return;
			_log.AppendText($"{msg}\n");
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

		// --- Utility ----------------------------------------------------------
		private static void ClearChildren(Node parent)
		{
			// QueueFree existing children
			for (int i = parent.GetChildCount() - 1; i >= 0; i--)
			{
				parent.GetChild(i).QueueFree();
			}
		}
	}
}
