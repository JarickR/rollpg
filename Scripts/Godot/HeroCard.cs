// res://Scripts/Godot/HeroCard.cs
#nullable enable
using Godot;
using DiceArena.Engine;

namespace DiceArena.GodotUI
{
	/// <summary>
	/// Visual card for a Hero. Call Bind(hero) once, then Refresh() when stats change.
	/// </summary>
	public partial class HeroCard : PanelContainer
	{
		// Bound data
		public Hero Data { get; private set; } = default!;

		// UI
		private VBoxContainer _root = default!;
		private Label _title = default!;
		private HBoxContainer _statsRow = default!;
		private Label _hp = default!;
		private Label _armor = default!;

		public override void _Ready()
		{
			BuildUi();
		}

		private void BuildUi()
		{
			// Card panel style
			var panel = new StyleBoxFlat
			{
				BgColor = new Color(0.12f, 0.14f, 0.18f, 1f), // deep gray-blue
				CornerRadiusTopLeft = 10,
				CornerRadiusTopRight = 10,
				CornerRadiusBottomLeft = 10,
				CornerRadiusBottomRight = 10,
				ContentMarginLeft = 12,
				ContentMarginRight = 12,
				ContentMarginTop = 10,
				ContentMarginBottom = 10
			};
			AddThemeStyleboxOverride("panel", panel);

			CustomMinimumSize = new Vector2(190, 110);

			_root = new VBoxContainer
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical = SizeFlags.ShrinkCenter
			};
			_root.AddThemeConstantOverride("separation", 8);
			AddChild(_root);

			_title = new Label
			{
				Text = "Hero",
				HorizontalAlignment = HorizontalAlignment.Center,
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				ClipText = true
			};
			_title.AddThemeFontSizeOverride("font_size", 18);
			_title.AddThemeColorOverride("font_color", Colors.White);
			_root.AddChild(_title);

			_statsRow = new HBoxContainer();
			_statsRow.AddThemeConstantOverride("separation", 12);
			_root.AddChild(_statsRow);

			_hp = new Label { Text = "HP: --/--" };
			_hp.AddThemeFontSizeOverride("font_size", 16);
			_hp.AddThemeColorOverride("font_color", new Color("7CFC00")); // lawn green
			_statsRow.AddChild(_hp);

			_armor = new Label { Text = "ARM: --" };
			_armor.AddThemeFontSizeOverride("font_size", 16);
			_armor.AddThemeColorOverride("font_color", new Color("87CEFA")); // light sky blue
			_statsRow.AddChild(_armor);
		}

		public void Bind(Hero hero)
		{
			Data = hero;
			Refresh();
		}

		public void Refresh()
		{
			if (Data == null) return;

			_title.Text = $"{Data.Name}  [{Data.ClassId}]";
			_hp.Text    = $"HP: {Data.Hp}/{Data.MaxHp}";
			_armor.Text = $"ARM: {Data.Armor}";

			// Dim card if downed
			var isDown = Data.Hp <= 0;
			Modulate = isDown ? new Color(1, 1, 1, 0.45f) : Colors.White;
		}

		/// <summary>Optional: call to make the card pop for the active hero.</summary>
		public void SetHighlighted(bool on)
		{
			Scale = on ? new Vector2(1.04f, 1.04f) : Vector2.One;
		}
	}
}
