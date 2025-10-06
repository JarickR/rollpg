using Godot;
using System;

namespace DiceArena.Godot
{
	public partial class IconTile : Button
	{
		public enum IconPool { None, Class, Tier1Spell, Tier2Spell }

		[Export] public IconPool Pool { get; set; } = IconPool.None;
		[Export] public string Id { get; set; } = ""; // matches file name (no extension)
		[Export] public bool Verbose { get; set; } = false;

		public override void _Ready()
		{
			if (Verbose)
				GD.Print($"[IconTile] Ready -> Pool: {Pool}, Id: {Id}, Icon: {(Icon != null)}");

			// If the icon is not already assigned in the editor, try to auto-load
			if (Icon == null)
			{
				var path = Pool switch
				{
					IconPool.Class => $"res://Content/Icons/Classes/{Id}.png",
					IconPool.Tier1Spell => $"res://Content/Icons/Spells/{Id}.png",
					IconPool.Tier2Spell => $"res://Content/Icons/Spells/{Id}.png",
					_ => ""
				};

				if (!string.IsNullOrEmpty(path) && ResourceLoader.Exists(path))
				{
					Icon = GD.Load<Texture2D>(path);
					if (Verbose)
						GD.Print($"[IconTile] Auto-loaded icon for {Id} -> {path}");
				}
				else if (Verbose)
				{
					GD.PrintErr($"[IconTile] Could not find icon for {Id} -> {path}");
				}
			}
		}
	}
}
