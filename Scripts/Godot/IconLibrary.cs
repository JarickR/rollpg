using Godot;
using System;
using System.Collections.Generic;

namespace DiceArena.GodotUI
{
	/// <summary>
	/// Centralized loader for individual PNG thumbnails (500x500) and misc helpers.
	/// Adjust the folder constants below to match your project paths.
	/// </summary>
	public static class IconLibrary
	{
		// ---------- Folders (adjust to your actual res:// layout) ----------
		private const string ENEMY_T1_DIR = "res://art/enemies/tier1/";
		private const string ENEMY_T2_DIR = "res://art/enemies/tier2/";
		private const string ENEMY_BOSS_DIR = "res://art/enemies/bosses/";

		private const string CLASS_DIR = "res://art/classes/";    // e.g., thief.png, judge.png, …
		private const string SPELL_DIR = "res://art/spells/";     // e.g., attack.png, shield.png, …

		// If you also have status icons:
		private const string STATUS_DIR = "res://art/status/";    // e.g., poison.png, bomb.png, …

		// ---------- Caches ----------
		private static readonly Dictionary<string, Texture2D> _texCache = new();

		// Public transparent 1×1 as a safe fallback
		public static Texture2D Transparent1x1()
		{
			var img = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
			img.Fill(Colors.Transparent);
			return ImageTexture.CreateFromImage(img);
		}

		/// <summary>Safe loader with caching.</summary>
		public static Texture2D LoadTexture(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return Transparent1x1();

			if (_texCache.TryGetValue(path, out var tex))
				return tex;

			var loaded = GD.Load<Texture2D>(path);
			if (loaded != null)
			{
				_texCache[path] = loaded;
				return loaded;
			}
			return Transparent1x1();
		}

		// ---------- Enemy thumbnails (individual files) ----------
		// Declare your files explicitly so we don’t need to scan directories at runtime.
		// Put the exact filenames you have in your project (500x500 PNGs).

		private static readonly string[] T1_ENEMIES =
		{
			ENEMY_T1_DIR + "bat.png",
			ENEMY_T1_DIR + "slime.png",
			ENEMY_T1_DIR + "skeleton.png",
			ENEMY_T1_DIR + "wolf.png",
			ENEMY_T1_DIR + "goblin.png",
		};

		private static readonly string[] T2_ENEMIES =
		{
			ENEMY_T2_DIR + "orc.png",
			ENEMY_T2_DIR + "ogre.png",
			ENEMY_T2_DIR + "wraith.png",
			ENEMY_T2_DIR + "elemental.png",
		};

		private static readonly string[] BOSS_ENEMIES =
		{
			ENEMY_BOSS_DIR + "dragon.png",
			ENEMY_BOSS_DIR + "lich_king.png",
			ENEMY_BOSS_DIR + "behemoth.png",
		};

		public static List<Texture2D> GetEnemiesForTier(int tier)
		{
			var list = new List<Texture2D>();
			var src = tier switch
			{
				1 => T1_ENEMIES,
				2 => T2_ENEMIES,
				3 => BOSS_ENEMIES,
				_ => T1_ENEMIES,
			};

			foreach (var p in src)
				list.Add(LoadTexture(p));

			return list;
		}

		/// <summary>Return a random enemy icon for the tier.</summary>
		public static Texture2D GetRandomEnemyIcon(int tier)
		{
			var pool = GetEnemiesForTier(tier);
			if (pool.Count == 0) return Transparent1x1();

			var rng = new RandomNumberGenerator();
			rng.Randomize();
			var idx = rng.RandiRange(0, pool.Count - 1);
			return pool[idx];
		}

		// ---------- Class logos (individual files) ----------
		// Keys must match your Hero.ClassId / class keys used in UI.

		private static readonly Dictionary<string, string> CLASS_LOGOS = new(StringComparer.OrdinalIgnoreCase)
		{
			["thief"]     = CLASS_DIR + "thief.png",
			["judge"]     = CLASS_DIR + "judge.png",
			["tank"]      = CLASS_DIR + "tank.png",
			["vampire"]   = CLASS_DIR + "vampire.png",
			["king"]      = CLASS_DIR + "king.png",
			["lich"]      = CLASS_DIR + "lich.png",
			["paladin"]   = CLASS_DIR + "paladin.png",
			["barbarian"] = CLASS_DIR + "barbarian.png",
			["bard"]      = CLASS_DIR + "bard.png",
			["warden"]    = CLASS_DIR + "warden.png",
		};

		public static Texture2D GetClassLogoByKey(string classKey)
		{
			if (string.IsNullOrWhiteSpace(classKey)) return Transparent1x1();
			return CLASS_LOGOS.TryGetValue(classKey, out var path) ? LoadTexture(path) : Transparent1x1();
		}

		// ---------- Spell icons (individual files) ----------
		// Map spell keys/names to files; adjust to your exact filenames.

		private static readonly Dictionary<string, string> SPELL_ICONS = new(StringComparer.OrdinalIgnoreCase)
		{
			// Tier 1
			["attack"]   = SPELL_DIR + "attack.png",
			["heart"]    = SPELL_DIR + "heart.png",
			["shield"]   = SPELL_DIR + "shield.png",   // “shield” applies armor (your note)
			["dash"]     = SPELL_DIR + "dash.png",
			["stab"]     = SPELL_DIR + "stab.png",
			["lightning"]= SPELL_DIR + "lightning.png",
			["ice"]      = SPELL_DIR + "ice.png",
			["thorns"]   = SPELL_DIR + "thorns.png",

			// Tier 2
			["bomb"]       = SPELL_DIR + "bomb.png",
			["poison"]     = SPELL_DIR + "poison.png",
			["yin_yang"]   = SPELL_DIR + "yin_yang.png",
			["backstab"]   = SPELL_DIR + "backstab.png",
			["lightning+"] = SPELL_DIR + "lightning_plus.png",
			["freeze"]     = SPELL_DIR + "freeze.png",
			["shield+"]    = SPELL_DIR + "shield_plus.png",
			["thorns+"]    = SPELL_DIR + "thorns_plus.png",

			// Tier 3 (examples)
			["attack++"]   = SPELL_DIR + "attack_plus_plus.png",
			["heart++"]    = SPELL_DIR + "heart_plus_plus.png",
			["shield++"]   = SPELL_DIR + "shield_plus_plus.png",
			["dash++"]     = SPELL_DIR + "dash_plus_plus.png",
			["lightning++"]= SPELL_DIR + "lightning_plus_plus.png",
			["ice++"]      = SPELL_DIR + "ice_plus_plus.png",
			["yin_yang++"] = SPELL_DIR + "yin_yang_plus_plus.png",
			["defensive"]  = SPELL_DIR + "defensive.png",
		};

		public static Texture2D GetSpellIconByName(string key)
		{
			if (string.IsNullOrWhiteSpace(key)) return Transparent1x1();
			return SPELL_ICONS.TryGetValue(key, out var path) ? LoadTexture(path) : Transparent1x1();
		}
	}
}
