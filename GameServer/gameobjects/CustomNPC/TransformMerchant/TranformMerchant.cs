/*
 * This NPC was originally written by elctotek 2022. 
 */

using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


namespace DOL.GS
{


    public class TransformMerchant : GameNPC
    {

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public const string FirstItem = "FIRSTITEM";
        public const string SecoundItem = "SECOUNDITEM";
        public const string _Spell1 = "TRANSFOMSPELL1";
        public const string _Spell2 = "TRANSFOMSPELL2";
        
        public const string _Proc1 = "PROC1";
        public const string _Proc2 = "PROC2";
        public const string _ItemModel = "ITEMMODEL";


        public const string ListLoaded = "LISTLOADED";

        public TransformMerchant()
            : base()
        {
            Flags |= GameNPC.eFlags.PEACE;
        }
        #region Add To World
        public override bool AddToWorld()
        {
           
            GuildName = "Tranform Merchant";
            Level = 50;
            base.AddToWorld();


            return true;
        }

        
        #endregion Add To World


        //protected static List<InventoryItem> recivedItems = new List<InventoryItem>();
        public virtual bool AllowedItemType(InventoryItem item)
        {
            switch (item.Object_Type)
            {

                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                case 27:
                case 28:
                case 32:
                case 33:
                case 34:
                case 35:
                case 36:
                case 37:
                case 38:
                case 41:
                case 42:
                case 45:
                    {
                        return true;
                    }
            }
            return false;
        }

        private static bool BonusExists(ItemUnique item, eProperty property)
        {
            if (item.Bonus1Type == (int)property ||
                item.Bonus2Type == (int)property ||
                item.Bonus3Type == (int)property ||
                item.Bonus4Type == (int)property ||
                item.Bonus5Type == (int)property)
                return true;

            return false;
        }

        public enum eBonusType
        {
            Stat,
            AdvancedStat,
            Resist,
            Skill,
            Focus,
        }


        private static eProperty GetProperty(eBonusType type, ItemUnique item)
        {
            switch (type)
            {
                case eBonusType.Focus:
                    {
                        return eProperty.AllFocusLevels;
                    }
                case eBonusType.Resist:
                    {
                        return (eProperty)Util.Random((int)eProperty.Resist_First, (int)eProperty.Resist_Last);
                    }
                case eBonusType.Skill:
                    {
                        // fill valid skills
                        ArrayList validSkills = new ArrayList();

                        bool fIndividualSkill = false;

                        // All Skills is never combined with any other skill
                        if (!BonusExists(item, eProperty.AllSkills))
                        {
                            // All type skills never combined with individual skills
                            if (!BonusExists(item, eProperty.AllMagicSkills) &&
                                !BonusExists(item, eProperty.AllMeleeWeaponSkills) &&
                                !BonusExists(item, eProperty.AllDualWieldingSkills) &&
                                !BonusExists(item, eProperty.AllArcherySkills))
                            {
                                // individual realm specific skills
                                if ((eRealm)item.Realm == eRealm.Albion)
                                {
                                    foreach (eProperty property in AlbSkillBonus)
                                    {
                                        if (!BonusExists(item, property))
                                        {
                                            if (SkillIsValidForObjectType(item, property))
                                                validSkills.Add(property);
                                        }
                                        else
                                            fIndividualSkill = true;
                                    }
                                }
                                else if ((eRealm)item.Realm == eRealm.Hibernia)
                                {
                                    foreach (eProperty property in HibSkillBonus)
                                    {
                                        if (!BonusExists(item, property))
                                        {
                                            if (SkillIsValidForObjectType(item, property))
                                                validSkills.Add(property);
                                        }
                                        else
                                            fIndividualSkill = true;
                                    }
                                }
                                else if ((eRealm)item.Realm == eRealm.Midgard)
                                {
                                    foreach (eProperty property in MidSkillBonus)
                                    {
                                        if (!BonusExists(item, property))
                                        {
                                            if (SkillIsValidForObjectType(item, property))
                                                validSkills.Add(property);
                                        }
                                        else
                                            fIndividualSkill = true;
                                    }
                                }

                                if (!fIndividualSkill)
                                {
                                    // ok to add AllSkills, but reduce the chance
                                    if (SkillIsValidForObjectType(item, eProperty.AllSkills) && Util.Chance(25))
                                        validSkills.Add(eProperty.AllSkills);
                                }
                            }

                            // All type skills never combined with individual skills
                            if (!fIndividualSkill)
                            {
                                if (!BonusExists(item, eProperty.AllMagicSkills) && SkillIsValidForObjectType(item, eProperty.AllMagicSkills))
                                    validSkills.Add(eProperty.AllMagicSkills);

                                if (!BonusExists(item, eProperty.AllMeleeWeaponSkills) && SkillIsValidForObjectType(item, eProperty.AllMeleeWeaponSkills))
                                    validSkills.Add(eProperty.AllMeleeWeaponSkills);

                                if (!BonusExists(item, eProperty.AllDualWieldingSkills) && SkillIsValidForObjectType(item, eProperty.AllDualWieldingSkills))
                                    validSkills.Add(eProperty.AllDualWieldingSkills);

                                if (!BonusExists(item, eProperty.AllArcherySkills) && SkillIsValidForObjectType(item, eProperty.AllArcherySkills))
                                    validSkills.Add(eProperty.AllArcherySkills);
                            }

                        }

                        int index = 0;
                        index = validSkills.Count - 1;

                        if (index < 1)
                        {
                            // return a safe random stat

                            type = eBonusType.Stat;

                            switch (Util.Random(0, 4))
                            {
                                case 0:
                                    return eProperty.MaxHealth;
                                case 1:
                                    return eProperty.Strength;
                                case 2:
                                    return eProperty.Dexterity;
                                case 3:
                                    return eProperty.Quickness;
                                case 4:
                                    return eProperty.Constitution;
                            }
                        }

                        return (eProperty)validSkills[Util.Random(0, index)];
                    }
                case eBonusType.Stat:
                    {
                        // ToDo: this does not check for duplicates like INT and Acuity
                        ArrayList validStats = new ArrayList();
                        foreach (eProperty property in StatBonus)
                        {
                            if (!BonusExists(item, property) && StatIsValidForObjectType(item, property) && StatIsValidForRealm((eRealm)item.Realm, property))
                            {
                                validStats.Add(property);
                            }
                        }
                        return (eProperty)validStats[Util.Random(0, validStats.Count - 1)];
                    }
                case eBonusType.AdvancedStat:
                    {
                        // ToDo: this does not check for duplicates like INT and Acuity
                        ArrayList validStats = new ArrayList();
                        foreach (eProperty property in AdvancedStats)
                        {
                            if (!BonusExists(item, property) && StatIsValidForObjectType(item, property) && StatIsValidForRealm((eRealm)item.Realm, property))
                                validStats.Add(property);
                        }
                        return (eProperty)validStats[Util.Random(0, validStats.Count - 1)];
                    }
            }
            return eProperty.MaxHealth;
        }

        private static eBonusType GetPropertyType(ItemUnique item)
        {
            //allfocus
            if (CanAddFocus(item))
                return eBonusType.Focus;

            // ToA allows stat cap bonuses
            if (item.Level > 50 && Util.Chance(20))
            {
                return eBonusType.AdvancedStat;
            }

            //stats
            if (Util.Chance(50))
                return eBonusType.Stat;
            //resists
            if (Util.Chance(40))
                return eBonusType.Resist;
            //skills
            return eBonusType.Skill;
        }

        // Only try to add focus once
        private static bool CanAddFocus(ItemUnique item)
        {
            if (item.Object_Type == (int)eObjectType.Staff)
            {
                if (item.Bonus1Type != 0)
                    return false;

                if (item.Realm == (int)eRealm.Albion && item.Description == "friar")
                    return false;

                return true;
            }

            return false;
        }

        private static void GenerateMagicalBonuses(ItemUnique item)
        {
            // unique objects have more bonuses as level rises

            int number = 3;

            if (Util.Chance(20 + item.Level * 2)) // 100% magical starting at level 40
            {
                //1
                number++;

                if (Util.Chance(item.Level * 8 - 40)) // level 6 - 17 (100%)
                {
                    //2
                    number++;

                    if (Util.Chance(item.Level * 6 - 60)) // level 11 - 27 (100%)
                    {
                        //3
                        number++;

                        if (Util.Chance(item.Level * 4 - 80)) // level 21 - 45 (100%)
                        {
                            //4
                            number++;

                            if (number == 4 && item.Level > 50)
                                number++; // 5
                        }
                    }
                }

            }

            // Magical items have at least 1 bonus
            if (item.Object_Type == (int)eObjectType.Magical && number < 1)
                number = 1;


            bool fNamed = false;
            bool fAddedBonus = false;

            double quality = (double)item.Quality * .01;

            double multiplier = (quality * quality * quality) + 0.15;

           if (item.Quality <= 89)
            {
                item.Quality = Util.Random(90, 98);
            }


            for (int i = 0; i < number; i++)
            {
                eBonusType type = GetPropertyType(item);
                eProperty property = GetProperty(type, item);
                if (!BonusExists(item, property))
                {
                    int amount = (int)Math.Ceiling((double)GetBonusAmount(type, property, item.Level) * multiplier);
                    WriteBonus(item, property, amount);
                    fAddedBonus = true;

                    
                    if (!fNamed && WriteMagicalName(property, item))
                    {
                        fNamed = true;
                        multiplier *= 0.65;
                    }
                   
                }
            }

             //non magical items get lowercase names
            if (number == 0 || !fAddedBonus)
                item.Name = item.Name.ToLower();
        }


