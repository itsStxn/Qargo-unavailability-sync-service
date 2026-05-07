using System;
using System.Text.Json.Serialization;

namespace Root.DTOs.ResourceListComponents;

public class BillingEntity {
	[JsonPropertyName("id")]
	public required string Id { get; set; }
	
	[JsonPropertyName("legal_name")]
	public required string LegalName { get; set; }
	
	[JsonPropertyName("vat_number")]
	public required string VatNumber { get; set; }
	
	[JsonPropertyName("code")]
	public required string Code { get; set; }

	[JsonPropertyName("company_registration_number")]
	public required string CompanyRegistrationNumber { get; set; }

	[JsonPropertyName("is_default")]
	public bool IsDefault { get; set; }
}
