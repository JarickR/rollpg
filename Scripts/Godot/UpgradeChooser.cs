// res://Scripts/Godot/UpgradeChooser.cs
#nullable enable
using Godot;
using System;

namespace DiceArena.GodotUI
{
	/// <summary>
	/// Modal overlay that lets the player choose which of the 4 faces to upgrade.
	/// Call Configure(...) before showing.
	/// </summary>
	public partial class UpgradeChooser : Control
	{
		public Action<int>? OnChosen;   // receives slot index [0..3]
		public Action? OnCanceled;

		private string[] _faceNames = Array.Empty<string>();
		private bool[] _upgradable = Array.Empty<bool>();

		// UI
		private ColorRect _backdrop = default!;
		private CenterContainer _center = default!;
		private PanelContainer _card = default!;
		private VBoxContainer _root = default!;
		private Label _hint = default!;

		public override void _Ready()
		{
			Name = "UpgradeChooser";
			AnchorLeft = 0; AnchorTop = 0; AnchorRight = 1; AnchorBottom = 1;
			OffsetLeft = 0; OffsetTop = 0; OffsetRight = 0; OffsetBottom = 0;
			MouseFilter = MouseFilterEnum.Stop;
			SetProcessUnhandledKeyInput(true);

			BuildUi();
		}

		public override void _UnhandledKeyInput(InputEvent e)
		{
			if (e.IsActionPressed("ui_cancel"))
			{
				OnCanceled?.Invoke();
				QueueFree();
			}
		}

		/// <summary>
		/// Provide current face names (length 4) and which can be upgraded (length 4).
		/// </summary>
		public void Configure(string[] faceNames, bool[] upgradable)
		{
			_faceNames = faceNames;
			_upgradable = upgradable;
			if (IsNodeReady()) RebuildButtons();
		}

		private void BuildUi()
		{
			_backdrop = new ColorRect { Color = new Color(0, 0, 0, 0.45f), MouseFilter = MouseFilterEnum.Ignore };
			_backdrop.AnchorLeft = 0; _backdrop.AnchorTop = 0; _backdrop.AnchorRight = 1; _backdrop.AnchorBottom = 1;
			AddChild(_backdrop);

			_center = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
			AddChild(_center);

			_card = new PanelContainer();
			var style = new StyleBoxFlat
			{
				BgColor = new Color(0.10f, 0.12f, 0.16f, 1f),
				CornerRadiusTopLeft = 10, CornerRadiusTopRight = 10,
				CornerRadiusBottomLeft = 10, CornerRadiusBottomRight = 10,
				ContentMarginLeft = 16, ContentMarginRight = 16,
				ContentMarginTop = 16, ContentMarginBottom = 16
			};
			_card.AddThemeStyleboxOverride("panel", style);
			_card.CustomMinimumSize = new Vector2(520, 220);
			_center.AddChild(_card);

			_root = new VBoxContainer();
			_root.AddThemeConstantOverride("separation", 12);
			_card.AddChild(_root);

			var title = new Label { Text = "Choose a face to upgrade", HorizontalAlignment = HorizontalAlignment.Center };
			title.AddThemeFontSizeOverride("font_size", 20);
			title.AddThemeColorOverride("font_color", Colors.White);
			_root.AddChild(title);

			_hint = new Label
			{
				Text = "Click one of the faces below. Disabled faces are already max tier.",
				HorizontalAlignment = HorizontalAlignment.Center
			};
			_hint.AddThemeFontSizeOverride("font_size", 13);
			_root.AddChild(_hint);

			// buttons row(s)
			RebuildButtons();

			// cancel row
			var row = new HBoxContainer();
			row.Alignment = BoxContainer.AlignmentMode.End;
			row.AddThemeConstantOverride("separation", 8);
			_root.AddChild(row);

			var cancel = new Button { Text = "Cancel" };
			cancel.Pressed += () => { OnCanceled?.Invoke(); QueueFree(); };
			row.AddChild(cancel);
		}

		private void RebuildButtons()
		{
			// Remove any old face buttons (keep title/hint and last row)
			while (_root.GetChildCount() > 2)
			{
				var idx = _root.GetChildCount() - 2; // keep last row (cancel)
				var node = _root.GetChild(idx);
				if (node is HBoxContainer && idx >= 2) // face rows are between title/hint and footer
				{
					_root.RemoveChild(node);
					node.QueueFree();
				}
				else break;
			}

			// Create grid: two rows of two buttons
			int created = 0;
			for (int r = 0; r < 2; r++)
			{
				var row = new HBoxContainer();
				row.AddThemeConstantOverride("separation", 12);
				row.Alignment = BoxContainer.AlignmentMode.Center;
				_root.AddChild(row);

				for (int c = 0; c < 2; c++)
				{
					if (created >= 4) break;
					int i = created++;
					var label = (i < _faceNames.Length) ? _faceNames[i] : $"Face {i+1}";
					bool can = (i < _upgradable.Length) && _upgradable[i];

					var btn = new Button
					{
						Text = $"{i+1}: {label}",
						CustomMinimumSize = new Vector2(200, 44),
						Disabled = !can
					};
					if (!can)
					{
						btn.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
					}

					btn.Pressed += () =>
					{
						OnChosen?.Invoke(i);
						QueueFree();
					};

					row.AddChild(btn);
				}
			}

			if (_upgradable.Length == 4 && !_upgradable[0] && !_upgradable[1] && !_upgradable[2] && !_upgradable[3])
			{
				_hint.Text = "No faces can be upgraded.";
			}
		}
	}
}
