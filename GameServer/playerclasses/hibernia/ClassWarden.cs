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
    [CharacterClassAttribute((int)eCharacterClass.Warden, "Warden", "Naturalist")]
	public class ClassWarden : ClassNaturalist
	{
		public ClassWarden() : base() 
		{
			m_profession = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Profession.PathofFocus");
            if (ServerProperties.Properties.ALLOW_OVERCAP)
            {
                m_specializationMultiplier = 18;
            }
            else
            {
                m_specializationMultiplier = 15;
            }
			m_primaryStat = eStat.EMP;
			m_secondaryStat = eStat.STR;
			m_tertiaryStat = eStat.CON;
			m_manaStat = eStat.EMP;
			m_wsbase = 360;
		}

		public override string GetTitle(int level, GamePlayer player) 
		{
			if (level >= 50) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Warden.GetTitle.50");
			if (level >= 45) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Warden.GetTitle.45");
			if (level >= 40) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Warden.GetTitle.40");
			if (level >= 35) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Warden.GetTitle.35");
			if (level >= 30) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Warden.GetTitle.30");
			if (level >= 25) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Warden.GetTitle.25");
			if (level >= 20) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Warden.GetTitle.20");
			if (level >= 15) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Warden.GetTitle.15");
			if (level >= 10) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Warden.GetTitle.10");
			if (level >= 5) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Warden.GetTitle.5");
			return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.GetTitle.none");
		}

		/// <summary>
		/// Update all skills and add new for current level
		/// </summary>
		/// <param name="player"></param>
        public override void OnLevelUp(GamePlayer player, int previousLevel)
        {
            base.OnLevelUp(player, previousLevel);

			// Specializations
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Blades));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Blunt));
            if (ServerProperties.Properties.ENABLE_WARDEN_SIELD_ABILITY)
            {
                player.AddSpecialization(SkillBase.GetSpecialization(Specs.Shields));
            }
			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine("Nurture Warden Spec"));
			player.AddSpellLine(SkillBase.GetSpellLine("Regrowth Warden Spec"));

			if (player.Level >= 5) 
			{
                player.AddAbility(SkillBase.GetAbility(Abilities.HibArmor, ArmorLevel.Cloth));
                player.AddAbility(SkillBase.GetAbility(Abilities.HibArmor, ArmorLevel.Leather));


                player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Small));
				player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Medium));

                
            }
            
			if (player.Level >= 7) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Shortbows));
			}
			if (player.Level >= 10) 
			{
               player.AddAbility(SkillBase.GetAbility(Abilities.HibArmor, ArmorLevel.Reinforced));
                
            }
			if (player.Level >= 15)
			{
                player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Large));
                player.AddAbility(SkillBase.GetAbility(Abilities.HibArmor, ArmorLevel.Scale));

                
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
			if (player.Level >= 20)
			{
                player.AddSpecialization(SkillBase.GetSpecialization(Specs.Parry));
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

		public override ushort MaxPulsingSpells
		{
			get { return 2; }
		}
	}
}