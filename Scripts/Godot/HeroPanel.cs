// Scripts/Godot/HeroPanel.cs
using Godot;
using System.Linq;
using DiceArena.Engine.Content;

public partial class HeroPanel : Control
{
	private ContentBundle _bundle = default!; // set via SetBundle or fallback in _Ready()

	[Export] private Label _classNameLabel = default!;
	[Export] private VBoxContainer _spellsList = default!;

	public void SetBundle(ContentBundle bundle) => _bundle = bundle;

	public override void _Ready()
	{
		if (_bundle == null)
		{
			GD.PushWarning("[HeroPanel] No bundle injected; loading fallback from res://Content");
			_bundle = ContentDatabase.LoadFromFolder("res://Content");
		}

		// If exported nodes weren’t wired in a scene, create simple fallbacks so it won’t crash.
		if (_classNameLabel == null || _spellsList == null)
		{
			foreach (var c in GetChildren()) c.QueueFree();

			var root = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
			AddChild(root);

			_classNameLabel = new Label { Text = "Class", HorizontalAlignment = HorizontalAlignment.Left };
			root.AddChild(_classNameLabel);

			root.AddChild(new HSeparator());

			_spellsList = new VBoxContainer();
			root.AddChild(_spellsList);
		}
	}

	public void ShowHero(string classId, string[] spellIds)
	{
		if (_bundle == null) return;

		var cls = _bundle.Classes.FirstOrDefault(c => c.Id == classId);
		_classNameLabel.Text = cls?.Name ?? classId;

		foreach (var c in _spellsList.GetChildren())
			c.QueueFree();

		foreach (var id in spellIds)
		{
			var sp = _bundle.Spells.FirstOrDefault(s => s.Id == id);
			var lbl = new Label { Text = sp?.Name ?? id };
			_spellsList.AddChild(lbl);
		}
	}
}
