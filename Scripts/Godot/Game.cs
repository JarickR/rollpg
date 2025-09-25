// Scripts/Godot/Game.cs
using Godot;
using DiceArena.Engine.Loadout;

namespace DiceArena.Godot
{
	/// <summary>
	/// Simple screen switcher: Loadout -> (Finalize) -> Battle.
	/// </summary>
	public partial class Game : Node
	{
		[Export] public NodePath LoadoutScreenPath { get; set; } = new NodePath();
		[Export] public NodePath BattleRootPath   { get; set; } = new NodePath();

		private LoadoutScreen? _loadout;
		private Control? _battleRoot;

		public override void _Ready()
		{
			_loadout   = GetNodeOrNull<LoadoutScreen>(LoadoutScreenPath);
			_battleRoot = GetNodeOrNull<Control>(BattleRootPath);

			if (_loadout != null)
				_loadout.Finalized += OnLoadoutFinalized;

			// Start on loadout
			if (_loadout != null) _loadout.Visible = true;
			if (_battleRoot != null) _battleRoot.Visible = false;
		}

		private void OnLoadoutFinalized(int partySize, string[] playersJson)
		{
			GD.Print($"[Game] Received finalize: party={partySize}, players={playersJson.Length}");

			if (_loadout != null) _loadout.Visible = false;
			if (_battleRoot != null) _battleRoot.Visible = true;

			// If your BattleRoot script needs the payload, you can look it up and pass it here.
			// Example:
			// var br = _battleRoot as BattleRoot;
			// br?.InitializeFromLoadout(partySize, playersJson);
		}
	}
}
