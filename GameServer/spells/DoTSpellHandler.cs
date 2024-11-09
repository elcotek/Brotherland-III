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
using DOL.Language;
using System;





namespace DOL.GS.Spells
{
    /// <summary>
    /// Damage Over Time spell handler
    /// </summary>
    [SpellHandlerAttribute("DamageOverTime")]
    public class DoTSpellHandler : SpellHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        private const string LOSEFFECTIVENESS = "LOS Effectivness";


        /// <summary>
        /// Execute damage over time spell
        /// </summary>
        /// <param name="target"></param>
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        public override double GetLevelModFactor()
        {
            return 0;
        }
        
        /// <summary>
        /// No variance for DOT spells
        /// </summary>
        /// <param name="target"></param>
        /// <param name="distance"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        protected override double CalculateAreaVariance(GameLiving target, int distance, int radius)
        {
            return 0;
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

        /// <summary>
        /// Calculates damage to target with resist chance and stores it in ad
        /// </summary>
        /// <param name="target">spell target</param>
        /// <param name="effectiveness">value from 0..1 to modify damage</param>
        /// <returns>attack data</returns>
        public override AttackData CalculateDamageToTarget(GameLiving target, double effectiveness)
        {
            AttackData ad = base.CalculateDamageToTarget(target, effectiveness);
            if (this.SpellLine.KeyName == GlobalSpellsLines.Mundane_Poisons)
            {
                RealmAbilities.L3RAPropertyEnhancer ra = Caster.GetAbility<RealmAbilities.ViperAbility>();
                if (ra != null)
                {
                    int additional = (int)((float)ad.Damage * ((float)ra.Amount / 100));
                    ad.Damage += additional;
                }
            }

            GameSpellEffect iWarLordEffect = SpellHandler.FindEffectOnTarget(target, "CleansingAura");
            if (iWarLordEffect != null)
            {
                //ad.Damage *= (int)(1.00 - (iWarLordEffect.Spell.Value * 0.01));
                int adDamageReduce = (int)((float)ad.Damage * ((float)iWarLordEffect.Spell.Value / 100));

                ad.Damage -= adDamageReduce;

               
                if (ad.Damage <= 0)
                {
                    CancelPulsingSpell(target, "DamageOverTime");
                }
                
            }
            //ad.CriticalDamage = 0; - DoTs can crit.
            return ad;
        }

        /// <summary>
        /// Calculates min damage variance %
        /// </summary>
        /// <param name="target">spell target</param>
        /// <param name="min">returns min variance</param>
        /// <param name="max">returns max variance</param>
        public override void CalculateDamageVariance(GameLiving target, out double min, out double max)
        {
            int speclevel = 1;
            min = 1.13;
            max = 1.13;

            if (m_caster is GamePlayer)
            {
                if (m_spellLine.KeyName == GlobalSpellsLines.Mundane_Poisons)
                {
                    speclevel = ((GamePlayer)m_caster).GetModifiedSpecLevel(Specs.Envenom);
                    min = 1.25;
                    max = 1.25;

                    if (target.Level > 0)
                    {
                        min = 0.25 + (speclevel - 1) / (double)target.Level;
                    }
                }
                else
                {
                    speclevel = ((GamePlayer)m_caster).GetModifiedSpecLevel(m_spellLine.Spec);

                    if (target.Level > 0)
                    {
                        min = 0.13 + (speclevel - 1) / (double)target.Level;
                    }
                }
            }

            // no overspec bonus for dots

            if (min > (max - 0.1)) min = (max - 0.1);
            if (min < (max - 0.2)) min = (max - 0.2);

        }


        /// <summary>
        /// Sends damage text messages but makes no damage
        /// </summary>
        /// <param name="ad"></param>
        public override void SendDamageMessages(AttackData ad)
        {
            // Graveen: only GamePlayer should receive messages :p
            GamePlayer PlayerReceivingMessages = null;
            if (m_caster is GamePlayer)
                PlayerReceivingMessages = m_caster as GamePlayer;
            if (m_caster is GamePet)
                if ((m_caster as GamePet).Brain is IControlledBrain)
                    PlayerReceivingMessages = ((m_caster as GamePet).Brain as IControlledBrain).GetPlayerOwner();
            if (PlayerReceivingMessages == null)
                return;


            
            string modmessage = "";


            if (ad.Attacker != null) // if attacked by player, don't show resists (?)
            {
                if (ad.Modifier > 0) modmessage = " (+" + ad.Modifier + ")";
                if (ad.Modifier < 0) modmessage = " (" + ad.Modifier + ")";
            }

            if (Spell.Name.StartsWith("Proc"))
            {
                MessageToCaster(String.Format(LanguageMgr.GetTranslation(PlayerReceivingMessages.Client.Account.Language, "DoTSpellHandler.SendDamageMessages.YouHitFor", ad.Target.GetName(0, false), ad.Damage, modmessage)), eChatType.CT_YouHit);
            }
            else
            {
                MessageToCaster(String.Format(LanguageMgr.GetTranslation(PlayerReceivingMessages.Client.Account.Language, "DoTSpellHandler.SendDamageMessages.YourHitsFor", Spell.Name, ad.Target.GetName(0, false), ad.Damage, modmessage)), eChatType.CT_YouHit);
            }
            if (ad.CriticalDamage > 0)
                MessageToCaster(String.Format(LanguageMgr.GetTranslation(PlayerReceivingMessages.Client.Account.Language, "DoTSpellHandler.SendDamageMessages.YourCriticallyHits", Spell.Name, ad.Target.GetName(0, false), ad.CriticalDamage)), eChatType.CT_YouHit);

        }




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


                        StartDotEffects(target, effectiveness);

                        player.TempProperties.removeProperty(LOSEFFECTIVENESS + target.ObjectID);

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

                    StartDotEffects(target, effectiveness);
                }
            }
            else
            {

                StartDotEffects(target, effectiveness);
            }
        }

        


        public virtual void StartDotEffects(GameLiving target, double effectiveness)
        {


            base.ApplyEffectOnTarget(target, effectiveness);
            target.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
        }

        /*
        protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
        {
            // damage is not reduced with distance
            return new GameSpellEffect(this, m_spell.Duration, m_spell.Frequency, effectiveness);
        }
        */
        protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
        {
            int duration = CalculateEffectDuration(target, effectiveness);
            return new GameSpellEffect(this, duration, m_spell.Frequency, effectiveness);
        }


        public override void OnEffectStart(GameSpellEffect effect)
        {
            SendEffectAnimation(effect.Owner, 0, false, 1);
        }

        public override void OnEffectPulse(GameSpellEffect effect)
        {
            base.OnEffectPulse(effect);

            if (effect.Owner.IsAlive)
            {
                // An acidic cloud surrounds you!
                MessageToLiving(effect.Owner, Spell.Message1, eChatType.CT_Spell);
                // {0} is surrounded by an acidic cloud!
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), eChatType.CT_YouHit, effect.Owner);
                OnDirectEffect(effect.Owner, effect.Effectiveness);
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
                // The acidic mist around you dissipates.
                MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);

                GameSpellEffect iWarLordEffect = SpellHandler.FindEffectOnTarget(effect.Owner, "CleansingAura");
                if (iWarLordEffect != null)
                {
                    iWarLordEffect.Cancel(false);
                }
                    // The acidic mist around {0} dissipates.
                    Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
            }
            return 0;
        }

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (target == null) return;
            if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;

            // no interrupts on DoT direct effect
            // calc damage
            AttackData ad = CalculateDamageToTarget(target, effectiveness);
            SendDamageMessages(ad);
            DamageTarget(ad, false, false);
        }

        public override double CalculateDamageBase(GameLiving target)
        {
            int manaStatValue = 0;
            int IntelligenceValue = 0;
            int PietyValue = 0;
            int EmpathyValue = 0;
            int subTeranLevel = 0;

            double spellDamage = Spell.Damage;
           
            GamePlayer player = null;
            if (m_caster is GamePlayer)
                player = m_caster as GamePlayer;
            

            if (player != null && player.CharacterClass.ManaStat != eStat.UNDEFINED)
            {

               

                if (player.CharacterClass.ID == (byte)eCharacterClass.Infiltrator || player.CharacterClass.ID == (byte)eCharacterClass.Shadowblade || player.CharacterClass.ID == (byte)eCharacterClass.Nightshade)
                {
                    manaStatValue = ServerProperties.Properties.STEALTHER_MANA_STAT; //player.GetModified((eProperty)player.CharacterClass.ManaStat);
                    //spellDamage *= manaStatValue / 275.0;
                    spellDamage *= ((manaStatValue * manaStatValue) / 200.0 / target.Level) * ServerProperties.Properties.GLOBAL_POISON;

                    
                    if (spellDamage < 0)
                        spellDamage = 0;
                    if (spellDamage < Spell.Damage)
                        spellDamage = Spell.Damage;

                }

                else if (player.CharacterClass.ID == (byte)eCharacterClass.Mentalist || player.CharacterClass.ID == (byte)eCharacterClass.Cabalist
                    || player.CharacterClass.ID == (byte)eCharacterClass.Sorcerer || player.CharacterClass.ID == (byte)eCharacterClass.Wizard
                    || player.CharacterClass.ID == (byte)eCharacterClass.Valewalker || player.CharacterClass.ID == (byte)eCharacterClass.Necromancer)
                {
                    IntelligenceValue = player.GetModified(eProperty.Intelligence);

                    spellDamage *= ((IntelligenceValue * IntelligenceValue) / 200.0 / target.Level) * ServerProperties.Properties.GLOBAL_POISON;

                   

                    if (spellDamage < 0)
                        spellDamage = 0;
                    if (spellDamage < Spell.Damage)
                      spellDamage = Spell.Damage;

                }
               
                else if (player.CharacterClass.ID == (byte)eCharacterClass.Bonedancer || player.CharacterClass.ID == (byte)eCharacterClass.Reaver || player.CharacterClass.ID == (byte)eCharacterClass.Heretic || player.CharacterClass.ID == (byte)eCharacterClass.Warlock)
                {
                    
                  
                    
                    PietyValue = player.GetModified(eProperty.Piety);
                    spellDamage *= ((PietyValue * PietyValue) / 200.0 / target.Level) * ServerProperties.Properties.GLOBAL_POISON;

                   
                                   
                    if (spellDamage < 0)
                        spellDamage = 0;
                    if (spellDamage < Spell.Damage)
                        spellDamage = Spell.Damage;
                }
                else if (player.CharacterClass.ID == (byte)eCharacterClass.Shaman)
                {

                    int manaStatValue2 = player.GetModified((eProperty)player.CharacterClass.ManaStat);
                    spellDamage *= (manaStatValue2 + 200) / 275.0;
                    if (spellDamage < 0)
                        spellDamage = 0;

                    subTeranLevel = player.GetBaseSpecLevel(Specs.Subterranean);

                   
                   // spellDamage *= (((int)player.Level * (int)5.42) / 200.0 / target.Level) * ServerProperties.Properties.GLOBAL_POISON;
                  
                    if (subTeranLevel > 0)
                    {
                        if (subTeranLevel > 50)
                        {
                            subTeranLevel = 50;
                        }
                        spellDamage += spellDamage * (double)subTeranLevel / 100;
                        
                    }

                    if (spellDamage < 0)
                        spellDamage = 0;
                    if (spellDamage < Spell.Damage)
                        spellDamage = Spell.Damage;
                }
                else if (player.CharacterClass.ID == (byte)eCharacterClass.Druid)
                {
                    EmpathyValue = player.GetModified(eProperty.Empathy);
                    spellDamage *= ((EmpathyValue * EmpathyValue) / 200.0 / target.Level) * ServerProperties.Properties.GLOBAL_POISON;

                    // spellDamage *= (EmpathyValue + ServerProperties.Properties.GLOBAL_POISON ) / 275.0 * (50 / Caster.target.Level);
                    if (spellDamage < 0)
                        spellDamage = 0;
                    if (spellDamage < Spell.Damage)
                        spellDamage = Spell.Damage;
                }

            }
            else if (m_caster is NecromancerPet)
            {
                GamePlayer this_necro_pl = null;
                this_necro_pl = ((m_caster as NecromancerPet).Brain as IControlledBrain).GetPlayerOwner();
                int NecIntelligenceValue = 0;

                NecIntelligenceValue = this_necro_pl.GetModified(eProperty.Intelligence);

                spellDamage *= ((NecIntelligenceValue * NecIntelligenceValue) / 200.0 / target.Level) * ServerProperties.Properties.GLOBAL_POISON;
                
               
                if (spellDamage < 0)
                    spellDamage = 0;
                if (spellDamage < Spell.Damage)
                    spellDamage = Spell.Damage;
            }
            else if (m_caster is GameNPC && m_caster is NecromancerPet == false)
            {
                int MobInt = m_caster.GetModified(eProperty.Intelligence);
                spellDamage *= ((MobInt * MobInt) / 200.0 / target.Level) * ServerProperties.Properties.GLOBAL_POISON_NPC;
                if (spellDamage < 0)
                    spellDamage = 0;
            }
                      
            return spellDamage;
        }

        // constructor
        public DoTSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
