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
 //changed by Elcotek

using DOL.GS.Effects;
using System.Collections.Generic;






namespace DOL.GS.Spells
{
    /// <summary>
    /// Base class for all spells that remove effects
    /// </summary>
    public abstract class RemoveSpellEffectHandler : SpellHandler
    {
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Stores spell effect type that will be removed
        /// RR4: now its a list of effects to remove
        /// </summary>
        protected List<string> m_spellTypesToRemove = null;

        /// <summary>
        /// Spell effect type that will be removed
        /// RR4: now its a list of effects to remove
        /// </summary>
        public virtual List<string> SpellTypesToRemove
        {
            get { return m_spellTypesToRemove; }
        }

        /// <summary>
        /// called when spell effect has to be started and applied to targets
        /// </summary>
        /// <param name="target"></param>
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
        {
            return new GameSpellEffect(this, CalculateEffectDuration(target, effectiveness), Spell.Frequency, effectiveness);
        }

        public override void OnEffectPulse(GameSpellEffect effect)
        {
            base.OnEffectPulse(effect);

            //Pulsing spell beenden wenn pulse erreicht sind
            if (effect.Owner != null && (effect.Name == "Pulsing Cure Disease II" || effect.Name == "Pulsing Cure Disease" || effect.Name == "Pulsing Cure Poison II" || effect.Name == "Pulsing Cure Poison" || effect.Name == "Pulsing Cure Disease"))
            {

                if (HasPositiveEffect && Spell.Pulse != 0 && Spell.Pulse >= 1)
                {

                    //Pulse
                    for (int i = 0; i < Spell.Pulse; i++)
                    {
                        effect.Owner.PulseCount++;
                        OnDirectEffect(effect.Owner, effect.Effectiveness);
                        // log.ErrorFormat("add pulse = {0}, {1}", effect.Owner.PulseCount, effect.Name);
                        if (effect.Owner.PulseCount == Spell.Pulse + 1)
                            break;
                    }
                    //log.ErrorFormat("pulse nun = {0}, {1}", effect.Owner.PulseCount, effect.Name);
                    if (effect.Owner.PulseCount - 1 > (byte)Spell.Pulse)//Count of allowed pulses
                    {
                        // log.ErrorFormat("Remove at pulse = {0}", effect.Owner.PulseCount);
                        CancelPulsingSpell(effect.Owner, Spell.SpellType);
                        CancelPulsingSpell(Caster, Spell.SpellType);
                        effect.Owner.PulseCount = 0;
                        return;
                    }
                    //else
                    /*
                    {

                        {
                           // effect.Owner.TempProperties.removeProperty(CurePulse);
                           /// effect.Owner.PulseCount = 0;
                            /*
                             *  //Pulsing spell beenden wenn das ziel zu weit weg ist
                            if (Caster is GamePlayer && HasPositiveEffect && Spell.Pulse != 0 && Caster.IsWithinRadius(effect.Owner, CalculateSpellRange()) == false)
                            {

                                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourTargetIsToFarAway"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                                GameSpellEffect peffect = SpellHandler.FindEffectOnTarget(effect.Owner, Spell.SpellType);
                                if (peffect != null)
                                {
                                    peffect.Cancel(false);//call OnEffectExpires
                                    CancelPulsingSpell(Caster, Spell.SpellType);
                                    CancelPulsingSpell(m_spellTarget, Spell.SpellType);
                                }
                                return;
                            }
                            */

                }
            }
            else
            {
                OnDirectEffect(effect.Owner, effect.Effectiveness);
            }
        }


        /// <summary>
        /// execute non duration spell effect on target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectiveness"></param>
        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            base.OnDirectEffect(target, effectiveness);
            if (target == null || !target.IsAlive)
                return;

            // RR4: we remove all the effects
            foreach (string toRemove in SpellTypesToRemove)
            {
                GameSpellEffect effect = SpellHandler.FindEffectOnTarget(target, toRemove);
                if (effect != null)
                    effect.Cancel(false);
            }
            SendEffectAnimation(target, 0, false, 1);
        }

        // constructor
        public RemoveSpellEffectHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}