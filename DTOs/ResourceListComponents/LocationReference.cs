using System;
using System.Text.Json.Serialization;

namespace Root.DTOs.ResourceListComponents;

public class LocationReference {
	[JsonPropertyName("id")]
	public required string Id { get; set; }
}
