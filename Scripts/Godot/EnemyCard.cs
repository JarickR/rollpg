using Godot;
using DiceArena.Godot;  // gives access to IconLibrary

namespace DiceArena.GodotUI
{
	/// <summary>
	/// Simple enemy card: title, small stat line, and an icon scaled to fit.
	/// </summary>
	public partial class EnemyCard : VBoxContainer
	{
		private Label _title = default!;
		private Label _stats = default!;
		private TextureRect _icon = default!;

		public override void _Ready()
		{
			AddThemeConstantOverride("separation", 6);

			_title = new Label { Text = "Enemy" };
			AddChild(_title);

			_icon = new TextureRect
			{
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				CustomMinimumSize = new Vector2(180, 180),
				SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
				SizeFlagsVertical = SizeFlags.ShrinkCenter,
			};
			AddChild(_icon);

			_stats = new Label { Text = "HP 0/0  ARM 0" };
			AddChild(_stats);
		}

		private void EnsureBuilt()
		{
			// If nodes weren’t built (rare), build them.
			if (_title == null || _icon == null || _stats == null)
				_Ready();
		}

		/// <summary>Populate this card.</summary>
		public void Bind(string name, int tier, int hp, int hpMax, int armor, Texture2D icon)
		{
			EnsureBuilt();

			_title!.Text = $"{name} (T{tier})";
			_icon!.Texture = icon ?? IconLibrary.Transparent1x1;
			_stats!.Text = $"HP {hp}/{hpMax}  ARM {armor}";
		}
	}
}
