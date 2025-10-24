// res://Scripts/Godot/Debug/ListAllTextEdits.cs
#nullable enable
using Godot;

public partial class ListAllTextEdits : Node
{
	public override void _Ready()
	{
		if (GetTree().Root != null)
			PrintTextEdits(GetTree().Root);
	}

	private void PrintTextEdits(Node n)
	{
		if (n is TextEdit te)
			GD.Print($"[TextEdit] {te.GetPath()} Editable={te.Editable} Lines={te.GetLineCount()}");

		foreach (var c in n.GetChildren())
			PrintTextEdits(c);
	}
}
