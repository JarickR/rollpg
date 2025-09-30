using Godot;
using EngineLoadout = DiceArena.Engine.Loadout;
using EngineGame    = DiceArena.Engine.Core.Game;

namespace DiceArena.Godot
{
	/// <summary>Shows Loadout first; when finalized, applies to Battle and swaps visibility.</summary>
	public partial class LoadoutToBattleBridge : Node
	{
		[Export] public NodePath LoadoutScreenPath { get; set; } = default!;
		[Export] public NodePath BattleRootPath    { get; set; } = default!;

		private EngineLoadout.LoadoutScreen? _loadout;
		private BattleRoot? _battle;

		public override void _Ready()
		{
			_loadout = GetNodeOrNull<EngineLoadout.LoadoutScreen>(LoadoutScreenPath);
			_battle  = GetNodeOrNull<BattleRoot>(BattleRootPath);

			if (_loadout == null || _battle == null)
			{
				GD.PushError("[Bridge] Missing paths.");
				return;
			}

			_loadout.Visible = true;
			_battle.Visible  = false;

			_loadout.Finalized += OnLoadoutFinalized;
		}

		private void OnLoadoutFinalized(EngineLoadout.LoadoutScreen sender, EngineGame game)
		{
			if (_battle == null) return;

			_battle.ApplyFromGame(game);

			// swap screens
			if (_loadout != null) _loadout.Visible = false;
			_battle.Visible = true;
		}
	}
}
