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
using DOL.Config;
using DOL.Database;
using DOL.Database.Attributes;
using DOL.Events;
using DOL.GS.Behaviour;
using DOL.GS.DatabaseUpdate;
using DOL.GS.Housing;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.PlayerTitles;
using DOL.GS.Quests;
using DOL.GS.ServerProperties;
using DOL.GS.ServerRules;
using DOL.Language;
using DOL.Mail;
using DOL.Network;
using log4net;
using log4net.Config;
using log4net.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;


namespace DOL.GS
{
    /// <summary>
    /// Class encapsulates all game server functionality
    /// </summary>
    public class GameServer : BaseServer
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        #region Variables

        /// <summary>
        /// Maximum UDP buffer size
        /// </summary>
        protected const int MAX_UDPBUF = 4096;

        /// <summary>
        /// Minute conversion from milliseconds
        /// </summary>
        protected const int MINUTE_CONV = 60000;

        /// <summary>
        /// The instance!
        /// </summary>
        protected static GameServer m_instance;

        /// <summary>
        /// The textwrite for log operations
        /// </summary>
        protected ILog m_cheatLog;

        /// <summary>
        /// Database instance
        /// </summary>
        protected IObjectDatabase m_database;

        /// <summary>
        /// The textwrite for log operations
        /// </summary>
        protected ILog m_gmLog;

        /// <summary>
        /// The textwrite for log operations
        /// </summary>
        protected ILog m_inventoryLog;

        /// <summary>
        /// Holds instance of current server rules
        /// </summary>
        protected IServerRules m_serverRules;

        /// <summary>
        /// Holds the startSystemTick when server is up.
        /// </summary>
        protected long m_startTick;

        /// <summary>
        /// Game server status variable
        /// </summary>
        protected eGameServerStatus m_status;

        /// <summary>
        /// World save timer
        /// </summary>
        protected Timer m_timer;
        protected Timer m_timer1;
        protected Timer m_timer2;
        /// <summary>
        /// Receive buffer for UDP
        /// </summary>
        protected byte[] m_udpBuf;

        /// <summary>
        // Socket that sends UDP packets
        /// </summary>
        protected Socket m_udpSocket;

        /// <summary>
        /// A general logger for the server
        /// </summary>
        public ILog Logger
        {
            get { return log; }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the instance
        /// </summary>
        public static GameServer Instance
        {
            get { return m_instance; }
        }

        /// <summary>
		/// Gets the server Scheduler
		/// </summary>
		public Scheduler.SimpleScheduler Scheduler { get; protected set; }

        /// <summary>
		/// Gets the server WorldManager
		/// </summary>
		public WorldManager WorldManager { get; protected set; }

        /// <summary>
        /// Retrieves the server configuration
        /// </summary>
        public new virtual GameServerConfiguration Configuration
        {
            get { return (GameServerConfiguration)_config; }
        }

        /// <summary>
        /// Gets the server status
        /// </summary>
        public eGameServerStatus ServerStatus
        {
            get { return m_status; }
        }

        /// <summary>
        /// Gets the server PlayerManager
        /// </summary>
        public PlayerManager PlayerManager { get; protected set; }

        /// <summary>
        /// Gets the server NpcManager
        /// </summary>
        public NpcManager NpcManager { get; protected set; }

        /// <summary>
        /// Gets the current rules used by server
        /// </summary>
        public static IServerRules ServerRules
        {
            get
            {
                if (Instance.m_serverRules == null)
                {
                    Instance.m_serverRules = ScriptMgr.CreateServerRules(Instance.Configuration.ServerType);
                    if (Instance.m_serverRules == null && log.IsErrorEnabled)
                    {
                        log.Error(
                            "Something errored in created new server rules.  This is a test to see if this is what is causing the weird keep guard brain bug");
                    }
                }
                return Instance.m_serverRules;
            }
        }

        /// <summary>
        /// Gets the database instance
        /// </summary>
        public static IObjectDatabase Database
        {
            get { return Instance.m_database; }
        }

        /// <summary>
		/// Gets this Instance's Database
		/// </summary>
		public IObjectDatabase IDatabase
        {
            get { return m_database; }
        }

        /// <summary>
        /// Gets or sets the world save interval
        /// </summary>
        public int SaveInterval
        {
            get { return Configuration.SaveInterval; }
            set
            {
                Configuration.SaveInterval = value;
                if (m_timer != null)
                    m_timer.Change(value * MINUTE_CONV, Timeout.Infinite);
            }
        }


        /// <summary>
        /// Gets or sets the world Timer interval
        /// </summary>
        public int WorldTimerInterval
        {
            get { return Configuration.WorldTimerInterval; }
            set
            {
                Configuration.WorldTimerInterval = value;
                if (m_timer1 != null)
                    m_timer1.Change(value * MINUTE_CONV, Timeout.Infinite);
            }
        }


        /// <summary>
        /// True if the server is listening
        /// </summary>
        public bool IsRunning
        {
            get { return _listen != null; }
        }

        /// <summary>
		/// Gets the number of millisecounds elapsed since the GameServer started.
		/// </summary>
		public long TickCount
        {
            get { return GameTimer.GetTickCount() - m_startTick; }
        }
        #endregion

        #region Initialization

        /// <summary>
        /// Creates the gameserver instance
        /// </summary>
        /// <param name="config"></param>
        public static void CreateInstance(GameServerConfiguration config)
        {
            //Only one intance
            if (Instance != null)
                return;

            //Try to find the log.config file, if it doesn't exist
            //we create it
            var logConfig = new FileInfo(config.LogConfigFile);
            if (!logConfig.Exists)
            {
                ResourceUtil.ExtractResource(logConfig.Name, logConfig.FullName);
            }

            //Configure and watch the config file
            XmlConfigurator.ConfigureAndWatch(logConfig);

            //Create the instance
            m_instance = new GameServer(config);
        }


        #endregion

        #region UDP

        /// <summary>
        /// Holds udp receive callback delegate
        /// </summary>
        protected readonly AsyncCallback m_udpReceiveCallback;

        /// <summary>
        /// Holds the async UDP send callback
        /// </summary>
        protected readonly AsyncCallback m_udpSendCallback;

        /// <summary>
        /// Gets the UDP Socket of this server instance
        /// </summary>
        protected Socket UDPSocket
        {
            get { return m_udpSocket; }
        }

        /// <summary>
        /// Gets the UDP buffer of this server instance
        /// </summary>
        protected byte[] UDPBuffer
        {
            get { return m_udpBuf; }
        }

        /// <summary>
        /// Starts the udp listening
        /// </summary>
        /// <returns>true if successfull</returns>
        protected bool StartUDP()
        {
            bool ret = true;
            try
            {
                // Open our udp socket
                m_udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                m_udpSocket.Bind(new IPEndPoint(Configuration.UDPIP, Configuration.UDPPort));

                ret = BeginReceiveUDP(m_udpSocket, this);
            }
            catch (Exception e)
            {
                if (log.IsErrorEnabled)
                    log.Error("StartUDP", e);
                ret = false;
            }

            return ret;
        }

        /// <summary>
        /// UDP event handler. Called when a UDP packet is waiting to be read
        /// </summary>
        /// <param name="ar"></param>
        protected void RecvFromCallback(IAsyncResult ar)
        {
            if (m_status != eGameServerStatus.GSS_Open || ar == null) return;

            GameServer server = (GameServer)ar.AsyncState;
            Socket s = server.UDPSocket;

            if (s == null) return;

            EndPoint tempRemoteEP = new IPEndPoint(IPAddress.Any, 0);
            bool receiving = false;

            try
            {
                int read = s.EndReceiveFrom(ar, ref tempRemoteEP);
                if (read == 0)
                {
                    log.Debug("UDP received bytes = 0");
                    return;
                }

                int pakCheck = (server.UDPBuffer[read - 2] << 8) | server.UDPBuffer[read - 1];
                int calcCheck = PacketProcessor.CalculateChecksum(server.UDPBuffer, 0, read - 2);

                if (calcCheck != pakCheck)
                {
                    if (log.IsWarnEnabled)
                        log.WarnFormat("Bad UDP packet checksum (packet:0x{0:X4} calculated:0x{1:X4}) -> ignored", pakCheck, calcCheck);
                    return;
                }

                var sender = (IPEndPoint)tempRemoteEP;
                var pakin = new GSPacketIn(read - GSPacketIn.HDR_SIZE);
                pakin.Load(server.UDPBuffer, 0, read);

                BeginReceiveUDP(s, server);
                receiving = true;

                GameClient client = WorldMgr.GetClientFromID(pakin.SessionID);
                if (client != null)
                {
                    if (client.UdpEndPoint == null)
                    {
                        client.UdpEndPoint = sender;
                        client.UdpConfirm = false;
                    }
                    if (client.UdpEndPoint.Equals(sender))
                    {
                        client.PacketProcessor.HandlePacket(pakin);
                    }
                }
                else if (log.IsErrorEnabled)
                {
                    log.Error($"Got an UDP packet from invalid client id or ip: client id = {pakin.SessionID}, ip = {sender},  code = {pakin.ID}");
                }
            }
            catch (SocketException) { /* Handle or log as needed */ }
            catch (ObjectDisposedException) { /* Handle or log as needed */ }
            catch (Exception e)
            {
                if (log.IsErrorEnabled) log.Error("RecvFromCallback", e);
            }
            finally
            {
                if (!receiving)
                {
                    BeginReceiveUDP(s, server);
                }
            }
        }

        /// <summary>
        /// Starts receiving UDP packets.
        /// </summary>
        /// <param name="s">Socket to receive packets.</param>
        /// <param name="server">Server instance used to receive packets.</param>
        private bool BeginReceiveUDP(Socket socket, GameServer server)
        {
            bool isSuccess = false;
            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                socket.BeginReceiveFrom(server.UDPBuffer, 0, MAX_UDPBUF, SocketFlags.None, ref remoteEndPoint, m_udpReceiveCallback, server);
                isSuccess = true;
            }
            catch (SocketException ex)
            {
                log.Fatal($"Failed to resume receiving UDP packets. Error Code: {ex.ErrorCode}, Socket Error Code: {ex.SocketErrorCode}", ex);
            }
            catch (ObjectDisposedException ex)
            {
                log.Fatal("Attempted to start UDP. The socket has been disposed.", ex);
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                {
                    log.Error("Exception occurred in UDP Recv", ex);
                }
            }

            return isSuccess;
        }

