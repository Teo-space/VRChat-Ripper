using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace YamLib
{
	public static class AppConfig
	{
		private const string configName = "config.json";

		private static readonly Dictionary<string, string> appConf = File.Exists(configName)
			? JsonConvert.DeserializeObject<Dictionary<string, string>>(
				File.ReadAllText(configName, Encoding.UTF8))
			: new Dictionary<string, string>();

		public static void EnsureKey(string key)
		{
			lock (appConf)
			{
				var updated = false;
				if (!appConf.TryGetValue(key, out var _))
				{
					appConf.Add(key, "");
					updated = true;
				}

				if (updated)
					File.WriteAllText(configName, JsonConvert.SerializeObject(appConf, Formatting.Indented));
			}
		}

		public static void Upsert(string key, string value)
		{
			lock (appConf)
			{
				var updated = false;
				if (appConf.TryGetValue(key, out var oldvalue))
				{
					if (oldvalue != value)
					{
						updated = true;
						appConf[key] = value;
					}
				}
				else
				{
					updated = true;
					appConf.Add(key, value);
				}

				if (updated)
					File.WriteAllText(configName, JsonConvert.SerializeObject(appConf, Formatting.Indented));
			}
		}

		public static bool TryGet(string key, out string value)
		{
			lock (appConf)
			{
				return appConf.TryGetValue(key, out value);
			}
		}

		public static string GetOrDefault(string key)
		{
			if (TryGet(key, out var value))
				return value;
			return null;
		}
	}
}