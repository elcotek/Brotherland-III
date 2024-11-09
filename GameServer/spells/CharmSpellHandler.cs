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
using System;
using System.Collections.Generic;
using System.Text;




namespace DOL.GS.Spells
{
    /// <summary>
    /// Charms target NPC for the spell duration.
    /// 
    /// Spell.Value is used for hard NPC level cap
    /// Spell.Damage is used for percent of caster level cap
    /// </summary>
    [SpellHandlerAttribute("Charm")]
    public class CharmSpellHandler : SpellHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// temp properties
        /// </summary>
        private readonly PropertyCollection m_tempProps = new PropertyCollection();

        /// <summary>
        /// use it to store temporary properties on this living
        /// beware to use unique keys so they do not interfere
        /// </summary>
        public PropertyCollection TempProperties
        {
            get { return m_tempProps; }
        }

        /// <summary>
        /// Holds the charmed Npc for pulsing spells
        /// </summary>
        protected GameNPC m_charmedNpc;
        //private bool regionchange = false;
        /// <summary>
        /// The property that stores the new npc brain
        /// </summary>
        protected ControlledNpcBrain m_controlledBrain;

        private IControlledBrain controlledBrain = null;
        private GamePlayer player = null;
        /// <summary>
        /// Tells pulsing spells to not add brain if it was not removed by expire effect
        /// </summary>
        protected bool m_isBrainSet;

        /// <summary>
        /// What type of mobs this spell can charm. based on amnesia chance value.
        /// </summary>        
        public enum eCharmType : ushort
        {
            All = 0,
            Humanoid = 1,
            Animal = 2,
            Insect = 3,
            HumanoidAnimal = 4,
            HumanoidAnimalInsect = 5,
            HumanoidAnimalInsectMagical = 6,
            HumanoidAnimalInsectMagicalUndead = 7,
            Reptile = 8,
        }

        /// <summary>
        /// called after normal spell cast is completed and effect has to be started
        /// </summary>
        public override void FinishSpellCast(GameLiving target)
        {
            Caster.Mana -= PowerCost(target);

            base.FinishSpellCast(target);
        }

        /// <summary>
        /// called when spell effect has to be started and applied to targets
        /// </summary>
        public override bool StartSpell(GameLiving target)
        {
           
            if (m_charmedNpc == null)
            {
                // save target on first start
                m_charmedNpc = target as GameNPC;

            }
            else
            {
                // reuse for pulsing spells
                target = m_charmedNpc;

            }
            if (target == null)
                return false;

            // Überprüfung, ob 'target' null ist bereits oben durchgeführt, keine weitere nötig
            // Sicherstellen, dass wir auf per Hegel nicht auf null zugreifen
            if (Caster == null) return false; // Caster muss existieren
            if (Spell == null) return false; // Spell muss existieren


            //add missing sounds for pulsing spell (charm active)
            if (Spell.Pulse > 0)
            {
                foreach(GamePlayer pl in Caster.GetPlayersInRadius(400))
                {
                    (pl as GamePlayer).Out.SendSoundEffect(21, 0, 0, 0, 0, 0);
                }
            }

            if (Util.Chance(CalculateSpellResistChance(target)))
            {
                OnSpellResisted(target);
            }
            else
            {
                ApplyEffectOnTarget(target, 1);
            }

            return true;
        }

        /// <summary>
        /// Calculates chance of spell getting resisted
        /// </summary>
        /// <param name="target">the target of the spell</param>
        /// <returns>chance that spell will be resisted for specific target</returns>
        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        /// <summary>
        /// All checks before any casting begins
        /// </summary>
        /// <param name="selectedTarget"></param>
        /// <returns></returns>
        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            GamePlayer Owner = Caster as GamePlayer;

           

            if (selectedTarget == null)
            {
                return false;
            }
            if (Owner == null) return false; // Sicherstellen, dass Owner existiert

