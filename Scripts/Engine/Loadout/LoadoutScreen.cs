// res://Scripts/Engine/Loadout/LoadoutScreen.cs
#nullable enable
using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

namespace DiceArena.Godot
{
	public partial class LoadoutScreen : Control
	{
		// ---- Inspector paths ----
		[Export] public NodePath CardsRootPath { get; set; } = "LayoutRoot/MainRow/RightPanel";
		[Export] public NodePath PartyButtonsRootPath { get; set; } = "LayoutRoot/PartySizeButtonContainer";
		[Export] public NodePath FinalizeButtonPath { get; set; } = "LayoutRoot/HeaderRow/FinalizeBtn";
		[Export(PropertyHint.Range, "1,4,1")] public int DefaultPartySize { get; set; } = 1;

		// Bridge (assign to your LoadoutToBattleBridge node)
		[Export] public NodePath BridgePath { get; set; } = default!;

		// ---- Runtime refs ----
		private Control _cardsRoot = null!;
		private Control _buttonsRoot = null!;
		private Button? _finalizeBtn;

		private readonly List<PlayerLoadoutPanel> _panels = new();
		private readonly List<Button> _partyButtons = new();

		public override void _Ready()
		{
			// Resolve nodes
			_cardsRoot = GetNode<Control>(CardsRootPath);
			_buttonsRoot = GetNode<Control>(PartyButtonsRootPath);

			// Collect player panels (children of CardsRoot)
			_panels.Clear();
			_panels.AddRange(_cardsRoot.GetChildren().OfType<PlayerLoadoutPanel>());

			// Collect party size buttons
			_partyButtons.Clear();
			_partyButtons.AddRange(_buttonsRoot.GetChildren().OfType<Button>());

			// Wire party size buttons (1..N)
			for (int i = 0; i < _partyButtons.Count; i++)
			{
				int target = i + 1;
				var b = _partyButtons[i];
				b.Text = target.ToString();
				b.Pressed += () => SetPartySize(target);
			}

			// Resolve Finalize button by exported path; if missing, try to find by name
			_finalizeBtn = GetNodeOrNull<Button>(FinalizeButtonPath);
			if (_finalizeBtn == null)
			{
				// Fallback: search for a Button named "FinalizeBtn" under us
				_finalizeBtn = FindChild("FinalizeBtn", true, false) as Button;
			}

			if (_finalizeBtn != null)
			{
				// Ensure we don't double-wire if scene reloads
				_finalizeBtn.Pressed -= OnFinalizePressed;
				_finalizeBtn.Pressed += OnFinalizePressed;
				GD.Print("[LoadoutScreen] Finalize button wired in code.");
			}
			else
			{
				GD.PushError("[LoadoutScreen] Finalize button NOT found. Check FinalizeButtonPath or node name.");
			}

			// Default party size
			int clamped = Math.Clamp(DefaultPartySize, 1, Math.Max(1, _panels.Count));
			SetPartySize(clamped);

			GD.Print($"[LoadoutScreen] Ready. Panels={_panels.Count}, PartyButtons={_partyButtons.Count}");
		}

		private void SetPartySize(int size)
		{
			if (_panels.Count == 0)
			{
				GD.PushWarning("[LoadoutScreen] No PlayerLoadoutPanel children found under CardsRoot.");
				return;
			}

			size = Math.Clamp(size, 1, _panels.Count);
			for (int i = 0; i < _panels.Count; i++)
				_panels[i].Visible = i < size;

			GD.Print($"[LoadoutScreen] SetPartySize => {size} (of {_panels.Count})");
		}

		// Hooked in code above; you do NOT need to connect in the editor,
		// but it will also work if you already connected the signal.
		public void OnFinalizePressed()
		{
			GD.Print("[LoadoutScreen] Finalize pressed → delegating to Bridge.");

			if (_panels.Count == 0)
			{
				GD.PushWarning("[LoadoutScreen] Finalize ignored: no panels present.");
				return;
			}

			var bridge = GetNodeOrNull<LoadoutToBattleBridge>(BridgePath);
			if (bridge == null)
			{
				GD.PushError("[LoadoutScreen] BridgePath not set or LoadoutToBattleBridge not found.");
				return;
			}

			// Send the first visible panel (Hero 1) to the bridge.
			var panel = _panels.FirstOrDefault(p => p.Visible);
			if (panel == null)
			{
				GD.PushWarning("[LoadoutScreen] No visible PlayerLoadoutPanel to finalize.");
				return;
			}

			bridge.FinalizeToBattle(panel);
		}
	}
}
