using System;

namespace VRC.API
{
	[Serializable]
	public class ApiFriendStatus
	{
		public bool isFriend { get; set; }
		public bool outgoingRequest { get; set; }
		public bool incomingRequest { get; set; }
	}
}