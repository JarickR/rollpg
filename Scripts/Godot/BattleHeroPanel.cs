using System.Collections.Generic;
using Godot;

namespace DiceArena.Godot
{
	/// <summary>
	/// Compact hero card for battle: Name, HP, ARM, status row, and a 6-slot loadout row.
	/// </summary>
	public partial class BattleHeroPanel : PanelContainer
	{
		// Public surface for BattleRoot
		public Button[] FaceButtons { get; private set; } = new Button[6];

		private Label _name = null!;
		private ProgressBar _hp = null!;
		private ProgressBar _arm = null!;
		private HBoxContainer _statuses = null!;
		private HBoxContainer _faces = null!;

		public override void _Ready()
		{
			// Visual shell
			CustomMinimumSize = new Vector2(300, 0);
			SizeFlagsHorizontal = SizeFlags.ExpandFill;

			var root = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore };
			AddChild(root);

			// Header row
			_name = new Label { Text = "Hero", AutowrapMode = TextServer.AutowrapMode.Off };
			_name.AddThemeFontSizeOverride("font_size", _name.GetThemeDefaultFontSize() + 2);
			_name.AddThemeColorOverride("font_color", Colors.White);
			root.AddChild(_name);

			// HP + ARM
			_hp = new ProgressBar { MinValue = 0, MaxValue = 10, Value = 10, SizeFlagsHorizontal = SizeFlags.ExpandFill };
			_hp.ShowPercentage = true;
			root.AddChild(_hp);

			_arm = new ProgressBar { MinValue = 0, MaxValue = 10, Value = 4, SizeFlagsHorizontal = SizeFlags.ExpandFill };
			_arm.ShowPercentage = true;
			_arm.CustomMinimumSize = new Vector2(0, 10);
			root.AddChild(_arm);

			// Status chips
			_statuses = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
			root.AddChild(_statuses);

			root.AddChild(new HSeparator());

			// Loadout faces row (6)
			_faces = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
			root.AddChild(_faces);

			for (int i = 0; i < 6; i++)
			{
				var b = new Button
				{
					Text = "–",
					TooltipText = "",
					FocusMode = FocusModeEnum.None,
					ToggleMode = false,
					CustomMinimumSize = new Vector2(40, 40),
					SizeFlagsHorizontal = SizeFlags.ShrinkCenter
				};
				_faces.AddChild(b);
				FaceButtons[i] = b;
			}
		}

		public void SetDisplayName(string name)
		{
			_name.Text = name;
		}

		public void SetHp(int current, int max)
		{
			_hp.MinValue = 0;
			_hp.MaxValue = Mathf.Max(1, max);
			_hp.Value = Mathf.Clamp(current, 0, max);
			_hp.TooltipText = $"HP {current}/{max}";
		}

		public void SetArmor(int armor, int maxShown = 10)
		{
			_arm.MinValue = 0;
			_arm.MaxValue = Mathf.Max(1, maxShown);
			_arm.Value = Mathf.Clamp(armor, 0, maxShown);
			_arm.TooltipText = $"Armor {armor}";
		}

		public void SetStatuses(IEnumerable<string> statuses)
		{
			// Clear
			foreach (var c in _statuses.GetChildren())
				((Node)c).QueueFree();

			// Tiny label chips
			foreach (var s in statuses)
			{
				var chip = new Label
				{
					Text = s,
					TooltipText = s,
					CustomMinimumSize = new Vector2(0, 0)
				};
				_statuses.AddChild(chip);
			}
		}

		/// <summary>Fill the six face slots (text and tooltips for now; you can assign icons later).</summary>
		public void SetFaces(string[] faces)
		{
			for (int i = 0; i < 6 && i < FaceButtons.Length; i++)
			{
				var txt = faces[i] ?? "";
				FaceButtons[i].Text = Shorten(txt);
				FaceButtons[i].TooltipText = txt;
			}
		}

		public void HighlightFace(int index) // index: 0..5
		{
			for (int i = 0; i < FaceButtons.Length; i++)
			{
				FaceButtons[i].AddThemeStyleboxOverride("normal", null);
				FaceButtons[i].AddThemeStyleboxOverride("hover", null);
				FaceButtons[i].AddThemeConstantOverride("content_margin_left", 0);
				FaceButtons[i].AddThemeConstantOverride("content_margin_right", 0);
			}
			if (index >= 0 && index < FaceButtons.Length)
			{
				// Simple visual nudge to show the rolled face
				FaceButtons[index].AddThemeConstantOverride("content_margin_left", 4);
				FaceButtons[index].AddThemeConstantOverride("content_margin_right", 4);
			}
		}

		private static string Shorten(string s)
		{
			if (string.IsNullOrWhiteSpace(s)) return "–";
			if (s.Length <= 8) return s;
			return s.Substring(0, 8);
		}
	}
}
