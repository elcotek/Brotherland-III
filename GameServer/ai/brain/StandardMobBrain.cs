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

using DOL.ai.brain;
using DOL.Events;
using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.Keeps;
using DOL.GS.Movement;
using DOL.GS.PacketHandler;
using DOL.GS.ServerProperties;
using DOL.GS.SkillHandler;
using DOL.GS.Spells;
using DOL.Language;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;


namespace DOL.AI.Brain
{
    /// <summary>
    /// Standard brain for standard mobs
    /// </summary>
    public class StandardMobBrain : APlayerVicinityBrain, IOldAggressiveBrain
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public const int MAX_AGGRO_DISTANCE = 3600;
        public const int MAX_AGGRO_LIST_DISTANCE = 6000;
        public const int MAX_PET_AGGRO_DISTANCE = 1500; // Tolakram - Live test with caby pet - I was extremely close before auto aggro

        public const string LastTimeAggros = "LASTTIMEAGGROS";


        //Mob aggro increasing
        private GameSpellEffect ArcherSpeed = null;
        private GameSpellEffect SpeedEnhancement = null;
        private GameSpellEffect EnduranceRegen = null;
        private GameSpellEffect SpeedOfTheRealm = null;


        // Used for AmbientBehaviour "Seeing" - maintains a list of GamePlayer in range
        public List<GamePlayer> PlayersSeen = new List<GamePlayer>();

       
        /// <summary>
        /// currently applied effects
        /// </summary>
        protected readonly GameEffectList m_effects;
        /// <summary>
        /// The flag indicating that this effect has expired
        /// </summary>
        protected bool m_expired;

        /// <summary>
        /// True if effect is in immunity state
        /// </summary>
        public bool ImmunityState
        {
            get { return m_expired; }
            set { m_expired = value; }
        }



        /// <summary>
        /// gets a list of active effects
        /// </summary>
        /// <returns></returns>
        public GameEffectList EffectList
        {
            get { return m_effects; }
        }

        /// <summary>
        /// Spawn point
        /// </summary>
        protected Point3D m_spawnPoint;
        /// <summary>
        /// Gets or sets the spawnposition of this npc
        /// </summary>
        public virtual Point3D SpawnPoint
        {
            get { return m_spawnPoint; }
            set { m_spawnPoint = value; }
        }

        /// <summary>
        /// Constructs a new StandardMobBrain
        /// </summary>
        public StandardMobBrain()
            : base()
        {
            m_aggroLevel = 0;
            m_aggroMaxRange = 0;
        }

