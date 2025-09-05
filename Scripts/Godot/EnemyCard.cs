// res://Scripts/Godot/EnemyCard.cs
using Godot;
using DiceArena.Engine;

namespace DiceArena.GodotUI
{
	public partial class EnemyCard : Panel
	{
		public Enemy? Data;
		private Label _title;
		private TextureProgressBar _hp;
		private Label _armor;

		public override void _Ready()
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;

			var vb = new VBoxContainer
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical = Control.SizeFlags.ShrinkCenter
			};
			vb.AddThemeConstantOverride("separation", 6);
			AddChild(vb);

			_title = UiUtils.MakeLabel("Enemy", 18, bold: true);
			vb.AddChild(_title);

			_hp = UiUtils.MakeHpBar(10, 10);
			vb.AddChild(_hp);

			_armor = UiUtils.MakeLabel("Armor: 0", 14);
			vb.AddChild(_armor);

			Refresh();
		}

		public void Bind(Enemy e)
		{
			Data = e;
			Refresh();
		}

		public void Refresh()
		{
			if (Data == null) return;
			_title.Text = $"{Data.Name} (T{Data.Tier}) — HP {Data.Hp}/{Data.MaxHp}";
			_hp.MaxValue = Data.MaxHp;
			_hp.Value = Data.Hp;
			_armor.Text = $"Armor: {Data.Armor}";
		}
	}
}