        public virtual bool ReadyTobeginWork(GamePlayer tragt)
        {
            if (tragt.TempProperties.getProperty<InventoryItem>(FirstItem) != null && tragt.TempProperties.getProperty<InventoryItem>(SecoundItem) != null)
            {
                return true;
            }
            else

            return false;
        }


public virtual ItemUnique CreateUniqueItem(InventoryItem item1, InventoryItem item2, GamePlayer player, ItemUnique option)
        {
            ItemUnique newUniqueItem = null;
            int firstvalue = 1;
            int secoundvalue = 1;

            Util.Random(firstvalue, secoundvalue);


            if (newUniqueItem == null)
            {
                newUniqueItem = new ItemUnique();

                newUniqueItem.Name = item2.Name;// + " of Transformation)";
                newUniqueItem.Description = "(Transformed Unique Item) Created By: " + player.Name+"";
                newUniqueItem.Realm = (int)player.Realm;
                newUniqueItem.Level = item1.Level;
                newUniqueItem.Price = item2.Price;
                newUniqueItem.Bonus = item1.Bonus;
                newUniqueItem.MaxCount = 1;
                newUniqueItem.Quality = item1.Quality;
                newUniqueItem.Model = item2.Model;
                newUniqueItem.Weight = item2.Weight;
                newUniqueItem.SPD_ABS = item2.SPD_ABS;
                newUniqueItem.DPS_AF = item2.DPS_AF;
                newUniqueItem.Hand = item2.Hand;
                newUniqueItem.IsDropable = item1.IsDropable;
                newUniqueItem.IsIndestructible = false;
                newUniqueItem.IsNotLosingDur = true;
                newUniqueItem.IsPickable = item2.IsPickable;
                newUniqueItem.IsTradable = item2.IsTradable;
                newUniqueItem.Item_Type = item2.Item_Type;
                newUniqueItem.Object_Type = item1.Object_Type;
                newUniqueItem.PackageID = "New Unique Item";
                newUniqueItem.PackSize = 1;
                newUniqueItem.PassiveSpell = item2.PassiveSpell;
                newUniqueItem.PoisonCharges = item2.PoisonCharges;
                newUniqueItem.PoisonMaxCharges = item2.PoisonMaxCharges;
                newUniqueItem.PoisonSpellID = item2.PoisonSpellID;
                newUniqueItem.ProcChance = item2.ProcChance;
                newUniqueItem.RepearCount = item2.RepearCount;
                newUniqueItem.SalvageYieldID = item2.SalvageYieldID;
                newUniqueItem.LevelRequirement = item1.LevelRequirement;
                newUniqueItem.BonusLevel = item1.BonusLevel;
                newUniqueItem.Type_Damage = item2.Type_Damage;
                //GenerateMagicalBonuses(newUniqueItem);

                /*
                if (option != null)
                newUniqueItem = option;


                //bonus values
                if (item1.Bonus1 > 0 && item2.Bonus1 > 0 && item2.Bonus1Type > 0)
                {
                  
                        newUniqueItem.Bonus1 = Util.Random(1, item2.Bonus1);
                   
                }
                else
                    newUniqueItem.Bonus1 = item1.Bonus1;

                if (item1.Bonus2 > 0 && item2.Bonus2 > 0 && item2.Bonus2Type > 0)
                {
                  
                        newUniqueItem.Bonus2 = Util.Random(1, item2.Bonus2);
                
                }
                else
                    newUniqueItem.Bonus2 = item1.Bonus2;



                if (item1.Bonus3 > 0 && item2.Bonus3 > 0 && item2.Bonus3Type > 0)
                {
                    newUniqueItem.Bonus3 = Util.Random(1, item2.Bonus3);
                }
                else
                    newUniqueItem.Bonus3 = item1.Bonus3;

                if (item1.Bonus4 > 0 && item2.Bonus4 > 0 && item2.Bonus4Type > 0)
                {
                    newUniqueItem.Bonus3 = Util.Random(1, item2.Bonus4);
                }
                else
                    newUniqueItem.Bonus4 = item1.Bonus4;

                if (item1.Bonus5 > 0 && item2.Bonus5 > 0 && item2.Bonus5Type > 0)
                {
                    newUniqueItem.Bonus5 = Util.Random(1, item2.Bonus5);
                }
                else
                    newUniqueItem.Bonus5 = item1.Bonus5;




                if (item1.Bonus6 > 0 && item2.Bonus6 > 0 && item2.Bonus6Type > 0)
                {
                    newUniqueItem.Bonus6 = Util.Random(1, item2.Bonus6);
                }
                else
                    newUniqueItem.Bonus6 = item1.Bonus6;

                if (item1.Bonus7 > 0 && item2.Bonus7 > 0 && item2.Bonus7Type > 0)
                {
                    newUniqueItem.Bonus7 = Util.Random(1, item2.Bonus7);
                }
                else
                    newUniqueItem.Bonus7 = item1.Bonus7;

                if (item1.Bonus8 > 0 && item2.Bonus8 > 0 && item2.Bonus8Type > 0)
                {
                    newUniqueItem.Bonus8 = Util.Random(1, item2.Bonus8);
                }
                else
                    newUniqueItem.Bonus8 = item1.Bonus8;

                if (item1.Bonus9 > 0 && item2.Bonus9 > 0 && item2.Bonus9Type > 0)
                {
                    newUniqueItem.Bonus9 = Util.Random(1, item2.Bonus9);
                }
                else
                    newUniqueItem.Bonus9 = item1.Bonus9;

                if (item1.Bonus10 > 0 && item2.Bonus10 > 0 && item2.Bonus10Type > 0)
                {
                    newUniqueItem.Bonus10 = Util.Random(1, item2.Bonus10);
                }
                else
                    newUniqueItem.Bonus10 = item1.Bonus10;

             
                //bonus type
                newUniqueItem.Bonus1Type = item2.Bonus1Type;
                newUniqueItem.Bonus2Type = item2.Bonus2Type;
                newUniqueItem.Bonus3Type = item2.Bonus3Type;
                newUniqueItem.Bonus4Type = item2.Bonus4Type;
                newUniqueItem.Bonus5Type = item2.Bonus5Type;
                newUniqueItem.Bonus6Type = item2.Bonus6Type;
                newUniqueItem.Bonus7Type = item2.Bonus7Type;
                newUniqueItem.Bonus8Type = item2.Bonus8Type;
                newUniqueItem.Bonus9Type = item2.Bonus9Type;
                newUniqueItem.Bonus10Type = item2.Bonus10Type;


                newUniqueItem.Color = item2.Color;
                newUniqueItem.Condition = item1.Condition;
                newUniqueItem.MaxCondition = item1.MaxCondition;

                //spells
                newUniqueItem.MaxCharges = item2.MaxCharges;
                newUniqueItem.MaxCharges1 = item2.MaxCharges1;
                newUniqueItem.SpellID = item2.SpellID;
                newUniqueItem.SpellID1 = item2.SpellID1;
                //procs
                newUniqueItem.ProcSpellID = item2.ProcSpellID;
                newUniqueItem.ProcSpellID1 = item2.ProcSpellID1;

                newUniqueItem.CanUseEvery = item1.CanUseEvery;
                newUniqueItem.CanDropAsLoot = item1.CanDropAsLoot;
                newUniqueItem.BonusLevel = item1.BonusLevel;
                newUniqueItem.AllowedClasses = item1.AllowedClasses;
                newUniqueItem.Effect = item1.Effect;
                newUniqueItem.Emblem = item1.Emblem;
                newUniqueItem.Extension = item2.Extension;
                newUniqueItem.Flags = item1.Flags;
                */

                GenerateMagicalBonuses(newUniqueItem);

                if (option != null)
                {
                    newUniqueItem = option;
                }

                newUniqueItem.Color = item2.Color;
                newUniqueItem.Condition = item1.Condition;
                newUniqueItem.MaxCondition = item1.MaxCondition;

                //spells
                newUniqueItem.MaxCharges = item2.MaxCharges;
                newUniqueItem.MaxCharges1 = item2.MaxCharges1;
                newUniqueItem.SpellID = item2.SpellID;
                newUniqueItem.SpellID1 = item2.SpellID1;
                //procs
                newUniqueItem.ProcSpellID = item2.ProcSpellID;
                newUniqueItem.ProcSpellID1 = item2.ProcSpellID1;

                newUniqueItem.CanUseEvery = item1.CanUseEvery;
                newUniqueItem.CanDropAsLoot = item1.CanDropAsLoot;
                newUniqueItem.BonusLevel = item1.BonusLevel;
                newUniqueItem.AllowedClasses = item1.AllowedClasses;
                newUniqueItem.Effect = item1.Effect;
                newUniqueItem.Emblem = item1.Emblem;
                newUniqueItem.Extension = item2.Extension;
                newUniqueItem.Flags = item1.Flags;


                return newUniqueItem;
            }

            return null;

        }





        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;


            TurnTo(player, 10000);
            InventoryItem item = null;
            
            if (player.TempProperties.getProperty<InventoryItem>(FirstItem) != null)
            {
                item = player.TempProperties.getProperty<InventoryItem>(FirstItem);
            }
           
            string msg0 = "";
            if (item != null)
            {
                msg0 = Name + " says, my service will cost  " + Money.GetShortString(item.Level * 3000) + " (You must pay me at the end of the prozess)";
            }
            string msg1 = Name + " says, i am the item tranformer, i can convert items for you if you wish.";
            string msg2 = Name + " says, hand me two items of the same type and level one after the other, for example two weapons of the same type and level, and I'll convert them into a new unique one for you.";
            string msg3 = Name + " says I'm ready to [begin] or would you rather [abort] " + player.Name + "? " + msg0;

