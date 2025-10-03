// Scripts/Data/GameDataRegistry.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace DiceArena.Data
{
	/// <summary>
	/// Loads classes.json + spells.json from the Content folder and exposes simple queries.
	/// Safe to call LoadAll() many times; it loads only once.
	/// </summary>
	public static class GameDataRegistry
	{
		private static readonly List<ClassData> _classes = new();
		private static readonly List<SpellData> _spells  = new();
		private static bool _loaded;

		// 🔒 Content-only locations (put your JSON in one of these).
		private const string ClassesPrimaryPath = "res://Content/Data/classes.json";
		private const string SpellsPrimaryPath  = "res://Content/Data/spells.json";

		// Optional fallback within Content if you prefer a flatter structure.
		private static readonly string[] ClassesFallbacks =
		{
			"res://Content/classes.json"
		};

		private static readonly string[] SpellsFallbacks =
		{
			"res://Content/spells.json"
		};

		public static void LoadAll()
		{
			if (_loaded) return;

			_classes.Clear();
			_spells.Clear();

			LoadJsonList(FindContentPath(ClassesPrimaryPath, ClassesFallbacks), _classes);
			LoadJsonList(FindContentPath(SpellsPrimaryPath, SpellsFallbacks), _spells);

			GD.Print($"[Registry] (Content) classes={_classes.Count}, spells={_spells.Count}");
			_loaded = true;
		}

		public static IEnumerable<ClassData> GetAllClasses()
		{
			if (!_loaded) LoadAll();
			return _classes;
		}

		public static IEnumerable<SpellData> GetSpellsByTier(int tier)
		{
			if (!_loaded) LoadAll();
			return _spells.Where(s => s.Tier == tier);
		}

		// --- internals -------------------------------------------------------------

		private static string? FindContentPath(string primary, string[] fallbacks)
		{
			if (FileAccess.FileExists(primary)) return primary;
			foreach (var f in fallbacks)
				if (FileAccess.FileExists(f)) return f;

			GD.PushWarning($"[Registry] No JSON found under Content for: {primary}");
			return null;
		}

		private static void LoadJsonList<T>(string? path, List<T> target)
		{
			if (string.IsNullOrEmpty(path)) return;

			try
			{
				using var f = FileAccess.Open(path, FileAccess.ModeFlags.Read);
				var json = f.GetAsText();

				var opts = new JsonSerializerOptions
				{
					ReadCommentHandling = JsonCommentHandling.Skip,
					AllowTrailingCommas = true,
					PropertyNameCaseInsensitive = true
				};

				var list = JsonSerializer.Deserialize<List<T>>(json, opts);
				if (list != null) target.AddRange(list);

				GD.Print($"[Registry] Loaded {typeof(T).Name} x{target.Count} from {path}");
			}
			catch (Exception ex)
			{
				GD.PushError($"[Registry] Failed loading {typeof(T).Name} from {path}: {ex.Message}");
			}
		}
	}
}
