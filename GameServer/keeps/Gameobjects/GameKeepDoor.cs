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
using System;
using System.Collections;


namespace DOL.GS.Keeps
{
    /// <summary>
    /// keep door in world
    /// </summary>
    public class GameKeepDoor : GameLiving, IDoor, IKeepItem
	{
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        #region properties

        protected int m_oldMaxHealth;

		protected byte m_oldHealthPercent;

		protected int m_doorID;

        public bool IsVisibleTo(GamePlayer player)
        {
            // Beispiel-Logik, um die Sichtbarkeit zu bestimmen
            return player != null; // Beispielbedingung
        }
        /// <summary>
        /// The door index which is unique
        /// </summary>
        public int DoorID
		{
			get { return m_doorID; }
			set { m_doorID = value; }
		}

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

		public int TowerNum
		{
			get { return (m_doorID / 10000) % 10; }
		}

		public int KeepID
		{
			get { return OwnerKeepID + TowerNum * 256; }
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
				return 4;
			}
		}

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

		public void UpdateLevel()
		{
			if (MaxHealth != m_oldMaxHealth)
			{
				if (m_oldMaxHealth > 0)
				{
					Health = (int)Math.Ceiling(((double)Health) * ((double)MaxHealth) / ((double)m_oldMaxHealth));
				}
				else
				{
					Health = MaxHealth;
				}

				m_oldMaxHealth = MaxHealth;
			}
		}

		public override bool IsAttackableDoor
		{
			get
			{
				if (this.Component == null || this.Component.Keep == null)
					return false;

				if (this.Component.Keep is GameKeepTower)
				{
					if (this.DoorIndex == 1)
						return true;
				}
				else if (this.Component.Keep is GameKeep)
				{
					if (this.Component.Skin == 10 || this.Component.Skin == 30) //old and new inner keep
					{
						if (this.DoorIndex == 1)
							return true;
					}
					if (this.Component.Skin == 0 || this.Component.Skin == 24)//old and new main gate
					{
						if (this.DoorIndex == 1 ||
							this.DoorIndex == 2)
							return true;
					}
				}
				return false;
			}
		}

		public override int Health
		{
			get
			{
				if (!IsAttackableDoor)
					return 0;
				return base.Health;
			}
			set
			{
				base.Health = value;

				if (HealthPercent > 15 && m_state == eDoorState.Open)
				{
					CloseDoor();
				}
			}
		}

		public override int RealmPointsValue
		{
			get
			{
				return 0;
			}
		}

		public override long ExperienceValue
		{
			get
			{
				return 0;
			}
		}

		public override string Name
		{
			get
			{
				string name = "";

				if (IsAttackableDoor)
				{
					name = "Keep Door";
				}
				else
				{
					name = "Postern Door";
				}

				if (ServerProperties.Properties.ENABLE_DEBUG)
				{
					name += " ( C:" + ComponentID + " T:" + TemplateID + ")";

				}

				return name;
			}
		}

		protected string m_templateID;
		public string TemplateID
		{
			get { return m_templateID; }
		}

		protected GameKeepComponent m_component;
		public GameKeepComponent Component
		{
			get { return m_component; }
			set { m_component = value; }
		}

		protected DBKeepPosition m_dbposition;
		public DBKeepPosition DBPosition
		{
			get { return m_dbposition; }
			set { m_dbposition = value; }
		}

		#endregion

		#region function override

		/// <summary>
		/// Procs don't normally fire on game keep components
		/// </summary>
		/// <param name="ad"></param>
		/// <param name="weapon"></param>
		/// <returns></returns>
		public override bool AllowWeaponMagicalEffect(AttackData ad, InventoryItem weapon, Spell weaponSpell)
        {
            if (weapon.Flags == 10) //Bruiser or any other item needs Itemtemplate "Flags" set to 10 to proc on keep components
                return true;
            else return false; // special code goes here
        }
        
        public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
        {
            if (damageAmount > 0)
            {
                this.Component.Keep.LastAttackedByEnemyTick = this.CurrentRegion.Time;
                base.TakeDamage(source, damageType, damageAmount, criticalAmount);

                //only on hp change
                if (m_oldHealthPercent != HealthPercent)
                {
                    m_oldHealthPercent = HealthPercent;
                    foreach (GameClient client in WorldMgr.GetClientsOfRegion(CurrentRegionID))
                    {
                        client.Out.SendObjectUpdate(this);
                    }
                }
            }
        }

        

