// res://Scripts/Godot/HeroCard.cs
#nullable enable
using Godot;
using System.Collections.Generic;
using DiceArena.Engine;

namespace DiceArena.GodotUI
{
	public partial class HeroCard : Panel
	{
		public Hero Data { get; private set; } = default!;

		private VBoxContainer _root = default!;
		private HBoxContainer _header = default!;
		private TextureRect _classIcon = default!;
		private Label _title = default!;
		private Label _hp = default!;
		private Label _armor = default!;

		private HBoxContainer _statusRow = default!;
		private TextureRect _thornIcon = default!;

		// Faces row (4 spell icons from the hero's die)
		private HBoxContainer _facesRow = default!;
		private readonly List<TextureRect> _faceIcons = new();

		public override void _Ready()
		{
			// Prevent any children from drawing outside the card
			ClipContents = true;

			CustomMinimumSize = new Vector2(260, 200); // compact
			AddThemeConstantOverride("margin_left", 8);
			AddThemeConstantOverride("margin_right", 8);
			AddThemeConstantOverride("margin_top", 8);
			AddThemeConstantOverride("margin_bottom", 8);
			AddThemeStyleboxOverride("panel", MakePanel(new Color(0.09f, 0.11f, 0.17f, 1f)));

			_root = new VBoxContainer
			{
				// Do not expand vertically; stay as compact as possible.
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical   = SizeFlags.ShrinkCenter
			};
			_root.AddThemeConstantOverride("separation", 10);
			AddChild(_root);

			// ===== Header (icon + title stack) =====
			_header = new HBoxContainer
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical   = SizeFlags.ShrinkCenter
			};
			_header.AddThemeConstantOverride("separation", 10);

			_classIcon = new TextureRect
			{
				CustomMinimumSize = new Vector2(48, 48),
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				// keep the given size (don't expand)
				ExpandMode  = TextureRect.ExpandModeEnum.KeepSize,
				SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
				SizeFlagsVertical   = SizeFlags.ShrinkCenter
			};
			_header.AddChild(_classIcon);

			var titleBox = new VBoxContainer
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical   = SizeFlags.ShrinkCenter
			};
			titleBox.AddThemeConstantOverride("separation", 2);

			_title = new Label { AutowrapMode = TextServer.AutowrapMode.Off };
			_title.AddThemeFontSizeOverride("font_size", 18);
			_title.AddThemeColorOverride("font_color", Colors.White);
			titleBox.AddChild(_title);

			var stats = new HBoxContainer
			{
				SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
				SizeFlagsVertical   = SizeFlags.ShrinkCenter
			};
			stats.AddThemeConstantOverride("separation", 12);

			_hp = new Label();    _hp.AddThemeColorOverride("font_color", new Color("7CFF3C"));
			_armor = new Label(); _armor.AddThemeColorOverride("font_color", new Color("3CC0FF"));
			stats.AddChild(_hp);
			stats.AddChild(_armor);
			titleBox.AddChild(stats);

			_header.AddChild(titleBox);
			_root.AddChild(_header);

			// ===== Faces row in compact panel (locked size, clipped) =====
			var facesPanel = new PanelContainer
			{
				CustomMinimumSize = new Vector2(0, 80),
				SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
				SizeFlagsVertical   = SizeFlags.ShrinkCenter,
				ClipContents        = true
			};
			var facesBox = new StyleBoxFlat
			{
				BgColor = new Color(0.18f, 0.18f, 0.18f, 0.92f),
				CornerRadiusTopLeft = 6,
				CornerRadiusTopRight = 6,
				CornerRadiusBottomLeft = 6,
				CornerRadiusBottomRight = 6
			};
			facesPanel.AddThemeStyleboxOverride("panel", facesBox);

			_facesRow = new HBoxContainer
			{
				Alignment = BoxContainer.AlignmentMode.Center,
				SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
				SizeFlagsVertical   = SizeFlags.ShrinkCenter
			};
			_facesRow.AddThemeConstantOverride("separation", 12);

