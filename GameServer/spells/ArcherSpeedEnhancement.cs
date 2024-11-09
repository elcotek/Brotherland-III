﻿/*
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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Language;
using System;
using System.Collections.Generic;




namespace DOL.GS.Spells
{
    /// <summary>
    /// Increases the target's movement speed.
    /// </summary>
    [SpellHandlerAttribute("ArcherSpeedEnhancement")]
    public class ArcherSpeedEnhancementSpellHandler : SpellHandler
    {
        const string SpamTime = "NOCHARMEFFECTSPAM";
        /// <summary>
        /// called after normal spell cast is completed and effect has to be started
        /// </summary>
        public override void FinishSpellCast(GameLiving target)
        {
            Caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        /// <summary>
        /// Apply effect on target or do spell action if non duration spell
        /// </summary>
        /// <param name="target">target that gets the effect</param>
        /// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            if (target.EffectList.GetOfType<ChargeEffect>() != null)
                return;

            if (target.TempProperties.getProperty("Charging", false))
                return;

            if (target.EffectList.GetOfType<ArmsLengthEffect>() != null)
                return;

            if (target.EffectList.GetOfType<SpeedOfSoundEffect>() != null)
                return;

            if (target is GamePlayer && (target as GamePlayer).IsOnHorse)
                return;

            if (target is GamePlayer && (target as GamePlayer).IsRiding)
                return;

            if (target is GameBoat)
                return;

            if (target is GameBoatTimed)
                return;

            if (target is Keeps.GameKeepGuard)
                return;


            {
                if ((Spell.Pulse != 0 || Spell.CastTime != 0) && target.InCombat && target.InCombat)
                {
                    //no message spam
                    if (target is GamePlayer)
                    {
                        long UPDATETICK = target.TempProperties.getProperty<long>(SpamTime);
                        long changeTime = target.CurrentRegion.Time - UPDATETICK;

                        if (changeTime > 10 * 1000 || UPDATETICK == 0)
                        {
                            target.TempProperties.setProperty(SpamTime, target.CurrentRegion.Time);
                            (target as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((target as GamePlayer).Client.Account.Language, "Spells.YouInCombatRecently", Spell.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        }
                    }
                                        
                    return;
                }
                base.ApplyEffectOnTarget(target, effectiveness);
            }
        }

        /// <summary>
        /// When an applied effect starts
        /// duration spells only
        /// </summary>
        /// <param name="effect"></param>
        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);

            GamePlayer player = effect.Owner as GamePlayer;

            if (player == null || !player.IsStealthed || player.IsOnHorse || player.InCombat)
            {
                effect.Owner.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, Spell.Value / 100.0);
                SendUpdates(effect.Owner);
            }

            GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
            GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackFinished, new DOLEventHandler(OnAttack));
            GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.CastFinished, new DOLEventHandler(OnAttack));
            GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacka)); //xx2
            GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackFinished, new DOLEventHandler(OnAttacka));
            GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.CastFinished, new DOLEventHandler(OnAttacka));
            if (player != null)
                GameEventMgr.AddHandler(player, GamePlayerEvent.StealthStateChanged, new DOLEventHandler(OnStealthStateChanged));
        }

        /// <summary>
        /// When an applied effect expires.
        /// Duration spells only.
        /// </summary>
        /// <param name="effect">The expired effect</param>
        /// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
        /// <returns>immunity duration in milliseconds</returns>
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            GamePlayer player = effect.Owner as GamePlayer;
            GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
            GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackFinished, new DOLEventHandler(OnAttack));
            GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacka)); //xx1
            GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackFinished, new DOLEventHandler(OnAttacka));
            GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.CastFinished, new DOLEventHandler(OnAttacka));
            if (player != null)
                GameEventMgr.RemoveHandler(player, GamePlayerEvent.StealthStateChanged, new DOLEventHandler(OnStealthStateChanged));

            effect.Owner.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, this);

            if (!noMessages)
            {
                SendUpdates(effect.Owner);
            }

            return 0;
        }


        /// <summary>
        /// Sends updates on effect start/stop
        /// </summary>
        /// <param name="owner"></param>
        protected virtual void SendUpdates(GameLiving owner)
        {
            if (owner.IsMezzed || owner.IsStunned)
                return;
            if (owner is GamePlayer && ((GamePlayer)owner).IsOnHorse)
                return;
          owner.UpdateMaxSpeed();
        }

        /// <summary>
        /// Handles attacks on player/by player
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
        {
            GameLiving living = sender as GameLiving;
            if (living == null || arguments == null) return;
            AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
            AttackFinishedEventArgs attackFinished = arguments as AttackFinishedEventArgs;
            CastingEventArgs castFinished = arguments as CastingEventArgs;
            AttackData ad = null;
            ISpellHandler sp = null;

            if (attackedByEnemy != null)
            {
                ad = attackedByEnemy.AttackData;
            }
            else if (attackFinished != null)
            {
                ad = attackFinished.AttackData;
            }
            else if (castFinished != null)
            {
                sp = castFinished.SpellHandler;
                ad = castFinished.LastAttackData;

                // Speed should drop if the player casts an offensive spell
            }
            if (sp == null && ad == null)
            {
                return;
            }
            else if (sp == null && (ad.AttackResult != GameLiving.eAttackResult.HitStyle && ad.AttackResult != GameLiving.eAttackResult.HitUnstyled))
            {
                return;
            }
            else if (sp != null && (sp.HasPositiveEffect || ad == null))
            {
                return;
            }

            GameSpellEffect speed = SpellHandler.FindEffectOnTarget(living, this);
            if (speed != null)
                speed.Cancel(false);
        }
        /// <summary>
        /// Handles Archery Attacks
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>

        private void OnAttacka(DOLEvent e, object sender, EventArgs arguments)
        {
            GameLiving living = sender as GameLiving;
            if (living == null) return;
            AttackFinishedEventArgs attackFinished = arguments as AttackFinishedEventArgs;
            CastingEventArgs castFinished = arguments as CastingEventArgs;
            AttackData adv = null;
            ISpellHandler spv = null;

            if (attackFinished != null)
            {
                adv = attackFinished.AttackData;
            }
            else if (castFinished != null)
            {
                spv = castFinished.SpellHandler;
                adv = castFinished.LastAttackData;
            }
            else if (spv != null && (spv.HasPositiveEffect || adv == null))
            {
                return;
            }
            if (living != null && spv is Archery == true)
            {
                GameSpellEffect speed = SpellHandler.FindEffectOnTarget(living, this);
                if (speed != null)
                    speed.Cancel(false);
            }
        }

        /// <summary>
        /// Handles stealth state changes
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        private void OnStealthStateChanged(DOLEvent e, object sender, EventArgs arguments)
        {
            GamePlayer player = (GamePlayer)sender;
            if (player.IsStealthed)
                player.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, this);
            else player.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, Spell.Value / 100.0);
            // max speed update is sent in setalth method
        }

        /// <summary>
        /// Delve Info
        /// </summary>
        public override IList<string> DelveInfo(GamePlayer player)
        {
           
            {
                /*
                <Begin Info: Motivation Sng>
 
                The movement speed of the target is increased.
 
                Target: Group
                Range: 2000
                Duration: 30 sec
                Frequency: 6 sec
                Casting time:      3.0 sec
				
                This spell's effect will not take hold while the target is in combat.
                <End Info>
                */
                IList<string> list = base.DelveInfo(player);

                list.Add(" "); //empty line
                list.Add("This spell's effect will not take hold while the target is in combat.");

                return list;
            }
        }

        /// <summary>
        /// The spell handler constructor
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="spell"></param>
        /// <param name="line"></param>
        public ArcherSpeedEnhancementSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
