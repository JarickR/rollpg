using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using DiceArena.Engine.Content;

namespace DiceArena.Engine.Loadout
{
	/// <summary>
	/// One party member card: class grid + offered spells (Tier1/Tier2) with constrained picks (2xT1, 1xT2).
	/// Visuals match the rounded highlight border you approved.
	/// </summary>
	public partial class LoadoutMemberCard : Control
	{
		// ---------- layout constants ----------
		private const int IconButtonSize       = 64;
		private const int IconCornerRadius     = 10;
		private const int IconBorderThickness  = 2;
		private const int IconInset            = 4;

		private const int GridColumns          = 5;   // classes row
		private const int SpellColumns         = 10;  // spell rows

		private const string MetaKind          = "kind";
		private const string KindClass         = "class";
		private const string KindT1            = "t1";
		private const string KindT2            = "t2";
		private const string MetaId            = "id";

		// ---------- UI ----------
		private VBoxContainer _root = default!;
		private Label _title = default!;
		private Label _classLabel = default!;
		private GridContainer _classGrid = default!;
		private Label _t1Label = default!;
		private GridContainer _t1Row = default!;
		private Label _t2Label = default!;
		private GridContainer _t2Row = default!;

		// ---------- state ----------
		private List<ClassDef> _classes = new();
		private MemberOffer _offer = default!;

		private string? _selectedClassId;

		private readonly Dictionary<string, Button> _classButtons = new();
		private readonly Dictionary<string, Button> _t1Buttons = new();
		private readonly Dictionary<string, Button> _t2Buttons = new();

		private readonly HashSet<string> _selT1 = new();
		private string? _selT2Id;

		private readonly Dictionary<string, SpellDef> _t1Defs = new();
		private readonly Dictionary<string, SpellDef> _t2Defs = new();
		private readonly Dictionary<string, ClassDef> _classDefs = new();

		public override void _Ready()
		{
			// No need to check Name.Length (StringName); just build
			BuildScaffold();
		}

		private void BuildScaffold()
		{
			_root = new VBoxContainer
			{
				CustomMinimumSize = new Vector2(700, 220),
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical = SizeFlags.ShrinkCenter
			};
			AddChild(_root);

			_title = MakeSectionTitle("Member");
			_root.AddChild(_title);

			_classLabel = MakeSmallHeader("Class");
			_root.AddChild(_classLabel);

			_classGrid = new GridContainer { Columns = GridColumns, CustomMinimumSize = new Vector2(0, IconButtonSize + 30) };
			_root.AddChild(_classGrid);

			_root.AddChild(Spacer(8));

			_t1Label = MakeSmallHeader("Tier 1 Spells (pick 2)");
			_root.AddChild(_t1Label);

			_t1Row = new GridContainer { Columns = SpellColumns };
			_root.AddChild(_t1Row);

			_root.AddChild(Spacer(8));

			_t2Label = MakeSmallHeader("Tier 2 Spells (pick 1)");
			_root.AddChild(_t2Label);

			_t2Row = new GridContainer { Columns = SpellColumns };
			_root.AddChild(_t2Row);
		}

		// ---------- public API ----------

		public void SetTitle(string text) => _title.Text = text;

