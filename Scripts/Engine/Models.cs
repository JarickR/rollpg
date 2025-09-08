// Scripts/Engine/Models.cs
namespace DiceArena.Engine
{
#if false
	/// <summary>
	/// Legacy Hero (disabled). Use Engine/Hero.cs instead.
	/// </summary>
	public class Hero
	{
		public string Name { get; set; }
		public string ClassId { get; set; }
		public int MaxHp { get; set; }
		public int Hp { get; set; }
		public int Armor { get; set; }
		public System.Collections.Generic.List<Spell> Spells { get; } = new();

		public Hero(string name, string classId)
		{
			Name = name;
			ClassId = classId;
			MaxHp = 20;
			Hp = 20;
			Armor = 0;
		}
	}
#endif
}
