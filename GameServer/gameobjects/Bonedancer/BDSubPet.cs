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
using DOL.GS.ServerProperties;
using System;

namespace DOL.GS
{
    public class BDSubPet : BDPet
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Holds the different subpet ids
        /// </summary>
        public enum SubPetType : byte
        {
            Melee = 0,
            Healer = 1,
            Caster = 2,
            Debuffer = 3,
            Buffer = 4,
            Archer = 5
        }

        /// <summary>
        /// Create a commander.
        /// </summary>
        /// <param name="npcTemplate"></param>
        /// <param name="owner"></param>
        public BDSubPet(INpcTemplate npcTemplate) : base(npcTemplate) { }


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

        protected short m_bd_subpetDex;

        /// <summary>
        /// Base dexterity. Make greater necroservant slightly more dextrous than
        /// all the other pets.
        /// </summary>
        public override short Dexterity
        {
            get
            {
                return m_bd_subpetDex += (short)((Level - 1) / 2);

            }
            set
            {
                value = m_bd_subpetDex;
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
            m_bd_subpetDex = (short)Math.Max(1, Properties.PET_AUTOSET_DEX_BASE);
            Intelligence = (short)Math.Max(1, Properties.PET_AUTOSET_INT_BASE);
            Empathy = (short)30;
            Piety = (short)30;
            Charisma = (short)30;

            // Now add stats for levelling
            Strength += (short)Math.Round(10.0 * (Level - 1) * Properties.PET_AUTOSET_STR_MULTIPLIER);
            Constitution += (short)Math.Round((Level - 1) * Properties.PET_AUTOSET_CON_MULTIPLIER);
            Quickness += (short)Math.Round((Level - 1) * Properties.PET_AUTOSET_QUI_MULTIPLIER);
            m_bd_subpetDex += (short)Math.Round((Level - 1) * Properties.PET_AUTOSET_DEX_MULTIPLIER);
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
        public override short MaxSpeed
        {
            get
            {
                if ((Brain as IControlledBrain).Owner.CurrentSpeed > 0)
                {
                    return (Brain as IControlledBrain).Owner.CurrentSpeed;
                }
                else
                    return (Brain as IControlledBrain).Owner.MaxSpeed;
            }
        }

    }
}