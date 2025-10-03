using Godot;

namespace DiceArena.Godot
{
	/// <summary>
	/// Small UI helpers. No reliance on nonexistent TextureButton properties.
	/// </summary>
	public static class IconHelpers
	{
		/// <summary>
		/// Stamp a texture on a TextureButton and make it size nicely inside containers.
		/// </summary>
		public static void Stamp(TextureButton btn, Texture2D tex, string? tooltip = null, int minSize = 64)
		{
			if (btn == null || tex == null) return;

			btn.TextureNormal   = tex;
			btn.TexturePressed  = tex;
			btn.TextureHover    = tex;
			btn.TextureDisabled = tex;
			btn.TextureFocused  = tex;

			// Layout-friendly sizing (no 'Expand' property on TextureButton in Godot 4)
			btn.CustomMinimumSize   = new Vector2(minSize, minSize);
			btn.StretchMode         = TextureButton.StretchModeEnum.KeepAspectCovered;
			btn.SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
			btn.SizeFlagsVertical   = Control.SizeFlags.Expand | Control.SizeFlags.Fill;

			if (!string.IsNullOrEmpty(tooltip))
				btn.TooltipText = tooltip!;
		}

		public static void Clear(TextureButton btn)
		{
			if (btn == null) return;
			btn.TextureNormal = DiceArena.Godot.IconLibrary.Transparent1x1;
		}
	}
}
