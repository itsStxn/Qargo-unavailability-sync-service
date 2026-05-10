using System;
using Serilog;

namespace Root.Core;

/// <summary>
/// Provides a base class with structured logging and message formatting capabilities.
/// All services that require consistent log output and message tagging should inherit from this class.
/// </summary>
public class Base {	
	
	/// <summary>
	/// Gets the logger instance used for logging operations within this class.
	/// </summary>
	private readonly ILogger _logger;

	/// <summary>The name of the service, used as a prefix in log messages.</summary>
	private readonly string _name;


	/// <summary>
	/// Initializes a new instance of <see cref="Base"/> with the service name set to <c>"None"</c>.
	/// </summary>
	public Base() {
		_logger = Log.ForContext(GetType());
		_name = "None";
	}

	/// <summary>
	/// Initializes a new instance of <see cref="Base"/> with a specified service name.
	/// </summary>
	/// <param name="name">The name of the service to include in log message prefixes.</param>
	protected Base(string name) {
		_logger = Log.ForContext(GetType());
		_name = name;
	}


	/// <summary>
	/// Normalizes the input text by trimming whitespace and converting to lowercase.
	/// </summary>
	/// <param name="text">The text to normalize.</param>
	/// <returns>A normalized string with leading/trailing whitespace removed and converted to lowercase.</returns>
	protected static string Normalize(string text) {
		return text.Trim().ToLowerInvariant();
	}

	/// <summary>
	/// Formats a message with the service name prefix, unless the message is already formatted.
	/// </summary>
	/// <param name="text">The raw message text to format.</param>
	/// <returns>
	/// The original trimmed message if it already starts with <c>"(service: "</c>;
	/// otherwise, a formatted string in the form <c>"(service: {name}) >> {text}"</c>.
	/// </returns>
	public string Msg(string text) {
		string trimmed = text.Trim();
		var ignoreCase = StringComparison.OrdinalIgnoreCase;

		if (trimmed.StartsWith("(service: ", ignoreCase)) return trimmed;
		return $"(service: {_name}) >> {trimmed}";
	}

	/// <summary>
	/// Logs a formatted informational message using the current service context.
	/// </summary>
	/// <param name="text">The message to log.</param>
	public void Echo(string text) {
		_logger.Information(Msg(text));
	}

	/// <summary>
	/// Logs a formatted warning message using the current service context.
	/// </summary>
	/// <param name="text">The message to log.</param>
	public void Warn(string text) {
		_logger.Warning(Msg(text));
	}

	/// <summary>
	/// Logs a formatted error message using the current service context.
	/// </summary>
	/// <param name="text">The message to log.</param>
	public void Error(string text) {
		_logger.Error(Msg(text));
	}
}
