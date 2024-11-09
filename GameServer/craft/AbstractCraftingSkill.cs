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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.ServerProperties;
using DOL.GS.Spells;
using DOL.Language;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DOL.GS
{
    /// <summary>
    /// AbstractCraftingSkill is the base class for all crafting skill
    /// </summary>
    public abstract class AbstractCraftingSkill
    {
        protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Declaration
        /// <summary>
        /// the maximum possible range within a player has to be to a forge , lathe ect to craft an item
        /// </summary>
        public const int CRAFT_DISTANCE = 512;

        /// <summary>
        /// The icone number used by this craft.
        /// </summary>
        private byte m_icon;

        /// <summary>
        /// The name show for this craft.
        /// </summary>
        private string m_name;

        /// <summary>
        /// The crafting skill id of this craft
        /// </summary>
        private eCraftingSkill m_eskill;

        /// <summary>
        /// The player currently crafting
        /// </summary>
        public const string PLAYER_CRAFTER = "PLAYER_CRAFTER";

        /// <summary>
        /// The recipe being crafted
        /// </summary>
        public const string RECIPE_BEING_CRAFTED = "RECIPE_BEING_CRAFTED";

        /// <summary>
        /// The list of raw materials for the recipe beign crafted
        /// </summary>
        public const string RECIPE_RAW_MATERIAL_LIST = "RECIPE_RAW_MATERIAL_LIST";

        protected const int subSkillCap = 1300;

        public virtual string CRAFTER_TITLE_PREFIX
        {
            get
            {
                return "";
            }
        }
        public eCraftingSkill eSkill
        {
            get
            {
                return m_eskill;
            }
            set
            {
                m_eskill = value;
            }
        }
        public byte Icon
        {
            get
            {
                return m_icon;
            }
            set
            {
                m_icon = value;
            }
        }
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        public long m_matPrice;
        public long MatPrice
        {
            get
            {
                return m_matPrice;
            }
            set
            {

                m_matPrice = value;
            }
        }

       protected  IList<string> addMaterials = null;

        #endregion

        #region First call function and callback

        /// <summary>
        /// Called when player tries to begin crafting an item
        /// </summary>
        public virtual void CraftItem(GamePlayer player, DBCraftedItem recipe, ItemTemplate itemToCraft, IList<DBCraftedXItem> rawMaterials)
        {
            if (!CanPlayerStartToCraftItem(player, recipe, itemToCraft, rawMaterials))
            {
                return;
            }

            if (Properties.Player_MaxLevel < 49)
            {
                if (Properties.Player_MaxLevel < itemToCraft.Level - 1)
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "You can maximal craft items with level: " + (Properties.Player_MaxLevel + 1) + " "), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }
            }


            if (player.IsCrafting)
            {
                StopCraftingCurrentItem(player, itemToCraft);
                return;
            }

            int craftingTime = GetCraftingTime(player, recipe, rawMaterials);

            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.CraftItem.BeginWork", itemToCraft.Name, CalculateChanceToMakeItem(player, recipe).ToString()), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
            player.Out.SendTimerWindow(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.CraftItem.CurrentlyMaking", itemToCraft.Name), craftingTime);

            player.Stealth(false);

            StartCraftingTimerAndSetCallBackMethod(player, recipe, rawMaterials, craftingTime);
        }


        protected virtual void StartCraftingTimerAndSetCallBackMethod(GamePlayer player, DBCraftedItem recipe, IList<DBCraftedXItem> rawMaterials, int craftingTime)
        {
            player.CraftTimer = new RegionTimer(player)
            {
                Callback = new RegionTimerCallback(MakeItem)
            };
            player.CraftTimer.Properties.setProperty(PLAYER_CRAFTER, player);
            player.CraftTimer.Properties.setProperty(RECIPE_BEING_CRAFTED, recipe);
            player.CraftTimer.Properties.setProperty(RECIPE_RAW_MATERIAL_LIST, rawMaterials);
            player.CraftTimer.Start(craftingTime * 1000);
        }

        protected virtual void StopCraftingCurrentItem(GamePlayer player, ItemTemplate itemToCraft)
        {
            player.CraftTimer.Stop();
            player.Out.SendCloseTimerWindow();
            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.CraftItem.StopWork", itemToCraft.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
        }

        protected virtual bool CanPlayerStartToCraftItem(GamePlayer player, DBCraftedItem recipe, ItemTemplate itemToCraft, IList<DBCraftedXItem> rawMaterials)
        {
            if (!GameServer.ServerRules.IsAllowedToCraft(player, itemToCraft))
            {
                return false;
            }

            if (!CheckForTools(player, recipe, itemToCraft, rawMaterials))
            {
                return false;
            }

            if (!CheckSecondCraftingSkillRequirement(player, recipe, itemToCraft, rawMaterials))
            {
                return false;
            }

            if (!CheckRawMaterials(player, recipe, itemToCraft, rawMaterials))
            {
                return false;
            }

            if (player.IsMoving || player.IsStrafing)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.CraftItem.MoveAndInterrupt"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (player.InCombat)
            {
                player.Out.SendMessage("You can't craft while in combat.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Make the item when craft time is finished
        /// </summary>
        protected virtual int MakeItem(RegionTimer timer)
        {
            GamePlayer player = timer.Properties.getProperty<GamePlayer>(PLAYER_CRAFTER, null);
            DBCraftedItem recipe = timer.Properties.getProperty<DBCraftedItem>(RECIPE_BEING_CRAFTED, null);
            IList<DBCraftedXItem> rawMaterials = timer.Properties.getProperty<IList<DBCraftedXItem>>(RECIPE_RAW_MATERIAL_LIST, null);

            if (player == null || recipe == null || rawMaterials == null)
            {
                if (player != null) player.Out.SendMessage("Could not find recipe or item to craft!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                log.Error("Crafting.MakeItem: Could not retrieve player, recipe, or raw materials to craft from CraftTimer.");
                return 0;
            }

            ItemTemplate itemToCraft = GameServer.Database.FindObjectByKey<ItemTemplate>(recipe.Id_nb);
            if (itemToCraft == null)
            {
                return 0;
            }

            player.CraftTimer.Stop();
            player.Out.SendCloseTimerWindow();

            if (Util.Chance(CalculateChanceToMakeItem(player, recipe)))
            {
                if (!RemoveUsedMaterials(player, recipe, rawMaterials))
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.MakeItem.NotAllMaterials"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                    if (player.Client.Account.PrivLevel == 1)
                        return 0;
                }

                BuildCraftedItem(player, recipe, itemToCraft);
                GainCraftingSkillPoints(player, recipe, rawMaterials);
            }
            else
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.MakeItem.LoseNoMaterials", itemToCraft.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                player.Out.SendPlaySound(eSoundType.Craft, 0x02);
            }
            return 0;
        }

        #endregion

        #region Requirement check

        /// <summary>
        /// Check if the player is near the needed tools (forge, lathe, etc)
        /// </summary>
        /// <param name="player">the crafting player</param>
        /// <param name="recipe">the recipe being used</param>
        /// <param name="itemToCraft">the item to make</param>
        /// <param name="rawMaterials">a list of raw materials needed to create this item</param>
        /// <returns>true if required tools are found</returns>
        protected virtual bool CheckForTools(GamePlayer player, DBCraftedItem recipe, ItemTemplate itemToCraft, IList<DBCraftedXItem> rawMaterials)
        {
            return true;
        }

        /// <summary>
        /// Check if the player has enough secondary crafting skill to build the item
        /// </summary>
        /// <param name="player"></param>
        /// <param name="recipe"></param>
        /// <returns></returns>
        public virtual bool CheckSecondCraftingSkillRequirement(GamePlayer player, DBCraftedItem recipe, ItemTemplate itemToCraft, IList<DBCraftedXItem> rawMaterials)
        {
            int minimumLevel = GetSecondaryCraftingSkillMinimumLevel(recipe, itemToCraft);

            if (minimumLevel <= 0)
                return true; // no requirement needed

            foreach (DBCraftedXItem material in rawMaterials)
            {
                ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>(material.IngredientId_nb);

                if (template == null)
                {
                    player.Out.SendMessage("Can't find a material (" + material.IngredientId_nb + ") needed for recipe.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    //log.Error("Cannot find raw material ItemTemplate: " + material.IngredientId_nb + ") needed for recipe: " + recipe.CraftedItemID);
                    return false;
                }

                switch (template.Model)
                {
                    case 522:   //"cloth square"
                    case 537:   //"heavy thread"
                        {
                            if (player.GetCraftingSkillValue(eCraftingSkill.ClothWorking) < minimumLevel)
                            {
                                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.CheckSecondCraftingSkillRequirement.NoClothworkingSkill", minimumLevel, template.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return false;
                            }
                            break;
                        }

                    case 521:   //"leather square"
                        {
                            if (player.GetCraftingSkillValue(eCraftingSkill.LeatherCrafting) < minimumLevel)
                            {
                                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.CheckSecondCraftingSkillRequirement.NoLeathercraftingSkill", minimumLevel, template.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return false;
                            }
                            break;
                        }

                    case 519:   //"metal bars"
                        {
                            if (player.GetCraftingSkillValue(eCraftingSkill.MetalWorking) < minimumLevel)
                            {
                                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.CheckSecondCraftingSkillRequirement.NoMetalworkingSkill", minimumLevel, template.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return false;
                            }
                            break;
                        }

                    case 520:   //"wooden boards"
                        {
                            if (player.GetCraftingSkillValue(eCraftingSkill.WoodWorking) < minimumLevel)
                            {
                                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.CheckSecondCraftingSkillRequirement.NoWoodworkingSkill", minimumLevel, template.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return false;
                            }
                            break;
                        }
                }
            }
            return true;
        }

       
       

        public virtual void SetCraftPrice(ItemTemplate itemToCraft, GamePlayer player)
        {
            if (player.Client.Account.PrivLevel == 1)
            {
                IList<ItemTemplate> items = GameServer.Database.SelectObjects<ItemTemplate>("`Id_nb` = @Id_nb", new QueryParameter("@Id_nb", itemToCraft.Id_nb));

                if (items != null)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        try
                        {
                            ItemTemplate item = items[i];

                            if (!item.CraftPriceSet)
                            {
                                item.AllowUpdate = true;

                                item.CraftPriceSet = true;
                                GameServer.Database.SaveObject(item);
                                GameServer.Database.UpdateInCache<ItemTemplate>(item.Id_nb);
                                log.WarnFormat("save! CraftItem: {0}, Id_nb: {1}, price {2} ", item.Name, item.Id_nb, Money.GetShortString((long)(item.Price)));

                            }

                        }
                        catch 
                        {

                        }
                    }
                }
            }
        }

      
        public void CraftPriceSave(ItemTemplate item)
        {
            if (item == null)
            {
                log.Error("ItemTemplate cannot be null.");
                return; // Beende die Methode, wenn das Item null ist
            }

            item.AllowUpdate = true;

            int iterationCount = 5; // Anzahl der Iterationen

            for (int i = 0; i < iterationCount; i++)
            {
                // Setze den Craft-Preis
                item.CraftPrice = MatPrice; // Setze den Craft-Preis immer auf MatPrice
            }
            
            // Überprüfe, ob der Craft-Preis gesetzt ist
            if (!item.CraftPriceSet)
            {
                if (MatPrice == 0)
                {
                    log.ErrorFormat("MatPrice was 0! for Item: {0}, Id_nb: {1}", item.Name, item.Id_nb);
                }
            }

            // Berechne den Preis und vermeide Division durch null
            item.Price = (item.CraftPrice == 0) ? 0 : item.CraftPrice * 100 / 75;

            // Speichere das Objekt in der Datenbank
            GameServer.Database.SaveObject(item);

            // Update den Cache für das Item
            GameServer.Database.UpdateInCache<ItemTemplate>(item.Id_nb);
        }
        public virtual void CraftPriceCalculator(GamePlayer player, ItemTemplate itemToCraft)
        {
            // Crafting Verkaufsberechnung
           
                var _craftPrice = 0L;
                var _sellPrice = 0L;
                var _merchantSellPrice = 0L;
                var preiseSetup = false;
               
                var items = GameServer.Database.SelectObjects<ItemTemplate>("`Id_nb` = @Id_nb", new QueryParameter("@Id_nb", itemToCraft.Id_nb));

                if (items == null || items.Count == 0) return;
                
                foreach (var item in items)
                {
                try
                {
                    if (player.Client.Account.PrivLevel == 1)
                    {
                        
                        if (!item.CraftPriceSet)
                        {

                            CraftPriceSave(item);

                            preiseSetup = true;
                        }

                        _craftPrice = item.CraftPrice;
                        _sellPrice = item.Price;
                        _merchantSellPrice = item.Price * ServerProperties.Properties.ITEM_SELL_RATIO / 100;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Crafting Verkaufsberechnung fehler!", ex);
                }
                finally
                {
                    // Infos nur für den Admin
                    if (player.Client.Account.PrivLevel == 1)
                    {
                        if (preiseSetup)
                        {
                            log.Error("CraftPrice was not correct! calculate new..!");
                            preiseSetup = false;
                            log.InfoFormat("CraftPrice set to:{0}", Money.GetShortString(_craftPrice));
                            log.InfoFormat("Price set to 75% of craft price:{0}", Money.GetShortString(_sellPrice));
                            log.InfoFormat("Sell Price on Merchant set to:{0}", Money.GetShortString(_merchantSellPrice));
                            log.ErrorFormat("MatPrice set to:{0} item.Template:{1}", MatPrice, item.Id_nb.ToString());
                        }
                    }
                    
                    MatPrice = 0;
                    addMaterials.Clear();
                }
                }
            
        }


        /// <summary>
        /// Verify that player has the needed materials to craft an item
        /// </summary>
        public virtual bool CheckRawMaterials(GamePlayer player, DBCraftedItem recipe, ItemTemplate itemToCraft, IList<DBCraftedXItem> rawMaterials)
        {

            List<string> missingMaterials = new List<string>();



            foreach (DBCraftedXItem material in rawMaterials)
            {
                ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>(material.IngredientId_nb);

                if (template == null)
                {
                    player.Out.SendMessage("Can't find a material (" + material.IngredientId_nb + ") needed for this recipe.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    //log.Error("Cannot find raw material ItemTemplate: " + material.IngredientId_nb + ") needed for recipe: " + recipe.CraftedItemID);
                    return false;
                }

                bool result = false;
                int count = material.Count;
                int realCount = material.Count;

                //Für Preis calculation only

                if (addMaterials == null)
                {
                    addMaterials = new List<string>(5);
                }
                addMaterials.Add($"({count}) {template.Name}");
                // MatPrice += count * template.Price;

                //log.ErrorFormat("MatPrice:{0} addMaterial:{1} Count:{2}, preis:{3}", MatPrice, template.Name, count, template.Price);
               

                lock (player.Inventory)
                {
                    foreach (InventoryItem item in player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
                    {
                        if (item != null && item.Name == template.Name)
                        {
                            if (item.Count >= count)
                            {

                                result = true;
                                break;
                            }
                            else
                            {
                                //count -= item.Count;
                            }
                        }
                    }

                    if (!result)
                    {
                        missingMaterials.Add("(" + count + ") " + template.Name);
                        // MatPrice += count * template.Price;
                    }
                }

                if (missingMaterials != null)
                {
                    if (missingMaterials.Count > 0)
                    {
                        
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.CheckRawMaterial.NoIngredients", itemToCraft.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractCraftingSkill.CheckRawMaterial.YouAreMissing", itemToCraft.Name), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        
                    }

                    bool messageSent = false;

                    foreach (string materialName in missingMaterials)
                    {
                        if (!messageSent)
                        {
                            //log.ErrorFormat("Missing Materials: {0}", string.Join(", ", missingMaterials.ToArray()));
                            player.Out.SendMessage(materialName, eChatType.CT_Important, eChatLoc.CL_ChatWindow);
                            messageSent = true; // Nachricht wurde gesendet
                            result = false;
                        }
                        result = false;
                    }

                    for (int i = 0; i < missingMaterials.Count; i++)
                    {
                        if (i == missingMaterials.Count)
                        {
                            if (player.Client.Account.PrivLevel == (uint)ePrivLevel.Player) return false;
                        }

                    }

                }
            }
            return true;
        }


        #endregion

        #region Gain points

        /// <summary>
        /// Gain a point in the appropriate skills for a recipe and materials
        /// </summary>
        public virtual void GainCraftingSkillPoints(GamePlayer player, DBCraftedItem recipe, IList<DBCraftedXItem> rawMaterials)
        {
            foreach (DBCraftedXItem material in rawMaterials)
            {
                ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>(material.IngredientId_nb);

                if (template != null)
                {
                    switch (template.Model)
                    {
                        case 522:   //"cloth square"
                        case 537:   //"heavy thread"
                            {
                                if (player.GetCraftingSkillValue(eCraftingSkill.ClothWorking) < subSkillCap)
                                {
                                    player.GainCraftingSkill(eCraftingSkill.ClothWorking, 1);
                                }
                                break;
                            }

                        case 521:   //"leather square"
                            {
                                if (player.GetCraftingSkillValue(eCraftingSkill.LeatherCrafting) < subSkillCap)
                                {
                                    player.GainCraftingSkill(eCraftingSkill.LeatherCrafting, 1);
                                }
                                break;
                            }

                        case 519:   //"metal bars"
                            {
                                if (player.GetCraftingSkillValue(eCraftingSkill.MetalWorking) < subSkillCap)
                                {
                                    player.GainCraftingSkill(eCraftingSkill.MetalWorking, 1);
                                }
                                break;
                            }

                        case 520:   //"wooden boards"
                            {
                                if (player.GetCraftingSkillValue(eCraftingSkill.WoodWorking) < subSkillCap)
                                {
                                    player.GainCraftingSkill(eCraftingSkill.WoodWorking, 1);
                                }
                                break;
                            }
                    }
                }
            }

            player.Out.SendUpdateCraftingSkills();
            //player.SaveIntoDatabase();//extra save
        }
        #endregion

        #region Use materials and created crafted item

        /// <summary>
        /// Remove used raw material from player inventory
        /// </summary>
        /// <param name="player"></param>
        /// <param name="recipe"></param>
        /// <returns></returns>
        public virtual bool RemoveUsedMaterials(GamePlayer player, DBCraftedItem recipe, IList<DBCraftedXItem> rawMaterials)
        {
            var dataSlots = new Dictionary<int, int?>(10);

            lock (player.Inventory)
            {
                foreach (var material in rawMaterials)
                {
                    var template = GameServer.Database.FindObjectByKey<ItemTemplate>(material.IngredientId_nb);

                    if (template == null)
                    {
                        player.Out.SendMessage($"Can't find a material ({material.IngredientId_nb}) needed for this recipe.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        //log.Error($"RemoveUsedMaterials: Cannot find raw material ItemTemplate: {material.IngredientId_nb} needed for recipe: {recipe.CraftedItemID}");
                        return false;
                    }

                    if (!TryRemoveMaterial(player, template, material.Count, dataSlots))
                    {
                        return false;
                    }
                }
            }

            CommitInventoryChanges(player, dataSlots);

            return true; // all raw materials removed and item created
        }

        private bool TryRemoveMaterial(GamePlayer player, ItemTemplate template, int count, Dictionary<int, int?> dataSlots)
        {
            foreach (var item in player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
            {
                if (item == null || item.Name != template.Name) continue;

                try
                {
                    if (item.Count >= count)
                    {
                        dataSlots.Add(item.SlotPosition, item.Count == count ? (int?)null : count);
                        MatPrice += item.Price * count;
                        return true; // Material successfully removed
                    }
                    else
                    {
                        dataSlots.Add(item.SlotPosition, null);
                        count -= item.Count;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Crafted Item count Error in AbstractCraftingSkill.cs", ex);
                    return false;
                }
            }

            return false; // Not enough materials found
        }

        private void CommitInventoryChanges(GamePlayer player, Dictionary<int, int?> dataSlots)
        {
            player.Inventory.BeginChanges();

            foreach (var de in dataSlots)
            {
                var item = player.Inventory.GetItem((eInventorySlot)de.Key);
                if (item != null)
                {
                    if (!de.Value.HasValue)
                    {
                        player.Inventory.RemoveItem(item);
                    }
                    else
                    {
                        player.Inventory.RemoveCountFromStack(item, de.Value.Value);
                    }
                }
            }

            player.Inventory.CommitChanges();
        }


        /// <summary>
        /// Make the crafted item and add it to player's inventory
        /// </summary>
        /// <param name="player"></param>
        /// <param name="recipe"></param>
        /// <returns></returns>
        protected virtual Task BuildCraftedItem(GamePlayer player, DBCraftedItem recipe, ItemTemplate itemToCraft)
        {
            return CraftItem(player, recipe, itemToCraft);
        }

        public async Task<bool> CraftItem(GamePlayer player, DBCraftedItem recipe, ItemTemplate itemToCraft)
        {
            // log.ErrorFormat("Craft item: {0}", itemToCraft.Name);
            InventoryItem newItem = null;

            CraftPriceCalculator(player, itemToCraft);

            await Task.Run(() =>
            {
                var changedSlots = new Dictionary<int, int>(5); // key : > 0 inventory ; < 0 ground || value: < 0 = new item count; > 0 = add to old

                lock (player.Inventory)
                {
                    int count = itemToCraft.PackSize < 1 ? 1 : itemToCraft.PackSize;
                    foreach (var item in player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
                    {
                        if (item == null || !item.Id_nb.Equals(itemToCraft.Id_nb) || item.Count >= itemToCraft.MaxCount)
                            continue;

                        int countFree = item.MaxCount - item.Count;
                        if (count > countFree)
                        {
                            changedSlots.Add(item.SlotPosition, countFree); // existing item should be changed
                            count -= countFree;
                        }
                        else
                        {
                            changedSlots.Add(item.SlotPosition, count); // existing item should be changed
                            count = 0;
                            break;
                        }
                    }

                    if (count > 0) // Add new object
                    {
                        var firstEmptySlot = player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
                        changedSlots.Add(firstEmptySlot == eInventorySlot.Invalid ? -1 : (int)firstEmptySlot, -count); // Create the item in either the ground or free slot
                        count = 0;
                    }

                    player.Inventory.BeginChanges();

                    foreach (var slot in changedSlots)
                    {
                        int countToAdd = slot.Value;
                        if (countToAdd > 0) // Add to existing item
                        {
                            newItem = player.Inventory.GetItem((eInventorySlot)slot.Key);
                            if (newItem != null && player.Inventory.AddCountToStack(newItem, countToAdd))
                            {
                                continue; // count incremented, continue with next change
                            }
                        }

                        if (recipe.MakeTemplated)
                        {
                            newItem = GameInventoryItem.Create<ItemTemplate>(itemToCraft);
                        }
                        else
                        {
                            var unique = new ItemUnique(itemToCraft);
                            GameServer.Database.AddObject(unique);
                            newItem = GameInventoryItem.Create<ItemUnique>(unique);
                            newItem.Quality = GetQuality(player, recipe);
                        }

                        newItem.IsCrafted = true;
                        newItem.Creator = player.Name;
                        newItem.Count = -countToAdd;

                        if (slot.Key > 0) // Create new item in the backpack
                        {
                            player.Inventory.AddItem((eInventorySlot)slot.Key, newItem);
                        }
                        else // Create new item on the ground
                        {
                            player.CreateItemOnTheGround(newItem);
                            player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0, 0, 0, 0, eDialogType.Ok, true,
                                LanguageMgr.GetTranslation(player.Client.Account.Language, $"AbstractCraftingSkill.BuildCraftedItem.BackpackFull", itemToCraft.Name));
                        }
                    }

                    player.Inventory.CommitChanges();

                    // Save Craft Price
                    SetCraftPrice(itemToCraft, player);

                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, $"AbstractCraftingSkill.BuildCraftedItem.Successfully", itemToCraft.Name, newItem.Quality), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);

                    if (!recipe.MakeTemplated && newItem.Quality == 100)
                    {
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, $"AbstractCraftingSkill.BuildCraftedItem.Masterpiece"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        player.Out.SendPlaySound(eSoundType.Craft, 0x04);
                    }
                    else
                    {
                        player.Out.SendPlaySound(eSoundType.Craft, 0x03);
                    }
                }
            });

            return true;
        }


        #endregion

        #region Calcul functions

        /// <summary>
        /// Calculate chance to succes
        /// </summary>
        public virtual int CalculateChanceToMakeItem(GamePlayer player, DBCraftedItem recipe)
        {
            int con = GetItemCon(player.GetCraftingSkillValue(m_eskill), recipe.CraftingLevel);

            // Begrenzung des Wertes von con zwischen -3 und 3
            con = Math.Max(-3, Math.Min(con, 3)); // Verwende Math.Max und Math.Min zur Begrenzung

            // Berechne die Chancen, basierend auf dem Wert von con
            int[] chances = { 100, 100, 100, 92, 84, 68, 0 }; // Index 0 entspricht -3 und Index 6 entspricht 3

            return chances[con + 3]; // Verschiebe den Index um 3 nach oben, um negative Indizes zu vermeiden
        }


        /// <summary>
        /// Calculate chance to gain point
        /// </summary>
        public virtual int CalculateChanceToGainPoint(GamePlayer player, DBCraftedItem recipe)
        {
            var con = GetItemCon(player.GetCraftingSkillValue(m_eskill), recipe.CraftingLevel);

            // Begrenzung des Wertes von con zwischen -3 und 3
            con = Math.Max(-3, Math.Min(con, 3)); // Verwende Math.Max und Math.Min zur Begrenzung

            // Verwende ein Array, um die Rückgabewerte schnell zu bestimmen
            int[] chances = { 0, 15, 30, 45, 55, 45, 0 }; // Index 0 entspricht -3 und Index 6 entspricht 3

            return chances[con + 3]; // Verschiebe den Index um 3 nach oben, um negative Indizes zu vermeiden
        }

        /// <summary>
        /// Calculate crafting time
        /// </summary>
        public virtual int GetCraftingTime(GamePlayer player, DBCraftedItem recipe, IList<DBCraftedXItem> rawMaterials)
        {
            var baseMultiplier = CalculateBaseMultiplier(recipe.CraftingLevel);
            var materialsCount = CalculateMaterialsCount(rawMaterials);
            var craftingTime = CalculateInitialCraftingTime(baseMultiplier, materialsCount);

            craftingTime = ApplyPlayerSpeed(craftingTime, player.CraftingSpeed);
            craftingTime = ApplyKeepBonuses(craftingTime, player.Realm);

            var con = GetItemCon(player.GetCraftingSkillValue(m_eskill), recipe.CraftingLevel);
            craftingTime = ApplyConMod(craftingTime, con);

            return craftingTime < 1 ? 1 : craftingTime;
        }

        private double CalculateBaseMultiplier(int craftingLevel)
        {
            return (craftingLevel / 100.0) + 1;
        }

        private ushort CalculateMaterialsCount(IList<DBCraftedXItem> rawMaterials)
        {
            var materialsCount = (ushort)0;
            foreach (var material in rawMaterials)
            {
                materialsCount += (ushort)material.Count;
            }
            return materialsCount;
        }

        private int CalculateInitialCraftingTime(double baseMultiplier, ushort materialsCount)
        {
            return (int)(baseMultiplier * materialsCount / 4);
        }

        private int ApplyPlayerSpeed(int craftingTime, double craftingSpeed)
        {
            return (int)(craftingTime / craftingSpeed);
        }

        private int ApplyKeepBonuses(int craftingTime, eRealm realm)
        {
            if (Keeps.KeepBonusMgr.RealmHasBonus(DOL.GS.Keeps.eKeepBonusType.Craft_Timers_5, realm))
                return (int)(craftingTime / 1.05);
            else if (Keeps.KeepBonusMgr.RealmHasBonus(DOL.GS.Keeps.eKeepBonusType.Craft_Timers_3, realm))
                return (int)(craftingTime / 1.03);

            return craftingTime;
        }

        private int ApplyConMod(int craftingTime, int con)
        {
            // Verwende ein Array für die Modifikatoren basierend auf dem Wert von con
            double[] mods = { 0.4, 0.6, 0.8, 1.0, 1.0, 1.0, 1.0 }; // Index 0 entspricht -3 und Index 6 entspricht 3

            // Begrenzung des Wertes von con zwischen -3 und 3
            con = Math.Max(-3, Math.Min(con, 3)); // Verwende Math.Max und Math.Min zur Begrenzung

            // Berechne und gebe den modifizierten Wert zurück
            return (int)(craftingTime * mods[con + 3]); // Verschiebe den Index um 3 nach oben
        }



        /// <summary>
        /// Calculate the minumum needed secondary crafting skill level to make the item
        /// </summary>
        public virtual int GetSecondaryCraftingSkillMinimumLevel(DBCraftedItem recipe, ItemTemplate itemToCraft)
        {
            return 0;
        }

        /// <summary>
        /// Calculate crafted item quality
        /// </summary>
        private int GetQuality(GamePlayer player, DBCraftedItem item)
        {

            //Extra masterpiece bonus
            GameSpellEffect masterpieceBonusEffect = SpellHandler.FindEffectOnTarget(player, "CraftingBetterBonus");

            int baseMasterPieceChance = 2;
            int baseMasterQualityChance = 3;
            if (masterpieceBonusEffect != null)
            {
                baseMasterPieceChance += (int)masterpieceBonusEffect.Spell.Value;
                baseMasterQualityChance += (int)masterpieceBonusEffect.Spell.Value;

            }

            // 2% chance to get masterpiece, 1:6 chance to get 94-99%, if legendary or if grey con
            // otherwise moving the most load towards 94%, the higher the item con to the crafter skill
            //1.87 patch raises min quality to 96%

            // legendary
            if (player.GetCraftingSkillValue(m_eskill) >= 1000)
            {
                if (Util.Chance(baseMasterPieceChance))
                {
                    return 100; // 2% chance for master piece
                }
                return 96 + Util.Random(baseMasterQualityChance); // 3% chance base
            }

            int delta = GetItemCon(player.GetCraftingSkillValue(m_eskill), item.CraftingLevel);
            if (delta < -2)
            {
                if (Util.Chance(baseMasterPieceChance))
                    return 100; // grey items get 2% chance to be master piece
                return 96 + Util.Random(baseMasterQualityChance); // handle grey items like legendary // 3% chance base
            }

            // this is a type of roulette selection, imagine a roulette wheel where all chances get different sized
            // fields where the ball can drop, the bigger the field, the bigger the chance
            // field size is modulated by item con and can be also 0

            // possible chance allocation scheme for yellow item
            // 99:
            // 98:
            // 97: o
            // 96: oo
            // where one 'o' marks 100 size, this example results in 10% chance for yellow items to be 97% quality

            delta *= 100;

            int[] chancePart = new int[4]; // index ranges from 96%(0) to 99%(5)
            int sum = 0;
            for (int i = 0; i < 4; i++)
            {
                chancePart[i] = Math.Max((4 - i) * 100 - delta, 0); // 0 minimum
                sum += chancePart[i];
            }

            // selection
            int rand = Util.Random(sum);
            for (int i = 3; i >= 0; i--)
            {
                if (rand < chancePart[i])
                    return 96 + i;
                rand -= chancePart[i];
            }

            // if something still not clear contact Thrydon/Blue

            return 96;
        }


        /// <summary>
        /// get item con color compared to crafters skill
        /// </summary>
        /// <param name="crafterSkill"></param>
        /// <param name="itemCraftingLevel"></param>
        /// <returns>-3 grey, -2 green, -1 blue, 0 yellow, 1 orange, 2 red, 3 purple</returns>
        public int GetItemCon(int crafterSkill, int itemCraftingLevel)
        {
            int diff = itemCraftingLevel - crafterSkill;

            // Verwende Array, um die Rückgabewerte basierend auf dem Unterschied zu speichern
            int[] conValues = { -3, -2, -1, 0, 1, 2, 3 }; // Reihenfolge entspricht den notwendigen Rückgabewerten
            int[] thresholds = { -50, -31, -11, 0, 19, 49 }; // Grenzwerte für die verschiedenen con-Werte

            // Bestimmen des entsprechenden con-Wertes
            if (diff <= thresholds[0]) return conValues[0]; // grey
            if (diff <= thresholds[1]) return conValues[1]; // green
            if (diff <= thresholds[2]) return conValues[2]; // blue
            if (diff <= thresholds[3]) return conValues[3]; // yellow
            if (diff <= thresholds[4]) return conValues[4]; // orange
            if (diff <= thresholds[5]) return conValues[5]; // red

            return conValues[6]; // impossible
        }

        #endregion
    }
}
