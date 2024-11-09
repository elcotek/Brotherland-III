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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using System;
using System.Collections.Generic;




namespace DOL.GS.Spells
{
    /// <summary>
    ///
    /// </summary>
    [SpellHandlerAttribute("OmniLifedrain")]
    public class OmniLifedrainSpellHandler : DirectDamageSpellHandler
    {
        /// <summary>
        /// execute direct effect
        /// </summary>
        /// <param>target that gets the damage</param>
        /// <param>factor from 0..1 (0%-100%)</param>
        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (target == null) return;
            if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;

            // calc damage and healing
            AttackData ad = CalculateDamageToTarget(target, effectiveness);
            SendDamageMessages(ad);
            DamageTarget(ad, false, false);

            StealLife(ad);
            StealEndo(ad);
            StealPower(ad);

            if (Spell.Name == "OmniTap")
            {
                SendEffectAnimation(Caster, 0, false, 1);
            }
                   

            MessageToLiving(Caster, Spell.Message1, eChatType.CT_Spell);
            Message.SystemToArea(ad.Target, Util.MakeSentence(Spell.Message2, ad.Target.GetName(0, true)), eChatType.CT_Spell, ad.Target);
            target.StartInterruptTimer(target.SpellInterruptDuration, ad.AttackType, Caster);
        }

        /// <summary>
        /// Uses percent of damage to heal the caster
        /// </summary>
        public virtual void StealLife(AttackData ad)
        {
            int heal = 0;

            if (ad == null) return;
            if (!m_caster.IsAlive) return;

            if (m_caster is GamePlayer && Spell.IsProcSpell)
            {
                heal = (ad.Damage) * Spell.LifeDrainReturn / 100; // % factor on all drains
            }
            else 
            {
                heal = (ad.Damage + ad.CriticalDamage) * Spell.LifeDrainReturn / 100; // % factor on all drains
            }

            if (m_caster.IsDiseased)
            {
                MessageToCaster("You are diseased!", eChatType.CT_Important);
                heal >>= 1;
            }

            heal = m_caster.ChangeHealth(m_caster, GameLiving.eHealthChangeType.Spell, heal);

            if (heal > 0)
            {
                MessageToCaster("You steal " + heal.ToString() + " hit point" + (heal == 1 ? "." : "s."), eChatType.CT_Spell);


                #region PVP DAMAGE

                if (m_caster is NecromancerPet && ((m_caster as NecromancerPet).Brain as IControlledBrain).GetPlayerOwner() != null || m_caster is GamePlayer)
                {
                    if (m_caster.DamageRvRMemory > 0)
                        m_caster.DamageRvRMemory -= (long)Math.Max(heal, 0);
                }

                #endregion PVP DAMAGE

            }
            else
            {
                MessageToCaster("You cannot absorb any more life.", eChatType.CT_Important);

                #region PVP DAMAGE

                if (m_caster is NecromancerPet && ((m_caster as NecromancerPet).Brain as IControlledBrain).GetPlayerOwner() != null || m_caster is GamePlayer)
                {
                    if (m_caster.DamageRvRMemory > 0)
                        m_caster.DamageRvRMemory = 0; //Remise a z�ro compteur dommages/heal rps
                }
                #endregion PVP DAMAGE
            }
        }
        /// <summary>
        /// Uses percent of damage to renew endurance
        /// </summary>
        public virtual void StealEndo(AttackData ad)
        {
            int renew = 0;

            if (ad == null) return;
            if (!m_caster.IsAlive) return;

            if (m_caster is GamePlayer && Spell.IsProcSpell)
            {
                renew = (ad.Damage * Spell.ResurrectHealth / 100) * Spell.LifeDrainReturn / 100; // %endo returned
            }
            else
            {
                renew = ((ad.Damage + ad.CriticalDamage) * Spell.ResurrectHealth / 100) * Spell.LifeDrainReturn / 100; // %endo returned
            }
            renew = m_caster.ChangeEndurance(m_caster, GameLiving.eEnduranceChangeType.Spell, renew);
            if (renew > 0)
            {
                MessageToCaster("You steal " + renew.ToString() + " endurance.", eChatType.CT_Spell);
            }
            else
            {
                MessageToCaster("You cannot steal any more endurance.", eChatType.CT_Important);
            }
        }
        /// <summary>
        /// Uses percent of damage to replenish power
        /// </summary>
        public virtual void StealPower(AttackData ad)
        {
            if (ad == null) return;
            if (!m_caster.IsAlive) return;

            int replenish = ((ad.Damage + ad.CriticalDamage) * Spell.ResurrectMana / 100) * Spell.LifeDrainReturn / 100; // %mana returned
            replenish = m_caster.ChangeMana(m_caster, GameLiving.eManaChangeType.Spell, replenish);
            if (replenish > 0)
            {
                MessageToCaster("You steal " + replenish.ToString() + " power.", eChatType.CT_Spell);
            }
            else
            {
                MessageToCaster("Your power is already full.", eChatType.CT_Important);
            }
        }

        /// <summary>
        /// Calculates the base 100% spell damage which is then modified by damage variance factors
        /// </summary>
        /// <returns></returns>
        public override double CalculateDamageBase(GameLiving target)
        {
            double spellDamage = Spell.Damage;
            return spellDamage;
        }

        // constructor
        public OmniLifedrainSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }


        public virtual IList<string> DelveInfo(GameLiving target)
        {

            {
                List<string> list = new List<string>
                {
                    //Name
                    "omni-lifedrain \n",
                    //Description
                    "Damages the target. A portion of damage is returned to the caster as health, power, and endurance.\n",
                    "Damage: " + Spell.Damage,
                    "Health returned: " + Spell.LifeDrainReturn.ToString() + "% of damage dealt \n Power returned: " + Spell.ResurrectMana.ToString() + "% of damage dealt \n Endurance returned: " + Spell.ResurrectHealth.ToString() + "% of damage dealt",
                    "Target: " + Spell.Target
                };
                if (Spell.Range != 0) list.Add("Range: " + Spell.Range.ToString());
                list.Add("Casting time: " + (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
                if (Spell.DamageType != eDamageType.Natural)
                    list.Add("Damage: " + GlobalConstants.DamageTypeToName(Spell.DamageType));
                return list;
            }
        }
    }
}