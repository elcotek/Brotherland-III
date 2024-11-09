using DOL.Database;
using DOL.GS.PacketHandler;


namespace DOL.GS.Scripts
{

    public class ChampNPC : GameNPC
    {

        public override bool AddToWorld()
        {
            GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
            switch (Realm)
            {
                case eRealm.Albion:
                    template.AddNPCEquipment(eInventorySlot.TorsoArmor, 2230); break;
                case eRealm.Midgard:
                    template.AddNPCEquipment(eInventorySlot.TorsoArmor, 2232);
                    template.AddNPCEquipment(eInventorySlot.ArmsArmor, 2233);
                    template.AddNPCEquipment(eInventorySlot.LegsArmor, 2234);
                    template.AddNPCEquipment(eInventorySlot.HandsArmor, 2235);
                    template.AddNPCEquipment(eInventorySlot.FeetArmor, 2236);
                    break;
                case eRealm.Hibernia:
                    template.AddNPCEquipment(eInventorySlot.TorsoArmor, 2231); ; break;
            }

            Inventory = template.CloseTemplate();
            Flags = eFlags.PEACE; // Peace flag.
            return base.AddToWorld();
        }


        public override bool ReceiveItem(GameLiving source, InventoryItem item)
        {

            GamePlayer t = source as GamePlayer;
            if (source != null && source.IsWithinRadius(this, WorldMgr.INTERACT_DISTANCE) == false)
            {
                ((GamePlayer)source).Out.SendMessage("You are too far away to give anything to " + GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }
            if (t.Inventory.IsSlotsFree(2, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
            {


            }
            else
            {
                t.Out.SendMessage("You don't have enough inventory space.  You need " + 2 + " free slot(s) to interact with me!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (t != null && item != null)
            {


                switch (item.Id_nb)
                {

                    #region Albion Champion Weapons

                    case "UpsilonWizardStaff":
                        {



                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "UpsilonSorcererStaff":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "UpsilonCabalistStaff":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "UpsilonTheurgistStaff":
                        {

                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "UpsilonNecromancerStaff":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "mauler_laevus_fist_wrapall":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "MaulerDexteraFistWraphib2":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "SatagoWarStaff":
                        {

                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ArmsmanDexteraBlade":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ArmsmanDexteraEdge":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ArmsmanDexteraMace":
                        {

                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ArmsmanSatagoArchMace":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }

                    case "ArmsmanSatagoFlamberge":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ArmsmanSatagoHalberd":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ArmsmanSatagoLance":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ArmsmanSatagoMattock":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ArmsmanSatagoPike":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ClericDexteraMace":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "FriarDexteraMace":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "FriarSatagoQuarterStaff":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "HereticDexteraBarbedChain":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "HereticDexteraFlail":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "HereticDexteraMace":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "InfiltratorDexteraBlade":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "InfiltratorDexteraEdge":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "InfiltratorLaevusBlade":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "InfiltratorLaevusEdge":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "MercenaryDexteraBlade":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "MercenaryDexteraEdge":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "MercenaryDexteraMace":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "MercenaryLaevusBlade":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "MercenaryLaevusEdge":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "MercenaryLaevusMace":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "MinstrelDexteraBlade":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "MinstrelDexteraEdge":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "MinstrelDexteraHarp":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "PaladinDexteraBlade":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "PaladinDexteraEdge":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "PaladinDexteraMace":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "PaladinSatagoGreatEdge":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "PaladinSatagoGreatHammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "PaladinSatagoGreatSword":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ReaverDexteraBarbedChain":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ReaverDexteraBlade":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ReaverDexteraEdge":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ReaverDexteraFlail":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ReaverDexteraMace":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ScoutDexteraBlade":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ScoutDexteraBow":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ScoutDexteraEdge":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    #endregion Albion Champion Weapons

                    #region Midgard Champion Weapons
                    case "AnsuzBonedancerStaff":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "AnsuzRunemasterStaff":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "AnsuzSpiritmasterStaff":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "AnsuzWarlockStaff":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazWarriorAxe":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazWarriorHammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazWarriorSword":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazWarriorTwohandedAxe":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazWarriorTwohandedHammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazWarriorTwohandedSword":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazValkyrieSlashingSpear":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazValkyrieSword":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazValkyrieThrustingSpear":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazValkyrieTwohandedSword":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "AnsuzHealerTwohandedHammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "AnsuzHealerHammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "AnsuzShamanHammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "AnsuzShamanTwohandedHammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazCompoundBow":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazHunterSlashingSpear":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazHunterSpear":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazHunterSword":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazHunterTwohandedSword":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazSavageAxe":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazSavageHammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazSavageSlashingGlaiverh":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazSavageSword":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazSavageThrashingGlaiverh":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazSavageTwohandedAxe":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazSavageTwohandedHammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazSavageTwohandedSword":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazBerserkerAxelh":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazBerserkerAxerh":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazBerserkerHammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazBerserkerSword":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazBerserkerTwohandedAxe":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazBerserkerTwohandedHammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazBerserkerTwohandedSword":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazShadowbladeAxe":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazShadowbladeHeavyAxe":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazShadowbladeHeavySword":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazShadowbladeSword":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazSkaldAxe":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazSkaldHammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazSkaldSword":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazSkaldTwohandedAxe":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazSkaldTwohandedHammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazSkaldTwohandedSword":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazThaneAxe":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazThaneHammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazThaneSword":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazThaneTwohandedAxe":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazThaneTwohandedHammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "ThurisazThaneTwohandedSword":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }

                    #endregion Midgard Champion Weapons

                    #region Hibernia Champion Weapons
                    case "DraiochtAnimistStaff":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DraiochtBainsheeStaff":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DraiochtEldritchStaff":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DraiochtEnchanterStaff":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DraiochtMentalistStaff":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharValewalkerScythe":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "VampiirFuarSteel":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DraiochtHarp":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharBardBlade":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharBardHammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharDruidBlade":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharDruidHammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharWardenBlade":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharWardenHammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharBlademasterBlade":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharBlademasterHammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharBlademasterSteel":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "BlademasterFuarBlade":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "BlademasterFuarHammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break; ;
                        }
                    case "BlademasterFuarSteel":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharChampionBlade":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharChampionHammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharChampionSteel":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharChampionWarblade":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharChampionWarhammerr":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharHeroBlade":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharHeroHammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharHeroSpear":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharHeroSteel":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharHeroWarblade":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharHeroWarhammer":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharNightshadeBlade":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharNightshadeSteel":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "NightshadeFuarBlade":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "NightshadeFuarSteel":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "RangerFuarBlade":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "RangerFuarSteel":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharRangerBlade":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharRangerSteel":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    case "DocharRecurveBow":
                        {
                            t.HasChampionWeapons = false;
                            t.Inventory.RemoveItem(item);
                            t.SaveIntoDatabase();
                            t.UpdatePlayerStatus();
                            t.Out.SendMessage(" " + Name + " has received your " + item.Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            t.Out.SendMessage(" " + t.Name + " You are now able to select a new Champion Wepon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        }
                    #endregion Hibernia Champion Weapons

                    default:
                        t.Out.SendMessage("Thats not a Champion Weapon!. Try another.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        break;

                }

                // t.Out.SendMessage("The npc has received Your " + t.XPStoneCount + " of max 20 pieces for today ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }

            return base.ReceiveItem(source, item);
        }



        public override bool Interact(GamePlayer player)
        {

            if (player.HasChampionWeapons)
            {
                player.Out.SendMessage("Sorry you are have already a Champion Weapon!" +
              " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);


                player.Out.SendMessage(" " + Name + " say's i em able to take back your Champion Weapon.\n" +
               "Hand me your Weapon and you will be able to select another.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;

            }
            if (player.ChampionLevel <= 4)
            {
                player.Out.SendMessage("Hello, I'am the Kings Armsmaster.\n" +
               "Reach Champion level 5 and I shall grant you a reward, you are currently Champion Level" + player.ChampionLevel + " !", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }

            if (player.Inventory.IsSlotsFree(2, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
            {


            }
            else
            {
                player.Out.SendMessage("You don't have enough inventory space. You need " + 2 + " free slot(s) to get the Reward!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (player.CharacterClass.Name == "Wizard" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                    "now select your weaopn type:" +
               "I have a [Upsilon Wizard Staff] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }

            if (player.CharacterClass.Name == "Sorcerer" || player.CharacterClass.Name == "Sorceress" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
              "now select your weaopn type:" +
              "I have a [Upsilon Sorcerer Staff] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }

            if (player.CharacterClass.Name == "Cabalist" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
             "now select your weaopn type:" +
             "I have a [Upsilon Cabalist Staff] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }

            if (player.CharacterClass.Name == "Theurgist" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
             "now select your weaopn type:" +
             "I have a [Upsilon Theurgist Staff] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }

            if (player.CharacterClass.Name == "Necromancer" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " + "now select your weaopn type:" +
                "I have a [Upsilon Necromancer Staff] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Mauler" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Mauler Laevus Fist Wrap], [Mauler Dextera Fist Wrap]," +
                "or [Satago War Staff] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Armsman" || player.CharacterClass.Name == "Armswoman" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Armsman Dextera Blade], [Armsman Dextera Edge], [Armsman Dextera Mace], [Armsman Satago Arch Mace], " +
                "[Armsman Satago Flamberge], [Armsman Satago Halberd], [Armsman Satago Lance], [Armsman Satago Mattock], " +
                "or [Armsman Satago Pike] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Cleric" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Cleric Dextera Mace] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Friar" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Friar Dextera Mace], or [Friar Satago Quarter Staff] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Heretic" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                 "now select your weaopn type:" +
                 "I have a [Heretic Dextera Barbed Chain], [Heretic Dextera Flail]," +
                 " or [Heretic Dextera Mace] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;

            }
            if (player.CharacterClass.Name == "Infiltrator" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                 "now select your weaopn type:" +
                 "I have a [InfiltratorDexteraBlade], [nfiltrator Dextera Edge]," +
                 " or [Infiltrator Laevus Blade], or [Infiltrator Laevus Edge] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Mercenary" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Mercenary Dextera Blade], [Mercenary Dextera Edge], [Mercenary Dextera Mace], [Mercenary Laevus Blade], " +
                "[Mercenary Laevus Edge], or [Mercenary Laevus Mace] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Minstrel" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Minstrel Dextera Blade], [Minstrel Dextera Edge]," +
                " or [Minstrel Dextera Harp] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Paladin" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Paladin Dextera Blade], [Paladin Dextera Edge], [Paladin Dextera Mace], " +
                "[Paladin Satago Great Edge], [Paladin Satago Great Hammer]," +
                "or [Paladin Satago GreatSword] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;

            }
            if (player.CharacterClass.Name == "Reaver" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Reaver Dextera Barbed Chain], [Reaver Dextera Blade], [Reaver Dextera Edge]," +
                "[Reaver Dextera Flail], or [Reaver Dextera Mace] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Scout" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Scout Dextera Blade], [ScoutDexteraBow], or [Scout Dextera Edge] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Bonedancer" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Ansuz Bonedancer Staff] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Runemaster" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Ansuz Runemaster Staff] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Spiritmaster" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Ansuz Spiritmaster Staff] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Warlock" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Ansuz Warlock Staff] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Warrior" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Thurisaz Warrior Axe], [Thurisaz Warrior Hammer], [Thurisaz Warrior Sword]," +
                "[Thurisaz Warrior Twohanded Axe], [Thurisaz Warrior Twohanded Hammer]," +
                "or [Thurisaz Warrior TwohandedSword] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Valkyrie" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Thurisaz Valkyrie Slashing Spear], [Thurisaz Valkyrie Sword]," +
                "[Thurisaz Valkyrie Thrusting Spear]," +
                "or [Thurisaz Valkyrie Twohanded Sword] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }

            if (player.CharacterClass.Name == "Healer" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [ Ansuz Healer Twohanded Hammer]," +
                "or [Ansuz Healer Hammer] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Shaman" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Ansuz Shaman Hammer]," +
                "or [Ansuz Shaman Twohanded Hammer] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Hunter" || player.CharacterClass.Name == "Huntress" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Thurisaz Compound Bow], [Thurisaz Hunter Slashing Spear]," +
                "[Thurisaz Hunter Spear], [Thurisaz Hunter Sword]," +
                "or [Thurisaz Hunter Twohanded Sword] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Savage" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
               "now select your weaopn type:" +
               "I have a [Thurisaz Savage Axe], [Thurisaz Savage Hammer]," +
               "[Thurisaz Savage Slashing Glaiverh], [Thurisaz Savage Sword]," +
               " [Thurisaz Savage Thrashing Glaiverh], [Thurisaz Savage Twohanded Axe], " +
               "[Thurisaz Savage Twohanded Hammer], or [Thurisaz Savage Twohanded Sword] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Berserker" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Thurisaz Berserker Axelh], [Thurisaz Berserker Axerh], [], [], " +
                "[Thurisaz Berserker Hammer], [Thurisaz Berserker Sword]," +
                "[Thurisaz Berserker Twohanded Axe], [Thurisaz Berserker Twohanded Hammer]," +
                " or [Thurisaz Berserker Twohanded Sword] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Shadowblade" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Thurisaz Shadowblade Axe], [Thurisaz Shadowblade Heavy Axe]," +
                "[Thurisaz Shadowblade Heavy Sword]," +
                "or [Thurisaz Shadowblade Sword] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Skald" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Thurisaz Skald Axe], [Thurisaz Skald Hammer]," +
                "[Thurisaz Skald Sword], [Thurisaz Skald Twohanded Axe]," +
                "[Thurisaz Skald Twohanded Hammer], or [Thurisaz Skald Twohanded Sword] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Thane" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Thurisaz Thane Axe], [Thurisaz Thane Hammer]," +
                "[Thurisaz Thane Sword], [Thurisaz Thane Twohanded Axe]," +
                "[Thurisaz Thane Twohanded Hammer], or [Thurisaz Thane Twohanded Sword] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Animist" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Draiocht Animist Staff] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Bainshee" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Draiocht Bainshee Staff] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Eldritch" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Draiocht Eldritch Staff] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Enchanter" || player.CharacterClass.Name == "Enchantress" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Draiocht Enchanter Staff] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Mentalist" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Draiocht Mentalist Staff] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Valewalker" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Dochar Valewalker Scythe] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Vampiir" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Vampiir Fuar Steel] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Bard" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Draiocht Harp], [Dochar Bard Blade]," +
                "or [Dochar Bard Hammer] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Druid" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Dochar Druid Blade]," +
                "or [Dochar Druid Hammer] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Warden" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Dochar Warden Blade]," +
                "or [Dochar Warden Hammer] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Blademaster" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Dochar Blademaste Blade], [Dochar Blademaster Hammer]," +
                "[Dochar Blademaster Steel], [Blademaster Fuar Blade], [Blademaster Fuar Hammer], " +
                "or [Blademaster Fuar Steel] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Champion" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Dochar Champion Blade], [Dochar Champion Hammer]," +
                "[Dochar Champion Steel], [Dochar Champion Warblade]," +
                "or [Dochar Champion Warhammer] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Hero" || player.CharacterClass.Name == "Heroine" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Dochar Hero Blade], [Dochar Hero Hammer]," +
                "[Dochar Hero Spear], [Dochar Hero Steel], [Dochar Hero Warblade]," +
                "or [Dochar Hero Warhammer] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Nightshade" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
               "now select your weaopn type:" +
               "I have a [Dochar Nightshade Blade], [Dochar Nightshade Steel]," +
               "[Nightshade Fuar Blade]," +
               "or [Nightshade Fuar Steel] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            if (player.CharacterClass.Name == "Ranger" && player.ChampionLevel >= 5)
            {
                player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
                "now select your weaopn type:" +
                "I have a [Ranger Fuar Blade], [Ranger Fuar Steel]," +
                "[Dochar Ranger Blade], [Dochar Ranger Steel]," +
                "or [Dochar Recurve Bow] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return true;
            }
            return true;
        }





