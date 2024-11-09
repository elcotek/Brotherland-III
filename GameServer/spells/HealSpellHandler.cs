
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

using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;
using DOL.AI.Brain;
using DOL.Language;



namespace DOL.GS.Spells
{
    using DOL.Events;
    using Effects;
    using log4net;
    using System.Reflection;

    /// <summary>
    /// 
    /// </summary>
    [SpellHandlerAttribute("Heal")]
    public class HealSpellHandler : SpellHandler
    {
        const string SpamTime = "NOCHARMEFFECTSPAM";

        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // constructor
        public HealSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
        /// <summary>
        /// Execute heal spell
        /// </summary>
        /// <param name="target"></param>
        public override bool StartSpell(GameLiving target)
        {
            GameNPC ControlledBody = null;
            GameLiving oldTarget = null;

            if (target is GamePlayer && ((GamePlayer)target).Client.Account.PrivLevel > 1) 
                return false;

            if (target != null)
                oldTarget = target;//save the old target


            //Necromancer pets
            if (target is GamePlayer && (target as GamePlayer).CharacterClass.ID == (byte)eCharacterClass.Necromancer
                && (target as GamePlayer).EffectList.GetOfType<NecromancerShadeEffect>() != null && (target as GamePlayer).HealthPercent == 100)
            {
                if ((target as GamePlayer).ControlledBrain != null && Spell.Target != "Group" && Spell.Radius <= 0)
                {
                    ControlledBody = ((target as GamePlayer).ControlledBrain).Body;
                }

                if (ControlledBody != null && ControlledBody.IsAlive && ControlledBody.Brain.IsActive)
                    target = ControlledBody;
                Caster.TargetObject = target;

                (Caster as GamePlayer).Out.SendChangeTarget(target);

            }


            var targets = SelectTargets(target);
            if (targets.Count <= 0) return false;

            bool healed = false;
            int minHeal;
            int maxHeal;

            CalculateHealVariance(out minHeal, out maxHeal);

            foreach (GameLiving healTarget in targets)
            {
                int heal = Util.Random(minHeal, maxHeal);

                //no heal bonis for procs
                if (Spell.IsProcSpell)
                {
                    if (Spell.Value > 0)
                    heal = (int)Spell.Value;
                }
                   
                if (SpellLine.KeyName == GlobalSpellsLines.Item_Effects)
                {
                    heal = maxHeal;
                }

                if (healTarget.IsDiseased)
                {
                    if (m_caster is GamePlayer)
                    {
                        // MessageToCaster("Your target is diseased!", eChatType.CT_SpellResisted);
                        ((GamePlayer)m_caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client.Account.Language, "HealSpellHandler.Execute.YourTargetIsDiseased", healTarget.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                    heal >>= 1;
                }

                if (SpellLine.KeyName == GlobalSpellsLines.Item_Effects)
                {
                    healed |= ProcHeal(healTarget, heal);
                }
                else
                {
                    healed |= HealTarget(healTarget, heal);
                }
            }

            // group heals seem to use full power even if no heals
            if (!healed && Spell.Target.ToLower() == "realm")
                m_caster.Mana -= PowerCost(target) * (int)DOL.GS.ServerProperties.Properties.HEAL_POWERCOST_MULTIPLICATOR >> 1; // only 1/2 power if no heal
            else
                m_caster.Mana -= PowerCost(target) * (int)DOL.GS.ServerProperties.Properties.HEAL_POWERCOST_MULTIPLICATOR;


            // send animation for non pulsing spells only
            if (Spell.Pulse == 0)
            {
                // show resisted effect if not healed
                foreach (GameLiving healTarget in targets)
                    if (healTarget.IsAlive)
                        SendEffectAnimation(healTarget, 0, false, healed ? (byte)1 : (byte)0);
            }

            if (!healed && Spell.CastTime == 0)
            {

                if (m_caster is GamePlayer)
                {
                    long UPDATETICK = m_caster.TempProperties.getProperty<long>(SpamTime);
                    long changeTime = m_caster.CurrentRegion.Time - UPDATETICK;

                    if (changeTime > 1 * 500 || UPDATETICK == 0)
                    {
                        m_caster.TempProperties.setProperty(SpamTime, m_caster.CurrentRegion.Time);
                        m_startReuseTimer = false;
                    }
                    else if (m_spell.RecastDelay > 0)
                    {
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "Do not spam this spell."), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        // m_startReuseTimer = false;
                    }
                }
            }
          

            return true;
        }

        /// <summary>
        /// Heals hit points of one target and sends needed messages, no spell effects
        /// </summary>
        /// <param name="target"></param>
        /// <param name="amount">amount of hit points to heal</param>
        /// <returns>true if heal was done</returns>
        public virtual bool HealTarget(GameLiving target, int amount)
        {
            return HealTarget(target, (double)amount);
        }

        private bool IsSamerealm(GameLiving caster, GameLiving target)
        {
            if (target.Realm == Caster.Realm)
                return true;

            if (target is GameNPC && ((target as GameNPC).Flags & GameNPC.eFlags.PEACE) != 0)//peace must be friendly
                return true;

            return false;
        }

        /// <summary>
        /// Heals hit points of one target and sends needed messages, no spell effects
        /// </summary>
        /// <param name="target"></param>
        /// <param name="amount">amount of hit points to heal</param>
        /// <returns>true if heal was done</returns>
        public virtual bool HealTarget(GameLiving target, double amount)
        {
            if (target == null || target.ObjectState != GameLiving.eObjectState.Active) return false;
            if (Caster == null || Caster.ObjectState != GameLiving.eObjectState.Active) return false;

           // if (target.Realm != Caster.Realm)
               // return false;

            // we can't heal enemy people
            //if (!GameServer.ServerRules.IsSameRealm(Caster, target, true))
            if (!IsSamerealm(Caster, target))
                return false;

            // no healing of keep components
            if (target is Keeps.GameKeepComponent || target is Keeps.GameKeepDoor)
                return false;




            if (!target.IsAlive)
            {
                //"You cannot heal the dead!" sshot550.tga
                // MessageToCaster(target.GetName(0, true) + " is dead!", eChatType.CT_SpellResisted);
                if (m_caster is GamePlayer)
                {
                    ((GamePlayer)m_caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client.Account.Language, "HealSpellHandler.Execute.TargetIsDead", target.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
                return false;
            }

            if (target is GamePlayer && (target as GamePlayer).NoHelp && Caster is GamePlayer)
            {
                //player not grouped, anyone else
                //player grouped, different group
                if ((target as GamePlayer).Group == null ||
                    (Caster as GamePlayer).Group == null ||
                    (Caster as GamePlayer).Group != (target as GamePlayer).Group)
                {
                    //MessageToCaster("That player does not want assistance", eChatType.CT_SpellResisted);
                    ((GamePlayer)m_caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client.Account.Language, "HealSpellHandler.Execute.PlayerDoesNotWantAssistance", target.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    return false;
                }
            }

            //moc heal decrease
            double mocFactor = 1.0;
            MasteryofConcentrationEffect moc = Caster.EffectList.GetOfType<MasteryofConcentrationEffect>();
            if (moc != null)
            {
                GamePlayer playerCaster = Caster as GamePlayer;
                MasteryofConcentrationAbility ra = playerCaster.GetAbility<MasteryofConcentrationAbility>();
                if (ra != null)
                    mocFactor = (double)ra.GetAmountForLevel(ra.Level) / 100.0;
                amount = amount * mocFactor;
            }
            double criticalvalue = 0;
            int criticalchance = Caster.GetModified(eProperty.CriticalHealHitChance);
            double effectiveness = 0;

            if (Caster is GamePlayer && Spell.IsProcSpell)
            {
               effectiveness = 1.0;
            }
            else if (Caster is GamePlayer)
            {
               effectiveness = (Caster as GamePlayer).Effectiveness + (double)(Caster.GetModified(eProperty.HealingEffectiveness)) * 0.01;
            }
            if (Caster is GameNPC)
                effectiveness = 1.0;

            //USE DOUBLE !
            double cache = amount * effectiveness;

            amount = cache;

            //no heal bonis for procs
            if (Spell.IsProcSpell == false)
            {
                if (Util.Chance(criticalchance))
                {
                    double minValue = amount / 10;
                    double maxValue = amount / 2 + 1;
                    criticalvalue = Util.RandomDouble() * (maxValue - minValue) + minValue;
                }
            }

            amount += criticalvalue;

            if (target != null && target is GamePlayer)
            {
                GamePlayer playerTarget = target as GamePlayer;
                GameSpellEffect HealEffect = SpellHandler.FindEffectOnTarget(playerTarget, "EfficientHealing");
                if (HealEffect != null)
                {
                    double HealBonus = amount * ((int)HealEffect.Spell.Value * 0.01);
                    amount += (int)HealBonus;
                    //playerTarget.Out.SendMessage("Your Efficient Healing buff grants you a additional" + HealBonus + " in the Heal!", eChatType.CT_Spell, eChatLoc.CL_ChatWindow);
                    ((GamePlayer)playerTarget).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)playerTarget).Client.Account.Language, "HealSpellHandler.Execute.YourEfficientHealingbuff", HealBonus), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }
                GameSpellEffect EndEffect = SpellHandler.FindEffectOnTarget(playerTarget, "EfficientEndurance");
                if (EndEffect != null)
                {
                    double EndBonus = amount * ((int)EndEffect.Spell.Value * 0.01);
                    //600 / 10 = 60end
                    playerTarget.Endurance += (int)EndBonus;
                    ((GamePlayer)playerTarget).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)playerTarget).Client.Account.Language, "HealSpellHandler.Execute.YourEfficientEndurancebuff", EndBonus), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    //playerTarget.Out.SendMessage("Your Efficient Endurance buff grants you " + EndBonus + " Endurance from the Heal!", eChatType.CT_Spell, eChatLoc.CL_ChatWindow);
                }
            }

