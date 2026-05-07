using System;
using System.Text.Json.Serialization;

namespace Root.DTOs.ResourceListComponents;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContainerStandardSize {
	[JsonPropertyName("FEET_20")]
	FEET_20,

	[JsonPropertyName("FEET_25")]
	FEET_25,

	[JsonPropertyName("FEET_30")]
	FEET_30,

	[JsonPropertyName("FEET_35")]
	FEET_35,

	[JsonPropertyName("FEET_40")]
	FEET_40,

	[JsonPropertyName("FEET_45")]
	FEET_45
}
