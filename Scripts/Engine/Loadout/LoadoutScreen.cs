// res://Scripts/Engine/Loadout/LoadoutScreen.cs
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace DiceArena.Godot
{
	public partial class LoadoutScreen : Control
	{
		// ------- Inspector paths -------
		[Export] public NodePath CardsRootPath { get; set; } = "LayoutRoot/MainRow/RightPanel";
		[Export] public NodePath PartyButtonsRootPath { get; set; } = "LayoutRoot/PartySizeButtonContainer";
		[Export] public NodePath FinalizeBtnPath { get; set; } = "LayoutRoot/HeaderRow/FinalizeBtn";

		// Default party size (1..4)
		[Export(PropertyHint.Range, "1,4,1")] public int DefaultPartySize { get; set; } = 1;

		// ------- Runtime refs -------
		private Control _cardsRoot = null!;
		private Control _buttonsRoot = null!;
		private Button  _finalizeBtn = null!;
		private LoadoutToBattleBridge? _bridge;

		// Panels & party size state
		private readonly List<PlayerLoadoutPanel> _panels = new();
		private readonly List<Button> _buttons = new();
		private int _partySize = 1;

		public override void _Ready()
		{
			// Resolve siblings/children
			_bridge = GetNodeOrNull<LoadoutToBattleBridge>("../LoadoutToBattleBridge");

			_cardsRoot   = GetNode<Control>(CardsRootPath);
			_buttonsRoot = GetNode<Control>(PartyButtonsRootPath);
			_finalizeBtn = GetNode<Button>(FinalizeBtnPath);

			_panels.Clear();
			_panels.AddRange(_cardsRoot.GetChildren().OfType<PlayerLoadoutPanel>());

			_buttons.Clear();
			_buttons.AddRange(_buttonsRoot.GetChildren().OfType<Button>());

			// Wire party-size buttons 1..N
			for (int i = 0; i < _buttons.Count; i++)
			{
				int target = i + 1;
				var b = _buttons[i];
				b.Text = target.ToString();
				b.Pressed += () => SetPartySize(target);
			}

			// Wire finalize
			_finalizeBtn.Pressed += OnFinalizePressed;

			// Apply initial party size
			SetPartySize(Math.Clamp(DefaultPartySize, 1, _panels.Count));
			GD.Print($"[LoadoutScreen] SetPartySize => {_partySize} (of {_panels.Count})");
		}

		private void SetPartySize(int size)
		{
			_partySize = Math.Clamp(size, 1, _panels.Count);
			for (int i = 0; i < _panels.Count; i++)
				_panels[i].Visible = i < _partySize;
		}

		private void OnFinalizePressed()
		{
			GD.Print("[LoadoutScreen] Finalize pressed → hiding self then delegating to Bridge.");
			Visible = false;
			ProcessMode = ProcessModeEnum.Disabled;

			// hand the active panels & party size to the Bridge
			_bridge?.FinalizeToBattle(_panels, _partySize);
		}
	}
}
