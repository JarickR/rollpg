// Scripts/Godot/HeroCard.cs
using Godot;
using System;
using DiceArena.Engine;

namespace DiceArena.Godot
{
	/// <summary>
	/// Displays hero info, class icon, and equipped spells on a card UI element.
	/// </summary>
	public partial class HeroCard : Control
	{
		[Export] private TextureRect? ClassIconRect;
		[Export] private TextureRect? Tier1SpellIconRect;
		[Export] private TextureRect? Tier2SpellIconRect;
		[Export] private Label? HeroNameLabel;

		private Hero? _hero;

		public override void _Ready()
		{
			if (HeroNameLabel != null)
				HeroNameLabel.Text = string.Empty;
		}

		/// <summary>
		/// Updates the card’s displayed hero info and icons.
		/// </summary>
		public void SetHero(Hero hero)
		{
			_hero = hero;
			UpdateCard();
		}

		private void UpdateCard()
		{
			if (_hero == null)
				return;

			// --- Hero name ---
			if (HeroNameLabel != null)
				HeroNameLabel.Text = _hero.Name ?? "Unnamed Hero";

			// --- Class icon ---
			if (ClassIconRect != null)
			{
				if (_hero.Class != null)
					ClassIconRect.Texture = IconLibrary.GetClassTexture(_hero.Class.Id);
				else
					ClassIconRect.Texture = IconLibrary.Transparent1x1;
			}

			// --- Tier 1 Spell ---
			if (Tier1SpellIconRect != null)
			{
				if (_hero.Tier1Spell != null)
					Tier1SpellIconRect.Texture = IconLibrary.GetSpellTexture(_hero.Tier1Spell.Id, 1);
				else
					Tier1SpellIconRect.Texture = IconLibrary.Transparent1x1;
			}

			// --- Tier 2 Spell ---
			if (Tier2SpellIconRect != null)
			{
				if (_hero.Tier2Spell != null)
					Tier2SpellIconRect.Texture = IconLibrary.GetSpellTexture(_hero.Tier2Spell.Id, 2);
				else
					Tier2SpellIconRect.Texture = IconLibrary.Transparent1x1;
			}
		}
	}
}
