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
using System.Collections;

namespace DOL.GS.Keeps
{
    /// <summary>
    /// Represents a keep hastener
    /// </summary>
    public class FrontierHastener : GameKeepGuard
    {
        public override eFlags Flags
        {
            get { return eFlags.PEACE; }
        }

        #region Examine/Interact Message

        /// <summary>
        /// Adds messages to ArrayList which are sent when object is targeted
        /// </summary>
        /// <param name="player">GamePlayer that is examining this object</param>
        /// <returns>list with string messages</returns>
        public override IList GetExamineMessages(GamePlayer player)
        {
            IList list = new ArrayList
            {
                "You examine " + GetName(0, false) + ".  " + GetPronoun(0, true) + " is " + GetAggroLevelString(player, false) + " and is a hastener."
            };
            return list;
        }

        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;

                TurnTo(player, 5000);
              
                if (player.InCombat)
                {
                    player.Out.SendMessage("You are currently in Combat!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return false;
                }
                // Hastener give only Speed if NPC Realm = Player Realm, if Server rules PVP he give every one ! 
                if (Realm != player.Realm && GameServer.Instance.Configuration.ServerType == eGameServerType.GST_Normal)
                {
                   // player.Out.SendMessage("Are you crazy ? lol go home guy!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return false;
                }  
           
                GameNPCHelper.CastSpellOnOwnerAndPets(this, player, SkillBase.GetSpellByID(GameHastener.SPEEDOFTHEREALMID), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells), false);
            //CastSpell(SkillBase.GetSpellByID(GameHastener.SPEEDOFTHEREALMID), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Fronier.Hastener.Message", player.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
           
            return true;
            }
        #endregion Examine/Interact Message
        }
    }