			// 4 fixed-size slots, each with a centered 48x48 icon
			for (int i = 0; i < 4; i++)
			{
				var slotPanel = new PanelContainer
				{
					CustomMinimumSize = new Vector2(64, 64),
					SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
					SizeFlagsVertical   = SizeFlags.ShrinkCenter,
					ClipContents        = true
				};
				var slotBox = new StyleBoxFlat
				{
					BgColor = new Color(0.08f, 0.08f, 0.08f, 0.85f),
					CornerRadiusTopLeft = 4,
					CornerRadiusTopRight = 4,
					CornerRadiusBottomLeft = 4,
					CornerRadiusBottomRight = 4
				};
				slotPanel.AddThemeStyleboxOverride("panel", slotBox);

				var face = new TextureRect
				{
					CustomMinimumSize = new Vector2(48, 48),
					StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
					ExpandMode  = TextureRect.ExpandModeEnum.KeepSize,
					SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
					SizeFlagsVertical   = SizeFlags.ShrinkCenter
				};

				slotPanel.AddChild(face);
				_facesRow.AddChild(slotPanel);
				_faceIcons.Add(face);
			}

			facesPanel.AddChild(_facesRow);
			_root.AddChild(facesPanel);

			// ===== Status row (e.g., Thorns) =====
			_statusRow = new HBoxContainer
			{
				SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
				SizeFlagsVertical   = SizeFlags.ShrinkCenter
			};
			_statusRow.AddThemeConstantOverride("separation", 6);

			_thornIcon = new TextureRect
			{
				CustomMinimumSize = new Vector2(32, 32),
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				ExpandMode  = TextureRect.ExpandModeEnum.KeepSize,
				Visible = false,
				SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
				SizeFlagsVertical   = SizeFlags.ShrinkCenter
			};
			_statusRow.AddChild(_thornIcon);

			_root.AddChild(_statusRow);
		}

		private static StyleBoxFlat MakePanel(Color bg)
		{
			var sb = new StyleBoxFlat
			{
				BgColor = bg,
				CornerRadiusTopLeft = 10,
				CornerRadiusTopRight = 10,
				CornerRadiusBottomLeft = 10,
				CornerRadiusBottomRight = 10
			};
			sb.BorderWidthLeft = sb.BorderWidthRight = sb.BorderWidthTop = sb.BorderWidthBottom = 2;
			sb.BorderColor = new Color(0.15f, 0.02f, 0.06f, 1f);
			return sb;
		}

		public void Bind(Hero hero)
		{
			Data = hero;

			_title.Text = $"{hero.Name} [{hero.ClassId}]";
			_hp.Text = $"HP: {hero.Hp}/{hero.MaxHp}";
			_armor.Text = $"ARM: {hero.Armor}";

			_classIcon.Texture = IconLibrary.GetClassLogo(ClassIndexFromId(hero.ClassId));
			_thornIcon.Texture ??= IconLibrary.GetThornIcon();

			Refresh();
		}

		public void Refresh()
		{
			if (Data == null) return;

			_hp.Text   = $"HP: {Data.Hp}/{Data.MaxHp}";
			_armor.Text = $"ARM: {Data.Armor}";

			_thornIcon.Visible = Data.SpikedThorns > 0;
			if (_thornIcon.Visible)
				_thornIcon.TooltipText = $"Thorns: {Data.SpikedThorns}";

			for (int i = 0; i < _faceIcons.Count; i++)
			{
				Texture2D tex;
				string tip;

				if (Data.Loadout != null && i < Data.Loadout.Count && Data.Loadout[i] != null)
				{
					tex = IconLibrary.GetSpellIcon(Data.Loadout[i]);
					tip = Data.Loadout[i].Name;
				}
				else
				{
					tex = IconLibrary.GetDefensiveLogo();
					tip = "Defensive action";
				}

				_faceIcons[i].Texture = tex;
				_faceIcons[i].TooltipText = tip;
			}
		}

		private static int ClassIndexFromId(string id)
		{
			if (string.IsNullOrEmpty(id)) return 0;
			id = id.ToLowerInvariant();

			var map = new Dictionary<string, int>
			{
				["king"] = 0,
				["judge"] = 1,
				["bard"] = 2,
				["paladin"] = 3,
				["assassin"] = 4,
				["priest"] = 5,
				["necromancer"] = 6,
				["druid"] = 7,
				["ranger"] = 8,
				["thief"] = 9,
				["barbarian"] = 9 // temporary fallback
			};

			return map.TryGetValue(id, out var idx) ? idx : 0;
		}
	}
}
