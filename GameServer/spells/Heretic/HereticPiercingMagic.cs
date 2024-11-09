using System;
using System.Collections;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Events;
namespace DOL.GS.Spells
{

    [SpellHandlerAttribute("HereticPiercingMagic")]
    public class HereticPiercingMagic : SpellHandler
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        protected GameLiving focustarget = null;
        protected ArrayList m_focusTargets = null;
        protected GameLiving effectOwner = null;
        public override void FinishSpellCast(GameLiving target)
        {
            base.FinishSpellCast(target);
            focustarget = target;

        }


        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
        }

        protected void LooseConscentration()
        {
            foreach (GamePlayer player in m_caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                if (player == null) continue;
               

                player.Out.SendInterruptAnimation(m_caster);

                // MessageToCaster("You lose your concentration!", eChatType.CT_SpellExpires);
                return;
            }
        }



        protected virtual void BeginEffect()
        {
           
            GameEventMgr.AddHandler(m_caster, GamePlayerEvent.EnemyKilled, new DOLEventHandler(EventActionEnemyKilled));
            GameEventMgr.AddHandler(m_caster, GamePlayerEvent.AttackFinished, new DOLEventHandler(EventAction));
            GameEventMgr.AddHandler(m_caster, GamePlayerEvent.CastStarting, new DOLEventHandler(EventAction));
            GameEventMgr.AddHandler(m_caster, GamePlayerEvent.Moving, new DOLEventHandler(EventAction));
            GameEventMgr.AddHandler(m_caster, GamePlayerEvent.Dying, new DOLEventHandler(EventAction));
            GameEventMgr.AddHandler(m_caster, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(EventAttackAction));
            GameEventMgr.AddHandler(m_caster, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(EventAttackAction));
            GameEventMgr.AddHandler(m_caster, GamePlayerEvent.ChangeTarget, new DOLEventHandler(EventAction));


            if (Spell.Radius > 0 && m_focusTargets != null)
            {

               

            }



            else if (m_caster.TargetObject == m_caster || m_caster.TargetObject == null || m_caster.TargetObject.IsObjectAlive == false || effectOwner != null && m_focusTargets.Contains(effectOwner) == false)
            {
               
                RemoveEffect();
                return;
            }
        }

        protected void EventAction(DOLEvent e, object sender, EventArgs args)
        {
            GameLiving player = sender as GameLiving;

            if (player == null) return;
            
            RemoveEffect();
        }


        protected void EventAttackAction(DOLEvent e, object sender, EventArgs args)
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
            
               
            
            if (Spell.Name != "Glistening Blaze" && Spell.Name != "Whirling Blaze" && Spell.Name != "Torrential Blaze")
            {
               
                RemoveEffect();
                              

                if (Spell.Pulse != 0 && Spell.Frequency > 0)
                {
                    CancelPulsingSpell(Caster, Spell.SpellType);
                }

            }

            else if (ad.AttackType == AttackData.eAttackType.MeleeDualWield || ad.AttackType == AttackData.eAttackType.MeleeOneHand || ad.AttackType == AttackData.eAttackType.MeleeTwoHand)
            {
                RemoveEffect();
                
                if (Spell.Pulse != 0 && Spell.Frequency > 0)
                {
                    CancelPulsingSpell(Caster, Spell.SpellType);
                }
            }
        }



        protected void EventActionFocusKill(DOLEvent e, object sender, EventArgs args)
        {
            GameLiving player = sender as GameLiving;

            if (player == null)
                return;
           
            RemoveEffect();
        }


        protected void EventActionEnemyKilled(DOLEvent e, object sender, EventArgs args)
        {
            GameLiving player = sender as GameLiving;

            if (player == null)
                return;

           

            if (effectOwner != null && Spell.Radius > 0)
            {
                if (Caster.TempProperties.getProperty<int>(HereticDamageSpeedDecreaseLostOnPulse.AETARGETS) <= 0)
                {
                   
                    GameSpellEffect effect = FindEffectOnTarget(effectOwner, this);
                    if (effect != null)
                    {
                        effect.Cancel(false);
                    }

                    GameSpellEffect DecreaseLOP = SpellHandler.FindEffectOnTarget(effectOwner, "HereticDamageSpeedDecreaseLOP");

                    if (DecreaseLOP != null)
                    {
                        DecreaseLOP.Cancel(false);
                    }

                }
                
            }
            if (Spell.Radius == 0)
            {
                if (effectOwner != null)
                {
                    GameSpellEffect effect = FindEffectOnTarget(effectOwner, this);
                    if (effect != null)
                    {
                        effect.Cancel(false);
                    }
                }
            }

            if (m_focusTargets != null && Spell.Radius > 0)
            {
               
                lock (m_focusTargets.SyncRoot)
                {
                    if (m_focusTargets.Count > 0 && effectOwner != null && m_focusTargets.Contains(effectOwner) && effectOwner.IsAlive == false)
                    {
                        m_focusTargets.Remove(effectOwner);
                        Caster.TempProperties.setProperty(HereticDamageSpeedDecreaseLostOnPulse.AETARGETS, m_focusTargets.Count);
                        return;


                    }
                    if (Caster.TempProperties.getProperty<int>(HereticDamageSpeedDecreaseLostOnPulse.AETARGETS) <= 0)
                    {
                        
                        RemoveEffect();
                        Caster.TempProperties.removeProperty(HereticDamageSpeedDecreaseLostOnPulse.AETARGETS);
                        CancelPulsingSpell(Caster, Spell.SpellType);
                    }
                }
            }
            if (Spell.Radius == 0)
            {
                RemoveEffect();
                CancelPulsingSpell(Caster, Spell.SpellType);

            }
        }




        protected virtual void RemoveEffect()
        {
           
            LooseConscentration();
            Caster.TempProperties.removeProperty(HereticDamageSpeedDecreaseLostOnPulse.AETARGETS);
            CancelPulsingSpell(Caster, Spell.SpellType);
            

            if (m_focusTargets != null)
            {
                lock (m_focusTargets.SyncRoot)
                {
                    foreach (GameLiving living in m_focusTargets)
                    {
                        if (living.IsAlive == false)
                        {
                            GameSpellEffect effect = FindEffectOnTarget(living, this);
                            if (effect != null)
                                effect.Cancel(false);


                            GameSpellEffect DamageSpeedDecrease = SpellHandler.FindEffectOnTarget(living, "HereticDamageSpeedDecreaseLOP");
                            if (DamageSpeedDecrease != null)
                            {
                                CancelPulsingSpell(living, "HereticDamageSpeedDecreaseLOP");

                            }
                        }
                    }
                }
                
                Caster.TempProperties.setProperty(HereticDamageSpeedDecreaseLostOnPulse.DAMAGE_UPDATE_TICK, 0);
               

                GameEventMgr.RemoveHandler(m_caster, GamePlayerEvent.AttackFinished, new DOLEventHandler(EventAction));
                GameEventMgr.RemoveHandler(m_caster, GamePlayerEvent.CastStarting, new DOLEventHandler(EventAction));
                GameEventMgr.RemoveHandler(m_caster, GamePlayerEvent.Moving, new DOLEventHandler(EventAction));
                GameEventMgr.RemoveHandler(m_caster, GamePlayerEvent.Dying, new DOLEventHandler(EventAction));
                GameEventMgr.RemoveHandler(m_caster, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(EventActionEnemyKilled));
                GameEventMgr.RemoveHandler(m_caster, GamePlayerEvent.ChangeTarget, new DOLEventHandler(EventAttackAction));
              
                return;

            }
        }
        public HereticPiercingMagic(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
