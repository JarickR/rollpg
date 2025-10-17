// res://Scripts/Godot/LoadoutToBattleBridge.cs
#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using DiceArena.Godot; // for PlayerLoadoutPanel

namespace RollPG.GodotUI
{
	public partial class LoadoutToBattleBridge : Node
	{
		[ExportGroup("Scene Roots")]
		[Export] public NodePath LoadoutScreenPath { get; set; } = "../LoadoutScreen";
		[Export] public NodePath BattleRootPath    { get; set; } = "../BattleRoot";

		[ExportGroup("Hero 1 HUD Slots (small)")]
		[Export] public TextureRect? Slot1 { get; set; }
		[Export] public TextureRect? Slot2 { get; set; }
		[Export] public TextureRect? Slot3 { get; set; }
		[Export] public TextureRect? Slot4 { get; set; }
		[Export] public TextureRect? Slot5 { get; set; }
		[Export] public TextureRect? Slot6 { get; set; }

		[ExportGroup("Hero 1 Large Portrait")]
		[Export] public TextureRect? Hero1ClassPortrait { get; set; }

		[ExportGroup("Fallback Icons (optional)")]
		// If you leave these null, the bridge will keep whatever textures are already on Slot5/Slot6.
		[Export] public Texture2D? DefensiveIcon { get; set; }
		[Export] public Texture2D? UpgradeIcon   { get; set; }

		// ---- runtime ----
		private Control _loadout = null!;
		private Control _battle  = null!;

		public override void _Ready()
		{
			_loadout = GetNode<Control>(LoadoutScreenPath);
			_battle  = GetNode<Control>(BattleRootPath);

			// Start in LOADOUT (battle hidden)
			SetBattleVisible(false);
			SetLoadoutVisible(true);

			GD.Print("[Bridge] State=LOADOUT (battle hidden).");
		}

		// Called by LoadoutScreen when Finalize is pressed
		public void FinalizeToBattle(List<PlayerLoadoutPanel> panels, int activeCount)
		{
			if (panels == null || panels.Count == 0)
			{
				GD.PushWarning("[Bridge] Finalize: no panels provided.");
				return;
			}

			// For now we only paint Hero 1 from panel 0 (you can extend to others later).
			var p1 = panels[0];

			p1.GetChosenTextures(out var classIcon, out var tier1, out var tier2);

			// Build the six small HUD slot textures (class, four action slots, upgrade).
			var slotTextures = BuildSixTextures(classIcon, tier1, tier2);

			// Paint small HUD
			var targets = new[] { Slot1, Slot2, Slot3, Slot4, Slot5, Slot6 };
			for (int i = 0; i < targets.Length; i++)
			{
				if (targets[i] is TextureRect tr && slotTextures[i] != null)
					tr.Texture = slotTextures[i];
			}

			// Paint the large portrait in Hero 1 header
			if (Hero1ClassPortrait != null)
				Hero1ClassPortrait.Texture = classIcon;

			GD.Print($"[Bridge] Finalize: class={classIcon?.ResourcePath ?? "null"}, " +
				$"t1={tier1.Count}, t2={tier2.Count}");

			// Switch visibility
			SetLoadoutVisible(false);
			SetBattleVisible(true);
			GD.Print("[Bridge] State=BATTLE (battle visible).");
		}

		// ================= helpers =================

		private Texture2D?[] BuildSixTextures(Texture2D? classIcon, List<Texture2D> t1, List<Texture2D> t2)
		{
			// Slot 1: class
			// Slots 2..5: spells and/or defensive icon filler
			// Slot 6: upgrade
			var result = new Texture2D?[6];

			result[0] = classIcon;

			// Gather up to four actions from tier1 first, then tier2.
			var actions = new List<Texture2D>(4);
			foreach (var t in t1)
			{
				if (actions.Count >= 4) break;
				actions.Add(t);
			}
			foreach (var t in t2)
			{
				if (actions.Count >= 4) break;
				actions.Add(t);
			}

			// Fill remaining action slots with defensive icon fallback (if provided),
			// otherwise keep whatever is already on the TextureRect.
			while (actions.Count < 4 && DefensiveIcon != null)
				actions.Add(DefensiveIcon);

			// Assign actions into slots 2..5
			for (int i = 0; i < 4; i++)
				result[1 + i] = i < actions.Count ? actions[i] : GetExistingTextureForSlot(1 + i);

			// Slot 6: upgrade (fallback to existing texture if not provided)
			result[5] = UpgradeIcon ?? GetExistingTextureForSlot(5);

			return result;
		}

		private Texture2D? GetExistingTextureForSlot(int index)
		{
			switch (index)
			{
				case 0: return Slot1?.Texture;
				case 1: return Slot2?.Texture;
				case 2: return Slot3?.Texture;
				case 3: return Slot4?.Texture;
				case 4: return Slot5?.Texture;
				case 5: return Slot6?.Texture;
			}
			return null;
		}

		private void SetLoadoutVisible(bool v)
		{
			_loadout.Visible = v;
			_loadout.ProcessMode = v ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
		}

		private void SetBattleVisible(bool v)
		{
			_battle.Visible = v;
			_battle.ProcessMode = v ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
		}
	}
}
