// res://Scripts/Engine/Dice.cs
using System;

namespace DiceArena.Engine
{
	public static class Dice
	{
		private static readonly Random Rng = new();

		public static int D6()  => Rng.Next(1, 7);
		public static int D20() => Rng.Next(1, 21);

		/// <summary>Build the six roll faces for a hero (class, 4 spells/blank, upgrade).</summary>
		public static Face[] BuildFacesFor(Hero h)
		{
			var faces = new Face[6];
			faces[0] = new Face(FaceType.ClassAbility);

			for (int i = 0; i < 4; i++)
			{
				var spell = (i < h.Loadout.Count) ? h.Loadout[i] : Spells.Blank();
				if (spell.Kind == SpellKind.Blank)
					faces[i + 1] = new Face(FaceType.Blank, i, spell);
				else
					faces[i + 1] = new Face(FaceType.Spell, i, spell);
			}

			faces[5] = new Face(FaceType.Upgrade);
			return faces;
		}

		public static Face RollFaceFor(Hero h)
		{
			var faces = BuildFacesFor(h);
			int idx = Rng.Next(0, faces.Length);
			return faces[idx];
		}
	}
}
