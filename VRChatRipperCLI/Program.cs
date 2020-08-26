using YamLib;
using Ripper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using VRC;
using VRC.API;

namespace AssetDB
{
	internal class Program
	{
		#region Variables
		private static readonly int buttonPressesBeforeExit = 5;
		private static readonly int apiDelayTimeInMs = 200;
		private static readonly int libCheckDelayTimeInMs = 10;
		private static readonly int libParseDelayTimeInMs = 2000;

		private static string inputFile = "";
		private static readonly string assetsPath = $"{FilesystemTools.GetExeDirectoryPath()}\\Assets";

		private static List<VrcId> inactiveIds = new List<VrcId>();
		private static List<VrcId> downloadedAssets = new List<VrcId>();

		private static OldVrcDb database;
		private static ApiClient client;
		private static SessionProtector protector = new SessionProtector();
		#endregion

		private static void DisableId(VrcId id, Exception ex)
		{
			if (!protector.TryStartSession())
				return;

			try
			{
				// Inform user
				Debug.LogLine($"[ERROR     ] Failed to get {id}:\tIt has been removed from vrchat servers", ConsoleColor.Red, false, true);

				database.MarkAsDeleted(id);
			}
			catch (Exception){}
			finally
			{
				protector.EndSession();
			}
		}

		private static void RipAvatar(ApiAvatar avatar)
		{
			if (avatar == null || avatar.id == null || avatar.name == null || avatar.authorId == null ||
			avatar.authorName == null)
			{
				Debug.LogLine("[ERROR     ] Avatar is NULL!", ConsoleColor.Red, false, true);

				return;
			}

			if (!protector.TryStartSession())
				return;

			try
			{
				string path = $"{assetsPath}\\Avatars\\{avatar.id.Guid.ToString("N")}";

				bool refreshMetaData = cmd.type == CommandType.Refresh;

				// Save avatar to database
				if (database.AddAvatar(avatar, refreshMetaData))
				{
					// Write assetfile, thumbnail, and description
					if (RipperTools.WriteAvatar(avatar, path, !refreshMetaData, refreshMetaData))
						// Inform user
						Debug.LogLine($"[INFO      ] Downloaded avatar {avatar.name}, created by {avatar.authorName}", ConsoleColor.Green, true);
				}
				else
				{
					Debug.LogLine($"[INFO      ] Skipped avatar {avatar.name}, created by {avatar.authorName}: Already in database / invalid meta-data!", ConsoleColor.Green, true);
				}
			}
			catch (Exception ex)
			{
				Debug.LogLine($"[EXCEPTION ] Failed to rip avatar {avatar.name}, created by {avatar.authorName}:\t{ex.Message}", ConsoleColor.Red, true, true);
			}
			finally
			{
				protector.EndSession();
			}
		}

		private static void RipWorld(ApiWorld world)
		{
			if (world == null || world.id == null || world.name == null || world.authorId == null ||
			world.authorName == null)
			{
				Debug.LogLine("[ERROR     ] Avatar is NULL!", ConsoleColor.Red, false, true);
				return;
			}

			if (!protector.TryStartSession())
				return;

			try
			{
				string path = $"{assetsPath}\\Worlds\\{world.id.Guid.ToString("N")}";

				bool refreshMetaData = cmd.type == CommandType.Refresh;

				// Save world to database
				if (database.AddWorld(world, refreshMetaData))
				{
					// Write assetfile, thumbnail, and description
					if (RipperTools.WriteWorld(world, path, !refreshMetaData, refreshMetaData))
						// Inform user
						Debug.LogLine($"[INFO      ] Downloaded world {world.name}, created by {world.authorName}", ConsoleColor.Green, true);
				}
				else
				{
					Debug.LogLine($"[INFO      ] Skipped world {world.name}, created by {world.authorName}: Already in database / invalid meta-data!", ConsoleColor.Green, true);
				}
			}
			catch (Exception ex)
			{
				Debug.LogLine($"[EXCEPTION ] Failed to rip world {world.name}, created by {world.authorName}:\t{ex.Message}", ConsoleColor.Red, true, true);
			}
			finally
			{
				protector.EndSession();
			}
		}

		private static object cleanupLock = new object();
		private static bool cleanupDone = false;

		private static bool CleanupDone()
		{
			lock (cleanupLock)
				return cleanupDone;
		}
		private static void Cleanup(bool force = false)
		{
			lock (cleanupLock)
			{
				if (cleanupDone == true)
					return;
				if (force)
				{
					protector.Exit();
				}
				else
				{
					protector.ExitAfterProcs();
				}
				Debug.LogText("[INFO      ] Waiting for requests to finish...\t\t");
				client.AbortRequests(true);
				Debug.LogLine("Done.", ConsoleColor.Green);
				Debug.LogText("[INFO      ] Closing database...\t\t\t");
				database.Close();
				Debug.LogLine("Done.", ConsoleColor.Green);
				cleanupDone = true;
			}
		}

