// res://Scripts/Godot/LoadoutToBattleBridge.cs
#nullable enable
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace DiceArena.Godot
{
	/// <summary>
	/// Bridges Loadout -> Battle:
	/// • Starts in Loadout (Battle hidden + ignoring input).
	/// • On Finalize, paints 6 HUD slots and shows Battle.
	/// Diagnostics prints help you see what's wired and what's painted.
	/// </summary>
	public partial class LoadoutToBattleBridge : Node
	{
		// ---------- Wire these in the Inspector ----------
		[ExportGroup("Screens")]
		[Export] public Control LoadoutScreen { get; set; } = default!;
		[Export] public Control BattleRoot    { get; set; } = default!;

		[ExportGroup("HUD (choose ONE approach)")]
		// EITHER: set these six directly...
		[Export] public TextureRect? Slot1 { get; set; }    // Class
		[Export] public TextureRect? Slot2 { get; set; }    // Spells/Def
		[Export] public TextureRect? Slot3 { get; set; }
		[Export] public TextureRect? Slot4 { get; set; }
		[Export] public TextureRect? Slot5 { get; set; }
		[Export] public TextureRect? Slot6 { get; set; }    // Upgrade
		// OR: set HUDRow and ensure it has children named Slot1..Slot6
		[Export] public HBoxContainer? HUDRow { get; set; }

		[ExportGroup("Icons")]
		[Export] public Texture2D DefensiveIcon { get; set; } = default!;
		[Export] public Texture2D UpgradeIcon   { get; set; } = default!;

		// Cached resolved slots
		private TextureRect?[] _slots = new TextureRect?[6];

		public override void _Ready()
		{
			if (LoadoutScreen == null || BattleRoot == null)
			{
				GD.PushError("[Bridge] LoadoutScreen / BattleRoot are not assigned.");
				return;
			}

			ResolveSlots();
			ShowLoadout(); // battle hidden at start
		}

		// Called by LoadoutScreen with currently visible panels & party size
		public void FinalizeToBattle(List<PlayerLoadoutPanel> panels, int partySize)
		{
			if (panels == null || panels.Count == 0)
			{
				GD.PushWarning("[Bridge] FinalizeToBattle: no panels passed.");
				return;
			}

			// Take P1’s picks for now
			var p1 = panels[0];
			p1.GetChosenTextures(out var classIcon, out var t1, out var t2);

			// Compose spells list (T1 then T2; add T3 here in future)
			var spells = new List<Texture2D>(t1);
			spells.AddRange(t2);

			GD.Print($"[Bridge] Finalize: class={(classIcon!=null ? classIcon.ResourcePath : "NULL")}, " +
					 $"t1={t1.Count}, t2={t2.Count}");

			PaintSixSlots(classIcon, spells);
			ShowBattle();
		}

		// ---------------- Internal helpers ----------------
		private void ShowLoadout()
		{
			// Loadout visible / interactive
			LoadoutScreen.Visible = true;
			LoadoutScreen.ProcessMode = ProcessModeEnum.Inherit;
			SetMouseFilterRecursive(LoadoutScreen, Control.MouseFilterEnum.Stop);

			// Battle hidden / ignores input
			BattleRoot.Visible = false;
			BattleRoot.ProcessMode = ProcessModeEnum.Disabled;
			SetMouseFilterRecursive(BattleRoot, Control.MouseFilterEnum.Ignore);

			GD.Print("[Bridge] State=LOADOUT (battle hidden & ignoring input).");
		}

		private void ShowBattle()
		{
			LoadoutScreen.Visible = false;
			LoadoutScreen.ProcessMode = ProcessModeEnum.Disabled;

			BattleRoot.Visible = true;
			BattleRoot.ProcessMode = ProcessModeEnum.Inherit;
			// Keep HUD non-interactive; change to Pass/Stop if you add battle buttons later.
			SetMouseFilterRecursive(BattleRoot, Control.MouseFilterEnum.Ignore);

			GD.Print("[Bridge] State=BATTLE (battle visible).");
		}

		private static void SetMouseFilterRecursive(Node node, Control.MouseFilterEnum filter)
		{
			if (node is Control c) c.MouseFilter = filter;
			foreach (var child in node.GetChildren())
				SetMouseFilterRecursive(child, filter);
		}

		private void ResolveSlots()
		{
			// Preferred: use the six direct exports if all are assigned
			var direct = new[] { Slot1, Slot2, Slot3, Slot4, Slot5, Slot6 };
			bool allDirect = direct.All(s => s != null);

			if (allDirect)
			{
				_slots = direct!;
				GD.Print("[Bridge] HUD slots resolved via direct exports (Slot1..Slot6).");
				LogSlotNames();
				return;
			}

			// Fallback: resolve from HUDRow by child name "Slot#"
			if (HUDRow == null)
			{
				GD.PushWarning("[Bridge] Neither Slot1..Slot6 nor HUDRow assigned. Painting will be blank.");
				_slots = new TextureRect?[6];
				return;
			}

			for (int i = 0; i < 6; i++)
			{
				var path = $"Slot{i + 1}";
				_slots[i] = HUDRow.GetNodeOrNull<TextureRect>(path);
				if (_slots[i] == null)
					GD.PushWarning($"[Bridge] Missing child TextureRect: {HUDRow.Name}/{path}");
			}

			GD.Print("[Bridge] HUD slots resolved via HUDRow children.");
			LogSlotNames();
		}

		private void LogSlotNames()
		{
			for (int i = 0; i < 6; i++)
			{
				var name = _slots[i]?.GetPath().ToString() ?? "NULL";
				GD.Print($"[Bridge] Slot{i + 1} -> {name}");
			}
		}

		private void PaintSixSlots(Texture2D? classIcon, List<Texture2D> spells)
		{
			// Defensive baseline (so you see something even if picks are empty)
			for (int i = 0; i < 5; i++)
				SetSlot(i, DefensiveIcon);
			SetSlot(5, UpgradeIcon);

			// Slot 1 = class
			SetSlot(0, classIcon);

			// Fill slots 2..5 with spells left→right
			int cursor = 1; // indexes 1..4
			for (int i = 0; i < spells.Count && cursor <= 4; i++)
				SetSlot(cursor++, spells[i]);

			// Diagnostics
			GD.Print($"[Bridge] Painted: S1={TexName(_slots[0])}, " +
					 $"S2={TexName(_slots[1])}, S3={TexName(_slots[2])}, " +
					 $"S4={TexName(_slots[3])}, S5={TexName(_slots[4])}, S6={TexName(_slots[5])}");
		}

		private void SetSlot(int index, Texture2D? tex)
		{
			if (index < 0 || index >= _slots.Length) return;
			var slot = _slots[index];
			if (slot == null)
			{
				GD.PushWarning($"[Bridge] Slot{index + 1} is NULL; cannot set texture.");
				return;
			}
			slot.Texture = tex;
		}

		private static string TexName(TextureRect? tr)
			=> tr?.Texture is Texture2D t ? t.ResourcePath : "NULL";
	}
}
