using System.Collections.Generic;

namespace DiceArena.Engine.Loadout
{
	/// <summary>
	/// Back-compat shim so older code that references PartyLoadout/MemberLoadout
	/// still compiles against the new canonical types.
	/// </summary>
	public class MemberLoadout : MemberSetup
	{
		public MemberLoadout() { }

		public MemberLoadout(MemberSetup from)
		{
			Class  = from.Class;
			Tier1  = new List<Content.SpellDef>(from.Tier1);
			Tier2  = from.Tier2;
			Dice   = new List<DieFace>(from.Dice);

			// copy compat id fields
			Tier1OfferIds       = new List<string>(from.Tier1OfferIds);
			Tier2OfferIds       = new List<string>(from.Tier2OfferIds);
			ChosenTier1SpellIds = new List<string>(from.ChosenTier1SpellIds);
			ChosenTier2SpellId  = from.ChosenTier2SpellId;
		}

		// Explicit passthroughs (older code may call these directly)
		public new string ClassId => base.ClassId;
	}

	public class PartyLoadout : PartySetup
	{
		public PartyLoadout() { }

		public PartyLoadout(PartySetup from)
		{
			Members = new List<MemberSetup>(from.Members);
		}

		// Some legacy code references this
		public int PartySize => Members?.Count ?? 0;
	}
}
