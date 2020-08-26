using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace YamLib
{
	public static class FilesystemTools
	{
		private static readonly IList<char> invalidFileNameChars = Path.GetInvalidFileNameChars();

		public static string MakeFileSafe(string name)
		{
			try
			{
				// Builds a string out of valid chars and replaces invalid chars with a unique letter (Moves the Char into the letter range of unicode, starting at "A")
				return new string(name.Select(ch =>
						invalidFileNameChars.Contains(ch) ? Convert.ToChar(invalidFileNameChars.IndexOf(ch) + 65) : ch)
					.ToArray());
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static bool FolderExists(string foldername)
		{
			foldername = Path.Combine(Directory.GetCurrentDirectory(), foldername);
			return Directory.Exists(foldername);
		}

		public static void EnsureFolder(string foldername)
		{
			foldername = Path.Combine(Directory.GetCurrentDirectory(), foldername);
			if (!Directory.Exists(foldername))
				_ = Directory.CreateDirectory(foldername);
		}

		public static void WriteStreamToFile(string filename, Stream inputStream)
		{
			if (string.IsNullOrEmpty(filename))
				return;

			if (File.Exists(filename)) File.Delete(filename);

			using (var fileStream = File.Create(Path.Combine(Directory.GetCurrentDirectory(), filename)))
			{
				inputStream.CopyTo(fileStream);
			}
		}

		/// <summary>
		///     Gets folder containing the compiled executable
		/// </summary>
		/// <returns>
		///     The path to the executable
		/// </returns>
		public static string GetExeDirectoryPath()
		{
			return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
		}

		/// <summary>
		///     Gets the path to the AppData LocalLow directory
		/// </summary>
		/// <returns>
		///     The AppData LocalLow path
		/// </returns>
		public static string GetLocalLowDirectory()
		{
			var pszPath = IntPtr.Zero;
			try
			{
				var hr = SHGetKnownFolderPath(new Guid("A520A1A4-1780-4FF6-BD18-167343C5AF16"), 0, IntPtr.Zero,
					out pszPath);
				if (hr >= 0)
					return Marshal.PtrToStringAuto(pszPath);
				throw Marshal.GetExceptionForHR(hr);
			}
			finally
			{
				if (pszPath != IntPtr.Zero)
					Marshal.FreeCoTaskMem(pszPath);
			}
		}

		[DllImport("shell32.dll")]
		private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags,
			IntPtr hToken, out IntPtr pszPath);
	}
}