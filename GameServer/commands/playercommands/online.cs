/*Reworked by Tinantiol to work with the lastest SVN (2350).
 * I don't know who are/is the original autor/autors, but the 
 * credits go to them, i just take the script into User Release 
 * Section and fixed to work with the lastest SVN.
 * Please guys, Dawn Of Light is an open source project, you all 
 * are using Tortoise SVN to get the lastest version of the code
 * that DOL staff is fixing for you, so now go to upload all your
 * scripts into the User Releases Section and get DOL at the TOP!
 */
using System;
using System.Collections;
using System.Collections.Generic;
using DOL.GS.Keeps;
using DOL.Language;
using DOL.GS.ServerProperties;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
    [CmdAttribute(
        "&online",
         new string[] { "&on" },
        ePrivLevel.Player,
        "Shows all Players that are currently online",
        "Usage: /online")]
    public class OnlineCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        // ~~~~~~~~~~~~~~~~~~~~  CONFIG  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

      

        //set this to true or false to display additional info about connecting, disconnecting, playing clients. 
        private static bool showAddOnlineInfo = true;

       
        //set this to true or false to show the realms population (eG: Albion: 13 34% 12Tanks | 1 Caster ... )
        private static bool showRealms = true;

        // set this to true or false to display players by zone (zone id must be added below)
       // private static bool showByZone = true;
        //add the ID´s of the zones(!) you want the command to display here
        //get the ID´s from your Zone Table, keep in mind Zones != Regions.
        private static ushort[] zoneIDs = { 335, 26 };

        // set this to true or false to display a displayed list of currently loged in classes.
        //private static bool showDetailedClass = true;

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


        public class ClassToCount
        {
            public string name;
            public int count;

            public ClassToCount(string name, int count)
            {
                this.name = name;
                this.count = count;
            }
        }

       
        public void OnCommand(GameClient client, string[] args)
        {
            List<string> textList = this.GetOnlineInfo(client.Account.PrivLevel >= (uint)ePrivLevel.GM, client);
            client.Out.SendCustomTextWindow("Currently Online", textList);
            return;
        }
      

        public List<string> GetOnlineInfo(bool bGM, GameClient PlayerClient)
        {
            List<string> output = new List<string>();
            IList<GameClient> clients = WorldMgr.GetAllPlayingClients();



            int connecting = 0, charscreen = 0, enterworld = 0,
                playing = 0, linkdeath = 0, disconnecting = 0;
            int midgard = 0, hibernia = 0, albion = 0, Jordheim = 0, Camelot = 0, TirNaNog = 0, midgardLevel50 = 0, hiberniaLevel50 = 0, albionLevel50 = 0;



            // Number of Alb, Mid and Hib tanks:
            foreach (GameClient c in clients)
            {
                if (c == null)
                    continue;
                

                #region count GMs, and different client states
                if (c.ClientState == GameClient.eClientState.Connecting &&  c.Account.PrivLevel != (uint)ePrivLevel.Admin)
                    ++connecting;
                else if (c.ClientState == GameClient.eClientState.Disconnected && c.Account.PrivLevel != (uint)ePrivLevel.Admin)
                    ++disconnecting;
                else if (c.ClientState == GameClient.eClientState.CharScreen && c.Account.PrivLevel != (uint)ePrivLevel.Admin)
                    ++charscreen;
                else if (c.ClientState == GameClient.eClientState.Linkdead && c.Account.PrivLevel != (uint)ePrivLevel.Admin)
                    ++linkdeath;
                else if (c.ClientState == GameClient.eClientState.WorldEnter && c.Account.PrivLevel != (uint)ePrivLevel.Admin)
                    ++enterworld;
                else if (c.ClientState == GameClient.eClientState.Playing && c.Account.PrivLevel != (uint)ePrivLevel.Admin)
                    ++playing;
                else if (c.Account.PrivLevel == (uint)ePrivLevel.Admin)
                {
                //do nothing
                }
                else
                    continue;

                
                // if a legal playing client, count some special things
                if (!c.IsPlaying
                    || c.Account == null
                    || c.Player == null
                    || c.Player.ObjectState != GameObject.eObjectState.Active)
                    continue;

                
                #endregion

                #region realm specific counting
                switch (c.Player.Realm)
                {   
                    // Alb:
                    case eRealm.Albion:
                        {
                            if (c.Account.PrivLevel != (uint)ePrivLevel.Admin)
                                ++albion;
                            if (c.Player.CurrentRegionID == 10 && c.Account.PrivLevel != (uint)ePrivLevel.Admin)
                                ++Camelot;
                            if (c.Player.Level == 50 && c.Account.PrivLevel != (uint)ePrivLevel.Admin)
                                ++albionLevel50;

                        }
                        break;

                    // Mid:
                    case eRealm.Midgard:
                        {
                            if (c.Account.PrivLevel != (uint)ePrivLevel.Admin)
                                ++midgard;
                            if (c.Player.CurrentRegionID == 101 && c.Account.PrivLevel != (uint)ePrivLevel.Admin)
                                ++Jordheim;
                            if (c.Player.Level == 50 && c.Account.PrivLevel != (uint)ePrivLevel.Admin)
                                ++midgardLevel50;
                        }

                        break;

                    // Hib:
                    case eRealm.Hibernia:
                        {
                            if (c.Account.PrivLevel != (uint)ePrivLevel.Admin)
                                ++hibernia;
                            if (c.Player.CurrentRegionID == 201 && c.Account.PrivLevel != (uint)ePrivLevel.Admin)
                                ++TirNaNog;
                            if (c.Player.Level == 50 && c.Account.PrivLevel != (uint)ePrivLevel.Admin)
                                ++hiberniaLevel50;
                        }
                        break;

                }
                #endregion
            }
            
            #region overview and class-specific
            int entering = connecting + enterworld + charscreen;
            int leaving = disconnecting + linkdeath;
            int albTotal = albion;
            int midTotal = midgard;
            int hibTotal = hibernia;
            int total = entering + playing + leaving;
            output.Add(string.Format("online informations System \n"));
            output.Add(string.Format("Currently online:  {0}\n Playing total:  {1} | Entering:  {2} | Leaving:  {3}",
                                      total, playing, entering, leaving));
            if (showAddOnlineInfo == true)
            {
                output.Add(string.Format("\n  (Connecting:  {0} | CharScreen:  {1} | EnterWorld:  {2} \nPlaying:  {3} | LinkDeath:  {4} | Disconnected:  {5})",
                                              connecting, enterworld, charscreen, playing, linkdeath, disconnecting));
            }

            if (showRealms == true)
            {
                if (total > 0)
                {
                    output.Add(string.Format("\nAlbion:   {0} ( {1}% )  \nCity of Camelot:   {2} \nLevel 50 online: {3}", albTotal, (int)(albTotal * 100 / total), Camelot, albionLevel50));
                    output.Add(string.Format("\nMidgard:  {0} ( {1}% )  \nCity of Jordheim:  {2} \nLevel 50 online: {3}", midTotal, (int)(midTotal * 100 / total), Jordheim, midgardLevel50));
                    output.Add(string.Format("\nHibernia:  {0} ( {1}% ) \nCity of Tir na Nog: {2} \nLevel 50 online: {3}", hibTotal, (int)(hibTotal * 100 / total), TirNaNog, hiberniaLevel50));
                }
                if (PlayerClient.Player != null)
                {
                    output.Add(string.Format("\n"));//abstände einfügen
                    /*
                    output.Add(string.Format((LanguageMgr.GetTranslation((PlayerClient.Player as GamePlayer).Client.Account.Language, "AntiRaidMsg3", Properties.RAID_MEMBER_COUNT + 1)), (Properties.RAID_MEMBER_COUNT + 1)));

                    if (PlayerClient.Player != null && PlayerClient.Player.AllowedToRaid() == false)
                    {
                        output.Add(string.Format("\n"));
                        output.Add(string.Format((LanguageMgr.GetTranslation((PlayerClient.Player as GamePlayer).Client.Account.Language, "AntiRaidMsg4"))));
                        output.Add(string.Format((LanguageMgr.GetTranslation((PlayerClient.Player as GamePlayer).Client.Account.Language, "AntiRaidMsg5"))));
                        output.Add(string.Format("\n"));
                    }
                    else if (PlayerClient.Player != null && PlayerClient.Player.AllowedToRaid())
                    {
                        output.Add(string.Format("\n"));
                        output.Add(string.Format((LanguageMgr.GetTranslation((PlayerClient.Player as GamePlayer).Client.Account.Language, "AntiRaidMsg6"))));
                        output.Add(string.Format("\n"));
                        output.Add(string.Format((LanguageMgr.GetTranslation((PlayerClient.Player as GamePlayer).Client.Account.Language, "AntiRaidMsg7"))));
                        output.Add(string.Format((LanguageMgr.GetTranslation((PlayerClient.Player as GamePlayer).Client.Account.Language, "AntiRaidMsg8"))));
                        output.Add(string.Format("\n"));
                    }
                    */
                    output.Add(string.Format((LanguageMgr.GetTranslation((PlayerClient.Player as GamePlayer).Client.Account.Language, "AntiRaidMsg9"))));
                    output.Add(string.Format((LanguageMgr.GetTranslation((PlayerClient.Player as GamePlayer).Client.Account.Language, "AntiRaidMsg10"))));
                    output.Add(string.Format((LanguageMgr.GetTranslation((PlayerClient.Player as GamePlayer).Client.Account.Language, "AntiRaidMsg11"))));
                    output.Add(string.Format("\n"));
                    output.Add(string.Format((LanguageMgr.GetTranslation((PlayerClient.Player as GamePlayer).Client.Account.Language, "AntiRaidMsg12"))));
                }
            }
            #endregion


            return output;
        }
    }
}

