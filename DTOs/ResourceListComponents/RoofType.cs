using System;
using System.Text.Json.Serialization;

namespace Root.DTOs.ResourceListComponents;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RoofType {
	[JsonPropertyName("CLOSED")]
	CLOSED,

	[JsonPropertyName("OPEN")]
	OPEN,

	[JsonPropertyName("FLATBED")]
	FLATBED
}
