// Scripts/Godot/IconLibrary.cs
#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using DiceArena.Engine; // for Spell overload

namespace DiceArena.GodotUI
{
	/// <summary>
	/// Central texture helper for class/spell vertical strips, enemy grid sheets, and single status icons.
	/// Includes backward-compatible overloads used across the project.
	/// </summary>
	public static class IconLibrary
	{
		// ---------- FILE PATHS ----------
		public const string TIER1_SPELLS = "res://Tier1Spells.png";
		public const string TIER2_SPELLS = "res://Tier2Spells.png";
		public const string TIER3_SPELLS = "res://Tier3Spells.png";

		public const string CLASS_LOGOS  = "res://class-logos.png";

		public const string TIER1_ENEMIES = "res://Tier1Enemies.png";
		public const string TIER2_ENEMIES = "res://Tier2Enemies.png";
		public const string BOSSES        = "res://Bosses.png";

		public const string POISON_ICON   = "res://PoisonIcon.png";
		public const string BOMB_ICON     = "res://BombIcon.png";
		public const string BLEED_ICON    = "res://BleedIcon.png";
		public const string GHOUL_ICON    = "res://GhoulIcon.png";
		public const string THORN_ICON    = "res://ThornIcon.png";
		public const string DEFENSE_ICON  = "res://DefensiveLogo.png";

		// ---------- DEFAULT COUNTS (adjust to match your sheets) ----------
		public const int DEFAULT_CLASS_COUNT = 10;

		// Tier orders should match your sprite strips:
		// Tier1: Attack, Heal, Shield, Dash, Stab, Lightning, Freeze, Thorns
		public const int DEFAULT_T1_COUNT = 8;

		// Update these to match your actual Tier2/Tier3 sheet lengths:
		public const int DEFAULT_T2_COUNT = 10;
		public const int DEFAULT_T3_COUNT = 8;

		// Enemy grid geometry (5 columns x 4 rows)
		public const int ENEMY_GRID_COLS = 5;
		public const int ENEMY_GRID_ROWS = 4;

		// ---------- CACHES ----------
		private static readonly Dictionary<string, Texture2D> _sheetCache = new();
		private static readonly Dictionary<(string,int,int,int), AtlasTexture> _gridCache = new();
		private static readonly Dictionary<(string,int), AtlasTexture> _stripCache = new();

		// ============================================================
		// ==============  VERTICAL STRIP (generic)  ==================
		// ============================================================
		public static AtlasTexture GetIconFromStrip(string sheetPath, int count, int index)
		{
			if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (index < 0 || index >= count) throw new ArgumentOutOfRangeException(nameof(index));

			var key = (sheetPath, index);
			if (_stripCache.TryGetValue(key, out var cached))
				return cached;

			var sheet = Load(sheetPath);
			int fullW = sheet.GetWidth();
			int fullH = sheet.GetHeight();

			int sliceH = Math.Max(1, fullH / count);
			int y = sliceH * index;
			if (index == count - 1)
				sliceH = Math.Max(1, fullH - y); // ensure last slice reaches bottom

			var at = new AtlasTexture
			{
				Atlas = sheet,
				Region = new Rect2(0, y, fullW, sliceH),
				FilterClip = true
			};
			_stripCache[key] = at;
			return at;
		}

		// ============================================================
		// ===================  SPELLS (vertical)  ====================
		// ============================================================
		public static AtlasTexture GetSpellIcon(int tier, int count, int index) =>
			tier switch
			{
				1 => GetIconFromStrip(TIER1_SPELLS, count, index),
				2 => GetIconFromStrip(TIER2_SPELLS, count, index),
				3 => GetIconFromStrip(TIER3_SPELLS, count, index),
				_ => throw new ArgumentOutOfRangeException(nameof(tier), "Tier must be 1, 2, or 3.")
			};

		public static AtlasTexture GetSpellIcon(int tier, int index) =>
			tier switch
			{
				1 => GetIconFromStrip(TIER1_SPELLS, DEFAULT_T1_COUNT, index),
				2 => GetIconFromStrip(TIER2_SPELLS, DEFAULT_T2_COUNT, index),
				3 => GetIconFromStrip(TIER3_SPELLS, DEFAULT_T3_COUNT, index),
				_ => throw new ArgumentOutOfRangeException(nameof(tier), "Tier must be 1, 2, or 3.")
			};

