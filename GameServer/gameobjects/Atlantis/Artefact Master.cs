using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;



namespace DOL.GS.Scripts
{
    public class ArtifactMasterNPC : GameNPC
    {
        private const string REFUND_ITEM_WEAK = "refunded item";
        private const string BookToRemove = "BOOKTOREMOVE";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
            Flags = eFlags.PEACE;	// Peace flag.
            return base.AddToWorld();
        }
        private readonly InventoryItem items = null;

        public override bool ReceiveItem(GameLiving source, InventoryItem item)
        {
            if (source == null || item == null)
                return false;

            GamePlayer player = (GamePlayer)source;
            GamePlayer t = source as GamePlayer;
            InventoryItem items = null;
            items = item;

            if (!this.IsWithinRadius(source, WorldMgr.INTERACT_DISTANCE))
            {
                ((GamePlayer)source).Out.SendMessage(LanguageMgr.GetTranslation((source as GamePlayer).Client.Account.Language, "PlayerMoveItemRequestHandler.TooFarAway", GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                //((GamePlayer)source).Out.SendMessage("You are too far away to give anything to " + GetName(0, false) + "." + " (you need the right class and Level)", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }
            if (t != null && item != null)
            {
                switch (item.Name)
                {

                    case "Alvaru's Letters": //Alvarus' Leggings
                        {
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Animist) || player.IsCharcterClass(eCharacterClass.Bainshee) //cloth
                                || player.IsCharcterClass(eCharacterClass.Eldritch) || player.IsCharcterClass(eCharacterClass.Enchanter)
                                || player.IsCharcterClass(eCharacterClass.Mentalist) || player.IsCharcterClass(eCharacterClass.Valewalker)
                                || player.IsCharcterClass(eCharacterClass.Heretic) || player.IsCharcterClass(eCharacterClass.Cabalist)
                                || player.IsCharcterClass(eCharacterClass.Necromancer) || player.IsCharcterClass(eCharacterClass.Sorcerer)
                                || player.IsCharcterClass(eCharacterClass.Theurgist) || player.IsCharcterClass(eCharacterClass.Wizard)
                                || player.IsCharcterClass(eCharacterClass.Bonedancer) || player.IsCharcterClass(eCharacterClass.Runemaster)
                                || player.IsCharcterClass(eCharacterClass.Spiritmaster) || player.IsCharcterClass(eCharacterClass.Warlock))) //Alvarus' Leggings
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("alvarus_leggings");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Vampiir) || player.IsCharcterClass(eCharacterClass.MaulerHib) //leather
                                || player.IsCharcterClass(eCharacterClass.MaulerMid) || player.IsCharcterClass(eCharacterClass.MaulerAlb)
                                || player.IsCharcterClass(eCharacterClass.Nightshade) || player.IsCharcterClass(eCharacterClass.Friar)
                                || player.IsCharcterClass(eCharacterClass.Infiltrator) || player.IsCharcterClass(eCharacterClass.Shadowblade)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("alvarus_leggings_leather");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Bard) || player.IsCharcterClass(eCharacterClass.Blademaster)  //reinforced
                                || player.IsCharcterClass(eCharacterClass.Ranger)))//Alvarus' Leggings
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("alvarus_leggings_reinforced");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Scout) || player.IsCharcterClass(eCharacterClass.Berserker) //studded
                                || player.IsCharcterClass(eCharacterClass.Hunter) || player.IsCharcterClass(eCharacterClass.Savage)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("alvarus_leggings_studded");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Champion) || player.IsCharcterClass(eCharacterClass.Druid) //scale
                                || player.IsCharcterClass(eCharacterClass.Hero) || player.IsCharcterClass(eCharacterClass.Warden)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("alvarus_leggings_scale");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cleric) || player.IsCharcterClass(eCharacterClass.Mercenary) //chain
                                || player.IsCharcterClass(eCharacterClass.Minstrel) || player.IsCharcterClass(eCharacterClass.Reaver)
                                || player.IsCharcterClass(eCharacterClass.Healer)
                                || player.IsCharcterClass(eCharacterClass.Shaman) || player.IsCharcterClass(eCharacterClass.Skald)
                                || player.IsCharcterClass(eCharacterClass.Thane) || player.IsCharcterClass(eCharacterClass.Valkyrie)
                                || player.IsCharcterClass(eCharacterClass.Warrior)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("alvarus_leggings_chain");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Paladin) //plate
                                || player.IsCharcterClass(eCharacterClass.Armsman)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("alvarus_leggings_plate");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }
                    case "Fish Scales": //Arms of the Winds
                        {
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Animist) || player.IsCharcterClass(eCharacterClass.Bainshee) //cloth
                                || player.IsCharcterClass(eCharacterClass.Eldritch) || player.IsCharcterClass(eCharacterClass.Enchanter)
                                || player.IsCharcterClass(eCharacterClass.Mentalist) || player.IsCharcterClass(eCharacterClass.Valewalker)
                                || player.IsCharcterClass(eCharacterClass.Heretic) || player.IsCharcterClass(eCharacterClass.Cabalist)
                                || player.IsCharcterClass(eCharacterClass.Necromancer) || player.IsCharcterClass(eCharacterClass.Sorcerer)
                                || player.IsCharcterClass(eCharacterClass.Theurgist) || player.IsCharcterClass(eCharacterClass.Wizard)
                                || player.IsCharcterClass(eCharacterClass.Bonedancer) || player.IsCharcterClass(eCharacterClass.Runemaster)
                                || player.IsCharcterClass(eCharacterClass.Spiritmaster) || player.IsCharcterClass(eCharacterClass.Warlock))) //Arms of the Winds
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("arms_of_the_winds_cloth");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Vampiir) || player.IsCharcterClass(eCharacterClass.MaulerHib) //leather
                                || player.IsCharcterClass(eCharacterClass.MaulerMid) || player.IsCharcterClass(eCharacterClass.MaulerAlb)
                                || player.IsCharcterClass(eCharacterClass.Nightshade) || player.IsCharcterClass(eCharacterClass.Friar)
                                || player.IsCharcterClass(eCharacterClass.Infiltrator) || player.IsCharcterClass(eCharacterClass.Shadowblade)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("arms_of_the_winds_leather");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Bard) || player.IsCharcterClass(eCharacterClass.Blademaster)  //reinforced
                                || player.IsCharcterClass(eCharacterClass.Ranger)))//Alvarus' Leggings
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("arms_of_the_winds_reinforced");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Scout) || player.IsCharcterClass(eCharacterClass.Berserker) //studded
                                || player.IsCharcterClass(eCharacterClass.Hunter) || player.IsCharcterClass(eCharacterClass.Savage)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("arms_of_the_winds_studded");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Champion) || player.IsCharcterClass(eCharacterClass.Druid) //scale
                                || player.IsCharcterClass(eCharacterClass.Hero) || player.IsCharcterClass(eCharacterClass.Warden)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("arms_of_the_winds_scale");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cleric) || player.IsCharcterClass(eCharacterClass.Mercenary) //chain
                                || player.IsCharcterClass(eCharacterClass.Minstrel) || player.IsCharcterClass(eCharacterClass.Warrior)
                                || player.IsCharcterClass(eCharacterClass.Reaver) || player.IsCharcterClass(eCharacterClass.Healer)
                                || player.IsCharcterClass(eCharacterClass.Shaman) || player.IsCharcterClass(eCharacterClass.Skald)
                                || player.IsCharcterClass(eCharacterClass.Thane) || player.IsCharcterClass(eCharacterClass.Valkyrie)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("arms_of_the_winds_chain");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Paladin) || player.IsCharcterClass(eCharacterClass.Armsman)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("arms_of_the_winds_plate");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);  //Plate
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;

                        }
                    case "Champion's Notes": //Aten's Shield 
                        {
                            if (t.Level > 39 && (player.IsCharcterClass(eCharacterClass.Bard) || player.IsCharcterClass(eCharacterClass.Nightshade)
                                || player.IsCharcterClass(eCharacterClass.Ranger) || player.IsCharcterClass(eCharacterClass.Druid)
                                || player.IsCharcterClass(eCharacterClass.Friar) || player.IsCharcterClass(eCharacterClass.Heretic)
                                || player.IsCharcterClass(eCharacterClass.Infiltrator) || player.IsCharcterClass(eCharacterClass.Minstrel)
                                || player.IsCharcterClass(eCharacterClass.Scout) || player.IsCharcterClass(eCharacterClass.Berserker)
                                || player.IsCharcterClass(eCharacterClass.Healer) || player.IsCharcterClass(eCharacterClass.Shadowblade)
                                || player.IsCharcterClass(eCharacterClass.Vampiir))) //Aten's Shield small
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("atens_shield"); //Aten's Shield 
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 39 && (player.IsCharcterClass(eCharacterClass.Blademaster) || player.IsCharcterClass(eCharacterClass.Cleric)
                                || player.IsCharcterClass(eCharacterClass.Mercenary) || player.IsCharcterClass(eCharacterClass.Skald))) //Aten's Shield medium
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("atens_shield_medium"); //Aten's Shield 
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 39 && (player.IsCharcterClass(eCharacterClass.Champion) || player.IsCharcterClass(eCharacterClass.Hero)
                                 || player.IsCharcterClass(eCharacterClass.Warden) || player.IsCharcterClass(eCharacterClass.Armsman)
                                 || player.IsCharcterClass(eCharacterClass.Paladin) || player.IsCharcterClass(eCharacterClass.Reaver)
                                 || player.IsCharcterClass(eCharacterClass.Thane) || player.IsCharcterClass(eCharacterClass.Valkyrie)
                                 || player.IsCharcterClass(eCharacterClass.Warrior))) //Aten's Shield large
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("atens_shield_large"); //Aten's Shield 
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)40), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;
                        }
                    case "Atlantis Tablet": //Atlantis Tablet
                        {
                            if (t.Level > 44)
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("tablet_of_atlantis");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;
                        }
                    case "King's Vase": //Band of Stars
                        {
                            if (t.Level > 39)
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("band_of_stars");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)40), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break; ;
                        }
                    case "Bane of Battler": //Bane of Battler
                        {
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Nightshade) || player.IsCharcterClass(eCharacterClass.Ranger)
                                || player.IsCharcterClass(eCharacterClass.Hero) || player.IsCharcterClass(eCharacterClass.Champion)
                                || player.IsCharcterClass(eCharacterClass.Blademaster) || player.IsCharcterClass(eCharacterClass.Druid)
                                || player.IsCharcterClass(eCharacterClass.Warden) || player.IsCharcterClass(eCharacterClass.Bard))) //Bane of Battler
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Hib Battler 1h Con Version]?,[Hib Battler 1h Str Version],[Hib Battler 1h blunt Version],[Hib Battler 2h Crush],[Hib Battler 2h Slash]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Infiltrator) || player.IsCharcterClass(eCharacterClass.Heretic)
                                || player.IsCharcterClass(eCharacterClass.Minstrel) || player.IsCharcterClass(eCharacterClass.Scout)
                                || player.IsCharcterClass(eCharacterClass.Armsman) || player.IsCharcterClass(eCharacterClass.Paladin)
                                || player.IsCharcterClass(eCharacterClass.Reaver) || player.IsCharcterClass(eCharacterClass.Mercenary))) //Bane of Battler
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Alb Battler 1h Slash Version],[Alb Battler 1h Crush Version],[Alb Battler 2h Crush],[Alb Battler 2h Slash]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Shadowblade) || player.IsCharcterClass(eCharacterClass.Warrior)
                                || player.IsCharcterClass(eCharacterClass.Thane) || player.IsCharcterClass(eCharacterClass.Skald)
                                || player.IsCharcterClass(eCharacterClass.Berserker) || player.IsCharcterClass(eCharacterClass.Healer)
                                || player.IsCharcterClass(eCharacterClass.Shadowblade) || player.IsCharcterClass(eCharacterClass.Savage)
                                || player.IsCharcterClass(eCharacterClass.Hunter) || player.IsCharcterClass(eCharacterClass.Valkyrie)
                                || player.IsCharcterClass(eCharacterClass.Shaman))) //Bane of Battler
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Mid Battler 1h Slash Version],[Mid Battler 1h Crush Version],[Mid Battler 2h Crush],[Mid Battler 2h Slash]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;
                        } //WIZ, THEU, CAB, CLER, FRI, HER, SORC, NECR, ANI, BARD, BAN, DRU, ELD, ENCH, MENT, WARD, BD, HEAL, RM, SHA, SM, WARL, MAUL
                    case "Belt of the Moon": //Belt of the Moon
                        {//Alb
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Heretic)
                                || player.IsCharcterClass(eCharacterClass.Cabalist) || player.IsCharcterClass(eCharacterClass.Friar)
                                || player.IsCharcterClass(eCharacterClass.Necromancer) || player.IsCharcterClass(eCharacterClass.Sorcerer)
                                || player.IsCharcterClass(eCharacterClass.Theurgist) || player.IsCharcterClass(eCharacterClass.Wizard)
                                || player.IsCharcterClass(eCharacterClass.Cleric) || player.IsCharcterClass(eCharacterClass.MaulerMid)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("belt_of_the_moon");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//hib
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Animist) || player.IsCharcterClass(eCharacterClass.Bainshee)
                                || player.IsCharcterClass(eCharacterClass.Eldritch) || player.IsCharcterClass(eCharacterClass.Enchanter)
                                || player.IsCharcterClass(eCharacterClass.Mentalist) || player.IsCharcterClass(eCharacterClass.Bard)
                                || player.IsCharcterClass(eCharacterClass.Druid) || player.IsCharcterClass(eCharacterClass.Warden)
                                || player.IsCharcterClass(eCharacterClass.MaulerHib)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("belt_of_the_moon");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Mid
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Bonedancer) || player.IsCharcterClass(eCharacterClass.Runemaster)
                                || player.IsCharcterClass(eCharacterClass.Spiritmaster) || player.IsCharcterClass(eCharacterClass.Warlock)
                                || player.IsCharcterClass(eCharacterClass.Healer) || player.IsCharcterClass(eCharacterClass.Shaman)
                                || player.IsCharcterClass(eCharacterClass.MaulerMid)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("belt_of_the_moon");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;
                        } // ARMS, HER, INF, MERC, MINS, PAL, REAV, SCOU, BM, CHA, HERO, NS, RANG, VW, VAMP, WARD, BER, HUNT, SAV, SB, SKLD, THAN, VALK, WAR
                    case "Scholar's Notes": //Belt of the Sun
                        {//alb
                            if (t.Level > 39 && (player.IsCharcterClass(eCharacterClass.Armsman) || player.IsCharcterClass(eCharacterClass.Heretic)
                                || player.IsCharcterClass(eCharacterClass.Infiltrator) || player.IsCharcterClass(eCharacterClass.Mercenary)
                                || player.IsCharcterClass(eCharacterClass.Minstrel) || player.IsCharcterClass(eCharacterClass.Paladin)
                                || player.IsCharcterClass(eCharacterClass.Reaver) || player.IsCharcterClass(eCharacterClass.Scout)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("belt_of_sun_alb");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Hib
                            if (t.Level > 39 && (player.IsCharcterClass(eCharacterClass.Blademaster) || player.IsCharcterClass(eCharacterClass.Champion)
                                || player.IsCharcterClass(eCharacterClass.Hero) || player.IsCharcterClass(eCharacterClass.Nightshade)
                                || player.IsCharcterClass(eCharacterClass.Ranger) || player.IsCharcterClass(eCharacterClass.Valewalker)
                                || player.IsCharcterClass(eCharacterClass.Vampiir) || player.IsCharcterClass(eCharacterClass.Warden)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("belt_of_the_sun");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//MID
                            if (t.Level > 39 && (player.IsCharcterClass(eCharacterClass.Shadowblade) || player.IsCharcterClass(eCharacterClass.Warrior)
                              || player.IsCharcterClass(eCharacterClass.Thane) || player.IsCharcterClass(eCharacterClass.Skald)
                              || player.IsCharcterClass(eCharacterClass.Berserker) || player.IsCharcterClass(eCharacterClass.Savage)
                              || player.IsCharcterClass(eCharacterClass.Hunter) || player.IsCharcterClass(eCharacterClass.Valkyrie)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("belt_of_the_sun_mid");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)40), eChatType.CT_System, eChatLoc.CL_PopupWindow);


                            break;
                        }
                    case "Apprentice's Notes": //Bracer of Zo'arkat
                        {// CLER, FRI, HER, SORC, THEU, WIZ, NECR, ANI, BAIN, BARD, DRU, ELD, ENCH, MENT, WARD, BD, HEAL, RM, SHA, SM, WARL
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cleric) || player.IsCharcterClass(eCharacterClass.Friar) //cloth
                               || player.IsCharcterClass(eCharacterClass.Heretic) || player.IsCharcterClass(eCharacterClass.Sorcerer)
                               || player.IsCharcterClass(eCharacterClass.Theurgist) || player.IsCharcterClass(eCharacterClass.Wizard)
                               || player.IsCharcterClass(eCharacterClass.Necromancer) || player.IsCharcterClass(eCharacterClass.Animist)
                               || player.IsCharcterClass(eCharacterClass.Bainshee) || player.IsCharcterClass(eCharacterClass.Bard)
                               || player.IsCharcterClass(eCharacterClass.Druid) || player.IsCharcterClass(eCharacterClass.Eldritch)
                               || player.IsCharcterClass(eCharacterClass.Enchanter) || player.IsCharcterClass(eCharacterClass.Mentalist)
                               || player.IsCharcterClass(eCharacterClass.Warden) || player.IsCharcterClass(eCharacterClass.Bonedancer)
                               || player.IsCharcterClass(eCharacterClass.Healer) || player.IsCharcterClass(eCharacterClass.Runemaster)
                               || player.IsCharcterClass(eCharacterClass.Shaman) || player.IsCharcterClass(eCharacterClass.Spiritmaster)
                               || player.IsCharcterClass(eCharacterClass.Warlock) || player.IsCharcterClass(eCharacterClass.Cabalist)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("bracer_of_zoarkat");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;
                        } //SCOU, RANG, HUNT
                    case "Carved Tablet": //Braggart's Bow
                        {
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Scout)) //Arms of the Winds sleeves
                            {//alb
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("braggarts_bow_alb");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Hib
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Ranger)) //Arms of the Winds sleeves
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("braggarts_bow");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//mid
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Hunter)) //Arms of the Winds sleeves
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("braggarts_bow_mid");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;
                        }
                    case "Tale of Bruiser": //Bruiser
                        { //ARMS, CLER, FRI, HER, MERC, PAL, REAV,    BARD, BM, CHA, DRU, HERO, WARD,    BER, HEAL, HUNT, SAV, SB, SHA, SKLD, THAN, WAR
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Armsman) || player.IsCharcterClass(eCharacterClass.Cleric)
                                || player.IsCharcterClass(eCharacterClass.Friar) || player.IsCharcterClass(eCharacterClass.Heretic)
                                || player.IsCharcterClass(eCharacterClass.Mercenary) || player.IsCharcterClass(eCharacterClass.Paladin)
                                || player.IsCharcterClass(eCharacterClass.Reaver)))
                            {//Alb
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Alb Bruiser 1h Crush Version],[Alb Bruiser 2h Crush Version]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }//Hib
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Bard) || player.IsCharcterClass(eCharacterClass.Blademaster)
                               || player.IsCharcterClass(eCharacterClass.Champion) || player.IsCharcterClass(eCharacterClass.Druid)
                               || player.IsCharcterClass(eCharacterClass.Hero) || player.IsCharcterClass(eCharacterClass.Warden)))
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Hib Bruiser 1h Blunt Version],[Hib Bruiser 2h Slash Version]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }//Mid
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Berserker) || player.IsCharcterClass(eCharacterClass.Healer)
                              || player.IsCharcterClass(eCharacterClass.Hunter) || player.IsCharcterClass(eCharacterClass.Savage)
                              || player.IsCharcterClass(eCharacterClass.Shadowblade) || player.IsCharcterClass(eCharacterClass.Skald)
                              || player.IsCharcterClass(eCharacterClass.Thane) || player.IsCharcterClass(eCharacterClass.Warrior)
                              || player.IsCharcterClass(eCharacterClass.Shaman)))
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Mid Bruiser 1h Crush Version],[Mid Bruiser 2h Crush Version]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;
                        }
                    case "Arbiter Papers": //Ceremonial Bracer
                        { //All
                            if (t.Level > 44)
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Ceremonial Bracer Acu Version],[Ceremonial Bracer Dex Version],[Ceremonial Bracer Qui Version] " +
                                    " ,[Ceremonial Bracer Con Version],[Ceremonial Bracer Str Version] ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;
                        }
                    case "Cloudsong": //Cloudsong
                        { //CAB, CLER, FRI, HER, NECR, SORC, THEU, WIZ, ANI, BARD, DRU, ELD, ENCH, MENT, WARD, VW, BD, HEAL, RM, SHA, SM, WARL

                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cabalist) || player.IsCharcterClass(eCharacterClass.Cleric)
                               || player.IsCharcterClass(eCharacterClass.Friar) || player.IsCharcterClass(eCharacterClass.Heretic)
                               || player.IsCharcterClass(eCharacterClass.Necromancer) || player.IsCharcterClass(eCharacterClass.Sorcerer)
                               || player.IsCharcterClass(eCharacterClass.Theurgist) || player.IsCharcterClass(eCharacterClass.Wizard)
                               || player.IsCharcterClass(eCharacterClass.Animist) || player.IsCharcterClass(eCharacterClass.Bard)
                               || player.IsCharcterClass(eCharacterClass.Druid) || player.IsCharcterClass(eCharacterClass.Eldritch)
                               || player.IsCharcterClass(eCharacterClass.Enchanter) || player.IsCharcterClass(eCharacterClass.Mentalist)
                               || player.IsCharcterClass(eCharacterClass.Warden) || player.IsCharcterClass(eCharacterClass.Valewalker)
                               || player.IsCharcterClass(eCharacterClass.Bonedancer) || player.IsCharcterClass(eCharacterClass.Healer)
                               || player.IsCharcterClass(eCharacterClass.Runemaster) || player.IsCharcterClass(eCharacterClass.Shaman)
                               || player.IsCharcterClass(eCharacterClass.Spiritmaster) || player.IsCharcterClass(eCharacterClass.Warlock)
                               || player.IsCharcterClass(eCharacterClass.Bainshee)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("cloudsong");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;
                        }
                    case "Tyrus Epic Poems": //Crocodile's Tears Ring
                        {// MIN, SORC, THEU, BAIN, BARD, ENCH, WARD, RM, SKLD
                            if (t.Level > 44)
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crocodile_tear_ring");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//ARMS, INF, MERC, MINS PAL, REAV, SCOU, BM, CHA, HERO, NS, RANG, BER, HUNT, SAV, THAN, SAV, VALK, WAR
                    case "Marricus Journal": //Crocodile's Tooth Dagger
                        {//Alb
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Armsman) || player.IsCharcterClass(eCharacterClass.Infiltrator)
                                || player.IsCharcterClass(eCharacterClass.Mercenary) || player.IsCharcterClass(eCharacterClass.Minstrel)
                                || player.IsCharcterClass(eCharacterClass.Paladin) || player.IsCharcterClass(eCharacterClass.Reaver)
                                || player.IsCharcterClass(eCharacterClass.Scout)))
                            { //crocodile_tooth_dagger_alb_crush crocodile_tooth_dagger_alb_slash crocodile_tooth_dagger_alb_thrust
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Alb Crocodile Tooth Dagger Crush Version],[Alb Crocodile Tooth Dagger Slash Version]"
                                    + " ,[Alb Crocodile Tooth Dagger Trust Version]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }//Hib
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Blademaster) || player.IsCharcterClass(eCharacterClass.Champion)
                                || player.IsCharcterClass(eCharacterClass.Hero) || player.IsCharcterClass(eCharacterClass.Nightshade)
                                || player.IsCharcterClass(eCharacterClass.Ranger)))
                            { //crocodiles_tooth_dagger_hib_pierce crocodiles_tooth_dagger_hib_blade crocodiles_tooth_dagger_hib_blunt
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Hib Crocodile Tooth Dagger Pierce Version],[Hib Crocodile Tooth Dagger Blade Version]"
                                    + " ,[Hib Crocodile Tooth Dagger blunt Version]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }//Hib Vampiir
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Vampiir))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crocodiles_tooth_dagger_hib_pierce");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Mid
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Berserker) || player.IsCharcterClass(eCharacterClass.Hunter)
                                || player.IsCharcterClass(eCharacterClass.Savage) || player.IsCharcterClass(eCharacterClass.Thane)
                                || player.IsCharcterClass(eCharacterClass.Valkyrie) || player.IsCharcterClass(eCharacterClass.Warrior)
                                || player.IsCharcterClass(eCharacterClass.Shadowblade)))
                            { //crocodiles_tooth_dagger_mid_axe  crocodiles_tooth_dagger_mid_hammer crocodiles_tooth_dagger_mid_sword

                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Mid Crocodile Tooth Dagger Axe Version],[Mid Crocodile Tooth Dagger Hammer Version]"
                                    + " ,[Mid Crocodile Tooth Dagger Sword Version]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);


                            break;
                        }//All
                    case "Crown of Zahur": //Crown of Zahur
                        {//Alb
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cabalist) || player.IsCharcterClass(eCharacterClass.Heretic)
                            || player.IsCharcterClass(eCharacterClass.Necromancer) || player.IsCharcterClass(eCharacterClass.Sorcerer)
                            || player.IsCharcterClass(eCharacterClass.Theurgist) || player.IsCharcterClass(eCharacterClass.Wizard)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crown_of_zahur_cloth_alb");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Scout))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crown_of_zahur_studded");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Mercenary) || player.IsCharcterClass(eCharacterClass.Reaver)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crown_of_zahur_chain");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Minstrel)
                            || player.IsCharcterClass(eCharacterClass.Cleric))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crown_of_zahur_chain_healer");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Armsman) || player.IsCharcterClass(eCharacterClass.Paladin)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crown_of_zahur_plate");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Infiltrator) || player.IsCharcterClass(eCharacterClass.Friar)
                          || player.IsCharcterClass(eCharacterClass.MaulerAlb)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crown_of_zahur_leather_alb");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            } //Hib
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Vampiir) || player.IsCharcterClass(eCharacterClass.Nightshade)
                            || player.IsCharcterClass(eCharacterClass.MaulerHib)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crown_of_zahur_leather_hib");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Animist) || player.IsCharcterClass(eCharacterClass.Eldritch)
                            || player.IsCharcterClass(eCharacterClass.Enchanter) || player.IsCharcterClass(eCharacterClass.Mentalist)
                            || player.IsCharcterClass(eCharacterClass.Valewalker) || player.IsCharcterClass(eCharacterClass.Bainshee)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crown_of_zahur_cloth_caster_hib");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Blademaster) || player.IsCharcterClass(eCharacterClass.Bard)
                            || player.IsCharcterClass(eCharacterClass.Ranger)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crown_of_zahur_reinforced");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Hero) || player.IsCharcterClass(eCharacterClass.Champion)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crown_of_zahur_scale_melee_hib");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Warden) || player.IsCharcterClass(eCharacterClass.Druid)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crown_of_zahur_scale_caster_hib");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Mid
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Bonedancer) || player.IsCharcterClass(eCharacterClass.Runemaster)
                             || player.IsCharcterClass(eCharacterClass.Spiritmaster) || player.IsCharcterClass(eCharacterClass.Warlock)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crown_of_zahur_cloth_mid");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Berserker) || player.IsCharcterClass(eCharacterClass.Hunter)
                               || player.IsCharcterClass(eCharacterClass.Savage)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crown_of_zahur_studded_mid");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Thane) || player.IsCharcterClass(eCharacterClass.Valkyrie)
                               || player.IsCharcterClass(eCharacterClass.Warrior)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crown_of_zahur_chain_mid");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Healer) || player.IsCharcterClass(eCharacterClass.Skald)
                             || player.IsCharcterClass(eCharacterClass.Shaman)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crown_of_zahur_chain_healer_mid");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.MaulerMid) || player.IsCharcterClass(eCharacterClass.Shadowblade)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crown_of_zahur_leather");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;
                        } // ARMS, CLER, FRI, INF, MERC, MINS, PAL, REAV, SCOU, BARD, BM, CHA, DRU, HERO, NS, RANG, WARD, BER, HEAL, HUNT, SAV, SB, SHA, SKLD, THAN, WAR   
                    case "Damyon's Journal": //Cyclops Eye Shield 
                        {//Alb
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Friar) || player.IsCharcterClass(eCharacterClass.Heretic)
                                || player.IsCharcterClass(eCharacterClass.Infiltrator) || player.IsCharcterClass(eCharacterClass.Minstrel)
                                 || player.IsCharcterClass(eCharacterClass.Scout))) //Aten's Shield small
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("cyclops_eye_shield_small_alb"); //Cyclops Shield small
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cleric) || player.IsCharcterClass(eCharacterClass.Mercenary)
                                || player.IsCharcterClass(eCharacterClass.Armsman) || player.IsCharcterClass(eCharacterClass.Paladin)
                                || player.IsCharcterClass(eCharacterClass.Reaver))) //Aten's Shield medium
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("cyclops_eye_shield_medium_alb"); //Cyclops Shield medium
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Hib 
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Bard) || player.IsCharcterClass(eCharacterClass.Druid)
                                || player.IsCharcterClass(eCharacterClass.Nightshade) || player.IsCharcterClass(eCharacterClass.Ranger)
                                || player.IsCharcterClass(eCharacterClass.Vampiir))) //Cyclops Shield small
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("cyclops_eye_shield_small");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Blademaster) || player.IsCharcterClass(eCharacterClass.Champion)
                                || player.IsCharcterClass(eCharacterClass.Hero) || player.IsCharcterClass(eCharacterClass.Warden)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("cyclops_eye_shield_medium"); //Cyclops Shield medium
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Mid    
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Berserker) || player.IsCharcterClass(eCharacterClass.Healer)
                                || player.IsCharcterClass(eCharacterClass.Hunter) || player.IsCharcterClass(eCharacterClass.Savage)
                                || player.IsCharcterClass(eCharacterClass.Shadowblade) || player.IsCharcterClass(eCharacterClass.Shaman)
                                || player.IsCharcterClass(eCharacterClass.Shaman))) //Cyclops Shield small
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("cyclops_eye_shield_small");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Skald) || player.IsCharcterClass(eCharacterClass.Thane)
                                || player.IsCharcterClass(eCharacterClass.Warrior) || player.IsCharcterClass(eCharacterClass.Berserker)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("cyclops_eye_shield_medium"); //Cyclops Shield medium
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;
                        }//All
                    case "Louka's Journal": //Dream Sphere
                        {
                            if (t.Level > 44)
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("dream_sphere");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }

                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;
                        }//All
                    case "Crafters Pages": //Eerie Darkness Lighting Stone
                        {
                            if (t.Level > 44)
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("eerie_darkness_stone");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;
                        }//CLER, FRI, HER, DRU, WARD, BARD, HEAL, SHA, VALK
                    case "Egg of Youth": //Egg of Youth
                        {
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cleric) || player.IsCharcterClass(eCharacterClass.Friar)
                                || player.IsCharcterClass(eCharacterClass.Heretic) || player.IsCharcterClass(eCharacterClass.Druid)
                                || player.IsCharcterClass(eCharacterClass.Warden) || player.IsCharcterClass(eCharacterClass.Bard)
                                || player.IsCharcterClass(eCharacterClass.Healer) || player.IsCharcterClass(eCharacterClass.Shaman)
                                || player.IsCharcterClass(eCharacterClass.Valkyrie)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("egg_of_youth");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//ARMS, CLER, MERC, MINS PAL, REAV, DRU, CHA, HERO, WARD, HEAL, SHA, SKLD, THAN, VALK, WAR
                    case "Eirene's Journal": //Eirene's Hauberk
                        {//Alb
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Armsman) || player.IsCharcterClass(eCharacterClass.Paladin)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("eirenes_chestpiece_plate_melee_alb");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Mercenary) || player.IsCharcterClass(eCharacterClass.Reaver)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("eirenes_chestpiece_chain_melee_alb");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cleric) || player.IsCharcterClass(eCharacterClass.Reaver)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("eirenes_chestpiece_chain_healer_alb");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Minstrel))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("eirenes_chestpiece_chain_minstrel_alb");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Hib DRU, CHA, HERO, WARD,
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Hero) || player.IsCharcterClass(eCharacterClass.Champion)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("eirenes_chestpiece_scale_melee_hib");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Druid))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("eirenes_chestpiece_scale_caster_hib");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Warden)) //Arms of the Winds sleeves
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("eirenes_chestpiece_scale_hybrid_hib");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Mid  HEAL, SHA, SKLD, THAN, VALK, WAR
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Thane) || player.IsCharcterClass(eCharacterClass.Warrior)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("eirenes_chestpiece_chain_melee_mid");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Healer) || player.IsCharcterClass(eCharacterClass.Shaman)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("eirenes_chestpiece_healer");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Skald) || player.IsCharcterClass(eCharacterClass.Valkyrie))) //Arms of the Winds sleeves
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("eirenes_chestpiece_skald_mid");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }// ARMS, CLER, MERC, MINS, PAL, REAV, SCOU, BARD, BM, CHA, DRU, HERO, RANG, WARD, BER, HEAL, HUNT, SAV, SHA, SKLD, THAN, VALK, WAR
                    case "Enyalio's Boots": //Enyalio's Boots
                        {//Alb
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Armsman) || player.IsCharcterClass(eCharacterClass.Paladin)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("enyalios_boots_plate");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cleric) || player.IsCharcterClass(eCharacterClass.Mercenary)
                                || player.IsCharcterClass(eCharacterClass.Reaver) || player.IsCharcterClass(eCharacterClass.Minstrel)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("enyalios_boots_chain");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Scout))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("enyalios_boots_studded");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Hib BARD, BM, CHA, DRU, HERO, RANG, WARD,
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Hero) || player.IsCharcterClass(eCharacterClass.Warden)
                                || player.IsCharcterClass(eCharacterClass.Druid) || player.IsCharcterClass(eCharacterClass.Champion)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("enyalios_boots_scale");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Bard) || player.IsCharcterClass(eCharacterClass.Blademaster)
                                || player.IsCharcterClass(eCharacterClass.Ranger)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("enyalios_boots_reinforced");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Mid  BER, HEAL, HUNT, SAV, SHA, SKLD, THAN, VALK, WAR
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Healer) || player.IsCharcterClass(eCharacterClass.Shaman)
                                || player.IsCharcterClass(eCharacterClass.Skald) || player.IsCharcterClass(eCharacterClass.Thane)
                                || player.IsCharcterClass(eCharacterClass.Warrior) || player.IsCharcterClass(eCharacterClass.Valkyrie)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("enyalios_boots_chain");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Berserker) || player.IsCharcterClass(eCharacterClass.Hunter)
                                || player.IsCharcterClass(eCharacterClass.Savage)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("enyalios_boots_studded");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;
                        } // CAB, NECR, SORC, THEU, WIZ, ANI, BAIN, ELD, ENCH, MENT, BD, RM, SM, WARL
                    case "Song of Erinys": //Erinys Charm
                        {
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cabalist) || player.IsCharcterClass(eCharacterClass.Necromancer)
                                || player.IsCharcterClass(eCharacterClass.Sorcerer) || player.IsCharcterClass(eCharacterClass.Theurgist)
                                || player.IsCharcterClass(eCharacterClass.Wizard) || player.IsCharcterClass(eCharacterClass.Animist)
                                || player.IsCharcterClass(eCharacterClass.Bainshee) || player.IsCharcterClass(eCharacterClass.Eldritch)
                                || player.IsCharcterClass(eCharacterClass.Enchanter) || player.IsCharcterClass(eCharacterClass.Mentalist)
                                || player.IsCharcterClass(eCharacterClass.Bonedancer) || player.IsCharcterClass(eCharacterClass.Runemaster)
                                || player.IsCharcterClass(eCharacterClass.Spiritmaster) || player.IsCharcterClass(eCharacterClass.Warlock)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("erinys_charm");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else

                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//CAB, CLER, FRI, HER, NECR, SORC, THEU, WIZ, ANI, BAIN, BARD, ELD, ENCH, MENT, WARD, BD, HEAL, RM, SHA, SM, WARL
                    case "Healer's Notes": //Eternal Plant
                        {
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cabalist) || player.IsCharcterClass(eCharacterClass.Cleric)
                                || player.IsCharcterClass(eCharacterClass.Friar) || player.IsCharcterClass(eCharacterClass.Heretic)
                                || player.IsCharcterClass(eCharacterClass.Necromancer) || player.IsCharcterClass(eCharacterClass.Sorcerer)
                                || player.IsCharcterClass(eCharacterClass.Sorcerer) || player.IsCharcterClass(eCharacterClass.Theurgist)
                                || player.IsCharcterClass(eCharacterClass.Wizard) || player.IsCharcterClass(eCharacterClass.Animist)
                                || player.IsCharcterClass(eCharacterClass.Bainshee) || player.IsCharcterClass(eCharacterClass.Bard)
                                || player.IsCharcterClass(eCharacterClass.Eldritch) || player.IsCharcterClass(eCharacterClass.Enchanter)
                                || player.IsCharcterClass(eCharacterClass.Mentalist) || player.IsCharcterClass(eCharacterClass.Warden)
                                || player.IsCharcterClass(eCharacterClass.Bonedancer) || player.IsCharcterClass(eCharacterClass.Healer)
                                || player.IsCharcterClass(eCharacterClass.Runemaster) || player.IsCharcterClass(eCharacterClass.Shaman)
                                || player.IsCharcterClass(eCharacterClass.Spiritmaster) || player.IsCharcterClass(eCharacterClass.Warlock)
                                || player.IsCharcterClass(eCharacterClass.Druid)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("Eternal_Plant_neu");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }// CAB, FRI, HER, INF, NECR, SORC, THEU, WIZ, ANI, BAIN, ELD, ENCH, MENT, NS, VW, VAMP BD, RM, SB, SM, WARL, MAUL
                    case "King Kiron's": //Flamedancer's Boots
                        {
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cabalist) || player.IsCharcterClass(eCharacterClass.Necromancer)
                                || player.IsCharcterClass(eCharacterClass.Sorcerer) || player.IsCharcterClass(eCharacterClass.Theurgist)
                                || player.IsCharcterClass(eCharacterClass.Wizard) || player.IsCharcterClass(eCharacterClass.Animist)
                                || player.IsCharcterClass(eCharacterClass.Bainshee) || player.IsCharcterClass(eCharacterClass.Eldritch)
                                || player.IsCharcterClass(eCharacterClass.Mentalist) || player.IsCharcterClass(eCharacterClass.Bonedancer)
                                || player.IsCharcterClass(eCharacterClass.Runemaster) || player.IsCharcterClass(eCharacterClass.Spiritmaster)
                                || player.IsCharcterClass(eCharacterClass.Warlock) || player.IsCharcterClass(eCharacterClass.Enchanter)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("flamedancers_boots_cloth");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }// CAB, FRI, HER, INF, NECR, SORC, THEU, WIZ, ANI, BAIN, ELD, ENCH, MENT, NS, VW, VAMP BD, RM, SB, SM, WARL, MAUL
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Heretic) || player.IsCharcterClass(eCharacterClass.Valewalker)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("flamedancers_boots_cloth_melee");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Friar) || player.IsCharcterClass(eCharacterClass.Infiltrator)
                               || player.IsCharcterClass(eCharacterClass.Nightshade) || player.IsCharcterClass(eCharacterClass.Vampiir)
                                || player.IsCharcterClass(eCharacterClass.Shadowblade) || player.IsCharcterClass(eCharacterClass.MaulerAlb)
                                || player.IsCharcterClass(eCharacterClass.MaulerHib) || player.IsCharcterClass(eCharacterClass.MaulerMid)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("flamedancers_boots_leather");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }

                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//All
                    case "Tale of a Flask": //A Flask 
                        {
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cabalist) || player.IsCharcterClass(eCharacterClass.Cleric)
                               || player.IsCharcterClass(eCharacterClass.Friar) || player.IsCharcterClass(eCharacterClass.Heretic)
                                || player.IsCharcterClass(eCharacterClass.Necromancer) || player.IsCharcterClass(eCharacterClass.Sorcerer)
                                || player.IsCharcterClass(eCharacterClass.Theurgist) || player.IsCharcterClass(eCharacterClass.Wizard)
                                || player.IsCharcterClass(eCharacterClass.Animist) || player.IsCharcterClass(eCharacterClass.Bainshee)
                                || player.IsCharcterClass(eCharacterClass.Bard) || player.IsCharcterClass(eCharacterClass.Druid)
                                || player.IsCharcterClass(eCharacterClass.Eldritch) || player.IsCharcterClass(eCharacterClass.Enchanter)
                                || player.IsCharcterClass(eCharacterClass.Mentalist) || player.IsCharcterClass(eCharacterClass.Cabalist)
                                || player.IsCharcterClass(eCharacterClass.Bonedancer) || player.IsCharcterClass(eCharacterClass.Healer)
                                || player.IsCharcterClass(eCharacterClass.Runemaster) || player.IsCharcterClass(eCharacterClass.Shaman)
                                || player.IsCharcterClass(eCharacterClass.Spiritmaster) || player.IsCharcterClass(eCharacterClass.Warlock)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("a_flask");//Caster Version
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Armsman) || player.IsCharcterClass(eCharacterClass.Infiltrator)
                                || player.IsCharcterClass(eCharacterClass.MaulerAlb) || player.IsCharcterClass(eCharacterClass.MaulerHib)
                                || player.IsCharcterClass(eCharacterClass.MaulerMid) || player.IsCharcterClass(eCharacterClass.Mercenary)
                                || player.IsCharcterClass(eCharacterClass.Minstrel) || player.IsCharcterClass(eCharacterClass.Paladin)
                                || player.IsCharcterClass(eCharacterClass.Reaver) || player.IsCharcterClass(eCharacterClass.Scout)
                                || player.IsCharcterClass(eCharacterClass.Blademaster) || player.IsCharcterClass(eCharacterClass.Champion)
                                || player.IsCharcterClass(eCharacterClass.Hero) || player.IsCharcterClass(eCharacterClass.Nightshade)
                                || player.IsCharcterClass(eCharacterClass.Ranger) || player.IsCharcterClass(eCharacterClass.Valewalker)
                                || player.IsCharcterClass(eCharacterClass.Vampiir) || player.IsCharcterClass(eCharacterClass.Warden)
                                || player.IsCharcterClass(eCharacterClass.Berserker) || player.IsCharcterClass(eCharacterClass.Hunter)
                                || player.IsCharcterClass(eCharacterClass.Savage) || player.IsCharcterClass(eCharacterClass.Shadowblade)
                                || player.IsCharcterClass(eCharacterClass.Skald) || player.IsCharcterClass(eCharacterClass.Thane)
                                || player.IsCharcterClass(eCharacterClass.Valkyrie) || player.IsCharcterClass(eCharacterClass.Warlock)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("a_flask_neu");//Tanke Version
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//SCOU, RANG, HUNT
                    case "Fool's Bow": //Fool's Bow 
                        {
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Scout))
                            {//Alb
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("fools_bow_alb");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Hib
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Ranger))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("fools_bow");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Mid
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Hunter))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("fools_bow_mid");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;
                        }// CAB, FRI, HER, INF, NECR, SCOU, SORC, THEU, WIZ, ANI, BAIN, BARD, BM, ELD, ENCH, MENT, NS, RANG, VW, VAMP, BER, BD, HUNT, RM, SAV, SB, SM, WARL, MAUL
                    case "Foppish Sleeves": //Foppish Sleeves
                        {
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cabalist) || player.IsCharcterClass(eCharacterClass.Heretic)
                               || player.IsCharcterClass(eCharacterClass.Necromancer) || player.IsCharcterClass(eCharacterClass.Sorcerer)
                               || player.IsCharcterClass(eCharacterClass.Theurgist) || player.IsCharcterClass(eCharacterClass.Wizard)
                               || player.IsCharcterClass(eCharacterClass.Animist) || player.IsCharcterClass(eCharacterClass.Bainshee)
                                || player.IsCharcterClass(eCharacterClass.Eldritch) || player.IsCharcterClass(eCharacterClass.Enchanter)
                                || player.IsCharcterClass(eCharacterClass.Mentalist) || player.IsCharcterClass(eCharacterClass.Valewalker)
                                || player.IsCharcterClass(eCharacterClass.Bonedancer) || player.IsCharcterClass(eCharacterClass.Runemaster)
                                || player.IsCharcterClass(eCharacterClass.Spiritmaster) || player.IsCharcterClass(eCharacterClass.Warlock)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("foppish_sleeves_cloth_caster");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }// CAB, FRI, HER, INF, NECR, SCOU, SORC, THEU, WIZ, ANI, BAIN, BARD, BM, ELD, ENCH, MENT, NS, RANG, VW, VAMP, BER, BD, HUNT, RM, SAV, SB, SM, WARL, MAUL
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Friar) || player.IsCharcterClass(eCharacterClass.Infiltrator)
                                || player.IsCharcterClass(eCharacterClass.Nightshade) || player.IsCharcterClass(eCharacterClass.Vampiir)
                                || player.IsCharcterClass(eCharacterClass.Shadowblade) || player.IsCharcterClass(eCharacterClass.MaulerAlb)
                                || player.IsCharcterClass(eCharacterClass.MaulerHib) || player.IsCharcterClass(eCharacterClass.MaulerMid)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("foppish_sleeves_leather");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }// CAB, FRI, HER, INF, NECR, SCOU, SORC, THEU, WIZ, ANI, BAIN, BARD, BM, ELD, ENCH, MENT, NS, RANG, VW, VAMP, BER, BD, HUNT, RM, SAV, SB, SM, WARL, MAUL
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Bard) || player.IsCharcterClass(eCharacterClass.Ranger)
                                || player.IsCharcterClass(eCharacterClass.Blademaster)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("foppish_sleeves_reinforced");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }// CAB, FRI, HER, INF, NECR, SCOU, SORC, THEU, WIZ, ANI, BAIN, BARD, BM, ELD, ENCH, MENT, NS, RANG, VW, VAMP, BER, BD, HUNT, RM, SAV, SB, SM, WARL, MAUL
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Berserker) || player.IsCharcterClass(eCharacterClass.Hunter)
                                || player.IsCharcterClass(eCharacterClass.Scout) || player.IsCharcterClass(eCharacterClass.Savage)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("foppish_sleeves_studded");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;
                        }//All
                    case "Gem of Lost Memories": //Gem of Lost Memories
                        {//Archer
                            if (t.Level > 29 && (player.IsCharcterClass(eCharacterClass.Scout) || player.IsCharcterClass(eCharacterClass.Ranger)
                                || player.IsCharcterClass(eCharacterClass.Hunter)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("gem_of_lost_memories_archer");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Caster
                            if (t.Level > 29 && (player.IsCharcterClass(eCharacterClass.Cabalist) || player.IsCharcterClass(eCharacterClass.Necromancer)
                              || player.IsCharcterClass(eCharacterClass.Sorcerer) || player.IsCharcterClass(eCharacterClass.Theurgist)
                              || player.IsCharcterClass(eCharacterClass.Wizard) || player.IsCharcterClass(eCharacterClass.Animist)
                              || player.IsCharcterClass(eCharacterClass.Bainshee) || player.IsCharcterClass(eCharacterClass.Eldritch)
                              || player.IsCharcterClass(eCharacterClass.Enchanter) || player.IsCharcterClass(eCharacterClass.Mentalist)
                              || player.IsCharcterClass(eCharacterClass.Bonedancer) || player.IsCharcterClass(eCharacterClass.Runemaster)
                              || player.IsCharcterClass(eCharacterClass.Spiritmaster) || player.IsCharcterClass(eCharacterClass.Warlock)
                              || player.IsCharcterClass(eCharacterClass.Shaman) || player.IsCharcterClass(eCharacterClass.Healer)
                              || player.IsCharcterClass(eCharacterClass.Druid) || player.IsCharcterClass(eCharacterClass.Cleric)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("gem_of_lost_memories_caster");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Melee
                            if (t.Level > 29 && (player.IsCharcterClass(eCharacterClass.Armsman) || player.IsCharcterClass(eCharacterClass.Friar)
                              || player.IsCharcterClass(eCharacterClass.Heretic) || player.IsCharcterClass(eCharacterClass.Infiltrator)
                              || player.IsCharcterClass(eCharacterClass.MaulerAlb) || player.IsCharcterClass(eCharacterClass.MaulerHib)
                              || player.IsCharcterClass(eCharacterClass.MaulerMid) || player.IsCharcterClass(eCharacterClass.Mercenary)
                              || player.IsCharcterClass(eCharacterClass.Minstrel) || player.IsCharcterClass(eCharacterClass.Paladin)
                              || player.IsCharcterClass(eCharacterClass.Reaver) || player.IsCharcterClass(eCharacterClass.Bard)
                              || player.IsCharcterClass(eCharacterClass.Blademaster) || player.IsCharcterClass(eCharacterClass.Champion)
                              || player.IsCharcterClass(eCharacterClass.Hero) || player.IsCharcterClass(eCharacterClass.Nightshade)
                              || player.IsCharcterClass(eCharacterClass.Valewalker) || player.IsCharcterClass(eCharacterClass.Vampiir)
                              || player.IsCharcterClass(eCharacterClass.Warden) || player.IsCharcterClass(eCharacterClass.Berserker)
                              || player.IsCharcterClass(eCharacterClass.Savage) || player.IsCharcterClass(eCharacterClass.Shadowblade)
                              || player.IsCharcterClass(eCharacterClass.Skald) || player.IsCharcterClass(eCharacterClass.Thane)
                              || player.IsCharcterClass(eCharacterClass.Valkyrie) || player.IsCharcterClass(eCharacterClass.Warrior)))
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Gem of Lost Memories Caster Version],[Gem of Lost Memories Melee Version]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)30), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;


                        }//ARMS, INF, MERC, MINS, PAL, REAV, SCOU, BM, CHA, HERO, NS, RANG, VW, BER, HUNT, SAV, SB, SKLD, THAN, VALK, WAR, MAUL
                    case "Diannas Letters": //Goddess' Necklace
                        {
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Armsman) || player.IsCharcterClass(eCharacterClass.Infiltrator)
                                || player.IsCharcterClass(eCharacterClass.Mercenary) || player.IsCharcterClass(eCharacterClass.Minstrel)
                                || player.IsCharcterClass(eCharacterClass.Paladin) || player.IsCharcterClass(eCharacterClass.Reaver)
                                || player.IsCharcterClass(eCharacterClass.Scout) || player.IsCharcterClass(eCharacterClass.Blademaster)
                                || player.IsCharcterClass(eCharacterClass.Champion) || player.IsCharcterClass(eCharacterClass.Hero)
                                || player.IsCharcterClass(eCharacterClass.Nightshade) || player.IsCharcterClass(eCharacterClass.Ranger)
                                || player.IsCharcterClass(eCharacterClass.Valewalker) || player.IsCharcterClass(eCharacterClass.Berserker)
                                || player.IsCharcterClass(eCharacterClass.Hunter) || player.IsCharcterClass(eCharacterClass.Savage)
                                || player.IsCharcterClass(eCharacterClass.Shadowblade) || player.IsCharcterClass(eCharacterClass.Skald)
                                || player.IsCharcterClass(eCharacterClass.Thane) || player.IsCharcterClass(eCharacterClass.Valkyrie)
                                || player.IsCharcterClass(eCharacterClass.Warrior) || player.IsCharcterClass(eCharacterClass.MaulerAlb)
                                || player.IsCharcterClass(eCharacterClass.MaulerHib) || player.IsCharcterClass(eCharacterClass.MaulerMid)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("goddess_necklace");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;
                        }//INF, SCOU, BARD, BM, NS, RANG, VAMP, BER, HUNT, SAV, SB, MAUL
                    case "Bence's Letters": //Golden Scarab Vest
                        {//Leather All
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Infiltrator) || player.IsCharcterClass(eCharacterClass.Nightshade)
                                || player.IsCharcterClass(eCharacterClass.Shadowblade) || player.IsCharcterClass(eCharacterClass.MaulerAlb)
                                || player.IsCharcterClass(eCharacterClass.MaulerHib) || player.IsCharcterClass(eCharacterClass.MaulerMid)
                                || player.IsCharcterClass(eCharacterClass.Vampiir)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("golden_scarab_vest_leather");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Alb
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Scout))
                            {//Studded Archer
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("golden_scarab_vest");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Mid
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Hunter))
                            {//Hunter
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("golden_scarab_vest");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Berserker)
                                || player.IsCharcterClass(eCharacterClass.Savage)))
                            {//Berserker - Savage
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("golden_scarab_vest_meele_mid_alb");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Hib
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Blademaster))
                            {//Blademaster
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("golden_scarab_vest_melee");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Ranger
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Ranger))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("golden_scarab_vest_Ranger");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Bard
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Bard))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("golden_scarab_vest_reinf");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                t.Out.SendMessage(t.Name + " Congratulations you have a new Artifact!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//INF, MINS, SCOU, HERO, NS, RANG, VAMP, HUNT, SB, VALK
                    case "Spear's History": //Golden Spear
                        {//Alb
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Infiltrator) || player.IsCharcterClass(eCharacterClass.Minstrel)
                                   || player.IsCharcterClass(eCharacterClass.Scout)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("the_golden_spear_alb_1h");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                t.Out.SendMessage(t.Name + " Congratulations you have a new Artifact!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Hib //INF, MINS, SCOU, HERO, NS, RANG, VAMP, HUNT, SB, VALK
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Hero))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("golden_spear_hib_2h");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Nightshade) || player.IsCharcterClass(eCharacterClass.Ranger)
                                || player.IsCharcterClass(eCharacterClass.Vampiir)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("golden_spear_hib");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Mid //INF, MINS, SCOU, HERO, NS, RANG, VAMP, HUNT, SB, VALK
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Hunter) || player.IsCharcterClass(eCharacterClass.Shadowblade)
                                || player.IsCharcterClass(eCharacterClass.Valkyrie)))
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Golden Spear 1h Sword Version],[Golden Spear 2h Spear Version]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;
                        }//All
                    case "A Love Story": //Guard of Valor vest
                        {//Alb
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cabalist) || player.IsCharcterClass(eCharacterClass.Necromancer)
                                || player.IsCharcterClass(eCharacterClass.Sorcerer) || player.IsCharcterClass(eCharacterClass.Theurgist)
                                || player.IsCharcterClass(eCharacterClass.Wizard)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("guard_of_valor_cloth_caster");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Heretic))
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Guard of Valor Heretic Caster Version], or [Guard of Valor Heretic Melee Version]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Friar) || player.IsCharcterClass(eCharacterClass.Infiltrator)
                                || player.IsCharcterClass(eCharacterClass.MaulerAlb)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("guard_of_valor_leather");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Cleric))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("guard_of_valor_chain_caster");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Mercenary) || player.IsCharcterClass(eCharacterClass.Minstrel)
                                || player.IsCharcterClass(eCharacterClass.Reaver)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("guard_of_valor_chain_melee");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Armsman) || player.IsCharcterClass(eCharacterClass.Paladin)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("guard_of_valor_plate");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Scout))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("guard_of_valor_studded_archer_alb");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//All
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Animist) || player.IsCharcterClass(eCharacterClass.Bainshee)
                                 || player.IsCharcterClass(eCharacterClass.Eldritch) || player.IsCharcterClass(eCharacterClass.Enchanter)
                                 || player.IsCharcterClass(eCharacterClass.Mentalist)))
                            { //Hib
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("guard_of_valor_cloth_caster");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Bard) || player.IsCharcterClass(eCharacterClass.Blademaster)))
                            { //Hib
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("guard_of_valor_reinf");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.MaulerHib) || player.IsCharcterClass(eCharacterClass.Nightshade)
                                 || player.IsCharcterClass(eCharacterClass.Vampiir)))
                            { //Hib
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("guard_of_valor_leather");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Champion) || player.IsCharcterClass(eCharacterClass.Hero)
                                 || player.IsCharcterClass(eCharacterClass.Warden)))
                            { //Hib
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("guard_of_valor_scale_melee");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Druid))
                            { //Hib
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("guard_of_valor_hib_healer");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Valewalker))
                            { //Hib
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("guard_of_valor_cloth_melee");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Ranger))
                            { //Hib
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("guard_of_valor_reinforced_archer");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//All
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Bonedancer) || player.IsCharcterClass(eCharacterClass.Runemaster)
                                 || player.IsCharcterClass(eCharacterClass.Spiritmaster) || player.IsCharcterClass(eCharacterClass.Warlock)))
                            { //Mid
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("guard_of_valor_cloth_caster");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.MaulerMid) || player.IsCharcterClass(eCharacterClass.Shadowblade)))
                            { //Mid
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("guard_of_valor_leather");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Warrior)))
                            { //Mid
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("guard_of_valor_chain_mid_melee");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Thane) || player.IsCharcterClass(eCharacterClass.Skald) || player.IsCharcterClass(eCharacterClass.Valkyrie)))
                            { //Mid
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("guard_of_valor_chain_mid2");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Healer) || player.IsCharcterClass(eCharacterClass.Shaman)))
                            { //Mid
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("guard_of_valor_chain_caster");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Berserker) || player.IsCharcterClass(eCharacterClass.Savage)))
                            { //Mid
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("guard_of_valor_studded");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Hunter))
                            { //Mid
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("guard_of_valor_studded_archer_mid");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;
                        } //ARMS, MERC, PAL, REAV, BM, CHA, HERO, VW, VAMP, WARD, BER, SAV, SKLD, THAN, VALK, WAR, MAUL
                    case "Bellona's Diary": //Harpy Feather Cloak
                        {
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Armsman) || player.IsCharcterClass(eCharacterClass.Mercenary)
                                || player.IsCharcterClass(eCharacterClass.Paladin) || player.IsCharcterClass(eCharacterClass.Reaver)
                                || player.IsCharcterClass(eCharacterClass.Blademaster) || player.IsCharcterClass(eCharacterClass.Champion)
                                || player.IsCharcterClass(eCharacterClass.Warden) || player.IsCharcterClass(eCharacterClass.Hero)
                                || player.IsCharcterClass(eCharacterClass.Valewalker) || player.IsCharcterClass(eCharacterClass.Vampiir)
                                || player.IsCharcterClass(eCharacterClass.Berserker) || player.IsCharcterClass(eCharacterClass.Savage)
                                || player.IsCharcterClass(eCharacterClass.Skald) || player.IsCharcterClass(eCharacterClass.Thane)
                                || player.IsCharcterClass(eCharacterClass.Valkyrie) || player.IsCharcterClass(eCharacterClass.Warrior)
                                || player.IsCharcterClass(eCharacterClass.MaulerAlb) || player.IsCharcterClass(eCharacterClass.MaulerHib)
                                || player.IsCharcterClass(eCharacterClass.MaulerMid) || player.IsCharcterClass(eCharacterClass.Friar)
                                || player.IsCharcterClass(eCharacterClass.Heretic)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("harpy_feather_cloak");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;

                        }//CLER, FRI, HER, BARD, DRU, MENT, WARD, HEAL, SHA
                    case "Vara's Medical Logs": //Healer's Embrace Cloak
                        {
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cleric) || player.IsCharcterClass(eCharacterClass.Friar)
                                || player.IsCharcterClass(eCharacterClass.Heretic) || player.IsCharcterClass(eCharacterClass.Bard)
                                || player.IsCharcterClass(eCharacterClass.Druid) || player.IsCharcterClass(eCharacterClass.Mentalist)
                                || player.IsCharcterClass(eCharacterClass.Warden) || player.IsCharcterClass(eCharacterClass.Healer)
                                || player.IsCharcterClass(eCharacterClass.Shaman)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("healers_embrace");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                            break;
                        }//CAB, CLER, FRI, HER, NECR, SORC, THEU, WIZ, ANI, BAIN, BARD, DRU, ELD, ENCH, MENT, WARD, BD, HEAL, RM, SHA, SM, WARL
                    case "Tarin's Animal Skins": //Jacina's Sash belt
                        {
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cabalist) || player.IsCharcterClass(eCharacterClass.Cleric)
                                || player.IsCharcterClass(eCharacterClass.Friar) || player.IsCharcterClass(eCharacterClass.Heretic)
                                || player.IsCharcterClass(eCharacterClass.Necromancer) || player.IsCharcterClass(eCharacterClass.Sorcerer)
                                || player.IsCharcterClass(eCharacterClass.Theurgist) || player.IsCharcterClass(eCharacterClass.Wizard)
                                || player.IsCharcterClass(eCharacterClass.Animist) || player.IsCharcterClass(eCharacterClass.Bainshee)
                                || player.IsCharcterClass(eCharacterClass.Bard) || player.IsCharcterClass(eCharacterClass.Druid)
                                || player.IsCharcterClass(eCharacterClass.Eldritch) || player.IsCharcterClass(eCharacterClass.Enchanter)
                                || player.IsCharcterClass(eCharacterClass.Mentalist) || player.IsCharcterClass(eCharacterClass.Warden)
                                || player.IsCharcterClass(eCharacterClass.Bonedancer) || player.IsCharcterClass(eCharacterClass.Healer)
                                || player.IsCharcterClass(eCharacterClass.Runemaster) || player.IsCharcterClass(eCharacterClass.Shaman)
                                || player.IsCharcterClass(eCharacterClass.Warlock) || player.IsCharcterClass(eCharacterClass.Spiritmaster)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("jacinas_sash");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//All
                    case "Kalare's Memoires": //Kalare's Necklace
                        {
                            if (t.Level > 44)
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("kalares_necklace");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//	All
                    case "Mad Tales": //Maddening Scalars gloves
                        {//Alb
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cabalist) || player.IsCharcterClass(eCharacterClass.Necromancer)
                               || player.IsCharcterClass(eCharacterClass.Sorcerer) || player.IsCharcterClass(eCharacterClass.Theurgist)
                               || player.IsCharcterClass(eCharacterClass.Wizard)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("maddening_scalars_cloth");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Heretic))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("maddening_scalars_cloth");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Infiltrator) || player.IsCharcterClass(eCharacterClass.Friar)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("maddening_scalars_leather_melee");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.MaulerAlb)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("maddening_scalars_leather");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Scout))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("maddening_scalars_studded");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cleric) || player.IsCharcterClass(eCharacterClass.Minstrel) || player.IsCharcterClass(eCharacterClass.Reaver)
                               || player.IsCharcterClass(eCharacterClass.Mercenary)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("maddening_scalars_chain");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Armsman) || player.IsCharcterClass(eCharacterClass.Paladin)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("maddening_scalars_plate");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }  //Hib 
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Valewalker))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("maddening_scalars_cloth");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Hib
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Animist) || player.IsCharcterClass(eCharacterClass.Bainshee)
                               || player.IsCharcterClass(eCharacterClass.Eldritch) || player.IsCharcterClass(eCharacterClass.Enchanter)
                                || player.IsCharcterClass(eCharacterClass.Mentalist)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("maddening_scalars_cloth");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Hib
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Nightshade) || player.IsCharcterClass(eCharacterClass.MaulerHib)
                               || player.IsCharcterClass(eCharacterClass.Vampiir)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("maddening_scalars_leather");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Hib
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Ranger) || player.IsCharcterClass(eCharacterClass.Blademaster)
                               || player.IsCharcterClass(eCharacterClass.Bard)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("maddening_scalars_reinforced");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Hib
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Hero) || player.IsCharcterClass(eCharacterClass.Champion)
                               || player.IsCharcterClass(eCharacterClass.Druid) || player.IsCharcterClass(eCharacterClass.Warden)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("maddening_scalars_scale");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//MId
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Bonedancer) || player.IsCharcterClass(eCharacterClass.Runemaster)
                               || player.IsCharcterClass(eCharacterClass.Spiritmaster) || player.IsCharcterClass(eCharacterClass.Warlock)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("maddening_scalars_cloth");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.MaulerMid) || player.IsCharcterClass(eCharacterClass.Shadowblade)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("maddening_scalars_leather");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Hunter) || player.IsCharcterClass(eCharacterClass.Berserker)
                               || player.IsCharcterClass(eCharacterClass.Savage)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("maddening_scalars_studded");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Healer) || player.IsCharcterClass(eCharacterClass.Shaman)
                               || player.IsCharcterClass(eCharacterClass.Skald) || player.IsCharcterClass(eCharacterClass.Thane)
                               || player.IsCharcterClass(eCharacterClass.Valkyrie) || player.IsCharcterClass(eCharacterClass.Warrior)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("maddening_scalars_chain");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//ARMS, CLER, HER, INF, MERC, MINS, PAL, REAV, SCOU, BARD, BM, CHA, DRU, HERO, NS, RANG, WARD, BER, HEAL, SAV, SB, SHA, SKLD, THAN, WAR
                    case "Story of Malice": //Malice Axe
                        {//Alb
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cleric) || player.IsCharcterClass(eCharacterClass.Heretic)
                                || player.IsCharcterClass(eCharacterClass.Minstrel) || player.IsCharcterClass(eCharacterClass.Scout)
                                || player.IsCharcterClass(eCharacterClass.Mercenary) || player.IsCharcterClass(eCharacterClass.Reaver)
                                || player.IsCharcterClass(eCharacterClass.Infiltrator)))
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Alb Malice Slash 1h Version],[Alb Malice Crush 1h Version]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (player.IsCharcterClass(eCharacterClass.Armsman) || player.IsCharcterClass(eCharacterClass.Paladin))
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Alb Malice Slash 1h Version],[Alb Malice Crush 1h Version]"
                                    + " ,[Alb Malice Slash 2h Version] ,[Alb Malice Crush 2h Version]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }//Hib //ARMS, CLER, HER, INF, MERC, MINS, PAL, REAV, SCOU, BARD, BM, CHA, DRU, HERO, NS, RANG, WARD, BER, HEAL, SAV, SB, SHA, SKLD, THAN, WAR
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Bard) || player.IsCharcterClass(eCharacterClass.Blademaster)
                            || player.IsCharcterClass(eCharacterClass.Druid) || player.IsCharcterClass(eCharacterClass.Nightshade)
                            || player.IsCharcterClass(eCharacterClass.Ranger) || player.IsCharcterClass(eCharacterClass.Warden)))
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Hib Malice Slash 1h Version],[Hib Malice Crush 1h Version]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (player.IsCharcterClass(eCharacterClass.Hero) || player.IsCharcterClass(eCharacterClass.Champion))
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Hib Malice Slash 1h Version],[Hib Malice Crush 1h Version]"
                                    + " ,[Hib Malice Slash 2h Version] ,[Hib Malice Crush 2h Version]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }//Mid BER, HEAL, SAV, SB, SHA, SKLD, THAN, WAR
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Berserker) || player.IsCharcterClass(eCharacterClass.Healer)
                            || player.IsCharcterClass(eCharacterClass.Shadowblade) || player.IsCharcterClass(eCharacterClass.Shaman)))
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Mid Malice Slash 1h Version],[Mid Malice Crush 1h Version]"
                                + " ,[Mid Malice LA Version]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Thane) || player.IsCharcterClass(eCharacterClass.Skald)
                                || player.IsCharcterClass(eCharacterClass.Savage) || player.IsCharcterClass(eCharacterClass.Warrior)))
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Mid Malice Slash 1h Version],[Mid Malice Crush 1h Version]"
                                    + " ,[Mid Malice Slash 2h Version] ,[Mid Malice Crush 2h Version]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//SCOU, RANG, HUNT
                    case "Mariasha's Wall": //Mariasha's Sharkskin Gloves
                        {//Alb
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Scout))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("mariashas_sharkskin_gloves_studded_alb_archer");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Hib
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Bard)
                               || player.IsCharcterClass(eCharacterClass.Blademaster)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("mariashas_sharkskin_gloves_reinforced");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Ranger)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("mariashas_sharkskin_gloves_reinforced_archer");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Mid
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Berserker)
                                || player.IsCharcterClass(eCharacterClass.Savage)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("mariashas_sharkskin_gloves_studded");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Hunter))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("mariashas_sharkskin_gloves_studded_archer");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//CAB, FRI, HER, NECR, SORC, THEU, WIZ, ANI, BAIN, ELD, ENCH, MENT, VW, BD, RM, SM, WARL
                    case "Nailah's Diary": //Nailah's Robes
                        {
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cabalist) || player.IsCharcterClass(eCharacterClass.Heretic)
                                || player.IsCharcterClass(eCharacterClass.Necromancer) || player.IsCharcterClass(eCharacterClass.Sorcerer)
                                || player.IsCharcterClass(eCharacterClass.Theurgist) || player.IsCharcterClass(eCharacterClass.Wizard)
                                || player.IsCharcterClass(eCharacterClass.Animist) || player.IsCharcterClass(eCharacterClass.Bainshee)
                                || player.IsCharcterClass(eCharacterClass.Eldritch) || player.IsCharcterClass(eCharacterClass.Enchanter)
                                || player.IsCharcterClass(eCharacterClass.Mentalist) || player.IsCharcterClass(eCharacterClass.Valewalker)
                                || player.IsCharcterClass(eCharacterClass.Bonedancer) || player.IsCharcterClass(eCharacterClass.Runemaster)
                                || player.IsCharcterClass(eCharacterClass.Spiritmaster) || player.IsCharcterClass(eCharacterClass.Warlock)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("nailahs_robes_cloth");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Frir Leather Version
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Friar))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("nailahs_robes_leather");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//All
                    case "Dysis Tablets": //Night's Shroud Bracelet
                        {
                            if (t.Level > 29)
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("nights_shroud_bracelet");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)30), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//All
                    case "Oglidarsh's Scrolls": //Oglidarsh's Belt
                        {
                            if (t.Level > 29)
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("belt_of_oglidarsh");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)30), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//All
                    case "Great Hunt": //Orion's Belt
                        {//Archer Version
                            if (t.Level > 29 && (player.IsCharcterClass(eCharacterClass.Scout) || player.IsCharcterClass(eCharacterClass.Ranger)
                                    || player.IsCharcterClass(eCharacterClass.Hunter))) //Arms of the Winds sleeves
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("orions_belt_archer");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Melee Version
                            if (t.Level > 29 && (player.IsCharcterClass(eCharacterClass.Armsman)
                                || player.IsCharcterClass(eCharacterClass.Infiltrator) || player.IsCharcterClass(eCharacterClass.Mercenary)
                                || player.IsCharcterClass(eCharacterClass.Paladin) || player.IsCharcterClass(eCharacterClass.Reaver)
                                || player.IsCharcterClass(eCharacterClass.Blademaster) || player.IsCharcterClass(eCharacterClass.Champion)
                                || player.IsCharcterClass(eCharacterClass.Hero) || player.IsCharcterClass(eCharacterClass.Nightshade)
                                || player.IsCharcterClass(eCharacterClass.Valewalker) || player.IsCharcterClass(eCharacterClass.Vampiir)
                                || player.IsCharcterClass(eCharacterClass.Berserker)
                                || player.IsCharcterClass(eCharacterClass.Savage) || player.IsCharcterClass(eCharacterClass.Shadowblade)
                                || player.IsCharcterClass(eCharacterClass.Thane)
                                || player.IsCharcterClass(eCharacterClass.Warrior)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("orions_belt_melee");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }// Caster Version
                            if (t.Level > 29 && (player.IsCharcterClass(eCharacterClass.Cabalist) || player.IsCharcterClass(eCharacterClass.Cleric)
                                 || player.IsCharcterClass(eCharacterClass.Necromancer) || player.IsCharcterClass(eCharacterClass.Sorcerer)
                                || player.IsCharcterClass(eCharacterClass.Theurgist) || player.IsCharcterClass(eCharacterClass.Wizard)
                                || player.IsCharcterClass(eCharacterClass.Animist) || player.IsCharcterClass(eCharacterClass.Bainshee)
                                || player.IsCharcterClass(eCharacterClass.Druid) || player.IsCharcterClass(eCharacterClass.Eldritch)
                                || player.IsCharcterClass(eCharacterClass.Enchanter) || player.IsCharcterClass(eCharacterClass.Mentalist)
                                || player.IsCharcterClass(eCharacterClass.Bonedancer) || player.IsCharcterClass(eCharacterClass.Healer)
                                || player.IsCharcterClass(eCharacterClass.Runemaster) || player.IsCharcterClass(eCharacterClass.Shaman)
                                || player.IsCharcterClass(eCharacterClass.Spiritmaster)) || player.IsCharcterClass(eCharacterClass.Warlock))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("orions_belt_caster");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 29 && (player.IsCharcterClass(eCharacterClass.Minstrel) || player.IsCharcterClass(eCharacterClass.Friar)
                                || player.IsCharcterClass(eCharacterClass.Bard) || player.IsCharcterClass(eCharacterClass.Heretic)
                                || player.IsCharcterClass(eCharacterClass.MaulerHib) || player.IsCharcterClass(eCharacterClass.MaulerAlb)
                                || player.IsCharcterClass(eCharacterClass.MaulerMid) || player.IsCharcterClass(eCharacterClass.Valkyrie)
                                || player.IsCharcterClass(eCharacterClass.Warden) || player.IsCharcterClass(eCharacterClass.Skald)))
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Orion Belt Caster Version],[Orion Belt Melee Version]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }//Hib //BARD, BM, CHA, HERO, VW, VAMP, WARD
                            else
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)30), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//MINS, BARD, SKLD //All
                    case "Phoebus Letters": //Phoebus' Harp necklace
                        {
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Minstrel) || player.IsCharcterClass(eCharacterClass.Bard)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("phoebus_harp_necklace_bard_minstrel");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Minstrel) == false && player.IsCharcterClass(eCharacterClass.Bard) == false))
                            {

                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("phoebus_harp_necklace");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }

                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//all
                    case "Public Notice": //Ring of Dances
                        {
                            if (t.Level > 44)
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Ring of Dances Stealther Version],[Ring of Dances Caster Version]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//CAB, CLER, FRI, HER, NECR, SORC, THEU, WIZ, ANI, BAIN, BARD, DRU, ELD, ENCH, MENT, WARD, BD, HEAL, RM, SHA, SM, WARL
                    case "Ring of Fire": //Ring of Fire
                        {
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cabalist) || player.IsCharcterClass(eCharacterClass.Cleric)
                               || player.IsCharcterClass(eCharacterClass.Friar) || player.IsCharcterClass(eCharacterClass.Heretic)
                                || player.IsCharcterClass(eCharacterClass.Necromancer) || player.IsCharcterClass(eCharacterClass.Sorcerer)
                                || player.IsCharcterClass(eCharacterClass.Theurgist) || player.IsCharcterClass(eCharacterClass.Wizard)
                                || player.IsCharcterClass(eCharacterClass.Animist) || player.IsCharcterClass(eCharacterClass.Bainshee)
                                || player.IsCharcterClass(eCharacterClass.Bard) || player.IsCharcterClass(eCharacterClass.Druid)
                                || player.IsCharcterClass(eCharacterClass.Eldritch) || player.IsCharcterClass(eCharacterClass.Enchanter)
                                || player.IsCharcterClass(eCharacterClass.Mentalist) || player.IsCharcterClass(eCharacterClass.Warden)
                                || player.IsCharcterClass(eCharacterClass.Bonedancer) || player.IsCharcterClass(eCharacterClass.Healer)
                                || player.IsCharcterClass(eCharacterClass.Runemaster) || player.IsCharcterClass(eCharacterClass.Shaman)
                                || player.IsCharcterClass(eCharacterClass.Spiritmaster) || player.IsCharcterClass(eCharacterClass.Warlock)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("ring_of_fire");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//All
                    case "Tribute to Aduron": //Ring of Unyielding Will
                        {
                            if (t.Level > 29)
                            {

                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("ring_of_unyielding_will");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)30), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }
                    case "Adne's Letters": //Scepter of the Meritorious
                        {//CLER, HER BARD, DRU, WARD, HEAL, SHA
                            if (t.Level > 39 && (player.IsCharcterClass(eCharacterClass.Cleric) || player.IsCharcterClass(eCharacterClass.Friar) || player.IsCharcterClass(eCharacterClass.Heretic)
                                || player.IsCharcterClass(eCharacterClass.Armsman) || player.IsCharcterClass(eCharacterClass.Reaver)
                                || player.IsCharcterClass(eCharacterClass.Mercenary)))
                            { //Alb
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("scepter_of_the_meritorious_alb_crush_1h");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 39 && (player.IsCharcterClass(eCharacterClass.Bard) || player.IsCharcterClass(eCharacterClass.Druid)
                                 || player.IsCharcterClass(eCharacterClass.Warden) || player.IsCharcterClass(eCharacterClass.Hero)
                                 || player.IsCharcterClass(eCharacterClass.Champion) || player.IsCharcterClass(eCharacterClass.Blademaster)))
                            { //Hib
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("scepter_of_the_meritorious_hib");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 39 && (player.IsCharcterClass(eCharacterClass.Healer) || player.IsCharcterClass(eCharacterClass.Shaman)
                                || player.IsCharcterClass(eCharacterClass.Warrior) || player.IsCharcterClass(eCharacterClass.Thane)
                                || player.IsCharcterClass(eCharacterClass.Berserker) || player.IsCharcterClass(eCharacterClass.Savage)))
                            { //Mid
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("scepter_of_the_meritorious_mid_crush_1h");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)40), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//All
                    case "Wooden Triptych": //Scorpion's Tail Ring
                        {
                            if (t.Level > 44)
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("the_scorpions_tail");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//ARMS, INF, MERC, MINS, PAL, REAV, SCOU, BM, CHA, HERO, NS, RANG, VW, VAMP,BER, HUNT, SAV, SB, SKLD, THAN, WAR, VALK
                    case "Regarding Shades": //Shades of Mist
                        {
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Armsman) || player.IsCharcterClass(eCharacterClass.Mercenary)
                                || player.IsCharcterClass(eCharacterClass.Paladin) || player.IsCharcterClass(eCharacterClass.Reaver)
                                || player.IsCharcterClass(eCharacterClass.Blademaster) || player.IsCharcterClass(eCharacterClass.Champion)
                                || player.IsCharcterClass(eCharacterClass.Hero) || player.IsCharcterClass(eCharacterClass.Valewalker)
                                || player.IsCharcterClass(eCharacterClass.Vampiir) || player.IsCharcterClass(eCharacterClass.Berserker)
                                || player.IsCharcterClass(eCharacterClass.Savage) || player.IsCharcterClass(eCharacterClass.Skald)
                                || player.IsCharcterClass(eCharacterClass.Thane) || player.IsCharcterClass(eCharacterClass.Warrior)
                                || player.IsCharcterClass(eCharacterClass.Valkyrie) || player.IsCharcterClass(eCharacterClass.MaulerHib)
                                || player.IsCharcterClass(eCharacterClass.MaulerAlb) || player.IsCharcterClass(eCharacterClass.MaulerMid)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("shades_of_mist_melee");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Infiltrator) || player.IsCharcterClass(eCharacterClass.Minstrel)
                                || player.IsCharcterClass(eCharacterClass.Scout) || player.IsCharcterClass(eCharacterClass.Nightshade)
                                || player.IsCharcterClass(eCharacterClass.Ranger) || player.IsCharcterClass(eCharacterClass.Hunter)
                                || player.IsCharcterClass(eCharacterClass.Shadowblade)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("shades_of_mist_stealther");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//ARMS, PAL, REAV, HERO, WAR, VALK, Warden
                    case "Shield of Khaos": //Shield of Khaos
                        {
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Armsman) || player.IsCharcterClass(eCharacterClass.Paladin)
                                || player.IsCharcterClass(eCharacterClass.Reaver) || player.IsCharcterClass(eCharacterClass.Hero)
                                || player.IsCharcterClass(eCharacterClass.Warrior) || player.IsCharcterClass(eCharacterClass.Valkyrie) || player.IsCharcterClass(eCharacterClass.Warden)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("shield_of_Khaos");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//HER, REAV, VW, SAV, MAUL
                    case "Julea's Story": //Snakecharmer's Weapon
                        {//Alb
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Heretic) || player.IsCharcterClass(eCharacterClass.Reaver)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("snakecharmers_whip");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//All Realms
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.MaulerAlb) || player.IsCharcterClass(eCharacterClass.MaulerMid)
                                || player.IsCharcterClass(eCharacterClass.MaulerHib)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("snakecharmers_wrap");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Mid 
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Savage))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("snakecharmers_hth");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Mid 
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Valewalker))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("snakecharmers_scythe");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }
                    case "Snatcher's Tale": //Snatcher's Tale bracer
                        {
                            if (t.Level > 44)
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("snatcher");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//ARMS, MERC, PAL, REAV, BARD, BM, CHA, HERO, VW, VAMP, WARD, BER, HUNT, SAV, SKLD, THAN, VALK, WAR
                    case "Spear of Kings": //Spear of Kings
                        {//Alb
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Mercenary) || player.IsCharcterClass(eCharacterClass.Reaver)
                                || player.IsCharcterClass(eCharacterClass.Minstrel) || player.IsCharcterClass(eCharacterClass.Infiltrator)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("spear_of_kings_1h_slash_alb");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Armsman) || player.IsCharcterClass(eCharacterClass.Paladin)))
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Alb Spear of Kings Slash 1h Version],[Alb Spear of Kings Slash 2h Version]"
                                    + " ,[Alb Spear of Kings Slash Pole Version] ,[Alb Spear of Kings Thrust Pole Version]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }//Hib //BARD, BM, CHA, HERO, VW, VAMP, WARD
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Bard) || player.IsCharcterClass(eCharacterClass.Blademaster)
                                   || player.IsCharcterClass(eCharacterClass.Vampiir) || player.IsCharcterClass(eCharacterClass.Warden)))
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Hib Spear of Kings Slash 1h Version],[Hib Spear of Kings Thrust Piercing Version] ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Valewalker))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("Scythe_of_Kingse");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Champion) || player.IsCharcterClass(eCharacterClass.Hero)))
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Hib Spear of Kings Slash 1h Version],[Hib Spear of Kings Slash 2h Version]"
                                    + " ,[Hib Spear of Kings Slash Spear Version] ,[Hib Spear of Kings Thrust Piercing Version]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }//Mid //BER, HUNT, SAV, SKLD, THAN, VALK, WAR
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Berserker) || player.IsCharcterClass(eCharacterClass.Hunter)
                                || player.IsCharcterClass(eCharacterClass.Savage) || player.IsCharcterClass(eCharacterClass.Skald)))
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Mid Spear of Kings Slash 1h Version],[Mid Spear of Kings Spear Version] ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Thane) || player.IsCharcterClass(eCharacterClass.Warrior)
                                || player.IsCharcterClass(eCharacterClass.Valkyrie)))
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Mid Spear of Kings Slash 1h Version],[Mid Spear of Kings 2h Version]"
                                    + " ,[Mid Spear of Kings Spear Version] ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }

                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//CAB, CLER, FRI, NECR, SORC, THEU, WIZ, ANI, BAIN, DRU, ELD, ENCH, MENT, BD, HEAL, RM, SHA, SM, WARL, MAUL
                    case "Trident of the Gods": //Staff of the God
                        {
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cabalist) || player.IsCharcterClass(eCharacterClass.Cleric)
                                || player.IsCharcterClass(eCharacterClass.Friar) || player.IsCharcterClass(eCharacterClass.Necromancer)
                                || player.IsCharcterClass(eCharacterClass.Sorcerer) || player.IsCharcterClass(eCharacterClass.Theurgist)
                                || player.IsCharcterClass(eCharacterClass.Wizard) || player.IsCharcterClass(eCharacterClass.Animist)
                                || player.IsCharcterClass(eCharacterClass.Bainshee) || player.IsCharcterClass(eCharacterClass.Druid)
                                || player.IsCharcterClass(eCharacterClass.Eldritch) || player.IsCharcterClass(eCharacterClass.Enchanter)
                                || player.IsCharcterClass(eCharacterClass.Mentalist) || player.IsCharcterClass(eCharacterClass.Bonedancer)
                                || player.IsCharcterClass(eCharacterClass.Healer) || player.IsCharcterClass(eCharacterClass.Runemaster)
                                || player.IsCharcterClass(eCharacterClass.Shaman) || player.IsCharcterClass(eCharacterClass.Spiritmaster)
                                || player.IsCharcterClass(eCharacterClass.Warlock) || player.IsCharcterClass(eCharacterClass.MaulerAlb)
                                || player.IsCharcterClass(eCharacterClass.MaulerMid) || player.IsCharcterClass(eCharacterClass.MaulerHib)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("staff_of_god");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//CAB, NECR, SORC, THEU, WIZ, ANI, BAIN, ELD, ENCH, MENT, BD, RM, SM, WARL
                    case "Helenia's Letters": //Stone of Atlantis jewel slot
                        {
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cabalist) || player.IsCharcterClass(eCharacterClass.Necromancer)
                                || player.IsCharcterClass(eCharacterClass.Sorcerer) || player.IsCharcterClass(eCharacterClass.Theurgist)
                                || player.IsCharcterClass(eCharacterClass.Wizard) || player.IsCharcterClass(eCharacterClass.Animist)
                                || player.IsCharcterClass(eCharacterClass.Bainshee) || player.IsCharcterClass(eCharacterClass.Eldritch)
                                || player.IsCharcterClass(eCharacterClass.Mentalist) || player.IsCharcterClass(eCharacterClass.Bonedancer)
                                || player.IsCharcterClass(eCharacterClass.Runemaster) || player.IsCharcterClass(eCharacterClass.Spiritmaster)
                                || player.IsCharcterClass(eCharacterClass.Warlock)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("stone_of_atlantis");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//CAB, FRI, NECR, SORC, THEU, WIZ, ANI, BAIN, ELD, ENCH, MENT, BD, RM, SM, WARL, MAUL
                    case "Tartaros Gift": //Tartaros Gift staff
                        {
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cabalist)
                                || player.IsCharcterClass(eCharacterClass.Necromancer) || player.IsCharcterClass(eCharacterClass.Sorcerer)
                                || player.IsCharcterClass(eCharacterClass.Theurgist) || player.IsCharcterClass(eCharacterClass.Wizard)
                                || player.IsCharcterClass(eCharacterClass.Animist) || player.IsCharcterClass(eCharacterClass.Bainshee)
                                || player.IsCharcterClass(eCharacterClass.Eldritch) || player.IsCharcterClass(eCharacterClass.Enchanter)
                                || player.IsCharcterClass(eCharacterClass.Mentalist)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("tartaros_gift_caster");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Bonedancer)
                                || player.IsCharcterClass(eCharacterClass.Runemaster) || player.IsCharcterClass(eCharacterClass.Spiritmaster)
                                || player.IsCharcterClass(eCharacterClass.Warlock)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("tartaros_gift_caster_mid");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Friar))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("tartaros_gift_friar");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.MaulerAlb) || player.IsCharcterClass(eCharacterClass.MaulerHib)
                                || player.IsCharcterClass(eCharacterClass.MaulerMid)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("tartaros_gift_mauler");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;

                        }//ARMS, INF, MERC, MINS, PAL, REAV, SCOU, BARD, BM, CHA, DRU, HERO, NS, RANG, VAMp, WARD, HUNT, SAV, SB, SKLD, THAN, VALK, WAR
                    case "Wall Glyphs": //Traitor's Dagger
                        {//Alb ARMS, INF, MERC, MINS, PAL, REAV, SCOU
                            if (t.Level > 39 && (player.IsCharcterClass(eCharacterClass.Armsman) || player.IsCharcterClass(eCharacterClass.Infiltrator)
                                || player.IsCharcterClass(eCharacterClass.Mercenary) || player.IsCharcterClass(eCharacterClass.Minstrel)
                                || player.IsCharcterClass(eCharacterClass.Paladin) || player.IsCharcterClass(eCharacterClass.Reaver)
                                || player.IsCharcterClass(eCharacterClass.Scout)))
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Alb Traitors Dagger Slash Version],[Alb Traitors Dagger Thrust Version]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }//Hib  BARD, BM, CHA, DRU, HERO, NS, RANG, VAMP, Ward
                            if (t.Level > 39 && (player.IsCharcterClass(eCharacterClass.Bard) || player.IsCharcterClass(eCharacterClass.Blademaster)
                                || player.IsCharcterClass(eCharacterClass.Champion) || player.IsCharcterClass(eCharacterClass.Druid)
                                || player.IsCharcterClass(eCharacterClass.Hero) || player.IsCharcterClass(eCharacterClass.Nightshade)
                                || player.IsCharcterClass(eCharacterClass.Ranger) || player.IsCharcterClass(eCharacterClass.Vampiir)
                                || player.IsCharcterClass(eCharacterClass.Warden)))
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Hib Traitors Dagger Slash Version],[Hib Traitors Dagger Thrust Version]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }//Mid  HUNT, SAV, SB, SKLD, THAN, VALK, WAR
                            if (t.Level > 39 && (player.IsCharcterClass(eCharacterClass.Hunter) || player.IsCharcterClass(eCharacterClass.Savage)
                                || player.IsCharcterClass(eCharacterClass.Skald) || player.IsCharcterClass(eCharacterClass.Thane)
                                || player.IsCharcterClass(eCharacterClass.Valkyrie) || player.IsCharcterClass(eCharacterClass.Warrior)
                                || player.IsCharcterClass(eCharacterClass.Berserker) || player.IsCharcterClass(eCharacterClass.Shadowblade)))
                            {
                                t.TempProperties.setProperty(BookToRemove, item);
                                t.Out.SendMessage("So " + t.Name + " you want the [Mid Traitors Dagger Sword Version],[Mid Traitors Dagger Axe Version]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)40), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//CAB, FRI, NECR, SORC, THEU, WIZ, ANI, BAIN, ELD, ENCH, MENT, BD, RM, SM, WARL, MAUL
                    case "Traldor's Oracle": //Traldor's Oracle
                        {
                            if (t.Level > 39 && (player.IsCharcterClass(eCharacterClass.Cabalist) || player.IsCharcterClass(eCharacterClass.Necromancer)
                               || player.IsCharcterClass(eCharacterClass.Sorcerer) || player.IsCharcterClass(eCharacterClass.Theurgist)
                                || player.IsCharcterClass(eCharacterClass.Wizard) || player.IsCharcterClass(eCharacterClass.Animist)
                                || player.IsCharcterClass(eCharacterClass.Bainshee) || player.IsCharcterClass(eCharacterClass.Eldritch)
                                || player.IsCharcterClass(eCharacterClass.Enchanter) || player.IsCharcterClass(eCharacterClass.Mentalist)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("traldors_oracle_caster");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 39 && (player.IsCharcterClass(eCharacterClass.MaulerAlb) || player.IsCharcterClass(eCharacterClass.MaulerHib)
                                || player.IsCharcterClass(eCharacterClass.MaulerMid)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("Traldors_Oracle_melee");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 39 && player.IsCharcterClass(eCharacterClass.Friar))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("traldors_oracle_friar");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 39 && (player.IsCharcterClass(eCharacterClass.Bonedancer)
                                || player.IsCharcterClass(eCharacterClass.Runemaster)
                                || player.IsCharcterClass(eCharacterClass.Spiritmaster)
                                || player.IsCharcterClass(eCharacterClass.Warlock)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("traldors_oracle_Mid");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)40), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//ARMS, MERC, MINS, PAL, REAV, SCOU, BARD, BM, CHA, DRU, HERO, RANG, WARD, BER, HEAL, HUNT, SAV, SHA, SKLD, THAN, VALK, WAR
                    case "Inscribed Stone": //Winged Helm
                        {//Alb
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Armsman) || player.IsCharcterClass(eCharacterClass.Paladin)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("winged_helm_plate_neu");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cleric) || player.IsCharcterClass(eCharacterClass.Mercenary)
                                 || player.IsCharcterClass(eCharacterClass.Minstrel) || player.IsCharcterClass(eCharacterClass.Reaver)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("winged_helm_chain_neu");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Scout))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("winged_helm_studded_neu");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Hib BARD, BM, CHA, DRU, HERO, RANG, WARD
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Champion) || player.IsCharcterClass(eCharacterClass.Druid)
                               || player.IsCharcterClass(eCharacterClass.Hero) || player.IsCharcterClass(eCharacterClass.Warden)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("winged_helm_scale");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Bard) || player.IsCharcterClass(eCharacterClass.Ranger)
                                || player.IsCharcterClass(eCharacterClass.Blademaster)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("winged_helm_reinforced");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Mid BER, HEAL, HUNT, SAV, SHA, SKLD, THAN, VALK, WAR
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Healer) || player.IsCharcterClass(eCharacterClass.Skald)
                               || player.IsCharcterClass(eCharacterClass.Thane) || player.IsCharcterClass(eCharacterClass.Valkyrie)
                                || player.IsCharcterClass(eCharacterClass.Warrior) || player.IsCharcterClass(eCharacterClass.Shaman)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("winged_helm_chain_mid");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Berserker) || player.IsCharcterClass(eCharacterClass.Hunter)
                                || player.IsCharcterClass(eCharacterClass.Savage)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("winged_helm_studded_mid");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;
                        }//ARMS, CLER, MERC, MINS, PAL, REAV, SCOU, BARD, DRU, BM, CHA, HERO, RANG, WARD, BER, HEAL, HUNT, SAV, SHA, SKLD, THAN, VALK, WAR
                    case "Wings Dive": //Wings Dive pants
                        {//Alb
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Armsman) || player.IsCharcterClass(eCharacterClass.Paladin)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("wings_dive_plate");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Cleric) || player.IsCharcterClass(eCharacterClass.Mercenary)
                               || player.IsCharcterClass(eCharacterClass.Minstrel) || player.IsCharcterClass(eCharacterClass.Reaver)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("wings_dive_chain_alb");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && player.IsCharcterClass(eCharacterClass.Scout))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("wings_dive");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Hib BARD, DRU, BM, CHA, HERO, RANG, WARD
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Druid) || player.IsCharcterClass(eCharacterClass.Champion)
                                || player.IsCharcterClass(eCharacterClass.Hero) || player.IsCharcterClass(eCharacterClass.Warden)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("wings_dive_scale");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Bard) || player.IsCharcterClass(eCharacterClass.Blademaster)
                                || player.IsCharcterClass(eCharacterClass.Ranger)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("wings_dive_reinforced");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }//Mid BER, HEAL, HUNT, SAV, SHA, SKLD, THAN, VALK, WAR
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Healer) || player.IsCharcterClass(eCharacterClass.Shaman)
                               || player.IsCharcterClass(eCharacterClass.Skald) || player.IsCharcterClass(eCharacterClass.Thane)
                               || player.IsCharcterClass(eCharacterClass.Valkyrie) || player.IsCharcterClass(eCharacterClass.Warrior)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("wings_dive_chain_alb");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            if (t.Level > 44 && (player.IsCharcterClass(eCharacterClass.Berserker) || player.IsCharcterClass(eCharacterClass.Hunter)
                                    || player.IsCharcterClass(eCharacterClass.Savage)))
                            {
                                ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("wings_dive");
                                InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                                t.Inventory.RemoveItem(item); t.UpdatePlayerStatus();
                                GamePlayerUtils.SendClearPopupWindow(t);
                                ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                break;
                            }
                            else
                                GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "IsNotPermitted", t.GetName(0, false), (int)45), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;

                        }

                }
                return true;
            }


            {
                
                t.TempProperties.setProperty(REFUND_ITEM_WEAK, new WeakRef(item));
            }

            return base.ReceiveItem(source, item);
        }

        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;


            ((GamePlayer)player).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client.Account.Language, "IHaveAllArtifactsOffer", player.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
            return true;
        }

        public virtual bool removeItemFromInventory(GamePlayer target)
        {
           // log.ErrorFormat("versuche buch zu entfernen");
            InventoryItem item = null;
            
            item = target.TempProperties.getProperty<InventoryItem>(BookToRemove);

            if (item == null)
            {
                return false;
            }

            lock (target.Inventory)
            {
                var items = target.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
                foreach (InventoryItem invItem in items)
                {
                    if (invItem.Name == item.Name)
                    {
                        
                        //log.ErrorFormat("entferne buch:  {0}", item.Name);
                        target.Inventory.RemoveItem(item); target.UpdatePlayerStatus();
                        target.UpdatePlayerStatus();
                        target.TempProperties.removeProperty(BookToRemove);
                        return true;
                    }
                }
            }
            return false;
        }


        public override bool WhisperReceive(GameLiving source, string str)
        {
            if (!base.WhisperReceive(source, str)) return false;
            if (!(source is GamePlayer)) return false;
            GamePlayer t = (GamePlayer)source;

            switch (str)
            {
                //Version select
                case "Hib Battler 1h Con Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("battler_hib_blade_1h_con"); //Battler con version Hib
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }

                case "Hib Battler 1h Str Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("battler_hib_blade_1h"); //Battler con version Hib
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Hib Battler 1h blunt Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("battler_hib_blunt_1h"); //Battler con version Hib
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Hib Battler 2h Crush":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("battler_hib_crush_2h"); //Battler con version Hib
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Hib Battler 2h Slash":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("battler_hib_slash_2h"); //Battler con version Hib
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Alb Battler 1h Slash Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("battler_alb_slash_1h"); //Battler con version Hib
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Alb Battler 1h Crush Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("battler_alb_crush_1h"); //Battler con version Hib
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Alb Battler 2h Crush":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("battler_alb_crush_2h"); //Battler con version Hib
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Alb Battler 2h Slash":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("battler_alb_slash_2h"); //Battler con version Hib
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Mid Battler 1h Slash Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("battler_mid_slash_1h"); //Battler con version Hib
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Mid Battler 1h Crush Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("battler_mid_crush_1h"); //Battler con version Hib
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Mid Battler 2h Crush":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("battler_mid_crush_2h"); //Battler con version Hib
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Mid Battler 2h Slash":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("battler_mid_slash_2h"); //Battler con version Hib
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Alb Bruiser 2h Crush Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("bruiser_alb_2h"); //Battler con version Hib
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Alb Bruiser 1h Crush Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("bruiser_alb_1h"); //Battler con version Hib
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Hib Bruiser 2h Slash Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("bruiser_hib_2h"); //Battler con version Hib
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Hib Bruiser 1h Blunt Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("bruiser_hib_1h"); //Battler con version Hib
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Mid Bruiser 1h Crush Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("bruiser_mid_1h"); //Battler con version Hib
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Mid Bruiser 2h Crush Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("bruiser_mid_2h"); //Battler con version Hib
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Ceremonial Bracer Acu Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("ceremonial_bracers_acuity");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                           
                        }
                        break;
                    }
                case "Ceremonial Bracer Dex Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("ceremonial_bracers_dexterity");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Ceremonial Bracer Qui Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("ceremonial_bracers_quickness");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Ceremonial Bracer Con Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("ceremonial_bracers_constitution");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Ceremonial Bracer Str Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("ceremonial_bracers_strength");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Mid Crocodile Tooth Dagger Axe Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crocodiles_tooth_dagger_mid_axe");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Mid Crocodile Tooth Dagger Hammer Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crocodiles_tooth_dagger_mid_hammer");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Mid Crocodile Tooth Dagger Sword Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crocodiles_tooth_dagger_mid_sword");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Hib Crocodile Tooth Dagger Pierce Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crocodiles_tooth_dagger_hib_pierce");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Hib Crocodile Tooth Dagger Blade Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crocodiles_tooth_dagger_hib_blade");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Hib Crocodile Tooth Dagger blunt Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crocodiles_tooth_dagger_hib_blunt");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Alb Crocodile Tooth Dagger Crush Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crocodiles_tooth_dagger_alb_crush");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Alb Crocodile Tooth Dagger Slash Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crocodiles_tooth_dagger_alb_slash");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Alb Crocodile Tooth Dagger Trust Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("crocodiles_tooth_dagger_alb_thrust");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Golden Spear 1h Sword Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("the_golden_spear_mid_1h");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Golden Spear 2h Spear Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("the_golden_spear_mid_2h");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Alb Malice Slash 1h Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("malice_alb_slash_1h");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Alb Malice Crush 1h Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("malice_alb_crush_1h");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Alb Malice Slash 2h Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("malice_alb_slash_2h");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Alb Malice Crush 2h Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("malice_alb_crush_2h");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Hib Malice Slash 1h Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("malice_axe_blade_hib");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Hib Malice Crush 1h Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("malice_mace_hib");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Hib Malice Slash 2h Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("malice_magma_axe_hib");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Hib Malice Crush 2h Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("malice_magma_mace_hib");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Mid Malice Slash 1h Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("malice_mid_axe_1h");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Mid Malice Crush 1h Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("malice_mid_hammer_1h");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Mid Malice Slash 2h Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("malice_mid_axe_2h");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Mid Malice Crush 2h Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("malice_mid_hammer_2h");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Mid Malice LA Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("malice_la_axe_1h");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Ring of Dances Stealther Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("ring_of_dances_stealther");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Ring of Dances Caster Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("ring_of_dances_caster");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Alb Spear of Kings Slash 1h Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("spear_of_kings_1h_slash_alb");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Alb Spear of Kings Slash 2h Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("spear_of_kings_2h_slash_alb");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Alb Spear of Kings Slash Pole Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("spear_of_kings_2h_slashpolearm_alb");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Alb Spear of Kings Thrust Pole Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("spear_of_kings_2h_thrustpolearm_alb");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Hib Spear of Kings Slash 1h Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("spear_of_kings_1h_blade_hib");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Hib Spear of Kings Slash 2h Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("spear_of_kings_2h_slash_hib");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Hib Spear of Kings Slash Spear Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("spear_of_kings_slash_spear");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Hib Spear of Kings Thrust Piercing Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("spear_of_kings_1h_hib");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Mid Spear of Kings Slash 1h Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("spear_of_kings_1h_thrust_mid");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Mid Spear of Kings 2h Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("spear_of_kings_2h_thrust_mid");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Mid Spear of Kings Spear Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("spear_of_kings_2h_thrustspear_mid");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Alb Traitors Dagger Slash Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("traitors_dagger_alb");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Alb Traitors Dagger Thrust Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("traitors_dagger_thrust_alb");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Hib Traitors Dagger Slash Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("traitors_dagger_hib");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Hib Traitors Dagger Thrust Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("traitors_dagger_pierce_hib");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Mid Traitors Dagger Sword Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("traitors_dagger_mid");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Mid Traitors Dagger Axe Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("traitors_axe_mid");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Gem of Lost Memories Caster Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("gem_of_lost_memories_caster");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Gem of Lost Memories Melee Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("gem_of_lost_memories_melee");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Guard of Valor Heretic Melee Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("guard_of_valor_cloth_melee_heretic");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Guard of Valor Heretic Caster Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("guard_of_valor_cloth_Caster_heretic");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
                case "Orion Belt Caster Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("orions_belt_caster");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }

                case "Orion Belt Melee Version":
                    {
                        if (removeItemFromInventory(t))
                        {
                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("orions_belt_melee");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            GamePlayerUtils.SendClearPopupWindow(t);
                            ((GamePlayer)t).Out.SendMessage(LanguageMgr.GetTranslation((t as GamePlayer).Client.Account.Language, "CongratulationsForThisArtifact", t.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                            break;
                    }
            }

            {
                t.TempProperties.setProperty(REFUND_ITEM_WEAK, new WeakRef(items));
            }

            return base.ReceiveItem(source, items);
        }

    }

}

