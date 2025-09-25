using Godot;

namespace DiceArena.Godot
{
	public partial class BattleRoot : Control
	{
		// Initialize non-nullable exported paths to silence CS8618 without changing usage.
		[Export] public NodePath FriendlyRootPath { get; set; } = new NodePath();
		[Export] public NodePath EnemyRootPath    { get; set; } = new NodePath();
		[Export] public NodePath LogRootPath      { get; set; } = new NodePath();

		public override void _Ready()
		{
			// Your existing _Ready logic stays as-is.
			// (No functional changes required for the warnings you reported.)
		}

		// ... keep the rest of your BattleRoot code unchanged ...
	}
}
