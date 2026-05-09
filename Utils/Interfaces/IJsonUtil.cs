using System;

namespace Root.Utils.Interfaces;

public interface IJsonUtil {
	public static abstract string ToPrettyJson<T>(T data, bool print = false);
}
