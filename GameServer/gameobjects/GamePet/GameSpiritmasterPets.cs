/*
*
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
    public class GameSpiritmasterPets : GamePet
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Checks if this NPC has equiped a spear weapon by command
        /// </summary>
        protected bool m_hasSpear = false;

        /// <summary>
        /// Checks if this NPC has equiped a spear weapon by command
        /// </summary>
        public bool HasSpear
        {
            get { return m_hasSpear; }
            set { m_hasSpear = value; }
        }


        /// <summary>
        /// Create a Underhill Pet
        /// </summary>
        /// <param name="npcTemplate"></param>
        /// <param name="owner"></param>
        public GameSpiritmasterPets(INpcTemplate npcTemplate)
            : base(npcTemplate)
        {
            switch (Name.ToLower())
            {
                case "Spirit Fighter":
                case "Spirit Soldier":
                case "Spirit Swordsman":
                case "Spirit Warrior":
                case "Spirit Champion":
                    {
                        SpiritPetType = 1;
                        break;
                    }
                default: break;
            }
            UpdateSpiritPetModelStats();
        }

        // 1 = basic, 2 = caster, 3 = tanker
        public int SpiritPetType = 1;
        public eRace SpiritmasterPetRace = eRace.Unknown;
        private readonly string templatedagger = "Spiritmaster_Pet_Dagger";
        private readonly string templatespeer = "Spiritmaster_Pet_Speer";
        private readonly string templateshield = "Spiritmaster_Pet_Shield";
        private readonly string templateaxe = "Spiritmaster_Pet_Axe";
        //private string templatetwohandsword = "Spiritmaster_Pet_TwoHandSword";

        public virtual ushort AviableModels(ushort PetModel)
        {
            switch (PetModel)
            {

                case 137:
                case 138:
                case 139:
                case 140:
                case 141:
                case 142:
                case 143:
                case 144:

                case 145:
                case 146:
                case 147:
                case 148:
                case 149:
                case 150:
                case 151:
                case 152:

                case 153:
                case 154:
                case 155:
                case 156:
                case 157:
                case 158:
                case 159:
                case 160:

                case 161:
                case 162:
                case 163:
                case 164:
                case 165:
                case 166:
                case 167:
                case 168:
                    /*
                    case 169:
                    case 170:
                    case 171:
                    case 172:
                    case 173:
                    case 174:
                    case 175:
                    case 176:
                    case 177:
                    case 178:
                    case 179:
                    case 180:
                    case 181:
                    case 182:
                    case 183:
                    case 184:
                    */
                    {
                        if (SpiritmasterPetRace == eRace.Norseman && Gender == eGender.Female)
                        {
                            return PetModel;
                        }
                        break;
                    }

                default: break;
            }
            return 168;
        }

        public bool UpdateSpiritPetModelStats()
        {
            switch (Model)
            {
                case 137:
                case 138:
                case 139:
                case 140:
                case 141:
                case 142:
                case 143:
                case 144:
                    {
                        SpiritmasterPetRace = eRace.Frostalf;
                        Gender = eGender.Male;
                        break;
                    }
                case 145:
                case 146:
                case 147:
                case 148:
                case 149:
                case 150:
                case 151:
                case 152:
                    {
                        SpiritmasterPetRace = eRace.Frostalf;
                        Gender = eGender.Female;
                        break;
                    }
                case 153:
                case 154:
                case 155:
                case 156:
                case 157:
                case 158:
                case 159:
                case 160:
                    {
                        SpiritmasterPetRace = eRace.Norseman;
                        Gender = eGender.Male;
                        break;
                    }
                case 161:
                case 162:
                case 163:
                case 164:
                case 165:
                case 166:
                case 167:
                case 168:


                    /*
                    case 169:
                    case 170:
                    case 171:
                    case 172:
                    case 173:
                    case 174:
                    case 175:
                    case 176:
                    case 177:
                    case 178:
                    case 179:
                    case 180:
                    case 181:
                    case 182:
                    case 183:
                    case 184:
                    */
                    {
                        SpiritmasterPetRace = eRace.Norseman;
                        Gender = eGender.Female;
                        break;
                    }

                default: break;
            }
            return true;

        }
        /// <summary>
        /// Called when owner sends a whisper to the pet
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        public override bool WhisperReceive(GameLiving source, string str)
        {
            string gendermessage = string.Empty;
            string racemessage = string.Empty;
            ushort newModel = 0;
            ushort newModel2 = 0;
            ushort newModel3 = 0;
            ushort newModel4 = 0;





            GamePlayer player = source as GamePlayer;
            if (this.InCombat || player == null || player != (Brain as IControlledBrain).Owner)
                return false;


            if (this.Name == "Spirit Fighter" == false &&
                      this.Name == "Spirit Soldier" == false &&
                       this.Name == "Spirit Swordsman" == false &&
                       this.Name == "Spirit Warrior" == false &&
                       this.Name == "Spirit Champion" == false)
                return false;





            /*
             this.Name == "Underhill Companion" ||
                       this.Name == "Underhill Zealot" ||
                        this.Name == "Underhill Friend" ||
                        this.Name == "Underhill Compatriot" ||
                        this.Name == "Underhill Ally"
                      && 
             */
            string[] strargs = str.ToLower().Split(' ');

            for (int i = 0; i < strargs.Length; i++)
            {
                String curStr = strargs[i];



                if (curStr == "niflheim")
                {

                    if (Gender == eGender.Male)
                    {
                        gendermessage = "[female]";
                    }


                    else if (Gender == eGender.Female)
                    {
                        gendermessage = "[male]";
                    }


                    else if (Gender == eGender.Neutral)
                    {
                        gendermessage = "[female] or [male]";
                    }


                    if (SpiritmasterPetRace == eRace.Norseman)
                    {
                        racemessage = "a [frostalf]";
                    }
                    else if (SpiritmasterPetRace == eRace.Frostalf)
                    {
                        racemessage = "a [norseman]";
                    }
                    if (SpiritmasterPetRace == eRace.Unknown)
                    {

                        racemessage = "a [frostalf] or [norseman]";
                    }

                    string playermessage = "Greetings, friend. As one of the Spiritmasters, I also have the ability to take the appearance of " + racemessage +
                       ". If you wish I can also can become a " + gendermessage + ". " +

                       ". I also have several [combat] tactics at your disposal.";

                    player.Out.SendMessage(playermessage, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    break;

                }

                if (curStr == "male")
                {

                    //  if (Gender == eGender.Neutral)
                    //    break;
                    if (Gender == eGender.Male)
                    {
                        player.Out.SendMessage("Your pet is already Male.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                        break;
                    }
                    if (Model >= 145 && Model <= 152)

                        newModel = (ushort)(Model - (Model + 8));

                    if (Model >= 153 && Model <= 160)

                        newModel = (ushort)(Model - (Model + 8));

                    Model = AviableModels(newModel);

                    UpdateSpiritPetModelStats();
                    player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                    break;
                }
                if (curStr == "female")
                {
                    // if (Gender == eGender.Neutral)
                    //   break;
                    if (Gender == eGender.Female)
                    {
                        player.Out.SendMessage("Your pet is already Female.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                        break;
                    }


                    if (Model >= 137 && Model <= 144)

                        newModel2 = (ushort)(Model + 8);

                    if (Model >= 153 && Model <= 160)

                        newModel2 = (ushort)(Model + 8);


                    Model = AviableModels(newModel2);

                    UpdateSpiritPetModelStats();
                    player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                    break;
                }

                if (curStr == "frostalf")
                {

                    // if (SpiritmasterPetRace == eRace.Unknown)
                    //    break;
                    if (Race == (short)eRace.Frostalf)
                    {
                        player.Out.SendMessage("Your pet is already Frostalf.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                        break;
                    }

                    if (Model >= 153 && Model <= 160)

                        newModel3 = (ushort)(Model - 8);

                    if (Model >= 161 && Model <= 168)

                        newModel3 = (ushort)(Model - 16);



                    Model = AviableModels(newModel3);

                    UpdateSpiritPetModelStats();
                    player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                    break;
                }

                if (curStr == "norseman")
                {
                    // if (SpiritmasterPetRace == eRace.Unknown)
                    //    break;
                    if (Race == (short)eRace.Norseman)
                    {
                        player.Out.SendMessage("Your pet is already Norseman.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                        break;
                    }


                    if (Model >= 138 && Model <= 144)

                        newModel4 = (ushort)(Model + 16);

                    if (Model >= 145 && Model <= 152)

                        newModel4 = (ushort)(Model + 8);


                    Model = AviableModels(newModel4);

                    UpdateSpiritPetModelStats();
                    player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                    break;
                }

                if (curStr == "combat" && SpiritPetType == 1)
                {
                    player.Out.SendMessage("You may bid me to [parry] with my weapon(s) or if you wish you may command me to [cease] parrying at any time." +
                        "I can also equip a [shield], [dagger], [speer], [axe] or [unequip] it just as easy. I am able to [assist] all your attacks or [taunt] your enemies so that they will focus me instead of yourself.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                }
                if (curStr == "parry")
                {
                    if (SpiritPetType == 1)
                    {
                        ParryChance = 10;
                        player.Out.SendMessage("As you wished. I am now parrying", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
                if (curStr == "assist")
                {
                    if (SpiritPetType == 1)
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
                    else
                    {
                        break;
                    }
                }
                if (curStr == "cease")
                {
                    if (SpiritPetType == 1)
                    {
                        ParryChance = 0;
                        player.Out.SendMessage("As you wished. I can nolonger parry", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
                if (curStr == "shield")
                {
                    if (SpiritPetType == 1)
                    {
                        if (ActiveWeaponSlot == eActiveWeaponSlot.TwoHanded)
                        {
                            InventoryItem item = Inventory.GetItem(eInventorySlot.TwoHandWeapon);
                            if (item != null)
                            {
                                Inventory.RemoveItem(item);
                                BroadcastLivingEquipmentUpdate();
                                SpiritmasterPetSwitchWeapon(eInventorySlot.RightHandWeapon, eActiveWeaponSlot.Standard, templatedagger);
                            }
                            SpiritmasterPetSwitchWeapon(eInventorySlot.LeftHandWeapon, eActiveWeaponSlot.Standard, templateshield);
                            // SpiritmasterPetSwitchWeapon(eInventorySlot.RightHandWeapon, eActiveWeaponSlot.Standard, templatedagger);
                            BlockChance = 10;
                            player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                            HasSpear = false;
                        }
                        else //if (ActiveWeaponSlot != eActiveWeaponSlot.TwoHanded)
                        {
                            SpiritmasterPetSwitchWeapon(eInventorySlot.LeftHandWeapon, eActiveWeaponSlot.Standard, templateshield);
                            BlockChance = 10;
                            player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                            HasSpear = false;
                        }

                        break;
                    }
                    break;
                }
                if (curStr == "unequip")
                {
                    if (SpiritPetType == 1)
                    {
                        InventoryItem item = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
                        if (item != null)
                        {
                            Inventory.RemoveItem(item);
                            BroadcastLivingEquipmentUpdate();
                            BlockChance = 0;
                            player.Out.SendMessage("As you wished. I will no longer block.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                            HasSpear = false;
                        }
                    }
                    break;
                }
                if (curStr == "speer")
                {
                    if (SpiritPetType == 1)
                    {
                        if (ActiveWeaponSlot == eActiveWeaponSlot.TwoHanded)
                        {
                            SpiritmasterPetSwitchWeapon(eInventorySlot.TwoHandWeapon, eActiveWeaponSlot.TwoHanded, templatespeer);
                            player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                            HasSpear = true;
                        }
                        else
                        {
                            SpiritmasterPetSwitchWeapon(eInventorySlot.TwoHandWeapon, eActiveWeaponSlot.TwoHanded, templatespeer);
                            player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                            HasSpear = true;
                        }
                        break;
                    }
                    break;
                }
                if (curStr == "dagger")
                {
                    if (SpiritPetType == 1)
                    {
                        if (ActiveWeaponSlot == eActiveWeaponSlot.TwoHanded)
                        {
                            SpiritmasterPetSwitchWeapon(eInventorySlot.RightHandWeapon, eActiveWeaponSlot.Standard, templatedagger);
                            player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                            HasSpear = false;
                        }
                        else
                        {
                            SpiritmasterPetSwitchWeapon(eInventorySlot.RightHandWeapon, eActiveWeaponSlot.Standard, templatedagger);
                            player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                            HasSpear = false;
                        }
                        break;
                    }
                }
                if (curStr == "axe")
                {
                    if (SpiritPetType == 1)
                    {
                        if (ActiveWeaponSlot == eActiveWeaponSlot.TwoHanded)
                        {
                            SpiritmasterPetSwitchWeapon(eInventorySlot.RightHandWeapon, eActiveWeaponSlot.Standard, templateaxe);
                            player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                            HasSpear = false;
                        }
                        else
                        {
                            SpiritmasterPetSwitchWeapon(eInventorySlot.RightHandWeapon, eActiveWeaponSlot.Standard, templateaxe);
                            player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                            HasSpear = false;
                        }
                        break;
                    }
                }
                if (curStr == "taunt")
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
                        Console.WriteLine("Couldn't find pet's taunt spell");
                    break;
                }

            }
            return base.WhisperReceive(source, str);
        }

        protected short m_spirit_petDex;

        /// <summary>
        /// Base dexterity. Make greater necroservant slightly more dextrous than
        /// all the other pets.
        /// </summary>
        public override short Dexterity
        {
            get
            {
                return m_spirit_petDex += (short)((Level - 1) / 2);

            }
            set
            {
                value = m_spirit_petDex;
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
            m_spirit_petDex = (short)Math.Max(1, Properties.PET_AUTOSET_DEX_BASE);
            Intelligence = (short)Math.Max(1, Properties.PET_AUTOSET_INT_BASE);
            Empathy = (short)30;
            Piety = (short)30;
            Charisma = (short)30;

            // Now add stats for levelling
            Strength += (short)Math.Round(10.0 * (Level - 1) * Properties.PET_AUTOSET_STR_MULTIPLIER);
            Constitution += (short)Math.Round((Level - 1) * Properties.PET_AUTOSET_CON_MULTIPLIER);
            Quickness += (short)Math.Round((Level - 1) * Properties.PET_AUTOSET_QUI_MULTIPLIER);
            m_spirit_petDex += (short)Math.Round((Level - 1) * Properties.PET_AUTOSET_DEX_MULTIPLIER);
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
        /// Changes the commander's weapon to the specified type
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="aSlot"></param>
        /// <param name="weaponType"></param>
        protected void SpiritmasterPetSwitchWeapon(eInventorySlot slot, eActiveWeaponSlot aSlot, string weapon)
        {
            if (Inventory == null)
                return;
            //all weapons removed before
            if (slot == eInventorySlot.LeftHandWeapon)
            {
                InventoryItem item = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
                if (item != null) Inventory.RemoveItem(item);
            }
            else
            {
                InventoryItem item = Inventory.GetItem(eInventorySlot.RightHandWeapon);
                if (item != null) Inventory.RemoveItem(item);
                item = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
                if (item != null) Inventory.RemoveItem(item);
                item = Inventory.GetItem(eInventorySlot.DistanceWeapon);
                if (item != null) Inventory.RemoveItem(item);
                item = Inventory.GetItem(eInventorySlot.TwoHandWeapon);
                if (item != null) Inventory.RemoveItem(item);
            }

            ItemTemplate temp = GameServer.Database.FindObjectByKey<ItemTemplate>(weapon);

            if (temp == null)
            {
                if (log.IsErrorEnabled)
                    log.Error(string.Format("Unable to find Underhill item: {0}", weapon));
                return;
            }

            Inventory.AddItem(slot, GameInventoryItem.Create<ItemTemplate>(temp));
            SwitchWeapon(aSlot);
            AddStatsToWeapon();
            BroadcastLivingEquipmentUpdate();
        }
    }
}

