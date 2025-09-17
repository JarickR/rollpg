// Scripts/Godot/IconLibrary.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DiceArena.Engine.Loadout
{
	/// <summary>
	/// Robust icon resolver with caching.
	/// Looks in your project tree:
	///   Classes:      res://art/Icons/Classes/<name>.(png|svg|webp)
	///   Tier1 Spells: res://art/Icons/Tier1Spells/<name>.(png|svg|webp)
	///   Tier2 Spells: res://art/Icons/Tier2Spells/<name>.(png|svg|webp)
	///   Tier3 Spells: res://art/Icons/Tier3Spells/<name>.(png|svg|webp)  (supported if you use it)
	/// Falls back to res://icon.svg when not found.
	/// Filename matching is forgiving: hyphens/underscores/spaces removed, pascalized variants, with/without "-1"/"-2" tier suffix.
	/// </summary>
	public static class IconLibrary
	{
		// ---- Paths (adjust if you ever move your folders) ----
		private const string ClassesDir = "res://art/Icons/Classes";
		private const string Tier1Dir   = "res://art/Icons/Tier1Spells";
		private const string Tier2Dir   = "res://art/Icons/Tier2Spells";
		private const string Tier3Dir   = "res://art/Icons/Tier3Spells"; // optional
		private const string Fallback   = "res://icon.svg";

		private static readonly string[] Exts = { ".png", ".svg", ".webp" };

		// Simple path->texture cache
		private static readonly Dictionary<string, Texture2D> Cache = new();

		// ------------- Public API -------------

		public static Texture2D GetClassTexture(string id)
		{
			// Try candidates inside ClassesDir; if not found, use fallback.
			return ResolveTexture(ClassesDir, id)
				   ?? Load(Fallback)
				   ?? new ImageTexture(); // never null, but prevents NRE
		}

		/// <summary>
		/// Returns a spell icon. Tries variants with and without a trailing "-tier" suffix.
		/// Example: "spiked-shield-1" will first look for "spiked-shield", then "spiked-shield-1".
		/// </summary>
		public static Texture2D GetSpellTexture(string id, int tier)
		{
			var dir = tier switch
			{
				1 => Tier1Dir,
				2 => Tier2Dir,
				3 => Tier3Dir, // supported if you have this folder; otherwise we'll fall back
				_ => Tier1Dir
			};

			var baseId = StripTierSuffix(id);

			return ResolveTexture(dir, baseId)
				   ?? ResolveTexture(dir, id)
				   ?? Load(Fallback)
				   ?? new ImageTexture();
		}

		/// <summary>Optional: clears the in-memory cache (e.g., if you hot-swap art while running).</summary>
		public static void ClearCache() => Cache.Clear();

		// ------------- Internals -------------

		private static Texture2D? ResolveTexture(string baseDir, string raw)
		{
			if (string.IsNullOrWhiteSpace(raw)) return null;

			foreach (var name in BuildCandidates(raw))
			{
				foreach (var ext in Exts)
				{
					var path = $"{baseDir}/{name}{ext}";
					var tex = Load(path);
					if (tex != null) return tex;
				}
			}
			return null;
		}

		private static IEnumerable<string> BuildCandidates(string raw)
		{
			// Common variants: lowercase, hyphens, underscores, no separators, PascalCase
			raw = raw.Trim();
			var lower   = raw.ToLowerInvariant();
			var hyphen  = lower.Replace("_", "-").Replace(" ", "-");
			var unders  = lower.Replace("-", "_").Replace(" ", "_");
			var noSep   = Regex.Replace(lower, @"[\s_\-]", "");
			var pascal  = ToPascal(noSep);

			// Try in order most likely to match hand-named assets
			return new[] { lower, hyphen, unders, noSep, pascal }.Distinct();
		}

		private static string ToPascal(string s)
		{
			if (string.IsNullOrEmpty(s)) return s;
			// If separators slipped through, normalize them
			var parts = Regex.Split(s, @"[\s_\-]+").Where(p => p.Length > 0);
			return string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + (p.Length > 1 ? p.Substring(1) : "")));
		}

		private static string StripTierSuffix(string id)
		{
			// e.g. "spiked-shield-1" -> "spiked-shield"
			if (string.IsNullOrWhiteSpace(id)) return "";
			return Regex.Replace(id.Trim(), @"-(\d+)$", "");
		}

		private static Texture2D? Load(string path)
		{
			if (string.IsNullOrWhiteSpace(path)) return null;
			if (Cache.TryGetValue(path, out var cached)) return cached;

			if (ResourceLoader.Exists(path))
			{
				var tex = ResourceLoader.Load<Texture2D>(path);
				if (tex != null)
				{
					Cache[path] = tex;
					return tex;
				}
			}
			return null;
		}
	}
}
