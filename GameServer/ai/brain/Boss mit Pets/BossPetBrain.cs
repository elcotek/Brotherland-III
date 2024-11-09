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
using log4net;
using System;
using System.Collections;
using System.Reflection;

namespace DOL.AI.Brain
{
    public abstract class BossPetBrain : ControlledNpcBrain
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        

        GameNPC Mobowner = null;

        protected const int BASEFORMATIONDIST = 50;

        public BossPetBrain(GameLiving Owner)
            : base(Owner)
        {
            IsMainPet = false;
        }

        public override int ThinkInterval
        {
            get { return 1300; }
        }


        public override int CastInterval
        {
            get { return 500; }
            set { }
        }
        /// <summary>
        /// Checks whether living has someone on its aggrolist
        /// </summary>
        public override bool HasAggro
        => AggroTable.Count > 0;


        /// <summary>
        /// Starts the brain thinking and resets the inactivity countdown
        /// </summary>
        /// <returns>true if started</returns>
        public override bool Start()
        {
            if (!base.Start()) return false;

            if (Body.ObjectState == GameObject.eObjectState.Active)
            {


                // [Ganrod] On supprime la cible du pet au moment  du contrôle.
                Body.TargetObject = null;
                GameEventMgr.AddHandler(Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnOwnerAttacked));
                GameEventMgr.AddHandler(Owner, GameLivingEvent.AttackFinished, new DOLEventHandler(OnOwnerAttacked));
                GameEventMgr.AddHandler(Owner, GameLivingEvent.Dying, new DOLEventHandler(OnWnerDieing));

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


            GameEventMgr.RemoveHandler(Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnOwnerAttacked));
            GameEventMgr.RemoveHandler(Owner, GameLivingEvent.AttackFinished, new DOLEventHandler(OnOwnerAttacked));
            GameEventMgr.RemoveHandler(Owner, GameLivingEvent.Dying, new DOLEventHandler(OnWnerDieing));
            // GameEventMgr.Notify(GameLivingEvent.PetReleased, Body);

