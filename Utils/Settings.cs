using System;
using static Root.Constants.Constants;

namespace Root.Utils;

public class Settings {
	static public (EnvLoader, RequestHandle) Init() {
		// ? Load environment variables
		var env = new EnvLoader();

		// ? Define cancellation token
		var cts = new CancellationTokenSource(TimeSpan.FromSeconds(CT_TIMEOUT));
		
		// ? Define request handle
		var handle = new RequestHandle(
			ct: cts.Token,
			maxAttempts: MAX_ATTEMPTS,
			client: new HttpClient() {
				BaseAddress = new Uri(BASEURL),
				Timeout = TimeSpan.FromSeconds(TIMEOUT)
			}
		);

		return (env, handle);
	}
}