        public override void ModifyAttack(AttackData attackData)
		{
			
			int toughness = ServerProperties.Properties.SET_KEEP_DOOR_TOUGHNESS;
			int baseDamage = attackData.Damage;
			int styleDamage = attackData.StyleDamage;
			int criticalDamage = 0;

			GameLiving source = attackData.Attacker;

			if (this.Component.Keep is GameKeepTower)
			{
				toughness = ServerProperties.Properties.SET_TOWER_DOOR_TOUGHNESS;
			}

			if (source is GamePlayer)
			{
				/*
                if (HealthPercent == 100 && source.CurrentRegionID == 163 && (source as GameLiving).AllowedToRaid() == false)
                {
                    baseDamage = 0;
                    styleDamage = 0;
                    criticalDamage = 0;

                    attackData.AttackResult = eAttackResult.NotAllowed_ServerRules;
                    ((GamePlayer)source).Out.SendMessage(LanguageMgr.GetTranslation((source as GamePlayer).Client.Account.Language, "AntiRaidMsg", Properties.RAID_MEMBER_COUNT + 1), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    return;
                }
                else
				*/
				baseDamage = (baseDamage - (baseDamage * 5 * (this.Component.Keep.Level / 100))) * toughness / 100;
				styleDamage = (styleDamage - (styleDamage * 5 * (this.Component.Keep.Level / 100))) * toughness / 100;
			}
			else if (source is GameNPC)
			{
				/*
                if (!ServerProperties.Properties.DOORS_ALLOWPETATTACK || HealthPercent == 100 && source.CurrentRegionID == 163 && (source as GameLiving).AllowedToRaid() == false)
                {
                    baseDamage = 0;
                    styleDamage = 0;
                    criticalDamage = 0;
                    attackData.AttackResult = eAttackResult.NotAllowed_ServerRules;
                    return;
                }
           		else
				*/

				baseDamage = (baseDamage - (baseDamage * 5 * this.Component.Keep.Level / 100)) * toughness / 100;
				styleDamage = (styleDamage - (styleDamage * 5 * this.Component.Keep.Level / 100)) * toughness / 100;

				if (((GameNPC)source).Brain is DOL.AI.Brain.IControlledBrain)
				{
					GamePlayer player = (((DOL.AI.Brain.IControlledBrain)((GameNPC)source).Brain).Owner as GamePlayer);
					if (player != null)
					{
						// special considerations for pet spam classes
						if (player.IsCharcterClass(eCharacterClass.Theurgist) || player.IsCharcterClass(eCharacterClass.Animist))
						{
							baseDamage = (int)(baseDamage + Util.Random(1, 5) * ServerProperties.Properties.PET_SPAM_DAMAGE_MULTIPLIER);
							styleDamage = (int)(styleDamage + Util.Random(1, 5) * ServerProperties.Properties.PET_SPAM_DAMAGE_MULTIPLIER);
						}
						else
						{
							baseDamage = (int)(baseDamage * ServerProperties.Properties.PET_DAMAGE_MULTIPLIER);
							styleDamage = (int)(styleDamage * ServerProperties.Properties.PET_DAMAGE_MULTIPLIER);
						}

					}
				}
			}
            
			attackData.Damage = baseDamage;
			attackData.StyleDamage = styleDamage;
			attackData.CriticalDamage = criticalDamage;
		}


		/// <summary>
		/// This function is called from the ObjectInteractRequestHandler
		/// It teleport player in the keep if player and keep have the same realm
		/// </summary>
		/// <param name="player">GamePlayer that interacts with this object</param>
		/// <returns>false if interaction is prevented</returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

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


