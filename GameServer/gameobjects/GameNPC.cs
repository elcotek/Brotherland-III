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

using DOL.AI;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.Housing;
using DOL.GS.Keeps;
using DOL.GS.Movement;
using DOL.GS.PacketHandler;
using DOL.GS.Quests;
using DOL.GS.Scripts;
using DOL.GS.ServerProperties;
using DOL.GS.Spells;
using DOL.GS.Styles;
using DOL.GS.Utils;
using DOL.Language;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DOL.GS
{
    /// <summary>
    /// This class is the baseclass for all Non Player Characters like
    /// Monsters, Merchants, Guards, Steeds ...
    /// </summary>
    public class GameNPC : GameLiving, ITranslatableObject
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public const string SpellQueued = "SPELLQUEUED";
        private const string LOSEFFECTIVENESSNpcs = "LOS Effectivness npcs";

        //private const int STICKMINIMUMRANGE = 100;
        private const int STICKMAXIMUMRANGE = 5000;


        /// <summary>
        /// Minimum Melee Attack Range 
        /// </summary>
        private static int m_minimumAttackRange;

        /// <summary>
        /// Minimum Melee Attack Range 
        /// </summary>
        private int STICKMINIMUMRANGE
        {
            get
            {
                if (MeleeAttackRange <= 0)
                    return m_minimumAttackRange = 100;


                return m_minimumAttackRange = MeleeAttackRange;
            }

        }


        private bool m_spawn;

        /// <summary>
        /// The Ork Spawn
        /// </summary>
        private bool TheOrkSpawn
        {
            get { return m_spawn; }
            set { m_spawn = value; }
        }




        private int m_KillCount;

        /// <summary>
        /// The NPC's Count of Kills
        /// </summary>
        public int KillCount
        {
            get { return m_KillCount; }
            set { m_KillCount = value; }
        }

        //DemonQueen Spawn
        private GameObject m_mobkiller;
        private string m_mobname;

        /// <summary>
        /// Gets/sets the mob killer
        /// </summary>
        private GameObject MobKiller
        {
            get
            {
                return m_mobkiller;
            }
            set
            {
                m_mobkiller = value;
            }
        }

        /// <summary>
        /// Gets/sets the killed Mob Name
        /// </summary>
        private string MobName
        {
            get
            {
                return m_mobname;
            }
            set
            {
                m_mobname = value;
            }
        }



        public GameLiving Owner
        {
            get
            {
                if (Brain is IControlledBrain)
                {
                    return (Brain as IControlledBrain).Owner;
                }

                return null;
            }
        }
        /// <summary>
        /// true if Strength debuff active on a npc.
        /// </summary>
        /// <param name="player"></param>
        private bool DebuffActive = false;
        protected short m_oldStr;
        ///protected IList m_newstyles;
        //protected IList m_newstyles2;
        /// <summary>
        /// old Strength value of debuffed npc.
        /// </summary>
        /// <param name="debuffed npcs"></param>
        public short OldSTR
        {
            get { return m_oldStr; }
            set
            {
                if (DebuffActive == false)
                    m_oldStr = value;
            }
        }

        protected bool m_assist = false;

        /// <summary>
        /// Enable Assist by command for pets
        /// </summary>
        public bool EnableAssist
        {

            get { return m_assist; }
            set { m_assist = value; }
        }


        /// <summary>
        /// Gets/sets the body of this brain
        /// </summary>
        public GameNPC Body
        {
            get { return m_body; }
            set { m_body = value; }
        }


        // protected GameLiving m_owner;
        /// <summary>
        /// The body of this brain
        /// </summary>
        protected GameNPC m_body;

        /// <summary>
        /// The living to that this effect is applied to
        /// </summary>
        //   public GameLiving Owner
        //  {
        //       get { return m_owner; }
        //  }


        /// <summary>
        /// Constant for determining if already at a point
        /// </summary>
        /// <remarks>
        /// This helps to reduce the turning of an npc while fighting or returning to a spawn
        /// Tested - min distance for mob sticking within combat range to player is 25
        /// </remarks>
        public const int CONST_WALKTOTOLERANCE = 25;

        /// <summary>
        /// For GamePets
        /// </summary>
        public bool letAttack = false;


        #region Debug
        private bool m_debugMode = ServerProperties.Properties.ENABLE_Pathig_DEBUG;
        public bool DebugMode
        {
            get { return m_debugMode; }
            set
            {
                m_debugMode = value;
                if (PathCalculator != null)
                    PathCalculator.VisualizePath = value;
            }
        }
        public virtual void DebugSend(string str, params object[] args)
        {
            if (!DebugMode)
                return;
            str = string.Format(str, args);
            // Say("[DEBUG] " + str);
            //log.Debug($"[pathing {Name}] {str}");
        }

        #endregion
        #region Formations/Spacing

        //Space/Offsets used in formations
        // Normal = 1
        // Big = 2
        // Huge = 3
        private byte m_formationSpacing = 1;

        /// <summary>
        /// The Minions's x-offset from it's commander
        /// </summary>
        public byte FormationSpacing
        {
            get { return m_formationSpacing; }
            set
            {
                //BD range values vary from 1 to 3.  It is more appropriate to just ignore the
                //incorrect values than throw an error since this isn't a very important area.
                if (value > 0 && value < 4)
                    m_formationSpacing = value;
            }
        }

        /// <summary>
        /// Used for that formation type if a GameNPC has a formation
        /// </summary>
        public enum eFormationType
        {
            // M = owner
            // x = following npcs
            //Line formation
            // M x x x
            Line,
            //Triangle formation
            //		x
            // M x
            //		x
            Triangle,
            //Protect formation
            //		 x
            // x  M
            //		 x
            Protect,
        }

        private eFormationType m_formation = eFormationType.Line;
        /// <summary>
        /// How the minions line up with the commander
        /// </summary>
        public eFormationType Formation
        {
            get { return m_formation; }
            set { m_formation = value; }
        }

        #endregion

        #region Sizes/Properties


        /// <summary>
        /// Holds the size of the NPC
        /// </summary>
        protected byte m_size;


        protected GameSpellEffect m_gameSpellEffect;



        /// <summary>
        /// Gets or sets the size of the npc
        /// </summary>
        public byte Size
        {
            get { return m_size; }
            set
            {
                m_size = value;
                if (ObjectState == eObjectState.Active)
                {
                    foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                        player.Out.SendModelAndSizeChange(this, Model, value);
                    //					BroadcastUpdate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the model of this npc
        /// </summary>
        public override ushort Model
        {
            get { return base.Model; }
            set
            {
                base.Model = value;
                if (ObjectState == eObjectState.Active)
                {
                    foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                        player.Out.SendModelChange(this, Model);
                }
            }
        }
        public virtual LanguageDataObject.eTranslationIdentifier TranslationIdentifier
        {
            get { return LanguageDataObject.eTranslationIdentifier.eNPC; }
        }

        /// <summary>
		/// Holds the translation id.
		/// </summary>
		protected string m_translationId = "";

        /// <summary>
        /// Gets or sets the translation id.
        /// </summary>
        public string TranslationId
        {
            get { return m_translationId; }
            set { m_translationId = (value ?? ""); }
        }




        /// <summary>
        /// Gets or sets the heading of this NPC
        /// </summary>
        public override ushort Heading
        {
            get { return base.Heading; }
            set
            {
                if (IsTurningDisabled)
                    return;
                ushort oldHeading = base.Heading;
                base.Heading = value;
                if (base.Heading != oldHeading)
                    BroadcastUpdate();
            }
        }

        /// <summary>
        /// Gets or sets the level of this NPC
        /// </summary>
        public override byte Level
        {
            get { return base.Level; }
            set
            {
                base.Level = value;

                if (Strength + Constitution + Dexterity + Quickness + Intelligence + Piety + Empathy + Charisma <= 0)
                {
                    AutoSetStats();
                }

                if (!InCombat && CanRestoreHealth(this))
                    m_health = MaxHealth;

                if (ObjectState == eObjectState.Active)
                {
                    foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    {
                        player.Out.SendNPCCreate(this);
                        if (m_inventory != null)
                            player.Out.SendLivingEquipmentUpdate(this);
                    }
                    BroadcastUpdate();
                }
            }
        }

        public virtual void AutoSetStats()
        {
            // Values changed by Argo, based on Tolakrams Advice for how to change the Multiplier for Autoset str

            Strength = (short)(Properties.MOB_AUTOSET_STR_BASE + Level * 10 * Properties.MOB_AUTOSET_STR_MULTIPLIER);
            Constitution = (short)(Properties.MOB_AUTOSET_CON_BASE + Level * Properties.MOB_AUTOSET_CON_MULTIPLIER);
            Quickness = (short)(Properties.MOB_AUTOSET_QUI_BASE + Level * Properties.MOB_AUTOSET_QUI_MULTIPLIER);
            Dexterity = (short)(Properties.MOB_AUTOSET_DEX_BASE + Level * Properties.MOB_AUTOSET_DEX_MULTIPLIER);

            Intelligence = (short)(30);
            Empathy = (short)(30);
            Piety = (short)(30);
            Charisma = (short)(30);

        }


        /// <summary>
        /// Gets or Sets the effective level of the Object
        /// </summary>
        public override int EffectiveLevel
        {
            get
            {
                IControlledBrain brain = Brain as IControlledBrain;
                if (brain != null)
                    return brain.Owner.Level;
                return base.Level;
            }
        }

        /// <summary>
        /// Gets or sets the Realm of this NPC
        /// </summary>
        public override eRealm Realm
        {
            get
            {
                IControlledBrain brain = Brain as IControlledBrain;
                if (brain != null)
                    return brain.Owner.Realm; // always realm of the owner
                return base.Realm;
            }
            set
            {
                base.Realm = value;
                if (ObjectState == eObjectState.Active)
                {
                    foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    {
                        player.Out.SendNPCCreate(this);
                        if (m_inventory != null)
                            player.Out.SendLivingEquipmentUpdate(this);
                    }
                    BroadcastUpdate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of this npc
        /// </summary>
        public override string Name
        {
            get { return base.Name; }
            set
            {
                base.Name = value;
                if (ObjectState == eObjectState.Active)
                {
                    foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    {
                        player.Out.SendNPCCreate(this);
                        if (m_inventory != null)
                            player.Out.SendLivingEquipmentUpdate(this);
                    }
                    BroadcastUpdate();
                }
            }
        }

        /// <summary>
		/// Holds the suffix.
		/// </summary>
		private string m_suffix = string.Empty;
        /// <summary>
        /// Gets or sets the suffix.
        /// </summary>
        public string Suffix
        {
            get { return m_suffix; }
            set
            {
                if (value == null)
                    m_suffix = string.Empty;
                else
                {
                    if (value == m_suffix)
                        return;
                    else
                        m_suffix = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the guild name
        /// </summary>
        public override string GuildName
        {
            get { return base.GuildName; }
            set
            {
                base.GuildName = value;
                if (ObjectState == eObjectState.Active)
                {
                    foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    {
                        player.Out.SendNPCCreate(this);
                        if (m_inventory != null)
                            player.Out.SendLivingEquipmentUpdate(this);
                    }
                    BroadcastUpdate();
                }
            }
        }

        /// <summary>
		/// Holds the examine article.
		/// </summary>
		private string m_examineArticle = string.Empty;
        /// <summary>
        /// Gets or sets the examine article.
        /// </summary>
        public string ExamineArticle
        {
            get { return m_examineArticle; }
            set
            {
                if (value == null)
                    m_examineArticle = string.Empty;
                else
                {
                    if (value == m_examineArticle)
                        return;
                    else
                        m_examineArticle = value;
                }
            }
        }

        /// <summary>
        /// Holds the message article.
        /// </summary>
        private string m_messageArticle = string.Empty;
        /// <summary>
        /// Gets or sets the message article.
        /// </summary>
        public string MessageArticle
        {
            get { return m_messageArticle; }
            set
            {
                if (value == null)
                    m_messageArticle = string.Empty;
                else
                {
                    if (value == m_messageArticle)
                        return;
                    else
                        m_messageArticle = value;
                }
            }
        }

        private Faction m_faction = null;
        /// <summary>
        /// Gets the Faction of the NPC
        /// </summary>
        public Faction Faction
        {
            get { return m_faction; }
            set
            {
                m_faction = value;
            }
        }

        private ArrayList m_linkedFactions;
        /// <summary>
        /// The linked factions for this NPC
        /// </summary>
        public ArrayList LinkedFactions
        {
            get { return m_linkedFactions; }
            set { m_linkedFactions = value; }
        }

        private bool m_isConfused;
        /// <summary>
        /// Is this NPC currently confused
        /// </summary>
        public bool IsConfused
        {
            get { return m_isConfused; }
            set { m_isConfused = value; }
        }

        private ushort m_bodyType;
        /// <summary>
        /// The NPC's body type
        /// </summary>
        public ushort BodyType
        {
            get { return m_bodyType; }
            set { m_bodyType = value; }
        }

        private ushort m_houseNumber;
        /// <summary>
        /// The NPC's current house
        /// </summary>
        public ushort HouseNumber
        {
            get { return m_houseNumber; }
            set { m_houseNumber = value; }
        }
        #endregion

        #region Stats


        /// <summary>
        /// Change a stat value
        /// (delegate to GameNPC)
        /// </summary>
        /// <param name="stat">The stat to change</param>
        /// <param name="val">The new value</param>
        public override void ChangeBaseStat(eStat stat, short val)
        {
            int oldstat = GetBaseStat(stat);
            base.ChangeBaseStat(stat, val);
            int newstat = GetBaseStat(stat);
            GameNPC npc = this;
            if (this != null && oldstat != newstat)
            {
                switch (stat)
                {
                    case eStat.STR: npc.Strength = (short)newstat; break;
                    case eStat.DEX: npc.Dexterity = (short)newstat; break;
                    case eStat.CON: npc.Constitution = (short)newstat; break;
                    case eStat.QUI: npc.Quickness = (short)newstat; break;
                    case eStat.INT: npc.Intelligence = (short)newstat; break;
                    case eStat.PIE: npc.Piety = (short)newstat; break;
                    case eStat.EMP: npc.Empathy = (short)newstat; break;
                    case eStat.CHR: npc.Charisma = (short)newstat; break;

                }
            }
        }

        /// <summary>
        /// Gets NPC's constitution
        /// </summary>
        public virtual short Constitution
        {
            get
            {
                return m_charStat[eStat.CON - eStat._First];
            }
            set
            {
                m_charStat[eStat.CON - eStat._First] = value;
            }

        }

        /// <summary>
        /// Gets NPC's dexterity
        /// </summary>
        public virtual short Dexterity
        {
            get { return m_charStat[eStat.DEX - eStat._First]; }
            set { m_charStat[eStat.DEX - eStat._First] = value; }
        }

        /// <summary>
        /// Gets NPC's strength
        /// </summary>
        public virtual short Strength
        {
            get { return m_charStat[eStat.STR - eStat._First]; }
            set { m_charStat[eStat.STR - eStat._First] = value; }
        }

        /// <summary>
        /// Gets NPC's quickness
        /// </summary>
        public virtual short Quickness
        {
            get { return m_charStat[eStat.QUI - eStat._First]; }
            set { m_charStat[eStat.QUI - eStat._First] = value; }
        }

        /// <summary>
        /// Gets NPC's intelligence
        /// </summary>
        public virtual short Intelligence
        {
            get { return m_charStat[eStat.INT - eStat._First]; }
            set { m_charStat[eStat.INT - eStat._First] = value; }
        }

        /// <summary>
        /// Gets NPC's piety
        /// </summary>
        public virtual short Piety
        {
            get { return m_charStat[eStat.PIE - eStat._First]; }
            set { m_charStat[eStat.PIE - eStat._First] = value; }
        }

        /// <summary>
        /// Gets NPC's empathy
        /// </summary>
        public virtual short Empathy
        {
            get { return m_charStat[eStat.EMP - eStat._First]; }
            set { m_charStat[eStat.EMP - eStat._First] = value; }
        }

        /// <summary>
        /// Gets NPC's charisma
        /// </summary>
        public virtual short Charisma
        {
            get { return m_charStat[eStat.CHR - eStat._First]; }
            set { m_charStat[eStat.CHR - eStat._First] = value; }
        }
        #endregion

        #region Flags/Position/SpawnPosition/UpdateTick/Tether
        /// <summary>
        /// Various flags for this npc
        /// </summary>
        [Flags]
        public enum eFlags : uint
        {
            /// <summary>
            /// The npc is translucent (like a ghost)
            /// </summary>
            GHOST = 0x01,
            /// <summary>
            /// The npc is stealthed (nearly invisible, like a stealthed player; new since 1.71)
            /// </summary>
            STEALTH = 0x02,
            /// <summary>
            /// The npc doesn't show a name above its head but can be targeted
            /// </summary>
            DONTSHOWNAME = 0x04,
            /// <summary>
            /// The npc doesn't show a name above its head and can't be targeted
            /// </summary>
            CANTTARGET = 0x08,
            /// <summary>
            /// Not in nearest enemyes if different vs player realm, but can be targeted if model support this
            /// </summary>
            PEACE = 0x10,
            /// <summary>
            /// The npc is flying (z above ground permitted)
            /// </summary>
            FLYING = 0x20,
            /// <summary>
            /// npc's torch is lit
            /// </summary>
            TORCH = 0x40,
            /// <summary>
            /// npc is a statue (no idle animation, no target...)
            /// </summary>
            STATUE = 0x80,
            /// <summary>
            /// npc is swimming
            /// </summary>
            SWIMMING = 0x100
        }

        /// <summary>
        /// Holds various flags of this npc
        /// </summary>
        protected eFlags m_flags;
        /// <summary>
        /// Spawn point
        /// </summary>
        protected Point3D m_spawnPoint;
        /// <summary>
        /// Spawn Heading
        /// </summary>
        protected ushort m_spawnHeading;


        /// <summary>
        /// package ID defined form this NPC
        /// </summary>
        protected string m_packageID;

        public string PackageID
        {
            get { return m_packageID; }
            set { m_packageID = value; }
        }



        /// <summary>
        /// The last time this NPC sent the 0x09 update packet
        /// </summary>
        protected volatile uint m_lastUpdateTickCount = uint.MinValue;
        /// <summary>
        /// The last time this NPC was actually updated to at least one player
        /// </summary>
        protected volatile uint m_lastVisibleToPlayerTick = uint.MinValue;

        /// <summary>
        /// Gets or Sets the flags of this npc
        /// </summary>
        public virtual eFlags Flags
        {
            get { return m_flags; }
            set
            {
                eFlags oldflags = m_flags;
                m_flags = value;
                if (ObjectState == eObjectState.Active)
                {
                    if (oldflags != m_flags)
                    {
                        foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                        {

                            player.Out.SendNPCCreate(this);
                            if (m_inventory != null)
                                player.Out.SendLivingEquipmentUpdate(this);
                        }
                    }
                    BroadcastUpdate();
                }
            }
        }




        public override bool IsUnderwater
        {
            get { return (m_flags & eFlags.SWIMMING) == eFlags.SWIMMING || base.IsUnderwater; }
        }


        /// <summary>
        /// Shows wether any player sees that mob
        /// we dont need to calculate things like AI if mob is in no way
        /// visible to at least one player
        /// </summary>
        public virtual bool IsVisibleToPlayers => GameTimer.GetTickCount() - m_lastVisibleToPlayerTick < 60000;

        /// <summary>
        /// Gets or sets the spawnposition of this npc
        /// </summary>
        public virtual Point3D SpawnPoint
        {
            get { return m_spawnPoint; }
            set { m_spawnPoint = value; }
        }

        /*
        /// <summary>
        /// Gets or sets the spawnposition of this npc
        /// </summary>
        [Obsolete("Use GameNPC.SpawnPoint")]
        public virtual int SpawnX
        {
            get { return m_spawnPoint.X; }
            set { m_spawnPoint.X = value; }
        }
        /// <summary>
        /// Gets or sets the spawnposition of this npc
        /// </summary>
        [Obsolete("Use GameNPC.SpawnPoint")]
        public virtual int SpawnY
        {
            get { return m_spawnPoint.Y; }
            set { m_spawnPoint.Y = value; }
        }
        /// <summary>
        /// Gets or sets the spawnposition of this npc
        /// </summary>
        [Obsolete("Use GameNPC.SpawnPoint")]
        public virtual int SpawnZ
        {
            get { return m_spawnPoint.Z; }
            set { m_spawnPoint.Z = value; }
        }
        */
        /// <summary>
        /// Gets or sets the spawnheading of this npc
        /// </summary>
        public virtual ushort SpawnHeading
        {
            get { return m_spawnHeading; }
            set { m_spawnHeading = value; }
        }

        /// <summary>
        /// Gets or sets the current speed of the npc
        /// </summary>
        public override short CurrentSpeed
        {
            set
            {
                SaveCurrentPosition();

                if (base.CurrentSpeed != value)
                {
                    base.CurrentSpeed = value;
                    BroadcastUpdate();
                }
            }
        }

        /// <summary>
        /// Stores the currentwaypoint that npc has to wander to
        /// </summary>
        protected PathPoint m_currentWayPoint = null;

        /// <summary>
        /// Gets sets the speed for traveling on path
        /// </summary>
        public short PathingNormalSpeed
        {
            get { return m_pathingNormalSpeed; }
            set { m_pathingNormalSpeed = value; }
        }
        /// <summary>
        /// Stores the speed for traveling on path
        /// </summary>
        protected short m_pathingNormalSpeed;

        /// <summary>
        /// Gets the current X of this living. Don't modify this property
        /// to try to change position of the mob while active. Use the
        /// MoveTo function instead
        /// </summary>
        public override int X
        {
            get
            {
                if (!IsMoving)
                    return base.X;

                if (TargetPosition.X != 0 || TargetPosition.Y != 0 || TargetPosition.Z != 0)
                {
                    long expectedDistance = FastMath.Abs((long)TargetPosition.X - m_x);

                    if (expectedDistance == 0)
                        return TargetPosition.X;

                    long actualDistance = FastMath.Abs((long)(MovementElapsedTicks * TickSpeedX));

                    if (expectedDistance - actualDistance < 0)
                        return TargetPosition.X;
                }

                return base.X;
            }
        }

        /// <summary>
        /// Gets the current Y of this NPC. Don't modify this property
        /// to try to change position of the mob while active. Use the
        /// MoveTo function instead
        /// </summary>
        public override int Y
        {
            get
            {
                if (!IsMoving)
                    return base.Y;

                if (TargetPosition.X != 0 || TargetPosition.Y != 0 || TargetPosition.Z != 0)
                {
                    long expectedDistance = FastMath.Abs((long)TargetPosition.Y - m_y);

                    if (expectedDistance == 0)
                        return TargetPosition.Y;

                    long actualDistance = FastMath.Abs((long)(MovementElapsedTicks * TickSpeedY));

                    if (expectedDistance - actualDistance < 0)
                        return TargetPosition.Y;
                }
                return base.Y;
            }
        }

        /// <summary>
        /// Gets the current Z of this NPC. Don't modify this property
        /// to try to change position of the mob while active. Use the
        /// MoveTo function instead
        /// </summary>
        public override int Z
        {
            get
            {
                if (!IsMoving)
                    return base.Z;

                if (TargetPosition.X != 0 || TargetPosition.Y != 0 || TargetPosition.Z != 0)
                {
                    long expectedDistance = FastMath.Abs((long)TargetPosition.Z - m_z);

                    if (expectedDistance == 0)
                        return TargetPosition.Z;

                    long actualDistance = FastMath.Abs((long)(MovementElapsedTicks * TickSpeedZ));

                    if (expectedDistance - actualDistance < 0)
                        return TargetPosition.Z;
                }
                return base.Z;
            }
        }







        /// <summary>
        /// The stealth state of this NPC
        /// </summary>
        public override bool IsStealthed
        {
            get
            {
                return (Flags & eFlags.STEALTH) != 0;
            }
        }

        protected int m_maxdistance;
        /// <summary>
        /// The Mob's max distance from its spawn before return automatically
        /// if MaxDistance > 0 ... the amount is the normal value
        /// if MaxDistance = 0 ... no maxdistance check
        /// if MaxDistance less than 0 ... the amount is calculated in procent of the value and the aggrorange (in StandardMobBrain)
        /// </summary>
        public int MaxDistance
        {
            get { return m_maxdistance; }
            set { m_maxdistance = value; }
        }

        protected int m_roamingRange;
        /// <summary>
        /// radius for roaming
        /// </summary>
        public int RoamingRange
        {
            get { return m_roamingRange; }
            set { m_roamingRange = value; }
        }

        protected int m_tetherRange;

        /// <summary>
        /// The mob's tether range; if mob is pulled farther than this distance
        /// it will return to its spawn point.
        /// if TetherRange > 0 ... the amount is the normal value
        /// if TetherRange less or equal 0 ... no tether check
        /// </summary>
        public int TetherRange
        {
            get { return m_tetherRange; }
            set { m_tetherRange = value; }
        }

        protected string m_mobstyleid;

        public string MobStyleID
        {
            get { return m_mobstyleid; }
            set { m_mobstyleid = value; }
        }
        /// <summary>
        /// True, if NPC is out of tether range, false otherwise; if no tether
        /// range is specified, this will always return false.
        /// </summary>
        public bool IsOutOfTetherRange
        {
            get
            {
                if (TetherRange > 0)
                {
                    if (this.IsWithinRadius(this.SpawnPoint, TetherRange))
                        return false;
                    else
                        return true;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion

        #region Movement
        /// <summary>
        /// Timer to be set if an OnArriveAtTarget
        /// handler is set before calling the WalkTo function
        /// </summary>
        protected ArriveAtTargetAction m_arriveAtTargetAction;

        /// <summary>
		/// Is the mob roaming towards a target?
		/// </summary>
		public bool IsRoaming
        {
            get
            {
                return m_arriveAtTargetAction != null && m_arriveAtTargetAction.IsAlive;
            }
        }

        /// <summary>
        /// Timer to be set if an OnCloseToTarget
        /// handler is set before calling the WalkTo function
        /// </summary>
        //protected CloseToTargetAction m_closeToTargetAction;
        /// <summary>
        /// Object that this npc is following as weakreference
        /// </summary>
        protected WeakReference m_followTarget;
        /// <summary>
        /// Max range to keep following
        /// </summary>
        protected int m_followMaxDist;
        /// <summary>
        /// Min range to keep to the target
        /// </summary>
        protected int m_followMinDist;
        /// <summary>
        /// Timer with purpose of follow updating
        /// </summary>
        protected NPCRegionTimer m_followTimer;
        /// <summary>
        /// Property entry on follow timer, wether the follow target is in range
        /// </summary>
        protected static readonly string FOLLOW_TARGET_IN_RANGE = "FollowTargetInRange";
        /// <summary>
        /// Minimum allowed attacker follow distance to avoid issues with client / server resolution (herky jerky motion)
        /// </summary>
        protected static readonly int MIN_ALLOWED_FOLLOW_DISTANCE = 100;

        /// <summary>
        /// Maximum allowed pet follow distance
        /// </summary>
        protected static readonly int MAX_ALLOWED_PET_FOLLOW_DISTANCE = 105;

        /// <summary>
        /// Minimum allowed pet follow distance
        /// </summary>
        protected static readonly int MIN_ALLOWED_PET_FOLLOW_DISTANCE = 90;
        /// <summary>
        /// At what health percent will npc give up range attack and rush the attacker
        /// </summary>
        protected const int MINHEALTHPERCENTFORRANGEDATTACK = 90;
        protected const int MINHEALTHPERCENTFORRANGEDATTACKRANGED = 98;
        private string m_pathID;
        public string PathID
        {
            get { return m_pathID; }
            set { m_pathID = value; }
        }

        private IPoint3D m_targetPosition = new Point3D(0, 0, 0);

        /// <summary>
        /// The target position.
        /// </summary>
        public virtual IPoint3D TargetPosition
        {
            get
            {
                return m_targetPosition;
            }

            protected set
            {
                if (value != m_targetPosition)
                {
                    SaveCurrentPosition();
                    m_targetPosition = value;
                }
            }
        }

        /// <summary>
        /// The target object.
        /// </summary>
        public override GameObject TargetObject
        {
            get
            {
                return base.TargetObject;
            }
            set
            {
                GameObject previousTarget = TargetObject;
                GameObject newTarget = value;

                base.TargetObject = newTarget;

                if (previousTarget != null && newTarget != previousTarget)
                    previousTarget.Notify(GameNPCEvent.SwitchedTarget, this,
                                          new SwitchedTargetEventArgs(previousTarget, newTarget));
            }
        }

        /// <summary>
        /// Updates the tick speed for this living.
        /// </summary>
        protected override void UpdateTickSpeed()
        {
            if (!IsMoving)
            {
                SetTickSpeed(0, 0, 0);
                return;
            }

            if (TargetPosition.X != 0 || TargetPosition.Y != 0 || TargetPosition.Z != 0)
            {
                double dist = this.GetDistanceTo(new Point3D(TargetPosition.X, TargetPosition.Y, TargetPosition.Z));

                if (dist <= 0)
                {
                    SetTickSpeed(0, 0, 0);
                    return;
                }

                double dx = (double)(TargetPosition.X - m_x) / dist;
                double dy = (double)(TargetPosition.Y - m_y) / dist;
                double dz = (double)(TargetPosition.Z - m_z) / dist;

                SetTickSpeed(dx, dy, dz, CurrentSpeed);
                return;
            }

            base.UpdateTickSpeed();
        }

       
        /// <summary>
        /// True if the mob is at its target position, else false.
        /// </summary>
        public bool IsAtTargetPosition
        {
            get
            {
                return (X == TargetPosition.X && Y == TargetPosition.Y && Z == TargetPosition.Z);
            }
        }

        /// <summary>
        /// Turns the npc towards a specific spot
        /// </summary>
        /// <param name="tx">Target X</param>
        /// <param name="ty">Target Y</param>
        public virtual void TurnTo(int tx, int ty)
        {
            TurnTo(tx, ty, true);
        }

        /// <summary>
        /// Turns the npc towards a specific spot
        /// optionally sends update to client
        /// </summary>
        /// <param name="tx">Target X</param>
        /// <param name="ty">Target Y</param>
        public virtual void TurnTo(int tx, int ty, bool sendUpdate)
        {
            if (IsStunned || IsMezzed) return;

            Notify(GameNPCEvent.TurnTo, this, new TurnToEventArgs(tx, ty));

            if (sendUpdate)
                Heading = GetHeading(new Point2D(tx, ty));
            else
                base.Heading = GetHeading(new Point2D(tx, ty));
        }

        /// <summary>
        /// Turns the npc towards a specific heading
        /// </summary>
        /// <param name="newHeading">the new heading</param>
        public virtual void TurnTo(ushort heading)
        {
            TurnTo(heading, true);
        }

        /// <summary>
        /// Turns the npc towards a specific heading
        /// optionally sends update to client
        /// </summary>
        /// <param name="newHeading">the new heading</param>
        public virtual void TurnTo(ushort heading, bool sendUpdate)
        {
            if (IsStunned || IsMezzed) return;

            Notify(GameNPCEvent.TurnToHeading, this, new TurnToHeadingEventArgs(heading));

            if (sendUpdate)
                if (Heading != heading) Heading = heading;
                else
                    if (base.Heading != heading) base.Heading = heading;
        }

        /// <summary>
        /// Turns the NPC towards a specific gameObject
        /// which can be anything ... a player, item, mob, npc ...
        /// </summary>
        /// <param name="target">GameObject to turn towards</param>
        public virtual void TurnTo(GameObject target)
        {
            if (target == null) // Null-Überprüfung hinzugefügt
            {
                //log.Warn("Zielobjekt ist null, kann nicht gedreht werden.");
                return;
            }

            TurnTo(target, true);
        }

        /// <summary>
        /// Turns the NPC towards a specific gameObject
        /// which can be anything ... a player, item, mob, npc ...
        /// optionally sends update to client
        /// </summary>
        /// <param name="target">GameObject to turn towards</param>
        public virtual void TurnTo(GameObject target, bool sendUpdate)
        {
            if (target == null || target != null && target.IsObjectAlive == false || target.CurrentRegion != CurrentRegion)
                return;

            TurnTo(target.X, target.Y, sendUpdate);
        }

        /// <summary>
        /// Turns the NPC towards a specific gameObject
        /// which can be anything ... a player, item, mob, npc ...
        /// and turn back after specified duration
        /// </summary>
        /// <param name="target">GameObject to turn towards</param>
        /// <param name="duration">restore heading after this duration</param>
        public virtual void TurnTo(GameObject target, int duration)
        {
            if (target == null || target != null && target.IsObjectAlive == false || target.CurrentRegion != CurrentRegion)
                return;

            // Store original heading if not set already.

            RestoreHeadingAction restore = (RestoreHeadingAction)TempProperties.getProperty<object>(RESTORE_HEADING_ACTION_PROP, null);

            if (restore == null)
            {
                restore = new RestoreHeadingAction(this);
                TempProperties.setProperty(RESTORE_HEADING_ACTION_PROP, restore);
            }

            TurnTo(target);
            restore.Start(duration);
        }

        /// <summary>
        /// Turns the NPC towards a specific gameObject
        /// which can be anything ... a player, item, mob, npc ...
        /// and turn back after specified duration
        /// </summary>
        /// <param name="target">GameObject to turn towards</param>
        /// <param name="duration">restore heading after this duration</param>
        public virtual void TurnToXY(int x, int y, int duration, bool sendUpdate, GameNPC npc, bool emote)
        {
            if (x == 0 || y == 0)
                return;

            // Store original heading if not set already.

            RestoreHeadingAction restore = (RestoreHeadingAction)TempProperties.getProperty<object>(RESTORE_HEADING_ACTION_PROP, null);

            if (restore == null)
            {
                restore = new RestoreHeadingAction(this);
                TempProperties.setProperty(RESTORE_HEADING_ACTION_PROP, restore);
            }

            TurnTo(x, y, sendUpdate);
            if (npc != null && emote)
            {

                npc.Emote(eEmote.Point);
            }
            restore.Start(duration);
        }
        /// <summary>
        /// The property used to store the NPC heading restore action
        /// </summary>
        protected const string RESTORE_HEADING_ACTION_PROP = "NpcRestoreHeadingAction";

        /// <summary>
        /// Restores the NPC heading after some time
        /// </summary>
        protected class RestoreHeadingAction : RegionAction
        {
            /// <summary>
            /// The NPCs old heading
            /// </summary>
            protected readonly ushort m_oldHeading;

            /// <summary>
            /// The NPCs old position
            /// </summary>
            protected readonly Point3D m_oldPosition;

            /// <summary>
            /// Creates a new TurnBackAction
            /// </summary>
            /// <param name="actionSource">The source of action</param>
            public RestoreHeadingAction(GameNPC actionSource)
                : base(actionSource)
            {
                m_oldHeading = actionSource.Heading;
                m_oldPosition = new Point3D(actionSource);
            }

            /// <summary>
            /// Called on every timer tick
            /// </summary>
            protected override void OnTick()
            {
                GameNPC npc = (GameNPC)m_actionSource;

                npc.TempProperties.removeProperty(RESTORE_HEADING_ACTION_PROP);

                if (npc.ObjectState != eObjectState.Active) return;
                if (!npc.IsAlive) return;
                if (npc.AttackState) return;
                if (npc.IsMoving) return;
                if (npc.Equals(m_oldPosition)) return;
                if (npc.Heading == m_oldHeading) return; // already set? oO

                npc.TurnTo(m_oldHeading);
            }
        }

        /// <summary>
        /// Gets the last time this mob was updated
        /// </summary>
        public uint LastUpdateTickCount
        {
            get { return m_lastUpdateTickCount; }
        }

        /// <summary>
        /// Gets the last this this NPC was actually update to at least one player.
        /// </summary>
        public uint LastVisibleToPlayersTickCount
        {
            get { return m_lastVisibleToPlayerTick; }
        }


        /// <summary>
        /// Delayed action that fires an event when an NPC arrives at its target
        /// </summary>
        protected class ArriveAtTargetAction : RegionAction
        {
            private Action<GameNPC> m_goToNodeCallback;

            /// <summary>
            /// Constructs a new ArriveAtTargetAction
            /// </summary>
            /// <param name="actionSource">The action source</param>
            public ArriveAtTargetAction(GameNPC actionSource, Action<GameNPC> goToNodeCallback = null)
                : base(actionSource)
            {
                m_goToNodeCallback = goToNodeCallback;
            }

            /// <summary>
            /// This function is called when the Mob arrives at its target spot
            /// This time was estimated using walking speed and distance.
            /// It fires the ArriveAtTarget event
            /// </summary>
            protected override void OnTick()
            {

                GameNPC npc = (GameNPC)m_actionSource;

                if (m_goToNodeCallback != null)
                {
                    m_goToNodeCallback(npc);
                    return;
                }

                bool arriveAtSpawnPoint = npc.IsReturningToSpawnPoint;




                npc.StopMoving();
                npc.Notify(GameNPCEvent.ArriveAtTarget, npc);

                if (arriveAtSpawnPoint)
                    npc.Notify(GameNPCEvent.ArriveAtSpawnPoint, npc);


            }
        }
        public virtual void StartWalkToTimer(int requiredTicks)
        {
            if (m_arriveAtTargetAction == null)
            {
                m_arriveAtTargetAction = new ArriveAtTargetAction(this);
                m_arriveAtTargetAction.Start((requiredTicks > 1) ? requiredTicks : 1);

            }
        }
        public virtual void CancelWalkToTimer()
        {
            if (IsMoving && m_arriveAtTargetAction != null && m_arriveAtTargetAction.IsAlive)
            {

                m_arriveAtTargetAction?.Stop();
                m_arriveAtTargetAction = null;
            }
           
        }

        /// <summary>
        /// Ticks required to arrive at a given spot.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        private int GetTicksToArriveAt(IPoint3D target, int speed)
        {
            return GetDistanceTo(target) * 1000 / speed;
        }

        /// <summary>
        /// Make the current (calculated) position permanent.
        /// </summary>
        private void SaveCurrentPosition()
        {
            if (this is IControlledBrain == false)
            {
                SavePosition(this);
            }
        }

        /// <summary>
        /// Make the target position permanent.
        /// </summary>
        private void SavePosition(IPoint3D target)
        {
            X = target.X;
            Y = target.Y;
            Z = target.Z;

            MovementStartTick = GameTimer.GetTickCount();
        }


        /// <summary>
        /// Walk to a certain spot at a given speed.
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="ty"></param>
        /// <param name="tz"></param>
        /// <param name="speed"></param>
        [Obsolete("Use .PathTo instead")]
        public virtual void WalkTo(int targetX, int targetY, int targetZ, short speed)
        {

            WalkTo(new Point3D(targetX, targetY, targetZ), speed);
        }
        ///private IPoint3D newPosition = new Point3D(0, 0, 0);
        /// <summary>
        /// Walk to a certain spot at a given speed.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="speed"></param>
        public virtual void WalkTo(IPoint3D target, short speed)
        {
            if (IsTurningDisabled)
                return;
            if (IsMezzed || IsStunned)
                return;

            if (speed > MaxSpeed)
                speed = MaxSpeed;

            if (speed <= 0)
                return;

            //KeepGuard Patrol formation
            if (this is GameKeepGuard && (this as GameKeepGuard).PatrolGroup != null)
            {
                int offX = 0; int offY = 0;
                if ((this as GameKeepGuard).InCombat == false && (this as GameKeepGuard).IsMovingOnPath && (this as GameKeepGuard).PatrolGroup != null)
                    (this as GameKeepGuard).PatrolGroup.GetMovementOffset((this as GameKeepGuard), out offX, out offY);

                TargetPosition = (new Point3D(target.X - offX, target.Y - offY, target.Z));  // this also saves the current position


            }
            else
                TargetPosition = target; // this also saves the current position




            if (IsWithinRadius(TargetPosition, CONST_WALKTOTOLERANCE))
            {
                // No need to start walking.

                Notify(GameNPCEvent.ArriveAtTarget, this);
                return;
            }

            CancelWalkToTimer();

            m_Heading = GetHeading(TargetPosition);
            m_currentSpeed = speed;

            UpdateTickSpeed();
            Notify(GameNPCEvent.WalkTo, this, new WalkToEventArgs(TargetPosition, speed));

            StartArriveAtTargetAction(GetTicksToArriveAt(TargetPosition, speed), null);
            BroadcastUpdate();
        }

        private void StartArriveAtTargetAction(int requiredTicks, Action<GameNPC> goToNextNodeCallback)
        {
            m_arriveAtTargetAction = new ArriveAtTargetAction(this, goToNextNodeCallback);
            m_arriveAtTargetAction.Start((requiredTicks > 1) ? requiredTicks : 1);
        }

        /// <summary>
        /// Walk to the spawn point
        /// </summary>

        public virtual void WalkToSpawn()
        {
            WalkToSpawn((short)(55));
        }

        /// <summary>
        /// Walk to the spawn point
        /// </summary>
        public virtual void CancelWalkToSpawn()
        {
            CancelWalkToTimer();
            IsReturningHome = false;
            IsReturningToSpawnPoint = false;

        }

        /// <summary>
        /// Walk to the spawn point with specified speed
        /// </summary>
        public virtual Task<bool> WalkToSpawn(short speed)
        {
            StopAttackReturningHome();
            //StopAttack();
            StopFollowing();

            StandardMobBrain brain = Brain as StandardMobBrain;


            if (brain != null) // && ((brain.AggroLevel < 1 || TargetObject != null && IsWithinRadius(TargetObject, brain.AggroRange) == false) && brain.HasAggro))
            {
                brain.ClearAggroList();

                if (InCombat == false)
                    Health = MaxHealth;
            }

            TargetObject = null;

            //IsReturningHome = true;
            IsReturningToSpawnPoint = true;

            //PathTo(SpawnPoint, speed);
            return PathTo(SpawnPoint.X, SpawnPoint.Y, SpawnPoint.Z, speed);


        }

        /// <summary>
        /// Check if NPC is back to his Spawnpoint
        /// </summary>
        /// <param name="NPC"></param>
        public void GetNPCBackHome(GameNPC NPC)
        {
            int distance = NPC.GetDistanceTo(NPC.SpawnPoint);

            if (NPC.InCombat == false && distance <= 50 && NPC.TempProperties.getProperty<GameNPC>(StandardMobBrain.LastTimeAggros) != null)
            {
                IsReturningHome = true;
                //NPC.TempProperties.removeProperty(StandardMobBrain.LastTimeAggros);
                //log.Error("returning home1!!! propertie removed");
            }
        }

        /// <summary>
        /// This function is used to start the mob walking. It will
        /// walk in the heading direction until the StopMovement function
        /// is called
        /// </summary>
        /// <param name="speed">walk speed</param>
        [Obsolete("fundamentally does not work well with pathing; avoid if possible")]
        public virtual void Walk(short speed)
        {
            Notify(GameNPCEvent.Walk, this, new WalkEventArgs(speed));

            CancelWalkToTimer();
            SaveCurrentPosition();
            TargetPosition.Clear();

            m_currentSpeed = speed;

            MovementStartTick = GameTimer.GetTickCount();
            UpdateTickSpeed();
            BroadcastUpdate();
        }


        #region Pathing

        /// <summary>
		/// Helper component for efficiently calculating paths
		/// </summary>
		public PathCalculator PathCalculator { get; protected set; } // Only visible for debugging


        /// <summary>
        /// Finds a valid path to the destination (or picks the direct path otherwise). Uses WalkTo for each of the pathing nodes.
        /// </summary>
        /// <returns>true if a path was found</returns>
        public Task<bool> PathTo(float destX, float destY, float destZ, short? speed = null, Action<GameNPC> onLastNodeReached = null)
        {
            //new ConfiguredTaskAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext)
            Vector3 bodyPosition = new Vector3(destX, destY, destZ);
            //onLastNodeReached.ConfigureAwait(false); //onLastNodeReached(this);
            return PathTo(bodyPosition, speed, onLastNodeReached);
        }

        public static short Clamp(short value, short min, short max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        /// <summary>
        /// Finds a valid path to the destination (or picks the direct path otherwise). Uses WalkTo for each of the pathing nodes.
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="speed"></param>
        /// <returns>true if a path was found</returns>
        public async Task<bool> PathTo(Vector3 dest, short? speed = null, Action<GameNPC> onLastNodeReached = null)
        {
            if (dest.Equals(new Vector3(X, Y, Z))) return true;
            if (IsTurningDisabled) return false;

            short walkSpeed = speed ?? MaxSpeed;
            walkSpeed = Clamp(walkSpeed, (short)0, MaxSpeed);
            if (walkSpeed <= 0) return false;

            Vector3 targetPos = dest;
            if (this is GameKeepGuard guard && guard.PatrolGroup != null)
            {
                int offX = 0, offY = 0;
                if (!guard.InCombat && guard.IsMovingOnPath)
                    guard.PatrolGroup.GetMovementOffset(guard, out offX, out offY);

                TargetPosition = new Point3D((int)targetPos.X - offX, (int)targetPos.Y - offY, (int)targetPos.Z);
            }
            else
            {
                dest = targetPos;
            }

            DebugSend("PathTo({0}, {1})", dest, walkSpeed);
            Interlocked.Increment(ref Statistics.PathToCalls);

            if (PathCalculator == null && PathCalculator.IsSupported(this))
            {
                PathCalculator = new PathCalculator(this)
                {
                    VisualizePath = ServerProperties.Properties.ENABLE_Pathig_Visual_DEBUG
                };
            }

            Vector3? nextNode = null;
            bool didFindPath = false;
            bool shouldUseAirPath = true;

            if (PathCalculator != null)
            {
                var result = await PathCalculator.CalculateNextTargetAsync(dest);
                nextNode = result.Item1;
                shouldUseAirPath = result.Item2 == NoPathReason.RECAST_FOUND_NO_PATH;
                didFindPath = PathCalculator.DidFindPath;
            }

            if (!nextNode.HasValue)
            {
                onLastNodeReached?.Invoke(this); // Custom action, e.g. used to start the follow timer
                WalkTo((int)dest.X, (int)dest.Y, (int)dest.Z, walkSpeed);
                return false;
            }

            IPoint3D point = new Point3D((int)dest.X, (int)dest.Y, (int)dest.Z);
            Notify(GameNPCEvent.WalkTo, this, new WalkToEventArgs(point, walkSpeed));

            WalkToPathNode(nextNode.Value, walkSpeed, npc => npc.PathTo(dest, speed, onLastNodeReached).ConfigureAwait(false));
            return true;
        }


        private void WalkToPathNode(Vector3 node, short speed, Action<GameNPC> goToNextNodeCallback)
        {
            if (IsTurningDisabled)
                return;

            if (speed > MaxSpeed)
                speed = MaxSpeed;

            if (speed <= 0)
                return;

            //IPoint3D point = new Point3D((int)node.X, (int)node.Y, (int)node.Z);
            TargetPosition = new Point3D((int)node.X, (int)node.Y, (int)node.Z); ; // this also saves the current position



            if (IsWithinRadius(TargetPosition, 5))
            {
                goToNextNodeCallback(this);
                return;
            }

            CancelWalkToTimer();

            m_Heading = GetHeading(TargetPosition);
            m_currentSpeed = speed;

            UpdateTickSpeed();
            StartArriveAtTargetAction(GetTicksToArriveAt(TargetPosition, speed), goToNextNodeCallback);
            BroadcastUpdate();
        }

        /// <summary>
        /// Clears all remaining elements in our pathing cache.
        /// </summary>
        public void ClearPathingCache()
        {
            PathCalculator?.Clear();
        }


        #endregion



        /// <summary>
        /// Gets the NPC current follow target
        /// </summary>
        public GameObject CurrentFollowTarget
        {
            get { return m_followTarget.Target as GameObject; }
        }
        /// <summary>
        /// Starts the movement of the NPC.
        /// </summary>
        public virtual void StartMoving()
        {

            if (IsMezzed == false && IsStunned == false)
            {
                StartWalkToTimer(1);
                //CancelWalkToSpawn();

                if (!IsMoving)
                    CurrentSpeed = 50;
            }
        }
        /// <summary>
        /// Stops the movement of the NPC.
        /// </summary>
        public virtual void StopMoving()
        {
            CancelWalkToSpawn();

            if (IsMoving)
                CurrentSpeed = 0;
        }

        /// <summary>
        /// Stops the movement of the mob and forcibly moves it to the
        /// given target position.
        /// </summary>
        public virtual void StopMovingAt(IPoint3D target)
        {
            CancelWalkToSpawn();

            if (IsMoving)
            {
                m_currentSpeed = 0;
                UpdateTickSpeed();
            }

            SavePosition(target);
            BroadcastUpdate();
        }



        /// <summary>
        /// Npc Follow los checks
        /// </summary>
        /// <param name="player"></param>
        /// <param name="response"></param>
        /// <param name="targetOID"></param>
        public void WalkToCheckLOS(GameLiving player, ushort response, ushort targetOID)
        {
            if (player == null) // Überprüfung hinzugefügt
            {
                //log.Warn("Spieler ist null, kann keine LOS prüfen.");
                return;
            }

            GameLiving target = this.CurrentRegion.GetObject(targetOID) as GameLiving;

            if ((response & 0x100) == 0x100 && target != null && target.ObjectState == GameLiving.eObjectState.Active)
            {
                AllowedToAttackTarget();
                player.TempProperties.removeProperty(LOSEFFECTIVENESSNpcs + target.ObjectID);
            }
            else
            {
                // this.BodyType == 1577 = Archer
                if (!this.LoadedFromScript && this is GameNPC npc && !(this is GameKeepGuard) && this.TargetObject != null
                    && !(this is GameMine) && !(this is GameSiegeWeapon)
                    && (this.HealthPercent > 98 || this.ActiveWeaponSlot == eActiveWeaponSlot.Distance || this.IsCasting))
                {
                    HandleNonPatrollingNPC(npc);
                    return;
                }

                if (this is GameKeepGuard guard && (this is GuardArcher == false && this is GuardLord == false || guard.PatrolGroup != null)
                    && this.TargetObject != null && (this.ActiveWeaponSlot == eActiveWeaponSlot.Distance || this.HealthPercent > 90 || this.IsCasting || this.LoadedFromScript))
                {
                    HandleKeepGuard(guard);
                    return;
                }
            }
        }

        private void HandleNonPatrollingNPC(GameNPC npc)
        {
            if (this.TargetObject != null)
            {
                npc.RemoveAttacker(npc.TargetObject);
            }
            npc.StopAttack();
            npc.WalkToSpawn();
            if (npc.Heading != npc.SpawnHeading)
                npc.Heading = npc.SpawnHeading;
        }

        private void HandleKeepGuard(GameKeepGuard guard)
        {
            if (guard.LoadedFromScript)
            {
                guard.StopAttack();
                guard.MoveOnPath(225);
                return;
            }

            guard.WalkToSpawn(300);
            if (guard.Heading != guard.SpawnHeading)
                guard.Heading = guard.SpawnHeading;
        }



        public virtual void CanFollow(GameObject target, GameLiving attacker)
        {
            if (target == null)
                return;


            if (target is GameLiving == false)
                return;


            GamePlayer thisLiving = null;
            if (target is GamePlayer)
                thisLiving = (GamePlayer)target;

            if (target is GameNPC && (target as GameNPC).Brain is MobPetBrain == false && (target as GameNPC).Brain is BossPetBrain == false && ((target as GameNPC).Brain is ControlledNpcBrain || (target as GameNPC).Brain is TheurgistPetBrain || (target as GameNPC).Brain is HoundsPetBrain || (target as GameNPC).Brain is ProcPetBrain))
            {
                IControlledBrain brain = ((GameNPC)target).Brain as IControlledBrain;
                if (brain != null)
                    thisLiving = brain.GetPlayerOwner();
            }

            if (thisLiving != null && thisLiving is GamePlayer && attacker != null)
            {

                thisLiving.TempProperties.setProperty(LOSEFFECTIVENESSNpcs + thisLiving.ObjectID, 1.0);
                (thisLiving as GamePlayer).Out.SendCheckLOS(attacker, (thisLiving as GamePlayer), new CheckLOSResponse(WalkToCheckLOS));
                return;
            }
        }

        /// <summary>
        ///Can we attack if we have los ?
        /// </summary>
        /// <returns></returns>
        public virtual bool AllowedToAttackTarget()
        {

            return true;
        }

        /// <summary>
        /// Follow given object
        /// </summary>
        /// <param name="target">Target to follow</param>
        /// <param name="minDistance">Min distance to keep to the target</param>
        /// <param name="maxDistance">Max distance to keep following</param>
        public virtual void Follow(GameObject target, int minDistance, int maxDistance)
        {
            if (m_followTimer.IsAlive)
                m_followTimer.Stop();
            GameLiving living = target as GameLiving;





            if (target == null || target.ObjectState != eObjectState.Active || target != null && target.IsObjectAlive == false)
                return;

            if (target != null && Brain != null && Brain is ControlledNpcBrain && (Brain as ControlledNpcBrain).WalkState == eWalkState.Stay)
                return;


            //Guard
            if (this is GuardStealther && living is GamePlayer && (living as GamePlayer).IsStealthed && (living as GamePlayer).EffectList.GetOfType<CamouflageEffect>() != null)
            {
                if (this is GuardStealther && (this as GuardStealther).IsAttacking)
                {
                    (this as GuardStealther).StopAttack();
                    (this as GuardStealther).WalkToSpawn();
                }
                return;
            }
            //pets
            if (this.Brain is ControlledNpcBrain && living is GamePlayer && (living as GamePlayer).IsStealthed && living.Realm != this.Realm)
            {
                if (this is GameNPC && (this as GameNPC).IsAttacking)
                {
                    (this as GameNPC).StopAttack();
                }
                return;
            }


            m_followMaxDist = maxDistance;
            m_followMinDist = minDistance;

            m_followTarget.Target = target;
            m_followTimer.Start(100);
        }


        /// <summary>
        /// Stop following
        /// </summary>
        public virtual void StopFollowing()
        {
            lock (m_followTimer)
            {
                if (m_followTimer.IsAlive)
                    m_followTimer.Stop();

                m_followTarget.Target = null;
                StopMoving();
            }
        }

        /// <summary>
        /// Will be called if follow mode is active
        /// and we reached the follow target
        /// </summary>
        public virtual void FollowTargetInRange()
        {
            if (AttackState && TargetObject != null && TargetObject.IsObjectAlive)
            {


                // if in last attack the enemy was out of range, we can attack him now immediately
                AttackData ad = (AttackData)TempProperties.getProperty<object>(LAST_ATTACK_DATA, null);

                if (ad != null && ad.AttackResult == eAttackResult.OutOfRange)
                {
                    m_attackAction.Start(1);// schedule for next tick
                }
            }
            //elcotek fix dauercasten beim Commander pet
            //this is CommanderPet == false && m_attackers.Count == 0
            else if (m_attackers.Count == 0 && this.Spells.Count > 0 && this.TargetObject != null && GameServer.ServerRules.IsAllowedToAttack(this, (this.TargetObject as GameLiving), true))
            {
                if (TargetObject.Realm == 0 || Realm == 0)
                    m_lastAttackTickPvE = m_CurrentRegion.Time;
                else m_lastAttackTickPvP = m_CurrentRegion.Time;
                if (this.CurrentRegion.Time - LastAttackedByEnemyTick > 10 * 1000)
                {
                    // Aredhel: Erm, checking for spells in a follow method, what did we create
                    // brain classes for again?

                    //Check for negatively casting spells

                    StandardMobBrain stanBrain = (StandardMobBrain)Brain;
                    if (stanBrain != null)
                        ((StandardMobBrain)stanBrain).CheckSpells(StandardMobBrain.eCheckSpellType.Offensive);
                }
            }
        }



        /// <summary>
        /// Keep following a specific object at a max distance
        /// </summary>
        protected virtual int FollowTimerCallback(NPCRegionTimer callingTimer)
        {
            short newspeed = MaxSpeed;
            int FollowCheckTime = ServerProperties.Properties.GAMENPC_FOLLOWCHECK_TIME;

            if (IsCasting)
                return ServerProperties.Properties.GAMENPC_FOLLOWCHECK_TIME;

            bool wasInRange = m_followTimer.Properties.getProperty(FOLLOW_TARGET_IN_RANGE, false);
            m_followTimer.Properties.removeProperty(FOLLOW_TARGET_IN_RANGE);

            GameObject followTarget = (GameObject)m_followTarget.Target;
            GameLiving followLiving = followTarget as GameLiving;

            //Stop following if target living is dead
            if (followLiving != null && !followLiving.IsAlive)
            {
                StopFollowing();
                Notify(GameNPCEvent.FollowLostTarget, this, new FollowLostTargetEventArgs(followTarget));
                return 0;
            }

            //Stop following if we have no target
            if (followTarget == null || followTarget.ObjectState != eObjectState.Active || followTarget != null && followTarget.IsObjectAlive == false || CurrentRegionID != followTarget.CurrentRegionID)
            {
                StopFollowing();
                Notify(GameNPCEvent.FollowLostTarget, this, new FollowLostTargetEventArgs(followTarget));
                return 0;
            }

            //Calculate the difference between our position and the players position
            float diffx = (long)followTarget.X - X;
            float diffy = (long)followTarget.Y - Y;
            float diffz = (long)followTarget.Z - Z;

            //SH: Removed Z checks when one of the two Z values is zero(on ground)
            //Tolakram: a Z of 0 does not indicate on the ground.  Z varies based on terrain  Removed 0 Z check
            float distance = (float)Math.Sqrt(diffx * diffx + diffy * diffy + diffz * diffz);

            //if distance is greater then the max follow distance, stop following and return home
            if ((int)distance > m_followMaxDist)
            {
                StopFollowing();
                Notify(GameNPCEvent.FollowLostTarget, this, new FollowLostTargetEventArgs(followTarget));
                this.WalkToSpawn();
                return 0;
            }
            int newX, newY, newZ;

            if (this.Brain is StandardMobBrain)
            {
                StandardMobBrain brain = this.Brain as StandardMobBrain;

                //if the npc hasn't hit or been hit in a while, stop following and return home
                if (!(Brain is IControlledBrain))
                {
                    if (AttackState && brain != null && followLiving != null)
                    {
                        long seconds = 20 + ((brain.GetAggroAmountForLiving(followLiving) / (MaxHealth + 1)) * 100);
                        long lastattacked = LastAttackTick;
                        long lasthit = LastAttackedByEnemyTick;
                        if (CurrentRegion.Time - lastattacked > seconds * 1000 && CurrentRegion.Time - lasthit > seconds * 1000)
                        {
                            //StopFollow();
                            Notify(GameNPCEvent.FollowLostTarget, this, new FollowLostTargetEventArgs(followTarget));
                            //brain.ClearAggroList();
                            this.WalkToSpawn();
                            return 0;
                        }
                    }
                }

                //If we're part of a formation, we can get out early.
                newX = followTarget.X;
                newY = followTarget.Y;
                newZ = followTarget.Z;


                //Pet speed
                IControlledBrain Petbrain = ((GameNPC)this).Brain as IControlledBrain;
                if (Petbrain != null)
                {
                    GameLiving owner = Petbrain.GetLivingOwner();
                    if (owner != null)
                    {
                        //Bote und Pferde
                        if (owner is GamePlayer && ((owner as GamePlayer).IsRiding || (owner as GamePlayer).Steed != null && (owner as GamePlayer).Steed is GameTaxiBoat) && owner == Petbrain.Body.CurrentFollowTarget)
                        {
                            //FollowCheckTime = 90;
                            Petbrain.Body.MaxSpeedBase = 5000;
                            newspeed = 5000;

                        }
                        else if (owner is GamePlayer && (owner as GamePlayer).IsRiding == false && owner == Petbrain.Body.CurrentFollowTarget || owner is GamePlayer && (owner as GamePlayer).IsRiding && Petbrain.Body.TargetObject != null && Petbrain.Body.TargetObject != owner)
                        {
                            if (Petbrain.Body.MaxSpeedBase == 5000)
                            {
                                Petbrain.Body.MaxSpeedBase = 248;
                            }

                        }

                        if (owner is GamePlayer && owner.IsMoving && owner.CurrentSpeed >= 191 && owner == Petbrain.Body.CurrentFollowTarget)
                        {
                            Point3D point = new Point3D(owner.X, owner.Y, owner.Z);


                            //if we use NF Boats
                            if ((owner as GamePlayer).Steed != null && (owner as GamePlayer).Steed is GameTaxiBoat)
                            {
                                newspeed = (owner as GamePlayer).Steed.CurrentSpeed;

                            }
                            else


                            if (Petbrain.Body.GetDistanceTo(point) > 100 && Petbrain.Body.CurrentSpeed <= Petbrain.Body.MaxSpeed)
                                newspeed = owner.CurrentSpeed++;
                            else
                                newspeed = owner.CurrentSpeed;


                        }
                        else if (owner.IsMoving && owner.IsAttacking == false && Petbrain.Body.InCombat == false && owner is GameNPC && owner.CurrentSpeed > 40 && owner == Petbrain.Body.CurrentFollowTarget)
                        {
                            newspeed = owner.CurrentSpeed;
                        }
                    }
                    else
                        // FollowCheckTime = ServerProperties.Properties.GAMENPC_FOLLOWCHECK_TIME;
                        newspeed = MaxSpeed;
                }


                if (brain.CheckFormation(ref newX, ref newY, ref newZ, ref newspeed))
                {
                    PathTo(newX, newY, (ushort)newZ, newspeed);


                    return FollowCheckTime;
                }
            }

            // Tolakram - Distances under 100 do not calculate correctly leading to the mob always being told to walkto
            int minAllowedFollowDistance = MIN_ALLOWED_FOLLOW_DISTANCE;

            // pets can follow closer.  need to implement /fdistance command to make this adjustable
            if (this.Brain is IControlledBrain)
                minAllowedFollowDistance = MIN_ALLOWED_PET_FOLLOW_DISTANCE;

            //Are we in range yet?
            if ((int)distance <= (m_followMinDist < minAllowedFollowDistance ? minAllowedFollowDistance : m_followMinDist))
            {
                StopMoving();
                TurnTo(followTarget);
                if (!wasInRange)
                {
                    m_followTimer.Properties.setProperty(FOLLOW_TARGET_IN_RANGE, true);
                    FollowTargetInRange();
                }
                return FollowCheckTime;
            }
            // follow on distance
            diffx = (diffx / distance) * m_followMinDist;
            diffy = (diffy / distance) * m_followMinDist;
            diffz = (diffz / distance) * m_followMinDist;

            //Subtract the offset from the target's position to get
            //our target position
            newX = (int)(followTarget.X - diffx);
            newY = (int)(followTarget.Y - diffy);
            newZ = (int)(followTarget.Z - diffz);
            PathTo(newX, newY, (ushort)newZ, newspeed);
            //WalkTo(new Point3D(newX, newY, (ushort)newZ, MaxSpeed));
            return FollowCheckTime;
        }

        /// <summary>
        /// Disables the turning for this living
        /// </summary>
        /// <param name="add"></param>
        public override void DisableTurning(bool add)
        {
            bool old = IsTurningDisabled;
            base.DisableTurning(add);
            if (old != IsTurningDisabled)
                BroadcastUpdate();
        }


        #endregion

        #region Path (Movement)
        /// <summary>
        /// Gets sets the currentwaypoint that npc has to wander to
        /// </summary>
        public PathPoint CurrentWayPoint
        {
            get { return m_currentWayPoint; }
            set { m_currentWayPoint = value; }
        }

        /// <summary>
        /// Is the NPC returning home, if so, we don't want it to think
        /// </summary>
        public bool IsReturningHome
        {
            get { return m_isReturningHome; }
            set { m_isReturningHome = value; }
        }

        /// <summary>
        /// Is the NPC returning home, if so, we don't want it to think
        /// </summary>
        public bool NpcIsAtHome
        {
            get { return m_npcIsAtHome; }
            set { m_npcIsAtHome = value; }
        }

        /// <summary>
        /// Is the NPC was swimming before aggro ?
        /// </summary>
        public bool IsNpcWasSwimming
        {
            get { return m_isNpcWasSwimming; }
            set { m_isNpcWasSwimming = value; }
        }


        protected bool m_isReturningHome = false;


        protected bool m_npcIsAtHome = false;

        protected bool m_isNpcWasSwimming = false;


        /// <summary>
        /// Whether or not the NPC is on its way back to the spawn point.
        /// [Aredhel: I decided to add this property in order not to mess
        /// with SMB and IsReturningHome. Also, to prevent outside classes
        /// from interfering the setter is now protected.]
        /// </summary>
        public bool IsReturningToSpawnPoint { get; protected set; }

        /// <summary>
        /// Gets if npc moving on path
        /// </summary>
        public bool IsMovingOnPath
        {
            get { return m_IsMovingOnPath; }
        }
        /// <summary>
        /// Stores if npc moving on path
        /// </summary>
        protected bool m_IsMovingOnPath = false;

        /// <summary>
        /// let the npc travel on its path
        /// </summary>
        /// <param name="speed">Speed on path</param>
        public void MoveOnPath(short speed)
        {
            if (this.InCombat == false)
            {

                if (IsMovingOnPath)
                    StopMovingOnPath();

                if (CurrentWayPoint == null)
                {
                    if (log.IsWarnEnabled)
                        log.Warn("No path to travel on for " + Name);
                    return;
                }

                PathingNormalSpeed = speed;

                if (this.IsWithinRadius(CurrentWayPoint, 100))
                {
                    // reaching a waypoint can start an ambient sentence
                    FireAmbientSentence(eAmbientTrigger.moving);

                    if (CurrentWayPoint.Type == ePathType.Path_Reverse && CurrentWayPoint.FiredFlag)
                        CurrentWayPoint = CurrentWayPoint.Prev;
                    else
                    {
                        if ((CurrentWayPoint.Type == ePathType.Loop) && (CurrentWayPoint.Next == null))
                            CurrentWayPoint = MovementMgr.FindFirstPathPoint(CurrentWayPoint);
                        else
                            CurrentWayPoint = CurrentWayPoint.Next;
                    }
                }

                if (CurrentWayPoint != null)
                {
                    GameEventMgr.AddHandler(this, GameNPCEvent.ArriveAtTarget, new DOLEventHandler(OnArriveAtWaypoint));
                    WalkTo(CurrentWayPoint.X, CurrentWayPoint.Y, CurrentWayPoint.Z, Math.Min(speed, (short)CurrentWayPoint.MaxSpeed));
                    m_IsMovingOnPath = true;
                    Notify(GameNPCEvent.PathMoveStarts, this);
                }
                else
                {
                    StopMovingOnPath();
                }
            }
        }

        /// <summary>
        /// Stop moving on path.
        /// </summary>
        public void StopMovingOnPath()
        {
            if (!IsMovingOnPath)
                return;

            GameEventMgr.RemoveHandler(this, GameNPCEvent.ArriveAtTarget, new DOLEventHandler(OnArriveAtWaypoint));
            Notify(GameNPCEvent.PathMoveEnds, this);
            m_IsMovingOnPath = false;
        }














        /// <summary>
        /// decides what to do on reached waypoint in path
        /// </summary>
        /// <param name="e"></param>
        /// <param name="n"></param>
        /// <param name="args"></param>
        protected void OnArriveAtWaypoint(DOLEvent e, object n, EventArgs args)
        {
            if (!IsMovingOnPath || n != this)
                return;

            if (CurrentWayPoint != null)
            {
                WaypointDelayAction waitTimer = new WaypointDelayAction(this);
                waitTimer.Start(Math.Max(1, CurrentWayPoint.WaitTime * 100));
            }
            else
                StopMovingOnPath();
        }

        /// <summary>
        /// Delays movement to the next waypoint
        /// </summary>
        protected class WaypointDelayAction : RegionAction
        {
            /// <summary>
            /// Constructs a new WaypointDelayAction
            /// </summary>
            /// <param name="actionSource"></param>
            public WaypointDelayAction(GameObject actionSource)
                : base(actionSource)
            {
            }

            /// <summary>
            /// Called on every timer tick
            /// </summary>
            protected override void OnTick()
            {
                GameNPC npc = (GameNPC)m_actionSource;
                if (!npc.IsMovingOnPath)
                    return;


                PathPoint oldPathPoint = npc.CurrentWayPoint;
                PathPoint nextPathPoint = npc.CurrentWayPoint.Next;
                if ((npc.CurrentWayPoint.Type == ePathType.Path_Reverse) && (npc.CurrentWayPoint.FiredFlag))
                    nextPathPoint = npc.CurrentWayPoint.Prev;

                if (nextPathPoint == null)
                {
                    switch (npc.CurrentWayPoint.Type)
                    {
                        case ePathType.Loop:
                            {
                                npc.CurrentWayPoint = MovementMgr.FindFirstPathPoint(npc.CurrentWayPoint);
                                npc.Notify(GameNPCEvent.PathMoveStarts, npc);
                                break;
                            }
                        case ePathType.Once:
                            npc.CurrentWayPoint = null;//to stop
                            break;
                        case ePathType.Path_Reverse://invert sens when go to end of path
                            if (oldPathPoint.FiredFlag)
                                npc.CurrentWayPoint = npc.CurrentWayPoint.Next;
                            else
                                npc.CurrentWayPoint = npc.CurrentWayPoint.Prev;
                            break;
                    }
                }
                else
                {
                    if ((npc.CurrentWayPoint.Type == ePathType.Path_Reverse) && (npc.CurrentWayPoint.FiredFlag))
                        npc.CurrentWayPoint = npc.CurrentWayPoint.Prev;
                    else
                        npc.CurrentWayPoint = npc.CurrentWayPoint.Next;
                }
                oldPathPoint.FiredFlag = !oldPathPoint.FiredFlag;

                if (npc.CurrentWayPoint != null)
                {
                    npc.PathTo(npc.CurrentWayPoint.X, npc.CurrentWayPoint.Y, npc.CurrentWayPoint.Z, (short)Math.Min(npc.PathingNormalSpeed, npc.CurrentWayPoint.MaxSpeed));
                }
                else
                {
                    npc.StopMovingOnPath();
                }
            }
        }


        #endregion

        #region Inventory/LoadfromDB
        private NpcTemplate m_npcTemplate;
        /// <summary>
        /// The NPC's template
        /// </summary>
        public NpcTemplate NPCTemplate
        {
            get { return m_npcTemplate; }
            set { m_npcTemplate = value; }
        }
        /// <summary>
        /// Loads the equipment template of this npc
        /// </summary>
        /// <param name="equipmentTemplateID">The template id</param>
        public virtual void LoadEquipmentTemplateFromDatabase(string equipmentTemplateID)
        {
            EquipmentTemplateID = equipmentTemplateID;
            if (EquipmentTemplateID != null && EquipmentTemplateID.Length > 0)
            {
                GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
                if (template.LoadFromDatabase(EquipmentTemplateID))
                {
                    m_inventory = template.CloseTemplate();
                }
                else
                {
                    //if (log.IsDebugEnabled)
                    //{
                    //    //log.Warn("Error loading NPC inventory: InventoryID="+EquipmentTemplateID+", NPC name="+Name+".");
                    //}
                }
                if (Inventory != null)
                {
                    //if the distance slot isnt empty we use that
                    //Seems to always
                    if (Inventory.GetItem(eInventorySlot.DistanceWeapon) != null)
                        SwitchWeapon(eActiveWeaponSlot.Distance);
                    else
                    {
                        InventoryItem twohand = Inventory.GetItem(eInventorySlot.TwoHandWeapon);
                        InventoryItem onehand = Inventory.GetItem(eInventorySlot.RightHandWeapon);

                        if (twohand != null && onehand != null)
                            //Let's add some random chance
                            SwitchWeapon(Util.Chance(50) ? eActiveWeaponSlot.TwoHanded : eActiveWeaponSlot.Standard);
                        else if (twohand != null)
                            //Hmm our right hand weapon may have been null
                            SwitchWeapon(eActiveWeaponSlot.TwoHanded);
                        else if (onehand != null)
                            //Hmm twohand was null lets default down here
                            SwitchWeapon(eActiveWeaponSlot.Standard);
                    }
                }
            }
        }

        private bool m_loadedFromScript = true;
        public bool LoadedFromScript
        {
            get { return m_loadedFromScript; }
            set { m_loadedFromScript = value; }
        }


        /// <summary>
        /// Load a npc from the npc template
        /// </summary>
        /// <param name="obj">template to load from</param>
        public override void LoadFromDatabase(DataObject obj)
        {

            if (obj == null) return;
            base.LoadFromDatabase(obj);
            if (!(obj is Mob)) return;
            m_loadedFromScript = false;
            Mob dbMob = (Mob)obj;
            INpcTemplate npcTemplate = NpcTemplateMgr.GetTemplate(dbMob.NPCTemplateID);

            if (npcTemplate != null && !npcTemplate.ReplaceMobValues)
            {
                LoadTemplate(npcTemplate);
            }
            TranslationId = dbMob.TranslationId;
            Name = dbMob.Name;
            Suffix = dbMob.Suffix;
            GuildName = dbMob.Guild;
            ExamineArticle = dbMob.ExamineArticle;
            MessageArticle = dbMob.MessageArticle;
            m_x = dbMob.X;
            m_y = dbMob.Y;
            m_z = dbMob.Z;
            m_Heading = (ushort)(dbMob.Heading & 0xFFF);
            m_maxSpeedBase = (short)dbMob.Speed;
            m_currentSpeed = 0;
            CurrentRegionID = dbMob.Region;
            Realm = (eRealm)dbMob.Realm;
            Model = dbMob.Model;
            Size = dbMob.Size;
            Level = dbMob.Level;	// health changes when GameNPC.Level changes
            Flags = (eFlags)dbMob.Flags;
            m_packageID = dbMob.PackageID;


            //Load Stats for Mobs
            if (npcTemplate != null && npcTemplate.Strength <= (short)(Properties.MOB_AUTOSET_STR_BASE + Level * Properties.MOB_AUTOSET_STR_MULTIPLIER))
            {
                Strength = (short)(Properties.MOB_AUTOSET_STR_BASE + Level * Properties.MOB_AUTOSET_STR_MULTIPLIER);
            }
            if (npcTemplate == null)
            {
                if (Strength <= (short)(Properties.MOB_AUTOSET_STR_BASE + Level * Properties.MOB_AUTOSET_STR_MULTIPLIER))
                    Strength = (short)(Properties.MOB_AUTOSET_STR_BASE + Level * Properties.MOB_AUTOSET_STR_MULTIPLIER);
                else
                {
                    Strength = (short)dbMob.Strength;
                }
            }

            if (npcTemplate != null && npcTemplate.Constitution <= (Properties.MOB_AUTOSET_CON_BASE + Level * Properties.MOB_AUTOSET_CON_MULTIPLIER))
            {
                Constitution = (short)(Properties.MOB_AUTOSET_CON_BASE + Level * Properties.MOB_AUTOSET_CON_MULTIPLIER);
            }
            if (npcTemplate == null)
            {
                if (Level > 5 && Constitution <= (Properties.MOB_AUTOSET_CON_BASE + Level * Properties.MOB_AUTOSET_CON_MULTIPLIER))
                    Constitution = (short)(Properties.MOB_AUTOSET_CON_BASE + Level * Properties.MOB_AUTOSET_CON_MULTIPLIER);
                else
                {
                    Constitution = (short)dbMob.Constitution;
                }
            }


            Dexterity = (short)dbMob.Dexterity;
            Quickness = (short)dbMob.Quickness;
            Intelligence = (short)dbMob.Intelligence;
            Piety = (short)dbMob.Piety;
            Charisma = (short)dbMob.Charisma;
            Empathy = (short)dbMob.Empathy;

            MeleeDamageType = (eDamageType)dbMob.MeleeDamageType;
            if (MeleeDamageType == 0)
            {
                MeleeDamageType = eDamageType.Slash;
            }
            m_activeWeaponSlot = eActiveWeaponSlot.Standard;
            ActiveQuiverSlot = eActiveQuiverSlot.None;

            m_faction = FactionMgr.GetFactionByID(dbMob.FactionID);

            LoadEquipmentTemplateFromDatabase(dbMob.EquipmentTemplateID);

            if (dbMob.RespawnInterval == -1)
            {
                dbMob.RespawnInterval = 0;
            }
            m_respawnInterval = dbMob.RespawnInterval * 1000;

            m_pathID = dbMob.PathID;

            if (dbMob.Brain != "")
            {
                try
                {
                    List<Assembly> asms = new List<Assembly>
                    {
                        typeof(GameServer).Assembly
                    };
                    asms.AddRange(ScriptMgr.Scripts);
                    ABrain brain = null;
                    foreach (Assembly asm in asms)
                    {
                        brain = (ABrain)asm.CreateInstance(dbMob.Brain, false);
                        if (brain != null)
                            break;
                    }
                    if (brain != null)
                        SetOwnBrain(brain);
                }
                catch
                {
                    log.ErrorFormat("GameNPC error in LoadFromDatabase: can not instantiate brain of type {0} for npc {1}, name = {2}.", dbMob.Brain, dbMob.ClassType, dbMob.Name);
                }
            }

            IOldAggressiveBrain aggroBrain = Brain as IOldAggressiveBrain;
            if (aggroBrain != null)
            {
                aggroBrain.AggroLevel = dbMob.AggroLevel;
                aggroBrain.AggroRange = dbMob.AggroRange;
                if (aggroBrain.AggroRange == Constants.USE_AUTOVALUES)
                {
                    if (Realm == eRealm.None)
                    {
                        aggroBrain.AggroRange = 400;
                        if (Name != Name.ToLower())
                        {
                            aggroBrain.AggroRange = 500;
                        }
                        if (CurrentRegion.IsDungeon)
                        {
                            aggroBrain.AggroRange = 300;
                        }
                    }
                    else
                    {
                        aggroBrain.AggroRange = 500;
                    }
                }
                if (aggroBrain.AggroLevel == Constants.USE_AUTOVALUES)
                {
                    aggroBrain.AggroLevel = 0;
                    if (Level > 5)
                    {
                        aggroBrain.AggroLevel = 30;
                    }
                    if (Name != Name.ToLower())
                    {
                        aggroBrain.AggroLevel = 30;
                    }
                    if (Realm != eRealm.None)
                    {
                        aggroBrain.AggroLevel = 60;
                    }
                }
            }

            m_race = (short)dbMob.Race;
            m_bodyType = (ushort)dbMob.BodyType;
            m_houseNumber = (ushort)dbMob.HouseNumber;
            m_maxdistance = dbMob.MaxDistance;
            m_roamingRange = dbMob.RoamingRange;
            m_isCloakHoodUp = dbMob.IsCloakHoodUp;
            m_visibleActiveWeaponSlots = dbMob.VisibleWeaponSlots;
            m_race = (short)dbMob.Race;
            m_isstyleallowed = dbMob.IsStyleAllowed;
            m_mobstyleid = (string)dbMob.MobStyleID;
            m_yellRange = dbMob.YellRange;
            m_meleeAttackRange = dbMob.MeleeAttackRange;
            m_npcIsFish = dbMob.NPCIsFish;
            m_npcIsSeeingAll = dbMob.NpcIsSeeingAll;
            m_ControlledCount = dbMob.ControlledCount;
            Gender = (eGender)dbMob.Gender;
            OwnerID = dbMob.OwnerID;



            if (npcTemplate != null && npcTemplate.ReplaceMobValues)
            {
                LoadTemplate(npcTemplate);
            }

            if (Inventory != null)
            {
                SwitchWeapon(ActiveWeaponSlot);
            }
        }

        /// <summary>
        /// Deletes the mob from the database
        /// </summary>
        public override void DeleteFromDatabase()
        {
            if (Brain != null && Brain is IControlledBrain)
            {
                return;
            }

            if (InternalID != null)
            {
                Mob mob = GameServer.Database.FindObjectByKey<Mob>(InternalID);
                if (mob != null)
                    GameServer.Database.DeleteObject(mob);
            }
        }

        /// <summary>
        /// Saves a mob into the db if it exists, it is
        /// updated, else it creates a new object in the DB
        /// </summary>
        public override void SaveIntoDatabase()
        {
            // do not allow saving in an instanced region
            if (CurrentRegion.IsInstance)
            {
                LoadedFromScript = true;
                return;
            }

            if (Brain != null && Brain is IControlledBrain)
            {
                // do not allow saving of controlled npc's
                return;
            }

            Mob mob = null;
            if (InternalID != null)
            {
                mob = GameServer.Database.FindObjectByKey<Mob>(InternalID);
            }

            if (mob == null)
            {
                if (LoadedFromScript == false)
                {
                    mob = new Mob();
                }
                else
                {
                    return;
                }
            }
            mob.TranslationId = TranslationId;
            mob.Name = Name;
            mob.Guild = GuildName;
            mob.X = X;
            mob.Y = Y;
            mob.Z = Z;
            mob.Heading = Heading;
            mob.Speed = MaxSpeedBase;
            mob.Region = CurrentRegionID;
            mob.Realm = (byte)Realm;
            mob.Model = Model;
            mob.Size = Size;
            mob.Level = Level;

            // Stats
            mob.Constitution = Constitution;
            mob.Dexterity = Dexterity;
            mob.Strength = Strength;
            mob.Quickness = Quickness;
            mob.Intelligence = Intelligence;
            mob.Piety = Piety;
            mob.Empathy = Empathy;
            mob.Charisma = Charisma;

            mob.ClassType = this.GetType().ToString();
            mob.Flags = (uint)Flags;
            mob.Speed = MaxSpeedBase;
            mob.RespawnInterval = m_respawnInterval / 1000;
            mob.HouseNumber = HouseNumber;
            mob.RoamingRange = RoamingRange;
            if (Brain.GetType().FullName != typeof(StandardMobBrain).FullName)
                mob.Brain = Brain.GetType().FullName;
            IOldAggressiveBrain aggroBrain = Brain as IOldAggressiveBrain;
            if (aggroBrain != null)
            {
                mob.AggroLevel = aggroBrain.AggroLevel;
                mob.AggroRange = aggroBrain.AggroRange;
            }
            mob.EquipmentTemplateID = EquipmentTemplateID;

            if (m_faction != null)
                mob.FactionID = m_faction.ID;

            mob.MeleeDamageType = (int)MeleeDamageType;

            if (NPCTemplate != null)
            {
                mob.NPCTemplateID = NPCTemplate.TemplateId;
            }
            else
            {
                mob.NPCTemplateID = -1;
            }

            mob.Race = Race;
            mob.BodyType = BodyType;
            mob.PathID = PathID;
            mob.MaxDistance = m_maxdistance;
            mob.IsCloakHoodUp = m_isCloakHoodUp;
            mob.Gender = (byte)Gender;
            mob.VisibleWeaponSlots = this.m_visibleActiveWeaponSlots;
            mob.PackageID = PackageID;
            mob.OwnerID = OwnerID;
            mob.IsStyleAllowed = IsStyleAllowed;
            mob.NPCIsFish = NPCIsFish;
            mob.YellRange = YellRange;
            mob.MeleeAttackRange = MeleeAttackRange;
            mob.NpcIsSeeingAll = NpcIsSeeingAll;
            mob.ControlledCount = ControlledCount;
            mob.MobStyleID = MobStyleID;

            if (InternalID == null)
            {
                GameServer.Database.AddObject(mob);
                InternalID = mob.ObjectId;
            }
            else
            {
                GameServer.Database.SaveObject(mob);
            }
        }

        /// <summary>
        /// Load a NPC template onto this NPC
        /// </summary>
        /// <param name="template"></param>
        public virtual void LoadTemplate(INpcTemplate template)
        {
            if (template == null)
                return;

            var m_templatedInventory = new List<string>();
            this.TranslationId = template.TranslationId;
            this.Name = template.Name;
            this.GuildName = template.GuildName;


            #region Models, Sizes, Levels
            // Grav: this.Model/Size/Level accessors are triggering SendUpdate()
            // so i must use them, and not directly use private variables

            var splitModel = template.Model.SplitCSV(true);
            ushort.TryParse(splitModel[Util.Random(0, splitModel.Count - 1)], out ushort choosenModel);

            if (choosenModel > 0)
                this.Model = choosenModel;

            byte choosenSize = 50;
            if (!Util.IsEmpty(template.Size))
            {
                var split = template.Size.SplitCSV(true);
                byte.TryParse(split[Util.Random(0, split.Count - 1)], out choosenSize);
            }
            this.Size = choosenSize;

            byte choosenLevel = 1;
            if (!Util.IsEmpty(template.Level))
            {
                var split = template.Level.SplitCSV(true);
                byte.TryParse(split[Util.Random(0, split.Count - 1)], out choosenLevel);
            }
            this.Level = choosenLevel;
            #endregion

            #region Stats
            // Stats
            if (template.Strength == 0)
            {
                this.AutoSetStats();
            }
            else
            {


                this.Constitution = (short)template.Constitution;
                this.Dexterity = (short)template.Dexterity;
                this.Strength = (short)template.Strength;
                this.Quickness = (short)template.Quickness;
                this.Intelligence = (short)template.Intelligence;
                this.Piety = (short)template.Piety;
                this.Empathy = (short)template.Empathy;
                this.Charisma = (short)template.Charisma;
            }
            #endregion

            #region Misc Stats
            this.MaxDistance = template.MaxDistance;
            this.TetherRange = template.TetherRange;
            this.Race = (short)template.Race;
            this.BodyType = (ushort)template.BodyType;
            this.MaxSpeedBase = template.MaxSpeed;
            this.Flags = (eFlags)template.Flags;
            this.MeleeDamageType = template.MeleeDamageType;
            this.ParryChance = template.ParryChance;
            this.EvadeChance = template.EvadeChance;
            this.BlockChance = template.BlockChance;
            this.LeftHandSwingChance = template.LeftHandSwingChance;
            #endregion

            #region Inventory
            //Ok lets start loading the npc equipment - only if there is a value!
            if (!Util.IsEmpty(template.Inventory))
            {
                bool equipHasItems = false;
                GameNpcInventoryTemplate equip = new GameNpcInventoryTemplate();
                //First let's try to reach the npcequipment table and load that!
                //We use a ';' split to allow npctemplates to support more than one equipmentIDs
                string[] equipIDs = template.Inventory.SplitCSV().ToArray();
                if (!template.Inventory.Contains(":"))
                {

                    foreach (string str in equipIDs)
                    {
                        m_templatedInventory.Add(str);
                    }

                    string equipid = "";

                    if (m_templatedInventory.Count > 0)
                    {
                        if (m_templatedInventory.Count == 1)
                            equipid = template.Inventory;
                        else
                            equipid = m_templatedInventory[Util.Random(m_templatedInventory.Count - 1)];
                    }
                    if (equip.LoadFromDatabase(equipid))
                        equipHasItems = true;
                }

                #region Legacy Equipment Code
                //Nope, nothing in the npcequipment table, lets do the crappy parsing
                //This is legacy code
                if (!equipHasItems && template.Inventory.Contains(":"))
                {
                    //Temp list to store our models
                    List<int> tempModels = new List<int>();

                    //Let's go through all of our ';' seperated slots
                    foreach (string str in equipIDs)
                    {
                        tempModels.Clear();
                        //Split the equipment into slot and model(s)
                        string[] slotXModels = str.Split(':');
                        //It should only be two in length SLOT : MODELS
                        if (slotXModels.Length == 2)
                        {

                            //Let's try to get our slot
                            if (Int32.TryParse(slotXModels[0], out int slot))
                            {
                                //Now lets go through and add all the models to the list
                                string[] models = slotXModels[1].Split('|');
                                foreach (string strModel in models)
                                {
                                    //We'll add it to the list if we successfully parse it!
                                    if (Int32.TryParse(strModel, out int model))
                                        tempModels.Add(model);
                                }

                                //If we found some models let's randomly pick one and add it the equipment
                                if (tempModels.Count > 0)
                                    equipHasItems |= equip.AddNPCEquipment((eInventorySlot)slot, tempModels[Util.Random(tempModels.Count - 1)]);
                            }
                        }
                    }
                }
                #endregion

                //We added some items - let's make it the new inventory
                if (equipHasItems)
                {
                    this.Inventory = new GameNPCInventory(equip);
                    if (this.Inventory.GetItem(eInventorySlot.DistanceWeapon) != null)
                        this.SwitchWeapon(eActiveWeaponSlot.Distance);
                }

                if (template.VisibleActiveWeaponSlot > 0)
                    this.VisibleActiveWeaponSlots = template.VisibleActiveWeaponSlot;
            }
            #endregion

            if (template.Spells != null) this.Spells = template.Spells;
            if (template.Styles != null) this.Styles = template.Styles;
            if (template.Abilities != null)
            {
                foreach (Ability ab in template.Abilities)
                    m_abilities[ab.KeyName] = ab;
            }
            BuffBonusCategory4[(int)eStat.STR] += template.Strength;
            BuffBonusCategory4[(int)eStat.DEX] += template.Dexterity;
            BuffBonusCategory4[(int)eStat.CON] += template.Constitution;
            BuffBonusCategory4[(int)eStat.QUI] += template.Quickness;
            BuffBonusCategory4[(int)eStat.INT] += template.Intelligence;
            BuffBonusCategory4[(int)eStat.PIE] += template.Piety;
            BuffBonusCategory4[(int)eStat.EMP] += template.Empathy;
            BuffBonusCategory4[(int)eStat.CHR] += template.Charisma;


            //no aggros in instances
            byte aggrolvl = 0;
            if (this != null && CurrentRegionID > 999)
            {
                aggrolvl = 0;

            }
            else
                aggrolvl = template.AggroLevel;

            m_ownBrain = new StandardMobBrain
            {
                Body = this,
                AggroLevel = aggrolvl,
                AggroRange = template.AggroRange


            };

            this.NPCTemplate = template as NpcTemplate;
        }

        /// <summary>
        /// Switches the active weapon to another one
        /// </summary>
        /// <param name="slot">the new eActiveWeaponSlot</param>
        public override void SwitchWeapon(eActiveWeaponSlot slot)
        {
            base.SwitchWeapon(slot);
            if (ObjectState == eObjectState.Active)
            {
                // Update active weapon appearence
                UpdateNPCEquipmentAppearance();
            }
        }
        /// <summary>
        /// Equipment templateID
        /// </summary>
        protected string m_equipmentTemplateID;
        /// <summary>
        /// The equipment template id of this npc
        /// </summary>
        public string EquipmentTemplateID
        {
            get { return m_equipmentTemplateID; }
            set { m_equipmentTemplateID = value; }
        }
        /// <summary>
        /// Updates the items on a character
        /// </summary>
        public void UpdateNPCEquipmentAppearance()
        {
            if (ObjectState == eObjectState.Active)
            {
                foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    player.Out.SendLivingEquipmentUpdate(this);
            }
        }

        #endregion


        #region Custom Questindicator

        public virtual void CustomQuestindicator()
        {
            Body = this;

            if (Body is ChampNPC)
            {
                foreach (GamePlayer player in Body.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    if (player.ChampionLevel >= 5 && player.HasChampionWeapons == false)
                    {
                        player.Out.SendNPCsQuestEffect(Body, eQuestIndicator.Available);
                    }
                    else if (player.HasChampionWeapons)
                    {
                        player.Out.SendNPCsQuestEffect(Body, eQuestIndicator.Lesson);
                    }
                    else if (player.ChampionLevel < 5)
                    {
                        player.Out.SendNPCsQuestEffect(Body, eQuestIndicator.None);
                    }
                }
            }
            if (Body is XPLootNPC)
            {
                foreach (GamePlayer player in Body.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    if (player.Level < 50)
                    {
                        if (player.XPStoneCount < 20 && player.XPStoneCount > 0)
                        {
                            player.Out.SendNPCsQuestEffect(Body, eQuestIndicator.Finish);
                        }
                        else if (player.XPStoneCount <= 0)
                        {
                            player.Out.SendNPCsQuestEffect(Body, eQuestIndicator.Available);
                        }
                        else if (player.XPStoneCount >= 20)
                        {
                            player.Out.SendNPCsQuestEffect(Body, eQuestIndicator.None);
                        }
                    }
                }
            }

            if (Body is PowerBarrelExchanger)
            {
                foreach (GamePlayer player in Body.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    if (player.BarrelForPowerElixirCount > 0 && player.BarrelForPowerElixirCount < 34)
                    {
                        player.Out.SendNPCsQuestEffect(Body, eQuestIndicator.Finish);
                    }
                    else if (player.GetCraftingSkillValue(eCraftingSkill.Alchemy) >= 1000 && player.BarrelForPowerElixirCount <= 0)
                    {
                        player.Out.SendNPCsQuestEffect(Body, eQuestIndicator.Available);
                    }
                    else if (player.BarrelForPowerElixirCount >= 34)
                    {
                        player.Out.SendNPCsQuestEffect(Body, eQuestIndicator.None);
                    }
                }
            }
            if (Body is HealingBarrelExchanger)
            {
                foreach (GamePlayer player in Body.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    if (player.BarrelForHealingElixirCount > 0 && player.BarrelForHealingElixirCount < 34)
                    {
                        player.Out.SendNPCsQuestEffect(Body, eQuestIndicator.Finish);
                    }
                    else if (player.GetCraftingSkillValue(eCraftingSkill.Alchemy) >= 1000 && player.BarrelForHealingElixirCount <= 0)
                    {
                        player.Out.SendNPCsQuestEffect(Body, eQuestIndicator.Available);
                    }
                    else if (player.BarrelForHealingElixirCount >= 34)
                    {
                        player.Out.SendNPCsQuestEffect(Body, eQuestIndicator.None);
                    }
                }
            }

        }




        #endregion


        #region Quest
        /// <summary>
        /// Holds all the quests this npc can give to players
        /// </summary>
        //protected readonly ArrayList m_questListToGive = new ArrayList();
        protected readonly List<AbstractQuest> m_questListToGive = new List<AbstractQuest>();
        /// <summary>
        /// Gets the questlist of this player
        /// </summary>
        public IList QuestListToGive
        {
            get { return m_questListToGive; }
        }

        /// <summary>
        /// Adds a scripted quest type to the npc questlist
        /// </summary>
        /// <param name="questType">The quest type to add</param>
        /// <returns>true if added, false if the npc has already the quest!</returns>
        public void AddQuestToGive(Type questType)
        {
            lock (((ICollection)m_questListToGive).SyncRoot)
            // lock (m_questListToGive.SyncRoot)
            {
                if (HasQuest(questType) == null)
                {
                    AbstractQuest newQuest = (AbstractQuest)Activator.CreateInstance(questType);
                    if (newQuest != null)
                    {
                        m_questListToGive.Add(newQuest);
                    }



                }
            }
        }

        /// <summary>
        /// removes a scripted quest from this npc
        /// </summary>
        /// <param name="questType">The questType to remove</param>
        /// <returns>true if added, false if the npc has already the quest!</returns>
        public bool RemoveQuestToGive(Type questType)
        {
            lock (((ICollection)m_questListToGive).SyncRoot)
            // lock (m_questListToGive.SyncRoot)
            {
                foreach (AbstractQuest q in m_questListToGive)
                {
                    if (q.GetType().Equals(questType))
                    {
                        m_questListToGive.Remove(q);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Check if the npc can give the specified quest to a player
        /// Used for scripted quests
        /// </summary>
        /// <param name="questType">The type of the quest</param>
        /// <param name="player">The player who search a quest</param>
        /// <returns>the number of time the quest can be done again</returns>
        public int CanGiveQuest(Type questType, GamePlayer player)
        {
            lock (((ICollection)m_questListToGive).SyncRoot)
            //lock (m_questListToGive.SyncRoot)
            {
                foreach (AbstractQuest q in m_questListToGive)
                {
                    if (q.GetType().Equals(questType) && q.CheckQuestQualification(player) && player.HasFinishedQuest(questType) < q.MaxQuestCount)
                    {
                        return q.MaxQuestCount - player.HasFinishedQuest(questType);
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// Return the proper indicator for quest
        /// TODO: check when finish indicator is set
        /// * when you have done the NPC quest
        /// * when you are at the last step
        /// </summary>
        /// <param name="questType">Type of quest</param>
        /// <param name="player">player requesting the quest</param>
        /// <returns></returns>
        public eQuestIndicator SetQuestIndicator(Type questType, GamePlayer player)
        {
            if (CanGiveOneQuest(player)) return eQuestIndicator.Available;
            if (player.HasFinishedQuest(questType) > 0) return eQuestIndicator.Finish;
            return eQuestIndicator.None;
        }

        protected GameNPC m_teleporterIndicator = null;

        /// <summary>
        /// Should this NPC have an associated teleporter indicator
        /// </summary>
        public virtual bool ShowTeleporterIndicator
        {
            get { return false; }
        }

        /// <summary>
        /// Should the NPC show a quest indicator, this can be overriden for custom handling
        /// Checks both scripted and data quests
        /// </summary>
        /// <param name="player"></param>
        /// <returns>True if the NPC should show quest indicator, false otherwise</returns>
        public virtual eQuestIndicator GetQuestIndicator(GamePlayer player)
        {
            // Available one ?
            if (CanGiveOneQuest(player))
                return eQuestIndicator.Available;

            // Finishing one ?
            if (CanFinishOneQuest(player))
                return eQuestIndicator.Finish;

            return eQuestIndicator.None;
        }

        /// <summary>
        /// Check if the npc can give one quest to a player
        /// Checks both scripted and data quests
        /// </summary>
        /// <param name="player">The player to check</param>
        /// <returns>true if yes, false if the npc can give any quest</returns>
        public bool CanGiveOneQuest(GamePlayer player)
        {
            // Scripted quests
            lock (((ICollection)m_questListToGive).SyncRoot)
            // lock (m_questListToGive.SyncRoot)
            {
                foreach (AbstractQuest q in m_questListToGive)
                {
                    if (q != null)
                    {
                        Type questType = q.GetType();
                        int doingQuest = (player.IsDoingQuest(questType) != null ? 1 : 0);
                        if (q.CheckQuestQualification(player) && player.HasFinishedQuest(questType) + doingQuest < q.MaxQuestCount)
                            return true;
                    }
                }
            }

            // Data driven quests
            lock (m_dataQuests)
            {
                foreach (DataQuest quest in DataQuestList)
                {
                    if (quest.ShowIndicator &&
                        quest.CheckQuestQualification(player))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// /// Check if the npc can finish one of DataQuest/RewardQuest Player is doing
        /// This can't be check with AbstractQuest as they don't implement anyway of knowing who is the last target or last step !
        /// </summary>
        /// <param name="player">The player to check</param>
        /// <returns>true if this npc is the last step of one quest, false otherwise</returns>
        public bool CanFinishOneQuest(GamePlayer player)
        {
            // browse Quests.
            List<AbstractQuest> dqs;
            lock (((ICollection)player.QuestList).SyncRoot)
            {
                dqs = new List<AbstractQuest>(player.QuestList);
            }

            foreach (AbstractQuest q in dqs)
            {
                // Handle Data Quest here.

                DataQuest quest = null;
                if (q is DataQuest)
                {
                    quest = (DataQuest)q;
                }

                if (quest != null && (quest.TargetName == Name && (quest.TargetRegion == 0 || quest.TargetRegion == CurrentRegionID)))
                {
                    switch (quest.StepType)
                    {
                        case DataQuest.eStepType.DeliverFinish:
                        case DataQuest.eStepType.InteractFinish:
                        case DataQuest.eStepType.KillFinish:
                        case DataQuest.eStepType.WhisperFinish:
                        case DataQuest.eStepType.CollectFinish:
                            return true;
                    }
                }

                // Handle Reward Quest here.

                RewardQuest rwQuest = null;

                if (q is RewardQuest)
                {
                    rwQuest = (RewardQuest)q;
                }

                if (rwQuest != null && rwQuest.QuestGiver == this)
                {
                    bool done = true;
                    foreach (RewardQuest.QuestGoal goal in rwQuest.Goals)
                    {
                        done &= goal.IsAchieved;
                    }

                    if (done)
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Give a quest a to specific player
        /// used for scripted quests
        /// </summary>
        /// <param name="questType">The quest type</param>
        /// <param name="player">The player that gets the quest</param>
        /// <param name="startStep">The starting quest step</param>
        /// <returns>true if added, false if the player do already the quest!</returns>
        public bool GiveQuest(Type questType, GamePlayer player, int startStep)
        {
            AbstractQuest quest = HasQuest(questType);
            if (quest != null)
            {
                AbstractQuest newQuest = (AbstractQuest)Activator.CreateInstance(questType, new object[] { player, startStep });
                if (newQuest != null && player.AddQuest(newQuest))
                {
                    player.Out.SendNPCsQuestEffect(this, GetQuestIndicator(player));
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if this npc already has a specified quest
        /// used for scripted quests
        /// </summary>
        /// <param name="questType">The quest type</param>
        /// <returns>the quest if the npc have the quest or null if not</returns>
        protected AbstractQuest HasQuest(Type questType)
        {
            lock (((ICollection)m_questListToGive).SyncRoot)
            //lock (m_questListToGive.SyncRoot)
            {
                foreach (AbstractQuest q in m_questListToGive)
                {
                    if (q.GetType().Equals(questType))
                        return q;
                }
            }
            return null;
        }

        #endregion

        #region Riding
        //NPC's can have riders :-)
        /// <summary>
        /// Holds the rider of this NPC as weak reference
        /// </summary>
        public GamePlayer[] Riders;

        /// <summary>
        /// This function is called when a rider mounts this npc
        /// Since only players can ride NPC's you should use the
        /// GamePlayer.MountSteed function instead to make sure all
        /// callbacks are called correctly
        /// </summary>
        /// <param name="rider">GamePlayer that is the rider</param>
        /// <param name="forced">if true, mounting can't be prevented by handlers</param>
        /// <returns>true if mounted successfully</returns>
        public virtual bool RiderMount(GamePlayer rider, bool forced)
        {
            int exists = RiderArrayLocation(rider);
            if (exists != -1)
                return false;


            rider.MoveTo(CurrentRegionID, X, Y, Z, Heading);

            Notify(GameNPCEvent.RiderMount, this, new RiderMountEventArgs(rider, this));
            int slot = GetFreeArrayLocation();
            Riders[slot] = rider;
            rider.Steed = this;
            return true;
        }

        /// <summary>
        /// This function is called when a rider mounts this npc
        /// Since only players can ride NPC's you should use the
        /// GamePlayer.MountSteed function instead to make sure all
        /// callbacks are called correctly
        /// </summary>
        /// <param name="rider">GamePlayer that is the rider</param>
        /// <param name="forced">if true, mounting can't be prevented by handlers</param>
        /// <param name="slot">The desired slot to mount</param>
        /// <returns>true if mounted successfully</returns>
        public virtual bool RiderMount(GamePlayer rider, bool forced, int slot)
        {
            int exists = RiderArrayLocation(rider);
            if (exists != -1)
                return false;

            if (Riders[slot] != null)
                return false;

            //rider.MoveTo(CurrentRegionID, X, Y, Z, Heading);

            Notify(GameNPCEvent.RiderMount, this, new RiderMountEventArgs(rider, this));
            Riders[slot] = rider;
            rider.Steed = this;
            return true;
        }

        /// <summary>
        /// Called to dismount a rider from this npc.
        /// Since only players can ride NPC's you should use the
        /// GamePlayer.MountSteed function instead to make sure all
        /// callbacks are called correctly
        /// </summary>
        /// <param name="forced">if true, the dismounting can't be prevented by handlers</param>
        /// <param name="player">the player that is dismounting</param>
        /// <returns>true if dismounted successfully</returns>
        public virtual bool RiderDismount(bool forced, GamePlayer player)
        {
            if (Riders.Length <= 0)
                return false;

            int slot = RiderArrayLocation(player);
            if (slot < 0)
            {
                return false;
            }
            Riders[slot] = null;

            Notify(GameNPCEvent.RiderDismount, this, new RiderDismountEventArgs(player, this));
            player.Steed = null;

            return true;
        }

        /// <summary>
        /// Get a free array location on the NPC
        /// </summary>
        /// <returns></returns>
        public int GetFreeArrayLocation()
        {
            for (int i = 0; i < MAX_PASSENGERS; i++)
            {
                if (Riders[i] == null)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Get the riders array location
        /// </summary>
        /// <param name="player">the player to get location of</param>
        /// <returns></returns>
        public int RiderArrayLocation(GamePlayer player)
        {
            for (int i = 0; i < MAX_PASSENGERS; i++)
            {
                if (Riders[i] == player)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Get the riders slot on the npc
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public int RiderSlot(GamePlayer player)
        {
            int location = RiderArrayLocation(player);
            if (location == -1)
                return location;
            return location + SLOT_OFFSET;
        }

        /// <summary>
        /// The maximum passengers the NPC can take
        /// </summary>
        public virtual int MAX_PASSENGERS
        {
            get { return 1; }
        }

        /// <summary>
        /// The minimum number of passengers required to move
        /// </summary>
        public virtual int REQUIRED_PASSENGERS
        {
            get { return 1; }
        }

        /// <summary>
        /// The slot offset for this NPC
        /// </summary>
        public virtual int SLOT_OFFSET
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets a list of the current riders
        /// </summary>
        public GamePlayer[] CurrentRiders
        {
            get
            {
                List<GamePlayer> list = new List<GamePlayer>(MAX_PASSENGERS);
                for (int i = 0; i < MAX_PASSENGERS; i++)
                {
                    if (Riders == null || i >= Riders.Length)
                        break;

                    GamePlayer player = Riders[i];
                    if (player != null)
                        list.Add(player);
                }
                return list.ToArray();
            }
        }
        #endregion

        #region Add/Remove/Create/Remove/Update
        /// <summary>
        /// Broadcasts the NPC Update to all players around
        /// </summary>
        public override void BroadcastUpdate()
        {
            base.BroadcastUpdate();

            m_lastUpdateTickCount = (uint)GameTimer.GetTickCount();
        }


        /// <summary>
        /// callback that npc was updated to the world
        /// so it must be visible to at least one player
        /// </summary>
        public void NPCUpdatedCallback()
        {
            m_lastVisibleToPlayerTick = (uint)GameTimer.GetTickCount();
            lock (BrainSync)
            {
                ABrain brain = Brain;
                if (brain != null)
                    brain.Start();
            }
        }
        /// <summary>
        /// Adds the npc to the world
        /// </summary>
        /// <returns>true if the npc has been successfully added</returns>
        public override bool AddToWorld()
        {
            if (!base.AddToWorld()) return false;

            if (MAX_PASSENGERS > 0)
                Riders = new GamePlayer[MAX_PASSENGERS];

            bool anyPlayer = false;
            foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                if (player == null) continue;
                player.Out.SendNPCCreate(this);
                if (m_inventory != null)
                    player.Out.SendLivingEquipmentUpdate(this);

                // If any player was initialized, update last visible tick to enable brain
                anyPlayer = true;
            }

            if (anyPlayer)
                m_lastVisibleToPlayerTick = (uint)GameTimer.GetTickCount();

            m_spawnPoint.X = X;
            m_spawnPoint.Y = Y;
            m_spawnPoint.Z = Z;
            m_spawnHeading = Heading;
            lock (BrainSync)
            {
                ABrain brain = Brain;
                if (brain != null)
                    brain.Start();
            }

            if (Mana <= 0 && MaxMana > 0)
                Mana = MaxMana;
            else if (Mana > 0 && MaxMana > 0)
                StartPowerRegeneration();

            //If the Mob has a Path assigned he will now walk on it!
            if (MaxSpeedBase > 0 && CurrentSpellHandler == null && !IsMoving
                && !AttackState && !InCombat && !IsMovingOnPath && !IsReturningToSpawnPoint
                //Check everything otherwise the Server will crash
                && PathID != null && PathID != "" && PathID != "NULL")
            {
                PathPoint path = MovementMgr.LoadPath(PathID);
                if (path != null)
                {
                    var p = path.GetNearestNextPoint(this);
                    CurrentWayPoint = p;
                    MoveOnPath((short)p.MaxSpeed);
                }
            }

            if (m_houseNumber > 0 && !(this is GameConsignmentMerchant))
            {
                log.Info("NPC '" + Name + "' added to house " + m_houseNumber);
                CurrentHouse = HouseMgr.GetHouse(m_houseNumber);
                if (CurrentHouse == null)
                    log.Warn("House " + CurrentHouse + " for NPC " + Name + " doesn't exist !!!");
                else
                    log.Info("Confirmed number: " + CurrentHouse.HouseNumber.ToString());
            }

            // [Ganrod] Nidel: spawn full life
            if (!InCombat && IsAlive && base.Health < MaxHealth)
            {
                base.Health = MaxHealth;
            }

            // create the ambiant text list for this NPC
            BuildAmbientTexts();
            if (GameServer.Instance.ServerStatus == eGameServerStatus.GSS_Open)
                FireAmbientSentence(eAmbientTrigger.spawning);



            if (ShowTeleporterIndicator)
            {
                if (m_teleporterIndicator == null)
                {
                    m_teleporterIndicator = new GameNPC
                    {
                        Name = "",
                        Model = 1923
                    };
                    m_teleporterIndicator.Flags ^= eFlags.PEACE;
                    m_teleporterIndicator.Flags ^= eFlags.CANTTARGET;
                    m_teleporterIndicator.Flags ^= eFlags.DONTSHOWNAME;
                    m_teleporterIndicator.Flags ^= eFlags.FLYING;
                    m_teleporterIndicator.X = X;
                    m_teleporterIndicator.Y = Y;
                    m_teleporterIndicator.Z = Z + 1;
                    m_teleporterIndicator.CurrentRegionID = CurrentRegionID;
                }

                m_teleporterIndicator.AddToWorld();
            }

            return true;
        }
        public virtual void ReloadAmbientText()
        {
            GameServer.Instance.NpcManager.AmbientReload();
            //log.Error("ambient text wurde neu geladen");
        }
        /// <summary>
        /// Fill the ambient text list for this NPC
        /// </summary>
        protected virtual void BuildAmbientTexts()
        {
            // list of ambient texts
            if (!string.IsNullOrEmpty(Name))
            {
                //  ambientTexts = GameServer.Database.SelectObjects<MobXAmbientBehaviour>("`Source` ='" + GameServer.Database.Escape(Name) + "';");
                // ambientTexts = GameServer.Database.SelectObjects<MobXAmbientBehaviour>("`Source` = @Source", new QueryParameter("@Source", GameServer.Database.Escape(Name)));
                ambientTexts = GameServer.Instance.NpcManager.AmbientBehaviour[Name];
            }
        }

        /// <summary>
        /// Removes the npc from the world
        /// </summary>
        /// <returns>true if the npc has been successfully removed</returns>
        public override bool RemoveFromWorld()
        {
            if (IsMovingOnPath)
                StopMovingOnPath();
            if (MAX_PASSENGERS > 0)
            {
                foreach (GamePlayer player in CurrentRiders)
                {
                    player.DismountSteed(true);
                }
            }

            if (ObjectState == eObjectState.Active)
            {
                foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    player.Out.SendObjectRemove(this);
            }
            if (!base.RemoveFromWorld()) return false;

            lock (BrainSync)
            {
                ABrain brain = Brain;
                brain.Stop();
            }
            EffectList.CancelAll();

            if (ShowTeleporterIndicator && m_teleporterIndicator != null)
            {
                m_teleporterIndicator.RemoveFromWorld();
                m_teleporterIndicator = null;
            }

            return true;
        }

        /// <summary>
        /// Move an NPC within the same region without removing from world
        /// </summary>
        /// <param name="regionID"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="heading"></param>
        /// <param name="forceMove">Move regardless of combat check</param>
        /// <returns>true if npc was moved</returns>
        public virtual bool MoveInRegion(ushort regionID, int x, int y, int z, ushort heading, bool forceMove)
        {
            if (m_ObjectState != eObjectState.Active)
                return false;

            // pets can't be moved across regions
            if (regionID != CurrentRegionID)
                return false;

            if (forceMove == false)
            {
                // do not move a pet in combat, player can passive / follow to bring pet to them
                if (InCombat)
                    return false;

                //  ControlledNpcBrain controlledBrain = Brain as ControlledNpcBrain;

                // only move pet if it's following the owner
                // if (controlledBrain != null && controlledBrain.WalkState != eWalkState.Follow && controlledBrain.AggressionState != eAggressionState.Passive)
                //    return false;
            }

            Region rgn = WorldMgr.GetRegion(regionID);

            if (rgn == null || rgn.GetZone(x, y) == null)
                return false;

            // For a pet move simple erase the pet from all clients and redraw in the new location

            Notify(GameObjectEvent.MoveTo, this, new MoveToEventArgs(regionID, x, y, z, heading));

            if (ObjectState == eObjectState.Active)
            {
                foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    player.Out.SendObjectRemove(this);
                }
            }

            m_x = x;
            m_y = y;
            m_z = z;
            m_Heading = heading;

            foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                if (player == null) continue;

                player.Out.SendNPCCreate(this);

                if (m_inventory != null)
                {
                    player.Out.SendLivingEquipmentUpdate(this);
                }
            }

            BroadcastUpdate();
            return true;
        }

        /// <summary>
        /// Gets or Sets the current Region of the Object
        /// </summary>
        public override Region CurrentRegion
        {
            get { return base.CurrentRegion; }
            set
            {
                Region oldRegion = CurrentRegion;
                base.CurrentRegion = value;
                Region newRegion = CurrentRegion;
                if (oldRegion != newRegion && newRegion != null)
                {
                    if (m_followTimer != null) m_followTimer.Stop();
                    m_followTimer = new NPCRegionTimer(this)
                    {
                        Callback = new NPCTimerCallback(FollowTimerCallback)
                    };
                }
            }
        }

        /// <summary>
        /// Marks this object as deleted!
        /// </summary>
        public override void Delete()
        {
            lock (m_respawnTimerLock)
            {
                if (m_respawnTimer != null)
                {
                    m_respawnTimer.Stop();
                    m_respawnTimer = null;
                }
            }
            lock (BrainSync)
            {
                ABrain brain = Brain;
                brain.Stop();
            }
            StopFollowing();
            TempProperties.removeProperty(CHARMED_TICK_PROP);
            base.Delete();
        }

        #endregion

        #region AI

        /// <summary>
        /// Holds the own NPC brain
        /// </summary>
        protected ABrain m_ownBrain;

        /// <summary>
        /// Holds the all added to this npc brains
        /// </summary>
        private ArrayList m_brains = new ArrayList(1);

        /// <summary>
        /// The sync object for brain changes
        /// </summary>
        private readonly object m_brainSync = new object();

        /// <summary>
        /// Gets the brain sync object
        /// </summary>
        /// private readonly
        public object BrainSync
        {
            get { return m_brainSync; }
        }

        /// <summary>
        /// Gets the current brain of this NPC
        /// </summary>
        public ABrain Brain
        {
            get
            {
                ArrayList brains = m_brains;
                if (brains.Count > 0)
                    return (ABrain)brains[brains.Count - 1];
                return m_ownBrain;
            }
        }



        /// <summary>
        /// Sets the NPC own brain
        /// </summary>
        /// <param name="brain">The new brain</param>
        /// <returns>The old own brain</returns>
        public ABrain SetOwnBrain(ABrain brain)
        {
            if (brain == null)
                return null;
            if (brain.IsActive)
                throw new ArgumentException("The new brain is already active.", "brain");

            lock (BrainSync)
            {
                ABrain oldBrain = m_ownBrain;
                bool activate = oldBrain.IsActive;
                if (activate)
                    oldBrain.Stop();
                m_ownBrain = brain;
                m_ownBrain.Body = this;
                m_ownBrain.Start();

                return oldBrain;
            }
        }

        /// <summary>
        /// Adds a temporary brain to Npc, last added brain is active
        /// </summary>
        /// <param name="newBrain"></param>
        public virtual void AddBrain(ABrain newBrain)
        {
            if (newBrain == null)
                throw new ArgumentNullException("newBrain");
            if (newBrain.IsActive)
                throw new ArgumentException("The new brain is already active.", "newBrain");

            lock (BrainSync)
            {
                Brain.Stop();
                ArrayList brains = new ArrayList(m_brains)
                {
                    newBrain
                };
                m_brains = brains; // make new array list to avoid locks in the Brain property
                newBrain.Body = this;
                newBrain.Start();
                // newBrain.Body.SendLivingStatsAndRegenUpdate();//neu
            }
        }

        /// <summary>
        /// Removes a temporary brain from Npc
        /// </summary>
        /// <param name="removeBrain">The brain to remove</param>
        /// <returns>True if brain was found</returns>
        public virtual bool RemoveBrain(ABrain removeBrain)
        {
            if (removeBrain == null) return false;

            lock (BrainSync)
            {
                ArrayList brains = new ArrayList(m_brains);
                int index = brains.IndexOf(removeBrain);
                if (index < 0) return false;
                bool active = brains[index] == Brain;
                if (active)
                    removeBrain.Stop();
                brains.RemoveAt(index);
                m_brains = brains;
                if (active)
                    Brain.Start();

                return true;
            }
        }

        #endregion

        #region Swimming npc follow stop


        /// <summary>
        /// Swimming npc walk to spawn if target is on ground
        /// </summary>
        /// <param name="SwimmingIsFollowLivingOnGround">NPC Walk to Spawn if true</param>
        /// <returns>true if target is not swimming and body is an fish Walk to Spawn</returns>
        public virtual bool SwimmingIsFollowLivingOnGround(GameNPC Body)
        {
            GamePlayer petOwner = null;
            GameLiving ownerPet = null;

            Body = this;

            if (Body.NPCIsFish == false) //only fishes will stop following
                return false;

            if (Body.IsAlive == false)
                return false;

            //Swimming npc don't follow player owner and there pets
            if (Body is GameNPC && Body.Brain is IControlledBrain == false && Body.TargetObject != null)
            {
                if (Body.TargetObject is GameNPC && (Body.TargetObject as GameNPC).Brain is IControlledBrain)
                {
                    petOwner = ((Body.TargetObject as GameNPC).Brain as IControlledBrain).Owner as GamePlayer;
                    ownerPet = Body.TargetObject as GameNPC;
                }
                //Single player
                if (Body.TargetObject is GamePlayer && ownerPet == null && !Body.IsReturningToSpawnPoint && ((GamePlayer)Body.TargetObject).IsSwimming == false)
                {
                    return true;
                }
                //Pet owner
                if (Body.TargetObject == petOwner && petOwner is GamePlayer && ownerPet != null && petOwner != null && !Body.IsReturningToSpawnPoint && petOwner.IsSwimming == false)
                {
                    return true;
                }
                //The pet 
                if (Body.TargetObject == ownerPet && ownerPet.IsWithinRadius(petOwner, 200) && petOwner is GamePlayer && ownerPet != null && petOwner != null && !Body.IsReturningToSpawnPoint && petOwner.IsSwimming == false)
                {
                    // if (Body.Z >= Body.CurrentZone.Waterlevel)
                    {
                        return true;
                    }
                }

            }
            return false;
        }

        #endregion


        /// <summary>
        /// Broadcast relevant messages to the raid.
        /// </summary>
        /// <param name="message">The message to be broadcast.</param>
        private void BroadcastMessage(String message)
        {
            foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.OBJ_UPDATE_DISTANCE))
            {
                player.Out.SendMessage(message, eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
            }
        }


        #region Dragon Fly

        /// <summary>
        /// Holds Dragon Fly true/false
        /// </summary>
        public const string dragonFly = "DRAGONFLY";


        public virtual void DragonFly()
        {
            Body = this;



            if (Body.IsMoving == false && Body.InCombat == false)
            {
                // Mob mobs = GameServer.Database.FindObjectByKey<Mob>(Body.InternalID);

                switch (Body.Name)
                {


                    case "Sarnvasath":
                        if (Body.TempProperties.getProperty<bool>(dragonFly) == true)
                        {
                            Body.Flags ^= GameNPC.eFlags.FLYING;

                            if ((Body.Flags & GameNPC.eFlags.FLYING) == 0)
                            {
                                BroadcastMessage(String.Format("The earth staggers under your feet as {0} touches the ground near you.", Body.Name));
                                Body.TempProperties.setProperty(dragonFly, false);
                            }
                        }
                        break;
                    case "Amrateth":
                        if (Body.TempProperties.getProperty<bool>(dragonFly) == true)
                        {
                            Body.Flags ^= GameNPC.eFlags.FLYING;

                            if ((Body.Flags & GameNPC.eFlags.FLYING) == 0)
                            {
                                BroadcastMessage(String.Format("The earth staggers under your feet as {0} touches the ground near you.", Body.Name));
                                Body.TempProperties.setProperty(dragonFly, false);
                            }
                        }
                        break;
                    case "Kjorlakath":
                        if (Body.TempProperties.getProperty<bool>(dragonFly) == true)
                        {
                            Body.Flags ^= GameNPC.eFlags.FLYING;

                            if ((Body.Flags & GameNPC.eFlags.FLYING) == 0)
                            {
                                BroadcastMessage(String.Format("The earth staggers under your feet as {0} touches the ground near you.", Body.Name));
                                Body.TempProperties.setProperty(dragonFly, false);
                            }
                        }
                        break;
                    case "Golestandt":
                        if (Body.TempProperties.getProperty<bool>(dragonFly) == true)
                        {
                            Body.Flags ^= GameNPC.eFlags.FLYING;

                            if ((Body.Flags & GameNPC.eFlags.FLYING) == 0)
                            {
                                BroadcastMessage(String.Format("The earth staggers under your feet as {0} touches the ground near you.", Body.Name));
                                Body.TempProperties.setProperty(dragonFly, false);
                            }
                        }
                        break;
                    case "Gjalpinulva":
                        if (Body.TempProperties.getProperty<bool>(dragonFly) == true)
                        {
                            Body.Flags ^= GameNPC.eFlags.FLYING;

                            if ((Body.Flags & GameNPC.eFlags.FLYING) == 0)
                            {
                                BroadcastMessage(String.Format("The earth staggers under your feet as {0} touches the ground near you.", Body.Name));
                                Body.TempProperties.setProperty(dragonFly, false);
                            }
                        }
                        break;
                    case "Cuuldurach the Glimmer King":
                        if (Body.TempProperties.getProperty<bool>(dragonFly) == true)
                        {
                            Body.Flags ^= GameNPC.eFlags.FLYING;

                            if ((Body.Flags & GameNPC.eFlags.FLYING) == 0)
                            {
                                BroadcastMessage(String.Format("The earth staggers under your feet as {0} touches the ground near you.", Body.Name));
                                Body.TempProperties.setProperty(dragonFly, false);
                            }
                        }
                        break;

                }
            }
            else if (Body.IsMovingOnPath && Body.IsMoving && Body.InCombat == false && Body.TempProperties.getProperty<bool>(dragonFly) == false)
            {

                switch (Body.Name)
                {


                    case "Sarnvasath":
                        {
                            Body.Flags ^= GameNPC.eFlags.FLYING;

                            if ((Body.Flags & GameNPC.eFlags.FLYING) != 0)
                            {
                                Body.MoveTo(Body.CurrentRegionID, Body.X, Body.Y, Body.Z, Body.Heading);
                                //Body.SaveIntoDatabase();
                                Body.TempProperties.setProperty(dragonFly, true);
                            }
                        }
                        break;
                    case "Amrateth":
                        {
                            Body.Flags ^= GameNPC.eFlags.FLYING;

                            if ((Body.Flags & GameNPC.eFlags.FLYING) != 0)
                            {
                                Body.MoveTo(Body.CurrentRegionID, Body.X, Body.Y, Body.Z, Body.Heading);
                                //Body.SaveIntoDatabase();
                                Body.TempProperties.setProperty(dragonFly, true);
                            }
                        }
                        break;
                    case "Kjorlakath":
                        {
                            Body.Flags ^= GameNPC.eFlags.FLYING;

                            if ((Body.Flags & GameNPC.eFlags.FLYING) != 0)
                            {
                                Body.MoveTo(Body.CurrentRegionID, Body.X, Body.Y, Body.Z, Body.Heading);
                                //Body.SaveIntoDatabase();
                                Body.TempProperties.setProperty(dragonFly, true);
                            }
                        }
                        break;
                    case "Golestandt":
                        {
                            Body.Flags ^= GameNPC.eFlags.FLYING;

                            if ((Body.Flags & GameNPC.eFlags.FLYING) != 0)
                            {
                                Body.MoveTo(Body.CurrentRegionID, Body.X, Body.Y, Body.Z, Body.Heading);
                                //Body.SaveIntoDatabase();
                                Body.TempProperties.setProperty(dragonFly, true);
                            }
                        }
                        break;
                    case "Gjalpinulva":
                        {
                            Body.Flags ^= GameNPC.eFlags.FLYING;

                            if ((Body.Flags & GameNPC.eFlags.FLYING) != 0)
                            {
                                Body.MoveTo(Body.CurrentRegionID, Body.X, Body.Y, Body.Z, Body.Heading);
                                //Body.SaveIntoDatabase();
                                Body.TempProperties.setProperty(dragonFly, true);
                            }
                        }
                        break;
                    case "Cuuldurach the Glimmer King":
                        {
                            Body.Flags ^= GameNPC.eFlags.FLYING;

                            if ((Body.Flags & GameNPC.eFlags.FLYING) != 0)
                            {
                                Body.MoveTo(Body.CurrentRegionID, Body.X, Body.Y, Body.Z, Body.Heading);
                                //Body.SaveIntoDatabase();
                                Body.TempProperties.setProperty(dragonFly, true);
                            }
                        }
                        break;

                }
            }
        }


        #endregion


        #region Quest Npc Stop

        /// <summary>
        ///  Quest-NPC stops
        /// </summary>
        public virtual void QuestNpcStop()
        {
            Body = this;

            //By Elcotek Quest-NPC stops if he on path and player near a given radius!
            //GamePlayer player = pl as GamePlayer;
            foreach (GamePlayer player in Body.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                if (player == null || Body == null)
                    continue;

                if (Body.CanFinishOneQuest(player) == false && Body.CanGiveOneQuest(player) == false)
                {
                    if (Body.Name == "Filidh Morven" || Body.Name == "Sir Prescott" || Body.Name == "Grumbald")
                    {
                        Body.StartMoving();
                        continue;
                    }
                }

                bool target0 = false;
                bool target1 = false;
                bool target2 = false;
                switch (Body.Name)
                {
                    case "Filidh Morven":
                        {
                            if (Body.IsWithinRadius(player, (ushort)400) && Body.IsMovingOnPath)
                            {

                                target0 = true;
                                Body.StopMoving();
                                Body.TargetObject = player;
                                Body.TurnTo(player);
                                //Body.Emote(eEmote.Bow);
                                player.Out.SendMessage("come to me my friend and listen!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            else
                            {
                                target0 = false;

                            }
                            break;

                        }
                    case "Sir Prescott":
                        {
                            if (Body.IsWithinRadius(player, (ushort)400) && Body.IsMovingOnPath)
                            {
                                target1 = true;
                                Body.StopMoving();
                                Body.TargetObject = player;
                                Body.TurnTo(player);
                                //Body.Emote(eEmote.Bow);
                                player.Out.SendMessage("come to me my friend and listen!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            else
                            {
                                target1 = false;

                            }
                            break;

                        }
                    case "Grumbald":
                        {
                            if (Body.IsWithinRadius(player, (ushort)400) && Body.IsMovingOnPath)
                            {
                                target2 = true;
                                Body.StopMoving();
                                Body.TargetObject = player;
                                Body.TurnTo(player);
                                //Body.Emote(eEmote.Bow);
                                player.Out.SendMessage("come to me my friend and listen!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            else
                            {
                                target2 = false;

                            }
                            break;

                        }

                    default: break;


                }



                if (Body.Name == "Filidh Morven" && !target0)
                {
                    target0 = false;
                    Body.Emote(eEmote.Cry);
                    Body.TargetObject = null;
                    Body.StartMoving();

                    // log.ErrorFormat("Moving {0} .", Body.Name);
                }

                if (Body.Name == "Sir Prescott" && !target1)
                {
                    target1 = false;
                    Body.Emote(eEmote.Cry);
                    Body.TargetObject = null;
                    Body.StartMoving();

                    //  log.ErrorFormat("Moving {0} .", Body.Name);
                }
                if (Body.Name == "Grumbald" && !target2)
                {
                    target2 = false;
                    Body.Emote(eEmote.Cry);
                    Body.TargetObject = null;
                    Body.StartMoving();

                    //   log.ErrorFormat("Moving {0} .", Body.Name);
                }
            }
        }

        #endregion

        #region Day Night NPC's

        private static bool m_loaded = false;
        private static readonly object m_loadingLock = new object();


        //Day/Night Monster names
        protected static List<string> DaymonsterNameList = new List<string>();
        protected static List<string> NightmonsterNameList = new List<string>();
        //Saved Monsters before Remove
        protected static List<GameLiving> NightMobs = new List<GameLiving>();
        protected static List<GameLiving> DayMobs = new List<GameLiving>();

        public static void LoadDayNightMobs()
        {
            lock (m_loadingLock)
            {
                if (!m_loaded)
                {
                    LoadDayTimeMonstersNames();
                    LoadNightTimeMonstersNames();
                    m_loaded = true;
                    log.Info("All Daytime/Nightime Mobs loaded!");
                }
            }
        }

        public static void ReloadDayNightMobs()
        {
            LoadDayTimeMonstersNames();
            LoadNightTimeMonstersNames();
            log.Info("All Daytime/Nightime Mobs reloaded!");
        }
        /// <summary>
        /// Loads DaymonsterName into List
        /// </summary>
        public static void LoadDayTimeMonstersNames()
        {
            string name = "";
            var monsters = GameServer.Database.SelectAllObjects<DayMonsters>();

            if (monsters != null)
            {
                for (int i = 0; i < monsters.Count; i++)
                {
                    if (string.IsNullOrEmpty(monsters[i].Name) == false)
                    {
                        name = monsters[i].Name;
                    }

                    if (DaymonsterNameList.Contains(name) == false)
                    {
                        DaymonsterNameList.Add(name);

                    }
                    //log.InfoFormat("Day time Monster added: {0}", name);
                }
            }
        }


        /// <summary>
        /// Loads NightmonsterName Into List
        /// </summary>
        public static void LoadNightTimeMonstersNames()
        {
            string name = "";
            var monsters = GameServer.Database.SelectAllObjects<NightMonsters>();

            if (monsters != null)
            {
                for (int i = 0; i < monsters.Count; i++)
                {
                    if (string.IsNullOrEmpty(monsters[i].Name) == false)
                    {
                        name = monsters[i].Name;
                    }

                    if (NightmonsterNameList.Contains(name) == false)
                    {
                        NightmonsterNameList.Add(name);

                    }
                    //log.InfoFormat("Night time Monster added: {0}", name);

                }
            }

        }



        protected virtual bool IsDayMonster(string npcname)
        {
            string name = "";
            for (int i = 0; i < DaymonsterNameList.Count; i++)
            {
                if (string.IsNullOrEmpty(DaymonsterNameList[i]) == false)
                {
                    name = DaymonsterNameList[i];
                }
                //log.ErrorFormat("Mob gefunden  = {0}", name);
                if (npcname == name)
                {
                    //log.Debug("Daytime Monster found: name: " + name + " ");
                    return true;
                }
            }
            return false;
        }


        protected virtual bool IsNightMonster(string npcname)
        {
            string name = "";

            for (int i = 0; i < NightmonsterNameList.Count; i++)
            {
                if (string.IsNullOrEmpty(NightmonsterNameList[i]) == false)
                {
                    name = NightmonsterNameList[i].ToString();
                }
                // log.ErrorFormat("Mob gefunden  = {0}", name);
                if (npcname == name)
                {
                    //log.Debug("Nightime Monster found: name: " + name + " ");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get Players from Current Region
        /// </summary>
        /// <returns></returns>
        public static Region GetPlayerRegion()
        {
            Region reg = null;
            foreach (GameClient client in WorldMgr.GetAllPlayingClients())
            {

                if (client != null && client.Player.ObjectState == eObjectState.Active)
                {
                    if (client.Player.CurrentRegion != null)
                    {
                        reg = client.Player.CurrentRegion;
                    }
                    else
                    {
                        return null;
                    }

                    return reg;
                }
            }
            return null;
        }

        /// <summary>
        /// Day/Night Mobs
        /// </summary>
        public virtual void CheckNPCsDayNight()
        {
            Body = this;

            //Monster die nur am Tag spawnen

            //Tag Mobs in der Nacht
            if (GetPlayerRegion() != null && GetPlayerRegion().IsNightTime == false)//am tage von 6 uhr bis 0
            {
                GameObject npc = null;

                if (DayMobs != null)
                {
                    for (int i = 0; i < DayMobs.Count; i++)
                    {
                        if (string.IsNullOrEmpty(DayMobs[i].ToString()) == false)
                        {
                            lock ((DayMobs as ICollection).SyncRoot)
                            {
                                npc = ((DayMobs[i]));

                                if (DayMobs.Contains(DayMobs[i]))
                                    DayMobs.RemoveAt(i);
                            }
                        }
                        // log.ErrorFormat("Mob gefunden  = {0}", name);

                        //log.ErrorFormat("mob internamID found = {0}", npc.InternalID);

                        if (npc != null && npc is GameNPC)
                        {
                            GameNPC mob = npc as GameNPC;
                            Mob mobs = GameServer.Database.FindObjectByKey<Mob>((mob).InternalID);

                            {
                                if (mobs != null)
                                {
                                    //log.ErrorFormat("Tag Mob gefunden!", mobs.Name);
                                    mob.LoadFromDatabase(mobs);
                                    mob.AddToWorld();
                                    /*
                                    if (DayMobs.Contains(mob))
                                    {
                                        DayMobs.Remove(mob);
                                    }
                                    */
                                }
                            }
                        }
                    }
                }
            }
            //Night time
            else if (Body != null && !Body.InCombat && Body.m_ObjectState == eObjectState.Active && GetPlayerRegion() != null && GetPlayerRegion().IsNightTime)//in der nacht von 0 uhr bis 6 sichtbar
            {
                if (IsDayMonster(Body.Name))
                {
                    //log.Error("es ist nacht:  " + Body.Name + "  tag mobs weg !");

                    Body.RemoveFromWorld();
                    if (DayMobs.Contains(Body) == false)
                    {
                        DayMobs.Add(Body);
                        //log.ErrorFormat("mob added to list: state = {0}", Body.m_ObjectState);
                    }
                }
            }





            ///Nacht Mobs am tag
            if (Body != null && !Body.InCombat && (IsNightMonster(Body.Name) && Body.m_ObjectState == eObjectState.Active
                || Body != null && Body.Model == 907) && GetPlayerRegion() != null && GetPlayerRegion().IsNightTime == false)//am tage von 6 uhr bis 0
            {

                // else
                // log.ErrorFormat("name nicht gefunden: {0}  Body name = {1}", (IsNightMonster(m_body.Name)), Body.Name);

                if (IsNightMonster(Body.Name))
                {
                    //log.Error("es ist tag:  " + Body.Name + "  nacht mobs weg !");

                    Body.RemoveFromWorld();
                    if (NightMobs.Contains(Body) == false)
                    {
                        NightMobs.Add(Body);
                        //log.ErrorFormat("mob added to list: state = {0}", Body.m_ObjectState);
                    }
                }

                if (Body != null)
                { //Artifact Belt of Sun protectors
                    switch (Body.Name)
                    {

                        //Artifact Belt of Sun protectors
                        case "protector of dark":
                            {
                                if (Body.Model != 912)
                                {
                                    Body.Model = 912;
                                    Body.Name = "protector of light";
                                    Body.BroadcastUpdate();
                                }
                                break;
                            }
                        case "protector of light":
                            {
                                if (Body.Model != 912)
                                {
                                    Body.Model = 912;
                                    Body.Name = "protector of night";
                                    Body.BroadcastUpdate();

                                }
                                break;
                            }
                    }
                }
            }
            ///Nacht Mobs in der nacht
            else if (GetPlayerRegion() != null && GetPlayerRegion().IsNightTime)//in der nacht von 0 uhr bis 6 sichtbar
            {
                GameObject npc = null;

                if (DayMobs != null)
                {
                    for (int i = 0; i < NightMobs.Count; i++)
                    {
                        if (string.IsNullOrEmpty(NightMobs[i].ToString()) == false)
                        {
                            lock ((NightMobs as ICollection).SyncRoot)
                            {
                                npc = ((NightMobs[i]));
                                if (NightMobs.Contains(NightMobs[i]))
                                    NightMobs.RemoveAt(i);
                            }
                        }
                        // log.ErrorFormat("Mob gefunden  = {0}", name);

                        //log.ErrorFormat("mob internamID found = {0}", npc.InternalID);

                        if (npc != null && npc is GameNPC)
                        {
                            GameNPC mob = npc as GameNPC;

                            Mob mobs = GameServer.Database.FindObjectByKey<Mob>(mob.InternalID);

                            {
                                if (mobs != null)
                                {
                                    //log.ErrorFormat("Mob gefunden!", mobs.Name);
                                    mob.LoadFromDatabase(mobs);
                                    mob.AddToWorld();
                                    /*
                                    if (NightMobs.Contains(mob))
                                    {
                                        NightMobs.Remove(mob);
                                    }
                                    */
                                }
                            }
                        }
                    }
                }
                if (Body != null)
                {
                    switch (Body.Name)
                    {

                        //Artifact Belt of Sun protectors
                        case "protector of dark":
                            {
                                if (Body.Model != 907)
                                {
                                    Body.Model = 907;
                                    Body.Name = "protector of night";
                                    Body.BroadcastUpdate();

                                }
                                break;
                            }
                        case "protector of light":
                            {
                                if (Body.Model != 907)
                                {
                                    Body.Model = 907;
                                    Body.Name = "protector of night";
                                    Body.BroadcastUpdate();

                                }
                                break;
                            }
                    }
                }
            }
        }

        #endregion


        #region GetAggroLevelString
        string aggroLevelString;
        /// <summary>
        /// How friendly this NPC is to player
        /// </summary>
        /// <param name="player">GamePlayer that is examining this object</param>
        /// <param name="firstLetterUppercase"></param>
        /// <returns>aggro state as string</returns>
        public virtual string GetAggroLevelString(GamePlayer player, bool firstLetterUppercase)
        {
            // "aggressive", "hostile", "neutral", "friendly"
            // TODO: correct aggro strings
            // TODO: some merchants can be aggressive to players even in same realm
            // TODO: findout if trainers can be aggro at all

            //int aggro = CalculateAggroLevelToTarget(player);

            // "aggressive towards you!", "hostile towards you.", "neutral towards you.", "friendly."
            // TODO: correct aggro strings

            int aggroLevel;
            if (Faction != null)
            {
                aggroLevel = Faction.GetAggroToFaction(player);


                //log.ErrorFormat("Faction aggro: {0}", aggroLevel);


                if (this is GameNPC && (Flags & GameNPC.eFlags.PEACE) != 0)//peace must be friendly
                    aggroLevelString = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameNPC.GetAggroLevelString.Friendly1");
                else if (aggroLevel > 0)
                    aggroLevelString = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameNPC.GetAggroLevelString.Aggressive1");
                else if (aggroLevel == 0)
                    aggroLevelString = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameNPC.GetAggroLevelString.Hostile1");
                else if (aggroLevel <= 20)
                    aggroLevelString = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameNPC.GetAggroLevelString.Neutral1");
                else if (aggroLevel < 65)
                    aggroLevelString = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameNPC.GetAggroLevelString.Friendly1");

            }
            else
            {
                IOldAggressiveBrain aggroBrain = Brain as IOldAggressiveBrain;
                if (GameServer.ServerRules.IsSameRealm(this, player, true))
                {
                    if (firstLetterUppercase) aggroLevelString = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameNPC.GetAggroLevelString.Friendly2");
                    else aggroLevelString = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameNPC.GetAggroLevelString.Friendly1");
                }
                else if (aggroBrain != null && aggroBrain.AggroLevel > 0)
                {
                    if (firstLetterUppercase) aggroLevelString = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameNPC.GetAggroLevelString.Aggressive2");
                    else aggroLevelString = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameNPC.GetAggroLevelString.Aggressive1");
                }
                else
                {
                    if (firstLetterUppercase) aggroLevelString = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameNPC.GetAggroLevelString.Neutral2");
                    else aggroLevelString = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameNPC.GetAggroLevelString.Neutral1");
                }
            }
            return LanguageMgr.GetTranslation(player.Client.Account.Language, "GameNPC.GetAggroLevelString.TowardsYou", aggroLevelString);
        }


        public string GetPronoun(int form, bool capitalize, string lang)
        {
            switch (Gender)
            {
                case eGender.Male:
                    switch (form)
                    {
                        case 1:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(lang, "GameLiving.Pronoun.Male.Possessive"));
                        case 2:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(lang, "GameLiving.Pronoun.Male.Objective"));
                        default:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(lang, "GameLiving.Pronoun.Male.Subjective"));
                    }

                case eGender.Female:
                    switch (form)
                    {
                        case 1:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(lang, "GameLiving.Pronoun.Female.Possessive"));
                        case 2:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(lang, "GameLiving.Pronoun.Female.Objective"));
                        default:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(lang, "GameLiving.Pronoun.Female.Subjective"));
                    }
                default:
                    switch (form)
                    {
                        case 1:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(lang, "GameLiving.Pronoun.Neutral.Possessive"));
                        case 2:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(lang, "GameLiving.Pronoun.Neutral.Objective"));
                        default:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(lang, "GameLiving.Pronoun.Neutral.Subjective"));
                    }
            }
        }
        /// <summary>
		/// Gets the proper pronoun including capitalization.
		/// </summary>
		/// <param name="form">1=his; 2=him; 3=he</param>
		/// <param name="capitalize"></param>
		/// <returns></returns>
		public override string GetPronoun(int form, bool capitalize)
        {
            String language = ServerProperties.Properties.DB_LANGUAGE;

            switch (Gender)
            {
                case eGender.Male:
                    switch (form)
                    {
                        case 1:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(language,
                                                                                     "GameLiving.Pronoun.Male.Possessive"));
                        case 2:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(language,
                                                                                     "GameLiving.Pronoun.Male.Objective"));
                        default:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(language,
                                                                                     "GameLiving.Pronoun.Male.Subjective"));
                    }

                case eGender.Female:
                    switch (form)
                    {
                        case 1:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(language,
                                                                                     "GameLiving.Pronoun.Female.Possessive"));
                        case 2:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(language,
                                                                                     "GameLiving.Pronoun.Female.Objective"));
                        default:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(language,
                                                                                     "GameLiving.Pronoun.Female.Subjective"));
                    }
                default:
                    switch (form)
                    {
                        case 1:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(language,
                                                                                     "GameLiving.Pronoun.Neutral.Possessive"));
                        case 2:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(language,
                                                                                     "GameLiving.Pronoun.Neutral.Objective"));
                        default:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(language,
                                                                                     "GameLiving.Pronoun.Neutral.Subjective"));
                    }
            }
        }


        /// <summary>
        /// Pick a random style for now.
        /// </summary>
        /// <returns></returns>
        protected override Style GetStyleToUse()
        {
            if (Styles != null && Styles.Count > 0 && Util.Chance(Properties.GAMENPC_CHANCES_TO_STYLE + Styles.Count))
            {
                Style style = (Style)Styles[Util.Random(Styles.Count - 1)];

                //Shield Stun
                InventoryItem leftWeapon = Inventory?.GetItem(eInventorySlot.LeftHandWeapon);

                if (Styles != null && style.WeaponTypeRequirement == (int)eObjectType.Spear && (this.Name == "Spirit Soldier" || this.Name == "Spirit Swordsman" || this.Name == "Spirit Warrior" || this.Name == "Spirit Champion"))
                {
                    //Spear Stun
                    InventoryItem TwohandedWeapon = Inventory?.GetItem(eInventorySlot.TwoHandWeapon);

                    if ((this as GameSpiritmasterPets).HasSpear == true && TwohandedWeapon != null && ActiveWeaponSlot == eActiveWeaponSlot.TwoHanded && style != null && style.WeaponTypeRequirement == (int)eObjectType.Spear)
                    {
                        if (StyleProcessor.CanUseStyle(this, style, AttackWeapon))
                            return style;
                    }
                }
                else if (leftWeapon != null && ActiveWeaponSlot == eActiveWeaponSlot.Standard && style != null && style.WeaponTypeRequirement == (int)eObjectType.Shield)
                {
                    if (StyleProcessor.CanUseStyle(this, style, AttackWeapon))
                        return style;
                }
                else if (style.WeaponTypeRequirement != (int)eObjectType.Shield && style.WeaponTypeRequirement != (int)eObjectType.Spear)
                {
                    if (StyleProcessor.CanUseStyle(this, style, AttackWeapon))
                        return style;
                }
            }
            return base.GetStyleToUse();
        }

        /// <summary>
        /// Adds messages to ArrayList which are sent when object is targeted
        /// </summary>
        /// <param name="player">GamePlayer that is examining this object</param>
        /// <returns>list with string messages</returns>
        public override IList GetExamineMessages(GamePlayer player)
        {
            switch (player.Client.Account.Language)
            {
                case "EN":
                case "en":
                    {
                        IList list = base.GetExamineMessages(player);
                        list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameNPC.GetExamineMessages.YouExamine",
                                                            GetName(0, false), GetPronoun(0, true), GetAggroLevelString(player, false)));
                        return list;
                    }
                default:
                    {
                        IList list = new ArrayList(4)
                        {
                            LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObject.GetExamineMessages.YouTarget",
                                                            GetName(0, false, player.Client.Account.Language, this)),
                            LanguageMgr.GetTranslation(player.Client.Account.Language, "GameNPC.GetExamineMessages.YouExamine",
                                                            GetName(0, false, player.Client.Account.Language, this),
                                                            GetPronoun(0, true, player.Client.Account.Language), GetAggroLevelString(player, false))
                        };
                        return list;
                    }
            }
        }

        /*		/// <summary>
                /// Pronoun of this NPC in case you need to refer it in 3rd person
                /// http://webster.commnet.edu/grammar/cases.htm
                /// </summary>
                /// <param name="firstLetterUppercase"></param>
                /// <param name="form">0=Subjective, 1=Possessive, 2=Objective</param>
                /// <returns>pronoun of this object</returns>
                public override string GetPronoun(bool firstLetterUppercase, int form)
                {
                    // TODO: when mobs will get gender
                    if(PlayerCharacter.Gender == 0)
                        // male
                        switch(form)
                        {
                            default: // Subjective
                                if(firstLetterUppercase) return "He"; else return "he";
                            case 1:	// Possessive
                                if(firstLetterUppercase) return "His"; else return "his";
                            case 2:	// Objective
                                if(firstLetterUppercase) return "Him"; else return "him";
                        }
                    else
                        // female
                        switch(form)
                        {
                            default: // Subjective
                                if(firstLetterUppercase) return "She"; else return "she";
                            case 1:	// Possessive
                                if(firstLetterUppercase) return "Her"; else return "her";
                            case 2:	// Objective
                                if(firstLetterUppercase) return "Her"; else return "her";
                        }

                    // it
                    switch(form)
                    {
                        // Subjective
                        default: if(firstLetterUppercase) return "It"; else return "it";
                        // Possessive
                        case 1:	if(firstLetterUppercase) return "Its"; else return "its";
                        // Objective
                        case 2: if(firstLetterUppercase) return "It"; else return "it";
                    }
                }*/
        #endregion

        #region Interact/WhisperReceive/SayTo

        /// <summary>
        /// The possible triggers for GameNPC ambient actions
        /// </summary>
        public enum eAmbientTrigger
        {
            spawning,
            dieing,
            aggroing,
            fighting,
            roaming,
            killing,
            moving,
            interact,
            seeing,
        }

        /// <summary>
        /// The ambient texts
        /// </summary>
        public IList<MobXAmbientBehaviour> ambientTexts;

        /// <summary>
        /// The Interacteffect duration in milliseconds
        /// </summary>
        public const int DURATION = 20000;


        /// <summary>
        /// This function is called from the ObjectInteractRequestHandler
        /// </summary>
        /// <param name="player">GamePlayer that interacts with this object</param>
        /// <returns>false if interaction is prevented</returns>
        public override bool Interact(GamePlayer player)
        {
            bool hasFaction = false;
            int factionAggroLevel = 0;
            if (!base.Interact(player)) return false;

            if (Faction != null && Faction.ID > 0)
            {
                factionAggroLevel = Faction.GetAggroToFaction(player);

                hasFaction = true;

                if (player.Client.Account.PrivLevel >= (int)ePrivLevel.GM)
                {
                    player.Out.SendMessage("factionAggroLevel =  " + factionAggroLevel + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
            }

            if (!GameServer.ServerRules.IsSameRealm(this, player, true) && hasFaction == false
            || !GameServer.ServerRules.IsSameRealm(this, player, true) && hasFaction && factionAggroLevel > 25)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameNPC.Interact.DirtyLook",
                    GetName(0, true, player.Client.Account.Language, this)), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                Notify(GameObjectEvent.InteractFailed, this, new InteractEventArgs(player));
                return false;
            }

            if (this.IsWithinRadius(player, 350) == false)
                return false;

            if (this.IsMoving && string.IsNullOrEmpty(this.PathID) == false && (this.CanFinishOneQuest(player) == false && this.CanGiveOneQuest(player) == false))
                return false;

            //Automatic pet Whisper by right clicking on the pet
            PetWhisperFunctionHandler(player, this);


            if (this is GameNPC && this.IsMovingOnPath && this is GameTaxiBoat == false)
            {


                if (this.IsMoving && this.PathID != null && this.Name == "Filith Melwasúl" || this.Name == "Sir Prescott" || this.Name == "Grumbald" || this.Name == "Estefa of Atlantis")
                {
                    this.StopMoving();
                    this.TargetObject = player;
                    this.TurnTo(player);
                    this.Emote(eEmote.Bow);
                }


                //for moving npcs with quest
                else if (this.IsMoving && this.PathID != null)
                {
                    this.StopMoving();
                    this.TargetObject = player;
                    this.TurnTo(player);
                    this.Emote(eEmote.Bow);
                    new NPCInteractEffect().Start(this);

                }
            }

            if (MAX_PASSENGERS > 1)
            {
                string name = "";


                if (this is GameBoat)
                    name = "Boat of " + player.Name;
                if (this is GameTaxiBoat)
                    name = "boat";
                if (this is GameSiegeRam)
                    name = "ram";

                if (RiderSlot(player) != -1)
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameNPC.Interact.AlreadyRiding", name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return false;
                }

                if (GetFreeArrayLocation() == -1)
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameNPC.Interact.IsFull", name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return false;
                }

                if (player.IsRiding)
                {
                    player.DismountSteed(true);
                }

                if (player.IsOnHorse)
                {
                    player.IsOnHorse = false;
                }

                player.MountSteed(this, true);
            }

            FireAmbientSentence(eAmbientTrigger.interact, player);
            return true;
        }

        //after interaction end time npc starts walking again.
        public void InteractActionEnd()
        {

            Body = this;
            if (Body.QuestListToGive != null && Body.IsMoving == false && Body.PathID != null)
            {
                Body.StartMoving();
            }
        }


        /// <summary>
        /// Automatic Pet wisper command
        /// </summary>
        /// <param name="player"></param>
        /// <param name="npc"></param>
        public void PetWhisperFunctionHandler(GamePlayer player, GameNPC npc)
        {
            npc = this;

            if (player != null && player.InCombat == false && npc.InCombat == false && npc is GameNPC && (npc as GameNPC).Brain is IControlledBrain)
            {
                string whisperCoamand = string.Empty;
                switch (npc.Name)
                {
                    //niflheim
                    case "Spirit Fighter":
                    case "Spirit Soldier":
                    case "Spirit Swordsman":
                    case "Spirit Warrior":
                    case "Spirit Champion":
                        {
                            whisperCoamand = "niflheim";
                            break;
                        }
                    //underhill
                    case "Underhill Companion":
                    case "Underhill Zealot":
                    case "Underhill Friend":
                    case "Underhill Compatriot":
                    case "Underhill Ally":
                        {
                            whisperCoamand = "underhill";
                            break;
                        }
                    //commander
                    case "dread guardian":
                    case "dread lich":
                    case "dread archer":
                    case "dread commander":
                    case "decayed commander":
                    case "returned commander":
                    case "skeletal commander":
                    case "bone commander":
                        {
                            whisperCoamand = "commander";
                            break;
                        }
                    //arawn
                    case "minor zombie servant":
                    case "lesser zombie servant":
                    case "zombie servant":
                    case "reanimated servant":
                    case "necroservant":
                    case "greater necroservant":
                    case "abomination":
                        {
                            whisperCoamand = "arawn";
                            break;
                        }

                    default: break;
                }

                GamePlayer owner;
                if (Brain is IControlledBrain && (Brain as IControlledBrain).GetPlayerOwner() != null)
                {
                    owner = (Brain as IControlledBrain).GetPlayerOwner();

                    if (owner != null && owner.InternalID == player.InternalID && owner.TargetObject != null) //&& ownerPlayer.TargetObject != null)
                    {
                        // log.DebugFormat("owner interact with his pet  = {0}", owner.Name);
                        GameObject obj = player.TargetObject;
                        if (obj != null && String.IsNullOrEmpty(whisperCoamand) == false)
                        {
                            //log.DebugFormat("wisper  = {0}", owner.Name);
                            player.Whisper(obj, whisperCoamand);
                        }
                    }
                }
            }
        }









        /// <summary>
        /// ToDo
        /// </summary>
        /// <param name="source"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public override bool WhisperReceive(GameLiving source, string text)
        {
            if (!base.WhisperReceive(source, text))
                return false;
            if (source is GamePlayer == false)
                return true;

            GamePlayer player = (GamePlayer)source;

            //TODO: Guards in rvr areas doesn't need check
            if (text == "task")
            {
                if (source.TargetObject == null)
                    return false;
                if (KillTask.CheckAvailability(player, (GameLiving)source.TargetObject))
                {
                    KillTask.BuildTask(player, (GameLiving)source.TargetObject);
                    return true;
                }
                else if (MoneyTask.CheckAvailability(player, (GameLiving)source.TargetObject))
                {
                    MoneyTask.BuildTask(player, (GameLiving)source.TargetObject);
                    return true;
                }
                else if (CraftTask.CheckAvailability(player, (GameLiving)source.TargetObject))
                {
                    CraftTask.BuildTask(player, (GameLiving)source.TargetObject);
                    return true;
                }
            }
            return true;
        }

        /// <summary>
        /// Format "say" message and send it to target in popup window
        /// </summary>
        /// <param name="target"></param>
        /// <param name="message"></param>
        public virtual void SayTo(GamePlayer target, string message, bool announce = true)
        {
            SayTo(target, eChatLoc.CL_PopupWindow, message, announce);
        }

        /// <summary>
        /// Format "say" message and send it to target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="loc">chat location of the message</param>
        /// <param name="message"></param>
        public virtual void SayTo(GamePlayer target, eChatLoc loc, string message, bool announce = true)
        {
            if (target == null)
                return;

            TurnTo(target);
            string resultText = LanguageMgr.GetTranslation(target.Client.Account.Language, "GameNPC.SayTo.Says", GetName(0, true, target.Client.Account.Language, this), message);
            switch (loc)
            {
                case eChatLoc.CL_PopupWindow:
                    target.Out.SendMessage(resultText, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    if (announce)
                    {
                        Message.ChatToArea(this, LanguageMgr.GetTranslation(target.Client.Account.Language, "GameNPC.SayTo.SpeaksTo", GetName(0, true, target.Client.Account.Language, this), target.GetName(0, false)), eChatType.CT_System, WorldMgr.SAY_DISTANCE, target);
                    }
                    break;
                case eChatLoc.CL_ChatWindow:
                    target.Out.SendMessage(resultText, eChatType.CT_Say, eChatLoc.CL_ChatWindow);
                    break;
                case eChatLoc.CL_SystemWindow:
                    target.Out.SendMessage(resultText, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    break;
            }
        }
        #endregion

        #region Combat

        /// <summary>
        /// The property that holds charmed tick if any
        /// </summary>
        public const string CHARMED_TICK_PROP = "CharmedTick";

        /// <summary>
        /// The duration of no exp after charmed, in game ticks
        /// </summary>
        public const int CHARMED_NOEXP_TIMEOUT = 60000;

        public const string LAST_LOS_TARGET_PROPERTY = "last_LOS_checkTarget";
        public const string LAST_LOS_TICK_PROPERTY = "last_LOS_checkTick";
        public const string NUM_LOS_CHECKS_INPROGRESS = "num_LOS_progress";

        protected object LOS_LOCK = new object();

        protected GameObject m_targetLOSObject = null;

        /// <summary>
		/// Starts a melee attack on a target
		/// </summary>
		/// <param name="target">The object to attack</param>
		public override void StartAttack(GameObject target)
        {
            if (target == null)
                return;

            TargetObject = target;


            if (IsAlive && Brain is ControlledNpcBrain)
            {
                (Brain as ControlledNpcBrain).CheckAbilities();
                //log.Error("check abilities");
            }


            long lastTick = this.TempProperties.getProperty<long>(LAST_LOS_TICK_PROPERTY);

            //Pet Losfix
            GameLiving petowner = null;

            if (Brain is IControlledBrain && this is GameKeepGuard == false)
            {
                petowner = (Brain as IControlledBrain).GetLivingOwner();

                if (petowner != null && petowner is GamePlayer)
                {

                    m_targetLOSObject = target;

                    //Los checks for Player Pets
                    (petowner as GamePlayer).Out.SendCheckLOS(this, target, new CheckLOSResponse(PetStartAttackCheckLOS));
                    return;
                }
            }


            //Los Checks for npcs only
            if (this is GameKeepGuard == false && ServerProperties.Properties.ALWAYS_CHECK_PET_LOS &&
                Brain != null &&
                Brain is IControlledBrain &&
                (target is GamePlayer || target is GameNPC || (target is GameNPC && (target as GameNPC).Brain != null && (target as GameNPC).Brain is IControlledBrain)))
            {
                ///log.ErrorFormat("Pet los npc name: {0} traget: {1} ", this.Name, target.Name);
                GameObject lastTarget = (GameObject)this.TempProperties.getProperty<object>(LAST_LOS_TARGET_PROPERTY, null);
                if (lastTarget != null && lastTarget == target)
                {
                    if (lastTick != 0 && CurrentRegion.Time - lastTick < ServerProperties.Properties.LOS_PLAYER_CHECK_FREQUENCY * 1000)
                        return;
                }

                GamePlayer losChecker = null;
                if (target is GamePlayer)
                {
                    losChecker = target as GamePlayer;
                }
                else if (target is GameNPC && (target as GameNPC).Brain is IControlledBrain && (target as GameNPC).Brain is MobPetBrain == false
                    && (target as GameNPC) is BossSubPet == false && (target as GameNPC).Brain is HoundsPetBrain == false)
                {
                    losChecker = ((target as GameNPC).Brain as IControlledBrain).GetPlayerOwner();
                }
                else
                {
                    // try to find another player to use for checking line of site
                    foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    {
                        losChecker = player;
                        break;
                    }
                }

                if (losChecker == null)
                {
                    return;
                }

                lock (LOS_LOCK)
                {
                    int count = TempProperties.getProperty<int>(NUM_LOS_CHECKS_INPROGRESS, 0);

                    if (count > 10)
                    {
                        log.DebugFormat("{0} LOS count check exceeds 10, aborting LOS check!", Name);

                        // Now do a safety check.  If it's been a while since we sent any check we should clear count
                        if (lastTick == 0 || CurrentRegion.Time - lastTick > ServerProperties.Properties.LOS_PLAYER_CHECK_FREQUENCY * 1000)
                        {

                            TempProperties.setProperty(NUM_LOS_CHECKS_INPROGRESS, 0);
                        }

                        return;
                    }

                    count++;
                    TempProperties.setProperty(NUM_LOS_CHECKS_INPROGRESS, count);

                    TempProperties.setProperty(LAST_LOS_TARGET_PROPERTY, target);
                    TempProperties.setProperty(LAST_LOS_TICK_PROPERTY, CurrentRegion.Time);
                    m_targetLOSObject = target;

                }

                losChecker.Out.SendCheckLOS(this, target, new CheckLOSResponse(this.NPCStartAttackCheckLOS));
                return;
            }
            //log.Error("starte attacke!!");
            ContinueStartAttack(target);
        }

        public void PetStartAttackCheckLOS(GameLiving player, ushort response, ushort targetOID)
        {
            if ((response & 0x100) == 0x100)
            {
                // make sure we didn't switch targets
                if (TargetObject != null && m_targetLOSObject != null && TargetObject == m_targetLOSObject)
                {
                    ContinueStartAttack(m_targetLOSObject);
                }
            }
        }



        /// <summary>
        /// We only attack if we have LOS
        /// </summary>
        /// <param name="player"></param>
        /// <param name="response"></param>
        /// <param name="targetOID"></param>
        public void NPCStartAttackCheckLOS(GameLiving player, ushort response, ushort targetOID)
        {
            lock (LOS_LOCK)
            {
                int count = TempProperties.getProperty<int>(NUM_LOS_CHECKS_INPROGRESS, 0);
                count--;
                TempProperties.setProperty(NUM_LOS_CHECKS_INPROGRESS, Math.Max(0, count));
            }

            if ((response & 0x100) == 0x100)
            {
                // make sure we didn't switch targets
                if (TargetObject != null && m_targetLOSObject != null && TargetObject == m_targetLOSObject)
                {
                    ContinueStartAttack(m_targetLOSObject);

                }
            }
            else
            {
                if (m_targetLOSObject != null && m_targetLOSObject is GameLiving && Brain != null && Brain is IOldAggressiveBrain)
                {
                    // there will be a think delay before mob attempts to attack next target
                    (Brain as IOldAggressiveBrain).RemoveFromAggroList(m_targetLOSObject as GameLiving);
                }
            }
        }


        public virtual void ContinueStartAttack(GameObject target)
        {
            GamePlayer owner2 = null;

            if (target != null)
            {
                StopMoving();
                StopMovingOnPath();

                if (Brain != null && Brain is IControlledBrain)
                {
                    if ((Brain as IControlledBrain).AggressionState == eAggressionState.Passive)
                        return;



                    if ((owner2 = ((IControlledBrain)Brain).GetPlayerOwner()) != null)

                        owner2.Stealth(false);
                }

                SetLastMeleeAttackTick();
                StartMeleeAttackTimer();

                /////////////////Pathing fix////////////////////////////////

                if (!IsWithinRadius(target, AttackRange - 50))
                {
                    if (Brain != null)
                    {
                        Brain.Body.StopFollowing();
                    }

                    //log.Error("Gehe zum Target!!");
                    //Body.WalkTo(target, Body.MaxSpeed);
                    PathTo(target.X, target.Y, target.Z, MaxSpeed);



                    if (IsWithinRadius(target, 500))
                    {
                        //log.Error("Starte Attacke!!");
                        base.StartAttack(target);
                    }
                }

                else
                {

                    //log.Error("Starte Attacke2!!");
                    base.StartAttack(target);
                }
                /////////////////Pathing fix////////////////////////////////


                if (AttackState)
                {
                    // if we're moving we need to lock down the current position
                    if (IsMoving)
                        SaveCurrentPosition();


                    //By Elcotek: Pets and NPCs don't walk in closed Keeps and Towers
                    GamePlayer owner = null;
                    foreach (GameObject door in target.GetDoorsInRadius(3000))
                    {

                        if (door is GameKeepDoor && (Brain is IControlledBrain || Brain is ControlledNpcBrain) && (target is GameNPC || target is GamePlayer) && door != null && Brain.Body.Z < Brain.Body.TargetObject.Z - 100
                            && Brain.Body.TargetObject.Z > door.Z + 200 && door.HealthPercent > 0
                            && (door as GameKeepDoor).Component.Keep.Realm != Brain.Body.Realm)
                        {
                            {


                                if (Brain.Body.TargetObject != null && Brain is MobPetBrain == false && Brain is HoundsPetBrain && (Brain is ControlledNpcBrain || Brain is TheurgistPetBrain))
                                {
                                    IControlledBrain brain = ((GameNPC)this).Brain as IControlledBrain;
                                    if (brain != null && Brain.Body.IsAlive)
                                        owner = brain.GetPlayerOwner();
                                }
                            }

                            if (owner != null && Brain is TheurgistPetBrain)
                            {
                                if (Brain.Body.IsCasting == false)

                                    Brain.Body.StopMoving();
                                Brain.Body.MaxSpeedBase = 0;

                                // owner.Out.SendMessage("Your pet will not attack targets in melee on highter position on a closed keep or Tower", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            else if (owner != null && ActiveWeaponSlot != eActiveWeaponSlot.Distance)
                            {

                                (Brain as IControlledBrain).ComeHere();
                                // owner.Out.SendMessage("Your pet will not attack targets in melee on highter position on a closed keep or Tower", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;

                            }
                        }
                    }

                    if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
                    {
                        //Follow(target, AttackRange, STICKMAXIMUMRANGE);
                        // Archer mobs sometimes bug and keep trying to fire at max range unsuccessfully so force them to get just a tad closer.
                        Follow(target, AttackRange - 30, STICKMAXIMUMRANGE);
                    }
                    else
                    {
                        Follow(target, STICKMINIMUMRANGE, STICKMAXIMUMRANGE);
                    }
                }



                GameSpellEffect StrDebuff = SpellHandler.FindEffectOnTarget(Brain.Body, "StrengthDebuff");
                GameSpellEffect StrConDebuff = SpellHandler.FindEffectOnTarget(Brain.Body, "StrengthConstitutionDebuff");



                //Strength debuff for npcs by Elcotek
                {
                    double StrDebuff1 = 0;
                    double StrConDebuff1 = 0;
                    double StrValue;
                    double StrValuePercent;
                    short oldStr;
                    if (StrDebuff != null)
                    {
                        StrDebuff1 = StrDebuff.Spell.Value;
                    }
                    if (StrConDebuff != null)
                    {
                        StrConDebuff1 = StrConDebuff.Spell.Value;
                    }
                    if (StrDebuff1 >= StrConDebuff1)
                    {
                        StrValue = StrDebuff1;
                    }
                    else if (StrDebuff1 <= StrConDebuff1)
                    {
                        StrValue = StrConDebuff1;
                    }
                    else
                        StrValue = 0;
                    oldStr = Convert.ToInt16(Brain.Body.GetBaseStat(eStat.STR));

                    StrValuePercent = ((StrValue * Convert.ToInt16(Brain.Body.Strength) / 100));

                    if (DebuffActive == false && StrValue > 0 && Brain.Body.Strength != (Convert.ToInt16((oldStr) - Convert.ToInt16(StrValuePercent))))
                    {

                        OldSTR = Brain.Body.Strength;
                        Brain.Body.Strength = (Convert.ToInt16((oldStr) - Convert.ToInt16(StrValuePercent)));
                        DebuffActive = true;
                    }
                    else if (StrValue == 0 && DebuffActive == true)
                    {
                        Brain.Body.Strength = OldSTR;
                        DebuffActive = false;
                        OldSTR = 0;
                    }
                }


                if (this.Level >= 50)
                {
                    if (this is GuardLord)
                    {
                        NextCombatStyle = null;

                        if (this.Level > 55 && Util.Chance(89))

                            NextCombatStyle = SkillBase.GetStyleByID(112, (int)eCharacterClass.Armsman);

                        else if (this.Level < 54 && Util.Chance(87))

                            NextCombatStyle = SkillBase.GetStyleByID(109, (int)eCharacterClass.Armsman);

                        else if (this.Level < 54 && Util.Chance(89))

                            NextCombatStyle = SkillBase.GetStyleByID(111, (int)eCharacterClass.Armsman);

                        else if (this.Level > 51 && Util.Chance(89))
                        {
                            NextCombatStyle = SkillBase.GetStyleByID(66, (int)eCharacterClass.Armsman);
                        }
                        else if (this.Level < 52 && Util.Chance(89))
                        {
                            NextCombatStyle = SkillBase.GetStyleByID(260, (int)eCharacterClass.Hero);
                        }

                    }



                    /// <summary>
                    ///Named NPC Combat Styles
                    /// </summary>
                    /// <param name="npc"></param>
                    /// <param name="Styles"></param>
                    switch (this.Name)
                    {
                        case "Renegade Shadowblade":

                            if (this.Level > 51 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(342, (int)eCharacterClass.Shadowblade);
                            }
                            else if (this.Level > 51 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(344, (int)eCharacterClass.Shadowblade);
                            }
                            else if (this.Level < 52 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(278, (int)eCharacterClass.Shadowblade);
                            }
                            else if (this.Level < 52 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(275, (int)eCharacterClass.Shadowblade);
                            }
                            break;

                        case "Shadowblade":

                            if (this.Level > 51 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(342, (int)eCharacterClass.Shadowblade);
                            }
                            else if (this.Level > 51 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(344, (int)eCharacterClass.Shadowblade);
                            }
                            else if (this.Level < 52 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(278, (int)eCharacterClass.Shadowblade);
                            }
                            else if (this.Level < 52 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(275, (int)eCharacterClass.Shadowblade);
                            }
                            break;


                        case "Renegade Infiltrator":

                            if (this.Level > 51 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(342, (int)eCharacterClass.Infiltrator);
                            }
                            else if (this.Level > 51 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(344, (int)eCharacterClass.Infiltrator);
                            }
                            else if (this.Level < 52 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(278, (int)eCharacterClass.Infiltrator);
                            }
                            else if (this.Level < 52 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(275, (int)eCharacterClass.Infiltrator);
                            }
                            break;

                        case "Infiltrator":

                            if (this.Level > 51 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(342, (int)eCharacterClass.Infiltrator);
                            }
                            else if (this.Level > 51 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(344, (int)eCharacterClass.Infiltrator);
                            }
                            else if (this.Level < 52 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(278, (int)eCharacterClass.Infiltrator);
                            }
                            else if (this.Level < 52 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(275, (int)eCharacterClass.Infiltrator);
                            }
                            break;

                        case "Renegade Nightshade":

                            if (this.Level > 51 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(342, (int)eCharacterClass.Nightshade);
                            }
                            else if (this.Level > 51 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(344, (int)eCharacterClass.Nightshade);
                            }
                            else if (this.Level < 52 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(278, (int)eCharacterClass.Nightshade);
                            }
                            else if (this.Level < 52 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(275, (int)eCharacterClass.Nightshade);
                            }
                            break;


                        case "Nightshade":

                            if (this.Level > 51 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(342, (int)eCharacterClass.Nightshade);
                            }
                            else if (this.Level > 51 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(344, (int)eCharacterClass.Nightshade);
                            }
                            else if (this.Level < 52 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(278, (int)eCharacterClass.Nightshade);
                            }
                            else if (this.Level < 52 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(275, (int)eCharacterClass.Nightshade);
                            }
                            break;


                        case "Renegade Guardian": //109 111 alt 112


                            if (this.Level > 51 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(66, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level < 52 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(260, (int)eCharacterClass.Hero);
                            }
                            break;

                        case "Guardian": //109 111 alt 112


                            if (this.Level > 51 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(66, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level < 52 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(260, (int)eCharacterClass.Hero);
                            }
                            break;


                        case "Renegade Huscarl": //109 111 alt 112

                            if (this.Level > 51 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(148, (int)eCharacterClass.Warrior);
                            }
                            else if (this.Level > 51 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(147, (int)eCharacterClass.Warrior);
                            }
                            else if (this.Level < 52 && Util.Chance(88))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(157, (int)eCharacterClass.Warrior);
                            }
                            break;

                        case "Huscarl": //109 111 alt 112

                            if (this.Level > 51 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(148, (int)eCharacterClass.Warrior);
                            }
                            else if (this.Level > 51 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(147, (int)eCharacterClass.Warrior);
                            }
                            else if (this.Level < 52 && Util.Chance(88))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(157, (int)eCharacterClass.Warrior);
                            }
                            break;

                        case "Renegade Hero": //109 111 alt 112

                            if (this.Level > 51 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(109, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level > 51 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(112, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level < 52 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(109, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level < 52 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(111, (int)eCharacterClass.Armsman);
                            }
                            break;

                        case "Hero": //109 111 alt 112

                            if (this.Level > 51 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(109, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level > 51 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(112, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level < 52 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(109, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level < 52 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(111, (int)eCharacterClass.Armsman);
                            }
                            break;

                        case "Renegade Armsman": //109 111 alt 112

                            if (this.Level > 51 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(109, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level > 51 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(112, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level < 52 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(109, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level < 52 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(111, (int)eCharacterClass.Armsman);
                            }
                            break;

                        case "Armsman": //109 111 alt 112

                            if (this.Level > 51 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(109, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level > 51 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(112, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level < 52 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(109, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level < 52 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(111, (int)eCharacterClass.Armsman);
                            }
                            break;

                        case "Renegade Armswoman": //109 111 alt 112

                            if (this.Level > 51 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(109, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level > 51 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(112, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level < 52 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(109, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level < 52 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(111, (int)eCharacterClass.Armsman);
                            }
                            break;

                        case "Armswoman": //109 111 alt 112

                            if (this.Level > 51 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(109, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level > 51 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(112, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level < 52 && Util.Chance(87))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(109, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level < 52 && Util.Chance(89))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(111, (int)eCharacterClass.Armsman);
                            }
                            break;

                    }
                    switch (this.GuildName)
                    {
                        case "Guardian of Agramon":

                            if (this.Level > 85 && Util.Chance(56))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(79, (int)eCharacterClass.Mercenary);
                            }
                            else if (this.Level > 85 && Util.Chance(29))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(101, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level < 85 && Util.Chance(29))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(293, (int)eCharacterClass.Hero);
                            }
                            else if (this.Level < 85 && Util.Chance(19))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(301, (int)eCharacterClass.Hero);
                            }
                            else if (this.Level < 76 && Util.Chance(15))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(221, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level < 76 && Util.Chance(85))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(260, (int)eCharacterClass.Hero);
                            }
                            break;

                        case "Epic Boss":
                            if (this.Level > 85 && Util.Chance(56))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(79, (int)eCharacterClass.Mercenary);
                            }
                            else if (this.Level > 85 && Util.Chance(29))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(101, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level < 85 && Util.Chance(29))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(293, (int)eCharacterClass.Hero);
                            }
                            else if (this.Level < 85 && Util.Chance(19))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(301, (int)eCharacterClass.Hero);
                            }
                            else if (this.Level < 76 && Util.Chance(15))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(221, (int)eCharacterClass.Armsman);
                            }
                            else if (this.Level < 76 && Util.Chance(85))
                            {
                                NextCombatStyle = SkillBase.GetStyleByID(260, (int)eCharacterClass.Hero);
                            }
                            break;

                        case "Guard": //109 111 alt 112
                            {
                                NextCombatStyle = null;
                                if (this.Level > 54 && Util.Chance(87))
                                {
                                    NextCombatStyle = SkillBase.GetStyleByID(109, (int)eCharacterClass.Armsman);
                                }
                                else if (this.Level > 54 && Util.Chance(89))
                                {
                                    NextCombatStyle = SkillBase.GetStyleByID(112, (int)eCharacterClass.Armsman);
                                }
                                else if (this.Level < 55 && Util.Chance(87))
                                {
                                    NextCombatStyle = SkillBase.GetStyleByID(109, (int)eCharacterClass.Armsman);
                                }
                                else if (this.Level < 55 && Util.Chance(89))
                                {
                                    NextCombatStyle = SkillBase.GetStyleByID(111, (int)eCharacterClass.Armsman);
                                }
                                break;

                            }
                        case "Stealther":
                            {
                                if (this.Level > 54 && Util.Chance(87))
                                {
                                    NextCombatStyle = SkillBase.GetStyleByID(342, (int)eCharacterClass.Nightshade);
                                }
                                else if (this.Level > 54 && Util.Chance(89))
                                {
                                    NextCombatStyle = SkillBase.GetStyleByID(344, (int)eCharacterClass.Nightshade);
                                }
                                else if (this.Level < 55 && Util.Chance(87))
                                {
                                    NextCombatStyle = SkillBase.GetStyleByID(278, (int)eCharacterClass.Nightshade);
                                }
                                else if (this.Level < 55 && Util.Chance(89))
                                {
                                    NextCombatStyle = SkillBase.GetStyleByID(275, (int)eCharacterClass.Nightshade);
                                }
                                break;
                            }

                        case "Shield Guard":
                            {
                                if (this.Level > 54 && Util.Chance(15))
                                {
                                    NextCombatStyle = SkillBase.GetStyleByID(228, (int)eCharacterClass.Armsman);
                                }
                                else if (this.Level > 54 && Util.Chance(95))
                                {
                                    NextCombatStyle = SkillBase.GetStyleByID(66, (int)eCharacterClass.Armsman);
                                }
                                else if (this.Level < 55 && Util.Chance(15))
                                {
                                    NextCombatStyle = SkillBase.GetStyleByID(221, (int)eCharacterClass.Armsman);
                                }
                                else if (this.Level < 55 && Util.Chance(95))
                                {
                                    NextCombatStyle = SkillBase.GetStyleByID(260, (int)eCharacterClass.Hero);
                                }
                                break;
                            }
                        case "Archer":
                            {

                                if (ActiveWeaponSlot != eActiveWeaponSlot.Distance && this.Level > 54 && Util.Chance(89))
                                {
                                    NextCombatStyle = SkillBase.GetStyleByID(323, (int)eCharacterClass.Ranger);
                                }
                                else if (ActiveWeaponSlot != eActiveWeaponSlot.Distance && this.Level > 54 && Util.Chance(90))
                                {
                                    NextCombatStyle = SkillBase.GetStyleByID(328, (int)eCharacterClass.Ranger);
                                }
                                else if (ActiveWeaponSlot != eActiveWeaponSlot.Distance && this.Level < 55 && Util.Chance(89))
                                {
                                    NextCombatStyle = SkillBase.GetStyleByID(331, (int)eCharacterClass.Ranger);
                                }
                                else if (ActiveWeaponSlot != eActiveWeaponSlot.Distance && this.Level < 55 && Util.Chance(90))
                                {
                                    NextCombatStyle = SkillBase.GetStyleByID(327, (int)eCharacterClass.Ranger);
                                }
                                else
                                    return;
                                break;

                            }
                    }
                }
            }
        }





        public override void RangedAttackFinished()
        {
            base.RangedAttackFinished();

            if (this is GameKeepGuard == false && ServerProperties.Properties.ALWAYS_CHECK_PET_LOS &&
                Brain != null &&
                Brain is IControlledBrain &&
                (TargetObject is GamePlayer || (TargetObject is GameNPC && (TargetObject as GameNPC).Brain != null && (TargetObject as GameNPC).Brain is IControlledBrain)))
            {
                GamePlayer player = null;

                if (TargetObject is GamePlayer)
                {
                    player = TargetObject as GamePlayer;
                }
                else if (TargetObject is GameNPC && (TargetObject as GameNPC).Brain != null && (TargetObject as GameNPC).Brain is IControlledBrain)
                {
                    if (((TargetObject as GameNPC).Brain as IControlledBrain).Owner is GamePlayer)
                    {
                        player = ((TargetObject as GameNPC).Brain as IControlledBrain).Owner as GamePlayer;
                    }
                }

                if (player != null)
                {
                    player.Out.SendCheckLOS(this, TargetObject, new CheckLOSResponse(NPCStopRangedAttackCheckLOS));
                    if (ServerProperties.Properties.ENABLE_DEBUG)
                    {
                        log.Debug(Name + " sent LOS check to player " + player.Name);
                    }
                }
            }
        }


        /// <summary>
        /// If we don't have LOS we stop attack
        /// </summary>
        /// <param name="player"></param>
        /// <param name="response"></param>
        /// <param name="targetOID"></param>
        public void NPCStopRangedAttackCheckLOS(GameLiving player, ushort response, ushort targetOID)
        {
            if ((response & 0x100) != 0x100)
            {
                if (ServerProperties.Properties.ENABLE_DEBUG)
                {
                    log.Debug(Name + " FAILED stop ranged attack LOS check to player " + player.Name);
                }

                StopAttack();
            }
        }


        private void SetLastMeleeAttackTick()
        {
            if (TargetObject != null)
            {
                if (TargetObject.Realm == 0 || Realm == 0)
                    m_lastAttackTickPvE = m_CurrentRegion.Time;
                else
                    m_lastAttackTickPvP = m_CurrentRegion.Time;
            }
        }

        private void StartMeleeAttackTimer()
        {
            if (m_attackers.Count == 0)
            {
                if (SpellTimer == null)
                    SpellTimer = new SpellAction(this);

                if (!SpellTimer.IsAlive)
                    SpellTimer.Start(1);
            }
        }

        /// <summary>
        /// Gets/sets the object health
        /// </summary>
        public override int Health
        {
            get
            {
                return base.Health;
            }
            set
            {
                base.Health = value;
                //Slow mobs down when they are hurt!
                short maxSpeed = MaxSpeed;
                if (CurrentSpeed > maxSpeed)
                    CurrentSpeed = maxSpeed;
            }
        }

        /// <summary>
        /// npcs can always have mana to cast
        /// </summary>
        public override int Mana
        {
            get { return 5000; }
        }

        /// <summary>
        /// The Max Mana for this NPC
        /// </summary>
        public override int MaxMana
        {
            get { return 1000; }
        }

        /// <summary>
        /// The Concentration for this NPC
        /// </summary>
        public override int Concentration
        {
            get
            {
                return 500;
            }
        }

        /// <summary>
        /// Tests if this MOB should give XP and loot based on the XPGainers
        /// </summary>
        /// <returns>true if it should deal XP and give loot</returns>
        public virtual bool IsWorthReward
        {
            get
            {

                if (CurrentRegion == null || CurrentRegion.Time - CHARMED_NOEXP_TIMEOUT < TempProperties.getProperty<long>(CHARMED_TICK_PROP))
                    return false;
                if (this.Brain is IControlledBrain)
                    return false;


                if (XPGainers.IsEmpty)
                    return false;

                foreach (var de in XPGainers)
                {
                    //log.ErrorFormat("Xpgainer = {0}", de.Key.Name);

                    GameObject obj = de.Key;
                    // If a player to which we are gray killed up we
                    // aren't worth anything either
                    if (obj is GameLiving living && living.IsObjectGreyCon(this))
                    {

                        return false;
                    }
                    //If a gameplayer with privlevel > 1 attacked the
                    //mob, then the players won't gain xp ...
                    if (obj is GamePlayer player && player.Client.Account.PrivLevel > 1)
                    {

                        return false;
                    }

                }
                return true;

            }
            set
            {
            }
        }
        protected void ControlledNPC_Release()
        {
            if (this.ControlledBrain != null)
            {

                //log.Info("On tue le pet !");
                this.Notify(GameLivingEvent.PetReleased, ControlledBrain.Body);
            }
        }

        /// <summary>
        /// Caer Caledon/Unknwn Dungeon RPS Calculator
        /// </summary>
        /// <param name="target"></param>
        /// <returns>true/false</returns>
        public static void GetOwnerOfCaerCaledonAndReciveRPS(GameObject killer, GameNPC KilledNPC)
        {

            int bonusRPSAmount;
            int BasePercent = 25;
            int levelDiffBonus = 0;
            double levelDiff = 0;


            if (killer != null && killer is GamePlayer)
            {
                if ((killer.CurrentRegionID == 498) && killer.Level >= 48) // (killer.CurrentRegionID == 166) if (ServerProperties.Properties.Enable_Caledonia_Event)
                {

                    ICollection<AbstractGameKeep> keepList = KeepMgr.GetAllKeeps();
                    foreach (AbstractGameKeep keep in keepList)
                    {

                        if (KilledNPC.Level >= 40 && KilledNPC != null && keep != null && keep.Name == "Caer Caledon")
                        {
                            GamePlayer player = killer as GamePlayer;

                            levelDiff = player.GetConLevel(KilledNPC);

                            if (levelDiff > 0 && levelDiff < 2)
                            {
                                levelDiffBonus = 10;
                            }
                            if (levelDiff > 1 && levelDiff < 3)
                            {
                                levelDiffBonus = 15;
                            }
                            if (levelDiff > 2)
                            {
                                levelDiffBonus = 20;
                            }

                            if (keep.Realm == killer.Realm)
                            {
                                if (killer is GamePlayer && killer.CurrentRegionID == 498)
                                {

                                    //Bonus RPS for Keepowning
                                    bonusRPSAmount = 50 * BasePercent / 100;




                                    //Bonus im Dungeon
                                    player.GainRealmPoints(Math.Max(25, 50 + bonusRPSAmount + (int)levelDiffBonus));

                                    player.Out.SendMessage("Your realm hold Caer Caledon. You receive " + BasePercent + "% Bonus RPs from this kill.  ", eChatType.CT_Important, eChatLoc.CL_SystemWindow);

                                    if (levelDiffBonus > 0)
                                    {
                                        player.Out.SendMessage("You receive " + levelDiffBonus + " extra Rps from this kill.  ", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    }
                                    return;


                                }
                                else
                                {
                                    //Bonus im Außerhalb
                                    bonusRPSAmount = 5 * BasePercent / 100;

                                    player.GainRealmPoints(Math.Max(2, 5 + bonusRPSAmount + (int)levelDiffBonus));
                                    player.Out.SendMessage("Your realm hold Caer Caledon. You receive " + BasePercent + "% Bonus RPs from this kill.  ", eChatType.CT_Important, eChatLoc.CL_SystemWindow);


                                    if (levelDiffBonus > 0)
                                    {

                                        player.Out.SendMessage("You receive " + levelDiffBonus + " extra Rps from this kill.  ", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    }
                                    return;

                                }

                            }
                            else
                            {
                                //Ohne Bonus im Dungeon
                                if (killer.CurrentRegionID == 498)
                                {
                                    player.GainRealmPoints(Math.Max(25, (50 / 2) + (int)levelDiffBonus));

                                    if (levelDiffBonus > 0)
                                    {
                                        player.Out.SendMessage("You receive " + levelDiffBonus + " extra Rps from your last kill.  ", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    }
                                    return;
                                }
                                else
                                {
                                    //Ohne Bonus im Außerhalb
                                    player.GainRealmPoints(Math.Max(2, (5 / 2) + (int)levelDiffBonus));
                                    if (levelDiffBonus > 0)
                                    {

                                        player.Out.SendMessage("You receive " + levelDiffBonus + " extra Rps from this kill.  ", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    }
                                    return;

                                }

                            }

                        }
                    }
                }

                if ((KilledNPC.Name == "Director Kobil" || KilledNPC.Name == "Lilith the Demon Queen" || KilledNPC.Name == "Legion") && killer.Level >= 50) // (killer.CurrentRegionID == 166) if (ServerProperties.Properties.Enable_Caledonia_Event)
                {

                    //Ohne Bonus im Dungeon
                    if (KilledNPC.Level >= 65 && KilledNPC != null)
                    {
                        GamePlayer player = killer as GamePlayer;

                        levelDiff = player.GetConLevel(KilledNPC);

                        if (levelDiff > 0 && levelDiff < 2)
                        {
                            levelDiffBonus = 10;
                        }
                        if (levelDiff > 1 && levelDiff < 3)
                        {
                            levelDiffBonus = 15;
                        }
                        if (levelDiff > 2)
                        {
                            levelDiffBonus = 20;
                        }



                        player.GainRealmPoints(Math.Max(25, (50 / 2) + (int)levelDiffBonus));

                        if (levelDiffBonus > 0)
                        {
                            player.Out.SendMessage("You receive " + levelDiffBonus + " extra Rps from your last kill.  ", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        }
                        return;
                    }
                    /*
                    else
                    {
                        //Ohne Bonus im Außerhalb
                        player.GainRealmPoints(Math.Max(2, (5 / 2) + (int)levelDiffBonus));
                        if (levelDiffBonus > 0)
                        {

                            player.Out.SendMessage("You receive " + levelDiffBonus + " extra Rps from this kill.  ", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        }
                        return;

                    }
                    */
                }






                if (killer.CurrentRegionID == 249 && killer.Level >= 50) // (killer.CurrentRegionID == 166) if (ServerProperties.Properties.Enable_Caledonia_Event)
                {
                    GamePlayer player = killer as GamePlayer;

                    if (player.RealmLevel < 40)
                    {
                        //Ohne Bonus im Dungeon
                        if (KilledNPC.Level >= 60 && KilledNPC != null)
                        {


                            levelDiffBonus = KilledNPC.Level - (player.Level + 10);

                            //log.ErrorFormat("levelDiff: {0}", levelDiffBonus);
                           
                            player.GainRealmPoints(Math.Max(25, (50 / 2) + (int)levelDiffBonus));

                            if (levelDiffBonus > 0)
                            {
                                player.Out.SendMessage("You receive " + levelDiffBonus + " extra Rps from your last kill.  ", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                            }
                            return;
                        }
                        /*
                        else
                        {
                            //Ohne Bonus im Außerhalb
                            player.GainRealmPoints(Math.Max(2, (5 / 2) + (int)levelDiffBonus));
                            if (levelDiffBonus > 0)
                            {

                                player.Out.SendMessage("You receive " + levelDiffBonus + " extra Rps from this kill.  ", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                            }
                            return;

                        }
                        */
                    }
                }
            }
        }

        /// <summary>
        /// Release Charmed pets from Player Owners and Remove Buffs
        /// </summary>
        /// <param name="npc"></param>
        private void ReleasePlayerCharamedPets(GameNPC npc)
        {
            GameSpellEffect charm = SpellHandler.FindEffectOnTarget(npc, "Charm");

            if (charm != null)
            {
                //Release Charmed pets from Owners and Remove Buffs
                if (npc.Brain is ControlledNpcBrain && ((npc as GameNPC).Brain as IControlledBrain).GetPlayerOwner() != null && ((npc as GameNPC).Brain as IControlledBrain).GetPlayerOwner() is GamePlayer)
                {
                    GamePlayer owner = null;
                    owner = ((npc as GameNPC).Brain as IControlledBrain).GetPlayerOwner();
                    if (owner != null)
                    {
                        if (npc is GamePet pet)
                            pet.StripOwnerBuffs(owner);

                        if (owner != null)
                        {
                            //log.ErrorFormat("Release Pet: {0}", Name);
                            owner.CommandNpcRelease();
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Called when this living dies
        /// </summary>
        public override void Die(GameObject killer)
        {
            GamePlayer playerKiller = null;
            GameLiving mobKiller = null;
            ControlledNpcBrain brain = null;
            

            FireAmbientSentence(eAmbientTrigger.dieing, killer as GameLiving);


            if (ControlledBrain != null)
                ControlledNPC_Release();

            if (killer != null)
            {
                if (IsWorthReward)
                   // log.Error("IsWorthReward ok!!");


                if (Name == "Sir Caledonia")
                    {

                        //log.ErrorFormat("Sir Caledonia resspan time = {0} portal = {1}", RespawnInterval, 5 * 60 * 1000);
                        ///Spawn of Unknown Portal
                        QueenSpawnAdd(5501, 65, 50542, 54367, 3369, 3869, 166, 0, -1, RespawnInterval);

                        if (killer != null)
                        {
                            foreach (GameClient client in WorldMgr.GetClientsOfRegion(166))
                            //client.Out.SendMessage(("The Sir Caledonia dies, and a Unknown Portal opens in his Ruins!"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            {
                                (client.Player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((client.Player as GamePlayer).Client.Account.Language, "The Sir Caledonia dies, and a Unknown Portal opens in his Ruins!"), eChatType.CT_ScreenCenterSmaller, eChatLoc.CL_PopupWindow);
                                (client.Player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((client.Player as GamePlayer).Client.Account.Language, "The Sir Caledonia dies, and a Unknown Portal opens in his Ruins!"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                        }
                    }

                if (this is GamePet == false && this.Brain is MobPetBrain == false && this.Brain is HoundsPetBrain == false)
                {


                    if (killer is GamePlayer)
                    {
                        GamePlayer player = killer as GamePlayer;

                        if (player.Group != null)
                        {
                            //PlayerGroup
                            foreach (GamePlayer players in (killer as GamePlayer).Group.GetPlayersInTheGroup())
                            {
                                if (players.IsWithinRadius(killer, 8000) == false)
                                    continue;

                                GetOwnerOfCaerCaledonAndReciveRPS(players, this);
                            }
                        }
                        else
                        {
                            GetOwnerOfCaerCaledonAndReciveRPS(player, this);
                        }
                    }
                }



                if (Name == "Aten")
                {
                    if (killer is GamePlayer == false && killer != null && killer != this)
                    {

                        if (killer != null && killer is GameNPC && ((GameNPC)killer).Brain is ControlledNpcBrain)
                        {
                            brain = ((GameNPC)killer).Brain as ControlledNpcBrain;
                        }

                        if (brain != null)
                        {
                            mobKiller = brain.GetLivingOwner();

                            if (mobKiller != null && mobKiller is GamePlayer)
                            {
                                playerKiller = mobKiller as GamePlayer;
                            }

                            if (killer != null && playerKiller != null)
                            {
                                playerKiller = killer as GamePlayer;
                            }
                        }
                    }
                    else if (killer != null && killer is GamePlayer)
                    {
                        playerKiller = killer as GamePlayer;
                    }
                    if (playerKiller != null)
                    {
                        if (playerKiller is GamePlayer && IsWorthReward && (playerKiller.Group != null || playerKiller.MemberIsInBG(playerKiller) && GS.ServerProperties.Properties.BATTLEGROUP_LOOT))
                        {
                            BattleGroup battleGroup = playerKiller.TempProperties.getProperty<BattleGroup>(BattleGroup.BATTLEGROUP_PROPERTY, null);

                            if (battleGroup != null && GS.ServerProperties.Properties.BATTLEGROUP_LOOT)
                            {
                                //BattleGroup
                                foreach (GamePlayer players in battleGroup.Members.Keys)
                                {
                                    if (players.IsWithinRadius(killer, 8000) == false)
                                        continue;
                                    players.ReceiveItem(this, "champions_notes_book", 0);
                                    players.BountyPointsAndMassage(20);
                                    players.UpdatePlayerStatus();
                                }
                            }
                            else
                                foreach (GamePlayer players in playerKiller.Group.GetPlayersInTheGroup())
                                {
                                    if (players.IsWithinRadius(killer, 8000) == false)
                                        continue;
                                    players.ReceiveItem(this, "champions_notes_book", 0);
                                    players.BountyPointsAndMassage(20);
                                    players.UpdatePlayerStatus();


                                }
                        }
                        else
                        {
                            if (playerKiller.IsWithinRadius(killer, 8000) == false)
                                return;
                            playerKiller.ReceiveItem(this, "champions_notes_book", 0);
                            playerKiller.BountyPointsAndMassage(20);
                            playerKiller.UpdatePlayerStatus();

                        }
                    }
                }
               // log.ErrorFormat("killer: {0}", killer.Name);
              //  if (killer != null && killer != this && killer is GamePlayer && Brain != null && Brain is ControlledNpcBrain == false)
             //   {

                    DropLoot(killer);

                    Message.SystemToArea(this, GetName(0, true) + " dies!", eChatType.CT_PlayerDied, killer);
                    if (killer is GamePlayer)
                        ((GamePlayer)killer).Out.SendMessage(GetName(0, true) + " dies!", eChatType.CT_PlayerDied, eChatLoc.CL_SystemWindow);
              //  }
            }
            StopFollowing();

            if (Group != null)
                Group.RemoveMember(this);

            if (killer != null)
            {
                MobKiller = killer;
                MobName = this.Name;

                // Handle faction alignement changes // TODO Review
                if ((Faction != null) && (killer is GamePlayer))
                {
                    // Get All Attackers. // TODO check if this shouldn't be set to Attackers instead of XPGainers ?
                    foreach (var de in XPGainers)
                    {
                        GameLiving living = de.Key as GameLiving;
                        GamePlayer player = living as GamePlayer;

                        // Get Pets Owner (// TODO check if they are not already treated as attackers ?)
                        if (living is GameNPC && (living as GameNPC).Brain is IControlledBrain)
                            player = ((living as GameNPC).Brain as IControlledBrain).GetPlayerOwner();

                        if (player != null && player.ObjectState == GameObject.eObjectState.Active && player.IsAlive && player.IsWithinRadius(this, WorldMgr.MAX_EXPFORKILL_DISTANCE))
                        {
                            double conlevel = player.GetConLevel(this);
                            if (conlevel <= -2.25)
                            {
                                //noXP
                            }
                            else
                            {
                                Faction.KillMember(player);
                            }
                        }
                    }
                }

                if (this is GameNPC && PathCalculator != null)
                    PathCalculator.Clear();

                //Release Charmed pets from Owners and Remove Buffs
                ReleasePlayerCharamedPets(this);


                // deal out exp and realm points based on server rules
                GameServer.ServerRules.OnNPCKilled(this, killer);
                base.Die(killer);
            }

            Delete();

            // remove temp properties
            TempProperties.removeAllProperties();

           

           //No respawn for pets and npcs in Instances
            if (this is GamePet == false && this.Brain is MobPetBrain == false && this.Brain is HoundsPetBrain == false && CurrentRegionID < 1000)
                StartRespawn();
        }


        /// <summary>
        /// Stores the melee damage type of this NPC
        /// </summary>
        protected eDamageType m_meleeDamageType = eDamageType.Slash;

        /// <summary>
        /// Gets or sets the melee damage type of this NPC
        /// </summary>
        public virtual eDamageType MeleeDamageType
        {
            get { return m_meleeDamageType; }
            set { m_meleeDamageType = value; }
        }

        /// <summary>
        /// Returns the damage type of the current attack
        /// </summary>
        /// <param name="weapon">attack weapon</param>
        public override eDamageType AttackDamageType(InventoryItem weapon)
        {
            return m_meleeDamageType;
        }

        /// <summary>
        /// Stores the NPC evade chance
        /// </summary>
        protected byte m_evadeChance;
        /// <summary>
        /// Stores the NPC block chance
        /// </summary>
        protected byte m_blockChance;
        /// <summary>
        /// Stores the NPC parry chance
        /// </summary>
        protected byte m_parryChance;
        /// <summary>
        /// Stores the NPC left hand swing chance
        /// </summary>
        protected byte m_leftHandSwingChance;

        /// <summary>
        /// Gets or sets the NPC evade chance
        /// </summary>
        public virtual byte EvadeChance
        {
            get { return m_evadeChance; }
            set { m_evadeChance = value; }
        }

        /// <summary>
        /// Gets or sets the NPC block chance
        /// </summary>
        public virtual byte BlockChance
        {
            get
            {
                InventoryItem leftWeapon = Inventory?.GetItem(eInventorySlot.LeftHandWeapon);
                //When npcs have two handed weapons, we don't want them to block
                if (ActiveWeaponSlot != eActiveWeaponSlot.Standard)
                    return 0;
                //block only if a shield in left hand slot active
                if (leftWeapon == null)
                    return 0;


                return m_blockChance;
            }
            set
            {
                m_blockChance = value;
            }
        }

        /// <summary>
        /// Gets or sets the NPC parry chance
        /// </summary>
        public virtual byte ParryChance
        {
            get { return m_parryChance; }
            set { m_parryChance = value; }
        }

        /// <summary>
        /// Gets or sets the NPC left hand swing chance
        /// </summary>
        public byte LeftHandSwingChance
        {
            get { return m_leftHandSwingChance; }
            set { m_leftHandSwingChance = value; }
        }

        /// <summary>
        /// Calculates how many times left hand swings
        /// </summary>
        /// <returns></returns>
        public override int CalculateLeftHandSwingCount()
        {
            if (Util.Chance(m_leftHandSwingChance))
                return 1;
            return 0;
        }

        /// <summary>
        /// Checks whether Living has ability to use lefthanded weapons
        /// </summary>
        public override bool CanUseLefthandedWeapon
        {
            get { return m_leftHandSwingChance > 0; }
        }

        /// <summary>
        /// Method to switch the npc to Melee attacks
        /// </summary>
        /// <param name="target"></param>
        public void SwitchToMelee(GameObject target)
        {
            // Tolakram: Order is important here.  First StopAttack, then switch weapon
            StopFollowing();
            StopAttack();

            InventoryItem twohand = Inventory.GetItem(eInventorySlot.TwoHandWeapon);
            InventoryItem righthand = Inventory.GetItem(eInventorySlot.RightHandWeapon);

            if (twohand != null && righthand == null)
                SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            else if (twohand != null && righthand != null)
            {
                if (Util.Chance(50))
                    SwitchWeapon(eActiveWeaponSlot.TwoHanded);
                else SwitchWeapon(eActiveWeaponSlot.Standard);
            }
            else
                SwitchWeapon(eActiveWeaponSlot.Standard);

            StartAttack(target);
        }

        /// <summary>
        /// Method to switch the guard to Ranged attacks
        /// </summary>
        /// <param name="target"></param>
        public void SwitchToRanged(GameObject target)
        {
            StopFollowing();
            StopAttack();
            SwitchWeapon(eActiveWeaponSlot.Distance);
            StartAttack(target);
        }

        /// <summary>
        /// Draw the weapon, but don't actually start a melee attack.
        /// </summary>		
        public virtual void DrawWeapon()
        {
            if (!AttackState)
            {
                AttackState = true;

                BroadcastUpdate();

                AttackState = false;
            }
        }

        /// <summary>
        /// If npcs cant move, they cant be interupted from range attack
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="attackType"></param>
        /// <returns></returns>
        protected override bool OnInterruptTick(GameLiving attacker, AttackData.eAttackType attackType)
        {
            if (this.MaxSpeedBase == 0)
            {
                if (attackType == AttackData.eAttackType.Ranged || attackType == AttackData.eAttackType.Spell)
                {
                    if (this.IsWithinRadius(attacker, 150) == false)
                        return false;
                }
            }

            // Experimental - this prevents interrupts from causing ranged attacks to always switch to melee
            if (AttackState)
            {  ///&& HealthPercent < MINHEALTHPERCENTFORRANGEDATTACK 
                if (ActiveWeaponSlot == eActiveWeaponSlot.Distance && HealthPercent < MINHEALTHPERCENTFORRANGEDATTACK
                    || SpellHandler.FindEffectOnTarget(Body, "CombatSpeedDebuff") != null
                    || SpellHandler.FindEffectOnTarget(Body, "DexterityDebuff") != null
                    || SpellHandler.FindEffectOnTarget(Body, "ConstitutionDebuff") != null
                    || SpellHandler.FindEffectOnTarget(Body, "CrushSlashThrustDebuff") != null
                    || SpellHandler.FindEffectOnTarget(Body, "DexterityQuicknessDebuff") != null
                    || SpellHandler.FindEffectOnTarget(Body, "EnergyResistDebuff") != null
                    || SpellHandler.FindEffectOnTarget(Body, "HeatResistDebuff") != null
                    || SpellHandler.FindEffectOnTarget(Body, "MatterResistDebuff") != null
                    || SpellHandler.FindEffectOnTarget(Body, "MeleeDamageDebuff") != null
                        || SpellHandler.FindEffectOnTarget(Body, "SlashResistDebuff") != null
                        || SpellHandler.FindEffectOnTarget(Body, "SpiritResistDebuff") != null
                        || SpellHandler.FindEffectOnTarget(Body, "StrengthConstitutionDebuff") != null
                        || SpellHandler.FindEffectOnTarget(Body, "StyleCombatSpeedDebuff") != null
                        || SpellHandler.FindEffectOnTarget(Body, "ThrustResistDebuff") != null
                        || SpellHandler.FindEffectOnTarget(Body, "ArmorAbsorptionDebuff") != null
                        || SpellHandler.FindEffectOnTarget(Body, "VampiirBolt") != null
                         || SpellHandler.FindEffectOnTarget(Body, "Taunt") != null)
                {
                    SwitchToMelee(attacker);
                }
                if (m_attackAction == null && ActiveWeaponSlot != eActiveWeaponSlot.Distance &&
                         Inventory != null &&
                         Inventory.GetItem(eInventorySlot.DistanceWeapon) != null && GetDistanceTo(attacker) > 500 && (HealthPercent > MINHEALTHPERCENTFORRANGEDATTACKRANGED || SpellHandler.FindEffectOnTarget(Body, "Taunt") != null))
                {
                    SwitchToRanged(attacker);
                    // }
                    // else if (m_attackAction == null && GetDistanceTo(attacker) > 600)
                    //{
                    //	SwitchToRanged(attacker);
                }
            }

            return base.OnInterruptTick(attacker, attackType);
        }



        /// <summary>
        /// The time to wait before each mob respawn
        /// </summary>
        protected int m_respawnInterval;

        /// <summary>
        /// A timer that will respawn this mob
        /// </summary>
        protected NPCRegionTimer m_respawnTimer;
        /// <summary>
        /// The sync object for respawn timer modifications
        /// </summary>
        protected readonly object m_respawnTimerLock = new object();


        /// <summary>
        /// The Respawn Interval of this mob in milliseconds
        /// </summary>
        public virtual int RespawnInterval
        {
            get
            {
                if (m_respawnInterval > 0 || m_respawnInterval < 0)
                    return m_respawnInterval;

                int minutes = Util.Random(ServerProperties.Properties.NPC_MIN_RESPAWN_INTERVAL, ServerProperties.Properties.NPC_MIN_RESPAWN_INTERVAL + 5);

                if (Name != Name.ToLower())
                {
                    minutes += 5;
                }

                if (Level <= 65 && Realm == 0)
                {
                    return minutes * 60000;
                }
                else if (Realm != 0)
                {
                    // 5 to 10 minutes for realm npc's
                    return Util.Random(5 * 60000, 10 * 60000);
                }
                else
                {
                    int add = (Level - 65) + ServerProperties.Properties.NPC_MIN_RESPAWN_INTERVAL;
                    return (minutes + add) * 60000;
                }
            }
            set
            {
                m_respawnInterval = value;
            }
        }




        private INpcTemplate m_addTemplate;

        /// <summary>
        /// Create an add from the specified template.
        /// </summary>
        /// <param name="templateID"></param>
        /// <param name="level"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="uptime"></param>
        /// <returns></returns>
        protected GameNPC SpawnAdd(int templateID, int level, int x, int y, int Z, ushort heading, int uptime)
        {
            GameNPC add = null;

            try
            {
                if (m_addTemplate == null || m_addTemplate.TemplateId != templateID)
                {
                    m_addTemplate = NpcTemplateMgr.GetTemplate(templateID);
                }

                // Create add from template.


                if (m_addTemplate != null)
                {
                    add = new GameNPC(m_addTemplate)
                    {
                        CurrentRegion = CurrentRegion,
                        Heading = heading,
                        Realm = 0,
                        X = x,
                        Y = y,
                        Z = Z,
                        CurrentSpeed = 0,
                        Level = (byte)level,
                        RespawnInterval = RespawnInterval,
                        RoamingRange = RoamingRange
                    };
                    add.AddToWorld();

                }
            }

            catch
            {
                log.Warn(String.Format("Unable to get template for {0}", Name));
            }
            return add;
        }


        /// <summary>
        /// True if NPC is alive, else false.
        /// </summary>
        public override bool IsAlive
        {
            get
            {
                bool alive = base.IsAlive;
                if (alive && IsRespawning)
                    return false;
                return alive;
            }
        }

        /// <summary>
        /// True, if the mob is respawning, else false.
        /// </summary>
        public bool IsRespawning
        {
            get
            {
                if (m_respawnTimer == null)
                    return false;
                return m_respawnTimer.IsAlive;
            }
        }





        /// <summary>
        /// Starts the Respawn Timer
        /// </summary>
        public virtual void StartRespawn()
        {
            if (IsAlive) return;


            if (this.Brain is IControlledBrain)
                return;


            //Demon Queen Spawn in DF
            if (MobKiller != null && MobName != null)
            {
                DemonSpawn(MobKiller, MobName);
            }

            int respawnInt = RespawnInterval;
            if (respawnInt > 0)
            {
                lock (m_respawnTimerLock)
                {
                    if (m_respawnTimer == null)
                    {
                        m_respawnTimer = new NPCRegionTimer(this)
                        {
                            Callback = new NPCTimerCallback(RespawnTimerCallback)
                        };
                    }
                    else if (m_respawnTimer.IsAlive)
                    {
                        m_respawnTimer.Stop();
                    }

                    // register Mob as "respawning"
                    CurrentRegion.MobsRespawning.TryAdd(this, respawnInt);

                    m_respawnTimer.Start(respawnInt);
                }
            }
        }

        readonly int HarbingerChance = Util.Random(2);
        readonly int GorfChance = Util.Random(3);
        readonly int AncientHermitChance = Util.Random(3);
        //int tiefSantaChance = Util.Random(4);

        //Quest: Eine harte Prüfung

        /// <summary>
        /// The callback that will respawn this mob
        /// </summary>
        /// <param name="respawnTimer">the timer calling this callback</param>
        /// <returns>the new interval</returns>
        protected virtual int RespawnTimerCallback(NPCRegionTimer respawnTimer)
        {

            // remove Mob from "respawning"
            CurrentRegion.MobsRespawning.TryRemove(this, out int dummy);

            if (dummy > 0)
            {
                lock (m_respawnTimerLock)
                {
                    if (m_respawnTimer != null)
                    {
                        m_respawnTimer.Stop();
                        m_respawnTimer = null;
                    }
                }
                //log.Error("vom respawntimer entfernt");
            }

            //DOLConsole.WriteLine("respawn");
            //TODO some real respawn handling
            if (IsAlive) return 0;
            if (ObjectState == eObjectState.Active) return 0;


            //hib wall : x515640 y477158 z8840 h1015 r163
            //alb wall : x535970 y476928 z8816 h3959 r163
            //mid wall : x534595 y449281 z9275 h9275 r163

            //Radom location Spawns
            if (Name == "Harbinger of Spring")
            {
                Health = MaxHealth;
                Mana = MaxMana;
                Endurance = MaxEndurance;

                //Caledonia
                /*
                if (HarbingerChance == 0)
                {
                    SpawnAdd(853437, Util.Random(65, 68), 25696, 52842, 3423, 1204, 0);
                }
                else if (HarbingerChance == 1)
                {
                    SpawnAdd(853437, Util.Random(65, 68), 31945, 25846, 3760, 2136, 0);
                }
                else if (HarbingerChance == 2)
                {
                    SpawnAdd(853437, Util.Random(65, 68), 58025, 30747, 3378, 3829, 0);
                }
                */
                //New Frontiers
                if (HarbingerChance == 0)
                {
                    SpawnAdd(853437, Util.Random(65, 68), 515640, 477158, 8840, 1015, 0);
                }
                else if (HarbingerChance == 1)
                {
                    SpawnAdd(853437, Util.Random(65, 68), 535970, 476928, 8816, 3959, 0);
                }
                else if (HarbingerChance == 2)
                {
                    SpawnAdd(853437, Util.Random(65, 68), 534595, 449281, 9275, 9275, 0);
                }
                return 0;
            }
            if (Name == "Gorf")
            {
                Health = MaxHealth;
                Mana = MaxMana;
                Endurance = MaxEndurance;

                if (GorfChance == 0)
                {
                    SpawnAdd(856649, Util.Random(38, 39), 532643, 554720, 6768, 1890, 0);
                }
                else if (GorfChance == 1)
                {
                    SpawnAdd(856649, Util.Random(38, 39), 534345, 573341, 6772, 2492, 0);
                }
                else if (GorfChance == 2)
                {
                    SpawnAdd(856649, Util.Random(38, 39), 574604, 568965, 6731, 1146, 0);
                }
                else if (GorfChance == 3)
                {
                    SpawnAdd(856649, Util.Random(38, 39), 574327, 554852, 7153, 1420, 0);
                }
                return 0;
            }
            if (Name == "Ancient Hermit")
            {
                Health = MaxHealth;
                Mana = MaxMana;
                Endurance = MaxEndurance;

                if (AncientHermitChance == 0)
                {
                    SpawnAdd(856650, Util.Random(55, 56), 534899, 552193, 4104, 2797, 0);
                }
                else if (AncientHermitChance == 1)
                {
                    SpawnAdd(856651, Util.Random(55, 56), 545438, 536636, 4202, 4060, 0);
                }
                else if (AncientHermitChance == 2)
                {
                    SpawnAdd(856652, Util.Random(55, 56), 546775, 570014, 3442, 2351, 0);
                }
                else if (AncientHermitChance == 3)
                {
                    SpawnAdd(856653, Util.Random(55, 56), 542886, 546016, 3873, 2187, 0);
                }
                return 0;
            }

            /*
            //Event Quest: Eine harte Prüfung
            if (Name == "Altus")
            {
                Health = MaxHealth;
                Mana = MaxMana;
                Endurance = MaxEndurance;

                if (AltusChance == 0)
                {
                    SpawnAdd(85663, 1, 454761, 571249, 8104, 3409, 0);
                }
                else if (AltusChance == 1)
                {
                    SpawnAdd(85663, 1, 466904, 604903, 8048, 1588, 0);
                }
                else if (AltusChance == 2)
                {
                    SpawnAdd(85663, 1, 451469, 597909, 8074, 1565, 0);
                }
                else if (AltusChance == 3)
                {
                    SpawnAdd(85663, 1, 429788, 578980, 8105, 2175, 0);
                }
                else if (AltusChance == 4)
                {
                    SpawnAdd(85663, 1, 444217, 599303, 8320, 3427, 0);
                }
                else if (AltusChance == 5)
                {
                    SpawnAdd(85663, 1, 456922, 604534, 8048, 411, 0);
                }
                return 0;
            }
            if (Name == "Grimhilde")
            {
                Health = MaxHealth;
                Mana = MaxMana;
                Endurance = MaxEndurance;

                if (GrimhildeChance == 0)
                {
                    SpawnAdd(85664, 1, 616229, 388355, 8281, 915, 0);
                }
                else if (GrimhildeChance == 1)
                {
                    SpawnAdd(85664, 1, 623524, 400032, 8834, 1944, 0);
                }
                else if (GrimhildeChance == 2)
                {
                    SpawnAdd(85664, 1, 643720, 369815, 8567, 846, 0);
                }
                else if (GrimhildeChance == 3)
                {
                    SpawnAdd(85664, 1, 640314, 381393, 8357, 82, 0);
                }
                else if (GrimhildeChance == 4)
                {
                    SpawnAdd(85664, 1, 642876, 421899, 8416, 1253, 0);
                }
                else if (GrimhildeChance == 5)
                {
                    SpawnAdd(85664, 1, 603439, 363131, 8295, 2001, 0);
                }
                return 0;
            }
            if (Name == "Elmar")
            {
                Health = MaxHealth;
                Mana = MaxMana;
                Endurance = MaxEndurance;

                if (ElmarChance == 0)
                {
                    SpawnAdd(85665, 1, 612302, 564019, 8048, 1, 0);
                }
                else if (ElmarChance == 1)
                {
                    SpawnAdd(85665, 1, 596336, 561331, 8077, 3437, 0);
                }
                else if (ElmarChance == 2)
                {
                    SpawnAdd(85665, 1, 584660, 589700, 7808, 2315, 0);
                }
                else if (ElmarChance == 3)
                {
                    SpawnAdd(85665, 1, 569413, 590416, 8053, 173, 0);
                }
                else if (ElmarChance == 4)
                {
                    SpawnAdd(85665, 1, 572191, 614411, 8935, 2535, 0);
                }
                else if (ElmarChance == 5)
                {
                    SpawnAdd(85665, 1, 605264, 607003, 8457, 3626, 0);
                }
                return 0;
            }
            */
            /*
            //Weihnachtsevent 2017
            if (Name == "tief Santa")
            {
                Health = MaxHealth;
                Mana = MaxMana;
                Endurance = MaxEndurance;

                if (tiefSantaChance == 0)
                {
                    SpawnAdd(856662, 1, 522344, 479075, 10162, 3915, 0);
                }
                else if (tiefSantaChance == 1)
                {
                    SpawnAdd(856662, 1, 531724, 470478, 9732, 1477, 0);
                }
                else if (tiefSantaChance == 2)
                {
                    SpawnAdd(856662, 1, 532107, 455722, 10492, 1330, 0);
                }
                else if (tiefSantaChance == 3)
                {
                    SpawnAdd(856662, 1, 506248, 450882, 8752, 3263, 0);
                }
                else if (tiefSantaChance == 4)
                {
                    SpawnAdd(856662, 1, 548045, 460080, 9128, 483, 0);
                }
                return 0;
            }
            */

            //Heal this mob, move it to the spawnlocation
            Health = MaxHealth;
            Mana = MaxMana;
            Endurance = MaxEndurance;
            int origSpawnX = m_spawnPoint.X;
            int origSpawnY = m_spawnPoint.Y;

            X = m_spawnPoint.X;
            Y = m_spawnPoint.Y;
            Z = m_spawnPoint.Z;
            Heading = m_spawnHeading;
            AddToWorld();
            m_spawnPoint.X = origSpawnX;
            m_spawnPoint.Y = origSpawnY;

            return 0;
        }

        /// <summary>
        /// Callback timer for health regeneration
        /// </summary>
        /// <param name="selfRegenerationTimer">the regeneration timer</param>
        /// <returns>the new interval</returns>
        protected override int HealthRegenerationTimerCallback(RegionTimer selfRegenerationTimer)
        {
            int period = m_healthRegenerationPeriod;
            if (!InCombat)
            {
                int oldPercent = HealthPercent;
                period = base.HealthRegenerationTimerCallback(selfRegenerationTimer);
                if (oldPercent != HealthPercent)
                    BroadcastUpdate();
            }
            return (Health < MaxHealth) ? period : 0;
        }

        /// <summary>
        /// The chance for a critical hit
        /// </summary>
        public override int AttackCriticalChance(InventoryItem weapon)
        {
            if (weapon != null && weapon.Item_Type == Slot.RANGED && RangedAttackType == eRangedAttackType.Critical)
                return 0; // no crit damage for crit shots

            return GetModified(eProperty.CriticalMeleeHitChance);
        }

        /// <summary>
        /// Stop attacking and following, but stay in attack mode (e.g. in
        /// order to cast a spell instead).
        /// </summary>
        public virtual void HoldAttack()
        {
            if (m_attackAction != null)
                m_attackAction.Stop();
            StopFollowing();
        }

        /// <summary>
        /// Continue a previously started attack.
        /// </summary>
        public virtual void ContinueAttack(GameObject target)
        {
            if (m_attackAction != null && target != null)
            {
                Follow(target, STICKMINIMUMRANGE, MaxDistance);
                m_attackAction.Start(1);
            }
        }

        /// <summary>
        /// Stops all attack actions, including following target
        /// </summary>
        public override void StopAttack()
        {
            if (this.InCombat == false && BodyType != 37 || this.InCombat == false && BodyType == 37 && IsMoving == false
                || this is GameKeepGuard || this is GamePet || Brain is ControlledNpcBrain || Brain is CommanderBrain || this is TheurgistPet || this is NecromancerPet)
            {
                base.StopAttack();
                StopFollowing();

                // Tolakram: If npc has a distance weapon it needs to be made active after attack is stopped
                if (Inventory != null && Inventory.GetItem(eInventorySlot.DistanceWeapon) != null && ActiveWeaponSlot != eActiveWeaponSlot.Distance)
                    SwitchWeapon(eActiveWeaponSlot.Distance);
            }
        }

        /// <summary>
        /// Stops all attack actions, including following target
        /// </summary>
        public virtual void StopAttackReturningHome()
        {

            {
                base.StopAttack();
                StopFollowing();

                // Tolakram: If npc has a distance weapon it needs to be made active after attack is stopped
                if (Inventory != null && Inventory.GetItem(eInventorySlot.DistanceWeapon) != null && ActiveWeaponSlot != eActiveWeaponSlot.Distance)
                    SwitchWeapon(eActiveWeaponSlot.Distance);
            }
        }


        
        //clear there Gainer list if npc is out of combat!
        public virtual void ClearGainerList(GameNPC npc)
        {
            if (npc.InCombat == false && npc.IsAlive && npc != null && npc is GameNPC)
            {

                   _XPGainers.Clear();
               
               // log.WarnFormat("liste gelöscht von {0}", npc.Name);
            }
        }
        
        private GamePlayer killerPlayer = null;
        

        /// <summary>
        /// This method is called to drop loot after this mob dies
        /// </summary>
        /// <param name="killer">The killer</param>
        public virtual void DropLoot(GameObject killer)
        {
                     
            ArrayList droplist = new ArrayList();
            ArrayList autolootlist = new ArrayList();
            ArrayList aplayer = new ArrayList();


            if (killer.ObjectState == eObjectState.Active && killer != this)
            {
              


                if (killer != null)
                {


                    var gainers = _XPGainers.ToArray();


                    if (gainers.Length == 0)
                        return;

                    ItemTemplate[] lootTemplates = LootMgr.GetLoot(this, killer);

                    foreach (ItemTemplate lootTemplate in lootTemplates)
                    {
                        if (lootTemplate == null) continue;

                        GameStaticItem loot;
                        if (GameMoney.IsItemMoney(lootTemplate.Name))
                        {
                            long value = lootTemplate.Price;


                            if (killer is GameNPC)
                            {
                                if (killer is GameNPC && ((killer as GameNPC).Brain is IControlledBrain) && ((killer as GameNPC).Brain is MobPetBrain == false && (killer as GameNPC).Brain is HoundsPetBrain == false))
                                    killerPlayer = ((killer as GameNPC).Brain as IControlledBrain).GetPlayerOwner();
                                else return;
                            }


                            if (killer is GamePlayer)
                            {
                                killerPlayer = killer as GamePlayer;
                            }
                            //[StephenxPimentel] - Zone Bonus XP Support
                            if (Properties.ENABLE_ZONE_BONUSES)
                            {
                                int zoneBonus = 0;

                                if (ZoneBonus.GetCoinBonus(killerPlayer) > 0 && value > 0)
                                {

                                    if (killerPlayer.Group != null || killerPlayer.MemberIsInBG(killerPlayer) && Properties.BATTLEGROUP_LOOT)
                                    {
                                        BattleGroup battleGroup = killerPlayer.TempProperties.getProperty<BattleGroup>(BattleGroup.BATTLEGROUP_PROPERTY, null);

                                        if (battleGroup != null && Properties.BATTLEGROUP_LOOT)
                                        {
                                            //BattleGroup
                                            foreach (GamePlayer players in battleGroup.Members.Keys)
                                            {
                                                if (players.IsWithinRadius(killer, 5000) == false)
                                                    return;
                                                if (players.IsWithinRadius(this, 5000) == false)
                                                    continue;

                                                zoneBonus = (((int)value * ZoneBonus.GetCoinBonus(players) / 100) / battleGroup.Members.Count);

                                                long amount = (long)(zoneBonus * Properties.MONEY_DROP);

                                                players.AddMoney(amount, ZoneBonus.GetBonusMessage(players, (int)(zoneBonus * Properties.MONEY_DROP), ZoneBonus.eZoneBonusType.COIN), eChatType.CT_Important, eChatLoc.CL_SystemWindow);

                                                if (Keeps.KeepBonusMgr.RealmHasBonus(eKeepBonusType.Coin_Drop_5, (eRealm)killer.Realm))
                                                    value += (((value / 100) * 5) / players.Group.MemberCount);
                                                else if (Keeps.KeepBonusMgr.RealmHasBonus(eKeepBonusType.Coin_Drop_3, (eRealm)killer.Realm))
                                                    value += (((value / 100) * 5) / players.Group.MemberCount);

                                                //this will need to be changed when the ML for increasing money is added
                                                if (value != lootTemplate.Price)
                                                {

                                                    if (players != null)
                                                        players.Out.SendMessage(LanguageMgr.GetTranslation(players.Client, "GameNPC.DropLoot.AdditionalMoney", Money.GetString(value - lootTemplate.Price)), eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
                                                }

                                                //Mythical Coin bonus property (Can be used for any equipped item, bonus 235)

                                                if (players.GetModified(eProperty.MythicalCoin) > 0)
                                                {
                                                    value += (value * players.GetModified(eProperty.MythicalCoin)) / 100;
                                                    players.Out.SendMessage(LanguageMgr.GetTranslation(players.Client, "GameNPC.DropLoot.ItemAdditionalMoney", Money.GetString(value - lootTemplate.Price)), eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
                                                }
                                            }
                                        }
                                        else


                                            foreach (GamePlayer players in killerPlayer.Group.GetPlayersInTheGroup())
                                            {
                                                if (players.IsWithinRadius(this, 5000) == false)
                                                    continue;

                                                zoneBonus = (((int)value * ZoneBonus.GetCoinBonus(players) / 100) / players.Group.MemberCount);
                                                long amount = (long)(zoneBonus * Properties.MONEY_DROP);

                                                players.AddMoney(amount, ZoneBonus.GetBonusMessage(players, (int)(zoneBonus * Properties.MONEY_DROP), ZoneBonus.eZoneBonusType.COIN), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                                if (KeepBonusMgr.RealmHasBonus(eKeepBonusType.Coin_Drop_5, (eRealm)killer.Realm))
                                                    value += (((value / 100) * 5) / players.Group.MemberCount);
                                                else if (KeepBonusMgr.RealmHasBonus(eKeepBonusType.Coin_Drop_3, (eRealm)killer.Realm))
                                                    value += (((value / 100) * 5) / players.Group.MemberCount);

                                                //this will need to be changed when the ML for increasing money is added
                                                if (value != lootTemplate.Price)
                                                {

                                                    if (players != null)
                                                        players.Out.SendMessage(LanguageMgr.GetTranslation(players.Client, "GameNPC.DropLoot.AdditionalMoney", Money.GetString(value - lootTemplate.Price)), eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
                                                }

                                                //Mythical Coin bonus property (Can be used for any equipped item, bonus 235)

                                                if (players.GetModified(eProperty.MythicalCoin) > 0)
                                                {
                                                    value += (value * players.GetModified(eProperty.MythicalCoin)) / 100;
                                                    players.Out.SendMessage(LanguageMgr.GetTranslation(players.Client, "GameNPC.DropLoot.ItemAdditionalMoney", Money.GetString(value - lootTemplate.Price)), eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
                                                }
                                            }
                                    }
                                    else

                                    {
                                        zoneBonus = (((int)value * ZoneBonus.GetCoinBonus(killerPlayer) / 100));
                                        long amount = (long)(zoneBonus * Properties.MONEY_DROP);

                                        killerPlayer.AddMoney(amount, ZoneBonus.GetBonusMessage(killerPlayer, (int)(zoneBonus * Properties.MONEY_DROP), ZoneBonus.eZoneBonusType.COIN), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                       
                                        if (KeepBonusMgr.RealmHasBonus(eKeepBonusType.Coin_Drop_5, (eRealm)killer.Realm))
                                            value += (value / 100) * 5;
                                        else if (KeepBonusMgr.RealmHasBonus(eKeepBonusType.Coin_Drop_3, (eRealm)killer.Realm))
                                            value += (value / 100) * 3;

                                        //this will need to be changed when the ML for increasing money is added
                                        if (value != lootTemplate.Price)
                                        {

                                            if (killerPlayer != null)
                                                killerPlayer.Out.SendMessage(LanguageMgr.GetTranslation(killerPlayer.Client, "GameNPC.DropLoot.AdditionalMoney", Money.GetString(value - lootTemplate.Price)), eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
                                        }

                                        //Mythical Coin bonus property (Can be used for any equipped item, bonus 235)
                                        if (killer is GamePlayer)
                                        {

                                            if (killerPlayer.GetModified(eProperty.MythicalCoin) > 0)
                                            {
                                                value += (value * killerPlayer.GetModified(eProperty.MythicalCoin)) / 100;
                                                killerPlayer.Out.SendMessage(LanguageMgr.GetTranslation(killerPlayer.Client, "GameNPC.DropLoot.ItemAdditionalMoney", Money.GetString(value - lootTemplate.Price)), eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
                                            }
                                        }
                                    }
                                }
                            }



                            loot = new GameMoney(value, this)
                            {
                                Name = lootTemplate.Name,
                                Model = (ushort)lootTemplate.Model
                            };
                        }
                        else if (lootTemplate.Name.StartsWith("scroll|"))
                        {
                            String[] scrollData = lootTemplate.Name.Split('|');
                            String artifactID = scrollData[1];
                            int pageNumber = UInt16.Parse(scrollData[2]);
                            loot = ArtifactMgr.CreateScroll(artifactID, pageNumber);
                            loot.X = X;
                            loot.Y = Y;
                            loot.Z = Z;
                            loot.Heading = Heading;
                            loot.CurrentRegion = CurrentRegion;
                            (loot as WorldInventoryItem).Item.IsCrafted = false;
                            (loot as WorldInventoryItem).Item.Creator = Name;
                        }
                        else
                        {

                            InventoryItem invitem;

                            if (lootTemplate is ItemUnique)
                            {
                                {
                                    try
                                    {
                                        GameServer.Database.AddObject(lootTemplate);
                                    }
                                    finally
                                    {
                                        invitem = GameInventoryItem.Create(lootTemplate as ItemUnique);
                                    }


                                }

                            }
                            else
                            {
                                //log.ErrorFormat("$$Loot Realm = {0}", lootTemplate.Realm);
                                invitem = GameInventoryItem.Create(lootTemplate);
                                //log.ErrorFormat("$$Create Loot Realm = {0}", invitem.Realm);
                                // log.ErrorFormat("WorldInventoryItem11 Realm = {0}", invitem.Realm);
                                //OKI
                            }

                            eRealm itemRealm = eRealm.None;
                            switch (invitem.Realm)
                            {
                                case 1:
                                    {
                                        itemRealm = eRealm.Albion;
                                        break;
                                    }
                                case 2:
                                    {
                                        itemRealm = eRealm.Midgard;
                                        break;
                                    }
                                case 3:
                                    {
                                        itemRealm = eRealm.Hibernia;
                                        break;
                                    }

                                default:
                                    {
                                        itemRealm = eRealm.None; break;
                                    }
                            }
                            loot = new WorldInventoryItem(invitem)
                            {
                                Realm = itemRealm,
                                X = X,
                                Y = Y,
                                Z = Z,
                                Heading = Heading,
                                CurrentRegion = CurrentRegion
                            };
                            //log.ErrorFormat("WorldInventoryItem22 Realm = {0}", loot.Realm);
                            (loot as WorldInventoryItem).Item.IsCrafted = false;
                            (loot as WorldInventoryItem).Item.Creator = Name;
                            
                            // This may seem like an odd place for this code, but loot-generating code further up the line
                            // is dealing strictly with ItemTemplate objects, while you need the InventoryItem in order
                            // to be able to set the Count property.
                            // Converts single drops of loot with PackSize > 1 (and MaxCount >= PackSize) to stacks of Count = PackSize
                            // if not, we use the count that is set in the loottemplate for the droped item
                            if (((WorldInventoryItem)loot).Item.PackSize > 1 && ((WorldInventoryItem)loot).Item.MaxCount >= ((WorldInventoryItem)loot).Item.PackSize)
                            {


                                ((WorldInventoryItem)loot).Item.Count = ((WorldInventoryItem)loot).Item.PackSize;
                            }
                            else
                            {
                                IList<LootTemplate> lootTMPL = GameServer.Database.SelectObjects<LootTemplate>("`TemplateName` = @TemplateName", new QueryParameter("@TemplateName", Name));

                                if (lootTMPL != null)
                                {
                                    foreach (LootTemplate loottemplate in lootTMPL)
                                    {
                                       // log.ErrorFormat("loot name: {0}, item name; {1}", loots.ItemTemplateID, ((WorldInventoryItem)loot).Item.Id_nb.ToLower());

                                        if (loottemplate.Chance < 100 && loottemplate.Count > 1 && loottemplate.ItemTemplateID.ToLower() == ((WorldInventoryItem)loot).Item.Id_nb.ToLower())
                                        {
                                            //log.ErrorFormat("LOOT FOUND: Count: {0}", loottemplate.Count);
                                            ((WorldInventoryItem)loot).Item.Count = loottemplate.Count;
                                        }
                                    }
                                }
                            }
                        }


                        GameObject gainer = null;
                        GamePlayer playerAttacker = null;
                        //foreach (var gainer in XPGainers.Keys) 
                        foreach (var g in gainers) // XPGainers.Keys)
                        {


                            //remove wrong owners
                            if (g.Key is GamePlayer)
                            {
                                if (loot.Level >= g.Key.Level && loot.Realm != g.Key.Realm || g.Key.IsWithinRadius(loot, 4000) == false)
                                {

                                    XPGainers.TryRemove(this, out float dummy);

                                    if (dummy >= 0)
                                    {
                                        //log.ErrorFormat("XPGainers aus liste entferntee: {0}", g.Key.Name);
                                    }

                                }
                            }

                            if (g.Key is GamePlayer)
                            {

                                gainer = g.Key as GamePlayer;
                                GamePlayer groupgainer = g.Key as GamePlayer;
                                if (groupgainer.Group != null)
                                {

                                    foreach (GamePlayer gPlayer in groupgainer.Group.GetPlayersInTheGroup())
                                    {
                                        if (gPlayer != null && gPlayer != g.Key)
                                        {
                                            gainer = gPlayer;
                                            if (gPlayer == gPlayer.Group.Leader)
                                                loot.AddOwner(gPlayer);
                                        }

                                    }
                                }
                                playerAttacker = gainer as GamePlayer;

                                /*
                                if (loot.Realm == 0)
                                {
                                    loot.Realm = ((GamePlayer)gainer).Realm;

                                }
                                */
                                //log.ErrorFormat("$$m_xpGainers2 Realm = {0}, loot Realm: {1}", gainer.Realm, loot.Realm);
                            }

                            ///////////////////////////////ADD OWNER!!!!!////////////////////////////////
                            if (gainer != null && (gainer.Realm == loot.Realm || loot.Realm == 0))
                            {
                                GameLiving npc = gainer as GameLiving;
                                //log.ErrorFormat("add gainer = {0}, loot Realm: {1}, gainer Realm: {2}", gainer.Name, loot.Realm, gainer.Realm);

                                loot.AddOwner(gainer);

                                if (npc is GameNPC)
                                {
                                    IControlledBrain brain = ((GameNPC)gainer).Brain as IControlledBrain;
                                    if (brain != null)
                                    {
                                        playerAttacker = brain.GetPlayerOwner();
                                        loot.AddOwner(brain.GetPlayerOwner());
                                    }
                                }
                                if (loot.Realm == 0 && playerAttacker != null)
                                {
                                    loot.Realm = playerAttacker.Realm;

                                }
                            }
                            ///////////////////////////////ADD OWNER!!!!!////////////////////////////////
                        }
                        if (playerAttacker == null) return; // no loot if mob kills another mob


                        if (playerAttacker is GamePlayer && playerAttacker.Group != null)
                        {
                            foreach (GamePlayer player in playerAttacker.Group.GetPlayersInTheGroup())
                            {
                                if (player.Realm == loot.Realm || loot.Realm == 0)
                                {
                                    droplist.Add(loot.GetName(1, false));

                                }
                            }
                        }
                        else
                        {

                            droplist.Add(loot.GetName(1, false));
                        }

                        loot.AddToWorld();


                        if (gainer is GamePlayer)
                        {
                            //log.ErrorFormat("XP Gainer: {0} ", gainer.Name);
                            GamePlayer player = gainer as GamePlayer;

                            if (player.Autoloot && IsWithinRadius(player, 1500) && (loot.Realm == player.Realm || loot.Realm == eRealm.None)) // should be large enough for most casters to autoloot
                            {

                                // if (player.Group == null || (player.Group != null && player == player.Group.Leader) || player.GetBattleGroupLeader(player) != null && player == player.GetBattleGroupLeader(player) && GS.ServerProperties.Properties.BATTLEGROUP_LOOT)

                                //log.ErrorFormat("Leader pick Loot - Name: {0} Loot name: {1}", player.Name, loot.Name);
                                aplayer.Add(player);
                                autolootlist.Add(loot);
                            }

                        }

                    }
                    //_XPGainers.Clear();
                }
            }
            //log.Error("DropLoot ok2!!");
            BroadcastLoot(droplist);

            if (autolootlist.Count > 0)
            {
                foreach (GameObject obj in autolootlist)
                {

                    foreach (GamePlayer player in aplayer)
                    {
                        if (obj != null && player != null)
                        {
                            //log.ErrorFormat("Pick up item Realm = {0}", obj.Realm);
                            player.PickupObject(obj, true);
                            //log.Error("DropLoot ok3!!");
                            break;
                        }
                    }
                }
            }
         }


        /// <summary>
        /// The enemy is healed, so we add to the xp gainers list
        /// </summary>
        /// <param name="enemy"></param>
        /// <param name="healSource"></param>
        /// <param name="changeType"></param>
        /// <param name="healAmount"></param>
        public override void EnemyHealed(GameLiving enemy, GameObject healSource, GameLiving.eHealthChangeType changeType, int healAmount)
        {
            base.EnemyHealed(enemy, healSource, changeType, healAmount);

            if (changeType != eHealthChangeType.Spell)
                return;
            if (enemy == healSource)
                return;
            if (!IsAlive)
                return;

            var attackerLiving = healSource as GameLiving;
            if (attackerLiving == null)
                return;

            Group attackerGroup = attackerLiving.Group;
            if (attackerGroup != null)
            {

                // collect "helping" group players in range
                var xpGainers = attackerGroup.GetMembersInTheGroup()
                    .Where(l => this.IsWithinRadius(l, WorldMgr.MAX_EXPFORKILL_DISTANCE) && l.IsAlive && l.ObjectState == eObjectState.Active).ToArray();

                float damageAmount = (float)healAmount / xpGainers.Length;

                foreach (GameLiving living in xpGainers)
                {
                    // add players in range for exp to exp gainers
                    this.AddXPGainer(living, damageAmount);
                }
            }
            else
            {
                this.AddXPGainer(healSource, (float)healAmount);
            }
            //DealDamage needs to be called after addxpgainer!
        }

        #endregion

        #region Spell

        /// <summary>
        /// Whether or not the NPC can cast harmful spells
        /// at the moment.
        /// </summary>
        public override bool CanCastHarmfulSpells
        {
            get
            {
                if (!base.CanCastHarmfulSpells)
                    return false;

                IList<Spell> harmfulSpells = HarmfulSpells;

                foreach (Spell harmfulSpell in harmfulSpells)
                    if (harmfulSpell.CastTime == 0)
                        return true;

                return (harmfulSpells.Count > 0 && !IsBeingInterrupted);
            }
        }

        public override IList<Spell> HarmfulSpells
        {
            get
            {
                IList<Spell> harmfulSpells = new List<Spell>();

                foreach (Spell spell in Spells)
                    if (spell.IsHarmful)
                        harmfulSpells.Add(spell);

                return harmfulSpells;
            }
        }

        private List<Spell> m_spells = new List<Spell>();
        /// <summary>
        /// property of spell array of NPC
        /// </summary>
        public virtual IList Spells
        {
            get { return m_spells; }
            set { m_spells = value?.Cast<Spell>().ToList(); }
        }

        private IList m_styles = new ArrayList(1);
        /// <summary>
        /// The Styles for this NPC
        /// </summary>
        public IList Styles
        {
            get { return m_styles; }
            set { m_styles = value; }
        }

        private IList m_mobstylesid = new ArrayList(1);
        /// <summary>
        /// The StyleID for this NPC
        /// </summary>
        public IList MobStylesID
        {
            get { return m_mobstylesid; }
            set { m_mobstylesid = value; }
        }

        /// <summary>
        /// The Abilities for this NPC
        /// </summary>
        public Dictionary<string, Ability> Abilities
        {
            get
            {
                Dictionary<string, Ability> tmp = new Dictionary<string, Ability>();

                lock (m_lockAbilities)
                {
                    tmp = new Dictionary<string, Ability>(m_abilities);
                }

                return tmp;
            }
        }

        private SpellAction m_spellaction = null;
        /// <summary>
        /// The timer that controls an npc's spell casting
        /// </summary>
        public SpellAction SpellTimer
        {
            get { return m_spellaction; }
            set { m_spellaction = value; }
        }

        /// <summary>
        /// Callback after spell execution finished and next spell can be processed
        /// </summary>
        /// <param name="handler"></param>
        public override void OnAfterSpellCastSequence(ISpellHandler handler)
        {
            if (SpellTimer != null)
            {
                if (this == null || this.ObjectState != eObjectState.Active || !this.IsAlive || this.TargetObject == null || (this.TargetObject is GameLiving && this.TargetObject.ObjectState != eObjectState.Active || !(this.TargetObject as GameLiving).IsAlive))
                    SpellTimer.Stop();
                else
                {
                    int interval = 1500;

                    if (Brain != null)
                    {
                        interval = Math.Min(interval, Brain.ThinkInterval);
                    }

                    SpellTimer.Start(interval);
                }
            }
            if (m_runningSpellHandler != null)
            {
                //prevent from relaunch
                base.OnAfterSpellCastSequence(handler);
            }

            // Notify Brain of Cast Finishing.
            if (Brain != null)
                Brain.Notify(GameNPCEvent.CastFinished, this, new CastingEventArgs(handler));
        }
        /// <summary>
        /// The spell action of this living
        /// </summary>
        public class SpellAction : RegionAction
        {
            /// <summary>
            /// Constructs a new attack action
            /// </summary>
            /// <param name="owner">The action source</param>
            public SpellAction(GameLiving owner)
                : base(owner)
            {
            }

            /// <summary>
            /// Called on every timer tick
            /// </summary>
            protected override void OnTick()
            {
                GameNPC owner;
                if (m_actionSource != null && m_actionSource is GameNPC)
                    owner = (GameNPC)m_actionSource;
                else
                {
                    Stop();
                    return;
                }

                if (owner.TargetObject == null || !owner.AttackState)
                {
                    Stop();
                    return;
                }

                //If we started casting a spell, stop the timer and wait for
                //GameNPC.OnAfterSpellSequenceCast to start again
                if (owner.Brain is StandardMobBrain && ((StandardMobBrain)owner.Brain).CheckSpells(StandardMobBrain.eCheckSpellType.Offensive))
                {
                    Stop();
                    return;
                }
                else
                {
                    //If we aren't a distance NPC, lets make sure we are in range to attack the target!
                    if (owner.ActiveWeaponSlot != eActiveWeaponSlot.Distance && !owner.IsWithinRadius(owner.TargetObject, m_minimumAttackRange))
                        ((GameNPC)owner).Follow(owner.TargetObject, m_minimumAttackRange, STICKMAXIMUMRANGE);
                }

                if (owner.Brain != null)
                {
                    Interval = Math.Min(1500, owner.Brain.CastInterval);
                }
                else
                {
                    Interval = 1500;
                }
            }
        }

        private const string LOSTEMPCHECKER = "LOSTEMPCHECKER";
        private const string LOSCURRENTSPELL = "LOSCURRENTSPELL";
        private const string LOSCURRENTLINE = "LOSCURRENTLINE";
        private const string LOSSPELLTARGET = "LOSSPELLTARGET";


        /// <summary>
        /// Cast a spell, with optional LOS check
        /// </summary>
        /// <param name="spell"></param>
        /// <param name="line"></param>
        /// <param name="checkLOS"></param>
        public virtual void CastSpell(Spell spell, SpellLine line, bool checkLOS)
        {
            if (IsIncapacitated)
                return;

            if (checkLOS)
            {
                CastSpell(spell, line);
            }
            else
            {
                Spell spellToCast;

                if (line.KeyName == GlobalSpellsLines.Mob_Spells)
                {
                    // NPC spells will get the level equal to their caster
                    spellToCast = (Spell)spell.Clone();
                    if (spellToCast != null)
                    {
                        spellToCast.Level = Level;
                    }
                }
                else
                {
                    spellToCast = spell;
                }

                base.CastSpell(spellToCast, line);
            }
        }

        /// <summary>
        /// Cast a spell with LOS check to a player
        /// </summary>
        /// <param name="spell"></param>
        /// <param name="line"></param>
        public override void CastSpell(Spell spell, SpellLine line)
        {
            if (IsIncapacitated)
                return;

            if (m_runningSpellHandler != null || TempProperties.getProperty<Spell>(LOSCURRENTSPELL, null) != null)
            {
                return;
            }

            Spell spellToCast = null;

            if (line.KeyName == GlobalSpellsLines.Mob_Spells)
            {
                // NPC spells will get the level equal to their caster
                spellToCast = (Spell)spell.Clone();
                spellToCast.Level = Level;
            }
            else
            {
                spellToCast = spell;
            }

            // Let's do a few checks to make sure it doesn't just wait on the LOS check
            int tempProp = TempProperties.getProperty<int>(LOSTEMPCHECKER);

            if (tempProp <= 0)
            {
                GamePlayer LOSChecker = TargetObject as GamePlayer;
                if (LOSChecker == null)
                {
                    foreach (GamePlayer ply in GetPlayersInRadius((ushort)spell.Range))
                    {
                        if (ply != null)
                        {
                            LOSChecker = ply;
                            break;
                        }
                    }
                }

                if (LOSChecker == null)
                {
                    TempProperties.setProperty(LOSTEMPCHECKER, 0);
                    base.CastSpell(spellToCast, line);
                }
                else
                {
                    TempProperties.setProperty(LOSTEMPCHECKER, 10);
                    TempProperties.setProperty(LOSCURRENTSPELL, spellToCast);
                    TempProperties.setProperty(LOSCURRENTLINE, line);
                    TempProperties.setProperty(LOSSPELLTARGET, TargetObject);
                    LOSChecker.Out.SendCheckLOS(LOSChecker, this, new CheckLOSResponse(StartSpellAttackCheckLOS));
                }
            }
            else
            {
                TempProperties.setProperty(LOSTEMPCHECKER, tempProp - 1);
            }

            return;

        }



        /// <summary>
        /// Spell Check Los
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="response"></param>
        /// <param name="targetOID"></param>
        public void StartSpellAttackCheckLOS(GameLiving obj, ushort response, ushort targetOID)//geändert vorher: GamePlayer player
        {

            SpellLine line = TempProperties.getProperty<SpellLine>(LOSCURRENTLINE, null);
            Spell spell = TempProperties.getProperty<Spell>(LOSCURRENTSPELL, null);
            GameObject target = TempProperties.getProperty<GameObject>(LOSSPELLTARGET, null);
            GameObject lasttarget = TargetObject;

            TempProperties.removeProperty(LOSSPELLTARGET);
            TempProperties.removeProperty(LOSTEMPCHECKER);
            TempProperties.removeProperty(LOSCURRENTLINE);
            TempProperties.removeProperty(LOSCURRENTSPELL);
            TempProperties.setProperty(LOSTEMPCHECKER, 0);

            if ((response & 0x100) == 0x100 && line != null && spell != null)
            {
                TargetObject = target;

                GameLiving living = TargetObject as GameLiving;

                if (living != null && living.EffectList.GetOfType<NecromancerShadeEffect>() != null)
                {
                    if (living is GamePlayer && (living as GamePlayer).ControlledBrain != null)
                    {
                        TargetObject = (living as GamePlayer).ControlledBrain.Body;
                    }
                }

                base.CastSpell(spell, line);
                TargetObject = lasttarget;
            }
            else
            {
                Notify(GameLivingEvent.CastFailed, this, new CastFailedEventArgs(null, CastFailedEventArgs.Reasons.TargetNotInView));
            }
        }

        public virtual void PauseCurrentSpellCast(GameLiving attacker)
        {
            if (m_runningSpellHandler != null)
                m_runningSpellHandler.PauseCasting();
        }

        #endregion

        #region Notify

        /// <summary>
        /// Handle event notifications
        /// </summary>
        /// <param name="e">The event</param>
        /// <param name="sender">The sender</param>
        /// <param name="args">The arguements</param>
        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            base.Notify(e, sender, args);

            ABrain brain = Brain;
            if (brain != null)
                brain.Notify(e, sender, args);

            if (e == GameNPCEvent.ArriveAtTarget)
            {
                if (IsReturningToSpawnPoint)
                {
                    TurnTo(SpawnHeading);
                    IsReturningToSpawnPoint = false;
                }
            }
        }


        /// <summary>
        /// Handle triggers for ambient sentences
        /// </summary>
        /// <param name="action">The trigger action</param>
        /// <param name="npc">The NPC to handle the trigger for</param>
        public void FireAmbientSentence(eAmbientTrigger trigger, GameLiving living = null)
        {
            if (IsSilent || ambientTexts == null || ambientTexts.Count == 0) return;
            if (trigger == eAmbientTrigger.interact && living == null) return;
            List<MobXAmbientBehaviour> mxa = (from i in ambientTexts where i.Trigger == trigger.ToString() select i).ToList();
            if (mxa.Count == 0) return;

            // grab random sentence
            var chosen = mxa[Util.Random(mxa.Count - 1)];
            if (!Util.Chance(chosen.Chance)) return;

            string controller = string.Empty;
            if (Brain is IControlledBrain)
            {
                GamePlayer playerOwner = (Brain as IControlledBrain).GetPlayerOwner();
                if (playerOwner != null)
                    controller = playerOwner.Name;
            }

            string text = chosen.Text.Replace("{sourcename}", Name).Replace("{targetname}", living == null ? string.Empty : living.Name).Replace("{controller}", controller);

            if (chosen.Emote != 0)
            {
                Emote((eEmote)chosen.Emote);
            }

            // issuing text
            if (living is GamePlayer)
                text = text.Replace("{class}", (living as GamePlayer).CharacterClass.Name).Replace("{race}", (living as GamePlayer).RaceName);
            if (living is GameNPC)
                text = text.Replace("{class}", "NPC").Replace("{race}", "NPC");

            // for interact text we pop up a window
            if (trigger == eAmbientTrigger.interact)
            {
                this.TurnTo(living);
                (living as GamePlayer).Out.SendMessage(text, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                return;
            }

            // broadcasted , yelled or talked ?
            if (chosen.Voice.StartsWith("b"))
            {
                //foreach (GamePlayer player in CurrentRegion.GetPlayersInRadius(X, Y, Z, 25000, false))
                foreach (GamePlayer player in CurrentRegion.GetPlayersInRadius(X, Y, Z, 25000, false))
                {

                    player.Out.SendMessage(text, eChatType.CT_Broadcast, eChatLoc.CL_ChatWindow);
                }
                return;
            }
            if (chosen.Voice.StartsWith("y"))
            {
                Yell(text);
                return;
            }
            Say(text);
        }
        #endregion

        #region ControlledNPCs

        public override void SetControlledBrain(IControlledBrain controlledBrain)
        {
            if (ControlledBrain == null)
                InitControlledBrainArray(1);

            ControlledBrain = controlledBrain;
        }
        /// <summary>
        /// Gets the controlled object of this NPC
        /// </summary>
        public override IControlledBrain ControlledBrain
        {
            get
            {
                if (m_controlledBrain == null) return null;
                return m_controlledBrain[0];
            }
        }

        /// <summary>
        /// Gets the controlled array of this NPC
        /// </summary>
        public IControlledBrain[] ControlledNpcList
        {


            get { return m_controlledBrain; }
        }

        /// <summary>
        /// Adds a pet to the current array of pets
        /// </summary>
        /// <param name="controlledNpc">The brain to add to the list</param>
        /// <returns>Whether the pet was added or not</returns>
        public virtual bool AddControlledNpc(IControlledBrain controlledNpc)
        {
            return true;
        }

        /// <summary>
        /// Removes the brain from
        /// </summary>
        /// <param name="controlledNpc">The brain to find and remove</param>
        /// <returns>Whether the pet was removed</returns>
        public virtual bool RemoveControlledNpc(IControlledBrain controlledNpc)
        {
            return true;
        }

        #endregion

        /// <summary>
        /// Whether this NPC is available to add on a fight.
        /// </summary>
        public virtual bool IsAvailable
        {
            get { return !(Brain is IControlledBrain) && !InCombat; }
        }

        /// <summary>
        /// Whether this NPC is aggressive.
        /// </summary>
        public virtual bool IsAggressive
        {
            get
            {
                ABrain brain = Brain;
                return (brain == null) ? false : (brain is IOldAggressiveBrain);
            }
        }

        /// <summary>
        /// Whether this NPC is a friend or not.
        /// </summary>
        /// <param name="npc">The NPC that is checked against.</param>
        /// <returns></returns>
        public virtual bool IsFriend(GameNPC npc)
        {
            GameNPC owner = null;

            if (Faction == null)
                return false;


            //NPC Controlled Pets
            if (npc.Brain != null && npc.Brain is IControlledBrain && (npc.Brain as IControlledBrain).Owner is GameNPC)
            {
                owner = (npc.Brain as IControlledBrain).Owner as GameNPC;
            }
            else
            {
                owner = npc;
            }
            if (owner != null && owner.ObjectState == eObjectState.Active)
            {
                if (owner.Faction == null)
                {
                    return false;
                }
            }
            else
                return false;


            return (npc.Faction == Faction || (Faction.FriendFactions?.Contains(npc.Faction) ?? false));
            //return (owner.Faction == Faction || Faction.FriendFactions.Contains(owner.Faction));
        }

        /// <summary>
        /// Broadcast loot to the raid.
        /// </summary>
        /// <param name="dropMessages">List of drop messages to broadcast.</param>
        protected virtual void BroadcastLoot(ArrayList droplist)
        {
            if (droplist.Count > 0)
            {
                String lastloot;
                foreach (GamePlayer player in GetPlayersInRadius(1000))
                {
                    lastloot = "";
                    foreach (string str in droplist)
                    {
                        // Suppress identical messages (multiple item drops).
                        if (str != lastloot)
                        {
                            player.Out.SendMessage(String.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameNPC.DropLoot.Drops",
                            GetName(0, true, player.Client.Account.Language, this), str)), eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
                            lastloot = str;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Gender of this NPC.
        /// </summary>
        public override eGender Gender { get; set; }

        public GameNPC Copy()
        {
            return Copy(null);
        }


        /// <summary>
        /// Create a copy of the GameNPC
        /// </summary>
        /// <param name="copyTarget">A GameNPC to copy this GameNPC to (can be null)</param>
        /// <returns>The GameNPC this GameNPC was copied to</returns>
        public GameNPC Copy(GameNPC copyTarget)
        {
            if (copyTarget == null)
                copyTarget = new GameNPC();

            copyTarget.TranslationId = TranslationId;
            copyTarget.BlockChance = BlockChance;
            copyTarget.BodyType = BodyType;
            copyTarget.CanUseLefthandedWeapon = CanUseLefthandedWeapon;
            copyTarget.Charisma = Charisma;
            copyTarget.Constitution = Constitution;
            copyTarget.CurrentRegion = CurrentRegion;
            copyTarget.Dexterity = Dexterity;
            copyTarget.Empathy = Empathy;
            copyTarget.Endurance = Endurance;
            copyTarget.EquipmentTemplateID = EquipmentTemplateID;
            copyTarget.EvadeChance = EvadeChance;
            copyTarget.Faction = Faction;
            copyTarget.Flags = Flags;
            copyTarget.GuildName = GuildName;
            copyTarget.Heading = Heading;
            copyTarget.Intelligence = Intelligence;
            copyTarget.IsCloakHoodUp = IsCloakHoodUp;
            copyTarget.IsCloakInvisible = IsCloakInvisible;
            copyTarget.IsHelmInvisible = IsHelmInvisible;
            copyTarget.LeftHandSwingChance = LeftHandSwingChance;
            copyTarget.Level = Level;
            copyTarget.LoadedFromScript = LoadedFromScript;
            copyTarget.MaxSpeedBase = MaxSpeedBase;
            copyTarget.MeleeDamageType = MeleeDamageType;
            copyTarget.Model = Model;
            copyTarget.Name = Name;
            copyTarget.NPCTemplate = NPCTemplate;
            copyTarget.ParryChance = ParryChance;
            copyTarget.PathID = PathID;
            copyTarget.PathingNormalSpeed = PathingNormalSpeed;
            copyTarget.Quickness = Quickness;
            copyTarget.Piety = Piety;
            copyTarget.Race = Race;
            copyTarget.Realm = Realm;
            copyTarget.RespawnInterval = RespawnInterval;
            copyTarget.RoamingRange = RoamingRange;
            copyTarget.Size = Size;
            copyTarget.SaveInDB = SaveInDB;
            copyTarget.Strength = Strength;
            copyTarget.TetherRange = TetherRange;
            copyTarget.MaxDistance = MaxDistance;
            copyTarget.X = X;
            copyTarget.Y = Y;
            copyTarget.Z = Z;
            copyTarget.OwnerID = OwnerID;
            copyTarget.PackageID = PackageID;
            copyTarget.IsStyleAllowed = IsStyleAllowed;
            copyTarget.m_yellRange = YellRange;
            copyTarget.m_meleeAttackRange = MeleeAttackRange;
            copyTarget.m_npcIsFish = NPCIsFish;
            copyTarget.m_npcIsSeeingAll = NpcIsSeeingAll;
            copyTarget.m_ControlledCount = ControlledCount;
            copyTarget.MobStyleID = MobStyleID;
            if (Abilities != null && Abilities.Count > 0)
            {
                foreach (Ability targetAbility in Abilities.Values)
                {
                    if (targetAbility != null)
                        copyTarget.AddAbility(targetAbility);
                }
            }

            ABrain brain = null;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                brain = (ABrain)assembly.CreateInstance(Brain.GetType().FullName, true);
                if (brain != null)
                    break;
            }

            if (brain == null)
            {
                log.Warn("GameNPC.Copy():  Unable to create brain:  " + Brain.GetType().FullName + ", using StandardMobBrain.");
                brain = new StandardMobBrain();
            }

            StandardMobBrain newBrainSMB = brain as StandardMobBrain;
            StandardMobBrain thisBrainSMB = this.Brain as StandardMobBrain;

            if (newBrainSMB != null && thisBrainSMB != null)
            {
                newBrainSMB.AggroLevel = thisBrainSMB.AggroLevel;
                newBrainSMB.AggroRange = thisBrainSMB.AggroRange;
            }

            copyTarget.SetOwnBrain(brain);

            if (Inventory != null && Inventory.AllItems.Count > 0)
            {
                GameNpcInventoryTemplate inventoryTemplate = Inventory as GameNpcInventoryTemplate;

                if (inventoryTemplate != null)
                    copyTarget.Inventory = inventoryTemplate.CloneTemplate();
            }

            if (Spells != null && Spells.Count > 0)
                copyTarget.Spells = new List<Spell>(Spells.Cast<Spell>());

            if (Styles != null && Styles.Count > 0)
                copyTarget.Styles = new ArrayList(Styles);

            if (copyTarget.Inventory != null)
                copyTarget.SwitchWeapon(ActiveWeaponSlot);

            return copyTarget;
        }


        /// <summary>
        /// Constructs a NPC
        /// </summary>
        public GameNPC()
            : base()
        {
            Level = 1; // health changes when GameNPC.Level changes
            m_Realm = 0;
            m_name = "new mob";
            m_model = 408;
            //Fill the living variables
            //			CurrentSpeed = 0; // cause position addition recalculation
            MaxSpeedBase = 225;
            GuildName = "";

            m_followTarget = new WeakRef(null);

            m_size = 50; //Default size
            TargetPosition = new Point3D();
            m_followMinDist = 100;
            m_followMaxDist = 3000;
            m_flags = 0;
            m_maxdistance = 0;
            m_roamingRange = 0; // default to non roaming - tolakram
            m_ownerID = "";

            if (m_spawnPoint == null)
                m_spawnPoint = new Point3D();

            //m_factionName = "";
            LinkedFactions = new ArrayList(1);
            if (m_ownBrain == null)
            {
                m_ownBrain = new StandardMobBrain
                {
                    Body = this
                };
            }
        }

        readonly INpcTemplate m_template = null;

        /// <summary>
        /// create npc from template
        /// </summary>
        /// <param name="template">template of generator</param>
        public GameNPC(INpcTemplate template)
            : this()
        {
            if (template == null) return;

            // save the original template so we can do calculations off the original values
            m_template = template;

            LoadTemplate(template);
        }

        // camp bonus
        private double m_campBonus = 1;
        /// <summary>
        /// gets/sets camp bonus experience this gameliving grants
        /// </summary>
        public virtual double CampBonus
        {
            get
            {
                return m_campBonus;
            }
            set
            {
                m_campBonus = value;
            }
        }

        public INpcTemplate Template => m_template;

        #region Ork Spawn


        //Ork Spawn for Albion SI
        public void OrkSpawn()
        {
            Body = this;
            long timer;
            int distance = 0;
            if (Body != null && Body.IsAlive && Body.InCombat == false && Body.Name == "Thrawn ogre squad-leader")
            {
                Region region = Body.CurrentRegion;

                if (region == null)
                    return;

                if (Body.LastCombatTickPvE == 0)
                {
                    return;
                }
                distance = Body.GetDistanceTo(Body.SpawnPoint);
                //log.ErrorFormat("Distance to Spawnpoint: {0}", distance);
               //Time Sec left
               timer = (((Body.LastCombatTickPvE + 150 * 1000) / 1000) - (region.Time / 1000));
                if (timer < 0)
                    timer = 0;

                //log.ErrorFormat("Rest time = {0}, is Returnit: {1} ", timer, Body.IsReturningToSpawnPoint);
                if (timer >= 0 && timer < 8)
                {

                   // log.ErrorFormat("mobs können nun respawnen: {0}, is Returnit: {1}  ", timer, Body.IsReturningToSpawnPoint);
                   
                    Body.StopMoving();
                    
                    if (TheOrkSpawn)
                        Body.Emote(eEmote.Wave);
                    TheOrkSpawn = false;
                    return;
                }

                if (TheOrkSpawn == false && distance <= 10 && Body.Name == "Thrawn ogre squad-leader")
                {
                    {
                        //Spawn
                        OrkSpawnAdd(3015, 50, 368726, 506879, 3080, 1836, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 368119, 507094, 3080, 2014, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 367526, 507081, 3080, 2312, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 367443, 507274, 3084, 2312, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 368125, 507223, 3080, 2014, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 507038, 507223, 3080, 2439, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 366860, 507086, 3080, 2417, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 367587, 506939, 3080, 2312, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 368260, 507161, 3080, 2014, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 368130, 507315, 3080, 2014, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 367037, 507195, 3081, 2439, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 367229, 506916, 3080, 2444, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 368192, 506997, 3080, 2014, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 366958, 506934, 3080, 2439, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 367671, 507163, 3080, 2312, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 367678, 506906, 3080, 2284, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 368592, 507094, 3080, 1836, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 367736, 507011, 3080, 2312, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 368267, 507308, 3080, 2014, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 368540, 506942, 3080, 1836, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 368254, 507049, 3080, 2014, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 368814, 507155, 3080, 1836, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 368633, 507217, 3080, 1836, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 367596, 507339, 3080, 2312, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 367042, 506812, 3080, 2439, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 368751, 506967, 3080, 1836, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 368583, 506814, 3080, 1846, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 367160, 506789, 3080, 2307, 51, 0, -1);
                        OrkSpawnAdd(3015, 50, 367110, 507056, 3080, 2519, 51, 0, -1);
                        //Spitze
                        //OrkSpawnAdd(3015, 50, 367910, 507443, 3080, 2083, 51, 0, -1);
                        //OrkSpawnAdd(3015, 50, 367820, 507548, 3080, 2048, 51, 0, -1);
                        //OrkSpawnAdd(3015, 50, 367987, 507560, 3080, 2064, 51, 0, -1);
                        //OrkSpawnAdd(3015, 50, 367779, 507723, 3095, 2056, 51, 0, -1);
                        //OrkSpawnAdd(3015, 50, 368057, 507710, 3091, 1964, 51, 0, -1);
                    }
                    TheOrkSpawn = true;
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
        protected GameNPC OrkSpawnAdd(int templateID, int level, int x, int y, int Z, ushort heading, ushort region, int roamingrange, int respawnInterval)
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
                log.Warn(String.Format("Unable to get template for {0}", Name));
            }
            return add;
        }
        /// </summary>
        protected class OrkSpawnTimers : GameTimer
        {
            private GameNPC m_npc;


            /// <summary>
            /// Constructs a new DespawnTimer.
            /// </summary>
            /// <param name="timerOwner">The owner of this timer.</param>
            /// <param name="npc">The GameNPC to despawn when the time is up.</param>
            /// <param name="delay">The time after which the add is supposed to despawn.</param>
            public OrkSpawnTimers(GameObject timerOwner, GameNPC npc, int delay)
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

                // Remove the NPC from the world.
                if (m_npc != null && m_npc.InCombat == false)
                {
                    m_npc.Delete();
                    m_npc = null;

                }
            }
        }




        #endregion

        #region Demon Queen Spawn




        private bool SpawnDemonQueen = false;

        private void DemonSpawn(GameObject PlayerMobKiller, string KilledMobName)
        {
            if (PlayerMobKiller is GamePlayer == false || PlayerMobKiller == null || KilledMobName == null)
                return;

            KillCount = 0;
            string queen = "Queen";
            string actualregion;


            foreach (GameClient client in WorldMgr.GetAllPlayingClients())
            {
                if (client == null) continue;
                {
                    if (PlayerMobKiller.Level == 50 && PlayerMobKiller != null && PlayerMobKiller is GamePlayer && PlayerMobKiller == client.Player)
                    {
                        if (PlayerMobKiller != null && SpawnDemonQueen == false && (KilledMobName == "deamhaness" || KilledMobName == "cambion" || KilledMobName == "rocot" || KilledMobName == "Lilith the Demon Queen" || MobName == "succubus"))
                        {
                            if (KilledMobName == "Lilith the Demon Queen")
                            {
                                ReportQueenNews(PlayerMobKiller);
                            }

                            //IList<DBQueenSpawn> queenname = GameServer.Database.SelectObjects<DBQueenSpawn>("Name = '" + queen + "'");
                            IList<DBQueenSpawn> queenname = GameServer.Database.SelectObjects<DBQueenSpawn>("`Name` = @Name", new QueryParameter("@Name", queen));
                            foreach (DBQueenSpawn dba in queenname)
                            {
                                if (dba != null && dba.Name == queen && PlayerMobKiller != null)
                                {
                                    if (dba.KillCount <= dba.SpawnCount && SpawnDemonQueen == false)
                                    {

                                        dba.KillCount = dba.KillCount += Util.Random(1, 3);
                                    }
                                    else if (dba.KillCount >= dba.SpawnCount + 1)
                                    {
                                        SpawnDemonQueen = true;
                                        dba.KillCount = 0;
                                    }

                                    dba.Name = "Queen";
                                    dba.Heading = PlayerMobKiller.Heading;
                                    dba.X = PlayerMobKiller.X + 300;
                                    dba.Y = PlayerMobKiller.Y + 300;
                                    dba.Z = PlayerMobKiller.Z;
                                    dba.Region = PlayerMobKiller.CurrentRegion.ID;
                                    GameServer.Database.SaveObject(dba);
                                    KillCount = dba.KillCount;

                                    if (SpawnDemonQueen == true && dba.KillCount == 0)
                                    {
                                        {
                                            SpawnDemonQueen = false;
                                        }
                                        //the Queen
                                        QueenSpawnAdd(dba.NPCTemplateID, Util.Random(65, 68), dba.X, dba.Y, dba.Z, dba.Heading, dba.Region, 0, -1, 360 * 1000);

                                        //succubus add
                                        QueenSpawnAdd(3004, 60, dba.X, dba.Y + 200, dba.Z, dba.Heading, dba.Region, 0, -1, 360 * 1000);
                                        QueenSpawnAdd(3004, 60, dba.X, dba.Y - 200, dba.Z, dba.Heading, dba.Region, 0, -1, 360 * 1000);

                                        QueenSpawnAdd(3004, 60, dba.X, dba.Y + 100, dba.Z, dba.Heading, dba.Region, 0, -1, 360 * 1000);
                                        QueenSpawnAdd(3004, 60, dba.X - 100, dba.Y, dba.Z, dba.Heading, dba.Region, 0, -1, 360 * 1000);


                                        QueenSpawnAdd(3004, 60, dba.X + 200, dba.Y, dba.Z, dba.Heading, dba.Region, 0, -1, 360 * 1000);
                                        QueenSpawnAdd(3004, 60, dba.X - 200, dba.Y, dba.Z, dba.Heading, dba.Region, 0, -1, 360 * 1000);
                                        //Handmaiden of Lilith add
                                        QueenSpawnAdd(3005, 60, dba.X + 300, dba.Y + 300, dba.Z, dba.Heading, dba.Region, 0, -1, 360 * 1000);
                                        QueenSpawnAdd(3005, 60, dba.X - 300, dba.Y - 300, dba.Z, dba.Heading, dba.Region, 0, -1, 360 * 1000);


                                        BroadcastUpdate();

                                        if (dba.Region == (ushort)163)
                                            actualregion = "Agramon";
                                        else if (dba.Region == (ushort)249)
                                            actualregion = "Darkness Falls";
                                        else
                                            actualregion = "unknown";


                                        client.Player.Out.SendMessage("After long sleep, the Demon is awake! The Queen of Darness Falls is Spawned in " + actualregion + " to defend her darkness!", eChatType.CT_Send, eChatLoc.CL_ChatWindow);
                                        client.Player.Out.SendMessage("After long sleep, the Demon is awake! The Queen of Darness Falls is Spawned in " + actualregion + " to defend her darkness!", eChatType.CT_SocialInterface, eChatLoc.CL_ChatWindow);
                                        client.Player.Out.SendMessage("After long sleep, the Demon is awake! The Queen of Darness Falls is Spawned in " + actualregion + " to defend her darkness!", eChatType.CT_ScreenCenterSmaller, eChatLoc.CL_ChatWindow);
                                        return;
                                    }
                                    //Console.WriteLine("cambion nr {0} spawn = {1}", KillCount, SpawnDemonQueen);
                                }
                            }
                        }
                    }
                }
            }
        }


        private INpcTemplate m_addMobTemplate;

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
        protected GameNPC QueenSpawnAdd(int templateID, int level, int x, int y, int Z, ushort heading, ushort region, int roamingrange, int respawnInterval, int despwantimer)
        {
            GameNPC add = null;

            try
            {
                if (m_addMobTemplate == null || m_addMobTemplate.TemplateId != templateID)
                {
                    m_addMobTemplate = NpcTemplateMgr.GetTemplate(templateID);
                }

                // Create add from template.


                if (m_addMobTemplate != null)
                {
                    add = new GameNPC(m_addMobTemplate)
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
                    new DespawnTimers2(add, add, despwantimer);
                }
            }

            catch
            {
                log.Warn(String.Format("Unable to get template for {0}", Name));
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
                    if (m_npc.Name == "Unknown Portal")
                    {
                        foreach (GameClient client in WorldMgr.GetClientsOfRegion(498))
                        {
                            if (client != null)
                            {
                                if (client.Player.ObjectState == GameObject.eObjectState.Active)
                                {
                                    GamePlayer player = client.Player as GamePlayer;

                                    switch (player.Realm)
                                    {
                                        case eRealm.Albion:
                                            player.MoveTo(166, 38027, 53524, 4154, 2527);
                                            break;

                                        case eRealm.Midgard:
                                            player.MoveTo(166, 54809, 24284, 4320, 958);
                                            break;

                                        case eRealm.Hibernia:
                                            player.MoveTo(166, 18211, 17653, 4320, 3924);
                                            break;

                                        default: break;
                                    }

                                }

                            }
                        }
                    }

                    m_npc.Delete();
                    m_npc = null;
                }
            }
        }
        /// <summary>
        /// Post a message in the server news and award a dragon kill point for
        /// every XP gainer in the raid.
        /// </summary>
        /// <param name="killer">The living that got the killing blow.</param>
        protected void ReportQueenNews(GameObject killer)
        {

            String message = String.Format("The Queen has been slain, player {0} has defeated the darkness!", killer.Name);
            NewsMgr.CreateNews(message, 0, eNewsType.PvE, true);

            if (Properties.GUILD_MERIT_ON_DRAGON_KILL > 0)
            {
                foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    if (player.IsEligibleToGiveMeritPoints)
                    {
                        GuildEventHandler.MeritForNPCKilled(player, this, Properties.GUILD_MERIT_ON_DRAGON_KILL);
                    }
                }
            }

        }
        #endregion

        #region ePrpertyHashtable
        protected static Dictionary<eProperty, string> hPropertyToMagicPrefix = new Dictionary<eProperty, string>();

        public static bool WriteMagicalName(eProperty property, ItemUnique item)
        {

            if (hPropertyToMagicPrefix.ContainsKey(property))
            {
                string str = hPropertyToMagicPrefix[property];
                if (str != string.Empty)
                    item.Name = str + " " + item.Name;
                return true;
            }

            return false;
        }

        public static void InitializeHashtables()
        {
            // Magic Prefix

            hPropertyToMagicPrefix.Add(eProperty.Strength, "Mighty");
            hPropertyToMagicPrefix.Add(eProperty.Dexterity, "Adroit");
            hPropertyToMagicPrefix.Add(eProperty.Constitution, "Fortifying");
            hPropertyToMagicPrefix.Add(eProperty.Quickness, "Speedy");
            hPropertyToMagicPrefix.Add(eProperty.Intelligence, "Insightful");
            hPropertyToMagicPrefix.Add(eProperty.Piety, "Willful");
            hPropertyToMagicPrefix.Add(eProperty.Empathy, "Attuned");
            hPropertyToMagicPrefix.Add(eProperty.Charisma, "Glib");
            hPropertyToMagicPrefix.Add(eProperty.MaxMana, "Arcane");
            hPropertyToMagicPrefix.Add(eProperty.MaxHealth, "Sturdy");
            hPropertyToMagicPrefix.Add(eProperty.PowerPool, "Arcane");

            hPropertyToMagicPrefix.Add(eProperty.Resist_Body, "Bodybender");
            hPropertyToMagicPrefix.Add(eProperty.Resist_Cold, "Icebender");
            hPropertyToMagicPrefix.Add(eProperty.Resist_Crush, "Bluntbender");
            hPropertyToMagicPrefix.Add(eProperty.Resist_Energy, "Energybender");
            hPropertyToMagicPrefix.Add(eProperty.Resist_Heat, "Heatbender");
            hPropertyToMagicPrefix.Add(eProperty.Resist_Matter, "Matterbender");
            hPropertyToMagicPrefix.Add(eProperty.Resist_Slash, "Edgebender");
            hPropertyToMagicPrefix.Add(eProperty.Resist_Spirit, "Spiritbender");
            hPropertyToMagicPrefix.Add(eProperty.Resist_Thrust, "Thrustbender");

            hPropertyToMagicPrefix.Add(eProperty.Skill_Two_Handed, "Sundering");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Body, "Soul Crusher");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Critical_Strike, "Lifetaker");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Cross_Bows, "Truefire");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Crushing, "Battering");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Death_Servant, "Death Binder");
            hPropertyToMagicPrefix.Add(eProperty.Skill_DeathSight, "Minionbound");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Dual_Wield, "Whirling");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Earth, "Earthborn");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Enhancement, "Fervent");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Envenom, "Venomous");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Fire, "Flameborn");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Flexible_Weapon, "Tensile");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Cold, "Iceborn");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Instruments, "Melodic");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Long_bows, "Winged");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Matter, "Earthsplitter");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Mind, "Dominating");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Pain_working, "Painbound");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Parry, "Bladeblocker");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Polearms, "Decimator");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Rejuvenation, "Rejuvenating");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Shields, "Protector's");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Slashing, "Honed");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Smiting, "Earthshaker");
            hPropertyToMagicPrefix.Add(eProperty.Skill_SoulRending, "Soul Taker");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Spirit, "Spiritbound");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Staff, "Thunderer");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Stealth, "Shadowwalker");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Thrusting, "Perforator");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Wind, "Airy");


            hPropertyToMagicPrefix.Add(eProperty.AllMagicSkills, "Mystical");
            hPropertyToMagicPrefix.Add(eProperty.AllMeleeWeaponSkills, "Gladiator");
            hPropertyToMagicPrefix.Add(eProperty.AllSkills, "Skillful");
            hPropertyToMagicPrefix.Add(eProperty.AllDualWieldingSkills, "Duelist");
            hPropertyToMagicPrefix.Add(eProperty.AllArcherySkills, "Bowmaster");


            hPropertyToMagicPrefix.Add(eProperty.Skill_Sword, "Serrated");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Hammer, "Demolishing");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Axe, "Swathe Cutter's");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Left_Axe, "Cleaving");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Spear, "Impaling");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Mending, "Bodymender");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Augmentation, "Empowering");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Darkness, "Shadowbender");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Suppression, "Spiritbinder");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Runecarving, "Runebender");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Stormcalling, "Stormcaller");
            hPropertyToMagicPrefix.Add(eProperty.Skill_BeastCraft, "Lifebender");

            hPropertyToMagicPrefix.Add(eProperty.Skill_Light, "Lightbender");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Void, "Voidbender");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Mana, "Starbinder");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Enchantments, "Chanter");

            hPropertyToMagicPrefix.Add(eProperty.Skill_Blades, "Razored");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Blunt, "Crushing");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Piercing, "Lancenator");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Large_Weapon, "Sundering");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Mentalism, "Mindbinder");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Regrowth, "Forestbound");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Nurture, "Plantbound");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Nature, "Animalbound");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Music, "Resonant");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Celtic_Dual, "Whirling");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Celtic_Spear, "Impaling");
            hPropertyToMagicPrefix.Add(eProperty.Skill_RecurvedBow, "Hawk");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Valor, "Courageous");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Subterranean, "Ancestral");
            hPropertyToMagicPrefix.Add(eProperty.Skill_BoneArmy, "Blighted");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Verdant, "Vale Defender");

            hPropertyToMagicPrefix.Add(eProperty.Skill_Battlesongs, "Motivating");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Composite, "Dragon");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Creeping, "Withering");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Arboreal, "Arbor Defender");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Scythe, "Reaper's");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Thrown_Weapons, "Catapult");
            hPropertyToMagicPrefix.Add(eProperty.Skill_HandToHand, "Martial");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Pacification, "Pacifying");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Savagery, "Savage");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Nightshade, "Nightshade");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Pathfinding, "Trail");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Summoning, "Soulbinder");

            hPropertyToMagicPrefix.Add(eProperty.Skill_Dementia, "Feverish");
            hPropertyToMagicPrefix.Add(eProperty.Skill_ShadowMastery, "Ominous");
            hPropertyToMagicPrefix.Add(eProperty.Skill_VampiiricEmbrace, "Deathly");
            hPropertyToMagicPrefix.Add(eProperty.Skill_EtherealShriek, "Shrill");
            hPropertyToMagicPrefix.Add(eProperty.Skill_PhantasmalWail, "Keening");
            hPropertyToMagicPrefix.Add(eProperty.Skill_SpectralForce, "Uncanny");
            hPropertyToMagicPrefix.Add(eProperty.Skill_OdinsWill, "Ardent");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Cursing, "Infernal");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Hexing, "Bedeviled");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Witchcraft, "Diabolic");

            // Mauler - live mauler prefixes do not exist, as lame as that sounds.
            hPropertyToMagicPrefix.Add(eProperty.Skill_Aura_Manipulation, string.Empty);
            hPropertyToMagicPrefix.Add(eProperty.Skill_FistWraps, string.Empty);
            hPropertyToMagicPrefix.Add(eProperty.Skill_MaulerStaff, string.Empty);
            hPropertyToMagicPrefix.Add(eProperty.Skill_Magnetism, string.Empty);
            hPropertyToMagicPrefix.Add(eProperty.Skill_Power_Strikes, string.Empty);
        }

        #endregion 
    }
}

