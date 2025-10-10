using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

namespace RollPG.Content
{
	/// <summary>
	/// Loads JSON from res://Content/Data/* and caches into dictionaries.
	/// Call ContentDB.Init() once at startup (e.g., an autoload or your root scene).
	/// </summary>
	public static class ContentDB
	{
		private static bool _initialized;

		private static Dictionary<string, ClassDef> _classes = new(StringComparer.OrdinalIgnoreCase);
		private static Dictionary<string, SpellDef> _spells  = new(StringComparer.OrdinalIgnoreCase);

		public static IReadOnlyDictionary<string, ClassDef> Classes => _classes;
		public static IReadOnlyDictionary<string, SpellDef> Spells  => _spells;

		public static void Init()
		{
			if (_initialized) return;

			_classes = LoadJsonArray<ClassDef>("res://Content/Data/classes.json", d => d.Id);
			_spells  = LoadJsonArray<SpellDef>("res://Content/Data/spells.json",  d => d.Id);

			_initialized = true;
			GD.Print($"[ContentDB] Loaded: {_classes.Count} classes, {_spells.Count} spells.");
		}

		private static Dictionary<string, T> LoadJsonArray<T>(string resPath, Func<T, string> getKey)
		{
			var dict = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
			using var f = FileAccess.Open(resPath, FileAccess.ModeFlags.Read);
			if (f == null)
			{
				GD.PushError($"[ContentDB] Could not open {resPath}");
				return dict;
			}

			string json = f.GetAsText();
			var options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				ReadCommentHandling = JsonCommentHandling.Skip,
				AllowTrailingCommas = true
			};

			try
			{
				var arr = JsonSerializer.Deserialize<System.Collections.Generic.List<T>>(json, options)
						  ?? new System.Collections.Generic.List<T>();
				foreach (var item in arr)
				{
					var key = getKey(item);
					if (string.IsNullOrWhiteSpace(key))
					{
						GD.PushWarning($"[ContentDB] Skipping item with empty key in {resPath}");
						continue;
					}
					dict[key] = item;
				}
			}
			catch (Exception ex)
			{
				GD.PushError($"[ContentDB] JSON parse error in {resPath}: {ex.Message}");
			}

			return dict;
		}

		public static ClassDef? GetClass(string id) => _classes.TryGetValue(id, out var c) ? c : null;
		public static SpellDef?  GetSpell(string id) => _spells.TryGetValue(id, out var s) ? s : null;

		/// <summary>Fetch all spells for a given tier (1/2/3), sorted by 'order'.</summary>
		public static System.Collections.Generic.List<SpellDef> GetSpellsByTier(int tier)
		{
			var list = new System.Collections.Generic.List<SpellDef>();
			foreach (var s in _spells.Values)
				if (s.Tier == tier) list.Add(s);
			list.Sort((a, b) => a.Order.CompareTo(b.Order));
			return list;
		}
	}
}
