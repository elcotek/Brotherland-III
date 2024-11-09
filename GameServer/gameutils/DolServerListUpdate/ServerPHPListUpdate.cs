/* This script was created to automate registering with the daocportal.net
 * Server List. Please take some time to edit the settings below to fit
 * your needs.
 * 
 * Author: Dawn of Light
 * Date: 10th December 2007
 * Updated: 11th April 2011 (new portal url)
 * Credits: Thanks to Cisien for the initial release.
 */

using System;
using System.Net;
using System.Text;
using System.Threading;
using DOL.Events;
using DOL.GS.ServerProperties;
using log4net;
using System.Reflection;
using DOL.Database;
using System.Collections.Generic;

namespace DOL.GS.GameEvents
{
	public class ServerPHPListUpdate
	{
		//private const string UpdateURL = "http://portal.dolserver.net/serverlist.php?action=submit";

		#region Code

		/// <summary>
		/// The Script Version
		/// </summary>
		protected static double ScriptVersion = 2.1;

		/// <summary>
		/// creates the thread which is used to update the portal's entry.
		/// </summary>
		protected static Thread m_thread;

		/// <summary>
		/// creates the timer which will set the interval on which the portal list will be updated
		/// </summary>
		protected static Timer m_timer;

		/// <summary>
		/// Interval between server updates in miliseconds
		/// </summary>
		protected const int UPDATE_INTERVAL = 3 * 60 * 1000;

		/// <summary>
		/// Sets up our logger instance
		/// </summary>
		protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// This method is called when the script is loaded.
		/// </summary>
		[GameServerStartedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			if (Properties.SERVERPHPListUPDATE_ENABLED)
				Init();
		}

		/// <summary>
		/// This method is called when the scripts are unloaded. 
		/// </summary>
		[GameServerStoppedEvent]
		public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			if (Properties.SERVERPHPListUPDATE_ENABLED)
				Stop();
		}

		/// <summary>
		/// Initializes the DAoCPortal Update Manager
		/// </summary>
		/// <returns>returns true if successful or if the username is not supplied</returns>
		public static bool Init()
		{
			m_timer = new Timer(new TimerCallback(StartListThread), m_timer, 0, UPDATE_INTERVAL);
			if (m_timer == null)
			{
				if (log.IsErrorEnabled)
					log.Error("Update timer failed to start. Stopping!");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Starts the thread which updates the portal. This runs on a seperate thread due 
		/// to the time it can take to complete a WebClient operation.
		/// </summary>
		/// <param name="timer"></param>
		private static void StartListThread(object timer)
		{
			m_thread = new Thread(StartList);
			m_thread.Start();
		}

		/// <summary>
		/// This method defines and formats the various strings to be used
		/// </summary>
		private static void StartList()
		{

			int albsCount = 0;
			int midsCount = 0;
			int hibsCount = 0;

			// = 0, midgard, hibernia;
			//Get Playing Clients Count
	
			int allCount = WorldMgr.GetAllPlayingClientsCount();
			foreach (GameClient realmPlayer in WorldMgr.GetAllPlayingClients())
			{
				if (realmPlayer != null)
				{
					switch (realmPlayer.Player.Realm)
					{
						case eRealm.Albion:
							{
								albsCount++;
								break;
							}
						case eRealm.Midgard:
							{
								midsCount++;
								break;
							}
						case eRealm.Hibernia:
							{
								hibsCount++;
								break;
							}
					}
				}
			}

			WriteStatsToDB(albsCount, midsCount, hibsCount, allCount);

			string serverOnline = ("Server status online.");
			//string serverVersion = ("Server Version: " + ScriptVersion.ToString() + " ");
			string serverPlayersOnline = ("Players online: " + allCount.ToString() + " ");
			string serverAlbionPlayersOnline = ("Albion currently online: " + albsCount + " ");
			string serverMidgardPlayersOnline = ("Midgard currently online: " + midsCount + " ");
			string serverHiberniaPlayersOnline = ("Hibernia currently online: " + hibsCount + " ");


			StatusServer.GetServerInfo(serverOnline + "\r\n" + serverPlayersOnline + "\r\n" +
				serverAlbionPlayersOnline + "\r\n" + serverMidgardPlayersOnline + "\r\n" + serverHiberniaPlayersOnline);
			
			if (log.IsInfoEnabled)
				log.Info("Your server's Stat Database entry was successfully updated!");

			
		}
		public static void WriteStatsToDB(int alb, int mid, int hib, int all)
		{
			
			IList<DBStatsCounter> dbstats = GameServer.Database.SelectAllObjects<DBStatsCounter>();

			foreach (DBStatsCounter statCounts in dbstats)
            {
				if (statCounts.Source == "Total:" && statCounts.Count != all)
                {
					
					statCounts.Count = all;
					GameServer.Database.SaveObject(statCounts);
				}
				if (statCounts.Source == "Albion:" && statCounts.Count != alb)
				{
					statCounts.Count = alb;
					GameServer.Database.SaveObject(statCounts);
				}
				if (statCounts.Source == "Midgard:" && statCounts.Count != mid)
				{
					statCounts.Count = mid;
					GameServer.Database.SaveObject(statCounts);
				}
				if (statCounts.Source == "Hibernia:" && statCounts.Count != hib)
				{
					statCounts.Count = hib;
					GameServer.Database.SaveObject(statCounts);
				}
				
				
			}
			
		}

		/// <summary>
		/// Stops the update process.
		/// </summary>
		/// <returns>true always</returns>
		public static bool Stop()
		{
			try
			{
				if (m_thread != null)
					m_thread = null;

				if (m_timer != null)
					m_timer = null;
				return true;
			}
			catch (Exception ex)
			{
				if (log.IsErrorEnabled)
					log.Error("An error occured: \r\n" + ex.ToString());
				return false;
			}

		}

		#endregion
	}
}
