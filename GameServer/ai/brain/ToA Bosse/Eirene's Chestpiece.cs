/*
 * 
 * StephenxPimentel / HellFire
 * 
 */
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.Keeps;
using DOL.GS.Movement;
using DOL.GS.PacketHandler;
using DOL.GS.ServerProperties;
using DOL.GS.SkillHandler;
using DOL.Language;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace DOL.GS.Scripts
{
    public class Khonorn : GameNPC
    {

        public override bool AddToWorld()
        {

            this.SetOwnBrain(new KhonornBrain());
            Name = "Khonorn";
            GuildName = "Artifact Encounter";
            Level = 65;
            Model = 33801;
            Strength = 410;
            Constitution = 350;
            Dexterity = 65;
            Quickness = 60;
            Flags = 0;
            return base.AddToWorld();
        }
        public override void Die(GameObject killer)
        {
            if (killer != null && killer != null)
            {
                base.Die(killer);

                GamePlayer player = null;
                GameLiving mobKiller = null;
                ControlledNpcBrain brain = null;

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
                            player = mobKiller as GamePlayer;
                        }

                        if (killer != null && player != null)
                        {
                            player = killer as GamePlayer;
                        }
                    }
                }
                else if (killer != null && killer is GamePlayer)
                {
                    player = killer as GamePlayer;
                }
                if (player != null)
                {
                    if (player is GamePlayer && IsWorthReward && (player.Group != null || player.MemberIsInBG(player) && ServerProperties.Properties.BATTLEGROUP_LOOT))
                    {
                        BattleGroup battleGroup = player.TempProperties.getProperty<BattleGroup>(BattleGroup.BATTLEGROUP_PROPERTY, null);

                        if (battleGroup != null && GS.ServerProperties.Properties.BATTLEGROUP_LOOT)
                        {
                            //BattleGroup
                            foreach (GamePlayer players in battleGroup.Members.Keys)
                            {
                                if (players.IsWithinRadius(killer, 8000) == false)
                                    continue;
                                players.ReceiveItem(this, "eirenes_journal_book", 0);
                                players.BountyPointsAndMassage(25);
                                players.UpdatePlayerStatus();
                            }
                        }
                        else
                        {
                            //PlayerGroup
                            foreach (GamePlayer players in player.Group.GetPlayersInTheGroup())
                            {
                                if (players.IsWithinRadius(killer, 8000) == false)
                                    continue;
                                players.ReceiveItem(this, "eirenes_journal_book", 0);
                                players.BountyPointsAndMassage(25);
                                players.UpdatePlayerStatus();
                            }
                        }
                    }
                    else
                    {
                        if (player.IsWithinRadius(killer, 8000) == false)
                            return;
                        player.ReceiveItem(this, "eirenes_journal_book", 0);
                        player.BountyPointsAndMassage(25);
                        player.UpdatePlayerStatus();

                    }
                }
            }
        }



        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        GameLiving m_MidKingTarget;
        public GameLiving MidKingTarget
        {
            get
            {
                return m_MidKingTarget;
            }
            set
            {
                m_MidKingTarget = value;
            }
        }

        public override int MaxHealth
        {
            get
            {
                return base.MaxHealth * 5;
            }
        }

        public override double AttackDamage(InventoryItem weapon)
        {
            return base.AttackDamage(weapon) * ServerProperties.Properties.SET_DIFFICULTY_ON_ARTIFACT_ENCOUNTERS;
        }


        public override int AttackRange
        {
            get
            {
                return 250;
            }
            set { }
        }
    }
}
namespace DOL.AI.Brain
{
    public class KhonornBrain : StandardMobBrain, IOldAggressiveBrain
    {

