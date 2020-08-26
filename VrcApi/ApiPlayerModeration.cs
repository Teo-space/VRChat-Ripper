using System;
using Newtonsoft.Json;
using VRC.API.Parsers;

namespace VRC.API
{
	[Serializable]
	public class ApiPlayerModeration
	{
		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId id { get; set; }

		public string type { get; set; }

		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId sourceUserId { get; set; }

		public string sourceDisplayName { get; set; }

		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId targetUserId { get; set; }

		public string targetDisplayName { get; set; }

		[JsonConverter(typeof(VrcDateTimeConverter))]
		public DateTime created { get; set; }

		public override string ToString()
		{
			return $"You {sourceDisplayName}, have {type}'d {targetDisplayName}";
		}
	}
}