using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Root.DTOs.ResourceListComponents;

public class Resource {
	[JsonPropertyName("row_id")]
	public required string RowId { get; set; }

	[JsonPropertyName("name")]
	public required string Name { get; set; }

	[JsonPropertyName("external_id")]
	public string? ExternalId { get; set; }
	
	[JsonPropertyName("locale")]
	public string? Locale { get; set; }
	
	[JsonPropertyName("code")]
	public string? Code { get; set; }

	[JsonPropertyName("note")]
	public string? Note { get; set; }
	
	[JsonPropertyName("custom_fields")]
	public JsonElement? CustomFields { get; set; }
	
	[JsonPropertyName("subcontractor")]
	public SubContractor? Subcontractor { get; set; }
	
	[JsonPropertyName("integrations")]
	public JsonElement? Integrations { get; set; }

	[JsonPropertyName("billing_entity")]
	public BillingEntity? BillingEntity { get; set; }

	[JsonPropertyName("is_active")]
	public bool? IsActive { get; set; }

	[JsonPropertyName("timestamp_updated")]
	public string? TimestampUpdated { get; set; }

	// * Only one of these will be populated per item
	
	[JsonPropertyName("driver")]
	public Driver? Driver { get; set; }

	[JsonPropertyName("tractor")]
	public Tractor? Tractor { get; set; }
	
	[JsonPropertyName("truck")]
	public Truck? Truck { get; set; }

	[JsonPropertyName("van")]
	public Van? Van { get; set; }
	
	[JsonPropertyName("trailer")]
	public Trailer? Trailer { get; set; }

	[JsonPropertyName("full_trailer")]
	public FullTrailer? FullTrailer { get; set; }
	
	[JsonPropertyName("chassis")]
	public Chassis? Chassis { get; set; }
	
	[JsonPropertyName("container")]
	public Container? Container { get; set; }
}
