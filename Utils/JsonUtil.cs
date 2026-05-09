using System;
using Root.Errors;
using System.Text.Json;
using Root.Utils.Interfaces;

namespace Root.Utils;

/// <summary>
/// Provides JSON serialization and deserialization utilities.
/// </summary>
/// <remarks>
/// This utility class offers methods for converting objects to and from JSON format.
/// It uses <see cref="JsonSerializer"/> with predefined formatting options for consistent output.
/// </remarks>
public class JsonUtil : IJsonUtil {
	
	/// <summary>
	/// Gets the default <see cref="JsonSerializerOptions"/> used for JSON serialization and deserialization in the application.
	/// </summary>
	public static readonly JsonSerializerOptions JsonOptions = new() { 
		WriteIndented = true
	};

	/// <summary>
	/// Serializes the provided object into formatted JSON,
	/// and returns the serialized string.
	/// </summary>
	/// <typeparam name="T">
	/// The type of object to serialize.
	/// </typeparam>
	/// <param name="data">
	/// The object instance to serialize and print.
	/// </param>
	/// <param name="print">
	/// If true, prints the resulting JSON to the console output.
	/// </param>
	/// <returns>
	/// A formatted JSON string representation of the provided object.
	/// </returns>
	/// <exception cref="ParseException">
	/// Thrown when serialization fails due to invalid JSON formatting.
	/// </exception>
	public static string ToPrettyJson<T>(T data, bool print = false) {
		// ? Json serialization
		try {
			var json = JsonSerializer.Serialize(data, JsonOptions);
			if (print) Console.WriteLine(json);
			return json;
		}
		catch (JsonException ex) {
			throw new ParseException("JSON is badly formatted", ex);
		}
	}
}
