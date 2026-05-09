using System;
using System.IO;
using Root.DTOs;
using Root.Errors;
using System.Text.Json;
using Root.Utils.Interfaces;

namespace Root.Utils;

/// <summary>
/// Provides utilities for caching, retrieving, and printing access token data.
/// Handles JSON serialization and deserialization for cached token storage.
/// </summary>
public class AccessTokenUtil : JsonUtil, IAccessTokenUtil {

	/// <summary>
	/// The file path used to persist the cached access token.
	/// </summary>
	private readonly string _filePath;


	/// <summary>
	/// Initializes a new instance of <see cref="AccessTokenUtil"/>
	/// using the specified cache file name.
	/// </summary>
	/// <param name="fileName">
	/// The logical cache file name used to generate the token cache path.
	/// </param>
	public AccessTokenUtil(string fileName) {
		_filePath = $"./Cache/{fileName.ToLower()}-access-token.json";
	}


	/// <summary>
	/// Reads and deserializes the cached access token from disk.
	/// Returns an empty string when no cache file exists.
	/// </summary>
	/// <returns>
	/// The cached access token if available; otherwise an empty string.
	/// </returns>
	/// <exception cref="StreamException">
	/// Thrown when the cache file cannot be read.
	/// </exception>
	/// <exception cref="ParseException">
	/// Thrown when the cache file contains invalid JSON
	/// or deserializes to <c>null</c>.
	/// </exception>
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


	/// <summary>
	/// Serializes and stores the provided access token in the cache file.
	/// Existing cache contents are overwritten.
	/// </summary>
	/// <param name="token">
	/// The access token value to cache.
	/// </param>
	/// <exception cref="ConfigException">
	/// Thrown when the provided token is empty.
	/// </exception>
	public void CreateCache(string token) {
		// ? Validate token
		if (token.Length == 0)
			throw new ConfigException("Access token cannot be empty");

		// ? Mount access token
		var data = new CachedAccessToken {
			AccessToken = token
		};

		// ? Rewrite cache
		var json = ToPrettyJson(data);
		File.WriteAllText(_filePath, json);
	}
}
