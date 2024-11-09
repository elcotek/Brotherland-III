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
using DOL.Database;

namespace DOL.GS
{
    /// <summary>
    /// MoneyLootGenerator
    /// At the moment this generaotr only adds money to the loot
    /// </summary>
    public class LootGeneratorMoney : LootGeneratorBase
	{
        
        /// <summary>
        /// Generate loot for given mob
        /// </summary>
        /// <param name="mob"></param>
        /// <param name="killer"></param>
        /// <returns></returns>
        public override LootList GenerateLoot(GameNPC mob, GameObject killer)
		{
			LootList loot = base.GenerateLoot(mob, killer);

			int lvl = mob.Level + 1;
			if (lvl < 1)
            {
                lvl = 1;
            }

            int minLoot = 2 + ((lvl * lvl * lvl) >> 3);

            if (killer.ObjectState != GameObject.eObjectState.Active)
                return loot;

            double con = GameObject.GetConLevel(killer.Level, mob.Level);
            if (con < -3.0)
                con = -3.0;


            if (con == -3.0)
            {
               
               // log.ErrorFormat("Mob ist grau also kein loot!! {0}", con);
                return loot; 
            }

            long moneyCount = minLoot + Util.Random(minLoot >> 1);
			moneyCount = (long)((double)moneyCount * ServerProperties.Properties.MONEY_DROP);

            ItemTemplate money = new ItemTemplate
            {
                Model = 488,
                Name = "bag of coins",
                Level = 0,

                Price = moneyCount
            };

            loot.AddFixed(money, 1);
			return loot;
		}
	}
}
