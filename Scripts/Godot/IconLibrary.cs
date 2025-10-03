using Godot;
using System.Collections.Generic;

/// <summary>
/// Global IconLibrary so legacy code that does `IconLibrary.*` compiles.
/// Also exposes overloads with extra arguments to match older call sites.
/// Looks for:
///   res://Content/Icons/Classes/{classId}.png
///   res://Content/Icons/Spells/{spellId}.png
/// </summary>
public static class IconLibrary
{
	private static readonly Dictionary<string, Texture2D> _cache = new();
	private const string ClassesDir = "res://Content/Icons/Classes";
	private const string SpellsDir  = "res://Content/Icons/Spells";

	// -------- Transparent 1x1 ----------
	private static Texture2D? _transparent;
	public static Texture2D Transparent1x1
	{
		get
		{
			if (_transparent == null)
			{
				var img = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
				img.Fill(Colors.Transparent);
				_transparent = ImageTexture.CreateFromImage(img);
			}
			return _transparent!;
		}
	}

	// -------- Class textures -----------
	public static Texture2D GetClassTexture(string classId)
	{
		if (string.IsNullOrWhiteSpace(classId)) return Transparent1x1;
		var path = $"{ClassesDir}/{classId}.png";
		return LoadTextureOrTransparent(path);
	}

	// Back-compat overloads (ignored extra args)
	public static Texture2D GetClassTexture(string classId, object _ignored)
		=> GetClassTexture(classId);

	// -------- Spell textures -----------
	public static Texture2D GetSpellTexture(string spellId)
	{
		if (string.IsNullOrWhiteSpace(spellId)) return Transparent1x1;
		var path = $"{SpellsDir}/{spellId}.png";
		return LoadTextureOrTransparent(path);
	}

	// Back-compat overloads (ignored extra args)
	public static Texture2D GetSpellTexture(string spellId, object _a, object _b)
		=> GetSpellTexture(spellId);

	// -------- Helpers ------------------
	private static Texture2D LoadTextureOrTransparent(string path)
	{
		if (_cache.TryGetValue(path, out var tex) && tex != null)
			return tex;

		var loaded = GD.Load<Texture2D>(path);
		if (loaded != null)
		{
			_cache[path] = loaded;
			return loaded;
		}

		GD.PushWarning($"[IconLibrary] Missing icon: {path}");
		return Transparent1x1;
	}
}

/// <summary>
/// Namespaced facade so files that refer to DiceArena.Godot.IconLibrary also work.
/// </summary>
namespace DiceArena.Godot
{
	public static class IconLibrary
	{
		public static Texture2D Transparent1x1 => global::IconLibrary.Transparent1x1;
		public static Texture2D GetClassTexture(string classId) => global::IconLibrary.GetClassTexture(classId);
		public static Texture2D GetClassTexture(string classId, object a) => global::IconLibrary.GetClassTexture(classId, a);
		public static Texture2D GetSpellTexture(string spellId) => global::IconLibrary.GetSpellTexture(spellId);
		public static Texture2D GetSpellTexture(string spellId, object a, object b) => global::IconLibrary.GetSpellTexture(spellId, a, b);
	}
}
