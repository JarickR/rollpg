// res://Scripts/Godot/EnemyCard.cs
#nullable enable
using Godot;

namespace RollPG.GodotUI
{
	/// <summary>
	/// Optional small card for each enemy (top-left in your sketch).
	/// </summary>
	public partial class EnemyCard : Control
	{
		[Export] public NodePath NameLabelPath { get; set; } = "NameLabel";
		[Export] public NodePath HpBarPath     { get; set; } = "HP";
		[Export] public NodePath ArmBarPath    { get; set; } = "ARM";
		[Export] public NodePath StatusRowPath { get; set; } = "StatusRow";

		private Label _name = null!;
		private ProgressBar _hp = null!;
		private ProgressBar _arm = null!;
		private HBoxContainer _status = null!;

		public override void _Ready()
		{
			_name   = GetNode<Label>(NameLabelPath);
			_hp     = GetNode<ProgressBar>(HpBarPath);
			_arm    = GetNode<ProgressBar>(ArmBarPath);
			_status = GetNode<HBoxContainer>(StatusRowPath);
		}

		public void Show(string name, int hp, int maxHp, int armor, Texture2D[]? statusIcons)
		{
			_name.Text = name;
			_hp.MaxValue = maxHp <= 0 ? 1 : maxHp;
			_hp.Value = Mathf.Clamp(hp, 0, (int)_hp.MaxValue);
			_arm.MaxValue = 20;
			_arm.Value = Mathf.Clamp(armor, 0, 20);

			foreach (var c in _status.GetChildren()) c.QueueFree();
			if (statusIcons != null)
				foreach (var t in statusIcons) _status.AddChild(new TextureRect { Texture = t, CustomMinimumSize = new Vector2(20,20) });
		}
	}
}
