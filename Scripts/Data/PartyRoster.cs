// res://Scripts/Data/PartyRoster.cs
#nullable enable
using System.Collections.Generic;

namespace DiceArena.DataModels
{
	/// <summary>
	/// Holds the current party selection for the player.
	/// </summary>
	public sealed class PartyRoster
	{
		public int MaxPartySize { get; }
		private readonly List<CharacterDefinition> _members;

		public PartyRoster(int maxPartySize = 4)
		{
			MaxPartySize = maxPartySize <= 0 ? 1 : maxPartySize;
			_members = new List<CharacterDefinition>(MaxPartySize);
		}

		public IReadOnlyList<CharacterDefinition> Members => _members;

		public bool TryAdd(CharacterDefinition def)
		{
			if (def is null) return false;
			if (_members.Count >= MaxPartySize) return false;
			_members.Add(def);
			return true;
		}

		public bool Remove(CharacterDefinition def) => _members.Remove(def);

		public void Clear() => _members.Clear();
	}
}
