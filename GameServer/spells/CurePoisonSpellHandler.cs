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
using DOL.GS.PacketHandler;
using DOL.Language;
using System.Collections.Generic;




namespace DOL.GS.Spells
{
    /// <summary>
    /// Cure Poison Spellhandler
    /// </summary>
    [SpellHandlerAttribute("CurePoison")]
    public class CurePoisonSpellHandler : RemoveSpellEffectHandler
    {

        public override bool CheckBeginCast(GameLiving target)
        {
            if (!GameServer.ServerRules.IsAllowedToAttack(Caster, target, true) == false)
            {
                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.InvalidTarget"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (target == null)
            {
                if (Caster is GamePlayer)
                {
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.SelectATargetForThisSpell", Spell.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    // MessageToCaster("You must select a target for this spell!", eChatType.CT_SpellResisted);
                }
                return false;
            }
            
            return base.CheckBeginCast(target);
        }

        // constructor
        public CurePoisonSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            // RR4: now it's a list
            m_spellTypesToRemove = new List<string>();
            m_spellTypesToRemove.Add("DamageOverTime");
            //  m_spellTypesToRemove.Add("StyleBleeding");
        }
    }
}
