// Scripts/Data/DataModels.cs
using System.Text.Json.Serialization;

namespace DiceArena.Data
{
	// Corresponds to an entry in classes.json
	public class ClassData
	{
		[JsonPropertyName("id")]
		public string Id { get; set; } = "";

		[JsonPropertyName("name")]
		public string Name { get; set; } = "";
	}

	// Corresponds to an entry in spells.json
	public class SpellData
	{
		[JsonPropertyName("id")]
		public string Id { get; set; } = "";

		[JsonPropertyName("name")]
		public string Name { get; set; } = "";
		
		[JsonPropertyName("tier")]
		public int Tier { get; set; }
	}
}
