public partial class ClickProbe : SubViewportContainer
{
	public override void _GuiInput(InputEvent e)
	{
		if (e is InputEventMouseButton mb && mb.Pressed)
			GD.Print($"[ClickProbe] {mb.ButtonIndex} at {mb.Position}");
	}
}
