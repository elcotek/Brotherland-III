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

namespace DOL.Database
{
    /// <summary>
    /// The database side of QueenSpawnCount
    /// </summary>
    [DataTable(TableName = "QueenSpawn")]
    public class DBQueenSpawn : DataObject
    {

        private string m_name;
        private int m_x;
        private int m_y;
        private int m_z;
        private ushort m_heading;
        private ushort m_region;
        private int m_npcTemplateID;
        private int m_KillCount;
        private int m_SpawnCount;
        

        /// <summary>
        /// The Constructor
        /// </summary>
        public DBQueenSpawn()
        {
            m_npcTemplateID = -1;
            m_KillCount = 0;
            m_SpawnCount = 0;
        }

        /// <summary>
        /// The Mob's ClassType
        /// </summary>
        [DataElement(AllowDbNull = true)]
        public int KillCount
        {
            get
            {
                return m_KillCount;
            }
            set
            {
                Dirty = true;
                m_KillCount = value;
            }
        }
        /// <summary>
        /// The Mob's ClassType
        /// </summary>
        [DataElement(AllowDbNull = true)]
        public int SpawnCount
        {
            get
            {
                return m_SpawnCount;
            }
            set
            {
                Dirty = true;
                m_SpawnCount = value;
            }
        }
        /// <summary>
        /// The Mob's Name
        /// </summary>
        [DataElement(AllowDbNull = false, Index = true)]
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                Dirty = true;
                m_name = value;
            }
        }


        /// <summary>
        /// The Mob's X Position
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public int X
        {
            get
            {
                return m_x;
            }
            set
            {
                Dirty = true;
                m_x = value;
            }
        }

        /// <summary>
        /// The Mob's Y Position
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public int Y
        {
            get
            {
                return m_y;
            }
            set
            {
                Dirty = true;
                m_y = value;
            }
        }

        /// <summary>
        /// The Mob's Z Position
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public int Z
        {
            get
            {
                return m_z;
            }
            set
            {
                Dirty = true;
                m_z = value;
            }
        }


        /// <summary>
        /// The Mob's Heading
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public ushort Heading
        {
            get
            {
                return m_heading;
            }
            set
            {
                Dirty = true;
                m_heading = value;
            }
        }

        /// <summary>
        /// The Killer's actual Region ID
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public ushort Region
        {
            get
            {
                return m_region;
            }
            set
            {
                Dirty = true;
                m_region = value;
            }
        }




        [DataElement(AllowDbNull = true)]
        public int NPCTemplateID
        {
            get
            {
                return m_npcTemplateID;
            }
            set
            {
                Dirty = true;
                m_npcTemplateID = value;
            }
        }
    }
}


