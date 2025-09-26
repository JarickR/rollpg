// Scripts/Godot/IconFlushOnBoot.cs
using Godot;

namespace DiceArena.Godot
{
	/// <summary>
	/// Keeps old startup behavior: clear any icon caches on boot.
	/// </summary>
	public partial class IconFlushOnBoot : Node
	{
		public override void _EnterTree()
		{
			IconLibrary.Clear();
		}
	}
}
