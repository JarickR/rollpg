// Scripts/Engine/Loadout/LoadoutMemberCard.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using DiceArena.Engine.Content;

namespace DiceArena.Engine.Loadout
{
	public partial class LoadoutMemberCard : VBoxContainer
	{
		// ---------- UI ----------
		private Label _titleLabel = null!;
		private GridContainer _classGrid = null!;
		private VBoxContainer _tier1Box = null!;
		private VBoxContainer _tier2Box = null!;

		// ---------- State ----------
		private readonly ButtonGroup _classGroup = new();
		private ButtonGroup _tier2Group = new(); // re-created whenever we rebuild tier2 row

		private readonly List<ClassDef> _classes = new();
		private readonly List<SpellDef> _t1Offers = new();
		private readonly List<SpellDef> _t2Offers = new();

		private readonly List<CheckButton> _t1Toggles = new();
		private readonly List<CheckBox> _t2Radios = new();

		private ContentBundle? _bundle;
		private string? _selectedClassId;

		public string TitleText
		{
			get => _titleLabel?.Text ?? string.Empty;
			set { if (_titleLabel != null) _titleLabel.Text = value; }
		}

		public string? GetSelectedClassId() => _selectedClassId;

		public (List<string> Tier1, string? Tier2) GetPicks()
		{
			var t1 = new List<string>();
			for (int i = 0; i < _t1Offers.Count && i < _t1Toggles.Count; i++)
			{
				if (_t1Toggles[i].ButtonPressed)
					t1.Add(_t1Offers[i].Id);
			}

			string? t2 = null;
			for (int i = 0; i < _t2Offers.Count && i < _t2Radios.Count; i++)
			{
				if (_t2Radios[i].ButtonPressed)
				{
					t2 = _t2Offers[i].Id;
					break;
				}
			}

			return (t1, t2);
		}

		/// <summary>Set data and (re)build the card.</summary>
		public void Setup(
			ContentBundle bundle,
			IReadOnlyList<ClassDef> classes,
			IReadOnlyList<SpellDef> tier1Offers,
			IReadOnlyList<SpellDef> tier2Offers)
		{
			_bundle = bundle;

			_classes.Clear();
			_classes.AddRange(classes);

			_t1Offers.Clear();
			_t1Offers.AddRange(tier1Offers);

			_t2Offers.Clear();
			_t2Offers.AddRange(tier2Offers);

			BuildIfNeeded();
			RebuildClassGrid();
			ShowOffers(_t1Offers, _t2Offers);
		}

		public void ShowOffers(IReadOnlyList<SpellDef> t1, IReadOnlyList<SpellDef> t2)
		{
			_t1Offers.Clear();
			_t1Offers.AddRange(t1);

			_t2Offers.Clear();
			_t2Offers.AddRange(t2);

			// clear previous rows
			ClearChildren(_tier1Box);
			ClearChildren(_tier2Box);

			// ---------- Tier 1 ----------
			var t1Panel = new PanelContainer();
			t1Panel.MouseFilter = MouseFilterEnum.Pass;
			var t1Inner = new VBoxContainer();
			t1Panel.AddChild(t1Inner);
			ApplyCardMargins(t1Inner);
			t1Inner.AddThemeConstantOverride("separation", 6);
			t1Inner.AddChild(new Label { Text = "Tier 1 — pick 2", HorizontalAlignment = HorizontalAlignment.Center });

			var t1Row = new HBoxContainer();
			t1Row.AddThemeConstantOverride("separation", 12);
			t1Inner.AddChild(t1Row);

			_t1Toggles.Clear();
			foreach (var sp in _t1Offers)
			{
				var cb = new CheckButton
				{
					Text = sp.Name,
					TooltipText = sp.Text
				};
				cb.CustomMinimumSize = new Vector2(150, 36);
				cb.Toggled += OnTier1Toggled;
				t1Row.AddChild(cb);
				_t1Toggles.Add(cb);
			}
			_tier1Box.AddChild(t1Panel);

			// ---------- Tier 2 ----------
			var t2Panel = new PanelContainer();
			t2Panel.MouseFilter = MouseFilterEnum.Pass;
			var t2Inner = new VBoxContainer();
			t2Panel.AddChild(t2Inner);
			ApplyCardMargins(t2Inner);
			t2Inner.AddThemeConstantOverride("separation", 6);
			t2Inner.AddChild(new Label { Text = "Tier 2 — pick 1", HorizontalAlignment = HorizontalAlignment.Center });

			var t2Row = new HBoxContainer();
			t2Row.AddThemeConstantOverride("separation", 12);
			t2Inner.AddChild(t2Row);

			_t2Radios.Clear();
			_tier2Group = new ButtonGroup(); // reset group

			foreach (var sp in _t2Offers)
			{
				var rb = new CheckBox
				{
					Text = sp.Name,
					TooltipText = sp.Text,
					ButtonGroup = _tier2Group,
					ToggleMode = true
				};
				rb.CustomMinimumSize = new Vector2(150, 36);

				// Explicit exclusivity to ensure only one Tier-2 stays selected
				rb.Toggled += pressed => OnTier2Toggled(rb, pressed);

				t2Row.AddChild(rb);
				_t2Radios.Add(rb);
			}
			_tier2Box.AddChild(t2Panel);
		}

