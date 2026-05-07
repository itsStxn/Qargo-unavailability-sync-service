using System;
using System.Text.Json.Serialization;

namespace Root.DTOs.ResourceListComponents;

public class Container {
	[JsonPropertyName("container_number")]
	public string? ContainerNumber { get; set; }

	[JsonPropertyName("manufacturer")]
	public string? Manufacturer { get; set; }

	[JsonPropertyName("standard_size")]
	public ContainerStandardSize? StandardSize { get; set; }
}
