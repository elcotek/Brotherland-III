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
using System.Threading;

using DOL.GS.Utils;
using log4net;
using System.IO;

namespace DOL.GS
{
    /// <summary>
    /// This class represents one Zone in DAOC. It holds all relevant information
    /// that is needed to do different calculations.
    /// </summary>
    public class Zone
    {
        /* 
        This file has been extensively modified for the new subzone management system
        So for old version please have a look in old release
		 */

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region constants data

        private const ushort SUBZONE_NBR_ON_ZONE_SIDE = 32; // MUST BE A POWER OF 2 (current implementation limit is 128 inclusive)

        /// <summary>
        /// Number of SubZone in a Zone
        /// </summary>
        private const ushort SUBZONE_NBR = (ushort)(SUBZONE_NBR_ON_ZONE_SIDE * SUBZONE_NBR_ON_ZONE_SIDE);

        private const ushort SUBZONE_SIZE = (ushort)(65536 / SUBZONE_NBR_ON_ZONE_SIDE);

        private static readonly ushort SUBZONE_SHIFT = (ushort)Math.Round(Math.Log(SUBZONE_SIZE) / Math.Log(2)); // to get log in base 2

        private static readonly ushort SUBZONE_ARRAY_Y_SHIFT = (ushort)Math.Round(Math.Log(SUBZONE_NBR_ON_ZONE_SIDE) / Math.Log(2));

        public const ushort MAX_REFRESH_INTERVAL = 2000; // in milliseconds

        #endregion

        #region Structures Definition

        /// <summary>
        /// Object Type is Item
        /// </summary>
        public enum eGameObjectType : byte
        {
            ITEM = 0,
            NPC = 1,
            PLAYER = 2,
            DOOR = 3,
        }

        /// <summary>
        /// This class represent a node in a doubly linked list
        /// </summary>
        private class SubNodeElement
        {
            public GameObject data = null;
            public SubNodeElement next = null;
            public SubNodeElement previous = null;


            public SubNodeElement()
            {
                next = this;
                previous = this;
            }


            /// <summary>
            /// Insert a node before this one
            /// </summary>
            /// <param name="p_elem">The node to insert</param>
            public void PushBack(SubNodeElement p_elem)
            {
                p_elem.previous = this;
                p_elem.next = next;
                next.previous = p_elem;
                next = p_elem;
            }


            /// <summary>
            /// Remove this node from the list
            /// </summary>
            public void Remove()
            {
                if (previous != this)
                {
                    previous.next = next;
                    next.previous = previous;
                }

                previous = this;
                next = this;
            }
        }

        #endregion

        #region variables

        /// <summary>
        /// Contains the list of objects per subzone
        /// </summary>
        private SubNodeElement[][] m_subZoneElements;

        /// <summary>
        /// Should be accessed as [(subzone/4)|objectType]
        /// </summary>
        private long[] m_subZoneTimestamps;

        private int m_objectCount;

        /// <summary>
        /// Holds a pointer to the region that is the parent of this zone
        /// </summary>
        private Region m_Region;

        /// <summary>
        /// The ID of the Zone eg. 15
        /// </summary>
        private readonly ushort m_ID;

        /// <summary>
        /// The id of the fake zone we send to the client.
        /// This is used for instances, which also need to create fake zones aswell as regions!
        /// </summary>
        private readonly ushort m_zoneSkinID;

        /// <summary>
        /// The description of the Zone eg. "Camelot Hills"
        /// </summary>
        private string m_Description;

        /// <summary>
        /// The XOffset of this Zone inside the region
        /// </summary>
        private readonly int m_XOffset;

        /// <summary>
        /// The YOffset of this Zone inside the region
        /// </summary>
        private readonly int m_YOffset;

        /// <summary>
        /// The Width of the Zone in Coordinates
        /// </summary>
        private readonly int m_Width;

        /// <summary>
        /// The Height of the Zone in Coordinates
        /// </summary>
        private readonly int m_Height;

        /// <summary>
        /// The waterlevel of this zone
        /// </summary>
        private int m_waterlevel;

        /// <summary>
        /// Does this zone support diving?
        /// </summary>
        private bool m_isDivingEnabled;

        /// <summary>
        /// Does this zone contain Lava
        /// </summary>
        private bool m_isLava;

        /// <summary>
        /// already initialized?
        /// </summary>
        private bool m_initialized = false;


        private int m_bonusXP = 0;
        private int m_bonusRP = 0;
        private int m_bonusBP = 0;
        private int m_bonusCoin = 0;
        private SubNodeElement currentElement;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new Zone object
        /// </summary>
        /// <param name="region">the parent region</param>
        /// <param name="id">the zone id (eg. 15)</param>
        /// <param name="desc">the zone description (eg. "Camelot Hills")</param>
        /// <param name="xoff">the X offset of this zone inside the region</param>
        /// <param name="yoff">the Y offset of this zone inside the region</param>
        /// <param name="width">the Width of this zone</param>
        /// <param name="height">the Height of this zone</param>
        /// <param name="zoneskinID">For clientside positioning in instances: The 'fake' zoneid we send to clients.</param>
        public Zone(Region region, ushort id, string desc, int xoff, int yoff, int width, int height, ushort zoneskinID, bool isDivingEnabled, int waterlevel, bool islava, int xpBonus, int rpBonus, int bpBonus, int coinBonus)
        {
            m_Region = region;
            m_ID = id;
            m_Description = desc;
            m_XOffset = xoff;
            m_YOffset = yoff;
            m_Width = width;
            m_Height = height;
            m_zoneSkinID = zoneskinID;
            m_waterlevel = waterlevel;
            m_isDivingEnabled = isDivingEnabled;
            m_isLava = islava;

            m_bonusXP = xpBonus;
            m_bonusRP = rpBonus;
            m_bonusBP = bpBonus;
            m_bonusCoin = coinBonus;

            // initialise subzone objects and counters
            m_subZoneElements = new SubNodeElement[SUBZONE_NBR][];
            m_initialized = false;

            // Nydirac 09/11/2016
            // Load terrain and offset files for calculating Z
            // TODO: if needed load water map

            // factors loading....

            // Yeah better to create some field on the db but i cant bother now... Its pretty simple to do

            string sectorpath = GameServer.Instance.Configuration.RootDirectory + "\\ServerFiles\\dat" + id.ToString("000") + "\\sector.dat";
            if (File.Exists(sectorpath))
            {
                TextReader reader = File.OpenText(sectorpath);

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.ToLower();

                    if (line.Contains("scalefactor"))
                    {
                        m_scalefactor = byte.Parse(line.Replace("scalefactor", "").Replace("=", "").Trim()); // separate replace of = because of inconsistency of spaces... not worth the time to use regex

                        if (m_offsetfactor != 0) //Dont need to parse ALL the file, its ugly but meh
                            break;
                    }

                    if (line.Contains("offsetfactor"))
                    {
                        m_offsetfactor = byte.Parse(line.Replace("offsetfactor", "").Replace("=", "").Trim());

                        if (m_scalefactor != 0) //Dont need to parse ALL the file, its ugly but meh
                            break;
                    }
                }
                reader.Dispose();
            }

