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
/*
TODO: listStart

/who  Can be modified with
[playername],
[class],
[#] level,
[location],
[##] [##] level range -
please note that /who CSR will not show hidden CSRs


1.69
- The /who command has been altered to show a [CG] and a [BG] next to players'
 names who are the leaders of a public chat group and battlegroup respectively.

- The /who command now allows multiple different search filters at once.
 For example, typing /who 40 50 Wizard Emain Dragonhearts would list all of the
 level 40 through 50 Wizards currently in Emain Macha with a guild that matches
 the "Dragonhearts" filter.

 */

using DOL.GS.PacketHandler;
using System;
using System.Collections;
using System.Text;

namespace DOL.GS.Commands
{
    [CmdAttribute(
        "&who",
        ePrivLevel.Player,
        "Shows who is online",
        //help:
        //"/who  Can be modified with [playername], [class], [#] level, [location], [##] [##] level range",
        "/WHO ALL lists all players online",
        "/WHO NF lists all players online in New Frontiers",
        // "/WHO CSR lists all Customer Service Representatives currently online",
        // "/WHO DEV lists all Development Team Members currently online",
        // "/WHO QTA lists all Quest Team Assistants currently online",
        "/WHO <name> lists players with names that start with <name>",
        "/WHO <guild name> lists players with names that start with <guild name>",
        "/WHO <class> lists players with of class <class>",
        "/WHO <location> lists players in the <location> area",
        "/WHO <level> lists players of level <level>",
        "/WHO <level> <level> lists players in level range",
        "/WHO <language> lists players with a specific language"
    )]
    public class WhoCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public const int MAX_LIST_SIZE = 26;
        public const string MESSAGE_LIST_TRUNCATED = "(Too many matches ({0}).  List truncated.)";
        private const string MESSAGE_NO_MATCHES = "No Matches.";
        private const string MESSAGE_NO_ARGS = "Type /WHO HELP for variations on the WHO command.";
        private const string MESSAGE_PLAYERS_ONLINE = "{0} player{1} currently online.";

        public void OnCommand(GameClient client, string[] args)
        {
            if (IsSpammingCommand(client.Player, "who"))
                return;

            int listStart = 1;
            ArrayList filters = null;
            ArrayList clientsList = new ArrayList();
            ArrayList resultMessages = new ArrayList();

            // get list of clients depending on server type
            foreach (GameClient serverClient in WorldMgr.GetAllPlayingClients())
            {
                GamePlayer addPlayer = serverClient.Player;
                if (addPlayer == null) continue;

                if (serverClient.Account.PrivLevel > (uint)ePrivLevel.Player && serverClient.Player.IsAnonymous)
                {
                    if (clientsList.Contains(addPlayer.Client))
                    {
                        clientsList.Remove(addPlayer.Client);
                    }
                    continue;
                }

                if (serverClient.Account.PrivLevel > (uint)ePrivLevel.Player && serverClient.Player.IsAnonymous == false)
                {
                    clientsList.Add(addPlayer.Client);
                    continue;
                }
                if (client.Account.PrivLevel == (uint)ePrivLevel.Player && addPlayer.Client != client && ServerProperties.Properties.Anon_RvR_Only == true)
                {
                    if (addPlayer.Client.Player.CurrentRegion.IsRvR && serverClient.Account.PrivLevel == (uint)ePrivLevel.Player && addPlayer.IsAnonymous || !GameServer.ServerRules.IsSameRealm(addPlayer, client.Player, true))
                    {
                        continue;
                    }

                }
                else if (ServerProperties.Properties.Anon_RvR_Only == false && addPlayer.Client != client // always add self
                    && client.Account.PrivLevel == (uint)ePrivLevel.Player
                    && (addPlayer.IsAnonymous || !GameServer.ServerRules.IsSameRealm(addPlayer, client.Player, true)))
                {
                    continue;
                }
                clientsList.Add(addPlayer.Client);
            }

            // no params
            if (args.Length == 1)
            {
                int playing = clientsList.Count;

                // including anon?
                DisplayMessage(client, string.Format(MESSAGE_PLAYERS_ONLINE, playing, playing > 1 ? "s" : ""));
                DisplayMessage(client, MESSAGE_NO_ARGS);
                return;
            }


            // any params passed?
            switch (args[1].ToLower())
            {
                case "all": // display all players, no filter
                    {
                        filters = null;
                        break;
                    }
                case "help": // list syntax for the who command
                    {
                        DisplaySyntax(client);
                        return;
                    }
                case "staff":
                case "gm":
                case "admin":
                    {
                        filters = new ArrayList(1);
                        filters.Add(new GMFilter());
                        break;
                    }
                case "en":
                case "cz":
                case "de":
                case "es":
                case "fr":
                case "it":
                    {
                        filters = new ArrayList(1);
                        filters.Add(new LanguageFilter(args[1].ToLower()));
                        break;
                    }
                case "bg":
                    {
                        filters = new ArrayList(1);
                        filters.Add(new BGGroupFilter());
                        break;
                    }
                case "cg":
                    {
                        filters = new ArrayList(1);
                        filters.Add(new ChatGroupFilter());
                        break;
                    }
                case "nf":
                    {
                        filters = new ArrayList(1);
                        filters.Add(new NewFrontiersFilter());
                        break;
                    }
                case "rp":
                    {
                        filters = new ArrayList(1);
                        filters.Add(new RPFilter());
                        break;
                    }
                default:
                    {
                        filters = new ArrayList();
                        AddFilters(filters, args, 1);
                        break;
                    }
            }


            int resultCount = 0;
            foreach (GameClient clients in clientsList)
            {
                if (ApplyFilter(filters, clients.Player))
                {
                    resultCount++;
                    if (resultMessages.Count < MAX_LIST_SIZE && resultCount >= listStart)
                    {
                        resultMessages.Add(resultCount + ") " + FormatLine(clients.Player, client.Account.PrivLevel));
                    }
                }
            }

            foreach (string str in resultMessages)
            {
                DisplayMessage(client, str);
            }

            if (resultCount == 0)
            {
                DisplayMessage(client, MESSAGE_NO_MATCHES);
            }
            else if (resultCount > MAX_LIST_SIZE)
            {
                DisplayMessage(client, string.Format(MESSAGE_LIST_TRUNCATED, resultCount));
            }

            filters = null;
        }



