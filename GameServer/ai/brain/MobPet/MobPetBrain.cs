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
using DOL.GS;
using log4net;
using System;
using System.Collections;
using System.Reflection;

namespace DOL.AI.Brain
{
    /// <summary>
    /// A brain for NPC pets.
    /// </summary>
    /// <author>Aredhel</author>
    public class MobPetBrain : ControlledNpcBrain
    {
        protected const int BASEFORMATIONDIST = 50;
        
        
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MobPetBrain(GameLiving owner)
            : base(owner)
        {
        }


        public override int ThinkInterval
        {
            get { return 1000; }
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

        protected ushort originalModel;
        /// <summary>
        /// Brain main loop.
        /// </summary>
        public override void Think()
        {
            // CheckTether();

            // Mob pets need there own think as they may need to cast a spell in any state
            if (IsActive)
            {
                GameNPC Mobowner = null;



                if (Body.IsAlive && Body.Brain is MobPetBrain && Body != null && GetNPCOwner() != null && GetNPCOwner() is GameNPC)
                {
                    Mobowner = GetNPCOwner();

                    if (Mobowner.ObjectState != GameLiving.eObjectState.Active)
                    {

                        Body.Die(Body);

                    }

                    
                    if (Body.IsMoving == false && Body.AttackState == false && Body.IsAlive && Body.InCombat == false)
                    {
                        Body.Formation = GameNPC.eFormationType.Triangle;
                        Body.Heading = Mobowner.Heading;
                    }


                    if (Body.IsAlive && (Mobowner.Brain as StandardMobBrain).HasAggro == false)
                    {
                        FollowOwner();
                        return;
                    }
                    /*
                    if (Body.IsAlive && Body.TargetObject.IsWithinRadius(Body.TargetObject, 1000) == false && Body.IsCasting == false && Body.TargetObject != null)
                    {
                        if (Body.TargetObject.IsWithinRadius(Body.TargetObject, 1000) == false)
                        {
                            Body.Follow(Body.TargetObject, 400, 600);
                        }
                    }
                    */

                    if (Body.Brain is MobPetBrain && Mobowner.Brain is StandardMobBrain && (Mobowner.Brain as StandardMobBrain).HasAggro && Body != null && Body.IsAlive && Mobowner.IsAlive && Mobowner.TargetObject != null)
                    {
                        Attack(Mobowner.TargetObject);
                        AttackMostWanted();
                        return;
                    }


                    if (Mobowner.IsAlive == false && Body != null)
                    {
                        Body.TargetObject = null;
                        Body.StopMoving();
                        Body.MaxSpeedBase = 0;
                        Body.Die(Body);
                    }
                }







                /*
                if (Body.TargetObject != null && Body.TargetObject.IsObjectAlive == false || Body.TargetObject != null && (Mobowner.Brain as StandardMobBrain).HasAggro == false && Body.IsAlive)
                {
                    log.DebugFormat("folge wieder owner");
                    Body.TargetObject = null;
                    Body.StopAttack();
                    FollowOwner();
                }
                */
                // Do not discover stealthed players
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
            }
        }

        /// Checks for the formation position of the NPC pet
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public override bool CheckFormation(ref int x, ref int y, ref int z, ref short speed)
        {
            if (Owner != null && Owner is GameNPC && Body.Brain == this && !Body.AttackState && Body.Attackers.Count == 0)
            {
                GameNPC mob = (GameNPC)Owner;
                double heading = ((double)mob.Heading) * Point2D.HEADING_TO_RADIAN;
                //Get which place we should put minion
                int i = 0;
                //How much do we want to slide back and left/right
                int perp_slide = 0;
                int par_slide = 0;
                for (; i < mob.ControlledNpcList.Length; i++)
                {
                    if (mob.ControlledNpcList[i] == this)
                        break;
                }
                switch (mob.Formation)
                {
                    case GameNPC.eFormationType.Triangle:
                        par_slide = BASEFORMATIONDIST;
                        perp_slide = BASEFORMATIONDIST;
                        if (i != 0)
                            par_slide = BASEFORMATIONDIST * 2;
                        break;
                    case GameNPC.eFormationType.Line:
                        par_slide = BASEFORMATIONDIST * (i + 1);
                        break;
                    case GameNPC.eFormationType.Protect:
                        switch (i)
                        {
                            case 0:
                                par_slide = -BASEFORMATIONDIST * 2;
                                break;
                            case 1:
                            case 2:
                                par_slide = -BASEFORMATIONDIST;
                                perp_slide = BASEFORMATIONDIST;
                                break;
                        }
                        break;
                }
                //Slide backwards - every pet will need to do this anyways
                x += (int)(((double)mob.FormationSpacing * par_slide) * Math.Cos(heading - Math.PI / 2));
                y += (int)(((double)mob.FormationSpacing * par_slide) * Math.Sin(heading - Math.PI / 2));
                //In addition with sliding backwards, slide the other two pets sideways
                switch (i)
                {
                    case 1:
                        x += (int)(((double)mob.FormationSpacing * perp_slide) * Math.Cos(heading - Math.PI));
                        y += (int)(((double)mob.FormationSpacing * perp_slide) * Math.Sin(heading - Math.PI));
                        break;
                    case 2:
                        x += (int)(((double)mob.FormationSpacing * perp_slide) * Math.Cos(heading));
                        y += (int)(((double)mob.FormationSpacing * perp_slide) * Math.Sin(heading));
                        break;
                }
                return true;
            }
            return false;
        }
    }
}


