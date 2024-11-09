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
using System;
using System.Collections.Generic;
using System.Linq;

using DOL.AI.Brain;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
    [SpellHandlerAttribute("BeFriend")]
    public class BeFriendSpellHandler : SpellHandler
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Dictionary to Keep track of Friend Brains Attached to NPC
        /// </summary>
        private readonly ReaderWriterDictionary<GameNPC, FriendBrain> m_NPCFriendBrain = new ReaderWriterDictionary<GameNPC, FriendBrain>();

        /// <summary>
        /// Consume Power on Spell Start
        /// </summary>
        /// <param name="target"></param>
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        /// <summary>
        /// Select only uncontrolled GameNPC Targets
        /// </summary>
        /// <param name="castTarget"></param>
        /// <returns></returns>
        public override IList<GameLiving> SelectTargets(GameObject castTarget)
        {
            return base.SelectTargets(castTarget).Where(t => t is GameNPC).ToList();
        }



        private const string LOSEFFECTIVENESS = "LOS Effectivness";


        double effectiveness = 0;

        //LosCheck
        private void CheckSpellLOS(GamePlayer player, ushort response, ushort targetOID)
        {
            GameLiving target = null;
            if ((response & 0x100) == 0x100)
            {
                try
                {
                    if (Caster != null && Caster.ObjectState == GameLiving.eObjectState.Active && Caster.CurrentRegion.GetObject(targetOID) is GameLiving)

                        target = (Caster.CurrentRegion.GetObject(targetOID) as GameLiving);

                    if (target != null && target.ObjectState == GameLiving.eObjectState.Active)
                    {
                        var targets = SelectTargets(target);
                        if (targets.Count <= 0) return;

                        foreach (GameLiving healTarget in targets)
                        {
                            if (healTarget != null && healTarget == target)
                            {
                                effectiveness = player.TempProperties.getProperty<double>(LOSEFFECTIVENESS + target.ObjectID, 1.0);


                                StartCharmEffects(healTarget, target, effectiveness);
                                //log.ErrorFormat("heal 1 ok  name = {0}", healTarget.Name);
                                player.TempProperties.removeProperty(LOSEFFECTIVENESS + target.ObjectID);

                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (log.IsErrorEnabled)
                        log.Error(string.Format("targetOID:{0} caster:{1} exception:{2}", targetOID, Caster, e));
                }
            }
        }


        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            bool spellOK = true;

            if (Spell.Target == "Self" || (Spell.Target == "Realm" && Spell.Radius > 0 && Spell.Range == 0))
            {
                spellOK = false;
            }

            if (target == null) return;

            var targets = SelectTargets(target);
            if (targets.Count <= 0) return;

            foreach (GameLiving healTarget in targets)
            {
                if (healTarget != null && healTarget == target)

                {
                    if ((Spell.Target != "Group" || Spell.CastTime == 0) && (spellOK == false || MustCheckLOS(Caster)))
                    {

                        GamePlayer checkPlayer = null;
                        if (healTarget is GamePlayer)
                        {
                            checkPlayer = healTarget as GamePlayer;
                        }
                        else
                        {
                            if (Caster is GamePlayer)
                            {
                                checkPlayer = Caster as GamePlayer;
                            }
                            else if (Caster is GameNPC && (Caster as GameNPC).Brain is IControlledBrain)
                            {
                                IControlledBrain brain = (Caster as GameNPC).Brain as IControlledBrain;
                                checkPlayer = brain.GetPlayerOwner();
                            }
                        }
                        if (checkPlayer != null)
                        {
                            checkPlayer.TempProperties.setProperty(LOSEFFECTIVENESS + healTarget.ObjectID, effectiveness);

                            checkPlayer.Out.SendCheckLOS(Caster, healTarget, new CheckLOSResponse(CheckSpellLOS));
                        }
                        else
                        {
                            StartCharmEffects(healTarget, target, effectiveness);
                            //log.ErrorFormat("heal 2 ok  name = {0}", healTarget.Name);
                        }
                    }
                    else
                    {
                        StartCharmEffects(healTarget, target, effectiveness);
                        //log.ErrorFormat("heal 3 ok  name = {0}", healTarget.Name);
                    }
                }
            }
        }

        /// <summary>
        /// called when spell effect has to be started and applied to targets
        /// </summary>
         public virtual void StartCharmEffects(GameLiving healTargets, GameLiving target, double effectiveness)
       // public virtual void StartCharmEffects(GameLiving target, double effectiveness)
        {
            var npcTarget = target as GameNPC;
            if (npcTarget == null) return;
            if (npcTarget.Realm != 0) return;
            if (npcTarget.Brain is KeepGuardBrain) return;
            if (npcTarget.Brain is StandardMobBrain == false) return;
            if (npcTarget.Brain is ControlledNpcBrain) return;

            if (npcTarget.Level > Spell.Value)
            {
                // Resisted
                SendSpellResistAnimation(target);
                this.MessageToCaster(eChatType.CT_SpellResisted, "{0} is too strong for you to charm!", target.GetName(0, true));
                return;
            }

            if (npcTarget.Brain is IControlledBrain)
            {
                SendSpellResistAnimation(target);
                this.MessageToCaster(eChatType.CT_SpellResisted, "{0} is already under control.", target.GetName(0, true));
                return;
            }

            base.ApplyEffectOnTarget(target, effectiveness);
        }

        /// <summary>
        /// On Effect Start Replace Brain with Fear Brain.
        /// </summary>
        /// <param name="effect"></param>
        public override void OnEffectStart(GameSpellEffect effect)
        {
            var npcTarget = effect.Owner as GameNPC;

            var currentBrain = npcTarget.Brain as IOldAggressiveBrain;
            var friendBrain = new FriendBrain(this);
            m_NPCFriendBrain.AddOrReplace(npcTarget, friendBrain);

            npcTarget.AddBrain(friendBrain);
            friendBrain.Think();

            // Prevent Aggro on Effect Expires.
            if (currentBrain != null)
                currentBrain.ClearAggroList();
            npcTarget.Realm = Caster.Realm;
            npcTarget.BroadcastUpdate();

           

            if (npcTarget != null)
            {
                foreach (GameNPC npc in npcTarget.GetNPCsInRadius((ushort)1500))
                {
                    if (!GameServer.ServerRules.IsAllowedToAttack(npc, npcTarget, true) || npc.IsAlive == false)
                        continue;

                   

                    if ((npcTarget.TargetObject == null || npcTarget.TargetObject == Caster) && npc != null && npcTarget.IsAlive)
                    {
                       npcTarget.TargetObject = npc;

                        (npcTarget.Brain as StandardMobBrain).AddToAggroList(npc, (npc.Level + 1) << 1, true);
                       if (npcTarget.TargetObject != null)
                         npcTarget.PathTo(npc.X, npc.Y, npc.Z, npcTarget.MaxSpeed);
                         npcTarget.StartAttack(npc);

                    }
                }
            }
            if (npcTarget != null)
            {
                foreach (GamePlayer player in npcTarget.GetPlayersInRadius((ushort)1500))
                {
                    if (!GameServer.ServerRules.IsAllowedToAttack(player, npcTarget, true) || player.IsAlive == false)
                        continue;

                  
                    if ((npcTarget.TargetObject == null || npcTarget.TargetObject == Caster) && player != null && npcTarget.IsAlive)
                    {
                        npcTarget.TargetObject = player;

                        (npcTarget.Brain as StandardMobBrain).AddToAggroList(player, (player.Level + 1) << 1, true);
                        if (npcTarget.CurrentRegion.IsDungeon == false && npcTarget.TargetObject != null)
                         npcTarget.PathTo(player.X, player.Y, player.Z, npcTarget.MaxSpeed);
                         npcTarget.StartAttack(player);

                    }
                }
            }



        }

        /// <summary>
        /// Called when Effect Expires
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="noMessages"></param>
        /// <returns></returns>
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            var npcTarget = effect.Owner as GameNPC;

            FriendBrain fearBrain;
            if (m_NPCFriendBrain.TryRemove(npcTarget, out fearBrain))
            {
                npcTarget.RemoveBrain(fearBrain);
            }

            if (npcTarget.Brain == null)
                npcTarget.AddBrain(new StandardMobBrain());
                npcTarget.Realm = 0;
                npcTarget.BroadcastUpdate();
            if(npcTarget.Brain != null)
                (npcTarget.Brain as StandardMobBrain).AddToAggroList(Caster, Caster.EffectiveLevel << 1);

            return base.OnEffectExpires(effect, noMessages);
        }

        /// <summary>
        /// Spell Resists don't trigger notification or interrupt
        /// </summary>
        /// <param name="target"></param>
        protected override void OnSpellResisted(GameLiving target)
        {
            SendSpellResistAnimation(target);
            SendSpellResistMessages(target);
            StartSpellResistLastAttackTimer(target);
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="spell"></param>
        /// <param name="line"></param>
        public BeFriendSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
