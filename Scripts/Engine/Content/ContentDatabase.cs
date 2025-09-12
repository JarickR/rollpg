// Scripts/Engine/Content/ContentDatabase.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot; // GD.PrintRich + Godot paths
using DiceArena.Engine.Content;

// Avoid clash with System.IO.FileAccess enum
using GFile = Godot.FileAccess;

namespace DiceArena.Engine.Content
{
	public static class ContentDatabase
	{
		/// <summary>
		/// Load content from a folder (e.g., "res://Content").
		/// Requires "classes.json" and "spells.json".
		/// </summary>
		public static ContentBundle LoadFromFolder(string folderPath)
		{
			GD.PrintRich($"[color=skyblue][Content] LoadFromFolder[/color] folderPath='{folderPath}'");

			// Use the typed array loader so we return List<ClassDef> / List<SpellDef> correctly
			var classes = LoadJsonArray<ClassDef>(folderPath, "classes.json", MapClass);
			var spells  = LoadJsonArray<SpellDef>(folderPath, "spells.json",  MapSpell);

			GD.PrintRich($"[color=skyblue][Content][/color] Loaded counts → classes={classes.Count}, spells={spells.Count}");

			var bundle = new ContentBundle
			{
				Classes = classes,
				Spells  = spells
			};

			Validate(bundle);
			return bundle;
		}

		// ---------- JSON core (works in editor & export) ----------

		/// <summary>
		/// Load a typed array from JSON (expects the file's root to be an array).
		/// Each element is mapped via <paramref name="mapper"/>.
		/// </summary>
		private static List<TItem> LoadJsonArray<TItem>(string folderPath, string filename, Func<JsonElement, TItem> mapper)
		{
			var path = System.IO.Path.Combine(folderPath, filename);
			var usingGodot = UseGodotIO(path);
			GD.PrintRich($"[color=skyblue][Content][/color] Probing file '{filename}' → path='{path}', io={(usingGodot ? "Godot.FileAccess" : "System.IO")}");

			string json;

			try
			{
				if (usingGodot)
				{
					if (!GFile.FileExists(path))
					{
						GD.PrintRich($"[color=yellow][Content][/color] Missing (Godot): {path}");
						return new List<TItem>();
					}

					using var f = GFile.Open(path, GFile.ModeFlags.Read);
					if (f == null)
					{
						GD.PrintRich($"[color=yellow][Content][/color] Could not open (Godot): {path}");
						return new List<TItem>();
					}

					var len = f.GetLength();
					json = f.GetAsText();
					GD.PrintRich($"[color=skyblue][Content][/color] Opened (Godot): {path} size={len} bytes");
				}
				else
				{
					if (!System.IO.File.Exists(path))
					{
						GD.PrintRich($"[color=yellow][Content][/color] Missing (System.IO): {path}");
						return new List<TItem>();
					}

					var len = new System.IO.FileInfo(path).Length;
					json = System.IO.File.ReadAllText(path);
					GD.PrintRich($"[color=skyblue][Content][/color] Opened (System.IO): {path} size={len} bytes");
				}
			}
			catch (Exception exOpen)
			{
				GD.PrintRich($"[color=red][Content Error][/color] Open failed: {path}\n{exOpen.GetType().Name}: {exOpen.Message}");
				return new List<TItem>();
			}

			try
			{
				var doc = JsonSerializer.Deserialize<JsonElement>(json, Options());

				if (doc.ValueKind != JsonValueKind.Array)
				{
					GD.PrintRich($"[color=yellow][Content][/color] JSON root is not an array for '{filename}'. ValueKind={doc.ValueKind}");
					return new List<TItem>();
				}

				var list = new List<TItem>();
				int idx = 0;
				foreach (var el in doc.EnumerateArray())
				{
					try { list.Add(mapper(el)); }
					catch (Exception exRow)
					{
						GD.PrintRich($"[color=yellow][Content][/color] Row parse failed in '{filename}' at index {idx}: {exRow.Message}");
					}
					idx++;
				}
				return list;
			}
			catch (Exception exParse)
			{
				GD.PrintRich($"[color=red][Content Error][/color] JSON parse failed: {filename}\n{exParse.GetType().Name}: {exParse.Message}");
				return new List<TItem>();
			}
		}

