// res://Scripts/Godot/LoadoutToBattleBridge.cs
#nullable enable
using Godot;
using System.Collections.Generic;

namespace DiceArena.Godot
{
	/// <summary>
	/// Copies chosen loadout into the Battle HUD (if present) and switches to the battle screen.
	/// Re-resolves nodes at finalize time, forcibly removes the live LoadoutScreen (hide+deparent+free),
	/// and verifies/removes any stragglers on the next frame.
	/// </summary>
	public partial class LoadoutToBattleBridge : Node
	{
		// Absolute paths that match your scene:
		private static readonly NodePath P1_PANEL_PATH   = new("/root/GameRoot/LoadoutScreen/LayoutRoot/MainRow/RightPanel/PlayerLoadoutPanel");
		private static readonly NodePath FINALIZE_BTN    = new("/root/GameRoot/LoadoutScreen/LayoutRoot/HeaderRow/FinalizeBtn");
		private static readonly NodePath LOADOUT_SCREEN  = new("/root/GameRoot/LoadoutScreen");
		private static readonly NodePath BATTLEROOT      = new("/root/GameRoot/BattleRoot");
		// Node that has the BattleHUD script attached:
		private static readonly NodePath BATTLE_HUD_PATH = new("/root/GameRoot/BattleRoot/HSplitContainer");

		// optional caches
		private Control? _battleRoot;
		private Button? _finalizeBtn;

		public override void _Ready()
		{
			var root = GetTree().Root;

			_battleRoot  = root.GetNodeOrNull<Control>(BATTLEROOT);
			_finalizeBtn = root.GetNodeOrNull<Button>(FINALIZE_BTN);

			GD.Print($"[Bridge] Ready: BattleRoot={(_battleRoot!=null?"ok":"NULL")} FinalizeBtn={(_finalizeBtn!=null?"ok":"NULL")}");

			if (_finalizeBtn != null && !_finalizeBtn.IsConnected(Button.SignalName.Pressed, Callable.From(OnFinalizePressed)))
				_finalizeBtn.Pressed += OnFinalizePressed;

			if (_battleRoot != null)
			{
				_battleRoot.Visible     = true;
				_battleRoot.TopLevel    = false;
				_battleRoot.ZIndex      = 0;
				_battleRoot.ProcessMode = Node.ProcessModeEnum.Inherit;
				_battleRoot.MouseFilter = Control.MouseFilterEnum.Pass;
			}
		}

		// keep editor stub if you wired it in the editor
		private void _on_finalize_btn_pressed()
		{
			GD.Print("[Bridge] Finalize pressed (editor wire).");
			FinalizeAndShowBattle();
		}

		private void OnFinalizePressed()
		{
			GD.Print("[Bridge] Finalize pressed (code wire).");
			FinalizeAndShowBattle();
		}