            GameSpellEffect flaskHeal = FindEffectOnTarget(target, "HealFlask");
            if (flaskHeal != null)
            {
                GameSpellEffect CombatHeal = SpellHandler.FindEffectOnTarget(target, "CombatHeal");
                if (CombatHeal == null)
                {
                    amount += (int)((amount * flaskHeal.Spell.Value) * 0.01);
                }
            }


            // Scale spells that are cast by pets
            if (Caster is GamePet && !(Caster is NecromancerPet) && ServerProperties.Properties.PET_SCALE_SPELL_MAX_LEVEL > 0)
                amount = ScalePetHeal(amount);

            int heal = target.ChangeHealth(Caster, GameLiving.eHealthChangeType.Spell, (int)Math.Round(amount));

            #region PVP DAMAGE

            long healedrp = 0;

            if (target.DamageRvRMemory > 0 &&
                (target is NecromancerPet &&
                ((target as NecromancerPet).Brain as IControlledBrain).GetPlayerOwner() != null
                || target is GamePlayer))
            {
                healedrp = (long)Math.Max(heal, 0);
                target.DamageRvRMemory -= healedrp;
            }

            if (heal == 0)
            {
                if (Spell.Pulse == 0)
                {
                    if (target == m_caster)

                        if (target is GamePlayer)
                        {
                            //  MessageToCaster("You are fully healed.", eChatType.CT_SpellResisted);
                            ((GamePlayer)target).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)target).Client.Account.Language, "SpellHandler.SendCastMessage.FullyHealed"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                        }
                        else
                        {

                        }

