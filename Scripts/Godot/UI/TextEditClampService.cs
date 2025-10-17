// res://Scripts/Godot/UI/TextEditClampService.cs
#nullable enable
using Godot;

namespace DiceArena.GodotUI
{
	/// <summary>
	/// Autoload this singleton to guard ALL TextEdits in the tree.
	/// It hooks TextChanged/VisibilityChanged and clamps caret/scroll to valid ranges.
	/// This eliminates "Index p_line = -1" errors project-wide.
	/// </summary>
	public partial class TextEditClampService : Node
	{
		public override void _EnterTree()
		{
			// Hook for nodes added later
			GetTree().NodeAdded += OnNodeAdded;

			// Attach to any TextEdits that already exist
			if (GetTree().Root != null)
				AttachRecursively(GetTree().Root);
		}

		public override void _ExitTree()
		{
			GetTree().NodeAdded -= OnNodeAdded;
		}

		private void OnNodeAdded(Node n)
		{
			if (n is TextEdit te)
				Attach(te);
		}

		private void AttachRecursively(Node root)
		{
			if (root is TextEdit te)
				Attach(te);

			foreach (var child in root.GetChildren())
				AttachRecursively(child);
		}

		private void Attach(TextEdit te)
		{
			// Avoid double-connecting by disconnecting first (no-op if not connected)
			te.TextChanged -= () => OnTextChanged(te);
			te.VisibilityChanged -= () => OnVisibilityChanged(te);

			te.TextChanged += () => OnTextChanged(te);
			te.VisibilityChanged += () => OnVisibilityChanged(te);

			// Initial clamp just in case
			ClampCaretAndScroll(te);
		}

		private void OnTextChanged(TextEdit te)
		{
			ClampCaretAndScroll(te);
		}

		private void OnVisibilityChanged(TextEdit te)
		{
			if (te.Visible)
				ClampCaretAndScroll(te);
		}

		/// <summary>
		/// Safely positions the caret at the end and scrolls to bottom.
		/// Works even if text is empty or rapidly changing.
		/// </summary>
		private static void ClampCaretAndScroll(TextEdit te)
		{
			if (te == null || !GodotObject.IsInstanceValid(te))
				return;

			// Godot TextEdit should always have at least 1 line, but guard anyway.
			int lineCount = te.GetLineCount();
			if (lineCount <= 0)
			{
				te.SetCaretLine(0);
				te.SetCaretColumn(0);
				te.ScrollVertical = 0;
				return;
			}

			int lastLine = lineCount - 1;

			// GetLine(lastLine) can be empty; Length is safe.
			int lastLen = te.GetLine(lastLine).Length;

			// Clamp caret
			te.SetCaretLine(lastLine);
			te.SetCaretColumn(lastLen);

			// Scroll to bottom (lines are the unit)
			te.ScrollVertical = lastLine;
		}
	}
}
