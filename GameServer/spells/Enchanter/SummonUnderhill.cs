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

namespace DOL.GS.Spells
{
    /// <summary>
    /// Spell handler to summon a bonedancer pet.
    /// </summary>
    /// <author>IST</author>
    [SpellHandler("SummonUnderhill")]
    public class SummonUnderhill : SummonSpellHandler
    {
        public SummonUnderhill(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (Caster is GamePlayer && ((GamePlayer)Caster).ControlledBrain != null)
            {
                //MessageToCaster("You already have a charmed creature, release it first!", eChatType.CT_SpellResisted);
                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CharmSpellHandler.YouAlreadyHaveACharmedCreature"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return false;
            }
            return base.CheckBeginCast(selectedTarget);
        }
    }
}
