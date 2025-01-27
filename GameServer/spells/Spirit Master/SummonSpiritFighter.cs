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
	[SpellHandler("SummonSpiritFighter")]
	public class SummonSpiritFighter : SummonSpellHandler
	{
		public SummonSpiritFighter(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
		}

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
        protected override GamePet GetGamePet(INpcTemplate template)
        {
            return new GameSpiritmasterPets(template);
        }
	}
}