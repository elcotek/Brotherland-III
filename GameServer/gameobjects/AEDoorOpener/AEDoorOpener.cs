/*
 * Elcotek: based on the work of ML10 Porter.
 * 
 * 
 * 
 */

using DOL.AI.Brain;
using DOL.Database;
using DOL.GS;
using DOL.GS.PacketHandler;
using log4net;
using System;
using System.Collections.Generic;


namespace DOL.GS
{
    public class AEDoorOpener : GameNPC
    {
        public AEDoorOpener()
            : base()
        {
            base.SetOwnBrain(new AEDoorOpenerBrain());
        }
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        

        public const int openDuration = 50000;


    }
}


        


namespace DOL.AI.Brain
{
    public class AEDoorOpenerBrain : StandardMobBrain
    {
        public AEDoorOpenerBrain()
            : base()
        {
            base.AggroLevel = 0;
            //base.AggroRange = 0;
            base.ThinkInterval = 1500;
        }
        private bool m_aeDoorState;

        /// <summary>
        /// Door state
        /// </summary>
        public bool AEDoorState
        {
            get { return m_aeDoorState; }
            set { m_aeDoorState = value; }
        }

        /// <summary>
		/// The timed action that will close the door
		/// </summary>
		protected GameTimer m_closeDoorAction;


        


        public override void Think()
        {

            foreach (GamePlayer player in Body.GetPlayersInRadius(600))
            {
                GamePlayer t = (GamePlayer)player;

                if (player == null) continue;
                //if (player.Client.Account.PrivLevel >= (uint)ePrivLevel.GM) continue;
                if ((!Body.IsWithinRadius(player, (int)AggroRange))) continue;
                {
                    switch (t.Realm)
                    {
                        case eRealm.Albion:
                            new DOL.GS.Effects.ArtifactDoorEffect().Start(Body);
                            OPENDOOR(eRealm.Albion);
                            break;

                        case eRealm.Midgard:

                            new DOL.GS.Effects.ArtifactDoorEffect().Start(Body);

                            OPENDOOR(eRealm.Midgard);
                             break;

                        case eRealm.Hibernia:
                            //  OpenAlbionDoor();
                            new DOL.GS.Effects.ArtifactDoorEffect().Start(Body);

                            OPENDOOR(eRealm.Hibernia);

                            break;

                        default: break; 
                    }
                    base.Think();
                }
            }
        }

        public static void OPENDOOR(eRealm realm)
        {
            int myid;
            switch (realm)
            {
                case eRealm.Albion:
                    {
                        myid = 87159201;
                    }
                    break;
                case eRealm.Midgard:
                    {
                        myid = 44159201;
                    }
                    break;
                case eRealm.Hibernia:
                    {
                        myid = 144159201;

                    }
                    break;
          
            default: myid = 87159201; break;
           }
            
            
            //DBDoor DoorDBTEST = GameServer.Database.FindObjectByKey<DBDoor>(myid);
           // if (DoorDBTEST != null)
           // {
               // Console.Out.WriteLine("DoorDBFound !");
           // }
            List<IDoor> GameDoorListTest = DoorMgr.getDoorByID(myid);
            if (GameDoorListTest.Count > 0)
            {
                //Console.Out.WriteLine("IDoorFound !");
                foreach (IDoor mydoor in GameDoorListTest)
                {
                    GameDoor door = mydoor as GameDoor;
                    if (door.State == eDoorState.Open)
                    {
                        foreach(GameNPC npc in door.GetNPCsInRadius(3000))
                        {
                            if (npc.Name == "Talos")
                            {
                                (npc as GameNPC).PathTo(479881, 693161, 16345, 150);
                                if ((npc as GameNPC).X == 479881)
                                {
                                    npc.StopMoving();
                                    npc.CancelWalkToSpawn();

                                }
                            }


                        }

                    }
                    if (door.State == eDoorState.Closed)

                        mydoor.Open();
                       return;
                }
               
            }
        }
        

        private void SendReply(GamePlayer target, string msg)
        {
            target.Client.Out.SendMessage(
                msg,
                eChatType.CT_Say, eChatLoc.CL_PopupWindow);
        }
    }
}
       