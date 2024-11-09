
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
// changed by Elcotek: add Realms/Staff in seperate lists and let see gms all message in the broatchat

using System.Collections;
using DOL.Language;
using DOL.GS;
using DOL.GS.ServerProperties;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
    [CmdAttribute(
         "&broadcast",
         new string[] { "&b" },
         ePrivLevel.Player,
         "Broadcast something to other players in the same zone",
         "/b <message>")]
    public class BroadcastCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        private enum eBroadcastType : int
        {
            Area = 1,
            Visible = 2,
            Zone = 3,
            Region = 4,
            Realm = 5,
            Server = 6,
        }

        public void OnCommand(GameClient client, string[] args)
        {
            const string BROAD_TICK = "Broad_Tick";
            if (args.Length < 2)
            {
                DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Broadcast.NoText"));
                return;
            }
            player = client.Player;
            if (client.Player.Level < 5 && client.Account.PrivLevel == (uint)ePrivLevel.Player)
            {
                client.Player.Out.SendMessage("You need first level 5 for the broat. You cannot broadcast.", eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
                return;
            }
            if (client.Player.IsMuted)
            {
                client.Player.Out.SendMessage("You have been muted. You cannot broadcast.", eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
                return;
            }
            string message = string.Join(" ", args, 1, args.Length - 1);



            if (message.Contains("gm?") || message.Contains("Admin?") || message.Contains("admin?") || message.Contains("GM?"))
            {
                //message = "use /who gm if you need one";
                client.Player.Out.SendMessage("use /who gm if you need one", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            


            long BroadTick = client.Player.TempProperties.getProperty<long>(BROAD_TICK);
            if (BroadTick > 0 && BroadTick - client.Player.CurrentRegion.Time <= 0)
            {
                client.Player.TempProperties.removeProperty(BROAD_TICK);
            }
            long changeTime = client.Player.CurrentRegion.Time - BroadTick;
            if (changeTime < 800 && BroadTick > 0)
            {
                client.Player.Out.SendMessage("Slow down! Think before you say each word!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Player.TempProperties.setProperty(BROAD_TICK, client.Player.CurrentRegion.Time);
                return;
            }
            
            Broadcast(client.Player, message);

            client.Player.TempProperties.setProperty(BROAD_TICK, client.Player.CurrentRegion.Time);
        }
        string realms = "";
        private GamePlayer player;
        private void Broadcast(GamePlayer player, string message)
        {
            //add Player Realms to the list
            foreach (GamePlayer p in GetTargets(player))
            {
                if (player.Client.Account.PrivLevel == (int)ePrivLevel.Player)
                {
                    //Ignore list
                    if (player != null && p.IgnoreList.Contains(player.Name))
                        continue;


                    if (player.Realm == eRealm.Albion)
                        realms = "Albion ";
                    if (player.Realm == eRealm.Hibernia)
                        realms = "Hibernia ";
                    if (player.Realm == eRealm.Midgard)
                        realms = "Midgard ";
                }
                    //add the Staff to the list
                else if (player.Client.Account.PrivLevel >= (int)ePrivLevel.GM)
                {
                    realms = "Staff ";
                }
                if (GameServer.ServerRules.IsAllowedToUnderstand(p, player) || p.Client.Account.PrivLevel == (int)ePrivLevel.GM || ((eBroadcastType)ServerProperties.Properties.BROADCAST_TYPE == eBroadcastType.Server))
                {


                    p.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Players.Broadcast.Message", realms + player.Name, message), eChatType.CT_Broadcast, eChatLoc.CL_ChatWindow);
                }
            }

        }

        private ArrayList GetTargets(GamePlayer player)
        {
            ArrayList list = new ArrayList();
            eBroadcastType type = 0;
            if (player.Client.Account.PrivLevel >= (int)ePrivLevel.GM)
            {
                eBroadcastType type1 = eBroadcastType.Server;
                type = type1;
            }
            if (player.Client.Account.PrivLevel == (int)ePrivLevel.Player)
            {
                eBroadcastType type2 = (eBroadcastType)ServerProperties.Properties.BROADCAST_TYPE;
                type = type2;
            }
            switch (type)
            {
                case eBroadcastType.Area:
                    {
                        bool found = false;
                        foreach (AbstractArea area in player.CurrentAreas)
                        {
                            if (area.CanBroadcast)
                            {
                                found = true;
                                foreach (GameClient thisClient in WorldMgr.GetClientsOfRegion(player.CurrentRegionID))
                                {
                                    if (thisClient.Player.CurrentAreas.Contains(area))
                                    {
                                        list.Add(thisClient.Player);
                                    }
                                }
                            }
                        }
                        if (!found)
                        {
                            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Players.Broadcast.NoHere"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        break;
                    }
                case eBroadcastType.Realm:
                    {
                        foreach (GameClient thisClient in WorldMgr.GetClientsOfRealm(player.Realm))
                        {
                            if (thisClient.Account.PrivLevel == (int)ePrivLevel.Player)
                                //add all players of a Realm
                                list.Add(thisClient.Player);
                        }
                        foreach (GameClient thisClient in WorldMgr.GetAllPlayingClients())
                        {
                          if  (thisClient.Account.PrivLevel >= (int)ePrivLevel.GM)
                            list.Add(thisClient.Player); //add all players to the GM list
                        }
                        break;
                    }
                case eBroadcastType.Region:
                    {
                        foreach (GameClient thisClient in WorldMgr.GetClientsOfRegion(player.CurrentRegionID))
                        {
                            list.Add(thisClient.Player);
                        }
                        break;
                    }
                case eBroadcastType.Server:
                    {
                        foreach (GameClient thisClient in WorldMgr.GetAllPlayingClients())
                        {
                            list.Add(thisClient.Player);
                        }
                        break;
                    }
                case eBroadcastType.Visible:
                    {
                        foreach (GamePlayer p in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                        {
                            list.Add(p);
                        }
                        break;
                    }
                case eBroadcastType.Zone:
                    {
                        foreach (GameClient thisClient in WorldMgr.GetClientsOfRegion(player.CurrentRegionID))
                        {
                            if (thisClient.Player.CurrentZone == player.CurrentZone)
                            {
                                list.Add(thisClient.Player);
                            }
                        }
                        break;
                    }
            }

            return list;
        }
    }
}
             