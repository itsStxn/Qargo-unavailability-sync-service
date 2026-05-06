using System;

namespace Root.Constants;

public class Constants {
		public const string BASEURL 	= "https://api.qargo.com/v1/";
		public const int MAX_ATTEMPTS = 5; // ? Max http request attempts
		public const int CT_TIMEOUT 	= 10; // ? Cancellation token timeout
		public const int TIMEOUT 	 	= 10; // ? Http request timeout
}
