// Scripts/Godot/BattleRoot.cs
// Builds the battle screen, reads loadout metadata safely, and never compares Variant to null.

using System;
using Godot;

namespace DiceArena.Godot
{
	public partial class BattleRoot : Control
	{
		// === Exported node paths (safe defaults) ===============================
		[Export] public NodePath FriendlyRootPath { get; set; } = new NodePath();
		[Export] public NodePath EnemyRootPath    { get; set; } = new NodePath();
		[Export] public NodePath LogRootPath      { get; set; } = new NodePath();

		// === Resolved nodes ====================================================
		private VBoxContainer? _friendlyRoot;
		private VBoxContainer? _enemyRoot;
		private RichTextLabel? _log;

		// === Random helper (Godot 4) ===========================================
		private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();

		public override void _Ready()
		{
			_rng.Randomize();

			_friendlyRoot = GetNodeOrNull<VBoxContainer>(SafePath(FriendlyRootPath));
			_enemyRoot    = GetNodeOrNull<VBoxContainer>(SafePath(EnemyRootPath));
			_log          = GetNodeOrNull<RichTextLabel>(SafePath(LogRootPath));

			// If any are missing, try to find by common names in the scene
			if (_friendlyRoot is null) _friendlyRoot = FindNodeByName<VBoxContainer>("FriendlyRoot", "Friendly", "Friends");
			if (_enemyRoot    is null) _enemyRoot    = FindNodeByName<VBoxContainer>("EnemyRoot", "Enemy", "Enemies");
			if (_log          is null) _log          = FindNodeByName<RichTextLabel>("LogRoot", "Log", "BattleLog");

			var missingFriendly = _friendlyRoot is null;
			var missingEnemy    = _enemyRoot    is null;
			var missingLog      = _log          is null;

			if (missingFriendly || missingEnemy || missingLog)
			{
				GD.PushError($"[Battle] Missing roots: friendly? {(!missingFriendly)}, enemy? {(!missingEnemy)}, log? {(!missingLog)}");
				BuildFallbackLayout();
			}

			// Try to read selection from metadata
			ApplyLoadoutFromMeta();

			// If nothing came from metadata, mock some content so layout shows
			if (_friendlyRoot?.GetChildCount() == 0)
			{
				AddHeroCard(_friendlyRoot!, "You", "guard", "basic_strike", "iron_skin");
			}
			if (_enemyRoot?.GetChildCount() == 0)
			{
				AddHeroCard(_enemyRoot!, "Slime", "slime", "slap", "goo_burst");
			}
		}

		// ----------------------- Layout helpers --------------------------------

		private static NodePath SafePath(NodePath p)
		{
			// NodePath has no .Empty in Godot 4; treat empty string as "no path".
			var s = p.ToString();
			return string.IsNullOrEmpty(s) ? new NodePath() : p;
		}

		private T? FindNodeByName<T>(params string[] names) where T : Node
		{
			foreach (var n in names)
			{
				var node = GetNodeOrNull<T>(n);
				if (node != null) return node;
			}
			return null;
		}

		private void BuildFallbackLayout()
		{
			// Clear existing
			foreach (var c in GetChildren())
			{
				if (c is Node n) n.QueueFree();
			}

			// Root HBox
			var row = new HBoxContainer
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical   = SizeFlags.ExpandFill
			};
			row.AddThemeConstantOverride("separation", 16);
			AddChild(row);

			// FRIENDLY panel
			var friendlyPanel = new PanelContainer
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical   = SizeFlags.ExpandFill
			};
			row.AddChild(friendlyPanel);

			var friendlyVBox = new VBoxContainer
			{
				Name = "FriendlyRoot",
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical   = SizeFlags.ExpandFill
			};
			friendlyVBox.AddThemeConstantOverride("separation", 8);
			friendlyPanel.AddChild(friendlyVBox);
			_friendlyRoot = friendlyVBox;

			// MIDDLE log panel
			var logPanel = new PanelContainer
			{
				CustomMinimumSize   = new Vector2(380, 0),
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical   = SizeFlags.ExpandFill
			};
			row.AddChild(logPanel);

