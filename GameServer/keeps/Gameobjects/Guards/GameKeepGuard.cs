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
using DOL.GS.PacketHandler;
using DOL.GS.Spells;
using DOL.Language;
using System;
using System.Collections;
using System.Collections.Generic;


namespace DOL.GS.Keeps
{
    /// <summary>
    /// Keep guard is gamemob with just different brain and load from other DB table
    /// </summary>
    public class GameKeepGuard : GameNPC, IKeepItem
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected const int BASEFORMATIONDIST = 50;
        private const string LOSEFFECTIVENESSGuards = "LOS Effectivness Guards";

        private Patrol m_Patrol = null;
        public Patrol PatrolGroup
        {
            get { return m_Patrol; }
            set { m_Patrol = value; }
        }

        private string m_templateID = String.Empty;
        public string TemplateID
        {
            get { return m_templateID; }
            set { m_templateID = value; }
        }

        private GameKeepComponent m_component;
        public GameKeepComponent Component
        {
            get { return m_component; }
            set { m_component = value; }
        }

        private DBKeepPosition m_dbposition;
        public DBKeepPosition DBPosition
        {
            get { return m_dbposition; }
            set { m_dbposition = value; }
        }

        private GameKeepHookPoint m_hookPoint;
        public GameKeepHookPoint HookPoint
        {
            get { return m_hookPoint; }
            set { m_hookPoint = value; }
        }

        private eRealm m_modelRealm = eRealm.None;
        public eRealm ModelRealm
        {
            get { return m_modelRealm; }
            set { m_modelRealm = value; }
        }

        public bool IsTowerGuard
        {
            get
            {
                if (this.Component != null && this.Component.Keep != null)
                {
                    return this.Component.Keep is GameKeepTower;
                }
                return false;
            }
        }

        public bool IsPortalKeepGuard
        {
            get
            {
                if (this.Component == null || this.Component.Keep == null)
                    return false;
                return this.Component.Keep.IsPortalKeep;
            }
        }

        /// <summary>
        /// We do this because if we set level when a guard is waiting to respawn,
        /// the guard will never respawn because the guard is given full health and
        /// is then considered alive
        /// </summary>
        public override byte Level
        {
            get
            {
                if (IsPortalKeepGuard)
                    return 255;

                return base.Level;
            }
            set
            {
                if (this.IsRespawning)
                    m_level = value;
                else
                    base.Level = value;
            }
        }

        public override double GetArmorAbsorb(eArmorSlot slot)
        {
            double abs = GetModified(eProperty.ArmorAbsorption);

            if (this is GuardLord)
            {
                abs += 5;
            }
            else if (this is GuardCaster)
            {
                abs -= 5;
            }

            return Math.Max(0.0, abs * 0.01);
        }

        /// <summary>
        /// Guards always have Mana to cast spells
        /// </summary>
        public override int Mana
        {
            get { return 50000; }
        }

        public override int MaxHealth
        {
            get { return GetModified(eProperty.MaxHealth) + (base.Level * 4); }
        }

        private bool m_changingPositions = false;


        #region Combat

        /// <summary>
        /// Here we set the speeds we want our guards to have, this affects weapon damage
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public override int AttackSpeed(params InventoryItem[] weapon)
        {
            //speed 1 second = 10
            int speed = 0;
            switch (ActiveWeaponSlot)
            {
                case eActiveWeaponSlot.Distance: speed = 45; break; //elcotek vorher 45
                case eActiveWeaponSlot.TwoHanded: speed = 40; break;
                default: speed = 24; break;
            }
            speed = speed + Util.Random(10) + 1;
            return speed * 100;
        }

        /// <summary>
        /// When moving guards have difficulty attacking players, so we double there attack range)
        /// </summary>
        public override int AttackRange
        {
            get
            {
                int range = base.AttackRange;
                              

                if (IsMoving && ActiveWeaponSlot != eActiveWeaponSlot.Distance)
                    range *= 1; //elcotek no extra range
                return range;
            }
            set
            {
                base.AttackRange = value;
            }
        }

