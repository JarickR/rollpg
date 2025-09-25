using Godot;

namespace DiceArena.Godot
{
	/// <summary>
	/// Clears the icon cache once when the scene boots, then removes itself.
	/// Drop this anywhere in the scene tree.
	/// </summary>
	public partial class IconFlushOnBoot : Node
	{
		public override void _Ready()
		{
			IconLibrary.ClearCache();
			QueueFree();
		}
	}
}
