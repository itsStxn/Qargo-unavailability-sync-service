using System;
using System.Text.Json;

namespace Root.Constants;

public class Constants {
		public const string BASEURL 	 	= "https://api.qargo.com/v1/";
		public const string UNAVAIL_YEAR = "2025";

		public const int MAX_ATTEMPTS  = 5; // ? Max http request attempts
		public const int CT_TIMEOUT 	 = 10; // ? Cancellation token timeout (seconds)
		public const int TIMEOUT 	 	 = 10; // ? Http request timeout (seconds)
		public const int RETRY_TIMEOUT = 1000; // ? Timeout for next http request (milliseconds)
		public const int LOGS_TTL 	 	 = 7; // ? How long longs are retained (seconds)
		
		public static readonly JsonSerializerOptions JSON_OPTIONS = new() { WriteIndented = true };

}