            // Offset loading...
            string offsetpath = GameServer.Instance.Configuration.RootDirectory + "\\ServerFiles\\dat" + id.ToString("000") + "\\offset.pcx";
            if (File.Exists(offsetpath))
            {
                System.Drawing.Bitmap offsetbmap = PCXFile.GetBitmap(offsetpath);
                byte[,] zoff = new byte[256, 256];
                for (int y = 0; y < 255; y++)
                {
                    for (int x = 0; x < 255; x++)
                    {
                        zoff[x, y] = offsetbmap.GetPixel(x, y).R;
                    }
                }

                m_zoneOffset = zoff;
                offsetbmap.Dispose();
            }

            // Offset loading...
            string terrainpath = GameServer.Instance.Configuration.RootDirectory + "\\ServerFiles\\dat" + id.ToString("000") + "\\terrain.pcx";
            if (File.Exists(terrainpath))
            {
                System.Drawing.Bitmap terrainbmap = PCXFile.GetBitmap(terrainpath);
                byte[,] zterr = new byte[256, 256];
                for (int y = 0; y < 255; y++)
                {
                    for (int x = 0; x < 255; x++)
                    {
                        zterr[x, y] = terrainbmap.GetPixel(x, y).R;
                    }
                }
                terrainbmap.Dispose();
                m_zoneTerrain = zterr;
            }

            if (m_offsetfactor != 0 && m_scalefactor != 0 && m_zoneTerrain != null && m_zoneOffset != null) // Z System components needed
                m_zActive = true;
        }


        public void Delete()
        {
            for (int i = 0; i < SUBZONE_NBR; i++)
            {
                if (m_subZoneElements[i] != null)
                {
                    for (int k = 0; k < m_subZoneElements[i].Length; k++)
                    {
                        if (m_subZoneElements[i][k] != null)
                        {
                            m_subZoneElements[i][k].data = null;
                            m_subZoneElements[i][k] = null;
                        }
                    }

                    m_subZoneElements[i] = null;
                }
            }

            m_subZoneElements = null;
            m_subZoneTimestamps = null;
            m_Region = null;
            DOL.Events.GameEventMgr.RemoveAllHandlersForObject(this);
        }

        private void InitializeZone()
        {
            if (m_initialized) return;
            for (int i = 0; i < SUBZONE_NBR; i++)
            {
                m_subZoneElements[i] = new SubNodeElement[4];
                for (int k = 0; k < m_subZoneElements[i].Length; k++)
                {
                    m_subZoneElements[i][k] = new SubNodeElement();
                }
            }
            m_subZoneTimestamps = new long[SUBZONE_NBR << 2];
            m_initialized = true;
        }

        #endregion

        #region properties

        public eRealm GetRealm()
        {
            return GetRealmByZoneID(ID);
        }

        /// <summary>
        /// Gets the Zones Realm by passing the ZoneID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static eRealm GetRealmByZoneID(ushort id)
        {
            //nice website here http://www.teatromusica.it/valmerwolf/varie/mappe/numzones.htm
            //wish there was a better way to do this

            //classic
            if (id >= 0 && id <= 26) return eRealm.Albion;
            else if (id >= 100 && id <= 129) return eRealm.Midgard;
            else if (id >= 200 && id <= 224) return eRealm.Hibernia;
            //SI
            else if (id >= 51 && id <= 62) return eRealm.Albion;
            else if (id >= 151 && id <= 161) return eRealm.Midgard;
            else if (id >= 181 && id <= 191) return eRealm.Hibernia;
            //foundations
            else if (id >= 13 && id <= 20) return eRealm.Albion;
            else if (id == 64) return eRealm.Albion;
            else if (id >= 260 && id <= 261) return eRealm.Albion;
            else if (id >= 114 && id <= 122) return eRealm.Midgard;
            else if (id >= 266 && id <= 267) return eRealm.Midgard;
            else if (id >= 213 && id <= 219) return eRealm.Hibernia;
            else if (id >= 272 && id <= 273) return eRealm.Hibernia;
            else if (id == 225) return eRealm.Hibernia;
            //old frontiers
            else if (id >= 11 && id <= 15) return eRealm.Albion;
            else if (id >= 111 & id <= 115) return eRealm.Midgard;
            else if (id >= 210 && id <= 214) return eRealm.Hibernia;
            //others
            //else if (id == 27) return eRealm.Albion;
            else if (id == 28 || id == 157) return eRealm.Midgard;
            else if (id == 29) return eRealm.Hibernia;
            //TOA
            else if (id == 70) return eRealm.Albion;
            else if (id >= 30 && id <= 47) return eRealm.Albion;
            else if (id == 71) return eRealm.Midgard;
            else if (id >= 73 && id <= 90) return eRealm.Midgard;
            else if (id == 72) return eRealm.Hibernia;
            else if (id >= 130 && id <= 147) return eRealm.Hibernia;
            //catacombs
            else if (id == 59) return eRealm.Albion;
            else if (id == 61) return eRealm.Hibernia;
            else if (id >= 63 && id <= 69) return eRealm.Albion;
            else if (id == 109 || id == 196 || id == 227) return eRealm.Albion;
            else if (id == 58) return eRealm.Midgard;
            else if (id == 148 || id == 149 || id == 162 || id == 188 || id == 189 ||
                     id == 195 || id == 226 || id == 229 || id == 243) return eRealm.Midgard;
            else if (id >= 92 && id <= 99) return eRealm.Hibernia;
            else if (id == 197 || id == 228) return eRealm.Hibernia;
            //instanced dungeons
            else if (id == 343 || id == 498) return eRealm.Albion;
            else if (id >= 376 && id <= 436) return eRealm.Albion;
            else if (id >= 300 && id <= 375) return eRealm.Midgard;
            else if (id == 439 || id == 440 || id == 497) return eRealm.Midgard;
            else if (id == 49 || id == 344) return eRealm.Hibernia;
            else if (id >= 437 && id <= 499) return eRealm.Hibernia;
            //new frontier & common
            else if (id >= 167 && id <= 170) return eRealm.Midgard;
            else if (id >= 171 && id <= 174) return eRealm.Hibernia;
            else if (id >= 175 && id <= 178) return eRealm.Albion;
            else if (id == 234) return eRealm.Albion;
            else if (id == 235) return eRealm.Midgard;
            else if (id == 236) return eRealm.Hibernia;
            else if (id >= 167 && id <= 170) return eRealm.Midgard;
            else if (id >= 171 && id <= 174) return eRealm.Hibernia;
            else if (id >= 175 && id <= 178) return eRealm.Albion;
            //new frontier and common
            //bg1 Fort Brolorn
            else if (id == 234) return eRealm.Midgard;
            //bg5 Leonis Keep
            else if (id == 235) return eRealm.Hibernia;
            //bg10 Caer Claret
            else if (id == 236) return eRealm.Albion;
            //bg15 Dun Killaloe
            else if (id == 237) return eRealm.Hibernia;
            //bg20 Thidranki Faste
            else if (id == 238) return eRealm.Midgard;
            //bg25 Dun Braemer
            else if (id == 239) return eRealm.Hibernia;
            //bg30 Caer Wilton
            else if (id == 240) return eRealm.Albion;
            //bg35 Molvik Faste
            else if (id == 241) return eRealm.Midgard;
            //bg40 Leirvik Castle
            else if (id == 242) return eRealm.Hibernia;

            //todo get the base realm for the other bgs not just the first 3
            return eRealm.None;
        }

