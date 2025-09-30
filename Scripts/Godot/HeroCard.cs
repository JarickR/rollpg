using Godot;
using EnginePM = DiceArena.Engine.Core.PartyMember;

namespace DiceArena.Godot
{
	/// <summary>Displays the hero portrait + two spell icons and a name label.</summary>
	public partial class HeroCard : Control
	{
		[Export] public NodePath ClassIconPath { get; set; } = default!;
		[Export] public NodePath Tier1IconPath { get; set; } = default!;
		[Export] public NodePath Tier2IconPath { get; set; } = default!;
		[Export] public NodePath NameLabelPath { get; set; } = default!;

		private TextureRect? _classIcon;
		private TextureRect? _t1Icon;
		private TextureRect? _t2Icon;
		private Label?       _nameLbl;

		private const int ICON_SIZE = 64; // keep aligned with your import size

		public override void _Ready()
		{
			_classIcon = GetNodeOrNull<TextureRect>(ClassIconPath);
			_t1Icon    = GetNodeOrNull<TextureRect>(Tier1IconPath);
			_t2Icon    = GetNodeOrNull<TextureRect>(Tier2IconPath);
			_nameLbl   = GetNodeOrNull<Label>(NameLabelPath);
		}

		public void SetHero(EnginePM m)
		{
			if (_nameLbl != null)
				_nameLbl.Text = m.ClassId;

			// Class icon
			if (_classIcon != null)
				_classIcon.Texture = IconLibrary.GetClassTexture(m.ClassId, ICON_SIZE);

			// Tier 1 (prefer A, fallback B)
			string? t1 = !string.IsNullOrEmpty(m.Tier1A) ? m.Tier1A : m.Tier1B;
			if (!string.IsNullOrEmpty(t1) && _t1Icon != null)
				_t1Icon.Texture = IconLibrary.GetSpellTexture(t1, 1, ICON_SIZE);

			// Tier 2
			if (!string.IsNullOrEmpty(m.Tier2) && _t2Icon != null)
				_t2Icon.Texture = IconLibrary.GetSpellTexture(m.Tier2, 2, ICON_SIZE);
		}
	}
}
