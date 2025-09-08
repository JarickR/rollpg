// res://Scripts/Engine/SpellFactory.cs
#nullable enable
using System;
using DiceArena.Engine;

namespace DiceArena.GameData
{
	/// <summary>
	/// Creates Engine.Spell instances from a string kind + tier (+ optional name override for UI).
	/// </summary>
	public static class SpellFactory
	{
		public static Spell FromKind(string kind, int tier, string? nameOverride = null)
		{
			kind = kind.Trim().ToLowerInvariant();
			Spell s = kind switch
			{
				"attack"        => Spells.Attack(tier),
				"sweep"         => Spells.Sweep(tier),
				"heal"          => Spells.Heal(tier),
				"armor"         => Spells.Armor(tier),
				"fireball"      => Spells.Fireball(tier),
				"poison"        => Spells.Poison(tier),
				"bomb"          => Spells.Bomb(tier),
				"concentration" => Spells.Concentration(tier),
				_               => Spells.Blank()
			};

			// If the data has a specific display name, apply it
			if (!string.IsNullOrWhiteSpace(nameOverride))
				s.Name = nameOverride!;
			return s;
		}
	}
}
