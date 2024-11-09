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
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


namespace DOL.GS.Keeps
{

    public class RelicGateMgr
    {

       
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


       
        public static void BroadcastGateOpen(GameDoor door)
        { 
            string message = String.Empty;
            if (door != null)
            {
              message = string.Format(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerManager.BroadcastGateOpen.Captured"), door.Name);
            }
           
           
            BroadcastMessage(message, eRealm.None);
            NewsMgr.CreateNews(message, eRealm.None, eNewsType.RvRGlobal, false);
        }
        /// <summary>
		/// Sends a message to all players to notify them of the Gates status
		/// </summary>
		/// <param name="keep">The keep object</param>
		/// <param name="realm">The raizing realm</param>
		/// <summary>
		/// Method to broadcast messages, if eRealm.None all can see,
		/// else only the right realm can see
		/// </summary>
		/// <param name="message">The message</param>
		/// <param name="realm">The realm</param>
		public static void BroadcastMessage(string message, eRealm realm)
        {
            foreach (GameClient client in WorldMgr.GetAllClients())
            {
                if (client.Player == null)
                    continue;
                if ((client.Account.PrivLevel != 1 || realm == eRealm.None) || client.Player.Realm == realm)
                {
                    client.Out.SendMessage(message, eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                }
            }
        }


        public static void OpenDoor(int doorID)
        {
            List<IDoor> GameDoorListTest = DoorMgr.getDoorByID(doorID);
            if (GameDoorListTest.Count > 0)
            {
                //Console.Out.WriteLine("IDoorFound !");
                foreach (IDoor mydoor in GameDoorListTest)
                {
                    GameDoor door = mydoor as GameDoor;

                    BroadcastGateOpen(door);
                    //if (door.State != eDoorState.Open)
                    //{
                    //        BroadcastGateOpen(door);
                   // }
                    if (door.State != eDoorState.Open)
                    {
                        if (door.Locked == 1)
                            door.Locked = 0;

                        mydoor.Open();
                        door.Locked = 1;
                        GameDoorListTest = null;
                        return;
                    }
                }
            }

        }
        

        public static int myid;
        public static void CheckKeeps()
        {
            //log.Error("Check Keepchain!!");
            IList m_AlbionKeeps = new ArrayList();
            IList m_MidgardKeeps = new ArrayList();
            IList m_HiberniaKeeps = new ArrayList();

            ICollection<AbstractGameKeep> keepList = KeepMgr.GetNFKeeps();
            foreach (AbstractGameKeep keep in keepList)
            {
                if (keep is GameKeep)
                {
                    foreach (GameKeepComponent keepComponent in keep.SentKeepComponents)
                    {

                        if (keep.Realm != keepComponent.Keep.OriginalRealm && keepComponent is GameKeepComponent && keepComponent != null)
                        {
                            //Albion Captured Keep Name
                            if (keepComponent.Keep.OriginalRealm == eRealm.Albion && m_AlbionKeeps.Contains(keep.Name) == false)
                            {
                                //log.ErrorFormat("Albion keep.Name {0}", keep.Name);
                                m_AlbionKeeps.Add(keep.Name);

                            }
                            //Midgard Captured Keep Name
                            if (keepComponent.Keep.OriginalRealm == eRealm.Midgard && m_MidgardKeeps.Contains(keep.Name) == false)
                            {
                                //log.ErrorFormat("Midgard keep.Name {0}", keep.Name);
                                m_MidgardKeeps.Add(keep.Name);

                            }
                            //Hibernia Captured Keep Name
                            if (keepComponent.Keep.OriginalRealm == eRealm.Hibernia && m_HiberniaKeeps.Contains(keep.Name) == false)
                            {
                                //log.ErrorFormat("Hibernia keep.Name {0}", keep.Name);
                                m_HiberniaKeeps.Add(keep.Name);

                            }
                        }
                        /*
                        //keep and tower repair if needed
                        if (keepComponent is GameKeepComponent && keepComponent != null)
                        {
                            if (keepComponent.HealthPercent < 100 && !keepComponent.InCombat)
                            {
                                //keepComponent.Repair((int)(keepComponent.MaxHealth * 0.01));
                               
                            }
                            //log.Error("Worldtimer healed Keep components");
                            
                        }
                        */
                    }
                }
            }



            bool power_Benowyc = false;
            bool power_Berkstead = false;
            bool power_Boldiam = false;
            bool power_Renaris = false;
            /// Albion Power Relic Wall - The first milegate can be opened by taking Caer Benowyc, Caer Berkstead, and Caer Boldiam. The second milegate on this wall can be opened by taking Caer Renaris after the other three have fallen.
            foreach (string val in m_AlbionKeeps)
            {
                if (power_Benowyc == false && val.Contains("Caer Benowyc"))
                {
                    power_Benowyc = true;
                }
                if (power_Berkstead == false && val.Contains("Caer Berkstead"))
                {
                    power_Berkstead = true;
                }
                if (power_Boldiam == false && val.Contains("Caer Boldiam"))
                {
                    power_Boldiam = true;
                }
                if (power_Renaris == false && val.Contains("Caer Renaris"))
                {
                    power_Renaris = true;
                }

                if (power_Benowyc && power_Berkstead && power_Boldiam)
                {
                    OpenDoor(175014001);
                    
                }
                if (power_Benowyc && power_Berkstead && power_Boldiam && power_Renaris)
                {
                    OpenDoor(175013901);
                }


            }


            bool strenght_Benowyc = false;
            bool strenght_Erasleigh = false;
            bool strenght_Sursbrook = false;
            bool strenght_Hurbury = false;
            ///Albion Strength Relic Wall - The first milegate can be opened by taking Caer Benowyc, Caer Erasleigh, and Caer Sursbrooke. The second milegate on this wall is opened by taking Caer Hurbury in addition to the other three.
            foreach (string val in m_AlbionKeeps)
            {
                if (strenght_Benowyc == false && val.Contains("Caer Benowyc"))
                {
                    strenght_Benowyc = true;
                }
                if (strenght_Erasleigh == false && val.Contains("Caer Erasleigh"))
                {
                    strenght_Erasleigh = true;
                }
                if (strenght_Sursbrook == false && val.Contains("Caer Sursbrooke"))
                {
                    strenght_Sursbrook = true;
                }
                if (strenght_Hurbury == false && val.Contains("Caer Hurbury"))
                {
                    strenght_Hurbury = true;
                }
                if (strenght_Benowyc && strenght_Erasleigh && strenght_Sursbrook)
                {
                    OpenDoor(176233801);
                }
                if (strenght_Benowyc && strenght_Erasleigh && strenght_Sursbrook && strenght_Sursbrook)
                {
                    OpenDoor(176233901);
                }
            }



            bool power_Bledmeer = false;
            bool power_Nottmoor = false;
            bool power_Glenlock = false;
            bool power_Arvakr = false;

            /// Midgard Power Relic Wall - First milegate: Bledmeer Faste, Notmoor Faste, and Glenlock Faste. Second milegate: Those three and Arvakr Faste.
            foreach (string val in m_MidgardKeeps)
            {
                if (power_Bledmeer == false && val.Contains("Bledmeer Faste"))
                {
                    power_Bledmeer = true;
                }
                if (power_Nottmoor == false && val.Contains("Nottmoor Faste"))
                {
                    power_Nottmoor = true;
                }
                if (power_Glenlock == false && val.Contains("Glenlock Faste"))
                {
                    power_Glenlock = true;
                }
                if (power_Arvakr == false && val.Contains("Arvakr Faste"))
                {
                    power_Arvakr = true;
                }
                if (power_Bledmeer && power_Nottmoor && power_Glenlock)
                {
                    OpenDoor(169000501);
                }
                if (power_Bledmeer && power_Nottmoor && power_Glenlock && power_Arvakr)
                {
                    OpenDoor(169000301);
                }
            }
            bool strenght_Bledmeer = false;
            bool strenght_Blendrake = false;
            bool strenght_Hlidskialf = false;
            bool strenght_Fensalir = false;
            /// Midgard Strength Relic Wall - First milegate: Bledmeer Faste, Blendrake Faste, and Hlidskialf Faste. Second milegate: Those three and Fensalir Faste.
            foreach (string val in m_MidgardKeeps)
            {
                if (strenght_Bledmeer == false && val.Contains("Bledmeer Faste"))
                {
                    strenght_Bledmeer = true;
                }
                if (strenght_Blendrake == false && val.Contains("Blendrake Faste"))
                {
                    strenght_Blendrake = true;
                }
                if (strenght_Hlidskialf == false && val.Contains("Hlidskialf Faste"))
                {
                    strenght_Hlidskialf = true;
                }
                if (strenght_Fensalir == false && val.Contains("Fensalir Faste"))
                {
                    strenght_Fensalir = true;
                }
                if (strenght_Bledmeer && strenght_Blendrake && strenght_Hlidskialf)
                {
                    OpenDoor(170128101); 
                }
                if (strenght_Bledmeer && strenght_Blendrake && strenght_Hlidskialf && strenght_Fensalir)
                {
                    OpenDoor(170127801);
                }
            }


            bool power_Crauchon = false;
            bool power_Crimthain = false;
            bool power_nGed = false;
            bool power_Scaithag = false;
            /// Hibernia Power Relic Wall - First milegate: Dun Crauchon, Dun Crimthain, and Dun nGed. Second milegate: Those three and Dun Scaithag.
            foreach (string val in m_HiberniaKeeps)
            {
                if (power_Crauchon == false && val.Contains("Dun Crauchon"))
                {
                    power_Crauchon = true;
                }
                if (power_Crimthain == false && val.Contains("Dun Crimthain"))
                {
                    power_Crimthain = true;
                }
                if (power_nGed == false && val.Contains("Dun nGed"))
                {
                    power_nGed = true;
                }                                         
                if (power_Scaithag == false && val.Contains("Dun Scathaig"))
                {
                    power_Scaithag = true;
                }
                if (power_Crauchon && power_Crimthain && power_nGed)
                {
                    OpenDoor(174061701);
                }
                if (power_Crauchon && power_Crimthain && power_nGed && power_Scaithag)
                {
                    OpenDoor(174149601);
                }

            }

            bool strength_Crauchon = false;
            bool strength_Bolg = false;
            bool strength_Behnn = false;
            bool strength_Ailinne = false;
            /// Hibernia Strength Relic Wall - First milegate: Dun Crauchon, Dun Bolg, and Dun da Behnn. Second milegate: Those three and Dun Ailinne. 
            foreach (string val in m_HiberniaKeeps)
            {
                if (strength_Crauchon == false && val.Contains("Dun Crauchon"))
                {
                    strength_Crauchon = true;
                }
                if (strength_Bolg == false && val.Contains("Dun Bolg"))
                {
                    strength_Bolg = true;
                }
                if (strength_Behnn == false && val.Contains("Dun da Behnn"))
                {
                    strength_Behnn = true;
                }
                if (strength_Ailinne == false && val.Contains("Dun Ailinne"))
                {
                    strength_Ailinne = true;
                }
                if (strength_Crauchon && strength_Bolg && strength_Behnn)
                {
                    OpenDoor(173000301);
                }
                if (strength_Crauchon && strength_Bolg && strength_Behnn && strength_Ailinne)
                {
                    OpenDoor(173000201);
                }
            }
        }
    }
}

                          












                            
                     

                      


		