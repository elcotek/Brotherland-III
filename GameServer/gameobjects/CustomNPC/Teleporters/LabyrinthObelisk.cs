using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS
{
    public class LabyObelisk : GameNPC
    {


        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        
        protected virtual void HubObelisk(GamePlayer player)
        {
            string obelisklist = "Select a location where you wish to teleport: \n[Collapsed] [Flooded] [Clockwork]";
            player.Out.SendMessage(obelisklist, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
        }
        protected virtual void Obelisk(GamePlayer player)
        {
            string obelisklist = "Do you want to [return] to the hub?";
            player.Out.SendMessage(obelisklist, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
        }
        protected virtual void CollapsedObelisk(GamePlayer player)
        {
            string obelisklist = "|Collapsed| [Flooded] [Clockwork]";
            string allobelisk = "Agramon's Lair;Discovery's Crossing;Forge of Pyrkagia;Shrine of Laresh (West);Shrine of Teragani (North);Temple of Februstos (North);";
            player.LoadLabyrinthObelisk(allobelisk, obelisklist);
        }
        protected virtual void FloodedObelisk(GamePlayer player)
        {
            string obelisklist = "[Collapsed] |Flooded| [Clockwork]";
            string allobelisk = "Anapaysi's Crossing;Catacombs of Februstos;Diabasi's Junction;Dracolich Den;Dynami's Crossing;Hall of Feretro;Hall of Thanatoy;Passage of Ygros;Path of Zoi;Plimmyra's Landing;Shrine of Teragani (South);Shrine of Vartigeth (Hib);Shrine of Vartigeth (Alb);Shrine of Vartigeth (Mid);Shrine of Nethuni (North);Temple of Laresh;Temple of Perizor (West);Ygrasia's Crossing;";
            player.LoadLabyrinthObelisk(allobelisk, obelisklist);
        }
        protected virtual void ClockworkObelisk(GamePlayer player)
        {
            string obelisklist = "[Collapsed] [Flooded] |Clockwork|";
            string allobelisk = "Construct Assembly Room;Efeyresi's Junction;Ergaleio's Path;Great Forge of Thivek;Hall of Allagi;Hall of Dimioyrgia;Kainotomia's Crossing;Path of Roloi;Shrine of Laresh (East);Shrine of Nethuni (South);Shrine of Tegashirg;Temple of Februstos (South);Temple of Perizor (East);Trela's Crossing;";
            player.LoadLabyrinthObelisk(allobelisk, obelisklist);
        }
        /// <summary>
        /// change model of the teleporter
        /// </summary>
        /// <returns></returns>
        public override bool AddToWorld()
        {
            this.Model = 2256;
            this.Size = 50;
            return base.AddToWorld();
        }
        /// <summary>
        /// Interact with the NPC.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player) || player == null)
                return false;
            if (player.InCombat)
            {
                player.Out.SendMessage("You are currently in Combat!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }
            GamePlayerUtils.SendClearPopupWindow(player);
            GameNPC[] mob = WorldMgr.GetNPCsByName(this.Name, Realm);
            bool namecheck = false;
          
            string allobelisk = "Nurizane's Crossroads;Agramon's Lair;Discovery's Crossing;Forge of Pyrkagia;Shrine of Laresh (West);Shrine of Teragani (North);Temple of Februstos (North);Anapaysi's Crossing;Catacombs of Februstos;Diabasi's Junction;Dracolich Den;Dynami's Crossing;Hall of Feretro;Hall of Thanatoy;Passage of Ygros;Path of Zoi;Plimmyra's Landing;Shrine of Teragani (South);Shrine of Vartigeth (Hib);Shrine of Vartigeth (Alb);Shrine of Vartigeth (Mid);Shrine of Nethuni (North);Temple of Laresh;Temple of Perizor (West);Ygrasia's Crossing;Construct Assembly Room;Efeyresi's Junction;Ergaleio's Path;Great Forge of Thivek;Hall of Allagi;Hall of Dimioyrgia;Kainotomia's Crossing;Path of Roloi;Shrine of Laresh (East);Shrine of Nethuni (South);Shrine of Tegashirg;Temple of Februstos (South);Temple of Perizor (East);Trela's Crossing;";
            string allobeliskComplette = "Shrine of Tegashirg;Discovery's Crossing;Plimmyra's Landing;Shrine of Nethuni (North);Hall of Feretro;Diabasi's Junction;Dynami's Crossing;Shrine of Vartigeth (Hib);Path of Zoi;Shrine of Vartigeth (Mid);Trela's Crossing;Hall of Thanatoy;Shrine of Teragani (South);Passage of Ygros;Shrine of Vartigeth (Alb);Agramon's Lair;Anapaysi's Crossing;Dracolich Den;Ygrasia's Crossing;Temple of Laresh;Temple of Perizor (West);Catacombs of Februstos;Efeyresi's Junction;Great Forge of Thivek;Kainotomia's Crossing;Hall of Dimioyrgia;Shrine of Nethuni (South);Temple of Februstos (South);Ergaleio's Path;Path of Roloi;Shrine of Laresh (East);Hall of Allagi;Construct Assembly Room;Temple of Perizor (East);Temple of Februstos (North);Forge of Pyrkagia;Shrine of Teragani (North);Shrine of Laresh (West);";

            //Nurizane's Crossroads;Agramon's Lair;Discovery's Crossing;Forge of Pyrkagia;Shrine of Laresh (West);Shrine of Teragani (North);Temple of Februstos (North);Anapaysi's Crossing;Catacombs of Februstos;Diabasi's Junction;Dracolich Den;Dynami's Crossing;Hall of Feretro;Hall of Thanatoy;Passage of Ygros;Path of Zoi;Plimmyra's Landing;Shrine of Teragani (South);Shrine of Vartigeth (Hib);Shrine of Vartigeth (Alb);Shrine of Vartigeth (Mid);Shrine of Nethuni (North);Temple of Laresh;Temple of Perizor (West);Ygrasia's Crossing;Construct Assembly Room;Efeyresi's Junction;Ergaleio's Path;Great Forge of Thivek;Hall of Allagi;Hall of Dimioyrgia;Kainotomia's Crossing;Path of Roloi;Shrine of Laresh (East);Shrine of Nethuni (South);Shrine of Tegashirg;Temple of Februstos (South);Temple of Perizor (East);Trela's Crossing
            //Shrine of Tegashirg;Discovery's Crossing;Plimmyra's Landing;Shrine of Nethuni (North);Hall of Feretro;Diabasi's Junction;Dynami's Crossing;Shrine of Vartigeth (Hib);Path of Zoi;Shrine of Vartigeth (Mid);Trela's Crossing;Hall of Thanatoy;Shrine of Teragani (South);Passage of Ygros;Shrine of Vartigeth (Alb);Agramon's Lair;Anapaysi's Crossing;Dracolich Den;Ygrasia's Crossing;Temple of Laresh;Temple of Perizor (West);Catacombs of Februstos;Efeyresi's Junction;Great Forge of Thivek;Kainotomia's Crossing;Hall of Dimioyrgia;Shrine of Nethuni (South);Temple of Februstos (South);Ergaleio's Path;Path of Roloi;Shrine of Laresh (East);Hall of Allagi;Construct Assembly Room;Temple of Perizor (East);Temple of Februstos (North);Forge of Pyrkagia;Shrine of Teragani (North);Shrine of Laresh (West);

            
            
            foreach (string dAllObelisk in allobelisk.Split(';'))
            {
                if (dAllObelisk.Length < 2) continue;
                if (dAllObelisk == this.Name) namecheck = true;

            }
            

            if (!namecheck)
            {
                player.Out.SendMessage("This obelisk has an invalid name. Check DOL forum for a complete list of names.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return false;
            }
            if (mob.Length > 1 && this.Name != "Nurizane's Crossroads")
            {
                player.Out.SendMessage("There already is a Teleporter with the name: " + this.Name + ". Please report this message to a GM/Admin!\n\n", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            }
            else if (!player.HaveLabyrinthObelisk(this.Name) && this.Name != "Nurizane's Crossroads")
            {
                player.LabyrinthObelisk += this.Name + ";";
                player.Out.SendMessage("You have activated " + this.Name + "-Obelisk!", eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
                player.SaveIntoDatabase();
                player.Out.SendSpellEffectAnimation(this, this, 1657, 0, false, 1);

                //Player Title
                if (player.LabyrinthObelisk != null && allobeliskComplette.Length == player.LabyrinthObelisk.Length && player.HasAllOberlisks == false)
                {
                    player.HasAllOberlisks = true;
                    player.SaveIntoDatabase();
                    player.UpdatePlayerStatus();
                    player.UpdateCurrentTitle();
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Obelisk.GetTitle.Congratulations", player.GetName(0, false)), eChatType.CT_ScreenCenter, eChatLoc.CL_PopupWindow);
                    player.Out.SendSpellEffectAnimation(this, this, 1692, 0, false, 1);
                }
            }
            else
            {
                if (this.Name == "Nurizane's Crossroads")
                    HubObelisk(player);
                else

                //Player Title
                if (player.LabyrinthObelisk != null && allobeliskComplette.Length == player.LabyrinthObelisk.Length && player.HasAllOberlisks == false)
                {
                    player.HasAllOberlisks = true;
                    player.SaveIntoDatabase();
                    player.UpdatePlayerStatus();
                    player.UpdateCurrentTitle();
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Obelisk.GetTitle.Congratulations", player.GetName(0, false)), eChatType.CT_ScreenCenter, eChatLoc.CL_PopupWindow);
                    player.Out.SendSpellEffectAnimation(this, this, 1692, 0, false, 1);
                }
                Obelisk(player);
            }
            return false;
        }

        /// <summary>
        /// Talk to the NPC.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public override bool WhisperReceive(GameLiving source, string text)
        {
            if (!base.WhisperReceive(source, text) || !(source is GamePlayer))
                return false;
            GamePlayer player = source as GamePlayer;
            if (text == "return" && this.Name != "Nurizane's Crossroads")
            {
                //there are 4 obelisks at the hub and you get ported to one of them randomly
                GameNPC[] mob = WorldMgr.GetNPCsByName("Nurizane's Crossroads", Realm);
                int randomobelisk = Util.Random(0, 3);
               
                if (mob[randomobelisk] != null)
                {

                    if (player.Group != null)
                    {
                                            
                        {
                            //PlayerGroup
                            foreach (GamePlayer players in player.Group.GetPlayersInTheGroup())
                            {
                                if (players.IsWithinRadius(this, 500) == false)
                                    continue;
                                  
                                    
                                    players.MoveTo((ushort)mob[randomobelisk].CurrentRegionID, mob[randomobelisk].X, mob[randomobelisk].Y, mob[randomobelisk].Z, (ushort)mob[randomobelisk].Heading);
                                    player.StartInvulnerabilityTimer(ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);

                            }
                        }
                    }
                    else
                    {
                       
                        player.MoveTo((ushort)mob[randomobelisk].CurrentRegionID, mob[randomobelisk].X, mob[randomobelisk].Y, mob[randomobelisk].Z, (ushort)mob[randomobelisk].Heading);
                        player.StartInvulnerabilityTimer(ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                    }
                                       
                }
            }
            else
            {
                if (player.HaveLabyrinthObelisk(text))
                {
                    GameNPC[] mob = WorldMgr.GetNPCsByName(text, Realm);
                    if (mob[0] != null)
                    {
                        player.MoveTo((ushort)mob[0].CurrentRegionID, mob[0].X, mob[0].Y, mob[0].Z, (ushort)mob[0].Heading);
                        player.StartInvulnerabilityTimer(ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                    }
                }
                switch (text.ToLower())
                {
                    case "collapsed":
                        CollapsedObelisk(player);
                        break;
                    case "flooded":
                        FloodedObelisk(player);
                        break;
                    case "clockwork":
                        ClockworkObelisk(player);
                        break;
                }
            }
            return true;
        }
    }
}