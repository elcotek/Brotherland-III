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
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.ServerProperties;
using DOL.Network;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

namespace DOL.GS
{
    /// <summary>
    /// Represents a single connection to the game server
    /// </summary>
    public class GameClient : BaseClient
    {
        #region eClientAddons enum

        /// <summary>
        /// The client addons enum
        /// </summary>
        [Flags]
        public enum eClientAddons
        {
            bit4 = 0x10,
            NewNewFrontiers = 0x20,
            Foundations = 0x40,
            NewFrontiers = 0x80,
        }

        #endregion

        #region eClientState enum

        /// <summary>
        /// Current state of the client
        /// </summary>
        public enum eClientState
        {
            NotConnected = 0x00,
            Connecting = 0x01,
            CharScreen = 0x02,
            WorldEnter = 0x03,
            Playing = 0x04,
            Linkdead = 0x05,
            Disconnected = 0x06,
        };

        #endregion

        #region eClientType enum

        /// <summary>
        /// The client software type enum
        /// </summary>
        public enum eClientType
        {
            Unknown = -1,
            Classic = 1,
            ShroudedIsles = 2,
            TrialsOfAtlantis = 3,
            Catacombs = 4,
            DarknessRising = 5,
            LabyrinthOfTheMinotaur = 6,
        }

        #endregion

        #region eClientVersion enum

        /// <summary>
        /// the version enum
        /// </summary>
        public enum eClientVersion
        {
            VersionNotChecked = -1,
            VersionUnknown = 0,
            _FirstVersion = 168,
            Version168 = 168,
            Version169 = 169,
            Version170 = 170,
            Version171 = 171,
            Version172 = 172,
            Version173 = 173,
            Version174 = 174,
            Version175 = 175,
            Version176 = 176,
            Version177 = 177,
            Version178 = 178,
            Version179 = 179,
            Version180 = 180,
            Version181 = 181,
            Version182 = 182,
            Version183 = 183,
            Version184 = 184,
            Version185 = 185,
            Version186 = 186,
            Version187 = 187,
            Version188 = 188,
            Version189 = 189,
            Version190 = 190,
            Version191 = 191,
            Version192 = 192,
            Version193 = 193,
            Version194 = 194,
            Version195 = 195,
            Version196 = 196,
            Version197 = 197,
            Version198 = 198,
            Version199 = 199,
            Version1100 = 1100,
            Version1101 = 1101,
            Version1102 = 1102,
            Version1103 = 1103,
            Version1104 = 1104,
            Version1105 = 1105,
            Version1106 = 1106,
            Version1107 = 1107,
            Version1108 = 1108,
            Version1109 = 1109,
            Version1110 = 1110,
            Version1111 = 1111,
            Version1112 = 1112,
            Version1113 = 1113,
            Version1114 = 1114,
            Version1115 = 1115,
            Version1116 = 1116,
            Version1117 = 1117,
            Version1118 = 1118,
            Version1119 = 1119,
            Version1120 = 1120,
            Version1121 = 1121,
            Version1122 = 1122,
            Version1123 = 1123,
            Version1124 = 1124,
            Version1125 = 1125,
            Version1126 = 1126,
            _LastVersion = 1126,
        }

        #endregion

        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// This variable holds the accountdata
        /// </summary>
        protected Account m_account;

        /// <summary>
        /// This variable holds the active charindex
        /// </summary>
        protected int m_activeCharIndex;

        /// <summary>
        /// Holds installed client addons
        /// </summary>
        protected eClientAddons m_clientAddons;

        /// <summary>
        /// Holds the current clientstate
        /// </summary>
        protected volatile eClientState m_clientState = eClientState.NotConnected;

        /// <summary>
        /// Holds client software type
        /// </summary>
        protected eClientType m_clientType = eClientType.Unknown;

        protected eClientVersion m_clientVersion;

        /// <summary>
        /// Holds the time of the last UDP ping
        /// </summary>
        protected string m_localIP = String.Empty;

