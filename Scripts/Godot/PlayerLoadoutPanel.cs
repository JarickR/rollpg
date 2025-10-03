// Scripts/Godot/PlayerLoadoutPanel.cs
using Godot;
using System.Collections.Generic;
using DiceArena.Data;

namespace DiceArena.Godot
{
	public partial class PlayerLoadoutPanel : Control
	{
		// Set these in the Inspector:
		[Export] public NodePath ClassSelectionContainerPath { get; set; }
		[Export] public NodePath Tier1SpellContainerPath   { get; set; }
		[Export] public NodePath Tier2SpellContainerPath   { get; set; }

		private GridContainer _classRow = null!;
		private GridContainer _tier1Row = null!;
		private GridContainer _tier2Row = null!;

		public override void _Ready()
		{
			// Make sure this panel actually claims some space
			SizeFlagsHorizontal = SizeFlags.Fill | SizeFlags.Expand;
			SizeFlagsVertical   = SizeFlags.Fill | SizeFlags.Expand;
			if (CustomMinimumSize.Y < 420) CustomMinimumSize = new Vector2(0, 420);

			var margin = GetNodeOrNull<MarginContainer>("MarginContainer");
			var vbox   = margin?.GetNodeOrNull<VBoxContainer>("VBoxContainer");
			if (margin != null)
			{
				margin.SizeFlagsHorizontal = SizeFlags.Fill | SizeFlags.Expand;
				margin.SizeFlagsVertical   = SizeFlags.Fill | SizeFlags.Expand;
			}
			if (vbox != null)
			{
				vbox.SizeFlagsHorizontal = SizeFlags.Fill | SizeFlags.Expand;
				vbox.SizeFlagsVertical   = SizeFlags.Fill | SizeFlags.Expand;
			}

			_classRow = GetNode<GridContainer>(ClassSelectionContainerPath);
			_tier1Row = GetNode<GridContainer>(Tier1SpellContainerPath);
			_tier2Row = GetNode<GridContainer>(Tier2SpellContainerPath);

			PrimeRow(_classRow, 8, 120);
			PrimeRow(_tier1Row, 8, 96);
			PrimeRow(_tier2Row, 8, 96);
		}

		private static void PrimeRow(GridContainer row, int cols = 8, int minY = 96)
		{
			row.Columns = cols;
			row.CustomMinimumSize = new Vector2(0, minY);
			row.SizeFlagsHorizontal = SizeFlags.Fill | SizeFlags.Expand;
			row.SizeFlagsVertical   = SizeFlags.ShrinkCenter;
		}

		public void Clear()
		{
			_classRow.QueueFreeChildren();
			_tier1Row.QueueFreeChildren();
			_tier2Row.QueueFreeChildren();
		}

		/// <summary>
		/// classes: all classes; tier1: tier-1 spells; tier2: tier-2 spells
		/// </summary>
		public void SetData(
			IEnumerable<ClassData> classes,
			IEnumerable<SpellData> tier1,
			IEnumerable<SpellData> tier2)
		{
			if (_classRow == null || _tier1Row == null || _tier2Row == null)
			{
				GD.PushError("[Panel] Rows not resolved; are the NodePaths set in the Inspector?");
				return;
			}

			Clear();
			PrimeRow(_classRow, 8, 120);
			PrimeRow(_tier1Row, 8, 96);
			PrimeRow(_tier2Row, 8, 96);

			// Classes
			foreach (var c in classes)
			{
				var texPath = IconPathLoader.LoadClassPath(c.Id);   // <-- from the separate file
				var tex = GD.Load<Texture2D>(texPath);
				if (tex == null)
				{
					GD.PushWarning($"[Panel] Class icon not found: {texPath}");
					_classRow.AddChild(MakeFallbackButton("?"));
				}
				else
				{
					_classRow.AddChild(MakeIconButton(tex, c.Name ?? c.Id));
				}
			}

			// Tier 1
			foreach (var s in tier1)
			{
				var texPath = IconPathLoader.LoadSpellPath(s.Id);
				var tex = GD.Load<Texture2D>(texPath);
				if (tex == null)
				{
					GD.PushWarning($"[Panel] Tier1 icon not found: {texPath}");
					_tier1Row.AddChild(MakeFallbackButton("?"));
				}
				else
				{
					_tier1Row.AddChild(MakeIconButton(tex, s.Name ?? s.Id));
				}
			}

			// Tier 2
			foreach (var s in tier2)
			{
				var texPath = IconPathLoader.LoadSpellPath(s.Id);
				var tex = GD.Load<Texture2D>(texPath);
				if (tex == null)
				{
					GD.PushWarning($"[Panel] Tier2 icon not found: {texPath}");
					_tier2Row.AddChild(MakeFallbackButton("?"));
				}
				else
				{
					_tier2Row.AddChild(MakeIconButton(tex, s.Name ?? s.Id));
				}
			}

			GD.Print($"[Panel:{Name}] populated classes={_classRow.GetChildCount()}, t1={_tier1Row.GetChildCount()}, t2={_tier2Row.GetChildCount()}");
		}

		private static Button MakeIconButton(Texture2D tex, string tooltip)
		{
			var b = new Button
			{
				Text = "",
				Icon = tex,
				ToggleMode = true,
				TooltipText = tooltip,
				Flat = true,
				ExpandIcon = true,
				IconAlignment = HorizontalAlignment.Center,
				VerticalIconAlignment = VerticalAlignment.Center,
				CustomMinimumSize = new Vector2(64, 64)
			};
			b.SizeFlagsHorizontal = SizeFlags.Fill | SizeFlags.Expand;
			b.SizeFlagsVertical   = SizeFlags.Fill;
			return b;
		}

		private static Button MakeFallbackButton(string text)
		{
			var b = new Button
			{
				Text = text,
				ToggleMode = false,
				TooltipText = "Missing icon",
				Flat = false,
				CustomMinimumSize = new Vector2(64, 64)
			};
			b.SizeFlagsHorizontal = SizeFlags.Fill | SizeFlags.Expand;
			b.SizeFlagsVertical   = SizeFlags.Fill;
			return b;
		}
	}

	internal static class NodeExtensions
	{
		public static void QueueFreeChildren(this Node n)
		{
			foreach (var child in n.GetChildren())
				child.QueueFree();
		}
	}
}
