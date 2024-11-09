using System;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.AI.Brain;
using DOL.Language;
using DOL.GS.ServerProperties;
using System.Collections;

namespace DOL.GS.Spells
{

    [SpellHandlerAttribute("HereticDamageSpeedDecreaseLOP")]
    public class HereticDamageSpeedDecreaseLostOnPulse : HereticSpeedDecreaseSpellHandler
    {
      
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public const string DAMAGE_UPDATE_TICK = "damage_Update_Tick";
        const string LAST_MANA_TICK = "last_mana_Tick";
        private const string LOSEFFECTIVENESS = "LOS Effectivness";
        public const string AETARGETS= "AE Targets";

        protected int m_lastdamage = 0;
        protected int m_lastCapdamage = 0;
        protected int m_pulsedamage = 0;
        //    protected int m_pulsecount = -1;

        public override void FinishSpellCast(GameLiving target)
        {
            BeginEffect();
            Caster.TempProperties.setProperty(DAMAGE_UPDATE_TICK, 0);
            base.FinishSpellCast(target);
        }

        public override double GetLevelModFactor()
        {
            return 0;
        }

        public override bool IsOverwritable(GameSpellEffect compare)
        {
            //if (base.IsOverwritable(compare) == false) return false;
            //if (compare.Spell.Duration != Spell.Duration) return false;
            return true;
        }

        public override AttackData CalculateDamageToTarget(GameLiving target, double effectiveness)
        {
            AttackData ad = base.CalculateDamageToTarget(target, effectiveness);
            ad.CriticalDamage = 0;
            ad.AttackType = AttackData.eAttackType.Unknown;
            return ad;
        }


        public override void CalculateDamageVariance(GameLiving target, out double min, out double max)
        {
            int speclevel = 1;
            if (m_caster is GamePlayer)
            {
                speclevel = ((GamePlayer)m_caster).GetModifiedSpecLevel(m_spellLine.Spec);
            }
            min = 1;
            max = 1;

            if (target.Level > 0)
            {
                min = 0.5 + (speclevel - 1) / (double)target.Level * 0.5;
            }

            if (speclevel - 1 > target.Level)
            {
                double overspecBonus = (speclevel - 1 - target.Level) * 0.005;
                min += overspecBonus;
                max += overspecBonus;
            }

            if (min > max) min = max;
            if (min < 0) min = 0;
        }


        protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
        {

            base.CreateSpellEffect(target, effectiveness);
            // damage is not reduced with distance
            return new GameSpellEffect(this, m_spell.Duration, m_spellLine.IsBaseLine ? 3000 : 2000, 1);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {

            base.OnEffectStart(effect);
            SendEffectAnimation(effect.Owner, 0, false, 1);
        }




        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            base.OnEffectExpires(effect, noMessages);
            if (!noMessages)
            {

                
                if (Caster.TempProperties.getProperty<int>(HereticDamageSpeedDecreaseLostOnPulse.AETARGETS) <= 0)
                {
                    
                    Caster.TempProperties.removeProperty(AETARGETS);
                    Caster.TempProperties.setProperty(DAMAGE_UPDATE_TICK, 0);
                     RemoveEffect();
                }

                 MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);

                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
            }

            return 0;
        }


        /// <summary>
        /// execute direct effect
        /// </summary>
        /// <param name="target">target that gets the damage</param>
        /// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {


            if (target == null) return;

            bool spellOK = true;


            if (m_focusTargets == null)
                m_focusTargets = new ArrayList();

