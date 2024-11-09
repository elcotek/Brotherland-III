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
 */

using DOL.Database;
using DOL.GS.Housing;
using DOL.GS.PacketHandler;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
// Tolakram - January 7, 2012

namespace DOL.GS
{
    /// <summary>
    /// Interface for a GameInventoryObject
    /// This is an object or NPC that can interact with a players inventory, buy, or sell items
    /// </summary>		
    public interface IGameInventoryObject
    {
        object LockObject();

        int FirstClientSlot { get; }
        int LastClientSlot { get; }
        int FirstDBSlot { get; }
        int LastDBSlot { get; }
        string GetOwner(GamePlayer player);
        IList<InventoryItem> DBItems(GamePlayer player = null);
        ConcurrentDictionary<int, InventoryItem> GetClientInventory(GamePlayer player);
        bool CanHandleMove(GamePlayer player, ushort fromClientSlot, ushort toClientSlot);
        bool MoveItem(GamePlayer player, ushort fromClientSlot, ushort toClientSlot);
        bool OnAddItem(GamePlayer player, InventoryItem item);
        bool OnRemoveItem(GamePlayer player, InventoryItem item);
        bool SetSellPrice(GamePlayer player, ushort clientSlot, uint sellPrice);
        bool SearchInventory(GamePlayer player, MarketSearch.SearchData searchData);
        void AddObserver(GamePlayer player);
 	    void RemoveObserver(GamePlayer player);
    }

