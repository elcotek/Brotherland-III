﻿/*
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

namespace DOL.Database
{
    /// <summary>
    /// Holds starter equipment
    /// </summary>
    [DataTable(TableName = "StarterEquipment")]
    public class DBStarterEquipment : DataObject
    {
        private int m_id;
        private string m_class;
        private string m_templateID;
        /// <summary>
		/// Create a relic row
		/// </summary>
		public DBStarterEquipment() { }
        /// <summary>
        /// Serialized classes this item should be given to (separator ':')
        /// 0 for all classes
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public string Class
        {
            get
            {
                return m_class;
            }
            set
            {
                Dirty = true;
                m_class = value;
            }
        }

        /// <summary>
        /// The template ID of free item
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public string TemplateID
        {
            get
            {
                return m_templateID;
            }
            set
            {
                m_templateID = value;
                Dirty = true;
            }
        }
        /// <summary>
        /// Index of relic
        /// </summary>


        /// <summary>
        /// The template ID of free item
        /// </summary>
        [PrimaryKey(AutoIncrement = true)]
        public int ID
        {
            get { return m_id; }
            set
            {
                Dirty = true;
                m_id = value;
            }
        }

        /// <summary>
        /// The ItemTemplate for this record
        /// </summary>
        [Relation(LocalField = "TemplateID", RemoteField = "Id_nb", AutoLoad = true, AutoDelete = false)]
        public ItemTemplate Template;
    }
}