        /// <summary>
        /// Returns the string representation of the StandardMobBrain
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString() + ", m_aggroLevel=" + m_aggroLevel.ToString() + ", m_aggroMaxRange=" + m_aggroMaxRange.ToString();
        }

        public override bool Stop()
        {
            // tolakram - when the brain stops, due to either death or no players in the vicinity, clear the aggro list
            if (base.Stop())
            {
                ClearHealList();
                ClearAggroList();
                return true;
            }

            return false;
        }

        #region AI

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
            //by elcotek Unshade for Keep Guard shade!
            /// <summary>
            ///Night Day System///
            /// <summary>
            // nigthtspawned Mobs





            // If the NPC is tethered and has been pulled too far it will
            // de-aggro and return to its spawn point.
            if (Body.IsOutOfTetherRange && !Body.InCombat)
            {
                Body.WalkToSpawn();
                return;
            }

            if (Body.SwimmingIsFollowLivingOnGround(Body))
            {
                Body.StopAttack();
                Body.WalkToSpawn(220);
            }


            if (Body.AttackState == false && Body.InCombat == false && Body != null && Body.TargetObject == null && Body.CanRestoreHealth(Body))
            {
                Body.Health = Body.MaxHealth;
            }




            if (Body.InCombat == false && Body.Name != "ancient bound djinn")
            {
                Body.CustomQuestindicator();
                Body.OrkSpawn();
                Body.QuestNpcStop();

                if (Body is GameGuard)
                    GameGuard.NPCWalkToTarget(Body);

            }
            if (string.IsNullOrEmpty(Body.PathID) == false && !Body.AttackState)
            {
                Body.DragonFly();
            }


            //Dynamic Thinking of Brains based on target distance
            foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)AggroRange))
            {
                if (player == null)
                    continue;


                int aggrolevel = 0;

                //Remove player from Aggrolist if 0 faction Aggros
                if (Body.Faction != null && Body.InCombat == false)
                {
                    aggrolevel = Body.Faction.GetAggroToFaction(player);

                    //log.ErrorFormat("Faction aggro {0}, {1}", aggrolevel, Body.Name);

                    if (aggrolevel <= 0)
                    {

                        if (player.ControlledBrain != null)
                        {
                            GameNPC pet = player.ControlledBrain.Body;

                            if (pet != null && pet.IsAlive && pet.ObjectState == GameObject.eObjectState.Active)
                            {
                                if (m_aggroTable.ContainsKey(pet))
                                {

                                    RemoveFromAggroList(pet);
                                    //  log.ErrorFormat("pet remove from {1} aggrolist: {0}", pet.Name, Body.Name);
                                }
                            }
                        }


                        if (m_aggroTable.ContainsKey(player) && player.ObjectState == GameObject.eObjectState.Active)
                        {

                            RemoveFromAggroList(player);
                            // log.ErrorFormat("player remove from {1} aggrolist: {0}", player.Name, Body.Name);
                        }

                    }
                }
            }

            // Add Solo Healers to aggro List
            if (Body.InCombat && Body.IsAlive)
            {
                foreach (GamePlayer healer in Body.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    if (healer == null) continue;
                    if (healer.Group != null) continue;
                    {
                        if (healer.EffectList.GetOfType<NecromancerShadeEffect>() != null) continue;
                        if (healer.IsAlive == false) continue;
                        //By Elcotek Aggromanagemant for mobs vs Players and Groups an Spells
                        foreach (GameLiving p in HealerList.Keys)
                        {
                            if (healer != null && p != null && p.Name == healer.Name)
                            {
                                //Console.Out.WriteLine("Heiler Name = {0} ist gleich der in der liste!", healer.Name);

                                // Add the Healer to aggro list
                                lock ((m_aggroTable as ICollection).SyncRoot)
                                {

                                    if (!m_aggroTable.ContainsKey(healer))
                                    {
                                        m_aggroTable[healer] = 1L;   // add the missing group member on aggro table
                                    }

                                    // Console.Out.WriteLine("Heiler = {0} ist nun in der Aggoliste!", healer.Name);
                                }
                            }
                        }
                    }
                }
            }


            //Elcotek weglaufende djinns
            if (!Body.InCombat)
            {
                switch (Body.Name)
                {

                    case "wandering jinni":
                    case "Dawar":
                        {
                            foreach (GamePlayer player in Body.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                            {
                                int distance = 0;
                                int AngleToNPC = 0;
                                int extradistance = 0;
                                int bodyheading = 0;

                                if (player == null)
                                    continue;

                                if (player != null)
                                {
                                    extradistance = Util.Random(200, 500);
                                    distance = player.GetDistanceTo(Body);
                                    AngleToNPC = Convert.ToInt16(Body.GetAngle(player));
                                }

                                if (Body.IsWithinRadius(player, 1000))
                                {


                                    if (distance <= 500 && Body.IsObjectInFront(player, 90))


                                        bodyheading = Body.Heading + Convert.ToUInt16(Util.Random(45, 90));


                                    Body.Heading = Convert.ToUInt16(bodyheading);
                                    Body.StopMoving();
                                    Body.PathTo(player.X + distance + extradistance, player.Y - AngleToNPC, player.Z, Body.MaxSpeed);
                                    break;


                                }
                                else
                                {
                                    if (Body.IsWithinRadius(player, 2500))
                                    {
                                        Body.WalkToSpawn(Body.MaxSpeed);
                                    }
                                    break;

                                }
                            }
                        }

                        break;

                    default: break;
                }
            }

            //Instead - lets just let CheckSpells() make all the checks for us
            //Check for just positive spells
            CheckSpells(eCheckSpellType.Defensive);

            //Clear Body Gainer List after returing home
            /////////////////////////////////////////////////////////////////////////////
            if (HasAggro)
            {
                //set last aggro
                Body.TempProperties.setProperty(LastTimeAggros, Body);
            }

            int oldPercent = Body.HealthPercent;//elcotek: mob health back after returning home
            if (Body.TargetObject == null && Body.IsReturningHome == true && Body.InCombat == false && HasAggro == false && Body is GameSiegeWeapon == false && Body.ObjectState == GameObject.eObjectState.Active)
            {

                //log.Info("Aggroliste löschen1");

                if (Body.HealthPercent == 100)
                {
                    //Clear XP-Gainer List
                    Body.ClearGainerList(Body);
                    Body.IsReturningHome = false;
                    // log.Info("Aggroliste löschen");

                }
                if (Body.HealthPercent < 100)
                    Body.Health++;

                if (oldPercent != Body.HealthPercent)
                    Body.BroadcastUpdate();


                Body.TempProperties.removeProperty(LastTimeAggros);

            }
            //////////////////////////////////////////////////////////////////////////////

            //log.ErrorFormat("returning home? {0}", Body.IsReturningHome);

            if (Body.IsReturningHome == false)
            {
                //Check if NPC is back home or not
                Body.GetNPCBackHome(Body);

                if (!Body.AttackState && CalculateAggroRange(Body) > 0)
                {
                    var currentPlayersSeen = new List<GamePlayer>();
                    foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)CalculateAggroRange(Body)))
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

                Point3D point0 = null;
                int distanceToTarget = 0;
                point0 = new Point3D(Body.X, Body.Y, Body.Z);

                if (Body.TargetObject != null && point0 != null)
                {
                    distanceToTarget = Body.TargetObject.GetDistanceTo(point0);
                    // log.ErrorFormat("distanceToTarget: {0}", distanceToTarget);
                }
                // Note: Offensive spells are checked in GameNPC:SpellAction timer

                // check for returning to home if to far away 
                //Elcotek: If Living in body Radius NPC will Attack continue!
                if (Body.IsMezzed == false && Body.IsStunned == false && Body.MaxDistance != 0 && !Body.IsReturningHome && distanceToTarget > 500)
                {


                    int distance = Body.GetDistanceTo(Body.SpawnPoint);
                    int maxdistance = Body.MaxDistance > 0 ? Body.MaxDistance : -Body.MaxDistance * CalculateAggroRange(Body) / 100;
                    if (maxdistance > 0 && distance > maxdistance && distanceToTarget > 500)
                    {
                        Body.WalkToSpawn();
                        return;
                    }
                }



                //If this NPC can randomly walk around, we allow it to walk around
                //if (!Body.AttackState && CanRandomWalk && !Body.IsRoaming && Util.Chance(DOL.GS.ServerProperties.Properties.GAMENPC_RANDOMWALK_CHANCE)
                if (!Body.AttackState && Body.IsMezzed == false && !Body.IsRoaming && Body.IsStunned == false && HasAggro == false && CanRandomWalk && Util.Chance(DOL.GS.ServerProperties.Properties.GAMENPC_RANDOMWALK_CHANCE))
                {
                    IPoint3D target = CalcRandomWalkTarget();
                    if (target != null)
                    {
                        if (Util.IsNearDistance(target.X, target.Y, target.Z, Body.X, Body.Y, Body.Z, GameNPC.CONST_WALKTOTOLERANCE))
                        {
                            // Body.TurnTo(Body.GetHeading(target)); test raus
                        }
                        else
                        {
                            if ((Body.Brain as StandardMobBrain).HasAggro == false)
                            {
                                Body.PathTo(target.X, target.Y, target.Z, 50);
                            }
                            else

                                AttackMostWanted();
                        }

                        Body.FireAmbientSentence(GameNPC.eAmbientTrigger.roaming);
                    }
                }
                //If the npc can move, and the npc is not casting, not moving, and not attacking or in combat
                //else if (Body.MaxSpeedBase > 0 && Body.CurrentSpellHandler == null && !Body.IsMoving && !Body.AttackState && !Body.InCombat)
                else if (Body.IsMezzed == false && Body.IsStunned == false && (Body.MaxSpeedBase > 0 && Body.CurrentSpellHandler == null && HasAggro == false && !Body.InCombat && !Body.IsMovingOnPath && !Body.IsMoving && (Body.AttackState == false || Body.AttackState && Body.TargetObject != null && Body.TargetObject.IsObjectAlive == false)))
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
                if (Body.IsMezzed == false && Body.IsStunned == false && (Body.MaxSpeedBase > 0 && Body.CurrentSpellHandler == null && !Body.IsMoving
                    && !Body.AttackState && !Body.InCombat && !Body.IsMovingOnPath
                    && Body.PathID != null && Body.PathID != "" && Body.PathID != "NULL"))
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
                if (Body.Brain is ControlledNpcBrain == false && Body.IsMezzed == false && Body.IsStunned == false && !Body.IsMovingOnPath && !Body.InCombat && HasAggro == false && !Body.AttackState && !Body.IsCasting && !Body.IsMoving && Body.IsWithinRadius(Body.SpawnPoint, 500) == false && Body.ObjectState == GameObject.eObjectState.Active)
                {
                    Body.WalkToSpawn(); // Mobs do not walk back at 2x their speed..
                    Body.IsReturningHome = false; // We are returning to spawn but not the long walk home, so aggro still possible
                }

                //vorzeitige Aggro erfassung wenn speed Effekte aktiv sind
                foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)DOL.GS.ServerProperties.Properties.MOB_Basevalue_Aggro_Range_IncreaseBase))
                {
                    int factionAggro = 0;

                    if (player != null && player.IsAlive && Body.TargetInView)
                    {
                        if (Body.Faction != null)
                        {
                            factionAggro = Body.Faction.GetAggroToFaction(player);
                        }

                        if ((AggroLevel > 0 || factionAggro > 0) && player != null && player.IsMoving && player.InCombat == false && Body.InCombat == false && (player.IsSprinting || EnduranceRegen != null || SpeedEnhancement != null || ArcherSpeed != null || SpeedOfTheRealm != null))
                        {
                            if (Body.CurrentRegion.IsDungeon)
                            {
                                //log.Info("3");
                                Body.NpcIsSeeingAll = DOL.GS.ServerProperties.Properties.MOB_Basevalue_Aggro_Range_SeeAll_InsideOfDungeons; //seeall in dungeons ?
                            }
                            else
                            {
                                Body.NpcIsSeeingAll = DOL.GS.ServerProperties.Properties.MOB_Basevalue_Aggro_Range_SeeAll_OutSideOfDungeons;//seeall outside dungeons ?
                            }

                            CheckPlayerAggro();
                        }
                        // log.ErrorFormat("CheckNPCAggro factionAggro: {0}", factionAggro);

                        //normale Aggro erfassung
                        //If we have an aggrolevel above 0, we check for players and npcs in the area to attack
                        else if (Body.AttackState == false && (AggroLevel > 0 || factionAggro > 0))
                        {
                            CheckPlayerAggro();
                            CheckNPCAggro();
                        }
                    }
                }
                if (HasAggro)
                {

                    //Namen aus dem Chache lesen
                    if ((HasAggro && Body.IsMoving) || Body.TargetObject != null || Body.InCombat)
                    {
                        if (Properties.ALLOW_STEALTHER_GUARDS)
                        {
                            if (StealthNameCache.Contains(Body.Name) && this.Body.Flags != 0)
                            {
                                this.Body.Flags = 0;
                            }
                        }
                    }


                    Body.FireAmbientSentence(GameNPC.eAmbientTrigger.fighting);
                    AttackMostWanted();
                    return;
                }
                else
                {
                    if (Body.AttackState)
                    {
                        Body.StopAttack();
                        Body.TargetObject = null;

                    }
                    if (!Body.IsMoving && !Body.InCombat)
                    {
                        //by elcotek shade for minotaur shade and Keep Guards!
                        if ((Body.TargetObject == null && !Body.InCombat)
                           && Properties.ALLOW_STEALTHER_GUARDS)

                            if (StealthNameCache.Contains(Body.Name) && this.Body.Flags == 0)
                            {
                                this.Body.Flags ^= GameNPC.eFlags.STEALTH;
                            }
                    }
                }
            }
        }
        bool HasLos = false;
        bool AtackerIsMob = false;

        /// <summary>
        /// Check for aggro against close NPCs
        /// </summary>
        protected virtual void CheckNPCAggro()
        {
           // if (Body.AttackState) return;

            if (HasAggro) return;


            foreach (GameNPC npc in Body.GetNPCsInRadius((ushort)CalculateAggroRange(Body)))
            {
                if (!GameServer.ServerRules.IsAllowedToAttack(Body, npc, true)) continue;
                
               


                if (Body.SwimmingIsFollowLivingOnGround(Body))
                    return;

                if (m_aggroTable.ContainsKey(npc) && npc.IsAlive == false)
                {
                    m_aggroTable.Remove(npc);
                    continue; // remove death npc from aggrolist.
                }
                if (m_aggroTable.ContainsKey(npc))
                    continue; // add only new NPCs
                if (!npc.IsAlive || npc.ObjectState != GameObject.eObjectState.Active)
                    continue;
                if (npc is GameTaxi)
                    continue; //do not attack horses
                if (npc is GameTaxiBoat)
                    continue; //do not attack horses


                GameLiving realPetTarget = null;
                GameLiving realTarget = null;


                //Pets
                if (Body != null && ((GameNPC)Body).Brain is IControlledBrain)
                {
                    GameLiving owner = (((GameNPC)Body).Brain as IControlledBrain).GetLivingOwner();
                    if (owner != null && owner is GamePlayer)
                        realPetTarget = owner;
                }
                // only attack if green+ to target
                if (realPetTarget != null && realPetTarget.IsObjectGreyCon(npc))
                    continue;



                //Mobs 
                if (npc != null && ((GameNPC)npc).Brain is IControlledBrain)
                {
                    GameLiving owner = (((GameNPC)npc).Brain as IControlledBrain).GetLivingOwner();
                    if (owner != null)
                        realTarget = owner;
                }


                // only attack if green+ to target
                if (realTarget != null && realTarget.IsObjectGreyCon(Body))
                    continue;
                        
               

                if (CalculateAggroLevelToTarget(npc) > 0)
                {
                    if (npc.Brain is ControlledNpcBrain) // This is a pet or charmed creature, checkLOS
                        AddToAggroList(npc, 1, true);
                    else
                        // AddToAggroList(npc, 1);
                       CheckForAggro(Body, npc);

                    if (AtackerIsMob)
                    {
                        AddToAggroList(npc, 1);
                    }
                    else if (HasLos)
                    {
                       
                        AddToAggroList(npc, 1);
                    }
                    //else
                     // log.Error("kein Los!");

                }
            }
        }

        /// <summary>
        /// Check for aggro against players
        /// </summary>
        protected virtual void CheckPlayerAggro()
        {
            //Check if we are already attacking, return if yes
            // if (Body.AttackState) return;

            if (HasAggro) return;
            

            if (Body.SwimmingIsFollowLivingOnGround(Body))
                return;

            int newaggroRange = 0;
          
            //Vorzeitige erfassung und Aggro Range erhöhung wenn effekte aktive sind.
            foreach (GamePlayer players in Body.GetPlayersInRadius((ushort)DOL.GS.ServerProperties.Properties.MOB_Basevalue_Aggro_Range_CheckAggro))
            {
                if (players != null && players.IsAlive)


                    ArcherSpeed = SpellHandler.FindEffectOnTarget(players, "ArcherSpeedEnhancement");
                SpeedEnhancement = SpellHandler.FindEffectOnTarget(players, "SpeedEnhancement");
                EnduranceRegen = SpellHandler.FindEffectOnTarget(players, "EnduranceRegenBuff");
                SpeedOfTheRealm = SpellHandler.FindEffectOnTarget(players, "SpeedOfTheRealm");

                if (players.IsMoving && Body.InCombat == false && players.InCombat == false && (SpeedEnhancement != null || ArcherSpeed != null || SpeedOfTheRealm != null))
                {
                    if (Body.CurrentRegion.IsDungeon)
                    {
                        //log.Info("1");
                        Body.NpcIsSeeingAll = DOL.GS.ServerProperties.Properties.MOB_Basevalue_Aggro_Range_SeeAll_InsideOfDungeons;//seeall in dungeons ?
                    }
                    else
                    {
                        Body.NpcIsSeeingAll = DOL.GS.ServerProperties.Properties.MOB_Basevalue_Aggro_Range_SeeAll_OutSideOfDungeons;//seeall outside dungeons ?
                    }

                    newaggroRange = CalculateAggroRange(Body) * (ushort)DOL.GS.ServerProperties.Properties.MOB_Basevalue_Aggro_Range_Increase_SpeedBuff;//2
                }
                else if (players.IsMoving && Body.InCombat == false && players.InCombat == false && (players.IsSprinting && EnduranceRegen != null && SpeedEnhancement != null || ArcherSpeed != null || SpeedOfTheRealm != null))
                {
                    if (Body.CurrentRegion.IsDungeon)
                    {
                        //log.Info("2");
                        Body.NpcIsSeeingAll = DOL.GS.ServerProperties.Properties.MOB_Basevalue_Aggro_Range_SeeAll_InsideOfDungeons; //seeall in dungeons ?
                    }
                    else
                    {
                        Body.NpcIsSeeingAll = DOL.GS.ServerProperties.Properties.MOB_Basevalue_Aggro_Range_SeeAll_OutSideOfDungeons;//seeall outside dungeons ?
                    }


                    newaggroRange = CalculateAggroRange(Body) * (ushort)DOL.GS.ServerProperties.Properties.MOB_Basevalue_Aggro_Range_Increase_SpeedBuffEndurance;//3
                }
                else if (players.IsMoving && Body.InCombat == false && players.InCombat == false && players.IsSprinting)
                {
                    if (Body.CurrentRegion.IsDungeon)
                    {
                        //log.Info("3");
                        Body.NpcIsSeeingAll = DOL.GS.ServerProperties.Properties.MOB_Basevalue_Aggro_Range_SeeAll_InsideOfDungeons; //seeall in dungeons ?
                    }
                    else
                    {
                        Body.NpcIsSeeingAll = DOL.GS.ServerProperties.Properties.MOB_Basevalue_Aggro_Range_SeeAll_OutSideOfDungeons;//seeall outside dungeons ?
                    }

                    newaggroRange = CalculateAggroRange(Body) * (ushort)DOL.GS.ServerProperties.Properties.MOB_Basevalue_Aggro_Range_Increase_Sprint;//2
                }
                else
                {
                    //log.Info("4");
                    newaggroRange = CalculateAggroRange(Body);
                }
            }
            foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)newaggroRange))
            {

                if (!GameServer.ServerRules.IsAllowedToAttack(Body, player, true)) continue;



               


                if (m_aggroTable.ContainsKey(player) && player.IsAlive == false || Body is GuardStealther == false && player.IsStealthed)
                {
                    m_aggroTable.Remove(player);
                    continue; //remove death players from aggrolist.
                }

                if (player.EffectList.GetOfType<NecromancerShadeEffect>() != null || !Body.TargetInView)
                    continue;

                int aggrolevel = 0;

                //Fracion Aggros
                if (Body.Faction != null)
                {
                    aggrolevel = Body.Faction.GetAggroToFaction(player);
                                  
                    if (aggrolevel <= 0 )
                    {
                       return;
                    }
                }
                else if (AggroLevel <= 0)
                {
                    //log.ErrorFormat("no Faction {0}", Body.Name);
                    return;
                }

                if (m_aggroTable.ContainsKey(player))
                    continue; // add only new players
                if (!player.IsAlive || player.ObjectState != GameObject.eObjectState.Active || Body is GuardStealther == false && player.IsStealthed)
                    continue;
                if (player.Steed != null)
                    continue; //do not attack players on steed

                if (CalculateAggroLevelToTarget(player) > 0)
                {
                   
                    AddToAggroList(player, 1, true);
                }
            }
        }

        public virtual void CheckForAggro(GameLiving living, GameLiving TargetObject)
        {
            GamePlayer thisLiving = null;

            if (living is GameNPC && (living as GameNPC).Brain is IControlledBrain)
                thisLiving = ((living as GameNPC).Brain as IControlledBrain).GetPlayerOwner();
            else
                AtackerIsMob = true;

            if (thisLiving != null)
            {
                thisLiving.Out.SendCheckLOS(TargetObject, living, new CheckLOSResponse(AggroCheckLos));
            }

        }


        private void AggroCheckLos(GameLiving player, ushort response, ushort targetOID)
        {
            if ((response & 0x100) == 0x100)
                HasLos = true;

            else
                HasLos = false;
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
        public virtual bool CheckFormation(ref int x, ref int y, ref int z, ref short speed)
        {
            return false;
        }

        /// <summary>
        /// Checks the Abilities
        /// </summary>
        public virtual void CheckAbilities()
        {
            //See CNPC
        }

        #endregion

        #region Aggro

        /// <summary>
        /// Max Aggro range in that this npc searches for enemies
        /// </summary>
        protected int m_aggroMaxRange;
        /// <summary>
        /// Aggressive Level of this npc
        /// </summary>
        protected int m_aggroLevel;
        /// <summary>
        /// List of livings that this npc has aggro on, living => aggroamount
        /// </summary>
        public readonly Dictionary<GameLiving, long> m_aggroTable = new Dictionary<GameLiving, long>();

        public readonly Dictionary<GameLiving, long> m_returnHomeAggroTable = new Dictionary<GameLiving, long>();

        /// <summary>
        /// List of livings that this npc has aggro on, living => aggroamount
        /// </summary>
        public readonly Dictionary<GameLiving, long> m_adds = new Dictionary<GameLiving, long>();



        /// List of solo healers that this npc has aggro on, living => aggroamount
        /// </summary>
        protected readonly Dictionary<GameLiving, long> m_HealerList = new Dictionary<GameLiving, long>();

        /// <summary>
        /// The aggression table for this mob
        /// </summary>
        public Dictionary<GameLiving, long> HealerList
        {
            get { return m_HealerList; }
        }


        /// <summary>
        /// The aggression table for this mob
        /// </summary>
        public Dictionary<GameLiving, long> AggroTable
        {
            get { return m_aggroTable; }
        }

        /// <summary>
        /// Aggressive Level in % 0..100, 0 means not Aggressive
        /// </summary>
        public virtual int AggroLevel
        {
            get { return m_aggroLevel; }
            set { m_aggroLevel = value; }
        }

        /// <summary>
        /// Range in that this npc aggros
        /// </summary>
        public virtual int AggroRange
        {
            get { return m_aggroMaxRange; }
            set { m_aggroMaxRange = value; }
        }

        /// <summary>
        /// Checks whether living has someone on its aggrolist
        /// </summary>
        public virtual bool HasAggro
        => AggroTable.Count > 0;

        public virtual int CalculateAggroRange(GameNPC npc)
        {
            int newAggroRange = AggroRange;

            if (npc is GameGuard == false && npc.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Distance)
            {
                GameSpellEffect nearsightSepell = SpellHandler.FindEffectOnTarget(npc, "Nearsight");

                if (nearsightSepell != null)
                {
                    newAggroRange -= (int)(((int)nearsightSepell.Spell.Value * AggroRange) / 100);

                    if (newAggroRange > 0)
                        //log.ErrorFormat("Aggro range: {0}", newAggroRange);
                        return newAggroRange;
                }
            }
            return AggroRange;
        }

        /// <summary>
        /// Add aggro table of this brain to that of another living.
        /// </summary>
        /// <param name="brain">The target brain.</param>
        public void AddAggroListTo(StandardMobBrain brain)
        {
            // TODO: This should actually be the other way round, but access
            // to m_aggroTable is restricted and needs to be threadsafe.

            // do not modify aggro list if dead
            if (!brain.Body.IsAlive) return;

            lock ((m_aggroTable as ICollection).SyncRoot)
            {
                Dictionary<GameLiving, long>.Enumerator dictEnum = m_aggroTable.GetEnumerator();
                while (dictEnum.MoveNext())
                    brain.AddToAggroList(dictEnum.Current.Key, Body.MaxHealth);
            }
        }

        // LOS Check on natural aggro (aggrorange & aggrolevel)
        // This part is here due to los check constraints;
        // Otherwise, it should be in CheckPlayerAggro() method.
        private bool m_AggroLOS;
        public virtual bool AggroLOS
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
        public virtual void AddToAggroList(GameLiving living, int aggroamount)
        {
            AddToAggroList(living, aggroamount, false);
        }


        public virtual void AddToAggroList(GameLiving living, int aggroamount, bool CheckLOS)
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
            if (DOL.GS.ServerProperties.Properties.ALWAYS_CHECK_LOS && CheckLOS)
            {
                GamePlayer thisLiving = null;
                if (living is GamePlayer)
                    thisLiving = (GamePlayer)living;

                else if (living is GameNPC && (living as GameNPC).Brain is IControlledBrain)
                    thisLiving = ((living as GameNPC).Brain as IControlledBrain).GetPlayerOwner();

                if (thisLiving != null)
                {
                    thisLiving.Out.SendCheckLOS(Body, living, new CheckLOSResponse(CheckAggroLOS));
                    if (!AggroLOS) return;
                }
            }

            #region HealAggro VS Player


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



                #endregion

                //ProtectEffect protect = (ProtectEffect) player.EffectList.GetOfType(typeof(ProtectEffect));
                foreach (ProtectEffect protect in player.EffectList.GetAllOfType<ProtectEffect>())
                {
                    if (protect == null) break;
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

                        protect.ProtectSource.Out.SendMessage(LanguageMgr.GetTranslation(protect.ProtectSource.Client.Account.Language, "AI.Brain.StandardMobBrain.YouProtDist", player.GetName(0, false), Body.GetName(0, false, protect.ProtectSource.Client.Account.Language, Body)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
        public virtual long GetAggroAmountForLiving(GameLiving living)
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
        public virtual void RemoveFromAggroList(GameLiving living)
        {
            lock ((m_aggroTable as ICollection).SyncRoot)
            {
                m_aggroTable.Remove(living);
            }
        }


        /// <summary>
        /// Remove all livings from the aggrolist
        /// </summary>
        public virtual void ClearHealList()
        {
            lock ((m_HealerList as ICollection).SyncRoot)
            {
                m_HealerList.Clear();
                // Body.TempProperties.removeProperty(Body.Attackers);
            }
        }

        /// <summary>
        /// Remove all livings from the aggrolist
        /// </summary>
        public virtual void ClearAggroList()
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
        public virtual Dictionary<GameLiving, long> CloneAggroList()
        {
            lock ((m_aggroTable as ICollection).SyncRoot)
            {
                return new Dictionary<GameLiving, long>(m_aggroTable);
            }
        }

        /// <summary>
        /// Selects and attacks the next target or does nothing
        /// </summary>
        protected virtual void AttackMostWanted()
        {
            if (!IsActive)
                return;

          
            
            Body.TargetObject = CalculateNextAttackTarget();
           

            if (Body.TargetObject != null && Body.TargetObject.ObjectState == GameObject.eObjectState.Active)
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
        protected virtual GameLiving CalculateNextAttackTarget()
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
                    //log.ErrorFormat("MoveNext: {0}", living.Name);
                    // check to make sure this target is still valid
                    if (living.IsAlive == false ||
                        living.ObjectState != GameObject.eObjectState.Active ||
                        living.IsStealthed && Body is GuardStealther == false ||
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
                        //log.ErrorFormat("Body gefunden name: {0}", Body.Name);
                        int distance = Body.GetDistanceTo(living);
                        //log.ErrorFormat("Body gefunden name: {0}, distance {1}", living.Name, distance);
                        int maxAggroDistance = (this is IControlledBrain) ? MAX_PET_AGGRO_DISTANCE : MAX_AGGRO_DISTANCE;

                        if (distance <= maxAggroDistance)
                        {
                            
                            double aggro = amount * Math.Min(100.0 / distance, 1);//original war: 500.0
                            if (aggro > maxAggro)
                            {
                                
                                maxAggroObject = living;
                                maxAggro = aggro;
                                //log.ErrorFormat("Mob gefunden add: {0}, maxAggro; {1}", living.Name, maxAggro);
                                
                            }
                        }
                    }
                }

                foreach (GameLiving l in removable)
                {
                    // RemoveFromHealList(l);
                    RemoveFromAggroList(l);
                    Body.RemoveAttacker(l);
                }
            }

            if (maxAggroObject == null)
            {
                m_aggroTable.Clear();
                // m_HealerList.Clear();
            }
            if (maxAggroObject != null)
            {
               Body.TurnTo(maxAggroObject);//rearlyer targetting
            }
            //Body.PathTo(maxAggroObject.X, maxAggroObject.Y, maxAggroObject.Z);
            return maxAggroObject;
        }

        /// <summary>
        /// calculate the aggro of this npc against another living
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual int CalculateAggroLevelToTarget(GameLiving target)
        {
            // Withdraw if can't attack.
            if (GameServer.ServerRules.IsAllowedToAttack(Body, target, true) == false)
                return 0;

            // Get owner if target is pet
            GameLiving realTarget = target;
            if (target is GameNPC)
            {
                if (((GameNPC)target).Brain is IControlledBrain)
                {
                    GameLiving owner = (((GameNPC)target).Brain as IControlledBrain).GetLivingOwner();
                    if (owner != null)
                        realTarget = owner;
                }
            }

            // only attack if green+ to target
            if (realTarget.IsObjectGreyCon(Body))
                return 0;


           

            // If this npc have Faction return the AggroAmount to Player
            if (Body.Faction != null)
            {
                if (realTarget is GamePlayer)
                {
                    return Math.Min(100, Body.Faction.GetAggroToFaction((GamePlayer)realTarget));
                }
               
                else if (realTarget is GameNPC && Body.Faction.EnemyFactions.Contains(((GameNPC)realTarget).Faction))
                {
                    return 65;
                }
               
               
            }

            //we put this here to prevent aggroing non-factions npcs
            if (Body.Realm == eRealm.None && realTarget is GameNPC)
                return 0;

            return Math.Min(100, AggroLevel);
        }


        /// <summary>
        /// Receives all messages of the body
        /// </summary>
        /// <param name="e">The event received</param>
        /// <param name="sender">The event sender</param>
        /// <param name="args">The event arguments</param>
        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            double healthaggro = 0;
            double Pethealthaggro = 0;
            double Sourcehealthaggro = 0;
            base.Notify(e, sender, args);

            if (!IsActive) return;

            if (sender == Body)
            {
                if (e == GameObjectEvent.TakeDamage)
                {
                    TakeDamageEventArgs eArgs = args as TakeDamageEventArgs;
                    if (eArgs == null || eArgs.DamageSource is GameLiving == false) return;

                    int aggro = eArgs.DamageAmount + eArgs.CriticalAmount;



                    if (eArgs.DamageSource is GameNPC)
                    {



                        // owner gets 25% of aggro
                        IControlledBrain brain = ((GameNPC)eArgs.DamageSource).Brain as IControlledBrain;
                        if (brain != null)
                        {
                            Pethealthaggro = (Math.Max(50, (double)Body.HealthPercent));
                            Sourcehealthaggro = (Math.Max(50, (double)((GameNPC)eArgs.DamageSource).HealthPercent));
                            healthaggro = Math.Max((double)15 * 0.01, ((Math.Max((double)Pethealthaggro, (double)Sourcehealthaggro)) * (Properties.MOB_petowner_base_Aggrolevel) / 100));


                            //log.ErrorFormat(" aggroamount = {0}", healthaggro);
                            AddToAggroList(brain.Owner, (int)Math.Max(1, aggro * healthaggro));//pet owner base aggro 25% original
                            aggro = (int)Math.Max(1, aggro * 0.75);
                        }
                    }


                    AddToAggroList((GameLiving)eArgs.DamageSource, aggro);
                    return;
                }
                else if (e == GameLivingEvent.AttackedByEnemy)
                {
                    AttackedByEnemyEventArgs eArgs = args as AttackedByEnemyEventArgs;
                    if (eArgs == null) return;
                    OnAttackedByEnemy(eArgs.AttackData);
                    return;
                }
                else if (e == GameLivingEvent.Dying)
                {
                    // clean aggro table
                    ClearAggroList();
                    ClearHealList();
                    return;
                }
                else if (e == GameNPCEvent.FollowLostTarget) // this means we lost the target
                {
                    FollowLostTargetEventArgs eArgs = args as FollowLostTargetEventArgs;
                    if (eArgs == null) return;
                    OnFollowLostTarget(eArgs.LostTarget);
                    return;
                }
                else if (e == GameLivingEvent.CastFailed)
                {
                    CastFailedEventArgs realArgs = args as CastFailedEventArgs;
                    if (realArgs == null || realArgs.Reason == CastFailedEventArgs.Reasons.AlreadyCasting || realArgs.Reason == CastFailedEventArgs.Reasons.CrowdControlled)
                        return;
                    Body.StartAttack(Body.TargetObject);
                }
            }

            if (e == GameLivingEvent.EnemyHealed)
            {
                EnemyHealedEventArgs eArgs = args as EnemyHealedEventArgs;
                if (eArgs != null && eArgs.HealSource is GameLiving)
                {


                    //heilung von aussen
                    if (eArgs.HealSource is GamePlayer)
                    {
                        GamePlayer player = eArgs.HealSource as GamePlayer;

                        if (player.IsAlive && player.Group == null)
                        {
                            //Console.Out.WriteLine("es wurde von: {0}  geheilt!!", player.Name);

                            if (!HealerList.ContainsKey(player))
                            {
                                HealerList.Add(player, 1L);
                            }
                        }
                    }


                    // first check to see if the healer is in our aggrolist so we don't go attacking anyone who heals
                    if (m_aggroTable.ContainsKey(eArgs.HealSource as GameLiving))
                    {
                        if (eArgs.HealSource is GamePlayer || (eArgs.HealSource is GameNPC && (((GameNPC)eArgs.HealSource).Flags & GameNPC.eFlags.PEACE) == 0))
                        {
                            AddToAggroList((GameLiving)eArgs.HealSource, eArgs.HealAmount);
                        }
                    }
                }
                return;
            }


            else if (e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs eArgs = args as EnemyKilledEventArgs;
                if (eArgs != null)
                {
                    // transfer all controlled target aggro to the owner
                    if (eArgs.Target is GameNPC)
                    {
                        IControlledBrain controlled = ((GameNPC)eArgs.Target).Brain as IControlledBrain;
                        if (controlled != null)
                        {
                            long contrAggro = GetAggroAmountForLiving(controlled.Body);
                            AddToAggroList(controlled.Owner, (int)contrAggro);
                        }
                    }

                    Body.Attackers.Remove(eArgs.Target);
                    AttackMostWanted();
                }
                return;
            }

        }

        /// <summary>
        /// Lost follow target event
        /// </summary>
        /// <param name="target"></param>
        protected virtual void OnFollowLostTarget(GameObject target)
        {
            AttackMostWanted();
            if (!Body.AttackState)
                Body.WalkToSpawn();
        }

        /// <summary>
        /// Attacked by enemy event
        /// </summary>
        /// <param name="ad"></param>
        protected virtual void OnAttackedByEnemy(AttackData ad)
        {
            if (!Body.AttackState
                && Body.IsAlive
                && Body.ObjectState == GameObject.eObjectState.Active)
            {
               

                if ((Body is GameKeepGuard || Body is GamePet || Body.Brain is ControlledNpcBrain
                    || Body.Brain is MobPetBrain || Body.Brain is HoundsPetBrain || Body.Brain is CommanderBrain
                    || Body is TheurgistPet || Body is NecromancerPet) && AggroTable.ContainsKey(ad.Attacker) == false)
                {
                    AddToAggroList(ad.Attacker, (ad.Attacker.Level + 1) << 1, true);
                }

                else if (AggroTable.ContainsKey(ad.Attacker) == false)
                {
                    AddToAggroList(ad.Attacker, (ad.Attacker.Level + 1) << 1);
                }
                Body.StartAttack(ad.Attacker);

                //log.Error("start attack");

                NpcYell(Body, ad.Attacker);
                BringFriends(ad);
            }
        }

        #endregion

        #region Bring a Friend

        /// <summary>
        /// Mobs within this range will be called upon to add on a group
        /// of players inside of a dungeon.
        /// </summary>
        protected static ushort m_BAFReinforcementsRange = 1000; //2000

        /// <summary>
        /// Players within this range around the puller will be subject
        /// to attacks from adds.
        /// </summary>
        protected static ushort m_BAFTargetPlayerRange = 1500; //3000

        /// <summary>
        /// BAF range for adds close to the pulled mob.
        /// </summary>
        public virtual ushort BAFCloseRange
        {
            get { return (ushort)((AggroRange * 2) / 5); }
        }

        /// <summary>
        /// BAF range for group adds in dungeons.
        /// </summary>
        public virtual ushort BAFReinforcementsRange
        {
            get { return m_BAFReinforcementsRange; }
            set { m_BAFReinforcementsRange = (value > 0) ? (ushort)value : (ushort)0; }
        }

        /// <summary>
        /// Range for potential targets around the puller.
        /// </summary>
        public virtual ushort BAFTargetPlayerRange
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
        protected virtual void BringFriends(AttackData attackData)
        {
            int membersInGroup = 0;
            int addChance = 5;

            // Only add on players.
            GameLiving attacker = attackData.Attacker;

            if (attacker.Group != null)
            {
                membersInGroup = attacker.Group.GetMembersInTheGroup().Count;
            }

            addChance = membersInGroup * addChance;

            if (attacker is GamePlayer && attacker.Group != null && Util.Chance(addChance))
            {
                BringCloseFriends(attackData);
                if (attacker.CurrentRegion.IsDungeon)
                {
                    BringReinforcements(attackData);
                }
            }
        }

        /// <summary>
        /// Get mobs close to the pulled mob to add on the puller and his
        /// group as well.
        /// </summary>
        /// <param name="attackData">The data associated with the puller's attack.</param>
        protected virtual void BringCloseFriends(AttackData attackData)
        {
            // Have every friend within close range add on the attacker's
            // group.
            // log.Warn("freund gefunden");
            StandardMobBrain brain = null;

            GamePlayer attacker = (GamePlayer)attackData.Attacker;

            foreach (GameNPC npc in Body.GetNPCsInRadius(BAFCloseRange))
            {

                if (npc.Brain is StandardMobBrain)
                {
                    brain = (StandardMobBrain)npc.Brain;
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
        protected virtual void BringReinforcements(AttackData attackData)
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
                            StandardMobBrain brain = (StandardMobBrain)npc.Brain;
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

        /// <summary>
        /// Pick a random target from the attacker's group that is within a certain
        /// range of the original puller.
        /// </summary>
        /// <param name="attacker">The original attacker.</param>
        /// <returns></returns>
        public virtual GamePlayer PickTarget(GamePlayer attacker)
        {
            Group attackerGroup = attacker.Group;

            BattleGroup battleGroup = attacker.TempProperties.getProperty<BattleGroup>(BattleGroup.BATTLEGROUP_PROPERTY, null);

            // If no group, pick the attacker himself.
            if (Properties.PVE_MOB_Pick_Attacker_Only)
            {
                return attacker;
            }
            else if (attackerGroup == null && battleGroup == null) return attacker;

            // Make a list of all players in the attacker's group within
            // a certain range around the puller.

            ArrayList attackersInRange = new ArrayList();


            if (battleGroup != null && GS.ServerProperties.Properties.BATTLEGROUP_LOOT)
            {
                foreach (GamePlayer players in battleGroup.Members.Keys)
                {
                    if (attacker.IsWithinRadius(players, BAFTargetPlayerRange))
                        attackersInRange.Add(players);
                }

            }
            else
            {
                foreach (GamePlayer player in attackerGroup.GetPlayersInTheGroup())
                    if (attacker.IsWithinRadius(player, BAFTargetPlayerRange))
                        attackersInRange.Add(player);
            }

            // Pick a random player from the list.

            return (GamePlayer)(attackersInRange[Util.Random(1, attackersInRange.Count) - 1]);
        }


        public static int GroupExtraXPBonus;
        public static bool WasBonus;

        /// <summary>
        /// NPCs can Yell for help
        /// range of the original puller.
        /// </summary>
        /// <param name="attacker">The original attacker.</param>
        /// /// <param name="AttacketNPC">The attcket npc.</param>
        /// <returns></returns>
        public virtual void NpcYell(GameNPC attackedNPC, GameLiving attacker)
        {
            bool wasYell = false;
            bool chanceToYell = Util.Chance(DOL.GS.ServerProperties.Properties.GAMENPC_YELL_CHANCE);//chance percent to yell
            bool chanceToYellBoss = Util.Chance(DOL.GS.ServerProperties.Properties.GAMENPC_YELL_CHANCE_BOSS);//chance percent to yell
            bool chanceToAddOnOwner = Util.Chance(DOL.GS.ServerProperties.Properties.GAMENPC_YELL_Switch_CHANCE);//chance percent to attack pet owners
            GameNPC npcFriend = null;
            GamePlayer attackOwner = null;
            attackedNPC = Body;

            if (chanceToYell && wasYell == false)
                wasYell = true;

            //only npcs allow to add
            if (attackedNPC == null || attacker == null || attackedNPC.Brain is StandardMobBrain == false)
                return;


            if (!GameServer.ServerRules.IsAllowedToAttack(attackedNPC, attacker, true))
                return;


            //no npc as attackers
            if (attacker is GameNPC && ((attacker as GameNPC).Brain is IControlledBrain == false || (attacker as GameNPC).Brain is MobPetBrain))
                return;


            //if there is an pet adds can attack owers 
            if (attacker is GameNPC && (attacker as GameNPC).Brain is IControlledBrain)
            {
                ControlledNpcBrain brain = ((GameNPC)attacker).Brain as ControlledNpcBrain;

                if (brain != null)
                {
                    attackOwner = brain.GetPlayerOwner();

                    //allow only necros pet as target
                    if (attackOwner != null && attacker.EffectList.GetOfType<NecromancerShadeEffect>() == null && chanceToAddOnOwner)
                    {
                        attacker = attackOwner;
                    }



                }
            }

            //don't add necromancer
            if (attacker.EffectList.GetOfType<NecromancerShadeEffect>() != null || attacker.Level < 10)
                return;

            if (attacker != null && attackedNPC.InCombat)
            {
                foreach (GameLiving adds in attackedNPC.GetNPCsInRadius((ushort)attackedNPC.YellRange))
                {


                    if (((GameNPC)adds) is GameKeepGuard || ((GameNPC)adds).Brain is StandardMobBrain == false || ((GameNPC)adds).Brain is GuardBrain)
                        continue;

                    GameNPC add = adds as GameNPC;



                    if (attackedNPC != null && add.IsAlive && add.Brain.IsActive && attacker.IsAlive)
                    {
                        if (attackedNPC.Brain is StandardMobBrain && wasYell && add != null && add.IsAlive && add.ObjectState == GameObject.eObjectState.Active && add.IsAvailable && add.Name == attackedNPC.Name && add.InternalID != attackedNPC.InternalID ||
                       chanceToYellBoss && add != null && add.IsAlive && add.ObjectState == GameObject.eObjectState.Active && add.IsAvailable && add != attackedNPC && attackedNPC.BodyType == 36 && add.BodyType == 37)
                        {
                            npcFriend = add;



                            ArrayList npcFriendInRange = new ArrayList();




                            if (attacker.IsObjectGreyCon(npcFriend))
                                continue;




                            if (attacker.Group == null && attacker is GamePlayer || attacker is GamePlayer && attacker.Group.MemberCount < 5)
                            {
                                npcFriendInRange.Add(npcFriend);
                            }



                            //Group extra Group bonus
                            if (m_adds.Count > 0)
                            {
                                //log.ErrorFormat("extra bonus = {0}", m_adds.Count);
                                GroupExtraXPBonus = m_adds.Count;
                            }


                            if (attackedNPC.BodyType == 36 == false && npcFriend.CurrentRegion.IsDungeon)
                                break;

                            if (npcFriend.IsStunned || npcFriend.IsMezzed || npcFriend.IsCasting || npcFriend.IsAttacking)
                                break;

                            if (attackedNPC.IsStunned || attackedNPC.IsMezzed)
                                break;



                            if (attackedNPC.BodyType == 36 && (attacker is GamePlayer && attacker.Group != null && m_adds.Count < attacker.Group.MemberCount && attacker.Group.MemberCount > 4 || attacker is GamePlayer && attacker.Group == null && npcFriendInRange.Count > 0 || attacker is GamePlayer && npcFriendInRange.Count > 0 && attacker.Group.MemberCount < 5) && attacker != null && attacker.IsAlive && npcFriend != null && npcFriend.InCombat == false && npcFriend.IsAlive && npcFriend.IsWithinRadius(attacker, 1850))
                            {
                                // log.Warn("freund gefunden");
                                StandardMobBrain Bossbrain = null;

                                if (npcFriend.Brain is StandardMobBrain)
                                {
                                    Bossbrain = (StandardMobBrain)npcFriend.Brain;
                                }

                                //pic random member from groups
                                if (Bossbrain != null && attacker is GamePlayer)
                                {

                                    attacker = Bossbrain.PickTarget((GamePlayer)attacker);
                                }
                                if (Bossbrain != null && Bossbrain.m_aggroTable.ContainsKey(attacker) == false)
                                {
                                    //log.Warn("Attacker ist nun in der aggoliste");
                                    Bossbrain.AddToAggroList(attacker, attacker.EffectiveLevel << 1);
                                    Bossbrain.Body.StopAttack();
                                    Bossbrain.Body.StopCurrentSpellcast();
                                    Bossbrain.Body.PathTo(attacker.X, attacker.Y, attacker.Z, Bossbrain.Body.MaxSpeed);
                                    Bossbrain.Body.StartAttack(attacker);
                                }
                            }
                            else if (attackedNPC.Brain is StandardMobBrain && (attacker is GamePlayer && attacker.Group != null && m_adds.Count < attacker.Group.MemberCount && attacker.Group.MemberCount > 4 || (attacker is GamePlayer && attacker.Group == null && npcFriendInRange.Count > 0 || npcFriendInRange.Count > 0 && attacker is GamePlayer && attacker.Group.MemberCount < 5)) && attacker != null && attacker.IsAlive && npcFriend != null && npcFriend.InCombat == false && npcFriend.IsAlive && npcFriend.IsWithinRadius(attacker, 1850))
                            {
                                {
                                    // log.Warn("freund gefunden");
                                    StandardMobBrain brain = null;

                                    if (npcFriend.Brain is StandardMobBrain)
                                    {
                                        brain = (StandardMobBrain)npcFriend.Brain;
                                    }

                                    //pic random member from groups
                                    if (brain != null && attacker is GamePlayer)
                                    {

                                        attacker = brain.PickTarget((GamePlayer)attacker);
                                    }
                                    if (brain != null && brain.m_aggroTable.ContainsKey(attacker) == false)
                                    {
                                        //log.Warn("Attacker ist nun in der aggoliste");
                                        //brain.AddToAggroList((attacker), 1);
                                        brain.AddToAggroList(attacker, attacker.EffectiveLevel << 1);
                                        brain.Body.StopAttack();
                                        brain.Body.StopCurrentSpellcast();
                                        //brain.Body.WalkTo(attacker, brain.Body.MaxSpeed);
                                        brain.Body.PathTo(attacker.X, attacker.Y, attacker.Z, brain.Body.MaxSpeed);
                                        brain.Body.StartAttack(attacker);

                                        if (m_adds.ContainsKey(brain.Body) == false)
                                        {
                                            m_adds.Add(brain.Body, 1);

                                            if (attacker.Group != null && m_adds.Count < attacker.Group.MemberCount && attacker.Group.MemberCount > 4)
                                                continue;
                                        }
                                    }



                                    //send yelling to all players around the attacked npc
                                    foreach (GamePlayer player in attackedNPC.GetPlayersInRadius(2500))
                                    {
                                        //send only if the attacker is in aggrolist
                                        if (player != null && brain.m_aggroTable.ContainsKey(attacker))
                                        {
                                            player.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameLiving.AttackData.BringFriend"), Body.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                            WasBonus = true;
                                            wasYell = false;
                                            m_adds.Clear();
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }




        #endregion

        #region Spells

        public enum eCheckSpellType
        {
            Offensive,
            Defensive
        }

        private GameLiving living = null;


        /// <summary>
        /// Checks if any spells need casting
        /// </summary>
        /// <param name="type">Which type should we go through and check for?</param>
        /// <returns></returns>
        public virtual bool CheckSpells(eCheckSpellType type)
        {
            if (Body.IsCasting)
                return true;

            if (Body is TheurgistPet == false)



                //By elcotek Mob cast only if spell not active on Target
                if (Body.TargetObject != null)
                {
                    living = Body.TargetObject as GameLiving;
                }


            bool casted = false;

            if (Body != null && Body.Spells != null && Body.Spells.Count > 0 && !Body.IsBeingInterrupted
                    || Body != null && Body.Spells != null && Body.Spells.Count > 0 && Body.IsWithinRadius(Body.TargetObject, 400))
            {

                ArrayList spell_rec = new ArrayList();
                Spell tire = null;
                bool needpet = false;
                bool needheal = false;

                if (type == eCheckSpellType.Defensive)
                {
                    foreach (Spell spell in Body.Spells)
                    {
                        if (spell == null) continue;//bugfix
                        if (Body.GetSkillDisabledDuration(spell) > 0 || SpellHandler.FindEffectAndImmunityOnTarget(living, spell.SpellType) != null && spell.SpellType.ToLower().Contains("summon") == false && spell.SpellType.ToLower().Contains("SummonMobPet") == false) continue;
                        if (spell.Target.ToLower() == "enemy" || spell.Target.ToLower() == "area" || spell.Target.ToLower() == "cone") continue;
                        // If we have no pets
                        if (Body.ControlledBrain == null)
                        {
                            if (spell.SpellType.ToLower() == "pet") continue;


                            //pet bugfix for SummonMobPet
                            if (spell.SpellType.ToLower().Contains("summon") || spell.SpellType.ToLower().Contains("SummonMobPet"))
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


                /// <summary>
                /// Checks if any Ovensive spells need casting near the Body
                /// </summary>
                /// <param name="type">New Intelligence for Casting mobs by Elcotek</param>
                /// <returns></returns>
                else if (type == eCheckSpellType.Offensive)
                {
                    if (Body is TheurgistPet == false)

                        foreach (Spell spell in Body.Spells)
                        {
                            //We allow casting only if there is not already the same type of spell and immunity active on target
                            if (living != null && Body.GetSkillDisabledDuration(spell) == 0 && SpellHandler.FindEffectAndImmunityOnTarget(living, spell.SpellType) == null || Body is GameKeepGuard || spell.SpellType.ToLower().Contains("DamageSpeedDecrease") || spell.SpellType.ToLower().Contains("DirectDamageWithDebuff"))
                            {

                                if (spell.CastTime > 0 && (!Body.IsBeingInterrupted && Body is NecromancerPet == false) && Body.IsWithinRadius(Body.TargetObject, spell.Range) || spell.Range == 0 && Body.IsWithinRadius(Body.TargetObject, spell.Radius)// (!Body.IsBeingInterrupted && Body is NecromancerPet  == false)  neu
                                    || spell.CastTime == 0 && Body.TargetObject != null && (Body.InCombat && Body is NecromancerPet == false))
                                {
                                    int Cast = Util.Random(1, 6);
                                    if (spell.CastTime == 0 && Cast != 3) continue;

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
                            if ((Body.IsWithinRadius(Body.TargetObject, 400) && CheckOffensiveSpells(tire)
                                || !Body.IsWithinRadius(Body.TargetObject, 400) && !Body.IsBeingInterrupted) && CheckOffensiveSpells(tire))
                            casted = true;
                    }
                }


            }
            return casted;
        }

        /// <summary>
        /// Checks defensive spells.  Handles buffs, heals, etc.
        /// </summary>
        protected virtual bool CheckDefensiveSpells(Spell spell)
        {
            if (spell == null || Body.GetSkillDisabledDuration(spell) > 0) return false;

            GameObject lastTarget = Body.TargetObject;
            Body.TargetObject = null;

            bool isTargetSelf = spell.Target.ToLower() == "self";

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
                        bool isMoving = Body.CurrentSpeed > 0;
                        if (!spell.MoveCast && spell.CastTime > 0 && isMoving) break;

                        if (!LivingHasEffect(Body, spell) && spell.CastTime > 0 && !Body.AttackState && !isTargetSelf)
                        {
                            Body.TargetObject = Body;
                          
                            break;
                        }

                        if (Body.ControlledBrain != null && Body.ControlledBrain.Body != null
                            && Util.Chance(40) && Body.GetDistanceTo(Body.ControlledBrain.Body) <= spell.Range
                            && !LivingHasEffect(Body.ControlledBrain.Body, spell) && !isTargetSelf)
                        {
                            Body.TargetObject = Body.ControlledBrain.Body;
                            break;
                        }
                        break;
                    }
                #endregion

                #region Disease Cure/Poison Cure/Summon
                case "CureDisease":
                    if (Body.IsDiseased || (Body.ControlledBrain?.Body?.IsDiseased == true && Body.GetDistanceTo(Body.ControlledBrain.Body) <= spell.Range && !isTargetSelf))
                    {
                        Body.TargetObject = Body.ControlledBrain?.Body ?? Body;
                        break;
                    }
                    break;

                case "CurePoison":
                    if (LivingIsPoisoned(Body) || (Body.ControlledBrain?.Body != null && LivingIsPoisoned(Body.ControlledBrain.Body) && Body.GetDistanceTo(Body.ControlledBrain.Body) <= spell.Range && !isTargetSelf))
                    {
                        Body.TargetObject = Body.ControlledBrain?.Body ?? Body;
                        break;
                    }
                    break;

                case "Summon":
                    Body.TargetObject = Body;
                    break;

                case "SummonMinion":
                    if (Body.ControlledNpcList == null)
                        Body.InitControlledBrainArray(2);
                    else
                    {
                        int numberofpets = Body.ControlledNpcList.Count(icb => icb != null);
                        if (numberofpets >= Body.ControlledNpcList.Length) break;
                    }
                    Body.TargetObject = Body;
                    break;
                #endregion

                #region Heals
                case "Heal":
                    if ((Body is GuardLord && Body.Level > 40 && Body.HealthPercent < 30 && Util.Chance(10)) ||
                        (Body is GuardLord == false && Body.HealthPercent < 30 && Util.Chance(10)) ||
                        (Body.ControlledBrain != null && Body.ControlledBrain.Body != null && Body.GetDistanceTo(Body.ControlledBrain.Body) <= spell.Range && Body.ControlledBrain.Body.HealthPercent < 60 && !isTargetSelf))
                    {
                        Body.TargetObject = Body.ControlledBrain?.Body ?? Body;
                        break;
                    }
                    break;
                #endregion

                #region Summon additional cases
                case "SummonMobPet":
                case "SummonFollowerPets":
                case "SummonCommander":
                case "SummonDruidPet":
                case "SummonHunterPet":
                case "SummonNecroPet":
                case "SummonUnderhill":
                case "FirbolgStagCallStone1":
                case "SummonSimulacrum":
                case "SummonSpiritFighter":
                    if (Body.ControlledBrain == null)
                        Body.TargetObject = Body;
                    break;
                    #endregion
            }

            if (Body.TargetObject != null)
            {
                bool isFarAway = spell.Range > 350 && Body.IsWithinRadius(Body.TargetObject, spell.Range);
                if (Body.IsMoving && spell.CastTime > 0 && isFarAway) Body.StopFollowing();
                if (Body.TargetObject != Body && spell.CastTime > 0 && isFarAway) Body.TurnTo(Body.TargetObject);

                Body.CastSpell(spell, m_mobSpellLine);
                Body.TargetObject = lastTarget;
                return true;
            }

            Body.TargetObject = lastTarget;
            return false;
        }


        /// <summary>
        /// Checks offensive spells.  Handles dds, debuffs, etc.
        /// </summary>
        protected virtual bool CheckOffensiveSpells(Spell spell)
        {
            if (spell.Target.ToLower() != "enemy" && spell.Target.ToLower() != "area" && spell.Target.ToLower() != "cone")
                return false;

            if (Body.TargetObject != null)
            {
                if (Body.IsMoving && spell.CastTime > 0 && spell.Range > 350 && Body.IsWithinRadius(Body.TargetObject, spell.Range))
                    Body.StopFollowing();

                if (Body.TargetObject != Body && spell.CastTime > 0 && spell.Range > 350 && Body.IsWithinRadius(Body.TargetObject, spell.Range))
                    Body.TurnTo(Body.TargetObject);

                if (Body.TargetObject != Body && spell.CastTime == 0 && Body.IsWithinRadius(Body.TargetObject, 150))
                    Body.TurnTo(Body.TargetObject);

                Body.CastSpell(spell, m_mobSpellLine);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks Instant Spells.  Handles Taunts, shouts, stuns, etc.
        /// </summary>
        protected virtual bool CheckInstantSpells(Spell spell)
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
                case "SpeedDecrease":
                case "UnbreakableSpeedDecrease":
                case "Disease":
                case "Stun":
                case "UnrresistableNonImunityStun":
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
                case "DefensiveProc":
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
                Body.CastSpell(spell, m_mobSpellLine);
                Body.TargetObject = lastTarget;
                return true;
            }

            Body.TargetObject = lastTarget;
            return false;
        }

        protected static SpellLine m_mobSpellLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells);

        /// <summary>
        /// Checks if the living target has a spell effect
        /// </summary>
        /// <param name="target">The target living object</param>
        /// <param name="spell">The spell to check</param>
        /// <returns>True if the living has the effect</returns>
        protected bool LivingHasEffect(GameLiving target, Spell spell)
        {
            if (target == null)
                return true;

            if (target is GamePlayer && (target as GamePlayer).IsCharcterClass(eCharacterClass.Vampiir))
            {
                switch (spell.SpellType)
                {
                    case "StrengthConstitutionBuff":
                    case "DexterityQuicknessBuff":
                    case "StrengthBuff":
                    case "DexterityBuff":
                    case "ConstitutionBuff":
                    case "AcuityBuff":
                        return true;
                }
            }
            if (target is GameNPC && spell.Target == "Self")
            {
                switch (spell.SpellType)
                {
                    //Für Threurg Air Pets
                    case "DefensiveProc":
                    case "OffensiveProc":
                        return false;
                }
            }




            lock (target.EffectList)
            {
                //Check through each effect in the target's effect list
                foreach (IGameEffect effect in target.EffectList)
                {
                    //If the effect we are checking is not a gamespelleffect keep going
                    if (effect is GameSpellEffect == false)
                        continue;

                    GameSpellEffect speffect = effect as GameSpellEffect;

                    //if the effect effectgroup is the same as the checking spells effectgroup then these are considered the same
                    if (speffect.Spell.EffectGroup == spell.EffectGroup)
                        return true;

                    //otherwise continue unless the SpellType is the same
                    if (speffect.Spell.SpellType == spell.SpellType)
                        return true;

                }
            }

            //the answer is no, the effect has not been found
            return false;
        }

        protected bool LivingIsPoisoned(GameLiving target)
        {
            foreach (IGameEffect effect in target.EffectList)
            {
                //If the effect we are checking is not a gamespelleffect keep going
                if (effect is GameSpellEffect == false)
                    continue;

                GameSpellEffect speffect = effect as GameSpellEffect;

                // if this is a DOT then target is poisoned
                if (speffect.Spell.SpellType == "DamageOverTime")
                    return true;
            }

            return false;
        }


        #endregion

        #region Random Walk
        public virtual bool CanRandomWalk
        {
            get
            {

                if (!Properties.ALLOW_ROAM)
                    return false;
                if (Body.RoamingRange == 0)
                    return false;
                return true;
            }
        }
        /// <summary> 
        /// Vector3D to Point3D 
        /// </summary>
        /// <param name="vector">Vector by which we offset the point. 
        /// <param name="point">Point being offset by the given vector.
        /// <returns>Result of addition.</returns>
        public static IPoint3D point3D(Vector3 vector)
        {
            Point3D point;

            point = new Point3D(vector.X, vector.Y, vector.Z);

            return point;
        }

        public virtual IPoint3D CalcRandomWalkTarget()
        {
            if (Body.IsCasting == false)
            {
                if (Body.CurrentZone.IsPathingEnabled)
                {
                    Vector3 spawnPointPosition = new Vector3(Body.SpawnPoint.X, Body.SpawnPoint.Y, Body.SpawnPoint.Z);
                    Point3D point;
                    var pt = PathingMgr.Instance.GetRandomPointAsync(Body.CurrentZone, spawnPointPosition, (float)(Body.RoamingRange > 0 ? Body.RoamingRange : 200)).Result;


                    if (pt.HasValue)
                    {
                        point = new Point3D(pt.Value.X, pt.Value.Y, pt.Value.Z);
                        if (GS.ServerProperties.Properties.ENABLE_Pathig_DEBUG)
                            log.ErrorFormat("pathing pt value: {0}!! bodyPosition: {1} ", pt.Value.ToString(), spawnPointPosition);
                        // Vector3 bodyPositions = new Vector3(pt.Value.X, pt.Value.Y, pt.Value.Z);

                        //var bodyPosition2 = new Point(pt.X, pt.Y, pt.Z);

                        return point;
                    }
                    else

                    if (GS.ServerProperties.Properties.ENABLE_Pathig_DEBUG)
                    {
                        log.Error("pathing ist aus!!");
                    }
                }

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
            else
                return null;
        }

        #endregion
    }
}