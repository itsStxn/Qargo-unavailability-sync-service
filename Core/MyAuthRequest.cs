using System;
using Root.DTOs;
using Root.Utils;
using System.Net;
using System.Text;
using Root.Source;
using Root.Errors;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace Root.Core;

public class MyAuthRequest : MyRequest {
	private readonly string _clientId;
	private readonly string _secret;
	private string _accessToken;
	private readonly AccessTokenCache _atc;

	public MyAuthRequest(string name, string clientId, string secret, RequestSource rs) : base(rs) {
		_atc = new AccessTokenCache(name);
		_accessToken = _atc.Read();
		_clientId = clientId;
		_secret = secret;
		_name = name;
	}

	private async Task RenewAccessTokenAsync() {
		// ? Generate credentials
		var cred = Convert.ToBase64String(
			Encoding.UTF8.GetBytes($"{_clientId}:{_secret}"));

		// ? Get and validate response
		var data = await SendAsync<AccessToken>(() => {
			// ? Build request
			var req = new HttpRequestMessage(HttpMethod.Post, "auth/token");
			req.Headers.Authorization = new AuthenticationHeaderValue("Basic", cred);
			req.Content = JsonContent.Create(new { grant_type = "client_credentials" });
			return req;
		});

		_accessToken = data.Token.Trim();
		_atc.Create(_accessToken);
	}

	protected override async Task<NetworkException> OnRetryAsync(Exception ex, TimeSpan delay) {
		var netEx = await base.OnRetryAsync(ex, delay);

		// ? Inject access token if necessary
		if (netEx.StatusCode == HttpStatusCode.Unauthorized) {
			await RenewAccessTokenAsync();
		}

		return netEx;
	}

	protected override async Task<T> TrySendAsync<T>(HttpRequestMessage req) where T: default {
		// ? Skip token injection if request is already using Basic auth
		if (req.Headers.Authorization?.Scheme != "Basic") {
			// ? Force pollying
			if (_accessToken == string.Empty) {
				throw new NetworkException(
					inner: null,
					message: Msg("Access token is empty"),
					statusCode: HttpStatusCode.Unauthorized
				);
			}

			// ? Inject access token
			req.Headers.Authorization = 
				new AuthenticationHeaderValue("Bearer", _accessToken);
		}

		return await base.TrySendAsync<T>(req);
	}
}