		private static bool UseGodotIO(string path)
			=> path.StartsWith("res://", StringComparison.OrdinalIgnoreCase)
			|| path.StartsWith("user://", StringComparison.OrdinalIgnoreCase);

		private static JsonSerializerOptions Options() => new()
		{
			PropertyNameCaseInsensitive = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
			AllowTrailingCommas = true
		};

		// ---------- Mappers (tolerant; captures unknown columns into Extra) ----------
		private static ClassDef MapClass(JsonElement el)
		{
			var d = new ClassDef
			{
				Id         = GetString(el, "id") ?? Slug(GetString(el, "name")),
				Name       = GetString(el, "name") ?? "",
				Trait      = GetString(el, "trait") ?? "",
				HeroAction = GetString(el, "heroAction") ?? GetString(el, "hero_action") ?? ""
			};
			d.Extra = CaptureExtras(el, new[] { "id", "name", "trait", "heroAction", "hero_action" });
			return d;
		}

		private static SpellDef MapSpell(JsonElement el)
		{
			var s = new SpellDef
			{
				Id    = GetString(el, "id") ?? Slug(GetString(el, "name")),
				Name  = GetString(el, "name") ?? "",
				Tier  = GetInt(el, "tier"),
				Kind  = GetString(el, "kind") ?? "",
				Text  = GetString(el, "text") ?? "",
				Order = GetInt(el, "order")
			};
			s.Extra = CaptureExtras(el, new[] { "id", "name", "tier", "kind", "text", "order" });
			return s;
		}

		private static string Slug(string? s)
		{
			if (string.IsNullOrWhiteSpace(s)) return "";
			var res = new System.Text.StringBuilder(s.Length);
			foreach (var c in s.Trim().ToLowerInvariant())
			{
				if (char.IsLetterOrDigit(c)) res.Append(c);
				else if (char.IsWhiteSpace(c) || c == '-' || c == '_') res.Append('_');
			}
			return res.ToString().Trim('_');
		}

		private static string? GetString(JsonElement el, string name)
			=> el.TryGetProperty(name, out var v) && v.ValueKind != JsonValueKind.Null ? v.GetString() : null;

		private static int GetInt(JsonElement el, string name)
		{
			if (!el.TryGetProperty(name, out var v) || v.ValueKind == JsonValueKind.Null) return 0;
			return v.ValueKind switch
			{
				JsonValueKind.Number => v.TryGetInt32(out var i) ? i : 0,
				JsonValueKind.String => int.TryParse(v.GetString(), out var i) ? i : 0,
				_ => 0
			};
		}

		private static Dictionary<string, object?> CaptureExtras(JsonElement el, IEnumerable<string> used)
		{
			var set = new HashSet<string>(used.Select(u => u.ToLowerInvariant()));
			var dict = new Dictionary<string, object?>();
			foreach (var prop in el.EnumerateObject())
			{
				var key = prop.Name.ToLowerInvariant();
				if (!set.Contains(key))
				{
					dict[prop.Name] = prop.Value.ValueKind == JsonValueKind.Null
						? null
						: JsonSerializer.Deserialize<object>(prop.Value.GetRawText(), Options());
				}
			}
			return dict;
		}

		// ---------- Validation (logs warnings, not fatal) ----------
		private static void Validate(ContentBundle bundle)
		{
			void Warn(string m) => GD.PrintRich($"[color=yellow][Content Warning][/color] {m}");

			var dupClasses = bundle.Classes.GroupBy(c => c.Id).Where(g => g.Count() > 1).Select(g => g.Key);
			foreach (var id in dupClasses) Warn($"Duplicate class id: {id}");

			var dupSpells = bundle.Spells.GroupBy(s => s.Id).Where(g => g.Count() > 1).Select(g => g.Key);
			foreach (var id in dupSpells) Warn($"Duplicate spell id: {id}");

			foreach (var c in bundle.Classes.Where(c => string.IsNullOrWhiteSpace(c.Id) || string.IsNullOrWhiteSpace(c.Name)))
				Warn($"Class missing id or name: {c.Name}");

			foreach (var s in bundle.Spells.Where(s => string.IsNullOrWhiteSpace(s.Id) || string.IsNullOrWhiteSpace(s.Name)))
				Warn($"Spell missing id or name: {s.Name}");
		}
	}
}
