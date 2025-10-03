using Godot;

/// <summary>
/// Tiny bridge that swaps visibility from the Loadout screen to the Battle root
/// when the Finalize button is pressed. No dependency on LoadoutScreen types.
/// Wire the three exported paths in the Inspector.
/// </summary>
public partial class LoadoutToBattleBridge : Node
{
	[Export] public NodePath LoadoutScreenPath { get; set; } = default!;
	[Export] public NodePath BattleRootPath    { get; set; } = default!;
	[Export] public NodePath FinalizeButtonPath { get; set; } = default!;

	private CanvasItem? _loadout;   // Control/CanvasLayer/anything deriving from CanvasItem
	private CanvasItem? _battleRoot;
	private Button? _finalizeBtn;

	public override void _Ready()
	{
		_loadout    = GetNodeOrNull<CanvasItem>(LoadoutScreenPath);
		_battleRoot = GetNodeOrNull<CanvasItem>(BattleRootPath);
		_finalizeBtn = GetNodeOrNull<Button>(FinalizeButtonPath);

		if (_loadout == null)
			GD.PushWarning("[Bridge] LoadoutScreenPath not set or node is not a CanvasItem.");
		if (_battleRoot == null)
			GD.PushWarning("[Bridge] BattleRootPath not set or node is not a CanvasItem.");
		if (_finalizeBtn == null)
			GD.PushWarning("[Bridge] FinalizeButtonPath not set or node is not a Button.");

		// Default: show loadout, hide battle.
		if (_loadout != null)    _loadout.Visible = true;
		if (_battleRoot != null) _battleRoot.Visible = false;

		if (_finalizeBtn != null)
			_finalizeBtn.Pressed += OnFinalizePressed;
	}

	private void OnFinalizePressed()
	{
		// Simple swap. If you need to pass data to battle, do it here
		// by pulling from your registries / LoadoutScreen instance.
		if (_loadout != null)    _loadout.Visible = false;
		if (_battleRoot != null) _battleRoot.Visible = true;

		GD.Print("[Bridge] Finalize pressed → switch to Battle.");
	}
}
