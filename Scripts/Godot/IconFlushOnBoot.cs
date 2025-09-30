// Scripts/Godot/IconFlushOnBoot.cs
// Optional: warms up IconLibrary so the transparent texture is ready.
// NOTE: There is no Clear() or ClearCache() in IconLibrary anymore.

using Godot;

namespace DiceArena.Godot
{
	public partial class IconFlushOnBoot : Node
	{
		public override void _Ready()
		{
			// Touch the static to ensure it’s initialized and log for visibility.
			_ = IconLibrary.Transparent1x1;
			GD.Print("[IconLibrary] Warmed up.");
		}
	}
}
