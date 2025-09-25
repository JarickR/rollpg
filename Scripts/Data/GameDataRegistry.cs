// File: Scripts/Data/GameDataRegistry.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiceArena.Data
{
	/// <summary>
	/// Centralized, read-only view of game content.
	/// Supports bundles that expose a flat Spells list or tiered spell properties.
	/// </summary>
	public static class GameDataRegistry
	{
		// Use fully-qualified content types to avoid any local type name collisions.
		public static IReadOnlyList<global::DiceArena.Engine.Content.ClassDef> Classes { get; private set; } = Array.Empty<global::DiceArena.Engine.Content.ClassDef>();
		public static IReadOnlyList<global::DiceArena.Engine.Content.SpellDef>  Spells  { get; private set; } = Array.Empty<global::DiceArena.Engine.Content.SpellDef>();

		public static IReadOnlyDictionary<string, global::DiceArena.Engine.Content.ClassDef> ClassById { get; private set; }
			= new Dictionary<string, global::DiceArena.Engine.Content.ClassDef>(StringComparer.OrdinalIgnoreCase);

		public static IReadOnlyDictionary<string, global::DiceArena.Engine.Content.SpellDef> SpellById { get; private set; }
			= new Dictionary<string, global::DiceArena.Engine.Content.SpellDef>(StringComparer.OrdinalIgnoreCase);

		public static IReadOnlyDictionary<int, IReadOnlyList<global::DiceArena.Engine.Content.SpellDef>> SpellsByTier { get; private set; }
			= new Dictionary<int, IReadOnlyList<global::DiceArena.Engine.Content.SpellDef>>();

		public static void Load(global::DiceArena.Engine.Content.ContentBundle bundle)
		{
			if (bundle == null) throw new ArgumentNullException(nameof(bundle));

			// Classes
			var classesLocal = bundle.Classes;
			Classes = classesLocal == null
				? Array.Empty<global::DiceArena.Engine.Content.ClassDef>()
				: (IReadOnlyList<global::DiceArena.Engine.Content.ClassDef>)classesLocal;

			ClassById = Classes
				.GroupBy(c => c.Id, StringComparer.OrdinalIgnoreCase)
				.ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

			// Spells
			var spells = ExtractSpellsFromBundle(bundle);
			Spells = spells
				.OrderBy(s => s.Tier)
				.ThenBy(s => s.Name ?? s.Id)
				.ThenBy(s => s.Id)
				.ToList();

			SpellById = Spells
				.GroupBy(s => s.Id, StringComparer.OrdinalIgnoreCase)
				.ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

			// Buckets 1..3
			var byTier = Spells
				.GroupBy(s => s.Tier)
				.ToDictionary(
					g => g.Key,
					g => (IReadOnlyList<global::DiceArena.Engine.Content.SpellDef>)g
						.OrderBy(x => x.Name ?? x.Id)
						.ThenBy(x => x.Id)
						.ToList()
				);

			for (int t = 1; t <= 3; t++)
				if (!byTier.ContainsKey(t))
					byTier[t] = Array.Empty<global::DiceArena.Engine.Content.SpellDef>();

			SpellsByTier = byTier;
		}

		/// <summary>
		/// Returns a flattened spell list from a bundle without mutating registry state.
		/// </summary>
		public static List<global::DiceArena.Engine.Content.SpellDef> GetAllSpells(global::DiceArena.Engine.Content.ContentBundle bundle)
		{
			if (bundle == null) throw new ArgumentNullException(nameof(bundle));

			// Prefer flat AllSpells
			var flatAll = TryReadEnumerable(bundle, "AllSpells");
			if (flatAll != null)
				return flatAll.OrderBy(s => s.Tier).ThenBy(s => s.Name ?? s.Id).ThenBy(s => s.Id).ToList();

			// Else flat Spells
			var flat = TryReadEnumerable(bundle, "Spells");
			if (flat != null)
				return flat.OrderBy(s => s.Tier).ThenBy(s => s.Name ?? s.Id).ThenBy(s => s.Id).ToList();

			// Else flatten tiered
			return ExtractSpellsFromBundle(bundle)
				.OrderBy(s => s.Tier).ThenBy(s => s.Name ?? s.Id).ThenBy(s => s.Id).ToList();
		}

		public static IReadOnlyList<global::DiceArena.Engine.Content.SpellDef> GetTier(int tier) =>
			SpellsByTier.TryGetValue(tier, out var list) ? list : Array.Empty<global::DiceArena.Engine.Content.SpellDef>();

		// ---- internals ----

		private static IReadOnlyList<global::DiceArena.Engine.Content.SpellDef> ExtractSpellsFromBundle(global::DiceArena.Engine.Content.ContentBundle bundle)
		{
			// Prefer flat 'Spells'
			var propSpells = bundle.GetType().GetProperty("Spells");
			if (propSpells != null &&
				typeof(IEnumerable<global::DiceArena.Engine.Content.SpellDef>).IsAssignableFrom(propSpells.PropertyType))
			{
				var val = (IEnumerable<global::DiceArena.Engine.Content.SpellDef>?)propSpells.GetValue(bundle);
				return (val ?? Enumerable.Empty<global::DiceArena.Engine.Content.SpellDef>()).ToList();
			}

			// Try tiered variants
			var tierNames = new[]
			{
				"SpellsTier1","SpellsTier2","SpellsTier3",
				"SpellsT1","SpellsT2","SpellsT3"
			};

			var collected = new List<global::DiceArena.Engine.Content.SpellDef>(64);
			foreach (var propName in tierNames)
			{
				var p = bundle.GetType().GetProperty(propName);
				if (p == null) continue;

				if (typeof(IEnumerable<global::DiceArena.Engine.Content.SpellDef>).IsAssignableFrom(p.PropertyType))
				{
					var list = (IEnumerable<global::DiceArena.Engine.Content.SpellDef>?)p.GetValue(bundle);
					if (list != null)
						collected.AddRange(list);
				}
			}

			return collected;
		}

		private static List<global::DiceArena.Engine.Content.SpellDef>? TryReadEnumerable(global::DiceArena.Engine.Content.ContentBundle bundle, string property)
		{
			var p = bundle.GetType().GetProperty(property);
			if (p == null) return null;
			if (!typeof(IEnumerable<global::DiceArena.Engine.Content.SpellDef>).IsAssignableFrom(p.PropertyType)) return null;
			var val = (IEnumerable<global::DiceArena.Engine.Content.SpellDef>?)p.GetValue(bundle);
			return val?.ToList();
		}
	}
}
