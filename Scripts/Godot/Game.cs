// res://Scripts/Godot/Game.cs
#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using DiceArena.Engine;
using DiceArena.GodotUI;
using DiceArena.GameData;
using DiceArena.Data;

namespace DiceArena.GodotApp
{
	public partial class Game : Control
	{
		private GameState _state = new();

		private VBoxContainer _root = default!;
		private Panel _logPanel = default!;
		private ScrollContainer _logScroll = default!;
		private VBoxContainer _logBox = default!;

		private Button _rollButton = default!;
		private HBoxContainer _enemyRow = default!;
		private HBoxContainer _heroRow = default!;

		private LoadoutScreen _loadout = default!;
		private RollPopup _rollPopup = default!;

		private readonly Random _rng = new();

		public override void _Ready()
		{
			Name = "Game";
			AnchorLeft = 0; AnchorTop = 0; AnchorRight = 1; AnchorBottom = 1;

			GameDataRegistry.EnsureLoaded();
			BuildUi();
			ShowLoadout();
		}

		private void BuildUi()
		{
			_root = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
			_root.AddThemeConstantOverride("separation", 12);
			AddChild(_root);

			_logBox = new VBoxContainer(); _logBox.AddThemeConstantOverride("separation", 4);
			_logScroll = new ScrollContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
			_logScroll.AddChild(_logBox);
			_logPanel = UiUtils.MakeTitledPanel("Log", _logScroll);
			_logPanel.CustomMinimumSize = new Vector2(0, 140);
			_logPanel.SizeFlagsVertical = SizeFlags.ShrinkBegin;
			_root.AddChild(_logPanel);

			_rollButton = new Button { Text = "Roll", Disabled = true, SizeFlagsHorizontal = SizeFlags.ExpandFill };
			_rollButton.Pressed += OnRollPressed;
			_root.AddChild(_rollButton);

			_enemyRow = UiUtils.HBox(12);
			_enemyRow.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			_enemyRow.SizeFlagsVertical   = SizeFlags.ShrinkCenter;

			_heroRow  = UiUtils.HBox(12);
			_heroRow.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			_heroRow.SizeFlagsVertical   = SizeFlags.ShrinkCenter;

			_root.AddChild(UiUtils.MakeTitledPanel("Enemies", _enemyRow));
			_root.AddChild(UiUtils.MakeTitledPanel("Heroes",  _heroRow));

			_rollPopup = new RollPopup();
			AddChild(_rollPopup);
		}

		private void ShowLoadout()
		{
			_loadout = new LoadoutScreen();
			AddChild(_loadout);
			_loadout.OnConfirm += StartGameFromLoadout;
			Log("Choose each player's Class and faces; unused slots become Defensive faces.");
		}

		private void StartGameFromLoadout(List<LoadoutScreen.PlayerSetup> players)
		{
			_loadout.OnConfirm -= StartGameFromLoadout;
			_loadout.QueueFree();

			_state = new GameState();
			_enemyRow.QueueFreeChildren();
			_heroRow.QueueFreeChildren();

			// Build heroes from selections
			for (int i = 0; i < players.Count; i++)
			{
				var p = players[i];
				// FIX: use (id) ctor, then set ClassId via initializer
				var hero = new Hero($"P{i+1}")
				{
					Name = $"P{i+1}",
					ClassId = p.ClassId,
					MaxHp = 20,
					Hp = 20,
					Armor = 0
				};
				hero.ClearLoadoutToBlanks();
				hero.SetLoadout(p.Faces); // exactly 4
				_state.Players.Add(hero);

				var card = new HeroCard();
				_heroRow.AddChild(card);
				card.Bind(hero);
			}

			// Enemies: mirror party size for now
			for (int i = 0; i < players.Count; i++)
			{
				var e = new Enemy($"Goblin {i+1}", 1) { MaxHp = 10, Hp = 10, Armor = 0 };
				_state.Enemies.Add(e);

				var ecard = new EnemyCard();
				_enemyRow.AddChild(ecard);
				ecard.Bind(e);
			}

			_rollButton.Disabled = false;
			Log($"Battle ready: {players.Count} heroes vs {players.Count} enemies.");
		}

		// ---- Die: 6 faces ----
		private enum DieFace { Class, Upgrade, Slot0, Slot1, Slot2, Slot3 }

