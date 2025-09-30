using System;

namespace DiceArena.Engine.Core
{
	/// <summary>Immutable snapshot of one party member’s loadout.</summary>
	public sealed class PartyMember
	{
		public string ClassId { get; }
		public string? Tier1A { get; }
		public string? Tier1B { get; }
		public string? Tier2  { get; }

		public PartyMember(string classId, string? t1a, string? t1b, string? t2)
		{
			ClassId = classId ?? throw new ArgumentNullException(nameof(classId));
			Tier1A  = t1a;
			Tier1B  = t1b;
			Tier2   = t2;
		}
	}
}
