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
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;
using DOL.GS.ServerProperties;
using DOL.GS.SkillHandler;
using DOL.Language;
using System;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Spell Handler for firing arrows
    /// </summary>
    public class ArrowSpellHandler : SpellHandler
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        const string SpamCritTime = "SPAMCRITTIME";

        const string HAS_NO_LOS = "HasNoLos";
        /// <summary>
        /// Does this spell break stealth on start?
        /// </summary>
        public override bool UnstealthCasterOnStart
        {
            get { return false; }
        }

        /// <summary>
		/// Fire arrow
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Endurance -= CalculateEnduranceCost();
            if (m_caster.Endurance < Spell.Power)
            {
                if (m_caster is GamePlayer)
                {
                    //MessageToCaster(String.Format("You can't shot, your endurance is too low to shot at {0}!", target.Name), eChatType.CT_SpellResisted);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "ArrowSpellHandler.YouCantShotEnduranceIsToLow", target.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
                return;
            }
            base.FinishSpellCast(target);
        }


        #region LOS Checks for Keeps
        /// <summary>
        /// called when spell effect has to be started and applied to targets
        /// </summary>
        public override bool StartSpell(GameLiving target)
        {
            int targetCount = 0;

            foreach (GameLiving targ in SelectTargets(target))
            {
                DealDamage(targ);
                targetCount++;

                if (Spell.Target.ToLower() == "area" && targetCount >= Spell.Value)
                {
                    // Volley is limited to Volley # + 2 targets.  This number is stored in Spell.Value
                    break;
                }
            }

            return true;
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

        //Secound LosCheck
        private void CheckSecondLOS(GamePlayer player, ushort response, ushort targetOID)
        {
            GameLiving target = null;
            GameLiving target2 = null;
            if ((response & 0x100) == 0x100)
            {
                try
                {

                    if (Caster != null && Caster.ObjectState == GameLiving.eObjectState.Active && Caster.CurrentRegion.GetObject(targetOID) is GameLiving)

                        target = (Caster.CurrentRegion.GetObject(targetOID) as GameLiving);

                    if (target != null && target.ObjectState == GameLiving.eObjectState.Active)
                    {
                        Caster.TempProperties.removeProperty(HAS_NO_LOS);
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
                try
                {
                    if (Caster.CurrentRegion.GetObject(targetOID) is GameLiving)


                        target2 = (Caster.CurrentRegion.GetObject(targetOID) as GameLiving);

                    if (target2 != null)
                    {

                        Caster.TempProperties.setProperty(HAS_NO_LOS, target2);


                        ArrowOnTargetAction arrow = new ArrowOnTargetAction(Caster, target2, this);

                        arrow.Stop();
                    }
                }
                catch (Exception e)
                {
                    if (log.IsErrorEnabled)
                        log.Error(string.Format("targetOID:{0} caster:{1} exception:{2}", targetOID, Caster, e));
                }
            }
        }


        private void DealDamageCheckLOS(GamePlayer player, ushort response, ushort targetOID)
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
                        DealDamage(target);
                    }
                }
                catch (Exception e)
                {
                    if (log.IsErrorEnabled)
                        log.Error(string.Format("targetOID:{0} caster:{1} exception:{2}", targetOID, Caster, e));
                }
            }
        }



        private void DealDamage(GameLiving target)
        {
            int ticksToTarget = m_caster.GetDistanceTo(target) * 100 / 85; // 85 units per 1/10s
            int delay = 1 + ticksToTarget / 100;
            foreach (GamePlayer player in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                player.Out.SendSpellEffectAnimation(m_caster, target, m_spell.ClientEffect, (ushort)(delay), false, 1);
            }
            ArrowOnTargetAction arrow = new ArrowOnTargetAction(Caster, target, this);

            //Second Los Check
            if (Caster is GamePlayer)
            {
                (Caster as GamePlayer).Out.SendCheckLOS(Caster, target, new CheckLOSResponse(CheckSecondLOS));
            }
            arrow.Start(1 + ticksToTarget);

        }
        #endregion

        /// <summary>
        /// The spell that we want to handle
        /// </summary>
        protected Spell m_spells;

        /// <summary>
        /// The Spell
        /// </summary>
        private Spell Spells
        {
            get { return m_spell; }
        }

        public virtual double DamageBowCap(double effectiveness)
        {
            //EDPS = effective dps of the weapon
            double EDPS = ((Caster.AttackWeapon.DPS_AF / 10.0));

            if ((Caster as GamePlayer).RealmLevel < 40 && EDPS >= 16.5)
            {
                EDPS -= 0.3;
            }
            //log.ErrorFormat("DPS = {0}", EDPS);

            //EDPS * (your WS / target AF) *(1 - absorb) * slow weap bonus *SPD * 2h weapon bonus* Arrow Bonus
            double damageCap = EDPS * (double)Caster.AttackWeapon.SPD_ABS * 3 * (1 + (Caster.AttackWeapon.SPD_ABS - 2) * 0.03) * (1.1 + (0.005 * Caster.GetModifiedSpecLevel(Specs.Archery)));



            return Math.Max(damageCap, Spells.Damage * Caster.Effectiveness);// (Spells.Damage * Caster.Effectiveness, Spells.Damage * ServerProperties.Properties.BOW_Archery_Effectiveness * effectiveness);


            //Damage Cap = EDPS * SPD * 3 * (1 + (SPD – 2) *.03) *(1.1 + (0.005 x spec) )
        }



        /// <summary>
        /// Delayed action when arrow reach the target
        /// </summary>
        protected class ArrowOnTargetAction : RegionAction
        {
            /// <summary>
            /// The arrow target
            /// </summary>
            protected readonly GameLiving m_arrowTarget;

            /// <summary>
            /// The spell handler
            /// </summary>
            protected readonly ArrowSpellHandler m_handler;


            /// <summary>
            /// Constructs a new ArrowOnTargetAction
            /// </summary>
            /// <param name="actionSource">The action source</param>
            /// <param name="arrowTarget">The arrow target</param>
            /// <param name="spellHandler"></param>
            public ArrowOnTargetAction(GameLiving actionSource, GameLiving arrowTarget, ArrowSpellHandler spellHandler)
                : base(actionSource)
            {
                if (arrowTarget == null)
                    throw new ArgumentNullException("arrowTarget");
                if (spellHandler == null)
                    throw new ArgumentNullException("spellHandler");
                m_arrowTarget = arrowTarget;
                m_handler = spellHandler;
            }

            /// <summary>
            /// Called on every timer tick
            /// </summary>
            public virtual void OnTickBase()
            {
                OnTick();
            }
            /// <summary>
            /// AttackData result for this spell, if any
            /// </summary>
            public AttackData LastAttackData
            {
                get { return m_lastAttackData; }
            }
            /// <summary>
            /// AttackData result for this spell, if any
            /// </summary>
            protected AttackData m_lastAttackData = null;

            protected override void OnTick()
            {
                GameLiving target = m_arrowTarget;
                GameLiving caster = (GameLiving)m_actionSource;
                if (target == null || !target.IsAlive || target.ObjectState != GameObject.eObjectState.Active || target.CurrentRegionID != caster.CurrentRegionID) return;

                int missrate = 100 - m_handler.CalculateToHitChance(target);
                // add defence bonus from last executed style if any
                AttackData targetAD = (AttackData)target.TempProperties.getProperty<object>(GameLiving.LAST_ATTACK_DATA, null);
                if (targetAD != null
                    && targetAD.AttackResult == GameLiving.eAttackResult.HitStyle
                    && targetAD.Style != null)
                {
                    missrate += targetAD.Style.BonusToDefense;
                }




                // half of the damage is magical
                // subtract any spelldamage bonus and re-calculate after half damage is calculated
                AttackData ad = m_handler.CalculateDamageToTarget(target, 0.5 - (caster.GetModified(eProperty.SpellDamage) * 0.01));
                // log.ErrorFormat("CalculateDamageToTarget {0}", ad.Damage);

                // check for bladeturn miss
                bool ArrowBlock = false;

                //Second LosCheck
                if (caster is GamePlayer && caster.TempProperties.getProperty<GameObject>(HAS_NO_LOS, null) != null && caster.TempProperties.getProperty<GameObject>(HAS_NO_LOS, null) == ad.Target)
                {
                    ad.AttackResult = GameLiving.eAttackResult.TargetNotVisible;

                    if (caster is GamePlayer)
                    {
                        (caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((caster as GamePlayer).Client.Account.Language, "ArrowSpellHandler.YouMiss", target.Name), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                    }
                    if (target is GamePlayer)
                    {
                        (target as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((target as GamePlayer).Client.Account.Language, "ArrowSpellHandler.HasMiss", caster.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                    }

                    caster.TempProperties.removeProperty(HAS_NO_LOS);
                    return;
                }

                if (ad.AttackResult == GameLiving.eAttackResult.Missed)
                {
                    return;
                }

                if (Util.Chance(missrate))
                {
                    ad.AttackResult = GameLiving.eAttackResult.Missed;
                   
                    if (caster is GamePlayer)
                    {
                        (caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((caster as GamePlayer).Client.Account.Language, "ArrowSpellHandler.YouMiss", target.Name), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                    }
                    if (target is GamePlayer)
                    {
                        (target as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((target as GamePlayer).Client.Account.Language, "ArrowSpellHandler.HasMiss", caster.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                    }
                    


                    target.OnAttackedByEnemy(ad);
                    target.StartInterruptTimer(target.SpellInterruptDuration, ad.AttackType, caster);
                    if (target is GameNPC)
                    {
                        IOldAggressiveBrain aggroBrain = ((GameNPC)target).Brain as IOldAggressiveBrain;
                        if (aggroBrain != null)
                            aggroBrain.AddToAggroList(caster, 1);
                    }
                    return;
                }

                ad.Damage = (int)((double)ad.Damage * (1.0 + caster.GetModified(eProperty.SpellDamage) * 0.01));
            

                if (target is GamePlayer && !target.IsStunned && (target as GamePlayer).IsRiding == false && !target.IsMezzed && !target.IsSitting && m_handler.Spell.LifeDrainReturn != (int)Archery.eShotType.Critical)
                {
                    GamePlayer player = (GamePlayer)target;
                    InventoryItem lefthand = player.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
                    if (lefthand != null && (player.AttackWeapon == null || player.AttackWeapon.Item_Type == Slot.RIGHTHAND || player.AttackWeapon.Item_Type == Slot.LEFTHAND))
                    {
                        if (target.IsObjectInFront(caster, 180) && lefthand.Object_Type == (int)eObjectType.Shield)
                        {
                            //shield size vs number of attackers
                            int shieldSize = 1;
                            if (lefthand != null)
                            {
                                shieldSize = lefthand.Type_Damage;
                                if (player.Attackers.Count > shieldSize)
                                    shieldSize /= player.Attackers.Count - shieldSize + 1;
                                if (shieldSize < 0)
                                    shieldSize = 0;
                            }

                            double shield = 0.5 * player.GetModifiedSpecLevel(Specs.Shields);
                            double blockchance = ((player.Dexterity * 2) - 100) / 40.0 + shield + (0 * (shieldSize + 1)) + 5;
                            blockchance += 30;
                            blockchance -= target.GetConLevel(caster) * 5;
                            if (blockchance >= 100) blockchance = 99;
                            if (blockchance <= 0) blockchance = 1;

                            if (target.IsEngaging)
                            {
                                EngageEffect engage = target.EffectList.GetOfType<EngageEffect>();
                                if (engage != null && target.AttackState && engage.EngageTarget == caster)
                                {
                                    // Engage raised block change to 85% if attacker is engageTarget and player is in attackstate
                                    // You cannot engage a mob that was attacked within the last X seconds...
                                    if (engage.EngageTarget.LastAttackedByEnemyTick > engage.EngageTarget.CurrentRegion.Time - EngageAbilityHandler.ENGAGE_ATTACK_DELAY_TICK)
                                    {
                                        if (engage.Owner is GamePlayer)
                                            (engage.Owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((engage.Owner as GamePlayer).Client.Account.Language, "ArrowSpellHandler.HasMissAttackRently", engage.EngageTarget.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                        //(engage.Owner as GamePlayer).Out.SendMessage(engage.EngageTarget.GetName(0, true) + " has been attacked recently and you are unable to engage.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }  // Check if player has enough endurance left to engage
                                    else if (engage.Owner.Endurance < EngageAbilityHandler.ENGAGE_DURATION_LOST)
                                    {
                                        engage.Cancel(false); // if player ran out of endurance cancel engage effect
                                    }
                                    else
                                    {
                                        engage.Owner.Endurance -= EngageAbilityHandler.ENGAGE_DURATION_LOST;
                                        if (engage.Owner is GamePlayer)
                                            //engage.Owner as GamePlayer).Out.SendMessage("You concentrate on blocking the blow!", eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
                                            (engage.Owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((engage.Owner as GamePlayer).Client.Account.Language, "ArrowSpellHandler.YouConcentrateOnBlocking"), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
                                        if (blockchance > 99)
                                            blockchance = 99;
                                    }
                                }
                            }
                            //Console.Out.WriteLine("blockchance = {0}", blockchance);
                            if (player.HasSpecialization(Specs.Shields) && Util.ChanceDouble(blockchance * 0.01) || player.HasSpecialization(Specs.Shields) == false && Util.ChanceDouble(0.05))
                            {
                                //block but no damage
                                {
                                    ArrowBlock = true;

                                    if (player is GamePlayer)
                                    {
                                        (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client.Account.Language, "ArrowSpellHandler.YouBlockArrow", caster.Name), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
                                    }
                                    if (m_handler.Spell.Target.ToLower() != "area")
                                    {
                                        if (caster is GamePlayer)
                                        {
                                            (caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((caster as GamePlayer).Client.Account.Language, "ArrowSpellHandler.BlockYourArrow", player.Name), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
                                        }
                                    }


                                    //Combat Animation and damage if not blocked
                                    m_handler.DamageTarget(ad, false, 0x02, true);
                                }
                            }
                        }
                    }
                }

                if (ArrowBlock == false)
                {
                    // now calculate the magical part of arrow damage (similar to bolt calculation).  Part 1 Physical, Part 2 Magical
                    double damage = m_handler.Spell.Damage / 2; // another half is physical damage
                    double effectiveness = caster.Effectiveness;
                    /*
                    int dex = 0;

                    dex = caster.GetModifiedFromItems(eProperty.Dexterity);
                    double effectiveness = caster.Effectiveness;


                    if (dex > 0)
                    {
                        //effectiveness += (double)dex * 0.0010;
                    }
                    */

                    if (target is GameKeepComponent || target is GameKeepDoor || target is GameSiegeWeapon)
                    {
                        effectiveness = 1.0; // caster.Effectiveness;

                        if (Math.Max(Util.RandomDouble() + 5.1, (damage / (Util.RandomDouble() + 8.8))) > 0)
                            //min damage 5.1 and max damage/8.8
                            damage = Math.Max(Util.RandomDouble() + 5.1, (damage / (Util.RandomDouble() + 8.8))); // (ws + 90.68) + 20 * 4.67;
                        else damage = 10;
                    }
                    //Bonus percent for siegedamage
                    if ((target is GameKeepComponent || target is GameKeepDoor || target is GameSiegeWeapon) && caster is GamePlayer && caster.GetModified(eProperty.KeepDamage) > 0)
                    {
                        effectiveness += (caster.GetModified(eProperty.KeepDamage) * 0.01);
                    }
                    

                    effectiveness += (caster.GetModified(eProperty.SpellDamage) * 0.01);
                    
                    effectiveness += ((caster.GetModifiedSpecLevel(Specs.Archery)) * 0.01);


                    damage *= effectiveness;

                    if (damage < 0) damage = 0;





                    //double crit damage reduce
                    double newffectiveness = 0;
                    //log.ErrorFormat("damage0 {0} + damage {1}", ad.Damage, (int)damage);
                    ad.Damage += (int)damage;
                                       
                    if (caster.AttackWeapon != null)
                    {
                        // log.ErrorFormat("damage1 {0}", ad.Damage);
                        int weaponQuality = caster.AttackWeapon.Quality;
                        // Quality
                        ad.Damage -= (int)(ad.Damage * (caster.Level / 50) * (100 - caster.AttackWeapon.Quality) * .01);

                        //log.ErrorFormat(" caster.AttackWeapon.Quality {0}", ad.Damage);
                        // Condition
                        ad.Damage = (int)((double)ad.Damage * Math.Min(1.0, (double)caster.AttackWeapon.Condition / (double)caster.AttackWeapon.MaxCondition));
                        //log.ErrorFormat(" caster.AttackWeapon.Condition {0}", ad.Damage);

                        // Patch Note:  http://support.darkageofcamelot.com/kb/article.php?id=931
                        // - The Damage Per Second (DPS) of your bow will have an effect on your damage for archery shots. If the effective DPS
                        //   of your equipped bow is less than that of your max DPS for the level of archery shot you are using, the damage of your
                        //   shot will be reduced. Max DPS for a particular level can be found by using this equation: (.3 * level) + 1.2

                        int spellRequiredDPS = 12 + 3 * m_handler.Spell.Level;

                        if (caster.AttackWeapon.DPS_AF < spellRequiredDPS)
                        {
                            double percentReduction = (double)caster.AttackWeapon.DPS_AF / (double)spellRequiredDPS;
                            ad.Damage = (int)(ad.Damage * percentReduction);
                            //log.ErrorFormat(" ad.Damage = (int)(ad.Damage * percentReduction); {0}", ad.Damage);
                        }
                    }

                    if (ad.Damage < 0) ad.Damage = 0;
                    //  log.ErrorFormat("damage1 {0}", ad.Damage);

                    ad.UncappedDamage = ad.Damage;
                    //log.ErrorFormat(" ad.UncappedDamage = ad.Damage; {0}", ad.Damage);

                    if (caster is GamePlayer && caster.AttackWeapon != null)
                    {

                        double CritDamage = 0;
                        double DamageMultify = 0;
                        if (caster.TempProperties.getProperty<bool>(GameLiving.WasCriticalShot) == true)//Was critical shot ?
                        {
                            //random damage crit shot
                            CritDamage = (double)1.0 * 0.6;
                            //DamageMultify = 0.320;

                            DamageMultify = (double)1.0 * 0.340;

                            /*
                            if (ad.Target != null && ad.Target is GamePlayer)
                            {




                                //Damage fix againt Players
                                int specLevelLongBow = caster.GetModifiedSpecLevel(Specs.Archery);
                                if (specLevelLongBow > 51)
                                    specLevelLongBow = 51;

                                if (specLevelLongBow > 0)
                                {
                                    ad.Damage += (int)(CritDamage = 1.8 * specLevelLongBow);

                                }
                            }
                            */
                        }
                        else
                            //random damage all others
                            DamageMultify = Math.Max(0.005, (double)Util.RandomDouble() * 0.014);  //0.065;

                        //Skilllevel
                        int skillevel = caster.GetBaseSpecLevel(Specs.Archery);

                        if (skillevel > 50)
                            skillevel = 50;
                        if (skillevel < 1)
                            skillevel = 1;

                        double DPSDamageeffectiveness = 0;
                        //log.ErrorFormat("skilllevel = {0}  caster.Level = {1}", skillevel, caster.Level); // *(skillevel / caster.Level)


                        double skillevelAbzug = (double)skillevel / (double)caster.Level;
                        double dpsAF = (double)caster.AttackWeapon.DPS_AF;
                        double dpsAFResult = 0;


                        if (caster is GamePlayer && (caster as GamePlayer).RealmLevel >= 40 && dpsAF > 162)//16.5 dps RR5
                        {
                            dpsAFResult = (double)caster.AttackWeapon.DPS_AF;
                            //log.ErrorFormat("dpsAFResult1 =  {0}", dpsAFResult);
                        }
                        else if (dpsAF > 162)
                        {
                            dpsAFResult = (double)caster.AttackWeapon.DPS_AF - 3;//16.2 dps
                            //log.ErrorFormat("dpsAFResult2 =  {0}", dpsAFResult);
                        }
                        if (dpsAF <= 162) //bis-16.2 dps
                        {
                            dpsAFResult = (double)caster.AttackWeapon.DPS_AF;
                            // log.ErrorFormat("dpsAFResult3 =  {0}", dpsAFResult);
                        }
                        //Damage effectiveness calculation        
                        DPSDamageeffectiveness = (((dpsAFResult * 0.01) + DamageMultify) / effectiveness + CritDamage);


                        //SLow Weapon bonus = 1 + ( (spd - 2) x 0.03) langsamste bogen 6,3 schnellste  1,2
                        double SPD_ABS = (double)(DPSDamageeffectiveness / 2 * (double)Math.Min(63, caster.AttackWeapon.SPD_ABS - 2) * .01);
                        DPSDamageeffectiveness = SPD_ABS + DPSDamageeffectiveness;

                        //log.ErrorFormat("qually = {0}, SPD_ABS = {1}  ad.Damage {2}", qually, SPD_ABS, ad.Damage);
                        //log.ErrorFormat("DPSDamageeffectiveness = {0}, Abzug = {1}  ", DPSDamageeffectiveness, skillevelAbzug);

                        newffectiveness = DPSDamageeffectiveness * skillevelAbzug;
                        effectiveness = newffectiveness;

                        //log.ErrorFormat("m_handler.DamageBowCap davor {0}", ad.Damage);

                        ad.Damage = (int)Math.Min(ad.Damage, m_handler.DamageBowCap(effectiveness));

                    }
                    else
                    {
                       //No message
                    }
                    //Doppelhit damage / 2
                    if (caster.AttackWeapon != null && ad.Target != null && caster is GamePlayer && caster.TempProperties.getProperty<bool>(GameLiving.WasCriticalShot) == true)
                    {

                        //doppelhit underdrückung
                        long UPDATETICK = ad.Target.TempProperties.getProperty<long>(SpamCritTime);
                        long changeTime = ad.Target.CurrentRegion.Time - UPDATETICK;

                        if (changeTime >= 10 * 1000 || UPDATETICK == 0)
                        {
                            ad.Target.TempProperties.setProperty(SpamCritTime, ad.Target.CurrentRegion.Time);

                            //remove protertie
                            caster.TempProperties.removeProperty(GameLiving.WasCriticalShot);


                        }
                        else
                        {
                            //remove protertie
                            caster.TempProperties.removeProperty(GameLiving.WasCriticalShot);
                            ((GamePlayer)caster).Out.SendMessage(string.Format(LanguageMgr.GetTranslation(((GamePlayer)caster).Client.Account.Language, "SpellHandler.CriticalShot.WasDoubleHit"), ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                            ad.Damage /= 2;
                        }
                    }


                    // Moc - subtract any Melee Damage and re-calculate after half damage is calculated
                    int halfOfSpellDamage = 0;
                    int resultdamage = 0;


                    int mocFactor;
                    MasteryofConcentrationEffect moc = caster.EffectList.GetOfType<MasteryofConcentrationEffect>();

                    if (moc != null && ad.Damage > 0)
                    {
                        MasteryofConcentrationAbility ra = caster.GetAbility<MasteryofConcentrationAbility>();

                        if (ra != null)
                        {
                            mocFactor = ra.GetAmountForLevel(ra.Level);

                            halfOfSpellDamage = ad.Damage * 50 / 100;
                            //log.ErrorFormat("1# hälfteDamage = {0}, ad.Damage = {1}", hälfteDamage, ad.Damage);
                            resultdamage = halfOfSpellDamage * mocFactor / 100;
                            //log.ErrorFormat("2# mocFactor = {0}, hälfteDamage = {1}", mocFactor, hälfteDamage);
                            ad.Damage = halfOfSpellDamage + resultdamage;
                            //log.ErrorFormat("#3 Result damge = {0}, resultdamage = {1}", ad.Damage, resultdamage);

                        }
                    }



                    if (ad.CriticalDamage > 0)
                    {
                        if (m_handler.Spell.Target.ToLower() == "area")
                        {
                            ad.CriticalDamage = 0;
                        }
                        else
                        {
                            int critMax = (target is GamePlayer) ? ad.Damage / 2 : ad.Damage;
                            ad.CriticalDamage = Util.Random(critMax / 10, critMax);
                        }

                        //////////////////////////////////////////////////////////////////////////7
                        // Moc - subtract any Melee CritDamage and re-calculate after half CritDamage is calculated
                        int halfCriteDamage = 0;
                        int resultCritdamage = 0;


                        int mocCritFactor;
                        MasteryofConcentrationEffect mocCrit = caster.EffectList.GetOfType<MasteryofConcentrationEffect>();

                        if (mocCrit != null && ad.CriticalDamage > 0)
                        {
                            MasteryofConcentrationAbility ra = caster.GetAbility<MasteryofConcentrationAbility>();

                            if (ra != null)
                            {
                                mocCritFactor = ra.GetAmountForLevel(ra.Level);                       //Math.Round((double)ra.GetAmountForLevel(ra.Level) * 25 / 100, 2);

                                halfCriteDamage = ad.CriticalDamage * 50 / 100;
                                //log.ErrorFormat("1# halfCriteDamage = {0}, ad.CriticalDamage = {1}", halfCriteDamage, ad.CriticalDamage);
                                resultCritdamage = halfCriteDamage * mocCritFactor / 100;
                                //log.ErrorFormat("2# mocCritFactor = {0}, halfCriteDamage = {1}", mocCritFactor, halfCriteDamage);
                                ad.CriticalDamage = halfCriteDamage + resultCritdamage;
                                //log.ErrorFormat("#3 ad.Critical Damage = {0}, resultdamage = {1}", ad.CriticalDamage, resultCritdamage);

                            }
                        }
                    }
                    m_handler.SendDamageMessages(ad);
                    m_handler.DamageTarget(ad, false, 0x14, false);
                    target.StartInterruptTimer(target.SpellInterruptDuration, ad.AttackType, caster);
                }


                if (m_handler.Spell.SubSpellID != 0)
                {
                    Spell subspell = SkillBase.GetSpellByID(m_handler.Spell.SubSpellID);
                    if (subspell != null)
                    {
                        subspell.Level = m_handler.Spell.Level;
                        ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(m_handler.Caster, subspell, SkillBase.GetSpellLine(GlobalSpellsLines.Combat_Styles_Effect));
                        if (spellhandler != null)
                        {
                            spellhandler.StartSpell(target);
                        }
                    }
                }

                if (ArrowBlock == false && m_handler.Caster.AttackWeapon != null && GlobalConstants.IsBowWeapon((eObjectType)m_handler.Caster.AttackWeapon.Object_Type))
                {
                    if (ad.AttackResult == GameLiving.eAttackResult.HitUnstyled || ad.AttackResult == GameLiving.eAttackResult.HitStyle)
                    {
                        caster.CheckWeaponMagicalEffect(ad, m_handler.Caster.AttackWeapon);
                    }
                }
            }
        }

        // constructor
        public ArrowSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
