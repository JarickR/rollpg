// Scripts/Engine/Loadout/IconTile.cs
using Godot;

namespace DiceArena.Engine.Loadout
{
	public partial class IconTile : Button
	{
		/// <summary>Default square icon size used by the loadout screen.</summary>
		public static int IconSize = 48;

		private string _tipHeader = "";
		private string _tipBody = "";

		public static IconTile Create(int size, Texture2D? tex, string header, string body)
		{
			var b = new IconTile
			{
				// Button look/feel
				Flat = false,
				ToggleMode = false,
				FocusMode = FocusModeEnum.None,
				ClipText = false,

				// Layout
				SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
				SizeFlagsVertical = SizeFlags.ShrinkCenter,
				CustomMinimumSize = new Vector2(size, size),

				// Icon rendering
				ExpandIcon = true,
				IconAlignment = HorizontalAlignment.Center
			};

			b.Icon = tex;
			b.SetTooltip(header, body);
			return b;
		}

		/// <summary>Sets both the simple TooltipText (fallback) and the richer custom tooltip.</summary>
		public void SetTooltip(string header, string body)
		{
			_tipHeader = header ?? "";
			_tipBody = body ?? "";

			// Fallback tooltip if custom tooltip can't be created.
			TooltipText = string.IsNullOrWhiteSpace(_tipBody) ? _tipHeader : $"{_tipHeader}\n\n{_tipBody}";
		}

		// Godot calls this to build the popup Control that follows the mouse.
		public override Control _MakeCustomTooltip(string forText)
		{
			// Panel wrapper
			var panel = new PanelContainer
			{
				MouseFilter = Control.MouseFilterEnum.Ignore, // don't capture/steal input
				FocusMode = FocusModeEnum.None
			};

			// Root container for content
			var root = new VBoxContainer
			{
				MouseFilter = Control.MouseFilterEnum.Ignore,
				SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
				CustomMinimumSize = new Vector2(0, 0)
			};

			// Header label
			if (!string.IsNullOrWhiteSpace(_tipHeader))
			{
				var header = new Label
				{
					Text = _tipHeader,
					AutowrapMode = TextServer.AutowrapMode.Word,
					SizeFlagsHorizontal = SizeFlags.ExpandFill
				};

				// Slightly larger & clearer without custom fonts
				header.AddThemeFontSizeOverride("font_size", (int)(header.GetThemeDefaultFontSize() * 1.05f));
				header.AddThemeColorOverride("font_color", Colors.White);

				root.AddChild(header);
				root.AddChild(new HSeparator { MouseFilter = Control.MouseFilterEnum.Ignore });
			}

			// Body label
			if (!string.IsNullOrWhiteSpace(_tipBody))
			{
				var body = new Label
				{
					Text = _tipBody,
					AutowrapMode = TextServer.AutowrapMode.Word,
					SizeFlagsHorizontal = SizeFlags.ExpandFill,
					// Cap width so long lines wrap instead of stretching the popup
					CustomMinimumSize = new Vector2(220, 0)
				};
				root.AddChild(body);
			}

			panel.AddChild(root);

			// Small insets for breathing room
			panel.AddThemeConstantOverride("margin_left", 8);
			panel.AddThemeConstantOverride("margin_right", 8);
			panel.AddThemeConstantOverride("margin_top", 6);
			panel.AddThemeConstantOverride("margin_bottom", 6);

			return panel;
		}
	}
}
