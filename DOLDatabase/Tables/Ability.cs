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
    /// The ability table
    /// </summary>
    [DataTable(TableName="Ability")]
	public class DBAbility : DataObject
	{
		protected string m_keyName;
        protected ushort m_ID;
		protected int	m_iconID = 0;		// 0 if no icon, ability icons start at 0x190
		protected string m_name = "unknown";
		protected string m_description = "no description";
		protected string m_implementation = null;

		/// <summary>
		/// Create ability
		/// </summary>
		public DBAbility()
		{
		}

		/// <summary>
		/// The key of this ability
		/// </summary>
		[DataElement(AllowDbNull=false,Unique=true)]
		public string KeyName
		{
			get {	return m_keyName;	}
			set	{
				Dirty = true;
				m_keyName = value;
			}
		}

		/// <summary>
		/// Name of this ability
		/// </summary>
		[DataElement(AllowDbNull=false)]
		public string Name
		{
			get {	return m_name;	}
			set	{
				Dirty = true;
				m_name = value;
			}
		}

		/// <summary>
        /// /// Ability ID (new in 1.112)
        /// </summary>
        [DataElement(AllowDbNull = true)]
        public ushort ID
        {
            get { return m_ID; }
            set
            {
                Dirty = true;
                m_ID = value;
            }
        }

        /// <summary>
		/// Small description of this ability
		/// </summary>
        [DataElement(AllowDbNull = false)]
		public string Description
		{
            get { return m_description; }
			set
			{
				Dirty = true;
				m_description = value;
			}
		}

		/// <summary>
		/// icon of ability
		/// </summary>
		[DataElement(AllowDbNull=false)]
		public int IconID
		{
			get {	return m_iconID;	}
			set
			{
				Dirty = true;
				m_iconID = value;
			}
		}

		/// <summary>
		/// Implementierungsklasse
		/// </summary>
		[DataElement(AllowDbNull=true)]
		public string Implementation
		{
			get {	return m_implementation;	}
			set
			{
				Dirty = true;
				m_implementation = value;
			}
		}
	}
}