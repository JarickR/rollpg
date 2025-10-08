// res://Scripts/Godot/PlayerLoadoutPanel.cs
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace DiceArena.Godot
{
	public partial class PlayerLoadoutPanel : Control
	{
		[Export] public NodePath ClassSelectionContainerPath { get; set; } = default!;
		[Export] public NodePath Tier1SpellContainerPath    { get; set; } = default!;
		[Export] public NodePath Tier2SpellContainerPath    { get; set; } = default!;
		[Export] public NodePath LoadoutContainerPath       { get; set; } = default!;

		[Export] public int Tier1OptionCount { get; set; } = 3;
		[Export] public int Tier2OptionCount { get; set; } = 1;

		[Export] public int MaxTier1 { get; set; } = 2;
		[Export] public int MaxTier2 { get; set; } = 1;

		private GridContainer? _classRow;
		private GridContainer? _tier1Row;
		private GridContainer? _tier2Row;
		private HBoxContainer? _loadoutRow;

		private readonly RandomNumberGenerator _rng = new();

		private string? _selectedClassId;
		private readonly HashSet<string> _selectedTier1 = new();
		private readonly HashSet<string> _selectedTier2 = new();

		private ButtonGroup? _classButtonGroup;

		public override void _Ready()
		{
			_rng.Randomize();

			_classRow   = GetNodeOrNull<GridContainer>(ClassSelectionContainerPath);
			_tier1Row   = GetNodeOrNull<GridContainer>(Tier1SpellContainerPath);
			_tier2Row   = GetNodeOrNull<GridContainer>(Tier2SpellContainerPath);
			_loadoutRow = GetNodeOrNull<HBoxContainer>(LoadoutContainerPath);

			WireRow(_classRow, isClassRow: true);
			WireRow(_tier1Row, isClassRow: false);
			WireRow(_tier2Row, isClassRow: false);

			RandomlyLimitRow(_tier1Row, Tier1OptionCount);
			RandomlyLimitRow(_tier2Row, Tier2OptionCount);

			ApplyLimits();      // enforce caps visually/interaction-wise
			RebuildSelectionsFromUI();
			UpdateLoadout();
		}

		public void AutoPickForPanel()
		{
			ClearPressedState(_tier1Row);
			ClearPressedState(_tier2Row);

			RandomlyLimitRow(_tier1Row, Tier1OptionCount);
			RandomlyLimitRow(_tier2Row, Tier2OptionCount);

			ApplyLimits();
			RebuildSelectionsFromUI();
			UpdateLoadout();
		}

		// ----- wiring & sizing -----
		private void WireRow(GridContainer? row, bool isClassRow)
		{
			if (row == null) return;

			if (isClassRow && _classButtonGroup == null)
				_classButtonGroup = new ButtonGroup();

			foreach (var tile in row.GetChildren().OfType<IconTile>())
			{
				tile.ToggleMode = true;
				if (isClassRow && _classButtonGroup != null)
					tile.ButtonGroup = _classButtonGroup;

				if (!tile.HasMeta("plp_toggled_wired"))
				{
					tile.SetMeta("plp_toggled_wired", true);
					tile.Toggled += (bool pressed) => OnTileToggled(row, tile, pressed);
				}

				// unified cell/icon sizing for the selection grids
				const int ICON = 64;
				tile.ExpandIcon = true;
				tile.IconAlignment = HorizontalAlignment.Center;
				tile.VerticalIconAlignment = VerticalAlignment.Center;
				tile.ClipContents = true;
				tile.Flat = true;
				tile.AddThemeConstantOverride("icon_max_width",  ICON);
				tile.AddThemeConstantOverride("icon_max_height", ICON);
				tile.CustomMinimumSize = new Vector2(ICON + 8, ICON + 8);
			}
		}

		// ----- enforcement -----
		private void OnTileToggled(GridContainer row, IconTile tile, bool pressed)
		{
			// Guard against overshoot: if this press would exceed cap, revert immediately.
			if (tile.Pool == IconTile.IconPool.Tier1Spell)
			{
				if (pressed && CountPressed(row) > MaxTier1)
					tile.ButtonPressed = false;
			}
			else if (tile.Pool == IconTile.IconPool.Tier2Spell)
			{
				if (pressed && CountPressed(row) > MaxTier2)
					tile.ButtonPressed = false;
			}
			// ButtonGroup already enforces single-select for Class.

			ApplyLimits();
			RebuildSelectionsFromUI();
			UpdateLoadout();
		}

		private void ApplyLimits()
		{
			UpdateLimitLockState(_tier1Row, MaxTier1);
			UpdateLimitLockState(_tier2Row, MaxTier2);

			GD.Print($"[Limits:{Name}] t1 pressed={CountPressed(_tier1Row)}/{MaxTier1} | t2 pressed={CountPressed(_tier2Row)}/{MaxTier2}");
		}

		private static void UpdateLimitLockState(GridContainer? row, int cap)
		{
			if (row == null) return;
			if (cap < 0) cap = 0;

			int pressed = CountPressed(row);
			bool atCap = pressed >= cap;

			foreach (var t in row.GetChildren().OfType<IconTile>())
			{
				if (!t.Visible)
				{
					t.Disabled = true;
					continue;
				}

				// Allow deselecting always; only block NEW presses at cap.
				t.Disabled = !t.ButtonPressed && atCap;

				// Optional visual dim when disabled:
				t.Modulate = t.Disabled ? new Color(1, 1, 1, 0.6f) : Colors.White;
			}
		}

		private static int CountPressed(GridContainer? row)
		{
			if (row == null) return 0;
			return row.GetChildren().OfType<IconTile>()
				.Count(t => t.Visible && t.ButtonPressed);
		}

		private static void RandomlyLimitRow(GridContainer? row, int showCount)
		{
			if (row == null) return;
			var tiles = row.GetChildren().OfType<IconTile>().ToList();

			foreach (var t in tiles)
			{
				t.Visible = false;
				t.ButtonPressed = false;
				t.Disabled = false;
				t.Modulate = Colors.White;
			}
			if (tiles.Count == 0 || showCount <= 0) return;

			var rng = new RandomNumberGenerator(); rng.Randomize();
			foreach (var t in tiles.OrderBy(_ => rng.Randi()).Take(Math.Min(showCount, tiles.Count)))
				t.Visible = true;
		}

		private static void ClearPressedState(GridContainer? row)
		{
			if (row == null) return;
			foreach (var t in row.GetChildren().OfType<IconTile>())
				t.ButtonPressed = false;
		}

		// ----- selection & loadout -----
		private static string ExtractId(IconTile t)
		{
			if (!string.IsNullOrWhiteSpace(t.Id))
				return t.Id.Trim();

			var name = t.Name.ToString();
			name = (name ?? string.Empty).Trim().ToLowerInvariant().Replace(" ", "");
			return name;
		}

		private void RebuildSelectionsFromUI()
		{
			_selectedClassId = null;
			_selectedTier1.Clear();
			_selectedTier2.Clear();

			if (_classRow != null)
			{
				var cls = _classRow.GetChildren().OfType<IconTile>()
					.FirstOrDefault(t => t.Visible && t.ButtonPressed);
				if (cls != null)
				{
					var id = ExtractId(cls);
					if (!string.IsNullOrWhiteSpace(id))
						_selectedClassId = id;
				}
			}

			if (_tier1Row != null)
			{
				foreach (var t in _tier1Row.GetChildren().OfType<IconTile>())
				{
					if (t.Visible && t.ButtonPressed)
					{
						var id = ExtractId(t);
						if (!string.IsNullOrWhiteSpace(id))
							_selectedTier1.Add(id);
					}
				}
			}

			if (_tier2Row != null)
			{
				foreach (var t in _tier2Row.GetChildren().OfType<IconTile>())
				{
					if (t.Visible && t.ButtonPressed)
					{
						var id = ExtractId(t);
						if (!string.IsNullOrWhiteSpace(id))
							_selectedTier2.Add(id);
					}
				}
			}

			GD.Print($"[Rebuild:{Name}] class={_selectedClassId ?? "-"} | t1=({string.Join(",", _selectedTier1)}) | t2=({string.Join(",", _selectedTier2)})");
		}

		private IconTile MakeMini(IconTile.IconPool pool, string id)
		{
			var mini = new IconTile();
			mini.Apply(pool, id, id);

			const int MINI = 48;
			mini.ExpandIcon = true;
			mini.IconAlignment = HorizontalAlignment.Center;
			mini.VerticalIconAlignment = VerticalAlignment.Center;
			mini.ClipContents = true;
			mini.Flat = true;
			mini.FocusMode = FocusModeEnum.None;
			mini.Disabled = true;

			mini.AddThemeConstantOverride("icon_max_width",  MINI);
			mini.AddThemeConstantOverride("icon_max_height", MINI);
			mini.CustomMinimumSize = new Vector2(MINI + 6, MINI + 6);
			return mini;
		}

		private void UpdateLoadout()
		{
			if (_loadoutRow == null) return;

			foreach (var child in _loadoutRow.GetChildren())
				child.QueueFree();

			var items = new List<(IconTile.IconPool pool, string id)>(6);

			if (!string.IsNullOrWhiteSpace(_selectedClassId))
				items.Add((IconTile.IconPool.Class, _selectedClassId!));

			foreach (var id in _selectedTier1)
				items.Add((IconTile.IconPool.Tier1Spell, id));
			foreach (var id in _selectedTier2)
				items.Add((IconTile.IconPool.Tier2Spell, id));

			foreach (var (pool, id) in items)
				_loadoutRow.AddChild(MakeMini(pool, id));

			_loadoutRow.Visible = items.Count > 0;
			_loadoutRow.AddThemeConstantOverride("separation", 6);

			GD.Print($"[Loadout:{Name}] slots={items.Count}");
		}
	}
}
