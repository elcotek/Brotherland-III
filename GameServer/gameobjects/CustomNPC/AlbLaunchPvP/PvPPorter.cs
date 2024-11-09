/*
 * Elcotek: based on the work of ML10 Porter.
 * 
 * 
 * 
 */

using DOL.AI.Brain;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Quests;

using log4net;

namespace DOL.GS.Scripts
{
    public class AlbLaunchPvPPorter : GameNPC
    {
        public AlbLaunchPvPPorter()
            : base()
        {
            SetOwnBrain(new AlbLaunchPvPPorterBrain());
            Model = 913;
            Name = "Unknown Portal";
            GuildName = "RvR Dungeon";
            Level = 65;
        }
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


    }
}

namespace DOL.AI.Brain
{
    public class AlbLaunchPvPPorterBrain : StandardMobBrain
    {
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
        public AlbLaunchPvPPorterBrain()
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

                if (player == null) continue;

                /*
                //Eintritt egal welches level für quest A tough test bei step 5 
                foreach (AbstractQuest quest in player.QuestList)
                {
                    // lock (((ICollection)player.QuestList).SyncRoot)
                    {
                        if (quest != null && quest.Name == "A tough test" && quest.Step == 5)
                        {
                            IsDoingTheQuest = true;
                        }
                        else
                            IsDoingTheQuest = false;
                    }
                }

                if (player.Level < 20 && IsDoingTheQuest == false)
                {
                    Body.Say(player.Name + " Sorry, come back if you are level 20");
                    continue;
                }
                if (player.Level > 25 && IsDoingTheQuest == false)
                {
                    Body.Say(player.Name + " Sorry, your level are to hight for this zone. You must be level 20-25!");
                    continue;
                }
                */

                //if (player.Client.Account.PrivLevel >= (uint)ePrivLevel.GM) continue;
                if ((!Body.IsWithinRadius(player, (short)150))) continue;
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
                                            break;
                                        }
                                    case 2:
                                        {
                                            t.MoveTo(498, 33735, 35608, 16070, 1367);
                                            break;
                                        }
                                    case 3:
                                        {
                                            t.MoveTo(498, 34475, 33163, 15963, 853);
                                            break;
                                        }
                                    case 4:
                                        {
                                            t.MoveTo(498, 32508, 28313, 16034, 3986);
                                            break;
                                        }
                                    case 5:
                                        {
                                            t.MoveTo(498, 31902, 30314, 16029, 3543);
                                            break;
                                        }
                                    case 6:
                                        {
                                            t.MoveTo(498, 29857, 29703, 15959, 3972);
                                            break;
                                        }
                                    case 7:
                                        {
                                            t.MoveTo(498, 31351, 31388, 15999, 2077);
                                            break;
                                        }
                                    case 8:
                                        {
                                            t.MoveTo(498, 31416, 32644, 15999, 4040);
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
                                            break;
                                        }
                                    case 2:
                                        {
                                            t.MoveTo(498, 32696, 36675, 15960, 2196);
                                            break;
                                        }
                                    case 3:
                                        {
                                            t.MoveTo(498, 32617, 33710, 15980, 2118);
                                            break;
                                        }
                                    case 4:
                                        {
                                            t.MoveTo(498, 30310, 32374, 16099, 688);
                                            break;
                                        }
                                    case 5:
                                        {
                                            t.MoveTo(498, 28869, 29375, 16007, 3220);
                                            break;
                                        }
                                    case 6:
                                        {
                                            t.MoveTo(498, 34584, 28827, 16069, 505);
                                            break;
                                        }
                                    case 7:
                                        {
                                            t.MoveTo(498, 33679, 32164, 16040, 765);
                                            break;
                                        }
                                    case 8:
                                        {
                                            t.MoveTo(498, 32321, 32995, 16000, 3615);
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
                                            break;
                                        }
                                    case 2:
                                        {
                                            t.MoveTo(498, 30387, 35743, 16120, 2424);
                                            break;
                                        }
                                    case 3:
                                        {
                                            t.MoveTo(498, 31117, 32344, 15999, 3796);
                                            break;
                                        }
                                    case 4:
                                        {
                                            t.MoveTo(498, 29073, 32547, 15959, 4033);
                                            break;
                                        }
                                    case 5:
                                        {
                                            t.MoveTo(498, 30276, 31607, 16099, 3504);
                                            break;
                                        }
                                    case 6:
                                        {
                                            t.MoveTo(498, 33989, 28589, 16084, 3804);
                                            break;
                                        }
                                    case 7:
                                        {
                                            t.MoveTo(498, 33732, 31950, 16020, 549);
                                            break;
                                        }
                                    case 8:
                                        {
                                            t.MoveTo(498, 33864, 28470, 16093, 28);
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

        private void SendReply(GamePlayer target, string msg)
        {
            target.Client.Out.SendMessage(
                msg,
                eChatType.CT_Say, eChatLoc.CL_PopupWindow);
        }
    }
}