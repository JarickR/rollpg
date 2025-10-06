// Scripts/Godot/PlayerLoadoutPanel.cs
using Godot;
using System.Collections.Generic;
using System.Linq;
using DiceArena.Data; // for ClassData / SpellData

namespace DiceArena.Godot
{
	public partial class PlayerLoadoutPanel : Control
	{
		// Set these in the Inspector to the three GridContainer rows
		[Export] public NodePath ClassSelectionContainerPath { get; set; }
		[Export] public NodePath Tier1SpellContainerPath     { get; set; }
		[Export] public NodePath Tier2SpellContainerPath     { get; set; }

		private GridContainer _classRow = null!;
		private GridContainer _tier1Row = null!;
		private GridContainer _tier2Row = null!;

		public override void _Ready()
		{
			// Resolve paths
			_classRow = GetNode<GridContainer>(ClassSelectionContainerPath);
			_tier1Row = GetNode<GridContainer>(Tier1SpellContainerPath);
			_tier2Row = GetNode<GridContainer>(Tier2SpellContainerPath);

			EnsureLayout();

			GD.Print($"[Panel:{Name}] _Ready");
		}

		/// <summary>
		/// Make sure the panel and its rows actually occupy space and show children.
		/// </summary>
		private void EnsureLayout()
		{
			// Give the panel a footprint
			CustomMinimumSize = new Vector2(480, 420);
			SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill;
			SizeFlagsVertical   = SizeFlags.Expand | SizeFlags.Fill;

			// Rows expand/fill too
			static void Fill(Control c)
			{
				c.SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill;
				c.SizeFlagsVertical   = SizeFlags.Expand | SizeFlags.Fill;
			}

			Fill(_classRow);
			Fill(_tier1Row);
			Fill(_tier2Row);

			// Reasonable defaults so tiles don’t wrap awkwardly
			if (_classRow.Columns <= 0) _classRow.Columns = 9;
			if (_tier1Row.Columns <= 0) _tier1Row.Columns = 9;
			if (_tier2Row.Columns <= 0) _tier2Row.Columns = 9;
		}

		/// <summary>
		/// Optional hook: called by LoadoutScreenExtensions.PopulateMemberPanels.
		/// You can use this to set tooltips or enable/disable existing IconTile children.
		/// </summary>
		public void SetData(
			IEnumerable<ClassData> classes,
			IEnumerable<SpellData> tier1,
			IEnumerable<SpellData> tier2)
		{
			// Resolve the three rows (once).
			_classRow ??= GetNode<GridContainer>(ClassSelectionContainerPath);
			_tier1Row  ??= GetNode<GridContainer>(Tier1SpellContainerPath);
			_tier2Row  ??= GetNode<GridContainer>(Tier2SpellContainerPath);

			// If you already placed IconTiles in the scene, keep them.
			var existing =
				(_classRow?.GetChildCount() ?? 0) +
				(_tier1Row?.GetChildCount()  ?? 0) +
				(_tier2Row?.GetChildCount()  ?? 0);

			if (existing > 0)
			{
				GD.Print($"[Panel:{Name}] Using inspector IconTiles (class={_classRow.GetChildCount()}, t1={_tier1Row.GetChildCount()}, t2={_tier2Row.GetChildCount()}).");
				return; // <-- Do not clear or repopulate
			}

			// (Optional fallback) If rows are empty, you could add code here to
			// generate tiles from data. Otherwise just leave this section blank.
		}

	}
}
