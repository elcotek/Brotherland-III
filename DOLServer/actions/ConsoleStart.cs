/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using DOL.Database;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOLGameServerConsole;
using log4net;

#pragma warning disable ET002 // Namespace does not match file path or default namespace
namespace DOL.DOLServer.Actions
#pragma warning restore ET002 // Namespace does not match file path or default namespace
{
	/// <summary>
	/// Handles console start requests of the gameserver
	/// </summary>
	public class ConsoleStart : IAction
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetStdHandle(int nStdHandle);
		[DllImport("kernel32.dll")]
		private static extern int GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern int SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

		/// <summary>
		/// returns the name of this action
		/// </summary>
		public string Name
		{
			get { return "--start"; }
		}

		/// <summary>
		/// returns the syntax of this action
		/// </summary>
		public string Syntax
		{
			get { return "--start [-config=./config/serverconfig.xml]"; }
		}

		/// <summary>
		/// returns the description of this action
		/// </summary>
		public string Description
		{
			get { return "Starts the DOL server in console mode"; }
		}

		private bool crashOnFail = false;


		private static bool StartServer()
		{
			Console.WriteLine("Starting the server");
			bool start = GameServer.Instance.Start();
			return start;
		}
		public static void ClearStatsDB()
		{

			IList<DBStatsCounter> dbstats = GameServer.Database.SelectAllObjects<DBStatsCounter>();

			foreach (DBStatsCounter statCounts in dbstats)
			{
				if (statCounts.Source == "Total:" && statCounts.Count != 0)
				{

					statCounts.Count = 0;
					GameServer.Database.SaveObject(statCounts);
				}
				if (statCounts.Source == "Albion:" && statCounts.Count != 0)
				{
					statCounts.Count = 0;
					GameServer.Database.SaveObject(statCounts);
				}
				if (statCounts.Source == "Midgard:" && statCounts.Count != 0)
				{
					statCounts.Count = 0;
					GameServer.Database.SaveObject(statCounts);
				}
				if (statCounts.Source == "Hibernia:" && statCounts.Count != 0)
				{
					statCounts.Count = 0;
					GameServer.Database.SaveObject(statCounts);
				}


			}

		}
		public void OnAction(Hashtable parameters)
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 10)
			{
				// enable VT100 emulation
				var handle = GetStdHandle(-11); // STD_OUTPUT_HANDLE
				if (handle != null)
				{
					uint mode;
					if (GetConsoleMode(handle, out mode) != 0)
						SetConsoleMode(handle, mode | 0x0004); // ENABLE_VIRTUAL_TERMINAL_PROCESSING
				}
			}

			Console.WriteLine("Starting GameServer ... please wait a moment!");
			FileInfo configFile;
			FileInfo currentAssembly = null;
			if (parameters["-config"] != null)
			{
				Console.WriteLine("Using config file: " + parameters["-config"]);
				configFile = new FileInfo((String)parameters["-config"]);
			}
			else
			{
				currentAssembly = new FileInfo(Assembly.GetEntryAssembly().Location);
				configFile = new FileInfo(currentAssembly.DirectoryName + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "serverconfig.xml");
			}
			if (parameters.ContainsKey("-crashonfail")) crashOnFail = true;

			var config = new GameServerConfiguration();
			if (configFile.Exists)
			{
				config.LoadFromXMLFile(configFile);
			}
			else
			{
				if (!configFile.Directory.Exists)
					configFile.Directory.Create();
				config.SaveToXMLFile(configFile);
				if (File.Exists(currentAssembly.DirectoryName + Path.DirectorySeparatorChar + "DOLConfig.exe"))
				{
					Console.WriteLine("No config file found, launching with default config and embedded database... (SQLite)");
				}
			}

			GameServer.CreateInstance(config);
			StartServer();

			if (crashOnFail && GameServer.Instance.ServerStatus == eGameServerStatus.GSS_Closed)
			{
				throw new ApplicationException("Server did not start properly.");
			}

			bool run = true;
			while (run)
			{
				Console.Write("> ");
				string line = Console.ReadLine();

				switch (line.ToLower())
				{
					case "exit":
						{
							run = false;
							ClearStatsDB();
						}
						break;
					case "stacktrace":
						log.Debug(PacketProcessor.GetConnectionThreadpoolStacks());
						break;
					case "clear":
						Console.Clear();
						break;
					default:
						if (line.Length <= 0)
							break;
						if (line[0] == '/')
						{
							line = line.Remove(0, 1);
							line = line.Insert(0, "&");
						}
						GameClient client = new GameClient(null);
						client.Out = new ConsolePacketLib();
						try
						{
							bool res = ScriptMgr.HandleCommandNoPlvl(client, line);
							if (!res)
							{
								Console.WriteLine("Unknown command: " + line);
							}
						}
						catch (Exception e)
						{
							Console.WriteLine(e.ToString());
						}
						break;
				}
			}
			if (GameServer.Instance != null)
				GameServer.Instance.Stop();
		}
	}
}
