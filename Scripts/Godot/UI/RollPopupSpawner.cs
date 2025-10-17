// res://Scripts/Godot/UI/RollPopupSpawner.cs
#nullable enable
using Godot;

namespace DiceArena.Godot
{
	/// <summary>
	/// Lives on a UI node (Control) and instantiates RollPopup scenes on demand.
	/// Position is in this node's canvas space; you can also anchor to a Control.
	/// </summary>
	public partial class RollPopupSpawner : Control
	{
		[ExportGroup("Scene Assets")]
		[Export] public PackedScene RollPopupScene { get; set; } = default!;

		[ExportGroup("Parenting")]
		/// <summary>
		/// Optional explicit parent container (e.g., a Control in the HUD).
		/// If null, popups are added as children of this node.
		/// </summary>
		[Export] public NodePath ParentContainerPath { get; set; } = default!;

		private Node _parentForPopups = null!;

		public override void _Ready()
		{
			_parentForPopups = string.IsNullOrEmpty(ParentContainerPath)
				? this
				: (GetNodeOrNull<Node>(ParentContainerPath) ?? this);

			// Ensure the spawner never blocks clicks and renders above typical UI.
			MouseFilter = MouseFilterEnum.Ignore;
			ZIndex = 1000;
		}

		/// <summary>
		/// Emit a roll popup at a canvas position (in this Control's local space).
		/// </summary>
		public void EmitRoll(Vector2 canvasPos, int value, string dieText, Texture2D? icon, bool isCrit = false, bool isFail = false)
		{
			if (RollPopupScene == null)
			{
				GD.PushError("[RollPopupSpawner] RollPopupScene not assigned.");
				return;
			}

			var popup = RollPopupScene.Instantiate<RollPopup>();
			_parentForPopups.AddChild(popup);

			// Make sure it renders above and never blocks input.
			popup.ZIndex = 1001;
			popup.MouseFilter = MouseFilterEnum.Ignore;

			popup.ShowRoll(value, dieText, icon, isCrit, isFail, canvasPos);
		}

		/// <summary>
		/// Emit a popup anchored to a Control (e.g., a hero panel).
		/// Converts the anchor's center (global screen) to this Control's local canvas position.
		/// </summary>
		public void EmitRollAtNode(Control anchor, int value, string dieText, Texture2D? icon, bool isCrit = false, bool isFail = false)
		{
			if (anchor == null || !IsInstanceValid(anchor))
			{
				EmitRoll(Vector2.Zero, value, dieText, icon, isCrit, isFail);
				return;
			}

			// 1) Get the anchor's center in GLOBAL screen coordinates
			var globalRect   = anchor.GetGlobalRect();
			var centerGlobal = globalRect.Position + globalRect.Size * 0.5f;

			// 2) Convert GLOBAL → THIS NODE'S LOCAL (CanvasItem transform math)
			//    In Godot 4, use CanvasItem.GetGlobalTransformWithCanvas().AffineInverse()
			//    and apply it to the global point via the '*' operator.
			var invToLocal   = GetGlobalTransformWithCanvas().AffineInverse();
			var centerLocal  = invToLocal * centerGlobal;

			EmitRoll(centerLocal, value, dieText, icon, isCrit, isFail);
		}
	}
}
