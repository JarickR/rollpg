// HeroCard.cs
// Display a hero with class + T1/T2 spell icons.

using System;
using Godot;

namespace DiceArena.Godot
{
	public partial class HeroCard : Control
	{
		// --- Configuration ----------------------------------------------------
		private const int ICON_SIZE = 32; // <== pick what looks best on your card

		// Optionally wire these in the Inspector (or leave empty and we'll search)
		[Export] public NodePath ClassIconPath { get; set; } = default;
		[Export] public NodePath Tier1IconPath  { get; set; } = default;
		[Export] public NodePath Tier2IconPath  { get; set; } = default;
		[Export] public NodePath NameLabelPath  { get; set; } = default;

		// --- Nodes -------------------------------------------------------------
		private TextureRect? _classIcon;
		private TextureRect? _t1Icon;
		private TextureRect? _t2Icon;
		private Label?       _name;

		public override void _Ready()
		{
			_classIcon = ResolveTextureRect(ClassIconPath) ?? Find<TextureRect>("ClassIcon", "Class", "ClassTex");
			_t1Icon    = ResolveTextureRect(Tier1IconPath) ?? Find<TextureRect>("T1Icon", "Tier1", "Tier1Tex");
			_t2Icon    = ResolveTextureRect(Tier2IconPath) ?? Find<TextureRect>("T2Icon", "Tier2", "Tier2Tex");
			_name      = ResolveLabel(NameLabelPath)       ?? Find<Label>("Name", "Title");

			// Safe defaults so nothing throws if a path isn't wired
			if (_classIcon != null) _classIcon.Texture = IconLibrary.Transparent1x1;
			if (_t1Icon    != null) _t1Icon.Texture    = IconLibrary.Transparent1x1;
			if (_t2Icon    != null) _t2Icon.Texture    = IconLibrary.Transparent1x1;
		}

		// Public API you can call from elsewhere:
		public void SetHero(string heroName, string classId, string? tier1SpellName, string? tier2SpellName)
		{
			if (_name != null)
				_name.Text = heroName;

			// NOTE: the only change vs your old code is the added ICON_SIZE argument.
			if (_classIcon != null)
				_classIcon.Texture = IconLibrary.GetClassTexture(classId, ICON_SIZE);

			if (_t1Icon != null)
				_t1Icon.Texture = tier1SpellName is { Length: > 0 }
					? IconLibrary.GetSpellTexture(tier1SpellName, 1, ICON_SIZE)
					: IconLibrary.Transparent1x1;

			if (_t2Icon != null)
				_t2Icon.Texture = tier2SpellName is { Length: > 0 }
					? IconLibrary.GetSpellTexture(tier2SpellName, 2, ICON_SIZE)
					: IconLibrary.Transparent1x1;
		}

		// Convenience overload if you only want to set class:
		public void SetClass(string classId)
		{
			if (_classIcon != null)
				_classIcon.Texture = IconLibrary.GetClassTexture(classId, ICON_SIZE);
		}

		// Convenience overloads for spells:
		public void SetTier1(string? spellName)
		{
			if (_t1Icon == null) return;
			_t1Icon.Texture = spellName is { Length: > 0 }
				? IconLibrary.GetSpellTexture(spellName, 1, ICON_SIZE)
				: IconLibrary.Transparent1x1;
		}

		public void SetTier2(string? spellName)
		{
			if (_t2Icon == null) return;
			_t2Icon.Texture = spellName is { Length: > 0 }
				? IconLibrary.GetSpellTexture(spellName, 2, ICON_SIZE)
				: IconLibrary.Transparent1x1;
		}

		// --- helpers -----------------------------------------------------------
		private TextureRect? ResolveTextureRect(NodePath path)
			=> path.IsEmpty ? null : GetNodeOrNull<TextureRect>(path);

		private Label? ResolveLabel(NodePath path)
			=> path.IsEmpty ? null : GetNodeOrNull<Label>(path);

		private T? Find<T>(params string[] names) where T : CanvasItem
		{
			foreach (var n in names)
			{
				var node = GetNodeOrNull<T>(n);
				if (node != null) return node;
			}
			return null;
		}
	}
}
