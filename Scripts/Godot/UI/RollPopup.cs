// res://Scripts/Godot/UI/RollPopup.cs
#nullable enable
using Godot;

namespace DiceArena.Godot
{
	public partial class RollPopup : Control
	{
		[Export] public Label       Text { get; set; } = default!;
		[Export] public TextureRect Icon { get; set; } = default!;

		public override void _Ready()
		{
			Text ??= GetNodeOrNull<Label>("Panel/HBoxContainer/Text")!;
			Icon ??= GetNodeOrNull<TextureRect>("Panel/HBoxContainer/Icon")!;
			MouseFilter = MouseFilterEnum.Ignore;
			Visible = true;
			Modulate = Colors.White;
		}

		public void ShowRoll(int value, string die, Texture2D? icon, bool crit, bool fail, Vector2 screenPos)
		{
			// Start a little above the anchor center.
			Position = screenPos - (Size * 0.5f) + new Vector2(0, -24);

			if (Text != null)
				Text.Text = crit ? $"{value} CRIT!" : fail ? $"{value} FAIL" : $"{value}";

			if (Icon != null)
			{
				Icon.Texture = icon;
				Icon.Visible = icon != null;
			}

			if (crit) Modulate = new Color(1f, 1f, 0.35f);
			if (fail) Modulate = new Color(1f, 0.4f, 0.4f);

			var endPos = Position + new Vector2(0, -48);

			var tween = CreateTween();
			tween.TweenProperty(this, "position", endPos, 0.6)
				 .SetTrans(Tween.TransitionType.Cubic)
				 .SetEase(Tween.EaseType.Out);
			tween.Parallel().TweenProperty(this, "modulate:a", 0.0f, 0.6);
			tween.Finished += QueueFree;
		}
	}
}
