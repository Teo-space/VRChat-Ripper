using System;
using Newtonsoft.Json;
using VRC.API.Parsers;

namespace VRC.API
{
	[Serializable]
	public class ApiConfig
	{
		public string messageOfTheDay { get; set; }
		public ApiEvent events { get; set; }

		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId timeOutWorldId { get; set; }

		public string gearDemoRoomId { get; set; }
		public string releaseServerVersionStandalone { get; set; }
		public string downloadLinkWindows { get; set; }
		public string releaseAppVersionStandalone { get; set; }
		public string devAppVersionStandalone { get; set; }
		public string devServerVersionStandalone { get; set; }
		public string devDownloadLinkWindows { get; set; }
		public int currentTOSVersion { get; set; }
		public string releaseSdkUrl { get; set; }
		public string releaseSdkVersion { get; set; }
		public string devSdkUrl { get; set; }
		public string devSdkVersion { get; set; }
		public string[] whiteListedAssetUrls { get; set; }
		public string clientApiKey { get; set; }
		public string viveWindowsUrl { get; set; }
		public string sdkUnityVersion { get; set; }

		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId hubWorldId { get; set; }

		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId homeWorldId { get; set; }

		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId tutorialWorldId { get; set; }

		public bool disableEventStream { get; set; }
		public bool disableAvatarGating { get; set; }
		public bool disableFeedbackGating { get; set; }
		public bool disableRegistration { get; set; }
		public bool disableUpgradeAccount { get; set; }
		public bool disableCommunityLabs { get; set; }
		public bool disableCommunityLabsPromotion { get; set; }
		public bool disableTwoFactorAuth { get; set; }
		public bool disableSteamNetworking { get; set; }
		public bool disableUdon { get; set; }
		public string plugin { get; set; }
		public string sdkNotAllowedToPublishMessage { get; set; }
		public string sdkDeveloperFaqUrl { get; set; }
		public string sdkDiscordUrl { get; set; }
		public string notAllowedToSelectAvatarInPrivateWorldMessage { get; set; }
		public int userVerificationTimeout { get; set; }
		public int userUpdatePeriod { get; set; }
		public int userVerificationDelay { get; set; }
		public int userVerificationRetry { get; set; }
		public int worldUpdatePeriod { get; set; }
		public int moderationQueryPeriod { get; set; }
		public int clientDisconnectTimeout { get; set; }

		[JsonConverter(typeof(VrcIdConverter))]
		public VrcId defaultAvatar { get; set; }

		public ApiWorldRow[] dynamicWorldRows { get; set; }
		public bool disableAvatarCopying { get; set; }
		public ApiAnnouncement[] announcements { get; set; }
		public bool useReliableUdpForVoice { get; set; }
		public int updateRateMsMaximum { get; set; }
		public int updateRateMsMinimum { get; set; }
		public int updateRateMsNormal { get; set; }
		public int clientBPSCeiling { get; set; }
		public int clientSentCountAllowance { get; set; }
		public int uploadAnalysisPercent { get; set; }
		public string address { get; set; }
		public string contactEmail { get; set; }
		public string supportEmail { get; set; }
		public string jobsEmail { get; set; }
		public string copyrightEmail { get; set; }
		public string moderationEmail { get; set; }
		public bool disableEmail { get; set; }
		public string appName { get; set; }
		public string serverName { get; set; }
		public string deploymentGroup { get; set; }
		public string buildVersionTag { get; set; }
		public string apiKey { get; set; }

		public override string ToString()
		{
			return
				"[Config]\n" +
				$"SdkVersion:  {releaseSdkVersion}\n" +
				$"Server:      {releaseServerVersionStandalone}\n" +
				$"UnityVersion {sdkUnityVersion}\n" +
				$"ApiKey:      {apiKey}\n";
		}
	}
}