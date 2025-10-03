using Godot;

public partial class FinalTest : Control
{
	public override void _Ready()
	{
		GD.Print("--- Starting Final Test ---");
		var grid = GetNode<GridContainer>("MyGrid");

		if (grid == null)
		{
			GD.PushError("Final Test FAILED: Could not find TestGrid.");
			return;
		}

		var highlightStyle = new StyleBoxFlat
		{
			BgColor = new Color(Colors.Gold, 0.2f),
			BorderColor = Colors.Gold,
			BorderWidthTop = 4, BorderWidthBottom = 4, BorderWidthLeft = 4, BorderWidthRight = 4
		};
		
		for (int i = 0; i < 4; i++)
		{
			var button = new Button
			{
				Text = $"Button {i + 1}",
				CustomMinimumSize = new Vector2(150, 150)
			};

			var highlight = new Panel
			{
				MouseFilter = MouseFilterEnum.Ignore
			};
			// --- THIS IS THE FIX ---
			highlight.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			
			highlight.AddThemeStyleboxOverride("panel", highlightStyle);
			
			button.AddChild(highlight);
			grid.AddChild(button);
		}
		
		GD.Print("--- Final Test Complete ---");
	}
}
