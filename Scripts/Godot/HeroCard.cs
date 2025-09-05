// res://Scripts/Godot/HeroCard.cs
using Godot;
using DiceArena.Engine;

namespace DiceArena.GodotUI
{
	public partial class HeroCard : Panel
	{
		public Hero? Data;
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

			_title = UiUtils.MakeLabel("Hero", 18, bold: true);
			vb.AddChild(_title);

			_hp = UiUtils.MakeHpBar(20, 20);
			vb.AddChild(_hp);

			_armor = UiUtils.MakeLabel("Armor: 0", 14);
			vb.AddChild(_armor);

			Refresh();
		}

		public void Bind(Hero h)
		{
			Data = h;
			Refresh();
		}

		public void Refresh()
		{
			if (Data == null) return;
			_title.Text = $"{Data.ClassId.ToUpper()} (P{Data.Id.TrimStart('P')}) — HP {Data.Hp}/{Data.MaxHp}";
			_hp.MaxValue = Data.MaxHp;
			_hp.Value = Data.Hp;
			_armor.Text = $"Armor: {Data.Armor}";
		}
	}
}
