// Scripts/Engine/Loadout/LoadoutModels.cs
using System;
using System.Collections.Generic;
using System.Linq;
using DiceArena.Engine.Content;

namespace DiceArena.Engine.Loadout
{
	public enum DieFaceKind { HeroAction, Upgrade, Spell, Defense }

	public enum DefenseKind
	{
		Disrupt,
		Redirect,
		Delay,
		Counterspell,
		DodgeRoll,
		SmokeScreen
	}

	public static class DefenseRules
	{
		public static readonly Dictionary<DefenseKind, string> Text = new()
		{
			{ DefenseKind.Disrupt, "Cancel the next spell that specifically targets you (not physical or AoE)." },
			{ DefenseKind.Redirect, "Choose another player: the next spell that targets you is redirected to them (not physical/AoE)." },
			{ DefenseKind.Delay, "The next spell that targets you is delayed; it resolves at the start of your next turn." },
			{ DefenseKind.Counterspell, "The next spell that targets you deals half damage to you and reflects half to the caster." },
			{ DefenseKind.DodgeRoll, "When targeted by a physical attack, roll d6: 1–3 Dodge (miss), 4–6 Fail (hit)." },
			{ DefenseKind.SmokeScreen, "The next spell attack targeting you has a 50% chance to miss (1–3 miss, 4–6 hit)." },
		};
	}

	// ⬇️ made properties mutable so Upgrade can change a face in-place
	public sealed class DieFace
	{
		public DieFaceKind Kind { get; set; }
		public string? SpellId { get; set; }           // used when Kind == Spell
		public DefenseKind? Defense { get; set; }      // used when Kind == Defense

		public override string ToString()
		{
			return Kind switch
			{
				DieFaceKind.HeroAction => "Hero Action",
				DieFaceKind.Upgrade    => "Upgrade",
				DieFaceKind.Spell      => $"Spell:{SpellId}",
				DieFaceKind.Defense    => $"Defense:{Defense}",
				_ => "?"
			};
		}
	}

	public sealed class MemberLoadout
	{
		public string ClassId { get; set; } = "";

		// Offers shown to the player
		public List<string> Tier1OfferIds { get; set; } = new(); // 3
		public List<string> Tier2OfferIds { get; set; } = new(); // 2

		// Player selections (must be 2×T1, 1×T2)
		public HashSet<string> ChosenTier1SpellIds { get; set; } = new();
		public string? ChosenTier2SpellId { get; set; }

		// Final d6 after sealing
		public List<DieFace> Die { get; set; } = new(6);
	}

	public sealed class PartyLoadout
	{
		public int PartySize { get; set; } = 1; // 1..4
		public List<MemberLoadout> Members { get; set; } = new();
	}

	public static class LoadoutRules
	{
		public const int Tier1OfferCount = 3;
		public const int Tier2OfferCount = 2;
		public const int Tier1Picks = 2;
		public const int Tier2Picks = 1;

		public const int TotalFaces = 6;
		public const int SpellFaceSlots = 4;

		public static bool IsValidPartySize(int s) => s >= 1 && s <= 4;

		public static bool IsValidMemberSelection(MemberLoadout m)
			=> m.ChosenTier1SpellIds.Count == Tier1Picks && !string.IsNullOrWhiteSpace(m.ChosenTier2SpellId);

		public static IEnumerable<DefenseKind> AllDefenses()
			=> Enum.GetValues(typeof(DefenseKind)).Cast<DefenseKind>();
	}
}
