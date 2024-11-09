
using System;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using System.Text;
using DOL.AI.Brain;
using DOL.GS.RealmAbilities;

namespace DOL.GS.Spells
{
    public abstract class HereticImmunityEffectSpellHandler : HereticPiercingMagic
    {

        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        const string SpeedDecrease_status = "SPEEDDERCREASE_STATUS";


        /// <summary>
        /// called when spell effect has to be started and applied to targets
        /// </summary>
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            

            GameEventMgr.AddHandler(m_caster, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(EventActionFocus));
            GameEventMgr.AddHandler(m_caster, GamePlayerEvent.Moving, new DOLEventHandler(EventActionFocus));



            Caster.TempProperties.setProperty(SpeedDecrease_status, false);



            base.FinishSpellCast(target);
        }

        /// <summary>
        /// Determines wether this spell is better than given one
        /// </summary>
        /// <param name="oldeffect"></param>
        /// <param name="neweffect"></param>
        /// <returns></returns>
        public override bool IsNewEffectBetter(GameSpellEffect oldeffect, GameSpellEffect neweffect)
        {
            return true;
            //if (oldeffect.Owner is GamePlayer) return false; //no overwrite for players
            //return base.IsNewEffectBetter(oldeffect, neweffect);
        }


        /// <summary>
        /// Apply effect on target or do spell action if non duration spell
        /// </summary>
        /// <param name="target">target that gets the effect</param>
        /// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {

            if (target.Realm == 0 || Caster.Realm == 0)
            {
                target.LastAttackedByEnemyTickPvE = target.CurrentRegion.Time;
                Caster.LastAttackTickPvE = Caster.CurrentRegion.Time;
            }
            else
            {
                target.LastAttackedByEnemyTickPvP = target.CurrentRegion.Time;
                Caster.LastAttackTickPvP = Caster.CurrentRegion.Time;
            }
            
