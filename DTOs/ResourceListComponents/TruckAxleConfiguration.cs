using System;
using System.Text.Json.Serialization;

namespace Root.DTOs.ResourceListComponents;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TruckAxleConfiguration {
	[JsonPropertyName("AC_4_X_2")]
	AC_4_X_2,

	[JsonPropertyName("AC_4_X_4")]
	AC_4_X_4,

	[JsonPropertyName("AC_6_X_2")]
	AC_6_X_2,

	[JsonPropertyName("AC_6_X_4")]
	AC_6_X_4,

	[JsonPropertyName("AC_6_X_6")]
	AC_6_X_6,

	[JsonPropertyName("AC_8_X_2")]
	AC_8_X_2,

	[JsonPropertyName("AC_8_X_4")]
	AC_8_X_4,

	[JsonPropertyName("AC_8_X_6")]
	AC_8_X_6,

	[JsonPropertyName("AC_10_X_4")]
	AC_10_X_4,

	[JsonPropertyName("AC_12_X_4")]
	AC_12_X_4
}
