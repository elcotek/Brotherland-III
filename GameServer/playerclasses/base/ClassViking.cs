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
    [CharacterClassAttribute((int)eCharacterClass.Viking, "Viking", "Viking")]
	public class ClassViking : DOL.GS.CharacterClassBase
	{
		public ClassViking() : base() {
			m_specializationMultiplier = 10;
			m_wsbase = 440;
			m_baseHP = 880;
		}

		public override string GetTitle(int level, GamePlayer player)
		{
			return LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerClass.GetTitle.none");
		}

		public override eClassType ClassType
		{
			get { return eClassType.PureTank; }
		}

		public override GameTrainer.eChampionTrainerType ChampionTrainerType()
		{
			return GameTrainer.eChampionTrainerType.Viking;
		}

        public override void OnLevelUp(GamePlayer player, int previousLevel)
        {
            base.OnLevelUp(player, previousLevel);

			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Axe));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Hammer));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Sword));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Parry));

			player.AddAbility(SkillBase.GetAbility(Abilities.Sprint));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Staves));
			player.AddAbility(SkillBase.GetAbility(Abilities.MidArmor, ArmorLevel.Studded));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Axes));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Hammers));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Swords));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Staves));
			
			if (player.Level >= 2)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Small));
			}
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return false;
		}
	}
}
