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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using DOL.Database;
using DOL.GS;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using log4net;


namespace DOL.GS
{
    /// <summary>
    /// Class representing a relic pillar.
    /// </summary>
    /// <author>Aredhel</author>
    public class RelicPillar : GameObject, IDoor
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool IsVisibleTo(GamePlayer player)
        {
            // Beispiel-Logik, um die Sichtbarkeit zu bestimmen
            return player != null; // Beispielbedingung
        }

        protected int m_doorID;
        /// <summary>
        /// The door index which is unique
        /// </summary>
        public int DoorID
        {
            get { return m_doorID; }
            set { m_doorID = value; }
        }

        private int m_locked;
        /// <summary>
        /// door open = 0 / lock = 1 
        /// </summary>
        public virtual int Locked
        {
            get { return m_locked; }
            set { m_locked = value; }
        }


        /// <summary>
        /// Get the ZoneID of this door
        /// </summary>
        public ushort ZoneID
        {
            get { return (ushort)(DoorID / 1000000); }
        }

        public int OwnerKeepID
        {
            get { return (m_doorID / 100000) % 1000; }
        }




        public int ComponentID
        {
            get { return (m_doorID / 100) % 100; }
        }

        public int DoorIndex
        {
            get { return m_doorID % 10; }
        }


        /// <summary>
        /// This flag is send in packet(keep door = 4, regular door = 0)
        /// </summary>
        public uint Flag
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
		/// door state (open or closed)
		/// </summary>
		protected eDoorState m_state;

        /// <summary>
        /// door state (open or closed)
        /// call the broadcast of state in area
        /// </summary>
        public eDoorState State
        {
            get { return m_state; }
            set
            {
                if (m_state != value)
                {
                    m_state = value;
                    BroadcastDoorStatus();
                }
            }
        }

        public virtual void CloseDoor()
        {
            m_state = eDoorState.Closed;

            BroadcastDoorStatus();
        }

        /// <summary>
        /// boradcast the door status to all player near the door
        /// </summary>
        public virtual void BroadcastDoorStatus()
        {
            foreach (GameClient client in WorldMgr.GetClientsOfRegion(CurrentRegionID))
            {
                client.Player.SendDoorUpdate(this);
            }
        }

        public void Open()
        {

            //do nothing because gamekeep must be destroyed to be open
        }



        /// <summary>
        /// call when player try to close door
        /// </summary>
        public void Close()
        {
            //do nothing because gamekeep must be destroyed to be open
        }



        public void NPCManipulateDoorRequest(GameNPC npc, bool open)
        { }
    }
}

