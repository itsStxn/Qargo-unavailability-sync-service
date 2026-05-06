using System;

namespace Root.Utils;

public class EnvLoader {
	public EnvLoader() {
		DotNetEnv.Env.Load();
	}
	
	public string Load(string varName) {
		return Environment.GetEnvironmentVariable(varName)
			?? throw new Exception($"\"{varName}\" environment variable is not set");
	}
}
