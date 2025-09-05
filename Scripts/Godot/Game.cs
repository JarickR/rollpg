// res://Scripts/Godot/Game.cs
using Godot;
using System;
using System.Collections.Generic;
using DiceArena.Engine;
using DiceArena.GodotUI;

namespace DiceArena.GodotApp
{
	public partial class Game : Control
	{
		// Engine state
		private GameState _state = new();

		// UI roots / controls
		private VBoxContainer _root;
		private VBoxContainer _logBox;        // scrollable list of labels (last 5)
		private Button _rollButton;
		private HBoxContainer _enemyRow;
		private HBoxContainer _heroRow;

		// Overlays
		private LoadoutScreen _loadout;
		private RollPopup _rollPopup;

		public override void _Ready()
		{
			Name = "Game";
			AnchorLeft = 0; AnchorTop = 0; AnchorRight = 1; AnchorBottom = 1;
			OffsetLeft = 0; OffsetTop = 0; OffsetRight = 0; OffsetBottom = 0;

			BuildUi();
			ShowLoadout();
		}

		// --------------------------------------------------------------------
		// UI BUILD
		// --------------------------------------------------------------------
		private void BuildUi()
		{
			_root = UiUtils.VBox(10);
			_root.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			_root.SizeFlagsVertical   = Control.SizeFlags.ExpandFill;
			AddChild(_root);

			// Log panel (we’ll keep max 5 Label children)
			_logBox = UiUtils.VBox(4);
			_root.AddChild(UiUtils.MakeTitledPanel("Log", _logBox));

			// Roll button (disabled until game starts)
			_rollButton = new Button
			{
				Text = "Roll",
				Disabled = true,
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};
			_rollButton.Pressed += OnRollPressed;
			_root.AddChild(_rollButton);

			// Enemy & Hero rows
			_enemyRow = UiUtils.HBox(8);
			_heroRow  = UiUtils.HBox(8);
			_root.AddChild(UiUtils.MakeTitledPanel("Enemies", _enemyRow));
			_root.AddChild(UiUtils.MakeTitledPanel("Heroes", _heroRow));

			// Popup (created once, reused)
			_rollPopup = new RollPopup();
			AddChild(_rollPopup);
		}

		private void ShowLoadout()
		{
			_loadout = new LoadoutScreen();
			AddChild(_loadout);
			_loadout.OnConfirm += StartGameFromLoadout;
			Log("Select party size and spells, then Finalize & Start.");
		}

		// --------------------------------------------------------------------
		// LOADOUT -> GAME START
		// --------------------------------------------------------------------
		private void StartGameFromLoadout(int partySize, List<Spell> spellsForP1)
		{
			// Remove loadout overlay
			_loadout.OnConfirm -= StartGameFromLoadout;
			_loadout.QueueFree();

			// Clear any previous state/ui
			_state = new GameState();
			_enemyRow.QueueFreeChildren();
			_heroRow.QueueFreeChildren();

			// Build heroes (for demo: everyone gets same chosen loadout)
			for (int i = 0; i < partySize; i++)
			{
				var hero = new Hero($"P{i+1}", "thief") { MaxHp = 20, Hp = 20, Armor = 0 };
				hero.Spells.AddRange(spellsForP1);
				_state.Players.Add(hero);

				var card = new HeroCard();
				_heroRow.AddChild(card);
				card.Bind(hero);
			}

			// Add sample enemies equal to party size (tier 1)
			for (int i = 0; i < partySize; i++)
			{
				var e = new Enemy($"Goblin {i+1}", 1) { MaxHp = 10, Hp = 10, Armor = 0 };
				_state.Enemies.Add(e);

				var ecard = new EnemyCard();
				_enemyRow.AddChild(ecard);
				ecard.Bind(e);
			}

			_rollButton.Disabled = false;
			Log($"Battle ready: {partySize} heroes vs {partySize} enemies.");
		}

		// --------------------------------------------------------------------
		// ROLL / TURN (simple preview)
		// --------------------------------------------------------------------
		private readonly Random _rng = new();

		private void OnRollPressed()
		{
			if (_state.Players.Count == 0)
			{
				Log("No heroes.");
				return;
			}

			// For now, always use hero 1
			var hero = _state.Players[0];
			if (hero.Hp <= 0)
			{
				Log($"{hero.Name} cannot act (down).");
				return;
			}

			// Simple 1..6
			int face = _rng.Next(1, 7);
			_rollPopup.ShowText($"Rolled {face}");
			Log($"{hero.Name} rolled {face}");

			// Demo: apply small damage to first alive enemy
			var target = _state.Enemies.Find(e => e.Hp > 0);
			if (target != null)
			{
				target.Hp = Math.Max(0, target.Hp - face); // fake damage = die face
				Log($"{hero.Name} hits {target.Name} for {face}. HP {target.Hp}/{target.MaxHp}");

				// Refresh the enemy card UI (bound cards will pull values from models)
				foreach (var child in _enemyRow.GetChildren())
				{
					if (child is EnemyCard ec && ec.Data == target)
					{
						ec.Refresh();
						break;
					}
				}

				if (target.Hp <= 0)
					Log($"{target.Name} is defeated!");
			}
			else
			{
				Log("No valid targets.");
			}
		}

		// --------------------------------------------------------------------
		// LOGGING (max 5 lines)
		// --------------------------------------------------------------------
		private void Log(string message)
		{
			var label = UiUtils.MakeLabel(message, 14);
			_logBox.AddChild(label);

			while (_logBox.GetChildCount() > 5)
			{
				var first = _logBox.GetChild(0);
				first.QueueFree();
			}
		}
	}

	// Small helper to free all children of a container cleanly
	internal static class NodeExtensions
	{
		public static void QueueFreeChildren(this Node node)
		{
			foreach (var child in node.GetChildren())
				(child as Node)?.QueueFree();
		}
	}
}
