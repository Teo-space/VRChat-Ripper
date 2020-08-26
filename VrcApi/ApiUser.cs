using System;
using Newtonsoft.Json;
using VRC.API.Parsers;

namespace VRC.API
{
	[Serializable]
	public class ApiUser
	{
		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId id { get; set; }

		public string username { get; set; }
		public string displayName { get; set; }
		public string bio { get; set; }
		public string[] bioLinks { get; set; }
		public string currentAvatarImageUrl { get; set; }
		public string currentAvatarThumbnailImageUrl { get; set; }
		public string status { get; set; }
		public string statusDescription { get; set; }
		public string state { get; set; }

		[JsonConverter(typeof(ApiUserTagConverter))]
		public ApiUserTag[] tags { get; set; }

		public string developerType { get; set; }

		[JsonConverter(typeof(VrcDateTimeConverter))]
		public DateTime? last_login { get; set; }

		public string last_platform { get; set; }
		public bool allowAvatarCopying { get; set; }
		public bool isFriend { get; set; }
		public string friendKey { get; set; }
		public string location { get; set; }

		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId worldId { get; set; }

		public string instanceId { get; set; }

		public override string ToString()
		{
			return
				"[User]\n" +
				$"Id:          {id}\n" +
				$"UserName:    {username}\n" +
				$"DisplayName: {displayName}\n" +
				$"location:    {location}\n";
		}
	}
}