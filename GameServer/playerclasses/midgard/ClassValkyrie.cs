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

namespace DOL.GS.PlayerClass
{
    /// <summary>
    ///
    /// </summary>
    [CharacterClassAttribute((int)eCharacterClass.Valkyrie, "Valkyrie", "Viking")]
	public class ClassValkyrie : ClassViking
	{

		public ClassValkyrie()
			: base()
		{
			m_profession = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Profession.HouseofOdin");
			m_specializationMultiplier = 20;
			m_primaryStat = eStat.CON;
			m_secondaryStat = eStat.STR;
			m_tertiaryStat = eStat.DEX;
			m_manaStat = eStat.PIE;
			m_wsbase = 360;
			m_baseHP = 720;
		}

		public override string GetTitle(int level, GamePlayer player)
		{
			if (level >= 50) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Valkyrie.GetTitle.50");
			if (level >= 45) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Valkyrie.GetTitle.45");
			if (level >= 40) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Valkyrie.GetTitle.40");
			if (level >= 35) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Valkyrie.GetTitle.35");
			if (level >= 30) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Valkyrie.GetTitle.30");
			if (level >= 25) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Valkyrie.GetTitle.25");
			if (level >= 20) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Valkyrie.GetTitle.20");
			if (level >= 15) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Valkyrie.GetTitle.15");
			if (level >= 10) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Valkyrie.GetTitle.10");
			if (level >= 5) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Valkyrie.GetTitle.5");
			return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.GetTitle.none");
		}

		public override eClassType ClassType
		{
			get { return eClassType.Hybrid; }
		}

        public override void OnLevelUp(GamePlayer player, int previousLevel)
        {
            base.OnLevelUp(player, previousLevel);

            player.RemoveSpecialization(Specs.Hammer);

            if (player.ChampionLevel <= 5)
            {
                player.RemoveAbility(Abilities.Weapon_Hammers);
            }

            player.RemoveSpecialization(Specs.Axe);
            player.RemoveAbility(Abilities.Weapon_Axes);
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Shields));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Spear));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Sword));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Mending));

			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Swords));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Spears));

			player.AddSpecialization(SkillBase.GetSpecialization(Specs.OdinsWill));
			player.AddSpellLine(SkillBase.GetSpellLine("Odin's Will"));
			player.AddSpellLine(SkillBase.GetSpellLine("Mending"));
			player.AddSpellLine(SkillBase.GetSpellLine("Valkyrie Mending Spec"));
          
            if (player.Level >= 5)
            {
               
                player.AddAbility(SkillBase.GetAbility(Abilities.MidArmor, ArmorLevel.Studded));

                player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Small));
                player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Medium));
               

            }
          
			
			if (player.Level >= 10)
			{
               
                player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 1));
            }

            if (player.Level >= 15)
			{
                player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Large));
                player.AddAbility(SkillBase.GetAbility(Abilities.MidArmor, ArmorLevel.Chain));
                player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
			if (player.Level >= 20)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 2));
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