            return true;
        }

        /// <summary>
        /// Brain main loop.
        /// </summary>
        public override void Think()
        {
            // CheckTether();

            // Mob pets need there own think as they may need to cast a spell in any state
            if (IsActive)
            {
               


                if (Body.IsAlive && Body.Brain is BossPetBrain && Body != null && GetNPCOwner() != null && GetNPCOwner() is GameNPC)
                {
                    Mobowner = GetNPCOwner();
                  


                    if (Mobowner != null && Body.IsAlive)
                    {

                        //If we have an aggrolevel above 0, we check for players and npcs in the area to attack
                        if (Mobowner.AttackState && Mobowner.InCombat || Mobowner.Brain is BossBrain && (Mobowner.Brain as BossBrain).HasAggro) //e && AggroLevel > 0)
                        {

                            if (Mobowner.HealthPercent < 100)
                            {
                                CheckSpells(eCheckSpellType.Defensive);
                            }

                            CheckPlayerAggro();
                            CheckNPCAggro();
                            //log.Debug("checker aggro");
                        }

                        if (HasAggro)
                        {
                            
                            CheckSpells(eCheckSpellType.Offensive);
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

                        Body.Heading = Mobowner.Heading;
                        FollowOwner();
                    }
                    //Sub pets follow the Owner
                    if (Body.AttackState == false && Body.InCombat == false && Body != null && Body.TargetObject == null)
                    {
                        FollowOwner();
                    }

                    if (Body.TargetObject != null)
                    {
                        if (Body.TargetObject is GamePlayer)
                        {
                            if (Body.IsAttacking && (Body.TargetObject as GamePlayer).IsStealthed)
                            {
                                Body.StopAttack();
                                FollowOwner();
                            }
                        }
                    }

                    if (Body.TargetObject != null && Body.TargetObject.IsObjectAlive == false || Body.TargetObject != null && (Mobowner.Brain as StandardMobBrain).HasAggro == false && Body.IsAlive)
                    {
                        //log.DebugFormat("folge wieder owner");
                        Body.TargetObject = null;
                        Body.StopAttack();
                        FollowOwner();
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
        protected override void OnOwnerAttacked(DOLEvent e, object sender, EventArgs arguments)
        {
            sender = Mobowner;

            {
                AttackedByEnemyEventArgs attackFinished = arguments as AttackedByEnemyEventArgs;

                if (attackFinished == null) return;

                if (attackFinished.AttackData.Target is GamePlayer && (attackFinished.AttackData.Target as GamePlayer).ControlledBrain != this)
                    return;
                // react only on these attack results
                switch (attackFinished.AttackData.AttackResult)
                {
                    case GameLiving.eAttackResult.Blocked:
                    case GameLiving.eAttackResult.Evaded:
                    case GameLiving.eAttackResult.Fumbled:
                    case GameLiving.eAttackResult.HitStyle:
                    case GameLiving.eAttackResult.HitUnstyled:
                    case GameLiving.eAttackResult.Missed:
                    case GameLiving.eAttackResult.Parried:
                    case GameLiving.eAttackResult.Any:
                        {
                            AddToAggroList(attackFinished.AttackData.Attacker, attackFinished.AttackData.Attacker.EffectiveLevel + attackFinished.AttackData.Damage + attackFinished.AttackData.CriticalDamage, true);

                            break;
                        }
                }
                if (attackFinished.AttackData.Attacker != null)
                {

                    StartAttacke(attackFinished.AttackData.Attacker);
                    AttackMostWanted();
                }
            }
        }

        /// <summary>
        /// Owner attacked event
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        protected virtual void OnWnerDieing(DOLEvent e, object sender, EventArgs arguments)
        {
            sender = Mobowner;

            {
                DyingEventArgs ownerdieing = arguments as DyingEventArgs;

                if (ownerdieing == null) return;

                if (ownerdieing.Killer != null)
                {
                    PetsDie(Owner as GameNPC);
                }
            }
        }

    /// <summary>
    /// Direct the subpets to attack too
    /// </summary>
    /// <param name="target">The target to attack</param>
    public virtual void PetsDie(GameNPC owner)
    {
        if (owner != null && owner is GameNPC && (owner as GameNPC).ControlledNpcList != null)
        {
            lock ((owner as GameNPC).ControlledNpcList)
            {
                foreach (BossPetBrain icb in (owner as GameNPC).ControlledNpcList)
                    if (icb != null && icb.Body.IsAlive)
                        icb.Body.Die(icb.Body);



            }
        }
    }
    /// <summary>
    /// Checks for the formation position of the BD pet
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public override bool CheckFormation(ref int x, ref int y, ref int z, ref short speed)
        {


            if (!Body.AttackState && Body.Attackers.Count == 0)
            {
                GameNPC commander = (GameNPC)Owner;
                double heading = ((double)commander.Heading) * Point2D.HEADING_TO_RADIAN;
                //Get which place we should put minion
                int i = 0;
                //How much do we want to slide back and left/right
                int perp_slide = 0;
                int par_slide = 0;
                for (; i < commander.ControlledNpcList.Length; i++)
                {
                    if (commander.ControlledNpcList[i] == this)
                        break;
                }
                switch (commander.Formation)
                {
                    case GameNPC.eFormationType.Triangle:
                        par_slide = BASEFORMATIONDIST;
                        perp_slide = BASEFORMATIONDIST;
                        if (i != 0 && commander.ControlledCount > 2)
                        {
                            par_slide = BASEFORMATIONDIST * 2;
                        }
                        if (i != 0 && commander.ControlledCount == 2)
                        {
                            perp_slide = -BASEFORMATIONDIST; //inten/vorne
                            par_slide = -BASEFORMATIONDIST * 2; // seitwärts
                        }



                        break;
                    case GameNPC.eFormationType.Line:
                        if (commander.ControlledCount == 1)
                        {

                        }
                        else
                            par_slide = BASEFORMATIONDIST;
                        if (i != 0)
                            par_slide = BASEFORMATIONDIST * (i + 1);// seitwärts



                        break;
                    case GameNPC.eFormationType.Protect:
                        switch (i)
                        {
                            case 0:
                                if (commander.ControlledCount == 2)
                                {
                                    par_slide = BASEFORMATIONDIST;// seitwärts
                                }
                                else
                                    par_slide = -BASEFORMATIONDIST * 2;// seitwärts
                                break;
                            case 1:
                            case 2:
                                if (i != 0 && commander.ControlledCount == 2)
                                {
                                   // perp_slide = -BASEFORMATIONDIST; //inten/vorne
                                    par_slide = -BASEFORMATIONDIST * 2; // seitwärts
                                }
                                else
                                par_slide = -BASEFORMATIONDIST;// seitwärts
                                perp_slide = BASEFORMATIONDIST;
                                break;
                            case 3:
                            case 4:
                                par_slide = -BASEFORMATIONDIST * 2;// seitwärts
                                perp_slide = BASEFORMATIONDIST * 2;//inten/vorne
                                break;
                           
                        }
                        break;
                }
                //Slide backwards - every pet will need to do this anyways

                if (commander.ControlledCount == 2)
                {
                    x += (int)(((double)commander.FormationSpacing * par_slide) * Math.Cos(heading - Math.PI));
                    y += (int)(((double)commander.FormationSpacing * par_slide) * Math.Sin(heading - Math.PI));
                }
                else
                {
                    x += (int)(((double)commander.FormationSpacing * par_slide) * Math.Cos(heading - Math.PI / 2));
                    y += (int)(((double)commander.FormationSpacing * par_slide) * Math.Sin(heading - Math.PI / 2));
                }
                //In addition with sliding backwards, slide the other two pets sideways
                switch (i)
                {
                    case 1:
                        x += (int)(((double)commander.FormationSpacing * perp_slide) * Math.Cos(heading - Math.PI));
                        y += (int)(((double)commander.FormationSpacing * perp_slide) * Math.Sin(heading - Math.PI));
                        break;
                    case 2:
                        x += (int)(((double)commander.FormationSpacing * perp_slide) * Math.Cos(heading));
                        y += (int)(((double)commander.FormationSpacing * perp_slide) * Math.Sin(heading));
                        break;
                    case 3:
                        x += (int)(((double)commander.FormationSpacing * perp_slide) * Math.Cos(heading + Math.PI));
                        y += (int)(((double)commander.FormationSpacing * perp_slide) * Math.Sin(heading + Math.PI));
                        break;
                    case 4:
                        x += (int)(((double)commander.FormationSpacing * perp_slide) * Math.Cos(heading + Math.PI * 2));
                        y += (int)(((double)commander.FormationSpacing * perp_slide) * Math.Sin(heading + Math.PI * 2));
                        break;
                }
                return true;
            }
            return false;
        }


        /// <summary>
        /// Direct the subpets to attack too
        /// </summary>
        /// <param name="target">The target to attack</param>
        public virtual void StartAttacke(GameObject target)
        {
            Body.StartAttack(target);

            //Check for any abilities
            CheckAbilities();

            if (Mobowner is GameNPC && (Mobowner as GameNPC).ControlledNpcList != null)
            {
                //Can control this pets too : //Ceremonial Bracers//Healers Embrace //A Flask //nailash Robe //shield of chaos //Stone of Atlantis // TRaitor's Dagger //The Scropion Tail //Traldor's Oracle
                lock ((Mobowner as GameNPC).ControlledNpcList)
                {
                    if (Mobowner.Brain is BossPetBrain) 
                    {
                        foreach (BossPetBrain icb in (Mobowner as GameNPC).ControlledNpcList)
                            if (icb != null && target != null)
                                if (icb.Body.TargetObject != null && icb.HasAggro && icb.Body.TargetObject.Equals(target))
                                    return;

                                else

                                    icb.Body.StartAttack(target);
                    }
                }
            }
        }



        /// <summary>
        /// Lost follow target event
        /// </summary>
        /// <param name="target"></param>
        protected override void OnFollowLostTarget(GameObject target)
        {
            if (target == Owner)
            {
                GameEventMgr.Notify(GameLivingEvent.PetReleased, Body);
                return;
            }
            FollowOwner();
        }
    }
}