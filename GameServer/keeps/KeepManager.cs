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
using DOL.GS.ServerProperties;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DOL.GS.Keeps
{
    /// <summary>
    /// KeepManager
    /// The manager that keeps track of the keeps and stuff.. in the future.
    /// Right now it just has some utilities.
    /// </summary>
    public sealed class KeepMgr
    {
        /// <summary>
        /// list of all keeps
        /// </summary>
        private static readonly Hashtable m_keepList = new Hashtable();

        public static Hashtable Keeps
        {
            get { return m_keepList; }
        }

        private static readonly List<Battleground> m_battlegrounds = new List<Battleground>();

        public const int NEW_FRONTIERS = 163;
        public const int PASSAGE_OF_CONFLICT = 244;

        // private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // public ILog Log
        // {
        //     get { return Logger; }
        // }

        /// <summary>
        /// load all keeps from the DB
        /// </summary>
        /// <returns></returns>
        public static bool Load()
        {
            ClothingMgr.LoadTemplates();

            //Dinberg - moved this here, battlegrounds must be loaded before keepcomponents are.
            LoadBattlegroundCaps();

            if (!ServerProperties.Properties.LOAD_KEEPS)
                return true;

            lock (m_keepList.SyncRoot)
            {
                m_keepList.Clear();

                var keeps = GameServer.Database.SelectAllObjects<DBKeep>();
                foreach (DBKeep datakeep in keeps)
                {
                    Region keepRegion = WorldMgr.GetRegion(datakeep.Region);
                    if (keepRegion == null)
                        continue;


                    //I don't want to touch the loading order of hookpoints, as i think they may depend on the
                    //assumption keeps and towers are linked before population. So we will settle for a second
                    //query. It's on server start, so it wont impact running performance.

                    //var currentKeepComponents = GameServer.Database.SelectObjects<DBKeepComponent>("`KeepID` = '" + datakeep.KeepID + "'");
                   // var currentKeepComponents = GameServer.Database.SelectObjects<DBKeepComponent>("`KeepID` = @KeepID", new QueryParameter("@KeepID", datakeep.KeepID));

                    //Pass through, and depending on the outcome of the components, determine the 'age' of the keep.
                    AbstractGameKeep keep;
                    if ((datakeep.KeepID >> 8) != 0 || ((datakeep.KeepID & 0xFF) > 150))
                    {
                        keep = keepRegion.CreateGameKeepTower();
                    }
                    else
                    {
                        keep = keepRegion.CreateGameKeep();
                    }

                    keep.Load(datakeep);
                    RegisterKeep(datakeep.KeepID, keep);
                }

                // This adds owner keeps to towers / portal keeps
                foreach (AbstractGameKeep keep in m_keepList.Values)
                {
                    GameKeepTower tower = keep as GameKeepTower;
                    if (tower != null)
                    {
                        int index = tower.KeepID & 0xFF;
                        GameKeep ownerKeep = GetKeepByID(index) as GameKeep;
                        if (ownerKeep != null)
                        {
                            ownerKeep.AddTower(tower);
                        }
                        tower.Keep = ownerKeep;
                        tower.OwnerKeepID = index;

                        if (tower.OwnerKeepID < 10)
                        {
                            Logger.WarnFormat("Tower.OwnerKeepID < 10 for KeepID {0}. Doors on this tower will not be targetable! ({0} & 0xFF < 10). Choose a different KeepID to correct this issue.", tower.KeepID);
                        }
                    }
                }
                if (ServerProperties.Properties.USE_NEW_KEEPS == 2) Logger.ErrorFormat("ServerProperty USE_NEW_KEEPS is actually set to 2 but it is no longer used. Loading as if he were 0 but please set to 0 or 1 !");

                var keepcomponents = default(IList<DBKeepComponent>);

                if (ServerProperties.Properties.USE_NEW_KEEPS == 0 || ServerProperties.Properties.USE_NEW_KEEPS == 2)
                    keepcomponents = GameServer.Database.SelectObjects<DBKeepComponent>("`Skin` < @Skin", new QueryParameter("@Skin", 20));
                else if (ServerProperties.Properties.USE_NEW_KEEPS == 1)
                    keepcomponents = GameServer.Database.SelectObjects<DBKeepComponent>("`Skin` > @Skin", new QueryParameter("@Skin", 20));

                foreach (DBKeepComponent component in keepcomponents)
                {
                    AbstractGameKeep keep = GetKeepByID(component.KeepID);
                    if (keep == null)
                    {
                        //missingKeeps = true;
                        continue;
                    }

                    GameKeepComponent gamecomponent = keep.CurrentRegion.CreateGameKeepComponent();
                    gamecomponent.LoadFromDatabase(component, keep);
                    keep.KeepComponents.Add(gamecomponent);
                }

                /*if (missingKeeps && log.IsWarnEnabled)
				{
					log.WarnFormat("Some keeps not found while loading components, possibly old/new keeptypes.");
				}*/

                if (m_keepList.Count != 0)
                {
                    foreach (AbstractGameKeep keep in m_keepList.Values)
                    {
                        if (keep.KeepComponents.Count != 0)
                            keep.KeepComponents.Sort();
                    }
                }
                LoadHookPoints();

                Logger.Info("Loaded " + m_keepList.Count + " keeps successfully");
            }

            if (ServerProperties.Properties.USE_KEEP_BALANCING)
                UpdateBaseLevels();

            if (ServerProperties.Properties.USE_LIVE_KEEP_BONUSES)
                KeepBonusMgr.UpdateCounts();

            return true;
        }


        public static bool IsNewKeepComponent(int skin)
        {
            if (skin > 20)
                return true;

            return false;
        }


        private static void LoadHookPoints()
        {
            if (!ServerProperties.Properties.LOAD_KEEPS || !ServerProperties.Properties.LOAD_HOOKPOINTS)
                return;

            Dictionary<string, List<DBKeepHookPoint>> hookPointList = new Dictionary<string, List<DBKeepHookPoint>>();

            var dbkeepHookPoints = GameServer.Database.SelectAllObjects<DBKeepHookPoint>();
            foreach (DBKeepHookPoint dbhookPoint in dbkeepHookPoints)
            {
                List<DBKeepHookPoint> currentArray;
                string key = dbhookPoint.KeepComponentSkinID + "H:" + dbhookPoint.Height;
                if (!hookPointList.ContainsKey(key))
                    hookPointList.Add(key, currentArray = new List<DBKeepHookPoint>());
                else
                    currentArray = hookPointList[key];
                currentArray.Add(dbhookPoint);
            }
            foreach (AbstractGameKeep keep in m_keepList.Values)
            {
                foreach (GameKeepComponent component in keep.KeepComponents)
                {
                    string key = component.Skin + "H:" + component.Height;
                    if ((hookPointList.ContainsKey(key)))
                    {
                        List<DBKeepHookPoint> HPlist = hookPointList[key];
                        if ((HPlist != null) && (HPlist.Count != 0))
                        {
                            foreach (DBKeepHookPoint dbhookPoint in hookPointList[key])
                            {
                                GameKeepHookPoint myhookPoint = new GameKeepHookPoint(dbhookPoint, component);
                                component.HookPoints.Add(dbhookPoint.HookPointID, myhookPoint);
                            }
                            continue;
                        }
                    }
                    //add this to keep hookpoint system until DB is not full
                    for (int i = 0; i < 38; i++)
                        component.HookPoints.Add(i, new GameKeepHookPoint(i, component));

                    component.HookPoints.Add(65, new GameKeepHookPoint(0x41, component));
                    component.HookPoints.Add(97, new GameKeepHookPoint(0x61, component));
                    component.HookPoints.Add(129, new GameKeepHookPoint(0x81, component));
                }
            }

            Logger.Info("Loading HookPoint items");

            //fill existing hookpoints with objects
            IList<DBKeepHookPointItem> items = GameServer.Database.SelectAllObjects<DBKeepHookPointItem>();
            foreach (AbstractGameKeep keep in m_keepList.Values)
            {
                foreach (GameKeepComponent component in keep.KeepComponents)
                {
                    foreach (GameKeepHookPoint hp in component.HookPoints.Values)
                    {
                        var item = items.FirstOrDefault(
                            it => it.KeepID == component.Keep.KeepID && it.ComponentID == component.ID && it.HookPointID == hp.ID);
                        if (item != null)
                            HookPointItem.Invoke(component.HookPoints[hp.ID] as GameKeepHookPoint, item.ClassType);
                    }
                }
            }
        }

        public static void RegisterKeep(int keepID, AbstractGameKeep keep)
        {
            m_keepList.Add(keepID, keep);
            Logger.Info("Registered Keep: " + keep.Name);

            if (keep.CurrentRegion.ID == 163 && Properties.Guild_Claim_Cost)
                //Logger.ErrorFormat("Redister Timer for Keep Name: {0}", keep.Name);
                keep.BPSClaimTimerStart();//Removeing BPS for Claiming per hour
        }


        /// <summary>
        /// get keep by ID
        /// </summary>
        /// <param name="id">id of keep</param>
        /// <returns> Game keep object with keepid = id</returns>
        public static AbstractGameKeep GetKeepByID(int id)
        {
            return m_keepList[id] as AbstractGameKeep;
        }

        /// <summary>
        /// get list of keep close to spot
        /// </summary>
        /// <param name="regionid"></param>
        /// <param name="point3d"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static IEnumerable GetKeepsCloseToSpot(ushort regionid, IPoint3D point3d, int radius)
        {
            return GetKeepsCloseToSpot(regionid, point3d.X, point3d.Y, point3d.Z, radius);
        }

        /// <summary>
        /// get the keep with minimum distance close to spot
        /// </summary>
        /// <param name="regionid"></param>
        /// <param name="point3d"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static AbstractGameKeep GetKeepCloseToSpot(ushort regionid, IPoint3D point3d, int radius)
        {
            return GetKeepCloseToSpot(regionid, point3d.X, point3d.Y, point3d.Z, radius);
        }


        /// <summary>
		/// Gets all keeps by a realm map /rw
		/// </summary>
		/// <param name="map"></param>
		/// <returns></returns>
		public static ICollection<AbstractGameKeep> getKeepsByRealmMap(int map)
        {
            List<AbstractGameKeep> myKeeps = new List<AbstractGameKeep>();
            SortedList keepsByID = new SortedList();
            foreach (AbstractGameKeep keep in m_keepList.Values)
            {
                if (Properties.Show_Realm == false)
                    continue;

                if (keep.CurrentRegion.ID != NEW_FRONTIERS)
                    continue;

                if (((keep.KeepID & 0xFF) / 25 - 1) == map)
                {
                    keepsByID.Add(keep.KeepID, keep);
                }
                else if (((keep.KeepID & 0xFF) > 150) && ((keep.KeepID & 0xFF) / 25 - 2) == map)
                {
                    keepsByID.Add(keep.KeepID, keep);
                }
            }

            foreach (AbstractGameKeep keep in keepsByID.Values)
                myKeeps.Add(keep);

            return myKeeps;
        }
        /*
        /// <summary>
        /// Gets all keeps by a realm map /rw
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static ICollection<AbstractGameKeep> getKeepsByRealmMap(int map)
		{
			List<AbstractGameKeep> myKeeps = new List<AbstractGameKeep>();
			SortedList keepsByID = new SortedList();
			foreach (AbstractGameKeep keep in m_keepList.Values)
			{
				if (keep.CurrentRegion.ID != NEW_FRONTIERS)
					continue;
				if (((keep.KeepID & 0xFF) / 25 - 1) == map)
					keepsByID.Add(keep.KeepID, keep);
			}
			foreach (AbstractGameKeep keep in keepsByID.Values)
				myKeeps.Add(keep);
			return myKeeps;
		}
        */
        /// <summary>
        /// Get the battleground portal keep for a player
        /// </summary>
        /// <param name="player">The player</param>
        /// <returns>The battleground portal keep as AbstractGameKeep or null</returns>
        public static AbstractGameKeep GetBGPK(GamePlayer player)
        {
            //the temporary keep variable for use in this method
            AbstractGameKeep tempKeep = null;

            //iterate through keeps and find all those which we aren't capped out for
            foreach (AbstractGameKeep keep in m_keepList.Values)
            {
                // find keeps in the battlegrounds that arent portal keeps
                if (keep.Region != NEW_FRONTIERS && !keep.IsPortalKeep) continue;
                Battleground bg = GetBattleground(keep.Region);
                if (bg == null) continue;
                if (player.Level >= bg.MinLevel &&
                    player.Level <= bg.MaxLevel &&
                    (bg.MaxRealmLevel == 0 || player.RealmLevel < bg.MaxRealmLevel))
                    tempKeep = keep;
            }

            //if we haven't found a CK, we're not going to find a PK
            if (tempKeep == null)
                return null;

            //we now use the central keep we found, to find the portal keeps
            foreach (AbstractGameKeep keep in GetKeepsOfRegion((ushort)tempKeep.Region))
            {
                //match the region keeps to a portal keep, and realm
                if (keep.IsPortalKeep && keep.Realm == player.Realm)
                    return keep;
            }

            return null;
        }

        public static ICollection<AbstractGameKeep> GetNFKeeps()
        {
            return GetKeepsOfRegion(NEW_FRONTIERS);
        }

        public static ICollection<AbstractGameKeep> GetKeepsOfRegion(ushort region)
        {
            List<AbstractGameKeep> myKeeps = new List<AbstractGameKeep>();
            foreach (AbstractGameKeep keep in m_keepList.Values)
            {
                if (keep.CurrentRegion.ID != region)
                    continue;
                myKeeps.Add(keep);
            }
            return myKeeps;
        }

        /// <summary>
        ///  get list of keep close to spot
        /// </summary>
        /// <param name="regionid"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static ICollection<AbstractGameKeep> GetKeepsCloseToSpot(ushort regionid, int x, int y, int z, int radius)
        {
            List<AbstractGameKeep> myKeeps = new List<AbstractGameKeep>();
            long radiussqrt = radius * radius;

            lock (m_keepList.SyncRoot)
            {
                foreach (AbstractGameKeep keep in m_keepList.Values)
                {
                    if (keep.DBKeep == null || keep.CurrentRegion.ID != regionid)
                        continue;
                    long xdiff = keep.DBKeep.X - x;
                    long ydiff = keep.DBKeep.Y - y;
                    long range = xdiff * xdiff + ydiff * ydiff;
                    if (range < radiussqrt)
                    {
                        myKeeps.Add(keep);
                    }
                }
            }
            return myKeeps;
        }

        /// <summary>
        /// get the keep with minimum distance close to spot
        /// </summary>
        /// <param name="regionid"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static AbstractGameKeep GetKeepCloseToSpot(ushort regionid, int x, int y, int z, int radius)
        {
            AbstractGameKeep closestKeep = null;

            lock (m_keepList.SyncRoot)
            {
                long radiussqrt = radius * radius;
                long lastKeepDistance = radiussqrt;

                foreach (AbstractGameKeep keep in m_keepList.Values)
                {
                    if (keep == null || keep.DBKeep == null || keep.DBKeep.Region != regionid)
                        continue;

                    long xdiff = keep.DBKeep.X - x;
                    long ydiff = keep.DBKeep.Y - y;
                    long range = xdiff * xdiff + ydiff * ydiff;

                    if (range > radiussqrt)
                        continue;

                    if (closestKeep == null || range <= lastKeepDistance)
                    {
                        closestKeep = keep;
                        lastKeepDistance = range;
                    }
                }
            }

            return closestKeep;
        }

        /// <summary>
        /// get keep count controlled by realm to calculate keep bonus
        /// </summary>
        /// <param name="realm"></param>
        /// <returns></returns>
        public static int GetTowerCountByRealm(eRealm realm)
        {
            int index = 0;
            //lock (m_keepList.SyncRoot)
            {
                foreach (AbstractGameKeep keep in KeepMgr.GetNFKeeps()) //m_keepList.Values)
                {
                    // if (keep.Region != NEW_FRONTIERS) continue;
                    if (keep is GameKeepTower == false) continue;

                    if ((eRealm)keep.Realm == realm)
                        index++;
                }
            }
            return index;
        }

        /// <summary>
        /// Get the tower count of each realm
        /// </summary>
        /// <returns></returns>
        public static Dictionary<eRealm, int> GetTowerCountAllRealm()
        {
            Dictionary<eRealm, int> realmXTower = new Dictionary<eRealm, int>(3)
            {
                { eRealm.Albion, 0 },
                { eRealm.Hibernia, 0 },
                { eRealm.Midgard, 0 }
            };

            lock (m_keepList.SyncRoot)
            {
                foreach (AbstractGameKeep keep in m_keepList.Values)
                {
                    if (keep.Region == NEW_FRONTIERS && keep is GameKeepTower)
                    {
                        realmXTower[keep.Realm] += 1;
                    }
                }
            }

            return realmXTower;
        }

        /// <summary>
        /// Get the tower count of each realm
        /// </summary>
        /// <returns></returns>
        public static Dictionary<eRealm, int> GetTowerCountFromZones(List<int> zones)
        {
            Dictionary<eRealm, int> realmXTower = new Dictionary<eRealm, int>(4)
            {
                { eRealm.Albion, 0 },
                { eRealm.Hibernia, 0 },
                { eRealm.Midgard, 0 },
                { eRealm.None, 0 }
            };

            lock (m_keepList.SyncRoot)
            {
                foreach (AbstractGameKeep keep in m_keepList.Values)
                {
                    if (keep.Region == NEW_FRONTIERS && keep is GameKeepTower && zones.Contains(keep.CurrentZone.ID))
                    {
                        realmXTower[keep.Realm] += 1;
                    }
                }
            }

            return realmXTower;
        }

        /// <summary>
        /// get keep count by realm
        /// </summary>
        /// <param name="realm"></param>
        /// <returns></returns>
        public static int GetKeepCountByRealm(eRealm realm)
        {
            int index = 0;
            lock (m_keepList.SyncRoot)
            {
                foreach (AbstractGameKeep keep in m_keepList.Values)
                {
                    if (keep.Region != NEW_FRONTIERS) continue;
                    if (((eRealm)keep.Realm == realm) && (keep is GameKeep))
                        index++;
                }
            }
            return index;
        }

        public static ICollection<AbstractGameKeep> GetAllKeeps()
        {
            List<AbstractGameKeep> myKeeps = new List<AbstractGameKeep>();
            lock (((ICollection)myKeeps).SyncRoot)//Elcotek Keepliste !!
            {
                foreach (AbstractGameKeep keep in m_keepList.Values)
                {
                    myKeeps.Add(keep);
                }
                return myKeeps;
            }
        }

        /// <summary>
        /// Main checking method to see if a player is an enemy of the keep
        /// </summary>
        /// <param name="keep">The keep checking</param>
        /// <param name="target">The target player</param>
        /// <param name="checkGroup">Do we check the players group for a friend</param>
        /// <returns>true if the player is an enemy of the keep</returns>
        public static bool IsEnemy(AbstractGameKeep keep, GamePlayer target, bool checkGroup)
        {
            if (target.Client.Account.PrivLevel != 1)
                return false;

            switch (GameServer.Instance.Configuration.ServerType)
            {
                case eGameServerType.GST_Normal:
                    {
                        return keep.Realm != target.Realm;
                    }
                case eGameServerType.GST_PvP:
                    {
                        if (keep.Guild == null)
                            return ServerProperties.Properties.PVP_UNCLAIMED_KEEPS_ENEMY;

                        //friendly player in group
                        if (checkGroup && target.Group != null)
                        {
                            foreach (GamePlayer player in target.Group.GetPlayersInTheGroup())
                            {
                                if (!IsEnemy(keep, target, false))
                                    return false;
                            }
                        }

                        //guild alliance
                        if (keep.Guild != null && keep.Guild.alliance != null)
                        {
                            if (keep.Guild.alliance.Guilds.Contains(target.Guild))
                                return false;
                        }

                        return keep.Guild != target.Guild;
                    }
                case eGameServerType.GST_PvE:
                    {
                        return !(target is GamePlayer);
                    }
            }
            return true;
        }

        /// <summary>
        /// Convinience method for checking if a player is an enemy of a keep
        /// This sets checkGroup to true in the main method
        /// </summary>
        /// <param name="keep">The keep checking</param>
        /// <param name="target">The target player</param>
        /// <returns>true if the player is an enemy of the keep</returns>
        public static bool IsEnemy(AbstractGameKeep keep, GamePlayer target)
        {
            return IsEnemy(keep, target, true);
        }

        /// <summary>
        /// Checks if a keep guard is an enemy of the player
        /// </summary>
        /// <param name="checker">The guard checker</param>
        /// <param name="target">The player target</param>
        /// <returns>true if the player is an enemy of the guard</returns>
        public static bool IsEnemy(GameKeepGuard checker, GamePlayer target)
        {
            if (checker.Component == null || checker.Component.Keep == null)
                return GameServer.ServerRules.IsAllowedToAttack(checker, target, true);
            return IsEnemy(checker.Component.Keep, target);
        }
        public static bool IsEnemy(GameKeepGuard checker, GamePlayer target, bool checkGroup)
        {
            if (checker.Component == null || checker.Component.Keep == null)
                return GameServer.ServerRules.IsAllowedToAttack(checker, target, true);
            return IsEnemy(checker.Component.Keep, target, checkGroup);
        }

        /// <summary>
        /// Checks if a keep door is an enemy of the player
        /// </summary>
        /// <param name="checker">The door checker</param>
        /// <param name="target">The player target</param>
        /// <returns>true if the player is an enemy of the door</returns>
        public static bool IsEnemy(GameKeepDoor checker, GamePlayer target)
        {
            return IsEnemy(checker.Component.Keep, target);
        }

        /// <summary>
        /// Checks if a keep component is an enemy of the player
        /// </summary>
        /// <param name="checker">The component checker</param>
        /// <param name="target">The player target</param>
        /// <returns>true if the player is an enemy of the component</returns>
        public static bool IsEnemy(GameKeepComponent checker, GamePlayer target)
        {
            return IsEnemy(checker.Keep, target);
        }

        /// <summary>
        /// Gets a component height from a level
        /// </summary>
        /// <param name="level">The level</param>
        /// <returns>The height</returns>
        public static byte GetHeightFromLevel(AbstractGameKeep keepType, byte level)
        {

            if (ServerProperties.Properties.USE_FIXED_KEEP_HIGHT)
            {
                if (keepType is GameKeepTower && level > 7)
                    return 1;
                else if (keepType is GameKeepTower && level > 4)
                    return 1;
                else if (keepType is GameKeepTower && level > 1)
                    return 0;

                if (keepType is GameKeep && level > 7)
                    return 2;
                else if (keepType is GameKeep && level > 4)
                    return 2;
                else if (keepType is GameKeep && level > 1)
                    return 1;
                else
                    return 0;

            }
            else
            {

                if (keepType is GameKeepTower && level > 7)
                    return 1;
                else if (keepType is GameKeepTower && level > 4)
                    return 1;
                else if (keepType is GameKeepTower && level > 1)
                    return 0;

                if (keepType is GameKeep && level > 7)
                    return 3;
                else if (keepType is GameKeep && level > 4)
                    return 2;
                else if (keepType is GameKeep && level > 1)
                    return 1;
                else
                    return 0;
            }
        }



        public static void GetBorderKeepLocation(int keepid, out int x, out int y, out int z, out ushort heading)
        {
            x = 0;
            y = 0;
            z = 0;
            heading = 0;
            switch (keepid)
            {
                //sauvage
                case 1:
                    {
                        x = 653811;
                        y = 616998;
                        z = 9560;
                        heading = 2040;
                        break;
                    }
                //snowdonia
                case 2:
                    {
                        x = 616149;
                        y = 679042;
                        z = 9560;
                        heading = 1611;
                        break;
                    }
                //svas
                case 3:
                    {
                        x = 651460;
                        y = 313758;
                        z = 9432;
                        heading = 1004;
                        break;
                    }
                //vind
                case 4:
                    {
                        x = 715179;
                        y = 365101;
                        z = 9432;
                        heading = 314;
                        break;
                    }
                //ligen
                case 5:
                    {
                        x = 396519;
                        y = 618017;
                        z = 9838;
                        heading = 2159;
                        break;
                    }
                //cain
                case 6:
                    {
                        x = 432841;
                        y = 680032;
                        z = 9747;
                        heading = 2585;
                        break;
                    }
            }
        }

        public static int GetRealmKeepBonusLevel(eRealm realm)
        {
            int keep = 7 - GetKeepCountByRealm(realm);
            return (int)(keep * ServerProperties.Properties.KEEP_BALANCE_MULTIPLIER);
        }

        public static int GetRealmTowerBonusLevel(eRealm realm)
        {
            int tower = 28 - GetTowerCountByRealm(realm);
            return (int)(tower * ServerProperties.Properties.TOWER_BALANCE_MULTIPLIER);
        }

        public static void UpdateBaseLevels()
        {
            lock (m_keepList.SyncRoot)
            {
                foreach (AbstractGameKeep keep in m_keepList.Values)
                {
                    if (keep.Region != NEW_FRONTIERS)
                        continue;

                    byte newLevel = keep.BaseLevel;

                    if (ServerProperties.Properties.BALANCE_TOWERS_SEPARATE)
                    {
                        if (keep is GameKeepTower)
                            newLevel = (byte)(keep.DBKeep.BaseLevel + KeepMgr.GetRealmTowerBonusLevel((eRealm)keep.Realm));
                        else
                            newLevel = (byte)(keep.DBKeep.BaseLevel + KeepMgr.GetRealmKeepBonusLevel((eRealm)keep.Realm));
                    }
                    else
                    {
                        newLevel = (byte)(keep.DBKeep.BaseLevel + KeepMgr.GetRealmKeepBonusLevel((eRealm)keep.Realm) + KeepMgr.GetRealmTowerBonusLevel((eRealm)keep.Realm));
                    }

                    if (keep.BaseLevel != newLevel)
                    {
                        keep.BaseLevel = newLevel;

                        foreach (GameKeepGuard guard in keep.Guards.Values)
                        {
                            keep.TemplateManager.GetMethod("SetGuardLevel").Invoke(null, new object[] { guard });
                        }
                    }
                }
            }
        }

        private static void LoadBattlegroundCaps()
        {
            m_battlegrounds.AddRange(GameServer.Database.SelectAllObjects<Battleground>());
        }

        public static Battleground GetBattleground(ushort region)
        {
            foreach (Battleground bg in m_battlegrounds)
            {
                if (bg.RegionID == region)
                    return bg;
            }
            return null;
        }


        public static void ExitBattleground(GamePlayer player)
        {
            string location = String.Empty;
            switch (player.Realm)
            {
                case eRealm.Albion: location = "Castle Sauvage"; break;
                case eRealm.Midgard: location = "Svasudheim Faste"; break;
                case eRealm.Hibernia: location = "Druim Ligen"; break;
            }

            if (String.IsNullOrEmpty(location) == false)
            {
                // Teleport t = GameServer.Database.SelectObject<Teleport>("`TeleportID` = '" + location + "'");
                Teleport t = GameServer.Database.SelectObjects<Teleport>("`TeleportID` = @TeleportID", new QueryParameter("@TeleportID", location)).FirstOrDefault();

                if (t != null)
                    player.MoveTo((ushort)t.RegionID, t.X, t.Y, t.Z, (ushort)t.Heading);
            }
        }
        public static void ExitBattleground2(GamePlayer player)
        {
            string location = "";
            switch (player.Realm)
            {
                case eRealm.Albion: location = "Forest Sauvage"; break;
                case eRealm.Midgard: location = "Uppland"; break;
                case eRealm.Hibernia: location = "Cruachan Gorge"; break;
            }

            if (String.IsNullOrEmpty(location) == false)
            {
                //Teleport t = GameServer.Database.SelectObject<Teleport>("`TeleportID` = '" + location + "'");
                Teleport t = GameServer.Database.SelectObjects<Teleport>("`TeleportID` = @TeleportID", new QueryParameter("@TeleportID", location)).FirstOrDefault();
                if (t != null)
                    player.MoveTo((ushort)t.RegionID, t.X, t.Y, t.Z, (ushort)t.Heading);
            }
        }
    }
}
