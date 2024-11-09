
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
using DOL.AI.Brain;
using DOL.GS.PacketHandler;




namespace DOL.GS.Spells
{

    [SpellHandlerAttribute("PowerRend")]
    public class PowerRendSpellHandler : SpellHandler
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public PowerRendSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }



        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (target == null) return;
            if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;

            int mana = 0;
            int MaxMana = 0;
            if (Spell.Damage < 0)//percent
            {
                MaxMana = target.GetModified(eProperty.MaxMana);
                mana = (int)Spell.Damage * -1;
                mana = MaxMana * mana / 100;
            }
            else
                mana = (int)(Spell.Damage);//dezimal


            target.ChangeMana(target, GameLiving.eManaChangeType.Spell, (-mana));
            //~yemla~Power rend shouldn't inrupt if im correct? A.k ML9 Perfector Power Drainging Ward Please more info on it.
            //target.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
        }

        public virtual void SendCasterMessage(GameLiving target, int mana)
        {
            MessageToCaster(string.Format("You steal {0} for {1} power!", target.Name, mana.ToString()), eChatType.CT_YouHit);
            if (mana > 0)
            {
                MessageToCaster("You steal " + mana.ToString() + " power points" + (mana == 1 ? "." : "s."), eChatType.CT_Spell);
            }
            else
            {
                MessageToCaster("You cannot absorb any more power.", eChatType.CT_SpellResisted);
            }
        }
    }
}