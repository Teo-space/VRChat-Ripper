using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VRC.API.Parsers;

namespace VRC.API
{
	[Serializable]
	public class ApiNotification
	{
		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId id { get; set; }

		public string type { get; set; }

		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId senderUserId { get; set; }

		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId receiverUserId { get; set; }

		public string message { get; set; }
		public JObject details { get; set; } // unknown
		public string jobName { get; set; }
		public string jobColor { get; set; }
	}
}