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
using DOL.AI.Brain;
using DOL.GS.Housing;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static DOL.GS.GameObject;

namespace DOL.GS
{
    /// <summary>
    /// Description of WorldUpdateThread.
    /// </summary>
    public static class WorldUpdateThread
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        const string SpamTime = "NOSPAMFORNIGHTUPDATES";
        /// <summary>
        /// Minimum Player Update Loop Refresh Rate. (ms)
        /// </summary>
        private static readonly uint MIN_PLAYER_WORLD_UPDATE_RATE = 50;

        /// <summary>
        /// Minimum NPC Update Loop Refresh Rate. (ms)
        /// </summary>
        private static readonly uint MIN_NPC_UPDATE_RATE = 1000;

        /// <summary>
        /// Minimum Static Item Update Loop Refresh Rate. (ms)
        /// </summary>
        private static readonly uint MIN_ITEM_UPDATE_RATE = 10000;

        /// <summary>
        /// Minimum Housing Update Loop Refresh Rate. (ms)
        /// </summary>
        private static readonly uint MIN_HOUSING_UPDATE_RATE = 10000;

        /// <summary>
        /// Minimum Player Position Update Loop Refresh Rate. (ms)
        /// </summary>
        private static readonly uint MIN_PLAYER_UPDATE_RATE = 1000;

        // Variable zum Cachen des Wertes
        private static uint? cachedPlayerWorldUpdateInterval = null;
        /// <summary>
        /// Get the Player World Update Refresh Rate.
        /// </summary>
        /// <returns></returns>
        private static uint GetPlayerWorldUpdateInterval
        {
            get
            {
                // Überprüfen, ob der Cache vorhanden ist
                if (cachedPlayerWorldUpdateInterval == null)
                {
                    // Cache den Wert, wenn er noch nicht gesetzt wurde
                    cachedPlayerWorldUpdateInterval = Math.Max(ServerProperties.Properties.WORLD_PLAYER_UPDATE_INTERVAL, MIN_PLAYER_WORLD_UPDATE_RATE);
                }

                // Rückgabe des im Cache gespeicherten Wertes
                return cachedPlayerWorldUpdateInterval.Value;
            }
        }

        // Variable zum Cachen des Wertes
        private static uint? cachedPlayerNPCWorldUpdateInterval = null;
        /// <summary>
        /// Get Player NPC Refresh Rate.
        /// </summary>
        /// <returns></returns>
        private static uint GetPlayerNPCUpdateInterval
        {
            get
            {
                if (cachedPlayerNPCWorldUpdateInterval == null)
                {
                    cachedPlayerNPCWorldUpdateInterval = Math.Max(ServerProperties.Properties.WORLD_NPC_UPDATE_INTERVAL, MIN_NPC_UPDATE_RATE);
                }
                return cachedPlayerNPCWorldUpdateInterval.Value;
            }
        }

        // Variable zum Cachen des Wertes
        private static uint? cachedPlayerItemUpdateInterval = null;
        /// <summary>
        /// Get Player Static Item Refresh Rate.
        /// </summary>
        /// <returns></returns>
        private static uint GetPlayerItemUpdateInterval
        {
            get
            {
                if (cachedPlayerItemUpdateInterval == null)
                {
                    cachedPlayerItemUpdateInterval = Math.Max(ServerProperties.Properties.WORLD_OBJECT_UPDATE_INTERVAL, MIN_ITEM_UPDATE_RATE);
                }
                return cachedPlayerItemUpdateInterval.Value;
            }
        }

        // Variable zum Cachen des Wertes
        private static uint? cachedPlayerHousingUpdateInterval = null;
        /// <summary>
        /// Get Player Housing Item Refresh Rate.
        /// </summary>
        /// <returns></returns>
        private static uint GetPlayerHousingUpdateInterval
        {
            get
            {
                if (cachedPlayerHousingUpdateInterval == null)
                {
                    cachedPlayerHousingUpdateInterval = Math.Max(ServerProperties.Properties.WORLD_OBJECT_UPDATE_INTERVAL, MIN_HOUSING_UPDATE_RATE);
                }
                return cachedPlayerHousingUpdateInterval.Value;
            }
        }

