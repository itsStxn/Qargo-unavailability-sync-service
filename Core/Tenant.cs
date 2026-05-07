using System;
using Root.DTOs;
using Root.Source;
using Root.Errors;
using Root.Core.Interfaces;
using static Root.Constants.Constants;
using Root.DTOs.ResourceListComponents;
using Root.DTOs.UnavailabilityListComponents;

namespace Root.Core;

public class Tenant : Base, ITenant {
	private readonly MyAuthRequest _auth;

	public Tenant(string name, string clientId, string secret, RequestSource rs) {
		_auth = new MyAuthRequest(name, clientId, secret, rs);
		_name = name;
	}

	public async Task<List<Resource>> GetResourcesAsync() {
		var resources = new List<Resource>();
		string? next = null;
		ResourceList data;
		
		try {
			do {
				// ? Prepare uri
				var path = "resources/resource";
				path += next == null ? string.Empty : $"?cursor={next}";
				
				// ? Fetch resources
				data = await _auth.SendAsync<ResourceList>(() =>
					new HttpRequestMessage(HttpMethod.Get, path));
				
				// ? Store and run until no cursor is found
				resources.AddRange(data.Items);
				next = data.NextCursor;
			}
			while (next != null && data.Items.Count > 0);
			return resources;
		}
		catch (Exception ex) {
			throw AppException.Label<AppException>(ex, Msg(ex.Message));
		}
	}

	public async Task<List<Unavailability>> GetUnavailabilitiesAsync(string resourceId) {
		var unavails = new List<Unavailability>();
		string? next = null;
		UnavailabilityList data;
		
		try {
			do {
				// ? Prepare uri
				var path = $"resources/resource/{resourceId}/unavailability";
				path += next == null ? string.Empty : $"?cursor={next}";
				
				// ? Fetch resources
				data = await _auth.SendAsync<UnavailabilityList>(() =>
					new HttpRequestMessage(HttpMethod.Get, path));
				
				// ? Filter by year and store
				unavails.AddRange(data.Items.Where(u => 
					u.StartTime.Contains(UNAVAIL_YEAR) && 
					(u.EndTime ?? string.Empty).Contains(UNAVAIL_YEAR)
				));

				// ? Run until no cursor is found
				next = data.NextCursor;
			}
			while (next != null && data.Items.Count > 0);
			return unavails;
		}
		catch (Exception ex) {
			throw AppException.Label<AppException>(ex, Msg(ex.Message));
		}
	}
}
