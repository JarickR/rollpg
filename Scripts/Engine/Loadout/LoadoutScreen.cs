// Scripts/Godot/LoadoutScreen.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DiceArena.Engine.Content;
using DiceArena.Engine.Loadout; // Log + LoadoutMemberCard

namespace DiceArena.GodotUI
{
	/// <summary>
	/// Root Loadout screen node.
	/// Public API:
	///   - SetBundle(typeof(ContentDatabase)) or SetBundle(dbInstance)
	///   - InjectContent(classes, t1, t2)
	///   - Build() / BuildCards()
	/// Emits SelectionChanged(int memberIndex, string? classId, string[] tier1Ids, string? tier2Id)
	/// when any card changes selection.
	/// </summary>
	public partial class LoadoutScreen : Control
	{
		[Export] public NodePath? MemberCardsRootPath { get; set; }
		[Export] public NodePath? PartySizeSpinPath { get; set; }

		[Export(PropertyHint.Range, "1,8,1")]
		public int DefaultPartySize { get; set; } = 3;

		[Export] public bool AutoBuild { get; set; } = false;

		// Internal content slices
		private List<ClassDef> _classes = new();
		private List<SpellDef> _tier1 = new();
		private List<SpellDef> _tier2 = new();
		private bool _hasContent = false;
		private bool _needsBuild = false; // set when a build is requested before content arrives

		public override void _Ready()
		{
			EnsureSpinDefaults();
			WirePartySizeSpin();

			if (AutoBuild && _hasContent)
				SafeBuild();
			else if (AutoBuild && !_hasContent)
				_needsBuild = true;
		}

		// -------- Public API (instance methods) --------

		/// <summary>
		/// Accepts an instance or a Type for a static class exposing:
		///   IEnumerable<ClassDef> GetAllClasses()
		///   IEnumerable<SpellDef> GetSpellsByTier(int)
		/// </summary>
		public void SetBundle(object db)
		{
			if (db == null) { Log.E("SetBundle: null db"); return; }

			try
			{
				Type t; object? target = null;
				if (db is Type dt) t = dt; else { t = db.GetType(); target = db; }

				var getAllClasses   = t.GetMethod("GetAllClasses",   BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static, Type.EmptyTypes);
				var getSpellsByTier = t.GetMethod("GetSpellsByTier", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static, new Type[] { typeof(int) });

				if (getAllClasses == null || getSpellsByTier == null)
				{
					Log.E("SetBundle: DB missing GetAllClasses() / GetSpellsByTier(int).");
					return;
				}

				var classes = (getAllClasses.Invoke(target, null) as IEnumerable<ClassDef>) ?? Enumerable.Empty<ClassDef>();
				var t1 = (getSpellsByTier.Invoke(target, new object[] { 1 }) as IEnumerable<SpellDef>) ?? Enumerable.Empty<SpellDef>();
				var t2 = (getSpellsByTier.Invoke(target, new object[] { 2 }) as IEnumerable<SpellDef>) ?? Enumerable.Empty<SpellDef>();

				InjectContent(classes, t1, t2);
				Log.I($"SetBundle: injected classes={_classes.Count}, t1={_tier1.Count}, t2={_tier2.Count}");
			}
			catch (Exception ex) { Log.E($"SetBundle reflection failed: {ex.Message}"); }
		}

		/// <summary>Explicit injection of slices you already loaded from JSON.</summary>
		public void InjectContent(IEnumerable<ClassDef> classes, IEnumerable<SpellDef> tier1, IEnumerable<SpellDef> tier2)
		{
			_classes = classes?.ToList() ?? new();
			_tier1 = tier1?.ToList() ?? new();
			_tier2 = tier2?.ToList() ?? new();
			_hasContent = true;

			// Ensure the spin reflects the intended default (prevents "1 card" surprise)
			var spin = GetNodeSafe<SpinBox>(PartySizeSpinPath);
			if (GodotObject.IsInstanceValid(spin) && spin.Value < DefaultPartySize)
				spin.Value = DefaultPartySize;

			Log.I($"InjectContent: classes={_classes.Count}, t1={_tier1.Count}, t2={_tier2.Count}");

			if (_needsBuild || AutoBuild)
			{
				_needsBuild = false;
				SafeBuild();
			}
		}

