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
    /// Objects as Tables, Lights, Bags non static in Game.
    /// </summary>
    [DataTable(TableName="WorldObject")]
	public class WorldObject : DataObject
	{
		private string		m_type;
        private string      m_translationId;
        private string		m_name;
		private int			m_x;
		private int			m_y;
		private int			m_z;
		private ushort		m_heading;
		private ushort		m_model;
		private ushort		m_region;
		private int			m_emblem;
		private byte 		m_realm;
		private int			m_respawnInterval;
		private int         m_health;
		private int         m_maxhealth;
		private byte        m_level;


		public WorldObject()
		{
			m_type = "DOL.GS.GameItem";
		}

		[DataElement(AllowDbNull = true)]
		public string ClassType
		{
			get
			{
				return m_type;
			}
			set
			{
				Dirty = true;
				m_type = value;
			}
		}

        [DataElement(AllowDbNull = true)]
        public string TranslationId
        {
            get { return m_translationId; }
            set
            {
                Dirty = true;
                m_translationId = value;
            }
        }

        [DataElement(AllowDbNull=false)]
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
		
		[DataElement(AllowDbNull=false)]
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
		
		[DataElement(AllowDbNull=false)]
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

		[DataElement(AllowDbNull=false)]
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
		
		[DataElement(AllowDbNull=false)]
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

		[DataElement(AllowDbNull=false, Index=true)]
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
		
		[DataElement(AllowDbNull=false)]
		public ushort Model
		{
			get
			{
				return m_model;
			}
			set
			{
				Dirty = true;
				m_model = value;
			}
		}
		
		[DataElement(AllowDbNull=false)]
		public int Emblem
		{
			get
			{
				return m_emblem;
			}
			set
			{
				Dirty = true;
				m_emblem = value;
			}
		}
		
		[DataElement(AllowDbNull = false)]
		public byte Realm
		{
			get
			{
				return m_realm;
			}
			set
			{
				Dirty = true;
				m_realm = value;
			}
		}

		/// <summary>
		/// Respawn interval, in seconds
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int RespawnInterval
		{
			get
			{
				return m_respawnInterval;
			}
			set
			{
				Dirty = true;
				m_respawnInterval = value;
			}
		}
		/// <summary>
		/// Health
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int Health
		{
			get
			{
				return m_health;
			}
			set
			{
				Dirty = true;
				m_health = value;
			}
		}
		/// <summary>
		/// Health
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int MaxHealth
		{
			get
			{
				return m_maxhealth;
			}
			set
			{
				Dirty = true;
				m_maxhealth = value;
			}
		}

		/// <summary>
		/// Health
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public byte Level
		{
			get
			{
				return m_level;
			}
			set
			{
				Dirty = true;
				m_level = value;
			}
		}
	}
}
