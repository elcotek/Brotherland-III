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
 * [StephenxPimentel
 * 1.108 - Valhallas Blessing now has a 75% chance to not use power or endurance.
 * 
 * -Code shown in SpellHandler - Line 1510
 * -And StyleProcessor - Line 577
 * 
 */

using System;
using System.Collections;
using DOL.Database;
using DOL.GS.Effects;
using System.Collections.Generic;
using DOL.Language;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Valhalla's Blessing RA
    /// </summary>
    public class ValhallasBlessingAbility : RR5RealmAbility
    {
        public const int DURATION = 30 * 1000;
        private const int SpellRadius = 1500;

        public ValhallasBlessingAbility(DBAbility dba, int level) : base(dba, level) { }

        /// <summary>
        /// Action
        /// </summary>
        /// <param name="living"></param>
        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

            GamePlayer player = living as GamePlayer;
            if (player != null)
            {
                ArrayList targets = new ArrayList();
                if (player.Group == null)
                    targets.Add(player);
                else
                {
                    foreach (GamePlayer grpplayer in player.Group.GetPlayersInTheGroup())
                    {
                        if (player.IsWithinRadius(grpplayer, SpellRadius) && grpplayer.IsAlive)
                            targets.Add(grpplayer);
                    }
                }
                foreach (GamePlayer target in targets)
                {
                    //send spelleffect
                    if (!target.IsAlive) continue;
                    ValhallasBlessingEffect ValhallasBlessing = target.EffectList.GetOfType<ValhallasBlessingEffect>();
                    if (ValhallasBlessing != null)
                        ValhallasBlessing.Cancel(false);
                    new ValhallasBlessingEffect().Start(target);
                }
            }
            DisableSkill(living);
        }

        public override int GetReUseDelay(int level)
        {
            return 600;
        }

        public override void AddEffectsInfo(IList<string> list, GamePlayer player)
        {
            list.Add("Spells/Styles used by group have has a chance of not costing power or endurance. 30s duration, 10min RUT.");
            list.Add("");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Target", "Group"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + " 30 seconds");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.CastingTime", "instant"));
            list.Add("");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.RecastTime") + " 10 min");
         }
    }
}