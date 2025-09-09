using Godot;
using System.Linq;
using DiceArena.Engine;

namespace DiceArena.GodotUI
{
	/// <summary>
	/// UI card showing the hero’s class logo and current loadout.
	/// </summary>
	public partial class HeroCard : VBoxContainer
	{
		private Label _classLabel = default!;
		private TextureRect _classLogo = default!;
		private HBoxContainer _spellsRow = default!;
		private HBoxContainer _actions = default!;
		private Button _upgradeBtn = default!;
		private Button _d6Btn = default!;

		private Hero _hero = default!;

		public override void _Ready()
		{
			AddThemeConstantOverride("separation", 8);

			_classLabel = new Label { Text = "Hero" };
			AddChild(_classLabel);

			_classLogo = new TextureRect
			{
				CustomMinimumSize = new Vector2(72, 72),
				StretchMode       = TextureRect.StretchModeEnum.KeepAspectCentered,
				SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
				SizeFlagsVertical   = SizeFlags.ShrinkCenter
			};
			AddChild(_classLogo);

			_spellsRow = new HBoxContainer();
			_spellsRow.AddThemeConstantOverride("separation", 8);
			AddChild(_spellsRow);

			_actions = new HBoxContainer();
			_actions.AddThemeConstantOverride("separation", 8);
			AddChild(_actions);

			_upgradeBtn = new Button { Text = "Upgrade" };
			_upgradeBtn.Pressed += () => GD.Print($"Upgrade {_hero?.Name}");
			_actions.AddChild(_upgradeBtn);

			_d6Btn = new Button { Text = "d6" };
			_d6Btn.Pressed += () => GD.Print($"Roll d6 for {_hero?.Name}");
			_actions.AddChild(_d6Btn);
		}

		public void Bind(Hero hero)
		{
			_hero = hero;

			// Use your engine’s class key/name property. If it’s "ClassKey" instead of "ClassId", change it here:
			string classKey = hero.ClassId; // or hero.ClassKey
			_classLabel.Text = $"{hero.Name} [{classKey}]";

			_classLogo.Texture = IconLibrary.GetClassLogoByKey(classKey);

			RebuildLoadoutIcons();
		}

		private void RebuildLoadoutIcons()
		{
			foreach (var c in _spellsRow.GetChildren())
				c.QueueFree();

			if (_hero?.Loadout == null || _hero.Loadout.Count == 0)
				return;

			foreach (var spell in _hero.Loadout)
			{
				var r = new TextureRect
				{
					CustomMinimumSize = new Vector2(56, 56),
					StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
					SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
					SizeFlagsVertical = SizeFlags.ShrinkCenter,
					Texture = IconLibrary.GetSpellIcon(spell),
					TooltipText = spell?.Name ?? "Spell"
				};
				_spellsRow.AddChild(r);
			}
		}
	}
}
