// res://Scripts/Godot/LoadoutScreen.cs
using Godot;
using System;
using System.Collections.Generic;
using DiceArena.Engine;

namespace DiceArena.GodotUI
{
	public partial class LoadoutScreen : Control
	{
		public event Action<int, List<Spell>>? OnConfirm; // (partySize, selected loadout for P1 just as example)

		private SpinBox _partySize;
		private VBoxContainer _root;
		private GridContainer _tierRows;
		private Button _confirm;

		private List<Spell> _t1Candidates = new();
		private List<Spell> _t2Candidates = new();
		private HashSet<int> _pickedT1 = new();
		private HashSet<int> _pickedT2 = new();

		public override void _Ready()
		{
			Name = "LoadoutScreen";
			AnchorLeft = 0; AnchorTop = 0; AnchorRight = 1; AnchorBottom = 1;
			OffsetLeft = 0; OffsetTop = 0; OffsetRight = 0; OffsetBottom = 0;

			_root = new VBoxContainer
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical   = Control.SizeFlags.ExpandFill
			};
			_root.AddThemeConstantOverride("separation", 10);
			AddChild(_root);

			// Party size row
			var top = new HBoxContainer
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};
			top.AddThemeConstantOverride("separation", 8);
			_root.AddChild(top);

			top.AddChild(UiUtils.MakeLabel("Party Size:", 14, false));
			_partySize = new SpinBox
			{
				MinValue = 1, MaxValue = 8, Step = 1, Value = 4,
				SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
			};
			top.AddChild(_partySize);

			// Tier rows panel
			_tierRows = new GridContainer
			{
				Columns = 1,
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical   = Control.SizeFlags.ExpandFill
			};
			_tierRows.AddThemeConstantOverride("h_separation", 8);
			_tierRows.AddThemeConstantOverride("v_separation", 8);
			_root.AddChild(_tierRows);

			// Confirm
			_confirm = new Button
			{
				Text = "Finalize & Start",
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};
			_confirm.Pressed += ConfirmPressed;
			_root.AddChild(_confirm);

			BuildCandidates();
			RebuildRows();
		}

		private void BuildCandidates()
		{
			var rng = new Random();

			// pick 3 random from Tier1Pool, 2 random from Tier2Pool
			_t1Candidates.Clear();
			_t2Candidates.Clear();
			_pickedT1.Clear();
			_pickedT2.Clear();

			// naive random picks without duplication
			var t1 = new List<Spell>(Spells.Tier1Pool);
			var t2 = new List<Spell>(Spells.Tier2Pool);

			for (int i = 0; i < 3 && t1.Count > 0; i++)
			{
				int idx = rng.Next(t1.Count);
				_t1Candidates.Add(t1[idx]);
				t1.RemoveAt(idx);
			}
			for (int i = 0; i < 2 && t2.Count > 0; i++)
			{
				int idx = rng.Next(t2.Count);
				_t2Candidates.Add(t2[idx]);
				t2.RemoveAt(idx);
			}
		}

		private void RebuildRows()
		{
			_tierRows.QueueFreeChildren();

			// T1 row
			var t1Row = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
			t1Row.AddChild(UiUtils.MakeLabel("Tier 1 (pick 2 max):", 14, true));
			foreach (var (spell, i) in Enumerate(_t1Candidates))
			{
				var btn = new Button { Text = spell.Name, ToggleMode = true };
				int idx = i;
				btn.Toggled += (pressed) =>
				{
					if (pressed)
					{
						if (_pickedT1.Count >= 2) { btn.ButtonPressed = false; return; }
						_pickedT1.Add(idx);
					}
					else
					{
						_pickedT1.Remove(idx);
					}
				};
				t1Row.AddChild(btn);
			}
			_tierRows.AddChild(t1Row);

			// T2 row
			var t2Row = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
			t2Row.AddChild(UiUtils.MakeLabel("Tier 2 (pick 1 max):", 14, true));
			foreach (var (spell, i) in Enumerate(_t2Candidates))
			{
				var btn = new Button { Text = spell.Name, ToggleMode = true };
				int idx = i;
				btn.Toggled += (pressed) =>
				{
					if (pressed)
					{
						if (_pickedT2.Count >= 1) { btn.ButtonPressed = false; return; }
						_pickedT2.Add(idx);
					}
					else
					{
						_pickedT2.Remove(idx);
					}
				};
				t2Row.AddChild(btn);
			}
			_tierRows.AddChild(t2Row);
		}

		private void ConfirmPressed()
		{
			// build 4-slot loadout with blanks for unpicked
			var chosen = new System.Collections.Generic.List<Spell>(4);
			foreach (var i in _pickedT1) chosen.Add(_t1Candidates[i]);
			foreach (var i in _pickedT2) chosen.Add(_t2Candidates[i]);

			while (chosen.Count < 4) chosen.Add(Spells.Blank());
			if (chosen.Count > 4) chosen.RemoveRange(4, chosen.Count - 4);

			OnConfirm?.Invoke((int)_partySize.Value, chosen);
		}

		private static System.Collections.Generic.IEnumerable<(Spell spell, int index)> Enumerate(System.Collections.Generic.IList<Spell> list)
		{
			for (int i = 0; i < list.Count; i++)
				yield return (list[i], i);
		}
	}
}
