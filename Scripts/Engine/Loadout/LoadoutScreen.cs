// Scripts/Engine/Loadout/LoadoutScreen.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using DiceArena.Engine.Content;

namespace DiceArena.Engine.Loadout
{
	public partial class LoadoutScreen : Control
	{
		[Export] public int PartySize { get; set; } = 3;

		public ContentBundle Bundle { get; set; } = default!;

		private readonly List<LoadoutMemberCard> _cards = new();

		private Control _bgDim = null!;
		private VBoxContainer _root = null!;
		private HBoxContainer _topBar = null!;
		private SpinBox _partySpin = null!;
		private Button _rerollBtn = null!;
		private Button _confirmBtn = null!;
		private GridContainer _membersGrid = null!;

		public override void _Ready()
		{
			// fallback if Game.cs didn’t inject
			if (Bundle == null)
			{
				GD.PushWarning("[LoadoutScreen] No bundle injected; loading fallback from res://Content");
				Bundle = ContentDatabase.LoadFromFolder("res://Content");
			}

			BuildUI();
			RebuildMembers(Mathf.Clamp(PartySize, 1, 4));
			RerollAllOffers();
		}

		private void BuildUI()
		{
			// Full-screen dimmer — does NOT consume input
			_bgDim = new PanelContainer
			{
				MouseFilter = MouseFilterEnum.Ignore,   // <-- don’t eat clicks
				ZIndex = 5
			};
			_bgDim.AnchorRight = 1;  // full rect
			_bgDim.AnchorBottom = 1;
			var dim = new StyleBoxFlat { BgColor = new Color(0, 0, 0, 0.12f) };
			_bgDim.AddThemeStyleboxOverride("panel", dim);
			AddChild(_bgDim);

			// Root UI that DOES receive input
			_root = new VBoxContainer
			{
				MouseFilter = MouseFilterEnum.Stop,     // <-- do eat clicks
				ZIndex = 10,
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical = SizeFlags.ExpandFill
			};
			_root.AnchorRight = 1;
			_root.AnchorBottom = 1;
			_root.AddThemeConstantOverride("separation", 12);
			AddChild(_root);

			// Top bar sits on its own solid panel (never blends)
			var topPanel = new PanelContainer
			{
				MouseFilter = MouseFilterEnum.Stop,
				ZIndex = 11
			};
			var topSb = new StyleBoxFlat { BgColor = new Color(0.15f, 0.15f, 0.15f, 1f) };
			topPanel.AddThemeStyleboxOverride("panel", topSb);
			_root.AddChild(topPanel);

			_topBar = new HBoxContainer
			{
				MouseFilter = MouseFilterEnum.Stop
			};
			_topBar.AddThemeConstantOverride("separation", 8);
			_topBar.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			topPanel.AddChild(_topBar);

			_topBar.AddChild(new Label { Text = "Party Size:" });

			_partySpin = new SpinBox
			{
				MinValue = 1,
				MaxValue = 4,
				Step = 1,
				Value = Mathf.Clamp(PartySize, 1, 4),
				CustomMinimumSize = new Vector2(80, 0)
			};
			_partySpin.ValueChanged += v =>
			{
				GD.Print($"[Loadout] Party size -> {v}");
				RebuildMembers((int)v);
				RerollAllOffers();
			};
			_topBar.AddChild(_partySpin);

			_rerollBtn = new Button { Text = "Re-roll Offers" };
			_rerollBtn.Pressed += () =>
			{
				GD.Print("[Loadout] Re-roll pressed");
				RerollAllOffers();
			};
			_topBar.AddChild(_rerollBtn);

			_confirmBtn = new Button { Text = "Confirm Loadout" };
			_confirmBtn.Pressed += () =>
			{
				GD.Print("[Loadout] Confirm pressed");
				ConfirmLoadout();
			};
			_topBar.AddChild(_confirmBtn);

			_membersGrid = new GridContainer
			{
				MouseFilter = MouseFilterEnum.Stop,
				Columns = 1,
				ZIndex = 10
			};
			_membersGrid.AddThemeConstantOverride("v_separation", 16);
			_membersGrid.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			_membersGrid.SizeFlagsVertical = SizeFlags.ExpandFill;
			_root.AddChild(_membersGrid);
		}

		private void RebuildMembers(int count)
		{
			_membersGrid.Columns = count <= 2 ? 1 : 2;

			for (int i = _membersGrid.GetChildCount() - 1; i >= 0; i--)
				_membersGrid.GetChild(i).QueueFree();
			_cards.Clear();

			var classes = Bundle.Classes.OrderBy(c => c.Name).ToList();

			for (int i = 0; i < count; i++)
			{
				var panel = new PanelContainer { MouseFilter = MouseFilterEnum.Stop };
				var inner = new VBoxContainer { MouseFilter = MouseFilterEnum.Stop };
				panel.AddChild(inner);
				inner.AddThemeConstantOverride("separation", 8);

				var card = new LoadoutMemberCard { TitleText = $"Member {i + 1}" };
				inner.AddChild(card);

				_membersGrid.AddChild(panel);
				_cards.Add(card);

				card.Setup(Bundle, classes, Array.Empty<SpellDef>(), Array.Empty<SpellDef>());
			}
		}

		private void RerollAllOffers()
		{
			foreach (var card in _cards)
			{
				var t1 = RollTierOffers(1, LoadoutRules.Tier1OfferCount).ToList();
				var t2 = RollTierOffers(2, LoadoutRules.Tier2OfferCount).ToList();
				card.Setup(Bundle, Bundle.Classes, t1, t2);
			}
		}

		private IEnumerable<SpellDef> RollTierOffers(int tier, int count)
		{
			var pool = Bundle.Spells.Where(s => s.Tier == tier).ToList();
			var picks = new List<SpellDef>(count);

			for (int i = 0; i < count && pool.Count > 0; i++)
			{
				int idx = (int)(GD.Randi() % (uint)pool.Count);
				picks.Add(pool[idx]);
				pool.RemoveAt(idx);
			}

			return picks;
		}

		private void ConfirmLoadout()
		{
			var party = new PartyLoadout { PartySize = _cards.Count };

			foreach (var card in _cards)
			{
				var m = new MemberLoadout
				{
					ClassId = card.GetSelectedClassId() ?? string.Empty,
					Tier1OfferIds = Bundle.Spells.Where(s => s.Tier == 1).Select(s => s.Id).ToList(),
					Tier2OfferIds = Bundle.Spells.Where(s => s.Tier == 2).Select(s => s.Id).ToList()
				};

				var (tier1, tier2) = card.GetPicks();
				m.ChosenTier1SpellIds = new HashSet<string>(tier1);
				m.ChosenTier2SpellId = tier2;

				party.Members.Add(m);
			}

			var (ok, badIndex, reason) = LoadoutSystem.ValidateParty(party);
			if (!ok)
			{
				GD.PushWarning($"Invalid party: Member {badIndex + 1}: {reason}");
				return;
			}

			GD.Print(LoadoutSystem.Describe(party, Bundle));
			// TODO: hand off to battle scene here.
		}
	}
}
