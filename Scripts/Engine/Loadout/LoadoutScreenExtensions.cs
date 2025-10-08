#nullable enable
using Godot;

namespace DiceArena.Engine.Loadout
{
	/// <summary>
	/// Legacy extension methods for LoadoutScreen.
	/// We keep these as no-ops so older references compile cleanly.
	/// </summary>
	public static class LoadoutScreenExtensions
	{
		/// <summary>
		/// No-op shim. Your LoadoutScreen now has its own PopulateMemberPanels() instance method.
		/// If any old code still calls the extension, this version prevents compile errors.
		/// </summary>
		public static void PopulateMemberPanels(this LoadoutScreen _)
		{
			// Intentionally left blank.
			// Use LoadoutScreen.PopulateMemberPanels() instance method instead.
		}

		/// <summary>
		/// (Optional) Legacy hook to “rebuild” panels that may exist in old code paths.
		/// Safe no-op to satisfy references without affecting runtime behavior.
		/// </summary>
		public static void RebuildPanelsLegacy(this LoadoutScreen _)
		{
			// No-op.
		}
	}
}
