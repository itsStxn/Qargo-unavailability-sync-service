using Polly;
using System;
using Polly.Retry;
using Root.Errors;
using Root.Source;
using Root.Core.Interfaces;
using System.Net.Http.Json;

namespace Root.Core;

/// <summary>
/// Provides a resilient HTTP client wrapper with automatic retry logic, structured error handling,
/// and JSON deserialization. Implements <see cref="IMyRequest"/> and inherits logging utilities from <see cref="Base"/>.
/// </summary>
public class MyRequest : Base, IMyRequest {
	
	/// <summary>The underlying <see cref="HttpSource"/> used to manage request behavior.</summary>
	protected readonly HttpSource _http;

	/// <summary>The Polly retry policy applied to all outgoing requests.</summary>
	private readonly AsyncRetryPolicy _retryPolicy;


	/// <summary>
	/// Initializes a new instance of <see cref="MyRequest"/> and configures the retry policy.
	/// Retries are triggered on <see cref="NetworkException"/>, with delays provided by <see cref="Timeout"/>
	/// and retry logging handled by <see cref="OnRetryAsync"/>.
	/// </summary>
	/// <param name="http">The <see cref="HttpSource"/> instance to use for all requests.</param>
	public MyRequest(HttpSource http) : base() {
		_http = http;
		_retryPolicy = BuildRetryPolicy(http);
	}

	/// <summary>
	/// Initializes a new instance of <see cref="MyRequest"/> and configures the retry policy.
	/// Retries are triggered on <see cref="NetworkException"/>, with delays provided by <see cref="Timeout"/>
	/// and retry logging handled by <see cref="OnRetryAsync"/>.
	/// </summary>
	/// <param name="http">The <see cref="HttpSource"/> instance to use for all requests.</param>
	/// <param name="name">The service name passed to <see cref="Base"/> for log message prefixing.</param>
	protected MyRequest(HttpSource http, string name) : base(name) {
		_http = http;
		_retryPolicy = BuildRetryPolicy(http);
	}


	/// <summary>
	/// Builds an asynchronous retry policy for handling network exceptions with exponential backoff.
	/// </summary>
	private AsyncRetryPolicy BuildRetryPolicy(HttpSource http) {
		return Policy
			.Handle<NetworkException>()
			.WaitAndRetryAsync(
				retryCount: http.Retry.MaxAttempts,
				sleepDurationProvider: (attempt, ex, ctx) => 
					Timeout(attempt, ex),
				onRetryAsync: async (ex, delay, attempt, ctx) => 
					await OnRetryAsync(ex, delay)
			);
	}

	/// <summary>
	/// Constructs the full absolute URI by combining the <see cref="HttpSource.Cli"/>'s base address
	/// with the relative URI from the given request message.
	/// </summary>
	/// <param name="req">The <see cref="HttpRequestMessage"/> containing the relative URI.</param>
	/// <returns>The fully resolved URI as a trimmed string.</returns>
	/// <exception cref="ConfigException">
	/// Thrown if the base address on <see cref="_http.Cli"/> or the URI on <paramref name="req"/> is null or empty.
	/// </exception>
	protected string BuildFullUri(HttpRequestMessage req) {
		// ? Validate HttpClient's base address
		var baseUri = _http.Cli.BaseAddress;
		if (baseUri == null || baseUri.ToString().Trim() == string.Empty)
			throw new ConfigException(
				Msg("The base address in HttpClient is empty or null"));

		// ? Validate request's uri
		var reqUri = req.RequestUri;
		if (reqUri == null || reqUri.ToString().Trim() == string.Empty)
			throw new ConfigException(
				Msg("The URI in HttpRequestMessage is empty or null"));

		return new Uri(baseUri, reqUri).ToString().Trim();
	}

