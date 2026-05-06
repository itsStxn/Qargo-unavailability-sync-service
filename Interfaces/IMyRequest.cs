using System;

namespace Root.Interfaces;

public interface IMyRequest {
	public Task<T?> SendAsync<T>(HttpRequestMessage req);
}