            player.Out.SendMessage(msg0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

            if (ReadyTobeginWork(player) == false)
            {
                //player.Out.SendMessage("Hand me two items from same type and level, one by one (no unique items) so i can create a new one for you.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                player.Out.SendMessage(msg1, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                player.Out.SendMessage(msg2, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            }

            else 
            {
                player.Out.SendMessage(msg3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                //player.Out.SendMessage(""+ Name + " say's, ok " + player.Name+ ", lets begin [beginWork] or [abort] ? (you will lost all items i get, there is now way back after i begin!)", eChatType.CT_System, eChatLoc.CL_PopupWindow);
            }
                

            return true;
        }
       

        public void SendReply(GamePlayer target, string msg)
        {
            target.Client.Out.SendMessage(
                msg,
                eChatType.CT_Say, eChatLoc.CL_PopupWindow);
        }




        public override bool ReceiveItem(GameLiving source, InventoryItem item)
        {

            GamePlayer t = source as GamePlayer;
            if (source != null && source.IsWithinRadius(this, WorldMgr.INTERACT_DISTANCE) == false)
            {
                ((GamePlayer)source).Out.SendMessage("You are too far away to give anything to " + GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (item.IsTradable == false)
            {

                if (t.TempProperties.getProperty<InventoryItem>(FirstItem) != null)
                {
                    t.Out.SendMessage("You get back: " + t.TempProperties.getProperty<InventoryItem>(FirstItem).Name + " ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    t.ReceiveItem(t, t.TempProperties.getProperty<InventoryItem>(FirstItem));
                    t.TempProperties.setProperty(FirstItem, null);
                    t.TempProperties.removeProperty(FirstItem);
                    t.SaveIntoDatabase();



                }
                Emote(eEmote.No);
                t.Out.SendMessage(Name + " says, sorry i can't handle it, this item is not Tradable! ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                return false;
            }

            if (item.Template is ItemUnique)
            {

                if (t.TempProperties.getProperty<InventoryItem>(FirstItem) != null)
                {
                    t.Out.SendMessage("You get back: " + t.TempProperties.getProperty<InventoryItem>(FirstItem).Name + " ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    t.ReceiveItem(t, t.TempProperties.getProperty<InventoryItem>(FirstItem));
                    t.TempProperties.setProperty(FirstItem, null);
                    t.TempProperties.removeProperty(FirstItem);
                    t.SaveIntoDatabase();



                }
                Emote(eEmote.No);
                t.Out.SendMessage(Name + " says, sorry i can't handle Unique items! ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                return false;
            }


            if (item.PackageID == "Artifact")
            {

                if (t.TempProperties.getProperty<InventoryItem>(FirstItem) != null)
                {
                    t.Out.SendMessage("You get back: " + t.TempProperties.getProperty<InventoryItem>(FirstItem).Name + " ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    t.ReceiveItem(t, t.TempProperties.getProperty<InventoryItem>(FirstItem));
                    t.TempProperties.setProperty(FirstItem, null);
                    t.TempProperties.removeProperty(FirstItem);
                    t.SaveIntoDatabase();
                   
                  

                }
                Emote(eEmote.No);
                t.Out.SendMessage(Name + " says, sorry i can't handle Artifacts! ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                return false;
            }
            if (item.IsIndestructible)
            {

                if (t.TempProperties.getProperty<InventoryItem>(FirstItem) != null)
                {
                    t.Out.SendMessage("You get back: " + t.TempProperties.getProperty<InventoryItem>(FirstItem).Name + " ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    t.ReceiveItem(t, t.TempProperties.getProperty<InventoryItem>(FirstItem));
                    t.TempProperties.setProperty(FirstItem, null);
                    t.TempProperties.removeProperty(FirstItem);
                    t.SaveIntoDatabase();
                    GamePlayerUtils.SendClearPopupWindow(t);
                   

                }
                Emote(eEmote.No);
                t.Out.SendMessage(Name + " says, sorry i can't handle this type of item! ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                return false;
            }
           


            if (t != null && item != null && AllowedItemType(item) == false)
            {
                Emote(eEmote.No);
                t.Out.SendMessage(Name + " says, sorry i can't handle this type of item! ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                return false;
            }
            else if (t != null && ReadyTobeginWork(t))
            {
                Emote(eEmote.No);
                t.Out.SendMessage(Name + " says, sorry i can't handle more than two items at the same time! ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                return false;
            }
            /*
            //34 * 3 = 102
            if (item != null  && item.Count > 1)
            {
                t.Out.SendMessage("Too mutch in stack, i can't handle this!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }
            */

            if (item != null)
            {


                Emote(eEmote.Induct);

                if (t.TempProperties.getProperty<InventoryItem>(FirstItem) == null)
                {
                    t.Out.SendMessage("You hand  " + item.Name + " to " + Name + ". ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    //recivedItems.Add(item);
                    t.Inventory.RemoveItem(item);
                    t.SaveIntoDatabase();
                    t.TempProperties.setProperty(FirstItem, item);
                    t.Out.SendMessage(Name + " Has Received the first item: " + item.Name + ".", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    t.Out.SendMessage(Name + " says, please hand me the secound item, must be the same type!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    Emote(eEmote.Smile);
                    return true;
                    
                }
                else if ((t.TempProperties.getProperty<InventoryItem>(FirstItem) != null && item.Object_Type != t.TempProperties.getProperty<InventoryItem>(FirstItem).Object_Type) || (item.Level != t.TempProperties.getProperty<InventoryItem>(FirstItem).Level))
                {
                                       
                    t.ReceiveItem(t, t.TempProperties.getProperty<InventoryItem>(FirstItem));
                    t.Out.SendMessage("You get back: " + t.TempProperties.getProperty<InventoryItem>(FirstItem).Name + " ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    t.Out.SendMessage(Name + " says, sorry the secound item must be the same type and level like the first one! ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    t.TempProperties.setProperty(FirstItem, null);
                    t.TempProperties.removeProperty(FirstItem);
                    t.SaveIntoDatabase();
                    Emote(eEmote.No);

                    return false;
                }

                else if (t.TempProperties.getProperty<InventoryItem>(SecoundItem) == null)
                {
                    t.Out.SendMessage("You hand the secound  " + item.Name + " to " + Name + ". ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    t.Inventory.RemoveItem(item);
                    t.SaveIntoDatabase();
                    t.TempProperties.setProperty(SecoundItem, item);
                    t.Out.SendMessage(Name + " has Received Secound one: " + item.Name + ".", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    t.Out.SendMessage(Name + " has Received Secound one: " + item.Name + ".", eChatType.CT_System, eChatLoc.CL_PopupWindow);

                   
                    t.Out.SendMessage("" + Name + " say's, ok " + t.Name + ", lets begin [begin] or [abort] ? (The items will be consumed during my work.) ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    t.Out.SendMessage("" + Name + " says, my service will cost  " + Money.GetShortString(item.Level * 3000) + "  (You must pay me at the end of the prozess)", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    Emote(eEmote.Yes);
                    return true;
                }
                else
                {
                    //t.Out.SendMessage("Ich kann im moment keine weiten Items verarbeiten!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return false;
                }
               
            }

            

            return base.ReceiveItem(source, item);
        }


        public override bool WhisperReceive(GameLiving source, string str)
        {
            if (!base.WhisperReceive(source, str)) return false;
            if (!(source is GamePlayer)) return false;
            GamePlayer t = (GamePlayer)source;
            InventoryItem item1 = null;
            InventoryItem item2 = null;
            ItemUnique newUnique = null;




            if (t.TempProperties.getProperty<InventoryItem>(FirstItem) != null)
            {
                item1 = t.TempProperties.getProperty<InventoryItem>(FirstItem);
            }
            if (t.TempProperties.getProperty<InventoryItem>(SecoundItem) != null)
            {
                item2 = t.TempProperties.getProperty<InventoryItem>(SecoundItem);
            }

            switch (str)
            {

                case "begin":
                    {
                        if (newUnique == null)
                        {
                            newUnique = new ItemUnique();

                        }




                        if (t != null && ReadyTobeginWork(t))
                        {

                            t.Out.SendMessage(Name + " says you will lose both items after my work, are you [sure]? or [abort]", eChatType.CT_System, eChatLoc.CL_PopupWindow);

                        }

                        break;
                    }
                case "abort":
                    {
                        if (t != null && item1 != null && item2 != null)
                        {
                            t.Out.SendMessage(Name + " says, as you wish, you'll get your items back!", eChatType.CT_System, eChatLoc.CL_PopupWindow);

                           
                            t.Out.SendMessage(" you get back: " + item1.Name + " ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            t.Out.SendMessage(" you get back: " + item2.Name + " ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            t.ReceiveItem(t, item1);
                            t.ReceiveItem(t, item2);

                            t.TempProperties.setProperty(FirstItem, null);
                            t.TempProperties.removeProperty(FirstItem);
                            t.TempProperties.setProperty(SecoundItem, null);
                            t.TempProperties.removeProperty(SecoundItem);
                            t.SaveIntoDatabase();
                            GamePlayerUtils.SendClearPopupWindow(t);
                            Emote(eEmote.Wave);
                            t.Emote(eEmote.PlayerPickup);
                        }
                        break;
                    }
                case "sure":
                    {
                        if (t != null)
                        {
                            t.Out.SendMessage(Name + " says, do you want to continue using the model from the [first]: "+ item1.Name + " or [second] item:  " + item2.Name + " ?", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            Emote(eEmote.Yes);
                        }
                        break;
                    }
                case "second":
                    {

                        GamePlayerUtils.SendClearPopupWindow(t);
                        t.Out.SendMessage(Name + " says, as you wish, I use the model from the second item: " + item2.Name + " for the transformation.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        Emote(eEmote.Yes);




                        string procitem1 = "";
                        string procitem2 = "";
                        Spell spell = SkillBase.GetSpellByID(item1.ProcSpellID);

                        if (spell != null && spell.ID > 0)
                        {


                            procitem1 = spell.Name;
                        }
                        else
                            procitem1 = "No proc available";

                        Spell spell2 = SkillBase.GetSpellByID(item2.ProcSpellID);

                        if (spell2 != null && spell2.ID > 0)
                        {
                            procitem2 = spell2.Name;
                        }
                        else
                            procitem2 = "No proc available";

                        if (newUnique != null)
                        {

                            newUnique.Object_Type = item2.Object_Type;
                            newUnique.Model = item2.Model;
                            newUnique.Effect = item2.Effect;
                            newUnique.Color = item2.Color;
                            newUnique.Hand = item2.Hand;
                            newUnique.Type_Damage = item2.Type_Damage;


                        }
                        t.TempProperties.setProperty(_ItemModel, item2.Model);


                        t.Out.SendMessage(Name + " says, do You like to use the [first proc] from secound item: " + procitem2 + "  or  [leave first]: " + procitem1 + " ?", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        break;
                    }

                case "first":
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        t.Out.SendMessage(Name + " says, as you wish, I use the model from the first item: " + item1.Name + " for the transformation.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        Emote(eEmote.Yes);

                        string procitem1 = "";
                        string procitem2 = "";
                        Spell spell1 = SkillBase.GetSpellByID(item1.ProcSpellID);

                        if (spell1 != null && spell1.ID > 0)
                        {


                            procitem1 = spell1.Name;
                        }
                        else
                            procitem1 = "No proc available";

                        Spell spell2 = SkillBase.GetSpellByID(item2.ProcSpellID);

                        if (spell2 != null && spell2.ID > 0)
                        {
                            procitem2 = spell2.Name;
                        }
                        else
                            procitem2 = "No proc available";

                        if (newUnique != null)
                        {
                            newUnique.Object_Type = item1.Object_Type;
                            newUnique.Model = item1.Model;
                            newUnique.Effect = item1.Effect;
                            newUnique.Color = item1.Color;
                            newUnique.Hand = item1.Hand;
                            newUnique.Type_Damage = item1.Type_Damage;

                        }
                        t.TempProperties.setProperty(_ItemModel, item1.Model);

                        t.Out.SendMessage(Name + " says, do You like to use the [first proc] from secound item: " + procitem2 + "  or  [leave first]: " + procitem1 + " ?", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        break;
                    }

                //proc
                case "leave first":
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        Emote(eEmote.Induct);
                        ////item1
                        Spell spell = SkillBase.GetSpellByID(item1.ProcSpellID);

                        if (spell != null && spell.ID > 0)
                        {

                            t.TempProperties.setProperty(_Proc1, spell.ID);

                        }
                        else

                            t.TempProperties.setProperty(_Proc1, 0);
                        //////////////////////////////////////////////////////////////////////////////////////////////


                        string procitem1 = "";
                        string procitem2 = "";

                        Spell spell1 = SkillBase.GetSpellByID(item1.ProcSpellID1);

                        if (spell1 != null && spell1.ID > 0)
                        {
                            procitem1 = spell1.Name;
                        }
                        else
                            procitem1 = "No proc available";

                        Spell spell2 = SkillBase.GetSpellByID(item2.ProcSpellID1);

                        if (spell2 != null && spell2.ID > 0)
                        {
                            procitem2 = spell2.Name;
                        }
                        else
                            procitem2 = "No proc available";



                        t.Out.SendMessage(Name + " says, do You like to use the [secound proc]: " + procitem2 + " from the secound item or [leave secound proc]:  " + procitem1 + "?", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        break;
                    }
                case "first proc":
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        Emote(eEmote.Induct);
                        ////item2
                        Spell spell = SkillBase.GetSpellByID(item2.ProcSpellID);

                        if (spell != null && spell.ID > 0)
                        {

                            t.TempProperties.setProperty(_Proc1, spell.ID);

                        }
                        else
                            t.TempProperties.setProperty(_Proc1, 0);

                        ///////////////////////////////////////////////////////////////////////////

                        if (newUnique != null)
                        {
                            newUnique.ProcSpellID = item2.ProcSpellID;

                        }



                        string procitem1 = "";
                        string procitem2 = "";

                        Spell spell1 = SkillBase.GetSpellByID(item1.ProcSpellID1);

                        if (spell1 != null && spell1.ID > 0)
                        {
                            procitem1 = spell1.Name;

                        }
                        else
                            procitem1 = "No proc available";


                        Spell spell2 = SkillBase.GetSpellByID(item2.ProcSpellID1);

                        if (spell2 != null && spell2.ID > 0)
                        {
                            procitem2 = spell2.Name;

                        }
                        else
                            procitem2 = "No proc available";




                        t.Out.SendMessage(Name + " says, do You like to use the [secound proc]: " + procitem2 + " from the secound item or [leave secound proc]:  " + procitem1 + "?", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        break;

                    }


                //proc1
                case "secound proc":
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        Emote(eEmote.Induct);
                        ////item2 zweiter proc
                        Spell spell = SkillBase.GetSpellByID(item2.ProcSpellID1);

                        if (spell != null && spell.ID > 0)
                        {

                            t.TempProperties.setProperty(_Proc2, spell.ID);

                        }
                        else
                            t.TempProperties.setProperty(_Proc2, 0);
                        /////////////////////////////////////////////////////////////////////////////////////////


                        string procitem1 = "";
                        string procitem2 = "";

                        Spell spell1 = SkillBase.GetSpellByID(item1.SpellID);

                        if (spell1 != null && spell1.ID > 0)
                        {
                            procitem1 = spell1.Name;

                        }
                        else
                            procitem1 = "No proc available";


                        Spell spell2 = SkillBase.GetSpellByID(item2.SpellID);

                        if (spell2 != null && spell2.ID > 0)
                        {
                            procitem2 = spell2.Name;

                        }
                        else
                            procitem2 = "No proc available";





                        t.Out.SendMessage(Name + " says, do you like to use the first spell from the secound item [first spell]:  " + procitem2 + " or [leave first spell]:  " + procitem1 + " ?", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        break;
                    }
                case "leave secound proc":
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        Emote(eEmote.Induct);
                        ////item1 zweiter proc
                        Spell spell = SkillBase.GetSpellByID(item1.ProcSpellID1);

                        if (spell != null && spell.ID > 0)
                        {

                            t.TempProperties.setProperty(_Proc2, spell.ID);

                        }
                        else
                            t.TempProperties.setProperty(_Proc2, 0);

                        /////////////////////////////////////////////////////////////////



                        if (newUnique != null)
                        {
                            newUnique.ProcSpellID1 = item1.ProcSpellID1;

                        }


                        string spellitem1 = "";
                        string spellitem2 = "";

                        Spell spell1 = SkillBase.GetSpellByID(item1.SpellID);

                        if (spell1 != null && spell1.ID > 0)
                        {
                            spellitem1 = spell1.Name;
                        }
                        else
                            spellitem1 = "No Spell available";

                        Spell spell2 = SkillBase.GetSpellByID(item2.SpellID);

                        if (spell2 != null && spell2.ID > 0)
                        {
                            spellitem2 = spell2.Name;

                        }
                        else
                            spellitem2 = "No Spell available";



                        t.Out.SendMessage(Name + " says, do you like to use the first spell from the secound item [first spell]:  " + spellitem2 + " or [leave first spell]:  " + spellitem1 + " ?", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        break;
                    }

                case "first spell":
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        Emote(eEmote.Induct);
                        ////item2 erster spell
                        Spell spell = SkillBase.GetSpellByID(item2.SpellID);

                        if (spell != null && spell.ID > 0)
                        {

                            t.TempProperties.setProperty(_Spell1, spell.ID);

                        }
                        else
                            t.TempProperties.setProperty(_Spell1, 0);
                        //////////////////////////////////////////////////////////////////////////////

                        if (newUnique != null)
                        {
                            newUnique.SpellID = item2.SpellID;
                            newUnique.Charges = item2.Charges;
                            newUnique.MaxCharges = item2.MaxCharges;

                        }


                        string spellitem1 = "";
                        string spellitem2 = "";

                        Spell spell1 = SkillBase.GetSpellByID(item1.SpellID1);

                        if (spell1 != null)
                        {

                            spellitem1 = spell1.Name;
                        }
                        else

                            spellitem1 = "No Spell available";

                        Spell spell2 = SkillBase.GetSpellByID(item2.SpellID1);

                        if (spell2 != null)
                        {

                            spellitem2 = spell2.Name;
                        }
                        else
                            spellitem2 = "No Spell available";



                        t.Out.SendMessage(Name + " says, do you like to use the secound spell from the secound item [secound spell]: " + spellitem2 + " or [leave secound spell]: " + spellitem1 + " ?", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        break;
                    }

                case "leave first spell":
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        Emote(eEmote.Induct);
                        ////item1 erster spell
                        Spell spell = SkillBase.GetSpellByID(item1.SpellID);

                        if (spell != null && spell.ID > 0)
                        {

                            t.TempProperties.setProperty(_Spell1, spell.ID);

                        }
                        else
                            t.TempProperties.setProperty(_Spell1, 0);
                        //////////////////////////////////////////////////////////////////////////////


                        if (newUnique != null)
                        {
                            newUnique.SpellID = item1.SpellID;
                            newUnique.Charges = item1.Charges;
                            newUnique.MaxCharges = item1.MaxCharges;
                        }


                        string spellitem1 = "";
                        string spellitem2 = "";

                        Spell spell1 = SkillBase.GetSpellByID(item1.SpellID1);

                        if (spell1 != null)
                        {

                            spellitem1 = spell1.Name;
                        }
                        else
                            spellitem1 = "No Spell available";

                        Spell spell2 = SkillBase.GetSpellByID(item2.SpellID1);

                        if (spell2 != null)
                        {

                            spellitem2 = spell2.Name;
                        }
                        else
                            spellitem2 = "No Spell available";




                        t.Out.SendMessage(Name + " says, do you like to use the secound spell from the secound item [secound spell]: " + spellitem2 + " or [leave secound spell]: " + spellitem1 + "?", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        break;
                    }

                case "secound spell":
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        Emote(eEmote.Induct);
                        ////item2 zweiter spell
                        Spell spell = SkillBase.GetSpellByID(item2.SpellID1);

                        if (spell != null && spell.ID > 0)
                        {
                            
                            t.TempProperties.setProperty(_Spell2, spell.ID);
                           
                        }
                        else
                            t.TempProperties.setProperty(_Spell2, 0);
                        //////////////////////////////////////////////////////////////////////////////




                        if (newUnique != null)
                        {

                            newUnique.SpellID1 = item2.SpellID1;
                            newUnique.Charges1 = item2.Charges1;
                            newUnique.MaxCharges1 = item2.MaxCharges1;

                        }
                        //log.ErrorFormat("benutze spell1!!!!!!!!1111: {0}", item2.SpellID1);

                        /*
                        ItemUnique Unique = CreateUniqueItem(item1, item2, t, newUnique);



                        t.TempProperties.getProperty<string>(_Spell3)
                        t.TempProperties.getProperty<string>(_Spell3)


                            t.Out.SendMessage(Name + " says, you prever Proc1: " + ProcName1Item1 + "", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            t.Out.SendMessage(Name + " says, you prever Proc2: " + ProcName2Item1 + "", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            t.Out.SendMessage(Name + " says, you prever Spell1: " + SpellName1Item1 + "", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            t.Out.SendMessage(Name + " says, you prever spell2: " + SpellName2Item1 + "", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        }
                        */


                        //t.Out.SendMessage(Name + " says, your new item is ready to generate, sould i [start generate] or [abort] ?", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        t.Out.SendMessage(Name + " says, your new item is ready to generate, [start generate] or [abort] ?", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        break;
                    }
                case "leave secound spell":
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        Emote(eEmote.Induct);
                        ////item2 zweiter spell
                        Spell spell = SkillBase.GetSpellByID(item1.SpellID1);

                        if (spell != null && spell.ID > 0)
                        {

                            t.TempProperties.setProperty(_Spell2, spell.ID);

                        }
                        else
                            t.TempProperties.setProperty(_Spell2, 0);
                        //////////////////////////////////////////////////////////////////////////////



                        if (newUnique != null)
                        {
                            newUnique.SpellID1 = item1.SpellID1;
                            newUnique.Charges1 = item1.Charges1;
                            newUnique.MaxCharges1 = item1.MaxCharges1;

                        }


                        /*
                    


                            t.Out.SendMessage(Name + " says, you prever Proc1: " + ProcName1Item1 + "", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            t.Out.SendMessage(Name + " says, you prever Proc2: " + ProcName2Item1 + "", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            t.Out.SendMessage(Name + " says, you prever Spell1: " + SpellName1Item1 + "", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            t.Out.SendMessage(Name + " says, you prever spell2: " + SpellName2Item1 + "", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        */

                        t.Out.SendMessage(Name + " says, your new item is ready to generate, [start generate] or [abort] ?", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        break;
                    }
/*
                case "See Overview":
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        Emote(eEmote.Induct);
                        /*
                        t.TempProperties.getProperty<ushort>(_Proc1);
                        t.TempProperties.getProperty<ushort>(_Proc2);
                        t.TempProperties.getProperty<ushort>(_Spell1);
                        t.TempProperties.getProperty<ushort>(_Spell2);
                        */

                        /*
                        log.ErrorFormat("proc 1 = {0}", t.TempProperties.getProperty<ushort>(_Proc1));
                        log.ErrorFormat("proc 2= {0}", t.TempProperties.getProperty<ushort>(_Proc2));
                        log.ErrorFormat("spell 1 = {0}", t.TempProperties.getProperty<ushort>(_Spell1));
                        log.ErrorFormat("spell 2 = {0}", t.TempProperties.getProperty<ushort>(_Spell2));
                        */
                        /*
                        string Procitem1 = "";
                        string Procitem2 = "";
                        string spellitem1 = "";
                        string spellitem2 = "";

                       

                        Spell procspell = SkillBase.GetSpellByID(t.TempProperties.getProperty<ushort>(_Proc1));

                        if (procspell != null && procspell.ID > 0)
                        {

                            Procitem1 = procspell.Name;

                        }
                        else
                            Procitem1 = "No Spell available";



                        Spell procspell2 = SkillBase.GetSpellByID(t.TempProperties.getProperty<ushort>(_Proc2));

                        if (procspell2 != null && procspell2.ID > 0)
                        {
                            Procitem2 = procspell2.Name;

                        }
                        else
                            Procitem2 = "No Spell available";


                        Spell spell1 = SkillBase.GetSpellByID(t.TempProperties.getProperty<ushort>(_Spell1));

                        if (spell1 != null && spell1.ID > 0)
                        {

                            spellitem1 = spell1.Name;

                        }
                        else
                            spellitem1 = "No Spell available";




                        



                        Spell spell2 = SkillBase.GetSpellByID(t.TempProperties.getProperty<ushort>(_Spell2));

                        if (spell2 != null && spell2.ID > 0)
                        {
                            log.ErrorFormat("Setze spell _Spell2 id {0}: name: {1}, spell.ID: {2}", t.TempProperties.getProperty<ushort>(_Spell2), spell2.Name, spell2.ID);
                            spellitem2 = spell2.Name;

                        }
                        else
                            spellitem2 = "No Spell available";


                        t.Out.SendMessage(Name + " says, you prefer Proc1: " + Procitem1 + "", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        t.Out.SendMessage(Name + " says, you prefer Proc2: " + Procitem2 + "", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        t.Out.SendMessage(Name + " says, you prefer Spell1: " + spellitem1 + "", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        t.Out.SendMessage(Name + " says, you prefer spell2: " + spellitem2 + "", eChatType.CT_System, eChatLoc.CL_PopupWindow);


                        t.Out.SendMessage(Name + " says, your new item is ready to generate, sould i [start generate] or [abort] ?", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        break;
                    }
            */
                case "start generate":
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        if (t.GetCurrentMoney() < (3000 * item1.Level))
                        {

                            t.Out.SendMessage(Name + " says, Sorry you has not enough money for my service. [abort]?", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            break;

                        }
                        else
                            GamePlayerUtils.SendClearPopupWindow(t);
                            t.Out.SendMessage("You are paying " + Money.GetShortString(3000* item1.Level) + "  to " + Name + "", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                           
                        
                        t.RemoveMoney(3000 * item1.Level);//15g at level 50


                        
                        if (t.TempProperties.getProperty<InventoryItem>(FirstItem) != null && t.TempProperties.getProperty<InventoryItem>(SecoundItem) != null)
                        {

                           
                            if (CreateUniqueItem(item1, item2, t, newUnique) != null)
                            {


                                ItemUnique Unique = CreateUniqueItem(item1, item2, t, newUnique);

                                if (t.TempProperties.getProperty<int>(_ItemModel) > 0)
                                {
                                    Unique.Model = t.TempProperties.getProperty<int>(_ItemModel);
                                   // log.ErrorFormat("Item model:{0}", Unique.Model);
                                }

                                if (t.TempProperties.getProperty<ushort>(_Proc1) > 0 && t.TempProperties.getProperty<ushort>(_Proc1) != 6461)
                                {
                                    Unique.ProcSpellID = t.TempProperties.getProperty<ushort>(_Proc1);
                                    
                                   // log.ErrorFormat("ProcSpellID:{0} ", Unique.ProcSpellID);
                                }

                                if (t.TempProperties.getProperty<ushort>(_Proc2) > 0 && t.TempProperties.getProperty<ushort>(_Proc2) != 6461)
                                {
                                    Unique.ProcSpellID1 = t.TempProperties.getProperty<ushort>(_Proc2);
                                   
                                  //  log.ErrorFormat("ProcSpellID1:{0} ", Unique.ProcSpellID1);
                                }

                                if (t.TempProperties.getProperty<ushort>(_Spell1) > 0 && t.TempProperties.getProperty<ushort>(_Spell1) != 6461)
                                {
                                    Unique.SpellID = t.TempProperties.getProperty<ushort>(_Spell1);
                                    Unique.Charges = 10;
                                    Unique.MaxCharges = 10;
                                    //log.ErrorFormat("Item SpellID:{0} ", Unique.Model);
                                }
                                if (t.TempProperties.getProperty<ushort>(_Spell2) > 0 && t.TempProperties.getProperty<ushort>(_Spell1) != 6461)
                                {
                                    Unique.SpellID1 = t.TempProperties.getProperty<ushort>(_Spell2);
                                    Unique.Charges1 = 10;
                                    Unique.MaxCharges1 = 10;
                                   // log.ErrorFormat("Item SpellID1:{0} ", Unique.SpellID1);
                                }


                                GameServer.Database.AddObject(Unique);
                                InventoryItem invitem = GameInventoryItem.Create<ItemUnique>(Unique);
                                t.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, invitem);
                                t.Out.SendMessage(Name + "says, Congratulations "+t.Name+"  here, take your unique item! " + invitem.Name, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                Emote(eEmote.Induct);
                                // t.SaveIntoDatabase();

                                t.TempProperties.setProperty(FirstItem, null);
                                t.TempProperties.removeProperty(FirstItem);
                                t.TempProperties.setProperty(SecoundItem, null);
                                t.TempProperties.removeProperty(SecoundItem);

                                t.TempProperties.removeProperty(_ItemModel);
                                t.TempProperties.removeProperty(_Proc1);
                                t.TempProperties.removeProperty(_Proc2);
                                t.TempProperties.removeProperty(_Spell1);
                                t.TempProperties.removeProperty(_Spell2);
                            }
                        }
                        break;
                    }
                default: break;
            }
            return true;
        }


        public enum eGenerateType
        {
            Weapon,
            Armor,
            Magical,
            None,
        }

        private static bool StatIsValidForObjectType(ItemUnique item, eProperty property)
        {
            switch ((eObjectType)item.Object_Type)
            {
                case eObjectType.Magical: return StatIsValidForRealm((eRealm)item.Realm, property);
                case eObjectType.Cloth:
                case eObjectType.Leather:
                case eObjectType.Studded:
                case eObjectType.Reinforced:
                case eObjectType.Chain:
                case eObjectType.Scale:
                case eObjectType.Plate: return StatIsValidForArmor(item, property);
                case eObjectType.Axe:
                case eObjectType.Blades:
                case eObjectType.Blunt:
                case eObjectType.CelticSpear:
                case eObjectType.CompositeBow:
                case eObjectType.Crossbow:
                case eObjectType.CrushingWeapon:
                case eObjectType.Fired:
                case eObjectType.Flexible:
                case eObjectType.Hammer:
                case eObjectType.HandToHand:
                case eObjectType.Instrument:
                case eObjectType.LargeWeapons:
                case eObjectType.LeftAxe:
                case eObjectType.Longbow:
                case eObjectType.Piercing:
                case eObjectType.PolearmWeapon:
                case eObjectType.RecurvedBow:
                case eObjectType.Scythe:
                case eObjectType.Shield:
                case eObjectType.SlashingWeapon:
                case eObjectType.Spear:
                case eObjectType.Staff:
                case eObjectType.Sword:
                case eObjectType.ThrustWeapon:
                case eObjectType.FistWraps: //Maulers
                case eObjectType.MaulerStaff: //Maulers
                case eObjectType.TwoHandedWeapon: return StatIsValidForWeapon(item, property);
            }
            return true;
        }

        private static bool SkillIsValidForObjectType(ItemUnique item, eProperty property)
        {
            switch ((eObjectType)item.Object_Type)
            {
                case eObjectType.Magical: return true;
                case eObjectType.Cloth:
                case eObjectType.Leather:
                case eObjectType.Studded:
                case eObjectType.Reinforced:
                case eObjectType.Chain:
                case eObjectType.Scale:
                case eObjectType.Plate: return SkillIsValidForArmor(item, property);
                case eObjectType.Axe:
                case eObjectType.Blades:
                case eObjectType.Blunt:
                case eObjectType.CelticSpear:
                case eObjectType.CompositeBow:
                case eObjectType.Crossbow:
                case eObjectType.CrushingWeapon:
                case eObjectType.Fired:
                case eObjectType.Flexible:
                case eObjectType.Hammer:
                case eObjectType.HandToHand:
                case eObjectType.Instrument:
                case eObjectType.LargeWeapons:
                case eObjectType.LeftAxe:
                case eObjectType.Longbow:
                case eObjectType.Piercing:
                case eObjectType.PolearmWeapon:
                case eObjectType.RecurvedBow:
                case eObjectType.Scythe:
                case eObjectType.Shield:
                case eObjectType.SlashingWeapon:
                case eObjectType.Spear:
                case eObjectType.Staff:
                case eObjectType.Sword:
                case eObjectType.ThrustWeapon:
                case eObjectType.MaulerStaff:
                case eObjectType.FistWraps:
                case eObjectType.TwoHandedWeapon: return SkillIsValidForWeapon(item, property);
            }
            return true;
        }

        private static bool SkillIsValidForArmor(ItemUnique item, eProperty property)
        {
            int level = item.Level;
            eRealm realm = (eRealm)item.Realm;
            eObjectType type = (eObjectType)item.Object_Type;

            switch (property)
            {
                case eProperty.Skill_Augmentation:
                    {
                        if (level < 10)
                        {
                            if (type == eObjectType.Leather)
                                return true;
                            return false;
                        }
                        else if (level < 20)
                        {
                            if (type == eObjectType.Studded)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Chain)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Axe:
                    {
                        if (type == eObjectType.Leather || type == eObjectType.Studded)
                            return true;
                        else if (type == eObjectType.Chain && level >= 10)
                            return true;

                        return false;
                    }
                case eProperty.Skill_Battlesongs:
                    {
                        if (level < 20)
                        {
                            if (type == eObjectType.Studded)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Chain)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Pathfinding:
                case eProperty.Skill_BeastCraft:
                    {
                        if (level < 10)
                        {
                            if (type == eObjectType.Leather)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Studded)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Blades:
                    {
                        if (type == eObjectType.Leather || type == eObjectType.Reinforced || type == eObjectType.Scale)
                            return true;
                        return false;
                    }
                case eProperty.Skill_Blunt:
                    {
                        if (type == eObjectType.Leather && level < 10)
                            return true;
                        else if (type == eObjectType.Reinforced || type == eObjectType.Scale)
                            return true;
                        return false;
                    }
                //Cloth skills
                case eProperty.Skill_Arboreal:
                case eProperty.Skill_Body:
                case eProperty.Skill_BoneArmy:
                case eProperty.Skill_Cold:
                case eProperty.Skill_Creeping:
                case eProperty.Skill_Cursing:
                case eProperty.Skill_Darkness:
                case eProperty.Skill_Death_Servant:
                case eProperty.Skill_DeathSight:
                case eProperty.Skill_Earth:
                case eProperty.Skill_Enchantments:
                case eProperty.Skill_EtherealShriek:
                case eProperty.Skill_Fire:
                case eProperty.Skill_Hexing:
                case eProperty.Skill_Light:
                case eProperty.Skill_Mana:
                case eProperty.Skill_Matter:
                case eProperty.Skill_Mentalism:
                case eProperty.Skill_Mind:
                case eProperty.Skill_Pain_working:
                case eProperty.Skill_PhantasmalWail:
                case eProperty.Skill_Runecarving:
                case eProperty.Skill_SpectralForce:
                case eProperty.Skill_Spirit:
                case eProperty.Skill_Summoning:
                case eProperty.Skill_Suppression:
                case eProperty.Skill_Verdant:
                case eProperty.Skill_Void:
                case eProperty.Skill_Wind:
                case eProperty.Skill_Witchcraft:
                    {
                        if (type == eObjectType.Cloth)
                            return true;
                        return false;
                    }
                case eProperty.Skill_Celtic_Dual:
                    {
                        if (type == eObjectType.Leather ||
                            type == eObjectType.Reinforced)
                            return true;
                        return false;
                    }
                case eProperty.Skill_Celtic_Spear:
                    {
                        if (level < 15)
                        {
                            if (type == eObjectType.Reinforced)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Scale)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Chants:
                    {
                        return false;
                    }
                case eProperty.Skill_Composite:
                case eProperty.Skill_RecurvedBow:
                case eProperty.Skill_Long_bows:
                case eProperty.Skill_Archery:
                    {
                        if (level < 10)
                        {
                            if (type == eObjectType.Leather)
                                return true;

                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Studded || type == eObjectType.Reinforced)
                                return true;

                            return false;
                        }
                    }
                case eProperty.Skill_Critical_Strike:
                case eProperty.Skill_Envenom:
                case eProperty.Skill_Dementia:
                case eProperty.Skill_Nightshade:
                case eProperty.Skill_ShadowMastery:
                case eProperty.Skill_VampiiricEmbrace:
                    {
                        if (type == eObjectType.Leather)
                            return true;
                        return false;
                    }
                case eProperty.Skill_Cross_Bows:
                    {
                        if (level < 15)
                        {
                            if (type == eObjectType.Chain)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Plate)
                                return true;
                            return false;
                        }
                    }

                case eProperty.Skill_Crushing:
                    {
                        if (realm == eRealm.Albion && type == eObjectType.Cloth) // heretic
                            return true;

                        if (level < 15)
                        {
                            if (type == eObjectType.Studded)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Chain || type == eObjectType.Plate)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Dual_Wield:
                    {
                        if (level < 20)
                        {
                            if (type == eObjectType.Leather || type == eObjectType.Studded)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Leather || type == eObjectType.Chain)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Enhancement:
                    {
                        // friar
                        if (type == eObjectType.Leather)
                            return true;

                        if (level < 20)
                        {
                            if (type == eObjectType.Studded)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Chain)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Flexible_Weapon:
                    {
                        if (type == eObjectType.Cloth) // Heretic
                            return true;

                        if (level < 10)
                        {
                            if (type == eObjectType.Studded)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Chain)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Hammer:
                    {
                        if (level < 10)
                        {
                            if (type == eObjectType.Leather)
                                return true;
                            return false;
                        }
                        if (level < 20)
                        {
                            if (type == eObjectType.Studded)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Chain)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_HandToHand:
                    {
                        if (type == eObjectType.Studded)
                            return true;
                        return false;
                    }
                case eProperty.Skill_Instruments:
                    {
                        if (level < 10)
                        {
                            if (type == eObjectType.Leather)
                                return true;
                            return false;
                        }
                        else if (level < 20)
                        {
                            if (type == eObjectType.Studded)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Chain)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Large_Weapon:
                    {
                        if (level < 15)
                        {
                            if (type == eObjectType.Reinforced)
                                return true;

                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Scale)
                                return true;

                            return false;
                        }
                    }
                case eProperty.Skill_Left_Axe:
                    {
                        if (type == eObjectType.Leather || type == eObjectType.Studded)
                            return true;
                        break;
                    }
                case eProperty.Skill_Music:
                    {
                        if (level < 15)
                        {
                            if (type == eObjectType.Leather)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Reinforced)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Nature:
                    {
                        if (level < 10)
                        {
                            if (type == eObjectType.Leather)
                                return true;
                            return false;
                        }
                        else if (level < 20)
                        {
                            if (type == eObjectType.Reinforced)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Scale)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Nurture:
                case eProperty.Skill_Regrowth:
                    {
                        if (level < 10)
                        {
                            if (type == eObjectType.Leather)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Reinforced || type == eObjectType.Scale)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_OdinsWill:
                    {
                        if (level < 10)
                        {
                            if (type == eObjectType.Studded)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Chain)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Pacification:
                    {
                        if (level < 10)
                        {
                            if (type == eObjectType.Leather)
                                return true;
                            return false;
                        }
                        else if (level < 20)
                        {
                            if (type == eObjectType.Studded)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Chain)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Parry:
                    {
                        if (type == eObjectType.Cloth && realm == eRealm.Hibernia && level >= 5)
                            return true;
                        else if (realm == eRealm.Hibernia && level < 2)
                            return false;
                        else if (realm == eRealm.Albion && level < 5)
                            return false;
                        else if (realm == eRealm.Albion && level < 10 && type == eObjectType.Studded)
                            return true;
                        else if (realm == eRealm.Albion && level >= 10 && (type == eObjectType.Leather || type == eObjectType.Chain || type == eObjectType.Plate))
                            return true;
                        else if (realm == eRealm.Hibernia && level < 20 && type == eObjectType.Reinforced)
                            return true;
                        else if (realm == eRealm.Hibernia && level >= 15 && type == eObjectType.Scale)
                            return true;
                        else if (realm == eRealm.Midgard && (type == eObjectType.Studded || type == eObjectType.Chain))
                            return true;

                        break;
                    }
                case eProperty.Skill_Piercing:
                    {
                        if (type == eObjectType.Leather || type == eObjectType.Reinforced)
                            return true;
                        return false;
                    }
                case eProperty.Skill_Polearms:
                    {
                        if (level < 5 && type == eObjectType.Studded)
                        {
                            return true;
                        }
                        else if (level < 15)
                        {
                            if (type == eObjectType.Chain)
                                return true;

                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Plate)
                                return true;

                            return false;
                        }
                    }
                case eProperty.Skill_Rejuvenation:
                    {
                        if (type == eObjectType.Cloth)
                            return true;
                        else if (type == eObjectType.Leather)
                            return true;
                        else if (type == eObjectType.Studded && level >= 10 && level < 20)
                            return true;
                        else if (type == eObjectType.Chain && level >= 20)
                            return true;
                        break;
                    }
                case eProperty.Skill_Savagery:
                    {
                        if (type == eObjectType.Studded)
                            return true;
                        break;
                    }
                case eProperty.Skill_Scythe:
                    {
                        if (type == eObjectType.Cloth)
                            return true;
                        break;
                    }
                case eProperty.Skill_Shields:
                    {
                        if (type == eObjectType.Cloth && realm == eRealm.Albion)
                            return true;
                        else if (type == eObjectType.Studded || type == eObjectType.Chain || type == eObjectType.Reinforced || type == eObjectType.Scale || type == eObjectType.Plate)
                            return true;
                        break;
                    }
                case eProperty.Skill_ShortBow:
                    {
                        return false;
                    }
                case eProperty.Skill_Smiting:
                    {
                        if (type == eObjectType.Leather && level < 10)
                            return true;
                        else if (type == eObjectType.Studded && level < 20)
                            return true;
                        else if (type == eObjectType.Chain && level >= 20)
                            return true;
                        break;
                    }
                case eProperty.Skill_SoulRending:
                    {
                        if (type == eObjectType.Studded && level < 10)
                            return true;
                        else if (type == eObjectType.Chain && level >= 10)
                            return true;
                        break;
                    }
                case eProperty.Skill_Spear:
                    {
                        if (type == eObjectType.Leather && level < 10)
                            return true;
                        else if (type == eObjectType.Studded)
                            return true;
                        else if (type == eObjectType.Chain && level >= 10)
                            return true;
                        break;
                    }
                case eProperty.Skill_Staff:
                    {
                        if (type == eObjectType.Leather && realm == eRealm.Albion)
                            return true;
                        break;
                    }
                case eProperty.Skill_Stealth:
                    {
                        if (type == eObjectType.Leather || type == eObjectType.Studded || type == eObjectType.Reinforced)
                            return true;
                        else if (realm == eRealm.Albion && level >= 20 && type == eObjectType.Chain)
                            return true;
                        break;
                    }
                case eProperty.Skill_Stormcalling:
                    {
                        if (type == eObjectType.Studded && level < 10)
                            return true;
                        else if (type == eObjectType.Chain && level >= 10)
                            return true;
                        break;
                    }
                case eProperty.Skill_Subterranean:
                    {
                        if (type == eObjectType.Leather && level < 10)
                            return true;
                        else if (type == eObjectType.Studded && level < 20)
                            return true;
                        else if (type == eObjectType.Chain && level >= 20)
                            return true;
                        break;
                    }
                case eProperty.Skill_Sword:
                    {
                        if (type == eObjectType.Studded || type == eObjectType.Chain)
                            return true;
                        break;
                    }
                case eProperty.Skill_Slashing:
                    {
                        if (type == eObjectType.Leather || type == eObjectType.Studded || type == eObjectType.Chain || type == eObjectType.Plate)
                            return true;
                        break;
                    }
                case eProperty.Skill_Thrusting:
                    {
                        if (type == eObjectType.Leather || type == eObjectType.Studded || type == eObjectType.Chain || type == eObjectType.Plate)
                            return true;
                        break;
                    }
                case eProperty.Skill_Two_Handed:
                    {
                        if (type == eObjectType.Studded && level < 10)
                            return true;
                        else if (type == eObjectType.Chain && level < 20)
                            return true;
                        else if (type == eObjectType.Plate)
                            return true;
                        break;
                    }
                case eProperty.Skill_Valor:
                    {
                        if (type == eObjectType.Reinforced && level < 20)
                            return true;
                        else if (type == eObjectType.Scale)
                            return true;
                        break;
                    }
                case eProperty.AllArcherySkills:
                    {
                        if (type == eObjectType.Leather && level < 10)
                            return true;
                        else if (level >= 10 && (type == eObjectType.Reinforced || type == eObjectType.Studded))
                            return true;

                        break;
                    }
                case eProperty.AllDualWieldingSkills:
                    {
                        //Dualwielders are always above level 4 and can wear better than cloth from the start.
                        if (type == eObjectType.Cloth)
                            return false;
                        //mercs are the only dualwielder who can wear chain
                        else if (realm == eRealm.Albion && type == eObjectType.Studded && level < 10)
                            return true;
                        else if (realm == eRealm.Albion && type == eObjectType.Chain)
                            return true;
                        //all assassins wear leather, blademasters and zerks wear studded.
                        else if (type == eObjectType.Leather || type == eObjectType.Reinforced || (type == eObjectType.Studded && realm == eRealm.Midgard))
                            return true;
                        break;
                    }
                case eProperty.AllMagicSkills:
                    {
                        // not for scouts
                        if (realm == eRealm.Albion && type == eObjectType.Studded && level >= 20)
                            return false;
                        // Paladins can't use + magic skills
                        if (realm == eRealm.Albion && type == eObjectType.Plate)
                            return false;

                        return true;
                    }
                case eProperty.AllMeleeWeaponSkills:
                    {
                        if (realm == eRealm.Midgard && type == eObjectType.Cloth)
                            return false;
                        else if (level >= 5)
                            return true;

                        break;
                    }
                case eProperty.AllSkills:
                    {
                        return true;
                    }
                case eProperty.Skill_Power_Strikes:
                case eProperty.Skill_Magnetism:
                case eProperty.Skill_MaulerStaff:
                case eProperty.Skill_Aura_Manipulation:
                case eProperty.Skill_FistWraps:
                    {
                        //Maulers
                        if (type == eObjectType.Leather) //Maulers can only wear leather.
                            return true;

                        break;
                    }

            }

            return false;
        }

        private static bool SkillIsValidForWeapon(ItemUnique item, eProperty property)
        {
            int level = item.Level;
            eRealm realm = (eRealm)item.Realm;
            eObjectType type = (eObjectType)item.Object_Type;

            switch (property)
            {
                case eProperty.Skill_Arboreal:
                case eProperty.Skill_Body:
                case eProperty.Skill_BoneArmy:
                case eProperty.Skill_Cold:
                case eProperty.Skill_Creeping:
                case eProperty.Skill_Cursing:
                case eProperty.Skill_Darkness:
                case eProperty.Skill_Death_Servant:
                case eProperty.Skill_DeathSight:
                case eProperty.Skill_Earth:
                case eProperty.Skill_Enchantments:
                case eProperty.Skill_EtherealShriek:
                case eProperty.Skill_Fire:
                case eProperty.Skill_Hexing:
                case eProperty.Skill_Light:
                case eProperty.Skill_Mana:
                case eProperty.Skill_Matter:
                case eProperty.Skill_Mentalism:
                case eProperty.Skill_Mind:
                case eProperty.Skill_Pain_working:
                case eProperty.Skill_PhantasmalWail:
                case eProperty.Skill_Runecarving:
                case eProperty.Skill_SpectralForce:
                case eProperty.Skill_Spirit:
                case eProperty.Skill_Summoning:
                case eProperty.Skill_Suppression:
                case eProperty.Skill_Verdant:
                case eProperty.Skill_Void:
                case eProperty.Skill_Wind:
                case eProperty.Skill_Witchcraft:
                    {
                        if (type == eObjectType.Staff && item.Description != "friar")
                            return true;
                        break;
                    }
                //healer things
                case eProperty.Skill_Smiting:
                    {
                        if ((type == eObjectType.Shield && item.Type_Damage < 3) || type == eObjectType.CrushingWeapon)
                            return true;
                        break;
                    }
                case eProperty.Skill_Enhancement:
                case eProperty.Skill_Rejuvenation:
                    {
                        if ((type == eObjectType.Staff && item.Description == "friar") || (type == eObjectType.Shield && item.Type_Damage < 3) || type == eObjectType.CrushingWeapon)
                            return true;
                        break;
                    }
                case eProperty.Skill_Augmentation:
                case eProperty.Skill_Mending:
                case eProperty.Skill_Subterranean:
                case eProperty.Skill_Nurture:
                case eProperty.Skill_Nature:
                case eProperty.Skill_Regrowth:
                    {
                        if (type == eObjectType.Hammer || (type == eObjectType.Shield && item.Type_Damage < 2) || type == eObjectType.Blunt || type == eObjectType.Blades)
                            return true;
                        break;
                    }
                //archery things
                case eProperty.Skill_Archery:
                    if (type == eObjectType.CompositeBow || type == eObjectType.RecurvedBow || type == eObjectType.Longbow)
                        return true;
                    break;
                case eProperty.Skill_Composite:
                    {
                        if (type == eObjectType.CompositeBow)
                            return true;
                        break;
                    }
                case eProperty.Skill_RecurvedBow:
                    {
                        if (type == eObjectType.RecurvedBow)
                            return true;
                        break;
                    }
                case eProperty.Skill_Long_bows:
                    {
                        if (type == eObjectType.Longbow)
                            return true;
                        break;
                    }
                //other specifics
                case eProperty.Skill_Staff:
                    {
                        if (type == eObjectType.Staff && item.Description == "friar")
                            return true;
                        break;
                    }
                case eProperty.Skill_Axe:
                    {
                        if (type == eObjectType.Axe || type == eObjectType.LeftAxe || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Battlesongs:
                    {
                        if (type == eObjectType.Sword || type == eObjectType.Axe || type == eObjectType.Hammer || (type == eObjectType.Shield && item.Type_Damage < 3))
                            return true;
                        break;
                    }
                case eProperty.Skill_BeastCraft:
                    {
                        if (type == eObjectType.Spear)
                            return true;
                        break;
                    }
                case eProperty.Skill_Blades:
                    {
                        if (type == eObjectType.Blades || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Blunt:
                    {
                        if (type == eObjectType.Blunt || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Celtic_Dual:
                    {
                        if (type == eObjectType.Piercing || type == eObjectType.Blades || type == eObjectType.Blunt)
                            return true;
                        break;
                    }
                case eProperty.Skill_Celtic_Spear:
                    {
                        if (type == eObjectType.CelticSpear)
                            return true;
                        break;
                    }
                case eProperty.Skill_Chants:
                    {
                        return false;
                    }
                case eProperty.Skill_Critical_Strike:
                    {
                        if (type == eObjectType.Piercing || type == eObjectType.SlashingWeapon || type == eObjectType.ThrustWeapon || type == eObjectType.Blades || type == eObjectType.Sword || type == eObjectType.Axe || type == eObjectType.LeftAxe)
                            return true;
                        break;
                    }
                case eProperty.Skill_Cross_Bows:
                    {
                        if (type == eObjectType.Crossbow)
                            return true;
                        break;
                    }
                case eProperty.Skill_Crushing:
                    {
                        if (type == eObjectType.CrushingWeapon ||
                            ((type == eObjectType.TwoHandedWeapon || type == eObjectType.PolearmWeapon) && item.Type_Damage == (int)eDamageType.Crush) ||
                            type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Dual_Wield:
                    {
                        if (type == eObjectType.SlashingWeapon || type == eObjectType.ThrustWeapon || type == eObjectType.CrushingWeapon)
                            return true;
                        break;
                    }
                case eProperty.Skill_Envenom:
                    {
                        if (type == eObjectType.SlashingWeapon || type == eObjectType.ThrustWeapon)
                            return true;
                        break;
                    }
                case eProperty.Skill_Flexible_Weapon:
                    {
                        if (type == eObjectType.Flexible || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Hammer:
                    {
                        if (type == eObjectType.Hammer || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_HandToHand:
                    {
                        if (type == eObjectType.HandToHand)
                            return true;
                        break;
                    }
                case eProperty.Skill_Instruments:
                    {
                        if (type == eObjectType.Instrument || (type == eObjectType.Shield && item.Type_Damage == 1))
                            return true;
                        break;
                    }
                case eProperty.Skill_Large_Weapon:
                    {
                        if (type == eObjectType.LargeWeapons)
                            return true;
                        break;
                    }
                case eProperty.Skill_Left_Axe:
                    {
                        if (type == eObjectType.Axe || type == eObjectType.LeftAxe)
                            return true;
                        break;
                    }
                case eProperty.Skill_Music:
                    {
                        if (type == eObjectType.Blades || type == eObjectType.Blunt || (type == eObjectType.Shield && item.Type_Damage == 1) || type == eObjectType.Instrument)
                            return true;
                        break;
                    }
                case eProperty.Skill_Nightshade:
                    {
                        if (type == eObjectType.Blades || type == eObjectType.Piercing || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_OdinsWill:
                    {
                        if (type == eObjectType.Sword || type == eObjectType.Spear || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Pathfinding:
                    {
                        if (type == eObjectType.RecurvedBow || type == eObjectType.Piercing || type == eObjectType.Blades)
                            return true;
                        break;
                    }
                case eProperty.Skill_Piercing:
                    {
                        if (type == eObjectType.Piercing || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Polearms:
                    {
                        if (type == eObjectType.PolearmWeapon)
                            return true;
                        break;
                    }
                case eProperty.Skill_Savagery:
                    {
                        if (type == eObjectType.Sword || type == eObjectType.Axe || type == eObjectType.Hammer || type == eObjectType.HandToHand)
                            return true;
                        break;
                    }
                case eProperty.Skill_Scythe:
                    {
                        if (type == eObjectType.Scythe)
                            return true;
                        break;
                    }

                case eProperty.Skill_VampiiricEmbrace:
                case eProperty.Skill_ShadowMastery:
                    {
                        if (type == eObjectType.Piercing)
                            return true;
                        break;
                    }
                case eProperty.Skill_Shields:
                    {
                        if (type == eObjectType.SlashingWeapon || type == eObjectType.CrushingWeapon || type == eObjectType.ThrustWeapon || type == eObjectType.Blunt || type == eObjectType.Blades || type == eObjectType.Piercing || type == eObjectType.Shield || type == eObjectType.Axe || type == eObjectType.Sword || type == eObjectType.Hammer)
                            return true;
                        break;
                    }
                case eProperty.Skill_ShortBow:
                    {
                        return false;
                    }
                case eProperty.Skill_Slashing:
                    {
                        if (type == eObjectType.SlashingWeapon ||
                            ((type == eObjectType.TwoHandedWeapon || type == eObjectType.PolearmWeapon) && item.Type_Damage == (int)eDamageType.Slash) ||
                            type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_SoulRending:
                    {
                        if (type == eObjectType.SlashingWeapon || type == eObjectType.CrushingWeapon || type == eObjectType.ThrustWeapon || type == eObjectType.Flexible || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Spear:
                    {
                        if (type == eObjectType.Spear)
                            return true;
                        break;
                    }
                case eProperty.Skill_Stealth:
                    {
                        if (type == eObjectType.Longbow || type == eObjectType.RecurvedBow || type == eObjectType.CompositeBow || (realm == eRealm.Albion && type == eObjectType.Shield && item.Type_Damage == 1) || type == eObjectType.Spear || type == eObjectType.Sword || type == eObjectType.Axe || type == eObjectType.LeftAxe || type == eObjectType.SlashingWeapon || type == eObjectType.ThrustWeapon || type == eObjectType.Piercing || type == eObjectType.Blades || (realm == eRealm.Albion && type == eObjectType.Instrument))
                            return true;
                        break;
                    }
                case eProperty.Skill_Stormcalling:
                    {
                        if (type == eObjectType.Sword || type == eObjectType.Axe || type == eObjectType.Hammer || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Sword:
                    {
                        if (type == eObjectType.Sword || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Thrusting:
                    {
                        if (type == eObjectType.ThrustWeapon ||
                            ((type == eObjectType.TwoHandedWeapon || type == eObjectType.PolearmWeapon) && item.Type_Damage == (int)eDamageType.Thrust) ||
                            type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Two_Handed:
                    {
                        if (type == eObjectType.TwoHandedWeapon)
                            return true;
                        break;
                    }
                case eProperty.Skill_Valor:
                    {
                        if (type == eObjectType.Blades || type == eObjectType.Piercing || type == eObjectType.Blunt || type == eObjectType.LargeWeapons || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Thrown_Weapons:
                    {
                        return false;
                    }
                case eProperty.Skill_Pacification:
                    {
                        if (type == eObjectType.Hammer)
                            return true;
                        break;
                    }
                case eProperty.Skill_Dementia:
                    {
                        if (type == eObjectType.Piercing)
                            return true;
                        break;
                    }
                case eProperty.AllArcherySkills:
                    {
                        if (type == eObjectType.CompositeBow || type == eObjectType.Longbow || type == eObjectType.RecurvedBow)
                            return true;
                        break;
                    }
                case eProperty.AllDualWieldingSkills:
                    {
                        if (type == eObjectType.Axe || type == eObjectType.Sword || type == eObjectType.Hammer || type == eObjectType.LeftAxe || type == eObjectType.SlashingWeapon || type == eObjectType.CrushingWeapon || type == eObjectType.ThrustWeapon || type == eObjectType.Piercing || type == eObjectType.Blades || type == eObjectType.Blunt)
                            return true;
                        break;
                    }
                case eProperty.AllMagicSkills:
                    {
                        //scouts, armsmen, paladins, mercs, blademasters, heroes, zerks, warriors do not need this.
                        if (type == eObjectType.Longbow || type == eObjectType.CelticSpear || type == eObjectType.PolearmWeapon || type == eObjectType.TwoHandedWeapon || type == eObjectType.Crossbow || (type == eObjectType.Shield && item.Type_Damage > 2))
                            return false;
                        else
                            return true;
                    }
                case eProperty.AllMeleeWeaponSkills:
                    {

                        if (type == eObjectType.Staff && realm != eRealm.Albion)
                            return false;
                        else if (type == eObjectType.Staff && item.Description != "friar") // do not add if caster staff
                            return false;
                        else if (type == eObjectType.Longbow || type == eObjectType.CompositeBow || type == eObjectType.RecurvedBow || type == eObjectType.Crossbow || type == eObjectType.Fired || type == eObjectType.Instrument)
                            return false;
                        else
                            return true;
                    }
                case eProperty.Skill_Aura_Manipulation: //Maulers
                    {
                        if (type == eObjectType.MaulerStaff || type == eObjectType.FistWraps)
                            return true;
                        break;
                    }
                case eProperty.Skill_Magnetism: //Maulers
                    {
                        if (type == eObjectType.FistWraps || type == eObjectType.MaulerStaff)
                            return true;
                        break;
                    }
                case eProperty.Skill_MaulerStaff: //Maulers
                    {
                        if (type == eObjectType.MaulerStaff)
                            return true;
                        break;
                    }
                case eProperty.Skill_Power_Strikes: //Maulers
                    {
                        if (type == eObjectType.MaulerStaff || type == eObjectType.FistWraps)
                            return true;
                        break;
                    }
                case eProperty.Skill_FistWraps: //Maulers
                    {
                        if (type == eObjectType.FistWraps)
                            return true;
                        break;
                    }
            }
            return false;
        }

        private static bool StatIsValidForRealm(eRealm realm, eProperty property)
        {
            switch (property)
            {
                case eProperty.Piety:
                case eProperty.PieCapBonus:
                    {
                        if (realm == eRealm.Hibernia)
                            return false;
                        break;
                    }
                case eProperty.Empathy:
                case eProperty.EmpCapBonus:
                    {
                        if (realm == eRealm.Midgard || realm == eRealm.Albion)
                            return false;
                        break;
                    }
                case eProperty.Intelligence:
                case eProperty.IntCapBonus:
                    {
                        if (realm == eRealm.Midgard)
                            return false;
                        break;
                    }
            }
            return true;
        }

        private static bool StatIsValidForArmor(ItemUnique item, eProperty property)
        {
            eRealm realm = (eRealm)item.Realm;
            eObjectType type = (eObjectType)item.Object_Type;

            switch (property)
            {
                case eProperty.Intelligence:
                case eProperty.IntCapBonus:
                    {
                        if (realm == eRealm.Midgard)
                            return false;

                        if (realm == eRealm.Hibernia && item.Level < 20 && type != eObjectType.Reinforced && type != eObjectType.Cloth)
                            return false;

                        if (realm == eRealm.Hibernia && item.Level >= 20 && type != eObjectType.Scale && type != eObjectType.Cloth)
                            return false;

                        if (type != eObjectType.Cloth)
                            return false;

                        break;
                    }
                case eProperty.Acuity:
                case eProperty.AcuCapBonus:
                case eProperty.PowerPool:
                case eProperty.PowerPoolCapBonus:
                    {
                        if (realm == eRealm.Albion && item.Level >= 20 && type == eObjectType.Studded)
                            return false;

                        if (realm == eRealm.Midgard && item.Level >= 10 && type == eObjectType.Leather)
                            return false;

                        if (realm == eRealm.Midgard && item.Level >= 20 && type == eObjectType.Studded)
                            return false;

                        break;
                    }
                case eProperty.Piety:
                case eProperty.PieCapBonus:
                    {
                        if (realm == eRealm.Albion)
                        {
                            if (type == eObjectType.Leather && item.Level >= 10)
                                return false;

                            if (type == eObjectType.Studded && item.Level >= 20)
                                return false;

                            if (type == eObjectType.Chain && item.Level < 10)
                                return false;
                        }
                        else if (realm == eRealm.Midgard)
                        {
                            if (type == eObjectType.Leather && item.Level >= 10)
                                return false;

                            if (type == eObjectType.Studded && item.Level >= 20)
                                return false;

                            if (type == eObjectType.Chain && item.Level < 10)
                                return false;
                        }
                        else if (realm == eRealm.Hibernia)
                        {
                            return false;
                        }
                        break;
                    }
                case eProperty.Charisma:
                case eProperty.ChaCapBonus:
                    {
                        if (realm == eRealm.Albion)
                        {
                            if (type == eObjectType.Leather && item.Level >= 10)
                                return false;

                            if (type == eObjectType.Studded && item.Level >= 20)
                                return false;

                            if (type == eObjectType.Chain && item.Level < 20)
                                return false;
                        }
                        if (realm == eRealm.Midgard)
                        {
                            if (type == eObjectType.Studded && item.Level >= 20)
                                return false;

                            if (type == eObjectType.Chain && item.Level < 20)
                                return false;
                        }
                        else if (realm == eRealm.Hibernia)
                        {
                            if (type == eObjectType.Leather && item.Level >= 15)
                                return false;

                            if (type == eObjectType.Reinforced && item.Level < 15)
                                return false;
                        }
                        break;
                    }
                case eProperty.Empathy:
                case eProperty.EmpCapBonus:
                    {
                        if (realm != eRealm.Hibernia)
                            return false;

                        if (type == eObjectType.Leather && item.Level >= 10)
                            return false;

                        if (type == eObjectType.Reinforced && item.Level >= 20)
                            return false;

                        if (type == eObjectType.Scale && item.Level < 20)
                            return false;

                        break;
                    }
            }
            return true;
        }

        private static bool StatIsValidForWeapon(ItemUnique item, eProperty property)
        {
            eRealm realm = (eRealm)item.Realm;
            eObjectType type = (eObjectType)item.Object_Type;

            switch (type)
            {
                case eObjectType.Staff:
                    {
                        if ((property == eProperty.Piety || property == eProperty.PieCapBonus) && realm == eRealm.Hibernia)
                            return false;
                        else if ((property == eProperty.Piety || property == eProperty.PieCapBonus) && realm == eRealm.Albion && item.Description != "friar")
                            return false; // caster staff
                        else if (property == eProperty.Charisma || property == eProperty.Empathy || property == eProperty.ChaCapBonus || property == eProperty.EmpCapBonus)
                            return false;
                        else if ((property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.AcuCapBonus) && item.Description == "friar")
                            return false;
                        break;
                    }

                case eObjectType.Shield:
                    {
                        if ((realm == eRealm.Albion || realm == eRealm.Midgard) && (property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.Empathy || property == eProperty.EmpCapBonus))
                            return false;
                        else if (realm == eRealm.Hibernia && (property == eProperty.Piety || property == eProperty.PieCapBonus))
                            return false;
                        else if ((realm == eRealm.Albion || realm == eRealm.Hibernia) && item.Type_Damage > 1 && (property == eProperty.Charisma || property == eProperty.ChaCapBonus))
                            return false;
                        else if (realm == eRealm.Midgard && item.Type_Damage > 2 && (property == eProperty.Charisma || property == eProperty.ChaCapBonus))
                            return false;
                        else if (item.Type_Damage > 2 && property == eProperty.MaxMana)
                            return false;

                        break;
                    }
                case eObjectType.Blades:
                case eObjectType.Blunt:
                    {
                        if (property == eProperty.Piety || property == eProperty.PieCapBonus)
                            return false;
                        break;
                    }
                case eObjectType.LargeWeapons:
                case eObjectType.Piercing:
                case eObjectType.Scythe:
                    {
                        if (property == eProperty.Piety || property == eProperty.Empathy || property == eProperty.Charisma)
                            return false;
                        break;
                    }
                case eObjectType.CrushingWeapon:
                    {
                        if (property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.Empathy || property == eProperty.EmpCapBonus || property == eProperty.Charisma || property == eProperty.ChaCapBonus)
                            return false;
                        break;
                    }
                case eObjectType.SlashingWeapon:
                case eObjectType.ThrustWeapon:
                case eObjectType.Hammer:
                case eObjectType.Sword:
                case eObjectType.Axe:
                    {
                        if (property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.Empathy || property == eProperty.EmpCapBonus || property == eProperty.AcuCapBonus || property == eProperty.Acuity)
                            return false;
                        break;
                    }
                case eObjectType.TwoHandedWeapon:
                case eObjectType.Flexible:
                    {
                        if (property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.Empathy || property == eProperty.EmpCapBonus || property == eProperty.Charisma || property == eProperty.ChaCapBonus)
                            return false;
                        break;
                    }
                case eObjectType.RecurvedBow:
                case eObjectType.CompositeBow:
                case eObjectType.Longbow:
                case eObjectType.Crossbow:
                case eObjectType.Fired:
                    {
                        if (property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.Empathy || property == eProperty.EmpCapBonus || property == eProperty.Charisma || property == eProperty.ChaCapBonus ||
                            property == eProperty.MaxMana || property == eProperty.PowerPool || property == eProperty.PowerPoolCapBonus || property == eProperty.AcuCapBonus || property == eProperty.Acuity || property == eProperty.Piety || property == eProperty.PieCapBonus)
                            return false;
                        break;
                    }
                case eObjectType.Spear:
                case eObjectType.CelticSpear:
                case eObjectType.LeftAxe:
                case eObjectType.PolearmWeapon:
                case eObjectType.HandToHand:
                case eObjectType.FistWraps: //Maulers
                case eObjectType.MaulerStaff: //Maulers
                    {
                        if (property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.Empathy || property == eProperty.EmpCapBonus || property == eProperty.Charisma || property == eProperty.ChaCapBonus ||
                            property == eProperty.MaxMana || property == eProperty.PowerPool || property == eProperty.PowerPoolCapBonus || property == eProperty.AcuCapBonus || property == eProperty.Acuity || property == eProperty.Piety || property == eProperty.PieCapBonus)
                            return false;
                        break;
                    }
                case eObjectType.Instrument:
                    {
                        if (property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.Empathy || property == eProperty.EmpCapBonus || property == eProperty.Piety || property == eProperty.PieCapBonus)
                            return false;
                        break;
                    }
            }
            return true;
        }

        private static void WriteBonus(ItemUnique item, eProperty property, int amount)
        {
            if (property == eProperty.AllFocusLevels)
            {
                amount = Math.Min(50, amount);
            }

            if (item.Bonus1 == 0)
            {
                item.Bonus1 = amount;
                item.Bonus1Type = (int)property;

                if (property == eProperty.AllFocusLevels)
                    item.Name = "Focus " + item.Name;
            }
            else if (item.Bonus2 == 0)
            {
                item.Bonus2 = amount;
                item.Bonus2Type = (int)property;
            }
            else if (item.Bonus3 == 0)
            {
                item.Bonus3 = amount;
                item.Bonus3Type = (int)property;
            }
            else if (item.Bonus4 == 0)
            {
                item.Bonus4 = amount;
                item.Bonus4Type = (int)property;
            }
            else if (item.Bonus5 == 0)
            {
                item.Bonus5 = amount;
                item.Bonus5Type = (int)property;
            }
        }

      
        private static int GetBonusAmount(eBonusType type, eProperty property, int level)
        {
            switch (type)
            {
                case eBonusType.Focus:
                    {
                        return level;
                    }
                case eBonusType.Resist:
                    {
                        int max = (int)Math.Ceiling((((level / 2.0) + 1) / 4));
                        return Util.Random((int)Math.Ceiling((double)max / 2.0), max);
                    }
                case eBonusType.Skill:
                    {
                        int max = (int)Math.Ceiling(((level / 5.0) + 1) / 3);
                        if (property == eProperty.AllSkills)
                            max = (int)Math.Ceiling((double)max / 2.0);
                        return Util.Random((int)Math.Ceiling((double)max / 2.0), max);
                    }
                case eBonusType.Stat:
                    {
                        if (property == eProperty.MaxHealth)
                        {
                            int max = (int)Math.Ceiling(((double)level * 4.0) / 4);
                            return Util.Random((int)Math.Ceiling((double)max / 2.0), max);
                        }
                        else if (property == eProperty.MaxMana)
                        {
                            int max = (int)Math.Ceiling(((double)level / 2.0 + 1) / 4);
                            return Util.Random((int)Math.Ceiling((double)max / 2.0), max);
                        }
                        else
                        {
                            int max = (int)Math.Ceiling(((double)level * 1.5) / 3);
                            return Util.Random((int)Math.Ceiling((double)max / 2.0), max);
                        }
                    }
                case eBonusType.AdvancedStat:
                    {
                        if (property == eProperty.MaxHealthCapBonus)
                            return Util.Random(5, 25); // cap is 400
                        else if (property == eProperty.PowerPoolCapBonus)
                            return Util.Random(1, 10); // cap is 50
                        else
                            return Util.Random(1, 6); // cap is 26
                    }
            }
            return 1;
        }

        private static eProperty[] StatBonus = new eProperty[]
        {
            eProperty.Strength,
            eProperty.Dexterity,
            eProperty.Constitution,
            eProperty.Quickness,
            eProperty.Intelligence,
            eProperty.Piety,
            eProperty.Empathy,
            eProperty.Charisma,
            eProperty.MaxMana,
            eProperty.MaxHealth,
        };

        private static eProperty[] AdvancedStats = new eProperty[]
        {
            eProperty.PowerPool,
            eProperty.PowerPoolCapBonus,
            eProperty.StrCapBonus,
            eProperty.DexCapBonus,
            eProperty.ConCapBonus,
            eProperty.QuiCapBonus,
            eProperty.IntCapBonus,
            eProperty.PieCapBonus,
            eProperty.EmpCapBonus,
            eProperty.ChaCapBonus,
            eProperty.MaxHealthCapBonus,
			//eProperty.Acuity,  // need duplicate check between base stat and acuity before enabling
			//eProperty.AcuCapBonus,
		};

        private static eProperty[] ResistBonus = new eProperty[]
        {
            eProperty.Resist_Body,
            eProperty.Resist_Cold,
            eProperty.Resist_Crush,
            eProperty.Resist_Energy,
            eProperty.Resist_Heat,
            eProperty.Resist_Matter,
            eProperty.Resist_Slash,
            eProperty.Resist_Spirit,
            eProperty.Resist_Thrust,
        };


        private static eProperty[] AlbSkillBonus = new eProperty[]
        {
            eProperty.Skill_Two_Handed,
            eProperty.Skill_Body,
	        //eProperty.Skill_Chants, // bonus not used
	        eProperty.Skill_Critical_Strike,
            eProperty.Skill_Cross_Bows,
            eProperty.Skill_Crushing,
            eProperty.Skill_Death_Servant,
            eProperty.Skill_DeathSight,
            eProperty.Skill_Dual_Wield,
            eProperty.Skill_Earth,
            eProperty.Skill_Enhancement,
            eProperty.Skill_Envenom,
            eProperty.Skill_Fire,
            eProperty.Skill_Flexible_Weapon,
            eProperty.Skill_Cold,
            eProperty.Skill_Instruments,
            eProperty.Skill_Archery,
            eProperty.Skill_Matter,
            eProperty.Skill_Mind,
            eProperty.Skill_Pain_working,
            eProperty.Skill_Parry,
            eProperty.Skill_Polearms,
            eProperty.Skill_Rejuvenation,
            eProperty.Skill_Shields,
            eProperty.Skill_Slashing,
            eProperty.Skill_Smiting,
            eProperty.Skill_SoulRending,
            eProperty.Skill_Spirit,
            eProperty.Skill_Staff,
            eProperty.Skill_Stealth,
            eProperty.Skill_Thrusting,
            eProperty.Skill_Wind,
            eProperty.Skill_Aura_Manipulation, //Maulers
			eProperty.Skill_FistWraps, //Maulers
			eProperty.Skill_MaulerStaff, //Maulers
			eProperty.Skill_Magnetism, //Maulers
			eProperty.Skill_Power_Strikes, //Maulers
        };


        private static eProperty[] HibSkillBonus = new eProperty[]
        {
            eProperty.Skill_Critical_Strike,
            eProperty.Skill_Envenom,
            eProperty.Skill_Parry,
            eProperty.Skill_Shields,
            eProperty.Skill_Stealth,
            eProperty.Skill_Light,
            eProperty.Skill_Void,
            eProperty.Skill_Mana,
            eProperty.Skill_Blades,
            eProperty.Skill_Blunt,
            eProperty.Skill_Piercing,
            eProperty.Skill_Large_Weapon,
            eProperty.Skill_Mentalism,
            eProperty.Skill_Regrowth,
            eProperty.Skill_Nurture,
            eProperty.Skill_Nature,
            eProperty.Skill_Music,
            eProperty.Skill_Celtic_Dual,
            eProperty.Skill_Celtic_Spear,
            eProperty.Skill_Archery,
            eProperty.Skill_Valor,
            eProperty.Skill_Verdant,
            eProperty.Skill_Creeping,
            eProperty.Skill_Arboreal,
            eProperty.Skill_Scythe,
	        //eProperty.Skill_Nightshade, // bonus not used
	        //eProperty.Skill_Pathfinding, // bonus not used
	        eProperty.Skill_Dementia,
            eProperty.Skill_ShadowMastery,
            eProperty.Skill_VampiiricEmbrace,
            eProperty.Skill_EtherealShriek,
            eProperty.Skill_PhantasmalWail,
            eProperty.Skill_SpectralForce,
            eProperty.Skill_Aura_Manipulation, //Maulers
			eProperty.Skill_FistWraps, //Maulers
			eProperty.Skill_MaulerStaff, //Maulers
			eProperty.Skill_Magnetism, //Maulers
			eProperty.Skill_Power_Strikes, //Maulers
        };

        private static eProperty[] MidSkillBonus = new eProperty[]
        {
            eProperty.Skill_Critical_Strike,
            eProperty.Skill_Envenom,
            eProperty.Skill_Parry,
            eProperty.Skill_Shields,
            eProperty.Skill_Stealth,
            eProperty.Skill_Sword,
            eProperty.Skill_Hammer,
            eProperty.Skill_Axe,
            eProperty.Skill_Left_Axe,
            eProperty.Skill_Spear,
            eProperty.Skill_Mending,
            eProperty.Skill_Augmentation,
	        //Skill_Cave_Magic = 59,
	        eProperty.Skill_Darkness,
            eProperty.Skill_Suppression,
            eProperty.Skill_Runecarving,
            eProperty.Skill_Stormcalling,
	        //eProperty.Skill_BeastCraft, // bonus not used
			eProperty.Skill_Archery,
            eProperty.Skill_Battlesongs,
            eProperty.Skill_Subterranean,
            eProperty.Skill_BoneArmy,
            eProperty.Skill_Thrown_Weapons,
            eProperty.Skill_HandToHand,
    		//eProperty.Skill_Pacification,
	        //eProperty.Skill_Savagery,
	        eProperty.Skill_OdinsWill,
            eProperty.Skill_Cursing,
            eProperty.Skill_Hexing,
            eProperty.Skill_Witchcraft,
            eProperty.Skill_Summoning,
            eProperty.Skill_Aura_Manipulation, //Maulers
			eProperty.Skill_FistWraps, //Maulers
			eProperty.Skill_MaulerStaff, //Maulers
			eProperty.Skill_Magnetism, //Maulers
			eProperty.Skill_Power_Strikes, //Maulers
		};



        private static int[] ArmorSlots = new int[] { 21, 22, 23, 25, 27, 28, };
        private static int[] MagicalSlots = new int[] { 24, 26, 29, 32, 33, 34, 35, 36 };

        // the following are doubled up to work around an apparent mid-number bias to the random number generator

        // note that weapon array has been adjusted to add weight to more commonly used items
        private static eObjectType[] AlbionWeapons = new eObjectType[]
        {
            eObjectType.ThrustWeapon,
            eObjectType.CrushingWeapon,
            eObjectType.SlashingWeapon,
            eObjectType.Shield,
            eObjectType.Staff,
            eObjectType.TwoHandedWeapon,
            eObjectType.Longbow,
            eObjectType.Flexible,
            eObjectType.PolearmWeapon,
            eObjectType.FistWraps, //Maulers
			eObjectType.MaulerStaff,//Maulers
			eObjectType.Instrument,
            eObjectType.Crossbow,
            eObjectType.ThrustWeapon,
            eObjectType.CrushingWeapon,
            eObjectType.SlashingWeapon,
            eObjectType.Shield,
            eObjectType.Staff,
            eObjectType.TwoHandedWeapon,
            eObjectType.Longbow,
            eObjectType.Flexible,
            eObjectType.PolearmWeapon,
            eObjectType.FistWraps, //Maulers
			eObjectType.MaulerStaff,//Maulers
			eObjectType.Instrument,
            eObjectType.Crossbow,
        };

        private static eObjectType[] AlbionArmor = new eObjectType[]
        {
            eObjectType.Cloth,
            eObjectType.Leather,
            eObjectType.Studded,
            eObjectType.Chain,
            eObjectType.Plate,
            eObjectType.Cloth,
            eObjectType.Leather,
            eObjectType.Studded,
            eObjectType.Chain,
            eObjectType.Plate,
        };
        private static eObjectType[] MidgardWeapons = new eObjectType[]
        {
            eObjectType.Sword,
            eObjectType.Hammer,
            eObjectType.Axe,
            eObjectType.Shield,
            eObjectType.Staff,
            eObjectType.Spear,
            eObjectType.CompositeBow ,
            eObjectType.LeftAxe,
            eObjectType.HandToHand,
            eObjectType.FistWraps,//Maulers
			eObjectType.MaulerStaff,//Maulers
			eObjectType.Sword,
            eObjectType.Hammer,
            eObjectType.Axe,
            eObjectType.Shield,
            eObjectType.Staff,
            eObjectType.Spear,
            eObjectType.CompositeBow ,
            eObjectType.LeftAxe,
            eObjectType.HandToHand,
            eObjectType.FistWraps,//Maulers
			eObjectType.MaulerStaff,//Maulers
		};

        private static eObjectType[] MidgardArmor = new eObjectType[]
        {
            eObjectType.Cloth,
            eObjectType.Leather,
            eObjectType.Studded,
            eObjectType.Chain,
            eObjectType.Cloth,
            eObjectType.Leather,
            eObjectType.Studded,
            eObjectType.Chain,
        };

        private static eObjectType[] HiberniaWeapons = new eObjectType[]
        {
            eObjectType.Blades,
            eObjectType.Blunt,
            eObjectType.Piercing,
            eObjectType.Shield,
            eObjectType.Staff,
            eObjectType.LargeWeapons,
            eObjectType.CelticSpear,
            eObjectType.Scythe,
            eObjectType.RecurvedBow,
            eObjectType.Instrument,
            eObjectType.FistWraps,//Maulers
			eObjectType.MaulerStaff,//Maulers
			eObjectType.Blades,
            eObjectType.Blunt,
            eObjectType.Piercing,
            eObjectType.Shield,
            eObjectType.Staff,
            eObjectType.LargeWeapons,
            eObjectType.CelticSpear,
            eObjectType.Scythe,
            eObjectType.RecurvedBow,
            eObjectType.Instrument,
            eObjectType.FistWraps,//Maulers
			eObjectType.MaulerStaff,//Maulers
		};

        private static eObjectType[] HiberniaArmor = new eObjectType[]
        {
            eObjectType.Cloth,
            eObjectType.Leather,
            eObjectType.Reinforced,
            eObjectType.Scale,
            eObjectType.Cloth,
            eObjectType.Leather,
            eObjectType.Reinforced,
            eObjectType.Scale,
        };
       
        
    }
}

