using System;
using Root.DTOs;
using Root.Utils;
using Root.Interfaces;

namespace Root.Core;

public class Tenant : ITenant {
	private readonly MyAuthRequest _auth;

	public Tenant(string name, string clientId, string secret, RequestHandle rh) {
		_auth = new MyAuthRequest(name, clientId, secret, rh);
	}

	public Task<Resource?> GetResourcesAsync() {
		// TODO: code...
		return default;
	}
	public Task<T?> GetUnavailabilitiesAsync<T>(string resourceId) {
		// TODO: code...
		return default;
	}
}
