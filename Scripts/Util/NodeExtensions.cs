// Scripts/Engine/NodeExtensions.cs
using Godot;

namespace DiceArena.Engine.Loadout
{
	/// <summary>
	/// Handy scene-graph helpers. The key one here is ApplyNearestTextureFilter,
	/// which force-sets NEAREST sampling on all TextureRects under a node.
	/// Use it after you build your UI to guarantee crisp pixel art regardless of import settings.
	/// </summary>
	public static class NodeExtensions
	{
		/// <summary>
		/// Recursively applies NEAREST filtering to every TextureRect under <paramref name="root"/>.
		/// Also ensures a reasonable icon minimum size (optional) and a safe stretch mode.
		/// </summary>
		/// <param name="root">Node whose subtree will be processed.</param>
		/// <param name="minIconSize">
		/// Optional minimum size to assign to TextureRects that have no CustomMinimumSize.
		/// Pass Vector2.Zero to skip touching sizes. Default is (56,56) to match your loadout tiles.
		/// </param>
		/// <param name="stretchMode">
		/// Stretch mode to enforce; default is KeepAspectCentered which avoids any scaling skew.
		/// </param>
		public static void ApplyNearestTextureFilter(
			this Node root,
			Vector2? minIconSize = null,
			TextureRect.StretchModeEnum stretchMode = TextureRect.StretchModeEnum.KeepAspectCentered)
		{
			var size = minIconSize ?? new Vector2(56, 56);
			ApplyNearestTextureFilterInternal(root, size, stretchMode);
		}

		/// <summary>
		/// Non-extension wrapper if you prefer static invocation.
		/// </summary>
		public static void ApplyNearestTextureFilter(Node root)
			=> ApplyNearestTextureFilterInternal(root, new Vector2(56, 56), TextureRect.StretchModeEnum.KeepAspectCentered);

		// ---- Implementation -------------------------------------------------

		private static void ApplyNearestTextureFilterInternal(
			Node node,
			Vector2 minSize,
			TextureRect.StretchModeEnum stretchMode)
		{
			// Process this node
			if (node is TextureRect tr)
			{
				tr.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;

				// Only set a min size if none exists; avoids overriding custom layouts.
				if (tr.CustomMinimumSize == Vector2.Zero && minSize != Vector2.Zero)
					tr.CustomMinimumSize = minSize;

				// Ensure we don't stretch in a way that blurs.
				tr.StretchMode = stretchMode;
			}

			// Recurse children
			var children = node.GetChildren();
			for (int i = 0; i < children.Count; i++)
			{
				if (children[i] is Node child && GodotObject.IsInstanceValid(child))
					ApplyNearestTextureFilterInternal(child, minSize, stretchMode);
			}
		}

		// --------------------------------------------------------------------
		// Legacy name kept for convenience if you referenced the earlier snippet.
		// --------------------------------------------------------------------
		public static void ForceNearestOnAllIcons(this Node root)
			=> ApplyNearestTextureFilter(root);
	}
}
