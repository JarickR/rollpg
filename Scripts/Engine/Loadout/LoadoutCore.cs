using System;
using System.Collections.Generic;
using System.Linq;
using DiceArena.Engine.Content;

namespace DiceArena.Engine.Loadout
{
	/// <summary>
	/// Pure helper: holds loadout constants and builds the party's dice given the user's final selections.
	/// </summary>
	public static class LoadoutCore
	{
		// Offer sizes (per member)
		public const int DefaultTier1OfferCount = 3;
		public const int DefaultTier2OfferCount = 2;

		// Required picks (per member)
		public const int DefaultTier1PickCount = 2;
		public const int DefaultTier2PickCount = 1;

		private static readonly string[] DefenseNames =
		{
			"Disrupt", "Redirect", "Delay", "Counterspell", "Dodge Roll", "Smokescreen"
		};

		public static PartySetup BuildPartySetup(
			IReadOnlyList<ClassDef> classes,
			IReadOnlyList<IReadOnlyList<SpellDef>> tier1Selections, // exactly 2 per member
			IReadOnlyList<SpellDef> tier2Selections)                // exactly 1 per member
		{
			if (classes.Count != tier1Selections.Count || classes.Count != tier2Selections.Count)
				throw new ArgumentException("Mismatched member counts when building party setup.");

			var rng = new Random();
			var party = new PartySetup();

			for (int i = 0; i < classes.Count; i++)
			{
				var cls = classes[i];
				var t1  = tier1Selections[i].ToList();
				var t2  = tier2Selections[i];

				var m = new MemberSetup
				{
					Class = cls,
					Tier1 = t1,
					Tier2 = t2,
					Dice  = BuildMemberDie(cls, t1, t2, rng)
				};

				// populate chosen id fields (offers can be filled by UI)
				m.ChosenTier1SpellIds = t1.Select(s => s.Id).ToList();
				m.ChosenTier2SpellId  = t2.Id;

				party.Members.Add(m);
			}

			return party;
		}

		// Exposed for callers that want to build one member die at a time
		public static List<DieFace> BuildMemberDie(ClassDef cls, IReadOnlyList<SpellDef> t1, SpellDef t2, Random? rng = null)
		{
			rng ??= new Random();

			var selectedSpells = new List<SpellDef>(4);
			selectedSpells.AddRange(t1);
			selectedSpells.Add(t2);

			var faces = new List<DieFace>(6);

			faces.Add(new DieFace { Kind = DieFaceKind.Hero, Display = cls.Name ?? cls.Id });
			faces.Add(new DieFace { Kind = DieFaceKind.Upgrade, Display = "Upgrade" });

			int spellSlots = 4;
			int used = 0;
			foreach (var s in selectedSpells)
			{
				if (used >= spellSlots) break;
				faces.Add(new DieFace
				{
					Kind = DieFaceKind.Spell,
					SpellId = s.Id,
					Display = s.Name ?? s.Id
				});
				used++;
			}

			for (; used < spellSlots; used++)
			{
				faces.Add(new DieFace
				{
					Kind = DieFaceKind.Defense,
					Display = $"Defense: {DefenseNames[rng.Next(DefenseNames.Length)]}"
				});
			}

			return faces;
		}
	}
}
