#nullable enable
using Godot;

namespace DiceArena.Engine.Loadout
{
	/// <summary>
	/// Thin wrapper so UI code doesn’t depend directly on the Godot-side IconLibrary.
	/// Also gives us safe fallbacks if anything is missing.
	/// </summary>
	public static class IconLibraryShim
	{
		/// <summary>Return a class icon at the requested size (pixels).</summary>
		public static Texture2D GetClassTexture(string classId, int size)
		{
			// Prefer the engine’s icon library if present
			var tex = DiceArena.Godot.IconLibrary.GetClassTexture(classId, size);
			return tex ?? Transparent1x1();
		}

		/// <summary>Return a spell icon at the requested size (pixels).</summary>
		public static Texture2D GetSpellTexture(string spellId, int tier, int size)
		{
			var tex = DiceArena.Godot.IconLibrary.GetSpellTexture(spellId, tier, size);
			return tex ?? Transparent1x1();
		}

		/// <summary>No-op cache clear (satisfies callers even if the underlying lib has no clear).</summary>
		public static void Clear() { /* intentionally empty */ }

		/// <summary>1×1 fully transparent texture (fallback if the library can’t provide one).</summary>
		private static Texture2D Transparent1x1()
		{
			// If your IconLibrary exposes a cached 1x1, use it; otherwise synthesize here.
			var libTransparent = DiceArena.Godot.IconLibrary.Transparent1x1;
			if (libTransparent != null)
				return libTransparent;

			var img = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
			img.Fill(Colors.Transparent);
			return ImageTexture.CreateFromImage(img);
		}
	}
}
