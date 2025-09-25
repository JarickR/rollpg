// File: Scripts/Godot/IconLibrary.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;

namespace DiceArena.Godot
{
	/// <summary>
	/// Image/texture loader with cache and filename heuristics:
	///   res://Content/Icons/Classes/*.png
	///   res://Content/Icons/Tier1Spells/*-t1.png
	///   res://Content/Icons/Tier2Spells/*-t2.png   (+ variant: *plus-t2.png)
	///   res://Content/Icons/Tier3Spells/*-t3.png   (++ variant: *plusplus-t3.png)
	/// Class files are TitleCased. Spell files are lowercase, spaceless.
	/// </summary>
	public static class IconLibrary
	{
		public const string BaseDir        = "res://Content/Icons";
		public const string ClassDir       = $"{BaseDir}/Classes";
		public const string Tier1SpellsDir = $"{BaseDir}/Tier1Spells";
		public const string Tier2SpellsDir = $"{BaseDir}/Tier2Spells";
		public const string Tier3SpellsDir = $"{BaseDir}/Tier3Spells";

		private static readonly Dictionary<string, Texture2D> _cache = new();

		public static void ClearCache()
		{
			_cache.Clear();
			GC.Collect();
		}

		public static Texture2D GetClassTexture(string classId, int size)
		{
			if (string.IsNullOrWhiteSpace(classId))
				return Transparent1x1;

			var title = ToTitleCase(classId);
			var candidates = new[]
			{
				$"{ClassDir}/{title}.png",
				$"{ClassDir}/{UpperFirst(classId)}.png",
				$"{ClassDir}/{classId}.png"
			};

			if (!TryPickExisting(candidates, out var path))
			{
				GD.PushWarning($"[IconLibrary] Class icon not found for '{classId}'.");
				return Transparent1x1;
			}

			var key = $"class:{classId}|{size}";
			var tex = LoadTextureCached(key, path);
			return ResizeToFit(tex, size);
		}

		/// <summary>
		/// Load a spell icon by *base* id (e.g., "chainlightning", "spikedshield") and tier (1/2/3).
		/// </summary>
		public static Texture2D GetSpellTexture(string baseId, int tier, int size)
		{
			if (string.IsNullOrWhiteSpace(baseId))
				return Transparent1x1;

			tier = Math.Clamp(tier, 1, 3);

			// Sanitize base id in case a caller passes "Fireball1" or "SpikedShieldT1"
			var baseRaw  = StripTierSuffix(baseId);
			baseRaw      = StripTrailingDigitThenDanglingT(baseRaw);
			var baseName = RemoveNonAlnum(baseRaw); // e.g., "cragshot"

			var dir = tier switch
			{
				1 => Tier1SpellsDir,
				2 => Tier2SpellsDir,
				_ => Tier3SpellsDir
			};

			// Preferred pattern
			var candidates = new List<string> { $"{dir}/{baseName}-t{tier}.png" };

			// + and ++ variants many of your Tier2/Tier3 files use
			if (tier == 2) candidates.Add($"{dir}/{baseName}plus-t2.png");
			if (tier == 3) candidates.Add($"{dir}/{baseName}plusplus-t3.png");

			if (!TryPickExisting(candidates, out var path))
			{
				GD.PushWarning($"[IconLibrary] Spell icon not found for '{baseId}' (tier {tier}). Tried: {string.Join(", ", candidates)}");
				return Transparent1x1;
			}

			var key = $"spell:{baseName}@{tier}|{size}";
			var tex = LoadTextureCached(key, path);
			return ResizeToFit(tex, size);
		}

		// ---------------------------- utils ----------------------------------

		private static bool TryPickExisting(IEnumerable<string> candidates, out string path)
		{
			foreach (var c in candidates)
			{
				if (FileAccess.FileExists(c))
				{
					path = c;
					return true;
				}
			}
			path = string.Empty;
			return false;
		}

		private static Texture2D LoadTextureCached(string key, string path)
		{
			if (_cache.TryGetValue(key, out var cached) && IsValid(cached))
				return cached;

			var tex = GD.Load<Texture2D>(path);
			if (tex == null)
			{
				try
				{
					var img = Image.LoadFromFile(path);
					if (img != null) tex = ImageTexture.CreateFromImage(img);
				}
				catch (Exception ex)
				{
					GD.PushWarning($"[IconLibrary] Failed to load '{path}': {ex.Message}");
				}
			}

			tex ??= Transparent1x1;
			_cache[key] = tex;
			return tex;
		}

		private static Texture2D ResizeToFit(Texture2D texture, int targetMaxSide)
		{
			if (texture == null) return Transparent1x1;

			int w = texture.GetWidth(), h = texture.GetHeight();
			int maxSide = Math.Max(w, h);
			if (maxSide <= targetMaxSide) return texture;

			float scale = (float)targetMaxSide / maxSide;
			int newW = Math.Max(1, (int)Math.Floor(w * scale));
			int newH = Math.Max(1, (int)Math.Floor(h * scale));

			var img = texture.GetImage();
			if (img == null) return texture;

			img.Resize(newW, newH, Image.Interpolation.Lanczos);
			return ImageTexture.CreateFromImage(img);
		}

		private static bool IsValid(Resource res) => GodotObject.IsInstanceValid(res);

		private static string ToTitleCase(string id)
		{
			var s = id.Replace('_', ' ').Replace('-', ' ');
			s = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLowerInvariant());
			return s.Replace(" ", string.Empty);
		}

		private static string UpperFirst(string s)
		{
			if (string.IsNullOrEmpty(s)) return s;
			if (s.Length == 1) return s.ToUpperInvariant();
			return char.ToUpperInvariant(s[0]) + s[1..];
		}

		private static string RemoveNonAlnum(string s)
		{
			Span<char> buffer = stackalloc char[s.Length];
			int i = 0;
			foreach (var ch in s)
				if (char.IsLetterOrDigit(ch))
					buffer[i++] = char.ToLowerInvariant(ch);
			return new string(buffer.Slice(0, i));
		}

		private static string StripTierSuffix(string s)
		{
			if (s.EndsWith("-1", StringComparison.OrdinalIgnoreCase) ||
				s.EndsWith("-2", StringComparison.OrdinalIgnoreCase) ||
				s.EndsWith("-3", StringComparison.OrdinalIgnoreCase))
				return s[..^2];
			return s;
		}

		/// <summary>
		/// Remove a trailing digit; **only then** remove a dangling 't/T' left by forms like "NameT1".
		/// This prevents clipping words that naturally end with 't' (e.g., "Crag Shot").
		/// </summary>
		private static string StripTrailingDigitThenDanglingT(string s)
		{
			if (string.IsNullOrEmpty(s)) return s;

			bool removedDigit = false;
			if (char.IsDigit(s[^1]))
			{
				s = s[..^1];
				removedDigit = true;
			}

			if (removedDigit && s.Length > 0 && (s[^1] == 't' || s[^1] == 'T'))
				s = s[..^1];

			return s;
		}

		// 1x1 transparent placeholder
		private static Texture2D? _transparent;
		public static Texture2D Transparent1x1
		{
			get
			{
				if (_transparent != null && IsValid(_transparent))
					return _transparent;

				var img = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
				img.SetPixel(0, 0, Colors.Transparent);
				_transparent = ImageTexture.CreateFromImage(img);
				return _transparent!;
			}
		}
	}
}
