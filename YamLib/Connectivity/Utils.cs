using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace YamLib.Connectivity
{
	public static class Utils
	{
		public static bool DownloadFile(string url, string path)
		{
			if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(path))
				return false;

			var req = WebRequest.Create(url);

			using (var response = req.GetResponse())
			{
				var filename = Path.GetFileName(response.ResponseUri.AbsoluteUri);
				using (var stream = response.GetResponseStream())
				{
					var extention = Path.GetExtension(filename);

					if (File.Exists(path + extention))
						return false;

					FilesystemTools.WriteStreamToFile(path + extention, stream);
				}
			}

			return true;
		}

		public static bool TryGetIPAddress(string hostname, out IPAddress ip)
		{
			ip = IPAddress.None;
			IPAddress[] addresses;
			try
			{
				addresses = Dns.GetHostEntry(hostname).AddressList;
			}
			catch (ArgumentOutOfRangeException)
			{
				System.Console.WriteLine("hostUri is more than 255 characters long.");
				return false;
			}
			catch (SocketException ex)
			{
				System.Console.WriteLine("An error was encountered when resolving the hostUri: {0}", ex.Message);
				return false;
			}
			catch (ArgumentException)
			{
				System.Console.WriteLine("hostUri is an invalid IP address");
				return false;
			}

			if (addresses.Length == 0 || addresses[0] == null)
			{
				System.Console.WriteLine("Not found");
				return false;
			}

			ip = addresses[0];
			return true;
		}

		public static IPAddress UintIPToIPAddress(uint ip)
		{
			return new IPAddress(new[]
			{
				(byte) ((ip >> 24) & 255u),
				(byte) ((ip >> 16) & 255u),
				(byte) ((ip >> 8) & 255u),
				(byte) (ip & 255u)
			});
		}
	}
}