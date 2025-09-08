// res://Scripts/Godot/EnemyCard.cs
#nullable enable
using Godot;
using DiceArena.Engine;

namespace DiceArena.GodotUI
{
	/// <summary>
	/// Visual card for an Enemy. Call Bind(enemy) once, then Refresh() when stats change.
	/// </summary>
	public partial class EnemyCard : PanelContainer
	{
		// Bound data
		public Enemy Data { get; private set; } = default!;

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
				BgColor = new Color(0.16f, 0.10f, 0.10f, 1f), // muted maroon
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
				Text = "Enemy",
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
			_hp.AddThemeColorOverride("font_color", new Color("FF6347")); // tomato
			_statsRow.AddChild(_hp);

			_armor = new Label { Text = "ARM: --" };
			_armor.AddThemeFontSizeOverride("font_size", 16);
			_armor.AddThemeColorOverride("font_color", new Color("FFD700")); // gold
			_statsRow.AddChild(_armor);
		}

		public void Bind(Enemy enemy)
		{
			Data = enemy;
			Refresh();
		}

		public void Refresh()
		{
			if (Data == null) return;

			_title.Text = $"{Data.Name}  (T{Data.Tier})";
			_hp.Text    = $"HP: {Data.Hp}/{Data.MaxHp}";
			_armor.Text = $"ARM: {Data.Armor}";

			// Dim card if defeated
			var isDown = Data.Hp <= 0;
			Modulate = isDown ? new Color(1, 1, 1, 0.45f) : Colors.White;
		}

		/// <summary>Optional: call to make the card pop for the current target.</summary>
		public void SetHighlighted(bool on)
		{
			Scale = on ? new Vector2(1.04f, 1.04f) : Vector2.One;
		}
	}
}
