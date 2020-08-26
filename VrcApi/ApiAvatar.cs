using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VRC.API.Parsers;

namespace VRC.API
{
	[Serializable]
	public class ApiAvatar
	{
		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId id { get; set; }

		public string name { get; set; }
		public string description { get; set; }

		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId authorId { get; set; }

		public string authorName { get; set; }
		public string[] tags { get; set; }
		public string assetUrl { get; set; }
		public JObject assetUrlObject { get; set; }
		public string imageUrl { get; set; }
		public string thumbnailImageUrl { get; set; }
		public string releaseStatus { get; set; }
		public int version { get; set; }
		public bool featured { get; set; }
		public ApiUnityPackage[] unityPackages { get; set; }
		public bool unityPackageUpdated { get; set; }
		public string unityPackageUrl { get; set; }
		public JObject unityPackageUrlObject { get; set; }

		[JsonConverter(typeof(VrcDateTimeConverter))]
		public DateTime? created_at { get; set; }

		[JsonConverter(typeof(VrcDateTimeConverter))]
		public DateTime? updated_at { get; set; }


		public override string ToString()
		{
			return "[Avatar]\n" +
			       $"Id:          {id}\n" +
			       $"Name:        {name}\n" +
			       $"Description: {description}\n" +
			       $"Author:      {authorName}\n";
		}
	}
}