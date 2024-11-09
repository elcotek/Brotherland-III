

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
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DOL.GS.Spells
{

    /// <summary>
    /// Effect that stays on target and does additional
    /// damage after each melee attack
    /// </summary>
    [SpellHandler("SavageMultipleMeleeTargets")]
    public class MultipleMeleeTargetsSpellHandler : SpellHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        protected static List<GameLiving> m_targets = new List<GameLiving>();


        const string hastakenDamage = "HASTAKENDAMAGE";


        /// <summary>
        /// Constants data change this to modify chance increase or decrease
        /// </summary>
        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            // "Your weapon is blessed by the gods!"
            // "{0}'s weapon glows with the power of the gods!"
            eChatType chatType = eChatType.CT_SpellPulse;
            if (Spell.Pulse == 0)
            {
                chatType = eChatType.CT_Spell;
            }
            MessageToLiving(effect.Owner, Spell.Message1, chatType);
            Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, true)), chatType, effect.Owner);
            GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackFinished, new DOLEventHandler(EventHandler));
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (!noMessages)
            {
                MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, true)), eChatType.CT_SpellExpires, effect.Owner);
            }
            GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackFinished, new DOLEventHandler(EventHandler));
            return 0;
        }


        // <summary>
        /// Savage Multible hits
        /// </summary>
        /// <param name="ad"></param>
        /// <returns></returns>
        public static bool IsMultipleTargetsStyle(AttackData ad)
        {

            switch (ad.Style.Name)
            {
                case "Tribal Assault":
                case "Totemic Wrath":
                case "Totemic Sacrifice":
                    {
                        return true;
                    }

                default: return false;
            }
        }

        /// <summary>
        /// Savage Multiple Tagrgets
        /// </summary>
        /// <param name="ad"></param>
        /// <param name="living"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public void EventHandler(DOLEvent e, object sender, EventArgs arguments)
        {
            AttackFinishedEventArgs args = arguments as AttackFinishedEventArgs;
            if (args == null || args.AttackData == null)
            {
                return;
            }

            AttackData ad = args.AttackData;
            if (ad.AttackResult != GameLiving.eAttackResult.HitStyle)
                return;

            if (!IsMultipleTargetsStyle(ad))
                return;

            /*
            InventoryItem rightHand = Caster.AttackWeapon;
            InventoryItem leftHand = Caster.Inventory.GetItem(eInventorySlot.LeftHandWeapon);

            log.ErrorFormat("rightHand:{0}, leftHand:{1}", rightHand.Name, leftHand.Name);
            */

            ad.Damage = Caster.DamageAndResistCalculator(ad);

            string modmessage = String.Empty;
            if (ad.Modifier > 0) modmessage = " (+" + ad.Modifier.ToString() + ")";
            if (ad.Modifier < 0) modmessage = " (" + ad.Modifier.ToString() + ")";


            InventoryItem weapon = ad.Weapon;


            string hitWeapon = String.Empty;

            switch ((Caster as GamePlayer).Client.Account.Language)
            {
                case "DE":
                    if (weapon != null)
                        hitWeapon = weapon.Name;
                    break;
                default:
                    if (weapon != null)
                        hitWeapon = GlobalConstants.NameToShortName(weapon.Name);
                    break;
            }

            if (hitWeapon.Length > 0)
                hitWeapon = " " + LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "GamePlayer.Attack.WithYour") + " " + hitWeapon;

            string attackTypeMsg = LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "GamePlayer.Attack.YouAttack");


            int spellRange = CalculateSpellRange();
            // log.ErrorFormat("Spellrange {0}", spellRange);
            if (ad.IsMeleeAttack)
            {
                GameLiving livingTargets = null;


                //NPCs as target
                foreach (GameLiving targets in ad.Target.GetNPCsInRadius((ushort)spellRange))
                {
                  if (!GameServer.ServerRules.IsAllowedToAttack(targets, Caster, true))
                       continue;


                        try
                    {
                        if (targets != Caster.TargetObject)
                        {
                            livingTargets = targets;

                            if (livingTargets != null && Caster.IsObjectInFront(livingTargets, 120))

                            {
                                if (!m_targets.Contains(livingTargets) && m_targets.Count <= 3)//max 4 targets
                                {
                                    m_targets.Add(livingTargets);
                                    livingTargets.TempProperties.setProperty(hastakenDamage, livingTargets);
                                }
                            }

                            foreach (GameLiving livingTarget in m_targets)
                            {

                                if (livingTarget is GameNPC && Caster.TargetObject is GamePlayer == false && Caster.TargetObject == livingTarget == false)
                                {
                                    IOldAggressiveBrain aggroBrain = ((GameNPC)ad.Target).Brain as IOldAggressiveBrain;
                                    if (aggroBrain != null)
                                    {
                                        aggroBrain.AddToAggroList(Caster, Math.Max(1, (int)(ad.Damage * Caster.Level * 0.1)));
                                        //log.DebugFormat("Damage: {0}, Taunt Value: {1}, Taunt Amount {2}", ad.Damage, Spell.Value, Math.Max(1, (int)(Spell.Value * Caster.Level * 0.1)));
                                    }
                                }



                                if (livingTarget is GamePlayer && Caster.TargetObject == livingTarget == false  && livingTargets.TempProperties.getProperty<GameLiving>(hastakenDamage) != null)
                                {
                                    GamePlayer player = livingTarget as GamePlayer;
                                    string damageAmount = (ad.StyleDamage > 0) ? " (+" + ad.StyleDamage.ToString() + ")" : "";
                                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "StyleProcessor.ExecuteStyle.PerformPerfectly", ad.Style.Name, damageAmount), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

                                    //log.ErrorFormat("NPC Damage1: {0} targets {1}", ad.Damage, targets.Name);

                                    targets.TakeDamage(Caster, ad.DamageType, ad.Damage, 0);
                                    targets.OnAttackedByEnemy(ad);
                                    //targets.DealDamage(ad);
                                    targets.StartInterruptTimer(targets.SpellInterruptDuration, ad.AttackType, Caster);

                                    foreach (GamePlayer players in ad.Attacker.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                                    {
                                        if (players == null) continue;
                                        players.Out.SendCombatAnimation(null, targets, 0, 0, 0, 0, 0x0A, targets.HealthPercent);
                                    }

                                    livingTargets.TempProperties.removeProperty(hastakenDamage);
                                }

                                else if (livingTarget is GameNPC && Caster.TargetObject == livingTarget == false && livingTargets.TempProperties.getProperty<GameLiving>(hastakenDamage) != null)
                                {

                                    string damageAmount = (ad.StyleDamage > 0) ? " (+" + ad.StyleDamage.ToString() + ")" : "";

                                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "GamePlayer.Attack.InterceptHit", attackTypeMsg,
                                        targets.GetName(0, false, (Caster as GamePlayer).Client.Account.Language, (targets as GameNPC)), hitWeapon, ad.Damage.ToString(), modmessage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

                                    //log.ErrorFormat("NPC Damage2: {0} targets {1}", ad.Damage, targets.Name);

                                    targets.TakeDamage(Caster, ad.DamageType, ad.Damage, 0);
                                    targets.OnAttackedByEnemy(ad);
                                    //targets.DealDamage(ad);
                                    targets.StartInterruptTimer(targets.SpellInterruptDuration, ad.AttackType, Caster);

                                    foreach (GamePlayer players in ad.Attacker.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                                    {
                                        if (players == null) continue;
                                        players.Out.SendCombatAnimation(null, targets, 0, 0, 0, 0, 0x0A, targets.HealthPercent);
                                    }
                                    livingTargets.TempProperties.removeProperty(hastakenDamage);
                                }
                            }
                        }
                    }
                    catch{}
                }

                //Players as target
                foreach (GameLiving targets in ad.Target.GetPlayersInRadius((ushort)spellRange))
                {
                    if (!GameServer.ServerRules.IsAllowedToAttack(targets, Caster, true))
                        continue;

                    if (targets.EffectList.GetOfType<NecromancerShadeEffect>() != null)
                        continue;

                    try
                    {

                        if (targets != Caster.TargetObject && targets != Caster)
                        {
                            livingTargets = targets;

                            if (livingTargets != null && Caster.IsObjectInFront(livingTargets, 120))

                            {
                                if (!m_targets.Contains(livingTargets) && m_targets.Count <= 3)//max 4 targets
                                {
                                    m_targets.Add(livingTargets);
                                    livingTargets.TempProperties.setProperty(hastakenDamage, livingTargets);
                                }
                            }


                            foreach (GameLiving livingTarget in m_targets)
                            {

                                if (livingTarget is GamePlayer && Caster.TargetObject == livingTarget == false && livingTargets.TempProperties.getProperty<GameLiving>(hastakenDamage) != null)
                                {
                                    GamePlayer player = livingTarget as GamePlayer;
                                    string damageAmount = (ad.StyleDamage > 0) ? " (+" + ad.StyleDamage.ToString() + ")" : "";
                                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "StyleProcessor.ExecuteStyle.PerformPerfectly", ad.Style.Name, damageAmount), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                                    //log.ErrorFormat("Damage1: {0} targets {1}", ad.Damage, targets.Name);

                                    targets.OnAttackedByEnemy(ad);
                                    targets.DealDamage(ad);
                                    targets.StartInterruptTimer(targets.SpellInterruptDuration, ad.AttackType, Caster);

                                    foreach (GamePlayer players in ad.Attacker.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                                    {
                                        if (players == null) continue;
                                        players.Out.SendCombatAnimation(null, targets, 0, 0, 0, 0, 0x0A, targets.HealthPercent);
                                    }
                                                                        
                                    livingTargets.TempProperties.removeProperty(hastakenDamage);
                                }

                                else if (livingTarget is GameNPC && Caster.TargetObject == livingTarget == false && livingTargets.TempProperties.getProperty<GameLiving>(hastakenDamage) != null)
                                {

                                    string damageAmount = (ad.StyleDamage > 0) ? " (+" + ad.StyleDamage.ToString() + ")" : "";

                                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "GamePlayer.Attack.InterceptHit", attackTypeMsg,
                                        targets.GetName(0, false, (Caster as GamePlayer).Client.Account.Language, (targets as GameNPC)), hitWeapon, ad.Damage.ToString(), modmessage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);


                                    targets.OnAttackedByEnemy(ad);
                                    targets.DealDamage(ad);
                                    targets.StartInterruptTimer(targets.SpellInterruptDuration, ad.AttackType, Caster);

                                    foreach (GamePlayer players in ad.Attacker.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                                    {
                                        if (players == null) continue;
                                        players.Out.SendCombatAnimation(null, targets, 0, 0, 0, 0, 0x0A, targets.HealthPercent);
                                    }
                                    //log.ErrorFormat("Damage3: {0} targets {1}", ad.Damage, targets.Name);
                                    livingTargets.TempProperties.removeProperty(hastakenDamage);
                                                                      
                                }

                            }
                        }

                    }
                    catch { }
                    
                }
                m_targets.Clear();
            }
        }
               
        public MultipleMeleeTargetsSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}   