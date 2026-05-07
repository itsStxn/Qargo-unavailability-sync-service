using System;
using System.Text.Json.Serialization;

namespace Root.DTOs.ResourceListComponents;

public class Trailer : Vehicle {
	[JsonPropertyName("axle_configuration")]
	public ChassisAxleConfiguration? AxleConfiguration { get; set; }
}