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
using DOL.AI.Brain;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using System;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Style speed decrease effect spell handler
    /// </summary>
    [SpellHandler("StyleSpeedDecrease")]
    public class StyleSpeedDecrease : SpeedDecreaseSpellHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override int CalculateSpellResistChance(GameLiving target)
        {
            if (target.EffectList.CountOfType(typeof(SpeedOfSoundEffect), typeof(ArmsLengthEffect), typeof(ChargeEffect)) > 0)
            {
                return 100;
            }
            return 0;
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

                        effectiveness = player.TempProperties.getProperty<double>(LOSEFFECTIVENESS + target.ObjectID, 1.0);


                        StartCombatSpeedDebuffEffects(target, effectiveness);

                        player.TempProperties.removeProperty(LOSEFFECTIVENESS + target.ObjectID);

                    }
                }
                catch (Exception e)
                {
                    if (log.IsErrorEnabled)
                        log.Error(string.Format("targetOID:{0} caster:{1} exception:{2}", targetOID.ToString(), Caster, e));
                }
            }
        }


        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {

            if (target == null) return;

            bool spellOK = true;

            if (Spell.Target.ToLower() == "cone" || (Spell.Target == "Enemy" && Spell.Radius > 0 && Spell.Range == 0))
            {
                spellOK = false;
            }

            if (spellOK == false || MustCheckLOS(Caster))
            {

                GamePlayer checkPlayer = null;
                if (target is GamePlayer)
                {
                    checkPlayer = target as GamePlayer;
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
                    checkPlayer.TempProperties.setProperty(LOSEFFECTIVENESS + target.ObjectID, effectiveness);
                    checkPlayer.Out.SendCheckLOS(Caster, target, new CheckLOSResponse(CheckSpellLOS));
                }
                else
                {

                    StartCombatSpeedDebuffEffects(target, effectiveness);
                }
            }
            else
            {

                StartCombatSpeedDebuffEffects(target, effectiveness);
            }
        }




        protected virtual int StartCombatSpeedDebuffEffects(GameLiving target, double effectiveness)
        {
            base.ApplyEffectOnTarget(target, effectiveness);

            // do damage even if immune to duration effect
            CalculateEffectDuration(target, effectiveness);

          

            return Spell.Duration;


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
            return 0;
        }

        // constructor
        public StyleSpeedDecrease(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
