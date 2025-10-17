// res://Scripts/Godot/BattleHUD.cs
#nullable enable
using Godot;
using System.Collections.Generic;
using DiceArena.GodotUI;

namespace RollPG.GodotUI
{
	/// <summary>
	/// UI-only battle HUD that you attach to the HSplitContainer under BattleRoot.
	/// It paints your top-left hero block, two enemy blocks, loadout grid, and log box.
	/// Call ShowHero/ShowEnemies from your Game/BattleRoot scripts.
	/// </summary>
	public partial class BattleHUD : Node
	{
		// ---------------- Node paths (defaults match your scene tree under HSplitContainer) ----------------
		[Export] public NodePath HeroNameLabelPath { get; set; } = "LeftCol/HeroPanel/Name";
		[Export] public NodePath HeroClassIconPath { get; set; } = "LeftCol/HeroPanel/ClassIcon";
		[Export] public NodePath HeroT1IconPath    { get; set; } = "LeftCol/HeroPanel/T1Icon";
		[Export] public NodePath HeroT2IconPath    { get; set; } = "LeftCol/HeroPanel/T2Icon";
		[Export] public NodePath HeroStatusPath    { get; set; } = "LeftCol/HeroPanel/Flex";      // status icon row
		[Export] public NodePath HeroAreaPath      { get; set; } = "LeftCol/HeroArea";            // loadout icon grid

		[Export] public NodePath EnemyPanelPath    { get; set; } = "LeftCol/EnemyPanel";
		[Export] public NodePath LogBoxPath        { get; set; } = "LeftCol/LogBox";

		// Adjust these two if your Clear/Roll buttons have different names:
		[Export] public NodePath ClearLogBtnPath   { get; set; } = "LeftCol/Buttons/Main_ButtonsRow#UpgradeBtn";
		[Export] public NodePath RollBtnPath       { get; set; } = "LeftCol/Buttons/Main_ButtonsRow#D6Btn";

		[Export] public int LoadoutSlotCount { get; set; } = 6;

		// --- cached node refs ---
		private Label _heroName = null!;
		private TextureRect _heroClass = null!;
		private TextureRect _heroT1 = null!;
		private TextureRect _heroT2 = null!;
		private Control _heroStatus = null!;
		private Control _heroArea = null!;
		private Control _enemyPanel = null!;
		private Node _logBox = null!;
		private Button _rollBtn = null!;
		private Button _clearBtn = null!;

		public override void _Ready()
		{
			// Resolve nodes relative to the HSplitContainer (this)
			_heroName   = GetNode<Label>(HeroNameLabelPath);
			_heroClass  = GetNode<TextureRect>(HeroClassIconPath);
			_heroT1     = GetNode<TextureRect>(HeroT1IconPath);
			_heroT2     = GetNode<TextureRect>(HeroT2IconPath);
			_heroStatus = GetNode<Control>(HeroStatusPath);
			_heroArea   = GetNode<Control>(HeroAreaPath);
			_enemyPanel = GetNode<Control>(EnemyPanelPath);
			_logBox     = GetNode(LogBoxPath);                 // ← fix: no FromString in Godot C# 4.x
			_rollBtn    = GetNode<Button>(RollBtnPath);
			_clearBtn   = GetNode<Button>(ClearLogBtnPath);

			_rollBtn.Pressed += () => AppendLog("[Roll] d6 pressed");
			_clearBtn.Pressed += ClearLog;

			EnsureHeroAreaSlots();
		}

		// ------------------------------------ HERO ------------------------------------

