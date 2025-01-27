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

namespace DOL
{
    namespace Database
    {
        /// <summary>
        /// Account table
        /// </summary>
        [DataTable(TableName = "Account")]
        public class Account : DataObject
        {
            private string m_name;
            private string m_password;
            private string m_accountid;
            private DateTime m_creationDate;
            private DateTime m_lastLogin;
            protected int m_realm;
            private uint m_plvl;
            private int m_state;
            private String m_mail;
            private string m_lastLoginIP;
            private string m_language;
            private string m_lastClientVersion;
            private bool m_isMuted;
            public int m_allowedClientCount;
            public int m_lastdetecttimeminutes;
            public DateTime m_loginwaittime;
            public int m_lastLoginrealm;
            public bool m_isotherrealmallowed;
            public bool m_hasHouseAlb;
            public bool m_hasHouseHib;
            public bool m_hasHouseMid;
            private int m_houseNumberAlb;
            private int m_houseNumberMid;
            private int m_houseNumberHib;
            /// <summary>
            /// Create account row in DB
            /// </summary>
            public Account()
            {
                m_name = null;
                m_password = null;
                m_creationDate = DateTime.Now;
                m_plvl = 1;
                m_realm = 0;
                m_isMuted = false;
                m_allowedClientCount = 0;
                m_isotherrealmallowed = false;
                m_hasHouseAlb = false;
                m_hasHouseHib = false;
                m_hasHouseMid = false;
                m_lastdetecttimeminutes = 0;
                m_lastLoginrealm = 0;
                m_accountid = null;
                m_houseNumberAlb = 0;
                m_houseNumberMid = 0;
                m_houseNumberHib = 0;

            }

                     


