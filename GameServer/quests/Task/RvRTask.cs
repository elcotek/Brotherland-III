using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Quests;
using DOL.Language;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DOL.GS
{
    public class RvRTask
    {
        private readonly static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected static List<GamePlayer> Players = new List<GamePlayer>();
        protected static List<GameStaticItem> Camps = new List<GameStaticItem>();

        private static int m_x;
        private static int m_y;
        private static int m_z;
        private static ushort m_heading;

        private static int X
        { get { return m_x; } set { m_x = value; } }
        private static int Y
        { get { return m_y; } set { m_y = value; } }
        private static int Z
        { get { return m_z; } set { m_z = value; } }

        private static ushort Heading
        { get { return m_heading; } set { m_heading = value; } }

        public static IArea Camp_Area = null;


        protected static void AddCampToList(GameStaticItem item)
        {
            Camps.Add(item);
        }
        protected static void RemovePlayerFromList(GamePlayer player)
        {
            if (Players.Contains(player))
            {
                Players.Remove(player);
            }
        }

        protected static void AddPlayerToList(GamePlayer player)
        {
            if (player.Mission != null && player.Mission.Description.Contains("[RvR Mission]"))
            {
                Players.Add(player);
            }
        }
        public static bool GetPlayerInTaskList(GamePlayer player)
        {

            //log.ErrorFormat("adde Player zur teilnehmerliste: {0}, Realm: {1}", player.Name, player.Realm);
            if (Players.Contains(player))
            {
                return true;
            }
            return false;
        }


        public static void CreateCampArea(GameStaticItem obj)
        {
            Camp_Area = WorldMgr.GetRegion(obj.CurrentRegionID).AddArea(new Area.Circle("The Camp Area", obj.X, obj.Y, obj.Z, 2500));
            Camp_Area.RegisterPlayerEnter(new DOLEventHandler(PlayerEnterCampArea));
            Camp_Area.RegisterPlayerLeave(new DOLEventHandler(PlayerLeaveCampArea));
        }
        public static void RemoveCampArea(GameStaticItem obj)
        {
            Camp_Area.UnRegisterPlayerEnter(new DOLEventHandler(PlayerEnterCampArea));
            Camp_Area.UnRegisterPlayerLeave(new DOLEventHandler(PlayerLeaveCampArea));
            WorldMgr.GetRegion(obj.CurrentRegionID).RemoveArea(Camp_Area);
        }
        protected static void PlayerEnterCampArea(DOLEvent e, object sender, EventArgs args)
        {
            AreaEventArgs aargs = args as AreaEventArgs;
            GamePlayer player = aargs.GameObject as GamePlayer;

            if (player == null)
                return;

            AddPlayerToList(player);

            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "YouEnterCamp"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
        }
        protected static void PlayerLeaveCampArea(DOLEvent e, object sender, EventArgs args)
        {
            AreaEventArgs aargs = args as AreaEventArgs;
            GamePlayer player = aargs.GameObject as GamePlayer;

            if (player == null)
                return;
                      
            if (GetPlayerInTaskList(player) && player.Mission != null && player.Mission.Description.Contains("[RvR Mission]"))
            {
                RemovePlayerFromList(player);
                player.Mission.ExpireMission();

            }
            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "YouLeaveCamp"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
        }

        static string spawnRealmName = "";
        static string destinationName = "";
        public static void SpawnLagerAlb()
        {
            int random = Util.Random(1, 3);

            switch (random)
            {
                case 1:
                    {
                        SpawnObject("Southeast Caer Eraleigh Mine", 100, 592322, 549822, 8160, 422, 163, 3489, -1, eRealm.None);
                        destinationName = "Southeast Caer Eraleigh Mine";
                        break;
                    }
                case 2:
                    {
                        SpawnObject("Northwest Caer Sursbroke Mine", 100, 574682, 590884, 7726, 4061, 163, 3489, -1, eRealm.None);
                        destinationName = "Northwest Caer Sursbroke Mine";
                        break;
                    }
                case 3:
                    {
                        SpawnObject("Northwest Caer Renaris Mine", 100, 623305, 596178, 8088, 1206, 163, 3489, -1, eRealm.None);
                        destinationName = "Northwest Caer Renaris Mine";
                        break;
                    }
            }
        }
        public static void SpawnLagerMid()
        {
            int random = Util.Random(1, 3);

            switch (random)
            {
                case 1:
                    {
                        SpawnObject("Northwest Blendake Faste Mine", 100, 577986, 404978, 8336, 1388, 163, 3489, -1, eRealm.None);
                        destinationName = "Northwest Blendake Faste Mine";
                        break;
                    }
                case 2:
                    {
                        SpawnObject("East Nottmore Faste Mine", 100, 564991, 366821, 8281, 989, 163, 3489, -1, eRealm.None);
                        destinationName = "East Nottmore Faste Mine";
                        break;
                    }
                case 3:
                    {
                        SpawnObject("North Arvakr Faste Mine", 100, 670638, 372900, 7882, 2018, 163, 3489, -1, eRealm.None);
                        destinationName = "North Arvakr Faste Mine";
                        break;
                    }
            }
        }

        public static void SpawnLagerHib()
        {
            int random = Util.Random(1, 3);

            switch (random)
            {
                case 1:
                    {
                        SpawnObject("East Dun nGed Mine", 100, 460362, 583375, 7806, 972, 163, 3489, -1, eRealm.None);
                        destinationName = "East Dun nGed Mine";
                        break;
                    }
                case 2:
                    {
                        SpawnObject("South Dun da Behn Mine", 100, 478928, 616900, 8048, 707, 163, 3489, -1, eRealm.None);
                        destinationName = "South Dun da Behn Mine";
                        break;
                    }
                case 3:
                    {
                        SpawnObject("North Dun Bolg Mine", 100, 457047, 537854, 8357, 2069, 163, 3489, -1, eRealm.None);
                        destinationName = "North Dun Bolg Mine";
                        break;
                    }
            }
        }
      


        /// <summary>
        /// Spawne Camp pro Realm 3
        /// </summary>
        public static void SpawnAllMines()
        {
           int random = Util.Random(1, 3);

            switch (random)
            {
                case 1:
                    {
                        SpawnLagerAlb();
                        spawnRealmName = "Albion";
                        break;
                    }
                case 2:
                    {
                        SpawnLagerMid();
                        spawnRealmName = "Midgard";
                        break;
                    }
                case 3:
                    {
                        SpawnLagerHib();
                        spawnRealmName = "Hibernia";
                        break;
                    }
            }
        }

        public static void SendMissionInfo(GamePlayer player)
        {
            string message = LanguageMgr.GetTranslation(player.Client.Account.Language, "RVRTaskInfo", spawnRealmName, destinationName);
            RvRTaskMission.MissionInfo = message;
        }

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
        protected static GameStaticItem SpawnObject(string name, int health, int x, int y, int Z, ushort heading, ushort region, ushort model, int respawnInterval, eRealm realm)
        {
            GameStaticItem add = null;
            
            try
            {
                if (string.IsNullOrEmpty(name) == false)
                {
                    add = new GameStaticItem()
                    {
                        CurrentRegionID = region,
                        Heading = Convert.ToUInt16(heading),
                        Realm = realm,
                        X = x,
                        Y = y,
                        Z = Z,
                        Name = name,
                        Model = model,
                        MaxHealth = health,
                        Health = health,
                        Level = 50,
                        RespawnInterval = respawnInterval,


                    };
                    add.AddToWorld();
                    //log.ErrorFormat("adde item name: {0}  realm: {1}", add.Name, add.Realm);
                    new DespawnTimers2(add, add, 30 * 60 * 1000);//remove bei 50 Minuten
                    if (add.Name == "Tent")
                    {
                        CreateCampArea(add);
                    }
                }
            }

            catch
            {
                log.Warn(String.Format("Unable to create Object"));
            }
            return add;
        }

        protected static GameObject RemainingObjects;


        /// <summary>
        /// Provides a timer to remove an NPC from the world after some
        /// time has passed.
        /// </summary>
        protected class DespawnTimers2 : GameTimer
        {
            private GameObject m_object;

            /// <summary>
            /// Constructs a new DespawnTimer.
            /// </summary>
            /// <param name="timerOwner">The owner of this timer.</param>
            /// <param name="npc">The GameNPC to despawn when the time is up.</param>
            /// <param name="delay">The time after which the add is supposed to despawn.</param>
            public DespawnTimers2(GameObject timerOwner, GameObject obj, int delay)
                : base(timerOwner.CurrentRegion.TimeManager)
            {
                m_object = obj;
                RemainingObjects = m_object;
                Start(delay);
            }
            /// <summary>
            /// Called on every timer tick.
            /// </summary>
            protected override void OnTick()
            {
                if (m_object != null)
                {
                    //Area vom Camp entfernen
                    if (m_object is GameStaticItem && m_object.Name == "Tent")
                    {
                        RemoveCampArea(m_object as GameStaticItem);
                    }
                    if (m_object is GameStaticItem && m_object.Realm == eRealm.None)
                    {
                        AddCampToList(m_object as GameStaticItem);
                                                                       
                        for (int i = 0; i < Camps.Count; i++)
                        {
                            //log.ErrorFormat("camps found = {0}", Camps[i].Name);
                            if (i == 0)
                            {
                                //log.ErrorFormat("item Realm: {0}, item Name: {1}, Count: {2} ", Camps[i].Realm, Camps[i].Name, i);
                                EndMission();
                                break;
                            }
                        }
                    }

                    if (m_object.Name != "Symbol" && m_object.Name != "Tent"  && m_object.Realm != eRealm.None)
                    {
                        //log.ErrorFormat("HAS REalm!! item Realm: {0}, item Name: {1}", m_object.Realm, m_object.Name);
                        EndMission();
                    }

                    m_object.Delete();
                    m_object = null;
                }
            }
        }

      

        static void EndMission()
        {
           MissionEndTimer();
        }
       
        /// <summary>
		/// Hold events for focus spells
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected static void FocusAction(DOLEvent e, object sender, EventArgs args)
        {
            GameLiving living = sender as GameLiving;
            if (living == null) return;

            GamePlayer player = living as GamePlayer;

            if (e == GameLivingEvent.Moving || e == GameLivingEvent.CastStarting
                || e == GameLivingEvent.Dying || e == GameLivingEvent.AttackFinished || e == GameLivingEvent.AttackedByEnemy)
            {
                player.CaptureTimer.Stop();
                player.Out.SendCloseTimerWindow();

                if (player is GamePlayer)
                {
                    GameEventMgr.RemoveHandler(player, GameLivingEvent.Moving, new DOLEventHandler(FocusAction));
                    GameEventMgr.RemoveHandler(player, GameLivingEvent.AttackFinished, new DOLEventHandler(FocusAction));
                    GameEventMgr.RemoveHandler(player, GameLivingEvent.CastStarting, new DOLEventHandler(FocusAction));
                    GameEventMgr.RemoveHandler(player, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(FocusAction));
                    GameEventMgr.RemoveHandler(player, GameLivingEvent.Dying, new DOLEventHandler(FocusAction));
                    (player as GamePlayer).Out.SendSoundEffect((ushort)601, 0, 0, 0, 0, 0);
                }
            }
        }

        /// <summary>
        /// Caüture Timer
        /// </summary>
        /// <param name="player"></param>
        /// <param name="obj"></param>
        public static void StartCapture(GamePlayer player, GameObject obj)
        {
            int duration = 30;
            if (obj.Realm != eRealm.None)
            {
                duration /= 2;
            }
           
            if (player.IsStealthed)
            {
                //you cant captute that in stealth
                return;
            }
            GameEventMgr.AddHandler(player, GameLivingEvent.Moving, new DOLEventHandler(FocusAction));
            GameEventMgr.AddHandler(player, GameLivingEvent.AttackFinished, new DOLEventHandler(FocusAction));
            GameEventMgr.AddHandler(player, GameLivingEvent.CastStarting, new DOLEventHandler(FocusAction));
            GameEventMgr.AddHandler(player, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(FocusAction));
            GameEventMgr.AddHandler(player, GameLivingEvent.Dying, new DOLEventHandler(FocusAction));

            if (player.IsStealthed)
            {
                player.Stealth(false);
            }

            player.Emote(eEmote.Induct);
            player.Out.SendTimerWindow("Create a Camp..." + player.TargetObject.Name, duration);
            player.CaptureTimer = new RegionTimer(player);
            player.CaptureTimer.Callback = new RegionTimerCallback(Proceed);
            player.CaptureTimer.Properties.setProperty("Capture_player", player);
            player.CaptureTimer.Properties.setProperty("Capture_target", obj);
            player.CaptureTimer.Start(duration * 1000);
        }


        protected static int Proceed(RegionTimer timer)
        {
            GamePlayer player = (GamePlayer)timer.Properties.getProperty<object>("Capture_player", null);
            GameStaticItem obj = (GameStaticItem)timer.Properties.getProperty<object>("Capture_target", null);

            if (player == null || obj == null)
            {
                if (log.IsWarnEnabled)
                    log.Warn("There was a problem getting back the target or player in Capure Timer!");
                return 0;
            }

            if (!player.IsAlive || player.IsMoving || player.IsCasting || player.InCombat || player.IsStealthed)
            {
                //log.ErrorFormat("You fail to get the Tent {0}", obj.Name);
                player.CaptureTimer.Stop();
                player.Out.SendCloseTimerWindow();
            }
           
            if (player.IsStealthed)
            {
                player.Stealth(false);
            }        

            if (obj != null)
            {
                obj.Realm = player.Realm;
                AddCampToList(obj as GameStaticItem);
                // obj.Realm = player.Realm;
                SpawnObject(obj.Name, 100, obj.X, obj.Y, obj.Z, obj.Heading, obj.CurrentRegionID, obj.Model, -1, player.Realm);

            }
           
            SendCaptureMessageToAllPlayer(player, obj.Name);
            SpawnObject("Tent", 100, obj.X, obj.Y, obj.Z, obj.Heading, 163, 4237, -1, player.Realm);

            /*
            mittleres banner alb 4122
            mittleres banner hib 4123
            mittleres banner mid 4124
            */
            switch (player.Realm)
            {
                case eRealm.Albion:
                    {
                        SpawnObject("Symbol", 150, obj.X - 150, obj.Y - 100, obj.Z, obj.Heading -= 100, 163, 4122, -1, eRealm.Albion);
                        break;
                    }
                case eRealm.Midgard:
                    {
                        SpawnObject("Symbol", 150, obj.X - 150, obj.Y - 100, obj.Z, obj.Heading -= 100, 163, 4124, -1, eRealm.Midgard);
                        break;
                    }
                case eRealm.Hibernia:
                    {
                        SpawnObject("Symbol", 150, obj.X - 150, obj.Y - 100, obj.Z, obj.Heading -= 100, 163, 4123, -1, eRealm.Hibernia);
                        break;
                    }
            }

            obj.Delete();
            obj = null;

            player.CaptureTimer.Stop();
            player.Out.SendCloseTimerWindow();

            return 0;
        }


        private static void SendCaptureMessageToAllPlayer(GamePlayer player, string name)
        {
            string realmName = "";

            switch (player.Realm)
            {
                case eRealm.Albion:
                    {
                        realmName = "Albion";
                        break;
                    }

                case eRealm.Midgard:
                    {
                        realmName = "Midgard";
                        break;
                    }
                case eRealm.Hibernia:
                    {
                        realmName = "Hibernia";
                        break;
                    }
            }

            foreach (GameClient c in WorldMgr.GetAllPlayingClients())
            {
                if (c != null && c.Player != null && c.IsPlaying)
                {

                    if (c.Player != player && c.Player.Mission != null && c.Player.Mission.Description.Contains("[RvR Mission]"))
                    {                                                                     
                        c.Player.Out.SendMessage(LanguageMgr.GetTranslation(c.Player.Client.Account.Language, "CampCaptured", player.Name, realmName, name), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    }
                    else
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "YouHasCaputured", name), eChatType.CT_ScreenCenterSmaller, eChatLoc.CL_SystemWindow);
                }
            }
        }
        private static void GetRPS(GamePlayer player, long amount)
        {
            long RewardMoney = 20 * 100 * 100;
            try
            {
                player.GainRealmPoints(amount, false);
                player.ReceiveMoney(player, 20 * 100 * 100);
                player.AddMoney(RewardMoney, "You recieve {0}.");
                player.Out.SendUpdatePlayer();
            }
            catch
            {

            }
        }
       

        /// <summary>
        /// Task Start Mission Timer
        /// </summary>
        /// <param name="player"></param>
        /// <param name="obj"></param>
        public static void StartMissionTimer() //in = GameServer/private int RVRTaskMissionTimerInterval = 5;//5 min
        {
            Players.Clear();
            Camps.Clear();

            //Verbleibende Objecte entfernen
            if (RemainingObjects != null)
            {
                if (RemainingObjects is GameStaticItem && (RemainingObjects as GameStaticItem).Name == "Tent")
                {
                    RemoveCampArea(RemainingObjects as GameStaticItem);
                }

                RemainingObjects.Delete();
                RemainingObjects = null;
            }
            //Spawn the Mine
            SpawnAllMines();

            foreach (GameClient c in WorldMgr.GetAllPlayingClients())
            {
                if (c != null && c.Player != null && c.IsPlaying)
                {
                    if (c.Player.Level == 50 && WorldMgr.GetAllPlayingClientsCount() >= 10)
                    {
                        if (c.Player != null && c.Player.Mission == null)
                        {
                            //Mission info text for all Players
                            SendMissionInfo(c.Player);
                            c.Player.Mission = new RvRTaskMission(c.Player);
                            c.Player.Out.SendMessage(LanguageMgr.GetTranslation(c.Player.Client.Account.Language, "RoundStarts"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        }
                    }

                }
            }
        }

        public static void MissionEndTimer()
        {
            long lowReward = 50;

            //Verbleibende Objecte entfernen
            if (RemainingObjects != null)
            {
                if (RemainingObjects is GameStaticItem && (RemainingObjects as GameStaticItem).Name == "Tent")
                {
                    RemoveCampArea(RemainingObjects as GameStaticItem);
                }

                RemainingObjects.Delete();
                RemainingObjects = null;
            }

            //Ermittle die Beteiligten am Camp
            foreach (GamePlayer player in Players)
            {
                              
                if (player != null)
                {
                    

                    if (player.Mission != null && player.Mission.Description.Contains("[RvR Mission]"))
                    {
                         if (player.IsCrafting == false && player.CaptureTimer != null)
                        {
                            player.Out.SendCloseTimerWindow();
                            player.CaptureTimer.Stop();
                        }
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "RoundOver"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    }

                    foreach (GameStaticItem item in Camps)
                    {
                        if (item != null)
                        {
                            switch (item.Realm)
                            {
                                case eRealm.Albion:
                                    {
                                        if (player.Realm == eRealm.Albion)
                                        {
                                            //log.ErrorFormat("Albion Player beteiligt: {0}", player.Name);
                                            if (player.Mission != null && player.Mission.Description.Contains("[RvR Mission]"))
                                                player.Mission.FinishMission();
                                        }
                                       
                                        else if (player.Mission != null && player.Mission.Description.Contains("[RvR Mission]"))
                                        {
                                            //log.ErrorFormat("Trost Preis RPS für: {0}", player.Name);
                                            GetRPS(player, lowReward);
                                            player.Mission.ExpireMission();
                                        }
                                        break;
                                    }
                                case eRealm.Midgard:
                                    {
                                        if (player.Realm == eRealm.Midgard)
                                        {
                                           
                                            if (player.Mission != null && player.Mission.Description.Contains("[RvR Mission]"))
                                                player.Mission.FinishMission();
                                        }

                                        else if (player.Mission != null && player.Mission.Description.Contains("[RvR Mission]"))
                                        {
                                           // log.ErrorFormat("Trost Preis RPS für: {0}", player.Name);
                                            GetRPS(player, lowReward);
                                            player.Mission.ExpireMission();
                                        }
                                        break;
                                    }
                                case eRealm.Hibernia:
                                    {
                                        if (player.Realm == eRealm.Hibernia)
                                        {
                                            
                                            if (player.Mission != null && player.Mission.Description.Contains("[RvR Mission]"))
                                                player.Mission.FinishMission();
                                        }

                                        else if (player.Mission != null && player.Mission.Description.Contains("[RvR Mission]"))
                                        {
                                            GetRPS(player, lowReward);
                                            player.Mission.ExpireMission();
                                        }
                                        break;
                                    }
                              
                                default: break;
                            }
                        }
                    }
                }
            }
            
            //Lösche Tasks der nichtbeteiligten an der Mission
            foreach (GameClient c in WorldMgr.GetAllPlayingClients())
            {
                if (c != null && c.Player != null && c.IsPlaying)
                {
                    if (c.Player.Mission != null && c.Player.Mission.Description.Contains("[RvR Mission]"))
                    {
                        if (c.Player.IsCrafting == false && c.Player.CaptureTimer != null)
                        {
                            c.Player.Out.SendCloseTimerWindow();
                            c.Player.CaptureTimer.Stop();
                        }
                        c.Player.Out.SendMessage(LanguageMgr.GetTranslation(c.Player.Client.Account.Language, "RoundOver"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        c.Player.Mission.ExpireMission();
                    }
                }
            }
        }
    }
}