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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;
using System.Collections.Generic;

namespace DOL.GS.Spells
{
    /// <summary>
    /// The spell used for the Personal Bind Recall Stone.
    /// </summary>
    /// <author>Aredhel</author>
    [SpellHandlerAttribute("Epic6AlbPortalStone")]
    public class Epic6AlbPortalStone : SpellHandler
    {
        public Epic6AlbPortalStone(GameLiving caster, Spell spell, SpellLine spellLine)
            : base(caster, spell, spellLine) { }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Can this spell be queued with other spells?
        /// </summary>
        public override bool CanQueue
        {
            get { return false; }
        }


        /// <summary>
        /// Whether this spell can be cast on the selected target at all.
        /// </summary>
        /// <param name="selectedTarget"></param>
        /// <returns></returns>
        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            GamePlayer player = Caster as GamePlayer;
            if (player == null)
                return false;

            if (player.CurrentRegionID != 10 && player.CurrentRegionID != 192)
            {
                // Actual live message is: You can't use that item!
                player.Out.SendMessage("This Item works only in Camelot and in the Dungeon!", DOL.GS.PacketHandler.eChatType.CT_System, DOL.GS.PacketHandler.eChatLoc.CL_SystemWindow);
                return false;
            }

            if (player.CurrentRegion.IsRvR)
            {
                // Actual live message is: You can't use that item!
                //player.Out.SendMessage("You can't use that here!", DOL.GS.PacketHandler.eChatType.CT_System, DOL.GS.PacketHandler.eChatLoc.CL_SystemWindow);
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "YouCantUseThatHere"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (player.IsMoving)
            {
                player.Out.SendMessage("You must be standing still to use this item!", DOL.GS.PacketHandler.eChatType.CT_System, DOL.GS.PacketHandler.eChatLoc.CL_SystemWindow);
                return false;
            }

            if (player.InCombat || GameRelic.IsPlayerCarryingRelic(player))
            {
                player.Out.SendMessage("You have been in combat recently and cannot use this item!", DOL.GS.PacketHandler.eChatType.CT_System, DOL.GS.PacketHandler.eChatLoc.CL_SystemWindow);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Always a constant casting time
        /// </summary>
        /// <returns></returns>
        public override int CalculateCastingTime()
        {
            return m_spell.CastTime;
        }


        /// <summary>
        /// Apply the effect.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectiveness"></param>
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GamePlayer player = Caster as GamePlayer;
            if (player == null)
                return;
           
            if (player.CurrentRegionID != 10 && player.CurrentRegionID != 192)
            {
                // Actual live message is: You can't use that item!
                player.Out.SendMessage("This Item works only in Camelot and in the Epic Dungeon!", DOL.GS.PacketHandler.eChatType.CT_System, DOL.GS.PacketHandler.eChatLoc.CL_SystemWindow);
                return;
            }

            if (player.InCombat || GameRelic.IsPlayerCarryingRelic(player) || player.IsMoving)
                return;

            SendEffectAnimation(player, 0, false, 1);

            UniPortalEffect effect = new UniPortalEffect(this, 1000);
            effect.Start(player);

            //DOLCharacters character = player.DBCharacter;

            switch (player.CurrentRegionID)
            {
                case 10:
                    {
                        if (player.Group != null)
                        {
                            foreach (GamePlayer plr in player.Group.GetPlayersInTheGroup())
                            {
                                if (plr.IsWithinRadius(player, 1000) == false)
                                {
                                    continue;
                                }

                                plr.Out.SendMessage("You use The Portal Magic and he teleport you to Ansgar's Cave.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                PortalSpell.SetDestinationAlb(192, 35084, 32426, 16171, 2171, eRealm.Albion);
                                GameNPCHelper.CastSpellOnOwnerAndPets(plr, plr, SkillBase.GetSpellByID(5612), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells), false);
                            }
                        }
                        else
                        {
                            
                                player.Out.SendMessage("You use The Portal Magic and he teleport you to Ansgar's Cave.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                PortalSpell.SetDestinationAlb(192, 35084, 32426, 16171, 2171, eRealm.Albion);
                                GameNPCHelper.CastSpellOnOwnerAndPets(player, player, SkillBase.GetSpellByID(5612), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells), false);
                          
                        }
                    }
                    break;

                case 192:
                    {
                        if (player.Group != null)
                        {
                            foreach (GamePlayer plr in player.Group.GetPlayersInTheGroup())
                            {
                                if (plr.IsWithinRadius(player, 1000) == false)
                                {
                                    continue;
                                }

                                plr.Out.SendMessage("You use The Portal Magic and he teleport you back to Camelot.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                PortalSpell.SetDestinationAlb(10, 38694, 27389, 8318, 3099, eRealm.Albion);
                                GameNPCHelper.CastSpellOnOwnerAndPets(plr, plr, SkillBase.GetSpellByID(5612), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells), false);
                            }
                        }
                        else
                        {
                            player.Out.SendMessage("You use The Portal Magic and he teleport you back to Camelot.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            PortalSpell.SetDestinationAlb(10, 38694, 27389, 8318, 3099, eRealm.Albion);
                            GameNPCHelper.CastSpellOnOwnerAndPets(player, player, SkillBase.GetSpellByID(5612), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells), false);
                        }
                    }
                    break;

                default: break;
            }
        }


        public override void CasterMoves()
        {
            InterruptCasting();
            MessageToCaster("You move and interrupt your spellcast!", DOL.GS.PacketHandler.eChatType.CT_Important);
        }


        public override void InterruptCasting()
        {
            m_startReuseTimer = false;
            base.InterruptCasting();
        }

        public override IList<string> DelveInfo(GamePlayer player)
        {

            {
                var list = new List<string>();
                list.Add(string.Format("  {0}", Spell.Description));

                return list;
            }
        }
    }
}
