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
using DOL.GS.PacketHandler;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace DOL.GS
{
    /// <summary>
    /// The GuildMgr holds pointers to all guilds, and pointers
    /// to their members.
    /// </summary>
    public sealed class GuildMgr
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// ArrayList of all guilds in the game
        /// </summary>
        static private readonly HybridDictionary m_guilds = new HybridDictionary();

        /// <summary>
        /// ArrayList of all GuildIDs to GuildNames
        /// </summary>
        static private readonly HybridDictionary m_guildids = new HybridDictionary();

        /// <summary>
        /// Holds all the players combined with their guilds for the social window.
        /// Keys are GuildID and player InternalID
        /// </summary>
        /// <remarks>For each guild stored a dictionary of all players in guikd, unsorted</remarks>
        private static readonly Dictionary<string, Dictionary<string, GuildMemberDisplay>> m_guildXAllMembers = new Dictionary<string, Dictionary<string, GuildMemberDisplay>>();

        /// <summary>
        /// Gets a copy of a dictionary of all guild members of a given guild, indexed on player InternalID
        /// </summary>
        /// <param name="guildID">The guild id for a player</param>
        /// <returns>Copy of a dictionary of all guild members for the guild or null if guild is not found</returns>
        public static Dictionary<string, GuildMemberDisplay> GetAllGuildMembers(string guildID)
        {
            if (m_guildXAllMembers.ContainsKey(guildID))
            {
                Dictionary<string, GuildMemberDisplay> dictionary = new Dictionary<string, GuildMemberDisplay>(m_guildXAllMembers[guildID]);
                return dictionary;
            }

            return null;
        }

        /// <summary>
        /// Add a player to the guild players dictionary list
        /// </summary>
        /// <param name="player">Player to add</param>
        public static void AddPlayerToAllGuildPlayersList(GamePlayer player)
        {
            // if (m_guildXAllMembers.ContainsKey(player.GuildID))
            //{

            if (player.GuildID != null)
            {
                //add player guild ID first to list if not exist.
                if (m_guildXAllMembers.ContainsKey(player.GuildID) == false)
                {
                    IList<DOLCharacters> guildCharacters = GameServer.Database.SelectObjects<DOLCharacters>("`GuildID` = @GuildID", new QueryParameter("@GuildID", player.GuildID));
                    Dictionary<string, GuildMemberDisplay> tempList = new Dictionary<string, GuildMemberDisplay>(guildCharacters.Count);
                    foreach (var (ch, member) in from DOLCharacters ch in guildCharacters
                                                 let member = new GuildMemberDisplay(ch.ObjectId,
                                             ch.Name,
                                             ch.Level.ToString(),
                                             ch.Class.ToString(),
                                             ch.GuildRank.ToString(),
                                             "0",
                                             ch.LastPlayed.ToShortDateString(),
                                             ch.GuildNote)
                                                 select (ch, member))
                    {
                        tempList.Add(ch.ObjectId, member);
                    }

                    m_guildXAllMembers.Add(player.GuildID, tempList);
                }
                if (!m_guildXAllMembers[player.GuildID].ContainsKey(player.InternalID))
                {
                    Dictionary<string, GuildMemberDisplay> guildMemberList = m_guildXAllMembers[player.GuildID];
                    GuildMemberDisplay member = new GuildMemberDisplay(player.InternalID,
                                                                        player.Name,
                                                                        player.Level.ToString(),
                                                                        player.CharacterClass.ID.ToString(),
                                                                        player.GuildRank.RankLevel.ToString(),
                                                                        player.Group != null ? player.Group.MemberCount.ToString() : "1",
                                                                        player.CurrentZone.Description,
                                                                        player.GuildNote);
                    guildMemberList.Add(player.InternalID, member);
                }
            }
        }

        /// <summary>
        /// Remove a player from the all guilds and players dictionary
        /// </summary>
        /// <param name="player">Player to remove</param>
        /// <returns>True if player was removed, else false.</returns>
        public static bool RemovePlayerFromAllGuildPlayersList(GamePlayer player)
        {
            if (m_guildXAllMembers.ContainsKey(player.GuildID))
            {
                return m_guildXAllMembers[player.GuildID].Remove(player.InternalID);
            }
            return false;
        }

        /// <summary>
        /// Remove a player from the all guilds and players dictionary by Player internalID
        /// </summary>
        /// <param name="player">Player to remove</param>
        /// <returns>True if player was removed, else false.</returns>
        public static bool RemovePlayerFromAllGuildPlayersListBYID(GamePlayer PlayerGuild, string PlayerID)
        {
            if (m_guildXAllMembers.ContainsKey(PlayerGuild.GuildID))
            {
                return m_guildXAllMembers[PlayerGuild.GuildID].Remove(PlayerID);
            }
            return false;
        }

        static private ushort m_lastID = 0;

        /// <summary>
        /// The cost in copper to reemblem the guild
        /// </summary>
        public const long COST_RE_EMBLEM = 1000000; //200 gold

        /// <summary>
        /// Adds a guild to the list of guilds
        /// </summary>
        /// <param name="guild">The guild to add</param>
        /// <returns>True if the function succeeded, otherwise false</returns>
        public static bool AddGuild(Guild guild)
        {
            if (guild == null)
                return false;

            lock (m_guilds.SyncRoot)
            {
                if (!m_guilds.Contains(guild.Name))
                {
                    m_guilds.Add(guild.Name, guild);
                    m_guildids.Add(guild.GuildID, guild.Name);
                    guild.ID = ++m_lastID;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes a guild from the manager
        /// </summary>
        /// <param name="guild">the guild</param>
        /// <returns></returns>
        public static bool RemoveGuild(Guild guild)
        {
            if (guild == null)
            {
                return false;
            }

            guild.ClearOnlineMemberList();
            lock (m_guilds.SyncRoot)
            {
                m_guilds.Remove(guild.Name);
                m_guildids.Remove(guild.GuildID);
            }
            return true;
        }

        /// <summary>
        /// Checks if a guild with guildName exists
        /// </summary>
        /// <param name="guildName">The guild to check</param>
        /// <returns>true or false</returns>
        public static bool DoesGuildExist(string guildName)
        {
            lock (m_guilds.SyncRoot)
            {
                return m_guilds.Contains(guildName);
            }
        }


        public static Guild CreateGuild(eRealm realm, string guildName, GamePlayer creator = null)
        {
            if (DoesGuildExist(guildName))
            {
                if (creator != null)
                {
                    creator.Out.SendMessage(guildName + " already exists!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }

                return null;
            }

            try
            {
                DBGuild dbGuild = new DBGuild
                {
                    GuildName = guildName,
                    GuildID = System.Guid.NewGuid().ToString(),
                    Realm = (byte)realm
                };
                Guild newguild = new Guild(dbGuild);
                if (newguild.AddToDatabase() == false)
                {
                    if (creator != null)
                    {
                        creator.Out.SendMessage("Database error, unable to add a new guild!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                    return null;
                }
                AddGuild(newguild);
                CreateRanks(newguild);

                if (log.IsDebugEnabled)
                    log.Debug("Create guild; guild name=\"" + guildName + "\" Realm=" + GlobalConstants.RealmToName(newguild.Realm));

                return newguild;
            }
            catch (Exception e)
            {
                if (log.IsErrorEnabled) log.Error("CreateGuild", e);
                return null;
            }
        }

        public static void CreateRanks(Guild guild)
        {
            DBRank rank;
            for (int i = 0; i < 10; i++)
            {
                rank = CreateRank(guild, i);

                GameServer.Database.AddObject(rank);
                guild.Ranks[i] = rank;
            }
        }

        public static void RepairRanks(Guild guild)
        {
            DBRank rank;
            for (int i = 0; i < 10; i++)
            {
                bool foundRank = false;

                for (int i1 = 0; i1 < guild.Ranks.Length; i1++)
                {
                    DBRank r = guild.Ranks[i1];
                    if (r == null)
                    {
                        // I love DOLDB relations!
                        break;
                    }

                    if (r.RankLevel == i)
                    {
                        foundRank = true;
                        break;
                    }
                }

                if (foundRank == false)
                {
                    rank = CreateRank(guild, i);
                    rank.Title = rank.Title.Replace("Rank", "Repaired Rank");
                    GameServer.Database.AddObject(rank);
                }
            }
        }

        private static DBRank CreateRank(Guild guild, int rankLevel)
        {
            DBRank rank = new DBRank
            {
                AcHear = false,
                AcSpeak = false,
                Alli = false,
                Claim = false,
                Emblem = false,
                GcHear = true,
                GcSpeak = false,
                GuildID = guild.GuildID,
                Invite = false,
                OcHear = false,
                OcSpeak = false,
                Promote = false,
                RankLevel = (byte)rankLevel,
                Release = false,
                Remove = false,
                Title = "Rank " + rankLevel.ToString(),
                Upgrade = false,
                View = false
            };
            rank.View = false;
            rank.Dues = false;

            if (rankLevel < 9)
            {
                rank.GcSpeak = true;
                rank.View = true;
                if (rankLevel < 8)
                {
                    rank.Emblem = true;
                    if (rankLevel < 7)
                    {
                        rank.AcHear = true;
                        if (rankLevel < 6)
                        {
                            rank.AcSpeak = true;
                            if (rankLevel < 5)
                            {
                                rank.OcHear = true;
                                if (rankLevel < 4)
                                {
                                    rank.OcSpeak = true;
                                    if (rankLevel < 3)
                                    {
                                        rank.Invite = true;
                                        rank.Promote = true;

                                        if (rankLevel < 2)
                                        {
                                            rank.Release = true;
                                            rank.Upgrade = true;
                                            rank.Claim = true;
                                            if (rankLevel < 1)
                                            {
                                                rank.Remove = true;
                                                rank.Alli = true;
                                                rank.Dues = true;
                                                rank.Withdraw = true;
                                                rank.Title = "Guildmaster";
                                                rank.Buff = true;
                                            }

                                        }

                                    }

                                }

                            }

                        }

                    }

                }

            }

            return rank;
        }

        /// <summary>
        /// Deletes a guild
        /// </summary>
        public static bool DeleteGuild(string guildName)
        {
            try
            {
                Guild removeGuild = GetGuildByName(guildName);
                // Does guild exist, if not return false.
                if (removeGuild == null)
                {
                    return false;
                }

                IList<DBGuild> guilds = GameServer.Database.SelectObjects<DBGuild>("`GuildName` = @GuildName", new QueryParameter("@GuildName", GameServer.Database.Escape(guildName)));

                //var guilds = GameServer.Database.SelectObjects<DBGuild>("GuildName='" + GameServer.Database.Escape(guildName) + "'");
                for (int i = 0; i < guilds.Count; i++)
                {// GameServer.Database.SelectObjects<DOLCharacters>("`GuildID` = @GuildID", new QueryParameter("@GuildID", GameServer.Database.Escape(guild.GuildID)));
                    DBGuild guild = guilds[i];
                    IList<DBGuild> list = GameServer.Database.SelectObjects<DBGuild>("`GuildID` = @GuildID", new QueryParameter("@GuildID", GameServer.Database.Escape(guild.GuildID)));
                    for (int i1 = 0; i1 < list.Count; i1++)
                    {
                        DBGuild cha = list[i1];
                        cha.GuildID = string.Empty;
                    }

                    GameServer.Database.DeleteObject(guild);
                }

                //[StephenxPimentel] We need to delete the guild specific ranks aswell!
                //GameServer.Database.SelectObjects<DBRank>("`GuildID` = '" + removeGuild.ID + "'");
                IList<DBRank> ranks = GameServer.Database.SelectObjects<DBRank>("`GuildID` = @GuildID", new QueryParameter("@GuildID", GameServer.Database.Escape(removeGuild.ID.ToString())));
                for (int i = 0; i < ranks.Count; i++) //GameServer.Database.SelectObjects<SalvageYield>("`GuildID` = @GuildID", new QueryParameter("@GuildID", removeGuild.ID)).FirstOrDefault();
                {
                    DBRank guildRank = ranks[i];
                    GameServer.Database.DeleteObject(guildRank);
                }


                lock (removeGuild.GetListOfOnlineMembers())
                {
                    IList<GamePlayer> list = removeGuild.GetListOfOnlineMembers();
                    for (int i = 0; i < list.Count; i++)
                    {
                        GamePlayer ply = list[i];
                        ply.Guild = null;
                        ply.GuildID = String.Empty;
                        ply.GuildName = String.Empty;
                        ply.GuildRank = null;
                    }
                }

                RemoveGuild(removeGuild);

                return true;
            }
            catch (Exception e)
            {
                if (log.IsErrorEnabled)
                    log.Error("DeleteGuild", e);
                return false;
            }
        }

        /// <summary>
        /// Returns a guild according to the matching name
        /// </summary>
        /// <returns>Guild</returns>
        public static Guild GetGuildByName(string guildName)
        {
            if (guildName == null)
            {
                return null;
            }

            lock (m_guilds.SyncRoot)
            {
                return (Guild)m_guilds[guildName];
            }
        }

        /// <summary>
        /// Returns a guild according to the matching database ID.
        /// </summary>
        /// <returns>Guild</returns>
        public static Guild GetGuildByGuildID(string guildid)
        {
            if (guildid == null)
            {
                return null;
            }

            lock (m_guildids.SyncRoot)
            {
                if (m_guildids[guildid] == null)
                {
                    return null;
                }

                lock (m_guilds.SyncRoot)
                {
                    return (Guild)m_guilds[m_guildids[guildid]];
                }
            }
        }

        /// <summary>
        /// Returns a database ID for a matching guild name.
        /// </summary>
        /// <returns>Guild</returns>
        public static string GuildNameToGuildID(string guildName)
        {
            Guild g = GetGuildByName(guildName);
            if (g == null)
                return "";
            return g.GuildID;
        }

        /// <summary>
        /// Load all guilds and alliances from the database
        /// </summary>
        public static bool LoadAllGuilds()
        {
            lock (m_guilds.SyncRoot)
            {
                m_guilds.Clear(); //clear guild list before loading!
            }
            m_lastID = 0;

            //load guilds
            IList<DBGuild> guildObjs = GameServer.Database.SelectAllObjects<DBGuild>();
            for (int i = 0; i < guildObjs.Count; i++)
            {
                DBGuild obj = guildObjs[i];
                var myguild = new Guild(obj);

                if (obj.Ranks == null ||
                    obj.Ranks.Length < 10 ||
                    obj.Ranks[0] == null ||
                    obj.Ranks[1] == null ||
                    obj.Ranks[2] == null ||
                    obj.Ranks[3] == null ||
                    obj.Ranks[4] == null ||
                    obj.Ranks[5] == null ||
                    obj.Ranks[6] == null ||
                    obj.Ranks[7] == null ||
                    obj.Ranks[8] == null ||
                    obj.Ranks[9] == null)
                {
                    log.ErrorFormat("GuildMgr: Ranks missing for {0}, creating new ones!", myguild.Name);

                    RepairRanks(myguild);

                    // now reload the guild to fix the relations
                    // myguild = new Guild(GameServer.Database.SelectObject<DBGuild>("GuildID = '" + obj.GuildID + "'"));
                    myguild = new Guild(GameServer.Database.SelectObjects<DBGuild>("`GuildID` = @GuildID", new QueryParameter("@GuildID", obj.GuildID)).FirstOrDefault());
                }

                AddGuild(myguild);

                // var guildCharacters = GameServer.Database.SelectObjects<DOLCharacters>(string.Format("GuildID = '" + GameServer.Database.Escape(myguild.GuildID) + "'"));

                IList<DOLCharacters> guildCharacters = GameServer.Database.SelectObjects<DOLCharacters>("`GuildID` = @GuildID", new QueryParameter("@GuildID", myguild.GuildID));
                Dictionary<string, GuildMemberDisplay> tempList = new Dictionary<string, GuildMemberDisplay>(guildCharacters.Count);
                foreach (var (ch, member) in from DOLCharacters ch in guildCharacters
                                             let member = new GuildMemberDisplay(ch.ObjectId,
                                     ch.Name,
                                     ch.Level.ToString(),
                                     ch.Class.ToString(),
                                     ch.GuildRank.ToString(),
                                     "0",
                                     ch.LastPlayed.ToShortDateString(),
                                     ch.GuildNote)
                                             select (ch, member))
                {
                    tempList.Add(ch.ObjectId, member);
                }

                m_guildXAllMembers.Add(myguild.GuildID, tempList);
            }

            //load alliances
            IList<DBAlliance> allianceObjs = GameServer.Database.SelectAllObjects<DBAlliance>();
            foreach (var (dball, myalliance) in from DBAlliance dball in allianceObjs
                                                let myalliance = new Alliance()
                                                select (dball, myalliance))
            {
                myalliance.LoadFromDatabase(dball);
                if (dball != null && dball.DBguilds != null)
                {
                    foreach (DBGuild mydbgui in dball.DBguilds)
                    {
                        var gui = GetGuildByName(mydbgui.GuildName);
                        myalliance.Guilds.Add(gui);
                        gui.alliance = myalliance;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Save all guild into database
        /// </summary>
        public static void SaveAllGuilds()
        {
            if (log.IsDebugEnabled)
                log.Debug("Saving all guilds...");
            try
            {
                lock (m_guilds.SyncRoot)
                {
                    foreach (Guild g in m_guilds.Values)
                    {
                        g.SaveIntoDatabase();
                    }
                }
            }
            catch (Exception e)
            {
                if (log.IsErrorEnabled)
                    log.Error("Error saving guilds.", e);
            }
        }

        /// <summary>
        /// Returns true if a guild is using the emblem
        /// </summary>
        /// <param name="emblem"></param>
        /// <returns></returns>
        public static bool IsEmblemUsed(int emblem)
        {
            lock (m_guilds.SyncRoot)
            {
                foreach (var _ in from Guild guild in m_guilds.Values
                                  where guild.Emblem == emblem
                                  select new { })
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Process for changing an emblem
        /// </summary>
        /// <param name="player"></param>
        /// <param name="oldemblem"></param>
        /// <param name="newemblem"></param>
        public static void ChangeEmblem(GamePlayer player, int oldemblem, int newemblem)
        {
            player.Guild.Emblem = newemblem;
            if (oldemblem != 0)
            {
                player.RemoveMoney(COST_RE_EMBLEM, null);
                //InventoryLogging.LogInventoryAction(player, "(GUILD;" + player.GuildName + ")", eInventoryActionType.Other, COST_RE_EMBLEM);
                IList<InventoryItem> objs = GameServer.Database.SelectObjects<InventoryItem>("`Emblem` = @Emblem", new QueryParameter("@Emblem", GameServer.Database.Escape(oldemblem.ToString())));
                //GameServer.Database.SelectObjects<InventoryItem>("Emblem = " + GameServer.Database.Escape(oldemblem.ToString()));

                for (int i = 0; i < objs.Count; i++)
                {
                    InventoryItem item = objs[i];
                    item.Emblem = newemblem;
                    GameServer.Database.SaveObject(item);
                }

                // change guild house emblem

                if (player.Guild.GuildOwnsHouse && player.Guild.GuildHouseNumber > 0)
                {
                    Housing.House guildHouse = Housing.HouseMgr.GetHouse(player.Guild.GuildHouseNumber);

                    if (guildHouse != null)
                    {
                        guildHouse.Emblem = player.Guild.Emblem;
                        guildHouse.SaveIntoDatabase();
                        guildHouse.SendUpdate();
                    }
                }
            }
        }

        /// <summary>
        /// Get a list of all guilds (creates a copy)
        /// </summary>
        /// <returns></returns>
        public static List<Guild> AllGuilds
        {
            get
            {
                List<Guild> guilds = new List<Guild>(m_guilds.Count);

                lock (m_guilds.SyncRoot)
                {
                    guilds.AddRange(from Guild guild in m_guilds.Values
                                    select guild);
                }

                return guilds;
            }
        }

        /// <summary>
        /// This class represents a guild member for the purpose of displaying in the social window
        /// </summary>
        public class GuildMemberDisplay
        {
            #region Members

            readonly string m_internalID;
            public string InternalID
            {
                get { return m_internalID; }
            }

            readonly string m_name;
            public string Name
            {
                get { return m_name; }
            }

            string m_level;
            public string Level
            {
                get { return m_level; }
                set { m_level = value; }
            }

            string m_characterClassID;
            public string ClassID
            {
                get { return m_characterClassID; }
                set { m_characterClassID = value; }
            }

            string m_rank;
            public string Rank
            {
                get { return m_rank; }
                set { m_rank = value; }
            }

            string m_groupSize = "0";
            public string GroupSize
            {
                get { return m_groupSize; }
                set { m_groupSize = value; }
            }

            string m_zoneOnline;
            public string ZoneOrOnline
            {
                get { return m_zoneOnline; }
                set { m_zoneOnline = value; }
            }

            string m_guildNote = "";
            public string Note
            {
                get { return m_guildNote; }
                set { m_guildNote = value; }
            }

            #endregion

            public string this[eSocialWindowSortColumn i]
            {
                get
                {
                    switch (i)
                    {
                        case eSocialWindowSortColumn.Name:
                            return Name;
                        case eSocialWindowSortColumn.ClassID:
                            return ClassID;
                        case eSocialWindowSortColumn.Group:
                            return GroupSize;
                        case eSocialWindowSortColumn.Level:
                            return Level;
                        case eSocialWindowSortColumn.Note:
                            return Note;
                        case eSocialWindowSortColumn.Rank:
                            return Rank;
                        case eSocialWindowSortColumn.ZoneOrOnline:
                            return ZoneOrOnline;
                        default:
                            return "";
                    }
                }
            }

            public GuildMemberDisplay(string internalID, string name, string level, string classID, string rank, string group, string zoneOrOnline, string note)
            {
                m_internalID = internalID;
                m_name = name;
                m_level = level;
                m_characterClassID = classID;
                m_rank = rank;
                m_groupSize = group;
                m_zoneOnline = zoneOrOnline;
                m_guildNote = note;
            }

            public GuildMemberDisplay(GamePlayer player)
            {
                m_internalID = player.InternalID;
                m_name = player.DBCharacter.Name;
                m_level = player.Level.ToString();
                m_characterClassID = player.CharacterClass.ID.ToString();
                m_rank = player.GuildRank.RankLevel.ToString(); ;
                m_groupSize = player.Group == null ? "1" : "2";
                m_zoneOnline = player.CurrentZone.ToString();
                m_guildNote = player.GuildNote;
            }

            /// <summary>
            /// This is used to send the correct information to the client social window
            /// </summary>
            /// <param name="position"></param>
            /// <param name="guildPop"></param>
            /// <returns></returns>
            public string ToString(int position, int guildPop)
            {
                return string.Format("E,{0},{1},{2},{3},{4},{5},{6},\"{7}\",\"{8}\"",
                                     position, guildPop, m_name, m_level, m_characterClassID, m_rank, m_groupSize, m_zoneOnline, m_guildNote);
            }

            public void UpdateMember(GamePlayer player)
            {
                Level = player.Level.ToString();
                ClassID = player.CharacterClass.ID.ToString();
                Rank = player.GuildRank.RankLevel.ToString();
                GroupSize = player.Group == null ? "1" : "2";
                Note = player.GuildNote;
                ZoneOrOnline = player.CurrentZone.Description;
            }

            public enum eSocialWindowSort : int
            {
                NameDesc = -1,
                NameAsc = 1,
                LevelDesc = -2,
                LevelAsc = 2,
                ClassDesc = -3,
                ClassAsc = 3,
                RankDesc = -4,
                RankAsc = 4,
                GroupDesc = -5,
                GroupAsc = 5,
                ZoneOrOnlineDesc = 6,
                ZoneOrOnlineAsc = -6,
                NoteDesc = 7,
                NoteAsc = -7
            }

            public enum eSocialWindowSortColumn : int
            {
                Name = 0,
                Level = 1,
                ClassID = 2,
                Rank = 3,
                Group = 4,
                ZoneOrOnline = 5,
                Note = 6
            }
        }
    }
}
