
using DOL.Database;
using DOL.Events;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.Relics;
using DOL.Language;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;



namespace DOL.GS
{
    public class GameRelicPad : GameObject, IDoor
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        const int PAD_AREA_RADIUS = 250;

        PadArea m_area = null;
        GameRelic m_mountedRelic = null;

        #region constructor
        public GameRelicPad()
            : base()
        {
            this.State = eDoorState.Closed;
        }
        #endregion

        public bool IsVisibleTo(GamePlayer player)
        {
            // Beispiel-Logik, um die Sichtbarkeit zu bestimmen
            return player != null; // Beispielbedingung
        }

        protected int m_doorID;
        /// <summary>
        /// The door index which is unique
        /// </summary>
        public int DoorID
        {
            get { return m_doorID; }
            set { m_doorID = value; }
        }

        private int m_locked;
        /// <summary>
        /// door open = 0 / lock = 1 
        /// </summary>
        public virtual int Locked
        {
            get { return m_locked; }
            set { m_locked = value; }
        }

        private uint m_flags = 0;
        /// <summary>
		/// Get the ZoneID of this door
		/// </summary>
		public ushort ZoneID
        {
            get { return (ushort)(DoorID / 1000000); }
        }

        public int OwnerKeepID
        {
            get { return (m_doorID / 100000) % 1000; }
        }




        public int ComponentID
        {
            get { return (m_doorID / 100) % 100; }
        }

        public int DoorIndex
        {
            get { return m_doorID % 10; }
        }

        /// <summary>
        /// This flag is send in packet(keep door = 4, regular door = 0)
        /// </summary>
        public uint Flag
        {
            get
            {
                return m_flags;
            }
        }




        #region Add/remove from world



        /// <summary>
        /// add the relicpad to world
        /// </summary>
        /// <returns></returns>
        public override bool AddToWorld()
        {
            m_area = new PadArea(this);
            CurrentRegion.AddArea(m_area);

            bool success = base.AddToWorld();
            if (success)
            {
                /*
				 * <[RF][BF]Cerian> mid: mjolnerr faste (str)
					<[RF][BF]Cerian> mjollnerr
					<[RF][BF]Cerian> grallarhorn faste (magic)
					<[RF][BF]Cerian> alb: Castle Excalibur (str)
					<[RF][BF]Cerian> Castle Myrddin (magic)
					<[RF][BF]Cerian> Hib: Dun Lamfhota (str), Dun Dagda (magic)
				 */
                //Name = GlobalConstants.RealmToName((DOL.GS.PacketHandler.eRealm)Realm)+ " Relic Pad";
                RelicMgr.AddRelicPad(this);
            }
            return success;
        }


        public override eRealm Realm
        {
            get
            {
                switch (DoorID)
                {

                    //Alb relict tempel
                    case 175135301:
                    case 175135302:
                    case 175135303:
                    case 176130401:
                    case 176130402:
                    case 176130403:
                        return eRealm.Albion;
                    //Mid relict tempel
                    case 169262401:
                    case 169262402:
                    case 169262403:
                    case 170347601:
                    case 170347602:
                    case 170347603:
                        return eRealm.Midgard;
                    //Hib relict tempel
                    case 173381701:
                    case 173381702:
                    case 173381703:
                    case 174226401:
                    case 174226402:
                    case 174226403:
                        return eRealm.Hibernia;
                    default:
                        return eRealm.None;
                }
            }
            set
            {
                base.Realm = value;
            }
        }


        public virtual eRelicType PadType
        {
            get
            {
                switch (DoorID)
                {
                    //Mjollner Mid str
                    case 170347601:
                    case 170347602:
                    case 170347603:

                    //Lamfhota Hib str
                    case 173381701:
                    case 173381702:
                    case 173381703:

                    //Excalibur Alb str
                    case 176130401:
                    case 176130402:
                    case 176130403:
                        {
                            return eRelicType.Strength;
                        }
                    //Grallarhorn Mid power
                    case 169262401:
                    case 169262402:
                    case 169262403:

                    //Dun Dagta Hib power
                    case 174226401:
                    case 174226402:
                    case 174226403:


                    //Myrddin Albion power
                    case 175135301:
                    case 175135302:
                    case 175135303:
                        {
                            return eRelicType.Magic;
                        }
                    default:
                        {
                            return eRelicType.Invalid;
                        }
                }
            }
        }

