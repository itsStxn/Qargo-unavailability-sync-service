using Polly;
using System;
using Root.Utils;
using Polly.Retry;
using Root.Interfaces;
using System.Text.Json;
using System.Net.Http.Json;

namespace Root.Core;

public class MyRequest : IMyRequest {
	protected readonly RequestHandle _rh;
	private readonly AsyncRetryPolicy _retryPolicy;

	public MyRequest(RequestHandle rh) {
		_rh = rh;
		_retryPolicy = Policy
			.Handle<HttpRequestException>()
			.WaitAndRetryAsync(
				retryCount: _rh.MaxAttempts,
				onRetryAsync: async (ex, delay) => 
					await OnRetryAsync(ex, delay),
				sleepDurationProvider: attempt => 
					TimeSpan.FromSeconds(Math.Pow(2, attempt))
			);
	}

	protected string BuildFullUri(Uri? path) {
		if (_rh.Client.BaseAddress == null)
			throw new Exception("The base address of HttpClient is null");

		if (path == null || path.ToString().Trim() == string.Empty)
			throw new Exception("The URI in HttpRequestMessage is null or empty");

		return new Uri(_rh.Client.BaseAddress, path).ToString().Trim();
	}

	protected virtual Task OnRetryAsync(Exception ex, TimeSpan delay) {
		Console.WriteLine($"SendAsync HTTP error, retrying after {delay.TotalSeconds}s: {ex.Message}");
		return Task.CompletedTask;
	}

	protected virtual async Task<T?> TrySendAsync<T>(HttpRequestMessage req) {
		string fullUri() => BuildFullUri(req.RequestUri);
		HttpResponseMessage res;

		// ? Get http response
		try {
			res = await _rh.Client.SendAsync(req, _rh.CT);
			res.EnsureSuccessStatusCode();
		}
		catch (HttpRequestException ex) {
			throw new Exception(
				$"Request to {fullUri()} failed: {ex.StatusCode} {ex.Message}", ex);
		}

		// ? Extract json data
		try {
			var data = await res.Content.ReadFromJsonAsync<T>()
				?? throw new Exception($"Response from {fullUri()} deserialized to null");
			Console.WriteLine($"Successful response from {fullUri()}");
			return data;
		}
		catch (JsonException ex) {
			throw new Exception($"Failed to deserialize response from {fullUri()}", ex);
		}
	}

	public virtual async Task<T?> SendAsync<T>(HttpRequestMessage req) {
		return await _retryPolicy.ExecuteAsync(async () => await TrySendAsync<T>(req));
	}
}
