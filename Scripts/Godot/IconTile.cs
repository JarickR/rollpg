using Godot;

namespace DiceArena.Godot
{
	/// <summary>
	/// Small wrapper for the icon buttons we place in the panel.
	/// Gives us Id/Pool and a reliable way to read the displayed texture.
	/// </summary>
	public partial class IconTile : Button
	{
		public enum Pool { Class, Tier1, Tier2 }

		/// <summary> Logical identifier for this tile (usually node name / spell id). </summary>
		public string Id { get; set; } = "";

		/// <summary> Which group (class / tier1 / tier2) this tile belongs to. </summary>
		public Pool BelongsTo { get; set; } = Pool.Class;

		/// <summary>
		/// Texture currently shown for this tile:
		/// - Prefer the Button's Icon when present
		/// - Fallback to a child TextureRect named "TextureRect", if it exists
		/// </summary>
		public Texture2D? IconTexture
		{
			get
			{
				if (Icon is Texture2D t) return t;
				var tr = GetNodeOrNull<TextureRect>("TextureRect");
				return tr?.Texture as Texture2D;
			}
		}
	}
}
