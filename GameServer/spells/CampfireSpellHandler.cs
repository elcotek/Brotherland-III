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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Language;



namespace DOL.GS.Spells
{
    /// <summary>
    /// Heal Over Time spell handler
    /// </summary>
    [SpellHandlerAttribute("Campfire")]
    public class CampfireSpellHandler : SpellHandler
    {
        /// <summary>
        /// Execute heal over time spell
        /// </summary>
        /// <param name="target"></param>
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }


        /// <summary>
        /// Determines wether this spell is compatible with given spell
        /// and therefore overwritable by better versions
        /// spells that are overwritable cannot stack
        /// </summary>
        /// <param name="compare"></param>
        /// <returns></returns>
        public override bool IsOverwritable(GameSpellEffect compare)
        {
            return Spell.SpellType == compare.Spell.SpellType && Spell.DamageType == compare.Spell.DamageType && SpellLine.IsBaseLine == compare.SpellHandler.SpellLine.IsBaseLine;
        }


        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            // TODO: correct formula
            double eff = 1.25;
            if (Caster is GamePlayer)
            {
                double lineSpec = Caster.GetModifiedSpecLevel(m_spellLine.Spec);
                if (lineSpec < 1)
                    lineSpec = 1;
                eff = 0.75;
                if (Spell.Level > 0)
                {
                    eff += (lineSpec - 1.0) / Spell.Level * 0.5;
                    if (eff > 1.25)
                        eff = 1.25;
                }
            }
            base.ApplyEffectOnTarget(target, eff);
        }


        protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
        {
            return new GameSpellEffect(this, Spell.Duration, Spell.Frequency, effectiveness);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            SendEffectAnimation(effect.Owner, 0, false, 1);
            //"{0} seems calm and healthy."

            if (effect.Owner is GamePlayer)
            {
                (effect.Owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((effect.Owner as GamePlayer).Client.Account.Language, "CampfireSpellHander.CampfireStart"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
            }
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), eChatType.CT_Spell, effect.Owner);
        }

        public override void OnEffectPulse(GameSpellEffect effect)
        {
            base.OnEffectPulse(effect);
            OnDirectEffect(effect.Owner, effect.Effectiveness);
        }

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            GamePlayer player = target as GamePlayer;
            if (target.ObjectState != GameObject.eObjectState.Active) return;
            if (target.IsAlive == false) return;
            if (target.InCombat) return;

            if (target is GamePlayer)

                if (target.CurrentRegion.IsRvR == true)
                {
                    player.Out.SendMessage("After a short blaze up the flames expire. This spell does not work in RvR areas.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    return;
                }

            base.OnDirectEffect(target, effectiveness);

            double heal = ((target.MaxHealth / 100 * Spell.Value) * 0.1);//Spell.Value * effectiveness;


            //Message and stop the Handler at maxpercent state.
            if (player.IsCharcterClass(eCharacterClass.Vampiir)
                             || player.IsCharcterClass(eCharacterClass.MaulerHib)
                             || player.IsCharcterClass(eCharacterClass.MaulerMid)
                             || player.IsCharcterClass(eCharacterClass.MaulerAlb))
            {
                if (target.HealthPercent > 99 && target.EndurancePercent > 99 && target.ManaPercent > 14)
                {
                    return;
                }
                else
                {
                    if (target is GamePlayer)
                    {
                        (target as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((target as GamePlayer).Client.Account.Language, "CampfireSpellHander.YouFeelCalmAndHealthy"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                      
                    }
                }
            }
            else
            //Message and stop the Handler at maxpercent state.
            if (target.HealthPercent > 99 && target.EndurancePercent > 99 && target.ManaPercent > 99)
            {
                return;
            }
            else
            {
                if (target is GamePlayer)
                {
                    (target as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((target as GamePlayer).Client.Account.Language, "CampfireSpellHander.YouFeelCalmAndHealthy"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                
                }
            }

            //Health
            if (target.HealthPercent < 100)
            {
                target.Health -= (int)heal;
            }

            //endurance
            double endurance = ((target.MaxEndurance / 100 * Spell.Value) * 0.1);

            if (target.EndurancePercent < 100)
            {
                target.Endurance -= (int)endurance;
            }

            double mana = ((target.MaxMana / 100 * Spell.Value) * 0.1);
          
            //Vampiir and Maulers regen Power but only 15% of there Maxhealth 
            if (player.IsCharcterClass(eCharacterClass.Vampiir)
                             || player.IsCharcterClass(eCharacterClass.MaulerHib)
                             || player.IsCharcterClass(eCharacterClass.MaulerMid)
                             || player.IsCharcterClass(eCharacterClass.MaulerAlb))

           

            if (target.ManaPercent < 15)
            {
                target.Mana -= (int)mana;
            }

            else
            //Power
           // double mana = ((target.MaxMana / 100 * Spell.Value) * 0.1);

            if (target.ManaPercent < 100)
            {
                target.Mana -= (int)mana;
            }
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
            base.OnEffectExpires(effect, noMessages);
            if (!noMessages)
            {
                //"Your meditative state fades."
                MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
                //"{0}'s meditative state fades."
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
            }
            return 0;
        }


        // constructor
        public CampfireSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