		private static ParsedArgs cmd;

		private static void Main(string[] args)
		{
			// Register exit handler
			Console.CancelKeyPress += new ConsoleCancelEventHandler(KeyHandler);

			cmd = ParseArgs(args);
#if !DEBUG
			if (cmd.type == CommandType.None)
			{
				PrintHelpMessage();
				return;
			}
#else
			inputFile = "avatars.txt";
			cmd.type = CommandType.Download;
			cmd.ignoreDeleted = true;
#endif

			Debug.LogText("[INFO      ] Opening database...\t\t\t");
			database = new OldVrcDb();
			if (!database.Open($"URI=file:{Directory.GetCurrentDirectory()}\\data.db"))
			{
				Debug.LogLine("ERROR", ConsoleColor.Red);
				return;
			}
			Debug.LogLine("Done.", ConsoleColor.Green);

			Debug.LogText("[INFO      ] Initializing database...\t\t\t");
			if (!database.Init())
			{
				Debug.LogLine("ERROR", ConsoleColor.Red);
				database.Close();
				return;
			}
			Debug.LogLine("Done.", ConsoleColor.Green);

			client = new ApiClient();

			var idsToDownload = new List<VrcId>();

			// Read database
			{
				Debug.LogText("[INFO      ] Reading ids from database...\t\t");
				var ids = cmd.ignoreDeleted ? database.GetAllIds(false) : database.GetAllIds();
				Debug.LogLine($"Got {ids.Length} ids", ConsoleColor.Green);
				downloadedAssets.AddRange(ids);
			}

			if (cmd.type == CommandType.ExtractIds)
			{
				File.WriteAllLines("extractedids.txt", downloadedAssets.Select(i => i.ToString()));
				foreach (var id in downloadedAssets)
					Console.WriteLine(id);

				return;
			}

			{
				Debug.LogText("[INFO      ] Reading deleted ids from database...\t");
				var ids = database.GetAllIds(true);
				Debug.LogLine($"Got {ids.Length} ids", ConsoleColor.Green);
				inactiveIds.AddRange(ids);
			}

			if (cmd.type == CommandType.Download)
			{
				// Read input avatar list
				Debug.LogText($"[INFO      ] Reading file...\t\t\t\t");
				string text = File.ReadAllText(inputFile);
				Debug.LogLine($"Read {ToolBox.FormatLog((ulong)text.Length, 'B')}", ConsoleColor.Green);


				Debug.LogText("[INFO      ] Parsing file...\t\t\t\t");
				if (!RipperTools.TryGetVrcIdList(text, out var ids))
				{
					Debug.LogLine("ERROR", ConsoleColor.Red);
					PrintHelpMessage();
					Cleanup();
					return;
				}
				Debug.LogLine($"Got {ids.Length} ids", ConsoleColor.Green);

				Debug.LogText("[INFO      ] Queueing ids...\t\t\t\t");
				idsToDownload.AddRange(ids);
				Debug.LogLine("Done.", ConsoleColor.Green);

				Debug.LogText("[INFO      ] Removing known ids...\t\t\t");
				idsToDownload = idsToDownload.Except(downloadedAssets).Except(inactiveIds).ToList();
				Debug.LogLine("Done.", ConsoleColor.Green);
			}
			else if (cmd.type == CommandType.Update)
			{
				idsToDownload.AddRange(downloadedAssets);
			}
			else if (cmd.type == CommandType.Refresh)
			{
				idsToDownload.AddRange(downloadedAssets);
				downloadedAssets.Clear();
			}


			Debug.LogText("[INFO      ] Removing duplicates...\t\t\t");
			inactiveIds = inactiveIds.Distinct().ToList();
			idsToDownload = idsToDownload.Distinct().ToList();
			downloadedAssets = downloadedAssets.Distinct().ToList();
			Debug.LogLine("Done.", ConsoleColor.Green);

			// TODO IMPLEMENTME: currently just lists found vrchat ids
			if (cmd.type == CommandType.Attach)
			{
				while (!CleanupDone())
				{
					if (RipperTools.TryGetVrcCacheEntries(true, out var entries))
					{
						entries.RemoveAll(m => downloadedAssets.Contains(m.id));

						foreach (var entry in entries)
						{
							Debug.LogLine(entry.id.ToString());
							downloadedAssets.Add(entry.id);
						}

						Thread.Sleep(libParseDelayTimeInMs);
					}
					else
					{
						Thread.Sleep(libCheckDelayTimeInMs);
					}
				}
			}
			else
			{
				Debug.LogText("[INFO      ] Starting requets...\t\t\t");
				Debug.LogLine($"{idsToDownload.Count} ids queued", ConsoleColor.Green);
				foreach (var id in idsToDownload)
				{
					if (CleanupDone())
						break;

					try
					{
						switch (id.IdType)
						{
							case VrcId.VrcIdType.Avatar:
								client.GetAvatarById(id, RipAvatar, DisableId);
								Thread.Sleep(apiDelayTimeInMs);
								break;
							case VrcId.VrcIdType.World:
								client.GetWorldById(id, RipWorld, DisableId);
								Thread.Sleep(apiDelayTimeInMs);
								break;
							case VrcId.VrcIdType.User:
							case VrcId.VrcIdType.LegacyUser:
								//database.AddUser();
								//Thread.Sleep(apiDelayTimeInMs);
								break;
							default:
								Debug.LogLine($"[UNPARSABLE] Skipping {id}", ConsoleColor.Yellow);
								continue;
						}
					}
					catch (Exception ex)
					{
						Debug.LogLine($"[EXCEPTION ] Failed to get {id}:\n\t{ex.Message}", ConsoleColor.Red, false, true);
						break;
					}
				}
			}
			// Exit after all processes are done
			protector.ExitAfterProcs();
			Cleanup();
		}