        /// <summary>
        /// Sends a UDP packet
        /// </summary>
        /// <param name="bytes">Packet to be sent</param>
        /// <param name="count">The count of bytes to send</param>
        /// <param name="clientEndpoint">Address of receiving client</param>
        public void SendUDP(byte[] bytes, int count, EndPoint clientEndpoint)
        {
            SendUDP(bytes, count, clientEndpoint, null);
        }

        /// <summary>
        /// Sends a UDP packet
        /// </summary>
        /// <param name="bytes">Packet to be sent</param>
        /// <param name="count">The count of bytes to send</param>
        /// <param name="clientEndpoint">Address of receiving client</param>
        /// <param name="callback"></param>
        public void SendUDP(byte[] bytes, int count, EndPoint clientEndpoint, AsyncCallback callback)
        {
            var start = GameTimer.GetTickCount();

            //log.Warn($"send UDP packet to {clientEndpoint}");
            m_udpSocket.BeginSendTo(bytes, 0, count, SocketFlags.None, clientEndpoint, callback, m_udpSocket);


            var took = GameTimer.GetTickCount() - start;
            if (took > 100 && log.IsWarnEnabled)
                log.WarnFormat("m_udpSocket.BeginSendTo took {0}ms! (UDP to {1})", took, clientEndpoint.ToString());
        }



        /// <summary>
        /// Callback function for UDP sends
        /// </summary>
        /// <param name="ar">Asynchronous result of this operation</param>
        protected void SendToCallback(IAsyncResult ar)
        {
            if (ar == null)
                return;

            Socket s = (Socket)ar.AsyncState;

            // Überprüfen, ob der Socket noch verbunden ist
            if (s != null && s.Connected)
            {
                try
                {
                    s.EndSendTo(ar);
                }
                catch (SocketException ex)
                {
                    // Hier könnten Sie spezifischere Logik hinzufügen, um mit Socketfehlern umzugehen
                    if (log.IsErrorEnabled)
                        log.Error("SocketException in SendToCallback", ex);
                }
                catch (Exception e)
                {
                    if (log.IsErrorEnabled)
                        log.Error("SendToCallback", e);
                }
            }
        }

        #endregion

        #region Start