		/// <summary>Paints the top-left hero block (icon trio, status row, loadout grid, and a log note).</summary>
		public void ShowHero(
			string name,
			int hp, int maxHp,
			int armor,
			Texture2D? classIcon,
			Texture2D? tier1Icon,
			Texture2D? tier2Icon,
			IReadOnlyList<Texture2D>? statusIcons = null,
			IReadOnlyList<Texture2D>? loadoutIcons = null)
		{
			_heroName.Text = name;

			if (classIcon != null) _heroClass.Texture = classIcon;
			if (tier1Icon != null) _heroT1.Texture    = tier1Icon;
			if (tier2Icon != null) _heroT2.Texture    = tier2Icon;

			// Status row: rebuild
			foreach (var c in _heroStatus.GetChildren()) c.QueueFree();
			if (statusIcons != null)
			{
				foreach (var tex in statusIcons)
				{
					var t = new TextureRect
					{
						Texture = tex,
						StretchMode = TextureRect.StretchModeEnum.KeepCentered,
						CustomMinimumSize = new Vector2(24, 24)
					};
					_heroStatus.AddChild(t);
				}
			}

			// Loadout grid: ensure slots exist, then fill left-to-right
			EnsureHeroAreaSlots();
			if (loadoutIcons != null)
			{
				int i = 0;
				foreach (var child in _heroArea.GetChildren())
				{
					if (child is TextureRect tr)
					{
						tr.Texture = (i < loadoutIcons.Count) ? loadoutIcons[i] : tr.Texture; // keep default otherwise
						i++;
					}
				}
			}

			AppendLog($"[HUD] {name} HP {hp}/{maxHp} ARM {armor}");
		}

		private void EnsureHeroAreaSlots()
		{
			var current = _heroArea.GetChildren().Count;
			if (current >= LoadoutSlotCount) return;

			for (int i = current; i < LoadoutSlotCount; i++)
			{
				var tr = new TextureRect
				{
					StretchMode = TextureRect.StretchModeEnum.KeepCentered,
					CustomMinimumSize = new Vector2(48, 48)
				};
				_heroArea.AddChild(tr);
			}
		}

		// ----------------------------------- ENEMIES -----------------------------------

		/// <summary>Shows enemy boxes: name + HP/ARM mini bars + status icons.</summary>
		public void ShowEnemies(IReadOnlyList<(string name, int hp, int maxHp, int armor, IReadOnlyList<Texture2D>? status)> enemies)
		{
			foreach (var c in _enemyPanel.GetChildren()) c.QueueFree();

			foreach (var e in enemies)
			{
				var vb = new VBoxContainer { CustomMinimumSize = new Vector2(280, 72) };

				var name = new Label { Text = e.name };
				var hpBar = new ProgressBar { MaxValue = e.maxHp <= 0 ? 1 : e.maxHp, Value = Mathf.Clamp(e.hp, 0, e.maxHp <= 0 ? 1 : e.maxHp) };
				hpBar.CustomMinimumSize = new Vector2(240, 10);

				var armBar = new ProgressBar { MaxValue = 20, Value = Mathf.Clamp(e.armor, 0, 20) };
				armBar.AddThemeColorOverride("fg_color", new Color(0.7f, 0.8f, 1f));
				armBar.CustomMinimumSize = new Vector2(240, 6);

				vb.AddChild(name);
				vb.AddChild(hpBar);
				vb.AddChild(armBar);

				var row = new HBoxContainer();
				if (e.status != null)
				{
					foreach (var tex in e.status)
						row.AddChild(new TextureRect { Texture = tex, CustomMinimumSize = new Vector2(20, 20) });
				}
				vb.AddChild(row);

				_enemyPanel.AddChild(vb);
			}
		}

		// ------------------------------------- LOG -------------------------------------

		public void AppendLog(string line)
		{
			switch (_logBox)
			{
				case RichTextLabel rtl:
					rtl.SafeAppendLine(line + "\n");
					break;
				case TextEdit te:
					te.Text += line + "\n";
					break;
				case Label l:
					l.Text += line + "\n";
					break;
			}
		}

		public void ClearLog()
		{
			switch (_logBox)
			{
				case RichTextLabel rtl:
					rtl.Clear();
					break;
				case TextEdit te:
					te.Text = "";
					break;
				case Label l:
					l.Text = "";
					break;
			}
		}
	}
}
