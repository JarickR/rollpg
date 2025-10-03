// Scripts/Godot/IconPathLoader.cs
using Godot;

namespace DiceArena.Godot
{
	/// <summary>
	/// Central place for loadout icon paths and convenience loaders.
	/// </summary>
	public static class IconPathLoader
	{
		private const string ClassesDir = "res://Content/Icons/Classes";
		private const string SpellsDir  = "res://Content/Icons/Spells";

		// --- Paths -------------------------------------------------------------

		public static string LoadClassPath(string classId) => $"{ClassesDir}/{classId}.png";
		public static string LoadSpellPath(string spellId) => $"{SpellsDir}/{spellId}.png";

		// --- Convenience texture loaders (optional) ---------------------------

		public static Texture2D LoadClassTexture(string classId)
		{
			var path = LoadClassPath(classId);
			return GD.Load<Texture2D>(path);
		}

		public static Texture2D LoadSpellTexture(string spellId)
		{
			var path = LoadSpellPath(spellId);
			return GD.Load<Texture2D>(path);
		}

		/// <summary>1×1 fully transparent texture (useful as a safe fallback).</summary>
		public static Texture2D Transparent1x1()
		{
			var img = new Image();
			img.CreateEmpty(1, 1, false, Image.Format.Rgba8);
			img.Fill(Colors.Transparent);
			return ImageTexture.CreateFromImage(img);
		}
	}
}