    /// <summary>
    /// This is an extension class for GameInventoryObjects.  It's a way to get around the fact C# doesn't support multiple inheritance. 
    /// We want the ability for a GameInventoryObject to be a game static object, or an NPC, or anything else, and yet still contain common functionality 
    /// for an inventory object with code written in just one place
    /// </summary>
    public static class GameInventoryObjectExtensions
    {
        public const string ITEM_BEING_ADDED = "ItemBeingAddedToObject";
        public const string TEMP_SEARCH_KEY = "TempSearchKey";

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Can this object handle the move request?
        /// </summary>
        public static bool CanHandleRequest(this IGameInventoryObject thisObject, GamePlayer player, ushort fromClientSlot, ushort toClientSlot)
        {
            // make sure from or to slots involve this object
            if ((fromClientSlot >= thisObject.FirstClientSlot && fromClientSlot <= thisObject.LastClientSlot) ||
                (toClientSlot >= thisObject.FirstClientSlot && toClientSlot <= thisObject.LastClientSlot))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the items of this object, mapped to the client inventory slots
        /// </summary>
        public static Dictionary<int, InventoryItem> GetClientItems(this IGameInventoryObject thisObject, GamePlayer player)
        {
            var inventory = new Dictionary<int, InventoryItem>();
            int slotOffset = thisObject.FirstClientSlot - thisObject.FirstDBSlot;

            // Überprüfung, ob der Spieler null ist
            if (player == null)
            {
                log.Error("Player is null, cannot get client items.");
                return inventory;  // Leeres Dictionary zurückgeben
            }

            foreach (InventoryItem item in thisObject.DBItems(player))
            {
                if (item != null)
                {
                    if (!inventory.ContainsKey(item.SlotPosition + slotOffset))
                    {
                        inventory.Add(item.SlotPosition + slotOffset, item);
                    }
                }
            }

            return inventory;
        }


        /// <summary>
        /// Move an item from the inventory object to a player's backpack (uses client slots)
        /// </summary>
        public static IDictionary<int, InventoryItem> MoveItemFromObject(this IGameInventoryObject thisObject, GamePlayer player, eInventorySlot fromClientSlot, eInventorySlot toClientSlot)
        {
            // We will only allow moving to the backpack.

            if (toClientSlot < eInventorySlot.FirstBackpack || toClientSlot > eInventorySlot.LastBackpack)
                return null;

            lock (thisObject.LockObject())
            {
                ConcurrentDictionary<int, InventoryItem> inventory = thisObject.GetClientInventory(player);

                // Überprüfung, ob der Spieler null ist
                if (player == null)
                {
                    log.Error("Player is null during MoveItemFromObject");
                    return null;  // Rückgabe null, wenn der Spieler ungültig ist
                }

                if (inventory.ContainsKey((int)fromClientSlot) == false)
                {
                    ChatUtil.SendErrorMessage(player, "Item not found in slot " + (int)fromClientSlot);
                    return null;
                }

                InventoryItem fromItem = inventory[(int)fromClientSlot];
                InventoryItem toItem = player.Inventory.GetItem(toClientSlot);
                GameConsignmentMerchant conMerchant = null;


                // if there is an item in the players target inventory slot then move it to the object
                if (toItem != null)
                {


                    if (player.TargetObject is GameConsignmentMerchant)
                    {
                        conMerchant = player.ActiveInventoryObject as GameConsignmentMerchant;

                        if (conMerchant != null)
                        {
                            if (player.IsWithinRadius(conMerchant, WorldMgr.INTERACT_DISTANCE) == false)
                                return null;
                        }
                    }
                }
                
                // if there is an item in the players target inventory slot then move it to the object
                if (toItem != null)
                {

                        player.Inventory.RemoveTradeItem(toItem);
                    toItem.SlotPosition = fromItem.SlotPosition;
                    toItem.OwnerID = thisObject.GetOwner(player);
                    thisObject.OnAddItem(player, toItem);
                    if (MarketCache.GetItems().Contains(toItem))
                    {
                        toItem.OwnerLot = 0;
                        MarketCache.RemoveItem(toItem);
                    }
                    GameServer.Database.SaveObject(toItem);
                }

                thisObject.OnRemoveItem(player, fromItem);

                // Create the GameInventoryItem from this InventoryItem.  This simply wraps the InventoryItem, 
                // which is still updated when this item is moved around
                InventoryItem objectItem = GameInventoryItem.Create<InventoryItem>(fromItem);

                player.Inventory.AddTradeItem(toClientSlot, objectItem);

                var updateItems = new Dictionary<int, InventoryItem>(1)
                {
                    { (int)fromClientSlot, toItem }
                };





                return updateItems;
            }
        }
        /// <summary>
        /// Move an item from a player's backpack to this inventory object (uses client slots)
        /// </summary>
        public static IDictionary<int, InventoryItem> MoveItemToObject(this IGameInventoryObject thisObject, GamePlayer player, eInventorySlot fromClientSlot, eInventorySlot toClientSlot)
        {
            // We will only allow moving from the backpack.

            // Sicherstellen, dass der Spieler nicht null ist
            if (player == null)
            {
                log.Error("Player is null during MoveItemToObject");
                return null;  // Rückgabe null, wenn der Spieler ungültig ist
            }


            if (fromClientSlot < eInventorySlot.FirstBackpack || fromClientSlot > eInventorySlot.LastBackpack)
                return null;

            InventoryItem fromItem = player.Inventory.GetItem(fromClientSlot);

            if (fromItem == null)
                return null;

            if (fromItem.IsTradable == false)
            {
                player.Out.SendMessage(fromItem.GetName(0, true) + " This item can not be store here!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return null;
            }

            if (fromItem.IsDropable == false)
            {
                player.Out.SendMessage(fromItem.GetName(0, true) + " This item can not be store here!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return null;
            }
            if (player.TempProperties.getProperty<string>(House.House_Owner_ID) == null)
            {
               // player.Out.SendMessage("No owner found for this Vault, send a GM for Help!!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
               //test raus
            }
           

            GameConsignmentMerchant conMerchant;

            // if there is an item in the players target inventory slot then move it to the object
            if (fromItem != null)
            {


                if (player.TargetObject is GameConsignmentMerchant)
                {
                    conMerchant = player.ActiveInventoryObject as GameConsignmentMerchant;

                    if (conMerchant != null)
                    {
                        if (player.IsWithinRadius(conMerchant, WorldMgr.INTERACT_DISTANCE) == false)
                            return null;
                    }
                }
            }



            lock (thisObject.LockObject())
            {
                ConcurrentDictionary<int, InventoryItem> inventory = thisObject.GetClientInventory(player);

                player.Inventory.RemoveTradeItem(fromItem);

                // if there is an item in the objects target slot then move it to the players inventory
                if (inventory.ContainsKey((int)toClientSlot))
                {
                    InventoryItem toItem = inventory[(int)toClientSlot];
                    thisObject.OnRemoveItem(player, toItem);
                    player.Inventory.AddTradeItem(fromClientSlot, toItem);
                }

                /*
                ArrayList houses = null;
                House Currenthouse = null;
                if (player.CurrentRegion.IsHousing)
                {
                    
                    //player OwnerID wechelt zum house owner!
                    houses = (ArrayList)HouseMgr.GetHousesCloseToSpot(player.CurrentRegionID, player.X, player.Y, 700);
                }


                if (houses.Count > 0 && (player.TargetObject is GameConsignmentMerchant))
                {
                   if (string.IsNullOrEmpty(player.TempProperties.getProperty<string>(GameConsignmentMerchant.ConsgnmentHouse_Owner_ID)) == false)
                    Currenthouse = HouseMgr.GetHouse(player.CurrentRegionID, (houses[0] as House).HouseNumber);

                }
                */
                if (player.TargetObject  != null &player.TargetObject is GameConsignmentMerchant && string.IsNullOrEmpty(player.TempProperties.getProperty<string>(GameConsignmentMerchant.ConsgnmentHouse_Owner_ID)) == false)
                {
                    //log.ErrorFormat("Consignment ownerID = {0}", player.TempProperties.getProperty<string>(GameConsignmentMerchant.ConsgnmentHouse_Owner_ID));
                    fromItem.OwnerID = player.TempProperties.getProperty<string>(GameConsignmentMerchant.ConsgnmentHouse_Owner_ID);
                    player.TempProperties.removeProperty(GameConsignmentMerchant.ConsgnmentHouse_Owner_ID);
                   // log.Debug("Item Owner ist nun der Consignment besitzer");
                }


                   
                if (player.TargetObject != null && player.TargetObject is GameHouseVault && string.IsNullOrEmpty(player.TempProperties.getProperty<string>(House.House_Owner_ID)) == false)
                {

                    // log.DebugFormat("fromItem.OwnerID = {0}", player.TempProperties.getProperty<string>(House.House_Owner_ID));
                    fromItem.OwnerID = player.TempProperties.getProperty<string>(House.House_Owner_ID); // Currenthouse.OwnerID;
                    player.TempProperties.removeProperty(House.House_Owner_ID);
                    //log.Debug("Item Owner ist nun der haus besitzer");
                }
                /*
                else if (Currenthouse != null)
                {
                    fromItem.OwnerID = Currenthouse.OwnerID;
                }
                */
                if (fromItem.OwnerID == null)
                {
                   // log.DebugFormat("fromItem.OwnerID = {0}", thisObject.GetOwner(player));
                    fromItem.OwnerID = thisObject.GetOwner(player);
                   // log.Debug("Owner ist der player geblieben");
                }
            }


            fromItem.SlotPosition = (int)(toClientSlot) - (int)(thisObject.FirstClientSlot) + thisObject.FirstDBSlot;
            thisObject.OnAddItem(player, fromItem);
            GameServer.Database.SaveObject(fromItem);

            var updateItems = new Dictionary<int, InventoryItem>(1)
            {
                { (int)toClientSlot, fromItem }
            };

            // for objects that support doing something when added (setting a price, for example)
            player.TempProperties.setProperty(ITEM_BEING_ADDED, fromItem);

            return updateItems;
        }

        /// <summary>
        /// Move an item around inside this object (uses client slots)
        /// </summary>
        public static IDictionary<int, InventoryItem> MoveItemInsideObject(this IGameInventoryObject thisObject, GamePlayer player, eInventorySlot fromSlot, eInventorySlot toSlot)
        {
            lock (thisObject.LockObject())
            {
                IDictionary<int, InventoryItem> inventory = thisObject.GetClientInventory(player);

                if (!inventory.ContainsKey((int)fromSlot))
                    return null;

                var updateItems = new Dictionary<int, InventoryItem>(2);
                InventoryItem fromItem = null, toItem = null;

                fromItem = inventory[(int)fromSlot];

                if (inventory.ContainsKey((int)toSlot))
                {
                    toItem = inventory[(int)toSlot];
                    toItem.SlotPosition = fromItem.SlotPosition;

                    GameServer.Database.SaveObject(toItem);
                }

                fromItem.SlotPosition = (int)toSlot - (int)(thisObject.FirstClientSlot) + thisObject.FirstDBSlot;
                GameServer.Database.SaveObject(fromItem);

                updateItems.Add((int)fromSlot, toItem);
                updateItems.Add((int)toSlot, fromItem);

                return updateItems;
            }
        }
    }
}
