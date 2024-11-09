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
using System;

namespace DOL.Database
{
    /// <summary>
    /// Bans table
    /// </summary>
    [DataTable(TableName = "ml10")]
    public class ML10 : DataObject
    {
        private string m_Name;
        private bool m_Spawned;
        public DateTime m_creationDate;

        /// <summary>
        /// Create account row in DB
        /// </summary>
        public ML10()
        {
            m_Name = string.Empty;
            m_Spawned = false;
            m_creationDate = DateTime.Now;
        }


        /// <summary>
        /// Sagittarius is spawned?
        /// </summary>
        [DataElement(AllowDbNull = true)]
        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
            }
        }

        /// <summary>
        /// Scorpius is spawned?
        /// </summary>
        [DataElement(AllowDbNull = true)]
        public bool Spawned
        {
            get
            {
                return m_Spawned;
            }
            set
            {
                m_Spawned = value;
            }
        }

        /// <summary>
        /// The date of creation of this account
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public DateTime CreationDate
        {
            get
            {
                return m_creationDate;
            }
            set
            {
                m_creationDate = value;
                Dirty = true;
            }
        }
    }
}
