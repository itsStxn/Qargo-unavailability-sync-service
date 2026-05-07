using System;
using System.IO;
using Root.DTOs;
using Root.Errors;
using System.Text.Json;
using Root.Utils.Interfaces;
using static Root.Constants.Constants;

namespace Root.Utils;

public class AccessTokenUtil : IAccessTokenUtil {
	private readonly string _filePath;

	public AccessTokenUtil(string fileName) {
		_filePath = $"./Cache/{fileName.ToLower()}-access-token.json";
	}

	public string ReadCache() {
		// ? Early exit
		if (!File.Exists(_filePath)) {
			return string.Empty;
		}

		// ? Get json string data
		string json;
		try {
			json = File.ReadAllText(_filePath);
		}
		catch (IOException ex) {
			throw new StreamException("Failed to read cache file.", ex);
		}

		// ? Parse string data into DTO
		CachedAccessToken? data;
		try {
			data = JsonSerializer.Deserialize<CachedAccessToken>(json);
		}
		catch (JsonException ex) {
			throw new ParseException("Cache file contains invalid JSON.", ex);
		}

		// ? Validate data
		if (data == null)
			throw new ParseException("Cache file deserialized to null.");
		return data.AccessToken;
	}

	public void CreateCache(string token) {
		// ? Validate token
		if (token.Length == 0)
			throw new ConfigException("Access token cannot be empty");

		// ? Mount access token
		var data = new CachedAccessToken {
			AccessToken = token
		};

		// ? Rewrite cache
		var json = JsonSerializer.Serialize(data, JSON_OPTIONS);
		File.WriteAllText(_filePath, json);
	}

	public static void PrintPretty(string json) {
		// ? Validate string json
		if (string.IsNullOrWhiteSpace(json))
			throw new ConfigException("JSON input is empty");

		// ? Json serialization
		try {
			using JsonDocument document = JsonDocument.Parse(json);

			string prettyJson = JsonSerializer.Serialize(
				document.RootElement, JSON_OPTIONS);
			Console.WriteLine(prettyJson);
		}
		catch (JsonException ex) {
			throw new ParseException("JSON input is empty", ex);
		}
	}
}
