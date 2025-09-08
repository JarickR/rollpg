// res://Scripts/Data/GameDataRegistry.cs
#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DiceArena.Data;
using DiceArena.Engine;

namespace DiceArena.GameData
{
	/// <summary>
	/// Loads data from res://Data/*.json and exposes helpers to fetch classes/spells and convert to Engine.Spell.
	/// </summary>
	public static class GameDataRegistry
	{
		private static bool _loaded;
		private static readonly Dictionary<string, ClassDef> _classes = new();
		private static readonly List<SpellDef> _spells = new();

		/// <summary>Call once at boot (or lazy on first access).</summary>
		public static void EnsureLoaded()
		{
			if (_loaded) return;

			try
			{
				// Classes
				var classesJson = ReadAllText("res://Data/classes.json");
				var classList = JsonSerializer.Deserialize<List<ClassDef>>(classesJson, JsonOpts()) ?? new();
				foreach (var c in classList)
				{
					if (!string.IsNullOrWhiteSpace(c.Id))
						_classes[c.Id.ToLowerInvariant()] = c;
				}

				// Spells
				var spellsJson = ReadAllText("res://Data/spells.json");
				var spellList = JsonSerializer.Deserialize<List<SpellDef>>(spellsJson, JsonOpts()) ?? new();
				_spells.Clear();
				_spells.AddRange(spellList);

				_loaded = true;
				GD.Print($"[GameDataRegistry] Loaded {_classes.Count} classes, {_spells.Count} spells");
			}
			catch (Exception ex)
			{
				GD.PushError($"[GameDataRegistry] Load failed: {ex}");
				_loaded = true; // avoid retry loop; still usable if empty
			}
		}

		public static IEnumerable<ClassDef> GetClasses()
		{
			EnsureLoaded();
			return _classes.Values;
		}

		public static ClassDef? GetClassById(string id)
		{
			EnsureLoaded();
			_classes.TryGetValue(id.ToLowerInvariant(), out var c);
			return c;
		}

		public static IEnumerable<SpellDef> GetSpellsByTier(int tier)
		{
			EnsureLoaded();
			return _spells.Where(s => s.Tier == tier);
		}

		public static IEnumerable<SpellDef> GetDefensiveSpells()
		{
			EnsureLoaded();
			return _spells.Where(s => s.Tags.Any(t => string.Equals(t, "defensive", StringComparison.OrdinalIgnoreCase)));
		}

		/// <summary>Convert a SpellDef to an Engine.Spell using the factory.</summary>
		public static Spell ToEngineSpell(SpellDef def)
		{
			return SpellFactory.FromKind(def.Kind, def.Tier, def.Name);
		}

		/// <summary>Pick a random defensive Engine.Spell.</summary>
		public static Spell RandomDefensive(Random rng)
		{
			var defs = GetDefensiveSpells().ToList();
			if (defs.Count == 0) return SpellFactory.FromKind("armor", 1, "Armor"); // fallback
			var pick = defs[rng.Next(defs.Count)];
			return ToEngineSpell(pick);
		}

		// ---------- helpers ----------
		private static string ReadAllText(string path)
		{
			using var fa = FileAccess.Open(path, FileAccess.ModeFlags.Read);
			if (fa == null) throw new Exception($"FileAccess.Open failed for {path}");
			return fa.GetAsText();
		}

		private static JsonSerializerOptions JsonOpts() => new()
		{
			PropertyNameCaseInsensitive = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
			AllowTrailingCommas = true
		};
	}
}
