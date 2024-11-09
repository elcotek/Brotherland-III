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

namespace DOL.GS.Spells
{
    using System;
    using Database;
    using Events;
    using DOL.GS.PacketHandler;
    using DOL.GS.Utils;
    using System.Collections.Generic;

    [SpellHandler("BeltOfSun")]
    public class BeltOfSun : SummonItemSpellHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ItemTemplate m_SunBlade;
        private ItemTemplate m_SunPierce;
        private ItemTemplate m_SunSlash;
        private ItemTemplate m_SunThrust;
        private ItemTemplate m_SunTwoHanded;
        private ItemTemplate m_SunCrush;
        private ItemTemplate m_SunScythe;
        private ItemTemplate m_SunFlex;
        private ItemTemplate m_SunClaw;
        private ItemTemplate m_SunAxe;
        private ItemTemplate m_SunLeftAxe;
        private ItemTemplate m_Sun2HAxe;
        private ItemTemplate m_Sun2HCrush;
        private ItemTemplate m_SunBow;
        private ItemTemplate m_SunBowH;
        private ItemTemplate m_SunStaff;
        private ItemTemplate m_SunPolearm;
        private ItemTemplate m_SunHibSpear;
        private ItemTemplate m_SunMidSpear;
        private ItemTemplate m_SunMFist;
        private ItemTemplate m_SunMStaff;
      

