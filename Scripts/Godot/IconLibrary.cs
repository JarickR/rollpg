using Godot;
using System;
using System.Collections.Generic;
using DiceArena.Engine; // for Spell

namespace DiceArena.GodotUI
{
	/// <summary>
	/// Centralized loader + sprite-sheet slicer for enemies, classes, and spells.
	/// Supports vertical strips (rows) and grid sheets.
	/// </summary>
	public static class IconLibrary
	{
		// ------------------ ART PATHS (adjust if needed) ------------------
		private const string ClassLogosPath = "res://art/class-logos.png"; // 10 rows

		private const string Tier1SpellsPath = "res://art/Tier1Spells.png"; // vertical strip
		private const string Tier2SpellsPath = "res://art/Tier2Spells.png"; // vertical strip
		private const string Tier3SpellsPath = "res://art/Tier3Spells.png"; // vertical strip

		// Optional enemy sheets (if/when you add them as grids)
		private const string Tier1EnemiesPath = "res://art/Tier1Enemies.png";
		private const string Tier2EnemiesPath = "res://art/Tier2Enemies.png";
		private const string BossesPath       = "res://art/Bosses.png";

		private const int ENEMY_GRID_COLS = 5;
		private const int ENEMY_GRID_ROWS = 4;

		// ------------------ CACHES ------------------
		private static readonly Dictionary<string, Texture2D> _texCache   = new();
		private static readonly Dictionary<string, AtlasTexture> _atlasCache = new();

		// ------------------ UTILITIES ------------------
		private static Texture2D Transparent1x1()
		{
			var img = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
			img.Fill(Colors.Transparent);
			return ImageTexture.CreateFromImage(img);
		}

		public static Texture2D MakePlaceholderTexture(int w, int h, Color color)
		{
			var img = Image.CreateEmpty(Math.Max(1, w), Math.Max(1, h), false, Image.Format.Rgba8);
			img.Fill(color);
			return ImageTexture.CreateFromImage(img);
		}

		/// <summary>Safe Texture2D loader with caching.</summary>
		public static Texture2D LoadTexture(string path)
		{
			if (_texCache.TryGetValue(path, out var t))
				return t;

			var loaded = GD.Load<Texture2D>(path);
			if (loaded != null)
				_texCache[path] = loaded;

			return loaded ?? Transparent1x1();
		}

		private static Texture2D GetFrameFromGridSheet(string sheetPath, int cols, int rows, int frameIndex)
		{
			var sheet = LoadTexture(sheetPath);

			// In some builds GetSize() returns Vector2 (float); cast explicitly.
			var size = sheet.GetSize();
			int sheetW = (int)MathF.Round(size.X);
			int sheetH = (int)MathF.Round(size.Y);

			cols = Math.Max(1, cols);
			rows = Math.Max(1, rows);

			int tileW = Math.Max(1, sheetW / cols);
			int tileH = Math.Max(1, sheetH / rows);

			int total = cols * rows;
			int idx = ((frameIndex % total) + total) % total;

			int col = idx % cols;
			int row = idx / cols;

			var region = new Rect2(col * tileW, row * tileH, tileW, tileH);
			string key = $"{sheetPath}|G|{region.Position.X},{region.Position.Y},{region.Size.X},{region.Size.Y}";

			if (_atlasCache.TryGetValue(key, out var cached))
				return cached;

			var atlas = new AtlasTexture { Atlas = sheet, Region = region };
			_atlasCache[key] = atlas;
			return atlas;
		}

		private static Texture2D GetFrameFromVerticalStrip(string sheetPath, int rowIndex, int rows)
		{
			var sheet = LoadTexture(sheetPath);

			// Explicit casts again (float -> int).
			var size = sheet.GetSize();
			int sheetW = (int)MathF.Round(size.X);
			int sheetH = (int)MathF.Round(size.Y);

			rows = Math.Max(1, rows);
			int tileH = Math.Max(1, sheetH / rows);
			int row = ((rowIndex % rows) + rows) % rows;

			var region = new Rect2(0, row * tileH, sheetW, tileH);
			string key = $"{sheetPath}|V|{row}|{rows}|{sheetW}x{tileH}";

			if (_atlasCache.TryGetValue(key, out var cached))
				return cached;

			var atlas = new AtlasTexture { Atlas = sheet, Region = region };
			_atlasCache[key] = atlas;
			return atlas;
		}

