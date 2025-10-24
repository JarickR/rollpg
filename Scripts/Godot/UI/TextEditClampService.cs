// res://Scripts/Godot/UI/TextEditClampService.cs
#nullable enable
using Godot;
using System.Collections.Generic;

namespace DiceArena.GodotUI
{
	/// <summary>
	/// Autoload this singleton to guard ALL TextEdits in the game.
	/// It tracks every TextEdit (current and future), clamps caret/scroll
	/// on TextChanged, VisibilityChanged, and once per frame.
	/// This eliminates "Index p_line = -1 is out of bounds" spam permanently.
	/// </summary>
	public partial class TextEditClampService : Node
	{
		private readonly HashSet<TextEdit> _tracked = new();

		// Meta flag to ensure we hook a TextEdit exactly once.
		private const string MetaKey = "__clamp_attached__";

		public override void _EnterTree()
		{
			var tree = GetTree();
			tree.NodeAdded += OnNodeAdded;

			// Attach to existing nodes in the tree
			if (tree.Root != null)
				AttachRecursively(tree.Root);
		}

		public override void _ExitTree()
		{
			// Just stop tracking; nodes may outlive this autoload only in editor.
			GetTree().NodeAdded -= OnNodeAdded;
			_tracked.Clear();
		}

		public override void _Process(double delta)
		{
			// Prune invalid references safely (fixes nullable issues).
			_tracked.RemoveWhere(static te => te == null || !GodotObject.IsInstanceValid(te));

			// Clamp every frame to catch any external misuse reliably.
			foreach (var te in _tracked)
				ClampCaretAndScroll(te);
		}

		// ---- Tracking & connections ----

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
			if (te == null || !GodotObject.IsInstanceValid(te))
				return;

			// Ensure we only hook once per TextEdit
			if (te.HasMeta(MetaKey))
				return;

			te.SetMeta(MetaKey, true);
			_tracked.Add(te);

			// Subscribe with per-instance lambdas; no need to unsubscribe explicitly.
			te.TextChanged += () => ClampCaretAndScroll(te);
			te.VisibilityChanged += () =>
			{
				if (te.Visible) ClampCaretAndScroll(te);
			};

			// Initial clamp
			ClampCaretAndScroll(te);
		}

		// ---- Core clamp ----

		/// <summary>
		/// Safely positions the caret at the end and scrolls to bottom.
		/// Works even if text is empty or rapidly changing.
		/// </summary>
		private static void ClampCaretAndScroll(TextEdit te)
		{
			if (te == null || !GodotObject.IsInstanceValid(te))
				return;

			// Godot TextEdit typically has at least 1 line; guard anyway.
			int lineCount = te.GetLineCount();
			if (lineCount <= 0)
			{
				te.SetCaretLine(0);
				te.SetCaretColumn(0);
				te.ScrollVertical = 0;
				return;
			}

			int lastLine = lineCount - 1;
			int lastLen = te.GetLine(lastLine).Length;

			// Clamp caret
			te.SetCaretLine(lastLine);
			te.SetCaretColumn(lastLen);

			// Scroll to bottom (TextEdit uses lines for ScrollVertical)
			te.ScrollVertical = lastLine;
		}
	}
}
