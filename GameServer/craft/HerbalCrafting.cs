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
using DOL.Language;
using System.Collections.Generic;

namespace DOL.GS
{

    public class HerbalCrafting : AbstractCraftingSkill
	{
		public HerbalCrafting()
		{
			Icon = 0x0A;
			Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Crafting.Name.Herbcrafting");
			eSkill = eCraftingSkill.HerbalCrafting;
		}

		/// <summary>
		/// Gain a point in the appropriate skills for a recipe and materials
		/// </summary>
		public override void GainCraftingSkillPoints(GamePlayer player, DBCraftedItem recipe, IList<DBCraftedXItem> rawMaterials)
		{
			if (Util.Chance(CalculateChanceToGainPoint(player, recipe)))
			{
                if (player.GetCraftingSkillValue(eCraftingSkill.HerbalCrafting) < subSkillCap)
                {
                    player.GainCraftingSkill(eCraftingSkill.HerbalCrafting, 1);
                }
				player.Out.SendUpdateCraftingSkills();
			}
		}
	}
}
