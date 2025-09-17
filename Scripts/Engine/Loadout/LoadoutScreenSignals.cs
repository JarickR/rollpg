// Scripts/Godot/LoadoutScreenSignals.cs
using Godot;

namespace DiceArena.GodotUI
{
	/// <summary>
	/// Signals for the LoadoutScreen (split to mirror your project structure).
	/// </summary>
	public partial class LoadoutScreen : Control
	{
		// Bubble-up from member cards so other systems can subscribe at the screen level.
		[Signal]
		public delegate void SelectionChangedEventHandler(int memberIndex, string? classId, string[] tier1Ids, string? tier2Id);
	}
}
