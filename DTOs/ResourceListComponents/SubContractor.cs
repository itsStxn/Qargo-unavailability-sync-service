using System;
using System.Text.Json.Serialization;

namespace Root.DTOs.ResourceListComponents;

public class SubContractor {
	[JsonPropertyName("row_id")]
	public required string RowId { get; set; }
}