        private class BGFilter : IWhoFilter
        {
            public bool ApplyFilter(GamePlayer player)
            {
               
                    //BGSearch(player);
                    return BGSearch(player);


            }
        }

        public static bool BGResuldFound = false;
       

        public static bool BGSearch(GamePlayer player)
        {
           
            foreach (GameClient clients in WorldMgr.GetAllPlayingClients())
            {

                if (clients.Player.Realm == player.Realm)
                {

                    BattleGroup battleGroup = clients.Player.TempProperties.getProperty<BattleGroup>(BattleGroup.BATTLEGROUP_PROPERTY, null);

                    if (battleGroup != null)
                    {
                        if ((bool)battleGroup.Members[clients.Player] == true && battleGroup.IsPublic)//Der BG Leader
                        {
                            BGResuldFound = true;
                            return true;
                            //log.ErrorFormat("leader = {0}", clients.Player.Name);
                            //result.Append(" [BG Leader: " + clients.Player.Name + " ]");
                        }
                    }
                }
            }
            return false;
        }
           
                    
       

        // make /who line using GamePlayer
        private string FormatLine(GamePlayer player, uint PrivLevel)
        {
            /*
			 * /setwho class | trade
			 * Sets how the player wishes to be displayed on a /who inquery.
			 * Class displays the character's class and level.
			 * Trade displays the tradeskill type and level of the character.
			 * and it is saved after char logs out
			 */
            
            
           
            if (player == null)
            {
                if (log.IsErrorEnabled)
                    log.Error("null player in who command");
                return "???";
            }

            StringBuilder result = new StringBuilder(player.Name, 100);
            
            if (player.GuildName != "")
            {
                result.Append(" <");
                result.Append(player.GuildName);
                result.Append(">");
            }

            // simle format for PvP
            if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvP && PrivLevel == 1)
                return result.ToString();

            result.Append(" the Level ");
            result.Append(player.Level);
            if (player.ClassNameFlag)
            {
                result.Append(" ");
                result.Append(player.CharacterClass.Name);
            }
            else if (player.CharacterClass != null)
            {
                result.Append(" ");
                AbstractCraftingSkill skill = CraftingMgr.getSkillbyEnum(player.CraftingPrimarySkill);
                result.Append(player.CraftTitle);
            }
            else
            {
                if (log.IsErrorEnabled)
                    log.Error("no character class spec in who commandhandler for player " + player.Name);
            }
            if (player.CurrentZone != null)
            {
                result.Append(" in ");
                result.Append(player.CurrentZone.Description);
            }
            else
            {
                if (log.IsErrorEnabled)
                    log.Error("no currentzone in who commandhandler for player " + player.Name);
            }


            ChatGroup mychatgroup = (ChatGroup)player.TempProperties.getProperty<object>(ChatGroup.CHATGROUP_PROPERTY, null);
            if (mychatgroup != null && (mychatgroup.Members.Contains(player) || mychatgroup.IsPublic && (bool)mychatgroup.Members[player] == true))
            {
                result.Append(" [CG]");
            }
            BattleGroup mycbattlegroup = (BattleGroup)player.TempProperties.getProperty<object>(BattleGroup.BATTLEGROUP_PROPERTY, null);
            if (mycbattlegroup != null && (mycbattlegroup.IsPublic && (bool)mycbattlegroup.Members[player] == true))
            {
                result.Append(" [BG Leader]");
            }