        /// <summary>
        /// removes the relicpad from the world
        /// </summary>
        /// <returns></returns>
        public override bool RemoveFromWorld()
        {
            if (m_area != null)
                CurrentRegion.RemoveArea(m_area);

            return base.RemoveFromWorld();
        }
        #endregion





        public virtual bool RelicAllowedToStoreOnPillar(GameRelic relic)
        {
            string AllowedRelic;



            switch (DoorID)
            {
              
                //Excalibur Albion Pillar Strength
                case 176130402: //Midgard
                    {
                        if (relic.OriginalRealm == eRealm.Midgard)
                        {
                            return true;

                        }
                        else
                            return false;
                    }

                case 176130403: //hib
                    {
                        if (relic.OriginalRealm == eRealm.Hibernia)
                        {
                            return true;

                        }
                        else
                            return false;
                    }
                case 176130401:  //alb
                    {
                        AllowedRelic = "Scabbard of Excalibur";
                        if (relic.Name == AllowedRelic)
                        {
                            return true;
                        }
                        else
                            return false;
                    }

                //Myrddin Albion Pillar Power
                case 175135303: //hib
                    {
                        if (relic.OriginalRealm == eRealm.Hibernia)
                        {
                            return true;

                        }
                        else
                            return false;
                    }
                case 175135302://mid
                    {
                        if (relic.OriginalRealm == eRealm.Midgard)
                        {
                            return true;

                        }
                        else
                            return false;
                    }
                case 175135301://alb
                    {
                        AllowedRelic = "Merlins Staff";
                        if (relic.Name == AllowedRelic)
                        {
                            return true;
                        }
                        else
                            return false;
                    }

                //Mjollner Midgard Pillar Strength
                case 170347603://alb
                    {
                        if (relic.OriginalRealm == eRealm.Albion)
                        {
                            return true;

                        }
                        else
                            return false;
                    }
                case 170347602://hib
                    {
                        if (relic.OriginalRealm == eRealm.Hibernia)
                        {
                            return true;

                        }
                        else
                            return false;
                    }
                case 170347601: //mid
             
                    {
                        AllowedRelic = "Thors Hammer";
                        if (relic.Name == AllowedRelic)
                        {
                            return true;
                        }
                        else
                            return false;

                    }
                //Grallarhorn Mid power
                case 169262402: //hib
                    {
                        if (relic.OriginalRealm == eRealm.Hibernia)
                        {
                            return true;

                        }
                        else
                            return false;
                    }
                case 169262403://alb
                    {
                        if (relic.OriginalRealm == eRealm.Albion)
                        {
                            return true;

                        }
                        else
                            return false;
                    }
                case 169262401://mid
                    {
                        AllowedRelic = "Horn of Valhalla";
                        if (relic.Name == AllowedRelic)
                        {
                            return true;
                        }
                        else
                            return false;
                    }

                //Lamfhota Hibernia Pillar Strength
                case 173381703: //mid
                    {
                        if (relic.OriginalRealm == eRealm.Midgard)
                        {
                            return true;

                        }
                        else
                            return false;
                    }
                case 173381702://alb
                    {
                        if (relic.OriginalRealm == eRealm.Albion)
                        {
                            return true;

                        }
                        else
                            return false;
                    }
                case 173381701://hib
                    {
                        AllowedRelic = "Lughs Spear of Lightning";
                        if (relic.Name == AllowedRelic)
                        {
                            return true;
                        }
                        else
                            return false;
                    }
                //Dun Dagta Hibernia Pillar Power
                case 174226403: //mid
                    {
                        if (relic.OriginalRealm == eRealm.Midgard)
                        {
                            return true;

                        }
                        else
                            return false;
                    }
                case 174226402://alb
                    {
                        if (relic.OriginalRealm == eRealm.Albion)
                        {
                            return true;

                        }
                        else
                            return false;
                    }
                case 174226401://hib
                    {
                        AllowedRelic = "Cauldron of Dagda";
                        if (relic.Name == AllowedRelic)
                        {
                            return true;
                        }
                        else
                            return false;
                    }


                default: return false;
            }
        }