		private DieFace RollDie(out int slotIndex)
		{
			int n = _rng.Next(1, 7);
			slotIndex = n switch { 3 => 0, 4 => 1, 5 => 2, 6 => 3, _ => -1 };
			return n switch
			{
				1 => DieFace.Class,
				2 => DieFace.Upgrade,
				3 => DieFace.Slot0,
				4 => DieFace.Slot1,
				5 => DieFace.Slot2,
				6 => DieFace.Slot3,
				_ => DieFace.Slot0
			};
		}

		private void OnRollPressed()
		{
			// Simple: first alive hero acts
			var hero = _state.Players.FirstOrDefault(h => h.Hp > 0);
			if (hero == null) { Log("No heroes can act."); return; }

			// Clear last-turn defenses at the start of your turn
			if (hero.Defense != null && hero.Defense.Active == false)
				hero.Defense = null;

			int slot;
			var face = RollDie(out slot);

			switch (face)
			{
				case DieFace.Class:   ShowClassAbility(hero); break;
				case DieFace.Upgrade: ShowUpgradeChooser(hero); break;
				default:              ResolveSpellFace(hero, slot); break;
			}
		}

		// ---- Class ability ----
		private void ShowClassAbility(Hero hero)
		{
			var cdef = GameDataRegistry.GetClassById(hero.ClassId);
			var title = cdef?.Name ?? hero.ClassId;
			var text  = cdef?.HeroAction ?? "Class Ability";
			_rollPopup.ShowText($"{title}: Class Ability");
			Log($"[{title}] Class Ability:\n{text}");
		}

		// ---- Upgrade (chooser) ----
		private void ShowUpgradeChooser(Hero hero)
		{
			string[] names = new string[4];
			bool[] can = new bool[4];

			for (int i = 0; i < 4; i++)
			{
				var s = hero.Loadout[i];
				if (TryInferKindTier(s, out var kind, out var tier))
				{
					names[i] = PrettyName(kind, tier);
					can[i] = tier < 3 && kind != "defend"; // Defend doesn’t upgrade
				}
				else { names[i] = s?.Name ?? $"Face {i+1}"; can[i] = false; }
			}

			if (!can[0] && !can[1] && !can[2] && !can[3])
			{
				_rollPopup.ShowText("Upgrade: no eligible face");
				Log($"{hero.Name} rolled Upgrade but nothing could be upgraded.");
				return;
			}

			var chooser = new UpgradeChooser();
			chooser.Configure(names, can);
			chooser.OnChosen = (idx) => ApplyUpgradeToFace(hero, idx);
			chooser.OnCanceled = () => Log("Upgrade canceled.");
			AddChild(chooser);
		}

		private void ApplyUpgradeToFace(Hero hero, int index)
		{
			if (index < 0 || index >= hero.Loadout.Count) return;

			var s = hero.Loadout[index];
			if (!TryInferKindTier(s, out var kind, out var tier) || tier >= 3 || kind == "defend")
			{
				Log("Selected face cannot be upgraded.");
				return;
			}
			int next = tier + 1;
			var upgraded = SpellFactory.FromKind(kind, next, PrettyName(kind, next));
			hero.Loadout[index] = upgraded;

			_rollPopup.ShowText($"Upgrade: {PrettyName(kind, tier)} → {PrettyName(kind, next)}");
			Log($"{hero.Name} upgrades face {index + 1}: {PrettyName(kind, tier)} → {PrettyName(kind, next)}");
		}

