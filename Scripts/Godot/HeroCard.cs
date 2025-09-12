using Godot;
using DiceArena.Engine;

namespace DiceArena.GodotUI
{
	/// <summary>
	/// UI card for a hero. Shows class logo and the hero's current loadout icons.
	/// </summary>
	public partial class HeroCard : VBoxContainer
	{
		private Label _classLabel = default!;
		private TextureRect _classLogo = default!;
		private HBoxContainer _spellsRow = default!;
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
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				CustomMinimumSize = new Vector2(64, 64),
				SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
				SizeFlagsVertical = SizeFlags.ShrinkCenter
			};
			AddChild(_classLogo);

			_spellsRow = new HBoxContainer();
			_spellsRow.AddThemeConstantOverride("separation", 8);
			AddChild(_spellsRow);

			var actions = new HBoxContainer();
			actions.AddThemeConstantOverride("separation", 8);
			AddChild(actions);

			_upgradeBtn = new Button { Text = "Upgrade" };
			_upgradeBtn.Pressed += OnUpgradePressed;
			actions.AddChild(_upgradeBtn);

			_d6Btn = new Button { Text = "d6" };
			_d6Btn.Pressed += OnD6Pressed;
			actions.AddChild(_d6Btn);
		}

		public void Bind(Hero hero)
		{
			_hero = hero;

			_classLabel.Text = $"{hero.Name} [{hero.ClassId}]";
			_classLogo.Texture = IconLibrary.GetClassLogoByKey(hero.ClassId);

			RebuildLoadoutIcons();
		}

		private void RebuildLoadoutIcons()
		{
			foreach (Node child in _spellsRow.GetChildren())
				child.QueueFree();

			if (_hero == null || _hero.Loadout == null) return;

			foreach (var spell in _hero.Loadout)
			{
				var icon = new TextureRect
				{
					StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
					CustomMinimumSize = new Vector2(56, 56),
					SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
					SizeFlagsVertical = SizeFlags.ShrinkCenter,
					// CHANGED: use name-based icon fetcher
					Texture = IconLibrary.GetSpellIconByName(spell?.Name ?? "")
				};

				_spellsRow.AddChild(icon);
			}
		}

		private void OnUpgradePressed()
		{
			GD.Print($"Upgrade pressed for hero {_hero?.Name}");
		}

		private void OnD6Pressed()
		{
			GD.Print($"d6 pressed for hero {_hero?.Name}");
		}
	}
}