            if (Owner.InCombat)
            {
                Owner.Out.SendMessage("You are currently in Combat, the cast will fail!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (selectedTarget.InCombat)
            {
                Owner.Out.SendMessage("Your target are currently in Combat, the cast will fail!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

           

            if (Owner != null && Owner.ControlledBrain != null)
            {
                if (Spell.Pulse == 0)
                {
                    SendSpellResistAnimation(selectedTarget);
                    //MessageToCaster("You already have a charmed creature, release it first!", eChatType.CT_SpellResisted);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CharmSpellHandler.YouAlreadyHaveACharmedCreature"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
                return false;
            }
          

            if (selectedTarget is GameNPC == false)
            {
                Owner.Out.SendMessage(LanguageMgr.GetTranslation(Owner.Client.Account.Language, "DoesNotCharmTypeOfCreatur", Spell.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return false;
            }

           

            // check cast target
            if (selectedTarget == null || selectedTarget != null && !selectedTarget.IsAlive || selectedTarget != null && selectedTarget.ObjectState != GameObject.eObjectState.Active)
            {


                Owner.Out.SendMessage(LanguageMgr.GetTranslation(Owner.Client.Account.Language, "SpellHandler.SelectATargetForThisSpell", Spell.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                // ((MessageToCaster("You must select a target for this spell!", eChatType.CT_SpellResisted);
                return false;
            }



            // Sicherstellen, dass auf 'Brain' von NPC überprüft wird
            if (selectedTarget is GameNPC npc)
            {
                if (npc.Brain == null ||
                    npc.Brain is StandardMobBrain == false ||
                    npc.Brain is BossBrain ||
                    npc.LoadedFromScript ||
                    npc.BodyType == 36 ||
                    npc.BodyType == 88 ||
                    npc.PetCount > 0 ||
                    (npc.Flags & GameNPC.eFlags.SWIMMING) != 0 ||
                    (npc.Flags & GameNPC.eFlags.PEACE) != 0 ||
                    (npc.Flags & GameNPC.eFlags.STATUE) != 0 ||
                    npc.ControlledCount > 0)
                {
                  
                    if (Owner != null && Owner.ControlledBrain != null && Owner.ControlledBrain.Body != null)
                    {
                        GameNPC controlled = Owner.ControlledBrain.Body as GameNPC;

                        if (controlled != null)
                        {
                            PulsingSpellEffect ownerEffect = FindPulsingSpellOnTarget(controlled, this);

                            if (ownerEffect != null && Spell.Pulse > 0)
                            {
                                selectedTarget.EffectList.CancelAll();
                                ownerEffect.Cancel(false);

                            }
                        }
                    }
                    if (Caster is GamePlayer)
                    {
                        SendSpellResistAnimation(selectedTarget);
                        Owner.Out.SendMessage(LanguageMgr.GetTranslation(Owner.Client.Account.Language, "DoesNotCharmTypeOfCreatur", Spell.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        // MessageToCaster("This spell does not charm this type of creature!", eChatType.CT_SpellResisted);
                    }
                    return false;
                }
            }
            if (selectedTarget.ControlledBrain != null || selectedTarget is GameTrainingDummy || selectedTarget is GameNPC == false || selectedTarget is GameKeepGuard || selectedTarget is GameGuard || selectedTarget is GameDragon || selectedTarget is GameSiegeWeapon)
            {
                if (Owner.ControlledBrain != null && Owner.ControlledBrain.Body != null)
                {
                    GameNPC controlled = Owner.ControlledBrain.Body as GameNPC;

                    if (controlled != null)
                    {
                        PulsingSpellEffect ownerEffect = FindPulsingSpellOnTarget(controlled, this);

                        if (ownerEffect != null && Spell.Pulse > 0)
                        {
                            selectedTarget.EffectList.CancelAll();
                            ownerEffect.Cancel(false);

                        }
                    }
                }
                if (Caster is GamePlayer)
                {
                    SendSpellResistAnimation(selectedTarget);
                    Owner.Out.SendMessage(LanguageMgr.GetTranslation(Owner.Client.Account.Language, "DoesNotCharmTypeOfCreatur", Spell.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    // MessageToCaster("This spell does not charm this type of creature!", eChatType.CT_SpellResisted);
                }
                return false;
            }
            //You should be able to chain pulsing charm on the same mob
            if (Spell.Pulse != 0 && Owner != null && Owner.ControlledBrain != null && Owner.ControlledBrain.Body == (GameNPC)selectedTarget)
            {
                GameNPC controlled = Owner.ControlledBrain.Body as GameNPC;

                if (controlled != null)
                {
                    Owner.CommandNpcRelease();
                }
            }

            if (!base.CheckBeginCast(selectedTarget))
                return false;
                              

            return true;
        }

        /// <summary>
		/// Apply effect on target or do spell action if non duration spell
		/// </summary>
		/// <param name="target">target that gets the effect</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            if (target == null) return; // Sicherstellen, dass target nicht null ist

            // check only if brain wasn't changed at least once
            if (m_controlledBrain == null)
            {
                //if (target.Realm != 0 || target is GameNPC == false)
                if (target != null && target is GameNPC && ((target as GameNPC).Brain is StandardMobBrain == false
          || target is GameNPC == false || target.Realm != 0 || (target as GameNPC).Brain is BossBrain || (target as GameNPC).LoadedFromScript
          || (target as GameNPC).BodyType == 36 || (target as GameNPC).BodyType == 88 || (target as GameNPC).PetCount > 0 || ((target as GameNPC).Flags & GameNPC.eFlags.SWIMMING) != 0 || ((target as GameNPC).Flags & GameNPC.eFlags.PEACE) != 0
          || ((target as GameNPC).Flags & GameNPC.eFlags.STATUE) != 0) || (target as GameNPC).ControlledCount > 0)
                {
                    if (Caster is GamePlayer)
                    {
                        PulsingSpellEffect ownerEffect = FindPulsingSpellOnTarget(Caster, this);

                        if (ownerEffect != null && Spell.Pulse > 0)
                        {
                            target.EffectList.CancelAll();
                            ownerEffect.Cancel(false);

                        }
                        SendSpellResistAnimation(target);
                        //MessageToCaster("This spell does not charm this type of monster!", eChatType.CT_SpellResisted);
                        (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "DoesNotCharmTypeOfCreatur", Spell.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                    return;
                }

                if (m_spell.AmnesiaChance != 0)
                {
                    if (((GameNPC)target).BodyType != m_spell.AmnesiaChance && m_spell.LifeDrainReturn == 0)
                    {
                        PulsingSpellEffect ownerEffect = FindPulsingSpellOnTarget(Caster, this);

                        if (ownerEffect != null && Spell.Pulse > 0)
                        {
                            target.EffectList.CancelAll();
                            ownerEffect.Cancel(false);
                        }

                        if (Caster is GamePlayer)
                        {
                            SendSpellResistAnimation(target);
                            (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "DoesNotCharmTypeOfCreatur", Spell.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        }
                        return;
                    }
                }
                else
                {
                    //ToDo: Proper check for0 bodytypes but for now allowing ability to charm all bodytypes if amnesiachance is 0 so can have Bodytypes implemented without breaking charming - Sand
                    if (target.Name != target.Name.ToLower() && m_spell.LifeDrainReturn == 0 || target.Name != target.Name.ToLower() && ((GameNPC)target).BodyType != m_spell.LifeDrainReturn)
                    {
                        PulsingSpellEffect ownerEffect = FindPulsingSpellOnTarget(Caster, this);

                        if (ownerEffect != null && Spell.Pulse > 0)
                        {
                            target.EffectList.CancelAll();
                            ownerEffect.Cancel(false);
                        }

                        if (Caster is GamePlayer)
                        {
                            SendSpellResistAnimation(target);
                            (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "DoesNotCharmTypeOfCreatur", Spell.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        }
                        return;
                    }
                }

                if (Caster.ControlledBrain != null)
                {
                    if (Caster is GamePlayer)
                    {
                        //MessageToCaster("You already have a charmed creature, release it first!", eChatType.CT_SpellResisted);
                        (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CharmSpellHandler.YouAlreadyHaveACharmedCreature"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                    return;
                }

                IControlledBrain brain = ((GameNPC)target).Brain as IControlledBrain;
                if (brain != null && (brain.Owner as GamePlayer) != Caster)
                {
                    PulsingSpellEffect ownerEffect = FindPulsingSpellOnTarget(Caster, this);

                    if (ownerEffect != null && Spell.Pulse > 0)
                    {
                        target.EffectList.CancelAll();
                        ownerEffect.Cancel(false);
                    }

                    if (Caster is GamePlayer)
                    {
                        SendSpellResistAnimation(target);
                        (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CharmSpellHandler.YourTargetIsNotValid"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                    return;
                }
            }
            if (target != null && target.IsWithinRadius(Caster, 4000) == false)
            {
                if (Caster is GamePlayer)
                {

                    PulsingSpellEffect ownerEffect = FindPulsingSpellOnTarget(Caster, this);
                    if (ownerEffect != null)
                    {
                        target.EffectList.CancelAll();
                        target.Die(target);

                        ownerEffect.Cancel(false);
                        (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "GamePlayer.SendCastMessage.ThatTargetIsToFahr", target.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        return;

                    }
                }

            }
            // Spell.Value == Max Level this spell can charm, Spell.Damage == Max percent of the caster level this spell can charm
            if (target.Level > Spell.Value || target.Level > 60 || target.Level > Caster.Level * Spell.Damage / 100)
            {
                PulsingSpellEffect ownerEffect = FindPulsingSpellOnTarget(Caster, this);

                if (ownerEffect != null && Spell.Pulse > 0)
                {
                    target.EffectList.CancelAll();
                    ownerEffect.Cancel(false);
                }

                if (Caster is GamePlayer)
                {
                    SendSpellResistAnimation(target);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CharmSpellHandler.TargetIsToStrongToCharm", target.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
                return;
            }

            if (Caster is GamePlayer)
            {
                // base resists for all charm spells
                int resistChance = 100 - (99 + ((Caster.Level - target.Level) / 2));



                if (controlledBrain != null && controlledBrain is GameNPC && (controlledBrain as GameNPC).IsAlive && (Caster.IsMezzed || Caster.IsStunned))
                {

                    return;
                }


                /*
               * The Minstrel/Mentalist has an almost certain chance to charm/retain control of
               * a creature his level or lower, although there is a small random chance that it
               * could fail. The higher the level of the charmed creature compared to the
               * Minstrel/Mentalist, the greater the chance the monster has of breaking the charm.
               * Please note that your specialization level in the magic skill that contains the
               * charm spell will modify your base chance of charming and retaining control.
               * The higher your spec level, the greater your chance of controlling.
               */

                int diffLevel = (int)(Caster.Level / 1.5 + Caster.GetModifiedSpecLevel(m_spellLine.Spec) / 2.7) - target.Level;

                //log.ErrorFormat("diffLevel = {0}", diffLevel);
                if (diffLevel > 0)
                {

                    resistChance = 10 - diffLevel * 3;
                    //log.ErrorFormat("resistChance = {0}", resistChance);
                    resistChance = Math.Max(resistChance, 1);
                    //log.DebugFormat("div1 {0} resist {1}", diffLevel, resistChance);
                }
                else
                {

                    resistChance = 10 + diffLevel * diffLevel * 3;
                    resistChance = Math.Min(resistChance, 99);
                    //log.DebugFormat("div2 {0} resist {1}", diffLevel, resistChance);
                }


                if (Util.Chance(resistChance) && target.Level > Caster.Level || Caster.IsMezzed || Caster.IsStunned || Caster.IsSilenced)
                {
                    {

                        //Bei dritten Ressist verliert man das pet
                        for (int i = 0; i < 1; i++)
                        {
                            (Caster as GamePlayer).ResistCount++;

                        }


                        if ((Caster as GamePlayer).ResistCount > 2)
                        {
                            (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CharmSpellHandler.TargetResistsTheCharm", target.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            return;
                        }
                    }
                }

                if (target != null && target.IsAlive)
                {
                    SendEffectAnimations(target);//for pulsing spell only
                    base.ApplyEffectOnTarget(target, effectiveness);
                }
            }
        }


        /// <summary>
        /// When an applied effect starts
        /// duration spells only
        /// </summary>
        /// <param name="effect"></param>
        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);


            GamePlayer player = Caster as GamePlayer;
            GameNPC npc = effect.Owner as GameNPC;


            if (player != null && npc != null)
            {

                if (m_controlledBrain == null)
                    m_controlledBrain = new ControlledNpcBrain(player);

                if (!m_isBrainSet)
                {

                    npc.AddBrain(m_controlledBrain);
                    npc.MaxSpeedBase = (short)300;
                    npc.Brain.ThinkInterval = 1500;
                    m_isBrainSet = true;
                    (Caster as GamePlayer).ResistCount = 0;
                    GameEventMgr.AddHandler(player, GameLivingEvent.RegionChanging, new DOLEventHandler(ReleaseEventHandler));
                    GameEventMgr.AddHandler(npc, GameLivingEvent.PetReleased, new DOLEventHandler(ReleaseEventHandler));
                    GameEventMgr.AddHandler(npc, GameLivingEvent.Dying, new DOLEventHandler(ReleaseEventHandler));
                }

                if (player.ControlledBrain != m_controlledBrain)
                {

                    // sorc: "The slough serpent is enthralled!" ct_spell
                    Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message1, npc.GetName(0, false)), eChatType.CT_Spell);
                    //MessageToCaster(npc.GetName(0, true) + " is now under your control.", eChatType.CT_Spell);
                    (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client.Account.Language, "CharmSpellHandler.TargetIsNowUnderYourControl", npc.Name), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);


                    player.SetControlledBrain(m_controlledBrain);

                    foreach (GamePlayer ply in npc.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    {

                        ply.Out.SendNPCCreate(npc);
                        if (npc.Inventory != null)

                            ply.Out.SendLivingEquipmentUpdate(npc);

                        ply.Out.SendObjectGuildID(npc, player.Guild);

                    }
                }

                npc.BroadcastUpdate();


            }
            else
            {
                // something went wrong.
                if (log.IsWarnEnabled)
                    log.Warn(string.Format("charm effect start: Caster={0} effect.Owner={1}",
                                           (Caster == null ? "(null)" : Caster.GetType().ToString()),
                                           (effect.Owner == null ? "(null)" : effect.Owner.GetType().ToString())));
            }
        }

        /// <summary>
        /// Handles release commands
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        private void ReleaseEventHandler(DOLEvent e, object sender, EventArgs arguments)
        {
            GameNPC controlledBody = null;
            if (e == GameLivingEvent.RegionChanging)
            {
                if (sender != null && sender is GamePlayer)
                {
                    player = ((GamePlayer)sender) as GamePlayer;
                }
                if (player != null && ((GamePlayer)player).ControlledBrain != null)
                {
                    controlledBody = player.ControlledBrain.Body;
                    if (player != null && controlledBody != null)
                    {
                        PulsingSpellEffect ownerEffect1 = FindPulsingSpellOnTarget(player, this);
                        if (ownerEffect1 != null)
                        {
                            ownerEffect1.Cancel(false);
                            {
                                //log.Warn("npc delete");
                                controlledBody.Die(controlledBody);
                            }
                        }
                    }
                }
            }
            if (e == GameLivingEvent.PetReleased)
            {
                if (sender != null && sender is GameNPC && ((GameNPC)sender).Brain is IControlledBrain)
                {
                    controlledBrain = ((GameNPC)sender).Brain as IControlledBrain;
                }
                if (controlledBrain != null)
                {
                    PulsingSpellEffect ownerEffect = FindPulsingSpellOnTarget(controlledBrain.Owner, this);
                    if (ownerEffect != null)
                    {
                        ownerEffect.Cancel(false);
                        //log.Warn("disable player effect");
                    }
                }
                GameSpellEffect npcEffect = SpellHandler.FindEffectOnTarget(controlledBrain.Body, "Charm");
                if (npcEffect != null && controlledBrain.Body.Brain is StandardMobBrain)
                {
                    npcEffect.Cancel(false);
                    (controlledBrain.Owner as GamePlayer).CommandNpcRelease();
                    TempProperties.removeAllProperties();
                    //log.Warn("entferne alle effekte");
                    //log.Warn("disable npc effect");
                }

                controlledBrain.Body.EffectList.CancelAll();
                controlledBrain.Body.SendLivingStatsAndRegenUpdate();
                controlledBrain.Body.StartAttack(controlledBrain.Owner);
            }
            else if (e == GameLivingEvent.Dying)

                if (sender != null && sender is GameNPC && ((GameNPC)sender).Brain is IControlledBrain)
                {
                    controlledBrain = ((GameNPC)sender).Brain as IControlledBrain;
                }


            if (controlledBrain == null)
                return;


            PulsingSpellEffect concEffect = FindPulsingSpellOnTarget(controlledBrain.Owner, this);
            if (concEffect != null)
                concEffect.Cancel(false);

            GameSpellEffect charm = FindEffectOnTarget(controlledBrain.Body, this);

            if (charm == null)
            {
                // log.Warn("charm effect is already canceled");
                return;
            }
            charm.Cancel(false);
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

            GamePlayer player = Caster as GamePlayer;
            GameNPC npc = effect.Owner as GameNPC;

            if (player != null && npc != null)
            {
                if (!noMessages) // no overwrite
                {

                    GameEventMgr.RemoveHandler(npc, GameLivingEvent.PetReleased, new DOLEventHandler(ReleaseEventHandler));
                    GameEventMgr.RemoveHandler(player, GameLivingEvent.RegionChanging, new DOLEventHandler(ReleaseEventHandler));
                    GameEventMgr.RemoveHandler(npc, GameLivingEvent.Dying, new DOLEventHandler(ReleaseEventHandler));
                    player.SetControlledBrain(null);

                    //add missing sounds for pulsing spell (release)
                    if (Spell.Pulse > 0)
                    {
                        foreach (GamePlayer pl in Caster.GetPlayersInRadius(400))
                        {
                            (pl as GamePlayer).Out.SendSoundEffect(23, 0, 0, 0, 0, 0);
                        }
                    }

                    //MessageToCaster("You lose control of " + npc.GetName(0, false) + "!", eChatType.CT_SpellExpires);
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.SetControlledNpc.ReleaseTarget2", npc.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    lock (npc.BrainSync)
                    {

                        npc.StopAttack();
                        npc.RemoveBrain(m_controlledBrain);
                        m_isBrainSet = false;
                        npc.MaxSpeedBase = (short)225;

                        if (Spell.Pulse != 0 && Caster.ObjectState == GameObject.eObjectState.Active && Caster.IsAlive)
                        {
                            ((IOldAggressiveBrain)npc.Brain).AddToAggroList(Caster, Caster.Level * 10);
                            npc.StartAttack(Caster);
                        }
                        else
                        {
                            npc.WalkToSpawn();
                        }



                        // remove NPC with new brain from all attackers aggro list
                        lock (npc.Attackers)

                            foreach (GameObject obj in npc.Attackers)
                            {

                                if (obj == null || !(obj is GameNPC))
                                    continue;

                                if (((GameNPC)obj).Brain != null && ((GameNPC)obj).Brain is IOldAggressiveBrain)
                                    ((IOldAggressiveBrain)((GameNPC)obj).Brain).RemoveFromAggroList(npc);
                            }

                        m_controlledBrain.ClearAggroList();
                        npc.StopFollowing();

                        npc.TempProperties.setProperty(GameNPC.CHARMED_TICK_PROP, npc.CurrentRegion.Time);


                        foreach (GamePlayer ply in npc.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                        {
                            if (npc.IsAlive)
                            {

                                ply.Out.SendNPCCreate(npc);

                                if (npc.Inventory != null)
                                    ply.Out.SendLivingEquipmentUpdate(npc);

                                ply.Out.SendObjectGuildID(npc, null);

                            }

                        }
                    }

                }
            }
            else
            {
                if (log.IsWarnEnabled)
                    log.Warn(string.Format("charm effect expired: Caster={0} effect.Owner={1}",
                                         (Caster == null ? "(null)" : Caster.GetType().ToString()),
                                           (effect.Owner == null ? "(null)" : effect.Owner.GetType().ToString())));
            }
            return 0;

        }



        /// <summary>
        /// Determines wether this spell is better than given one
        /// </summary>
        /// <param name="oldeffect"></param>
        /// <param name="neweffect"></param>
        /// <returns>true if this spell is better version than compare spell</returns>
        public override bool IsNewEffectBetter(GameSpellEffect oldeffect, GameSpellEffect neweffect)
        {

            if (oldeffect.Spell.SpellType != neweffect.Spell.SpellType)
            {
                if (log.IsWarnEnabled)
                    log.Warn("Spell effect compare with different types " + oldeffect.Spell.SpellType.ToString() + " <=> " + neweffect.Spell.SpellType.ToString() + "\n" + Environment.StackTrace);

                return false;
            }

            return neweffect.SpellHandler == this;
        }

        //Right Animation for Pet and Owner
        protected virtual void SendEffectAnimations(GameLiving target)
        {
            foreach (GamePlayer player in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                if (Caster is GamePlayer && Spell.Pulse > 0 && (Caster as GamePlayer).IsCharcterClass(eCharacterClass.Minstrel))
                {
                    player.Out.SendSpellEffectAnimation(m_caster, target, Spell.ClientEffect, 0, false, 1);
                }
                else if (Caster is GamePlayer && Spell.Pulse > 0)
                {
                    if (target != Caster)
                    player.Out.SendSpellEffectAnimation(Caster, target, Spell.ClientEffect, 0, false, 1);

                }
            }
        }
        /// <summary>
        /// Send the Effect Animation
        /// </summary>
        /// <param name="target">The target object</param>
        /// <param name="boltDuration">The duration of a bolt</param>
        /// <param name="noSound">sound?</param>
        /// <param name="success">spell success?</param>
        public override void SendEffectAnimation(GameObject target, ushort boltDuration, bool noSound, byte success)
        {
            if (Caster is GamePlayer)
            {
                if (Spell.Pulse == 0)
                    base.SendEffectAnimation(target, boltDuration, noSound, success);
            }
        }

        /// <summary>
        /// Send Spell Resisted Animation
        /// </summary>
        /// <param name="target"></param>
        public override void SendSpellResistAnimation(GameLiving target)
        {
           
                SendEffectAnimation(target, 0, false, 0);
        }

        /// <summary>
        /// Delve Info
        /// </summary>
        public override IList<string> DelveInfo(GamePlayer player)
        {

            {
                var list = new List<string>();

                list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "CharmSpellHandler.DelveInfo.Function", (Spell.SpellType == "" ? "(not implemented)" : Spell.SpellType)));
                list.Add(" "); //empty line
                list.Add(Spell.Description);
                list.Add(" "); //empty line
                if (Spell.InstrumentRequirement != 0)
                    list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.InstrumentRequire", GlobalConstants.InstrumentTypeToName(Spell.InstrumentRequirement)));
                list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Target", Spell.Target));
                if (Spell.Range != 0)
                    list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Range", Spell.Range.ToString()));
                if (Spell.Duration >= ushort.MaxValue * 1000)
                    list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + " Permanent.");
                else if (Spell.Duration > 60000)
                    list.Add(string.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + Spell.Duration / 60000 + ":" + (Spell.Duration % 60000 / 1000).ToString("00") + " min"));
                else if (Spell.Duration != 0)
                    list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + (Spell.Duration / 1000).ToString("0' sec';'Permanent.';'Permanent.'"));
                if (Spell.Frequency != 0)
                    list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Frequency", (Spell.Frequency * 0.001).ToString("0.0")));
                if (Spell.Power != 0)
                    list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.PowerCost", Spell.Power.ToString("0;0'%'")));
                list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.CastingTime", (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'")));
                if (Spell.RecastDelay > 60000)
                    list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.RecastTime") + Spell.RecastDelay / 60000 + ":" + (Spell.RecastDelay % 60000 / 1000).ToString("00") + " min");
                else if (Spell.RecastDelay > 0)
                    list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.RecastTime") + (Spell.RecastDelay / 1000).ToString() + " sec");
                if (Spell.Concentration != 0)
                    list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.ConcentrationCost", Spell.Concentration.ToString()));
                if (Spell.Radius != 0)
                    list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Radius", Spell.Radius.ToString()));
                if (Spell.DamageType != eDamageType.Natural)
                    list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Damage", GlobalConstants.DamageTypeToName(Spell.DamageType).ToString()));

                return list;
            }
        }
              
        // Constructs new Charm spell handler
        public CharmSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
        }

        /*

        http://www.camelotherald.com/more/1775.shtml

        ... Can you please explain what the max level pet a hunter can charm if they are fully Beastcraft specd? The community feels its no higher then 41, but the builder says max level 50.

        A: Sayeth the Oracle: ”It's 82% of the caster's level for the highest charm in beastcraft; or level 41 if the caster is 50. Spec doesn't determine the level of the pet - it's purely based on the spell.”



        http://vnboards.ign.com/message.asp?topic=87170081&start=87173224&search=charm

        More info in the sticky thread, but...


        <copies and pastes her charm spell info>

        What you can charm:
        4 - humanoids
        10 - humanoids, animals
        17 - humanoids, animals, insects
        25 - humanoids, animals, insects, magical
        33 - humanoids, animals, insects, magical, undead
        42 - anything charmable

        Always use lowest charm to save power.

        Safety level formula:
        (level * .66) + (spec level * .33)
        spec level includes: trainings, items, and realm rank

        Mastery of Focus:
        Mastery of Focus affects SPELL level. Notice that SPELL level is not included in the above formula. SPEC level is important. If you raise the lvl 4 charm up to lvl 20 it makes NO difference to what you can charm.

        Current charm bugs:
        - Porting has the chance to completely break your charm if there is a delay in porting. Pet will show up at portal location very very mad.
        - Porting also causes your pet to completely disappear. Walk away and it should reappear. Maybe

        NOT A BUG, working as intended
        - Artifact chants (Cloudsong, Crown, etc.) will interfere and overwrite your charm.





        sorc

        <Begin Info: Coerce Will>
        Function: charm
 
        Attempts to bring the target under the caster's control.
 
        Target: Targetted
        Range: 1000
        Duration: Permanent.
        Power cost: 25%
        Casting time:      4.0 sec
        Damage: Energy
 
        <End Info>

        [06:23:57] You begin casting a Coerce Will spell!
        [06:24:01] The slough serpent attacks you and misses!
        [06:24:01] You cast a Coerce Will Spell!
        [06:24:01] The slough serpent is enthralled!
        [06:24:01] The slough serpent is now under your control.

        [14:30:55] The frost stallion dies!
        [14:30:55] This monster has been charmed recently and is worth no experience.




        pulsing, mentalist

        <Begin Info: Imaginary Enemy>
        Function: charm
 
        Attempts to bring the target under the caster's control.
 
        Target: Targetted
        Range: 2000
        Duration: 10 sec
        Frequency:      4.8 sec
        Casting time:      3.0 sec
        Damage: Heat
 
        <End Info>

        [16:11:59] You begin casting a Imaginary Enemy spell!
        [16:11:59] You are already casting a spell!  You prepare this spell as a follow up!
        [16:12:01] You are already casting a spell!  You prepare this spell as a follow up!
        [16:12:02] You cast a Imaginary Enemy Spell!
        [16:12:02] The villainous youth is now under your control.
        [16:12:02] You cancel your effect.

        [16:11:42] You can't attack yourself!
        [16:11:42] You lose control of the villainous youth!
        [16:11:42] You lose control of the villainous youth.




        minstrel

        [09:00:12] <Begin Info: Attracting Melodies>
        [09:00:12] Function: charm
        [09:00:12]
        [09:00:12] Attempts to bring the target under the caster's control.
        [09:00:12]
        [09:00:12] Target: Targetted
        [09:00:12] Range: 2000
        [09:00:12] Duration: 10 sec
        [09:00:12] Frequency:      5.0 sec
        [09:00:12] Casting time: instant
        [09:00:12] Recast time: 5 sec
        [09:00:12]
        [09:00:12] <End Info>

        [09:05:56] You command the the worker ant to kill your target!
        [09:05:59] The worker ant attacks the worker ant and hits!
        [09:06:00] The worker ant attacks the worker ant and hits!
        [09:06:01] You lose control of the worker ant!
        [09:06:01] You release control of your controlled target.

        [09:06:50] The worker ant is now under your control.
        [09:06:51] The worker ant attacks you and misses!
        [09:06:55] The worker ant attacks the worker ant and hits!
        [09:06:55] The worker ant resists the charm!

         */
    }
}
