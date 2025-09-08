// res://Scripts/Godot/RollPopup.cs
#nullable enable
using Godot;

namespace DiceArena.GodotUI
{
	/// <summary>
	/// Lightweight popup banner that shows short messages like "Rolled 6".
	/// </summary>
	public partial class RollPopup : PanelContainer
	{
		private Label _label = default!;

		public override void _Ready()
		{
			Name = "RollPopup";
			AnchorLeft = 0; AnchorTop = 0; AnchorRight = 0; AnchorBottom = 0;
			OffsetLeft = 8; OffsetTop = 8;

			// Panel style
			var style = new StyleBoxFlat
			{
				BgColor = new Color(0, 0, 0, 0.55f),
				CornerRadiusTopLeft = 6,
				CornerRadiusTopRight = 6,
				CornerRadiusBottomLeft = 6,
				CornerRadiusBottomRight = 6,
				ContentMarginLeft = 10,
				ContentMarginRight = 10,
				ContentMarginTop = 8,
				ContentMarginBottom = 8
			};
			AddThemeStyleboxOverride("panel", style);

			_label = new Label
			{
				Text = "Rolling...",
				HorizontalAlignment = HorizontalAlignment.Left,
				SizeFlagsHorizontal = SizeFlags.ShrinkBegin,
			};
			_label.AddThemeFontSizeOverride("font_size", 18);
			_label.AddThemeColorOverride("font_color", Colors.White);
			AddChild(_label);

			Visible = true;
		}

		public void ShowText(string text)
		{
			_label.Text = text;
			// Simple "flash": briefly show opaque, then fade
			Modulate = Colors.White;
			Show();
			// You can add a tween fade here if desired
		}
	}
}
