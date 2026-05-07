using System;
using System.Text.Json.Serialization;

namespace Root.DTOs.ResourceListComponents;

public class Tractor {
	[JsonPropertyName("tare_weight_kg")]
	public string? Tare { get; set; }
	
	[JsonPropertyName("exterior_length_m")]
	public string? ExteriorLength { get; set; }
	
	[JsonPropertyName("exterior_width_m")]
	public string? ExteriorWidth { get; set; }
	
	[JsonPropertyName("exterior_height_m")]
	public string? ExteriorHeight { get; set; }
	
	[JsonPropertyName("vin_number")]
	public string? VinNumber { get; set; }
	
	[JsonPropertyName("manufacturer")]
	public string? Manufacturer { get; set; }
	
	[JsonPropertyName("license_plate")]
	public string? LicencePlate { get; set; }
	
	[JsonPropertyName("model")]
	public string? Model { get; set; }
	
	[JsonPropertyName("axle_configuration")]
	public TruckAxleConfiguration? AxleConfiguration { get; set; }
	
	[JsonPropertyName("emission_class")]
	public EmissionClass? EmissionClass { get; set; }
	
	[JsonPropertyName("fuel_type")]
	public FuelType? FuelType { get; set; }
}
