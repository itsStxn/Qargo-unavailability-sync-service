using System;
using System.Text.Json.Serialization;

namespace Root.DTOs.ResourceListComponents;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EmissionClass {
	[JsonPropertyName("EURO_1")]
	EURO_1,

	[JsonPropertyName("EURO_2")]
	EURO_2,

	[JsonPropertyName("EURO_3")]
	EURO_3,

	[JsonPropertyName("EURO_4")]
	EURO_4,

	[JsonPropertyName("EURO_5")]
	EURO_5,

	[JsonPropertyName("EURO_6")]
	EURO_6
}
