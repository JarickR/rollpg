// Scripts/Godot/HoverBubble.cs
using Godot;

namespace DiceArena.GodotUI
{
	/// <summary>
	/// Hover popup drawn on its own CanvasLayer above UI.
	/// Use ShowBubbleNear(text, anchorRect) to anchor to a control.
	/// Opaque dark tooltip with rounded corners, border, shadow, width cap.
	/// </summary>
	public partial class HoverBubble : Panel
	{
		private static CanvasLayer? _layer; // shared per SceneTree
		private RichTextLabel _label = default!;

		private const float MaxWidth = 420f; // wrap long text
		private const float MinWidth = 220f; // pleasant minimum
		private const int LayerOrder = 100;  // above normal UI

		public HoverBubble()
		{
			Name = "HoverBubble";
			TopLevel = true;
			Visible = false;
			MouseFilter = MouseFilterEnum.Ignore;
			ZIndex = 10000;

			// Opaque dark gray background + subtle border & shadow
			var bg = new StyleBoxFlat
			{
				BgColor = new Color(0.10f, 0.10f, 0.10f, 1f),
				BorderColor = new Color(1, 1, 1, 0.55f),
				CornerRadiusTopLeft = 6,
				CornerRadiusTopRight = 6,
				CornerRadiusBottomLeft = 6,
				CornerRadiusBottomRight = 6,
				ShadowColor = new Color(0, 0, 0, 0.5f),
				ShadowSize = 6,
				ContentMarginLeft = 12,
				ContentMarginRight = 12,
				ContentMarginTop = 8,
				ContentMarginBottom = 8
			};
			bg.BorderWidthLeft = 2;
			bg.BorderWidthRight = 2;
			bg.BorderWidthTop = 2;
			bg.BorderWidthBottom = 2;

			AddThemeStyleboxOverride("panel", bg);
		}

		public override void _Ready()
		{
			_label = new RichTextLabel
			{
				FitContent = true,
				AutowrapMode = TextServer.AutowrapMode.WordSmart,
				ClipContents = false,
				SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
				SizeFlagsVertical = SizeFlags.ShrinkCenter,
				BbcodeEnabled = true
			};
			_label.AddThemeColorOverride("default_color", Colors.White);
			AddChild(_label);
		}

		/// <summary>
		/// Ensure a high CanvasLayer exists and parent this bubble to it.
		/// </summary>
		public void AttachToScene(SceneTree tree)
		{
			if (_layer == null || !_layer.IsInsideTree())
			{
				_layer = new CanvasLayer { Layer = LayerOrder, Name = "HoverBubbleLayer" };
				tree.Root.AddChild(_layer);
			}
			if (GetParent() != _layer)
				_layer.AddChild(this);
		}

		/// <summary>
		/// Old mouse-follow version (kept for fallback/debug).
		/// </summary>
		public void ShowBubble(string text, Vector2 screenPos)
		{
			_label.Text = text ?? "";
			_label.CustomMinimumSize = new Vector2(MinWidth, 0);
			_label.Size = new Vector2(MaxWidth, 0);

			var sb = GetThemeStylebox("panel");
			float padX = sb.ContentMarginLeft + sb.ContentMarginRight;
			float padY = sb.ContentMarginTop + sb.ContentMarginBottom;

			var labelMin = _label.GetMinimumSize();
			Size = new Vector2(Mathf.Clamp(labelMin.X, MinWidth, MaxWidth) + padX,
							   labelMin.Y + padY);

			var vp = GetViewport().GetVisibleRect().Size;
			var pos = screenPos + new Vector2(16, 16);
			pos.X = Mathf.Clamp(pos.X, 0, Mathf.Max(0, vp.X - Size.X));
			pos.Y = Mathf.Clamp(pos.Y, 0, Mathf.Max(0, vp.Y - Size.Y));

			Position = pos;
			Visible = true;
		}

		/// <summary>
		/// Anchor the bubble to a control: prefer below-left; flip if out of bounds.
		/// Pass the control's GetGlobalRect().
		/// </summary>
		public void ShowBubbleNear(string text, Rect2 anchorRect)
		{
			_label.Text = text ?? "";
			_label.CustomMinimumSize = new Vector2(MinWidth, 0);
			_label.Size = new Vector2(MaxWidth, 0);

			var sb = GetThemeStylebox("panel");
			float padX = sb.ContentMarginLeft + sb.ContentMarginRight;
			float padY = sb.ContentMarginTop + sb.ContentMarginBottom;

			var labelMin = _label.GetMinimumSize();
			Size = new Vector2(Mathf.Clamp(labelMin.X, MinWidth, MaxWidth) + padX,
							   labelMin.Y + padY);

			var vp = GetViewport().GetVisibleRect().Size;

			// Start below-left of the control with a small gap
			var pos = new Vector2(anchorRect.Position.X,
								  anchorRect.Position.Y + anchorRect.Size.Y + 6);

			// If overflowing right, shift to keep inside
			if (pos.X + Size.X > vp.X)
				pos.X = Mathf.Max(0, anchorRect.End.X - Size.X);

			// If overflowing bottom, place above the control
			if (pos.Y + Size.Y > vp.Y)
				pos.Y = Mathf.Max(0, anchorRect.Position.Y - Size.Y - 6);

			Position = pos;
			Visible = true;
		}

		public void HideBubble() => Visible = false;
	}
}
