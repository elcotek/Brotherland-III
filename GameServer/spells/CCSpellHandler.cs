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
using DOL.AI.Brain;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using DOL.GS.RealmAbilities;
using DOL.Language;
using System.Reflection;
using log4net;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Abstract CC spell handler
    /// </summary>
    public abstract class AbstractCCSpellHandler : ImmunityEffectSpellHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Apply effect on target or do spell action if non duration spell
        /// </summary>
        /// <param name="target">target that gets the effect</param>
        /// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            if (target.HasAbility(Abilities.CCImmunity))
            {
                if (Caster is GamePlayer)
                {
                    // MessageToCaster(target.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CCSpellHandler.TargetIsImmuneToThisEffect", target.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
                SendEffectAnimation(target, 0, false, 0);
                return;
            }
            if (target.EffectList.GetOfType<ChargeEffect>() != null || target.TempProperties.getProperty("Charging", false))
            {
                if (Caster is GamePlayer)
                {
                    //MessageToCaster(target.Name + " is moving too fast for this spell to have any effect!", eChatType.CT_SpellResisted);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CCSpellHandler.TargetIsMovingTooFast", target.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
                return;
            }

            base.ApplyEffectOnTarget(target, effectiveness);
        }

        /// <summary>
        /// When an applied effect starts
        /// duration spells only
        /// </summary>
        /// <param name="effect"></param>
        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);

            MessageToLiving(effect.Owner, Spell.Message1, eChatType.CT_Spell);
            MessageToCaster(Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, true)), eChatType.CT_Spell);
            Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, true)), eChatType.CT_Spell, effect.Owner, m_caster);

            GamePlayer player = effect.Owner as GamePlayer;
            if (player != null)
            {
                player.Client.Out.SendUpdateMaxSpeed();
                if (player.Group != null)
                    player.Group.UpdateMember(player, false, false);
            }
            else
            {
                effect.Owner.StopAttack();
                if (effect.Owner.IsMoving && effect.Owner is GameNPC)
                {
                    //log.Error("Stop moving");
                    (effect.Owner as GameNPC).StopMoving();
                }
            }

            effect.Owner.Notify(GameLivingEvent.CrowdControlled, effect.Owner);
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
            if (effect.Owner == null) return 0;

            base.OnEffectExpires(effect, noMessages);

            GamePlayer player = effect.Owner as GamePlayer;

            if (player != null)
            {
                player.Client.Out.SendUpdateMaxSpeed();
                if (player.Group != null)
                    player.Group.UpdateMember(player, false, false);
            }
            else
            {
                GameNPC npc = effect.Owner as GameNPC;
                if (npc != null)
                {
                    IOldAggressiveBrain aggroBrain = npc.Brain as IOldAggressiveBrain;
                    if (aggroBrain != null)
                        aggroBrain.AddToAggroList(Caster, 1);
                }
            }

            effect.Owner.Notify(GameLivingEvent.CrowdControlExpired, effect.Owner);

            return (effect.Name == "Pet Stun") ? 0 : 60000;
        }

        protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
        {
            eDamageType damageType = eDamageType.Body;
            if (Spell.DamageType > 0)
            {
                damageType = Spell.DamageType;
            }

            double duration = base.CalculateEffectDuration(target, effectiveness);
            duration -= duration * target.GetResist(damageType) * 0.01;


            double mocFactor = 1.0;


            MasteryofConcentrationEffect moc = Caster.EffectList.GetOfType<MasteryofConcentrationEffect>();
            if (moc != null)
            {
                MasteryofConcentrationAbility ra = Caster.GetAbility<MasteryofConcentrationAbility>();
                if (ra != null)
                    mocFactor += ra.GetAmountForLevel(ra.Level) / 100.0;
                duration = (double)Math.Round(duration * mocFactor);
            }


            if (Spell.SpellType.ToLower() != "stylestun")
            {
                // capping duration adjustment to 100%, live cap unknown - Tolakram
                int hitChance = Math.Min(200, CalculateToHitChance(target));

                if (hitChance <= 0)
                {
                    duration = 0;
                }
                else if (hitChance < 55)
                {
                    duration -= (int)(duration * (55 - hitChance) * 0.01);
                }
                else if (hitChance > 100)
                {
                    duration += (int)(duration * (hitChance - 100) * 0.01);
                }
            }

            if (target is GameNPC && target.Level > Caster.Level)
            {
                double levelfactor = 1.00;
                levelfactor -= 0.03 * (target.Level - Caster.Level);
                if (levelfactor < 0.10) levelfactor = 0.10;
                duration *= levelfactor;
            }

            return (int)duration;
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            int resistvalue = 0;
            int resist = 0;
            GameSpellEffect fury = SpellHandler.FindEffectOnTarget(target, "Fury");
            if (fury != null)
            {
                resist += (int)fury.Spell.Value;
            }

            //bonedancer rr5
            if (target.EffectList.GetOfType<AllureofDeathEffect>() != null)
            {
                return AllureofDeathEffect.ccchance;
            }
            if (target.EffectList.GetOfType<FerociousWillEffect>() != null)
            {
                resist += 25;
            }
            if (m_spellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect)
                return 0;
            if (HasPositiveEffect)
                return 0;

            int hitchance = CalculateToHitChance(target);

            //Calculate the Resistchance
            resistvalue = (100 - hitchance + resist);
            if (resistvalue > 100)
                resistvalue = 100;
            //use ResurrectHealth=1 if the CC should not be resisted
            if (Spell.ResurrectHealth == 1) resistvalue = 0;
            //always 1% resistchance!
            else if (resistvalue < 1)
                resistvalue = 1;
            return resistvalue;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="spell"></param>
        /// <param name="line"></param>
        public AbstractCCSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// Mezz
    /// </summary>
    [SpellHandlerAttribute("Mesmerize")]
    public class MesmerizeSpellHandler : AbstractCCSpellHandler
    {
        public override void OnEffectPulse(GameSpellEffect effect)
        {
            SendEffectAnimation(effect.Owner, 0, false, 1);
            base.OnEffectPulse(effect);
        }

        public override bool CheckDuringCast(GameLiving target)
        {
            bool valid = base.CheckDuringCast(target);

            if (Spell.Pulse != 0 //less checks for pulsing mez
                && target != null
                && GameServer.ServerRules.IsAllowedToAttack(Caster, target, true)
                && Caster.IsWithinRadius(target, CalculateSpellRange() * 2))
                return true;

            else return valid;
        }

        public override bool CheckEndCast(GameLiving target)
        {
            bool valid = base.CheckEndCast(target);

            if (Spell.Pulse != 0 //less checks for pulsing mez
                && target != null
                && GameServer.ServerRules.IsAllowedToAttack(Caster, target, true)
                && Caster.IsWithinRadius(target, CalculateSpellRange() * 2))
                return true;

            else return valid;
        }

        public override bool CheckAfterCast(GameLiving target)
        {
            bool valid = base.CheckAfterCast(target);

            if (Spell.Pulse != 0 //less checks for pulsing mez
                && target != null
                && GameServer.ServerRules.IsAllowedToAttack(Caster, target, true)
                && Caster.IsWithinRadius(target, CalculateSpellRange() * 2))
                return true;

            else return valid;
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            effect.Owner.IsMezzed = true;
            //effect.Owner.StopAttack();
            effect.Owner.StopCurrentSpellcast();
            effect.Owner.DisableTurning(true);
            GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
            base.OnEffectStart(effect);
        }

        /// <summary>
        /// Variance is max 50% for players, none for mobs
        /// </summary>
        /// <param name="target">target to calculate variance for</param>
        /// <param name="distance">distance from the target the spell was cast on</param>
        /// <param name="radius">radius of the spell</param>
        /// <returns>amount to subtract from effectiveness</returns>
        protected override double CalculateAreaVariance(GameLiving target, int distance, int radius)
        {
            if (target is GamePlayer || (target is GameNPC && (target as GameNPC).Brain is IControlledBrain))
            {
                return ((double)distance / (double)radius) / 2.0;
            }

            return 0;
        }




        //If mez resisted, just rupt, dont demez
        protected override void OnSpellResisted(GameLiving target)
        {
            //SendEffectAnimation(target, 0, false, 0);
            /*
            if (Caster is GamePlayer)
            {
                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CCSpellHandler.TargetRessistEffect", target.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                // MessageToCaster("You stop playing your song.", eChatType.CT_Spell);
                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CCSpellHandler.StopPlayingSong"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
            }
            */
            //effect.Cancel(false);//call OnEffectExpires
            if (Spell.Pulse != 0)
                CancelPulsingSpell(Caster, Spell.SpellType);
            //MessageToCaster(target.GetName(0, true) + " resists the effect!", eChatType.CT_SpellResisted);
            // target.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
            base.OnSpellResisted(target);
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
            GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
            effect.Owner.IsMezzed = false;
            effect.Owner.DisableTurning(false);
            return base.OnEffectExpires(effect, noMessages);
        }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            if (Spell.Pulse != 0 && !base.CheckDuringCast(target, false))
                return;

            GameSpellEffect DecreaseLOP = SpellHandler.FindEffectOnTarget(target, "HereticDamageSpeedDecreaseLOP");
            if (DecreaseLOP != null)
            {
                //if player have SpeedDecrease effect we remove the SpeedDecrease
                DecreaseLOP.Cancel(false);
            }

            if (FindStaticEffectOnTarget(target, typeof(MezzRootImmunityEffect)) != null || target.HasAbility(Abilities.CCImmunity))
            {
                if (Caster is GamePlayer)
                {
                    // MessageToCaster(target.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CCSpellHandler.TargetIsImmuneToThisEffect", target.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
                SendEffectAnimation(target, 0, false, 0);
                return;
            }
            if (target.EffectList.GetOfType<ChargeEffect>() != null || target.EffectList.GetOfType<SpeedOfSoundEffect>() != null || target.TempProperties.getProperty("Charging", false))
            {
                if (Caster is GamePlayer)
                {
                    //MessageToCaster(target.Name + " is moving too fast for this spell to have any effect!", eChatType.CT_SpellResisted);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CCSpellHandler.TargetIsMovingTooFast", target.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
                return;
            }
            GameSpellEffect mezz = SpellHandler.FindEffectOnTarget(target, Spell.SpellType);
            if (mezz != null)
            {
                if (Caster is GamePlayer)
                {
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CCSpellHandler.YourTargetIsAlreadyMezzed"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
                //MessageToCaster("Your target is already mezzed!", eChatType.CT_SpellResisted);
                SendEffectAnimation(target, 0, false, 0);
                return;
            }

            base.ApplyEffectOnTarget(target, effectiveness);
        }
        /// <summary>
        /// Calculates the effect duration in milliseconds
        /// </summary>
        /// <param name="target">The effect target</param>
        /// <param name="effectiveness">The effect effectiveness</param>
        /// <returns>The effect duration in milliseconds</returns>
        protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
        {
            double duration = base.CalculateEffectDuration(target, effectiveness);
            duration *= target.GetModified(eProperty.MesmerizeDurationReduction) * 0.01;
            if (duration < 1)
                duration = 1;
            else if (duration > (Spell.Duration * 4))
                duration = (Spell.Duration * 4);
            return (int)duration;
        }

        protected virtual void OnAttacked(DOLEvent e, object sender, EventArgs arguments)
        {
            AttackedByEnemyEventArgs attackArgs = arguments as AttackedByEnemyEventArgs;
            GameLiving living = sender as GameLiving;
            if (attackArgs == null) return;
            if (living == null) return;

            bool remove = false;

            if (attackArgs.AttackData.AttackType != AttackData.eAttackType.Spell)
            {
                switch (attackArgs.AttackData.AttackResult)
                {
                    case GameLiving.eAttackResult.HitStyle:
                    case GameLiving.eAttackResult.HitUnstyled:
                    case GameLiving.eAttackResult.Blocked:
                    case GameLiving.eAttackResult.Evaded:
                    case GameLiving.eAttackResult.Fumbled:
                    case GameLiving.eAttackResult.Missed:
                    case GameLiving.eAttackResult.Parried:
                        remove = true;
                        break;
                }
            }
            //If the spell was resisted - then we don't break mezz
            else if (!attackArgs.AttackData.IsSpellResisted)
            {
                //temporary fix for DirectDamageDebuff not breaking mez
                if (attackArgs.AttackData.SpellHandler is PropertyChangingSpell && attackArgs.AttackData.SpellHandler.HasPositiveEffect == false && attackArgs.AttackData.Damage > 0)
                    remove = true;
                //debuffs/shears dont interrupt mez, neither does recasting mez
                else if (attackArgs.AttackData.SpellHandler is PropertyChangingSpell || attackArgs.AttackData.SpellHandler is MesmerizeSpellHandler
                         || attackArgs.AttackData.SpellHandler is NearsightSpellHandler || attackArgs.AttackData.SpellHandler.HasPositiveEffect) return;

                if (attackArgs.AttackData.AttackResult == GameLiving.eAttackResult.Missed || attackArgs.AttackData.AttackResult == GameLiving.eAttackResult.HitUnstyled)
                    remove = true;
            }

            if (remove)
            {
                GameSpellEffect effect = SpellHandler.FindEffectOnTarget(living, this);
                if (effect != null)
                    effect.Cancel(false);//call OnEffectExpires
            }
        }

        // constructor
        public MesmerizeSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    /// <summary>
    /// Stun
    /// </summary>
    [SpellHandlerAttribute("Stun")]
    public class StunSpellHandler : AbstractCCSpellHandler
    {
       
        protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
        {

            GameSpellEffect speed = FindEffectOnTarget(target, "SpeedEnhancement");

            if (speed != null)

                speed.Cancel(false);

            GameSpellEffect DecreaseLOP = SpellHandler.FindEffectOnTarget(target, "HereticDamageSpeedDecreaseLOP");
            if (DecreaseLOP != null)
            {
                //if player have SpeedDecrease effect we remove the SpeedDecrease
                DecreaseLOP.Cancel(false);
            }

            GameSpellEffect mes = SpellHandler.FindEffectOnTarget(target, "Mesmerize");
            if (mes != null && target.IsMezzed)
            {
                //if player have mez effect we remove mez
                mes.Cancel(false);
            }
            GameSpellEffect aemes = SpellHandler.FindEffectOnTarget(target, "AOEMesmerize");
            if (aemes != null && target.IsMezzed)
            {
                //if player have mez effect we remove mez
                aemes.Cancel(false);
            }

            //use ResurrectMana=1 if the Stun should not have immunity
            if (Spell.ResurrectMana == 1)
            {
                int freq = Spell != null ? Spell.Frequency : 0;
                return new GameSpellEffect(this, CalculateEffectDuration(target, effectiveness), freq, effectiveness);
            }
            else return new GameSpellAndImmunityEffect(this, CalculateEffectDuration(target, effectiveness), 0, effectiveness);
        }

        /// <summary>
        /// Variance is max 50% for players, none for mobs
        /// </summary>
        /// <param name="target">target to calculate variance for</param>
        /// <param name="distance">distance from the target the spell was cast on</param>
        /// <param name="radius">radius of the spell</param>
        /// <returns>amount to subtract from effectiveness</returns>
        protected override double CalculateAreaVariance(GameLiving target, int distance, int radius)
        {
            if (target is GamePlayer || (target is GameNPC && (target as GameNPC).Brain is IControlledBrain))
            {
                return ((double)distance / (double)radius) / 2.0;
            }

            return 0;
        }


        public override void OnEffectStart(GameSpellEffect effect)
        {
            effect.Owner.IsStunned = true;
            //effect.Owner.StopAttack();
           if (effect.Owner is GameNPC)
                ((GameNPC)effect.Owner).PauseCurrentSpellCast(m_caster);
            else effect.Owner.StopCurrentSpellcast();
            effect.Owner.DisableTurning(true);
            base.OnEffectStart(effect);
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
            effect.Owner.IsStunned = false;
            effect.Owner.DisableTurning(false);
            //use ResurrectHealth>0 to calculate stun immunity timer (such pet stun spells), actually (1.90) pet stun immunity is 5x the stun duration, 99 means no imun
            if (Spell.ResurrectHealth == 99)
            {
                base.OnEffectExpires(effect, noMessages);
                return 0;
            }
            else if (Spell.ResurrectHealth > 0)
            {
                base.OnEffectExpires(effect, noMessages);
                return Spell.Duration * Spell.ResurrectHealth;
            }
            return base.OnEffectExpires(effect, noMessages);
        }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GameSpellEffect stunblock = SpellHandler.FindEffectOnTarget(target, "CeremonialBracerStun");

            //Ceremonial Bracer Stun Feedback
            if (stunblock != null && target != null)
            {

                {
                    if (Caster is GamePlayer)
                    {
                        (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CCSpellHandler.EnemyItemEffectInterceptsTheMez"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                    if (target is GamePlayer)
                    {
                        (target as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((target as GamePlayer).Client.Account.Language, "CCSpellHandler.YourItemEffectInterceptsTheStun"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }


                }

                stunblock.Cancel(false);
                base.OnSpellResisted(target);
                return;
            }

            if (target.HasAbility(Abilities.StunImmunity))
            {
                if (Caster is GamePlayer)
                {
                    //MessageToCaster(target.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CCSpellHandler.TargetIsImmuneToThisEffect", target.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
                base.OnSpellResisted(target);
                return;
            }
        

            base.ApplyEffectOnTarget(target, effectiveness);
        }


        /// <summary>
        /// Calculates the effect duration in milliseconds
        /// </summary>
        /// <param name="target">The effect target</param>
        /// <param name="effectiveness">The effect effectiveness</param>
        /// <returns>The effect duration in milliseconds</returns>
        protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
        {
            double duration = base.CalculateEffectDuration(target, effectiveness);
            duration *= target.GetModified(eProperty.StunDurationReduction) * 0.01;

            if (duration < 1)
                duration = 1;
            else if (duration > (Spell.Duration * 4))
                duration = (Spell.Duration * 4);
            return (int)duration;
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
            if (Spell.EffectGroup != 0)
                return Spell.EffectGroup == compare.Spell.EffectGroup;
            if (compare.Spell.SpellType == "StyleStun") return true;
            return base.IsOverwritable(compare);
        }

        // constructor
        public StunSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
