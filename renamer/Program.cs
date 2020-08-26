using Ripper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace renamer
{
	class Program
	{
		// Encode contents from old database with "Windows-1252" and decode with "UTF-8" then input into new db

		static void Main()
		{
			VrcDb db_new = new VrcDb();
			OldVrcDb db_main = new OldVrcDb();
			OldVrcDb db_backup = new OldVrcDb();
			db_new.Open($"URI=file:{Directory.GetCurrentDirectory()}\\new.db");
			db_main.Open($"URI=file:{Directory.GetCurrentDirectory()}\\data.db");
			db_backup.Open($"URI=file:{Directory.GetCurrentDirectory()}\\backup\\data.db");
			db_main.Init();
			db_backup.Init();
			Console.WriteLine(db_new.GetAllIds().Length);
			Console.WriteLine(db_main.GetAllIds().Length);
			Console.WriteLine(db_backup.GetAllIds().Length);
			db_new.Close();
			db_main.Close();
			db_backup.Close();
			Console.ReadLine();
		}
	}
}
