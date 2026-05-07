using System;
using System.Text.Json.Serialization;

namespace Root.DTOs.ResourceListComponents;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FuelType {
	[JsonPropertyName("DIESEL")]
	DIESEL,

	[JsonPropertyName("BIOFUELS")]
	BIOFUELS,

	[JsonPropertyName("LPG")]
	LPG,

	[JsonPropertyName("ELECTRIC")]
	ELECTRIC,

	[JsonPropertyName("HYBRID_ELECTRIC")]
	HYBRID_ELECTRIC
}
