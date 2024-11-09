/*
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
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.Housing;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.PacketHandler.Client.v168;
using DOL.GS.PlayerTitles;
using DOL.GS.PropertyCalc;
using DOL.GS.Quests;
using DOL.GS.RealmAbilities;
using DOL.GS.ServerProperties;
using DOL.GS.SkillHandler;
using DOL.GS.Spells;
using DOL.GS.Styles;
using DOL.GS.Utils;
using DOL.Language;
using log4net;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DOL.GS
{

    /// <summary>
    /// This class represents a player inside the game
    /// </summary>
    public class GamePlayer : GameLiving
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly object m_LockObject = new object();
        private static readonly ReaderWriterLockSlim m_syncLockUpdates = new ReaderWriterLockSlim();
        public const string PlayerRespecMSG = "PLAYERRESPECMSG";
        public const string PetDamageFromAttacker = "PETDAMAGEFROMATTACKER";
        public const string PetCritDamageFromAttacker = "PETCRITDAMAGEFROMATTACKER";
        public const string StealGold = "STEALGOLD";





        #region Client/Character/VariousFlags


        /// <summary>
        /// This is our gameclient!
        /// </summary>
        protected readonly GameClient m_client;
        /// <summary>
        /// This holds the character this player is
        /// based on!
        /// (renamed and private, cause if derive is needed overwrite PlayerCharacter)
        /// </summary>
        protected DOLCharacters m_dbCharacter;
        /// <summary>
        /// The guild id this character belong to
        /// </summary>
        protected string m_guildId;
        /// <summary>
        /// The guild id this character belong to
        /// </summary>
        protected string m_claimedKeep;
        /// <summary>
        /// Char spec points checked on load
        /// </summary>
        protected bool SpecPointsOk = true;
        /// <summary>
        /// Has this player entered the game, will be
        /// true after the first time the char enters
        /// the world
        /// </summary>
        protected bool m_enteredGame;

        /// <summary>
        /// Holds the objects around us
        /// </summary>
        public readonly Dictionary<GameLiving, bool> m_livingInVisibility = new Dictionary<GameLiving, bool>();

        public Dictionary<GameLiving, bool> LivingInVisibility
        {
            get
            {
                return m_livingInVisibility;
            }
        }
        /// <summary>
        /// Holds the objects that need update
		/// </summary>
		//protected readonly BitArray[] m_objectUpdates;
        /// <summary>
        /// Holds the index into the last update array
        /// </summary>
       // protected bool m_lastUpdateArray;
        /// <summary>
        /// Holds the tickcount when the objects around this player
        /// were checked the last time for new npcs. Will be done
        /// every 250ms in WorldMgr.
        /// </summary>
       // protected long m_lastWorldUpdate;

        /// <summary>
        /// Is this player being 'jumped' to a new location?
        /// </summary>
        public bool IsJumping { get; set; }


        /// <summary>
        /// The last time a Passive spell has been cast
        /// </summary>
        private long LastPassiveSpellUse = 0;


        /// <summary>
        /// true if the targetObject is visible
        /// </summary>
        protected bool m_targetInView;


        /// <summary>
        /// Property for the optional away from keyboard message.
        /// </summary>
        public static readonly string AFK_MESSAGE = "afk_message";

        /// <summary>
        /// Property for the optional away from keyboard message.
        /// </summary>
        public static readonly string QUICK_CAST_CHANGE_TICK = "quick_cast_change_tick";

        /// <summary>
        /// Property for the optional away from keyboard message.
        /// </summary>
        public static readonly string Quick_Cast_Remove_TICK = "QUICK_CAST_Remove_TICK";


        public static readonly string ConcentrationTimeValue = "CONCENTATIONSTARTTimeVALUE";

        /// <summary>
        /// Last spell cast from a used item
        /// </summary>
        public static readonly string LAST_USED_ITEM_SPELL = "last_used_item_spell";

        /// <summary>
        /// Array that stores ML step completition
        /// </summary>
        private readonly ArrayList m_mlSteps = new ArrayList();


        /// <summary>
        /// Can this living accept any item regardless of tradable or droppable?
        /// </summary>
        public override bool CanTradeAnyItem { get { return Client.Account.PrivLevel > (int)ePrivLevel.Player; } }


        /// <summary>
        /// Gets or sets the targetObject's visibility
        /// </summary>
        public override bool TargetInView
        {
            get { return m_targetInView; }
            set { m_targetInView = value; }
        }

        /// <summary>
        /// Holds the ground target visibility flag
        /// </summary>
        protected bool m_groundtargetInView;

        /// <summary>
        /// Gets or sets the GroundTargetObject's visibility
        /// </summary>
        public override bool GroundTargetInView
        {
            get { return m_groundtargetInView; }
            set { m_groundtargetInView = value; }
        }



        /// <summary>
        /// Gets the GroundTargetObject set flag
        /// </summary>
        /// <param name="player"></param>
        /// <returns>true/false</returns>
        public virtual bool HasGroundTarget()
        {
            // Überprüfung, ob das aktuelle Objekt nicht null ist 
            // und ob der Zustand aktiv ist
            if (this != null && this.ObjectState == GameObject.eObjectState.Active)
            {
                // Verwendung des Null-Koaleszenz-Operators, um sicherzustellen, dass CurrentRegion nicht null ist
                Region rgn = WorldMgr.GetRegion(this.CurrentRegion?.ID ?? 0);
                // Überprüfen, ob die Zone im aktuellen Region null ist
                if (rgn?.GetZone(this.GroundTarget.X, this.GroundTarget.Y) == null)
                    return false;
                return true;
            }
            return false; // Rückgabe false, wenn das Objekt null ist oder nicht aktiv
        }


        public virtual bool MemberIsInBG(GamePlayer player)
        {
            BattleGroup battleGroup = player.TempProperties.getProperty<BattleGroup>(BattleGroup.BATTLEGROUP_PROPERTY, null);
            if (battleGroup != null)
            {
                foreach (GamePlayer players in battleGroup.Members.Keys)
                {
                    if (players != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Player is in BattleGroup ?
        /// </summary>
        protected bool m_isInBattleGroup;
        public bool IsInBattleGroup
        {
            get { return m_isInBattleGroup; }
            set { m_isInBattleGroup = value; }
        }

        /// <summary>
        /// Current warmap page
        /// </summary>
        private byte m_warmapPage = 1;
        public byte WarMapPage
        {
            get { return m_warmapPage; }
            set { m_warmapPage = value; }
        }


        /// <summary>
        /// Returns the GameClient of this Player
        /// </summary>
        public GameClient Client
        {
            get { return m_client; }
        }

        /// <summary>
        /// Returns the PacketSender for this player
        /// </summary>
        public IPacketLib Out
        {
            get
            {
                return Client.Out;
            }
        }

        /// <summary>
        /// The character the player is based on
        /// </summary>
        public DOLCharacters DBCharacter
        {
            get { return m_dbCharacter; }
        }

        /// <summary>
        /// Has this player entered the game for the first
        /// time after logging on (not Zoning!)
        /// </summary>
        public bool EnteredGame
        {
            get { return m_enteredGame; }
            set { m_enteredGame = value; }
        }

        protected DateTime m_previousLoginDate = DateTime.MinValue;
        /// <summary>
        /// What was the last time this player logged in?
        /// </summary>
        public DateTime PreviousLoginDate
        {
            get { return m_previousLoginDate; }
            set { m_previousLoginDate = value; }
        }

        /// <summary>
        /// Gets or sets the anonymous flag for this player
        /// (delegate to property in PlayerCharacter)
        /// </summary>
        public bool IsAnonymous
        {
            get
            {
                // Überprüfen, ob Client und DBCharacter nicht null sind, bevor darauf zugegriffen wird.
                if (Client?.Account?.PrivLevel == (uint)ePrivLevel.Admin && ServerProperties.Properties.ANON_MODIFIER == -1)
                {
                    return DBCharacter?.IsAnonymous ?? false; // DBCharacter könnte null sein, daher ?? false
                }
                return DBCharacter?.IsAnonymous == true && (ServerProperties.Properties.ANON_MODIFIER != -1); // DBCharacter könnte null sein
            }
            set
            {
                // Setzen wird nur ausgeführt, wenn DBCharacter nicht null ist
                if (DBCharacter != null)
                    DBCharacter.IsAnonymous = value;
            }
        }
        /// <summary>
        /// Whether or not the player can be attacked.
        /// </summary>
        public override bool IsAttackable { get { return (Client.Account.PrivLevel <= (uint)ePrivLevel.Player && base.IsAttackable); } }


        /// <summary>
        /// Is this player PvP enabled
        /// </summary>
        public virtual bool IsPvP { get { return GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvP; } }


        /// <summary>
        /// Can this player use cross realm items
        /// </summary>
        public virtual bool CanUseCrossRealmItems { get { return ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS; } }

        protected bool m_canUseSlashLevel = false;
        public bool CanUseSlashLevel { get { return m_canUseSlashLevel; } }

        /// <summary>
        /// if player uses debug before (to prevent hack client fly mode for players using debug and then turning it off)
        /// </summary>
        protected bool m_canFly;

        /// <summary>
		/// Is this player allowed to fly?
		/// This should only be set in debug command handler.  If player is flying but this flag is false then fly hack is detected
		/// </summary>
		public bool IsAllowedToFly
        {
            get { return m_canFly; }
            set { m_canFly = value; }
        }
        private bool m_statsAnon = false;

        /// <summary>
        /// Gets or sets the stats anon flag for the command /statsanon
        /// (delegate to property in PlayerCharacter)
        /// </summary>
        public bool StatsAnonFlag
        {
            get { return m_statsAnon; }
            set { m_statsAnon = value; }
        }
        #endregion


        public virtual GamePlayer GetBattleGroupLeader(GamePlayer player)
        {


            BattleGroup battleGroup = player.TempProperties.getProperty<BattleGroup>(BattleGroup.BATTLEGROUP_PROPERTY, null);

            if (battleGroup != null)
            {
                foreach (GamePlayer BGplayer in battleGroup.Members.Keys)
                {


                    if ((bool)battleGroup.Members[BGplayer] == true)
                    {
                        if (BGplayer == player)
                        {
                            //log.ErrorFormat("BGleader: = {0}", BGplayer.Name); //der mit der propertie ist der leader!!
                            return player;
                        }
                    }

                }
            }
            return null;
        }

        #region DoorCache
        protected Dictionary<int, eDoorState> m_doorUpdateList = null;

        protected ushort m_doorUpdateRegionID;

        /// <summary>
        /// Send a door state to this client
        /// </summary>
        /// <param name="door">the door</param>
        /// <param name="forceUpdate">force a send of the door state regardless of status</param>
        public void SendDoorUpdate(IDoor door, bool forceUpdate = false)
        {
            Out.SendObjectCreate(door as GameObject);

            if (m_doorUpdateList == null || m_doorUpdateRegionID != CurrentRegionID)
            {
                m_doorUpdateList = new Dictionary<int, eDoorState>();
                m_doorUpdateRegionID = CurrentRegionID;
                m_doorUpdateList.Add(door.ObjectID, door.State);
                Out.SendDoorState(CurrentRegion, door);
            }
            else if (forceUpdate || m_doorUpdateList.ContainsKey(door.ObjectID) == false || m_doorUpdateList[door.ObjectID] != door.State)
            {
                Out.SendDoorState(CurrentRegion, door);

                try
                {
                    m_doorUpdateList[door.ObjectID] = door.State;
                }
                catch {
                    //log.Error("Door State nicht gefunden!");
                }
            }

            Out.SendObjectUpdate(door as GameObject);
        }

        #endregion

        #region Database Accessor

        /// <summary>
        /// Gets or sets the Database ObjectId for this player
        /// (delegate to property in DBCharacter)
        /// </summary>
        public string ObjectId
        {
            //get { return DBCharacter != null ? DBCharacter.ObjectId : InternalID; }
            get { return DBCharacter?.ObjectId ?? InternalID; }
            set { if (DBCharacter != null) DBCharacter.ObjectId = value; }
        }

        /// <summary>
        /// Gets or sets the no help flag for this player
        /// </summary>
        public bool NoHelp
        {
            get { return DBCharacter != null ? DBCharacter.NoHelp : false; }
            set { if (DBCharacter != null) DBCharacter.NoHelp = value; }
        }

        /// <summary>
		/// Gets or sets the no help flag for this player
		/// </summary>
		public bool HasFreeStartArmor
        {
            get { return DBCharacter != null ? DBCharacter.HasFreeStartArmor : false; }
            set { if (DBCharacter != null) DBCharacter.HasFreeStartArmor = value; }
        }
        /// <summary>
		/// Gets or sets the no help flag for this player
		/// </summary>
		public bool HasFreeStartArmor40
        {
            get { return DBCharacter != null ? DBCharacter.HasFreeStartArmor40 : false; }
            set { if (DBCharacter != null) DBCharacter.HasFreeStartArmor40 = value; }
        }
        /// <summary>
        /// Gets or sets the no help flag for this player
        /// </summary>
        public bool HasFreeStartWeapons
        {
            get { return DBCharacter != null ? DBCharacter.HasFreeStartWeapons : false; }
            set { if (DBCharacter != null) DBCharacter.HasFreeStartWeapons = value; }
        }

        /// <summary>
        /// Gets or sets the no help flag for this player
        /// </summary>
        public bool HasFreeStartWeapons40
        {
            get { return DBCharacter != null ? DBCharacter.HasFreeStartWeapons40 : false; }
            set { if (DBCharacter != null) DBCharacter.HasFreeStartWeapons40 = value; }
        }

        /// <summary>
        /// Gets or sets the no help flag for this player
        /// </summary>
        public bool HasStartWeapon30
        {
            get { return DBCharacter != null ? DBCharacter.HasStartWeapon30 : false; }
            set { if (DBCharacter != null) DBCharacter.HasStartWeapon30 = value; }
        }

        /// <summary>
        /// Gets or sets the no help flag for this player
        /// </summary>
        public bool HasStartWeapon40
        {
            get { return DBCharacter != null ? DBCharacter.HasStartWeapon40 : false; }
            set { if (DBCharacter != null) DBCharacter.HasStartWeapon40 = value; }
        }


        /// <summary>
        /// Gets or sets the no help flag for this player
        /// </summary>
        public bool HasStartWeapon45
        {
            get { return DBCharacter != null ? DBCharacter.HasStartWeapon45 : false; }
            set { if (DBCharacter != null) DBCharacter.HasStartWeapon45 = value; }
        }



        /// <summary>
        /// Gets or sets the no help flag for this player
        /// </summary>
        public bool HasML10
        {
            get { return DBCharacter != null ? DBCharacter.HasML10 : false; }
            set { if (DBCharacter != null) DBCharacter.HasML10 = value; }
        }

        /// <summary>
        /// Gets or sets the no help flag for this player
        /// </summary>
        public bool HasAllOberlisks
        {
            get { return DBCharacter != null ? DBCharacter.HasAllOberlisks : false; }
            set
            {
                if (DBCharacter != null) DBCharacter.HasAllOberlisks = value;
                Notify(GamePlayerEvent.AllOberlisksCaptured, this);
            }
        }

        /// <summary>
        /// Gets or sets the no help flag for this player
        /// </summary>
        public bool HasChampionWeapons
        {
            get { return DBCharacter != null ? DBCharacter.HasChampionWeapons : false; }
            set { if (DBCharacter != null) DBCharacter.HasChampionWeapons = value; }
        }
        /// <summary>
        /// Gets or sets the show guild logins flag for this player
        /// </summary>
        public bool ShowGuildLogins
        {
            get { return DBCharacter != null ? DBCharacter.ShowGuildLogins : false; }
            set { if (DBCharacter != null) DBCharacter.ShowGuildLogins = value; }
        }

        /// <summary>
        /// Gets or sets the gain XP flag for this player
        /// (delegate to property in PlayerCharacter)
        /// </summary>
        
        public bool GainXP
        {
            // Gibt true zurück, wenn DBCharacter null ist, andernfalls gibt es den Wert von GainXP zurück
            get { return DBCharacter != null ? DBCharacter.GainXP : true; }
            // Setzen wird nur ausgeführt, wenn DBCharacter nicht null ist
            set { if (DBCharacter != null) DBCharacter.GainXP = value; }
        }

        /// <summary>
        /// Gets or sets the gain RP flag for this player
        /// (delegate to property in PlayerCharacter)
        /// </summary>
        public bool GainRP
        {
            get { return (DBCharacter != null ? DBCharacter.GainRP : true); }
            set { if (DBCharacter != null) DBCharacter.GainRP = value; }
        }

        /// <summary>
        /// Gets or sets the roleplay flag for this player
        /// (delegate to property in PlayerCharacter)
        /// </summary>
        public bool RPFlag
        {
            get { return (DBCharacter != null ? DBCharacter.RPFlag : true); }
            set { if (DBCharacter != null) DBCharacter.RPFlag = value; }
        }

        /// <summary>
        /// gets or sets the guildnote for this player
        /// (delegate to property in PlayerCharacter)
        /// </summary>

        public string GuildNote
        {
            get
            {
                // Rückgabe eines leeren Strings, wenn DBCharacter null ist, um Nullreferenzen zu vermeiden
                return DBCharacter?.GuildNote ?? String.Empty;
            }
            set
            {
                // Setzen wird nur ausgeführt, wenn DBCharacter nicht null ist
                if (DBCharacter != null)
                    DBCharacter.GuildNote = value;
            }
        }

        /// <summary>
        /// Gets or sets the DailyReward flag for this player
        /// (delegate to property in PlayerCharacter)
        /// </summary>
        public bool DailyReward
        {
            get { return DBCharacter != null ? DBCharacter.DailyReward : true; }
            set { if (DBCharacter != null) DBCharacter.DailyReward = value; }
        }

        /// <summary>
        /// Gets or sets the DailyReward flag for this player
        /// (delegate to property in PlayerCharacter)
        /// </summary>
        public int XPStoneCount
        {
            get { return DBCharacter != null ? DBCharacter.XPStoneCount : 0; }
            set { if (DBCharacter != null) DBCharacter.XPStoneCount = value; }
        }

        /// <summary>
        /// Gets or sets the PowerElixirCount for a player
        /// (delegate to property in PlayerCharacter)
        /// </summary>
        public int BarrelForPowerElixirCount
        {
            get { return DBCharacter != null ? DBCharacter.BarrelForPowerElixirCount : 0; }
            set { if (DBCharacter != null) DBCharacter.BarrelForPowerElixirCount = value; }
        }

        /// <summary>
        /// Gets or sets the HealingElixirCount for a player
        /// (delegate to property in PlayerCharacter)
        /// </summary>
        public int BarrelForHealingElixirCount
        {
            get { return DBCharacter != null ? DBCharacter.BarrelForHealingElixirCount : 0; }
            set { if (DBCharacter != null) DBCharacter.BarrelForHealingElixirCount = value; }
        }


        /// <summary>
        /// Gets or sets the autoloot flag for this player
        /// (delegate to property in PlayerCharacter)
        /// </summary>
        public bool Autoloot
        {
            get { return DBCharacter != null ? DBCharacter.Autoloot : true; }
            set { if (DBCharacter != null) DBCharacter.Autoloot = value; }
        }

        /// <summary>
        /// Gets or sets the advisor flag for this player
        /// (delegate to property in PlayerCharacter)
        /// </summary>
        public bool Advisor
        {
            get { return DBCharacter != null ? DBCharacter.Advisor : false; }
            set { if (DBCharacter != null) DBCharacter.Advisor = value; }
        }


        /// <summary>
        /// Gets or sets the BindHouseRegion for this player
        /// (delegate to property in DBCharacter)
        /// </summary>
        public int BindHouseRegion
        {
            get { return DBCharacter != null ? DBCharacter.BindHouseRegion : 0; }
            set { if (DBCharacter != null) DBCharacter.BindHouseRegion = value; }
        }

        /// <summary>
        /// Gets or sets the BindHouseXpos for this player
        /// (delegate to property in DBCharacter)
        /// </summary>
        public int BindHouseXpos
        {
            get { return DBCharacter?.BindHouseXpos ?? 0; } // Verwendung des Null-Koaleszenz-Operators
            set { if (DBCharacter != null) DBCharacter.BindHouseXpos = value; }
        }

        /// <summary>
        /// Gets or sets the BindHouseYpos for this player
        /// (delegate to property in DBCharacter)
        /// </summary>
        public int BindHouseYpos
        {
            get { return DBCharacter != null ? DBCharacter.BindHouseYpos : 0; }
            set { if (DBCharacter != null) DBCharacter.BindHouseYpos = value; }
        }

        /// <summary>
        /// Gets or sets BindHouseZpos for this player
        /// (delegate to property in DBCharacter)
        /// </summary>
        public int BindHouseZpos
        {
            get { return DBCharacter != null ? DBCharacter.BindHouseZpos : 0; }
            set { if (DBCharacter != null) DBCharacter.BindHouseZpos = value; }
        }

        /// <summary>
        /// Gets or sets the BindHouseHeading for this player
        /// (delegate to property in DBCharacter)
        /// </summary>
        public int BindHouseHeading
        {
            get { return DBCharacter != null ? DBCharacter.BindHouseHeading : 0; }
            set { if (DBCharacter != null) DBCharacter.BindHouseHeading = value; }
        }

        /// <summary>
        /// Gets or sets the BindHouseHeading for this player
        /// (delegate to property in DBCharacter)
        /// </summary>
        public int BindHouseNumber
        {
            get { return DBCharacter != null ? DBCharacter.BindHouseNumber : 0; }
            set { if (DBCharacter != null) DBCharacter.BindHouseNumber = value; }
        }

        /// <summary>
		/// Gets or sets the BindRegion for this player
		/// (delegate to property in DBCharacter)
		/// </summary>
		public int BindRegion
        {
            get { return DBCharacter != null ? DBCharacter.BindRegion : 0; }
            set { if (DBCharacter != null) DBCharacter.BindRegion = value; }
        }

        /// <summary>
        /// Gets or sets the BindXpos for this player
        /// (delegate to property in DBCharacter)
        /// </summary>
        public int BindXpos
        {
            get { return DBCharacter != null ? DBCharacter.BindXpos : 0; }
            set { if (DBCharacter != null) DBCharacter.BindXpos = value; }
        }

        /// <summary>
        /// Gets or sets the BindYpos for this player
        /// (delegate to property in DBCharacter)
        /// </summary>
        public int BindYpos
        {
            get { return DBCharacter != null ? DBCharacter.BindYpos : 0; }
            set { if (DBCharacter != null) DBCharacter.BindYpos = value; }
        }

        /// <summary>
        /// Gets or sets the BindZpos for this player
        /// (delegate to property in DBCharacter)
        /// </summary>
        public int BindZpos
        {
            get { return DBCharacter != null ? DBCharacter.BindZpos : 0; }
            set { if (DBCharacter != null) DBCharacter.BindZpos = value; }
        }

        /// <summary>
        /// Gets or sets the BindHeading for this player
        /// (delegate to property in DBCharacter)
        /// </summary>
        public int BindHeading
        {
            get { return DBCharacter != null ? DBCharacter.BindHeading : 0; }
            set { if (DBCharacter != null) DBCharacter.BindHeading = value; }
        }

        #endregion

        #region Player Quitting

        /// <summary>
        /// quit timer
        /// </summary>
        protected RegionTimer m_quitTimer;

        /// <summary>
        /// Timer callback for quit
        /// </summary>
        /// <param name="callingTimer">the calling timer</param>
        /// <returns>the new intervall</returns>
        protected virtual int QuitTimerCallback(RegionTimer callingTimer)
        {
            if (!IsAlive || ObjectState != eObjectState.Active)
            {
                m_quitTimer = null;
                return 0;
            }

            //Gms can quit instantly
            if (Client.Account.PrivLevel == 1)
            {
                if (CraftTimer != null && CraftTimer.IsAlive)
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Quit.CantQuitCrafting"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    m_quitTimer = null;
                    return 0;
                }

                long lastCombatAction = LastAttackedByEnemyTick;
                if (lastCombatAction < LastAttackTick)
                {
                    lastCombatAction = LastAttackTick;
                }
                long secondsleft = 60 - (CurrentRegion.Time - lastCombatAction + 500) / 1000; // 500 is for rounding
                if (secondsleft > 0)
                {
                    if (secondsleft == 15 || secondsleft == 10 || secondsleft == 5)
                    {
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Quit.YouWillQuit1", secondsleft.ToString()), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    }
                    return 1000;
                }
            }

            Out.SendPlayerQuit(false);
            Quit(true);
            SaveIntoDatabase();
            m_quitTimer = null;
            return 0;
        }

        /// <summary>
        /// Gets the amount of time the player must wait before quit, in seconds
        /// </summary>
        public virtual int QuitTime
        {
            get
            {
                if (m_quitTimer == null)
                {
                    // dirty trick ;-) (20sec min quit time)
                    if (CurrentRegion.Time - LastAttackTickPvP > 40000)
                        LastAttackTickPvP = CurrentRegion.Time - 40000;
                    if (CurrentRegion.Time - LastAttackTickPvE > 40000)
                        LastAttackTickPvE = CurrentRegion.Time - 40000;
                }
                long lastCombatAction = LastAttackTick;
                if (lastCombatAction < LastAttackedByEnemyTick)
                {
                    lastCombatAction = LastAttackedByEnemyTick;
                }
                return (int)(60 - (CurrentRegion.Time - lastCombatAction + 500) / 1000); // 500 is for rounding
            }
            set
            { }
        }
        #endregion

        #region Player Linking Dead
        /// <summary>
		/// Callback method, called when the player went linkdead and now he is
		/// allowed to be disconnected
		/// </summary>
		/// <param name="callingTimer">the timer</param>
		/// <returns>0</returns>
		protected int LinkdeathTimerCallback(RegionTimer callingTimer)
        {
            //If we died during our callback time we release
            try
            {
                if (!IsAlive)
                {
                    Release(m_releaseType, true);
                    if (log.IsInfoEnabled)
                        log.Info("Linkdead player " + Name + "(" + Client.Account.Name + ") was auto-released from death!");
                }

                SaveIntoDatabase();
            }
            finally
            {
                Client.Quit();
            }

            return 0;
        }



        public void OnLinkdeath()
        {
            if (log.IsInfoEnabled)
                log.InfoFormat("Player {0}({1}) went linkdead!", Name, Client.Account.Name);

            // LD Necros need to be "Unshaded"
            if (Client.Player.CharacterClass.Player.IsShade)
            {
                Client.Player.CharacterClass.Player.Shade(false);
            }

            // Dead link-dead players release on live servers
            if (!IsAlive)
            {
                Release(m_releaseType, true);
                if (log.IsInfoEnabled)
                    log.InfoFormat("Linkdead player {0}({1}) was auto-released from death!", Name, Client.Account.Name);
                SaveIntoDatabase();
                Client.Quit();
                return;
            }

            //Stop player if he's running....
            CurrentSpeed = 0;
            foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                if (player.ObjectState != eObjectState.Active || player == null || player == this)
                    continue;

                //Maybe there is a better solution?
                player.Out.SendObjectRemove(this);
                player.Out.SendPlayerCreate(this);
            }


            UpdatePlayerEquipment();
            //LeaveHouse();


            if (m_quitTimer != null)
            {
                m_quitTimer.Stop();
                m_quitTimer = null;
            }

            int secondsToQuit = QuitTime;
            if (log.IsInfoEnabled)
            {
                log.InfoFormat("Linkdead player {0}({1}) will quit in {2}", Name, Client.Account.Name, secondsToQuit.ToString());

                if (Client != null)
                {
                    SaveIntoDatabase();
                    Client.Quit();
                }
            }

            RegionTimer timer = new RegionTimer(this);// make sure it is not stopped!
            if (timer.IsAlive)
            {
                timer.Callback = new RegionTimerCallback(LinkdeathTimerCallback);
                timer.Start(1 + secondsToQuit * 1000);
            }


            TradeWindow?.CloseTrade();

            //Notify players in close proximity!
            foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
            {
                if (player == null) continue;
                if (GameServer.ServerRules.IsAllowedToUnderstand(this, player))
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.OnLinkdeath.Linkdead", Name), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
            }

            //Notify other group members of this linkdead
            if (Group != null)
                Group.UpdateMember(this, false, false);

            CheckIfNearEnemyKeepAndAddToRvRLinkDeathListIfNecessary();

            //Notify our event handlers (if any)
            Notify(GamePlayerEvent.Linkdeath, this);
        }

        private void CheckIfNearEnemyKeepAndAddToRvRLinkDeathListIfNecessary()
        {
            AbstractGameKeep keep = KeepMgr.GetKeepCloseToSpot(this.CurrentRegionID, this, WorldMgr.VISIBILITY_DISTANCE);
            if (keep != null && this.Client.Account.PrivLevel == 1 && KeepMgr.IsEnemy(keep, this))
            {
                if (WorldMgr.RvRLinkDeadPlayers.ContainsKey(this.m_InternalID))
                {
                    WorldMgr.RvRLinkDeadPlayers.Remove(this.m_InternalID);
                }
                WorldMgr.RvRLinkDeadPlayers.Add(this.m_InternalID, DateTime.Now);
            }
        }

        /// <summary>
        /// Stop all timers, events and remove player from everywhere (group/guild/chat)
        /// </summary>
        protected virtual void CleanupOnDisconnect()
        {
            StopAttack();
            // remove all stealth handlers
            Stealth(false);
            if (IsOnHorse)
                IsOnHorse = false;

            if (m_inventory != null)
            {
                GameEventMgr.RemoveAllHandlersForObject(m_inventory);
            }


            if (CraftTimer != null)
            {
                CraftTimer.Stop();
                CraftTimer = null;
            }

            if (QuestActionTimer != null)
            {
                QuestActionTimer.Stop();
                QuestActionTimer = null;
            }

            if (Group != null)
                Group.RemoveMember(this);

            BattleGroup mybattlegroup = (BattleGroup)this.TempProperties.getProperty<object>(BattleGroup.BATTLEGROUP_PROPERTY, null);
            if (mybattlegroup != null)
                mybattlegroup.RemoveBattlePlayer(this);

            if (TradeWindow != null)
                TradeWindow.CloseTrade();

            if (m_guild != null)
                m_guild.RemoveOnlineMember(this);

            // RR4: expire personal mission, if any
            if (Mission != null)
                Mission.ExpireMission();

            ChatGroup mychatgroup = (ChatGroup)TempProperties.getProperty<object>(ChatGroup.CHATGROUP_PROPERTY, null);
            if (mychatgroup != null)
                mychatgroup.RemovePlayer(this);

            if (this.ControlledBrain != null)
                CommandNpcRelease();

            if (SiegeWeapon != null)
                SiegeWeapon.ReleaseControl();

            //if (InHouse)
                //LeaveHouse();

            // Dinberg: this will eventually need to be changed so that it moves them to the location they TP'ed in.
            // DamienOphyr: Overwrite current position with Bind position in database, MoveTo() is inoperant
            if (CurrentRegion.IsInstance)
            {
                DBCharacter.Region = DBCharacter.BindRegion;
                DBCharacter.Xpos = DBCharacter.BindXpos;
                DBCharacter.Ypos = DBCharacter.BindYpos;
                DBCharacter.Zpos = DBCharacter.BindZpos;
                DBCharacter.Direction = DBCharacter.BindHeading;
            }

            //check for battleground caps
            Battleground bg = KeepMgr.GetBattleground(CurrentRegionID);
            if (bg != null)
            {
                if (Level > bg.MaxLevel || RealmLevel >= bg.MaxRealmLevel)
                {
                    // Only kick players out
                    if (Client.Account.PrivLevel == (int)ePrivLevel.Player)
                    {
                        KeepMgr.ExitBattleground(this);
                    }
                }
            }

            // Remove champion dedicated spell line
            SkillBase.UnRegisterSpellLine(ChampionSpellLineName);

            // cancel all effects until saving of running effects is done
            try
            {
                EffectList.SaveAllEffects();
                CancelAllConcentrationEffects();
                EffectList.CancelAll();

            }
            catch (Exception e)
            {
                log.ErrorFormat("Cannot cancel all effects on player: {0}, Ex: {1} ", Name, e.ToString());
            }
        }

        /// <summary>
        /// This function saves the character and sends a message to all others
        /// that the player has quit the game!
        /// </summary>
        /// <param name="forced">true if Quit can not be prevented!</param>
        public Task<bool> Quit(bool forced)
        {
           return PlayerQuit(forced);
        }
        
        public async Task<bool> PlayerQuit(bool forced)
        {

                await Task.Run(() =>
                {
                    if (!forced)
                    {
                        if (!IsAlive)
                        {
                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Quit.CantQuitDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return false;
                        }
                        if (Steed != null || IsOnHorse)
                        {
                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Quit.CantQuitMount"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return false;
                        }
                        if (IsMoving)
                        {
                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Quit.CantQuitStanding"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return false;
                        }
                        if (CraftTimer != null && CraftTimer.IsAlive)
                        {
                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Quit.CantQuitCrafting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return false;
                        }

                        if (CurrentRegion.IsInstance)
                        {
                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Quit.CantQuitInInstance"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return false;
                        }

                        if (Statistics != null)
                        {
                            string stats = Statistics.GetStatisticsMessage();
                            if (String.IsNullOrEmpty(stats) == false)
                            {
                                Out.SendMessage(stats, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                        }
                        if (!IsSitting)
                        {
                            Sit(true);
                        }
                        int secondsleft = QuitTime;

                        if (m_quitTimer == null)
                        {
                            m_quitTimer = new RegionTimer(this)
                            {
                                Callback = new RegionTimerCallback(QuitTimerCallback)
                            };
                            m_quitTimer.Start(1);
                        }

                        if (secondsleft > 20)
                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Quit.RecentlyInCombat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Quit.YouWillQuit2", secondsleft.ToString()), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                    else
                    {
                        //Notify our event handlers (if any)
                        Notify(GamePlayerEvent.Quit, this);

                        // log quit
                        AuditMgr.AddAuditEntry(Client, AuditType.Character, AuditSubtype.CharacterLogout, "", Name);

                        //Cleanup stuff
                       
                        Delete();
                    }
                    return true;
                });

            return true;
        }


    #endregion

        #region Combat timer
    /// <summary>
    /// gets the DamageRvR Memory of this player
    /// </summary>
    public override long DamageRvRMemory
        {
            get { return m_damageRvRMemory; }
            set { m_damageRvRMemory = value; }
        }

        /// <summary>
        /// Override For Combat Timer Update
        /// </summary>
        public override long LastAttackedByEnemyTickPvE
        {
            set
            {
                bool wasInCombat = InCombat;
                base.LastAttackedByEnemyTickPvE = value;
                if (!wasInCombat && InCombat)
                    Out.SendUpdateMaxSpeed();

                ResetInCombatTimer();
            }
        }

        /// <summary>
        /// Override For Combat Timer Update
        /// </summary>
        public override long LastAttackTickPvE
        {
            set
            {
                bool wasInCombat = InCombat;
                base.LastAttackTickPvE = value;
                if (!wasInCombat && InCombat)
                    Out.SendUpdateMaxSpeed();

                ResetInCombatTimer();
            }
        }

        /// <summary>
        /// Expire Combat Timer Interval
        /// </summary>
        protected virtual int CombatTimerInterval { get { return 11000; } }

        /// <summary>
        /// Combat Timer Lock
        /// </summary>
        private readonly object m_CombatTimerLock = new object();

        /// <summary>
        /// Combat Timer
        /// </summary>
        private RegionTimerAction<GamePlayer> m_CombatTimer = null;

        /// <summary>
        /// Reset and Restart Combat Timer
        /// </summary>
        protected void ResetInCombatTimer()
        {
            lock (m_CombatTimerLock)
            {
                if (m_CombatTimer == null)
                {
                    m_CombatTimer = new RegionTimerAction<GamePlayer>(this, p => p.Out.SendUpdateMaxSpeed());
                }
                m_CombatTimer.Stop();
                m_CombatTimer.Start(CombatTimerInterval);
            }
        }

        /*
        /// <summary>
        /// The last time we did update the world around us
        /// </summary>
        public long LastWorldUpdate
        {
            get { return m_lastWorldUpdate; }
            set { m_lastWorldUpdate = value; }
        }
        */

        #endregion

        #region release/bind/pray
        /// <summary>
        /// Property that holds tick when the player bind last time
        /// </summary>
        public const string LAST_BIND_TICK = "LastBindTick";
        public const int RVRIllnes = 2435;

        /// <summary>
		/// Min Allowed Interval Between Player Bind
		/// </summary>
		public virtual int BindAllowInterval { get { return 60000; } }

        /// <summary>
        /// Binds this player to the current location
        /// </summary>
        /// <param name="forced">if true, can bind anywhere</param>
        public virtual void Bind(bool forced)
        {
            // Sicherstellen, dass CurrentRegion nicht null ist
            if (CurrentRegion == null ||Client == null)
            {
                return;
            }

            if (CurrentRegion.IsInstance)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Bind.CantHere"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            // Nur fortfahren, wenn der Spieler am Leben ist und Client nicht null ist
            if (Client == null)
            {
               return;
            }



            if (forced)
            {
                BindRegion = CurrentRegionID;
                BindHeading = Heading;
                BindXpos = X;
                BindYpos = Y;
                BindZpos = Z;
                if (DBCharacter != null)
                    GameServer.Database.SaveObject(DBCharacter);
                return;
            }
            // Nur fortfahren, wenn der Spieler am Leben ist und Client nicht null ist
            if (!IsAlive)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Bind.CantBindDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            //60 second rebind timer

            //long lastBindTick = TempProperties != null ? TempProperties.getProperty<long>(LAST_BIND_TICK, 0) : 0;
            long lastBindTick = TempProperties?.getProperty<long>(LAST_BIND_TICK, 0) ?? 0;
            long changeTime = CurrentRegion.Time - lastBindTick;
            if (Client.Account.PrivLevel <= (uint)ePrivLevel.Player && changeTime < BindAllowInterval)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Bind.MustWait", (1 + (BindAllowInterval - changeTime) / 1000).ToString()), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            string description = String.Empty;

            Region region = null;
            if (WorldMgr.GetRegion((ushort)BindRegion) != null)
            {
                region = WorldMgr.GetRegion((ushort)BindRegion);
            }

            Zone zon = null;
            if (region != null && region.GetZone(BindXpos, BindYpos) != null)
            {
                zon = region.GetZone(BindXpos, BindYpos);
            }
            if (zon != null)
            {
                IList areas = zon.GetAreasOfSpot(BindXpos, BindYpos, BindZpos);

                foreach (AbstractArea area in areas)
                {
                    if (!area.DisplayMessage) continue;
                    description = area.Description;
                    break;
                }
            }
            if (description == String.Empty)
            {
                if (zon != null)
                    description = zon.Description;
            }
            else
            {
                description += " in " + zon.Description;
            }


            if (!IsAlive)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Bind.CantBindDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }


            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Bind.LastBindPoint", description), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            //string description = string.Format("in {0}", this.GetBindSpotDescription());
            // Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Bind.LastBindPoint", description), eChatType.CT_System, eChatLoc.CL_SystemWindow);

            bool bound = false;

            //var bindarea = CurrentAreas.OfType<Area.BindArea>().FirstOrDefault(ar => GameServer.ServerRules.IsAllowedToBind(this, ar.BindPoint));

            var bindarea = CurrentAreas
            .OfType<Area.BindArea>()
            .FirstOrDefault(ar => GameServer.ServerRules.IsAllowedToBind(this, ar.BindPoint));

            if (bindarea != null)
            {
                bound = true;
                BindRegion = CurrentRegionID;
                BindHeading = Heading;
                BindXpos = X;
                BindYpos = Y;
                BindZpos = Z;
                if (DBCharacter != null)
                    GameServer.Database.SaveObject(DBCharacter);
            }

            //if we are not bound yet lets check if we are in a house where we can bind
            if (!bound && InHouse && CurrentHouse != null) // lets do a double check, more safe
            {
                House house = CurrentHouse;
                //var obj = GameServer.Database.SelectObjects<DBHouseHookpointItem>("HouseNumber = '" + house.HouseNumber.ToString() + "'");
                var obj = GameServer.Database.SelectObjects<DBHouseHookpointItem>("`HouseNumber` = @HouseNumber", new QueryParameter("@HouseNumber", house.HouseNumber.ToString()));

                bool canbindhere = false;
                foreach (DBHouseHookpointItem hpi in obj)
                {
                    if (hpi.ItemTemplateID.Contains("_bindstone"))
                    {
                        canbindhere = true;
                        break;
                    }
                }

                if (canbindhere)
                {
                    // make sure we can actually use the bindstone
                    if (!house.CanBindInHouse(this))
                    {
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Bind.CantHere"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    bound = true;
                    double angle = house.Heading * ((Math.PI * 2) / 360); // angle*2pi/360;
                    int outsideX = (int)(house.X + (0 * Math.Cos(angle) + 500 * Math.Sin(angle)));
                    int outsideY = (int)(house.Y - (500 * Math.Cos(angle) - 0 * Math.Sin(angle)));
                    ushort outsideHeading = (ushort)((house.Heading < 180 ? house.Heading + 180 : house.Heading - 180) / 0.08789);
                    BindHouseRegion = CurrentRegionID;
                    BindHouseHeading = outsideHeading;
                    BindHouseXpos = outsideX;
                    BindHouseYpos = outsideY;
                    BindHouseZpos = house.Z;
                    BindHouseNumber = house.HouseNumber;
                    if (DBCharacter != null)
                        GameServer.Database.SaveObject(DBCharacter);
                }
            }
            if (bound)
            {
                if (!IsMoving)
                {
                    eEmote bindEmote = eEmote.Bind;
                    switch (this.Realm)
                    {
                        case eRealm.Albion: bindEmote = eEmote.BindAlb; break;
                        case eRealm.Midgard: bindEmote = eEmote.BindMid; break;
                        case eRealm.Hibernia: bindEmote = eEmote.BindHib; break;
                    }
                    foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    {
                        if (player == null)
                            return;

                        if ((int)player.Client.Version < (int)GameClient.eClientVersion.Version187)
                            player.Out.SendEmoteAnimation(this, eEmote.Bind);
                        else
                            player.Out.SendEmoteAnimation(this, bindEmote);
                    }
                }
                TempProperties.setProperty(LAST_BIND_TICK, CurrentRegion.Time);
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Bind.Bound"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
            else
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Bind.CantHere"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
        }
        #endregion


        #region Releasing
        /// <summary>
        /// tick when player is died
        /// </summary>
        protected long m_deathTick;

        /// <summary>
        /// choosed the player to release as soon as possible?
        /// </summary>
        protected bool m_automaticRelease = false;

        /// <summary>
        /// The release timer for this player
        /// </summary>
        protected RegionTimer m_releaseTimer;

        /// <summary>
        /// Stops release timer and closes timer window
        /// </summary>
        public void StopReleaseTimer()
        {
            Out.SendCloseTimerWindow();
            if (m_releaseTimer != null)
            {
                m_releaseTimer.Stop();
                m_releaseTimer = null;
            }
        }

        /// <summary>
        /// minimum time to wait before release is possible in seconds
        /// </summary>
        protected const int RELEASE_MINIMUM_WAIT = 10;

        /// <summary>
        /// max time before auto release in seconds
        /// </summary>
        protected const int RELEASE_TIME = 900;

        /// <summary>
        /// The property name that is set when releasing to another region
        /// </summary>
        public const string RELEASING_PROPERTY = "releasing";

        /// <summary>
        /// The current after-death player release type
        /// </summary>
        public enum eReleaseType
        {
            /// <summary>
            /// Normal release to the bind point using /release command and 10sec delay after death
            /// </summary>
            Normal,
            /// <summary>
            /// Release to the players home city
            /// </summary>
            City,
            /// <summary>
            /// Release to the current location
            /// </summary>
            Duel,
            /// <summary>
            /// Release to your bind point
            /// </summary>
            Bind,
            /// <summary>
            /// Release in a battleground or the frontiers
            /// </summary>
            RvR,
            /// <summary>
            /// Release to players house
            /// </summary>
            House,
        }

        /// <summary>
        /// The current release type
        /// </summary>
        protected eReleaseType m_releaseType = eReleaseType.Normal;

        /// <summary>
        /// Gets the player's current release type.
        /// </summary>
        public eReleaseType ReleaseType
        {
            get { return m_releaseType; }
        }


        /// <summary>
        /// Gets a new bind point if RvR bind is not valid
        /// </summary>
        /// <param name="player"></param>
        /// <param name="releaseType"></param>
        /// <returns></returns>
        protected eReleaseType GetReleaseTypes(eReleaseType releaseType)
        {
            //RvR enter timer after realm switch
            IList<Account> PlayerAccountName = GameServer.Database.SelectObjects<Account>("`Name` = @Name", new QueryParameter("@Name", Client.Account.Name));

            foreach (Account db in PlayerAccountName)
            {
                if (db.PrivLevel == 1 && db != null && db.LastLoginRealm != 0 && db.IsOtherRealmAllowed == false && (byte)Realm != db.LastLoginRealm && ServerProperties.Properties.LOGIN_OTHER_RELAM_SWITCH_TIMER_MINUTES > 0)
                {
                    if ((DBCharacter.BindRegion == 163 || DBCharacter.BindRegion == 245) && CurrentRegion != null && db.LoginWaitTime > DateTime.Now)
                    {
                        switch (Client.Player.Realm)
                        {
                            case eRealm.Albion:

                                //Sauvage Faste
                                return eReleaseType.City;


                            case eRealm.Midgard:

                                //Sauvage Faste
                                return eReleaseType.City;

                            case eRealm.Hibernia:
                                //Sauvage Faste
                                return eReleaseType.City;

                            default: break;
                        }
                    }
                    else
                        return releaseType;

                }
                else
                    return releaseType;
            }
            return releaseType;
        }

        /// <summary>
        /// Releases this player after death ... subtracts xp etc etc...
        /// </summary>
        /// <param name="releaseCommand">The type of release used for this player</param>
        /// <param name="forced">if true, will release even if not dead</param>
        public virtual void Release(eReleaseType releaseCommand, bool forced)
        {
            DOLCharacters character = DBCharacter;
            if (character == null) return;

            //Get new Bind point if RvR is not valid
            if (releaseCommand == eReleaseType.Normal || releaseCommand == eReleaseType.RvR)
            {
                releaseCommand = GetReleaseTypes(releaseCommand);
            }
            // check if valid housebind
            if (releaseCommand == eReleaseType.House && !(character.BindHouseRegion > 0))
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Release.NoValidBindpoint"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                releaseCommand = eReleaseType.Bind;
            }

            //battlegrounds caps
            Battleground bg = KeepMgr.GetBattleground(CurrentRegionID);
            if (bg != null && releaseCommand == eReleaseType.RvR)
            {
                if (Level > bg.MaxLevel)
                    releaseCommand = eReleaseType.Normal;
            }

            if (IsAlive)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Release.NotDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            

            if (!forced)
            {
                if (m_releaseType == eReleaseType.Duel)
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Release.CantReleaseDuel"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }
                m_releaseType = releaseCommand;
                // we use realtime, because timer window is realtime
                var diff = m_deathTick - GameTimer.GetTickCount() + RELEASE_MINIMUM_WAIT * 1000;
                if (diff >= 1000)
                {
                    if (m_automaticRelease)
                    {
                        m_automaticRelease = false;
                        //Get new Bind point is RvR not valid
                        m_releaseType = GetReleaseTypes(eReleaseType.Normal);
                        // m_releaseType = eReleaseType.Normal;
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Release.NoLongerReleaseAuto", (diff / 1000).ToString()), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    m_automaticRelease = true;
                    switch (releaseCommand)
                    {
                        default:
                            {
                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Release.WillReleaseAuto", (diff / 1000).ToString()), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                        case eReleaseType.City:
                            {
                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Release.WillReleaseAutoCity", (diff / 1000).ToString()), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                        case eReleaseType.RvR:
                            {
                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Release.ReleaseToPortalKeep", (diff / 1000).ToString()), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                        case eReleaseType.House:
                            {
                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Release.ReleaseToHouse", (diff / 1000).ToString()), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                    }
                }
            }
            else
            {
                m_releaseType = releaseCommand;
            }

            int relX = 0, relY = 0, relZ = 0;
            ushort relRegion = 0, relHeading = 0;
            switch (m_releaseType)
            {
                case eReleaseType.Duel:
                    {
                        relRegion = (ushort)character.Region;
                        relX = character.Xpos;
                        relY = character.Ypos;
                        relZ = character.Zpos;
                        relHeading = 2048;
                        break;
                    }
                case eReleaseType.House:
                    {
                        relRegion = (ushort)character.BindHouseRegion;
                        relX = character.BindHouseXpos;
                        relY = character.BindHouseYpos;
                        relZ = character.BindHouseZpos;
                        relHeading = (ushort)character.BindHouseHeading;
                        break;
                    }

                case eReleaseType.City:
                    {
                        if (Realm == eRealm.Hibernia)
                        {
                            relRegion = 201; // Tir Na Nog
                            relX = 8192 + 15780;
                            relY = 8192 + 22727;
                            relZ = 7060;
                        }
                        else if (Realm == eRealm.Midgard)
                        {
                            relRegion = 101; // Jordheim
                            relX = 8192 + 24664;
                            relY = 8192 + 21402;
                            relZ = 8759;
                        }
                        else
                        {
                            relRegion = 10; // City of Camelot
                            relX = 8192 + 26315;
                            relY = 8192 + 21177;
                            relZ = 8256;
                        }
                        relHeading = 2048;
                        break;
                    }
                case eReleaseType.RvR:
                    {
                        foreach (AbstractGameKeep keep in KeepMgr.GetKeepsOfRegion(CurrentRegionID))
                        {
                            if (keep.IsPortalKeep && keep.OriginalRealm == Realm)
                            {
                                relRegion = keep.CurrentRegion.ID;
                                relX = keep.X;
                                relY = keep.Y;
                                relZ = keep.Z;
                            }
                        }

                        //if we aren't releasing anywhere, release to the border keeps
                        if (relX == 0)
                        {
                            relRegion = CurrentRegion.ID;
                            KeepMgr.GetBorderKeepLocation(((byte)Realm * 2) / 1, out relX, out relY, out relZ, out relHeading);
                        }
                        break;
                    }
                default:
                    {
                        if (!ServerProperties.Properties.DISABLE_TUTORIAL)
                        {
                            //Tutorial
                            if (character.BindRegion == 27)
                            {
                                switch (Realm)
                                {
                                    case eRealm.Albion:
                                        {
                                            relRegion = 1; // Cotswold
                                            relX = 8192 + 553251;
                                            relY = 8192 + 502936;
                                            relZ = 2280;
                                            break;
                                        }
                                    case eRealm.Midgard:
                                        {
                                            relRegion = 100; // Mularn
                                            relX = 8192 + 795621;
                                            relY = 8192 + 719590;
                                            relZ = 4680;
                                            break;
                                        }
                                    case eRealm.Hibernia:
                                        {
                                            relRegion = 200; // MagMell
                                            relX = 8192 + 338652;
                                            relY = 8192 + 482335;
                                            relZ = 5200;
                                            break;
                                        }
                                }
                                break;
                            }
                        }
                        switch (CurrentRegionID)
                        {
                            //battlegrounds
                            case 234:
                            case 235:
                            case 236:
                            case 237:
                            case 238:
                            case 239:
                            case 240:
                            case 241:
                            case 242:
                                {
                                    //get the bg cap
                                    byte cap = 50;
                                    foreach (AbstractGameKeep keep in KeepMgr.GetKeepsOfRegion(CurrentRegionID))
                                    {
                                        if (keep.DBKeep.BaseLevel < cap)
                                        {
                                            cap = keep.DBKeep.BaseLevel;
                                            break;
                                        }
                                    }
                                    //get the portal location
                                    foreach (AbstractGameKeep keep in KeepMgr.GetKeepsOfRegion(CurrentRegionID))
                                    {
                                        if (keep.DBKeep.BaseLevel > 50 && keep.Realm == Realm)
                                        {
                                            relRegion = (ushort)keep.Region;
                                            relX = keep.X;
                                            relY = keep.Y;
                                            relZ = keep.Z;
                                            break;
                                        }
                                    }
                                    break;
                                }
                            //nf
                            case 163:
                                {
                                    if (character.BindRegion != 163)
                                    {
                                        relRegion = 163;
                                        switch (Realm)
                                        {
                                            case eRealm.Albion:
                                                {
                                                    KeepMgr.GetBorderKeepLocation(1, out relX, out relY, out relZ, out relHeading);
                                                    break;
                                                }
                                            case eRealm.Midgard:
                                                {
                                                    KeepMgr.GetBorderKeepLocation(3, out relX, out relY, out relZ, out relHeading);
                                                    break;
                                                }
                                            case eRealm.Hibernia:
                                                {
                                                    KeepMgr.GetBorderKeepLocation(5, out relX, out relY, out relZ, out relHeading);
                                                    break;
                                                }
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        relRegion = (ushort)character.BindRegion;
                                        relX = character.BindXpos;
                                        relY = character.BindYpos;
                                        relZ = character.BindZpos;
                                        relHeading = (ushort)character.BindHeading;
                                    }
                                    break;
                                }/*
								//bg45-49
							case 165:
								{
									break;
								}*/
                            default:
                                {
                                    relRegion = (ushort)character.BindRegion;
                                    relX = character.BindXpos;
                                    relY = character.BindYpos;
                                    relZ = character.BindZpos;
                                    relHeading = (ushort)character.BindHeading;
                                    break;
                                }
                        }
                        break;
                    }
            }

            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Release.YouRelease"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
            Out.SendCloseTimerWindow();
            if (m_releaseTimer != null)
            {
                m_releaseTimer.Stop();
                m_releaseTimer = null;
            }

            if (Realm != eRealm.None)
            {
                if (Level > 5)
                {
                    // actual lost exp, needed for 2nd stage deaths
                    long lostExp = Experience;
                    long lastDeathExpLoss = TempProperties.getProperty<long>(DEATH_EXP_LOSS_PROPERTY);
                    TempProperties.removeProperty(DEATH_EXP_LOSS_PROPERTY);

                    GainExperience(GameLiving.eXPSource.Other, -lastDeathExpLoss);
                    lostExp -= Experience;

                    // raise only the gravestone if xp has to be stored in it
                    if (lostExp > 0)
                    {
                        // find old gravestone of player and remove it
                        if (character.HasGravestone)
                        {
                            Region reg = WorldMgr.GetRegion((ushort)character.GravestoneRegion);
                            if (reg != null)
                            {
                                GameGravestone oldgrave = reg.FindGraveStone(this);
                                if (oldgrave != null)
                                {
                                    oldgrave.Delete();
                                }
                            }
                            character.HasGravestone = false;
                        }

                        GameGravestone gravestone = new GameGravestone(this, lostExp);
                        gravestone.AddToWorld();
                        character.GravestoneRegion = gravestone.CurrentRegionID;
                        character.HasGravestone = true;
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Release.GraveErected"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Release.ReturnToPray"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
                    }
                }
            }

            if (Level > 10)
            {
                int deathConLoss = TempProperties.getProperty<int>(DEATH_CONSTITUTION_LOSS_PROPERTY); // get back constitution lost at death
                if (deathConLoss > 0)
                {
                    TotalConstitutionLostAtDeath += deathConLoss;
                    Out.SendCharStatsUpdate();
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Release.LostConstitution"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
                }
            }

            //Update health&sit state first!
            Health = MaxHealth;
            StartPowerRegeneration();
            StartEnduranceRegeneration();
            BroadcastUpdate();

            Region region;
            Zone regionZone;

            region = WorldMgr.GetRegion((ushort)character.BindRegion);
            regionZone = region.GetZone(character.BindXpos, character.BindYpos);
            if (region != null && regionZone != null)
            {
                if (Level > 10 && m_releaseType != eReleaseType.Duel)
                {
                    CastSpell(SkillBase.GetSpellByID(RVRIllnes), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Release.SurroundingChange"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
                }
                else if (Level <= 10 && m_releaseType != eReleaseType.Duel)
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Release.SurroundingChange"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
                }
                else if (m_releaseType == eReleaseType.Duel)
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Release.SurroundingChange"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
                }
            }
            else
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Release.NoValidBindpoint"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                Bind(true);
            }


            int oldRegion = CurrentRegionID;

            //Call MoveTo after new GameGravestone(this...
            //or the GraveStone will be located at the player's bindpoint

            MoveTo(relRegion, relX, relY, relZ, relHeading);
            //It is enough if we revive the player on this client only here
            //because for other players the player will be removed in the MoveTo
            //method and added back again (if in view) with full health ... so no
            //revive needed for others...
            Out.SendPlayerRevive(this);
            //			Out.SendUpdatePlayer();
            Out.SendUpdatePoints();

            //Set property indicating that we are releasing to another region; used for Released event
            if (oldRegion != character.BindRegion)
                TempProperties.setProperty(RELEASING_PROPERTY, true);
            else
            {
                // fire the player revive event
                Notify(GamePlayerEvent.Revive, this);
                Notify(GamePlayerEvent.Released, this);
            }

            TempProperties.removeProperty(DEATH_CONSTITUTION_LOSS_PROPERTY);

            //Reset last valide position array to prevent /stuck avec /release
            lock (m_lastUniqueLocations)
            {
                for (int i = 0; i < m_lastUniqueLocations.Length; i++)
                {
                    GameLocation loc = m_lastUniqueLocations[i];
                    loc.X = X;
                    loc.Y = Y;
                    loc.Z = Z;
                    loc.Heading = Heading;
                    loc.RegionID = CurrentRegionID;
                }
            }
        }

         
        /// <summary>
        /// helper state var for different release phases
        /// </summary>
        private byte m_releasePhase = 0;

        /// <summary>
        /// callback every second to control realtime release
        /// </summary>
        /// <param name="callingTimer"></param>
        /// <returns></returns>
        protected virtual int ReleaseTimerCallback(RegionTimer callingTimer)
        {
            if (IsAlive)
                return 0;
            var diffToRelease = GameTimer.GetTickCount() - m_deathTick;
            if (m_automaticRelease && diffToRelease > RELEASE_MINIMUM_WAIT * 1000)
            {
                Release(m_releaseType, true);
                return 0;
            }
            diffToRelease = (RELEASE_TIME * 1000 - diffToRelease) / 1000;
            if (diffToRelease <= 0)
            {
                Release(m_releaseType, true);
                return 0;
            }
            if (m_releasePhase <= 1 && diffToRelease <= 10 && diffToRelease >= 8)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Release.WillReleaseIn", 10.ToString()), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                m_releasePhase = 2;
            }
            if (m_releasePhase == 0 && diffToRelease <= 30 && diffToRelease >= 28)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Release.WillReleaseIn", 30.ToString()), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                m_releasePhase = 1;
            }
            return 1000;
        }

        /// <summary>
        /// Called when player revive
        /// </summary>
        public virtual void OnRevive(DOLEvent e, object sender, EventArgs args)
        {
            if (sender is GamePlayer player) // Überprüfung, ob sender null ist und den erwarteten Typ hat
            {
                //GamePlayer player = (GamePlayer)sender;

                if (player.IsUnderwater && player.CanBreathUnderWater == false)
                    player.Diving(WaterBreath.Holding);

                if (player.Level > 5)
                {
                    // get illness after level 5
                    SpellLine Line = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
                    if (Line == null) return;
                    ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(player, GlobalSpells.PvERezIllness, Line);
                    spellHandler.StartSpell(player);
                }
                GameEventMgr.RemoveHandler(this, GamePlayerEvent.Revive, new DOLEventHandler(OnRevive));
            }
        }



        /// <summary>
        /// Property that saves experience lost on last death
        /// </summary>
        public const string DEATH_EXP_LOSS_PROPERTY = "death_exp_loss";
        /// <summary>
        /// Property that saves condition lost on last death
        /// </summary>
        public const string DEATH_CONSTITUTION_LOSS_PROPERTY = "death_con_loss";

        #endregion

        #region Praying
        /// <summary>
        /// The timer that will be started when the player wants to pray
        /// </summary>
        protected RegionTimerAction<GameGravestone> m_prayAction;
        /// <summary>
        /// The delay to wait until xp is regained, in milliseconds
        /// </summary>
        protected virtual ushort PrayDelay { get { return 5000; } }
        /// <summary>
        /// Gets the praying-state of this living
        /// </summary>
        public virtual bool IsPraying
        {
            get { return m_prayAction != null && m_prayAction.IsAlive; }
        }

        /// <summary>
        /// Prays on a gravestone for XP!
        /// </summary>
        public virtual void Pray()
        {
            string cantPrayMessage = string.Empty;
            GameGravestone gravestone = TargetObject as GameGravestone;

            if (!IsAlive)
                cantPrayMessage = "GamePlayer.Pray.CantPrayNow";
            else if (IsRiding)
                cantPrayMessage = "GamePlayer.Pray.CantPrayRiding";
            else if (gravestone == null)
                cantPrayMessage = "GamePlayer.Pray.NeedTarget";
            else if (!gravestone.InternalID.Equals(InternalID))
                cantPrayMessage = "GamePlayer.Pray.SelectGrave";
            else if (!IsWithinRadius(gravestone, 2000))
                cantPrayMessage = "GamePlayer.Pray.MustGetCloser";
            else if (IsMoving)
                cantPrayMessage = "GamePlayer.Pray.MustStandingStill";
            else if (IsPraying)
                cantPrayMessage = "GamePlayer.Pray.AlreadyPraying";

            if (cantPrayMessage != string.Empty)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client, cantPrayMessage), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (m_prayAction != null)
                m_prayAction.Stop();

            m_prayAction = new RegionTimerAction<GameGravestone>(gravestone, stn =>
            {
                if (stn.XPValue > 0)
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Pray.GainBack"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    GainExperience(eXPSource.Praying, stn.XPValue);
                }
                stn.XPValue = 0;
                stn.Delete();
            });
            m_prayAction.Start(PrayDelay);

            Sit(true);
            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Pray.Begin"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

            foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                if (player == null) continue;
                player.Out.SendEmoteAnimation(this, eEmote.Pray);
            }
        }

        /// <summary>
        /// Stop praying; used when player changes target
        /// </summary>
        public void PrayTimerStop()
        {
            if (!IsPraying)
                return;
            m_prayAction.Stop();
            m_prayAction = null;
        }
        #endregion



        #region Name/LastName/GuildName/Model

        /// <summary>
        /// The lastname of this player
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual string LastName
        {
            get { return DBCharacter != null ? DBCharacter.LastName : string.Empty; }
            set
            {
                if (DBCharacter == null) return;
                DBCharacter.LastName = value;
                //update last name for all players if client is playing
                if (ObjectState == eObjectState.Active)
                {
                    Out.SendUpdatePlayer();
                    foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    {
                        if (player == null) continue;
                        if (player != this)
                        {
                            player.Out.SendObjectRemove(this);
                            player.Out.SendPlayerCreate(this);
                            player.Out.SendLivingEquipmentUpdate(this);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the guildname of this player
        /// (delegate to PlayerCharacter)
        /// </summary>
        public override string GuildName
        {
            get
            {
                // Rückgabe eines leeren Strings, wenn m_guild null ist, um Null-Referenz zu vermeiden
                if (m_guild == null)
                    return "";
                // Gibt den Namen der Gilde zurück
                return m_guild.Name;
            }
            // Keine Notwendigkeit für eine Set-Methode, wenn m_guild null sein kann
            set { }
        }

        /// <summary>
        /// Gets or sets the name of the player
        /// (delegate to PlayerCharacter)
        /// </summary>
        public override string Name
        {
            get { return DBCharacter != null ? DBCharacter.Name : string.Empty; }
            set
            {
                var oldname = base.Name;
                base.Name = value;

                if (DBCharacter == null) return;
                DBCharacter.Name = value;

                if (oldname != value)
                {
                    //base.Name = value; // only need if there will be some special code in base-property in future
                    //update name for all players if client is playing
                    if (ObjectState == eObjectState.Active)
                    {
                        Out.SendUpdatePlayer();
                        if (Group != null)
                            Out.SendGroupWindowUpdate();
                        foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                        {
                            if (player == null) continue;
                            if (player != this)
                            {
                                player.Out.SendObjectRemove(this);
                                player.Out.SendPlayerCreate(this);
                                player.Out.SendLivingEquipmentUpdate(this);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The prefix for RR 12/13 players
        /// </summary>
        public virtual string PrefixName
        {
            get
            {
                if (RealmLevel >= 110)
                    return RealmTitle;
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// Sets or gets the model of the player. If the player is
        /// active in the world, the modelchange will be visible
        /// (delegate to PlayerCharacter)
        /// </summary>
        /// <remarks>
        /// The model of a GamePlayer is a 16-bit unsigned integer.
        /// The leftmost 3 bits are related to hair color.
        /// The next 2 bits are for the size: 01 = short, 10 = average, 11 = tall (00 appears to be average as well)
        /// The remaining 11 bits are for the model (see monsters.csv in gamedata.mpk)
        /// </remarks>
        public override ushort Model
        {
            get
            {
                return base.Model;
            }
            set
            {
                if (base.Model != value)
                {
                    base.Model = value;

                    // Only GM's can persist model changes - Tolakram
                    if (Client.Account.PrivLevel > (int)ePrivLevel.Player && DBCharacter != null && DBCharacter.CurrentModel != base.Model)
                    {
                        DBCharacter.CurrentModel = base.Model;
                    }

                    if (ObjectState == eObjectState.Active)
                    {
                        Notify(GamePlayerEvent.ModelChanged, this);

                        foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                        {
                            if (player == null) continue;
                            player.Out.SendModelChange(this, Model);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Male or Female (from DBCharacter)
        /// Note: DB Gender is 0=male, 1=female while enum is 0=neutral, 1=male, 2=female
        /// </summary>
        public override eGender Gender
        {
            get
            {
                if (DBCharacter.Gender == 0)
                {
                    return eGender.Male;
                }

                return eGender.Female;
            }
            set
            {
            }
        }


        public enum eSize : ushort
        {
            Short = 0x800,
            Average = 0x1000,
            Tall = 0x1800
        }


        public eSize Size
        {
            get
            {
                ushort size = (ushort)(Model & (ushort)eSize.Tall);

                switch (size)
                {
                    case 0x800: return eSize.Short;
                    case 0x1800: return eSize.Tall;
                    default: return eSize.Average;
                }
            }

            set
            {
                if (value != Size)
                {
                    ushort modelID = (ushort)(Model & 0x7FF);

                    Model = (ushort)(modelID | (ushort)value);
                }
            }
        }

        #endregion

        #region Stats

        /// <summary>
        /// Holds if the player can gain a FreeLevel
        /// </summary>
        public virtual byte FreeLevelState
        {
            get
            {
                int freelevel_days = 7;
                switch (Realm)
                {
                    case eRealm.Albion:
                        if (ServerProperties.Properties.FREELEVEL_DAYS_ALBION == -1)
                            return 1;
                        else
                            freelevel_days = ServerProperties.Properties.FREELEVEL_DAYS_ALBION;
                        break;
                    case eRealm.Midgard:
                        if (ServerProperties.Properties.FREELEVEL_DAYS_MIDGARD == -1)
                            return 1;
                        else
                            freelevel_days = ServerProperties.Properties.FREELEVEL_DAYS_MIDGARD;
                        break;
                    case eRealm.Hibernia:
                        if (ServerProperties.Properties.FREELEVEL_DAYS_HIBERNIA == -1)
                            return 1;
                        else
                            freelevel_days = ServerProperties.Properties.FREELEVEL_DAYS_HIBERNIA;
                        break;
                }
                //flag 1 = above level, 2 = elligable, 3= time until, 4 = level and time until, 5 = level until
                if (Level >= 48)
                    return 1;
                TimeSpan t = new TimeSpan((long)(DateTime.Now.Ticks - DBCharacter.LastFreeLeveled.Ticks));
                if (t.Days >= freelevel_days)
                {
                    if (Level >= DBCharacter.LastFreeLevel + 2)
                        return 2;
                    else return 5;
                }
                else
                {
                    if (Level >= DBCharacter.LastFreeLevel + 2)
                        return 3;
                    else return 4;
                }
            }
        }

        /// <summary>
        /// Gets/sets the player efficacy percent
        /// (delegate to PlayerCharacter)
        /// </summary>
        public int TotalConstitutionLostAtDeath
        {
            get { return DBCharacter != null ? DBCharacter.ConLostAtDeath : 0; }
            set { if (DBCharacter != null) DBCharacter.ConLostAtDeath = value; }
        }

        /// <summary>
        /// Change a stat value
        /// (delegate to PlayerCharacter)
        /// </summary>
        /// <param name="stat">The stat to change</param>
        /// <param name="val">The new value</param>
        public override void ChangeBaseStat(eStat stat, short val)
        {
            int oldstat = GetBaseStat(stat);
            base.ChangeBaseStat(stat, val);
            int newstat = GetBaseStat(stat);
            DOLCharacters character = DBCharacter; // to call it only once, if in future there will be some special code to get the character
                                                   // Graveen: always positive and not null. This allows /player stats to substract values safely
            if (newstat < 1) newstat = 1;
            if (character != null && oldstat != newstat)
            {
                switch (stat)
                {
                    case eStat.STR: character.Strength = newstat; break;
                    case eStat.DEX: character.Dexterity = newstat; break;
                    case eStat.CON: character.Constitution = newstat; break;
                    case eStat.QUI: character.Quickness = newstat; break;
                    case eStat.INT: character.Intelligence = newstat; break;
                    case eStat.PIE: character.Piety = newstat; break;
                    case eStat.EMP: character.Empathy = newstat; break;
                    case eStat.CHR: character.Charisma = newstat; break;
                }
            }
        }

        /// <summary>
        /// Gets player's constitution
        /// </summary>
        public int Constitution
        {
            //get { return GetModified(eProperty.Constitution); }
            get { return DBCharacter != null ? GetModified(eProperty.Constitution) : 0; }
        }

        /// <summary>
        /// Gets player's dexterity
        /// </summary>
        public int Dexterity
        {
            get { return GetModified(eProperty.Dexterity); }
        }

        /// <summary>
        /// Gets player's strength
        /// </summary>
        public int Strength
        {
            get { return GetModified(eProperty.Strength); }
        }

        /// <summary>
        /// Gets player's quickness
        /// </summary>
        public int Quickness
        {
            get { return GetModified(eProperty.Quickness); }
        }

        /// <summary>
        /// Gets player's intelligence
        /// </summary>
        public int Intelligence
        {
            get { return GetModified(eProperty.Intelligence); }
        }

        /// <summary>
        /// Gets player's piety
        /// </summary>
        public int Piety
        {
            get { return GetModified(eProperty.Piety); }
        }

        /// <summary>
        /// Gets player's empathy
        /// </summary>
        public int Empathy
        {
            get { return GetModified(eProperty.Empathy); }
        }

        /// <summary>
        /// Gets player's charisma
        /// </summary>
        public int Charisma
        {
            get { return GetModified(eProperty.Charisma); }
        }

        protected IPlayerStatistics m_statistics = null;

        /// <summary>
        /// Get the statistics for this player
        /// </summary>
        public virtual IPlayerStatistics Statistics
        {
            get { return m_statistics; }
        }

        /// <summary>
        /// Create played statistics for this player
        /// </summary>
        public virtual void CreateStatistics()
        {
            m_statistics = new PlayerStatistics(this);
        }

        /// <summary>
        /// Formats this players statistics.
        /// </summary>
        /// <returns>List of strings.</returns>
        public virtual IList<string> FormatStatistics()
        {
            return GameServer.ServerRules.FormatPlayerStatistics(this);
        }

        #endregion

        #region Health/Mana/Endurance/Regeneration

        /// <summary>
        /// Starts the health regeneration.
        /// Overriden. No lazy timers for GamePlayers.
        /// </summary>
        public override void StartHealthRegeneration()
        {
            if (ObjectState != eObjectState.Active) return;
            if (m_healthRegenerationTimer.IsAlive) return;
            m_healthRegenerationTimer.Start(m_healthRegenerationPeriod);
        }
        /// <summary>
        /// Starts the power regeneration.
        /// Overriden. No lazy timers for GamePlayers.
        /// </summary>
        public override void StartPowerRegeneration()
        {
            if (ObjectState != eObjectState.Active) return;
            if (m_powerRegenerationTimer.IsAlive) return;
            m_powerRegenerationTimer.Start(m_powerRegenerationPeriod);
        }
        /// <summary>
        /// Starts the endurance regeneration.
        /// Overriden. No lazy timers for GamePlayers.
        /// </summary>
        public override void StartEnduranceRegeneration()
        {
            if (ObjectState != eObjectState.Active) return;
            if (m_enduRegenerationTimer.IsAlive) return;
            m_enduRegenerationTimer.Start(m_enduranceRegenerationPeriod);
        }
        /// <summary>
        /// Stop the health regeneration.
        /// Overriden. No lazy timers for GamePlayers.
        /// </summary>
        public override void StopHealthRegeneration()
        {
            if (m_healthRegenerationTimer == null) return;
            m_healthRegenerationTimer.Stop();
        }
        /// <summary>
        /// Stop the power regeneration.
        /// Overriden. No lazy timers for GamePlayers.
        /// </summary>
        public override void StopPowerRegeneration()
        {
            if (m_powerRegenerationTimer == null) return;
            m_powerRegenerationTimer.Stop();
        }
        /// <summary>
        /// Stop the endurance regeneration.
        /// Overriden. No lazy timers for GamePlayers.
        /// </summary>
        public override void StopEnduranceRegeneration()
        {
            if (m_enduRegenerationTimer == null) return;
            m_enduRegenerationTimer.Stop();
        }

        /// <summary>
        /// Override HealthRegenTimer because if we are not connected anymore
        /// we DON'T regenerate health, even if we are not garbage collected yet!
        /// </summary>
        /// <param name="callingTimer">the timer</param>
        /// <returns>the new time</returns>
        protected override int HealthRegenerationTimerCallback(RegionTimer callingTimer)
        {
            ushort regSpeedup = 3000;
            // I'm not sure what the point of this is.
            if (Client.ClientState != GameClient.eClientState.Playing)
                return HealthRegenerationPeriod;

            // adjust timer based on Live testing of player

            if (Health < MaxHealth)
            {
                ChangeHealth(this, eHealthChangeType.Regenerate, GetModified(eProperty.HealthRegenerationRate));
            }
            #region PVP DAMAGE
            if (DamageRvRMemory > 0)
                DamageRvRMemory -= (long)Math.Max(GetModified(eProperty.HealthRegenerationRate), 0);

            #endregion PVP DAMAGE
            //If we are fully healed, we stop the timer
            if (Health >= MaxHealth)
            {
                #region PVP DAMAGE

                // Fully Regenerated, Set DamageRvRMemory to 0
                if (DamageRvRMemory > 0)
                    DamageRvRMemory = 0;

                #endregion PVP DAMAGE
                //We clean all damagedealers if we are fully healed,
                //no special XP calculations need to be done
                if (XPGainers != null)
                    _XPGainers.Clear();
               

                return 0;
            }

            if (InCombat)
            {
                // in combat each tic is aprox 6 seconds - tolakram
                return HealthRegenerationPeriod * 2;
            }

            if (HealthPercent >= 50 && InCombat == false)
            {
                regSpeedup = HealthRegenerationPeriod;
                regSpeedup /= 2;
                return regSpeedup;
            }
            //Heal at standard rate
            return HealthRegenerationPeriod;
        }

        /// <summary>
        /// Override PowerRegenTimer because if we are not connected anymore
        /// we DON'T regenerate mana, even if we are not garbage collected yet!
        /// </summary>
        /// <param name="selfRegenerationTimer">the timer</param>
        /// <returns>the new time</returns>
        protected override int PowerRegenerationTimerCallback(RegionTimer selfRegenerationTimer)
        {
            if (Client.ClientState != GameClient.eClientState.Playing)
                return PowerRegenerationPeriod;
            int interval = base.PowerRegenerationTimerCallback(selfRegenerationTimer);
            return interval;
        }

        /// <summary>
        /// Override EnduranceRegenTimer because if we are not connected anymore
        /// we DON'T regenerate endurance, even if we are not garbage collected yet!
        /// </summary>
        /// <param name="selfRegenerationTimer">the timer</param>
        /// <returns>the new time</returns>
        protected override int EnduranceRegenerationTimerCallback(RegionTimer selfRegenerationTimer)
        {
            if (Client.ClientState != GameClient.eClientState.Playing)
                return EnduranceRegenerationPeriod;

            bool sprinting = IsSprinting;

            if (Endurance < MaxEndurance || sprinting)
            {
                int regen = GetModified(eProperty.EnduranceRegenerationRate);

                int longwind = 5;
                if (sprinting && IsMoving)
                {
                    //TODO : cache LongWind level when char is loaded and on train ability
                    LongWindAbility ra = GetAbility<LongWindAbility>();
                    if (ra != null)
                        longwind = 5 - (ra.GetAmountForLevel(CalculateSkillLevel(ra)) * 5 / 100);

                    regen -= longwind;
                    if (Endurance + regen > MaxEndurance - longwind)
                    {
                        regen -= (Endurance + regen) - (MaxEndurance - longwind);
                    }
                }

                if (regen != 0)
                {
                    ChangeEndurance(this, eEnduranceChangeType.Regenerate, regen);
                }
            }
            if (!sprinting)
            {
                if (Endurance >= MaxEndurance) return 0;
            }
            else
            {
                long lastmove = TempProperties.getProperty<long>(PlayerPositionUpdateHandler.LASTMOVEMENTTICK);
                if ((lastmove > 0 && lastmove + 5000 < CurrentRegion.Time) //cancel sprint after 5sec without moving?
                    || Endurance - 5 <= 0)
                    Sprint(false);
            }

            return 500 + Util.Random(EnduranceRegenerationPeriod);
        }

        /// <summary>
        /// Gets/sets the object health
        /// </summary>
        public override int Health
        {
            // Rückgabe der Gesundheit vom DBCharacter oder der Basis-Gesundheit
            get { return DBCharacter != null ? DBCharacter.Health : base.Health; }
            set
            {
                // Sicherstellen, dass DBCharacter nicht null ist
                if (DBCharacter != null)
                {
                    // Clamp Wert zwischen 0 und MaxHealth
                    value = value.Clamp(0, MaxHealth);
                    // Wenn der neue Wert gleich der aktuellen Gesundheit ist, führe keine weiteren Aktionen aus
                    if (Health == value)
                    {
                        base.Health = value; // benötigt um die Regeneration zu starten
                        return;
                    }

                    int oldPercent = HealthPercent;
                    DBCharacter.Health = value; // Aktualisierung der Gesundheit im DBCharacter
                    base.Health = value;
                    // Nur Gruppenupdate, wenn sich die Prozentzahl geändert hat 
                    if (oldPercent != HealthPercent)
                    {
                        // Überprüfen, ob die Gruppe nicht null ist, bevor die Methode aufgerufen wird
                        Group?.UpdateMember(this, false, false);
                        UpdatePlayerStatus();
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the maximum health for a specific playerlevel and constitution
        /// </summary>
        /// <param name="level">The level of the player</param>
        /// <param name="constitution">The constitution of the player</param>
        /// <returns></returns>
        public virtual int CalculateMaxHealth(int level, int constitution)
        {
                 
                constitution -= 50;
           if (constitution < 0) 
                constitution *= 2;
                       
            // hp1 : from level
            // hp2 : from constitution
            // hp3 : from champions level
            // hp4 : from artifacts such Spear of Kings charge
            int hp1 = CharacterClass.BaseHP * level;
            int hp2 = hp1 * constitution / 10000;
            int hp3 = 0;
            if (ChampionLevel >= 1)
                hp3 = ServerProperties.Properties.HPS_PER_CHAMPIONLEVEL * ChampionLevel;
            double hp4 = 20 + hp1 / 50 + hp2 + hp3;
            if (GetModified(eProperty.ExtraHP) > 0)
                hp4 += Math.Round(hp4 * (double)GetModified(eProperty.ExtraHP) / 100);

            return Math.Max(1, (int)hp4);
        }

        public override byte HealthPercentGroupWindow
        {
            get
            {
                return CharacterClass.HealthPercentGroupWindow;
            }
        }

        /// <summary>
        /// Calculate max mana for this player based on level and mana stat level
        /// </summary>
        /// <param name="level"></param>
        /// <param name="manaStat"></param>
        /// <returns></returns>
        public virtual int CalculateMaxMana(int level, int manaStat)
        {

            int maxpower = 0;

            //Special handling for Vampiirs:
            /* There is no stat that affects the Vampiir's power pool or the damage done by its power based spells.
			 * The Vampiir is not a focus based class like, say, an Enchanter.
			 * The Vampiir is a lot more cut and dried than the typical casting class.
			 * EDIT, 12/13/04 - I was told today that this answer is not entirely accurate.
			 * While there is no stat that affects the damage dealt (in the way that intelligence or piety affects how much damage a more traditional caster can do),
			 * the Vampiir's power pool capacity is intended to be increased as the Vampiir's strength increases.
			 *
			 * This means that strength ONLY affects a Vampiir's mana pool
			 * 
			 * http://www.camelotherald.com/more/1913.shtml
			 * Strength affects the amount of damage done by spells in all of the Vampiir's spell lines.
			 * The amount of said affecting was recently increased slightly (fixing a bug), and that minor increase will go live in 1.74 next week.
			 * 
			 * Strength ALSO affects the size of the power pool for a Vampiir… sort of.
			 * Your INNATE strength (the number of attribute points your character has for strength) has no effect at all.
			 * Extra points added through ITEMS, however, does increase the size of your power pool.

			 */
            if (CharacterClass.ManaStat != eStat.UNDEFINED || IsCharcterClass(eCharacterClass.Vampiir))
            {
                maxpower = Math.Max(5, (level * 5) + (manaStat - 50));
            }
            else if (CharacterClass.ManaStat == eStat.UNDEFINED && Champion && ChampionLevel > 0)
            {
                maxpower = 100; // This is a guess, need feedback
            }

            if (maxpower < 0)
                maxpower = 0;

            return maxpower;
        }


        /// <summary>
        /// Gets/sets the object mana
        /// </summary>
        public override int Mana
        {
            get { return DBCharacter != null ? DBCharacter.Mana : base.Mana; }
            set
            {
                value = Math.Min(value, MaxMana);
                value = Math.Max(value, 0);
                //If it is already set, don't do anything
                if (Mana == value)
                {
                    base.Mana = value; //needed to start regeneration
                    return;
                }
                int oldPercent = ManaPercent;
                base.Mana = value;
                if (DBCharacter != null)
                    DBCharacter.Mana = value;
                if (oldPercent != ManaPercent)
                {
                    if (Group != null)
                        Group.UpdateMember(this, false, false);
                    UpdatePlayerStatus();
                }
            }
        }

        /// <summary>
        /// Gets/sets the object max mana
        /// </summary>
        public override int MaxMana
        {
            get { return GetModified(eProperty.MaxMana); }
        }

        /// <summary>
        /// Gets/sets the object endurance
        /// </summary>
        public override int Endurance
        {
            get { return DBCharacter != null ? DBCharacter.Endurance : base.Endurance; }
            set
            {
                value = Math.Min(value, MaxEndurance);
                value = Math.Max(value, 0);
                //If it is already set, don't do anything
                if (Endurance == value)
                {
                    base.Endurance = value; //needed to start regeneration
                    return;
                }
                else if (IsAlive && value < MaxEndurance)
                    StartEnduranceRegeneration();
                int oldPercent = EndurancePercent;
                base.Endurance = value;
                if (DBCharacter != null)
                    DBCharacter.Endurance = value;
                if (oldPercent != EndurancePercent)
                {
                    //ogre: 1.69+ endurance is displayed on group window
                    if (Group != null)
                        Group.UpdateMember(this, false, false);
                    //end ogre
                    UpdatePlayerStatus();
                }
            }
        }

        /// <summary>
        /// Gets/sets the objects maximum endurance
        /// </summary>
        public override int MaxEndurance
        {
            get { return GetModified(eProperty.Fatigue); }
            set
            {
                //If it is already set, don't do anything
                if (MaxEndurance == value)
                    return;
                base.MaxEndurance = value;
                if (DBCharacter != null)
                    DBCharacter.MaxEndurance = value;
                UpdatePlayerStatus();
            }
        }

        /// <summary>
        /// Gets the concentration left
        /// </summary>
        public override int Concentration
        {
            get { return MaxConcentration - ConcentrationEffects.UsedConcentration; }
        }

        /// <summary>
        /// Gets the maximum concentration for this player
        /// </summary>
        public override int MaxConcentration
        {
            get { return GetModified(eProperty.MaxConcentration); }
        }
        #region Calculate Fall Damage

        /// <summary>
		/// Calculates fall damage taking fall damage reduction bonuses into account
		/// </summary>
		/// <returns></returns>
		public virtual double CalcFallDamage(int fallDamagePercent)
        {
            if (fallDamagePercent <= 0)
                return 0;

            int safeFallLevel = GetAbilityLevel(Abilities.SafeFall);
            int mythSafeFall = GetModified(eProperty.MythicalSafeFall);
            int safeCloudSongFall = 0;

            // [Freya] Nidel: CloudSong falling damage reduction changed by Elcotek
            GameSpellEffect cloudSongFall = SpellHandler.FindEffectOnTarget(this, "CloudsongFall");

            if (cloudSongFall != null)
            {
                safeCloudSongFall = (int)cloudSongFall.Spell.Value;
                mythSafeFall += safeCloudSongFall;

                if (mythSafeFall >= 100)
                {
                    mythSafeFall = 99;
                }
            }

            if (mythSafeFall > 0 & fallDamagePercent > mythSafeFall)
            {

                fallDamagePercent = mythSafeFall;
                Client.Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "PlayerPositionUpdateHandler.MythSafeFall"), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
            }

            if (safeFallLevel > 0 & mythSafeFall == 0)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "PlayerPositionUpdateHandler.SafeFall"),
                                eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
            }

            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "PlayerPositionUpdateHandler.FallPercent", fallDamagePercent), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);


            Endurance -= MaxEndurance * fallDamagePercent / 100;
            double damage = (0.01 * fallDamagePercent * (MaxHealth - 1));


            TakeDamage(null, eDamageType.Falling, (int)damage, 0);

            //Update the player's health to all other players around
            foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                if (player == null) continue;
                Out.SendCombatAnimation(null, Client.Player, 0, 0, 0, 0, 0, HealthPercent);
            }
            return damage;
        }


        #endregion
        #endregion

        #region Class/Race

        /// <summary>
        /// Returns localized race names
        /// </summary>
        /// <param name="client"></param>
        /// <param name="Race"></param>
        /// <param name="Gender"></param>
        /// <returns></returns>
        public static string RACENAMES(GameClient client, int Race, int Gender)
        {
            eRace r = (eRace)Race;

            if (r != 0)
            {
                switch (Gender)
                {
                    case 0:
                        return LanguageMgr.GetTranslation(client.Account.Language, "GamePlayer.PlayerRace.Male." + r.ToString("F"));
                    case 1:
                        return LanguageMgr.GetTranslation(client.Account.Language, "GamePlayer.PlayerRace.Female." + r.ToString("F"));
                }
            }
            return LanguageMgr.GetTranslation(client.Account.Language, "GamePlayer.PlayerRace." + r.ToString("F")); //Returns 'Unknown'
        }

        /// <summary>
        /// Gets/sets the player's race name
        /// </summary>
        public virtual string RaceName
        {
            get { return RACENAMES(Client, DBCharacter.Race, DBCharacter.Gender); }
        }

        /// <summary>
        /// Gets or sets this player's race id
        /// (delegate to PlayerCharacter)
        /// </summary>
        public override short Race
        {
            get { return (short)(DBCharacter != null ? DBCharacter.Race : 0); }
            set { if (DBCharacter != null) DBCharacter.Race = value; }
        }

        /// <summary>
        /// Players class
        /// </summary>
        protected ICharacterClass m_characterClass;

        /// <summary>
        /// Gets the player's character class
        /// </summary>
        public ICharacterClass CharacterClass
        {
            get { return m_characterClass; }
        }

        /// <summary>
        /// Set the character class to a specific one
        /// </summary>
        /// <param name="id">id of the character class</param>
        /// <returns>success</returns>
        public virtual bool SetCharacterClass(int id)
        {
            ICharacterClass cl = ScriptMgr.FindCharacterClass(id);

            if (cl == null)
            {
                if (log.IsErrorEnabled)
                    log.ErrorFormat("No CharacterClass with ID {0} found", id.ToString());
                return false;
            }

            m_characterClass = cl;

            if (m_characterClass != null)
            {
                m_characterClass.Init(this);
            }

            if (Util.IsEmpty(m_characterClass.FemaleName) == false && DBCharacter.Gender == 1)
                m_characterClass.SwitchToFemaleName();

            DBCharacter.Class = m_characterClass.ID;

            if (Group != null)
            {
                Group.UpdateMember(this, false, true);
            }
            return true;
        }

        /// <summary>
        /// Hold all player face custom attibutes
        /// </summary>
        protected byte[] m_customFaceAttributes = new byte[(int)eCharFacePart._Last + 1];

        /// <summary>
        /// Get the character face attribute you want
        /// </summary>
        /// <param name="part">face part</param>
        /// <returns>attribute</returns>
        public byte GetFaceAttribute(eCharFacePart part)
        {
            return m_customFaceAttributes[(int)part];
        }

        #endregion

        #region Spells/Skills/Abilities/Effects

        /// <summary>
        /// Holds the player specializable skills and style lines
        /// (KeyName -> Specialization)
        /// </summary>
        protected readonly ConcurrentDictionary<string, Specialization> m_specialization = new ConcurrentDictionary<string, Specialization>();

        /// <summary>
        /// Holds the players specs again but ordered
        /// </summary>
        protected ConcurrentBag<Specialization> m_specList = new ConcurrentBag<Specialization>();

        /// <summary>
        /// Holds the Spell lines the player can use
        /// </summary>
        protected readonly List<SpellLine> m_spellLines = new List<SpellLine>();

        /// <summary>
        /// Object to use when locking the SpellLines list
        /// </summary>
        public Object lockSpellLinesList = new Object();

        /// <summary>
        /// Holds all styles of the player
        /// </summary>
        protected readonly ConcurrentDictionary<int, Style> m_styles = new ConcurrentDictionary<int, Style>();

        /// <summary>
        /// Used to lock the style list
        /// </summary>
        public Object lockStyleList = new Object();

        /// <summary>
        /// Temporary Stats Boni
        /// </summary>
        protected readonly int[] m_statBonus = new int[8];

        /// <summary>
        /// Temporary Stats Boni in percent
        /// </summary>
        protected readonly int[] m_statBonusPercent = new int[8];


        /// <summary>
        /// Get the name associated with a skill.  Some skills, like vampiir abilities, add text to the skill name when displayed
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        public virtual string GetSkillName(Skill skill)
        {
            string name = skill.Name;

            if (skill.Name == Abilities.VampiirConstitution ||
                skill.Name == Abilities.VampiirDexterity ||
                skill.Name == Abilities.VampiirStrength)
            {
                name += " +" + ((CalculateSkillLevel(skill) - 5) * 3).ToString();
            }
            else if (skill.Name == Abilities.VampiirQuickness)
            {
                name += " +" + ((CalculateSkillLevel(skill) - 5) * 2).ToString();
            }

            return name;
        }

        /// <summary>
        /// Calculate the level of a skill.  Generally the skill.Level except for Vampiir skills
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        public override int CalculateSkillLevel(Skill skill)
        {
            if (skill.Name == Abilities.VampiirConstitution ||
                skill.Name == Abilities.VampiirDexterity ||
                skill.Name == Abilities.VampiirStrength ||
                skill.Name == Abilities.VampiirQuickness)
            {
                return Level;
            }

            return base.CalculateSkillLevel(skill);
        }

        /// <summary>
        /// Gets/Sets amount of full skill respecs
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int RespecAmountAllSkill
        {
            get { return DBCharacter != null ? DBCharacter.RespecAmountAllSkill : 0; }
            set { if (DBCharacter != null) DBCharacter.RespecAmountAllSkill = value; }
        }

        /// <summary>
        /// Gets/Sets amount of single-line respecs
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int RespecAmountSingleSkill
        {
            get { return DBCharacter != null ? DBCharacter.RespecAmountSingleSkill : 0; }
            set { if (DBCharacter != null) DBCharacter.RespecAmountSingleSkill = value; }
        }

        /// <summary>
        /// Gets/Sets amount of realm skill respecs
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int RespecAmountRealmSkill
        {
            get { return DBCharacter != null ? DBCharacter.RespecAmountRealmSkill : 0; }
            set { if (DBCharacter != null) DBCharacter.RespecAmountRealmSkill = value; }
        }

        /// <summary>
        /// Gets/Sets amount of DOL respecs
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int RespecAmountDOL
        {
            get { return DBCharacter != null ? DBCharacter.RespecAmountDOL : 0; }
            set { if (DBCharacter != null) DBCharacter.RespecAmountDOL = value; }
        }

        /// <summary>
        /// Gets/Sets level respec usage flag
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual bool IsLevelRespecUsed
        {
            get { return DBCharacter != null ? DBCharacter.IsLevelRespecUsed : true; }
            set { if (DBCharacter != null) DBCharacter.IsLevelRespecUsed = value; }
        }


        protected static readonly int[] m_numRespecsCanBuyOnLevel =
        {
            1,1,1,1,1, //1-5
			2,2,2,2,2,2,2, //6-12
			3,3,3,3, //13-16
			4,4,4,4,4,4, //17-22
			5,5,5,5,5, //23-27
			6,6,6,6,6,6, //28-33
			7,7,7,7,7, //34-38
			8,8,8,8,8,8, //39-44
			9,9,9,9,9, //45-49
			10 //50
		};


        /// <summary>
        /// Can this player buy a respec?
        /// </summary>
        public virtual bool CanBuyRespec
        {
            get
            {
                return (RespecBought < m_numRespecsCanBuyOnLevel[Level - 1]);
            }
        }

        /// <summary>
        /// Gets/Sets amount of bought respecs
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int RespecBought
        {
            get { return DBCharacter != null ? DBCharacter.RespecBought : 0; }
            set { if (DBCharacter != null) DBCharacter.RespecBought = value; }
        }


        protected static readonly int[] m_respecCost =
        {
            1,2,3, //13
			2,5,9, //14
			3,9,17, //15
			6,16,30, //16
			10,26,48,75, //17
			16,40,72,112, //18
			22,56,102,159, //19
			31,78,140,218, //20
			41,103,187,291, //21
			54,135,243,378, //22
			68,171,308,480,652, //23
			85,214,385,600,814, //24
			105,263,474,738,1001, //25
			128,320,576,896,1216, //26
			153,383,690,1074,1458, //27
			182,455,820,1275,1731,2278, //28
			214,535,964,1500,2036,2679, //29
			250,625,1125,1750,2375,3125, //30
			289,723,1302,2025,2749,3617, //31
			332,831,1497,2329,3161,4159, //32
			380,950,1710,2661,3612,4752, //33
			432,1080,1944,3024,4104,5400,6696, //34
			488,1220,2197,3417,4638,6103,7568, //35
			549,1373,2471,3844,5217,6865,8513, //36
			615,1537,2767,4305,5843,7688,9533, //37
			686,1715,3087,4802,6517,8575,10633, //38
			762,1905,3429,5335,7240,9526,11813,14099, //39
			843,2109,3796,5906,8015,10546,13078,15609, //40
			930,2327,4189,6516,8844,11637,14430,17222, //41
			1024,2560,4608,7168,9728,1280,15872,18944, //42
			1123,2807,5053,7861,10668,14037,17406,20776, //43
			1228,3070,5527,8597,11668,15353,19037,22722, //44
			1339,3349,6029,9378,12725,16748,20767,24787,28806, //45
			1458,3645,6561,10206,13851,18225,22599,26973,31347, //46
			1582,3957,7123,11080,15037,19786,24535,29283,34032, //47
			1714,4286,7716,12003,16290,21434,26578,31722,36867, //48
			1853,4634,8341,12976,17610,23171,28732,34293,39854, //49
			2000,5000,9000,14000,19000,25000,31000,37000,43000,50000 //50
		};


        /// <summary>
        /// How much does this player have to pay for a respec?
        /// </summary>
        public virtual long RespecCost
        {
            get
            {
                if (Level <= 12) //1-12
                    return m_respecCost[0];

                if (CanBuyRespec)
                {
                    int t = 0;
                    for (int i = 13; i < Level; i++)
                    {
                        t += m_numRespecsCanBuyOnLevel[i - 1];
                    }

                    return m_respecCost[t + RespecBought];
                }

                return -1;
            }
        }


        /// <summary>
        /// give player a new Specialization
        /// </summary>
        /// <param name="skill"></param>
       
        public virtual void AddSpecialization(Specialization skill)
        {
            if (skill == null)
                return;

            if (m_specialization.TryAdd(skill.KeyName, skill))
            {
                //DOLConsole.WriteLine("Spec " + skill.Name + " added");
                m_specList.Add(skill);
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.AddSpecialisation.YouLearn", skill.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
            else
            {
                // Existierende Spezialisierung gefunden, Level aktualisieren
                if (m_specialization.TryGetValue(skill.KeyName, out Specialization oldskill) && oldskill.Level < skill.Level)
                {
                    oldskill.Level = skill.Level; // Stellen Sie sicher, dass dies thread-sicher ist
                }
            }
        }


        /// <summary>
        /// Removes the existing specialization from the player
        /// </summary>
        /// <param name="specKeyName">The spec keyname to remove</param>
        /// <returns>true if removed</returns>
        public virtual bool RemoveSpecialization(string specKeyName)
        {
           

            Specialization playerSpec = null;
           
                    if (!m_specialization.TryGetValue(specKeyName, out playerSpec))
                        return false;
                    if (m_specList.Contains(playerSpec))
                        m_specList.TryTake(out playerSpec);

                    m_specialization.TryRemove(specKeyName, out playerSpec);
               
           
            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RemoveSpecialization.YouLose", playerSpec), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            return true;
        }

        /// <summary>
        /// Removes the existing spellline from the player, the line instance should be called with GamePlayer.GetSpellLine ONLY and NEVER SkillBase.GetSpellLine!!!!!
        /// </summary>
        /// <param name="line">The spell line to remove</param>
        /// <returns>true if removed</returns>
        protected virtual bool RemoveSpellLine(SpellLine line)
        {
            if (!m_spellLines.Contains(line))
            {
                return false;
            }

            lock (((ICollection)m_spellLines).SyncRoot)
            {
                m_spellLines.Remove(line);
            }

            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RemoveSpellLine.YouLose", line.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            return true;
        }

        /// <summary>
        /// Removes the existing specialization from the player
        /// </summary>
        /// <param name="lineKeyName">The spell line keyname to remove</param>
        /// <returns>true if removed</returns>
        public virtual bool RemoveSpellLine(string lineKeyName)
        {
            SpellLine line = GetSpellLine(lineKeyName);
            if (line == null)
                return false;

            return RemoveSpellLine(line);
        }

        /// <summary>
        /// Reset this player to level 1, respec all skills, remove all spec points, and reset stats
        /// </summary>
        public virtual void Reset()
        {
            //Guard Remove
            if (HasAbility(Abilities.Guard))
            {
                RemoveAbility(Abilities.Guard);
            }


            //SafeFall
            if (HasAbility(Abilities.SafeFall))
            {
                RemoveAbility(Abilities.SafeFall);
            }

            //Shadowstrike remove
            if (HasAbility(Abilities.ShadowStrike))
            {
                RemoveAbility(Abilities.ShadowStrike);
            }

            //Detect Hidden remove
            if (HasAbility(Abilities.DetectHidden))
            {
                RemoveAbility(Abilities.DetectHidden);
            }

            //Distraction remove
            if (HasAbility(Abilities.Distraction))
            {
                RemoveAbility(Abilities.Distraction);
            }

            //Danger Sense remove
            if (HasAbility(Abilities.DangerSense))
            {
                RemoveAbility(Abilities.DangerSense);
            }

            //Snapshot remove
            if (HasAbility(Abilities.Snapshot))
            {
                RemoveAbility(Abilities.Snapshot);
            }

            //Engage remove
            if (HasAbility(Abilities.Engage))
            {
                RemoveAbility(Abilities.Engage);
            }

            //Tactics remove
            if (HasAbility(Abilities.Tactics))
            {
                RemoveAbility(Abilities.Tactics);
            }



            byte originalLevel = Level;
            Level = 1;
            Experience = 0;
            RespecAllLines();
            SkillSpecialtyPoints = 0;

            if (Level < originalLevel && originalLevel > 5)
            {
                for (int i = 6; i <= originalLevel; i++)
                {
                    if (CharacterClass.PrimaryStat != eStat.UNDEFINED)
                    {
                        ChangeBaseStat(CharacterClass.PrimaryStat, -1);
                    }
                    if (CharacterClass.SecondaryStat != eStat.UNDEFINED && ((i - 6) % 2 == 0))
                    {
                        ChangeBaseStat(CharacterClass.SecondaryStat, -1);
                    }
                    if (CharacterClass.TertiaryStat != eStat.UNDEFINED && ((i - 6) % 3 == 0))
                    {
                        ChangeBaseStat(CharacterClass.TertiaryStat, -1);
                    }
                }
            }
            CharacterClass.OnLevelUp(this, originalLevel);
        }

        public virtual int RespecAll()
        {
            //Guard Remove
            if (HasAbility(Abilities.Guard))
            {
                RemoveAbility(Abilities.Guard);
            }

            //SafeFall
            if (HasAbility(Abilities.SafeFall))
            {
                RemoveAbility(Abilities.SafeFall);
            }

            //Shadowstrike remove
            if (HasAbility(Abilities.ShadowStrike))
            {
                RemoveAbility(Abilities.ShadowStrike);
            }

            //Climb Walls remove
            if (HasAbility(Abilities.Climbing))
            {
                RemoveAbility(Abilities.Climbing);
            }

            //Detect Hidden remove
            if (HasAbility(Abilities.DetectHidden))
            {
                RemoveAbility(Abilities.DetectHidden);
            }

            //Distraction remove
            if (HasAbility(Abilities.Distraction))
            {
                RemoveAbility(Abilities.Distraction);
            }

            //Danger Sense remove
            if (HasAbility(Abilities.DangerSense))
            {
                RemoveAbility(Abilities.DangerSense);
            }

            //Snapshot remove
            if (HasAbility(Abilities.Snapshot))
            {
                RemoveAbility(Abilities.Snapshot);
            }

            //Engage remove
            if (HasAbility(Abilities.Engage))
            {
                RemoveAbility(Abilities.Engage);
            }

            //Tactics remove
            if (HasAbility(Abilities.Tactics))
            {
                RemoveAbility(Abilities.Tactics);
            }

            int specPoints = RespecAllLines(); // Wipe skills and styles.

            RespecAmountAllSkill--; // Decriment players respecs available.




            if (Level == 5)
                IsLevelRespecUsed = true;
            return specPoints;
        }

        public virtual int RespecDOL()
        {
            //Guard remove
            if (HasAbility(Abilities.Guard))
            {
                RemoveAbility(Abilities.Guard);
            }

            //SafeFall
            if (HasAbility(Abilities.SafeFall))
            {
                RemoveAbility(Abilities.SafeFall);
            }

            //Shadowstrike remove
            if (HasAbility(Abilities.ShadowStrike))
            {
                RemoveAbility(Abilities.ShadowStrike);
            }

            //Climb Walls remove
            if (HasAbility(Abilities.Climbing))
            {
                RemoveAbility(Abilities.Climbing);
            }

            //Detect Hidden remove
            if (HasAbility(Abilities.DetectHidden))
            {
                RemoveAbility(Abilities.DetectHidden);
            }

            //Distraction remove
            if (HasAbility(Abilities.Distraction))
            {
                RemoveAbility(Abilities.Distraction);
            }

            //Danger Sense remove
            if (HasAbility(Abilities.DangerSense))
            {
                RemoveAbility(Abilities.DangerSense);
            }

            //Snapshot remove
            if (HasAbility(Abilities.Snapshot))
            {
                RemoveAbility(Abilities.Snapshot);
            }

            //Engage remove
            if (HasAbility(Abilities.Engage))
            {
                RemoveAbility(Abilities.Engage);
            }

            //Tactics remove
            if (HasAbility(Abilities.Tactics))
            {
                RemoveAbility(Abilities.Tactics);
            }

            int specPoints = RespecAllLines(); // Wipe skills and styles.

            RespecAmountDOL--; // Decriment players respecs available.

            return specPoints;
        }

        public virtual int RespecSingle(Specialization specLine)
        {

            //Guard remove
            if (HasAbility(Abilities.Guard) && specLine.Name == "Shield")
            {
                RemoveAbility(Abilities.Guard);
            }

            //SafeFall
            if (HasAbility(Abilities.SafeFall) && specLine.Name == "Stealth")
            {
                RemoveAbility(Abilities.SafeFall);
            }
            //Shadowstrike remove
            if (HasAbility(Abilities.ShadowStrike) && specLine.Name == "Stealth")
            {
                RemoveAbility(Abilities.ShadowStrike);
            }

            //Climb Walls remove
            if (HasAbility(Abilities.Climbing) && specLine.Name == "Stealth")
            {
                RemoveAbility(Abilities.Climbing);
            }

            //Detect Hidden remove
            if (HasAbility(Abilities.DetectHidden) && specLine.Name == "Stealth")
            {
                RemoveAbility(Abilities.DetectHidden);
            }

            //Distraction remove
            if (HasAbility(Abilities.Distraction) && specLine.Name == "Stealth")
            {
                RemoveAbility(Abilities.Distraction);
            }

            //Danger Sense remove
            if (HasAbility(Abilities.DangerSense) && specLine.Name == "Stealth")
            {
                RemoveAbility(Abilities.DangerSense);
            }

            if (HasAbility(Abilities.Snapshot) && specLine.Name == "Crossbows")
            {
                RemoveAbility(Abilities.Snapshot);
            }

            //Guard remove
            if (HasAbility(Abilities.Engage) && specLine.Name == "Shield")
            {
                RemoveAbility(Abilities.Engage);
            }

            //Tactics remove
            if (HasAbility(Abilities.Tactics) && specLine.Name == "Shield")
            {
                RemoveAbility(Abilities.Tactics);
            }

            int specPoints = RespecSingleLine(specLine); // Wipe skills and styles.
            RespecAmountSingleSkill--; // Decriment players respecs available.
            if (Level == 20 || Level == 40)
            {
                IsLevelRespecUsed = true;
            }
            return specPoints;
        }

        public virtual int RespecRealm()
        {
            int respecPoints = 0;
            foreach (Ability ab in GetAllAbilities())
            {
                if (ab is RealmAbility && ab is RR5RealmAbility == false)
                {
                    for (int i = 0; i < ab.Level; i++)
                    {
                        respecPoints += ((RealmAbility)ab).CostForUpgrade(i);
                    }
                    RemoveAbility(ab.KeyName);
                }
            }
            RespecAmountRealmSkill--;
            return respecPoints;
        }

        protected virtual int RespecAllLines()
        {
            int specPoints = 0;
            
                foreach (Specialization cspec in m_specList)
                {
                    if (cspec.Level < 2)
                        continue;
                    specPoints += RespecSingleLine(cspec);
                }
           
            return specPoints;
        }

        /// <summary>
        /// Respec single line
        /// </summary>
        /// <param name="specLine">spec line being respec'd</param>
        /// <returns>Amount of points spent in that line</returns>
        protected virtual int RespecSingleLine(Specialization specLine)
        {
            int specPoints = (specLine.Level * (specLine.Level + 1) - 2) / 2;
            // Graveen - autotrain 1.87
            specPoints -= GetAutoTrainPoints(specLine, 0);

            //setting directly the autotrain points in the spec
            if (GetAutoTrainPoints(specLine, 4) == 1 && Level >= 8)
            {
                specLine.Level = (int)Math.Floor((double)Level / 4);
            }
            else specLine.Level = 1;

            return specPoints;
        }

        /// <summary>
        /// Send this players trainer window
        /// </summary>
        public virtual void SendTrainerWindow()
        {
            Out.SendTrainerWindow();
        }

        /// <summary>
        /// returns a list with all specializations
        /// in the order they were added
        /// be careful when iterating this list, it has to be
        /// synced via SyncRoot before any foreach loop
        /// because its a reference to the player internal list of specs
        /// that can change at any time
        /// </summary>
        /// <returns>list of Spec's</returns>
        public virtual ConcurrentBag<Specialization> GetSpecList()
        {
            /*
            var bag = new ConcurrentBag<Specialization>();
            foreach (var spec in m_specList.OrderBy(spec => spec.Level))
            {
                bag.Add(spec);
            }
            return bag;
            */
            return m_specList;
        }

        /// <summary>
        /// returns a list with all non trainable skills without styles
        /// in the order they were added
        /// be careful when iterating this list, it has to be
        /// synced via SyncRoot before any foreach loop
        /// because its a reference to the player internal list of skills
        /// that can change at any time
        /// </summary>
        /// <returns>list of Skill's</returns>
        public virtual IList GetNonTrainableSkillList()
        {
            return m_skillList;
        }

        /// <summary>
        /// Retrieves a specific specialization by key name
        /// </summary>
        /// <param name="keyName">the key name</param>
        /// <returns>the found specialization or null</returns>
        public virtual Specialization GetSpecialization(string keyName)
        {

            if (!m_specialization.TryGetValue(keyName, out Specialization spec))
            {
                if (spec != null)
                {
                    // try case insensitive search
                    foreach (Specialization sp in m_specialization.Values)
                    {
                        if (sp.KeyName.ToLower() == keyName.ToLower())
                        {
                            spec = sp;
                            break;
                        }
                    }
                }
                               
            }

            return spec;
        }

        /// <summary>
        /// Retrives a specific specialization by name
        /// </summary>
        /// <param name="name">the name of the specialization line</param>
        /// <param name="caseSensitive">false for case-insensitive compare</param>
        /// <returns>found specialization or null</returns>
        public virtual Specialization GetSpecializationByName(string name, bool caseSensitive)
        {
            
                if (caseSensitive)
                {
                    foreach (Specialization spec in m_specList)
                        if (spec.Name == name)
                            return spec;
                }
                else
                {
                    name = name.ToLower();
                    foreach (Specialization spec in m_specList)
                        if (spec.Name.ToLower() == name)
                            return spec;
                }
          
            return null;
        }

        /// <summary>
        /// The best armor level this player can use.
        /// </summary>
        public virtual int BestArmorLevel
        {
            get
            {
                int bestLevel = -1;
                bestLevel = Math.Max(bestLevel, GetAbilityLevel(Abilities.AlbArmor));
                bestLevel = Math.Max(bestLevel, GetAbilityLevel(Abilities.HibArmor));
                bestLevel = Math.Max(bestLevel, GetAbilityLevel(Abilities.MidArmor));
                return bestLevel;
            }
        }

        #region Abilities

        /// <summary>
        /// Adds a new Ability to the player
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="sendUpdates"></param>
        public override void AddAbility(Ability ability, bool sendUpdates)
        {
            if (ability == null)
                return;

            if (IsCharcterClass(eCharacterClass.Nightshade) == false
                && IsCharcterClass(eCharacterClass.Infiltrator) == false
                && IsCharcterClass(eCharacterClass.Shadowblade) == false
                && ability.KeyName == Abilities.DetectHidden)
                return;

            if (IsCharcterClass(eCharacterClass.Nightshade) == false
                && IsCharcterClass(eCharacterClass.Infiltrator) == false
                && IsCharcterClass(eCharacterClass.Minstrel) == false
                && IsCharcterClass(eCharacterClass.Shadowblade) == false
                && IsCharcterClass(eCharacterClass.Vampiir) == false
                && IsCharcterClass(eCharacterClass.Hero) == false
                && IsCharcterClass(eCharacterClass.Armsman) == false
                && IsCharcterClass(eCharacterClass.Warrior) == false
                && ability.KeyName == Abilities.Climbing)
                return;

            if (IsCharcterClass(eCharacterClass.Infiltrator)
                && ability.KeyName == Abilities.Snapshot)
                return;

            base.AddAbility(ability, sendUpdates);
        }

        #endregion Abilities

        public virtual void RemoveAllSkills()
        {
            lock (((ICollection)m_skillList).SyncRoot)
            {
                foreach (NamedSkill skill in m_skillList)
                {
                    if (m_abilities.ContainsKey(skill.KeyName))
                        m_abilities.Remove(skill.KeyName);
                }
                m_skillList.Clear();


            }
        }

        public virtual void RemoveAllSpecs()
        {
            
                foreach (Specialization spec in m_specList)
                {
                    if (m_specialization.ContainsKey(spec.KeyName))
                        m_specialization.TryRemove(spec.KeyName, out Specialization _);

                }
                m_specList = new ConcurrentBag<Specialization>();

        }



        public virtual void RemoveAllSpellLines()
        {
            lock (lockSpellLinesList)
            {
                m_spellLines.Clear();
            }
        }

        public virtual void RemoveAllStyles()
        {

            m_styles.Clear();

        }

        /// <summary>
        /// Asks for existance of specific specialization
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public virtual bool HasSpecialization(string keyName)
        {
            return m_specialization.ContainsKey(keyName);
        }

        /// <summary>
        /// Checks whether Living has ability to use lefthanded weapons
        /// </summary>
        public override bool CanUseLefthandedWeapon
        {
            get
            {
                return CharacterClass.CanUseLefthandedWeapon(this);
            }
        }




        /// <summary>
        /// Calculates how many times left hand swings
        /// </summary>
        public override int CalculateLeftHandSwingCount()
        {


            if (CanUseLefthandedWeapon == false)
                return 0;



            if (GetBaseSpecLevel(Specs.Left_Axe) > 0)
                return 1; // always use left axe


            int specLevel = Math.Max(GetModifiedSpecLevel(Specs.Celtic_Dual), GetModifiedSpecLevel(Specs.Dual_Wield));
            specLevel = Math.Max(specLevel, GetModifiedSpecLevel(Specs.Fist_Wraps));
            if (specLevel > 0)
            {
                return Util.Chance(25 + (specLevel - 1) * 68 / 100) ? 1 : 0;
            }
            int HandToHandSpec = GetModifiedSpecLevel(Specs.HandToHand);

            int CurrentHandToHandSpec = GetModifiedSpecLevel(Specs.HandToHand);

            //If Style ?
            GamePlayer owner = Client.Player;
            Style combatStyle;
            InventoryItem attackWeapon = owner.AttackWeapon;

            combatStyle = owner.GetStyleToUse();

            //Savage HtH chance


            InventoryItem leftWeapon = Inventory?.GetItem(eInventorySlot.LeftHandWeapon);
            if (combatStyle != null && HandToHandSpec > 0 && ActiveWeaponSlot == eActiveWeaponSlot.Standard
                && attackWeapon != null && attackWeapon.Object_Type == (int)eObjectType.HandToHand &&
                leftWeapon != null && leftWeapon.Object_Type == (int)eObjectType.HandToHand)
            {
                HandToHandSpec--;
                if (HandToHandSpec <= 0)
                    HandToHandSpec = 1;
                int randomChance = Util.Random(Level + 1);
                int hitChance = HandToHandSpec >> 1;
                if (hitChance == 0)
                    hitChance = 1;

                // log.ErrorFormat("randomChance {0}    hitChance {1}", randomChance, hitChance);
                if (randomChance <= hitChance)
                {
                    //  log.ErrorFormat("randomChance1 {0}    hitChance1 {1}", randomChance, hitChance);
                    return 1; // 1 hit = spec/2
                }

                hitChance += HandToHandSpec >> 2;
                if (randomChance <= hitChance && CurrentHandToHandSpec >= 25)
                    return 2; // 2 hits = spec/4
                hitChance += HandToHandSpec >> 4;
                if (randomChance <= hitChance && CurrentHandToHandSpec >= 42)
                    return 3; // 3 hits = spec/16


            }

            return 0;
        }

        /// <summary>
        /// Returns a multiplier to adjust left hand damage
        /// </summary>
        /// <returns></returns>
        public override double CalculateLeftHandEffectiveness(InventoryItem mainWeapon, InventoryItem leftWeapon)
        {
            double effectiveness = 1.0;


            //If Style ?
            Style combatStyle;


            int LASpec = GetModifiedSpecLevel(Specs.Left_Axe);
            if (LASpec > 50)
                LASpec = 50;

            combatStyle = GetStyleToUse();

            if (CanUseLefthandedWeapon && leftWeapon != null && (leftWeapon.Object_Type == (int)eObjectType.LeftAxe || leftWeapon.Object_Type == (int)eObjectType.Axe) && mainWeapon != null &&
                (mainWeapon.Item_Type == Slot.RIGHTHAND || mainWeapon.Item_Type == Slot.LEFTHAND))

            {
                if (LASpec > 1 && combatStyle != null)
                {
                    //Left Axe(note that this applies to both mainhand and offhand weapons):
                    //modified damage = base damage * (.625 + .0034 * LA spec)
                    // log.ErrorFormat("LEFTHAND:  la spec = {0}, gesammt wert von formel = {1} ", LASpec, 0.625 + 0.0034 * LASpec);
                    effectiveness = 0.625 + 0.0034 + (ServerProperties.Properties.Dual_LeftAxe_Damage * LASpec);
                }

                return effectiveness;
            }
            else if (CanUseLefthandedWeapon && leftWeapon != null && leftWeapon.Object_Type != (int)eObjectType.LeftAxe && (leftWeapon.Object_Type == (int)eObjectType.Axe
                        || leftWeapon.Object_Type == (int)eObjectType.Piercing
                        || leftWeapon.Object_Type == (int)eObjectType.SlashingWeapon
                        || leftWeapon.Object_Type == (int)eObjectType.CrushingWeapon || leftWeapon.Object_Type == (int)eObjectType.Blades
                        || leftWeapon.Object_Type == (int)eObjectType.ThrustWeapon
                        || leftWeapon.Object_Type == (int)eObjectType.Blunt) && mainWeapon != null &&
                        (mainWeapon.Item_Type == Slot.RIGHTHAND || mainWeapon.Item_Type == Slot.LEFTHAND))
            {

                int celtcSpec = GetModifiedSpecLevel(Specs.Celtic_Dual);
                int WieldSpec = GetModifiedSpecLevel(Specs.Dual_Wield);
                if (celtcSpec > 50)
                    celtcSpec = 50;
                if (WieldSpec > 50)
                    WieldSpec = 50;

                //Formal = chance to swing offhand weapon = 25 % +.68 * CD / DW spec


                if (TargetObject is GameNPC && celtcSpec > 1 && combatStyle != null) //  && combatStyle.SkillType.Equals(Specs.Dual_Wield)//&& combatStyle.WeaponTypeRequirement.Equals(Specs.Dual_Wield))
                {
                    effectiveness = 0.625 + 0.0034 + (ServerProperties.Properties.Dual_Lefthand_Damage * celtcSpec);
                }
                else if (TargetObject is GamePlayer && celtcSpec > 1 && combatStyle != null) //  && combatStyle.SkillType.Equals(Specs.Dual_Wield)//&& combatStyle.WeaponTypeRequirement.Equals(Specs.Dual_Wield))
                {
                    effectiveness = 0.625 + 0.0034 + (ServerProperties.Properties.PvP_Dual_Lefthand_Damage * celtcSpec);
                }

                else if (TargetObject is GameNPC && WieldSpec > 1 && combatStyle != null)
                {
                    effectiveness = 0.625 + 0.0034 + (ServerProperties.Properties.Dual_Lefthand_Damage_ALB * WieldSpec);

                }
                else if (TargetObject is GamePlayer && WieldSpec > 1 && combatStyle != null)
                {
                    effectiveness = 0.625 + 0.0034 + (ServerProperties.Properties.PvP_Dual_Lefthand_Damage_ALB * WieldSpec);

                }
            }
            return effectiveness;
        }


        /// <summary>
        /// Returns a multiplier to adjust right hand damage
        /// </summary>
        /// <param name="leftWeapon"></param>
        /// <returns></returns>
        public override double CalculateMainHandEffectiveness(InventoryItem mainWeapon, InventoryItem leftWeapon)
        {
            double effectiveness = 1.0;

            if (CanUseLefthandedWeapon && leftWeapon != null && leftWeapon.Object_Type == (int)eObjectType.LeftAxe && mainWeapon != null &&
                (mainWeapon.Item_Type == Slot.RIGHTHAND || mainWeapon.Item_Type == Slot.LEFTHAND))
            {
                int LASpec = GetModifiedSpecLevel(Specs.Left_Axe);
                if (LASpec > 1)
                    if (LASpec > 50)
                        LASpec = 50;

                {
                    //log.ErrorFormat("la spec = {0}, gesammt wert von formel = {1} ", LASpec, 0.625 + 0.0034 * LASpec);
                    //Formel = modified damage = base damage * (.625 + .0034 * LA spec)
                    effectiveness = 0.625 + 0.0034 * LASpec;
                }
            }

            return effectiveness;
        }


        /// <summary>
        /// returns the level of a specialization
        /// if 0 is returned, the spec is non existent on player
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public override int GetBaseSpecLevel(string keyName)
        {


            if (m_specialization.TryGetValue(keyName, out Specialization spec))
                return spec.Level;
            return 0;

        }

        /// <summary>
        /// returns the level of a specialization + bonuses from RR and Items
        /// if 0 is returned, the spec is non existent on the player
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public override int GetModifiedSpecLevel(string keyName)
        {
            if (keyName == GlobalSpellsLines.Champion_Spells)
                return 50;


            if (!m_specialization.TryGetValue(keyName, out Specialization spec))
            {
                if (keyName == GlobalSpellsLines.Combat_Styles_Effect)
                {
                    if (IsCharcterClass(eCharacterClass.Reaver) || IsCharcterClass(eCharacterClass.Heretic))
                        return GetModifiedSpecLevel(Specs.Flexible);
                    if (IsCharcterClass(eCharacterClass.Valewalker))
                        return GetModifiedSpecLevel(Specs.Scythe);
                    if (IsCharcterClass(eCharacterClass.Savage))
                        return GetModifiedSpecLevel(Specs.Savagery);
                }

                return 0;
            }
            int res = spec.Level;
            // TODO: should be all in calculator later, right now
            // needs specKey -> eProperty conversion to find calculator and then
            // needs eProperty -> specKey conversion to find how much points player has spent
            eProperty skillProp = SkillBase.SpecToSkill(keyName);
            if (skillProp != eProperty.Undefined)
                res += GetModified(skillProp);
            return res;
        }

        /// <summary>
        /// Adds a spell line to the player
        /// </summary>
        /// <param name="line"></param>
        public virtual void AddSpellLine(SpellLine line)
        {
            if (line == null)
                return;

            SpellLine oldline = GetSpellLine(line.KeyName);
            if (oldline == null)
            {
                lock (((ICollection)m_spellLines).SyncRoot)
                {
                    m_spellLines.Add(line);
                }
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.AddSpellLine.YouLearn", line.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
            else
            {
                if (oldline.Level < line.Level)
                {
                    oldline.Level = line.Level;
                }
            }
        }

        /// <summary>
        /// return a list of spell lines in the order they were added
        /// iterate only with locking SyncRoot on the list!
        /// </summary>
        /// <returns></returns>
        public virtual List<SpellLine> GetSpellLines()
        {
            return m_spellLines;
        }

        /// <summary>
        /// find a spell line on player and return them
        /// </summary>
        /// <param name="keyname"></param>
        /// <returns></returns>
        public virtual SpellLine GetSpellLine(string keyname)
        {
            lock (((ICollection)m_spellLines).SyncRoot)
            {
                foreach (SpellLine line in m_spellLines)
                {
                    if (line.KeyName == keyname)
                        return line;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets a list of available styles
        /// This creates a copy
        /// </summary>
        public virtual List<Style> GetStyleList()
        {
            // Gibt die Liste der Stile nach Style ID sortiert zurück
            return m_styles.Values.OrderBy(style => style.ID).ToList();
        }

        /// <summary>
        /// Get a list of Champion styles for this player
        /// </summary>
        /// <returns></returns>
        public virtual List<Style> GetChampionStyleList()
        {
            List<Spell> champSpells = SkillBase.GetSpellList(ChampionSpellLineName);
            List<Style> champStyles = new List<Style>();

            foreach (Spell champSpell in champSpells)
            {
                if (champSpell.SpellType == "StyleHandler" || champSpell.SpellType == "ChampionStyleHandler")
                {
                    Style champStyle = SkillBase.GetStyleByID((int)champSpell.Value, CharacterClass.ID);
                    if (champStyle == null)
                    {
                        //log.Error("Kein Champ style gefunden!!");
                        champStyle = SkillBase.GetStyleByID((int)champSpell.Value, 0); //0 for Cl-Style Class ID
                    }

                    if (champStyle != null)
                    {
                        //log.ErrorFormat("Champ style gefunden!!", champStyle.Name);
                        champStyle.Level = champSpell.Level;
                        champStyles.Add(champStyle);
                    }
                }
            }

            return champStyles;
        }

        /// <summary>
        /// Return the count of all spells the player can use
        /// This is for hybrid classes only
        /// </summary>
        /// <returns></returns>
        public virtual int GetSpellCount()
        {
            lock (m_LockObject)
            {
                return GetUsableSpells(m_spellLines, true).Count;
            }
        }


        /// <summary>
        /// Should we allow multiple versions of each spell type in this spell line
        /// Used for hybrid classes
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        protected virtual bool AllowMultipleSpellVersions(SpellLine line)
        {
            bool allow = false;

            switch (line.Spec)
            {
                case Specs.Augmentation:
                    allow = true;
                    break;

                case Specs.Enhancement:
                    if ((line.IsBaseLine || IsCharcterClass(eCharacterClass.Cleric) && IsCharcterClass(eCharacterClass.Heretic) == false))
                        allow = true;

                    break;

                case Specs.Nurture:
                    if (line.IsBaseLine || IsCharcterClass(eCharacterClass.Druid))
                        allow = true;

                    break;

                case Specs.Soulrending:
                    allow = true;
                    break;
            }

            return allow;
        }

        /// <summary>
        /// Is this a Champion or ML SpellLine
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public virtual bool IsAdvancedSpellLine(SpellLine line)
        {
            // ML lines
            switch (line.KeyName)
            {
                case Specs.Convoker:
                case Specs.Banelord:
                case Specs.Stormlord:
                case Specs.Perfecter:
                case Specs.Sojourner:
                case Specs.Spymaster:
                case Specs.Battlemaster:
                case Specs.Warlord:
                case GlobalSpellsLines.Character_Abilities:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Skill update after respec
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        public virtual bool RespecUpdate(bool update)
        {
            if (!update && CharacterClass.ClassType != eClassType.ListCaster)
            {
                foreach (KeyValuePair<Spell, SpellLine> spell in m_usableSpells.Values.ToList())
                {
                    //if trainer spec < spec level
                    if (spell.Key.Level > spell.Value.Level)
                    {
                        // this is probably due to a respec, wipe list and rebuild
                        // this will cause the quick bars to jumble, no idea how to avoid it
                        m_usableSpells = new ReaderWriterDictionary<string, KeyValuePair<Spell, SpellLine>>();
                        update = true;
                        break;
                    }
                }

            }
            return true;
        }


        /// <summary>
        /// A list of all usable spells for this player.  This list is maintained as long as the player is active
        /// with new spells always added to the end of the list.  This is used for all hybrid classes
        /// Structure of this list: uniquekey-Spell-SpellLine
        /// </summary>
        protected ReaderWriterDictionary<string, KeyValuePair<Spell, SpellLine>> m_usableSpells = null;


        /// <summary>
        /// Return a list of spells usable for all spell lines provided.  This is used for hybrid classes.
        /// This list should return the highest level spell of each spell type (or spell group, if provided)
        /// </summary>
        /// <param name="spelllines">list of spellLines</param>
        /// <returns>list of Spells</returns>
        public virtual ReaderWriterDictionary<string, KeyValuePair<Spell, SpellLine>> GetUsableSpells(List<SpellLine> spellLines, bool update = false)
        {

            if (m_usableSpells == null)
            {
                m_usableSpells = new ReaderWriterDictionary<string, KeyValuePair<Spell, SpellLine>>();
            }

            ReaderWriterDictionary<string, KeyValuePair<Spell, SpellLine>> results = new ReaderWriterDictionary<string, KeyValuePair<Spell, SpellLine>>();


            if (!update && RespecUpdate(true))
            {
                if (m_usableSpells.Count > 0)
                    results = new ReaderWriterDictionary<string, KeyValuePair<Spell, SpellLine>>(m_usableSpells);

                // return results if cache is valid.
                if (results.Count > 0)
                    return results;

            }


            // m_usableSpells.FreezeWhile(innerList =>
            // {

            if (update && RespecUpdate(true))
            {
                ReaderWriterDictionary<string, KeyValuePair<Spell, SpellLine>> usableBaseSpells = new ReaderWriterDictionary<string, KeyValuePair<Spell, SpellLine>>(); // highest level of spell type or group
                ReaderWriterDictionary<string, KeyValuePair<Spell, SpellLine>> usableSpecSpells = new ReaderWriterDictionary<string, KeyValuePair<Spell, SpellLine>>(); // second highest level of spell type or group
                                                                                                                                                                        // ConcurrentDictionary<string, KeyValuePair<Spell, SpellLine>> spellList = new ConcurrentDictionary<string, KeyValuePair<Spell, SpellLine>>();
                List<KeyValuePair<Spell, SpellLine>> spellList = new List<KeyValuePair<Spell, SpellLine>>();

                foreach (SpellLine line in spellLines.ToList())
                {

                    m_syncLockUpdates.EnterReadLock();
                    try
                    {
                        if (IsAdvancedSpellLine(line) == false) // don't add advanced spell lines to this list
                        {

                            foreach (Spell spell in SkillBase.GetSpellList(line.KeyName))
                            {

                                if (spell.Level <= line.Level && spell.SpellType != "StyleHandler" && spell.SpellType != "ChampionStyleHandler")
                                {
                                    try
                                    {

                                        spellList.Add(new KeyValuePair<Spell, SpellLine>(spell, line));
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                    }

                    finally
                    {
                        m_syncLockUpdates.ExitReadLock();
                    }
                }


                int counter = 0;

                foreach (KeyValuePair<Spell, SpellLine> spell in spellList.ToList())
                {
                    string key;

                    if (spell.Key.Group == 0)
                    {
                        // Tolakram:
                        // This is an attempt to make sure all the correct spells are provided.
                        // If there is a special rule for a spelltype then spells of that spelltype should be provided with
                        // a unique spellgroup number to ensure they get included in the list.

                        key = spell.Value.KeyName + "+" + spell.Key.SpellType + "+" + spell.Key.Target;

                        if (spell.Key.CastTime == 0)
                        {
                            key += "+INSTANT";
                        }

                        if (spell.Key.Radius > 0)
                        {
                            key += "+AOE";
                        }

                        if (spell.Key.SubSpellID > 0)
                        {
                            key += "+SUB";
                        }

                        if (spell.Value.Spec == GlobalSpellsLines.Champion_Spells)
                        {
                            key += counter++;
                        }
                    }
                    else
                    {
                        key = spell.Key.Group.ToString();
                    }

                    string key1 = "1:" + key;
                    string key2 = "2:" + key;

                    m_syncLockUpdates.EnterReadLock();
                    try
                    {
                        if (usableBaseSpells.ContainsKey(key1))
                        {
                            if (spell.Key.Level > usableBaseSpells[key1].Key.Level)
                            {
                                if (AllowMultipleSpellVersions(spell.Value))
                                {
                                    // add or replace in the secondary list
                                    if (usableSpecSpells.ContainsKey(key2))
                                    {
                                        usableSpecSpells[key2] = usableBaseSpells[key1];
                                    }
                                    else
                                    {
                                        try
                                        {
                                            usableSpecSpells.Add(key2, usableBaseSpells[key1]);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }

                                usableBaseSpells[key1] = spell;
                            }
                        }
                        else
                        {
                            usableBaseSpells.Add(key1, spell);
                        }
                    }
                    finally
                    {
                        m_syncLockUpdates.ExitReadLock();
                    }
                }


                m_syncLockUpdates.EnterReadLock();
                try
                {
                    // Replace any existing spells or adding new ones to the end of the list
                    // foreach (var kvp in LocalyKeyWords.ToList())  // .ToList() makes a temp copy
                    foreach (KeyValuePair<string, KeyValuePair<Spell, SpellLine>> spell in usableBaseSpells.ToList())
                    {


                        if (m_usableSpells.ContainsKey(spell.Key))
                        {

                            m_usableSpells[spell.Key] = spell.Value;
                        }
                        else
                        {
                            try
                            {
                                m_usableSpells.Add(spell.Key, spell.Value);
                            }
                            catch
                            {
                            }
                        }
                    }
                }

                finally
                {
                    m_syncLockUpdates.ExitReadLock();
                }

                m_syncLockUpdates.EnterReadLock();
                try
                {
                    foreach (KeyValuePair<string, KeyValuePair<Spell, SpellLine>> spell in usableSpecSpells.ToList())
                    {

                        if (m_usableSpells.ContainsKey(spell.Key))
                        {

                            m_usableSpells[spell.Key] = spell.Value;
                        }
                        else
                        {
                            try
                            {
                                m_usableSpells.Add(spell.Key, spell.Value);
                            }
                            catch
                            {
                            }
                        }
                    }
                }

                finally
                {
                    m_syncLockUpdates.ExitReadLock();
                }
            }


            return m_usableSpells;
        }


        /// <summary>
        /// updates the list of available styles
        /// </summary>
        /// <param name="sendMessages">sends "you learn" messages if true</param>
        public virtual void RefreshSpecDependantSkills(bool sendMessages)
        {
            List<Style> newStyles = new List<Style>();
           
                    foreach (Specialization spec in m_specList)
                    {
                        // check styles
                        List<Style> styles = SkillBase.GetStyleList(spec.KeyName, CharacterClass.ID);
                        foreach (Style style in styles)
                        {
                            if (style == null)
                                continue;
                            if (style.SpecLevelRequirement <= spec.Level)
                            {
                                if (!m_styles.ContainsKey(style.ID))
                                {
                                    newStyles.Add(style);
                                    m_styles.TryAdd(style.ID, style);
                                }
                            }
                        }

                        // check abilities
                        List<Ability> abilities = SkillBase.GetSpecAbilityList(spec.KeyName);
                        foreach (Ability ability in abilities)
                        {
                            if (ability.SpecLevelRequirement <= spec.Level)
                            {
                                AddAbility(ability);    // add ability cares about all
                            }
                       
                }

                foreach (Style style in GetChampionStyleList())
                {
                    if (style == null)
                        continue;

                    if (!m_styles.ContainsKey(style.ID))
                    {
                        newStyles.Add(style);
                        m_styles.TryAdd(style.ID, style);
                    }
                }
            }

            if (sendMessages)
            {
                foreach (Style style in newStyles)
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RefreshSpec.YouLearn", style.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                    string message = null;
                    if (Style.eOpening.Offensive == style.OpeningRequirementType)
                    {
                        switch (style.AttackResultRequirement)
                        {
                            case Style.eAttackResult.Style:
                            case Style.eAttackResult.Hit: // TODO: make own message for hit after styles DB is updated

                                Style reqStyle = SkillBase.GetStyleByID(style.OpeningRequirementValue, CharacterClass.ID);
                                if (reqStyle == null)
                                    message = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RefreshSpec.AfterStyle", "(style " + style.OpeningRequirementValue.ToString() + " not found)");
                                else message = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RefreshSpec.AfterStyle", reqStyle.Name);

                                break;
                            case Style.eAttackResult.Miss: message = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RefreshSpec.AfterMissed"); break;
                            case Style.eAttackResult.Parry: message = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RefreshSpec.AfterParried"); break;
                            case Style.eAttackResult.Block: message = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RefreshSpec.AfterBlocked"); break;
                            case Style.eAttackResult.Evade: message = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RefreshSpec.AfterEvaded"); break;
                            case Style.eAttackResult.Fumble: message = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RefreshSpec.AfterFumbles"); break;
                        }
                    }
                    else if (Style.eOpening.Defensive == style.OpeningRequirementType)
                    {
                        switch (style.AttackResultRequirement)
                        {
                            case Style.eAttackResult.Miss: message = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RefreshSpec.TargetMisses"); break;
                            case Style.eAttackResult.Hit: message = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RefreshSpec.TargetHits"); break;
                            case Style.eAttackResult.Parry: message = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RefreshSpec.TargetParried"); break;
                            case Style.eAttackResult.Block: message = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RefreshSpec.TargetBlocked"); break;
                            case Style.eAttackResult.Evade: message = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RefreshSpec.TargetEvaded"); break;
                            case Style.eAttackResult.Fumble: message = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RefreshSpec.TargetFumbles"); break;
                            case Style.eAttackResult.Style: message = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RefreshSpec.TargetStyle"); break;
                        }
                    }

                    if (message != null)
                        Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
            }
        }

        /// <summary>
        /// updates the levels of all spell lines
        /// specialized spell lines depend from spec levels
        /// base lines depend from player level
        /// </summary>
        /// <param name="sendMessages">sends "You gain power" messages if true</param>
        public virtual void UpdateSpellLineLevels(bool sendMessages)
        {
            lock (((ICollection)m_spellLines).SyncRoot)
            {
                foreach (SpellLine line in m_spellLines)
                {
                    if (line.IsBaseLine)
                    {
                        line.Level = Level;
                    }
                    else
                    {
                        int newSpec = GetBaseSpecLevel(line.Spec);
                        if (newSpec > 0)
                        {
                            if (sendMessages && line.Level < newSpec)
                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.UpdateSpellLine.GainPower", line.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            line.Level = newSpec;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called by trainer when specialization points were added to a skill
        /// </summary>
        /// <param name="skill"></param>
        public void OnSkillTrained(Specialization skill)
        {
            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnSkillTrained.YouSpend", skill.Level.ToString(), skill.Name.ToString()), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnSkillTrained.YouHave", SkillSpecialtyPoints.ToString()), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            Message.SystemToOthers(this, LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnSkillTrained.TrainsInVarious", GetName(0, true)), eChatType.CT_System);
            CharacterClass.OnSkillTrained(this, skill);
            RefreshSpecDependantSkills(true);
            UpdateSpellLineLevels(true);
            Out.SendUpdatePlayerSkills();
        }

        /// <summary>
        /// effectiveness of the player (resurrection illness)
        /// Effectiveness is used in physical/magic damage (exept dot), in weapon skill and max concentration formula
        /// </summary>
        protected double m_playereffectiveness = 1.0;

        /// <summary>
        /// get / set the player's effectiveness.
        /// Effectiveness is used in physical/magic damage (exept dot), in weapon skill and max concentration
        /// </summary>
        public override double Effectiveness
        {
            get { return m_playereffectiveness; }
            set { m_playereffectiveness = value; }
        }

        /// <summary>
        /// Creates new effects list for this living.
        /// </summary>
        /// <returns>New effects list instance</returns>
        protected override GameEffectList CreateEffectsList()
        {
            return new GameEffectPlayerList(this);
        }

        #endregion

        #region Realm-/Region-/Bount-/Skillpoints...

        /// <summary>
        /// Gets/sets player bounty points And Message
        /// (delegate to PlayerCharacter and amount update)
        /// </summary>
        public virtual void BountyPointsAndMassage(long amount)
        {
            if (amount > 0)
                BountyPoints += amount;

            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.YouGainBP", amount.ToString()), eChatType.CT_System, eChatLoc.CL_SystemWindow);

            Out.SendUpdatePoints();
        }


        /// <summary>
        /// Gets/sets player bounty points
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual long BountyPoints
        {
            get { return DBCharacter != null ? DBCharacter.BountyPoints : 0; }
            set { if (DBCharacter != null) DBCharacter.BountyPoints = value; }
        }

        /// <summary>
        /// Gets/sets player realm points
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual long RealmPoints
        {
            get { return DBCharacter != null ? DBCharacter.RealmPoints : 0; }
            set { if (DBCharacter != null) DBCharacter.RealmPoints = value; }
        }

        /// <summary>
        /// Gets/sets player skill specialty points
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int SkillSpecialtyPoints
        {
            get { return DBCharacter != null ? DBCharacter.SkillSpecialtyPoints : 0; }
            set { if (DBCharacter != null) DBCharacter.SkillSpecialtyPoints = value; }
        }

        /// <summary>
        /// Gets/sets player realm specialty points
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int RealmSpecialtyPoints
        {
            get { return DBCharacter != null ? DBCharacter.RealmSpecialtyPoints : 0; }
            set { if (DBCharacter != null) DBCharacter.RealmSpecialtyPoints = value; }
        }

        /// <summary>
        /// Gets/sets player realm rank
        /// </summary>
        public virtual int RealmLevel
        {
            get { return DBCharacter != null ? DBCharacter.RealmLevel : 0; }
            set
            {
                if (DBCharacter != null)
                    DBCharacter.RealmLevel = value;
                CharacterClass.OnRealmLevelUp(this);
            }
        }

        /// <summary>
        /// Returns a realm rank title by the given players realm and players gender
        /// </summary>
        /// <param name="Realm"></param>
        /// <param name="Gender"></param>
        /// <returns></returns>
        public virtual string RealmRankTitles(eRealm Realm, int Gender)
        {
            if (Realm != eRealm.None)
            {
                int m_RR = DBCharacter.RealmLevel / 10;

                switch (m_RR)
                {
                    //Because realm levels are only start to count at realm rank 1 (and so we need 120 levels to get realm rank 13), our first case
                    //will be start with 0 (realm rank 1) and our last case will be end with 12 (realm rank 13)
                    case 0:
                        if (DBCharacter.RealmLevel < 1)
                        {
                            /*******************************************************************************************************************/
                            /* This titles are only used by characters where realm level == 0. If players realm points are higher ( > ) then 0 */
                            /* or player reaches level 20, normal realm rank titles will be used.                                              */
                            /*                                                                                                                 */
                            /* This titles are only displayed (without listing it as realm rank in the character info) in realm enemy names.   */
                            /*******************************************************************************************************************/

                            //Because albion and midgard using the same title, we only check if the realm is unequal to hibernia
                            if (Realm != eRealm.Hibernia)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Invader");
                            }
                            //Otherwise we return the hibernian title
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Defender");
                        }
                        else
                        {
                            if (Realm == eRealm.Albion)
                            {
                                if (Gender != 0)
                                {
                                    return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.Guardian");
                                }
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.Guardian");
                            }

                            if (Realm == eRealm.Midgard)
                            {
                                if (Gender != 0)
                                {
                                    return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.Skiltvakten");
                                }
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.Skiltvakten");
                            }

                            if (Realm == eRealm.Hibernia)
                            {
                                if (Gender != 0)
                                {
                                    return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.Savant");
                                }
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.Savant");
                            }
                        }
                        break;
                    case 1:
                        if (Realm == eRealm.Albion)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.Warder");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.Warder");
                        }

                        if (Realm == eRealm.Midgard)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.IsenVakten");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.IsenVakten");
                        }

                        if (Realm == eRealm.Hibernia)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.Cosantoir");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.Cosantoir");
                        }
                        break;
                    case 2:
                        if (Realm == eRealm.Albion)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.Myrmidon");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.Myrmidon");
                        }

                        if (Realm == eRealm.Midgard)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.FlammenVakten");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.FlammenVakten");
                        }

                        if (Realm == eRealm.Hibernia)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.Brehon");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.Brehon");
                        }
                        break;
                    case 3:
                        if (Realm == eRealm.Albion)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.GryphonKnight");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.GryphonKnight");
                        }

                        if (Realm == eRealm.Midgard)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.EldingVakten");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.EldingVakten");
                        }

                        if (Realm == eRealm.Hibernia)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.GroveProtector");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.GroveProtector");
                        }
                        break;
                    case 4:
                        if (Realm == eRealm.Albion)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.EagleKnight");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.EagleKnight");
                        }

                        if (Realm == eRealm.Midgard)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.StormurVakten");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.StormurVakten");
                        }

                        if (Realm == eRealm.Hibernia)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.RavenArdent");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.RavenArdent");
                        }
                        break;
                    case 5:
                        if (Realm == eRealm.Albion)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.PhoenixKnight");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.PhoenixKnight");
                        }

                        if (Realm == eRealm.Midgard)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.IsenFru");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.IsenHerra");
                        }

                        if (Realm == eRealm.Hibernia)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.SilverHand");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.SilverHand");
                        }
                        break;
                    case 6:
                        if (Realm == eRealm.Albion)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.AlerionKnight");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.AlerionKnight");
                        }

                        if (Realm == eRealm.Midgard)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.FlammenFru");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.FlammenHerra");
                        }

                        if (Realm == eRealm.Hibernia)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.Thunderer");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.Thunderer");
                        }
                        break;
                    case 7:
                        if (Realm == eRealm.Albion)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.UnicornKnight");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.UnicornKnight");
                        }

                        if (Realm == eRealm.Midgard)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.EldingFru");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.EldingHerra");
                        }

                        if (Realm == eRealm.Hibernia)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.GildedSpear");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.GildedSpear");
                        }
                        break;
                    case 8:
                        if (Realm == eRealm.Albion)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.LionKnight");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.LionKnight");
                        }

                        if (Realm == eRealm.Midgard)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.StormurFru");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.StormurHerra");
                        }

                        if (Realm == eRealm.Hibernia)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.Bantiarna");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.Tiarna");
                        }
                        break;
                    case 9:
                        if (Realm == eRealm.Albion)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.DragonKnight");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.DragonKnight");
                        }

                        if (Realm == eRealm.Midgard)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.Einherjar");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.Einherjar");
                        }

                        if (Realm == eRealm.Hibernia)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.EmeraldRidere");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.EmeraldRidere");
                        }
                        break;
                    case 10:
                        if (Realm == eRealm.Albion)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.Lady");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.Lord");
                        }

                        if (Realm == eRealm.Midgard)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.Fru");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.Herra");
                        }

                        if (Realm == eRealm.Hibernia)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.Banbharun");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.Barun");
                        }
                        break;
                    case 11:
                        if (Realm == eRealm.Albion)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.Baronetess");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.Baronet");
                        }

                        if (Realm == eRealm.Midgard)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.Baronsfru");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.Hersir");
                        }

                        if (Realm == eRealm.Hibernia)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.ArdBantiarna");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.ArdTiarna");
                        }
                        break;
                    case 12:
                        if (Realm == eRealm.Albion)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.Baroness");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.Baron");
                        }

                        if (Realm == eRealm.Midgard)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.Vicomtessa");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.Vicomte");
                        }

                        if (Realm == eRealm.Hibernia)
                        {
                            if (Gender != 0)
                            {
                                return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Female.CiannCath");
                            }
                            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.Male.CiannCath");
                        }
                        break;
                }
            }
            return LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.RealmTitle.UnknownRealm"); //Returns 'Unknown Realm'
        }


        /// <summary>
        /// Gets player realm rank name
        /// sirru mod 20.11.06
        /// </summary>
        public virtual string RealmTitle
        {
            get
            {
                return RealmRankTitles(Realm, DBCharacter.Gender);
            }
        }

        /// <summary>
        /// Called when this player gains realm points
        /// </summary>
        /// <param name="amount">The amount of realm points gained</param>
        public override void GainRealmPoints(long amount)
        {
            GainRealmPoints(amount, true, true);
        }

        /// <summary>
        /// Called when this living gains realm points
        /// </summary>
        /// <param name="amount">The amount of realm points gained</param>
        public void GainRealmPoints(long amount, bool modify)
        {
            GainRealmPoints(amount, modify, true);
        }




        /// <summary>
        /// Called when this player gains realm points
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="modify"></param>
        /// <param name="sendMessage"></param>
        public void GainRealmPoints(long amount, bool modify, bool sendMessage)
        {
            GainRealmPoints(amount, modify, true, true);
        }


        /// <summary>
        /// Called when this player gains realm points
        /// </summary>
        /// <param name="amount">The amount of realm points gained</param>
        /// <param name="modify">Should we apply the rp modifer</param>
        /// <param name="sendMessage">Wether to send a message like "You have gained N realmpoints"</param>
        /// <param name="notify"></param>
        public virtual void GainRealmPoints(long amount, bool modify, bool sendMessage, bool notify)
        {
            if (!GainRP)
                return;

            if (RealmLevel >= 40 && ServerProperties.Properties.REALM_RANG5_CAP == true)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client, "You have the Realm Rang 5 cap reatched, we wait first for more players, later we will disable this option."), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                return;
            }


            if (modify)
            {
                //rp rate modifier
                double modifier = ServerProperties.Properties.RP_RATE;
                if (modifier != -1)
                    if (RealmLevel < 40)
                        amount = (long)(amount * modifier);
                if (RealmLevel >= 40 && RealmLevel < 50)
                    amount = (long)(amount * ServerProperties.Properties.LOW_RP_RATE);
                if (RealmLevel >= 50)
                    amount = (long)(amount * ServerProperties.Properties.LOW_RP_RATE_RR6);


                if (ServerProperties.Properties.ENABLE_ZONE_BONUSES)
                {
                    int zoneBonus = (((int)amount * ZoneBonus.GetRPBonus(this)) / 100);
                    if (zoneBonus > 0)
                    {
                        Out.SendMessage(ZoneBonus.GetBonusMessage(this, (int)(zoneBonus * ServerProperties.Properties.RP_RATE), ZoneBonus.eZoneBonusType.RP),
                                        eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        if (RealmLevel < 40)
                            GainRealmPoints((long)(zoneBonus * ServerProperties.Properties.RP_RATE), false, false, false);
                        if (RealmLevel >= 40 && RealmLevel < 50)
                            GainRealmPoints((long)(zoneBonus * ServerProperties.Properties.LOW_RP_RATE), false, false, false);
                        if (RealmLevel >= 50)
                            GainRealmPoints((long)(zoneBonus * (ServerProperties.Properties.LOW_RP_RATE_RR6)), false, false, false);

                    }
                }

                //[Freya] Nidel: ToA Rp Bonus
                long rpBonus = GetModified(eProperty.RealmPoints);
                if (rpBonus > 0)
                {
                    amount += (amount * rpBonus) / 100;
                }
            }

            if (notify)
                base.GainRealmPoints(amount);




            if (Client.Player.TargetObject != null && Client.Player.TargetObject is GamePlayer && ServerProperties.Properties.LOG_RP_FARMING)
            {
                string ipAddress = Client.TcpEndpointAddress;
                GamePlayer target = Client.Player.TargetObject as GamePlayer;
                GamePlayer killer = Client.Player as GamePlayer;

                if (DamageRvRMemory <= 0 && target.Client.TcpEndpointAddress != null && killer.Client.TcpEndpointAddress.Equals(target.Client.TcpEndpointAddress) && killer.Client.Account.Name != target.Client.Account.Name && target.IsAlive == false)
                {

                    String message = String.Format("Warning: self RP farming of  " + Client.Player.Name + " have been logged", Name);
                    NewsMgr.CreateNews(message, killer.Realm, eNewsType.RvRGlobal, true);

                    Out.SendMessage(LanguageMgr.GetTranslation(Client, "your ip =  " + ipAddress + " have been logged"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);

                    amount = 0;
                }
            }



            RealmPoints += amount;

            if (m_guild != null && Client.Account.PrivLevel == 1)
                m_guild.RealmPoints += amount;

            if (sendMessage == true && amount > 0)
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainRealmPoints.YouGet", amount.ToString()), eChatType.CT_Important, eChatLoc.CL_SystemWindow);

            while (RealmPoints >= CalculateRPsFromRealmLevel(RealmLevel + 1) && RealmLevel < (REALMPOINTS_FOR_LEVEL.Length - 1))
            {
                RealmLevel++;
                RealmSpecialtyPoints++;
                Out.SendUpdatePlayer();
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainRealmPoints.GainedLevel"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                if (RealmLevel % 10 == 0)
                {
                    Out.SendUpdatePlayerSkills();
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainRealmPoints.GainedRank"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainRealmPoints.ReachedRank", ((RealmLevel / 10) + 1)).ToString(), eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainRealmPoints.NewRealmTitle", RealmTitle), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainRealmPoints.GainBonus", (RealmLevel / 10).ToString()), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    foreach (GamePlayer plr in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                        plr.Out.SendLivingDataUpdate(this, true);
                    Notify(GamePlayerEvent.RRLevelUp, this);
                }
                else
                    Notify(GamePlayerEvent.RLLevelUp, this);
                if (CanGenerateNews && ((RealmLevel >= 40 && RealmLevel % 10 == 0) || RealmLevel >= 60))
                {
                    string RealmLevelToName = " " + RealmLevel / 10 + "L0";

                    string newsmessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainRealmPoints.ReachedRankNews", Name, RealmLevelToName, LastPositionUpdateZone.Description);
                    NewsMgr.CreateNews(newsmessage, this.Realm, eNewsType.RvRLocal, true);
                }
                if (CanGenerateNews && RealmPoints >= 1000000 && RealmPoints - amount < 1000000)
                {
                    string newsmessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainRealmPoints.Earned", Name, LastPositionUpdateZone.Description);
                    NewsMgr.CreateNews(newsmessage, this.Realm, eNewsType.RvRLocal, true);
                }
            }
            Out.SendUpdatePoints();
        }

        /// <summary>
        /// Called when this living buy something with realm points
        /// </summary>
        /// <param name="amount">The amount of realm points loosed</param>
        public bool RemoveBountyPoints(long amount)
        {
            return RemoveBountyPoints(amount, null);
        }
        /// <summary>
        /// Called when this living buy something with realm points
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool RemoveBountyPoints(long amount, string str)
        {
            return RemoveBountyPoints(amount, str, eChatType.CT_Say, eChatLoc.CL_SystemWindow);
        }
        /// <summary>
        /// Called when this living buy something with realm points
        /// </summary>
        /// <param name="amount">The amount of realm points loosed</param>
        /// <param name="loc">The chat location</param>
        /// <param name="str">The message</param>
        /// <param name="type">The chat type</param>
        public virtual bool RemoveBountyPoints(long amount, string str, eChatType type, eChatLoc loc)
        {
            if (BountyPoints < amount)
                return false;
            BountyPoints -= amount;
            Out.SendUpdatePoints();
            if (str != null && amount != 0)
                Out.SendMessage(str, type, loc);
            return true;
        }

        /// <summary>
        /// Player gains bounty points
        /// </summary>
        /// <param name="amount">The amount of bounty points</param>
        public override void GainBountyPoints(long amount)
        {
            GainBountyPoints(amount, true, true);
        }

        /// <summary>
        /// Player gains bounty points
        /// </summary>
        /// <param name="amount">The amount of bounty points</param>
        public void GainBountyPoints(long amount, bool modify)
        {
            GainBountyPoints(amount, modify, true);
        }

        /// <summary>
        /// Called when player gains bounty points
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="modify"></param>
        /// <param name="sendMessage"></param>
        public void GainBountyPoints(long amount, bool modify, bool sendMessage)
        {
            GainBountyPoints(amount, modify, true, true);
        }


        /// <summary>
        /// Called when player gains bounty points
        /// </summary>
        /// <param name="amount">The amount of bounty points gained</param>
        /// <param name="multiply">Should this amount be multiplied by the BP Rate</param>
        /// <param name="sendMessage">Wether to send a message like "You have gained N bountypoints"</param>
        public virtual void GainBountyPoints(long amount, bool modify, bool sendMessage, bool notify)
        {
            if (modify)
            {
                //bp rate modifier
                double modifier = ServerProperties.Properties.BP_RATE;
                if (modifier != -1)
                    amount = (long)(amount * modifier);

                //[StephenxPimente]: Zone Bonus Support
                if (ServerProperties.Properties.ENABLE_ZONE_BONUSES)
                {
                    int zoneBonus = (((int)amount * ZoneBonus.GetBPBonus(this)) / 100);
                    if (zoneBonus > 0)
                    {
                        Out.SendMessage(ZoneBonus.GetBonusMessage(this, (int)(zoneBonus * ServerProperties.Properties.BP_RATE), ZoneBonus.eZoneBonusType.BP),
                                        eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        GainBountyPoints((long)(zoneBonus * ServerProperties.Properties.BP_RATE), false, false, false);
                    }
                }

                //[Freya] Nidel: ToA Bp Bonus
                long bpBonus = GetModified(eProperty.BountyPoints);

                if (bpBonus > 0)
                {
                    amount += (amount * bpBonus) / 100;
                }
            }

            if (notify)
                base.GainBountyPoints(amount);

            BountyPoints += amount;

            if (m_guild != null && Client.Account.PrivLevel == 1)
                m_guild.BountyPoints += amount;

            if (sendMessage == true)
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainBountyPoints.YouGet", amount.ToString()), eChatType.CT_Important, eChatLoc.CL_SystemWindow);

            Out.SendUpdatePoints();
        }

        /// <summary>
        /// Holds realm points needed for special realm level
        /// </summary>
        public static readonly long[] REALMPOINTS_FOR_LEVEL =
        {
            0,	// for level 0
			0,	// for level 1
			25,	// for level 2
			125,	// for level 3
			350,	// for level 4
			750,	// for level 5
			1375,	// for level 6
			2275,	// for level 7
			3500,	// for level 8
			5100,	// for level 9
			7125,	// for level 10
			9625,	// for level 11
			12650,	// for level 12
			16250,	// for level 13
			20475,	// for level 14
			25375,	// for level 15
			31000,	// for level 16
			37400,	// for level 17
			44625,	// for level 18
			52725,	// for level 19
			61750,	// for level 20
			71750,	// for level 21
			82775,	// for level 22
			94875,	// for level 23
			108100,	// for level 24
			122500,	// for level 25
			138125,	// for level 26
			155025,	// for level 27
			173250,	// for level 28
			192850,	// for level 29
			213875,	// for level 30
			236375,	// for level 31
			260400,	// for level 32
			286000,	// for level 33
			313225,	// for level 34
			342125,	// for level 35
			372750,	// for level 36
			405150,	// for level 37
			439375,	// for level 38
			475475,	// for level 39
			513500,	// for level 40
			553500,	// for level 41
			595525,	// for level 42
			639625,	// for level 43
			685850,	// for level 44
			734250,	// for level 45
			784875,	// for level 46
			837775,	// for level 47
			893000,	// for level 48
			950600,	// for level 49
			1010625,	// for level 50
			1073125,	// for level 51
			1138150,	// for level 52
			1205750,	// for level 53
			1275975,	// for level 54
			1348875,	// for level 55
			1424500,	// for level 56
			1502900,	// for level 57
			1584125,	// for level 58
			1668225,	// for level 59
			1755250,	// for level 60
			1845250,	// for level 61
			1938275,	// for level 62
			2034375,	// for level 63
			2133600,	// for level 64
			2236000,	// for level 65
			2341625,	// for level 66
			2450525,	// for level 67
			2562750,	// for level 68
			2678350,	// for level 69
			2797375,	// for level 70
			2919875,	// for level 71
			3045900,	// for level 72
			3175500,	// for level 73
			3308725,	// for level 74
			3445625,	// for level 75
			3586250,	// for level 76
			3730650,	// for level 77
			3878875,	// for level 78
			4030975,	// for level 79
			4187000,	// for level 80
			4347000,	// for level 81
			4511025,	// for level 82
			4679125,	// for level 83
			4851350,	// for level 84
			5027750,	// for level 85
			5208375,	// for level 86
			5393275,	// for level 87
			5582500,	// for level 88
			5776100,	// for level 89
			5974125,	// for level 90
			6176625,	// for level 91
			6383650,	// for level 92
			6595250,	// for level 93
			6811475,	// for level 94
			7032375,	// for level 95
			7258000,	// for level 96
			7488400,	// for level 97
			7723625,	// for level 98
			7963725,	// for level 99
			8208750,	// for level 100
			9111713,	// for level 101
			10114001,	// for level 102
			11226541,	// for level 103
			12461460,	// for level 104
			13832221,	// for level 105
			15353765,	// for level 106
			17042680,	// for level 107
			18917374,	// for level 108
			20998286,	// for level 109
			23308097,	// for level 110
			25871988,	// for level 111
			28717906,	// for level 112
			31876876,	// for level 113
			35383333,	// for level 114
			39275499,	// for level 115
			43595804,	// for level 116
			48391343,	// for level 117
			53714390,	// for level 118
			59622973,	// for level 119
			66181501,	// for level 120
		};

        /// <summary>
        /// Calculates amount of RealmPoints needed for special realm level
        /// </summary>
        /// <param name="realmLevel">realm level</param>
        /// <returns>amount of realm points</returns>
        protected virtual long CalculateRPsFromRealmLevel(int realmLevel)
        {
            if (realmLevel < REALMPOINTS_FOR_LEVEL.Length)
                return REALMPOINTS_FOR_LEVEL[realmLevel];

            // thanks to Linulo from http://daoc.foren.4players.de/viewtopic.php?t=40839&postdays=0&postorder=asc&start=0
            return (long)(25.0 / 3.0 * (realmLevel * realmLevel * realmLevel) - 25.0 / 2.0 * (realmLevel * realmLevel) + 25.0 / 6.0 * realmLevel);
        }

        /// <summary>
        /// Calculates realm level from realm points. SLOW.
        /// </summary>
        /// <param name="realmPoints">amount of realm points</param>
        /// <returns>realm level: RR5L3 = 43, RR1L2 = 2</returns>
        protected virtual int CalculateRealmLevelFromRPs(long realmPoints)
        {
            if (realmPoints == 0)
                return 0;

            int i;

            for (i = REALMPOINTS_FOR_LEVEL.Length - 1; i > 0; i--)
            {
                if (REALMPOINTS_FOR_LEVEL[i] <= realmPoints)
                    break;
            }

            return i;
        }

        /// <summary>
        /// Realm point value of this player
        /// </summary>
        public override int RealmPointsValue
        {
            get
            {
                // http://www.camelotherald.com/more/2275.shtml
                // new 1.81D formula
                // Realm point value = (level - 20)squared + (realm rank level x 5) + (champion level x 10) + (master level (squared)x 5)
                //we use realm level 1L0 = 0, mythic uses 1L0 = 10, so we + 10 the realm level
                int level = Math.Max(0, Level - 20);
                if (level == 0)
                    return Math.Max(1, (RealmLevel + 10) * 5);

                return Math.Max(1, level * level + (RealmLevel + 10) * 5);
            }
        }

        /// <summary>
        /// Bounty point value of this player
        /// </summary>
        public override int BountyPointsValue
        {
            // TODO: correct formula!
            get { return (int)(1 + Level * 0.6); }
        }

        /// <summary>
        /// Returns the amount of experience this player is worth
        /// </summary>
        public override long ExperienceValue
        {
            get
            {
                return base.ExperienceValue * 4;
            }
        }

        public static readonly int[] prcRestore =
        {
			// http://www.silicondragon.com/Gaming/DAoC/Misc/XPs.htm
			0,//0
			0,//1
			0,//2
			0,//3
			0,//4
			0,//5
			33,//6
			53,//7
			82,//8
			125,//9
			188,//10
			278,//11
			352,//12
			443,//13
			553,//14
			688,//15
			851,//16
			1048,//17
			1288,//18
			1578,//19
			1926,//20
			2347,//21
			2721,//22
			3146,//23
			3633,//24
			4187,//25
			4820,//26
			5537,//27
			6356,//28
			7281,//29
			8337,//30
			9532,//31 - from logs
			10886,//32 - from logs
			12421,//33 - from logs
			14161,//34
			16131,//35
			18360,//36 - recheck
			19965,//37 - guessed
			21857,//38
			23821,//39
			25928,//40 - guessed
			28244,//41
			30731,//42
			33411,//43
			36308,//44
			39438,//45
			42812,//46
			46454,//47
			50385,//48
			54625,//49
			59195,//50
		};

        /// <summary>
        /// Money value of this player
        /// </summary>
        public override long MoneyValue
        {
            get
            {
                return 3 * prcRestore[Level < GamePlayer.prcRestore.Length ? Level : GamePlayer.prcRestore.Length - 1];
            }
        }

        #endregion

        #region Level/Experience

        /// <summary>
        /// What is the maximum level a player can achieve?
        /// To alter this in a custom GamePlayer class you must override this method and
        /// provide your own XPForLevel array with MaxLevel + 1 entries
        /// </summary>
        public virtual byte MaxLevel
        {
            get { return (byte)Properties.Player_MaxLevel; }
        }

        /*
        /// <summary>
        /// How much experience is needed for a given level?
        /// </summary>
        public virtual long GetExperienceNeededForLevel(int level)
        {
            if (level > MaxLevel)
                return XPForLevel[MaxLevel];

            if (level <= 0)
                return XPForLevel[0];

            return XPForLevel[level - 1];
        }
        */

        /// <summary>
		/// How much experience is needed for a given level?
		/// </summary>
		public virtual long GetExperienceNeededForLevel(int level)
        {
            if (level > MaxLevel)
                return GetExperienceAmountForLevel(MaxLevel);

            if (level <= 0)
                return GetExperienceAmountForLevel(0);

            return GetExperienceAmountForLevel(level - 1);
        }

        /// <summary>
		/// How Much Experience Needed For Level
		/// </summary>
		/// <param name="level"></param>
		/// <returns></returns>
		public static long GetExperienceAmountForLevel(int level)
        {
            try
            {
                return XPForLevel[level];
            }
            catch
            {
                return 0;
            }
        }


        /// <summary>
        /// A table that holds the required XP/Level
        /// This must include a final entry for MaxLevel + 1
        /// </summary>
        private static readonly long[] XPForLevel =
        {
            0, // xp to level 1
			50, // xp to level 2
			250, // xp to level 3
			850, // xp to level 4
			2300, // xp to level 5
			6350, // xp to level 6
			15950, // xp to level 7
			37950, // xp to level 8
			88950, // xp to level 9
			203950, // xp to level 10
			459950, // xp to level 11
			839950, // xp to level 12
			1399950, // xp to level 13
			2199950, // xp to level 14
			3399950, // xp to level 15
			5199950, // xp to level 16
			7899950, // xp to level 17
			11799950, // xp to level 18
			17499950, // xp to level 19
			25899950, // xp to level 20
			38199950, // xp to level 21
			54699950, // xp to level 22
			76999950, // xp to level 23
			106999950, // xp to level 24
			146999950, // xp to level 25
			199999950, // xp to level 26
			269999950, // xp to level 27
			359999950, // xp to level 28
			479999950, // xp to level 29
			639999950, // xp to level 30
			849999950, // xp to level 31
			1119999950, // xp to level 32
			1469999950, // xp to level 33
			1929999950, // xp to level 34
			2529999950, // xp to level 35
			3319999950, // xp to level 36
			4299999950, // xp to level 37
			5499999950, // xp to level 38
			6899999950, // xp to level 39
			8599999950, // xp to level 40
			12899999950, // xp to level 41
			20699999950, // xp to level 42
			29999999950, // xp to level 43
			40799999950, // xp to level 44
			53999999950, // xp to level 45
			69599999950, // xp to level 46
			88499999950, // xp to level 47
			110999999950, // xp to level 48
			137999999950, // xp to level 49
			169999999950, // xp to level 50
			999999999950, // xp to level 51
		};

        /// <summary>
        /// Gets or sets the current xp of this player
        /// </summary>
        public virtual long Experience
        {
            get { return DBCharacter != null ? DBCharacter.Experience : 0; }
            set
            {
                if (DBCharacter != null)
                    DBCharacter.Experience = value;
            }
        }

        /// <summary>
        /// Returns the xp that are needed for the next level
        /// </summary>
        public virtual long ExperienceForNextLevel
        {
            get
            {
                return GetExperienceNeededForLevel(Level + 1);
            }
        }

        /// <summary>
        /// Returns the xp that were needed for the current level
        /// </summary>
        public virtual long ExperienceForCurrentLevel
        {
            get
            {
                return GetExperienceNeededForLevel(Level);
            }
        }

        /// <summary>
        /// Returns the xp that is needed for the second stage of current level
        /// </summary>
        public virtual long ExperienceForCurrentLevelSecondStage
        {
            get { return 1 + ExperienceForCurrentLevel + (ExperienceForNextLevel - ExperienceForCurrentLevel) / 2; }
        }

        /// <summary>
        /// Returns how far into the level we have progressed
        /// A value between 0 and 1000 (1 bubble = 100)
        /// </summary>
        public virtual ushort LevelPermill
        {
            get
            {
                //No progress if we haven't even reached current level!
                if (Experience < ExperienceForCurrentLevel)
                    return 0;
                //No progess after maximum level
                if (Level > MaxLevel)
                    return 0;
                return (ushort)(1000 * (Experience - ExperienceForCurrentLevel) / (ExperienceForNextLevel - ExperienceForCurrentLevel));
            }
        }

        /// <summary>
        /// Called whenever this player gains experience
        /// </summary>
        /// <param name="expTotal"></param>
        /// <param name="expCampBonus"></param>
        /// <param name="expGroupBonus"></param>
        /// <param name="expOutpostBonus"></param>
        /// <param name="sendMessage"></param>
        public void GainExperience(eXPSource xpSource, long expTotal, long expCampBonus, long expGroupBonus, long expOutpostBonus, bool sendMessage)
        {
            GainExperience(xpSource, expTotal, expCampBonus, expGroupBonus, expOutpostBonus, sendMessage, true);
        }

        /// <summary>
        /// Called whenever this player gains experience
        /// </summary>
        /// <param name="expTotal"></param>
        /// <param name="expCampBonus"></param>
        /// <param name="expGroupBonus"></param>
        /// <param name="expOutpostBonus"></param>
        /// <param name="sendMessage"></param>
        /// <param name="allowMultiply"></param>
        public void GainExperience(eXPSource xpSource, long expTotal, long expCampBonus, long expGroupBonus, long expOutpostBonus, bool sendMessage, bool allowMultiply)
        {
            GainExperience(xpSource, expTotal, expCampBonus, expGroupBonus, expOutpostBonus, sendMessage, allowMultiply, true);
          
        }

        /// <summary>
        /// Called whenever this player gains experience
        /// </summary>
        /// <param name="expTotal"></param>
        /// <param name="expCampBonus"></param>
        /// <param name="expGroupBonus"></param>
        /// <param name="expOutpostBonus"></param>
        /// <param name="sendMessage"></param>
        /// <param name="allowMultiply"></param>
        /// <param name="notify"></param>
        public override void GainExperience(eXPSource xpSource, long expTotal, long expCampBonus, long expGroupBonus, long expOutpostBonus, bool sendMessage, bool allowMultiply, bool notify)
        {
            
            if (!GainXP && expTotal > 0)
                return;

          
            //xp rate modifier
            if (allowMultiply)
            {
                //we only want to modify the base rate, not the group or camp bonus
                expTotal -= expGroupBonus;
                expTotal -= expCampBonus;
                expTotal -= expOutpostBonus;
                GamePlayer player = Client.Player as GamePlayer;
                //[StephenxPimentel] - Zone Bonus XP Support
                if (ServerProperties.Properties.ENABLE_ZONE_BONUSES)
                {
                    int zoneBonus = (((int)expTotal * ZoneBonus.GetXPBonus(this)) / 100);
                    if (zoneBonus > 0 && player.IsAlive)
                    {
                        Out.SendMessage(ZoneBonus.GetBonusMessage(this, (int)(zoneBonus * ServerProperties.Properties.XP_RATE), ZoneBonus.eZoneBonusType.XP),
                                        eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        GainExperience(eXPSource.Other, (long)(zoneBonus * ServerProperties.Properties.XP_RATE), 0, 0, 0, false, false, false);
                    }
                }


                if (ServerProperties.Properties.LOW_XP_RATES)
                {
                    if (this.CurrentRegion.IsRvR)
                        expTotal = (long)(expTotal * ServerProperties.Properties.RvR_XP_RATE / player.Level);
                    else
                        expTotal = (long)(expTotal * ServerProperties.Properties.XP_RATE / player.Level);
                }
                else
                {
                    if (this.CurrentRegion.IsRvR)
                        expTotal = (long)(expTotal * ServerProperties.Properties.RvR_XP_RATE);
                    else
                        expTotal = (long)(expTotal * ServerProperties.Properties.XP_RATE);
                }


                long hardXPCap = (long)(GameServer.ServerRules.GetExperienceForLiving(Level) * ServerProperties.Properties.XP_HARDCAP_PERCENT / 100);

                if (expTotal > hardXPCap)
                    expTotal = hardXPCap;

                expTotal += expOutpostBonus;
                expTotal += expGroupBonus;
                expTotal += expCampBonus;

            }

            // Get Champion Experience too
            GainChampionExperience(expTotal);

            //catacombs characters get 50% boost if they are elligable for slash level
            switch ((eCharacterClass)CharacterClass.ID)
            {
                case eCharacterClass.Heretic:
                case eCharacterClass.Valkyrie:
                case eCharacterClass.Warlock:
                case eCharacterClass.Bainshee:
                case eCharacterClass.Vampiir:
                case eCharacterClass.MaulerAlb:
                case eCharacterClass.MaulerHib:
                case eCharacterClass.MaulerMid:
                    {
                        //we don't want to allow catacombs classes to use free levels and
                        //have a 50% bonus
                        if (!ServerProperties.Properties.ALLOW_CATA_SLASH_LEVEL && CanUseSlashLevel && Level < 20)
                        {
                            expTotal = (long)(expTotal * 1.5);
                        }
                        break;
                    }
            }
            


            base.GainExperience(xpSource, expTotal, expCampBonus, expGroupBonus, expOutpostBonus, sendMessage, allowMultiply, notify);

            
            if (IsLevelSecondStage)
            {
                if (Experience + expTotal < ExperienceForCurrentLevelSecondStage)
                {
                    expTotal = ExperienceForCurrentLevelSecondStage - Experience;
                }
             
            }


            //Bug XP: XP werden falsch/zu hoch angerechnet nach level up
            /*
            else if (Experience + expTotal < (ExperienceForCurrentLevel / Level))
            {
                l
                log.ErrorFormat("13 xpReward {0}, player: {1} player.Level: {2},", expTotal, Name, Level);
                expTotal = ExperienceForCurrentLevel - Experience;
                log.ErrorFormat("14 xpReward {0}, player: {1} player.Level: {2}", expTotal, Name, Level);
            }
            */


            // [Freya] Nidel: ToA Xp Bonus
            long xpBonus = GetModified(eProperty.XpPoints);

          

            long erhalten = 0;
            if (xpBonus != 0)
            {
                expTotal += (expTotal * xpBonus) / 100;
                erhalten = expTotal / xpBonus;
            }

           
            if (sendMessage && expTotal > 0)
            {
                System.Globalization.NumberFormatInfo format = System.Globalization.NumberFormatInfo.InvariantInfo;
                string totalItemExpStr = erhalten.ToString("N0", format);
                string totalExpStr = expTotal.ToString("N0", format);
                string expCampBonusStr = String.Empty;
                string expGroupBonusStr = String.Empty;
                string expOutpostBonusStr = String.Empty;
               
                if (expCampBonus > 0)
                {
                    expCampBonusStr = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainExperience.CampBonus", expCampBonus.ToString("N0", format));
                }
                if (expGroupBonus > 0)
                {
                    expGroupBonusStr = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainExperience.GroupBonus", expGroupBonus.ToString("N0", format));
                }
                if (expOutpostBonus > 0)
                {
                    expOutpostBonusStr = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainExperience.OutpostBonus", expOutpostBonus.ToString("N0", format));
                }

                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainExperience.YouGet", totalExpStr) + expCampBonusStr + expGroupBonusStr + expOutpostBonusStr, eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                if (erhalten > 0)
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainExperience.ItemBonus", totalItemExpStr), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
            }

            Experience += expTotal;

            if (expTotal >= 0)
            {
                //Level up
                if (Level >= 5 && CharacterClass.BaseName == CharacterClass.Name)
                {
                    if (expTotal > 0)
                    {
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainExperience.CannotRaise"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainExperience.TalkToTrainer"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    }
                }
                else if (Level >= 40 && Level < MaxLevel && !IsLevelSecondStage && Experience >= ExperienceForCurrentLevelSecondStage)
                {
                    OnLevelSecondStage();
                    Notify(GamePlayerEvent.LevelSecondStage, this);
                    SaveIntoDatabase(); // save char on levelup
                }
                else if (Level < MaxLevel && Experience >= ExperienceForNextLevel)
                {
                    Level++;
                    SaveIntoDatabase(); // save char on levelup
                }
            }
            
            Out.SendUpdatePoints();
        }

        /// <summary>
        /// Gets or sets the level of the player
        /// (delegate to PlayerCharacter)
        /// </summary>
        public override byte Level
        {
            get { return DBCharacter != null ? (byte)DBCharacter.Level : base.Level; }
            set
            {
                int oldLevel = Level;
                base.Level = value;
                if (DBCharacter != null)
                    DBCharacter.Level = value;
                if (oldLevel > 0)
                {
                    if (value > oldLevel)
                    {
                        OnLevelUp(oldLevel);
                        Notify(GamePlayerEvent.LevelUp, this);
                        BroadcastUpdate();
                    }
                    else
                    {
                        //update the mob colours
                        Out.SendLevelUpSound();
                        BroadcastUpdate();
                    }
                }
            }
        }

        /// <summary>
        /// What is the base, unmodified level of this character.
        /// </summary>
        public override byte BaseLevel
        {
            get { return DBCharacter != null ? (byte)DBCharacter.Level : base.BaseLevel; }
        }

        /// <summary>
        /// What level is displayed to another player
        /// </summary>
        public override byte GetDisplayLevel(GamePlayer player)
        {
            return Math.Min((byte)50, Level);
        }

        /// <summary>
        /// Is this player in second stage of current level
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual bool IsLevelSecondStage
        {
            get { return DBCharacter != null ? DBCharacter.IsLevelSecondStage : false; }
            set { if (DBCharacter != null) DBCharacter.IsLevelSecondStage = value; }
        }

        /// <summary>
        /// Called when this player levels
        /// </summary>
        /// <param name="previouslevel"></param>
        public virtual void OnLevelUp(int previouslevel)
        {
            IsLevelSecondStage = false;

            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnLevelUp.YouRaise", Level.ToString()), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnLevelUp.YouAchieved", Level.ToString()), eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
            Out.SendPlayerFreeLevelUpdate();
            if (FreeLevelState == 2)
            {
                Out.SendDialogBox(eDialogCode.SimpleWarning, 0, 0, 0, 0, eDialogType.Ok, true, LanguageMgr.GetTranslation(Client, "GamePlayer.OnLevelUp.FreeLevelEligible"));
            }

            switch (Level)
            {
                // full respec on level 5 since 1.70
                case 5:
                    RespecAmountAllSkill++;
                    IsLevelRespecUsed = false;
                    break;
                case 6:
                    if (IsLevelRespecUsed) break;
                    RespecAmountAllSkill--;
                    break;

                // single line respec
                case 20:
                case 40:
                    {
                        RespecAmountSingleSkill++; // Give character their free respecs at 20 and 40
                        IsLevelRespecUsed = false;
                        break;
                    }
                case 21:
                case 41:
                    {
                        if (IsLevelRespecUsed) break;
                        RespecAmountSingleSkill--; // Remove free respecs if it wasn't used
                        break;
                    }
                case 50:
                    {
                        if (CanGenerateNews)
                        {
                            string newsmessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnLevelUp.Reached", Name, Level.ToString(), LastPositionUpdateZone.Description);
                            NewsMgr.CreateNews(newsmessage, Realm, eNewsType.PvE, true);
                        }
                        break;
                    }
            }

            // Graveen: give a DOL respec on the GIVE_DOL_RESPEC_ON_LEVELS levels


            foreach (string str in ServerProperties.Properties.GIVE_DOL_RESPEC_AT_LEVEL.SplitCSV(true))
            {
                if (!byte.TryParse(str, out byte level_respec))
                    level_respec = 0;

                if (Level == level_respec)
                {
                    int oldAmount = RespecAmountDOL;
                    RespecAmountDOL++;

                    if (oldAmount != RespecAmountDOL)
                    {
                        Out.SendMessage("As you reached level " + Level.ToString() + ", you are awarded a DOL (full) respec!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    }
                }
            }

            //level 20 changes realm title and gives 1 realm skill point
            if (Level == 20)
                GainRealmPoints(0);

            // Adjust stats
            bool statsChanged = false;
            // stat increases start at level 6
            if (Level > 5)
            {
                for (int i = Level; i > Math.Max(previouslevel, 5); i--)
                {
                    if (CharacterClass.PrimaryStat != eStat.UNDEFINED)
                    {
                        ChangeBaseStat(CharacterClass.PrimaryStat, 1);
                        statsChanged = true;
                    }
                    if (CharacterClass.SecondaryStat != eStat.UNDEFINED && ((i - 6) % 2 == 0))
                    { // base level to start adding stats is 6
                        ChangeBaseStat(CharacterClass.SecondaryStat, 1);
                        statsChanged = true;
                    }
                    if (CharacterClass.TertiaryStat != eStat.UNDEFINED && ((i - 6) % 3 == 0))
                    { // base level to start adding stats is 6
                        ChangeBaseStat(CharacterClass.TertiaryStat, 1);
                        statsChanged = true;
                    }
                }
            }

            if (statsChanged)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnLevelUp.StatRaise"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
            }

            CharacterClass.OnLevelUp(this, previouslevel);
            UpdateSpellLineLevels(true);
            RefreshSpecDependantSkills(true);

            // Echostorm - Code for display of new title on level up
            // Get old and current rank titles
            string currenttitle = CharacterClass.GetTitle(Level, this);

            // check for difference
            if (CharacterClass.GetTitle(previouslevel, this) != currenttitle)
            {
                // Inform player of new title.
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnLevelUp.AttainedRank", currenttitle), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
            }

            // spec points
            int specpoints = 0;
            for (int i = Level; i > previouslevel; i--)
            {
                if (i <= 5) specpoints += i; //start levels
                else specpoints += CharacterClass.SpecPointsMultiplier * i / 10; //spec levels
            }
            if (specpoints > 0)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnLevelUp.YouGetSpec", specpoints.ToString()), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
            }
            SkillSpecialtyPoints += specpoints;

            // graveen 1.87 Autotrain
            foreach (Specialization spec in GetSpecList())
                if (Level > 5) SkillSpecialtyPoints += GetAutoTrainPoints(spec, 1);

            // old hp
            int oldhp = CalculateMaxHealth(previouslevel, GetBaseStat(eStat.CON));

            // old power
            int oldpow = 0;
            if (CharacterClass.ManaStat != eStat.UNDEFINED)
            {
                oldpow = CalculateMaxMana(previouslevel, GetBaseStat(CharacterClass.ManaStat));
            }

            // hp upgrade
            int newhp = CalculateMaxHealth(Level, GetBaseStat(eStat.CON));
            if (oldhp > 0 && oldhp < newhp)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnLevelUp.HitsRaise", (newhp - oldhp).ToString()), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
            }

            // power upgrade
            if (CharacterClass.ManaStat != eStat.UNDEFINED)
            {
                int newpow = CalculateMaxMana(Level, GetBaseStat(CharacterClass.ManaStat));
                if (newpow > 0 && oldpow < newpow)
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnLevelUp.PowerRaise", (newpow - oldpow).ToString()), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                }
            }

            if (IsAlive)
            {
                // workaround for starting regeneration
                StartHealthRegeneration();
                StartPowerRegeneration();
            }

            if (DBCharacter != null)
                DBCharacter.DeathCount = 0;

            if (Group != null)
            {
                Group.UpdateGroupWindow();
            }
            Out.SendUpdatePlayer(); // Update player level
            Out.SendCharStatsUpdate(); // Update Stats and MaxHitpoints
            Out.SendCharResistsUpdate();
            Out.SendUpdatePlayerSkills();
            Out.SendUpdatePoints();
            UpdatePlayerStatus();

            // not sure what package this is, but it triggers the mob color update
            Out.SendLevelUpSound();

            // update color on levelup
            if (ObjectState == eObjectState.Active)
            {
                foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    if (player == null) continue;
                    player.Out.SendEmoteAnimation(this, eEmote.LvlUp);
                }
            }

            // Reset taskDone per level.
            if (QuestTask != null)
            {
                QuestTask.TasksDone = 0;
                QuestTask.SaveIntoDatabase();
            }
        }

        /// <summary>
        /// Called when this player reaches second stage of the current level
        /// </summary>
        public virtual void OnLevelSecondStage()
        {
            IsLevelSecondStage = true;

            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnLevelUp.SecondStage", Level.ToString()), eChatType.CT_Important, eChatLoc.CL_SystemWindow);

            // spec points
            int specpoints = CharacterClass.SpecPointsMultiplier * Level / 20;
            if (specpoints > 0)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnLevelUp.YouGetSpec", specpoints.ToString()), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
            }

            SkillSpecialtyPoints += specpoints;
            //death penalty reset on mini-ding
            if (DBCharacter != null)
                DBCharacter.DeathCount = 0;

            if (Group != null)
            {
                Group.UpdateGroupWindow();
            }
            Out.SendUpdatePlayer(); // Update player level
            Out.SendCharStatsUpdate(); // Update Stats and MaxHitpoints
            Out.SendUpdatePlayerSkills();
            Out.SendUpdatePoints();
            UpdatePlayerStatus();
        }

        /// <summary>
        /// Calculate the Autotrain points.
        /// </summary>
        /// <param name="spec">Specialization</param>
        /// <param name="mode">various AT related calculations (amount of points, level of AT...)</param>
        public virtual int GetAutoTrainPoints(Specialization spec, int Mode)
        {
            int max_autotrain = Level / 4;
            if (max_autotrain == 0) max_autotrain = 1;

            foreach (string autotrainKey in CharacterClass.GetAutotrainableSkills())
            {
                if (autotrainKey == spec.KeyName)
                {
                    switch (Mode)
                    {
                        case 0:// return sum of all AT points in the spec
                            {
                                int pts_to_refund = Math.Min(max_autotrain, spec.Level);
                                return ((pts_to_refund * (pts_to_refund + 1) - 2) / 2);
                            }
                        case 1: // return max AT level + message
                            {
                                if (Level % 4 == 0)
                                    if (spec.Level >= max_autotrain)
                                        return max_autotrain;
                                    else
                                        Out.SendDialogBox(eDialogCode.SimpleWarning, 0, 0, 0, 0, eDialogType.Ok, true, LanguageMgr.GetTranslation(Client.Account.Language, "PlayerClass.OnLevelUp.Autotrain", spec.Name, max_autotrain.ToString()));
                                // this.Out.SendUpdatePoints();
                                // this.Out.SendUpdatePlayer();
                                // this.Out.SendUpdatePlayerSkills();
                                return 0;
                            }
                        case 2: // return next free points due to AT change on levelup
                            {
                                if (spec.Level < max_autotrain)
                                    return (spec.Level + 1);
                                else
                                    return 0;
                            }
                        case 3: // return sum of all free AT points
                            {
                                if (spec.Level < max_autotrain)
                                    return (((max_autotrain * (max_autotrain + 1) - 2) / 2) - ((spec.Level * (spec.Level + 1) - 2) / 2));
                                else
                                    return ((max_autotrain * (max_autotrain + 1) - 2) / 2);
                            }
                        case 4: // spec is autotrainable
                            {
                                return 1;
                            }
                    }
                }
            }
            return 0;
        }
        #endregion

        #region Combat

        /// <summary>
        /// The time someone can hold a ranged attack before tiring
        /// </summary>
        internal const string RANGE_ATTACK_HOLD_START = " RangeAttackHoldStart";
        /// <summary>
        /// Endurance used for normal range attack
        /// </summary>
        public const int RANGE_ATTACK_ENDURANCE = 5;
        /// <summary>
        /// Endurance used for critical shot
        /// </summary>
        public const int CRITICAL_SHOT_ENDURANCE = 10;

        /// <summary>
        /// Holds the cancel style flag
        /// </summary>
        protected bool m_cancelStyle;

        /// <summary>
        /// Gets or Sets the cancel style flag
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual bool CancelStyle
        {
            get { return DBCharacter != null ? DBCharacter.CancelStyle : false; }
            set { if (DBCharacter != null) DBCharacter.CancelStyle = value; }
        }
        /// <summary>
        /// Decides which style living will use in this moment
        /// </summary>
        /// <returns>Style to use or null if none</returns>
        protected override Style GetStyleToUse()
        {
            InventoryItem weapon;
            if (NextCombatStyle == null) return null;
            if (NextCombatStyle.WeaponTypeRequirement == (int)eObjectType.Shield)
                weapon = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
            else weapon = AttackWeapon;

            if (StyleProcessor.CanUseStyle(this, NextCombatStyle, weapon))
                return NextCombatStyle;

            if (NextCombatBackupStyle == null) return NextCombatStyle;

            return NextCombatBackupStyle;
        }

        /// <summary>
        /// Gets/Sets safety flag
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual bool SafetyFlag
        {
            get { return DBCharacter != null ? DBCharacter.SafetyFlag : false; }
            set { if (DBCharacter != null) DBCharacter.SafetyFlag = value; }
        }

        /// <summary>
        /// Sets/gets the living's cloak hood state
        /// (delegate to PlayerCharacter)
        /// </summary>
        public override bool IsCloakHoodUp
        {
            get { return DBCharacter != null ? DBCharacter.IsCloakHoodUp : base.IsCloakHoodUp; }
            set
            {
                //base.IsCloakHoodUp = value; // only needed if some special code will be added in base-property in future
                DBCharacter.IsCloakHoodUp = value;

                Out.SendInventoryItemsUpdate(null);
                UpdatePlayerEquipment();

                if (value)
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.IsCloakHoodUp.NowWear"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                else
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.IsCloakHoodUp.NoLongerWear"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
            }
        }

        /// <summary>
        /// Sets/gets the living's cloak visible state
        /// (delegate to PlayerCharacter)
        /// </summary>
        public override bool IsCloakInvisible
        {
            get
            {
                return DBCharacter != null ? DBCharacter.IsCloakInvisible : base.IsCloakInvisible;
            }
            set
            {
                DBCharacter.IsCloakInvisible = value;

                Out.SendInventoryItemsUpdate(null);
                UpdatePlayerEquipment();

                if (value)
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.IsCloakInvisible.Invisible"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                else
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.IsCloakInvisible.Visible"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
            }
        }

        /// <summary>
        /// Sets/gets the living's helm visible state
        /// (delegate to PlayerCharacter)
        /// </summary>
        public override bool IsHelmInvisible
        {
            get
            {
                return DBCharacter != null ? DBCharacter.IsHelmInvisible : base.IsHelmInvisible;
            }
            set
            {
                DBCharacter.IsHelmInvisible = value;

                Out.SendInventoryItemsUpdate(null);
                UpdatePlayerEquipment();

                if (value)
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.IsHelmInvisible.Invisible"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                else
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.IsHelmInvisible.Visible"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
            }
        }

        /// <summary>
        /// Gets or sets the players SpellQueue option
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual bool SpellQueue
        {
            get { return DBCharacter != null ? DBCharacter.SpellQueue : false; }
            set { if (DBCharacter != null) DBCharacter.SpellQueue = value; }
        }


        /// <summary>
        /// Switches the active weapon to another one
        /// </summary>
        /// <param name="slot">the new eActiveWeaponSlot</param>
        public override void SwitchWeapon(eActiveWeaponSlot slot)
        {
            //When switching weapons, attackmode is removed!
            if (AttackState)
                StopAttack();

            if (CurrentSpellHandler != null)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.SwitchWeapon.SpellCancelled"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                StopCurrentSpellcast();
            }

            switch (slot)
            {
                case eActiveWeaponSlot.Standard:
                    // remove endurance remove handler
                    if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
                        GameEventMgr.RemoveHandler(this, GameLivingEvent.AttackFinished, new DOLEventHandler(RangeAttackHandler));
                    break;

                case eActiveWeaponSlot.TwoHanded:
                    // remove endurance remove handler
                    if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
                        GameEventMgr.RemoveHandler(this, GameLivingEvent.AttackFinished, new DOLEventHandler(RangeAttackHandler));
                    break;

                case eActiveWeaponSlot.Distance:
                    // add endurance remove handler
                    if (ActiveWeaponSlot != eActiveWeaponSlot.Distance)
                        GameEventMgr.AddHandler(this, GameLivingEvent.AttackFinished, new DOLEventHandler(RangeAttackHandler));
                    break;
            }



            InventoryItem[] oldActiveSlots = new InventoryItem[4];
            InventoryItem[] newActiveSlots = new InventoryItem[4];
            InventoryItem rightHandSlot = Inventory.GetItem(eInventorySlot.RightHandWeapon);
            InventoryItem leftHandSlot = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
            InventoryItem twoHandSlot = Inventory.GetItem(eInventorySlot.TwoHandWeapon);
            InventoryItem distanceSlot = Inventory.GetItem(eInventorySlot.DistanceWeapon);

            // save old active weapons
            // simple active slot logic:
            // 0=right hand, 1=left hand, 2=two-hand, 3=range, F=none
            switch (VisibleActiveWeaponSlots & 0x0F)
            {
                case 0: oldActiveSlots[0] = rightHandSlot; break;
                case 2: oldActiveSlots[2] = twoHandSlot; break;
                case 3: oldActiveSlots[3] = distanceSlot; break;
            }
            if ((VisibleActiveWeaponSlots & 0xF0) == 0x10) oldActiveSlots[1] = leftHandSlot;


            base.SwitchWeapon(slot);


            // save new active slots
            switch (VisibleActiveWeaponSlots & 0x0F)
            {
                case 0: newActiveSlots[0] = rightHandSlot; break;
                case 2: newActiveSlots[2] = twoHandSlot; break;
                case 3: newActiveSlots[3] = distanceSlot; break;
            }
            if ((VisibleActiveWeaponSlots & 0xF0) == 0x10) newActiveSlots[1] = leftHandSlot;

            // unequip changed items
            for (int i = 0; i < 4; i++)
            {
                if (oldActiveSlots[i] != null && newActiveSlots[i] == null)
                    Notify(PlayerInventoryEvent.ItemUnequipped, Inventory, new ItemUnequippedArgs(oldActiveSlots[i], oldActiveSlots[i].SlotPosition));
            }

            // equip new active items
            for (int i = 0; i < 4; i++)
            {
                if (newActiveSlots[i] != null && oldActiveSlots[i] == null)
                    Notify(PlayerInventoryEvent.ItemEquipped, Inventory, new ItemEquippedArgs(newActiveSlots[i], newActiveSlots[i].SlotPosition));
            }

            if (ObjectState == eObjectState.Active)
            {
                //Send new wield info, no items updated
                Out.SendInventorySlotsUpdate(null);
                // Update active weapon appearence (has to be done with all
                // equipment in the packet else player is naked)
                UpdatePlayerEquipment();
                //Send new weapon stats
                Out.SendUpdateWeaponAndArmorStats();
            }
        }

        /// <summary>
        /// Removes ammo and endurance on range attack
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        protected virtual void RangeAttackHandler(DOLEvent e, object sender, EventArgs arguments)
        {
            AttackFinishedEventArgs args = arguments as AttackFinishedEventArgs;
            if (args == null) return;

            switch (args.AttackData.AttackResult)
            {
                case eAttackResult.HitUnstyled:
                case eAttackResult.Missed:
                case eAttackResult.Blocked:
                case eAttackResult.Parried:
                case eAttackResult.Evaded:
                case eAttackResult.HitStyle:
                case eAttackResult.Fumbled:
                    // remove an arrow and endurance
                    InventoryItem ammo = RangeAttackAmmo;
                    Inventory.RemoveCountFromStack(ammo, 1);

                    if (RangedAttackType == eRangedAttackType.Critical)
                        Endurance -= CRITICAL_SHOT_ENDURANCE;
                    else if (RangedAttackType == eRangedAttackType.RapidFire && GetAbilityLevel(Abilities.RapidFire) == 1)
                        Endurance -= 2 * RANGE_ATTACK_ENDURANCE;
                    else Endurance -= RANGE_ATTACK_ENDURANCE;
                    break;
            }
        }

        /// <summary>
        /// Starts a melee attack with this player
        /// </summary>
        /// <param name="attackTarget">the target to attack</param>
        public override void StartAttack(GameObject attackTarget)
        {
            if (CharacterClass.StartAttack(attackTarget) == false)
            {
                return;
            }

            if (!IsAlive)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.StartAttack.YouCantCombat"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                return;
            }

            // Necromancer with summoned pet cannot attack
            if (ControlledBrain != null)
                if (ControlledBrain.Body != null)
                    if (ControlledBrain.Body is NecromancerPet)
                    {
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.StartAttack.CantInShadeMode"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        return;
                    }

            if (IsStunned)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.StartAttack.CantAttackStunned"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                return;
            }
            if (IsMezzed)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.StartAttack.CantAttackmesmerized"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                return;
            }

            long vanishTimeout = TempProperties.getProperty<long>(VanishEffect.VANISH_BLOCK_ATTACK_TIME_KEY);
            if (vanishTimeout > 0 && vanishTimeout > CurrentRegion.Time)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.StartAttack.YouMustWaitAgain", ((vanishTimeout - CurrentRegion.Time + 1000) / 1000)).ToString(), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                return;
            }

            long VanishTick = this.TempProperties.getProperty<long>(VanishEffect.VANISH_BLOCK_ATTACK_TIME_KEY);
            long changeTime = this.CurrentRegion.Time - VanishTick;
            if (changeTime < 30000 && VanishTick > 0)
            {
                this.Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.StartAttack.YouMustWait", ((30000 - changeTime) / 1000).ToString()), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                return;
            }

            if (IsOnHorse)
                IsOnHorse = false;

            if (IsDisarmed)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.StartAttack.CantDisarmed"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                return;
            }

            if (IsSitting)
            {
                Sit(false);
            }



            if (AttackWeapon == null)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.StartAttack.CannotWithoutWeapon"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                return;
            }

            if (SpellHandler.FindEffectOnTarget(this, "SummonMonster") != null)
            {

                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "Skill.Ability.CannotUseDead"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                return;
            }

            if (AttackWeapon.Object_Type == (int)eObjectType.Instrument)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.StartAttack.CannotMelee"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                return;
            }
            //			if(attackTarget!=null && attackTarget is GamePlayer && ((GamePlayer)attackTarget).IsShade)
            //			{
            //				Out.SendMessage("You cannot attack shaded player!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
            //				return;
            //			}
            if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
            {
                if (ServerProperties.Properties.ALLOW_OLD_ARCHERY == false)
                {
                    if ((eCharacterClass)DBCharacter.Class == eCharacterClass.Scout || (eCharacterClass)DBCharacter.Class == eCharacterClass.Hunter || (eCharacterClass)DBCharacter.Class == eCharacterClass.Ranger)
                    {
                        // There is no feedback on live when attempting to fire a bow with arrows
                        return;
                    }
                }

                // Check arrows for ranged attack
                if (RangeAttackAmmo == null)
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.StartAttack.SelectQuiver"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                    return;
                }
                // Check if selected ammo is compatible for ranged attack
                if (!CheckRangedAmmoCompatibilityWithActiveWeapon())
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.StartAttack.CantUseQuiver"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                    return;
                }

                lock (EffectList)
                {
                    foreach (IGameEffect effect in EffectList) // switch to the correct range attack type
                    {
                        if (effect is SureShotEffect)
                        {
                            RangedAttackType = eRangedAttackType.SureShot;
                            break;
                        }

                        if (effect is RapidFireEffect)
                        {
                            RangedAttackType = eRangedAttackType.RapidFire;
                            break;
                        }

                        if (effect is TrueshotEffect)
                        {
                            RangedAttackType = eRangedAttackType.Long;
                            break;
                        }
                    }
                }

                if (RangedAttackType == eRangedAttackType.Critical && IsStealthed == false)
                {
                    Out.SendMessage("you can't fire, critical Must be fired from stealth!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                    return;
                }

                if (RangedAttackType == eRangedAttackType.Critical && Endurance < CRITICAL_SHOT_ENDURANCE)
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.StartAttack.TiredShot"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                    return;
                }

                if (Endurance < RANGE_ATTACK_ENDURANCE)
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.StartAttack.TiredUse", AttackWeapon.Name), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                    return;
                }

                if (IsStealthed)
                {
                    /*
					 * -Chance to unstealth while nocking an arrow = stealth spec / level
					 * -Chance to unstealth nocking a crit = stealth / level  0.20
					 */
                    int stealthSpec = GetModifiedSpecLevel(Specs.Stealth);
                    int stayStealthed = stealthSpec * 100 / Level;
                    if (RangedAttackType == eRangedAttackType.Critical)
                        stayStealthed -= 20;

                    if (!Util.Chance(stayStealthed))
                        Stealth(false);
                }
            }
            else
            {
                if (attackTarget == null)
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.StartAttack.CombatNoTarget"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                else
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.StartAttack.CombatTarget", attackTarget.GetName(0, false)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
            }
            // vampiir
            if (CharacterClass is PlayerClass.ClassVampiir)
            {
                GameSpellEffect removeEffect = SpellHandler.FindEffectOnTarget(this, "VampiirSpeedEnhancement");
                if (removeEffect != null)
                    removeEffect.Cancel(false);
            }
            else
            {
                // Bard RR5 ability must drop when the player starts a melee attack
                IGameEffect DreamweaverRR5 = EffectList.GetOfType<DreamweaverEffect>();
                if (DreamweaverRR5 != null)
                    DreamweaverRR5.Cancel(false);
            }


            if (IsCasting && !m_runningSpellHandler.Spell.Uninterruptible)
            {
                StopCurrentSpellcast();
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.StartAttack.SpellCancelled"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
            }

            base.StartAttack(attackTarget);


            //Clear styles
            NextCombatStyle = null;
            NextCombatBackupStyle = null;

            if (ActiveWeaponSlot != eActiveWeaponSlot.Distance)
            {
                Out.SendAttackMode(AttackState);
            }
            else
            {
                TempProperties.setProperty(RANGE_ATTACK_HOLD_START, 0L);

                string typeMsg = "shot";
                if (AttackWeapon.Object_Type == (int)eObjectType.Thrown)
                    typeMsg = "throw";

                string targetMsg = String.Empty;
                if (attackTarget != null)
                {
                    if (this.IsWithinRadius(attackTarget, AttackRange))
                        targetMsg = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.StartAttack.TargetInRange");
                    else
                        targetMsg = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.StartAttack.TargetOutOfRange");
                }

                int speed = AttackSpeed(AttackWeapon) / 100;
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.StartAttack.YouPrepare", typeMsg, speed / 10, speed % 10, targetMsg), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
            }
        }

        /// <summary>
        /// Stops all attacks this player is making
        /// </summary>
        /// <param name="forced">Is this a forced stop or is the client suggesting we stop?</param>
        public override void StopAttack(bool forced)
        {
            NextCombatStyle = null;
            NextCombatBackupStyle = null;
            base.StopAttack(forced);
            if (IsAlive)
            {
                Out.SendAttackMode(AttackState);
            }
        }


        /// <summary>
        /// Switches the active quiver slot to another one
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="forced"></param>
        public virtual void SwitchQuiver(eActiveQuiverSlot slot, bool forced)
        {
            if (slot != eActiveQuiverSlot.None)
            {
                eInventorySlot updatedSlot = eInventorySlot.Invalid;
                if ((slot & eActiveQuiverSlot.Fourth) > 0)
                    updatedSlot = eInventorySlot.FourthQuiver;
                else if ((slot & eActiveQuiverSlot.Third) > 0)
                    updatedSlot = eInventorySlot.ThirdQuiver;
                else if ((slot & eActiveQuiverSlot.Second) > 0)
                    updatedSlot = eInventorySlot.SecondQuiver;
                else if ((slot & eActiveQuiverSlot.First) > 0)
                    updatedSlot = eInventorySlot.FirstQuiver;

                if (Inventory.GetItem(updatedSlot) != null && (ActiveQuiverSlot != slot || forced))
                {
                    ActiveQuiverSlot = slot;
                    //GamePlayer.SwitchQuiver.ShootWith:		You will shoot with: {0}.
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.SwitchQuiver.ShootWith", Inventory.GetItem(updatedSlot).GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                else
                {
                    ActiveQuiverSlot = eActiveQuiverSlot.None;
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.SwitchQuiver.NoMoreAmmo"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }

                Out.SendInventorySlotsUpdate(new int[] { (int)updatedSlot });
            }
            else
            {
                if (Inventory.GetItem(eInventorySlot.FirstQuiver) != null)
                    SwitchQuiver(eActiveQuiverSlot.First, true);
                else if (Inventory.GetItem(eInventorySlot.SecondQuiver) != null)
                    SwitchQuiver(eActiveQuiverSlot.Second, true);
                else if (Inventory.GetItem(eInventorySlot.ThirdQuiver) != null)
                    SwitchQuiver(eActiveQuiverSlot.Third, true);
                else if (Inventory.GetItem(eInventorySlot.FourthQuiver) != null)
                    SwitchQuiver(eActiveQuiverSlot.Fourth, true);
                else
                {
                    ActiveQuiverSlot = eActiveQuiverSlot.None;
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.SwitchQuiver.NotUseQuiver"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                Out.SendInventorySlotsUpdate(null);
            }
        }

       
        /// <summary>
        /// Check the selected range ammo and decides if it's compatible with select weapon
        /// </summary>
        /// <returns>True if compatible, false if not</returns>
        protected virtual bool CheckRangedAmmoCompatibilityWithActiveWeapon()
        {
            InventoryItem weapon = AttackWeapon;
            if (weapon != null)
            {
                switch ((eObjectType)weapon.Object_Type)
                {
                    case eObjectType.Crossbow:
                    case eObjectType.Longbow:
                    case eObjectType.CompositeBow:
                    case eObjectType.RecurvedBow:
                    case eObjectType.Fired:
                        {
                            if (ActiveQuiverSlot != eActiveQuiverSlot.None)
                            {
                                InventoryItem ammo = null;
                                switch (ActiveQuiverSlot)
                                {
                                    case eActiveQuiverSlot.Fourth: ammo = Inventory.GetItem(eInventorySlot.FourthQuiver); break;
                                    case eActiveQuiverSlot.Third: ammo = Inventory.GetItem(eInventorySlot.ThirdQuiver); break;
                                    case eActiveQuiverSlot.Second: ammo = Inventory.GetItem(eInventorySlot.SecondQuiver); break;
                                    case eActiveQuiverSlot.First: ammo = Inventory.GetItem(eInventorySlot.FirstQuiver); break;
                                }

                                if (ammo == null) return false;

                                if (weapon.Object_Type == (int)eObjectType.Crossbow)
                                    return ammo.Object_Type == (int)eObjectType.Bolt;
                                return ammo.Object_Type == (int)eObjectType.Arrow;
                            }
                        }
                        break;
                }
            }
            return true;
        }

        /// <summary>
        /// Holds the arrows for next range attack
        /// </summary>
        protected WeakReference m_rangeAttackAmmo;

        /// <summary>
        /// Gets/Sets the item that is used for ranged attack
        /// </summary>
        /// <returns>Item that will be used for range/accuracy/damage modifications</returns>
        protected override InventoryItem RangeAttackAmmo
        {
            get
            {
                //TODO: ammo should be saved on start of every range attack and used here
                InventoryItem ammo = null;//(InventoryItem)m_rangeAttackArrows.Target;

                InventoryItem weapon = AttackWeapon;
                if (weapon != null)
                {
                    switch (weapon.Object_Type)
                    {
                        case (int)eObjectType.Thrown: ammo = Inventory.GetItem(eInventorySlot.DistanceWeapon); break;
                        case (int)eObjectType.Crossbow:
                        case (int)eObjectType.Longbow:
                        case (int)eObjectType.CompositeBow:
                        case (int)eObjectType.RecurvedBow:
                        case (int)eObjectType.Fired:
                            {
                                switch (ActiveQuiverSlot)
                                {
                                    case eActiveQuiverSlot.First: ammo = Inventory.GetItem(eInventorySlot.FirstQuiver); break;
                                    case eActiveQuiverSlot.Second: ammo = Inventory.GetItem(eInventorySlot.SecondQuiver); break;
                                    case eActiveQuiverSlot.Third: ammo = Inventory.GetItem(eInventorySlot.ThirdQuiver); break;
                                    case eActiveQuiverSlot.Fourth: ammo = Inventory.GetItem(eInventorySlot.FourthQuiver); break;
                                    case eActiveQuiverSlot.None:
                                        eObjectType findType = eObjectType.Arrow;
                                        if (weapon.Object_Type == (int)eObjectType.Crossbow)
                                            findType = eObjectType.Bolt;

                                        ammo = Inventory.GetFirstItemByObjectType((int)findType, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);

                                        break;
                                }
                            }
                            break;
                    }
                }

                return ammo;
            }
            set { m_rangeAttackAmmo.Target = value; }
        }

        /// <summary>
        /// Holds the target for next range attack
        /// </summary>
        protected WeakReference m_rangeAttackTarget;

        /// <summary>
        /// Gets/Sets the target for current ranged attack
        /// </summary>
        /// <returns></returns>
        protected override GameObject RangeAttackTarget
        {
            get
            {
                GameObject target = (GameObject)m_rangeAttackTarget.Target;
                if (target == null || target.ObjectState != eObjectState.Active)
                    target = TargetObject;
                return target;
            }
            set { m_rangeAttackTarget.Target = value; }
        }

        /// <summary>
        /// Check the range attack state and decides what to do
        /// Called inside the AttackTimerCallback
        /// </summary>
        /// <returns></returns>
        protected override eCheckRangeAttackStateResult CheckRangeAttackState(GameObject target)
        {
            long holdStart = TempProperties.getProperty<long>(RANGE_ATTACK_HOLD_START);
            if (holdStart == 0)
            {
                holdStart = CurrentRegion.Time;
                TempProperties.setProperty(RANGE_ATTACK_HOLD_START, holdStart);
            }
            //DOLConsole.WriteLine("Holding.... ("+holdStart+") "+(Environment.TickCount - holdStart));
            if ((CurrentRegion.Time - holdStart) > 15000 && AttackWeapon.Object_Type != (int)eObjectType.Crossbow)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.TooTired"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return eCheckRangeAttackStateResult.Stop; //Stop the attack
            }

            //This state is set when the player wants to fire!
            if (RangedAttackState == eRangedAttackState.Fire
                || RangedAttackState == eRangedAttackState.AimFire
                || RangedAttackState == eRangedAttackState.AimFireReload)
            {
                RangeAttackTarget = null; // clean the RangeAttackTarget at the first shot try even if failed

                if (target == null || !(target is GameLiving))
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "System.MustSelectTarget"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                else if (!this.IsWithinRadius(target, AttackRange))
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.TooFarAway", target.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                else if (!TargetInView)  // TODO : wrong, must be checked with the target parameter and not with the targetObject
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.CantSeeTarget"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                else if (!IsObjectInFront(target, 90))
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.NotInView", target.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                else if (RangeAttackAmmo == null)
                {
                    //another check for ammo just before firing
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.MustSelectQuiver"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                }
                else if (!CheckRangedAmmoCompatibilityWithActiveWeapon())
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.CantUseQuiver"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                }
                else if (GameServer.ServerRules.IsAllowedToAttack(this, (GameLiving)target, false))
                {
                    GameLiving living = target as GameLiving;
                    if (RangedAttackType == eRangedAttackType.Critical && living != null
                        && (living.CurrentSpeed > 90 //walk speed == 85, hope that's what they mean
                            || (living.AttackState && living.InCombat) //maybe not 100% correct
                            || SpellHandler.FindEffectOnTarget(living, "Mesmerize") != null
                            || SpellHandler.FindEffectOnTarget(living, "AOEMesmerize") != null
                           ))
                    {
                        /*
						 * http://rothwellhome.org/guides/archery.htm
						 * Please note that critical shot will work against targets that are:
						 * sitting, standing still (which includes standing in combat mode but
						 * not actively swinging at something), walking, moving backwards,
						 * strafing, or casting a spell. Critical shot will not work against
						 * targets that are: running, in active combat (swinging at something),
						 * or mezzed. Stunned targets may be critical shot once any timers from
						 * active combat have expired if they are not yet free to act; i.e.:
						 * they may not be critical shot until their weapon delay timer has run
						 * out after their last attack, they may be critical shot during the
						 * period between the weapon delay running out and the stun wearing off,
						 * and they may not be critical shot once they have begun swinging again.
						 * If the target was in melee with an archer, the critical shot may not
						 * be drawn against them until after their weapon delay has run out or it
						 * will be interrupted.  This means that the scout's shield stun is much
						 * less effective against large weapon wielders (who have longer weapon
						 * delays) than against fast piercing/thrusting weapon wielders.
						 */

                        // TODO: more checks?
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.CantCritical"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        RangedAttackType = eRangedAttackType.Normal;
                    }
                    return eCheckRangeAttackStateResult.Fire;
                }

                RangedAttackState = eRangedAttackState.ReadyToFire;
                return eCheckRangeAttackStateResult.Hold;
            }

            //Player is aiming
            if (RangedAttackState == eRangedAttackState.Aim)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.ReadyToFire"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                RangedAttackState = eRangedAttackState.ReadyToFire;
                return eCheckRangeAttackStateResult.Hold;
            }
            else if (RangedAttackState == eRangedAttackState.ReadyToFire)
            {
                return eCheckRangeAttackStateResult.Hold; //Hold the shot
            }
            return eCheckRangeAttackStateResult.Fire;
        }

        /// <summary>
		/// Send the messages to the GamePlayer
		/// </summary>
		/// <param name="ad"></param>
		protected override void SendAttackingCombatMessages(AttackData ad)
        {
            base.SendAttackingCombatMessages(ad);
            GameObject target = ad.Target;
            InventoryItem weapon = ad.Weapon;
            if (ad.Target is GameNPC)
            {

                switch (ad.AttackResult)
                {
                    case eAttackResult.TargetNotVisible:
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.NotInView",
ad.Target.GetName(0, true, Client.Account.Language, (ad.Target as GameNPC))), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                    case eAttackResult.OutOfRange:
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.TooFarAway",
ad.Target.GetName(0, true, Client.Account.Language, (ad.Target as GameNPC))), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                    case eAttackResult.TargetDead:
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.AlreadyDead",
ad.Target.GetName(0, true, Client.Account.Language, (ad.Target as GameNPC))), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                    case eAttackResult.Blocked:
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Blocked",
ad.Target.GetName(0, true, Client.Account.Language, (ad.Target as GameNPC))), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                    case eAttackResult.Parried:
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Parried",
ad.Target.GetName(0, true, Client.Account.Language, (ad.Target as GameNPC))), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                    case eAttackResult.Evaded:
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Evaded",
 ad.Target.GetName(0, true, Client.Account.Language, (ad.Target as GameNPC))), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                    case eAttackResult.NoTarget: Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.NeedTarget"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                    case eAttackResult.NoValidTarget: Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.CantBeAttacked"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                    case eAttackResult.Missed: Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Miss"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                    case eAttackResult.Fumbled: Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Fumble"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                    case eAttackResult.HitStyle:
                    case eAttackResult.HitUnstyled:


                        //calculate resist and absorb
                        ad.Damage = DamageAndResistCalculator(ad);

                        string modmessage = String.Empty;
                        if (ad.Modifier > 0) modmessage = " (+" + ad.Modifier.ToString() + ")";
                        if (ad.Modifier < 0) modmessage = " (" + ad.Modifier.ToString() + ")";

                        string hitWeapon = String.Empty;

                        switch (Client.Account.Language)
                        {
                            case "DE":
                                if (weapon != null)
                                    hitWeapon = weapon.Name;
                                break;
                            default:
                                if (weapon != null)
                                    hitWeapon = GlobalConstants.NameToShortName(weapon.Name);
                                break;
                        }

                        if (hitWeapon.Length > 0)
                            hitWeapon = " " + LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.WithYour") + " " + hitWeapon;

                        string attackTypeMsg = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.YouAttack");
                        if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
                            attackTypeMsg = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.YouShot");

                        // intercept messages
                        if (target != null && target != ad.Target)
                        {
                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Intercepted", ad.Target.GetName(0, true), target.GetName(0, false)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.InterceptedHit", attackTypeMsg, target.GetName(0, false), hitWeapon, ad.Target.GetName(0, false), ad.Damage.ToString(), modmessage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        }
                        else
                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.InterceptHit", attackTypeMsg,
                                ad.Target.GetName(0, false, Client.Account.Language, (ad.Target as GameNPC)), hitWeapon, ad.Damage.ToString(), modmessage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

                        // critical hit
                        if (ad.CriticalDamage > 0)
                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Critical",
                                ad.Target.GetName(0, false, Client.Account.Language, (ad.Target as GameNPC)), ad.CriticalDamage.ToString()), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        break;
                }
            }
            else
            {
                switch (ad.AttackResult)
                {
                    case eAttackResult.TargetNotVisible: Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.NotInView", ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                    case eAttackResult.OutOfRange: Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.TooFarAway", ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                    case eAttackResult.TargetDead: Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.AlreadyDead", ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                    case eAttackResult.Blocked: Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Blocked", ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                    case eAttackResult.Parried: Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Parried", ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                    case eAttackResult.Evaded: Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Evaded", ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                    case eAttackResult.NoTarget: Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.NeedTarget"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                    case eAttackResult.NoValidTarget: Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.CantBeAttacked"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                    case eAttackResult.Missed: Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Miss"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                    case eAttackResult.Fumbled: Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Fumble"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                    case eAttackResult.HitStyle:
                    case eAttackResult.HitUnstyled:



                        string modmessage = String.Empty;
                        if (ad.Modifier > 0) modmessage = " (+" + ad.Modifier.ToString() + ")";
                        if (ad.Modifier < 0) modmessage = " (" + ad.Modifier.ToString() + ")";

                        string hitWeapon = String.Empty;

                        switch (Client.Account.Language)
                        {
                            case "DE":
                                if (weapon != null)
                                    hitWeapon = weapon.Name;
                                break;
                            default:
                                if (weapon != null)
                                    hitWeapon = GlobalConstants.NameToShortName(weapon.Name);
                                break;
                        }

                        if (hitWeapon.Length > 0)
                            hitWeapon = " " + LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.WithYour") + " " + hitWeapon;

                        string attackTypeMsg = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.YouAttack");
                        if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
                            attackTypeMsg = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.YouShot");

                        // intercept messages
                        if (target != null && target != ad.Target)
                        {
                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Intercepted", ad.Target.GetName(0, true), target.GetName(0, false)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.InterceptedHit", attackTypeMsg, target.GetName(0, false), hitWeapon, ad.Target.GetName(0, false), ad.Damage.ToString(), modmessage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        }
                        else
                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.InterceptHit", attackTypeMsg, ad.Target.GetName(0, false), hitWeapon, ad.Damage.ToString(), modmessage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

                        // critical hit
                        if (ad.CriticalDamage > 0)
                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Critical", ad.Target.GetName(0, false), ad.CriticalDamage.ToString()), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        break;
                }
            }
        }

        /// <summary>
        /// Called whenever a single attack strike is made
        /// </summary>
        /// <param name="target"></param>
        /// <param name="weapon"></param>
        /// <param name="style"></param>
        /// <param name="effectiveness"></param>
        /// <param name="interruptDuration"></param>
        /// <param name="dualWield"></param>
        /// <returns></returns>
        protected override AttackData MakeAttack(GameObject target, InventoryItem weapon, Style style, double effectiveness, int interruptDuration, bool dualWield)
        {
            if (IsCrafting)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.InterruptedCrafting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                CraftTimer.Stop();
                CraftTimer = null;
                Out.SendCloseTimerWindow();
            }
           
            AttackData ad = base.MakeAttack(target, weapon, style, effectiveness * Effectiveness, interruptDuration, dualWield);

            //Clear the styles for the next round!
            NextCombatStyle = null;
            NextCombatBackupStyle = null;

            switch (ad.AttackResult)
            {
                case eAttackResult.HitStyle:
                case eAttackResult.HitUnstyled:
                    {
                        //keep component
                        if (ad != null && (ad.Target != null && ad.Target is GameKeepComponent || ad.Target is GameKeepDoor || ad.Target is GameSiegeWeapon) && ad.Attacker is GamePlayer && ad.Attacker.GetModified(eProperty.KeepDamage) > 0)
                        {
                            int keepdamage = (int)Math.Floor((double)ad.Damage * ((double)ad.Attacker.GetModified(eProperty.KeepDamage) / 100));
                            int keepstyle = (int)Math.Floor((double)ad.StyleDamage * ((double)ad.Attacker.GetModified(eProperty.KeepDamage) / 100));
                            ad.Damage += keepdamage;
                            ad.StyleDamage += keepstyle;
                        }

                        // vampiir Style Mana generating
                        if (CharacterClass is PlayerClass.ClassVampiir
                            && ad.AttackType == AttackData.eAttackType.MeleeOneHand && (ad.AttackResult == eAttackResult.HitStyle || ad.AttackResult == eAttackResult.HitUnstyled)
                            && target is GameKeepComponent == false
                            && target is GameKeepDoor == false
                            && target is GameSiegeWeapon == false
                            && target is GameMine == false
                             && target is GameFont == false
                             && target is GameStorm == false
                             && target is GameSiegeWeapon == false)
                        {
                            int perc = Convert.ToInt32(((double)(ad.Damage + ad.CriticalDamage) / 100) * (51 - (this.TargetObject.Level / 1.08)));
                            perc = (perc < 2) ? 2 : ((perc > 9) ? 9 : perc);
                            this.Mana += Convert.ToInt32(Math.Ceiling(((Decimal)(perc * this.MaxMana) / 100)));
                        }

                        //only miss when strafing when attacking a player
                        //30% chance to miss
                        if (IsStrafing && ad.Target is GamePlayer && Util.Chance(60))
                        {
                            ad.AttackResult = eAttackResult.Missed;
                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.StrafMiss"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                            break;
                        }


                        if (ad.Target is GuardLord)
                        {
                            GuardLord gurad = ad.Target as GuardLord;

                            if (IsWithinRadius(gurad, 127, true) == false || IsWithinRadius(gurad, 127, true) == false)
                            {
                                /*
                                if (gurad.Component != null && gurad.Component.Keep != null && gurad.Component.Keep is GameKeepTower)
                                {
                                    GameKeepComponent component = gurad.Component as GameKeepComponent;

                                    if (component.IsRaized)
                                    {
                                       // rized tower
                                        log.Error("component ist zerstört!!");
                                    }
                                }
                                */
                                Out.SendMessage(ad.Target.Name + " is immune to damage from this range", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                                Out.SendMessage(" You has to be in melee range of the Lord to attack him.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                                ad.AttackResult = eAttackResult.Missed;
                                break;
                            }
                        }
                        break;
                    }
            }

            switch (ad.AttackResult)
            {
                case eAttackResult.Blocked:
                case eAttackResult.Fumbled:
                case eAttackResult.HitStyle:
                case eAttackResult.HitUnstyled:
                case eAttackResult.Missed:
                case eAttackResult.Parried:
                    //Condition percent can reach 70%
                    //durability percent can reach zero
                    // if item durability reachs 0, item is useless and become broken item

                    if (weapon != null && weapon is GameInventoryItem)
                    {
                        (weapon as GameInventoryItem).OnStrikeTarget(this, target);
                    }
                    //Camouflage
                    if (target is GamePlayer && HasAbility(Abilities.Camouflage))
                    {
                        CamouflageEffect camouflage = EffectList.GetOfType<CamouflageEffect>();
                        if (camouflage != null)
                        {
                            DisableSkill(SkillBase.GetAbility(Abilities.Camouflage), CamouflageSpecHandler.DISABLE_DURATION);
                            camouflage.Cancel(false);
                        }
                    }

                    // Multiple Hit check
                    if (ad.AttackResult == eAttackResult.HitStyle)
                    {
                        byte numTargetsCanHit;
                        int random;
                        IList extraTargets = new ArrayList();
                        IList listAvailableTargets = new ArrayList();
                        InventoryItem attackWeapon = AttackWeapon;
                        InventoryItem leftWeapon = Inventory?.GetItem(eInventorySlot.LeftHandWeapon);
                        switch (style.ID)
                        {
                            case 374: numTargetsCanHit = 1; break; //Tribal Assault:   Hits 2 targets
                            case 377: numTargetsCanHit = 1; break; //Clan's Might:      Hits 2 targets
                            case 379: numTargetsCanHit = 2; break; //Totemic Wrath:      Hits 3 targets
                            case 384: numTargetsCanHit = 3; break; //Totemic Sacrifice:   Hits 4 targets
                            case 600: numTargetsCanHit = 255; break; //Shield Swipe: No Cap on Targets
                            default: numTargetsCanHit = 0; break; //For others;
                        }
                        if (numTargetsCanHit > 0)
                        {
                            if (style.ID != 600) // Not Shield Swipe
                            {
                                foreach (GamePlayer pl in GetPlayersInRadius(false, (ushort)AttackRange))
                                {
                                    if (pl == null) continue;
                                    if (GameServer.ServerRules.IsAllowedToAttack(this, pl, true))
                                    {
                                        listAvailableTargets.Add(pl);
                                    }
                                }
                                foreach (GameNPC npc in GetNPCsInRadius(false, (ushort)AttackRange))
                                {
                                    if (GameServer.ServerRules.IsAllowedToAttack(this, npc, true))
                                    {
                                        listAvailableTargets.Add(npc);
                                    }
                                }

                                // remove primary target
                                listAvailableTargets.Remove(target);
                                numTargetsCanHit = (byte)Math.Min(numTargetsCanHit, listAvailableTargets.Count);

                                if (listAvailableTargets.Count > 1)
                                {
                                    while (extraTargets.Count < numTargetsCanHit)
                                    {
                                        random = Util.Random(listAvailableTargets.Count - 1);
                                        if (!extraTargets.Contains(listAvailableTargets[random]))
                                            extraTargets.Add(listAvailableTargets[random] as GameObject);
                                    }
                                    foreach (GameObject obj in extraTargets)
                                    {
                                        if (obj is GamePlayer && ((GamePlayer)obj).IsSitting)
                                        {
                                            effectiveness *= 2;
                                        }
                                        new WeaponOnTargetAction(this, obj as GameObject, attackWeapon, leftWeapon, effectiveness, AttackSpeed(attackWeapon), null).Start(1);  // really start the attack
                                    }
                                }
                            }
                            else // shield swipe
                            {
                                foreach (GameNPC npc in GetNPCsInRadius(false, (ushort)AttackRange))
                                {
                                    if (GameServer.ServerRules.IsAllowedToAttack(this, npc, true))
                                    {
                                        listAvailableTargets.Add(npc);
                                    }
                                }

                                listAvailableTargets.Remove(target);
                                numTargetsCanHit = (byte)Math.Min(numTargetsCanHit, listAvailableTargets.Count);

                                if (listAvailableTargets.Count > 1)
                                {
                                    while (extraTargets.Count < numTargetsCanHit)
                                    {
                                        random = Util.Random(listAvailableTargets.Count - 1);
                                        if (!extraTargets.Contains(listAvailableTargets[random]))
                                        {
                                            extraTargets.Add(listAvailableTargets[random] as GameObject);
                                        }
                                    }
                                    foreach (GameNPC obj in extraTargets)
                                    {
                                        if (obj != ad.Target)
                                        {
                                            this.MakeAttack(obj, attackWeapon, null, 1, ServerProperties.Properties.SPELL_INTERRUPT_DURATION, false, false);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
            return ad;
        }


        /// <summary>
        /// Try and execute a weapon style
        /// </summary>
        /// <param name="style"></param>
        public virtual void ExecuteWeaponStyle(Style style)
        {
            StyleProcessor.TryToUseStyle(this, style);
        }

        /// <summary>
        /// Calculates melee critical damage of this player
        /// </summary>
        /// <param name="ad">The attack data</param>
        /// <param name="weapon">The weapon used</param>
        /// <returns>The amount of critical damage</returns>
        public override int GetMeleeCriticalDamage(AttackData ad, InventoryItem weapon)
        {
            if (Util.Chance(AttackCriticalChance(weapon)))
            {
                // triple wield prevents critical hits
                if (ad.Target.EffectList.GetOfType<TripleWieldEffect>() != null) return 0;

                int critMin;
                int critMax;
                int CritMaxValue;
               


                BerserkEffect berserk = EffectList.GetOfType<BerserkEffect>();

                if (berserk != null)
                {
                    
                    int level = GetAbilityLevel(Abilities.Berserk);
                    // According to : http://daoc.catacombs.com/forum.cfm?ThreadKey=10833&DefMessage=922046&forum=37
                    // Zerk 1 = 1-25%
                    // Zerk 2 = 1-50%
                    // Zerk 3 = 1-75%
                    // Zerk 4 = 1-99%
                    //critMin = (int)(0.01 * ad.Damage);

                    
                    switch(level)
                    {
                        case 1:
                            {
                                CritMaxValue = 45;
                                break;
                            }
                        case 2:
                            {
                                CritMaxValue = 55;
                                break;
                            }
                        case 3:
                            {
                                CritMaxValue = 65;
                                break;
                            }
                        case 4:
                            {
                                CritMaxValue = 75;
                                break;
                            }
                        default:
                            {
                                CritMaxValue = 50;
                            }
                            break;
                    }
                   
                    critMin = ad.Damage * 20 / 100;

                    //critMax = (int)(Math.Min(0.99, (level * 0.25)) * ad.Damage);
                    critMax = ad.Damage * CritMaxValue / 100;
                    //log.ErrorFormat("critMin {0} critMax {1} !!", critMin, critMax);
                }
                else
                {
                    //think min crit dmage is 10% of damage
                    critMin = ad.Damage * 20 / 100;
                    // Critical damage to players is 50%, low limit should be around 20% but not sure
                    // zerkers in Berserk do up to 99%
                    if (ad.Target is GamePlayer)
                        critMax = ad.Damage * 50 / 100;
                    else
                        critMax = ad.Damage * 75 / 100;
                }
                critMin = Math.Max(critMin, 0);
                critMax = Math.Max(critMin, critMax);

                return Util.Random(critMin, critMax);
            }
            return 0;
        }

        /// <summary>
        /// This method is called at the end of the attack sequence to
        /// notify objects if they have been attacked/hit by an attack
        /// </summary>
        /// <param name="ad">information about the attack</param>
        public override void OnAttackedByEnemy(AttackData ad)
        {
            if (IsOnHorse && ad.IsHit)
                IsOnHorse = false;
            base.OnAttackedByEnemy(ad);
            GameLiving Petattacker = null;
            int SourceDamage = 0;
            //calculate resist and absorb
            ad.Damage = DamageAndResistCalculator(ad);


            switch (ad.AttackResult)
            {
                // is done in game living because of guard
                //case eAttackResult.Blocked : Out.SendMessage(ad.Attacker.GetName(0, true) + " attacks you and you block the blow!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow); break;
                case eAttackResult.Parried:
                    if (ad.Attacker is GameNPC)
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Parry", ad.Attacker.GetName(0, true, Client.Account.Language, (ad.Attacker as GameNPC))), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                    else
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Parry", ad.Attacker.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                    break;
                case eAttackResult.Evaded:
                    if (ad.Attacker is GameNPC)
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Evade", ad.Attacker.GetName(0, true, Client.Account.Language, (ad.Attacker as GameNPC))), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                    else
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Evade", ad.Attacker.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                    break;
                case eAttackResult.Fumbled:
                    if (ad.Attacker is GameNPC)
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Fumbled", ad.Attacker.GetName(0, true, Client.Account.Language, (ad.Attacker as GameNPC))), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                    else
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Fumbled", ad.Attacker.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                    break;
                case eAttackResult.Missed:
                    if (ad.AttackType == AttackData.eAttackType.Spell)
                        break;
                    if (ad.Attacker is GameNPC)
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Missed", ad.Attacker.GetName(0, true, Client.Account.Language, (ad.Attacker as GameNPC))), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                    else
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Missed", ad.Attacker.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                    break;
                case eAttackResult.HitStyle:
                case eAttackResult.HitUnstyled:
                    {
                        if (ad.Damage == -1)
                            break;

                        #region Messages

                        string hitLocName = null;
                        switch (ad.ArmorHitLocation)
                        {
                            //GamePlayer.Attack.Location.Feet:	feet
                            // LanguageMgr.GetTranslation(Client.Account.Language, "", ad.Attacker.GetName(0, true))
                            case eArmorSlot.TORSO: hitLocName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Location.Torso"); break;
                            case eArmorSlot.ARMS: hitLocName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Location.Arm"); break;
                            case eArmorSlot.HEAD: hitLocName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Location.Head"); break;
                            case eArmorSlot.LEGS: hitLocName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Location.Leg"); break;
                            case eArmorSlot.HAND: hitLocName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Location.Hand"); break;
                            case eArmorSlot.FEET: hitLocName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Location.Foot"); break;
                        }
                        string modmessage = String.Empty;
                        if (ad.Attacker is GamePlayer == false && GetPetAttacker(ad) == null) // if attacked by player, don't show resists (?) //no show for all
                        {
                            if (ad.Modifier > 0) modmessage = " (+" + ad.Modifier.ToString() + ")";
                            if (ad.Modifier < 0) modmessage = " (" + ad.Modifier.ToString() + ")";
                        }

                        //log.ErrorFormat("damage modifier = {0}", ad.Modifier);



                        if (ad.Attacker is GameNPC && GetPetAttacker(ad) == null)
                        {

                            if (hitLocName != null)
                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.HitsYour",
                                    ad.Attacker.GetName(0, true, Client.Account.Language, (ad.Attacker as GameNPC)), hitLocName, ad.Damage.ToString(), modmessage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                            else
                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.HitsYou",
                                    ad.Attacker.IsAlive ? ad.Attacker.GetName(0, true, Client.Account.Language, (ad.Attacker as GameNPC)) : "A dead enemy", ad.Damage.ToString(), modmessage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);

                            if (ad.CriticalDamage > 0)
                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.HitsYouCritical",
                                    ad.Attacker.GetName(0, true, Client.Account.Language, (ad.Attacker as GameNPC)), ad.CriticalDamage.ToString()), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                        }
                        else if (ad.Attacker is GameNPC && GetPetAttacker(ad) != null && ad.Target is GamePlayer && ad.IsMeleeAttack == false)//Spell damage Message pet vs Player
                        {

                            if (hitLocName != null)
                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.HitsYour",
                                    ad.Attacker.GetName(0, true, Client.Account.Language, (ad.Attacker as GameNPC)), hitLocName, ad.Damage.ToString(), modmessage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                            else
                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.HitsYou",
                                    ad.Attacker.IsAlive ? ad.Attacker.GetName(0, true, Client.Account.Language, (ad.Attacker as GameNPC)) : "A dead enemy", ad.Damage.ToString(), modmessage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);

                            if (ad.CriticalDamage > 0)
                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.HitsYouCritical",
                                    ad.Attacker.GetName(0, true, Client.Account.Language, (ad.Attacker as GameNPC)), ad.CriticalDamage.ToString()), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                        }
                        else if (ad.Attacker is GameNPC && GetPetAttacker(ad) != null && ad.IsMeleeAttack)
                        {

                            //calculate resist and absorb
                            //ad.Damage = DamageAndResistCalculator(ad);

                            GameLiving player;
                            player = GetPetAttacker(ad);

                            if (player != null && player is GamePlayer)
                            {
                                Petattacker = player;
                                SourceDamage = ad.Damage;
                                if (hitLocName != null)
                                    if (ad.Modifier < 0)
                                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.HitsYour", player.GetName(0, true), hitLocName, ad.Damage.ToString(), modmessage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                                    else
                                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.HitsYour", player.GetName(0, true), hitLocName, ad.Damage.ToString(), modmessage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                                else
                                {
                                    if (ad.Modifier < 0)
                                    {
                                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.HitsYou", player.IsAlive ? ad.Attacker.GetName(0, true) : "A dead enemy", ad.Damage.ToString(), modmessage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                                    }
                                    else
                                    {
                                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.HitsYou", player.IsAlive ? ad.Attacker.GetName(0, true) : "A dead enemy", ad.Damage.ToString(), modmessage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                                    }
                                }
                                if (ad.CriticalDamage > 0)
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.HitsYouCritical", player.GetName(0, true), ad.CriticalDamage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                            }
                        }
                        else if (ad.Attacker is GamePlayer)
                        {
                            //log.ErrorFormat("Wird getroffen mit {0} Modivier+- {1}", ad.Damage, ad.Modifier);
                            if (hitLocName != null)

                                if (ad.Modifier < 0)
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.HitsYour", ad.Attacker.GetName(0, true), hitLocName, ad.Damage.ToString(), modmessage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                                else
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.HitsYour", ad.Attacker.GetName(0, true), hitLocName, ad.Damage.ToString(), modmessage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                            else
                            {
                                if (ad.Modifier < 0)
                                {
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.HitsYou", ad.Attacker.IsAlive ? ad.Attacker.GetName(0, true) : "A dead enemy", ad.Damage, modmessage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                                }
                                else
                                {
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.HitsYou", ad.Attacker.IsAlive ? ad.Attacker.GetName(0, true) : "A dead enemy", ad.Damage.ToString(), modmessage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                                }
                            }
                            if (ad.CriticalDamage > 0)
                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.HitsYouCritical", ad.Attacker.GetName(0, true), ad.CriticalDamage.ToString()), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                        }
                        //Pet vs Player neu
                        if (ad.Attacker != null && Petattacker != null && ad.Target.TempProperties.getProperty<int>(PetDamageFromAttacker) > 0)
                        {
                            int mod = 0;
                            int critdamage = 0;
                            int damage = ad.Target.TempProperties.getProperty<int>(PetDamageFromAttacker);
                            mod = damage - SourceDamage;
                            damage -= mod; //Real Damage,  mod = abzug

                            string modmessage2 = "";
                            if (mod > 0) modmessage2 = " (-" + mod.ToString() + ")";
                            if (mod < 0) modmessage2 = " (+" + mod.ToString() + ")";

                            if (ad.Target.TempProperties.getProperty<int>(PetCritDamageFromAttacker) > 0)
                            {
                                critdamage = ad.Target.TempProperties.getProperty<int>(PetCritDamageFromAttacker);
                            }


                            string attackTypeMsg = "attacks";

                            if (ad.Attacker.ActiveWeaponSlot == eActiveWeaponSlot.Distance)
                            {
                                attackTypeMsg = "shoots";
                            }

                            (Petattacker as GamePlayer).Out.SendMessage(string.Format(LanguageMgr.GetTranslation((Petattacker as GamePlayer).Client.Account.Language, "GameLiving.AttackData.YourHits"), ad.Attacker.Name, attackTypeMsg, ad.Target.GetName(0, false), damage, modmessage2), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                            if (critdamage > 0)
                            {
                                (Petattacker as GamePlayer).Out.SendMessage(string.Format(LanguageMgr.GetTranslation((Petattacker as GamePlayer).Client.Account.Language, "GameLiving.AttackData.YourCriticallyHits"), ad.Attacker.Name, ad.Target.GetName(0, false), critdamage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                            }


                        }
                        #endregion

                        // decrease condition of hitted armor piece
                        if (ad.ArmorHitLocation != eArmorSlot.NOTSET)
                        {
                            InventoryItem item = Inventory.GetItem((eInventorySlot)ad.ArmorHitLocation);

                            if (item != null)
                            {
                                TryReactiveEffect(item, ad.Attacker);

                                if (item is GameInventoryItem)
                                {
                                    (item as GameInventoryItem).OnStruckByEnemy(this, ad.Attacker);
                                }
                            }
                        }
                        break;
                    }
                case eAttackResult.Blocked:
                    {
                        InventoryItem reactiveItem = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
                        if (reactiveItem != null && reactiveItem.Object_Type == (int)eObjectType.Shield)
                        {
                            TryReactiveEffect(reactiveItem, ad.Attacker);

                            if (reactiveItem is GameInventoryItem)
                            {
                                (reactiveItem as GameInventoryItem).OnStruckByEnemy(this, ad.Attacker);
                            }
                        }
                        break;
                    }
            }
            // vampiir
            if (CharacterClass is PlayerClass.ClassVampiir)
            {
                GameSpellEffect removeEffect = SpellHandler.FindEffectOnTarget(this, "VampiirSpeedEnhancement");
                if (removeEffect != null)
                    removeEffect.Cancel(false);
            }

            if (IsCrafting)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.InterruptedCrafting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                CraftTimer.Stop();
                CraftTimer = null;
                Out.SendCloseTimerWindow();
            }
        }


        /// <summary>
        /// Launch any reactive effect on an item
        /// </summary>
        /// <param name="reactiveItem"></param>
        /// <param name="target"></param>
        protected virtual void TryReactiveEffect(InventoryItem reactiveItem, GameLiving target)
        {
            if (reactiveItem != null)
            {
                int requiredLevel = reactiveItem.Template.LevelRequirement > 0 ? reactiveItem.Template.LevelRequirement : Math.Min(MaxLevel, reactiveItem.Level);

                if (requiredLevel <= Level)
                {
                    SpellLine reactiveEffectLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);

                    if (reactiveEffectLine != null)
                    {
                        if (reactiveItem.ProcSpellID != 0)
                        {
                            Spell spell = SkillBase.FindSpell(reactiveItem.ProcSpellID, reactiveEffectLine);

                            if (spell != null)
                            {
                                int chance = reactiveItem.ProcChance > 0 ? reactiveItem.ProcChance : 10;

                                if (Util.Chance(chance))
                                {
                                    ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(this, spell, reactiveEffectLine);
                                    if (spellHandler != null)
                                    {
                                        spellHandler.StartSpell(target, reactiveItem);
                                    }
                                }
                            }
                        }

                        if (reactiveItem.ProcSpellID1 != 0)
                        {
                            Spell spell = SkillBase.FindSpell(reactiveItem.ProcSpellID1, reactiveEffectLine);

                            if (spell != null)
                            {
                                int chance = reactiveItem.ProcChance > 0 ? reactiveItem.ProcChance : 10;

                                if (Util.Chance(chance))
                                {
                                    ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(this, spell, reactiveEffectLine);
                                    if (spellHandler != null)
                                    {
                                        spellHandler.StartSpell(target, reactiveItem);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Does needed interrupt checks and interrupts if needed
        /// </summary>
        /// <param name="attacker">the attacker that is interrupting</param>
        /// <param name="attackType">The attack type</param>
        /// <returns>true if interrupted successfully</returns>
        protected override bool OnInterruptTick(GameLiving attacker, AttackData.eAttackType attackType)
        {
            if (base.OnInterruptTick(attacker, attackType))
            {
                if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
                {
                    string attackTypeMsg = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Type.Shot");
                    if (AttackWeapon != null && AttackWeapon.Object_Type == (int)eObjectType.Thrown)
                        attackTypeMsg = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Type.Throw");
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.Interrupted", attacker.GetName(0, true), attackTypeMsg), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                }
                return true;
            }
            return false;
        }

        public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
        {
            if (source != null && damageAmount > 0 || damageType == eDamageType.Falling && damageAmount > 0 || damageType == eDamageType.Natural && damageAmount > 0 || damageType == eDamageType.Heat && damageAmount > 0)
            {
                #region PVP DAMAGE

                if (source != null && (source is GamePlayer || (source is GameNPC && (source as GameNPC).Brain is IControlledBrain && ((source as GameNPC).Brain as IControlledBrain).GetPlayerOwner() != null)))
                {
                    if (Realm != source.Realm && source.Realm != 0)
                        DamageRvRMemory += (long)(damageAmount + criticalAmount);
                }

                #endregion PVP DAMAGE

                base.TakeDamage(source, damageType, damageAmount, criticalAmount);
                if (this.HasAbility(Abilities.DefensiveCombatPowerRegeneration))
                {
                    this.Mana += (int)((damageAmount + criticalAmount) * 0.25);
                }
            }
        }

        /// <summary>
        /// Gets the effective AF of this living
        /// </summary>
        public override int EffectiveOverallAF
        { // TODO: consider player level against armor level
            get
            {
                int eaf = 0;
                int abs = 0;
                foreach (InventoryItem item in Inventory.VisibleItems)
                {
                    double factor = 0;
                    switch (item.Item_Type)
                    {
                        case Slot.TORSO:
                            factor = 2.2;
                            break;
                        case Slot.LEGS:
                            factor = 1.3;
                            break;
                        case Slot.ARMS:
                            factor = 0.75;
                            break;
                        case Slot.HELM:
                            factor = 0.5;
                            break;
                        case Slot.HANDS:
                            factor = 0.25;
                            break;
                        case Slot.FEET:
                            factor = 0.25;
                            break;
                    }

                    int itemAFCap = Level << 1;
                    if (RealmLevel > 39)
                        itemAFCap += 2;
                    switch ((eObjectType)item.Object_Type)
                    {
                        case eObjectType.Cloth:
                            abs = 0;
                            itemAFCap >>= 1;
                            break;
                        case eObjectType.Leather:
                            abs = 10;
                            break;
                        case eObjectType.Reinforced:
                            abs = 19;
                            break;
                        case eObjectType.Studded:
                            abs = 19;
                            break;
                        case eObjectType.Scale:
                            abs = 27;
                            break;
                        case eObjectType.Chain:
                            abs = 27;
                            break;
                        case eObjectType.Plate:
                            abs = 34;
                            break;
                    }

                    if (factor > 0)
                    {
                        int af = item.DPS_AF;
                        if (af > itemAFCap)
                            af = itemAFCap;
                        double piece_eaf = af * item.Quality / 100.0 * item.ConditionPercent / 100.0 * (1 + abs / 100.0);
                        eaf += (int)(piece_eaf * factor);
                    }
                }

                // Overall AF CAP = 10 * level * (1 + abs%/100)
                int bestLevel = -1;
                bestLevel = Math.Max(bestLevel, GetAbilityLevel(Abilities.AlbArmor));
                bestLevel = Math.Max(bestLevel, GetAbilityLevel(Abilities.HibArmor));
                bestLevel = Math.Max(bestLevel, GetAbilityLevel(Abilities.MidArmor));
                switch (bestLevel)
                {
                    default: abs = 0; break; // cloth etc
                    case ArmorLevel.Leather: abs = 10; break;
                    case ArmorLevel.Studded: abs = 19; break;
                    case ArmorLevel.Chain: abs = 27; break;
                    case ArmorLevel.Plate: abs = 34; break;
                }

                eaf += BaseBuffBonusCategory[(int)eProperty.ArmorFactor]; // base buff before cap
                int eafcap = (int)(10 * Level * (1 + abs * 0.01));
                if (eaf > eafcap)
                    eaf = eafcap;
                eaf += (int)Math.Min(Level * 1.875, SpecBuffBonusCategory[(int)eProperty.ArmorFactor])
                    - DebuffCategory[(int)eProperty.ArmorFactor]
                    + BuffBonusCategory4[(int)eProperty.ArmorFactor]
                    + Math.Min(Level, ItemBonus[(int)eProperty.ArmorFactor]);

                eaf = (int)(eaf * BuffBonusMultCategory1.Get((int)eProperty.ArmorFactor));

                return eaf;
            }
        }
        /// <summary>
        /// Calc Armor hit location when player is hit by enemy
        /// </summary>
        /// <returns>slotnumber where enemy hits</returns>
        /// attackdata(ad) changed
        public virtual eArmorSlot CalculateArmorHitLocation(AttackData ad)
        {
            if (ad.Style != null)
            {
                if (ad.Style.ArmorHitLocation != eArmorSlot.NOTSET)
                    return ad.Style.ArmorHitLocation;
            }
            int chancehit = Util.Random(1, 100);
            if (chancehit <= 40)
            {
                return eArmorSlot.TORSO;
            }
            else if (chancehit <= 65)
            {
                return eArmorSlot.LEGS;
            }
            else if (chancehit <= 80)
            {
                return eArmorSlot.ARMS;
            }
            else if (chancehit <= 90)
            {
                return eArmorSlot.HEAD;
            }
            else if (chancehit <= 95)
            {
                return eArmorSlot.HAND;
            }
            else
            {
                return eArmorSlot.FEET;
            }
        }

        /// <summary>
        /// determines current weaponspeclevel
        /// </summary>
        public override int WeaponSpecLevel(InventoryItem weapon)
        {
            if (weapon == null)
                return 0;
            // use axe spec if left hand axe is not in the left hand slot
            if (weapon.Object_Type == (int)eObjectType.LeftAxe && weapon.SlotPosition != Slot.LEFTHAND)
                return GameServer.ServerRules.GetObjectSpecLevel(this, eObjectType.Axe);

            // use left axe spec if axe is in the left hand slot
            if (weapon.SlotPosition == Slot.LEFTHAND
                && (weapon.Object_Type == (int)eObjectType.Axe
                    || weapon.Object_Type == (int)eObjectType.Sword
                    || weapon.Object_Type == (int)eObjectType.Hammer))
                return GameServer.ServerRules.GetObjectSpecLevel(this, eObjectType.LeftAxe);
            return GameServer.ServerRules.GetObjectSpecLevel(this, (eObjectType)weapon.Object_Type);
        }

        public virtual String GetWeaponSpec(InventoryItem weapon)
        {
            if (weapon == null)
                return null;
            // use axe spec if left hand axe is not in the left hand slot
            if (weapon.Object_Type == (int)eObjectType.LeftAxe && weapon.SlotPosition != Slot.LEFTHAND)
                return SkillBase.ObjectTypeToSpec(eObjectType.Axe);
            // use left axe spec if axe is in the left hand slot
            if (weapon.SlotPosition == Slot.LEFTHAND
                && (weapon.Object_Type == (int)eObjectType.Axe
                    || weapon.Object_Type == (int)eObjectType.Sword
                    || weapon.Object_Type == (int)eObjectType.Hammer))
                return SkillBase.ObjectTypeToSpec(eObjectType.LeftAxe);
            return SkillBase.ObjectTypeToSpec((eObjectType)weapon.Object_Type);
        }

        /// <summary>
        /// determines current weaponspeclevel
        /// </summary>
        public int WeaponBaseSpecLevel(InventoryItem weapon)
        {
            if (weapon == null)
                return 0;
            // use axe spec if left hand axe is not in the left hand slot
            if (weapon.Object_Type == (int)eObjectType.LeftAxe && weapon.SlotPosition != Slot.LEFTHAND)
                return GameServer.ServerRules.GetBaseObjectSpecLevel(this, eObjectType.Axe);
            // use left axe spec if axe is in the left hand slot
            if (weapon.SlotPosition == Slot.LEFTHAND
                && (weapon.Object_Type == (int)eObjectType.Axe
                    || weapon.Object_Type == (int)eObjectType.Sword
                    || weapon.Object_Type == (int)eObjectType.Hammer))
                return GameServer.ServerRules.GetBaseObjectSpecLevel(this, eObjectType.LeftAxe);
            return GameServer.ServerRules.GetBaseObjectSpecLevel(this, (eObjectType)weapon.Object_Type);
        }

        /// <summary>
        /// Gets the weaponskill of weapon
        /// </summary>
        /// <param name="weapon"></param>
        public override double GetWeaponSkill(InventoryItem weapon)
        {
            if (weapon == null)
            {
                return 0;
            }
            double classbase =
                (weapon.SlotPosition == (int)eInventorySlot.DistanceWeapon
                 ? CharacterClass.WeaponSkillRangedBase
                 : CharacterClass.WeaponSkillBase);

            //added for WS Poisons
            double preBuff = ((Level * classbase * 0.02 * (1 + (GetWeaponStat(weapon) - 50) * 0.005)) * Effectiveness);

            //return ((Level * classbase * 0.02 * (1 + (GetWeaponStat(weapon) - 50) * 0.005)) * PlayerEffectiveness);
            return Math.Max(0, preBuff * GetModified(eProperty.WeaponSkill) * 0.01);
        }

        /// <summary>
        /// calculates weapon stat
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public override int GetWeaponStat(InventoryItem weapon)
        {
            if (weapon != null)
            {
                switch ((eObjectType)weapon.Object_Type)
                {
                    // DEX modifier
                    case eObjectType.Staff:
                    case eObjectType.Fired:
                    case eObjectType.Longbow:
                    case eObjectType.Crossbow:
                    case eObjectType.CompositeBow:
                    case eObjectType.RecurvedBow:
                    case eObjectType.Thrown:
                    case eObjectType.Shield:
                        return GetModified(eProperty.Dexterity);

                    // STR+DEX modifier
                    case eObjectType.ThrustWeapon:
                    case eObjectType.Piercing:
                    case eObjectType.Spear:
                    case eObjectType.Flexible:
                    case eObjectType.HandToHand:
                        return (GetModified(eProperty.Strength) + GetModified(eProperty.Dexterity)) >> 1;
                }
            }
            // STR modifier for others
            return GetModified(eProperty.Strength);
        }
        /*
        /// <summary>
        /// ArmorHitchance Calculator
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int ArmorLevelHitchance(InventoryItem item)
        {
            // HEAD = eInventorySlot.HeadArmor, 21
            // HAND = eInventorySlot.HandsArmor, 22
            // FEET = eInventorySlot.FeetArmor,  23
            // TORSO = eInventorySlot.TorsoArmor, 25
            // LEGS = eInventorySlot.LegsArmor, 27
            // ARMS = eInventorySlot.ArmsArmor, 28


            if (item != null)
            {
                // 40 % Body
                ///25 % Legs
                //15 % Arms
                //10 % Head
                //5 % Feet
                // 5 % Hand  

                foreach(var slot in item.SlotPosition.ToString())
                {
                   if (slot  == "21")
                    {


                    }
                    if (slot == "22")
                    {

                    }

                    if (slot == "23")
                    {

                    }
                    if (slot == "25")
                    {

                    }
                   
                    if (slot == "27")
                    {

                    }
                    if (slot == "28")
                    {

                    }



                }
                
                               switch (item.SlotPosition)
                {
                    case 21: //HEAD
                        {
                            return item.Level * 10;
                        }

                    case 22: //HAND
                        {
                            return item.Level * 5;

                        }
                    case 23: //FEET
                        {
                            return item.Level * 5;

                        }
                    case 25: //TORSO
                        {
                            return item.Level * 40;

                        }
                    case 27: //LEGS
                        {
                            return item.Level * 25;

                        }
                    case 28: //ARMS
                        {
                            return item.Level * 15;

                        }

                }



            }
            return 0;
        }
    */
        /// <summary>
		/// calculate item armor factor influenced by quality, con and duration
		/// </summary>
		/// <param name="slot"></param>
		/// <returns></returns>
		public override double GetArmorAF(eArmorSlot slot)
        {
            if (slot == eArmorSlot.NOTSET) return 0;
            InventoryItem item = Inventory.GetItem((eInventorySlot)slot);
            if (item == null) return 0;
            double eaf = item.DPS_AF + BaseBuffBonusCategory[(int)eProperty.ArmorFactor]; // base AF buff

            int itemAFcap = Level;
            if (RealmLevel > 39)
                itemAFcap++;
            if (item.Object_Type != (int)eObjectType.Cloth)
            {
                itemAFcap <<= 1;
            }

            eaf = Math.Min(eaf, itemAFcap);

            //250AF = 20,875 % weniger schaden bei ~300 damage
            eaf *= 4.67; // compensate *4.67 in damage formula

            // my test shows that qual is added after AF buff
            eaf *= item.Quality * 0.01 * item.Condition / item.MaxCondition;

            //eaf += (GetModified(eProperty.ArmorFactor) / 4.67); //In den multiplizierten AF Grundwert addieren

            return eaf;
        }

        /// <summary>
        /// Calculates armor absorb level
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public override double GetArmorAbsorb(eArmorSlot slot)
        {
            if (slot == eArmorSlot.NOTSET) return 0;
            InventoryItem item = Inventory.GetItem((eInventorySlot)slot);
            if (item == null) return 0;
            // vampiir random armor debuff change ~
            double eaf = (item.SPD_ABS + GetModified(eProperty.ArmorAbsorption)) * 0.01;
            return eaf;
        }

        /// <summary>
        /// Weaponskill thats shown to the player
        /// </summary>
        public virtual int DisplayedWeaponSkill
        {
            get
            {
                int itemBonus = WeaponSpecLevel(AttackWeapon) - WeaponBaseSpecLevel(AttackWeapon) - RealmLevel / 10;
                double m = 0.56 + itemBonus / 70.0;
                double weaponSpec = WeaponSpecLevel(AttackWeapon) + itemBonus * m;
                return (int)(GetWeaponSkill(AttackWeapon) * (1.00 + weaponSpec * 0.01));
            }
        }

        int RRBonus = 0;
        /// <summary>
        /// Gets the weapondamage of currently used weapon
        /// Used to display weapon damage in stats, 16.5dps = 1650
        /// </summary>
        /// <param name="weapon">the weapon used for attack</param>
        public override double WeaponDamage(InventoryItem weapon)
        {
           
           
            if (weapon != null)
            {
                int DPSAF = weapon.DPS_AF;

                double mythicalDPS = GetModified(eProperty.MythicalDPS);

               


                if (Client.Player.RealmLevel >= 40 && DPSAF == 165)
                {
                   RRBonus = 3;
                }
                else if (DPSAF == 165)
                {
                    DPSAF = DPSAF -= 3;
                    RRBonus = 0;
                }
                else
                { 
                    RRBonus = 0;
                }
                
                //log.ErrorFormat("wdamage1 {0}", weapon.DPS_AF);
                //TODO if attackweapon is ranged -> attackdamage is arrow damage
                int DPS = DPSAF;
               
                if (Properties.ENABLE_RELIC_DAMAGE_BONUS)
                {
                    // apply relic bonus prior to cap
                    DPS = (int)((double)DPS * (1.0 + RelicMgr.GetRelicBonusModifier(Realm, eRelicType.Strength)));
                }
                
                // apply damage cap before quality
                // http://www.classesofcamelot.com/faq.asp?mode=view&cat=10
                //int cap = (int)12 + RRBonus * Level;
                int cap = (int)(12 + Level * 3);
                cap += RRBonus;


                if (mythicalDPS > 0)
                {
                    cap += Convert.ToInt16(mythicalDPS);
                }
               
               

                if (DPS > cap)
                {
                    DPS = cap;
                }
               
                //log.ErrorFormat("wdamage2 {0}", DPS);
                //(1.0 + BuffBonusCategory1[(int)eProperty.DPS]/100.0 - BuffBonusCategory3[(int)eProperty.DPS]/100.0)
                DPS = (int)((DPS + mythicalDPS) * (1 + ((GetModified(eProperty.DPS)) * 0.01)));
                //log.ErrorFormat("DPS: {0}", DPS);
                // beware to use always ConditionPercent, because Condition is abolute value
                //				return (int) ((DPS/10.0)*(weapon.Quality/100.0)*(weapon.Condition/(double)weapon.MaxCondition)*100.0);
                double wdamage = (0.001 * DPS * weapon.Quality * weapon.Condition) / weapon.MaxCondition;
                //log.ErrorFormat("wdamage: {0}", wdamage);

                return wdamage;
            }
            else
            {
                return 0;
            }
        }


        /// <summary>
        /// Max. Damage possible without style
        /// </summary>
        /// <param name="weapon">attack weapon</param>
        public override double UnstyledDamageCap(InventoryItem weapon)
        {
            if (weapon != null)
            {

                int DPS = weapon.DPS_AF;
                                               
                int cap = 12 + 3 * Level;
                    cap += RRBonus;
                if (DPS > cap)
                    DPS = cap;

               
                
                int WeaponSPD = weapon.SPD_ABS;
                double result;


                //result = weapon.DPS_AF * (weapon.SPD_ABS + DPSADD) * 0.06 * ((0.01886 * Level));
                //result = DPS * (weapon.SPD_ABS + CeleADD) * 0.03 * (0.94 + 0.003 * weapon.SPD_ABS); ok
                result = DPS * (weapon.SPD_ABS) * 0.03 * (0.94 + 0.002 * weapon.SPD_ABS);



                //Twohanded
                if (weapon.Hand == 1)
                {
                    result *= 1.1 + (WeaponSpecLevel(weapon) - 1) * 0.005;
                    //log.ErrorFormat("result2 {0}", result);


                    //Old Archery
                    if (weapon.Item_Type == Slot.RANGED)
                    {
                        // http://home.comcast.net/~shadowspawn3/bowdmg.html
                        //ammo damage bonus
                        double ammoDamageBonus = 1;
                        if (RangeAttackAmmo != null)
                        {
                            switch ((RangeAttackAmmo.SPD_ABS) & 0x3)
                            {
                                case 0: ammoDamageBonus = 0.85; break; 	//Blunt       (light) -15%
                                case 1: ammoDamageBonus = 1; break; 	//Bodkin     (medium)   0%
                                case 2: ammoDamageBonus = 1.15; break; 	//doesn't exist on live
                                case 3: ammoDamageBonus = 1.25; break; 	//Broadhead (X-heavy) +25%
                            }
                        }
                        result *= ammoDamageBonus;
                    }
                }
                if (weapon.Item_Type == Slot.RANGED && (weapon.Object_Type == (int)eObjectType.Longbow || weapon.Object_Type == (int)eObjectType.RecurvedBow || weapon.Object_Type == (int)eObjectType.CompositeBow))
                {
                    if (ServerProperties.Properties.ALLOW_OLD_ARCHERY == true)
                    {
                        result += GetModified(eProperty.RangedDamage) * 0.01;
                    }
                    else
                    {
                        result += GetModified(eProperty.SpellDamage) * 0.01;
                        result += GetModified(eProperty.RangedDamage) * 0.01;
                        result += GetModified(eProperty.CastingSpeed) * 0.01;
                    }
                }
                else if (weapon.Item_Type == Slot.RANGED)
                {
                    //Ranged damage buff,debuff,Relic,RA
                    result += GetModified(eProperty.RangedDamage) * 0.01;
                }
                else if (weapon.Item_Type == Slot.RIGHTHAND || weapon.Item_Type == Slot.LEFTHAND || weapon.Item_Type == Slot.TWOHAND)
                {
                    result += GetModified(eProperty.MeleeDamage) * 0.01;

                }

                return result;
            }
            else
            {
                return AttackDamage(weapon) * 3 * (1 + (AttackSpeed(weapon) * 0.001 - 2) * .03);
            }
        }


        /// <summary>
        /// Can this player cast the given spell while in combat?
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        public override bool CanCastInCombat(Spell spell)
        {
            if (CharacterClass is PlayerClass.ClassVampiir ||
                CharacterClass is PlayerClass.ClassMaulerAlb ||
                CharacterClass is PlayerClass.ClassMaulerMid ||
                CharacterClass is PlayerClass.ClassMaulerHib ||
                (CharacterClass is PlayerClass.ClassWarden && spell.SpellType == "HealOverTime") ||
                (CharacterClass is PlayerClass.ClassFriar && spell.SpellType == "HealOverTime"))
            {
                return true;
            }
            //Heretic nicht unterbrechbare Spells
            if (CharacterClass is PlayerClass.ClassHeretic &&
             spell.Name == "Glistening Blaze" || spell.Name == "Whirling Blaze" || spell.Name == "Torrential Blaze")
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// The chance for a critical hit
        /// </summary>
        /// <param name="weapon">attack weapon</param>
        public override int AttackCriticalChance(InventoryItem weapon)
        {
            if (weapon != null && weapon.Item_Type == Slot.RANGED && RangedAttackType == eRangedAttackType.Critical == false)
                return 0; // no crit damage for ranged Spells


            // check for melee attack
            if (weapon != null && weapon.Item_Type != Slot.RANGED)
            {
                return GetModified(eProperty.CriticalMeleeHitChance);
            }

            // check for ranged attack
            if (weapon != null && weapon.Item_Type == Slot.RANGED || RangedAttackType == eRangedAttackType.Critical)
            {
                return GetModified(eProperty.CriticalSpellHitChance);
            }

            // base 10% chance of critical for all with melee weapons
            return 10;
        }

        /// <summary>
        /// Returns the damage type of the current attack
        /// </summary>
        /// <param name="weapon">attack weapon</param>
        public override eDamageType AttackDamageType(InventoryItem weapon)
        {
            if (weapon == null)
                return eDamageType.Natural;
            switch ((eObjectType)weapon.Object_Type)
            {
                case eObjectType.Crossbow:
                case eObjectType.Longbow:
                case eObjectType.CompositeBow:
                case eObjectType.RecurvedBow:
                case eObjectType.Fired:
                    InventoryItem ammo = RangeAttackAmmo;
                    if (ammo == null)
                        return (eDamageType)weapon.Type_Damage;
                    return (eDamageType)ammo.Type_Damage;
                case eObjectType.Shield:
                    return eDamageType.Crush; // TODO: shields do crush damage (!) best is if Type_Damage is used properly
                default:
                    return (eDamageType)weapon.Type_Damage;
            }
        }

        /// <summary>
        /// Returns the AttackRange of this living
        /// </summary>
        public override int AttackRange
        { /*
		tested with:
		staff					= 125-130
		sword			   		= 126-128.06
		shield (Numb style)		= 127-129
		polearm	(Impale style)	= 127-130
		mace (Daze style)		= 127.5-128.7

		Think it's safe to say that it never changes; different with mobs.
			 */
            get
            {
                GameLiving livingTarget = TargetObject as GameLiving;

                //TODO change to real distance of bows!
                if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
                {
                    InventoryItem weapon = AttackWeapon;
                    if (weapon == null)
                        return 0;

                    double range;
                    InventoryItem ammo = RangeAttackAmmo;

                    switch ((eObjectType)weapon.Object_Type)
                    {
                        case eObjectType.Longbow: range = 1760; break;
                        case eObjectType.RecurvedBow: range = 1680; break;
                        case eObjectType.CompositeBow: range = 1600; break;
                        default: range = 1200; break; // shortbow, xbow, throwing
                    }

                    range = Math.Max(32, range * GetModified(eProperty.ArcheryRange) * 0.01);

                    if (ammo != null)
                        switch ((ammo.SPD_ABS >> 2) & 0x3)
                        {
                            case 0: range *= 0.85; break; //Clout -15%
                                                          //						case 1:                break; //(none) 0%
                            case 2: range *= 1.15; break; //doesn't exist on live
                            case 3: range *= 1.25; break; //Flight +25%
                        }
                    if (livingTarget != null) range += Math.Min((Z - livingTarget.Z) / 2.0, 500);
                    if (range < 32) range = 32;

                    return (int)(range);
                }

                int meleerange = 128;
                GameKeepComponent keepcomponent = livingTarget as GameKeepComponent; // TODO better component melee attack range check
                if (keepcomponent != null)
                    meleerange += 150;
                else
                {
                    if (livingTarget is GameNPC && livingTarget != null && livingTarget.MeleeAttackRange > 0 && livingTarget.IsMoving == false)
                    {
                        meleerange = livingTarget.MeleeAttackRange;
                    }

                    if (livingTarget != null && livingTarget.IsMoving)
                        meleerange += 32;
                    if (IsMoving)
                        meleerange += 32;
                }
                return meleerange;
            }
            set { }
        }

        /// <summary>
        /// Gets the current attackspeed of this living in milliseconds
        /// </summary>
        /// <param name="weapons">attack weapons</param>
        /// <returns>effective speed of the attack. average if more than one weapon.</returns>
        public override int AttackSpeed(params InventoryItem[] weapons)
        {
            if (weapons == null || weapons.Length < 1)
                return 0;

            int count = 0;
            double speed = 0;
            bool bowWeapon = true;

            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null)
                {
                    speed += weapons[i].SPD_ABS;
                    count++;

                    switch (weapons[i].Object_Type)
                    {
                        case (int)eObjectType.Fired:
                        case (int)eObjectType.Longbow:
                        case (int)eObjectType.Crossbow:
                        case (int)eObjectType.RecurvedBow:
                        case (int)eObjectType.CompositeBow:
                            break;
                        default:
                            bowWeapon = false;
                            break;
                    }
                }
            }

            if (count < 1)
                return 0;

            speed /= count;

            int qui = Math.Min(250, Quickness); //250 soft cap on quickness

            if (bowWeapon)
            {
                if (ServerProperties.Properties.ALLOW_OLD_ARCHERY)
                {
                    //Draw Time formulas, there are very many ...
                    //Formula 2: y = iBowDelay * ((100 - ((iQuickness - 50) / 5 + iMasteryofArcheryLevel * 3)) / 100)
                    //Formula 1: x = (1 - ((iQuickness - 60) / 500 + (iMasteryofArcheryLevel * 3) / 100)) * iBowDelay
                    //Table a: Formula used: drawspeed = bowspeed * (1-(quickness - 50)*0.002) * ((1-MoA*0.03) - (archeryspeedbonus/100))
                    //Table b: Formula used: drawspeed = bowspeed * (1-(quickness - 50)*0.002) * (1-MoA*0.03) - ((archeryspeedbonus/100 * basebowspeed))

                    //For now use the standard weapon formula, later add ranger haste etc.
                    speed *= (1.0 - (qui - 60) * 0.002);
                    double percent;
                    // Calcul ArcherySpeed bonus to substract
                    percent = speed * 0.01 * GetModified(eProperty.ArcherySpeed);
                    // Apply RA difference
                    speed -= percent;
                    //log.Debug("speed = " + speed + " percent = " + percent + " eProperty.archeryspeed = " + GetModified(eProperty.ArcherySpeed));
                    if (RangedAttackType == eRangedAttackType.Critical)
                        speed = speed * 2 - (GetAbilityLevel(Abilities.Critical_Shot) - 1) * speed / 10;
                }
                else
                {
                    // no archery bonus
                    speed *= (1.0 - (qui - 60) * 0.002);
                }
            }
            else
            {
                // TODO use haste
                //Weapon Speed*(1-(Quickness-60)/500]*(1-Haste)
                speed *= (1.0 - (qui - 60) * 0.002) * 0.01 * GetModified(eProperty.MeleeSpeed);
            }

            // apply (same 1.5 cap) speed cap for Twohanded
            if (speed < 15 && this.AttackWeapon.Item_Type == 12)
            {
                speed = 15;
                //log.ErrorFormat("speed twohanded {0}", speed);
            }
            else if (speed < 15 && this.AttackWeapon.Item_Type != 12)
            {
                speed = 15;
                //log.ErrorFormat("speed onehand {0}", speed);
            }

            return (int)(speed * 100);
        }

        /// <summary>
        /// Gets the attack damage
        /// </summary>
        /// <param name="weapon">the weapon used for attack</param>
        /// <returns>the weapon damage</returns>
        public override double AttackDamage(InventoryItem weapon)
        {
            if (weapon == null)
                return 0;

            double effectiveness = 1.00;
            double damage = WeaponDamage(weapon) * weapon.SPD_ABS * 0.1;

            if (weapon.Hand == 1) // two-hand
            {
                // twohanded used weapons get 2H-Bonus = 10% + (Skill / 2)%
                int spec = WeaponSpecLevel(weapon) - 1;
                damage *= 1.1 + spec * 0.005;
            }

            if (weapon.Item_Type == Slot.RANGED)
            {
                //ammo damage bonus
                if (RangeAttackAmmo != null)
                {

                    //Falconeye Ability Handling for Crit's by Elcotek
                    int BaseChance = ServerProperties.Properties.BOW_BASE_CRITCHANCE;

                    bool CritChance = false;

                    RAPropertyEnhancer FalconEye = GetAbility<FalconsEyeAbility>();

                    if (FalconEye != null)
                    {

                        BaseChance += FalconEye.GetAmountForLevel(CalculateSkillLevel(FalconEye));
                        if (Util.Chance(BaseChance))
                        {
                            CritChance = true;
                        }
                    }
                    else
                    {
                        BaseChance = ServerProperties.Properties.BOW_BASE_CRITCHANCE;
                        if (Util.Chance(BaseChance))
                        {
                            CritChance = true;
                        }
                    }

                    double CritDamage;
                    int RandomValue = Util.Random(3);
                    CritDamage = (RandomValue * 0.34);

                    switch ((RangeAttackAmmo.SPD_ABS) & 0x3)
                    {

                        case 1: //&& weapon != null && this is GamePlayer && this.RangedAttackType == eRangedAttackType.Critical elcoek falconeye
                            if (CritChance == true)
                            {
                                damage *= (0.85 + CritDamage);
                                if (CritDamage > 0)
                                {
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client, "This shot was critical! the chance is: " + BaseChance.ToString() + " %"), eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);
                                }

                                break; //Blunt       (light) -15%



                            }
                            else
                                damage *= 0.85; break; //Blunt       (light) -15%
                        //case 1: damage *= 1;	break; //Bodkin     (medium)   0%
                        case 2:
                            if (CritChance == true)
                            {

                                damage *= (1.15 + CritDamage);
                                if (CritDamage > 0)
                                {
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client, "This shot was critical! the chance is: " + BaseChance.ToString() + "  %"), eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);
                                }

                                break; //doesn't exist on live
                            }
                            else
                                damage *= 1.15; break; //doesn't exist on live

                        case 3:
                            if (CritChance == true)
                            {
                                damage *= (1.25 + CritDamage);
                                if (CritDamage > 0)
                                {
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client, "This shot was critical! the chance is: " + BaseChance.ToString() + "  %"), eChatType.CT_DamageAdd, eChatLoc.CL_SystemWindow);
                                }
                                break;
                            }
                            else
                                damage *= 1.25; break; //Broadhead (X-heavy) +25%
                    }
                }
                effectiveness += GetModified(eProperty.RangedDamage) * 0.01;
            }
            else if (weapon.Item_Type == Slot.RIGHTHAND || weapon.Item_Type == Slot.LEFTHAND || weapon.Item_Type == Slot.TWOHAND)
            {
                //Melee damage buff,debuff,Relic,RA
                effectiveness += GetModified(eProperty.MeleeDamage) * 0.01;
            }
            damage *= effectiveness;
            return damage;
        }

        /// <summary>
        /// Stores the amount of realm points gained by other players on last death
        /// </summary>
        protected long m_lastDeathRealmPoints;
        private string playerMessage;
        private string publicMessage;
        /// <summary>
        /// Gets/sets the amount of realm points gained by other players on last death
        /// </summary>
        public long LastDeathRealmPoints
        {
            get { return m_lastDeathRealmPoints; }
            set { m_lastDeathRealmPoints = value; }
        }

        /// <summary>
        /// Called when the player dies
        /// </summary>
        /// <param name="killer">the killer</param>
        public override void Die(GameObject killer)
        {
            // ambiant talk
            if (killer is GameNPC)
                (killer as GameNPC).FireAmbientSentence(GameNPC.eAmbientTrigger.killing, this);

            CharacterClass.Die(killer);

            bool realmDeath = killer != null && killer.Realm != eRealm.None;

            TargetObject = null;
            Diving(WaterBreath.Normal);
            if (IsOnHorse)
                IsOnHorse = false;

            // cancel task if active
            if (QuestTask != null && QuestTask.TaskActive)
                QuestTask.ExpireTask();



            ushort messageDistance = WorldMgr.DEATH_MESSAGE_DISTANCE;
            m_releaseType = eReleaseType.Normal;

            string location = String.Empty;
            if (CurrentAreas.Count > 0 && (CurrentAreas[0] is Area.BindArea) == false)
                location = (CurrentAreas[0] as AbstractArea).Description;
            else
                location = CurrentZone.Description;

            if (killer == null)
            {
                if (realmDeath)
                {
                    playerMessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.KilledLocation", GetName(0, true), location);
                    publicMessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.KilledLocation", GetName(0, true), location);
                }
                else
                {
                    playerMessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.Killed", GetName(0, true));
                    publicMessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.Killed", GetName(0, true));
                }
            }
            else
            {
                if (DuelTarget == killer)
                {
                    m_releaseType = eReleaseType.Duel;
                    messageDistance = WorldMgr.YELL_DISTANCE;
                    playerMessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.DuelDefeated", GetName(0, true), killer.GetName(1, false));
                    publicMessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.DuelDefeated", GetName(0, true), killer.GetName(1, false));
                }
                else
                {
                    messageDistance = 0;
                    if (realmDeath)
                    {
                        //für alle Sprachen in public!!
                        foreach (GameClient client in WorldMgr.GetAllPlayingClients())
                        {
                            if (client.Player.CurrentRegion == killer.CurrentRegion)
                            {

                                playerMessage = LanguageMgr.GetTranslation(this.Client.Account.Language, "GamePlayer.Die.KilledByLocation", GetName(0, true), killer.GetName(1, false), location);
                                publicMessage = LanguageMgr.GetTranslation(client.Account.Language, "GamePlayer.Die.KilledByLocation", GetName(0, true), killer.GetName(1, false), location);
                            }
                        }
                    }
                    else
                    {
                        playerMessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.KilledBy", GetName(0, true), killer.GetName(1, false));
                        publicMessage = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.KilledBy", GetName(0, true), killer.GetName(1, false));
                    }
                }
            }

            DuelStop();


            eChatType messageType;
            if (m_releaseType == eReleaseType.Duel)
                messageType = eChatType.CT_Emote;
            else if (killer == null)
            {
                messageType = eChatType.CT_PlayerDied;
            }
            else
            {
                switch ((eRealm)killer.Realm)
                {
                    case eRealm.Albion: messageType = eChatType.CT_KilledByAlb; break;
                    case eRealm.Midgard: messageType = eChatType.CT_KilledByMid; break;
                    case eRealm.Hibernia: messageType = eChatType.CT_KilledByHib; break;
                    default: messageType = eChatType.CT_PlayerDied; break; // killed by mob
                }
            }

            if (killer is GamePlayer && killer != this)
            {
                ((GamePlayer)killer).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)killer).Client.Account.Language, "GamePlayer.Die.YouKilled", GetName(0, false)), eChatType.CT_PlayerDied, eChatLoc.CL_SystemWindow);
            }

            ArrayList players = new ArrayList();
            if (messageDistance == 0)
            {
                foreach (GameClient client in WorldMgr.GetClientsOfRegion(CurrentRegionID))
                {
                    players.Add(client.Player);
                }
            }
            else
            {
                foreach (GamePlayer player in GetPlayersInRadius(messageDistance))
                {
                    if (player == null) continue;
                    players.Add(player);
                }
            }

            foreach (GamePlayer player in players)
            {
                // on normal server type send messages only to the killer and dead players realm
                // check for gameplayer is needed because killers realm don't see deaths by guards
                if (
                    (player != killer) && (
                        (killer != null && killer is GamePlayer && GameServer.ServerRules.IsSameRealm((GamePlayer)killer, player, true))
                        || (GameServer.ServerRules.IsSameRealm(this, player, true))
                        || ServerProperties.Properties.DEATH_MESSAGES_ALL_REALMS)
                )
                    if (player == this)
                        player.Out.SendMessage(playerMessage, messageType, eChatLoc.CL_SystemWindow);
                    else player.Out.SendMessage(publicMessage, messageType, eChatLoc.CL_SystemWindow);
            }

            //Dead ppl. dismount ...
            if (Steed != null)
                DismountSteed(true);
            //Dead ppl. don't sit ...
            if (IsSitting)
            {
                IsSitting = false;
                UpdatePlayerStatus();
            }

            // then buffs drop messages
            base.Die(killer);

            lock (m_LockObject)
            {
                if (m_releaseTimer != null)
                {
                    m_releaseTimer.Stop();
                    m_releaseTimer = null;
                }

                if (m_quitTimer != null)
                {
                    m_quitTimer.Stop();
                    m_quitTimer = null;
                }
                m_automaticRelease = m_releaseType == eReleaseType.Duel;
                m_releasePhase = 0;
                m_deathTick = GameTimer.GetTickCount(); // we use realtime, because timer window is realtime
                                                        //UpdatePlayerStatus();

                Out.SendTimerWindow(LanguageMgr.GetTranslation(Client.Account.Language, "System.ReleaseTimer"), (m_automaticRelease ? RELEASE_MINIMUM_WAIT : RELEASE_TIME));
                m_releaseTimer = new RegionTimer(this)
                {
                    Callback = new RegionTimerCallback(ReleaseTimerCallback)
                };
                m_releaseTimer.Start(1000);

                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.ReleaseToReturn"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);

                // clear target object so no more actions can used on this target, spells, styles, attacks...
                TargetObject = null;

                //remove buffs for others too if not yet!!
                foreach (GameSpellEffect effect in EffectList.GetAllOfType<GameSpellEffect>())
                {
                    if (effect.SpellHandler.HasPositiveEffect && effect.SpellHandler.Spell.Target != "Self")
                        effect.Cancel(false);
                }

                foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    if (player == null) continue;
                    player.Out.SendPlayerDied(this, killer);
                }

                // first penalty is 5% of expforlevel, second penalty comes from release
                int xpLossPercent = 5;
                if (Level > 40)
                {
                    xpLossPercent = 1;
                }

                if (realmDeath)
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.DeadRVR"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
                    xpLossPercent = 0;
                }
                else if (Level > 5) // under level 5 there is no penalty
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.LoseExperience"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
                    // if this is the first death in level, you lose only half the penalty
                    switch (DBCharacter.DeathCount)
                    {
                        case 0:
                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.DeathN1"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
                            xpLossPercent /= 3;
                            break;
                        case 1:
                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.DeathN2"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
                            xpLossPercent = xpLossPercent * 2 / 3;
                            break;
                    }

                    DBCharacter.DeathCount++;

                    long xpLoss = (ExperienceForNextLevel - ExperienceForCurrentLevel) * xpLossPercent / 100;
                    GainExperience(eXPSource.Other, -xpLoss, 0, 0, 0, false, true);
                    TempProperties.setProperty(DEATH_EXP_LOSS_PROPERTY, xpLoss);

                    int conLoss = DBCharacter.DeathCount;
                    if (conLoss > 3)
                        conLoss = 3;
                    else if (conLoss < 1)
                        conLoss = 1;
                    TempProperties.setProperty(DEATH_CONSTITUTION_LOSS_PROPERTY, conLoss);
                }
                GameEventMgr.AddHandler(this, GamePlayerEvent.Revive, new DOLEventHandler(OnRevive));
            }

            if (this.ControlledBrain != null)
                CommandNpcRelease();

            if (this.SiegeWeapon != null)
                SiegeWeapon.ReleaseControl();

            // sent after buffs drop
            //Message.SystemToOthers2(this, eChatType.CT_PlayerDied, "GamePlayer.Die.CorpseLies", GetName(0, true), GetPronoun(this.Client, 1, true));
            // Message.SystemToOthers2(this, LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.DuelDefeated", Name, GetPronoun(this.Client, 1, true).ToString()), eChatType.CT_PlayerDied);
            Message.SystemToOthers(this, LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.CorpseLies", Name, GetPronoun(this.Client, 1, true).ToString()), eChatType.CT_Emote);
            if (m_releaseType == eReleaseType.Duel)
            {
                Message.SystemToOthers(this, LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Die.DuelWinner", killer.Name), eChatType.CT_Emote);
                //Message.SystemToOthers(this, killer.Name + "GamePlayer.Die.DuelDefeated", eChatType.CT_Emote);
            }
            // deal out exp and realm points based on server rules
            GameServer.ServerRules.OnPlayerKilled(this, killer);

            if (m_releaseType != eReleaseType.Duel)
                DBCharacter.DeathTime = PlayedTime;

            IsSwimming = false;
        }

        public override void EnemyKilled(GameLiving enemy)
        {
            if (Group != null || MemberIsInBG(this) && GS.ServerProperties.Properties.BATTLEGROUP_LOOT)
            {

                BattleGroup battleGroup = TempProperties.getProperty<BattleGroup>(BattleGroup.BATTLEGROUP_PROPERTY, null);

                if (battleGroup != null && GS.ServerProperties.Properties.BATTLEGROUP_LOOT)
                {
                    //BattleGroup
                    foreach (GamePlayer players in battleGroup.Members.Keys)
                    {
                        if (players == this) continue;
                        if (enemy.Attackers.Contains(players)) continue;
                        if (this.IsWithinRadius(players, WorldMgr.MAX_EXPFORKILL_DISTANCE))
                        {

                            enemy.EnemyKilled(players);
                            //Notify(GameLivingEvent.EnemyKilled, players, new EnemyKilledEventArgs(enemy));

                           // if (players.Attackers.Contains(enemy))
                                //players.RemoveAttacker(enemy);

                            if (players.ControlledBrain != null && players.ControlledBrain.Body.Attackers.Contains(enemy))
                                players.ControlledBrain.Body.RemoveAttacker(enemy);
                        }
                    }

                }
                else
                {
                    foreach (GamePlayer player in Group.GetPlayersInTheGroup())
                    {
                        if (player == this) continue;
                        if (enemy.Attackers.Contains(player)) continue;
                        if (this.IsWithinRadius(player, WorldMgr.MAX_EXPFORKILL_DISTANCE))
                        {

                            // enemy.EnemyKilled(enemy);
                            // Notify(GameLivingEvent.EnemyKilled, player, new EnemyKilledEventArgs(enemy));

                            if (player.Attackers.Contains(enemy))
                                player.RemoveAttacker(enemy);

                            if (player.ControlledBrain != null && player.ControlledBrain.Body.Attackers.Contains(enemy))
                                player.ControlledBrain.Body.RemoveAttacker(enemy);
                        }
                    }
                }
            }
           

            if (ControlledBrain != null && ControlledBrain.Body.Attackers.Contains(enemy))
                ControlledBrain.Body.RemoveAttacker(enemy);

            base.EnemyKilled(enemy);
        }


        /// <summary>
        /// Check this flag to see wether this living is involved in combat
        /// </summary>
        public override bool InCombat
        {
            get
            {
                IControlledBrain npc = ControlledBrain;
                if (npc != null && npc.Body.InCombat)
                    return true;
                return base.InCombat;
            }
        }

        /// <summary>
        /// Easy method to get the resist of a certain damage type
        /// Good for when we add RAs
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public override int GetDamageResist(eProperty property)
        {
            int res = 0;
            int classResist = 0;
            int secondResist = 0;

            //Q: Do the Magic resist bonuses from Bedazzling Aura and Empty Mind stack with each other?
            //A: Nope.
            switch ((eResist)property)
            {
                case eResist.Body:
                case eResist.Cold:
                case eResist.Energy:
                case eResist.Heat:
                case eResist.Matter:
                case eResist.Spirit:
                    res += BaseBuffBonusCategory[(int)eProperty.MagicAbsorption];
                    break;
                default:
                    break;
            }
            return (int)((res + classResist) - 0.01 * secondResist * (res + classResist) + secondResist);
        }

        #endregion

        #region Duel

        /// <summary>
        /// The duel target of this player
        /// </summary>
        protected GamePlayer m_duelTarget;

        /// <summary>
        /// Gets the duel target of this player
        /// </summary>
        public GamePlayer DuelTarget
        {
            get { return m_duelTarget; }
        }

        /// <summary>
        /// Starts the duel
        /// </summary>
        /// <param name="duelTarget">The duel target</param>
        public void DuelStart(GamePlayer duelTarget)
        {
            if (DuelTarget != null)
                return;

            GameEventMgr.AddHandler(this, GamePlayerEvent.Quit, new DOLEventHandler(DuelOnPlayerQuit));
            GameEventMgr.AddHandler(this, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(DuelOnAttack));
            GameEventMgr.AddHandler(this, GameLivingEvent.AttackFinished, new DOLEventHandler(DuelOnAttack));
            m_duelTarget = duelTarget;
            duelTarget.DuelStart(this);
        }

        /// <summary>
        /// Stops the duel if it is running
        /// </summary>
        public void DuelStop()
        {
            GamePlayer target = DuelTarget;
            if (target == null)
                return;

            foreach (GameSpellEffect effect in EffectList.GetAllOfType<GameSpellEffect>())
            {
                if (effect.SpellHandler.Caster == target && !effect.SpellHandler.HasPositiveEffect)
                    effect.Cancel(false);
            }
            m_duelTarget = null;
            GameEventMgr.RemoveHandler(this, GamePlayerEvent.Quit, new DOLEventHandler(DuelOnPlayerQuit));
            GameEventMgr.RemoveHandler(this, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(DuelOnAttack));
            GameEventMgr.RemoveHandler(this, GameLivingEvent.AttackFinished, new DOLEventHandler(DuelOnAttack));

            if (XPGainers != null)
                _XPGainers.Clear();
           
            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.DuelStop.DuelEnds"), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
            target.DuelStop();
        }

        /// <summary>
        /// Stops the duel if player attack or is attacked by anything other that duel target
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        private void DuelOnAttack(DOLEvent e, object sender, EventArgs arguments)
        {
            AttackData ad = null;
            GameLiving target = null;
            if (arguments is AttackFinishedEventArgs)
            {
                ad = ((AttackFinishedEventArgs)arguments).AttackData;
                target = ad.Target;
            }
            else if (arguments is AttackedByEnemyEventArgs)
            {
                ad = ((AttackedByEnemyEventArgs)arguments).AttackData;
                target = ad.Attacker;
            }

            if (ad == null)
                return;

            // check pets owner for my and enemy attacks
            GameNPC npc = target as GameNPC;
            if (npc != null)
            {
                IControlledBrain brain = npc.Brain as IControlledBrain;
                if (brain != null)
                    target = brain.Owner;
            }

            // Duel should end if players join group and trys to attack
            if (ad.Attacker.Group != null && ad.Attacker.Group.IsInTheGroup(ad.Target))
            {
                DuelStop();
            }

            switch (ad.AttackResult)
            {
                case eAttackResult.Blocked:
                case eAttackResult.Evaded:
                case eAttackResult.Fumbled:
                case eAttackResult.HitStyle:
                case eAttackResult.HitUnstyled:
                case eAttackResult.Missed:
                case eAttackResult.Parried:
                    if (target != DuelTarget)
                        DuelStop();
                    break;
            }
        }

        /// <summary>
        /// Stops the duel on quit/link death
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        private void DuelOnPlayerQuit(DOLEvent e, object sender, EventArgs arguments)
        {
            DuelStop();
        }

        #endregion

        #region Spell cast

        /// <summary>
        /// The time someone can not cast
        /// </summary>
        protected long m_disabledCastingTimeout = 0;
        /// <summary>
        /// Time when casting is allowed again (after interrupt from enemy attack)
        /// </summary>
        public long DisabledCastingTimeout
        {
            get { return m_disabledCastingTimeout; }
            set { m_disabledCastingTimeout = value; }
        }




        /// <summary>
        /// The next spell
        /// </summary>
        protected Spell m_nextSpell;
        /// <summary>
        /// The next spell line
        /// </summary>
        protected SpellLine m_nextSpellLine;
        /// <summary>
        /// The next spell target
        /// </summary>
        protected GameLiving m_nextSpellTarget;
        /// <summary>
        /// A lock for the spellqueue
        /// </summary>
        protected object m_spellQueueAccessMonitor = new object();

        /// <summary>
        /// Clears the spell queue when a player is interrupted
        /// </summary>
        public void ClearSpellQueue()
        {
            lock (m_spellQueueAccessMonitor)
            {
                m_nextSpell = null;
                m_nextSpellLine = null;
                m_nextSpellTarget = null;
            }
        }

        /// <summary>
        /// Callback after spell execution finished and next spell can be processed
        /// </summary>
        /// <param name="handler"></param>
        public override void OnAfterSpellCastSequence(ISpellHandler handler)
        {
            InventoryItem lastUsedItem = TempProperties.getProperty<InventoryItem>(LAST_USED_ITEM_SPELL, null);
            if (lastUsedItem != null)
            {
                if (handler.StartReuseTimer)
                {
                    lastUsedItem.CanUseAgainIn = lastUsedItem.CanUseEvery;
                }

                TempProperties.removeProperty(LAST_USED_ITEM_SPELL);
            }

            lock (m_spellQueueAccessMonitor)
            {
                Spell nextSpell = m_nextSpell;
                SpellLine nextSpellLine = m_nextSpellLine;
                GameLiving nextSpellTarget = m_nextSpellTarget;
                // warlock
                if (nextSpell != null)
                {
                    if (nextSpell.IsSecondary)
                    {
                        GameSpellEffect effect = SpellHandler.FindEffectOnTarget(this, "Powerless");
                        if (effect == null)
                            effect = SpellHandler.FindEffectOnTarget(this, "Range");
                        if (effect == null)
                            effect = SpellHandler.FindEffectOnTarget(this, "Uninterruptable");

                        if (effect != null)
                            effect.Cancel(false);
                    }

                }
                m_runningSpellHandler = null;
                m_nextSpell = null;         // avoid restarting nextspell by reentrance from spellhandler
                m_nextSpellLine = null;
                m_nextSpellTarget = null;

                if (nextSpell != null)
                    m_runningSpellHandler = ScriptMgr.CreateSpellHandler(this, nextSpell, nextSpellLine);
            }
            if (m_runningSpellHandler != null)
            {
                m_runningSpellHandler.CastingCompleteEvent += new CastingCompleteCallback(OnAfterSpellCastSequence);
                if (m_nextSpellTarget != null)
                    m_runningSpellHandler.CastSpell(m_nextSpellTarget);
                else
                    m_runningSpellHandler.CastSpell();
            }
        }

        public override void CastSpell(Spell spell, SpellLine line)
        {
            if (IsCrafting)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.InterruptedCrafting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                CraftTimer.Stop();
                CraftTimer = null;
                Out.SendCloseTimerWindow();
            }
            //Champion Styles

            /*
            Style StyleHandlerStyle = null;
            if (spell.SpellType == "ChampionStyleHandler")
            {
                {
                    StyleHandlerStyle = SkillBase.GetStyleByID((int)spell.Value, (int)spell.AmnesiaChance);
                }

                //  style = SkillBase.GetStyleByID((int)spell.Value, CharacterClass.ID);

                if (StyleHandlerStyle != null)
                {
                    StyleProcessor.TryToUseStyle(this, StyleHandlerStyle);
                }
                else { Out.SendMessage("That style is not implemented!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow); }
            }
            */
            //Masterlevel Styles
            if (spell.SpellType == "MLStyleHandler" || spell.SpellType == "StyleHandler" || spell.SpellType == "ChampionStyleHandler")
            {
                Style style = SkillBase.GetStyleByID((int)spell.Value, CharacterClass.ID);
                //Andraste - Vico : try to use classID=0 (easy way to implement CL Styles)
                if (style == null) style = SkillBase.GetStyleByID((int)spell.Value, 0);
                if (style != null)
                {
                    StyleProcessor.TryToUseStyle(this, style);
                }
                else { Out.SendMessage("That style is not implemented!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow); }
            }

            else if (spell.SpellType == "BodyguardHandler")
            {
                Ability ab = SkillBase.GetAbility("Bodyguard");
                IAbilityActionHandler handler = SkillBase.GetAbilityActionHandler(ab.KeyName);
                if (handler != null)
                {
                    handler.Execute(ab, this);
                    return;
                }
            }
            else
            {
                if (IsStunned)
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CastSpell.CantCastStunned"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    return;
                }
                if (IsMezzed)
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CastSpell.CantCastMezzed"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    return;
                }

                if (IsSilenced)
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CastSpell.CantCastFumblingWords"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    return;
                }

                double fumbleChance = GetModified(eProperty.SpellFumbleChance);
                fumbleChance *= 0.01;
                if (fumbleChance > 0)
                {
                    if (Util.ChanceDouble(fumbleChance))
                    {
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CastSpell.CantCastFumblingWords"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                        return;
                    }
                }

                lock (m_spellQueueAccessMonitor)
                {
                    if (m_runningSpellHandler != null)
                    {
                        if (m_runningSpellHandler.CanQueue == false)
                        {
                            m_runningSpellHandler.CasterMoves();
                            return;
                        }

                        if (spell.CastTime > 0 && !(m_runningSpellHandler is ChamberSpellHandler) && spell.SpellType != "Chamber")
                        {
                            if (m_runningSpellHandler.Spell.InstrumentRequirement != 0)
                            {
                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CastSpell.AlreadyPlaySong"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (SpellQueue)
                            {
                                if (spell.SpellType.ToLower() == "archery")
                                {
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CastSpell.FollowSpell", spell.Name), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                                }
                                else
                                {
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CastSpell.AlreadyCastFollow"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                }

                                m_nextSpell = spell;
                                m_nextSpellLine = line;
                                m_nextSpellTarget = TargetObject as GameLiving;
                            }
                            else Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CastSpell.AlreadyCastNoQueue"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            return;
                        }
                        else if (m_runningSpellHandler is PrimerSpellHandler)
                        {
                            if (!spell.IsSecondary)
                            {
                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CastSpell.OnlyASecondarySpell"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            }
                            else
                            {
                                if (SpellQueue && !(m_runningSpellHandler is ChamberSpellHandler))
                                {
                                    Spell cloneSpell = null;
                                    if (m_runningSpellHandler is PowerlessSpellHandler)
                                    {
                                        cloneSpell = spell.Copy();
                                        cloneSpell.CostPower = false;
                                        m_nextSpell = cloneSpell;
                                        m_nextSpellLine = line;
                                    }
                                    else if (m_runningSpellHandler is RangeSpellHandler)
                                    {
                                        cloneSpell = spell.Copy();
                                        cloneSpell.CostPower = false;
                                        cloneSpell.OverrideRange = m_runningSpellHandler.Spell.Range;
                                        m_nextSpell = cloneSpell;
                                        m_nextSpellLine = line;
                                    }
                                    else if (m_runningSpellHandler is UninterruptableSpellHandler)
                                    {
                                        cloneSpell = spell.Copy();
                                        cloneSpell.CostPower = false;
                                        m_nextSpell = cloneSpell;
                                        m_nextSpellLine = line;
                                    }
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CastSpell.PrepareSecondarySpell"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                }
                                return;
                            }
                        }
                        else if (m_runningSpellHandler is ChamberSpellHandler)
                        {
                            ChamberSpellHandler chamber = (ChamberSpellHandler)m_runningSpellHandler;
                            if (IsMoving || IsStrafing)
                            {
                                m_runningSpellHandler = null;
                                return;
                            }
                            if (spell.IsPrimary)
                            {
                                if ((spell.SpellType == "Bolt" || spell.SpellType == "BoltSpec" || spell.SpellType == "WLBolt") && !chamber.Spell.AllowBolt)
                                {
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CastSpell.SpellNotInChamber"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                    return;
                                }
                                if (chamber.PrimarySpell == null)
                                {
                                    Spell cloneSpell = spell.Copy();
                                    cloneSpell.InChamber = true;
                                    cloneSpell.CostPower = false;
                                    chamber.PrimarySpell = cloneSpell;
                                    chamber.PrimarySpellLine = line;
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CastSpell.SpellInChamber", spell.Name, ((ChamberSpellHandler)m_runningSpellHandler).Spell.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CastSpell.SelectSecondSpell", ((ChamberSpellHandler)m_runningSpellHandler).Spell.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                                else
                                {
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CastSpell.SpellNotInChamber"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                }
                            }
                            else if (spell.IsSecondary)
                            {
                                if (chamber.PrimarySpell != null)
                                {
                                    if (chamber.SecondarySpell == null)
                                    {
                                        Spell cloneSpell = spell.Copy();
                                        cloneSpell.CostPower = false;
                                        cloneSpell.InChamber = true;
                                        cloneSpell.OverrideRange = chamber.PrimarySpell.Range;
                                        chamber.SecondarySpell = cloneSpell;
                                        chamber.SecondarySpellLine = line;
                                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CastSpell.SpellInChamber", spell.Name, ((ChamberSpellHandler)m_runningSpellHandler).Spell.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    else
                                    {
                                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CastSpell.AlreadyChosenSpells"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                    }
                                }
                                else
                                {
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CastSpell.PrimarySpellFirst"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                }
                            }

                        }
                        else if (!(m_runningSpellHandler is ChamberSpellHandler) && spell.SpellType == "Chamber")
                        {
                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CastSpell.NotAFollowSpell"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            return;
                        }
                    }
                }
                ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(this, spell, line);
                if (spellhandler != null)
                {
                    if (spell.CastTime > 0)
                    {
                        GameSpellEffect effect = SpellHandler.FindEffectOnTarget(this, "Chamber", spell.Name);

                        if (effect != null && spell.Name == effect.Spell.Name)
                        {
                            spellhandler.CastSpell();
                        }
                        else
                        {
                            if (spellhandler is ChamberSpellHandler && m_runningSpellHandler == null)
                            {
                                ((ChamberSpellHandler)spellhandler).EffectSlot = ChamberSpellHandler.GetEffectSlot(spellhandler.Spell.Name);
                                m_runningSpellHandler = spellhandler;
                                m_runningSpellHandler.CastingCompleteEvent += new CastingCompleteCallback(OnAfterSpellCastSequence);
                                spellhandler.CastSpell();
                            }
                            else if (m_runningSpellHandler == null)
                            {
                                m_runningSpellHandler = spellhandler;
                                m_runningSpellHandler.CastingCompleteEvent += new CastingCompleteCallback(OnAfterSpellCastSequence);
                                spellhandler.CastSpell();
                            }
                        }
                    }
                    else
                    {
                        if (spell.IsSecondary)
                        {
                            GameSpellEffect effect = SpellHandler.FindEffectOnTarget(this, "Powerless");
                            if (effect == null)
                                effect = SpellHandler.FindEffectOnTarget(this, "Range");
                            if (effect == null)
                                effect = SpellHandler.FindEffectOnTarget(this, "Uninterruptable");

                            if (m_runningSpellHandler == null && effect == null)
                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CastSpell.CantSpellDirectly"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            else if (m_runningSpellHandler != null)
                            {
                                if (m_runningSpellHandler.Spell.IsPrimary)
                                {
                                    lock (m_spellQueueAccessMonitor)
                                    {
                                        if (SpellQueue && !(m_runningSpellHandler is ChamberSpellHandler))
                                        {
                                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CastSpell.PrepareSecondarySpell"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                            m_nextSpell = spell;
                                            spell.OverrideRange = m_runningSpellHandler.Spell.Range;
                                            m_nextSpellLine = line;
                                        }
                                    }
                                }
                                else if (!(m_runningSpellHandler is ChamberSpellHandler))
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CastSpell.CantSpellDirectly"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            }
                            else if (effect != null)
                            {
                                Spell cloneSpell = null;
                                if (effect.SpellHandler is PowerlessSpellHandler)
                                {
                                    cloneSpell = spell.Copy();
                                    cloneSpell.CostPower = false;
                                    spellhandler = ScriptMgr.CreateSpellHandler(this, cloneSpell, line);
                                    spellhandler.CastSpell();
                                    effect.Cancel(false);
                                }
                                else if (effect.SpellHandler is RangeSpellHandler)
                                {
                                    cloneSpell = spell.Copy();
                                    cloneSpell.CostPower = false;
                                    cloneSpell.OverrideRange = effect.Spell.Range;
                                    spellhandler = ScriptMgr.CreateSpellHandler(this, cloneSpell, line);
                                    spellhandler.CastSpell();
                                    effect.Cancel(false);
                                }
                                else if (effect.SpellHandler is UninterruptableSpellHandler)
                                {
                                    cloneSpell = spell.Copy();
                                    cloneSpell.CostPower = false;
                                    spellhandler = ScriptMgr.CreateSpellHandler(this, cloneSpell, line);
                                    spellhandler.CastSpell();
                                    effect.Cancel(false);
                                }
                            }
                        }
                        else
                            spellhandler.CastSpell();
                    }
                }
                else
                {
                    Out.SendMessage(spell.Name + " not implemented yet (" + spell.SpellType + ")", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }
            }
            return;
        }

        public void CastSpell(SpellCastingAbilityHandler ab)
        {
            if (IsCrafting)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.InterruptedCrafting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                CraftTimer.Stop();
                CraftTimer = null;
                Out.SendCloseTimerWindow();
            }

            ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(this, ab.Spell, ab.SpellLine);
            if (spellhandler != null)
            {
                // Instant cast abilities should not interfere with the spell queue
                if (spellhandler.Spell.CastTime > 0)
                {
                    m_runningSpellHandler = spellhandler;
                    m_runningSpellHandler.CastingCompleteEvent += new CastingCompleteCallback(OnAfterSpellCastSequence);
                }

                spellhandler.Ability = ab;
                spellhandler.CastSpell();
            }
            else
            {
                Out.SendMessage(ab.Spell.Name + " not implemented yet (" + ab.Spell.SpellType + ")", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
        }

        /// <summary>
        /// Calculate how fast this player can cast a given spell
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        public override int CalculateCastingTime(SpellLine line, Spell spell)
        {
            int ticks = spell.CastTime;

            if (spell.IsNotDexbased)
            {
                if (this is GamePlayer && (this as GamePlayer).IsCharcterClass(eCharacterClass.Vampiir))
                {
                

                    double reductionpercent = DexterityCastTimeReduction;
                    //log.ErrorFormat("reductionpercent: {0}, CastingSpeedReductionCap: {1}", reductionpercent, CastingSpeedReductionCap);

                    if (reductionpercent < 0.575)
                        reductionpercent = 0.575; //cap for vamps

                    ticks = (int)(ticks * Math.Max(CastingSpeedReductionCap, reductionpercent));
                    if (ticks < MinimumCastingSpeed)
                        ticks = MinimumCastingSpeed;
                }

                //log.Error("Is not Dexbased");
                return ticks;
            }

            //Warlock
            if ((line.KeyName == "Cursing"
                 || line.KeyName == "Cursing Spec"
                 || line.KeyName == "Hexing"
                 || line.KeyName == "Witchcraft")
                 && (spell.SpellType != "ArmorFactorBuff"
                    && spell.SpellType != "Bladeturn"
                    && spell.SpellType != "ArmorAbsorptionBuff"
                    && spell.SpellType != "MatterResistDebuff"
                    && spell.SpellType != "Uninterruptable"
                    && spell.SpellType != "Powerless"
                    && spell.SpellType != "Range"
                    && spell.Name != "Charmed Gift of Health"
                    && spell.Name != "Charmed Boon of Health"
                    && spell.Name != "Charmed Favor of Health"
                    && spell.Name != "Lesser Twisting Curse"
                    && spell.Name != "Twisting Curse Enemy"
                    && spell.Name != "Lesser Winding Curse Enemy"
                    && spell.Name != "Winding Curse Enemy"
                    && spell.Name != "Lesser Wrenching Curse Enemy"
                    && spell.Name != "Wrenching Curse Enemy"
                    && spell.Name != "Lesser Warping Curse Enemy"
                    && spell.Name != "Warping Curse Enemy"))

            {
                // log.Debug("nicht dex basierend");
                return ticks;
            }






            if (spell.InstrumentRequirement != 0 ||
                line.KeyName == GlobalSpellsLines.Item_Spells ||
                line.KeyName.StartsWith(GlobalSpellsLines.Champion_Spells))
            {
                return ticks;
            }

            if (CharacterClass.CanChangeCastingSpeed(line, spell) == false)
                return ticks;

            // no Casttime reduction for :Stormlord and Convoker Spells 
            switch (spell.SpellType)
            {
                case "SummonWood":
                case "SummonTitan":
                case "PrescienceNode":
                case "SummonWarcrystal":
                case "DissonanceTrap":
                case "DazzlingArray":
                case "EnervatingGas":
                case "InebriatingFumes":
                case "MentalSiphon":
                case "ChokingVapors":
                case "SenseDullingCloud":
                case "EnergyTempest":
                    return ticks;
            }


            if (EffectList.GetOfType<QuickCastEffect>() != null)
            {

                // Most casters have access to the Quickcast ability (or the Necromancer equivalent, Facilitate Painworking).
                // This ability will allow you to cast a spell without interruption.
                // http://support.darkageofcamelot.com/kb/article.php?id=022

                // A: You're right. The answer I should have given was that Quick Cast reduces the time needed to cast to a flat two seconds,
                // and that a spell that has been quick casted cannot be interrupted. ...
                // http://www.camelotherald.com/news/news_article.php?storyid=1383
                if (SpellHandler.FindEffectOnTarget(this, "Amnesia") == null)
                    return 2000;
                else
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "Amnesia.MessageToTarget"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return ticks;
            }
            double percent = DexterityCastTimeReduction;

            percent *= 1.0 - GetModified(eProperty.CastingSpeed) * 0.01;

            ticks = (int)(ticks * Math.Max(CastingSpeedReductionCap, percent));
            if (ticks < MinimumCastingSpeed)
                ticks = MinimumCastingSpeed;

            return ticks;
        }


        #endregion

        #region Realm Abilities
        /// <summary>
        /// This is the timer used to count time when a player casts a RA
        /// </summary>
        private RegionTimer m_realmAbilityCastTimer;

        /// <summary>
        /// Get and set the RA cast timer
        /// </summary>
        public RegionTimer RealmAbilityCastTimer
        {
            get { return m_realmAbilityCastTimer; }
            set { m_realmAbilityCastTimer = value; }
        }

        /// <summary>
        /// Does the player is casting a realm ability
        /// </summary>
        public bool IsCastingRealmAbility
        {
            get { return (m_realmAbilityCastTimer != null && m_realmAbilityCastTimer.IsAlive); }
        }
        #endregion

        #region Vault/Money/Items/Trading/UseSlot/ApplyPoison

        private IGameInventoryObject m_activeInventoryObject;

        /// <summary>
        /// The currently active InventoryObject
        /// This is new and will probably replace the above Active methods in time.
        /// </summary>
        public IGameInventoryObject ActiveInventoryObject
        {
            get { return m_activeInventoryObject; }
            set { m_activeInventoryObject = value; }
        }

        /// <summary>
        /// Property that holds tick when charged item was used last time
        /// </summary>
        public const string LAST_CHARGED_ITEM_USE_TICK = "LastChargedItemUsedTick";
        public const string ITEM_USE_DELAY = "ItemUseDelay";
        public const string ITEM_USE_DELAY_ARTIFACT = "ItemUseDelayArtifact";
        long m_newReUseTime = 0;

        /// <summary>
        /// Gets the NewReUseTime
        /// </summary>
        public long NewReUseTime
        {
            get { return m_newReUseTime; }
            set { m_newReUseTime = value; }
        }





        public const string NEXT_POTION_AVAIL_TIME = "LastPotionItemUsedTick";
        public const string NEXT_SPELL_AVAIL_TIME_BECAUSE_USE_POTION = "SpellAvailableTime";

        /// <summary>
        /// Called when this player receives a trade item
        /// </summary>
        /// <param name="source">the source of the item</param>
        /// <param name="item">the item</param>
        /// <returns>true to accept, false to deny the item</returns>
        public virtual bool ReceiveTradeItem(GamePlayer source, InventoryItem item)
        {
            if (source == null || item == null || source == this)
                return false;

            lock (m_LockObject)
            {
                lock (source)
                {
                    if ((TradeWindow != null && source != TradeWindow.Partner) || (TradeWindow == null && !OpenTrade(source)))
                    {
                        if (TradeWindow != null)
                        {
                            GamePlayer partner = TradeWindow.Partner;
                            if (partner == null)
                            {
                                source.Out.SendMessage(Name + " is still selfcrafting.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            else
                            {
                                source.Out.SendMessage(Name + " is still trading with " + partner.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                        }
                        else if (source.TradeWindow != null)
                        {
                            GamePlayer sourceTradePartner = source.TradeWindow.Partner;
                            if (sourceTradePartner == null)
                            {
                                source.Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ReceiveTradeItem.StillSelfcrafting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            else
                            {
                                source.Out.SendMessage("You are still trading with " + sourceTradePartner.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                        }
                        return false;
                    }

                    //trade while crafing
                    if (source.CraftTimer != null && source.CraftTimer.IsAlive)
                    {
                        source.Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ReceiveTradeItemWhileCrafting.CantTrade"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return false;
                    }

                    if (item.Id_nb == "GameRelic" || item.IsTradable == false && source.CanTradeAnyItem == false && TradeWindow.Partner.CanTradeAnyItem == false)
                    {
                        source.Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ReceiveTradeItem.CantTrade"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return false;
                    }

                    if (!source.TradeWindow.AddItemToTrade(item))
                    {
                        source.Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ReceiveTradeItem.CantTrade"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                    return true;
                }
            }
        }

        /// <summary>
        /// Called when the player receives trade money
        /// </summary>
        /// <param name="source">the source</param>
        /// <param name="money">the money value</param>
        /// <returns>true to accept, false to deny</returns>
        public virtual bool ReceiveTradeMoney(GamePlayer source, long money)
        {
            if (source == null || source == this || money == 0)
                return false;

            lock (m_LockObject)
            {
                lock (source)
                {
                    if ((TradeWindow != null && source != TradeWindow.Partner) || (TradeWindow == null && !OpenTrade(source)))
                    {
                        if (TradeWindow != null)
                        {
                            GamePlayer partner = TradeWindow.Partner;
                            if (partner == null)
                            {
                                source.Out.SendMessage(Name + " is still selfcrafting.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            else
                            {
                                source.Out.SendMessage(Name + " is still trading with " + partner.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                        }
                        else if (source.TradeWindow != null)
                        {
                            GamePlayer sourceTradePartner = source.TradeWindow.Partner;
                            if (sourceTradePartner == null)
                            {
                                source.Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ReceiveTradeItem.StillSelfcrafting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            else
                            {
                                source.Out.SendMessage("You are still trading with " + sourceTradePartner.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                        }
                        return false;
                    }

                    source.TradeWindow.AddMoneyToTrade(money);
                    return true;
                }
            }
        }

        /// <summary>
        /// Gets the money value this player owns
        /// </summary>
        /// <returns></returns>
        public virtual long GetCurrentMoney()
        {
            return Money.GetMoney(DBCharacter.Mithril, DBCharacter.Platinum, DBCharacter.Gold, DBCharacter.Silver, DBCharacter.Copper);
        }

        /// <summary>
        /// Adds money to this player
        /// </summary>
        /// <param name="money">money to add</param>
        public virtual void AddMoney(long money)
        {
            AddMoney(money, null, eChatType.CT_System, eChatLoc.CL_SystemWindow);
        }

        /// <summary>
        /// Adds money to this player
        /// </summary>
        /// <param name="money">money to add</param>
        /// <param name="messageFormat">null if no message or "text {0} text"</param>
        public virtual void AddMoney(long money, string messageFormat)
        {
            AddMoney(money, messageFormat, eChatType.CT_System, eChatLoc.CL_SystemWindow);
        }

        /// <summary>
        /// Adds money to this player
        /// </summary>
        /// <param name="money">money to add</param>
        /// <param name="messageFormat">null if no message or "text {0} text"</param>
        /// <param name="ct">message chat type</param>
        /// <param name="cl">message chat location</param>
        public virtual void AddMoney(long money, string messageFormat, eChatType ct, eChatLoc cl)
        {
            long newMoney = GetCurrentMoney() + money;

            DBCharacter.Copper = Money.GetCopper(newMoney);
            DBCharacter.Silver = Money.GetSilver(newMoney);
            DBCharacter.Gold = Money.GetGold(newMoney);
            DBCharacter.Platinum = Money.GetPlatinum(newMoney);
            DBCharacter.Mithril = Money.GetMithril(newMoney);

            Out.SendUpdateMoney();

            if (messageFormat != null)
            {
                Out.SendMessage(string.Format(messageFormat, Money.GetString(money)), ct, cl);
            }
        }

        /// <summary>
        /// Removes money from the player
        /// </summary>
        /// <param name="money">money value to subtract</param>
        /// <returns>true if successfull, false if player doesn't have enough money</returns>
        public virtual bool RemoveMoney(long money)
        {
            return RemoveMoney(money, null, eChatType.CT_System, eChatLoc.CL_SystemWindow);
        }

        /// <summary>
        /// Removes money from the player
        /// </summary>
        /// <param name="money">money value to subtract</param>
        /// <param name="messageFormat">null if no message or "text {0} text"</param>
        /// <returns>true if successfull, false if player doesn't have enough money</returns>
        public virtual bool RemoveMoney(long money, string messageFormat)
        {
            return RemoveMoney(money, messageFormat, eChatType.CT_System, eChatLoc.CL_SystemWindow);
        }

        /// <summary>
        /// Removes money from the player
        /// </summary>
        /// <param name="money">money value to subtract</param>
        /// <param name="messageFormat">null if no message or "text {0} text"</param>
        /// <param name="ct">message chat type</param>
        /// <param name="cl">message chat location</param>
        /// <returns>true if successfull, false if player doesn't have enough money</returns>
        public virtual bool RemoveMoney(long money, string messageFormat, eChatType ct, eChatLoc cl)
        {
            if (money > GetCurrentMoney())
                return false;

            long newMoney = GetCurrentMoney() - money;

            DBCharacter.Mithril = Money.GetMithril(newMoney);
            DBCharacter.Platinum = Money.GetPlatinum(newMoney);
            DBCharacter.Gold = Money.GetGold(newMoney);
            DBCharacter.Silver = Money.GetSilver(newMoney);
            DBCharacter.Copper = Money.GetCopper(newMoney);

            Out.SendUpdateMoney();

            if (messageFormat != null && money != 0)
            {
                Out.SendMessage(string.Format(messageFormat, Money.GetString(money)), ct, cl);
            }
            return true;
        }

        private InventoryItem m_useItem;

        /// <summary>
        /// The item the player is trying to use.
        /// </summary>
        public InventoryItem UseItem
        {
            get { return m_useItem; }
            set { m_useItem = value; }
        }

        /// <summary>
        /// Called when the player uses an inventory in a slot
        /// eg. by clicking on the icon in the qickbar dragged from a slot
        /// </summary>
        /// <param name="slot">inventory slot used</param>
        /// <param name="type">type of slot use (0=simple click on icon, 1=use, 2=/use2)</param>
        public virtual void UseSlot(eInventorySlot slot, eUseType type)
        {
            UseSlot((int)slot, (int)type);
        }

        public virtual void UseSlot(int slot, int type)
        {
            if (!IsAlive)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.UseSlot.CantFire"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            lock (Inventory)
            {
                InventoryItem useItem = Inventory.GetItem((eInventorySlot)slot);
                UseItem = useItem;

                if (useItem == null)
                {
                    if ((slot >= Slot.FIRSTQUIVER) && (slot <= Slot.FOURTHQUIVER))
                    {
                        Out.SendMessage("The quiver slot " + (slot - (Slot.FIRSTQUIVER) + 1).ToString() + " is empty!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                    else
                    {
                        // don't allow using empty slots
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.UseSlot.IllegalSourceObject", slot.ToString()), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                    return;
                }

                if (useItem is IGameInventoryItem)
                {

                    if ((useItem as IGameInventoryItem).Use(this))
                    {

                        return;
                    }
                }

                if (useItem.Item_Type >= (int)eInventorySlot.LeftFrontSaddleBag && useItem.Item_Type <= (int)eInventorySlot.RightRearSaddleBag)
                {
                    UseSaddleBag(useItem);
                    return;
                }

                if (useItem.Item_Type != Slot.RANGED && (slot != Slot.HORSE || type != 0))
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.UseSlot.AttemptToUse", useItem.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }

                #region Non-backpack/vault slots

                switch (slot)
                {
                    case Slot.HORSEARMOR:
                    case Slot.HORSEBARDING:
                        return;
                    case Slot.HORSE:
                        if (type == 0)
                        {
                            if (IsOnHorse)
                                IsOnHorse = false;
                            else
                            {
                                if (Level < useItem.Level)
                                {
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.UseSlot.SummonHorseLevel", useItem.Level), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    return;
                                }

                                string reason = GameServer.ServerRules.ReasonForDisallowMounting(this);
                                if (!String.IsNullOrEmpty(reason))
                                {
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client, reason), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    return;
                                }

                                if (IsSummoningMount)
                                {
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.UseSlot.StopCallingMount"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    StopWhistleTimers();
                                    return;
                                }
                                ChargeEffect effect1 = Client.Player.EffectList.GetOfType<ChargeEffect>();
                                if (effect1 != null)
                                {

                                    Out.SendMessage("Charge is active you Cannot Call your Mount!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    StopWhistleTimers();
                                    return;
                                }
                                SpeedOfSoundEffect effect2 = Client.Player.EffectList.GetOfType<SpeedOfSoundEffect>();
                                if (effect2 != null)
                                {

                                    Out.SendMessage("Speed Of Sound is active you Cannot Call your Mount!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    StopWhistleTimers();
                                    return;
                                }



                                Out.SendTimerWindow("Summoning Mount", 5);
                                foreach (GamePlayer plr in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                                {
                                    if (plr == null) continue;

                                    plr.Out.SendEmoteAnimation(this, eEmote.Horse_whistle);
                                }
                                // vampiir ~


                                GameSpellEffect effects = SpellHandler.FindEffectOnTarget(this, "VampiirSpeedEnhancement");
                                GameSpellEffect effect = SpellHandler.FindEffectOnTarget(this, "SpeedEnhancement");
                                GameSpellEffect effectss = SpellHandler.FindEffectOnTarget(this, "SpeedOfTheRealm");


                                if (effects != null)
                                    effects.Cancel(false);
                                if (effect != null)
                                    effect.Cancel(false);
                                if (effectss != null)
                                    effectss.Cancel(false);


                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.UseSlot.WhistleMount"), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                                m_whistleMountTimer = new RegionTimer(this)
                                {
                                    Callback = new RegionTimerCallback(WhistleMountTimerCallback)
                                };
                                m_whistleMountTimer.Start(5000);
                            }
                        }
                        break;
                    case Slot.RIGHTHAND:
                    case Slot.LEFTHAND:
                        if (type != 0) break;
                        if (ActiveWeaponSlot == eActiveWeaponSlot.Standard)
                            break;
                        SwitchWeapon(eActiveWeaponSlot.Standard);
                        Notify(GamePlayerEvent.UseSlot, this, new UseSlotEventArgs(slot, type));
                        return;

                    case Slot.TWOHAND:
                        if (type != 0) break;
                        if (ActiveWeaponSlot == eActiveWeaponSlot.TwoHanded)
                            break;
                        SwitchWeapon(eActiveWeaponSlot.TwoHanded);
                        Notify(GamePlayerEvent.UseSlot, this, new UseSlotEventArgs(slot, type));
                        return;

                    case Slot.RANGED:
                        bool newAttack = false;
                        if (ActiveWeaponSlot != eActiveWeaponSlot.Distance)
                        {
                            SwitchWeapon(eActiveWeaponSlot.Distance);
                        }
                        else if (!AttackState)
                        {
                            StopCurrentSpellcast();
                            StartAttack(TargetObject);
                            newAttack = true;
                        }

                        //Clean up range attack state/type if we are not in combat mode
                        //anymore
                        if (!AttackState)
                        {
                            RangedAttackState = eRangedAttackState.None;
                            RangedAttackType = eRangedAttackType.Normal;
                        }
                        if (!newAttack && RangedAttackState != eRangedAttackState.None)
                        {
                            if (RangedAttackState == eRangedAttackState.ReadyToFire)
                            {
                                RangedAttackState = eRangedAttackState.Fire;
                                StopCurrentSpellcast();
                                m_attackAction.Start(1);
                            }
                            else if (RangedAttackState == eRangedAttackState.Aim)
                            {
                                if (!TargetInView)
                                {
                                    // Don't store last target if it's not visible
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.UseSlot.CantSeeTarget"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                                else
                                {
                                    if (m_rangeAttackTarget.Target == null)
                                    {
                                        //set new target only if there was no target before
                                        RangeAttackTarget = TargetObject;
                                    }

                                    RangedAttackState = eRangedAttackState.AimFire;
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.UseSlot.AutoReleaseShot"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                            }
                            else if (RangedAttackState == eRangedAttackState.AimFire)
                            {
                                RangedAttackState = eRangedAttackState.AimFireReload;
                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.UseSlot.AutoReleaseShotReload"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            else if (RangedAttackState == eRangedAttackState.AimFireReload)
                            {
                                RangedAttackState = eRangedAttackState.Aim;
                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.UseSlot.NoAutoReleaseShotReload"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                        }
                        break;
                    case Slot.FIRSTQUIVER: SwitchQuiver(eActiveQuiverSlot.First, false); break;
                    case Slot.SECONDQUIVER: SwitchQuiver(eActiveQuiverSlot.Second, false); break;
                    case Slot.THIRDQUIVER: SwitchQuiver(eActiveQuiverSlot.Third, false); break;
                    case Slot.FOURTHQUIVER: SwitchQuiver(eActiveQuiverSlot.Fourth, false); break;
                }

                #endregion

                if (useItem.SpellID != 0 || useItem.SpellID1 != 0 || useItem.PoisonSpellID != 0) // don't return without firing events
                {
                    if (IsSitting)
                    {
                        Out.SendMessage("You can't use an item while sitting!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }


                    // Item with a non-charge ability.

                    if (useItem.Object_Type == (int)eObjectType.Magical
                        && useItem.Item_Type == (int)eInventorySlot.FirstBackpack
                        && useItem.SpellID > 0
                        && useItem.MaxCharges == 0)
                    {
                        UseMagicalItem(useItem, type);
                        return;
                    }

                    // Artifacts don't require charges.

                    if ((type < 2 && useItem.SpellID > 0 && useItem.Charges < 1 && !(useItem is InventoryArtifact || useItem.PackageID == "Endless" || useItem.PackageID == "Artifact")) ||
                        (type == 2 && useItem.SpellID1 > 0 && useItem.Charges1 < 1 && !(useItem is InventoryArtifact || useItem.PackageID == "Endless" || useItem.PackageID == "Artifact")) ||
                        (useItem.PoisonSpellID > 0 && useItem.PoisonCharges < 1))
                    {
                        Out.SendMessage("The " + useItem.Name + " is out of charges.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
                    else
                    {
                        if (useItem.Object_Type == (int)eObjectType.Poison)
                        {
                            InventoryItem mainHand = AttackWeapon;
                            InventoryItem leftHand = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
                            if (mainHand != null && mainHand.PoisonSpellID == 0)
                            {
                                ApplyPoison(useItem, mainHand);
                            }
                            else if (leftHand != null && leftHand.PoisonSpellID == 0)
                            {
                                ApplyPoison(useItem, leftHand);
                            }
                        }
                        else if (useItem.SpellID > 0 && useItem.Charges > 0 && useItem.Object_Type == (int)eObjectType.Magical && (useItem.Item_Type == (int)eInventorySlot.FirstBackpack || useItem.Item_Type == 41))
                        {
                            SpellLine potionEffectLine = SkillBase.GetSpellLine(GlobalSpellsLines.Potions_Effects);

                            if (useItem.Item_Type == 41)
                            {
                                potionEffectLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
                            }

                            Spell spell = SkillBase.FindSpell(useItem.SpellID, potionEffectLine);

                            if (spell != null)
                            {

                               
                                // For potions most can be used by any player level except a few higher level ones.
                                // So for the case of potions we will only restrict the level of usage if LevelRequirement is >0 for the item

                                long nextPotionAvailTime = TempProperties.getProperty<long>(NEXT_POTION_AVAIL_TIME + "_Type" + (spell.SharedTimerGroup));

                                if (Client.Account.PrivLevel == 1 && nextPotionAvailTime > CurrentRegion.Time && (useItem.PackageID == "Elixir" == false))
                                {
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.UseSlot.MustWaitBeforeUse", (nextPotionAvailTime - CurrentRegion.Time) / 1000), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                                else
                                {
                                    if (potionEffectLine != null)
                                    {
                                        int requiredLevel = useItem.Template.LevelRequirement > 0 ? useItem.Template.LevelRequirement : Math.Min(MaxLevel, useItem.Level);

                                        if (requiredLevel <= Level)
                                        {
                                            if (spell.CastTime > 0 && AttackState)
                                            {
                                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.UseSlot.CantUseInCombat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                            }
                                            //Eden
                                            else if ((IsStunned && !(Steed != null && Steed.Name == "Forceful Zephyr")) || IsMezzed || !IsAlive)
                                            {
                                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.UseSlot.CantUseState", useItem.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                            }
                                            else if (spell.CastTime > 0 && IsCasting)
                                            {
                                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.UseSlot.CantUseCast", useItem.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                            }
                                            else
                                            {
                                                SpellHandler spellHandler = ScriptMgr.CreateSpellHandler(this, spell, potionEffectLine) as SpellHandler;
                                                if (spellHandler != null)
                                                {
                                                    GameLiving target = TargetObject as GameLiving;

                                                    // Tobz: make sure we have the appropriate target for our charge spell,
                                                    // otherwise don't waste a charge.
                                                    if (spell.Target.ToLower() == "enemy")
                                                    {


                                                        if (!this.IsWithinRadius(target, (spellHandler as SpellHandler).CalculateSpellRange()))
                                                        {
                                                            // not allowed to attack for highter range as allowed.
                                                            // Out.SendMessage("That target is too far away!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.SendCastMessage.ThatTargetIsToFahr"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                                            return;
                                                        }

                                                        // we need an enemy target.
                                                        if (!GameServer.ServerRules.IsAllowedToAttack(this, target, true))
                                                        {
                                                            // not allowed to attack, so they are not an enemy.
                                                            //Out.SendMessage("You need a target for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.SendCastMessage.ThatTargetIsNotValid"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                                            return;
                                                        }
                                                    }

                                                    Stealth(false);

                                                    if (useItem.Item_Type == (int)eInventorySlot.FirstBackpack)
                                                    {
                                                        Emote(eEmote.Drink);

                                                        if (spell.CastTime > 0)
                                                            TempProperties.setProperty(NEXT_SPELL_AVAIL_TIME_BECAUSE_USE_POTION, 6 * 1000 + CurrentRegion.Time);
                                                    }

                                                    //Spell
                                                    if (spellHandler.StartSpell(target, useItem))
                                                    {

                                                        if (useItem.Count > 1)
                                                        {
                                                            Inventory.RemoveCountFromStack(useItem, 1);
                                                            //InventoryLogging.LogInventoryAction(this, "(potion)", eInventoryActionType.Other, useItem.Template);
                                                        }
                                                        else
                                                        {
                                                            useItem.Charges--;
                                                            if (useItem.Charges < 1)
                                                            {
                                                                Inventory.RemoveCountFromStack(useItem, 1);
                                                                //InventoryLogging.LogInventoryAction(this, "(potion)", eInventoryActionType.Other, useItem.Template);
                                                            }
                                                        }
                                                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.UseSlot.Used", useItem.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                                                        TempProperties.setProperty(NEXT_POTION_AVAIL_TIME + "_Type" + (spell.SharedTimerGroup), useItem.CanUseEvery * 1000 + CurrentRegion.Time);
                                                    }
                                                    else
                                                    {
                                                        // StartItemSpell is responsible for sending failure message to player
                                                    }
                                                }
                                                else
                                                {
                                                    Out.SendMessage("Potion effect ID " + spell.ID.ToString() + " is not implemented yet.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.UseSlot.NotEnouthPower"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                        }
                                    }
                                    else
                                    {
                                        Out.SendMessage("Potion effect line not found", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                }
                            }
                            else
                            {
                                Out.SendMessage("Potion effect spell ID " + useItem.SpellID.ToString() + " not found.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                        }
                        else if (type > 0)
                        {
                            if (!Inventory.EquippedItems.Contains(useItem))
                            {
                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.UseSlot.CantUseFromBackpack"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                            }
                            else
                            {
                                long lastChargedItemUseTick = TempProperties.getProperty<long>(LAST_CHARGED_ITEM_USE_TICK);
                                long changeTime = CurrentRegion.Time - lastChargedItemUseTick;
                                long delay = TempProperties.getProperty<long>(ITEM_USE_DELAY, 0L);
                                //reuse for Artifacts
                                long delay_artifact = TempProperties.getProperty<long>(ITEM_USE_DELAY_ARTIFACT, 0L);

                                long itemdelay = TempProperties.getProperty<long>("ITEMREUSEDELAY" + useItem.Id_nb);
                                long itemreuse = (long)useItem.CanUseEvery * 1000;
                                long NewReUseTime = 0;
                                long timenow = 0;
                                if (itemdelay == 0)
                                    itemdelay = CurrentRegion.Time - itemreuse;

                                if ((IsStunned && !(Steed != null && Steed.Name == "Forceful Zephyr")) || IsMezzed || !IsAlive)
                                {
                                    //Out.SendMessage("In your state you can't discharge any object.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.SendCastMessage.WhileInZephyr"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                                else if (Client.Account.PrivLevel == 1 && (changeTime < delay || changeTime < delay_artifact || (CurrentRegion.Time - itemdelay) < itemreuse)) //2 minutes reuse timer
                                {
                                    if ((CurrentRegion.Time - itemdelay) < itemreuse)
                                    {

                                        // Out.SendMessage("You must wait " + (itemreuse - (CurrentRegion.Time - itemdelay)) / 1000 + " more second before discharge " + useItem.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.SendCastMessage.YouMustWait", (itemreuse - (CurrentRegion.Time - itemdelay)) / 1000, useItem.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    else
                                    {
                                        //reuse for Artifacts
                                        if (useItem.PackageID == "Artifact")
                                        {
                                            timenow = (delay_artifact - changeTime) / 1000;
                                            if (timenow < 0)
                                            {
                                                timenow = (delay - changeTime) / 1000;
                                            }
                                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.SendCastMessage.YouMustWaitAgain", timenow), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                            //Out.SendMessage("You must wait " + timenow + " more second before discharge another object!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                            return;
                                        }
                                        else if (itemdelay > 0)
                                        {
                                            if (type == 1) //use1
                                            {
                                                if (useItem.SpellID == 0)
                                                    return;

                                                UseItemCharge(useItem, type);
                                            }
                                            else if (type == 2) //use2
                                            {
                                                if (useItem.SpellID1 == 0)
                                                    return;

                                                UseItemCharge(useItem, type);
                                            }
                                        }
                                        else
                                        {
                                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.SendCastMessage.YouMustWaitNow", (delay - changeTime) / 1000), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                            //Out.SendMessage("You must wait " + (delay - changeTime) / 1000 + " more second before discharge another object!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                            return;
                                        }
                                    }

                                    /*
                                    if ((CurrentRegion.Time - itemdelay) < itemreuse)
                                    {

                                        // Out.SendMessage("You must wait " + (itemreuse - (CurrentRegion.Time - itemdelay)) / 1000 + " more second before discharge " + useItem.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.SendCastMessage.YouMustWait", (itemreuse - (CurrentRegion.Time - itemdelay)) / 1000, useItem.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    else
                                    {
                                        //reuse for Artifacts
                                        if (useItem.PackageID == "Artifact")
                                        {
                                            timenow = (delay_artifact - changeTime) / 1000;
                                            if (timenow < 0)
                                            {
                                                timenow = (delay - changeTime) / 1000;
                                            }
                                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.SendCastMessage.YouMustWaitAgain", timenow), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                            //Out.SendMessage("You must wait " + timenow + " more second before discharge another object!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                            return;
                                        }
                                        else
                                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.SendCastMessage.YouMustWaitNow", (delay - changeTime) / 1000), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                        //Out.SendMessage("You must wait " + (delay - changeTime) / 1000 + " more second before discharge another object!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }

                                    return;
                                    */
                                }
                                else
                                {
                                    //new reuse time for used Artifacts
                                    if (useItem.PackageID == "Artifact")
                                    {
                                        NewReUseTime = itemdelay;

                                    }
                                    if (type == 1) //use1
                                    {
                                        if (useItem.SpellID == 0)
                                            return;

                                        UseItemCharge(useItem, type);
                                    }
                                    else if (type == 2) //use2
                                    {
                                        if (useItem.SpellID1 == 0)
                                            return;

                                        UseItemCharge(useItem, type);
                                    }
                                }
                            }
                        }
                    }
                }


                // notify event handlers about used slot
                Notify(GamePlayerEvent.UseSlot, this, new UseSlotEventArgs(slot, type));
            }
        }



        /// <summary>
        /// Player is using a saddle bag to open up slots on a mount
        /// </summary>
        /// <param name="useItem"></param>
        protected virtual void UseSaddleBag(InventoryItem useItem)
        {
            eHorseSaddleBag bag = eHorseSaddleBag.None;

            switch ((eInventorySlot)useItem.Item_Type)
            {
                case eInventorySlot.LeftFrontSaddleBag:
                    if (ChampionLevel >= 2)
                    {
                        bag = eHorseSaddleBag.LeftFront;
                    }
                    else
                    {
                        Out.SendMessage("This saddlebag requires Champion Level 2!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                    break;
                case eInventorySlot.RightFrontSaddleBag:
                    if (ChampionLevel >= 3)
                    {
                        bag = eHorseSaddleBag.RightFront;
                    }
                    else
                    {
                        Out.SendMessage("This saddlebag requires Champion Level 3!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                    break;
                case eInventorySlot.LeftRearSaddleBag:
                    if (ChampionLevel >= 4)
                    {
                        bag = eHorseSaddleBag.LeftRear;
                    }
                    else
                    {
                        Out.SendMessage("This saddlebag requires Champion Level 4!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                    break;
                case eInventorySlot.RightRearSaddleBag:
                    if (ChampionLevel >= 5)
                    {
                        bag = eHorseSaddleBag.RightRear;
                    }
                    else
                    {
                        Out.SendMessage("This saddlebag requires Champion Level 5!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                    break;
            }

            if (bag != eHorseSaddleBag.None)
            {
                if ((DBCharacter.ActiveSaddleBags & (byte)bag) == 0)
                {
                    if (Inventory.RemoveItem(useItem))
                    {
                        //InventoryLogging.LogInventoryAction(this, "(HorseSaddleBag)", eInventoryActionType.Other, useItem.Template, useItem.Count);
                        DBCharacter.ActiveSaddleBags |= (byte)bag;
                        Out.SendSetControlledHorse(this);
                        Out.SendMessage("You've activated a saddlebag!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        SaveIntoDatabase();
                    }
                    else
                    {
                        Out.SendMessage("An error occurred while trying to activate this saddlebag!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                }
                else
                {
                    Out.SendMessage("You've already activated this saddlebag!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
            }
        }

        private const int NUM_SLOTS_PER_SADDLEBAG = 4;

        /// <summary>
        /// Can player use this horse inventory slot
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public virtual bool CanUseHorseInventorySlot(int slot)
        {
            if (Inventory.GetItem(eInventorySlot.Horse) == null)
            {
                Out.SendMessage("You must be equipped with a horse.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (IsOnHorse == false)
            {
                Out.SendMessage("You must be on your horse to use this inventory.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (ChampionLevel == 0)
            {
                Out.SendMessage("You must be a champion to use this inventory.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (slot < (int)eInventorySlot.FirstBagHorse || slot > (int)eInventorySlot.LastBagHorse || (ChampionLevel >= 5 && ActiveSaddleBags == (byte)eHorseSaddleBag.All))
            {
                return true;
            }

            try
            {
                eHorseSaddleBag saddleBagRequired = (eHorseSaddleBag)Enum.GetValues(typeof(eHorseSaddleBag)).GetValue(((slot / NUM_SLOTS_PER_SADDLEBAG) - 19)); // 1, 2, 3, or 4

                // ChatUtil.SendDebugMessage(this, string.Format("Check slot {0} if between {1} and {2}.  CL is {3}, ActiveSaddleBags is {4}, Required Bag is {5}", slot, (int)eInventorySlot.FirstBagHorse, (int)eInventorySlot.LastBagHorse, ChampionLevel, ActiveSaddleBags, saddleBagRequired));

                if ((ActiveSaddleBags & (byte)saddleBagRequired) > 0)
                {
                    if (ChampionLevel >= 2 && slot < (int)eInventorySlot.FirstBagHorse + NUM_SLOTS_PER_SADDLEBAG)
                    {
                        return true;
                    }
                    else if (ChampionLevel >= 3 && slot < (int)eInventorySlot.FirstBagHorse + NUM_SLOTS_PER_SADDLEBAG * 2)
                    {
                        return true;
                    }
                    else if (ChampionLevel >= 4 && slot < (int)eInventorySlot.FirstBagHorse + NUM_SLOTS_PER_SADDLEBAG * 3)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                ChatUtil.SendDebugMessage(this, "CanSeeInventory: " + ex.Message);
            }

            Out.SendMessage("You can't use this inventory.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            return false;
        }


        /// <summary>
        /// Use a charged ability on an item
        /// </summary>
        /// <param name="useItem"></param>
        /// <param name="type">1 == use1, 2 == use2</param>
        protected virtual void UseItemCharge(InventoryItem useItem, int type)
        {
            int requiredLevel = useItem.Template.LevelRequirement > 0 ? useItem.Template.LevelRequirement : Math.Min(MaxLevel, useItem.Level);

            if (requiredLevel > Level)
            {
                Out.SendMessage("You are not powerful enough to use this item's spell.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            SpellLine chargeEffectLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
            Spell spell;

            if (type == 1)
            {
                spell = SkillBase.FindSpell(useItem.SpellID, chargeEffectLine);
            }
            else
            {
                spell = SkillBase.FindSpell(useItem.SpellID1, chargeEffectLine);
            }

            if (spell != null)
            {
               
               
                ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(this, spell, chargeEffectLine);
                if (spellHandler != null)
                {
                    if (IsOnHorse && !spellHandler.HasPositiveEffect)
                        IsOnHorse = false;

                    Stealth(false);

                    if (spellHandler.CastSpell())
                    {
                                               
                        bool castOk = spellHandler.StartReuseTimer;

                        if (spell.SubSpellID > 0)
                        {
                            Spell subspell = SkillBase.GetSpellByID(spell.SubSpellID);
                            if (subspell != null)
                            {
                                ISpellHandler subSpellHandler = ScriptMgr.CreateSpellHandler(this, subspell, chargeEffectLine);
                                if (subSpellHandler != null)
                                {
                                  subSpellHandler.CastSpell();
                                }
                            }
                        }

                        if (type == 1 && useItem.MaxCharges > 0)
                        {
                            useItem.Charges--;
                        }
                        else
                        {
                            if (useItem.MaxCharges1 > 0)
                            {
                                useItem.Charges1--;
                            }
                        }

                        if (castOk)
                        {
                            TempProperties.setProperty(LAST_CHARGED_ITEM_USE_TICK, CurrentRegion.Time);

                            if (useItem.PackageID == "Artifact" == false)
                            {
                                TempProperties.setProperty(ITEM_USE_DELAY, (long)(60000 * 2));
                            }
                            //last used Artifact reuse time
                            if (useItem.PackageID == "Artifact" == true)
                            {
                                TempProperties.setProperty(ITEM_USE_DELAY_ARTIFACT, (long)(m_newReUseTime));
                            }

                            TempProperties.setProperty("ITEMREUSEDELAY" + useItem.Id_nb, CurrentRegion.Time);
                        }
                    }
                }
                else
                {
                    Out.SendMessage("Charge effect ID " + spell.ID.ToString() + " is not implemented yet.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
            }
            else
            {
                if (type == 1)
                {
                    Out.SendMessage("Charge effect ID " + useItem.SpellID.ToString() + " not found.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                else
                {
                    Out.SendMessage("Charge effect ID " + useItem.SpellID1.ToString() + " not found.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
            }
        }


        /// <summary>
        /// Use a magical item's spell.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="type"></param>
        protected virtual bool UseMagicalItem(InventoryItem item, int type)
        {
            if (item == null)
                return false;

            int cooldown = item.CanUseAgainIn;
            if (cooldown > 0 && Client.Account.PrivLevel == (uint)ePrivLevel.Player)
            {
                int minutes = cooldown / 60;
                int seconds = cooldown % 60;
                Out.SendMessage(String.Format("You must wait {0} to discharge this item!",
                                              (minutes <= 0)
                                              ? String.Format("{0} more seconds", seconds.ToString())
                                              : String.Format("{0} more minutes and {1} seconds",
                                                              minutes.ToString(), seconds.ToString())),
                                eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return false;
            }


            //Eden
            if (IsMezzed || (IsStunned && !(Steed != null && Steed.Name == "Forceful Zephyr")) || !IsAlive)
            {
                //Out.SendMessage("You can't use anything in your state.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.SendCastMessage.YourStateWhileInZephyr"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (m_runningSpellHandler != null)
            {
                //Out.SendMessage("You are already casting a spell.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.SendCastMessage.YouAlreadyCasting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            SpellLine itemSpellLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);

            if (itemSpellLine == null)
                return false;

            if (type == 1 || type == 0)
            {
                Spell spell = SkillBase.FindSpell(item.SpellID, itemSpellLine);

                if (spell != null)
                {
                    int requiredLevel = item.Template.LevelRequirement > 0 ? item.Template.LevelRequirement : Math.Min(MaxLevel, item.Level);

                    if (requiredLevel > Level)
                    {
                        //Out.SendMessage("You are not powerful enough to use this item's spell.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.SendCastMessage.YouNotPowerfulEnough"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return false;
                    }

                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.SendCastMessage.YouUseItem", item.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    //Out.SendMessage(String.Format("You use {0}.", item.GetName(0, false)), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);

                    ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(this, spell, itemSpellLine);
                    if (spellHandler == null)
                        return false;

                    if (IsOnHorse && !spellHandler.HasPositiveEffect)
                        IsOnHorse = false;

                    Stealth(false);

                    if (spellHandler.CheckBeginCast(TargetObject as GameLiving))
                    {
                        TempProperties.setProperty(LAST_USED_ITEM_SPELL, item);
                        m_runningSpellHandler = spellHandler;
                        m_runningSpellHandler.CastingCompleteEvent += new CastingCompleteCallback(OnAfterSpellCastSequence);
                        spellHandler.CastSpell(item);
                        return true;
                    }
                }
            }
            return false;
        }
        GameNPC npc = null;


        /// <summary>
        /// Event : player moves, unkown zone
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void PlayerOutOfZone(DOLEvent e, object sender, EventArgs args)
        {
            ushort relRegion = 0;
            int relX = 0;
            int relY = 0;
            int relZ = 0;


            sender = this;
            if (this == null) return;
            if (e == GamePlayerEvent.Moving)
            {
                if (log.IsErrorEnabled && OutOfRegion == false)
                {
                    log.Error(Name + "'s position in unknown zone!");
                    OutOfRegion = true;
                }
                if (IsRiding && Steed is GameNPC)
                {
                    npc = Steed;
                    if (npc != null)
                    {
                        npc.StopMoving();

                        TempProperties.setProperty(PlayerPort, this);
                        DismountSteed(true);
                    }
                }

                switch (Realm)
                {
                    case eRealm.Albion:
                        {
                            relRegion = 1; // Cotswold
                            relX = 8192 + 553251;
                            relY = 8192 + 502936;
                            relZ = 2280;
                            break;
                        }
                    case eRealm.Midgard:
                        {
                            relRegion = 100; // Mularn
                            relX = 8192 + 795621;
                            relY = 8192 + 719590;
                            relZ = 4680;
                            break;
                        }
                    case eRealm.Hibernia:
                        {
                            relRegion = 200; // MagMell
                            relX = 8192 + 338652;
                            relY = 8192 + 482335;
                            relZ = 5200;
                            break;
                        }
                }




                MoveTo(relRegion, relX, relY, relZ, 0);

                GameEventMgr.RemoveHandler(this, GamePlayerEvent.Moving, new DOLEventHandler(PlayerOutOfZone));
                OutOfRegion = false;
                return;

            }
        }


        /// <summary>
        /// Apply poison to weapon
        /// </summary>
        /// <param name="poisonPotion"></param>
        /// <param name="toItem"></param>
        /// <returns>true if applied</returns>
        public bool ApplyPoison(InventoryItem poisonPotion, InventoryItem toItem)
        {
            if (poisonPotion == null || toItem == null) return false;
            int envenomSpec = GetModifiedSpecLevel(Specs.Envenom);
            if (envenomSpec < 1)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ApplyPoison.CantUsePoisons"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }
            if (!GlobalConstants.IsWeapon(toItem.Object_Type))
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ApplyPoison.PoisonsAppliedWeapons"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }
            if (!HasAbilityToUseItem(toItem.Template))
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ApplyPoison.CantPoisonWeapon"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }
            if (envenomSpec < poisonPotion.Level)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ApplyPoison.CantUsePoison"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }
            if (InCombat)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ApplyPoison.CantApplyRecentCombat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (toItem.PoisonSpellID != 0)
            {
                bool canApply = false;
                SpellLine poisonLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mundane_Poisons);
                if (poisonLine != null)
                {
                    List<Spell> spells = SkillBase.GetSpellList(poisonLine.KeyName);
                    foreach (Spell spl in spells)
                    {
                        if (spl.ID == toItem.PoisonSpellID)
                        {
                            canApply = true;
                            break;
                        }
                    }
                }
                if (canApply == false)
                {
                    Out.SendMessage(string.Format("You can't poison your {0}!", toItem.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return false;
                }
            }

            // Apply poison effect to weapon
            if (poisonPotion.PoisonSpellID != 0)
            {
                toItem.PoisonCharges = poisonPotion.PoisonCharges;
                toItem.PoisonMaxCharges = poisonPotion.PoisonMaxCharges;
                toItem.PoisonSpellID = poisonPotion.PoisonSpellID;
            }
            else
            {
                toItem.PoisonCharges = poisonPotion.Template.PoisonCharges;
                toItem.PoisonMaxCharges = poisonPotion.Template.PoisonMaxCharges;
                toItem.PoisonSpellID = poisonPotion.Template.PoisonSpellID;
            }
            Inventory.RemoveCountFromStack(poisonPotion, 1);
            //InventoryLogging.LogInventoryAction(this, "(poison)", eInventoryActionType.Other, poisonPotion.Template);
            Out.SendMessage(string.Format("You apply {0} to {1}.", poisonPotion.GetName(0, false), toItem.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            return true;
        }

        #endregion

        #region Send/Say/Yell/Whisper

        public bool IsIgnoring(GameLiving source)
        {
            if (source is GamePlayer)
            {
                var sender = source as GamePlayer;
                foreach (string Name in IgnoreList)
                {
                    if (sender.Name == Name && sender.Client.Account.PrivLevel < 2)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Delegate to be called when this player receives a text
        /// by someone sending something
        /// </summary>
        public delegate bool SendReceiveHandler(GamePlayer source, GamePlayer receiver, string str);

        /// <summary>
        /// Delegate to be called when this player is about to send a text
        /// </summary>
        public delegate bool SendHandler(GamePlayer source, GamePlayer receiver, string str);

        /// <summary>
        /// Event that is fired when the Player is about to send a text
        /// </summary>
        public event SendHandler OnSend;

        /// <summary>
        /// Event that is fired when the Player receives a Send text
        /// </summary>
        public event SendReceiveHandler OnSendReceive;

        /// <summary>
        /// Clears all send event handlers
        /// </summary>
        public void ClearOnSend()
        {
            if (this == null)
            {
                OnSend = null;
            }
        }

        /// <summary>
        /// Clears all OnSendReceive event handlers
        /// </summary>
        public void ClearOnSendReceive()
        {
            if (this == null)
            {
                OnSendReceive = null;
            }
        }

        /// <summary>
        /// This function is called when the Player receives a sent text
        /// </summary>
        /// <param name="source">GamePlayer that was sending</param>
        /// <param name="str">string that was sent</param>
        /// <returns>true if the string was received successfully, false if it was not received</returns>
        public virtual bool PrivateMessageReceive(GamePlayer source, string str)
        {
            var onSendReceive = OnSendReceive;
            if (onSendReceive != null && !onSendReceive(source, this, str))
                return false;

            if (IsIgnoring(source))
                return true;

            eChatType type = eChatType.CT_Send;
            if (source.Client.Account.PrivLevel > 1)
                type = eChatType.CT_Staff;

            if (GameServer.ServerRules.IsAllowedToUnderstand(source, this))
                Out.SendMessage(LanguageMgr.GetTranslation("EN", "GamePlayer.SendReceive.Sends", source.Name, str), type,
                                eChatLoc.CL_ChatWindow);
            else
            {
                Out.SendMessage(LanguageMgr.GetTranslation("EN", "GamePlayer.SendReceive.FalseLanguage", source.Name),
                                eChatType.CT_Send, eChatLoc.CL_ChatWindow);
                return true;
            }

            var afkmessage = TempProperties.getProperty<string>(AFK_MESSAGE);
            if (afkmessage != null)
            {
                if (afkmessage == String.Empty)
                {
                    source.Out.SendMessage(LanguageMgr.GetTranslation("EN", "GamePlayer.SendReceive.Afk", Name),
                                           eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                else
                {
                    source.Out.SendMessage(
                        LanguageMgr.GetTranslation("EN", "GamePlayer.SendReceive.AfkMessage", Name, afkmessage), eChatType.CT_Say,
                        eChatLoc.CL_ChatWindow);
                }
            }

            return true;
        }

        /// <summary>
        /// Sends a text to a target
        /// </summary>
        /// <param name="target">The target of the send</param>
        /// <param name="str">string to send (without any "xxx sends:" in front!!!)</param>
        /// <returns>true if text was sent successfully</returns>
        public virtual bool SendPrivateMessage(GamePlayer target, string str)
        {
            if (target == null || str == null)
                return false;

            SendHandler onSend = OnSend;
            if (onSend != null && !onSend(this, target, str))
                return false;

            if (!target.PrivateMessageReceive(this, str))
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Send.target.DontUnderstandYou", target.Name),
                                eChatType.CT_Send, eChatLoc.CL_ChatWindow);
                return false;
            }

            if (Client.Account.PrivLevel == 1 && target.Client.Account.PrivLevel > 1 && target.IsAnonymous)
                return true;

            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Send.YouSendTo", str, target.Name), eChatType.CT_Send,
                            eChatLoc.CL_ChatWindow);

            return true;
        }

        /// <summary>
        /// This function is called when the Player receives a Say text!
        /// </summary>
        /// <param name="source">The source living saying something</param>
        /// <param name="str">the text that was said</param>
        /// <returns>true if received successfully</returns>
        public override bool SayReceive(GameLiving source, string str)
        {
            if (!base.SayReceive(source, str))
                return false;
            if (IsIgnoring(source))
                return true;
            if (GameServer.ServerRules.IsAllowedToUnderstand(source, this) || Properties.ENABLE_DEBUG)
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.SayReceive.Says", source.GetName(0, false), str),
                                eChatType.CT_Say, eChatLoc.CL_ChatWindow);
            else
                Out.SendMessage(
                    LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.SayReceive.FalseLanguage", source.GetName(0, false)),
                    eChatType.CT_Say, eChatLoc.CL_ChatWindow);
            return true;
        }

        /// <summary>
        /// Call this function to make the player say something
        /// </summary>
        /// <param name="str">string to say</param>
        /// <returns>true if said successfully</returns>
        public override bool Say(string str)
        {
            if (!GameServer.ServerRules.IsAllowedToSpeak(this, "talk"))
                return false;
            if (!base.Say(str))
                return false;
            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Say.YouSay", str), eChatType.CT_Say,
                            eChatLoc.CL_ChatWindow);
            return true;
        }

        /// <summary>
        /// This function is called when the player hears a yell
        /// </summary>
        /// <param name="source">the source living yelling</param>
        /// <param name="str">string that was yelled</param>
        /// <returns>true if received successfully</returns>
        public override bool YellReceive(GameLiving source, string str)
        {
            if (!base.YellReceive(source, str))
                return false;
            if (IsIgnoring(source))
                return true;
            if (GameServer.ServerRules.IsAllowedToUnderstand(source, this))
                Out.SendMessage(source.GetName(0, false) + " yells, \"" + str + "\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
            else
                Out.SendMessage(source.GetName(0, false) + " yells something in a language you don't understand.", eChatType.CT_Say,
                                eChatLoc.CL_ChatWindow);
            return true;
        }

        /// <summary>
        /// Call this function to make the player yell something
        /// </summary>
        /// <param name="str">string to yell</param>
        /// <returns>true if yelled successfully</returns>
        public override bool Yell(string str)
        {
            if (!GameServer.ServerRules.IsAllowedToSpeak(this, "yell"))
                return false;
            if (!base.Yell(str))
                return false;
            Out.SendMessage("You yell, \"" + str + "\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
            return true;
        }

        /// <summary>
        /// This function is called when the player hears a whisper
        /// from some living
        /// </summary>
        /// <param name="source">Source that was living</param>
        /// <param name="str">string that was whispered</param>
        /// <returns>true if whisper was received successfully</returns>
        public override bool WhisperReceive(GameLiving source, string str)
        {
            if (!base.WhisperReceive(source, str))
                return false;
            if (IsIgnoring(source))
                return true;
            if (GameServer.ServerRules.IsAllowedToUnderstand(source, this))
                Out.SendMessage(source.GetName(0, false) + " whispers to you, \"" + str + "\"", eChatType.CT_Say,
                                eChatLoc.CL_ChatWindow);
            else
                Out.SendMessage(source.GetName(0, false) + " whispers something in a language you don't understand.",
                                eChatType.CT_Say, eChatLoc.CL_ChatWindow);
            return true;
        }

        /// <summary>
        /// Call this function to make the player whisper to someone
        /// </summary>
        /// <param name="target">GameLiving to whisper to</param>
        /// <param name="str">string to whisper</param>
        /// <returns>true if whispered successfully</returns>
        public override bool Whisper(GameObject target, string str)
        {
            if (target == null)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Whisper.SelectTarget"), eChatType.CT_System,
                                eChatLoc.CL_ChatWindow);
                return false;
            }
            if (!GameServer.ServerRules.IsAllowedToSpeak(this, "whisper"))
                return false;
            if (!base.Whisper(target, str))
                return false;
            if (target is GamePlayer)
                Out.SendMessage("You whisper, \"" + str + "\" to " + target.GetName(0, false), eChatType.CT_Say,
                                eChatLoc.CL_ChatWindow);
            return true;
        }
        /// <summary>
        /// A message to this player from some piece of code (message to ourself)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="chatType"></param>
        public override void MessageToSelf(string message, eChatType chatType)
        {
            Out.SendMessage(message, chatType, eChatLoc.CL_SystemWindow);
        }
        /// <summary>
        /// A message from something we control, usually a pet
        /// </summary>
        /// <param name="message"></param>
        /// <param name="chatType"></param>
        public override void MessageFromControlled(string message, eChatType chatType)
        {
            MessageToSelf(message, chatType);
        }

        /// <summary>
        /// A general message from the area intended for this player.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <param name="chatType"></param>
        /// <param name="chatLocation"></param>
        public virtual void MessageFromArea(GameObject source, string message, eChatType chatType, eChatLoc chatLocation)
        {
            Out.SendMessage(message, chatType, chatLocation);
        }


        #endregion

        #region Steed

        /// <summary>
        /// Holds the GameLiving that is the steed of this player as weakreference
        /// </summary>
        protected WeakReference m_steed;
        /// <summary>
        /// Holds the Steed of this player
        /// </summary>
        public GameNPC Steed
        {
            get { return m_steed.Target as GameNPC; }
            set { m_steed.Target = value; }
        }

        /*
        /// <summary>
        /// Delegate callback to be called when the player
        /// tries to mount a steed
        /// </summary>
        public delegate bool MountSteedHandler(GamePlayer rider, GameNPC steed, bool forced);

        /// <summary>
        /// Event will be fired whenever the player tries to
        /// mount a steed
        /// </summary>
        public event MountSteedHandler OnMountSteed;
        /// <summary>
        /// Clears all MountSteed handlers
        /// </summary>
        public void ClearMountSteedHandlers()
        {
            OnMountSteed = null;
        }
        */
        /// <summary>
        /// Mounts the player onto a steed
        /// </summary>
        /// <param name="steed">the steed to mount</param>
        /// <param name="forced">true if the mounting can not be prevented by handlers</param>
        /// <returns>true if mounted successfully or false if not</returns>
        public virtual void MountSteed(GameNPC steed, bool forced)
        {
            // Sanity 'coherence' checks
            if (Steed != null)
                if (!DismountSteed(forced))
                    return;


            if (IsCasting)
            {
                if (steed is GameSiegeRam)
                {
                    IsOnHorse = false;
                    return;
                }

            }


            if (GameRelic.IsPlayerCarryingRelic(this) && IsOnHorse)
            {
                Out.SendMessage(LanguageMgr.GetTranslation((this as GamePlayer).Client.Account.Language, "Scripts.Player.Boat.YouCantUseWithRelics"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                IsOnHorse = false;
            }

            if (IsStealthed)
            {
                Stealth(false);
            }

            if (IsOnHorse)
                IsOnHorse = false;

            if (!steed.RiderMount(this, forced) && !forced)
                return;

            // if (OnMountSteed != null && !OnMountSteed(this, steed, forced) && !forced)
            //   return;

            // Standard checks, as specified in rules
            if (GameServer.ServerRules.ReasonForDisallowMounting(this) != string.Empty && !forced)
                return;

            foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                if (player == null) continue;
                player.Out.SendRiding(this, steed, false);
            }


        }

        /*
         /// <summary>
         /// Delegate callback to be called whenever the player tries
         /// to dismount from a steed
         /// </summary>
         public delegate bool DismountSteedHandler(GamePlayer rider, GameLiving steed, bool forced);

         /// <summary>
         /// Event will be fired whenever the player tries to dismount
         /// from a steed
         /// </summary>
         public event DismountSteedHandler OnDismountSteed;
         /// <summary>
         /// Clears all DismountSteed handlers
         /// </summary>
         public void ClearDismountSteedHandlers()
         {
             OnDismountSteed = null;
         }
         */
        /// <summary>
        /// Dismounts the player from it's steed.
        /// </summary>
        /// <param name="forced">true if the dismounting should not be prevented by handlers</param>
        /// <returns>true if the dismount was successful, false if not</returns>
        public virtual bool DismountSteed(bool forced)
        {
            TempProperties.setProperty(PlayerPort, this);

            if (Steed == null)
                return false;
            if (Steed.Name == "Forceful Zephyr" && !forced) return false;
            //if (OnDismountSteed != null && !OnDismountSteed(this, Steed, forced) && !forced)
            // return false;
            GameObject steed = Steed;
            if (!Steed.RiderDismount(forced, this) && !forced)
                return false;

            foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                if (player == null) continue;
                player.Out.SendRiding(this, steed, true);

            }

            return true;
        }

        /// <summary>
        /// Returns if the player is riding or not
        /// </summary>
        /// <returns>true if on a steed, false if not</returns>
        public virtual bool IsRiding
        {
            get { return Steed != null; }
        }

        public void SwitchSeat(int slot)
        {
            if (Steed == null)
                return;

            if (Steed.Riders[slot] != null)
                return;

            Out.SendMessage("You switch to seat " + slot.ToString() + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);

            GameNPC steed = Steed;
            steed.RiderDismount(true, this);
            steed.RiderMount(this, true, slot);
            foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                if (player == null) continue;
                player.Out.SendRiding(this, steed, false);
            }
        }

        #endregion

        #region Add/Move/Remove

        /// <summary>
        /// Called to create an player in the world and send the other
        /// players around this player an update
        /// </summary>
        /// <returns>true if created, false if creation failed</returns>
        public override bool AddToWorld()
        {
            //DOLConsole.WriteLine("add to world "+Name);
            if (!base.AddToWorld())
                return false;

            IsJumping = false;
            m_invulnerabilityTick = 0;
            m_healthRegenerationTimer = new RegionTimer(this);
            m_powerRegenerationTimer = new RegionTimer(this);
            m_enduRegenerationTimer = new RegionTimer(this);
            m_healthRegenerationTimer.Callback = new RegionTimerCallback(HealthRegenerationTimerCallback);
            m_powerRegenerationTimer.Callback = new RegionTimerCallback(PowerRegenerationTimerCallback);
            m_enduRegenerationTimer.Callback = new RegionTimerCallback(EnduranceRegenerationTimerCallback);
            foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                if (player == null) continue;
                if (player != this)
                    player.Out.SendPlayerCreate(this);
            }
            UpdatePlayerEquipment();

            // display message
            if (SpecPointsOk == false)
            {
                Out.SendMessage("Your Charakers Specs points total was incorrect. Its now corrected, please train again.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                SpecPointsOk = true;
            }

            //Dinberg, instance change.
            if (CurrentRegion is BaseInstance)
                ((BaseInstance)CurrentRegion).OnPlayerEnterInstance(this);

            RefreshItemBonuses();

            return true;
        }

        /// <summary>
        /// Called to remove the item from the world. Also removes the
        /// player visibly from all other players around this one
        /// </summary>
        /// <returns>true if removed, false if removing failed</returns>
        public override bool RemoveFromWorld()
        {
            if (CharacterClass.RemoveFromWorld() == false)
            {
                return false;
            }

            if (ObjectState == eObjectState.Active)
            {
                DismountSteed(true);
                if (CurrentRegion.GetZone(X, Y) == null)
                {
                    if (this is GamePlayer && this.Client.Account.PrivLevel < 3 && !(this as GamePlayer).TempProperties.getProperty("isbeingbanned", false))
                    {
                        GamePlayer player = this as GamePlayer;
                        player.TempProperties.setProperty("isbeingbanned", true);
                        player.MoveToBind();
                    }
                }
                else foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    {
                        if (player == null) continue;
                        if (player != this)
                            player.Out.SendObjectRemove(this);
                    }
            }
            if (!base.RemoveFromWorld()) return false;

            IsJumping = false;

            if (m_invulnerabilityTimer != null)
            {
                m_invulnerabilityTimer.Stop();
                m_invulnerabilityTimer = null;
            }
            Diving(WaterBreath.Normal);
            if (IsOnHorse)
                IsOnHorse = false;

            //Dinberg, instance change.
            if (CurrentRegion is BaseInstance)
                ((BaseInstance)CurrentRegion).OnPlayerLeaveInstance(this);

            return true;
        }

        /// <summary>
        /// Marks this player as deleted
        /// </summary>
        public override void Delete()
        {
            if (ObjectState == eObjectState.Deleted)
                return;

           
            //log.ErrorFormat("Wait: {0}", Client.Account.LoginWaitTime);
            if (Client.Account.PrivLevel == 1 && Client.Account.Realm != 0 && Client.Account.IsOtherRealmAllowed == false && Client.Account.LoginWaitTime > DateTime.Now && Client.ClientState == GameClient.eClientState.CharScreen)
            {
                //Do not let a player with Login timer switch the realm again. Instead disconnect him!
                Client.Out.SendPlayerQuit(true);
                Client.Disconnect();
            }

            if (Client.Account.PrivLevel == 1 && Client.ClientState == GameClient.eClientState.CharScreen)
            {
               //Don't delete again, if the char is already on Login screen!
                return;
            }
            

            
            // do some Cleanup
            CleanupOnDisconnect();

            string[] friendList = new string[]
            {
                Name
            };
            foreach (GameClient clientp in WorldMgr.GetAllPlayingClients())
            {
                if (clientp.Player.Friends.Contains(Name))
                    clientp.Out.SendRemoveFriends(friendList);
            }
            if (Group != null)
            {
                Group.RemoveMember(this);
            }
            BattleGroup mybattlegroup = (BattleGroup)this.TempProperties.getProperty<object>(BattleGroup.BATTLEGROUP_PROPERTY, null);
            if (mybattlegroup != null)
            {
                mybattlegroup.RemoveBattlePlayer(this);
            }
            if (m_guild != null)
            {
                m_guild.RemoveOnlineMember(this);
            }
            GroupMgr.RemovePlayerLooking(this);
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("({0}) player.Delete()", Name);
            }
            base.Delete();
        }


        public const string PlayerPort = "PLAYERPORT";


        /// <summary>
        /// The property to save debug mode on region change
        /// </summary>
        public const string DEBUG_MODE_PROPERTY = "Player.DebugMode";

        public bool IsRvR(ushort id)
        {
            
            {
                switch (id)
                {
                    //case 27://Isle
                    case 91://Celestius
                    case 163://new frontiers
                    case 165://cathal valley
                    case 166://caldedonia
                    case 233://Sumoner hall
                    case 234://1to4BG
                    case 235://5to9BG
                    case 236://10to14BG
                    case 237://15to19BG
                    case 238://20to24BG
                    case 239://25to29BG
                    case 240://30to34BG
                    case 241://35to39BG
                    case 242://40to44BG and Test BG
                    case 244://Frontiers RvR dungeon
                    case 245://Labyrinth
                    case 249://Darkness Falls - RvR dungeon
                    case 489://lvl5-9 Demons breach
                    case 497://MidLaunch
                    case 498://AlbLaunch
                    case 499://HibLaunch
                        return true;
                    default:
                        return false;
                }
            }
        }
        protected virtual void RemovePulseEffects(GameLiving living)
        {
            lock (living.ConcentrationEffects)
            {

                for (int i = 0; i < living.ConcentrationEffects.Count; i++)
                {
                    PulsingSpellEffect effect = ConcentrationEffects[i] as PulsingSpellEffect;
                    if (effect == null)
                        continue;

                    if (effect.SpellHandler.Spell.Pulse > 0)
                    {
                        effect.Cancel(false);
                    }
                }
            }
        }
        /// <summary>
        /// This function moves a player to a specific region and
        /// specific coordinates.
        /// </summary>
        /// <param name="regionID">RegionID to move to</param>
        /// <param name="x">X target coordinate</param>
        /// <param name="y">Y target coordinate</param>
        /// <param name="z">Z target coordinate (0 to put player on floor)</param>
        /// <param name="heading">Target heading</param>
        /// <returns>true if move succeeded, false if failed</returns>
        public override bool MoveTo(ushort regionID, int x, int y, int z, ushort heading)
        {
            if (ObjectState != GameObject.eObjectState.Active)
            {
                //ANTIHACK: Player was porting 
                TempProperties.setProperty(PlayerPort, this);

                log.ErrorFormat("player konnte nicht porten - name: {0}", Name);
                return false;
            }


                //ANTIHACK: Player was porting 
                TempProperties.setProperty(PlayerPort, this);

           

            //if we are jumping somewhere away from our house not using house.Exit
            //we need to make the server know we have left the house
            if ((CurrentHouse != null || InHouse) && CurrentHouse.RegionID != regionID)
            {
                InHouse = false;
                CurrentHouse = null;
            }
            //if we send a jump, we get off the horse
            if (IsOnHorse)
                IsOnHorse = false;
            //Get the destination region based on the ID
            Region rgn = WorldMgr.GetRegion(regionID);
            //If the region doesn't exist, return false or if they aren't allowed to zone here
            if (rgn == null || !GameServer.ServerRules.IsAllowedToZone(this, rgn))
                return false;
            //If the x,y inside this region doesn't point to a zone
            //return false
            if (rgn.GetZone(x, y) == null)
                return false;



            //RvR enter timer after realm switch
            IList<Account> PlayerAccountName = GameServer.Database.SelectObjects<Account>("`Name` = @Name", new QueryParameter("@Name", Client.Account.Name));

            foreach (Account db in PlayerAccountName)
            {

                if (Client.Player.CurrentRegion.IsRvR == false && db.LastLoginRealm != 0 && db.PrivLevel == 1 && db != null && db.IsOtherRealmAllowed == false && (byte)Realm != db.LastLoginRealm && ServerProperties.Properties.LOGIN_OTHER_RELAM_SWITCH_TIMER_MINUTES > 0)
                {
                    if (CurrentRegion != null && db.LoginWaitTime > DateTime.Now)
                    {
                        string _realm = "";

                        if (rgn.IsRvR)
                        {
                            switch (db.LastLoginRealm)
                            {
                                case 1:
                                    _realm = "Albion";
                                    break;
                                case 2:
                                    _realm = "Midgard";
                                    break;
                                case 3:
                                    _realm = "Hibernia";
                                    break;
                            }

                            string message = string.Format("Destination region {0} is not available at this time - reason: You were recently in {2}." + " You can enter RvR regions again at: {1} EU time.", CurrentRegion.Description.ToString(), db.LoginWaitTime.ToString(), _realm.ToString());


                            Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return false;
                        }
                    }
                }
            }



            Diving(WaterBreath.Normal);

            if (SiegeWeapon != null)
                SiegeWeapon.ReleaseControl();

            if (regionID != CurrentRegionID)
            {
                GameEventMgr.Notify(GamePlayerEvent.RegionChanging, this);
                if (!RemoveFromWorld())
                   return false;
              
                //notify event
                CurrentRegion.Notify(RegionEvent.PlayerLeave, CurrentRegion, new RegionPlayerEventArgs(this));

                RemovePulseEffects(this);

                if (Properties.BUFF_RANGE > 0 || Properties.BUFF_Remove_on_porting || IsRvR(regionID))
                   CancelAllConcentrationEffects(true);

                //Stop Duel if zoning
                if (DuelTarget != null)
                    DuelStop();

                if (ControlledBrain != null)
                    CommandNpcRelease();
            }
            else
            {
                //Stop Duel if zoning
                if (DuelTarget != null)
                    DuelStop();

                //Just remove the player visible, but leave his OID intact!
                //If player doesn't change region
                if (Steed != null)
                    DismountSteed(true);

                foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {

                    if (player == null) continue;
                    if (player != this)
                    {
                        player.Out.SendObjectRemove(this);
                    }
                }

                IsJumping = true;
            }
            bool hasPetToMove = false;
            //Remove the last update tick property, to prevent speedhack messages during zoning and teleporting!
            // LastPositionUpdateTick = 0;

            if (ControlledBrain != null && ControlledBrain.WalkState != eWalkState.Stay)
            {
                if (IsCharcterClass(eCharacterClass.Theurgist) == false && IsCharcterClass(eCharacterClass.Animist) == false)
                {
                    hasPetToMove = true;
                }
            }
            //Set the new destination
            //Current Speed = 0 when moved ... else X,Y,Z continue to be modified
            CurrentSpeed = 0;
            MovementStartTick = GameTimer.GetTickCount();
            Point3D originalPoint = new Point3D(X, Y, Z);
            X = x;
            Y = y;
            Z = z;
            Heading = heading;

            //Remove the last update tick property, to prevent speedhack messages during zoning and teleporting!
            TempProperties.removeProperty(PlayerPositionUpdateHandler.LASTMOVEMENTTICK);
            //TempProperties.removeProperty(PlayerPositionUpdateHandler.LASTCPSTICK);
            //If the destination is in another region


            if (regionID != CurrentRegionID)
            {
                //Set our new region
                CurrentRegionID = regionID;

                //Send the region update packet, the rest will be handled
                //by the packethandlers
                Out.SendRegionChanged();
            }
            else
            {

                //Add the player to the new coordinates
                Out.SendPlayerJump(false);

                // are we jumping far enough to force a complete refresh?
                if (GetDistanceTo(originalPoint) > WorldMgr.REFRESH_DISTANCE)
                {
                    RefreshWorld();
                }
                               
                UpdatePlayerEquipment();

                if (this.IsUnderwater)
                    this.IsDiving = true;

                if (hasPetToMove)
                {
                    Point2D point = GetPointFromHeading(Heading, 64);

                    IControlledBrain npc = ControlledBrain;
                    if (npc != null)
                    {
                        GameNPC petBody = npc.Body;

                        petBody.MoveInRegion(CurrentRegionID, point.X, point.Y, this.Z + 10, (ushort)((this.Heading + 2048) % 4096), false);

                        if (petBody != null && petBody.ControlledNpcList != null)
                        {
                            foreach (IControlledBrain icb in petBody.ControlledNpcList)
                            {
                                if (icb != null && icb.Body != null)
                                {
                                    GameNPC petBody2 = icb.Body;
                                    if (petBody2 != null && originalPoint.IsWithinRadius(petBody2, 500))
                                        petBody2.MoveInRegion(CurrentRegionID, point.X, point.Y, this.Z + 10, (ushort)((this.Heading + 2048) % 4096), false);
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// Refresh all objects around the player
        /// </summary>
        public virtual void RefreshWorld()
        {
            //log.Error("update player");
            foreach (GameNPC npc in GetNPCsInRadius(WorldMgr.VISIBILITY_DISTANCE * 2))
            {
                Out.SendNPCCreate(npc);
                if (npc.Inventory != null)
                    Out.SendLivingEquipmentUpdate(npc);
            }

            foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                if (player == null) continue;

                if (player != null)
                {
                    if (IsStealthed == false || CanDetect(player))
                    {
                        Out.SendPlayerCreate(player);
                    }

                    if (player.IsStealthed == false || player.CanDetect(this))
                    {
                        player.Out.SendPlayerCreate(this);
                    }

                    if (player.IsStealthed == false || player.CanDetect(this))
                    {
                        player.Out.SendPlayerCreate(this);
                        player.Out.SendLivingEquipmentUpdate(this);
                    }

                    if (IsStealthed == false || CanDetect(player))
                    {
                        Out.SendPlayerCreate(player);
                        Out.SendLivingEquipmentUpdate(player);
                    }
                }
            }
        }



        //Eden - Move to bind, and check if the loc is allowed
        public virtual bool MoveToBind()
        {
            Region rgn = WorldMgr.GetRegion((ushort)DBCharacter.BindRegion);
            if (rgn == null || rgn.GetZone(DBCharacter.BindXpos, DBCharacter.BindYpos) == null)
            {
                if (log.IsErrorEnabled)
                    log.ErrorFormat("Player: {0} unknown bind point.", Name);
                   //log.Error("Player: " + Name + " unknown bind point : (R/X/Y) " + DBCharacter.BindRegion.ToString() + "/" + DBCharacter.BindXpos.ToString() + "/" + DBCharacter.BindYpos.ToString());
                //Kick the player, avoid server freeze
                Client.Out.SendPlayerQuit(true);
                SaveIntoDatabase();
                Quit(true);
                //now ban him
                if (ServerProperties.Properties.BAN_HACKERS)
                {
                    DBBannedAccount b = new DBBannedAccount
                    {
                        Author = "SERVER",
                        Ip = Client.TcpEndpointAddress,
                        Account = Client.Account.Name,
                        DateBan = DateTime.Now,
                        Type = "B",
                        Reason = "X/Y/Zone : " + X.ToString() + "/" + Y.ToString() + "/" + CurrentRegion.ID.ToString()
                    };
                    GameServer.Database.AddObject(b);
                    GameServer.Database.SaveObject(b);
                    string message = "Unknown bind point, your account is banned, contact a GM.";
                    Client.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_SystemWindow);
                    Client.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_ChatWindow);
                }
                return false;
            }

            if (GameServer.ServerRules.IsAllowedToMoveToBind(this))
                return MoveTo((ushort)DBCharacter.BindRegion, DBCharacter.BindXpos, DBCharacter.BindYpos, DBCharacter.BindZpos, (ushort)DBCharacter.BindHeading);

            return false;
        }

        #endregion

        #region Group/Friendlist/guild

        private Guild m_guild;
        private DBRank m_guildRank;

        /// <summary>
        /// Gets or sets the player's guild
        /// </summary>
        public Guild Guild
        {
            get { return m_guild; }
            set
            {
                if (value == null)
                {
                    // remove this player from the online list of their current guild
                    m_guild.RemoveOnlineMember(this);
                }

                m_guild = value;

                //update guild name for all players if client is playing
                if (ObjectState == eObjectState.Active)
                {
                    Out.SendUpdatePlayer();
                    foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    {
                        if (player == null) continue;
                        if (player != this)
                        {
                            player.Out.SendObjectRemove(this);
                            player.Out.SendPlayerCreate(this);
                            player.Out.SendLivingEquipmentUpdate(this);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the player's guild rank
        /// </summary>
        public DBRank GuildRank
        {
            get { return m_guildRank; }
            set
            {
                m_guildRank = value;
                if (value != null)
                    DBCharacter.GuildRank = value.RankLevel;//maybe mistake here and need to change and make an index var
            }
        }

        /// <summary>
        /// Gets or sets the database guildid of this player
        /// (delegate to PlayerCharacter)
        /// </summary>
        public string GuildID
        {
            get { return DBCharacter != null ? DBCharacter.GuildID : string.Empty; }
            set { if (DBCharacter != null) DBCharacter.GuildID = value; }
        }

        /// <summary>
        /// Gets or sets the player's guild flag
        /// (delegate to PlayerCharacter)
        /// </summary>
        public bool ClassNameFlag
        {
            get { return DBCharacter.FlagClassName; }
            set { DBCharacter.FlagClassName = value; }
        }

        /// <summary>
        /// true if this player is looking for a group
        /// </summary>
        protected bool m_lookingForGroup;
        /// <summary>
        /// true if this player want to receive loot with autosplit between members of group
        /// </summary>
        protected bool m_autoSplitLoot = true;

        /// <summary>
        /// Gets or sets the LookingForGroup flag in this player
        /// </summary>
        public bool LookingForGroup
        {
            get { return m_lookingForGroup; }
            set { m_lookingForGroup = value; }
        }
        /// <summary>
        /// Gets/sets the autosplit for loot
        /// </summary>
        public bool AutoSplitLoot
        {
            get { return m_autoSplitLoot; }
            set { m_autoSplitLoot = value; }
        }

        /// <summary>
        /// Gets or sets the friends of this player
        /// (delegate to PlayerCharacter)
        /// </summary>
        public List<string> Friends
        {
            get
            {
                if (DBCharacter != null && DBCharacter.SerializedFriendsList != null)
                    return new List<string>(DBCharacter.SerializedFriendsList.Split(','));

                return new List<string>();
            }
            set
            {
                if (DBCharacter == null)
                    return;

                if (value == null)
                {
                    DBCharacter.SerializedFriendsList = String.Empty;
                }
                else
                {
                    DBCharacter.SerializedFriendsList = string.Join(",", value.ToArray());
                }

                GameServer.Database.SaveObject(DBCharacter);
            }
        }

        /// <summary>
        /// Gets or sets the IgnoreList of a Player
        /// (delegate to PlayerCharacter)
        /// </summary>
        public ArrayList IgnoreList
        {
            get
            {
                if (DBCharacter != null && DBCharacter.SerializedIgnoreList != null)
                    return new ArrayList(DBCharacter.SerializedIgnoreList.Split(','));
                return new ArrayList(0);
            }
            set
            {
                if (DBCharacter == null) return;
                if (value == null)
                    DBCharacter.SerializedIgnoreList = String.Empty;
                else
                    DBCharacter.SerializedIgnoreList = String.Join(",", (string[])value.ToArray(typeof(string)));
                GameServer.Database.SaveObject(DBCharacter);
            }
        }

        /// <summary>
        /// Modifies the friend list of this player
        /// </summary>
        /// <param name="friendName">the friend name</param>
        /// <param name="remove">true to remove this friend, false to add it</param>
        public void ModifyFriend(string friendName, bool remove)
        {
            var currentFriends = Friends;
            if (remove && currentFriends != null)
            {
                if (currentFriends.Contains(friendName))
                {
                    currentFriends.Remove(friendName);
                    Friends = currentFriends;
                }
            }
            else
            {
                if (!currentFriends.Contains(friendName))
                {
                    currentFriends.Add(friendName);
                    Friends = currentFriends;
                }
            }
        }


        /// <summary>
        /// Modifies the friend list of this player
        /// </summary>
        /// <param name="friendName">the friend name</param>
        /// <param name="remove">true to remove this friend, false to add it</param>
        public void ModifyIgnoreList(string Name, bool remove)
        {
            ArrayList currentIgnores = IgnoreList;
            if (remove && currentIgnores != null)
            {
                if (currentIgnores.Contains(Name))
                {
                    currentIgnores.Remove(Name);
                    IgnoreList = currentIgnores;
                }
            }
            else
            {
                if (!currentIgnores.Contains(Name))
                {
                    currentIgnores.Add(Name);
                    IgnoreList = currentIgnores;
                }
            }
        }

        #endregion

        #region X/Y/Z/Region/Realm/Position...

        /// <summary>
        /// Holds all areas this player is currently within
        /// </summary>
        private IList m_currentAreas;

        /// <summary>
        /// Holds all areas this player is currently within
        /// </summary>
        public override IList CurrentAreas
        {
            get { return m_currentAreas; }
            set { m_currentAreas = value; }
        }

        /// <summary>
        /// Property that saves last maximum Z value
        /// </summary>
        public const string MAX_LAST_Z = "max_last_z";

        /// <summary>
        /// The base speed of the player
        /// </summary>
        public const int PLAYER_BASE_SPEED = 191;

        public long m_areaUpdateTick = 0;


        /// <summary>
        /// Gets the tick when the areas should be updated
        /// </summary>
        public long AreaUpdateTick
        {
            get { return m_areaUpdateTick; }
            set { m_areaUpdateTick = value; }
        }

        public uint LastNPCUpdateTick = 0;
        public uint LastNPCUpdateAllTick = 0;

        /// <summary>
        /// Gets the current position of this player
        /// </summary>
        public override int X
        {
            set
            {
                base.X = value;
                DBCharacter.Xpos = base.X;
            }
        }

        /// <summary>
        /// Gets the current position of this player
        /// </summary>
        public override int Y
        {
            set
            {
                base.Y = value;
                DBCharacter.Ypos = base.Y;
            }
        }

        /// <summary>
        /// Gets the current position of this player
        /// </summary>
        public override int Z
        {
            set
            {
                base.Z = value;
                DBCharacter.Zpos = base.Z;
            }
        }

       
        /// <summary>
        /// Gets or sets the current speed of this player
        /// </summary>
        public override short CurrentSpeed
        {
            set
            {
                base.CurrentSpeed = value;
                if (value != 0)
                {
                    OnPlayerMove();
                }
            }
        }
       

        public void SetCurrentSpeed(short speed) => CurrentSpeed = Math.Min(speed, MaxSpeed);

        public override void UpdateMaxSpeed()
        {
            Out.SendUpdateMaxSpeed();
        }

        /// <summary>
        /// Gets or sets the region of this player
        /// </summary>
        public override Region CurrentRegion
        {
            set
            {
                base.CurrentRegion = value;
                DBCharacter.Region = CurrentRegionID;
            }
        }

        /// <summary>
        /// Holds the zone player was in after last position update
        /// </summary>
        private Zone m_lastPositionUpdateZone;

        /// <summary>
        /// Gets or sets the zone after last position update
        /// </summary>
        public Zone LastPositionUpdateZone
        {
            get { return m_lastPositionUpdateZone; }
            set { m_lastPositionUpdateZone = value; }
        }


        private long m_lastPositionUpdateTick = 0;

        /// <summary>
        /// The environment tick count when this players position was last updated
        /// </summary>
        public long LastPositionUpdateTick
        {
            get { return m_lastPositionUpdateTick; }
            set { m_lastPositionUpdateTick = value; }
        }

        private Point3D m_lastPositionUpdatePoint = new Point3D(0, 0, 0);

        /// <summary>
        /// The last recorded position of this player
        /// </summary>
        public Point3D LastPositionUpdatePoint
        {
            get { return m_lastPositionUpdatePoint; }
            set { m_lastPositionUpdatePoint = value; }
        }

        /// <summary>
        /// Holds the players max Z for fall damage
        /// </summary>
        private int m_maxLastZ;

        /// <summary>
        /// Gets or sets the players max Z for fall damage
        /// </summary>
        public int MaxLastZ
        {
            get { return m_maxLastZ; }
            set { m_maxLastZ = value; }
        }

        /// <summary>
        /// Gets or sets the realm of this player
        /// </summary>
        public override eRealm Realm
        {
            get { return DBCharacter != null ? (eRealm)DBCharacter.Realm : base.Realm; }
            set
            {
                base.Realm = value;
                DBCharacter.Realm = (byte)value;
            }
        }

        /// <summary>
        /// Gets or sets the heading of this player
        /// </summary>
        public override ushort Heading
        {
            set
            {
                base.Heading = value;
                DBCharacter.Direction = value;

                if (AttackState && ActiveWeaponSlot != eActiveWeaponSlot.Distance)
                {
                    AttackData ad = TempProperties.getProperty<object>(LAST_ATTACK_DATA, null) as AttackData;
                    if (ad != null && ad.IsMeleeAttack && (ad.AttackResult == eAttackResult.TargetNotVisible || ad.AttackResult == eAttackResult.OutOfRange))
                    {
                        //Does the target can be attacked ?
                        if (ad.Target != null && IsObjectInFront(ad.Target, 120) && this.IsWithinRadius(ad.Target, AttackRange) && m_attackAction != null)
                        {
                            m_attackAction.Start(1);
                        }
                    }
                }
            }
        }

        protected bool m_climbing;
        /// <summary>
        /// Gets/sets the current climbing state
        /// </summary>
        public bool IsClimbing
        {
            get { return m_climbing; }
            set
            {
                if (value == m_climbing) return;
                m_climbing = value;
            }
        }

        protected bool m_swimming;
        /// <summary>
        /// Gets/sets the current swimming state
        /// </summary>
        public virtual bool IsSwimming
        {
            get { return m_swimming; }
            set
            {
                if (value == m_swimming) return;
                m_swimming = value;
                Notify(GamePlayerEvent.SwimmingStatus, this);
                //Handle Lava Damage
                if (m_swimming && CurrentZone.IsLava == true)
                {
                    if (m_lavaBurningTimer == null)
                    {
                        m_lavaBurningTimer = new RegionTimer(this)
                        {
                            Callback = new RegionTimerCallback(LavaBurnTimerCallback),
                            Interval = 2000
                        };
                        m_lavaBurningTimer.Start(1);
                    }
                }
                if (!m_swimming && CurrentZone.IsLava == true && m_lavaBurningTimer != null)
                {
                    m_lavaBurningTimer.Stop();
                    m_lavaBurningTimer = null;
                }
            }
        }

        /// <summary>
        /// The stuck state of this player
        /// </summary>
        private bool m_stuckFlag = false;

        /// <summary>
        /// Gets/sets the current stuck state
        /// </summary>
        public virtual bool Stuck
        {
            get { return m_stuckFlag; }
            set
            {
                if (value == m_stuckFlag) return;
                m_stuckFlag = value;
            }
        }


        public enum WaterBreath : byte
        {
            Normal = 0,
            Holding = 1,
            Drowning = 2,
        }

        protected long m_beginDrowningTick;
        protected WaterBreath m_currentWaterBreathState;

        protected int DrowningTimerCallback(RegionTimer callingTimer)
        {
            if (!IsAlive || ObjectState != eObjectState.Active)
                return 0;
            if (this.Client.Account.PrivLevel == 1 && this != null)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.DrowningTimerCallback.CannotBreath"), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.DrowningTimerCallback.Take5%Damage"), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                if (CurrentRegion.Time - m_beginDrowningTick > 15000) // 15 sec
                {
                    TakeDamage(null, eDamageType.Natural, MaxHealth, 0);
                    Out.SendCloseTimerWindow();
                    return 0;
                }
                else
                    TakeDamage(null, eDamageType.Natural, MaxHealth / 20, 0);
            }
            return 3000;
        }

        protected int HoldingBreathTimerCallback(RegionTimer callingTimer)
        {
            m_holdBreathTimer = null;
            Diving(WaterBreath.Drowning);
            return 0;
        }

        protected RegionTimer m_drowningTimer;
        protected RegionTimer m_holdBreathTimer;
        protected RegionTimer m_lavaBurningTimer;
        /// <summary>
        /// The diving state of this player
        /// </summary>
        protected bool m_diving;
        /// <summary>
        /// Gets/sets the current diving state
        /// </summary>
        public bool IsDiving
        {
            get { return m_diving; }
            set
            {
                if (m_diving != value)
                    if (value && !CanBreathUnderWater)
                        Diving(WaterBreath.Holding);
                    else
                        Diving(WaterBreath.Normal);
                m_diving = value;
            }
        }

        protected bool m_canBreathUnderwater;
        public bool CanBreathUnderWater
        {
            get { return m_canBreathUnderwater; }
            set
            {
                m_canBreathUnderwater = value;
                if (!value && IsDiving)
                    Diving(WaterBreath.Holding);
                else
                    Diving(WaterBreath.Normal);
            }
        }

        public void Diving(WaterBreath state)
        {
            //bool changeSpeed = false;
            if (m_currentWaterBreathState != state)
            {
                //changeSpeed = true;
                Out.SendCloseTimerWindow();
            }

            if (m_holdBreathTimer != null)
            {
                m_holdBreathTimer.Stop();
                m_holdBreathTimer = null;
            }
            if (m_drowningTimer != null)
            {
                m_drowningTimer.Stop();
                m_drowningTimer = null;
            }
            switch (state)
            {
                case WaterBreath.Normal:
                    break;
                case WaterBreath.Holding:
                    if (m_holdBreathTimer == null)
                    {
                        Out.SendTimerWindow("Holding Breath", 30);
                        m_holdBreathTimer = new RegionTimer(this)
                        {
                            Callback = new RegionTimerCallback(HoldingBreathTimerCallback)
                        };
                        m_holdBreathTimer.Start(30001);
                    }
                    break;
                case WaterBreath.Drowning:
                    m_beginDrowningTick = CurrentRegion.Time;
                    if (m_drowningTimer == null)
                    {
                        Out.SendTimerWindow("Drowning", 15);
                        m_drowningTimer = new RegionTimer(this)
                        {
                            Callback = new RegionTimerCallback(DrowningTimerCallback)
                        };
                        m_drowningTimer.Start(1);
                    }
                    break;
            }
            m_currentWaterBreathState = state;
            //if (changeSpeed)
            //	Out.SendUpdateMaxSpeed();
        }

        protected int LavaBurnTimerCallback(RegionTimer callingTimer)
        {
            if (!IsAlive || ObjectState != eObjectState.Active || !IsSwimming)
                return 0;



            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.LavaBurnTimerCallback.YourInLava"), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.LavaBurnTimerCallback.Take34%Damage"), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
            if (Client.Account.PrivLevel == 1)
            {
                TakeDamage(null, eDamageType.Natural, (int)(MaxHealth * 0.34), 0);

                foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    if (player == null) continue;
                          player.Out.SendCombatAnimation(null, this, 0x0000, 0x0000, 0x00, 0x00, 0x14, HealthPercent);
                }
            }
            return 2000;
        }

        /// <summary>
        /// The sitting state of this player
        /// </summary>
        protected bool m_sitting;
        /// <summary>
        /// Gets/sets the current sit state
        /// </summary>
        public override bool IsSitting
        {
            get { return m_sitting; }
            set
            {
                m_sitting = value;
                if (value)
                {
                    if (IsCasting)
                        m_runningSpellHandler.CasterMoves();
                    if (AttackState && ActiveWeaponSlot == eActiveWeaponSlot.Distance)
                    {
                        string attackTypeMsg = "shot";
                        if (AttackWeapon.Object_Type == (int)eObjectType.Thrown)
                            attackTypeMsg = "throw";
                        Out.SendMessage("You move and interrupt your " + attackTypeMsg + "!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        StopAttack();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the max speed of this player
        /// (delegate to PlayerCharacter)
        /// </summary>
        public override short MaxSpeedBase
        {
            get { return (short)(DBCharacter != null ? DBCharacter.MaxSpeed : base.MaxSpeedBase); }
            set
            {
                base.MaxSpeedBase = value;
                DBCharacter.MaxSpeed = value;
            }
        }

        /// <summary>
        /// the moving state of this player
        /// </summary>
        public override bool IsMoving => base.IsMoving || IsStrafing;

        /// <summary>
        /// The sprint effect of this player
        /// </summary>
        protected SprintEffect m_sprintEffect = null;
        /// <summary>
        /// Gets sprinting flag
        /// </summary>
        public bool IsSprinting
        {
            get { return m_sprintEffect != null; }
        }
        /// <summary>
        /// Change sprint state of this player
        /// </summary>
        /// <param name="state">new state</param>
        /// <returns>sprint state after command</returns>
        public virtual bool Sprint(bool state)
        {
            if (state == IsSprinting)
                return state;

            if (state)
            {
                // can't start sprinting with 10 endurance on 1.68 server
                if (Endurance <= 10)
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Sprint.TooFatigued"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return false;
                }
                if (IsStealthed)
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Sprint.CantSprintHidden"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return false;
                }
                if (!IsAlive)
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Sprint.CantSprintDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return false;
                }

                m_sprintEffect = new SprintEffect();
                m_sprintEffect.Start(this);
                Out.SendUpdateMaxSpeed();
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Sprint.PrepareSprint"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return true;
            }
            else
            {
                m_sprintEffect.Stop();
                m_sprintEffect = null;
                Out.SendUpdateMaxSpeed();
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Sprint.NoLongerReady"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }
        }

        /// <summary>
        /// The strafe state of this player
        /// </summary>
        protected bool m_strafing;
        /// <summary>
        /// Gets/sets the current strafing mode
        /// </summary>
        public override bool IsStrafing
        {
            set
            {
                m_strafing = value;
                if (value)
                {
                    OnPlayerMove();
                }
            }
            get { return m_strafing; }
        }

        public virtual void OnPlayerMove()
        {
           
            if (IsSitting)
            {
               Sit(false);
            }
            if (IsCasting)
            {
                m_runningSpellHandler.CasterMoves();
            }
            if (IsCastingRealmAbility)
            {
                Out.SendInterruptAnimation(this);
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "SpellHandler.CasterMove"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                RealmAbilityCastTimer.Stop();
                RealmAbilityCastTimer = null;
            }
            if (IsCrafting)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnPlayerMove.InterruptCrafting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                CraftTimer.Stop();
                CraftTimer = null;
                Out.SendCloseTimerWindow();
            }
            if (IsSummoningMount)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnPlayerMove.CannotCallMount"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                StopWhistleTimers();
            }
            if (AttackState)
            {
                if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
                {
                    string attackTypeMsg = (AttackWeapon.Object_Type == (int)eObjectType.Thrown ? "throw" : "shot");
                    Out.SendMessage("You move and interrupt your " + attackTypeMsg + "!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    StopAttack();
                }
                else
                {
                    AttackData ad = TempProperties.getProperty<object>(LAST_ATTACK_DATA, null) as AttackData;
                    if (ad != null && ad.IsMeleeAttack && (ad.AttackResult == eAttackResult.TargetNotVisible || ad.AttackResult == eAttackResult.OutOfRange))
                    {
                        //Does the target can be attacked ?
                        if (ad.Target != null && IsObjectInFront(ad.Target, 120) && this.IsWithinRadius(ad.Target, AttackRange) && m_attackAction != null)
                        {
                            m_attackAction.Start(1);
                        }
                    }
                }
            }
            //Notify the GameEventMgr of the moving player
            GameEventMgr.Notify(GamePlayerEvent.Moving, this);
        }

        /// <summary>
        /// Sits/Stands the player
        /// </summary>
        /// <param name="sit">True if sitting, otherwise false</param>
        public virtual void Sit(bool sit)
        {
            CurrentSpeed = 0;
            Sprint(false);
            if (IsSummoningMount)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Sit.InterruptCallMount"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                StopWhistleTimers();
            }
            if (IsSitting == sit)
            {
                if (sit)
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Sit.AlreadySitting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                if (!sit)
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Sit.NotSitting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return; // already done
            }

            if (!IsAlive)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Sit.CantSitDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (IsStunned)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Sit.CantSitStunned"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (IsMezzed)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Sit.CantSitMezzed"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (sit && CurrentSpeed > 0)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Sit.MustStandingStill"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (Steed != null || IsOnHorse)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Sit.MustDismount"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (sit)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Sit.YouSitDown"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
            else
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Sit.YouStandUp"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }

            //Stop attack if you sit down while attacking
            if (sit && AttackState)
            {
                StopAttack();
            }

            if (!sit)
            {
                //Stop quit sequence if you stand up
                if (m_quitTimer != null)
                {
                    m_quitTimer.Stop();
                    m_quitTimer = null;
                    Stuck = false;
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Sit.NoLongerWaitingQuit"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                //Stop praying if the player stands up...
                if (IsPraying)
                {
                    m_prayAction.Stop();
                }
            }
            //Update the client
            if (sit && !IsSitting)
                Out.SendStatusUpdate(2);// unknown why on begin sit it send 2 in sitting flag like player begin sitting
            IsSitting = sit;
            UpdatePlayerStatus();
        }

        /// <summary>
        /// Sets the Living's ground-target Coordinates inside the current Region
        /// </summary>
        public override void SetGroundTarget(int groundX, int groundY, int groundZ)
        {
            base.SetGroundTarget(groundX, groundY, groundZ);
            Out.SendMessage(String.Format("You ground-target {0},{1},{2}", groundX.ToString(), groundY.ToString(), groundZ.ToString()), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            if (SiegeWeapon != null)
                SiegeWeapon.SetGroundTarget(groundX, groundY, groundZ);
        }

        /// <summary>
        /// Holds unique locations array
        /// </summary>
        protected readonly GameLocation[] m_lastUniqueLocations;

        /// <summary>
        /// Gets unique locations array
        /// </summary>
        public GameLocation[] LastUniqueLocations
        {
            get { return m_lastUniqueLocations; }
        }
        /// <summary>
        /// Updates Health, Mana, Sitting, Endurance, Concentration and Alive status to client
        /// </summary>
        public void UpdatePlayerStatus()
        {
            Out.SendStatusUpdate();
        }

        #endregion

        #region Equipment/Encumberance


        protected bool m_oppressionEncumberance;

        /// <summary>
        ///Banelord-3 Oppression active ?
        /// </summary>
        public bool OppressionEncumberance
        {
            get { return m_oppressionEncumberance; }
            set { m_oppressionEncumberance = value; }
        }

        /// <summary>
        /// Gets the total possible Encumberance
        /// </summary>
        public virtual int MaxEncumberance
        {
            get
            {
                double enc = (double)Strength;
                RAPropertyEnhancer ab = GetAbility<LifterAbility>();
                if (ab != null)
                    enc *= 1 + ((double)ab.Amount / 100);

                // Apply Sojourner ability
                if (this.GetSpellLine("Sojourner") != null)
                {
                    enc *= 1.25;
                }

                // Apply Mythirian Bonus
                if (GetModified(eProperty.MythicalDiscumbering) > 0)
                {
                    enc += GetModified(eProperty.MythicalDiscumbering);
                }

                // Apply Walord effect
                GameSpellEffect iBaneLordEffect = SpellHandler.FindEffectOnTarget(this, "Oppression");
                if (OppressionEncumberance && iBaneLordEffect != null)
                {
                    enc *= 1.00 - (75 * 0.01);
                }

                return (int)enc;
            }
        }

        /// <summary>
        /// Gets the current Encumberance
        /// </summary>
        public virtual int Encumberance
        {
            get { return Inventory.InventoryWeight; }
        }

        /// <summary>
        /// The Encumberance state of this player
        /// </summary>
        protected bool m_overencumbered = true;

        /// <summary>
        /// Gets/Set the players Encumberance state
        /// </summary>
        public bool IsOverencumbered
        {
            get { return m_overencumbered; }
            set { m_overencumbered = value; }
        }

        /// <summary>
        /// Updates the appearance of the equipment this player is using
        /// </summary>
        public virtual void UpdatePlayerEquipment()
        {
            foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                if (player == null) continue;
                if (player != this)
                    player.Out.SendLivingEquipmentUpdate(this);
            }
        }


        /// <summary>
        /// Updates Encumberance and its effects
        /// </summary>
        public void UpdateEncumberance()
        {
            if (Inventory.InventoryWeight > MaxEncumberance)
            {
                if (IsOverencumbered == false)
                {
                    IsOverencumbered = true;
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.UpdateEncumberance.EncumberedMoveSlowly"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                else
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.UpdateEncumberance.Encumbered"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                Out.SendUpdateMaxSpeed();
            }
            else if (IsOverencumbered)
            {
                IsOverencumbered = false;
                Out.SendUpdateMaxSpeed();
            }
            Out.SendEncumberance();
        }

        /// <summary>
        /// Get the bonus names
        /// </summary>
        public string ItemBonusName(int BonusType)
        {
            string BonusName = String.Empty;


            if (BonusType == 1) BonusName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ItemBonusName.Bonus1");//Strength
            if (BonusType == 2) BonusName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ItemBonusName.Bonus2");//Dexterity
            if (BonusType == 3) BonusName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ItemBonusName.Bonus3");//Constitution
            if (BonusType == 4) BonusName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ItemBonusName.Bonus4");//Quickness
            if (BonusType == 5) BonusName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ItemBonusName.Bonus5");//Intelligence
            if (BonusType == 6) BonusName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ItemBonusName.Bonus6");//Piety
            if (BonusType == 7) BonusName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ItemBonusName.Bonus7");//Empathy
            if (BonusType == 8) BonusName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ItemBonusName.Bonus8");//Charisma
            if (BonusType == 9) BonusName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ItemBonusName.Bonus9");//Power
            if (BonusType == 10) BonusName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ItemBonusName.Bonus10");//Hits
            if (BonusType == 11) BonusName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ItemBonusName.Bonus11");//Body
            if (BonusType == 12) BonusName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ItemBonusName.Bonus12");//Cold
            if (BonusType == 13) BonusName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ItemBonusName.Bonus13");//Crush
            if (BonusType == 14) BonusName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ItemBonusName.Bonus14");//Energy
            if (BonusType == 15) BonusName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ItemBonusName.Bonus15");//Heat
            if (BonusType == 16) BonusName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ItemBonusName.Bonus16");//Matter
            if (BonusType == 17) BonusName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ItemBonusName.Bonus17");//Slash
            if (BonusType == 18) BonusName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ItemBonusName.Bonus18");//Spirit
            if (BonusType == 19) BonusName = LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ItemBonusName.Bonus19");//Thrust
            return BonusName;
        }

        /// <summary>
        /// Adds magical bonuses whenever item was equipped
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender">inventory</param>
        /// <param name="arguments"></param>
        protected virtual void OnItemEquipped(DOLEvent e, object sender, EventArgs arguments)
        {
            if (arguments is ItemEquippedArgs == false) return;
            InventoryItem item = ((ItemEquippedArgs)arguments).Item;
            if (item == null) return;

            if (item is IGameInventoryItem)
            {
                (item as IGameInventoryItem).OnEquipped(this);
                
            }

            if (item.Item_Type >= Slot.RIGHTHAND && item.Item_Type <= Slot.RANGED)
            {
                if (item.Hand == 1) // 2h
                {
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemEquipped.WieldBothHands", item.GetName(0, false))), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                else if (item.SlotPosition == Slot.LEFTHAND)
                {
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemEquipped.WieldLeftHand", item.GetName(0, false))), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                else
                {
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemEquipped.WieldRightHand", item.GetName(0, false))), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
            }

            if (item.Item_Type == (int)eInventorySlot.Horse)
            {
                if (item.SlotPosition == Slot.HORSE)
                {
                    ActiveHorse.ID = (byte)(item.SPD_ABS == 0 ? 1 : item.SPD_ABS);
                    ActiveHorse.Name = item.Creator;
                }
                return;
            }
            else if (item.Item_Type == (int)eInventorySlot.HorseArmor)
            {
                if (item.SlotPosition == Slot.HORSEARMOR)
                {
                    //					Out.SendDebugMessage("Try apply horse armor.");
                    ActiveHorse.Saddle = (byte)(item.DPS_AF);
                }
                return;
            }
            else if (item.Item_Type == (int)eInventorySlot.HorseBarding)
            {
                if (item.SlotPosition == Slot.HORSEBARDING)
                {
                    //					Out.SendDebugMessage("Try apply horse barding.");
                    ActiveHorse.Barding = (byte)(item.DPS_AF);
                }
                return;
            }
            if (ObjectState == eObjectState.Active
                                        && (LastPassiveSpellUse <= 0 || LastPassiveSpellUse + 2000 < CurrentRegion.Time)
                                        && item.PassiveSpell != 0)
            {
                Spell passiveSpell = SkillBase.GetSpellByID(item.PassiveSpell);
                if (passiveSpell != null)
                {
                    ISpellHandler passiveHandler = ScriptMgr.CreateSpellHandler(this, passiveSpell, SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects));
                    if (passiveHandler != null)
                    {
                        LastPassiveSpellUse = CurrentRegion.Time;
                        passiveHandler.StartSpell(this);
                    }
                }
            }
                     

            if (!item.IsMagical) return;

            Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemEquipped.Magic", item.GetName(0, false))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);

            if (item.Bonus1 != 0)
            {
                ItemBonus[item.Bonus1Type] += item.Bonus1;
                if (item.Bonus1Type < 20)
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemEquipped.Increased", ItemBonusName(item.Bonus1Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }
            if (item.Bonus2 != 0)
            {
                ItemBonus[item.Bonus2Type] += item.Bonus2;
                if (item.Bonus2Type < 20)
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemEquipped.Increased", ItemBonusName(item.Bonus2Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }
            if (item.Bonus3 != 0)
            {
                ItemBonus[item.Bonus3Type] += item.Bonus3;
                if (item.Bonus3Type < 20)
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemEquipped.Increased", ItemBonusName(item.Bonus3Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }
            if (item.Bonus4 != 0)
            {
                ItemBonus[item.Bonus4Type] += item.Bonus4;
                if (item.Bonus4Type < 20)
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemEquipped.Increased", ItemBonusName(item.Bonus4Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }
            if (item.Bonus5 != 0)
            {
                ItemBonus[item.Bonus5Type] += item.Bonus5;
                if (item.Bonus5Type < 20)
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemEquipped.Increased", ItemBonusName(item.Bonus5Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }
            if (item.Bonus6 != 0)
            {
                ItemBonus[item.Bonus6Type] += item.Bonus6;
                if (item.Bonus6Type < 20)
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemEquipped.Increased", ItemBonusName(item.Bonus6Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }
            if (item.Bonus7 != 0)
            {
                ItemBonus[item.Bonus7Type] += item.Bonus7;
                if (item.Bonus7Type < 20)
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemEquipped.Increased", ItemBonusName(item.Bonus7Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }
            if (item.Bonus8 != 0)
            {
                ItemBonus[item.Bonus8Type] += item.Bonus8;
                if (item.Bonus8Type < 20)
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemEquipped.Increased", ItemBonusName(item.Bonus8Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }
            if (item.Bonus9 != 0)
            {
                ItemBonus[item.Bonus9Type] += item.Bonus9;
                if (item.Bonus9Type < 20)
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemEquipped.Increased", ItemBonusName(item.Bonus9Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }
            if (item.Bonus10 != 0)
            {
                ItemBonus[item.Bonus10Type] += item.Bonus10;
                if (item.Bonus10Type < 20)
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemEquipped.Increased", ItemBonusName(item.Bonus10Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }
            if (item.ExtraBonus != 0)
            {
                ItemBonus[item.ExtraBonusType] += item.ExtraBonus;
                //if (item.ExtraBonusType < 20)
                //Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemEquipped.Increased", ItemBonusName(item.ExtraBonusType))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }

            if (ObjectState == eObjectState.Active)
            {
                // TODO: remove when properties system is finished
                Out.SendCharStatsUpdate();
                Out.SendCharResistsUpdate();
                Out.SendUpdateWeaponAndArmorStats();
                Out.SendUpdateMaxSpeed();
                Out.SendEncumberance();
                Out.SendUpdatePlayerSkills();
                UpdatePlayerStatus();


                if (IsAlive)
                {
                    if (Health < MaxHealth) StartHealthRegeneration();
                    else if (Health > MaxHealth) Health = MaxHealth;

                    if (Mana < MaxMana) StartPowerRegeneration();
                    else if (Mana > MaxMana) Mana = MaxMana;

                    if (Endurance < MaxEndurance) StartEnduranceRegeneration();
                    else if (Endurance > MaxEndurance) Endurance = MaxEndurance;
                }
            }
        }

        /// <summary>
        /// Removes magical bonuses whenever item was unequipped
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender">inventory</param>
        /// <param name="arguments"></param>
        protected virtual void OnItemUnequipped(DOLEvent e, object sender, EventArgs arguments)
        {
            if (arguments is ItemUnequippedArgs == false) return;
            InventoryItem item = ((ItemUnequippedArgs)arguments).Item;
            int prevSlot = (int)((ItemUnequippedArgs)arguments).PreviousSlotPosition;
            if (item == null) return;
          

            if (item.Item_Type >= Slot.RIGHTHAND && item.Item_Type <= Slot.RANGED)
            {
                if (item.Hand == 1) // 2h
                {
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemUnequipped.BothHandsFree", item.GetName(0, false))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
                }
                else if (prevSlot == Slot.LEFTHAND)
                {
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemUnequipped.LeftHandFree", item.GetName(0, false))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
                }
                else
                {
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemUnequipped.RightHandFree", item.GetName(0, false))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
                }
            }

            if (prevSlot == Slot.MYTHICAL && item.Item_Type == (int)eInventorySlot.Mythical && item is GameMythirian)
            {
                (item as GameMythirian).OnUnEquipped(this);
            }
            if (item.Item_Type == (int)eInventorySlot.Horse)
            {
                if (IsOnHorse)
                    IsOnHorse = false;
                ActiveHorse.ID = 0;
                ActiveHorse.Name = String.Empty;
                //				Out.SendDebugMessage("Try unapply horse.");
                return;
            }
            else if (item.Item_Type == (int)eInventorySlot.HorseArmor)
            {
                ActiveHorse.Saddle = 0;
                //				Out.SendDebugMessage("Try unapply saddle.");
                return;
            }
            else if (item.Item_Type == (int)eInventorySlot.HorseBarding)
            {
                ActiveHorse.Barding = 0;
                //				Out.SendDebugMessage("Try unapply barding.");
                return;
            }
            //Elcotek: disable spells after unequip
            if (ObjectState == eObjectState.Active && item.SpellID != 0 && IsThisAWeapon(item) == false)
            {
                Spell SpellID = SkillBase.GetSpellByID(item.SpellID);
                if (SpellID != null)
                {
                    GameSpellEffect SpellIDEffect = SpellHandler.FindEffectOnTarget(this, SpellID.SpellType, SpellID.Name);
                    if (SpellIDEffect != null)
                        SpellIDEffect.Cancel(false);
                }
                if (SpellID.SubSpellID > 0)
                {
                    Spell SubSpellID = SkillBase.GetSpellByID(SpellID.SubSpellID);

                    GameSpellEffect SubSpellIDEffect = SpellHandler.FindEffectOnTarget(this, SubSpellID.SpellType, SubSpellID.Name);
                    if (SubSpellIDEffect != null)
                        SubSpellIDEffect.Cancel(false);

                }
            }
            if (ObjectState == eObjectState.Active && item.SpellID1 != 0 && IsThisAWeapon(item) == false)
            {
                Spell SpellID1 = SkillBase.GetSpellByID(item.SpellID1);
                if (SpellID1 != null)
                {
                    GameSpellEffect SpellID1Effect = SpellHandler.FindEffectOnTarget(this, SpellID1.SpellType, SpellID1.Name);
                    if (SpellID1Effect != null)
                        SpellID1Effect.Cancel(false);

                    if (SpellID1.SubSpellID > 0)
                    {
                        Spell SubSpellID = SkillBase.GetSpellByID(SpellID1.SubSpellID);

                        GameSpellEffect SubSpellID1Effect = SpellHandler.FindEffectOnTarget(this, SubSpellID.SpellType, SubSpellID.Name);
                        if (SubSpellID1Effect != null)
                            SubSpellID1Effect.Cancel(false);

                    }
                }
            }


            if (ObjectState == eObjectState.Active && item.PassiveSpell != 0 && IsThisAWeapon(item) == false)
            {
                Spell passiveSpell = SkillBase.GetSpellByID(item.PassiveSpell);
                if (passiveSpell != null)
                {
                    GameSpellEffect passiveEffect = SpellHandler.FindEffectOnTarget(this, passiveSpell.SpellType, passiveSpell.Name);
                    if (passiveEffect != null)
                        passiveEffect.Cancel(false);
                }
            }
            if (!item.IsMagical) return;

            if (item.Bonus1 != 0)
            {
                ItemBonus[item.Bonus1Type] -= item.Bonus1;
                if (item.Bonus1Type < 20)
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemUnequipped.Decreased", ItemBonusName(item.Bonus1Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }
            if (item.Bonus2 != 0)
            {
                ItemBonus[item.Bonus2Type] -= item.Bonus2;
                if (item.Bonus2Type < 20)
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemUnequipped.Decreased", ItemBonusName(item.Bonus2Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }
            if (item.Bonus3 != 0)
            {
                ItemBonus[item.Bonus3Type] -= item.Bonus3;
                if (item.Bonus3Type < 20)
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemUnequipped.Decreased", ItemBonusName(item.Bonus3Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }
            if (item.Bonus4 != 0)
            {
                ItemBonus[item.Bonus4Type] -= item.Bonus4;
                if (item.Bonus4Type < 20)
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemUnequipped.Decreased", ItemBonusName(item.Bonus4Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }
            if (item.Bonus5 != 0)
            {
                ItemBonus[item.Bonus5Type] -= item.Bonus5;
                if (item.Bonus5Type < 20)
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemUnequipped.Decreased", ItemBonusName(item.Bonus5Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }
            if (item.Bonus6 != 0)
            {
                ItemBonus[item.Bonus6Type] -= item.Bonus6;
                if (item.Bonus6Type < 20)
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemUnequipped.Decreased", ItemBonusName(item.Bonus6Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }
            if (item.Bonus7 != 0)
            {
                ItemBonus[item.Bonus7Type] -= item.Bonus7;
                if (item.Bonus7Type < 20)
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemUnequipped.Decreased", ItemBonusName(item.Bonus7Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }
            if (item.Bonus8 != 0)
            {
                ItemBonus[item.Bonus8Type] -= item.Bonus8;
                if (item.Bonus8Type < 20)
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemUnequipped.Decreased", ItemBonusName(item.Bonus8Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }
            if (item.Bonus9 != 0)
            {
                ItemBonus[item.Bonus9Type] -= item.Bonus9;
                if (item.Bonus9Type < 20)
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemUnequipped.Decreased", ItemBonusName(item.Bonus9Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }
            if (item.Bonus10 != 0)
            {
                ItemBonus[item.Bonus10Type] -= item.Bonus10;
                if (item.Bonus10Type < 20)
                    Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OnItemUnequipped.Decreased", ItemBonusName(item.Bonus10Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }
            if (item.ExtraBonus != 0)
            {
                ItemBonus[item.ExtraBonusType] -= item.ExtraBonus;
                //if (item.ExtraBonusType < 20)
                //Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemUnequipped.Decreased", ItemBonusName(item.ExtraBonusType))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }

            if (item is IGameInventoryItem)
            {
                (item as IGameInventoryItem).OnUnEquipped(this);
            }

            if (ObjectState == eObjectState.Active)
            {
                // TODO: remove when properties system is finished
                Out.SendCharStatsUpdate();
                Out.SendCharResistsUpdate();
                Out.SendUpdateWeaponAndArmorStats();
                Out.SendUpdateMaxSpeed();
                Out.SendEncumberance();
                Out.SendUpdatePlayerSkills();
                UpdatePlayerStatus();

                if (IsAlive)
                {
                    if (Health < MaxHealth) StartHealthRegeneration();
                    else if (Health > MaxHealth) Health = MaxHealth;

                    if (Mana < MaxMana) StartPowerRegeneration();
                    else if (Mana > MaxMana) Mana = MaxMana;

                    if (Endurance < MaxEndurance) StartEnduranceRegeneration();
                    else if (Endurance > MaxEndurance) Endurance = MaxEndurance;
                }
            }
        }

        public virtual bool IsThisAWeapon(InventoryItem item)
        {
            switch (item.Item_Type)
            {

                case (int)eInventorySlot.TwoHandWeapon:
                case (int)eInventorySlot.LeftHandWeapon:
                case (int)eInventorySlot.RightHandWeapon:
                case (int)eInventorySlot.DistanceWeapon:

                    {
                        return true;
                    }
            }
            return false;
        }

        public virtual void RefreshItemBonuses()
        {
            m_itemBonus = new PropertyIndexer();
            string slotToLoad = "";
            switch (VisibleActiveWeaponSlots)
            {
                case 16: slotToLoad = "rightandleftHandSlot"; break;
                case 18: slotToLoad = "leftandtwoHandSlot"; break;
                case 31: slotToLoad = "leftHandSlot"; break;
                case 34: slotToLoad = "twoHandSlot"; break;
                case 51: slotToLoad = "distanceSlot"; break;
                case 240: slotToLoad = "righttHandSlot"; break;
                case 242: slotToLoad = "twoHandSlot"; break;
                default: break;
            }

            foreach (InventoryItem item in Inventory.EquippedItems)
            {
                if (item == null)
                    continue;
                // skip weapons. only active weapons should fire equip event, done in player.SwitchWeapon
                bool add = true;
                if (String.IsNullOrEmpty(slotToLoad) == false)
                {
                    switch (item.SlotPosition)
                    {

                        case Slot.TWOHAND:
                            if (slotToLoad.Contains("twoHandSlot") == false)
                            {
                                add = false;
                            }
                            break;

                        case Slot.RIGHTHAND:
                            if (slotToLoad.Contains("right") == false)
                            {
                                add = false;
                            }
                            break;
                        case Slot.SHIELD:
                        case Slot.LEFTHAND:
                            if (slotToLoad.Contains("left") == false)
                            {
                                add = false;
                            }
                            break;
                        case Slot.RANGED:
                            if (slotToLoad != "distanceSlot")
                            {
                                add = false;
                            }
                            break;
                        default: break;
                    }
                }

                if (!add) continue;

                if (item is IGameInventoryItem)
                {
                    (item as IGameInventoryItem).CheckValid(this);
                }

                if (item.IsMagical)
                {
                    if (item.Bonus1 != 0)
                    {
                        ItemBonus[item.Bonus1Type] += item.Bonus1;
                    }
                    if (item.Bonus2 != 0)
                    {
                        ItemBonus[item.Bonus2Type] += item.Bonus2;
                    }
                    if (item.Bonus3 != 0)
                    {
                        ItemBonus[item.Bonus3Type] += item.Bonus3;
                    }
                    if (item.Bonus4 != 0)
                    {
                        ItemBonus[item.Bonus4Type] += item.Bonus4;
                    }
                    if (item.Bonus5 != 0)
                    {
                        ItemBonus[item.Bonus5Type] += item.Bonus5;
                    }
                    if (item.Bonus6 != 0)
                    {
                        ItemBonus[item.Bonus6Type] += item.Bonus6;
                    }
                    if (item.Bonus7 != 0)
                    {
                        ItemBonus[item.Bonus7Type] += item.Bonus7;
                    }
                    if (item.Bonus8 != 0)
                    {
                        ItemBonus[item.Bonus8Type] += item.Bonus8;
                    }
                    if (item.Bonus9 != 0)
                    {
                        ItemBonus[item.Bonus9Type] += item.Bonus9;
                    }
                    if (item.Bonus10 != 0)
                    {
                        ItemBonus[item.Bonus10Type] += item.Bonus10;
                    }
                    if (item.ExtraBonus != 0)
                    {
                        ItemBonus[item.ExtraBonusType] += item.ExtraBonus;
                    }
                }
            }
        }

        /// <summary>
        /// Handles a bonus change on an item.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void OnItemBonusChanged(DOLEvent e, object sender, EventArgs args)
        {
            ItemBonusChangedEventArgs changeArgs = args as ItemBonusChangedEventArgs;
            if (changeArgs == null || changeArgs.BonusType == 0 || changeArgs.BonusAmount == 0)
                return;

            ItemBonus[changeArgs.BonusType] += changeArgs.BonusAmount;

            if (ObjectState == eObjectState.Active)
            {
                Out.SendCharStatsUpdate();
                Out.SendCharResistsUpdate();
                Out.SendUpdateWeaponAndArmorStats();
                Out.SendUpdatePlayerSkills();
                UpdatePlayerStatus();

                if (IsAlive)
                {
                    if (Health < MaxHealth) StartHealthRegeneration();
                    else if (Health > MaxHealth) Health = MaxHealth;

                    if (Mana < MaxMana) StartPowerRegeneration();
                    else if (Mana > MaxMana) Mana = MaxMana;

                    if (Endurance < MaxEndurance) StartEnduranceRegeneration();
                    else if (Endurance > MaxEndurance) Endurance = MaxEndurance;
                }
            }
        }

        #endregion



        #region ReceiveItem/DropItem/PickupObject
        /// <summary>
        /// Receive an item from another living
        /// </summary>
        /// <param name="source"></param>
        /// <param name="item"></param>
        /// <returns>true if player took the item</returns>
        public override bool ReceiveItem(GameLiving source, InventoryItem item)
        {
            if (item == null) return false;

            if (!Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
                return false;
            //InventoryLogging.LogInventoryAction(source, this, eInventoryActionType.Trade, item.Template, item.Count);

            if (source == null)
            {
                Out.SendMessage(String.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ReceiveItem.Receive", item.GetName(0, false))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }
            else
            {
                if (source is GameNPC)
                    Out.SendMessage(String.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ReceiveItem.ReceiveFrom", item.GetName(0, false), source.GetName(0, false, Client.Account.Language, (source as GameNPC)))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
                else
                    Out.SendMessage(String.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ReceiveItem.ReceiveFrom", item.GetName(0, false), source.GetName(0, false))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }


            if (source is GamePlayer)
            {
                GamePlayer sourcePlayer = source as GamePlayer;
                if (sourcePlayer != null)
                {
                    uint privLevel1 = Client.Account.PrivLevel;
                    uint privLevel2 = sourcePlayer.Client.Account.PrivLevel;
                    if (privLevel1 != privLevel2
                        && (privLevel1 > 1 || privLevel2 > 1)
                        && (privLevel1 == 1 || privLevel2 == 1))
                    {
                        GameServer.Instance.LogGMAction("   Item: " + source.Name + "(" + sourcePlayer.Client.Account.Name + ") -> " + Name + "(" + Client.Account.Name + ") : " + item.Name + "(" + item.Id_nb.ToString() + ")");
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Called to drop an Item from the Inventory to the floor
        /// </summary>
        /// <param name="slot_pos">SlotPosition to drop</param>
        /// <returns>true if dropped</returns>
        /// <summary>
        /// Called to drop an Item from the Inventory to the floor
        /// </summary>
        /// <param name="slot_pos">SlotPosition to drop</param>
        /// <returns>true if dropped</returns>
        public virtual bool DropItem(eInventorySlot slot_pos)
        {

#pragma warning disable IDE0059 // Der Wert, der dem Symbol zugeordnet ist, wird niemals verwendet.
            return DropItem(slot_pos, out WorldInventoryItem tempItem);
#pragma warning restore IDE0059 // Der Wert, der dem Symbol zugeordnet ist, wird niemals verwendet.
        }

        /// <summary>
        /// model's for boats and siege and levels
        /// </summary>
        /// <returns>the model of the boat</returns> 
        private ushort ShipModel = 0;
        private ushort SiegeModel = 0;
        private byte SiegeLevel = 0;


        /// <summary>
        ///if a player is in Battelground region ?
        /// </summary>
        ////// <returns>true if this is true</returns> 
        private bool IsInBG(GamePlayer player)
        {
            //player = this;

            if (player != null)
            {
                switch (player.CurrentRegionID)
                {
                    case 239:
                    case 165:
                    case 241:
                    case 242:
                    case 236:
                    case 235:
                    case 234:
                    case 238:
                    case 240:
                    case 489:
                    case 237:
                        {
                            return true;
                        }
                }
            }
            return false;
        }
        /// <summary>
		/// Called to drop an item from the Inventory to the floor
		/// and return the GameInventoryItem that is created on the floor
		/// </summary>
		/// <param name="slot_pos">SlotPosition to drop</param>
		/// <param name="droppedItem">out GameItem that was created</param>
		/// <returns>true if dropped</returns>
        public virtual bool DropItem(eInventorySlot slot_pos, out WorldInventoryItem droppedItem)
        {
            droppedItem = null;
            if (slot_pos >= eInventorySlot.FirstBackpack && slot_pos <= eInventorySlot.LastBackpack)
            {
                lock (Inventory)
                {
                    InventoryItem item = Inventory.GetItem(slot_pos);

                    if (!item.IsDropable)
                    {
                        Out.SendMessage(item.GetName(0, true) + " cannot be dropped!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return false;
                    }

                    if (!Inventory.RemoveItem(item)) return false;

                    //Boote und Rammen System by Elcotek
                    switch (item.Name)
                    {
                        case "scout boat":
                            ShipModel = 2648;
                            break;
                        case "Skiff":
                            ShipModel = 1616;
                            break;
                        case "Warship":
                            ShipModel = 2647;
                            break;
                        case "Galleon":
                            ShipModel = 2646;
                            break;
                        case "Viking Longship":
                            ShipModel = 1615;
                            break;
                        case "British Cog":
                            ShipModel = 1614;
                            break;


                        //Siege
                        case "mini ram":
                            SiegeModel = 2605;
                            SiegeLevel = 0;
                            break;
                        case "light ram":
                            SiegeModel = 2600;
                            SiegeLevel = 1;
                            break;
                        case "medium ram":
                            SiegeModel = 2601;
                            SiegeLevel = 2;
                            break;
                        case "heavy ram":
                            SiegeModel = 2602;
                            SiegeLevel = 3;
                            break;
                        case "catapult":
                            SiegeModel = 2598;
                            SiegeLevel = 0;
                            break;
                        case "ballista":
                            SiegeModel = 2596;
                            SiegeLevel = 0;
                            break;
                        case "trebuchet":
                            SiegeModel = 2599;
                            SiegeLevel = 0;
                            break;



                    }
                    if (item.Model == SiegeModel)
                    {


                        WorldInventoryItem ItemModel = null;

                        ItemModel = (item as IGameInventoryItem).Drop(this);

                        ItemModel.Model = 0;



                        //Siegeweapons only setup is Rais is allowed in NF
                        if (AllowedToRaid() == false && CurrentRegionID == 163)
                        {

                            switch (item.Model)
                            {
                                case 2600:
                                case 2601:
                                case 2602:
                                case 2598:
                                case 2596:
                                case 2599:
                                    {

                                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "AntiRaidMsg", Properties.RAID_MEMBER_COUNT + 1), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                                        Out.SendMessage(item.GetName(0, true) + " cannot be dropped!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                        PickupObject(ItemModel, true);
                                        return false;

                                    }
                                default: break;
                            }
                        }

                        //miniram and balista only allowed in BG's
                        if (this.CurrentRegionID != 163)
                        {

                            switch (item.Model)
                            {
                                case 2600:
                                case 2601:
                                case 2602:
                                case 2598:
                                //case 2596: balista
                                case 2599:
                                    {
                                        if (IsInBG(this))//player in Battleground  Region?
                                            PickupObject(ItemModel, true);
                                        this.Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "DontWorkInBG", Name, item.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                        return false;
                                    }
                                default: break;
                            }
                        }


                        if (this.CurrentRegionID == 163)
                        {

                            switch (item.Model)
                            {
                                case 2605:
                                    {
                                        if (IsInBG(this) == false)//player in New Frontiers  Region?
                                            PickupObject(ItemModel, true);
                                        this.Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "DontWorkINewFrontiers", Name, item.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);


                                        return false;
                                    }
                                default: break;
                            }
                        }

                        if (IsSitting)
                        {
                            PickupObject(ItemModel, true);
                            return false;
                        }
                        if (IsSwimming)
                        {
                            // Not in water
                            PickupObject(ItemModel, true);
                            this.Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "SiegeDontWorkInWater", item.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return false;
                        }
                        if (IsStealthed)
                        {
                            // Not Stealthed
                            PickupObject(ItemModel, true);
                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "DontWorkWihleStealthed", Name, item.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return false;
                        }
                        if (IsMezzed || IsStunned || IsSilenced || InCombat)
                        {
                            // Not Mezzed Stunned Silenced Sitting
                            PickupObject(ItemModel, true);
                            this.Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "DontWorkWihleInCombat", item.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return false;
                        }

                        foreach (GameObject door in this.GetDoorsInRadius(1000))
                        {
                            if (ItemModel.IsWithinRadius(door, 300) && door is GameKeepDoor == true && this is GamePlayer && door != null)
                            {
                                PickupObject(ItemModel, true);

                                //Keep Doors
                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "YouCantPlaceHere", Name, item.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return false;

                            }
                        }




                        if (item.Name == "trebuchet")
                        {
                            GameSiegeTimedTrebuchet tre = new GameSiegeTimedTrebuchet
                            {
                                X = ItemModel.X,
                                Y = ItemModel.Y,
                                Z = ItemModel.Z,
                                Heading = ItemModel.Heading,
                                CurrentRegion = this.CurrentRegion,
                                //tre.OwnerID = ItemModel.InternalID;
                                Model = SiegeModel,
                                Level = 0,
                                Name = "trebuchet",
                                Realm = this.Realm,
                                Riders = new GamePlayer[1]
                            };
                            BlankBrain brain = new BlankBrain();
                            tre.SetOwnBrain(brain);
                            tre.AddToWorld();
                            ItemModel.RemoveFromWorld();
                        }
                        else if (item.Name == "ballista")
                        {
                            GameSiegeTimedBallista bal = new GameSiegeTimedBallista
                            {
                                X = ItemModel.X,
                                Y = ItemModel.Y,
                                Z = ItemModel.Z,
                                Heading = ItemModel.Heading,
                                CurrentRegion = this.CurrentRegion,
                                // bal.OwnerID = ItemModel.InternalID;

                                Model = SiegeModel,
                                Level = 0,
                                Name = "ballista",
                                Realm = this.Realm,
                                Riders = new GamePlayer[1]
                            };
                            BlankBrain brain = new BlankBrain();
                            bal.SetOwnBrain(brain);
                            bal.AddToWorld();
                            ItemModel.RemoveFromWorld();
                        }
                        else if (item.Name == "catapult")
                        {
                            GameSiegeTimedCatapult cat = new GameSiegeTimedCatapult
                            {
                                X = ItemModel.X,
                                Y = ItemModel.Y,
                                Z = ItemModel.Z,
                                Heading = ItemModel.Heading,
                                CurrentRegion = this.CurrentRegion,
                                //cat.OwnerID = ItemModel.InternalID;
                                Model = SiegeModel,
                                Level = 0,
                                Name = "catapult",
                                Realm = this.Realm,
                                Riders = new GamePlayer[1]
                            };
                            BlankBrain brain = new BlankBrain();
                            cat.SetOwnBrain(brain);
                            cat.AddToWorld();
                            ItemModel.RemoveFromWorld();
                        }
                        else
                        {
                            GameSiegeTimedRam ram = new GameSiegeTimedRam
                            {
                                X = ItemModel.X,
                                Y = ItemModel.Y,
                                Z = ItemModel.Z,
                                Heading = ItemModel.Heading,
                                CurrentRegion = this.CurrentRegion,
                                //ram.OwnerID = ItemModel.InternalID;

                                Model = SiegeModel,
                                Level = SiegeLevel,
                                Name = item.Name,
                                Realm = this.Realm
                            };
                            //ram.Riders = new GamePlayer[SiegeLevel];
                            //BlankBrain brain = new BlankBrain();
                            //ram.SetOwnBrain(brain);
                            ram.AddToWorld();
                            ItemModel.RemoveFromWorld();
                        }
                        {
                            Notify(PlayerInventoryEvent.ItemDropped, this, new ItemDroppedEventArgs(item, droppedItem));
                        }
                        return true;
                    }

                    if (item.Model == ShipModel)
                    {

                        GameBoat curBoat = BoatMgr.GetBoatByOwner(this.InternalID);
                        GameBoatTimed playerBoat = new GameBoatTimed();
                        WorldInventoryItem ItemModel = null;

                        ItemModel = (item as IGameInventoryItem).Drop(this);

                        if (curBoat != null && curBoat.BoatOwner == this.InternalID)
                        {
                            curBoat.RemoveFromWorld();
                            BoatMgr.DeleteBoat(curBoat.Name);
                        }
                        if (GameRelic.IsPlayerCarryingRelic(this))
                        {
                            Out.SendMessage(LanguageMgr.GetTranslation((this as GamePlayer).Client.Account.Language, "Scripts.Player.Boat.YouCantUseWithRelics"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            this.PickupObject(ItemModel, true);
                            return false;
                        }

                        if (!this.IsSwimming)
                        {
                            // only in water
                            Out.SendMessage(LanguageMgr.GetTranslation(this.Name, "Scripts.Player.Boat.NotInWater"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            this.PickupObject(ItemModel, true);
                            return false;
                        }
                        if (curBoat == null)
                        {
                            ItemModel.Model = 0;

                            playerBoat.BoatID = System.Guid.NewGuid().ToString();
                            playerBoat.Model = ShipModel;
                            playerBoat.Name = this.Name + "'s " + item.Name;
                            playerBoat.X = ItemModel.X;
                            playerBoat.Y = ItemModel.Y;
                            playerBoat.Z = ItemModel.Z;
                            playerBoat.Heading = ItemModel.Heading;
                            playerBoat.Realm = this.Realm; ;
                            playerBoat.CurrentRegionID = this.CurrentRegionID;
                            playerBoat.BoatOwner = ItemModel.InternalID;
                            playerBoat.MaxSpeedBase = 500;
                            playerBoat.Riders = new GamePlayer[8];
                            BlankBrain brain = new BlankBrain();
                            playerBoat.SetOwnBrain(brain);
                            BoatMgr.CreateBoat(this, playerBoat);
                            if (this.Guild != null)
                            {
                                if (this.Guild.Emblem != 0)
                                    playerBoat.Emblem = (ushort)this.Guild.Emblem;
                                playerBoat.GuildName = this.Guild.Name;
                                playerBoat.AddToWorld();
                                this.MountSteed(playerBoat, true);
                                BoatMgr.AddBoat(playerBoat);
                                BoatMgr.SaveAllBoats();
                                ItemModel.RemoveFromWorld();
                            }
                            else
                            {
                                if (playerBoat.Realm == eRealm.Albion)
                                    playerBoat.Emblem = (ushort)464;
                                if (playerBoat.Realm == eRealm.Midgard)
                                    playerBoat.Emblem = (ushort)465;
                                if (playerBoat.Realm == eRealm.Hibernia)
                                    playerBoat.Emblem = (ushort)466;
                                {

                                    playerBoat.AddToWorld();
                                    this.MountSteed(playerBoat, true);
                                    BoatMgr.AddBoat(playerBoat);
                                    BoatMgr.SaveAllBoats();
                                    ItemModel.RemoveFromWorld();
                                }
                            }
                            {
                                this.Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "Scripts.Player.Boat.Help.Summoned"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                // this.Out.SendMessage("use: (/boat unsummon) to take back your boat into your inventory!"

                                // + " please use: /boat for more commands!", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                            }
                            Notify(PlayerInventoryEvent.ItemDropped, this, new ItemDroppedEventArgs(item, droppedItem));
                        }

                        return true;
                    }


                    //InventoryLogging.LogInventoryAction(this, "(ground)", eInventoryActionType.Other, item.Template, item.Count);

                    droppedItem = CreateItemOnTheGround(item);

                    if (droppedItem != null)
                    {
                        Notify(PlayerInventoryEvent.ItemDropped, this, new ItemDroppedEventArgs(item, droppedItem));
                    }

                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// called to make an item on the ground with owner is player
        /// </summary>
        /// <param name="item">the item to create on the ground</param>
        /// <returns>the GameInventoryItem on the ground</returns>
        public virtual WorldInventoryItem CreateItemOnTheGround(InventoryItem item)
        {
            WorldInventoryItem gameItem;
            if (item is IGameInventoryItem)
            {
                gameItem = (item as IGameInventoryItem).Drop(this);

            }
            else
            {

                gameItem = new WorldInventoryItem(item);

                Point2D itemloc = this.GetPointFromHeading(this.Heading, 30);
                gameItem.X = itemloc.X;
                gameItem.Y = itemloc.Y;
                gameItem.Z = Z;
                gameItem.Heading = Heading;
                gameItem.CurrentRegionID = CurrentRegionID;

                gameItem.AddOwner(this);
                gameItem.AddToWorld();
            }

            return gameItem;
        }
        


        /// <summary>
        /// Called when the player picks up an item from the ground
        /// </summary>
        /// <param name="floorObject"></param>
        /// <param name="checkRange"></param>
        /// <returns></returns>
        /// <summary>
        /// Called when the player picks up an item from the ground
        /// </summary>
        /// <param name="floorObject"></param>
        /// <param name="checkRange"></param>
        /// <returns></returns>
        public virtual bool PickupObject(GameObject floorObject, bool checkRange)
        {
           
            if (floorObject == null)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.PickupObject.MustHaveTarget"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (floorObject.ObjectState != eObjectState.Active)
                return false;


            //log.ErrorFormat("Floor item realm = {0}", floorObject.Realm);

            if (floorObject is GameStaticItemTimed && (floorObject as GameStaticItemTimed).IsOwner(this) == false && Client.Account.PrivLevel == (int)ePrivLevel.Player)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.PickupObject.LootDoesntBelongYou"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if ((floorObject is GameBoat == false) && !checkRange && !floorObject.IsWithinRadius(this, GS.ServerProperties.Properties.WORLD_PICKUP_DISTANCE, true))
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.PickupObject.ObjectTooFarAway", floorObject.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                try
                {
                    //	log.DebugFormat("Pickup error: {0}  object x{1}, y{2}, z{3}, r{4} - player x{5}, y{6}, z{7}, r{8}",
                    //                Name,
                    //                floorObject.X, floorObject.Y, floorObject.Z, floorObject.CurrentRegionID,
                    //               X, Y, Z, CurrentRegionID);
                }
                catch
                {
                }
                return false;
            }
            
            if (floorObject is WorldInventoryItem)
            {
                WorldInventoryItem floorItem = floorObject as WorldInventoryItem;

                lock (floorItem)
                {

                    if (floorItem.ObjectState != eObjectState.Active)
                        return false;

                    //log.Error("loot1");
                    //Änderung
                    if (floorItem.Item == null || floorItem.Item.IsPickable == false) // || floorItem.Item.Realm != (int)Client.Player.Realm && floorItem.Item.Realm != 0) 
                    {
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.PickupObject.CantGetThat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return false;
                    }
                    if (floorItem.GetPickupTime > 0)
                    {
                        Out.SendMessage("You must wait another " + (floorItem.GetPickupTime / 1000).ToString() + " seconds to pick up " + floorItem.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return false;
                    }

                    Group group = Group;
                    BattleGroup mybattlegroup = (BattleGroup)TempProperties.getProperty<object>(BattleGroup.BATTLEGROUP_PROPERTY, null);
                    if (mybattlegroup != null && GS.ServerProperties.Properties.BATTLEGROUP_LOOT && mybattlegroup.GetBGLootType() == true && mybattlegroup.GetBGTreasurer() != null)
                    {
                        GamePlayer theTreasurer = mybattlegroup.GetBGTreasurer();
                        if (theTreasurer.CanSeeObject(floorObject))
                        {
                            //elcotek fix so das keine nichthandelbaren sachen übertragen werden können.
                            if (floorItem.Item.IsTradable == false)
                            {
                                theTreasurer.Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.ReceiveTradeItem.CantTrade"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return false;
                            }
                            bool good = false;
                            if (floorItem.Item.IsStackable)
                                good = theTreasurer.Inventory.AddTemplate(floorItem.Item, floorItem.Item.Count, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
                            else
                                good = theTreasurer.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, floorItem.Item);

                            if (!good)
                            {
                                theTreasurer.Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.PickupObject.BackpackFull"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return false;
                            }
                            theTreasurer.Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.PickupObject.YouGet", floorItem.Item.GetName(1, false), floorItem.Item.Count), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            Message.SystemToOthers(this, LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.PickupObject.GroupMemberPicksUp", Name, floorItem.Item.GetName(1, false)), eChatType.CT_System);
                            // InventoryLogging.LogInventoryAction("(ground)", this, eInventoryActionType.Loot, floorItem.Item.Template, floorItem.Item.IsStackable ? floorItem.Item.Count : 1);

                        }
                        else
                        {
                            mybattlegroup.SendMessageToBattleGroupMembers(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.PickupObject.NoOneWantsThis", floorObject.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                    }
                    else if (group != null && group.AutosplitLoot || mybattlegroup != null && GS.ServerProperties.Properties.BATTLEGROUP_LOOT && mybattlegroup.GetBGLootType() == false && mybattlegroup.GetBGTreasurer() == null)
                    {
                        int memberCount = 8;
                        List<GameObject> owners = new List<GameObject>((GameObject[])floorItem.Owners);
                        BattleGroup battleGroup = TempProperties.getProperty<BattleGroup>(BattleGroup.BATTLEGROUP_PROPERTY, null);


                        if (battleGroup != null && GS.ServerProperties.Properties.BATTLEGROUP_LOOT)
                        {
                            memberCount = battleGroup.Members.Count;

                        }
                        else
                        {
                            memberCount = group.MemberCount;

                        }

                        List<GamePlayer> eligibleMembers = new List<GamePlayer>(memberCount);


                        if (battleGroup != null && GS.ServerProperties.Properties.BATTLEGROUP_LOOT)
                        {
                            //BattleGroup
                            foreach (GamePlayer players in battleGroup.Members.Keys)
                            {

                                if (players.IsAlive
                                && players.CanSeeObject(floorObject)
                                && this.IsWithinRadius(players, WorldMgr.MAX_EXPFORKILL_DISTANCE)
                                && (players.ObjectState == eObjectState.Active)
                                && (owners.Contains(players) || owners.Count == 0)
                                && (players.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) != eInventorySlot.Invalid))
                                {

                                    eligibleMembers.Add(players);

                                }
                            }
                        }
                        else
                        {
                            //Group Autosplit 
                            //log.ErrorFormat("Alle erhalten items? {0}", Group.AutosplitCoins);
                            foreach (GamePlayer ply in group.GetPlayersInTheGroup())
                            {
                                //log.ErrorFormat("Player Name = {0}, Realm: {1}", ply.Name, ply.Realm);
                                //log.ErrorFormat("Floor item realm = {0}", floorObject.Realm);
                                if (ply.IsAlive
                                    && ply.CanSeeObject(floorObject)
                                    && this.IsWithinRadius(ply, WorldMgr.MAX_EXPFORKILL_DISTANCE)
                                    && (ply.ObjectState == eObjectState.Active)
                                    && (ply.AutoSplitLoot)
                                    //&& (owners.Contains(ply) || owners.Count == 0)
                                    && (ply.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) != eInventorySlot.Invalid))
                                {
                                    //log.ErrorFormat("Player Name2 = {0}, Realm {1}", ply.Name, ply.Realm);
                                    /*
                                    //Änderung
                                    if (floorObject.Realm == 0)
                                    {
                                        //log.ErrorFormat("item realm 0 - Player list: {0}", ply.Name);
                                        eligibleMembers.Add(ply);
                                    }
                                    else 
                                    */
                                    if (ply.Realm == floorObject.Realm || floorObject.Realm == 0)
                                    {
                                        //log.ErrorFormat("Players added to eligibleMembers list: {0}", ply.Name);
                                        eligibleMembers.Add(ply);
                                    }
                                }
                            }
                        }

                        //log.ErrorFormat("items? {0}", Group.AutosplitCoins);
                        if (eligibleMembers.Count <= 0)
                        {
                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.PickupObject.NoOneWantsThis", floorObject.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return false;
                        }

                        int i = Util.Random(0, eligibleMembers.Count - 1);
                        GamePlayer eligibleMember = eligibleMembers[i];
                        if (eligibleMember != null)
                        {
                            bool good = false;
                            if (floorItem.Item.IsStackable) // poison ID is lost here
                                good = eligibleMember.Inventory.AddTemplate(floorItem.Item, floorItem.Item.Count, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
                            else
                                good = eligibleMember.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, floorItem.Item);

                            if (!good)
                            {
                                eligibleMember.Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.PickupObject.BackpackFull"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return false;
                            }
                            Message.SystemToOthers(this, LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.PickupObject.GroupMemberPicksUp", Name, floorItem.Item.GetName(1, false)), eChatType.CT_System);

                                                       
                            if (mybattlegroup != null && GS.ServerProperties.Properties.BATTLEGROUP_LOOT)
                            {
                                mybattlegroup.SendMessageToGroupMembers(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.PickupObject.Autosplit", floorItem.Item.GetName(1, true), eligibleMember.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                              
                            }
                            else
                                group.SendMessageToGroupMembers(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.PickupObject.Autosplit", floorItem.Item.GetName(1, true), eligibleMember.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                           
                            //InventoryLogging.LogInventoryAction("(ground)", this, eInventoryActionType.Loot, floorItem.Item.Template, floorItem.Item.IsStackable ? floorItem.Item.Count : 1);
                        }
                    }
                    else
                    {
                        GamePlayer gLeader = null;



                        //BG Treasurer
                        if (mybattlegroup != null && mybattlegroup.GetBGTreasurer() != null)
                        {

                            gLeader = mybattlegroup.GetBGTreasurer();

                            //leader is not in range
                            if (gLeader != null && gLeader.IsWithinRadius(floorItem, 4000) == false)
                            {
                                gLeader = Client.Player;
                            }
                            //log.ErrorFormat("BG leader = {0}", gLeader.Name);
                        }
                        //Groups
                        else if (Client.Player.Group != null)
                        {
                            GamePlayer player = Client.Player;
                           

                            foreach (GamePlayer gPlayer in player.Group.GetPlayersInTheGroup())
                            {
                                if (gPlayer != null && gPlayer == gPlayer.Group.Leader)
                                {

                                  
                                    gLeader = gPlayer.Group.Leader;

                                    //leader is not in range
                                    if (gLeader != null && gLeader.IsWithinRadius(floorItem, 4000) == false)
                                    {
                                        gLeader = Client.Player;
                                    }
                                }
                                //log.ErrorFormat("Gruppe erhält keine loot! Owner = {0}", gLeader.Name);
                            }
                        }
                        else
                        {
                           // BattleGroup mybattlegroup = Client.Player.TempProperties.getProperty<BattleGroup>(BattleGroup.BATTLEGROUP_PROPERTY, null);

                            gLeader = Client.Player;
                              
                        }
                       


                        bool good = false;
                        if (floorItem.Item.IsStackable)
                            good = gLeader.Inventory.AddTemplate(GameInventoryItem.Create<InventoryItem>(floorItem.Item), floorItem.Item.Count, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
                        else
                            good = gLeader.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, floorItem.Item);
                        if (floorItem.Item.Realm != (int)gLeader.Realm && floorItem.Item.Realm != 0)
                        {
                            good = false;
                            gLeader.Out.SendMessage(LanguageMgr.GetTranslation(gLeader.Client.Account.Language, "GamePlayer.PickupObject.CantGetThat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return false;
                        }
                        if (!good)
                        {
                            gLeader.Out.SendMessage(LanguageMgr.GetTranslation(gLeader.Client.Account.Language, "GamePlayer.PickupObject.BackpackFull"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return false;
                        }
                        gLeader.Out.SendMessage(LanguageMgr.GetTranslation(gLeader.Client.Account.Language, "GamePlayer.PickupObject.YouGet", floorItem.Item.GetName(1, false), floorItem.Item.Count), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        Message.SystemToOthers(gLeader, LanguageMgr.GetTranslation(gLeader.Client.Account.Language, "GamePlayer.PickupObject.GroupMemberPicksUp", gLeader.Name, floorItem.Item.GetName(1, false)), eChatType.CT_System);
                        //InventoryLogging.LogInventoryAction("(ground)", this, eInventoryActionType.Loot, floorItem.Item.Template, floorItem.Item.IsStackable ? floorItem.Item.Count : 1);
                    }
                    floorItem.RemoveFromWorld();
                }
                return true;
            }
            else if (floorObject is GameMoney)
            {
                
                GameMoney moneyObject = floorObject as GameMoney;

                lock (moneyObject)
                {
                    if (moneyObject.ObjectState != eObjectState.Active)
                        return false;
                    

                    //Relic Gold Bonus
                    if (Properties.ENABLE_RELIC_DAMAGE_BONUS == false)
                    {

                        if (RelicMgr.GetRelicBonusModifier(Realm, eRelicType.Strength) > 0 || RelicMgr.GetRelicBonusModifier(Realm, eRelicType.Magic) > 0)
                        {
                            double eXtraGold = (RelicMgr.GetRelicBonusModifier(Realm, eRelicType.Strength) + RelicMgr.GetRelicBonusModifier(Realm, eRelicType.Magic)) * 100.0;

                            moneyObject.TotalCopper += moneyObject.TotalCopper * (long)eXtraGold / 100;
                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.PickupObject.RelicStrenghtBonus", eXtraGold), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                    }
                    BattleGroup battleGroup = TempProperties.getProperty<BattleGroup>(BattleGroup.BATTLEGROUP_PROPERTY, null);



                    if (Group != null && Group.AutosplitCoins || battleGroup != null && GS.ServerProperties.Properties.BATTLEGROUP_LOOT)
                    {
                        //log.ErrorFormat("Alle erhalten Coins? {0}", Group.AutosplitCoins);
                        if (battleGroup != null && GS.ServerProperties.Properties.BATTLEGROUP_LOOT)
                        {
                            //BattleGroup
                            foreach (GamePlayer players in battleGroup.Members.Keys)
                            {
                                long moneyToBtPlayer = moneyObject.TotalCopper / battleGroup.Members.Count;

                                if (players.IsAlive && players.CanSeeObject(floorObject) && players.ObjectState == eObjectState.Active)
                                {
                                    if (players.Guild != null && players.Guild.IsGuildDuesOn())
                                    {
                                        long moneyToGuild = moneyToBtPlayer * players.Guild.GetGuildDuesPercent() / 100;
                                        if (players.Guild.GetGuildDuesPercent() != 100)
                                            players.AddMoney(moneyToBtPlayer, LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.PickupObject.YourLootShare", Money.GetString(moneyToBtPlayer)));
                                        else
                                            players.AddMoney(moneyToBtPlayer);

                                        //InventoryLogging.LogInventoryAction("(ground)", eligibleMember, eInventoryActionType.Loot, moneyToPlayer);
                                        players.Guild.SetGuildBank(players, moneyToGuild);
                                    }
                                    else
                                    {
                                        players.AddMoney(moneyToBtPlayer, LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.PickupObject.YourLootShare", Money.GetString(moneyToBtPlayer)));
                                        //InventoryLogging.LogInventoryAction("(ground)", eligibleMember, eInventoryActionType.Loot, moneyToPlayer);
                                    }

                                }
                            }
                        }
                        else
                        {
                         

                            //Spread the money in the group
                            var eligibleMembers = from p in Group.GetPlayersInTheGroup()
                                                  where p.IsAlive && p.CanSeeObject(floorObject) && p.ObjectState == eObjectState.Active
                                                  select p;

                            if (!eligibleMembers.Any())
                            {
                                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.PickupObject.NoOneGroupWantsMoney"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return false;
                            }


                            long moneyToPlayer = moneyObject.TotalCopper / eligibleMembers.Count();


                            foreach (GamePlayer eligibleMember in eligibleMembers)
                            {
                                if (eligibleMember.Guild != null && eligibleMember.Guild.IsGuildDuesOn())
                                {
                                    long moneyToGuild = moneyToPlayer * eligibleMember.Guild.GetGuildDuesPercent() / 100;
                                    if (eligibleMember.Guild.GetGuildDuesPercent() != 100)
                                        eligibleMember.AddMoney(moneyToPlayer, LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.PickupObject.YourLootShare", Money.GetString(moneyToPlayer)));
                                    else
                                        eligibleMember.AddMoney(moneyToPlayer);

                                    //InventoryLogging.LogInventoryAction("(ground)", eligibleMember, eInventoryActionType.Loot, moneyToPlayer);
                                    eligibleMember.Guild.SetGuildBank(eligibleMember, moneyToGuild);
                                }
                                else
                                {
                                    eligibleMember.AddMoney(moneyToPlayer, LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.PickupObject.YourLootShare", Money.GetString(moneyToPlayer)));
                                    //InventoryLogging.LogInventoryAction("(ground)", eligibleMember, eInventoryActionType.Loot, moneyToPlayer);
                                }

                                //Spymaster Goldbonus
                                if (eligibleMember.TempProperties.getProperty<bool>(StealGold) == false && eligibleMember.GetSpellLine("ML1 Spymaster") != null)
                                {
                                    eligibleMember.TempProperties.setProperty(StealGold, true);

                                    if (eligibleMember.Group != null)
                                    {
                                        string gold = String.Empty;
                                        int pCount = eligibleMember.Group.MemberCount;

                                        foreach (GamePlayer Stealers in eligibleMember.Group.GetPlayersInTheGroup())
                                        {
                                           
                                            long money = 0;
                                            GamePlayer player = Stealers as GamePlayer;
                                            money += 20 * moneyObject.TotalCopper / 100;

                                            money = money / pCount;

                                            if (money > 0)
                                            {
                                                //long money = (long)(Money.GetMoney(0, 0, 17, 85, 0) * damagePercent * killedPlayer.Level / 50);


                                                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractServerRules.AddMoney"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                                player.AddMoney(money, "{0}");
                                                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractServerRules.AddMoney2", gold), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                                //InventoryLogging.LogInventoryAction(killer, player, eInventoryActionType.Other, money);
                                            }
                                            player.TempProperties.setProperty(StealGold, true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        BattleGroup mybattlegroup = (BattleGroup)TempProperties.getProperty<object>(BattleGroup.BATTLEGROUP_PROPERTY, null);
                        /*

                        GamePlayer gLeader = null;

                        if (Client.Player.Group != null)
                        {
                            GamePlayer player = Client.Player;


                            foreach (GamePlayer gPlayer in player.Group.GetPlayersInTheGroup())
                            {
                                if (gPlayer != null && gPlayer == gPlayer.Group.Leader)
                                {
                                    gLeader = gPlayer.Group.Leader;
                                }
                                //log.ErrorFormat("Gruppe erhält keine loot! Owner = {0}", gLeader.Name);
                            }
                        }
                        else
                        {
                            gLeader = Client.Player;
                        }
                        */
                        GamePlayer gLeader = null;



                        //BG Treasurer
                        if (mybattlegroup != null && mybattlegroup.GetBGTreasurer() != null)
                        {

                            gLeader = mybattlegroup.GetBGTreasurer();

                            //leader is not in range
                            if (gLeader != null && gLeader.IsWithinRadius(moneyObject, 4000) == false)
                            {
                                gLeader = Client.Player;
                            }
                            //log.ErrorFormat("BG leader = {0}", gLeader.Name);
                        }
                        //Groups
                        else if (Client.Player.Group != null)
                        {
                            GamePlayer player = Client.Player;


                            foreach (GamePlayer gPlayer in player.Group.GetPlayersInTheGroup())
                            {
                                if (gPlayer != null && gPlayer == gPlayer.Group.Leader)
                                {


                                    gLeader = gPlayer.Group.Leader;

                                    //leader is not in range
                                    if (gLeader != null && gLeader.IsWithinRadius(moneyObject, 4000) == false)
                                    {
                                        gLeader = Client.Player;
                                    }
                                }
                                //log.ErrorFormat("Gruppe erhält keine loot! Owner = {0}", gLeader.Name);
                            }
                        }
                        else
                        {
                            // BattleGroup mybattlegroup = Client.Player.TempProperties.getProperty<BattleGroup>(BattleGroup.BATTLEGROUP_PROPERTY, null);

                            gLeader = Client.Player;

                        }

                        //Add money only to picking player
                        if (Guild != null && Guild.IsGuildDuesOn() && moneyObject.TotalCopper > 0 && gLeader.Guild.GetGuildDuesPercent() > 0)
                        {
                            long moneyToGuild = moneyObject.TotalCopper * gLeader.Guild.GetGuildDuesPercent() / 100;
                            if (Guild.GetGuildDuesPercent() != 100)
                            {
                                gLeader.AddMoney(moneyObject.TotalCopper, LanguageMgr.GetTranslation(gLeader.Client.Account.Language, "GamePlayer.PickupObject.YouPickUp", Money.GetString(moneyObject.TotalCopper)));

                                //Spymaster Goldbonus
                                if (gLeader.GetSpellLine("ML1 Spymaster") != null)
                                {
                                    gLeader.TempProperties.setProperty(StealGold, true);
                                    string gold = String.Empty;
                                    long money = 0;
                                    GamePlayer player = gLeader as GamePlayer;
                                    money += 20 * moneyObject.TotalCopper / 100;

                                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractServerRules.AddMoney"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    player.AddMoney(money, "{0}");
                                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractServerRules.AddMoney2", gold), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    //InventoryLogging.LogInventoryAction(killer, player, eInventoryActionType.Other, money);
                                }
                            }
                            else
                            {
                                //Spymaster Goldbonus
                                if (gLeader.GetSpellLine("ML1 Spymaster") != null)
                                {
                                    gLeader.TempProperties.setProperty(StealGold, true);
                                    string gold = String.Empty;
                                    long money = 0;
                                    GamePlayer player = gLeader as GamePlayer;
                                    money += 20 * moneyObject.TotalCopper / 100;
                                    
                                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractServerRules.AddMoney"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    player.AddMoney(money, "{0}");
                                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractServerRules.AddMoney2", gold), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    //InventoryLogging.LogInventoryAction(killer, player, eInventoryActionType.Other, money);
                                }

                                gLeader.AddMoney(moneyObject.TotalCopper);
                            }
                            //InventoryLogging.LogInventoryAction("(ground)", this, eInventoryActionType.Loot, moneyObject.TotalCopper);
                            gLeader.Guild.SetGuildBank(this, moneyToGuild);
                        }
                        else
                        {
                           
                            gLeader.AddMoney(moneyObject.TotalCopper, LanguageMgr.GetTranslation(gLeader.Client.Account.Language, "GamePlayer.PickupObject.YouPickUp", Money.GetString(moneyObject.TotalCopper)));
                            //InventoryLogging.LogInventoryAction("(ground)", this, eInventoryActionType.Loot, moneyObject.TotalCopper);

                            //Spymaster Goldbonus
                            if (gLeader.TempProperties.getProperty<bool>(StealGold) == false &&  gLeader.GetSpellLine("ML1 Spymaster") != null)
                            {
                                gLeader.TempProperties.setProperty(StealGold, true);
                                string gold = String.Empty;
                                long money = 0;
                                GamePlayer player = gLeader as GamePlayer;
                                   money += 20 * moneyObject.TotalCopper / 100;
                               
                                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractServerRules.AddMoney"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                player.AddMoney(money, "{0}");
                                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractServerRules.AddMoney2", gold), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                //InventoryLogging.LogInventoryAction(killer, player, eInventoryActionType.Other, money);
                            }
                            

                        }
                    }
                    Client.Player.TempProperties.removeProperty(StealGold);
                    moneyObject.Delete();
                   
                    return true;
                }
            }
            else if (floorObject is GameBoat)
            {
                if (!this.IsWithinRadius(floorObject, 1000))
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.PickupObject.TooFarFromBoat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return false;
                }


                if (!InCombat)
                    MountSteed(floorObject as GameBoat, false);

                return true;
            }
            else if (floorObject is GameHouseVault && floorObject.CurrentHouse != null)
            {
                GameHouseVault houseVault = floorObject as GameHouseVault;
                if (houseVault.Detach(this))
                {
                    ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>(houseVault.TemplateID);
                    Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, GameInventoryItem.Create<ItemTemplate>(template));
                    //InventoryLogging.LogInventoryAction("(HOUSE;" + floorObject.CurrentHouse.HouseNumber.ToString() + ")", this, eInventoryActionType.Other, template);
                }
                return true;
            }
            else if ((floorObject is GameNPC || floorObject is GameStaticItem) && floorObject.CurrentHouse != null)
            {
                floorObject.CurrentHouse.EmptyHookpoint(this, floorObject);
                return true;
            }
            else
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.PickupObject.CantGetThat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;

            }
        }


        /// <summary>
        /// Checks to see if an object is viewable from the players perspective
        /// </summary>
        /// <param name="obj">The Object to be seen</param>
        /// <returns>True/False</returns>
        public bool CanSeeObject(GameObject obj)
        {
            return IsWithinRadius(obj, WorldMgr.VISIBILITY_DISTANCE);
        }

        /// <summary>
        /// Checks to see if an object is viewable from the players perspective
        /// </summary>
        /// <param name="player">The Player that can see</param>
        /// <param name="obj">The Object to be seen</param>
        /// <returns>True/False</returns>
        public static bool CanSeeObject(GamePlayer player, GameObject obj)
        {
            return player.IsWithinRadius(obj, WorldMgr.VISIBILITY_DISTANCE);
        }

        #endregion

        #region Database

        /// <summary>
        /// Subtracts the current time from the last time the character was saved
        /// and adds it in the form of seconds to player.PlayedTime
        /// for the /played command.
        /// </summary>
        public long PlayedTime
        {
            get
            {
                DateTime rightNow = DateTime.Now;
                DateTime oldLast = DBCharacter.LastPlayed;
                // Get the total amount of time played between now and lastplayed
                // This is safe as lastPlayed is updated on char load.
                TimeSpan playaPlayed = rightNow.Subtract(oldLast);
                TimeSpan newPlayed = playaPlayed + TimeSpan.FromSeconds(DBCharacter.PlayedTime);
                return (long)newPlayed.TotalSeconds;
            }
        }

        /// <summary>
        /// Saves the player's skills
        /// </summary>
        protected virtual void SaveSkillsToCharacter()
        {
            StringBuilder ab = new StringBuilder();
            StringBuilder sp = new StringBuilder();
            StringBuilder styleList = new StringBuilder();

            lock (((ICollection)m_skillList).SyncRoot)
            {
                foreach (Skill skill in m_skillList)
                {
                    Ability ability = skill as Ability;
                    // foreach (Ability ability in m_usableSkills.Where(e => e.Item1 is Ability).Select(e => e.Item1).Cast<Ability>())
                    if (ability != null)
                    {
                        if (ab.Length > 0)
                        {
                            ab.Append(";");
                        }
                        ab.AppendFormat("{0}|{1}", ability.KeyName, ability.Level);
                    }
                }
            }

            foreach (Specialization spec in m_specList)
            {
                if (sp.Length > 0)
                {
                    sp.Append(";");
                }
                //sp += spec.KeyName + "|" + spec.Level;
                sp.AppendFormat("{0}|{1}", spec.KeyName, spec.Level);
            }


            foreach (Style style in m_styles.Values)
            {
                if (styleList.Length > 0)
                {
                    styleList.Append(";");
                }
                //styleList += style.ID;
                styleList.AppendFormat("{0}|{1}", style.ID, style.Level);
            }


            StringBuilder disabledSpells = new StringBuilder();
            StringBuilder disabledAbilities = new StringBuilder();

            ICollection<Skill> disabledSkills = GetAllDisabledSkills();

            foreach (Skill skill in disabledSkills)
            {
                int duration = GetSkillDisabledDuration(skill);
                if (duration <= 0)
                    continue;


                if (skill is Spell)
                {
                    Spell spl = (Spell)skill;
                    if (disabledSpells.Length > 0)
                        disabledSpells.Append(";");


                    disabledSpells.AppendFormat("{0}|{1}", spl.ID, duration);
                }
                else if (skill is Ability)
                {
                    Ability ability = (Ability)skill;

                    if (disabledAbilities.Length > 0)
                        disabledAbilities.Append(";");

                    disabledAbilities.AppendFormat("{0}|{1}", ability.KeyName, duration);
                }
                else
                {
                    if (log.IsWarnEnabled)
                        log.WarnFormat("{0}: Can't save disabled skill {1}", Name, skill.GetType().ToString());
                }

            }

            StringBuilder spellLines = new StringBuilder();
            lock (((ICollection)m_spellLines).SyncRoot)
            {
                foreach (SpellLine line in m_spellLines)
                {
                    if (spellLines.Length > 0)
                        spellLines.Append(';');
                    spellLines.AppendFormat("{0}|{1}", line.KeyName, line.Level);
                }

            }
            DBCharacter.SerializedAbilities = ab.ToString();
            DBCharacter.SerializedSpecs = sp.ToString();
            DBCharacter.SerializedSpellLines = spellLines.ToString();
            DBCharacter.DisabledSpells = disabledSpells.ToString();
            DBCharacter.DisabledAbilities = disabledAbilities.ToString();

        }

        /// <summary>
        /// Loads the Skills from the Character
        /// Called after the default skills / level have been set!
        /// </summary>
        protected virtual void LoadSkillsFromCharacter()
        {
            DOLCharacters character = DBCharacter; // if its derived and filled with some code
            if (character == null) return; // no character => exit

            string tmpStr;

            #region Load Abilities
            //Load up the player skills
            //1. Load all abilities
            //2. Disable appropriate abilities
            lock (((ICollection)m_skillList).SyncRoot)
            {
                tmpStr = character.SerializedAbilities;
                if (tmpStr != null && tmpStr.Length > 0)
                {
                    //Add the abilities to our skill list!
                    foreach (string ability in tmpStr.SplitCSV())
                    {
                        string[] values = ability.Split('|');
                        if (values.Length >= 2 && !HasAbility(values[0]))
                        {
                            string keyname = values[0];

                            if (int.TryParse(values[1], out int level))
                                AddAbility(SkillBase.GetAbility(keyname, level), true);
                        }
                    }

                    //Since we added all the abilities that this character has, let's now disable the disabled ones!
                    tmpStr = character.DisabledAbilities;
                    if (tmpStr != null && tmpStr.Length > 0)
                    {
                        foreach (string str in tmpStr.SplitCSV())
                        {
                            string[] values = str.Split('|');
                            if (values.Length >= 2)
                            {
                                string keyname = values[0];

                                if (HasAbility(keyname) && int.TryParse(values[1], out int duration))
                                    DisableSkill(GetAbility(keyname), duration);
                                else if (log.IsErrorEnabled)
                                    log.Error(Name + ": error in loading disabled abilities => '" + tmpStr + "'");
                            }
                        }
                    }
                }
            }
            #endregion

            #region Load Specs
           
                tmpStr = character.SerializedSpecs;
                if (tmpStr != null && tmpStr.Length > 0)
                {
                    foreach (string spec in tmpStr.SplitCSV())
                    {
                        string[] values = spec.Split('|');
                        if (values.Length >= 2)
                        {
                            Specialization tempSpec = SkillBase.GetSpecialization(values[0], false);

                            if (tempSpec != null)
                            {

                                if (int.TryParse(values[1], out int level))
                                {
                                    tempSpec.Level = level;
                                    if (!HasSpecialization(values[0]))
                                        AddSpecialization(tempSpec);
                                    CharacterClass.OnSkillTrained(this, tempSpec);
                                }
                                else if (log.IsErrorEnabled)
                                {
                                    log.Error(Name + ": error in loading specs => '" + tmpStr + "'");
                                }
                            }
                            else if (log.IsErrorEnabled)
                            {
                                log.Error(Name + ": can't find spec '" + values[0] + "'");
                            }
                        }
                   
                }
            }
            #endregion

            LoadSpellLines();
            CharacterClass.OnLevelUp(this, Level); // load all skills from DB first to keep the order
            CharacterClass.OnRealmLevelUp(this);
            RefreshSpecDependantSkills(false);
            UpdateSpellLineLevels(false);
        }

        public virtual void LoadSpellLines()
        {
            DOLCharacters character = DBCharacter; // if its derived and filled with some code
            if (character == null) return; // no character => exit

            RemoveAllSpellLines();

            Dictionary<ushort, int> disabledSpells = new Dictionary<ushort, int>();

            //Load the disabled spells
            string tmpStr = character.DisabledSpells;
            if (tmpStr != null && tmpStr.Length > 0)
            {
                foreach (string str in tmpStr.SplitCSV())
                {
                    string[] values = str.Split('|');

                    if (values.Length >= 2 && ushort.TryParse(values[0], out ushort spellid) && int.TryParse(values[1], out int duration))
                    {
                        if (disabledSpells.ContainsKey(spellid))
                            continue;
                        disabledSpells.Add(spellid, duration);
                    }
                    else if (log.IsErrorEnabled)
                        log.Error(Name + ": error in loading disabled spells => '" + tmpStr + "'");
                }
            }

            lock (((ICollection)m_spellLines).SyncRoot)
            {
                tmpStr = character.SerializedSpellLines;
                if (tmpStr != null && tmpStr.Length > 0)
                {
                    foreach (string serializedSpellLine in tmpStr.SplitCSV())
                    {
                        string[] values = serializedSpellLine.Split('|');
                        if (values.Length >= 2)
                        {
                            if (values[0] == ChampionSpellLineName)
                                continue; // LoadChampionSpells takes care of adding the spell line

                            SpellLine splLine = SkillBase.GetSpellLine(values[0], false);

                            if (splLine != null)
                            {

                                if (int.TryParse(values[1], out int level))
                                {
                                    splLine.Level = level;
                                    AddSpellLine(splLine);

                                    foreach (Spell spell in SkillBase.GetSpellList(splLine.KeyName))
                                    {
                                        if (disabledSpells.ContainsKey(spell.ID))
                                            DisableSkill(spell, disabledSpells[spell.ID]);
                                    }
                                }
                                else if (log.IsErrorEnabled)
                                {
                                    log.Error("Error loading SpellLine '" + serializedSpellLine + "' from character '" + character.Name + "'");
                                }
                            }
                            else if (log.IsErrorEnabled)
                            {
                                log.Error("Can't find SpellLine '" + values[0] + "' for character '" + character.Name + "'");
                            }
                        }
                    }
                }

                LoadChampionSpells(disabledSpells);
            }
        }


        /// <summary>
        /// Verify this player has the correct number of spec points for the players level
        /// </summary>
        public virtual bool VerifySpecPoints()
        {
            // calc normal spec points for the level & classe
            int allpoints = -1;
            for (int i = 1; i <= Level; i++)
            {
                if (i <= 5) allpoints += i; //start levels
                if (i > 5) allpoints += CharacterClass.SpecPointsMultiplier * i / 10; //normal levels
                if (i > 40) allpoints += CharacterClass.SpecPointsMultiplier * (i - 1) / 20; //half levels
            }
            if (IsLevelSecondStage == true && Level != 50)
                allpoints += CharacterClass.SpecPointsMultiplier * Level / 20; // add current half level

            // calc spec points player have (autotrain is not anymore processed here - 1.87 livelike)
            int mypoints = SkillSpecialtyPoints;
            foreach (Specialization spec in GetSpecList())
            {
                mypoints += (spec.Level * (spec.Level + 1) - 2) / 2;
                mypoints -= GetAutoTrainPoints(spec, 0);
            }

            // check if correct, if not respec. Not applicable to GMs
            SpecPointsOk = true;
            if (allpoints != mypoints)
            {



                if (Client.Account.PrivLevel == 1)
                {
                    log.ErrorFormat("Spec points total for player {0} incorrect: {1} instead of {2}.", Name, mypoints.ToString(), allpoints.ToString());
                    RespecPlayerWrongPoints(this);

                    SkillSpecialtyPoints = allpoints;
                    SpecPointsOk = false;

                    Out.SendUpdatePoints();
                    Out.SendUpdatePlayer();
                    SaveIntoDatabase();

                }

            }

            return SpecPointsOk;
        }
        public virtual void RespecPlayerWrongPoints(GamePlayer player)
        {
            int specPoints = 0;



            specPoints = player.RespecAll();

            if (specPoints > 0)
            {
                player.SkillSpecialtyPoints += specPoints;
                player.RemoveAllStyles(); // Kill styles
                player.UpdateSpellLineLevels(false);
            }

            player.Out.SendUpdatePlayerSkills();
            player.Out.SendUpdatePoints();
            player.Out.SendUpdatePlayer();
            player.SaveIntoDatabase();
            player.TempProperties.setProperty(PlayerRespecMSG, true);
           

        }
        /// <summary>
        /// Loads this player from a character table slot
        /// </summary>
        /// <param name="obj">DOLCharacter</param>
        public override void LoadFromDatabase(DataObject obj)
        {
            base.LoadFromDatabase(obj);
            if (!(obj is DOLCharacters))
                return;
            m_dbCharacter = (DOLCharacters)obj;

            Model = (ushort)m_dbCharacter.CurrentModel;

            m_customFaceAttributes[(int)eCharFacePart.EyeSize] = m_dbCharacter.EyeSize;
            m_customFaceAttributes[(int)eCharFacePart.LipSize] = m_dbCharacter.LipSize;
            m_customFaceAttributes[(int)eCharFacePart.EyeColor] = m_dbCharacter.EyeColor;
            m_customFaceAttributes[(int)eCharFacePart.HairColor] = m_dbCharacter.HairColor;
            m_customFaceAttributes[(int)eCharFacePart.FaceType] = m_dbCharacter.FaceType;
            m_customFaceAttributes[(int)eCharFacePart.HairStyle] = m_dbCharacter.HairStyle;
            m_customFaceAttributes[(int)eCharFacePart.MoodType] = m_dbCharacter.MoodType;

            #region guild handling
            //TODO: overwork guild handling (VaNaTiC)
            m_guildId = m_dbCharacter.GuildID;
            if (m_guildId != null)
                m_guild = GuildMgr.GetGuildByGuildID(m_guildId);
            else
                m_guild = null;

            if (m_guild != null)
            {
                foreach (DBRank rank in m_guild.Ranks)
                {
                    if (rank == null) continue;
                    if (rank.RankLevel == m_dbCharacter.GuildRank)
                    {
                        m_guildRank = rank;
                        break;
                    }
                }

                m_guildName = m_guild.Name;
                m_guild.AddOnlineMember(this);
            }
            #endregion

            #region setting world-init-position (delegate to PlayerCharacter dont make sense)
            m_x = m_dbCharacter.Xpos;
            m_y = m_dbCharacter.Ypos;
            m_z = m_dbCharacter.Zpos;
            m_Heading = (ushort)m_dbCharacter.Direction;
            //important, use CurrentRegion property
            //instead because it sets the Region too
            CurrentRegionID = (ushort)m_dbCharacter.Region;
            if (CurrentRegion == null || CurrentRegion.GetZone(m_x, m_y) == null)
            {
                log.WarnFormat("Invalid region/zone on char load ({0}): x={1} y={2} z={3} reg={4}; moving to bind point.", m_dbCharacter.Name, X, Y, Z, m_dbCharacter.Region);
                m_x = m_dbCharacter.BindXpos;
                m_y = m_dbCharacter.BindYpos;
                m_z = m_dbCharacter.BindZpos;
                m_Heading = (ushort)m_dbCharacter.BindHeading;
                CurrentRegionID = (ushort)m_dbCharacter.BindRegion;
            }

            for (int i = 0; i < m_lastUniqueLocations.Length; i++)
            {
                m_lastUniqueLocations[i] = new GameLocation(null, CurrentRegionID, m_x, m_y, m_z);
            }
            #endregion

            // stats first
            m_charStat[eStat.STR - eStat._First] = (short)m_dbCharacter.Strength;
            m_charStat[eStat.DEX - eStat._First] = (short)m_dbCharacter.Dexterity;
            m_charStat[eStat.CON - eStat._First] = (short)m_dbCharacter.Constitution;
            m_charStat[eStat.QUI - eStat._First] = (short)m_dbCharacter.Quickness;
            m_charStat[eStat.INT - eStat._First] = (short)m_dbCharacter.Intelligence;
            m_charStat[eStat.PIE - eStat._First] = (short)m_dbCharacter.Piety;
            m_charStat[eStat.EMP - eStat._First] = (short)m_dbCharacter.Empathy;
            m_charStat[eStat.CHR - eStat._First] = (short)m_dbCharacter.Charisma;

            SetCharacterClass(m_dbCharacter.Class);

            m_currentSpeed = 0;
            if (MaxSpeedBase == 0)
                MaxSpeedBase = PLAYER_BASE_SPEED;

            m_inventory.LoadFromDatabase(InternalID);

            SwitchQuiver((eActiveQuiverSlot)(m_dbCharacter.ActiveWeaponSlot & 0xF0), false);
            SwitchWeapon((eActiveWeaponSlot)(m_dbCharacter.ActiveWeaponSlot & 0x0F));



            if (m_dbCharacter.PlayedTime == 0)
            {
                if (Properties.STARTING_LEVEL > 1 && m_dbCharacter.Experience < GetExperienceNeededForLevel(Properties.STARTING_LEVEL - 1))
                {
                    m_dbCharacter.Experience = GetExperienceNeededForLevel(Properties.STARTING_LEVEL - 1);
                    m_dbCharacter.Level = Properties.STARTING_LEVEL;
                }


                Health = MaxHealth;
                Mana = MaxMana;
                Endurance = MaxEndurance; // has to be set after max, same applies to other values with max properties
            }
            else
            {
                Health = m_dbCharacter.Health;
                Mana = m_dbCharacter.Mana;
                Endurance = m_dbCharacter.Endurance; // has to be set after max, same applies to other values with max properties
            }

            if (Health <= 0)
            {
                Health = 1;
            }

            if (RealmLevel == 0)
                RealmLevel = CalculateRealmLevelFromRPs(RealmPoints);

            //Need to load the skills at the end, so the stored values modify the
            //existing skill levels for this player
            LoadSkillsFromCharacter();

            LoadCraftingSkills();

            VerifySpecPoints();

            LoadQuests();

            // Load Task object of player ...
            var tasks = GameServer.Database.SelectObjects<DBTask>("`Character_ID` = @Character_ID", new QueryParameter("@Character_ID", InternalID));
            if (tasks.Count == 1)
            {
                m_questTask = AbstractTask.LoadFromDatabase(this, tasks[0]);
            }
            else if (tasks.Count > 1)
            {
                if (log.IsErrorEnabled)
                    log.Error("More than one DBTask Object found for player " + Name);
            }

            // Load ML steps of player ...
            //var mlsteps = GameServer.Database.SelectObjects<DBCharacterXMasterLevel>("Character_ID ='" + GameServer.Database.Escape(QuestPlayerID) + "'");
            var mlsteps = GameServer.Database.SelectObjects<DBCharacterXMasterLevel>("`Character_ID` = @Character_ID", new QueryParameter("@Character_ID", QuestPlayerID));
            if (mlsteps.Count > 0)
            {
                foreach (DBCharacterXMasterLevel mlstep in mlsteps)
                    m_mlSteps.Add(mlstep);
            }

            m_previousLoginDate = m_dbCharacter.LastPlayed;

            // Has to be updated on load to ensure time offline isn't added to character /played.
            m_dbCharacter.LastPlayed = DateTime.Now;
            m_dbCharacter.LastDailyRewardTime = m_dbCharacter.LastDailyRewardTime;

            m_titles.Clear();
            m_titles.AddRange(PlayerTitleMgr.GetPlayerTitles(this));
            IPlayerTitle t = PlayerTitleMgr.GetTitleByTypeName(m_dbCharacter.CurrentTitleType);
            if (t == null)
                t = PlayerTitleMgr.ClearTitle;
            m_currentTitle = t;

            //let's only check if we can use /level once shall we,
            //this is nice because i want to check the property often for the new catacombs classes

            //find all characters in the database
            foreach (DOLCharacters plr in Client.Account.Characters)
            {
                //where the level of one of the characters if 50
                if (plr.Level == ServerProperties.Properties.SLASH_LEVEL_REQUIREMENT && GameServer.ServerRules.CountsTowardsSlashLevel(plr))
                {
                    m_canUseSlashLevel = true;
                    break;
                }
            }

            // check the account for the Muted flag
            if (Client.Account.IsMuted)
                IsMuted = true;
        }


        /// <summary>
        /// Save the player into the database
        /// </summary>
        public override void SaveIntoDatabase()
        {
            try
            {
                // Ff this player is a GM always check and set the IgnoreStatistics flag
                if (Client.Account.PrivLevel > 1 && m_dbCharacter.IgnoreStatistics == false)
                {
                    m_dbCharacter.IgnoreStatistics = true;
                }

                SaveSkillsToCharacter();
                SaveCraftingSkills();
                FactionMgr.SaveAllAggroToFaction();

                m_dbCharacter.PlayedTime = PlayedTime;  //We have to set the PlayedTime on the character before setting the LastPlayed
                m_dbCharacter.LastPlayed = DateTime.Now;
                m_dbCharacter.LastDailyRewardTime = LastDailyRewardTime;
                              

                m_dbCharacter.ActiveWeaponSlot = (byte)((byte)ActiveWeaponSlot | (byte)ActiveQuiverSlot);
                if (m_stuckFlag)
                {
                    lock (m_lastUniqueLocations)
                    {
                        GameLocation loc = m_lastUniqueLocations[m_lastUniqueLocations.Length - 1];
                        m_dbCharacter.Xpos = loc.X;
                        m_dbCharacter.Ypos = loc.Y;
                        m_dbCharacter.Zpos = loc.Z;
                        m_dbCharacter.Region = loc.RegionID;
                        m_dbCharacter.Direction = loc.Heading;
                    }
                }
                GameServer.Database.SaveObject(m_dbCharacter);
                Inventory.SaveIntoDatabase(InternalID);

                DOLCharacters cachedCharacter = null;

                foreach (DOLCharacters accountChar in Client.Account.Characters)
                {
                    if (accountChar.ObjectId == InternalID)
                    {
                        cachedCharacter = accountChar;
                        break;
                    }
                }

                if (cachedCharacter != null)
                {
                    cachedCharacter = m_dbCharacter;
                }


                if (m_mlSteps != null)
                {
                    GameServer.Database.SaveObject(m_mlSteps.OfType<DBCharacterXMasterLevel>());
                }

                if (log.IsInfoEnabled)
                    log.Info(m_dbCharacter.Name + " saved!");
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.SaveIntoDatabase.CharacterSaved"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
            catch (Exception e)
            {
                if (log.IsErrorEnabled)
                    log.Error("Error saving player " + Name + "!", e);
            }
        }

        #endregion

        #region CustomDialog

        /// <summary>
        /// Holds the delegates that calls
        /// </summary>
        public CustomDialogResponse m_customDialogCallback;

        /// <summary>
        /// Gets/sets the custom dialog callback
        /// </summary>
        public CustomDialogResponse CustomDialogCallback
        {
            get { return m_customDialogCallback; }
            set { m_customDialogCallback = value; }
        }

        #endregion

        #region GetPronoun/GetExamineMessages

        /// <summary>
        /// Pronoun of this player in case you need to refer it in 3rd person
        /// http://webster.commnet.edu/grammar/cases.htm
        /// </summary>
        /// <param name="firstLetterUppercase"></param>
        /// <param name="form">0=Subjective, 1=Possessive, 2=Objective</param>
        /// <returns>pronoun of this object</returns>
        public override string GetPronoun(int form, bool firstLetterUppercase)
        {
            if (DBCharacter.Gender == 0) // male
                switch (form)
                {
                    default:
                        // Subjective
                        if (firstLetterUppercase)
                            return "He";
                        else
                            return "he";
                    case 1:
                        // Possessive
                        if (firstLetterUppercase)
                            return "His";
                        else
                            return "his";
                    case 2:
                        // Objective
                        if (firstLetterUppercase)
                            return "Him";
                        else
                            return "him";
                }
            else
                // female
                switch (form)
                {
                    default:
                        // Subjective
                        if (firstLetterUppercase)
                            return "She";
                        else
                            return "she";
                    case 1:
                        // Possessive
                        if (firstLetterUppercase)
                            return "Her";
                        else
                            return "her";
                    case 2:
                        // Objective
                        if (firstLetterUppercase)
                            return "Her";
                        else
                            return "her";
                }
        }

        public string GetPronoun(GameClient Client, int form, bool capitalize)
        {
            if (Gender == eGender.Male)
                switch (form)
                {
                    default:
                        return Capitalize(capitalize, LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Pronoun.Male.Subjective"));
                    case 1:
                        return Capitalize(capitalize, LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Pronoun.Male.Possessive"));
                    case 2:
                        return Capitalize(capitalize, LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Pronoun.Male.Objective"));
                }
            else
                switch (form)
                {
                    default:
                        return Capitalize(capitalize, LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Pronoun.Female.Subjective"));
                    case 1:
                        return Capitalize(capitalize, LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Pronoun.Female.Possessive"));
                    case 2:
                        return Capitalize(capitalize, LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Pronoun.Female.Objective"));
                }
        }

        public string GetName(GamePlayer target)
        {
            return GameServer.ServerRules.GetPlayerName(this, target);
        }

        /// <summary>
        /// Adds messages to ArrayList which are sent when object is targeted
        /// </summary>
        /// <param name="player">GamePlayer that is examining this object</param>
        /// <returns>list with string messages</returns>
        public override IList GetExamineMessages(GamePlayer player)
        {
            // TODO: PvP & PvE messages
            IList list = base.GetExamineMessages(player);

            string message;
            switch (GameServer.Instance.Configuration.ServerType)
            {//FIXME: Better extract this to a new function in ServerRules !!! (VaNaTiC)
                case eGameServerType.GST_Normal:
                    {
                        if (Realm == player.Realm || Client.Account.PrivLevel > 1 || player.Client.Account.PrivLevel > 1)
                            message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.GetExamineMessages.RealmMember", player.GetName(this), GetPronoun(Client, 0, true), CharacterClass.Name);
                        else
                            message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.GetExamineMessages.EnemyRealmMember", player.GetName(this), GetPronoun(Client, 0, true));
                        break;
                    }

                case eGameServerType.GST_PvP:
                    {
                        if (Client.Account.PrivLevel > 1 || player.Client.Account.PrivLevel > 1)
                            message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.GetExamineMessages.YourGuildMember", player.GetName(this), GetPronoun(Client, 0, true), CharacterClass.Name);
                        else if (Guild == null)
                            message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.GetExamineMessages.NeutralMember", player.GetName(this), GetPronoun(Client, 0, true));
                        else if (Guild == player.Guild || Client.Account.PrivLevel > 1 || player.Client.Account.PrivLevel > 1)
                            message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.GetExamineMessages.YourGuildMember", player.GetName(this), GetPronoun(Client, 0, true), CharacterClass.Name);
                        else
                            message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.GetExamineMessages.OtherGuildMember", player.GetName(this), GetPronoun(Client, 0, true), GuildName);
                        break;
                    }

                default:
                    {
                        message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.GetExamineMessages.YouExamine", player.GetName(this));
                        break;
                    }
            }

            list.Add(message);
            return list;
        }

        #endregion

        #region Stealth / Wireframe

        bool m_isWireframe = false;

        /// <summary>
        /// Player is drawn as a Wireframe.  Not sure why or when this is used.  -- Tolakram
        /// </summary>
        public bool IsWireframe
        {
            get { return m_isWireframe; }
            set
            {
                bool needUpdate = m_isWireframe != value;
                m_isWireframe = value;
                if (needUpdate && ObjectState == eObjectState.Active)
                    foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    {
                        if (player == null) continue;
                        player.Out.SendPlayerModelTypeChange(this, (byte)(value ? 1 : 0));
                    }
            }
        }

        private bool m_isTorchLighted = false;

        /// <summary>
        /// Is player Torch lighted ?
        /// </summary>
        public bool IsTorchLighted
        {
            get { return m_isTorchLighted; }
            set { m_isTorchLighted = value; }
        }


        /// <summary>
        /// Property that holds tick when stealth state was changed last time
        /// </summary>
        public const string STEALTH_CHANGE_TICK = "StealthChangeTick";
        /// <summary>
        /// Holds the stealth effect
        /// </summary>
        protected StealthEffect m_stealthEffect = null;
        /// <summary>
        /// The stealth state of this player
        /// </summary>
        public override bool IsStealthed
        {
            get { return m_stealthEffect != null; }
        }
        public static void Unstealth(DOLEvent ev, object sender, EventArgs args)
        {
            AttackedByEnemyEventArgs atkArgs = args as AttackedByEnemyEventArgs;
            GamePlayer player = sender as GamePlayer;
            if (player == null || atkArgs == null) return;

            if (atkArgs.AttackData.AttackResult != eAttackResult.HitUnstyled && atkArgs.AttackData.AttackResult != eAttackResult.HitStyle) return;
            if (atkArgs.AttackData.Damage == -1) return;

            player.Stealth(false);
        }



        /// <summary>
        /// Set player's stealth state
        /// </summary>
        /// <param name="goStealth">true is stealthing, false if unstealthing</param>
        public virtual void Stealth(bool goStealth)
        {
            if (IsStealthed == goStealth)
                return;


            if (goStealth && CraftTimer != null && CraftTimer.IsAlive)
            {
                Out.SendMessage("You can't stealth while crafting!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (IsOnHorse || IsSummoningMount)
                IsOnHorse = false;


            if (IsRiding && Steed is GameSiegeRam)
            {
                Out.SendMessage("You can't stealth while using a siegeweapon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }


            UncoverStealthAction action = (UncoverStealthAction)TempProperties.getProperty<object>(UNCOVER_STEALTH_ACTION_PROP, null);
            if (goStealth)
            {
                //start the uncover timer
                if (action == null)
                    action = new UncoverStealthAction(this);
                action.Interval = 2000;
                action.Start(2000);
                TempProperties.setProperty(UNCOVER_STEALTH_ACTION_PROP, action);

                if (ObjectState == eObjectState.Active)
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Stealth.NowHidden"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                Out.SendPlayerModelTypeChange(this, 3);
                m_stealthEffect = new StealthEffect();
                m_stealthEffect.Start(this);
                Sprint(false);
                GameEventMgr.AddHandler(this, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(Unstealth));
                foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    if (player == null) continue;
                    if (player == this) continue;
                    if (!player.CanDetect(this))
                        player.Out.SendObjectDelete(this);
                }
            }
            else
            {
                //stop the uncover timer
                if (action != null)
                {
                    action.Stop();
                    TempProperties.removeProperty(UNCOVER_STEALTH_ACTION_PROP);
                }

                if (ObjectState == eObjectState.Active)
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Stealth.NoLongerHidden"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                CamouflageEffect cam = EffectList.GetOfType<CamouflageEffect>();
                if (cam != null)
                {
                    cam.Stop();
                }
                //Andraste
                try
                {
                    GameSpellEffect effect = SpellHandler.FindEffectOnTarget(this, "BlanketOfCamouflage");
                    if (effect != null) effect.Cancel(false);
                }
                catch (Exception) { }

                Out.SendPlayerModelTypeChange(this, 2);
                if (m_stealthEffect != null) m_stealthEffect.Stop();
                m_stealthEffect = null;
                GameEventMgr.RemoveHandler(this, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(Unstealth));
                foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    if (player == null) continue;
                    //TODO: more correct way to do it
                    if (player == this) continue;
                    //if a player could see us stealthed, we just update our model to avoid untargetting.
                    if (player.CanDetect(this))
                        player.Out.SendPlayerModelTypeChange(this, 2);
                    else
                        player.Out.SendPlayerCreate(this);
                    player.Out.SendLivingEquipmentUpdate(this);
                }
            }
            Notify(GamePlayerEvent.StealthStateChanged, this, null);
            Out.SendUpdateMaxSpeed();
        }

        /// <summary>
        /// The temp property that stores the uncover stealth action
        /// </summary>
        protected const string UNCOVER_STEALTH_ACTION_PROP = "UncoverStealthAction";

        /// <summary>
        /// Uncovers the player if a mob is too close
        /// </summary>
        protected class UncoverStealthAction : RegionAction
        {
            /// <summary>
            /// Constructs a new uncover stealth action
            /// </summary>
            /// <param name="actionSource">The action source</param>
            public UncoverStealthAction(GamePlayer actionSource)
                : base(actionSource)
            {
            }

            /// <summary>
            /// Called on every timer tick
            /// </summary>
            protected override void OnTick()
            {
                GamePlayer player = (GamePlayer)m_actionSource;
                if (player.Client.Account.PrivLevel > 1) return;

                bool checklos = false;
                foreach (AbstractArea area in player.CurrentAreas)
                {
                    if (area.CheckLOS)
                    {
                        checklos = true;
                        break;
                    }
                }

                foreach (GameNPC npc in player.GetNPCsInRadius(1024))
                {
                    // Friendly mobs do not uncover stealthed players
                    if (!GameServer.ServerRules.IsAllowedToAttack(npc, player, true)) continue;

                    // Npc with player owner don't uncover
                    if (npc.Brain != null
                        && (npc.Brain as IControlledBrain) != null
                        && (npc.Brain as IControlledBrain).GetPlayerOwner() != null) continue;

                    double npcLevel = Math.Max(npc.Level, 1.0);
                    double stealthLevel = player.GetModifiedSpecLevel(Specs.Stealth);
                    double detectRadius = 125.0 + ((npcLevel - stealthLevel) * 20.0);

                    // we have detect hidden and enemy don't = higher range
                    if (npc.HasAbility(Abilities.DetectHidden) && player.EffectList.GetOfType<CamouflageEffect>() == null)
                    {
                        detectRadius += 125;
                    }

                    if (detectRadius < 126) detectRadius = 126;

                    double distanceToPlayer = npc.GetDistanceTo(player);

                    if (distanceToPlayer > detectRadius)
                        continue;

                    double fieldOfView = 90.0;  //90 degrees  = standard FOV
                    double fieldOfListen = 120.0; //120 degrees = standard field of listening
                    if (npc.Level > 50)
                    {
                        fieldOfListen += (npc.Level - player.Level) * 3;
                    }

                    double angle = npc.GetAngle(player);

                    //player in front
                    fieldOfView /= 2.0;
                    bool canSeePlayer = (angle >= 360 - fieldOfView || angle < fieldOfView);

                    //If npc can not see nor hear the player, continue the loop
                    fieldOfListen /= 2.0;
                    if (canSeePlayer == false &&
                        !(angle >= (45 + 60) - fieldOfListen && angle < (45 + 60) + fieldOfListen) &&
                        !(angle >= (360 - 45 - 60) - fieldOfListen && angle < (360 - 45 - 60) + fieldOfListen))
                        continue;

                    double chanceMod = 1.0;

                    //Chance to detect player decreases after 125 coordinates!
                    if (distanceToPlayer > 125)
                        chanceMod = 1f - (distanceToPlayer - 125.0) / (detectRadius - 125.0);

                    double chanceToUncover = 0.1 + (npc.Level - stealthLevel) * 0.01 * chanceMod;
                    if (chanceToUncover < 0.01) chanceToUncover = 0.01;


                    if (Util.ChanceDouble(chanceToUncover))
                    {
                        if (canSeePlayer)
                        {
                            if (checklos)
                            {
                                player.Out.SendCheckLOS(player, npc, new CheckLOSResponse(player.UncoverLOSHandler));
                            }
                            else
                            {
                                player.Out.SendMessage(npc.GetName(0, true) + " uncovers you!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                player.Stealth(false);
                                break;
                            }
                        }
                        else
                        {
                            npc.TurnTo(player, 10000);
                        }
                    }
                }
            }
        }




        /// <summary>
        /// This handler is called by the unstealth check of mobs
        /// </summary>
        public void UncoverLOSHandler(GameLiving player, ushort response, ushort targetOID)
        {
            GameObject target = CurrentRegion.GetObject(targetOID);

            if (player is GamePlayer == false || (target == null) || (player.IsStealthed == false)) return;

            if ((response & 0x100) == 0x100)
            {
                ((GamePlayer)player).Out.SendMessage(target.GetName(0, true) + " uncovers you!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                ((GamePlayer)player).Stealth(false);
            }
        }
        /// <summary>
        /// This handler is called by the unstealth check of mobs
        /// </summary>
        public void UncoverLOSHandler(GameObject source, bool losOK)
        {

            if (source == null || this.IsStealthed == false) return;

            if (losOK)
            {
                this.Out.SendMessage(source.GetName(0, true) + " uncovers you!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                this.Stealth(false);
            }
        }


        public bool StealhtActive = false;

        /// <summary>
        /// Checks whether this player can detect stealthed enemy
        /// </summary>
        /// <param name="enemy"></param>
        /// <returns>true if enemy can be detected</returns>
        /// <summary>
        /// Checks whether this player can detect stealthed enemy
        /// </summary>
        /// <param name="enemy"></param>
        /// <returns>true if enemy can be detected</returns>
        public virtual bool CanDetect(GamePlayer enemy)
        {
            if (enemy.CurrentRegionID != CurrentRegionID)
                return false;
            if (!IsAlive)
                return false;
            if (enemy.EffectList.GetOfType<VanishEffect>() != null)
                return false;
            if (this.Client.Account.PrivLevel > 1)
                return true;
            if (enemy.Client.Account.PrivLevel > 1)
                return false;

            /*
			 * http://www.critshot.com/forums/showthread.php?threadid=3142
			 * The person doing the looking has a chance to find them based on their level, minus the stealthed person's stealth spec.
			 *
			 * -Normal detection range = (enemy lvl  your stealth spec) * 20 + 125
			 * -Detect Hidden Range = (enemy lvl  your stealth spec) * 50 + 250
			 * -See Hidden range = 2700 - (38 * your stealth spec)
			 */

            int EnemyStealthLevel = enemy.GetModifiedSpecLevel(Specs.Stealth);
            int PlayerStealthLevel = this.GetModifiedSpecLevel(Specs.Stealth);
            if (PlayerStealthLevel > 50)
                PlayerStealthLevel = 50;
            if (EnemyStealthLevel > 50)
                EnemyStealthLevel = 50;
            int levelDiff = this.Level - EnemyStealthLevel;
            if (levelDiff < 0) levelDiff = 0;
            int range2;
            int range = 0;
            int HiddenRange = ServerProperties.Properties.HIDDEN_RANGE;


            bool enemyHasCamouflage = enemy.EffectList.GetOfType<CamouflageEffect>() != null;
            if (enemyHasCamouflage && !HasAbility(Abilities.DetectHidden))
                range2 = ServerProperties.Properties.CAMOUFLAGE_MAX_RANGE;
            //Cammo on
            else if (enemyHasCamouflage && HasAbility(Abilities.DetectHidden))
            {
                range2 = ServerProperties.Properties.CAMOUFLAGE_HIDDEN_RANGE;
            }
            else
                range2 = 1;


            // Mastery of Stealth Bonus
            RAPropertyEnhancer mos = GetAbility<MasteryOfStealthAbility>();

            if (PlayerStealthLevel > 0 || EnemyStealthLevel > 0)
            {
                range += (BaseBuffBonusCategory[(int)eProperty.Skill_Stealth]) + levelDiff * 20 + 125 / range2; // Normal detection range
            }
            if (PlayerStealthLevel < 1 || EnemyStealthLevel < 1)
            {
                range += levelDiff * 1 + 125 / range2; // Normal detection range
            }
            //range += (BaseBuffBonusCategory[(int)eProperty.Skill_Stealth] / range2); 





            //Nightshade Only - Greater chance of remaining hidden while stealthed for 1 minute
            //after executing a killing blow on a realm opponent.
            GameSpellEffect SubtleKills = SpellHandler.FindEffectOnTarget((GameLiving)enemy, "SubtleKills");
            if (SubtleKills != null)
            {

                range -= (int)SubtleKills.Spell.Value;
                if (range < 0) range = 0;
            }

            // Apply Blanket of camouflage effect
            GameSpellEffect iSpymasterEffect1 = SpellHandler.FindEffectOnTarget((GameLiving)enemy, "BlanketOfCamouflage");
            if (iSpymasterEffect1 != null)
            {
                //StealtIncreased = true;
                range -= (int)iSpymasterEffect1.Spell.Value;
                if (range < 0) range = 0;
            }



            // Apply Prescience node effect
            GameSpellEffect iConvokerEffect = SpellHandler.FindEffectOnTarget((GameLiving)enemy, "Prescience");
            if (iConvokerEffect != null && (!enemyHasCamouflage && enemy.IsMoving))
            {
                StealhtActive = true;
                range += (int)iConvokerEffect.Spell.Value;
            }

            else if (HasAbility(Abilities.DetectHidden) && enemy.HasAbility(Abilities.DetectHidden) && !enemyHasCamouflage)
            {
                //schleicher vs schleicher mit detect hidden
                // we have detect hidden and enemy have = higher range
                range = (levelDiff * levelDiff) * 50 + 125 - HiddenRange / range2; // Detect Hidden advantage
            }

            else if (HasAbility(Abilities.DetectHidden) && !enemy.HasAbility(Abilities.DetectHidden) && !enemyHasCamouflage)
            {
                // we have detect hidden and enemy don't = higher range
                range = 50 + 250 / range2; // Detect Hidden advantage
            }
            else
            {

                if (StealhtActive == true || PlayerStealthLevel > 0 && EnemyStealthLevel > 0)
                {
                    range = 20 + 125 / range2; // Normal detection range
                    StealhtActive = false;
                }
                else
                {
                    range = 50; // Normal detection range
                }
            }

            if (mos != null) //|| (enemyHasCamouflage && !IsMoving))

                if (!HasAbility(Abilities.DetectHidden) || !enemy.HasAbility(Abilities.DetectHidden))

                    range += mos.GetAmountForLevel(CalculateSkillLevel(mos)) / range2;



            GameSpellEffect LookOutEffect = SpellHandler.FindEffectOnTarget((GameLiving)this, "Loockout");
            if (LookOutEffect != null && !enemyHasCamouflage)
            {
                StealhtActive = true;
                range += (int)LookOutEffect.Spell.Value;
            }

            //Buff (Stealth Detection)
            //Increases the target's ability to detect stealthed players and monsters.
            GameSpellEffect iVampiirEffect = SpellHandler.FindEffectOnTarget((GameLiving)this, "VampiirStealthDetection");
            if (iVampiirEffect != null && !enemyHasCamouflage)// && enemy.IsMoving)
            {
                StealhtActive = true;
                range += (int)iVampiirEffect.Spell.Value;
            }


            // Apply Lookout effect
            //Infill Only - Greater Chance to Detect Stealthed Enemies for 1 minute
            //after executing a klling blow on a realm opponent.                                     
            GameSpellEffect HeightenedAwareness = SpellHandler.FindEffectOnTarget((GameLiving)this, "HeightenedAwareness");
            if (HeightenedAwareness != null)
            {
                StealhtActive = true;
                range += (int)HeightenedAwareness.Spell.Value;
            }


            //Hard cap is 1900
            if (range > 1900)
                range = 1900;
            //everyone can see your own group stealthed
            else if (enemy.Group != null && Group != null && enemy.Group == Group)
            {
                range = 2500;
            }

            // Fin
            // vampiir stealth range, uncomment when add eproperty stealthrange i suppose
            return this.IsWithinRadius(enemy, range);
        }


        #endregion

        #region BattleGrounds

        
        #endregion

        #region Task

        /// <summary>
        /// Holding tasks of player
        /// </summary>
        AbstractTask m_questTask = null;

        /// <summary>
        /// Gets the tasklist of this player
        /// </summary>
        public AbstractTask QuestTask
        {
            get { return m_questTask; }
            set { m_questTask = value; }
        }

        #endregion

        #region Mission

        private AbstractMission m_mission = null;

        /// <summary>
        /// Gets the personal mission
        /// </summary>
        public AbstractMission Mission
        {
            get { return m_mission; }
            set
            {
                m_mission = value;
                this.Out.SendQuestListUpdate();
                if (value != null) Out.SendMessage(m_mission.Description, eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
        }

        #endregion

        #region Quest

        /// <summary>
        /// Get the player ID used for quests.  Usually InternalID, provided for customization
        /// </summary>
        public virtual string QuestPlayerID
        {
            get { return InternalID; }
        }

        public virtual bool GetSpecificDataquestByID(int DQID)
        {
            // Data driven quests for this player

            // Data driven quests for this player
            var dataQuests = GameServer.Database.SelectObjects<CharacterXDataQuest>("`Character_ID` = @Character_ID", new QueryParameter("@Character_ID", QuestPlayerID));
            foreach (CharacterXDataQuest quest in dataQuests)
            {
                DBDataQuest dbDataQuest = GameServer.Database.FindObjectByKey<DBDataQuest>(quest.DataQuestID);
                if (dbDataQuest != null && dbDataQuest.StartType != (byte)DataQuest.eStartType.Collection)
                {
                    //DataQuest dataQuest = new DataQuest(this, dbDataQuest, quest);

                    /*
                    if (quest.DataQuestID == DQID)
                    {
                        log.ErrorFormat("Qest step {0}", quest.Step);// must be 2 for the Quest Out of Nowhere
                    }
                    */
                    if (quest.DataQuestID == DQID && quest.Step == 2)
                    {
                        return true;
                    }
                    
                }
               
            }
            return false;
        }
        /// <summary>
        /// Load all the ongoing or completed quests for this player
        /// </summary>
        public virtual void LoadQuests()
        {
            m_questList.Clear();
            m_questListFinished.Clear();

            // Scripted quests
            var quests = GameServer.Database.SelectObjects<DBQuest>("`Character_ID` = @Character_ID", new QueryParameter("@Character_ID", QuestPlayerID));
            foreach (DBQuest dbquest in quests)
            {
                AbstractQuest quest = AbstractQuest.LoadFromDatabase(this, dbquest);
                if (quest != null)
                {
                    if (quest.Step == -1)
                        m_questListFinished.Add(quest);
                    else
                        m_questList.Add(quest);
                }
            }

            // Data driven quests for this player
            var dataQuests = GameServer.Database.SelectObjects<CharacterXDataQuest>("`Character_ID` = @Character_ID", new QueryParameter("@Character_ID", QuestPlayerID));
            foreach (CharacterXDataQuest quest in dataQuests)
            {
                DBDataQuest dbDataQuest = GameServer.Database.FindObjectByKey<DBDataQuest>(quest.DataQuestID);
                if (dbDataQuest != null && dbDataQuest.StartType != (byte)DataQuest.eStartType.Collection)
                {
                    DataQuest dataQuest = new DataQuest(this, dbDataQuest, quest);

                    if (quest.Step > 0)
                    {
                        m_questList.Add((AbstractQuest)dataQuest);
                    }
                    else if (quest.Count > 0)
                    {
                        m_questListFinished.Add((AbstractQuest)dataQuest);
                    }
                }
            }
        }

        /// <summary>
        /// Holds all the quests currently active on this player
        /// </summary>
        protected List<AbstractQuest> m_questList = new List<AbstractQuest>();

        /// <summary>
        /// Holds all already finished quests off this player
        /// </summary>
        protected List<AbstractQuest> m_questListFinished = new List<AbstractQuest>();

        protected RegionTimer m_questActionTimer = null;

        public RegionTimer QuestActionTimer
        {
            get { return m_questActionTimer; }
            set { m_questActionTimer = value; }
        }

        /// <summary>
        /// Gets the questlist of this player
        /// </summary>
        public List<AbstractQuest> QuestList
        {
            get { return m_questList; }
        }

        /// <summary>
        /// Gets the finished quests of this player
        /// </summary>
        public List<AbstractQuest> QuestListFinished
        {
            get { return m_questListFinished; }
        }

        /// <summary>
        /// Add a quest to the players finished list
        /// </summary>
        /// <param name="quest"></param>
        public void AddFinishedQuest(AbstractQuest quest)
        {
            lock (((ICollection)QuestListFinished).SyncRoot)
            {
                if (m_questListFinished.Contains(quest) == false)
                {
                    m_questListFinished.Add(quest);
                }
            }
        }

        /// <summary>
        /// Adds a quest to the players questlist
        /// Can be used by both scripted quests and data quests
        /// </summary>
        /// <param name="quest">The quest to add</param>
        /// <returns>true if added, false if player is already doing the quest!</returns>
        public bool AddQuest(AbstractQuest quest)
        {
            lock (((ICollection)m_questList).SyncRoot)
            {
                if (IsDoingQuest(quest) != null)
                    return false;

                if (m_questList.Contains(quest) == false)
                {
                    m_questList.Add(quest);
                    quest.OnQuestAssigned(this);
                }
            }
            Out.SendQuestUpdate(quest);
            return true;
        }

        /// <summary>
        /// Remove credit for this type of encounter.
        /// Used for scripted quests
        /// </summary>
        /// <param name="questType"></param>
        /// <returns></returns>
        public bool RemoveEncounterCredit(Type questType)
        {
            if (questType == null)
                return false;

            lock (((ICollection)QuestListFinished).SyncRoot)
            {
                foreach (AbstractQuest q in m_questListFinished)
                {
                    if (q is DataQuest == false)
                    {
                        if (q.GetType().Equals(questType) && q.Step == -1)
                        {
                            if (m_questListFinished.Contains(q))
                            {
                                m_questListFinished.Remove(q);
                                q.DeleteFromDatabase();
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Checks if a player has done a specific quest type
        /// This is used for scripted quests
        /// </summary>
        /// <param name="questType">The quest type</param>
        /// <returns>the number of times the player did this quest</returns>
        public int HasFinishedQuest(Type questType)
        {
            int counter = 0;
            lock (((ICollection)QuestListFinished).SyncRoot)
            {
                foreach (AbstractQuest q in m_questListFinished)
                {
                    if (q is DataQuest == false)
                    {
                        if (q.GetType().Equals(questType))
                            counter++;
                    }
                }
            }
            return counter;
        }

        /// <summary>
        /// Checks if this player is currently doing the specified quest
        /// Can be used by scripted and data quests
        /// </summary>
        /// <param name="questType">The quest type</param>
        /// <returns>the quest if player is doing the quest or null if not</returns>
        public AbstractQuest IsDoingQuest(AbstractQuest quest)
        {
            lock (((ICollection)m_questList).SyncRoot)
            {
                foreach (AbstractQuest q in m_questList)
                {
                    if (q.GetType().Equals(quest.GetType()) && q.IsDoingQuest(quest))
                        return q;
                }
            }
            return null;
        }


        /// <summary>
        /// Checks if this player is currently doing the specified quest type
        /// This is used for scripted quests
        /// </summary>
        /// <param name="questType">The quest type</param>
        /// <returns>the quest if player is doing the quest or null if not</returns>
        public AbstractQuest IsDoingQuest(Type questType)
        {

            lock (((ICollection)QuestList).SyncRoot)
            {
                foreach (AbstractQuest q in m_questList)
                {
                    if (q is DataQuest == false)
                    {
                        if (q.GetType().Equals(questType))
                            return q;
                    }
                }
            }
            return null;
        }

        #endregion

        #region Notify
        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            CharacterClass.Notify(e, sender, args);
            base.Notify(e, sender, args);

            // events will only fire for currently active quests.
            lock (((ICollection)m_questList).SyncRoot)
            {
                foreach (AbstractQuest q in new List<AbstractQuest>(m_questList))
                {
                    // player forwards every single notify message to all active quests
                    q.Notify(e, sender, args);
                }
            }

            if (QuestTask != null)
                QuestTask.Notify(e, sender, args);

            if (Mission != null)
                Mission.Notify(e, sender, args);

            if (Group != null && Group.Mission != null)
                Group.Mission.Notify(e, sender, args);

            //Realm mission will be handled on the capture event args
        }

        public override void Notify(DOLEvent e, object sender)
        {
            Notify(e, sender, null);
        }

        public override void Notify(DOLEvent e)
        {
            Notify(e, null, null);
        }

        public override void Notify(DOLEvent e, EventArgs args)
        {
            Notify(e, null, args);
        }
        #endregion

        #region Crafting

        public Object CraftingLock = new Object();

        /// <summary>
        /// Store all player crafting skill and their value (eCraftingSkill => Value)
        /// </summary>
        protected Dictionary<eCraftingSkill, int> m_craftingSkills = new Dictionary<eCraftingSkill, int>();

        /// <summary>
        /// Store the player primary crafting skill
        /// </summary>
        protected eCraftingSkill m_craftingPrimarySkill = 0;

        /// <summary>
        /// Get all player crafting skill and their value
        /// </summary>
        public Dictionary<eCraftingSkill, int> CraftingSkills
        {
            get { return m_craftingSkills; }
        }

        /// <summary>
        /// Store the player primary crafting skill
        /// </summary>
        public eCraftingSkill CraftingPrimarySkill
        {
            get { return m_craftingPrimarySkill; }
            set { m_craftingPrimarySkill = value; }
        }

        /// <summary>
        /// Get the specified player crafting skill value
        /// </summary>
        /// <param name="skill">The crafting skill to get value</param>
        /// <returns>the level in the specified crafting if valid and -1 if not</returns>
        public virtual int GetCraftingSkillValue(eCraftingSkill skill)
        {
            lock (((ICollection)m_craftingSkills).SyncRoot)
            {
                if (!m_craftingSkills.ContainsKey(skill)) return -1;
                return m_craftingSkills[skill];
            }
        }

        /// <summary>
        /// Increase the specified player crafting skill
        /// </summary>
        /// <param name="skill">Crafting skill to increase</param>
        /// <param name="count">How much increase or decrase</param>
        /// <returns>true if the skill is valid and -1 if not</returns>
        public virtual bool GainCraftingSkill(eCraftingSkill skill, int count)
        {
            if (skill == eCraftingSkill.NoCrafting) return false;

            lock (((ICollection)m_craftingSkills).SyncRoot)
            {
                AbstractCraftingSkill craftingSkill = CraftingMgr.getSkillbyEnum(skill);
                if (craftingSkill != null && count > 0)
                {
                    m_craftingSkills[skill] = count + m_craftingSkills[skill];
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainCraftingSkill.GainSkill", craftingSkill.Name, m_craftingSkills[skill]), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    int currentSkillLevel = GetCraftingSkillValue(skill);
                    if (HasPlayerReachedNewCraftingTitle(currentSkillLevel))
                    {
                        GameEventMgr.Notify(GamePlayerEvent.NextCraftingTierReached, this, new NextCraftingTierReachedEventArgs(skill, currentSkillLevel));
                    }
                    if (CanGenerateNews && currentSkillLevel >= 1000 && currentSkillLevel - count < 1000)
                    {
                        string message = string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.GainCraftingSkill.ReachedSkill", Name, craftingSkill.Name));
                        NewsMgr.CreateNews(message, Realm, eNewsType.PvE, true);
                    }
                }
                return true;
            }
        }


        protected bool m_isEligibleToGiveMeritPoints = true;

        /// <summary>
        /// Can actions done by this player reward merit points to the players guild?
        /// </summary>
        public virtual bool IsEligibleToGiveMeritPoints
        {
            get
            {
                if (Guild == null || Client.Account.PrivLevel > 1 || m_isEligibleToGiveMeritPoints == false)
                    return false;

                return true;
            }

            set { m_isEligibleToGiveMeritPoints = value; }
        }

        protected bool m_canGenerateNews = true;

        /// <summary>
        /// Can this player generate news items?
        /// </summary>
        public virtual bool CanGenerateNews
        {
            get { return m_canGenerateNews && GameServer.ServerRules.CanGenerateNews(this); }
            set { m_canGenerateNews = value; }
        }

        /// <summary>
        /// Get the crafting speed multiplier for this player
        /// This might be modified by region or equipment
        /// </summary>
        public virtual double CraftingSpeed
        {
            get
            {
                


                double speed = Properties.CRAFTING_SPEED;
                double speedbonus;
                GameSpellEffect bonusEffect = SpellHandler.FindEffectOnTarget(this, "CraftingSpeedBonus");

                if (bonusEffect != null)
                {
                    speedbonus = speed / 100 * bonusEffect.Spell.Value;
                    speed += speedbonus;
                }


                if (speed <= 0)
                    speed = 1.0;

                if (Guild != null && Guild.BonusType == Guild.eBonusType.CraftingHaste)
                {
                    speed *= (1.0 + Properties.GUILD_BUFF_CRAFTING * .01);
                }

                if (CurrentRegion.IsCapitalCity || CurrentRegion.IsHousing && Properties.CAPITAL_CITY_CRAFTING_SPEED_BONUS > 0)
                {
                    return speed * Properties.CAPITAL_CITY_CRAFTING_SPEED_BONUS;
                }

                return speed;
            }
        }

        protected int craftskillbonus = 0;

        /// <summary>
        /// Get the crafting skill bonus for this player.
        /// This might be modified by region or equipment
        /// Values represents a percent; 0 - 100
        /// </summary>
        public virtual int CraftingSkillBonus
        {
            get
            {
                
                GameSpellEffect skillbonusEffect = SpellHandler.FindEffectOnTarget(this, "CraftingSkillBonus");
                                   
                
                if (skillbonusEffect != null)
                {
                    craftskillbonus = (int)skillbonusEffect.Spell.Value;
                   
                }


                if (CurrentRegion.IsCapitalCity || CurrentRegion.IsHousing)
                {
                    craftskillbonus += Properties.CAPITAL_CITY_CRAFTING_SKILL_GAIN_BONUS;
                    //return Properties.CAPITAL_CITY_CRAFTING_SKILL_GAIN_BONUS;
                }

                return craftskillbonus;

               
            }
            
        }

        protected virtual bool HasPlayerReachedNewCraftingTitle(int skillLevel)
        {
            // no titles after 1000 any more, checked in 1.97
            if (skillLevel <= 1000)
            {
                if (skillLevel % 100 == 0)
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Add a new crafting skill to the player
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="startValue"></param>
        /// <returns></returns>
        public virtual bool AddCraftingSkill(eCraftingSkill skill, int startValue)
        {
            if (skill == eCraftingSkill.NoCrafting) return false;

            if (CraftingPrimarySkill == eCraftingSkill.NoCrafting)
                CraftingPrimarySkill = eCraftingSkill.BasicCrafting;

            lock (((ICollection)m_craftingSkills).SyncRoot)
            {
                if (m_craftingSkills.ContainsKey(skill))
                {
                    AbstractCraftingSkill craftingSkill = CraftingMgr.getSkillbyEnum(skill);
                    if (craftingSkill != null && !m_craftingSkills.ContainsKey(skill))
                    {
                        m_craftingSkills[skill] = startValue;
                        Out.SendMessage("You gain skill in " + craftingSkill.Name + "! (" + startValue.ToString() + ").", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// This is the timer used to count time when a player craft
        /// </summary>
        private RegionTimer m_crafttimer;

        /// <summary>
        /// Get and set the craft timer
        /// </summary>
        public RegionTimer CraftTimer
        {
            get { return m_crafttimer; }
            set { m_crafttimer = value; }
        }



        /// <summary>
        /// This is the timer used to count time when a player craft
        /// </summary>
        private RegionTimer m_capturetimer;

        /// <summary>
        /// Get and set the Capure Timer
        /// </summary>
        public RegionTimer CaptureTimer
        {
            get { return m_capturetimer; }
            set { m_capturetimer = value; }
        }

        /// <summary>
        /// Does the player is crafting
        /// </summary>
        public bool IsCrafting
        {
            get { return (m_crafttimer != null && m_crafttimer.IsAlive); }
        }

        /// <summary>
        /// Get the craft title string of the player
        /// </summary>
        public virtual string CraftTitle
        {
            get
            {
                if (CraftingPrimarySkill == eCraftingSkill.NoCrafting || !m_craftingSkills.ContainsKey(CraftingPrimarySkill))
                {
                    return String.Empty;
                }

                AbstractCraftingSkill craftingSkill = CraftingMgr.getSkillbyEnum(CraftingPrimarySkill);

                return (craftingSkill is AbstractProfession)
                    ? (craftingSkill as AbstractProfession).GetTitle((int)m_craftingSkills[CraftingPrimarySkill], this)
                    : String.Empty;
            }
        }

        /// <summary>
        /// This function saves all player crafting skill in the db
        /// </summary>
        protected void SaveCraftingSkills()
        {
            DBCharacter.CraftingPrimarySkill = (byte)CraftingPrimarySkill;

            string cs = String.Empty;

            if (CraftingPrimarySkill != eCraftingSkill.NoCrafting)
            {
                lock (((ICollection)m_craftingSkills).SyncRoot)
                {
                    foreach (KeyValuePair<eCraftingSkill, int> de in m_craftingSkills)
                    {
                        if (cs.Length > 0) cs += ";";

                        cs += Convert.ToInt32(de.Key) + "|" + Convert.ToInt32(de.Value);
                    }
                }
            }

            DBCharacter.SerializedCraftingSkills = cs;
        }

        /// <summary>
        /// This function load all player crafting skill from the db
        /// </summary>
        protected void LoadCraftingSkills()
        {
            if (DBCharacter.SerializedCraftingSkills == "" || DBCharacter.CraftingPrimarySkill == 0)
            {
                AddCraftingSkill(eCraftingSkill.BasicCrafting, 1);
                SaveCraftingSkills();
                Out.SendUpdateCraftingSkills();
                return;
            }
            try
            {
                CraftingPrimarySkill = (eCraftingSkill)DBCharacter.CraftingPrimarySkill;

                lock (((ICollection)m_craftingSkills).SyncRoot)
                {
                    foreach (string skill in DBCharacter.SerializedCraftingSkills.SplitCSV())
                    {
                        string[] values = skill.Split('|');
                        //Load by crafting skill name
                        if (values[0].Length > 3)
                        {
                            int i = 0;
                            switch (values[0])
                            {
                                case "WeaponCrafting": i = 1; break;
                                case "ArmorCrafting": i = 2; break;
                                case "SiegeCrafting": i = 3; break;
                                case "Alchemy": i = 4; break;
                                case "MetalWorking": i = 6; break;
                                case "LeatherCrafting": i = 7; break;
                                case "ClothWorking": i = 8; break;
                                case "GemCutting": i = 9; break;
                                case "HerbalCrafting": i = 10; break;
                                case "Tailoring": i = 11; break;
                                case "Fletching": i = 12; break;
                                case "SpellCrafting": i = 13; break;
                                case "WoodWorking": i = 14; break;
                                case "BasicCrafting": i = 15; break;

                            }
                            if (!m_craftingSkills.ContainsKey((eCraftingSkill)i))
                            {
                                if (IsCraftingSkillDefined(Convert.ToInt32(values[0])))
                                {
                                    m_craftingSkills.Add((eCraftingSkill)i, Convert.ToInt32(values[1]));
                                }
                                else
                                {
                                    log.Error("Tried to load invalid CraftingSkill :" + values[0]);
                                }
                            }

                        }
                        //Load by number
                        else if (!m_craftingSkills.ContainsKey((eCraftingSkill)Convert.ToInt32(values[0])))
                        {
                            if (IsCraftingSkillDefined(Convert.ToInt32(values[0])))
                            {
                                m_craftingSkills.Add((eCraftingSkill)Convert.ToInt32(values[0]), Convert.ToInt32(values[1]));
                            }
                            else
                            {
                                log.Error("Tried to load invalid CraftingSkill :" + values[0]);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (log.IsErrorEnabled)
                    log.Error(Name + ": error in loading playerCraftingSkills => " + DBCharacter.SerializedCraftingSkills, e);
            }
        }

        private bool IsCraftingSkillDefined(int craftingSkillToCheck)
        {
            return Enum.IsDefined(typeof(eCraftingSkill), craftingSkillToCheck);
        }

        /// <summary>
        /// This function is called each time a player tries to make a item
        /// </summary>
        public virtual void CraftItem(ushort itemID)
        {
            DBCraftedItem recipe = GameServer.Database.FindObjectByKey<DBCraftedItem>(itemID.ToString());
            if (recipe != null)
            {
                ItemTemplate itemToCraft;
                itemToCraft = GameServer.Database.FindObjectByKey<ItemTemplate>(recipe.Id_nb);
                IList<DBCraftedXItem> rawMaterials = GameServer.Database.SelectObjects<DBCraftedXItem>("`CraftedItemId_nb` = @CraftedItemId_nb", new QueryParameter("@CraftedItemId_nb", recipe.Id_nb));
                if (rawMaterials.Count > 0)
                {
                    if (itemToCraft != null)
                    {
                        AbstractCraftingSkill skill = CraftingMgr.getSkillbyEnum((eCraftingSkill)recipe.CraftingSkillType);
                        if (skill != null)
                        {
                            skill.CraftItem(this, recipe, itemToCraft, rawMaterials);
                        }
                        else
                        {
                            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CraftItem.DontHaveAbilityMake"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                    }
                    else
                    {
                        Out.SendMessage("Crafted ItemTemplate (" + recipe.Id_nb.ToString() + ") not implemented yet.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                }
                else
                {
                    Out.SendMessage("Craft recipe for (" + recipe.Id_nb.ToString() + ") is missing raw materials!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
            }
            else
            {
                Out.SendMessage("CraftedItemID: (" + itemID.ToString() + ") not implemented yet.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
        }

        /// <summary>
        /// This function is called each time a player try to salvage a item
        /// </summary>
        public virtual void SalvageItem(InventoryItem item)
        {
           Salvage.BeginWork(this, item);
        }

        /// <summary>
        /// This function is called each time a player try to repair a item
        /// </summary>
        public virtual void RepairItem(InventoryItem item)
        {
            Repair.BeginWork(this, item);
        }

        #endregion

        #region Housing

        /*
		/// <summary>
		/// Holds the houses that need a update
		/// </summary>
		private BitArray m_housingUpdateArray;

		/// <summary>
		/// Returns the Housing Update Array
		/// </summary>
		public BitArray HousingUpdateArray
		{
			get { return m_housingUpdateArray; }
			set { m_housingUpdateArray = value; }
		}
        */
        /// <summary>
        /// Jumps the player out of the house he is in
        /// </summary>
        public void LeaveHouse()
        {
            if (CurrentHouse == null || (!InHouse))
            {
                InHouse = false;
                return;
            }

            House house = CurrentHouse;
            InHouse = false;
            CurrentHouse = null;

            house.Exit(this, false);
        }


        #endregion

        #region Trade

        /// <summary>
        /// Holds the trade window object
        /// </summary>
        protected ITradeWindow m_tradeWindow;

        /// <summary>
        /// Gets or sets the player trade windows
        /// </summary>
        public ITradeWindow TradeWindow
        {
            get { return m_tradeWindow; }
            set { m_tradeWindow = value; }
        }

        /// <summary>
        /// Opens the trade between two players
        /// </summary>
        /// <param name="tradePartner">GamePlayer to trade with</param>
        /// <returns>true if trade has started</returns>
        public bool OpenTrade(GamePlayer tradePartner)
        {
            lock (m_LockObject)
            {
                lock (tradePartner)
                {
                    if (tradePartner.TradeWindow != null)
                        return false;

                    object sync = new object();
                    PlayerTradeWindow initiantWindow = new PlayerTradeWindow(tradePartner, false, sync);
                    PlayerTradeWindow recipientWindow = new PlayerTradeWindow(this, true, sync);

                    tradePartner.TradeWindow = initiantWindow;
                    TradeWindow = recipientWindow;

                    initiantWindow.PartnerWindow = recipientWindow;
                    recipientWindow.PartnerWindow = initiantWindow;

                    return true;
                }
            }
        }

        /// <summary>
        /// Opens the trade window for myself (combine, repair)
        /// </summary>
        /// <param name="item">The item to spell craft</param>
        /// <returns>true if trade has started</returns>
        public bool OpenSelfCraft(InventoryItem item)
        {
            if (item == null) return false;

            lock (m_LockObject)
            {
                if (TradeWindow != null)
                {
                    GamePlayer sourceTradePartner = TradeWindow.Partner;
                    if (sourceTradePartner == null)
                    {
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OpenSelfCraft.AlreadySelfCrafting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                    else
                    {
                        Out.SendMessage("You are still trading with " + sourceTradePartner.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                    return false;
                }

                if (item.SlotPosition < (int)eInventorySlot.FirstBackpack || item.SlotPosition > (int)eInventorySlot.LastBackpack)
                {
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.OpenSelfCraft.CanOnlyCraftBackpack"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return false;
                }

                TradeWindow = new SelfCraftWindow(this, item);
                TradeWindow.TradeUpdate();

                return true;
            }
        }

        #endregion

        #region ControlledNpc

        /// <summary>
        /// Sets the controlled object for this player
        /// </summary>
        /// <param name="controlledNpc"></param>
        public override void SetControlledBrain(IControlledBrain controlledBrain)
        {
            CharacterClass.SetControlledBrain(controlledBrain);
        }

        /// <summary>
        /// Commands controlled object to attack
        /// </summary>
        public virtual void CommandNpcAttack()
        {
            IControlledBrain npc = ControlledBrain;
            if (npc == null || npc.Body.IsConfused || !GameServer.ServerRules.IsAllowedToAttack(this, TargetObject as GameLiving, false))
                return;

            if (npc.Body.IsConfused)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CommandNpcAttack.IsConfused", npc.Body.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (!IsWithinRadius(TargetObject, 2000))
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CommandNpcAttack.TooFarAwayForPet"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (!TargetInView)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Attack.CantSeeTarget"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return;
            }

            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CommandNpcAttack.KillTarget", npc.Body.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            npc.Attack(TargetObject);
        }

        /// <summary>
        /// Releases controlled object
        /// </summary>
        public virtual void CommandNpcRelease()
        {
            CharacterClass.CommandNpcRelease();
            this.JuwelinePetCount = 0;
        }

        /// <summary>
        /// Commands controlled object to follow
        /// </summary>
        public virtual void CommandNpcFollow()
        {
            IControlledBrain npc = ControlledBrain;
            if (npc == null)
                return;

            if (npc.Body.IsConfused)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CommandNpcAttack.IsConfused", npc.Body.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CommandNpcAttack.FollowYou", npc.Body.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            npc.Follow(this);
        }

        /// <summary>
        /// Commands controlled object to stay where it is
        /// </summary>
        public virtual void CommandNpcStay()
        {
            IControlledBrain npc = ControlledBrain;
            if (npc == null)
                return;

            if (npc.Body.IsConfused)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CommandNpcAttack.IsConfused", npc.Body.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CommandNpcAttack.Stay", npc.Body.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            npc.Stay();
        }

        /// <summary>
        /// Commands controlled object to go to players location
        /// </summary>
        public virtual void CommandNpcComeHere()
        {
            IControlledBrain npc = ControlledBrain;
            if (npc == null)
                return;

            if (npc.Body.IsConfused)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CommandNpcAttack.IsConfused", npc.Body.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CommandNpcAttack.ComeHere", npc.Body.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            npc.ComeHere();
        }

        /// <summary>
        /// Commands controlled object to go to target
        /// </summary>
        public virtual void CommandNpcGoTarget()
        {
            IControlledBrain npc = ControlledBrain;
            if (npc == null)
                return;

            if (npc.Body.IsConfused)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CommandNpcAttack.IsConfused", npc.Body.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            GameObject target = TargetObject;
            if (target == null)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CommandNpcGoTarget.MustSelectDestination"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CommandNpcAttack.GoToTarget", npc.Body.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            npc.Goto(target);
        }

        /// <summary>
        /// Changes controlled object state to passive
        /// </summary>
        public virtual void CommandNpcPassive()
        {
            IControlledBrain npc = ControlledBrain;
            if (npc == null)
                return;

            if (npc.Body.IsConfused)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CommandNpcAttack.IsConfused", npc.Body.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CommandNpcAttack.Passive", npc.Body.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            npc.SetAggressionState(eAggressionState.Passive);
            npc.Body.StopAttack();
            npc.Body.StopCurrentSpellcast();
            npc.Follow(this);
        }

        /// <summary>
        /// Changes controlled object state to aggressive
        /// </summary>
        public virtual void CommandNpcAgressive()
        {
            IControlledBrain npc = ControlledBrain;
            if (npc == null)
                return;

            if (npc.Body.IsConfused)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CommandNpcAttack.IsConfused", npc.Body.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CommandNpcAttack.Aggressive", npc.Body.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            npc.SetAggressionState(eAggressionState.Aggressive);
        }

        /// <summary>
        /// Changes controlled object state to defensive
        /// </summary>
        public virtual void CommandNpcDefensive()
        {
            IControlledBrain npc = ControlledBrain;
            if (npc == null)
                return;

            if (npc.Body.IsConfused)
            {
                Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CommandNpcAttack.IsConfused", npc.Body.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CommandNpcAttack.Denfensive", npc.Body.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            npc.SetAggressionState(eAggressionState.Defensive);
        }
        #endregion

        #region Labyrinth Obelisks
        public virtual string LabyrinthObelisk
        {
            get { return DBCharacter?.LabyrinthObelisk; }
            set { if (DBCharacter != null) DBCharacter.LabyrinthObelisk = value; }
        }
        /// <summary>
        /// Checks if a player has activated an obelisk
        /// </summary>
        public virtual bool HaveLabyrinthObelisk(string obelisk)
        {
            if (LabyrinthObelisk != null && LabyrinthObelisk.Length > 0)
            {
                foreach (string dObelisk in LabyrinthObelisk.Split(';'))
                {
                    if (dObelisk.Length < 2) continue;
                    if (dObelisk == obelisk) return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Loads obelisk list
        /// </summary>
        public virtual void LoadLabyrinthObelisk(string allobelisk, string obelisklist)
        {
            GamePlayer player = this as GamePlayer;
            if (allobelisk != null && allobelisk.Length > 0)
            {
                foreach (string dAllobelisk in allobelisk.Split(';'))
                {
                    if (dAllobelisk.Length < 2) continue;
                    if (player.HaveLabyrinthObelisk(dAllobelisk)) obelisklist += "\n[" + dAllobelisk + "]";
                    else obelisklist += "\n " + dAllobelisk;
                }
            }
            //player.ChatClear();
            player.Out.SendMessage(obelisklist, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
        }
        /// <summary>
        /// Clears the Popup window
        /// </summary>
        
        #endregion

        #region Shade

        protected ShadeEffect m_ShadeEffect = null;

        /// <summary>
        /// The shade effect of this player
        /// </summary>
        public ShadeEffect ShadeEffect
        {
            get { return m_ShadeEffect; }
            set { m_ShadeEffect = value; }
        }

        /// <summary>
        /// Gets flag indication whether player is in shade mode
        /// </summary>
        public bool IsShade
        {
            get
            {
                bool shadeModel = Model == ShadeModel;
                return m_ShadeEffect != null ? true : shadeModel;
            }
        }

        /// <summary>
        /// Create a shade effect for this player.
        /// </summary>
        /// <returns></returns>
        protected virtual ShadeEffect CreateShadeEffect()
        {
            return CharacterClass.CreateShadeEffect();
        }

        /// <summary>
        /// The model ID used on character creation.
        /// </summary>
        public ushort CreationModel
        {
            get
            {
                return (ushort)m_client.Account.Characters[m_client.ActiveCharIndex].CreationModel;
            }
        }

        /// <summary>
        /// The model ID used for shade morphs.
        /// </summary>
        public ushort ShadeModel
        {
            get
            {
                // Aredhel: Bit fishy, necro in caster from could use
                // Traitor's Dagger... FIXME!

                if (IsCharcterClass(eCharacterClass.Necromancer))
                    return 822;

                switch (Race)
                {
                    // Albion Models.
                    case (int)eRace.Inconnu: return (ushort)(DBCharacter.Gender + 1351);
                    case (int)eRace.Briton: return (ushort)(DBCharacter.Gender + 1353);
                    case (int)eRace.Avalonian: return (ushort)(DBCharacter.Gender + 1359);
                    case (int)eRace.Highlander: return (ushort)(DBCharacter.Gender + 1355);
                    case (int)eRace.Saracen: return (ushort)(DBCharacter.Gender + 1357);
                    case (int)eRace.HalfOgre: return (ushort)(DBCharacter.Gender + 1361);
                    case (int)eRace.AlbionMinotaur: return (ushort)(DBCharacter.Gender + 1582);

                    // Midgard Models.
                    case (int)eRace.Troll: return (ushort)(DBCharacter.Gender + 1363);
                    case (int)eRace.Dwarf: return (ushort)(DBCharacter.Gender + 1369);
                    case (int)eRace.Norseman: return (ushort)(DBCharacter.Gender + 1365);
                    case (int)eRace.Kobold: return (ushort)(DBCharacter.Gender + 1367);
                    case (int)eRace.Valkyn: return (ushort)(DBCharacter.Gender + 1371);
                    case (int)eRace.Frostalf: return (ushort)(DBCharacter.Gender + 1373);
                    case (int)eRace.MidgardMinotaur: return (ushort)(DBCharacter.Gender + 1582);

                    // Hibernia Models.
                    case (int)eRace.Firbolg: return (ushort)(DBCharacter.Gender + 1375);
                    case (int)eRace.Celt: return (ushort)(DBCharacter.Gender + 1377);
                    case (int)eRace.Lurikeen: return (ushort)(DBCharacter.Gender + 1379);
                    case (int)eRace.Elf: return (ushort)(DBCharacter.Gender + 1381);
                    case (int)eRace.Sylvan: return (ushort)(DBCharacter.Gender + 1383);
                    case (int)eRace.Shar: return (ushort)(DBCharacter.Gender + 1385);
                    case (int)eRace.HiberniaMinotaur: return (ushort)(DBCharacter.Gender + 1582);

                    default: return Model;
                }
            }
        }

        /// <summary>
        /// Changes shade state of the player.
        /// </summary>
        /// <param name="state">The new state.</param>
        public virtual void Shade(bool state)
        {
            CharacterClass.Shade(state);
        }
        #endregion

        #region Siege Weapon
        private GameSiegeWeapon m_siegeWeapon;

        public GameSiegeWeapon SiegeWeapon
        {
            get { return m_siegeWeapon; }
            set { m_siegeWeapon = value; }
        }
        public void SalvageSiegeWeapon(GameSiegeWeapon siegeWeapon)
        {
            Salvage.BeginWork(this, siegeWeapon);
        }
        #endregion

        #region Invulnerability

        /// <summary>
        /// The delegate for invulnerability expire callbacks
        /// </summary>
        public delegate void InvulnerabilityExpiredCallback(GamePlayer player);
        /// <summary>
        /// Holds the invulnerability timer
        /// </summary>
        protected InvulnerabilityTimer m_invulnerabilityTimer;
        /// <summary>
        /// Holds the invulnerability expiration tick
        /// </summary>
        protected long m_invulnerabilityTick;

        /// <summary>
        /// Starts the Invulnerability Timer
        /// </summary>
        /// <param name="duration">The invulnerability duration in milliseconds</param>
        /// <param name="callback">
        /// The callback for when invulnerability expires;
        /// not guaranteed to be called if overwriten by another invulnerability
        /// </param>
        /// <returns>true if invulnerability was set (smaller than old invulnerability)</returns>
        public virtual bool StartInvulnerabilityTimer(int duration, InvulnerabilityExpiredCallback callback)
        {
            if (duration < 1)
                throw new ArgumentOutOfRangeException("duration", duration, "Immunity duration cannot be less than 1ms");

            long newTick = CurrentRegion.Time + duration;
            if (newTick < m_invulnerabilityTick)
                return false;

            m_invulnerabilityTick = newTick;
            if (m_invulnerabilityTimer != null)
                m_invulnerabilityTimer.Stop();

            if (callback != null)
            {
                m_invulnerabilityTimer = new InvulnerabilityTimer(this, callback);
                m_invulnerabilityTimer.Start(duration);
            }
            else
            {
                m_invulnerabilityTimer = null;
            }

            return true;
        }

        /// <summary>
        /// True if player is invulnerable to any attack
        /// </summary>
        public virtual bool IsInvulnerableToAttack
        {
            get { return m_invulnerabilityTick > CurrentRegion.Time; }
        }

        /// <summary>
        /// The timer to call invulnerability expired callbacks
        /// </summary>
        protected class InvulnerabilityTimer : RegionAction
        {
            /// <summary>
            /// Defines a logger for this class.
            /// </summary>
            private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            /// <summary>
            /// Holds the callback
            /// </summary>
            private readonly InvulnerabilityExpiredCallback m_callback;

            /// <summary>
            /// Constructs a new InvulnerabilityTimer
            /// </summary>
            /// <param name="actionSource"></param>
            /// <param name="callback"></param>
            public InvulnerabilityTimer(GamePlayer actionSource, InvulnerabilityExpiredCallback callback)
                : base(actionSource)
            {
                m_callback = callback ?? throw new ArgumentNullException("callback");
            }

            /// <summary>
            /// Called on every timer tick
            /// </summary>
            protected override void OnTick()
            {
                try
                {
                    m_callback((GamePlayer)m_actionSource);
                }
                catch (Exception e)
                {
                    log.Error("InvulnerabilityTimer callback", e);
                }
            }
        }

        #endregion

        #region Player Titles

        /// <summary>
        /// Holds all players titles.
        /// </summary>
        protected readonly ArrayList m_titles = new ArrayList();

        /// <summary>
        /// Holds current selected title.
        /// </summary>
        protected IPlayerTitle m_currentTitle = PlayerTitleMgr.ClearTitle;

        /// <summary>
        /// Adds the title to player.
        /// </summary>
        /// <param name="title">The title to add.</param>
        /// <returns>true if added.</returns>
        public virtual bool AddTitle(IPlayerTitle title)
        {
            if (m_titles.Contains(title))
                return false;
            m_titles.Add(title);
            title.OnTitleGained(this);
            return true;
        }

        /// <summary>
        /// Removes the title from player.
        /// </summary>
        /// <param name="title">The title to remove.</param>
        /// <returns>true if removed.</returns>
        public virtual bool RemoveTitle(IPlayerTitle title)
        {
            if (!m_titles.Contains(title))
                return false;
            if (CurrentTitle == title)
                CurrentTitle = PlayerTitleMgr.ClearTitle;
            m_titles.Remove(title);
            title.OnTitleLost(this);
            return true;
        }

        /// <summary>
        /// Gets all player's titles.
        /// </summary>
        public virtual IList Titles
        {
            get { return m_titles; }
        }

        /// <summary>
        /// Gets/sets currently selected/active player title.
        /// </summary>
        public virtual IPlayerTitle CurrentTitle
        {
            get { return m_currentTitle; }
            set
            {
                if (value == null)
                    value = PlayerTitleMgr.ClearTitle;
                m_currentTitle = value;
                DBCharacter.CurrentTitleType = value.GetType().FullName;

                //update newTitle for all players if client is playing
                if (ObjectState == eObjectState.Active)
                {
                    if (value == PlayerTitleMgr.ClearTitle)
                        Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.CurrentTitle.TitleCleared"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    else
                        Out.SendMessage("Your title has been set to " + value.GetDescription(this) + '.', eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                UpdateCurrentTitle();
            }
        }

        /// <summary>
        /// Updates player's current title to him and everyone around.
        /// </summary>
        public virtual void UpdateCurrentTitle()
        {
            if (ObjectState == eObjectState.Active)
            {
                foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    if (player == null) continue;
                    if (player != this)
                    {
                        player.Out.SendPlayerTitleUpdate(this);
                    }
                }
                Out.SendUpdatePlayer();
            }
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Gets or sets the count of albion players killed.
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int KillsAlbionPlayers
        {
            get { return DBCharacter != null ? DBCharacter.KillsAlbionPlayers : 0; }
            set
            {
                if (DBCharacter != null) DBCharacter.KillsAlbionPlayers = value;
                Notify(GamePlayerEvent.KillsAlbionPlayersChanged, this);
                Notify(GamePlayerEvent.KillsTotalPlayersChanged, this);
            }
        }

        /// <summary>
        /// Gets or sets the count of midgard players killed.
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int KillsMidgardPlayers
        {
            get { return DBCharacter != null ? DBCharacter.KillsMidgardPlayers : 0; }
            set
            {
                if (DBCharacter != null) DBCharacter.KillsMidgardPlayers = value;
                Notify(GamePlayerEvent.KillsMidgardPlayersChanged, this);
                Notify(GamePlayerEvent.KillsTotalPlayersChanged, this);
            }
        }

        /// <summary>
        /// Gets or sets the count of hibernia players killed.
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int KillsHiberniaPlayers
        {
            get { return DBCharacter != null ? DBCharacter.KillsHiberniaPlayers : 0; }
            set
            {
                if (DBCharacter != null) DBCharacter.KillsHiberniaPlayers = value;
                Notify(GamePlayerEvent.KillsHiberniaPlayersChanged, this);
                Notify(GamePlayerEvent.KillsTotalPlayersChanged, this);
            }
        }

        /// <summary>
        /// Gets or sets the count of death blows on albion players.
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int KillsAlbionDeathBlows
        {
            get { return DBCharacter != null ? DBCharacter.KillsAlbionDeathBlows : 0; }
            set
            {
                if (DBCharacter != null) DBCharacter.KillsAlbionDeathBlows = value;
                Notify(GamePlayerEvent.KillsTotalDeathBlowsChanged, this);
            }
        }

        /// <summary>
        /// Gets or sets the count of death blows on midgard players.
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int KillsMidgardDeathBlows
        {
            get { return DBCharacter != null ? DBCharacter.KillsMidgardDeathBlows : 0; }
            set
            {
                if (DBCharacter != null) DBCharacter.KillsMidgardDeathBlows = value;
                Notify(GamePlayerEvent.KillsTotalDeathBlowsChanged, this);
            }
        }

        /// <summary>
        /// Gets or sets the count of death blows on hibernia players.
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int KillsHiberniaDeathBlows
        {
            get { return DBCharacter != null ? DBCharacter.KillsHiberniaDeathBlows : 0; }
            set
            {
                if (DBCharacter != null) DBCharacter.KillsHiberniaDeathBlows = value;
                Notify(GamePlayerEvent.KillsTotalDeathBlowsChanged, this);
            }
        }

        /// <summary>
        /// Gets or sets the count of killed solo albion players.
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int KillsAlbionSolo
        {
            get { return DBCharacter != null ? DBCharacter.KillsAlbionSolo : 0; }
            set
            {
                if (DBCharacter != null) DBCharacter.KillsAlbionSolo = value;
                Notify(GamePlayerEvent.KillsTotalSoloChanged, this);
            }
        }

        /// <summary>
        /// Gets or sets the count of killed solo midgard players.
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int KillsMidgardSolo
        {
            get { return DBCharacter != null ? DBCharacter.KillsMidgardSolo : 0; }
            set
            {
                if (DBCharacter != null) DBCharacter.KillsMidgardSolo = value;
                Notify(GamePlayerEvent.KillsTotalSoloChanged, this);
            }
        }

        /// <summary>
        /// Gets or sets the count of killed solo hibernia players.
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int KillsHiberniaSolo
        {
            get { return DBCharacter != null ? DBCharacter.KillsHiberniaSolo : 0; }
            set
            {
                if (DBCharacter != null) DBCharacter.KillsHiberniaSolo = value;
                Notify(GamePlayerEvent.KillsTotalSoloChanged, this);
            }
        }

        /// <summary>
        /// Gets or sets the count of captured keeps.
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int CapturedKeeps
        {
            get { return DBCharacter != null ? DBCharacter.CapturedKeeps : 0; }
            set
            {
                if (DBCharacter != null) DBCharacter.CapturedKeeps = value;
                Notify(GamePlayerEvent.CapturedKeepsChanged, this);
            }
        }

        /// <summary>
        /// Gets or sets the count of captured towers.
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int CapturedTowers
        {
            get { return DBCharacter != null ? DBCharacter.CapturedTowers : 0; }
            set
            {
                if (DBCharacter != null) DBCharacter.CapturedTowers = value;
                Notify(GamePlayerEvent.CapturedTowersChanged, this);
            }
        }

        /// <summary>
        /// Gets or sets the count of captured relics.
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int CapturedRelics
        {
            get { return DBCharacter != null ? DBCharacter.CapturedRelics : 0; }
            set
            {
                if (DBCharacter != null) DBCharacter.CapturedRelics = value;
                Notify(GamePlayerEvent.CapturedRelicsChanged, this);
            }
        }

        /// <summary>
        /// Gets or sets the count of dragons killed.
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int KillsDragon
        {
            get { return DBCharacter != null ? DBCharacter.KillsDragon : 0; }
            set
            {
                if (DBCharacter != null) DBCharacter.KillsDragon = value;
                Notify(GamePlayerEvent.KillsDragonChanged, this);
            }
        }

        /// <summary>
        /// Gets or sets the pvp deaths
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int DeathsPvP
        {
            get { return DBCharacter != null ? DBCharacter.DeathsPvP : 0; }
            set { if (DBCharacter != null) DBCharacter.DeathsPvP = value; }
        }

        /// <summary>
        /// Gets or sets the count of killed Legions.
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int KillsLegion
        {
            get { return DBCharacter != null ? DBCharacter.KillsLegion : 0; }
            set
            {
                if (DBCharacter != null) DBCharacter.KillsLegion = value;
                Notify(GamePlayerEvent.KillsLegionChanged, this);
            }
        }

        /// <summary>
        /// Gets or sets the count of killed Epic Boss.
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int KillsEpicBoss
        {
            get { return DBCharacter != null ? DBCharacter.KillsEpicBoss : 0; }
            set
            {
                if (DBCharacter != null) DBCharacter.KillsEpicBoss = value;
                Notify(GamePlayerEvent.KillsEpicBossChanged, this);
            }
        }
        #endregion

        #region Controlled Mount

        protected RegionTimer m_whistleMountTimer;
        protected ControlledHorse m_controlledHorse;

        public bool HasHorse
        {
            get
            {
                if (ActiveHorse == null || ActiveHorse.ID == 0)
                    return false;
                return true;
            }
        }

        public bool IsSummoningMount
        {
            get { return m_whistleMountTimer != null && m_whistleMountTimer.IsAlive; }
        }

        protected bool m_isOnHorse;
        public virtual bool IsOnHorse
        {
            get { return m_isOnHorse; }
            set
            {
                if (m_whistleMountTimer != null)
                    StopWhistleTimers();
                m_isOnHorse = value;
                Out.SendControlledHorse(this, value); // fix very rare bug when this player not in GetPlayersInRadius;
                foreach (GamePlayer plr in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {

                    if (plr == null) continue;
                    if (plr == this)
                        continue;


                    plr.Out.SendControlledHorse(this, value);
                }
                if (m_isOnHorse)



                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.IsOnHorse.MountSteed"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                else
                    Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.IsOnHorse.DismountSteed"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                Out.SendUpdateMaxSpeed();
            }
        }

        protected void StopWhistleTimers()
        {
            if (m_whistleMountTimer != null)
            {


                m_whistleMountTimer.Stop();
                Out.SendCloseTimerWindow();
            }
            m_whistleMountTimer = null;
        }

        protected int WhistleMountTimerCallback(RegionTimer callingTimer)
        {
            StopWhistleTimers();

            //no horse whistle with relic!
            if (GameRelic.IsPlayerCarryingRelic(this))
            {
                return 0;
            }



            SpeedOfSoundEffect effect1 = this.EffectList.GetOfType<SpeedOfSoundEffect>();
            GameSpellEffect effect2 = SpellHandler.FindEffectOnTarget(this, "SpeedEnhancement");
            GameSpellEffect effect3 = SpellHandler.FindEffectOnTarget(this, "SpeedOfTheRealm");
            if (effect1 != null)
            {
                effect1.Cancel(false);
            }
            if (effect2 != null)
            {
                effect2.Cancel(false);
            }
            if (effect3 != null)
            {
                effect3.Cancel(false);
            }

            IsOnHorse = true;
            return 0;
        }

#pragma warning disable IDE1006 // Benennungsstile
        public enum eHorseSaddleBag : byte
#pragma warning restore IDE1006 // Benennungsstile
        {
            None = 0x00,
            LeftFront = 0x01,
            RightFront = 0x02,
            LeftRear = 0x04,
            RightRear = 0x08,
            All = 0x0F
        }

        /// <summary>
        /// What horse saddle bags are active on this player?
        /// </summary>
        public virtual byte ActiveSaddleBags
        {
            get { return DBCharacter.ActiveSaddleBags; }
            set { DBCharacter.ActiveSaddleBags = value; }
        }

        public ControlledHorse ActiveHorse
        {
            get { return m_controlledHorse; }
        }

        public class ControlledHorse
        {
            protected byte m_id;
            protected byte m_bardingId;
            protected ushort m_bardingColor;
            protected byte m_saddleId;
            protected byte m_saddleColor;
            protected byte m_slots;
            protected byte m_armor;
            protected string m_name;
            protected int m_level;
            protected GamePlayer m_player;

            public ControlledHorse(GamePlayer player)
            {
                m_name = String.Empty;
                m_player = player;
            }

            public byte ID
            {
                get { return m_id; }
                set
                {
                    m_id = value;
                    InventoryItem item = m_player.Inventory.GetItem(eInventorySlot.Horse);
                    if (item != null)
                        m_level = item.Level;
                    else
                        m_level = 35;//base horse by default
                    m_player.Out.SendSetControlledHorse(m_player);
                }
            }

            public byte Barding
            {
                get
                {
                    InventoryItem barding = m_player.Inventory.GetItem(eInventorySlot.HorseBarding);
                    if (barding != null)
                        return (byte)barding.DPS_AF;
                    return m_bardingId;
                }
                set
                {
                    m_bardingId = value;
                    m_player.Out.SendSetControlledHorse(m_player);
                }
            }

            public ushort BardingColor
            {
                get
                {
                    InventoryItem barding = m_player.Inventory.GetItem(eInventorySlot.HorseBarding);
                    if (barding != null)
                        return (ushort)barding.Color;
                    return m_bardingColor;
                }
                set
                {
                    m_bardingColor = value;
                    m_player.Out.SendSetControlledHorse(m_player);
                }
            }

            public byte Saddle
            {
                get
                {
                    InventoryItem armor = m_player.Inventory.GetItem(eInventorySlot.HorseArmor);
                    if (armor != null)
                        return (byte)armor.DPS_AF;
                    return m_saddleId;
                }
                set
                {
                    m_saddleId = value;
                    m_player.Out.SendSetControlledHorse(m_player);
                }
            }

            public byte SaddleColor
            {
                get
                {
                    InventoryItem armor = m_player.Inventory.GetItem(eInventorySlot.HorseArmor);
                    if (armor != null)
                        return (byte)armor.Color;
                    return m_saddleColor;
                }
                set
                {
                    m_saddleColor = value;
                    m_player.Out.SendSetControlledHorse(m_player);
                }
            }

            public byte Slots
            {
                get { return m_player.ActiveSaddleBags; }
            }

            public byte Armor
            {
                get { return m_armor; }
            }

            public string Name
            {
                get { return m_name; }
                set
                {
                    m_name = value;
                    InventoryItem item = m_player.Inventory.GetItem(eInventorySlot.Horse);
                    if (item != null)
                        item.Creator = Name;
                    m_player.Out.SendSetControlledHorse(m_player);
                }
            }

            public short Speed
            {
                get
                {
                    if (m_level <= 35)
                        return ServerProperties.Properties.MOUNT_UNDER_LEVEL_35_SPEED;
                    else
                        return ServerProperties.Properties.MOUNT_OVER_LEVEL_35_SPEED;
                }
            }

            public bool IsSummonRvR
            {
                get
                {
                    if (m_level <= 35)
                        return false;
                    else
                        return true;
                }
            }

            public bool IsCombatHorse
            {
                get
                {
                    return false;
                }
            }
        }
        #endregion

        #region GuildBanner
        protected GuildBanner m_guildBanner = null;

        /// <summary>
        /// Gets/Sets the visibility of the carryable RvrGuildBanner. Wont work if the player has no guild.
        /// </summary>
        public GuildBanner GuildBanner
        {
            get { return m_guildBanner; }
            set
            {
                //cant send guildbanner for players without guild.
                if (Guild == null)
                    return;

                m_guildBanner = value;

                if (value != null)
                {
                    foreach (GamePlayer playerToUpdate in GetPlayersInRadius(WorldMgr.OBJ_UPDATE_DISTANCE))
                    {
                        if (playerToUpdate == null) continue;

                        if (playerToUpdate != null && playerToUpdate.Client.IsPlaying)
                        {
                            playerToUpdate.Out.SendRvRGuildBanner(this, true);
                        }
                    }
                }
                else
                {
                    foreach (GamePlayer playerToUpdate in GetPlayersInRadius(WorldMgr.OBJ_UPDATE_DISTANCE))
                    {
                        if (playerToUpdate == null) continue;

                        if (playerToUpdate != null && playerToUpdate.Client.IsPlaying)
                        {
                            playerToUpdate.Out.SendRvRGuildBanner(this, false);
                        }
                    }
                }
            }
        }

        #endregion

        #region Champion Levels

        public virtual string ChampionSpellLineName
        {
            get { return GlobalSpellsLines.Champion_Spells + ":" + InternalID; }
        }

        /// <summary>
        /// The maximum champion level a player can reach
        /// </summary>
        public const int CL_MAX_LEVEL = 10;

        /// <summary>
        /// A table that holds the required XP/Level
        /// </summary>
        public static readonly long[] CLXPLevel =
        {
            0, //xp tp level 0
			32000, //xp to level 1
			64000, // xp to level 2
			96000, // xp to level 3
			128000, // xp to level 4
			160000, // xp to level 5
			192000, // xp to level 6
			224000, // xp to level 7
			256000, // xp to level 8
			288000, // xp to level 9
			320000, // xp to level 10
            640000, // xp to level 11
			640000, // xp to level 12
			640000, // xp to level 13
			640000, // xp to level 14
			640000, // xp to level 15
		};

        /// <summary>
        /// Get the CL title string of the player
        /// </summary>
        public virtual string CLTitle
        {
            get
            {
                if (Champion && ChampionLevel > 0)
                                       
                //return LanguageMgr.GetTranslation(Client.Account.Language, String.Format("Titles.CL.Level{0}", ChampionLevel));
                return string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "Titles.CL.Level"+ChampionLevel+"", ChampionLevel));

                else
                    return "";
            }
        }

        /// <summary>
        /// Is Champion level activated
        /// </summary>
        public virtual bool Champion
        {
            get { return DBCharacter != null ? DBCharacter.Champion : false; }
            set { if (DBCharacter != null) DBCharacter.Champion = value; }
        }

        /// <summary>
        /// Get or create the Champion spell line for this player
        /// </summary>
        /// <returns></returns>
        public virtual SpellLine GetChampionSpellLine()
        {
            SpellLine championPlayerSpellLine = SkillBase.GetSpellLine(ChampionSpellLineName, false);
            if (championPlayerSpellLine == null)
            {
                championPlayerSpellLine = new SpellLine(ChampionSpellLineName, GlobalSpellsLines.Champion_Spells, GlobalSpellsLines.Champion_Spells, true)
                {
                    Level = 50
                };
                SkillBase.RegisterSpellLine(championPlayerSpellLine);
            }

            AddSpellLine(championPlayerSpellLine);
            return championPlayerSpellLine;
        }

        /// <summary>
        /// Champion level
        /// </summary>
        public virtual int ChampionLevel
        {
            get { return DBCharacter != null ? DBCharacter.ChampionLevel : 0; }
            set { if (DBCharacter != null) DBCharacter.ChampionLevel = value; }
        }

        /// <summary>
        /// Max champion level for the player
        /// </summary>
        public virtual int ChampionMaxLevel
        {
            get { return CL_MAX_LEVEL; }
        }

        /// <summary>
        /// Champion Experience
        /// </summary>
        public virtual long ChampionExperience
        {
            get { return DBCharacter != null ? DBCharacter.ChampionExperience : 0; }
            set { if (DBCharacter != null) DBCharacter.ChampionExperience = value; }
        }

        /// <summary>
        /// Champion Available speciality points
        /// </summary>
        public virtual int ChampionSpecialtyPoints
        {
            get { return DBCharacter != null ? DBCharacter.ChampionSpecialtyPoints : 0; }
            set { if (DBCharacter != null) DBCharacter.ChampionSpecialtyPoints = value; }
        }

        /// <summary>
        /// Serialised Champion spells
        /// </summary>
        public virtual string ChampionSpells
        {
            get { return DBCharacter?.ChampionSpells; }
            set { if (DBCharacter != null) DBCharacter.ChampionSpells = value; }
        }

        /// <summary>
        /// Returns how far into the champion level we have progressed
        /// A value between 0 and 1000 (1 bubble = 100)
        /// </summary>
        public virtual ushort ChampionLevelPermill
        {
            get
            {
                //No progress if we haven't even reached current level!
                if (ChampionExperience <= ChampionExperienceForCurrentLevel)
                    return 0;
                //No progess after maximum level
                if (ChampionLevel > CL_MAX_LEVEL) // needed to get exp after 10
                    return 0;
                if ((ChampionExperienceForNextLevel - ChampionExperienceForCurrentLevel) > 0)
                    return (ushort)(1000 * (ChampionExperience - ChampionExperienceForCurrentLevel) / (ChampionExperienceForNextLevel - ChampionExperienceForCurrentLevel));
                else return 0;

            }
        }

        /// <summary>
        /// Returns the xp that are needed for the next level
        /// </summary>
        public virtual long ChampionExperienceForNextLevel
        {
            get { return GetChampionExperienceForLevel(ChampionLevel + 1); }
        }

        /// <summary>
        /// Returns the xp that were needed for the current level
        /// </summary>
        public virtual long ChampionExperienceForCurrentLevel
        {
            get { return GetChampionExperienceForLevel(ChampionLevel); }
        }

        /// <summary>
        /// Gets/Sets amount of champion skills respecs
        /// (delegate to PlayerCharacter)
        /// </summary>
        public virtual int RespecAmountChampionSkill
        {
            get { return DBCharacter != null ? DBCharacter.RespecAmountChampionSkill : 0; }
            set { if (DBCharacter != null) DBCharacter.RespecAmountChampionSkill = value; }
        }

        /// <summary>
        /// Returns the xp that are needed for the specified level
        /// </summary>
        public virtual long GetChampionExperienceForLevel(int level)
        {
            if (level > CL_MAX_LEVEL)
                return CLXPLevel[GamePlayer.CL_MAX_LEVEL]; // exp for level 51, needed to get exp after 50
            if (level <= 0)
                return CLXPLevel[0];
            return CLXPLevel[level];
        }

        public void GainChampionExperience(long experience)
        {
            GainChampionExperience(experience, eXPSource.Other);
        }

        /// <summary>
        /// The process that gains exp
        /// </summary>
        /// <param name="experience">Amount of Experience</param>
        public virtual void GainChampionExperience(long experience, eXPSource source)
        {
            if (ChampionExperience >= 320000)
            {
                ChampionExperience = 320000;
                return;
            }

            // Do not gain experience:
            // - if champion not activated
            // - if champion max level reached
            // - if experience is negative
            // - if praying at your grave
            if (!Champion || ChampionLevel == CL_MAX_LEVEL || experience <= 0 || IsPraying)
                return;

            if (source != eXPSource.GM && source != eXPSource.Quest)
            {
                double modifier = ServerProperties.Properties.CL_XP_RATE;

                // 1 CLXP point per 333K normal XP
                if (this.CurrentRegion.IsRvR)
                {
                    experience = (long)((double)experience * modifier / 333000);
                }
                else // 1 CLXP point per 2 Million normal XP
                {
                    experience = (long)((double)experience * modifier / 2000000);
                }
            }

            System.Globalization.NumberFormatInfo format = System.Globalization.NumberFormatInfo.InvariantInfo;
            Out.SendMessage("You get " + experience.ToString("N0", format) + " champion experience points.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);

            ChampionExperience += experience;
            Out.SendUpdatePoints();
        }


        /// <summary>
        /// Reset all Champion skills for this player
        /// </summary>
        public virtual void RespecChampionSkills()
        {
            SpellLine championSpellLine = GetChampionSpellLine();

            if (championSpellLine != null)
            {
                SkillBase.ClearSpellLine(ChampionSpellLineName);
                if (m_usableSpells != null) m_usableSpells.Clear(); // clear the hybrids spell list
                ChampionSpells = String.Empty;
                ChampionSpecialtyPoints = ChampionLevel;
                UpdateSpellLineLevels(false);
                RemoveAllStyles();
                RefreshSpecDependantSkills(false);
                Out.SendUpdatePlayer();
                Out.SendUpdatePoints();
                Out.SendUpdatePlayerSkills();
                UpdatePlayerStatus();
            }
        }


        /// <summary>
        /// Remove all Champion levels and XP from this character.
        /// </summary>
        public virtual void RemoveChampionLevels()
        {
            ChampionExperience = 0;
            ChampionLevel = 0;
            ChampionSpecialtyPoints = 0;
            ChampionSpells = String.Empty;

            SpellLine championSpellLine = GetChampionSpellLine();

            if (championSpellLine != null)
            {
                RemoveSpellLine(ChampionSpellLineName);
                if (m_usableSpells != null) m_usableSpells.Clear(); // clear the hybrids spell list
                SkillBase.ClearSpellLine(ChampionSpellLineName);
                SkillBase.UnRegisterSpellLine(ChampionSpellLineName);
                UpdateSpellLineLevels(false);
                GetChampionSpellLine();
                RemoveAllStyles();
                RefreshSpecDependantSkills(false);
                Out.SendUpdatePlayer();
                Out.SendUpdatePoints();
                UpdatePlayerStatus();
                Out.SendUpdatePlayerSkills();
            }
        }

        /// <summary>
        /// Holds what happens when your champion level goes up;
        /// </summary>
        public virtual void ChampionLevelUp()
        {
            ChampionLevel++;
            ChampionSpecialtyPoints++;

            // If this is a pure tank then give them full power when reaching champ level 1
            if (ChampionLevel == 1 && CharacterClass.ClassType == eClassType.PureTank)
            {
                Mana = CalculateMaxMana(Level, 0);
            }

            if (ChampionLevel == 3)
            {
                switch (Realm)
                {
                    case eRealm.Albion:
                        AddAbility(SkillBase.GetAbility(Abilities.Weapon_Slashing));
                        AddAbility(SkillBase.GetAbility(Abilities.Weapon_Thrusting));
                        AddAbility(SkillBase.GetAbility(Abilities.Weapon_Crushing));
                        break;
                    case eRealm.Midgard:
                        AddAbility(SkillBase.GetAbility(Abilities.Weapon_Axes));
                        AddAbility(SkillBase.GetAbility(Abilities.Weapon_Hammers));
                        AddAbility(SkillBase.GetAbility(Abilities.Weapon_Swords));
                        break;
                    case eRealm.Hibernia:
                        AddAbility(SkillBase.GetAbility(Abilities.Weapon_Blades));
                        AddAbility(SkillBase.GetAbility(Abilities.Weapon_Blunt));
                        AddAbility(SkillBase.GetAbility(Abilities.Weapon_Piercing));
                        break;
                }
                AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Small));
            }

            Out.SendUpdatePlayerSkills();
            Notify(GamePlayerEvent.ChampionLevelUp, this);
            //Out.SendMessage("You have gained one champion level!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
            Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Champion.YouGainedOneChampionLevel"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            Out.SendUpdatePlayer();
            Out.SendUpdatePoints();
            UpdatePlayerStatus();
        }

        /// <summary>
        /// Load champion spells of this player
        /// </summary>
        protected virtual void LoadChampionSpells(Dictionary<ushort, int> disabledSpells)
        {
            try
            {
                if (string.IsNullOrEmpty(ChampionSpells))
                    return;

                SpellLine championPlayerSpellLine = GetChampionSpellLine();

                if (championPlayerSpellLine == null)
                    return;

                List<int> championSpellList = new List<int>();
                SkillBase.ClearSpellLine(ChampionSpellLineName);

                foreach (string cSpell in ChampionSpells.SplitCSV())
                {
                    string[] cSpellProp = cSpell.Split('|');
                    if (cSpellProp.Length < 2) continue;
                    championSpellList.Add(int.Parse(cSpellProp[0]));
                }

                if (championSpellList != null)
                {
                    foreach (int spellID in championSpellList)
                    {
                        SkillBase.AddSpellToSpellLine(ChampionSpellLineName, spellID);
                    }
                    AddSpellLine(championPlayerSpellLine);
                }

                if (disabledSpells != null)
                {
                    foreach (Spell spell in SkillBase.GetSpellList(ChampionSpellLineName))
                    {
                        if (disabledSpells.ContainsKey(spell.ID))
                            DisableSkill(spell, disabledSpells[spell.ID]);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error loading champion spell line for player {0} ID {1}. ", Name, ChampionSpellLineName);
                log.Error("LoadChampionSpells", ex);
            }
        }

        /// <summary>
        /// Checks if player has this champion spell
        /// </summary>
        public virtual bool HasChampionSpell(int spellid)
        {
            string championSpells = ChampionSpells;
            foreach (string cSpell in championSpells.SplitCSV())
            {
                string[] cSpellProp = cSpell.Split('|');
                if (cSpellProp.Length < 2) continue;
                if (int.Parse(cSpellProp[0]) == spellid) return true;
            }
            return false;
        }

        /// <summary>
        /// Can this player train this a champion spell (meets the prerequisites)
        /// </summary>
        public virtual bool CanTrainChampionSpell(int idline, int skillindex, int index)
        {
            // players can always train the first spell in the sequence
            if (index == 1)
                return true;

            ChampSpec spec;
            ChampSpec specA;
            ChampSpec specB;

            spec = ChampSpecMgr.GetAbilityFromIndex(idline, skillindex, index - 1);

            if (spec == null)
            {
                specA = ChampSpecMgr.GetAbilityFromIndex(idline, skillindex - 1, index - 1);
                specB = ChampSpecMgr.GetAbilityFromIndex(idline, skillindex + 1, index - 1);
                return ((specA != null && HasChampionSpell(specA.SpellID)) || (specB != null && HasChampionSpell(specB.SpellID)));
            }

            return HasChampionSpell(spec.SpellID);
        }

        #endregion

        #region Master levels

        /// <summary>
        /// The maximum ML level a player can reach
        /// </summary>
        public const int ML_MAX_LEVEL = 10;

        /// <summary>
        /// Amount of MLXP required for ML validation. MLXP reset at every ML.
        /// </summary>
        private static readonly long[] MLXPLevel =
        {
            0, //xp tp level 0
			32000, //xp to level 1
			32000, // xp to level 2
			32000, // xp to level 3
			32000, // xp to level 4
			32000, // xp to level 5
			32000, // xp to level 6
			32000, // xp to level 7
			32000, // xp to level 8
			32000, // xp to level 9
			32000, // xp to level 10
		};

        /// <summary>
        /// Get the amount of XP needed for a ML
        /// </summary>
        /// <param name="ml"></param>
        /// <returns></returns>
        public virtual long GetXPForML(byte ml)
        {
            if (ml > MLXPLevel.Length - 1)
                return 0;

            return MLXPLevel[ml];
        }

        /// <summary>
        /// How many steps for each Master Level
        /// </summary>
        private static readonly byte[] MLStepsForLevel =
        {
            10, // ML0 == ML1
			10, // ML1
			11,
            11,
            11,
            11,
            11,
            11,
            11,
            11,
            5, // ML10
		};

        /// <summary>
        /// Get the number of steps required for a ML
        /// </summary>
        /// <param name="ml"></param>
        /// <returns></returns>
        public virtual byte GetStepCountForML(byte ml)
        {
            if (ml > MLStepsForLevel.Length - 1)
                return 0;

            return MLStepsForLevel[ml];
        }

        /// <summary>
        /// True if player has started Master Levels
        /// </summary>
        public virtual bool MLGranted
        {
            get { return DBCharacter != null ? DBCharacter.MLGranted : false; }
            set { if (DBCharacter != null) DBCharacter.MLGranted = value; }
        }

        /// <summary>
        /// Last Daily Reward Time
        /// </summary>
        public virtual DateTime LastDailyRewardTime
        {
            get { return DBCharacter != null ? DBCharacter.LastDailyRewardTime : DateTime.Now; }
            set { if (DBCharacter != null) DBCharacter.LastDailyRewardTime = value; }
        }

        /// <summary>
        /// What ML line has this character chosen
        /// </summary>
        public virtual byte MLLine
        {
            get { return DBCharacter != null ? DBCharacter.ML : (byte)0; }
            set { if (DBCharacter != null) DBCharacter.ML = value; }
        }

        /// <summary>
        /// Gets and sets the last ML the player has completed.
        /// MLLevel is advanced once all steps are completed.
        /// </summary>
        public virtual int MLLevel
        {
            get { return DBCharacter != null ? DBCharacter.MLLevel : 0; }
            set { if (DBCharacter != null) DBCharacter.MLLevel = value; }
        }

        /// <summary>
        /// Gets and sets ML Experience for the current ML level
        /// </summary>
        public virtual long MLExperience
        {
            get { return DBCharacter != null ? DBCharacter.MLExperience : 0; }
            set { if (DBCharacter != null) DBCharacter.MLExperience = value; }
        }

        /// <summary>
        /// Get the number of steps completed for a ML
        /// </summary>
        /// <param name="ml"></param>
        /// <returns></returns>
        public virtual byte GetCountMLStepsCompleted(byte ml)
        {
            byte count = 0;
            int steps = GetStepCountForML(ml);

            for (byte i = 1; i <= steps; i++)
            {
                if (HasFinishedMLStep(ml, i))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Check ML step completition.
        /// Arbiter checks this to see if player is eligible to advance to the next Master Level.
        /// </summary>
        public virtual bool HasFinishedMLStep(int mlLevel, int step)
        {
            // No steps registered so false
            if (m_mlSteps == null) return false;

            // Current ML Level >= required ML, so true
            if (MLLevel >= mlLevel) return true;

            // Check current registered steps
            foreach (DBCharacterXMasterLevel mlStep in m_mlSteps)
            {
                // Found so return value
                if (mlStep.MLLevel == mlLevel && mlStep.MLStep == step)
                    return mlStep.StepCompleted;
            }

            // Not found so false
            return false;
        }

        /// <summary>
        /// Sets an ML step to finished or clears it
        /// </summary>
        /// <param name="mlLevel"></param>
        /// <param name="step"></param>
        /// <param name="setFinished">(optional) false will remove the finished entry for this step</param>
        public virtual void SetFinishedMLStep(int mlLevel, int step, bool setFinished = true)
        {
            // Check current registered steps in case of previous GM rollback command
            if (m_mlSteps != null)
            {
                foreach (DBCharacterXMasterLevel mlStep in m_mlSteps)
                {
                    if (mlStep.MLLevel == mlLevel && mlStep.MLStep == step)
                    {
                        mlStep.StepCompleted = setFinished;
                        return;
                    }
                }
            }

            if (setFinished)
            {
                // Register new step
                DBCharacterXMasterLevel newStep = new DBCharacterXMasterLevel
                {
                    Character_ID = QuestPlayerID,
                    MLLevel = mlLevel,
                    MLStep = step,
                    StepCompleted = true,
                    ValidationDate = DateTime.Now
                };
                m_mlSteps.Add(newStep);

                // Add it in DB
                try
                {
                    GameServer.Database.AddObject(newStep);
                }
                catch (Exception e)
                {
                    if (log.IsErrorEnabled)
                        log.Error("Error adding player " + Name + " ml step!", e);
                }

                // Refresh Window
                Out.SendMasterLevelWindow((byte)mlLevel);
            }
        }

        /// <summary>
        /// Returns the xp that are needed for the specified level
        /// </summary>
        public virtual long GetMLExperienceForLevel(int level)
        {
            if (level >= ML_MAX_LEVEL)
                return MLXPLevel[GamePlayer.ML_MAX_LEVEL - 1]; // exp for level 9, needed to get exp after 9
            if (level <= 0)
                return MLXPLevel[0];
            return MLXPLevel[level];
        }

        /// <summary>
        /// Get the Masterlevel window text for a ML and Step
        /// </summary>
        /// <param name="ml"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public virtual string GetMLStepDescription(byte ml, int step)
        {
            string description;

            if (HasFinishedMLStep(ml, step))
            {
                description = LanguageMgr.GetTranslation(Client.Account.Language, String.Format("SendMasterLevelWindow.Complete.ML{0}.Step{1}", ml, step));
               
                if (description.Contains("DE") || description.Contains("EN") || description.Contains("FR") || description.Contains("IT") || description.Contains("ES") || description.Contains("NL") || description.Contains("CZ"))
                {
                    description = "Click on forward > for ML text step infos.";
                }
                
            }
            else
                description = LanguageMgr.GetTranslation(Client.Account.Language, String.Format("SendMasterLevelWindow.Uncomplete.ML{0}.Step{1}", ml, step));

            return description;
        }


        /// <summary>
        /// Get the ML title string of the player
        /// </summary>
        public virtual string MLTitle
        {
            get
            {

                if (MLLine > 0)
                    return string.Format(LanguageMgr.GetTranslation(Client.Account.Language, "Titles.ML.Line" + (int)MLLine + "", (int)MLLine));
                //return LanguageMgr.GetTranslation(Client.Account.Language, String.Format("Titles.ML.Line{0}", (int)MLLine));
                else
                    return "None";
            }
        }

        #endregion

        #region Minotaur Relics
        protected MinotaurRelic m_minoRelic = null;

        /// <summary>
        /// sets or sets the Minotaur Relic of this Player
        /// </summary>
        public MinotaurRelic MinotaurRelic
        {
            get { return m_minoRelic; }
            set { m_minoRelic = value; }
        }
        #endregion

        #region Artifacts

        /// <summary>
        /// Checks if the player's class has at least one version of the artifact specified available to them.
        /// </summary>
        /// <param name="artifactID"></param>
        /// <returns>True when at least one version exists, false when no versions are available.</returns>
        public bool CanReceiveArtifact(string artifactID)
        {
            Dictionary<String, ItemTemplate> possibleVersions = ArtifactMgr.GetArtifactVersions(artifactID, (eCharacterClass)CharacterClass.ID, Realm);

            if (possibleVersions.Count == 0)
                return false;

            return true;
        }

        #endregion

        #region Constructors
        /// <summary>
        /// Returns the string representation of the GamePlayer
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return new StringBuilder(base.ToString())
                .Append(" class=").Append(CharacterClass.Name)
                .Append('(').Append(CharacterClass.ID.ToString()).Append(')')
                .ToString();
        }

        /// <summary>
        /// Creates a new player
        /// </summary>
        /// <param name="client">The GameClient for this player</param>
        /// <param name="dbChar">The character for this player</param>
        public GamePlayer(GameClient client, DOLCharacters dbChar)
            : base()
        {
            IsJumping = false;
            m_steed = new WeakRef(null);
            m_rangeAttackAmmo = new WeakRef(null);
            m_rangeAttackTarget = new WeakRef(null);
            m_client = client;
            m_dbCharacter = dbChar;
            m_controlledHorse = new ControlledHorse(this);
            m_buff1Bonus = new PropertyIndexer((int)eProperty.MaxProperty); // set up a fixed indexer for players
            m_buff2Bonus = new PropertyIndexer((int)eProperty.MaxProperty);
            m_debuffBonus = new PropertyIndexer((int)eProperty.MaxProperty);
            m_buff4Bonus = new PropertyIndexer((int)eProperty.MaxProperty);
            m_itemBonus = new PropertyIndexer((int)eProperty.MaxProperty);
            m_lastUniqueLocations = new GameLocation[4];
            //m_objectUpdates = new BitArray[2];
            //m_objectUpdates[0] = new BitArray(Properties.REGION_MAX_OBJECTS);
            //m_objectUpdates[1] = new BitArray(Properties.REGION_MAX_OBJECTS);
            //m_housingUpdateArray = null;
            // m_lastUpdateArray = false;
            m_canFly = false;


            CreateInventory();
            GameEventMgr.AddHandler(m_inventory, PlayerInventoryEvent.ItemEquipped, new DOLEventHandler(OnItemEquipped));
            GameEventMgr.AddHandler(m_inventory, PlayerInventoryEvent.ItemUnequipped, new DOLEventHandler(OnItemUnequipped));
            GameEventMgr.AddHandler(m_inventory, PlayerInventoryEvent.ItemBonusChanged, new DOLEventHandler(OnItemBonusChanged));

            m_enteredGame = false;
            m_customDialogCallback = null;
            m_sitting = false;
            m_isWireframe = false;
            m_characterClass = new DefaultCharacterClass();
            m_groupIndex = 0xFF;
            m_currentAreas = new ArrayList();

            m_saveInDB = true;
            LoadFromDatabase(dbChar);

            CreateStatistics();
        }

        /// <summary>
        /// Create this players inventory
        /// </summary>
        protected virtual void CreateInventory()
        {
            m_inventory = new GamePlayerInventory(this);
        }

        #endregion

        #region Delving
        /// <summary>
		/// Player is delving an item
		/// </summary>
		/// <param name="item"></param>
		/// <param name="delveInfo"></param>
		/// <returns>false if delve not handled</returns>
		public virtual bool DelveItem<T>(T item, List<string> delveInfo, GamePlayer player)
        {
            player = this;
           
            if (item is IGameInventoryItem)
            {
                (item as IGameInventoryItem).Delve(delveInfo, player);
            }
            else if (item is InventoryItem)
            {
                GameInventoryItem tempItem = GameInventoryItem.Create<InventoryItem>(item as InventoryItem);
                tempItem.Delve(delveInfo, player);
            }
            else if (item is ItemTemplate)
            {
                GameInventoryItem tempItem = GameInventoryItem.Create<ItemTemplate>(item as ItemTemplate);
                tempItem.Delve(delveInfo, player);
            }
            else
            {
                delveInfo.Add("Error, unable to delve this item!");
                //log.ErrorFormat("Error delving item of ClassType {0}", item.GetType().FullName);
            }

            return true;
        }


        /// <summary>
        /// Player is delving a spell
        /// </summary>
        /// <param name="output"></param>
        /// <param name="spell"></param>
        /// <param name="spellLine"></param>
        /// <returns>false if not handled here, use default delve</returns>
        public virtual bool DelveSpell(IList<string> output, Spell spell, SpellLine spellLine)
        {
            return false;
        }


        /// <summary>
        /// Delve a weapon style for this player
        /// </summary>
        /// <param name="delveInfo"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public virtual void DelveWeaponStyle(IList<string> delveInfo, Style style)
        {
            StyleProcessor.DelveWeaponStyle(delveInfo, style, this);
        }



        /// <summary>
        /// Get a list of bonuses that effect this player
        /// </summary>
        public virtual ICollection<string> GetBonuses()
        {
            return this.GetBonusesInfo();
        }


        #endregion

        #region Combat Calc (unused ?)


        public virtual double GetEvadeChance()
        {
            double evadeChance = 0;

            GameSpellEffect evade = SpellHandler.FindEffectOnTarget(this, "EvadeBuff");
            if (evade == null)
                evade = SpellHandler.FindEffectOnTarget(this, "SavageEvadeBuff");

            if (HasAbility(Abilities.Advanced_Evade) || EffectList.GetOfType<CombatAwarenessEffect>() != null || EffectList.GetOfType<RuneOfUtterAgilityEffect>() != null)
                evadeChance = GetModified(eProperty.EvadeChance);
            else if (evade != null || HasAbility(Abilities.Evade))
            {
                int res = GetModified(eProperty.EvadeChance);
                if (res > 0)
                    evadeChance = res;
            }

            if (evadeChance > 0)
            {
                evadeChance *= 0.001;
                if (evadeChance < 0.01)
                    evadeChance = 0.01;
                if (evadeChance > 0.5)
                    evadeChance = 0.5;
            }
            return Math.Round(evadeChance * 10000) / 100;
        }

        public virtual double GetBlockChance()
        {
            double blockChance = 0;
            InventoryItem lefthand;
            if (HasAbility(Abilities.Shield))
            {
                lefthand = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
                if (lefthand != null && (AttackWeapon == null || AttackWeapon.Item_Type == Slot.RIGHTHAND || AttackWeapon.Item_Type == Slot.LEFTHAND))
                {
                    if (lefthand.Object_Type == (int)eObjectType.Shield)
                        blockChance = GetModified(eProperty.BlockChance) * lefthand.Quality * 0.01;
                }
            }
            if (blockChance > 0)
            {
                blockChance *= 0.001;
                if (blockChance > 0.99) blockChance = 0.99;
                if (blockChance < 0.01) blockChance = 0.01;
            }

            return Math.Round(blockChance * 10000) / 100;
        }

        public virtual double GetParryChance()
        {
            double parryChance = 0;

            GameSpellEffect parry = SpellHandler.FindEffectOnTarget(this, "ParryBuff");
            if (parry == null)
                parry = SpellHandler.FindEffectOnTarget(this, "SavageParryBuff");

            if ((HasSpecialization(Specs.Parry) || parry != null) && (AttackWeapon != null))
                parryChance = GetModified(eProperty.ParryChance);
            else if (EffectList.GetOfType<BladeBarrierEffect>() != null)
                parryChance = GetModified(eProperty.ParryChance);

            if (parryChance > 0)
            {
                parryChance *= 0.001;
                if (parryChance < 0.01) parryChance = 0.01;
                if (parryChance > 0.99) parryChance = 0.99;
            }

            return Math.Round(parryChance * 10000) / 100;
        }
        #endregion


        /// <summary>
        /// True, if the player has been standing still for at least 3 seconds,
        /// else false.
        /// </summary>
        private bool IsStandingStill
        {
            get
            {
                if (IsMoving)
                    return false;

                long lastMovementTick = TempProperties.getProperty<long>("PLAYERPOSITION_LASTMOVEMENTTICK");
                return (CurrentRegion.Time - lastMovementTick > 3000);
            }
        }

        /// <summary>
        /// This player's bodyguard (ML ability) or null, if there is none.
        /// </summary>
        public GamePlayer Bodyguard
        {
            get
            {
                var bodyguardEffects = EffectList.GetAllOfType<BodyguardEffect>();

                BodyguardEffect bodyguardEffect = bodyguardEffects.FirstOrDefault();
                if (bodyguardEffect == null || bodyguardEffect.GuardTarget != this)
                    return null;

                GamePlayer guard = bodyguardEffect.GuardSource;
                GamePlayer guardee = this;

                return (!guard.IsRiding && guard.IsAlive && !guard.IsStunned && !guard.IsMezzed && (guard.CurrentRegion.IsRvR) && guard.IsWithinRadius((GamePlayer)guardee, BodyguardAbilityHandler.BODYGUARD_DISTANCE) &&
                        !guard.IsCasting && guardee.IsStandingStill && !guardee.IsRiding)
                    ? guard
                    : null;
            }
        }

    }

}
