// Scripts/Engine/Game.cs
using Godot;
using RollPG.GodotUI;

namespace DiceArena.Godot
{
	/// <summary>
	/// Very small game state container (autoload/singleton).
	/// Add this script to an autoload named "Game" (Project Settings → AutoLoad).
	/// </summary>
	public partial class Game : Node
	{
		public static Game I { get; private set; } = null!;

		public override void _EnterTree() => I = this;

		// -------- Party state ----------
		public int PartySize { get; private set; } = 1;

		/// <summary>Up to 4 members; only index 0 is used right now.</summary>
		public readonly PartyMember[] Members = new PartyMember[4];

		public void SetPartySize(int size) => PartySize = Mathf.Clamp(size, 1, 4);

		public void SetMember(int index, PartyMember m)
		{
			if (index < 0 || index >= Members.Length) return;
			Members[index] = m;
		}
	}

	/// <summary>Simple DTO for the hero shown on the battle screen.</summary>
	public struct PartyMember
	{
		public string Name;        // Display (e.g. "You")
		public string ClassId;     // e.g. "Thief"
		public string? Tier1;      // e.g. "attack"
		public string? Tier2;      // e.g. "attackplus"
	}
}
