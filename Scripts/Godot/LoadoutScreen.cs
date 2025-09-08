// res://Scripts/Godot/LoadoutScreen.cs
#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using DiceArena.Engine;
using DiceArena.GameData;   // GameDataRegistry, SpellFactory
using DiceArena.Data;

namespace DiceArena.GodotUI
{
	public partial class LoadoutScreen : Control
	{
		public class PlayerSetup
		{
			public string ClassId = "thief";
			public List<Spell> Faces = new(4);
		}

		public event Action<List<PlayerSetup>> OnConfirm = delegate { };

		private const int MAX_PARTY = 4;
		private const int MAX_SLOTS = 4;
		private const int CAP_T1 = 2;
		private const int CAP_T2 = 1;
		private const int OFFER_T1 = 3;
		private const int OFFER_T2 = 2;

		private ColorRect _backdrop = default!;
		private CenterContainer _center = default!;
		private PanelContainer _card = default!;
		private VBoxContainer _root = default!;

		private SpinBox _partySize = default!;
		private HBoxContainer _topRow = default!;
		private Label _stepTitle = default!;

		private ScrollContainer _classBarScroll = default!;
		private HBoxContainer _classBar = default!;
		private PanelContainer _classInfoPanel = default!;
		private RichTextLabel _classInfo = default!;

		private GridContainer _gridT1 = default!;
		private GridContainer _gridT2 = default!;
		private Label _countT1 = default!;
		private Label _countT2 = default!;
		private Label _selectedCount = default!;

		private Button _reroll = default!;
		private Button _clear = default!;
		private Button _prev = default!;
		private Button _next = default!;
		private Button _finalize = default!;

		private readonly Random _rng = new();

		private class PlayerState
		{
			public string ClassId = "thief";
			public List<SpellDef> OfferT1 = new();
			public List<SpellDef> OfferT2 = new();
			public List<SpellDef> SelT1 = new();
			public List<SpellDef> SelT2 = new();
		}

		private int _partyCount = 4;
		private int _current = 0;
		private readonly List<PlayerState> _players = new(MAX_PARTY);

		private readonly Dictionary<Button, SpellDef> _btnToT1 = new();
		private readonly Dictionary<Button, SpellDef> _btnToT2 = new();

		private readonly List<(Button btn, string id)> _classButtons = new();
		private readonly List<ClassDef> _classDefs = new();

		public override void _Ready()
		{
			Name = "LoadoutScreen";
			AnchorLeft = 0; AnchorTop = 0; AnchorRight = 1; AnchorBottom = 1;
			MouseFilter = MouseFilterEnum.Stop;

			GameDataRegistry.EnsureLoaded();
			SetProcessUnhandledKeyInput(true);

			_classDefs.Clear();
			_classDefs.AddRange(GameDataRegistry.GetClasses().OrderBy(c => c.Name));

			BuildUi();

			_players.Clear();
			for (int i = 0; i < MAX_PARTY; i++) _players.Add(new PlayerState());

			_partyCount = 4;
			_current = 0;

			RebuildPage();
		}

		public override void _UnhandledKeyInput(InputEvent e)
		{
			if (e.IsActionPressed("ui_cancel")) { QueueFree(); return; }
			if (e.IsActionPressed("ui_accept")) { OnNext(); return; }
		}

