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
//Eden - 1.94 RR5 Paladin

using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.RealmAbilities
{
    public class SelflessDevotionAbility : RR5RealmAbility
    {
        public SelflessDevotionAbility(DBAbility dba, int level) : base(dba, level) { }

        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

            GamePlayer player = living as GamePlayer;
            if (player.Group == null)
                ((GamePlayer)living).Out.SendMessage("You need an Group for the Ability! ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            if (player.Group != null)
            {
                SendCasterSpellEffectAndCastMessage(player, 7039, true);
                SelflessDevotionEffect effect = new SelflessDevotionEffect();
                effect.Start(player);
            }
            DisableSkill(living);
        }

        public GameLiving player;
        // GameLiving player = as PlayerGroup;    
        public override int GetReUseDelay(int level)
        {
            //GameLiving living;
            return 900;
        }

        public override void AddEffectsInfo(IList<string> list, GamePlayer player)
        {
            list.Add("Decrease Paladin stats by 25%, and pulse a 300 points group heal with a 750 units range every 3 seconds for 15 seconds total.");
            list.Add("");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Target", "Group"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + " 15 seconds");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.CastingTime", "instant"));
       }
    }
}