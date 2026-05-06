using System;
using Root.Utils;

namespace Root.Core;

public class Master : Tenant {
	public Master(EnvLoader el, RequestHandle rh) : base(
		name:     "Master",
		clientId: el.Load("MASTER_CLIENT_ID"),
		secret:   el.Load("MASTER_SECRET"),
		rh
	) { }
}