                 GameSpellEffect Zephyr = SpellHandler.FindEffectOnTarget(target, "Zephyr");
            if (Zephyr != null)
            {
                MessageToCaster(target.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
                return;
            }

            GameSpellEffect Phaseshift = SpellHandler.FindEffectOnTarget(target, "Phaseshift");
            if (Phaseshift != null)
            {
                MessageToCaster(target.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
                return;
            }
            SpeedOfSoundEffect SpeedOfSoundEffect = target.EffectList.GetOfType<SpeedOfSoundEffect>();
            if (SpeedOfSoundEffect != null)
            {
                MessageToCaster(target.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
                return;
            }

            if (target.HasAbility(Abilities.CCImmunity))
            {
                MessageToCaster(target.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
                return;
            }
            if (target.TempProperties.getProperty("Charging", false))
            {
                MessageToCaster(target.Name + " is moving to fast for this spell to have any effect!", eChatType.CT_SpellResisted);
                return;
            }

                                
            if (!m_caster.IsWithinRadius(target, CalculateSpellRange()))
            {
                GameSpellEffect DecreaseLOP = SpellHandler.FindEffectOnTarget(target, "HereticDamageSpeedDecreaseLOP");


                if (DecreaseLOP != null)
                {
                    DecreaseLOP.Cancel(false);
                }

                RemoveEffect();
                //return;
            }

            if (Spell.Radius > 0 && m_focusTargets != null)
            {

                if (m_focusTargets.Count <= 0)
                {
                    RemoveEffect();
                 
                }
            }

            if (target is GameNPC)
            {
                GameNPC npc = (GameNPC)target;
                IOldAggressiveBrain aggroBrain = npc.Brain as IOldAggressiveBrain;
                if (aggroBrain != null)
                    aggroBrain.AddToAggroList(Caster, 1);
            }


            if (Caster.TempProperties.getProperty<bool>(SpeedDecrease_status) == false)
            {
                //Wichtig!!!
                base.ApplyEffectOnTarget(target, effectiveness);
                base.OnDurationEffectApply(target, effectiveness);
            }

        }



        /// <summary>
        /// Focus Action for brakeable and none breakeabel spells
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void EventActionFocus(DOLEvent e, object sender, EventArgs args)
        {
           
            GameLiving player = sender as GameLiving;
            AttackedByEnemyEventArgs argument = args as AttackedByEnemyEventArgs;
            AttackData ad = null;
            if (player == null) return;

            if (argument != null)
            {
                ad = argument.AttackData;
            }
            if (ad == null)
            {
                return;
            }

            if (m_focusTargets != null)
            {
                lock (m_focusTargets.SyncRoot)
                {
                    foreach (GameLiving living in m_focusTargets)
                    {
                        {
                           
                            GameSpellEffect effect = FindEffectOnTarget(living, this);
                            if (effect != null)
                                effect.Cancel(false);

                            GameSpellEffect DecreaseLOP = SpellHandler.FindEffectOnTarget(effectOwner, "HereticDamageSpeedDecreaseLOP");

                            if (DecreaseLOP != null)
                            {
                                DecreaseLOP.Cancel(false);
                            }
                        }
                    }
                }
            }
            if (Spell.Name != "Glistening Blaze" && Spell.Name != "Whirling Blaze" && Spell.Name != "Torrential Blaze")
            {
                RemoveEffect();

                if (Spell.Pulse != 0 && Spell.Frequency > 0)
                {
                    CancelPulsingSpell(Caster, Spell.SpellType);

                }
                Caster.TempProperties.setProperty(SpeedDecrease_status, true);


                GameEventMgr.RemoveHandler(m_caster, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(EventActionFocus));
                GameEventMgr.RemoveHandler(m_caster, GamePlayerEvent.Moving, new DOLEventHandler(EventActionFocus));
                //GameEventMgr.RemoveHandler(m_caster, GamePlayerEvent.EnemyKilled, new DOLEventHandler(RemoveEffectAfterKill));

            }

            else if (ad.AttackType == AttackData.eAttackType.MeleeDualWield || ad.AttackType == AttackData.eAttackType.MeleeOneHand || ad.AttackType == AttackData.eAttackType.MeleeTwoHand)
            {

                RemoveEffect();

                if (Spell.Pulse != 0 && Spell.Frequency > 0)
                {
                    CancelPulsingSpell(Caster, Spell.SpellType);

                }
                Caster.TempProperties.setProperty(SpeedDecrease_status, true);


                GameEventMgr.RemoveHandler(m_caster, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(EventActionFocus));
                GameEventMgr.RemoveHandler(m_caster, GamePlayerEvent.Moving, new DOLEventHandler(EventActionFocus));
            }
        }

        protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
        {
            double duration = base.CalculateEffectDuration(target, effectiveness);
            double mocFactor = 1.0;
            MasteryofConcentrationEffect moc = Caster.EffectList.GetOfType<MasteryofConcentrationEffect>();
            if (moc != null)
            {
                MasteryofConcentrationAbility ra = Caster.GetAbility<MasteryofConcentrationAbility>();
                if (ra != null)
                    mocFactor = System.Math.Round((double)ra.GetAmountForLevel(ra.Level) * 25 / 100, 2);
                duration = (double)Math.Round(duration * mocFactor);
            }


          
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
          

            if (target is GameNPC && target.Level > Caster.Level)
            {
                double levelfactor = 1.00;
                levelfactor -= 0.03 * (target.Level - Caster.Level);
                if (levelfactor < 0.10) levelfactor = 0.10;
                duration *= levelfactor;
            }

            return (int)duration;
        }
        

        /// <summary>
        /// Creates the corresponding spell effect for the spell
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectiveness"></param>
        /// <returns></returns>
        protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
        {
            return new GameSpellAndImmunityEffect(this, (int)CalculateEffectDuration(target, effectiveness), 0, effectiveness);
        }

        // constructor
        public HereticImmunityEffectSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) { }
    }

    [SpellHandler("HereticSpeedDecrease")]
    public class HereticSpeedDecreaseSpellHandler : HereticImmunityEffectSpellHandler
    {
        private readonly object TIMER_PROPERTY;
        private const string EFFECT_PROPERTY = "Effect";

        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);

            effect.Owner.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, effect, 1.0 - Spell.Value * 0.01);

