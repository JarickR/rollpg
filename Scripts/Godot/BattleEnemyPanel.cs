using System.Collections.Generic;
using Godot;

namespace DiceArena.Godot
{
	/// <summary>
	/// Compact enemy card: Name, HP, ARM, status chips.
	/// </summary>
	public partial class BattleEnemyPanel : PanelContainer
	{
		private Label _name = null!;
		private ProgressBar _hp = null!;
		private ProgressBar _arm = null!;
		private HBoxContainer _statuses = null!;

		public override void _Ready()
		{
			CustomMinimumSize = new Vector2(220, 0);
			SizeFlagsHorizontal = SizeFlags.ExpandFill;

			var root = new VBoxContainer
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				MouseFilter = MouseFilterEnum.Ignore
			};
			AddChild(root);

			_name = new Label { Text = "Enemy", AutowrapMode = TextServer.AutowrapMode.Off };
			_name.AddThemeFontSizeOverride("font_size", _name.GetThemeDefaultFontSize() + 2);
			_name.AddThemeColorOverride("font_color", Colors.White);
			root.AddChild(_name);

			_hp = new ProgressBar
			{
				MinValue = 0,
				MaxValue = 10,
				Value = 10,
				SizeFlagsHorizontal = SizeFlags.ExpandFill
			};
			_hp.ShowPercentage = true;
			root.AddChild(_hp);

			_arm = new ProgressBar
			{
				MinValue = 0,
				MaxValue = 10,
				Value = 3,
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				CustomMinimumSize = new Vector2(0, 10)
			};
			_arm.ShowPercentage = true;
			root.AddChild(_arm);

			_statuses = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
			root.AddChild(_statuses);
		}

		public void SetDisplayName(string name) => _name.Text = name;

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
			foreach (var c in _statuses.GetChildren())
				((Node)c).QueueFree();

			foreach (var s in statuses)
			{
				var lbl = new Label { Text = s, TooltipText = s };
				_statuses.AddChild(lbl);
			}
		}
	}
}
