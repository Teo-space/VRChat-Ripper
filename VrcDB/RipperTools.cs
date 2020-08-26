using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using YamLib;
using YamLib.Connectivity;
using VRC;
using VRC.API;

namespace Ripper
{
	public static class RipperTools
	{
		/// <summary>
		///     Selects all valid id's
		/// </summary>
		private static readonly Regex assetRegex =
			new Regex(
				@"((?:avtr|usr|wrld|wld|pmod|unp|authcookie|file)_[a-f0-9]{8}\-[a-f0-9]{4}\-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12})");

		/// <summary>
		///     Selects all valid file id's and their version number from the vrchat cache
		/// </summary>
		private static readonly Regex vrcLibraryRegex =
			new Regex(
				@"https:\/\/api\.vrchat\.cloud\/api\/1\/file\/file_([a-f0-9]{8}\-[a-f0-9]{4}\-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12})\/([0-9]+)\/file");

		/// <summary>
		///     VRChat cache location
		/// </summary>
		private static readonly string
			vrcLibPath = $"{FilesystemTools.GetLocalLowDirectory()}\\VRChat\\VRChat\\Library";

		private static readonly JsonSerializerSettings settings = new JsonSerializerSettings
		{
			Formatting = Formatting.Indented,
			MissingMemberHandling = MissingMemberHandling.Ignore,
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore
		};

		public static bool WriteWorld(ApiWorld world, string path, bool downloadAsset, bool overwriteMetaData = false)
		{
			try
			{
				// Create a sub-path for the current version of the world to be stored in
				var currentVersionPath = $"{path}\\{world.version}";

				// Ensure folder
				_ = Directory.CreateDirectory(currentVersionPath);

				// Create version specific descriptor
				var data = JsonConvert.SerializeObject(world, settings);

				if (!File.Exists(path + "\\data.json") || overwriteMetaData)
					File.WriteAllText(path + "\\data.json", data, Encoding.UTF8);

				if (!File.Exists(currentVersionPath + "\\data.json") || overwriteMetaData)
					File.WriteAllText(currentVersionPath + "\\data.json", data, Encoding.UTF8);

				if (downloadAsset)
				{
					// Download!
					if (!File.Exists(currentVersionPath + "\\asset"))
						_ = Utils.DownloadFile(world.assetUrl, currentVersionPath + "\\asset");
					if (!File.Exists(currentVersionPath + "\\plugin"))
						_ = Utils.DownloadFile(world.pluginUrl, currentVersionPath + "\\plugin");
					if (!File.Exists(currentVersionPath + "\\thumbnail"))
						_ = Utils.DownloadFile(world.thumbnailImageUrl, currentVersionPath + "\\thumbnail");
					if (!File.Exists(currentVersionPath + "\\image"))
						_ = Utils.DownloadFile(world.imageUrl, currentVersionPath + "\\image");
				}

				return true;
			}
			catch (Exception ex)
			{
				Debug.LogLine($"Failed to write world:\n{ex.Message}", ConsoleColor.Red);
				return false;
			}
		}

		public static bool WriteAvatar(ApiAvatar avatar, string path, bool downloadAsset, bool overwriteMetaData = false)
		{
			try
			{
				// Create a sub-path for the current version of the world to be stored in
				var currentVersionPath = $"{path}\\{avatar.version}";

				// Ensure folder
				_ = Directory.CreateDirectory(currentVersionPath);

				// Create version specific descriptor
				var data = JsonConvert.SerializeObject(avatar, settings);
				
				if (!File.Exists(path + "\\data.json") || overwriteMetaData)
					File.WriteAllText(path + "\\data.json", data, Encoding.UTF8);

				if (!File.Exists(currentVersionPath + "\\data.json") || overwriteMetaData)
					File.WriteAllText(currentVersionPath + "\\data.json", data, Encoding.UTF8);

				if (downloadAsset)
				{
					// Download!
					if (!File.Exists(currentVersionPath + "\\asset"))
						_ = Utils.DownloadFile(avatar.assetUrl, currentVersionPath + "\\asset");
					if (!File.Exists(currentVersionPath + "\\thumbnail"))
						_ = Utils.DownloadFile(avatar.thumbnailImageUrl, currentVersionPath + "\\thumbnail");
					if (!File.Exists(currentVersionPath + "\\image"))
						_ = Utils.DownloadFile(avatar.imageUrl, currentVersionPath + "\\image");
				}

				return true;
			}
			catch (Exception ex)
			{
				Debug.LogLine($"Failed to write avatar:\n{ex.Message}", ConsoleColor.Red);
				return false;
			}
		}

		public static bool TryGetVrcIdList(string input, out VrcId[] vrcIdList)
		{
			try
			{
				var mc = assetRegex.Matches(input);
				vrcIdList = new VrcId[mc.Count];
				for (var i = 0; i < mc.Count; ++i)
					vrcIdList[i] = new VrcId(mc[i].ToString());
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogLine($"Failed get id list:\n{ex.Message}", ConsoleColor.Red);
			}

			vrcIdList = null;
			return false;
		}

		public static bool TryGetVrcCacheEntries(bool clearCache, out List<VrcCacheEntry> entries)
		{
			entries = null;
			var retval = false;

			if (!File.Exists(vrcLibPath))
				return retval;

			try
			{
				var contents = File.ReadAllText(vrcLibPath, Encoding.UTF8);

				var mc = vrcLibraryRegex.Matches(contents);
				var list = new VrcCacheEntry[mc.Count];
				for (var i = 0; i < mc.Count; ++i)
					list[i] = new VrcCacheEntry()
					{
						id = new VrcId(VrcId.VrcIdType.File, new Guid(mc[i].Groups[1].Value)),
						version = uint.Parse(mc[i].Groups[2].Value)
					};

				entries = new List<VrcCacheEntry>(list);
				retval = true;

				if (clearCache && File.Exists(vrcLibPath))
					File.Delete(vrcLibPath);
			}
			catch (Exception ex)
			{
				Debug.LogLine($"Failed to get vrc cache:\n{ex.Message}", ConsoleColor.Red);
				return false;
			}

			return retval;
		}

		public struct VrcCacheEntry
		{
			public VrcId id { get; set; }
			public uint version { get; set; }
		}
	}
}
