// Scripts/Engine/Loadout/LoadoutScreenExtensions.cs
using Godot;
using System.Collections.Generic;
using System.Linq;
using DiceArena.Godot;   // this is where your LoadoutScreen + PlayerLoadoutPanel live
using DiceArena.Data;    // GameDataRegistry

namespace DiceArena.Engine.Loadout
{
	public static class LoadoutScreenExtensions
	{
		/// <summary>
		/// Loads registry data and calls SetData(...) on each PlayerLoadoutPanel
		/// under the configured MemberCardsContainerPath (or the whole screen if not set).
		/// </summary>
		public static void PopulateMemberPanels(this DiceArena.Godot.LoadoutScreen s)
		{
			// 1) Load data
			GameDataRegistry.LoadAll();
			var classes = GameDataRegistry.GetAllClasses().ToList();
			var tier1   = GameDataRegistry.GetSpellsByTier(1).ToList();
			var tier2   = GameDataRegistry.GetSpellsByTier(2).ToList();

			// 2) Resolve the host
			Node host = s;
			if (!s.MemberCardsContainerPath.IsEmpty)
			{
				var n = s.GetNodeOrNull(s.MemberCardsContainerPath);
				if (n != null) host = n;
				else GD.PushWarning("[Loadout] MemberCardsContainerPath didn't resolve; scanning whole LoadoutScreen.");
			}

			// 3) Find panels
			var panels = FindPanels(host).ToList();

			// 4) Populate
			foreach (var p in panels)
				p.SetData(classes, tier1, tier2);

			GD.Print($"[Loadout] Populated {panels.Count} panels.");
		}

		private static IEnumerable<PlayerLoadoutPanel> FindPanels(Node root)
		{
			foreach (var child in root.GetChildren())
			{
				if (child is PlayerLoadoutPanel p)
					yield return p;

				foreach (var nested in FindPanels(child))
					yield return nested;
			}
		}
	}
}
