using System;
using System.Text.Json.Serialization;

namespace Root.DTOs;

public class AccessTokenResponse {
	[JsonPropertyName("access_token")]
	public required string AccessToken { get; set; }

	[JsonPropertyName("token_type")]
	public required string TokenType { get; set; }

	[JsonPropertyName("expires_in")]
	public required int ExpiresIn { get; set; }
}
