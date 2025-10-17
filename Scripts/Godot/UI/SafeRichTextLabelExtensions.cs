#nullable enable
using System;
using Godot;

namespace DiceArena.GodotUI
{
	public static class SafeRichTextLabelExtensions
	{
		public static void SafeClear(this RichTextLabel r)
		{
			if (r is null) return;
			r.Clear();
			if (r.ScrollActive) r.ScrollToLine(0);
		}

		public static void SafeSetText(this RichTextLabel r, string text)
		{
			if (r is null) return;
			r.Clear();
			r.AppendText(text ?? string.Empty);
			r.SafeScrollToEnd();
		}

		public static void SafeAppendLine(this RichTextLabel r, string line)
		{
			if (r is null) return;
			if (!line.EndsWith("\n")) line += "\n";
			r.AppendText(line);
			r.SafeScrollToEnd();
		}

		public static void SafeScrollToEnd(this RichTextLabel r)
		{
			if (r is null) return;
			if (!r.ScrollActive) return;
			int last = Math.Max(0, r.GetLineCount() - 1);
			r.ScrollToLine(last);
		}
	}
}
