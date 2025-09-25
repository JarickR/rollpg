using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

using EngineModels = DiceArena.Engine.Content;

namespace DiceArena.Data
{
	/// <summary>
	/// Minimal runtime adapter that reads res://Content/*.json and projects
	/// to DiceArena.Engine.Content models with a forgiving JSON reader.
	/// </summary>
	public static class ContentDatabaseCompat
	{
		public static ContentBundle LoadOrCreate()
		{
			// Your content lives under the Content folder (per your screenshot).
			const string classesPath = "res://Content/classes.json";
			const string spellsPath  = "res://Content/spells.json";

			// Reuse your existing loader that takes both paths.
			return ContentDatabase.LoadAll(classesPath, spellsPath);
		}

		// Cached content
		private static List<EngineModels.ClassDef> _classes = new();
		private static List<EngineModels.SpellDef> _spells  = new();
		private static bool _loaded;

		// ---------- Public API ----------

		public static EngineModels.ContentBundle LoadFromProjectDefaults()
			=> LoadAll("res://Content/classes.json", "res://Content/spells.json");

		public static EngineModels.ContentBundle LoadAll(string classesPath, string spellsPath)
		{
			_classes = ReadClasses(classesPath);
			_spells  = ReadSpells(spellsPath);
			_loaded  = true;

			GD.Print($"[ContentCompat] Loaded classes={_classes.Count}, spells={_spells.Count}");

			return new EngineModels.ContentBundle(_classes, _spells);
		}

		public static IReadOnlyList<EngineModels.ClassDef> GetClasses(EngineModels.ContentBundle _)
		{
			EnsureLoaded();
			return _classes;
		}

		public static IReadOnlyList<EngineModels.SpellDef> GetTier1Spells(EngineModels.ContentBundle _)
		{
			EnsureLoaded();
			return _spells.Where(s => s.Tier == 1).ToList();
		}

		public static IReadOnlyList<EngineModels.SpellDef> GetTier2Spells(EngineModels.ContentBundle _)
		{
			EnsureLoaded();
			return _spells.Where(s => s.Tier == 2).ToList();
		}

		// ---------- JSON rows (engine-agnostic) ----------

		private sealed class ClassRow
		{
			[JsonPropertyName("id")]   public string Id   { get; set; } = "";
			[JsonPropertyName("name")] public string Name { get; set; } = "";
		}

		private sealed class SpellRow
		{
			[JsonPropertyName("id")]   public string Id   { get; set; } = "";
			[JsonPropertyName("name")] public string Name { get; set; } = "";
			[JsonPropertyName("tier")] public int    Tier { get; set; }
		}

		private sealed class ClassesFile
		{
			[JsonPropertyName("Classes")] public List<ClassRow> Classes { get; set; } = new();
		}
		private sealed class SpellsFile
		{
			[JsonPropertyName("Spells")] public List<SpellRow> Spells { get; set; } = new();
		}

		// ---------- Readers (tolerant to shape) ----------

		private static List<EngineModels.ClassDef> ReadClasses(string path)
		{
			var json = LoadText(path);
			var rows = ParseArrayOrWrapped<ClassRow, ClassesFile>(json, w => w.Classes);

			var list = new List<EngineModels.ClassDef>(rows.Count);
			foreach (var r in rows)
			{
				if (string.IsNullOrWhiteSpace(r.Id)) continue;
				list.Add(new EngineModels.ClassDef { Id = r.Id, Name = r.Name });
			}
			return list;
		}

		private static List<EngineModels.SpellDef> ReadSpells(string path)
		{
			var json = LoadText(path);
			var rows = ParseArrayOrWrapped<SpellRow, SpellsFile>(json, w => w.Spells);

			var list = new List<EngineModels.SpellDef>(rows.Count);
			foreach (var r in rows)
			{
				if (string.IsNullOrWhiteSpace(r.Id)) continue;
				list.Add(new EngineModels.SpellDef { Id = r.Id, Name = r.Name, Tier = r.Tier });
			}
			return list;
		}

		/// <summary>
		/// If the JSON is a top-level array, deserialize it as List&lt;TItem&gt;.
		/// If it's an object wrapper, deserialize TWrap and extract a list with selector.
		/// </summary>
		private static List<TItem> ParseArrayOrWrapped<TItem, TWrap>(string json, Func<TWrap, List<TItem>> fromWrap)
		{
			try
			{
				using var doc = JsonDocument.Parse(json);
				if (doc.RootElement.ValueKind == JsonValueKind.Array)
				{
					return JsonSerializer.Deserialize<List<TItem>>(json) ?? new();
				}
				else
				{
					var wrap = JsonSerializer.Deserialize<TWrap>(json);
					return wrap != null ? (fromWrap(wrap) ?? new()) : new();
				}
			}
			catch (Exception e)
			{
				GD.PushWarning($"[ContentCompat] JSON parse error: {e.Message}");
				return new();
			}
		}

		private static string LoadText(string resPath)
		{
			using var fa = global::Godot.FileAccess.Open(resPath, global::Godot.FileAccess.ModeFlags.Read);
			return fa.GetAsText();
		}

		private static void EnsureLoaded()
		{
			if (_loaded) return;
			LoadFromProjectDefaults();
		}
	}
}
