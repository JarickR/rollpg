// Scripts/Godot/Game.cs
using Godot;
using System;
using System.Collections.Generic;
using DiceArena.Engine;
using DiceArena.GodotUI;

namespace DiceArena.GodotApp
{
	public partial class Game : Control
	{
		// === UI roots ===
		private VBoxContainer _rootCol = default!;
		private GridContainer _enemyRow = default!; // switched to GridContainer
		private PanelContainer _actionBand = default!;
		private PanelContainer _heroBand = default!;
		private RichTextLabel _log = default!;
		private Button _rollBtn = default!;

		// Demo data
		private readonly List<Engine.Enemy> _enemies = new();
		private Hero _hero = new Hero("P1", "P1", "barbarian");

		public override void _Ready()
		{
			Name = "Game";
			BuildUi();
			PopulateDemoData();
			RefreshEnemies();
			RefreshHeroBand();
			Log("Round 1\n[font_size=16][color=yellow]Heroes: Roll to start![/color][/font_size]");
		}

		// ------------------------------------------------------------
		// UI Construction
		// ------------------------------------------------------------
		private void BuildUi()
		{
			var h = new HSplitContainer
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical = SizeFlags.ExpandFill
			};
			AddChild(h);

			var left = new VBoxContainer
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical = SizeFlags.ExpandFill
			};
			h.AddChild(left);

			// --------------- Enemies section ---------------
			var enemiesPanel = MakeBand("Enemies");
			left.AddChild(enemiesPanel);

			_enemyRow = new GridContainer
			{
				Columns = 4, // lock to 4 per row
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical = SizeFlags.ShrinkCenter,
				CustomMinimumSize = new Vector2(0, 180)
			};
			_enemyRow.AddThemeConstantOverride("h_separation", 16);
			_enemyRow.AddThemeConstantOverride("v_separation", 16);
			enemiesPanel.AddChild(_enemyRow);

			// --------------- Action band (ROLL) ---------------
			_actionBand = MakeBand();
			left.AddChild(_actionBand);

