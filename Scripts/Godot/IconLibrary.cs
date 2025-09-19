using System;
using System.Collections.Generic;
using Godot;

namespace DiceArena.Godot
{
	/// <summary>
	/// Centralized icon loader + nearest-neighbor scaler + cache.
	/// Looks up icons under:
	///   Classes: res://Content/Icons/Classes/{classId}.png
	///   Spells : res://Content/Icons/Spells/{spellId}-t{tier}.png
	///
	/// Also tries "plus"/"plusplus" alternates for tier 2/3 if base name not found.
	/// </summary>
	public static class IconLibrary
	{
		public const string ClassDir = "res://Content/Icons/Classes";
		public const string SpellDir = "res://Content/Icons/Spells";

		private static readonly Dictionary<string, Texture2D> _cache = new();
		private static ImageTexture? _transparent1x1;

		/// <summary>1x1 fully transparent pixel texture.</summary>
		public static ImageTexture Transparent1x1 => _transparent1x1 ??= CreateTransparent();

		/// <summary>Clear all cached textures (called once on boot or when reloading content).</summary>
		public static void Clear()
		{
			_cache.Clear();
			// Keep the tiny transparent around; it’s fine to reuse.
		}

		/// <summary>Get a class icon scaled to the requested pixel size.</summary>
		public static Texture2D GetClassTexture(string classId, int size)
		{
			if (string.IsNullOrWhiteSpace(classId) || size <= 0)
				return Transparent1x1;

			var key = $"class:{classId}:{size}";
			if (_cache.TryGetValue(key, out var tex))
				return tex;

			var fname = $"{ClassDir}/{Sanitize(classId)}.png";
			tex = LoadAndScale(fname, size) ?? Transparent1x1;
			_cache[key] = tex;
			return tex;
		}

		/// <summary>Get a spell icon by spell id + tier (1..3) scaled to size.</summary>
		public static Texture2D GetSpellTexture(string spellId, int tier, int size)
		{
			if (string.IsNullOrWhiteSpace(spellId) || tier < 1 || tier > 3 || size <= 0)
				return Transparent1x1;

			var key = $"spell:{spellId}:t{tier}:{size}";
			if (_cache.TryGetValue(key, out var tex))
				return tex;

			var baseName = Sanitize(spellId);
			// Primary: "{name}-t{tier}.png"
			string[] candidates =
			{
				$"{SpellDir}/{baseName}-t{tier}.png",
				// Alternates the project uses:
				// tier 2: "{name}plus-t2.png"
				// tier 3: "{name}plusplus-t3.png"
				tier == 2 ? $"{SpellDir}/{baseName}plus-t2.png" : null,
				tier == 3 ? $"{SpellDir}/{baseName}plusplus-t3.png" : null,
			};

			foreach (var path in candidates)
			{
				if (string.IsNullOrEmpty(path)) continue;
				tex = LoadAndScale(path, size);
				if (tex != null)
				{
					_cache[key] = tex;
					return tex;
				}
			}

			// Not found → transparent fallback
			_cache[key] = Transparent1x1;
			return Transparent1x1;
		}

		// ---------- helpers ----------

		private static string Sanitize(string id)
		{
			// Normalize ids to file names like "chain lightning" -> "chainlightning"
			// (your files are "name-t1.png", all lowercase, no spaces)
			Span<char> buf = stackalloc char[id.Length];
			int j = 0;
			foreach (var ch in id)
			{
				if (char.IsLetterOrDigit(ch))
					buf[j++] = char.ToLowerInvariant(ch);
			}
			return new string(buf[..j]);
		}

		private static Texture2D? LoadAndScale(string resPath, int size)
		{
			// Prefer loading Image directly so we can resize with Nearest.
			if (!FileAccess.FileExists(resPath))
				return null;

			var img = Image.LoadFromFile(resPath);
			if (img == null)
				return null;

			// Use Nearest to keep pixel art crisp.
			img.Resize(size, size, Image.Interpolation.Nearest);
			var tex = ImageTexture.CreateFromImage(img);
			return tex;
		}

		private static ImageTexture CreateTransparent()
		{
			var img = new Image();
			img.CreateEmpty(1, 1, false, Image.Format.Rgba8);
			img.SetPixel(0, 0, new Color(0, 0, 0, 0));
			return ImageTexture.CreateFromImage(img);
		}
	}
}
