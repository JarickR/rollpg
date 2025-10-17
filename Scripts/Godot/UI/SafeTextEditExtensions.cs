#nullable enable
using System;
using Godot;

namespace DiceArena.GodotUI
{
	public static class SafeTextEditExtensions
	{
		public static void SafeClear(this TextEdit t)
		{
			if (t is null) return;

			// Always leave at least one empty line so caret ops are valid.
			t.Text = string.Empty;
		}

		public static void SafeAppendLine(this TextEdit t, string line)
		{
			if (t is null) return;

			if (!line.EndsWith("\n"))
				line += "\n";

			// Godot 4: append via Text property (InsertTextAtCaret can explode if caret is invalid).
			t.Text += line;

			t.SafeMoveCaretToEnd();
			t.SafeScrollToBottom();
		}

		public static void SafeSetText(this TextEdit t, string text)
		{
			if (t is null) return;

			t.Text = text ?? string.Empty;

			t.SafeMoveCaretToEnd();
			t.SafeScrollToBottom();
		}

		public static void SafeMoveCaretToEnd(this TextEdit t)
		{
			if (t is null) return;

			int count = t.GetLineCount();
			int lastLine = Math.Max(0, count - 1);
			int lastCol  = t.GetLine(lastLine).Length;
		}

		public static void SafeScrollToBottom(this TextEdit t)
		{
			if (t is null) return;

			int count = t.GetLineCount();
			int lastLine = Math.Max(0, count - 1);
		}
	}
}
