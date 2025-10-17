// res://Scripts/Godot/UI/TextEditCaretGuard.cs
#nullable enable
using Godot;

namespace DiceArena.GodotUI
{
	/// <summary>
	/// Attach to a TextEdit used as a log. It clamps caret/scroll to valid ranges
	/// whenever the text changes or the control becomes visible, eliminating
	/// "Index p_line = -1 is out of bounds" errors on Godot 4.
	/// </summary>
	public partial class TextEditCaretGuard : TextEdit
	{
		[Export(PropertyHint.Range, "0.0,1.0,0.01")]
		public float AutoScrollDelaySec { get; set; } = 0.0f; // 0 = immediate clamp

		private double _accum = 0.0;
		private bool _pendingClamp = false;

		public override void _Ready()
		{
			// Typical log UX for Godot 4 TextEdit
			Editable = false;          // make it read-only via Editable
			ScrollSmooth = true;

			TextChanged += OnTextChanged;
			VisibilityChanged += OnVisibilityChanged;

			QueueClamp();
		}

		public override void _Process(double delta)
		{
			if (!_pendingClamp)
				return;

			if (AutoScrollDelaySec <= 0.0f)
			{
				ClampCaretAndScroll();
				_pendingClamp = false;
				_accum = 0.0;
				return;
			}

			_accum += delta;
			if (_accum >= AutoScrollDelaySec)
			{
				ClampCaretAndScroll();
				_pendingClamp = false;
				_accum = 0.0;
			}
		}

		private void OnTextChanged()
		{
			QueueClamp();
		}

		private void OnVisibilityChanged()
		{
			if (Visible)
				QueueClamp();
		}

		private void QueueClamp()
		{
			_pendingClamp = true;
			_accum = 0.0;
		}

		private void ClampCaretAndScroll()
		{
			// Ensure there is at least one valid line index.
			int lineCount = GetLineCount();
			if (lineCount <= 0)
			{
				SetCaretLine(0);
				SetCaretColumn(0);
				ScrollVertical = 0;
				return;
			}

			int lastLine = lineCount - 1;
			int lastLen = GetLine(lastLine).Length;

			// Clamp caret to end
			SetCaretLine(lastLine);
			SetCaretColumn(lastLen);

			// Scroll to bottom
			ScrollVertical = lastLine;
		}

		// -------- Optional convenience APIs if you want to call them directly --------

		public void SafeAppend(string text)
		{
			Text += text;
			QueueClamp();
		}

		public void SafeAppendLine(string line)
		{
			if (!line.EndsWith("\n"))
				line += "\n";
			Text += line;
			QueueClamp();
		}

		public void SafeClear()
		{
			Text = string.Empty;
			QueueClamp();
		}
	}
}
