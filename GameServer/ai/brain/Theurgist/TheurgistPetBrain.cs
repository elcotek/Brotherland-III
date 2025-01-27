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
using DOL.GS.Spells;
using log4net;
using System;
using System.Reflection;

namespace DOL.AI.Brain
{
    public class TheurgistPetBrain : StandardMobBrain, IControlledBrain
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly GameLiving m_owner;
        private GameLiving m_target;
        private bool m_melee = false;
        private bool m_active = true;


        /// <summary>
        /// Allows to check if your target is stealthing - trying to escape your pet
        /// </summary>
        protected bool previousIsStealthed;


        public TheurgistPetBrain(GameLiving owner)
        {
            if (owner != null)
            {
                m_owner = owner;
            }
            base.AggroLevel = 100;
            IsMainPet = false;
        }
        public virtual GameNPC GetNPCOwner()
        {
            return null;
        }
        public virtual GameLiving GetLivingOwner()
        {
            GamePlayer player = GetPlayerOwner();
            if (player != null)
                return player;

            GameNPC npc = GetNPCOwner();
            if (npc != null)
                return npc;

            return null;
        }
        public override int ThinkInterval { get { return 1500; } }

        public override void Think() { AttackMostWanted(); }

        public void SetAggressionState(eAggressionState state) { }

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            if (!IsActive || m_melee || !m_active) return;

            if (args as AttackFinishedEventArgs != null)
            {
                m_melee = true;
                GameLiving target = m_target;
                if (target != null) Body.StartAttack(target);
                return;
            }
            if (e == GameLivingEvent.CastFailed)
            {

                GameLiving target = m_target;
                if (target != null) Body.StartAttack(target);
                return;

            }
        }

        protected override void AttackMostWanted()
        {

            if (!IsActive || !m_active) return;

           

            if (Owner != null)
            {
                GameSpellEffect speed = SpellHandler.FindEffectOnTarget(Owner, "SpeedOfTheRealm");
                if (speed != null)
                    speed.Cancel(false);
            }


            if (m_target == null) m_target = (GameLiving)Body.TempProperties.getProperty<object>("target", null);
            if (m_target == null) return;
            GameLiving target = m_target;
            //Confusion kill the pet
            GameSpellEffect confusion = SpellHandler.FindEffectOnTarget(Body, "Confusion");


            if (Owner != null && Owner is GamePlayer && (Owner as GamePlayer).IsAlive == false && Body == Body as TheurgistPet)
            {
                m_target = null;
                m_active = false;
                Body.StopMoving();
                Body.MaxSpeedBase = 0;

                if (Body == Body as TheurgistPet)
                {
                    Body.Die(Body);
                }
            }



            if (target is GamePlayer && target.IsAlive)
            {
                Body.TargetObject = target;

                if (Body.IsAttacking && (Body.TargetObject as GamePlayer).IsStealthed)
                {
                    target = null;
                    Body.StopAttack();
                    Body.StopMoving();
                    Body.MaxSpeedBase = 0;

                }
            }



            if (Body.IsAlive && target != null && target.IsAlive && confusion == null)
            {


                if (!CheckSpells(eCheckSpellType.Offensive))

                    Body.StartAttack(target);
            }
            else
            {
                m_target = null;
                m_active = false;
                Body.StopMoving();
                Body.MaxSpeedBase = 0;

                if (Body.IsAlive && Body == Body as TheurgistPet)
                {
                    Body.Die(Body);
                }
            }
        }

        public override bool CheckSpells(eCheckSpellType type)
        {
            if (Body == null || Body.Spells == null || Body.Spells.Count < 1 || m_melee)
                return false;

            if (Body.IsCasting)
                return true;

            bool casted = false;
            if (type == eCheckSpellType.Defensive)
            {
                foreach (Spell spell in Body.Spells)
                {
                    if (!Body.IsBeingInterrupted && Body.GetSkillDisabledDuration(spell) == 0 && CheckDefensiveSpells(spell))
                    {
                        casted = true;
                        break;
                    }
                }
            }
            else
            {
                foreach (Spell spell in Body.Spells)
                {
                    if (Body.GetSkillDisabledDuration(spell) == 0)
                    {
                        GameObject target = Body.TargetObject;
                        if (spell.CastTime > 0 && Body.IsWithinRadius(target, spell.Range) && !Body.IsBeingInterrupted)//fix von Elcotel: dont stay out from Spell range
                        {
                            if (CheckOffensiveSpells(spell))
                            {
                                Body.StopMoving();/// test
                                Body.StopFollowing();
                                casted = true;
                                break;



                            }
                        }
                        else
                        {
                            CheckInstantSpells(spell);
                        }
                    }
                }
            }
            if (this is IControlledBrain && !Body.AttackState)
                ((IControlledBrain)this).Follow(((IControlledBrain)this).Owner);
            return casted;
        }

        #region IControlledBrain Members
        public eWalkState WalkState { get { return eWalkState.Stay; } }
        public eAggressionState AggressionState { get { return eAggressionState.Aggressive; } set { } }
        public GameLiving Owner { get { return m_owner; } }
        public void Attack(GameObject target) { }
        public void Follow(GameObject target) { }
        public void FollowOwner() { }
        public void Stay() { }
        public void ComeHere() { }
        public void Goto(GameObject target) { }
        public void UpdatePetWindow() { }
        public GamePlayer GetPlayerOwner() { return m_owner as GamePlayer; }
        public bool IsMainPet { get { return false; } set { } }
        #endregion
    }
}