		// ------------------ ENEMIES (optional grid) ------------------
		public static Texture2D GetEnemyFrame(int tier, int frameIndex)
		{
			string sheetPath = tier switch
			{
				1 => Tier1EnemiesPath,
				2 => Tier2EnemiesPath,
				3 => BossesPath,
				_ => Tier1EnemiesPath
			};

			return GetFrameFromGridSheet(sheetPath, ENEMY_GRID_COLS, ENEMY_GRID_ROWS, frameIndex);
		}

		// ------------------ CLASSES (vertical strip with 10 rows) ------------------
		/// <summary>
		/// Order (0-based rows): Thief, Judge, Tank, Vampire, King, Lich, Paladin, Barbarian, Warden, Bard
		/// </summary>
		public static Texture2D GetClassLogoByKey(string key)
		{
			if (string.IsNullOrWhiteSpace(key))
				return MakePlaceholderTexture(64, 64, Colors.Gray);

			var name = key.Trim().ToLowerInvariant();

			var indexByName = new Dictionary<string, int>
			{
				["thief"]     = 0,
				["judge"]     = 1,
				["tank"]      = 2,
				["vampire"]   = 3,
				["king"]      = 4,
				["lich"]      = 5,
				["paladin"]   = 6,
				["barbarian"] = 7,
				["warden"]    = 8,
				["bard"]      = 9
			};

			if (indexByName.TryGetValue(name, out int idx))
				return GetFrameFromVerticalStrip(ClassLogosPath, idx, 10);

			GD.PrintErr($"IconLibrary: Unknown class '{key}'.");
			return MakePlaceholderTexture(64, 64, Colors.DimGray);
		}

		// ------------------ SPELLS (vertical strips) ------------------
		public static Texture2D GetSpellIcon(int tier, int index, int rows)
		{
			string path = tier switch
			{
				1 => Tier1SpellsPath,
				2 => Tier2SpellsPath,
				3 => Tier3SpellsPath,
				_ => Tier1SpellsPath
			};
			return GetFrameFromVerticalStrip(path, index, rows);
		}

		public static Texture2D GetSpellIcon(Spell spell)
		{
			if (spell == null)
				return MakePlaceholderTexture(56, 56, Colors.DarkSlateGray);

			var name = (spell.Name ?? "").Trim().ToLowerInvariant();

			var t1 = new Dictionary<string, int>
			{
				["attack"]   = 0, ["sword"] = 0,
				["heart"]    = 1, ["heal"]  = 1,
				["shield"]   = 2,
				["dash"]     = 3,
				["stab"]     = 4, ["dagger"] = 4,
				["lightning"]= 5,
				["ice"]      = 6, ["freeze"] = 6,
				["thorns"]   = 7
			};

			var t2 = new Dictionary<string, int>
			{
				["attack+"]   = 0,
				["heart+"]    = 1,
				["shield+"]   = 2,
				["yinyang"]   = 3, ["yin yang"] = 3,
				["fire"]      = 4,
				["poison"]    = 5,
				["bomb"]      = 6,
				["backstab"]  = 7
			};

			var t3 = new Dictionary<string, int>
			{
				["attack++"]   = 0,
				["heart++"]    = 1,
				["shield++"]   = 2,
				["dash++"]     = 3,
				["dagger++"]   = 4,
				["lightning++"]= 5,
				["ice++"]      = 6,
				["thorns++"]   = 7
			};

			if (t1.TryGetValue(name, out var j1)) return GetSpellIcon(1, j1, 8);
			if (t2.TryGetValue(name, out var j2)) return GetSpellIcon(2, j2, 8);
			if (t3.TryGetValue(name, out var j3)) return GetSpellIcon(3, j3, 8);

			GD.PrintErr($"IconLibrary: Unknown spell '{spell.Name}'.");
			return MakePlaceholderTexture(56, 56, Colors.DarkSlateGray);
		}

		public static Texture2D GetSpellIconByName(string spellName)
		{
			var s = new Spell { Name = spellName };
			return GetSpellIcon(s);
		}
	}
}