		#region ClosingHandler
		private static int timesPressed = 0;
		private static void KeyHandler(object sender, ConsoleCancelEventArgs args)
		{
			Debug.LogLine($"[SIGNAL    ] Caught exit signal, stopping application... [{timesPressed}/{buttonPressesBeforeExit}]", ConsoleColor.Red);

			args.Cancel = true;
			bool force = (++timesPressed < buttonPressesBeforeExit);
			_ = Task.Run(() => Cleanup(force));
		}
		#endregion
		#region ArgumentHandling

		private enum CommandType
		{
			Attach,
			Update,
			Refresh,
			Download,
			ExtractIds,
			None
		}

		private struct ParsedArgs
		{
			public CommandType type;
			public bool ignoreDeleted;
		}

		private static ParsedArgs ParseArgs(string[] args)
		{
			ParsedArgs output;
			output.type = CommandType.None;
			output.ignoreDeleted = false;

			foreach (string arg in args)
			{
				switch (arg)
				{
					case "--attach":
						if (output.type != CommandType.None)
						{
							PrintHelpMessage("Too many arguments!");
							output.type = CommandType.None;
							output.ignoreDeleted = false;
							break;
						}

						output.type = CommandType.Attach;
						continue;
					case "--update":
						if (output.type != CommandType.None)
						{
							PrintHelpMessage("Too many arguments!");
							output.type = CommandType.None;
							output.ignoreDeleted = false;
							break;
						}

						output.type = CommandType.Update;
						continue;
					case "--refresh":
						if (output.type != CommandType.None)
						{
							PrintHelpMessage("Too many arguments!");
							output.type = CommandType.None;
							output.ignoreDeleted = false;
							break;
						}

						output.type = CommandType.Refresh;
						continue;
					case "--extract-ids":
						if (output.type != CommandType.None)
						{
							PrintHelpMessage("Too many arguments!");
							output.type = CommandType.None;
							output.ignoreDeleted = false;
							break;
						}

						output.type = CommandType.ExtractIds;
						continue;
					case "--ignore-deleted":
						output.ignoreDeleted = true;
						continue;
					default:
						if (output.type != CommandType.None)
						{
							PrintHelpMessage("Too many arguments!");
							output.type = CommandType.None;
							output.ignoreDeleted = false;
							break;
						}

						if (!File.Exists(arg))
						{
							PrintHelpMessage("Input file doesnt exist!");
							output.type = CommandType.None;
							output.ignoreDeleted = false;
							break;
						}

						output.type = CommandType.Download;
						inputFile = arg;
						continue;
				}

				break;
			}

			return output;
		}

		private static void PrintHelpMessage(string reason = "")
		{
			Debug.LogLine($"{reason}" +
					   "Usage:\n" +
					   "inputFile should be a plaintext file with all the assetId's you want to rip inside\n" +
					   "\n" +
					   "Example:\n" +
					   "\n" +
					   "-----------------\n" +
					   "  inputfile.txt  \n" +
					   "-----------------\n" +
					   "avtr_abcdef123456\n" +
					   "wrld_abcdef123456\n" +
					   "-----------------\n" +
					   "\n" +
					   "Download all assets in input file:\n" +
					   "\tAssetInfoRipper.exe [inputFile] [--ignore-deleted]\n" +
					   "Attach to vrchat and download avatars:\n" +
					   "\tAssetInfoRipper.exe --attach [--ignore-deleted]\n" +
					   "Update all cached assets:\n" +
					   "\tAssetInfoRipper.exe --update [--ignore-deleted]\n" +
					   "Download all cached assets again:\n" +
					   "\tAssetInfoRipper.exe --refresh");
		}
		#endregion
	}
}