            GameLiving living = target as GameLiving;
            effectOwner = living;



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
                    checkPlayer.Out.SendCheckLOS(Caster, target, new CheckLOSResponse(DealDamageCheckLOS));
                }
                else
                {
                    if (Spell.Radius > 0)
                    {
                        lock (m_focusTargets.SyncRoot)
                        {
                           
                            if (!m_focusTargets.Contains(effectOwner) && target.IsAlive)
                                m_focusTargets.Add(effectOwner);
                                Caster.TempProperties.setProperty(AETARGETS, m_focusTargets.Count);

                        }
                    }
                    DealDamage(target, effectiveness);
                    
                }
            }
            else
            {
                if (Spell.Radius > 0)
                {
                    lock (m_focusTargets.SyncRoot)
                    {
                        if (!m_focusTargets.Contains(effectOwner) && target.IsAlive)
                            m_focusTargets.Add(effectOwner);
                            Caster.TempProperties.setProperty(AETARGETS, m_focusTargets.Count);

                    }
                }
                
                DealDamage(target, effectiveness);
            }
        }

        protected static int CalculateDamageForLevel(AttackData ad, int damage)
        {
            double conLevel;
            int finalDamage = damage;
            

            conLevel = GameObject.GetConLevel(ad.Attacker.Level, ad.Target.Level);


            // Console.WriteLine("conlevel2 = {0}", conLevel);

            if (conLevel > 0)
            {

                finalDamage -= Convert.ToInt16(damage * conLevel * .1);
               
                return finalDamage;
            }

            return finalDamage;
        }


        private void DealDamage(GameLiving target, double effectiveness)
        {
            if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;

            // calc damage for HereticDamageSpeedDecreaseLOP
            AttackData ad = CalculateDamageToTarget(target, effectiveness);


            if (ad.Damage < Caster.TempProperties.getProperty<int>(DAMAGE_UPDATE_TICK))
            {
                Caster.TempProperties.setProperty(DAMAGE_UPDATE_TICK, ad.Damage);
            }
            if (m_lastdamage <= 0)
            {
                m_lastdamage = ad.Damage;
              
            }
            else
            {

                if (Spell.AmnesiaChance > 0)
                {
                    m_pulsedamage = Convert.ToInt32(m_lastdamage * (Spell.AmnesiaChance * 0.1));
                }
                else
                {
                   
                    m_pulsedamage = Convert.ToInt32(m_lastdamage * Properties.HERETIC_AE_TICK_DAMAGE);

                }


                if (target == focustarget)

                    m_lastdamage += m_pulsedamage;


                if (capForDamage == true)
                {
                   
                    m_lastCapdamage = Caster.TempProperties.getProperty<int>(DAMAGE_UPDATE_TICK);
                    Caster.TempProperties.setProperty(DAMAGE_UPDATE_TICK, m_lastCapdamage);
                }
                else if (capForDamage == false)
                {
                   
                    Caster.TempProperties.setProperty(DAMAGE_UPDATE_TICK, m_lastdamage);
                }



                if (m_lastdamage > 0)
                {


                    int m_damage = Caster.TempProperties.getProperty<int>(DAMAGE_UPDATE_TICK);

                    if (m_damage >= 1099)
                    {
                        Caster.TempProperties.setProperty(DAMAGE_UPDATE_TICK, 1099);
                    }
                }
            }
            int Lastdamage = CalculateDamageForLevel(ad, Caster.TempProperties.getProperty<int>(DAMAGE_UPDATE_TICK));

           
            ad.Damage = Lastdamage;

            SendEffectAnimation(target, 0, false, 1);
           
            if (Lastdamage > 0)
            {
               
                DamageTarget(ad, true, false);
                SendDamageMessages(ad);
            }
            target.StartInterruptTimer(target.SpellInterruptDuration, ad.AttackType, Caster);
        }

        private bool CheckLOS(GameLiving living)
        {
            foreach (AbstractArea area in living.CurrentAreas)
            {
                if (area.CheckLOS)
                    return true;
            }
            return false;
        }

        private void DealDamageCheckLOS(GamePlayer player, ushort response, ushort targetOID)
        {
            GameLiving target = null;

            if (player == null || Caster.ObjectState != GameObject.eObjectState.Active)
                return;

            if ((response & 0x100) == 0x100)
            {
                try
                {
                    if (Caster != null && Caster.ObjectState == GameLiving.eObjectState.Active && Caster.CurrentRegion.GetObject(targetOID) is GameLiving)

                        target = (Caster.CurrentRegion.GetObject(targetOID) as GameLiving);

                    if (target != null && target.ObjectState == GameLiving.eObjectState.Active)
                    {

                        double effectiveness = player.TempProperties.getProperty<double>(LOSEFFECTIVENESS + target.ObjectID, 1.0);

                        DealDamage(target, effectiveness);
                        player.TempProperties.removeProperty(LOSEFFECTIVENESS + target.ObjectID);

                    }
                }
                catch (Exception e)
                {
                    if (log.IsErrorEnabled)
                        log.Error(string.Format("targetOID:{0} caster:{1} exception:{2}", targetOID, Caster, e));
                }
            }
            else
            {
                if (Spell.Radius > 0)
                {
                  
                    RemoveEffect();
                  
                    try
                    {

                        if (Caster != null && Caster.ObjectState == GameLiving.eObjectState.Active && Caster.CurrentRegion.GetObject(targetOID) is GameLiving)

                            target = (Caster.CurrentRegion.GetObject(targetOID) as GameLiving);

                        if (target != null && target.ObjectState == GameLiving.eObjectState.Active)
                        {

                            double effectiveness = player.TempProperties.getProperty<double>(LOSEFFECTIVENESS + target.ObjectID, 1.0);

                            GameSpellEffect HereticDamageSpeedDecreaseLOP = FindEffectOnTarget(target, "HereticDamageSpeedDecreaseLOP");
                          
                            if (HereticDamageSpeedDecreaseLOP != null)
                            {
                                HereticDamageSpeedDecreaseLOP.Cancel(false);
                               
                            }
                           


                            player.TempProperties.removeProperty(LOSEFFECTIVENESS + target.ObjectID);

                        }
                    }
                    catch (Exception e)
                    {
                        if (log.IsErrorEnabled)
                            log.Error(string.Format("targetOID:{0} caster:{1} exception:{2}", targetOID, Caster, e));
                    }



                    return;
                }
            }
        }

        private void ResistSpellCheckLOS(GamePlayer player, ushort response, ushort targetOID)
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
                        SpellResisted(target);
                    }
                }
                catch (Exception e)
                {
                    if (log.IsErrorEnabled)
                        log.Error(string.Format("targetOID:{0} caster:{1} exception:{2}", targetOID, Caster, e));
                }
            }
        }

        protected override void OnSpellResisted(GameLiving target)
        {
            if (target is GamePlayer && Caster.TempProperties.getProperty("player_in_keep_property", false))
            {
                GamePlayer player = target as GamePlayer;
                player.Out.SendCheckLOS(Caster, player, new CheckLOSResponse(ResistSpellCheckLOS));
            }
            else SpellResisted(target);
        }

        private void SpellResisted(GameLiving target)
        {
            base.OnSpellResisted(target);
        }


        public HereticDamageSpeedDecreaseLostOnPulse(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
