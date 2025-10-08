using DiceArena.Data;
using Godot;
using DiceArena.Godot; // <-- so IconLibrary resolves

namespace DiceArena.Engine.Loadout
{
	/// <summary>
	/// Legacy shim preserved for older references.
	/// </summary>
	public static class IconLibraryShim
	{
		public static Texture2D GetClassTexture(ClassData data)
			=> IconLibrary.GetClassTexture(data.Id);

		public static Texture2D GetSpellTexture(SpellData data)
			=> IconLibrary.GetSpellTexture(data.Id, data.Tier);

		public static Texture2D GetTransparent()
			=> IconLibrary.Transparent1x1;

		// Legacy aliases to satisfy old call-sites:
		public static Texture2D Class(ClassData data) => GetClassTexture(data);
		public static Texture2D Spell(SpellData data) => GetSpellTexture(data);
	}
}
