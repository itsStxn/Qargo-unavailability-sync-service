using System;
using Root.DTOs;

namespace Root.Core.Interfaces;

public interface ITenant {
	public Task<ResourceList> GetResourcesAsync();
	public Task<T> GetUnavailabilitiesAsync<T>(string resourceId);
}