		// ---- Resolve spell slot (includes Defend face) ----
		private void ResolveSpellFace(Hero hero, int slot)
		{
			if (slot < 0 || slot >= hero.Loadout.Count) { Log("Invalid slot."); return; }
			var s = hero.Loadout[slot];

			if (!TryInferKindTier(s, out var kind, out var tier))
			{
				_rollPopup.ShowText($"{hero.Name} rolled: {s.Name}");
				Log($"{hero.Name} rolled {s.Name}, but effect not recognized.");
				return;
			}

			// Defend face → roll for one-turn defense
			if (kind == "defend")
			{
				GrantRandomDefense(hero);
				return;
			}

			_rollPopup.ShowText($"{hero.Name} rolled {PrettyName(kind, tier)}");

			switch (kind)
			{
				case "attack":      ApplySingleTargetDamage(hero, dmg: 2 * tier, bypassArmor:false); break;
				case "fireball":    ApplySingleTargetDamage(hero, dmg: 2 * tier, bypassArmor:true);  break;
				case "sweep":       ApplyAoeDamage(hero, dmg: tier, bypassArmor:false);               break;

				case "heal":
					hero.Heal(2 * tier);
					Log($"{hero.Name} heals {2 * tier}. HP {hero.Hp}/{hero.MaxHp}");
					RefreshHeroCard(hero);
					break;

				case "armor":
					hero.AddArmor(2 * tier);
					Log($"{hero.Name} gains {2 * tier} Armor. AR {hero.Armor}");
					RefreshHeroCard(hero);
					break;

				case "poison":
					{
						var t = FirstAliveEnemy();
						if (t != null) { t.PoisonStacks += 1; Log($"{hero.Name} applies Poison to {t.Name} (stacks {t.PoisonStacks})."); RefreshEnemyCard(t); }
						else Log("No valid targets.");
					}
					break;

				case "bomb":
					{
						var t = FirstAliveEnemy();
						if (t != null) { t.BombStacks += 1; Log($"{hero.Name} plants a Bomb on {t.Name} (stacks {t.BombStacks})."); RefreshEnemyCard(t); }
						else Log("No valid targets.");
					}
					break;

				case "concentration":
					hero.ConcentrationStacks += 1;
					Log($"{hero.Name} gains Concentration (stacks {hero.ConcentrationStacks}).");
					break;

				default:
					Log($"{hero.Name} rolled {s.Name} (no specific effect hooked up).");
					break;
			}
		}

		// ---------------- DEFENSE SYSTEM ----------------

		private void GrantRandomDefense(Hero hero)
		{
			int roll = _rng.Next(1, 7);
			var type = (DefenseType)roll;
			hero.Defense = new DefenseToken { Type = type, Active = true };

			string text = type switch
			{
				DefenseType.Disrupt      => "Disrupt 🔄 — Cancel the next spell that targets you.",
				DefenseType.Redirect     => "Redirect ↪️ — The next spell that targets you is redirected to another player.",
				DefenseType.Delay        => "Delay ⏳ — The next spell that targets you resolves at the start of your next turn.",
				DefenseType.Counterspell => "Counterspell ✋ — Next spell that hits you deals half to you and half back to the caster.",
				DefenseType.Dodge        => "Dodge Roll — Against a physical attack: 1–3 Dodge, 4–6 Fail.",
				DefenseType.Smoke        => "Smoke Screen 🌫️ — Next spell targeting you has a 50% chance to miss.",
				_ => "Defense ready."
			};

			_rollPopup.ShowText($"Defensive Action: {text}");
			Log($"{hero.Name} gains a one-turn defensive action:\n{text}");
		}

		private bool ResolveIncomingSpellDefense(Hero target, string casterName, out string? newTargetId, out int reflectDamageHalf)
		{
			newTargetId = null;
			reflectDamageHalf = 0;

			var tok = target.Defense;
			if (tok == null || !tok.Active) return true; // no defense to apply

			switch (tok.Type)
			{
				case DefenseType.Disrupt:
					Log($"{target.Name}'s Disrupt 🔄 cancels the spell!");
					tok.Active = false; return false;

				case DefenseType.Redirect:
					var ally = _state.Players.FirstOrDefault(h => h.Hp > 0 && h.Id != target.Id);
					if (ally != null)
					{
						newTargetId = ally.Id;
						Log($"{target.Name} uses Redirect ↪️ — the spell is redirected to {ally.Name}!");
						tok.Active = false; return true;
					}
					Log($"{target.Name} tried Redirect, but no valid ally; spell proceeds.");
					tok.Active = false; return true;

				case DefenseType.Delay:
					Log($"{target.Name} uses Delay ⏳ — the spell is suspended until the start of their next turn.");
					tok.Active = false; return false;

				case DefenseType.Counterspell:
					Log($"{target.Name} uses Counterspell ✋ — half damage will reflect to {casterName}.");
					reflectDamageHalf = 1;
					tok.Active = false; return true;

				case DefenseType.Smoke:
					{
						int d6 = _rng.Next(1, 7);
						bool miss = d6 <= 3;
						Log($"{target.Name} has Smoke Screen 🌫️ — roll {d6} → {(miss ? "MISS" : "HIT")}");
						tok.Active = false;
						return !miss;
					}

				case DefenseType.Dodge:
					// Not for spells
					return true;

				default:
					return true;
			}
		}