		/// <summary>
		/// Back-compat overload: single argument. Returns Tier-1 icon at given index.
		/// </summary>
		public static AtlasTexture GetSpellIcon(int index) =>
			GetIconFromStrip(TIER1_SPELLS, DEFAULT_T1_COUNT, index);

		/// <summary>
		/// Accept a Spell object, attempting to resolve tier/index via properties or name.
		/// </summary>
		public static Texture2D GetSpellIcon(Spell spell)
		{
			if (spell is null) return GetDefensiveLogo();

			// Try numeric properties first
			int tier = TryGetInt(spell, "Tier")
					?? TryGetInt(spell, "SpellTier")
					?? TryGetInt(spell, "Level")
					?? 1;

			int? idx = TryGetInt(spell, "IconIndex")
					?? TryGetInt(spell, "SpriteIndex")
					?? TryGetInt(spell, "IconId")
					?? TryGetInt(spell, "Index")
					?? TryGetInt(spell, "Icon");

			if (idx.HasValue)
				return GetSpellIcon(NormalizeTier(tier), idx.Value);

			// Fall back to name lookup
			var byName = GetSpellIconByName(spell.Name);
			return byName ?? GetDefensiveLogo();
		}

		// ============================================================
		// ===================  CLASS LOGOS (vertical)  ===============
		// ============================================================
		public static AtlasTexture GetClassLogo(int index) =>
			GetIconFromStrip(CLASS_LOGOS, DEFAULT_CLASS_COUNT, index);

		public static AtlasTexture GetClassLogo(int count, int index) =>
			GetIconFromStrip(CLASS_LOGOS, count, index);

		/// <summary>Lookup a class logo by string key ("barbarian", "thief", ...).</summary>
		public static Texture2D? GetClassLogoByKey(string? classId)
		{
			if (string.IsNullOrWhiteSpace(classId)) return null;

			switch (classId.Trim().ToLowerInvariant())
			{
				case "thief":       return GetClassLogo(0);
				case "judge":       return GetClassLogo(1);
				case "knight":      return GetClassLogo(2);
				case "vampire":     return GetClassLogo(3);
				case "king":        return GetClassLogo(4);
				case "necromancer": return GetClassLogo(5);
				case "priest":      return GetClassLogo(6);
				case "barbarian":   return GetClassLogo(7);
				case "druid":       return GetClassLogo(8);
				case "bard":        return GetClassLogo(9);
				default:            return GetClassLogo(0);
			}
		}

		// ============================================================
		// ======================  ENEMIES (grid)  ====================
		// ============================================================
		public static AtlasTexture GetIconFromGrid(string sheetPath, int cols, int rows, int index)
		{
			if (cols <= 0 || rows <= 0) throw new ArgumentOutOfRangeException("cols/rows must be > 0");
			int total = cols * rows;
			if (index < 0 || index >= total) throw new ArgumentOutOfRangeException(nameof(index));

			var key = (sheetPath, cols, rows, index);
			if (_gridCache.TryGetValue(key, out var cached))
				return cached;

			var sheet = Load(sheetPath);
			int fullW = sheet.GetWidth();
			int fullH = sheet.GetHeight();

			int cellW = Math.Max(1, fullW / cols);
			int cellH = Math.Max(1, fullH / rows);

			int r = index / cols;
			int c = index % cols;

			int x = c * cellW;
			int y = r * cellH;

			if (c == cols - 1) cellW = Math.Max(1, fullW - x);
			if (r == rows - 1) cellH = Math.Max(1, fullH - y);

			var at = new AtlasTexture
			{
				Atlas = sheet,
				Region = new Rect2(x, y, cellW, cellH),
				FilterClip = true
			};
			_gridCache[key] = at;
			return at;
		}

