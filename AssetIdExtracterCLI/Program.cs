using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VRC;

namespace ListMerger
{
	class Program
	{
		/// <summary>
		///     Selects all valid id's
		/// </summary>
		private static readonly Regex assetRegex = new Regex(@"((?:avtr|usr|wrld|wld|unp|pmod|not|authcookie|file)_[a-f0-9]{8}\-[a-f0-9]{4}\-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12})");

		public static bool TryGetVrcIdList(string input, out VrcId[] vrcIdList)
		{
			try
			{
				var mc = assetRegex.Matches(input);
				vrcIdList = new VrcId[mc.Count];
				for (int i = 0; i < mc.Count; i++)
					vrcIdList[i] = new VrcId(mc[i].ToString());
				return true;
			}
			catch (Exception)
			{
			}

			vrcIdList = null;
			return false;
		}

		static void Main(string[] args)
		{
#if DEBUG
			args = (new List<String>()
			{
				"avatars.json",
				"avatars.txt"
			}).ToArray();
#endif

			if (args.Length < 2)
			{
				Console.WriteLine("Please select one or more two files containing vrc ids which you want to extract the ids out of");
				return;
			}

			var idList = new List<VrcId>();

			foreach (string arg in args)
			{
				if (!File.Exists(arg))
				{
					Console.WriteLine($"\"{arg}\" is not a file, or doesnt exist.\nPlease verify your arguments");
					return;
				}

				Console.WriteLine($"Parsing \"{arg}\"...");

				var e = File.OpenText(arg);
				while (!e.EndOfStream)
				{
					string str = e.ReadLine();

					if (TryGetVrcIdList(str, out var ids))
						idList.AddRange(ids);
				}
				e.Close();
			}

			var output = new List<String>();
			var distinctIdList = idList.Distinct().ToList();

			distinctIdList.RemoveAll(i =>
				i.IdType != VrcId.VrcIdType.Avatar && i.IdType != VrcId.VrcIdType.World &&
				i.IdType != VrcId.VrcIdType.LegacyWorld);

			Console.WriteLine($"Removing duplicates...");
			foreach (var id in idList.Distinct())
				output.Add(id.ToString());


			Console.WriteLine($"Writing {output.Count} ID's to file...");
			File.WriteAllLines("merged.txt", output);


			Console.WriteLine($"Done.");
		}
	}
}
