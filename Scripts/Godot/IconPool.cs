// Scripts/Godot/IconPool.cs
namespace DiceArena.Godot
{
	/// <summary>
	/// Single, canonical enum for where an icon is looked up from.
	/// Keep this as the only definition of IconPool in the project.
	/// </summary>
	public enum IconPool
	{
		None = 0,
		Class = 1,
		Tier1Spell = 2,
		Tier2Spell = 3,
		Tier3Spell = 4
	}
}
