using System;
using Root.Utils;
using Root.Interfaces;

namespace Root.Core;

public class Qargo : Tenant, IQargo {
	public Qargo(EnvLoader el, RequestHandle rh) : base(
		name:     "Qargo",
		clientId: el.Load("QARGO_CLIENT_ID"),
		secret:   el.Load("QARGO_SECRET"),
		rh
	) { }

	public Task UpdateUnavailabilitiesAsync(string resourceId, MasterUnavailabilityActions actions) {
		// TODO: code...
		return default;
	}
	
	public Task CreateUnavailabilitiesAsync(string resourceId, MasterUnavailabilityActions actions) {
		// TODO: code...
		return default;
	}
}