        public const int mez = 1686;
        public const int snare = 900021;
        public const int pbaoeheal = 1667;

        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructs a new StandardMobBrain
        /// </summary>
        public KhonornBrain()
            : base()
        {
            base.m_aggroLevel = 0;
            base.m_aggroMaxRange = 0;
            base.AggroRange = 500;
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


            // If the NPC is tethered and has been pulled too far it will
            // de-aggro and return to its spawn point.
            if (Body.IsOutOfTetherRange && !Body.InCombat)
            {
                Body.WalkToSpawn();
                return;
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
                && String.IsNullOrEmpty(Body.PathID) == false)
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

        #endregion

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

                if (player.Group != null)
                { // player is in group, add whole group to aggro list
                    lock ((m_aggroTable as ICollection).SyncRoot)
                    {
                        foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
                        {
                            if (!m_aggroTable.ContainsKey(p))
                            {
                                m_aggroTable[p] = 1L;	// add the missing group member on aggro table
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
                        protect.ProtectSource.Out.SendMessage(LanguageMgr.GetTranslation(protect.ProtectSource.Client, "AI.Brain.KhonornBrain.YouProtDist", player.GetName(0, false), Body.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
        /// Remove all livings from the aggrolist
        /// </summary>
        public override void ClearHealList()
        {
            lock ((m_HealerList as ICollection).SyncRoot)
            {
                m_HealerList.Clear();
                // Body.TempProperties.removeProperty(Body.Attackers);
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
                    foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)AggroRange))
                    {

                        if (Body.InCombat)
                        {
                            int RandSpell = Util.Random(1, 16);

                            if (RandSpell == 1)
                            {
                                //PBAOE Heal (Clerics "Banish Evil" Spell)
                                Body.CastSpell(SkillBase.GetSpellByID(pbaoeheal), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));

                            }
                            if (RandSpell == 2)
                            {

                                //PBAOE Mez (Clerics "Theophany" Spell)
                                Body.CastSpell(SkillBase.GetSpellByID(mez), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                            }
                            if (RandSpell == 3)
                            {
                                //PBAOE Snare (Clerics "Blessed Deliverance" Spell)
                                Body.CastSpell(SkillBase.GetSpellByID(snare), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));

                            }
                            if (RandSpell == 4)
                            {

                            }
                        }
                    }
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
        /// Receives all messages of the body
        /// </summary>
        /// <param name="e">The event received</param>
        /// <param name="sender">The event sender</param>
        /// <param name="args">The event arguments</param>
        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
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
                            AddToAggroList(brain.Owner, (int)Math.Max(1, aggro * 0.25));
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
                    ClearHealList();
                    ClearAggroList();
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
                //Yell
                NpcYell(Body, ad.Attacker);
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
            KhonornBrain brain = null;

            GamePlayer attacker = (GamePlayer)attackData.Attacker;

            foreach (GameNPC npc in Body.GetNPCsInRadius(BAFCloseRange))
            {
                if (npc.Brain is KhonornBrain)
                {
                    brain = (KhonornBrain)npc.Brain;
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
                            KhonornBrain brain = (KhonornBrain)npc.Brain;
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
        /// NPCs can Yell for help
        /// range of the original puller.
        /// </summary>
        /// <param name="attacker">The original attacker.</param>
        /// /// <param name="AttacketNPC">The attcket npc.</param>
        /// <returns></returns>
        public override void NpcYell(GameNPC attackedNPC, GameLiving attacker)
        {

            bool chanceToYellBoss = Util.Chance(DOL.GS.ServerProperties.Properties.GAMENPC_YELL_CHANCE_BOSS);//chance percent to yell
            bool chanceToAddOnOwner = Util.Chance(DOL.GS.ServerProperties.Properties.GAMENPC_YELL_Switch_CHANCE);//chance percent to attack pet owners
            GameNPC npcFriend = null;
            GamePlayer attackOwner = null;
            attackedNPC = Body;

            //only npcs allow to add
            if (attackedNPC == null || attacker == null || attackedNPC.Brain is KhonornBrain == false)
                return;
            //log.Warn("wurde attacked");

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
                // log.Warn("attacker gefunden 2");
                foreach (GameLiving adds in attackedNPC.GetNPCsInRadius((ushort)attackedNPC.YellRange))
                {
                    if (((GameNPC)adds) is GameKeepGuard || ((GameNPC)adds).Brain is StandardMobBrain == false || ((GameNPC)adds).Brain is GuardBrain)
                        continue;
                    GameNPC add = adds as GameNPC;

                    //log.Warn("freund gefunden 1");
                    if (attackedNPC != null && add.IsAlive && add.Brain.IsActive && attacker.IsAlive && add.TargetInView)
                    {
                        if (attackedNPC.Brain is KhonornBrain && chanceToYellBoss && add != null && add.IsAlive && add.ObjectState == GameObject.eObjectState.Active && add.IsAvailable && add != attackedNPC && attackedNPC.BodyType == 36 && add.BodyType == 37)
                        {
                            npcFriend = add;

                            ArrayList npcFriendInRange = new ArrayList();

                            if (npcFriendInRange.Count > 1)
                                break;

                            if (attacker.IsObjectGreyCon(npcFriend))
                                continue;


                            //add to list
                            if (npcFriendInRange.Count == 0)
                            {

                                npcFriendInRange.Add(npcFriend);
                            }



                            if (npcFriend.IsStunned || npcFriend.IsMezzed || npcFriend.IsCasting || npcFriend.IsAttacking)
                                break;

                            if (attackedNPC.IsStunned || attackedNPC.IsMezzed)
                                break;



                            if (attackedNPC.BodyType == 36 && npcFriendInRange.Count > 0 && attacker != null && attacker.IsAlive && npcFriend != null && npcFriend.InCombat == false && npcFriend.IsAlive && npcFriend.IsWithinRadius(attacker, 1850))
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

                                    brain.AddToAggroList(attacker, attacker.EffectiveLevel << 1);
                                    brain.Body.StopAttack();
                                    brain.Body.StopCurrentSpellcast();
                                    brain.Body.PathTo(attacker.X, attacker.Y, attacker.Z, brain.Body.MaxSpeed);
                                    brain.Body.StartAttack(attacker);
                                }
                            }
                        }
                    }
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

                Body.CastSpell(spell, m_mobSpellLine);
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
                Body.CastSpell(spell, m_mobSpellLine);
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
            if (Body.CurrentZone.IsPathingEnabled)
            {
                Vector3 bodyPosition = new Vector3(Body.X, Body.Y, Body.Z);

                var pt = PathingMgr.Instance.GetRandomPointAsync(Body.CurrentZone, bodyPosition, Body.RoamingRange > 0 ? Body.RoamingRange : 200).Result;


                if (pt.HasValue)
                {
                    if (GS.ServerProperties.Properties.ENABLE_Pathig_DEBUG)
                        log.ErrorFormat("pathing pt value: {0}!! bodyPosition: {1} ", pt.Value.ToString(), bodyPosition);
                    // Vector3 bodyPositions = new Vector3(pt.Value.X, pt.Value.Y, pt.Value.Z);

                    //var bodyPosition2 = new Point(pt.X, pt.Y, pt.Z);

                    return point3D(pt.Value);
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

        #endregion
    }
}