        // Variable zum Cachen des Wertes
        private static uint? cachedPlayertoPlayerUpdateInterval = null;
        /// <summary>
        /// Get Player to Other Player Update Rate
        /// </summary>
        /// <returns></returns>
        private static uint GetPlayertoPlayerUpdateInterval
        {
            get 
            {
                if (cachedPlayertoPlayerUpdateInterval == null)
                {
                    cachedPlayertoPlayerUpdateInterval = Math.Max(ServerProperties.Properties.WORLD_PLAYERTOPLAYER_UPDATE_INTERVAL, MIN_PLAYER_UPDATE_RATE);
                }
                return cachedPlayertoPlayerUpdateInterval.Value;
            }
        }
       

        /// <summary>
        /// Update all World Around Player
        /// </summary>
        /// <param name="player">The player needing update</param>
        private static void UpdatePlayerWorld(GamePlayer player)
        {
            if (player != null && player.ObjectState == eObjectState.Active)
            UpdatePlayerWorld(player, GameTimer.GetTickCount());

        }

        /// <summary>
        /// Update all World Around Player
        /// </summary>
        /// <param name="player">The player needing update</param>
        /// <param name="nowTicks">The actual time of the refresh.</param>
        private static void UpdatePlayerWorld(GamePlayer player, long nowTicks)
        {
            // Update Player Player's
            if (ServerProperties.Properties.WORLD_PLAYERTOPLAYER_UPDATE_INTERVAL > 0)
            {
                UpdatePlayerOtherPlayers(player, nowTicks);

            }

            // Update Player Mob's
            if (ServerProperties.Properties.WORLD_NPC_UPDATE_INTERVAL > 0)
            {
                UpdatePlayerNPCs(player, nowTicks);
            }

            // Update Player Static Item
            if (ServerProperties.Properties.WORLD_OBJECT_UPDATE_INTERVAL > 0)
            {
                UpdatePlayerItems(player, nowTicks);
            }

            // Update Player Doors
            if (ServerProperties.Properties.WORLD_OBJECT_UPDATE_INTERVAL > 0)
            {
                UpdatePlayerDoors(player, nowTicks);
            }

            // Update Player Housing
            if (ServerProperties.Properties.WORLD_OBJECT_UPDATE_INTERVAL > 0)
            {
                UpdatePlayerHousing(player, nowTicks);
            }
        }



        private static void UpdatePlayerOtherPlayers(GamePlayer player, long nowTicks)
        {
            GameObject obj = null;
            // Get All Player in Range
            //var players = player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE).Cast<GamePlayer>().Where(p => p != null && p.IsVisibleTo(player) && (!p.IsStealthed || player.CanDetect(p))).ToArray();

            var players = new HashSet<GamePlayer>(
           player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE)
          .Cast<GamePlayer>()
          .Where(p => p != null && p.IsVisibleTo(player) && (!p.IsStealthed || player.CanDetect(p))));

            try
            {
                foreach (var objEntry in player.Client.GameObjectUpdateArray)
                {
                    var objKey = objEntry.Key;
                   
                    if (objKey.Item1 != 0 && objKey.Item2 != 0)
                    {
                        var region = WorldMgr.GetRegion(objKey.Item1);
                        if (region != null)
                        {
                            obj = region.GetObject(objKey.Item2);
                        }
                    }

                    // We have a Player in cache that is not in vincinity
                    if (obj is GamePlayer gamePlayer && !players.Contains(gamePlayer) && (nowTicks - objEntry.Value) >= GetPlayertoPlayerUpdateInterval)
                    {
                        // Update him out of View and delete from cache
                        if (obj.IsVisibleTo(player) && (!gamePlayer.IsStealthed || player.CanDetect(gamePlayer)))
                        {
                            player.Client.Out.SendPlayerForgedPosition(gamePlayer);
                        }

                        player.Client.GameObjectUpdateArray.TryRemove(objKey, out long dummy);
                    }
                }

            }
            catch (Exception e)
            {
                if (log.IsErrorEnabled)
                    log.ErrorFormat("Error while Cleaning OtherPlayers cache for Player : {0}, Exception : {1}", player.Name, e);
            }

