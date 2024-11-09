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
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DOL.GS
{

    /// <summary>
    /// A vault.
    /// </summary>
    /// <author>Aredhel, Tolakram</author>
    public class GameVault : GameStaticItem, IGameInventoryObject
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// This list holds all the players that are currently viewing
        /// the vault; it is needed to update the contents of the vault
        /// for any one observer if there is a change.
        /// </summary>
        protected readonly ConcurrentDictionary<string, GamePlayer> _observers = new ConcurrentDictionary<string, GamePlayer>();

        /// <summary>
        /// Number of items a single vault can hold.
        /// </summary>
        private const int VAULT_SIZE = 100;

        protected int m_vaultIndex;

        /// <summary>
        /// This is used to synchronize actions on the vault.
        /// </summary>
        protected object m_vaultSync = new object();

        public object LockObject()
        {
            return m_vaultSync;
        }

        /// <summary>
        /// Index of this vault.
        /// </summary>
        public int Index
        {
            get { return m_vaultIndex; }
            set { m_vaultIndex = value; }
        }

        /// <summary>
        /// Gets the number of items that can be held in the vault.
        /// </summary>
        public virtual int VaultSize
        {
            get { return VAULT_SIZE; }
        }

        /// <summary>
        /// What is the first client slot this inventory object uses? This is client window dependent, and for 
        /// housing vaults we use the housing vault window
        /// </summary>
        public virtual int FirstClientSlot
        {
            get { return (int)eInventorySlot.HousingInventory_First; }
        }

        /// <summary>
        /// Last slot of the client window that shows this inventory
        /// </summary>
        public int LastClientSlot
        {
            get { return (int)eInventorySlot.HousingInventory_Last; }
        }

        /// <summary>
        /// First slot in the DB.
        /// </summary>
        public virtual int FirstDBSlot
        {
            get { return (int)(eInventorySlot.HouseVault_First) + VaultSize * Index; }
        }

        /// <summary>
        /// Last slot in the DB.
        /// </summary>
        public virtual int LastDBSlot
        {
            get { return (int)(eInventorySlot.HouseVault_First) + VaultSize * (Index + 1) - 1; }
        }

        public virtual string GetOwner(GamePlayer player = null)
        {
            if (player == null)
            {
                log.Error("GameVault GetOwner(): player cannot be null!");
                return "PlayerIsNullError";
            }

            return player.InternalID;
        }

        /// <summary>
        /// Do we handle a search?
        /// </summary>
        public bool SearchInventory(GamePlayer player, MarketSearch.SearchData searchData)
        {
            return false; // not applicable
        }

        /// <summary>
        /// Inventory for this vault.
        /// </summary>
        public virtual ConcurrentDictionary<int, InventoryItem> GetClientInventory(GamePlayer player)
        {
            var inventory = new ConcurrentDictionary<int, InventoryItem>();
            int slotOffset = -FirstDBSlot + (int)(eInventorySlot.HousingInventory_First);

            if (player != null)
            {
                // Sicherstellen, dass die DBItems nicht null ist, bevor die Schleife ausgef�hrt wird.
                var dbItems = DBItems(player);
                if (dbItems != null) // Null-Pr�fung hinzugef�gt.
                {
                    foreach (InventoryItem item in dbItems)
                    {
                        if (item != null)
                        {
                            if (!inventory.ContainsKey(item.SlotPosition + slotOffset))
                            {
                                inventory.TryAdd(item.SlotPosition + slotOffset, item);
                            }
                            else
                            {
                                try
                                {
                                    // �berpr�fung, ob die Zielposition nicht null ist
                                    if (inventory.ContainsKey((int)item.SlotPosition + slotOffset))
                                    {
                                        InventoryItem toItem = inventory[(int)item.SlotPosition + slotOffset];
                                        if (toItem != null)
                                        {
                                            // Sicherstellen, dass das Inventar des Spielers nicht null ist
                                            if (player.Inventory != null)
                                            {
                                                player.Inventory.AddTradeItem(eInventorySlot.FirstEmptyBackpack, toItem);
                                            }
                                            player.Out.SendInventorySlotsUpdate(null);
                                            player.Out.SendMessage("There is already another item in this slot!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                        }
                                    }
                                }
                                catch
                                {
                                    log.ErrorFormat("GAMEVAULT: Duplicate item: {0}, owner: {1}", item.Name, player.OwnerID);
                                }
                            }
                        }
                    }
                }
                return inventory;
            }
            return null;
        }
        /// <summary>
        /// Player interacting with this vault.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;

            if (player == null) // Null-Pr�fung f�r player hinzugef�gt.
            {
                log.Error("Player cannot be null when interacting with vault.");
                return false;
            }

            if (!CanView(player))
            {
                player.Out?.SendMessage("You don't have permission to view this vault!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            // Es muss sichergestellt werden, dass GetClientInventory nicht null zur�ckgibt
            var clientInventory = GetClientInventory(player);
            if (clientInventory == null)
            {
                log.Error("GameVault Interact: No items found in Vault ");
                return false;
            }

            player.ActiveInventoryObject = this;
            player.Out.SendInventoryItemsUpdate(clientInventory, eInventoryWindowType.HouseVault);

            if (player.ActiveInventoryObject != null)
            {
                player.ActiveInventoryObject.RemoveObserver(player);
            }

            return true;
        }

        /// <summary>
        /// List of items in the vault.
        /// </summary>
        public IList<InventoryItem> DBItems(GamePlayer player)
        {
            if (player != null && string.IsNullOrEmpty(GetOwner(player)) == false)
            {

                return GameServer.Database.SelectObjects<InventoryItem>("`OwnerID` = @OwnerID and `SlotPosition` >= @FirstDBSlot and `SlotPosition` <= @LastDBSlot",
                                                                         new[] { new QueryParameter("@OwnerID", GetOwner(player)), new QueryParameter("@FirstDBSlot", FirstDBSlot), new QueryParameter("@LastDBSlot", LastDBSlot) });


            }
            return null;
        }

        /// <summary>
        /// Is this a move request for a housing vault?
        /// </summary>
        /// <param name="player"></param>
        /// <param name="fromSlot"></param>
        /// <param name="toSlot"></param>
        /// <returns></returns>
        public virtual bool CanHandleMove(GamePlayer player, ushort fromSlot, ushort toSlot)
        {
            if (player == null || player.ActiveInventoryObject != this)
                return false;

            bool canHandle = false;

            // House Vaults and GameConsignmentMerchant Merchants deliver the same slot numbers
            if (fromSlot >= (ushort)eInventorySlot.HousingInventory_First &&
                fromSlot <= (ushort)eInventorySlot.HousingInventory_Last)
            {
                canHandle = true;
            }
            else if (toSlot >= (ushort)eInventorySlot.HousingInventory_First &&
                toSlot <= (ushort)eInventorySlot.HousingInventory_Last)
            {
                canHandle = true;
            }

            return canHandle;
        }

        /// <summary>
        /// Move an item from, to or inside a house vault.  From IGameInventoryObject
        /// </summary>
        public virtual bool MoveItem(GamePlayer player, ushort fromSlot, ushort toSlot)
        {
            if (fromSlot == toSlot || player == null)
            {
                return false;
            }

            bool fromHousing = (fromSlot >= (ushort)eInventorySlot.HousingInventory_First && fromSlot <= (ushort)eInventorySlot.HousingInventory_Last);
            bool toHousing = (toSlot >= (ushort)eInventorySlot.HousingInventory_First && toSlot <= (ushort)eInventorySlot.HousingInventory_Last);

            if (fromHousing == false && toHousing == false)
            {
                return false;
            }

            if (!(player.ActiveInventoryObject is GameVault gameVault))
            {
                player.Out.SendMessage("You are not actively viewing a vault!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                player.Out.SendInventoryItemsUpdate(null);
                return false;
            }

            if (toHousing && gameVault.CanAddItems(player) == false)
            {
                player.Out.SendMessage("You don't have permission to add items!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (fromHousing && gameVault.CanRemoveItems(player) == false)
            {
                player.Out.SendMessage("You don't have permission to remove items!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            InventoryItem itemInFromSlot = player.Inventory.GetItem((eInventorySlot)fromSlot);
            InventoryItem itemInToSlot = player.Inventory.GetItem((eInventorySlot)toSlot);

            // Check for a swap to get around not allowing non-tradables in a housing vault - Tolakram
            if (fromHousing && itemInToSlot != null && itemInToSlot.IsTradable == false)
            {
                player.Out.SendMessage("You can not swap with an untradable item!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                // log.DebugFormat("GameVault: {0} attempted to swap untradable item {2} with {1}", player.Name, itemInFromSlot.Name, itemInToSlot.Name);
                player.Out.SendInventoryItemsUpdate(null);
                return false;
            }



            // Allow people to get untradables out of their house vaults (old bug) but 
            // block placing untradables into housing vaults from any source - Tolakram
            if (toHousing && itemInFromSlot != null && itemInFromSlot.IsTradable == false)
            {
                player.Out.SendMessage("You can not put this item into a House Vault!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                player.Out.SendInventoryItemsUpdate(null);
                return false;
            }

            // let's move it

            lock (m_vaultSync)
            {
                if (fromHousing)
                {
                    if (toHousing)
                    {
                        NotifyObservers(player, this.MoveItemInsideObject(player, (eInventorySlot)fromSlot, (eInventorySlot)toSlot));
                    }
                    else
                    {
                        NotifyObservers(player, this.MoveItemFromObject(player, (eInventorySlot)fromSlot, (eInventorySlot)toSlot));
                    }
                }
                else if (toHousing)
                {
                    ////
                    NotifyObservers(player, this.MoveItemToObject(player, (eInventorySlot)fromSlot, (eInventorySlot)toSlot));
                }
            }

            return true;
        }

        /// <summary>
        /// Add an item to this object
        /// </summary>
        public virtual bool OnAddItem(GamePlayer player, InventoryItem item)
        {
            return false;
        }

        /// <summary>
        /// Remove an item from this object
        /// </summary>
        public virtual bool OnRemoveItem(GamePlayer player, InventoryItem item)
        {
            return false;
        }


        /// <summary>
        /// Not applicable for vaults
        /// </summary>
        public virtual bool SetSellPrice(GamePlayer player, ushort clientSlot, uint price)
        {
            return false;
        }


        /// <summary>
        /// Send inventory updates to all players actively viewing this vault;
        /// players that are too far away will be considered inactive.
        /// </summary>
        /// <param name="updateItems"></param>
        protected virtual void NotifyObservers(GamePlayer player, IDictionary<int, InventoryItem> updateItems)
        {
            // Null-Pr�fungen f�r player.Client und player.Client.Out hinzuf�gen
            if (player?.Client?.Out != null)
            {
                player.Client.Out.SendInventoryItemsUpdate(updateItems, eInventoryWindowType.Update);
            }
            else
            {
                log.Error("NotifyObservers: Player's client or output is null!");
            }
        }

        /// <summary>
        /// Whether or not this player can view the contents of this vault.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public virtual bool CanView(GamePlayer player)
        {
            return true;
        }

        /// <summary>
        /// Whether or not this player can move items inside the vault
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public virtual bool CanAddItems(GamePlayer player)
        {
            return true;
        }

        /// <summary>
        /// Whether or not this player can move items inside the vault
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public virtual bool CanRemoveItems(GamePlayer player)
        {
            return true;
        }
        public virtual void AddObserver(GamePlayer player)
        {
            try
            {
                if (_observers.ContainsKey(player.Name) == false)
                {
                    _observers.TryAdd(player.Name, player);
                }
            }
            catch { log.Error("missing observer in GameVault"); }
        }
        public virtual void RemoveObserver(GamePlayer player)
        {
            try
            {
                if (_observers.ContainsKey(player.Name))
                {
                    _observers.TryRemove(player.Name, out player);
                }
            }
            catch { }
        }
    }
}