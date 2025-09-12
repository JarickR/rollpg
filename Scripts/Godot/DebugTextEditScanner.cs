using Godot;

namespace DiceArena.GodotUI
{
	/// <summary>
	/// Safe helpers for TextEdit/RichTextLabel in Godot 4.
	/// (Replaces old uses of Readonly, Variant.Type.Int64, etc.)
	/// </summary>
	public static class DebugTextEditScanner
	{
		/// <summary>Make a TextEdit read-only (Godot 4: use Editable = false).</summary>
		public static void MakeReadOnly(TextEdit? te)
		{
			if (te == null) return;
			te.Editable = false;
		}

		/// <summary>
		/// Try to query line count from a variety of node types without throwing
		/// or relying on obsolete Variant type checks.
		/// </summary>
		public static int GetLineCountSafe(Node? node)
		{
			if (node == null) return 0;

			if (node is RichTextLabel rtl)
				return rtl.GetLineCount();

			if (node is TextEdit te)
				return te.GetLineCount();

			// Generic fallback if something exposes get_line_count at runtime
			if (node.HasMethod("get_line_count"))
			{
				// node.Call returns a Variant; in C# (Godot 4) cast via long then to int.
				try
				{
					var v = node.Call("get_line_count");
					int lines = (int)(long)v;
					return lines < 0 ? 0 : lines;
				}
				catch
				{
					return 0;
				}
			}

			return 0;
		}
	}
}
