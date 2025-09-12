using Godot;

namespace DiceArena.GodotUI
{
	/// <summary>
	/// Simple battle log panel with a header, clear button, and a RichTextLabel.
	/// Provides AppendLine(string) and AppendLine(string, Color?) for the logger.
	/// </summary>
	public partial class BattleLogPanel : VBoxContainer
	{
		private Label _title = default!;
		private Button _clearBtn = default!;
		private RichTextLabel _log = default!;

		public override void _Ready()
		{
			AddThemeConstantOverride("separation", 6);

			// Header
			var header = new HBoxContainer();
			header.AddThemeConstantOverride("separation", 8);
			AddChild(header);

			_title = new Label { Text = "Battle Log" };
			header.AddChild(_title);

			_clearBtn = new Button { Text = "Clear" };
			_clearBtn.Pressed += Clear;
			header.AddChild(_clearBtn);

			// Log body
			_log = new RichTextLabel
			{
				FitContent = false,
				ScrollActive = true,
				AutowrapMode = TextServer.AutowrapMode.Word
			};

			// Sizing
			_log.CustomMinimumSize = new Vector2(520, 220);
			_log.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			_log.SizeFlagsVertical   = SizeFlags.ShrinkCenter;

			AddChild(_log);
		}

		/// <summary>Write a line in default color.</summary>
		public void AppendLine(string text) => AppendLine(text, null);

		/// <summary>Write a colored line.</summary>
		public void AppendLine(string text, Color? color)
		{
			if (_log == null) return;

			bool pushed = false;
			if (color.HasValue)
			{
				_log.PushColor(color.Value);
				pushed = true;
			}

			_log.AppendText((text ?? string.Empty) + "\n");

			if (pushed)
				_log.Pop();

			// Safe, deferred scroll (no TextEdit crash paths)
			UiScroll.ToBottom(_log);
		}

		public void Clear()
		{
			if (_log == null) return;
			_log.Clear();
			// No scroll when empty
		}
	}
}
