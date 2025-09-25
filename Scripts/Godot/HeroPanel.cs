// File: Scripts/Godot/HeroPanel.cs
using DiceArena.Engine.Content;
using G = global::Godot;

namespace DiceArena.Godot
{
	// Attach this to a Control node in your Hero scene if you use it
	public partial class HeroPanel : G.Control
	{
		// Paths to your data files (you can tweak these in the editor)
		[G.Export] public string ClassesPath { get; set; } = "res://Data/classes.json";
		[G.Export] public string SpellsPath  { get; set; } = "res://Data/spells.json";

		private ContentBundle _bundle = null!; // set in _Ready()

		public override void _Ready()
		{
			// Load everything in here; don't try to 'new ContentBundle()' directly.
			_bundle = ContentDatabase.LoadAll(ClassesPath, SpellsPath);

			// If you later need to use the content here, you can:
			// var classes = _bundle.Classes;
			// var spells  = _bundle.Spells;
			//
			// Build UI, populate lists, etc...
		}
	}
}
