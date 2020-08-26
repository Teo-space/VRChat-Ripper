using System;

namespace VRC.API
{
	[Serializable]
	public class ApiWorldInstance
	{
		public string id { get; set; }
		public int occupants { get; set; }
	}
}
