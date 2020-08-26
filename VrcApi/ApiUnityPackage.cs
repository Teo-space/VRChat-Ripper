using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VRC.API.Parsers;

namespace VRC.API
{
	[Serializable]
	public class ApiUnityPackage
	{
		[JsonProperty("id")]
		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId Id { get; set; }

		[JsonProperty("assetUrl")] public string AssetUrl { get; set; }

		[JsonProperty("pluginUrl")] public string PluginUrl { get; set; }

		[JsonProperty("unityVersion")] public string UnityVersion { get; set; }

		[JsonProperty("unitySortNumber")] public long UnitySortNumber { get; set; }

		[JsonProperty("assetVersion")] public int AssetVersion { get; set; }

		[JsonProperty("platform")] public string Platform { get; set; }

		[JsonProperty("created_at")]
		[JsonConverter(typeof(VrcDateTimeConverter))]
		public DateTime? CreatedAt { get; set; }

		[JsonProperty("assetUrlObject")] public JObject AssetUrlObject { get; set; }

		[JsonProperty("pluginUrlObject")] public JObject PluginUrlObject { get; set; }
	}
}