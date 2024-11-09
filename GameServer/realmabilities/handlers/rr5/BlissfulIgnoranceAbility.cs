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
using System.Collections;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.Spells;
using System.Collections.Generic;
using DOL.GS.SkillHandler;
using DOL.Language;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Minion Rescue RA
    /// </summary>
    public class BlissfulIgnoranceAbility : RR5RealmAbility
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const int DURATION = 40 * 1000;
        public const int ignorePunctures = 35006;
        public const int ignoreLacerations = 35005;
        public const int ignoreBlows = 35001;
        public const int evasionOfKelgor = 35003;
        public const int fangsOfKelgor = 35004;
        public const int savageBlows = 35002;
        public const int zealOfKelgor = 35000;
        protected readonly Hashtable m_specialization = new Hashtable();

        public BlissfulIgnoranceAbility(DBAbility dba, int level) : base(dba, level) { }

        /// <summary>
        /// returns the level of a specialization
        /// if 0 is returned, the spec is non existent on player
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public virtual int GetBaseSpecLevel(string keyName)
        {
            Specialization spec = m_specialization[keyName] as Specialization;
            if (spec == null)
                return 0;
            return spec.Level;
        }
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
                int specLevel = ((GamePlayer)player).GetBaseSpecLevel(Specs.Savagery);

                if (specLevel < 45)
                {
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(ignoreBlows), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                }
                else if (specLevel == 45)
                {
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(ignoreBlows), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(evasionOfKelgor), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                }
                else if (specLevel > 45 && specLevel < 47)
                {
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(ignoreBlows), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(evasionOfKelgor), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(fangsOfKelgor), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                }
                else if (specLevel == 47)
                {
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(ignoreBlows), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(evasionOfKelgor), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(fangsOfKelgor), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(ignorePunctures), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                }
                else if (specLevel > 47 && specLevel < 49)
                {
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(ignoreBlows), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(evasionOfKelgor), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(fangsOfKelgor), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(ignorePunctures), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(ignoreLacerations), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(zealOfKelgor), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                }
                else if (specLevel > 48)
                {
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(ignoreBlows), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(evasionOfKelgor), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(fangsOfKelgor), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(ignorePunctures), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(ignoreLacerations), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(zealOfKelgor), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    ((GamePlayer)player).CastSpell(SkillBase.GetSpellByID(savageBlows), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));

                }
                DisableSkill(living);
            }
        }

        public override int GetReUseDelay(int level)
        {
            return 300;
        }

        public override void AddEffectsInfo(IList<string> list, GamePlayer player)
        {
            list.Add("No penality Hit from self buffs. 30s duration, 5min RUT.");
            list.Add("You need to skill more as 45 in Savagery for more Buffs!");
            list.Add("");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Target", "Self"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + " 30 seconds");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.CastingTime", "instant"));
        }
    }
}
