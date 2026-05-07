using System;
using System.Text.Json.Serialization;
using Root.DTOs.UnavailabilityListComponents;

namespace Root.DTOs;

public record UnavailabilityList {
	[JsonPropertyName("next_cursor")]
	public string? NextCursor { get; init; }

	[JsonPropertyName("items")]
	public List<Unavailability> Items { get; init; } = [];
}
