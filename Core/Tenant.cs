using System;
using Root.DTOs;
using Root.Source;
using Root.Core.Interfaces;

namespace Root.Core;

public class Tenant : Base, ITenant {
	private readonly MyAuthRequest _auth;

	public Tenant(string name, string clientId, string secret, RequestSource rs) {
		_auth = new MyAuthRequest(name, clientId, secret, rs);
		_name = name;
	}

	public Task<ResourceList?> GetResourcesAsync() {
		// TODO: code...
		return default;
	}
	public Task<T?> GetUnavailabilitiesAsync<T>(string resourceId) {
		// TODO: code...
		return default;
	}
}
