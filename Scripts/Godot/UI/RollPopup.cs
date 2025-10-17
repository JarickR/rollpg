// res://Scripts/Godot/UI/RollPopup.cs
#nullable enable
using Godot;
using System;

namespace DiceArena.Godot
{
	/// <summary>
	/// A floating roll popup: shows die text/icon + value, tints for crit/fail,
	/// eases upward, fades out, then frees or hides itself.
	/// Hook up nodes in the Inspector; no layout is hardcoded.
	/// </summary>
	public partial class RollPopup : Control
	{
		[ExportGroup("Node Paths")]
		[Export] public NodePath ValueLabelPath { get; set; } = default!;
		[Export] public NodePath DieLabelPath   { get; set; } = default!;  // or use icon
		[Export] public NodePath IconPath       { get; set; } = default!;  // TextureRect (optional)
		[Export] public NodePath SfxPath        { get; set; } = default!;  // AudioStreamPlayer (optional)

		[ExportGroup("Behavior")]
		[Export(PropertyHint.Range, "0.2,3.0,0.05")]
		public float LifeSeconds { get; set; } = 1.25f;

		[Export(PropertyHint.Range, "10,300,1")]
		public float FloatPixels { get; set; } = 64f;

		[Export(PropertyHint.Range, "0.0,1.0,0.01")]
		public float StartOpacity { get; set; } = 1.0f;

		[Export(PropertyHint.Enum, "Free,Hide")]
		public string OnFinish { get; set; } = "Free";

		[ExportGroup("Colors")]
		[Export] public Color NormalColor { get; set; } = new Color(1, 1, 1);         // white
		[Export] public Color CritColor   { get; set; } = new Color(1, 0.95f, 0.3f);  // gold-ish
		[Export] public Color FailColor   { get; set; } = new Color(1, 0.4f, 0.4f);   // red-ish

		private Label _value = null!;
		private Label _die   = null!;
		private TextureRect? _icon;
		private AudioStreamPlayer? _sfx;

		private Vector2 _spawnPos;

		public override void _Ready()
		{
			_value = GetNode<Label>(ValueLabelPath);
			_die   = GetNode<Label>(DieLabelPath);
			_icon  = string.IsNullOrEmpty(IconPath) ? null : GetNodeOrNull<TextureRect>(IconPath);
			_sfx   = string.IsNullOrEmpty(SfxPath)  ? null : GetNodeOrNull<AudioStreamPlayer>(SfxPath);

			// Default to ignore mouse so it never blocks clicks.
			MouseFilter = MouseFilterEnum.Ignore;
			SelfModulate = new Color(SelfModulate, StartOpacity);
		}

		/// <summary>
		/// Configure and play the popup animation.
		/// </summary>
		/// <param name="value">The rolled number (e.g., 17)</param>
		/// <param name="dieText">e.g., "d20", "2d6+3", or a short label like "Attack"</param>
		/// <param name="iconTex">Optional die/class/skill icon</param>
		/// <param name="isCrit">True for crit highlight</param>
		/// <param name="isFail">True for fail highlight</param>
		/// <param name="spawnAt">Canvas position to spawn/fade from (Control coordinates)</param>
		public void ShowRoll(int value, string dieText, Texture2D? iconTex, bool isCrit, bool isFail, Vector2 spawnAt)
		{
			_spawnPos = spawnAt;

			_value.Text = value.ToString();
			_die.Text   = dieText;

			if (_icon != null)
			{
				_icon.Texture = iconTex;
				_icon.Visible = iconTex != null;
			}

			// Pick tint color priority: crit > fail > normal
			if (isCrit)    SelfModulate = new Color(CritColor, StartOpacity);
			else if (isFail) SelfModulate = new Color(FailColor, StartOpacity);
			else           SelfModulate = new Color(NormalColor, StartOpacity);

			// Place at spawn
			Position = spawnAt - (Size * 0.5f); // center popup around point

			// Optional SFX
			_sfx?.Play();

			// Animate: ease up and fade out
			var tween = CreateTween();
			tween.SetTrans(Tween.TransitionType.Cubic);
			tween.SetEase(Tween.EaseType.Out);

			var endPos = spawnAt - new Vector2(0, FloatPixels);
			tween.TweenProperty(this, "position", endPos - (Size * 0.5f), LifeSeconds);
			tween.Parallel().TweenProperty(this, "self_modulate:a", 0.0f, LifeSeconds);

			tween.Finished += () =>
			{
				if (OnFinish == "Free") QueueFree();
				else { Visible = false; ProcessMode = ProcessModeEnum.Disabled; }
			};
		}
	}
}