        /// <summary>
        /// The distance attack range
        /// </summary>
        public virtual int AttackRangeDistance
        {
            get
            {
                return 0;
            }
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
        /// We need an event after an attack is finished so we know when players are unreachable by archery
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        public static void AttackFinished(DOLEvent e, object sender, EventArgs arguments)
        {
            GameKeepGuard guard = sender as GameKeepGuard;

            if (guard.TargetObject == null)
                return;


            if (!guard.AttackState)
                return;

            if (guard is GuardArcher == false && guard is GuardLord == false && guard is GuardCaster == false)
                return;

            AttackFinishedEventArgs afargs = arguments as AttackFinishedEventArgs;

            if (guard.ActiveWeaponSlot != eActiveWeaponSlot.Distance && !guard.IsMoving)
            {
                eAttackResult result = afargs.AttackData.AttackResult;
                if (result == eAttackResult.OutOfRange)
                {
                    guard.StopAttack();
                    lock (guard.Attackers)
                    {
                        foreach (GameLiving living in guard.Attackers)
                        {
                            if (guard.IsWithinRadius(living, guard.AttackRange))
                            {
                                guard.StartAttack(living);
                                return;
                            }
                        }
                    }

                    if (guard.IsWithinRadius(guard.TargetObject, guard.AttackRangeDistance))
                    {
                        if (guard.MaxSpeedBase == 0 || (guard is GuardArcher && !guard.BeenAttackedRecently))
                            guard.SwitchToRanged(guard.TargetObject);
                    }
                }
                return;
            }

            if (guard.ActiveWeaponSlot == eActiveWeaponSlot.Distance)
            {
                if (GameServer.ServerRules.IsAllowedToAttack(guard, guard.TargetObject as GameLiving, true) == false)
                {
                    guard.StopAttack();
                    return;
                }
                if (!guard.IsWithinRadius(guard.TargetObject, guard.AttackRange))
                {
                    guard.StopAttack();
                    return;
                }
            }

            GamePlayer player = null;

            if (guard.TargetObject is GamePlayer)
            {
                player = guard.TargetObject as GamePlayer;
            }
            else if (guard.TargetObject is GameNPC && (guard.TargetObject as GameNPC).Brain != null && (guard.TargetObject as GameNPC).Brain is IControlledBrain)
            {
                if (((guard.TargetObject as GameNPC).Brain as IControlledBrain).Owner is GamePlayer)
                {
                    player = ((guard.TargetObject as GameNPC).Brain as IControlledBrain).Owner as GamePlayer;
                }
            }
            if (player is GamePlayer && player != null && player.ObjectState == GameObject.eObjectState.Active && guard != null && guard.ObjectState == GameObject.eObjectState.Active)
            player.Out.SendCheckLOS(guard, player, new CheckLOSResponse(guard.GuardStopAttackCheckLOS));
        }

        /// <summary>
        /// Override for StartAttack which chooses Ranged or Melee attack
        /// </summary>
        /// <param name="attackTarget"></param>
        public override void StartAttack(GameObject attackTarget)
        {
            if (attackTarget == null || attackTarget.ObjectState != GameObject.eObjectState.Active)
                return;
            
            // Sicherstellen, dass wir nicht mit einem inaktiven Ziel arbeiten
            base.TargetObject = attackTarget;

            GameLiving target = attackTarget as GameLiving;

            // Null-Überprüfung für target
            if (target == null || !target.IsAlive)
                return;

            // Überprüfung auf erlaubte Angriffe
            if (!GameServer.ServerRules.IsAllowedToAttack(this, target, true))
                return;

            //Keep patrols
            if (LoadedFromScript && AttackState == false && attackTarget != null && Body.Brain is KeepGuardBrain && attackTarget.IsWithinRadius(this, (this.Brain as KeepGuardBrain).AggroRange) == false)
            {
                if (IsMoving == false || IsMovingOnPath == false)
                {
                    StartMoving();
                    MoveOnPath(225);
                }
                return;
            }
            


            if (IsPortalKeepGuard)
            {
                base.StartAttack(attackTarget);
                return;
            }

            if (CurrentSpellHandler != null)
                return;

            //Keep patrols
            if (LoadedFromScript && IsMovingOnPath)
            {
                StopMovingOnPath();
                StopMoving();
            }
   

            //Prevent spam for LOS to same target multiple times
            {
                GameObject lastTarget = (GameObject)this.TempProperties.getProperty<object>(LAST_LOS_TARGET_PROPERTY, null);
                long lastTick = this.TempProperties.getProperty<long>(LAST_LOS_TICK_PROPERTY);

                if (lastTarget != null && lastTarget == attackTarget)
                {
                    if (lastTick != 0 && this.CurrentRegion.Time - lastTick < ServerProperties.Properties.KEEP_GUARD_LOS_CHECK_TIME * 1000)
                    {
                        return;
                    }
                }
                else
                {
                   
                    //if (IsAttacking == false)
                    //log.Error("Target gewechselt!");
                    //StopAttack();
                    StopFollowing();
                }

                GamePlayer LOSChecker = null;
                if (attackTarget is GamePlayer)
                {
                   LOSChecker = attackTarget as GamePlayer;
                }
                else if (attackTarget is GameNPC && (attackTarget as GameNPC).Brain != null && (attackTarget as GameNPC).Brain is IControlledBrain)
                {
                    if (((attackTarget as GameNPC).Brain as IControlledBrain).Owner is GamePlayer)
                    {
                        LOSChecker = ((attackTarget as GameNPC).Brain as IControlledBrain).Owner as GamePlayer;
                    }
                }


                else
                {
                    // try to find another player to use for checking line of site
                    foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    {
                        LOSChecker = player;
                        break;
                    }
                }

                if (LOSChecker == null)
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
                        if (lastTick == 0 || this.CurrentRegion.Time - lastTick > ServerProperties.Properties.LOS_PLAYER_CHECK_FREQUENCY * 1000)
                        {
                           this.TempProperties.setProperty(NUM_LOS_CHECKS_INPROGRESS, 0);
                        }

                        return;
                    }
                    
                    count++;
                    this.TempProperties.setProperty(NUM_LOS_CHECKS_INPROGRESS, count);

                    //this.TempProperties.setProperty(LAST_LOS_TARGET_PROPERTY, attackTarget);
                    this.TempProperties.setProperty(LAST_LOS_TICK_PROPERTY, CurrentRegion.Time);
                    TargetObject = attackTarget;
                    
                }
               
                LOSChecker.TempProperties.setProperty(LOSEFFECTIVENESSGuards + attackTarget.ObjectID, 1.0);
                LOSChecker.Out.SendCheckLOS(this, attackTarget, new CheckLOSResponse(this.GuardStartAttackCheckLOS));

            }
          

        }


        /// <summary>
        /// We only attack if we have LOS
        /// </summary>
        /// <param name="player"></param>
        /// <param name="response"></param>
        /// <param name="targetOID"></param>
        public void GuardStartAttackCheckLOS(GameLiving player, ushort response, ushort targetOID)
        {
            GameLiving target = null;
            

            lock (LOS_LOCK)
            {
                int count = TempProperties.getProperty<int>(NUM_LOS_CHECKS_INPROGRESS, 0);
                count--;
                TempProperties.setProperty(NUM_LOS_CHECKS_INPROGRESS, Math.Max(0, count));
            }
            

            if ((response & 0x100) == 0x100) // || NeedNoLos(Spell) == true)
            {
               
                if (this != null && this.ObjectState == GameLiving.eObjectState.Active && this.CurrentRegion.GetObject(targetOID) is GameLiving)

                    target = (this.CurrentRegion.GetObject(targetOID) as GameLiving);
               
                if (target != null && target.ObjectState == GameLiving.eObjectState.Active)
                {
                 

                    // make sure we didn't switch targets
                    if (target != null)
                    {
                       // log.ErrorFormat("kann {0} angreifen", target.Name);

                        if (this is GuardArcher || this is GuardLord)
                        {
                            if (ActiveWeaponSlot != eActiveWeaponSlot.Distance)
                            {
                                if (CanUseRanged)

                                    SwitchToRanged(target);
                            }
                        }
                        
                        if (target != null && Brain is KeepGuardBrain && target.IsWithinRadius(this, (this.Brain as KeepGuardBrain).AggroRange) == false)
                        {
                            if (!IsWithinRadius(target, AttackRange + 100))
                                StopAttack();

                            PathTo(target.X, target.Y, target.Z, MaxSpeed);
                        }
                        else
                        {
                            //log.Debug("StartAttack2");
                            base.StartAttack(target);
                            ContinueStartAttack(target);
                        }
                        ///log.ErrorFormat("start attck {0}", TargetObject.Name);
                        //base.StartAttack(target);
                        //ContinueStartAttack(target);
                        player.TempProperties.removeProperty(LOSEFFECTIVENESSGuards + target.ObjectID);
                        CanAttack();
                    }

                }
            }
            else
            {
               
                //if (InCombat == false && IsAttacking == false || this is GuardArcher || this is GuardCaster)
                if (BodyType == 1576)
                {
                    if (this is GuardArcher || this is GuardCaster)
                    {
                        Body.StopMovingOnPath();
                        StopFollowing();
                        //StopAttack();
                    }
                    else
                        Body.StopMovingOnPath();
                }

                if (BodyType != 1576)
                    // StopAttack();
                    StopFollowing();
                if (m_targetLOSObject != null && m_targetLOSObject is GameLiving && Brain != null && Brain is IOldAggressiveBrain)
                {
                    if (Attackers.Contains(m_targetLOSObject))
                    {
                        RemoveAttacker(m_targetLOSObject as GameLiving);
                        // there will be a think delay before mob attempts to attack next target
                        // (Brain as IOldAggressiveBrain).RemoveFromAggroList(m_targetLOSObject as GameLiving);
                    }
                }

            }
        }

        /// <summary>
        /// Attack coninue
        /// </summary>
        /// <returns></returns>
        public bool CanAttack()
        {
           //log.Error("Attack is now allowed");
            return true;
        }
        /// <summary>
        /// If we don't have LOS we stop attack
        /// </summary>
        /// <param name="player"></param>
        /// <param name="response"></param>
        /// <param name="targetOID"></param>
        public void GuardStopAttackCheckLOS(GameLiving player, ushort response, ushort targetOID)
        {
            if ((response & 0x100) != 0x100)
            {
                StopAttack();

                if (TargetObject != null && TargetPosition is GameLiving && this.Brain is KeepGuardBrain)
                {
                    (this.Brain as KeepGuardBrain).RemoveFromAggroList(TargetObject as GameLiving);
                }
            }
        }

        public void GuardStartSpellHealCheckLOS(GameLiving player, ushort response, ushort targetOID)
        {
            if ((response & 0x100) == 0x100)
            {
                SpellMgr.CastHealSpell(this, TargetObject as GameLiving);
            }
        }


        public void GuardStartSpellNukeCheckLOS(GameLiving player, ushort response, ushort targetOID)
        {
            if ((response & 0x100) == 0x100)
            {
                SpellMgr.CastNukeSpell(this, TargetObject as GameLiving);
            }
        }


        /// <summary>
        /// Method to see if the Guard has been left alone long enough to use Ranged attacks
        /// </summary>
        /// <returns></returns>
        public bool CanUseRanged
        {
            get
            {
                if (this.ObjectState != GameObject.eObjectState.Active) return false;
                if (this is GuardFighter) return false;
                if (this is GuardArcher || this is GuardLord)
                {
                    if (this.Inventory == null) return false;
                    if (this.Inventory.GetItem(eInventorySlot.DistanceWeapon) == null) return false;
                   // if (this.AttackRange < 200 && this.TargetObject != null && this.IsWithinRadius(this.TargetObject, this.AttackRange)) return false;
                    if (this.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Distance) return false;
                }
                if (this is GuardCaster || this is GuardHealer)
                {
                    if (this.CurrentSpellHandler != null) return false;
                }
                return !this.BeenAttackedRecently;
            }
        }

       
        /// <summary>
        /// Static archers attack with melee the closest if being engaged in melee
        /// </summary>
        /// <param name="ad"></param>
        public override void OnAttackedByEnemy(AttackData ad)
        {
            //this is for static archers only
            if (MaxSpeedBase == 0)
            {
                //if we are currently fighting in melee
                if (ActiveWeaponSlot == eActiveWeaponSlot.Standard || ActiveWeaponSlot == eActiveWeaponSlot.TwoHanded)
                {
                    //if we are targeting something, and the distance to the target object is greater than the attack range
                    if (TargetObject != null && !this.IsWithinRadius(TargetObject, AttackRange))
                    {
                        //stop the attack
                        StopAttack();
                        //if the distance to the attacker is less than the attack range
                        if (this.IsWithinRadius(ad.Attacker, AttackRange))
                        {
                            //attack it
                            StartAttack(ad.Attacker);
                        }
                    }
                }
            }
            base.OnAttackedByEnemy(ad);
        }
        private bool ComponentWasRised = false;


        /// <summary>
        /// When guards Die and it isnt a keep reset (this killer) we call GuardSpam function
        /// </summary>
        /// <param name="killer"></param>
        public virtual void DieAfterRize(GameObject killer, bool wasrised)
        {
            //log.Debug("guard ist gestorben");


            if (wasrised)
                ComponentWasRised = true;//component was raised so dont reset the guild of the lord
            if (wasrised == false)
                ComponentWasRised = false;

            if (killer != this && wasrised == false)
                //if (killer != this && (killer is GamePlayer || killer is GameNPC))
                GuardSpam(this);
            base.Die(killer);
            if (RespawnInterval == -1)
                Delete();
        }


        /// <summary>
        /// When guards Die and it isnt a keep reset (this killer) we call GuardSpam function
        /// </summary>
        /// <param name="killer"></param>
        public override void Die(GameObject killer)
        {
            //log.Debug("guard ist gestorben");
                      

           
            if (killer != this && (killer is GamePlayer || killer is GameNPC))
                GuardSpam(this);
            base.Die(killer);
            if (RespawnInterval == -1)
                Delete();
        }

        #region Guard Spam
        /// <summary>
        /// Sends message to guild for guard death with enemy count in area
        /// </summary>
        /// <param name="guard">The guard object</param>
        public static void GuardSpam(GameKeepGuard guard)
        {
            if (guard.Component == null) return;
            if (guard.Component.Keep == null) return;
            if (guard.Component.Keep.Guild == null) return;

            int inArea = guard.GetEnemyCountInArea();
            string message = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameKeepGuard.GuardSpam.Killed", guard.Name, guard.Component.Keep.Name, inArea);
            KeepGuildMgr.SendMessageToGuild(message, guard.Component.Keep.Guild);
        }

        /// <summary>
        /// Gets the count of enemies in the Area
        /// </summary>
        /// <returns></returns>
        public int GetEnemyCountInArea()
        {
            int inArea = 0;
            foreach (GamePlayer NearbyPlayers in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                if (this.Component != null)
                {
                    if (KeepMgr.IsEnemy(this.Component.Keep, NearbyPlayers))
                        inArea++;
                }
                else
                {
                    if (GameServer.ServerRules.IsAllowedToAttack(this, NearbyPlayers, true))
                        inArea++;
                }
            }
            return inArea;
        }


        #endregion

        /// <summary>
        /// Has the NPC been attacked recently.. currently 10 seconds
        /// </summary>
        public bool BeenAttackedRecently
        {
            get
            {
                return CurrentRegion.Time - LastAttackedByEnemyTick < 10 * 1000;
            }
        }
        #endregion

        /// <summary>
        /// When we add a guard to the world, we also attach an AttackFinished handler
        /// We use this to check LOS and range issues for our ranged guards
        /// </summary>
        /// <returns></returns>
        public override bool AddToWorld()
        {
            base.RoamingRange = 0;
            base.TetherRange = 10000;

            if (!base.AddToWorld())
                return false;

            if (IsPortalKeepGuard && (Brain as KeepGuardBrain != null))
            {
                (this.Brain as KeepGuardBrain).AggroRange = 2000;
                (this.Brain as KeepGuardBrain).AggroLevel = 99;
            }

            GameEventMgr.AddHandler(this, GameNPCEvent.AttackFinished, new DOLEventHandler(AttackFinished));

            if (PatrolGroup != null && !m_changingPositions)
            {
                bool foundGuard = false;
                foreach (GameKeepGuard guard in PatrolGroup.PatrolGuards)
                {
                    if (guard.IsAlive && guard.CurrentWayPoint != null)
                    {
                        CurrentWayPoint = guard.CurrentWayPoint;
                        m_changingPositions = true;
                        MoveTo(guard.CurrentRegionID, guard.X - Util.Random(200, 350), guard.Y - Util.Random(200, 350), guard.Z, guard.Heading);
                        m_changingPositions = false;
                        foundGuard = true;
                        break;
                    }
                }

                if (!foundGuard)
                    CurrentWayPoint = PatrolGroup.PatrolPath;

                MoveOnPath(Patrol.PATROL_SPEED);
            }

            return true;
        }

        /// <summary>
        /// When we remove from world, we remove our special handler
        /// </summary>
        /// <returns></returns>
        public override bool RemoveFromWorld()
        {
            if (base.RemoveFromWorld())
            {
                GameEventMgr.RemoveHandler(this, GameNPCEvent.AttackFinished, new DOLEventHandler(AttackFinished));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Method to stop a guards respawn
        /// </summary>
        public void StopRespawn()
        {
            if (IsRespawning)
                m_respawnTimer.Stop();
        }

        /// <summary>
        /// When guards respawn we refresh them, if a patrol guard respawns we
        /// call a special function to update leadership
        /// </summary>
        /// <param name="respawnTimer"></param>
        /// <returns></returns>
        protected override int RespawnTimerCallback(NPCRegionTimer respawnTimer)
        {
            int temp = base.RespawnTimerCallback(respawnTimer);
            if (Component != null && Component.Keep != null)
            {
                Component.Keep.TemplateManager.GetMethod("RefreshTemplate").Invoke(null, new object[] { this });
            }
            else
            {
                TemplateMgr.RefreshTemplate(this);
            }
            return temp;
        }

        /// <summary>
        /// Gets the messages when you click on a guard
        /// </summary>
        /// <param name="player">The player that has done the clicking</param>
        /// <returns></returns>
        public override IList GetExamineMessages(GamePlayer player)
        {
            //You target [Armwoman]
            //You examine the Armswoman. She is friendly and is a realm guard.
            //She has upgraded equipment (5).
            List<string> list = new List<string>(4);
            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameKeepGuard.GetExamineMessages.YouTarget", GetName(0, false)));
            if (Realm != eRealm.None)
            {
                list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameKeepGuard.GetExamineMessages.YouExamine", GetName(0, false), GetPronoun(0, true), GetAggroLevelString(player, false)));
                if (this.Component != null)
                {
                    string text = String.Empty;
                    if (this.Component.Keep.Level > 1 && this.Component.Keep.Level < 250 && GameServer.ServerRules.IsSameRealm(player, this, true))
                        text = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameKeepGuard.GetExamineMessages.Upgraded", GetPronoun(0, true), this.Component.Keep.Level);
                    if (ServerProperties.Properties.USE_KEEP_BALANCING && this.Component.Keep.Region == 163 && !(this.Component.Keep is GameKeepTower))
                        text += LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameKeepGuard.GetExamineMessages.Balancing", GetPronoun(0, true), (Component.Keep.BaseLevel - 50).ToString());
                    if (String.IsNullOrEmpty(text) == false)
                        list.Add(text);
                }
            }
            return list;
        }

        /// <summary>
        /// Gets the pronoun for the guards gender
        /// </summary>
        /// <param name="form">Form of the pronoun</param>
        /// <param name="firstLetterUppercase">Weather or not we want the first letter uppercase</param>
        /// <returns></returns>
        public override string GetPronoun(int form, bool firstLetterUppercase)
        {
            string s = String.Empty;
            switch (form)
            {
                default:
                    {
                        // Subjective
                        if (Gender == GS.eGender.Male)
                            s = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameKeepGuard.GetPronoun.He");
                        else s = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameKeepGuard.GetPronoun.She");
                        if (!firstLetterUppercase)
                            s = s.ToLower();
                        break;
                    }
                case 1:
                    {
                        // Possessive
                        if (Gender == eGender.Male)
                            s = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameKeepGuard.GetPronoun.His");
                        else s = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameKeepGuard.GetPronoun.Hers");
                        if (!firstLetterUppercase)
                            s = s.ToLower();
                        break;
                    }
                case 2:
                    {
                        // Objective
                        if (Gender == eGender.Male)
                            s = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameKeepGuard.GetPronoun.Him");
                        else s = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameKeepGuard.GetPronoun.Her");
                        if (!firstLetterUppercase)
                            s = s.ToLower();
                        break;
                    }
            }
            return s;
        }

        #region Database

        string m_dataObjectID = "";

       
        public override void LoadFromDatabase(DataObject mobobject)
        {
            if (mobobject == null) return;
            base.LoadFromDatabase(mobobject);
            string sKey = mobobject.ObjectId;
            foreach (AbstractArea area in this.CurrentAreas)
            {
                if (area is GameKeepArea keepArea)
                {
                    Component = new GameKeepComponent();
                    Component.Keep = keepArea.Keep;
                    m_dataObjectID = mobobject.ObjectId;
                    // mob reload command might be reloading guard, so check to make sure it isn't already added
                    if (Component.Keep.Guards.ContainsKey(sKey) == false)
                        Component.Keep.Guards.Add(sKey, this);
                    // break; This is a bad idea.  If there are multiple KeepAreas, we should put a guard on each
                }
            }

            TemplateMgr.RefreshTemplate(this);
        }


        public void DeleteObject()
        {
            if (Component != null)
            {
                if (Component.Keep != null)
                {
                    string skey = m_dataObjectID;
                    if (Component.Keep.Guards.ContainsKey(skey))
                        Component.Keep.Guards.Remove(skey);
                    else if (log.IsWarnEnabled)
                        log.Warn($"Can't find {DBPosition.ClassType} with dataObjectId {m_dataObjectID} in Component InternalID {Component.InternalID} Guard list.");
                }
                else if (log.IsWarnEnabled)
                    log.Warn($"Keep is null on delete of guard {Name} with dataObjectId {m_dataObjectID}");

                Component.Delete();
            }
            else if (log.IsWarnEnabled)
                log.Warn($"Component is null on delete of guard {Name} with dataObjectId {m_dataObjectID}");

            HookPoint = null;
            Component = null;
            if (Inventory != null)
                Inventory.ClearInventory();
            Inventory = null;
            DBPosition = null;
            TempProperties.removeAllProperties();

            base.Delete();

            SetOwnBrain(null);
            CurrentRegion = null;

            GameEventMgr.RemoveAllHandlersForObject(this);
        }

        public override void Delete()
        {
            if (HookPoint != null && Component != null)
                Component.Keep.Guards.Remove(m_templateID); //Remove(this.ObjectID); LoadFromPosition() uses position.TemplateID as the insertion key

            TempProperties.removeAllProperties();

            base.Delete();
        }

        public override void DeleteFromDatabase()
        {
            foreach (AbstractArea area in this.CurrentAreas)
            {
                if (area is GameKeepArea && Component != null)
                {
                    if (Component.Keep.Guards.ContainsKey(this.InternalID))
                        Component.Keep.Guards.Remove(m_dataObjectID); //Remove(this.InternalID); LoadFromDatabase() adds using m_dataObjectID
                    break;                                       // break; This is a bad idea.  If there are multiple KeepAreas, we could end up with instantiated keep items that are no longer in the DB
                }
            }
            base.DeleteFromDatabase();
        }


        /// <summary>
        /// Load the guard from a position
        /// </summary>
        /// <param name="pos">The position for the guard</param>
        /// <param name="component">The component it is being spawned on</param>
        public void LoadFromPosition(DBKeepPosition pos, GameKeepComponent component)
        {
            m_templateID = pos.TemplateID;
            m_component = component;
            component.Keep.Guards[m_templateID] = this;
            PositionMgr.LoadGuardPosition(pos, this);
            if (Component != null && Component.Keep != null)
            {
                Component.Keep.TemplateManager.GetMethod("RefreshTemplate").Invoke(null, new object[] { this });
            }
            else
            {
                TemplateMgr.RefreshTemplate(this);
            }
            this.AddToWorld();
        }

        /// <summary>
        /// Move a guard to a position
        /// </summary>
        /// <param name="position">The new position for the guard</param>
        public void MoveToPosition(DBKeepPosition position)
        {
            PositionMgr.LoadGuardPosition(position, this);
            if (!this.InCombat)
                this.MoveTo(this.CurrentRegionID, this.X, this.Y, this.Z, this.Heading);
        }
        #endregion

        /// <summary>
        /// Change guild of guard (emblem on equipment) when keep is claimed
        /// </summary>
        public void ChangeGuild()
        {
           if (ComponentWasRised == false)//component was raised so dont reset the guild

                ClothingMgr.EquipGuard(this);

            Guild guild = this.Component.Keep.Guild;
            string guildname = String.Empty;
            if (guild != null)
                guildname = guild.Name;

            this.GuildName = guildname;

            if (this.Inventory == null)
                return;

            int emblem = 0;
            if (guild != null)
                emblem = guild.Emblem;
            InventoryItem lefthand = this.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
            if (lefthand != null)
                lefthand.Emblem = emblem;

            InventoryItem cloak = this.Inventory.GetItem(eInventorySlot.Cloak);
            if (cloak != null)
            {
                cloak.Emblem = emblem;

                if (cloak.Emblem != 0)
                    cloak.Model = 558; // change to a model that looks ok with an emblem

            }
            if (IsAlive)
            {
                UpdateNPCEquipmentAppearance();
            }
        }
        /// <summary>
        /// Holds the controlled object
        /// </summary>
        protected GameKeepGuard[] m_GuardFightern = null;

        /// <summary>
        /// Gets the controlled object of this NPC
        /// </summary>
        public virtual GameKeepGuard GuardFightern
        {
            get
            {
                if (m_GuardFightern == null) return null;
                return m_GuardFightern[0];
            }
        }

        /// <summary>
        /// <summary>
        /// Gets the controlled array of this NPC
        /// </summary>
        public GameKeepGuard[] ControlledNpcList1
        {
            get { return m_GuardFightern; }
        }
       
        /// <summary>
        /// Walk to the spawn point, always max speed for keep guards, or continue patrol.
        /// </summary>
        public override void WalkToSpawn()
        {
            if (PatrolGroup != null)
            {
                StopAttack();
                StopFollowing();

                StandardMobBrain brain = Brain as StandardMobBrain;
                if (brain != null && brain.HasAggro)
                {
                    brain.ClearAggroList();
                }

                PatrolGroup.StartPatrol();
            }
            else
            {
                WalkToSpawn(MaxSpeed);
            }
        }

    }
}