        public bool IsDungeon
        {
            get
            {
                switch (m_Region.ID)
                {
                    case 24:
                    case 65:
                    case 66:
                    case 67:
                    case 68:
                    case 92:
                    case 93:
                    case 109:
                    case 149:
                    case 196:
                    case 221:
                    case 227:
                    case 228:
                    case 229:
                    case 244:
                    case 249:
                    case 296:
                    case 297:
                    case 298:
                    case 326:
                    case 335:
                    case 352:
                    case 356:
                    case 376:
                    case 377:
                    case 379:
                    case 382:
                    case 383:
                    case 386:
                    case 387:
                    case 388:
                    case 390:
                    case 395:
                    case 396:
                    case 397:
                    case 415:
                    case 443:
                    case 489://lvl5-9 Demons breach
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Returns the region of this zone
        /// </summary>
        public Region ZoneRegion
        {
            get { return m_Region; }
            set { m_Region = value; }
        }

        /// <summary>
        /// Returns the ID of this zone
        /// </summary>
        public ushort ID
        {
            get { return m_ID; }
        }

        //Dinberg: added for instances.
        /// <summary>
        /// The ID we send to the client, for client-side positioning of gameobjects and npcs.
        /// </summary>
        public ushort ZoneSkinID
        { get { return m_zoneSkinID; } }

        /// <summary>
        /// Return the description of this zone
        /// </summary>
        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }

        /// <summary>
        /// Returns the XOffset of this Zone
        /// </summary>
        public int XOffset
        {
            get { return m_XOffset; }
        }

        /// <summary>
        /// Returns the YOffset of this Zone
        /// </summary>
        public int YOffset
        {
            get { return m_YOffset; }
        }

        /// <summary>
        /// Returns the Width of this Zone
        /// </summary>
        public int Width
        {
            get { return m_Width; }
        }

        /// <summary>
        /// Returns the Height of this Zone
        /// </summary>
        public int Height
        {
            get { return m_Height; }
        }

        public int Waterlevel
        {
            get { return m_waterlevel; }
            set { m_waterlevel = value; }
        }

        public bool IsDivingEnabled
        {
            get { return m_isDivingEnabled; }
            set { m_isDivingEnabled = value; }
        }

        /// <summary>
        /// Is water in this zone lava?
        /// </summary>
        public virtual bool IsLava
        {
            get { return m_isLava; }
            set { m_isLava = value; }
        }

        /// <summary>
        /// Returns the total number of objects held in the zone
        /// </summary>
        public int TotalNumberOfObjects
        {
            get { return m_objectCount; }
        }

        #endregion

        #region New subzone Management function


        public bool IsPathingEnabled { get; set; } = false;

        private short GetSubZoneOffset(int lineSubZoneIndex, int columnSubZoneIndex)
        {
            return (short)(columnSubZoneIndex + (lineSubZoneIndex << SUBZONE_ARRAY_Y_SHIFT));
        }


        /// <summary>
        /// Returns the SubZone index using a position in the zone
        /// </summary>
        /// <param name="p_X">X position</param>
        /// <param name="p_Y">Y position</param>
        /// <returns>The SubZoneIndex</returns>
        private short GetSubZoneIndex(float p_X, float p_Y)
        {
            int xDiff = (int)(p_X - m_XOffset);
            int yDiff = (int)(p_Y - m_YOffset);

            if ((xDiff < 0) || (xDiff > 65535) || (yDiff < 0) || (yDiff > 65535))
            {
                // the object is out of the zone
                return -1;
            }
            else
            {
                // the object is in the zone
                //DOLConsole.WriteWarning("GetSubZoneIndex : " + SUBZONE_NBR_ON_ZONE_SIDE + ", " + SUBZONE_NBR + ", " + SUBZONE_SHIFT + ", " + SUBZONE_ARRAY_Y_SHIFT);

                xDiff >>= SUBZONE_SHIFT;
                yDiff >>= SUBZONE_SHIFT;

                return GetSubZoneOffset(yDiff, xDiff);
            }
        }

        /// <summary>
        /// Get the index of the subzone from the GameObject position
        /// </summary>
        /// <param name="p_Obj">The GameObject</param>
        /// <returns>The index of the subzone</returns>
        private short GetSubZoneIndex(GameObject p_Obj)
        {
            return GetSubZoneIndex(p_Obj.X, p_Obj.Y);
        }


        /// <summary>
        /// Handle a GameObject entering a zone
        /// </summary>
        /// <param name="p_Obj">The GameObject object</param>
        public void ObjectEnterZone(GameObject p_Obj)
        {

            if (!m_initialized) InitializeZone();
            int subZoneIndex = GetSubZoneIndex(p_Obj);
            if ((subZoneIndex >= 0) && (subZoneIndex < SUBZONE_NBR))
            {
                SubNodeElement element = new SubNodeElement
                {
                    data = p_Obj
                };

                int type = -1;

                //Only GamePlayer, GameNPC and GameStaticItem classes
                //are handled.
                if (p_Obj is GamePlayer)
                    type = (int)eGameObjectType.PLAYER;
                else if (p_Obj is GameNPC)
                    type = (int)eGameObjectType.NPC;
                else if (p_Obj is GameStaticItem)
                    type = (int)eGameObjectType.ITEM;
                else if (p_Obj is IDoor)
                    type = (int)eGameObjectType.DOOR;
                if (type == -1)
                    return;

                if (log.IsDebugEnabled)
                {
                    log.Debug("Object " + p_Obj.ObjectID.ToString() + " (" + ((eGameObjectType)type).ToString() + ") entering subzone " + subZoneIndex.ToString() + " in zone " + this.ID.ToString() + " in region " + m_Region.ID.ToString());
                }

                lock (m_subZoneElements[subZoneIndex][type])
                {
                    // add to subzone list
                    m_subZoneElements[subZoneIndex][type].PushBack(element);
                }

                Interlocked.Increment(ref m_objectCount);
            }
        }


        /// <summary>
        /// Handles movement of objects from zone to zone
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="element"></param>
        private void ObjectEnterZone(eGameObjectType objectType, SubNodeElement element)
        {

            if (!m_initialized) InitializeZone();
            int subZoneIndex = GetSubZoneIndex(element.data);

            if (log.IsDebugEnabled)
            {
                log.Debug("Object " + element.data.ObjectID.ToString() + "(" + objectType.ToString() + ") entering (inner) subzone " + subZoneIndex.ToString() + " in zone " + this.ID.ToString() + " in region " + m_Region.ID.ToString());
            }

            if ((subZoneIndex >= 0) && (subZoneIndex < SUBZONE_NBR))
            {
                int type = (int)objectType;

                lock (m_subZoneElements[subZoneIndex][type])
                {
                    // add to subzone list
                    m_subZoneElements[subZoneIndex][type].PushBack(element);
                }

                Interlocked.Increment(ref m_objectCount);
            }
        }


        /// <summary>
        /// Gets the lists of objects, located in the current Zone and of the given type, that are at most at a 'radius' distance from (x,y,z)
        /// The found objects are appended to the given 'partialList'.
        /// </summary>
        /// <param name="type">the type of objects to look for</param>
        /// <param name="x">the x coordinate of the observation position</param>
        /// <param name="y">the y coordinate of the observation position</param>
        /// <param name="z">the z coordinate of the observation position</param>
        /// <param name="radius">the radius to check against</param>
        /// <param name="partialList">an initial (eventually empty but initialized, i.e. never null !!) list of objects</param>
        /// <returns>partialList augmented with the new objects verigying both type and radius in the current Zone</returns>
        internal List<GameObject> GetObjectsInRadius(eGameObjectType type, float x, float y, float z, ushort radius, List<GameObject> partialList)
        {
            if (!m_initialized) InitializeZone();
            // initialisiere Parameter
            uint sqRadius = (uint)radius * (uint)radius;
            int referenceSubzoneIndex = GetSubZoneIndex((int)x, (int)y);
            int typeIndex = (int)type;

            int xInZone = (int)(x - m_XOffset); // x in Zonen-Koordinaten
            int yInZone = (int)(y - m_YOffset); // y in Zonen-Koordinaten

            int cellNbr = (radius >> SUBZONE_SHIFT) + 1; // Radius in Bezug auf die Subzone
            int xInCell = xInZone >> SUBZONE_SHIFT; // xInZone in Bezug auf Subzone-Koordinaten
            int yInCell = yInZone >> SUBZONE_SHIFT; // yInZone in Bezug auf Subzone-Koordinaten

            int minColumn = xInCell - cellNbr;
            if (minColumn < 0)
            {
                minColumn = 0;
            }

            int maxColumn = xInCell + cellNbr;
            if (maxColumn > (SUBZONE_NBR_ON_ZONE_SIDE - 1))
            {
                maxColumn = SUBZONE_NBR_ON_ZONE_SIDE - 1;
            }

            int minLine = yInCell - cellNbr;
            if (minLine < 0)
            {
                minLine = 0;
            }

            int maxLine = yInCell + cellNbr;
            if (maxLine > (SUBZONE_NBR_ON_ZONE_SIDE - 1))
            {
                maxLine = SUBZONE_NBR_ON_ZONE_SIDE - 1;
            }

            Collections.Hashtable inZoneElements = new Collections.Hashtable();
            Collections.Hashtable outOfZoneElements = new Collections.Hashtable();

            for (int currentLine = minLine; currentLine <= maxLine; ++currentLine)
            {
                int currentSubZoneIndex = 0;
                SubNodeElement startElement = null;

                for (int currentColumn = minColumn; currentColumn <= maxColumn; ++currentColumn)
                {
                    currentSubZoneIndex = GetSubZoneOffset(currentLine, currentColumn);

                    // Holen Sie sich die richtige Liste von Objekten
                    startElement = m_subZoneElements[currentSubZoneIndex][typeIndex];

                    if (startElement != startElement.next)
                    { // erlauben Sie hier schmutzige Lesevorg�nge f�r eine effizientere und feink�rnige Sperrung sp�ter...
                      // die aktuelle Subzone enth�lt einige Objekte

                        if (currentSubZoneIndex == referenceSubzoneIndex)
                        {
                            lock (startElement)
                            {
                                // Wir befinden uns in der Subzone des Beobachtungspunkts
                                // => �berpr�fen Sie alle Abst�nde f�r alle Objekte in der Subzone
                                UnsafeAddToListWithDistanceCheck(startElement, (int)x, (int)y, (int)z, sqRadius, typeIndex, currentSubZoneIndex, partialList, inZoneElements, outOfZoneElements);
                                UnsafeUpdateSubZoneTimestamp(currentSubZoneIndex, typeIndex);
                            }
                        }
                        else
                        {
                            int xLeft = currentColumn << SUBZONE_SHIFT;
                            int xRight = xLeft + SUBZONE_SIZE;
                            int yTop = currentLine << SUBZONE_SHIFT;
                            int yBottom = yTop + SUBZONE_SIZE;

                            if (CheckMinDistance(xInZone, yInZone, xLeft, xRight, yTop, yBottom, sqRadius))
                            {
                                // Der Mindestabstand ist kleiner als der Radius

                                if (CheckMaxDistance(xInZone, yInZone, xLeft, xRight, yTop, yBottom, sqRadius))
                                {
                                    // Die aktuelle Subzone ist vollst�ndig im Radius enthalten
                                    // => F�gen Sie alle Objekte der aktuellen Subzone hinzu

                                    lock (startElement)
                                    {
                                        UnsafeAddToListWithoutDistanceCheck(startElement, typeIndex, currentSubZoneIndex, partialList, inZoneElements, outOfZoneElements);
                                        UnsafeUpdateSubZoneTimestamp(currentSubZoneIndex, typeIndex);
                                    }
                                }
                                else
                                {
                                    // Die aktuelle Subzone ist teilweise im Radius enthalten
                                    // => F�gen Sie nur die Objekte im richtigen Bereich hinzu

                                    lock (startElement)
                                    {
                                        UnsafeAddToListWithDistanceCheck(startElement, (int)x, (int)y, (int)z, sqRadius, typeIndex, currentSubZoneIndex, partialList, inZoneElements, outOfZoneElements);
                                        UnsafeUpdateSubZoneTimestamp(currentSubZoneIndex, typeIndex);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //
            // F�hren Sie die erforderlichen Relokalisierungen durch
            //

            if (inZoneElements.Count > 0)
            {
                PlaceElementsInZone(inZoneElements);

                if (log.IsDebugEnabled)
                {
                    log.Debug("Zone" + ID.ToString() + " " + inZoneElements.Count.ToString() + " Objekte wurden in die Zone verschoben");
                }
            }

            if (outOfZoneElements.Count > 0)
            {
                PlaceElementsInOtherZones(outOfZoneElements);

                if (log.IsDebugEnabled)
                {
                    log.Debug("Zone" + ID.ToString() + " " + outOfZoneElements.Count.ToString() + " Objekte wurden aus der Zone verschoben");
                }
            }

            return partialList;
        }


        private static Collections.Hashtable GetInZoneElemants()
        {
            Collections.Hashtable hashtable = new Collections.Hashtable();
            return hashtable;
        }

        private void UnsafeAddToListWithoutDistanceCheck(SubNodeElement startElement, int typeIndex, int subZoneIndex, List<GameObject> partialList, Collections.Hashtable inZoneElements, DOL.GS.Collections.Hashtable outOfZoneElements)
        {
            SubNodeElement currentElement = startElement.next;
            SubNodeElement elementToRemove;
            GameObject currentObject;

            do
            {
                currentObject = currentElement.data;
                if (ShouldElementMove(currentElement, typeIndex, subZoneIndex, inZoneElements, outOfZoneElements))
                {
                    elementToRemove = currentElement;
                    currentElement = currentElement.next;

                    elementToRemove.Remove();

                    if (log.IsDebugEnabled)
                    {
                        log.Debug("Zone" + ID.ToString() + ": " + ((currentObject != null) ? "object " + currentObject.ObjectID.ToString() : "ghost object") + " removed for subzone");
                    }
                }
                else
                {
                    // the current object exists, is Active and still in the current subzone
                    // => add it
                    if (!partialList.Contains(currentObject))
                    {
                        partialList.Add(currentObject);
                    }

                    currentElement = currentElement.next;
                }
            } while (currentElement != startElement);
        }



        private void UnsafeAddToListWithDistanceCheck(
            SubNodeElement startElement,
            int x,
            int y,
            int z,
            uint sqRadius,
            int typeIndex,
            int subZoneIndex,
            List<GameObject> partialList,
            Collections.Hashtable inZoneElements,
            Collections.Hashtable outOfZoneElements)
        {
            if (startElement == null)
            {
                throw new ArgumentNullException(nameof(startElement));
            }

            // => check all distances for all objects in the subzone

            SubNodeElement currentElement = startElement.next;
            SubNodeElement elementToRemove;
            GameObject currentObject;

            do
            {
                currentObject = currentElement.data;

                if (ShouldElementMove(currentElement, typeIndex, subZoneIndex, inZoneElements, outOfZoneElements))
                {
                    elementToRemove = currentElement;
                    currentElement = currentElement.next;

                    elementToRemove.Remove();

                    if (log.IsDebugEnabled)
                    {
                        log.Debug("Zone" + ID.ToString() + ": " + ((currentObject != null) ? "object " + currentObject.ObjectID.ToString() : "ghost object") + " removed for subzone");
                    }
                }
                else
                {
                    if (CheckSquareDistance(x, y, z, currentObject.X, currentObject.Y, currentObject.Z, sqRadius) && !partialList.Contains(currentObject))
                    {
                        // the current object exists, is Active and still in the current subzone
                        // moreover it is in the right range and not yet in the result set
                        // => add it
                        partialList.Add(currentObject);
                    }

                    currentElement = currentElement.next;
                }
            } while (currentElement != startElement);
        }



        #region Relocation

        internal void Relocate(object state)
        {
            if (!m_initialized) return;
            if (m_objectCount > 0)
            {
                SubNodeElement startElement;
                SubNodeElement currentElement;
                SubNodeElement elementToRemove;

                Collections.Hashtable outOfZoneElements = new Collections.Hashtable();
                Collections.Hashtable inZoneElements = new Collections.Hashtable();

                for (int subZoneIndex = 0; subZoneIndex < m_subZoneElements.Length; subZoneIndex++)
                {
                    for (int typeIndex = 0; typeIndex < m_subZoneElements[subZoneIndex].Length; typeIndex++)
                    {
                        if (GameTimer.GetTickCount() > m_subZoneTimestamps[(subZoneIndex << 2) | typeIndex])
                        {
                            // it is time to relocate some elements in this subzone
                            // => perform needed relocations of elements
                            startElement = m_subZoneElements[subZoneIndex][typeIndex];

                            lock (startElement)
                            {
                                if (startElement != startElement.next)
                                {
                                    // there are some elements in the list

                                    currentElement = startElement.next;

                                    do
                                    {
                                        if (ShouldElementMove(currentElement, typeIndex, subZoneIndex, inZoneElements, outOfZoneElements))
                                        {
                                            elementToRemove = currentElement;
                                            currentElement = currentElement.next;

                                            elementToRemove.Remove();

                                            if (log.IsDebugEnabled)
                                            {
                                                log.Debug("Zone" + ID.ToString() + " object " + elementToRemove.data.ObjectID.ToString() + " removed for subzone");
                                            }
                                        }
                                        else
                                        {
                                            currentElement = currentElement.next;
                                        }
                                    } while (currentElement != startElement);

                                    UnsafeUpdateSubZoneTimestamp(subZoneIndex, typeIndex);
                                }
                            }
                        }
                    }
                }


                if (inZoneElements.Count > 0)
                {
                    PlaceElementsInZone(inZoneElements);

                    if (log.IsDebugEnabled)
                    {
                        log.Debug("Zone" + ID.ToString() + " " + inZoneElements.Count.ToString() + " objects moved inside zone");
                    }
                }

                if (outOfZoneElements.Count > 0)
                {
                    PlaceElementsInOtherZones(outOfZoneElements);

                    if (log.IsDebugEnabled)
                    {
                        log.Debug("Zone" + ID.ToString() + " " + outOfZoneElements.Count.ToString() + " objects moved outside zone");
                    }
                }
            }
        }


        private void UnsafeUpdateSubZoneTimestamp(int subZoneIndex, int typeIndex)
        {
            var nextUpdateTimestamp = GameTimer.GetTickCount() + Zone.MAX_REFRESH_INTERVAL;

            if (nextUpdateTimestamp < 0)
            {
                // an overflow occured
                nextUpdateTimestamp += int.MaxValue; // as TickCount wraps around 0
            }

            m_subZoneTimestamps[(subZoneIndex << 2) | typeIndex] = nextUpdateTimestamp;
        }


        private bool ShouldElementMove(SubNodeElement currentElement, int typeIndex, int subZoneIndex, DOL.GS.Collections.Hashtable inZoneElements, DOL.GS.Collections.Hashtable outOfZoneElements)
        {

            if (!m_initialized) InitializeZone();
            bool removeElement = true;
            GameObject currentObject = currentElement.data;

            if ((currentObject != null) &&
                (((int)currentObject.ObjectState) == (int)GameObject.eObjectState.Active)
                && (currentObject.CurrentRegion == ZoneRegion))
            {
                // the current object exists, is Active and still in the Region where this Zone is located

                int currentElementSubzoneIndex = GetSubZoneIndex(currentObject.X, currentObject.Y);

                if (currentElementSubzoneIndex == -1)
                {
                    // the object has moved in another Zone in the same Region

                    ArrayList movedElements = (ArrayList)outOfZoneElements[typeIndex];

                    if (movedElements == null)
                    {
                        movedElements = new ArrayList();
                        outOfZoneElements[typeIndex] = movedElements;
                    }

                    movedElements.Add(currentElement);

                    Interlocked.Decrement(ref m_objectCount);
                }
                else
                {
                    // the object is still inside this Zone

                    if (removeElement = (currentElementSubzoneIndex != subZoneIndex))
                    {
                        // it has changed of subzone
                        SubNodeElement newSubZoneStartElement = m_subZoneElements[currentElementSubzoneIndex][typeIndex];
                        ArrayList movedElements = (ArrayList)inZoneElements[newSubZoneStartElement];

                        if (movedElements == null)
                        {
                            movedElements = new ArrayList();
                            inZoneElements[newSubZoneStartElement] = movedElements;
                        }

                        // make it available for relocation
                        movedElements.Add(currentElement);
                    }
                }
            }
            else
            {
                // ghost object
                // => remove it

                Interlocked.Decrement(ref m_objectCount);
            }

            return removeElement;
        }


        private void PlaceElementsInZone(DOL.GS.Collections.Hashtable elements)
        {
            ArrayList currentList;
            SubNodeElement currentElement;

            IEnumerator entryEnumerator = elements.GetEntryEnumerator();

            while (entryEnumerator.MoveNext())
            {
                Collections.DictionaryEntry currentEntry = (Collections.DictionaryEntry)entryEnumerator.Current;
                SubNodeElement currentStartElement = (SubNodeElement)currentEntry.key;

                currentList = (ArrayList)currentEntry.value;

                lock (currentStartElement)
                {
                    for (int i = 0; i < currentList.Count; i++)
                    {
                        currentElement = (SubNodeElement)currentList[i];
                        currentStartElement.PushBack(currentElement);
                    }
                }
            }
        }


        private void PlaceElementsInOtherZones(DOL.GS.Collections.Hashtable elements)
        {
            Collections.DictionaryEntry currentEntry;

            int currentType;
            Zone currentZone;

            IEnumerator entryEnumerator = elements.GetEntryEnumerator();

            while (entryEnumerator.MoveNext())
            {
                currentEntry = (Collections.DictionaryEntry)entryEnumerator.Current;
                currentType = (int)currentEntry.key;

                ArrayList currentList = (ArrayList)currentEntry.value;

                for (int i = 0; i < currentList.Count; i++)
                {
                    currentElement = (SubNodeElement)currentList[i];
                    currentZone = ZoneRegion.GetZone(currentElement.data.X, currentElement.data.Y);

                    if (currentZone != null)
                    {
                        currentZone.ObjectEnterZone((eGameObjectType)currentType, currentElement);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Checks that the square distance between two arbitary points in space is lower or equal to the given square distance
        /// </summary>
        /// <param name="x1">X of Point1</param>
        /// <param name="y1">Y of Point1</param>
        /// <param name="z1">Z of Point1</param>
        /// <param name="x2">X of Point2</param>
        /// <param name="y2">Y of Point2</param>
        /// <param name="z2">Z of Point2</param>
        /// <param name="sqDistance">the square distance to check for</param>
        /// <returns>The distance</returns>
        public static bool CheckSquareDistance(int x1, int y1, int z1, int x2, int y2, int z2, uint sqDistance)
        {
            int xDiff = x1 - x2;
            long dist = ((long)xDiff) * xDiff;

            if (dist > sqDistance)
            {
                return false;
            }

            int yDiff = y1 - y2;
            dist += ((long)yDiff) * yDiff;

            if (dist > sqDistance)
            {
                return false;
            }

            int zDiff = z1 - z2;
            dist += ((long)zDiff) * zDiff;

            if (dist > sqDistance)
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Checks that the minimal distance between a point and a subzone (defined by the four position of the sides) is lower or equal
        /// to the distance (given as a square distance)
        /// PRECONDITION : the point is not in the tested subzone
        /// </summary>
        /// <param name="x">X position of the point</param>
        /// <param name="y">Y position of the square</param>
        /// <param name="xLeft">X value of the left side of the square</param>
        /// <param name="xRight">X value of the right side of the square</param>
        /// <param name="yTop">Y value of the top side of the square</param>
        /// <param name="yBottom">Y value of the bottom side of the square</param>
        /// <param name="squareRadius">the square of the radius to check for</param>
        /// <returns>The distance</returns>
        private bool CheckMinDistance(int x, int y, int xLeft, int xRight, int yTop, int yBottom, uint squareRadius)
        {
            long distance;

            if ((y >= yTop) && (y <= yBottom))
            {
                int xdiff = Math.Min(FastMath.Abs(x - xLeft), FastMath.Abs(x - xRight));
                distance = (long)xdiff * xdiff;
            }
            else
            {
                if ((x >= xLeft) && (x <= xRight))
                {
                    int ydiff = Math.Min(FastMath.Abs(y - yTop), FastMath.Abs(y - yBottom));
                    distance = (long)ydiff * ydiff;
                }
                else
                {
                    int xdiff = Math.Min(FastMath.Abs(x - xLeft), FastMath.Abs(x - xRight));
                    int ydiff = Math.Min(FastMath.Abs(y - yTop), FastMath.Abs(y - yBottom));
                    distance = (long)xdiff * xdiff + (long)ydiff * ydiff;
                }
            }

            return (distance <= squareRadius);
        }


        /// <summary>
        /// Checks that the maximal distance between a point and a subzone (defined by the four position of the sides) is lower or equal
        /// to the distance (given as a square distance)
        /// </summary>
        /// <param name="x">X position of the point</param>
        /// <param name="y">Y position of the square</param>
        /// <param name="xLeft">X value of the left side of the square</param>
        /// <param name="xRight">X value of the right side of the square</param>
        /// <param name="yTop">Y value of the top side of the square</param>
        /// <param name="yBottom">Y value of the bottom side of the square</param>
        /// <param name="squareRadius">the square of the radius to check for</param>
        /// <returns>The distance</returns>
        private bool CheckMaxDistance(int x, int y, int xLeft, int xRight, int yTop, int yBottom, uint squareRadius)
        {
            long distance;

            int xdiff = Math.Max(FastMath.Abs(x - xLeft), FastMath.Abs(x - xRight));
            int ydiff = Math.Max(FastMath.Abs(y - yTop), FastMath.Abs(y - yBottom));
            distance = (long)xdiff * xdiff + (long)ydiff * ydiff;

            return (distance <= squareRadius);
        }

        #endregion

        #region Area functions

        /// <summary>
        /// Convinientmethod for Region.GetAreasOfZone(),
        /// since zone.Region.getAreasOfZone(zone,x,y,z) is a bit confusing ...
        /// </summary>
        /// <param name="spot"></param>
        /// <returns></returns>
        public IList GetAreasOfSpot(IPoint3D spot)
        {
            return GetAreasOfSpot(spot, true);
        }

        public IList GetAreasOfSpot(int x, int y, int z)
        {
            return m_Region.GetAreasOfZone(this, x, y, z);
        }

        public IList GetAreasOfSpot(IPoint3D spot, bool checkZ)
        {
            return m_Region.GetAreasOfZone(this, spot, checkZ);
        }

        #endregion

        #region Get random NPC

        /// <summary>
        /// Get's a random NPC based on a con level
        /// </summary>
        /// <param name="realm"></param>
        /// <param name="compareLevel"></param>
        /// <param name="conLevel">-3 grey, -2 green, -1 blue, 0 yellow, 1 - orange, 2 red, 3 purple</param>
        /// <returns></returns>
        public GameNPC GetRandomNPCByCon(eRealm realm, int compareLevel, int conLevel)
        {
            List<GameNPC> npcs = GetNPCsOfZone(new eRealm[] { realm }, 0, 0, compareLevel, conLevel, true);
            GameNPC randomNPC = (npcs.Count == 0 ? null : npcs[0]);
            return randomNPC;
        }

        /// <summary>
        /// Get a random NPC belonging to a realm
        /// </summary>
        /// <param name="realm">The realm the NPC belong to</param>
        /// <returns>a npc</returns>
        public GameNPC GetRandomNPC(eRealm realm)
        {
            return GetRandomNPC(new eRealm[] { realm }, -1, -1);
        }


        /// <summary>
        /// Get a random NPC belonging to a realm between levels minlevel and maxlevel
        /// </summary>
        /// <param name="realm">The realm the NPC belong to</param>
        /// <param name="minLevel">The minimal level of the NPC</param>
        /// <param name="maxLevel">The maximal level NPC</param>
        /// <returns>A npc</returns>
        public GameNPC GetRandomNPC(eRealm realm, int minLevel, int maxLevel)
        {
            return GetRandomNPC(new eRealm[] { realm }, minLevel, maxLevel);
        }


        /// <summary>
        /// Get a random npc from zone with given realms
        /// </summary>
        /// <param name="realms">The realms to get the NPC from</param>
        /// <returns>The NPC</returns>
        public GameNPC GetRandomNPC(eRealm[] realms)
        {
            return GetRandomNPC(realms, -1, -1);
        }


        /// <summary>
        /// Get a random npc from zone with given realms
        /// </summary>
        /// <param name="realms">The realms to get the NPC from</param>
        /// <param name="maxLevel">The minimal level of the NPC</param>
        /// <param name="minLevel">The maximum level of the NPC</param>
        /// <returns>The NPC</returns>
        public GameNPC GetRandomNPC(eRealm[] realms, int minLevel, int maxLevel)
        {
            List<GameNPC> npcs = GetNPCsOfZone(realms, minLevel, maxLevel, 0, 0, true);
            GameNPC randomNPC = (npcs.Count == 0 ? null : npcs[0]);
            return randomNPC;
        }

        /// <summary>
        /// Gets all NPC's in zone
        /// </summary>
        /// <param name="realm"></param>
        /// <returns></returns>
        public List<GameNPC> GetNPCsOfZone(eRealm realm)
        {
            return GetNPCsOfZone(new eRealm[] { realm }, 0, 0, 0, 0, false);
        }


        /// <summary>
        /// Get NPCs of a zone given various parameters
        /// </summary>
        /// <param name="realms"></param>
        /// <param name="minLevel"></param>
        /// <param name="maxLevel"></param>
        /// <param name="compareLevel"></param>
        /// <param name="conLevel"></param>
        /// <param name="firstOnly"></param>
        /// <returns></returns>
        public List<GameNPC> GetNPCsOfZone(eRealm[] realms, int minLevel, int maxLevel, int compareLevel, int conLevel, bool firstOnly)
        {
            List<GameNPC> list = new List<GameNPC>();

            try
            {
                if (!m_initialized) InitializeZone();
                // select random starting subzone and iterate over all objects in subzone than in all subzone...
                int currentSubZoneIndex = Util.Random(SUBZONE_NBR);
                int startSubZoneIndex = currentSubZoneIndex;
                GameNPC currentNPC = null;
                bool stopSearching = false;
                do
                {
                    SubNodeElement startElement = m_subZoneElements[currentSubZoneIndex][(int)eGameObjectType.NPC];
                    lock (startElement)
                    {
                        // if list is not empty
                        if (startElement != startElement.next)
                        {
                            SubNodeElement curElement = startElement.next;
                            do
                            {
                                currentNPC = (GameNPC)curElement.data;
                                bool added = false;
                                // Check for specified realms
                                for (int i = 0; i < realms.Length; ++i)
                                {
                                    eRealm realm = realms[i];
                                    if (currentNPC.Realm == realm)
                                    {
                                        // Check for min-max level, if any specified
                                        bool addToList = true;
                                        if (compareLevel > 0 && conLevel > 0)
                                            addToList = ((int)GameObject.GetConLevel(compareLevel, currentNPC.Level) == conLevel);
                                        else
                                        {
                                            if (minLevel > 0 && currentNPC.Level < minLevel)
                                                addToList = false;
                                            if (maxLevel > 0 && currentNPC.Level > maxLevel)
                                                addToList = false;
                                        }
                                        if (addToList)
                                        {
                                            list.Add(currentNPC);
                                            added = true;
                                            break;
                                        }
                                    }
                                }
                                // If we have added and must return one only result,
                                // then mark for stop searching
                                if (firstOnly && added)
                                {
                                    stopSearching = true;
                                    break;
                                }
                                curElement = curElement.next;
                            } while (curElement != startElement);
                        }
                    }
                    if (++currentSubZoneIndex >= SUBZONE_NBR)
                    {
                        currentSubZoneIndex = 0;
                    }
                    // If stop searching forced, then exit
                    if (stopSearching)
                        break;
                } while (currentSubZoneIndex != startSubZoneIndex);
            }
            catch (Exception ex)
            {
                log.Error("GetNPCsOfZone: Caught Exception for zone " + Description + ".", ex);
            }

            return list;
        }

        #endregion

        #region Zone Bonuses
        /// <summary>
        /// Bonus XP Gained (%)
        /// </summary>
        public int BonusExperience
        {
            get { return m_bonusXP; }
            set { m_bonusXP = value; }
        }
        /// <summary>
        /// Bonus RP Gained (%)
        /// </summary>
        public int BonusRealmpoints
        {
            get { return m_bonusRP; }
            set { m_bonusRP = value; }
        }
        /// <summary>
        /// Bonus BP Gained (%)
        /// </summary>
        public int BonusBountypoints
        {
            get { return m_bonusBP; }
            set { m_bonusBP = value; }
        }
        /// <summary>
        /// Bonus Money Gained (%)
        /// </summary>
        public int BonusCoin
        {
            get { return m_bonusCoin; }
            set { m_bonusCoin = value; }
        }
        #endregion

        #region Z
        private readonly bool m_zActive = false;
        public bool zActive
        {
            get { return m_zActive; }
        }

        private readonly byte[,] m_zoneTerrain;
        private readonly byte[,] m_zoneOffset;

        private readonly byte m_scalefactor;
        private readonly byte m_offsetfactor;

        public int GetZ(int X, int Y)
        {
            if (!m_zActive || m_zoneTerrain == null || m_zoneOffset == null) return 0; // In case of error you COULD return -1 and handle all the problems...

            try
            {
                int x = X - XOffset; // EDIT: Sorry, forgot to restore after fix
                int y = Y - YOffset;
                if (x < 0 || x > 65535 || y < 0 || y > 65535)
                {
                    //log.Error(string.Format("ZMgr: Value either too big or too low! X:{0} Y:{1} ZoneID:{2}", x, y, ZoneID));
                    return 0;
                }

                int sx = x >> 8;
                int sy = y >> 8;
                double ox = x / 256.0f - sx;
                double oy = y / 256.0f - sy;

                if (ox + oy < 1) //Check the position of the point: we are going to use math to convert a 256x256 matrix to a 65kx65k! yay math!
                {
                    //Top triangle
                    int h_xy = GetZonePointHeight(sx, sy);
                    int h_x1y = GetZonePointHeight(sx + 1, sy);
                    int h_xy1 = GetZonePointHeight(sx, sy + 1);
                    return (int)(h_xy + (h_x1y - h_xy) * ox + (h_xy1 - h_xy) * oy);
                }
                else
                {
                    //Bottom triangle
                    int h_x1y = GetZonePointHeight(sx + 1, sy);
                    int h_xy1 = GetZonePointHeight(sx, sy + 1);
                    int h_x1y1 = GetZonePointHeight(sx + 1, sy + 1);
                    return (int)(h_x1y1 + (h_xy1 - h_x1y1) * (1.0 - ox) + (h_x1y - h_x1y1) * (1.0 - oy));
                }
            }
            catch (Exception)
            {
                //DOL.Promise.Mail.SendMail("nydirac@gmail.com", "Z Exception", ex.Message + " ---------> " + ex.StackTrace);
                return 0;
            }
        }


        private int GetZonePointHeight(int X, int Y) //Raw data from the terrain data
        {
            if (X > 255 || X < 0 || Y > 255 || Y < 0) // In case of freak accident, better to be safe than sorry...
            {
                //log.Warn("GetZonePointHeight error! X: " + X + " Y: " + Y + " ZoneID: " + ZoneID);
                return 0;
            }

            return (m_zoneTerrain[X, Y] * m_scalefactor + m_zoneOffset[X, Y] * m_offsetfactor);
        }

        #endregion
    }
}
