using Godot;

namespace DiceArena.GodotUI
{
	public static class Log
	{
		private static System.Action<string, Color?>? _sink;

		public static void Attach(BattleLogPanel panel)
		{
			_sink = panel.AppendLine;
		}

		public static void Detach() => _sink = null;

		public static void Info(string msg)   => _sink?.Invoke(msg, null);
		public static void Damage(string msg) => _sink?.Invoke(msg, Colors.IndianRed);
		public static void Heal(string msg)   => _sink?.Invoke(msg, Colors.SeaGreen);
		public static void System(string msg) => _sink?.Invoke(msg, Colors.SlateGray);
	}
}
