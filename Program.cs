using System;
using Root.Core;
using Root.Utils;

namespace Root;

class Program {
	static public async Task Main(string[] args) {
		// ? Get env variables and http handle
		var (env, handle) = Settings.Init();

		// ? Define tenants
		var qargo = new Qargo(env, handle);
		var master = new Master(env, handle);

		// TODO: Implement access token cache
		// TODO: Implement a logger with tenant identity feature
		// TODO: Implement tenant operations
		// TODO: Implement parallel flow logic
		// TODO: Implement inter-environment operations
	}
}
