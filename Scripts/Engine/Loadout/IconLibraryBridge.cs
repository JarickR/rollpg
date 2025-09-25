// File: Scripts/Engine/Loadout/IconLibraryBridge.cs
using System;
using Godot;
using DiceArena.Godot; // IconLibrary

namespace DiceArena.Engine.Loadout
{
	public static class IconLibraryBridge
	{
		public static Texture2D Class(string classId, int size)
		{
			var tex = IconLibrary.GetClassTexture(classId, size);
			return tex ?? Fallback();
		}

		public static Texture2D Spell(string spellId, int tier, int size)
		{
			if (string.IsNullOrWhiteSpace(spellId))
				return Fallback();

			var baseId = CleanBase(spellId);
			var tex = IconLibrary.GetSpellTexture(baseId, tier, size);
			return tex ?? Fallback();
		}

		private static string CleanBase(string id)
		{
			id = StripPlusSuffix(id);
			id = StripTierSuffix(id);
			id = StripTrailingDigitThenDanglingT(id); // <— renamed helper semantics
			return Slug(id);
		}

		private static string StripPlusSuffix(string s)
		{
			if (string.IsNullOrEmpty(s)) return s;
			int i = s.Length - 1;
			while (i >= 0 && s[i] == '+') i--;
			return s.Substring(0, i + 1);
		}

		private static string StripTierSuffix(string s)
		{
			if (s.EndsWith("-1", StringComparison.OrdinalIgnoreCase) ||
				s.EndsWith("-2", StringComparison.OrdinalIgnoreCase) ||
				s.EndsWith("-3", StringComparison.OrdinalIgnoreCase))
				return s[..^2];
			return s;
		}

		private static string StripTrailingDigitThenDanglingT(string s)
		{
			if (string.IsNullOrEmpty(s)) return s;

			bool removedDigit = false;
			if (char.IsDigit(s[^1]))
			{
				s = s[..^1];
				removedDigit = true;
			}

			if (removedDigit && s.Length > 0 && (s[^1] == 't' || s[^1] == 'T'))
				s = s[..^1];

			return s;
		}

		private static string Slug(string s)
		{
			Span<char> buf = stackalloc char[s.Length];
			int j = 0;
			for (int i = 0; i < s.Length; i++)
			{
				char c = char.ToLowerInvariant(s[i]);
				if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
					buf[j++] = c;
			}
			return new string(buf.Slice(0, j));
		}

		private static Texture2D Fallback()
		{
			var tex = IconLibrary.Transparent1x1;
			if (tex != null) return tex;

			var img = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
			img.Fill(new Color(0, 0, 0, 0));
			return ImageTexture.CreateFromImage(img);
		}
	}
}