        public BeltOfSun(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            if (caster.CurrentRegion.IsNightTime)
            {
                MessageToCaster("The powers of the Belt of Sun, can only be Summon under the Sun light!", eChatType.CT_SpellResisted);
                return;
            }

            GamePlayer player = caster as GamePlayer;

            #region Alb
            if (player.IsCharcterClass(eCharacterClass.Armsman))
            {
              
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush_alb") ?? Crush;
                items.Add(GameInventoryItem.Create<ItemTemplate>(m_SunCrush));

               
                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash_alb") ?? Slash;
                items.Add(GameInventoryItem.Create<ItemTemplate>(m_SunSlash));

               
                m_SunThrust = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust_alb") ?? Thrust;
                items.Add(GameInventoryItem.Create<ItemTemplate>(m_SunThrust));

              
                m_SunTwoHanded = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded_alb") ?? TwoHanded;
                items.Add(GameInventoryItem.Create<ItemTemplate>(m_SunTwoHanded));

               
                m_SunPolearm = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Polearm_alb") ?? Polearm;
                items.Add(GameInventoryItem.Create<ItemTemplate>(m_SunPolearm));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.Friar))
            {
                //m_SunCrush = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Crush'") ?? Crush;

                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush_alb") ?? Crush;
                items.Add(GameInventoryItem.Create<ItemTemplate>(m_SunCrush));

                //m_SunStaff = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Staff'") ?? Staff;
                m_SunStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Staff_alb") ?? Staff;
                items.Add(GameInventoryItem.Create<ItemTemplate>(m_SunStaff));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.Heretic))
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush_alb") ?? Crush;
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunFlex = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Flex_alb") ?? Flex;
                items.Add(GameInventoryItem.Create(m_SunFlex));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.Infiltrator))
            {
                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash_alb") ?? Slash;
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunThrust = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust_alb") ?? Thrust;
                items.Add(GameInventoryItem.Create(m_SunThrust));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.Mercenary))
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush_alb") ?? Crush;
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash_alb") ?? Slash;
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunThrust = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust_alb") ?? Thrust;
                items.Add(GameInventoryItem.Create(m_SunThrust));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.Minstrel))
            {
                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash_alb") ?? Slash;
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunThrust = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust_alb") ?? Thrust;
                items.Add(GameInventoryItem.Create(m_SunThrust));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.Paladin))
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush_alb") ?? Crush;
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash_alb") ?? Slash;
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunThrust = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust_alb") ?? Thrust;
                items.Add(GameInventoryItem.Create(m_SunThrust));

                m_SunTwoHanded = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded_alb") ?? TwoHanded;
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.Reaver))
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush_alb") ?? Crush;
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash_alb") ?? Slash;
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunThrust = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust_alb") ?? Thrust;
                items.Add(GameInventoryItem.Create(m_SunThrust));

                m_SunFlex = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Flex_alb") ?? Flex;
                items.Add(GameInventoryItem.Create(m_SunFlex));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.Scout))
            {
                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash_alb") ?? Slash;
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunThrust = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust_alb") ?? Thrust;
                items.Add(GameInventoryItem.Create(m_SunThrust));

                m_SunBow = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Bow_alb") ?? Bow;
                items.Add(GameInventoryItem.Create(m_SunBow));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.MaulerAlb))
            {
                m_SunMFist = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_MFist") ?? MFist;
                items.Add(GameInventoryItem.Create(m_SunMFist));

                m_SunMStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_MStaff") ?? MStaff;
                items.Add(GameInventoryItem.Create(m_SunMStaff));
                return;
            }
            #endregion Alb

            #region Mid
            if (player.IsCharcterClass(eCharacterClass.Berserker))
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush_mid") ?? CrushM; //
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash_mid") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Axe_mid") ?? Axe; //
                items.Add(GameInventoryItem.Create(m_SunAxe));

                m_SunTwoHanded = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded_mid") ?? TwoHandedM; // 2handed Sword
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_Sun2HCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HCrush_mid") ?? THCrushM;
                items.Add(GameInventoryItem.Create(m_Sun2HCrush));

                m_Sun2HAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HAxe_mid") ?? THAxe;
                items.Add(GameInventoryItem.Create(m_Sun2HAxe));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.Hunter))
            {
                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash_mid") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunMidSpear = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Spear_mid") ?? SpearM; // Spear
                items.Add(GameInventoryItem.Create(m_SunMidSpear));

                m_SunBow = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Bow_mid") ?? BowM; //
                items.Add(GameInventoryItem.Create(m_SunBow));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.Savage))
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush_mid") ?? CrushM; //
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash_mid") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Axe_mid") ?? Axe; //
                items.Add(GameInventoryItem.Create(m_SunAxe));

                m_SunThrust = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Claw_mid") ?? Claw; //
                items.Add(GameInventoryItem.Create(m_SunThrust));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.Shadowblade))
            {
                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash_mid") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Axe_mid") ?? Axe; //
                items.Add(GameInventoryItem.Create(m_SunAxe));

                m_SunLeftAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_LeftAxe_mid") ?? LeftAxe; //
                items.Add(GameInventoryItem.Create(m_SunLeftAxe));

                m_SunTwoHanded = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded_mid") ?? TwoHandedM; // 2handed Sword
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_Sun2HAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HAxe_mid") ?? THAxe;
                items.Add(GameInventoryItem.Create(m_Sun2HAxe));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.Skald))
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush_mid") ?? CrushM; //
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash_mid") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Axe_mid") ?? Axe; //
                items.Add(GameInventoryItem.Create(m_SunAxe));

                m_SunTwoHanded = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded_mid") ?? TwoHandedM; // 2handed Sword
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_Sun2HCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HCrush_mid") ?? THCrushM;
                items.Add(GameInventoryItem.Create(m_Sun2HCrush));

                m_Sun2HAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HAxe_mid") ?? THAxe;
                items.Add(GameInventoryItem.Create(m_Sun2HAxe));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.Thane))
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush_mid") ?? CrushM; //
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash_mid") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Axe_mid") ?? Axe; //
                items.Add(GameInventoryItem.Create(m_SunAxe));

                m_SunTwoHanded = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded_mid") ?? TwoHandedM; // 2handed Sword
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_Sun2HCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HCrush_mid") ?? THCrushM;
                items.Add(GameInventoryItem.Create(m_Sun2HCrush));

                m_Sun2HAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HAxe_mid") ?? THAxe;
                items.Add(GameInventoryItem.Create(m_Sun2HAxe));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.Valkyrie))
            {
                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash_mid") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunTwoHanded = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded_mid") ?? TwoHandedM; // 2handed Sword
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_SunMidSpear = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Spear_mid") ?? SpearM; // Spear
                items.Add(GameInventoryItem.Create(m_SunMidSpear));
                return;
            }


            if (player.IsCharcterClass(eCharacterClass.Warrior))
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush_mid") ?? CrushM; //
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash_mid") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Axe_mid") ?? Axe; //
                items.Add(GameInventoryItem.Create(m_SunAxe));

                m_SunTwoHanded = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded_mid") ?? TwoHandedM; // 2handed Sword
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_Sun2HCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HCrush_mid") ?? THCrushM;
                items.Add(GameInventoryItem.Create(m_Sun2HCrush));

                m_Sun2HAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HAxe_mid") ?? THAxe;
                items.Add(GameInventoryItem.Create(m_Sun2HAxe));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.MaulerMid))
            {
                m_SunMFist = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_MFist") ?? MFist;
                items.Add(GameInventoryItem.Create(m_SunMFist));

                m_SunMStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_MStaff") ?? MStaff;
                items.Add(GameInventoryItem.Create(m_SunMStaff));
                return;
            }


            #endregion Mid

            #region Hib
            if (player.IsCharcterClass(eCharacterClass.Bard))
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush_hib") ?? CrushH; // Blunt
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunBlade = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Blade_hib") ?? BladeH; // Blades
                items.Add(GameInventoryItem.Create(m_SunBlade));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.Blademaster))
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush_hib") ?? CrushH; // Blunt
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunBlade = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Blade_hib") ?? BladeH; // Blades
                items.Add(GameInventoryItem.Create(m_SunBlade));

                m_SunPierce = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Pierce_hib") ?? SunPierceH; // Piercing
                items.Add(GameInventoryItem.Create(m_SunPierce));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.Champion))
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush_hib") ?? CrushH; // Blunt
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunBlade = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Blade_hib") ?? BladeH; // Blades
                items.Add(GameInventoryItem.Create(m_SunBlade));

                m_SunPierce = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Pierce_hib") ?? SunPierceH; // Piercing
                items.Add(GameInventoryItem.Create(m_SunPierce));

                m_SunTwoHanded = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash_hib") ?? TwoHandedH; // LargeWeapon
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.Hero))
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush_hib") ?? CrushH; // Blunt
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunBlade = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Blade_hib") ?? BladeH; // Blades
                items.Add(GameInventoryItem.Create(m_SunBlade));

                m_SunPierce = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Pierce_hib") ?? SunPierceH; // Piercing
                items.Add(GameInventoryItem.Create(m_SunPierce));

                m_SunTwoHanded = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash_hib") ?? TwoHandedH; // LargeWeapon
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_SunHibSpear = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Spear_hib") ?? SpearH; // Spear
                items.Add(GameInventoryItem.Create(m_SunHibSpear));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.Nightshade))
            {
                m_SunBlade = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Blade_hib") ?? BladeH; // Blades
                items.Add(GameInventoryItem.Create(m_SunBlade));

                m_SunPierce = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Pierce_hib") ?? SunPierceH; // Piercing
                items.Add(GameInventoryItem.Create(m_SunPierce));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.Ranger))
            {
                m_SunBlade = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Blade_hib") ?? BladeH; // Blades
                items.Add(GameInventoryItem.Create(m_SunBlade));

                m_SunPierce = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Pierce_hib") ?? SunPierceH; // Piercing
                items.Add(GameInventoryItem.Create(m_SunPierce));

                m_SunBowH = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Bow_hib") ?? BowH; //
                items.Add(GameInventoryItem.Create(m_SunBowH));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.Valewalker))
            {
                m_SunScythe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Scythe_hib") ?? Scythe;
                items.Add(GameInventoryItem.Create(m_SunScythe));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.Valewalker))
            {
                m_SunPierce = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Pierce_hib") ?? SunPierceH; // Piercing
                items.Add(GameInventoryItem.Create(m_SunPierce));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.Warden))
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush_hib") ?? CrushH; // Blunt
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunBlade = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Blade_hib") ?? BladeH; // Blades
                items.Add(GameInventoryItem.Create(m_SunBlade));
                return;
            }

            if (player.IsCharcterClass(eCharacterClass.MaulerHib))
            {
                m_SunMFist = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_MFist") ?? MFist;
                items.Add(GameInventoryItem.Create(m_SunMFist));

                m_SunMStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_MStaff") ?? MStaff;
                items.Add(GameInventoryItem.Create(m_SunMStaff));
                return;
            }

            else
            {
                player.Out.SendMessage("" + player.CharacterClass.Name + "'s cant Summon Light!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
        }
        #endregion Hib

        #region Sun Albion Weapons
        private ItemTemplate Crush
        {
            get
            {
                m_SunCrush = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush_alb");
                if (m_SunCrush == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Crush, loading it ...");
                    m_SunCrush = new ItemTemplate();
                    m_SunCrush.Id_nb = "Sun_Crush_alb";
                    m_SunCrush.Name = "Sun Mace";
                    m_SunCrush.Level = 50;
                    m_SunCrush.Durability = 50000;
                    m_SunCrush.MaxDurability = 50000;
                    m_SunCrush.Condition = 50000;
                    m_SunCrush.MaxCondition = 50000;
                    m_SunCrush.Quality = 100;
                    m_SunCrush.DPS_AF = 165;
                    m_SunCrush.SPD_ABS = 35;
                    m_SunCrush.Type_Damage = 1;
                    m_SunCrush.Object_Type = 2;
                    m_SunCrush.Item_Type = 11;
                    m_SunCrush.Hand = 2;
                    m_SunCrush.Bonus = 35;
                    m_SunCrush.Model = 1916;
                    m_SunCrush.Bonus1 = 6;
                    m_SunCrush.Bonus2 = 27;
                    m_SunCrush.Bonus3 = 2;
                    m_SunCrush.Bonus4 = 2;
                    m_SunCrush.Bonus5 = 2;
                    m_SunCrush.Bonus1Type = 25;
                    m_SunCrush.Bonus2Type = 1;
                    m_SunCrush.Bonus3Type = 173;
                    m_SunCrush.Bonus4Type = 200;
                    m_SunCrush.Bonus5Type = 155;
                    m_SunCrush.IsPickable = false;
                    m_SunCrush.IsDropable = false;
                    m_SunCrush.CanDropAsLoot = false;
                    m_SunCrush.IsTradable = false;
                    m_SunCrush.MaxCount = 1;
                    m_SunCrush.PackSize = 1;
                    m_SunCrush.ProcSpellID = 65513;
                    m_SunCrush.PackageID = "SunWeapon";

                }
                return m_SunCrush;
            }
        }

        private ItemTemplate Slash
        {
            get
            {
                m_SunSlash = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash_alb");
                if (m_SunSlash == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Slash, loading it ...");
                    m_SunSlash = new ItemTemplate();
                    m_SunSlash.Id_nb = "Sun_Slash_alb";
                    m_SunSlash.Name = "Sun Sword";
                    m_SunSlash.Level = 50;
                    m_SunSlash.Durability = 50000;
                    m_SunSlash.MaxDurability = 50000;
                    m_SunSlash.Condition = 50000;
                    m_SunSlash.MaxCondition = 50000;
                    m_SunSlash.Quality = 100;
                    m_SunSlash.DPS_AF = 165;
                    m_SunSlash.SPD_ABS = 35;
                    m_SunSlash.Type_Damage = 2;
                    m_SunSlash.Object_Type = 3;
                    m_SunSlash.Item_Type = 11;
                    m_SunSlash.Hand = 2;
                    m_SunSlash.Model = 1948;
                    m_SunSlash.Bonus = 35;
                    m_SunSlash.Bonus1 = 6;
                    m_SunSlash.Bonus2 = 27;
                    m_SunSlash.Bonus3 = 2;
                    m_SunSlash.Bonus4 = 2;
                    m_SunSlash.Bonus5 = 2;
                    m_SunSlash.Bonus1Type = 44;
                    m_SunSlash.Bonus2Type = 1;
                    m_SunSlash.Bonus3Type = 173;
                    m_SunSlash.Bonus4Type = 200;
                    m_SunSlash.Bonus5Type = 155;
                    m_SunSlash.IsPickable = false;
                    m_SunSlash.IsDropable = false;
                    m_SunSlash.CanDropAsLoot = false;
                    m_SunSlash.IsTradable = false;
                    m_SunSlash.MaxCount = 1;
                    m_SunSlash.PackSize = 1;
                    m_SunSlash.ProcSpellID = 65513;
                    m_SunSlash.PackageID = "SunWeapon";
                }
                return m_SunSlash;
            }
        }

        private ItemTemplate Thrust
        {
            get
            {
                m_SunThrust = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust_alb");
                if (m_SunThrust == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Thrust, loading it ...");
                    m_SunThrust = new ItemTemplate();
                    m_SunThrust.Id_nb = "Sun_Thrust_alb";
                    m_SunThrust.Name = "Sun Sword";
                    m_SunThrust.Level = 50;
                    m_SunThrust.Durability = 50000;
                    m_SunThrust.MaxDurability = 50000;
                    m_SunThrust.Condition = 50000;
                    m_SunThrust.MaxCondition = 50000;
                    m_SunThrust.Quality = 100;
                    m_SunThrust.DPS_AF = 165;
                    m_SunThrust.SPD_ABS = 35;
                    m_SunThrust.Type_Damage = 3;
                    m_SunThrust.Object_Type = 4;
                    m_SunThrust.Item_Type = 11;
                    m_SunThrust.Hand = 2;
                    m_SunThrust.Model = 1948;
                    m_SunThrust.Bonus1 = 35;
                    m_SunThrust.Bonus1 = 6;
                    m_SunThrust.Bonus2 = 27;
                    m_SunThrust.Bonus3 = 2;
                    m_SunThrust.Bonus4 = 2;
                    m_SunThrust.Bonus5 = 2;
                    m_SunThrust.Bonus1Type = 50;
                    m_SunThrust.Bonus2Type = 1;
                    m_SunThrust.Bonus3Type = 173;
                    m_SunThrust.Bonus4Type = 200;
                    m_SunThrust.Bonus5Type = 155;
                    m_SunThrust.IsPickable = false;
                    m_SunThrust.IsDropable = false;
                    m_SunThrust.CanDropAsLoot = false;
                    m_SunThrust.IsTradable = false;
                    m_SunThrust.MaxCount = 1;
                    m_SunThrust.PackSize = 1;
                    m_SunThrust.ProcSpellID = 65513;
                    m_SunThrust.PackageID = "SunWeapon";
                }
                return m_SunThrust;
            }
        }

        private ItemTemplate Flex
        {
            get
            {
                m_SunFlex = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Flex_alb");
                if (m_SunFlex == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Flex, loading it ...");
                    m_SunFlex = new ItemTemplate();
                    m_SunFlex.Id_nb = "Sun_Flex_alb";
                    m_SunFlex.Name = "Sun Spiked Flail";
                    m_SunFlex.Level = 50;
                    m_SunFlex.Durability = 50000;
                    m_SunFlex.MaxDurability = 50000;
                    m_SunFlex.Condition = 50000;
                    m_SunFlex.MaxCondition = 50000;
                    m_SunFlex.Quality = 100;
                    m_SunFlex.DPS_AF = 165;
                    m_SunFlex.SPD_ABS = 35;
                    m_SunFlex.Type_Damage = 2;
                    m_SunFlex.Object_Type = 24;
                    m_SunFlex.Item_Type = 10;
                    m_SunFlex.Hand = 0;
                    m_SunFlex.Model = 1924;
                    m_SunFlex.Bonus = 35;
                    m_SunFlex.Bonus1 = 6;
                    m_SunFlex.Bonus2 = 27;
                    m_SunFlex.Bonus3 = 2;
                    m_SunFlex.Bonus4 = 2;
                    m_SunFlex.Bonus5 = 2;
                    m_SunFlex.Bonus1Type = 33;
                    m_SunFlex.Bonus2Type = 1;
                    m_SunFlex.Bonus3Type = 173;
                    m_SunFlex.Bonus4Type = 200;
                    m_SunFlex.Bonus5Type = 155;
                    m_SunFlex.IsPickable = false;
                    m_SunFlex.IsDropable = false;
                    m_SunFlex.CanDropAsLoot = false;
                    m_SunFlex.IsTradable = false;
                    m_SunFlex.MaxCount = 1;
                    m_SunFlex.PackSize = 1;
                    m_SunFlex.ProcSpellID = 65513;
                    m_SunFlex.PackageID = "SunWeapon";
                }
                return m_SunFlex;
            }
        }

        private ItemTemplate Polearm
        {
            get
            {
                m_SunPolearm = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Polearm_alb");
                if (m_SunPolearm == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Polearm, loading it ...");
                    m_SunPolearm = new ItemTemplate();
                    m_SunPolearm.Id_nb = "Sun_Polearm_alb";
                    m_SunPolearm.Name = "Sun Glaive";
                    m_SunPolearm.Level = 50;
                    m_SunPolearm.Durability = 50000;
                    m_SunPolearm.MaxDurability = 50000;
                    m_SunPolearm.Condition = 50000;
                    m_SunPolearm.MaxCondition = 50000;
                    m_SunPolearm.Quality = 100;
                    m_SunPolearm.DPS_AF = 165;
                    m_SunPolearm.SPD_ABS = 52;
                    m_SunPolearm.Type_Damage = 1;
                    m_SunPolearm.Object_Type = 7;
                    m_SunPolearm.Item_Type = 12;
                    m_SunPolearm.Hand = 1;
                    m_SunPolearm.Model = 1936;
                    m_SunPolearm.Bonus = 35;
                    m_SunPolearm.Bonus1 = 6;
                    m_SunPolearm.Bonus2 = 27;
                    m_SunPolearm.Bonus3 = 2;
                    m_SunPolearm.Bonus4 = 2;
                    m_SunPolearm.Bonus5 = 2;
                    m_SunPolearm.Bonus1Type = 41;
                    m_SunPolearm.Bonus2Type = 1;
                    m_SunPolearm.Bonus3Type = 173;
                    m_SunPolearm.Bonus4Type = 200;
                    m_SunPolearm.Bonus5Type = 155;
                    m_SunPolearm.IsPickable = false;
                    m_SunPolearm.IsDropable = false;
                    m_SunPolearm.CanDropAsLoot = false;
                    m_SunPolearm.IsTradable = false;
                    m_SunPolearm.MaxCount = 1;
                    m_SunPolearm.PackSize = 1;
                    m_SunPolearm.ProcSpellID = 65513;
                    m_SunPolearm.PackageID = "SunWeapon";
                }
                return m_SunPolearm;
            }
        }

        private ItemTemplate TwoHanded
        {
            get
            {
                m_SunTwoHanded = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded_alb");
                if (m_SunTwoHanded == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_TwoHanded, loading it ...");
                    m_SunTwoHanded = new ItemTemplate();
                    m_SunTwoHanded.Id_nb = "Sun_TwoHanded_alb";
                    m_SunTwoHanded.Name = "Sun Twohanded Sword";
                    m_SunTwoHanded.Level = 50;
                    m_SunTwoHanded.Durability = 50000;
                    m_SunTwoHanded.MaxDurability = 50000;
                    m_SunTwoHanded.Condition = 50000;
                    m_SunTwoHanded.MaxCondition = 50000;
                    m_SunTwoHanded.Quality = 100;
                    m_SunTwoHanded.DPS_AF = 165;
                    m_SunTwoHanded.SPD_ABS = 52;
                    m_SunTwoHanded.Type_Damage = 2;
                    m_SunTwoHanded.Object_Type = 6;
                    m_SunTwoHanded.Item_Type = 12;
                    m_SunTwoHanded.Hand = 1;
                    m_SunTwoHanded.Model = 1904;
                    m_SunTwoHanded.Bonus = 35;
                    m_SunTwoHanded.Bonus1 = 6;
                    m_SunTwoHanded.Bonus2 = 27;
                    m_SunTwoHanded.Bonus3 = 2;
                    m_SunTwoHanded.Bonus4 = 2;
                    m_SunTwoHanded.Bonus5 = 2;
                    m_SunTwoHanded.Bonus1Type = 20;
                    m_SunTwoHanded.Bonus2Type = 1;
                    m_SunTwoHanded.Bonus3Type = 173;
                    m_SunTwoHanded.Bonus4Type = 200;
                    m_SunTwoHanded.Bonus5Type = 155;
                    m_SunTwoHanded.IsPickable = false;
                    m_SunTwoHanded.IsDropable = false;
                    m_SunTwoHanded.CanDropAsLoot = false;
                    m_SunTwoHanded.IsTradable = false;
                    m_SunTwoHanded.MaxCount = 1;
                    m_SunTwoHanded.PackSize = 1;
                    m_SunTwoHanded.ProcSpellID = 65513;
                    m_SunTwoHanded.PackageID = "SunWeapon";
                }
                return m_SunTwoHanded;
            }
        }

        private ItemTemplate Bow
        {
            get
            {
                m_SunBow = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Bow_alb");
                if (m_SunBow == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Bow, loading it ...");
                    m_SunBow = new ItemTemplate();
                    m_SunBow.Id_nb = "Sun_Bow_alb";
                    m_SunBow.Name = "Sun Bow";
                    m_SunBow.Level = 50;
                    m_SunBow.Durability = 50000;
                    m_SunBow.MaxDurability = 50000;
                    m_SunBow.Condition = 50000;
                    m_SunBow.MaxCondition = 50000;
                    m_SunBow.Quality = 100;
                    m_SunBow.DPS_AF = 165;
                    m_SunBow.SPD_ABS = 48;
                    m_SunBow.Type_Damage = 3;
                    m_SunBow.Object_Type = 9;
                    m_SunBow.Item_Type = 13;
                    m_SunBow.Hand = 1;
                    m_SunBow.Model = 1912;
                    m_SunBow.Bonus = 35;
                    m_SunBow.Bonus1 = 6;
                    m_SunBow.Bonus2 = 27;
                    m_SunBow.Bonus3 = 2;
                    m_SunBow.Bonus4 = 2;
                    m_SunBow.Bonus5 = 2;
                    m_SunBow.Bonus1Type = 36;
                    m_SunBow.Bonus2Type = 1;
                    m_SunBow.Bonus3Type = 173;
                    m_SunBow.Bonus4Type = 200;
                    m_SunBow.Bonus5Type = 155;
                    m_SunBow.IsPickable = false;
                    m_SunBow.IsDropable = false;
                    m_SunBow.CanDropAsLoot = false;
                    m_SunBow.IsTradable = false;
                    m_SunBow.MaxCount = 1;
                    m_SunBow.PackSize = 1;
                    m_SunBow.ProcSpellID = 65513;
                    m_SunBow.PackageID = "SunWeapon";
                }
                return m_SunBow;
            }
        }

        private ItemTemplate Staff
        {
            get
            {
                m_SunStaff = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Staff_alb");
                if (m_SunStaff == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Staff, loading it ...");
                    m_SunStaff = new ItemTemplate();
                    m_SunStaff.Id_nb = "Sun_Staff_alb";
                    m_SunStaff.Name = "Sun QuarterStaff";
                    m_SunStaff.Level = 50;
                    m_SunStaff.Durability = 50000;
                    m_SunStaff.MaxDurability = 50000;
                    m_SunStaff.Condition = 50000;
                    m_SunStaff.MaxCondition = 50000;
                    m_SunStaff.Quality = 100;
                    m_SunStaff.DPS_AF = 165;
                    m_SunStaff.SPD_ABS = 42;
                    m_SunStaff.Type_Damage = 1;
                    m_SunStaff.Object_Type = 8;
                    m_SunStaff.Item_Type = 12;
                    m_SunStaff.Hand = 1;
                    m_SunStaff.Model = 1952;
                    m_SunStaff.Bonus = 35;
                    m_SunStaff.Bonus1 = 6;
                    m_SunStaff.Bonus2 = 27;
                    m_SunStaff.Bonus3 = 2;
                    m_SunStaff.Bonus4 = 2;
                    m_SunStaff.Bonus5 = 2;
                    m_SunStaff.Bonus1Type = 48;
                    m_SunStaff.Bonus2Type = 1;
                    m_SunStaff.Bonus3Type = 173;
                    m_SunStaff.Bonus4Type = 200;
                    m_SunStaff.Bonus5Type = 155;
                    m_SunStaff.IsPickable = false;
                    m_SunStaff.IsDropable = false;
                    m_SunStaff.CanDropAsLoot = false;
                    m_SunStaff.IsTradable = false;
                    m_SunStaff.MaxCount = 1;
                    m_SunStaff.PackSize = 1;
                    m_SunStaff.ProcSpellID = 65513;
                    m_SunStaff.PackageID = "SunWeapon";
                }
                return m_SunStaff;
            }
        }

        private ItemTemplate MStaff
        {
            get
            {
                m_SunMStaff = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_MStaff");
                if (m_SunMStaff == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_MStaff, loading it ...");
                    m_SunMStaff = new ItemTemplate();
                    m_SunMStaff.Id_nb = "Sun_MStaff";
                    m_SunMStaff.Name = "Sun Maulers QuarterStaff";
                    m_SunMStaff.Level = 50;
                    m_SunMStaff.Durability = 50000;
                    m_SunMStaff.MaxDurability = 50000;
                    m_SunMStaff.Condition = 50000;
                    m_SunMStaff.MaxCondition = 50000;
                    m_SunMStaff.Quality = 100;
                    m_SunMStaff.DPS_AF = 165;
                    m_SunMStaff.SPD_ABS = 42;
                    m_SunMStaff.Type_Damage = 1;
                    m_SunMStaff.Object_Type = 28;
                    m_SunMStaff.Item_Type = 12;
                    m_SunMStaff.Hand = 1;
                    m_SunMStaff.Model = 1952;
                    m_SunMStaff.Bonus = 35;
                    m_SunMStaff.Bonus1 = 6;
                    m_SunMStaff.Bonus2 = 27;
                    m_SunMStaff.Bonus3 = 2;
                    m_SunMStaff.Bonus4 = 2;
                    m_SunMStaff.Bonus5 = 2;
                    m_SunMStaff.Bonus1Type = 109;
                    m_SunMStaff.Bonus2Type = 1;
                    m_SunMStaff.Bonus3Type = 173;
                    m_SunMStaff.Bonus4Type = 200;
                    m_SunMStaff.Bonus5Type = 155;
                    m_SunMStaff.IsPickable = false;
                    m_SunMStaff.IsDropable = false;
                    m_SunMStaff.CanDropAsLoot = false;
                    m_SunMStaff.IsTradable = false;
                    m_SunMStaff.MaxCount = 1;
                    m_SunMStaff.PackSize = 1;
                    m_SunMStaff.ProcSpellID = 65513;
                    m_SunMStaff.PackageID = "SunWeapon";
                }
                return m_SunMStaff;
            }
        }

        private ItemTemplate MFist
        {
            get
            {
                m_SunMFist = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_MFist");
                if (m_SunMFist == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_MFist, loading it ...");
                    m_SunMFist = new ItemTemplate();
                    m_SunMFist.Id_nb = "Sun_MFist";
                    m_SunMFist.Name = "Sun MFist";
                    m_SunMFist.Level = 50;
                    m_SunMFist.Durability = 50000;
                    m_SunMFist.MaxDurability = 50000;
                    m_SunMFist.Condition = 50000;
                    m_SunMFist.MaxCondition = 50000;
                    m_SunMFist.Quality = 100;
                    m_SunMFist.DPS_AF = 165;
                    m_SunMFist.SPD_ABS = 42;
                    m_SunMFist.Type_Damage = 1;
                    m_SunMFist.Object_Type = 27;
                    m_SunMFist.Item_Type = 11;
                    m_SunMFist.Hand = 2;
                    m_SunMFist.Model = 2028;
                    m_SunMFist.Bonus = 35;
                    m_SunMFist.Bonus1 = 6;
                    m_SunMFist.Bonus2 = 27;
                    m_SunMFist.Bonus3 = 2;
                    m_SunMFist.Bonus4 = 2;
                    m_SunMFist.Bonus5 = 2;
                    m_SunMFist.Bonus1Type = 110;
                    m_SunMFist.Bonus2Type = 1;
                    m_SunMFist.Bonus3Type = 173;
                    m_SunMFist.Bonus4Type = 200;
                    m_SunMFist.Bonus5Type = 155;
                    m_SunMFist.IsPickable = false;
                    m_SunMFist.IsDropable = false;
                    m_SunMFist.CanDropAsLoot = false;
                    m_SunMFist.IsTradable = false;
                    m_SunMFist.MaxCount = 1;
                    m_SunMFist.PackSize = 1;
                    m_SunMFist.ProcSpellID = 65513;
                    m_SunMFist.PackageID = "SunWeapon";
                }
                return m_SunMFist;
            }
        }
        #endregion Alb Weapons

        #region Sun Midgard Weapons
        private ItemTemplate CrushM
        {
            get
            {
                m_SunCrush = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush_mid");
                if (m_SunCrush == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Crush, loading it ...");
                    m_SunCrush = new ItemTemplate();
                    m_SunCrush.Id_nb = "Sun_Crush_mid";
                    m_SunCrush.Name = "Sun Warhammer";
                    m_SunCrush.Level = 50;
                    m_SunCrush.Durability = 50000;
                    m_SunCrush.MaxDurability = 50000;
                    m_SunCrush.Condition = 50000;
                    m_SunCrush.MaxCondition = 50000;
                    m_SunCrush.Quality = 100;
                    m_SunCrush.DPS_AF = 165;
                    m_SunCrush.SPD_ABS = 35;
                    m_SunCrush.Type_Damage = 1;
                    m_SunCrush.Object_Type = 12;
                    m_SunCrush.Item_Type = 10;
                    m_SunCrush.Hand = 2;
                    m_SunCrush.Model = 2044;
                    m_SunCrush.Bonus = 35;
                    m_SunCrush.Bonus1 = 6;
                    m_SunCrush.Bonus2 = 27;
                    m_SunCrush.Bonus3 = 2;
                    m_SunCrush.Bonus4 = 2;
                    m_SunCrush.Bonus5 = 2;
                    m_SunCrush.Bonus1Type = 53;
                    m_SunCrush.Bonus2Type = 1;
                    m_SunCrush.Bonus3Type = 173;
                    m_SunCrush.Bonus4Type = 200;
                    m_SunCrush.Bonus5Type = 155;
                    m_SunCrush.IsPickable = false;
                    m_SunCrush.IsDropable = false;
                    m_SunCrush.CanDropAsLoot = false;
                    m_SunCrush.IsTradable = false;
                    m_SunCrush.MaxCount = 1;
                    m_SunCrush.PackSize = 1;
                    m_SunCrush.ProcSpellID = 65513;
                    m_SunCrush.PackageID = "SunWeapon";
                }
                return m_SunCrush;
            }
        }

        private ItemTemplate SlashM
        {
            get
            {
                m_SunSlash = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash_mid");
                if (m_SunSlash == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Slash, loading it ...");
                    m_SunSlash = new ItemTemplate();
                    m_SunSlash.Id_nb = "Sun_Slash_mid";
                    m_SunSlash.Name = "Sun Sword";
                    m_SunSlash.Level = 50;
                    m_SunSlash.Durability = 50000;
                    m_SunSlash.MaxDurability = 50000;
                    m_SunSlash.Condition = 50000;
                    m_SunSlash.MaxCondition = 50000;
                    m_SunSlash.Quality = 100;
                    m_SunSlash.DPS_AF = 165;
                    m_SunSlash.SPD_ABS = 35;
                    m_SunSlash.Type_Damage = 2;
                    m_SunSlash.Object_Type = 11;
                    m_SunSlash.Item_Type = 10;
                    m_SunSlash.Hand = 2;
                    m_SunSlash.Model = 2036;
                    m_SunSlash.Bonus = 35;
                    m_SunSlash.Bonus1 = 6;
                    m_SunSlash.Bonus2 = 27;
                    m_SunSlash.Bonus3 = 2;
                    m_SunSlash.Bonus4 = 2;
                    m_SunSlash.Bonus5 = 2;
                    m_SunSlash.Bonus1Type = 52;
                    m_SunSlash.Bonus2Type = 1;
                    m_SunSlash.Bonus3Type = 173;
                    m_SunSlash.Bonus4Type = 200;
                    m_SunSlash.Bonus5Type = 155;
                    m_SunSlash.IsPickable = false;
                    m_SunSlash.IsDropable = false;
                    m_SunSlash.CanDropAsLoot = false;
                    m_SunSlash.IsTradable = false;
                    m_SunSlash.MaxCount = 1;
                    m_SunSlash.PackSize = 1;
                    m_SunSlash.ProcSpellID = 65513;
                    m_SunSlash.PackageID = "SunWeapon";
                }
                return m_SunSlash;
            }
        }

        private ItemTemplate Axe
        {
            get
            {
                m_SunAxe = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Axe_mid");
                if (m_SunAxe == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Axe, loading it ...");
                    m_SunAxe = new ItemTemplate();
                    m_SunAxe.Id_nb = "Sun_Axe_mid";
                    m_SunAxe.Name = "Sun Axe";
                    m_SunAxe.Level = 50;
                    m_SunAxe.Durability = 50000;
                    m_SunAxe.MaxDurability = 50000;
                    m_SunAxe.Condition = 50000;
                    m_SunAxe.MaxCondition = 50000;
                    m_SunAxe.Quality = 100;
                    m_SunAxe.DPS_AF = 165;
                    m_SunAxe.SPD_ABS = 41;
                    m_SunAxe.Type_Damage = 2;
                    m_SunAxe.Object_Type = 13;
                    m_SunAxe.Item_Type = 10;
                    m_SunAxe.Hand = 0;
                    m_SunAxe.Model = 2032;
                    m_SunAxe.Bonus = 35;
                    m_SunAxe.Bonus1 = 6;
                    m_SunAxe.Bonus2 = 27;
                    m_SunAxe.Bonus3 = 2;
                    m_SunAxe.Bonus4 = 2;
                    m_SunAxe.Bonus5 = 2;
                    m_SunAxe.Bonus1Type = 54;
                    m_SunAxe.Bonus2Type = 1;
                    m_SunAxe.Bonus3Type = 173;
                    m_SunAxe.Bonus4Type = 200;
                    m_SunAxe.Bonus5Type = 155;
                    m_SunAxe.IsPickable = false;
                    m_SunAxe.IsDropable = false;
                    m_SunAxe.CanDropAsLoot = false;
                    m_SunAxe.IsTradable = false;
                    m_SunAxe.MaxCount = 1;
                    m_SunAxe.PackSize = 1;
                    m_SunAxe.ProcSpellID = 65513;
                    m_SunAxe.PackageID = "SunWeapon";
                }
                return m_SunAxe;
            }
        }

        private ItemTemplate LeftAxe
        {
            get
            {
                m_SunLeftAxe = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_LeftAxe_mid");
                if (m_SunLeftAxe == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_LeftAxe, loading it ...");
                    m_SunLeftAxe = new ItemTemplate();
                    m_SunLeftAxe.Id_nb = "Sun_LeftAxe_mid";
                    m_SunLeftAxe.Name = "Sun LeftAxe";
                    m_SunLeftAxe.Level = 50;
                    m_SunLeftAxe.Durability = 50000;
                    m_SunLeftAxe.MaxDurability = 50000;
                    m_SunLeftAxe.Condition = 50000;
                    m_SunLeftAxe.MaxCondition = 50000;
                    m_SunLeftAxe.Quality = 100;
                    m_SunLeftAxe.DPS_AF = 165;
                    m_SunLeftAxe.SPD_ABS = 35;
                    m_SunLeftAxe.Type_Damage = 2;
                    m_SunLeftAxe.Object_Type = 17;
                    m_SunLeftAxe.Item_Type = 11;
                    m_SunLeftAxe.Hand = 2;
                    m_SunLeftAxe.Model = 2032;
                    m_SunLeftAxe.Bonus = 35;
                    m_SunLeftAxe.Bonus1 = 6;
                    m_SunLeftAxe.Bonus2 = 27;
                    m_SunLeftAxe.Bonus3 = 2;
                    m_SunLeftAxe.Bonus4 = 2;
                    m_SunLeftAxe.Bonus5 = 2;
                    m_SunLeftAxe.Bonus1Type = 55;
                    m_SunLeftAxe.Bonus2Type = 1;
                    m_SunLeftAxe.Bonus3Type = 173;
                    m_SunLeftAxe.Bonus4Type = 200;
                    m_SunLeftAxe.Bonus5Type = 155;
                    m_SunLeftAxe.IsPickable = false;
                    m_SunLeftAxe.IsDropable = false;
                    m_SunLeftAxe.CanDropAsLoot = false;
                    m_SunLeftAxe.IsTradable = false;
                    m_SunLeftAxe.MaxCount = 1;
                    m_SunLeftAxe.PackSize = 1;
                    m_SunLeftAxe.ProcSpellID = 65513;
                    m_SunLeftAxe.PackageID = "SunWeapon";
                }
                return m_SunLeftAxe;
            }
        }

        private ItemTemplate Claw
        {
            get
            {
                m_SunClaw = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Claw_mid");
                if (m_SunClaw == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Claw, loading it ...");
                    m_SunClaw = new ItemTemplate();
                    m_SunClaw.Id_nb = "Sun_Claw_mid";
                    m_SunClaw.Name = "Sun Claw";
                    m_SunClaw.Level = 50;
                    m_SunClaw.Durability = 50000;
                    m_SunClaw.MaxDurability = 50000;
                    m_SunClaw.Condition = 50000;
                    m_SunClaw.MaxCondition = 50000;
                    m_SunClaw.Quality = 100;
                    m_SunClaw.DPS_AF = 165;
                    m_SunClaw.SPD_ABS = 35;
                    m_SunClaw.Type_Damage = 2;
                    m_SunClaw.Object_Type = 25;
                    m_SunClaw.Item_Type = 11;
                    m_SunClaw.Hand = 2;
                    m_SunClaw.Model = 2028;
                    m_SunClaw.Bonus = 35;
                    m_SunClaw.Bonus1 = 6;
                    m_SunClaw.Bonus2 = 27;
                    m_SunClaw.Bonus3 = 2;
                    m_SunClaw.Bonus4 = 2;
                    m_SunClaw.Bonus5 = 2;
                    m_SunClaw.Bonus1Type = 92;
                    m_SunClaw.Bonus2Type = 1;
                    m_SunClaw.Bonus3Type = 173;
                    m_SunClaw.Bonus4Type = 200;
                    m_SunClaw.Bonus5Type = 155;
                    m_SunClaw.IsPickable = false;
                    m_SunClaw.IsDropable = false;
                    m_SunClaw.CanDropAsLoot = false;
                    m_SunClaw.IsTradable = false;
                    m_SunClaw.MaxCount = 1;
                    m_SunClaw.PackSize = 1;
                    m_SunClaw.ProcSpellID = 65513;
                    m_SunClaw.PackageID = "SunWeapon";
                }
                return m_SunClaw;
            }
        }

        private ItemTemplate SpearM
        {
            get
            {
                m_SunMidSpear = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Spear_mid");
                if (m_SunMidSpear == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Spear_Mid, loading it ...");
                    m_SunMidSpear = new ItemTemplate();
                    m_SunMidSpear.Id_nb = "Sun_Spear_mid";
                    m_SunMidSpear.Name = "Sun Spear";
                    m_SunMidSpear.Level = 50;
                    m_SunMidSpear.Durability = 50000;
                    m_SunMidSpear.MaxDurability = 50000;
                    m_SunMidSpear.Condition = 50000;
                    m_SunMidSpear.MaxCondition = 50000;
                    m_SunMidSpear.Quality = 100;
                    m_SunMidSpear.DPS_AF = 165;
                    m_SunMidSpear.SPD_ABS = 48;
                    m_SunMidSpear.Type_Damage = 2;
                    m_SunMidSpear.Object_Type = 14;
                    m_SunMidSpear.Item_Type = 12;
                    m_SunMidSpear.Hand = 1;
                    m_SunMidSpear.Model = 2048;
                    m_SunMidSpear.Bonus = 35;
                    m_SunMidSpear.Bonus1 = 6;
                    m_SunMidSpear.Bonus2 = 27;
                    m_SunMidSpear.Bonus3 = 2;
                    m_SunMidSpear.Bonus4 = 2;
                    m_SunMidSpear.Bonus5 = 2;
                    m_SunMidSpear.Bonus1Type = 56;
                    m_SunMidSpear.Bonus2Type = 1;
                    m_SunMidSpear.Bonus3Type = 173;
                    m_SunMidSpear.Bonus4Type = 200;
                    m_SunMidSpear.Bonus5Type = 155;
                    m_SunMidSpear.IsPickable = false;
                    m_SunMidSpear.IsDropable = false;
                    m_SunMidSpear.CanDropAsLoot = false;
                    m_SunMidSpear.IsTradable = false;
                    m_SunMidSpear.MaxCount = 1;
                    m_SunMidSpear.PackSize = 1;
                    m_SunMidSpear.ProcSpellID = 65513;
                    m_SunMidSpear.PackageID = "SunWeapon";
                }
                return m_SunMidSpear;
            }
        }

        private ItemTemplate TwoHandedM
        {
            get
            {
                m_SunTwoHanded = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded_mid");
                if (m_SunTwoHanded == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_TwoHanded, loading it ...");
                    m_SunTwoHanded = new ItemTemplate();
                    m_SunTwoHanded.Id_nb = "Sun_TwoHanded_mid";
                    m_SunTwoHanded.Name = "Sun Greater Sword";
                    m_SunTwoHanded.Level = 50;
                    m_SunTwoHanded.Durability = 50000;
                    m_SunTwoHanded.MaxDurability = 50000;
                    m_SunTwoHanded.Condition = 50000;
                    m_SunTwoHanded.MaxCondition = 50000;
                    m_SunTwoHanded.Quality = 100;
                    m_SunTwoHanded.DPS_AF = 165;
                    m_SunTwoHanded.SPD_ABS = 52;
                    m_SunTwoHanded.Type_Damage = 2;
                    m_SunTwoHanded.Object_Type = 11;
                    m_SunTwoHanded.Item_Type = 12;
                    m_SunTwoHanded.Hand = 1;
                    m_SunTwoHanded.Model = 2060;
                    m_SunTwoHanded.Bonus = 35;
                    m_SunTwoHanded.Bonus1 = 6;
                    m_SunTwoHanded.Bonus2 = 27;
                    m_SunTwoHanded.Bonus3 = 2;
                    m_SunTwoHanded.Bonus4 = 2;
                    m_SunTwoHanded.Bonus5 = 2;
                    m_SunTwoHanded.Bonus1Type = 52;
                    m_SunTwoHanded.Bonus2Type = 1;
                    m_SunTwoHanded.Bonus3Type = 173;
                    m_SunTwoHanded.Bonus4Type = 200;
                    m_SunTwoHanded.Bonus5Type = 155;
                    m_SunTwoHanded.IsPickable = false;
                    m_SunTwoHanded.IsDropable = false;
                    m_SunTwoHanded.CanDropAsLoot = false;
                    m_SunTwoHanded.IsTradable = false;
                    m_SunTwoHanded.MaxCount = 1;
                    m_SunTwoHanded.PackSize = 1;
                    m_SunTwoHanded.ProcSpellID = 65513;
                    m_SunTwoHanded.PackageID = "SunWeapon";
                }
                return m_SunTwoHanded;
            }
        }

        private ItemTemplate BowM
        {
            get
            {
                m_SunBow = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Bow_mid");
                if (m_SunBow == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Bow, loading it ...");
                    m_SunBow = new ItemTemplate();
                    m_SunBow.Id_nb = "Sun_Bow_mid";
                    m_SunBow.Name = "Sun Bow";
                    m_SunBow.Level = 50;
                    m_SunBow.Durability = 50000;
                    m_SunBow.MaxDurability = 50000;
                    m_SunBow.Condition = 50000;
                    m_SunBow.MaxCondition = 50000;
                    m_SunBow.Quality = 100;
                    m_SunBow.DPS_AF = 165;
                    m_SunBow.SPD_ABS = 48;
                    m_SunBow.Type_Damage = 3;
                    m_SunBow.Object_Type = 15;
                    m_SunBow.Item_Type = 13;
                    m_SunBow.Hand = 1;
                    m_SunBow.Model = 2064;
                    m_SunBow.Bonus = 35;
                    m_SunBow.Bonus1 = 6;
                    m_SunBow.Bonus2 = 27;
                    m_SunBow.Bonus3 = 2;
                    m_SunBow.Bonus4 = 2;
                    m_SunBow.Bonus5 = 2;
                    m_SunBow.Bonus1Type = 68;
                    m_SunBow.Bonus2Type = 1;
                    m_SunBow.Bonus3Type = 173;
                    m_SunBow.Bonus4Type = 200;
                    m_SunBow.Bonus5Type = 155;
                    m_SunBow.IsPickable = false;
                    m_SunBow.IsDropable = false;
                    m_SunBow.CanDropAsLoot = false;
                    m_SunBow.IsTradable = false;
                    m_SunBow.MaxCount = 1;
                    m_SunBow.PackSize = 1;
                    m_SunBow.ProcSpellID = 65513;
                    m_SunBow.PackageID = "SunWeapon";
                }
                return m_SunBow;
            }
        }

        private ItemTemplate THCrushM
        {
            get
            {
                m_Sun2HCrush = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HCrush_mid");
                if (m_Sun2HCrush == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_2HCrush, loading it ...");
                    m_Sun2HCrush = new ItemTemplate();
                    m_Sun2HCrush.Id_nb = "Sun_2HCrush_mid";
                    m_Sun2HCrush.Name = "Sun Greater Warhammer";
                    m_Sun2HCrush.Level = 50;
                    m_Sun2HCrush.Durability = 50000;
                    m_Sun2HCrush.MaxDurability = 50000;
                    m_Sun2HCrush.Condition = 50000;
                    m_Sun2HCrush.MaxCondition = 50000;
                    m_Sun2HCrush.Quality = 100;
                    m_Sun2HCrush.DPS_AF = 165;
                    m_Sun2HCrush.SPD_ABS = 52;
                    m_Sun2HCrush.Type_Damage = 1;
                    m_Sun2HCrush.Object_Type = 12;
                    m_Sun2HCrush.Item_Type = 12;
                    m_Sun2HCrush.Hand = 1;
                    m_Sun2HCrush.Model = 2056;
                    m_Sun2HCrush.Bonus = 35;
                    m_Sun2HCrush.Bonus1 = 6;
                    m_Sun2HCrush.Bonus2 = 27;
                    m_Sun2HCrush.Bonus3 = 2;
                    m_Sun2HCrush.Bonus4 = 2;
                    m_Sun2HCrush.Bonus5 = 2;
                    m_Sun2HCrush.Bonus1Type = 53;
                    m_Sun2HCrush.Bonus2Type = 1;
                    m_Sun2HCrush.Bonus3Type = 173;
                    m_Sun2HCrush.Bonus4Type = 200;
                    m_Sun2HCrush.Bonus5Type = 155;
                    m_Sun2HCrush.IsPickable = false;
                    m_Sun2HCrush.IsDropable = false;
                    m_Sun2HCrush.CanDropAsLoot = false;
                    m_Sun2HCrush.IsTradable = false;
                    m_Sun2HCrush.MaxCount = 1;
                    m_Sun2HCrush.PackSize = 1;
                    m_Sun2HCrush.ProcSpellID = 65513;
                    m_Sun2HCrush.PackageID = "SunWeapon";
                }
                return m_Sun2HCrush;
            }
        }

        private ItemTemplate THAxe
        {
            get
            {
                m_Sun2HAxe = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HAxe_mid");
                if (m_Sun2HAxe == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_2HAxe, loading it ...");
                    m_Sun2HAxe = new ItemTemplate();
                    m_Sun2HAxe.Id_nb = "Sun_2HAxe_mid";
                    m_Sun2HAxe.Name = "Sun Greater Axe";
                    m_Sun2HAxe.Level = 50;
                    m_Sun2HAxe.Durability = 50000;
                    m_Sun2HAxe.MaxDurability = 50000;
                    m_Sun2HAxe.Condition = 50000;
                    m_Sun2HAxe.MaxCondition = 50000;
                    m_Sun2HAxe.Quality = 100;
                    m_Sun2HAxe.DPS_AF = 165;
                    m_Sun2HAxe.SPD_ABS = 52;
                    m_Sun2HAxe.Type_Damage = 2;
                    m_Sun2HAxe.Object_Type = 13;
                    m_Sun2HAxe.Item_Type = 12;
                    m_Sun2HAxe.Hand = 1;
                    m_Sun2HAxe.Model = 2052;
                    m_Sun2HAxe.Bonus = 35;
                    m_Sun2HAxe.Bonus1 = 6;
                    m_Sun2HAxe.Bonus2 = 27;
                    m_Sun2HAxe.Bonus3 = 2;
                    m_Sun2HAxe.Bonus4 = 2;
                    m_Sun2HAxe.Bonus5 = 2;
                    m_Sun2HAxe.Bonus1Type = 54;
                    m_Sun2HAxe.Bonus2Type = 1;
                    m_Sun2HAxe.Bonus3Type = 173;
                    m_Sun2HAxe.Bonus4Type = 200;
                    m_Sun2HAxe.Bonus5Type = 155;
                    m_Sun2HAxe.IsPickable = false;
                    m_Sun2HAxe.IsDropable = false;
                    m_Sun2HAxe.CanDropAsLoot = false;
                    m_Sun2HAxe.IsTradable = false;
                    m_Sun2HAxe.MaxCount = 1;
                    m_Sun2HAxe.PackSize = 1;
                    m_Sun2HAxe.ProcSpellID = 65513;
                    m_Sun2HAxe.PackageID = "SunWeapon";
                }
                return m_Sun2HAxe;
            }
        }

        #endregion Mid Weapons

        #region Sun Hibernia Weapons
        private ItemTemplate CrushH
        {
            get
            {
                m_SunCrush = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush_hib");
                if (m_SunCrush == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Crush, loading it ...");
                    m_SunCrush = new ItemTemplate();
                    m_SunCrush.Id_nb = "Sun_Crush_hib";
                    m_SunCrush.Name = "Sun Hammer";
                    m_SunCrush.Level = 50;
                    m_SunCrush.Durability = 50000;
                    m_SunCrush.MaxDurability = 50000;
                    m_SunCrush.Condition = 50000;
                    m_SunCrush.MaxCondition = 50000;
                    m_SunCrush.Quality = 100;
                    m_SunCrush.DPS_AF = 165;
                    m_SunCrush.SPD_ABS = 35;
                    m_SunCrush.Type_Damage = 1;
                    m_SunCrush.Object_Type = 20;
                    m_SunCrush.Item_Type = 11;
                    m_SunCrush.Hand = 2;
                    m_SunCrush.Model = 1988;
                    m_SunCrush.Bonus = 35;
                    m_SunCrush.Bonus1 = 6;
                    m_SunCrush.Bonus2 = 27;
                    m_SunCrush.Bonus3 = 2;
                    m_SunCrush.Bonus4 = 2;
                    m_SunCrush.Bonus5 = 2;
                    m_SunCrush.Bonus1Type = 73;
                    m_SunCrush.Bonus2Type = 1;
                    m_SunCrush.Bonus3Type = 173;
                    m_SunCrush.Bonus4Type = 200;
                    m_SunCrush.Bonus5Type = 155;
                    m_SunCrush.IsPickable = false;
                    m_SunCrush.IsDropable = false;
                    m_SunCrush.CanDropAsLoot = false;
                    m_SunCrush.IsTradable = false;
                    m_SunCrush.MaxCount = 1;
                    m_SunCrush.PackSize = 1;
                    m_SunCrush.ProcSpellID = 65513;
                    m_SunCrush.PackageID = "SunWeapon";
                }
                return m_SunCrush;
            }
        }
      
        private ItemTemplate BladeH
        {
            get
            {
                m_SunBlade = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_blade_hib");
                if (m_SunBlade == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_blade, loading it ...");
                    m_SunBlade = new ItemTemplate();
                    m_SunBlade.Id_nb = "Sun_blade_hib";
                    m_SunBlade.Name = "Sun Blade";
                    m_SunBlade.Level = 50;
                    m_SunBlade.Durability = 50000;
                    m_SunBlade.MaxDurability = 50000;
                    m_SunBlade.Condition = 50000;
                    m_SunBlade.MaxCondition = 50000;
                    m_SunBlade.Quality = 100;
                    m_SunBlade.DPS_AF = 165;
                    m_SunBlade.SPD_ABS = 35;
                    m_SunBlade.Type_Damage = 2;
                    m_SunBlade.Object_Type = 19;
                    m_SunBlade.Item_Type = 11;
                    m_SunBlade.Hand = 2;
                    m_SunBlade.Model = 1948;
                    m_SunBlade.Bonus = 35;
                    m_SunBlade.Bonus1 = 6;
                    m_SunBlade.Bonus2 = 27;
                    m_SunBlade.Bonus3 = 2;
                    m_SunBlade.Bonus4 = 2;
                    m_SunBlade.Bonus5 = 2;
                    m_SunBlade.Bonus1Type = 72;
                    m_SunBlade.Bonus2Type = 1;
                    m_SunBlade.Bonus3Type = 173;
                    m_SunBlade.Bonus4Type = 200;
                    m_SunBlade.Bonus5Type = 155;
                    m_SunBlade.IsPickable = false;
                    m_SunBlade.IsDropable = false;
                    m_SunBlade.CanDropAsLoot = false;
                    m_SunBlade.IsTradable = false;
                    m_SunBlade.MaxCount = 1;
                    m_SunBlade.PackSize = 1;
                    m_SunBlade.ProcSpellID = 65513;
                    m_SunBlade.PackageID = "SunWeapon";
                }
                return m_SunBlade;
            }
        }

        private ItemTemplate SunPierceH
        {
            get
            {
                m_SunPierce = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Pierce_hib");
                if (m_SunPierce == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Pierce, loading it ...");
                    m_SunPierce = new ItemTemplate();
                    m_SunPierce.Id_nb = "Sun_Pierce_hib";
                    m_SunPierce.Name = "Sun Pierce";
                    m_SunPierce.Level = 50;
                    m_SunPierce.Durability = 50000;
                    m_SunPierce.MaxDurability = 50000;
                    m_SunPierce.Condition = 50000;
                    m_SunPierce.MaxCondition = 50000;
                    m_SunPierce.Quality = 100;
                    m_SunPierce.DPS_AF = 165;
                    m_SunPierce.SPD_ABS = 35;
                    m_SunPierce.Type_Damage = 3;
                    m_SunPierce.Object_Type = 21;
                    m_SunPierce.Item_Type = 11;
                    m_SunPierce.Hand = 2;
                    m_SunPierce.Model = 1948;
                    m_SunPierce.Bonus = 35;
                    m_SunPierce.Bonus1 = 6;
                    m_SunPierce.Bonus2 = 27;
                    m_SunPierce.Bonus3 = 2;
                    m_SunPierce.Bonus4 = 2;
                    m_SunPierce.Bonus5 = 2;
                    m_SunPierce.Bonus1Type = 74;
                    m_SunPierce.Bonus2Type = 1;
                    m_SunPierce.Bonus3Type = 173;
                    m_SunPierce.Bonus4Type = 200;
                    m_SunPierce.Bonus5Type = 155;
                    m_SunPierce.IsPickable = false;
                    m_SunPierce.IsDropable = false;
                    m_SunPierce.CanDropAsLoot = false;
                    m_SunPierce.IsTradable = false;
                    m_SunPierce.MaxCount = 1;
                    m_SunPierce.PackSize = 1;
                    m_SunPierce.ProcSpellID = 65513;
                    m_SunPierce.PackageID = "SunWeapon";
                }
                return m_SunPierce;
            }
        }


        private ItemTemplate Scythe
        {
            get
            {
                m_SunScythe = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Scythe_hib");
                if (m_SunScythe == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Scythe, loading it ...");
                    m_SunScythe = new ItemTemplate();
                    m_SunScythe.Id_nb = "Sun_Scythe_hib";
                    m_SunScythe.Name = "Sun Scythe";
                    m_SunScythe.Level = 50;
                    m_SunScythe.Durability = 50000;
                    m_SunScythe.MaxDurability = 50000;
                    m_SunScythe.Condition = 50000;
                    m_SunScythe.MaxCondition = 50000;
                    m_SunScythe.Quality = 100;
                    m_SunScythe.DPS_AF = 165;
                    m_SunScythe.SPD_ABS = 35;
                    m_SunScythe.Type_Damage = 2;
                    m_SunScythe.Object_Type = 26;
                    m_SunScythe.Item_Type = 12;
                    m_SunScythe.Model = 2004;
                    m_SunScythe.Hand = 1;
                    m_SunScythe.Bonus = 35;
                    m_SunScythe.Bonus1 = 6;
                    m_SunScythe.Bonus2 = 27;
                    m_SunScythe.Bonus3 = 2;
                    m_SunScythe.Bonus4 = 2;
                    m_SunScythe.Bonus5 = 2;
                    m_SunScythe.Bonus1Type = 90;
                    m_SunScythe.Bonus2Type = 1;
                    m_SunScythe.Bonus3Type = 173;
                    m_SunScythe.Bonus4Type = 200;
                    m_SunScythe.Bonus5Type = 155;
                    m_SunScythe.IsPickable = false;
                    m_SunScythe.IsDropable = false;
                    m_SunScythe.CanDropAsLoot = false;
                    m_SunScythe.IsTradable = false;
                    m_SunScythe.MaxCount = 1;
                    m_SunScythe.PackSize = 1;
                    m_SunScythe.ProcSpellID = 65513;
                    m_SunScythe.PackageID = "SunWeapon";
                }
                return m_SunScythe;
            }
        }

        private ItemTemplate SpearH
        {
            get
            {
                m_SunHibSpear = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Spear_hib");
                if (m_SunHibSpear == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Spear_Hib, loading it ...");
                    m_SunHibSpear = new ItemTemplate();
                    m_SunHibSpear.Id_nb = "Sun_Spear_hib";
                    m_SunHibSpear.Name = "Sun Spear";
                    m_SunHibSpear.Level = 50;
                    m_SunHibSpear.Durability = 50000;
                    m_SunHibSpear.MaxDurability = 50000;
                    m_SunHibSpear.Condition = 50000;
                    m_SunHibSpear.MaxCondition = 50000;
                    m_SunHibSpear.Quality = 100;
                    m_SunHibSpear.DPS_AF = 165;
                    m_SunHibSpear.SPD_ABS = 52;
                    m_SunHibSpear.Type_Damage = 2;
                    m_SunHibSpear.Object_Type = 23;
                    m_SunHibSpear.Item_Type = 12;
                    m_SunHibSpear.Hand = 1;
                    m_SunHibSpear.Model = 2008;
                    m_SunHibSpear.Bonus = 35;
                    m_SunHibSpear.Bonus1 = 6;
                    m_SunHibSpear.Bonus2 = 27;
                    m_SunHibSpear.Bonus3 = 2;
                    m_SunHibSpear.Bonus4 = 2;
                    m_SunHibSpear.Bonus5 = 2;
                    m_SunHibSpear.Bonus1Type = 82;
                    m_SunHibSpear.Bonus2Type = 1;
                    m_SunHibSpear.Bonus3Type = 173;
                    m_SunHibSpear.Bonus4Type = 200;
                    m_SunHibSpear.Bonus5Type = 155;
                    m_SunHibSpear.IsPickable = false;
                    m_SunHibSpear.IsDropable = false;
                    m_SunHibSpear.CanDropAsLoot = false;
                    m_SunHibSpear.IsTradable = false;
                    m_SunHibSpear.MaxCount = 1;
                    m_SunHibSpear.PackSize = 1;
                    m_SunHibSpear.ProcSpellID = 65513;
                    m_SunHibSpear.PackageID = "SunWeapon";
                }
                return m_SunHibSpear;
            }
        }

        private ItemTemplate TwoHandedH
        {
            get
            {
                m_SunTwoHanded = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded_hib");
                if (m_SunTwoHanded == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_TwoHanded, loading it ...");
                    m_SunTwoHanded = new ItemTemplate();
                    m_SunTwoHanded.Id_nb = "Sun_TwoHanded_hib";
                    m_SunTwoHanded.Name = "Sun Large Weapon";
                    m_SunTwoHanded.Level = 50;
                    m_SunTwoHanded.Durability = 50000;
                    m_SunTwoHanded.MaxDurability = 50000;
                    m_SunTwoHanded.Condition = 50000;
                    m_SunTwoHanded.MaxCondition = 50000;
                    m_SunTwoHanded.Quality = 100;
                    m_SunTwoHanded.DPS_AF = 165;
                    m_SunTwoHanded.SPD_ABS = 52;
                    m_SunTwoHanded.Type_Damage = 2;
                    m_SunTwoHanded.Object_Type = 22;
                    m_SunTwoHanded.Item_Type = 12;
                    m_SunTwoHanded.Hand = 1;
                    m_SunTwoHanded.Model = 1984;
                    m_SunTwoHanded.Bonus = 35;
                    m_SunTwoHanded.Bonus1 = 6;
                    m_SunTwoHanded.Bonus2 = 27;
                    m_SunTwoHanded.Bonus3 = 2;
                    m_SunTwoHanded.Bonus4 = 2;
                    m_SunTwoHanded.Bonus5 = 2;
                    m_SunTwoHanded.Bonus1Type = 75;
                    m_SunTwoHanded.Bonus2Type = 1;
                    m_SunTwoHanded.Bonus3Type = 173;
                    m_SunTwoHanded.Bonus4Type = 200;
                    m_SunTwoHanded.Bonus5Type = 155;
                    m_SunTwoHanded.IsPickable = false;
                    m_SunTwoHanded.IsDropable = false;
                    m_SunTwoHanded.CanDropAsLoot = false;
                    m_SunTwoHanded.IsTradable = false;
                    m_SunTwoHanded.MaxCount = 1;
                    m_SunTwoHanded.PackSize = 1;
                    m_SunTwoHanded.ProcSpellID = 65513;
                    m_SunTwoHanded.PackageID = "SunWeapon";
                }
                return m_SunTwoHanded;
            }
        }

        private ItemTemplate BowH
        {
            get
            {
                m_SunBowH = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Bow_hib");
                if (m_SunBowH == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Bow_Hib, loading it ...");
                    m_SunBowH = new ItemTemplate();
                    m_SunBowH.Id_nb = "Sun_Bow_hib";
                    m_SunBowH.Name = "Sun Bow";
                    m_SunBowH.Level = 50;
                    m_SunBowH.Durability = 50000;
                    m_SunBowH.MaxDurability = 50000;
                    m_SunBowH.Condition = 50000;
                    m_SunBowH.MaxCondition = 50000;
                    m_SunBowH.Quality = 100;
                    m_SunBowH.DPS_AF = 165;
                    m_SunBowH.SPD_ABS = 48;
                    m_SunBowH.Type_Damage = 3;
                    m_SunBowH.Object_Type = 18;
                    m_SunBowH.Item_Type = 13;
                    m_SunBowH.Hand = 1;
                    m_SunBowH.Model = 1996;
                    m_SunBowH.Bonus = 35;
                    m_SunBowH.Bonus1 = 6;
                    m_SunBowH.Bonus2 = 27;
                    m_SunBowH.Bonus3 = 2;
                    m_SunBowH.Bonus4 = 2;
                    m_SunBowH.Bonus5 = 2;
                    m_SunBowH.Bonus1Type = 83;
                    m_SunBowH.Bonus2Type = 1;
                    m_SunBowH.Bonus3Type = 173;
                    m_SunBowH.Bonus4Type = 200;
                    m_SunBowH.Bonus5Type = 155;
                    m_SunBowH.IsPickable = false;
                    m_SunBowH.IsDropable = false;
                    m_SunBowH.CanDropAsLoot = false;
                    m_SunBowH.IsTradable = false;
                    m_SunBowH.MaxCount = 1;
                    m_SunBowH.PackSize = 1;
                    m_SunBowH.ProcSpellID = 65513;
                    m_SunBowH.PackageID = "SunWeapon";
                }
                return m_SunBowH;
            }
        }

        #endregion Hib Weapons


        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            base.OnDirectEffect(target, effectiveness);
            GameEventMgr.AddHandler(Caster, GamePlayerEvent.Released, OnPlayerReleased);
            GameEventMgr.AddHandler(Caster, GamePlayerEvent.Quit, OnPlayerLeft);
        }


        private static void OnPlayerReleased(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(sender is GamePlayer))
                return;

            GamePlayer player = sender as GamePlayer;

            lock (player.Inventory)
            {
                var items = player.Inventory.GetItemRange(eInventorySlot.MinEquipable, eInventorySlot.LastBackpack);
                foreach (InventoryItem invItem in items)
                {
                    if (player.CurrentRegion.IsNightTime)
                    {
                        if (invItem.PackageID.Equals("SunWeapon"))
                            player.Inventory.RemoveItem(invItem);


                        player.Out.SendMessage("The Power of Belt of Sun, has left you!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                }
            }
            GameEventMgr.RemoveHandler(sender, GamePlayerEvent.Released, OnPlayerReleased);
        }

        private static void OnPlayerLeft(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(sender is GamePlayer))
                return;

            GamePlayer player = sender as GamePlayer;
            lock (player.Inventory)
            {
                var items = player.Inventory.GetItemRange(eInventorySlot.MinEquipable, eInventorySlot.LastBackpack);
                foreach (InventoryItem invItem in items)
                {
                    if (invItem.PackageID.Equals("SunWeapon"))
                        player.Inventory.RemoveItem(invItem);
                }
            }
            GameEventMgr.RemoveHandler(sender, GamePlayerEvent.Quit, OnPlayerLeft);
        }
    }
}