using System.Collections.Generic;
using DiceArena.Engine.Content;

namespace DiceArena.Engine.Loadout
{
	/// <summary>Kinds of faces you can roll on a member's d6.</summary>
	public enum DieFaceKind
	{
		Spell,
		Hero,
		Upgrade,
		Defense
	}

	/// <summary>A single face on the d6.</summary>
	public class DieFace
	{
		public DieFaceKind Kind { get; set; }
		/// <summary>Used when Kind == Spell (id from ContentDatabase).</summary>
		public string? SpellId { get; set; }
		/// <summary>Display label / description, e.g. class hero action or defense name.</summary>
		public string? Display { get; set; }
	}

	/// <summary>Per-member spell offers rendered on the loadout card.</summary>
	public sealed class MemberOffer
	{
		public int MemberIndex { get; set; }
		public List<SpellDef> Tier1 { get; set; } = new();
		public List<SpellDef> Tier2 { get; set; } = new();
		public int Tier1PickCount { get; set; } = 2;
		public int Tier2PickCount { get; set; } = 1;
	}

	/// <summary>All offers for the party at loadout time.</summary>
	public sealed class PartyOffer
	{
		public List<MemberOffer> Members { get; } = new();

		public int Count => Members.Count;
		public MemberOffer this[int index] => Members[index];

		public void Clear() => Members.Clear();
		public void Add(MemberOffer m) => Members.Add(m);
	}

	/// <summary>Final per-member selection + the built d6.</summary>
	public class MemberSetup
	{
		public ClassDef Class { get; set; } = default!;
		public List<SpellDef> Tier1 { get; set; } = new(); // exactly 2
		public SpellDef Tier2 { get; set; } = default!;    // exactly 1
		public List<DieFace> Dice { get; set; } = new(6);  // built d6

		// ---- Back-compat fields (IDs) expected by older code ----
		public string ClassId => Class.Id;

		/// <summary>IDs of the offered Tier1 spells for this member (3 ids).</summary>
		public List<string> Tier1OfferIds { get; set; } = new();

		/// <summary>IDs of the offered Tier2 spells for this member (2 ids).</summary>
		public List<string> Tier2OfferIds { get; set; } = new();

		/// <summary>IDs of the chosen Tier1 spells (exactly 2 ids).</summary>
		public List<string> ChosenTier1SpellIds { get; set; } = new();

		/// <summary>ID of the chosen Tier2 spell (exactly 1 id).</summary>
		public string? ChosenTier2SpellId { get; set; }
	}

	/// <summary>Final party configuration ready for the battle scene / loop.</summary>
	public class PartySetup
	{
		public List<MemberSetup> Members { get; set; } = new();

		public int Count => Members.Count;
		public MemberSetup this[int index] => Members[index];
	}
}
