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
using DOL.GS.ServerProperties;
using DOL.Language;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace DOL.GS.Housing
{
    public class HouseMgr
    {
        public static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected static Timer CheckRentTimer = null;
        protected static Dictionary<ushort, Dictionary<int, House>> _houseList;
        protected static Dictionary<ushort, int> _idList;

        protected enum eLotSpawnType
        {
            Marker,
            House
        }


        public static bool Start(GameClient client = null)
        {
            // load hookpoint offsets
            House.LoadHookpointOffsets();

            // initialize the house template manager
            HouseTemplateMgr.Initialize();

            _houseList = new Dictionary<ushort, Dictionary<int, House>>();
            _idList = new Dictionary<ushort, int>();

            int regions = 0;
            foreach (var reg in from RegionEntry entry in WorldMgr.GetRegionList()
                                let reg = WorldMgr.GetRegion(entry.id)
                                where reg != null && reg.UseHousingManager
                                select reg)
            {
                if (!_houseList.ContainsKey(reg.ID))
                {
                    _houseList.Add(reg.ID, new Dictionary<int, House>());
                }

                if (!_idList.ContainsKey(reg.ID))
                {
                    _idList.Add(reg.ID, 0);
                }

                regions++;
            }

            int houses = 0;
            int lotmarkers = 0;

            foreach (DBHouse house in GameServer.Database.SelectAllObjects<DBHouse>())
            {

                _houseList.TryGetValue(house.RegionID, out Dictionary<int, House> housesForRegion);

                // if we don't have the given region loaded as a housing zone, skip this house
                if (housesForRegion == null)
                {
                    continue;
                }

                // if we already loaded this house, that's no bueno, but just skip
                if (housesForRegion.ContainsKey(house.HouseNumber))
                {
                    continue;
                }

                if (SpawnLot(house, housesForRegion) == eLotSpawnType.House)
                {
                    houses++;
                }
                else
                {
                    lotmarkers++;
                }
            }

            if (log.IsInfoEnabled)
                log.Info("[Housing] Loaded " + houses + " houses and " + lotmarkers + " lotmarkers in " + regions + " regions!");

            if (client != null)
            {
                client.Out.SendMessage("Loaded " + houses + " houses and " + lotmarkers + " lotmarkers in " + regions + " regions!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }

            if (CheckRentTimer != null)
            {
                CheckRentTimer.Change(Properties.RENT_CHECK_INTERVAL * 60 * 1000, Properties.RENT_CHECK_INTERVAL * 60 * 1000);
            }
            else
            {

                CheckRentTimer = new Timer(CheckRents, null, Properties.RENT_CHECK_INTERVAL * 60 * 1000, Properties.RENT_CHECK_INTERVAL * 60 * 1000);
            }

            return true;
        }

        /// <summary>
        /// Force loading of any defined housing and house markers for a specific region
        /// </summary>
        /// <param name="regionID"></param>
        /// <returns></returns>
        public static string LoadHousingForRegion(ushort regionID)
        {
            IList<DBHouse> regionHousing = GameServer.Database.SelectObjects<DBHouse>("`RegionID` = @RegionID", new QueryParameter("@RegionID", regionID));

            if (regionHousing == null || regionHousing.Count == 0)
            {
                return "No housing found for region.";
            }

            int houses = 0;
            int lotmarkers = 0;

            // re-initialize lists for this region

            if (!_houseList.ContainsKey(regionID))
            {
                _houseList.Add(regionID, new Dictionary<int, House>());
            }
            else
            {
                _houseList[regionID] = new Dictionary<int, House>();
            }

            if (!_idList.ContainsKey(regionID))
            {
                _idList.Add(regionID, 0);
            }
            else
            {
                _idList[regionID] = 0;
            }

            _houseList.TryGetValue(regionID, out Dictionary<int, House> housesForRegion);

            if (housesForRegion == null)
            {
                string result = "LoadHousingForRegion: No dictionary defined for region ID " + regionID;
                log.WarnFormat(result);
                return result;
            }

            for (int i = 0; i < regionHousing.Count; i++)
            {
                DBHouse house = regionHousing[i];
                // if we already loaded this house, that's no bueno, but just skip
                if (housesForRegion.ContainsKey(house.HouseNumber))
                    continue;

                if (SpawnLot(house, housesForRegion) == eLotSpawnType.House)
                {
                    houses++;
                }
                else
                {
                    lotmarkers++;
                }
            }

            string results = "[Housing] Loaded " + houses + " houses and " + lotmarkers + " lotmarkers for region " + regionID + "!";

            if (log.IsInfoEnabled)
                log.Info(results);

            return results;
        }

        /// <summary>
        /// Spawn house or lotmarker on this lot
        /// </summary>
        /// <param name="house"></param>
        private static eLotSpawnType SpawnLot(DBHouse house, Dictionary<int, House> housesForRegion)
        {
            eLotSpawnType spawnType = eLotSpawnType.Marker;

            if (string.IsNullOrEmpty(house.OwnerID) == false)
            {
                var newHouse = new House(house) { UniqueID = house.HouseNumber };

                newHouse.LoadFromDatabase();

                // store the house
                housesForRegion.Add(newHouse.HouseNumber, newHouse);

                if (newHouse.Model > 0)
                {
                    spawnType = eLotSpawnType.House;
                }
            }

            if (spawnType == eLotSpawnType.Marker)
            {
                // this is either an available lot or a purchased lot without a house
                GameLotMarker.SpawnLotMarker(house);
            }

            return spawnType;
        }

        /// <summary>
        /// Remove all housing for this regionID
        /// </summary>
        /// <param name="regionID"></param>
        public static void RemoveHousingForRegion(ushort regionID)
        {
            if (_houseList.ContainsKey(regionID))
            {
                _houseList.Remove(regionID);
            }

            if (_idList.ContainsKey(regionID))
            {
                _idList.Remove(regionID);
            }
        }

        public static void Stop()
        {
        }

        public static IDictionary<int, House> GetHouses(ushort regionID)
        {
            // try and get the houses for the given region
            _houseList.TryGetValue(regionID, out Dictionary<int, House> housesByRegion);

            return housesByRegion;
        }

        public static House GetHouse(ushort regionID, int houseNumber)
        {
            // try and get the houses for the given region
            _houseList.TryGetValue(regionID, out Dictionary<int, House> housesByRegion);

            // if we couldn't find houses for the region, return null
            if (housesByRegion == null)
            {
                return null;
            }

            // if the house number exists, return the house
            if (housesByRegion.ContainsKey(houseNumber))
            {
                return housesByRegion[houseNumber];
            }

            // couldn't find the house, return null
            return null;
        }

        public static House GetHouse(int houseNumber)
        {
            // search thru each housing region, and if a house is found with
            // the given house number, return it

            foreach (var housingRegion in _houseList.Values)
            {
                if (housingRegion.ContainsKey(houseNumber))
                {
                    return housingRegion[houseNumber];
                }
            }

            // couldn't find the house, return null
            return null;
        }


        public static GameConsignmentMerchant GetConsignmentByHouseNumber(int houseNumber)
        {
            // search thru each housing region, and if a house is found with
            // the given house number, return the consignment merchant

            foreach (var housingRegion in _houseList.Values)
            {
                if (housingRegion == null)
                {
                    return null;
                }
                //lock(_houseList.Values as Dictionary).
                if (housingRegion.ContainsKey(houseNumber))
                {
                    return housingRegion[houseNumber].ConsignmentMerchant;
                }
            }

            // couldn't find the house, return null
            return null;
        }


        public static void AddHouse(House house)
        {
            // try and get the houses for the given region
            _houseList.TryGetValue(house.RegionID, out Dictionary<int, House> housesByRegion);

            if (housesByRegion == null)
            {
                return;
            }

            // if the house doesn't exist yet, add it
            if (!housesByRegion.ContainsKey(house.HouseNumber))
            {
                housesByRegion.Add(house.HouseNumber, house);
            }
            else
            {
                // replace the existing lot with our new house
                housesByRegion[house.HouseNumber] = house;
            }

            if (house.Model == 0)
            {
                // if this is a lot marker purchase then reset all permissions and customization
                RemoveHouseItems(house);
                RemoveHousePermissions(house);
                ResetHouseData(house);
            }
            else
            {
                // create a new set of permissions
                for (int i = HousingConstants.MinPermissionLevel; i < HousingConstants.MaxPermissionLevel + 1; i++)
                {
                    if (house.PermissionLevels.ContainsKey(i))
                    {
                        var oldPermission = house.PermissionLevels[i];
                        if (oldPermission != null)
                        {
                            GameServer.Database.DeleteObject(oldPermission);
                        }
                    }

                    // create a new, blank permission
                    var permission = new DBHousePermissions(house.HouseNumber, i);
                    house.PermissionLevels.Add(i, permission);

                    // add the permission to the database
                    GameServer.Database.AddObject(permission);
                }
            }

            // save the house, broadcast an update
            house.SaveIntoDatabase();
            house.SendUpdate();
        }

        public static bool UpgradeHouse(House house, InventoryItem deed)
        {
            int newModel = 0;
            switch (deed.Id_nb)
            {
                case "housing_alb_cottage_deed":
                    newModel = 1;
                    break;
                case "housing_alb_house_deed":
                    newModel = 2;
                    break;
                case "housing_alb_villa_deed":
                    newModel = 3;
                    break;
                case "housing_alb_mansion_deed":
                    newModel = 4;
                    break;
                case "housing_mid_cottage_deed":
                    newModel = 5;
                    break;
                case "housing_mid_house_deed":
                    newModel = 6;
                    break;
                case "housing_mid_villa_deed":
                    newModel = 7;
                    break;
                case "housing_mid_mansion_deed":
                    newModel = 8;
                    break;
                case "housing_hib_cottage_deed":
                    newModel = 9;
                    break;
                case "housing_hib_house_deed":
                    newModel = 10;
                    break;
                case "housing_hib_villa_deed":
                    newModel = 11;
                    break;
                case "housing_hib_mansion_deed":
                    newModel = 12;
                    break;
                default:
                    break;
            }

            if (newModel == 0)
            {
                return false;
            }

            // remove all players from the home before we upgrade it
            foreach (GamePlayer player in house.GetAllPlayersInHouse())
            {
                player.LeaveHouse();
            }

            // if there is a consignment merchant, we have to re-initialize since we changed the house
            var merchant = GameServer.Database.SelectObjects<HouseConsignmentMerchant>("`HouseNumber` = @HouseNumber", new QueryParameter("@HouseNumber", house.HouseNumber)).FirstOrDefault();
            long oldMerchantMoney = 0;
            if (merchant != null)
            {
                oldMerchantMoney = merchant.Money;
            }

            RemoveHouseItems(house);

            // change the model of the house
            house.Model = newModel;

            // re-add the merchant if there was one
            if (merchant != null)
            {
                house.AddConsignment(oldMerchantMoney);
            }

            // save the house, and broadcast an update
            house.SaveIntoDatabase();
            house.SendUpdate();

            return true;
        }

        public static void RemoveEmptyLotByTimer(House house)
        {
            if (house.Model > 0)
                return;
            // try and get the houses for the given region
            _houseList.TryGetValue(house.RegionID, out Dictionary<int, House> housesByRegion);

            if (housesByRegion == null)
            {
                return;
            }

            // remove all players from the house
            IList<GamePlayer> list = house.GetAllPlayersInHouse();
            for (int i = 0; i < list.Count; i++)
            {
                GamePlayer player = list[i];
                player.LeaveHouse();
            }

            // remove the house for all nearby players
            foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot(house, WorldMgr.OBJ_UPDATE_DISTANCE))
            {
                player.Out.SendRemoveHouse(house);
                player.Out.SendGarden(house);
            }

            if (house.Model > 0)
                log.WarnFormat("Removing house #{0} owned by {1}!", house.HouseNumber, house.DatabaseItem.Name);
            else
                log.WarnFormat("Lotmarker #{0} owned by {1} had rent due, lot now for sale!", house.HouseNumber, house.DatabaseItem.Name);

            RemoveHouseItems(house);
            RemoveHousePermissions(house);
            ResetHouseData(house);

            house.OwnerID = "";
            house.DatabaseItem.Porch = false;
            house.DatabaseItem.HasConsignment = false;
            house.KeptMoney = 0;
            house.Name = ""; // not null !
            house.DatabaseItem.CreationTime = DateTime.Now;
            house.DatabaseItem.LastPaid = DateTime.MinValue;

            // saved the cleared house in the database
            house.SaveIntoDatabase();

            // remove the house from the list of houses in the region
            housesByRegion.Remove(house.HouseNumber);

            // spawn a lot marker for the now-empty lot
            GameLotMarker.SpawnLotMarker(house.DatabaseItem);
        }

        public static void RemoveHouse(House house)
        {
            // try and get the houses for the given region
            _houseList.TryGetValue(house.RegionID, out Dictionary<int, House> housesByRegion);

            if (housesByRegion == null)
            {
                return;
            }

            // remove all players from the house
            IList<GamePlayer> list = house.GetAllPlayersInHouse();
            for (int i = 0; i < list.Count; i++)
            {
                GamePlayer player = list[i];
                player.LeaveHouse();
            }

            // remove the house for all nearby players
            foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot(house, WorldMgr.OBJ_UPDATE_DISTANCE))
            {
                player.Out.SendRemoveHouse(house);
                player.Out.SendGarden(house);
            }

            if (house.Model > 0)
                log.WarnFormat("Removing house #{0} owned by {1}!", house.HouseNumber, house.DatabaseItem.Name);
            else
                log.WarnFormat("Lotmarker #{0} owned by {1} had rent due, lot now for sale!", house.HouseNumber, house.DatabaseItem.Name);

            RemoveHouseItems(house);
            RemoveHousePermissions(house);
            ResetHouseData(house);

            house.OwnerID = "";
            house.DatabaseItem.Porch = false;
            house.DatabaseItem.HasConsignment = false;
            house.KeptMoney = 0;
            house.Name = ""; // not null !
            house.DatabaseItem.CreationTime = DateTime.Now;
            house.DatabaseItem.LastPaid = DateTime.MinValue;

            // saved the cleared house in the database
            house.SaveIntoDatabase();

            // remove the house from the list of houses in the region
            housesByRegion.Remove(house.HouseNumber);

            // spawn a lot marker for the now-empty lot
            GameLotMarker.SpawnLotMarker(house.DatabaseItem);
        }

        public static int GetRealmByHousenumber(int houseNumber)
        {
            if (houseNumber >= 1 && houseNumber <= 1383)
            {
                return 1;
            }

            if (houseNumber >= 1601 && houseNumber <= 2574)
            {
                return 2;
            }

            if (houseNumber >= 3200 && houseNumber <= 4399)
            {
                return 3;
            }
            else
                return 0;
        }

        public static void RemoveHouseItems(House house)
        {
            house.RemoveConsignmentMerchant();

            IList<DBHouseIndoorItem> iobjs = GameServer.Database.SelectObjects<DBHouseIndoorItem>("`HouseNumber` = @HouseNumber", new QueryParameter("@HouseNumber", house.HouseNumber));
            GameServer.Database.DeleteObject(iobjs);
            house.IndoorItems.Clear();

            IList<DBHouseOutdoorItem> oobjs = GameServer.Database.SelectObjects<DBHouseOutdoorItem>("`HouseNumber` = @HouseNumber", new QueryParameter("@HouseNumber", house.HouseNumber));
            GameServer.Database.DeleteObject(oobjs);
            house.OutdoorItems.Clear();

            IList<DBHouseHookpointItem> hpobjs = GameServer.Database.SelectObjects<DBHouseHookpointItem>("`HouseNumber` = @HouseNumber", new QueryParameter("@HouseNumber", house.HouseNumber));
            GameServer.Database.DeleteObject(hpobjs);

            IList<HouseConsignmentMerchant> ConsignmentMerchant = GameServer.Database.SelectObjects<HouseConsignmentMerchant>("`HouseNumber` = @HouseNumber", new QueryParameter("@HouseNumber", house.HouseNumber));
            GameServer.Database.DeleteObject(ConsignmentMerchant);

            //IList<Account> lastAccountId = GameServer.Database.SelectObjects<Account>("AccountID = '" + playerAccount.AccountID + "'");

            switch (GetRealmByHousenumber(house.HouseNumber))
            {
                case 1:
                    IList<Account> houseNumberAlb = GameServer.Database.SelectObjects<Account>("`HouseNumberAlb` = @HouseNumberAlb", new QueryParameter("@HouseNumberAlb", house.HouseNumber));

                    for (int i = 0; i < houseNumberAlb.Count; i++)
                    {
                        Account dba = houseNumberAlb[i];
                        if (dba != null && house.HouseNumber == dba.HouseNumberAlb && dba.HasHouseAlb)
                        {
                            dba.HasHouseAlb = false;
                            dba.HouseNumberAlb = 0;
                        }
                        GameServer.Database.SaveObject(dba);
                    }
                    break;
                case 2:
                    IList<Account> houseNumberMid = GameServer.Database.SelectObjects<Account>("`HouseNumberMid` = @HouseNumberMid", new QueryParameter("@HouseNumberMid", house.HouseNumber));

                    for (int i = 0; i < houseNumberMid.Count; i++)
                    {
                        Account dba = houseNumberMid[i];
                        if (dba != null && house.HouseNumber == dba.HouseNumberMid && dba.HasHouseMid)
                        {
                            dba.HasHouseMid = false;
                            dba.HouseNumberMid = 0;
                        }
                        GameServer.Database.SaveObject(dba);
                    }
                    break;
                case 3:

                    IList<Account> houseNumberHib = GameServer.Database.SelectObjects<Account>("`HouseNumberHib` = @HouseNumberHib", new QueryParameter("@HouseNumberHib", house.HouseNumber));

                    for (int i = 0; i < houseNumberHib.Count; i++)
                    {
                        Account dba = houseNumberHib[i];
                        if (dba != null && house.HouseNumber == dba.HouseNumberHib && dba.HasHouseHib)
                        {
                            dba.HasHouseHib = false;
                            dba.HouseNumberHib = 0;
                        }
                        GameServer.Database.SaveObject(dba);
                    }
                    break;
            }



            foreach (DBHouseHookpointItem item in house.HousepointItems.Values)
            {
                if (item.GameObject is GameObject)
                {
                    (item.GameObject as GameObject).Delete();
                }
            }
            house.HousepointItems.Clear();
        }

        public static void RemoveHousePermissions(House house)
        {
            // clear the house number for the guild if this is a guild house
            if (house.DatabaseItem.GuildHouse)
            {
                Guild guild = GuildMgr.GetGuildByName(house.DatabaseItem.GuildName);
                if (guild != null)
                {
                    guild.GuildHouseNumber = 0;
                }
            }

            house.DatabaseItem.GuildHouse = false;
            house.DatabaseItem.GuildName = null;

            IList<DBHousePermissions> pobjs = GameServer.Database.SelectObjects<DBHousePermissions>("`HouseNumber` = @HouseNumber", new QueryParameter("@HouseNumber", house.HouseNumber));
            GameServer.Database.DeleteObject(pobjs);
            house.PermissionLevels.Clear();


            IList<DBHouseCharsXPerms> cpobjs = GameServer.Database.SelectObjects<DBHouseCharsXPerms>("`HouseNumber` = @HouseNumber", new QueryParameter("@HouseNumber", house.HouseNumber));
            GameServer.Database.DeleteObject(cpobjs);
            house.CharXPermissions.Clear();
        }

        public static void ResetHouseData(House house)
        {
            house.Model = 0;
            house.Emblem = 0;
            house.IndoorGuildBanner = false;
            house.IndoorGuildShield = false;
            house.DoorMaterial = 0;
            house.OutdoorGuildBanner = false;
            house.OutdoorGuildShield = false;
            house.Porch = false;
            house.PorchMaterial = 0;
            house.PorchRoofColor = 0;
            house.RoofMaterial = 0;
            house.Rug1Color = 0;
            house.Rug2Color = 0;
            house.Rug3Color = 0;
            house.Rug4Color = 0;
            house.TrussMaterial = 0;
            house.WallMaterial = 0;
            house.WindowMaterial = 0;
        }

        /// <summary>
        /// Checks if a player is the owner of the house, this checks all characters on the account
        /// </summary>
        /// <param name="house">The house object</param>
        /// <param name="player">The player to check</param>
        /// <returns>True if the player is the owner</returns>
        public static bool IsOwner(DBHouse house, GamePlayer player)
        {
            // house and player can't be null
            if (house == null || player == null)
            {
                return false;
            }

            // if owner id isn't set, there is no owner
            if (string.IsNullOrEmpty(house.OwnerID))
            {
                return false;
            }

            // check if this a guild house, and if the player
            // 1) belongs to the guild and is 2) a GM in the guild
            if (player.Guild != null && house.GuildHouse)
            {
                if (player.Guild.Name == house.GuildName && player.Guild.HasRank(player, Guild.eRank.Leader))
                {
                    return true;
                }
            }
            else
            {
                foreach (DOLCharacters c in player.Client.Account.Characters)
                {
                    if (house.OwnerID == c.ObjectId)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static int GetHouseNumberByPlayer(GamePlayer p)
        {
            House house = GetHouseByPlayer(p);

            return house != null ? house.HouseNumber : 0;
        }


        /// <summary>
        /// Get the house object from the owner player
        /// </summary>
        /// <param name="p">The player owner</param>
        /// <returns>The house object</returns>
        public static House GetHouseByPlayer(GamePlayer p)
        {
            // check every house in every region until we find
            // a house that belongs to this player
            //Elcotek bugfix for House search!!
            lock (((ICollection)_houseList).SyncRoot)
            {
                foreach (var regs in _houseList)
                {
                    foreach (var entry in regs.Value)
                    {
                        var house = entry.Value;

                        //

                        switch (GetRealmByHousenumber(house.HouseNumber))
                        {
                            case 1:
                                {
                                    if (p.Realm == eRealm.Albion)
                                    {
                                        if (house.OwnerID == p.DBCharacter.ObjectId || p.Client.Account.HasHouseAlb && p.Client.Account.HouseNumberAlb == house.HouseNumber)
                                            return house;
                                    }
                                }
                                break;
                            case 2:
                                {
                                    if (p.Realm == eRealm.Midgard)
                                    {
                                        if (house.OwnerID == p.DBCharacter.ObjectId || p.Client.Account.HasHouseMid && p.Client.Account.HouseNumberMid == house.HouseNumber)
                                            return house;
                                    }
                                }
                                break;
                            case 3:
                                {
                                    if (p.Realm == eRealm.Hibernia)
                                    {
                                        if (house.OwnerID == p.DBCharacter.ObjectId || p.Client.Account.HasHouseHib && p.Client.Account.HouseNumberHib == house.HouseNumber)
                                            return house;
                                    }
                                }
                                break;
                        }

                        // if (house.OwnerID == p.DBCharacter.ObjectId || p.Client.Account.HasHouse && p.Client.Account.HouseNumber == house.HouseNumber)
                        //  return house;
                    }
                }

                // didn't find a house that belonged to the player,
                // so return null
                return null;
            }
        }

        /// <summary>
		/// Gets the guild house object by real owner
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public static House GetGuildHouseByPlayer(GamePlayer p)
        {
            // make sure player is in a guild
            if (p.Guild == null || p.Guild.GuildOwnsHouse == false)
            {
                return null;
            }

            var house = GetHouse(p.Guild.GuildHouseNumber);

            if (house != null)
            {
                return house;
            }

            // check every house in every region until we find
            // a house that belongs to the same guild as the player
            foreach (var regs in _houseList)
            {
                foreach (var entry in regs.Value)
                {
                    house = entry.Value;




                    switch (GetRealmByHousenumber(house.HouseNumber))
                    {
                        case 1:
                            {
                                if (p.Realm == eRealm.Albion)
                                {
                                    if (house.DatabaseItem.GuildName == p.Guild.Name)
                                        return house;
                                }
                            }
                            break;
                        case 2:
                            {
                                if (p.Realm == eRealm.Midgard)
                                {
                                    if (house.DatabaseItem.GuildName == p.Guild.Name)
                                        return house;
                                }
                            }
                            break;
                        case 3:
                            {
                                if (p.Realm == eRealm.Hibernia)
                                {
                                    if (house.DatabaseItem.GuildName == p.Guild.Name)
                                        return house;
                                }
                            }
                            break;
                    }
                }
            }

            // didn't find a house that belonged to the player's guild,
            // or they aren't in a guild, so return null
            return null;
        }

        /// <summary>
        /// Transfer a house to a guild house
        /// </summary>
        /// <param name="player"></param>
        /// <param name="house"></param>
        /// <returns></returns>
        public static bool HouseTransferToGuild(GamePlayer player, House house)
        {
            // player must be in a guild
            if (player.Guild == null)
            {
                return false;
            }

            // player's guild can't already have a guild house
            if (player.Guild.GuildOwnsHouse)
            {
                return false;
            }

            // player needs to own the house to be able to xfer it
            if (house.HasOwnerPermissions(player) == false)
            {
                ChatUtil.SendSystemMessage(player, "You do not own this house!");
                return false;
            }

            // player needs to be a GM in the guild to xfer his personal house to the guild
            if (player.Guild.HasRank(player, Guild.eRank.Leader) == false)
            {
                ChatUtil.SendSystemMessage(player, "You are not the leader of a guild!");
                return false;
            }

            // Demand any consignment merchant inventory is removed before allowing a transfer
            var consignmentMerchant = house.ConsignmentMerchant;
            if (consignmentMerchant != null && (consignmentMerchant.DBItems().Count > 0 || consignmentMerchant.TotalMoney > 0))
            {
                ChatUtil.SendSystemMessage(player, "All items and money must be removed from your consigmment merchant in order to transfer this house!");
                return false;
            }

            // send house xfer prompt to player
            player.Out.SendCustomDialog(LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Player.Housing.TransferToGuild", player.Guild.Name), MakeGuildLot);

            return true;
        }

        private static void MakeGuildLot(GamePlayer player, byte response)
        {
            // user responded no/decline
            if (response != 0x01)
            {
                return;
            }

            var playerHouse = GetHouse(GetHouseNumberByPlayer(player));
            var playerGuild = player.Guild;

            // double check and make sure this guild isn't null
            if (playerGuild == null)
            {
                return;
            }

            // adjust the house to be under guild control
            playerHouse.DatabaseItem.OwnerID = playerGuild.GuildID;
            playerHouse.DatabaseItem.Name = playerGuild.Name;
            playerHouse.DatabaseItem.GuildHouse = true;
            playerHouse.DatabaseItem.GuildName = playerGuild.Name;

            // adjust guild to reflect their new guild house
            player.Guild.GuildHouseNumber = playerHouse.HouseNumber;

            // notify guild members of the guild house acquisition
            player.Guild.SendMessageToGuildMembers(
                LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Player.Housing.GuildNowOwns", player.Guild.Name, player.Name),
                eChatType.CT_Guild, eChatLoc.CL_SystemWindow);

            // save the guild and broadcast updates
            player.Guild.SaveIntoDatabase();
            player.Guild.UpdateGuildWindow();

            // save the house and broadcast updates
            playerHouse.SaveIntoDatabase();
            playerHouse.SendUpdate();
        }

        public static long GetRentByModel(int model)
        {
            if (model > 0)
            {
                switch (model % 4)
                {
                    case 0:
                        return Properties.HOUSING_RENT_MANSION;
                    case 1:
                        return Properties.HOUSING_RENT_COTTAGE;
                    case 2:
                        return Properties.HOUSING_RENT_HOUSE;
                    case 3:
                        return Properties.HOUSING_RENT_VILLA;
                }
            }

            return Properties.HOUSING_RENT_COTTAGE;
        }

        public static void CheckRents(object state)
        {

            //Remove empty lots
            GameLotMarker.AutomaticEmptyLotRemover();


            if (Properties.RENT_DUE_DAYS == 0)
            {
                return;
            }

            log.Debug("[Housing] Starting timed rent check");

            TimeSpan diff;
            var houseRemovalList = new List<House>();
            //Elcotek bugfix for House search!!
            lock (((ICollection)_houseList).SyncRoot)
            {
                foreach (var regs in _houseList)
                {
                    foreach (var entry in regs.Value)
                    {
                        var house = entry.Value;

                        // if the house has no owner or is set to not be purged, 
                        // we just skip over it
                        if (string.IsNullOrEmpty(house.OwnerID) || house.NoPurge)
                        {
                            continue;
                        }

                        // get the time that rent was last paid for the house
                        diff = DateTime.Now - house.LastPaid;

                        // get the amount of rent for the given house
                        long rent = GetRentByModel(house.Model);

                        // Does this house need to pay rent?
                        if (rent > 0L && diff.Days >= Properties.RENT_DUE_DAYS)
                        {
                            long lockboxAmount = house.KeptMoney;
                            long consignmentAmount = 0;

                            var consignmentMerchant = house.ConsignmentMerchant;
                            if (consignmentMerchant != null)
                            {
                                consignmentAmount = consignmentMerchant.TotalMoney;
                            }

                            // try to pull from the lockbox first
                            if (lockboxAmount >= rent)
                            {
                                house.KeptMoney -= rent;
                                house.LastPaid = DateTime.Now;
                                house.SaveIntoDatabase();
                            }
                            else
                            {
                                long remainingDifference = (rent - lockboxAmount);

                                // not enough was in the lockbox.  see if we have the difference
                                // on the consignment merchant
                                if (remainingDifference <= consignmentAmount && consignmentAmount > 0)
                                {
                                    // we have the difference, phew!
                                    house.KeptMoney = 0;
                                    consignmentMerchant.TotalMoney -= remainingDifference;
                                    consignmentMerchant.SaveIntoDatabase();

                                    while (house.LastPaid != DateTime.Now)
                                    {
                                        house.LastPaid = DateTime.Now;
                                        house.SaveIntoDatabase();
                                    }
                                }

                                else
                                {
                                    // house can't afford rent, so we schedule house to be repossessed.
                                    houseRemovalList.Add(house);
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < houseRemovalList.Count; i++)
                {
                    House h = houseRemovalList[i];
                    RemoveHouse(h);
                }
            }
        }


        public static void SendHousingMerchantWindow(GamePlayer player, eMerchantWindowType merchantType)
        {
            GameServer.ServerRules.SendHousingMerchantWindow(player, merchantType);
        }

        public static void BuyHousingItem(GamePlayer player, ushort slot, byte count, eMerchantWindowType merchantType)
        {
            GameServer.ServerRules.BuyHousingItem(player, slot, count, merchantType);
        }


        /// <summary>
        /// This function gets the house close to spot
        /// </summary>
        /// <returns>array of house</returns>
        public static IEnumerable GetHousesCloseToSpot(ushort regionid, int x, int y, int radius)
        {
            var myhouses = new ArrayList();
            int radiussqrt = radius * radius;
            lock (((ICollection)GetHouses(regionid).Values).SyncRoot)
            {
                foreach (House house in GetHouses(regionid).Values)
                {
                    int xdiff = house.X - x;
                    int ydiff = house.Y - y;
                    int range = xdiff * xdiff + ydiff * ydiff;
                    if (range < 0)
                    {

                        range *= -1;
                    }
                    //log.ErrorFormat("range {0}", range);
                    if (range > radiussqrt)
                    {
                        //log.ErrorFormat("range {0} > radiussqrt {1}", range, radiussqrt);
                        continue;
                    }
                    // log.InfoFormat("Haus gefunden: house.Name: {0} house.Model:{1} house.HouseNumber{2} house.Position: {3} ", house.Name, house.Model, house.HouseNumber, house.X, house.Y, house.Z);
                    myhouses.Add(house);
                }
            }
            return myhouses;
        }
    }
}