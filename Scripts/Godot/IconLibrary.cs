// Scripts/Godot/IconLibrary.cs
using Godot;

namespace DiceArena.Godot
{
	/// <summary>
	/// Resolve textures for classes & spells with loud logging and safe fallbacks.
	/// </summary>
	public static class IconLibrary
	{
		// 1x1 transparent texture for "empty" slots.
		private static Texture2D? _transparent1x1;
		public static Texture2D Transparent1x1 => _transparent1x1 ??= MakeSolid(1, 1, Colors.Transparent);

		// Visible fallback so missing icons stand out.
		private static Texture2D? _fallback32;
		public static Texture2D Fallback32 => _fallback32 ??= MakeSolid(32, 32, new Color(1f, 0f, 1f)); // magenta

		/// <summary>Convenience for old callers still invoking Clear() on boot.</summary>
		public static void Clear()
		{
			// We don’t keep a dictionary cache anymore, but keep this to satisfy callers.
			_transparent1x1 = null;
			_fallback32 = null;
			GD.Print("[IconLibrary] Cleared simple in-memory fallbacks.");
		}

		/// <summary>res://Content/Icons/Classes/{Class}.png</summary>
		public static Texture2D GetClassTexture(string classId, int _size = 32)
		{
			var safe = (classId ?? string.Empty).Trim();
			var path = $"res://Content/Icons/Classes/{safe}.png";
			return LoadTextureOrFallback(path);
		}

		/// <summary>
		/// T1: res://Content/Icons/Tier1Spells/{name}-t1.png
		/// T2: res://Content/Icons/Tier2Spells/{name}-t2.png
		/// T3: res://Content/Icons/Tier3Spells/{name}-t3.png
		/// </summary>
		public static Texture2D GetSpellTexture(string name, int tier, int _size = 32)
		{
			var safe = (name ?? string.Empty).Trim().ToLowerInvariant();

			string dir = tier switch
			{
				1 => "Tier1Spells",
				2 => "Tier2Spells",
				3 => "Tier3Spells",
				_ => "Tier1Spells"
			};

			string suffix = tier switch
			{
				1 => "-t1",
				2 => "-t2",
				3 => "-t3",
				_ => "-t1"
			};

			var path = $"res://Content/Icons/{dir}/{safe}{suffix}.png";
			return LoadTextureOrFallback(path);
		}

		// ----------------- helpers -----------------

		private static Texture2D LoadTextureOrFallback(string path)
		{
			if (!FileAccess.FileExists(path))
			{
				GD.PushWarning($"[IconLibrary] File not found: {path}");
				return Fallback32;
			}

			var tex = ResourceLoader.Load<Texture2D>(path);
			if (tex == null)
			{
				GD.PushError($"[IconLibrary] Load failed (null): {path}");
				return Fallback32;
			}

			GD.Print($"[IconLibrary] Load ok: {path}");
			return tex;
		}

		private static Texture2D MakeSolid(int w, int h, Color c)
		{
			var img = Image.CreateEmpty(w, h, false, Image.Format.Rgba8);
			img.Fill(c);
			return ImageTexture.CreateFromImage(img);
		}
	}
}
