using System;
using System.Collections.Generic;
using Godot;
using DiceArena.Godot;

namespace DiceArena.Engine.Loadout
{
	public partial class LoadoutScreen : Node
	{
		[Export] public NodePath HostRootPath { get; set; } = new NodePath("MemberCardsRoot");
		[Export] public NodePath PartySizeSpinPath { get; set; } = new NodePath("PartySizeSpin");

		private Control _root = default!;
		private SpinBox _partySpin = default!;

		public record UiMemberSelection(
			int? SelectedClassId,
			List<int> Tier1ChosenIds,
			int? Tier2ChosenId);

		private readonly List<UiMember> _members = new();

		private class UiMember
		{
			public Control Root = default!;
			public GridContainer ClassGrid = default!;
			public HFlowContainer T1Row = default!;
			public HFlowContainer T2Row = default!;

			public int? SelectedClassId;
			public readonly List<int> T1 = new();
			public int? T2;
		}

		public override void _Ready()
		{
			_root = GetNodeOrNull<Control>(HostRootPath) ?? throw new Exception($"[Loadout] HostRoot '{HostRootPath}' not found");
			_partySpin = GetNodeOrNull<SpinBox>(PartySizeSpinPath) ?? throw new Exception($"[Loadout] PartySpin '{PartySizeSpinPath}' not found");
		}

		// External entry from Game.cs:
		public void Build(IReadOnlyList<string> classNames, IReadOnlyList<string> tier1Names, IReadOnlyList<string> tier2Names)
		{
			_root.QueueFreeChildren();
			_members.Clear();

			int partySize = (int)_partySpin.Value;
			for (int i = 0; i < partySize; i++)
			{
				_members.Add(MakeMemberCard($"Member {i + 1}", classNames, tier1Names, tier2Names));
			}
		}

		public List<UiMemberSelection> GetSelections()
		{
			var result = new List<UiMemberSelection>(_members.Count);
			foreach (var m in _members)
				result.Add(new UiMemberSelection(m.SelectedClassId, new List<int>(m.T1), m.T2));
			return result;
		}

		// ---------- UI builders ----------

		private UiMember MakeMemberCard(string title, IReadOnlyList<string> classNames, IReadOnlyList<string> t1, IReadOnlyList<string> t2)
		{
			var panel = new PanelContainer
			{
				CustomMinimumSize = new Vector2(360, 220)
			};
			panel.AddThemeConstantOverride("margin_left", 8);
			panel.AddThemeConstantOverride("margin_right", 8);
			panel.AddThemeConstantOverride("margin_top", 6);
			panel.AddThemeConstantOverride("margin_bottom", 6);
			_root.AddChild(panel);

			var vb = new VBoxContainer();
			panel.AddChild(vb);

			vb.AddChild(new Label { Text = title });

			// Class grid 5x2
			vb.AddChild(new Label { Text = "Class" });
			var classGrid = new GridContainer { Columns = 5 };
			classGrid.AddThemeConstantOverride("v_separation", 4);
			classGrid.AddThemeConstantOverride("h_separation", 4);
			vb.AddChild(classGrid);

			// 3x T1 (pick 2)
			vb.AddChild(new Label { Text = "Tier 1 Spells (pick 2)" });
			var t1Row = new HFlowContainer();
			t1Row.AddThemeConstantOverride("h_separation", 4);
			vb.AddChild(t1Row);

			// 2x T2 (pick 1)
			vb.AddChild(new Label { Text = "Tier 2 Spells (pick 1)" });
			var t2Row = new HFlowContainer();
			t2Row.AddThemeConstantOverride("h_separation", 4);
			vb.AddChild(t2Row);

			var ui = new UiMember
			{
				Root = panel,
				ClassGrid = classGrid,
				T1Row = t1Row,
				T2Row = t2Row
			};

			// Populate classes
			for (int i = 0; i < Math.Min(10, classNames.Count); i++)
			{
				int id = i; // stable capture
				var name = classNames[i];
				var tile = new IconTile();
				tile.SetTexture(IconLibrary.GetClassTexture(name));
				tile.SetTooltip(name);
				tile.Toggled += pressed =>
				{
					if (!pressed)
					{
						if (ui.SelectedClassId == id) ui.SelectedClassId = null;
						return;
					}
					// turn others off
					foreach (var c in ui.ClassGrid.GetChildren())
						if (c is IconTile other && other != tile) other.ButtonPressed = false;
					ui.SelectedClassId = id;
				};
				classGrid.AddChild(tile);
			}

			// Populate tier1 (3 buttons)
			var t1Ids = PickRandomIndices(t1, 3);
			foreach (var idx in t1Ids)
			{
				int id = idx;
				var name = t1[id];
				var tile = new IconTile();
				tile.SetTexture(IconLibrary.GetSpellTexture(name, 1));
				tile.SetTooltip(name);
				tile.Toggled += pressed =>
				{
					if (pressed)
					{
						if (ui.T1.Count < 2 && !ui.T1.Contains(id))
							ui.T1.Add(id);
						else
							tile.ButtonPressed = false; // deny a 3rd pick
					}
					else
					{
						ui.T1.Remove(id);
					}
				};
				t1Row.AddChild(tile);
			}

			// Populate tier2 (2 buttons)
			var t2Ids = PickRandomIndices(t2, 2);
			foreach (var idx in t2Ids)
			{
				int id = idx;
				var name = t2[id];
				var tile = new IconTile();
				tile.SetTexture(IconLibrary.GetSpellTexture(name, 2));
				tile.SetTooltip(name);
				tile.Toggled += pressed =>
				{
					if (!pressed)
					{
						if (ui.T2 == id) ui.T2 = null;
						return;
					}
					// ensure single selection
					foreach (var c in ui.T2Row.GetChildren())
						if (c is IconTile other && other != tile) other.ButtonPressed = false;
					ui.T2 = id;
				};
				t2Row.AddChild(tile);
			}

			return ui;
		}

		// Utility: return N distinct indices from 0..(count-1)
		private static List<int> PickRandomIndices(IReadOnlyList<string> source, int count)
		{
			var list = new List<int>(source.Count);
			for (int i = 0; i < source.Count; i++) list.Add(i);

			var rng = new Random();
			for (int i = list.Count - 1; i > 0; i--)
			{
				int j = rng.Next(i + 1);
				(list[i], list[j]) = (list[j], list[i]);
			}
			if (count < list.Count) list.RemoveRange(count, list.Count - count);
			return list;
		}
	}

	internal static class NodeUtil
	{
		public static void QueueFreeChildren(this Node n)
		{
			foreach (var c in n.GetChildren()) (c as Node)?.QueueFree();
		}
	}
}
