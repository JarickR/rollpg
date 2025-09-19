using System.Collections.Generic;
using Godot;
using DiceArena.Engine.Content; // ClassDef, ContentBundle
using DiceArena.Godot; // <-- add this line


public partial class HeroCard : Control
{
	private VBoxContainer _root = default!;
	private HBoxContainer _topRow = default!;
	private HBoxContainer _t1Row = default!;
	private HBoxContainer _t2Row = default!;
	private Label _title = default!;

	public override void _Ready()
	{
		Name = "HeroCard";
		CustomMinimumSize = new Vector2(280, 120);

		_root = new VBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical   = SizeFlags.ShrinkCenter,
		};
		_root.AddThemeConstantOverride("separation", 6);
		AddChild(_root);

		// Top (class icon + title)
		_topRow = new HBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical   = SizeFlags.ShrinkCenter,
		};
		_topRow.AddThemeConstantOverride("separation", 8);
		_root.AddChild(_topRow);

		_title = new Label
		{
			Text = "Member — Class",
			ThemeTypeVariation = "Bold",
			VerticalAlignment = VerticalAlignment.Center
		};

		// Tier 1 row
		_t1Row = new HBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical   = SizeFlags.ShrinkCenter,
		};
		_t1Row.AddThemeConstantOverride("separation", 8);

		// Tier 2 row
		_t2Row = new HBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical   = SizeFlags.ShrinkCenter,
		};
		_t2Row.AddThemeConstantOverride("separation", 8);

		// Labels for rows
		var t1Label = new Label { Text = "T1:", VerticalAlignment = VerticalAlignment.Center };
		var t2Label = new Label { Text = "T2:", VerticalAlignment = VerticalAlignment.Center };

		var t1Wrap = new HBoxContainer();
		t1Wrap.AddThemeConstantOverride("separation", 6);
		t1Wrap.AddChild(t1Label);
		t1Wrap.AddChild(_t1Row);

		var t2Wrap = new HBoxContainer();
		t2Wrap.AddThemeConstantOverride("separation", 6);
		t2Wrap.AddChild(t2Label);
		t2Wrap.AddChild(_t2Row);

		_root.AddChild(t1Wrap);
		_root.AddChild(t2Wrap);
	}

	/// <summary>
	/// Build the card contents.
	/// </summary>
	/// <param name="cls">Class definition (Name used for title and class icon)</param>
	/// <param name="t1">Tier 1 spell names</param>
	/// <param name="t2">Tier 2 spell name (nullable)</param>
	/// <param name="bundle">Content bundle (kept for signature compatibility)</param>
	public void Setup(ClassDef cls, IEnumerable<string> t1, string? t2, ContentBundle bundle)
	{
		// Clear dynamic rows (keep container scaffolding)
		ClearChildren(_topRow);
		ClearChildren(_t1Row);
		ClearChildren(_t2Row);

		// Class icon
		Texture2D? classTex = IconLibrary.GetClassTexture(cls.Name);
		var classIcon = MakeIconOrFallback(classTex, "", new Color(0.82f, 0.75f, 0.0f)); // golden bg

		// Title
		_title.Text = $"Member — {cls.Name}";

		_topRow.AddChild(classIcon);
		_topRow.AddChild(_title);

		// T1 spells
		foreach (var s in t1)
		{
			var tex = IconLibrary.GetSpellTexture(s, 1);
			_t1Row.AddChild(MakeIconOrFallback(tex, s, Colors.DarkSeaGreen));
		}

		// T2 spell (single, optional)
		if (!string.IsNullOrWhiteSpace(t2))
		{
			var tex2 = IconLibrary.GetSpellTexture(t2!, 2);
			_t2Row.AddChild(MakeIconOrFallback(tex2, t2!, Colors.SlateBlue));
		}
	}

	// ---------- helpers ----------

	private static void ClearChildren(Node n)
	{
		for (int i = n.GetChildCount() - 1; i >= 0; i--)
			n.GetChild(i).QueueFree();
	}

	/// <summary>
	/// Builds an icon+caption pill. Accepts null texture and falls back to a transparent 1x1.
	/// </summary>
	private static Control MakeIconOrFallback(Texture2D? tex, string caption, Color bg)
	{
		var panel = new PanelContainer
		{
			SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
			SizeFlagsVertical   = SizeFlags.ShrinkCenter,
			CustomMinimumSize   = new Vector2(88, 28),
		};

		var style = new StyleBoxFlat
		{
			BgColor = bg,
			CornerRadiusTopLeft = 4,
			CornerRadiusTopRight = 4,
			CornerRadiusBottomLeft = 4,
			CornerRadiusBottomRight = 4
		};
		panel.AddThemeStyleboxOverride("panel", style);

		var row = new HBoxContainer();
		row.AddThemeConstantOverride("separation", 6);
		panel.AddChild(row);

		var icon = new TextureRect
		{
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			CustomMinimumSize = new Vector2(22, 22),
			Texture = tex ?? IconLibrary.Transparent1x1
		};
		row.AddChild(icon);

		if (!string.IsNullOrWhiteSpace(caption))
		{
			var label = new Label
			{
				Text = caption,
				VerticalAlignment = VerticalAlignment.Center
			};
			row.AddChild(label);
		}

		return panel;
	}
}
