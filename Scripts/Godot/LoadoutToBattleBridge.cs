// Scripts/Godot/LoadoutToBattleBridge.cs
using Godot;

namespace DiceArena.Godot
{
	/// <summary>
	/// Connects the Loadout screen's "Finalized" signal/event to the Battle scene.
	/// Set both exports in the Inspector.
	/// </summary>
	public partial class LoadoutToBattleBridge : Node
	{
		// Make these nullable to avoid CS8625 when left unassigned in the scene file.
		[Export] public DiceArena.Engine.Loadout.LoadoutScreen? LoadoutScreen { get; set; }
		[Export] public BattleRoot? BattleRoot { get; set; }

		public override void _Ready()
		{
			// Subscribe if present; if not, we just do nothing (no exceptions).
			if (LoadoutScreen != null)
			{
				// LoadoutScreen.Finalized has a custom delegate type on that class.
				// Its signature is (int partySize, string[] players). We match that exactly.
				LoadoutScreen.Finalized += OnLoadoutFinalized;
			}
		}

		public override void _ExitTree()
		{
			if (LoadoutScreen != null)
				LoadoutScreen.Finalized -= OnLoadoutFinalized;
		}

		// Matches LoadoutScreen.FinalizedEventHandler signature:
		private void OnLoadoutFinalized(int partySize, string[] players)
		{
			// Minimal proof it’s wired:
			GD.PrintErr($"[Bridge] Received finalize: party={partySize}, players={players?.Length ?? 0}");

			// Hand off to BattleRoot if available.
			if (BattleRoot != null)
			{
				// Right now we just append a log line. Replace/extend to actually build the party.
				// e.g., BattleRoot.AddHeroCardsFrom(players) or similar once you expose an API.
				var names = players == null ? "null" : string.Join(", ", players);
				var msg = $"[Bridge] Finalized party -> size {partySize}, players [{names}]";
				// BattleRoot has AddLog as a private helper in our version; so we poke via a small placeholder:
				// You can add a public method on BattleRoot to log or to rebuild the UI with heroes.
				// Example public method you might add:
				//    public void BridgeLog(string s) => GetNodeOrNull<VBoxContainer>(LogRootPath)?.AddChild(new Label{ Text = s });
				// For now, we just print to the console so you can see it's flowing.
				GD.Print(msg);
			}
		}
	}
}
