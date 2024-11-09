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
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;
using DOL.GS.Spells;
using DOL.Language;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace DOL.AI.Brain
{
    /// <summary>
    /// A brain that can be controlled
    /// </summary>
    public class ControlledNpcBrain : StandardMobBrain, IControlledBrain
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // note that a minimum distance is inforced in GameNPC
        public static readonly short MIN_OWNER_FOLLOW_DIST = 50;
        //4000 - rough guess, needs to be confirmed
        public static readonly short MAX_OWNER_FOLLOW_DIST = 5000; // setting this to max stick distance
        public static readonly short MIN_ENEMY_FOLLOW_DIST = 90;
        public static readonly short MAX_ENEMY_FOLLOW_DIST = 512;

        protected int m_tempX = 0;
        protected int m_tempY = 0;
        protected int m_tempZ = 0;
        public const string HAS_TARGET = "HasTarget";
        const string SpamTime = "NOCHARMEFFECTSPAM";
        private bool orderAttackTarget = false;
        private const string IsCasting = "IS_CASTING";


        /// <summary>
        /// Holds the controlling player of this brain
        /// </summary>
        protected readonly GameLiving m_owner;

        /// <summary>
        /// Holds the walk state of the brain
        /// </summary>
        protected eWalkState m_walkState;

        /// <summary>
        /// Holds the aggression level of the brain
        /// </summary>
        protected eAggressionState m_aggressionState;

        /// <summary>
        /// Allows to check if your target is stealthing - trying to escape your pet
        /// </summary>
        protected bool previousIsStealthed;


        /// <summary>
        /// Constructs new controlled npc brain
        /// </summary>
        /// <param name="owner"></param>
        public ControlledNpcBrain(GameLiving owner)
            : base()
        {
            m_owner = owner ?? throw new ArgumentNullException("owner was null");
            m_aggressionState = eAggressionState.Defensive;
            m_walkState = eWalkState.Follow;

            if (owner is GameNPC && (owner as GameNPC).Brain is StandardMobBrain)
            {
                m_aggroLevel = ((owner as GameNPC).Brain as StandardMobBrain).AggroLevel;
            }
            else
                m_aggroLevel = 99;
            m_aggroMaxRange = 1500;
        }


        protected bool m_isMainPet = true;

        /// <summary>
        /// Checks if this NPC is a permanent/charmed or timed pet
        /// </summary>
        public bool IsMainPet
        {
            get { return m_isMainPet; }
            set { m_isMainPet = value; }
        }

        /// <summary>
        /// The number of seconds/10 this brain will stay active even when no player is close
        /// Overriden. Returns int.MaxValue
        /// </summary>
        protected override int NoPlayersStopDelay
        {
            get { return int.MaxValue; }
        }

        /// <summary>
        /// The interval for thinking, set via server property, default is 1500 or every 1.5 seconds
        /// </summary>
        public override int ThinkInterval
        {
            get
            {
                if (Owner is GameNPC)
                    return DOL.GS.ServerProperties.Properties.NPC_MIN_THINKINTERVAL;
                else
                    return DOL.GS.ServerProperties.Properties.PET_MIN_THINKINTERVAL;

            }
        }


        #region Control

        /// <summary>
        /// Gets the controlling owner of the brain
        /// </summary>
        public GameLiving Owner
        {
            get { return m_owner; }
        }

        /// <summary>
        /// Find the player owner of the pets at the top of the tree
        /// </summary>
        /// <returns>Player owner at the top of the tree.  If there was no player, then return null.</returns>
        public virtual GamePlayer GetPlayerOwner()
        {
            GameLiving owner = Owner;
            int i = 0;
            while (owner is GameNPC && owner != null)
            {
                i++;
                if (i > 50)
                    throw new Exception("GetPlayerOwner() from " + Owner.Name + "caused a cyclical loop.");
                //If this is a pet, get its owner
                if (((GameNPC)owner).Brain is IControlledBrain)
                    owner = ((IControlledBrain)((GameNPC)owner).Brain).Owner;
                //This isn't a pet, that means it's at the top of the tree.  This case will only happen if
                //owner is not a GamePlayer
                else
                    break;
            }
            //Return if we found the gameplayer
            if (owner is GamePlayer)
                return (GamePlayer)owner;
            //If the root owner was not a player or npc then make sure we know that something went wrong!
            if (!(owner is GameNPC))
                throw new Exception("Unrecognized owner: " + owner.GetType().FullName);
            //No GamePlayer at the top of the tree
            return null;
        }

        public virtual GameNPC GetNPCOwner()
        {
            if (!(Owner is GameNPC))
                return null;

            GameNPC owner = Owner as GameNPC;

            int i = 0;
            while (owner != null)
            {
                i++;
                if (i > 50)
                {

                    break;
                }
                if (owner.Brain is IControlledBrain)
                {
                    if ((owner.Brain as IControlledBrain).Owner is GamePlayer)
                        return null;
                    else
                        owner = (owner.Brain as IControlledBrain).Owner as GameNPC;
                }
                else
                    break;
            }
            return owner;
        }

        public virtual GameLiving GetLivingOwner()
        {
            GamePlayer player = null;

            if (GetPlayerOwner() is GamePlayer)
                player = GetPlayerOwner();

            if (player != null)
                return player;

            GameNPC npc = GetNPCOwner();
            if (npc != null)
                return npc;

            return null;
        }

        /// <summary>
        /// Gets or sets the walk state of the brain
        /// </summary>
        public virtual eWalkState WalkState
        {
            get { return m_walkState; }
            set
            {
                m_walkState = value;
                UpdatePetWindow();
            }
        }

        /// <summary>
        /// Gets or sets the aggression state of the brain
        /// </summary>
        public virtual eAggressionState AggressionState
        {
            get { return m_aggressionState; }
            set
            {
                m_aggressionState = value;
                m_orderAttackTarget = null;
                if (m_aggressionState == eAggressionState.Passive)
                {
                    ClearAggroList();
                    if (Body.IsAttacking)
                    {
                        Body.StopAttack();
                    }
                    Body.TargetObject = null;
                    WalkState = eWalkState.Follow;

                    UpdatePetWindow();
                    if (WalkState == eWalkState.Follow && Body.Brain is TheurgistPetBrain == false && Body.AttackState == false)
                    {
                        FollowOwner();
                    }
                    if (WalkState == eWalkState.ComeHere && Body.Brain is TheurgistPetBrain == false)
                    {
                        FollowOwner();
                    }
                }
                if (m_aggressionState == eAggressionState.Defensive)
                {

                    if (Body.Brain is TheurgistPetBrain == false && Body.IsAttacking == false && Owner.InCombat == false && Owner.IsAttacking == false && WalkState == eWalkState.Stay == false && WalkState == eWalkState.GoTarget == false)
                    {
                        ClearAggroList();
                        if (Body.IsAttacking)
                        {
                            Body.StopAttack();
                        }
                        Body.TargetObject = null;
                        FollowOwner();
                    }
                    else if (Owner.IsAttacking || Owner.IsCasting || Owner.InCombat || Owner.IsStunned || Owner.IsMezzed || Owner.IsStunned)
                    {
                        //log.Error("Attack ordered!!");
                        AttackMostWanted();
                    }
                    if (m_aggressionState == eAggressionState.Aggressive)
                    {
                          AttackMostWanted();
           
                    }
                }
            }
        }
        
        /// <summary>
        /// Attack the target on command
        /// </summary>
        /// <param name="target"></param>
        public virtual void Attack(GameObject target)
        {

            if (target is GameKeepComponent)
                return;
           
            if (PetAllowedToAttackFoolow(target) == false)
            {
                return;
            }

            if (WalkState == eWalkState.Stay)
            {
                WalkState = eWalkState.GoTarget;
            }

            if (AggressionState == eAggressionState.Passive)
            {
                AggressionState = eAggressionState.Defensive;
                UpdatePetWindow();
            }

            m_orderAttackTarget = target as GameLiving;
            previousIsStealthed = false;


            if (target is GamePlayer)
            {
                previousIsStealthed = (target as GamePlayer).IsStealthed;
            }
            Body.letAttack = true;
            StartAttackTargetByPet();
           
        }

        /// <summary>
        /// Follow the target on command
        /// </summary>
        /// <param name="target"></param>
        public virtual void Follow(GameObject target)
        {
            if (Body is NecromancerPet)
            {
                WalkState = eWalkState.Follow;
            }
            else
            WalkState = eWalkState.GoTarget;

            //Body.WalkTo(new Point3D(target.X, target.Y, target.Z), Body.MaxSpeed);
            Body.Follow(target, MIN_OWNER_FOLLOW_DIST, MAX_OWNER_FOLLOW_DIST);

        }

        /// <summary>
        /// Stay at current position on command
        /// </summary>
        public virtual void Stay()
        {
            m_tempX = Body.X;
            m_tempY = Body.Y;
            m_tempZ = Body.Z;
            WalkState = eWalkState.Stay;
            Body.StopFollowing();

        }

        /// <summary>
        /// Go to owner on command
        /// </summary>
        public virtual void ComeHere()
        {

            WalkState = eWalkState.ComeHere;

            if (Body.IsAttacking)
            {
                Body.StopAttack();
            }
            Body.StopFollowing();
            Body.TargetObject = null;

            Body.PathTo(Owner.X, Owner.Y, Owner.Z, Body.MaxSpeed);

            Body.Heading = Owner.Heading;
            //Body.TurnTo(Body.GetHeading(Owner));

        }

        
        /// <summary>
        /// By Elcotek: Pets and NPCs don't walk/attack highter tower positions if the Tower is closed
        /// </summary>
        /// <param name="target"></param>
        /// <returns>true/false</returns>
        public virtual bool PetAllowedToAttackFoolow(GameObject target)
        {

            if (target != null && target != null && target is GameKeepDoor == false && target is GameDoor == false)
            {


                foreach (IDoor door in Body.GetDoorsInRadius(200))
                {
                    if (door != null && door is GameKeepDoor)
                    {
                        if (door.State == eDoorState.Closed && door.Realm != Body.Realm)
                        {
                                                       
                            orderAttackTarget = false;
                            WalkState = eWalkState.ComeHere;
                            UpdatePetWindow();
                            return false;

                        }
                    }
                }
            }

            return true;
        }
      


        /// <summary>
        /// By Elcotek: Pets and NPCs don't walk/attack highter Keep/tower positions
        /// </summary>
        /// <param name="target"></param>
        /// <returns>true/false</returns>
        public virtual bool PetAllowedToAttack(GameObject target)
        {
            if (target != null)
            {
                ICollection<AbstractGameKeep> keepList = KeepMgr.GetNFKeeps();
                foreach (AbstractGameKeep keep in keepList)
                {
                    if (keep is GameKeep || keep is GameKeepTower)
                    {
                        foreach (GameKeepComponent keepComponent in keep.KeepComponents)
                        {
                            if (target.IsWithinRadius(keepComponent, 1200) && keepComponent is GameKeepComponent && keepComponent != null)
                            {

                                if ((target is GameNPC || target is GamePlayer) && Body.Z < target.Z - 100 && target.Realm != Body.Realm)
                                {

                                    orderAttackTarget = false;
                                    WalkState = eWalkState.ComeHere;
                                    UpdatePetWindow();
                                    return false;

                                }
                            }
                        }
                    }
                }
               
            }
            return true;
        }


        /// <summary>
        /// Go to targets location on command
        /// </summary>
        /// <param name="target"></param>
        public virtual void Goto(GameObject target)
        {
            //if (Body.InCombat == false)
           // {
                //If the Player is riding
                if (Body.MaxSpeedBase == 5000)
                {
                    Body.MaxSpeedBase = 248;
                }
           
            if (PetAllowedToAttackFoolow(target) == false)
            {
               
                return;
            }
                

                if (Owner.TargetInView == false)
                {
                    (Owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language, "ArrowSpellHandler.YourTargetIsNotInView"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    return;
                }

                //if (PetAllowedToAttack(target))
                {
                    m_tempX = Body.X;
                    m_tempY = Body.Y;
                    m_tempZ = Body.Z;
                    WalkState = eWalkState.GoTarget;
                    Body.StopFollowing();

                    if (!Body.IsWithinRadius(target, 90))// && Body.AllowedToAttackTarget())
                    {
                        //Body.WalkTo(target, Body.MaxSpeed);
                        Body.PathTo(target.X, target.Y, target.Z, Body.MaxSpeed);
                    }
               
            }
        }


        public virtual void SetAggressionState(eAggressionState state)
        {
            AggressionState = state;
            UpdatePetWindow();
        }

        /// <summary>
        /// Updates the pet window
        /// </summary>
        public virtual void UpdatePetWindow()
        {

            if (m_owner is GamePlayer && m_body != null && m_body.Name != "Brittle Guard" && m_body.Name != "Crystal Titan")
                ((GamePlayer)m_owner).Out.SendPetWindow(m_body, ePetWindowAction.Update, m_aggressionState, m_walkState);

        }
        bool returnToOwner = false;
        /// <summary>
        /// Start following the owner
        /// </summary>
        public virtual void FollowOwner()
        {

            if (Body.ObjectState == GameObject.eObjectState.Active)
            {
                //When the NecromancerPet SpellQueued is > 0 
                if (Body is NecromancerPet && Body.TempProperties.getProperty<bool>(GameNPC.SpellQueued) && m_aggroTable.Count > 0)
                {
                    if (m_aggroTable.Count == 1 && Body.TargetObject != null && Body.TargetObject.IsObjectAlive == false)
                    {

                    }
                    else
                        return;
                }

                if (WalkState != eWalkState.Stay || WalkState == eWalkState.Follow)
                {

                    if (Body.IsAttacking)
                        Body.StopAttack();
                    if (Owner is GamePlayer
                        && IsMainPet
                        && ((GamePlayer)Owner).IsCharcterClass(eCharacterClass.Animist) == false
                        && ((GamePlayer)Owner).IsCharcterClass(eCharacterClass.Theurgist) == false)
                    {
                        Body.Follow(Owner, MIN_OWNER_FOLLOW_DIST, MAX_OWNER_FOLLOW_DIST);

                    }
                    else if (Owner is GameNPC)
                        Body.Follow(Owner, MIN_OWNER_FOLLOW_DIST, MAX_OWNER_FOLLOW_DIST);

                }
                returnToOwner = true;
            }
        }

        #endregion

        #region AI

        /// <summary>
        /// The attack target ordered by the owner
        /// </summary>
        protected GameLiving m_orderAttackTarget;

        /// <summary>
        /// Starts the brain thinking and resets the inactivity countdown
        /// </summary>
        /// <returns>true if started</returns>
        public override bool Start()
        {
            if (!base.Start()) return false;

            if (Body.ObjectState == GameObject.eObjectState.Active)
            {
                if (WalkState == eWalkState.Follow && Body.Brain is TheurgistPetBrain == false)
                    FollowOwner();

                // [Ganrod] On supprime la cible du pet au moment  du contrôle.
                Body.TargetObject = null;
                GameEventMgr.AddHandler(Body, GameLivingEvent.EnemyKilled, new DOLEventHandler(PetAttackFinish));
                GameEventMgr.AddHandler(Owner, GameLivingEvent.EnemyKilled, new DOLEventHandler(PetAttackFinish));

                GameEventMgr.AddHandler(Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnOwnerAttacked));
                GameEventMgr.AddHandler(Owner, GameLivingEvent.CastFinished, new DOLEventHandler(OnAttackByOwner));




               

                if (Body.Brain is BDArcherBrain || Body.Brain is BDBufferBrain || Body.Brain is BDCasterBrain || Body.Brain is BDDebufferBrain
                    || Body.Brain is BDHealerBrain || Body.Brain is BDMeleeBrain)
                {
                   

                    GameEventMgr.AddHandler(Body, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttackSubPets));
                }


                                return true;
            }
            return false;
        }

        /// <summary>
        /// Stops the brain thinking
        /// </summary>
        /// <returns>true if stopped</returns>
        public override bool Stop()
        {
            if (!base.Stop()) return false;
            GameEventMgr.RemoveHandler(Body, GameLivingEvent.EnemyKilled, new DOLEventHandler(PetAttackFinish));
            GameEventMgr.RemoveHandler(Owner, GameLivingEvent.EnemyKilled, new DOLEventHandler(PetAttackFinish));


            GameEventMgr.RemoveHandler(Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnOwnerAttacked));
            GameEventMgr.RemoveHandler(Owner, GameLivingEvent.CastFinished, new DOLEventHandler(OnAttackByOwner));
            GameEventMgr.Notify(GameLivingEvent.PetReleased, Body);

            if (Body.Brain is BDArcherBrain || Body.Brain is BDBufferBrain || Body.Brain is BDCasterBrain || Body.Brain is BDDebufferBrain
                   || Body.Brain is BDHealerBrain || Body.Brain is BDMeleeBrain)
            {
               GameEventMgr.RemoveHandler(Body, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttackSubPets));
            }
           

            return true;
        }

        /// <summary>
        ///Right Animation for Pet and Owner
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effect"></param>
        /// <param name="owner"></param>
        protected virtual void SendEffectAnimations(GameLiving target, GameLiving owner)
        {
            foreach (GamePlayer player in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                player.Out.SendSpellEffectAnimation(Owner, target, 1156, 0, false, 1);

            }
        }

        public virtual void StartAttackTargetByPet()
        {
            if (Body.letAttack)
            AttackMostWanted();
            
           

        }
        /// <summary>
        /// Do the mob AI
        /// </summary>
        public override void Think()
        {
            GamePlayer playerowner = GetPlayerOwner();
            GameLiving target = Owner.TargetObject as GameLiving;
                       

            if (target is GameKeepComponent)
                return;

            if (m_orderAttackTarget != null)// && Body.IsWithinRadius(m_orderAttackTarget, 500))
            {
                CheckSpells(eCheckSpellType.Offensive);
                CheckSpells(eCheckSpellType.Defensive);

                //log.Error("starte attacke!!");
                StartAttackTargetByPet();
            }

            if (PetAllowedToAttackFoolow(target) == false)
            {
               FollowOwner();
            }
            //Charm effect on Body for none pulse
            if (Body != null)
            {
                GameSpellEffect charm = SpellHandler.FindEffectOnTarget(Body, "Charm");

                if (Owner != null && charm != null && charm.Spell.Pulse == 0)
                {
                    long UPDATETICK = Body.TempProperties.getProperty<long>(SpamTime);
                    long changeTime = Body.CurrentRegion.Time - UPDATETICK;

                    if (changeTime > 4 * 1000 || UPDATETICK == 0)
                    {
                        Body.TempProperties.setProperty(SpamTime, Body.CurrentRegion.Time);
                        SendEffectAnimations(Body, Owner);
                    }
                }
            }


            if (m_aggressionState == eAggressionState.Passive && Body.Brain is TheurgistPetBrain == false)
            {
                orderAttackTarget = false;
                WalkState = eWalkState.Follow;

                UpdatePetWindow();
            }



            if (!playerowner.Client.GameObjectUpdateArray.TryGetValue(new Tuple<ushort, ushort>(Body.CurrentRegionID, (ushort)Body.ObjectID), out long lastUpdate))
                lastUpdate = 0;

            if (playerowner != null && (GameTimer.GetTickCount() - lastUpdate) > ThinkInterval)
            {
                playerowner.Out.SendObjectUpdate(Body);
            }

            else if ((Body.IsAlive && Body.IsMezzed == false && Body.IsStunned == false && Body.IsConfused == false
                && m_aggressionState == eAggressionState.Defensive) && (Owner.IsMezzed || Owner.IsStunned || Owner.IsDiseased || Owner.IsSilenced))
            {
              
                    AttackMostWanted();
            }
            if (m_aggressionState == eAggressionState.Defensive && Body.Brain is TheurgistPetBrain == false && Body.ObjectState == GameObject.eObjectState.Active)
            {


                if (Body.IsCasting == false && Owner.IsCasting == false && Body.IsAttacking == false
                     && Owner.InCombat == false && Owner.IsAttacking == false && WalkState == eWalkState.Stay == false
                     && Owner.IsMezzed == false && Owner.IsStunned == false && Owner.IsDiseased == false
                     && Owner.IsSilenced == false && WalkState == eWalkState.GoTarget == false && Body.letAttack == false)
                {
                    ClearAggroList();
                    if (Body.IsAttacking)
                    {
                        Body.StopAttack();
                    }
                    Body.TargetObject = null;

                    FollowOwner();
                }
            }
            //See if the pet is too far away, if so release it!
            if (Owner is GamePlayer && IsMainPet)
            {
                if (!Body.IsWithinRadius(Owner, MAX_OWNER_FOLLOW_DIST) && this is NecromancerPetBrain == false)
                    (Owner as GamePlayer).CommandNpcRelease();
            }

            if (WalkState == eWalkState.ComeHere && Body.Brain is TheurgistPetBrain == false)
            {


                if (Body.IsWithinRadius(Owner, MIN_OWNER_FOLLOW_DIST) == false)
                {
                    AggressionState = eAggressionState.Passive;
                }
                WalkState = eWalkState.Follow;
                AggressionState = eAggressionState.Defensive;
                UpdatePetWindow();
                FollowOwner();
            }
            if (Body.TargetObject != null && Body.TargetObject.IsObjectAlive == false && AggressionState == eAggressionState.Defensive && orderAttackTarget == false && (WalkState != eWalkState.Stay || Body.InCombat == false && WalkState == eWalkState.Follow || Body.InCombat == false && WalkState == eWalkState.GoTarget) && WalkState != eWalkState.ComeHere)
            {

                if (WalkState != eWalkState.Follow)
                {
                    WalkState = eWalkState.Follow;
                    UpdatePetWindow();

                }
            }
            // if pet is in agressive mode then check aggressive spells and attacks first
            if (AggressionState == eAggressionState.Aggressive && WalkState == eWalkState.ComeHere == false)
            {
              
                
                CheckPlayerAggro();
                CheckNPCAggro();
                               
                  AttackMostWanted();
                    
            }


            if (Body.IsCasting == false && Body.IsAttacking == false && WalkState == eWalkState.ComeHere == false)
            {
                // Check for buffs, heals, etc
                // Only prevent casting if we are ordering pet to come to us or go to target
                if (Owner is GameNPC || (Owner is GamePlayer && WalkState != eWalkState.ComeHere && WalkState != eWalkState.GoTarget))
                {
                    CheckSpells(eCheckSpellType.Defensive);
                }
            }

            // Stop hunting player entering in steath
            if (Body.TargetObject != null && Body.TargetObject is GamePlayer)
            {
                GamePlayer player = Body.TargetObject as GamePlayer;
                if (Body.IsAttacking && player.IsStealthed && !previousIsStealthed)
                {
                    if (Body.IsAttacking)
                    {
                        Body.StopAttack();
                    }
                    Body.StopCurrentSpellcast();
                    RemoveFromAggroList(player);
                    Body.TargetObject = null;
                    FollowOwner();

                }
                previousIsStealthed = player.IsStealthed;
            }

        }

        /// <summary>
        /// Checks the Abilities
        /// </summary>
        public override void CheckAbilities()
        {
            ////load up abilities
            if (Body.Abilities != null && Body.Abilities.Count > 0)
            {
                foreach (Ability ab in Body.Abilities.Values)
                {
                    switch (ab.KeyName)
                    {
                        case GS.Abilities.Intercept:
                            {
                                if (Body.IsMezzed == false && Body.IsStunned == false && Body.EffectList.GetOfType<InterceptEffect>() == null)
                                {
                                    GamePlayer player = Owner as GamePlayer;
                                    //the pet should intercept even if a player is till intercepting for the owner
                                    new InterceptEffect().Start(Body, player);
                                }
                                break;
                            }
                        case GS.Abilities.Guard:
                            {
                                if (Body.IsMezzed == false && Body.IsStunned == false && Body.EffectList.GetOfType<GuardEffect>() == null)
                                {
                                    GamePlayer player = Owner as GamePlayer;
                                    new GuardEffect().Start(Body, player);
                                }
                                break;
                            }
                        case Abilities.ChargeAbility:
                            {
                                if (Body.IsMezzed == false && Body.IsStunned == false && !Body.IsWithinRadius(Body.TargetObject, 500))
                                {
                                    ChargeAbility charge = Body.GetAbility<ChargeAbility>();
                                    if (charge != null && Body.GetSkillDisabledDuration(charge) <= 0)
                                    {
                                        charge.Execute(Body);
                                    }
                                }
                                break;
                            }
                    }
                }
            }
        }


        private GameLiving living = null;


        /// <summary>
        /// Checks if any spells need casting
        /// </summary>
        /// <param name="type">Which type should we go through and check for?</param>
        /// <returns></returns>
        public override bool CheckSpells(eCheckSpellType type)
        {
            int spRange = 0;



            bool casted = false;


            if (Body.IsCasting && Body.TempProperties.getProperty<bool>(IsCasting) == true)//Body.IsCasting)
            {

                return true;

            }
            else
            {

                Body.TempProperties.removeProperty(IsCasting);
            }



            // if (Body is TheurgistPet == false)



            //By elcotek Mob cast only if spell not active on Target
            if (Body.TargetObject != null)
            {
                living = Body.TargetObject as GameLiving;
            }


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
                        if (spell == null)
                        {
                            Body.TempProperties.setProperty(IsCasting, false);
                            continue;
                        }
                        Body.TempProperties.setProperty(IsCasting, false);

                        if (Body.GetSkillDisabledDuration(spell) > 0 || SpellHandler.FindEffectAndImmunityOnTarget(living, spell.SpellType) != null && spell.SpellType.ToLower().Contains("summon") == false && spell.SpellType.ToLower().Contains("SummonMobPet") == false)
                        {
                            Body.TempProperties.setProperty(IsCasting, false);
                            continue;
                        }
                        if (spell.Target.ToLower() == "enemy" || spell.Target.ToLower() == "area" || spell.Target.ToLower() == "cone")
                        {
                            Body.TempProperties.setProperty(IsCasting, false);
                            continue;
                        }

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
                            if (tire.Uninterruptible && CheckDefensiveSpells(tire) || CheckDefensiveSpells(tire) && tire.CastTime == 0)
                            {
                                casted = true;
                                Body.TempProperties.setProperty(IsCasting, false);

                            }
                            else

                                if (!Body.IsBeingInterrupted && CheckDefensiveSpells(tire) || CheckDefensiveSpells(tire) && tire.CastTime == 0)
                            {
                                casted = true;
                                Body.TempProperties.setProperty(IsCasting, false);

                            }
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
                    SpellLine spellLine = null;
                    if (Body is TheurgistPet == false)


                        foreach (Spell spell in Body.Spells)
                        {
                            //We allow casting only if there is not already the same type of spell and immunity active on target
                            if (living != null && Body.GetSkillDisabledDuration(spell) == 0 && SpellHandler.FindEffectAndImmunityOnTarget(living, spell.SpellType) == null || spell.SpellType.ToLower().Contains("DamageSpeedDecrease") || spell.SpellType.ToLower().Contains("DirectDamageWithDebuff"))
                            {
                                //Calculate correct Spell Range
                                if (spell != null)
                                {
                                    if (spellLine == null)
                                    {
                                        spellLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells);
                                    }
                                    if (spellLine != null)
                                    {
                                        ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(Body, spell, spellLine);

                                        if (spellHandler != null)
                                        {
                                            spRange = spellHandler.CalculateSpellRange();
                                        }
                                    }
                                    if (spRange == 0)
                                    {

                                        spRange = spell.Range;
                                    }
                                }


                                if (spell.CastTime > 0 && !Body.IsBeingInterrupted && Body.IsWithinRadius(Body.TargetObject, spRange)
                                    || spell.CastTime == 0 && Body.TargetObject != null)
                                {
                                    int Cast = Util.Random(1, 6);
                                    if (spell.CastTime == 0 && Cast != 3) continue;

                                    if (spell.Target.ToLower() == "enemy" || spell.Target.ToLower() == "area" || spell.Target.ToLower() == "cone")
                                        spell_rec.Add(spell);

                                    if (spell.CastTime > 0 && spell.Target.ToLower() == "enemy")
                                    {
                                        if (Body.TargetObject != null && Body is TheurgistPet == false)
                                        {
                                            Body.Follow(Body.TargetObject, spRange - 100, spRange);
                                        }
                                    }

                                }
                            }
                        }
                    if (spell_rec.Count > 0)
                    {
                        tire = (Spell)spell_rec[Util.Random((spell_rec.Count - 1))];

                        if (CheckDefensiveSpells(tire) && tire.CastTime == 0)
                        {
                            Body.TempProperties.setProperty(IsCasting, false);
                        }
                        else
                            Body.TempProperties.setProperty(IsCasting, true);


                        if (tire.Uninterruptible && CheckOffensiveSpells(tire) || CheckDefensiveSpells(tire) && tire.CastTime == 0)
                            casted = true;
                        else
                        if ((Body.IsWithinRadius(Body.TargetObject, 400) && CheckOffensiveSpells(tire)
                                || !Body.IsWithinRadius(Body.TargetObject, 400) && (!Body.IsBeingInterrupted) && CheckOffensiveSpells(tire) || CheckDefensiveSpells(tire) && tire.CastTime == 0))
                            casted = true;
                    }
                }


            }
            return casted;
        }

        /// <summary>
        /// Checks the Positive Spells.  Handles buffs, heals, etc.
        /// </summary>
        protected override bool CheckDefensiveSpells(Spell spell)
        {
            GameObject lastTarget = Body.TargetObject;
            Body.TargetObject = null;
            GamePlayer player = null;
            GameLiving owner = null;

            // clear current target, set target based on spell type, cast spell, return target to original target

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
                        //Buff self
                        if (!LivingHasEffect(Body, spell))
                        {
                            Body.TargetObject = Body;
                            break;
                        }

                        if (spell.Target == "Realm" || spell.Target == "Group")
                        {
                            owner = (this as IControlledBrain).Owner;
                            player = null;
                            //Buff owner
                            if (!LivingHasEffect(owner, spell))
                            {
                                Body.TargetObject = owner;
                                break;
                            }

                            if (owner is GameNPC)
                            {
                                //Buff other minions
                                foreach (IControlledBrain icb in ((GameNPC)owner).ControlledNpcList)
                                {
                                    if (icb == null)
                                        continue;
                                    if (!LivingHasEffect(icb.Body, spell))
                                    {
                                        Body.TargetObject = icb.Body;
                                        break;
                                    }
                                }
                            }

                            player = GetPlayerOwner();

                            //Buff player
                            if (player != null)
                            {
                                if (!LivingHasEffect(player, spell))
                                {
                                    Body.TargetObject = player;
                                    break;
                                }

                                if (player.Group != null)
                                {
                                    foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
                                    {
                                        if (!LivingHasEffect(p, spell) && Body.GetDistanceTo(p) <= spell.Range)
                                        {
                                            Body.TargetObject = p;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                #endregion Buffs

                #region Disease Cure/Poison Cure/Summon
                case "CureDisease":
                    //Cure self
                    if (Body.IsDiseased)
                    {
                        Body.TargetObject = Body;
                        break;
                    }

                    //Cure owner
                    owner = (this as IControlledBrain).Owner;
                    if (owner.IsDiseased)
                    {
                        Body.TargetObject = owner;
                        break;
                    }

                    // Cure group members

                    player = GetPlayerOwner();

                    if (player.Group != null)
                    {
                        foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
                        {
                            if (p.IsDiseased && Body.GetDistanceTo(p) <= spell.Range)
                            {
                                Body.TargetObject = p;
                                break;
                            }
                        }
                    }
                    break;
                case "CurePoison":
                    //Cure self
                    if (LivingIsPoisoned(Body))
                    {
                        Body.TargetObject = Body;
                        break;
                    }

                    //Cure owner
                    owner = (this as IControlledBrain).Owner;
                    if (LivingIsPoisoned(owner))
                    {
                        Body.TargetObject = owner;
                        break;
                    }

                    // Cure group members

                    player = GetPlayerOwner();

                    if (player.Group != null)
                    {
                        foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
                        {
                            if (LivingIsPoisoned(p) && Body.GetDistanceTo(p) <= spell.Range)
                            {
                                Body.TargetObject = p;
                                break;
                            }
                        }
                    }
                    break;
                case "Summon":
                    Body.TargetObject = Body;
                    break;
                #endregion

                #region Heals
                case "Heal":
                    if (spell.Target.ToLower() == "self")
                    {
                        // if we have a self heal and health is less than 75% then heal, otherwise return false to try another spell or do nothing
                        if (Body.HealthPercent < 75)
                        {
                            Body.TargetObject = Body;
                        }
                        break;
                    }

                    //Heal self
                    if (Body.HealthPercent < 75)
                    {
                        Body.TargetObject = Body;
                        break;
                    }

                    //Heal owner
                    owner = (this as IControlledBrain).Owner;
                    if (owner.HealthPercent < 75)
                    {
                        Body.TargetObject = owner;
                        break;
                    }

                    player = GetPlayerOwner();

                    if (player.Group != null && (spell.Target.ToLower() == "realm" || spell.Target.ToLower() == "group"))
                    {
                        foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
                        {
                            if (p.HealthPercent < 75 && Body.GetDistanceTo(p) <= spell.Range)
                            {
                                Body.TargetObject = p;
                                break;
                            }
                        }
                    }
                    break;
                    #endregion
            }

            if (Body.TargetObject != null && Body.TargetObject.IsObjectAlive)
            {
                if (Body.IsMoving && CheckOffensiveSpells(spell))
                    Body.StopFollowing();

                if (Body.TargetObject != Body && spell.CastTime > 0)

                    if (Body.TempProperties.getProperty<bool>(HAS_TARGET, false))
                    {
                        Body.TempProperties.setProperty(HAS_TARGET, true);

                        Body.TurnTo(Body.TargetObject);
                    }

                Body.CastSpell(spell, m_mobSpellLine);
                Body.TargetObject = lastTarget;
                return true;
            }

            Body.TargetObject = lastTarget;

            return false;
        }

        /// <summary>
        /// Lost follow target event
        /// </summary>
        /// <param name="target"></param>
        protected override void OnFollowLostTarget(GameObject target)
        {
            if (target == Owner && this is NecromancerPetBrain == false)
            {
                GameEventMgr.Notify(GameLivingEvent.PetReleased, Body);
                return;
            }
            FollowOwner();
        }

        /// <summary>
        /// Add living to the aggrolist
        /// aggroamount can be negative to lower amount of aggro
        /// </summary>
        /// <param name="living"></param>
        /// <param name="aggroamount"></param>
        public override void AddToAggroList(GameLiving living, int aggroamount)
        {
            GameNPC npc_owner = GetNPCOwner();
            if (npc_owner == null || !(npc_owner.Brain is StandardMobBrain))
                base.AddToAggroList(living, aggroamount, true);
            else
            {
                (npc_owner.Brain as StandardMobBrain).AddToAggroList(living, aggroamount, true);
            }
        }

        public override int CalculateAggroLevelToTarget(GameLiving target)
        {
            // only attack if target is green+ to OWNER; always attack higher levels regardless of CON
            if (GameServer.ServerRules.IsAllowedToAttack(Body, target, true) == false)
                return 0;

            if (target.IsObjectGreyCon(Body))
                return 0; // only attack if green+ to target

            return AggroLevel > 100 ? 100 : AggroLevel;
        }

        /// <summary>
        /// Returns the best target to attack
        /// </summary>
        /// <returns>the best target</returns>
        protected override GameLiving CalculateNextAttackTarget()
        {
            if (AggressionState == eAggressionState.Passive)
                return null;

            if (Body is NecromancerPet && m_orderAttackTarget == null &&  AggressionState == eAggressionState.Defensive)
                return null;

                if (Body is TheurgistPet)
                return null;

            if (m_orderAttackTarget != null)
            {
                if (m_orderAttackTarget.IsAlive &&
                    m_orderAttackTarget.ObjectState == GameObject.eObjectState.Active &&
                    GameServer.ServerRules.IsAllowedToAttack(this.Body, m_orderAttackTarget, true))
                {
                  
                    orderAttackTarget = true;
                    return m_orderAttackTarget;
                   
                }

                m_orderAttackTarget = null;
            }
          

            lock ((m_aggroTable as ICollection).SyncRoot)
            {
                IDictionaryEnumerator aggros = m_aggroTable.GetEnumerator();
                List<GameLiving> removable = new List<GameLiving>();
                while (aggros.MoveNext())
                {
                    GameLiving living = (GameLiving)aggros.Key;

                    //log.ErrorFormat("Mob gefunden: {0}", living.Name);

                    if (living.IsMezzed ||
                        living.IsAlive == false ||
                        living.ObjectState != GameObject.eObjectState.Active ||
                        Body.GetDistanceTo(living, 0) > MAX_AGGRO_LIST_DISTANCE ||
                        GameServer.ServerRules.IsAllowedToAttack(this.Body, living, true) == false)
                    {
                        //log.ErrorFormat("Mob gefunden222: {0}", living.Name);
                        removable.Add(living);
                    }
                    else
                    {
                        GameSpellEffect root = SpellHandler.FindEffectOnTarget(living, "SpeedDecrease");
                        if (root != null && root.Spell.Value == 99)
                        {
                            removable.Add(living);
                        }
                    }
                }

                foreach (GameLiving living in removable)
                {
                    RemoveFromAggroList(living);
                    Body.RemoveAttacker(living);

                }
            }

            return base.CalculateNextAttackTarget();
        }

        /// <summary>
        /// Selects and attacks the next target or does nothing
        /// </summary>
        protected override void AttackMostWanted()
        {
            if (!IsActive) return;

            if (m_aggressionState == eAggressionState.Passive)
                return;
            
            //Check abilities is now in GameNPC (Start Attack)

            GameNPC owner_npc = GetNPCOwner();
            if (owner_npc != null && owner_npc.Brain is StandardMobBrain && WalkState == eWalkState.ComeHere == false)
            {

                if ((owner_npc.IsCasting && owner_npc.TargetObject != null || owner_npc.IsAttacking) &&
                    owner_npc.TargetObject != null &&
                    owner_npc.TargetObject is GameLiving && owner_npc.TargetObject.IsObjectAlive &&
                    GameServer.ServerRules.IsAllowedToAttack(owner_npc, owner_npc.TargetObject as GameLiving, false))
                {
                    if (Body.TempProperties.getProperty<bool>(IsCasting) == false && PetAllowedToAttack(owner_npc.TargetObject))
                    {
                        Body.StartAttack(owner_npc.TargetObject);
                        Body.TempProperties.removeProperty(IsCasting);
                        //log.Error("pet attackiert!!");
                    }
                    return;
                }
            }

            GameLiving target = CalculateNextAttackTarget();
           

            if (target != null && target.IsObjectAlive && AggressionState != eAggressionState.Passive)
            {
               
                if (!Body.IsAttacking || target != Body.TargetObject)
                {
                    Body.TargetObject = target;

                   

                    List<GameSpellEffect> effects = new List<GameSpellEffect>();

                    //Pets Speed effects remove wehn in combat
                    if (Body.InCombat && target.InCombat)
                    {

                        if (target is GamePlayer)
                        {
                            Body.LastAttackTickPvP = Body.CurrentRegion.Time;
                            Owner.LastAttackedByEnemyTickPvP = Body.CurrentRegion.Time;
                        }
                        else
                        {
                            Body.LastAttackTickPvE = Body.CurrentRegion.Time;
                            Owner.LastAttackedByEnemyTickPvE = Body.CurrentRegion.Time;
                        }
                   
                      
                        lock (Body.EffectList)
                        {
                            foreach (IGameEffect effect in Body.EffectList)
                            {
                                if (effect is GameSpellEffect && (effect as GameSpellEffect).SpellHandler is SpeedEnhancementSpellHandler)
                                {
                                    effects.Add(effect as GameSpellEffect);
                                }
                            }
                        }
                        
                       
                        foreach (GameSpellEffect effect in effects)
                        {
                            effect.Cancel(false);
                        }
                        
                    }
                    //log.ErrorFormat("castet0 = {0}", Body.TempProperties.getProperty<bool>(IsCasting));
                    if (Body.TempProperties.getProperty<bool>(IsCasting) == false && PetAllowedToAttack(target)) // && PetAllowedToAttackDoor(target))
                    {
                        
                        Body.StartAttack(target);
                        Body.TempProperties.removeProperty(IsCasting);
                    }
                }
            }

            else
            {
                if (Body.TargetObject != null && m_aggroTable.ContainsKey((GameLiving)Body.TargetObject) && Body.TargetObject.IsObjectAlive == false)
                {
                    RemoveFromAggroList((GameLiving)Body.TargetObject);

                }

                if (Body.SpellTimer != null && Body.SpellTimer.IsAlive)
                {
                    Body.SpellTimer.Stop();
                }

                // if (Body.SpellTimer != null && Body.SpellTimer.IsAlive)
                //  Body.SpellTimer.Stop();

                if (WalkState != eWalkState.Stay && m_aggroTable.Count < 1)
                {


                    //Folge Player nach Kampf
                    if (Owner is GamePlayer)
                    {


                        //Player is owner
                        ClearAggroList();
                        Body.TargetObject = null;
                        Body.TempProperties.setProperty(HAS_TARGET, false);
                        while (Body.TargetObject == null && returnToOwner == false)
                        {
                            FollowOwner();
                            break;
                        }
                    }
                    else

                    {
                        //Npc is Owner
                        FollowOwner();
                        Body.TempProperties.setProperty(HAS_TARGET, false);
                    }
                }
            }
        }

        /// <summary>
        /// Owner attacked event
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        protected virtual void OnOwnerAttacked(DOLEvent e, object sender, EventArgs arguments)
        {
            // theurgist pets don't help their owner
            //edit for BD - possibly add support for Theurgist GameNPCs
            if (Owner is GamePlayer && ((GamePlayer)Owner).IsCharcterClass(eCharacterClass.Theurgist))
                return;

            if (AggressionState == eAggressionState.Passive)
            {
                return;
            }
            if (Body.IsAttacking || WalkState == eWalkState.Stay && orderAttackTarget == false)
            {
                return;
            }
            AttackedByEnemyEventArgs args = arguments as AttackedByEnemyEventArgs;
            if (args == null) return;
            if (args.AttackData.Target is GamePlayer && (args.AttackData.Target as GamePlayer).ControlledBrain != this)
                return;
            // react only on these attack results
            switch (args.AttackData.AttackResult)
            {
                case GameLiving.eAttackResult.Blocked:
                case GameLiving.eAttackResult.Evaded:
                case GameLiving.eAttackResult.Fumbled:
                case GameLiving.eAttackResult.HitStyle:
                case GameLiving.eAttackResult.HitUnstyled:
                case GameLiving.eAttackResult.Missed:
                case GameLiving.eAttackResult.Parried:
                    AddToAggroList(args.AttackData.Attacker, args.AttackData.Attacker.EffectiveLevel + args.AttackData.Damage + args.AttackData.CriticalDamage, true);
                    break;
            }
            if (orderAttackTarget == false)
                orderAttackTarget = true;

            if (args.AttackData.Attacker != null)
            {
                if (PetAllowedToAttack(args.AttackData.Attacker))
                {
                    Attack(args.AttackData.Attacker);
                }
                AttackMostWanted();
            }
        }


        /// <summary>
        /// Assist command
        /// </summary>
        /// <returns></returns>
        public bool Assist()
        {
            if (Body.EnableAssist)
            {
                return true;
            }
            else
                return false;
        }



        /// <summary>
        /// Handles attacks on Subpets by Enemys
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        private void OnAttackSubPets(DOLEvent e, object sender, EventArgs arguments)
        {

            base.Notify(e, sender, arguments);
            CommanderBrain CommanderBrain = null;

            GameNPC MainPet = null;

            //log.Error("Subpets werden angegriffen!!");


            if (m_aggressionState == eAggressionState.Passive)
                return;
                       
            GameLiving npc_owner = GetLivingOwner();

            if (npc_owner != null && npc_owner.ObjectState == GameObject.eObjectState.Active)
            {
                if (npc_owner is GamePlayer)
                {
                    GamePlayer MainpetOwner = npc_owner as GamePlayer;

                    if (MainpetOwner != null && MainpetOwner is GamePlayer && (MainpetOwner as GamePlayer).ControlledBrain != null)
                    {
                        if ((MainpetOwner as GamePlayer).ControlledBrain is CommanderBrain)
                        {
                            //log.Error("CommanderBrain gefunden");
                            MainPet = (MainpetOwner as GamePlayer).ControlledBrain.Body;
                           
                               //log.ErrorFormat("CommanderPet name: {0} gefunden", MainPet.Name);
                        }
                    }
                }

            }

            if (MainPet == null)
                return;

            CommanderPet commander = MainPet as CommanderPet;



            if (commander == null)
                return;

           


            if ((commander.Brain as ControlledNpcBrain).m_aggressionState == eAggressionState.Passive)
                return;
                        

            if (e == GameLivingEvent.EnemyKilled && Body.IsAttacking)
                return;
                      

            GameLiving living = sender as GameLiving;
            if (living == null) return;

            //log.ErrorFormat("living name: {0}", living.Name);//Subpet

            AttackedByEnemyEventArgs args = arguments as AttackedByEnemyEventArgs;

            if (living != null)//Body.IsCasting)
            {

                if (args == null) return;
                //log.ErrorFormat("under attack name: {0}", Body.Name);//Subpet
                if (args.AttackData.Attacker != null && args.AttackData.Attacker.IsAlive)
                {

                    GameLiving Attacktarget = args.AttackData.Attacker as GameLiving;



                    if ((commander.Brain is CommanderBrain) && !(commander.Brain as CommanderBrain).m_aggroTable.ContainsKey(Attacktarget))
                    {
                        CommanderBrain = (CommanderBrain)commander.Brain;
                       
                        if (CommanderBrain != null)
                        {

                            CommanderBrain.AddToAggroList(Attacktarget, 90, true);
                        }
                    }

                    if (commander.InCombat && commander.TargetObject != null && commander.TargetObject.IsObjectAlive == false)
                    {

                    }
                    else
                    {
                        if (commander.InCombat)
                        {
                            return;
                        }
                    }

                    if (CommanderBrain != null && CommanderBrain.orderAttackTarget == false)
                        CommanderBrain.orderAttackTarget = true;


                    if (CommanderBrain != null && Attacktarget != null && CommanderBrain.PetAllowedToAttack(Attacktarget))
                    {
                        CommanderBrain.Attack(Attacktarget);
                    }
                }
            }
        }


          



        /// <summary>
        /// Handles attacks on Living by Owner
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        private void OnAttackByOwner(DOLEvent e, object sender, EventArgs arguments)
        {

            base.Notify(e, sender, arguments);

            if (m_aggressionState == eAggressionState.Passive)
                return;

          
            if (Body is TheurgistPet)
                return;

            if (e == GameLivingEvent.EnemyKilled && Body.IsAttacking)
                return;



            GameLiving living = sender as GameLiving;
            if (living == null) return;
            GameLiving target = Owner.TargetObject as GameLiving;
            // AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
            AttackFinishedEventArgs attackFinished = arguments as AttackFinishedEventArgs;
            CastingEventArgs castFinished = arguments as CastingEventArgs;
            AttackData ad = null;
            ISpellHandler sp = null;

            if (attackFinished != null)
            {
                ad = attackFinished.AttackData;
            }
            else if (castFinished != null)
            {
                sp = castFinished.SpellHandler;
                ad = castFinished.LastAttackData;


            }
            if (sp == null && ad == null)
            {
                return;
            }
            else if (sp == null && (ad.AttackResult != GameLiving.eAttackResult.Any))
            {
                return;
            }
            else if (sp != null && (sp.HasPositiveEffect || ad == null))
            {
                return;
            }
            if ((AggressionState == eAggressionState.Defensive || AggressionState == eAggressionState.Passive || WalkState == eWalkState.Stay) && Assist() == false)
            {
                return;
            }

            if (target == null || target.IsAlive == false)
            {

                Body.StopCurrentSpellcast();

                return;
            }

            if (!m_aggroTable.ContainsKey(target))
            {
                AddToAggroList(target, 90, true);
            }
            if (orderAttackTarget == false)
                orderAttackTarget = true;
            

            if (target != null && PetAllowedToAttack(target))
            {
               Attack(target);
            }
        }



        /// <summary>
		/// Handler fired on every kill
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
        protected void PetAttackFinish(DOLEvent e, object sender, EventArgs arguments)
        {
            GameLiving living = sender as GameLiving;
            if (living == null) return;
            EnemyKilledEventArgs atkArgs = arguments as EnemyKilledEventArgs;

            if (Body is TheurgistPet)
                return;
            
            if (m_aggressionState == eAggressionState.Passive)
                return;
            

            AttackedByEnemyEventArgs args = arguments as AttackedByEnemyEventArgs;
            if (Body.IsCasting && Body.TempProperties.getProperty<bool>(IsCasting) == false)//Body.IsCasting)
            {
                
                if (args == null) return;
                if (args.AttackData.Target is GamePlayer && (args.AttackData.Target as GamePlayer).ControlledBrain != this)
                    return;
            }
           

            if (Body.Brain is TurretBrain)
            {
                if (orderAttackTarget == true)
                {
                    orderAttackTarget = false;

                }
                return;
            }
           
            if (atkArgs != null && orderAttackTarget == true)
            {
                orderAttackTarget = false;

                //Console.WriteLine("attack finish1 {0}", orderAttackTarget);
            }

            if (Body.TargetObject != null && Body.TargetObject.IsObjectAlive == false && m_aggroTable.ContainsKey((GameLiving)Body.TargetObject))
            {
                Body.StopCurrentSpellcast();

                RemoveFromAggroList((GameLiving)Body.TargetObject);

                if (m_aggroTable.Count == 0)
                {

                    Body.TempProperties.setProperty(HAS_TARGET, false);
                    Body.TargetObject = null;
                    FollowOwner();
                }
            }

           
            {
                if (atkArgs == null || Body == null || m_aggroTable.Count > 0) //Body.TargetObject != null && Body.InCombat && Body.TargetObject.IsObjectAlive)
                {
                    if (m_aggroTable.Count == 1 && Body.TargetObject != null && Body.TargetObject.IsObjectAlive == false)
                    {

                    }
                    else
                        return;
                }
            }

           
            Body.TempProperties.setProperty(HAS_TARGET, false);
            Body.TargetObject = null;
            FollowOwner();
        }

        protected override void BringFriends(AttackData ad)
        {
            // don't
        }

        public virtual bool CheckFormation(ref int x, ref int y, ref int z) { return false; }

        #endregion
    }
}
