using DOL.AI.Brain;
using DOL.Events;
using log4net;
using System;
using System.Reflection;

namespace DOL.GS
{
    public class MLTokens : GameBountyMerchant
    {
        private static MLTokens Albion_MLTokensNPC = null;
        private static MLTokens Midgard_MLTokensNPC = null;
        private static MLTokens Hibernia_MLTokensNPC = null;

        #region Constructor

        public MLTokens()
            : base()
        {
            SetOwnBrain(new BlankBrain());
        }

        #endregion Constructor

        #region AddToWorld

        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        public static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [ScriptLoadedEvent]
        public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
        {
            GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
            // template.AddNPCEquipment(eInventorySlot.Cloak, 1722);
            // template.AddNPCEquipment(eInventorySlot.TorsoArmor, 2160);
            //template = template.CloseTemplate();

            Albion_MLTokensNPC = new MLTokens
            {
                Model = 6,
                Name = "Albion ML Tokens",
                GuildName = "ML Bounty Merchant",
                Realm = (eRealm)1,
                CurrentRegionID = 70,
                Size = 50,
                Level = 50,
                MaxSpeedBase = 0,
                //Albion_MLTokensNPC.X = 578466;
                //Albion_MLTokensNPC.Y = 533696;
                //  Albion_MLTokensNPC.Z = 7295;
                //Albion_MLTokensNPC.Heading = 1871;
                Inventory = template,
                TradeItems = new MerchantTradeItems("mltokens")
            };
            //  Albion_MLTokensNPC.AddToWorld();
            Albion_MLTokensNPC.SwitchWeapon(eActiveWeaponSlot.TwoHanded);

            Midgard_MLTokensNPC = new MLTokens
            {
                Model = 217,
                Name = "Midgard ML Tokens",
                GuildName = "ML Bounty Merchant",
                Realm = (eRealm)2,
                CurrentRegionID = 71,
                Size = 50,
                Level = 50,
                MaxSpeedBase = 0,
                // Midgard_MLTokensNPC.X = 565204;
                // Midgard_MLTokensNPC.Y = 569992;
                // Midgard_MLTokensNPC.Z = 7255;
                // Midgard_MLTokensNPC.Heading = 2967;
                Inventory = template,
                TradeItems = new MerchantTradeItems("mltokens")
            };
            //  Midgard_MLTokensNPC.AddToWorld();
            Midgard_MLTokensNPC.SwitchWeapon(eActiveWeaponSlot.TwoHanded);

            Hibernia_MLTokensNPC = new MLTokens
            {
                Model = 389,
                Name = "Hibernia ML Tokens",
                GuildName = "ML Bounty Merchant",
                Realm = (eRealm)3,
                CurrentRegionID = 72,
                Size = 50,
                Level = 50,
                MaxSpeedBase = 0,
                // Hibernia_MLTokensNPC.X = 551505;
                // Hibernia_MLTokensNPC.Y = 576910;
                // Hibernia_MLTokensNPC.Z = 6767;
                // Hibernia_MLTokensNPC.Heading = 2844;
                Inventory = template,
                TradeItems = new MerchantTradeItems("mltokens")
            };
            // Hibernia_MLTokensNPC.AddToWorld();
            Hibernia_MLTokensNPC.SwitchWeapon(eActiveWeaponSlot.TwoHanded);

            if (log.IsInfoEnabled)
                log.Info("ML Token NPCs Loaded");
        }

        [ScriptUnloadedEvent]
        public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
        {
           if (Albion_MLTokensNPC != null)
            Albion_MLTokensNPC.Delete();
            if (Midgard_MLTokensNPC != null)
                Midgard_MLTokensNPC.Delete();
            if (Hibernia_MLTokensNPC != null)
                Hibernia_MLTokensNPC.Delete();
        }

        #endregion AddToWorld
    }
}