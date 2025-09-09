// Scripts/Godot/UiScroll.cs
using Godot;

namespace DiceArena.GodotUI
{
	/// <summary>
	/// No-op scroll helper. Left in place so callers can compile safely.
	/// </summary>
	public static class UiScroll
	{
		/// <summary>
		/// Intentionally does nothing (prevents TextEdit/RichTextLabel p_line errors).
		/// </summary>
		public static void ToBottom(Node node) { /* no-op */ }
	}
}
