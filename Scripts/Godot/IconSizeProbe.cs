using Godot;
using System;

namespace DiceArena.Godot
{
	/// <summary>
	/// Simple debug helper that prints out the resolved sizes of class/spell textures
	/// using IconLibrary. Safe to leave in project; does nothing unless in a scene.
	/// </summary>
	[GlobalClass]
	public partial class IconSizeProbe : Node
	{
		// Spell to probe
		[Export] public string SpellName { get; set; } = "attack";
		[Export(PropertyHint.Range, "1,3,1")] public int SpellTier { get; set; } = 1;

		// Class to probe
		[Export] public string ClassId { get; set; } = "warrior";

		// Icon size to request from IconLibrary
		[Export(PropertyHint.Range, "16,256,1")] public int Size { get; set; } = 24;

		public override void _Ready()
		{
			// Probe a spell icon
			Texture2D? spell = IconLibrary.GetSpellTexture(SpellName, SpellTier, Size);
			if (spell != null)
			{
				GD.Print($"[IconSizeProbe] Spell '{SpellName}' t{SpellTier} @ {Size}px -> {spell.GetWidth()}x{spell.GetHeight()}");
			}
			else
			{
				GD.PrintErr($"[IconSizeProbe] Spell '{SpellName}' t{SpellTier} @ {Size}px not found.");
			}

			// Probe a class icon
			Texture2D? cls = IconLibrary.GetClassTexture(ClassId, Size);
			if (cls != null)
			{
				GD.Print($"[IconSizeProbe] Class '{ClassId}' @ {Size}px -> {cls.GetWidth()}x{cls.GetHeight()}");
			}
			else
			{
				GD.PrintErr($"[IconSizeProbe] Class '{ClassId}' @ {Size}px not found.");
			}
		}
	}
}
