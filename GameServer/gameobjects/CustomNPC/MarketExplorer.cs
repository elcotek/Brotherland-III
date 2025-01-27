using DOL.Database;
using DOL.GS.Housing;
using DOL.GS.PacketHandler;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DOL.GS
{
    public class MarketExplorer : GameNPC, IGameInventoryObject
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public const string EXPLORER_ITEM_LIST = "MarketExplorerItems";

        public object LockObject()
        {
            return new object(); // not applicable for a Market Explorer
        }

        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;

            if (player.ActiveInventoryObject != null)
            {
                player.ActiveInventoryObject.RemoveObserver(player);
                player.ActiveInventoryObject = null;
            }

            if (ServerProperties.Properties.MARKET_ENABLE)
            {
                player.ActiveInventoryObject = this;
                player.Out.SendMarketExplorerWindow();
            }
            else
            {
                player.Out.SendMessage("Sorry, the market is not available at this time.", eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
            }
            return true;
        }

        public virtual string GetOwner(GamePlayer player)
        {
            return player.InternalID;
        }

        public virtual ConcurrentDictionary<int, InventoryItem> GetClientInventory(GamePlayer player)
        {
            return null; // we don't have any inventory
        }

        /// <summary>
        /// List of items in this objects inventory
        /// </summary>
        public virtual IList<InventoryItem> DBItems(GamePlayer player = null)
        {
            return MarketCache.GetItems();
        }

        /// <summary>
        /// First slot of the client window that shows this inventory
        /// </summary>
        public virtual int FirstClientSlot
        {
            get { return (int)eInventorySlot.MarketExplorerFirst; }
        }

        /// <summary>
        /// Last slot of the client window that shows this inventory
        /// </summary>
        public virtual int LastClientSlot
        {
            get { return (int)eInventorySlot.MarketExplorerFirst + 39; } // not really sure
        }

        /// <summary>
        /// First slot in the DB.
        /// </summary>
        public virtual int FirstDBSlot
        {
            get { return (int)eInventorySlot.Consignment_First; } // not used
        }

        /// <summary>
        /// Last slot in the DB.
        /// </summary>
        public virtual int LastDBSlot
        {
            get { return (int)eInventorySlot.Consignment_Last; } // not used
        }
        protected GameConsignmentMerchant hasCM = null;

        /// <summary>
        /// Search the MarketCache
        /// </summary>
        public virtual bool SearchInventory(GamePlayer player, MarketSearch.SearchData searchData)
        {
            MarketSearch marketSearch = new MarketSearch(player);
            List<InventoryItem> items = marketSearch.FindItemsInList(DBItems(), searchData);

            if (items != null)
            {

                int maxPerPage = 20;
                byte maxPages = (byte)(Math.Ceiling((double)items.Count / (double)maxPerPage - 1));

                //log.DebugFormat("Count: {0}  maxPerPage; {1} maxPages gesammt: {2}", (double)items.Count, (double)maxPerPage, maxPages);
                int first = (searchData.page) * (maxPerPage -1);//sucher f�ngt ab 0 an deshalb maxpage -1
                int last = first + maxPerPage -1;
                List<InventoryItem> list = new List<InventoryItem>();
                int index = 0;

                lock (((ICollection)list).SyncRoot)
                {
                    foreach (InventoryItem item in items)
                    {
                        if (item != null && item.OwnerLot > 0)
                        {
                            GameConsignmentMerchant cm = HouseMgr.GetConsignmentByHouseNumber((int)item.OwnerLot);
                            House house = HouseMgr.GetHouse(item.OwnerLot);

                            //Ist ein Haus vorhanden und ist ein consignment Merchant vorhanden ? Aber CM = null ?
                            if ((hasCM == null || cm == null) && house != null && house.ConsignmentMerchant != null)
                            {
                                ConsignmentReload(player.Realm, "Consignment Merchant", item);
                                // return false;
                            }

                            //List only items if there a consignment merchant
                            if (hasCM != null || cm != null)
                            {
                                try
                                {
                                    if (index >= first && index <= last)
                                        list.Add(item);
                                    index++;
                                }

                                catch { log.Error("Error: SearchInventory: item was null, at item search!"); }
                            }
                        }
                    }

                    if (items.Count == 0)   // No items returned, let the client know
                    {
                        player.Out.SendMarketExplorerWindow(list, 0, 0);
                        ConsignmentReload(player.Realm, "Consignment Merchant", null);
                    }
                    else if ((int)searchData.page <= (int)maxPages) //Don't let us tell the client about any more than the max pages
                    {
                       player.Out.SendMarketExplorerWindow(list, searchData.page, maxPages);
                    }

                    // Save the last search list in case we buy an item from it
                    player.TempProperties.setProperty(EXPLORER_ITEM_LIST, list);
                }
            }
            return true;
        }

        /// <summary>
        /// Is this a move request for a market explorer
        /// </summary>
        /// <param name="player"></param>
        /// <param name="fromClientSlot"></param>
        /// <param name="toClientSlot"></param>
        /// <returns></returns>
        public virtual bool CanHandleMove(GamePlayer player, ushort fromClientSlot, ushort toClientSlot)
        {
            if (player == null || player.ActiveInventoryObject != this)
                return false;

            bool canHandle = false;

            if (fromClientSlot >= FirstClientSlot && toClientSlot >= (int)eInventorySlot.FirstBackpack && toClientSlot <= (ushort)eInventorySlot.LastBackpack)
            {
                // buy request
                canHandle = true;
            }

            return canHandle;
        }

        /// <summary>
        /// Move Item from MarketExplorer
        /// </summary>
        /// <param name="player"></param>
        /// <param name="fromClientSlot"></param>
        /// <param name="toClientSlot"></param>
        /// <returns></returns>
        public virtual bool MoveItem(GamePlayer player, ushort fromClientSlot, ushort toClientSlot)
        {
            IList<InventoryItem> list = null;
            // this move represents a buy item request
            if (fromClientSlot >= (ushort)eInventorySlot.MarketExplorerFirst &&
                toClientSlot >= (ushort)eInventorySlot.FirstBackpack &&
                toClientSlot <= (ushort)eInventorySlot.LastBackpack &&
                player.ActiveInventoryObject == this)
            {
                if (player != null && player.TempProperties.getProperty<List<InventoryItem>>(EXPLORER_ITEM_LIST, null) != null)
                {
                    list = player.TempProperties.getProperty<List<InventoryItem>>(EXPLORER_ITEM_LIST, null);

                    int itemSlot = fromClientSlot - (ushort)eInventorySlot.MarketExplorerFirst;


                    //7
                    InventoryItem item = list[itemSlot];

                    if (item != null && player != null && player.ObjectState == GameObject.eObjectState.Active)
                    {
                        
                        log.ErrorFormat("item: {0}",  item.Name);
                        BuyItem(item, player);
                        return true;
                    }
                    else
                        return false;
                }

            }
            return false;
        }

        /// <summary>
        /// Add an item to this object
        /// </summary>
        public virtual bool OnAddItem(GamePlayer player, InventoryItem item)
        {
            return false;
        }

        /// <summary>
        /// Not applicable
        /// </summary>
        public virtual bool SetSellPrice(GamePlayer player, ushort clientSlot, uint price)
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


        //protected GameConsignmentMerchant hasCM = null;

        /// <summary>
        /// Consignment reload if cm == null
        /// </summary>
        /// <param realm="Realm"></param>
        /// <param name="Consignment Merchant"></param>
        /// <param name="item"></param>
        public virtual void ConsignmentReload(eRealm realm, string name, InventoryItem item)
        {
            foreach (GameNPC mob in WorldMgr.GetNPCsByName(name, realm))
            {
                if (!mob.LoadedFromScript)
                {
                    if (mob.Name == name)
                    {
                        mob.RemoveFromWorld();

                        Mob mobs = GameServer.Database.FindObjectByKey<Mob>(mob.InternalID);
                        if (mobs != null)
                        {
                            mob.LoadFromDatabase(mobs);
                            mob.AddToWorld();

                        }
                        if (item != null)
                        {
                            GameConsignmentMerchant cm = HouseMgr.GetConsignmentByHouseNumber((int)item.OwnerLot);

                            if (cm != null)
                                hasCM = cm;
                        }
                    }
                }
            }
        }

        public virtual void BuyItem(InventoryItem item, GamePlayer player)
        {
            if (item.OwnerLot > 0 && item != null && player != null)
            {
                GameConsignmentMerchant cm = HouseMgr.GetConsignmentByHouseNumber((int)item.OwnerLot);

                if (cm == null)
                {
                    ConsignmentReload(player.Realm, "Consignment Merchant", item);


                    if (hasCM == null)
                    {


                        player.Out.SendMessage("I can't find the consigmnent merchant for this item!", eChatType.CT_Merchant, eChatLoc.CL_ChatWindow);
                        log.ErrorFormat("ME: Error finding consignment merchant for lot {0}", item.OwnerLot);
                        return;
                    }
                    else

                    if (player.ActiveInventoryObject != null)
                    {
                        player.ActiveInventoryObject.RemoveObserver(player);
                    }

                    player.ActiveInventoryObject = hasCM; // activate the target con merchant
                    player.Out.SendInventoryItemsUpdate(hasCM.GetClientInventory(player), eInventoryWindowType.ConsignmentViewer);
                    hasCM.AddObserver(player);



                }
                else

                if (player.ActiveInventoryObject != null)
                {
                    player.ActiveInventoryObject.RemoveObserver(player);
                }

                player.ActiveInventoryObject = cm; // activate the target con merchant
                player.Out.SendInventoryItemsUpdate(cm.GetClientInventory(player), eInventoryWindowType.ConsignmentViewer);
                cm.AddObserver(player);
            }
        }

        public virtual void AddObserver(GamePlayer player)
        {
            // not applicable
        }

        public virtual void RemoveObserver(GamePlayer player)
        {
            // not applicable
        }
    }
}
