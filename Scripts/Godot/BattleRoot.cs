// res://Scripts/Godot/BattleRoot.cs
#nullable enable
using Godot;

namespace DiceArena.Godot
{
	public partial class BattleRoot : Control
	{
		[Export] public NodePath ClassIconPath { get; set; } = "HUDRow/ClassIcon";
		[Export] public NodePath T1IconPath    { get; set; } = "HUDRow/T1Icon";
		[Export] public NodePath T2IconPath    { get; set; } = "HUDRow/T2Icon";

		private TextureRect _classIcon = null!;
		private TextureRect _t1Icon    = null!;
		private TextureRect _t2Icon    = null!;

		public override void _Ready()
		{
			_classIcon = GetNode<TextureRect>(ClassIconPath);
			_t1Icon    = GetNode<TextureRect>(T1IconPath);
			_t2Icon    = GetNode<TextureRect>(T2IconPath);

			// Start hidden while in loadout
			Visible = false;
		}

		public void HideBattle() => Visible = false;
		public void ShowBattle() => Visible = true;

		public void PaintFrom(Texture2D? classIcon, Texture2D? t1, Texture2D? t2)
		{
			_classIcon.Texture = classIcon;
			_t1Icon.Texture    = t1;
			_t2Icon.Texture    = t2;

			GD.Print($"[BattleRoot] PaintFrom -> class={classIcon?.ResourcePath ?? "<null>"} t1={t1?.ResourcePath ?? "<null>"} t2={t2?.ResourcePath ?? "<null>"}");
		}
	}
}
