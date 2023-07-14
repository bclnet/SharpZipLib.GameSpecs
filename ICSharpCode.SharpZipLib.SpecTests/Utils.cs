using System;
using System.Globalization;
using System.Linq;

public static class Utils
{
	public static byte[] ParseKey(string str)
	{
		if (string.IsNullOrEmpty(str)) { return null; }
		if (str.StartsWith("aes:", StringComparison.OrdinalIgnoreCase))
		{
			var keyStr = str.Substring(4);
			var key = keyStr.StartsWith("/")
				? Enumerable.Range(0, keyStr.Length >> 2).Select(x => byte.Parse(keyStr.Substring((x << 2) + 2, 2), NumberStyles.HexNumber)).ToArray()
				: Enumerable.Range(0, keyStr.Length >> 1).Select(x => byte.Parse(keyStr.Substring(x << 1, 2), NumberStyles.HexNumber)).ToArray();
			return key;
		}
		else throw new ArgumentOutOfRangeException(nameof(str), str);
	}
}
