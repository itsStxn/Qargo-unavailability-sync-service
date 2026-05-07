using System;
using System.Text.Json.Serialization;

namespace Root.DTOs.ResourceListComponents;

public class Vehicle : Frame {
	[JsonPropertyName("capacity_pallet_spaces")]
	public string? CapacityPalletSpaces { get; set; }

	[JsonPropertyName("capacity_volume_m3")]
	public string? CapacityVolumeM3 { get; set; }

	[JsonPropertyName("capacity_weight_kg")]
	public string? CapacityWeightKg { get; set; }
}
