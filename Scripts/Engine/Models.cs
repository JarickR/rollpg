// Scripts/Engine/Models.cs
using System.Collections.Generic;

namespace DiceArena.Engine
{
	/// <summary>
	/// Player/party member model used by the engine and UI.
	/// Relies on DiceArena.Engine.Spell (defined elsewhere).
	/// </summary>
	public class Hero
	{
		public string Name { get; set; }
		public string ClassId { get; set; }
		public int MaxHp { get; set; }
		public int Hp { get; set; }
		public int Armor { get; set; }

		/// <summary>
		/// The hero’s equipped/known spells (UI fills this during loadout).
		/// </summary>
		public List<Spell> Spells { get; } = new List<Spell>();

		public Hero(string name, string classId)
		{
			Name = name;
			ClassId = classId;
			MaxHp = 20;
			Hp = 20;
			Armor = 0;
		}
	}
}
