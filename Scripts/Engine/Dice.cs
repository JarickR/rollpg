using System;

namespace DiceArena.Engine
{
	public static class Dice
	{
		private static readonly Random _rng = new Random();

		public static int RollD(int sides) => _rng.Next(1, sides + 1);
		public static int RollD6() => RollD(6);
		public static int RollD20() => RollD(20);
	}
}
