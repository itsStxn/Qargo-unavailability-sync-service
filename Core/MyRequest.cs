using Polly;
using System;
using Root.Source;
using Polly.Retry;
using Root.Errors;
using Root.Core.Interfaces;
using System.Net.Http.Json;

namespace Root.Core;

public class MyRequest : Base, IMyRequest {
	protected readonly RequestSource _rs;
	private readonly AsyncRetryPolicy _retryPolicy;

	public MyRequest(RequestSource rs) {
		_rs = rs;
		_retryPolicy = Policy
			.Handle<NetworkException>()
			.WaitAndRetryAsync(
				retryCount: _rs.MaxAttempts,
				sleepDurationProvider: (attempt, ex, ctx) => 
					Timeout(attempt, ex),
				onRetryAsync: async (ex, delay, attempt, ctx) => 
					await OnRetryAsync(ex, delay)
			);
	}

	protected string BuildFullUri(HttpRequestMessage req) {
		// ? Validate HttpClient's base address
		var baseUri = _rs.Client.BaseAddress;
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

	private TimeSpan Timeout(int attempt, Exception ex) {
		// ? Try to respect server's rightAfter clause
		var netEx = AppException.Wrap<NetworkException>(ex);
		
		if (netEx.RetryAfter.HasValue) {
			var time = netEx.RetryAfter.Value;
			Warn($"Rate limited, retrying after {time.TotalSeconds}s...");
			return time;
		}

		// ? Manually calculate timeout duration
		var delayMs = Math.Min(1000, 50 * Math.Pow(2, attempt - 1)); // ? Capped at 1s
		return TimeSpan.FromMilliseconds(delayMs);
	}

	protected virtual Task<NetworkException> OnRetryAsync(Exception ex, TimeSpan delay) {
		Warn($"Network error, retrying after {delay.TotalSeconds}s: {ex.Message}");
		return Task.FromResult(AppException.Wrap<NetworkException>(ex));
	}

	protected virtual async Task<T> TrySendAsync<T>(HttpRequestMessage req) {
		string fullUri() => BuildFullUri(req);
		HttpResponseMessage? res = null;

		// ? Get http response
		try {
			Echo($"Fetching result at {fullUri()}...");
			res = await _rs.Client.SendAsync(req, _rs.Ct);
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

	public async Task<T> SendAsync<T>(Func<HttpRequestMessage> reqFactory) {
		return await _retryPolicy.ExecuteAsync(async () => 
			await TrySendAsync<T>(reqFactory()));
	}
}