		/// <summary>Enemy frame by numeric tier (1=T1, 2=T2, 3=Bosses) and index.</summary>
		public static AtlasTexture GetEnemyFrame(int tier, int index) =>
			tier switch
			{
				1 => GetIconFromGrid(TIER1_ENEMIES, ENEMY_GRID_COLS, ENEMY_GRID_ROWS, index),
				2 => GetIconFromGrid(TIER2_ENEMIES, ENEMY_GRID_COLS, ENEMY_GRID_ROWS, index),
				3 => GetIconFromGrid(BOSSES,        ENEMY_GRID_COLS, ENEMY_GRID_ROWS, index),
				_ => throw new ArgumentOutOfRangeException(nameof(tier), "Tier must be 1, 2, or 3.")
			};

		/// <summary>
		/// Back-compat overload: accepts "T1"/"T2"/"T3" (or "1"/"2"/"3") for tier.
		/// </summary>
		public static AtlasTexture GetEnemyFrame(string tier, int index)
		{
			if (string.IsNullOrWhiteSpace(tier))
				throw new ArgumentNullException(nameof(tier));

			string t = tier.Trim().ToUpperInvariant();
			if (t.StartsWith("T")) t = t[1..];

			if (!int.TryParse(t, out int numeric) || numeric is < 1 or > 3)
				throw new ArgumentOutOfRangeException(nameof(tier), "Tier must be T1/T2/T3 or 1/2/3.");

			return GetEnemyFrame(numeric, index);
		}

		// ============================================================
		// =======================  SINGLE ICONS  ======================
		// ============================================================
		public static Texture2D GetPoisonIcon()    => Load(POISON_ICON);
		public static Texture2D GetBombIcon()      => Load(BOMB_ICON);
		public static Texture2D GetBleedIcon()     => Load(BLEED_ICON);
		public static Texture2D GetGhoulIcon()     => Load(GHOUL_ICON);
		public static Texture2D GetThornIcon()     => Load(THORN_ICON);
		public static Texture2D GetDefensiveLogo() => Load(DEFENSE_ICON);

		// ============================================================
		// ===================  NAME LOOKUPS ==========================
		// ============================================================
		/// <summary>
		/// Lookup a spell icon by its name string across tiers.
		/// Edit arrays so the order matches your sprite strips.
		/// </summary>
		public static Texture2D? GetSpellIconByName(string? name)
		{
			if (string.IsNullOrWhiteSpace(name)) return null;
			string key = name.Trim().ToLowerInvariant();

			// Tier 1
			string[] t1 = { "attack", "heal", "shield", "dash", "stab", "lightning", "freeze", "thorns" };
			int idx = Array.FindIndex(t1, n => key.Contains(n));
			if (idx >= 0) return GetSpellIcon(1, idx);

			// Tier 2 (adjust to match your Tier2Spells.png order)
			string[] t2 = { "fireball", "bomb", "chain", "cone", "poison", "bleed", "shock", "ice", "smite", "barrier" };
			idx = Array.FindIndex(t2, n => key.Contains(n));
			if (idx >= 0) return GetSpellIcon(2, idx);

			// Tier 3 (adjust to match your Tier3Spells.png order)
			string[] t3 = { "meteor", "storm", "avalanche", "doom", "cataclysm", "inferno", "blizzard", "quake" };
			idx = Array.FindIndex(t3, n => key.Contains(n));
			if (idx >= 0) return GetSpellIcon(3, idx);

			return null;
		}

		// ============================================================
		// =========================  UTIL  ============================
		// ============================================================
		private static Texture2D Load(string path)
		{
			if (_sheetCache.TryGetValue(path, out var tex))
				return tex;

			var loaded = ResourceLoader.Load<Texture2D>(path);
			if (loaded is null)
				throw new InvalidOperationException($"Failed to load texture at {path}.");
			_sheetCache[path] = loaded;
			return loaded;
		}

		private static int NormalizeTier(int t) => (t < 1 || t > 3) ? 1 : t;

		// Reflection helpers so we work with different Spell model shapes
		private static int? TryGetInt(object obj, string prop)
		{
			var p = obj.GetType().GetProperty(prop, BindingFlags.Public | BindingFlags.Instance);
			if (p == null) return null;
			var val = p.GetValue(obj);
			if (val == null) return null;
			try { return Convert.ToInt32(val); } catch { return null; }
		}
	}
}
