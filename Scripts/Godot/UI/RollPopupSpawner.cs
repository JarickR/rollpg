#nullable enable
using Godot;

namespace DiceArena.Godot
{
	public partial class RollPopupSpawner : Control
	{
		[Export] public PackedScene RollPopupScene { get; set; } = default!;

		public override void _Ready()
		{
			MouseFilter = MouseFilterEnum.Ignore;
			TopLevel = true;
			ZIndex = 200;
		}

		public void EmitDiceRoll(Control anchor, int finalValue, Texture2D[] faces)
		{
			if (RollPopupScene == null) { GD.PushWarning("RollPopupScene not set."); return; }

			Transform2D xf = anchor.GetGlobalTransformWithCanvas();
			Vector2 screenPos = xf * (anchor.Size * 0.5f);

			var popup = RollPopupScene.Instantiate<DiceRollPopup>();
			AddChild(popup);
			popup.PlayRoll(finalValue, screenPos, faces);
		}
	}
}
