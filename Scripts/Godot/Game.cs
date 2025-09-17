// Scripts/Godot/Game.cs
using Godot;
using System;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using DiceArena.Engine.Content;  // ClassDef / SpellDef models
using DiceArena.GodotUI;         // LoadoutScreen

namespace DiceArena.GodotApp
{
	public partial class Game : Control
	{
		// Assign these in the editor (drag the nodes onto these fields on the Game node)
		[Export] public NodePath? LoadoutScreenPath { get; set; }
		[Export] public NodePath? BattleRootPath { get; set; }

		private DiceArena.GodotUI.LoadoutScreen? _loadout;
		private Node? _battleRoot;

		private enum GameMode { Loadout, Battle }
		private GameMode _mode = GameMode.Loadout;

		public override void _Ready()
		{
			// Grab nodes
			_loadout    = GetNodeOrNull<DiceArena.GodotUI.LoadoutScreen>(LoadoutScreenPath);
			_battleRoot = GetNodeOrNull<Node>(BattleRootPath);

			if (!GodotObject.IsInstanceValid(_loadout))
			{
				GD.PushError("Game: LoadoutScreenPath not assigned / node missing.");
				return;
			}
			if (!GodotObject.IsInstanceValid(_battleRoot))
			{
				GD.PushWarning("Game: BattleRootPath not assigned / node missing. Only loadout will show.");
			}

			// Make both screens fill the window (prevents overlay offsets)
			if (_loadout is Control lc) FillParent(lc);
			if (_battleRoot is Control bc) FillParent(bc);

			// ---- Load JSON directly and inject into loadout ----
			var classes = LoadJsonList<ClassDef>("res://Content/classes.json");
			var spells  = LoadJsonList<SpellDef>("res://Content/spells.json");

			var tier1 = spells.Where(s => s != null && s.Tier == 1).ToList();
			var tier2 = spells.Where(s => s != null && s.Tier == 2).ToList();

			GD.Print($"[Game] Injecting content → classes={classes.Count}, t1={tier1.Count}, t2={tier2.Count}");
			_loadout.InjectContent(classes, tier1, tier2);
			_loadout.Build(); // builds cards based on PartySizeSpin / DefaultPartySize

			// Start in Loadout mode (hide Battle so they don't collide)
			SwitchToLoadout();
		}

		public override void _UnhandledInput(InputEvent e)
		{
			if (e is InputEventKey k && k.Pressed && !k.Echo)
			{
				if (k.Keycode == Key.F1) SwitchToLoadout(); // quick toggle for testing
				if (k.Keycode == Key.F2) SwitchToBattle();
			}
		}

		// ---------- Mode switching ----------
		public void SwitchToLoadout()
		{
			_mode = GameMode.Loadout;

			if (_loadout is CanvasItem li)
			{
				li.Visible = true;
				li.ZIndex = 10;
			}
			if (_battleRoot is CanvasItem bi)
			{
				bi.Visible = false;
				bi.ZIndex = 0;
			}
		}

		public void SwitchToBattle()
		{
			_mode = GameMode.Battle;

			if (_loadout is CanvasItem li)
			{
				li.Visible = false;
				li.ZIndex = 0;
			}
			if (_battleRoot is CanvasItem bi)
			{
				bi.Visible = true;
				bi.ZIndex = 10;
			}
		}

		// ---------- Helpers ----------
		private static void FillParent(Control c)
		{
			c.AnchorLeft = 0;  c.AnchorTop = 0;  c.AnchorRight = 1;  c.AnchorBottom = 1;
			c.OffsetLeft = 0;  c.OffsetTop = 0;  c.OffsetRight = 0;  c.OffsetBottom = 0;
			c.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			c.SizeFlagsVertical   = Control.SizeFlags.ExpandFill;
		}

		private static List<T> LoadJsonList<T>(string resPath)
		{
			try
			{
				using var fa = FileAccess.Open(resPath, FileAccess.ModeFlags.Read);
				if (fa == null)
				{
					GD.PushError($"[Game] File open failed: {resPath}");
					return new();
				}

				var json = fa.GetAsText();
				var opts = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
					ReadCommentHandling = JsonCommentHandling.Skip,
					AllowTrailingCommas = true
				};
				var list = JsonSerializer.Deserialize<List<T>>(json, opts);
				return list ?? new();
			}
			catch (Exception ex)
			{
				GD.PushError($"[Game] Load error for {resPath}: {ex.Message}");
				return new();
			}
		}
	}
}
