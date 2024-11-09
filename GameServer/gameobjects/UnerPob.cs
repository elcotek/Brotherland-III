
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DOL.Database;
using DOL.Language;
using log4net;
using DOL.GS;

namespace DOL.GS
{
    public class UnerPob
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly DBUnerPob _databaseItems;




        public int Realmb
        {
            get { return _databaseItems.Realmb; }
            set { _databaseItems.Realmb = value; }
        }

        public int PA
        {
            get { return _databaseItems.PA; }
            set { _databaseItems.PA = value; }
        }



        public int PH
        {
            get { return _databaseItems.PH; }
            set { _databaseItems.PH = value; }
        }

        public int PM
        {
            get { return _databaseItems.PM; }
            set { _databaseItems.PM = value; }
        }
        public UnerPob(DBUnerPob UnerPob)
        {
            _databaseItems = UnerPob;
        }


        public void SaveIntoDatabase()
        {
            GameServer.Database.SaveObject(_databaseItems);
        }

    }
}