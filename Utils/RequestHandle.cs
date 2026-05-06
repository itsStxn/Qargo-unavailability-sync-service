using System;

namespace Root.Utils;

public class RequestHandle {
	public readonly HttpClient Client;
	public readonly CancellationToken CT;
	public readonly int MaxAttempts;

	public RequestHandle(HttpClient client, CancellationToken ct, int maxAttempts) {
		MaxAttempts = maxAttempts;
		Client = client;
		CT = ct;
	}
}
