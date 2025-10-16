// res://Scripts/Data/CharacterDefinition.cs
#nullable enable
using Godot;

namespace DiceArena.DataModels
{
	/// <summary>
	/// Basic definition of a playable character/class.
	/// </summary>
	public sealed class CharacterDefinition
	{
		/// <summary>Display name shown in UI (e.g., "Barbarian").</summary>
		public required string DisplayName { get; init; }

		/// <summary>Portrait/icon texture.</summary>
		public required Texture2D Portrait { get; init; }

		/// <summary>Scene to instantiate for this character in battle.</summary>
		public required PackedScene BattleScene { get; init; }
	}
}