			var actionInner = new HBoxContainer
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical = SizeFlags.ShrinkCenter
			};
			_actionBand.AddChild(actionInner);

			actionInner.AddChild(new Control
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				CustomMinimumSize = new Vector2(16, 0)
			});

			_rollBtn = new Button
			{
				Text = "ROLL",
				CustomMinimumSize = new Vector2(420, 72),
				SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
				FocusMode = FocusModeEnum.None
			};
			_rollBtn.Pressed += OnRoll;
			actionInner.AddChild(_rollBtn);

			actionInner.AddChild(new Control
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				CustomMinimumSize = new Vector2(16, 0)
			});

			// --------------- Hero band ---------------
			_heroBand = MakeBand("Hero");
			left.AddChild(_heroBand);

			// --------------- Right: Battle log ---------------
			var right = new VBoxContainer
			{
				CustomMinimumSize = new Vector2(260, 0),
				SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
				SizeFlagsVertical = SizeFlags.ExpandFill
			};
			h.AddChild(right);

			var logHeader = new HBoxContainer
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill
			};
			var logTitle = new Label { Text = "Battle Log" };
			var clearBtn = new Button { Text = "Clear", FocusMode = FocusModeEnum.None };
			clearBtn.Pressed += () => _log.Text = string.Empty;

			logHeader.AddChild(logTitle);
			logHeader.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
			logHeader.AddChild(clearBtn);
			right.AddChild(logHeader);

			var logPanel = new PanelContainer
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical = SizeFlags.ExpandFill
			};
			right.AddChild(logPanel);

			_log = new RichTextLabel
			{
				FitContent = false,
				ScrollActive = true,
				AutowrapMode = TextServer.AutowrapMode.WordSmart,
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical = SizeFlags.ExpandFill
			};
			logPanel.AddChild(_log);
		}

		private PanelContainer MakeBand(string? caption = null)
		{
			var outer = new VBoxContainer
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill
			};

			if (!string.IsNullOrEmpty(caption))
			{
				var cap = new Label
				{
					Text = caption,
					ThemeTypeVariation = "HeaderSmall"
				};
				outer.AddChild(cap);
			}

			var panel = new PanelContainer
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical = SizeFlags.ShrinkBegin,
				CustomMinimumSize = new Vector2(0, 120)
			};
			outer.AddChild(panel);

			_rootCol ??= new VBoxContainer
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical = SizeFlags.ExpandFill
			};
			if (GetChildCount() == 0 || GetChild(0) != _rootCol)
				AddChild(_rootCol);
			if (!_rootCol.IsAncestorOf(outer)) _rootCol.AddChild(outer);

			return panel;
		}

		// ------------------------------------------------------------
		// Demo population
		// ------------------------------------------------------------
		private void PopulateDemoData()
		{
			_enemies.Clear();
			for (int i = 0; i < 4; i++)
				_enemies.Add(new Engine.Enemy { Name = $"Goblin {i + 1}", Tier = 1, Hp = 10, MaxHp = 10 });
		}

		// ------------------------------------------------------------
		// Renderers
		// ------------------------------------------------------------
		private void RefreshEnemies()
		{
			_enemyRow.QueueFreeChildren();
			foreach (var e in _enemies)
				_enemyRow.AddChild(MakeEnemyCard(e));
		}

		private Control MakeEnemyCard(Engine.Enemy e)
		{
			var card = new PanelContainer
			{
				CustomMinimumSize = new Vector2(260, 140),
				SizeFlagsHorizontal = SizeFlags.ShrinkBegin
			};

			var vb = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
			card.AddChild(vb);

			var name = new Label { Text = $"{e.Name} (T{e.Tier})" };
			vb.AddChild(name);

			var stats = new HBoxContainer();
			stats.AddChild(new Label { Text = $"HP {e.Hp}/{e.MaxHp}" });
			stats.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
			stats.AddChild(new Label { Text = $"ARM {e.Armor}" });
			vb.AddChild(stats);

			return card;
		}

		private void RefreshHeroBand()
		{
			_heroBand.QueueFreeChildren();
			var row = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
			_heroBand.AddChild(row);

			var classLine = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
			classLine.AddChild(new Label { Text = "Class" });

			var classIcon = new TextureRect
			{
				ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				CustomMinimumSize = new Vector2(48, 48)
			};
			classIcon.Texture = IconLibrary.GetClassLogoByKey(_hero.ClassId);
			classLine.AddChild(classIcon);

			classLine.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
			row.AddChild(classLine);

			var faces = new HBoxContainer
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				CustomMinimumSize = new Vector2(0, 88)
			};
			row.AddChild(faces);

			var upgrade = new Button { Text = "Upgrade", CustomMinimumSize = new Vector2(128, 56) };
			upgrade.Pressed += () => Log("[i]Upgrade clicked[/i]");
			faces.AddChild(upgrade);

			var d6 = new Button { Text = "d6", CustomMinimumSize = new Vector2(88, 56) };
			d6.Pressed += () => Log("[i]Rolled a d6[/i]");
			faces.AddChild(d6);

			for (int i = 0; i < 4; i++)
			{
				var tr = new TextureRect
				{
					CustomMinimumSize = new Vector2(72, 72),
					StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
					ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize
				};

				Spell? s = null;
				if (_hero.Loadout is not null && i < _hero.Loadout.Count)
					s = _hero.Loadout[i];

				tr.Texture = s == null
					? null
					: IconLibrary.GetSpellIconByName(s.Name);

				faces.AddChild(tr);
			}
		}

		// ------------------------------------------------------------
		// Interactions
		// ------------------------------------------------------------
		private void OnRoll()
		{
			Log("[b]ROLL[/b] (stub)");
		}

		private void Log(string bbcode)
		{
			if (string.IsNullOrWhiteSpace(bbcode)) return;
			if (_log.Text.Length > 0) _log.AppendText("\n");
			_log.AppendText(bbcode);
			_log.ScrollToLine(_log.GetLineCount() - 1);
		}
	}

	internal static class NodeExtensions
	{
		public static void QueueFreeChildren(this Node node)
		{
			foreach (var child in node.GetChildren())
				(child as Node)?.QueueFree();
		}
	}
}
