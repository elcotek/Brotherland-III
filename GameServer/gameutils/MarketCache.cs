using DOL.Database;
using log4net;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DOL.GS
{
    public class MarketCache
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static ConcurrentDictionary<string, InventoryItem> m_itemCache = new ConcurrentDictionary<string, InventoryItem>();

        /// <summary>
        /// Return a List of all items in the cache
        /// </summary>
        /// </summary>
        public static List<InventoryItem> GetItems()
        {
            return new List<InventoryItem>(m_itemCache.Values);
        }

        /// <summary>
        /// Load or reload all items into the market cache
        /// </summary>
        public static bool GetInitialize()
        {
            log.Info("Building Market Cache ....");
            try
            {
                IList<InventoryItem> list = GameServer.Database.SelectObjects<InventoryItem>(
                    "`SlotPosition` >= @SlotPositionFirst AND `SlotPosition` <= @SlotPositionLast AND `OwnerLot` > @OwnerLot",
                    new[]
                    {
                        new QueryParameter("@SlotPositionFirst", (int)eInventorySlot.Consignment_First),
                        new QueryParameter("@SlotPositionLast", (int)eInventorySlot.Consignment_Last),
                        new QueryParameter("@OwnerLot", 0)
                    });

                foreach (InventoryItem item in list)
                {
                    var playerItem = GameInventoryItem.Create(item);
                    m_itemCache.TryAdd(item.ObjectId, playerItem);
                }

                log.Info("Market Cache initialized with " + m_itemCache.Count + " items.");
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Add an item to the market cache
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool AddItem(InventoryItem item)
        {
            if (item != null && item.OwnerID != null)
            {
                return m_itemCache.TryAdd(item.ObjectId, item);
            }
            return false;
        }

        /// <summary>
        /// Remove an item from the market cache
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool RemoveItem(InventoryItem item)
        {
            if (item != null)
            {
                return m_itemCache.TryRemove(item.ObjectId, out _);
            }
            return false;
        }
    }
}
