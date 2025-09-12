// Scripts/Godot/Game.cs
using Godot;
using DiceArena.Engine.Content;
using DiceArena.Engine.Loadout;

public partial class Game : Node
{
	private ContentBundle _bundle = default!;

	public override void _Ready()
	{
		_bundle = ContentDatabase.LoadFromFolder("res://Content");

		// Inject bundle into LoadoutScreen if present
		var loadout = GetNodeOrNull<LoadoutScreen>("%LoadoutScreen");
		if (loadout != null)
			loadout.Bundle = _bundle;

		// If you have a battle HUD root, hide it so loadout owns the screen
		var battleHud = GetNodeOrNull<Control>("%BattleHud");
		if (battleHud != null)
			battleHud.Visible = false;

		// If you don’t have a single root, you can group battle UI nodes as "battle_ui"
		// and hide them like this:
		// foreach (var n in GetTree().GetNodesInGroup("battle_ui"))
		//     if (n is CanvasItem c) c.Visible = false;

		// Optional: inject bundle into HeroPanel if present (keeps previous behavior)
		var heroPanel = GetNodeOrNull<HeroPanel>("%HeroPanel");
		if (heroPanel != null)
			heroPanel.SetBundle(_bundle);
	}
}
