// res://Scripts/Godot/UI/RollButtonController.cs
#nullable enable
using Godot;

namespace DiceArena.Godot
{
	/// <summary>
	/// Attach this to the RollBtn (Button) under a hero's panel.
	/// On press, it rolls and emits a RollPopup anchored to the hero panel.
	/// </summary>
	public partial class RollButtonController : Button
	{
		[ExportGroup("Connections")]
		/// <summary>Path to the RollPopupSpawner (e.g., HUDLayer/RollPopupLayer).</summary>
		[Export] public NodePath PopupSpawnerPath { get; set; } = default!;
		/// <summary>The hero panel Control to anchor the popup to (e.g., HeroPanel1).</summary>
		[Export] public NodePath AnchorControlPath { get; set; } = default!;

		[ExportGroup("Roll Settings")]
		/// <summary>Display text in the popup (e.g., "d20", "Attack").</summary>
		[Export] public string DieText { get; set; } = "d20";
		/// <summary>Number of sides for the roll.</summary>
		[Export(PropertyHint.Range, "2,100,1")] public int Sides { get; set; } = 20;
		/// <summary>Optional icon to show in the popup.</summary>
		[Export] public Texture2D? RollIconTexture { get; set; }
		/// <summary>Highlight as crit when value == Sides.</summary>
		[Export] public bool CritOnMax { get; set; } = true;
		/// <summary>Highlight as fail when value == 1.</summary>
		[Export] public bool FailOnMin { get; set; } = true;

		private RollPopupSpawner? _spawner;
		private Control? _anchor;
		private RandomNumberGenerator _rng = new();

		public override void _Ready()
		{
			// Resolve refs
			_spawner = string.IsNullOrEmpty(PopupSpawnerPath) ? null : GetNodeOrNull<RollPopupSpawner>(PopupSpawnerPath);
			_anchor  = string.IsNullOrEmpty(AnchorControlPath) ? null : GetNodeOrNull<Control>(AnchorControlPath);

			if (_spawner == null)
				GD.PushWarning("[RollButtonController] PopupSpawnerPath is not assigned or not found.");
			if (_anchor == null)
				GD.PushWarning("[RollButtonController] AnchorControlPath is not assigned or not found.");

			// Ensure button doesn't block other UI unexpectedly
			MouseFilter = MouseFilterEnum.Stop;

			// Hook the press
			Pressed += OnPressed;

			_rng.Randomize();
		}

		private void OnPressed()
		{
			if (_spawner == null || _anchor == null)
			{
				GD.PushWarning("[RollButtonController] Missing spawner or anchor; cannot emit roll popup.");
				return;
			}

			if (Sides < 2) Sides = 2;

			int roll = _rng.RandiRange(1, Sides);
			bool isCrit = CritOnMax && roll == Sides;
			bool isFail = FailOnMin && roll == 1;

			_spawner.EmitRollAtNode(_anchor, roll, DieText, Icon, isCrit, isFail);
			GD.Print($"[RollButton] {DieText} → {roll} (crit={isCrit}, fail={isFail})");
		}
	}
}
