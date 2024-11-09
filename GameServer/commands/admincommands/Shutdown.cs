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
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.ServerProperties;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DOL.GS.Commands
{
    [CmdAttribute(
        "&shutdown",
        ePrivLevel.Admin,
        "Shutdown the server in next minute",
        "/shutdown on <hour>:<min>  - shutdown on this time",
        "/shutdown <mins>  - shutdown in minutes",
        "/shutdown wrestart - Enable/Disable the auto Restart of the Server")]
    public class ShutdownCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int AUTOMATEDSHUTDOWN_CHECKINTERVALMINUTES = 15;
        private const int AUTOMATEDSHUTDOWN_HOURTOSHUTDOWN = 4; // local time
        private const int AUTOMATEDSHUTDOWN_SHUTDOWNWARNINGMINUTES = 45;

        private static long m_counter = 0;
        private static Timer m_timer;
        private static readonly int m_time = 5;
        private static bool m_shuttingDown = false;
        private static bool m_firstAutoCheck = true;
        private static long m_currentCallbackTime = 0;

        public static long getShutdownCounter()
        {
            return m_counter;
        }

        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            m_currentCallbackTime = AUTOMATEDSHUTDOWN_CHECKINTERVALMINUTES * 60 * 1000;
            m_timer = new Timer(new TimerCallback(AutomaticShutdown), null, 0, m_currentCallbackTime);
        }

        public static void AutomaticShutdown(object param)
        {
            if (m_firstAutoCheck)
            {
                // skip the first check.  This is for debugging, to make sure the timer continues to run after setting it to a small interval for testing
                m_firstAutoCheck = false;
                return;
            }

            // At least 1 hour
            if (Properties.HOURS_UPTIME_BETWEEN_SHUTDOWN <= 0) return;

            if (m_shuttingDown)
                return;

            TimeSpan uptime = TimeSpan.FromMilliseconds(GameServer.Instance.TickCount);

            if (uptime.TotalHours >= Properties.HOURS_UPTIME_BETWEEN_SHUTDOWN && DateTime.Now.Hour == AUTOMATEDSHUTDOWN_HOURTOSHUTDOWN)
            {
                m_counter = AUTOMATEDSHUTDOWN_SHUTDOWNWARNINGMINUTES * 60;

                //Set the timer for a 5 min callback
                m_currentCallbackTime = 5 * 60 * 1000;
                m_timer.Dispose();
                m_timer = new Timer(new TimerCallback(CountDown), null, m_currentCallbackTime, 1);

                DateTime date;
                date = DateTime.Now;
                date = date.AddSeconds(m_counter);

                foreach (GameClient m_client in WorldMgr.GetAllPlayingClients())
                {
                    m_client.Out.SendDialogBox(eDialogCode.SimpleWarning, 0, 0, 0, 0, eDialogType.Ok, true, "Automated server restart / backup triggered. Restart in " + m_counter / 60 + " mins! (Restart at " + date.ToString("HH:mm \"GMT\" zzz") + ")");
                    //m_client.Out.SendMessage("Automated server restart / backup triggered. Restart in " + m_counter / 60 + " mins! (Restart on " + date.ToString("HH:mm \"GMT\" zzz") + ")", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                }

                string msg = "Automated server restart in " + m_counter / 60 + " mins! (Restart at " + date.ToString("HH:mm \"GMT\" zzz") + ")";
                log.Warn(msg);
            }
            else
            {
                log.Info("Uptime = " + uptime.TotalHours.ToString("N1") + ", restart uptime = " + Properties.HOURS_UPTIME_BETWEEN_SHUTDOWN.ToString() +
                         " | Current hour = " + DateTime.Now.Hour.ToString() + ", restart hour = " + AUTOMATEDSHUTDOWN_HOURTOSHUTDOWN.ToString());
            }
        }

        public static void CountDown(object param)
        {
            //Subtract the current callback time
            m_counter -= m_currentCallbackTime / 1000;

            //Make sure we set this correctly
            m_shuttingDown = true;
            if (m_counter <= 0)
            {
                // You have an IRC Bot
                //if (ServerIRC.IRCBot != null) ServerIRC.IRCBot.SendMessage(ServerIRC.CHANNEL, "Server is restarting ...");

                m_timer.Dispose();
                new Thread(new ThreadStart(ShutDownServer)).Start();
                return;
            }
            else
            {
                if (m_counter > 120)
                    log.Warn("Server shutdown in " + (int)(m_counter / 60) + " minutes!");
                else
                    log.Warn("Server shutdown in " + m_counter + " seconds!");

                long secs = m_counter;
                long mins = secs / 60;
                long hours = mins / 60;

                string sendMessage = "";

                if (hours > 3) //hours...
                {
                    if (mins % 60 < 15) //every hour..
                        sendMessage = "Server shutdown in " + hours + " hours!";
                    //15 minutes between checks
                    m_currentCallbackTime = 15 * 60 * 1000;
                }
                else if (hours > 0) //hours...
                {
                    if (mins % 30 < 5) //every 30 mins..
                        sendMessage = "Server shutdown in " + hours + " hours and " + (mins - (hours * 60)) + " minutes!";
                    //5 minutes between checks
                    m_currentCallbackTime = 5 * 60 * 1000;
                }
                else if (mins >= 10)
                {
                    if (mins % 15 < 1) //every 15 mins..
                    {
                        sendMessage = "Server shutdown in " + mins + " minutes!";

                        // You have an IRC Bot
                        //if (ServerIRC.IRCBot != null) ServerIRC.IRCBot.SendMessage(ServerIRC.CHANNEL, sendMessage);
                    }

                    //1 minute between checks
                    m_currentCallbackTime = 60 * 1000;
                }
                else if (mins >= 5)
                {
                    if (secs % 60 < 15) //every min...
                    {
                        sendMessage = "Server shutdown in " + mins + " minutes!";

                        // You have an IRC Bot
                        //if (ServerIRC.IRCBot != null && mins % 2 == 0) ServerIRC.IRCBot.SendMessage(ServerIRC.CHANNEL, sendMessage);
                    }

                    //15 secs between checks
                    m_currentCallbackTime = 15 * 1000;
                }
                else if (mins >= 4 && mins < 5)
                {
                    sendMessage = "Server shutdown in " + mins + " minutes! (" + secs + " seconds)";
                    sendMessage = "please logout before you get auto disconnect, to be sure your charcter get saved!";
                    sendMessage = "The server will all clients auto disconnect 2 min before shutdown!";
                    

                    // You have an IRC Bot
                    //if (ServerIRC.IRCBot != null && secs % 60 == 0) ServerIRC.IRCBot.SendMessage(ServerIRC.CHANNEL, sendMessage);

                    //15 secs between checks
                    m_currentCallbackTime = 15 * 1000;
                }
                else if (mins >= 3 && mins < 4)
                {
                    sendMessage = "PLEASE LOG OFF | STOP ANY PULL AND LOG OFF OR YOU GET NOT SAVED!";

                    //15 secs between checks
                    m_currentCallbackTime = 15 * 1000;
                }
                else if (secs > 120 && mins < 3)
                {
                    sendMessage = "The server will disconnect all clients before Shutdown!";


                    // You have an IRC Bot
                    //if (ServerIRC.IRCBot != null && secs % 60 == 0) ServerIRC.IRCBot.SendMessage(ServerIRC.CHANNEL, sendMessage);

                    //15 secs between checks
                    m_currentCallbackTime = 15 * 1000;
                }
                else
                {

                    sendMessage = "Server shutdown in " + secs + " seconds! Please logout!";
                    DisconnectClients();
                    //5 secs between checks
                    m_currentCallbackTime = 5 * 1000;
                }

                //log.Debug(string.Format("counter: {0} callback: {1}", m_counter, m_currentCallbackTime));
                //log.Debug(sendMessage);

                //Change the timer to the new callback time
                m_timer.Change(m_currentCallbackTime, m_currentCallbackTime);

                if (sendMessage != "")
                {
                    foreach (GameClient client in WorldMgr.GetAllPlayingClients())
                    {
                        client.Out.SendMessage(sendMessage, eChatType.CT_Staff, eChatLoc.CL_ChatWindow);
                    }
                }

                if (mins <= 3 && GameServer.Instance.ServerStatus != eGameServerStatus.GSS_Closed) // 2 mins remaining
                {
                    GameServer.Instance.Close();
                    string msg = "Server is now closed (shutdown in " + mins + " mins)";

                    // You have an IRC Bot
                    //if (ServerIRC.IRCBot != null) ServerIRC.IRCBot.SendMessage(ServerIRC.CHANNEL, msg);
                }
            }
        }
        static bool wResatart = false;

        public static void ShutDownServer()
        {
            if (GameServer.Instance.IsRunning)
            {
                //send the shutdown status to the protal
                StatusServer.GetServerInfo("offline");

                //shutdown the server
                GameServer.Instance.Stop();
                log.Info("Automated server shutdown!");
                Thread.Sleep(5000);
                ClearStatsDB();

                if (ShutdownAutorestart())
                {
                    log.Info("Automated server restart is enabeld!");
                    Thread.Sleep(20000);
                    System.Diagnostics.Process.Start("dolserver.exe");
                    Environment.Exit(0);
                    wResatart = false;
                    
                }
                else
                    Environment.Exit(0);
            }
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

        public static bool ShutdownAutorestart()
        {
            if (wResatart == true)
                return true;

            return false;
        }
        public void OnCommand(GameClient client, string[] args)
        {
            DateTime date;
            //if (m_counter > 0) return 0;
            if (args.Length >= 2)
            {
                if (args[1] == "wrestart")
                {

                    if (wResatart == false)
                    {
                        wResatart = true;
                       // Console.WriteLine("Shutdown with auto restart is now activated");
                        client.Out.SendMessage("Shutdown with auto restart is now activated", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                    else if (wResatart == true)
                    {
                        wResatart = false;
                        //Console.WriteLine("Shutdown with auto restart is now disabled");
                        client.Out.SendMessage("Shutdown with auto restart is now disabled", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                    return;
                }
                if (args.Length == 2)
                {
                    try
                    {
                        if (System.Convert.ToInt32(args[1]) >= 5)
                        {
                            m_counter = System.Convert.ToInt32(args[1]) * 60;
                        }
                    }
                    catch (Exception)
                    {
                        DisplaySyntax(client);
                        return;
                    }
                }
                else
                {
                    if ((args.Length == 3) && (args[1] == "on"))
                    {
                        string[] shutdownsplit = args[2].Split(':');

                        if ((shutdownsplit == null) || (shutdownsplit.Length < 2))
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        int hour = Convert.ToInt32(shutdownsplit[0]);
                        int min = Convert.ToInt32(shutdownsplit[1]);
                        // found next date with hour:min

                        date = DateTime.Now;

                        if ((date.Hour > hour) ||
                            (date.Hour == hour && date.Minute > min)
                           )
                            date = new DateTime(date.Year, date.Month, date.Day + 1);

                        if (date.Minute > min)
                            date = new DateTime(date.Year, date.Month, date.Day, date.Hour + 1, 0, 0);

                        date = date.AddHours(hour - date.Hour);
                        date = date.AddMinutes(min - date.Minute + 2);
                        date = date.AddSeconds(-date.Second);

                        m_counter = (date.ToFileTime() - DateTime.Now.ToFileTime()) / TimeSpan.TicksPerSecond;

                        if (m_counter < 60) m_counter = 60;

                    }
                    else
                    {
                        DisplaySyntax(client);
                        return;
                    }

                }
            }
            else
            {
                DisplaySyntax(client);
                return;
            }

            if (m_counter % 5 != 0)
                m_counter = (m_counter / 5 * 5);

            if (m_counter == 0)
                m_counter = m_time * 60;

            date = DateTime.Now;
            date = date.AddSeconds(m_counter);

            string msg = "Server shutdown in " + m_counter / 60 + " mins!";
            bool popup = ((m_counter / 60) < 60);


            foreach (GameClient m_client in WorldMgr.GetAllPlayingClients())
            {
                if (popup)
                {
                    m_client.Out.SendDialogBox(eDialogCode.SimpleWarning, 0, 0, 0, 0, eDialogType.Ok, true, "Attention: Server shutdown in " + m_counter / 60 + " mins! (shutdown at " + date.ToString("HH:mm \"GMT\" zzz") + ")");
                    m_client.Out.SendMessage("Server shutdown in " + m_counter / 60 + " mins! (shutdown on " + date.ToString("HH:mm \"GMT\" zzz") + ")", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                }

                m_client.Out.SendMessage(msg, eChatType.CT_Staff, eChatLoc.CL_ChatWindow);
            }

            log.Warn(msg);

            // You have an IRC Bot
            //if (ServerIRC.IRCBot != null) ServerIRC.IRCBot.SendMessage(ServerIRC.CHANNEL, msg);

            m_currentCallbackTime = 0;
            if (m_timer != null)
                m_timer.Dispose();
            m_timer = new Timer(new TimerCallback(CountDown), null, 0, 15000);
        }
        public static void DisconnectClients()
        {
            foreach (GameClient clients in WorldMgr.GetAllPlayingClients())
            {
                if (clients.Account.PrivLevel == 1 && clients != null)
                {
                    clients.Player.Quit(true);
                    clients.Disconnect();
                    return;
                }
            }
        }
    }
}
