using System;

namespace VRC.API
{
	[Serializable]
	public class ApiEvent
	{
		public int distanceClose { get; set; }
		public int distanceFactor { get; set; }
		public int distanceFar { get; set; }
		public int groupDistance { get; set; }
		public int maximumBunchSize { get; set; }
		public int notVisibleFactor { get; set; }
		public int playerOrderBucketSize { get; set; }
		public int playerOrderFactor { get; set; }
		public int slowUpdateFactorThreshold { get; set; }
		public int viewSegmentLength { get; set; }
	}
}