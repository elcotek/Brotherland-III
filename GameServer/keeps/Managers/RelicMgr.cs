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
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace DOL.GS
{
    /// <summary>
    /// RelicManager
    /// The manager that keeps track of the relics.
    /// </summary>
    public sealed class RelicMgr
    {
        /// <summary>
        /// table of all relics, id as key
        /// </summary>
        private static readonly Dictionary<int, GameRelic> m_relics = new Dictionary<int, GameRelic>();


        /// <summary>
        /// list of all relicPads
        /// </summary>
        private static readonly List<GameRelicPad> m_relicPads = new List<GameRelicPad>();


        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// load all relics from DB
        /// </summary>
        /// <returns></returns>
        public static bool Init()
        {
            lock (((ICollection)m_relics).SyncRoot)
            {
                //at first remove all relics
                foreach (GameRelic rel in m_relics.Values)
                {
                    rel.SaveIntoDatabase();
                    rel.RemoveFromWorld();
                }

                //then clear the hashtable
                m_relics.Clear();

                //then we remove all relics from the pads
                foreach (GameRelicPad pad in m_relicPads)
                {
                    pad.RemoveRelic();
                }

                // if relics are on the ground during init we will return them to their owners
                List<GameRelic> lostRelics = new List<GameRelic>();

                IList<DBRelic> relics = GameServer.Database.SelectAllObjects<DBRelic>();
                foreach (DBRelic datarelic in relics)
                {
                    if (datarelic.relicType < 0 || datarelic.relicType > 1
                        || datarelic.OriginalRealm < 1 || datarelic.OriginalRealm > 3)
                    {
                        log.Warn("DBRelic: Could not load " + datarelic.RelicID + ": Realm or Type missmatch.");
                        continue;
                    }

                    if (WorldMgr.GetRegion((ushort)datarelic.Region) == null)
                    {
                        log.Warn("DBRelic: Could not load " + datarelic.RelicID + ": Region missmatch.");
                        continue;
                    }
                    GameRelic relic = new GameRelic(datarelic);
                    m_relics.Add(datarelic.RelicID, relic);

                    relic.AddToWorld();
                    GameRelicPad pad = GetPadAtRelicLocation(relic);
                    if (pad != null)
                    {
                        //log.ErrorFormat("add type name: {0}", pad.PadType);
                        if (relic.RelicType == pad.PadType)
                        {
                            //log.ErrorFormat("add pad name: {0}", pad.Name);
                            relic.RelicPadTakesOver(pad, true);
                            log.Debug("DBRelic: " + relic.Name + " has been loaded and added to pad " + pad.Name + ".");
                        }
                    }
                    else
                    {
                        lostRelics.Add(relic);
                    }
                }

                foreach (GameRelic lostRelic in lostRelics)
                {
                    eRealm returnRealm = (eRealm)lostRelic.LastRealm;

                    if (returnRealm == eRealm.None)
                    {
                        returnRealm = lostRelic.OriginalRealm;
                    }

                    // ok, now we have a realm to return the relic too, lets find a pad

                    foreach (GameRelicPad pad in m_relicPads)
                    {
                        if (pad.MountedRelic == null && pad.Realm == returnRealm && pad.PadType == lostRelic.RelicType)
                        {
                            lostRelic.RelicPadTakesOver(pad, true);
                            log.Debug("Lost Relic: " + lostRelic.Name + " has returned to last pad: " + pad.Name + ".");
                        }
                    }
                }

                // Final cleanup.  If any relic is still unmounted then mount the damn thing to any empty pad

                foreach (GameRelic lostRelic in lostRelics)
                {
                    if (lostRelic.CurrentRelicPad == null)
                    {
                        foreach (GameRelicPad pad in m_relicPads)
                        {
                            if (pad.MountedRelic == null && pad.PadType == lostRelic.RelicType)
                            {
                                lostRelic.RelicPadTakesOver(pad, true);
                                log.Debug("Lost Relic: " + lostRelic.Name + " auto assigned to pad: " + pad.Name + ".");
                            }
                        }
                    }
                }
            }

            log.Debug(m_relicPads.Count + " relicpads" + ((m_relicPads.Count > 1) ? "s were" : " was") + " loaded.");
            log.Debug(m_relics.Count + " relic" + ((m_relics.Count > 1) ? "s were" : " was") + " loaded.");
            return true;
        }


        /// <summary>
        /// This is called when the GameRelicPads are added to world
        /// </summary>
        /// <param name="pad"></param>
        public static void AddRelicPad(GameRelicPad pad)
        {
            lock (((ICollection)m_relicPads).SyncRoot)
            {
                if (!m_relicPads.Contains(pad))
                    m_relicPads.Add(pad);
            }
        }

        /// <summary>
        /// This is called on during the loading. It looks for relicpads and where it could be stored.
        /// </summary>
        /// <returns>null if no GameRelicPad was found at the relic's position.</returns>
        public static GameRelicPad GetPadAtRelicLocation(GameRelic relic)
        {

            lock (((ICollection)m_relicPads).SyncRoot)
            {
                foreach (GameRelicPad pad in m_relicPads)
                {
                    if (relic.IsWithinRadius(pad, 200))
                        //if (pad.X == relic.X && pad.Y == relic.Y && pad.Z == relic.Z && pad.CurrentRegionID == relic.CurrentRegionID)
                        return pad;
                }
                return null;
            }

        }


        /// <summary>
        /// get relic by ID
        /// </summary>
        /// <param name="id">id of relic</param>
        /// <returns> Relic object with relicid = id</returns>
        public static GameRelic GetRelic(int id)
        {
            if (m_relics.ContainsKey(id))
                return m_relics[id] as GameRelic;

            return null;
        }





        #region Helpers

        public static IList GetNFRelics()
        {
            return new List<GameRelic>(m_relics.Values);
        }

        /// <summary>
        /// Returns an enumeration with all mounted Relics of an realm
        /// </summary>
        /// <param name="Realm"></param>
        /// <returns></returns>
        public static IEnumerable GetRelics(eRealm Realm)
        {
            List<GameRelic> realmRelics = new List<GameRelic>();
            lock (((ICollection)m_relics).SyncRoot)
            {
                foreach (GameRelic relic in m_relics.Values)
                {
                    if (relic.Realm == Realm && relic.IsMounted)
                        realmRelics.Add(relic);
                }
            }
            return realmRelics;
        }


        /// <summary>
        /// Returns an enumeration with all mounted Relics of an realm by a specified RelicType
        /// </summary>
        /// <param name="Realm"></param>
        /// <param name="RelicType"></param>
        /// <returns></returns>
        public static IEnumerable GetRelics(eRealm Realm, eRelicType RelicType)
        {
            List<GameRelic> realmTypeRelics = new List<GameRelic>();
            foreach (GameRelic relic in GetRelics(Realm))
            {
                if (relic.RelicType == RelicType)
                    realmTypeRelics.Add(relic);
            }
            return realmTypeRelics;
        }



        /// <summary>
        /// get relic count by realm
        /// </summary>
        /// <param name="realm"></param>
        /// <returns></returns>
        public static int GetRelicCount(eRealm realm)
        {
            int index = 0;
            lock (((ICollection)m_relics).SyncRoot)
            {
                foreach (GameRelic relic in m_relics.Values)
                {
                    if ((relic.Realm == realm) && (relic is GameRelic))
                        index++;
                }
            }
            return index;
        }

        /// <summary>
        /// get relic count by realm and relictype
        /// </summary>
        /// <param name="realm"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int GetRelicCount(eRealm realm, eRelicType type)
        {
            int index = 0;
            lock (((ICollection)m_relics).SyncRoot)
            {
                foreach (GameRelic relic in m_relics.Values)
                {
                    if ((relic.Realm == realm) && (relic.RelicType == type) && (relic is GameRelic))
                        index++;
                }
            }
            return index;

        }

        public static double GetRelicBonusModifier(eRealm realm, eRelicType type)
        {
            double bonus = 0.0;
            bool owningSelf = false;
            //only playerrealms can get bonus
            foreach (GameRelic rel in GetRelics(realm, type))
            {
                if (rel.Realm == rel.OriginalRealm)
                {
                    owningSelf = true;
                }
                else
                {
                    bonus += ServerProperties.Properties.RELIC_OWNING_BONUS * 0.01;
                }
            }

            // Bonus apply only if owning original relic
            if (owningSelf)
                return bonus;

            return 0.0;
        }




        /// <summary>
        /// Returns if a player is allowed to pick up a mounted relic (depends if they own their own relic of the same type)
        /// </summary>
        /// <param name="player"></param>
        /// <param name="relic"></param>
        /// <returns></returns>
        public static bool CanPickupRelicFromShrine(GamePlayer player, GameRelic relic)
        {
            //debug: if (player == null || relic == null) return false;
            //their own relics can always be picked up.
            if (player.Realm == relic.OriginalRealm)
                return true;
            IEnumerable list = GetRelics(player.Realm, relic.RelicType);
            foreach (GameRelic curRelic in list)
            {
                if (curRelic.Realm == curRelic.OriginalRealm)
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Gets a copy of the current relics table, keyvalue is the relicId
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, GameRelic> GetAllRelics()
        {
            lock (((ICollection)m_relics).SyncRoot)
            {
                return m_relics;
            }
        }
        #endregion



    }
}
