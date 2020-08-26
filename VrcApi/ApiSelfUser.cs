using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VRC.API.Parsers;

namespace VRC.API
{
	[Serializable]
	public class VrcUserNameHistory
	{
		public string displayName { get; set; }

		[JsonConverter(typeof(VrcDateTimeConverter))]
		public DateTime? updated_at { get; set; }
	}

	[Serializable]
	public class VrcFeature
	{
		public bool twoFactorAuth { get; set; }
	}

	[Serializable]
	public class ApiSelfUser : ApiUser
	{
		public VrcUserNameHistory[] pastDisplayNames;
		public bool hasEmail { get; set; }
		public bool hasPendingEmail { get; set; }
		public string email { get; set; }
		public string obfuscatedEmail { get; set; }
		public string obfuscatedPendingEmail { get; set; }
		public bool emailVerified { get; set; }
		public bool hasBirthday { get; set; }
		public bool unsubscribe { get; set; }

		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId[] friends { get; set; }

		public string[] friendGroupNames { get; set; }

		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId currentAvatar { get; set; }

		public string currentAvatarAssetUrl { get; set; }

		[JsonConverter(typeof(VrcDateTimeConverter))]
		public DateTime? accountDeletionDate { get; set; }

		public uint acceptedTOSVersion { get; set; }
		public string steamId { get; set; }
		public JObject steamDetails { get; set; }
		public string oculusId { get; set; }
		public string hasLoggedInFromClient { get; set; }
		public string homeLocation { get; set; }
		public bool twoFactorAuthEnabled { get; set; }
		public VrcFeature feature { get; set; }

		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId[] onlineFriends { get; set; }

		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId[] activeFriends { get; set; }

		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId[] offlineFriends { get; set; }
	}
}