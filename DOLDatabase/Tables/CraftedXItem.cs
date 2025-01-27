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
        /// raw materials for craft item
        /// </summary>
        [DataTable(TableName="CraftedXItem")]
		public class DBCraftedXItem : DataObject
		{
			private string m_ingredientId_nb;
			private int m_count;
			private string m_craftedItemId_nb;
            private int m_XPrice;
            private int m_EndPrice;
            /// <summary>
            /// create a raw material
            /// </summary>
            public DBCraftedXItem()
			{
				AllowAdd=false;
			}

           
            /// <summary>
            /// the index
            /// </summary>
            [DataElement(AllowDbNull=false, Index=true)]
			public string CraftedItemId_nb
			{
				get
				{
					return m_craftedItemId_nb;
				}
				set
				{
					Dirty = true;
					m_craftedItemId_nb = value;
				}
			}

			/// <summary>
			/// the raw material used to craft
			/// </summary>
			[DataElement(AllowDbNull=false)]
			public string IngredientId_nb
			{
				get
				{
					return m_ingredientId_nb;
				}
				set
				{
					Dirty = true;
					m_ingredientId_nb = value;
				}
			}

			/// <summary>
			/// The count of the raw material to use
			/// </summary>
			[DataElement(AllowDbNull=false)]
			public int Count
			{
				get
				{
					return m_count;
				}
				set
				{
					Dirty = true;
					m_count = value;
				}
			}

            /// <summary>
            /// the index
            /// </summary>
            [DataElement(AllowDbNull = true)]
            public int XPrice
            {
                get
                {
                    return m_XPrice;
                }
                set
                {
                    Dirty = true;
                    m_XPrice = value;
                }
            }

            /// <summary>
			/// the index
			/// </summary>
			[DataElement(AllowDbNull = true)]
            public int EndPrice
            {
                get
                {
                    return m_EndPrice;
                }
                set
                {
                    Dirty = true;
                    m_EndPrice = value;
                }
            }
        }
	}
}
