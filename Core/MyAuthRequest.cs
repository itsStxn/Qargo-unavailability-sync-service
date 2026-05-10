using System;
using Root.DTOs;
using Root.Utils;
using System.Net;
using System.Text;
using Root.Errors;
using Root.Source;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace Root.Core;

/// <summary>
/// Extends <see cref="MyRequest"/> with OAuth2 client credentials authentication.
/// Automatically injects Bearer tokens into outgoing requests, and renews the access token
/// on <c>401 Unauthorized</c> responses via the inherited retry policy.
/// </summary>
public class MyAuthRequest : MyRequest {

	/// <summary>The OAuth2 client ID used to obtain access tokens.</summary>
	private readonly string _clientId;

	/// <summary>The OAuth2 client secret used alongside <see cref="_clientId"/> to obtain access tokens.</summary>
	private readonly string _secret;

	/// <summary>The currently active Bearer access token. Populated from cache on construction and refreshed on expiry.</summary>
	private string _accessToken;

	/// <summary>Utility for reading and writing the access token cache.</summary>
	private readonly AccessTokenUtil _atu;


	/// <summary>
	/// Initializes a new instance of <see cref="MyAuthRequest"/>, restoring the access token from cache if available.
	/// </summary>
	/// <param name="name">The service name passed to <see cref="Base"/> for log message prefixing.</param>
	/// <param name="clientId">The OAuth2 client ID.</param>
	/// <param name="secret">The OAuth2 client secret.</param>
	/// <param name="http">The <see cref="HttpSource"/> instance to use for all requests.</param>
	public MyAuthRequest(string name, string clientId, string secret, HttpSource http) : base(http, name) {
		_atu = new AccessTokenUtil(name);
		_accessToken = _atu.ReadCache();
		_clientId = clientId;
		_secret = secret;
	}


	/// <summary>
	/// Fetches a new access token from the auth endpoint using Basic authentication
	/// and the configured client credentials, then persists it to cache.
	/// </summary>
	/// <remarks>
	/// Encodes <see cref="_clientId"/> and <see cref="_secret"/> as a Base64 Basic credential,
	/// then POSTs to <c>auth/token</c> with a <c>client_credentials</c> grant type.
	/// </remarks>
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
		_atu.CreateCache(_accessToken);
	}

	/// <summary>
	/// Extends the base retry hook to trigger token renewal on <c>401 Unauthorized</c> responses.
	/// All other cases are delegated to <see cref="MyRequest.OnRetryAsync"/>.
	/// </summary>
	/// <param name="ex">The exception that triggered the retry.</param>
	/// <param name="delay">The delay duration that will be observed before the next attempt.</param>
	/// <returns>A <see cref="Task"/> resolving to the wrapped <see cref="NetworkException"/>.</returns>
	protected override async Task<NetworkException> OnRetryAsync(Exception ex, TimeSpan delay) {
		var netEx = await base.OnRetryAsync(ex, delay);

		// ? Inject access token if necessary
		if (netEx.StatusCode == HttpStatusCode.Unauthorized) {
			await RenewAccessTokenAsync();
		}

		return netEx;
	}

	/// <summary>
	/// Extends the base send logic to inject a Bearer token into the request's
	/// <c>Authorization</c> header before dispatch.
	/// Requests already using Basic auth (e.g. token renewal) are passed through unmodified.
	/// </summary>
	/// <remarks>
	/// If the access token is empty and the request is not a Basic auth request, a
	/// <see cref="NetworkException"/> with status <see cref="HttpStatusCode.Unauthorized"/> is thrown
	/// deliberately to trigger the retry policy and force a token renewal via <see cref="OnRetryAsync"/>.
	/// </remarks>
	/// <typeparam name="T">The type to deserialize the response body into.</typeparam>
	/// <param name="req">The <see cref="HttpRequestMessage"/> to send.</param>
	/// <returns>A <see cref="Task"/> resolving to the deserialized response of type <typeparamref name="T"/>.</returns>
	/// <exception cref="NetworkException">
	/// Thrown with <see cref="HttpStatusCode.Unauthorized"/> if the access token is empty,
	/// to intentionally trigger the retry policy.
	/// </exception>
	protected override async Task<T> TrySendAsync<T>(HttpRequestMessage req) where T : default {
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
