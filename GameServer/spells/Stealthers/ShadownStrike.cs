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
 * Author : Xenturio
 */
using System;
using System.Collections.Generic;
using DOL.GS.Styles;
using DOL.GS.Spells;
using DOL.GS.PlayerClass;
using DOL.GS.Effects;
using DOL.GS.SkillHandler;
using System.Text;
using System.Collections;
using System.Reflection;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Spells
{
    [SpellHandler("ShadowStrike")]
    public class ShadowStrikeSpellHandler : SpellHandler
    {
        private const string TEMP_KEY_JUMP = "ShadowStrikeJumpLocStack";

        /// <summary>
        /// Does this spell break stealth on start?
        /// </summary>
        public override bool UnstealthCasterOnStart
        {
            get { return false; }
        }

        public override void FinishSpellCast(GameLiving player) {

            GamePlayer caster = (GamePlayer)m_caster;
            int xrange = 0;
            int yrange = 0;
            double angle = 0.00153248422;
            Style style = null;
            GameLiving ta = player as GameNPC;
            GameLiving TargetObject = player as GamePlayer;

            if (TargetObject.IsMoving)
            {
                ((GamePlayer)caster).Out.SendMessage("Your target is moving, the strike fail!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return;
            }


            //teleport vorbereiten für rückport //Rückport ist in GameLiving: MoveBackToLastDestinationLocation
            Stack<GameLocation> locations;
            if (caster != null)
            {
                //caster.TempProperties.setProperty(GameLiving.ShadowStrikeTime, caster.CurrentRegion.Time);
                locations = caster.TempProperties.getProperty<object>(TEMP_KEY_JUMP, null) as Stack<GameLocation>;

                if (locations == null)
                {
                    locations = new Stack<GameLocation>(3);
                    caster.TempProperties.setProperty(TEMP_KEY_JUMP, locations);
                }
                locations.Push(new GameLocation("tempstrikeloc", caster.CurrentRegionID, caster.X, caster.Y, caster.Z, caster.Heading));
            }

            if (m_caster.GetModifiedSpecLevel(Specs.Critical_Strike) >= 21)
            {

                m_caster.MoveTo(caster.CurrentRegionID, (int)(m_spellTarget.X - ((xrange + 10) * Math.Sin(angle * m_spellTarget.Heading))), (int)(m_spellTarget.Y + ((yrange + 10) * Math.Cos(angle * m_spellTarget.Heading))), m_spellTarget.Z, m_caster.Heading);
                style = SkillBase.GetStyleByID(612, caster.CharacterClass.ID);
            }
            else if (m_caster.GetModifiedSpecLevel(Specs.Critical_Strike) >= 10)
            {

                m_caster.MoveTo(caster.CurrentRegionID, (int)(m_spellTarget.X - ((xrange - 10) * Math.Sin(angle * m_spellTarget.Heading))), (int)(m_spellTarget.Y + ((yrange - 10) * Math.Cos(angle * m_spellTarget.Heading))), m_spellTarget.Z, m_caster.Heading);
                style = SkillBase.GetStyleByID(613, caster.CharacterClass.ID);
            }
            else if (m_caster.GetModifiedSpecLevel(Specs.Critical_Strike) >= 2)
            {

                m_caster.MoveTo(caster.CurrentRegionID, (int)(m_spellTarget.X - ((xrange - 10) * Math.Sin(angle * m_spellTarget.Heading))), (int)(m_spellTarget.Y + ((yrange - 10) * Math.Cos(angle * m_spellTarget.Heading))), m_spellTarget.Z, m_caster.Heading);
                style = SkillBase.GetStyleByID(614, caster.CharacterClass.ID);
            }
            if (style != null)
            {
                StyleProcessor.TryToUseStyle(caster, style);
            }
            caster.TempProperties.setProperty(GameLiving.ShadowStrikeTime, caster.CurrentRegion.Time);
            base.FinishSpellCast(player);
           
        }
        public ShadowStrikeSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
 
}
