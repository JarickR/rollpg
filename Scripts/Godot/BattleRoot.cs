using Godot;
using EngineGame = DiceArena.Engine.Core.Game;
using EnginePM   = DiceArena.Engine.Core.PartyMember;

namespace DiceArena.Godot
{
	/// <summary>Minimal battle screen root that shows one hero card + log.</summary>
	public partial class BattleRoot : Control
	{
		[Export] public NodePath LogBoxPath    { get; set; } = default!;
		[Export] public NodePath HeroPanelPath { get; set; } = default!;

		private RichTextLabel? _log;
		private HeroCard? _hero;

		public override void _Ready()
		{
			_log  = GetNodeOrNull<RichTextLabel>(LogBoxPath);
			_hero = GetNodeOrNull<HeroCard>(HeroPanelPath);
		}

		public void ApplyFromGame(EngineGame game)
		{
			if (_hero == null || game.Members.Count == 0)
				return;

			EnginePM m = game.GetMember(0);
			_hero.SetHero(m);

			_log?.Clear();
			_log?.AddText($"Loaded party size {game.PartySize}\n");
			_log?.AddText($"Class: {m.ClassId}, T1: {(m.Tier1A ?? m.Tier1B ?? "-")}, T2: {m.Tier2 ?? "-"}\n");
		}
	}
}