        /// <summary>
        /// The packetsender of this client
        /// </summary>
        protected IPacketLib m_packetLib;

        /// <summary>
        /// The packetreceiver of this client
        /// </summary>
        protected PacketProcessor m_packetProcessor;

        /// <summary>
        /// Holds the time of the last ping
        /// </summary>
        protected long m_pingTime = DateTime.Now.Ticks; // give ping time on creation

        /// <summary>
        /// This variable holds all info about the active player
        /// </summary>
        protected GamePlayer m_player;

        /// <summary>
        /// This variable holds the sessionid
        /// </summary>
        protected int m_sessionID;

        /// <summary>
        /// This variable holds the UDP endpoint of this client
        /// </summary>
        protected volatile bool m_udpConfirm;

        /// <summary>
        /// This variable holds the UDP endpoint of this client
        /// </summary>
        protected IPEndPoint m_udpEndpoint;

        /// <summary>
        /// Holds the time of the last UDP ping
        /// </summary>
        protected long m_udpPingTime = DateTime.Now.Ticks;

        /// <summary>
        /// Holds the Player Collection of Updated Object with last update time.
        /// </summary>
        protected ConcurrentDictionary<Tuple<ushort, ushort>, long> m_GameObjectUpdateArray;

        /// <summary>
        /// Holds the Player Collection of Updated House with last update time.
        /// </summary>
        protected ReaderWriterDictionary<Tuple<ushort, ushort>, long> m_HouseUpdateArray;

        // Trainer window Cache, (Object Type, Object ID) => Skill
        public List<Tuple<Specialization, List<Tuple<int, int, Skill>>>> TrainerSkillCache = null;

        // Tooltip Request Time Cache, (Object Type => (Object ID => expires))
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, long>> m_tooltipRequestTimes = new ConcurrentDictionary<int, ConcurrentDictionary<int, long>>();

        /// <summary>
        /// Try to Send Tooltip to Client, return false if cache hit.
        /// Return true and register cache before you can send tooltip !
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool CanSendTooltip(int type, int id)
        {
            m_tooltipRequestTimes.TryAdd(type, new ConcurrentDictionary<int, long>());

            // Queries cleanup
            foreach (Tuple<int, int> keys in m_tooltipRequestTimes.SelectMany(e => e.Value.Where(it => it.Value < GameTimer.GetTickCount()).Select(el => new Tuple<int, int>(e.Key, el.Key))))
            {
                m_tooltipRequestTimes[keys.Item1].TryRemove(keys.Item2, out long dummy);
            }

            // Query hit ?
            if (m_tooltipRequestTimes[type].ContainsKey(id))
                return false;

            // Query register
            m_tooltipRequestTimes[type].TryAdd(id, GameTimer.GetTickCount() + 3600000);
            return true;
        }


        /// <summary>
        /// Constructor for a game client
        /// </summary>
        /// <param name="srvr">The server that's communicating with this client</param>
        public GameClient(BaseServer srvr)
            : base(srvr)
        {
            m_clientVersion = eClientVersion.VersionNotChecked;
            m_player = null;
            m_activeCharIndex = -1; //No character loaded yet!
            m_GameObjectUpdateArray = new ConcurrentDictionary<Tuple<ushort, ushort>, long>();
            m_HouseUpdateArray = new ReaderWriterDictionary<Tuple<ushort, ushort>, long>();
        }

        /// <summary>
        /// UDP address for this client
        /// </summary>
        public IPEndPoint UdpEndPoint
        {
            get { return m_udpEndpoint; }
            set { m_udpEndpoint = value; }
        }

