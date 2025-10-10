using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiceArena.Godot
{
	public partial class PlayerLoadoutPanel : Control
	{
		// ---- Inspector paths ----
		[Export] public NodePath ClassSelectionContainerPath { get; set; }
		[Export] public NodePath Tier1SpellContainerPath { get; set; }
		[Export] public NodePath Tier2SpellContainerPath { get; set; }
		[Export] public NodePath LoadoutContainerPath { get; set; }

		// Scenes for DEF/UPGRADE tiles (your .tscn scenes)
		[Export] public PackedScene UpgradeSlotScene { get; set; }
		[Export] public PackedScene DefensiveSlotScene { get; set; }

		// Limits (shown vs picks)
		[Export] public int Tier1OptionsShown { get; set; } = 3;
		[Export] public int Tier2OptionsShown { get; set; } = 2;
		[Export] public int Tier1PickCount { get; set; } = 2;
		[Export] public int Tier2PickCount { get; set; } = 1;
		[Export] public int LoadoutSlotCount { get; set; } = 6;

		// ---- Runtime refs ----
		private Container _classRoot;
		private Container _tier1Root;
		private Container _tier2Root;
		private Container _loadoutRoot;

		// Chosen textures
		private Texture2D _classPick;
		private readonly List<Texture2D> _tier1Picks = new();
		private readonly List<Texture2D> _tier2Picks = new();

		// Loadout slot nodes (TextureRect / Button / TextureButton)
		private readonly List<Control> _loadoutSlots = new();

		public override void _Ready()
		{
			// Resolve nodes
			_classRoot = GetNode<Container>(ClassSelectionContainerPath);
			_tier1Root = GetNode<Container>(Tier1SpellContainerPath);
			_tier2Root = GetNode<Container>(Tier2SpellContainerPath);
			_loadoutRoot = GetNode<Container>(LoadoutContainerPath);

			// Wire buttons
			WireClassButtons();
			WireTierButtons(_tier1Root, 1);
			WireTierButtons(_tier2Root, 2);

			// Limit the options shown visually
			LimitOptionsShown(_tier1Root, Tier1OptionsShown);
			LimitOptionsShown(_tier2Root, Tier2OptionsShown);

			// Build loadout slots list (use existing children or create)
			BuildOrCollectLoadoutSlots();

			UpdateLoadoutVisuals();
		}

		private void WireClassButtons()
		{
			foreach (var child in _classRoot.GetChildren())
			{
				if (child is Button b)
					b.Pressed += () => OnClassPicked(b);
			}
		}

		private void WireTierButtons(Container root, int tier)
		{
			foreach (var child in root.GetChildren())
			{
				if (child is Button b)
					b.Pressed += () => OnTierPicked(tier, b);
			}
		}

		private void LimitOptionsShown(Container root, int shown)
		{
			int i = 0;
			foreach (var child in root.GetChildren())
			{
				if (child is Control c)
					c.Visible = i < shown;
				i++;
			}
		}

		private void BuildOrCollectLoadoutSlots()
		{
			// Always rebuild from scenes so we guarantee DEF/UP graphics are present
			if (DefensiveSlotScene == null || UpgradeSlotScene == null)
			{
				GD.PushError("[PlayerLoadoutPanel] DefensiveSlotScene / UpgradeSlotScene are not assigned in the Inspector.");
				return;
			}

			// Clear anything that was in the container
			foreach (var child in _loadoutRoot.GetChildren())
				child.QueueFree();

			_loadoutSlots.Clear();

			for (int i = 0; i < LoadoutSlotCount; i++)
			{
				var scene = (i == LoadoutSlotCount - 1) ? UpgradeSlotScene : DefensiveSlotScene;
				var inst = scene.Instantiate<Control>();
				_loadoutRoot.AddChild(inst);
				_loadoutSlots.Add(inst);
			}
		}

		// ---------------- Click handlers ----------------

		private void OnClassPicked(Button btn)
		{
			GD.Print($"[Click:PlayerLoadoutPanel] class on {btn.Name}");
			var tex = FindFirstTexture(btn);
			if (tex == null)
			{
				GD.Print($"[Click] No texture found in class {btn.Name}");
				return;
			}
			_classPick = tex;
			UpdateLoadoutVisuals();
		}

		private void OnTierPicked(int tier, Button btn)
		{
			var tex = FindFirstTexture(btn);
			if (tex == null)
			{
				GD.Print($"[Click] No texture found in {btn.Name}");
				return;
			}

			if (tier == 1)
			{
				if (!_tier1Picks.Contains(tex))
				{
					_tier1Picks.Add(tex);
					if (_tier1Picks.Count > Tier1PickCount)
						_tier1Picks.RemoveAt(0);
				}
				GD.Print($"[Click:PlayerLoadoutPanel] tier=1 on {btn.Name}");
			}
			else
			{
				if (!_tier2Picks.Contains(tex))
				{
					_tier2Picks.Add(tex);
					if (_tier2Picks.Count > Tier2PickCount)
						_tier2Picks.RemoveAt(0);
				}
				GD.Print($"[Click:PlayerLoadoutPanel] tier=2 on {btn.Name}");
			}

			UpdateLoadoutVisuals();
		}

		// ---------------- Painting the loadout ----------------

		private void UpdateLoadoutVisuals()
		{
			if (_loadoutSlots.Count == 0) return;

			int last = _loadoutSlots.Count - 1;

			// 1) Reset **all** slots to their scene-default texture (DEF or UPG)
			for (int i = 0; i < _loadoutSlots.Count; i++)
			{
				var defTex = ExtractDefaultTexture(_loadoutSlots[i]);
				SetSlotTexture(i, defTex);
			}

			// 2) Put class in slot 0 (if chosen)
			if (_classPick != null)
				SetSlotTexture(0, _classPick);

			// 3) Fill spells into slots 1..last-1 (last is always the UPGRADE tile)
			int cursor = 1;
			foreach (var t in _tier1Picks)
			{
				if (cursor >= last) break;
				SetSlotTexture(cursor++, t);
			}
			foreach (var t in _tier2Picks)
			{
				if (cursor >= last) break;
				SetSlotTexture(cursor++, t);
			}
		}

		private void SetSlotTexture(int index, Texture2D tex)
		{
			if (index < 0 || index >= _loadoutSlots.Count) return;
			var node = _loadoutSlots[index];

			switch (node)
			{
				case TextureRect tr:
					tr.Texture = tex;
					break;
				case Button b:
					b.Icon = tex;
					break;
				case TextureButton tb:
					tb.TextureNormal = tex;
					break;
			}
		}

		private Texture2D ExtractDefaultTexture(Control c)
		{
			// Returns whatever texture/icon the slot came with (DEF or UPG),
			// so baseline fill preserves your scenes’ artwork.
			return c switch
			{
				TextureRect tr when tr.Texture is Texture2D t => t,
				Button b when b.Icon is Texture2D t => t,
				TextureButton tb when tb.TextureNormal is Texture2D t => t,
				_ => null
			};
		}

		// Try to get a texture from various control types
		private Texture2D FindFirstTexture(Node n)
		{
			if (n is Button b && b.Icon is Texture2D t1) return t1;
			if (n is TextureButton tb && tb.TextureNormal is Texture2D t2) return t2;
			if (n is TextureRect tr && tr.Texture is Texture2D t3) return t3;

			// look into first child TextureRect/Button/TextureButton
			foreach (var child in n.GetChildren())
			{
				var tex = FindFirstTexture(child);
				if (tex != null) return tex;
			}
			return null;
		}
	}
}
