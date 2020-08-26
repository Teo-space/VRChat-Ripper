using System;

namespace VRC.API
{
	[Serializable]
	public class ApiWorldRow
	{
		public string name { get; set; }
		public string sortHeading { get; set; }
		public string sortOwnership { get; set; }
		public string sortOrder { get; set; }
		public string platform { get; set; }
		public string tag { get; set; }
		public int index { get; set; }
	}
}