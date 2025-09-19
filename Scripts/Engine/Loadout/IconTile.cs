using Godot;

namespace DiceArena.Engine.Loadout
{
	/// <summary>
	/// Small square button that shows an icon and supports highlighted (selected) state.
	/// </summary>
	public partial class IconTile : Button
	{
		// Keep tiles a consistent square.
		public const int Tile = 32;

		public IconTile()
		{
			ToggleMode = true;
			FocusMode = FocusModeEnum.None;
			Flat = true;
			ExpandIcon = true;                 // scale icon to fill
			Text = string.Empty;               // icon-only
			CustomMinimumSize = new Vector2(Tile, Tile);
			SizeFlagsHorizontal = (int)(SizeFlags.Expand | SizeFlags.Fill);
			SizeFlagsVertical   = (int)(SizeFlags.Expand | SizeFlags.Fill);

			Toggled += OnToggled;
		}

		public void SetTexture(Texture2D? tex)
		{
			Icon = tex;
		}

		public void SetTooltip(string text)
		{
			TooltipText = text ?? string.Empty;
		}

		private void OnToggled(bool pressed)
		{
			// Simple highlight: thicker border when pressed.
			var sb = new StyleBoxFlat
			{
				BgColor = Colors.Transparent,
				CornerRadiusTopLeft = 6,
				CornerRadiusTopRight = 6,
				CornerRadiusBottomLeft = 6,
				CornerRadiusBottomRight = 6
			};
			if (pressed)
			{
				sb.BorderColor = new Color("ffd84a");
				sb.BorderWidthTop = sb.BorderWidthBottom = sb.BorderWidthLeft = sb.BorderWidthRight = 2;
			}
			else
			{
				sb.BorderColor = new Color(1,1,1,0.1f);
				sb.BorderWidthTop = sb.BorderWidthBottom = sb.BorderWidthLeft = sb.BorderWidthRight = 1;
			}

			AddThemeStyleboxOverride("normal", sb);
			AddThemeStyleboxOverride("hover", sb);
			AddThemeStyleboxOverride("pressed", sb);
			AddThemeStyleboxOverride("focus", sb);
		}
	}
}
