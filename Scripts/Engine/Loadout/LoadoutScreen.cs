// res://Scripts/Engine/Loadout/LoadoutScreen.cs
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;

namespace DiceArena.Engine.Loadout
{
	public partial class LoadoutScreen : Control
	{
		[Export] public NodePath MemberCardsContainerPath { get; set; } = default!;
		[Export] public NodePath PartyButtonsContainerPath { get; set; } = default!;
		[Export] public int DefaultPartySize { get; set; } = 1;
		[Export] public bool AutoPickWhenShown { get; set; } = true;

		private Control? _cardsRoot;
		private Control? _buttonsRoot;

		private readonly List<DiceArena.Godot.PlayerLoadoutPanel> _panels = new();
		private readonly List<BaseButton> _partyButtons = new();
		private bool _initialApplied;

		public override void _Ready()
		{
			GD.Print("[LoadoutScreen] READY");

			_cardsRoot   = GetNodeOrNull<Control>(MemberCardsContainerPath);
			_buttonsRoot = GetNodeOrNull<Control>(PartyButtonsContainerPath);

			GD.Print($"[LoadoutScreen] cardsRoot={( _cardsRoot!=null ? _cardsRoot.GetPath() : (NodePath)"<null>")}");
			GD.Print($"[LoadoutScreen] buttonsRoot={( _buttonsRoot!=null ? _buttonsRoot.GetPath() : (NodePath)"<null>")}");

			_panels.AddRange(FindAllPanels());
			GD.Print($"[LoadoutScreen] found panels={_panels.Count}");
			if (_panels.Count == 0)
				GD.PushError("[LoadoutScreen] NO PlayerLoadoutPanel nodes found. Party-size controls won’t change anything.");

			_partyButtons.AddRange(FindAllPartyButtons());
			GD.Print($"[LoadoutScreen] found partyButtons={_partyButtons.Count}");
			if (_partyButtons.Count == 0)
				GD.PushWarning("[LoadoutScreen] No party-size buttons found. You can still change size from code.");

			for (int i = 0; i < _panels.Count; i++)
				GD.Print($"[LoadoutScreen] panel[{i}] = { _panels[i].GetPath() }");
			foreach (var b in _partyButtons)
				GD.Print($"[LoadoutScreen] button '{b.Name}' target={ParseTargetSize(b)} path={b.GetPath()}");

			foreach (var btn in _partyButtons)
			{
				if (btn.HasMeta("wired_party_size")) continue;
				btn.SetMeta("wired_party_size", true);

				int target = Math.Clamp(ParseTargetSize(btn), 1, Math.Max(1, _panels.Count));
				btn.ToggleMode = true;
				btn.ButtonPressed = false;

				btn.Pressed += () =>
				{
					GD.Print($"[LoadoutScreen] Button pressed -> size {target}");
					SetPartySize(target);
				};
			}

			CallDeferred(nameof(ApplyInitial));
		}

		private void ApplyInitial()
		{
			if (_initialApplied) return;
			_initialApplied = true;

			int wanted = Math.Clamp(DefaultPartySize, 1, Math.Max(1, _panels.Count));
			GD.Print($"[LoadoutScreen] ApplyInitial -> DefaultPartySize={DefaultPartySize} -> clamped={wanted}");
			SetPartySize(wanted);
		}

		// ---------- Discovery (robust, no type-string) ----------
		private static IEnumerable<Node> Walk(Node root)
		{
			var stack = new Stack<Node>();
			stack.Push(root);
			while (stack.Count > 0)
			{
				var n = stack.Pop();
				yield return n;
				foreach (var c in n.GetChildren()) stack.Push((Node)c);
			}
		}

		private List<DiceArena.Godot.PlayerLoadoutPanel> FindAllPanels()
		{
			var list = new List<DiceArena.Godot.PlayerLoadoutPanel>();

			// 1) Under configured container
			if (_cardsRoot != null)
				list.AddRange(Walk(_cardsRoot).OfType<DiceArena.Godot.PlayerLoadoutPanel>());

			// 2) Entire scene (fallback)
			if (list.Count == 0)
				list.AddRange(Walk(GetTree().Root).OfType<DiceArena.Godot.PlayerLoadoutPanel>());

			// 3) Optional group “loadout_panel”
			foreach (var n in GetTree().GetNodesInGroup("loadout_panel"))
				if (n is DiceArena.Godot.PlayerLoadoutPanel p && !list.Contains(p))
					list.Add(p);

			return list.Distinct().ToList();
		}

		private List<BaseButton> FindAllPartyButtons()
		{
			var grouped = GetTree().GetNodesInGroup("party_size").OfType<BaseButton>().ToList();
			if (grouped.Count > 0)
				return grouped.OrderBy(ParseTargetSize).ToList();

			if (_buttonsRoot != null)
			{
				var found = Walk(_buttonsRoot).OfType<BaseButton>()
					.Where(b => ParseTargetSize(b) >= 1)
					.OrderBy(ParseTargetSize)
					.ToList();
				if (found.Count > 0) return found;
			}

			return Walk(GetTree().Root).OfType<BaseButton>()
				.Where(b => ParseTargetSize(b) >= 1)
				.OrderBy(ParseTargetSize)
				.ToList();
		}
		// --------------------------------------------------------

		private static int ParseTargetSize(BaseButton b)
		{
			if (b is Button btn && !string.IsNullOrWhiteSpace(btn.Text))
				if (int.TryParse(btn.Text.Trim(), out var n1)) return n1;

			var label = b.GetChildren().OfType<Label>().FirstOrDefault();
			if (label != null && int.TryParse(label.Text.Trim(), out var n2))
				return n2;

			var m = Regex.Match(b.Name, @"(\d+)");
			if (m.Success && int.TryParse(m.Groups[1].Value, out var n3))
				return n3;

			if (b.HasMeta("party_size"))
			{
				var s = b.GetMeta("party_size").ToString();
				if (!string.IsNullOrWhiteSpace(s) && int.TryParse(s, out var n4))
					return n4;
			}

			return 0;
		}

		private void SetPartySize(int count)
		{
			if (_panels.Count == 0)
			{
				GD.PushError("[LoadoutScreen] SetPartySize called but no panels were found.");
				return;
			}

			count = Math.Clamp(count, 1, _panels.Count);
			GD.Print($"[LoadoutScreen] SetPartySize -> {count} (of {_panels.Count})");

			for (int i = 0; i < _panels.Count; i++)
			{
				bool show = i < count;
				var panel = _panels[i];

				panel.Visible = show;
				panel.SetDeferred("visible", show);
				panel.CustomMinimumSize = Vector2.Zero;

				if (show && AutoPickWhenShown)
					panel.CallDeferred(nameof(DiceArena.Godot.PlayerLoadoutPanel.AutoPickForPanel));
			}

			foreach (var b in _partyButtons)
			{
				var t = ParseTargetSize(b);
				b.ButtonPressed = (t == count);
			}

			if (_cardsRoot is Container cont) cont.QueueSort();
			else _cardsRoot?.QueueRedraw();
		}
	}
}
