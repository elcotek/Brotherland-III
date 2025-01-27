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
using DOL.GS.Spells;
using DOL.Language;
using System;
using System.Collections;


namespace DOL.GS.SkillHandler
{
    /// <summary>
    /// Handler for Stealth Spec clicks
    /// </summary>
    [SkillHandlerAttribute(Specs.Stealth)]
    public class StealthSpecHandler : ISpecActionHandler
    {
      
        /// <summary>
        /// Executes the stealth ability
        /// </summary>
        /// <param name="spec"></param>
        /// <param name="player"></param>
        public void Execute(Specialization spec, GamePlayer player)
        {
            
            if (player.InCombat && !player.IsStealthed && player.Client.Account.PrivLevel == (int)ePrivLevel.Player)
            {
                //player.Out.SendMessage("You've been in combat recently and can't hide yet!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client.Account.Language, "SpellHandler.YouAreInCombatDandCantHiding"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
                        
            long stealthChangeTick = player.TempProperties.getProperty<long>(GamePlayer.STEALTH_CHANGE_TICK);
            long changeTime = player.CurrentRegion.Time - stealthChangeTick;
            if (changeTime < 2000)
            {
                //player.Out.SendMessage("You must wait " + ((2000 - changeTime) / 1000).ToString() + " more second to attempt to hide!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client.Account.Language, "SpellHandler.YouMustWaitToHide", ((2000 - changeTime) / 1000)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            player.TempProperties.setProperty(GamePlayer.STEALTH_CHANGE_TICK, player.CurrentRegion.Time);

            if (!player.IsStealthed)
            {
                // Dead can't stealth
                if (!player.IsAlive)
                {
                    //player.Out.SendMessage("You can't hide when dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client.Account.Language, "SpellHandler.YouCantHideWhenDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }

                // Can't stealth if in attack mode
                if (player.AttackState || (player.CurrentSpellHandler != null && player.CurrentSpellHandler.IsCasting))
                {
                    //player.Out.SendMessage("You must end your current action before hiding!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client.Account.Language, "SpellHandler.YouMustEndYourCurrentActionBeforeHiding"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }

                //TODO: more checks in this order

                //"You can't hide with a relic!"


                /// <summary>
                /// Chancel Ablive Armor Effect to hide
                /// </summary>
                /// <param name="spec"></param>
                /// <param name="player"></param>
                GameSpellEffect Ablative = SpellHandler.FindEffectOnTarget(player, "AblativeArmor");
                // GameSpellEffect Ablative = SpellHandler.FindEffectOnTarget(((GameLiving)caster.TargetObject), "AblativeArmor");
                if (player.IsStealthed && Ablative != null)
                {
                    //player.Out.SendMessage("You cancel your effect.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client.Account.Language, "SpellHandler.YouCancelYourEffect"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    CancelPulsingSpell(player, "AblativeArmor");
                }
                
                if (player.ConcentrationEffects.GetOfType(typeof(PulsingSpellEffect)) != null)
                {
                    //player.Out.SendMessage("You currently have an active, pulsing spell effect and cannot hide!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client.Account.Language, "SpellHandler.YouCurrentlyHaveActivePulsingSpell"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }
                
                //HasVanishRealmAbilityActivated -> Allow stealthing, Stop further checks ...

                if (player.IsMezzed)
                {
                    //player.Out.SendMessage("You can't hide while mesmerized!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client.Account.Language, "SpellHandler.YouCantHideWhileMesmerized"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }

                if (player.IsStunned)
                {
                    //player.Out.SendMessage("You can't hide while stunned!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client.Account.Language, "SpellHandler.YouCantHideWhileStunned"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }

                // Check if enemy player is close
                foreach (GamePlayer ply in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    if (ply.ObjectState != GameObject.eObjectState.Active) continue;

                    //Dead players don't prevent stealth!
                    if (!ply.IsAlive)
                    {
                        continue;
                    }

                    //TODO: "True Seeing" realm ability check
                    //True Seeing denies restealthing for VISIBILITY_DISTANCE!
                    //There was/is a huge discussion about this on live servers since
                    //all Assessins found this quite unfair!
                    //But the discussion has died down since True Seeing etc.

                    //Friendly players/mobs don't prevent stealth!
                    if (!GameServer.ServerRules.IsAllowedToAttack(ply, player, true))
                    {
                        continue;
                    }

                    //GM's don't prevent stealth
                    if (ply.Client.Account.PrivLevel > 1)
                    {
                        continue;
                    }

                    //Range check
                    if (!IsObjectTooClose(ply, player))
                    {
                        continue;
                    }

                    //player.Out.SendMessage("You are too close to an enemy to hide!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client.Account.Language, "SpellHandler.YouAreTooCloseToAnEnemyToHide"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }

                // Check if enemy NPC is close
                foreach (GameNPC npc in player.GetNPCsInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    if (npc.ObjectState != GameObject.eObjectState.Active)
                    {
                        continue;
                    }

                    //Dead npc don't prevent stealth!
                    if (!npc.IsAlive)
                    {
                        continue;
                    }

                    //Friendly players/mobs don't prevent stealth!
                    if (!GameServer.ServerRules.IsAllowedToAttack(npc, player, true))
                    {
                        continue;
                    }

                    //Range check
                    if (!IsObjectTooClose(npc, player))
                    {
                        continue;
                    }

                    //player.Out.SendMessage("You are too close to an enemy to hide!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client.Account.Language, "SpellHandler.YouAreTooCloseToAnEnemyToHide"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }
            }
            //since 1.88 (?), players which stealth, doesn't be followed by mobs [by Suncheck]
            //TODO: Some further checks need?
            ArrayList attackers = new ArrayList();
            attackers.AddRange(player.Attackers);
            foreach (GameLiving attacker in attackers)
            {
                if (attacker.TargetObject == (GameLiving)player)
                {
                    attacker.TargetObject = null;
                    if (attacker is GamePlayer)
                    {
                        ((GamePlayer)attacker).Out.SendChangeTarget(attacker.TargetObject);
                    }
                    if (attacker is GameNPC)
                    {
                        GameNPC npc = (GameNPC)attacker;
                        if (npc.Brain is IOldAggressiveBrain)
                        {
                            ((IOldAggressiveBrain)npc.Brain).RemoveFromAggroList(player);
                        }
                        attacker.StopAttack();
                    }
                }
            }
            player.Stealth(!player.IsStealthed);
        }

        /// <summary>
        /// Checks whether object is too close to player
        /// </summary>
        /// <param name="obj">object</param>
        /// <param name="player">player</param>
        /// <returns>true if object prevents player from hiding</returns>
        private bool IsObjectTooClose(GameObject obj, GamePlayer player)
        {
            float enemyLevel = Math.Max(1f, obj.Level);
            float stealthLevel = player.GetModifiedSpecLevel(Specs.Stealth);
            if (stealthLevel > 50)
            {
                stealthLevel = 50;
            }

            float radius;

            if (obj is GamePlayer && ((GamePlayer)obj).HasAbility(Abilities.DetectHidden))
            {
                //1792.0 = 2048.0 - 256.0 <- Detect Hidden doubles the range
                radius = 2048f - (1792f * stealthLevel / enemyLevel);
            }
            else
            {
                //1024.0 = 1024.0 - 128.0 <- normal Range
                radius = 1024f - (896f * stealthLevel / enemyLevel);
            }

            //If we are so skilled we can hide right under the nose of
            //this enemy player, we continue the checks for the next 
            //player without any redundant distance calculations
            if (radius <= 0)
            {
                return false;
            }

            //Test if the stealthing player is in range of this 
            //player's "impossible to hide" circle
            return obj.IsWithinRadius(player, (int)radius);
        }

        /// <summary>
        /// Cancels first pulsing spell of type
        /// </summary>
        /// <param name="living">owner of pulsing spell</param>
        /// <param name="spellType">type of spell to cancel</param>
        /// <returns>true if any spells were canceled</returns>
        public virtual bool CancelPulsingSpell(GameLiving living, string spellType)
        {
            lock (living.ConcentrationEffects)
            {
                for (int i = 0; i < living.ConcentrationEffects.Count; i++)
                {
                    PulsingSpellEffect effect = living.ConcentrationEffects[i] as PulsingSpellEffect;
                    if (effect == null)
                        continue;
                    if (effect.SpellHandler.Spell.SpellType == spellType)
                    {
                        effect.Cancel(false);
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// The spell that we want to handle
        /// </summary>
        protected Spell m_spell;
        /// <summary>
        /// The Spell
        /// </summary>
        public Spell Spell
        {
            get { return m_spell; }
        }
    }
}