// Scripts/Engine/Loadout/IconLibraryBridge.cs
using DiceArena.Data;
using Godot;
using DiceArena.Godot; // IconLibrary

namespace DiceArena.Engine.Loadout
{
	/// <summary>
	/// Thin adapter so engine/loadout code can ask for textures
	/// without depending on the Godot-side internals.
	/// </summary>
	public static class IconLibraryBridge
	{
		// --------- Preferred (data objects) ----------
		public static Texture2D GetClassTexture(ClassData data)
			=> IconLibrary.GetClassTexture(data.Id);

		public static Texture2D GetSpellTexture(SpellData data)
			=> IconLibrary.GetSpellTexture(data.Id, data.Tier);

		public static Texture2D GetTransparent()
			=> IconLibrary.Transparent1x1;

		// --------- String id overloads ----------
		public static Texture2D GetClassTexture(string classId)
			=> IconLibrary.GetClassTexture(classId);

		public static Texture2D GetSpellTexture(string spellId, int tier)
			=> IconLibrary.GetSpellTexture(spellId, tier);

		// --------- Legacy aliases (keep signatures used around the codebase) ----------
		// Class
		public static Texture2D Class(ClassData data) => GetClassTexture(data);
		public static Texture2D Class(ClassData data, int _ignoredSize) => GetClassTexture(data);

		// ✅ Add string + size to satisfy calls like Class("thief", 64)
		public static Texture2D Class(string classId) => GetClassTexture(classId);
		public static Texture2D Class(string classId, int _ignoredSize) => GetClassTexture(classId);

		// Spell
		public static Texture2D Spell(SpellData data) => GetSpellTexture(data);
		public static Texture2D Spell(SpellData data, int _ignoredTier, int _ignoredSize) => GetSpellTexture(data);

		// ✅ Clean, explicit string overloads (handle calls like Spell("fireball", 2, 64))
		public static Texture2D Spell(string spellId, int tier) => GetSpellTexture(spellId, tier);
		public static Texture2D Spell(string spellId, int tier, int _ignoredSize) => GetSpellTexture(spellId, tier);
	}
}
