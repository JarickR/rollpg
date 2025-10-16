using Godot;

public partial class HPBar : Control
{
	[Export] public int MaxHP { get; set; } = 20;
	[Export] public int HP    { get; set; } = 20;
	[Export] public int Armor { get; set; } = 0;

	private ProgressBar _hp = null!;
	private ProgressBar _armor = null!;
	private Label _text = null!;

	public override void _Ready()
	{
		_hp    = GetNode<ProgressBar>("HpFill");
		_armor = GetNode<ProgressBar>("ArmorFill");
		_text  = GetNode<Label>("CenterText");
		Apply();
	}

	public void SetValues(int hp, int maxHp, int armor)
	{
		MaxHP = Mathf.Max(1, maxHp);
		HP    = Mathf.Clamp(hp, 0, MaxHP);
		Armor = Mathf.Max(0, armor);
		Apply();
	}

	private void Apply()
	{
		// Base HP (green)
		_hp.MaxValue = MaxHP;
		_hp.Value    = Mathf.Clamp(HP, 0, MaxHP);

		// Silver overlay shows HP + Armor (clamped)
		_armor.MaxValue = MaxHP;
		_armor.Value    = Mathf.Clamp(HP + Armor, 0, MaxHP);
		_armor.Visible  = Armor > 0;  // hide if no armor

		_text.Text = $"{HP}/{MaxHP}";
	}
}
