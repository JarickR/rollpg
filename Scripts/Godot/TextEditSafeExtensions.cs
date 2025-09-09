using Godot;

namespace DiceArena.GodotUI
{
	/// <summary>
	/// Safe helpers that work with RichTextLabel or any text-like Node (e.g., TextEdit),
	/// without hard-referencing API members that may not exist in your bindings.
	/// </summary>
	public static class TextEditSafeExtensions
	{
		/// <summary>
		/// Append a line (adds a newline) and auto-scroll safely (deferred).
		/// Works for RichTextLabel and generic text controls.
		/// </summary>
		public static void AppendLineSafe(this Node node, string line)
		{
			if (node == null) return;

			string textToAdd = (line ?? string.Empty) + "\n";

			// Fast-path: RichTextLabel has AppendText.
			if (node is RichTextLabel rtl)
			{
				rtl.AppendText(textToAdd);
				UiScroll.ToBottom(rtl);
				return;
			}

			// Generic path: try common methods dynamically.
			// 1) append_text(str)
			if (node.HasMethod("append_text"))
			{
				node.Call("append_text", textToAdd);
				UiScroll.ToBottom(node);
				return;
			}

			// 2) get_text() / set_text(str)
			string curr = "";
			if (node.HasMethod("get_text"))
			{
				var v = node.Call("get_text");
				if (v.VariantType == Variant.Type.String)
					curr = (string)v;
				else
					curr = v.ToString();
			}

			if (node.HasMethod("set_text"))
			{
				node.Call("set_text", curr + textToAdd);
				UiScroll.ToBottom(node);
				return;
			}

			// 3) clear() + set_text as fallback (rare)
			if (node.HasMethod("clear"))
			{
				node.Call("clear");
			}
		}

		/// <summary>
		/// Clear text content safely without trying to scroll.
		/// Works for RichTextLabel and generic text controls.
		/// </summary>
		public static void ClearSafe(this Node node)
		{
			if (node == null) return;

			if (node is RichTextLabel rtl)
			{
				rtl.Clear();
				return;
			}

			if (node.HasMethod("clear"))
			{
				node.Call("clear");
				return;
			}

			if (node.HasMethod("set_text"))
			{
				node.Call("set_text", "");
				return;
			}
		}

		/// <summary>
		/// Request a safe, deferred scroll-to-bottom using UiScroll, for any supported node.
		/// </summary>
		public static void SafeScrollToBottom(this Node node)
		{
			UiScroll.ToBottom(node);
		}
	}
}