        /// <summary>
        /// Starts the server
        /// </summary>
        /// <returns>True if the server was successfully started</returns>
        public override bool Start()
        {
            try
            {
                if (log.IsDebugEnabled)
                    log.DebugFormat("Starting Server, Memory is {0}MB", GC.GetTotalMemory(false) / 1024 / 1024);

                m_status = eGameServerStatus.GSS_Closed;
                Thread.CurrentThread.Priority = ThreadPriority.Normal;

                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                //---------------------------------------------------------------
                // Try to compile the Scripts
                if (!InitComponent(CompileScripts(), "Script compilation"))
                    return false;

                //---------------------------------------------------------------
                // Check and update the database if needed
                if (!UpdateDatabase())
                    return false;

                //---------------------------------------------------------------
                // Try to init the server port
                if (!InitComponent(InitSocket(), "InitSocket()"))
                    return false;

                //---------------------------------------------------------------
                // Packet buffers
                if (!InitComponent(AllocatePacketBuffers(), "AllocatePacketBuffers()"))
                    return false;

                //---------------------------------------------------------------
                // Try to start the udp port
                if (!InitComponent(StartUDP(), "StartUDP()"))
                    return false;

                //---------------------------------------------------------------
                // Try to initialize the Scheduler
                if (!InitComponent(() => Scheduler = new Scheduler.SimpleScheduler(), "Scheduler Initialization"))
                    return false;

                //---------------------------------------------------------------
                // Try to initialize various managers
                if (!InitComponent(() => WorldManager = new WorldManager(this), "World Manager Initialization"))
                    return false;

                if (!InitComponent(() => PlayerManager = new PlayerManager(this), "Player Manager Initialization"))
                    return false;

                if (!InitComponent(() => NpcManager = new NpcManager(this), "NPC Manager Initialization"))
                    return false;

                if (!InitComponent(LanguageMgr.Init(), "Multi Language Initialization"))
                    return false;

                InitComponent(MailMgr.Init(), "Mail Manager Initialization");

                //---------------------------------------------------------------
                // Early init for WorldMgr
                if (!InitComponent(WorldMgr.EarlyInit(out RegionData[] regionsData), "World Manager PreInitialization"))
                    return false;

                //---------------------------------------------------------------
                // Try to initialize multiple components
                if (!InitComponent(StartScriptComponents(), "Script components"))
                    return false;

                if (!InitComponent(FactionMgr.Init(), "Faction Managers"))
                    return false;

                InitComponent(ArtifactMgr.Init(), "Artifact Manager");

                if (!InitComponent(GameLiving.LoadCalculators(), "GameLiving.LoadCalculators()"))
                    return false;

                if (!InitComponent(GameNpcInventoryTemplate.Init(), "Npc Equipment"))
                    return false;

                if (!InitComponent(NpcTemplateMgr.Init(), "Npc Templates Manager"))
                    return false;

                if (!InitComponent(HouseMgr.Start(), "House Manager"))
                    return false;

                if (!InitComponent(MarketCache.GetInitialize(), "Market Cache"))
                    return false;

                if (!InitComponent(WorldMgr.StartRegionMgrs(), "Region Managers"))
                    return false;

                //---------------------------------------------------------------
                // Enable Worldsave timer
                SetTimer(ref m_timer, SaveTimerProc, SaveInterval * MINUTE_CONV, "World save timer");
                SetTimer(ref m_timer1, WorldTimerProc, WorldTimerInterval * MINUTE_CONV, "World timer");
                SetTimer(ref m_timer2, WorldTimerProc2, 120 * MINUTE_CONV, "World timer2");

                //---------------------------------------------------------------
                // Load additional managers
                if (!InitComponent(BoatMgr.LoadAllBoats(), "Boat Manager"))
                    return false;

                if (!InitComponent(GuildMgr.LoadAllGuilds(), "Guild Manager"))
                    return false;

                if (!InitComponent(KeepMgr.Load(), "Keep Manager"))
                    return false;

                if (!InitComponent(AreaMgr.LoadAllAreas(), "Areas"))
                    return false;

                if (!InitComponent(DoorMgr.Init(), "Door Manager"))
                    return false;

                if (!InitComponent(WorldMgr.Init(regionsData), "World Manager Initialization"))
                    return false;

                regionsData = null;

                if (!InitComponent(() => LosCheckMgr.Initialize(), "LosCheckMgr Initialization"))
                    return false;

                if (!InitComponent(RelicMgr.Init(), "Relic Manager"))
                    return false;

                if (!InitComponent(CraftingMgr.Init(), "Crafting Managers"))
                    return false;

                if (!InitComponent(PlayerTitleMgr.Init(), "Player Titles Manager"))
                    return false;

                if (!InitComponent(BehaviourMgr.Init(), "Behaviour Manager"))
                    return false;

                // Load the quest managers if enabled
                if (Properties.LOAD_QUESTS)
                {
                    if (!InitComponent(QuestMgr.Init(), "Quest Manager"))
                        return false;
                }
                else
                {
                    log.InfoFormat("Not Loading Quest Manager : Obeying Server Property <load_quests> - {0}", Properties.LOAD_QUESTS);
                }

                //---------------------------------------------------------------
                // Try to initialize the DayNight Components
                if (!InitComponent(StartDayNightComponents(), "DayNightComponents"))
                    return false;

                // Notify our scripts that everything went fine!
                GameEventMgr.Notify(ScriptEvent.Loaded);

                // Set the GameServer StartTick
                m_startTick = GameTimer.GetTickCount();

                // Notify everyone that the server is now started!
                GameEventMgr.Notify(GameServerEvent.Started, this);

                // Try to start the base server (open server port for connections)
                if (!InitComponent(base.Start(), "base.Start()"))
                    return false;

                // Starts the Pathing Server
                if (!InitComponent(() => PathingMgr.Init(), "Pathing Manager Initialization"))
                    return false;

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

                // Open the server, players can now connect!
                m_status = eGameServerStatus.GSS_Open;

                if (log.IsInfoEnabled)
                    log.Info("GameServer is now open for connections!");

                // INIT WAS FINE!
                return true;
            }
            catch (Exception e)
            {
                if (log.IsErrorEnabled)
                    log.Error("Failed to start the server", e);

                return false;
            }
        }

