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

namespace DOL.GS.PropertyCalc
{
    /// <summary>
    /// The health regen rate calculator
    /// 
    /// BuffBonusCategory1 is used for all buffs
    /// BuffBonusCategory2 is used for all debuffs (positive values expected here)
    /// BuffBonusCategory3 unused
    /// BuffBonusCategory4 unused
    /// BuffBonusMultCategory1 unused
    /// </summary>
    [PropertyCalculator(eProperty.EnduranceRegenerationRate)]
	public class EnduranceRegenerationRateCalculator : PropertyCalculator
	{
		public EnduranceRegenerationRateCalculator() {}

		/// <summary>
		/// calculates the final property value
		/// </summary>
		/// <param name="living"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		public override int CalcValue(GameLiving living, eProperty property)
		{
			int debuff = living.SpecBuffBonusCategory[(int)property];
			if (debuff < 0)
				debuff = -debuff;

			// buffs allow to regenerate endurance even in combat and while moving
			double regen =
				 living.BaseBuffBonusCategory[(int)property]
				+living.ItemBonus[(int)property];

			if (regen == 0 && living is GamePlayer) //&& ((GamePlayer)living).HasAbility(Abilities.Tireless))
				regen++;

			/*    Patch 1.87 - COMBAT AND REGENERATION CHANGES
    			- The bonus to regeneration while standing out of combat has been greatly increased. The amount of ticks 
				  a player receives while standing has been doubled and it will now match the bonus to regeneration while sitting.
 				  Players will no longer need to sit to regenerate faster.
			    - Fatigue now regenerates at the standing rate while moving.
			*/
			if (!living.InCombat)
			{
				if (living is GamePlayer)
				{
					if (!((GamePlayer)living).IsSprinting)
					{
						regen += 4;
					}
				}
			}
				

			regen -= debuff;

			if (regen < 0)
				regen = 0;

			if (regen != 0 && ServerProperties.Properties.ENDURANCE_REGEN_RATE != 1)
				regen *= ServerProperties.Properties.ENDURANCE_REGEN_RATE;

			double decimals = regen - (int)regen;
			if (Util.ChanceDouble(decimals))
			{
				regen += 1;	// compensate int rounding error
			}

			return (int)regen;
		}
	}
}
