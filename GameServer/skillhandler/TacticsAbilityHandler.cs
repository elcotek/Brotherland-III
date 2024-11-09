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
using System.Collections;

namespace DOL.GS.SkillHandler
{
    /// <summary>
    /// Handler for Tactics Ability clicks
    /// </summary>
    [SkillHandlerAttribute(Abilities.Tactics)]
    public class TacticsAbilityHandler : IAbilityActionHandler
    {
        protected const int REUSE_TIMER = 600 * 1000; // 10 minutes

        public const int EvadeBuff = 130066;
        public const int ParryBuff = 130067;
        public const int BlockBuff = 130068;
        protected readonly Hashtable m_specialization = new Hashtable();

        /// <summary>
        /// returns the level of a specialization
        /// if 0 is returned, the spec is non existent on player
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public virtual int GetBaseSpecLevel(string keyName)
        {
            Specialization spec = m_specialization[keyName] as Specialization;
            return spec == null ? 0 : spec.Level;
        }


        public void Execute(Ability ab, GamePlayer player)
        {
            //GamePlayer player = living as GamePlayer; 
            if (player != null)
            {
                int specLevel = ((GamePlayer)player).GetBaseSpecLevel(Specs.Shields);
                {
                    if (player.ActiveWeaponSlot != GameLiving.eActiveWeaponSlot.Standard)
                    {
                        player.Out.SendMessage("you must have a active Shield in your hand for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
                    else if (specLevel < 45)
                    {
                        player.Out.SendMessage("You need a base shield skill of 45 for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
                    if (player.Group == null)
                    {
                        player.Out.SendMessage("You must be in a group to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }



                    else if (specLevel > 44)
                    {

                        
                        ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(EvadeBuff), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                        ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(ParryBuff), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                        ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(BlockBuff), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));

                        player.DisableSkill(ab, REUSE_TIMER);

                    }
                }
            }
        }
          

        public virtual int GetReUseDelay(int level)
        {
            return 300;
        }
    }
}
     
   
        
        
        
        
       
 










