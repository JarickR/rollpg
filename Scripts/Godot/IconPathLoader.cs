// Scripts/Godot/IconPathLoader.cs
using Godot;

namespace DiceArena.Godot
{
	/// <summary>
	/// Resolves resource paths for class/spell icons from the Content folder.
	/// Uses the single canonical IconPool defined in IconPool.cs.
	/// </summary>
	public static class IconPathLoader
	{
		/// <summary>
		/// Returns a Godot resource path for an icon, or empty string if not found.
		/// </summary>
		public static string LoadPath(IconPool pool, string id)
		{
			if (string.IsNullOrWhiteSpace(id))
				return string.Empty;

			// You unified spells into one folder; classes in their own.
			string baseDir = pool switch
			{
				IconPool.Class      => "res://Content/Icons/Classes",
				IconPool.Tier1Spell => "res://Content/Icons/Spells",
				IconPool.Tier2Spell => "res://Content/Icons/Spells",
				IconPool.Tier3Spell => "res://Content/Icons/Spells",
				_ => "res://Content/Icons/Spells"
			};

			string[] tryExts = { ".png", ".webp" };

			foreach (var ext in tryExts)
			{
				var candidate = $"{baseDir}/{id}{ext}";
				if (ResourceLoader.Exists(candidate))
					return candidate;
			}

			// Fallback: check raw file presence explicitly (fully qualify FileAccess)
			foreach (var ext in tryExts)
			{
				var candidate = $"{baseDir}/{id}{ext}";
				if (global::Godot.FileAccess.FileExists(candidate))
					return candidate;
			}

			return string.Empty;
		}
	}
}