            SendUpdates(effect.Owner);

            MessageToLiving(effect.Owner, Spell.Message1, eChatType.CT_Spell);
            Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, true)), eChatType.CT_Spell, effect.Owner);

            RestoreSpeedTimer timer = new RestoreSpeedTimer(effect);
            effect.Owner.TempProperties.setProperty(effect, timer);

            //REVOIR
            timer.Interval = 650;
            timer.Start(1 + (effect.Duration >> 1));

            //effect.Owner.StartInterruptTimer(effect.Owner.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
        }

        

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            base.OnEffectExpires(effect, noMessages);

            GameTimer timer = (GameTimer)effect.Owner.TempProperties.getProperty<object>(effect, null);
            effect.Owner.TempProperties.removeProperty(effect);
            timer.Stop();

            effect.Owner.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, effect);

            SendUpdates(effect.Owner);

            MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
            Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, true)), eChatType.CT_SpellExpires, effect.Owner);

            return 60000;
        }


        protected virtual void OnAttacked(DOLEvent e, object sender, EventArgs arguments)
        {
            AttackedByEnemyEventArgs attackArgs = arguments as AttackedByEnemyEventArgs;
            GameLiving living = sender as GameLiving;
            if (attackArgs == null) return;
            if (living == null) return;


            GameSpellEffect effect = FindEffectOnTarget(living, this);
            if (attackArgs.AttackData.Damage > 0)
            {
                if (effect != null)
                    effect.Cancel(false);
            }

            if (attackArgs.AttackData.SpellHandler is StyleBleeding || attackArgs.AttackData.SpellHandler is DoTSpellHandler || attackArgs.AttackData.SpellHandler is HereticDoTSpellHandler)
            {
                GameSpellEffect affect = FindEffectOnTarget(living, this);
                if (affect != null)
                    affect.Cancel(false);
            }
        }


        /// <summary>
        /// Sends updates on effect start/stop
        /// </summary>
        /// <param name="owner"></param>
        protected static void SendUpdates(GameLiving owner)
        {
            if (owner.IsMezzed || owner.IsStunned)
                return;

            owner.UpdateMaxSpeed();
        }


        protected HereticSpeedDecreaseSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line)
        {
            TIMER_PROPERTY = this;
        }

        /// <summary>
        /// Slowly restores the livings speed
        /// </summary>
        private sealed class RestoreSpeedTimer : GameTimer
        {
            /// <summary>
            /// The speed changing effect
            /// </summary>
            private readonly GameSpellEffect m_effect;

            /// <summary>
            /// Constructs a new RestoreSpeedTimer
            /// </summary>
            /// <param name="effect">The speed changing effect</param>
            public RestoreSpeedTimer(GameSpellEffect effect) : base(effect.Owner.CurrentRegion.TimeManager)
            {
                m_effect = effect;
            }

            /// <summary>
            /// Called on every timer tick
            /// </summary>
            protected override void OnTick()
            {
                GameSpellEffect effect = m_effect;

                double factor = 2.0 - (effect.Duration - effect.RemainingTime) / (double)(effect.Duration >> 1);
                if (factor < 0) factor = 0;
                else if (factor > 1) factor = 1;

                effect.Owner.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, effect, 1.0 - effect.Spell.Value * factor * 0.01);

                SendUpdates(effect.Owner);

                if (factor <= 0)
                    Stop();
            }


            /// <summary>
            /// Returns short information about the timer
            /// </summary>
            /// <returns>Short info about the timer</returns>
            public override string ToString()
            {
                return new StringBuilder(base.ToString())
                    .Append(" SpeedDecreaseEffect: (").Append(m_effect.ToString()).Append(')')
                    .ToString();
            }
        }
    }
}
