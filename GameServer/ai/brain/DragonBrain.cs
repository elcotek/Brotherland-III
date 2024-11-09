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
using DOL.Events;
using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.Movement;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.Language;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace DOL.AI.Brain
{
    /// <summary>
    /// AI for dragon like NPCs.
    /// </summary>
    /// <author>Aredhel</author>
    public class DragonBrain : StandardMobBrain, IOldAggressiveBrain
    {
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        
        /// <summary>
        /// Create a new DragonBrain.
        /// </summary>
        public DragonBrain()
            : base()
        {
            base.m_aggroLevel = 0;
            base.m_aggroMaxRange = 0;
            base.AggroRange = 500;
            base.AggroLevel = 65;
        }

        /// <summary>
        /// Do the mob AI
        /// </summary>
        public override void Think()
        {


            //Satyr:
            //This is a general information. When i review this Think-Procedure and the interaction between it and some
            //code of GameNPC.cs i have the feeling this is a mixture of much ideas of diffeent people, much unfinished
            //features like random-walk which does not actually fit to the rest of this Brain-logic.
            //In other words:
            //If somebody feeling like redoing this stuff completly i would appreciate it. It might be worth redoing
            //instead of trying desperately to make something work that is simply chaoticly moded by too much
            //diffeent inputs.
            //For NOW i made the aggro working the following way (close to live but not yet 100% equal):
            //Mobs will not aggro on their way back home (in fact they should even under some special circumstances)
            //They will completly forget all Aggro when respawned and returned Home.


            // If the NPC is tethered and has been pulled too far it will
            // de-aggro and return to its spawn point.
            /*if (Body.IsOutOfTetherRange && !Body.InCombat)
            {
                Body.WalkToSpawn();
                return;
            }*/
            if (CheckHealth()) return;
            if (PickGlareTarget()) return;
            PickThrowTarget();

            //Instead - lets just let CheckSpells() make all the checks for us
            //Check for just positive spells
            CheckSpells(eCheckSpellType.Defensive);

            // Note: Offensive spells are checked in GameNPC:SpellAction timer

            // check for returning to home if to far away
            if (Body.MaxDistance != 0 && !Body.IsReturningHome)
            {
                int distance = Body.GetDistanceTo(Body.SpawnPoint);
                int maxdistance = Body.MaxDistance > 0 ? Body.MaxDistance : -Body.MaxDistance * AggroRange / 100;
                if (maxdistance > 0 && distance > maxdistance)
                {
                    Body.WalkToSpawn();
                    return;
                }
            }
            if (string.IsNullOrEmpty(Body.PathID) == false && !Body.AttackState)
            {
                Body.DragonFly();
            }

            //If this NPC can randomly walk around, we allow it to walk around
            if (!Body.AttackState && CanRandomWalk && Util.Chance(DOL.GS.ServerProperties.Properties.GAMENPC_RANDOMWALK_CHANCE))
            {
                IPoint3D target = CalcRandomWalkTarget();
                if (target != null)
                {
                    if (Util.IsNearDistance(target.X, target.Y, target.Z, Body.X, Body.Y, Body.Z, GameNPC.CONST_WALKTOTOLERANCE))
                    {
                        Body.TurnTo(Body.GetHeading(target));
                    }
                    else
                    {
                        Body.PathTo(target.X, target.Y, target.Z, 50);
                    }

                    Body.FireAmbientSentence(GameNPC.eAmbientTrigger.roaming);
                }
            }
            //If the npc can move, and the npc is not casting, not moving, and not attacking or in combat
            //else if (Body.MaxSpeedBase > 0 && Body.CurrentSpellHandler == null && !Body.IsMoving && !Body.AttackState && !Body.InCombat)
            else if (Body.MaxSpeedBase > 0 && Body.CurrentSpellHandler == null && !Body.IsMoving && !Body.AttackState && !Body.InCombat && !Body.IsMovingOnPath)
            {
                //If the npc is not at it's spawn position, we tell it to walk to it's spawn position
                //Satyr: If we use a tolerance to stop their Way back home we also need the same
                //Tolerance to check if we need to go home AGAIN, otherwise we might be told to go home
                //for a few units only and this may end before the next Arrive-At-Target Event is fired and in this case
                //We would never lose the state "IsReturningHome", which is then followed by other erros related to agro again to players
                if (!Util.IsNearDistance(Body.X, Body.Y, Body.Z, Body.SpawnPoint.X, Body.SpawnPoint.Y, Body.SpawnPoint.Z, GameNPC.CONST_WALKTOTOLERANCE))
                    Body.WalkToSpawn();
                else if (Body.Heading != Body.SpawnHeading)
                    Body.Heading = Body.SpawnHeading;
            }

            //Mob will now always walk on their path
            if (Body.MaxSpeedBase > 0 && Body.CurrentSpellHandler == null && !Body.IsMoving
                && !Body.AttackState && !Body.InCombat && !Body.IsMovingOnPath
                && Body.PathID != null && Body.PathID != "" && Body.PathID != "NULL")
            {
                PathPoint path = MovementMgr.LoadPath(Body.PathID);
                if (path != null)
                {
                    Body.CurrentWayPoint = path;
                    Body.MoveOnPath((short)path.MaxSpeed);
                }
                else
                {
                    log.ErrorFormat("Path {0} not found for mob {1}.", Body.PathID, Body.Name);
                }
            }

            //If we are not attacking, and not casting, and not moving, and we aren't facing our spawn heading, we turn to the spawn heading
            //if( !Body.InCombat && !Body.AttackState && !Body.IsCasting && !Body.IsMoving && Body.IsWithinRadius( Body.SpawnPoint, 500 ) == false )
            if (!Body.IsMovingOnPath && !Body.InCombat && !Body.AttackState && !Body.IsCasting && !Body.IsMoving && Body.IsWithinRadius(Body.SpawnPoint, 500) == false)
            {
                Body.WalkToSpawn(); // Mobs do not walk back at 2x their speed..
                Body.IsReturningHome = false; // We are returning to spawn but not the long walk home, so aggro still possible
            }

            if (Body.IsReturningHome == false)
            {
                if (!Body.AttackState && AggroRange > 0)
                {
                    var currentPlayersSeen = new List<GamePlayer>();
                    foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)AggroRange))
                    {
                        if (!PlayersSeen.Contains(player))
                        {
                            Body.FireAmbientSentence(GameNPC.eAmbientTrigger.seeing, player as GameLiving);
                            PlayersSeen.Add(player);
                        }
                        currentPlayersSeen.Add(player);
                    }

                    for (int i = 0; i < PlayersSeen.Count; i++)
                    {
                        if (!currentPlayersSeen.Contains(PlayersSeen[i])) PlayersSeen.RemoveAt(i);
                    }

                }
                //If we have an aggrolevel above 0, we check for players and npcs in the area to attack
                if (!Body.AttackState && AggroLevel > 0)
                {
                    CheckPlayerAggro();
                    CheckNPCAggro();
                }

                if (HasAggro)
                {
                    Body.FireAmbientSentence(GameNPC.eAmbientTrigger.fighting);
                    AttackMostWanted();
                    return;
                }
                else
                {
                    if (Body.AttackState)
                        Body.StopAttack();

                    Body.TargetObject = null;
                }
            }
        }

        /// <summary>
        /// Check for aggro against close NPCs
        /// </summary>
        protected override void CheckNPCAggro()
        {
            //if (Body.AttackState) return;

            if (HasAggro) return;

            foreach (GameNPC npc in Body.GetNPCsInRadius((ushort)AggroRange))
            {
                if (!GameServer.ServerRules.IsAllowedToAttack(Body, npc, true)) continue;

              

                if (m_aggroTable.ContainsKey(npc))
                    continue; // add only new NPCs
                if (!npc.IsAlive || npc.ObjectState != GameObject.eObjectState.Active)
                    continue;
                if (npc is GameTaxi)
                    continue; //do not attack horses

                if (CalculateAggroLevelToTarget(npc) > 0)
                {
                    //AddToAggroList(npc, (npc.Level + 1) << 1);
                    if (npc.Brain is ControlledNpcBrain) // This is a pet or charmed creature, checkLOS
                        AddToAggroList(npc, 1, true);
                    else
                        AddToAggroList(npc, 1);
                }
            }
        }


        /// <summary>
        /// Check for aggro against players
        /// </summary>
        protected override void CheckPlayerAggro()
        {
            //Check if we are already attacking, return if yes
            //if (Body.AttackState) return;

            if (HasAggro) return;

            foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)AggroRange))
            {

                if (!GameServer.ServerRules.IsAllowedToAttack(Body, player, true)) continue;
                // Don't aggro on immune players.

              

                if (player.EffectList.GetOfType<NecromancerShadeEffect>() != null)
                    continue;

                int aggrolevel = 0;

                if (Body.Faction != null)
                {
                    aggrolevel = Body.Faction.GetAggroToFaction(player);
                    if (aggrolevel < 0)
                        aggrolevel = 0;
                }

                if (aggrolevel <= 0 && AggroLevel <= 0)
                    return;

                if (m_aggroTable.ContainsKey(player))
                    continue; // add only new players
                if (!player.IsAlive || player.ObjectState != GameObject.eObjectState.Active || player.IsStealthed)
                    continue;
                if (player.Steed != null)
                    continue; //do not attack players on steed

                if (CalculateAggroLevelToTarget(player) > 0)
                {
                    //AddToAggroList(player, player.EffectiveLevel << 1, true);
                    AddToAggroList(player, 1, true);
                }
            }
        }
        /// <summary>
        /// The interval for thinking, min 1.5 seconds
        /// 10 seconds for 0 aggro mobs
        /// </summary>
        public override int ThinkInterval
        {
            get { return Math.Max(1500, 10000 - AggroLevel * 100); }
        }

        /// <summary>
        /// If this brain is part of a formation, it edits it's values accordingly.
        /// </summary>
        /// <param name="x">The x-coordinate to refer to and change</param>
        /// <param name="y">The x-coordinate to refer to and change</param>
        /// <param name="z">The x-coordinate to refer to and change</param>
        public virtual bool CheckFormation(ref int x, ref int y, ref int z)
        {
            return false;
        }

        /// <summary>
        /// Checks the Abilities
        /// </summary>
        public override void CheckAbilities()
        {
            //See CNPC
        }
        #region Aggro

      
        /// <summary>
        /// Aggressive Level in % 0..100, 0 means not Aggressive
        /// </summary>
        public override int AggroLevel
        {
            get { return m_aggroLevel; }
            set { m_aggroLevel = value; }
        }

        /// <summary>
        /// Range in that this npc aggros
        /// </summary>
        public override int AggroRange
        {
            get { return m_aggroMaxRange; }
            set { m_aggroMaxRange = value; }
        }

        /// <summary>
        /// Checks whether living has someone on its aggrolist
        /// </summary>
        public override bool HasAggro
        => AggroTable.Count > 0;



        // LOS Check on natural aggro (aggrorange & aggrolevel)
        // This part is here due to los check constraints;
        // Otherwise, it should be in CheckPlayerAggro() method.
        private bool m_AggroLOS;
        public override bool AggroLOS
        {
            get { return m_AggroLOS; }
            set { m_AggroLOS = value; }
        }
        private void CheckAggroLOS(GameLiving player, ushort response, ushort targetOID)
        {
            if ((response & 0x100) == 0x100)
                AggroLOS = true;
            else
                AggroLOS = false;
        }

        /// <summary>
        /// Add living to the aggrolist
        /// aggroamount can be negative to lower amount of aggro
        /// </summary>
        /// <param name="living"></param>
        /// <param name="aggroamount"></param>
        public override void AddToAggroList(GameLiving living, int aggroamount)
        {
            AddToAggroList(living, aggroamount, false);
        }

        public override void AddToAggroList(GameLiving living, int aggroamount, bool LosCheck)
        {
            if (m_body.IsConfused) return;

            // tolakram - duration spell effects will attempt to add to aggro after npc is dead
            if (!m_body.IsAlive) return;

            if (living == null) return;

            //Handle trigger to say sentance on first aggro.
            if (m_aggroTable.Count < 1)
                Body.FireAmbientSentence(GameNPC.eAmbientTrigger.aggroing, living);

            // Check LOS (walls, pits, etc...) before  attacking, player + pet
            // Be sure the aggrocheck is triggered by the brain on Think() method
            if (DOL.GS.ServerProperties.Properties.ALWAYS_CHECK_LOS && LosCheck)
            {
                GamePlayer thisLiving = null;
                if (living is GamePlayer)
                    thisLiving = (GamePlayer)living;

                if (living is GamePet)
                {
                    IControlledBrain brain = ((GamePet)living).Brain as IControlledBrain;
                    thisLiving = brain.GetPlayerOwner();
                }

                if (thisLiving != null)
                {
                    thisLiving.Out.SendCheckLOS(Body, living, new CheckLOSResponse(CheckAggroLOS));
                    if (!AggroLOS) return;
                }
            }

            // only protect if gameplayer and aggroamout > 0
            if (living is GamePlayer && aggroamount > 0)
            {
                GamePlayer player = (GamePlayer)living;

                if (player.Group != null || player.MemberIsInBG(player) && GS.ServerProperties.Properties.BATTLEGROUP_LOOT)
                { // player is in group, add whole group to aggro list

                    BattleGroup battleGroup = player.TempProperties.getProperty<BattleGroup>(BattleGroup.BATTLEGROUP_PROPERTY, null);

                    if (battleGroup != null && GS.ServerProperties.Properties.BATTLEGROUP_LOOT)
                    {
                        lock ((m_aggroTable as ICollection).SyncRoot)
                        {
                            //BattleGroup
                            foreach (GamePlayer players in battleGroup.Members.Keys)
                            {
                                if (!m_aggroTable.ContainsKey(players))
                                {
                                    m_aggroTable[players] = 1L;   // add the missing group member on aggro table
                                }
                            }
                        }
                    }
                    else
                        lock ((m_aggroTable as ICollection).SyncRoot)
                        {
                            foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
                            {
                                if (!m_aggroTable.ContainsKey(p))
                                {
                                    m_aggroTable[p] = 1L;   // add the missing group member on aggro table
                                }
                            }
                        }
                }

                //ProtectEffect protect = (ProtectEffect) player.EffectList.GetOfType(typeof(ProtectEffect));
                foreach (ProtectEffect protect in player.EffectList.GetAllOfType<ProtectEffect>())
                {
                    // if no aggro left => break
                    if (aggroamount <= 0) break;

                    //if (protect==null) continue;
                    if (protect.ProtectTarget != living) continue;
                    if (protect.ProtectSource.IsStunned) continue;
                    if (protect.ProtectSource.IsMezzed) continue;
                    if (protect.ProtectSource.IsSitting) continue;
                    if (protect.ProtectSource.ObjectState != GameObject.eObjectState.Active) continue;
                    if (!protect.ProtectSource.IsAlive) continue;
                    if (!protect.ProtectSource.InCombat) continue;

                    if (!living.IsWithinRadius(protect.ProtectSource, ProtectAbilityHandler.PROTECT_DISTANCE))
                        continue;
                    // P I: prevents 10% of aggro amount
                    // P II: prevents 20% of aggro amount
                    // P III: prevents 30% of aggro amount
                    // guessed percentages, should never be higher than or equal to 50%
                    int abilityLevel = protect.ProtectSource.GetAbilityLevel(Abilities.Protect);
                    int protectAmount = (int)((abilityLevel * 0.10) * aggroamount);

                    if (protectAmount > 0)
                    {
                        aggroamount -= protectAmount;
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AI.Brain.StandardMobBrain.YouProtDist", protect.ProtectSource.Name, Body.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        //protect.ProtectSource.Out.SendMessage(LanguageMgr.GetTranslation(protect.ProtectSource.Client, "AI.Brain.DracoBrain.YouProtDist", player.GetName(0, false), Body.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        //player.Out.SendMessage("You are protected by " + protect.ProtectSource.GetName(0, false) + " from " + Body.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        lock ((m_aggroTable as ICollection).SyncRoot)
                        {
                            if (m_aggroTable.ContainsKey(protect.ProtectSource))
                                m_aggroTable[protect.ProtectSource] += protectAmount;
                            else
                                m_aggroTable[protect.ProtectSource] = protectAmount;
                        }
                    }
                }
            }

            lock ((m_aggroTable as ICollection).SyncRoot)
            {
                if (m_aggroTable.ContainsKey(living))
                {
                    long amount = m_aggroTable[living];
                    amount += aggroamount;

                    // can't be removed this way, set to minimum
                    if (amount <= 0)
                        amount = 1L;

                    m_aggroTable[living] = amount;
                }
                else
                {
                    if (aggroamount > 0)
                    {
                        m_aggroTable[living] = aggroamount;
                    }
                    else
                    {
                        m_aggroTable[living] = 1L;
                    }

                }
            }
        }

        /// <summary>
        /// Get current amount of aggro on aggrotable
        /// </summary>
        /// <param name="living"></param>
        /// <returns></returns>
        public override long GetAggroAmountForLiving(GameLiving living)
        {
            lock ((m_aggroTable as ICollection).SyncRoot)
            {
                if (m_aggroTable.ContainsKey(living))
                {
                    return m_aggroTable[living];
                }
                return 0;
            }
        }

        /// <summary>
        /// Remove one living from aggro list
        /// </summary>
        /// <param name="living"></param>
        public override void RemoveFromAggroList(GameLiving living)
        {
            lock ((m_aggroTable as ICollection).SyncRoot)
            {
                m_aggroTable.Remove(living);
            }
        }

        /// <summary>
        /// Remove all livings from the aggrolist
        /// </summary>
        public override void ClearAggroList()
        {
            lock ((m_aggroTable as ICollection).SyncRoot)
            {
                m_aggroTable.Clear();
                Body.TempProperties.removeProperty(Body.Attackers);
            }
        }

        /// <summary>
        /// Makes a copy of current aggro list
        /// </summary>
        /// <returns></returns>
        public override Dictionary<GameLiving, long> CloneAggroList()
        {
            lock ((m_aggroTable as ICollection).SyncRoot)
            {
                return new Dictionary<GameLiving, long>(m_aggroTable);
            }
        }

        /// <summary>
        /// Selects and attacks the next target or does nothing
        /// </summary>
        protected override void AttackMostWanted()
        {
            if (!IsActive)
                return;

           

            Body.TargetObject = CalculateNextAttackTarget();

            if (Body.TargetObject != null)
            {
                if (!CheckSpells(eCheckSpellType.Offensive))
                {
                    Body.StartAttack(Body.TargetObject);
                }
            }
        }




        /// <summary>
        /// Returns the best target to attack
        /// </summary>
        /// <returns>the best target</returns>
        protected override GameLiving CalculateNextAttackTarget()
        {
            GameLiving maxAggroObject = null;
            lock ((m_aggroTable as ICollection).SyncRoot)
            {
                double maxAggro = 0;
                Dictionary<GameLiving, long>.Enumerator aggros = m_aggroTable.GetEnumerator();
                List<GameLiving> removable = new List<GameLiving>();
                while (aggros.MoveNext())
                {
                    GameLiving living = aggros.Current.Key;

                    // check to make sure this target is still valid
                    if (living.IsAlive == false ||
                        living.ObjectState != GameObject.eObjectState.Active ||
                        living.IsStealthed ||
                        Body.GetDistanceTo(living, 0) > MAX_AGGRO_LIST_DISTANCE ||
                        GameServer.ServerRules.IsAllowedToAttack(Body, living, true) == false)
                    {
                        removable.Add(living);
                        continue;
                    }

                    // Don't bother about necro shade, can't attack it anyway.
                    if (living.EffectList.GetOfType<NecromancerShadeEffect>() != null)
                        continue;

                    long amount = aggros.Current.Value;

                    if (living.IsAlive
                        && amount > maxAggro
                        && living.CurrentRegion == Body.CurrentRegion
                        && living.ObjectState == GameObject.eObjectState.Active)
                    {
                        int distance = Body.GetDistanceTo(living);
                        int maxAggroDistance = (this is IControlledBrain) ? MAX_PET_AGGRO_DISTANCE : MAX_AGGRO_DISTANCE;

                        if (distance <= maxAggroDistance)
                        {
                            double aggro = amount * Math.Min(500.0 / distance, 1);
                            if (aggro > maxAggro)
                            {
                                maxAggroObject = living;
                                maxAggro = aggro;
                            }
                        }
                    }
                }

                foreach (GameLiving l in removable)
                {
                    RemoveFromAggroList(l);
                    Body.RemoveAttacker(l);
                }
            }

            if (maxAggroObject == null)
            {
                m_aggroTable.Clear();
            }

            return maxAggroObject;
        }

        /// <summary>
        /// calculate the aggro of this npc against another living
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public override int CalculateAggroLevelToTarget(GameLiving target)
        {
            if (GameServer.ServerRules.IsAllowedToAttack(Body, target, true) == false)
                return 0;

            // related to the pet owner if applicable
            if (target is GameNPC)
            {
                IControlledBrain brain = ((GameNPC)target).Brain as IControlledBrain;
                if (brain != null)
                {
                    GameLiving thisLiving = (((GameNPC)target).Brain as IControlledBrain).GetLivingOwner();
                    if (thisLiving != null)
                    {
                        if (thisLiving.IsObjectGreyCon(Body))
                            return 0;
                    }
                }
            }

            if (target.IsObjectGreyCon(Body)) return 0;	// only attack if green+ to target

            if (Body.Faction != null && target is GamePlayer)
            {
                GamePlayer player = (GamePlayer)target;
                AggroLevel = Body.Faction.GetAggroToFaction(player);
            }
            else if (Body.Faction != null && target is GameNPC)
            {
                GameNPC npc = (GameNPC)target;
                if (npc.Faction != null)
                {
                    if (npc.Brain is IControlledBrain && (npc.Brain as IControlledBrain).GetPlayerOwner() != null)
                    {
                        GamePlayer factionChecker = (npc.Brain as IControlledBrain).GetPlayerOwner();
                        AggroLevel = Body.Faction.GetAggroToFaction(factionChecker);
                    }
                    else
                    {
                        if (Body.Faction.EnemyFactions.Contains(npc.Faction))
                            AggroLevel = 100;
                    }
                }
            }

            //we put this here to prevent aggroing non-factions npcs
            if (target.Realm == eRealm.None && target is GameNPC)
                return 0;

            if (AggroLevel >= 100) return 100;

            return AggroLevel;
        }
		/// <summary>
		/// Called whenever the dragon's body sends something to its brain.
		/// </summary>
		/// <param name="e">The event that occured.</param>
		/// <param name="sender">The source of the event.</param>
		/// <param name="args">The event details.</param>
		public override void Notify(DOL.Events.DOLEvent e, object sender, EventArgs args)
		{
			base.Notify(e, sender, args);
			if (sender == Body)
			{
				GameDragon dragon = sender as GameDragon;
				if (e == GameObjectEvent.TakeDamage)
				{
					if (CheckHealth()) return;

					// Someone hit the dragon. If the attacker is in melee range, there
					// is a chance the dragon will cast a debuff specific to melee
					// classes on him, if not, well, dragon will try to get its Glare off...

					GameObject source = (args as TakeDamageEventArgs).DamageSource;
					if (source != null)
					{
                        if (dragon.IsWithinRadius(source, dragon.AttackRange))
                        {
                            if (Util.Chance(80))
                            {
                                dragon.CheckMeleeDebuff(source as GamePlayer);
                            }
                            else { }
                        }
                        else
                        {
                            if (Util.Chance(80))
                            {
                                dragon.CheckGlare(source as GamePlayer);
                            }
                            else { }
                        }
					}
					else
					{
						log.Error("Dragon takes damage from null source. args = " + (args == null ? "null" : args.ToString()));
					}
				}
				else if (e == GameLivingEvent.EnemyHealed)
				{
					// Someone healed an enemy. If the healer is in melee range, there
					// is a chance the dragon will cast a debuff specific to ranged
					// classes on him, if not, there's still Glare...

					GameObject source = (args as EnemyHealedEventArgs).HealSource;

					if (source != null)
					{
						if (dragon.IsWithinRadius(source, dragon.AttackRange))
						{
							dragon.CheckRangedDebuff(source as GamePlayer);
						}
						else
						{
							dragon.CheckGlare(source as GamePlayer);
						}
					}
					else
					{
						log.Error("Dragon heal source null. args = " + (args == null ? "null" : args.ToString()));
					}
				}
			}
			else if (e == GameNPCEvent.ArriveAtTarget && sender != null)
			{
				// Message from another NPC, such as a retriever,
				// for example.

				log.Info(String.Format("DragonBrain.Notify: ArriveAtTarget({0})", (sender as GameObject).Name));
				(Body as GameDragon).OnRetrieverArrived(sender as GameNPC);
			}
        }
        /// <summary>
        /// Lost follow target event
        /// </summary>
        /// <param name="target"></param>
        protected override void OnFollowLostTarget(GameObject target)
        {
            AttackMostWanted();
            if (!Body.AttackState)
                Body.WalkToSpawn();
        }

        /// <summary>
        /// Attacked by enemy event
        /// </summary>
        /// <param name="ad"></param>
        protected override void OnAttackedByEnemy(AttackData ad)
        {
            if (!Body.AttackState
                && Body.IsAlive
                && Body.ObjectState == GameObject.eObjectState.Active)
            {
                if (AggroTable.ContainsKey(ad.Attacker) == false)
                {
                    AddToAggroList(ad.Attacker, (ad.Attacker.Level + 1) << 1);
                }

                Body.StartAttack(ad.Attacker);
                BringFriends(ad);
            }
        }
        #endregion
        #region Bring a Friend

        
        /// <summary>
        /// BAF range for adds close to the pulled mob.
        /// </summary>
        public override ushort BAFCloseRange
        {
            get { return (ushort)((AggroRange * 2) / 5); }
        }

        /// <summary>
        /// BAF range for group adds in dungeons.
        /// </summary>
        public override ushort BAFReinforcementsRange
        {
            get { return m_BAFReinforcementsRange; }
            set { m_BAFReinforcementsRange = (value > 0) ? (ushort)value : (ushort)0; }
        }

        /// <summary>
        /// Range for potential targets around the puller.
        /// </summary>
        public override ushort BAFTargetPlayerRange
        {
            get { return m_BAFTargetPlayerRange; }
            set { m_BAFTargetPlayerRange = (value > 0) ? (ushort)value : (ushort)0; }
        }

        /// <summary>
        /// Bring friends when this living is attacked. There are 2
        /// different mechanisms for BAF:
        /// 1) Any mobs of the same faction within a certain (short) range
        ///    around the pulled mob will add on the puller, anywhere.
        /// 2) In dungeons, group size is taken into account as well, the
        ///    bigger the group, the more adds will come, even if they are
        ///    not close to the pulled mob.
        /// </summary>
        /// <param name="attackData">The data associated with the puller's attack.</param>
        protected override void BringFriends(AttackData attackData)
        {
            // Only add on players.

            GameLiving attacker = attackData.Attacker;
            if (attacker is GamePlayer)
            {
                BringCloseFriends(attackData);
                if (attacker.CurrentRegion.IsDungeon)
                    BringReinforcements(attackData);
            }
        }

        /// <summary>
        /// Get mobs close to the pulled mob to add on the puller and his
        /// group as well.
        /// </summary>
        /// <param name="attackData">The data associated with the puller's attack.</param>
        protected override void BringCloseFriends(AttackData attackData)
        {
            // Have every friend within close range add on the attacker's
            // group.
            DragonBrain brain = null;

            GamePlayer attacker = (GamePlayer)attackData.Attacker;

            foreach (GameNPC npc in Body.GetNPCsInRadius(BAFCloseRange))
            {
                if (npc.Brain is DragonBrain)
                {
                    brain = (DragonBrain)npc.Brain;
                }
                if (brain != null && npc.IsFriend(Body) && npc.IsAvailable && npc.IsAggressive)
                {
                    brain.AddToAggroList(PickTarget(attacker), 1);
                    brain.AttackMostWanted();

                }
            }
        }

        /// <summary>
        /// Get mobs to add on the puller's group, their numbers depend on the
        /// group's size.
        /// </summary>
        /// <param name="attackData">The data associated with the puller's attack.</param>
        protected override void BringReinforcements(AttackData attackData)
        {
            // Determine how many friends to bring, as a rule of thumb, allow for
            // max 2 players dealing with 1 mob. Only players from the group the
            // original attacker is in will be taken into consideration.
            // Example: A group of 3 or 4 players will get 1 add, a group of 7 or 8
            // players will get 3 adds.

            GamePlayer attacker = (GamePlayer)attackData.Attacker;
            Group attackerGroup = attacker.Group;
            int numAttackers = (attackerGroup == null) ? 1 : attackerGroup.MemberCount;
            int maxAdds = (numAttackers + 1) / 2 - 1;
            if (maxAdds > 0)
            {
                // Bring friends, try mobs in the neighbourhood first. If there
                // aren't any, try getting some from farther away.

                int numAdds = 0;
                ushort range = 250;

                while (numAdds < maxAdds && range <= BAFReinforcementsRange)
                {
                    foreach (GameNPC npc in Body.GetNPCsInRadius(range))
                    {
                        if (numAdds >= maxAdds) break;

                        // If it's a friend, have it attack a random target in the
                        // attacker's group.

                        if (npc.IsFriend(Body) && npc.IsAggressive && npc.IsAvailable)
                        {
                            DragonBrain brain = (DragonBrain)npc.Brain;
                            brain.AddToAggroList(PickTarget(attacker), 1);
                            brain.AttackMostWanted();
                            ++numAdds;
                        }
                    }

                    // Increase the range for finding friends to join the fight.

                    range *= 2;
                }
            }
        }

        

        #endregion
                #region Spells

      

        /// <summary>
        /// Checks if any spells need casting
        /// </summary>
        /// <param name="type">Which type should we go through and check for?</param>
        /// <returns></returns>
        public override bool CheckSpells(eCheckSpellType type)
        {
            if (Body.IsCasting)
                return true;

            bool casted = false;

            if (Body != null && Body.Spells != null && Body.Spells.Count > 0)
            {
                ArrayList spell_rec = new ArrayList();
                Spell tire = null;
                bool needpet = false;
                bool needheal = false;

                if (type == eCheckSpellType.Defensive)
                {
                    foreach (Spell spell in Body.Spells)
                    {
                        if (Body.GetSkillDisabledDuration(spell) > 0) continue;
                        if (spell.Target.ToLower() == "enemy" || spell.Target.ToLower() == "area" || spell.Target.ToLower() == "cone") continue;
                        // If we have no pets
                        if (Body.ControlledBrain == null)
                        {
                            if (spell.SpellType.ToLower() == "pet") continue;
                            if (spell.SpellType.ToLower().Contains("summon"))
                            {
                                spell_rec.Add(spell);
                                needpet = true;
                            }
                        }
                        if (Body.ControlledBrain != null && Body.ControlledBrain.Body != null)
                        {
                            if (Util.Chance(30) && Body.ControlledBrain != null && spell.SpellType.ToLower() == "heal" &&
                                Body.GetDistanceTo(Body.ControlledBrain.Body) <= spell.Range &&
                                Body.ControlledBrain.Body.HealthPercent < 60 && spell.Target.ToLower() != "self")
                            {
                                spell_rec.Add(spell);
                                needheal = true;
                            }
                            if (LivingHasEffect(Body.ControlledBrain.Body, spell) && (spell.Target.ToLower() != "self")) continue;
                        }
                        if (!needpet && !needheal)
                            spell_rec.Add(spell);
                    }
                    if (spell_rec.Count > 0)
                    {
                        tire = (Spell)spell_rec[Util.Random((spell_rec.Count - 1))];
                        if (!Body.IsReturningToSpawnPoint)
                        {
                            if (tire.Uninterruptible && CheckDefensiveSpells(tire))
                                casted = true;
                            else
                                if (!Body.IsBeingInterrupted && CheckDefensiveSpells(tire))
                                    casted = true;
                        }
                    }
                }
                else if (type == eCheckSpellType.Offensive)
                {
                    foreach (Spell spell in Body.Spells)
                    {

                        if (Body.GetSkillDisabledDuration(spell) == 0)
                        {
                            if (spell.CastTime > 0)
                            {
                                if (spell.Target.ToLower() == "enemy" || spell.Target.ToLower() == "area" || spell.Target.ToLower() == "cone")
                                    spell_rec.Add(spell);
                            }
                        }
                    }
                    if (spell_rec.Count > 0)
                    {
                        tire = (Spell)spell_rec[Util.Random((spell_rec.Count - 1))];


                        if (tire.Uninterruptible && CheckOffensiveSpells(tire))
                            casted = true;
                        else
                            if (!Body.IsBeingInterrupted && CheckOffensiveSpells(tire))
                                casted = true;
                    }
                }

                return casted;
            }
            return casted;
        }

        /// <summary>
        /// Checks defensive spells.  Handles buffs, heals, etc.
        /// </summary>
        protected override bool CheckDefensiveSpells(Spell spell)
        {
            if (spell == null) return false;
            if (Body.GetSkillDisabledDuration(spell) > 0) return false;
            GameObject lastTarget = Body.TargetObject;
            Body.TargetObject = null;
            switch (spell.SpellType)
            {
        #region Buffs
                case "StrengthConstitutionBuff":
                case "DexterityQuicknessBuff":
                case "StrengthBuff":
                case "DexterityBuff":
                case "ConstitutionBuff":
                case "ArmorFactorBuff":
                case "ArmorAbsorptionBuff":
                case "CombatSpeedBuff":
                case "MeleeDamageBuff":
                case "AcuityBuff":
                case "HealthRegenBuff":
                case "DamageAdd":
                case "DamageShield":
                case "BodyResistBuff":
                case "ColdResistBuff":
                case "EnergyResistBuff":
                case "HeatResistBuff":
                case "MatterResistBuff":
                case "SpiritResistBuff":
                case "BodySpiritEnergyBuff":
                case "HeatColdMatterBuff":
                case "CrushSlashThrustBuff":
                case "AllMagicResistsBuff":
                case "AllMeleeResistsBuff":
                case "AllResistsBuff":
                case "OffensiveProc":
                case "DefensiveProc":
                case "Bladeturn":
                case "ToHitBuff":
                    {
                        // Buff self, if not in melee, but not each and every mob
                        // at the same time, because it looks silly.
                        if (!LivingHasEffect(Body, spell) && !Body.AttackState && Util.Chance(40) && spell.Target.ToLower() != "pet")
                        {
                            Body.TargetObject = Body;
                            break;
                        }
                        if (Body.ControlledBrain != null && Body.ControlledBrain.Body != null && Util.Chance(40) && Body.GetDistanceTo(Body.ControlledBrain.Body) <= spell.Range && !LivingHasEffect(Body.ControlledBrain.Body, spell) && spell.Target.ToLower() != "self")
                        {
                            Body.TargetObject = Body.ControlledBrain.Body;
                            break;
                        }
                        break;
                    }
                #endregion Buffs
        
         #region Disease Cure/Poison Cure/Summon
                case "CureDisease":
                    if (Body.IsDiseased)
                    {
                        Body.TargetObject = Body;
                        break;
                    }
                    if (Body.ControlledBrain != null && Body.ControlledBrain.Body != null && Body.ControlledBrain.Body.IsDiseased
&& Body.GetDistanceTo(Body.ControlledBrain.Body) <= spell.Range && spell.Target.ToLower() != "self")
                    {
                        Body.TargetObject = Body.ControlledBrain.Body;
                        break;
                    }
                    break;
                case "CurePoison":
                    if (LivingIsPoisoned(Body))
                    {
                        Body.TargetObject = Body;
                        break;
                    }
                    if (Body.ControlledBrain != null && Body.ControlledBrain.Body != null && LivingIsPoisoned(Body.ControlledBrain.Body)
&& Body.GetDistanceTo(Body.ControlledBrain.Body) <= spell.Range && spell.Target.ToLower() != "self")
                    {
                        Body.TargetObject = Body.ControlledBrain.Body;
                        break;
                    }
                    break;
                case "Summon":
                    Body.TargetObject = Body;
                    break;
                case "SummonMinion":
                    //If the list is null, lets make sure it gets initialized!
                    if (Body.ControlledNpcList == null)
                        Body.InitControlledBrainArray(2);
                    else
                    {
                        //Let's check to see if the list is full - if it is, we can't cast another minion.
                        //If it isn't, let them cast.
                        IControlledBrain[] icb = Body.ControlledNpcList;
                        int numberofpets = 0;
                        for (int i = 0; i < icb.Length; i++)
                        {
                            if (icb[i] != null)
                                numberofpets++;
                        }
                        if (numberofpets >= icb.Length)
                            break;
                    }
                    Body.TargetObject = Body;
                    break;
                #endregion Disease Cure/Poison Cure/Summon
         #region Heals
                case "Heal":
                    // Chance to heal self when dropping below 30%, do NOT spam it.

                    if (Body.HealthPercent < 30 && Util.Chance(10) && spell.Target.ToLower() != "pet")
                    {
                        Body.TargetObject = Body;
                        break;
                    }

                    if (Body.ControlledBrain != null && Body.ControlledBrain.Body != null
                        && Body.GetDistanceTo(Body.ControlledBrain.Body) <= spell.Range && Body.ControlledBrain.Body.HealthPercent < 60 && spell.Target.ToLower() != "self")
                    {
                        Body.TargetObject = Body.ControlledBrain.Body;
                        break;
                    }
                    break;
                #endregion

                case "SummonCommander":
                case "SummonDruidPet":
                case "SummonHunterPet":
                case "SummonNecroPet":
                case "SummonUnderhill":
                case "SummonSimulacrum":
                case "SummonSpiritFighter":
                    //case "SummonTheurgistPet":
                    if (Body.ControlledBrain != null)
                        break;
                    Body.TargetObject = Body;
                    break;
            }
            if (Body.TargetObject != null)
            {
                if (Body.IsMoving && spell.CastTime > 0)
                    Body.StopFollowing();

                if (Body.TargetObject != Body && spell.CastTime > 0)
                    Body.TurnTo(Body.TargetObject);

                Body.CastSpell(spell, m_mobSpellLine, true);

                Body.TargetObject = lastTarget;
                return true;
            }

            Body.TargetObject = lastTarget;

            return false;
        }

        /// <summary>
        /// Checks offensive spells.  Handles dds, debuffs, etc.
        /// </summary>
        protected override bool CheckOffensiveSpells(Spell spell)
        {
            if (spell.Target.ToLower() != "enemy" && spell.Target.ToLower() != "area" && spell.Target.ToLower() != "cone")
                return false;

            if (Body.TargetObject != null)
            {
                if (Body.IsMoving && spell.CastTime > 0)
                    Body.StopFollowing();

                if (Body.TargetObject != Body && spell.CastTime > 0)
                    Body.TurnTo(Body.TargetObject);

                Body.CastSpell(spell, m_mobSpellLine, true);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks Instant Spells.  Handles Taunts, shouts, stuns, etc.
        /// </summary>
        protected override bool CheckInstantSpells(Spell spell)
        {
            GameObject lastTarget = Body.TargetObject;
            Body.TargetObject = null;

            switch (spell.SpellType)
            {
         #region Enemy Spells
                case "DirectDamage":
                case "Lifedrain":
                case "DexterityDebuff":
                case "StrengthConstitutionDebuff":
                case "CombatSpeedDebuff":
                case "DamageOverTime":
                case "MeleeDamageDebuff":
                case "AllStatsPercentDebuff":
                case "CrushSlashThrustDebuff":
                case "EffectivenessDebuff":
                case "SpeedDecrease":  //neu fehlte
                case "Disease":
                case "Stun":
                case "Mez":
                case "Taunt":
                    if (!LivingHasEffect(lastTarget as GameLiving, spell))
                    {
                        Body.TargetObject = lastTarget;
                    }
                    break;
                #endregion
                #region Combat Spells
                case "CombatHeal":
                case "DamageAdd":
                case "ArmorFactorBuff":
                case "DexterityQuicknessBuff":
                case "EnduranceRegenBuff":
                case "CombatSpeedBuff":
                case "AblativeArmor":
                case "Bladeturn":
                case "OffensiveProc":
                    if (!LivingHasEffect(Body, spell))
                    {
                        Body.TargetObject = Body;
                    }
                    break;
                #endregion
            }

            if (Body.TargetObject != null)
            {
                Body.CastSpell(spell, m_mobSpellLine, true);
                Body.TargetObject = lastTarget;
                return true;
            }

            Body.TargetObject = lastTarget;
            return false;
        }
        

                #endregion
        #region Random Walk
        public override bool CanRandomWalk
        {
            get
            {
                if (!DOL.GS.ServerProperties.Properties.ALLOW_ROAM)
                    return false;
                if (Body.RoamingRange == 0)
                    return false;
                return true;
            }
        }

        public override IPoint3D CalcRandomWalkTarget()
        {
            int maxRoamingRadius = Body.CurrentRegion.IsDungeon ? 5 : 500;
            int minRoamingRadius = Body.CurrentRegion.IsDungeon ? 1 : 100;

            if (Body.RoamingRange > 0)
            {
                maxRoamingRadius = Body.RoamingRange;

                if (minRoamingRadius >= maxRoamingRadius)
                    minRoamingRadius = maxRoamingRadius / 3;
            }

            int roamingRadius = Util.Random(minRoamingRadius, maxRoamingRadius);

            double angle = Util.Random(0, 360) / (2 * Math.PI);
            double targetX = Body.SpawnPoint.X + Util.Random(-roamingRadius, roamingRadius);
            double targetY = Body.SpawnPoint.Y + Util.Random(-roamingRadius, roamingRadius);

            double targetFlyX = Body.SpawnPoint.X + Util.Random(-roamingRadius, roamingRadius);
            double targetFlyY = Body.SpawnPoint.Y + Util.Random(-roamingRadius, roamingRadius);

            double targetFlyZ = Body.SpawnPoint.Z + Util.RandomDouble(-(double)roamingRadius * GS.ServerProperties.Properties.Roam_ZSCALEFACTOR * 0.1, (double)roamingRadius * GS.ServerProperties.Properties.Roam_ZSCALEFACTOR * 0.1); ;




            if (Body.Flags == GameNPC.eFlags.FLYING || Body.Flags == GameNPC.eFlags.SWIMMING && Body.NPCIsFish)
            {
                return new Point3D((int)targetFlyX, (int)targetFlyY, (int)targetFlyZ);
            }

            else

                return new Point3D((int)targetX, (int)targetY, Body.SpawnPoint.Z);

       
    }

        #endregion
        #region Health Check

        private int m_stage = 10;

        /// <summary>
        /// This keeps track of the stage the encounter is in, so players
        /// don't have to go through all the PBAoE etc. again, just because
        /// the dragon regains a small amount of health. Starts at 10 (full
        /// health) and drops to 0.
        /// </summary>
        public int Stage
        {
            get { return m_stage; }
            set { if (value >= 0 && value <= 10) m_stage = value; }
        }

        /// <summary>
        /// Actions to be taken into consideration when health drops.
        /// </summary>
        /// <returns>Whether any action was taken.</returns>
        private bool CheckHealth()
        {
            GameDragon dragon = Body as GameDragon;
            if (dragon == null) return false;

            int healthOld = dragon.HealthPercentOld / 10;
            int healthNow = dragon.HealthPercent / 10;

            // Stun when health drops below 30%.

            if (healthNow < 3)
            {
                if (dragon.CheckStun(healthNow < healthOld))
                    return true;
            }

            if (healthNow < healthOld && Stage > healthNow)
            {
                Stage = healthNow;

                // Breathe at 89%/79%/69%/49% and 9%.

                switch (healthNow)
                {
                    case 8:
                    case 7:
                    case 6:
                    case 4:
                    case 0: if (dragon.CheckBreath())
                            return true;
                        break;
                }

                // Spawn adds at 49% and 19% (hunch).

                switch (healthNow)
                {
                    case 5:
                    case 3: if (dragon.CheckAddSpawns())
                            return true;
                        break;
                }
            }
            return false;
        }

        #endregion
		#region Glare

		/// <summary>
		/// Try to find a potential target for Glare.
		/// </summary>
		/// <returns>Whether or not a target was picked.</returns>
		private bool PickGlareTarget()
		{
			GameDragon dragon = Body as GameDragon;
			if (dragon == null) return false;

            ArrayList inRangeLiving = new ArrayList();

            lock ((m_aggroTable as ICollection).SyncRoot)
            {
				Dictionary<GameLiving, long>.Enumerator enumerator = m_aggroTable.GetEnumerator();
                while (enumerator.MoveNext())
				{
                    GameLiving living = enumerator.Current.Key;
					if (living != null && 
						living.IsAlive &&
						living.EffectList.GetOfType<NecromancerShadeEffect>() == null && 
						!dragon.IsWithinRadius(living, dragon.AttackRange))
					{
						inRangeLiving.Add(living);
					}
				}
            }

			if (inRangeLiving.Count > 0)
			{
				return dragon.CheckGlare((GameLiving)(inRangeLiving[Util.Random(1, inRangeLiving.Count) - 1]));
			}

			return false;
		}

		#endregion

        #region Throw

		/// <summary>
		/// Pick a target to hurl into the air.
		/// </summary>
		/// <returns>Whether or not a target was picked.</returns>
		private bool PickThrowTarget()
		{
			GameDragon dragon = Body as GameDragon;
			if (dragon == null) return false;

            ArrayList inRangeLiving = new ArrayList();
			foreach (GamePlayer player in dragon.GetPlayersInRadius((ushort)dragon.AttackRange))
			{
				if (player.IsAlive && player.EffectList.GetOfType<NecromancerShadeEffect>() == null)
				{
					inRangeLiving.Add(player);
				}
			}

			foreach (GameNPC npc in dragon.GetNPCsInRadius((ushort)dragon.AttackRange))
			{
				if (npc.IsAlive && npc.Brain != null && npc.Brain is IControlledBrain)
				{
					inRangeLiving.Add(npc);
				}
			}

			if (inRangeLiving.Count > 0)
			{
				return dragon.CheckThrow((GameLiving)(inRangeLiving[Util.Random(1, inRangeLiving.Count) - 1]));
			}

			return false;
		}

        #endregion
    }
}
