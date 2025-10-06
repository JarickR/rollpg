using Godot;

namespace DiceArena.Godot
{
	/// <summary>
	/// Centralized icon path + tiny transparent texture helper.
	/// Keep a single copy of this class in the project.
	/// </summary>
	public static class IconPathLoader
	{
		private const string ClassesDir = "res://Content/Icons/Classes";
		private const string SpellsDir  = "res://Content/Icons/Spells";

		public static string LoadClassPath(string id) => $"{ClassesDir}/{id}.png";
		public static string LoadSpellPath(string id) => $"{SpellsDir}/{id}.png";

		/// <summary>1x1 transparent texture for safe fallbacks.</summary>
		public static Texture2D Transparent1x1()
		{
			var img = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
			img.Fill(new Color(0, 0, 0, 0));
			return ImageTexture.CreateFromImage(img);
		}

		/// <summary>Load a Texture2D if it exists, else return transparent.</summary>
		public static Texture2D TryLoadTexture(string path)
		{
			if (string.IsNullOrEmpty(path))
				return Transparent1x1();

			if (ResourceLoader.Exists(path))
			{
				var tex = GD.Load<Texture2D>(path);
				return tex ?? Transparent1x1();
			}

			GD.PushWarning($"[IconPathLoader] Missing icon: {path}");
			return Transparent1x1();
		}
	}
}
