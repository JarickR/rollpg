// res://Scripts/Godot/LoadoutToBattleBridge.cs
#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiceArena.Godot
{
	public partial class LoadoutToBattleBridge : Node
	{
		[Export] public NodePath LoadoutScreenPath { get; set; } = "LoadoutScreen";
		[Export] public NodePath BattleRootPath    { get; set; } = "BattleRoot";

		[Export] public NodePath HUDSlot0Path { get; set; } = "BattleRoot/HUDLayer/HUDRow/Slot1";
		[Export] public NodePath HUDSlot1Path { get; set; } = "BattleRoot/HUDLayer/HUDRow/Slot2";
		[Export] public NodePath HUDSlot2Path { get; set; } = "BattleRoot/HUDLayer/HUDRow/Slot3";
		[Export] public NodePath HUDSlot3Path { get; set; } = "BattleRoot/HUDLayer/HUDRow/Slot4";
		[Export] public NodePath HUDSlot4Path { get; set; } = "BattleRoot/HUDLayer/HUDRow/Slot5";
		[Export] public NodePath HUDSlot5Path { get; set; } = "BattleRoot/HUDLayer/HUDRow/Slot6";

		// Dice / overlay references
		[Export] public NodePath? DiceOverlayPath    { get; set; } = "DiceOverlay"; // direct overlay control
		[Export] public NodePath? Dice3DPath         { get; set; } = "DiceOverlay/DiceViewport/DiceWorld/Dice3d";
		[Export] public NodePath? DiceInteractorPath { get; set; } = "DiceOverlay"; // node with DiceInteractor
		[Export] public NodePath? DiceDockCardP1Path { get; set; } = null;

		[Export] public Texture2D? UpgradeIcon  { get; set; }
		[Export] public Texture2D? FallbackIcon { get; set; }

		private Control    _loadout   = default!;
		private CanvasItem _battleRoot= default!;
		private readonly List<Control> _hudSlots = new();

		private Control?        _diceOverlay;
		private Dice3D?         _dice;
		private DiceInteractor? _diceInteractor;

		public override void _Ready()
		{
			_loadout     = GetNode<Control>(LoadoutScreenPath);
			_battleRoot  = GetNode<CanvasItem>(BattleRootPath);

			_hudSlots.Clear();
			_hudSlots.Add(Require<Control>(HUDSlot0Path));
			_hudSlots.Add(Require<Control>(HUDSlot1Path));
			_hudSlots.Add(Require<Control>(HUDSlot2Path));
			_hudSlots.Add(Require<Control>(HUDSlot3Path));
			_hudSlots.Add(Require<Control>(HUDSlot4Path));
			_hudSlots.Add(Require<Control>(HUDSlot5Path));

			_diceOverlay    = GetNodeOrNull<Control>(DiceOverlayPath ?? "");
			_dice           = GetNodeOrNull<Dice3D>(Dice3DPath ?? "");
			_diceInteractor = GetNodeOrNull<DiceInteractor>(DiceInteractorPath ?? "");

			// Force overlay hidden on Loadout (and non-blocking), regardless of interactor init order.
			_diceInteractor?.EnableDice(false);
			SetDiceOverlayVisible(false);
			CallDeferred(nameof(HideDiceOverlayDeferred));

			ShowLoadout();
		}

		private void HideDiceOverlayDeferred()
		{
			_diceInteractor?.EnableDice(false);
			SetDiceOverlayVisible(false);
		}

		// Hard toggle overlay visibility + input blocking in one place
		private void SetDiceOverlayVisible(bool on)
		{
			if (_diceOverlay == null) return;
			_diceOverlay.Visible      = on;
			_diceOverlay.MouseFilter  = on ? Control.MouseFilterEnum.Stop : Control.MouseFilterEnum.Ignore;
			_diceOverlay.ProcessMode  = on ? ProcessModeEnum.Inherit     : ProcessModeEnum.Disabled;
		}

		// ---------- Compatibility overloads ----------
		public void FinalizeToBattle(PlayerLoadoutPanel panel)
		{
			panel.GetChosenTextures(out var cls, out var t1, out var t2);
			FinalizeToBattle(cls, t1, t2);
		}
		public void FinalizeToBattle(Texture2D? classIcon)
			=> FinalizeToBattle(classIcon, new List<Texture2D>(), new List<Texture2D>());
		public void FinalizeToBattle(Texture2D? classIcon, List<Texture2D> tier1)
			=> FinalizeToBattle(classIcon, tier1, new List<Texture2D>());

		// -------------- Primary method --------------
		public void FinalizeToBattle(Texture2D? classIcon, List<Texture2D>? tier1, List<Texture2D>? tier2)
		{
			tier1 ??= new List<Texture2D>();
			tier2 ??= new List<Texture2D>();

			GD.Print($"[Bridge] Finalize: class={(classIcon as Resource)?.ResourcePath}, t1={tier1.Count}, t2={tier2.Count}");

			var mid = new List<Texture2D>(4);
			void TryAdd(Texture2D? tx) { if (tx != null && mid.Count < 4) mid.Add(tx); }
			foreach (var t in tier1) TryAdd(t);
			foreach (var t in tier2) TryAdd(t);
			while (mid.Count < 4) TryAdd(FallbackIcon ?? UpgradeIcon ?? classIcon ?? tier1.FirstOrDefault() ?? tier2.FirstOrDefault());

			SetControlTexture(_hudSlots[0], classIcon ?? FallbackIcon);
			for (int i = 0; i < 4; i++) SetControlTexture(_hudSlots[1 + i], mid[i]);
			SetControlTexture(_hudSlots[5], UpgradeIcon ?? FallbackIcon);

			if (_dice != null)
			{
				try
				{
					_dice.ApplyLoadoutFaces(
						classIcon:      classIcon ?? FallbackIcon ?? mid[0],
						slotIcons1to4:  mid,
						upgradeIcon:    UpgradeIcon ?? FallbackIcon ?? mid[0],
						fallback:       FallbackIcon
					);
				}
				catch (Exception ex) { GD.PushWarning($"[Bridge] Dice paint failed: {ex.Message}"); }
			}

			ShowBattle();

			if (_diceInteractor != null && DiceDockCardP1Path is { } np)
			{
				var card = GetNodeOrNull<Control>(np);
				if (card != null)
					_diceInteractor.AppearUnderCard(card, 8);
			}
		}

		private void ShowLoadout()
		{
			_loadout.Visible = true;
			_loadout.ProcessMode = ProcessModeEnum.Inherit;

			_battleRoot.Visible = false;
			_battleRoot.ProcessMode = ProcessModeEnum.Disabled;

			_diceInteractor?.EnableDice(false);
			SetDiceOverlayVisible(false);

			GD.Print("[Bridge] State=LOADOUT (battle hidden).");
		}

		private void ShowBattle()
		{
			_loadout.Visible = false;
			_loadout.ProcessMode = ProcessModeEnum.Disabled;

			_battleRoot.Visible = true;
			_battleRoot.ProcessMode = ProcessModeEnum.Inherit;

			_diceInteractor?.EnableDice(true);

			// NEW: force the overlay to be visible & centered so we can verify it renders
			_diceInteractor?.ForceShowCentered(360);

			GD.Print("[Bridge] State=BATTLE (battle visible).");
		}

		private T Require<T>(NodePath path) where T : class
		{
			var n = GetNodeOrNull<T>(path);
			if (n == null) throw new Exception($"Node not found: '{path}'");
			return n;
		}

		private static void SetControlTexture(Control c, Texture2D? tex)
		{
			if (c == null || tex == null) return;
			switch (c)
			{
				case TextureRect tr:   tr.Texture = tex; break;
				case TextureButton tb: tb.TextureNormal = tex; break;
				case Button b:         b.Icon = tex; break;
				default:
					var tr2 = c.GetNodeOrNull<TextureRect>("TextureRect");
					if (tr2 != null) tr2.Texture = tex;
					break;
			}
		}
	}
}
