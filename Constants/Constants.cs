using System;
using System.Text.Json;

namespace Root.Constants;

public class Constants {
		public const string BASEURL 	= "https://api.qargo.com/v1/";
		public const int MAX_ATTEMPTS = 3; // ? Max http request attempts
		public const int CT_TIMEOUT 	= 10; // ? Cancellation token timeout
		public const int TIMEOUT 	 	= 10; // ? Http request timeout
		public const int LOGS_TTL 	 	= 7; // ? How long longs are retained
		public static readonly JsonSerializerOptions JSON_OPTIONS = new() { WriteIndented = true };

}
