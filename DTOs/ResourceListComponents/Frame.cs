using System;
using System.Text.Json.Serialization;

namespace Root.DTOs.ResourceListComponents;

public class Frame {
	[JsonPropertyName("tare_weight_kg")]
	public string? TareWeightKg { get; set; }

	[JsonPropertyName("exterior_length_m")]
	public string? ExteriorLengthM { get; set; }

	[JsonPropertyName("exterior_width_m")]
	public string? ExteriorWidthM { get; set; }

	[JsonPropertyName("exterior_height_m")]
	public string? ExteriorHeightM { get; set; }

	[JsonPropertyName("gross_weight_vehicle_rating_weight_kg")]
	public string? GrossWeightVehicleRatingWeightKg { get; set; }

	[JsonPropertyName("license_plate")]
	public string? LicensePlate { get; set; }

	[JsonPropertyName("model")]
	public string? Model { get; set; }

	[JsonPropertyName("manufacturer")]
	public string? Manufacturer { get; set; }

	[JsonPropertyName("vin_number")]
	public string? VinNumber { get; set; }
}
