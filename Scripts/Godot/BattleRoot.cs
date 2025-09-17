// Scripts/Godot/BattleRoot.cs
using Godot;
using System;
using System.Linq;
using DiceArena.Engine.Loadout;   // PartyLoadout, MemberLoadout
using DiceArena.Engine.Content;   // ContentBundle, ClassDef, SpellDef

public partial class BattleRoot : Control
{
	[Export] public NodePath? HeroPanelPath { get; set; }   // points at a container to hold hero cards
	[Export] public NodePath? LogBoxPath { get; set; }      // points at a RichTextLabel (optional)

	private Control? _heroPanel;
	private RichTextLabel? _logBox;

	public override void _Ready()
	{
		_heroPanel = GetNodeOrNull<Control>(HeroPanelPath ?? string.Empty);
		_logBox    = GetNodeOrNull<RichTextLabel>(LogBoxPath ?? string.Empty);

		if (_heroPanel == null)
			GD.PushWarning("[BattleRoot] HeroPanelPath not assigned or node missing.");
		if (_logBox == null)
			GD.PushWarning("[BattleRoot] LogBoxPath not assigned or node missing.");
	}

	/// <summary>
	/// Called by Game.cs after the loadout is confirmed.
	/// Builds hero cards and logs what was picked.
	/// </summary>
	public void BeginBattle(PartyLoadout party, ContentBundle bundle)
	{
		if (_heroPanel == null)
		{
			GD.PushWarning("[BattleRoot] BeginBattle called without a HeroPanel.");
			return;
		}

		// Clean panel
		ClearChildren(_heroPanel);

		// Build a simple vertical list of hero “cards”
		var wrapper = new VBoxContainer();
		wrapper.AddThemeConstantOverride("separation", 12);
		_heroPanel.AddChild(wrapper);

		// Iterate each member selected on the loadout screen
		for (int i = 0; i < party.Members.Count; i++)
		{
			var m = party.Members[i];

			// Look up the selected class
			var cls = bundle.Classes.FirstOrDefault(c => c.Id == m.ClassId);
			var className = cls?.Name ?? "(None)";

			// ---- LOG: where we expect the class icon to be
			GD.Print($"[BattleRoot] Class icon -> {IconLibrary.ClassDir}/{className}.png");

			// Card container
			var cardPanel = new PanelContainer();
			var card = new VBoxContainer();
			cardPanel.AddChild(card);
			card.AddThemeConstantOverride("separation", 6);
			wrapper.AddChild(cardPanel);

			// Header row: class icon + title
			var header = new HBoxContainer();
			header.AddThemeConstantOverride("separation", 8);
			card.AddChild(header);

			// Class icon
			var classIcon = new TextureRect
			{
				CustomMinimumSize = new Vector2(48, 48),
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
			};
			// This will also log probe/load because IconLibrary.Verbose = true by default
			classIcon.Texture = (cls != null)
				? IconLibrary.GetClassTexture(className)
				: IconLibrary.Transparent1x1;
			header.AddChild(classIcon);

			// Title
			var title = new Label { Text = $"Member {i + 1} — {className}" };
			title.AddThemeFontSizeOverride("font_size", 16);
			header.AddChild(title);

			// Tier 1 row
			var t1Row = new HBoxContainer();
			t1Row.AddThemeConstantOverride("separation", 8);
			card.AddChild(new Label { Text = "T1:" });
			card.AddChild(t1Row);

			foreach (var spellId in m.ChosenTier1SpellIds)
			{
				var sp = bundle.Spells.FirstOrDefault(s => s.Id == spellId);
				if (sp == null) continue;

				// ---- LOG: what we think the spell icon path should be
				GD.Print($"[BattleRoot] Spell icon -> name='{sp.Name}' tier={sp.Tier}");

				t1Row.AddChild(SpellChip(sp.Name, sp.Tier));
			}

			// Tier 2 row
			card.AddChild(new Label { Text = "T2:" });
			var t2Row = new HBoxContainer();
			t2Row.AddThemeConstantOverride("separation", 8);
			card.AddChild(t2Row);

			if (!string.IsNullOrWhiteSpace(m.ChosenTier2SpellId))
			{
				var sp2 = bundle.Spells.FirstOrDefault(s => s.Id == m.ChosenTier2SpellId);
				if (sp2 != null)
				{
					GD.Print($"[BattleRoot] Spell icon -> name='{sp2.Name}' tier={sp2.Tier}");
					t2Row.AddChild(SpellChip(sp2.Name, sp2.Tier));
				}
			}

			// Log to battle log (optional)
			if (_logBox != null)
			{
				_logBox.AppendText($"Member {i + 1}: Class = {className}\n");
				var t1Names = string.Join(", ",
					m.ChosenTier1SpellIds.Select(id => bundle.Spells.FirstOrDefault(s => s.Id == id)?.Name ?? id));
				_logBox.AppendText($"  T1: {t1Names}\n");

				if (!string.IsNullOrWhiteSpace(m.ChosenTier2SpellId))
				{
					var name2 = bundle.Spells.FirstOrDefault(s => s.Id == m.ChosenTier2SpellId)?.Name
								?? m.ChosenTier2SpellId;
					_logBox.AppendText($"  T2: {name2}\n");
				}
				_logBox.Newline();
			}
		}
	}

	// Creates a small “pill” with an icon + name for a spell
	private Control SpellChip(string displayName, int tier)
	{
		var chip = new HBoxContainer();
		chip.AddThemeConstantOverride("separation", 6);

		var icon = new TextureRect
		{
			CustomMinimumSize = new Vector2(28, 28),
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
		};
		icon.Texture = IconLibrary.GetSpellTexture(displayName, tier);

		var label = new Label { Text = displayName };

		chip.AddChild(icon);
		chip.AddChild(label);
		return chip;
	}

	private static void ClearChildren(Node n)
	{
		for (int i = n.GetChildCount() - 1; i >= 0; i--)
			n.GetChild(i).QueueFree();
	}
}
