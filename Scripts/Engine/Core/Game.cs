using System;
using System.Collections.Generic;

namespace DiceArena.Engine.Core
{
	/// <summary>Minimal engine state we pass from the Loadout screen to the Battle screen.</summary>
	public sealed class Game
	{
		private readonly List<PartyMember> _members = new();

		/// <summary>Total intended party size (UI sets this).</summary>
		public int PartySize { get; set; } = 1;

		public IReadOnlyList<PartyMember> Members => _members;

		public void ClearMembers() => _members.Clear();

		public void AddMember(PartyMember pm)
		{
			if (pm is null) throw new ArgumentNullException(nameof(pm));
			_members.Add(pm);
		}

		public PartyMember GetMember(int index) => _members[index];
	}
}
