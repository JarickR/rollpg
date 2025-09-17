// Scripts/Engine/Loadout/Logging.cs
using Godot;

namespace DiceArena.Engine.Loadout
{
	/// <summary>
	/// Minimal logger for the loadout/UI systems.
	/// V = verbose (debug-only), I = info, W = warning, E = error.
	/// Also includes domain tags used elsewhere: Damage, System.
	/// </summary>
	public static class Log
	{
#if DEBUG
		public static bool VerboseEnabled = true;
#else
		public static bool VerboseEnabled = false;
#endif

		// ---- Core ----
		public static void V(string msg)
		{
#if DEBUG
			if (VerboseEnabled) GD.Print($"[Loadout][V] {msg}");
#endif
		}

		public static void I(string msg) => GD.Print($"[Loadout] {msg}");
		public static void W(string msg) => GD.PushWarning($"[Loadout][WARN] {msg}");
		public static void E(string msg) => GD.PushError($"[Loadout][ERR] {msg}");

		// ---- Domain tags (used by EnemyPanel and others) ----
		public static void Damage(string msg) => GD.Print($"[Loadout][DMG] {msg}");
		public static void System(string msg) => GD.Print($"[Loadout][SYS] {msg}");

		// (Optional convenience) formatted variants if you ever need them:
		public static void Vf(string fmt, params object[] args)
		{
#if DEBUG
			if (VerboseEnabled) GD.Print($"[Loadout][V] {string.Format(fmt, args)}");
#endif
		}
		public static void If(string fmt, params object[] args) => GD.Print($"[Loadout] {string.Format(fmt, args)}");
		public static void Wf(string fmt, params object[] args) => GD.PushWarning($"[Loadout][WARN] {string.Format(fmt, args)}");
		public static void Ef(string fmt, params object[] args) => GD.PushError($"[Loadout][ERR] {string.Format(fmt, args)}");
		public static void Damagef(string fmt, params object[] args) => GD.Print($"[Loadout][DMG] {string.Format(fmt, args)}");
		public static void Systemf(string fmt, params object[] args) => GD.Print($"[Loadout][SYS] {string.Format(fmt, args)}");
	}
}
