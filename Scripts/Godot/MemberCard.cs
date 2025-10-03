using Godot;
using System.Collections.Generic;

namespace DiceArena.Engine.Loadout
{
	/// <summary>
	/// Summary card for a single member.
	/// It ONLY displays current selection (class icon, two T1 icons, one T2 icon).
	/// No layout/creation logic here; it uses the existing nodes in the scene.
	/// </summary>
	public partial class MemberCard : Control
	{
		// Scene nodes (assign in the MemberCard.tscn Inspector if names differ)
		[Export] public NodePath ClassIconPath { get; set; } = "MarginContainer/VBoxContainer/ClassRow/ClassIcon";
		[Export] public NodePath T1AIconPath  { get; set; } = "MarginContainer/VBoxContainer/Tier1SpellsRow/T1IconA";
		[Export] public NodePath T1BIconPath  { get; set; } = "MarginContainer/VBoxContainer/Tier1SpellsRow/T1IconB";
		[Export] public NodePath T2IconPath   { get; set; } = "MarginContainer/VBoxContainer/Tier2SpellsRow/T2Icon";

		private TextureRect? _classIcon;
		private TextureRect? _t1a;
		private TextureRect? _t1b;
		private TextureRect? _t2;

		public override void _Ready()
		{
			_classIcon = GetNodeOrNull<TextureRect>(ClassIconPath);
			_t1a       = GetNodeOrNull<TextureRect>(T1AIconPath);
			_t1b       = GetNodeOrNull<TextureRect>(T1BIconPath);
			_t2        = GetNodeOrNull<TextureRect>(T2IconPath);
		}

		public void SetClassIcon(string? classIdOrNull, int size = 64)
		{
			if (_classIcon == null) return;
			_classIcon.Texture = classIdOrNull != null
				? LoadClassTexture(classIdOrNull)
				: Transparent1x1;
		}

		public void SetTier1Icons(List<string> t1Ids, int size = 64)
		{
			if (_t1a != null)
				_t1a.Texture = t1Ids.Count > 0 ? LoadSpellTexture(t1Ids[0]) : Transparent1x1;

			if (_t1b != null)
				_t1b.Texture = t1Ids.Count > 1 ? LoadSpellTexture(t1Ids[1]) : Transparent1x1;
		}

		public void SetTier2Icon(string? t2IdOrNull, int size = 64)
		{
			if (_t2 == null) return;
			_t2.Texture = t2IdOrNull != null ? LoadSpellTexture(t2IdOrNull) : Transparent1x1;
		}

		// --- Local texture helpers (no IconLibrary dependency) ---
		private static Texture2D LoadClassTexture(string classId)
		{
			var path = $"res://Content/Icons/Classes/{classId}.png";
			return LoadTextureOrFallback(path);
		}

		private static Texture2D LoadSpellTexture(string spellId)
		{
			var path = $"res://Content/Icons/Spells/{spellId}.png";
			return LoadTextureOrFallback(path);
		}

		private static Texture2D LoadTextureOrFallback(string path)
		{
			var tex = GD.Load<Texture2D>(path);
			if (tex != null) return tex;
			return Transparent1x1;
		}

		private static Texture2D? _transparent;
		private static Texture2D Transparent1x1
		{
			get
			{
				if (_transparent != null) return _transparent;
				var img = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
				var itex = ImageTexture.CreateFromImage(img);
				_transparent = itex;
				return itex;
			}
		}
	}
}
