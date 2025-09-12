// Scripts/Engine/Loadout/LoadoutSystem.cs
using System;
using System.Collections.Generic;
using System.Linq;
using DiceArena.Engine.Content;

namespace DiceArena.Engine.Loadout
{
	/// <summary>
	/// Creates and validates a PartyLoadout using the models in LoadoutModels.cs
	/// (PartyLoadout + MemberLoadout).
	/// </summary>
	public static class LoadoutSystem
	{
		/// <summary>
		/// Create a new party with the specified size and (optionally) assign class ids in order.
		/// </summary>
		public static PartyLoadout NewParty(int partySize, IReadOnlyList<string> classIds)
		{
			if (!LoadoutRules.IsValidPartySize(partySize))
				throw new ArgumentOutOfRangeException(nameof(partySize), "Party size must be 1..4.");

			var party = new PartyLoadout { PartySize = partySize };

			for (int i = 0; i < partySize; i++)
			{
				var m = new MemberLoadout();
				if (classIds != null && i < classIds.Count)
					m.ClassId = classIds[i] ?? string.Empty;

				party.Members.Add(m);
			}

			return party;
		}

		/// <summary>
		/// Assign the rolled offers and the player's selections for a single member.
		/// Overwrites previous values for that member.
		/// </summary>
		public static void ApplyPicks(
			PartyLoadout party,
			int memberIndex,
			IEnumerable<string> tier1OfferIds,
			IEnumerable<string> tier2OfferIds,
			IEnumerable<string> selectedTier1,
			string? selectedTier2)
		{
			if (party == null) throw new ArgumentNullException(nameof(party));
			if (memberIndex < 0 || memberIndex >= party.Members.Count)
				throw new ArgumentOutOfRangeException(nameof(memberIndex));

			var member = party.Members[memberIndex];

			member.Tier1OfferIds = (tier1OfferIds ?? Enumerable.Empty<string>())
				.Where(NotNullOrEmpty).Distinct().ToList();

			member.Tier2OfferIds = (tier2OfferIds ?? Enumerable.Empty<string>())
				.Where(NotNullOrEmpty).Distinct().ToList();

			member.ChosenTier1SpellIds = new HashSet<string>(
				(selectedTier1 ?? Enumerable.Empty<string>())
					.Where(NotNullOrEmpty)
					.Distinct()
					.Take(LoadoutRules.Tier1Picks));

			member.ChosenTier2SpellId = string.IsNullOrWhiteSpace(selectedTier2) ? null : selectedTier2;
		}

		/// <summary>
		/// Validate a single member’s picks against the rules and their offers.
		/// </summary>
		public static (bool ok, string reason) ValidateMember(MemberLoadout m)
		{
			if (string.IsNullOrWhiteSpace(m.ClassId))
				return (false, "Class not selected.");

			if (m.ChosenTier1SpellIds.Count != LoadoutRules.Tier1Picks)
				return (false, $"Tier 1 selections must be exactly {LoadoutRules.Tier1Picks}.");

			if (m.ChosenTier2SpellId == null)
				return (false, $"Tier 2 selection must be exactly {LoadoutRules.Tier2Picks}.");

			// Containment checks
			if (!m.ChosenTier1SpellIds.All(id => m.Tier1OfferIds.Contains(id)))
				return (false, "One or more Tier 1 selections are not in the offered list.");

			if (!m.Tier2OfferIds.Contains(m.ChosenTier2SpellId))
				return (false, "Tier 2 selection is not in the offered list.");

			return (true, "");
		}

		/// <summary>
		/// Validate the entire party. Returns index of first invalid member if any.
		/// </summary>
		public static (bool ok, int badIndex, string reason) ValidateParty(PartyLoadout party)
		{
			if (party == null) return (false, -1, "Party is null.");
			for (int i = 0; i < party.Members.Count; i++)
			{
				var (ok, reason) = ValidateMember(party.Members[i]);
				if (!ok) return (false, i, reason);
			}
			return (true, -1, "");
		}

		private static bool NotNullOrEmpty(string s) => !string.IsNullOrEmpty(s);

		/// <summary>
		/// Convenience: return a readable string describing a party's picks (for logs/debug UI).
		/// </summary>
		public static string Describe(PartyLoadout party, ContentBundle? bundle = null)
		{
			var lines = new List<string>();
			for (int i = 0; i < party.Members.Count; i++)
			{
				var m = party.Members[i];
				string className = m.ClassId;

				if (bundle != null)
				{
					var cls = bundle.Classes.FirstOrDefault(c => c.Id == m.ClassId);
					if (cls != null) className = cls.Name;
				}

				lines.Add($"Member {i + 1}: Class = {className}");
				lines.Add($"  T1 Offers : [{string.Join(", ", ResolveNames(m.Tier1OfferIds, bundle))}]");
				lines.Add($"  T2 Offers : [{string.Join(", ", ResolveNames(m.Tier2OfferIds, bundle))}]");
				lines.Add($"  Picked T1 : [{string.Join(", ", ResolveNames(m.ChosenTier1SpellIds, bundle))}]");
				lines.Add($"  Picked T2 : {ResolveName(m.ChosenTier2SpellId ?? "", bundle)}");
			}
			return string.Join(Environment.NewLine, lines);
		}

		private static IEnumerable<string> ResolveNames(IEnumerable<string> ids, ContentBundle? bundle)
		{
			foreach (var id in ids)
				yield return ResolveName(id, bundle);
		}

		private static string ResolveName(string id, ContentBundle? bundle)
		{
			if (bundle == null || string.IsNullOrEmpty(id)) return id ?? "";
			var sp = bundle.Spells.FirstOrDefault(s => s.Id == id);
			if (sp != null) return sp.Name;
			var cls = bundle.Classes.FirstOrDefault(c => c.Id == id);
			if (cls != null) return cls.Name;
			return id;
		}
	}
}
