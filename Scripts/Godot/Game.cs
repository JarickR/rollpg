// res://Scripts/Godot/Game.cs
using Godot;
using System;

public partial class Game : Control
{
	private VBoxContainer _root;
	private TextEdit _logBox;
	private Button _rollButton;

	public override void _Ready()
	{
		// ===== Root layout =====
		_root = new VBoxContainer
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical   = Control.SizeFlags.ExpandFill
		};
		// Spacing between children
		_root.AddThemeConstantOverride("separation", 12);
		// Soft margins (theme-based so it works on containers)
		_root.AddThemeConstantOverride("margin_left",   20);
		_root.AddThemeConstantOverride("margin_top",    20);
		_root.AddThemeConstantOverride("margin_right",  20);
		_root.AddThemeConstantOverride("margin_bottom", 20);
		AddChild(_root);

		// ===== Log (TextEdit) =====
		_logBox = new TextEdit
		{
			Editable            = false, // read-only
			WrapMode            = TextEdit.LineWrappingMode.Word,
			CustomMinimumSize   = new Vector2(480, 220),
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical   = Control.SizeFlags.ExpandFill
		};
		_root.AddChild(_logBox);

		// ===== Roll button =====
		_rollButton = new Button
		{
			Text                = "Roll D20",
			CustomMinimumSize   = new Vector2(140, 44),
			SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
			SizeFlagsVertical   = Control.SizeFlags.ShrinkCenter
		};
		// Slight inner padding via theme constants
		_rollButton.AddThemeConstantOverride("hseparation", 10);
		_rollButton.AddThemeConstantOverride("vseparation", 6);
		_rollButton.Pressed += OnRollPressed;
		_root.AddChild(_rollButton);

		Log("Game initialized. Click “Roll D20”.");
	}

	private void OnRollPressed()
	{
		// Simple demo roll
		var rng = new Random();
		int result = rng.Next(1, 21); // [1..20]
		Log($"You rolled: {result}");
	}

	private void Log(string message)
	{
		if (string.IsNullOrEmpty(message)) return;
		_logBox.Text += message + "\n";
		// Godot 4: scroll to the newest line using method, not a bool/int property
		int lastLine = Math.Max(0, _logBox.GetLineCount() - 1);
		_logBox.ScrollToLine(lastLine);
	}
}
