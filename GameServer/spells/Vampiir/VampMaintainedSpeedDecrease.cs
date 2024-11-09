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
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Language;



namespace DOL.GS.Spells
{
    /// <summary>
    /// Spell handler for speed decreasing spells.  Special for vampiirs
    /// </summary>
    [SpellHandler("VampSpeedDecrease")]
    public class VampMaintainedSpeedDecrease : SpeedDecreaseSpellHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected GameLiving m_originalTarget = null;
        protected bool m_isPulsing = false;

        public VampMaintainedSpeedDecrease(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        /// <summary>
        /// Creates the corresponding spell effect for the spell
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectiveness"></param>
        /// <returns></returns>
        protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
        {
            m_originalTarget = target;

            // this acts like a pulsing spell effect, but with 0 frequency.
            return new GameSpellEffect(this, CalculateEffectDuration(target, effectiveness), 0, effectiveness);
        }

        /// <summary>
        /// This spell is a pulsing spell, not a pulsing effect, so we check spell pulse
        /// </summary>
        /// <param name="effect"></param>
        public override void OnSpellPulse(PulsingSpellEffect effect)
        {
            if (m_originalTarget == null || Caster.ObjectState != GameObject.eObjectState.Active || m_originalTarget.ObjectState != GameObject.eObjectState.Active)
            {
                if (Caster is GamePlayer)
                {
                    //MessageToCaster("Your spell was cancelled.", eChatType.CT_SpellExpires);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourSpellWasCancelled"), eChatType.CT_SpellExpires, eChatLoc.CL_SystemWindow);
                }
                    effect.Cancel(false);
                return;
            }

            if (!Caster.IsAlive ||
                !m_originalTarget.IsAlive ||
                Caster.IsMezzed ||
                Caster.IsStunned ||
                Caster.IsSitting ||
                (Caster.TargetObject is GameLiving ? m_originalTarget != Caster.TargetObject as GameLiving : true))
            {
                if (Caster is GamePlayer)
                {
                    //MessageToCaster("Your spell was cancelled.", eChatType.CT_SpellExpires);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourSpellWasCancelled"), eChatType.CT_SpellExpires, eChatLoc.CL_SystemWindow);
                }
                    effect.Cancel(false);
                return;
            }

            if (!Caster.IsWithinRadius(m_originalTarget, CalculateSpellRange()))
            {
                if (Caster is GamePlayer)
                {
                    //MessageToCaster("Your target is no longer in range.", eChatType.CT_SpellExpires);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourTargetIsNoLongerInRange"), eChatType.CT_SpellExpires, eChatLoc.CL_SystemWindow);
                }
                    effect.Cancel(false);
                return;
            }

            if (!Caster.TargetInView)
            {
                if (Caster is GamePlayer)
                {
                    //MessageToCaster("Your target is no longer in view.", eChatType.CT_SpellExpires);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourTargetIsNoLongerInView"), eChatType.CT_SpellExpires, eChatLoc.CL_SystemWindow);
                }
                    effect.Cancel(false);
                return;
            }

            base.OnSpellPulse(effect);
        }
        /// <summary>
        /// Handles attack on Debuff owner
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        protected override void OnAttacked(DOLEvent e, object sender, EventArgs arguments)
        {
            AttackedByEnemyEventArgs attackArgs = arguments as AttackedByEnemyEventArgs;
            GameLiving living = sender as GameLiving;
            if (attackArgs == null) return;
            if (living == null && m_originalTarget == null) return;

            switch (attackArgs.AttackData.AttackResult)
            {
                case GameLiving.eAttackResult.HitStyle:
                case GameLiving.eAttackResult.HitUnstyled:
                    GameSpellEffect effect = FindEffectOnTarget(m_originalTarget, "VampSpeedDecrease");
                    if (effect != null)

                        if (Caster is GamePlayer)
                        {
                            (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourSpellWasCancelledWileTheTargetHit"), eChatType.CT_SpellExpires, eChatLoc.CL_SystemWindow);
                            //MessageToCaster("Your spell was cancelled wile the target is hit by damage.", eChatType.CT_SpellExpires);
                        }
                            effect.Cancel(false);
                    break;
            }
        }

        // protected override void OnAttacked(DOLEvent e, object sender, EventArgs arguments)
        //  {
        // Spell can be used in combat, do nothing
        //   }

    }
}