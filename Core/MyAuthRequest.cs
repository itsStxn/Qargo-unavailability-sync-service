using System;
using Root.DTOs;
using System.Net;
using Root.Utils;
using System.Text;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace Root.Core;

public class MyAuthRequest : MyRequest {
	private readonly string _clientId;
	private readonly string _secret;
	private readonly string _name;
	private string _accessToken;

	public MyAuthRequest(string name, string clientId, string secret, RequestHandle rh) : base(rh) {
		_accessToken = string.Empty;
		_clientId = clientId;
		_secret = secret;
		_name = name;
	}

	private async Task SetAccessTokenAsync() {
		int attempt = 0;
		while (attempt < _rh.MaxAttempts) {
			// ? Build request
			var req = new HttpRequestMessage(HttpMethod.Post, "auth/token");
			Console.WriteLine($"(Attempt no {++attempt}) Fetching access token at {BuildFullUri(req.RequestUri)}");

			var cred = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_secret}"));
			req.Headers.Authorization = new AuthenticationHeaderValue("Basic", cred);
			req.Content = JsonContent.Create(new { grant_type = "client_credentials" });

			// ? Get and validate response
			var res = await _rh.Client.SendAsync(req, _rh.CT);

			if (res.StatusCode == HttpStatusCode.TooManyRequests) {
				var retryAfter = res.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(60);
				Console.WriteLine($"Rate limited, retrying after {retryAfter.TotalSeconds}s...");
				await Task.Delay(retryAfter, _rh.CT);
				continue; // ? retry
			}

			res.EnsureSuccessStatusCode();

			// ? Get and set new access token
			var data = await res.Content.ReadFromJsonAsync<AccessTokenResponse>(_rh.CT)
				?? throw new Exception("Failed to deserialize access token response");
			_accessToken = data.AccessToken.Trim();

			// ? Validate access token
			if (_accessToken.Length == 0)
				throw new Exception("Access token is unexpectedly an empty string");
			return;
		}
	}

	protected override async Task OnRetryAsync(Exception ex, TimeSpan delay) {
		await base.OnRetryAsync(ex, delay);
		// ? Inject access token
		await SetAccessTokenAsync();
	}

	protected override async Task<T?> TrySendAsync<T>(HttpRequestMessage req) where T: default {
		// ? Inject access token
		if (_accessToken != string.Empty) {
			req.Headers.Authorization = 
				new AuthenticationHeaderValue("Bearer", _accessToken);
		}
		return await base.TrySendAsync<T>(req);
	}

	public override async Task<T?> SendAsync<T>(HttpRequestMessage req) where T: default {
		// ? Inject access token
		if (_accessToken == string.Empty) {
			await SetAccessTokenAsync();
		}
		return await base.SendAsync<T>(req);
	}
}
