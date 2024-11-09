using DOL.AI.Brain;
using DOL.GS;
using DOL.GS.PacketHandler;
using System;

namespace DOL.GS.Scripts
{
    public class OutOfNowhereSpawn : GameNPC
    {
        public OutOfNowhereSpawn()
            : base()
        {
            SetOwnBrain(new OutOfNowhereSpawnBrain());
        }
     }
}

namespace DOL.AI.Brain
{
   
    public class OutOfNowhereSpawnBrain : StandardMobBrain
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public OutOfNowhereSpawnBrain()
            : base()
        {
            AggroLevel = 0;
            AggroRange = 0;
            ThinkInterval = 1500;
        }

        public static bool SpawnAlb = false;
        public static bool SpawnMid = false;
        public static bool SpawnHib= false;


        public override void Think()
        {

            foreach (GamePlayer player in Body.GetPlayersInRadius(4000))
            {
                GamePlayer t = (GamePlayer)player;

                if (player == null) continue;



                if (player.GetSpecificDataquestByID(3456) || player.GetSpecificDataquestByID(3457) || player.GetSpecificDataquestByID(3458))
                {
                    if ((Body.IsWithinRadius(player, (short)3000)))
                    {
                        player.Out.SendMessage("You are approaching the place where the hikers were last seen!", eChatType.CT_Send, eChatLoc.CL_ChatWindow);

                        if (player.IsRiding)
                        {
                            player.DismountSteed(true);
                        }

                        if (player.IsOnHorse)
                        {
                            player.IsOnHorse = false;
                        }
                    }
                    //else
                    //{
                    //    player.Out.SendMessage("You are leaving the place where the hikers were last seen!", eChatType.CT_Send, eChatLoc.CL_ChatWindow);
                   // }
                }
               

                //if (player.Client.Account.PrivLevel >= (uint)ePrivLevel.GM) continue;
                if ((!Body.IsWithinRadius(player, (short)600))) continue;
                {
                    switch (t.Realm)
                    {
                        case eRealm.Albion:
                            {
                                if (player.GetSpecificDataquestByID(3456)) //Quest name: Out of Nowhere
                                {
                                    if (SpawnAlb == false)
                                    {
                                        player.Out.SendMessage("You fall unexpectedly into a trap, you have to fear for your life. Defend yourself or you will die!", eChatType.CT_Send, eChatLoc.CL_ChatWindow);
                                        //the Boss Vaico the slyer
                                        QuestSpawnAdd(3025, Util.Random(47, 51), Body.X, Body.Y, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);

                                        //skograsra woodsman
                                        QuestSpawnAdd(3026, Util.Random(45, 48), Body.X, Body.Y + 200, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);
                                        QuestSpawnAdd(3027, Util.Random(45, 48), Body.X, Body.Y - 200, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);

                                        //skograsra greenhand
                                        QuestSpawnAdd(3028, Util.Random(46, 49), Body.X, Body.Y + 100, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);
                                        QuestSpawnAdd(3028, Util.Random(46, 49), Body.X - 100, Body.Y, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);

                                        //fomorian stylore
                                        QuestSpawnAdd(3029, Util.Random(45, 48), Body.X + 200, Body.Y, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);
                                        QuestSpawnAdd(3029, Util.Random(45, 48), Body.X - 200, Body.Y, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);
                                        //vanvettig djur
                                        QuestSpawnAdd(3030, Util.Random(45, 48), Body.X + 300, Body.Y + 300, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);
                                        QuestSpawnAdd(3030, Util.Random(45, 48), Body.X - 300, Body.Y - 300, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);
                                        SpawnAlb = true;
                                    }
                                }
                                break;
                            }

                        case eRealm.Midgard:
                            {
                                if (player.GetSpecificDataquestByID(3457)) //Quest name: Out of Nowhere
                                {
                                    if (SpawnMid == false)
                                    {
                                        player.Out.SendMessage("You fall unexpectedly into a trap, you have to fear for your life. Defend yourself or you will die!", eChatType.CT_Send, eChatLoc.CL_ChatWindow);
                                        //the Boss Skoran the Butcher
                                        QuestSpawnAdd(3024, Util.Random(47, 51), Body.X, Body.Y, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);

                                        //skograsra woodsman
                                        QuestSpawnAdd(3021, Util.Random(45, 48), Body.X, Body.Y + 200, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);
                                        QuestSpawnAdd(3021, Util.Random(45, 48), Body.X, Body.Y - 200, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);

                                        //skograsra greenhand
                                        QuestSpawnAdd(3022, Util.Random(46, 49), Body.X, Body.Y + 100, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);
                                        QuestSpawnAdd(3022, Util.Random(46, 49), Body.X - 100, Body.Y, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);

                                        //fomorian stylore
                                        QuestSpawnAdd(3020, Util.Random(45, 48), Body.X + 200, Body.Y, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);
                                        QuestSpawnAdd(3020, Util.Random(45, 48), Body.X - 200, Body.Y, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);
                                        //vanvettig djur
                                        QuestSpawnAdd(3023, Util.Random(45, 48), Body.X + 300, Body.Y + 300, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);
                                        QuestSpawnAdd(3023, Util.Random(45, 48), Body.X - 300, Body.Y - 300, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);
                                        SpawnMid = true;
                                    }
                                }
                                break;
                            }

                        case eRealm.Hibernia:
                            {
                                if (player.GetSpecificDataquestByID(3458)) //Quest name: Out of Nowhere
                                {
                                    if (SpawnHib == false)
                                    {
                                        player.Out.SendMessage("You fall unexpectedly into a trap, you have to fear for your life. Defend yourself or you will die!", eChatType.CT_Send, eChatLoc.CL_ChatWindow);
                                        //the Boss Jester the Dirty
                                        QuestSpawnAdd(3016, Util.Random(47, 51), Body.X, Body.Y, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);

                                        //fomorian myder
                                        QuestSpawnAdd(3017, Util.Random(45, 48), Body.X, Body.Y + 200, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);
                                        QuestSpawnAdd(3017, Util.Random(45, 48), Body.X, Body.Y - 200, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);

                                        //fomorian anistruct
                                        QuestSpawnAdd(3018, Util.Random(46, 49), Body.X, Body.Y + 100, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);
                                        QuestSpawnAdd(3018, Util.Random(46, 49), Body.X - 100, Body.Y, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);

                                        //fomorian stylore
                                        QuestSpawnAdd(3020, Util.Random(45, 48), Body.X + 200, Body.Y, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);
                                        QuestSpawnAdd(3020, Util.Random(45, 48), Body.X - 200, Body.Y, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);
                                        //aggressive tumbler
                                        QuestSpawnAdd(3019, Util.Random(45, 48), Body.X + 300, Body.Y + 300, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);
                                        QuestSpawnAdd(3019, Util.Random(45, 48), Body.X - 300, Body.Y - 300, Body.Z, Body.Heading, Body.CurrentRegionID, 0, -1);
                                        SpawnHib = true;
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

        private INpcTemplate m_addOrkTemplate;

        /// <summary>
        /// Create an add from the specified template.
        /// </summary>
        /// <param name="templateID"></param>
        /// <param name="level"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="uptime"></param>
        /// <returns></returns>
        ///  SpawnAdd(dba.NPCTemplateID, Util.Random(65, 68), dba.X, dba.Y, dba.Z, dba.Heading, dba.Region);
        protected GameNPC QuestSpawnAdd(int templateID, int level, int x, int y, int Z, ushort heading, ushort region, int roamingrange, int respawnInterval)
        {
            GameNPC add = null;

            try
            {
                if (m_addOrkTemplate == null || m_addOrkTemplate.TemplateId != templateID)
                {
                    m_addOrkTemplate = NpcTemplateMgr.GetTemplate(templateID);
                }

                // Create add from template.


                if (m_addOrkTemplate != null)
                {
                    add = new GameNPC(m_addOrkTemplate)
                    {
                        CurrentRegionID = region,
                        Heading = Convert.ToUInt16(heading),
                        Realm = 0,
                        X = x,
                        Y = y,
                        Z = Z,
                        CurrentSpeed = 0,
                        Level = (byte)level,
                        RespawnInterval = respawnInterval,
                        RoamingRange = roamingrange
                    };
                    add.AddToWorld();
                    new DespawnTimers2(add, add, 80 * 1000);
                }
            }

            catch
            {
                log.Warn(String.Format("Unable to get template for {0}"));
            }
            return add;
        }
        /// <summary>
        /// Provides a timer to remove an NPC from the world after some
        /// time has passed.
        /// </summary>
        protected class DespawnTimers2 : GameTimer
        {
            private GameNPC m_npc;

            /// <summary>
            /// Constructs a new DespawnTimer.
            /// </summary>
            /// <param name="timerOwner">The owner of this timer.</param>
            /// <param name="npc">The GameNPC to despawn when the time is up.</param>
            /// <param name="delay">The time after which the add is supposed to despawn.</param>
            public DespawnTimers2(GameObject timerOwner, GameNPC npc, int delay)
                : base(timerOwner.CurrentRegion.TimeManager)
            {
                m_npc = npc;
                Start(delay);
            }
            /// <summary>
            /// Called on every timer tick.
            /// </summary>
            protected override void OnTick()
            {

                if (m_npc != null && m_npc.InCombat == false)
                {

                    m_npc.Delete();
                    m_npc = null;
                    SpawnAlb = false;
                    SpawnMid = false;
                    SpawnHib = false;
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

