// res://Scripts/Godot/RollPopup.cs
using Godot;

namespace DiceArena.GodotUI
{
	public partial class RollPopup : Window
	{
		private Label _label;

		public override void _Ready()
		{
			Title = "Roll";
			InitialPosition = WindowInitialPosition.CenterPrimaryScreen;
			Size = new Vector2I(420, 180);
			Borderless = false;

			var vb = new VBoxContainer
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical   = Control.SizeFlags.ExpandFill
			};
			vb.AddThemeConstantOverride("separation", 8);
			AddChild(vb);

			_label = UiUtils.MakeLabel("Rolling…", 18, bold: true);
			vb.AddChild(_label);
		}

		public void ShowText(string msg)
		{
			_label.Text = msg;
			PopupCentered();
		}
	}
}
