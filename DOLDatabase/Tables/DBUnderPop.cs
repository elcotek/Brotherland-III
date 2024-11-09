
using System;

using DOL.Database;
using DOL.Database.Attributes;


namespace DOL
{
    namespace Database
    {

        [DataTable(TableName = "DBUnerPob")]
        public class DBUnerPob : DataObject
        {
            //  protected string m_id_unique;
            //   protected int m_Population;
            //  protected int m_Populationid;


            public int m_realmb;
            public int m_pa;
            public int m_ph;
            public int m_pm;

            public DBUnerPob() { }
            // protected bool m_allowUpdate = true;



           //public DBUnerPob()
           // { }
           public DBUnerPob(int Realmb, int PA, int PM, int PH)
            {
                m_realmb = Realmb;
                m_pa = PA;
                m_ph = PH;
                m_pm = PM;
               
                
            }
            
                    

            [PrimaryKey] /// [PrimaryKey(AutoIncrement = true)]
            public int Realmb
            {
                get {
                    Console.WriteLine("OMG! es wurde PA gelesen*************");
                    return m_realmb; 
                }
                set
                {
                    
                    Dirty = true;
                    Console.WriteLine("OMG! es wurde PA gesetzt*************");
                    m_realmb = value;
                }
            }
            [DataElement(AllowDbNull = false)]
            public int PA
            {
                get
                {
                    Console.WriteLine("OMG! es wurde PA gelesen*************");
                    return m_pa;
                }
                set
                {
                    Dirty = true;
                    Console.WriteLine("OMG! es wurde PA gesetzt*************");
                   m_pa = value;
                }
            }

            [DataElement(AllowDbNull = false)]
            public int PH
            {
                get
                {
                    Console.WriteLine("OMG! es wurde PH gelesen*************");
                    return m_ph;
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
                    m_pm = value;
                }
            }
        }
    }
}
