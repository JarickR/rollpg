// res://Scripts/Godot/PlayerLoadoutPanel.cs
#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiceArena.Godot
{
	public partial class PlayerLoadoutPanel : Control
	{
		// ---- Inspector paths ----
		[Export] public NodePath ClassSelectionContainerPath { get; set; } = default!;
		[Export] public NodePath Tier1SpellContainerPath   { get; set; } = default!;
		[Export] public NodePath Tier2SpellContainerPath   { get; set; } = default!;
		[Export] public NodePath LoadoutContainerPath      { get; set; } = default!;

		// Scenes for DEF/UPGRADE tiles
		[Export] public PackedScene UpgradeSlotScene   { get; set; } = default!;
		[Export] public PackedScene DefensiveSlotScene { get; set; } = default!;

		// Limits (shown vs picks)
		[Export] public int Tier1OptionsShown { get; set; } = 3;
		[Export] public int Tier2OptionsShown { get; set; } = 2;
		[Export] public int Tier1PickCount    { get; set; } = 2;
		[Export] public int Tier2PickCount    { get; set; } = 1;
		[Export] public int LoadoutSlotCount  { get; set; } = 6;

		// ---- Runtime refs ----
		private Container _classRoot   = default!;
		private Container _tier1Root   = default!;
		private Container _tier2Root   = default!;
		private Container _loadoutRoot = default!;

		// Chosen visuals
		private Texture2D? _classPick;
		private readonly List<Texture2D> _tier1Picks = new();
		private readonly List<Texture2D> _tier2Picks = new();

		// Chosen class display name (used by bridge to label Hero 1)
		private string? _className;
		public string? GetSelectedClassName() => _className;

		// Loadout slot nodes (TextureRect / Button / TextureButton)
		private readonly List<Control> _loadoutSlots = new();

		public override void _Ready()
		{
			_classRoot   = GetNode<Container>(ClassSelectionContainerPath);
			_tier1Root   = GetNode<Container>(Tier1SpellContainerPath);
			_tier2Root   = GetNode<Container>(Tier2SpellContainerPath);
			_loadoutRoot = GetNode<Container>(LoadoutContainerPath);

			WireButtons(_classRoot, 0);
			WireButtons(_tier1Root, 1);
			WireButtons(_tier2Root, 2);

			// Build loadout tiles now so layout exists
			BuildOrCollectLoadoutSlots();

			// Defer so children are fully ready before we toggle visibility
			CallDeferred(nameof(ApplyLimitsAndRefresh));
		}

		private void ApplyLimitsAndRefresh()
		{
			RandomizeOptionsShown(_tier1Root, Math.Max(0, Tier1OptionsShown), seedSalt: 101);
			RandomizeOptionsShown(_tier2Root, Math.Max(0, Tier2OptionsShown), seedSalt: 202);
			UpdateLoadoutVisuals();
		}

		// ---------------- wiring ----------------

		private void WireButtons(Container root, int tier)
		{
			int btn = 0, tbtn = 0, other = 0;
			foreach (var child in root.GetChildren())
			{
				switch (child)
				{
					case Button b:
						btn++;
						if (tier == 0) b.Pressed += () => OnClassPicked(b);
						else           b.Pressed += () => OnTierPicked(tier, b);
						break;

					case TextureButton tb:
						tbtn++;
						if (tier == 0) tb.Pressed += () => OnClassPicked(tb);
						else            tb.Pressed += () => OnTierPicked(tier, tb);
						break;

					default:
						if (child is Control) other++;
						break;
				}
			}
			string label = tier switch { 0 => "Class", 1 => "Tier1", 2 => "Tier2", _ => $"Tier{tier}" };
			GD.Print($"[PlayerLoadoutPanel] WireTiles[{label}] -> Buttons={btn}, TextureButtons={tbtn}, Controls={other}");
		}

		/// <summary>
		/// Randomly selects which option tiles are visible (without reordering nodes).
		/// </summary>
		private void RandomizeOptionsShown(Container root, int shown, int seedSalt = 0)
		{
			var tiles = root.GetChildren()
				.OfType<Control>()
				.Where(c => c is Button || c is TextureButton)
				.ToList();

			if (tiles.Count == 0)
			{
				GD.Print($"[PlayerLoadoutPanel] Randomize: no selectable tiles under {root.GetPath()}");
				return;
			}

			int seed =
				(int)(Time.GetTicksUsec() & 0x7FFFFFFF) ^
				(int)(GetInstanceId() & 0x7FFFFFFF) ^
				seedSalt;

			var rng = new System.Random(seed);

			var idx = Enumerable.Range(0, tiles.Count).ToArray();
			for (int i = idx.Length - 1; i > 0; i--)
			{
				int j = rng.Next(i + 1);
				(idx[i], idx[j]) = (idx[j], idx[i]);
			}

			int allow = Math.Clamp(shown, 0, tiles.Count);
			var visibleSet = new HashSet<int>(idx.Take(allow));

			for (int i = 0; i < tiles.Count; i++)
				tiles[i].Visible = visibleSet.Contains(i);

			var hiddenNames = tiles.Where((_, i) => !visibleSet.Contains(i)).Select(t => t.Name);
			GD.Print($"[PlayerLoadoutPanel] Shown {allow}/{tiles.Count} → hidden [{string.Join(", ", hiddenNames)}]");
		}

		private void BuildOrCollectLoadoutSlots()
		{
			if (DefensiveSlotScene == null || UpgradeSlotScene == null)
			{
				GD.PushError("[PlayerLoadoutPanel] DefensiveSlotScene / UpgradeSlotScene are not assigned in the Inspector.");
				return;
			}

			foreach (var child in _loadoutRoot.GetChildren())
				child.QueueFree();

			_loadoutSlots.Clear();

			for (int i = 0; i < LoadoutSlotCount; i++)
			{
				var scene = (i == LoadoutSlotCount - 1) ? UpgradeSlotScene : DefensiveSlotScene;
				var inst  = scene.Instantiate<Control>();
				_loadoutRoot.AddChild(inst);
				_loadoutSlots.Add(inst);
			}
		}

		// ---------------- Click handlers ----------------

		private void OnClassPicked(Node src)
		{
			var tex = FindFirstTexture(src);
			if (tex == null)
			{
				GD.Print($"[Click] No texture found in class {src.Name}");
				return;
			}
			_classPick = tex;

			// Store display name (use node name as the label)
			_className = src.Name;

			GD.Print($"[Click:PlayerLoadoutPanel] class on {src.Name}");
			UpdateLoadoutVisuals();
		}

		private void OnTierPicked(int tier, Node src)
		{
			var tex = FindFirstTexture(src);
			if (tex == null)
			{
				GD.Print($"[Click] No texture found in {src.Name}");
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
				GD.Print($"[Click:PlayerLoadoutPanel] tier=1 on {src.Name}");
			}
			else
			{
				if (!_tier2Picks.Contains(tex))
				{
					_tier2Picks.Add(tex);
					if (_tier2Picks.Count > Tier2PickCount)
						_tier2Picks.RemoveAt(0);
				}
				GD.Print($"[Click:PlayerLoadoutPanel] tier=2 on {src.Name}");
			}

			UpdateLoadoutVisuals();
		}

		// ---------------- Painting the loadout ----------------

		private void UpdateLoadoutVisuals()
		{
			if (_loadoutSlots.Count == 0) return;

			int last = _loadoutSlots.Count - 1;

			// 1) Reset all slots to their scene-default texture (DEF or UPG)
			for (int i = 0; i < _loadoutSlots.Count; i++)
			{
				var defTex = ExtractDefaultTexture(_loadoutSlots[i]);
				SetSlotTexture(i, defTex);
			}

			// 2) Put class in slot 0 (if chosen)
			if (_classPick != null)
				SetSlotTexture(0, _classPick);

			// 3) Fill spells into slots 1..last-1 (last is the UPGRADE tile)
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

		private void SetSlotTexture(int index, Texture2D? tex)
		{
			if (index < 0 || index >= _loadoutSlots.Count) return;
			if (tex == null) return;

			switch (_loadoutSlots[index])
			{
				case TextureRect tr:     tr.Texture       = tex; break;
				case Button b:           b.Icon           = tex; break;
				case TextureButton tb:   tb.TextureNormal = tex; break;
			}
		}

		private static Texture2D? ExtractDefaultTexture(Control c)
		{
			return c switch
			{
				TextureRect tr   when tr.Texture is Texture2D t      => t,
				Button b         when b.Icon          is Texture2D t => t,
				TextureButton tb when tb.TextureNormal is Texture2D t=> t,
				_ => null
			};
		}

		private static Texture2D? FindFirstTexture(Node n)
		{
			if (n is Button b && b.Icon is Texture2D t1) return t1;
			if (n is TextureButton tb && tb.TextureNormal is Texture2D t2) return t2;
			if (n is TextureRect tr && tr.Texture is Texture2D t3) return t3;

			foreach (var child in n.GetChildren())
			{
				var tex = FindFirstTexture(child);
				if (tex != null) return tex;
			}
			return null;
		}

		// ===================== PUBLIC API (for bridge/HUD) =====================

		public Texture2D? GetClassPickTexture() => _classPick;
		public IReadOnlyList<Texture2D> GetTier1Picks() => _tier1Picks.ToArray();
		public IReadOnlyList<Texture2D> GetTier2Picks() => _tier2Picks.ToArray();

		public void GetChosenTextures(out Texture2D? classIcon, out List<Texture2D> tier1, out List<Texture2D> tier2)
		{
			classIcon = _classPick;
			tier1 = new List<Texture2D>(_tier1Picks);
			tier2 = new List<Texture2D>(_tier2Picks);
		}
	}
}
