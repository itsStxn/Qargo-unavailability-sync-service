using System;
using System.Text.Json.Serialization;

namespace Root.DTOs.UnavailabilityListComponents;

public class Unavailability {
	[JsonPropertyName("id")]
	public required string Id { get; set; }
	
	[JsonPropertyName("external_id")]
	public string? ExternalId { get; set; }
	
	[JsonPropertyName("start_time")]
	public required string StartTime { get; set; }
	
	[JsonPropertyName("end_time")]
	public string? EndTime { get; set; }
	
	[JsonPropertyName("reason")]
	public required Reason Reason { get; set; }
	
	[JsonPropertyName("description")]
	public required string Description { get; set; }
}

