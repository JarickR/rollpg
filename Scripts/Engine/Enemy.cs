// res://Scripts/Engine/Enemy.cs
namespace DiceArena.Engine
{
	public class Enemy
	{
		public string Id { get; set; } = "E1";
		public string Name { get; set; } = "Enemy";
		public int Tier { get; set; } = 1;

		public int MaxHp { get; set; } = 8;
		public int Hp { get; set; } = 8;
		public int Armor { get; set; } = 0;

		// Alias for some UIs
		public int CurrentHp { get => Hp; set => Hp = value; }

		public int PoisonStacks { get; set; } = 0;
		public int BombStacks { get; set; } = 0;

		public Enemy() { }

		// Minimal compat ctor
		public Enemy(string id, int hp = 8)
		{
			Id = id; Name = id; Tier = 1;
			MaxHp = hp; Hp = hp;
		}

		// Full ctor
		public Enemy(string id, string name, int tier, int maxHp, int armor = 0)
		{
			Id = id; Name = name; Tier = tier;
			MaxHp = maxHp; Hp = maxHp; Armor = armor;
		}

		public void AddArmor(int amt) => Armor = System.Math.Max(0, Armor + System.Math.Max(0, amt));

		public void Damage(int amt, bool bypassArmor)
		{
			amt = System.Math.Max(0, amt);
			if (!bypassArmor && Armor > 0)
			{
				var blocked = System.Math.Min(Armor, amt);
				Armor -= blocked;
				amt   -= blocked;
			}
			if (amt > 0) Hp = System.Math.Max(0, Hp - amt);
		}

		public override string ToString() => $"{Id} {Name} (T{Tier}) HP {Hp}/{MaxHp} AR {Armor}";
	}
}
