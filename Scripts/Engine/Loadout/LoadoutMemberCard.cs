// File: Scripts/Engine/Loadout/LoadoutMemberCard.cs
using Godot;
using DiceArena.Engine.Content;

namespace DiceArena.Engine.Loadout
{
	/// <summary>
	/// Compact visual for a selected party member:
	/// [Class Icon + name]  |  [T1] [T2] [T3]
	/// All icons have tooltips built from content.
	/// </summary>
	public partial class LoadoutMemberCard : HBoxContainer
	{
		// Sizing
		[Export] public int IconSize { get; set; } = 48;
		[Export] public int Gap { get; set; } = 8;

		// UI parts
		private TextureRect _classIcon = null!;
		private Label _className = null!;
		private TextureRect _t1 = null!;
		private TextureRect _t2 = null!;
		private TextureRect _t3 = null!;

		// Data
		private ClassDef? _classDef;
		private SpellDef? _tier1;
		private SpellDef? _tier2;
		private SpellDef? _tier3;

		public override void _Ready()
		{
			BuildSkeleton();
			if (_classDef != null)
				Refresh();
		}

		private void BuildSkeleton()
		{
			// Set spacing for this HBox
			AddThemeConstantOverride("separation", Gap);

			// Left: class icon + name
			var left = new VBoxContainer
			{
				SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
				SizeFlagsVertical   = Control.SizeFlags.ShrinkCenter
			};
			AddChild(left);

			_classIcon = NewIconRect();
			left.AddChild(_classIcon);

			_className = new Label
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				Text = ""
			};
			left.AddChild(_className);

			// Right: three spell icons in a row
			var right = new HBoxContainer
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical   = Control.SizeFlags.ShrinkCenter
			};
			right.AddThemeConstantOverride("separation", Gap);
			AddChild(right);

			_t1 = NewIconRect();
			_t2 = NewIconRect();
			_t3 = NewIconRect();

			right.AddChild(_t1);
			right.AddChild(_t2);
			right.AddChild(_t3);
		}

		private TextureRect NewIconRect()
		{
			return new TextureRect
			{
				// In Godot 4, just set a minimum size and a stretch mode.
				// (There is no 'IgnoreTextureSize' property anymore.)
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				CustomMinimumSize = new Vector2(IconSize, IconSize),
				SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
				SizeFlagsVertical   = Control.SizeFlags.ShrinkCenter,
				TooltipText = ""
			};
		}

		/// <summary>Set/refresh the card with content.</summary>
		public void SetData(ClassDef cls, SpellDef? t1, SpellDef? t2, SpellDef? t3)
		{
			_classDef = cls;
			_tier1 = t1;
			_tier2 = t2;
			_tier3 = t3;

			if (IsInsideTree())
				Refresh();
		}

		private void Refresh()
		{
			if (_classDef == null)
				return;

			_className.Text = _classDef.Name ?? _classDef.Id;

			// Class icon + tooltip
			_classIcon.Texture = IconLibraryBridge.Class(_classDef.Id, IconSize);
			_classIcon.TooltipText = TooltipBuilder.BuildClassTooltip(_classDef);

			// Spells (some may be null)
			ApplySpell(_t1, _tier1);
			ApplySpell(_t2, _tier2);
			ApplySpell(_t3, _tier3);
		}

		private void ApplySpell(TextureRect target, SpellDef? def)
		{
			if (def == null)
			{
				target.Texture = null; // empty slot
				target.TooltipText = "Empty";
				return;
			}

			target.Texture = IconLibraryBridge.Spell(def.Id, def.Tier, IconSize);
			target.TooltipText = TooltipBuilder.BuildSpellTooltip(def);
		}
	}
}