		/// <summary>
		/// Re-resolves live nodes, paints HUD (optional), then synchronously removes LoadoutScreen
		/// (hide + RemoveChild + QueueFree) and verifies removal next frame.
		/// </summary>
		private async void FinalizeAndShowBattle()
		{
			GD.Print("[Bridge] FinalizeAndShowBattle() begin.");

			var root          = GetTree().Root;
			var gameRoot      = root.GetNodeOrNull<Node>("/root/GameRoot");
			var loadoutScreen = root.GetNodeOrNull<Control>(LOADOUT_SCREEN);
			var battleRoot    = root.GetNodeOrNull<Control>(BATTLEROOT);
			var hud           = root.GetNodeOrNull<RollPG.GodotUI.BattleHUD>(BATTLE_HUD_PATH);
			var p1Panel       = root.GetNodeOrNull<PlayerLoadoutPanel>(P1_PANEL_PATH);

			GD.Print($"[Bridge] Re-resolved: Loadout={(loadoutScreen!=null?"ok":"NULL")} " +
					 $"Battle={(battleRoot!=null?"ok":"NULL")} HUD={(hud!=null?"ok":"NULL")} P1={(p1Panel!=null?"ok":"NULL")}");

			// Paint HUD if present (optional)
			if (hud != null)
			{
				Texture2D? classIcon = null;
				var t1 = new List<Texture2D>();
				var t2 = new List<Texture2D>();

				if (p1Panel != null)
				{
					p1Panel.GetChosenTextures(out classIcon, out var t1Icons, out var t2Icons);
					if (t1Icons != null) t1.AddRange(t1Icons);
					if (t2Icons != null) t2.AddRange(t2Icons);
				}

				var loadoutIcons = new List<Texture2D>(t1);
				loadoutIcons.AddRange(t2);

				hud.ShowHero(
					name: "Player 1",
					hp: 10, maxHp: 12, armor: 0,
					classIcon: classIcon,
					tier1Icon: t1.Count > 0 ? t1[0] : null,
					tier2Icon: t2.Count > 0 ? t2[0] : null,
					statusIcons: null,
					loadoutIcons: loadoutIcons
				);
				hud.ShowEnemies(new List<(string, int, int, int, IReadOnlyList<Texture2D>?)>());

				GD.Print("[Bridge] HUD painted.");
			}
			else
			{
				GD.PushWarning("[Bridge] BattleHUD not found — skipping HUD paint, proceeding with screen switch.");
			}

			// Synchronously remove LoadoutScreen now.
			if (loadoutScreen != null)
			{
				var id = loadoutScreen.GetInstanceId();
				GD.Print($"[Bridge] Removing LoadoutScreen instance id={id}");

				// Hide & disable immediately
				ForceHideTree(loadoutScreen);

				// Deparent synchronously (so it disappears right away)
				var parent = loadoutScreen.GetParent();
				if (parent != null)
				{
					parent.RemoveChild(loadoutScreen);
					GD.Print("[Bridge] LoadoutScreen removed from parent.");
				}

				// Free on idle
				loadoutScreen.QueueFree();
				GD.Print("[Bridge] LoadoutScreen QueueFree() queued.");
			}
			else
			{
				GD.PushWarning("[Bridge] LoadoutScreen not found at finalize time.");
			}

			// Ensure BattleRoot is interactable/visible
			if (battleRoot != null)
			{
				battleRoot.Visible     = true;
				battleRoot.TopLevel    = false;
				battleRoot.ZIndex      = 0;
				battleRoot.ProcessMode = Node.ProcessModeEnum.Inherit;
				battleRoot.MouseFilter = Control.MouseFilterEnum.Pass;
				EnableInputsRecursively(battleRoot);
				battleRoot.QueueRedraw();
				GD.Print("[Bridge] BattleRoot enabled with normal input.");
			}
			else
			{
				GD.PushWarning("[Bridge] BattleRoot not found.");
			}

			// Wait a frame, then nuke any straggler nodes named "LoadoutScreen" under GameRoot.
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

			int survivors = 0;
			if (gameRoot != null)
			{
				foreach (var child in gameRoot.GetChildren())
				{
					if (child is Node n && n.Name == "LoadoutScreen")
					{
						survivors++;
						GD.PushWarning("[Bridge] Straggler LoadoutScreen found; hiding & freeing.");
						if (n is CanvasItem ci) ForceHideTree(ci);
						n.QueueFree();
					}
				}
			}

			GD.Print($"[Bridge] Post-frame verification: stragglers={survivors}");
		}

		// -------- helpers -------
		private static void ForceHideTree(CanvasItem node)
		{
			node.Visible = false;
			node.Modulate = new Color(1,1,1,0);
			if (node is Control c)
			{
				c.ProcessMode = Node.ProcessModeEnum.Disabled;
				c.MouseFilter = Control.MouseFilterEnum.Ignore;
			}
			foreach (var child in node.GetChildren())
			{
				if (child is CanvasItem ci) ForceHideTree(ci);
			}
		}

		private static void EnableInputsRecursively(Node n)
		{
			foreach (var child in n.GetChildren())
			{
				if (child is Control c)
				{
					c.ProcessMode = Node.ProcessModeEnum.Inherit;
					c.MouseFilter = (c is Button)
						? Control.MouseFilterEnum.Stop
						: Control.MouseFilterEnum.Pass;
				}
				EnableInputsRecursively(child);
			}
		}
	}
}
