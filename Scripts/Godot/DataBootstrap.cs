using Godot;
using RollPG.Content;

public partial class DataBootstrap : Node
{
	public override void _Ready()
	{
		// Load all JSON content once when the scene starts.
		ContentDB.Init();
		GD.Print("[DataBootstrap] Content ready.");
	}
}
