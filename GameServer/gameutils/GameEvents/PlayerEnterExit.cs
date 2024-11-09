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
using DOL.GS.Housing;
using DOL.GS.PacketHandler;
using DOL.GS.ServerProperties;
using DOL.Language;
using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DOL.GS.GameEvents
{
    /// <summary>
    /// Spams everyone online with player enter/exit messages
    /// </summary>
    public class PlayerEnterExit
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public const string DayItem1 = "dragon_scale";
        public const string DayItem2 = "atlantean_glas";
        public const string DayItem3 = "aurulite";
        public const string DayItem4 = "diamondseal";

        public const string DayItem5 = "Sapphireseal"; //level  44 
        public const string DayItem6 = "emeraldseal";  //level  34 - 43

        static string msg = "";

        /// <summary>
        /// Event handler fired when server is started
        /// </summary>
        [GameServerStartedEvent]
        public static void OnServerStart(DOLEvent e, object sender, EventArgs arguments)
        {
            GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEntered));
            GameEventMgr.AddHandler(GamePlayerEvent.Quit, new DOLEventHandler(PlayerQuit));
        }

        /// <summary>
        /// Event handler fired when server is stopped
        /// </summary>
        [GameServerStoppedEvent]
        public static void OnServerStop(DOLEvent e, object sender, EventArgs arguments)
        {
            GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEntered));
            GameEventMgr.RemoveHandler(GamePlayerEvent.Quit, new DOLEventHandler(PlayerQuit));


        }


        /// <summary>
        /// Dayly Events
        /// </summary>
        /// <param name="player"></param>
        public static void DailyEvents(GamePlayer player)
        {

           
            #region Daily Events / XP Loot and Barrels allowed


            ///DOLCharacters last login
            IList<DOLCharacters> CharacterPlayers = GameServer.Database.SelectObjects<DOLCharacters>("`Name` = @Name", new QueryParameter("@Name", player.Name));

            foreach (DOLCharacters dba in CharacterPlayers)
            {
                {
                    if (dba != null && String.IsNullOrEmpty(dba.LastDailyRewardTime.ToString()) == false)
                    {
                        
                        TimeSpan diff;
                        diff = DateTime.Now - player.LastDailyRewardTime;


                       //log.ErrorFormat("diff = {0}", diff.Days.ToString());

                        // Does this character need daylys?
                        if (diff.Days >= Properties.Daily_Events_timer)
                        {
                           
                            {
                                player.LastDailyRewardTime = DateTime.Now;
                                for (int i = 0; i < dba.LastDailyRewardTime.Day; i++)
                                {
                                    if (dba.LastDailyRewardTime != DateTime.Now)
                                    {
                                        dba.LastDailyRewardTime = DateTime.Now;
                                    }
                                }

                                if (dba.Name == player.Name)
                                {
                                    //XPStones Reset for the day
                                    if (player.Level >= 1 && player.Level < 50)
                                    {
                                        //XP Loot
                                        if (player.XPStoneCount > 0)
                                        {
                                            dba.XPStoneCount = 0;
                                            player.XPStoneCount = 0;
                                        }
                                    }
                                    //PowerElixirCount
                                    if (player.BarrelForPowerElixirCount > 0)
                                    {
                                        dba.BarrelForPowerElixirCount = 0;
                                        player.BarrelForPowerElixirCount = 0;
                                    }
                                    //HealingElixirCount
                                    if (player.BarrelForHealingElixirCount > 0)
                                    {
                                        dba.BarrelForHealingElixirCount = 0;
                                        player.BarrelForHealingElixirCount = 0;
                                    }
                                }

                                GameServer.Database.SaveObject(dba);

                                //Daily Rewards
                                if (Properties.Allow_Daily_Rewards && dba.Name == player.Name && player.DailyReward == true)
                                {
                                    //log.ErrorFormat("player gefunden1 {0} now {1}", dba.LastPlayed.Day, date.Day);
                                    if (player.Client.Account.PrivLevel < 2 && player.Level >= 45)
                                    {
                                        int number = Util.Random(1, 5);

                                        switch (number)
                                        {
                                            case 1:
                                                {
                                                    int scales = Util.Random(5, 50);
                                                    player.ReceiveItems(player, DayItem1, (short)scales);
                                                    msg = "You recive " + scales + " Dragon Scales";
                                                    break;
                                                }
                                            case 2:
                                                {
                                                    int glas = Util.Random(5, 50);
                                                    player.ReceiveItems(player, DayItem2, (short)glas);
                                                    msg = "You recive " + glas + " Atlantean Glas";
                                                    break;
                                                }
                                            case 3:
                                                {
                                                    int aurulite = Util.Random(5, 50);
                                                    player.ReceiveItems(player, DayItem3, (short)aurulite);
                                                    msg = "You recive " + aurulite + " Aurulite";
                                                    break;
                                                }
                                            case 4:
                                                {
                                                    int diamond = Util.Random(5, 15);
                                                    player.ReceiveItems(player, DayItem4, (short)diamond);
                                                    msg = "You recive " + diamond + " Diamond Seals";
                                                    break;
                                                }
                                            case 5:
                                                {
                                                    int bps = Util.Random(35, 50);
                                                    player.GainBountyPoints((short)bps);
                                                    msg = "You recive " + bps + " Bounty Points";
                                                    break;
                                                }
                                        }
                                       player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "YouReciveForDay", player.Name, msg), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                   
                                    }


                                    if (player.Client.Account.PrivLevel < 2 && player.Level < 45 && player.Level >= 35)
                                    {

                                        int sapphire = Util.Random(5, 10);
                                        player.Client.Player.ReceiveItems(player, DayItem5, (short)sapphire);
                                        msg = "You recive " + sapphire + " Sapphire Seals";
                                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "YouReciveForDay", player.Name, msg), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                        break;


                                    }


                                    if (player.Client.Account.PrivLevel < 2 && player.Level < 35 && player.Level >= 15)
                                    {

                                        int emerald = Util.Random(5, 10);
                                        player.Client.Player.ReceiveItems(player, DayItem6, (short)emerald);
                                        msg = "You recive " + emerald + " Emerald Seals";
                                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "YouReciveForDay", player.Name, msg), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                        break;

                                    }
                                   
                                }
                               
                            }
                         
                           #endregion
                        }
                    }
                }
            }
        }






        /// <summary>
        /// Event handler fired when players enters the game
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        private static void PlayerEntered(DOLEvent e, object sender, EventArgs arguments)
        {


            GamePlayer player = sender as GamePlayer;
            if (player == null) return;
            
            //Daily Events
            DailyEvents(player);
           

            if (String.IsNullOrEmpty(Properties.MOTD) == false)
            {
                if (Properties.MOTD.Length >= 2048)
                {
                    log.ErrorFormat("Properties.MOTD is geater than 2048. Properties.MOTD ignored!");

                }
                else
                    player.Out.SendMessage(Properties.MOTD, eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
            //RvR enter timer after realm switch
            IList<Account> PlayerAccountName = GameServer.Database.SelectObjects<Account>("`Name` = @Name", new QueryParameter("@Name", player.Client.Account.Name));

            foreach (Account db in PlayerAccountName)
            {
                if (db.PrivLevel == 1 && db != null && db.LastLoginRealm != 0 && db.IsOtherRealmAllowed == false && (byte)player.Realm != db.LastLoginRealm && Properties.LOGIN_OTHER_RELAM_SWITCH_TIMER_MINUTES > 0)
                {

                    TimeSpan diff;
                    diff = DateTime.Now - db.LoginWaitTime;


                    
                    if (diff.TotalMinutes >= Properties.LOGIN_OTHER_RELAM_SWITCH_TIMER_MINUTES)
                    {


                        if (player.CurrentRegion != null && player.CurrentRegion.IsRvR)
                        {
                            switch (player.Client.Player.Realm)
                            {
                                case eRealm.Albion:

                                    //Sauvage Faste
                                    player.Client.Player.MoveTo(1, 584099, 486558, 2184, 3558);
                                    break;

                                case eRealm.Midgard:

                                    //Svasud Faste
                                    player.Client.Player.MoveTo(100, 764953, 673088, 5736, 2085);
                                    break;

                                case eRealm.Hibernia:
                                    //Druim Liegen
                                    player.Client.Player.MoveTo(200, 333997, 420350, 5184, 200);
                                    break;

                                default: break;
                            }
                        }
                    }
                }
            }

            //AlbLaunch 
            if (player.CurrentRegionID == 498 && player.Client.Account.PrivLevel < (uint)ePrivLevel.GM)
            {
                if (player.ObjectState == GameObject.eObjectState.Active)
                {

                    switch (player.Realm)
                    {
                        case eRealm.Albion:
                            //player.MoveTo(166, 38027, 53524, 4154, 2527);
                            //Sauvage Faste
                            player.Client.Player.MoveTo(1, 584099, 486558, 2184, 3558);
                            break;

                        case eRealm.Midgard:
                            //player.MoveTo(166, 54809, 24284, 4320, 958);
                            //Svasud Faste
                            player.Client.Player.MoveTo(100, 764953, 673088, 5736, 2085);
                            break;

                        case eRealm.Hibernia:
                            //player.MoveTo(166, 18211, 17653, 4320, 3924);
                            //Druim Liegen
                            player.Client.Player.MoveTo(200, 333997, 420350, 5184, 200);
                            break;

                        default: break;
                    }

                }
                // player.MoveToBind();
            }
           



            if (player.TempProperties.getProperty<bool>(GamePlayer.PlayerRespecMSG) == true)
            {
                string Skill_msg = @"Wrong skill points where found! Your characters Skills has bean reset to correct values. - You must seek a Trainer and re-distribute your Skill-Points.";

                player.Out.SendMessage(Skill_msg, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                player.TempProperties.removeProperty(GamePlayer.PlayerRespecMSG);
            }

           

            if (player.IsAnonymous)
                return;



            foreach (GameClient pclient in WorldMgr.GetAllPlayingClients())
            {
                if (player.Client == pclient)
                    continue;

                string message = LanguageMgr.GetTranslation(pclient, "Scripts.Events.PlayerEnterExit.Entered", player.Name);


                if (player.Client.Account.PrivLevel > 1)
                {
                    message = LanguageMgr.GetTranslation(pclient, "Scripts.Events.PlayerEnterExit.Staff", message);
                }



                if (ServerProperties.Properties.SHOW_LOGINS && player.Realm == pclient.Player.Realm)
                {
                    eChatType chatType = eChatType.CT_System;

                    if (Enum.IsDefined(typeof(eChatType), ServerProperties.Properties.SHOW_LOGINS_CHANNEL))
                        chatType = (eChatType)ServerProperties.Properties.SHOW_LOGINS_CHANNEL;

                    pclient.Out.SendMessage(message, chatType, eChatLoc.CL_SystemWindow);




                }
            }
        }

        /// <summary>
        /// Event handler fired when player leaves the game
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        private static void PlayerQuit(DOLEvent e, object sender, EventArgs arguments)
        {
            GamePlayer player = sender as GamePlayer;

            
            if (player.InHouse)
            {
               player.TargetObject = null;//The target must be remove 
               player.LeaveHouse();
            }
            //RvR enter timer after realm switch
            IList<Account> lastAccountId = GameServer.Database.SelectObjects<Account>("`AccountID` = @AccountID", new QueryParameter("@AccountID", player.Client.Account.AccountID));

            if (lastAccountId != null)
            {
                try
                {
                    //Speichern des des letzten Realms nach dem ausloggen und kein weiteres Speichern solange der Timmer noch gillt.
                    foreach (Account db in lastAccountId)
                    {
                        if (db.AccountID == player.Client.Account.AccountID && string.IsNullOrEmpty(player.Client.Account.AccountID) == false)
                        {

                            //now set the last login Realm
                            if (db.LastLoginRealm != (int)player.Realm)
                            {
                                if (db.LoginWaitTime <= DateTime.Now)
                                {
                                    db.LastLoginRealm = (int)player.Realm;
                                }
                                //continue;
                            }
                            if (db.LoginWaitTime > DateTime.Now)
                            {
                                if (db.Realm != 0)
                                    db.Realm = 0;
                            }
                            GameServer.Database.SaveObject(db);

                        }
                    }
                    lastAccountId.Clear();
                }
                catch
                {
                    //log.Error("Player EnterExit: Kann im moment das realm nicht speichern!");
                }
            }

           


            if (ServerProperties.Properties.SHOW_LOGINS == false)
                return;


            if (player == null) return;

           
            

            if (player.IsAnonymous) return;
            if (player.Client.Account.PrivLevel < 2) return; // loginshow fix

           
            foreach (GameClient pclient in WorldMgr.GetAllPlayingClients())
            {
                if (player.Client == pclient)
                    continue;

                string message = LanguageMgr.GetTranslation(pclient, "Scripts.Events.PlayerEnterExit.Left", player.Name);

                if (player.Client.Account.PrivLevel > 1)
                {
                    message = LanguageMgr.GetTranslation(pclient, "Scripts.Events.PlayerEnterExit.Staff", message);
                }
                else
                {
                    string realm = String.Empty;
                    if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_Normal)
                    {
                        realm = "[" + GlobalConstants.RealmToName(player.Realm) + "] ";
                    }
                    message = realm + message;
                }

                eChatType chatType = eChatType.CT_System;

                if (Enum.IsDefined(typeof(eChatType), ServerProperties.Properties.SHOW_LOGINS_CHANNEL))
                    chatType = (eChatType)ServerProperties.Properties.SHOW_LOGINS_CHANNEL;

                pclient.Out.SendMessage(message, chatType, eChatLoc.CL_SystemWindow);


            }
          
        }
    }
}