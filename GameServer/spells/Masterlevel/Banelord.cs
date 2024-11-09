using DOL.AI.Brain;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using System;
using System.Collections.Generic;



namespace DOL.GS.Spells
{
    //http://www.camelotherald.com/masterlevels/ma.php?ml=Banelord
    //shared timer 1
    #region Banelord-1
    [SpellHandlerAttribute("CastingSpeedDebuff")]
    public class CastingSpeedDebuff : MasterlevelDebuffHandling
    {
        public override eProperty Property1 { get { return eProperty.CastingSpeed; } }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            base.ApplyEffectOnTarget(target, effectiveness);
            //target.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
        }

        // constructor
        public CastingSpeedDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    //shared timer 5 for ml2 - shared timer 3 for ml8
    #region Banelord-2/8
    [SpellHandlerAttribute("PBAEDamage")]
    public class PBAEDamage : MasterlevelHandling
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // constructor
        public PBAEDamage(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            //For Banelord ML 8, it drains Life from the Caster
            if (Spell.Damage > 0)
            {
                int chealth;
                chealth = (m_caster.Health * (int)Spell.Damage) / 100;

                if (m_caster.Health < chealth)
                    chealth = 0;

                m_caster.Health -= chealth;
            }
            base.FinishSpellCast(target);
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


                        StartPBAEDamageEffects(target, effectiveness);

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


        public override void OnDirectEffect(GameLiving target, double effectiveness)
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

                    StartPBAEDamageEffects(target, effectiveness);
                }
            }
            else
            {

                StartPBAEDamageEffects(target, effectiveness);
            }
        }


        public virtual void StartPBAEDamageEffects(GameLiving target, double effectiveness)
        {

            if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;

            base.OnDirectEffect(target, effectiveness);


            GamePlayer player = target as GamePlayer;
            if (target is GamePlayer)
            {
                int mana;
                int health;
                int end;

                int value = (int)Spell.Value;
                mana = (player.Mana * value) / 100;
                end = (player.Endurance * value) / 100;
                health = (player.Health * value) / 100;

                //You don't gain RPs from this Spell
                if (player.Health < health)
                    player.Health = 1;
                else
                    player.Health -= health;

                if (player.Mana < mana)
                    player.Mana = 1;
                else
                    player.Mana -= mana;

                if (player.Endurance < end)
                    player.Endurance = 1;
                else
                    player.Endurance -= end;

                GameSpellEffect aoemez = SpellHandler.FindEffectOnTarget(target, "AOEMesmerize");
                if (aoemez != null)
                    aoemez.Cancel(false);

                GameSpellEffect effect2 = SpellHandler.FindEffectOnTarget(target, "Mesmerize");
                if (effect2 != null)
                {
                    effect2.Cancel(true);
                    return;
                }
                foreach (GamePlayer ply in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    SendEffectAnimation(player, 0, false, 1);
                }
                player.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
            }
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 25;
        }
    }
    #endregion

    //shared timer 3
    #region Banelord-3
    [SpellHandlerAttribute("Oppression")]
    public class OppressionSpellHandler : MasterlevelHandling
    {

        public override bool IsOverwritable(GameSpellEffect compare)
        {
            return true;
        }
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }
        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }
        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            if (effect.Owner is GamePlayer)
            {
                if (((GamePlayer)effect.Owner).OppressionEncumberance == false)
                {
                    ((GamePlayer)effect.Owner).OppressionEncumberance = true;
                    ((GamePlayer)effect.Owner).UpdateEncumberance();

                }
            }
            effect.Owner.StartInterruptTimer(effect.Owner.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
        }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GameSpellEffect aoemez = SpellHandler.FindEffectOnTarget(target, "AOEMesmerize");
            if (aoemez != null)
                aoemez.Cancel(false);

            GameSpellEffect mezz = SpellHandler.FindEffectOnTarget(target, "Mesmerize");
            if (mezz != null)
                mezz.Cancel(false);
            base.ApplyEffectOnTarget(target, effectiveness);
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (effect.Owner is GamePlayer)
            {
                if (((GamePlayer)effect.Owner).OppressionEncumberance == true)
                {
                    ((GamePlayer)effect.Owner).OppressionEncumberance = false;
                    ((GamePlayer)effect.Owner).UpdateEncumberance();
                }
            }
            return base.OnEffectExpires(effect, noMessages);
        }
        public OppressionSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    //shared timer 1
    #region Banelord-4
    [SpellHandler("MLFatDebuff")]
    public class MLFatDebuffHandler : MasterlevelDebuffHandling
    {
        public override eProperty Property1 { get { return eProperty.FatigueConsumption; } }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GameSpellEffect aoemez = SpellHandler.FindEffectOnTarget(target, "AOEMesmerize");
            if (aoemez != null)
                aoemez.Cancel(false);

            GameSpellEffect effect2 = SpellHandler.FindEffectOnTarget(target, "Mesmerize");
            if (effect2 != null)
            {
                effect2.Cancel(false);
                return;
            }
            base.ApplyEffectOnTarget(target, effectiveness);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            effect.Owner.StartInterruptTimer(effect.Owner.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
            base.OnEffectStart(effect);
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        // constructor
        public MLFatDebuffHandler(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) { }
    }
    #endregion

    //shared timer 5
    #region Banelord-5
    [SpellHandlerAttribute("MissHit")]
    public class MissHit : MasterlevelBuffHandling
    {
        public override eProperty Property1 { get { return eProperty.MissHit; } }

        // constructor
        public MissHit(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    //shared timer 1
    #region Banelord-6
    #region ML6Snare
    [SpellHandler("MLUnbreakableSnare")]
    public class MLUnbreakableSnare : BanelordSnare
    {
        protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
        {
            int duration = Spell.Duration;
            if (duration < 1)
                duration = 1;
            else if (duration > (Spell.Duration * 4))
                duration = (Spell.Duration * 4);
            return duration;
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        public MLUnbreakableSnare(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
        }
    }
    #endregion
    #region ML6Stun
    [SpellHandler("UnrresistableNonImunityStun")]
    public class UnrresistableNonImunityStun : MasterlevelHandling
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
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


                        StartUnbreakableSpeedStunEffects(target, effectiveness);

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

                    StartUnbreakableSpeedStunEffects(target, effectiveness);
                }
            }
            else
            {

                StartUnbreakableSpeedStunEffects(target, effectiveness);
            }
        }


        public virtual void StartUnbreakableSpeedStunEffects(GameLiving target, double effectiveness)
        { 
            if (target.HasAbility(Abilities.CCImmunity)||target.HasAbility(Abilities.StunImmunity))
            {
                MessageToCaster(target.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
                return;
            }
            if (target.EffectList.CountOfType(typeof(SpeedOfSoundEffect), typeof(ArmsLengthEffect), typeof(ChargeEffect)) > 0)
            {
                MessageToCaster(target.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
                return;
            }


            base.ApplyEffectOnTarget(target, effectiveness);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            effect.Owner.IsStunned = true;
            effect.Owner.StopAttack();
            effect.Owner.StopCurrentSpellcast();
            effect.Owner.DisableTurning(true);

            SendEffectAnimation(effect.Owner, 0, false, 1);

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
            }

            base.OnEffectStart(effect);
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            effect.Owner.IsStunned = false;
            effect.Owner.DisableTurning(false);

            if (effect.Owner == null) return 0;

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
            return 0;
        }

        protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
        {
            return Spell.Duration;
        }

        public override bool IsOverwritable(GameSpellEffect compare)
        {
            if (Spell.EffectGroup != 0)
                return Spell.EffectGroup == compare.Spell.EffectGroup;
            if (compare.Spell.SpellType == "UnrresistableNonImunityStun") return true;
            return base.IsOverwritable(compare);
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        public override bool HasPositiveEffect
        {
            get
            {
                return false;
            }
        }

        public UnrresistableNonImunityStun(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
        }
    }
    #endregion
    #endregion

    //shared timer 3
    #region Banelord-7
    [SpellHandlerAttribute("BLToHit")]
    public class BLToHit : MasterlevelBuffHandling
    {
        public override eProperty Property1 { get { return eProperty.ToHitBonus; } }

        // constructor
        public BLToHit(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    //shared timer 5
    #region Banelord-9
    [SpellHandlerAttribute("EffectivenessDebuff")]
    public class EffectivenessDeBuff : MasterlevelHandling
    {
        /// <summary>
        /// called after normal spell cast is completed and effect has to be started
        /// </summary>
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }


        /// <summary>
        /// When an applied effect starts
        /// duration spells only
        /// </summary>
        /// <param name="effect"></param>
        public override void OnEffectStart(GameSpellEffect effect)
        {
            GamePlayer player = effect.Owner as GamePlayer;
            if (player != null)
            {
                player.Effectiveness -= Spell.Value * 0.01;
                player.Out.SendUpdateWeaponAndArmorStats();
                player.Out.SendStatusUpdate();
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
            GamePlayer player = effect.Owner as GamePlayer;
            if (player != null)
            {
                player.Effectiveness += Spell.Value * 0.01;
                player.Out.SendUpdateWeaponAndArmorStats();
                player.Out.SendStatusUpdate();
            }
            return 0;
        }

        public override IList<string> DelveInfo(GamePlayer player)
        {
            
            {
                var list = new List<string>(16);
                //Name
                list.Add("Name: " + Spell.Name);
                //Description
                list.Add("Description: " + Spell.Description);
                //Target
                list.Add("Target: " + Spell.Target);
                //Cast
                list.Add("Casting time: " + (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
                //Duration
                if (Spell.Duration >= ushort.MaxValue * 1000)
                    list.Add("Duration: Permanent.");
                else if (Spell.Duration > 60000)
                    list.Add(string.Format("Duration: {0}:{1} min", Spell.Duration / 60000, (Spell.Duration % 60000 / 1000).ToString("00")));
                else if (Spell.Duration != 0)
                    list.Add("Duration: " + (Spell.Duration / 1000).ToString("0' sec';'Permanent.';'Permanent.'"));
                //Recast
                if (Spell.RecastDelay > 60000)
                    list.Add("Recast time: " + (Spell.RecastDelay / 60000).ToString() + ":" + (Spell.RecastDelay % 60000 / 1000).ToString("00") + " min");
                else if (Spell.RecastDelay > 0)
                    list.Add("Recast time: " + (Spell.RecastDelay / 1000).ToString() + " sec");
                //Range
                if (Spell.Range != 0) list.Add("Range: " + Spell.Range);
                //Radius
                if (Spell.Radius != 0) list.Add("Radius: " + Spell.Radius);
                //Cost
                if (Spell.Power != 0)
                    list.Add("Power cost: " + Spell.Power.ToString("0;0'%'"));
                //Effect
                list.Add("Debuff Effectiveness Of target By: " + Spell.Value + "%");

                if (Spell.Frequency != 0)
                    list.Add("Frequency: " + (Spell.Frequency * 0.001).ToString("0.0"));

                return list;
            }
        }
        public EffectivenessDeBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion








    //no shared timer
    #region Banelord-10
    [SpellHandlerAttribute("Banespike")]
    public class BaneLordEffectivenessBuff : MasterlevelHandling
    {
        /// <summary>
        /// called after normal spell cast is completed and effect has to be started
        /// </summary>
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        public override bool HasPositiveEffect
        {
            get { return true; }
        }

        /// <summary>
        /// When an applied effect starts
        /// duration spells only
        /// </summary>
        /// <param name="effect"></param>
        public override void OnEffectStart(GameSpellEffect effect)
        {
            GamePlayer player = effect.Owner as GamePlayer;
            if (player != null)
            {
                    player.Effectiveness += Spell.Value * 0.01;
                    player.Out.SendUpdateWeaponAndArmorStats();
                    player.Out.SendStatusUpdate();
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
            GamePlayer player = effect.Owner as GamePlayer;
            if (player != null)
            {
                player.Effectiveness -= Spell.Value * 0.01;
                player.Out.SendUpdateWeaponAndArmorStats();
                player.Out.SendStatusUpdate();
            }
            return 0;
        }

        public override IList<string> DelveInfo(GamePlayer player)
        {
            
            {
                var list = new List<string>(16);
                //Name
                list.Add("Name: " + Spell.Name);
                //Description
                list.Add("Description: " + Spell.Description);
                //Target
                list.Add("Target: " + Spell.Target);
                //Cast
                list.Add("Casting time: " + (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
                //Duration
                if (Spell.Duration >= ushort.MaxValue * 1000)
                    list.Add("Duration: Permanent.");
                else if (Spell.Duration > 60000)
                    list.Add(string.Format("Duration: {0}:{1} min", Spell.Duration / 60000, (Spell.Duration % 60000 / 1000).ToString("00")));
                else if (Spell.Duration != 0)
                    list.Add("Duration: " + (Spell.Duration / 1000).ToString("0' sec';'Permanent.';'Permanent.'"));
                //Recast
                if (Spell.RecastDelay > 60000)
                    list.Add("Recast time: " + (Spell.RecastDelay / 60000).ToString() + ":" + (Spell.RecastDelay % 60000 / 1000).ToString("00") + " min");
                else if (Spell.RecastDelay > 0)
                    list.Add("Recast time: " + (Spell.RecastDelay / 1000).ToString() + " sec");
                //Range
                if (Spell.Range != 0) list.Add("Range: " + Spell.Range);
                //Radius
                if (Spell.Radius != 0) list.Add("Radius: " + Spell.Radius);
                //Cost
                if (Spell.Power != 0)
                    list.Add("Power cost: " + Spell.Power.ToString("0;0'%'"));
                //Effect
                list.Add("Point blank area effect shout, that boots the damage of attacks from nearby allies by "+Spell.Value+"%.");

                if (Spell.Frequency != 0)
                    list.Add("Frequency: " + (Spell.Frequency * 0.001).ToString("0.0"));

                return list;
            }
        }
        public BaneLordEffectivenessBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion
}

#region MisshitCalc

namespace DOL.GS.PropertyCalc
{
    /// <summary>
    /// The melee damage bonus percent calculator
    ///
    /// BuffBonusCategory1 is used for buffs
    /// BuffBonusCategory2 unused
    /// BuffBonusCategory3 is used for debuff
    /// BuffBonusCategory4 unused
    /// BuffBonusMultCategory1 unused
    /// </summary>
    [PropertyCalculator(eProperty.MissHit)]
    public class MissHitPercentCalculator : PropertyCalculator
    {
        public override int CalcValue(GameLiving living, eProperty property)
        {
            return (int)(
                +living.BaseBuffBonusCategory[(int)property]
                + living.SpecBuffBonusCategory[(int)property]
                - living.DebuffCategory[(int)property]
                + living.BuffBonusCategory4[(int)property]);
        }
    }
}

#endregion