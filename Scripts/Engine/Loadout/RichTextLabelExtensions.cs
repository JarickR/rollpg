using Godot;

namespace DiceArena.Engine.Loadout
{
	/// <summary>
	/// Compatibility shim so we can call RichTextLabel.PushParagraph() with no args.
	/// Targets Godot 4.4 C# signature:
	/// PushParagraph(HorizontalAlignment, Control.TextDirection, StringName, int)
	/// </summary>
	public static class RichTextLabelExtensions
	{
		public static void PushParagraph(this RichTextLabel rtl)
		{
			rtl.PushParagraph(
				HorizontalAlignment.Left,
				Control.TextDirection.Auto,
				default(StringName),
				0
			);
		}
	}
}
