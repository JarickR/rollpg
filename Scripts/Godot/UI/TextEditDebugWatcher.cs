// res://Scripts/Godot/UI/TextEditDebugWatcher.cs
#nullable enable
using Godot;
using System.Collections.Generic;

namespace DiceArena.GodotUI
{
	/// <summary>
	/// Autoload this to detect which TextEdit instance is causing "Index p_line = -1" errors.
	/// Logs node path + caret/line info + .NET stack trace whenever it observes an invalid caret.
	/// Safe to leave enabled during debugging; very light.
	/// </summary>
	public partial class TextEditDebugWatcher : Node
	{
		private readonly HashSet<TextEdit> _tracked = new();
		private const string MetaKey = "__debug_watcher_attached__";

		public override void _EnterTree()
		{
			var tree = GetTree();
			tree.NodeAdded += OnNodeAdded;

			if (tree.Root != null)
				AttachRecursively(tree.Root);
		}

		public override void _ExitTree()
		{
			GetTree().NodeAdded -= OnNodeAdded;
			_tracked.Clear();
		}

		public override void _Process(double delta)
		{
			// Prune dead refs
			_tracked.RemoveWhere(static t => t == null || !GodotObject.IsInstanceValid(t));

			// Periodic check
			foreach (var te in _tracked)
				CheckAndReport(te, reason: "process");
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
			if (te == null || !GodotObject.IsInstanceValid(te))
				return;

			if (te.HasMeta(MetaKey))
				return;

			te.SetMeta(MetaKey, true);
			_tracked.Add(te);

			te.TextChanged += () => CheckAndReport(te, reason: "TextChanged");
			te.VisibilityChanged += () => CheckAndReport(te, reason: "VisibilityChanged");
			te.GuiInput += (InputEvent _) => CheckAndReport(te, reason: "GuiInput");

			CheckAndReport(te, reason: "attach");
		}

		private static void CheckAndReport(TextEdit te, string reason)
		{
			if (te == null || !GodotObject.IsInstanceValid(te))
				return;

			int caretLine = te.GetCaretLine();
			int caretCol  = te.GetCaretColumn();
			int lines     = te.GetLineCount();

			if (caretLine < 0 || caretLine >= lines)
			{
				var path = te.GetPath();
				GD.PrintRich($"[color=yellow][TextEditDebugWatcher][/color] invalid caret detected ({reason})");
				GD.PrintRich($"   Path: [b]{path}[/b]");
				GD.PrintRich($"   Caret: line={caretLine}, col={caretCol}, lineCount={lines}");
				GD.PrintRich($"   Visible={te.Visible}, Editable={te.Editable}, ScrollVertical={te.ScrollVertical}");

				// Fully-qualify to avoid ambiguity with Godot.Environment
				GD.Print("   Stack:\n" + System.Environment.StackTrace);

				ClampCaret(te);
			}
		}

		private static void ClampCaret(TextEdit te)
		{
			int lines = te.GetLineCount();
			int lastLine = System.Math.Max(0, lines - 1);
			int lastLen  = te.GetLine(lastLine).Length;

			te.SetCaretLine(lastLine);
			te.SetCaretColumn(lastLen);
			te.ScrollVertical = lastLine;
		}
	}
}