                    else
                    {
                        if (m_caster is GamePlayer)
                        {
                            //  MessageToCaster("You are fully healed.", eChatType.CT_SpellResisted);
                            ((GamePlayer)m_caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client.Account.Language, "SpellHandler.SendCastMessage.OtherTargetFullyHealed", target.Name), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                        }
                        else
                        {

                        }
                    }
                    // MessageToCaster(target.GetName(0, true) + " is fully healed.", eChatType.CT_SpellResisted);
                }
                return false;
            }

            if (m_caster is GamePlayer && target is NecromancerPet &&
                ((target as NecromancerPet).Brain as IControlledBrain).GetPlayerOwner() != null
                || target is GamePlayer && healedrp > 0)
            {
                int HEALRP_VALUE_RP = ServerProperties.Properties.HEAL_PVP_DAMAGE_VALUE_RP; // ...% de bonus RP pour les soins effectués

                if (m_spell.Pulse == 0 && m_caster.CurrentRegionID != 242 && // On Exclu zone COOP
                    m_spell.SpellType.ToLower() != "spreadheal" && target != m_caster &&
                    m_spellLine.KeyName != GlobalSpellsLines.Item_Spells &&
                    m_spellLine.KeyName != GlobalSpellsLines.Potions_Effects &&
                    m_spellLine.KeyName != GlobalSpellsLines.Combat_Styles_Effect &&
                    m_spellLine.KeyName != GlobalSpellsLines.Reserved_Spells)
                {
                    GamePlayer player = m_caster as GamePlayer;

                    if (player != null)
                    {
                        long Bonus_RP_Value = Convert.ToInt64((double)healedrp * HEALRP_VALUE_RP / 100.0);

                        if (Bonus_RP_Value >= 1)
                        {
                            PlayerStatistics stats = player.Statistics as PlayerStatistics;

                            if (stats != null)
                            {
                                stats.RPEarnedFromHitPointsHealed += (uint)Bonus_RP_Value;
                                stats.HitPointsHealed += (uint)healedrp;
                            }

                            player.GainRealmPoints(Bonus_RP_Value, false);
                            player.Out.SendMessage("You Gain " + Bonus_RP_Value.ToString() + "extra rps for you realm.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        }
                    }
                }
            }

            #endregion PVP DAMAGE

            if (m_caster == target && m_caster is GamePlayer)
            {

                //MessageToCaster("You heal yourself for " + heal + " hit points.", eChatType.CT_Spell);
                ((GamePlayer)m_caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client.Account.Language, "HealSpellHandler.Execute.HealYourself", heal), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                if (heal < amount)
                {
                    #region PVP DAMAGE

                    if (target is NecromancerPet &&
                        ((target as NecromancerPet).Brain as IControlledBrain).GetPlayerOwner() != null || target is GamePlayer)
                    {
                        if (target.DamageRvRMemory > 0)
                            target.DamageRvRMemory = 0; //Remise a zéro compteur dommages/heal rps
                    }

                    #endregion PVP DAMAGE

                    if (m_caster is GamePlayer)
                    {
                        ((GamePlayer)m_caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client.Account.Language, "SpellHandler.SendCastMessage.FullyHealed"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    }
                }
            }
            else
            {
                if (m_caster is GamePlayer)
                {
                    //MessageToCaster("You heal " + target.GetName(0, false) + " for " + heal + " hit points!", eChatType.CT_Spell);
                    (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "HealSpellHandler.Execute.YouHealTarget", target.Name, heal), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }
                if (target is GamePlayer)
                {
                    (target as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((target as GamePlayer).Client.Account.Language, "HealSpellHandler.Execute.YouAreHealedBy", m_caster.Name, heal), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }
                if (heal < amount)
                {

                    #region PVP DAMAGE

                    if (target is NecromancerPet &&
                        ((target as NecromancerPet).Brain as IControlledBrain).GetPlayerOwner() != null || target is GamePlayer)
                    {
                        if (target.DamageRvRMemory > 0)
                            target.DamageRvRMemory = 0; //Remise a zéro compteur dommages/heal rps
                    }

                    #endregion PVP DAMAGE

                    if (m_caster is GamePlayer)
                    {
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "HealSpellHandler.Execute.TargetIsFullyHealed", target.Name), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                        //MessageToCaster(target.GetName(0, true) + " is fully healed.", eChatType.CT_Spell);
                    }
                }
                if (heal > 0 && criticalvalue > 0)
                    if (m_caster is GamePlayer)
                    {
                        ((GamePlayer)m_caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client.Account.Language, "HealSpellHandler.Execute.YouHealCriticalFor", criticalvalue), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                        //MessageToCaster("Your heal criticals for an extra " + criticalvalue + " amount of hit points!", eChatType.CT_Spell);
                    }
            }
            return true;
        }

        /// <summary>
        /// A heal generated by an item proc.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public virtual bool ProcHeal(GameLiving target, int amount)
        { return ProcHeal(target, (double)amount); }


        /// <summary>
        /// A heal generated by an item proc.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public virtual bool ProcHeal(GameLiving target, double amount)
        {
            if (target == null || target.ObjectState != GameLiving.eObjectState.Active) return false;
            if (Caster == null || Caster.ObjectState != GameLiving.eObjectState.Active) return false;

            // we can't heal enemy people
            if (!GameServer.ServerRules.IsSameRealm(Caster, target, true))
                return false;

            if (!target.IsAlive)
                return false;

            // no healing of keep components
            if (target is Keeps.GameKeepComponent || target is Keeps.GameKeepDoor)
                return false;

            // Scale spells that are cast by pets
            if (Caster is GamePet && !(Caster is NecromancerPet) && ServerProperties.Properties.PET_SCALE_SPELL_MAX_LEVEL > 0)
                amount = ScalePetHeal(amount);

            int heal = target.ChangeHealth(Caster, GameLiving.eHealthChangeType.Spell, (int)Math.Round(amount));

            if (m_caster == target && heal > 0)
            {
                if (m_caster is GamePlayer)
                {
                    (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "HealSpellHandler.Execute.HealYourself", heal), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    //MessageToCaster("You heal yourself for " + heal + " hit points.", eChatType.CT_Spell);
                }
                if (heal < amount)
                {
                    if (m_caster is GamePlayer)
                    {
                        ((GamePlayer)m_caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client.Account.Language, "SpellHandler.SendCastMessage.FullyHealed"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    }
                    else
                    {

                    }
                    #region PVP DAMAGE

                    if (target is NecromancerPet &&
                        ((target as NecromancerPet).Brain as IControlledBrain).GetPlayerOwner() != null || target is GamePlayer)
                    {
                        if (target.DamageRvRMemory > 0)
                            target.DamageRvRMemory = 0; //Remise a zéro compteur dommages/heal rps
                    }

                    #endregion PVP DAMAGE
                }
            }
            else if (heal > 0)
            {
                if (m_caster is GamePlayer)
                {
                    (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "HealSpellHandler.Execute.YouHealTarget", target.Name, heal), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
                //MessageToCaster("You heal " + target.GetName(0, false) + " for " + heal + " hit points!", eChatType.CT_Spell);
                if (target is GamePlayer)
                {
                    (target as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((target as GamePlayer).Client.Account.Language, "HealSpellHandler.Execute.YouAreHealedBy", m_caster.Name, heal), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }

                #region PVP DAMAGE

                if (heal < amount)
                {
                    if (target is NecromancerPet &&
                        ((target as NecromancerPet).Brain as IControlledBrain).GetPlayerOwner() != null || target is GamePlayer)
                    {
                        if (target.DamageRvRMemory > 0)
                            target.DamageRvRMemory = 0; //Remise a zéro compteur dommages/heal rps
                    }
                }
                else
                {
                    if (target is NecromancerPet &&
                        ((target as NecromancerPet).Brain as IControlledBrain).GetPlayerOwner() != null || target is GamePlayer)
                    {
                        if (target.DamageRvRMemory > 0)
                            target.DamageRvRMemory -= (long)Math.Max(heal, 0);
                    }
                }
            }

            #endregion PVP DAMAGE

            return true;
        }


        /// <summary>
        /// Calculates heal variance based on spec
        /// </summary>
        /// <param name="min">store min variance here</param>
        /// <param name="max">store max variance here</param>
        public virtual void CalculateHealVariance(out int min, out int max)
        {
            double spellValue = m_spell.Value;
            GamePlayer casterPlayer = m_caster as GamePlayer;

            if (m_spellLine.KeyName == GlobalSpellsLines.Item_Effects)
            {
                if (m_spell.Value > 0)
                {
                    min = (int)(spellValue * 0.75);
                    max = (int)(spellValue * 1.25);
                    return;
                }
            }

            if (m_spellLine.KeyName == GlobalSpellsLines.Potions_Effects)
            {
                if (m_spell.Value > 0)
                {
                    min = (int)(spellValue * 1.00);
                    max = (int)(spellValue * 1.25);
                    return;
                }
            }

            if (m_spellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect)
            {
                if (m_spell.Value > 0)
                {
                    if (UseMinVariance)
                    {
                        min = (int)(spellValue * 1.25);
                    }
                    else
                    {
                        min = (int)(spellValue * 0.75);
                    }

                    max = (int)(spellValue * 1.25);
                    return;
                }
            }

            if (m_spellLine.KeyName == GlobalSpellsLines.Reserved_Spells)
            {
                min = max = (int)spellValue;
                return;
            }

            // percents if less than zero
            if (spellValue < 0)
            {
                spellValue = (spellValue / -100.0) * m_caster.MaxHealth;

                min = max = (int)spellValue;
                return;
            }

            int upperLimit = (int)(spellValue * 1.25);
            if (upperLimit < 1)
            {
                upperLimit = 1;
            }

            double eff = 1.25;
            if (Caster is GamePlayer)
            {
                double lineSpec = Caster.GetModifiedSpecLevel(m_spellLine.Spec);
                if (lineSpec < 1)
                    lineSpec = 1;
                eff = 0.25;
                if (Spell.Level > 0)
                {
                    eff += (lineSpec - 1.0) / Spell.Level;
                    if (eff > 1.25)
                        eff = 1.25;
                }
            }

            int lowerLimit = (int)(spellValue * eff);
            if (lowerLimit < 1)
            {
                lowerLimit = 1;
            }
            if (lowerLimit > upperLimit)
            {
                lowerLimit = upperLimit;
            }

            min = lowerLimit;
            max = upperLimit;
            return;
        }

        private double ScalePetHeal(double amount)
        {
            return amount * (double)(Caster.Level) / (double)(ServerProperties.Properties.PET_SCALE_SPELL_MAX_LEVEL);
        }
    }
}
