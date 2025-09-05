// res://Scripts/Engine/CombatSystem.cs
using System;
using System.Linq;

namespace DiceArena.Engine
{
	public static class CombatSystem
	{
		private static readonly Random Rng = new();

		// Simple enemy action
		public static void TakeTurn(Hero hero, Enemy enemy)
		{
			if (hero == null || enemy == null || enemy.Hp <= 0) return;
			ApplyDamage(hero, 2, bypassArmor: false);
		}

		// Overload used by some UIs (indices)
		public static void TakeTurn(GameState state, int enemyIndex, int playerIndex)
		{
			if (state == null) return;
			if (enemyIndex < 0 || enemyIndex >= state.Enemies.Count) return;
			if (playerIndex < 0 || playerIndex >= state.Players.Count) return;
			TakeTurn(state.Players[playerIndex], state.Enemies[enemyIndex]);
		}

		public static void Cast(Hero caster, Enemy target, Spell spell)
		{
			if (caster == null || target == null || spell == null) return;

			int bonus = 0;
			if (caster.ConcentrationStacks > 0)
			{
				bonus = 2 * caster.ConcentrationStacks;
				caster.ConcentrationStacks = 0;
			}

			switch (spell.Kind)
			{
				case SpellKind.Attack:
				case SpellKind.Sweep:
					ApplyDamage(target, DamageByTier(spell.Tier) + bonus, bypassArmor: false);
					break;

				case SpellKind.Heal:
					caster.Heal(HealByTier(spell.Tier));
					break;

				case SpellKind.Armor:
					caster.AddArmor(ArmorByTier(spell.Tier));
					break;

				case SpellKind.Fireball:
					ApplyDamage(target, DamageByTier(spell.Tier) + bonus, bypassArmor: true);
					break;

				case SpellKind.Poison:
					target.PoisonStacks += 1;
					break;

				case SpellKind.Bomb:
					target.BombStacks += 1;
					break;

				case SpellKind.Concentration:
					caster.ConcentrationStacks += 1;
					break;

				default:
					break;
			}
		}

		// MAIN shim older code calls: with state + player index
		public static void ResolvePlayerRoll(GameState state, int playerIndex, Face face)
		{
			if (state == null || face == null) return;
			if (playerIndex < 0 || playerIndex >= state.Players.Count) return;

			var hero = state.Players[playerIndex];
			Enemy? target = state.Enemies.FirstOrDefault(e => e.Hp > 0);

			ResolvePlayerRoll(hero, face, target, state);
		}

		// Secondary shim: with hero + optional target
		public static void ResolvePlayerRoll(Hero hero, Face face, Enemy? target = null, GameState? state = null)
		{
			if (hero == null || face == null) return;

			switch (face.Type)
			{
				case FaceType.ClassAbility:
					hero.AddArmor(2);
					state?.AddLog($"{hero.Name} used class ability (+2 ARM).");
					break;

				case FaceType.Upgrade:
					if (state != null)
					{
						int slot = (face.Slot >= 0 && face.Slot <= 3) ? face.Slot : 0;
						var current = (hero.Loadout.Count > slot) ? hero.Loadout[slot] : Spells.Blank();
						// Upgrade blanks or T1 -> random T2
						var upgraded = (current.Tier <= 1) ? Spells.RandomFromTier(2, state.Rng) : current;
						hero.SetSlot(slot, upgraded);
						state.AddLog($"{hero.Name} upgraded slot {slot} → {upgraded.Name}.");
					}
					break;

				case FaceType.Spell:
					{
						int slot = (face.Slot >= 0) ? face.Slot : 0;
						var spell = face.Spell ?? ((hero.Loadout.Count > slot) ? hero.Loadout[slot] : Spells.Blank());
						if (spell.Kind == SpellKind.Blank)
						{
							state?.AddLog($"{hero.Name} rolled a blank.");
							return;
						}

						// pick a target if offensive
						if (target == null && (spell.Kind == SpellKind.Attack || spell.Kind == SpellKind.Sweep ||
											   spell.Kind == SpellKind.Fireball || spell.Kind == SpellKind.Poison ||
											   spell.Kind == SpellKind.Bomb))
						{
							if (state != null) target = state.Enemies.FirstOrDefault(e => e.Hp > 0);
						}

						if (target == null && state != null)
						{
							state.AddLog($"{hero.Name} has no target.");
							return;
						}

						Cast(hero, target!, spell);
						state?.AddLog($"{hero.Name} cast {spell.Name}{(target != null ? $" on {target.Name}" : "")}.");
					}
					break;

				case FaceType.Blank:
				default:
					state?.AddLog($"{hero.Name} rolled a blank.");
					break;
			}
		}

		// Damage helpers
		public static void ApplyDamage(Hero hero, int amount, bool bypassArmor)
		{
			amount = System.Math.Max(0, amount);
			if (!bypassArmor && hero.Armor > 0)
			{
				var blocked = System.Math.Min(hero.Armor, amount);
				hero.Armor -= blocked;
				amount -= blocked;
			}
			if (amount > 0) hero.Hp = System.Math.Max(0, hero.Hp - amount);
		}

		public static void ApplyDamage(Enemy enemy, int amount, bool bypassArmor)
		{
			amount = System.Math.Max(0, amount);
			if (!bypassArmor && enemy.Armor > 0)
			{
				var blocked = System.Math.Min(enemy.Armor, amount);
				enemy.Armor -= blocked;
				amount -= blocked;
			}
			if (amount > 0) enemy.Hp = System.Math.Max(0, enemy.Hp - amount);
		}

		public static int DamageByTier(int tier) => tier switch { 1 => 2, 2 => 4, 3 => 6, _ => 1 };
		public static int HealByTier(int tier)   => tier switch { 1 => 2, 2 => 4, 3 => 6, _ => 1 };
		public static int ArmorByTier(int tier)  => tier switch { 1 => 2, 2 => 4, 3 => 6, _ => 1 };
	}
}
