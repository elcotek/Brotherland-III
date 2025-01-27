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
using DOL.Database.Attributes;

namespace DOL
{
    namespace Database
    {
        /// <summary>
        /// Crafted item table
        /// </summary>
        [DataTable(TableName="CraftedItem")]
		public class DBCraftedItem : DataObject
		{
			private string m_craftedItemID;
			private int m_craftinglevel;
			private string m_id_nb;
			private int m_craftingSkillType;
			private bool m_makeTemplated;

			/// <summary>
			/// Create an crafted item
			/// </summary>
			public DBCraftedItem()
			{
				AllowAdd=false;
			}

			/// <summary>
			/// Crafting id of item to craft
			/// </summary>
			[PrimaryKey]
			public string CraftedItemID
			{
				get
				{
					return m_craftedItemID;
				}
				set
				{
					Dirty = true;
					m_craftedItemID = value;
				}
			}

			/// <summary>
			/// Index of item to craft
			/// </summary>
			[DataElement(AllowDbNull=false, Index=true)]
			public string Id_nb
			{
				get
				{
					return m_id_nb;
				}
				set
				{
					Dirty = true;
					m_id_nb = value;
				}
			}

			/// <summary>
			/// Crafting level of this item
			/// </summary>
			[DataElement(AllowDbNull=false,Unique=false)]
			public int CraftingLevel
			{
				get
				{
					return m_craftinglevel;
				}
				set
				{
					Dirty = true;
					m_craftinglevel = value;
				}
			}
			
			/// <summary>
			/// Crafting skill needed to craft this item
			/// </summary>
			[DataElement(AllowDbNull=false)]
			public int CraftingSkillType
			{
				get
				{
					return m_craftingSkillType;
				}
				set
				{
					Dirty = true;
					m_craftingSkillType = value;
				}
			}

			/// <summary>
			/// Do we create a templated item or do we create an ItemUnique?
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public bool MakeTemplated
			{
				get
				{
					return m_makeTemplated;
				}
				set
				{
					Dirty = true;
					m_makeTemplated = value;
				}
			}
		}
	}
}
