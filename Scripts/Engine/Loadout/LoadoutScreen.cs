using Godot;
using EngineGame = DiceArena.Engine.Core.Game;
using EnginePM   = DiceArena.Engine.Core.PartyMember;

namespace DiceArena.Engine.Loadout
{
	[GlobalClass]
	public partial class LoadoutScreen : Control
	{
		// --- Preview icons on the left (assign in Inspector) ---
		[Export] public NodePath ClassPreviewPath  { get; set; } = default!;
		[Export] public NodePath Tier1PreviewPath  { get; set; } = default!;
		[Export] public NodePath Tier2PreviewPath  { get; set; } = default!;

		// --- Your existing input controls (assign these too) ---
		[Export] public NodePath ClassOptionPath   { get; set; } = default!; // OptionButton
		[Export] public NodePath Tier1AttackPath   { get; set; } = default!; // CheckBox/CheckButton
		[Export] public NodePath Tier1HealPath     { get; set; } = default!;
		[Export] public NodePath Tier1ShieldPath   { get; set; } = default!;
		[Export] public NodePath Tier2OptionPath   { get; set; } = default!; // OptionButton
		[Export] public NodePath FinalizeBtnPath   { get; set; } = default!; // Button

		public delegate void FinalizedEventHandler(EngineGame game);
		public event FinalizedEventHandler? Finalized;

		private TextureRect? _classPrev, _t1Prev, _t2Prev;
		private OptionButton? _classOpt, _t2Opt;
		private CheckButton? _t1Atk, _t1Heal, _t1Shield;
		private Button? _finalize;

		private const int ICON_SIZE = 64; // match your imports

		public override void _Ready()
		{
			// Cache nodes
			_classPrev = GetNodeOrNull<TextureRect>(ClassPreviewPath);
			_t1Prev    = GetNodeOrNull<TextureRect>(Tier1PreviewPath);
			_t2Prev    = GetNodeOrNull<TextureRect>(Tier2PreviewPath);

			_classOpt  = GetNodeOrNull<OptionButton>(ClassOptionPath);
			_t2Opt     = GetNodeOrNull<OptionButton>(Tier2OptionPath);

			_t1Atk     = GetNodeOrNull<CheckButton>(Tier1AttackPath);
			_t1Heal    = GetNodeOrNull<CheckButton>(Tier1HealPath);
			_t1Shield  = GetNodeOrNull<CheckButton>(Tier1ShieldPath);

			_finalize  = GetNodeOrNull<Button>(FinalizeBtnPath);

			// Populate (simple demo data — replace with your DB if you have one)
			if (_classOpt != null && _classOpt.ItemCount == 0)
			{
				_classOpt.AddItem("Thief");   // must match your class png
				_classOpt.AddItem("Paladin");
				_classOpt.AddItem("Warden");
			}
			if (_t2Opt != null && _t2Opt.ItemCount == 0)
			{
				_t2Opt.AddItem("attackplus"); // must match Tier2 png name
				_t2Opt.AddItem("freezeblast");
				_t2Opt.AddItem("healpulse");
			}

			// Signals -> refresh preview
			if (_classOpt != null) _classOpt.ItemSelected += _ => RefreshPreview();
			if (_t2Opt    != null) _t2Opt.ItemSelected    += _ => RefreshPreview();
			if (_t1Atk    != null) _t1Atk.Toggled         += _ => RefreshPreview();
			if (_t1Heal   != null) _t1Heal.Toggled        += _ => RefreshPreview();
			if (_t1Shield != null) _t1Shield.Toggled      += _ => RefreshPreview();

			if (_finalize != null) _finalize.Pressed += OnFinalizePressed;

			RefreshPreview();
		}

		private void RefreshPreview()
		{
			var classId = _classOpt?.GetItemText(_classOpt.GetSelectedId()) ?? "Thief";

			// Tier1 preview – show whichever is on (first match wins)
			string? t1 = null;
			if (_t1Atk?.ButtonPressed   == true) t1 = "attack";
			else if (_t1Heal?.ButtonPressed  == true) t1 = "heal";
			else if (_t1Shield?.ButtonPressed== true) t1 = "shield";

			var t2 = _t2Opt?.GetItemText(_t2Opt.GetSelectedId()) ?? "";

			if (_classPrev != null)
				_classPrev.Texture = DiceArena.Godot.IconLibrary.GetClassTexture(classId, ICON_SIZE);

			if (_t1Prev != null)
				_t1Prev.Texture = DiceArena.Godot.IconLibrary.GetSpellTexture(t1 ?? "", 1, ICON_SIZE);

			if (_t2Prev != null)
				_t2Prev.Texture = DiceArena.Godot.IconLibrary.GetSpellTexture(t2, 2, ICON_SIZE);
		}

		private void OnFinalizePressed()
		{
			// Build a 1-member game from current UI
			var game = new EngineGame { PartySize = 1 };

			var classId = _classOpt?.GetItemText(_classOpt.GetSelectedId()) ?? "Thief";
			string? t1A = null; string? t1B = null;

			if (_t1Atk?.ButtonPressed   == true) t1A = "attack";
			if (_t1Heal?.ButtonPressed  == true) t1B = t1B ?? "heal";
			if (_t1Shield?.ButtonPressed== true) t1B = t1B ?? "shield";

			var t2 = _t2Opt?.GetItemText(_t2Opt.GetSelectedId());

			game.AddMember(new EnginePM(classId, t1A, t1B, t2));

			Finalized?.Invoke(game);
		}
	}
}
