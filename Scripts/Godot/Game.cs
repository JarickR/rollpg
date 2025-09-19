using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using DiceArena.Engine.Content;
using DiceArena.Engine.Loadout;

namespace DiceArena.Godot
{
	public partial class Game : Control
	{
		[Export] public NodePath LoadoutScreenPath { get; set; }

		private LoadoutScreen _loadout = null!;

		public override void _Ready()
		{
			_loadout = GetNode<LoadoutScreen>(LoadoutScreenPath);

			// Load content directly from JSON (no ContentDatabase.* usage)
			var classes = LoadJsonList<ClassDef>("res://Content/classes.json");
			var spells  = LoadJsonList<SpellDef>("res://Content/spells.json");
			var spellsT1 = spells.Where(s => s.Tier == 1).ToList();
			var spellsT2 = spells.Where(s => s.Tier == 2).ToList();

			GD.Print($"[Game] JSON loaded | classes={classes.Count}, spells={spells.Count} (t1={spellsT1.Count}, t2={spellsT2.Count})");

			_loadout.InjectContent(classes, spellsT1, spellsT2);
			_loadout.Build();
		}

		// ---------------- utils ----------------

		private static List<T> LoadJsonList<T>(string resPath)
		{
			try
			{
				if (!FileAccess.FileExists(resPath))
				{
					GD.PushWarning($"[Game] Missing JSON at {resPath}");
					return new List<T>();
				}

				using var fa = FileAccess.Open(resPath, FileAccess.ModeFlags.Read);
				var json = fa.GetAsText();
				var data = System.Text.Json.JsonSerializer.Deserialize<List<T>>(json,
					new System.Text.Json.JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});
				return data ?? new List<T>();
			}
			catch (Exception ex)
			{
				GD.PushError($"[Game] LoadJsonList<{typeof(T).Name}> failed for {resPath}: {ex.Message}");
				return new List<T>();
			}
		}
	}
}
