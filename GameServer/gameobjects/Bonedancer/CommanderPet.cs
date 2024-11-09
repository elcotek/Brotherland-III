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
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.ServerProperties;
using DOL.Language;
using System;


namespace DOL.GS
{
    public class CommanderPet : BDPet
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Create a commander.
        /// </summary>
        /// <param name="npcTemplate"></param>
        /// <param name="owner"></param>
        public CommanderPet(INpcTemplate npcTemplate)
            : base(npcTemplate)
        {
            if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.ReturnedCommander"))
            {
                InitControlledBrainArray(0);
            }

            if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DecayedCommander") ||
                Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.SkeletalCommander"))
            {
                InitControlledBrainArray(1);
            }

            if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.BoneCommander"))
            {
                InitControlledBrainArray(2);
            }

            if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadCommander") ||
                Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadGuardian") ||
                Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadLich") ||
                Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadArcher"))
            {
                InitControlledBrainArray(3);
            }
        }

        /// <summary>
        /// Called when owner sends a whisper to the pet
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        public override bool WhisperReceive(GameLiving source, string str)
        {
            GamePlayer player = source as GamePlayer;
            if (player == null || player != (Brain as IControlledBrain).Owner)
                return false;

            string[] strargs = str.ToLower().Split(' ');

            for (int i = 0; i < strargs.Length; i++)
            {
                String curStr = strargs[i];

                if (curStr == "commander")
                {
                    if (Name == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.DreadGuardian"))
                    {
                        player.Out.SendMessage(LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.WR.DreadGuardian", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }

                    if (Name == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.DreadLich"))
                    {
                        player.Out.SendMessage(LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.WR.DreadLich", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }

                    if (Name == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.DreadArcher"))
                    {
                        player.Out.SendMessage(LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.WR.DreadArcher", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }

                    if (Name.ToLower() == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.DreadCommander") ||
                        Name.ToLower() == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.DecayedCommander") ||
                        Name.ToLower() == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.ReturnedCommander") ||
                        Name.ToLower() == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.SkeletalCommander") ||
                        Name.ToLower() == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.BoneCommander"))
                    {
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.XCommander", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }

                }

                if (curStr == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.WR.Const.Combat"))
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.WR.Combat", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }

                if (curStr == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.WR.Const.Assist"))
                {
                    string name = "";
                    if (player.TargetObject != null)
                        name = player.TargetObject.Name;
                    //assist on/off
                    if (EnableAssist == false)
                    {
                        EnableAssist = true;
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.Assist.Text", name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        break;
                    }
                    else
                    {
                        EnableAssist = false;
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.AssistOff.Text", name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        break;
                    }
                }
                if (curStr == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.WR.Const.Taunt"))
                {
                    bool found = false;
                    foreach (Spell spell in Spells)
                    {
                        //If the taunt spell's ID is changed - this needs to be changed
                        if (spell.ID == 60127)
                        {
                            Spells.Remove(spell);
                            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.CommNoTaunt"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                            found = true;
                            break;
                        }
                    }
                    if (found) break;
                    //TODO: change this so it isn't hardcoded
                    Spell tauntspell = SkillBase.GetSpellByID(60127);
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.CommStartTaunt"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                    if (tauntspell != null)

                        Spells.Add(tauntspell);


                    else
                        Console.WriteLine("Couldn't find BD pet's taunt spell");
                    break;
                }

                if (curStr == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.WR.Const.Weapons"))
                {
                    if (Name.ToLower() == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.DreadCommander") &&
                        Name.ToLower() == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.DecayedCommander") &&
                        Name.ToLower() == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.ReturnedCommander") &&
                        Name.ToLower() == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.SkeletalCommander") &&
                        Name.ToLower() == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.BoneCommander"))
                    {
                        break;
                    }
                    player.Out.SendMessage(LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.WR.DiffCommander", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }

                if (curStr == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.WR.Const.Spells"))
                {
                    if (Name.ToLower() != LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.DreadLich"))
                    {
                        return false;
                    }
                    player.Out.SendMessage(LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.WR.DreadLich2", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }

                if (curStr == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.WR.Const.Empower"))
                {
                    if (Name.ToLower() == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.DreadGuardian") ||
                        Name.ToLower() == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.DreadLich") ||
                        Name.ToLower() == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.DreadArcher"))
                    {

                        //Empower
                        bool found = false;
                        foreach (Spell spell in Spells)
                        {
                            //If the taunt spell's ID is changed - this needs to be changed
                            if (spell.ID == 60129)
                            {

                                Spells.Remove(spell);
                                player.Out.SendMessage(LanguageMgr.GetTranslation("EN", "Your commander will no long empower its enemies!"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                                found = true;
                                break;
                            }
                        }
                        if (found) break;
                        //TODO: change this so it isn't hardcoded
                        Spell empowerspell = SkillBase.GetSpellByID(60129);
                        player.Out.SendMessage(LanguageMgr.GetTranslation("EN", "Your commander will start to taunt its enemies!"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                        if (empowerspell != null)

                            Spells.Add(empowerspell);


                        else
                            Console.WriteLine("Couldn't find BD pet's Empower spell");
                        break;
                    }
                }

                if (curStr == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.WR.Const.Snares"))
                {
                }

                if (curStr == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.WR.Const.Debilitating"))
                {
                }

                if (curStr == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.WR.Const.Damage"))
                {
                }

                if (curStr == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.WR.Const.One"))
                {
                    i++;
                    if (i + 1 >= strargs.Length)
                        return false;
                    CommanderSwitchWeapon(eInventorySlot.RightHandWeapon, eActiveWeaponSlot.Standard, strargs[++i]);
                }

                if (curStr == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.WR.Const.Two"))
                {
                    i++;
                    if (i + 1 >= strargs.Length)
                        return false;
                    CommanderSwitchWeapon(eInventorySlot.TwoHandWeapon, eActiveWeaponSlot.TwoHanded, strargs[++i]);
                }

                if (curStr == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.WR.Const.Harm"))
                {
                    if (Name.ToLower() != LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.DreadGuardian"))
                    {
                        return false;
                    }
                    player.Out.SendMessage(LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.WR.DreadGuardian2", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }

                if (curStr == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.WR.Const.Drain"))
                {
                }

                if (curStr == LanguageMgr.GetTranslation("EN", "GameObjects.CommanderPet.WR.Const.Suppress"))
                {
                }
            }
            return base.WhisperReceive(source, str);
        }

        /// <summary>
        /// Changes the commander's weapon to the specified type
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="aSlot"></param>
        /// <param name="weaponType"></param>
        protected void CommanderSwitchWeapon(eInventorySlot slot, eActiveWeaponSlot aSlot, string weaponType)
        {
            if (Inventory == null)
                return;

            string itemId = string.Format("BD_Commander_{0}_{1}", slot.ToString(), weaponType);
            //all weapons removed before
            InventoryItem item = Inventory.GetItem(eInventorySlot.RightHandWeapon);
            if (item != null) Inventory.RemoveItem(item);
            item = Inventory.GetItem(eInventorySlot.TwoHandWeapon);
            if (item != null) Inventory.RemoveItem(item);

            ItemTemplate temp = GameServer.Database.FindObjectByKey<ItemTemplate>(itemId);

            if (temp == null)
            {
                if (log.IsErrorEnabled)
                    log.Error(string.Format("Unable to find Bonedancer item: {0}", itemId));
                return;
            }

            Inventory.AddItem(slot, GameInventoryItem.Create<ItemTemplate>(temp));
            SwitchWeapon(aSlot);
            AddStatsToWeapon();
            UpdateNPCEquipmentAppearance();
        }

        protected short m_bd_petDex;

        /// <summary>
        /// Base dexterity. Make greater necroservant slightly more dextrous than
        /// all the other pets.
        /// </summary>
        public override short Dexterity
        {
            get
            {
                return m_bd_petDex += (short)((Level - 1) / 2);

            }
            set
            {
                value = m_bd_petDex;
            }
        }
        #region Stats
        /// <summary>
        /// Set stats according to PET_AUTOSET values, then scale them according to the values in the DB
        /// </summary>
        public override void AutoSetStats()
        {
            // Assign values from Autoset
            Strength = (short)Math.Max(1, Properties.PET_AUTOSET_STR_BASE);
            Constitution = (short)Math.Max(1, Properties.PET_AUTOSET_CON_BASE);
            Quickness = (short)Math.Max(1, Properties.PET_AUTOSET_QUI_BASE);
            m_bd_petDex = (short)Math.Max(1, Properties.PET_AUTOSET_DEX_BASE);
            Intelligence = (short)Math.Max(1, Properties.PET_AUTOSET_INT_BASE);
            Empathy = (short)30;
            Piety = (short)30;
            Charisma = (short)30;

            // Now add stats for levelling
            Strength += (short)Math.Round(10.0 * (Level - 1) * Properties.PET_AUTOSET_STR_MULTIPLIER);
            Constitution += (short)Math.Round((Level - 1) * Properties.PET_AUTOSET_CON_MULTIPLIER);
            Quickness += (short)Math.Round((Level - 1) * Properties.PET_AUTOSET_QUI_MULTIPLIER);
            m_bd_petDex += (short)Math.Round((Level - 1) * Properties.PET_AUTOSET_DEX_MULTIPLIER);
            Intelligence += (short)Math.Round((Level - 1) * Properties.PET_AUTOSET_INT_MULTIPLIER);
            Empathy += (short)(Level - 1);
            Piety += (short)(Level - 1);
            Charisma += (short)(Level - 1);

            // Now scale them according to NPCTemplate values
            if (NPCTemplate != null)
            {
                if (NPCTemplate.Strength > 0)
                    Strength = (short)Math.Round(Strength * (NPCTemplate.Strength / 100.0));
                if (NPCTemplate.Constitution > 0)
                    Constitution = (short)Math.Round(Constitution * (NPCTemplate.Constitution / 100.0));
                if (NPCTemplate.Quickness > 0)
                    Quickness = (short)Math.Round(Quickness * (NPCTemplate.Quickness / 100.0));
                if (NPCTemplate.Dexterity > 0)
                    Dexterity = (short)Math.Round(Dexterity * (NPCTemplate.Dexterity / 100.0));
                if (NPCTemplate.Intelligence > 0)
                    Intelligence = (short)Math.Round(Intelligence * (NPCTemplate.Intelligence / 100.0));
                // Except for CHA, EMP, AND PIE as those don't have autoset values.
                if (NPCTemplate.Empathy > 0)
                    Empathy = (short)NPCTemplate.Empathy;
                if (NPCTemplate.Piety > 0)
                    Piety = (short)NPCTemplate.Piety;
                if (NPCTemplate.Charisma > 0)
                    Charisma = (short)NPCTemplate.Charisma;
            }
        }
        #endregion

        #region Melee

        /// <summary>
        /// The type of damage the currently active weapon does.
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public override eDamageType AttackDamageType(InventoryItem weapon)
        {
            if (weapon != null)
            {
                switch ((eWeaponDamageType)weapon.Type_Damage)
                {
                    case eWeaponDamageType.Crush: return eDamageType.Crush;
                    case eWeaponDamageType.Slash: return eDamageType.Slash;
                }
            }

            return eDamageType.Crush;
        }

        /// <summary>
        /// Get melee speed in milliseconds.
        /// </summary>
        /// <param name="weapons"></param>
        /// <returns></returns>
        public override int AttackSpeed(params InventoryItem[] weapons)
        {
            double weaponSpeed = 0.0;
            int speedCap;
            if (weapons != null)
            {
                foreach (InventoryItem item in weapons)
                    if (item != null)
                        weaponSpeed += item.SPD_ABS;
                    else
                    {
                        weaponSpeed += 34;
                    }
                weaponSpeed = (weapons.Length > 0) ? weaponSpeed / weapons.Length : 34.0;
            }
            else
            {
                weaponSpeed = 34.0;
            }

            double speed = 100 * weaponSpeed * (1.0 - (GetModified(eProperty.Quickness) - 60) / 500.0);

            speedCap = (int)(speed * GetModified(eProperty.MeleeSpeed) * 0.01);

            if (speedCap < 2500)
            {
                speedCap = 1900;
            }

            return speedCap;
        }

        /// <summary>
        /// Calculate how fast this pet can cast a given spell
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        public override int CalculateCastingTime(SpellLine line, Spell spell)
        {
            int ticks = spell.CastTime;

            double percent = DexterityCastTimeReduction;
            percent -= GetModified(eProperty.CastingSpeed) * .01;

            ticks = (int)(ticks * Math.Max(CastingSpeedReductionCap, percent));
            if (ticks < MinimumCastingSpeed)
                ticks = MinimumCastingSpeed;

            return ticks;
        }

        /// <summary>
        /// Whether or not pet can use left hand weapon.
        /// </summary>
        public override bool CanUseLefthandedWeapon
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Calculates how many times left hand can swing.
        /// </summary>
        /// <returns></returns>
        public override int CalculateLeftHandSwingCount()
        {
            return 0;
        }




        #endregion
        /// <summary>
        /// Adds a pet to the current array of pets
        /// </summary>
        /// <param name="controlledNpc">The brain to add to the list</param>
        /// <returns>Whether the pet was added or not</returns>
        public override bool AddControlledNpc(IControlledBrain controlledNpc)
        {
            IControlledBrain[] brainlist = ControlledNpcList;
            if (brainlist == null) return false;
            foreach (IControlledBrain icb in brainlist)
            {
                if (icb == controlledNpc)
                    return false;
            }

            if (controlledNpc.Owner != this)
                throw new ArgumentException("ControlledNpc with wrong owner is set (player=" + Name + ", owner=" + controlledNpc.Owner.Name + ")", "controlledNpc");

            //Find the next spot for this new pet
            int i = 0;
            for (; i < brainlist.Length; i++)
            {
                if (brainlist[i] == null)
                    break;
            }


            //If we didn't find a spot return false
            if (i >= m_controlledBrain.Length)
                return false;
            m_controlledBrain[i] = controlledNpc;

            ///Calculate Subpet Level form skilllevel
            if (Owner != null && Owner is GamePlayer && m_controlledBrain[i] != null)
            {
                SetSupPetLevelFromSkillLevel(m_controlledBrain[i].Body, Owner as GamePlayer);
            }

            PetCount++;
            return base.AddControlledNpc(controlledNpc);
        }


        /// <summary>
        /// Bondancer Subpet Level from skillLevel
        /// </summary>
        /// <param name="SubPet"></param>
        /// <param name="Owner"></param>
        public virtual void SetSupPetLevelFromSkillLevel(GameLiving SubPet, GamePlayer Owner)
        {
            int DarknessSkillLevel = 0;
            int SuppressionSkillLevel = 0;
            int BoneArmySkillLevel = 0;
            byte SubPetLevel = 0;
            byte maxlevel = 0;
            if (Owner is GamePlayer)
            {

                SuppressionSkillLevel = Owner.GetBaseSpecLevel(Specs.Suppression);

                switch (SubPet.Name)
                {

                    //Suppression pets
                    case "bone patroller":
                        {
                            maxlevel = 19;
                            break;
                        }
                    case "bonepatcher":
                        {
                            maxlevel = 21;
                            break;
                        }
                    case "bone sentry":
                        {
                            maxlevel = 26;
                            break;
                        }
                    case "bonefixer":
                        {
                            maxlevel = 26;
                            break;
                        }
                    case "bone bouncer":
                        {
                            maxlevel = 28;
                            break;
                        }
                    case "bonebuilder":
                        {
                            maxlevel = 30;
                            break;
                        }
                    case "bone defender":
                        {
                            maxlevel = 32;
                            break;
                        }
                    case "bonehealer":
                        {
                            maxlevel = 35;
                            break;
                        }
                    case "bone watchman":
                        {
                            maxlevel = 44;
                            break;
                        }
                    case "boneknitter":
                        {
                            maxlevel = 44;
                            break;
                        }
                    case "bone guard":
                        {
                            maxlevel = 45;
                            break;
                        }
                    case "bonemender":
                        {
                            maxlevel = 45;
                            break;
                        }
                }
                if (SubPet.Level < SuppressionSkillLevel && SubPet.Level < (byte)(Owner.Level * 0.76))
                {
                    //log.ErrorFormat("Subpet level vorher {0}", SubPet.Level);
                    //75% of Player Level is Max
                    SubPetLevel = (byte)(Owner.Level * 0.76);
                    if (SuppressionSkillLevel > SubPetLevel && maxlevel == 45)
                    {
                        SubPet.Level = (byte)(Owner.Level * 0.76);
                        //log.ErrorFormat("new Subpet level nun {0}", SubPet.Level);
                    }
                    else

                    if (maxlevel >= SuppressionSkillLevel)
                    {
                        SubPet.Level = (byte)SuppressionSkillLevel;
                    }
                    else
                        SubPet.Level = maxlevel;
                    //log.ErrorFormat("Subpet level nun {0}", SubPet.Level);
                }
            }


            if (Owner is GamePlayer)
            {
                DarknessSkillLevel = Owner.GetBaseSpecLevel(Specs.Darkness);


                switch (SubPet.Name)
                {

                    //Darkness pets
                    case "bonemage":
                        {
                            maxlevel = 19;
                            break;
                        }
                    case "bonebreaker":
                        {
                            maxlevel = 21;
                            break;
                        }
                    case "bonecaster":
                        {
                            maxlevel = 26;
                            break;
                        }
                    case "bonejinxer":
                        {
                            maxlevel = 26;
                            break;
                        }
                    case "bone shaman":
                        {
                            maxlevel = 28;
                            break;
                        }
                    case "bonecurser":
                        {
                            maxlevel = 30;
                            break;
                        }
                    case "bone warlock":
                        {
                            maxlevel = 32;
                            break;
                        }
                    case "bonebeguilder":
                        {
                            maxlevel = 35;
                            break;
                        }
                    case "bone diviner":
                        {
                            maxlevel = 44;
                            break;
                        }
                    case "bonebewitcher":
                        {
                            maxlevel = 44;
                            break;
                        }
                    case "bone warmage":
                        {
                            maxlevel = 45;
                            break;
                        }
                    case "bonehexer":
                        {
                            maxlevel = 45;
                            break;
                        }
                }



                if (SubPet.Level < DarknessSkillLevel && SubPet.Level < (byte)(Owner.Level * 0.76))
                {
                    //log.ErrorFormat("Subpet level vorher {0}", SubPet.Level);
                    //75% of Player Level is Max
                    SubPetLevel = (byte)(Owner.Level * 0.76);
                    if (DarknessSkillLevel > SubPetLevel && maxlevel == 45)
                    {
                        SubPet.Level = (byte)(Owner.Level * 0.76);
                        //log.ErrorFormat("new Subpet level nun {0}", SubPet.Level);
                    }
                    else
                    
                    if (maxlevel >= DarknessSkillLevel)
                    {
                        SubPet.Level = (byte)DarknessSkillLevel;
                    }
                    else
                        SubPet.Level = maxlevel;

                    //log.ErrorFormat("Subpet level nun {0}", SubPet.Level);
                }
            }

            if (Owner is GamePlayer)
            {


                BoneArmySkillLevel = Owner.GetBaseSpecLevel(Specs.BoneArmy);


                switch (SubPet.Name)
                {



                    //BoneArmy pets
                    case "bone dart":
                        {
                            maxlevel = 19;
                            break;
                        }
                    case "bonesmasher":
                        {
                            maxlevel = 21;
                            break;
                        }
                    case "bone arrow":
                        {
                            maxlevel = 26;
                            break;
                        }
                    case "bonecrusher":
                        {
                            maxlevel = 26;
                            break;
                        }
                    case "bone archer":
                        {
                            maxlevel = 28;
                            break;
                        }
                    case "bonecracker":
                        {
                            maxlevel = 30;
                            break;
                        }
                    case "bone bolt":
                        {
                            maxlevel = 32;
                            break;
                        }
                    case "boneslammer":
                        {
                            maxlevel = 35;
                            break;
                        }
                    case "bone hunter":
                        {
                            maxlevel = 44;
                            break;
                        }
                    case "boneburster":
                        {
                            maxlevel = 44;
                            break;
                        }
                    case "bone deadeye":
                        {
                            maxlevel = 45;
                            break;
                        }
                    case "bonerazer":
                        {
                            maxlevel = 45;
                            break;
                        }
                }



                if (SubPet.Level < BoneArmySkillLevel && SubPet.Level < (byte)(Owner.Level * 0.76))
                {
                    //log.ErrorFormat("Subpet level vorher {0}", SubPet.Level);
                    //75% of Player Level is Max
                    SubPetLevel = (byte)(Owner.Level * 0.76);
                    if (BoneArmySkillLevel > SubPetLevel && maxlevel == 45)
                    {
                        SubPet.Level = (byte)(Owner.Level * 0.76);
                        //log.ErrorFormat("new Subpet level nun {0}", SubPet.Level);
                    }
                    else 
                    
                    if (maxlevel >= BoneArmySkillLevel)
                    {
                        SubPet.Level = (byte)BoneArmySkillLevel;
                    }
                    else
                        SubPet.Level = maxlevel;
                    //log.ErrorFormat("Subpet level nun {0}", SubPet.Level);
                }
            }
        }
                   /*
                    //Darkness pets
                    case "bonemage":
                    case "bonebreaker":
                    case "bonecaster":
                    case "bonejinxer":
                    case "bone shaman":
                    case "bonecurser":
                    case "bone warlock":
                    case "bonebeguilder":
                    case "bone diviner":
                    case "bonebewitcher":
                    case "bone warmage":
                    case "bonehexer":
                        {
                            if (SubPet.Level < DarknessSkillLevel && SubPet.Level < (byte)(Owner.Level * 0.76))
                            {
                                //log.ErrorFormat("darkness Subpet level vorher {0}", SubPet.Level);
                                //75% of Player Level is Max
                                SubPetLevel = (byte)(Owner.Level * 0.76);
                                if (DarknessSkillLevel > SubPetLevel)
                                {
                                    SubPet.Level = (byte)(Owner.Level * 0.76);
                                    //log.ErrorFormat("new darkness Subpet level nun {0}", SubPet.Level);
                                }
                                else
                                    SubPet.Level = (byte)DarknessSkillLevel;
                                //log.ErrorFormat("darkness Subpet level nun {0}", SubPet.Level);
                            }
                        }
                        break;

                    //BoneArmy pets
                    case "bone dart":
                    case "bonesmasher":
                    case "bone arrow":
                    case "bonecrusher":
                    case "bone archer":
                    case "bonecracker":
                    case "bone bolt":
                    case "boneslammer":
                    case "bone hunter":
                    case "boneburster":
                    case "bone deadeye":
                    case "bonerazer":
                    {
                            if (SubPet.Level < BoneArmySkillLevel && SubPet.Level < (byte)(Owner.Level * 0.76))
                            {
                                //log.ErrorFormat("BoneArmy Subpet level vorher {0}", SubPet.Level);
                                //75% of Player Level is Max
                                SubPetLevel = (byte)(Owner.Level * 0.76);
                                if (BoneArmySkillLevel > SubPetLevel)
                                {
                                    SubPet.Level = (byte)(Owner.Level * 0.76);
                                    //log.ErrorFormat("new BoneArmy Subpet level nun {0}", SubPet.Level);
                                }
                                else
                                    SubPet.Level = (byte)BoneArmySkillLevel;
                               // log.ErrorFormat("BoneArmy Subpet level nun {0}", SubPet.Level);
                            }
                        }
                        break;

                    default: break;
                }
                
            }

          

        }
                   */
        /// <summary>
        /// Removes the brain from
        /// </summary>
        /// <param name="controlledNpc">The brain to find and remove</param>
        /// <returns>Whether the pet was removed</returns>
        public override bool RemoveControlledNpc(IControlledBrain controlledNpc)
        {
            bool found = false;
            lock (ControlledNpcList)
            {
                if (controlledNpc == null) return false;
                IControlledBrain[] brainlist = ControlledNpcList;
                int i = 0;
                //Try to find the minion in the list
                for (; i < brainlist.Length; i++)
                {
                    //Found it
                    if (brainlist[i] == controlledNpc)
                    {
                        found = true;
                        break;
                    }
                }

                //Found it, lets remove it
                if (found)
                {
                    if (controlledNpc.Body is GamePet minion)
                        minion.StripOwnerBuffs(Owner);

                    //First lets store the brain to kill it
                    IControlledBrain tempBrain = m_controlledBrain[i];
                    //Lets get rid of the brain asap
                    m_controlledBrain[i] = null;

                    //Only decrement, we just lost one pet
                    PetCount--;

                    return base.RemoveControlledNpc(controlledNpc);
                }
            }

            return found;
        }
    }
}