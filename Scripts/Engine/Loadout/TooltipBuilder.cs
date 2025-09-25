// Scripts/Engine/Loadout/TooltipBuilder.cs
using System.Text;
using DiceArena.Engine.Content; // ClassDef, SpellDef

namespace DiceArena.Engine.Loadout
{
	public static class TooltipBuilder
	{
		/// <summary>Body text for a class tooltip.</summary>
		public static string BuildClassTooltip(ClassDef c)
		{
			var sb = new StringBuilder();

			// ClassDef properties per current model:
			// Id, Name, Trait, HeroAction :contentReference[oaicite:0]{index=0}
			if (!string.IsNullOrWhiteSpace(c.Trait))
				sb.AppendLine(c.Trait.Trim());

			if (!string.IsNullOrWhiteSpace(c.HeroAction))
			{
				if (sb.Length > 0) sb.AppendLine();
				sb.Append(c.HeroAction.Trim());
			}

			return sb.ToString().Trim();
		}

		/// <summary>Body text for a spell tooltip.</summary>
		public static string BuildSpellTooltip(SpellDef s)
		{
			var sb = new StringBuilder();

			// SpellDef properties per current model:
			// Id, Name, Tier, Kind, Text, Order :contentReference[oaicite:1]{index=1}
			sb.AppendLine($"Tier {s.Tier}");
			if (!string.IsNullOrWhiteSpace(s.Kind))
				sb.AppendLine(s.Kind.Trim());

			if (!string.IsNullOrWhiteSpace(s.Text))
			{
				if (sb.Length > 0) sb.AppendLine();
				sb.Append(s.Text.Trim());
			}

			return sb.ToString().Trim();
		}
	}
}