    /// <summary>
    /// Checks if a GameRelic is mounted at this GameRelicPad
    /// </summary>
    /// <param name="relic"></param>
    /// <returns></returns>
    public bool IsMountedHere(GameRelic relic)
		{
            

            return m_mountedRelic == relic;
		}

		public void MountRelic(GameRelic relic, bool returning)
		{
			m_mountedRelic = relic;


            if (relic.CurrentCarrier != null && returning == false)
			{
				/* Sending broadcast */
                GamePlayer player = relic.CurrentCarrier as GamePlayer;


                if ((player is GamePlayer) && (player.Group != null))
                {
                    foreach (GamePlayer players in player.Group.GetPlayersInTheGroup())
                    {
                        if (!players.IsWithinRadius(m_mountedRelic, 4000)) continue;

                        switch (players.Realm)
                        {
                            case eRealm.Albion:
                                {
                                    players.Out.SendMessage("you gain a little reward for defendig your homeland! ", DOL.GS.PacketHandler.eChatType.CT_SpellResisted, DOL.GS.PacketHandler.eChatLoc.CL_ChatWindow);
                                    players.GainBountyPoints(ServerProperties.Properties.RELIC_BP_RATE, false);
                                    players.GainRealmPoints(ServerProperties.Properties.RELIC_RP_RATE, false);
                                    players.UpdatePlayerStatus();
                                    players.Out.SendUpdatePlayer();
                                    players.SaveIntoDatabase();

                                    break;
                                }
                            case eRealm.Hibernia:
                                {
                                    players.Out.SendMessage("you gain a little reward for defendig your homeland! ", DOL.GS.PacketHandler.eChatType.CT_SpellResisted, DOL.GS.PacketHandler.eChatLoc.CL_ChatWindow);
                                    players.GainBountyPoints(ServerProperties.Properties.RELIC_BP_RATE, false);
                                    players.GainRealmPoints(ServerProperties.Properties.RELIC_RP_RATE, false);
                                    players.UpdatePlayerStatus();
                                    players.Out.SendUpdatePlayer();
                                    players.SaveIntoDatabase();
                                    break;
                                }
                            case eRealm.Midgard:
                                {
                                    players.Out.SendMessage("you gain a little reward for defendig your homeland! ", DOL.GS.PacketHandler.eChatType.CT_SpellResisted, DOL.GS.PacketHandler.eChatLoc.CL_ChatWindow);
                                    players.GainBountyPoints(ServerProperties.Properties.RELIC_BP_RATE, false);
                                    players.GainRealmPoints(ServerProperties.Properties.RELIC_RP_RATE, false);
                                    players.UpdatePlayerStatus();
                                    players.Out.SendUpdatePlayer();
                                    players.SaveIntoDatabase();
                                    break;
                                }
                        }
                    }
                }
                else
                {
                    switch (player.Realm)
                    {
                        case eRealm.Albion:
                            {

                                player.Out.SendMessage("you gain a little reward for defendig your homeland! ", DOL.GS.PacketHandler.eChatType.CT_SpellResisted, DOL.GS.PacketHandler.eChatLoc.CL_ChatWindow);
                                player.GainBountyPoints(ServerProperties.Properties.RELIC_BP_RATE, false);
                                player.GainRealmPoints(ServerProperties.Properties.RELIC_RP_RATE, false);
                                player.UpdatePlayerStatus();
                                player.Out.SendUpdatePlayer();
                                player.SaveIntoDatabase();
                                break;
                            }
                        case eRealm.Hibernia:
                            {
                                player.Out.SendMessage("you gain a little reward for defendig your homeland! ", DOL.GS.PacketHandler.eChatType.CT_SpellResisted, DOL.GS.PacketHandler.eChatLoc.CL_ChatWindow);
                                player.GainBountyPoints(ServerProperties.Properties.RELIC_BP_RATE, false);
                                player.GainRealmPoints(ServerProperties.Properties.RELIC_RP_RATE, false);
                                player.UpdatePlayerStatus();
                                player.Out.SendUpdatePlayer();
                                player.SaveIntoDatabase();
                                break;
                            }
                        case eRealm.Midgard:
                            {
                                player.Out.SendMessage("you gain a little reward for defendig your homeland! ", DOL.GS.PacketHandler.eChatType.CT_SpellResisted, DOL.GS.PacketHandler.eChatLoc.CL_ChatWindow);
                                player.GainBountyPoints(ServerProperties.Properties.RELIC_BP_RATE, false);
                                player.GainRealmPoints(ServerProperties.Properties.RELIC_RP_RATE, false);
                                player.UpdatePlayerStatus();
                                player.Out.SendUpdatePlayer();
                                player.SaveIntoDatabase();
                                break;
                            }
                    }
                }


                /* Sending broadcast */
                string message = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameRelicPad.MountRelic.Stored", relic.CurrentCarrier.Name, GlobalConstants.RealmToName((eRealm)relic.CurrentCarrier.Realm), relic.Name, Name);
                foreach (GameClient cl in WorldMgr.GetAllPlayingClients())
                {
                    if (cl.Player.ObjectState != eObjectState.Active) continue;
                    cl.Out.SendMessage(LanguageMgr.GetTranslation(cl.Account.Language, "GameRelicPad.MountRelic.Captured", GlobalConstants.RealmToName((eRealm)relic.CurrentCarrier.Realm), relic.Name), eChatType.CT_ScreenCenterSmaller, eChatLoc.CL_SystemWindow);
                    cl.Out.SendMessage(message + "\n" + message + "\n" + message, eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                }
                NewsMgr.CreateNews(message, relic.CurrentCarrier.Realm, eNewsType.RvRGlobal, false);

                /* Increasing of CapturedRelics */
                //select targets to increase CapturedRelics
                //TODO increase stats

                List<GamePlayer> targets = new List<GamePlayer>();
                if (relic.CurrentCarrier.Group != null)
                {
                    foreach (GamePlayer p in relic.CurrentCarrier.Group.GetPlayersInTheGroup())
                    {
                        if (p == null) continue;

                        targets.Add(p);

                    }
                }
                else
                {

                    targets.Add(relic.CurrentCarrier);

                }
				foreach (GamePlayer target in targets)
				{
                    
					target.CapturedRelics++;
                   
                }

				Notify(RelicPadEvent.RelicMounted, this, new RelicPadEventArgs(relic.CurrentCarrier, relic));
			}
			else
			{
                // relic returned to pad, probably because it was dropped on ground and timer expired.
                string message = string.Format("The {0} has been returned to {1}.", relic.Name, Name);
                foreach (GameClient cl in WorldMgr.GetAllPlayingClients())
                {
                    if (cl.Player.ObjectState != eObjectState.Active) continue;
                    cl.Out.SendMessage(message + "\n" + message + "\n" + message, eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                }

			}
		}

		public void RemoveRelic(GameRelic relic)
		{
			m_mountedRelic = null;

			if (relic.CurrentCarrier != null)
			{
                string message = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameRelicPad.RemoveRelic.Removed", relic.CurrentCarrier.Name, GlobalConstants.RealmToName((eRealm)relic.CurrentCarrier.Realm), relic.Name, Name);
                foreach (GameClient cl in WorldMgr.GetAllPlayingClients())
                {
                    if (cl.Player.ObjectState != eObjectState.Active) continue;
                    cl.Out.SendMessage(message + "\n" + message + "\n" + message, eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                }
                NewsMgr.CreateNews(message, relic.CurrentCarrier.Realm, eNewsType.RvRGlobal, false);

                Notify(RelicPadEvent.RelicStolen, this, new RelicPadEventArgs(relic.CurrentCarrier, relic));
            }
		}

		public void RemoveRelic()
		{
			m_mountedRelic = null;
		}

		public GameRelic MountedRelic
		{
			get { return m_mountedRelic; }
		}

      
        /// <summary>
        /// Area around the pit that checks if a player brings a GameRelic
        /// </summary>
        public class PadArea : Area.Circle
		{
			private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            readonly GameRelicPad m_parent;

			public PadArea(GameRelicPad parentPad)
				: base("", parentPad.X, parentPad.Y, parentPad.Z, PAD_AREA_RADIUS)
			{
				m_parent = parentPad;
			}

			public override void OnPlayerEnter(GamePlayer player)
			{


				GameRelic relicOnPlayer = player.TempProperties.getProperty<object>(GameRelic.PLAYER_CARRY_RELIC_WEAK, null) as GameRelic;

				if (relicOnPlayer == null)
				{
					return;
				}


                if (m_parent != null && m_parent.RelicAllowedToStoreOnPillar(relicOnPlayer) == false)
                {
                    if (relicOnPlayer.CurrentCarrier != null)
                        relicOnPlayer.CurrentCarrier.Out.SendMessage("You can only store an Relic on the right colored realm Pillar! ", DOL.GS.PacketHandler.eChatType.CT_Important, DOL.GS.PacketHandler.eChatLoc.CL_ChatWindow);
                    return;
                }

              

                if (relicOnPlayer.RelicType != m_parent.PadType || m_parent.MountedRelic != null)
                {
                    player.Client.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameRelicPad.OnPlayerEnter.EmptyRelicPad"), relicOnPlayer.RelicType), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    log.DebugFormat("Player {0} needs to find an empty {1} relic pad in order to place {2}.", player.Name, relicOnPlayer.RelicType, relicOnPlayer.Name);
                    return;
                }

                if (player.Realm == m_parent.Realm)
                {
                    log.DebugFormat("Player {0} captured relic {1}.", player.Name, relicOnPlayer.Name);
                    relicOnPlayer.RelicPadTakesOver(m_parent, false);
                }
                else
				{
					log.DebugFormat("Player realm {0} wrong realm on attempt to capture relic {1} of realm {2} on pad of realm {3}.",
					                GlobalConstants.RealmToName(player.Realm),
					                relicOnPlayer.Name,
					                GlobalConstants.RealmToName(relicOnPlayer.Realm),
					                GlobalConstants.RealmToName(m_parent.Realm));
				}
			}
		}



        /*
        /// <summary>
		/// Get the realm of the keep door from keep owner
		/// </summary>
		public override eRealm Realm
        {
            get
            {
                if (this.Component == null || this.Component.Keep == null)
                {
                    return eRealm.None;
                }

                return this.Component.Keep.Realm;
            }
        }
        */
        /// <summary>
		/// door state (open or closed)
		/// </summary>
		protected eDoorState m_state;

        /// <summary>
        /// door state (open or closed)
        /// call the broadcast of state in area
        /// </summary>
        public eDoorState State
        {
            get { return m_state; }
            set
            {
                if (m_state != value)
                {
                    m_state = value;
                    BroadcastDoorStatus();
                }
            }
        }

        /*
        /// <summary>
		/// The level of door is keep level now
		/// </summary>
		public override byte Level
        {
            get
            {
                if (this.Component == null || this.Component.Keep == null)
                {
                    return 0;
                }

                return (byte)this.Component.Keep.Level;
            }
        }
        */
        /*
        public override string Name
        {
            get
            {
                string name = String.Empty;


                {
                    name = "Pillar REALM  " + Realm + " ";
                }

                if (ServerProperties.Properties.ENABLE_DEBUG)
                {
                    name += " ( C:" + ComponentID + " T:" + TemplateID + ")";

                }

                return name;
            }
        }
        */


        

        /// <summary>
		/// This function is called from the ObjectInteractRequestHandler
		/// It teleport player in the keep if player and keep have the same realm
		/// </summary>
		/// <param name="player">GamePlayer that interacts with this object</param>
		/// <returns>false if interaction is prevented</returns>
		public override bool Interact(GamePlayer player)
        {
            ushort soundId = 42;
            if (!base.Interact(player))
                return false;

            /*
            if (player.IsMezzed)
            {
                player.Out.SendMessage("You are mesmerized!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }
            if (player.InCombat && ServerProperties.Properties.CANT_USE_KEEPDOORS_IN_COMBAT)
            {
                player.Out.SendMessage("you are in combat, you can't enter the Door!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (player.IsStunned)
            {
                player.Out.SendMessage("You are stunned!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }
            */
           

            if (player.Client.Account.PrivLevel >= (uint)ePrivLevel.GM)
            { 
                foreach (GamePlayer players in player.GetPlayersInRadius(WorldMgr.REFRESH_DISTANCE))
                {
                    players.Out.SendSoundEffect(soundId, 0, 0, 0, 0, 0);
                }
          
                State = eDoorState.Open;
                BroadcastDoorStatus();
            }
            else if (State == eDoorState.Open)
            {

                player.SendDoorUpdate(this, true);
            }
           

            return base.Interact(player);
        }


        public override IList GetExamineMessages(GamePlayer player)
        {
            /*
			 * You select the Keep Gate. It belongs to your realm.
			 * You target [the Keep Gate]
			 * 
			 * You select the Keep Gate. It belongs to an enemy realm and can be attacked!
			 * You target [the Keep Gate]
			 * 
			 * You select the Postern Door. It belongs to an enemy realm!
			 * You target [the Postern Door]
			 */

            IList list = base.GetExamineMessages(player);
            string text = "You select the " + Name + ".";

            /*
            if (!KeepMgr.IsEnemy(this, player))
            {
                text = text + " It belongs to your realm.";
            }
            else
            {
                if (IsAttackableDoor)
                {
                    text = text + " It belongs to an enemy realm and can be attacked!";
                }
                else
                */
            {
                text = text + " It belongs to an enemy realm!";
            }


            list.Add(text);

            ChatUtil.SendDebugMessage(player, "Health = " + Health);

            /*
            if (IsAttackableDoor)
            {
                // Attempt to fix issue where some players see door as closed when it should be broken open
                // if you target a door it will re-broadcast it's state



                if (Health == 0)// && State != eDoorState.Open)
                {
                    State = eDoorState.Open;
                    BroadcastDoorStatus();
                }
                else if (State == eDoorState.Open)
                {
                    player.SendDoorUpdate(this, true);
                }
            }
            */
            return list;
        }

        public override string GetName(int article, bool firstLetterUppercase)
        {
            return "the " + base.GetName(article, firstLetterUppercase);
        }



        /// <summary>
		/// save the keep door object in DB
		/// </summary>
		public override void SaveIntoDatabase()
        {

        }


        /// <summary>
		/// Loads this door from a door table slot
		/// </summary>
		/// <param name="obj">DBDoor</param>
		public override void LoadFromDatabase(DataObject obj)
        {
            base.LoadFromDatabase(obj);
            DBDoor m_dbdoor = obj as DBDoor;

           
            {
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
        }
        

        public void Open()
        {
            //do nothing because gamekeep must be destroyed to be open
        }

           

        /// <summary>
        /// call when player try to close door
        /// </summary>
        public void Close()
        {
            //do nothing because gamekeep must be destroyed to be open
        }


        /// <summary>
		/// This method is called when door is repair or keep is reset
		/// </summary>
        /// 

          
		public virtual void CloseDoor()
        {
            m_state = eDoorState.Closed;

            BroadcastDoorStatus();
        }
     
        /// <summary>
		/// boradcast the door status to all player near the door
		/// </summary>
		public virtual void BroadcastDoorStatus()
        {
            foreach (GameClient client in WorldMgr.GetClientsOfRegion(CurrentRegionID))
            {
                // client.Player.SendDoorUpdate(this);
                client.Player.SendDoorUpdate(this);
            }
        }

       
        public void NPCManipulateDoorRequest(GameNPC npc, bool open)
        { }


    }
}
