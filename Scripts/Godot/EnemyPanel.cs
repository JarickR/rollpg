// Scripts/Godot/EnemyPanel.cs
using Godot;

namespace DiceArena.Godot
{
	public partial class EnemyPanel : Control
	{
		// Hook these up in the scene (optional).
		[Export] public NodePath? NameLabelPath { get; set; }
		[Export] public NodePath? HpLabelPath { get; set; }
		[Export] public NodePath? IconTextureRectPath { get; set; }

		private Label? _name;
		private Label? _hp;
		private TextureRect? _icon;

		public override void _Ready()
		{
			_name = GetNodeOrNull<Label>(NameLabelPath);
			_hp = GetNodeOrNull<Label>(HpLabelPath);
			_icon = GetNodeOrNull<TextureRect>(IconTextureRectPath);

			GD.Print("[EnemyPanel] Ready");
		}

		// ---- Public helpers you can call from other scripts ----

		public void SetEnemyName(string name)
		{
			if (_name != null)
				_name.Text = name;
		}

		public void SetHp(int current, int max)
		{
			if (_hp != null)
				_hp.Text = $"{current}/{max}";
		}

		public void SetIcon(Texture2D? texture)
		{
			if (_icon != null)
				_icon.Texture = texture;
		}

		public void DebugLog(string message)
		{
			GD.Print($"[EnemyPanel] {message}");
		}
	}
}
