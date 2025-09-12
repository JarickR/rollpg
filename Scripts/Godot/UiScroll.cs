using Godot;

namespace DiceArena.GodotUI
{
	public static class UiScroll
	{
		public static void ToBottom(Node node)
		{
			if (node == null) return;
			node.CallDeferred(nameof(DoScrollToBottom), node);
		}

		public static void DoScrollToBottom(Node node)
		{
			if (node is RichTextLabel rtl)
			{
				int lines = rtl.GetLineCount();
				if (lines > 0)
					rtl.ScrollToLine(lines - 1);
			}
			// If it's TextEdit or anything else: no-op on purpose
		}
	}
}
