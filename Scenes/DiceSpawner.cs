// res://Scenes/DiceSpawner.cs
#nullable enable
using Godot;

namespace DiceArena.Godot
{
	public partial class DiceSpawner : Node
	{
		[Export] public NodePath DiceWorldPath { get; set; } = default!; // points to the node with DiceInteractor

		private DiceInteractor? _interactor;

		public override void _Ready()
		{
			if (DiceWorldPath == default)
			{
				GD.PushError("[DiceSpawner] DiceWorldPath is not set.");
				return;
			}

			_interactor = GetNode<DiceInteractor>(DiceWorldPath);
			if (_interactor == null)
			{
				GD.PushError("[DiceSpawner] DiceWorldPath does not point to a DiceInteractor.");
				return;
			}

			// Show/enable the dice and place it at the center at the start
			_interactor.SpawnAtCenterAndWake();
			_interactor.EnableDice(true);
		}

		// You can call this from your turn system later:
		public void StartPlayerTurn()
		{
			_interactor?.SpawnAtCenterAndWake();
			_interactor?.EnableDice(true);
		}

		public void EndPlayerTurn()
		{
			_interactor?.EnableDice(false);
		}
	}
}
