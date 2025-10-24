// Scripts/Util/SafeTextEditExtensions.cs
#nullable enable
using Godot;

namespace DiceArena.Godot.Util
{
	public static class SafeTextEditExtensions
	{
		/// <summary>
		/// Safely append text and scroll to end for either RichTextLabel or TextEdit.
		/// Never crashes if the control is empty.
		/// </summary>
		public static void SafeAppendAndScroll(this Control control, string text)
		{
			switch (control)
			{
				case RichTextLabel rtl:
					rtl.AppendText(text);
					rtl.SafeScrollToEnd();
					break;

				case TextEdit te:
					// Append
					if (string.IsNullOrEmpty(te.Text))
						te.Text = text;
					else
						te.Text += text;

					te.SafeScrollToEnd();
					break;

				default:
					// Not a text control; ignore.
					break;
			}
		}

		/// <summary>
		/// Scrolls to the last line safely (RichTextLabel).
		/// </summary>
		public static void SafeScrollToEnd(this RichTextLabel rtl)
		{
			int lines = rtl.GetLineCount();           // 0..N
			int last  = Mathf.Max(0, lines - 1);      // clamp to >= 0
			// RichTextLabel has ScrollToLine in Godot 4.x
			rtl.ScrollToLine(last);
		}

		/// <summary>
		/// Moves caret to the last line/column and ensures it’s visible (TextEdit).
		/// Uses Call() so it compiles across API differences.
		/// </summary>
		public static void SafeScrollToEnd(this TextEdit te)
		{
			// Compute last valid line/column
			int lines = te.GetLineCount();            // may be 0
			int lastLine = Mathf.Max(0, lines - 1);
			int lastCol  = 0;
			if (lines > 0)
			{
				string lastLineText = te.GetLine(lastLine) ?? string.Empty;
				lastCol = lastLineText.Length;
			}

			// Try Godot 4-style caret API via Call()
			// If it doesn’t exist on this build, silently no-op.
			try { te.Call("set_caret_line", lastLine); } catch { }
			try { te.Call("set_caret_column", lastCol); } catch { }

			// Godot 4: ensure_caret_visible
			try { te.Call("ensure_caret_visible"); } catch { }

			// Fallback scroll if available
			try { te.Call("scroll_to_line", lastLine); } catch { }
			// Older 3.x-style names (harmless if missing)
			try { te.Call("cursor_set_line", lastLine); } catch { }
			try { te.Call("cursor_set_column", lastCol); } catch { }
			try { te.Call("center_viewport_to_caret"); } catch { }
		}
	}
}