		public void Build(List<ClassDef> classes, MemberOffer offer)
		{
			_classes = classes ?? throw new ArgumentNullException(nameof(classes));
			_offer = offer ?? throw new ArgumentNullException(nameof(offer));

			_classDefs.Clear();
			_t1Defs.Clear();
			_t2Defs.Clear();

			ClearGrid(_classGrid);
			ClearGrid(_t1Row);
			ClearGrid(_t2Row);

			_classButtons.Clear();
			_t1Buttons.Clear();
			_t2Buttons.Clear();

			_selT1.Clear();
			_selT2Id = null;
			_selectedClassId = null;

			_title.Text = $"Member {offer.MemberIndex + 1}";
			_t1Label.Text = $"Tier 1 Spells (pick {offer.Tier1PickCount})";
			_t2Label.Text = $"Tier 2 Spells (pick {offer.Tier2PickCount})";

			// classes
			foreach (var c in _classes)
			{
				_classDefs[c.Id] = c;
				var btn = MakeIconButton(IconLibrary.GetClassTexture(c.Id), c.Name ?? c.Id, IconCornerRadius, IconButtonSize);
				btn.SetMeta(MetaKind, KindClass);
				btn.SetMeta(MetaId, c.Id);
				btn.ToggleMode = true;
				btn.Toggled += (bool pressed) => OnClassToggled(btn, pressed);
				_classGrid.AddChild(btn);
				_classButtons[c.Id] = btn;
			}

			// tier1 offered
			foreach (var s in _offer.Tier1)
			{
				_t1Defs[s.Id] = s;
				var tex = IconLibrary.GetSpellTexture(s.Id, 1);
				var btn = MakeIconButton(tex, SafeName(s.Name, s.Id), IconCornerRadius, IconButtonSize);
				btn.SetMeta(MetaKind, KindT1);
				btn.SetMeta(MetaId, s.Id);
				btn.ToggleMode = true;
				btn.Toggled += (bool pressed) => OnT1Toggled(btn, pressed);
				_t1Row.AddChild(btn);
				_t1Buttons[s.Id] = btn;
			}

			// tier2 offered
			foreach (var s in _offer.Tier2)
			{
				_t2Defs[s.Id] = s;
				var tex = IconLibrary.GetSpellTexture(s.Id, 2);
				var btn = MakeIconButton(tex, SafeName(s.Name, s.Id), IconCornerRadius, IconButtonSize);
				btn.SetMeta(MetaKind, KindT2);
				btn.SetMeta(MetaId, s.Id);
				btn.ToggleMode = true;
				btn.Toggled += (bool pressed) => OnT2Toggled(btn, pressed);
				_t2Row.AddChild(btn);
				_t2Buttons[s.Id] = btn;
			}
		}

		public bool IsComplete()
		{
			if (string.IsNullOrEmpty(_selectedClassId)) return false;
			if (_selT1.Count != _offer.Tier1PickCount) return false;
			if (_offer.Tier2PickCount == 1 && _selT2Id is null) return false;
			return true;
		}

		public (ClassDef Class, List<SpellDef> Tier1, SpellDef Tier2) GetSelections()
		{
			if (!IsComplete())
				throw new InvalidOperationException("Member card selections are incomplete.");

			var cls = _classDefs[_selectedClassId!];
			var t1 = _selT1.Select(id => _t1Defs[id]).ToList();
			t1.Sort((a, b) => string.Compare(a.Id, b.Id, StringComparison.Ordinal));
			var t2 = _t2Defs[_selT2Id!];
			return (cls, t1, t2);
		}

		// ---------- event handlers ----------
		private void OnClassToggled(Button btn, bool pressed)
		{
			var id = (string)btn.GetMeta(MetaId);
			if (pressed)
			{
				foreach (var kv in _classButtons)
					if (kv.Key != id && kv.Value.ButtonPressed)
						kv.Value.ButtonPressed = false;

				_selectedClassId = id;
			}
			else if (_selectedClassId == id)
			{
				_selectedClassId = null;
			}
			SetHighlight(btn, pressed);
		}

		private void OnT1Toggled(Button btn, bool pressed)
		{
			var id = (string)btn.GetMeta(MetaId);
			if (pressed)
			{
				if (_selT1.Count >= _offer.Tier1PickCount)
				{
					btn.ButtonPressed = false;
					return;
				}
				_selT1.Add(id);
			}
			else
			{
				_selT1.Remove(id);
			}
			SetHighlight(btn, pressed);
		}

		private void OnT2Toggled(Button btn, bool pressed)
		{
			var id = (string)btn.GetMeta(MetaId);
			if (pressed)
			{
				if (_selT2Id != null && _selT2Id != id && _t2Buttons.TryGetValue(_selT2Id, out var prev))
					prev.ButtonPressed = false;
				_selT2Id = id;
			}
			else if (_selT2Id == id)
			{
				_selT2Id = null;
			}
			SetHighlight(btn, pressed);
		}

