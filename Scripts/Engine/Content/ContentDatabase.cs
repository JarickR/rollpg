// File: Scripts/Engine/Content/ContentDatabase.cs
using System.Collections.Generic;
using System.Text.Json;

namespace DiceArena.Engine.Content
{
	public static class ContentDatabase
	{
		private static readonly JsonSerializerOptions JsonOpts = new()
		{
			PropertyNameCaseInsensitive = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
			AllowTrailingCommas = true
		};

		public static ContentBundle LoadAll(string classesPath, string spellsPath)
		{
			var classes = Deserialize<List<ClassDef>>(LoadText(classesPath)) ?? new List<ClassDef>();
			var spells  = Deserialize<List<SpellDef>>(LoadText(spellsPath)) ?? new List<SpellDef>();
			return new ContentBundle(classes, spells);
		}

		private static string LoadText(string path)
		{
			// Use fully-qualified Godot namespace to avoid DiceArena.Godot collision.
			if (!global::Godot.FileAccess.FileExists(path))
				return "[]";

			using var f = global::Godot.FileAccess.Open(path, global::Godot.FileAccess.ModeFlags.Read);
			return f.GetAsText();
		}

		private static T? Deserialize<T>(string json) =>
			JsonSerializer.Deserialize<T>(json, JsonOpts);
	}
}