            try
            {
                // Now Send Remaining Players.
                foreach (GamePlayer lplayer in players)
                {
                    GamePlayer otherply = lplayer;

                    if (otherply != null)
                    {
                        // Get last update time
                        long lastUpdate;
                        if (player.Client.GameObjectUpdateArray.TryGetValue(new Tuple<ushort, ushort>(otherply.CurrentRegionID, (ushort)otherply.ObjectID), out lastUpdate))
                        {
                            // This Player Needs Update
                            if ((nowTicks - lastUpdate) >= GetPlayertoPlayerUpdateInterval)
                            {
                                player.Client.Out.SendPlayerForgedPosition(otherply);
                            }
                        }
                        else
                        {
                            player.Client.Out.SendPlayerForgedPosition(otherply);
                        }
                    }
                }
            }
            catch
            {
                if (log.IsErrorEnabled)
                    log.ErrorFormat("Error while updating OtherPlayers for Player : {0}", player.Name);
            }
        }

        /// <summary>
        /// Send Mobs Update to Player depending on last refresh time.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="nowTicks"></param>
        private static void UpdatePlayerNPCs(GamePlayer player, long nowTicks)
        {
            GameObject obj = null;
            var npcs = new ConcurrentBag<GameNPC>();

            var npcList = player.GetNPCsInRadius(WorldMgr.VISIBILITY_DISTANCE)
                .Cast<GameNPC>(); // Cast to IEnumerable<GameNPC>

            Parallel.ForEach(npcList, npc =>
            {
                if (npc.IsVisibleTo(player))
                {
                    npcs.Add(npc);
                }
            });

            HashSet<Tuple<ushort, ushort>> cacheKeysToRemove = new HashSet<Tuple<ushort, ushort>>();

            try
            {
                // Clean Cache
                foreach (var objEntry in player.Client.GameObjectUpdateArray)
                {
                    var objKey = objEntry.Key;

                    if (objKey.Item1 != 0 && objKey.Item2 != 0)
                    {
                        obj = WorldMgr.GetRegion(objKey.Item1).GetObject(objKey.Item2);
                    }

                    // Brain is updating to its master, no need to handle it.
                    if (obj is GameNPC gameNpc && ((gameNpc.Brain is IControlledBrain controlledBrain) && controlledBrain.GetPlayerOwner() == player))
                        continue;

                    // We have a NPC in cache that is not in vicinity
                    if (obj is GameNPC && !npcs.Contains((GameNPC)obj) && (nowTicks - objEntry.Value) >= GetPlayerNPCUpdateInterval)
                    {
                        // Update him out of View
                        if (obj.IsVisibleTo(player))
                        {
                            player.Client.Out.SendObjectUpdate(obj);
                        }

                        // Queue up for removal
                        lock (cacheKeysToRemove)
                        {
                            cacheKeysToRemove.Add(objKey);
                        }
                    }
                   
                }

                // Remove keys in bulk
                foreach (var key in cacheKeysToRemove)
                {
                    player.Client.GameObjectUpdateArray.TryRemove(key, out long _);
                }
            }
            catch
            {
                if (log.IsErrorEnabled)
                    log.ErrorFormat("Error while Cleaning NPC cache for Player : {0}", player.Name);
            }

            try
            {
                #region Update Day/Night NPCS
                try
                {
                    if (npcs == null) return; // Überprüfung auf null

                    foreach (GameNPC dnnpc in npcs.OfType<GameNPC>())
                    {
                        
                        if (dnnpc.ObjectState != eObjectState.Active || dnnpc.CurrentRegion == null) continue;

                        long UPDATETICK = dnnpc.TempProperties.getProperty<long>(SpamTime);
                        long changeTime = dnnpc.CurrentRegion.Time - UPDATETICK;

                        if (changeTime > 120 * 1000 || UPDATETICK == 0)
                        {
                            dnnpc.TempProperties.setProperty(SpamTime, dnnpc.CurrentRegion.Time);
                            dnnpc.CheckNPCsDayNight();

                            //log.InfoFormat("NPC {0} is updating", dnnpc.Name);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (log.IsErrorEnabled)
                        log.Warn(e.Message, e);
                }
                #endregion

                if (npcs == null) return;

                Parallel.ForEach(npcs, npc =>
                {
                    // Get last update time
                    if (player.Client.GameObjectUpdateArray.TryGetValue(new Tuple<ushort, ushort>(npc.CurrentRegionID, (ushort)npc.ObjectID), out long lastUpdate))
                    {
                        // This NPC Needs Update
                        if ((nowTicks - lastUpdate) >= GetPlayerNPCUpdateInterval)
                        {
                            player.Client.Out.SendObjectUpdate(npc);
                        }
                    }
                    else
                    {
                        // Not in cache, Object entering in range, sending update will add it to cache.
                        player.Client.Out.SendObjectUpdate(npc);
                    }
                });
            }
            catch
            {
                if (log.IsErrorEnabled)
                    log.ErrorFormat("Error while updating NPC for Player : {0}", player.Name);
            }
        }


        /// <summary>
        /// Send Game Static Item depending on last refresh time
        /// </summary>
        /// <param name="player"></param>
        /// <param name="nowTicks"></param>
        private static void UpdatePlayerItems(GamePlayer player, long nowTicks)
        {
            // Nutze ein HashSet für schnelle Sichtbarkeitsprüfung
            var objs = player.GetItemsInRadius(WorldMgr.OBJ_UPDATE_DISTANCE)
                              .Cast<GameStaticItem>()
                              .Where(i => i != null && i.IsVisibleTo(player))
                              .ToHashSet();

            try
            {
                // Entfernen von nicht sichtbaren Objekten aus dem Update-Array
                var keysToRemove = new List<Tuple<ushort, ushort>>();

                foreach (var objEntry in player.Client.GameObjectUpdateArray)
                {
                    var objKey = objEntry.Key;
                    if (objKey == null) continue;

                    var obj = WorldMgr.GetRegion(objKey.Item1).GetObject(objKey.Item2);
                    if (obj is GameStaticItem staticItem &&
                        !objs.Contains(staticItem) &&
                        (nowTicks - objEntry.Value) >= GetPlayerItemUpdateInterval)
                    {
                        keysToRemove.Add(objKey);
                    }
                }

                // Entfernen der gesammelten Schlüssel
                foreach (var key in keysToRemove)
                {
                    player.Client.GameObjectUpdateArray.TryRemove(key, out _);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while Cleaning Static Item cache for Player : {0}, Exception: {1}", player.Name, ex.Message);
            }

            try
            {
                // Aktualisieren der sichtbaren Objekte in Parallel
                Parallel.ForEach(objs, staticObj =>
                {
                    var objKey = new Tuple<ushort, ushort>(staticObj.CurrentRegionID, (ushort)staticObj.ObjectID);
                    if (player.Client.GameObjectUpdateArray.TryGetValue(objKey, out long lastUpdate))
                    {
                        if ((nowTicks - lastUpdate) >= GetPlayerItemUpdateInterval)
                        {
                            player.Client.Out.SendObjectCreate(staticObj);
                        }
                    }
                    else
                    {
                        player.Client.Out.SendObjectCreate(staticObj);
                    }
                });
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while updating Static Item for Player : {0}, Exception: {1}", player.Name, ex.Message);
            }
        }




        /// <summary>
        /// Send Game Doors Depening on last refresh time.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="nowTicks"></param>
        /// <summary>
        /// Send Game Doors Depening on last refresh time.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="nowTicks"></param>
        private static void UpdatePlayerDoors(GamePlayer player, long nowTicks)
        {
            var doors = new HashSet<GameObject>(
                player.GetDoorsInRadius(WorldMgr.VISIBILITY_DISTANCE)
                .Cast<IDoor>()
                .Where(o => o != null && o.IsVisibleTo(player))
                .Select(o => (GameObject)o)
            );

            GameObject obj = null;
            var region = default(Region); // Cached region object

            try
            {
                foreach (var objEntry in player.Client.GameObjectUpdateArray)
                {
                    var objKey = objEntry.Key;

                    if (objKey != null && objKey.Item1 != 0 && objKey.Item2 != 0)
                    {
                        if (region == null)
                        {
                            region = WorldMgr.GetRegion(objKey.Item1); // Caching region
                        }

                        obj = region.GetObject(objKey.Item2);

                        if (obj is IDoor && !doors.Contains(obj) && (nowTicks - objEntry.Value) >= GetPlayerItemUpdateInterval)
                        {
                            player.Client.GameObjectUpdateArray.TryRemove(objKey, out long _);
                        }
                    }
                }
            }
            catch
            {
                if (log.IsErrorEnabled)
                    log.ErrorFormat("Error while Cleaning Doors cache for Player : {0}", player.Name);
            }

            try
            {
                // Send remaining doors parallel
                Parallel.ForEach(doors, door =>
                {
                    if (door is IDoor idoor)
                    {
                        if (player.Client.GameObjectUpdateArray.TryGetValue(
                            new Tuple<ushort, ushort>(((GameObject)idoor).CurrentRegionID, (ushort)idoor.ObjectID),
                            out long lastUpdate))
                        {
                            if ((nowTicks - lastUpdate) >= GetPlayerItemUpdateInterval)
                            {

                                player.SendDoorUpdate(idoor, true);
                            }
                        }
                        else
                        {

                            player.SendDoorUpdate(idoor, true);
                        }
                    }
                });
            }
            catch
            {
                if (log.IsErrorEnabled)
                    log.ErrorFormat("Error while updating Doors for Player : {0}", player.Name);
            }
        }

        public static void UpdatePlayerHousing(GamePlayer player, long nowTicks)
        {
            if (player.CurrentRegion == null || !player.CurrentRegion.HousingEnabled)
                return;

            IDictionary<int, House> housesDict = HouseMgr.GetHouses(player.CurrentRegionID);
            var housesSet = new HashSet<House>(housesDict.Values.Where(h => h != null && player.IsWithinRadius(h, HousingConstants.HouseViewingDistance)));

            try
            {
                // Clean Cache
                var keysToRemove = new ConcurrentBag<Tuple<ushort, ushort>>(); // Verwenden Sie eine Thread-sichere Sammlung
                foreach (var houseEntry in player.Client.HouseUpdateArray)
                {
                    var houseKey = houseEntry.Key;
                    House house = HouseMgr.GetHouse(houseKey.Item1, houseKey.Item2);

                    // Überprüfen, ob das Haus nicht mehr in der Nähe ist
                    if (!housesSet.Contains(house) && (nowTicks - houseEntry.Value) >= (GetPlayerHousingUpdateInterval >> 2))
                    {
                        keysToRemove.Add(houseKey);
                    }
                }

                // Entfernen von veralteten Einträgen
                foreach (var key in keysToRemove)
                {
                    player.Client.HouseUpdateArray.TryRemove(key, out long _);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while Cleaning House cache for Player : {0}, Exception: {1}", player.Name, ex);
            }

            try
            {
                // Parallelisierte Verarbeitung der Häuser
                Parallel.ForEach(housesSet, (lhouse) =>
                {
                    long lastUpdate;
                    if (player.Client.HouseUpdateArray.TryGetValue(new Tuple<ushort, ushort>(lhouse.RegionID, (ushort)lhouse.HouseNumber), out lastUpdate))
                    {
                        if ((nowTicks - lastUpdate) >= GetPlayerHousingUpdateInterval)
                        {
                            player.Client.Out.SendHouseOccupied(lhouse, lhouse.IsOccupied);
                        }
                    }
                    else
                    {
                        // Neu in Reichweite
                        player.Client.Out.SendHouse(lhouse);
                        player.Client.Out.SendGarden(lhouse);
                        player.Client.Out.SendHouseOccupied(lhouse, lhouse.IsOccupied);
                    }
                });
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while updating Houses for Player : {0}, Exception: {1}", player.Name, ex);
            }
        }
        
        /// <summary>
        /// Check if this player can be updated
        /// </summary>
        /// <param name="lastUpdate"></param>
        /// <returns></returns>
        private static bool PlayerNeedUpdate(long lastUpdate)
        {
            return (GameTimer.GetTickCount() - lastUpdate) >= GetPlayerWorldUpdateInterval;


        }

        private static bool StartPlayerUpdateTask(GameClient client, IDictionary<GameClient, Tuple<long, Task, Region>> clientsUpdateTasks, long begin)
        {
            GamePlayer player = client.Player;

            if (player.ObjectState != GameObject.eObjectState.Active)
            {
                // don't update not active players
                return false;
            }

            if (!clientsUpdateTasks.TryGetValue(client, out var clientEntry))
            {
                // Client not in tasks, create it and run it !
                clientEntry = CreatePlayerUpdateTask(player, begin);
                clientsUpdateTasks.Add(client, clientEntry);
                return true;
            }
            else
            {
                // Get client entry data.
                long lastUpdate = clientEntry.Item1;
                Task taskEntry = clientEntry.Item2;
                Region lastRegion = clientEntry.Item3;

                // Check if task finished
                if (!taskEntry.IsCompleted)
                {
                    // Check for how long
                    if ((begin - lastUpdate) > GetPlayerWorldUpdateInterval)
                    {
                        LogLongRunningTaskWarning(player, lastUpdate, begin);
                    }
                    return false;
                }

                // Display Exception
                if (taskEntry.IsFaulted)
                {
                    LogPlayerTaskError(player);
                }

                // Region Refresh
                if (player.CurrentRegion != lastRegion)
                {
                    lastUpdate = 0;
                    lastRegion = player.CurrentRegion;
                    client.GameObjectUpdateArray.Clear();
                    client.HouseUpdateArray.Clear();
                }

                // If this player need update.
                if (PlayerNeedUpdate(lastUpdate))
                {
                    // Update Time, Region and Create Task
                    var newClientEntry = CreatePlayerUpdateTask(player, begin);
                    clientsUpdateTasks[client] = newClientEntry;

                    return true;
                }
            }

            return false;
        }

        private static Tuple<long, Task, Region> CreatePlayerUpdateTask(GamePlayer player, long begin)
        {
            var task = Task.Factory.StartNew(() => UpdatePlayerWorld(player));
            return new Tuple<long, Task, Region>(begin, task, player.CurrentRegion);
        }

        private static void LogLongRunningTaskWarning(GamePlayer player, long lastUpdate, long begin)
        {
            if (log.IsWarnEnabled && (GameTimer.GetTickCount() - player.TempProperties.getProperty<long>("LAST_WORLD_UPDATE_THREAD_WARNING", 0) >= 1000))
            {
                log.WarnFormat("Player Update Task ({0}) Taking more than world update refresh rate : {1} ms (real {2} ms) - Task Status : {3}!", player.Name, GetPlayerWorldUpdateInterval.ToString(), (begin - lastUpdate).ToString(), "Running");
                player.TempProperties.setProperty("LAST_WORLD_UPDATE_THREAD_WARNING", GameTimer.GetTickCount().ToString());
            }
        }

        private static void LogPlayerTaskError(GamePlayer player)
        {
            if (log.IsErrorEnabled)
            {
                log.ErrorFormat("Error in World Update Thread, Player Task ({0})!", player.Name);
            }
        }
        private static bool IsTaskCompleted(GameClient client, IDictionary<GameClient, Tuple<long, Task, Region>> clientsUpdateTasks)
        {
            // Check for existing Task
            if (clientsUpdateTasks.TryGetValue(client, out Tuple<long, Task, Region> clientEntry))
            {
                Task taskEntry = clientEntry.Item2;

                if (taskEntry != null)
                    return taskEntry.IsCompleted;
            }

            return true;
        }


        /// <summary>
        /// This thread updates the NPCs and objects around the player at very short
        /// intervalls! But since the update is very quick the thread will
        /// sleep most of the time!
        /// </summary>
        public static void WorldUpdateThreadStart()
        {
            // Tasks Collection of running Player updates, with starting time.
            var clientsUpdateTasks = new Dictionary<GameClient, Tuple<long, Task, Region>>();

            bool running = true;

            if (log.IsInfoEnabled)
            {
                log.InfoFormat("World Update Thread Starting - ThreadId = {0}", Thread.CurrentThread.ManagedThreadId);
            }

            while (running)
            {
                try
                {
                    // Start Time of the loop
                    long begin = GameTimer.GetTickCount();

                    // Get All Clients
                    var clients = WorldMgr.GetAllClients().ToList(); // Convert to list for parallel processing

                    // Clean Tasks Dict on Client Exiting.
                    foreach (GameClient cli in clientsUpdateTasks.Keys.ToList())
                    {
                        if (cli == null)
                            continue;

                        GamePlayer player = cli.Player;

                        bool notActive = cli.ClientState != GameClient.eClientState.Playing || player == null || player.ObjectState != GameObject.eObjectState.Active;
                        bool notConnected = !clients.Contains(cli);

                        if (notConnected || (notActive && IsTaskCompleted(cli, clientsUpdateTasks)))
                        {
                            clientsUpdateTasks.Remove(cli);
                            cli.GameObjectUpdateArray.Clear();
                            cli.HouseUpdateArray.Clear();
                        }
                    }

                    // Parallel processing of clients
                    Parallel.ForEach(clients, client =>
                    {
                        // Check that client is healthy
                        if (client == null)
                            return;

                        GamePlayer player = client.Player;

                        if (client.ClientState == GameClient.eClientState.Playing && player == null)
                        {
                            if (log.IsErrorEnabled)
                                log.Error("account has no active player but is playing, disconnecting! => " + client.Account.Name);

                            // Disconnect buggy Client
                            GameServer.Instance.Disconnect(client);
                            return;
                        }

                        // Check that player is active.
                        if (client.ClientState != GameClient.eClientState.Playing || player == null || player.ObjectState != GameObject.eObjectState.Active)
                            return;

                        // Start Update Task
                        StartPlayerUpdateTask(client, clientsUpdateTasks, begin);
                    });

                    long took = GameTimer.GetTickCount() - begin;

                    if (took >= 500)
                    {
                        if (log.IsWarnEnabled)
                            log.WarnFormat("World Update Thread (NPC/Object update) took {0} ms", took);
                    }

                    // relaunch update thread every 100 ms to check if any player need updates.
                    Thread.Sleep((int)Math.Max(1, 100 - took));
                }
                catch (ThreadAbortException)
                {
                    if (log.IsInfoEnabled)
                        log.Info("World Update Thread stopping...");

                    running = false;
                    break;
                }
                catch (Exception e)
                {
                    if (log.IsErrorEnabled)
                        log.Error("Error in World Update (NPC/Object Update) Thread!", e);
                }
            }
        }
    }
}

