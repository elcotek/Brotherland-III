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
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;
using System;
using System.Threading;

namespace DOL.GS
{
    /// <summary>
    /// GameDoor is class for regular door
    /// </summary>
    public class GameDoor : GameLiving, IDoor
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        private bool m_openDead = false;
        private static Timer m_timer;
        protected volatile uint m_lastUpdateTickCount = uint.MinValue;
        private readonly object m_LockObject = new object();
        private uint m_flags = 0;

        public bool IsVisibleTo(GamePlayer player)
        {
            // Beispiel-Logik, um die Sichtbarkeit zu bestimmen
            return player != null; // Beispielbedingung
        }

        /// <summary>
		/// The time interval after which door will be closed, in milliseconds
		/// On live this is usually 5 seconds
		/// </summary>
		protected const int CLOSE_GateDOOR_TIME = 300000 * 5; //15 min

        /// <summary>
        /// The time interval after which door will be closed, in milliseconds
        /// On live this is usually 5 seconds
        /// </summary>
        protected const int CLOSE_DOOR_TIME = 8000;

        /// <summary>
        /// The timed action that will close the door
        /// </summary>
        protected GameTimer m_closeDoorAction;

        /// <summary>
		/// The Interacteffect duration in milliseconds
		/// </summary>
		public const int OpenDuration = 1500;

        /// <summary>
        /// Creates a new GameDoor object
        /// </summary>
        public GameDoor()
            : base()
        {
            m_state = eDoorState.Closed;
            m_model = 0xFFFF;
        }

        /// <summary>
        /// Loads this door from a door table slot
        /// </summary>
        /// <param name="obj">DBDoor</param>
        public override void LoadFromDatabase(DataObject obj)
        {
            base.LoadFromDatabase(obj);
            DBDoor m_dbdoor = obj as DBDoor;
            if (m_dbdoor == null) return;
            Zone curZone = WorldMgr.GetZone((ushort)(m_dbdoor.InternalID / 1000000));
            if (curZone == null) return;
            this.CurrentRegion = curZone.ZoneRegion;
            m_name = m_dbdoor.Name;
            m_Heading = (ushort)m_dbdoor.Heading;
            m_x = m_dbdoor.X;
            m_y = m_dbdoor.Y;
            m_z = m_dbdoor.Z;
            m_level = 0;
            m_model = 0xFFFF;
            m_doorID = m_dbdoor.InternalID;
            m_guildName = m_dbdoor.Guild;
            m_Realm = (eRealm)m_dbdoor.Realm;
            m_level = m_dbdoor.Level;
            m_health = m_dbdoor.MaxHealth;
            m_maxHealth = m_dbdoor.MaxHealth;
            m_locked = m_dbdoor.Locked;
            m_flags = m_dbdoor.Flags;

            this.AddToWorld();
        }
        /// <summary>
        /// save this door to a door table slot
        /// </summary>
        public override void SaveIntoDatabase()
        {
            DBDoor obj = null;
            if (InternalID != null)
                obj = GameServer.Database.FindObjectByKey<DBDoor>(InternalID);
            if (obj == null)
                obj = new DBDoor();
            obj.Name = this.Name;
            obj.InternalID = this.DoorID;
            obj.Type = DoorID / 100000000;
            obj.Guild = this.GuildName;
            obj.Flags = this.Flag;
            obj.Realm = (byte)this.Realm;
            obj.Level = this.Level;
            obj.MaxHealth = this.MaxHealth;
            obj.Health = this.MaxHealth;
            obj.Locked = this.Locked;
            if (InternalID == null)
            {
                GameServer.Database.AddObject(obj);
                InternalID = obj.ObjectId;
            }
            else
                GameServer.Database.SaveObject(obj);
        }

        #region Properties

        private int m_locked;
        /// <summary>
        /// door open = 0 / lock = 1 
        /// </summary>
        public virtual int Locked
        {
            get { return m_locked; }
            set { m_locked = value; }
        }