		// ---------- UI helpers ----------
		private static Control Spacer(int h) => new Control { CustomMinimumSize = new Vector2(0, h) };

		private static Label MakeSectionTitle(string text) =>
			new Label { Text = text, ThemeTypeVariation = "HeaderMedium", AutowrapMode = TextServer.AutowrapMode.Off, ClipText = true };

		private static Label MakeSmallHeader(string text) =>
			new Label { Text = text, ThemeTypeVariation = "HeaderSmall", AutowrapMode = TextServer.AutowrapMode.Off, ClipText = true };

		private static void ClearGrid(Container c)
		{
			foreach (var child in c.GetChildren())
				child.QueueFree();
		}

		private static string SafeName(string? name, string id) => string.IsNullOrWhiteSpace(name) ? id : name!;

		private Button MakeIconButton(Texture2D? icon, string caption, int radius, int size)
		{
			var outer = new Button
			{
				ToggleMode = true,
				FocusMode = FocusModeEnum.None,
				CustomMinimumSize = new Vector2(size + IconInset * 2, size + 24 + IconInset * 2),
			};

			var vb = new VBoxContainer
			{
				AnchorsPreset = (int)LayoutPreset.FullRect,
				OffsetTop = IconInset,
				OffsetLeft = IconInset,
				OffsetRight = -IconInset,
				OffsetBottom = -IconInset,
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical = SizeFlags.ExpandFill,
			};
			outer.AddChild(vb);

			var tex = new TextureRect
			{
				Texture = icon ?? IconLibrary.Transparent1x1,
				ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional,
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				CustomMinimumSize = new Vector2(size, size),
				SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
				SizeFlagsVertical = SizeFlags.ShrinkCenter
			};
			vb.AddChild(tex);

			var lab = new Label
			{
				Text = caption,
				HorizontalAlignment = HorizontalAlignment.Center,
				AutowrapMode = TextServer.AutowrapMode.Off,
				ClipText = true
			};
			vb.AddChild(lab);

			// Rounded border overlay as a Panel with StyleBoxFlat (no custom draw)
			var overlay = new BorderOverlay
			{
				CornerRadius = radius,
				BorderThickness = IconBorderThickness,
				Visible = false
			};
			overlay.SetAnchorsPreset(LayoutPreset.FullRect);
			outer.AddChild(overlay);

			outer.SetMeta("overlay", overlay);
			return outer;
		}

		private static BorderOverlay? GetOverlay(Button b)
		{
			var v = b.GetMeta("overlay");
			if (v.VariantType == Variant.Type.Object)
			{
				var obj = v.AsGodotObject();
				return obj as BorderOverlay;
			}
			return null;
		}

		private static void SetHighlight(Button b, bool on)
		{
			var bo = GetOverlay(b);
			if (bo != null) bo.Visible = on;
		}
	}

	/// <summary>Rounded-border overlay for selection highlight, implemented with StyleBoxFlat.</summary>
	public partial class BorderOverlay : Panel
	{
		[Export] public int CornerRadius { get; set; } = 10;
		[Export] public int BorderThickness { get; set; } = 2;
		[Export] public Color BorderColor { get; set; } = new Color(1, 0.8f, 0.2f, 1f);

		public override void _Ready()
		{
			var sb = new StyleBoxFlat
			{
				DrawCenter = false,
				BorderColor = BorderColor,
				CornerRadiusTopLeft = CornerRadius,
				CornerRadiusTopRight = CornerRadius,
				CornerRadiusBottomLeft = CornerRadius,
				CornerRadiusBottomRight = CornerRadius,
				BorderWidthTop = BorderThickness,
				BorderWidthRight = BorderThickness,
				BorderWidthBottom = BorderThickness,
				BorderWidthLeft = BorderThickness,
			};
			AddThemeStyleboxOverride("panel", sb);
		}
	}
}
