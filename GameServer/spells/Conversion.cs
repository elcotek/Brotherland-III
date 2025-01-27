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
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using System;
using System.Collections.Generic;




namespace DOL.GS.Spells
{
    [SpellHandlerAttribute("ManaEnduConversion")]//only hits from enemys
    public class ManaEnduConversionSpellHandler : SpellHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        int spellDamage = 0;

        public const string AttackConvertDamage = "Conversion";

        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            effect.Owner.TempProperties.setProperty(AttackConvertDamage, 100000);
            GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));


            eChatType toLiving = (Spell.Pulse == 0) ? eChatType.CT_Spell : eChatType.CT_SpellPulse;
            eChatType toOther = (Spell.Pulse == 0) ? eChatType.CT_System : eChatType.CT_Spell;
            MessageToLiving(effect.Owner, Spell.Message1, toLiving);
            Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), toOther, effect.Owner);
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));

            effect.Owner.TempProperties.removeProperty(AttackConvertDamage);
            return 1;
        }

        protected virtual void OndamageConverted(AttackData ad, int DamageAmount)
        {
        }


        private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
        {
            GameLiving living = sender as GameLiving;
            if (living == null) return;
            AttackedByEnemyEventArgs AttackFinished = arguments as AttackedByEnemyEventArgs;
            AttackData ad = null;



            //Melee AND SPELL damage
            if (AttackFinished != null)
            {
                ad = AttackFinished.AttackData;

            }

            if (ad == null || ad.Attacker == Caster)
            {
                return;
            }

            //int reduceddmg = living.TempProperties.getProperty<int>(ConvertDamage);
            double damagePercent = Spell.Value;
            int damageConverted = (ad.Damage + ad.CriticalDamage + spellDamage);
            double adddamagePercent = 0;
            if (damageConverted > 0)
            {
                adddamagePercent = damageConverted * damagePercent / 100;
                OndamageConverted(ad, (int)adddamagePercent);
            }

            if (Caster.Endurance != Caster.MaxEndurance && adddamagePercent > 0)
            {
                MessageToCaster("You convert " + ((int)adddamagePercent).ToString() + " damage into endurance", eChatType.CT_Spell);
                Caster.Endurance = Caster.Endurance + (int)adddamagePercent;
            }
            else
            {
                MessageToCaster("You cannot convert anymore endurance!", eChatType.CT_Spell);
            }
            if (Caster.Mana != Caster.MaxMana && adddamagePercent > 0)
            {
                MessageToCaster("You convert " + ((int)adddamagePercent).ToString() + " damage into mana.", eChatType.CT_Spell);
                Caster.Mana = Caster.Mana + (int)adddamagePercent;
            }
            else
            {
                MessageToCaster("You cannot convert anymore mana!", eChatType.CT_Spell);
            }
        }



        public override IList<string> DelveInfo(GamePlayer player)
        {

            {
                var list = new List<string>();
                list.Add("Name: " + Spell.Name);
                list.Add("Description: " + Spell.Description);
                list.Add("Target: " + Spell.Target);
                if (Spell.Value != 0)
                {
                    //list.Add("Health Return: " + Spell.Value + "%");
                    list.Add("Power Return: " + Spell.Value.ToString() + "%");
                    list.Add("Endurance Return: " + Spell.Value.ToString() + "%");
                }
                return list;
            }
        }
        public ManaEnduConversionSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        [SpellHandlerAttribute("Conversion")]
        public class ConversionSpellHandler : SpellHandler
        {
            public const string ConvertDamage = "Conversion";

            public override void FinishSpellCast(GameLiving target)
            {
                m_caster.Mana -= PowerCost(target);
                base.FinishSpellCast(target);
            }

            public override void OnEffectStart(GameSpellEffect effect)
            {
                effect.Owner.TempProperties.setProperty(ConvertDamage, 100000);
                GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));

                eChatType toLiving = (Spell.Pulse == 0) ? eChatType.CT_Spell : eChatType.CT_SpellPulse;
                eChatType toOther = (Spell.Pulse == 0) ? eChatType.CT_System : eChatType.CT_Spell;
                MessageToLiving(effect.Owner, Spell.Message1, toLiving);
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), toOther, effect.Owner);
            }

            public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
            {
                GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
                effect.Owner.TempProperties.removeProperty(ConvertDamage);
                return 1;
            }

            protected virtual void OndamageConverted(AttackData ad, int DamageAmount)
            {
            }

            private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
            {
                GameLiving living = sender as GameLiving;
                if (living == null) return;
                AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
                AttackData ad = null;
                if (attackedByEnemy != null)
                {
                    ad = attackedByEnemy.AttackData;
                }
                int reduceddmg = living.TempProperties.getProperty<int>(ConvertDamage);
                double absorbPercent = Spell.Damage;
                int damageConverted = (int)(0.01 * absorbPercent * (ad.Damage + ad.CriticalDamage));

                if (damageConverted > reduceddmg)
                {
                    damageConverted = reduceddmg;
                    reduceddmg -= damageConverted;
                    ad.Damage -= damageConverted;
                    OndamageConverted(ad, damageConverted);
                }

                if (ad.Damage > 0)
                    MessageToLiving(ad.Target, string.Format("You convert {0} damage into " + damageConverted.ToString() + " Health.", damageConverted.ToString()), eChatType.CT_Spell);
                MessageToLiving(ad.Attacker, string.Format("A magical spell absorbs {0} damage of your attack!", damageConverted.ToString()), eChatType.CT_Spell);

                if (Caster.Health != Caster.MaxHealth)
                {
                    MessageToCaster("You convert " + damageConverted.ToString() + " damage into health.", eChatType.CT_Spell);
                    Caster.Health = Caster.Health + damageConverted;

                    #region PVP DAMAGE

                    if (ad.Target is NecromancerPet &&
                        ((ad.Target as NecromancerPet).Brain as IControlledBrain).GetPlayerOwner() != null
                        || ad.Target is GamePlayer)
                    {
                        if (ad.Target.DamageRvRMemory > 0)
                            ad.Target.DamageRvRMemory -= (long)Math.Max(damageConverted, 0);
                    }

                    #endregion PVP DAMAGE

                }
                else
                {
                    MessageToCaster("You cannot convert anymore health!", eChatType.CT_Spell);
                }

                if (Caster.Endurance != Caster.MaxEndurance)
                {
                    MessageToCaster("You convert " + damageConverted.ToString() + " damage into endurance", eChatType.CT_Spell);
                    Caster.Endurance = Caster.Endurance + damageConverted;
                }
                else
                {
                    MessageToCaster("You cannot convert anymore endurance!", eChatType.CT_Spell);
                }
                if (Caster.Mana != Caster.MaxMana)
                {
                    MessageToCaster("You convert " + damageConverted.ToString() + " damage into mana.", eChatType.CT_Spell);
                    Caster.Mana = Caster.Mana + damageConverted;
                }
                else
                {
                    MessageToCaster("You cannot convert anymore mana!", eChatType.CT_Spell);
                }

                if (reduceddmg <= 0)
                {
                    GameSpellEffect effect = SpellHandler.FindEffectOnTarget(living, this);
                    if (effect != null)
                        effect.Cancel(false);
                }
            }
            public override IList<string> DelveInfo(GamePlayer player)
            {

                {
                    var list = new List<string>();
                    list.Add("Name: " + Spell.Name);
                    list.Add("Description: " + Spell.Description);
                    list.Add("Target: " + Spell.Target);
                    if (Spell.Damage != 0)
                    {
                        list.Add("Damage Absorb: " + Spell.Damage.ToString() + "%");
                        list.Add("Health Return: " + Spell.Damage.ToString() + "%");
                        list.Add("Power Return: " + Spell.Damage.ToString() + "%");
                        list.Add("Endurance Return: " + Spell.Damage.ToString() + "%");
                    }
                    return list;
                }
            }
            public ConversionSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
        }

        [SpellHandlerAttribute("MagicConversion")]
        public class MagicConversionSpellHandler : ConversionSpellHandler
        {
            //public const string ConvertDamage = "Conversion";

            private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
            {
                GameLiving living = sender as GameLiving;
                if (living == null) return;
                AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
                AttackData ad = null;
                if (attackedByEnemy != null)
                {
                    ad = attackedByEnemy.AttackData;
                }


                if (ad.Damage > 0)
                {
                    switch (attackedByEnemy.AttackData.AttackType)
                    {
                        case AttackData.eAttackType.Spell:
                            {
                                int reduceddmg = living.TempProperties.getProperty<int>(ConvertDamage, 0);
                                double absorbPercent = Spell.Damage;
                                int damageConverted = (int)(0.01 * absorbPercent * (ad.Damage + ad.CriticalDamage));
                                if (damageConverted > reduceddmg)
                                {
                                    damageConverted = reduceddmg;
                                    reduceddmg -= damageConverted;
                                    ad.Damage -= damageConverted;
                                    OndamageConverted(ad, damageConverted);
                                }
                                if (reduceddmg <= 0)
                                {
                                    GameSpellEffect effect = SpellHandler.FindEffectOnTarget(living, this);
                                    if (effect != null)
                                        effect.Cancel(false);
                                }
                                MessageToLiving(ad.Target, string.Format("You convert {0} damage into " + damageConverted.ToString() + " Health.", damageConverted.ToString()), eChatType.CT_Spell);
                                MessageToLiving(ad.Attacker, string.Format("A magical spell absorbs {0} damage of your attack!", damageConverted.ToString()), eChatType.CT_Spell);
                                if (Caster.Health != Caster.MaxHealth)
                                {
                                    MessageToCaster("You convert " + damageConverted.ToString() + " damage into health.", eChatType.CT_Spell);
                                    Caster.Health = Caster.Health + damageConverted;
                                }
                                else
                                {
                                    MessageToCaster("You cannot convert anymore health!", eChatType.CT_Spell);
                                }

                                if (Caster.Endurance != Caster.MaxEndurance)
                                {
                                    MessageToCaster("You convert " + damageConverted.ToString() + " damage into endurance", eChatType.CT_Spell);
                                    Caster.Endurance = Caster.Endurance + damageConverted;
                                }
                                else
                                {
                                    MessageToCaster("You cannot convert anymore endurance!", eChatType.CT_Spell);
                                }
                                if (Caster.Mana != Caster.MaxMana)
                                {
                                    MessageToCaster("You convert " + damageConverted.ToString() + " damage into mana.", eChatType.CT_Spell);
                                    Caster.Mana = Caster.Mana + damageConverted;
                                }
                                else
                                {
                                    MessageToCaster("You cannot convert anymore mana!", eChatType.CT_Spell);
                                }
                            }
                            break;
                    }
                }
            }

            public MagicConversionSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
        }

        [SpellHandlerAttribute("AttackDamageConversion")]//only hits from Caster
        public class AttackDamageConversionSpellHandler : SpellHandler
        {
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            int spellDamage = 0;

            public const string AttackConvertDamage = "Conversion";

            public override void FinishSpellCast(GameLiving target)
            {
                m_caster.Mana -= PowerCost(target);
                base.FinishSpellCast(target);
            }

            public override void OnEffectStart(GameSpellEffect effect)
            {
                effect.Owner.TempProperties.setProperty(AttackConvertDamage, 100000);
                GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackFinished, new DOLEventHandler(OnAttack));
                GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.CastFinished, new DOLEventHandler(OnAttack));

                eChatType toLiving = (Spell.Pulse == 0) ? eChatType.CT_Spell : eChatType.CT_SpellPulse;
                eChatType toOther = (Spell.Pulse == 0) ? eChatType.CT_System : eChatType.CT_Spell;
                MessageToLiving(effect.Owner, Spell.Message1, toLiving);
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), toOther, effect.Owner);
            }

            public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
            {
                GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackFinished, new DOLEventHandler(OnAttack));
                GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.CastFinished, new DOLEventHandler(OnAttack));
                effect.Owner.TempProperties.removeProperty(AttackConvertDamage);
                return 1;
            }

            protected virtual void OndamageConverted(AttackData ad, int DamageAmount)
            {
            }


            private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
            {
                GameLiving living = sender as GameLiving;
                if (living == null) return;
                AttackFinishedEventArgs AttackFinished = arguments as AttackFinishedEventArgs;
                CastingEventArgs castFinished = arguments as CastingEventArgs;
                AttackData ad = null;
                ISpellHandler sp = null;


                //Melee damage
                if (AttackFinished != null)
                {
                    ad = AttackFinished.AttackData;

                }
                //Cast damage
                if (castFinished != null)
                {
                    sp = castFinished.SpellHandler;
                    ad = castFinished.LastAttackData;

                }
                if (sp == null && ad == null)
                {
                    return;
                }

                else if (sp != null && (sp.HasPositiveEffect) || ad == null)
                {
                    return;
                }
                if (ad != null && sp != null)
                {
                    //hole damage wert vom Spellhandler
                    //get Spell Damage for the last cast
                    spellDamage = Caster.TempProperties.getProperty<int>(SpellHandler.LAST_ATTACK_DAMAGE);

                }

                //int reduceddmg = living.TempProperties.getProperty<int>(ConvertDamage);
                double damagePercent = Spell.Value;
                int damageConverted = (ad.Damage + ad.CriticalDamage + spellDamage);
                double adddamagePercent = 0;
                if (damageConverted > 0)
                {
                    adddamagePercent = damageConverted * damagePercent / 100;
                    OndamageConverted(ad, (int)adddamagePercent);
                }

                if (Caster.Endurance != Caster.MaxEndurance && adddamagePercent > 0)
                {
                    MessageToCaster("You convert " + ((int)adddamagePercent).ToString() + " damage into endurance", eChatType.CT_Spell);
                    Caster.Endurance = Caster.Endurance + (int)adddamagePercent;
                }
                else
                {
                    MessageToCaster("You cannot convert anymore endurance!", eChatType.CT_Spell);
                }
                if (Caster.Mana != Caster.MaxMana && adddamagePercent > 0)
                {
                    MessageToCaster("You convert " + ((int)adddamagePercent).ToString() + " damage into mana.", eChatType.CT_Spell);
                    Caster.Mana = Caster.Mana + (int)adddamagePercent;
                }
                else
                {
                    MessageToCaster("You cannot convert anymore mana!", eChatType.CT_Spell);
                }
                //l�sche prpertie nach gebrauch
                m_caster.TempProperties.removeProperty(SpellHandler.LAST_ATTACK_DAMAGE);
            }

            public override IList<string> DelveInfo(GamePlayer player)
            {
                {
                    var list = new List<string>();
                    list.Add("Name: " + Spell.Name);
                    list.Add("Description: " + Spell.Description);
                    list.Add("Target: " + Spell.Target);
                    if (Spell.Value != 0)
                    {
                        //list.Add("Health Return: " + Spell.Value + "%");
                        list.Add("Power Return: " + Spell.Value.ToString() + "%");
                        list.Add("Endurance Return: " + Spell.Value.ToString() + "%");
                    }
                    return list;
                }
            }
            public AttackDamageConversionSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
        }
    }
}
