// res://Scripts/Engine/Face.cs
namespace DiceArena.Engine
{
	public enum FaceType
	{
		ClassAbility = 0,
		Spell        = 1,
		Upgrade      = 2,
		Blank        = 3
	}

	public class Face
	{
		public FaceType Type { get; set; }
		public int Slot { get; set; }            // primary
		public int SlotIndex { get => Slot; set => Slot = value; } // alias for old code
		public Spell? Spell { get; set; }

		public Face() { Type = FaceType.Blank; Slot = -1; Spell = null; }
		public Face(FaceType type) { Type = type; Slot = -1; Spell = null; }
		public Face(FaceType type, int slot, Spell? spell = null)
		{
			Type = type; Slot = slot; Spell = spell;
		}

		public override string ToString()
		{
			return Type switch
			{
				FaceType.Spell        => $"Spell(slot {Slot}: {Spell?.Name ?? "?"})",
				FaceType.Upgrade      => "Upgrade",
				FaceType.ClassAbility => "Class Ability",
				_                     => "Blank"
			};
		}
	}
}
