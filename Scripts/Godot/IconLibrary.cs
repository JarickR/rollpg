// Scripts/Godot/IconLibrary.cs
using Godot;
using System;

namespace DiceArena.Godot
{
	/// <summary>
	/// Central icon cache for textures used in the UI (classes, spells, etc.).
	/// Caches loaded textures and provides a 1×1 transparent fallback.
	/// </summary>
	public static class IconLibrary
	{
		private static readonly System.Collections.Generic.Dictionary<string, Texture2D> _cache = new();
		private static Texture2D? _transparent;

		/// <summary>1×1 transparent texture fallback, lazily created.</summary>
		public static Texture2D Transparent1x1
		{
			get
			{
				if (_transparent == null)
				{
					var img = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
					img.Fill(new Color(0, 0, 0, 0));
					_transparent = ImageTexture.CreateFromImage(img);
				}
				return _transparent;
			}
		}

		public static Texture2D GetClassTexture(string classId)
		{
			if (string.IsNullOrWhiteSpace(classId))
				return Transparent1x1;

			var key = $"class:{classId}";
			if (_cache.TryGetValue(key, out var cached))
				return cached;

			var path = IconPathLoader.LoadPath(IconPool.Class, classId);
			var tex = TryLoad(path) ?? Transparent1x1;
			_cache[key] = tex;
			return tex;
		}

		public static Texture2D GetSpellTexture(string spellId, int tier)
		{
			if (string.IsNullOrWhiteSpace(spellId))
				return Transparent1x1;

			var key = $"spell:{spellId}:{tier}";
			if (_cache.TryGetValue(key, out var cached))
				return cached;

			var pool = tier switch
			{
				1 => IconPool.Tier1Spell,
				2 => IconPool.Tier2Spell,
				3 => IconPool.Tier3Spell,
				_ => IconPool.Tier1Spell
			};

			var path = IconPathLoader.LoadPath(pool, spellId);
			var tex = TryLoad(path) ?? Transparent1x1;
			_cache[key] = tex;
			return tex;
		}

		// -------------------- internals --------------------

		private static Texture2D? TryLoad(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return null;

			try
			{
				if (ResourceLoader.Exists(path))
					return ResourceLoader.Load<Texture2D>(path);

				// Fallback: check raw file presence explicitly (use fully qualified FileAccess)
				if (global::Godot.FileAccess.FileExists(path))
					return ResourceLoader.Load<Texture2D>(path);
			}
			catch (Exception ex)
			{
				GD.PushWarning($"[IconLibrary] Failed loading texture at {path}: {ex.Message}");
			}

			return null;
		}
	}
}
