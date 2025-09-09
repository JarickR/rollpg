using Godot;

namespace DiceArena.GodotUI
{
	public partial class BattleLogPanel : PanelContainer
	{
		private Label _title = default!;
		private Button _clearBtn = default!;
		private RichTextLabel _log = default!;

		public override void _Ready()
		{
			var root = new VBoxContainer();
			root.AddThemeConstantOverride("separation", 6);
			AddChild(root);

			var header = new HBoxContainer();
			header.AddThemeConstantOverride("separation", 8);
			root.AddChild(header);

			_title = new Label { Text = "Battle Log" };
			header.AddChild(_title);

			_clearBtn = new Button { Text = "Clear" };
			_clearBtn.Pressed += Clear;
			header.AddChild(_clearBtn);

			_log = new RichTextLabel
			{
				FitContent = false,
				ScrollActive = true,
				ScrollFollowing = true,        // auto-follow once we scroll
				AutowrapMode = TextServer.AutowrapMode.Word,
				CustomMinimumSize = new Vector2(520, 220),
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical = SizeFlags.ShrinkCenter
			};
			root.AddChild(_log);
		}

		public void Log(string line) => AppendLine(line);

		public void AppendLine(string text)
		{
			if (_log == null) return;
			_log.AppendText((text ?? string.Empty) + "\n");

			// Defer the scroll until the frame after the append,
			// to avoid p_line = -1 issues.
			CallDeferred(nameof(ScrollToBottom));
		}

		private void ScrollToBottom()
		{
			if (_log == null) return;
			int lines = _log.GetLineCount();
			if (lines > 0)
				_log.ScrollToLine(lines - 1);
		}

		public void Clear()
		{
			if (_log == null) return;
			_log.Clear();
		}
	}
}
