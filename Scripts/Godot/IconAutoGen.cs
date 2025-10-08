using Godot;
using System.Collections.Generic;

namespace DiceArena.Godot
{
	public static class IconAutoGen
	{
		private static readonly Dictionary<string, Texture2D> Cache = new();
		private const int Size = 64;

		public static Texture2D Get(string id, string category)
		{
			string key = $"{category}:{id}";
			if (Cache.TryGetValue(key, out var tex))
				return tex;

			tex = Generate(id, category);
			Cache[key] = tex;
			return tex;
		}

		private static Texture2D Generate(string id, string category)
		{
			Color bg = category switch
			{
				"class" => new Color(0.8f, 0.7f, 0.2f),
				"tier1" => new Color(0.1f, 0.2f, 0.6f),
				"tier2" => new Color(0.1f, 0.6f, 0.3f),
				_ => new Color(0.5f, 0.5f, 0.5f)
			};

			Image img = Image.CreateEmpty(Size, Size, false, Image.Format.Rgba8);
			img.Fill(bg);

			// Simple "symbol" for visibility.
			for (int x = 8; x < Size - 8; x++)
				img.SetPixel(x, x, Colors.White);
			for (int y = 8; y < Size - 8; y++)
				img.SetPixel(Size - y - 1, y, Colors.White);

			return ImageTexture.CreateFromImage(img);
		}
	}
}
