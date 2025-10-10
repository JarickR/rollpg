// res://Scripts/Godot/BattleRoot.cs
#nullable enable
using Godot;
using System.Reflection;
using EngineGame = DiceArena.Engine.Core.Game;
using DiceArena.Engine;   // Hero
using RollPG.GodotUI;    // HeroCard

namespace DiceArena.Godot
{
	public partial class BattleRoot : Control
	{
		// --- Inspector paths --------------------------------------------------
		[Export] public PackedScene HeroCardScene { get; set; } = default!;
		[Export] public NodePath FriendlyRootPath { get; set; } = new NodePath("HSplitContainer/LeftCol/HeroArea");
		[Export] public NodePath EnemyRootPath    { get; set; } = new NodePath("HSplitContainer/LeftCol/EnemyPanel");
		[Export] public NodePath LogRootPath      { get; set; } = new NodePath("HSplitContainer/LeftCol/LogBox/RichTextLabel");
		[Export] public NodePath TemplateHeroCardPath { get; set; } = new NodePath("HeroCard");

		// --- Cached nodes -----------------------------------------------------
		private Control? _friendlyRoot;
		private Control? _enemyRoot;
		private RichTextLabel? _log;

		public override void _Ready()
		{
			EnsureLayout();
			HideTemplateCardIfPresent();

			_friendlyRoot = GetNodeOrNull<Control>(FriendlyRootPath);
			_enemyRoot    = GetNodeOrNull<Control>(EnemyRootPath);
			_log          = GetNodeOrNull<RichTextLabel>(LogRootPath);

			if (_friendlyRoot == null) GD.PushWarning("[BattleRoot] FriendlyRoot not found.");
			if (_enemyRoot    == null) GD.PushWarning("[BattleRoot] EnemyRoot not found.");
			if (_log          == null) GD.PushWarning("[BattleRoot] Log label not found.");

			// Clear any template / test children so the surface is clean
			if (_friendlyRoot != null) ClearChildren(_friendlyRoot);
			if (_enemyRoot    != null) ClearChildren(_enemyRoot);

			GD.Print("[BattleRoot] Layout ensured, template hidden, hero/enemy areas cleared.");
		}

		private void EnsureLayout()
		{
			// BattleRoot fills viewport
			SetFullRect(this);

			// Split fills BattleRoot
			var split = GetNodeOrNull<HSplitContainer>("HSplitContainer");
			if (split != null)
			{
				SetFullRect(split);
				split.SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
				split.SizeFlagsVertical   = Control.SizeFlags.Expand | Control.SizeFlags.Fill;

				var leftCol = split.GetNodeOrNull<Control>("LeftCol");
				if (leftCol != null)
				{
					SetFullRect(leftCol);
					leftCol.SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
					leftCol.SizeFlagsVertical   = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
				}
			}

			// Key areas can grow
			var logBox   = GetNodeOrNull<Control>("HSplitContainer/LeftCol/LogBox");
			var heroArea = GetNodeOrNull<Control>("HSplitContainer/LeftCol/HeroArea");
			var enemyPan = GetNodeOrNull<Control>("HSplitContainer/LeftCol/EnemyPanel");

			if (logBox != null)
			{
				logBox.SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
				logBox.SizeFlagsVertical   = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
				logBox.CustomMinimumSize   = new Vector2(420, 120);
			}

			if (heroArea != null)
			{
				heroArea.SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
				heroArea.SizeFlagsVertical   = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
				heroArea.CustomMinimumSize   = new Vector2(420, 160);
			}

			if (enemyPan != null)
			{
				enemyPan.SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
				enemyPan.SizeFlagsVertical   = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
				enemyPan.CustomMinimumSize   = new Vector2(420, 120);
			}
		}

		private static void SetFullRect(Control c)
		{
			// Equivalent to Layout → Full Rect
			c.AnchorLeft = 0f;  c.AnchorTop = 0f;  c.AnchorRight = 1f;  c.AnchorBottom = 1f;
			c.OffsetLeft = 0f;  c.OffsetTop = 0f;  c.OffsetRight = 0f;  c.OffsetBottom = 0f;
			c.SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
			c.SizeFlagsVertical   = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
		}

		private void HideTemplateCardIfPresent()
		{
			var stray = GetNodeOrNull<Control>(TemplateHeroCardPath);
			if (stray != null)
			{
				stray.Visible = false;
				stray.ProcessMode = ProcessModeEnum.Disabled;
			}
		}

		/// <summary>
		/// Populate UI from an engine Game instance (optional hook).
		/// </summary>
		public void ApplyFromGame(EngineGame game)
		{
			if (game == null)
			{
				GD.PushWarning("[BattleRoot] ApplyFromGame called with null game.");
				return;
			}

			if (_friendlyRoot != null) ClearChildren(_friendlyRoot);
			if (_enemyRoot    != null) ClearChildren(_enemyRoot);

			if (_friendlyRoot != null && HeroCardScene != null)
			{
				foreach (var member in game.Members)
				{
					var hero = ExtractHero(member);
					if (hero == null)
					{
						GD.PushWarning("[BattleRoot] Skipped member: could not extract Hero.");
						continue;
					}

					var cardInstance = HeroCardScene.Instantiate<HeroCard>();
					cardInstance.SetHero(hero);
					_friendlyRoot.AddChild(cardInstance);
				}
			}

			LogInfo("[Battle] UI hydrated from EngineGame.");
		}

		private static Hero? ExtractHero(object? member)
		{
			if (member is Hero h) return h;

			if (member != null)
			{
				try
				{
					var p = member.GetType().GetProperty("Hero", BindingFlags.Instance | BindingFlags.Public);
					if (p != null && p.GetValue(member) is Hero hx) return hx;
				}
				catch { /* ignore */ }
			}
			return null;
		}

		// --- Logging helpers --------------------------------------------------
		public void ClearLog()
		{
			if (_log != null) _log.Clear();
		}

		public void AppendLog(string message)
		{
			LogInfo(message);
		}

		private void LogInfo(string msg)
		{
			if (_log != null) _log.AppendText($"{msg}\n");
		}

		private void LogWarn(string msg)
		{
			if (_log != null) _log.AppendText($"[color=yellow]{msg}[/color]\n");
		}

		private void LogError(string msg)
		{
			if (_log != null) _log.AppendText($"[color=red]{msg}[/color]\n");
		}

		// --- Utility ----------------------------------------------------------
		private static void ClearChildren(Node parent)
		{
			for (int i = parent.GetChildCount() - 1; i >= 0; i--)
				parent.GetChild(i).QueueFree();
		}
	}
}