		// ---------- UI (unchanged structure; class buttons across top) ----------
		private void BuildUi()
		{
			_backdrop = new ColorRect { Color = new Color(0,0,0,0.35f), MouseFilter = MouseFilterEnum.Ignore };
			_backdrop.AnchorLeft = 0; _backdrop.AnchorTop = 0; _backdrop.AnchorRight = 1; _backdrop.AnchorBottom = 1;
			AddChild(_backdrop);

			_center = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
			AddChild(_center);

			_card = new PanelContainer();
			var panel = new StyleBoxFlat
			{
				BgColor = new Color(0.10f, 0.12f, 0.16f, 1f),
				CornerRadiusTopLeft = 10, CornerRadiusTopRight = 10,
				CornerRadiusBottomLeft = 10, CornerRadiusBottomRight = 10,
				ContentMarginLeft = 16, ContentMarginRight = 16,
				ContentMarginTop = 16, ContentMarginBottom = 16
			};
			_card.AddThemeStyleboxOverride("panel", panel);
			_card.CustomMinimumSize = new Vector2(900, 600);
			_center.AddChild(_card);

			_root = new VBoxContainer(); _root.AddThemeConstantOverride("separation", 12);
			_card.AddChild(_root);

			var title = new Label { Text = "Party Setup", HorizontalAlignment = HorizontalAlignment.Center };
			title.AddThemeFontSizeOverride("font_size", 20);
			title.AddThemeColorOverride("font_color", Colors.White);
			_root.AddChild(title);

			_topRow = new HBoxContainer(); _topRow.AddThemeConstantOverride("separation", 8);
			_root.AddChild(_topRow);

			var sizeLabel = new Label { Text = "Party Size:" };
			sizeLabel.AddThemeFontSizeOverride("font_size", 14);
			_topRow.AddChild(sizeLabel);

			_partySize = new SpinBox { MinValue = 1, MaxValue = 4, Value = 4, CustomMinimumSize = new Vector2(96, 0) };
			_partySize.ValueChanged += (double v) =>
			{
				_partyCount = Math.Clamp((int)v, 1, MAX_PARTY);
				_current = System.Math.Min(_current, _partyCount - 1);
				UpdateStepTitle();
				UpdateNavButtons();
			};
			_topRow.AddChild(_partySize);

			_topRow.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });

			_stepTitle = new Label { Text = "Player 1 of 4" };
			_stepTitle.AddThemeFontSizeOverride("font_size", 14);
			_topRow.AddChild(_stepTitle);

			_reroll = new Button { Text = "Reroll Offer" };
			_reroll.TooltipText = "New random set (3×T1, 2×T2) for this player";
			_reroll.Pressed += () => { RerollOfferForCurrent(); };
			_topRow.AddChild(_reroll);

			var classRow = new VBoxContainer(); classRow.AddThemeConstantOverride("separation", 6);
			_root.AddChild(classRow);

			var classRowTitle = new Label { Text = "Choose Class", HorizontalAlignment = HorizontalAlignment.Left };
			classRowTitle.AddThemeFontSizeOverride("font_size", 16);
			classRowTitle.AddThemeColorOverride("font_color", Colors.White);
			classRow.AddChild(classRowTitle);

			_classBarScroll = new ScrollContainer
			{
				HorizontalScrollMode = ScrollContainer.ScrollMode.Auto,
				VerticalScrollMode = ScrollContainer.ScrollMode.Disabled,
				SizeFlagsHorizontal = SizeFlags.ExpandFill
			};
			classRow.AddChild(_classBarScroll);

			_classBar = new HBoxContainer(); _classBar.AddThemeConstantOverride("separation", 8);
			_classBarScroll.AddChild(_classBar);
			BuildClassButtons();

