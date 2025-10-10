using System;
using System.Collections.Generic;

namespace RollPG.Engine
{
	/// <summary>
	/// Deterministic-friendly dice roller (inject your own Random for seeds/tests).
	/// Supports: basic rolls, advantage/disadvantage, exploding dice, and dice pools.
	/// </summary>
	public static class Dice
	{
		private static Random _rng = new Random();

		/// <summary>Optional: set a custom Random for deterministic tests.</summary>
		public static void SetRandom(Random rng) => _rng = rng ?? throw new ArgumentNullException(nameof(rng));

		/// <summary>Rolls a single die with Sides sides (1..Sides).</summary>
		public static int D(int sides)
		{
			if (sides < 2) throw new ArgumentOutOfRangeException(nameof(sides), "Die must have at least 2 sides.");
			return _rng.Next(1, sides + 1);
		}

		/// <summary>Roll N dice of Sides each; returns total and individual results.</summary>
		public static (int total, List<int> rolls) Pool(int count, int sides)
		{
			if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
			var rolls = new List<int>(count);
			int total = 0;
			for (int i = 0; i < count; i++)
			{
				int r = D(sides);
				rolls.Add(r);
				total += r;
			}
			return (total, rolls);
		}

		/// <summary>Roll with advantage: roll two dN and keep highest.</summary>
		public static int Advantage(int sides) => Math.Max(D(sides), D(sides));

		/// <summary>Roll with disadvantage: roll two dN and keep lowest.</summary>
		public static int Disadvantage(int sides) => Math.Min(D(sides), D(sides));

		/// <summary>
		/// Exploding dice: each time the max side appears, roll again and add.
		/// Returns total and a list of chained rolls (e.g., [6, 6, 2] for exploding d6).
		/// </summary>
		public static (int total, List<int> chain) Explode(int sides)
		{
			if (sides < 2) throw new ArgumentOutOfRangeException(nameof(sides));
			var chain = new List<int>();
			int total = 0;
			int roll;
			do
			{
				roll = D(sides);
				chain.Add(roll);
				total += roll;
			} while (roll == sides);
			return (total, chain);
		}

		/// <summary>
		/// Roll many dice with optional explode and drop-lowest/highest rules.
		/// </summary>
		public static (int total, List<int> rolls, List<int> dropped) AdvancedPool(
			int count,
			int sides,
			bool explodeMax = false,
			int dropLowest = 0,
			int dropHighest = 0)
		{
			if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
			var rolls = new List<int>();
			for (int i = 0; i < count; i++)
			{
				if (explodeMax)
				{
					var (t, _) = Explode(sides);
					rolls.Add(t); // store collapsed chain sum per die
				}
				else
				{
					rolls.Add(D(sides));
				}
			}

			rolls.Sort();
			var dropped = new List<int>();
			// Drop lowest
			for (int i = 0; i < Math.Min(dropLowest, rolls.Count); i++)
				dropped.Add(rolls[i]);
			// Drop highest
			for (int i = 0; i < Math.Min(dropHighest, Math.Max(0, rolls.Count - dropLowest)); i++)
				dropped.Add(rolls[rolls.Count - 1 - i]);

			int sum = 0;
			for (int i = 0; i < rolls.Count; i++)
			{
				bool isDroppedLow = i < dropLowest;
				bool isDroppedHigh = i >= rolls.Count - dropHighest;
				if (!isDroppedLow && !isDroppedHigh)
					sum += rolls[i];
			}

			return (sum, rolls, dropped);
		}
	}
}