			if (!KeepMgr.IsEnemy(this, player) || player.Client.Account.PrivLevel != 1)
			{
				int keepz = Z, distance;

				//calculate distance
				//normal door
				if (DoorIndex == 1)
					distance = 150;
				//side or internal door
				else
					distance = 100;

				//calculate Z
				if (this.Component.Keep is GameKeepTower && !this.Component.Keep.IsPortalKeep)
				{
					//when entering a tower, we need to raise Z
					//portal keeps are considered towers too, so we check component count
					if (IsObjectInFront(player, 180, false) && DoorIndex == 1)
					{
						keepz = Z + 83;
					}
				}
				else
				{
					//when entering a keeps inner door, we need to raise Z
					if (IsObjectInFront(player, 180, false))
					{
						//To find out if a door is the keeps inner door, we compare the distance between
						//the component for the keep and the component for the gate
						int keepdistance = int.MaxValue;
						int gatedistance = int.MaxValue;
						foreach (GameKeepComponent c in this.Component.Keep.KeepComponents)
						{
							if ((GameKeepComponent.eComponentSkin)c.Skin == GameKeepComponent.eComponentSkin.Keep)
							{
								keepdistance = GetDistanceTo(c);
							}
							if ((GameKeepComponent.eComponentSkin)c.Skin == GameKeepComponent.eComponentSkin.Gate)
							{
								gatedistance = GetDistanceTo(c);
							}
							//when these are filled we can stop the search
							if (keepdistance != int.MaxValue && gatedistance != int.MaxValue)
								break;
						}
						if (DoorIndex == 1 && keepdistance < gatedistance)
							keepz = Z + 92;//checked in game with lvl 1 keep
					}
				}

                Point2D keepPoint;
				//calculate x y
				if (IsObjectInFront(player, 180, false))
					keepPoint = GetPointFromHeading( this.Heading, -distance );
				else
					keepPoint = GetPointFromHeading( this.Heading, distance );

				//move player
				player.MoveTo(CurrentRegionID, keepPoint.X, keepPoint.Y, keepz, player.Heading);
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
				{
					text = text + " It belongs to an enemy realm!";
				}
			}

			list.Add(text);

			ChatUtil.SendDebugMessage(player, "Health = " + Health);

			if (IsAttackableDoor)
			{
				// Attempt to fix issue where some players see door as closed when it should be broken open
				// if you target a door it will re-broadcast it's state

				if (Health <= 0 && State != eDoorState.Open)
				{
					State = eDoorState.Open;
					BroadcastDoorStatus();
					//log.Debug("Door update1");
				}
				else if (State == eDoorState.Open && Health > 0)
				{
					BroadcastDoorStatus();
					//log.Debug("Door update2");
				}
			}

