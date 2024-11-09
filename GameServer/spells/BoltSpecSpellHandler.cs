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
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.Language;
using System;


namespace DOL.GS.Spells
{
    /// <summary>
    /// Spell Handler for firing bolts
    /// </summary>
    [SpellHandlerAttribute("BoltSpec")]
    public class BoltSpecSpellHandler : SpellHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string LOSEFFECTIVENESS = "LOS Effectivness";
        const string HAS_NO_LOS = "HasNoLos";

        private bool m_castFailed = false;

        /// <summary>
        /// Fire bolt
        /// </summary>
        /// <param name="target"></param>
        public override void FinishSpellCast(GameLiving target)
        {
            if (!m_castFailed)
            {
                m_caster.Mana -= PowerCost(target);
            }
            if ((target is Keeps.GameKeepDoor || target is Keeps.GameKeepComponent) && Spell.SpellType != "SiegeArrow")
            {
                MessageToCaster(String.Format("Your spell has no effect on the {0}!", target.Name), eChatType.CT_SpellResisted);
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
            foreach (GameLiving targ in SelectTargets(target))
            {
                if (targ is GamePlayer && Spell.Target.ToLower() == "cone" && CheckLOS(Caster))
                {
                    GamePlayer player = targ as GamePlayer;
                    player.Out.SendCheckLOS(Caster, player, new CheckLOSResponse(DealDamageCheckLOS));
                }
                else
                {
                    DealDamage(targ);

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
                        // Caster.TempProperties.removeProperty(HAS_NO_LOS);
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
                        //log.Error("no Los");
                        Caster.TempProperties.setProperty(HAS_NO_LOS, target2);


                        BoltOnTargetAction bolt = new BoltOnTargetAction(Caster, target2, this);

                        bolt.Stop();
                    }
                }
                catch (Exception e)
                {
                    if (log.IsErrorEnabled)
                        log.Error(string.Format("targetOID:{0} caster:{1} exception:{2}", targetOID, Caster, e));
                }
            }
        }


        double effectiveness = 0;
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
                        Caster.TempProperties.removeProperty(HAS_NO_LOS);
                        effectiveness = player.TempProperties.getProperty<double>(LOSEFFECTIVENESS + target.ObjectID, 1.0);
                        DealDamage(target);

