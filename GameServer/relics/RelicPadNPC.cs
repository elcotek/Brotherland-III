using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.Relics;
using DOL.GS;
using DOL.GS.PacketHandler;
using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using DOL.Events;
using DOL.GS.Keeps;

namespace DOL.GS.Relics
{
    public class RelicPadNPC : GameNPC
    {
        private const string PillarStatus = "PILLARSTATUS";

        private PillarPadArea m_area = null;

        public RelicPadNPC()
            : base()
        {
            //base.SetOwnBrain(new RelicPadNPCBrain());
        }


        public override bool AddToWorld()
        {


            bool success = base.AddToWorld();
            if (success)
            {
                m_area = new PillarPadArea(this);
                CurrentRegion.AddArea(m_area);

                /*
				 * <[RF][BF]Cerian> mid: mjolnerr faste (str)
					<[RF][BF]Cerian> mjollnerr
					<[RF][BF]Cerian> grallarhorn faste (magic)
					<[RF][BF]Cerian> alb: Castle Excalibur (str)
					<[RF][BF]Cerian> Castle Myrddin (magic)
					<[RF][BF]Cerian> Hib: Dun Lamfhota (str), Dun Dagda (magic)
				 */
                //Name = GlobalConstants.RealmToName((DOL.GS.PacketHandler.eRealm)Realm)+ " Relic Pad";

            }

            return success;
        }




        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);




        const int PAD_AREA_RADIUS = 150;

        public const int openDuration = 50000;


        /// <summary>
		/// Relic pad radius.
		/// </summary>
		static public int Radius
        {
            get { return 200; }
        }


        public bool PlayerCanUsePad(GamePlayer Player)
        {

            switch (Player.Realm)
            {
                case eRealm.Albion:
                    {
                        if (Player.IsWithinRadius(this, 1200) && this.Name == "AlbionPad")
                            return false;
                    }
                    break;
                case eRealm.Hibernia:
                    {
                        if (Player.IsWithinRadius(this, 1200) && this.Name == "HiberniaPad")
                            return false;
                    }
                    break;

                case eRealm.Midgard:
                    {
                        if (Player.IsWithinRadius(this, 1200) && this.Name == "MidgardPad")
                            return false;
                    }
                    break;


                default: return true;
            } 
                     
            
            return true;
        }
        private protected int m_playersOnPad = 0;
        public int PlayersOnPad
        {
            get { return m_playersOnPad; }
            set
            {
                if (value < 0)
                    return;
               

                m_playersOnPad = value;

               
                if (this.TempProperties.getProperty<bool>(PillarStatus) == false && m_playersOnPad >= GS.ServerProperties.Properties.RELIC_PLAYERS_REQUIRED_ON_PAD)
                {
                    GetDoorToOpen(true);
                    this.TempProperties.setProperty(PillarStatus, true);
                   
                }
                else if (m_playersOnPad <= 0)
                {
                    GetDoorToOpen(false);
                    this.TempProperties.setProperty(PillarStatus, false);
                }
            }
        }

       

        /// <summary>
        /// Called when a players steps on the pad.
        /// </summary>
        /// <param name="player"></param>
        public void OnPlayerEnter(GamePlayer player)
        {
           if (PlayerCanUsePad(player))
            PlayersOnPad++;
        }

        /// <summary>
        /// Called when a player steps off the pad.
        /// </summary>
        /// <param name="player"></param>
        public void OnPlayerLeave(GamePlayer player)
        {
            if (PlayerCanUsePad(player))
                PlayersOnPad--;
        }

        public sealed class PillarPadArea : Area.Circle
        {
            readonly RelicPadNPC m_parent;

            public PillarPadArea(RelicPadNPC parentPad)
                : base("", parentPad.X, parentPad.Y, parentPad.Z, PAD_AREA_RADIUS)
            {
                m_parent = parentPad;
            }

            public override void OnPlayerEnter(GamePlayer player)
            {
                m_parent.OnPlayerEnter(player);
            }

            public override void OnPlayerLeave(GamePlayer player)
            {
                m_parent.OnPlayerLeave(player);
            }
        }

        public virtual void  StartPlaySound()
        {
            ushort soundId = 42;

            foreach (GamePlayer players in Body.GetPlayersInRadius(WorldMgr.REFRESH_DISTANCE))
            {
                players.Out.SendSoundEffect(soundId, 0, 0, 0, 0, 0);
            }

        }
        
       

        int DorID;
        public virtual void GetDoorToOpen(bool open)
        {
           

            if (Body != null)
            {

                ushort _bodyType = Body.BodyType;


                switch (_bodyType)
                {

                    case 1:
                        {   //Excalibur Albion Pillar Strength
                            DorID = 176130401; //Alb str in alb
                            break;
                        }
                    case 2:
                        {
                            DorID = 170347603; //Mid str in alb
                            break;
                        }
                    case 3:
                        {
                            DorID = 173381702;  //hib str in alb
                            break;
                        }
                    case 4:
                        {  //Myrddin Albion Pillar Power
                            DorID = 175135301; //Alb  poer in alb
                            break;
                        }
                    case 5:
                        {
                            DorID = 169262403;//mid power in alb
                            break;
                        }
                    case 6:
                        {
                            DorID = 174226402; //hib hib power in alb
                            break;
                        } //Mjollner Midgard Pillar Strength
                    case 7:
                        {
                            DorID = 170347601;//mid str in mid
                            break;
                        }
                    case 8:
                        {
                            DorID = 176130402; //alb str in mid
                            break;
                        }
                    case 9:
                        {
                            DorID = 173381703; //hib str in mid
                            break;
                        }
                    case 10://Grallarhorn Mid power
                        {
                            DorID = 169262401; //mid power in mid
                            break;
                        }
                    case 11:
                        {
                            DorID = 175135302; //alb power in mid
                            break;
                        }
                    case 12:
                        {
                            DorID = 174226403; //hib power in mid
                            break;
                        }  //Lamfhota Hibernia Pillar Strength
                    case 13:
                        {
                            DorID = 173381701; //hib str in hib
                            break;
                        }
                    case 14:
                        {
                            DorID = 176130403; //alb str in hib
                            break;
                        }
                    case 15:
                        {
                            DorID = 170347602; //mid str in hib
                            break;
                        }
                    case 16:  //Dun Dagta Hibernia Pillar Power
                        {
                            DorID = 174226401; //hib power in hib
                            break;
                        }
                    case 17:
                        {
                            DorID = 175135303; //alb power in hib
                            break;
                        }
                    case 18:
                        {
                            DorID = 169262402; //mid power in hib
                            break;
                        }
                    // default: DorID = 176130401; break;
                    default: DorID = 0; break;
                }

            }
            if (DorID > 0)
            {
               
                List<IDoor> GameDoorListTest = DoorMgr.getDoorByID(DorID);
                if (GameDoorListTest != null && GameDoorListTest.Count > 0)
                {
                    //Console.Out.WriteLine("IDoorFound !");
                    foreach (IDoor mydoor in GameDoorListTest)
                    {

                        if (mydoor != null)
                        {

                            GameRelicPad door = mydoor as GameRelicPad;
                          
                            if (open && door.State == eDoorState.Closed)
                            {

                               
                                door.State = eDoorState.Open;
                                door.BroadcastDoorStatus();
                                StartPlaySound();
                                return;
                            }
                            else
                            {
                               
                                
                                door.State = eDoorState.Closed;
                                door.BroadcastDoorStatus();
                                StartPlaySound();
                                return;
                            }

                        }
                    }
                }
            }
        }

    }
}

  


