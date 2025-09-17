// Scripts/Engine/Loadout/LoadoutMemberCard.cs
using Godot;
using System.Collections.Generic;
using System.Linq;
using DiceArena.Engine.Content;

namespace DiceArena.Engine.Loadout
{
	/// <summary>
	/// One party member card with:
	///  - Class grid (single-select)
	///  - Tier 1 spells row (max 2)
	///  - Tier 2 spells row (single-select)
	/// Icons from IconLibrary (class by id, spell by id+tier).
	/// Captions fallback: Name ?? Id ?? "?"
	/// Emits SelectionChanged when a selection toggles.
	/// </summary>
	public partial class LoadoutMemberCard : PanelContainer
	{
		[Signal] public delegate void SelectionChangedEventHandler(int memberIndex, string? classId, string[] tier1Ids, string? tier2Id);

		public int MemberIndex { get; private set; } = -1;

		// Theme/layout knobs
		private static readonly Vector2 CardMinSize = new Vector2(440, 360);
		private const int ClassColumns = 5;
		private static readonly Vector2 IconSize = new Vector2(56, 56);
		private const float Spacing = 8f;
		private const float SectionGap = 12f;
		private const float CaptionFontSize = 13f;
		private const float SectionTitleFontSize = 16f;

		// Built nodes
		private VBoxContainer? _root;
		private GridContainer? _classGrid;
		private HBoxContainer? _tier1Row;
		private HBoxContainer? _tier2Row;

		// Button lookup
		private readonly Dictionary<string, BaseButton> _classBtns = new();
		private readonly Dictionary<string, BaseButton> _tier1Btns = new();
		private readonly Dictionary<string, BaseButton> _tier2Btns = new();

		// Current selection
		public string? SelectedClassId { get; private set; }
		public HashSet<string> SelectedTier1Ids { get; } = new();
		public string? SelectedTier2Id { get; private set; }

		public void Build(
			int memberIndex,
			IReadOnlyList<ClassDef> classes,
			IReadOnlyList<SpellDef> spellsTier1,
			IReadOnlyList<SpellDef> spellsTier2)
		{
			MemberIndex = memberIndex;

			// Clear old children safely
			for (int i = GetChildCount() - 1; i >= 0; i--)
			{
				var c = GetChild(i);
				if (GodotObject.IsInstanceValid(c))
					RemoveChild(c);
				c.QueueFree();
			}

			// Reset selections & lookups
			SelectedClassId = null;
			SelectedTier1Ids.Clear();
			SelectedTier2Id = null;
			_classBtns.Clear();
			_tier1Btns.Clear();
			_tier2Btns.Clear();

			// Panel styling for separation
			CustomMinimumSize = CardMinSize;
			var sb = new StyleBoxFlat
			{
				BgColor = new Color(0.12f, 0.12f, 0.12f, 1f),
				CornerRadiusTopLeft = 12,
				CornerRadiusTopRight = 12,
				CornerRadiusBottomLeft = 12,
				CornerRadiusBottomRight = 12,
				BorderWidthLeft = 1,
				BorderWidthTop = 1,
				BorderWidthRight = 1,
				BorderWidthBottom = 1,
				BorderColor = new Color(0.25f, 0.25f, 0.25f, 1f)
			};
			AddThemeStyleboxOverride("panel", sb);
			AddThemeConstantOverride("margin_left", 10);
			AddThemeConstantOverride("margin_right", 10);
			AddThemeConstantOverride("margin_top", 10);
			AddThemeConstantOverride("margin_bottom", 10);

			_root = new VBoxContainer
			{
				Name = $"MemberCard_{memberIndex}",
				CustomMinimumSize = CardMinSize
			};
			_root.AddThemeConstantOverride("separation", (int)SectionGap);
			AddChild(_root);

			// Header
			_root.AddChild(MakeSectionLabel($"Member {memberIndex + 1}", SectionTitleFontSize, true));

			// Class grid section
			_root.AddChild(MakeSectionLabel("Class", SectionTitleFontSize, false));
			_classGrid = new GridContainer
			{
				Columns = ClassColumns,
				CustomMinimumSize = new Vector2(CardMinSize.X, IconSize.Y + 24f)
			};
			_classGrid.AddThemeConstantOverride("v_separation", (int)Spacing);
			_classGrid.AddThemeConstantOverride("h_separation", (int)Spacing);

			foreach (var cls in classes)
			{
				var id = cls.Id;
				var (tile, btn) = MakeIconToggleWithCaption(
					texture: IconLibrary.GetClassTexture(id),
					caption: SafeName(cls.Name, cls.Id),
					tooltip: SafeName(cls.Name, cls.Id));
				_classBtns[id] = btn;
				btn.ToggleMode = true;
				btn.Toggled += (bool pressed) => OnClassToggled(id, pressed);
				_classGrid.AddChild(tile);
			}
			_root.AddChild(_classGrid);

			// Tier 1 spells (2 picks) with horizontal scroll if long
			_root.AddChild(MakeSectionLabel("Tier 1 Spells (pick 2)", SectionTitleFontSize, false));
			var t1Scroll = new ScrollContainer
			{
				HorizontalScrollMode = ScrollContainer.ScrollMode.Auto,
				VerticalScrollMode = ScrollContainer.ScrollMode.Disabled,
				CustomMinimumSize = new Vector2(CardMinSize.X, IconSize.Y + 28f)
			};
			_tier1Row = new HBoxContainer();
			_tier1Row.AddThemeConstantOverride("separation", (int)Spacing);
			t1Scroll.AddChild(_tier1Row);
			foreach (var sp in spellsTier1)
			{
				var id = sp.Id;
				var (tile, btn) = MakeIconToggleWithCaption(
					texture: IconLibrary.GetSpellTexture(id, 1),
					caption: SafeName(sp.Name, sp.Id),
					tooltip: MakeSpellTooltip(sp));
				_tier1Btns[id] = btn;
				btn.ToggleMode = true;
				btn.Toggled += (bool pressed) => OnTier1Toggled(id, pressed);
				_tier1Row.AddChild(tile);
			}
			_root.AddChild(t1Scroll);

			// Tier 2 spells (1 pick)
			_root.AddChild(MakeSectionLabel("Tier 2 Spells (pick 1)", SectionTitleFontSize, false));
			var t2Scroll = new ScrollContainer
			{
				HorizontalScrollMode = ScrollContainer.ScrollMode.Auto,
				VerticalScrollMode = ScrollContainer.ScrollMode.Disabled,
				CustomMinimumSize = new Vector2(CardMinSize.X, IconSize.Y + 28f)
			};
			_tier2Row = new HBoxContainer();
			_tier2Row.AddThemeConstantOverride("separation", (int)Spacing);
			t2Scroll.AddChild(_tier2Row);
			foreach (var sp in spellsTier2)
			{
				var id = sp.Id;
				var (tile, btn) = MakeIconToggleWithCaption(
					texture: IconLibrary.GetSpellTexture(id, 2),
					caption: SafeName(sp.Name, sp.Id),
					tooltip: MakeSpellTooltip(sp));
				_tier2Btns[id] = btn;
				btn.ToggleMode = true;
				btn.Toggled += (bool pressed) => OnTier2Toggled(id, pressed);
				_tier2Row.AddChild(tile);
			}
			_root.AddChild(t2Scroll);

			// Post-build nudge to prevent collapse and trigger reflow
			SetDeferred("custom_minimum_size", CardMinSize);
			_root.SetDeferred("custom_minimum_size", CardMinSize);
			QueueRedraw();

			Log.V($"Built member card {MemberIndex + 1}: classes={classes.Count}, t1={spellsTier1.Count}, t2={spellsTier2.Count}");
		}

