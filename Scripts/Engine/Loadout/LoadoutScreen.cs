// Scripts/Engine/Loadout/LoadoutScreen.cs
using Godot;
using DiceArena.Engine.Loadout; // ← brings in the extension method PopulateMemberPanels()

namespace DiceArena.Godot
{
	public partial class LoadoutScreen : Control
	{
		// Set this in the Inspector to your container that holds the 1–4 PlayerLoadoutPanel instances
		[Export] public NodePath MemberCardsContainerPath { get; set; } = new NodePath();

		public override void _EnterTree()
		{
			GD.Print($"[Loadout] _EnterTree (node path={GetPath()})");
		}

		public override void _Ready()
		{
			GD.Print($"[Loadout] _Ready begin (node path={GetPath()})");

			// Let the extension fill all PlayerLoadoutPanel children under the host.
			this.PopulateMemberPanels();

			GD.Print("[Loadout] _Ready end");
		}
	}
}
