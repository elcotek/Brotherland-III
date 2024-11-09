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
using DOL.GS.Keeps;
using DOL.GS.Relics;
using log4net;
using System.Collections.Generic;
using System.Reflection;

namespace DOL.GS
{
    /// <summary>
    /// DoorMgr is manager of all door regular door and keep door
    /// </summary>
    public sealed class DoorMgr
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly Dictionary<int, List<IDoor>> m_doors = new Dictionary<int, List<IDoor>>();

        public const string WANT_TO_ADD_DOORS = "WantToAddDoors";


        private static bool IsRelicPillar(DBDoor door)
        {
            switch (door.InternalID)
            {

                //Myrddin Albion power
                case 175135301: //Albion Pillar
                case 175135302: //Midgard Pillar 
                case 175135303: //Hibernia Pillar

                //Excalibur Albion Strength
                case 176130401: //Albion Pillar
                case 176130402://Midgard Pillar
                case 176130403://Hibernia Pillar

                //Grallahorn Midard Power 
                case 169262401: //Midgard Pillar
                case 169262402: //Hibernia Pillar
                case 169262403: //Albion Pillar

                //Mjollner Midgard Strength
                case 170347601: //Midgard Pillar
                case 170347602: //Hibernia Pillar
                case 170347603: //Albion Pillar

                //Dun Lamfhota Hibernia Strength
                case 173381701: //Lamfhota Hibernia Pillar
                case 173381702: //Lamfhota Albion Pillar
                case 173381703: //Lamfhota Midgard Pillar

                //Dun Lamfhota Hibernia Strength
                case 174226401: //Dun Dagta Hibernia Pillar
                case 174226402: //Dun Dagta Albion Pillar
                case 174226403: //Dun Dagta Midgard Pillar

                    {
                        return true;
                    }

                default: return false;

            }
        }

        public static bool IsRelicGate(DBDoor door)
        {
            switch (door.InternalID)
            {  //Relic Gates
                case 173000301:
                case 173000201:
                case 174061701:
                case 174149601:
                case 170128101:
                case 170127801:
                case 169000501:
                case 169000301:
                case 176233801:

                case 176233901:
                case 175014001:
                case 175013901:
                    {
                        return true;
                    }
            }
            return false;
        }



        /// <summary>
        /// this function load all door from DB
        /// </summary>
        public static bool Init()
        {
            var dbdoors = GameServer.Database.SelectAllObjects<DBDoor>();
            foreach (DBDoor door in dbdoors)
            {



                if (!LoadDoor(door))
                {
                    log.Error("Unable to load door id " + door.ObjectId + ", correct your database");
                    return false;
                }
            }

            return true;
        }

        public static bool LoadDoor(DBDoor door)
        {
            IDoor mydoor = null;
            ushort zone = (ushort)(door.InternalID / 1000000);

            Zone currentZone = WorldMgr.GetZone(zone);
            if (currentZone == null) return false;

            //check if the door is a keep door
            foreach (AbstractArea area in currentZone.GetAreasOfSpot(door.X, door.Y, door.Z))
            {
                if (IsRelicPillar(door) == false && IsRelicGate(door) == false)//RelicPillar
                {
                    if (area is GameKeepArea)
                    {
                        mydoor = new GameKeepDoor();
                        mydoor.LoadFromDatabase(door);
                        break;
                    }
                }

            }




            if (IsRelicPillar(door))
            {
                foreach (AbstractArea area in currentZone.GetAreasOfSpot(door.X, door.Y, door.Z))
                {
                    if (area is GameKeepArea)
                    {
                        mydoor = new GameRelicPad();
                        mydoor.LoadFromDatabase(door);
                    }
                }

            }



            //if the door is not a keep door, create a standard door
            if (mydoor == null)
            {
                mydoor = new GameDoor();
                mydoor.LoadFromDatabase(door);
            }

            //add to the list of doors
            if (mydoor != null)
            {
                RegisterDoor(mydoor);
            }

            return true;
        }

        public static void RegisterDoor(IDoor door)
        {
            if (!m_doors.ContainsKey(door.DoorID))
            {
                List<IDoor> createDoorList = new List<IDoor>();
                m_doors.Add(door.DoorID, createDoorList);
            }

            List<IDoor> addDoorList = m_doors[door.DoorID] as List<IDoor>;
            addDoorList.Add(door);
        }

        public static void UnRegisterDoor(int doorID)
        {
            if (m_doors.ContainsKey(doorID))
            {
                m_doors.Remove(doorID);
            }
        }

        /// <summary>
        /// This function get the door object by door index
        /// </summary>
        /// <returns>return the door with the index</returns>
        public static List<IDoor> getDoorByID(int id)
        {
            if (m_doors.ContainsKey(id))
            {
                return m_doors[id];
            }
            else
            {
                return new List<IDoor>();
            }
        }
    }
}
