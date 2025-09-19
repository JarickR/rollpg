using Godot;

namespace DiceArena.Godot
{
	/// <summary>
	/// Clears the icon cache once when this node enters the scene.
	/// Safe to add anywhere in your boot scene; it frees itself after running.
	/// </summary>
	[GlobalClass]
	public partial class IconFlushOnBoot : Node
	{
		[Export] public bool ClearOnReady { get; set; } = true;

		public override void _Ready()
		{
			if (ClearOnReady)
				IconLibrary.Clear();

			QueueFree();
		}
	}
}
