using System;
using System.Text.Json.Serialization;

namespace Root.DTOs.ResourceListComponents;

public class Chassis : Frame {
	[JsonPropertyName("axle_configuration")]
	public ChassisAxleConfiguration? AxleConfiguration { get; set; }
}