			_classInfoPanel = new PanelContainer();
			var infoStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.08f, 0.09f, 0.12f),
				ContentMarginLeft = 10, ContentMarginRight = 10,
				ContentMarginTop = 10, ContentMarginBottom = 10
			};
			_classInfoPanel.AddThemeStyleboxOverride("panel", infoStyle);
			_classInfoPanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			_root.AddChild(_classInfoPanel);

			_classInfo = new RichTextLabel { BbcodeEnabled = true, FitContent = true, ScrollActive = false };
			_classInfoPanel.AddChild(_classInfo);

			_root.AddChild(MakeSectionHeader("Tier 1 (max 2) — Offered", out _countT1));
			_gridT1 = new GridContainer { Columns = 3 }; _gridT1.AddThemeConstantOverride("h_separation", 8); _gridT1.AddThemeConstantOverride("v_separation", 8);
			_root.AddChild(_gridT1);

			_root.AddChild(MakeSectionHeader("Tier 2 (max 1) — Offered", out _countT2));
			_gridT2 = new GridContainer { Columns = 2 }; _gridT2.AddThemeConstantOverride("h_separation", 8); _gridT2.AddThemeConstantOverride("v_separation", 8);
			_root.AddChild(_gridT2);

			var footerTop = new HBoxContainer(); footerTop.AddThemeConstantOverride("separation", 8);
			_root.AddChild(footerTop);

			_selectedCount = new Label { Text = "Selected: 0 / 4" };
			_selectedCount.AddThemeFontSizeOverride("font_size", 14);
			_selectedCount.AddThemeColorOverride("font_color", new Color("FFD700"));
			footerTop.AddChild(_selectedCount);

			footerTop.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });

			_clear = new Button { Text = "Clear" }; _clear.Pressed += ClearSelectionForCurrent; footerTop.AddChild(_clear);

			var footerBottom = new HBoxContainer(); footerBottom.AddThemeConstantOverride("separation", 8); footerBottom.Alignment = BoxContainer.AlignmentMode.End;
			_root.AddChild(footerBottom);

			_prev = new Button { Text = "Prev" }; _prev.Pressed += () => { OnPrev(); };
			footerBottom.AddChild(_prev);

			_next = new Button { Text = "Next" }; _next.Pressed += () => { OnNext(); };
			footerBottom.AddChild(_next);

			_finalize = new Button { Text = "Finalize & Start" }; _finalize.Pressed += () => { OnFinalize(); };
			footerBottom.AddChild(_finalize);
		}

		private void BuildClassButtons()
		{
			foreach (var child in _classBar.GetChildren()) child.QueueFree();
			_classButtons.Clear();

			if (_classDefs.Count == 0)
			{
				_classBar.AddChild(new Label { Text = "No classes found (check Data/classes.json)" });
				return;
			}

			foreach (var c in _classDefs)
			{
				var btn = new Button
				{
					Text = c.Name,
					CustomMinimumSize = new Vector2(140, 40),
					ToggleMode = true
				};
				btn.Pressed += () => OnClassButtonPressed(btn, c.Id);
				_classBar.AddChild(btn);
				_classButtons.Add((btn, c.Id));
			}
		}

		private Control MakeSectionHeader(string text, out Label countLabel)
		{
			var row = new HBoxContainer(); row.AddThemeConstantOverride("separation", 8);
			var label = new Label { Text = text }; label.AddThemeFontSizeOverride("font_size", 16); label.AddThemeColorOverride("font_color", Colors.White);
			row.AddChild(label);
			countLabel = new Label { Text = "" }; countLabel.AddThemeFontSizeOverride("font_size", 14); countLabel.AddThemeColorOverride("font_color", new Color("FFD700"));
			row.AddChild(countLabel);
			return row;
		}

		private void RebuildPage()
		{
			UpdateStepTitle();
			UpdateNavButtons();

			var ps = _players[_current];
			if (string.IsNullOrWhiteSpace(ps.ClassId)) ps.ClassId = "thief";

			if (ps.OfferT1.Count == 0 && ps.OfferT2.Count == 0) BuildRandomOffer(ps);

			UpdateClassButtonsSelection(ps.ClassId);
			UpdateClassInfo(ps.ClassId);

			PopulateOfferButtons(ps);
			UpdateUiState(ps);
		}

		private void UpdateStepTitle() => _stepTitle.Text = $"Player {_current + 1} of {_partyCount}";
		private void UpdateNavButtons()
		{
			_prev.Disabled = _current <= 0;
			_next.Disabled = _current >= _partyCount - 1;
			_finalize.Disabled = _current != _partyCount - 1;
		}

		private void OnClassButtonPressed(Button btn, string classId)
		{
			_players[_current].ClassId = classId;
			UpdateClassButtonsSelection(classId);
			UpdateClassInfo(classId);
		}

		private void UpdateClassButtonsSelection(string classId)
		{
			foreach (var (btn, id) in _classButtons)
			{
				bool on = string.Equals(id, classId, System.StringComparison.OrdinalIgnoreCase);
				btn.SetPressedNoSignal(on);
				if (on) { btn.AddThemeColorOverride("font_outline_color", Colors.White); btn.AddThemeConstantOverride("outline_size", 1); }
				else { if (btn.HasThemeConstantOverride("outline_size")) btn.RemoveThemeConstantOverride("outline_size"); btn.RemoveThemeColorOverride("font_outline_color"); }
			}
		}

		private void UpdateClassInfo(string classId)
		{
			var c = GameDataRegistry.GetClassById(classId);
			if (c == null) { _classInfo.Text = "[b]Unknown Class[/b]"; return; }

			var heroAction = (c.HeroAction ?? "").Replace("\r\n", "\n").Replace("\n•", "\n  •");
			_classInfo.Text =
				$"[b]{c.Name}[/b]\n" +
				$"[i]Trait:[/i] {c.Trait}\n" +
				$"[i]Hero Action:[/i]\n{heroAction}";
		}

		private void BuildRandomOffer(PlayerState ps)
		{
			ps.OfferT1.Clear(); ps.OfferT2.Clear();
			ps.SelT1.Clear(); ps.SelT2.Clear();

			var t1Pool = GameDataRegistry.GetSpellsByTier(1).ToList();
			ShuffleInPlace(t1Pool);
			foreach (var d in t1Pool) { ps.OfferT1.Add(d); if (ps.OfferT1.Count >= OFFER_T1) break; }

			var t2Pool = GameDataRegistry.GetSpellsByTier(2).ToList();
			ShuffleInPlace(t2Pool);
			foreach (var d in t2Pool) { ps.OfferT2.Add(d); if (ps.OfferT2.Count >= OFFER_T2) break; }
		}

		private void PopulateOfferButtons(PlayerState ps)
		{
			foreach (var c in _gridT1.GetChildren()) c.QueueFree();
			foreach (var c in _gridT2.GetChildren()) c.QueueFree();
			_btnToT1.Clear(); _btnToT2.Clear();

			foreach (var def in ps.OfferT1) AddSpellButton(_gridT1, def, _btnToT1, OnT1Toggled);
			foreach (var def in ps.OfferT2) AddSpellButton(_gridT2, def, _btnToT2, OnT2Toggled);

			foreach (var kv in _btnToT1) if (ps.SelT1.Contains(kv.Value)) { kv.Key.SetPressedNoSignal(true); StyleSelected(kv.Key, true); }
			foreach (var kv in _btnToT2) if (ps.SelT2.Contains(kv.Value)) { kv.Key.SetPressedNoSignal(true); StyleSelected(kv.Key, true); }
		}

		private void AddSpellButton(Container parent, SpellDef def, Dictionary<Button, SpellDef> map, Action<Button> onToggle)
		{
			var btn = new Button
			{
				Text = string.IsNullOrWhiteSpace(def.Name) ? def.Id : def.Name,
				ToggleMode = true,
				CustomMinimumSize = new Vector2(160, 48),
				TooltipText = def.Text ?? ""
			};
			map[btn] = def;
			btn.Pressed += () => onToggle(btn);
			parent.AddChild(btn);
		}

		private void RerollOfferForCurrent()
		{
			var ps = _players[_current];
			BuildRandomOffer(ps);
			PopulateOfferButtons(ps);
			UpdateUiState(ps);
			_ = Blink(_reroll, new Color(0.7f, 0.9f, 1f));
		}

		private int TotalChosen(PlayerState ps) => ps.SelT1.Count + ps.SelT2.Count;

		private void OnT1Toggled(Button btn)
		{
			var ps = _players[_current];
			bool on = btn.ButtonPressed;
			var def = _btnToT1[btn];

			if (on)
			{
				if (ps.SelT1.Count >= CAP_T1 || TotalChosen(ps) >= MAX_SLOTS) { btn.SetPressedNoSignal(false); _ = Blink(_countT1, new Color(1, 0.6f, 0.6f)); return; }
				ps.SelT1.Add(def); StyleSelected(btn, true);
			}
			else { ps.SelT1.Remove(def); StyleSelected(btn, false); }

			UpdateUiState(ps);
		}

		private void OnT2Toggled(Button btn)
		{
			var ps = _players[_current];
			bool on = btn.ButtonPressed;
			var def = _btnToT2[btn];

			if (on)
			{
				if (ps.SelT2.Count >= CAP_T2 || TotalChosen(ps) >= MAX_SLOTS) { btn.SetPressedNoSignal(false); _ = Blink(_countT2, new Color(1, 0.6f, 0.6f)); return; }
				ps.SelT2.Add(def); StyleSelected(btn, true);
			}
			else { ps.SelT2.Remove(def); StyleSelected(btn, false); }

			UpdateUiState(ps);
		}

		private void StyleSelected(Button btn, bool on)
		{
			if (on) { btn.AddThemeColorOverride("font_outline_color", Colors.White); btn.AddThemeConstantOverride("outline_size", 1); }
			else { if (btn.HasThemeConstantOverride("outline_size")) btn.RemoveThemeConstantOverride("outline_size"); btn.RemoveThemeColorOverride("font_outline_color"); }
		}

		private void UpdateUiState(PlayerState ps)
		{
			_countT1.Text = $"  T1: {ps.SelT1.Count}/{CAP_T1}";
			_countT2.Text = $"  T2: {ps.SelT2.Count}/{CAP_T2}";
			_selectedCount.Text = $"Selected: {TotalChosen(ps)} / {MAX_SLOTS}";

			bool t1Block = ps.SelT1.Count >= CAP_T1 || TotalChosen(ps) >= MAX_SLOTS;
			bool t2Block = ps.SelT2.Count >= CAP_T2 || TotalChosen(ps) >= MAX_SLOTS;

			foreach (var kv in _btnToT1) { if (!kv.Key.ButtonPressed) kv.Key.Disabled = t1Block; else kv.Key.Disabled = false; }
			foreach (var kv in _btnToT2) { if (!kv.Key.ButtonPressed) kv.Key.Disabled = t2Block; else kv.Key.Disabled = false; }
		}

		private void ClearSelectionForCurrent()
		{
			var ps = _players[_current];
			ps.SelT1.Clear(); ps.SelT2.Clear();
			foreach (var kv in _btnToT1) { kv.Key.SetPressedNoSignal(false); StyleSelected(kv.Key, false); kv.Key.Disabled = false; }
			foreach (var kv in _btnToT2) { kv.Key.SetPressedNoSignal(false); StyleSelected(kv.Key, false); kv.Key.Disabled = false; }
			UpdateUiState(ps);
		}

		private async System.Threading.Tasks.Task Blink(Control ctrl, Color color, float seconds = 0.18f)
		{
			var old = ctrl.Modulate;
			ctrl.Modulate = color;
			await ToSignal(GetTree().CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);
			ctrl.Modulate = old;
		}

		private static void ShuffleInPlace<T>(IList<T> list)
		{
			var rng = new System.Random();
			for (int i = list.Count - 1; i > 0; i--)
			{
				int j = rng.Next(i + 1);
				(list[i], list[j]) = (list[j], list[i]);
			}
		}

		private bool ValidateCurrent(out string error) { error = ""; return true; }

		private void OnPrev() { if (_current <= 0) return; _current--; RebuildPage(); }
		private void OnNext() { if (!ValidateCurrent(out _)) return; if (_current >= _partyCount - 1) return; _current++; RebuildPage(); }

		private void OnFinalize()
		{
			if (!ValidateCurrent(out _)) return;

			var result = new List<PlayerSetup>(_partyCount);
			for (int i = 0; i < _partyCount; i++)
			{
				var ps = _players[i];
				var faces = new List<Spell>(MAX_SLOTS);
				faces.AddRange(ps.SelT1.Select(GameDataRegistry.ToEngineSpell));
				faces.AddRange(ps.SelT2.Select(GameDataRegistry.ToEngineSpell));

				// >>> NEW: pad with "Defend" faces so unused slots become Defensive Action spaces
				while (faces.Count < MAX_SLOTS)
				{
					// If SpellFactory supports kind "defend", use it; else make a minimal Spell named "Defend".
					var defend = SpellFactory.FromKind("defend", 1, "Defend");
					faces.Add(defend);
				}
				if (faces.Count > MAX_SLOTS) faces.RemoveRange(MAX_SLOTS, faces.Count - MAX_SLOTS);

				result.Add(new PlayerSetup { ClassId = ps.ClassId, Faces = faces });
			}

			OnConfirm(result);
		}
	}
}