            if (player.IsAnonymous)
            {
                result.Append(" <ANON>");
            }
            if (player.TempProperties.getProperty<string>(GamePlayer.AFK_MESSAGE) != null)
            {
                result.Append(" <AFK>");
            }
            if (player.Advisor)
            {
                result.Append(" <ADV>");
            }
            if (player.Client.Account.PrivLevel == (uint)ePrivLevel.GM)
            {
                result.Append(" <GM>");
            }
            if (player.Client.Account.PrivLevel == (uint)ePrivLevel.Admin)
            {
                result.Append(" <Admin>");
            }
            if (ServerProperties.Properties.ALLOW_CHANGE_LANGUAGE)
            {
                result.Append(" <" + player.Client.Account.Language + ">");
            }

            return result.ToString();
        }

        private void AddFilters(ArrayList filters, string[] args, int skip)
        {
            for (int i = skip; i < args.Length; i++)
            {
                if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvP)
                    filters.Add(new StringFilter(args[i]));
                else
                {
                    try
                    {
                        int currentNum = (int)System.Convert.ToUInt32(args[i]);
                        int nextNum = -1;
                        try
                        {
                            nextNum = (int)System.Convert.ToUInt32(args[i + 1]);
                        }
                        catch
                        {
                        }

                        if (nextNum != -1)
                        {
                            filters.Add(new LevelRangeFilter(currentNum, nextNum));
                            i++;
                        }
                        else
                        {
                            filters.Add(new LevelFilter(currentNum));
                        }
                    }
                    catch
                    {
                        filters.Add(new StringFilter(args[i]));
                    }
                }
            }
        }


        private bool ApplyFilter(ArrayList filters, GamePlayer player)
        {
            if (filters == null)
                return true;
            foreach (IWhoFilter filter in filters)
            {
                if (!filter.ApplyFilter(player))
                    return false;
            }
            return true;
        }


        //Filters

        private class StringFilter : IWhoFilter
        {
            private string m_filterString;

            public StringFilter(string str)
            {
                m_filterString = str.ToLower().Trim();
            }

            public bool ApplyFilter(GamePlayer player)
            {
                if (player.Name.ToLower().StartsWith(m_filterString))
                    return true;
                if (player.GuildName.ToLower().StartsWith(m_filterString))
                    return true;
                if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvP)
                    return false;
                if (player.CharacterClass.Name.ToLower().StartsWith(m_filterString))
                    return true;
                if (player.CurrentZone != null && player.CurrentZone.Description.ToLower().Contains(m_filterString))
                    return true;
                return false;
            }
        }

        private class LevelRangeFilter : IWhoFilter
        {
            private int m_minLevel;
            private int m_maxLevel;

            public LevelRangeFilter(int minLevel, int maxLevel)
            {
                m_minLevel = Math.Min(minLevel, maxLevel);
                m_maxLevel = Math.Max(minLevel, maxLevel);
            }

            public bool ApplyFilter(GamePlayer player)
            {
                if (player.Level >= m_minLevel && player.Level <= m_maxLevel)
                    return true;
                return false;
            }
        }

        private class LevelFilter : IWhoFilter
        {
            private int m_level;

            public LevelFilter(int level)
            {
                m_level = level;
            }

            public bool ApplyFilter(GamePlayer player)
            {
                return player.Level == m_level;
            }
        }

        private class GMFilter : IWhoFilter
        {
            public bool ApplyFilter(GamePlayer player)
            {
                if (!player.IsAnonymous && player.Client.Account.PrivLevel > (uint)ePrivLevel.Player)
                    return true;
                return false;
            }
        }

        private class LanguageFilter : IWhoFilter
        {
            private string m_str;
            public bool ApplyFilter(GamePlayer player)
            {
                if (!player.IsAnonymous && player.Client.Account.Language.ToLower() == m_str)
                    return true;
                return false;
            }

            public LanguageFilter(string language)
            {
                m_str = language;
            }
        }


        private class BGGroupFilter : IWhoFilter
        {
            public bool ApplyFilter(GamePlayer player)
            {
                BattleGroup bg = (BattleGroup)player.TempProperties.getProperty<object>(BattleGroup.BATTLEGROUP_PROPERTY, null);
                //no chatgroup found
                if (bg == null)
                    return false;

                //always show your own bg
                //TODO

                //player is a bg leader, and the bg is public
                if ((bool)bg.Members[player] == true && bg.IsPublic)
                    return true;

                return false;
            }
        }

        private class ChatGroupFilter : IWhoFilter
        {
            public bool ApplyFilter(GamePlayer player)
            {
                ChatGroup cg = (ChatGroup)player.TempProperties.getProperty<object>(ChatGroup.CHATGROUP_PROPERTY, null);
                //no chatgroup found
                if (cg == null)
                    return false;

                //always show your own cg
                //TODO

                //player is a cg leader, and the cg is public
                if ((bool)cg.Members[player] == true && cg.IsPublic)
                    return true;

                return false;
            }
        }

        private class NewFrontiersFilter : IWhoFilter
        {
            public bool ApplyFilter(GamePlayer player)
            {
                return player.CurrentRegionID == 163;
            }
        }

        private class RPFilter : IWhoFilter
        {
            public bool ApplyFilter(GamePlayer player)
            {
                return player.RPFlag;
            }
        }

        private interface IWhoFilter
        {
            bool ApplyFilter(GamePlayer player);
        }
    }
}