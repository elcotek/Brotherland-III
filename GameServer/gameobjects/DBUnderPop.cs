
using System;

using DOL.Database;
using DOL.Database.Attributes;

namespace DOL
{
    namespace DOL.Database
    {

        [DataTable(TableName = "Pobulation")]
        public class DBPopulation : DataObject
        {
            //  protected string m_id_unique;
            //   protected int m_Population;
            //  protected int m_Populationid;
            private int DBPopulationID;
            private int m_pa;
            private int m_pm;
            private int m_ph;
            private int m_realm;
            private int m_id;

            protected bool m_allowUpdate = true;



            public DBPopulation()
            { }
            /*public DBPopulation(int DBPopulationID, int PA, int PM, int PH)
            {
                AllowUpdate = true;
                m_pa = PA;
                m_pm = PM;
                m_ph = PH;
                m_id = DBPopulationID;
            }
            public bool AllowUpdate
            {
                get { return (m_allowUpdate); }
                set { m_allowUpdate = true; }
            }
            public override bool Dirty
            {
                get { return base.Dirty; }
                set
                {
                    if (AllowUpdate)
                        base.Dirty = value;
                    else
                        base.Dirty = false;
                }
          */

            [PrimaryKey]/// [PrimaryKey(AutoIncrement = true)]
            public int DBPopulationID
            {
                get { return m_id; }
                set
                {

                    Dirty = true;
                    m_id = value;
                }
            }
            [DataElement(AllowDbNull = false)]
            public int PA
            {
                get
                {
                    Console.WriteLine("OMG! es wurde PA gelesen*************");
                    return this.m_pa;
                }
                set
                {
                    Dirty = true;
                    Console.WriteLine("OMG! es wurde PA gesetzt*************");
                    this.m_pa = value;
                }
            }

            [DataElement(AllowDbNull = false)]
            public int PH
            {
                get
                {
                    Console.WriteLine("OMG! es wurde PH gelesen*************");
                    return this.m_ph;
                }
                set
                {
                    Dirty = true;
                    Console.WriteLine("OMG! es wurde PA gesetzt*************");
                    this.m_ph = value;
                }
            }
            [DataElement(AllowDbNull = false)]
            public int PM
            {
                get
                {
                    Console.WriteLine("OMG! es wurde PM gelesen*************");
                    return this.m_pm;
                }
                set
                {
                    Dirty = true;
                    Console.WriteLine("OMG! es wurde PA gesetzt*************");
                    this.m_pm = value;
                }
            }
        }
    }
}

