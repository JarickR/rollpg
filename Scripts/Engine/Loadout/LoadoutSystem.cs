using System;
using System.Collections.Generic;
using System.Linq;
using DiceArena.Engine.Content;

namespace DiceArena.Engine.Loadout
{
	/// <summary>Centralizes numeric rules for offers & picks (used by older code paths).</summary>
	public static class LoadoutRules
	{
		public const int Tier1Offers = LoadoutCore.DefaultTier1OfferCount; // 3
		public const int Tier2Offers = LoadoutCore.DefaultTier2OfferCount; // 2
		public const int Tier1Picks  = LoadoutCore.DefaultTier1PickCount;  // 2
		public const int Tier2Picks  = LoadoutCore.DefaultTier2PickCount;  // 1
	}

	/// <summary>
	/// Thin compatibility layer so older call-sites can build offers and finalize a party
	/// without directly touching the new UI classes.
	/// </summary>
	public static class LoadoutSystem
	{
		/// <summary>Generate a PartyOffer from explicit spell pools.</summary>
		public static PartyOffer BuildOffers(int partySize, IList<SpellDef> tier1Pool, IList<SpellDef> tier2Pool, int? seed = null)
		{
			var rng = seed.HasValue ? new Random(seed.Value) : new Random();
			partySize = Math.Clamp(partySize, 1, 4);

			var offer = new PartyOffer();
			for (int i = 0; i < partySize; i++)
			{
				var t1 = DrawDistinct(tier1Pool, LoadoutRules.Tier1Offers, rng);
				var t2 = DrawDistinct(tier2Pool, LoadoutRules.Tier2Offers, rng);

				offer.Members.Add(new MemberOffer
				{
					MemberIndex    = i,
					Tier1          = t1,
					Tier2          = t2,
					Tier1PickCount = LoadoutRules.Tier1Picks,
					Tier2PickCount = LoadoutRules.Tier2Picks
				});
			}
			return offer;
		}

		/// <summary>Convert a PartyOffer to a PartyLoadout shell with offer IDs pre-populated (legacy).</summary>
		public static PartyLoadout ToPartyLoadout(PartyOffer offers)
		{
			var party = new PartyLoadout();
			for (int i = 0; i < offers.Count; i++)
			{
				var m = new MemberLoadout
				{
					Tier1OfferIds = offers[i].Tier1.Select(s => s.Id).ToList(),
					Tier2OfferIds = offers[i].Tier2.Select(s => s.Id).ToList()
				};
				party.Members.Add(m);
			}
			return party;
		}

		/// <summary>
		/// Finalize a party using concrete selections (resolved defs), returning a PartySetup with built dice.
		/// Accepts IList<> for older call sites and converts internally.
		/// </summary>
		public static PartySetup Finalize(
			IList<ClassDef> classes,
			IList<IReadOnlyList<SpellDef>> chosenT1,
			IList<SpellDef> chosenT2)
		{
			// Convert to List<> so we can pass IReadOnlyList<> cleanly
			var classList = classes is List<ClassDef> lc ? lc : new List<ClassDef>(classes);
			var t1List    = chosenT1 is List<IReadOnlyList<SpellDef>> lt1 ? lt1 : new List<IReadOnlyList<SpellDef>>(chosenT1);
			var t2List    = chosenT2 is List<SpellDef> lt2 ? lt2 : new List<SpellDef>(chosenT2);

			return LoadoutCore.BuildPartySetup(classList, t1List, t2List);
		}

		private static List<T> DrawDistinct<T>(IList<T> pool, int count, Random rng)
		{
			var result = new List<T>(count);
			if (pool.Count == 0) return result;

			var idx = Enumerable.Range(0, pool.Count).ToList();
			for (int pick = 0; pick < count && idx.Count > 0; pick++)
			{
				int j = rng.Next(0, idx.Count);
				result.Add(pool[idx[j]]);
				idx.RemoveAt(j);
			}
			return result;
		}
	}
}
