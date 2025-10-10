using System.Text.Json.Serialization;

namespace RollPG.Content
{
	/// <summary>
	/// Matches your classes.json entries: id, name, trait, heroAction.
	/// </summary>
	public sealed class ClassDef
	{
		[JsonPropertyName("id")]         public string Id          { get; set; } = "";
		[JsonPropertyName("name")]       public string Name        { get; set; } = "";
		[JsonPropertyName("trait")]      public string Trait       { get; set; } = "";
		[JsonPropertyName("heroAction")] public string HeroAction  { get; set; } = "";
	}

	/// <summary>
	/// Matches your spells.json entries: id, name, tier, kind, text, order.
	/// </summary>
	public sealed class SpellDef
	{
		[JsonPropertyName("id")]    public string Id     { get; set; } = "";
		[JsonPropertyName("name")]  public string Name   { get; set; } = "";
		[JsonPropertyName("tier")]  public int Tier      { get; set; }
		[JsonPropertyName("kind")]  public string Kind   { get; set; } = "";
		[JsonPropertyName("text")]  public string Text   { get; set; } = "";
		[JsonPropertyName("order")] public int Order     { get; set; }
	}

	/// <summary>
	/// Player selection snapshot you can pass into battle.
	/// </summary>
	public sealed class Loadout
	{
		public string ClassId { get; set; } = "";
		// Store spell ids in the order your UI chooses them.
		public System.Collections.Generic.List<string> SpellIds { get; set; } = new();
	}
}
