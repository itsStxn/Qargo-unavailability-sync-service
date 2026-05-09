using System;

namespace Root;

public class Constants {
		public const string BASEURL 	 	= "https://api.qargo.com/v1/";
		public const string UNAVAIL_YEAR = "2025";

		public const int REQ_RETRY_TIMEOUT = 1000; // ? Timeout until next http request (milliseconds)
		public const int REQ_TIMEOUT 	 	  = 10; // ? HTTP request timeout duration (seconds)
		public const int REQ_MAX_ATTEMPTS  = 5; // ? Max http request attempts
		public const int CT_TIMEOUT 	 	  = 10; // ? Cancellation token timeout (minutes)
		public const int LOGS_TTL 	 	 	  = 3; // ? How long longs are retained (days)
}