			var logVBox = new VBoxContainer
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical   = SizeFlags.ExpandFill
			};
			logVBox.AddThemeConstantOverride("separation", 8);
			logPanel.AddChild(logVBox);

			var logLabel = new Label { Text = "Battle Log" };
			logVBox.AddChild(logLabel);

			var logText = new RichTextLabel
			{
				Name               = "BattleLog",
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical   = SizeFlags.ExpandFill,
				ScrollActive       = true,
				AutowrapMode       = TextServer.AutowrapMode.Word
			};
			logVBox.AddChild(logText);
			_log = logText;

			// ENEMY panel
			var enemyPanel = new PanelContainer
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical   = SizeFlags.ExpandFill
			};
			row.AddChild(enemyPanel);

			var enemyVBox = new VBoxContainer
			{
				Name = "EnemyRoot",
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical   = SizeFlags.ExpandFill
			};
			enemyVBox.AddThemeConstantOverride("separation", 8);
			enemyPanel.AddChild(enemyVBox);
			_enemyRoot = enemyVBox;
		}

		// ----------------------- Loadout from metadata --------------------------

		private void ApplyLoadoutFromMeta()
		{
			// Expect metas:
			//  SetMeta("partySize", int)
			//  SetMeta("players", Godot.Collections.Array of JSON strings)
			if (!HasMeta("players"))
			{
				Log("[i]No loadout metadata found; using mock heroes.[/i]");
				return;
			}

			var playersVar = GetMeta("players");
			if (playersVar.VariantType == Variant.Type.Nil)
			{
				Log("[i]Players meta is Nil; using mock heroes.[/i]");
				return;
			}
			if (playersVar.VariantType != Variant.Type.Array)
			{
				Log("[color=yellow][i]Players meta has unexpected type; using mock heroes.[/i][/color]");
				return;
			}

			var arr = (global::Godot.Collections.Array)playersVar;

			// Clear current roots
			if (_friendlyRoot != null) ClearChildren(_friendlyRoot);
			if (_enemyRoot != null) ClearChildren(_enemyRoot);

			// Build friendlies from metadata (first N entries)
			int idx = 0;
			foreach (Variant item in arr)
			{
				if (idx >= 4) break;

				if (item.VariantType == Variant.Type.String)
				{
					string json = item.AsString();
					TryAddHeroFromJson(_friendlyRoot!, json, $"Hero {idx + 1}");
				}
				idx++;
			}

			// Ensure at least one enemy so we see both sides
			AddHeroCard(_enemyRoot!, "Enemy", "enemy", "bite", "roar");
			Log($"[b]Loaded {idx} player(s) from metadata.[/b]");
		}

		private void TryAddHeroFromJson(VBoxContainer parent, string json, string fallbackName)
		{
			// Expecting keys: ClassId (string), Tier1 (string[]), Tier2 (string)
			try
			{
				var doc  = System.Text.Json.JsonDocument.Parse(json);
				var root = doc.RootElement;

				string heroName = fallbackName;
				if (root.TryGetProperty("Name", out var nameProp) && nameProp.ValueKind == System.Text.Json.JsonValueKind.String)
					heroName = nameProp.GetString() ?? fallbackName;

				string classId = "";
				if (root.TryGetProperty("ClassId", out var cid) && cid.ValueKind == System.Text.Json.JsonValueKind.String)
					classId = cid.GetString() ?? "";

				string? t1 = null;
				if (root.TryGetProperty("Tier1", out var t1Prop) && t1Prop.ValueKind == System.Text.Json.JsonValueKind.Array)
				{
					foreach (var e in t1Prop.EnumerateArray())
					{
						if (e.ValueKind == System.Text.Json.JsonValueKind.String)
						{
							t1 = e.GetString();
							break;
						}
					}
				}

				string? t2 = null;
				if (root.TryGetProperty("Tier2", out var t2Prop) && t2Prop.ValueKind == System.Text.Json.JsonValueKind.String)
					t2 = t2Prop.GetString();

				AddHeroCard(parent, heroName, classId, t1, t2);
			}
			catch (Exception ex)
			{
				Log($"[color=red]Failed to parse player JSON:[/color] {ex.Message}");
				AddHeroCard(parent, fallbackName, "unknown", null, null);
			}
		}

		// -------------------------- UI utilities --------------------------------

		private static void ClearChildren(Node parent)
		{
			foreach (var c in parent.GetChildren())
			{
				if (c is Node n) n.QueueFree();
			}
		}

		private void Log(string text)
		{
			if (_log == null) return;
			_log.AppendText(text + "\n");
			_log.ScrollToLine(_log.GetLineCount() - 1);
		}

		// Creates a simple “card” for a unit on the given side using HeroCard if present
		private void AddHeroCard(VBoxContainer parent, string name, string classId, string? tier1, string? tier2)
		{
			// Try to instantiate a HeroCard scene/node if available, else fall back to plain HBox
			HeroCard? cardNode = null;

			// Look for an existing packed scene named "HeroCard.tscn"
			var heroCardScene = GD.Load<PackedScene>("res://Scripts/Godot/HeroCard.tscn");
			if (heroCardScene != null)
			{
				var inst = heroCardScene.Instantiate();
				cardNode = inst as HeroCard;
			}

			if (cardNode == null)
			{
				// Fallback: minimal card
				var panel = new PanelContainer
				{
					SizeFlagsHorizontal = SizeFlags.ExpandFill
				};
				var row = new HBoxContainer
				{
					SizeFlagsHorizontal = SizeFlags.ExpandFill
				};
				row.AddThemeConstantOverride("separation", 6);
				panel.AddChild(row);

				var nameLabel = new Label { Text = name };
				row.AddChild(nameLabel);

				parent.AddChild(panel);
				return;
			}

			cardNode.SetHero(name, classId, tier1, tier2);
			parent.AddChild(cardNode);
		}
	}
}
