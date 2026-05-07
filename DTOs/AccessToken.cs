using System;
using System.Text.Json.Serialization;

namespace Root.DTOs;

public class AccessToken {
	[JsonPropertyName("access_token")]
	public required string Token { get; set; }

	[JsonPropertyName("token_type")]
	public required string TokenType { get; set; }

	[JsonPropertyName("expires_in")]
	public required int ExpiresIn { get; set; }
}
