// res://Scripts/Godot/EnemyCard.cs
#nullable enable
using Godot;
using DiceArena.Engine;

namespace DiceArena.GodotUI
{
	public partial class EnemyCard : Panel
	{
		public Enemy Data { get; private set; } = default!;

		private VBoxContainer _root = default!;
		private HBoxContainer _header = default!;
		private TextureRect _portrait = default!;
		private Label _title = default!;
		private Label _hp = default!;
		private Label _armor = default!;

		private HBoxContainer _statusRow = default!;
		private TextureRect _poisonIcon = default!;
		private TextureRect _bombIcon = default!;
		private TextureRect _bleedIcon = default!;
		private TextureRect _ghoulIcon = default!;

		public string SheetTier { get; set; } = "tier1"; // "tier1" | "tier2" | "boss"
		public int SheetIndex { get; set; } = 0;         // 0..19

		public override void _Ready()
		{
			CustomMinimumSize = new Vector2(260, 180);
			AddThemeConstantOverride("margin_left", 8);
			AddThemeConstantOverride("margin_right", 8);
			AddThemeConstantOverride("margin_top", 8);
			AddThemeConstantOverride("margin_bottom", 8);
			AddThemeStyleboxOverride("panel", MakePanel(new Color(0.05f, 0.08f, 0.12f, 1f)));

			_root = new VBoxContainer(); _root.AddThemeConstantOverride("separation", 6);
			AddChild(_root);

			_header = new HBoxContainer(); _header.AddThemeConstantOverride("separation", 8);

			_portrait = new TextureRect
			{
				CustomMinimumSize = new Vector2(96, 96),
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional
			};
			_header.AddChild(_portrait);

			_title = new Label(); _title.AddThemeFontSizeOverride("font_size", 18);
			_title.AddThemeColorOverride("font_color", Colors.White);
			_header.AddChild(_title);

			_root.AddChild(_header);

			var stats = new HBoxContainer(); stats.AddThemeConstantOverride("separation", 12);
			_hp = new Label(); _hp.AddThemeColorOverride("font_color", new Color("7CFF3C"));
			_armor = new Label(); _armor.AddThemeColorOverride("font_color", new Color("3CC0FF"));
			stats.AddChild(_hp); stats.AddChild(_armor);
			_root.AddChild(stats);

			_statusRow = new HBoxContainer(); _statusRow.AddThemeConstantOverride("separation", 6);

			_poisonIcon = MakeSmallIcon();
			_bombIcon   = MakeSmallIcon();
			_bleedIcon  = MakeSmallIcon();
			_ghoulIcon  = MakeSmallIcon();

			_statusRow.AddChild(_poisonIcon);
			_statusRow.AddChild(_bombIcon);
			_statusRow.AddChild(_bleedIcon);
			_statusRow.AddChild(_ghoulIcon);

			_root.AddChild(_statusRow);
		}

		private static TextureRect MakeSmallIcon() => new TextureRect
		{
			CustomMinimumSize = new Vector2(48, 48),
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional,
			Visible = false
		};

		private static StyleBoxFlat MakePanel(Color bg)
		{
			var sb = new StyleBoxFlat { BgColor = bg, CornerRadiusTopLeft = 10, CornerRadiusTopRight = 10, CornerRadiusBottomLeft = 10, CornerRadiusBottomRight = 10 };
			sb.BorderWidthLeft = sb.BorderWidthRight = sb.BorderWidthTop = sb.BorderWidthBottom = 2;
			sb.BorderColor = new Color(0.12f, 0.03f, 0.04f, 1f);
			return sb;
		}

		public void Bind(Enemy enemy)
		{
			Data = enemy;

			_title.Text = $"{enemy.Name} (T{enemy.Tier})";
			_hp.Text = $"HP {enemy.Hp}/{enemy.MaxHp}";
			_armor.Text = $"ARM {enemy.Armor}";

			_portrait.Texture = IconLibrary.GetEnemyFrame(SheetTier, SheetIndex);

			_poisonIcon.Texture ??= IconLibrary.GetPoisonIcon();
			_bombIcon.Texture   ??= IconLibrary.GetBombIcon();
			_bleedIcon.Texture  ??= IconLibrary.GetBleedIcon();
			_ghoulIcon.Texture  ??= IconLibrary.GetGhoulIcon();

			Refresh();
		}

		public void Refresh()
		{
			if (Data == null) return;

			_hp.Text = $"HP {Data.Hp}/{Data.MaxHp}";
			_armor.Text = $"ARM {Data.Armor}";

			_poisonIcon.Visible = Data.PoisonStacks > 0;
			if (_poisonIcon.Visible) _poisonIcon.TooltipText = $"Poison x{Data.PoisonStacks}";

			_bombIcon.Visible = Data.BombStacks > 0;
			if (_bombIcon.Visible) _bombIcon.TooltipText = $"Bomb x{Data.BombStacks}";

			// placeholders for when/if you add these to Enemy
			_bleedIcon.Visible = false;
			_ghoulIcon.Visible = false;
		}
	}
}
