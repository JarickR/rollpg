using Godot;

namespace DiceArena.Godot
{
	public static class IconLibrary
	{
		// Adjust to your real folders / filenames (lowercase/uppercase must match).
		private const string CLASS_DIR = "res://Content/Icons/Classes/";        // e.g. Thief.png
		private const string TIER1_DIR = "res://Content/Icons/Tier1Spells/";    // e.g. attack-t1.png
		private const string TIER2_DIR = "res://Content/Icons/Tier2Spells/";    // e.g. attackplus-t2.png

		public static Texture2D? GetClassTexture(string classId, int size)
			=> LoadScaled($"{CLASS_DIR}{classId}.png", size);

		public static Texture2D? GetSpellTexture(string spellName, int tier, int size)
		{
			if (string.IsNullOrEmpty(spellName)) return null;
			var dir = tier == 1 ? TIER1_DIR : TIER2_DIR;

			// two common filename styles; try both
			var a = $"{dir}{spellName}.png";
			var b = $"{dir}{spellName.ToLower()}-t{tier}.png";

			return LoadScaled(a, size) ?? LoadScaled(b, size);
		}

		private static Texture2D? LoadScaled(string path, int size)
		{
			var tex = ResourceLoader.Load<Texture2D>(path);
			if (tex == null)
			{
				GD.PushWarning($"[Icons] Missing: {path}");
				return null;
			}
			// Return as-is (you’re importing at the correct size), or
			// wrap in an AtlasTexture if you need strict sizing.
			return tex;
		}
	}
}
