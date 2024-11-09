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
using System;
using System.Collections.Generic;
using System.Text;
using DOL.GS.Effects;
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using DOL.Events;
using DOL.GS.PropertyCalc;
using System.Collections;
using DOL.Language;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Spell handler to summon a bonedancer pet.
    /// </summary>
    /// <author>IST</author>
    [SpellHandler("SummonMobPet")]
    public class SummonMobPet : SummonSpellHandler
    {

        public SummonMobPet(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }


        


        // protected override GamePet GetGamePet(INpcTemplate template)
        // {
        //     return new NecromancerPet(template, m_summonConBonus, m_summonHitsBonus);
        // }
        /// <summary>
        /// Check whether it's possible to summon a pet.
        /// </summary>
        /// <param name="selectedTarget"></param>
        /// <returns></returns>
        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (Caster is GamePlayer)
            {
                //MessageToCaster("You have too many controlled creatures!", eChatType.CT_SpellResisted);
                return false;
            }


            if (Caster is GameNPC && ((GameNPC)Caster).ControlledBrain != null && Caster.PetCount >= this.Spell.ResurrectMana)
            {
                //MessageToCaster("You have too many controlled creatures!", eChatType.CT_SpellResisted);
                return false;
            }

            /*
                 foreach (GameNPC npc in Caster.GetNPCsInRadius(3000))
                 {
                     if (!(npc is GamePet) || npc != null)
                     {
                         continue;
                     }
                     if (Caster.PetCount > 0 && npc == null)
                     {
                         Caster.PetCount = 0;
                     }
                     // if (Caster.IsControlledNPC(npc))
                     //{
                     //PetCounter is decremented when pet die.
                     //    npc.Die(Caster);
                 }
             */

            if (Caster is GamePlayer && Caster.PetCount >= 1)
            {
                MessageToCaster("You have too many controlled creatures!", eChatType.CT_SpellResisted);
                return false;
            }

            if (Caster is GamePlayer && Caster.ControlledBrain != null)
            {
                //MessageToCaster("You already have a charmed creature, release it first!", eChatType.CT_SpellResisted);
                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CharmSpellHandler.YouAlreadyHaveACharmedCreature"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return false;
            }
            return base.CheckBeginCast(selectedTarget);
        }




        protected override void OnNpcReleaseCommand(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(sender is GameNPC))
                return;

            GameNPC pet = sender as GameNPC;

            if (pet.ControlledNpcList != null)
            {
                foreach (MobPetBrain cnpc in pet.ControlledNpcList)
                {
                    if (cnpc != null)
                        GameEventMgr.Notify(GameLivingEvent.PetReleased, cnpc.Body);
                }
            }
            base.OnNpcReleaseCommand(e, sender, arguments);
        }

        protected override IControlledBrain GetPetBrain(GameLiving owner)
        {
            return new MobPetBrain(owner);
        }

        protected override GamePet GetGamePet(INpcTemplate template)
        {
            return new GamePet(template);
        }
    }
}


