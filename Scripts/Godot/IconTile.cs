// Scripts/Godot/IconTile.cs
#nullable enable
using Godot;

namespace DiceArena.Godot
{
	/// <summary>
	/// Small clickable icon button used in the loadout UI.
	/// Stores which pool it belongs to and the selected id so other scripts
	/// (like PlayerLoadoutPanel) can read or assign Pool/Id.
	/// </summary>
	public partial class IconTile : Button
	{
		// Keep the enum NESTED so external code can reference IconTile.IconPool
		public enum IconPool { None = 0, Class = 1, Tier1Spell = 2, Tier2Spell = 3 }

		/// <summary>The logical pool this tile represents (class / tier1 / tier2).</summary>
		public IconPool Pool { get; set; } = IconPool.None;

		/// <summary>The id of the class or spell currently shown on this tile.</summary>
		public string Id { get; set; } = string.Empty;

		/// <summary>Apply an icon to this tile and remember its identity.</summary>
		public void Apply(IconPool pool, string id, string? tooltip = null)
		{
			Pool = pool;
			Id   = id ?? string.Empty;

			Texture2D tex = IconLibrary.Transparent1x1;

			switch (pool)
			{
				case IconPool.Class:
					tex = IconLibrary.GetClassTexture(Id);
					break;

				case IconPool.Tier1Spell:
					tex = IconLibrary.GetSpellTexture(Id, 1);
					break;

				case IconPool.Tier2Spell:
					tex = IconLibrary.GetSpellTexture(Id, 2);
					break;

				default:
					tex = IconLibrary.Transparent1x1;
					break;
			}

			// Button icon in Godot 4
			Icon = tex;

			// Helpful tooltip for hover (falls back to the id if not provided).
			TooltipText = tooltip ?? Id;

			// Make it nice and uniform; Button doesn't scale the icon automatically.
			// This ensures a consistent clickable area that matches your grid cell.
			CustomMinimumSize = new Vector2(64, 64);

			// Make sure the button isn't "latched" visually when reused.
			ButtonPressed = false;
		}

		/// <summary>Clear to a transparent icon and blank identity.</summary>
		public void Clear()
		{
			Pool = IconPool.None;
			Id   = string.Empty;
			Icon = IconLibrary.Transparent1x1;
			TooltipText = string.Empty;
			ButtonPressed = false;
		}
	}
}
