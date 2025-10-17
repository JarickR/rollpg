#nullable enable
using Godot;
using DiceArena.GodotUI;

namespace DiceArena.Godot
{
	/// <summary>
	/// Simple log wrapper around a RichTextLabel. No caret, no -1 errors.
	/// </summary>
	public partial class BattleLogPanel : Control
	{
		[Export] public NodePath LogLabelPath { get; set; } = default!;
		private RichTextLabel _log = null!;

		public override void _Ready()
		{
			_log = GetNode<RichTextLabel>(LogLabelPath);

			// sensible defaults for a log
			_log.ScrollActive = true;
			_log.FollowScroll = true;      // auto-sticks to bottom
			_log.BbcodeEnabled = false;    // set true if you want color/bold, etc.
			_log.Selectable = false;       // turn on if you want selection

			_log.SafeClear();
		}

		// Public API you can call from anywhere:
		public void WriteLine(string line) => _log.SafeAppendLine(line);
		public void Clear() => _log.SafeClear();
		public void SetAll(string text) => _log.SafeSetText(text);
	}
}
