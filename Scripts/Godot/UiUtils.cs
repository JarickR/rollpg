// res://Scripts/Godot/UiUtils.cs
using Godot;

namespace DiceArena.GodotUI
{
	/// <summary>
	/// Small UI helpers for building consistent Godot 4 controls.
	/// </summary>
	public static class UiUtils
	{
		/// <summary>
		/// Creates a label with optional font size and bold-ish white tint.
		/// </summary>
		public static Label MakeLabel(string text, int fontSize = 14, bool bold = false, bool wrap = true)
		{
			var l = new Label
			{
				Text = text,
				AutowrapMode = wrap ? TextServer.AutowrapMode.Word : TextServer.AutowrapMode.Off,
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};
			l.AddThemeFontSizeOverride("font_size", fontSize);
			if (bold)
				l.AddThemeColorOverride("font_color", Colors.White);
			return l;
		}

		/// <summary>
		/// Creates a simple titled panel: a Panel with a VBox (title + content).
		/// </summary>
		public static Panel MakeTitledPanel(string title, Control content, int titleSize = 16)
		{
			var root = new Panel
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical   = Control.SizeFlags.ShrinkCenter
			};

			var vb = new VBoxContainer
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical   = Control.SizeFlags.ShrinkCenter
			};
			vb.AddThemeConstantOverride("separation", 6);
			root.AddChild(vb);

			var head = new HBoxContainer
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};
			head.AddChild(MakeLabel(title, titleSize, bold: true, wrap: false));
			vb.AddChild(head);

			// Ensure the passed content expands horizontally
			content.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			vb.AddChild(content);

			return root;
		}

		/// <summary>
		/// A compact HP bar (TextureProgressBar) with no percent text.
		/// Use theme to color (e.g., via a StyleBox), or set textures as you prefer.
		/// </summary>
		public static TextureProgressBar MakeHpBar(int max = 20, int value = 20, Vector2? minSize = null)
		{
			var bar = new TextureProgressBar
			{
				MinValue = 0.0,
				MaxValue = max,
				Value    = Mathf.Clamp(value, 0, max),
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical   = Control.SizeFlags.ShrinkCenter,
				CustomMinimumSize   = minSize ?? new Vector2(160, 18)
			};

			// Godot 4 note:
			// - TextureProgressBar does not draw text by default (no ShowPercentage).
			// - If you want text, overlay a Label on top in your UI code.

			// Optional: If you want a flat look without textures, consider using ProgressBar instead
			// and styling via theme. This helper stays with TextureProgressBar per existing usage.

			return bar;
		}

		/// <summary>
		/// Simple horizontal spacer that expands to fill free space.
		/// </summary>
		public static Control HSpacer()
		{
			return new Control
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical   = Control.SizeFlags.ShrinkCenter
			};
		}

		/// <summary>
		/// Vertical stack container with consistent separation.
		/// </summary>
		public static VBoxContainer VBox(int separation = 8)
		{
			var vb = new VBoxContainer
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical   = Control.SizeFlags.ShrinkCenter
			};
			vb.AddThemeConstantOverride("separation", separation);
			return vb;
		}

		/// <summary>
		/// Horizontal stack container with consistent separation.
		/// </summary>
		public static HBoxContainer HBox(int separation = 8)
		{
			var hb = new HBoxContainer
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical   = Control.SizeFlags.ShrinkCenter
			};
			hb.AddThemeConstantOverride("separation", separation);
			return hb;
		}
	}
}
