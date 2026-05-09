using System;

namespace Root.Utils.Interfaces;

public interface IAccessTokenUtil {
	public string ReadCache();
	public void CreateCache(string token);
}
