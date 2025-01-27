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
using DOL.Database;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Language;
using System;
using System.Collections.Generic;




namespace DOL.GS.Spells
{
    // Melee ablative
    [SpellHandlerAttribute("AblativeArmor")]
    public class AblativeArmorSpellHandler : SpellHandler
    {
        public const string ABLATIVE_HP = "ablative hp";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            effect.Owner.TempProperties.setProperty(ABLATIVE_HP, (int)Spell.Value);
            GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));

            eChatType toLiving = (Spell.Pulse == 0) ? eChatType.CT_Spell : eChatType.CT_SpellPulse;
            eChatType toOther = (Spell.Pulse == 0) ? eChatType.CT_System : eChatType.CT_SpellPulse;
            MessageToLiving(effect.Owner, Spell.Message1, toLiving);
            Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), toOther, effect.Owner);
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
            GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
            effect.Owner.TempProperties.removeProperty(ABLATIVE_HP);
            if (!noMessages && Spell.Pulse == 0)
            {
                MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
            }
            return 0;
        }

        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        /// <summary>
		/// Calculates the effect duration in milliseconds
		/// </summary>
		/// <param name="target">The effect target</param>
		/// <param name="effectiveness">The effect effectiveness</param>
		/// <returns>The effect duration in milliseconds</returns>
		protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
        {
            double duration = Spell.Duration;
            duration *= (1.0 + m_caster.GetModified(eProperty.SpellDuration) * 0.01);
            return (int)duration;
        }

        private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
        {
            GameLiving living = sender as GameLiving;
            if (living == null) return;
            AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
            AttackData ad = null;
            if (attackedByEnemy != null)
                ad = attackedByEnemy.AttackData;

            //Damagetype Melee only

            //log.DebugFormat("sender:{0} res:{1} IsMelee:{2} Type:{3}", living.Name, ad.AttackResult, ad.IsMeleeAttack, ad.AttackType);

            // Melee or Magic or Both ?
            if (!MatchingDamageType(ref ad)) return;

            int ablativehp = living.TempProperties.getProperty<int>(ABLATIVE_HP);
            double absorbPercent = 25;
            if (Spell.Damage > 0)
                absorbPercent = Spell.Damage;
            //because albatives can reach 100%
            if (absorbPercent > 100)
                absorbPercent = 100;
            int damageAbsorbed = (int)(0.01 * absorbPercent * (ad.Damage + ad.CriticalDamage));
            if (damageAbsorbed > ablativehp)
                damageAbsorbed = ablativehp;
            ablativehp -= damageAbsorbed;
            ad.Damage -= damageAbsorbed;
            OnDamageAbsorbed(ad, damageAbsorbed);

            if (ad.Target is GamePlayer)
            {
                (ad.Target as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((ad.Target as GamePlayer).Client.Account.Language, "AblativeArmor.Target", damageAbsorbed.ToString()), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
            }
            if (ad.Attacker is GamePlayer)
            {
                (ad.Attacker as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((ad.Attacker as GamePlayer).Client.Account.Language, "AblativeArmor.Attacker", damageAbsorbed.ToString()), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
            }
            if (ablativehp <= 0)
            {
                GameSpellEffect effect = SpellHandler.FindEffectOnTarget(living, this);
                if (effect != null)
                    effect.Cancel(false);
            }
            else
            {
                living.TempProperties.setProperty(ABLATIVE_HP, ablativehp);
            }
        }

        // Check if Melee
        protected virtual bool MatchingDamageType(ref AttackData ad)
        {
            if (ad.Damage > 0 && ad.DamageType != eDamageType.Crush && ad.DamageType != eDamageType.Slash && ad.DamageType != eDamageType.Thrust)
                return false;

            if (ad == null || (ad.AttackResult != GameLiving.eAttackResult.HitStyle && ad.AttackResult != GameLiving.eAttackResult.HitUnstyled))
                return false;
            if (!ad.IsMeleeAttack && ad.AttackType != AttackData.eAttackType.Ranged)
                return false;

            return true;
        }

        protected virtual void OnDamageAbsorbed(AttackData ad, int DamageAmount)
        {
        }

        public override PlayerXEffect GetSavedEffect(GameSpellEffect e)
        {
            if ( //VaNaTiC-> this cannot work, cause PulsingSpellEffect is derived from object and only implements IConcEffect
                 //e is PulsingSpellEffect ||
                 //VaNaTiC<-
                Spell.Pulse != 0 || Spell.Concentration != 0 || e.RemainingTime < 1)
                return null;
            PlayerXEffect eff = new PlayerXEffect();
            eff.Var1 = Spell.ID;
            eff.Duration = e.RemainingTime;
            eff.IsHandler = true;
            eff.Var2 = (int)Spell.Value;
            eff.SpellLine = SpellLine.KeyName;
            return eff;
        }

        public override void OnEffectRestored(GameSpellEffect effect, int[] vars)
        {
            effect.Owner.TempProperties.setProperty(ABLATIVE_HP, (int)vars[1]);
            GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
        }

        public override int OnRestoredEffectExpires(GameSpellEffect effect, int[] vars, bool noMessages)
        {
            GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
            effect.Owner.TempProperties.removeProperty(ABLATIVE_HP);
            if (!noMessages && Spell.Pulse == 0)
            {
                MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
            }
            return 0;
        }
        #region Delve Info
        public override IList<string> DelveInfo(GamePlayer player)
        {

            var list = new List<string>();

            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "AblativeArmor.DelveInfo.Function"));
            list.Add("");
            list.Add(Spell.Description);
            list.Add("");
            if (Spell.Damage != 0)
                list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "AblativeArmor.DelveInfo.Absorption1", Spell.Damage.ToString()));
            if (Spell.Damage > 100)
                list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "AblativeArmor.DelveInfo.Absorption2"));
            if (Spell.Damage == 0)
                list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "AblativeArmor.DelveInfo.Absorption3"));
            if (Spell.Value != 0)
                list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Value", Spell.Value.ToString()));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Target", Spell.Target.ToString()));
            if (Spell.Range != 0)
                list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Range", Spell.Range.ToString()));
            if (Spell.Duration >= ushort.MaxValue * 1000)
                list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + " Permanent.");
            else if (Spell.Duration > 60000)
                list.Add(string.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + Spell.Duration / 60000 + ":" + (Spell.Duration % 60000 / 1000).ToString("00") + " min"));
            else if (Spell.Duration != 0)
                list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + (Spell.Duration / 1000).ToString("0' sec';'Permanent.';'Permanent.'"));
            if (Spell.Power != 0)
                list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.PowerCost", Spell.Power.ToString("0;0'%'")));
            if (Spell.CastTime < 0.1)
                list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "AblativeArmor.DelveInfo.CastingTime"));
            else if (Spell.CastTime > 0)
                list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.CastingTime", (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'")));
            {
                if (ServerProperties.Properties.SERV_LANGUAGE != "DE")
                {
                    //SpellType
                    list.Add(GetAblativeType());

                    //Radius
                    if (Spell.Radius != 0)
                        list.Add(LanguageMgr.GetTranslation((player as GamePlayer).Client, "DelveInfo.Radius", Spell.Radius.ToString()));

                    //Frequency
                    if (Spell.Frequency != 0)
                        list.Add(LanguageMgr.GetTranslation((player as GamePlayer).Client, "DelveInfo.Frequency", (Spell.Frequency * 0.001).ToString("0.0")));
                    //DamageType
                    if (Spell.DamageType != 0)
                        list.Add(LanguageMgr.GetTranslation((player as GamePlayer).Client, "DelveInfo.DamageType", Spell.DamageType.ToString()));
                }
                return list;
            }

            #endregion
        }


        // for delve info
        protected virtual string GetAblativeType()
        {
            return "Type: Melee Absorption";
        }

        public AblativeArmorSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    // Magic Ablative
    [SpellHandlerAttribute("MagicAblativeArmor")]
    public class MagicAblativeArmorSpellHandler : AblativeArmorSpellHandler
    {
        public MagicAblativeArmorSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        // Check if Melee
        protected override bool MatchingDamageType(ref AttackData ad)
        {
            if (ad == null || (ad.AttackResult == GameLiving.eAttackResult.HitStyle && ad.AttackResult == GameLiving.eAttackResult.HitUnstyled))
                return false;
            if (ad.IsMeleeAttack && ad.AttackType == AttackData.eAttackType.Ranged)
                return false;

            return true;
        }

        // for delve info
        protected override string GetAblativeType()
        {
            return "Type: Magic Absorption";
        }
    }
    //Both Magic/melee ablative 1.101 druids mite have a buff like this...
    [SpellHandlerAttribute("BothAblativeArmor")]
    public class BothAblativeArmorSpellHandler : AblativeArmorSpellHandler
    {
        public BothAblativeArmorSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        // Anything is absorbed with this method
        protected override bool MatchingDamageType(ref AttackData ad)
        {
            return true;
        }

        // for delve info
        protected override string GetAblativeType()
        {
            return "Type: Melee/Magic Absorption";
        }
    }
}