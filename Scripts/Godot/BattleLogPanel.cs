// res://Scripts/Godot/BattleLogPanel.cs
#nullable enable
using Godot;
using DiceArena.GodotUI; // for SafeRichTextLabelExtensions

namespace DiceArena.Godot
{
	/// <summary>
	/// Simple log wrapper around a RichTextLabel. No caret, so no TextEdit -1 errors.
	/// </summary>
	public partial class BattleLogPanel : Control
	{
		[Export] public NodePath LogLabelPath { get; set; } = default!;

		private RichTextLabel _log = null!;

		public override void _Ready()
		{
			_log = GetNode<RichTextLabel>(LogLabelPath);

			// Sensible defaults for a log
			_log.ScrollActive = true;        // enable scroll bar
			_log.BbcodeEnabled = false;      // set true if you want bbcode formatting
			_log.SelectionEnabled = false;   // disable text selection (optional)

			// If you want wrapping, set in Inspector: Autowrap Mode = Word
			_log.SafeClear();
		}

		// Public API to use from elsewhere
		public void WriteLine(string line) => _log.SafeAppendLine(line);
		public void Clear() => _log.SafeClear();
		public void SetAll(string text) => _log.SafeSetText(text);
	}
}
