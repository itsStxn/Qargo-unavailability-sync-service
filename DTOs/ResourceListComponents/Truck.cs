using System;
using System.Text.Json.Serialization;

namespace Root.DTOs.ResourceListComponents;

public class Truck : Vehicle {
	[JsonPropertyName("axle_configuration")]
	public TruckAxleConfiguration? AxleConfiguration { get; set; }

	[JsonPropertyName("emission_class")]
	public EmissionClass? EmissionClass { get; set; }

	[JsonPropertyName("fuel_type")]
	public FuelType? FuelType { get; set; }

	[JsonPropertyName("roof_type")]
	public RoofType? RoofType { get; set; }
}
