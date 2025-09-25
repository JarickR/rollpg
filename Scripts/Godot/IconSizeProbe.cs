using Godot;
using GodotEngine = global::Godot.Engine;

namespace DiceArena.Godot
{
	/// <summary>
	/// Editor-only helper that keeps preview TextureRects small.
	/// Deletes itself at runtime.
	/// </summary>
	[Tool]
	public partial class IconSizeProbe : Control
	{
		[Export] public int PreviewSize { get; set; } = 96;
		[Export] public string ClassPreviewName { get; set; } = "ClassPreview";
		[Export] public string SpellPreviewName { get; set; } = "SpellPreview";

		private TextureRect? _classPrev;
		private TextureRect? _spellPrev;

		public override void _Ready()
		{
			// Use the alias to force the real Godot Engine singleton
			if (!GodotEngine.IsEditorHint())
			{
				QueueFree();
				return;
			}

			Name = "IconSizeProbe (editor-only)";

			_classPrev = GetNodeOrNull<TextureRect>(ClassPreviewName)
						 ?? FindChild(ClassPreviewName) as TextureRect;
			_spellPrev = GetNodeOrNull<TextureRect>(SpellPreviewName)
						 ?? FindChild(SpellPreviewName) as TextureRect;

			if (_classPrev == null)
			{
				_classPrev = MakePreviewRect(ClassPreviewName);
				AddChild(_classPrev);
			}

			if (_spellPrev == null)
			{
				_spellPrev = MakePreviewRect(SpellPreviewName);
				AddChild(_spellPrev);
			}

			ApplyPreviewSize();
		}

		public override void _Process(double delta)
		{
			if (!GodotEngine.IsEditorHint())
				return;

			ApplyPreviewSize();
		}

		private TextureRect MakePreviewRect(string name)
		{
			var r = new TextureRect
			{
				Name = name,
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				// In Godot 4 there's no SizeFlags.None — set to 0 for “no flags”
				SizeFlagsHorizontal = 0,
				SizeFlagsVertical   = 0,
				MouseFilter = MouseFilterEnum.Ignore
			};

			var v = new Vector2(PreviewSize, PreviewSize);
			r.CustomMinimumSize = v;
			r.Size = v;
			r.Position = Vector2.Zero;

			return r;
		}

		private void ApplyPreviewSize()
		{
			var v = new Vector2(PreviewSize, PreviewSize);

			if (_classPrev != null)
			{
				_classPrev.CustomMinimumSize = v;
				_classPrev.Size = v;
				_classPrev.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
				_classPrev.SizeFlagsHorizontal = 0;
				_classPrev.SizeFlagsVertical   = 0;
			}

			if (_spellPrev != null)
			{
				_spellPrev.CustomMinimumSize = v;
				_spellPrev.Size = v;
				_spellPrev.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
				_spellPrev.SizeFlagsHorizontal = 0;
				_spellPrev.SizeFlagsVertical   = 0;
			}

			CustomMinimumSize = Vector2.Zero;
			SizeFlagsHorizontal = 0;
			SizeFlagsVertical   = 0;
		}
	}
}
