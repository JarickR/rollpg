// res://Scripts/Data/ContentDatabaseCompat.cs
#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public static class ContentDatabase
{
	// --------------------------------------------------------------------
	// Legacy init entry points (VOID) — callers that don't use a return value
	// --------------------------------------------------------------------
	public static void LoadAll() => RollPG.Content.ContentDB.Init();

	public static void LoadAll(string _rootFolder)
		=> RollPG.Content.ContentDB.Init();

	// NOTE: We intentionally DO NOT provide a void overload with (string, string)
	// because some call sites expect this signature to RETURN a ContentBundle.

	public static void Init()
		=> RollPG.Content.ContentDB.Init();

	// --------------------------------------------------------------------
	// Legacy init entry points (RETURNING ContentBundle)
	// Matches call sites like:
	//   var bundle = ContentDatabase.LoadAll(classesPath, spellsPath);
	// Your ContentBundle constructor expects:
	//   ContentBundle(IReadOnlyList<DiceArena.Engine.Content.ClassDef>,
	//                 IReadOnlyList<DiceArena.Engine.Content.SpellDef>)
	// We construct those lists via reflection and property mapping.
	// --------------------------------------------------------------------
	public static DiceArena.Engine.Content.ContentBundle LoadAll(string _classesPath, string _spellsPath)
	{
		RollPG.Content.ContentDB.Init();

		// Reflect legacy types
		var asm = typeof(DiceArena.Engine.Content.ContentBundle).Assembly;
		var ns  = "DiceArena.Engine.Content";
		var legacyClassType = asm.GetType($"{ns}.ClassDef", throwOnError: false);
		var legacySpellType = asm.GetType($"{ns}.SpellDef", throwOnError: false);

		if (legacyClassType == null || legacySpellType == null)
			throw new InvalidOperationException("Legacy content types not found (ClassDef/SpellDef).");

		// Build List<legacyClassType> and List<legacySpellType>
		var legacyClassList = CreateGenericList(legacyClassType);
		var legacySpellList = CreateGenericList(legacySpellType);

		// Map RollPG classes -> legacy classes
		foreach (var kv in RollPG.Content.ContentDB.Classes)
		{
			var from = kv.Value; // RollPG.Content.ClassDef
			var to = Activator.CreateInstance(legacyClassType)!;

			// best-effort property copy (no hard dependency on exact names)
			SetIfExists(to, "Id",         from.Id);
			SetIfExists(to, "Name",       from.Name);
			SetIfExists(to, "Trait",      from.Trait);
			SetIfExists(to, "HeroAction", from.HeroAction);

			legacyClassList.Add(to);
		}

		// Map RollPG spells -> legacy spells
		foreach (var kv in RollPG.Content.ContentDB.Spells)
		{
			var from = kv.Value; // RollPG.Content.SpellDef
			var to = Activator.CreateInstance(legacySpellType)!;

			SetIfExists(to, "Id",    from.Id);
			SetIfExists(to, "Name",  from.Name);
			SetIfExists(to, "Tier",  from.Tier);
			SetIfExists(to, "Kind",  from.Kind);
			SetIfExists(to, "Text",  from.Text);
			SetIfExists(to, "Order", from.Order);

			legacySpellList.Add(to);
		}

		// Convert List<T> -> IReadOnlyList<T> (List<T> already implements it)
		var bundle = CreateBundle(legacyClassList, legacySpellList);
		return bundle;
	}

	// Some older code may call with a bool flag to disambiguate; support it too.
	public static DiceArena.Engine.Content.ContentBundle LoadAll(bool _)
		=> LoadAll(string.Empty, string.Empty);

	// --------------------------------------------------------------------
	// Quick readiness check (optional)
	// --------------------------------------------------------------------
	public static bool IsInitialized =>
		RollPG.Content.ContentDB.Classes.Count > 0 || RollPG.Content.ContentDB.Spells.Count > 0;

	// --------------------------------------------------------------------
	// Read-only maps (mirror the old API surface)
	// --------------------------------------------------------------------
	public static IReadOnlyDictionary<string, RollPG.Content.ClassDef> Classes
		=> RollPG.Content.ContentDB.Classes;

	public static IReadOnlyDictionary<string, RollPG.Content.SpellDef> Spells
		=> RollPG.Content.ContentDB.Spells;

	// --------------------------------------------------------------------
	// Lookup helpers (keeping the old method names)
	// --------------------------------------------------------------------
	public static RollPG.Content.ClassDef? GetClass(string id)
		=> RollPG.Content.ContentDB.GetClass(id);

	public static RollPG.Content.SpellDef? GetSpell(string id)
		=> RollPG.Content.ContentDB.GetSpell(id);

	public static List<RollPG.Content.SpellDef> GetSpellsByTier(int tier)
		=> RollPG.Content.ContentDB.GetSpellsByTier(tier);

	// ====================================================================
	// Helpers
	// ====================================================================

	private static IList CreateGenericList(Type t)
	{
		var listType = typeof(List<>).MakeGenericType(t);
		return (IList)Activator.CreateInstance(listType)!;
	}

	private static void SetIfExists(object target, string propName, object? value)
	{
		var p = target.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
		if (p != null && p.CanWrite)
		{
			try { p.SetValue(target, value); } catch { /* ignore */ }
		}
		else
		{
			var f = target.GetType().GetField(propName, BindingFlags.Public | BindingFlags.Instance);
			if (f != null) { try { f.SetValue(target, value); } catch { /* ignore */ } }
		}
	}

	private static DiceArena.Engine.Content.ContentBundle CreateBundle(IList legacyClassList, IList legacySpellList)
	{
		// Find ctor: ContentBundle(IReadOnlyList<ClassDef>, IReadOnlyList<SpellDef>)
		var bundleType = typeof(DiceArena.Engine.Content.ContentBundle);
		var ctors = bundleType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

		foreach (var ctor in ctors)
		{
			var ps = ctor.GetParameters();
			if (ps.Length == 2 &&
				ps[0].ParameterType.IsGenericType &&
				ps[0].ParameterType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>) &&
				ps[1].ParameterType.IsGenericType &&
				ps[1].ParameterType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
			{
				var expectedClassElem = ps[0].ParameterType.GetGenericArguments()[0];
				var expectedSpellElem = ps[1].ParameterType.GetGenericArguments()[0];

				if (ListElementType(legacyClassList) == expectedClassElem &&
					ListElementType(legacySpellList) == expectedSpellElem)
				{
					var instance = ctor.Invoke(new object[] { legacyClassList, legacySpellList });
					return (DiceArena.Engine.Content.ContentBundle)instance;
				}
			}
		}

		// Fallback: try a parameterless ctor if available (empty bundle)
		var emptyCtor = bundleType.GetConstructor(Type.EmptyTypes);
		if (emptyCtor != null)
		{
			return (DiceArena.Engine.Content.ContentBundle)emptyCtor.Invoke(Array.Empty<object>());
		}

		throw new MissingMethodException("No compatible ContentBundle constructor found.");
	}

	private static Type? ListElementType(IList list)
	{
		var t = list.GetType();
		return t.IsGenericType ? t.GetGenericArguments()[0] : null;
	}
}