        // Helper method to set a timer
        private void SetTimer(ref Timer timer, TimerCallback callback, int interval, string logMessage)
        {
            if (timer != null)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                timer.Dispose();
            }
            timer = new Timer(callback, null, interval, Timeout.Infinite);
            if (log.IsInfoEnabled)
                log.Info($"{logMessage}: true");
        }

        /// <summary>
        /// Logs unhandled exceptions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log.Fatal("Unhandled exception!\n" + e.ExceptionObject);
            if (e.IsTerminating)
                LogManager.Shutdown();
        }

        /// <summary>
        /// Recomiples the scripts dll
        /// </summary>
        /// <returns></returns>
        /*
        public bool RecompileScripts()
        {
            string scriptDirectory = Configuration.RootDirectory + Path.DirectorySeparatorChar + "scripts";
            if (!Directory.Exists(scriptDirectory))
                Directory.CreateDirectory(scriptDirectory);

            string[] parameters = Configuration.ScriptAssemblies.Split(',');
            return ScriptMgr.CompileScripts(false, scriptDirectory, Configuration.ScriptCompilationTarget, parameters);
        }
        */

        /// <summary>
		/// Recompiles or loads the scripts dll
		/// </summary>
		/// <returns></returns>
		public bool CompileScripts()
        {
            string scriptDirectory = Path.Combine(Configuration.RootDirectory, "scripts");
            if (!Directory.Exists(scriptDirectory))
                Directory.CreateDirectory(scriptDirectory);

            bool compiled = false;
            Configuration.EnableCompilation = true;

            /*
            // Check if Configuration Forces to use Pre-Compiled Game Server Scripts Assembly
            if (!Configuration.EnableCompilation)
            {
                log.Info("Script Compilation Disabled in Server Configuration, Loading pre-compiled Assembly...");

                if (File.Exists(Configuration.ScriptCompilationTarget))
                {
                    ScriptMgr.LoadAssembly(Configuration.ScriptCompilationTarget);
                }
                else
                {
                    log.WarnFormat("Compilation Disabled - Could not find pre-compiled Assembly : {0} - Server starting without Scripts Assembly!", Configuration.ScriptCompilationTarget);
                }

                compiled = true;
            }
            else
            */
            {
                compiled = ScriptMgr.CompileScripts(false, scriptDirectory, Configuration.ScriptCompilationTarget, Configuration.ScriptAssemblies);
            }

            if (compiled)
            {
                //---------------------------------------------------------------
                //Register Script Tables
                if (log.IsInfoEnabled)
                    log.Info("GameServerScripts Tables Initializing...");

                try
                {
                    // Walk through each assembly in scripts
                    foreach (Assembly asm in ScriptMgr.Scripts)
                    {
                        // Walk through each type in the assembly
                        foreach (Type type in asm.GetTypes())
                        {
                            if (type.IsClass != true || !typeof(DataObject).IsAssignableFrom(type))
                                continue;

                            object[] attrib = type.GetCustomAttributes(typeof(DataTable), false);
                            if (attrib.Length > 0)
                            {
                                if (log.IsInfoEnabled)
                                    log.Info("Registering Scripts table: " + type.FullName);

                                GameServer.Database.RegisterDataObject(type);
                            }
                        }
                    }
                }
                catch (DatabaseException dbex)
                {
                    if (log.IsErrorEnabled)
                        log.Error("Error while registering Script Tables", dbex);

                    return false;
                }

                if (log.IsInfoEnabled)
                    log.Info("GameServerScripts Database Tables Initialization: true");

                return true;
            }

            return false;
        }
        /// <summary>
        /// Initialize all script components
        /// </summary>
        /// <returns>true if successfull, false if not</returns>
        protected bool StartScriptComponents()
        {
            try
            {
                //---------------------------------------------------------------
                //Create the server rules
                m_serverRules = ScriptMgr.CreateServerRules(Configuration.ServerType);
                if (log.IsInfoEnabled)
                    log.Info("Server rules: true");

                //---------------------------------------------------------------
                //Load the skills
                SkillBase.LoadSkills();
                if (log.IsInfoEnabled)
                    log.Info("Loading skills: true");

                //---------------------------------------------------------------
                //Register all event handlers
                var scripts = new ArrayList(ScriptMgr.Scripts);
                scripts.Insert(0, typeof(GameServer).Assembly);
                foreach (Assembly asm in scripts)
                {
                    GameEventMgr.RegisterGlobalEvents(asm, typeof(GameServerStartedEventAttribute), GameServerEvent.Started);
                    GameEventMgr.RegisterGlobalEvents(asm, typeof(GameServerStoppedEventAttribute), GameServerEvent.Stopped);
                    GameEventMgr.RegisterGlobalEvents(asm, typeof(ScriptLoadedEventAttribute), ScriptEvent.Loaded);
                    GameEventMgr.RegisterGlobalEvents(asm, typeof(ScriptUnloadedEventAttribute), ScriptEvent.Unloaded);
                }
                if (log.IsInfoEnabled)
                    log.Info("Registering global event handlers: true");
            }
            catch (Exception e)
            {
                if (log.IsErrorEnabled)
                    log.Error("StartScriptComponents", e);
                return false;
            }
            //---------------------------------------------------------------
            return true;
        }
        /// <summary>
        /// Initialize all Day/Night Mob components
        /// </summary>
        /// <returns>true if successfull, false if not</returns>
        protected bool StartDayNightComponents()
        {
            try
            {

                //---------------------------------------------------------------
                //Load the Day/Night mobs from DB
                GameNPC.LoadDayNightMobs();

                if (log.IsInfoEnabled)
                    log.Info("Loading Day/Night Mobs: true");

                GameNPC.InitializeHashtables();
                if (log.IsInfoEnabled)
                    log.Info("Loading NPC InitializeHashtables: true");


            }
            catch (Exception e)
            {
                if (log.IsErrorEnabled)
                    log.Error("StartDayNightComponents", e);
                return false;
            }
            //---------------------------------------------------------------
            return true;
        }
                

        /// <summary>
        /// Do any required updates to the database
        /// </summary>
        /// <returns>true if all went fine, false if errors</returns>
        protected virtual bool UpdateDatabase()
        {
            try
            {

                if (Properties.Database_Cleanup)
                {


                    /*

                    IList<Account> dbAccounts2 = GameServer.Database.SelectAllObjects<Account>();


                    foreach (Account _accounts in dbAccounts2)
                    {
                        if (string.IsNullOrEmpty(_accounts.HouseNumber.ToString()) == false)
                        {
                            switch (GetRealmByHousenumber(_accounts.HouseNumber))
                            {
                                case 1:
                                    {
                                        if (dbAccounts2 != null && _accounts.HasHouseAlb == false)
                                        {

                                            {
                                                _accounts.HouseNumberAlb = _accounts.HouseNumber;
                                                _accounts.HasHouseAlb = true;
                                                _accounts.AllowAdd = true;
                                                log.ErrorFormat("Alb House {0} übertragen!!", _accounts.HouseNumber);
                                            }
                                            GameServer.Database.SaveObject(_accounts);

                                        }
                                    }
                                    break;
                                case 2:
                                    {
                                        if (_accounts != null && _accounts.HasHouseMid == false)
                                        {
                                            {
                                                _accounts.HouseNumberMid = _accounts.HouseNumber;
                                                _accounts.HasHouseMid = true;
                                                _accounts.AllowAdd = true;
                                                log.ErrorFormat("Md House {0} übertragen!!", _accounts.HouseNumber);
                                            }
                                            GameServer.Database.SaveObject(_accounts);

                                        }
                                    }
                                    break;
                                case 3:
                                    {
                                        if (_accounts != null && _accounts.HasHouseHib == false)
                                        {

                                            {
                                                _accounts.HouseNumberHib = _accounts.HouseNumber;
                                                _accounts.HasHouseHib = true;
                                                _accounts.AllowAdd = true;
                                                log.ErrorFormat("Hib House {0} übertragen!!", _accounts.HouseNumber);
                                            }
                                            GameServer.Database.SaveObject(_accounts);

                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
                log.Error("Häuser übertragen!!");
                return false;
            }

                    */















                    /*
                        var chars = GameServer.Database.SelectAllObjects<DOLCharacters>();
                        log.Info("Checking old for accounts for delete... Please wait..");

                        foreach (DOLCharacters Dolchar in chars)
                        {

                            IList<Account> dbAccounts2 = GameServer.Database.SelectAllObjects<Account>();

                            foreach (Account _accounts in dbAccounts2)
                            {
                                if (Dolchar.AccountName == _accounts.Name && _accounts.LastLogin.Year <= 2022)
                                {


                                    if (Dolchar != null)
                                    {

                                        log.ErrorFormat("Delete Account!: {0} ", Dolchar.AccountName);

                                        IList<DOLCharacters> allchars = GameServer.Database.SelectAllObjects<DOLCharacters>();


                                        foreach (DOLCharacters owner in allchars)
                                        {
                                            if (Dolchar.AccountName == owner.AccountName && owner != null && owner.AccountName != null)
                                            {
                                                if (owner != null && string.IsNullOrEmpty(owner.ObjectId) == false)
                                                {

                                                    var guild = GameServer.Database.SelectObjects<DBGuild>("`GuildID` = @GuildID", new QueryParameter("@GuildID", owner.GuildID)).FirstOrDefault();


                                                    if (guild != null)// && string.IsNullOrEmpty(guild.GuildID) == false)// && guild.GuildID == _charinventory.OwnerID)
                                                    {


                                                        if (string.IsNullOrEmpty(owner.GuildID) == false && string.IsNullOrEmpty(guild.GuildID) == false && owner.GuildID == guild.GuildID && owner.GuildRank > 0)
                                                        {

                                                            log.ErrorFormat("Character deleted, but was not owner of his guild! - OwnerID: {0}", owner.ObjectId);
                                                            GameServer.Database.DeleteObject(owner);

                                                        }


                                                        else if (owner.GuildID == guild.GuildID && owner.GuildRank == 0 && guild.GuildName != "Mularn Protectors" && guild.GuildName != "Defender of Albion" && guild.GuildName != "Tir na Nog Adventurers" && guild.GuildName != "Brotherland Staff")
                                                        {

                                                            IList<DBRank> ranks = GameServer.Database.SelectObjects<DBRank>("`GuildID` = @GuildID", new QueryParameter("@GuildID", GameServer.Database.Escape(owner.GuildID)));
                                                            for (int i = 0; i < ranks.Count; i++) //GameServer.Database.SelectObjects<SalvageYield>("`GuildID` = @GuildID", new QueryParameter("@GuildID", removeGuild.ID)).FirstOrDefault();
                                                            {
                                                                DBRank guildRank = ranks[i];
                                                                if (guildRank.GuildID == owner.GuildID)
                                                                {
                                                                    log.ErrorFormat("Delete Guild  Rank from Guild- Name: {0} from Guildmaster: {1}", guild.GuildName, owner.Name);
                                                                    GameServer.Database.DeleteObject(ranks);
                                                                    break;

                                                                }
                                                                GameServer.Database.SaveObject(ranks);
                                                            }

                                                            IList<DBGuild> gul = GameServer.Database.SelectObjects<DBGuild>("`GuildID` = @GuildID", new QueryParameter("@GuildID", GameServer.Database.Escape(owner.GuildID)));
                                                            for (int i = 0; i < gul.Count; i++) //GameServer.Database.SelectObjects<SalvageYield>("`GuildID` = @GuildID", new QueryParameter("@GuildID", removeGuild.ID)).FirstOrDefault();
                                                            {
                                                                DBGuild guilds = gul[i];
                                                                if (guilds.GuildID == owner.GuildID)
                                                                {
                                                                    log.ErrorFormat("Delete Ranks - Name: {0} from Guildmaster: {1}", guild.GuildName, owner.Name);
                                                                    GameServer.Database.DeleteObject(gul);
                                                                    break;
                                                                }
                                                                GameServer.Database.SaveObject(gul);
                                                            }
                                                            GameServer.Database.DeleteObject(owner);
                                                        }
                                                        continue;
                                                    }
                                                    else

                                                        GameServer.Database.DeleteObject(owner);
                                                    log.ErrorFormat("Character deleted, but Character has no guild! - OwnerID: {0}", owner.ObjectId);
                                                    continue;


                                                }
                                            }
                                        }
                                    }
                                }
                            }

                        }
                        log.ErrorFormat("Character cleaning compeltted!");
                        return false;
                    }
                }
                */

            ///war test -->>>
            /*
            var chars = GameServer.Database.SelectAllObjects<DOLCharacters>();
            log.Info("Checking old for accounts for delete... Please wait..");

            foreach (DOLCharacters Dolchar in chars)
            {

                IList<Account> dbAccounts2 = GameServer.Database.SelectAllObjects<Account>();

                foreach (Account _accounts in dbAccounts2)
                {
                    if (Dolchar.AccountName == _accounts.Name && _accounts.LastLogin.Year <= 2022)
                    {


                        if (Dolchar != null)
                        {

                            log.ErrorFormat("Delete Quests!: {0} ", Dolchar.AccountName);

                            IList<DOLCharacters> allchars = GameServer.Database.SelectAllObjects<DOLCharacters>();


                            foreach (DOLCharacters owner in allchars)
                            {


                                IList<DBQuest> quests = GameServer.Database.SelectAllObjects<DBQuest>();
                                foreach (DBQuest quest in quests)
                                {
                                    if (quest.Character_ID == owner.ObjectId)
                                    {
                                        GameServer.Database.DeleteObject(quest);

                                        log.ErrorFormat("Quest Found and removed for Owner: {0} ok", owner.ObjectId);
                                    }

                                    //GameServer.Database.SaveObject(quest);



                                }

                                IList<CharacterXDataQuest> dataquests = GameServer.Database.SelectAllObjects<CharacterXDataQuest>();
                                foreach (CharacterXDataQuest xquest in dataquests)
                                {
                                    if (xquest.Character_ID == owner.ObjectId)
                                    {
                                        GameServer.Database.DeleteObject(xquest);

                                        log.ErrorFormat("Quest Found and removed for Owner: {0} ok", owner.ObjectId);
                                    }

                                    //GameServer.Database.SaveObject(quest);



                                }

                            }
                        }
                    }
                }
            }
        }
        */

                    log.Info("Checking Inventorys for missing Characters... Please wait..");
                    IList<InventoryItem> charinventar2 = GameServer.Database.SelectAllObjects<InventoryItem>();
                    foreach (InventoryItem _charinventory in charinventar2)
                    {
                        if (_charinventory != null)
                        {
                            var owner = GameServer.Database.SelectObjects<DOLCharacters>("`DOLCharacters_ID` = @DOLCharacters_ID", new QueryParameter("@DOLCharacters_ID", _charinventory.OwnerID)).FirstOrDefault();
                            if (owner != null && string.IsNullOrEmpty(owner.ObjectId) == false)
                            {
                                //log.ErrorFormat("Character Found: {0} ok", owner.ObjectId);
                            }
                            else
                            {
                                /*
                                log.Info("Checking Inventorys for missing UTemplates... Please wait..");
                                IList<ItemUnique> DbItemUnique = GameServer.Database.SelectAllObjects<ItemUnique>();
                                foreach (ItemUnique _unique in DbItemUnique)
                                {

                                    var item = GameServer.Database.SelectObjects<InventoryItem>("`UTemplate_Id` = @UTemplate_Id", new QueryParameter("@UTemplate_Id", _unique.Id_nb)).FirstOrDefault();
                                    if (item != null && string.IsNullOrEmpty(item.UTemplate_Id) == false)
                                    {
                                        //log.ErrorFormat("Itemunique found: {0} ok", unique.Id_nb);
                                    }
                                    else
                                    {
                                        log.ErrorFormat("delete unused unique entry - Id_nb: {0}", _unique.Id_nb);
                                        GameServer.Database.DeleteObject(_unique);

                                    }
                                }


                                //Quets
                                IList<DBQuest> quests = GameServer.Database.SelectAllObjects<DBQuest>();
                                foreach (DBQuest quest in quests)
                                {
                                    if (quest.Character_ID == _charinventory.OwnerID)
                                    {
                                        GameServer.Database.DeleteObject(quest);

                                        log.ErrorFormat("Quest Found and removed for Owner: {0} ok", _charinventory.OwnerID);
                                    }

                                    //GameServer.Database.SaveObject(quest);

                                }

                                IList<CharacterXDataQuest> dataquests = GameServer.Database.SelectAllObjects<CharacterXDataQuest>();
                                foreach (CharacterXDataQuest xquest in dataquests)
                                {
                                    if (xquest.Character_ID == _charinventory.OwnerID)
                                    {
                                        GameServer.Database.DeleteObject(xquest);

                                        log.ErrorFormat("DataQuest Found and removed for Owner: {0} ok", _charinventory.ObjectId);
                                    }

                                    //GameServer.Database.SaveObject(quest);

                                }

                                //Boote
                                IList<DBBoat> playerBoats = GameServer.Database.SelectAllObjects<DBBoat>();
                                foreach (DBBoat boat in playerBoats)
                                {
                                    if (boat.BoatOwner == _charinventory.OwnerID)
                                    {
                                        GameServer.Database.DeleteObject(boat);

                                        log.ErrorFormat("PlayerBoat Found and removed for Owner: {0} ok", _charinventory.ObjectId);
                                    }

                                    //GameServer.Database.SaveObject(quest);

                                }
                            }
                        }

                    }
                    log.ErrorFormat("Quest, boats and Inventorys cleaning compeltted!");
                    return false;
                }
                                */


                                //Boote
                                //IList<CharacterXOneTimeDrop> OneTimeDrops = GameServer.Database.SelectAllObjects<CharacterXOneTimeDrop>();
                                var OneTimeDrops = GameServer.Database.SelectObjects<CharacterXOneTimeDrop>("`CharacterID` = @CharacterID", new QueryParameter("@CharacterID", _charinventory.OwnerID));
                                foreach (CharacterXOneTimeDrop drop in OneTimeDrops)
                                {
                                    if (drop.CharacterID == _charinventory.OwnerID)
                                    {
                                        GameServer.Database.DeleteObject(drop);

                                        log.ErrorFormat("CharacterXOneTimeDrop Found and removed for Owner: {0} ok", _charinventory.ObjectId);
                                    }


                                }


                                //IList<DBFactionAggroLevel> DBFactionAggroLevels = GameServer.Database.SelectAllObjects<DBFactionAggroLevel>();
                                var DBFactionAggroLevels = GameServer.Database.SelectObjects<DBFactionAggroLevel>("`CharacterID` = @CharacterID", new QueryParameter("@CharacterID", _charinventory.OwnerID));
                                foreach (DBFactionAggroLevel levelchar in DBFactionAggroLevels)
                                {
                                    if (levelchar.CharacterID == _charinventory.OwnerID)
                                    {
                                        GameServer.Database.DeleteObject(levelchar);

                                        log.ErrorFormat("DBFactionAggroLevel Found and removed for Owner: {0} ok", _charinventory.ObjectId);
                                    }

                                }
                                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


                                InventoryItem invitem = null;
                                //IList<InventoryItem> playerInventorys = GameServer.Database.SelectObjects<InventoryItem>();
                                var playerInventorys = GameServer.Database.SelectObjects<InventoryItem>("`OwnerID` = @OwnerID", new QueryParameter("@OwnerID", _charinventory.OwnerID));
                                foreach (InventoryItem invent in playerInventorys)
                                {
                                    if (invent != null)
                                    {

                                        invitem = invent;


                                        // log.ErrorFormat("Delete players Guild Inventory: {0} ok", _charinventory.OwnerID);
                                    }

                                    //GameServer.Database.SaveObject(quest);

                                }

                                // log.ErrorFormat("Delete - OwnerID: {0}", _charinventory.OwnerID);
                                if (invitem != null)
                                {
                                    var guild = GameServer.Database.SelectObjects<DBGuild>("`GuildID` = @GuildID", new QueryParameter("@GuildID", invitem.OwnerID)).FirstOrDefault();

                                    if (guild != null)// && string.IsNullOrEmpty(guild.GuildID) == false)// && guild.GuildID == _charinventory.OwnerID)
                                    {
                                        /*
                                        if (invitem.OwnerID != guild.GuildID)
                                        {

                                            GameServer.Database.DeleteObject(invitem);

                                            log.ErrorFormat("Delete Guild Inventory von gilde: {0} ok", invitem.OwnerID);

                                        }
                                        else
                                        */
                                        log.ErrorFormat("Nicht gelöscht weil GuildID gefunden! - OwnerID: {0}", invitem.OwnerID);
                                        ///continue;
                                    }
                                    else
                                    {
                                        GameServer.Database.DeleteObject(invitem);

                                        log.ErrorFormat("Delete normal Inventory: {0} ok", invitem.OwnerID);

                                    }
                                }
                            }
                        }
                    }
                    log.Error("Inventorys cleaning compeltted!");

                    {

                        log.Info("Checking Inventorys for missing UTemplates... Please wait..");
                        IList<ItemUnique> DbItemUnique = GameServer.Database.SelectAllObjects<ItemUnique>();
                        foreach (ItemUnique _unique in DbItemUnique)
                        {

                            var item = GameServer.Database.SelectObjects<InventoryItem>("`UTemplate_Id` = @UTemplate_Id", new QueryParameter("@UTemplate_Id", _unique.Id_nb)).FirstOrDefault();
                            if (item != null && string.IsNullOrEmpty(item.UTemplate_Id) == false)
                            {
                                //log.ErrorFormat("Itemunique found: {0} ok", unique.Id_nb);
                            }
                            else
                            {
                                log.ErrorFormat("delete unused unique entry - Id_nb: {0}", _unique.Id_nb);
                                GameServer.Database.DeleteObject(_unique);

                            }
                        }

                    }

                    log.Error("UTemplates cleaning compeltted!");
                    return false;
                }

            }

            /*
             log.Info("Delete LastLogin Accounts and Chars from 2022 and before... Please wait..");
             IList<Account> dbAccounts1 = GameServer.Database.SelectAllObjects<Account>();
             foreach (Account _accounts in dbAccounts1)
             {
                 if (_accounts != null && _accounts.LastLogin.Year <= 2022)
                 {
                     log.ErrorFormat("Last login war {0}", _accounts.LastLogin.Year.ToString());


                     House h = HouseMgr.GetHouse(_accounts.HouseNumber);
                     log.ErrorFormat("Delete House - Nummer: {0}", _accounts.HouseNumber);
                     HouseMgr.RemoveHouse(h as House);


                     log.ErrorFormat("Delete Account - Name: {0}", _accounts.Name);
                     GameServer.Database.DeleteObject(_accounts);
                     // continue;
                     //log.ErrorFormat("Delete unused Account - Name: {0}", _accounts.Name);
                     //GameServer.Database.DeleteObject(_accounts);
                 }
             }


             log.Info("Checking Accounts for missing Charcter entrys... Please wait..");
             IList<Account> dbAccounts = GameServer.Database.SelectAllObjects<Account>();
             foreach (Account _accounts in dbAccounts)
             {

                 var Characcaunt = GameServer.Database.SelectObjects<DOLCharacters>("`AccountName` = @AccountName", new QueryParameter("@AccountName", _accounts.Name)).FirstOrDefault();
                 if (Characcaunt != null && string.IsNullOrEmpty(Characcaunt.AccountName) == false)
                 {
                     //log.ErrorFormat("Character Found: {0} ok", owner.ObjectId);
                     // continue;

                 }
                 else
                 {


                     if (_accounts != null)
                     {


                         log.ErrorFormat("Delete unused Account - Name: {0}", _accounts.Name);
                         GameServer.Database.DeleteObject(_accounts);
                     }
                 }
             }
             return false;

         }

        }
            */

            /*
                log.Info("Checking database for updates ...");
                foreach (Type type in typeof(GameServer).Assembly.GetTypes())
                {
                    if (!type.IsClass) continue;
                    if (!typeof(IDatabaseUpdater).IsAssignableFrom(type)) continue;
                    object[] attributes = type.GetCustomAttributes(typeof(DatabaseUpdateAttribute), false);
                    if (attributes.Length <= 0) continue;

                    var instance = Activator.CreateInstance(type) as IDatabaseUpdater;
                    instance.Update();
                }
                    */


            catch (Exception e)
            {
                log.Error("Error checking/updating database: ", e);
                return false;
            }

            log.Info("Database cleanup complete.");
            return true;
        }

        /// <summary>
        /// Prints out some text info on component initialisation
        /// and stops the server again if the component failed
        /// </summary>
        /// <param name="componentInitState">The state</param>
        /// <param name="text">The text to print</param>
        /// <returns>false if startup should be interrupted</returns>
        protected bool InitComponent(bool componentInitState, string text)
        {
            if (log.IsDebugEnabled)
                log.DebugFormat("Start Memory {0}: {1}MB", text, GC.GetTotalMemory(false) / 1024 / 1024);

            if (log.IsInfoEnabled)
                log.InfoFormat("{0}: {1}", text, componentInitState);

            if (!componentInitState)
                Stop();

            if (log.IsDebugEnabled)
                log.DebugFormat("Finish Memory {0}: {1}MB", text, GC.GetTotalMemory(false) / 1024 / 1024);

            return componentInitState;
        }


        protected bool InitComponent(Action componentInitMethod, string text)
        {
            if (log.IsDebugEnabled)
                log.DebugFormat("Start Memory {0}: {1}MB", text, GC.GetTotalMemory(false) / 1024 / 1024);

            bool componentInitState = false;
            try
            {
                componentInitMethod();
                componentInitState = true;
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                    log.ErrorFormat("{0}: Error While Initialization\n{1}", text, ex);
            }

            if (log.IsInfoEnabled)
                log.InfoFormat("{0}: {1}", text, componentInitState);

            if (!componentInitState)
                Stop();

            if (log.IsDebugEnabled)
                log.DebugFormat("Finish Memory {0}: {1}MB", text, GC.GetTotalMemory(false) / 1024 / 1024);

            return componentInitState;
        }
        #endregion

        #region Stop

        public void Close()
        {
            m_status = eGameServerStatus.GSS_Closed;
        }

        public void Open()
        {
            m_status = eGameServerStatus.GSS_Open;
        }

        /// <summary>
        /// Stops the server, disconnects all clients, and writes the database to disk
        /// </summary>
        public override void Stop()
        {
            //Stop new clients from logging in
            m_status = eGameServerStatus.GSS_Closed;

            log.Info("GameServer.Stop() - enter method");

            if (log.IsWarnEnabled)
            {
                string stacks = PacketProcessor.GetConnectionThreadpoolStacks();
                if (stacks.Length > 0)
                {
                    log.Warn("Packet processor thread stacks:");
                    log.Warn(stacks);
                }
            }

            //Notify our scripthandlers
            GameEventMgr.Notify(ScriptEvent.Unloaded);

            //Notify of the global server stop event
            //We notify before we shutdown the database
            //so that event handlers can use the datbase too
            GameEventMgr.Notify(GameServerEvent.Stopped, this);
            GameEventMgr.RemoveAllHandlers(true);

            //Stop the World Save timer

            //Stop the World Save timer
            if (m_timer != null)
            {
                m_timer.Change(Timeout.Infinite, Timeout.Infinite);
                m_timer.Dispose();
                m_timer = null;
            }
            if (m_timer1 != null)
            {
                m_timer1.Change(Timeout.Infinite, Timeout.Infinite);
                m_timer1.Dispose();
                m_timer1 = null;
            }
            if (m_timer2 != null)
            {
                m_timer2.Change(Timeout.Infinite, Timeout.Infinite);
                m_timer2.Dispose();
                m_timer2 = null;
            }

            //Stop the base server
            base.Stop();

            //Close the UDP connection
            if (m_udpSocket != null)
            {
                m_udpSocket.Close();
                m_udpSocket = null;
            }


            //Stop all mobMgrs
            WorldMgr.StopRegionMgrs();

            //unload all weatherMgr
            // WeatherMgr.Unload();

            //Stop the WorldMgr, save all players
            //WorldMgr.SaveToDatabase();
            SaveTimerProc(null);

            WorldMgr.Exit();


            //Save the database
            // 2008-01-29 Kakuri - Obsolete
            /*if ( m_database != null )
                {
                    m_database.WriteDatabaseTables();
                }*/

            m_serverRules = null;
            ////////////////////////////////////////////////
            // Stop Server Scheduler
            if (Scheduler != null)
                Scheduler.Shutdown();
            Scheduler = null;


            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            if (log.IsInfoEnabled)
                log.Info("Server Stopped");

            LogManager.Shutdown();
        }

        #endregion

        #region Packet buffer pool

        /// <summary>
        /// The size of all packet buffers.
        /// </summary>
        private const int BUF_SIZE = 2048;

        /// <summary>
        /// Holds all packet buffers.
        /// </summary>
        private Queue<byte[]> m_packetBufPool;
        private readonly object m_packetBufPoolLock = new object();

        public int MaxPacketPoolSize
        {
            get { return Configuration.MaxClientCount * 3; }
        }

        /// <summary>
        /// Gets the count of packet buffers in the pool.
        /// </summary>
        public int PacketPoolSize
        {
            get
            {
                int packetBufCount = 0;

                lock (m_packetBufPoolLock)
                    packetBufCount = m_packetBufPool.Count;

                return packetBufCount;
            }
        }

        /// <summary>
        /// Allocates all packet buffers.
        /// </summary>
        /// <returns>success</returns>
        private bool AllocatePacketBuffers()
        {
            int count = MaxPacketPoolSize;

            lock (m_packetBufPoolLock)
            {
                m_packetBufPool = new Queue<byte[]>(count);

                for (int i = 0; i < count; i++)
                {
                    m_packetBufPool.Enqueue(new byte[BUF_SIZE]);
                }
            }

            if (log.IsDebugEnabled)
                log.DebugFormat("allocated packet buffers: {0}", count.ToString());

            return true;
        }

        /// <summary>
        /// Gets packet buffer from the pool.
        /// </summary>
        /// <returns>byte array that will be used as packet buffer.</returns>
        public override byte[] AcquirePacketBuffer()
        {
            lock (m_packetBufPoolLock)
            {
                if (m_packetBufPool.Count > 0)
                    return m_packetBufPool.Dequeue();
            }

            log.Warn("packet buffer pool is empty!");

            return new byte[BUF_SIZE];
        }

        /// <summary>
        /// Releases previously acquired packet buffer.
        /// </summary>
        /// <param name="buf">The released buf</param>
        public override void ReleasePacketBuffer(byte[] buf)
        {
            if (buf == null)
                return;

            lock (m_packetBufPoolLock)
            {
                if (m_packetBufPool.Count < MaxPacketPoolSize)
                    m_packetBufPool.Enqueue(buf);
            }
        }

        #endregion

        #region Client

        /// <summary>
        /// Creates a new client
        /// </summary>
        /// <returns>An instance of a new client</returns>
        protected override BaseClient GetNewClient()
        {
            var client = new GameClient(this);
            GameEventMgr.Notify(GameClientEvent.Created, client);
            client.UdpConfirm = false;

            return client;
        }

        #endregion

        #region Logging

        /// <summary>
        /// Writes a line to the gm log file
        /// </summary>
        /// <param name="text">the text to log</param>
        public void LogGMAction(string text)
        {
            m_gmLog.Logger.Log(typeof(GameServer), Level.Alert, text, null);
        }

        /// <summary>
        /// Writes a line to the cheat log file
        /// </summary>
        /// <param name="text">the text to log</param>
        public void LogCheatAction(string text)
        {
            m_cheatLog.Logger.Log(typeof(GameServer), Level.Alert, text, null);
            log.Debug(text);
        }

        /// <summary>
        /// Writes a line to the cheat log file
        /// </summary>
        /// <param name="text">the text to log</param>
        public void LogSendAction(string text)
        {
            m_cheatLog.Logger.Log(typeof(GameServer), Level.Alert, text, null);
           
        }

        /// <summary>
        /// Writes a line to the inventory log file
        /// </summary>
        /// <param name="text">the text to log</param>
        public void LogInventoryAction(string text)
        {
            m_inventoryLog.Logger.Log(typeof(GameServer), Level.Alert, text, null);
        }

        #endregion


        #region World Timer


        /// <summary>
        /// The Worldtimer for the Relic Manager
        /// </summary>
        /// <param name="sender"></param>
        protected void WorldTimerProc(object sender)
        {

            {
                try
                {

                    ThreadPriority oldprio = Thread.CurrentThread.Priority;
                    Thread.CurrentThread.Priority = ThreadPriority.Lowest;

                    if (Properties.World_Timer_Enabled)
                    {
                       WorldMgr.WorldTimercheck();
                    }
                }
                catch (Exception ex)
                {
                    if (log.IsErrorEnabled)
                        log.Error("WorldTimer error: ", ex);
                }
                finally
                {
                    if (m_timer1 != null)
                        m_timer1.Change(WorldTimerInterval * MINUTE_CONV, Timeout.Infinite);

                }
            }
        }

        /// <summary>
        /// The Worldtimer for the Relic Manager
        /// </summary>
        /// <param name="sender"></param>
        protected void WorldTimerProc2(object sender)
        {

            {
                try
                {

                    ThreadPriority oldprio = Thread.CurrentThread.Priority;
                    Thread.CurrentThread.Priority = ThreadPriority.Lowest;

                    WorldMgr.KeepHealTimercheck();
                                       
                   
                }
                catch (Exception ex)
                {
                    if (log.IsErrorEnabled)
                        log.Error("WorldTimerProc2 error: ", ex);
                }
                finally
                {
                    if (m_timer2 != null)
                        m_timer2.Change(1 * MINUTE_CONV, Timeout.Infinite);

                }
            }
        }
        #endregion

        #region Database

        /// <summary>
        /// Initializes the database
        /// </summary>
        /// <returns>True if the database was successfully initialized</returns>
        public bool InitDB
        {
            get
            {
                if (m_database == null)
                {
                    m_database = ObjectDatabase.GetObjectDatabase(Configuration.DBType, Configuration.DBConnectionString);

                    try
                    {
                        //We will search our assemblies for DataTables by reflection so
                        //it is not neccessary anymore to register new tables with the
                        //server, it is done automatically!

                        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            // Walk through each type in the assembly
                            assembly.GetTypes().AsParallel().ForAll(type =>
                            // foreach (Type type in assembly.GetTypes())
                            {
                                if (!type.IsClass || type.IsAbstract)
                                {
                                    return;
                                }
                                //continue;

                                var attrib = type.GetCustomAttributes<DataTable>(false);
                                if (attrib.Any())
                                {
                                    if (log.IsInfoEnabled)
                                    {
                                        log.InfoFormat("Registering table: {0}", type.FullName);

                                    }
                                    m_database.RegisterDataObject(type);
                                }
                            });
                        }
                    }
                    catch (DatabaseException e)
                    {
                        if (log.IsErrorEnabled)
                            log.Error("Error registering Tables", e);
                        return false;
                    }
                }
                if (log.IsInfoEnabled)
                    log.Info("Database Initialization: true");
                return true;
            }
        }

        /// <summary>
        /// Function called at X interval to write the database to disk
        /// </summary>
        /// <param name="sender">Object that generated the event</param>
        protected void SaveTimerProc(object sender)
        {
           
            if (Properties.Auto_Save_Database)
            {
                try
                {
                    var startTick = GameTimer.GetTickCount();
                    if (log.IsInfoEnabled)
                        log.Info("Saving database...");
                    if (log.IsDebugEnabled)
                        log.Debug("Save ThreadId=" + Thread.CurrentThread.ManagedThreadId.ToString());
                    //int saveCount = 0;
                    if (m_database != null)
                    {
                        ThreadPriority oldprio = Thread.CurrentThread.Priority;
                        Thread.CurrentThread.Priority = ThreadPriority.Lowest;

                        //Only save the players, NOT any other object!
                        //saveCount = WorldMgr.SavePlayers();//raus

                        //The following line goes through EACH region and EACH object
                        //is tested for savability. A real waste of time, so it is commented out

                        GuildMgr.SaveAllGuilds();
                        BoatMgr.SaveAllBoats();
                        FactionMgr.SaveAllAggroToFaction();

                        // 2008-01-29 Kakuri - Obsolete

                        Thread.CurrentThread.Priority = oldprio;
                    }
                    if (log.IsInfoEnabled)
                        log.Info("Saving database complete!");

                    /*
                    //var took = GameTimer.GetTickCount() - startTick;
                    if (log.IsInfoEnabled)
                    {
                        log.Info("Saving database complete!");  //  log.Info("Saved all databases and " + saveCount.ToString() + " players in " + startTick.ToString() + "ms");
                    }
                    */
                }
                catch (Exception e1)
                {
                    if (log.IsErrorEnabled)
                        log.Error("SaveTimerProc", e1);
                }
                finally
                {
                    if (m_timer != null)
                        m_timer.Change(SaveInterval * MINUTE_CONV, Timeout.Infinite);
                    GameEventMgr.Notify(GameServerEvent.WorldSave);
                }
            }
            
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default game server constructor
        /// </summary>
        protected GameServer()
            : this(new GameServerConfiguration())
        {
        }

        /// <summary>
        /// Constructor with a given configuration
        /// </summary>
        /// <param name="config">A valid game server configuration</param>
        protected GameServer(GameServerConfiguration config)
            : base(config)
        {
            m_gmLog = LogManager.GetLogger(Configuration.GMActionsLoggerName);
            m_cheatLog = LogManager.GetLogger(Configuration.CheatLoggerName);
            m_inventoryLog = LogManager.GetLogger(Configuration.InventoryLoggerName);

            if (log.IsDebugEnabled)
            {
                log.Debug("Current directory is: " + Directory.GetCurrentDirectory());
                log.Debug("Gameserver root directory is: " + Configuration.RootDirectory);
                log.Debug("Changing directory to root directory");
            }
            Directory.SetCurrentDirectory(Configuration.RootDirectory);

            try
            {


                m_udpBuf = new byte[MAX_UDPBUF];
                m_udpReceiveCallback = new AsyncCallback(RecvFromCallback);
                m_udpSendCallback = new AsyncCallback(SendToCallback);

                if (!InitDB || m_database == null)
                {
                    if (log.IsErrorEnabled)
                        log.Error("Could not initialize DB, please check path/connection string");
                    throw new ApplicationException("DB initialization error");
                }

                if (log.IsInfoEnabled)
                    log.Info("Game Server Initialization finished!");
            }
            catch (Exception e)
            {
                if (log.IsFatalEnabled)
                    log.Fatal("GameServer initialization failed!", e);
                throw new ApplicationException("Fatal Error: Could not initialize Game Server", e);
            }
        }
       #endregion
    }
}
