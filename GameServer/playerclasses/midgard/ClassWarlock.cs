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
    [CharacterClassAttribute((int)eCharacterClass.Warlock, "Warlock", "Mystic")]
	public class ClassWarlock : ClassMystic
	{
		public ClassWarlock() : base()
		{
			m_profession = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Profession.HouseofHel");
			m_specializationMultiplier = 10;
			m_primaryStat = eStat.PIE;
			m_secondaryStat = eStat.CON;
			m_tertiaryStat = eStat.DEX;
			m_manaStat = eStat.PIE;
		}

		public override string GetTitle(int level, GamePlayer player) 
		{
			if (level >= 50) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Warlock.GetTitle.50");
			if (level >= 45) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Warlock.GetTitle.45");
			if (level >= 40) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Warlock.GetTitle.40");
			if (level >= 35) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Warlock.GetTitle.35");
			if (level >= 30) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Warlock.GetTitle.30");
			if (level >= 25) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Warlock.GetTitle.25");
			if (level >= 20) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Warlock.GetTitle.20");
			if (level >= 15) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Warlock.GetTitle.15");
			if (level >= 10) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Warlock.GetTitle.10");
			if (level >= 5) return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.Warlock.GetTitle.5");
			return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.GetTitle.none");
		}

        public override void OnLevelUp(GamePlayer player, int previousLevel)
        {
            base.OnLevelUp(player, previousLevel);

			//Remove
			player.RemoveSpellLine("Darkness");
			player.RemoveSpellLine("Suppression");
			player.RemoveSpecialization(Specs.Darkness);
			player.RemoveSpecialization(Specs.Suppression);
		
			// Specializations
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Cursing));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Hexing));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Witchcraft));
						

			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine("Cursing"));
			player.AddSpellLine(SkillBase.GetSpellLine("Cursing Spec"));
			player.AddSpellLine(SkillBase.GetSpellLine("Hexing"));
			player.AddSpellLine(SkillBase.GetSpellLine("Witchcraft"));
            //player.RemoveAbility(Abilities.Tireless);
            
			if (player.Level >= 15)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
        }

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
        /*
		public override bool CanChangeCastingSpeed(SpellLine line, Spell spell)
		{
			if (spell.SpellType == "Chamber")
				return false;

			if ((line.KeyName == "Cursing"
				 || line.KeyName == "Cursing Spec"
				 || line.KeyName == "Hexing"
				 || line.KeyName == "Witchcraft")
				&& (spell.SpellType != "ArmorFactorBuff"
					&& spell.SpellType != "Bladeturn"
					&& spell.SpellType != "ArmorAbsorptionBuff"
					&& spell.SpellType != "MatterResistDebuff"
					&& spell.SpellType != "Uninterruptable"
					&& spell.SpellType != "Powerless"
					&& spell.SpellType != "Range"
                    && spell.Name != "Warping Curse Enemy"
                    && spell.Name != "Lesser Twisting Curse"
					&& spell.Name != "Twisting Curse"
					&& spell.Name != "Lesser Winding Curse"
					&& spell.Name != "Winding Curse"
					&& spell.Name != "Lesser Wrenching Curse"
					&& spell.Name != "Wrenching Curse"
					&& spell.Name != "Lesser Warping Curse"
					&& spell.Name != "Warping Curse"))
			{
				return false;
			}

			return true;
		}
        */
	}
}