		private bool ResolveIncomingPhysicalDefense(Hero target)
		{
			var tok = target.Defense;
			if (tok == null || !tok.Active) return true;

			if (tok.Type == DefenseType.Dodge)
			{
				int d6 = _rng.Next(1, 7);
				bool dodge = d6 <= 3;
				Log($"{target.Name} Dodge Roll — {d6} → {(dodge ? "DODGE" : "FAIL")}");
				tok.Active = false;
				return !dodge;
			}
			return true;
		}

		private Enemy? FirstAliveEnemy() => _state.Enemies.Find(e => e.Hp > 0);

		private void ApplySingleTargetDamage(Hero hero, int dmg, bool bypassArmor)
		{
			var target = FirstAliveEnemy();
			if (target == null) { Log("No valid targets."); return; }

			int before = target.Hp;
			target.Damage(dmg, bypassArmor);
			Log($"{hero.Name} hits {target.Name} for {dmg}" + (bypassArmor ? " (bypass armor)" : "") + $". HP {target.Hp}/{target.MaxHp}");
			RefreshEnemyCard(target);
			if (target.Hp <= 0 && before > 0) Log($"{target.Name} is defeated!");
		}

		private void ApplyAoeDamage(Hero hero, int dmg, bool bypassArmor)
		{
			bool any = false;
			foreach (var e in _state.Enemies.Where(x => x.Hp > 0))
			{
				any = true;
				e.Damage(dmg, bypassArmor);
				RefreshEnemyCard(e);
			}
			if (any) Log($"{hero.Name} sweeps all enemies for {dmg} each.");
			else Log("No valid targets.");
		}

		private void RefreshEnemyCard(Enemy target)
		{
			foreach (var child in _enemyRow.GetChildren())
			{
				if (child is EnemyCard ec && ec.Data == target) { ec.Refresh(); break; }
			}
		}

		private void RefreshHeroCard(Hero h)
		{
			foreach (var child in _heroRow.GetChildren())
			{
				if (child is HeroCard hc && hc.Data == h) { hc.Refresh(); break; }
			}
		}

		private void Log(string message)
		{
			var label = UiUtils.MakeLabel(message, 14);
			_logBox.AddChild(label);

			while (_logBox.GetChildCount() > 5)
			{
				var first = _logBox.GetChild(0);
				_logBox.RemoveChild(first);
				first.QueueFree();
			}
			CallDeferred(nameof(ScrollLogToBottom));
		}

		private void ScrollLogToBottom()
		{
			var vsb = _logScroll.GetVScrollBar();
			if (IsInstanceValid(vsb)) _logScroll.ScrollVertical = (int)vsb.MaxValue;
		}

		private static bool TryInferKindTier(Spell s, out string kind, out int tier)
		{
			var name = (s?.Name ?? "").Trim().ToLowerInvariant();

			int pluses = 0;
			if (name.EndsWith("++")) { pluses = 2; name = name[..^2]; }
			else if (name.EndsWith("+")) { pluses = 1; name = name[..^1]; }
			name = name.Replace(" ", "");

			string baseKind = name switch
			{
				"defend"        => "defend",
				"attack"        => "attack",
				"sweep"         => "sweep",
				"heal"          => "heal",
				"armor"         => "armor",
				"fireball"      => "fireball",
				"poison"        => "poison",
				"bomb"          => "bomb",
				"concentrate" or "concentration" => "concentration",
				_ => ""
			};

			if (string.IsNullOrEmpty(baseKind)) { kind = ""; tier = 1; return false; }
			kind = baseKind; tier = 1 + pluses;
			return true;
		}

		private static string PrettyName(string kind, int tier)
			=> tier switch { 1 => Title(kind), 2 => $"{Title(kind)}+", 3 => $"{Title(kind)}++", _ => Title(kind) };

		private static string Title(string kind)
			=> kind.Length == 0 ? "" : char.ToUpper(kind[0]) + (kind.Length > 1 ? kind.Substring(1) : "");
	}
}