	/// <summary>
	/// Calculates the delay duration before the next retry attempt.
	/// Respects the server's <c>Retry-After</c> header if present; otherwise applies
	/// exponential backoff capped at <c>RETRY_TIMEOUT</c> milliseconds.
	/// </summary>
	/// <param name="attempt">The current retry attempt number (1-based).</param>
	/// <param name="ex">The exception that triggered the retry.</param>
	/// <returns>A <see cref="TimeSpan"/> representing how long to wait before the next attempt.</returns>
	private TimeSpan Timeout(int attempt, Exception ex) {
		// ? Try to respect server's rightAfter clause
		var netEx = AppException.Wrap<NetworkException>(ex);

		if (netEx.RetryAfter.HasValue) {
			var time = netEx.RetryAfter.Value;
			Warn($"Rate limited, retrying after {time.TotalSeconds}s...");
			return time;
		}

		// ? Manually calculate timeout duration
		// ? It is capped at _http.RequestProps.RetryTimeout, in milliseconds
		var delayMs = Math.Min(_http.Retry.Timeout, 50 * Math.Pow(5, attempt - 1)); // ? Capped at 1s
		return TimeSpan.FromMilliseconds(delayMs);
	}

	/// <summary>
	/// Called by the retry policy before each retry attempt. Logs a warning and wraps the
	/// triggering exception in a <see cref="NetworkException"/>.
	/// Override in derived classes to add custom retry behaviour.
	/// </summary>
	/// <param name="ex">The exception that triggered the retry.</param>
	/// <param name="delay">The delay duration that will be observed before the next attempt.</param>
	/// <returns>A <see cref="Task"/> resolving to the wrapped <see cref="NetworkException"/>.</returns>
	protected virtual Task<NetworkException> OnRetryAsync(Exception ex, TimeSpan delay) {
		Warn($"Network error, retrying after {delay.TotalSeconds}s: {ex.Message}");
		return Task.FromResult(AppException.Wrap<NetworkException>(ex));
	}

	/// <summary>
	/// Sends the given <see cref="HttpRequestMessage"/> and deserializes the JSON response body into <typeparamref name="T"/>.
	/// Override in derived classes to intercept or extend request/response handling.
	/// </summary>
	/// <typeparam name="T">The type to deserialize the response body into.</typeparam>
	/// <param name="req">The <see cref="HttpRequestMessage"/> to send.</param>
	/// <returns>A <see cref="Task"/> resolving to the deserialized response of type <typeparamref name="T"/>.</returns>
	/// <exception cref="NetworkException">Thrown if the HTTP request fails or returns a non-success status code.</exception>
	/// <exception cref="ParseException">Thrown if the response body deserializes to <c>null</c>.</exception>
	/// <exception cref="StreamException">Thrown if reading or deserializing the response body is cancelled or faulted.</exception>
	protected virtual async Task<T> TrySendAsync<T>(HttpRequestMessage req) {
		string fullUri() => BuildFullUri(req);
		HttpResponseMessage? res = null;

		// ? Get http response
		try {
			Echo($"Fetching result at {fullUri()}...");
			res = await _http.Cli.SendAsync(req, _http.CancToken);
			res.EnsureSuccessStatusCode();
		}
		catch (HttpRequestException ex) {
			throw new NetworkException(
				message: 	Msg($"Request to {fullUri()} failed: {ex.StatusCode} {ex.Message}"),
				retryAfter: res?.Headers.RetryAfter?.Delta,
				statusCode: ex.StatusCode,
				inner: 		ex);
		}

		// ? Extract json data
		try {
			var data = await res.Content.ReadFromJsonAsync<T>()
				?? throw new ParseException(
					Msg($"Response from {fullUri()} deserialized to null"));

			Echo($"Successful response from {fullUri()}");
			return data;
		}
		catch (OperationCanceledException ex) {
			throw new StreamException(
				message: Msg($"Failed to deserialize response from {fullUri()}"),
				inner: 	ex);
		}
	}

	/// <summary>
	/// The primary public entry point for sending HTTP requests. Executes <see cref="TrySendAsync{T}"/>
	/// within the configured retry policy. A factory is used instead of a direct message instance
	/// to allow the request to be reconstructed on each retry, since <see cref="HttpRequestMessage"/>
	/// cannot be reused after it has been sent.
	/// </summary>
	/// <typeparam name="T">The type to deserialize the successful response body into.</typeparam>
	/// <param name="reqFactory">
	/// A factory function that produces a fresh <see cref="HttpRequestMessage"/> for each attempt.
	/// </param>
	/// <returns>A <see cref="Task"/> resolving to the deserialized response of type <typeparamref name="T"/>.</returns>
	public async Task<T> SendAsync<T>(Func<HttpRequestMessage> reqFactory) {
		return await _retryPolicy.ExecuteAsync(async () => 
			await TrySendAsync<T>(reqFactory()));
	}
}
