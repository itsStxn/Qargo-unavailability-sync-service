using System;
using System.Text.Json.Serialization;

namespace Root.DTOs.ResourceListComponents;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChassisAxleConfiguration {
	[JsonPropertyName("AC_12_TRAILER")]
	AC_12_TRAILER,

	[JsonPropertyName("AC_10_TRAILER")]
	AC_10_TRAILER,

	[JsonPropertyName("AC_8_TRAILER")]
	AC_8_TRAILER,

	[JsonPropertyName("AC_6_TRAILER")]
	AC_6_TRAILER,

	[JsonPropertyName("AC_4_TRAILER")]
	AC_4_TRAILER,

	[JsonPropertyName("AC_2_TRAILER")]
	AC_2_TRAILER
}
