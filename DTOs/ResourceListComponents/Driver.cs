using System;
using System.Text.Json.Serialization;

namespace Root.DTOs.ResourceListComponents;

public class Driver {
	[JsonPropertyName("first_name")]
	public string? FirstName { get; set; }

	[JsonPropertyName("last_name")]
	public string? LastName { get; set; }

	[JsonPropertyName("email_address")]
	public string? EmailAddress { get; set; }

	[JsonPropertyName("date_of_birth")]
	public string? DOB { get; set; }

	[JsonPropertyName("phone_number")]
	public string? PhoneNumber { get; set; }
	
	[JsonPropertyName("start_stop_location")]
	public LocationReference? StartStopLocation { get; set; }
}
