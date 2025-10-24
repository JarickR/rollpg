#nullable enable
using Godot;
using System;
using System.Threading.Tasks;

namespace DiceArena.Godot
{
	public partial class DiceRollPopup : Control
	{
		[Export] public TextureRect? Face { get; set; }
		[Export] public Label? ValueLabel { get; set; }

		[Export] public int SpinCycles { get; set; } = 10;
		[Export] public float StartSpeed { get; set; } = 0.06f;
		[Export] public float EndSpeed   { get; set; } = 0.12f;
		[Export] public float LiftPixels { get; set; } = -48f;
		[Export] public float LifeSeconds { get; set; } = 0.8f;

		private readonly RandomNumberGenerator _rng = new();
		private Texture2D[] _faces = Array.Empty<Texture2D>();

		public override void _Ready()
		{
			MouseFilter = MouseFilterEnum.Ignore;
			TopLevel = true;
			ZIndex = 200;

			// Autowire if not assigned
			Face ??= GetNodeOrNull<TextureRect>("Panel/HBoxContainer/Face");
			ValueLabel ??= GetNodeOrNull<Label>("Panel/HBoxContainer/ValueLabel");

			// Safety: ensure it has a visible footprint even before a texture
			if (Face != null && Face.CustomMinimumSize == Vector2.Zero)
				Face.CustomMinimumSize = new Vector2(96, 96);
			if (Face != null)
				Face.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;

			Scale = Vector2.One;
			Modulate = Colors.White;
			if (ValueLabel != null) ValueLabel.Visible = false;
		}

		public async void PlayRoll(int finalValue, Vector2 screenPos, Texture2D[]? facesOverride = null)
		{
			_faces = (facesOverride != null && facesOverride.Length >= 1)
				? facesOverride
				: Array.Empty<Texture2D>();

			Position = screenPos - (Size * 0.5f) + new Vector2(0, -24);

			int clamped = Math.Clamp(finalValue, 1, 6);
			int current = Mathf.Clamp(_rng.RandiRange(1, 6), 1, 6);

			// If we have at least one face, show something immediately
			if (_faces.Length > 0 && Face != null)
				Face.Texture = _faces[Mathf.Clamp(current - 1, 0, _faces.Length - 1)];

			// Float up
			var drift = CreateTween();
			drift.TweenProperty(this, "position",
				Position + new Vector2(0, LiftPixels),
				StartSpeed * SpinCycles + EndSpeed + LifeSeconds)
				.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);

			// Spin even if faces fewer than 6 (we’ll clamp index safely)
			for (int i = 0; i < SpinCycles; i++)
			{
				float t = SpinCycles <= 1 ? 1f : i / (float)(SpinCycles - 1);
				float step = Mathf.Lerp(StartSpeed, EndSpeed, t);

				int next;
				do { next = _rng.RandiRange(1, 6); } while (next == current);
				await FlipTo(next, step);
				current = next;
			}

			await FlipTo(clamped, EndSpeed);

			if (ValueLabel != null)
			{
				ValueLabel.Text = clamped.ToString();
				ValueLabel.Visible = true;
			}

			var fade = CreateTween();
			fade.TweenProperty(this, "modulate:a", 0f, LifeSeconds);
			await ToSignal(fade, Tween.SignalName.Finished);
			QueueFree();
		}

		private async Task FlipTo(int faceValue, float stepTime)
		{
			var flip = CreateTween();
			flip.TweenProperty(this, "scale:x", 0.0f, stepTime * 0.5f)
				.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.In);
			await ToSignal(flip, Tween.SignalName.Finished);

			if (Face != null && _faces.Length > 0)
				Face.Texture = _faces[Mathf.Clamp(faceValue - 1, 0, _faces.Length - 1)];

			var unflip = CreateTween();
			unflip.TweenProperty(this, "scale:x", 1.0f, stepTime * 0.5f)
				  .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
			await ToSignal(unflip, Tween.SignalName.Finished);
		}
	}
}
