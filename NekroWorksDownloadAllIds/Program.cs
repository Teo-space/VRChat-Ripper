using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace NekroWorks
{
	class Program
	{
		private static readonly Regex assetRegex = new Regex(@"(avtr_[a-f0-9]{8}\-[a-f0-9]{4}\-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12})");
		static void Main()
		{
			string[] ids;

			Console.WriteLine($"Starting download...");
			using (var client = new WebClient())
			{
				String str = client.DownloadString("https://nekro-works.de/client/98235789323/NW_client/list_all_avatars.php");

				Console.WriteLine($"Parsing data...");
				var mc = assetRegex.Matches(str);
				ids = new string[mc.Count];
				for (int i = 0; i < mc.Count; i++)
					ids[i] = mc[i].ToString();
			}
			Console.WriteLine($"Got {ids.Length} id's");

			Console.WriteLine($"Removing duplicates...");
			var distinct = ids.Distinct();

			Console.WriteLine($"Writing {distinct.Count()} id's to file");
			File.WriteAllLines("avatars.txt", distinct);
		}
	}
}
