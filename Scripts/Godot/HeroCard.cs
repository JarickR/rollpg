// res://Scripts/Godot/HeroCard.cs
#nullable enable
using Godot;
using DiceArena.Engine;
using System.Text;

public partial class HeroCard : Panel
{
	public Hero Data { get; private set; } = default!;

	private VBoxContainer _root = default!;
	private Label _title = default!;
	private HBoxContainer _statsRow = default!;
	private Label _hp = default!;
	private Label _armor = default!;
	private VBoxContainer _facesBox = default!;
	private RichTextLabel _facesList = default!;

	public override void _Ready()
	{
		CustomMinimumSize = new Vector2(220, 140);
		var style = new StyleBoxFlat
		{
			BgColor = new Color(0.12f, 0.14f, 0.18f),
			CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
			CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
			ContentMarginLeft = 10, ContentMarginRight = 10, ContentMarginTop = 10, ContentMarginBottom = 10
		};
		AddThemeStyleboxOverride("panel", style);

		_root = new VBoxContainer(); _root.AddThemeConstantOverride("separation", 6);
		AddChild(_root);

		_title = new Label { Text = "Hero", HorizontalAlignment = HorizontalAlignment.Left };
		_title.AddThemeFontSizeOverride("font_size", 16);
		_title.AddThemeColorOverride("font_color", Colors.White);
		_root.AddChild(_title);

		_statsRow = new HBoxContainer(); _statsRow.AddThemeConstantOverride("separation", 12);
		_root.AddChild(_statsRow);

		_hp = new Label { Text = "HP 0/0" }; _hp.AddThemeFontSizeOverride("font_size", 14);
		_statsRow.AddChild(_hp);

		_armor = new Label { Text = "AR 0" }; _armor.AddThemeFontSizeOverride("font_size", 14);
		_statsRow.AddChild(_armor);

		_facesBox = new VBoxContainer(); _facesBox.AddThemeConstantOverride("separation", 4);
		_root.AddChild(_facesBox);

		var facesHeader = new Label { Text = "Faces", HorizontalAlignment = HorizontalAlignment.Left };
		facesHeader.AddThemeFontSizeOverride("font_size", 14);
		facesHeader.AddThemeColorOverride("font_color", new Color("FFD700"));
		_facesBox.AddChild(facesHeader);

		_facesList = new RichTextLabel { BbcodeEnabled = true, FitContent = true, ScrollActive = false };
		_facesBox.AddChild(_facesList);
	}

	public void Bind(Hero hero)
	{
		Data = hero;
		Refresh();
	}

	public void Refresh()
	{
		if (Data == null) return;
		_title.Text = $"{Data.Name} [{Data.ClassId}]";
		_hp.Text = $"HP {Data.Hp}/{Data.MaxHp}";
		_armor.Text = $"AR {Data.Armor}" + (Data.SpikedThorns > 0 ? $"  (Thorns {Data.SpikedThorns})" : "");

		// Faces list (2x2 conceptual; we render as 4 bullets)
		var sb = new StringBuilder();
		for (int i = 0; i < Data.Loadout.Count; i++)
		{
			var s = Data.Loadout[i];
			sb.Append($"• {s?.Name ?? "?"}\n");
		}
		_facesList.Text = sb.ToString().TrimEnd();
	}
}