        /*    player.Out.SendMessage(" Hello, I'm the Kings Armsmaster.\n " +
         "now select your weaopn type:" +
         "I have a [], [], [], [], " +
         "[], [], [], [], " + 
         "or [] for you!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            return true;
         */
        public override bool WhisperReceive(GameLiving player, string str)
        {
            GamePlayer t = (GamePlayer)player;

            switch (str)
            {
                #region Albion Champion Weapons

                case "Upsilon Wizard Staff":
                    {
                        //InventoryItem generic0 = new inventory();
                        //ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("UpsilonWizardStaff");
                        //generic0.CopyFrom(tgeneric0);
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("UpsilonWizardStaff");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Upsilon Sorcerer Staff":
                    {
                        //InventoryItem generic0 = new GameInventoryItem();
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("UpsilonSorcererStaff");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                        //generic0.CopyFrom(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Upsilon Cabalist Staff":
                    {
                        //InventoryItem generic0 = new GameInventoryItem();
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("UpsilonCabalistStaff");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                        //generic0.CopyFrom(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Upsilon Theurgist Staff":
                    {

                        //InventoryItem generic0 = new GameInventoryItem();
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("UpsilonTheurgistStaff");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                        //generic0.CopyFrom(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Upsilon Necromancer Staff":
                    {
                        //InventoryItem generic0 = new GameInventoryItem();
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("UpsilonNecromancerStaff");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                        //generic0.CopyFrom(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Mauler Laevus Fist Wrap":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("mauler_laevus_fist_wrapall");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        break;
                    }
                case "Mauler Dextera Fist Wrap":
                    {
                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("maulerDexteraFistWraphib2");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        break;
                    }
                case "Satago War Staff":
                    {

                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("SatagoWarStaff");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        break;
                    }
                case "Armsman Dextera Blade":
                    {
                        //InventoryItem generic0 = new GameInventoryItem();
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("ArmsmanDexteraBlade");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                        //generic0.CopyFrom(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        break;
                    }
                case "Armsman Dextera Edge":
                    {
                        //InventoryItem generic1 = new GameInventoryItem();
                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("ArmsmanDexteraEdge");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);
                        //generic1.CopyFrom(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Armsman Dextera Mace":
                    {

                        //InventoryItem generic2 = new GameInventoryItem();
                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("ArmsmanDexteraMace");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);
                        //generic2.CopyFrom(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Armsman Satago Arch Mace":
                    {
                        //InventoryItem generic3 = new GameInventoryItem();
                        ItemTemplate tgeneric3 = GameServer.Database.FindObjectByKey<ItemTemplate>("ArmsmanSatagoArchMace");
                        InventoryItem generic3 = new GameInventoryItem(tgeneric3);
                        //generic3.CopyFrom(tgeneric3);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic3);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        break;
                    }

                case "Armsman Satago Flamberge":
                    {
                        //InventoryItem generic4 = new GameInventoryItem();
                        ItemTemplate tgeneric4 = GameServer.Database.FindObjectByKey<ItemTemplate>("ArmsmanSatagoFlamberge");
                        InventoryItem generic4 = new GameInventoryItem(tgeneric4);
                        //generic4.CopyFrom(tgeneric4);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic4);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Armsman Satago Halberd":
                    {
                        //InventoryItem generic5 = new GameInventoryItem();
                        ItemTemplate tgeneric5 = GameServer.Database.FindObjectByKey<ItemTemplate>("ArmsmanSatagoHalberd");
                        InventoryItem generic5 = new GameInventoryItem(tgeneric5);
                        //generic5.CopyFrom(tgeneric5);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic5);

                        break;
                    }
                case "Armsman Satago Lance":
                    {
                        //InventoryItem generic6 = new GameInventoryItem();
                        ItemTemplate tgeneric6 = GameServer.Database.FindObjectByKey<ItemTemplate>("ArmsmanSatagoLance");
                        InventoryItem generic6 = new GameInventoryItem(tgeneric6);
                        //generic6.CopyFrom(tgeneric6);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic6);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Armsman Satago Mattock":
                    {
                        //InventoryItem generic7 = new GameInventoryItem();
                        ItemTemplate tgeneric7 = GameServer.Database.FindObjectByKey<ItemTemplate>("ArmsmanSatagoMattock");
                        InventoryItem generic7 = new GameInventoryItem(tgeneric7);
                        //generic7.CopyFrom(tgeneric7);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic7);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        break;
                    }
                case "Armsman Satago Pike":
                    {
                        //InventoryItem generic8 = new GameInventoryItem();
                        ItemTemplate tgeneric8 = GameServer.Database.FindObjectByKey<ItemTemplate>("ArmsmanSatagoPike");
                        InventoryItem generic8 = new GameInventoryItem(tgeneric8);
                        //generic8.CopyFrom(tgeneric8);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic8);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        break;
                    }
                case "Cleric Dextera Mace":
                    {

                        //InventoryItem generic0 = new GameInventoryItem();
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("ClericDexteraMace");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                        //generic0.CopyFrom(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Friar Dextera Mace":
                    {
                        //InventoryItem generic0 = new GameInventoryItem();
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("FriarDexteraMace");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                        //generic0.CopyFrom(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        break;
                    }
                case "Friar Satago Quarter Staff":
                    {

                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("FriarSatagoQuarterStaff");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);
                        //generic1.CopyFrom(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Heretic Dextera Barbed Chain":
                    {
                        //InventoryItem generic0 = new GameInventoryItem();
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("HereticDexteraBarbedChain");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                        //generic0.CopyFrom(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Heretic Dextera Flail":
                    {
                        //InventoryItem generic1 = new GameInventoryItem();
                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("HereticDexteraFlail");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);
                        //generic1.CopyFrom(tgeneric1);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Heretic Dextera Mace":
                    {
                        //InventoryItem generic2 = new GameInventoryItem();
                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("HereticDexteraMace");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);
                        //generic2.CopyFrom(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        break;
                    }
                case "InfiltratorDexteraBlade":
                    {
                        //InventoryItem generic0 = new GameInventoryItem();
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("InfiltratorDexteraBlade");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                        //generic0.CopyFrom(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Infiltrator Dextera Edge":
                    {
                        //InventoryItem generic1 = new GameInventoryItem();
                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("InfiltratorDexteraEdge");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);
                        //generic1.CopyFrom(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Infiltrator Laevus Blade":
                    {
                        //InventoryItem generic2 = new GameInventoryItem();
                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("InfiltratorLaevusBlade");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);
                        //generic2.CopyFrom(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Infiltrator Laevus Edge":
                    {

                        //InventoryItem generic3 = new GameInventoryItem();
                        ItemTemplate tgeneric3 = GameServer.Database.FindObjectByKey<ItemTemplate>("InfiltratorLaevusEdge");
                        InventoryItem generic3 = new GameInventoryItem(tgeneric3);
                        //generic3.CopyFrom(tgeneric3);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic3);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Mercenary Dextera Blade":
                    {

                        //InventoryItem generic0 = new GameInventoryItem();
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("MercenaryDexteraBlade");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                        //generic0.CopyFrom(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        break;
                    }
                case "Mercenary Dextera Edge":
                    {
                        //InventoryItem generic1 = new GameInventoryItem();
                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("MercenaryDexteraEdge");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);
                        //generic1.CopyFrom(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        break;
                    }
                case "Mercenary Dextera Mace":
                    {
                        //InventoryItem generic2 = new GameInventoryItem();
                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("MercenaryDexteraMace");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);
                        //generic2.CopyFrom(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }

                case "Mercenary Laevus Blade":
                    {

                        //InventoryItem generic3 = new GameInventoryItem();
                        ItemTemplate tgeneric3 = GameServer.Database.FindObjectByKey<ItemTemplate>("MercenaryLaevusBlade");
                        InventoryItem generic3 = new GameInventoryItem(tgeneric3);
                        //generic3.CopyFrom(tgeneric3);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic3);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Mercenary Laevus Edge":
                    {

                        //InventoryItem generic4 = new GameInventoryItem();
                        ItemTemplate tgeneric4 = GameServer.Database.FindObjectByKey<ItemTemplate>("MercenaryLaevusEdge");
                        InventoryItem generic4 = new GameInventoryItem(tgeneric4);
                        //generic4.CopyFrom(tgeneric4);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic4);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Mercenary Laevus Mace":
                    {
                        //InventoryItem generic5 = new GameInventoryItem();
                        ItemTemplate tgeneric5 = GameServer.Database.FindObjectByKey<ItemTemplate>("MercenaryLaevusMace");
                        InventoryItem generic5 = new GameInventoryItem(tgeneric5);
                        //generic5.CopyFrom(tgeneric5);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic5);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Minstrel Dextera Blade":
                    {

                        //InventoryItem generic0 = new GameInventoryItem();
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("MinstrelDexteraBlade");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                        //generic0.CopyFrom(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }

                case "Minstrel Dextera Edge":
                    {
                        //InventoryItem generic1 = new GameInventoryItem();
                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("MinstrelDexteraEdge");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);
                        //generic1.CopyFrom(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }

                case "Minstrel Dextera Harp":
                    {
                        //InventoryItem generic2 = new GameInventoryItem();
                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("MinstrelDexteraHarp");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);
                        //generic2.CopyFrom(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Paladin Dextera Blade":
                    {
                        //InventoryItem generic0 = new GameInventoryItem();
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("PaladinDexteraBlade");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                        //generic0.CopyFrom(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Paladin Dextera Edge":
                    {
                        //InventoryItem generic1 = new GameInventoryItem();
                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("PaladinDexteraEdge");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);
                        //generic1.CopyFrom(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Paladin Dextera Mace":
                    {

                        //InventoryItem generic2 = new GameInventoryItem();
                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("PaladinDexteraMace");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);
                        //generic2.CopyFrom(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        break;
                    }
                case "Paladin Satago Great Edge":
                    {
                        //InventoryItem generic3 = new GameInventoryItem();
                        ItemTemplate tgeneric3 = GameServer.Database.FindObjectByKey<ItemTemplate>("PaladinSatagoGreatEdge");
                        InventoryItem generic3 = new GameInventoryItem(tgeneric3);
                        //generic3.CopyFrom(tgeneric3);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic3);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }

                case "Paladin Satago Great Hammer":
                    {

                        //InventoryItem generic4 = new GameInventoryItem();
                        ItemTemplate tgeneric4 = GameServer.Database.FindObjectByKey<ItemTemplate>("PaladinSatagoGreatHammer");
                        InventoryItem generic4 = new GameInventoryItem(tgeneric4);
                        //generic4.CopyFrom(tgeneric4);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic4);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Paladin Satago GreatSword":
                    {

                        //InventoryItem generic5 = new GameInventoryItem();
                        ItemTemplate tgeneric5 = GameServer.Database.FindObjectByKey<ItemTemplate>("PaladinSatagoGreatSword");
                        InventoryItem generic5 = new GameInventoryItem(tgeneric5);
                        //generic5.CopyFrom(tgeneric5);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic5);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Reaver Dextera Barbed Chain":
                    {
                        //InventoryItem generic0 = new GameInventoryItem();
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("ReaverDexteraBarbedChain");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                        //generic0.CopyFrom(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }

                case "Reaver Dextera Blade":
                    {
                        //InventoryItem generic1 = new GameInventoryItem();
                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("ReaverDexteraBlade");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);
                        //generic1.CopyFrom(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Reaver Dextera Edge":
                    {

                        //InventoryItem generic2 = new GameInventoryItem();
                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("ReaverDexteraEdge");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);
                        //generic2.CopyFrom(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Reaver Dextera Flail":
                    {

                        //InventoryItem generic3 = new GameInventoryItem();
                        ItemTemplate tgeneric3 = GameServer.Database.FindObjectByKey<ItemTemplate>("ReaverDexteraFlail");
                        InventoryItem generic3 = new GameInventoryItem(tgeneric3);
                        //generic3.CopyFrom(tgeneric3);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic3);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Reaver Dextera Mace":
                    {
                        ItemTemplate tgeneric4 = GameServer.Database.FindObjectByKey<ItemTemplate>("ReaverDexteraMace");
                        InventoryItem generic4 = new GameInventoryItem(tgeneric4);
                        //generic4.CopyFrom(tgeneric4);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic4);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }

                case "Scout Dextera Blade":
                    {

                        //InventoryItem generic0 = new GameInventoryItem();
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("ScoutDexteraBlade");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                        //generic0.CopyFrom(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }

                case "ScoutDexteraBow":
                    {
                        //InventoryItem generic1 = new GameInventoryItem();
                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("ScoutDexteraBow");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);
                        //generic1.CopyFrom(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }

                case "Scout Dextera Edge":
                    {
                        //InventoryItem generic2 = new GameInventoryItem();
                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("ScoutDexteraEdge");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);
                        //generic2.CopyFrom(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                #endregion Albion Champion Weapons

                #region Midgard Champion Weapons
                case "Ansuz Bonedancer Staff":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("AnsuzBonedancerStaff");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Ansuz Runemaster Staff":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("AnsuzRunemasterStaff");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Ansuz Spiritmaster Staff":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("AnsuzSpiritmasterStaff");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Ansuz Warlock Staff":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("AnsuzWarlockStaff");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Thurisaz Warrior Axe":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazWarriorAxe");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Thurisaz Warrior Hammer":
                    {

                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazWarriorHammer");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Thurisaz Warrior Sword":
                    {
                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazWarriorSword");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);


                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Thurisaz Warrior Twohanded Axe":
                    {

                        ItemTemplate tgeneric3 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazWarriorTwohandedAxe");
                        InventoryItem generic3 = new GameInventoryItem(tgeneric3);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic3);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Thurisaz Warrior Twohanded Hammer":
                    {
                        ItemTemplate tgeneric4 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazWarriorTwohandedHammer");
                        InventoryItem generic4 = new GameInventoryItem(tgeneric4);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic4);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Thurisaz Warrior TwohandedSword":
                    {

                        ItemTemplate tgeneric5 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazWarriorTwohandedSword");
                        InventoryItem generic5 = new GameInventoryItem(tgeneric5);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic5);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        break;
                    }
                case "Thurisaz Valkyrie Slashing Spear":
                    {

                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazValkyrieSlashingSpear");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Thurisaz Valkyrie Sword":
                    {
                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazValkyrieSword");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Thurisaz Valkyrie Thrusting Spear":
                    {
                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazValkyrieThrustingSpear");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Thurisaz Valkyrie Twohanded Sword":
                    {

                        ItemTemplate tgeneric3 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazValkyrieTwohandedSword");
                        InventoryItem generic3 = new GameInventoryItem(tgeneric3);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic3);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Ansuz Healer Twohanded Hammer":
                    {

                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("AnsuzHealerTwohandedHammer");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Ansuz Healer Hammer":
                    {

                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("AnsuzHealerHammer");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        break;
                    }
                case "Ansuz Shaman Hammer":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("AnsuzShamanHammer");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Ansuz Shaman Twohanded Hammer":
                    {
                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("AnsuzShamanTwohandedHammer");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Thurisaz Compound Bow":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazCompoundBow");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);


                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Thurisaz Hunter Slashing Spear":
                    {

                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazHunterSlashingSpear");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Thurisaz Hunter Spear":
                    {
                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazHunterSpear");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Thurisaz Hunter Sword":
                    {
                        ItemTemplate tgeneric3 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazHunterSword");
                        InventoryItem generic3 = new GameInventoryItem(tgeneric3);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic3);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Thurisaz Hunter Twohanded Sword":
                    {
                        ItemTemplate tgeneric4 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazHunterTwohandedSword");
                        InventoryItem generic4 = new GameInventoryItem(tgeneric4);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic4);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        break;
                    }
                case "Thurisaz Savage Axe":
                    {

                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazSavageAxe");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);


                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Thurisaz Savage Hammer":
                    {
                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazSavageHammer");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Thurisaz Savage Slashing Glaiverh":
                    {
                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazSavageSlashingGlaiverh");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Thurisaz Savage Sword":
                    {
                        ItemTemplate tgeneric4 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazSavageSword");
                        InventoryItem generic4 = new GameInventoryItem(tgeneric4);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic4);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Thurisaz Savage Thrashing Glaiverh":
                    {
                        ItemTemplate tgeneric5 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazSavageThrashingGlaiverh");
                        InventoryItem generic5 = new GameInventoryItem(tgeneric5);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic5);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Thurisaz Savage Twohanded Axe":
                    {
                        ItemTemplate tgeneric7 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazSavageTwohandedAxe");
                        InventoryItem generic7 = new GameInventoryItem(tgeneric7);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic7);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Savage Twohanded Hammer":
                    {
                        ItemTemplate tgeneric8 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazSavageTwohandedHammer");
                        InventoryItem generic8 = new GameInventoryItem(tgeneric8);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic8);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Savage Twohanded Sword":
                    {
                        ItemTemplate tgeneric9 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazSavageTwohandedSword");
                        InventoryItem generic9 = new GameInventoryItem(tgeneric9);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic9);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Berserker Axelh":
                    {

                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazBerserkerAxelh");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Berserker Axerh":
                    {
                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazBerserkerAxerh");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);


                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Berserker Hammer":
                    {

                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazBerserkerHammer");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Berserker Sword":
                    {

                        ItemTemplate tgeneric3 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazBerserkerSword");
                        InventoryItem generic3 = new GameInventoryItem(tgeneric3);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic3);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Berserker Twohanded Axe":
                    {
                        ItemTemplate tgeneric4 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazBerserkerTwohandedAxe");
                        InventoryItem generic4 = new GameInventoryItem(tgeneric4);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic4);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Berserker Twohanded Hammer":
                    {

                        ItemTemplate tgeneric5 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazBerserkerTwohandedHammer");
                        InventoryItem generic5 = new GameInventoryItem(tgeneric5);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic5);


                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Berserker Twohanded Sword":
                    {
                        ItemTemplate tgeneric6 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazBerserkerTwohandedSword");
                        InventoryItem generic6 = new GameInventoryItem(tgeneric6);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic6);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        break;

                    }
                case "Thurisaz Shadowblade Axe":
                    {

                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazShadowbladeAxe");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Shadowblade Heavy Axe":
                    {
                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazShadowbladeHeavyAxe");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Shadowblade Heavy Sword":
                    {
                        ItemTemplate tgeneric3 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazShadowbladeHeavySword");
                        InventoryItem generic3 = new GameInventoryItem(tgeneric3);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic3);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Shadowblade Sword":
                    {
                        ItemTemplate tgeneric4 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazShadowbladeSword");
                        InventoryItem generic4 = new GameInventoryItem(tgeneric4);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic4);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Skald Axe":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazSkaldAxe");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);


                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Skald Hammer":
                    {

                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazSkaldHammer");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);


                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Skald Sword":
                    {

                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazSkaldSword");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);


                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Skald Twohanded Axe":
                    {

                        ItemTemplate tgeneric3 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazSkaldTwohandedAxe");
                        InventoryItem generic3 = new GameInventoryItem(tgeneric3);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic3);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Skald Twohanded Hammer":
                    {
                        ItemTemplate tgeneric4 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazSkaldTwohandedHammer");
                        InventoryItem generic4 = new GameInventoryItem(tgeneric4);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic4);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Skald Twohanded Sword":
                    {

                        ItemTemplate tgeneric5 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazSkaldTwohandedSword");
                        InventoryItem generic5 = new GameInventoryItem(tgeneric5);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic5);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Thane Axe":
                    {

                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazThaneAxe");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Thane Hammer":
                    {
                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazThaneHammer");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Thane Sword":
                    {
                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazThaneSword");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Thane Twohanded Axe":
                    {

                        ItemTemplate tgeneric3 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazThaneTwohandedAxe");
                        InventoryItem generic3 = new GameInventoryItem(tgeneric3);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic3);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Thane Twohanded Hammer":
                    {
                        ItemTemplate tgeneric4 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazThaneTwohandedHammer");
                        InventoryItem generic4 = new GameInventoryItem(tgeneric4);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic4);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }
                case "Thurisaz Thane Twohanded Sword":
                    {
                        ItemTemplate tgeneric5 = GameServer.Database.FindObjectByKey<ItemTemplate>("ThurisazThaneTwohandedSword");
                        InventoryItem generic5 = new GameInventoryItem(tgeneric5);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic5);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;

                    }

                #endregion Midgard Champion Weapons

                #region Hibernia Champion Weapons
                case "Draiocht Animist Staff":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("DraiochtAnimistStaff");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Draiocht Bainshee Staff":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("DraiochtBainsheeStaff");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Draiocht Eldritch Staff":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("DraiochtEldritchStaff");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Draiocht Enchanter Staff":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("DraiochtEnchanterStaff");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Draiocht Mentalist Staff":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("DraiochtMentalistStaff");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Valewalker Scythe":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharValewalkerScythe");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Vampiir Fuar Steel":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("VampiirFuarSteel");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Draiocht Harp":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("DraiochtHarp");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Bard Blade":
                    {
                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharBardBlade");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);
                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Bard Hammer":
                    {
                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharBardHammer");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Druid Blade":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharDruidBlade");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Druid Hammer":
                    {
                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharDruidHammer");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Warden Blade":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharWardenBlade");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Warden Hammer":
                    {
                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharWardenHammer");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Blademaste Blade":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharBlademasterBlade");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Blademaster Hammer":
                    {

                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharBlademasterHammer");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Blademaster Steel":
                    {
                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharBlademasterSteel");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Blademaster Fuar Blade":
                    {
                        ItemTemplate tgeneric3 = GameServer.Database.FindObjectByKey<ItemTemplate>("BlademasterFuarBlade");
                        InventoryItem generic3 = new GameInventoryItem(tgeneric3);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic3);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Blademaster Fuar Hammer":
                    {
                        ItemTemplate tgeneric4 = GameServer.Database.FindObjectByKey<ItemTemplate>("BlademasterFuarHammer");
                        InventoryItem generic4 = new GameInventoryItem(tgeneric4);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic4);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Blademaster Fuar Steel":
                    {

                        ItemTemplate tgeneric5 = GameServer.Database.FindObjectByKey<ItemTemplate>("BlademasterFuarSteel");
                        InventoryItem generic5 = new GameInventoryItem(tgeneric5);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic5);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Champion Blade":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharChampionBlade");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Champion Hammer":
                    {
                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharChampionHammer");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Champion Steel":
                    {
                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharChampionSteel");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Champion Warblade":
                    {
                        ItemTemplate tgeneric3 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharChampionWarblade");
                        InventoryItem generic3 = new GameInventoryItem(tgeneric3);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic3);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Champion Warhammer":
                    {
                        ItemTemplate tgeneric4 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharChampionWarhammer");
                        InventoryItem generic4 = new GameInventoryItem(tgeneric4);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic4);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Hero Blade":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharHeroBlade");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Hero Hammer":
                    {
                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharHeroHammer");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Hero Spear":
                    {
                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharHeroSpear");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Hero Steel":
                    {

                        ItemTemplate tgeneric3 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharHeroSteel");
                        InventoryItem generic3 = new GameInventoryItem(tgeneric3);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic3);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Hero Warblade":
                    {
                        ItemTemplate tgeneric4 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharHeroWarblade");
                        InventoryItem generic4 = new GameInventoryItem(tgeneric4);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic4);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Hero Warhammer":
                    {
                        ItemTemplate tgeneric5 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharHeroWarhammer");
                        InventoryItem generic5 = new GameInventoryItem(tgeneric5);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic5);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Nightshade Blade":
                    {
                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharNightshadeBlade");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Nightshade Steel":
                    {
                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharNightshadeSteel");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Nightshade Fuar Blade":
                    {
                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("NightshadeFuarBlade");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);


                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Nightshade Fuar Steel":
                    {
                        ItemTemplate tgeneric3 = GameServer.Database.FindObjectByKey<ItemTemplate>("NightshadeFuarSteel");
                        InventoryItem generic3 = new GameInventoryItem(tgeneric3);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic3);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Ranger Fuar Blade":
                    {

                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("RangerFuarBlade");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Ranger Fuar Steel":
                    {
                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("RangerFuarSteel");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);


                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Ranger Blade":
                    {
                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharRangerBlade");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Ranger Steel":
                    {
                        ItemTemplate tgeneric3 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharRangerSteel");
                        InventoryItem generic3 = new GameInventoryItem(tgeneric3);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic3);

                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case "Dochar Recurve Bow":
                    {
                        ItemTemplate tgeneric4 = GameServer.Database.FindObjectByKey<ItemTemplate>("DocharRecurveBow");
                        InventoryItem generic4 = new GameInventoryItem(tgeneric4);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic4);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
                #endregion Hibernia Champion Weapons

                #region Champion Jewelry

                case "Jewelry":
                    {

                        ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("ChampionCloak");
                        InventoryItem generic0 = new GameInventoryItem(tgeneric0);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);


                        ItemTemplate tgeneric1 = GameServer.Database.FindObjectByKey<ItemTemplate>("ChampionNecklace");
                        InventoryItem generic1 = new GameInventoryItem(tgeneric1);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic1);


                        ItemTemplate tgeneric2 = GameServer.Database.FindObjectByKey<ItemTemplate>("ChampionBelt");
                        InventoryItem generic2 = new GameInventoryItem(tgeneric2);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic2);


                        ItemTemplate tgeneric3 = GameServer.Database.FindObjectByKey<ItemTemplate>("ChampionJewel");
                        InventoryItem generic3 = new GameInventoryItem(tgeneric3);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic3);


                        ItemTemplate tgeneric4 = GameServer.Database.FindObjectByKey<ItemTemplate>("ChampionRing");
                        InventoryItem generic4 = new GameInventoryItem(tgeneric4);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic4);


                        ItemTemplate tgeneric5 = GameServer.Database.FindObjectByKey<ItemTemplate>("ChampionBand");
                        InventoryItem generic5 = new GameInventoryItem(tgeneric5);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic5);


                        ItemTemplate tgeneric6 = GameServer.Database.FindObjectByKey<ItemTemplate>("ChampionBracer");
                        InventoryItem generic6 = new GameInventoryItem(tgeneric6);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic6);

GamePlayerUtils.SendClearPopupWindow(t);
                        ItemTemplate tgeneric7 = GameServer.Database.FindObjectByKey<ItemTemplate>("ChampionWristBand");
                        InventoryItem generic7 = new GameInventoryItem(tgeneric7);

                        t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic7);
                        t.UpdatePlayerStatus();
                        t.Out.SendMessage("Here you are!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                    break;
            }

            t.HasChampionWeapons = true;
            GamePlayerUtils.SendClearPopupWindow(t);
            return true;
        }

        private void SendReply(GamePlayer target, string msg)
        {
            target.Out.SendMessage(msg, eChatType.CT_System, eChatLoc.CL_PopupWindow);
        }
    }
}
#endregion Champion Jewelry  