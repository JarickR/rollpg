// res://Scripts/Godot/HeroCard.cs
#nullable enable
using Godot;
using System;
using System.Linq;
using System.Reflection;
using DiceArena.Engine; // for Hero

namespace RollPG.GodotUI
{
	/// <summary>
	/// Small hero card UI. You can instance this in BattleRoot and call SetHero(hero).
	/// Scene expectations (default NodePaths below can be changed in Inspector):
	///  - NameLabel: Label
	///  - HP: ProgressBar
	///  - ARM: ProgressBar
	///  - StatusRow: HBoxContainer (icons optional)
	///  - LoadoutRow: HBoxContainer (icons optional)
	/// </summary>
	public partial class HeroCard : Control
	{
		[Export] public NodePath NameLabelPath  { get; set; } = "NameLabel";
		[Export] public NodePath HpBarPath      { get; set; } = "HP";
		[Export] public NodePath ArmBarPath     { get; set; } = "ARM";
		[Export] public NodePath StatusRowPath  { get; set; } = "StatusRow";
		[Export] public NodePath LoadoutRowPath { get; set; } = "LoadoutRow";

		private Label _name = null!;
		private ProgressBar _hp = null!;
		private ProgressBar _arm = null!;
		private HBoxContainer _status = null!;
		private HBoxContainer _loadout = null!;

		public override void _Ready()
		{
			_name   = GetNode<Label>(NameLabelPath);
			_hp     = GetNode<ProgressBar>(HpBarPath);
			_arm    = GetNode<ProgressBar>(ArmBarPath);
			_status = GetNode<HBoxContainer>(StatusRowPath);
			_loadout= GetNode<HBoxContainer>(LoadoutRowPath);
		}

		/// <summary>
		/// Convenience API used by BattleRoot. Reads common Hero fields by name.
		/// If MaxHp isn't available, uses current Hp as the max.
		/// </summary>
		public void SetHero(Hero hero)
		{
			if (hero == null) return;

			string name = TryGetString(hero, "Name") ?? "Hero";
			int hp      = TryGetInt(hero, "Hp");
			int armor   = TryGetInt(hero, "Armor");

			// Try a few common max-hp property names; fallback to current hp
			int maxHp =
				TryGetInt(hero, "MaxHp",
				TryGetInt(hero, "MaxHP",
				TryGetInt(hero, "HPMax", hp)));

			_name.Text = name;

			_hp.MaxValue = Math.Max(1, maxHp);
			_hp.Value    = Math.Clamp(hp, 0, (int)_hp.MaxValue);

			_arm.MaxValue = 20;
			_arm.Value    = Math.Clamp(armor, 0, 20);

			// Optional: populate status + loadout if your Hero exposes them.
			// We keep it no-op by default; you can wire textures later via Show(...)
			ClearRow(_status);
			ClearRow(_loadout);
		}

		/// <summary>
		/// UI-driven API if you have textures handy.
		/// </summary>
		public void Show(string name, int hp, int maxHp, int armor, Texture2D[]? statusIcons, Texture2D[]? loadoutIcons)
		{
			_name.Text = name;
			_hp.MaxValue = Math.Max(1, maxHp);
			_hp.Value    = Math.Clamp(hp, 0, (int)_hp.MaxValue);

			_arm.MaxValue = 20;
			_arm.Value    = Math.Clamp(armor, 0, 20);

			ClearRow(_status);
			if (statusIcons != null)
				foreach (var t in statusIcons)
					_status.AddChild(new TextureRect { Texture = t, CustomMinimumSize = new Vector2(20, 20) });

			ClearRow(_loadout);
			if (loadoutIcons != null)
				foreach (var t in loadoutIcons)
					_loadout.AddChild(new TextureRect { Texture = t, CustomMinimumSize = new Vector2(28, 28) });
		}

		// ---------- helpers ----------
		private static void ClearRow(Container c)
		{
			for (int i = c.GetChildCount() - 1; i >= 0; i--)
				c.GetChild(i).QueueFree();
		}

		private static string? TryGetString(object obj, string prop)
		{
			try
			{
				var p = obj.GetType().GetProperty(prop, BindingFlags.Public | BindingFlags.Instance);
				if (p != null && p.PropertyType == typeof(string))
					return (string?)p.GetValue(obj);
			}
			catch { }
			return null;
		}

		private static int TryGetInt(object obj, string prop, int fallback = 0)
		{
			try
			{
				var p = obj.GetType().GetProperty(prop, BindingFlags.Public | BindingFlags.Instance);
				if (p != null)
				{
					var v = p.GetValue(obj);
					if (v is int i) return i;
					if (v is long l) return (int)l;
					if (v is float f) return (int)f;
					if (v is double d) return (int)d;
				}
				// Also try fields
				var fInfo = obj.GetType().GetField(prop, BindingFlags.Public | BindingFlags.Instance);
				if (fInfo != null)
				{
					var v = fInfo.GetValue(obj);
					if (v is int i) return i;
					if (v is long l) return (int)l;
					if (v is float f) return (int)f;
					if (v is double d) return (int)d;
				}
			}
			catch { }
			return fallback;
		}
	}
}
