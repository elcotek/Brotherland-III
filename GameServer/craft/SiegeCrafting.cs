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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DOL.GS
{
	public class SiegeCrafting : AbstractProfession
	{
		public SiegeCrafting()
			: base()
		{
			Icon = 0x03;
			Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Crafting.Name.Siegecraft");
			eSkill = eCraftingSkill.SiegeCrafting;
		}
		public override string CRAFTER_TITLE_PREFIX
		{
			get
			{
				return "Siegecrafter";
			}
		}

		protected override String Profession
		{
			get
			{
				return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE,
					"CraftersProfession.Siegecrafter");
			}
		}

		/// <summary>
		/// Gain a point in the appropriate skills for a recipe and materials
		/// </summary>
		public override void GainCraftingSkillPoints(GamePlayer player, DBCraftedItem recipe, IList<DBCraftedXItem> rawMaterials)
		{
			if (Util.Chance(CalculateChanceToGainPoint(player, recipe)))
			{
				player.GainCraftingSkill(eCraftingSkill.SiegeCrafting, 1);
				player.Out.SendUpdateCraftingSkills();
			}
		}


		protected override Task BuildCraftedItem(GamePlayer player, DBCraftedItem recipe, ItemTemplate itemToCraft)
		{

			return base.BuildCraftedItem(player, recipe, itemToCraft);

		}
	}
}


			/*
			protected override void BuildCraftedItem(GamePlayer player, DBCraftedItem recipe, ItemTemplate itemToCraft)
			{
				GameSiegeWeapon siegeweapon = null;
				switch ((eObjectType)itemToCraft.Object_Type)
				{
					case eObjectType.SiegeBalista:
						{
							siegeweapon = new GameSiegeBallista();
						}
						break;
					case eObjectType.SiegeCatapult:
						{
							siegeweapon = new GameSiegeCatapult();
						}
						break;
					case eObjectType.SiegeCauldron:
						{
							siegeweapon = new GameSiegeCauldron();
						}
						break;
					case eObjectType.SiegeRam:
						{
							siegeweapon = new GameSiegeRam();
						}
						break;
					case eObjectType.SiegeTrebuchet:
						{
							siegeweapon = new GameSiegeTrebuchet();
						}
						break;
					default:
						{
							base.BuildCraftedItem(player, recipe, itemToCraft);
							return;
						}
				}
				
			//actually stores the Id_nb of the siegeweapon
			siegeweapon.ItemId = itemToCraft.Id_nb;

			siegeweapon.LoadFromDatabase(itemToCraft);
			siegeweapon.CurrentRegion = player.CurrentRegion;
			siegeweapon.Heading = player.Heading;
			siegeweapon.X = player.X;
			siegeweapon.Y = player.Y;
			siegeweapon.Z = player.Z;
			siegeweapon.Realm = player.Realm;
			siegeweapon.AddToWorld();
		}
	}
}
			*/