using Godot;

namespace DiceArena.GodotUI
{
	/// <summary>Simple enemy card with icon and stat labels.</summary>
	public partial class EnemyCard : VBoxContainer
	{
		private Label _title = default!;
		private TextureRect _icon = default!;
		private Label _stats = default!;

		public override void _Ready()
		{
			AddThemeConstantOverride("separation", 6);

			_title = new Label { Text = "Enemy" };
			AddChild(_title);

			_icon = new TextureRect
			{
				CustomMinimumSize = new Vector2(320, 220),
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical = SizeFlags.ExpandFill
			};
			AddChild(_icon);

			_stats = new Label { Text = "HP / ARM" };
			AddChild(_stats);
		}

		public void Bind(string name, int tier, int hp, int hpMax, int armor, Texture2D icon)
		{
			_title.Text = $"{name} (T{tier})";
			_icon.Texture = icon;
			_stats.Text = $"HP {hp}/{hpMax}   ARM {armor}";
		}
	}
}
