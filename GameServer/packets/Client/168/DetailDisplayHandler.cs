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
using DOL.GS.Quests;
using DOL.GS.RealmAbilities;
using DOL.GS.Spells;
using DOL.GS.Styles;
using DOL.Language;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DOL.GS.PacketHandler.Client.v168
{
    /// <summary>
    /// delve button shift+i = detail of spell object...
    /// </summary>
    [PacketHandlerAttribute(PacketHandlerType.TCP, 0x70 ^ 168, "Handles detail display")]
    public class DetailDisplayHandler : IPacketHandler
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Proc Spelltype for items
        /// </summary>
        protected Spell procType = null;

        /// <summary>
        /// Proc Spelltype for items
        /// </summary>
        protected Spell procType1 = null;

        public void HandlePacket(GameClient client, GSPacketIn packet)
        {
            if (client == null || client.Player == null)
            {
                return;
            }

            ushort objectType = packet.ReadShort();

            uint extraID = 0;
            if (client.Version >= GameClient.eClientVersion.Version186)
            {
                extraID = packet.ReadInt();
            }

            ushort objectID = packet.ReadShort();

            string caption = String.Empty;
            var objectInfo = new List<string>();

            /*
            Type    Description         Id
            1       Inventory item      Slot (ie. 0xC for 2 handed weapon)
            2       Spell               spell level + spell line ID * 100 (starting from 0)
            3       ???
            4       Merchant item       Slot (divide by 30 to get page)
            5       Buff/effect         The buff id (each buff has a unique id)
            6       Style               style list index = ID-100-abilities count
            7       Trade window        position in trade window (starting form 0)
            8       Ability             100+position in players abilities list (?)
            9       Trainers skill      position in trainers window list
            10		Market Search		slot?
            19		Reward Quest
             */

            ChatUtil.SendDebugMessage(client, string.Format("Delve objectType={0}, objectID={1}, extraID={2}", objectType, objectID, extraID));

            ItemTemplate item = null;
            InventoryItem invItem = null;


            switch (objectType)
            {
                #region Inventory Item
                case 1: //Display Infos on inventory item
                case 10: // market search
                    {
                        if (objectType == 1)
                        {
                            IGameInventoryObject invObject = client.Player.TargetObject as IGameInventoryObject;
                                                       
                            if (invItem == null)
                            {
                                invItem = client.Player.Inventory.GetItem((eInventorySlot)objectID);
                            }
                            // finally try active inventory object
                            if (invItem == null)
                            {
                                if (client.Player.ActiveInventoryObject != null)
                                {
                                    invObject = client.Player.ActiveInventoryObject;
                                    if (invObject != null && invObject.GetClientInventory(client.Player) != null)
                                    invObject.GetClientInventory(client.Player).TryGetValue(objectID, out invItem);
                                }
                            }
                                 // finally try direct inventory access
                            if (invItem == null)
                            {
                                invItem = client.Player.Inventory.GetItem((eInventorySlot)objectID);
                            }
 	  	 
 	  	                                                         // Failed to get any inventory
                                if (invItem == null)
                                    return;
                             
                        }
                        else if (objectType == 10)
                        {
                            List<InventoryItem> list = client.Player.TempProperties.getProperty<object>(MarketExplorer.EXPLORER_ITEM_LIST, null) as List<InventoryItem>;
                            if (list == null)
                            {
                                list = client.Player.TempProperties.getProperty<object>("TempSearchKey", null) as List<InventoryItem>;
                                if (list == null)
                                    return;
                            }

                            if (objectID >= list.Count)
                                return;

                            invItem = list[objectID];

                            if (invItem == null)
                                return;
                        }

                        // Aredhel: Start of a more sophisticated item delve system.
                        // The idea is to have every item inherit from an item base class,
                        // this base class will provide a method
                        //
                        // public virtual void Delve(List<String>, GamePlayer player)
                        //
                        // which can be overridden in derived classes to provide additional
                        // information. Same goes for spells, just add the spell delve
                        // in the Delve() hierarchy. This will on one hand make this class
                        // much more concise (1800 lines at the time of this writing) and
                        // on the other hand the whole delve system much more flexible, for
                        // example when adding new item types (artifacts, for example) you
                        // provide *only* an overridden Delve() method, use the base
                        // Delve() and you're done, spells, charges and everything else.

                        // Let the player class create the appropriate item to delve
                        caption = invItem.Name;

                        if (client.Player.DelveItem<InventoryItem>(invItem, objectInfo, client.Player))
                            break;

                        #region Old Delve

                        if (invItem is InventoryArtifact)
                        {
                            List<string> delve = new List<string>();
                            (invItem as InventoryArtifact).Delve(delve, client.Player);

                            foreach (string line in delve)
                                objectInfo.Add(line);

                            break;
                        }

                        //**********************************
                        //show crafter name
                        //**********************************
                        if (invItem.IsCrafted)
                        {
                            objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.CrafterName", invItem.Creator));
                            objectInfo.Add(" ");
                        }
                        else if (invItem.Description != null && invItem.Description != "")
                        {
                            objectInfo.Add(invItem.Description);
                            objectInfo.Add(" ");
                        }

                        if ((invItem.Object_Type >= (int)eObjectType.GenericWeapon) && (invItem.Object_Type <= (int)eObjectType._LastWeapon) ||
                            invItem.Object_Type == (int)eObjectType.Instrument)
                        {
                            WriteUsableClasses(objectInfo, invItem, client);
                            WriteMagicalBonuses(objectInfo, invItem, client, false);
                            WriteClassicWeaponInfos(objectInfo, invItem, client);
                        }

                        if (invItem.Object_Type >= (int)eObjectType.Cloth && invItem.Object_Type <= (int)eObjectType.Scale)
                        {
                            WriteUsableClasses(objectInfo, invItem, client);
                            WriteMagicalBonuses(objectInfo, invItem, client, false);
                            WriteClassicArmorInfos(objectInfo, invItem, client);
                        }

                        if (invItem.Object_Type == (int)eObjectType.Shield)
                        {
                            WriteUsableClasses(objectInfo, invItem, client);
                            WriteMagicalBonuses(objectInfo, invItem, client, false);
                            WriteClassicShieldInfos(objectInfo, invItem, client);
                        }

                        if (invItem.Object_Type == (int)eObjectType.Magical || invItem.Object_Type == (int)eObjectType.AlchemyTincture || invItem.Object_Type == (int)eObjectType.SpellcraftGem)
                        {
                            WriteMagicalBonuses(objectInfo, invItem, client, false);
                        }

                        //***********************************
                        //shows info for Poison Potions
                        //***********************************
                        if (invItem.Object_Type == (int)eObjectType.Poison)
                        {
                            WritePoisonInfo(objectInfo, invItem, client);
                        }

                        if (invItem.Object_Type == (int)eObjectType.Magical && invItem.Item_Type == (int)eInventorySlot.FirstBackpack) // potion
                        {
                            WritePotionInfo(objectInfo, invItem, client);
                        }
                        else if (invItem.CanUseEvery > 0)
                        {
                            // Items with a reuse timer (aka cooldown).
                            objectInfo.Add(" ");

                            int minutes = invItem.CanUseEvery / 60;
                            int seconds = invItem.CanUseEvery % 60;

                            if (minutes == 0)
                            {
                                objectInfo.Add(String.Format("Can use item every: {0} sec", seconds));
                            }
                            else
                            {
                                objectInfo.Add(String.Format("Can use item every: {0}:{1:00} min", minutes, seconds));
                            }

                            // objectInfo.Add(String.Format("Can use item every: {0:00}:{1:00}", minutes, seconds));

                            int cooldown = invItem.CanUseAgainIn;

                            if (cooldown > 0)
                            {
                                minutes = cooldown / 60;
                                seconds = cooldown % 60;

                                if (minutes == 0)
                                {
                                    objectInfo.Add(String.Format("Can use again in: {0} sec", seconds));
                                }
                                else
                                {
                                    objectInfo.Add(String.Format("Can use again in: {0}:{1:00} min", minutes, seconds));
                                }

                                // objectInfo.Add(String.Format("Can use again in: {0:00}:{1:00}", minutes, seconds));
                            }
                        }

                        if (!invItem.IsDropable || !invItem.IsPickable || invItem.IsIndestructible)
                            objectInfo.Add(" ");

                        if (!invItem.IsPickable)
                            objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.CannotTraded"));

                        if (!invItem.IsDropable)
                            objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.CannotSold"));

                        if (invItem.IsIndestructible)
                            objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.CannotDestroyed"));


                        if (invItem.BonusLevel > 0)
                        {
                            objectInfo.Add(" ");
                            objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.BonusLevel", invItem.BonusLevel));

                        }

                        //Add admin info
                        if (client.Account.PrivLevel > 1)
                        {
                            WriteTechnicalInfo(objectInfo, invItem);
                        }

                        break;

                        #endregion Old Delve
                    }
                #endregion
                #region Spell
                case 2: //spell
                  {
                        int lineID = objectID / 100;
                        int spellID = objectID % 100;

                        SpellLine spellLine = client.Player.GetSpellLines()[lineID] as SpellLine;
                        if (spellLine == null)
                            return;

                        Spell spell = null;
                        foreach (Spell spl in SkillBase.GetSpellList(spellLine.KeyName))
                        {
                            if (spl.Level == spellID)
                            {
                                spell = spl;
                                break;
                            }
                        }
                        if (spell == null)
                            return;

                        caption = spell.Name;
                        WriteSpellInfo(objectInfo, spell, spellLine, client);
                        break;
                    }
                case 3: //spell
                    {
                        IList skillList = client.Player.GetNonTrainableSkillList();
                        IList styles = client.Player.GetStyleList();
                        int index = objectID - skillList.Count - styles.Count - 100;

                        List<SpellLine> spelllines = client.Player.GetSpellLines();
                        if (spelllines == null || index < 0)
                            break;

                        lock (client.Player.lockSpellLinesList)
                        {
                            ReaderWriterDictionary<string, KeyValuePair<Spell, SpellLine>> spelllist = client.Player.GetUsableSpells(spelllines, false);

                            if (index >= spelllist.Count)
                            {
                                index -= spelllist.Count;
                            }

                            else
                            {


                                var spellenum = spelllist.Values.GetEnumerator();

                                // ConcurrentDictionary<string, KeyValuePair<Spell, SpellLine>> spellenum = spelllist.Values.GetEnumerator();
                                int i = 0;
                                while (spellenum.MoveNext())
                                {
                                    if (i == index)
                                    {
                                        caption = spellenum.Current.Key.Name;
                                        WriteSpellInfo(objectInfo, spellenum.Current.Key, spellenum.Current.Value, client);
                                        break;
                                    }

                                    i++;
                                }

                            }

                            break;


                        }
                    }
                #endregion
                #region Merchant / RewardQuest
                case 4: //Display Infos on Merchant objects
                case 19: //Display Info quest reward
                    {
                        if (objectType == 4)
                        {
                            GameMerchant merchant = null;
                            if (client.Player.TargetObject != null && client.Player.TargetObject is GameMerchant)
                                merchant = (GameMerchant)client.Player.TargetObject;
                            if (merchant == null)
                                return;

                            int pagenumber = objectID / MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;
                            int slotnumber = objectID % MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;

                            item = merchant.TradeItems.GetItem(pagenumber, (eMerchantWindowSlot)slotnumber);
                        }
                        else if (objectType == 19)
                        {
                            ushort questID = (ushort)((extraID << 12) | (ushort)(objectID >> 4));
                            int index = objectID & 0x0F;

                            GameLiving questGiver = null;
                            if (client.Player.TargetObject != null && client.Player.TargetObject is GameLiving)
                                questGiver = (GameLiving)client.Player.TargetObject;

                            ChatUtil.SendDebugMessage(client, "Quest ID: " + questID);

                            if (questID == 0)
                                return; // questID == 0, wrong ID ?

                            if (questID <= DataQuest.DATAQUEST_CLIENTOFFSET)
                            {
                                AbstractQuest q = client.Player.IsDoingQuest(QuestMgr.GetQuestTypeForID(questID));

                                if (q == null)
                                {
                                    // player not doing quest, most likely on offer screen
                                    if (questGiver != null)
                                    {
                                        try
                                        {
                                            q = (AbstractQuest)Activator.CreateInstance(QuestMgr.GetQuestTypeForID(questID), new object[] { client.Player, 1 });
                                        }
                                        catch
                                        {
                                            // we tried!
                                        }
                                    }

                                    if (q == null)
                                    {
                                        ChatUtil.SendDebugMessage(client, "Can't find or create quest!");
                                        return;
                                    }
                                }

                                if (!(q is RewardQuest))
                                    return; // this is not new quest

                                List<ItemTemplate> rewards = null;
                                if (index < 8)
                                    rewards = (q as RewardQuest).Rewards.BasicItems;
                                else
                                {
                                    rewards = (q as RewardQuest).Rewards.OptionalItems;
                                    index -= 8;
                                }
                                if (rewards != null && index >= 0 && index < rewards.Count)
                                {
                                    item = rewards[index];
                                }
                            }
                            else // Data quest support, check for RewardQuest type
                            {
                                DataQuest dq = null;

                                foreach (DBDataQuest d in GameObject.DataQuestCache)
                                {
                                    if (d.ID == questID - DataQuest.DATAQUEST_CLIENTOFFSET)
                                    {
                                        dq = new DataQuest(d);
                                        break;
                                    }
                                }

                                if (dq != null && dq.StartType == DataQuest.eStartType.RewardQuest)
                                {
                                    List<ItemTemplate> rewards = null;
                                    if (index < 8)
                                        rewards = dq.FinalRewards;
                                    else
                                    {
                                        rewards = dq.OptionalRewards;
                                        index -= 8;
                                    }
                                    if (rewards != null && index >= 0 && index < rewards.Count)
                                    {
                                        item = rewards[index];
                                    }
                                }
                            }

                        }


                        if (item == null)
                            return;

                        caption = item.Name;

                        if (client.Player.DelveItem<ItemTemplate>(item, objectInfo, client.Player))
                            break;

                        #region Old Delve

                        // fallback to old delve

                        if (item.Item_Type == (int)eInventorySlot.Horse)
                        {
                            WriteHorseInfo(objectInfo, item, client, "");
                        }

                        if ((item.Item_Type == (int)eInventorySlot.HorseBarding || item.Item_Type == (int)eInventorySlot.HorseArmor) && item.Level > 0)
                        {
                            objectInfo.Add(" ");//empty line
                            objectInfo.Add(" ");//empty line
                            objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.ChampionLevel", item.Level));
                        }

                        if ((item.Object_Type >= (int)eObjectType.GenericWeapon) && (item.Object_Type <= (int)eObjectType.MaulerStaff) ||
                            item.Object_Type == (int)eObjectType.Instrument)
                        {
                            WriteUsableClasses(objectInfo, item, client);
                            WriteMagicalBonuses(objectInfo, item, client, false);
                            WriteClassicWeaponInfos(objectInfo, GameInventoryItem.Create<ItemTemplate>(item), client);
                        }

                        if (item.Object_Type >= (int)eObjectType.Cloth && item.Object_Type <= (int)eObjectType.Scale)
                        {
                            WriteUsableClasses(objectInfo, item, client);
                            WriteMagicalBonuses(objectInfo, item, client, false);
                            WriteClassicArmorInfos(objectInfo, GameInventoryItem.Create<ItemTemplate>(item), client);
                        }

                        if (item.Object_Type == (int)eObjectType.Shield)
                        {
                            WriteUsableClasses(objectInfo, item, client);
                            WriteMagicalBonuses(objectInfo, item, client, false);
                            WriteClassicShieldInfos(objectInfo, item, client);
                        }

                        if ((item.Item_Type != (int)eInventorySlot.Horse && item.Object_Type == (int)eObjectType.Magical) || item.Object_Type == (int)eObjectType.AlchemyTincture || item.Object_Type == (int)eObjectType.SpellcraftGem)
                        {
                            WriteMagicalBonuses(objectInfo, item, client, false);
                        }

                        if (item.Object_Type == (int)eObjectType.Poison)
                        {
                            WritePoisonInfo(objectInfo, item, client);
                        }

                        if (item.Object_Type == (int)eObjectType.Magical && item.Item_Type == 40) // potion
                            WritePotionInfo(objectInfo, item, client);

                        //Add admin info
                        if (client.Account.PrivLevel > 1)
                        {
                            WriteTechnicalInfo(objectInfo, GameInventoryItem.Create<ItemTemplate>(item), item.MaxDurability, item.MaxCondition);
                        }
                        break;

                        #endregion Old Delve
                    }
                #endregion
                #region Effect
                case 5: //icons on top (buffs/dots)
                    {
                        IGameEffect foundEffect = null;
                        lock (client.Player.EffectList)
                        {
                            foreach (IGameEffect effect in client.Player.EffectList)
                            {
                                if (effect.InternalID == objectID)
                                {
                                    foundEffect = effect;
                                    break;
                                }
                            }
                        }

                        if (foundEffect == null) break;

                        caption = foundEffect.Name;
                        objectInfo.AddRange(foundEffect.DelveInfo(client.Player));

                        if (client.Account.PrivLevel > 1 && foundEffect is GameSpellEffect)
                        {
                            if ((foundEffect as GameSpellEffect).Spell != null)
                            {
                                if (client.Account.PrivLevel > 1)
                                {
                                    objectInfo.Add(" ");
                                    objectInfo.Add("----------Technical informations----------");
                                    objectInfo.Add("Line: " + ((foundEffect as GameSpellEffect).SpellHandler == null ? "unknown" : (foundEffect as GameSpellEffect).SpellHandler.SpellLine.Name));
                                    objectInfo.Add("SpellID: " + (foundEffect as GameSpellEffect).Spell.ID);
                                    objectInfo.Add("Type: " + (foundEffect as GameSpellEffect).Spell.SpellType);
                                    objectInfo.Add("ClientEffect: " + (foundEffect as GameSpellEffect).Spell.ClientEffect);
                                    objectInfo.Add("Icon: " + (foundEffect as GameSpellEffect).Spell.Icon);
                                    if ((foundEffect as GameSpellEffect).SpellHandler != null)
                                        objectInfo.Add("HasPositiveEffect: " + (foundEffect as GameSpellEffect).SpellHandler.HasPositiveEffect);
                                    objectInfo.Add("Disabled: " + (foundEffect as GameSpellEffect).IsDisabled);
                                }
                            }
                        }


                        break;
                    }
                #endregion
                #region Style
                case 6: //style
                    {
                        IList styleList = client.Player.GetStyleList();
                        IList skillList = client.Player.GetNonTrainableSkillList();
                        Style style = null;
                        int styleID = objectID - skillList.Count - 100;

                        if (styleID < 0 || styleID >= styleList.Count) break;

                        style = styleList[styleID] as Style;
                        if (style == null) break;

                        caption = style.Name;

                        WriteStyleInfo(objectInfo, style, client);
                        break;
                    }
                #endregion
                #region Trade Window
                case 7: //trade windows
                    {
                        ITradeWindow playerTradeWindow = client.Player.TradeWindow;
                        if (playerTradeWindow == null)
                            return;

                        if (playerTradeWindow.PartnerTradeItems != null && objectID < playerTradeWindow.PartnerItemsCount)
                            invItem = (InventoryItem)playerTradeWindow.PartnerTradeItems[objectID];

                        if (invItem == null)
                            return;

                        // Let the player class create the appropriate item to delve
                        caption = invItem.Name;

                        if (client.Player.DelveItem<InventoryItem>(invItem, objectInfo, client.Player))
                            break;

                        #region Old Delve
                        // fallback to old delve

                        if (invItem.Item_Type == (int)eInventorySlot.Horse)
                        {
                            WriteHorseInfo(objectInfo, invItem, client, "");
                        }

                        if ((invItem.Item_Type == (int)eInventorySlot.HorseBarding || invItem.Item_Type == (int)eInventorySlot.HorseArmor) && invItem.Level > 0)
                        {
                            objectInfo.Add(" ");//empty line
                            objectInfo.Add(" ");//empty line
                            objectInfo.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.HandlePacket.ChampionLevel", invItem.Level));
                        }
                        if ((invItem.Object_Type >= (int)eObjectType.GenericWeapon) && (invItem.Object_Type <= (int)eObjectType.MaulerStaff) ||
                            invItem.Object_Type == (int)eObjectType.Instrument)
                        {
                            WriteUsableClasses(objectInfo, invItem, client);
                            WriteMagicalBonuses(objectInfo, invItem, client, false);
                            WriteClassicWeaponInfos(objectInfo, invItem, client);
                        }

                        if (invItem.Object_Type >= (int)eObjectType.Cloth && invItem.Object_Type <= (int)eObjectType.Scale)
                        {
                            WriteUsableClasses(objectInfo, invItem, client);
                            WriteMagicalBonuses(objectInfo, invItem, client, false);
                            WriteClassicArmorInfos(objectInfo, invItem, client);
                        }

                        if (invItem.Object_Type == (int)eObjectType.Shield)
                        {
                            WriteUsableClasses(objectInfo, invItem, client);
                            WriteMagicalBonuses(objectInfo, invItem, client, false);
                            WriteClassicShieldInfos(objectInfo, invItem, client);
                        }

                        if ((invItem.Item_Type != (int)eInventorySlot.Horse && invItem.Object_Type == (int)eObjectType.Magical) || invItem.Object_Type == (int)eObjectType.AlchemyTincture || invItem.Object_Type == (int)eObjectType.SpellcraftGem)
                        {
                            WriteMagicalBonuses(objectInfo, invItem, client, false);
                        }

                        if (invItem.Object_Type == (int)eObjectType.Poison)
                        {
                            WritePoisonInfo(objectInfo, invItem, client);
                        }

                        if (invItem.Object_Type == (int)eObjectType.Magical && invItem.Item_Type == 40) // potion
                            WritePotionInfo(objectInfo, invItem, client);

                        //Add admin info
                        if (client.Account.PrivLevel > 1)
                        {
                            WriteTechnicalInfo(objectInfo, invItem);
                        }

                        break;

                        #endregion Old Delve
                    }
                #endregion
                #region Ability
                case 8://abilities
                    {
                        int id = objectID - 100;
                        IList skillList = client.Player.GetNonTrainableSkillList();
                        Ability abil = (Ability)skillList[id];
                        if (abil != null)
                        {
                            IList allabilitys = client.Player.GetAllAbilities();
                            foreach (Ability checkab in allabilitys)
                            {
                                if (checkab.Name == abil.Name)
                                {
                                    if (checkab.DelveInfo(client.Player).Count > 0)
                                        objectInfo.AddRange(checkab.DelveInfo(client.Player));
                                    else
                                        objectInfo.Add("There is no special information.");


                                }
                            }
                        }
                        break;
                    }
                #endregion
                #region Trainer
                case 9: //trainer window "info" button
                    {
                        IList specList = client.Player.GetSpecList().ToArray();
                        Specialization spec;
                        if (objectID < specList.Count)
                        {
                            spec = (Specialization)specList[objectID];
                        }
                        else
                        {
                            //delve on realm abilities [by Suncheck]
                            if (objectID >= 50)
                            {
                                int clientclassID = client.Player.CharacterClass.ID;
                                int sub = 50;
                                var ra_list = SkillBase.GetClassRealmAbilities(clientclassID).Where(ra => !(ra is RR5RealmAbility));

                                RealmAbility ab = ra_list.ElementAtOrDefault((int)(objectID - sub));


                                if (ab != null)
                                {
                                    caption = ab.Name;
                                    objectInfo.AddRange(ab.DelveInfo(client.Player));
                                    break;
                                }
                            }
                            caption = "Specialization not found";
                            objectInfo.Add("that specialization is not found, id=" + objectID);
                            break;
                        }

                        List<Style> styles = SkillBase.GetStyleList(spec.KeyName, client.Player.CharacterClass.ID);
                        IList playerSpells = client.Player.GetSpellLines();
                        SpellLine selectedSpellLine = null;

                        lock (playerSpells.SyncRoot)
                        {
                            foreach (SpellLine line in playerSpells)
                            {
                                if (!line.IsBaseLine && line.Spec == spec.KeyName)
                                {
                                    selectedSpellLine = line;
                                    break;
                                }
                            }
                        }

                        List<Spell> spells = new List<Spell>();
                        if (selectedSpellLine != null)
                            spells = SkillBase.GetSpellList(selectedSpellLine.KeyName);

                        caption = spec.Name;

                        if (styles.Count <= 0 && playerSpells.Count <= 0)
                        {
                            objectInfo.Add("no info found for this spec");
                            break;
                        }
                       
                        objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.LevName"));
                        foreach (Style style in styles)
                        {
                            objectInfo.Add(style.Level + ": " + style.Name);
                        }
                        foreach (Spell spell in spells)
                        {
                            objectInfo.Add(spell.Level + ": " + spell.Name);
                        }
                        break;
                    }
                #endregion
                #region Group
                case 12: // Item info to Group Chat
                    {
                        invItem = client.Player.Inventory.GetItem((eInventorySlot)objectID);
                        if (invItem == null) return;
                        string str = LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.Item", client.Player.Name, GetShortItemInfo(invItem, client));
                        if (client.Player.Group == null)
                        {
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.NoGroup"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }
                        client.Player.Group.SendMessageToGroupMembers(str, eChatType.CT_Group, eChatLoc.CL_ChatWindow);
                        return;
                    }
                #endregion
                #region Guild
                case 13: // Item info to Guild Chat
                    {
                        invItem = client.Player.Inventory.GetItem((eInventorySlot)objectID);
                        if (invItem == null) return;
                        string str = LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.GuildItem", client.Player.Name, GetShortItemInfo(invItem, client));
                        if (client.Player.Guild == null)
                        {
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.DontBelongGuild"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }
                        if (!client.Player.Guild.HasRank(client.Player, Guild.eRank.GcSpeak))
                        {
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.NoPermissionToSpeak"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }
                        foreach (GamePlayer ply in client.Player.Guild.GetListOfOnlineMembers())
                        {
                            if (!client.Player.Guild.HasRank(ply, Guild.eRank.GcHear)) continue;
                            ply.Out.SendMessage(str, eChatType.CT_Guild, eChatLoc.CL_ChatWindow);
                        }
                        return;
                    }
                #endregion
                #region ChatGroup
                case 15: // Item info to Chat group
                    {
                        invItem = client.Player.Inventory.GetItem((eInventorySlot)objectID);
                        if (invItem == null) return;

                        ChatGroup mychatgroup = (ChatGroup)client.Player.TempProperties.getProperty<object>(ChatGroup.CHATGROUP_PROPERTY, null);
                        if (mychatgroup == null)
                        {
                            client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.MustBeInChatGroup"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }
                        if (mychatgroup.Listen == true && (((bool)mychatgroup.Members[client.Player]) == false))
                        {
                            client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.OnlyModerator"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }
                        string str = LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.ChatItem", client.Player.Name, GetShortItemInfo(invItem, client));
                        foreach (GamePlayer ply in mychatgroup.Members.Keys)
                        {
                            ply.Out.SendMessage(str, eChatType.CT_Chat, eChatLoc.CL_ChatWindow);
                        }
                        return;
                    }
                #endregion
                #region Trainer Window
                //styles
                case 20:
                    {
                        Style style = SkillBase.GetStyleByID((int)objectID, client.Player.CharacterClass.ID);
                        if (style == null) return;

                        caption = style.Name;
                        WriteStyleInfo(objectInfo, style, client);
                        break;
                    }
                //spells
                case 22:
                    {
                        Spell spell = SkillBase.GetSpellByID((int)objectID);
                        if (spell == null) return;

                        caption = spell.Name;
                        WriteSpellInfo(objectInfo, spell, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells), client);
                        break;
                    }
                #endregion
                #region Repair
                case 100://repair
                    {
                        invItem = client.Player.Inventory.GetItem((eInventorySlot)objectID);
                        if (invItem != null)
                        {
                            client.Player.RepairItem(invItem);
                        }
                        else
                        {
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.VeryStrange"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        return;
                    }
                #endregion
                #region Self Craft
                case 101://selfcraft
                    {
                        invItem = client.Player.Inventory.GetItem((eInventorySlot)objectID);
                        if (invItem != null)
                        {
                            client.Player.OpenSelfCraft(invItem);
                        }
                        else
                        {
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.VeryStrange"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        return;
                    }
                #endregion
                #region Salvage
                case 102://salvage
                    {
                        invItem = client.Player.Inventory.GetItem((eInventorySlot)objectID);
                        if (invItem != null)
                        {
                            client.Player.SalvageItem(invItem);
                        }
                        else
                        {
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.VeryStrange"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        return;
                    }
                #endregion
                #region BattleGroup
                case 103: // Item info to battle group
                    {
                        invItem = client.Player.Inventory.GetItem((eInventorySlot)objectID);
                        if (invItem == null) return;

                        BattleGroup mybattlegroup = (BattleGroup)client.Player.TempProperties.getProperty<object>(BattleGroup.BATTLEGROUP_PROPERTY, null);
                        if (mybattlegroup == null)
                        {
                            client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.MustBeInBattleGroup"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }
                        if (mybattlegroup.Listen == true && (((bool)mybattlegroup.Members[client.Player]) == false))
                        {
                            client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.OnlyModerator"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }
                        string str = LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.ChatItem", client.Player.Name, GetShortItemInfo(invItem, client));
                        foreach (GamePlayer ply in mybattlegroup.Members.Keys)
                        {
                            ply.Out.SendMessage(str, eChatType.CT_Chat, eChatLoc.CL_ChatWindow);
                        }
                        return;
                    }
                #endregion

                #region v1.110+
                case 24://SpellsNew
                    if (client.CanSendTooltip(24, objectID))
			        client.Out.SendDelveInfo(DelveSpell(client, objectID));
					break;
				case 25://StylesNew
                    if (client.CanSendTooltip(25, objectID))
                    client.Out.SendDelveInfo(DelveStyle(client, objectID));
                    break;
				case 26://SongsNew
                    if (client.CanSendTooltip(26, objectID))
					client.Out.SendDelveInfo(DelveSong(client, objectID));
                    client.Out.SendDelveInfo(DelveSpell(client, objectID));
					break;
				case 27://RANew
                    if (client.CanSendTooltip(27, objectID))
                    client.Out.SendDelveInfo(DelveRealmAbility(client, objectID));
                    break;
				case 28://AbilityNew
                    if (client.CanSendTooltip(28, objectID))
			        client.Out.SendDelveInfo(DelveAbility(client, objectID));
			        break;
				#endregion
				#region ChampionAbilities delve from trainer window
                default:
                    {
                        //Extra Champ spells and Lines for Necromancers
                        if (client.Player.IsCharcterClass(eCharacterClass.Necromancer))
                        {

                            //int ctype = 0;
                            switch (objectType)
                            {
                                case 4:
                                    {
                                        objectType = 20;
                                    }
                                    break;
                                case 5:
                                    {
                                        objectType = 21;
                                    }
                                    break;
                                case 6:
                                    {
                                        objectType = 22;
                                    }
                                    break;
                                case 7:
                                    {
                                        objectType = 23;
                                    }
                                    break;


                                default: break;

                            }
                            //log.ErrorFormat("ctype = {0}", idLine);
                        }

                        // Try and handle all Champion lines, including custom lines
                        ChampSpec spec = ChampSpecMgr.GetAbilityFromIndex(objectType - 150, objectID / 256 + 1, objectID % 256 + 1);
                        if (spec != null)
                        {
                            Spell spell = SkillBase.GetSpellByID(spec.SpellID);
                            if (spell != null)
                            {
                                SpellLine spellLine = client.Player.GetChampionSpellLine();
                                if (spellLine != null)
                                {
                                    caption = spell.Name;
                                    WriteSpellInfo(objectInfo, spell, spellLine, client);
                                }
                                else
                                {
                                    objectInfo.Add("Champion spell line not found!");
                                }
                            }
                        }

                        break;
                    }
                #endregion
            }

            if (objectInfo.Count > 0)
            {
                client.Out.SendCustomTextWindow(caption, objectInfo);
            }
            else
            {
                //solange deaktiviert
               // log.WarnFormat("DetailDisplayHandler no info for objectID {0} of type {1}. Item: {2}, client: {3}", objectID, objectType, (item == null ? (invItem == null ? "null" : invItem.Id_nb) : item.Id_nb), client);
            }
        }

        public static void WriteStyleInfo(IList<string> objectInfo, Style style, GameClient client)
        {
            client.Player.DelveWeaponStyle(objectInfo, style);
        }

        /// <summary>
        /// Write a formatted description of a spell
        /// </summary>
        /// <param name="output"></param>
        /// <param name="spell"></param>
        /// <param name="spellLine"></param>
        /// <param name="client"></param>
        public void WriteSpellInfo(IList<string> output, Spell spell, SpellLine spellLine, GameClient client)
        {
            if (client == null || client.Player == null)
            {
                return;
            }

            // check to see if player class handles delve
            if (client.Player.DelveSpell(output, spell, spellLine))
            {
                return;
            }

            ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spell, spellLine);
            if (spellHandler == null)
            {
                output.Add(" ");
                output.Add("Spell type (" + spell.SpellType + ") is not implemented.");
            }
            else
            {
                output.AddRange(spellHandler.DelveInfo(client.Player));
                //Subspells
                if (spell.SubSpellID > 0)
                {
                    Spell s = SkillBase.GetSpellByID(spell.SubSpellID);
                    output.Add(" ");

                    ISpellHandler sh = ScriptMgr.CreateSpellHandler(client.Player, s, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
                    output.AddRange(sh.DelveInfo(client.Player));
                }
            }
            if (client.Account.PrivLevel > 1)
            {
                output.Add(" ");
                output.Add("--- Spell Technical Information ---");
                output.Add(" ");
                output.Add("Line: " + (spellHandler == null ? spellLine.KeyName : spellHandler.SpellLine.Name));
                output.Add("Type: " + spell.SpellType);
                output.Add(" ");
                output.Add("SpellID: " + spell.ID);
                output.Add("Icon: " + spell.Icon);
                output.Add("Type: " + spell.SpellType);
                output.Add("ClientEffect: " + spell.ClientEffect);
                output.Add("Target: " + spell.Target);
                output.Add("MoveCast: " + spell.MoveCast);
                output.Add("Uninterruptible: " + spell.Uninterruptible);
                output.Add("Value: " + spell.Value);
                output.Add("LifeDrainReturn: " + spell.LifeDrainReturn);
                if (spellHandler != null)
                    output.Add("HasPositiveEffect: " + spellHandler.HasPositiveEffect);
                output.Add("SharedTimerGroup: " + spell.SharedTimerGroup);
                output.Add("EffectGroup: " + spell.EffectGroup);
                output.Add("SpellGroup (for hybrid grouping): " + spell.Group);
                output.Add("Spell AllowCoexisting: " + spell.AllowCoexisting);
            }
        }

        public void WriteTechnicalInfo(IList<string> output, InventoryItem item)
        {
            WriteTechnicalInfo(output, item, item.Durability, item.Condition);
        }

        public void WriteTechnicalInfo(IList<string> output, InventoryItem item, int dur, int con)
        {
            output.Add(" ");
            output.Add("--- Item Technical Information ---");
            output.Add(" ");
            output.Add("Item Template: " + item.Id_nb);
            output.Add("         Name: " + item.Name);
            output.Add("        Level: " + item.Level);
            output.Add("       Object: " + GlobalConstants.ObjectTypeToName(item.Object_Type) + " (" + item.Object_Type + ")");
            output.Add("         Type: " + GlobalConstants.SlotToName(item.Item_Type) + " (" + item.Item_Type + ")");
            output.Add("    Extension: " + item.Extension);
            output.Add("        Model: " + item.Model);
            output.Add("        Color: " + item.Color);
            output.Add("       Emblem: " + item.Emblem);
            output.Add("       Effect: " + item.Effect);
            output.Add("  Value/Price: " + Money.GetShortString(item.Price));
            output.Add("       Weight: " + (item.Weight / 10.0f) + "lbs");
            output.Add("      Quality: " + item.Quality + "%");
            output.Add("   Durability: " + dur + "/" + item.MaxDurability);
            output.Add("    Condition: " + con + "/" + item.MaxCondition);
            output.Add("  RepearCount: " + item.RepearCount);
            output.Add("        Realm: " + item.Realm);
            output.Add("     Dropable: " + (item.IsDropable ? "yes" : "no"));
            output.Add("     Pickable: " + (item.IsPickable ? "yes" : "no"));
            output.Add("     Tradable: " + (item.IsTradable ? "yes" : "no"));
            output.Add("    AlwaysDUR: " + (item.IsNotLosingDur ? "yes" : "no"));
            output.Add("   Indestruct: " + (item.IsIndestructible ? "yes" : "no"));
            output.Add("    Stackable: " + (item.IsStackable ? "yes" : "no"));
            output.Add("  ProcSpellID: " + item.ProcSpellID);
            output.Add(" ProcSpellID1: " + item.ProcSpellID1);
            output.Add("      SpellID: " + item.SpellID + " (" + item.Charges + "/" + item.MaxCharges + ")");
            output.Add("     SpellID1: " + item.SpellID1 + " (" + item.Charges1 + "/" + item.MaxCharges1 + ")");
            output.Add("PoisonSpellID: " + item.PoisonSpellID + " (" + item.PoisonCharges + "/" + item.PoisonMaxCharges + ") ");
            output.Add(" PassiveSpell: " + item.PassiveSpell);

            if (GlobalConstants.IsWeapon(item.Object_Type))
            {
                output.Add("         Hand: " + GlobalConstants.ItemHandToName(item.Hand) + " (" + item.Hand + ")");
                output.Add("Damage/Second: " + (item.DPS_AF / 10.0f));
                output.Add("        Speed: " + (item.SPD_ABS / 10.0f));
                output.Add("  Damage type: " + GlobalConstants.WeaponDamageTypeToName(item.Type_Damage) + " (" + item.Type_Damage + ")");
                output.Add("        Bonus: " + item.Bonus);
            }
            else if (GlobalConstants.IsArmor(item.Object_Type))
            {
                output.Add("  Armorfactor: " + item.DPS_AF);
                output.Add("   Absorption: " + item.SPD_ABS);
                output.Add("        Bonus: " + item.Bonus);
            }
            else if (item.Object_Type == (int)eObjectType.Shield)
            {
                output.Add("Damage/Second: " + (item.DPS_AF / 10.0f));
                output.Add("        Speed: " + (item.SPD_ABS / 10.0f));
                output.Add("  Shield type: " + GlobalConstants.ShieldTypeToName(item.Type_Damage) + " (" + item.Type_Damage + ")");
                output.Add("        Bonus: " + item.Bonus);
            }
            else if (item.Object_Type == (int)eObjectType.Arrow || item.Object_Type == (int)eObjectType.Bolt)
            {
                output.Add(" Ammunition #: " + item.DPS_AF);
                output.Add("       Damage: " + GlobalConstants.AmmunitionTypeToDamageName(item.SPD_ABS));
                output.Add("        Range: " + GlobalConstants.AmmunitionTypeToRangeName(item.SPD_ABS));
                output.Add("     Accuracy: " + GlobalConstants.AmmunitionTypeToAccuracyName(item.SPD_ABS));
                output.Add("        Bonus: " + item.Bonus);
            }
            else if (item.Object_Type == (int)eObjectType.Instrument)
            {
                output.Add("   Instrument: " + GlobalConstants.InstrumentTypeToName(item.DPS_AF));
            }

            output.Add(" ");
            output.Add("            Flags: " + item.Flags);
            output.Add("        PackageID: " + item.PackageID);
        }

        protected string GetShortItemInfo(InventoryItem item, GameClient client)
        {
            // TODO: correct info format if anyone is interested...
            /*
                        [Guild] Player/Item:  "- [Kerubis' Scythe]: scythe No Sell. No Destroy."
                        [Guild] Player/Item: "- [Adroit Runed Duelist Rapier]: rapier 14.1 DPS 3.6 speed 89% qual 100% con (Thrust). Bonuses:  3% Spirit, 19 Dexterity, 2 Thrust, 6% Thrust, Tradeable.".
                        [Party] Player/Item: "- [Matterbender Belt]: belt 4% Energy, 8% Matter, 9 Constitution, 4% Slash, Tradeable."

                        multiline item...
                        [Guild] Player/Item: "- [shimmering Archmage Focus Briny Staff]: staff for Theurgist, Wizard, Sorcerer, Necromancer, Cabalist, Spiritmaster, Runemaster, Bonedancer, Theurgist, Wizard,
                        Sorcerer, Necromancer, Cabalist, Spiritmaster, Runemaster, Bonedancer, 16.5".
                        [Guild] Player/Item: "- DPS 3.0 speed 94% qual 100% con (Crush) (Two Handed). Bonuses:
                        22 Hits, 5% Energy, 2 All Casting, 50 lvls ALL focus 7% buff bonus, (10/10 char
                        ges) health regen Value: 8  Tradeable.".
             */

            string str = "- [" + item.Name + "]: " + GlobalConstants.ObjectTypeToName(item.Object_Type);
            var objectInfo = new List<string>();

            if ((item.Object_Type >= (int)eObjectType.GenericWeapon) && (item.Object_Type <= (int)eObjectType.MaulerStaff))
            {
                WriteMagicalBonuses(objectInfo, item, client, true);
                WriteClassicWeaponInfos(objectInfo, item, client);
            }
            if (item.Object_Type >= (int)eObjectType.Cloth && item.Object_Type <= (int)eObjectType.Scale)
            {
                WriteMagicalBonuses(objectInfo, item, client, true);
                WriteClassicArmorInfos(objectInfo, item, client);
            }
            if (item.Object_Type == (int)eObjectType.Shield)
            {
                WriteMagicalBonuses(objectInfo, item, client, true);
                WriteClassicShieldInfos(objectInfo, item, client);
            }
            if (item.Object_Type == (int)eObjectType.Magical ||
                item.Object_Type == (int)eObjectType.Instrument)
            {
                WriteMagicalBonuses(objectInfo, item, client, true);
            }
            if (item.IsCrafted)
            {
                objectInfo.Add(" ");//empty line
                objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.GetShortItemInfo.CrafterName", item.Creator));
            }
            if (item.Object_Type == (int)eObjectType.Poison)
            {
                WritePoisonInfo(objectInfo, item, client);
            }

            if (item.Object_Type == (int)eObjectType.Magical && item.Item_Type == 40) // potion
                WritePotionInfo(objectInfo, item, client);

            if (!item.IsDropable)
            {
                objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.GetShortItemInfo.NoDrop"));
                objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.GetShortItemInfo.NoSell"));
            }
            if (!item.IsPickable)
                objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.GetShortItemInfo.NoTrade"));

            for (int i = 0; i < objectInfo.Count; i++)
            {
                string s = objectInfo[i];
                str += " " + s;
            }

            return str;
        }

        /// <summary>
        /// Damage Modifiers:
        /// - X.X Base DPS
        /// - X.X Clamped DPS
        /// - XX Weapon Speed
        /// - XX% Quality
        /// - XX% Condition
        /// Damage Type: XXX
        ///
        /// Effective Damage:
        /// - X.X DPS
        /// </summary>
        public void WriteClassicWeaponInfos(IList<string> output, InventoryItem item, GameClient client)
        {

            double DPSRR5 = 0;
            if (client.Player.RealmLevel >= 40)
            {
                DPSRR5 = 0.3;
            }
            else
            {
                DPSRR5 = 0.0;
            }
            double itemDPS = item.DPS_AF / 10.0;
            double clampedDPS = Math.Min(itemDPS, 1.2 + 0.3 * client.Player.Level);
            double itemSPD = item.SPD_ABS / 10.0;
            double effectiveDPS = (clampedDPS + DPSRR5) * item.Quality / 100.0 * item.Condition / item.Template.MaxCondition;
           
            output.Add(" ");
            output.Add(" ");
            output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicWeaponInfos.DamageMod"));
            if (itemDPS != 0)
            {
                output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicWeaponInfos.BaseDPS", itemDPS.ToString("0.0")));
                output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicWeaponInfos.ClampDPS", clampedDPS.ToString("0.0")));
            }

            if (item.SPD_ABS >= 0)
            {
                output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicWeaponInfos.SPD", itemSPD.ToString("0.0")));
            }

            if (item.Quality != 0)
            {
                output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicWeaponInfos.Quality", item.Quality));
            }
            if (item.Condition != 0)
            {
                output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicWeaponInfos.Condition", item.ConditionPercent));
            }

            output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicWeaponInfos.DamageType",
                                                  (item.Type_Damage == 0 ? "None" : GlobalConstants.WeaponDamageTypeToName(item.Type_Damage))));
            output.Add(" ");

            output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicWeaponInfos.EffDamage"));
            if (itemDPS != 0)
            {
                output.Add("- " + effectiveDPS.ToString("0.0") + " DPS");
            }
        }

        public void WriteUsableClasses(IList<string> output, InventoryItem item, GameClient client)
        {
            WriteUsableClasses(output, item.Template, client);
        }
        public void WriteUsableClasses(IList<string> output, ItemTemplate item, GameClient client)
        {
            if (Util.IsEmpty(item.AllowedClasses, true))//itemTemplate
            {
               return;
            }
            log.Warn("gameinvenory 2");
            output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteUsableClasses.UsableBy"));

            foreach (string allowed in item.AllowedClasses.SplitCSV(true))
            {
                int classID = -1;
                if (int.TryParse(allowed, out classID))
                    output.Add("- " + ((eCharacterClass)classID).ToString());
                else log.Error(item.Id_nb + " has an invalid entry for allowed classes '" + allowed + "'");
            }
        }
       

        /// <summary>
        /// Damage Modifiers (when used with shield styles):
        /// - X.X Base DPS
        /// - X.X Clamped DPS
        /// - XX Shield Speed
        /// </summary>
        public void WriteClassicShieldInfos(IList<string> output, InventoryItem item, GameClient client)
        {
            WriteClassicShieldInfos(output, item.Template, client);
        }
        public void WriteClassicShieldInfos(IList<string> output, ItemTemplate item, GameClient client)
        {
            
            double itemDPS = item.DPS_AF / 10.0;
            double clampedDPS = Math.Min(itemDPS, 1.2 + 0.3 * client.Player.Level);
            double itemSPD = item.SPD_ABS / 10.0;
            
            output.Add(" ");
            output.Add(" ");
            output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicShieldInfos.DamageMod"));
            if (itemDPS != 0)
            {
                output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicShieldInfos.BaseDPS", itemDPS.ToString("0.0")));
                output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicShieldInfos.ClampDPS", clampedDPS.ToString("0.0")));
            }
            if (item.SPD_ABS >= 0)
            {
                output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicShieldInfos.SPD", itemSPD.ToString("0.0")));
            }

            output.Add(" ");

            switch (item.Type_Damage)
            {
                case 1: output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicShieldInfos.Small")); break;
                case 2: output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicShieldInfos.Medium")); break;
                case 3: output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicShieldInfos.Large")); break;
            }
        }

        /// <summary>
        /// Armor Modifiers:
        /// - X.X Base Factor
        /// - X.X Clamped Factor
        /// - XX% Absorption
        /// - XX% Quality
        /// - XX% Condition
        /// Damage Type: XXX
        ///
        /// Effective Armor:
        /// - X.X Factor
        /// </summary>
        public void WriteClassicArmorInfos(IList<string> output, InventoryItem item, GameClient client)
        {
            int AFRR5 = 0;
            if (client.Player.RealmLevel >= 40)
            {
                AFRR5 = 1;
            }
            else
            {
                AFRR5 = 0;
            }
            output.Add(" ");
            output.Add(" ");
            output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicArmorInfos.ArmorMod"));
            if (item.DPS_AF != 0)
            {
                output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicArmorInfos.BaseFactor", item.DPS_AF));
            }
            double AF = 0;
            if (item.DPS_AF != 0)
            {
                int afCap = client.Player.Level + AFRR5;
                if (item.Object_Type != (int)eObjectType.Cloth)
                {
                    afCap *= 2;
                }

                AF = Math.Min(afCap, item.DPS_AF);

                output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicArmorInfos.ClampFact", (int)AF));
            }
            if (item.SPD_ABS >= 0)
            {
                output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicArmorInfos.Absorption", item.SPD_ABS));
            }
            if (item.Quality != 0)
            {
                output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicArmorInfos.Quality", item.Quality));
            }
            if (item.Condition != 0)
            {
                output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicArmorInfos.Condition", 100 /*item.ConditionPercent*/));
            }
            output.Add(" ");

            output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicArmorInfos.EffArmor"));
            double EAF = 0;
            if (item.DPS_AF != 0)
            {
                EAF = AF * item.Quality / 100.0 * item.Condition / item.MaxCondition * (1 + item.SPD_ABS / 100.0);
                output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicArmorInfos.Factor", (int)EAF));
            }

        }

        public void WriteMagicalBonuses(IList<string> output, ItemTemplate item, GameClient client, bool shortInfo)
        {
            WriteMagicalBonuses(output, GameInventoryItem.Create<ItemTemplate>(item), client, shortInfo);
        }

        public void WriteMagicalBonuses(IList<string> output, InventoryItem item, GameClient client, bool shortInfo)
        {
            int oldCount = output.Count;

            //Player Stats display
            WriteBonusLine(output, client, item.Bonus1Type, item.Bonus1);
            WriteBonusLine(output, client, item.Bonus2Type, item.Bonus2);
            WriteBonusLine(output, client, item.Bonus3Type, item.Bonus3);
            WriteBonusLine(output, client, item.Bonus4Type, item.Bonus4);
            WriteBonusLine(output, client, item.Bonus5Type, item.Bonus5);
            WriteBonusLine(output, client, item.Bonus6Type, item.Bonus6);
            WriteBonusLine(output, client, item.Bonus7Type, item.Bonus7);
            WriteBonusLine(output, client, item.Bonus8Type, item.Bonus8);
            WriteBonusLine(output, client, item.Bonus9Type, item.Bonus9);
            WriteBonusLine(output, client, item.Bonus10Type, item.Bonus10);
            WriteBonusLine(output, client, item.ExtraBonusType, item.ExtraBonus);

            //GM Stats display
            WriteBonusLineGM(output, client, item.Bonus1Type, item.Bonus1);
            WriteBonusLineGM(output, client, item.Bonus2Type, item.Bonus2);
            WriteBonusLineGM(output, client, item.Bonus3Type, item.Bonus3);
            WriteBonusLineGM(output, client, item.Bonus4Type, item.Bonus4);
            WriteBonusLineGM(output, client, item.Bonus5Type, item.Bonus5);
            WriteBonusLineGM(output, client, item.Bonus6Type, item.Bonus6);
            WriteBonusLineGM(output, client, item.Bonus7Type, item.Bonus7);
            WriteBonusLineGM(output, client, item.Bonus8Type, item.Bonus8);
            WriteBonusLineGM(output, client, item.Bonus9Type, item.Bonus9);
            WriteBonusLineGM(output, client, item.Bonus10Type, item.Bonus10);
            WriteBonusLineGM(output, client, item.ExtraBonusType, item.ExtraBonus);

           
            if (output.Count > oldCount)
            {
                output.Add(" ");
                output.Insert(oldCount, LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.MagicBonus"));
                output.Insert(oldCount, " ");
            }

            oldCount = output.Count;


            if (!shortInfo)
            {
                if (item.ProcSpellID != 0 || item.ProcSpellID1 != 0 || item.SpellID != 0 || item.SpellID1 != 0 || item.PassiveSpell != 0)
                {
                    int requiredLevel = item.LevelRequirement > 0 ? item.LevelRequirement : Math.Min(50, item.Level);
                    output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.LevelRequired2", requiredLevel));
                    output.Add(" ");
                }
            }

            //Player Stats display
            WriteItemExtraBonusLine(output, client, item.Bonus1Type, item.Bonus1);
            WriteItemExtraBonusLine(output, client, item.Bonus2Type, item.Bonus2);
            WriteItemExtraBonusLine(output, client, item.Bonus3Type, item.Bonus3);
            WriteItemExtraBonusLine(output, client, item.Bonus4Type, item.Bonus4);
            WriteItemExtraBonusLine(output, client, item.Bonus5Type, item.Bonus5);
            WriteItemExtraBonusLine(output, client, item.Bonus6Type, item.Bonus6);
            WriteItemExtraBonusLine(output, client, item.Bonus7Type, item.Bonus7);
            WriteItemExtraBonusLine(output, client, item.Bonus8Type, item.Bonus8);
            WriteItemExtraBonusLine(output, client, item.Bonus9Type, item.Bonus9);
            WriteItemExtraBonusLine(output, client, item.Bonus10Type, item.Bonus10);
            WriteItemExtraBonusLine(output, client, item.ExtraBonusType, item.ExtraBonus);

            if (output.Count > oldCount)
            {
               output.Add(" ");
              //  output.Insert(oldCount, LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.ItemBonus"));
              //  output.Insert(oldCount, " ");
            }

            oldCount = output.Count;


            WriteFocusLine(output, item.Bonus1Type, item.Bonus1, client);
            WriteFocusLine(output, item.Bonus1Type, item.Bonus1, client);
            WriteFocusLine(output, item.Bonus2Type, item.Bonus2, client);
            WriteFocusLine(output, item.Bonus3Type, item.Bonus3, client);
            WriteFocusLine(output, item.Bonus4Type, item.Bonus4, client);
            WriteFocusLine(output, item.Bonus5Type, item.Bonus5, client);
            WriteFocusLine(output, item.Bonus6Type, item.Bonus6, client);
            WriteFocusLine(output, item.Bonus7Type, item.Bonus7, client);
            WriteFocusLine(output, item.Bonus8Type, item.Bonus8, client);
            WriteFocusLine(output, item.Bonus9Type, item.Bonus9, client);
            WriteFocusLine(output, item.Bonus10Type, item.Bonus10, client);
            WriteFocusLine(output, item.ExtraBonusType, item.ExtraBonus, client);

            if (output.Count > oldCount)
            {
                //output.Add(" ");
                output.Insert(oldCount, LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.FocusBonus"));
                output.Insert(oldCount, " ");
            }

            if (!shortInfo)
            {
                if (item.Object_Type == (int)eObjectType.Magical && item.Item_Type == (int)eInventorySlot.FirstBackpack) // potion
                {
                    // let WritePotion handle the rest of the display
                    return;
                }


                #region Proc1
                if (item.ProcSpellID != 0)
                {
                    //defensiver oder offensiver proc ?
                    SpellLine lineType = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
                    if (lineType != null)
                    {
                        procType = SkillBase.FindSpell(item.ProcSpellID, lineType);
                    }
                    string spellNote = String.Empty;
                    output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.MagicAbility"));
                    if (GlobalConstants.IsWeapon(item.Object_Type))
                    {
                        //defensiver oder offensiver proc ?
                        if (procType != null && procType.SpellType == "DefensiveProc")
                            spellNote = LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.StrikeItem");
                        else
                            spellNote = LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.StrikeEnemy");
                    }
                    else if (GlobalConstants.IsArmor(item.Object_Type))
                    {
                        spellNote = LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.StrikeArmor");
                    }

                    SpellLine line = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
                    if (line != null)
                    {
                        Spell procSpell = SkillBase.FindSpell(item.ProcSpellID, line);

                        if (procSpell != null)
                        {
                            ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, procSpell, line);
                            if (spellHandler != null)
                            {
                                output.AddRange(spellHandler.DelveInfo(client.Player));
                                output.Add(" ");
                            }
                            else
                            {
                                output.Add("-" + procSpell.Name + " (Spell Handler Not Implemented)");
                            }

                            output.Add(spellNote);
                        }
                        else
                        {
                            output.Add("- Spell Not Found: " + item.ProcSpellID);
                        }
                    }
                    else
                    {
                        output.Add("- Item_Effects Spell Line Missing");
                    }

                    output.Add(" ");
                }
                #endregion
                #region Proc2
                if (item.ProcSpellID1 != 0)
                {
                    //defensiver oder offensiver proc ?
                    SpellLine lineType1 = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
                    if (lineType1 != null)
                    {
                        procType1 = SkillBase.FindSpell(item.ProcSpellID1, lineType1);
                    }

                    string spellNote = "";
                    output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.MagicAbility"));
                    if (GlobalConstants.IsWeapon(item.Object_Type))
                    {
                        //defensiver oder offensiver proc ?
                        if (procType != null && procType.SpellType == "DefensiveProc")
                            spellNote = LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.StrikeItem");
                        else
                            spellNote = LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.StrikeEnemy");
                    }
                    else if (GlobalConstants.IsArmor(item.Object_Type))
                    {
                        spellNote = LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.StrikeArmor");
                    }

                    SpellLine line = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
                    if (line != null)
                    {
                        Spell procSpell = SkillBase.FindSpell(item.ProcSpellID1, line);

                        if (procSpell != null)
                        {
                            ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, procSpell, line);
                            if (spellHandler != null)
                            {
                                output.AddRange(spellHandler.DelveInfo(client.Player));
                                output.Add(" ");
                            }
                            else
                            {
                                output.Add("-" + procSpell.Name + " (Spell Handler Not Implemented)");
                            }

                            output.Add(spellNote);
                        }
                        else
                        {
                            output.Add("- Spell Not Found: " + item.ProcSpellID1);
                        }
                    }
                    else
                    {
                        output.Add("- Item_Effects Spell Line Missing");
                    }

                    output.Add(" ");
                }
                #endregion
                #region Charge1
                if (item.SpellID != 0)
                {
                    SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
                    if (chargeEffectsLine != null)
                    {
                        Spell spell = SkillBase.FindSpell(item.SpellID, chargeEffectsLine);
                        if (spell != null)
                        {
                            ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spell, chargeEffectsLine);

                            if (spellHandler != null)
                            {
                                if (item.MaxCharges > 0)
                                {
                                    output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.ChargedMagic"));
                                    output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.Charges", item.Charges));
                                    output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.MaxCharges", item.MaxCharges));
                                    output.Add(" ");
                                    output.Add(" ");
                                }

                                output.AddRange(spellHandler.DelveInfo(client.Player));
                                output.Add(" ");
                                output.Add("- This spell is cast when the item is used.");
                            }
                            else
                            {
                                output.Add("- Item_Effects Spell Line Missing");
                            }
                        }
                        else
                        {
                            output.Add("- Spell Not Found: " + item.SpellID);
                        }
                    }

                    output.Add(" ");
                }
                #endregion
                #region Charge2
                if (item.SpellID1 != 0)
                {
                    SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
                    if (chargeEffectsLine != null)
                    {
                        Spell spell = SkillBase.FindSpell(item.SpellID1, chargeEffectsLine);
                        if (spell != null)
                        {
                            ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spell, chargeEffectsLine);

                            if (spellHandler != null)
                            {
                                if (item.MaxCharges > 0)
                                {
                                    output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.ChargedMagic"));
                                    output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.Charges", item.Charges));
                                    output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.MaxCharges", item.MaxCharges));
                                    output.Add(" ");
                                }

                                output.AddRange(spellHandler.DelveInfo(client.Player));
                                output.Add(" ");
                                output.Add("- This spell is cast when the item is used.");
                            }
                            else
                            {
                                output.Add("- Item_Effects Spell Line Missing");
                            }
                        }
                        else
                        {
                            output.Add("- Spell Not Found: " + item.SpellID1);
                        }
                    }

                    output.Add(" ");
                }
                #endregion
                #region PassiveSpell
 	  	                                 if (item.PassiveSpell != 0)
 	  	                                 {
 	  	                                         SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
 	  	                                         if (chargeEffectsLine != null)
 	  	                                         {
 	  	                                                 Spell spell = SkillBase.FindSpell(item.PassiveSpell, chargeEffectsLine);
                                                         if (spell != null)
                                                         {
                                                             ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spell, chargeEffectsLine);

                                                             if (spellHandler != null)
                                                             {
                                                                 output.AddRange(spellHandler.DelveInfo(client.Player));
                                                                 output.Add(" ");
                                                                 output.Add("- This spell is cast when the item is used.");
                                                             }
                                                             else
                                                             {
                                                                 output.Add("- Item_Effects Spell Line Missing");
                                                             }
                                                         }
                                                         else
                                                         {
                                                             output.Add("- Spell Not Found: " + item.SpellID1);
                                                         }
 	  	                                         }
 	  	 
 	  	                                         output.Add(" ");
 	  	                                 }
 	  	                                 #endregion PassiveSpell
                
                #region Poison
                if (item.PoisonSpellID != 0)
                {
                    if (GlobalConstants.IsWeapon(item.Object_Type))// Poisoned Weapon
                    {
                        SpellLine poisonLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mundane_Poisons);
                        if (poisonLine != null)
                        {
                            List<Spell> spells = SkillBase.GetSpellList(poisonLine.KeyName);
                            foreach (Spell spl in spells)
                            {
                                if (spl.ID == item.PoisonSpellID)
                                {
                                    output.Add(" ");
                                    output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.LevelRequired"));
                                    output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.Level", spl.Level));
                                    output.Add(" ");
                                    output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.ChargedMagic"));
                                    output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.Charges", item.PoisonCharges));
                                    output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.MaxCharges", item.PoisonMaxCharges));
                                    output.Add(" ");

                                    ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spl, poisonLine);
                                    if (spellHandler != null)
                                    {
                                        output.AddRange(spellHandler.DelveInfo(client.Player));
                                        output.Add(" ");
                                    }
                                    else
                                    {
                                        output.Add("-" + spl.Name + "(Not implemented yet)");
                                    }
                                    output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.StrikeEnemy"));
                                    return;
                                }
                            }
                        }
                    }

                    SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
                    if (chargeEffectsLine != null)
                    {
                        List<Spell> spells = SkillBase.GetSpellList(chargeEffectsLine.KeyName);
                        foreach (Spell spl in spells)
                        {
                            if (spl.ID == item.SpellID)
                            {
                                output.Add(" ");
                                output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.LevelRequired"));
                                output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.Level", spl.Level));
                                output.Add(" ");
                                if (item.MaxCharges > 0)
                                {
                                    output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.ChargedMagic"));
                                    output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.Charges", item.Charges));
                                    output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.MaxCharges", item.MaxCharges));
                                }
                                else
                                {
                                    output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.MagicAbility"));
                                }
                                output.Add(" ");

                                ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spl, chargeEffectsLine);
                                if (spellHandler != null)
                                {
                                    output.AddRange(spellHandler.DelveInfo(client.Player));
                                    output.Add(" ");
                                }
                                else
                                {
                                    output.Add("-" + spl.Name + "(Not implemented yet)");
                                }
                                output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.UsedItem"));
                                output.Add(" ");
                                if (spl.RecastDelay > 0)
                                    output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.UseItem1", Util.FormatTime(spl.RecastDelay / 1000)));
                                else
                                    output.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WriteMagicalBonuses.UseItem2"));
                                long lastChargedItemUseTick = client.Player.TempProperties.getProperty<long>(GamePlayer.LAST_CHARGED_ITEM_USE_TICK);
                                long changeTime = client.Player.CurrentRegion.Time - lastChargedItemUseTick;
                                long recastDelay = (spl.RecastDelay > 0) ? spl.RecastDelay : 60000 * 3;
                                if (changeTime < recastDelay) //3 minutes reuse timer
                                    output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.UseItem3", Util.FormatTime((recastDelay - changeTime) / 1000)));
                                return;
                            }
                        }
                    }
                }
                #endregion
            }
        }

        //GM Stats display
        protected virtual void WriteBonusLineGM(IList<string> list, GameClient client, int bonusCat, int bonusValue)
        {
            if (client.Account.PrivLevel > (int)ePrivLevel.Player)
            {
                if (bonusCat != 0 && bonusValue != 0 && !SkillBase.CheckPropertyType((eProperty)bonusCat, ePropertyType.Focus))
                {

                    if (IsToABonus((eProperty)bonusCat) || IsPvEBonus((eProperty)bonusCat))
                    {
                        // Evade: {0}% (PvE Only)
                        list.Add(string.Format(SkillBase.GetPropertyName((eProperty)bonusCat, client.Account.Language), bonusValue));
                    }
                    else

                    {
                        //- Axe: 5 pts
                        //- Strength: 15 pts
                        //- Constitution: 15 pts
                        //- Hits: 40 pts
                        //- Fatigue: 8 pts
                        //- Heat: 7%
                        //Bonus to casting speed: 2%
                        //Bonus to armor factor (AF): 18
                        //Power pool: 6 %
                        list.Add(string.Format(
                            "- {0}: {1}{2}",
                            SkillBase.GetPropertyName((eProperty)bonusCat, client.Account.Language),
                            bonusValue.ToString("+0 ;-0 ;0 "), //Eden
                             ((bonusCat == (int)eProperty.PowerPool)
                             || (bonusCat >= (int)eProperty.Resist_First && bonusCat <= (int)eProperty.Resist_Last)
                             || (bonusCat >= (int)eProperty.ResCapBonus_First && bonusCat <= (int)eProperty.ResCapBonus_Last)
                             || bonusCat == (int)eProperty.Conversion
                             || bonusCat == (int)eProperty.ExtraHP
                             || bonusCat == (int)eProperty.RealmPoints
                             || bonusCat == (int)eProperty.StyleAbsorb
                             || bonusCat == (int)eProperty.ArcaneSyphon
                             || bonusCat == (int)eProperty.BountyPoints
                             || bonusCat == (int)eProperty.XpPoints)
                             || bonusCat == (int)eProperty.PowerPoolCapBonus
                             || bonusCat == (int)eProperty.SpellRange
                             || bonusCat == (int)eProperty.SpellDamage
                             || bonusCat == (int)eProperty.SpellDuration
                             || bonusCat == (int)eProperty.CastingSpeed
                             || bonusCat == (int)eProperty.ResistPierce
                             || bonusCat == (int)eProperty.StyleDamage
                             || bonusCat == (int)eProperty.MeleeDamage
                             || bonusCat == (int)eProperty.EvadeChance
                             || bonusCat == (int)eProperty.BlockChance
                             || bonusCat == (int)eProperty.ParryChance
                             || bonusCat == (int)eProperty.StyleCostReduction
                             || bonusCat == (int)eProperty.ArcherySpeed
                             || bonusCat == (int)eProperty.ArcheryRange
                             || bonusCat == (int)eProperty.BlockChance
                             || bonusCat == (int)eProperty.MeleeSpeed
                             || bonusCat == (int)eProperty.DeathExpLoss
                             || bonusCat == (int)eProperty.ReactionaryStyleDamage
                             || bonusCat == (int)eProperty.SpeedDecreaseDurationReduction
                             || bonusCat == (int)eProperty.SpellFumbleChance
                             || bonusCat == (int)eProperty.NegativeReduction
                             || bonusCat == (int)eProperty.MesmerizeDurationReduction
                             || bonusCat == (int)eProperty.BuffEffectiveness
                             || bonusCat == (int)eProperty.HealingEffectiveness
                            ? ((bonusCat == (int)eProperty.PowerPool) ? LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteBonusLine.PowerPool") : "%")
                            : LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteBonusLine.Points")
                        ));

                    }
                }
            }
        }

        //Player Stats display stats
        protected virtual void WriteBonusLine(IList<string> list, GameClient client, int bonusCat, int bonusValue)
        {
            if (client.Account.PrivLevel == (int)ePrivLevel.Player)
            {
                if (bonusCat != 0 && bonusValue != 0 && IsPvEBonus((eProperty)bonusCat) == false && IsToABonus((eProperty)bonusCat) == false && !SkillBase.CheckPropertyType((eProperty)bonusCat, ePropertyType.Focus) && (bonusCat >= (int)eProperty.StatCapBonus_First && bonusCat <= (int)eProperty.StatCapBonus_Last) == false && (bonusCat >= (int)eProperty.ResCapBonus_First && bonusCat <= (int)eProperty.ResCapBonus_Last) == false && (bonusCat >= (int)eProperty.ToABonus_First && bonusCat <= (int)eProperty.ToABonus_Last) == false && IsExtaBonus((eProperty)bonusCat) == false)
                {
                    /*
                    if (IsPvEBonus((eProperty)bonusCat))
                    {
                        // Evade: {0}% (PvE Only)
                        list.Add(string.Format(SkillBase.GetPropertyName((eProperty)bonusCat, client.Account.Language), bonusValue));
                    }
                    else
                    */
                    {
                        //- Axe: 5 pts
                        //- Strength: 15 pts
                        //- Constitution: 15 pts
                        //- Hits: 40 pts
                        //- Fatigue: 8 pts
                        //- Heat: 7%
                        //Bonus to casting speed: 2%
                        //Bonus to armor factor (AF): 18
                        //Power pool: 6 %
                        list.Add(string.Format(
                            "- {0}: {1}{2}",
                            SkillBase.GetPropertyName((eProperty)bonusCat, client.Account.Language),
                            bonusValue.ToString("+0 ;-0 ;0 "), //Eden
                             ((bonusCat == (int)eProperty.PowerPool)
                             || (bonusCat >= (int)eProperty.Resist_First && bonusCat <= (int)eProperty.Resist_Last))
                            // || (bonusCat >= (int)eProperty.ResCapBonus_First && bonusCat <= (int)eProperty.ResCapBonus_Last)

                            ? ((bonusCat == (int)eProperty.PowerPool) ? LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteBonusLine.PowerPool") : "%")
                            : LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteBonusLine.Points")
                        ));

                    }
                }
            }
        }

        //Player Stats display ToA bonis and Caps
        protected virtual void WriteItemExtraBonusLine(IList<string> list, GameClient client, int bonusCat, int bonusValue)
        {
            if (client.Account.PrivLevel == (int)ePrivLevel.Player)
            {
                if (bonusCat != 0 && bonusValue != 0 && !SkillBase.CheckPropertyType((eProperty)bonusCat, ePropertyType.Focus) && ((bonusCat >= (int)eProperty.StatCapBonus_First && bonusCat <= (int)eProperty.StatCapBonus_Last) || (bonusCat >= (int)eProperty.ResCapBonus_First && bonusCat <= (int)eProperty.ResCapBonus_Last)
                    || (bonusCat >= (int)eProperty.ToABonus_First && bonusCat <= (int)eProperty.ToABonus_Last) || IsPvEBonus((eProperty)bonusCat) == true || IsToABonus((eProperty)bonusCat) == true || IsExtaBonus((eProperty)bonusCat) == true))
                {

                    if (IsToABonus((eProperty)bonusCat) || IsPvEBonus((eProperty)bonusCat))
                    {
                        // Evade: {0}% (PvE Only)
                        list.Add(string.Format(SkillBase.GetPropertyName((eProperty)bonusCat, client.Account.Language), bonusValue));
                    }
                    else

                    {

                        //- Axe: 5 pts
                        //- Strength: 15 pts
                        //- Constitution: 15 pts
                        //- Hits: 40 pts
                        //- Fatigue: 8 pts
                        //- Heat: 7%
                        //Bonus to casting speed: 2%
                        //Bonus to armor factor (AF): 18
                        //Power pool: 6 %
                        list.Add(string.Format(
                            "- {0}: {1}{2}",
                            SkillBase.GetPropertyName((eProperty)bonusCat, client.Account.Language),
                            bonusValue.ToString("+0 ;-0 ;0 "), //Eden
                            ((bonusCat >= (int)eProperty.ResCapBonus_First && bonusCat <= (int)eProperty.ResCapBonus_Last)
                            || (bonusCat >= (int)eProperty.ToABonus_First && bonusCat <= (int)eProperty.ToABonus_Last)
                             || bonusCat == (int)eProperty.Conversion
                             || bonusCat == (int)eProperty.ExtraHP
                             || bonusCat == (int)eProperty.RealmPoints
                             || bonusCat == (int)eProperty.StyleAbsorb
                             || bonusCat == (int)eProperty.ArcaneSyphon
                             || bonusCat == (int)eProperty.BountyPoints
                             || bonusCat == (int)eProperty.XpPoints)
                             || bonusCat == (int)eProperty.PowerPoolCapBonus
                             || bonusCat == (int)eProperty.SpellRange
                             || bonusCat == (int)eProperty.SpellDamage
                             || bonusCat == (int)eProperty.SpellDuration
                             || bonusCat == (int)eProperty.CastingSpeed
                             || bonusCat == (int)eProperty.ResistPierce
                             || bonusCat == (int)eProperty.StyleDamage
                             || bonusCat == (int)eProperty.MeleeDamage
                             || bonusCat == (int)eProperty.EvadeChance
                             || bonusCat == (int)eProperty.BlockChance
                             || bonusCat == (int)eProperty.ParryChance
                             || bonusCat == (int)eProperty.StyleCostReduction
                             || bonusCat == (int)eProperty.ArcherySpeed
                             || bonusCat == (int)eProperty.ArcheryRange
                             || bonusCat == (int)eProperty.BlockChance
                             || bonusCat == (int)eProperty.MeleeSpeed
                             || bonusCat == (int)eProperty.DeathExpLoss
                             || bonusCat == (int)eProperty.ReactionaryStyleDamage
                             || bonusCat == (int)eProperty.SpeedDecreaseDurationReduction
                             || bonusCat == (int)eProperty.SpellFumbleChance
                             || bonusCat == (int)eProperty.NegativeReduction
                             || bonusCat == (int)eProperty.MesmerizeDurationReduction
                             || bonusCat == (int)eProperty.BuffEffectiveness
                             || bonusCat == (int)eProperty.HealingEffectiveness


                        ? ((bonusCat >= (int)eProperty.ColdResCapBonus && bonusCat <= (int)eProperty.Resist_Last) ? LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteBonusLine.PowerPool") : "%")
                        : LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteBonusLine.Points")
                        ));
                    }
                }
            }
        }

        protected virtual bool IsToABonus(eProperty property)
        {
            switch (property)
            {
                case eProperty.ResistPierce:
                case eProperty.ArcaneSyphon:
                case eProperty.Conversion:
                    return true;

                default:
                    return false;
            }
        }

        protected virtual bool IsExtaBonus(eProperty property)
        {
            switch (property)
            {
                case eProperty.ResistPierce:
                case eProperty.ArcaneSyphon:
                case eProperty.ArmorFactor:
                case eProperty.Conversion:
                case eProperty.ExtraHP:
                case eProperty.RealmPoints:
                case eProperty.StyleAbsorb:
                case eProperty.BountyPoints:
                case eProperty.XpPoints:
                case eProperty.PowerPoolCapBonus:
                case eProperty.SpellRange:
                case eProperty.SpellDamage:
                case eProperty.SpellDuration:
                case eProperty.CastingSpeed:
                case eProperty.StyleDamage:
                case eProperty.MeleeDamage:
                case eProperty.EvadeChance:
                case eProperty.BlockChance:
                case eProperty.ParryChance:
                case eProperty.StyleCostReduction:
                case eProperty.ArcherySpeed:
                case eProperty.ArcheryRange:
                case eProperty.MeleeSpeed:
                case eProperty.DeathExpLoss:
                case eProperty.ReactionaryStyleDamage:
                case eProperty.SpeedDecreaseDurationReduction:
                case eProperty.SpellFumbleChance:
                case eProperty.NegativeReduction:
                case eProperty.MesmerizeDurationReduction:
                case eProperty.BuffEffectiveness:
                case eProperty.HealingEffectiveness:

                    return true;

                default:
                    return false;
            }
        }

        protected bool IsPvEBonus(eProperty property)
        {
            switch (property)
            {
                case eProperty.BlockChance:
                case eProperty.ParryChance:
                case eProperty.EvadeChance:
                case eProperty.DefensiveBonus:
                case eProperty.BladeturnReinforcement:
                case eProperty.NegativeReduction:
                case eProperty.PieceAblative:
                case eProperty.ReactionaryStyleDamage:
                case eProperty.SpellPowerCost:
                case eProperty.StyleCostReduction:
                case eProperty.ToHitBonus:
                 return true;

                default:
                    return false;
            }
        }

        protected void WriteFocusLine(IList<string> list, int focusCat, int focusLevel, GameClient client)
        {
            if (SkillBase.CheckPropertyType((eProperty)focusCat, ePropertyType.Focus))
            {
                //- Body Magic: 4 lvls
                list.Add(string.Format("- {0}: {1} lvls", SkillBase.GetPropertyName((eProperty)focusCat, client.Account.Language), focusLevel));
            }
        }

        protected void WriteHorseInfo(IList<string> list, InventoryItem item, GameClient client, string horseName)
        {
            WriteHorseInfo(list, item.Template, client, horseName);
        }
        protected void WriteHorseInfo(IList<string> list, ItemTemplate item, GameClient client, string horseName)
        {
            list.Add(" ");
            list.Add(" ");
            if (item.Level <= 35)
            {
                list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.BasicHorse"));
                list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.Speed1"));
                list.Add(" ");
                list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.MountWindow"));
                list.Add(" ");
                list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.Quickbar"));
                list.Add(" ");
                list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.RvRZzones"));
            }
            else
            {
                list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.AdvancedHorse"));
                list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.Speed2"));
                list.Add(" ");
                list.Add(" ");
                list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.Summon"));
                list.Add(" ");
                list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.Name", ((horseName == null || horseName == "") ? "None" : horseName)));
                list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.NameMount"));
            }
            list.Add(" ");
            list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.Armor", (item.DPS_AF == 0 ? "None" : item.DPS_AF.ToString())));
            list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.Barding"));
            list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.Food"));
        }

        protected void WritePoisonInfo(IList<string> list, ItemTemplate item, GameClient client)
        {
            WritePoisonInfo(list, GameInventoryItem.Create<ItemTemplate>(item), client);
        }

        protected void WritePoisonInfo(IList<string> list, InventoryItem item, GameClient client)
        {
            if (item.PoisonSpellID != 0)
            {
                SpellLine poisonLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mundane_Poisons);
                if (poisonLine != null)
                {
                    List<Spell> spells = SkillBase.GetSpellList(poisonLine.KeyName);

                    foreach (Spell spl in spells)
                    {
                        if (spl.ID == item.PoisonSpellID)
                        {
                            list.Add(" ");
                            list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePoisonInfo.LevelRequired"));
                            list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePoisonInfo.Level", spl.Level));
                            list.Add(" ");
                            list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePoisonInfo.ProcAbility"));
                            list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePoisonInfo.Charges", item.PoisonCharges));
                            list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePoisonInfo.MaxCharges", item.PoisonMaxCharges));
                            list.Add(" ");

                            ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spl, poisonLine);
                            if (spellHandler != null)
                            {
                                list.AddRange(spellHandler.DelveInfo(client.Player));
                            }
                            else
                            {
                                list.Add("-" + spl.Name + " (Not implemented yet)");
                            }
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Nidel: Write potions infos. Spell's infos include
        /// </summary>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <param name="client"></param>
        private static void WritePotionInfo(IList<string> list, ItemTemplate item, GameClient client)
        {
            WritePotionInfo(list, GameInventoryItem.Create<ItemTemplate>(item), client);
        }

        private static void WritePotionInfo(IList<string> list, InventoryItem item, GameClient client)
        {
            if (item.SpellID != 0)
            {
                SpellLine potionLine = SkillBase.GetSpellLine(GlobalSpellsLines.Potions_Effects);
                if (potionLine != null)
                {
                    List<Spell> spells = SkillBase.GetSpellList(potionLine.KeyName);

                    foreach (Spell spl in spells)
                    {
                        if (spl.ID == item.SpellID)
                        {
                            list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePotionInfo.ChargedMagic"));
                            list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePotionInfo.Charges", item.Charges));
                            list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePotionInfo.MaxCharges", item.MaxCharges));
                            list.Add(" ");
                            WritePotionSpellsInfos(list, client, spl, potionLine);
                            list.Add(" ");
                            long nextPotionAvailTime = client.Player.TempProperties.getProperty<long>("LastPotionItemUsedTick_Type" + spl.SharedTimerGroup);
                            // Satyr Update: Individual Reuse-Timers for Pots need a Time looking forward
                            // into Future, set with value of "itemtemplate.CanUseEvery" and no longer back into past
                            if (nextPotionAvailTime > client.Player.CurrentRegion.Time)
                            {
                                list.Add(LanguageMgr.GetTranslation(client, "DetailDisplayHandler.WritePotionInfo.UseItem3", Util.FormatTime((nextPotionAvailTime - client.Player.CurrentRegion.Time) / 1000)));
                            }
                            else
                            {
                                int minutes = item.CanUseEvery / 60;
                                int seconds = item.CanUseEvery % 60;

                                if (minutes == 0)
                                {
                                    list.Add(String.Format("Can use item every: {0} sec", seconds));
                                }
                                else
                                {
                                    list.Add(String.Format("Can use item every: {0}:{1:00} min", minutes, seconds));
                                }
                            }

                            if (spl.CastTime > 0)
                            {
                                list.Add(" ");
                                list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePotionInfo.NoUseInCombat"));
                            }
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Nidel: Write spell's infos of potions and subspell's infos with recursive method.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="client"></param>
        /// <param name="spl"></param>
        /// <param name="line"></param>
        private static void WritePotionSpellsInfos(IList<string> list, GameClient client, Spell spl, NamedSkill line)
        {
            if (spl != null)
            {
                list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.MagicAbility"));
                list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePotionInfo.Type", spl.SpellType));
                list.Add(" ");
                list.Add(spl.Description);
                list.Add(" ");
                if (spl.Value != 0)
                {
                    list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePotionInfo.Value", spl.Value));
                }
                list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePotionInfo.Target", spl.Target));
                if (spl.Range > 0)
                {
                    list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePotionInfo.Range", spl.Range));
                }
                list.Add(" ");
                list.Add(" ");
                if (spl.SubSpellID > 0)
                {
                    List<Spell> spells = SkillBase.GetSpellList(line.KeyName);
                    foreach (Spell subSpell in spells)
                    {
                        if (subSpell.ID == spl.SubSpellID)
                        {
                            WritePotionSpellsInfos(list, client, subSpell, line);
                            break;
                        }
                    }
                }
            }
        }
         
#region v1.110+
        /// <summary>
        /// Writer for v1.110+ delves
        /// 
        /// Examples: 
        /// <example>
        ///  (Spell (Function "")(Index " "))
        /// </example>
        /// 
        /// @author mlinder
        /// </summary>
        public sealed class DelveWriter {

            private int _openTags = 0;
            private readonly StringBuilder _str = new StringBuilder();

            /// <summary>
            /// Begins a new tag
            /// </summary>
            /// <param name="name">Name of the tag, e.g. Spell</param>
            public DelveWriter Begin(string name) {
                _str.Append('(');
                _str.Append(name);
                _str.Append(' ');
                _openTags++;
                return this;
            }

            /// <summary>
            /// Adds a subtag key-value pair
            /// </summary>
            /// <param name="name">Name of the sub-tag, e.g. Index</param>
            /// <param name="value">Value of the sub-tag, e.g. 3</param>
            public DelveWriter Value(string name, object value) {
                Begin(name).Value(value).End();
                return this;
            }

            /// <summary>
            /// Subtag which is only added if the specified condition matches
            /// </summary>
            /// <param name="name">Name of the sub-tag</param>
            /// <param name="value">Value of the sub-tag</param>
            /// <param name="condition">Condition which has to match</param>
            public DelveWriter Value(string name, object value, bool condition)
            {
				if (condition)
					Begin(name).Value(value).End();
                return this;
            }

            /// <summary>
            /// Only writes the value of a tag
            /// </summary>
            /// <param name="value">e.g. 3</param>
            public DelveWriter Value(object value) {
                _str.Append("\"");
                _str.Append((value ?? "").ToString().Replace("\"", "\\\""));
                _str.Append("\"");
                return this;
            }

            /// <summary>
            /// Closes an opened tag. Can only be called after a previous Begin()
            /// </summary>
            /// <returns></returns>
            public DelveWriter End() {
                _str.Append(')');
                _openTags--;
                if (_openTags < 0) 
                    throw new ArgumentException("More End() than Begin() calls");
                return this;
            }

            public override string ToString() {
                while (_openTags > 0)
                    End();
                return _str.ToString();
            }
        }

        /** General info @ v1.110:
         *  - Examples can be found at http://dl.dropbox.com/u/48908369/delve.txt
         *  - 'Expires' can be left out
         *  - No idea what 'Fingerprint' does
         **/

        static string DelveAbility(GameClient clt, int id) { /* or skill */
            Skill s = clt.Player.GetNonTrainableSkillList().Cast<Skill>().Where(sk => sk.ID == id).FirstOrDefault();
            var dw = new DelveWriter().Begin(s is Ability ? "Ability" : "Skill").Value("Index", id);
            if (s != null) {
                dw.Value("Name", s.Name);
            }
            else dw.Value("Name", "(not found)");
            return dw.ToString();
        }


		/// <summary>
		/// Delve Info for Songs (V1.110+)
		/// </summary>
		/// <param name="clt">Client</param>
		/// <param name="id">SpellID</param>
		/// <returns></returns>
		public static string DelveSong(GameClient clt, int id)
		{
			var dw = new DelveWriter();

			Spell spell = SkillBase.GetSpellByID(id);
		
	
			if (spell != null)
			{
	
					dw.Begin("Song")
						.Value("Index", id)
						.Value("Name", spell.Name)
						.Value("effect", spell.ClientEffect)
						.Value("description_string", spell.Description, !string.IsNullOrEmpty(spell.Description))
						;
					//log.Info(dw.ToString());
					return dw.ToString();
			}

			// not found
			return dw.Begin("Song").Value("Name", "(not found)").Value("Index", id).ToString();
		}

		/// <summary>
		/// Delve Info for Spells (V1.110+)
		/// </summary>
		/// <param name="clt">Client</param>
		/// <param name="id">SpellID</param>
		/// <returns></returns>
        public static string DelveSpell(GameClient clt, int id) {
            var dw = new DelveWriter();
			
			var spell = SkillBase.GetSpellByID(id);

			//if (spell != null && spellLine != null)
			{
				/*
				var spellLine = clt.Player.GetSpellLines()[lineId] as SpellLine;
				if (spellLine != null) {
					Spell spell = null;
					foreach (Spell spl in SkillBase.GetSpellList(spellLine.KeyName)) {
						if (spl.Level == level) {
							spell = spl;
							break;
						}
					}*/
				//if (spell != null) {
				// NOT GOOD SOLUTION SPELLLINE IS BETTER INCLUDED IN SPELL
				ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(clt.Player, spell,SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
				if (spellHandler != null)
				{
				dw.Begin("Spell")
						.Value("Index", id)
						.Value("Name", spell.Name)
						//.Value("Function", spellHandler.FunctionName ?? "0")
						.Value("cast_timer", spell.CastTime - 2000, spell.CastTime > 2000) //minus 2 seconds (why mythic?)
						.Value("instant","1",spell.CastTime==0)
						//.Value("damage", spellHandler.GetDelveValueDamage, spellHandler.GetDelveValueDamage != 0)
						.Value("damage_type", (int) spell.DamageType + 1, (int) spell.DamageType > 0) // Damagetype not the same as dol
						//.Value("type1", spellHandler.GetDelveValueType1, spellHandler.GetDelveValueType1 > 0)
						.Value("level", spell.Level, spell.Level > 0)
						.Value("power_cost", spell.Power, spell.Power != 0)
						//.Value("round_cost",spellHandler.GetDelveValueRoundCost,spellHandler.GetDelveValueRoundCost!=0)
						//.Value("power_level", spellHandler.GetDelveValuePowerLevel,spellHandler.GetDelveValuePowerLevel!=0)
						.Value("range", spell.Range, spell.Range > 0)
						.Value("duration", spell.Duration/1000, spell.Duration > 0) //seconds
						.Value("dur_type", GetDurationType(spell), GetDurationType(spell) > 0)
						//.Value("parm",spellHandler.GetDelveValueParm,spellHandler.GetDelveValueParm>0) 
						.Value("timer_value", spell.RecastDelay/1000, spell.RecastDelay > 1000)
						//.Value("bonus", spellHandler.GetDelveValueBonus, spellHandler.GetDelveValueBonus > 0)
						//.Value("no_combat"," ",Util.Chance(50))//TODO
						//.Value("link",14000)
						//.Value("ability",4) // ??
						//.Value("use_timer",4)
						.Value("target", GetSpellTargetType(spell.Target), GetSpellTargetType(spell.Target) > 0)
						//.Value("frequency", spellHandler.GetDelveValueFrequency, spellHandler.GetDelveValueFrequency != 0)
						.Value("description_string", spell.Description, !string.IsNullOrEmpty(spell.Description))
						.Value("radius", spell.Radius, spell.Radius > 0)
						.Value("concentration_points", spell.Concentration, spell.Concentration > 0)
						//.Value("num_targets", spellHandler.GetDelveValueNumTargets, spellHandler.GetDelveValueNumTargets>0)
						//.Value("no_interrupt", spell.Interruptable ? (char)0 : (char)1) //Buggy?
						;
					//log.Info(dw.ToString());
					return dw.ToString();
					// }
				}
			}

        	// not found
            return dw.Begin("Spell").Value("Name", "(not found)").Value("Index", id).ToString();
        }

		#region delvespell methods

		/// <summary>
		/// Returns delve code for target
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		static int GetSpellTargetType(string target)
		{
			switch (target)
			{
				case "Realm":
					return 7;
				case "Self":
					return 0;
				case "Enemy":
					return 1;
				case "Pet":
					return 6;
				case "Group":
					return 3;
				case "Area":
					return 0; // TODO
				default:
					return 0;
			}
		}

		static int GetDurationType(Spell spell)
		{
			//2-seconds,4-conc,5-focus
			if (spell.Duration>0)
			{
				return 2;
			}
			if (spell.Concentration>0)
			{
				return 4;
			}


			return 0;
		}

		#endregion
		static string DelveStyle(GameClient clt, int id)
        {
            var style = SkillBase.GetStyleByID(id, clt.Player.CharacterClass.ID);

            var dw = new DelveWriter()
                .Begin("Style").Value("Index", id);
            
            if (style != null) {
                // Not implemented:
                // (Style (FollowupStyle "Sapphire Slash")(LevelBonus "2")(OpeningDamage "16")(Skill "1")(Expires "1343375647"))
                // (Style (Fingerprint "1746652963")(FollowupStyle "Thigh Cut")(Hidden "1")OpeningDamage "55")(Skill "118")(SpecialNumber "1511")(SpecialType "1")(Expires "1342381240"))

            	dw
            		.Value("Name", style.Name)
            		.Value("Icon", style.Icon)
            		.Value("Level", style.Level)
            		.Value("Fatigue", style.EnduranceCost)
            		//.Value("SpecialType", (int)style.SpecialType, style.SpecialType != 0)
					//.Value("SpecialNumber", GetSpecialNumber(style), GetSpecialNumber(style)!=0)
            		.Value("DefensiveMod", style.BonusToDefense, style.BonusToDefense != 0)
            		.Value("AttackMod", style.BonusToHit, style.BonusToHit != 0)
            		.Value("OpeningType", (int)style.OpeningRequirementType)
					.Value("OpeningNumber", style.OpeningRequirementValue, style.OpeningRequirementType == Style.eOpening.Positional)
					//.Value("OpeningResult",GetOpeningResult(style,clt),GetOpeningResult(style,clt)>0)
					//.Value("OpeningStyle",GetOpeningStyle(style),(Style.eAttackResult)GetOpeningResult(style,clt) == Style.eAttackResult.Style)
            		.Value("Weapon", style.GetRequiredWeaponName(), style.WeaponTypeRequirement > 0)
            		.Value("Hidden", "1",style.StealthRequirement)
            		//.Value("TwoHandedIcon", 10, style.TwoHandAnimation > 0)
					//.Value("Skill",43)
					.Value("OpeningDamage",style.GrowthRate*100,style.GrowthRate>0)
					//.Value("SpecialValue", GetSpecialValue(style),GetSpecialValue(style)!=0)
					//.Value("FollowupStyle",style.DelveFollowUpStyles,!string.IsNullOrEmpty(style.DelveFollowUpStyles))
            		;
            }
            else {
                dw.Value("Name", "(not found)");
            }
            return dw.End().ToString();
        }

		#region style v1.110 methods
		/*
		public static int GetSpecialNumber(Style style)
		{
			if (style.SpecialType == Style.eSpecialType.Effect)
			{
				Spell spell = SkillBase.GetSpellById(style.SpecialValue);
				if (spell != null)
					return spell.ClientEffect;
			}
			return 0;
		}

		public static int GetSpecialValue(Style style)
		{
			switch(style.SpecialType)
			{
				case Style.eSpecialType.ExtendedRange:
					return 128; // Extended Range f�r Reaver style
				case Style.eSpecialType.Taunt:
					return style.SpecialValue;
			}
			return 0;
		}*/

		
		/*public static int GetOpeningResult(Style style,GameClient clt)
		{
			switch(StyleProcessor.ResolveAttackResult(style,clt.Player.PlayerCharacter.Class))
			{
				case GameLiving.eAttackResult.Any:
					return (int)Style.eAttackResult.Any;
				case GameLiving.eAttackResult.Missed:
					return (int) Style.eAttackResult.Miss;
				case GameLiving.eAttackResult.Parried:
					return (int)Style.eAttackResult.Parry;
				case GameLiving.eAttackResult.Evaded:
					return (int)Style.eAttackResult.Evade;
				case GameLiving.eAttackResult.Blocked:
					return (int)Style.eAttackResult.Block;
				case GameLiving.eAttackResult.Fumbled:
					return (int)Style.eAttackResult.Fumble;
				case GameLiving.eAttackResult.HitStyle:
					return (int)Style.eAttackResult.Style;
				case GameLiving.eAttackResult.HitUnstyled:
					return (int)Style.eAttackResult.Hit;
			}
			return 0;
		}*/
		/*
		public static string GetOpeningStyle(Style style)
		{
			if (style.OpeningRequirementValue > 0)
			{
				Style style2 = SkillBase.GetStyleByID(style.OpeningRequirementValue);
				if (style2!=null)
					return style2.Name;
				return "";
			}
			return "";
		}*/

		#endregion

		/// <summary>
		/// Delve the realm abilities for v1.110+ clients
		/// </summary>
		/// <param name="clt"></param>
		/// <param name="id"></param>
		/// <returns></returns>
        static string DelveRealmAbility(GameClient clt, int id) {
            var dw = new DelveWriter().Begin("RealmAbility").Value("Index", id);
            var ra = clt.Player.GetNonTrainableSkillList().Cast<Skill>().Where(sk => sk.ID == id).FirstOrDefault() as RealmAbility;
            if (ra != null)
            {
                ra.AddDelve(dw, clt.Player);
            }
            else dw.Value("Name", "(not found)");
            return dw.ToString();
        }
        #endregion
    }
 }

