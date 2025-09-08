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

		// Freeze: number of actions to skip per enemy
		private readonly Dictionary<Enemy, int> _freezeCounters = new();

		// Simple active index for whose turn (we only have one hero rolling right now)
		private int _round = 1;

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
			_logPanel.CustomMinimumSize = new Vector2(0, 160);
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
			_freezeCounters.Clear();
			_round = 1;

			// Heroes
			for (int i = 0; i < players.Count; i++)
			{
				var p = players[i];
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
				hero.SpikedThorns = 0;
				hero.ConcentrationStacks = 0;
				hero.Defense = null;

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
			Log($"Round {_round} — Battle ready: {_state.Players.Count} heroes vs {_state.Enemies.Count} enemies.");
			_rollPopup.ShowText("Heroes: Roll to start!");
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

		private async void OnRollPressed()
		{
			// Simple: first alive hero acts
			var hero = _state.Players.FirstOrDefault(h => h.Hp > 0);
			if (hero == null) { Log("No heroes can act."); return; }

			// New round housekeeping: clear last-turn unused defense at start of hero turn
			if (hero.Defense != null) hero.Defense = null;

			int slot;
			var face = RollDie(out slot);

			switch (face)
			{
				case DieFace.Class:   ShowClassAbility(hero); break;
				case DieFace.Upgrade: ShowUpgradeChooser(hero); break;
				default:              ResolveSpellFace(hero, slot); break;
			}

			// After hero acts, enemies take their turn
			_rollButton.Disabled = true;
			await EnemyTurn();
			_rollButton.Disabled = false;

			_round++;
			Log($"Round {_round} — Heroes, roll!");
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

			if (!can.Any(x => x))
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

			// Refresh hero card faces
			RefreshHeroCard(hero);
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

			// Concentration doubling: only for magic kinds
			int multiplier = 1;
			if (hero.ConcentrationStacks > 0 && IsMagicKind(kind))
			{
				multiplier = 2;
				hero.ConcentrationStacks = Math.Max(0, hero.ConcentrationStacks - 1);
				Log($"{hero.Name}'s Concentration doubles the effect!");
			}

			_rollPopup.ShowText($"{hero.Name} rolled {PrettyName(kind, tier)}");

			switch (kind)
			{
				case "attack":      ApplySingleTargetDamage(hero, dmg: 2 * tier * multiplier, bypassArmor:false); break;
				case "fireball":    ApplySingleTargetDamage(hero, dmg: (tier == 1 ? 1 : (tier == 2 ? 3 : 5)) * multiplier, bypassArmor:true);  break;
				case "sweep":       ApplyAoeDamage(hero, dmg: tier * multiplier, bypassArmor:false);               break;

				case "heal":
					{
						int amt = (tier == 1 ? 2 : tier == 2 ? 4 : 6) * multiplier;
						hero.Heal(amt);
						Log($"{hero.Name} heals {amt}. HP {hero.Hp}/{hero.MaxHp}");
						RefreshHeroCard(hero);
					}
					break;

				case "armor":
					{
						int ar = (tier == 1 ? 2 : tier == 2 ? 4 : 6) * multiplier;
						hero.AddArmor(ar);
						Log($"{hero.Name} gains {ar} Armor. AR {hero.Armor}");
						RefreshHeroCard(hero);
					}
					break;

				case "chain":
					ApplyChainLightning(hero, tier, multiplier);
					break;

				case "freeze":
					ApplyFreeze(hero, tier * multiplier);
					break;

				case "spiked":
					ApplySpikedShield(hero, tier * multiplier);
					break;

				case "poison":
					{
						int stacks = 1 * multiplier;
						var t = FirstAliveEnemy();
						if (t != null) { t.PoisonStacks += stacks; Log($"{hero.Name} applies Poison ×{stacks} to {t.Name} (stacks {t.PoisonStacks})."); RefreshEnemyCard(t); }
						else Log("No valid targets.");
					}
					break;

				case "bomb":
					{
						int stacks = 1 * multiplier;
						var t = FirstAliveEnemy();
						if (t != null) { t.BombStacks += stacks; Log($"{hero.Name} plants Bomb ×{stacks} on {t.Name} (stacks {t.BombStacks})."); RefreshEnemyCard(t); }
						else Log("No valid targets.");
					}
					break;

				case "concentration":
					hero.ConcentrationStacks += 1; // one use = doubles next magic
					Log($"{hero.Name} gains Concentration (x{hero.ConcentrationStacks}).");
					break;

				default:
					Log($"{hero.Name} rolled {s.Name} (no specific effect hooked up).");
					break;
			}
		}

		// ---- Helpers: spell kinds ----
		private static bool IsMagicKind(string kind)
			=> kind is "fireball" or "chain" or "freeze" or "poison" or "bomb" or "concentration";

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

		// ----------------- ENEMY TURN / STATUSES -----------------

		private async System.Threading.Tasks.Task EnemyTurn()
		{
			_rollPopup.ShowText("Enemies act…");

			// 1) Status ticks on enemies (Poison, Bomb)
			ProcessEnemyStatuses();

			// 2) Each alive enemy acts once
			foreach (var enemy in _state.Enemies.ToList())
			{
				if (enemy.Hp <= 0) continue;

				// Freeze skip?
				if (_freezeCounters.TryGetValue(enemy, out var skips) && skips > 0)
				{
					_freezeCounters[enemy] = skips - 1;
					Log($"{enemy.Name} is frozen and skips an action. ({skips - 1} left)");
					continue;
				}

				// Choose a target hero: first alive hero for now
				var target = _state.Players.FirstOrDefault(h => h.Hp > 0);
				if (target == null) { Log("All heroes are down!"); break; }

				// Physical attack baseline (2 damage)
				int dmg = 2;

				// Check target's physical defense (Dodge)
				if (!ResolveIncomingPhysicalDefense(target))
				{
					Log($"{enemy.Name}'s attack misses {target.Name}!");
					continue;
				}

				// Apply damage (respect Armor)
				int hpBefore = target.Hp;
				target.Damage(dmg, bypassArmor: false);
				Log($"{enemy.Name} strikes {target.Name} for {dmg}. {target.Name} HP {target.Hp}/{target.MaxHp}");
				RefreshHeroCard(target);

				// Thorns from Spiked Shield reflect to the attacker
				if (target.SpikedThorns > 0 && enemy.Hp > 0)
				{
					int thorn = target.SpikedThorns;
					enemy.Damage(thorn, bypassArmor: true);
					Log($"{target.Name}'s Spiked Shield reflects {thorn} back to {enemy.Name} (ignores Armor).");
					RefreshEnemyCard(enemy);
					if (enemy.Hp <= 0)
					{
						Log($"{enemy.Name} is defeated by thorns!");
					}
				}

				// Tiny delay so actions are readable
				await ToSignal(GetTree().CreateTimer(0.3f), SceneTreeTimer.SignalName.Timeout);
			}

			// 3) Cleanup dead enemies
			_state.Enemies.RemoveAll(e => e.Hp <= 0);
			foreach (var child in _enemyRow.GetChildren().ToArray())
			{
				if (child is EnemyCard ec && ec.Data.Hp <= 0) { ec.QueueFree(); }
			}
			if (_state.Enemies.Count == 0)
			{
				_rollPopup.ShowText("Victory!");
				Log("All enemies defeated!");
				_rollButton.Disabled = true;
			}
		}

		private void ProcessEnemyStatuses()
		{
			if (_state.Enemies.Count == 0) return;

			// We’ll manage Bomb passing using enemy index adjacency (wrap-around)
			for (int i = 0; i < _state.Enemies.Count; i++)
			{
				var e = _state.Enemies[i];
				if (e.Hp <= 0) continue;

				// Poison: for each stack, roll d6: 1-4 deal 1 (ignore armor), 5-6 cure 1
				if (e.PoisonStacks > 0)
				{
					int totalDmg = 0;
					int cures = 0;
					int stacksToProcess = e.PoisonStacks;
					for (int s = 0; s < stacksToProcess; s++)
					{
						int roll = _rng.Next(1,7);
						if (roll <= 4) totalDmg += 1; else cures += 1;
					}
					e.PoisonStacks = Math.Max(0, e.PoisonStacks - cures);
					if (totalDmg > 0)
					{
						e.Damage(totalDmg, bypassArmor: true);
					}
					if (totalDmg > 0 || cures > 0)
					{
						Log($"{e.Name} Poison: {totalDmg} dmg, {cures} cure(s). (stacks {e.PoisonStacks})");
						RefreshEnemyCard(e);
					}
				}

				// Bomb: for each stack, roll d6 → 1-2 explode (6 dmg to holder),
				// 3-4 pass right, 5-6 pass left (wrap).
				if (e.BombStacks > 0)
				{
					int stacks = e.BombStacks;
					int remain = stacks;
					for (int b = 0; b < stacks; b++)
					{
						int roll = _rng.Next(1,7);
						if (roll <= 2)
						{
							// explode
							e.Damage(6, bypassArmor: true);
							Log($"{e.Name}'s Bomb explodes for 6 (ignores Armor)!");
							remain--;
							if (e.Hp <= 0) break;
						}
						else
						{
							// pass
							int idx = i;
							if (roll <= 4)
							{
								// pass right
								idx = (i + 1) % _state.Enemies.Count;
								Log($"{e.Name}'s Bomb passes right to {_state.Enemies[idx].Name}.");
							}
							else
							{
								// pass left
								idx = (i - 1 + _state.Enemies.Count) % _state.Enemies.Count;
								Log($"{e.Name}'s Bomb passes left to {_state.Enemies[idx].Name}.");
							}
							_state.Enemies[idx].BombStacks += 1;
							remain--;
						}
					}
					e.BombStacks = Math.Max(0, remain);
					RefreshEnemyCard(e);
				}
			}
		}

		// ----------------- SPELL IMPLEMENTATIONS -----------------

		private Enemy? FirstAliveEnemy() => _state.Enemies.Find(e => e.Hp > 0);

		private void ApplySingleTargetDamage(Hero hero, int dmg, bool bypassArmor)
		{
			var target = FirstAliveEnemy();
			if (target == null) { Log("No valid targets."); return; }

			// NOTE: If you later make enemies cast spells to heroes, call ResolveIncomingSpellDefense before applying.
			target.Damage(dmg, bypassArmor);
			Log($"{hero.Name} hits {target.Name} for {dmg}" + (bypassArmor ? " (bypass armor)" : "") + $". HP {target.Hp}/{target.MaxHp}");
			RefreshEnemyCard(target);
			if (target.Hp <= 0) Log($"{target.Name} is defeated!");
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

		private void ApplyChainLightning(Hero hero, int tier, int multiplier)
		{
			var alive = _state.Enemies.Where(e => e.Hp > 0).ToList();
			if (alive.Count == 0) { Log("No valid targets."); return; }

			int dmg = (tier == 1 ? 1 : tier == 2 ? 2 : 3) * multiplier;

			if (tier == 3)
			{
				foreach (var e in alive)
				{
					e.Damage(dmg, bypassArmor: true);
					RefreshEnemyCard(e);
				}
				Log($"{hero.Name}'s Chain Lightning++ arcs to all enemies for {dmg} (ignores Armor).");
				return;
			}

			int hit = 0;
			foreach (var e in alive)
			{
				e.Damage(dmg, bypassArmor: true);
				RefreshEnemyCard(e);
				hit++;
				if (hit >= 3) break; // up to 3 total targets
			}
			Log($"{hero.Name}'s Chain Lightning{(tier==2?"+":"")} hits {hit} target(s) for {dmg} each (ignores Armor).");
		}

		private void ApplyFreeze(Hero hero, int skipCount)
		{
			var t = FirstAliveEnemy();
			if (t == null) { Log("No valid targets."); return; }

			if (!_freezeCounters.ContainsKey(t)) _freezeCounters[t] = 0;
			_freezeCounters[t] += skipCount;
			Log($"{hero.Name} freezes {t.Name}: they will skip {skipCount} action(s).");
			RefreshEnemyCard(t);
		}

		private void ApplySpikedShield(Hero hero, int value)
		{
			hero.AddArmor(value);
			hero.SpikedThorns = Math.Max(hero.SpikedThorns, value);
			Log($"{hero.Name} readies Spiked Shield: +{value} Armor and {value} thorns on melee hits.");
			RefreshHeroCard(hero);
		}

		// ----------------- UI REFRESH / LOG -----------------

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

			while (_logBox.GetChildCount() > 7)
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

		// ---- Name parsing supports new kinds ----
		private static bool TryInferKindTier(Spell s, out string kind, out int tier)
		{
			var name = (s?.Name ?? "").Trim().ToLowerInvariant();

			int pluses = 0;
			if (name.EndsWith("++")) { pluses = 2; name = name[..^2]; }
			else if (name.EndsWith("+")) { pluses = 1; name = name[..^1]; }
			name = name.Replace(" ", "");

			string baseKind = name switch
			{
				"defend"              => "defend",
				"attack"              => "attack",
				"sweep"               => "sweep",
				"heal"                => "heal",
				"armor" or "shield"   => "armor",
				"fireball"            => "fireball",
				"poison"              => "poison",
				"bomb"                => "bomb",
				"concentrate" or "concentration" => "concentration",
				"chainlightning"      => "chain",
				"freeze"              => "freeze",
				"spikedshield"        => "spiked",
				_ => ""
			};

			if (string.IsNullOrEmpty(baseKind)) { kind = ""; tier = 1; return false; }
			kind = baseKind; tier = 1 + pluses;
			return true;
		}

		private static string PrettyName(string kind, int tier)
			=> tier switch { 1 => Title(kind), 2 => $"{Title(kind)}+", 3 => $"{Title(kind)}++", _ => Title(kind) };

		private static string Title(string kind)
		{
			return kind switch
			{
				"chain"   => "Chain Lightning",
				"freeze"  => "Freeze",
				"spiked"  => "Spiked Shield",
				_ => kind.Length == 0 ? "" : char.ToUpper(kind[0]) + (kind.Length > 1 ? kind.Substring(1) : "")
			};
		}
	}
}
