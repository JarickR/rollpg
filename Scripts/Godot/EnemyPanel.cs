// Scripts/Godot/EnemyPanel.cs
using Godot;
using System.Collections.Generic;
using DiceArena.GodotUI;

namespace DiceArena.GodotUI
{
	/// <summary>
	/// Very simple enemy list for smoke testing: shows name + HP and a Hit button.
	/// Replace with your real enemy system later.
	/// </summary>
	public partial class EnemyPanel : VBoxContainer
	{
		private VBoxContainer _list = default!;
		private List<EnemyVM> _enemies = new();

		public override void _Ready()
		{
			AddThemeConstantOverride("separation", 6);

			var title = new Label { Text = "Enemies" };
			AddChild(title);

			_list = new VBoxContainer();
			AddChild(_list);
		}

		public void SetEnemies(IEnumerable<EnemyVM> enemies)
		{
			_enemies = new List<EnemyVM>(enemies);
			Rebuild();
		}

		private void Rebuild()
		{
			ClearChildren(_list);

			if (_enemies.Count == 0)
			{
				_list.AddChild(new Label { Text = "(none)" });
				return;
			}

			foreach (var e in _enemies)
			{
				var row = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
				row.AddChild(new Label { Text = e.Name, SizeFlagsHorizontal = SizeFlags.ExpandFill });

				var hp = new Label { Text = $"HP: {e.HP}" };
				row.AddChild(hp);

				var hit = new Button { Text = "Hit" };
				hit.Pressed += () =>
				{
					if (e.HP <= 0) return;
					e.HP -= 1;
					hp.Text = $"HP: {e.HP}";
					Log.Damage($"{e.Name} takes 1 damage. ({e.HP} HP)");
					if (e.HP <= 0) Log.System($"{e.Name} is defeated.");
				};
				row.AddChild(hit);

				_list.AddChild(row);
			}
		}

		public sealed class EnemyVM
		{
			public string Name { get; set; } = "Goblin";
			public int HP { get; set; } = 5;
		}

		// ---- local helper ----
		private static void ClearChildren(Node n)
		{
			foreach (var c in n.GetChildren())
				(c as Node)?.QueueFree();
		}
	}
}
