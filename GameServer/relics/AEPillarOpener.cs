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
    public class AEPillarOpener : GameNPC
    {
        public AEPillarOpener()
            : base()
        {
            base.SetOwnBrain(new AEPillarOpenerBrain());
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
    public class AEPillarOpenerBrain : StandardMobBrain
    {
        public AEPillarOpenerBrain()
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

        private static bool IsRelicGates(DBDoor door)
        {
            switch (door.InternalID)
            {

                //Hibernia Dun Lamhota Hibernia Strength Relic
                case 173000201: //Melee Gates Dun Lamhota Relic Gate II
                case 173000301: //Melee Gates Dun Lamhota Relic Gate I

                //Hibernia Dun Dagta Power Relic
                case 174061701: //Magic Gates Dun Dagta Relic Gate I
                case 174149601:  //Magic Gates Dun Dagta Relic Gate II

                //Midgard  Mjollner Strength Relic
                case 170128101: //Midgard Gate Stärke Mjollner Relic Gate I
                case 170127801: //Midgard Gate Stärke Mjollner Relic Gate II

                //Grallahorn Midard Power Relic
                case 169000501: //Midgard Gate power Grallarhorn Relic Gate I
                case 169000301: //Midgard Gate power Grallarhorn Relic Gate II

                //Albion Castle Excalibur Strength
                case 176233801: //Albion Gate Strenght Castle Excalibur Relic Gate I
                case 176233901: //Albion Gate Strenght Castle Excalibur Relic Gate II

                //Albion Castle Myrddin Power
                case 175014001: //Albion power Castle Myrddin Relic Gate I
                case 175013901:  //Albion power Castle Myrddin Relic Gate II

                    {
                        return true;
                    }

                default: return false;

            }
        }

        public static void OPENDOOR(eRealm realm)
        {
            int myid;
            switch (realm)
            {
                case eRealm.Albion:
                    {
                        myid = 0;
                    }
                    break;
                case eRealm.Midgard:
                    {
                       myid = 0;
                    }
                    break;
                case eRealm.Hibernia:
                    {
                       myid = 0;

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
                        /*
                        foreach (GameNPC npc in door.GetNPCsInRadius(3000))
                        {
                            if (npc.Name == "Talos")
                            {
                                (npc as GameNPC).WalkTo(479881, 693161, 16345, 150);
                                if ((npc as GameNPC).X == 479881)
                                {
                                    npc.StopMoving();
                                    npc.CancelWalkToSpawn();

                                }
                            }


                        }
                        */

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