        /// <summary>
        /// this hold the door index which is unique
        /// </summary>
        private int m_doorID;

        /// <summary>
        /// door index which is unique
        /// </summary>
        public virtual int DoorID
        {
            get { return m_doorID; }
            set { m_doorID = value; }
        }

        /// <summary>
        /// Get the ZoneID of this door
        /// </summary>
        public virtual ushort ZoneID
        {
            get { return (ushort)(DoorID / 1000000); }
        }

        private int m_type;

        /// <summary>
        /// Door Type
        /// </summary>
        public virtual int Type
        {
            get { return m_type; }
            set { m_type = value; }
        }

        /// <summary>
        /// This is used to identify what sound a door makes when open / close
        /// </summary>
        public virtual uint Flag
        {
            get { return m_flags; }
            set { m_flags = value; }
        }

        /// <summary>
        /// This hold the state of door
        /// </summary>
        protected eDoorState m_state;

        /// <summary>
        /// The state of door (open or close)
        /// </summary>
        public virtual eDoorState State
        {
            get { return m_state; }
            set
            {
                if (m_state != value)
                {
                    lock (m_LockObject)
                    {
                        m_state = value;
                        BroadcastDoorStatus();
                    }
                }
            }
        }

        #endregion
        protected bool IsRelicGate(GameDoor door)
        {
            door = this;

            if (door != null)
            {
                switch (door.DoorID)
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


                    //Toa Talos Door
                    case 87159201: //Talo Albion
                    case 144159201: //Talo Hibernia
                    case 44159201: //Talo  Midgard


                    //Relic Gates
                    case 173000301:
                    case 173000201:
                    case 174061701:
                    case 174149601:
                    case 170128101:
                    case 170127801:
                    case 169000501:
                    case 169000301:
                    case 176233801:

                    case 176233901:
                    case 175014001:
                    case 175013901:


                        {
                            lock (m_LockObject)
                            {
                                if (m_closeDoorAction == null)
                                {
                                    m_closeDoorAction = new CloseDoorAction(door);
                                }

                                m_closeDoorAction.Start(CLOSE_GateDOOR_TIME);

                            }
                            return true;
                        }


                    default: return false;

                }

            }
            return false;
        }


        /// <summary>
        /// Call this function to open the door
        /// </summary>
        public virtual void Open()
        {
            if (Locked == 0)
            {
                this.State = eDoorState.Open;

                new Effects.GameDoorInteractEffect().Start(this);
                if ((HealthPercent > 40 || !m_openDead) && IsRelicGate(this) == false)
                {
                    lock (m_LockObject)
                    {
                        if (m_closeDoorAction == null)
                        {
                            m_closeDoorAction = new CloseDoorAction(this);
                        }


                        m_closeDoorAction.Start(CLOSE_DOOR_TIME);

                    }
                }
            }
        }

        public virtual byte Status
		{
			get
			{
			//	if( this.HealthPercent == 0 ) return 0x01;//broken
				return 0x00;
			}
		}
        
        /// <summary>
        /// Call this function to close the door
        /// </summary>
        public virtual void Close()
		{
			if (!m_openDead)
				this.State = eDoorState.Closed;
			m_closeDoorAction = null;
		}

		/// <summary>
		/// Allow a NPC to manipulate the door
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="open"></param>
        public virtual void NPCManipulateDoorRequest(GameNPC npc, bool open)
		{
			npc.TurnTo(this.X, this.Y);
			if (open && m_state != eDoorState.Open)
				this.Open();
			else if (!open && m_state != eDoorState.Closed)
				this.Close();

		}
		
		public override int Health
		{
			get { return m_health; }
			set
			{

				int maxhealth = MaxHealth;
				if( value >= maxhealth )
				{
					m_health = maxhealth;
                    _XPGainers.Clear();
                }
				else if( value > 0 )
				{
					m_health = value;
				}
				else
				{
					m_health = 0;
				}

				if( IsAlive && m_health < maxhealth )
				{
					StartHealthRegeneration( );
				}
			}
		}
		
		/// <summary>
		/// Get the solidity of the door
		/// </summary>
		public override int MaxHealth
		{
			get {	return 5 * GetModified(eProperty.MaxHealth);}
		}
		
		/// <summary>
		/// No regeneration over time of the door
		/// </summary>
		/// <param name="killer"></param>
		public override void Die(GameObject killer)
		{
			base.Die(killer);
			StartHealthRegeneration();
            BroadcastDoorStatus();
        }

        /// <summary>
		/// boradcast the door status to all player near the door
		/// </summary>
		public virtual void BroadcastDoorStatus()
        {
            foreach (GameClient client in WorldMgr.GetClientsOfRegion(CurrentRegionID))
            {
                client.Player.SendDoorUpdate(this);
            }
        }


        private static long m_healthregentimer = 0;

        public virtual void RegenDoorHealth()
		{
			Health = 0;
			if (Locked == 0)
				Open();
			
			m_healthregentimer = 9999;
			m_timer = new Timer(new TimerCallback(StartHealthRegen), null, 0, 1000);

		}

        public virtual void StartHealthRegen(object param)
		{
			if (HealthPercent >= 40)
			{
				m_timer.Dispose( );
				m_openDead = false;
				Close( );
				return;
			}
				
			if (Health == MaxHealth)
			{
				m_timer.Dispose( );
				m_openDead = false;
				Close();
				return;
			}

			if( m_healthregentimer <= 0 )
			{
				m_timer.Dispose();
				m_openDead = false;
				Close( );
				return;
			}
			this.Health += this.Level*2;
			m_healthregentimer -= 10;
		}

		public override void TakeDamage ( GameObject source, eDamageType damageType, int damageAmount, int criticalAmount )
		{
			
			if( !m_openDead && this.Realm != eRealm.Door )
			{
				base.TakeDamage(source, damageType, damageAmount, criticalAmount);

				double damageDealt = damageAmount + criticalAmount;
			}
				
			GamePlayer attackerPlayer = source as GamePlayer;
			if( attackerPlayer != null)
			{
				if( !m_openDead && this.Realm != eRealm.Door )
				{
                    attackerPlayer.Out.SendMessage(LanguageMgr.GetTranslation(attackerPlayer.Client.Account.Language, "GameDoor.NowOpen", Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                }
                if ( !m_openDead && this.Realm != eRealm.Door )
				{
					Health -= damageAmount + criticalAmount;
			
					if( !IsAlive )
					{
                        attackerPlayer.Out.SendMessage(LanguageMgr.GetTranslation(attackerPlayer.Client.Account.Language, "GameDoor.NowOpen", Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        Die(source);
						m_openDead = true;
						RegenDoorHealth();
						if( Locked == 0 )
							Open( );
								
						Group attackerGroup = attackerPlayer.Group;
						if( attackerGroup != null )
						{
							foreach( GameLiving living in attackerGroup.GetMembersInTheGroup( ) )
							{
                                ((GamePlayer)living).Out.SendMessage(LanguageMgr.GetTranslation(attackerPlayer.Client.Account.Language, "GameDoor.NowOpen", Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                        }
					}
				}
			}
		}
		/// <summary>
		/// The action that closes the door after specified duration
		/// </summary>
		protected class CloseDoorAction : RegionAction
		{
			/// <summary>
			/// Constructs a new close door action
			/// </summary>
			/// <param name="door">The door that should be closed</param>
			public CloseDoorAction(GameDoor door)
				: base(door)
			{
			}

			/// <summary>
			/// This function is called to close the door 10 seconds after it was opened
			/// </summary>
			protected override void OnTick()
			{
				GameDoor door = (GameDoor)m_actionSource;
				door.Close();
			}
		}
	}
}