                        player.TempProperties.removeProperty(LOSEFFECTIVENESS + target.ObjectID);
                        // Due to LOS check delay the actual cast happens after FinishSpellCast does a notify, so we notify again
                        GameEventMgr.Notify(GameLivingEvent.CastFinished, m_caster, new CastingEventArgs(this, target, m_lastAttackData));

                    }
                }
                catch (Exception e)
                {
                    m_castFailed = true;

                    if (log.IsErrorEnabled)
                        log.Error(string.Format("targetOID:{0} caster:{1} exception:{2}", targetOID, Caster, e));
                }
            }
            else
            {
                if (Spell.Target.ToLower() == "enemy" && Spell.Radius == 0 && Spell.Range != 0)
                {
                    try
                    {

                        if (Caster != null && Caster.ObjectState == GameLiving.eObjectState.Active && Caster.CurrentRegion.GetObject(targetOID) is GameLiving)

                            target = (Caster.CurrentRegion.GetObject(targetOID) as GameLiving);

                        if (target != null && target.ObjectState == GameLiving.eObjectState.Active)
                        {
                            Caster.TempProperties.removeProperty(HAS_NO_LOS);
                            effectiveness = player.TempProperties.getProperty<double>(LOSEFFECTIVENESS + target.ObjectID, 1.0);
                            DealDamage(target);

                            player.TempProperties.removeProperty(LOSEFFECTIVENESS + target.ObjectID);
                            // Due to LOS check delay the actual cast happens after FinishSpellCast does a notify, so we notify again
                            GameEventMgr.Notify(GameLivingEvent.CastFinished, m_caster, new CastingEventArgs(this, target, m_lastAttackData));

                        }
                    }
                    catch (Exception e)
                    {
                        m_castFailed = true;

                        if (log.IsErrorEnabled)
                            log.Error(string.Format("targetOID:{0} caster:{1} exception:{2}", targetOID, Caster, e));
                    }
                }
                else
                    m_castFailed = true;
                // MessageToCaster("You can't see your target!", eChatType.CT_SpellResisted);
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

            AttackData ad = CalculateDamageToTarget(target, effectiveness);
            BoltOnTargetAction bolt = new BoltOnTargetAction(Caster, target, this);

            //Second Los Check
            if (Caster is GamePlayer)
            {
                (Caster as GamePlayer).Out.SendCheckLOS(Caster, target, new CheckLOSResponse(CheckSecondLOS));
            }

            bolt.Start(1 + ticksToTarget);

            //Second Los Check
            if (Caster is GamePlayer)
            {
                (Caster as GamePlayer).Out.SendCheckLOS(Caster, target, new CheckLOSResponse(CheckSecondLOS));
            }


            //target.StartInterruptTimer(target.SpellInterruptDuration, ad.AttackType, Caster);
        }
        #endregion

        /// <summary>
        /// Delayed action when bolt reach the target
        /// </summary>
        protected class BoltOnTargetAction : RegionAction
        {
            /// <summary>
            /// The bolt target
            /// </summary>
            protected readonly GameLiving m_boltTarget;

            /// <summary>
            /// The spell handler
            /// </summary>
            protected readonly BoltSpecSpellHandler m_handler;

            /// <summary>
            /// Constructs a new BoltOnTargetAction
            /// </summary>
            /// <param name="actionSource">The action source</param>
            /// <param name="boltTarget">The bolt target</param>
            /// <param name="spellHandler"></param>
            public BoltOnTargetAction(GameLiving actionSource, GameLiving boltTarget, BoltSpecSpellHandler spellHandler) : base(actionSource)
            {
                if (boltTarget == null)
                    throw new ArgumentNullException("boltTarget");
                if (spellHandler == null)
                    throw new ArgumentNullException("spellHandler");
                m_boltTarget = boltTarget;
                m_handler = spellHandler;
            }

            /// <summary>
            /// Called on every timer tick
            /// </summary>
            protected override void OnTick()
            {
                GameLiving target = m_boltTarget;
                GameLiving caster = (GameLiving)m_actionSource;
                if (target == null) return;
                if (target.CurrentRegionID != caster.CurrentRegionID) return;
                if (target.ObjectState != GameObject.eObjectState.Active) return;
                if (!target.IsAlive) return;
                int damageRduction = 0;


                // Related to PvP hitchance
                // http://www.camelotherald.com/news/news_article.php?storyid=2444
                // No information on bolt hitchance against npc's
                // Bolts are treated as physical attacks for the purpose of ABS only
                // Based on this I am normalizing the miss rate for npc's to be that of a standard spell

                int missrate = 0;

                if (caster is GamePlayer && target is GamePlayer)
                {
                    if (target.InCombat)
                    {
                        foreach (GameLiving attacker in target.Attackers)
                        {
                            if (attacker != caster && target.GetDistanceTo(attacker) <= 200)
                            {
                                // each attacker within 200 units adds a 20% chance to miss
                                missrate += 20;
                            }
                        }
                    }
                }



                if (target is GameNPC || caster is GameNPC)
                {
                    missrate += (int)(ServerProperties.Properties.PVE_SPELL_CONHITPERCENT * caster.GetConLevel(target));
                }

                // add defence bonus from last executed style if any
                AttackData targetAD = (AttackData)target.TempProperties.getProperty<object>(GameLiving.LAST_ATTACK_DATA, null);
                if (targetAD != null
                    && targetAD.AttackResult == GameLiving.eAttackResult.HitStyle
                    && targetAD.Style != null)
                {
                    missrate += targetAD.Style.BonusToDefense;
                }

                AttackData ad = m_handler.CalculateDamageToTarget(target, 0.5 - (caster.GetModified(eProperty.SpellDamage) * 0.01));

                //Second LosCheck
                if (caster is GamePlayer && caster.TempProperties.getProperty<GameObject>(HAS_NO_LOS, null) != null && caster.TempProperties.getProperty<GameObject>(HAS_NO_LOS, null) == ad.Target)
                {
                    ad.AttackResult = GameLiving.eAttackResult.TargetNotVisible;
                    //log.Error("no Los 2");
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

                if (Util.Chance(missrate))
                {
                    ad.AttackResult = GameLiving.eAttackResult.Missed;
                    m_handler.MessageToCaster("You miss!", eChatType.CT_YouHit);
                    m_handler.MessageToLiving(target, caster.GetName(0, false) + " missed!", eChatType.CT_Missed);
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
             
                damageRduction = (int)((double)ad.Damage * ((1.0 + caster.GetModified(eProperty.SpellDamage) * 0.01) + (ServerProperties.Properties.SPELL_BOLT_DAMAGE)));
                //log.ErrorFormat("damage vorher {0}", damageRduction);
                
                ad.Damage = damageRduction * 80 / 100; 
                //log.ErrorFormat("damage nacher {0}", ad.Damage);

                // Block
                bool blocked = false;
                if (target is GamePlayer)
                { // mobs left out yet
                    GamePlayer player = (GamePlayer)target;
                    InventoryItem lefthand = player.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
                    if (lefthand != null && (player.AttackWeapon == null || player.AttackWeapon.Item_Type == Slot.RIGHTHAND || player.AttackWeapon.Item_Type == Slot.LEFTHAND))
                    {
                        if (target.IsObjectInFront(caster, 180) && lefthand.Object_Type == (int)eObjectType.Shield)
                        {
                            double shield = 0.5 * player.GetModifiedSpecLevel(Specs.Shields);
                            double blockchance = ((player.Dexterity * 2) - 100) / 40.0 + shield + 5;
                            // Removed 30% increased chance to block, can find no clear evidence this is correct - tolakram
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
                                            (engage.Owner as GamePlayer).Out.SendMessage(engage.EngageTarget.GetName(0, true) + " has been attacked recently and you are unable to engage.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }  // Check if player has enough endurance left to engage
                                    else if (engage.Owner.Endurance < EngageAbilityHandler.ENGAGE_DURATION_LOST)
                                    {
                                        engage.Cancel(false); // if player ran out of endurance cancel engage effect
                                    }
                                    else
                                    {
                                        engage.Owner.Endurance -= EngageAbilityHandler.ENGAGE_DURATION_LOST;
                                        if (engage.Owner is GamePlayer)
                                            (engage.Owner as GamePlayer).Out.SendMessage("You concentrate on blocking the blow!", eChatType.CT_Skill, eChatLoc.CL_SystemWindow);

                                        if (blockchance < 85)
                                            blockchance = 85;
                                    }
                                }
                            }

                            if (blockchance >= Util.Random(1, 100))
                            {
                                m_handler.MessageToLiving(player, "You partially block " + caster.GetName(0, false) + "'s spell!", eChatType.CT_Missed);
                                m_handler.MessageToCaster(player.GetName(0, true) + " blocks!", eChatType.CT_YouHit);
                                blocked = true;
                            }
                        }
                    }
                }

                double effectiveness = 1.0 + (caster.GetModified(eProperty.SpellDamage) * 0.01);

                // simplified melee damage calculation
                if (blocked == false)
                {
                    // TODO: armor resists to damage type

                    double damage = m_handler.Spell.Damage / 2; // another half is physical damage
                    if (target is GamePlayer)
                        ad.ArmorHitLocation = ((GamePlayer)target).CalculateArmorHitLocation(ad);

                    InventoryItem armor = null;
                    if (target.Inventory != null)
                        armor = target.Inventory.GetItem((eInventorySlot)ad.ArmorHitLocation);

                    double ws = (caster.Level * 8 * (1.0 + (caster.GetModified(eProperty.Dexterity) - 50) / 200.0));

                    damage *= ((ws + 90.68) / (target.GetArmorAF(ad.ArmorHitLocation) + 20 * 4.67));
                    damage *= 1.0 - Math.Min(0.85, ad.Target.GetArmorAbsorb(ad.ArmorHitLocation));
                    ad.Modifier = (int)(damage * (ad.Target.GetResist(ad.DamageType) + SkillBase.GetArmorResist(armor, ad.DamageType, ad.Target)) / -100.0);
                    damage += ad.Modifier;

                    damage = damage * effectiveness;


                    if (damage < 0) damage = 0;
                    ad.Damage += (int)damage;
                }

                if (m_handler != null && m_handler.Spell.SpellType.ToLower().Contains("SiegeArrow") == false)
                {
                    ad.UncappedDamage = ad.Damage;
                    ad.Damage = (int)Math.Min(ad.Damage, m_handler.DamageCap(effectiveness));
                }

                ad.Damage = (int)(ad.Damage * caster.Effectiveness);

                if (blocked == false && ad.CriticalDamage > 0)
                {
                    int critMax = (target is GamePlayer) ? ad.Damage / 2 : ad.Damage;
                    ad.CriticalDamage = Util.Random(critMax / 10, critMax);
                }

                m_handler.SendDamageMessages(ad);
                m_handler.DamageTarget(ad, false, (blocked ? 0x02 : 0x14), false);
                target.StartInterruptTimer(target.SpellInterruptDuration, ad.AttackType, caster);
            }
        }

        // constructor
        public BoltSpecSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