		/// <summary>Builds/rebuilds the member cards based on current slices and spin value.</summary>
		public void BuildCards()
		{
			if (!_hasContent)
			{
				_needsBuild = true;
				return;
			}

			var memberRoot = GetNodeSafe<Container>(MemberCardsRootPath);
			var partySpin  = GetNodeSafe<SpinBox>(PartySizeSpinPath);

			if (!GodotObject.IsInstanceValid(memberRoot))
			{
				Log.E("BuildCards: MemberCardsRootPath invalid / unset.");
				return;
			}

			int partySize = DefaultPartySize;
			if (GodotObject.IsInstanceValid(partySpin))
			{
				if (partySpin.Value < DefaultPartySize) partySpin.Value = DefaultPartySize;
				partySize = Mathf.Max(1, (int)partySpin.Value);
			}

			// If the root is a GridContainer, show all cards in one row:
			if (memberRoot is GridContainer g)
			{
				g.Columns = Mathf.Max(1, partySize);
				g.AddThemeConstantOverride("h_separation", 16);
				g.AddThemeConstantOverride("v_separation", 16);
			}

			// Root layout guards
			memberRoot.SetDeferred("custom_minimum_size", new Vector2(1280, 520));

			// Clear old
			for (int i = memberRoot.GetChildCount() - 1; i >= 0; i--)
			{
				var c = memberRoot.GetChild(i);
				if (GodotObject.IsInstanceValid(c)) memberRoot.RemoveChild(c);
				c.QueueFree();
			}

			// Build cards + wire signals
			for (int i = 0; i < partySize; i++)
			{
				var card = new DiceArena.Engine.Loadout.LoadoutMemberCard();
				if (card is Control cc)
				{
					cc.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
					cc.SizeFlagsVertical   = Control.SizeFlags.ShrinkCenter;
				}

				memberRoot.AddChild(card);
				card.Build(i, _classes, _tier1, _tier2);

				// Bubble card selections up to the screen level
				card.SelectionChanged += (int memberIndex, string? classId, string[] t1Ids, string? t2Id) =>
				{
					EmitSignal(SignalName.SelectionChanged, memberIndex, classId, t1Ids, t2Id);
				};
			}

			memberRoot.SetDeferred("custom_minimum_size", new Vector2(1280, 520));
			memberRoot.QueueRedraw();

			var cols = (memberRoot as GridContainer)?.Columns ?? -1;
			Log.V($"BuildCards complete: partySize={partySize}, rootChildren={memberRoot.GetChildCount()}, rootType={memberRoot.GetType().Name}, gridCols={cols}");

			if (memberRoot.GetChildCount() == 0 && (_classes.Count == 0 && _tier1.Count == 0 && _tier2.Count == 0))
				InsertEmergencyLabel("⚠ No content available");
		}

		// Convenience
		public void Build() => BuildCards();

		// -------- Internals --------

		private void EnsureSpinDefaults()
		{
			var spin = GetNodeSafe<SpinBox>(PartySizeSpinPath);
			if (!GodotObject.IsInstanceValid(spin)) return;

			// Ensure sane config
			if (spin.MinValue < 1) spin.MinValue = 1;
			if (spin.MaxValue < 1) spin.MaxValue = 8;
			if (spin.Step <= 0)    spin.Step     = 1;

			if (spin.Value < 1)
				spin.Value = DefaultPartySize;
		}

		private void WirePartySizeSpin()
		{
			var spin = GetNodeSafe<SpinBox>(PartySizeSpinPath);
			if (!GodotObject.IsInstanceValid(spin)) return;
			spin.ValueChanged += (double _) =>
			{
				if (!_hasContent) { _needsBuild = true; return; }
				SafeBuild();
			};
		}

		private void SafeBuild()
		{
			try { BuildCards(); }
			catch (Exception ex) { Log.W($"BuildCards exception: {ex.Message}"); }
		}

		private void InsertEmergencyLabel(string text)
		{
			var memberRoot = GetNodeSafe<Container>(MemberCardsRootPath);
			if (!GodotObject.IsInstanceValid(memberRoot)) return;

			var label = new Label
			{
				Text = text,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill
			};
			memberRoot.AddChild(label);
			label.SetDeferred("custom_minimum_size", new Vector2(640, 180));
			memberRoot.QueueRedraw();
		}

		private T? GetNodeSafe<T>(NodePath? path) where T : class
		{
			if (path == null || path.IsEmpty) return null;
			return base.GetNodeOrNull(path) as T;
		}
	}
}
