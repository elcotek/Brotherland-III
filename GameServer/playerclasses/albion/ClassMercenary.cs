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
using DOL.Language;
using System.Collections.Generic;

namespace DOL.GS.PlayerClass
{
    /// <summary>
    ///
    /// </summary>
    [CharacterClassAttribute((int)eCharacterClass.Mercenary, "Mercenary", "Fighter")]
    public class ClassMercenary : ClassFighter
    {
        private static readonly string[] AutotrainableSkills = new[] { Specs.Slash, Specs.Thrust };

        public ClassMercenary() : base()
        {
            m_profession = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Profession.GuildofShadows");
            m_specializationMultiplier = 20;
            m_primaryStat = eStat.STR;
            m_secondaryStat = eStat.DEX;
            m_tertiaryStat = eStat.CON;
            m_manaStat = eStat.STR;
            m_baseHP = 880;
        }

        public override string GetTitle(int level, GamePlayer player)
        {
            if (level >= 50) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Mercenary.GetTitle.50");
            if (level >= 45) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Mercenary.GetTitle.45");
            if (level >= 40) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Mercenary.GetTitle.40");
            if (level >= 35) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Mercenary.GetTitle.35");
            if (level >= 30) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Mercenary.GetTitle.30");
            if (level >= 25) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Mercenary.GetTitle.25");
            if (level >= 20) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Mercenary.GetTitle.20");
            if (level >= 15) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Mercenary.GetTitle.15");
            if (level >= 10) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Mercenary.GetTitle.10");
            if (level >= 5) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Mercenary.GetTitle.5");
            return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.GetTitle.none");
        }

        public override bool CanUseLefthandedWeapon(GamePlayer player)
        {
            return true;
        }

        public override IList<string> GetAutotrainableSkills()
        {
            return AutotrainableSkills;
        }

        /// <summary>
        /// Update all skills and add new for current level
        /// </summary>
        /// <param name="player"></param>
        public override void OnLevelUp(GamePlayer player, int previousLevel)
        {
            base.OnLevelUp(player, previousLevel);

            player.AddSpecialization(SkillBase.GetSpecialization(Specs.Shields));
            player.AddSpecialization(SkillBase.GetSpecialization(Specs.Dual_Wield));

            if (player.Level >= 5)
            {
                player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Small));
                player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Medium));

            }
            if (player.Level >= 10)
            {
                player.AddAbility(SkillBase.GetAbility(Abilities.AlbArmor, ArmorLevel.Chain));
                player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Shortbows));
                player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 1));

            }
            if (player.Level >= 15)
            {
                player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 1));
                player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
            }
            if (player.Level >= 17)
            {
                player.AddSpecialization(SkillBase.GetSpecialization(Specs.Parry));
            }
            if (player.Level >= 19)
            {
                player.AddAbility(SkillBase.GetAbility(Abilities.Intercept));
            }
            if (player.Level >= 20)
            {
                player.AddAbility(SkillBase.GetAbility(Abilities.DirtyTricks));
            }
            if (player.Level >= 23)
            {
                player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 2));
            }
            if (player.Level >= 24)
            {
                player.AddAbility(SkillBase.GetAbility(Abilities.PreventFlight));
            }
            if (player.Level >= 30)
            {
                player.AddAbility(SkillBase.GetAbility(Abilities.Flurry));
            }
            if (player.Level >= 32)
            {
                player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 3));
            }
            if (player.Level >= 35)
            {
                player.AddAbility(SkillBase.GetAbility(Abilities.Advanced_Evade));
                player.AddAbility(SkillBase.GetAbility(Abilities.Stoicism));
            }
        }

        /// <summary>
        /// Add all spell-lines and other things that are new when this skill is trained
        /// </summary>
        /// <param name="player">player to modify</param>
        /// <param name="skill">The skill to train</param>
        public override void OnSkillTrained(GamePlayer player, Specialization skill)
        {
            base.OnSkillTrained(player, skill);

            switch (skill.KeyName)
            {
                case Specs.Shields:
                    if (skill.Level >= 45) player.AddAbility(SkillBase.GetAbility(Abilities.Tactics));
                    if (skill.Level >= 7) player.AddAbility(SkillBase.GetAbility(Abilities.Engage));
                    if (skill.Level >= 5) player.AddAbility(SkillBase.GetAbility(Abilities.Guard, 1));
                    if (skill.Level >= 10) player.AddAbility(SkillBase.GetAbility(Abilities.Guard, 2));
                    if (skill.Level >= 15) player.AddAbility(SkillBase.GetAbility(Abilities.Guard, 3));
                    break;
            }
        }

        public override bool HasAdvancedFromBaseClass()
        {
            return true;
        }
    }
}
