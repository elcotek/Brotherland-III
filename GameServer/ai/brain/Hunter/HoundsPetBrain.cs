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
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;
using System;
using System.Reflection;

namespace DOL.AI.Brain
{
    public class HoundsPetBrain : StandardMobBrain, IControlledBrain
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);



        const string NO_ENEMY = "No_Enemy";
        const string ENEMY_FOUND = "Enemy_Found";

        protected const int BASEFORMATIONDIST = 50;

        private readonly GameLiving m_owner;
        private GameLiving m_target;
        private bool m_melee = false;
        private bool m_active = true;

        public HoundsPetBrain(GameLiving owner)
        {
            if (owner != null)
            {
                m_owner = owner;
            }
            base.AggroLevel = 0;
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
        /// <summary>
        /// Checks for the formation position of the BD pet
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public override bool CheckFormation(ref int x, ref int y, ref int z, ref short speed)
        {
            if (Body.TargetObject != null)
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
                x += (int)(((double)commander.FormationSpacing * par_slide) * Math.Cos(heading - Math.PI / 2));
                y += (int)(((double)commander.FormationSpacing * par_slide) * Math.Sin(heading - Math.PI / 2));
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
                }
                return true;
            }
            return false;
        }
        
        public override int ThinkInterval { get { return 1500; } }

        public override void Think()
        {
            foreach (GamePlayer enemy in Owner.GetPlayersInRadius(5000))
            {

                if (Body.TargetObject == null && enemy != null && enemy.Realm != Owner.Realm && Owner.IsWithinRadius(enemy, 2500))
                {

                    Body.TargetObject = enemy;
                    Body.TurnTo(Body.TargetObject);
                    if (Body.TargetObject == null)
                    Body.TempProperties.setProperty(NO_ENEMY, false);
                }
                if (Body.TargetObject != null && Body.TempProperties.getProperty<bool>(NO_ENEMY) == false)
                {
                    //StartsEnemySearch: Your Hound is starting to search!
                    //EnemyFound: Your Hound has discovered an enemy!

                    ((GamePlayer)Owner).Out.SendMessage(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language, "StartsEnemySearch"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    Body.TempProperties.setProperty(NO_ENEMY, true);
                    Body.TempProperties.setProperty(ENEMY_FOUND, false);
                    
                }


                if (Body.IsAlive == false)
                {
                    Body.TargetObject = null;

                }
                if (Body.TargetObject != null && Body.IsWithinRadius(Body.TargetObject, 250) == false)
                // Body.Follow(player, 100, 5000);
                {

                    //Body.WalkTo(Body.TargetObject, 90);
                    if (Body.Formation == GameNPC.eFormationType.Protect == false)
                    {
                        Body.Formation = GameNPC.eFormationType.Protect;
                    }

                    Body.PathTo(Body.TargetObject.X, Body.TargetObject.Y, (ushort)Body.TargetObject.Z, 90);
                    Body.BroadcastUpdate();
                    //return;
                }
                else if (Body.IsWithinRadius(Body.TargetObject, 250) == true)
                {
                    if (Owner is GamePlayer && Body.TempProperties.getProperty<bool>(ENEMY_FOUND) == false)
                    {
                        ((GamePlayer)Owner).Out.SendMessage(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language, "EnemyFound"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        Body.TempProperties.setProperty(ENEMY_FOUND, true);
                    }
                    //void SendSpellEffectAnimation(GameObject spellCaster, GameObject spellTarget, ushort spellid, ushort boltTime,
                    //          bool noSound, byte success);
                    //Now we send the combat-animation to all players that are viewing
                    //the combat scene			
                    foreach (GamePlayer player in Body.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                        player.Out.SendCombatAnimation(Body, Body.TargetObject, 0, 0, 0, 0, (byte)0, 100);//Die Hunde bellen wenn sie nahe am ziel sind!
                                                                                                          //Body.FireAmbientSentence(GameNPC.eAmbientTrigger.aggroing, Body.TargetObject as GameLiving);
                                                                                                          // Body.FireAmbientSentence(GameNPC.eAmbientTrigger.fighting, Body.TargetObject as GameLiving);
                    Body.StopMoving();
                    Body.TurnTo(Body.TargetObject);
                }
            }
        }

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
            if (m_target == null) m_target = (GameLiving)Body.TempProperties.getProperty<object>("target", null);
            if (m_target == null) return;
            GameLiving target = m_target;
            if (target != null && target.IsAlive)
            {
                Body.TargetObject = target;

                if (!CheckSpells(eCheckSpellType.Offensive))
                    Body.StartAttack(target);
            }
            else
            {
                m_target = null;
                m_active = false;
                Body.StopMoving();
                Body.MaxSpeedBase = 0;
                if (Body == Body as HoundsPet)
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
                        if (spell.CastTime > 0 && Body.IsWithinRadius(target, spell.Range))//fix von Elcotel: dont stay out from Spell range
                        {
                            if (!Body.IsBeingInterrupted && CheckOffensiveSpells(spell))
                            {
                                Body.StopMoving();/// test
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
