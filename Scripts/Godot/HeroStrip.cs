using Godot;
using System.Collections.Generic;

namespace DiceArena.GodotApp
{
	public partial class HeroStrip : PanelContainer
	{
		[Signal] public delegate void RollPressedEventHandler();
		[Signal] public delegate void UpgradePressedEventHandler();
		[Signal] public delegate void FaceClickedEventHandler(int index);

		private Label _classLabel = null!;
		private Button _upgradeBtn = null!;
		private Button _d6Btn = null!;
		private HBoxContainer _facesRow = null!;
		private readonly List<Button> _faceBtns = new();

		public override void _Ready()
		{
			AddThemeConstantOverride("margin_left", 8);
			AddThemeConstantOverride("margin_right", 8);
			AddThemeConstantOverride("margin_top", 8);
			AddThemeConstantOverride("margin_bottom", 8);

			var root = new VBoxContainer
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical = SizeFlags.ExpandFill
			};
			AddChild(root);

			var header = new HBoxContainer();
			header.AddChild(new Label { Text = "Hero", SizeFlagsHorizontal = SizeFlags.ExpandFill });
			root.AddChild(header);

			var controls = new HBoxContainer();
			_classLabel = new Label { Text = "Class", CustomMinimumSize = new Vector2(120, 0) };
			_upgradeBtn = new Button { Text = "Upgrade", CustomMinimumSize = new Vector2(120, 0) };
			_d6Btn = new Button { Text = "d6", CustomMinimumSize = new Vector2(120, 0) };
			_upgradeBtn.Pressed += () => EmitSignal(SignalName.UpgradePressed);
			_d6Btn.Pressed += () => EmitSignal(SignalName.RollPressed);

			controls.AddChild(_classLabel);
			controls.AddChild(_upgradeBtn);
			controls.AddChild(_d6Btn);
			root.AddChild(controls);

			_facesRow = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
			for (int i = 0; i < 4; i++)
			{
				var b = new Button
				{
					Text = $"Spell {i + 1}",
					CustomMinimumSize = new Vector2(120, 64),
					SizeFlagsHorizontal = SizeFlags.ExpandFill
				};
				int captured = i;
				b.Pressed += () => EmitSignal(SignalName.FaceClicked, captured);
				_facesRow.AddChild(b);
				_faceBtns.Add(b);
			}
			root.AddChild(_facesRow);

			root.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });
		}

		public void SetClassName(string className) => _classLabel.Text = className;

		public void SetFaces(IReadOnlyList<string> faces)
		{
			for (int i = 0; i < _faceBtns.Count; i++)
			{
				_faceBtns[i].Text = (faces != null && i < faces.Count) ? faces[i] : $"Spell {i + 1}";
			}
		}
	}
}
