using System;
using System.Text.Json.Serialization;
using Root.DTOs.ResourceListComponents;

namespace Root.DTOs;

public record ResourceList {
	[JsonPropertyName("next_cursor")]
	public string? NextCursor { get; init; }

	[JsonPropertyName("items")]
	public List<Resource> Items { get; init; } = [];
}
