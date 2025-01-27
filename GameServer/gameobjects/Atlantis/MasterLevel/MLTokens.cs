using DOL.Database;
using DOL.Events;
using log4net;
using System;
using System.Linq;
using System.Reflection;

namespace DOL.GS.Items
{
    public class MLTokens
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        [GameServerStartedEvent]
        public static void OnServerStartup(DOLEvent e, object sender, EventArgs args)
        {
            ItemTemplate item = null;

            #region ML 1 Token

            item = GameServer.Database.FindObjectByKey<ItemTemplate>("ml1token");
            if (item == null)
            {
                item = new ItemTemplate
                {
                    Id_nb = "ml1token",
                    Name = "ML1 Credit",
                    Level = 50,
                    Item_Type = 40,
                    Model = 485,
                    IsTradable = true,
                    Object_Type = 0,
                    Quality = 100,
                    Weight = 1,
                    Price = 10000,
                    MaxCondition = 100,
                    MaxDurability = 100,
                    Condition = 100,
                    Durability = 100
                };

                GameServer.Database.AddObject(item);
                if (log.IsDebugEnabled)
                    log.Debug("Added " + item.Id_nb);
            }
            item = null;

            #endregion lesser_mythirian_of_health

            #region ML 2 Token

            item = GameServer.Database.FindObjectByKey<ItemTemplate>("ml2token");
            if (item == null)
            {
                item = new ItemTemplate
                {
                    Id_nb = "ml2token",
                    Name = "ML2 Credit",
                    Level = 50,
                    Item_Type = 40,
                    Model = 485,
                    IsTradable = true,
                    Object_Type = 0,
                    Quality = 100,
                    Weight = 1,
                    Price = 10000,
                    MaxCondition = 100,
                    MaxDurability = 100,
                    Condition = 100,
                    Durability = 100
                };

                GameServer.Database.AddObject(item);
                if (log.IsDebugEnabled)
                    log.Debug("Added " + item.Id_nb);
            }
            item = null;

            #endregion lesser_mythirian_of_power

            #region ML 3 Token

            item = GameServer.Database.FindObjectByKey<ItemTemplate>("ml3token");
            if (item == null)
            {
                item = new ItemTemplate
                {
                    Id_nb = "ml3token",
                    Name = "ML3 Credit",
                    Level = 50,
                    Item_Type = 40,
                    Model = 485,
                    IsTradable = true,
                    Object_Type = 0,
                    Quality = 100,
                    Weight = 1,
                    Price = 10000,
                    MaxCondition = 100,
                    MaxDurability = 100,
                    Condition = 100,
                    Durability = 100
                };

                GameServer.Database.AddObject(item);
                if (log.IsDebugEnabled)
                    log.Debug("Added " + item.Id_nb);
            }
            item = null;

            #endregion lesser_mythirian_of_scathing

            #region ML 4 Token

            item = GameServer.Database.FindObjectByKey<ItemTemplate>("ml4token");
            if (item == null)
            {
                item = new ItemTemplate
                {
                    Id_nb = "ml4token",
                    Name = "ML4 Credit",
                    Level = 50,
                    Item_Type = 40,
                    Model = 485,
                    IsTradable = true,
                    Object_Type = 0,
                    Quality = 100,
                    Weight = 1,
                    Price = 10000,
                    MaxCondition = 100,
                    MaxDurability = 100,
                    Condition = 100,
                    Durability = 100
                };

                GameServer.Database.AddObject(item);
                if (log.IsDebugEnabled)
                    log.Debug("Added " + item.Id_nb);
            }
            item = null;

            #endregion lesser_mythirian_of_focal

            #region ML 5 Token

            item = GameServer.Database.FindObjectByKey<ItemTemplate>("ml5token");
            if (item == null)
            {
                item = new ItemTemplate
                {
                    Id_nb = "ml5token",
                    Name = "ML5 Credit",
                    Level = 50,
                    Item_Type = 40,
                    Model = 485,
                    IsTradable = true,
                    Object_Type = 0,
                    Quality = 100,
                    Weight = 1,
                    Price = 10000,
                    MaxCondition = 100,
                    MaxDurability = 100,
                    Condition = 100,
                    Durability = 100
                };

                GameServer.Database.AddObject(item);
                if (log.IsDebugEnabled)
                    log.Debug("Added " + item.Id_nb);
            }
            item = null;

            #endregion lesser_mythirian_of_deflection

            #region ML 6 Token

            item = GameServer.Database.FindObjectByKey<ItemTemplate>("ml6token");
            if (item == null)
            {
                item = new ItemTemplate
                {
                    Id_nb = "ml6token",
                    Name = "ML6 Credit",
                    Level = 50,
                    Item_Type = 40,
                    Model = 485,
                    IsTradable = true,
                    Object_Type = 0,
                    Quality = 100,
                    Weight = 1,
                    Price = 10000,
                    MaxCondition = 100,
                    MaxDurability = 100,
                    Condition = 100,
                    Durability = 100
                };

                GameServer.Database.AddObject(item);
                if (log.IsDebugEnabled)
                    log.Debug("Added " + item.Id_nb);
            }
            item = null;

            #endregion lesser_mythirian_of_evasion

            #region ML 7 Token

            item = GameServer.Database.FindObjectByKey<ItemTemplate>("ml7token");
            if (item == null)
            {
                item = new ItemTemplate
                {
                    Id_nb = "ml7token",
                    Name = "ML7 Credit",
                    Level = 50,
                    Item_Type = 40,
                    Model = 485,
                    IsTradable = true,
                    Object_Type = 0,
                    Quality = 100,
                    Weight = 1,
                    Price = 10000,
                    MaxCondition = 100,
                    MaxDurability = 100,
                    Condition = 100,
                    Durability = 100
                };

                GameServer.Database.AddObject(item);
                if (log.IsDebugEnabled)
                    log.Debug("Added " + item.Id_nb);
            }
            item = null;

            #endregion lesser_mythirian_of_barricading

            #region ML 8 Token

            item = GameServer.Database.FindObjectByKey<ItemTemplate>("ml8token");
            if (item == null)
            {
                item = new ItemTemplate
                {
                    Id_nb = "ml8token",
                    Name = "ML8 Credit",
                    Level = 50,
                    Item_Type = 40,
                    Model = 485,
                    IsTradable = true,
                    Object_Type = 0,
                    Quality = 100,
                    Weight = 1,
                    Price = 10000,
                    MaxCondition = 100,
                    MaxDurability = 100,
                    Condition = 100,
                    Durability = 100
                };

                GameServer.Database.AddObject(item);
                if (log.IsDebugEnabled)
                    log.Debug("Added " + item.Id_nb);
            }
            item = null;

            #endregion lesser_mythirian_of_guerdon

            #region ML 9 Token

            item = GameServer.Database.FindObjectByKey<ItemTemplate>("ml9token");
            if (item == null)
            {
                item = new ItemTemplate
                {
                    Id_nb = "ml9token",
                    Name = "ML9 Credit",
                    Level = 50,
                    Item_Type = 40,
                    Model = 485,
                    IsTradable = true,
                    Object_Type = 0,
                    Quality = 100,
                    Weight = 1,
                    Price = 10000,
                    MaxCondition = 100,
                    MaxDurability = 100,
                    Condition = 100,
                    Durability = 100
                };

                GameServer.Database.AddObject(item);
                if (log.IsDebugEnabled)
                    log.Debug("Added " + item.Id_nb);
            }
            item = null;

            #endregion lesser_mythirian_of_siphoning

            #region ML 10 Token

            item = GameServer.Database.FindObjectByKey<ItemTemplate>("ml10token");
            if (item == null)
            {
                item = new ItemTemplate
                {
                    Id_nb = "ml10token",
                    Name = "ML10 Credit",
                    Level = 50,
                    Item_Type = 40,
                    Model = 485,
                    IsTradable = true,
                    Object_Type = 0,
                    Quality = 100,
                    Weight = 1,
                    Price = 10000,
                    MaxCondition = 100,
                    MaxDurability = 100,
                    Condition = 100,
                    Durability = 100
                };

                GameServer.Database.AddObject(item);
                if (log.IsDebugEnabled)
                    log.Debug("Added " + item.Id_nb);
            }
            item = null;

            #endregion lesser_adroit_mythirian

            #region ML Respec Token

            item = GameServer.Database.FindObjectByKey<ItemTemplate>("mlrespectoken");
            if (item == null)
            {
                item = new ItemTemplate
                {
                    Id_nb = "mlrespectoken",
                    Name = "ML Respec Token",
                    Level = 50,
                    Item_Type = 40,
                    Model = 485,
                    IsTradable = true,
                    Object_Type = 0,
                    Quality = 100,
                    Weight = 1,
                    Price = 10000,
                    MaxCondition = 100,
                    MaxDurability = 100,
                    Condition = 100,
                    Durability = 100
                };

                GameServer.Database.AddObject(item);
                if (log.IsDebugEnabled)
                    log.Debug("Added " + item.Id_nb);
            }
            item = null;

            #endregion lesser_insightful_mythirain
            
            MerchantItem m_item = null;
            //m_item = GameServer.Database.SelectObject<MerchantItem>("ItemListID='mltokens'");
            m_item = GameServer.Database.SelectObjects<MerchantItem>("`ItemListID` = @ItemListID", new QueryParameter("@ItemListID", "mltokens")).FirstOrDefault();
            if (m_item == null)
            {
                #region Merchant Items

                m_item = new MerchantItem
                {
                    ItemListID = "mltokens",
                    ItemTemplateID = "ml1token",
                    PageNumber = 0,
                    SlotPosition = 0,
                    AllowAdd = true
                };
                GameServer.Database.AddObject(m_item);
                item = null;

                m_item = new MerchantItem
                {
                    ItemListID = "mltokens",
                    ItemTemplateID = "ml2token",
                    PageNumber = 0,
                    SlotPosition = 1,
                    AllowAdd = true
                };
                GameServer.Database.AddObject(m_item);
                item = null;

                m_item = new MerchantItem
                {
                    ItemListID = "mltokens",
                    ItemTemplateID = "ml3token",
                    PageNumber = 0,
                    SlotPosition = 2,
                    AllowAdd = true
                };
                GameServer.Database.AddObject(m_item);
                item = null;

                m_item = new MerchantItem
                {
                    ItemListID = "mltokens",
                    ItemTemplateID = "ml4token",
                    PageNumber = 0,
                    SlotPosition = 3,
                    AllowAdd = true
                };
                GameServer.Database.AddObject(m_item);
                item = null;

                m_item = new MerchantItem
                {
                    ItemListID = "mltokens",
                    ItemTemplateID = "ml5token",
                    PageNumber = 0,
                    SlotPosition = 4,
                    AllowAdd = true
                };
                GameServer.Database.AddObject(m_item);
                item = null;

                m_item = new MerchantItem
                {
                    ItemListID = "mltokens",
                    ItemTemplateID = "ml6token",
                    PageNumber = 0,
                    SlotPosition = 5,
                    AllowAdd = true
                };
                GameServer.Database.AddObject(m_item);
                item = null;

                m_item = new MerchantItem
                {
                    ItemListID = "mltokens",
                    ItemTemplateID = "ml7token",
                    PageNumber = 0,
                    SlotPosition = 6,
                    AllowAdd = true
                };
                GameServer.Database.AddObject(m_item);
                item = null;

                m_item = new MerchantItem
                {
                    ItemListID = "mltokens",
                    ItemTemplateID = "ml8token",
                    PageNumber = 0,
                    SlotPosition = 7,
                    AllowAdd = true
                };
                GameServer.Database.AddObject(m_item);
                item = null;

                m_item = new MerchantItem
                {
                    ItemListID = "mltokens",
                    ItemTemplateID = "ml9token",
                    PageNumber = 0,
                    SlotPosition = 8,
                    AllowAdd = true
                };
                GameServer.Database.AddObject(m_item);
                item = null;

                m_item = new MerchantItem
                {
                    ItemListID = "mltokens",
                    ItemTemplateID = "ml10token",
                    PageNumber = 0,
                    SlotPosition = 9,
                    AllowAdd = true
                };
                GameServer.Database.AddObject(m_item);
                item = null;

                m_item = new MerchantItem
                {
                    ItemListID = "mltokens",
                    ItemTemplateID = "mlrespectoken",
                    PageNumber = 0,
                    SlotPosition = 10,
                    AllowAdd = true
                };
                GameServer.Database.AddObject(m_item);
                item = null;

                #endregion Merchant Items
            }
            item = null;
        }
    }
}