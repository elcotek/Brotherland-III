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
using DOL.Language;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DOL.GS.Keeps
{
    //TODO : find all skin of keep door to load it from here
    /// <summary>
    /// A keepComponent
    /// </summary>
    public class GameKeepComponent : GameLiving, IComparable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected readonly ushort INVISIBLE_MODEL = 150;

        public enum eComponentSkin : byte
        {
            Gate = 0,
            WallInclined = 1,
            WallInclined2 = 2,
            WallAngle2 = 3,
            TowerAngle = 4,
            WallAngle = 5,
            WallAngleInternal = 6,
            TowerHalf = 7,
            WallHalfAngle = 8,
            Wall = 9,
            Keep = 10,
            Tower = 11,
            WallWithDoorLow = 12,
            WallWithDoorHigh = 13,
            BridgeHigh = 14,
            WallInclinedLow = 15,
            BridgeLow = 16,
            BridgeHightSolid = 17,
            BridgeHighWithHook = 18,
            GateFree = 19,
            BridgeHightWithHook2 = 20,

            NewSkinClimbingWall = 27,
            NewSkinTower = 31,
        }

        #region properties

        /// <summary>
        /// keep owner of component
        /// </summary>
        private AbstractGameKeep m_keep;
        /// <summary>
        /// keep owner of component
        /// </summary>
        public AbstractGameKeep Keep
        {
            get { return m_keep; }
            set { m_keep = value; }
        }

        /// <summary>
        /// id of keep component id keep
        /// </summary>
        private int m_id;
        /// <summary>
        /// id of keep component id keep
        /// </summary>
        public int ID
        {
            get { return m_id; }
            set { m_id = value; }
        }

        /// <summary>
        /// height of keep grow with level
        /// </summary>
        public int Height
        {
            get { return KeepMgr.GetHeightFromLevel(this.Keep, this.Keep.Level); }
        }

        /// <summary>
        /// skin of keep component (wall, tower, ...)
        /// </summary>
        private int m_skin;
        public int Skin
        {
            get { return m_skin; }
            set { m_skin = value; }
        }

        public bool Climbing
        {
            get
            {
                if (ServerProperties.Properties.ALLOW_TOWER_CLIMB)
                {
                    if (m_skin == (int)eComponentSkin.Wall || m_skin == (int)eComponentSkin.NewSkinClimbingWall || m_skin == (int)eComponentSkin.Tower || m_skin == (int)eComponentSkin.NewSkinTower && !Keep.IsPortalKeep) return true;
                }
                else
                {
                    if (m_skin == (int)eComponentSkin.Wall || m_skin == (int)eComponentSkin.NewSkinClimbingWall && !Keep.IsPortalKeep) return true;
                }
                return false;
            }
        }

        /// <summary>
        /// relative X to keep
        /// </summary>
        private int m_componentx;
        /// <summary>
        /// relative X to keep
        /// </summary>
        public int ComponentX
        {
            get { return m_componentx; }
            set { m_componentx = value; }
        }

        /// <summary>
        /// relative Y to keep
        /// </summary>
        private int m_componenty;
        /// <summary>
        /// relative Y to keep
        /// </summary>
        public int ComponentY
        {
            get { return m_componenty; }
            set { m_componenty = value; }
        }

        /// <summary>
        /// relative heading to keep ( 0, 1, 2, 3)
        /// </summary>
        private int m_componentHeading;
        /// <summary>
        /// relative heading to keep ( 0, 1, 2, 3)
        /// </summary>
        public int ComponentHeading
        {
            get { return m_componentHeading; }
            set { m_componentHeading = value; }
        }

        protected int m_oldMaxHealth;

        /// <summary>
        /// Level of component
        /// </summary>
        public override byte Level
        {
            get
            {
                //return (byte)(40 + Keep.Level);
                return (byte)(Keep.BaseLevel - 10 + (Keep.Level * 3));
            }
        }

        public override eRealm Realm
        {
            get
            {
                if (Keep != null)
                {
                    return Keep.Realm;
                }

                return eRealm.None;
            }
        }

        private Hashtable m_hookPoints;
        protected byte m_oldHealthPercent;
        protected bool m_isRaized;

        public Hashtable HookPoints
        {
            get { return m_hookPoints; }
            set { m_hookPoints = value; }
        }


        private readonly Hashtable m_positions;
        public Hashtable Positions
        {
            get { return m_positions; }
        }

        protected string m_CreateInfo = String.Empty;

        #endregion

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

        public override int AttackRange
        {
            get { return 1000; }
        }


        public override IList GetExamineMessages(GamePlayer player)
        {
            IList list = base.GetExamineMessages(player);

            if (player.Client.Account.PrivLevel > 1)
            {
                list.Add(Name + " with a Z of " + Z.ToString());
            }

            return list;
        }
        /// <summary>
        /// Procs don't normally fire on game keep components
        /// </summary>
        /// <param name="ad"></param>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public override bool AllowWeaponMagicalEffect(AttackData ad, InventoryItem weapon, Spell weaponSpell)
        {
            Spell procSpell = null;
            Spell procSpell1 = null;

            SpellLine line = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
            if (line != null)
            {
                procSpell = SkillBase.FindSpell(weapon.ProcSpellID, line);
                procSpell1 = SkillBase.FindSpell(weapon.ProcSpellID1, line);
            }
          
            // elcotek KeepDamage proc nur an keep komponenten.
            if (procSpell != null && procSpell.SpellType == "KeepDamageBuff" || procSpell1 != null && procSpell1.SpellType == "KeepDamageBuff")
                return true;


            return false;
        }

        /// <summary>
        /// do not regen
        /// </summary>
        public override void StartHealthRegeneration()
        {
            m_repairTimer = new RegionTimer(CurrentRegion.TimeManager)
            {
                Callback = new RegionTimerCallback(RepairTimerCallback),
                Interval = repairInterval
            };
            m_repairTimer.Start(1);
        }

        public virtual void RemoveTimers()
        {
            if (m_repairTimer != null)
            {
                m_repairTimer.Stop();
                m_repairTimer = null;
            }
        }

        /// <summary>
        /// constructor of component
        /// </summary>
        public GameKeepComponent()
        {
            m_hookPoints = new Hashtable(41);
            m_positions = new Hashtable();
        }

        /// <summary>
        /// load component from db object
        /// </summary>
        public virtual void LoadFromDatabase(DBKeepComponent component, AbstractGameKeep keep)
        {
            Region myregion = WorldMgr.GetRegion((ushort)keep.Region);
            if (myregion == null)
                return;
            this.Keep = keep;
            //this.DBKeepComponent = component;
            base.LoadFromDatabase(component);
            //this x and y is for get object in radius
            double angle = keep.Heading * ((Math.PI * 2) / 360); // angle*2pi/360;
            X = (int)(keep.X + ((sbyte)component.X * 148 * Math.Cos(angle) + (sbyte)component.Y * 148 * Math.Sin(angle)));
            Y = (int)(keep.Y - ((sbyte)component.Y * 148 * Math.Cos(angle) - (sbyte)component.X * 148 * Math.Sin(angle)));
            this.Z = keep.Z;



            // and this one for packet sent
            this.ComponentX = component.X;
            this.ComponentY = component.Y;
            this.ComponentHeading = (ushort)component.Heading;
            //need check to be sure for heading
            angle = (component.Heading * 90 + keep.Heading);
            if (angle > 360) angle -= 360;
            this.Heading = (ushort)(angle / 0.08789);
            this.Name = keep.Name;
            this.Model = INVISIBLE_MODEL;
            this.Skin = component.Skin;
            m_oldMaxHealth = MaxHealth;
            this.Health = MaxHealth;
            //			this.Health = component.Health;
            this.m_oldHealthPercent = this.HealthPercent;
            this.CurrentRegion = myregion;
            this.ID = component.ID;
            this.SaveInDB = false;
            this.IsRaized = false;
            LoadPositions();
            this.AddToWorld();
            FillPositions();
            this.RepairedHealth = this.MaxHealth;
            m_CreateInfo = component.CreateInfo;
            StartHealthRegeneration();
        }

        public virtual void LoadPositions()
        {
            ushort region = CurrentRegionID;
            if (CurrentRegion is BaseInstance)
            {
                region = (CurrentRegion as BaseInstance).Skin;
            }

            Battleground bg = KeepMgr.GetBattleground(region);

            this.Positions.Clear();
            /*
            string query = "`ComponentSkin` = '" + this.Skin + "'";
            if (Skin != (int)eComponentSkin.Keep && Skin != (int)eComponentSkin.Tower && Skin != (int)eComponentSkin.Gate)
            {
                query = query + " AND `ComponentRotation` = '" + this.ComponentHeading + "'";
            }
            if (bg != null)
            {
                // Battlegrounds, ignore all but GameKeepDoor
                query += " AND `ClassType` = 'DOL.GS.Keeps.GameKeepDoor'";
            }

            System.Collections.Generic.IList<DBKeepPosition> DBPositions = GameServer.Database.SelectObjects<DBKeepPosition>(query);
            */

            List<QueryParameter> parameters = new List<QueryParameter>(3);
            parameters.Add(new QueryParameter("@Skin", Skin));
            string query = "`ComponentSkin` = @Skin";
            if (Skin != (int)eComponentSkin.Keep && Skin != (int)eComponentSkin.Tower && Skin != (int)eComponentSkin.Gate)
            {
                parameters.Add(new QueryParameter("@Rotation", ComponentHeading));
                query += " AND `ComponentRotation` = @Rotation";
            }
            if (bg != null && GameServer.Instance.Configuration.ServerType != eGameServerType.GST_PvE)
            {
                // Battlegrounds, ignore all but GameKeepDoor
                query += " AND `ClassType` = 'DOL.GS.Keeps.GameKeepDoor'";
                // Battlegrounds, ignore all but GameKeepDoor
                parameters.Add(new QueryParameter("@ClassType", "'DOL.GS.Keeps.GameKeepDoor'"));
                //query = query + " AND `ClassType` = @ClassType";

            }

            var DBPositions = GameServer.Database.SelectObjects<DBKeepPosition>(query, parameters.ToArray());

            foreach (DBKeepPosition position in DBPositions)
            {
                DBKeepPosition[] list = this.Positions[position.TemplateID] as DBKeepPosition[];
                if (list == null)
                {
                    list = new DBKeepPosition[4];
                    this.Positions[position.TemplateID] = list;
                }

                list[position.Height] = position;
            }
           
        }

        /// <summary>
        /// Populate GameKeepItems for this component into the keep
        /// </summary>
        public virtual void FillPositions()
        {
            foreach (DBKeepPosition[] positionGroup in Positions.Values)
            {
                for (int i = this.Height; i >= 0; i--)
                {
                    if (positionGroup[i] is DBKeepPosition position)
                    {
                        bool create = false;
                        string sKey = position.TemplateID;

                        switch (position.ClassType)
                        {
                            case "DOL.GS.Keeps.GameKeepBanner":
                                if (Keep.Banners.ContainsKey(sKey) == false)
                                    create = true;
                                break;
                            case "DOL.GS.Keeps.GameKeepDoor":
                                if (Keep.Doors.ContainsKey(sKey) == false)
                                    create = true;
                                break;
                            case "DOL.GS.Keeps.FrontierTeleportStone":
                                if (Keep.TeleportStone == null)
                                    create = true;
                                break;
                            case "DOL.GS.Keeps.Patrol":
                                if ((position.KeepType == (int)AbstractGameKeep.eKeepType.Any || position.KeepType == (int)Keep.KeepType)
                                    && Keep.Patrols.ContainsKey(sKey) == false)
                                {
                                    Patrol p = new Patrol(this);
                                    p.SpawnPosition = position;
                                    p.PatrolID = position.TemplateID;
                                    p.InitialiseGuards();
                                }
                                continue;
                            case "DOL.GS.Keeps.FrontierHastener":
                                if (Keep.HasHastener && log.IsWarnEnabled)
                                    log.Warn($"FillPositions(): KeepComponent_ID {InternalID}, KeepPosition_ID {position.ObjectId}: There is already a {position.ClassType} on Keep {Keep.KeepID}");

                                if (Keep.Guards.ContainsKey(sKey) == false)
                                {
                                    Keep.HasHastener = true;
                                    create = true;
                                }
                                break;
                            case "DOL.GS.Keeps.MissionMaster":
                                if (Keep.HasCommander && log.IsWarnEnabled)
                                    log.Warn($"FillPositions(): KeepComponent_ID {InternalID}, KeepPosition_ID {position.ObjectId}: There is already a {position.ClassType} on Keep {Keep.KeepID}");

                                if (Keep.Guards.ContainsKey(sKey) == false)
                                {
                                    Keep.HasCommander = true;
                                    create = true;
                                }
                                break;
                            case "DOL.GS.Keeps.GuardLord":
                                if (Keep.HasLord && log.IsWarnEnabled)
                                    log.Warn($"FillPositions(): KeepComponent_ID {InternalID}, KeepPosition_ID {position.ObjectId}: There is already a {position.ClassType} on Keep {Keep.KeepID}");

                                if (Keep.Guards.ContainsKey(sKey) == false)
                                {
                                    Keep.HasLord = true;
                                    create = true;
                                }
                                break;
                            default:
                                if (Keep.Guards.ContainsKey(sKey) == false)
                                    create = true;
                                break;
                        }// switch (position.ClassType)

                        if (create)
                        {
                            //create the object
                            try
                            {
                                Assembly asm = Assembly.GetExecutingAssembly();
                                IKeepItem obj = (IKeepItem)asm.CreateInstance(position.ClassType, true);
                                if (obj != null)
                                    obj.LoadFromPosition(position, this);

                                if (Properties.ENABLE_DEBUG)
                                {
                                    if (obj is GameLiving living)
                                        living.Name += " is living, component " + obj.Component.ID;
                                    else if (obj is GameObject game)
                                        game.Name += " is object, component " + obj.Component.ID;
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("FillPositions(): " + position.ClassType, ex);
                            }
                        }
                        else
                        {
                            // move the object
                            if (position.ClassType == "DOL.GS.Keeps.GameKeepBanner")
                            {
                                if (Keep.Banners.ContainsKey(sKey))
                                {
                                    foreach (var banners in Keep.Banners.Values)
                                    {
                                        if (banners.DBPosition == position)
                                            banners.MoveToPosition(position);
                                    }
                                }

                            }
                            else if (position.ClassType == "DOL.GS.Keeps.GameKeepDoor")
                            {
                                //doors dont move
                            }
                            else if (position.ClassType == "DOL.GS.Keeps.FrontierPortalStone")
                            {
                                //these dont move
                            }
                            else
                            {
                                // move the object
                                if (Keep.Guards.ContainsKey(sKey))
                                {
                                    foreach (var guard in Keep.Guards.Values)
                                    {
                                        if (guard.DBPosition == position)
                                            guard.MoveToPosition(position);
                                    }
                                }
                            }
                        }
                        break; // We found the highest item for that position, move onto the next one
                    }// if (positionGroup[i] is DBKeepPosition position)
                }// for (int i = this.Height; i >= 0; i--)
            }// foreach (DBKeepPositioion[] positionGroup in this.Positions.Values)

            foreach (var guard in Keep.Guards.Values)
            {
                if (guard.PatrolGroup != null)
                    continue;
                if (guard.HookPoint != null) continue;
                if (guard.DBPosition == null) continue;
                if (guard.DBPosition.Height > guard.Component.Height)
                    guard.RemoveFromWorld();
                else
                {
                    if (guard.DBPosition.Height <= guard.Component.Height &&
                        guard.ObjectState != GameObject.eObjectState.Active && !guard.IsRespawning)
                        guard.AddToWorld();
                }
            }

            foreach (var banner in Keep.Banners.Values)
            {
                if (banner.DBPosition == null) continue;
                if (banner.DBPosition.Height > banner.Component.Height)
                    banner.RemoveFromWorld();
                else
                {
                    if (banner.DBPosition.Height <= banner.Component.Height &&
                        banner.ObjectState != GameObject.eObjectState.Active)
                        banner.AddToWorld();
                }
            }
        }

        /// <summary>
        /// save component in DB
        /// </summary>
        public override void SaveIntoDatabase()
        {


            DBKeepComponent obj = null;
            bool New = false;
            if (InternalID != null)
                obj = GameServer.Database.FindObjectByKey<DBKeepComponent>(InternalID);
            if (obj == null)
            {
                obj = new DBKeepComponent();
                New = true;
            }
            obj.KeepID = Keep.KeepID;
            obj.Heading = ComponentHeading;
            obj.Health = Health;
            obj.X = this.ComponentX;
            obj.Y = this.ComponentY;
            obj.ID = this.ID;
            obj.Skin = this.Skin;
            obj.CreateInfo = m_CreateInfo;


            if (New)
            {
                GameServer.Database.AddObject(obj);
                InternalID = obj.ObjectId;
                log.DebugFormat("Added new component for keep ID {0} health {1}", Keep.KeepID, Health);
            }
            else
            {
                try
                {

                    GameServer.Database.SaveObject(obj);
                }
                catch
                {
                    log.ErrorFormat("Added new component Error for1;  {0}", obj.TableName);
                }
            }

            try
            {

                base.SaveIntoDatabase();
            }
            catch
            {
                log.ErrorFormat("Added new component Error for:  {0}", Name);
            }
        }
        public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
        {
            if (damageAmount > 0)
            {
                this.Keep.LastAttackedByEnemyTick = this.CurrentRegion.Time;
                base.TakeDamage(source, damageType, damageAmount, criticalAmount);

                //only on hp change
                if (m_oldHealthPercent != HealthPercent)
                {
                    m_oldHealthPercent = HealthPercent;
                    foreach (GameClient client in WorldMgr.GetClientsOfRegion(CurrentRegionID))
                    {
                        client.Out.SendObjectUpdate(this);
                        client.Out.SendKeepComponentDetailUpdate(this); // I knwo this works, not sure if ObjectUpdate is needed - Tolakram
                    }
                }
            }
        }




        public override void ModifyAttack(AttackData attackData)
        {
            int toughness = Properties.SET_STRUCTURES_TOUGHNESS;
            int baseDamage = attackData.Damage;
            int styleDamage = attackData.StyleDamage;
            int criticalDamage = 0;

            GameLiving source = attackData.Attacker;


            if (source is GamePlayer)
            {
                if (source.CurrentRegionID == 163 && (source as GameLiving).AllowedToRaid() == false && this.Keep.InCombat == false)
                {
                    baseDamage = 0;
                    styleDamage = 0;
                    attackData.AttackResult = eAttackResult.NotAllowed_ServerRules;
                    ((GamePlayer)source).Out.SendMessage(LanguageMgr.GetTranslation((source as GamePlayer).Client.Account.Language, "AntiRaidMsg", Properties.RAID_MEMBER_COUNT + 1), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    return;
                }
                else

                    baseDamage = (baseDamage - (baseDamage * 5 * this.Keep.Level / 100)) * toughness / 100;
                styleDamage = (styleDamage - (styleDamage * 5 * this.Keep.Level / 100)) * toughness / 100;
            }
            else if (source is GameNPC)
            {
                if (source.CurrentRegionID == 163 && (source as GameLiving).AllowedToRaid() == false && this.Keep.InCombat == false)
                {
                    baseDamage = 0;
                    styleDamage = 0;
                    attackData.AttackResult = eAttackResult.NotAllowed_ServerRules;
                }
                else if (!ServerProperties.Properties.STRUCTURES_ALLOWPETATTACK)
                {
                    baseDamage = 0;
                    styleDamage = 0;
                    attackData.AttackResult = eAttackResult.NotAllowed_ServerRules;
                }
                else
                {
                    baseDamage = (baseDamage - (baseDamage * 5 * this.Keep.Level / 100)) * toughness / 100;
                    styleDamage = (styleDamage - (styleDamage * 5 * this.Keep.Level / 100)) * toughness / 100;

                    if (((GameNPC)source).Brain is DOL.AI.Brain.IControlledBrain)
                    {
                        GamePlayer player = (((DOL.AI.Brain.IControlledBrain)((GameNPC)source).Brain).Owner as GamePlayer);
                        if (player != null)
                        {
                            // special considerations for pet spam classes
                            if (player.IsCharcterClass(eCharacterClass.Theurgist) || player.IsCharcterClass(eCharacterClass.Animist))
                            {
                                baseDamage = (int)(baseDamage * ServerProperties.Properties.PET_SPAM_DAMAGE_MULTIPLIER);
                                styleDamage = (int)(styleDamage * ServerProperties.Properties.PET_SPAM_DAMAGE_MULTIPLIER);
                            }
                            else
                            {
                                baseDamage = (int)(baseDamage * ServerProperties.Properties.PET_DAMAGE_MULTIPLIER);
                                styleDamage = (int)(styleDamage * ServerProperties.Properties.PET_DAMAGE_MULTIPLIER);
                            }
                        }
                    }
                }
            }

            attackData.Damage = baseDamage;
            attackData.StyleDamage = styleDamage;
            attackData.CriticalDamage = criticalDamage;
        }

        private bool wasRised = false;
        /// <summary>
        ///Set Guards new Position after Repair
        /// </summary>
        public void SetLordPosition()
        {
           
            if (this.Keep is GameKeepTower && wasRised)
            {
                GameKeepComponent component = null;

                foreach (GameKeepComponent c in this.Keep.KeepComponents)
                {

                    int id2 = 0;


                    if (this.Keep is GameKeepTower)
                    {

                        id2 = 11;
                    }
                    else
                    {

                        id2 = 10;
                    }

                    if (c.Skin == id2)
                    {
                        component = c;
                        break;
                    }
                }
                if (component == null)
                    return;
                int lordZ = component.Keep.Z + 81 + 384;
                //81 = compoponent z
               
                foreach (GameKeepGuard guard in this.Keep.Guards.Values)
                {
                    if (wasRised == true && guard is GuardLord)
                    {
                        guard.DieAfterRize(guard, wasRised);//component was raised so dont reset the guild of the lord
                        guard.MoveTo(guard.CurrentRegionID, guard.X, guard.Y, lordZ, guard.Heading);
                        guard.SpawnPoint.Z = lordZ;

                        wasRised = false;
                    }
                    if (guard is GuardStaticArcher)
                    {
                        guard.Die(guard);
                        guard.MoveTo(guard.CurrentRegionID, guard.X, guard.Y, lordZ - 252, guard.Heading);
                        guard.SpawnPoint.Z = lordZ - 252;
                    }

                    foreach (GameClient client in WorldMgr.GetClientsOfRegion(this.CurrentRegionID))
                        client.Out.SendKeepComponentDetailUpdate(this);
                }
            }
        }

        public override void Die(GameObject killer)
        {
            base.Die(killer);
            if (this.Keep is GameKeepTower && ServerProperties.Properties.CLIENT_VERSION_MIN >= (int)GameClient.eClientVersion.Version175)
            {
                if (IsRaized == false)
                {
                    Notify(KeepEvent.TowerRaized, this.Keep, new KeepEventArgs(this.Keep, killer.Realm));
                    PlayerMgr.BroadcastRaize(this.Keep, killer.Realm);
                    IsRaized = true;


                    GameKeepComponent component = null;
                    ushort distance = 0;

                    foreach (GameKeepComponent c in this.Keep.KeepComponents)
                    {

                        int id = 0;


                        if (this.Keep is GameKeepTower)
                        {
                            distance = 750;
                            id = 11;
                        }
                        else
                        {
                            distance = 1500;
                            id = 10;
                        }

                        if (c.Skin == id)
                        {
                            component = c;
                            break;
                        }
                    }
                    if (component == null)
                        return;

                    GameKeepHookPoint hookpoint = component.HookPoints[97] as GameKeepHookPoint;

                    if (hookpoint == null)
                        return;

                    if (hookpoint == null)
                        return;

                    // int height = KeepMgr.GetHeightFromLevel(this.Keep, this.Level);
                    //DBKeepHookPoint hp = GameServer.Database.SelectObject<DBKeepHookPoint>("HookPointID = '97' and Height = '" + height + "'");

                    DBKeepHookPoint hp = GameServer.Database.SelectObjects<DBKeepHookPoint>("`HookPointID` = @HookPointID AND `Height` = @Height",
                                                                                    new[] { new QueryParameter("@HookPointID", 97), new QueryParameter("@Height", Height) }).FirstOrDefault();
                    if (hp == null)
                        return;



                    foreach (GameKeepGuard guard in this.Keep.Guards.Values)
                    {
                        int newZ = component.Keep.Z + 81;

                        //int z = component.Z + hp.Z;

                        int d = hookpoint.GetDistance(guard as IPoint2D);

                        if (d > distance)
                            continue;

                        if (guard is GuardFighter)
                        {
                            guard.MoveTo(guard.CurrentRegionID, guard.X, guard.Y, newZ, guard.Heading);
                            guard.SpawnPoint.Z = newZ; // keepGuard Ground is highter than Z

                        }
                        if (component.IsRaized && (guard is GuardStaticArcher || guard is GuardStaticCaster || guard is GuardLord))
                        {
                            guard.DieAfterRize(guard, true);//component was raised so so dont reset the guild of the lord
                            guard.MoveTo(guard.CurrentRegionID, guard.X, guard.Y, newZ, guard.Heading);
                            guard.SpawnPoint.Z = newZ;//vorher z
                            wasRised = true;
                        }
                    }
                }

                foreach (GameClient client in WorldMgr.GetClientsOfRegion(this.CurrentRegionID))
                    client.Out.SendKeepComponentDetailUpdate(this);
            }
        }


        public override void Delete()
        {
            StopHealthRegeneration();
            RemoveTimers();
            HookPoints.Clear();
            Positions.Clear();
            Keep = null;
            base.Delete();
            CurrentRegion = null;
        }

        /// <summary>
        /// Remove a component and delete it from the database
        /// </summary>
        public virtual void Remove()
        {
            Delete();
            DBKeepComponent obj = null;
            if (this.InternalID != null)
                obj = GameServer.Database.FindObjectByKey<DBKeepComponent>(this.InternalID);
            if (obj != null)
                GameServer.Database.DeleteObject(obj);

            log.Warn("Keep Component deleted from database: " + obj.ID);
            //todo find a packet to remove the keep
        }

        /// <summary>
        /// IComparable.CompareTo implementation.
        /// </summary>
        public int CompareTo(object obj)
        {
            if (obj is GameKeepComponent)
                return (this.ID - ((GameKeepComponent)obj).ID);
            else
                return 0;
        }

        public virtual byte Status
        {
            get
            {
                if (this.Keep is GameKeepTower)
                {
                    if (this.m_isRaized)
                    {
                        if (this.HealthPercent >= 25)
                        {
                            IsRaized = false;
                        }
                        else return 0x02;
                    }
                    if (this.HealthPercent < 35) return 0x01;//broken
                }
                if (this.Keep is GameKeep)


                    if (this.HealthPercent < 1) return 0x01;//broken
                //Elcotek extra Update für Keep Wall wenns kaputt ist!
                foreach (GameClient client in WorldMgr.GetClientsOfRegion(CurrentRegionID))
                {
                    if (this.Keep is GameKeep && !this.IsAlive && client != null)
                    {
                        client.Out.SendObjectUpdate(this);
                    }
                }
                return 0x00;

            }
        }

        public virtual void UpdateLevel()
        {
            if ((IsRaized == false) && (MaxHealth != m_oldMaxHealth))
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

        public virtual bool IsRaized
        {
            get { return m_isRaized; }
            set
            {
                RepairedHealth = 0;
                m_isRaized = value;
                if (value == true)
                {
                    if (this.Keep.Level > 1)
                        Keep.ChangeLevel(1);
                }
                else
                {
                    FillPositions();
                }
            }
        }
       
        public int RepairedHealth = 0;
        
       protected RegionTimer m_repairTimer;
        protected static int repairInterval = 60 * 1000 * 60;

        
        //deaktiviert!! --->>>        
        public virtual int RepairTimerCallback(RegionTimer timer)
        {
            if (HealthPercent == 100 || InCombat)
            {
                return 0;
            }
            
            
            //Repair(MaxHealth * 0.15);
            return repairInterval; 

        }

        public virtual void Repair(int amount)
        {

            if (amount > 0)
            {
                byte oldStatus = Status;
                Health += amount;

                if (m_oldHealthPercent != HealthPercent)
                {
                    m_oldHealthPercent = HealthPercent;
                    foreach (GameClient client in WorldMgr.GetClientsOfRegion(CurrentRegionID))
                    {
                        client.Out.SendObjectUpdate(this);
                        client.Out.SendKeepComponentDetailUpdate(this); // I knwo this works, not sure if ObjectUpdate is needed - Tolakram
                    }
                }

                //m_oldHealthPercent = HealthPercent;
                if (oldStatus != Status)
                {
                    foreach (GameClient client in WorldMgr.GetClientsOfRegion(this.CurrentRegionID))
                    {
                        client.Out.SendKeepComponentDetailUpdate(this);
                    }
                }
                RepairedHealth = Health;
                //log.ErrorFormat("Keep HealthPercent: {0} ", HealthPercent);
                if (HealthPercent >= 25)
                {
                    //if a tower is repaired reload the guards so they arent on the floor
                    if (Keep is GameKeepTower && oldStatus == 0x02 && oldStatus != Status)
                    {
                        foreach (GameKeepComponent component in Keep.KeepComponents)
                            component.FillPositions();

                        SetLordPosition();
                    }
                }
            }
        }
       

        public override string ToString()
        {
            if (Keep == null)
            {
                return "Keep is null!";
            }

            return new StringBuilder(base.ToString())
                .Append(" ComponentID=").Append(ID)
                .Append(" Skin=").Append(Skin)
                .Append(" Height=").Append(Height)
                .Append(" Heading=").Append(Heading)
                .Append(" nComponentX=").Append((sbyte)ComponentX)
                .Append(" ComponentY=").Append((sbyte)ComponentY)
                .Append(" ComponentHeading=").Append(ComponentHeading)
                .ToString();
        }
    }
}
