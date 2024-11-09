using System;
using DOL.Database.Attributes;

namespace DOL.Database
{
    /// <summary>
    /// DB village holds the x, y, z and region positions of villages within DDAoC. It also is used as a parent
    /// for village hookpoints and npcs.
    /// </summary>
    [DataTable(TableName = "Spawn")]
    public class DBSpawn : DataObject
    {
        static bool m_AllowAdd;

        private int m_x;
        private int m_y;
        private int m_z;
        private string m_dayMobs;
        private string m_nightMobs;
        private ushort m_region;
        private int m_maxNumber;
        private ushort m_radius;
        private int m_rate;
        private bool m_dumpMobsOnDayChange;


        /// <summary>
        /// Create a village row
        /// </summary>
        public DBSpawn()
        {

        }

        #region AllowAdd
        override public bool AllowAdd
        {
            get
            {
                return m_AllowAdd;
            }
            set
            {
                m_AllowAdd = value;
            }
        }
        #endregion

        #region ID

        [DataElement(AllowDbNull = true)]
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

        [DataElement(AllowDbNull = true)]
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

        #endregion

        #region Location
        [DataElement(AllowDbNull = true)]
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


        [DataElement(AllowDbNull = true)]
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
        public ushort Radius
        {
            get
            {
                return m_radius;
            }
            set
            {
                Dirty = true;
                m_radius = value;
            }
        }


        [DataElement(AllowDbNull = true)]
        public int MaxNumber
        {
            get
            {
                return m_maxNumber;
            }
            set
            {
                Dirty = true;
                m_maxNumber = value;
            }
        }

        [DataElement(AllowDbNull = true)]
        public int Rate
        {
            get
            {
                return m_rate;
            }
            set
            {
                Dirty = true;
                m_rate = value;
            }
        }

        [DataElement(AllowDbNull = true)]
        public string SerialisedNightMobs
        {
            get
            {
                return m_nightMobs;
            }
            set
            {
                Dirty = true;
                m_nightMobs = value;
            }
        }

        [DataElement(AllowDbNull = true)]
        public string SerialisedDayMobs
        {
            get
            {
                return m_dayMobs;
            }
            set
            {
                Dirty = true;
                m_dayMobs = value;
            }
        }



        [DataElement(AllowDbNull = true)]
        public bool DumpMobsOnDayChange
        {
            get
            {
                return m_dumpMobsOnDayChange;
            }
            set
            {
                Dirty = true;
                m_dumpMobsOnDayChange = value;
            }
        }

        #endregion
    }
}
