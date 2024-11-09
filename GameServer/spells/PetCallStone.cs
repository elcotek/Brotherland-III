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
    [SpellHandler("FirbolgStagCallStone1")]
    public class FirbolgStagCallStone1 : SummonSpellHandler
    {
        public FirbolgStagCallStone1(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }


        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            GamePlayer player = Caster as GamePlayer;
            if (player.CurrentRegion.IsBG || player.CurrentRegion.IsRvR)
            {
                //MessageToCaster("There pets are only usable in PVE zones!", eChatType.CT_Important);
                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CharmSpellHandler.OnlyUsableInPvEZones"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                return false;
            }
            if (Caster is GamePlayer && ((GamePlayer)Caster).ControlledBrain != null)
            {
                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CharmSpellHandler.YouAlreadyHaveACharmedCreatureich schaue "), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                return false;
            }
            return base.CheckBeginCast(selectedTarget);
        }
    }
}