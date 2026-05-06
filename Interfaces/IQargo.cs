using System;
using Root.Utils;

namespace Root.Interfaces;

public interface IQargo {
	public Task UpdateUnavailabilitiesAsync(string resourceId, MasterUnavailabilityActions actions);
	public Task CreateUnavailabilitiesAsync(string resourceId, MasterUnavailabilityActions actions);
}