            /// <summary>
            /// The name of the account (login)
            /// </summary>
            [PrimaryKey]
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
            /// The password of this account encode in MD5 or clear when start with ##
            /// </summary>
            [DataElement(AllowDbNull = true)]
            public string AccountID
            {
                get
                {
                    return m_accountid;
                }
                set
                {
                    Dirty = true;
                    m_accountid = value;
                }
            }
            /// <summary>
            /// The password of this account encode in MD5 or clear when start with ##
            /// </summary>
            [DataElement(AllowDbNull = false)]
            public string Password
            {
                get
                {
                    return m_password;
                }
                set
                {
                    Dirty = true;
                    m_password = value;
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

            /// <summary>
            /// The login wait time
            /// </summary>
            [DataElement(AllowDbNull = true)]
            public DateTime LoginWaitTime
            {
                get
                {
                    return m_loginwaittime;
                }
                set
                {
                    Dirty = true;
                    m_loginwaittime = value;
                }
            }

            /// <summary>
            /// The date of last login of this account
            /// </summary>
            [DataElement(AllowDbNull = true)]
            public int LastLoginRealm
            {
                get
                {
                    return m_lastLoginrealm;
                }
                set
                {
                    Dirty = true;
                    m_lastLoginrealm = value;
                }
            }

            /// <summary>
            /// The date of last login of this account
            /// </summary>
            [DataElement(AllowDbNull = true)]
            public bool IsOtherRealmAllowed
            {
                get
                {
                    return m_isotherrealmallowed;
                }
                set
                {
                    Dirty = true;
                    m_isotherrealmallowed = value;
                }
            }

            /// <summary>
            /// Has a house in albion ?
            /// </summary>
            [DataElement(AllowDbNull = true)]
            public bool HasHouseAlb
            {
                get
                {
                    return m_hasHouseAlb;
                }
                set
                {
                    Dirty = true;
                    m_hasHouseAlb = value;
                }
            }

            /// <summary>
            /// Has a house in Midgard ?
            /// </summary>
            [DataElement(AllowDbNull = true)]
            public bool HasHouseMid
            {
                get
                {
                    return m_hasHouseMid;
                }
                set
                {
                    Dirty = true;
                    m_hasHouseMid = value;
                }
            }

            /// <summary>
            /// Has a house in Hibernia ?
            /// </summary>
            [DataElement(AllowDbNull = true)]
            public bool HasHouseHib
            {
                get
                {
                    return m_hasHouseHib;
                }
                set
                {
                    Dirty = true;
                    m_hasHouseHib = value;
                }
            }

            /// <summary>
            /// The House in Albion
            /// </summary>
            [DataElement(AllowDbNull = true, Index = true)]
            public int HouseNumberAlb
            {
                get { return m_houseNumberAlb; }
                set
                {
                    Dirty = true;
                    m_houseNumberAlb = value;
                }
            }

            /// <summary>
            /// The House in Midgard
            /// </summary>
            [DataElement(AllowDbNull = true, Index = true)]
            public int HouseNumberMid
            {
                get { return m_houseNumberMid; }
                set
                {
                    Dirty = true;
                    m_houseNumberMid = value;
                }
            }

            /// <summary>
            /// The House in Hibernia
            /// </summary>
            [DataElement(AllowDbNull = true, Index = true)]
            public int HouseNumberHib
            {
                get { return m_houseNumberHib; }
                set
                {
                    Dirty = true;
                    m_houseNumberHib= value;
                }
            }


            /// <summary>
            /// The date of last login of this account
            /// </summary>
            [DataElement(AllowDbNull = true)]
            public DateTime LastLogin
            {
                get
                {
                    return m_lastLogin;
                }
                set
                {
                    Dirty = true;
                    m_lastLogin = value;
                }
            }

            /// <summary>
            /// The realm of this account
            /// </summary>
            [DataElement(AllowDbNull = true)]
            public int Realm
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
            /// The private level of this account (admin=3, GM=2 or player=1)
            /// </summary>
            [DataElement(AllowDbNull = true)]
            public uint PrivLevel
            {
                get
                {
                    return m_plvl;
                }
                set
                {
                    m_plvl = value;
                    Dirty = true;
                }
            }

            /// <summary>
            /// Status of this account
            /// </summary>
            [DataElement(AllowDbNull = true)]
            public int Status
            {
                get { return m_state; }
                set { Dirty = true; m_state = value; }
            }

            /// <summary>
            /// The mail of this account
            /// </summary>
            [DataElement(AllowDbNull = true)]
            public string Mail
            {
                get { return m_mail; }
                set { Dirty = true; m_mail = value; }
            }

            /// <summary>
            /// The last IP logged onto this account
            /// </summary>
            [DataElement(AllowDbNull = true, Index = true)]
            public string LastLoginIP
            {
                get { return m_lastLoginIP; }
                set { m_lastLoginIP = value; }
            }

            /// <summary>
            /// The last Client Version used
            /// </summary>
            [DataElement(AllowDbNull = true)]
            public string LastClientVersion
            {
                get { return m_lastClientVersion; }
                set { m_lastClientVersion = value; }
            }

            /// <summary>
            /// The player language
            /// </summary>
            [DataElement(AllowDbNull = true)]
            public string Language
            {
                get { return m_language; }
                set { Dirty = true; m_language = value; }
            }

            /// <summary>
            /// Is this account muted from public channels?
            /// </summary>
            [DataElement(AllowDbNull = true)]
            public bool IsMuted
            {
                get { return m_isMuted; }
                set { Dirty = true; m_isMuted = value; }
            }

            /// <summary>
            /// The realm of this account
            /// </summary>
            [DataElement(AllowDbNull = true)]
            public int AllowedClientCount
            {
                get
                {
                    return m_allowedClientCount;
                }
                set
                {
                    Dirty = true;
                    m_allowedClientCount = value;
                }
            }
            /// <summary>
            /// The realm of this account
            /// </summary>
            [DataElement(AllowDbNull = true)]
            public int LastDetectTimeMinutes
            {
                get
                {
                    return m_lastdetecttimeminutes;
                }
                set
                {
                    Dirty = true;
                    m_lastdetecttimeminutes = value;
                }
            }
            /// <summary>
            /// List of characters on this account
            /// </summary>
            [Relation(LocalField = "Name", RemoteField = "AccountName", AutoLoad = true, AutoDelete = true)]
            public DOLCharacters[] Characters;

            /// <summary>
            /// List of bans on this account
            /// </summary>
            [Relation(LocalField = "Name", RemoteField = "Account", AutoLoad = true, AutoDelete = true)]
            public DBBannedAccount[] BannedAccount;
        }
    }
}
