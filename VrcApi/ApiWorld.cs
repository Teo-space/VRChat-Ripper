using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VRC.API.Parsers;

namespace VRC.API
{
	[Serializable]
	public class ApiWorld
	{
		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId id { get; set; }

		public string name { get; set; }
		public string description { get; set; }
		public bool featured { get; set; }

		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId authorId { get; set; }

		public string authorName { get; set; }
		public int capacity { get; set; }
		public string[] tags { get; set; }
		public string releaseStatus { get; set; }
		public string imageUrl { get; set; }
		public string thumbnailImageUrl { get; set; }
		public string assetUrl { get; set; }
		public JObject assetUrlObject { get; set; }
		public string pluginUrl { get; set; }
		public JObject pluginUrlObject { get; set; }
		public string unityPackageUrl { get; set; }
		public JObject unityPackageUrlObject { get; set; }

		[JsonProperty("namespace")]
		public string @namespace { get; set; }

		public ApiUnityPackage[] unityPackages { get; set; }
		public int version { get; set; }
		public string organization { get; set; }
		public string previewYoutubeId { get; set; }
		public int favorites { get; set; }

		[JsonConverter(typeof(VrcDateTimeConverter))]
		public DateTime? created_at { get; set; }

		[JsonConverter(typeof(VrcDateTimeConverter))]
		public DateTime? updated_at { get; set; }

		[JsonConverter(typeof(VrcDateTimeConverter))]
		public DateTime? publicationDate { get; set; }

		[JsonConverter(typeof(VrcDateTimeConverter))]
		public DateTime? labsPublicationDate { get; set; }

		public int visits { get; set; }
		public int popularity { get; set; }
		public int heat { get; set; }
		public int publicOccupants { get; set; }
		public int privateOccupants { get; set; }
		public int occupants { get; set; }
		[JsonConverter(typeof(ApiWorldInstanceConverter))]
		public ApiWorldInstance[] instances { get; set; }


		public override string ToString()
		{
			return "[World]\n" +
			       $"Id:          {id}\n" +
			       $"Name:        {name}\n" +
			       $"Description: {description}\n" +
			       $"Author:      {authorName}\n";
		}
	}
}