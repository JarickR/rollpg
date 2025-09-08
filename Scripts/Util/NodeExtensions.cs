// res://Scripts/Util/NodeExtensions.cs
#nullable enable
using Godot;
using System.Linq;

namespace DiceArena.Engine
{
	public static class NodeExtensions
	{
		/// <summary>
		/// Queues all direct children of this node for deletion at idle time.
		/// Safe to call during _Ready/_Process; uses QueueFree on each child.
		/// </summary>
		public static void QueueFreeChildren(this Node node)
		{
			// Godot 4: GetChildren() returns Godot.Collections.Array
			var kids = node.GetChildren().OfType<Node>().ToArray();
			for (int i = 0; i < kids.Length; i++)
			{
				kids[i].QueueFree();
			}
		}
	}
}