		// =========================================================
		// Building blocks & helpers
		// =========================================================
		private void BuildIfNeeded()
		{
			if (_titleLabel != null) return;

			AddThemeConstantOverride("separation", 12);

			_titleLabel = new Label { Text = "Member", ThemeTypeVariation = "" };
			_titleLabel.AddThemeFontSizeOverride("font_size", 18);
			AddChild(_titleLabel);

			// Classes
			_classGrid = new GridContainer { Columns = 8 };
			_classGrid.AddThemeConstantOverride("h_separation", 8);
			_classGrid.AddThemeConstantOverride("v_separation", 8);
			AddChild(_classGrid);

			// Tier sections
			_tier1Box = new VBoxContainer();
			_tier1Box.AddThemeConstantOverride("separation", 8);
			AddChild(_tier1Box);

			_tier2Box = new VBoxContainer();
			_tier2Box.AddThemeConstantOverride("separation", 8);
			AddChild(_tier2Box);
		}

		private void RebuildClassGrid()
		{
			ClearChildren(_classGrid);

			foreach (var cls in _classes)
			{
				var btn = new CheckBox
				{
					Text = cls.Name,
					ButtonGroup = _classGroup,
					ToggleMode = true,
					TooltipText = $"{cls.Name}"
				};
				btn.CustomMinimumSize = new Vector2(140, 40);

				string id = cls.Id;
				btn.Toggled += toggled =>
				{
					if (toggled) _selectedClassId = id;
				};

				_classGrid.AddChild(btn);
			}
		}

		private static void ClearChildren(Node n)
		{
			for (int i = n.GetChildCount() - 1; i >= 0; i--)
				n.GetChild(i).QueueFree();
		}

		private static void ApplyCardMargins(Control c)
		{
			c.AddThemeConstantOverride("margin_left", 10);
			c.AddThemeConstantOverride("margin_top", 8);
			c.AddThemeConstantOverride("margin_right", 10);
			c.AddThemeConstantOverride("margin_bottom", 8);
		}

		private void OnTier1Toggled(bool _pressed)
		{
			// Enforce "pick exactly two" interactively
			var on = _t1Toggles.Where(t => t.ButtonPressed).ToList();
			if (on.Count <= 2) return;

			// Turn off the first one to keep max=2
			on[0].ButtonPressed = false;
		}

		private void OnTier2Toggled(CheckBox who, bool pressed)
		{
			if (!pressed) return;

			// Guarantee exclusivity in case ButtonGroup doesn’t
			foreach (var other in _t2Radios)
			{
				if (!ReferenceEquals(other, who) && other.ButtonPressed)
					other.ButtonPressed = false;
			}
		}
	}
}