			return list;
		}

		public override string GetName(int article, bool firstLetterUppercase)
		{
			return "the " + base.GetName(article, firstLetterUppercase);
		}

		/// <summary>
		/// Starts the power regeneration
		/// </summary>
		public override void StartPowerRegeneration()
		{
			//No regeneration for doors
			return;
		}
		/// <summary>
		/// Starts the endurance regeneration
		/// </summary>
		public override void StartEnduranceRegeneration()
		{
			//No regeneration for doors
			return;
		}

		public override void StartHealthRegeneration()
		{

			if (m_repairTimer != null && m_repairTimer.IsAlive) return;
            m_repairTimer = new RegionTimer(CurrentRegion.TimeManager)
            {
                Callback = new RegionTimerCallback(RepairTimerCallback),
                Interval = repairInterval
            };
            m_repairTimer.Start(repairInterval);
		}

		public void DeleteObject()
		{
			RemoveTimers();

			if (Component != null)
			{
				if (Component.Keep != null)
				{
					Component.Keep.Doors.Remove(ObjectID.ToString());
				}

				Component.Delete();
			}

			Component = null;
			DBPosition = null;
			base.Delete();
			CurrentRegion = null;
		}

		public virtual void RemoveTimers()
		{
			if (m_repairTimer != null)
			{
				m_repairTimer.Stop();
				m_repairTimer = null;
			}

		}
		#endregion

		#region Save/load DB

		/// <summary>
		/// save the keep door object in DB
		/// </summary>
		public override void SaveIntoDatabase()
		{

		}

		/// <summary>
		/// load the keep door object from DB object
		/// </summary>
		/// <param name="obj"></param>
		public override void LoadFromDatabase(DataObject obj)
		{
			DBDoor door = obj as DBDoor;
			if (door == null)
				return;
			base.LoadFromDatabase(obj);

			Zone curZone = WorldMgr.GetZone((ushort)(door.InternalID / 1000000));
			if (curZone == null) return;
			this.CurrentRegion = curZone.ZoneRegion;
			m_name = door.Name;
			m_Heading = (ushort)door.Heading;
			m_x = door.X;
			m_y = door.Y;
			m_z = door.Z;
			m_level = 0;
			m_model = 0xFFFF;
			m_doorID = door.InternalID;
			m_state = eDoorState.Closed;
			this.AddToWorld();


			foreach (AbstractArea area in this.CurrentAreas)
			{
				if (area is GameKeepArea keepArea)
				{
					string sKey = door.InternalID.ToString();
					if (!keepArea.Keep.Doors.ContainsKey(sKey))
					{
						Component = new GameKeepComponent();
						Component.Keep = keepArea.Keep;
						keepArea.Keep.Doors.Add(sKey, this);
					}
					break;
				}
			}
			
			m_health = MaxHealth;
			StartHealthRegeneration();
			DoorMgr.RegisterDoor(this);
		}

		public virtual void LoadFromPosition(DBKeepPosition pos, GameKeepComponent component)
		{
			m_templateID = pos.TemplateID;
			m_component = component;

			PositionMgr.LoadKeepItemPosition(pos, this);
			component.Keep.Doors[m_templateID] = this;

			m_oldMaxHealth = MaxHealth;
			m_health = MaxHealth;
			m_name = "Keep Door";
			m_oldHealthPercent = HealthPercent;
			m_doorID = GenerateDoorID();
			this.m_model = 0xFFFF;
			m_state = eDoorState.Closed;

			if (AddToWorld())
			{
				StartHealthRegeneration();
				DoorMgr.RegisterDoor(this);
			}
			else
			{
				log.Error("Failed to load keep door from keepposition_id =" + pos.ObjectId + ". Component SkinID=" + component.Skin + ". KeepID=" + component.Keep.KeepID);
			}

		}

		public void MoveToPosition(DBKeepPosition position)
		{ }

		public int GenerateDoorID()
		{
			int doortype = 7;
			int ownerKeepID = 0;
			int towerIndex = 0;

			if (m_component.Keep is GameKeepTower)
			{
				GameKeepTower tower = m_component.Keep as GameKeepTower;
				if (tower.Keep != null)
				{
					ownerKeepID = tower.Keep.KeepID;
				}
				towerIndex = tower.KeepID >> 8;
			}
			else
			{
				ownerKeepID = m_component.Keep.KeepID;
			}

			int componentID = m_component.ID;

			//index not sure yet
			int doorIndex = this.DBPosition.TemplateType;
			int id = 0;
			//add door type
			id += doortype * 100000000;
			id += ownerKeepID * 100000;
			id += towerIndex * 10000;
			id += componentID * 100;
			id += doorIndex;
			return id;
		}
		#endregion

		/// <summary>
		/// call when player try to open door
		/// </summary>
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
		/// This function is called when door "die" to open door
		/// </summary>
		public override void Die(GameObject killer)
		{
			base.Die(killer);

            //foreach (GameClient player in WorldMgr.GetClientsOfRegion(CurrentRegionID))
			foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
				player.Out.SendMessage("The Keep Gate is broken!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

			m_state = eDoorState.Open;
			BroadcastDoorStatus();
		}

		/// <summary>
		/// This method is called when door is repair or keep is reset
		/// </summary>
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

                client.Player.SendDoorUpdate(this);
            }
		}

		protected RegionTimer m_repairTimer;
		protected static int repairInterval = 30 * 60 * 1000;

		public int RepairTimerCallback(RegionTimer timer)
		{

           
            if (Component == null || Component.Keep == null)
				return 0;
           
           
			if (HealthPercent == 100 || Component.Keep.InCombat)
				return repairInterval;

			Repair((MaxHealth / 100) * 5);
			return repairInterval;
		}

		/// <summary>
		/// This Function is called when door has been repaired
		/// </summary>
		/// <param name="amount">how many HP is repaired</param>
		public void Repair(int amount)
		{
			Health += amount;
		}
		/// <summary>
		/// This Function is called when keep is taken to repair door
		/// </summary>
		/// <param name="realm">new realm of keep taken</param>
		public void Reset(eRealm realm)
		{
			Realm = realm;
			Health = MaxHealth;
			m_oldHealthPercent = HealthPercent;
			CloseDoor();
		}

		/*
		 * Note that 'enter' and 'exit' commands will also work at these doors.
		 */

		public override bool WhisperReceive(GameLiving source, string str)
		{
			if (!base.WhisperReceive(source, str))
				return false;

			if (source is GamePlayer == false)
				return false;

			str = str.ToLower();

			if (str.Contains("enter") || str.Contains("exit"))
				Interact(source as GamePlayer);
			return true;
		}

		public override bool SayReceive(GameLiving source, string str)
		{
			if (!base.SayReceive(source, str))
				return false;

			if (source is GamePlayer == false)
				return false;

			str = str.ToLower();

			if (str.Contains("enter") || str.Contains("exit"))
				Interact(source as GamePlayer);
			return true;
		}

		public void NPCManipulateDoorRequest(GameNPC npc, bool open)
		{ }
	}
}