        /// <summary>
        /// Gets or sets the client state
        /// </summary>
        public eClientState ClientState
        {
            get { return m_clientState; }
            set
            {
                eClientState oldState = m_clientState;

                // refresh ping timeouts immediately when we change into playing state or charscreen
                if ((oldState != eClientState.Playing && value == eClientState.Playing) ||
                    (oldState != eClientState.CharScreen && value == eClientState.CharScreen))
                {
                    PingTime = DateTime.Now.Ticks;
                }

                m_clientState = value;
                GameEventMgr.Notify(GameClientEvent.StateChanged, this);
                //DOLConsole.WriteSystem("New State="+value.ToString());
            }
        }

        /// <summary>
        /// Variable is false if account/player is Ban, for a wrong password, if server is closed etc ... 
        /// </summary>
        public bool IsConnected = true;

        /// <summary>
        /// Gets whether or not the client is playing
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                //Linkdead players also count as playing :)
                return m_clientState == eClientState.Playing || m_clientState == eClientState.Linkdead;
            }
        }

        /// <summary>
        /// Gets or sets the account being used by this client
        /// </summary>
        public Account Account
        {
            get { return m_account; }
            set
            {
                m_account = value;
                GameEventMgr.Notify(GameClientEvent.AccountLoaded, this);
            }
        }

        public string UniqueID;
        private bool _doubleAccountCache = false;
        private long _doubleAccountCacheDate = 0;
        public bool IsDoubleAccount
        {
            get
            {
                if (_doubleAccountCacheDate < DateTime.Now.Ticks)
                {
                    _doubleAccountCache = WorldMgr.GetAllPlayingClients().Where(cl => cl.UniqueID == UniqueID).Count() >= 2;
                    _doubleAccountCacheDate = DateTime.Now.Ticks + 30 * 60 * 1000 * 10000L;
                }
                return _doubleAccountCache;
            }
        }


        /// <summary>
        /// Gets or sets the player this client is using
        /// </summary>
        public GamePlayer Player
        {
            get { return m_player; }
            set
            {
                GamePlayer oldPlayer = Interlocked.Exchange(ref m_player, value);
                if (oldPlayer != null)
                {
                    oldPlayer.Delete();
                   
                }

                GameEventMgr.Notify(GameClientEvent.PlayerLoaded, this); // hmm seems not right
            }
        }

        /// <summary>
        /// Gets or sets the character index for the player currently being used
        /// </summary>
        public int ActiveCharIndex
        {
            get { return m_activeCharIndex; }
            set { m_activeCharIndex = value; }
        }

        /// <summary>
        /// Gets or sets the session ID for this client
        /// </summary>
        public int SessionID
        {
            get { return m_sessionID; }
            internal set { m_sessionID = value; }
        }

        /// <summary>
        /// Gets/Sets the time of last ping packet
        /// </summary>
        public long PingTime
        {
            get { return m_pingTime; }
            set { m_pingTime = value; }
        }

        /// <summary>
        /// UDP address for this client
        /// </summary>
        public string LocalIP
        {
            get { return m_localIP; }
            set { m_localIP = value; }
        }

        /// <summary>
        /// Gets/Sets the time of last UDP ping packet
        /// </summary>
        public long UdpPingTime
        {
            get { return m_udpPingTime; }
            set { m_udpPingTime = value; }
        }

        /// <summary>
        /// UDP confirm flag from this client
        /// </summary>
        public bool UdpConfirm
        {
            get { return m_udpConfirm; }
            set { m_udpConfirm = value; }
        }

        /// <summary>
        /// Gets or sets the packet sender
        /// </summary>
        public IPacketLib Out
        {
            get { return m_packetLib; }
            set { m_packetLib = value; }
        }

        /// <summary>
        /// Gets or Sets the packet receiver
        /// </summary>
        public PacketProcessor PacketProcessor
        {
            get { return m_packetProcessor; }
            set { m_packetProcessor = value; }
        }

        /// <summary>
        /// the version of this client
        /// </summary>
        public eClientVersion Version
        {
            get { return m_clientVersion; }
            set { m_clientVersion = value; }
        }

        /// <summary>
        /// Gets/sets client software type (classic/SI/ToA/Catacombs)
        /// </summary>
        public eClientType ClientType
        {
            get { return m_clientType; }
            set { m_clientType = value; }
        }

        public string MinorRev = "";
        public byte MajorBuild = 0;
        public byte MinorBuild = 0;

        /// <summary>
        /// Gets/sets installed client addons (housing/new frontiers)
        /// </summary>
        public eClientAddons ClientAddons
        {
            get { return m_clientAddons; }
            set { m_clientAddons = value; }
        }

        /// <summary>
        /// Get the Game Object Update Array (Read/Write)
        /// </summary>
        public ConcurrentDictionary<Tuple<ushort, ushort>, long> GameObjectUpdateArray
        {
            get { return m_GameObjectUpdateArray; }
        }

        /// <summary>
        /// Get the House Update Array (Read/Write)
        /// </summary>
        public ReaderWriterDictionary<Tuple<ushort, ushort>, long> HouseUpdateArray
        {
            get { return m_HouseUpdateArray; }
        }

        /// <summary>
        /// Called when a packet has been received.
        /// </summary>
        /// <param name="numBytes">The number of bytes received</param>
        /// <remarks>This function parses the incoming data into individual packets and then calls the appropriate handler.</remarks>
        protected override void OnReceive(int numBytes)
        {
            //This is the first received packet ...
            if (Version == eClientVersion.VersionNotChecked)
            {
                //Disconnect if the packet seems wrong
                if (numBytes < 17) // 17 is correct bytes count for 0xF4 packet
                {
                    if (log.IsWarnEnabled)
                    {
                        log.WarnFormat("Disconnected {0} in login phase because wrong packet size {1}", TcpEndpoint, numBytes.ToString());
                        log.Warn("numBytes=" + numBytes.ToString());
                        log.Warn(Marshal.ToHexDump("packet buffer:", _pBuf, 0, numBytes));
                    }
                    GameServer.Instance.Disconnect(this);
                    return;
                }

                int version;

                /// <summary>
                /// The First Packet Format Change after 1.115c
                /// If "numbytes" is below 19 we have a pre-1.115c packet !
                /// </summary>
                if (numBytes < 19)
                {
                    //Currently, the version is sent with the first packet, no
                    //matter what packet code it is
                    //Console.Out.WriteLine("_pBuf[12] =  {0}, {1}, {2}", Convert.ToString(_pBuf[12]), Convert.ToString(_pBuf[13]), Convert.ToString(_pBuf[14]));
                    version = (_pBuf[12] * 100) + (_pBuf[13] * 10) + _pBuf[14];
                    //Console.Out.WriteLine("numBytes =  {0}", Convert.ToString(numBytes));
                    //Console.Out.WriteLine("version1 =  {0}", Convert.ToString(version));

                    // we force the versionning: 200 correspond to 1.100 (1100)
                    // thus we could handle logically packets with version number based on the client version
                    if (version >= 200) version += 900;
                }
                else
                {
                    // post 1.115c
                    // first byte is major (1), second byte is minor (1), third byte is version (15)
                    // revision (c) is also coded in ascii after that, then a build number appear using two bytes (0x$$$$)
                    version = _pBuf[11] * 1000 + _pBuf[12] * 100 + _pBuf[13];
                    //Console.Out.WriteLine("version2 =  {0}", Convert.ToString(version));
                }

                IPacketLib lib = AbstractPacketLib.CreatePacketLibForVersion(version, this, out eClientVersion ver);

                if (lib == null)
                {
                    Version = eClientVersion.VersionUnknown;
                    if (log.IsWarnEnabled)
                        log.Warn(TcpEndpointAddress + " client Version " + version.ToString() + " not handled on this server!");
                    GameServer.Instance.Disconnect(this);
                }
                else
                {
                    log.Info("Incoming connection from " + TcpEndpointAddress + " using client version " + version.ToString());
                    Version = ver;
                    Out = lib;
                    PacketProcessor = new PacketProcessor(this);
                }
            }

            if (Version != eClientVersion.VersionUnknown)
            {
                m_packetProcessor.ReceiveBytes(numBytes);
            }
        }

        /// <summary>
        /// Called when this client has been disconnected
        /// </summary>
        public override void OnDisconnect()
        {
            try
            {
                PacketProcessor?.OnDisconnect(); // Verwendung des null-conditional Operators

                if (ClientState == eClientState.Playing)
                {
                    OnLinkdeath();
                    return;
                }

                if (ClientState == eClientState.WorldEnter && Player != null)
                {
                    Player.SaveIntoDatabase();
                }
            }
            catch (Exception e)
            {
                if (log.IsErrorEnabled)
                {
                    log.Error("OnDisconnect", e);
                }
            }
            finally
            {
                Quit(); // Immer sicherstellen, dass der Client getrennt wird
            }
        }

        /// <summary>
        /// Called when this client has connected
        /// </summary>
        public override void OnConnect()
        {
            GameEventMgr.Notify(GameClientEvent.Connected, this);
        }

        public void LoadPlayer(int accountindex)
        {
            LoadPlayer(accountindex, Properties.PLAYER_CLASS);
        }

        /// <summary>
        /// Loads a player from the DB
        /// </summary>
        /// <param name="accountindex">Index of the character within the account</param>
        public void LoadPlayer(int accountindex, string playerClass)
        {
            m_activeCharIndex = accountindex;
            GamePlayer player = null;

            // refreshing Account to load any changes from the DB
            GameServer.Database.FillObjectRelations(m_account);

            DOLCharacters dolChar = m_account.Characters[m_activeCharIndex];
            Assembly gasm = Assembly.GetAssembly(typeof(GameServer));

            player = CreatePlayerInstance(gasm, playerClass, dolChar) ?? CreatePlayerFromScripts(playerClass, dolChar);

            if (player == null)
            {
                log.ErrorFormat("Could not instantiate player class '{0}', using GamePlayer instead!", playerClass);
                player = new GamePlayer(this, dolChar);
            }

            Thread.MemoryBarrier();
            Player = player;
        }

        private GamePlayer CreatePlayerInstance(Assembly gasm, string playerClass, DOLCharacters dolChar)
        {
            try
            {
                return (GamePlayer)gasm.CreateInstance(playerClass, false, BindingFlags.CreateInstance, null, new object[] { this, dolChar }, null, null);
            }
            catch (Exception e)
            {
                if (log.IsErrorEnabled)
                    log.Error("LoadPlayer", e);
                return null;
            }
        }

        private GamePlayer CreatePlayerFromScripts(string playerClass, DOLCharacters dolChar)
        {
            foreach (Assembly asm in ScriptMgr.Scripts)
            {
                try
                {
                    return (GamePlayer)asm.CreateInstance(playerClass, false, BindingFlags.CreateInstance, null, new object[] { this, dolChar }, null, null);
                }
                catch (Exception e)
                {
                    if (log.IsErrorEnabled)
                        log.Error("LoadPlayer", e);
                }
            }
            return null;
        }

        /// <summary>
        /// Saves a player to the DB
        /// </summary>
        public void SavePlayer()
        {
            try
            {
                if (m_activeCharIndex != -1 && m_player != null)
                {
                    m_player.SaveIntoDatabase();
                }
            }
            catch (Exception e)
            {
                if (log.IsErrorEnabled)
                    log.Error("SavePlayer", e);
            }
        }

        /// <summary>
        /// Called when a player goes linkdead
        /// </summary>
        protected void OnLinkdeath()
        {
            if (log.IsDebugEnabled)
                log.Debug("Linkdeath called (" + Account.Name + ")  client state=" + ClientState.ToString());

            //If we have no sessionid we simply disconnect
            GamePlayer curPlayer = Player;
            if (m_sessionID == 0 || curPlayer == null)
            {
                Quit();
            }
            else
            {
                //ANTIHACK: Player was porting 
                curPlayer.TempProperties.setProperty(GamePlayer.PlayerPort, this);


                ClientState = eClientState.Linkdead;
                // If we have a good sessionid, we won't remove the client yet!
                // OnLinkdeath() can start a timer to remove the client "a bit later"
                curPlayer.OnLinkdeath();
            }
        }


        /// <summary>
        /// Quits a client from the world
        /// </summary>
        protected internal void Quit()
        {
            lock (this)
            {
                try
                {
                    eClientState oldClientState = ClientState;
                    if (m_sessionID != 0)
                    {
                        if (oldClientState == eClientState.Playing || oldClientState == eClientState.WorldEnter ||
                            oldClientState == eClientState.Linkdead)
                        {
                            try
                            {
                                Player?.Quit(true); // sichere Verwendung von null-conditional Operator
                            }
                            catch (Exception e)
                            {
                                log.Error("player cleanup on client quit", e);
                            }
                        }

                        try
                        {
                            WorldMgr.RemoveClient(this);
                        }
                        catch (Exception e)
                        {
                            log.Error("client cleanup on quit", e);
                        }
                    }

                    ClientState = eClientState.Disconnected;
                    Player = null;

                    DateTime date = DateTime.UtcNow.AddMinutes(ServerProperties.Properties.LOGIN_OTHER_RELAM_TIME);
                    int time = date.Minute;
                    GameEventMgr.Notify(GameClientEvent.Disconnected, this);

                    if (Account != null)
                    {
                        IList<Account> PlayerAccountID = GameServer.Database.SelectObjects<Account>("`AccountID` = @AccountID", new QueryParameter("@AccountID", Account.AccountID));

                        if (PlayerAccountID != null)
                        {
                            try
                            {
                                foreach (Account db in PlayerAccountID)
                                {
                                    if (db.PrivLevel == 1 && db.LoginWaitTime >= DateTime.UtcNow && !string.IsNullOrEmpty(Account.AccountID) && !db.IsOtherRealmAllowed && ServerProperties.Properties.LOGIN_OTHER_RELAM_SWITCH_TIMER_MINUTES > 0)
                                    {
                                        if (db.LoginWaitTime.Minute != time && db.AccountID == Account.AccountID)
                                        {
                                            for (int i = 0; i < (byte)db.LastLoginRealm; i++)
                                            {
                                                db.Realm = db.LastLoginRealm;
                                            }
                                            GameServer.Database.SaveObject(db);
                                        }
                                    }
                                }
                                PlayerAccountID.Clear();
                            }
                            catch 
                            {
                                //log.Error("GameClient: Cannot save new realm at the moment!");
                            }
                        }

                        log.Info($"({TcpEndpoint}) {Account.Name} just disconnected!");

                        AuditMgr.AddAuditEntry(this, AuditType.Account, AuditSubtype.AccountLogout, "", Account.Name);
                    }
                }
                catch (Exception e)
                {
                    log.Error("Quit", e);
                }
            }
        }

        /// <summary>
        /// Returns short informations about the client
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return new StringBuilder(128)
                .Append(Version.ToString())
                .Append(" pakLib:").Append(Out == null ? "(null)" : Out.GetType().FullName)
                .Append(" type:").Append(ClientType.ToString())
                .Append('(').Append(ClientType).Append(')')
                .Append(" addons:").Append(ClientAddons.ToString("G"))
                .Append(" state:").Append(ClientState.ToString())
                .Append(" IP:").Append(TcpEndpoint)
                .Append(" session:").Append(SessionID)
                .Append(" acc:").Append(Account == null ? "null" : Account.Name)
                .Append(" char:").Append(Player == null ? "null" : Player.Name)
                .Append(" class:").Append(Player == null ? "null" : Player.CharacterClass.ID.ToString())
                .ToString();
        }
    }
}
