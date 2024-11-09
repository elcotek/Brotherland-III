/*
 * Elcotek: based on the work of ML10 Porter.
 * 
 * 
 * 
 */

using DOL.AI.Brain;
using DOL.GS;
using DOL.GS.PacketHandler;

using log4net;

namespace DOL.GS
{
    public class AlbLaunchTeleporterRein : GameNPC
    {
        public AlbLaunchTeleporterRein()
            : base()
        {
            SetOwnBrain(new AlbLaunchTeleporterPorterReinBrain());

        }
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


    }
}

namespace DOL.AI.Brain
{
    public class AlbLaunchTeleporterPorterReinBrain : StandardMobBrain
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Holds the the Out of Region Flag of this object
        /// </summary>
        protected bool m_isDoingQuest = false;

        /// <summary>
        /// Gets or Sets the out of Region Flag
        /// </summary>
        public virtual bool IsDoingTheQuest
        {
            get { return m_isDoingQuest; }
            set { m_isDoingQuest = value; }
        }

        /// <summary>
        /// The Teleporter Brain
        /// </summary>
        public AlbLaunchTeleporterPorterReinBrain()
            : base()
        {
            AggroLevel = 0;
            AggroRange = 0;
            ThinkInterval = 1300;
        }
        public override void Think()
        {


            foreach (GamePlayer player in Body.GetPlayersInRadius(600))
            {
                GamePlayer t = (GamePlayer)player;

                if (player == null || player.IsAlive == false) continue;

                bool found = false;

                foreach (GameNPC npc in Body.GetNPCsInRadius(300))
                {
                    if (npc != null && npc.Name == "Unknown Portal")
                    {
                        //log.Error("mob found!");
                        found = true;
                    }
                }


                //if (player.Client.Account.PrivLevel >= (uint)ePrivLevel.GM) continue;
                if ((!Body.IsWithinRadius(player, (short)150))) continue;
                {
                    if (found)
                    {
                        int Randomport = Util.Random(1, 8);
                        switch (t.Realm)
                        {
                            case eRealm.Albion:
                                {
                                    switch (Randomport)
                                    {
                                        case 1:
                                            {
                                                t.MoveTo(498, 31771, 37346, 16024, 2079);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }
                                        case 2:
                                            {
                                                t.MoveTo(498, 33735, 35608, 16070, 1367);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }
                                        case 3:
                                            {
                                                t.MoveTo(498, 34475, 33163, 15963, 853);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }
                                        case 4:
                                            {
                                                t.MoveTo(498, 32508, 28313, 16034, 3986);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }
                                        case 5:
                                            {
                                                t.MoveTo(498, 31902, 30314, 16029, 3543);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }
                                        case 6:
                                            {
                                                t.MoveTo(498, 29857, 29703, 15959, 3972);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }
                                        case 7:
                                            {
                                                t.MoveTo(498, 31351, 31388, 15999, 2077);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }
                                        case 8:
                                            {
                                                t.MoveTo(498, 31416, 32644, 15999, 4040);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }

                                    }
                                    break;
                                }
                            case eRealm.Midgard:
                                {
                                    switch (Randomport)
                                    {
                                        case 1:
                                            {
                                                t.MoveTo(498, 32231, 37549, 16021, 1917);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }
                                        case 2:
                                            {
                                                t.MoveTo(498, 32696, 36675, 15960, 2196);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }
                                        case 3:
                                            {
                                                t.MoveTo(498, 32617, 33710, 15980, 2118);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }
                                        case 4:
                                            {
                                                t.MoveTo(498, 30310, 32374, 16099, 688);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }
                                        case 5:
                                            {
                                                t.MoveTo(498, 28869, 29375, 16007, 3220);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }
                                        case 6:
                                            {
                                                t.MoveTo(498, 34584, 28827, 16069, 505);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }
                                        case 7:
                                            {
                                                t.MoveTo(498, 33679, 32164, 16040, 765);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }
                                        case 8:
                                            {
                                                t.MoveTo(498, 32321, 32995, 16000, 3615);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }

                                    }
                                    break;
                                }
                            case eRealm.Hibernia:
                                {
                                    switch (Randomport)
                                    {
                                        case 1:
                                            {
                                                t.MoveTo(498, 31939, 37309, 16024, 2692);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }
                                        case 2:
                                            {
                                                t.MoveTo(498, 30387, 35743, 16120, 2424);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }
                                        case 3:
                                            {
                                                t.MoveTo(498, 31117, 32344, 15999, 3796);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }
                                        case 4:
                                            {
                                                t.MoveTo(498, 29073, 32547, 15959, 4033);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }
                                        case 5:
                                            {
                                                t.MoveTo(498, 30276, 31607, 16099, 3504);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }
                                        case 6:
                                            {
                                                t.MoveTo(498, 33989, 28589, 16084, 3804);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }
                                        case 7:
                                            {
                                                t.MoveTo(498, 33732, 31950, 16020, 549);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }
                                        case 8:
                                            {
                                                t.MoveTo(498, 33864, 28470, 16093, 28);
                                                t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                                                break;
                                            }

                                    }
                                    break;
                                }
                            default: break;
                        }
                        base.Think();
                    }
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