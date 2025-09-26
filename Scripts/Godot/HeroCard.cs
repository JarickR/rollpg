// Scripts/Godot/HeroCard.cs
// Display a hero with class + T1/T2 spell icons.

using System;
using Godot;

namespace DiceArena.Godot
{
	public partial class HeroCard : Control
	{
		private const int ICON_SIZE = 32;

		// Initialize to non-null NodePaths to silence CS8618
		[Export] public NodePath ClassIconPath { get; set; } = new NodePath();
		[Export] public NodePath Tier1IconPath  { get; set; } = new NodePath();
		[Export] public NodePath Tier2IconPath  { get; set; } = new NodePath();
		[Export] public NodePath NameLabelPath  { get; set; } = new NodePath();

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

		public void SetHero(string heroName, string classId, string? tier1SpellName, string? tier2SpellName)
		{
			if (_name != null)
				_name.Text = heroName;

			if (_classIcon != null)
				_classIcon.Texture = IconLibrary.GetClassTexture(classId, ICON_SIZE);

			if (_t1Icon != null)
				_t1Icon.Texture = !string.IsNullOrEmpty(tier1SpellName)
					? IconLibrary.GetSpellTexture(tier1SpellName!, 1, ICON_SIZE)
					: IconLibrary.Transparent1x1;

			if (_t2Icon != null)
				_t2Icon.Texture = !string.IsNullOrEmpty(tier2SpellName)
					? IconLibrary.GetSpellTexture(tier2SpellName!, 2, ICON_SIZE)
					: IconLibrary.Transparent1x1;
		}

		public void SetClass(string classId)
		{
			if (_classIcon != null)
				_classIcon.Texture = IconLibrary.GetClassTexture(classId, ICON_SIZE);
		}

		public void SetTier1(string? spellName)
		{
			if (_t1Icon == null) return;
			_t1Icon.Texture = !string.IsNullOrEmpty(spellName)
				? IconLibrary.GetSpellTexture(spellName!, 1, ICON_SIZE)
				: IconLibrary.Transparent1x1;
		}

		public void SetTier2(string? spellName)
		{
			if (_t2Icon == null) return;
			_t2Icon.Texture = !string.IsNullOrEmpty(spellName)
				? IconLibrary.GetSpellTexture(spellName!, 2, ICON_SIZE)
				: IconLibrary.Transparent1x1;
		}

		// --- helpers -----------------------------------------------------------
		private TextureRect? ResolveTextureRect(NodePath path)
		{
			var s = path.ToString();
			return string.IsNullOrEmpty(s) ? null : GetNodeOrNull<TextureRect>(path);
		}

		private Label? ResolveLabel(NodePath path)
		{
			var s = path.ToString();
			return string.IsNullOrEmpty(s) ? null : GetNodeOrNull<Label>(path);
		}

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
