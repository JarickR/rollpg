using System.Collections.Generic;
using System.Linq;
using Godot;
using DiceArena.GodotUI;

namespace DiceArena.Godot
{
	public partial class LoadoutScreen : Control
	{
		[Export] public NodePath CardsRootPath { get; set; } = "LayoutRoot/MainRow/RightPanel";
		[Export] public NodePath PartyButtonsRootPath { get; set; } = "LayoutRoot/PartySizeButtonContainer";
		[Export(PropertyHint.Range, "1,4,1")] public int DefaultPartySize { get; set; } = 1;

		private Control _cardsRoot = null!;
		private Control _buttonsRoot = null!;
		private List<PlayerLoadoutPanel> _panels = new();
		private List<Button> _buttons = new();

		public override void _Ready()
		{
			_cardsRoot = GetNode<Control>(CardsRootPath);
			_buttonsRoot = GetNode<Control>(PartyButtonsRootPath);

			_panels = _cardsRoot.GetChildren().OfType<PlayerLoadoutPanel>().ToList();
			_buttons = _buttonsRoot.GetChildren().OfType<Button>().ToList();

			for (int i = 0; i < _buttons.Count; i++)
			{
				int target = i + 1;
				var b = _buttons[i];
				b.Text = target.ToString();
				b.Pressed += () => SetPartySize(target);
			}

			SetPartySize(Math.Clamp(DefaultPartySize, 1, _panels.Count));
		}

		private void SetPartySize(int size)
		{
			size = Math.Clamp(size, 1, _panels.Count);
			for (int i = 0; i < _panels.Count; i++)
				_panels[i].Visible = i < size;

			GD.Print($"[LoadoutScreen] SetPartySize => {size} (of {_panels.Count})");
		}
	}
}
