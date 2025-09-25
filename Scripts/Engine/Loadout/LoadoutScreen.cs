// Scripts/Engine/Loadout/LoadoutScreen.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using DiceArena.Data;
using Compat = DiceArena.Data.ContentDatabaseCompat;

// Aliases to avoid ambiguity with DiceArena.Data.*
using ClassDef = DiceArena.Engine.Content.ClassDef;
using SpellDef = DiceArena.Engine.Content.SpellDef;

namespace DiceArena.Engine.Loadout
{
	public partial class LoadoutScreen : Control
	{
		[Export] public NodePath MemberCardsRootPath { get; set; } = new NodePath();
		[Export] public NodePath PartySizeSpinPath   { get; set; } = new NodePath();

		// Live nodes
		private Control? _cardsRoot;
		private SpinBox? _partySpin;

		// Data
		private ContentBundle? _bundle;

		// Current selection (member 0 for now)
		private string? _classId;
		private readonly List<string> _tier1Picked = new();
		private string? _tier2Id;

		// --------- Signals (kept here to avoid partial duplication) ---------
		[Signal] public delegate void SelectionChangedEventHandler(int memberIndex, string? classId, string[] tier1Ids, string? tier2Id);
		[Signal] public delegate void FinalizedEventHandler(int partySize, string[] playersJson);

		public override void _Ready()
		{
			_cardsRoot = GetNodeOrNull<Control>(MemberCardsRootPath);
			_partySpin = GetNodeOrNull<SpinBox>(PartySizeSpinPath);

			// Load content (shim we add in ContentDatabaseCompat)
			_bundle = Compat.LoadOrCreate();

			BuildForPartySize(GetPartySize());

			if (_partySpin != null)
			{
				_partySpin.MinValue = 1;
				_partySpin.MaxValue = 4;
				_partySpin.ValueChanged += OnPartySizeChanged;
			}
		}

		private int GetPartySize() => (int)(_partySpin?.Value ?? 1);

		private void OnPartySizeChanged(double value) => BuildForPartySize((int)value);

		private static void ClearChildren(Node parent)
		{
			foreach (var c in parent.GetChildren())
				if (c is Node n) n.QueueFree();
		}

		private void BuildForPartySize(int partySize)
		{
			if (_cardsRoot == null) return;
			ClearChildren(_cardsRoot);

			// Member 0 card for now
			var card = BuildMemberCard(0);
			_cardsRoot.AddChild(card);

			// Finalize
			var finalize = new Button { Text = "Finalize" };
			finalize.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			finalize.Pressed += OnFinalizePressed;
			_cardsRoot.AddChild(finalize);
		}

		private Control BuildMemberCard(int memberIndex)
		{
			var root = new VBoxContainer
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill
			};

			// --- Class ---
			root.AddChild(new Label { Text = "Class (pick 1)" });

			var classOb = new OptionButton();
			if (_bundle != null)
			{
				var classes = ContentDatabaseCompat.GetClasses(_bundle);
				for (int i = 0; i < classes.Count; i++)
					classOb.AddItem(classes[i].Name, i);

				if (classes.Count > 0)
				{
					classOb.Select(0);
					_classId = classes[0].Id;
				}
			}
			classOb.ItemSelected += idx =>
			{
				if (_bundle == null) return;
				var classes = ContentDatabaseCompat.GetClasses(_bundle);
				if (idx >= 0 && idx < (uint)classes.Count)
					_classId = classes[(int)idx].Id;

				// Coalesce to avoid nullable warnings when emitting Variant-backed signals
				EmitSignal(SignalName.SelectionChanged, memberIndex, _classId ?? string.Empty, _tier1Picked.ToArray(), _tier2Id ?? string.Empty);
			};
			root.AddChild(classOb);

			// --- Tier 1 ---
			root.AddChild(new Label { Text = "Tier 1 (pick 2 of 3)" });
			var t1Row = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
			if (_bundle != null)
			{
				_tier1Picked.Clear();
				var t1 = ContentDatabaseCompat.GetTier1Spells(_bundle).Take(3).ToList();
				foreach (var s in t1)
				{
					var cb = new CheckButton { Text = s.Name };
					cb.Toggled += toggled =>
					{
						if (toggled)
						{
							if (_tier1Picked.Count < 2) _tier1Picked.Add(s.Id);
							else cb.ButtonPressed = false;
						}
						else
						{
							_tier1Picked.Remove(s.Id);
						}

						EmitSignal(SignalName.SelectionChanged, memberIndex, _classId ?? string.Empty, _tier1Picked.ToArray(), _tier2Id ?? string.Empty);
					};
					t1Row.AddChild(cb);
				}
			}
			root.AddChild(t1Row);

			// --- Tier 2 ---
			root.AddChild(new Label { Text = "Tier 2 (pick 1 of 2)" });
			var t2Ob = new OptionButton();
			if (_bundle != null)
			{
				var t2 = ContentDatabaseCompat.GetTier2Spells(_bundle).Take(2).ToList();
				for (int i = 0; i < t2.Count; i++)
					t2Ob.AddItem(t2[i].Name, i);

				if (t2.Count > 0)
				{
					t2Ob.Select(0);
					_tier2Id = t2[0].Id;
				}
			}
			t2Ob.ItemSelected += idx =>
			{
				if (_bundle == null) return;
				var t2 = ContentDatabaseCompat.GetTier2Spells(_bundle).Take(2).ToList();
				if (idx >= 0 && idx < (uint)t2.Count)
					_tier2Id = t2[(int)idx].Id;

				EmitSignal(SignalName.SelectionChanged, memberIndex, _classId ?? string.Empty, _tier1Picked.ToArray(), _tier2Id ?? string.Empty);
			};
			root.AddChild(t2Ob);

			return root;
		}

		private void OnFinalizePressed()
		{
			var partySize = GetPartySize();

			// Single member payload (extend to N later)
			var member = new
			{
				ClassId = _classId,
				Tier1   = _tier1Picked.ToArray(),
				Tier2   = _tier2Id
			};

			var json = System.Text.Json.JsonSerializer.Serialize(member);
			var players = new[] { json };

			EmitSignal(SignalName.Finalized, partySize, players);
			GD.Print($"[Loadout] Finalized party={partySize}, players={players.Length}");
		}
	}
}