		// ---- Selection handlers ----

		private void OnClassToggled(string id, bool pressed)
		{
			if (pressed)
			{
				// single-select: unpress others
				foreach (var kv in _classBtns)
					if (kv.Key != id) kv.Value.SetPressedNoSignal(false);
				SelectedClassId = id;
			}
			else
			{
				// allow deselect to "none"
				if (SelectedClassId == id) SelectedClassId = null;
			}
			EmitSelection();
		}

		private void OnTier1Toggled(string id, bool pressed)
		{
			if (pressed)
			{
				if (SelectedTier1Ids.Count >= 2)
				{
					// reject third pick
					if (_tier1Btns.TryGetValue(id, out var b))
						b.SetPressedNoSignal(false);
					Log.W($"Member {MemberIndex + 1}: Tier1 limit reached (2).");
				}
				else
				{
					SelectedTier1Ids.Add(id);
				}
			}
			else
			{
				SelectedTier1Ids.Remove(id);
			}
			EmitSelection();
		}

		private void OnTier2Toggled(string id, bool pressed)
		{
			if (pressed)
			{
				// single-select: unpress others
				foreach (var kv in _tier2Btns)
					if (kv.Key != id) kv.Value.SetPressedNoSignal(false);
				SelectedTier2Id = id;
			}
			else
			{
				if (SelectedTier2Id == id) SelectedTier2Id = null;
			}
			EmitSelection();
		}

		private void EmitSelection()
		{
			EmitSignal(SignalName.SelectionChanged, MemberIndex, SelectedClassId,
				SelectedTier1Ids.ToArray(), SelectedTier2Id);
		}

		// ---- UI helpers ----

		private Label MakeSectionLabel(string text, float size, bool emphasize)
		{
			var lbl = new Label
			{
				Text = text,
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Center
			};
			lbl.AddThemeFontSizeOverride("font_size", (int)(emphasize ? size + 1 : size));
			lbl.AddThemeColorOverride("font_color", Colors.White);
			return lbl;
		}

		/// <summary>Returns (tile container, toggle button) so caller can hook events.</summary>
		private (Control tile, BaseButton btn) MakeIconToggleWithCaption(Texture2D? texture, string caption, string tooltip)
		{
			var v = new VBoxContainer { CustomMinimumSize = new Vector2(IconSize.X, IconSize.Y + 22f) };
			v.AddThemeConstantOverride("separation", 4);

			BaseButton btn;
			if (texture != null)
			{
				var tbtn = new TextureButton
				{
					StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered,
					CustomMinimumSize = IconSize,
					TooltipText = tooltip ?? caption ?? "?"
				};
				tbtn.TextureNormal = texture;
				tbtn.ToggleMode = true;
				btn = tbtn;
			}
			else
			{
				var b = new Button
				{
					Text = "?",
					CustomMinimumSize = IconSize,
					TooltipText = tooltip ?? caption ?? "?"
				};
				b.ToggleMode = true;
				btn = b;
			}
			v.AddChild(btn);

			var cap = new Label
			{
				Text = string.IsNullOrWhiteSpace(caption) ? "?" : caption,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			cap.AddThemeFontSizeOverride("font_size", (int)CaptionFontSize);
			cap.ClipText = true;
			v.AddChild(cap);

			return (v, btn);
		}

		private static string SafeName(string? name, string? id)
		{
			if (!string.IsNullOrWhiteSpace(name)) return name.Trim();
			if (!string.IsNullOrWhiteSpace(id)) return id.Trim();
			return "?";
		}

		private static string MakeSpellTooltip(SpellDef sp)
		{
			var n = SafeName(sp?.Name, sp?.Id);
			var d = string.IsNullOrWhiteSpace(sp?.Text) ? "(no description)" : sp.Text.Trim();
			return $"{n}\n{d}";
		}
	}
}
