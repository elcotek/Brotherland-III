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
using DOL.GS.RealmAbilities;
using DOL.GS.ServerProperties;
using DOL.GS.SkillHandler;
using DOL.Language;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static DOL.GS.GameLiving;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Default class for spell handler
    /// should be used as a base class for spell handler
    /// </summary>
    public class SpellHandler : ISpellHandler
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        const string SpamTime = "NOSPELLEFFECTSPAM";
        private const string AnimStatus = "ANIM STATUS";




        /// <summary>
        /// Maximum number of sub-spells to get delve info for.
        /// </summary>
        protected static readonly byte MAX_DELVE_RECURSION = 5;

        protected DelayedCastTimer m_castTimer;
        /// <summary>
        /// The spell that we want to handle
        /// </summary>
        protected Spell m_spell;
        /// <summary>
        /// The spell line the spell belongs to
        /// </summary>
        protected SpellLine m_spellLine;
        /// <summary>
        /// The caster of the spell
        /// </summary>
        protected GameLiving m_caster;
        /// <summary>
        /// The target for this spell
        /// </summary>
        protected GameLiving m_spellTarget = null;
        /// <summary>
        /// Has the spell been interrupted
        /// </summary>
        protected bool m_interrupted = false;

        protected bool m_paused = false;
        /// <summary>
        /// Delayedcast Stage
        /// </summary>
        public int Stage
        {
            get { return m_stage; }
            set { m_stage = value; }
        }
        protected int m_stage = 0;


        /// <summary>
        /// Use to store Time when the delayedcast started
        /// </summary>
        protected long m_started = 0;
        /// <summary>
        /// Shall we start the reuse timer
        /// </summary>
        protected bool m_startReuseTimer = true;

        public bool StartReuseTimer
        {
            get { return m_startReuseTimer; }
        }

        protected bool m_wasResist;

        /// <summary>
        /// Was this a ressist ?
        /// </summary>
        public virtual bool WasResist
        {
            get { return m_wasResist; }
            set { m_wasResist = value; }
        }


        /// <summary>
        /// Can this spell be queued with other spells?
        /// </summary>
        public virtual bool CanQueue
        {
            get { return true; }
        }

        /// <summary>
        /// Does this spell break stealth on start of cast?
        /// </summary>
        public virtual bool UnstealthCasterOnStart
        {
            get { return true; }
        }

        /// <summary>
        /// Does this spell break stealth on Finish of cast?
        /// </summary>
        public virtual bool UnstealthCasterOnFinish
        {
            get { return true; }
        }

        protected InventoryItem m_spellItem = null;

        /// <summary>
        /// Ability that casts a spell
        /// </summary>
        protected SpellCastingAbilityHandler m_ability = null;

        /// <summary>
        /// Stores the current delve info depth
        /// </summary>
        private byte m_delveInfoDepth;

        /// <summary>
        /// AttackData result for this spell, if any
        /// </summary>
        protected AttackData m_lastAttackData = null;
        /// <summary>
        /// AttackData result for this spell, if any
        /// </summary>
        public AttackData LastAttackData
        {
            get { return m_lastAttackData; }
        }

        /// <summary>
        /// The property key for the interrupt timeout
        /// </summary>
        public const string INTERRUPT_TIMEOUT_PROPERTY = "CAST_INTERRUPT_TIMEOUT";
        /// <summary>
        /// The property key for focus spells
        /// </summary>
        protected const string FOCUS_SPELL = "FOCUSING_A_SPELL";

        protected bool m_ignoreDamageCap = false;

        /// <summary>
        /// Does this spell ignore any damage cap?
        /// </summary>
        public bool IgnoreDamageCap
        {
            get { return m_ignoreDamageCap; }
            set { m_ignoreDamageCap = value; }
        }

        protected bool m_useMinVariance = false;


        /// <summary>
        /// Should this spell use the minimum variance for the type?
        /// Followup style effects, for example, always use the minimum variance
        /// </summary>
        public bool UseMinVariance
        {
            get { return m_useMinVariance; }
            set { m_useMinVariance = value; }
        }

        /// <summary>
        /// Can this SpellHandler Coexist with other Overwritable Spell Effect
        /// </summary>
        public virtual bool AllowCoexisting
        {
            get { return Spell.AllowCoexisting; }
        }

        /// <summary>
        ///is Attacker Grey con to Caster ?
        /// </summary>
        private bool m_isObjectGreyCon;
        public virtual bool AttackerIsGreyConToCaster
        {
            get { return m_isObjectGreyCon; }
            set { m_isObjectGreyCon = value; }
        }

        /// <summary>
        ///gets the GreyCon of the caster attacker
        /// </summary>
        /// <param name="defender"></param>
        /// <param name="attacker"></param>
        private void AttackerIsGreyCon(GameLiving defender, GameLiving attacker)
        {
            if (Caster.IsObjectGreyCon(attacker) && attacker.InCombat && attacker.TargetObject == Caster && attacker.TargetObject != null)
                AttackerIsGreyConToCaster = true;
            else
                AttackerIsGreyConToCaster = false;
        }


        /// <summary>
        /// The CastingCompleteEvent
        /// </summary>
        public event CastingCompleteCallback CastingCompleteEvent;
        //public event SpellEndsCallback SpellEndsEvent;

        /// <summary>
        /// spell handler constructor
        /// <param name="caster">living that is casting that spell</param>
        /// <param name="spell">the spell to cast</param>
        /// <param name="spellLine">the spell line that spell belongs to</param>
        /// </summary>
        public SpellHandler(GameLiving caster, Spell spell, SpellLine spellLine)
        {
            m_caster = caster;
            m_spell = spell;
            m_spellLine = spellLine;
        }

        /// <summary>
        /// Returns the string representation of the SpellHandler
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return new StringBuilder(128)
                .Append("Caster=").Append(Caster == null ? "(null)" : Caster.Name)
                .Append(", IsCasting=").Append(IsCasting)
                .Append(", m_interrupted=").Append(m_interrupted)
                .Append("\nSpell: ").Append(Spell == null ? "(null)" : Spell.ToString())
                .Append("\nSpellLine: ").Append(SpellLine == null ? "(null)" : SpellLine.ToString())
                .ToString();
        }

        #region Pulsing Spells



        /// <summary>
        /// When spell pulses
        /// </summary>
        public virtual void OnSpellPulse(PulsingSpellEffect effect)
        {
            
            if (Caster.IsMoving && Spell.IsFocus || Caster.IsSitting && Spell.IsFocus)
            {
                if (Caster is GamePlayer)
                {
                    //MessageToCaster("Your spell was cancelled.", eChatType.CT_SpellExpires);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourSpellWasCancelled"), eChatType.CT_SpellExpires, eChatLoc.CL_SystemWindow);
                }
                effect.Cancel(false);
                return;
            }
            if (Spell.IsCastFocus || Caster.IsSitting && Spell.IsCastFocus)
            {
                if (Caster is GamePlayer)
                {
                    //MessageToCaster("Your spell was cancelled.", eChatType.CT_SpellExpires);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourSpellWasCancelled"), eChatType.CT_SpellExpires, eChatLoc.CL_SystemWindow);
                }
                effect.Cancel(false);
                return;
            }
            if ((Caster.IsSitting || Caster.IsStunned || Caster.IsMezzed || Caster.IsMoving) && Spell.IsMoveFocus)
            {
                if (Caster is GamePlayer)
                {
                    //MessageToCaster("Your spell was cancelled.", eChatType.CT_SpellExpires);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourSpellWasCancelled"), eChatType.CT_SpellExpires, eChatLoc.CL_SystemWindow);
                }
                effect.Cancel(false);
                return;
            }



            if (Caster.IsAlive == false)
            {
                effect.Cancel(false);
                return;
            }
            if (Caster.ObjectState != GameObject.eObjectState.Active)
                return;
            if (Caster.IsStunned || Caster.IsMezzed)
                return;


            // no instrument anymore = stop the song
            if (m_spell.InstrumentRequirement != 0 && !CheckInstrument())
            {
                if (Caster is GamePlayer)
                {
                    //MessageToCaster("You stop playing your song.", eChatType.CT_Spell);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CCSpellHandler.StopPlayingSong"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }
                effect.Cancel(false);
                return;
            }

            //ressist for lute pulse
            if (m_spell.InstrumentRequirement != 0 && WasResist == true)
            {
                if (Caster is GamePlayer)
                {
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.TargetRessistTheEffect", effect.OwnerName), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);

                }
                effect.Cancel(false);
                WasResist = false;
                return;
            }
            //Range for lute pulse
            if (!HasPositiveEffect && m_spell.Pulse != 0 && m_spell.InstrumentRequirement != 0 && m_spellTarget != null && !Caster.IsWithinRadius(m_spellTarget, CalculateSpellRange()))
            {
                if (Caster is GamePlayer)
                {
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourTargetIsNoLongerInRange", m_spellTarget.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);

                }
                Caster.TempProperties.setProperty(AnimStatus, true);

                // effect.Cancel(false);
                return;
            }
            //log.Error("Remove propertie1");
            Caster.TempProperties.removeProperty(AnimStatus);
            //Heretic Spells
            if (((Spell.SpellType == "HereticDamageSpeedDecreaseLOP" || Spell.SpellType == "HereticPiercingMagic" || Spell.SpellType == "HereticDoTLostOnPulse") && (Caster.TargetObject != null && Spell.Target == "Enemy" && m_spellTarget.IsObjectAlive == false
                || Spell.Target == "Enemy" && m_spellTarget == null)))
            {

                effect.Cancel(false);
                return;
            }
            /*
            if (m_spell.InstrumentRequirement != 0 && !CheckInstrument())
            {
                if (Caster is GamePlayer)
                {
                    //MessageToCaster("You stop playing your song.", eChatType.CT_Spell);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CCSpellHandler.StopPlayingSong"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }
                effect.Cancel(false);
                return;
            }
            */



            //do not again start mez effect if imunity already active
            if (!HasPositiveEffect && Spell.InstrumentRequirement != 0 && SpellHandler.FindEffectAndImmunityOnTarget(m_spellTarget, Spell.SpellType) != null || Spell.InstrumentRequirement != 0 && !CheckInstrument())
            {

                SendEffectAnimation(m_spellTarget, 0, false, 0); // pulsing auras or songs resist animation
                effect.Cancel(false);
                return;
            }

            // Minstrel  Mesmerize need no target at start of the Spell
            if (m_spellTarget != null && Caster is GamePlayer && Caster.IsCastingMesmerizeSpellWithPulse(Spell, (Caster as GamePlayer)) && !m_caster.IsObjectInFront(m_spellTarget, 175))
            {

                Caster.TempProperties.setProperty(AnimStatus, true);
            }


            //log.Error("Remove propertie2");
           

            if (Caster.Mana >= Spell.PulsePower)
            {

                Caster.Mana -= Spell.PulsePower;

                

                if (FindImmunityOfSpellTypeOnTarget(m_spellTarget, Spell.SpellType) == null && m_spellTarget != null && Caster.TempProperties.getProperty<bool>(AnimStatus) == false && Spell.Pulse != 0 && Spell.InstrumentRequirement != 0 && !HasPositiveEffect)
                {
                    
                    SendEffectAnimation(Caster, 0, true, 1); // pulsing auras or songs
                }
                else if (FindImmunityOfSpellTypeOnTarget(m_spellTarget, Spell.SpellType) == null && (Spell.InstrumentRequirement != 0 || !HasPositiveEffect))
                {
                    
                    SendEffectAnimation(Caster, 0, true, 1);
                }


                StartSpell(m_spellTarget);
                // Caster.TempProperties.removeProperty(AnimStatus);
            }
            else
            {
                if (Spell.IsFocus)
                {
                    FocusSpellAction(null, Caster, null);
                }

                if (Caster is GamePlayer)
                {
                    //MessageToCaster("You do not have enough mana and your spell was cancelled.", eChatType.CT_SpellExpires);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.NotEnoughManaYourSpellWasCancelled"), eChatType.CT_SpellExpires, eChatLoc.CL_SystemWindow);
                }
                effect.Cancel(false);
            }
        }

        /// <summary>
        /// Checks if caster holds the right instrument for this spell
        /// </summary>
        /// <returns>true if right instrument</returns>
        protected bool CheckInstrument()
        {
            InventoryItem instrument = Caster.AttackWeapon;
            // From patch 1.97:  Flutes, Lutes, and Drums will now be able to play any song type, and will no longer be limited to specific songs.
            if (instrument == null || instrument.Object_Type != (int)eObjectType.Instrument || (instrument.DPS_AF != 4 && instrument.DPS_AF != m_spell.InstrumentRequirement))
            {
                return false;
            }
            return true;
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
                        if (living is GamePlayer)
                        {
                            GamePlayer player = Caster as GamePlayer;

                            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "SpellHandler.ThePulsingSpell.Ends", effect.SpellHandler.Spell.Name), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                           

                            if (player.IsCharcterClass(eCharacterClass.Heretic))
                            {
                                Caster.TempProperties.setProperty(HereticDoTLostOnPulse.DAMAGE_UPDATE_TICK, 0);
                                Caster.TempProperties.setProperty(HereticDamageSpeedDecreaseLostOnPulse.DAMAGE_UPDATE_TICK, 0);
                            }
                        }
                        
                        effect.Cancel(false);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Cancels all pulsing spells
        /// </summary>
        /// <param name="living"></param>
        public static void CancelAllPulsingSpells(GameLiving living)
        {
            List<IConcentrationEffect> pulsingSpells = new List<IConcentrationEffect>();

            GamePlayer player = living as GamePlayer;

            lock (living.ConcentrationEffects)
            {
                for (int i = 0; i < living.ConcentrationEffects.Count; i++)
                {
                    PulsingSpellEffect effect = living.ConcentrationEffects[i] as PulsingSpellEffect;
                    if (effect == null)
                        continue;
                   if (effect.SpellHandler.Spell.SpellType == "Charm")
                       continue;

                    if (player != null && player.CharacterClass.MaxPulsingSpells > 1)
                        pulsingSpells.Add(effect);
                    else
                        effect.Cancel(false);
                }
            }

            // Non-concentration spells are grouped at the end of GameLiving.ConcentrationEffects.
            // The first one is added at the very end; successive additions are inserted just before the last element
            // which results in the following ordering:
            // Assume pulsing spells A, B, C, and D were added in that order; X, Y, and Z represent other spells
            // ConcentrationEffects = { X, Y, Z, ..., B, C, D, A }
            // If there are only ever 2 or less pulsing spells active, then the oldest one will always be at the end.
            // However, if an update or modification allows more than 2 to be active, the goofy ordering of the spells
            // will prevent us from knowing which spell is the oldest and should be canceled - we can go ahead and simply
            // cancel the last spell in the list (which will result in inconsistent behavior) or change the code that adds
            // spells to ConcentrationEffects so that it enforces predictable ordering.
            if (pulsingSpells.Count > 1)
            {
                pulsingSpells[pulsingSpells.Count - 1].Cancel(false);
            }
        }

        #endregion

        //

        /// <summary>
        /// Cast a spell by using an item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool CastSpell(InventoryItem item)
        {
            m_spellItem = item;
            return CastSpell(Caster.TargetObject as GameLiving);
        }

        /// <summary>
        /// Cast a spell by using an Item
        /// </summary>
        /// <param name="targetObject"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool CastSpell(GameLiving targetObject, InventoryItem item)
        {
            m_spellItem = item;
            return CastSpell(targetObject);
        }

        /// <summary>
        /// called whenever the player clicks on a spell icon
        /// or a GameLiving wants to cast a spell
        /// </summary>
        public virtual bool CastSpell()
        {

            return CastSpell(Caster.TargetObject as GameLiving);
        }


        public virtual bool CastSpell(GameLiving targetObject)
        {
            bool success = true;

            m_spellTarget = targetObject;


            Caster.Notify(GameLivingEvent.CastStarting, m_caster, new CastingEventArgs(this));



            //[Stryve]: Do not break stealth if spell can be cast without breaking stealth.
            if (Caster is GamePlayer && UnstealthCasterOnStart)
                ((GamePlayer)Caster).Stealth(false);
              

            if (Caster.IsEngaging)
            {
                EngageEffect effect = Caster.EffectList.GetOfType<EngageEffect>();

                if (effect != null)
                    effect.Cancel(false);
            }

            m_interrupted = false;

            if (Spell.Target.ToLower() == "pet")
            {
                // Pet is the target, check if the caster is the pet.

                if (Caster is GameNPC && (Caster as GameNPC).Brain is IControlledBrain)
                    m_spellTarget = Caster;

                if (Caster is GamePlayer && Caster.ControlledBrain != null && Caster.ControlledBrain.Body != null)
                {
                    if (m_spellTarget == null || !Caster.IsControlledNPC(m_spellTarget as GameNPC))
                    {
                        m_spellTarget = Caster.ControlledBrain.Body;
                    }
                }
            }
            else if (Spell.Target.ToLower() == "controlled")
            {
                // Can only be issued by the owner of a pet and the target
                // is always the pet then.

                if (Caster is GamePlayer && Caster.ControlledBrain != null)
                    m_spellTarget = Caster.ControlledBrain.Body;
                else
                    m_spellTarget = null;
            }
           
            if (Spell.Pulse != 0 && !Spell.IsFocus && CancelPulsingSpell(Caster, Spell.SpellType))
            {
                if (Spell.InstrumentRequirement == 0)
                {
                    if (Caster is GamePlayer)
                    {
                        //MessageToCaster("You cancel your effect.", eChatType.CT_Spell);
                        (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YouCancelYourEffect"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    }
                }
                else

                     if (Caster is GamePlayer)
                {
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CCSpellHandler.StopPlayingSong"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    //MessageToCaster("You stop playing your song.", eChatType.CT_Spell);
                }
            }
            else if (GameServer.ServerRules.IsAllowedToCastSpell(Caster, m_spellTarget, Spell, m_spellLine))
            {
                if (CheckBeginCast(m_spellTarget))
                {
                    if (m_caster is GamePlayer && (m_caster as GamePlayer).IsOnHorse && !HasPositiveEffect)
                    {
                        (m_caster as GamePlayer).IsOnHorse = false;
                    }

                    if (!Spell.IsInstantCast)
                    {
                        //is the spell a Call pet ?
                        if (Caster is GamePlayer && Spell.SpellType == "FirbolgStagCallStone1")
                        {
                            Caster.JuwelinePetCount = 1;
                        }
                        StartCastTimer(m_spellTarget);//fehlte


                        if ((Caster is GamePlayer && (Caster as GamePlayer).IsStrafing) || Caster.IsMoving)
                            CasterMoves();
                    }
                    else
                    {

                        if (Caster.ControlledBrain == null || Caster.ControlledBrain.Body == null || !(Caster.ControlledBrain.Body is NecromancerPet))
                        {
                            SendCastAnimation(0);
                        }

                        FinishSpellCast(m_spellTarget);
                    }
                }
                else
                {
                    success = false;
                }
            }

            // This is critical to restore the casters state and allow them to cast another spell
            if (!IsCasting)
                OnAfterSpellCastSequence();

            return success;
        }



        #region New Archery Check Arrows


        protected static eDamageType m_arrowDamageType;
        public static eDamageType ArrowDamageType

        {
            get { return m_arrowDamageType; }
            set { m_arrowDamageType = value; }
        }



        protected int m_arrowRange;
        public int ArrowRange

        {
            get { return m_arrowRange; }
            set { m_arrowRange = value; }
        }


        protected static int m_arrowDamgePercent;
        public static int ArrowDamagePercent

        {
            get { return m_arrowDamgePercent; }
            set { m_arrowDamgePercent = value; }
        }

        protected static int m_arrowAccuracyPercent;
        public static int ArrowAccuracyPercent

        {
            get { return m_arrowAccuracyPercent; }
            set { m_arrowAccuracyPercent = value; }
        }


        /// <summary>
        /// Holds the arrows for next range attack
        /// </summary>
        protected WeakReference m_rangeAttackAmmo;

        /// <summary>
        /// Gets/Sets the item that is used for ranged attack
        /// </summary>
        /// <returns>Item that will be used for range/accuracy/damage modifications</returns>
        protected virtual InventoryItem RangeAttackAmmo
        {
            get
            {
                //TODO: ammo should be saved on start of every range attack and used here
                InventoryItem ammo = null;//(InventoryItem)m_rangeAttackArrows.Target;

                InventoryItem weapon = Caster.AttackWeapon;
                if (weapon != null)
                {
                    switch (weapon.Object_Type)
                    {
                        case (int)eObjectType.Thrown: ammo = Caster.Inventory.GetItem(eInventorySlot.DistanceWeapon); break;
                        case (int)eObjectType.Crossbow:
                        case (int)eObjectType.Longbow:
                        case (int)eObjectType.CompositeBow:
                        case (int)eObjectType.RecurvedBow:
                        case (int)eObjectType.Fired:
                            {
                                switch (Caster.ActiveQuiverSlot)
                                {
                                    case GameLiving.eActiveQuiverSlot.First: ammo = Caster.Inventory.GetItem(eInventorySlot.FirstQuiver); break;
                                    case GameLiving.eActiveQuiverSlot.Second: ammo = Caster.Inventory.GetItem(eInventorySlot.SecondQuiver); break;
                                    case GameLiving.eActiveQuiverSlot.Third: ammo = Caster.Inventory.GetItem(eInventorySlot.ThirdQuiver); break;
                                    case GameLiving.eActiveQuiverSlot.Fourth: ammo = Caster.Inventory.GetItem(eInventorySlot.FourthQuiver); break;
                                    case GameLiving.eActiveQuiverSlot.None:
                                        eObjectType findType = eObjectType.Arrow;
                                        if (weapon.Object_Type == (int)eObjectType.Crossbow)
                                            findType = eObjectType.Bolt;

                                       // ammo = Caster.Inventory.GetFirstItemByObjectType((int)findType, eInventorySlot.FirstQuiver, eInventorySlot.LastEmptyQuiver);
                                        ammo = Caster.Inventory.GetFirstItemByObjectType((int)findType, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
                                        break;
                                }
                            }
                            break;
                    }
                }

                return ammo;
            }
            set { m_rangeAttackAmmo.Target = value; }
        }

         
       

        /// <summary>
        /// Check the selected range ammo and decides if it's compatible with select weapon
        /// </summary>
        /// <returns>True if compatible, false if not</returns>
        protected virtual bool CheckRangedAmmoCompatibilityWithActiveWeapon()
        {
            InventoryItem weapon = Caster.AttackWeapon;
            if (weapon != null)
            {
                switch ((eObjectType)weapon.Object_Type)
                {
                    case eObjectType.Crossbow:
                    case eObjectType.Longbow:
                    case eObjectType.CompositeBow:
                    case eObjectType.RecurvedBow:
                    case eObjectType.Fired:
                        {
                            if (Caster.ActiveQuiverSlot != GameLiving.eActiveQuiverSlot.None)
                            {
                                InventoryItem ammo = null;
                                switch (Caster.ActiveQuiverSlot)
                                {
                                    case GameLiving.eActiveQuiverSlot.Fourth: ammo = Caster.Inventory.GetItem(eInventorySlot.FourthQuiver); break;
                                    case GameLiving.eActiveQuiverSlot.Third: ammo = Caster.Inventory.GetItem(eInventorySlot.ThirdQuiver); break;
                                    case GameLiving.eActiveQuiverSlot.Second: ammo = Caster.Inventory.GetItem(eInventorySlot.SecondQuiver); break;
                                    case GameLiving.eActiveQuiverSlot.First: ammo = Caster.Inventory.GetItem(eInventorySlot.FirstQuiver); break;
                                }

                                if (ammo == null) return false;
                                //log.ErrorFormat("Munition Type range: {0}", GlobalConstants.AmmunitionTypeToRangeName(ammo.SPD_ABS));

                                //Get the Arrow range type
                                MaxArrowRange(GlobalConstants.AmmunitionTypeToRangeName(ammo.SPD_ABS), weapon);
                                ArrowDamage(GlobalConstants.AmmunitionTypeToDamageName(ammo.SPD_ABS));
                                ArrowoAccuracy(GlobalConstants.AmmunitionTypeToAccuracyName(ammo.SPD_ABS));
                               
                                switch(ammo.Type_Damage)
                                {
                                    case 0:
                                        {
                                            m_arrowDamageType = eDamageType.Natural;
                                            break;
                                        }
                                    case 1:
                                        {
                                            m_arrowDamageType = eDamageType.Crush;
                                            //og.Error("Crush");
                                            break;
                                        }
                                           
                                    case 2:
                                        {
                                            m_arrowDamageType = eDamageType.Slash;
                                            //log.Error("Slash");
                                            break;
                                        }
                                    case 3:
                                        {
                                            m_arrowDamageType = eDamageType.Thrust;
                                           //log.Error("Thrust");
                                            break;
                                        }

                                    default: m_arrowDamageType = eDamageType.Slash; break; 
                                }
                                
                             



                                if (weapon.Object_Type == (int)eObjectType.Crossbow)
                                    return ammo.Object_Type == (int)eObjectType.Bolt;
                                return ammo.Object_Type == (int)eObjectType.Arrow;


                               
                            }
                        }
                        break;
                }
            }
            return true;
        }

        

        public virtual int ArrowDamage(string ammutype)
        {

            switch (ammutype)
            {
                case "light":
                    m_arrowDamgePercent = 80;
                    break;
                case "medium":
                    m_arrowDamgePercent = 85;
                    break;
                case "heavy":
                    m_arrowDamgePercent = 90;
                    break;
                case "X-heavy":
                    m_arrowDamgePercent = 95;
                    break;
               
            }
            return 100;
        }
        public virtual int ArrowoAccuracy(string ammutype)
        {
           
            switch (ammutype)
            {
                case "reduced":
                    //log.Error("reduced");
                    m_arrowAccuracyPercent = 84;
                    break;
                case "normal":
                    //log.Error("normal");
                    m_arrowAccuracyPercent = 90;
                    break;
               case "enhanced":
                    //log.Error("enhanced");
                    m_arrowAccuracyPercent = 95;
                    break;
                case "improved":
                    //log.Error("improved");
                    m_arrowAccuracyPercent = 99;
                    break;
            }
           
            return 100;
        }


        
        //public int ArrowRange = 0;
        public virtual int MaxArrowRange(string arrowType, InventoryItem weapon)
        {


            switch ((eObjectType)weapon.Object_Type)
            {
              
                        //Hibernia Recurve Bow Medium 1428, 1680, 2100
                        //Albion Longbow Heavy 1496, 1760, 2200
                        //Midgard Composite Bow Light, 1360, 1600, 2000

                   case eObjectType.Longbow:
                    {

                        switch (arrowType)
                        {
                            case "short":
                                m_arrowRange = 1496;
                                break;
                            case "medium":
                                m_arrowRange = 1760;
                                break;
                            case "X-long":
                               m_arrowRange = 2200;
                                break;
                        }
                        break;
                    }
                case eObjectType.CompositeBow:
                    {
                        switch (arrowType)
                        {
                            case "short":
                                m_arrowRange = 1360;
                                break;
                            case "medium":
                                m_arrowRange = 1600;
                                break;
                            case "X-long":
                                m_arrowRange = 2000;
                                break;
                        }
                        break;
                    }
                case eObjectType.RecurvedBow:
                    {
                        switch (arrowType)
                        {
                            case "short":
                                m_arrowRange = 1428;
                                break;
                            case "medium":
                                m_arrowRange = 1680;
                                break;
                            case "X-long":
                                m_arrowRange = 2100;
                                break;
                        }
                        break;
                    }

                default: return 1000;
            }
            return 1000;

        }



        public virtual bool CheckForArrow(GameLiving caster)
        {
            if (caster is GamePlayer)
            {
                GamePlayer player = caster as GamePlayer;


               if (player.ActiveQuiverSlot == GameLiving.eActiveQuiverSlot.None)
                {
                    //player.ActiveQuiverSlot = GameLiving.eActiveQuiverSlot.None;
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.SwitchQuiver.NoMoreAmmo"), eChatType.CT_System, eChatLoc.CL_SystemWindow);


                    return false;
                }
                


                if (player.IsArcherClass())
                {

                   
                    if (RangeAttackAmmo == null)
                    {
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.StartAttack.SelectQuiver"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        return false;
                    }
                   
                    // remove an arrow and endurance
                    // Check if selected ammo is compatible for ranged attack
                    if (!CheckRangedAmmoCompatibilityWithActiveWeapon())
                    {
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.StartAttack.CantUseQuiver"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        return false;
                    }
                   

                    return true;
                }
                return true;
            }
            return true;
        }

        #endregion


        public virtual void StartCastTimer(GameLiving target)
        {
            m_interrupted = false;
            SendSpellMessages();

            if (Caster is GamePlayer && Properties.Allow_New_Archery_Check_Arrows)
            {

                if (Spell.SpellType == "Archery" && CheckForArrow(Caster) == false)
                {

                    return;
                }
            }


            int time = CalculateCastingTime();


            int step1 = time / 3;
            if (step1 > ServerProperties.Properties.SPELL_INTERRUPT_MAXSTAGELENGTH)
                step1 = ServerProperties.Properties.SPELL_INTERRUPT_MAXSTAGELENGTH;
            if (step1 < 1)
                step1 = 1;

            int step3 = time / 3;
            if (step3 > ServerProperties.Properties.SPELL_INTERRUPT_MAXSTAGELENGTH)
                step3 = ServerProperties.Properties.SPELL_INTERRUPT_MAXSTAGELENGTH;
            if (step3 < 1)
                step3 = 1;

            int step2 = time - step1 - step3;
            if (step2 < 1)
                step2 = 1;

            if (Caster is GamePlayer && ServerProperties.Properties.ENABLE_DEBUG)
            {
                (Caster as GamePlayer).Out.SendMessage("[DEBUG] spell time = " + time.ToString() + ", step1 = " + step1.ToString() + ", step2 = " + step2.ToString() + ", step3 = " + step3.ToString(), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }

            try
            {


                m_castTimer = new DelayedCastTimer(Caster, this, target, step2, step3);
                m_castTimer.Start(step1);
                m_started = Caster.CurrentRegion.Time;

                SendCastAnimation();
                if (m_caster.IsMoving || m_caster.IsStrafing)
                {
                    CasterMoves();
                }
            }
            catch
            {
                log.Error("Unknown error in Spellhandler -->> DelayedCastTimer");
            }
        }

        /// <summary>
        /// Is called when the caster moves
        /// </summary>
        public virtual void CasterMoves()
        {
            if (Spell.InstrumentRequirement != 0)
                return;

            if (Spell.MoveCast)
                return;

            InterruptCasting();

            if (Caster is GamePlayer)
            {
                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "SpellHandler.CasterMove"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                Caster.JuwelinePetCount = 0;
            }
        }

        /// <summary>
        /// This sends the spell messages to the player/target.
        ///</summary>
        public virtual void SendSpellMessages()
        {
            if (Spell.InstrumentRequirement == 0)
            {
                //player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractArea.Left", Description),
                //   eChatType.CT_System, eChatLoc.CL_SystemWindow);
                if (Caster is GamePlayer)
                {
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.CasterBeginCasting"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }
                else

                    MessageToCaster("You begin casting a " + Spell.Name + " spell!", eChatType.CT_Spell);
                //(Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.CasterBeginCastingSpellName", Spell.Name), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
            }
            else
            {
                if (Caster is GamePlayer)
                {
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.CasterBeginPlaying", Spell.Name), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }
                else
                    //(Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.CasterBeginPlaying", Spell.Name), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    MessageToCaster("You begin playing " + Spell.Name + "!", eChatType.CT_Spell);
            }
        }


        /// <summary>
        /// casting sequence has a chance for interrupt through attack from enemy
        /// the final decision and the interrupt is done here
        /// TODO: con level dependend
        /// </summary>
        /// <param name="attacker">attacker that interrupts the cast sequence</param>
        /// <returns>true if casting was interrupted</returns>
        public virtual bool CasterIsAttacked(GameLiving attacker)
        {
            //[StephenxPimentel] Check if the necro has MoC effect before interrupting.
            if (Caster is NecromancerPet)
            {
                if ((Caster as NecromancerPet).Owner.EffectList.GetOfType<MasteryofConcentrationEffect>() != null)
                {
                    return false;
                }
            }
            //Check if the necro owner has MoC or Facilitate effect before interrupting.
            if (Caster is NecromancerPet)
            {
                GameLiving petowner = null;
                NecromancerPet Necpet = Caster as NecromancerPet;
                if (petowner == null)
                {
                    petowner = ((Necpet.Brain as IControlledBrain).Owner) as GameLiving;
                }
                if (petowner != null && petowner.EffectList.CountOfType(typeof(MasteryofConcentrationEffect), typeof(FacilitatePainworkingEffect)) > 0)
                {
                    return false;
                }
            }

            if (Spell.Uninterruptible)
                return false;

            if (Caster.EffectList.CountOfType(typeof(QuickCastEffect), typeof(MasteryofConcentrationEffect), typeof(FacilitatePainworkingEffect)) > 0)
                return false;


            //Check Attackers Con
            AttackerIsGreyCon(Caster, attacker);

            if (AttackerIsGreyConToCaster)
            {
                //log.DebugFormat("attacker is greycon");
                return false;
            }

            if (IsCasting && Stage < 2)
            {


                if (Caster.ChanceSpellInterrupt(attacker))
                {
                    if (Caster is GamePlayer)
                    {

                        Caster.LastInterruptMessage = string.Format(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client.Account.Language, "GameLiving.AttacksYouAndYourSpellIsInterrupted"), attacker.GetName(0, true));
                        MessageToLiving(Caster, Caster.LastInterruptMessage, eChatType.CT_SpellResisted);
                    }

                    InterruptCasting(); // always interrupt at the moment
                    return true;
                }
            }
            return false;
        }

        #region begin & end cast check

        public virtual bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (selectedTarget != null)
            {
                //Check the Con of the target
                AttackerIsGreyCon(Caster, selectedTarget);
            }
            return CheckBeginCast(selectedTarget, false);
        }

        /// <summary>
        /// All checks before any casting begins
        /// </summary>
        /// <param name="selectedTarget"></param>
        /// <returns></returns>
        public virtual bool CheckBeginCast(GameLiving selectedTarget, bool quiet)
        {
            if (m_caster.ObjectState != GameLiving.eObjectState.Active)
            {
                return false;
            }

          
            //Stop melee attack wehn casting             
            if (Caster is GamePlayer && (Caster as GamePlayer).AttackState && !Spell.Uninterruptible && Caster.ActiveWeaponSlot != GameLiving.eActiveWeaponSlot.Distance && Spell.CastTime > 0)
            {
                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.InAttackStateCast", Spell.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);

                (Caster as GamePlayer).StopAttack();
            }

            if (Spell.SpellType != "MonsterDoT" && Spell.SpellType != "MonsterDisease" && SpellHandler.FindEffectOnTarget(m_caster, "SummonMonster") != null)
            {
                if (!quiet)
                    if (m_caster is GamePlayer)
                    {
                        ((GamePlayer)m_caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client.Account.Language, "SpellHandler.YouAreDeatCantCast"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                return false;
            }
          
            if (!m_caster.IsAlive)
            {
                if (!quiet)
                    if (m_caster is GamePlayer)
                    {
                        ((GamePlayer)m_caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client.Account.Language, "SpellHandler.YouAreDeatCantCast"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                return false;
            }


            if (m_caster is GamePlayer)
            {
                long nextSpellAvailTime = m_caster.TempProperties.getProperty<long>(GamePlayer.NEXT_SPELL_AVAIL_TIME_BECAUSE_USE_POTION);

                if (nextSpellAvailTime > m_caster.CurrentRegion.Time)
                {
                    ((GamePlayer)m_caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client.Account.Language, "GamePlayer.CastSpell.MustWaitBeforeCast", (nextSpellAvailTime - m_caster.CurrentRegion.Time) / 1000), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return false;
                }
                if (((GamePlayer)m_caster).Steed != null && ((GamePlayer)m_caster).Steed is GameSiegeRam)
                {
                    if (!quiet)
                        //MessageToCaster("You can't cast in a siegeram!.", eChatType.CT_System);
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YouCantCastInASiegeram"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return false;
                }
                GameSpellEffect naturesWomb = FindEffectOnTarget(Caster, typeof(NaturesWombEffect));
                if (naturesWomb != null)
                {
                    //[StephenxPimentel]
                    //Get Correct Message for 1.108 update.
                    //MessageToCaster("You are silenced and cannot cast a spell right now.", eChatType.CT_SpellResisted);
                    if (m_caster is GamePlayer)
                    {
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YouAreSilenced"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                    return false;
                }

                // ((GamePlayer)m_caster).UpdateDisabledSkills();
            }

            GameSpellEffect Phaseshift = FindEffectOnTarget(Caster, "Phaseshift");
            if (Phaseshift != null && (Spell.InstrumentRequirement == 0 || Spell.SpellType == "Mesmerize" || Spell.SpellType == "AOEMesmerize"))
            {
                if (!quiet)
                    if (m_caster is GamePlayer)
                    {
                        // MessageToCaster("You're phaseshifted and can't cast a spell", eChatType.CT_System);
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YouArePhaseshifted"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                return false;
            }

            // Apply Mentalist RA5L
            if (Spell.Range > 0)
            {
                SelectiveBlindnessEffect SelectiveBlindness = Caster.EffectList.GetOfType<SelectiveBlindnessEffect>();
                if (SelectiveBlindness != null)
                {
                    GameLiving EffectOwner = SelectiveBlindness.EffectSource;
                    if (EffectOwner == selectedTarget)
                    {
                        if (m_caster is GamePlayer && !quiet)
                        {
                            //  ((GamePlayer)m_caster).Out.SendMessage(string.Format("{0} is invisible to you!", selectedTarget.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                            (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.IsNotVisibleForYou", selectedTarget.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                        }
                        return false;
                    }
                }
            }

            if (selectedTarget != null && selectedTarget.HasAbility("DamageImmunity") && Spell.SpellType == "DirectDamage" && Spell.Radius == 0)
            {
                if (!quiet)
                    if (m_caster is GamePlayer)
                    {
                        //MessageToCaster(selectedTarget.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "CCSpellHandler.TargetIsImmuneToThisEffect", selectedTarget.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        //log.Error("hier 1");
                    }
                return false;
            }

            if (m_spell.InstrumentRequirement != 0)
            {
                if (!CheckInstrument())
                {
                    if (!quiet)
                        if (m_caster is GamePlayer)
                        {
                            ((GamePlayer)m_caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client.Account.Language, "SpellHandler.NotRightTypeOfInstrument"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            //MessageToCaster("You are not wielding the right type of instrument!",
                        }
                    return false;
                }
            }
            else if (m_caster.IsSitting) // songs can be played if sitting
            {
                //Purge can be cast while sitting but only if player has negative effect that
                //don't allow standing up (like stun or mez)
                if (!quiet)
                    if (m_caster is GamePlayer)
                    {
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YouCantCastWhileSitting"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                return false;
            }


            if (m_caster is GameNPC && m_caster.AttackState && m_spell.CastTime != 0)
            {

                if (m_caster.CanCastInCombat(Spell) == false && AttackerIsGreyConToCaster == false)
                {
                    m_caster.StopAttack();
                    return false;
                }
            }


            if (Stage < 2 && AttackerIsGreyConToCaster == false && !m_spell.Uninterruptible && m_spell.CastTime > 0 && m_caster is GamePlayer &&
                m_caster.EffectList.GetOfType<QuickCastEffect>() == null && m_caster.EffectList.GetOfType<MasteryofConcentrationEffect>() == null
                || Stage < 2 && m_caster.EffectList.GetOfType<QuickCastEffect>() == null && AttackerIsGreyConToCaster == false && m_caster.EffectList.GetOfType<MasteryofConcentrationEffect>() == null && !m_spell.Uninterruptible && m_spell.CastTime > 0 && m_caster is GamePlayer)//Bodygard will break if hit by negative spells
            {


                if (Caster.InterruptAction > 0 && Caster.InterruptAction + Caster.SpellInterruptRecastTime > Caster.CurrentRegion.Time && AttackerIsGreyConToCaster == false)
                {
                    if (!quiet)
                        if (Caster is GamePlayer)
                        {
                            (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourMustWaitToUseSpell", (((Caster.InterruptAction + Caster.SpellInterruptRecastTime) - Caster.CurrentRegion.Time) / 1000 + 1)), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            // MessageToCaster("You must wait " + (((Caster.InterruptAction + Caster.SpellInterruptRecastTime) - Caster.CurrentRegion.Time) / 1000 + 1).ToString() + " seconds to cast a spell!", eChatType.CT_SpellResisted);
                        }
                    return false;
                }
            }

            if (m_spell.RecastDelay > 0)
            {

                int left = m_caster.GetSkillDisabledDuration(m_spell);
                if (left > 0)
                {
                    if (m_caster is NecromancerPet && ((m_caster as NecromancerPet).Owner as GamePlayer).Client.Account.PrivLevel > (int)ePrivLevel.Player)
                    {
                        // Ignore Recast Timer
                    }
                    else
                    {
                        if (!quiet)
                            if (Caster is GamePlayer)
                            {
                                //MessageToCaster("You must wait " + (left / 1000 + 1).ToString() + " seconds to use this spell!", eChatType.CT_System);
                                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourMustWaitToUseSpell", (left / 1000 + 1).ToString()), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                        return false;
                    }
                }
            }

            String targetType = m_spell.Target.ToLower();

            //[Ganrod] Nidel: Can cast pet spell on all Pet/Turret/Minion (our pet)
            if (targetType.Equals("pet"))
            {
                if (selectedTarget == null || !Caster.IsControlledNPC(selectedTarget as GameNPC))
                {
                    if (Caster.ControlledBrain != null && Caster.ControlledBrain.Body != null)
                    {
                        selectedTarget = Caster.ControlledBrain.Body;
                    }
                    else
                    {
                        if (!quiet)

                            if (Caster is GamePlayer)
                            {
                                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YouCanThisOnlyOnSelfControllingCreatures"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                //MessageToCaster("You must cast this spell on a creature you are controlling.", eChatType.CT_System);
                            }
                        return false;
                    }
                }
            }
            if (targetType == "area")
            {
                if (!m_caster.IsWithinRadius(m_caster.GroundTarget, CalculateSpellRange()))
                {
                    if (!quiet)
                        if (m_caster is GamePlayer)
                        {
                            (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.SelectACloserGroundTarget"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            //MessageToCaster("Your area target is out of range.  Select a closer target.", eChatType.CT_SpellResisted);
                        }
                    return false;
                }
                if (ServerProperties.Properties.SPELL_GTAE_NEED_LOS)
                {
                    if (!Caster.GroundTargetInView)
                    {
                        if (Caster is GamePlayer)
                        {
                            //MessageToCaster("Your ground target is not in view!", eChatType.CT_SpellResisted);
                            (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourGroundTargetIsNotInView"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        }
                        return false;
                    }
                }
            }
            else if (targetType != "self" && targetType != "group" && targetType != "pet"
                     && targetType != "controlled" && targetType != "cone" && m_spell.Range > 0)
            {
                // All spells that need a target.

                if (selectedTarget == null || selectedTarget.ObjectState != GameLiving.eObjectState.Active)
                {
                    if (Caster is GamePlayer && !quiet)
                    {
                        (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.SelectATargetForThisSpell", Spell.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        //MessageToCaster("You must select a target for this spell!", eChatType.CT_SpellResisted);
                    }
                    return false;
                }

                if (!m_caster.IsWithinRadius(selectedTarget, CalculateSpellRange()))
                {
                    if (Caster is GamePlayer && !quiet)
                    {
                        //MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
                        (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourTargetIsToFarAway"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                    Caster.Notify(GameLivingEvent.CastFailed, new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetTooFarAway));
                    return false;
                }

                switch (m_spell.Target.ToLower())
                {
                    case "enemy":
                        if (selectedTarget == m_caster)
                        {
                            if (!quiet)
                                if (m_caster is GamePlayer)
                                {
                                    (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YouCantAttackYourself"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    //MessageToCaster("You can't attack yourself! ", eChatType.CT_System);
                                }
                            return false;
                        }

                        if (FindStaticEffectOnTarget(selectedTarget, typeof(NecromancerShadeEffect)) != null)
                        {
                            if (!quiet)
                                if (m_caster is GamePlayer)
                                {
                                    (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.InvalidTarget"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    //MessageToCaster("Invalid target.", eChatType.CT_System);
                                }
                            return false;
                        }



                        if (m_caster.IsObjectInFront(selectedTarget, 180) == false)
                        {

                            if (Caster is GamePlayer && ((Caster as GamePlayer).CharacterClass).ID == (int)eCharacterClass.Minstrel == false && (!quiet))
                            {
                                //MessageToCaster("Your target is not in view!", eChatType.CT_SpellResisted);
                                (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.InvalidTarget"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                Caster.Notify(GameLivingEvent.CastFailed, new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetNotInView));
                                return false;

                            }
                            //Caster is GamePlayer && m_spell.SpellType == "Mesmerize" == false 
                            if (Caster is GamePlayer && Caster.IsCastingMesmerizeSpellWithPulse(Spell, (Caster as GamePlayer)) == false && (Caster as GamePlayer).IsCharcterClass(eCharacterClass.Minstrel) && (!quiet))
                            {
                                //MessageToCaster("Your target is not in view!", eChatType.CT_SpellResisted);
                                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourTargetNotInView"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                Caster.Notify(GameLivingEvent.CastFailed, new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetNotInView));
                                return false;
                            }
                        }
                       
                        if (m_spell.SpellType == "Charm" && m_spell.CastTime == 0 && m_spell.Pulse != 0)
                            break;

                        if (selectedTarget == m_caster.TargetObject && m_caster.TargetInView == false)
                        {
                            if (!quiet)

                                //MessageToCaster("Your target is not visible!", eChatType.CT_SpellResisted);
                                if (m_caster is GamePlayer)
                                {
                                    //log.Error("kein target");
                                    (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YourTargetIsNotVisible"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                }
                            Caster.Notify(GameLivingEvent.CastFailed, new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetNotInView));
                            return false;
                        }

                        if (!GameServer.ServerRules.IsAllowedToAttack(Caster, selectedTarget, quiet))
                        {
                            return false;
                        }
                        break;

                    case "corpse":
                        if (selectedTarget.IsAlive || !GameServer.ServerRules.IsSameRealm(Caster, selectedTarget, true))
                        {
                            if (!quiet)
                                if (Caster is GamePlayer)
                                {
                                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.OnlyWorksOnDeadRealmMembers"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                    //MessageToCaster("This spell only works on dead members of your realm!", eChatType.CT_SpellResisted);
                                }
                            return false;
                        }
                        break;

                    case "realm":
                        if (GameServer.ServerRules.IsAllowedToAttack(Caster, selectedTarget, true))
                        {
                            return false;
                        }
                        break;
                }

                //heals/buffs/rez need LOS only to start casting
                if (!m_caster.TargetInView && m_spell.Target.ToLower() != "pet" && selectedTarget != Caster)
                {
                    if (!quiet)
                        if (m_caster is GamePlayer)
                        {
                            //MessageToCaster("Your target is not in visible!", eChatType.CT_SpellResisted);
                            (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YourTargetIsNotVisible"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        }
                    Caster.Notify(GameLivingEvent.CastFailed, new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetNotInView));
                    return false;
                }

                if (m_spell.Target.ToLower() != "corpse" && !selectedTarget.IsAlive)
                {
                    if (!quiet)
                        if (m_caster is GamePlayer)
                        {
                            ((GamePlayer)m_caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client.Account.Language, "HealSpellHandler.Execute.TargetIsDead", selectedTarget.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        }
                    return false;
                }
            }

            //Ryan: don't want mobs to have reductions in mana
            if (Spell.Power != 0 && m_caster is GamePlayer && (m_caster as GamePlayer).IsCharcterClass(eCharacterClass.Savage) == false && m_caster.Mana < PowerCost(selectedTarget) && Spell.SpellType != "Archery")
            {
                if (!quiet)
                    if (m_caster is GamePlayer)
                    {
                        //MessageToCaster("You don't have enough power to cast that!", eChatType.CT_SpellResisted);
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YouDontHaveEnoughMana"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                return false;
            }

            if (m_caster is GamePlayer && m_spell.Concentration > 0)
            {
                if (m_caster.Concentration < m_spell.Concentration)
                {
                    if (!quiet)
                        if (m_caster is GamePlayer)
                        {
                            // MessageToCaster("This spell requires " + m_spell.Concentration + " concentration points to cast!", eChatType.CT_SpellResisted);
                            (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.ThisSpellRequiresConcentrationPoints", m_spell.Concentration.ToString()), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        }
                    return false;
                }

                if (m_caster.ConcentrationEffects.ConcSpellsCount >= 50)
                {
                    if (!quiet)
                        if (m_caster is GamePlayer)
                        {
                            //MessageToCaster("You can only cast up to 50 simultaneous concentration spells!", eChatType.CT_SpellResisted);
                            (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YouCanOnlyCastUpTo"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        }
                    return false;
                }
            }

            // Cancel engage if user starts attack
            if (m_caster.IsEngaging)
            {
                EngageEffect engage = m_caster.EffectList.GetOfType<EngageEffect>();
                if (engage != null)
                {
                    engage.Cancel(false);
                }
            }

            if (!(Caster is GamePlayer))
            {
                Caster.Notify(GameLivingEvent.CastSucceeded, this, new PetSpellEventArgs(Spell, SpellLine, selectedTarget));
            }

            return true;
        }

        /// <summary>
        /// Does the area we are in force an LoS check on everything?
        /// </summary>
        /// <param name="living"></param>
        /// <returns></returns>
        protected bool MustCheckLOS(GameLiving living)
        {
            if (!(living is GamePlayer) && ServerProperties.Properties.ALWAYS_CHECK_LOS)
                return true;

            foreach (AbstractArea area in living.CurrentAreas)
            {
                if (area.CheckLOS)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Check the Line of Sight from you to your pet
        /// </summary>
        /// <param name="player">The player</param>
        /// <param name="response">The result</param>
        /// <param name="targetOID">The target OID</param>
        public virtual void CheckLOSYouToPet(GameLiving player, ushort response, ushort targetOID)
        {
            if (player == null) // Hmm
                return;
            if ((response & 0x100) == 0x100) // In view ?
                return;
            if (m_caster is GamePlayer)
            {
                (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YourPetNotInView"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
            }
            //MessageToLiving(player, "Your pet not in view.", eChatType.CT_SpellResisted);
            InterruptCasting(); // break;
        }

        /*
        /// <summary>
        /// Check the Line of Sight from a player to a target
        /// </summary>
        /// <param name="player">The player</param>
        /// <param name="response">The result</param>
        /// <param name="targetOID">The target OID</param>
        public virtual void CheckLOSPlayerToTarget(GameLiving player, ushort response, ushort targetOID)
        {
            if (player == null) // Hmm
                return;

            if ((response & 0x100) == 0x100) // In view?
                return;

            if (ServerProperties.Properties.ENABLE_DEBUG)
            {
                MessageToCaster("LoS Interrupt in CheckLOSPlayerToTarget", eChatType.CT_System);
                log.Debug("LoS Interrupt in CheckLOSPlayerToTarget");
            }

            if (m_caster is GamePlayer)
            {
                //MessageToCaster("You can't see your target from here!", eChatType.CT_SpellResisted);
                (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YouCantSeeYourTarget"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
            }

            InterruptCasting();
        }
        */

        /// <summary>
        /// Check the Line of Sight from an npc to a target
        /// </summary>
        /// <param name="player">The player</param>
        /// <param name="response">The result</param>
        /// <param name="targetOID">The target OID</param>
        public virtual void CheckLOSNPCToTarget(GameLiving living, ushort response, ushort targetOID)
        {
            if (living == null) // Hmm
                return;

            if ((response & 0x100) == 0x100) // In view?
                return;

            if (ServerProperties.Properties.ENABLE_DEBUG)
            {
                MessageToCaster("LoS Interrupt in CheckLOSNPCToTarget", eChatType.CT_System);
                log.Debug("LoS Interrupt in CheckLOSNPCToTarget");
            }

            InterruptCasting();
        }
        /// <summary>
        /// Checks after casting before spell is executed
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual bool CheckEndCast(GameLiving target)
        {
            if (m_caster.ObjectState != GameLiving.eObjectState.Active)
            {
                return false;
            }

            if (!m_caster.IsAlive)
            {

                if (m_caster is GamePlayer)
                {
                    //MessageToCaster("You are dead and can't cast!", eChatType.CT_System);
                    ((GamePlayer)m_caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client.Account.Language, "SpellHandler.YouAreDeatCantCast"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                return false;
            }

            if (m_spell.InstrumentRequirement != 0)
            {
                if (!CheckInstrument())
                {
                    //MessageToCaster("You are not wielding the right type of instrument!", eChatType.CT_SpellResisted);
                    ((GamePlayer)m_caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client.Account.Language, "SpellHandler.NotRightTypeOfInstrument"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    return false;
                }
            }
            else if (m_caster.IsSitting) // songs can be played if sitting
            {
                //Purge can be cast while sitting but only if player has negative effect that
                //don't allow standing up (like stun or mez)
                //MessageToCaster("You can't cast while sitting!", eChatType.CT_SpellResisted);
                if (m_caster is GamePlayer)
                {
                    (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YouCantCastWhileSitting"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
                return false;
            }

            if (m_spell.Target.ToLower() == "area")
            {
                if (!m_caster.IsWithinRadius(m_caster.GroundTarget, CalculateSpellRange()))
                {
                    if (m_caster is GamePlayer)
                    {
                        // MessageToCaster("Your area target is out of range.  Select a closer target.", eChatType.CT_SpellResisted);
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.SelectACloserGroundTarget"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                    return false;
                }
            }
            else if (m_spell.Target.ToLower() != "self" && m_spell.Target.ToLower() != "group" && m_spell.Target.ToLower() != "cone" && m_spell.Range > 0)
            {
                if (m_spell.Target.ToLower() != "pet")
                {
                    //all other spells that need a target
                    if (target == null || target.ObjectState != GameObject.eObjectState.Active)
                    {
                        if (m_caster is GamePlayer)
                        {
                            (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.SelectATargetForThisSpell", Spell.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            // MessageToCaster("You must select a target for this spell!", eChatType.CT_SpellResisted);
                        }
                        return false;
                    }

                    if (!m_caster.IsWithinRadius(target, CalculateSpellRange()))
                    {
                        if (m_caster is GamePlayer)
                        {
                            (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YourTargetIsToFarAway"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        }
                        //MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
                        return false;
                    }
                }

                switch (m_spell.Target)
                {
                    case "Enemy":


                        //Minstrel  Mesmerize need no target at start of the Spell
                        if (target == null || (target != Caster && target != null && Caster is GamePlayer && ((Caster as GamePlayer).CharacterClass).ID == (int)eCharacterClass.Minstrel == false && !m_caster.IsObjectInFront(target, 175)))
                        {
                            //log.Error("euer ziel ist nicht in sicht1");
                            //MessageToCaster("Your target is not in view. The spell fails.", eChatType.CT_SpellResisted);
                            (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.TargetNotInViewSpellFails"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            Caster.TempProperties.setProperty(AnimStatus, true);
                            return false;
                        }
                        //log.Error("Remove propertie2");
                        Caster.TempProperties.removeProperty(AnimStatus);

                        if (target == null || (target != m_caster && target != null && Caster is GamePlayer && Caster.IsCastingMesmerizeSpellWithPulse(Spell, (Caster as GamePlayer)) == false && ((Caster as GamePlayer).CharacterClass).ID == (int)eCharacterClass.Minstrel == true && !m_caster.IsObjectInFront(target, 175)))
                        {

                            //MessageToCaster("Your target is not in view. The spell fails.", eChatType.CT_SpellResisted);
                            (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.TargetNotInViewSpellFails"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);

                            return false;
                        }



                        if (!GameServer.ServerRules.IsAllowedToAttack(Caster, target, false))
                        {
                            return false;
                        }
                        break;

                    case "Corpse":
                        if (target.IsAlive || !GameServer.ServerRules.IsSameRealm(Caster, target, true))
                        {
                            if (Caster is GamePlayer)
                            {
                                //MessageToCaster("This spell only works on dead members of your realm!", eChatType.CT_SpellResisted);
                                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.OnlyWorksOnDeadRealmMembers"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            }
                            return false;
                        }
                        break;

                    case "Realm":
                        if (GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
                        {
                            return false;
                        }
                        break;

                    case "Pet":
                        /*
                         * [Ganrod] Nidel: Can cast pet spell on all Pet/Turret/Minion (our pet)
                         * -If caster target's isn't own pet.
                         *  -check if caster have controlled pet, select this automatically
                         *  -check if target isn't null
                         * -check if target isn't too far away
                         * If all checks isn't true, return false.
                         */
                        if (target == null || !Caster.IsControlledNPC(target as GameNPC))
                        {
                            if (Caster.ControlledBrain != null && Caster.ControlledBrain.Body != null)
                            {
                                target = Caster.ControlledBrain.Body;
                            }
                            else
                            {
                                if (Caster is GamePlayer)
                                {
                                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YouCanThisOnlyOnSelfControllingCreatures"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                    //MessageToCaster("You must cast this spell on a creature you are controlling.", eChatType.CT_System);
                                }
                                return false;
                            }
                        }
                        //Now check distance for own pet
                        if (!m_caster.IsWithinRadius(target, CalculateSpellRange()))
                        {
                            if (m_caster is GamePlayer)
                            {
                                // MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
                                (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YourTargetIsToFarAway"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            }
                            return false;
                        }
                        break;
                }
            }

            if (m_caster.Mana <= 0 && Spell.Power != 0 && Spell.SpellType != "Archery")
            {
                if (m_caster is GamePlayer)
                {
                    //MessageToCaster("You have exhausted all of your power and cannot cast spells!", eChatType.CT_SpellResisted);
                    (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YouHaveExhaustedAllMana"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
                return false;
            }
            if (Spell.Power != 0 && m_caster.Mana < PowerCost(target) && Spell.SpellType != "Archery")
            {
                if (m_caster is GamePlayer)
                {
                    //MessageToCaster("You don't have enough power to cast that!", eChatType.CT_SpellResisted);
                    (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YouDontHaveEnoughMana"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
                return false;
            }

            if (m_caster is GamePlayer && m_spell.Concentration > 0 && m_caster.Concentration < m_spell.Concentration)
            {
                //MessageToCaster("This spell requires " + m_spell.Concentration + " concentration points to cast!", eChatType.CT_SpellResisted);
                (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.ThisSpellRequiresConcentrationPoints", m_spell.Concentration.ToString()), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (m_caster is GamePlayer && m_spell.Concentration > 0 && m_caster.ConcentrationEffects.ConcSpellsCount >= 50)
            {
                //MessageToCaster("You can only cast up to 50 simultaneous concentration spells!", eChatType.CT_SpellResisted);
                (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YouCanOnlyCastUpTo"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return false;
            }

            return true;
        }

        public virtual bool CheckDuringCast(GameLiving target)
        {
            return CheckDuringCast(target, false);
        }

        public virtual bool CheckDuringCast(GameLiving target, bool quiet)
        {
            if (m_interrupted)
            {
                return false;
            }

            if (Stage < 2 && !m_spell.Uninterruptible && m_spell.CastTime > 0 && AttackerIsGreyConToCaster == false && m_caster is GamePlayer &&
                m_caster.EffectList.GetOfType<QuickCastEffect>() == null && m_caster.EffectList.GetOfType<MasteryofConcentrationEffect>() == null
                 || Stage < 2 && m_caster.EffectList.GetOfType<QuickCastEffect>() == null && AttackerIsGreyConToCaster == false && m_caster.EffectList.GetOfType<MasteryofConcentrationEffect>() == null && !m_spell.Uninterruptible && m_spell.CastTime > 0 && m_caster is GamePlayer)//Bodygard will break if hit by negative spells
            {
                if (Caster.InterruptTime > 0 && Caster.InterruptTime > m_started)
                {
                    if (!quiet)
                    {
                        if (Caster.LastInterruptMessage != "") MessageToCaster(Caster.LastInterruptMessage, eChatType.CT_SpellResisted);
                        else

                         if (m_caster is GamePlayer)
                        {
                            (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YouAreInterruptedAndMustWait", ((Caster.InterruptTime - m_started) / 1000 + 1).ToString()), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            //MessageToCaster("You are interrupted and must wait " + ((Caster.InterruptTime - m_started) / 1000 + 1).ToString() + " seconds to cast a spell!", eChatType.CT_SpellResisted);
                        }

                    }
                    return false;
                }
            }

            if (m_caster.ObjectState != GameLiving.eObjectState.Active)
            {
                return false;
            }

            if (!m_caster.IsAlive)
            {
                if (!quiet)
                    ((GamePlayer)m_caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client.Account.Language, "SpellHandler.YouAreDeatCantCast"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                //MessageToCaster("You are dead and can't cast!", eChatType.CT_System);
                return false;
            }

            if (m_spell.InstrumentRequirement != 0)
            {
                if (!CheckInstrument())
                {
                    if (!quiet)
                        ((GamePlayer)m_caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client.Account.Language, "SpellHandler.NotRightTypeOfInstrument"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    //MessageToCaster("You are not wielding the right type of instrument!", eChatType.CT_SpellResisted);
                    return false;
                }
            }
            else if (m_caster.IsSitting) // songs can be played if sitting
            {
                //Purge can be cast while sitting but only if player has negative effect that
                //don't allow standing up (like stun or mez)
                if (!quiet)
                    if (m_caster is GamePlayer)
                    {
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YouCantCastWhileSitting"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        //MessageToCaster("You can't cast while sitting!", eChatType.CT_SpellResisted);
                    }
                return false;
            }

            if (m_spell.Target.ToLower() == "area")
            {
                if (!m_caster.IsWithinRadius(m_caster.GroundTarget, CalculateSpellRange()))
                {
                    if (!quiet)
                        if (m_caster is GamePlayer)
                        {
                            (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.SelectACloserGroundTarget"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            //MessageToCaster("Your area target is out of range.  Select a closer target.", eChatType.CT_SpellResisted);
                        }
                    return false;
                }
            }
            else if (m_spell.Target.ToLower() != "self" && m_spell.Target.ToLower() != "group" && m_spell.Target.ToLower() != "cone" && m_spell.Range > 0)
            {
                if (m_spell.Target.ToLower() != "pet")
                {
                    //all other spells that need a target
                    if (target == null || target.ObjectState != GameObject.eObjectState.Active)
                    {
                        if (Caster is GamePlayer && !quiet)
                        {

                            (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.SelectATargetForThisSpell", Spell.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            //MessageToCaster("You must select a target for this spell!", eChatType.CT_SpellResisted);
                        }
                        return false;
                    }

                    if (Caster is GamePlayer && !m_caster.IsWithinRadius(target, CalculateSpellRange()))
                    {
                        if (!quiet)
                            (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourTargetIsToFarAway"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        //MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
                        return false;
                    }
                }

                switch (m_spell.Target.ToLower())
                {
                    case "enemy":
                        //enemys have to be in front and in view for targeted spells
                        if (Caster is GamePlayer && Caster.IsCastingMesmerizeSpellWithPulse(Spell, (Caster as GamePlayer)) == false && !m_caster.IsObjectInFront(target, 175) && !Caster.IsWithinRadius(target, 50))
                        {
                            if (!quiet)
                            {

                                //log.Error("euer ziel ist nicht in sicht2");
                                // MessageToCaster("Your target is not in view. The spell fails.", eChatType.CT_SpellResisted);
                                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.TargetNotInViewSpellFails"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                Caster.Notify(GameLivingEvent.CastFinished, new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetNotInView));
                                Caster.TempProperties.setProperty(AnimStatus, true);
                                return false;
                            }
                        }
                        //log.Error("Remove propertie3");
                        Caster.TempProperties.removeProperty(AnimStatus);
                        /*
                        //Minstrel  Mesmerize need no target at start of the Spell
                        if (Caster is GamePlayer && CastingCompleteEvent != null && m_spell.SpellType == "Mesmerize" && ((Caster as GamePlayer).CharacterClass).ID == (int)eCharacterClass.Minstrel == true && !m_caster.IsObjectInFront(target, 180) && !Caster.IsWithinRadius(target, 50))
                        {
                            if (!quiet)

                                Caster.Notify(GameLivingEvent.CastFinished, new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetNotInView));

                            //MessageToCaster("Your target is not in view. The spell fails.", eChatType.CT_SpellResisted);
                            (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.TargetNotInViewSpellFails"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            return false;
                        }
                        //Minstrel  Mesmerize need no target at start of the Spell
                        if (Caster is GamePlayer && m_spell.SpellType == "Mesmerize" == false && ((Caster as GamePlayer).CharacterClass).ID == (int)eCharacterClass.Minstrel == true && !m_caster.IsObjectInFront(target, 180) && !Caster.IsWithinRadius(target, 50))
                        {
                            if (!quiet)
                                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.TargetNotInViewSpellFails"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            //MessageToCaster("Your target is not in view. The spell fails.", eChatType.CT_SpellResisted);
                            return false;
                        }
                        */
                        if (ServerProperties.Properties.CHECK_LOS_DURING_CAST)
                        {
                            GamePlayer playerChecker = null;

                            if (target is GamePlayer)
                            {
                                playerChecker = target as GamePlayer;
                            }
                            else if (Caster is GamePlayer)
                            {
                                playerChecker = Caster as GamePlayer;
                            }
                            else if (Caster is GameNPC && (Caster as GameNPC).Brain != null && (Caster as GameNPC).Brain is IControlledBrain)
                            {
                                playerChecker = ((Caster as GameNPC).Brain as IControlledBrain).GetPlayerOwner();
                            }

                            if (playerChecker != null)
                            {
                                // If the area forces an LoS check then we do it, otherwise we only check
                                // if caster or target is a player
                                // This will generate an interrupt if LOS check fails
                                /*
                                if (Caster is GamePlayer && IsThereKeepComponents(Caster) == false)
                                {
                                    playerChecker.Out.SendCheckLOS(Caster, target, new CheckLOSResponse(CheckLOSPlayerToTarget));
                                }
                                */
                                if (Caster is GameNPC && target is GamePlayer || Caster is GameNPC && MustCheckLOS(Caster))
                                {
                                    playerChecker.Out.SendCheckLOS(Caster, target, new CheckLOSResponse(CheckLOSNPCToTarget));
                                }
                            }
                        }

                        if (!GameServer.ServerRules.IsAllowedToAttack(Caster, target, quiet))
                        {
                            return false;
                        }
                        break;

                    case "corpse":
                        if (target.IsAlive || !GameServer.ServerRules.IsSameRealm(Caster, target, quiet))
                        {
                            if (!quiet)
                                if (Caster is GamePlayer)
                                {
                                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.OnlyWorksOnDeadRealmMembers"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                    //MessageToCaster("This spell only works on dead members of your realm!",eChatType.CT_SpellResisted);
                                }
                            return false;
                        }
                        break;

                    case "realm":
                        if (GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
                        {
                            return false;
                        }
                        break;

                    case "pet":
                        /*
                         * Can cast pet spell on all Pet/Turret/Minion (our pet)
                         * -If caster target's isn't own pet.
                         *  -check if caster have controlled pet, select this automatically
                         *  -check if target isn't null
                         * -check if target isn't too far away
                         * If all checks isn't true, return false.
                         */
                        if (target == null || !Caster.IsControlledNPC(target as GameNPC))
                        {
                            if (Caster.ControlledBrain != null && Caster.ControlledBrain.Body != null)
                            {
                                target = Caster.ControlledBrain.Body;
                            }
                            else
                            {
                                if (!quiet)
                                    if (Caster is GamePlayer)
                                    {
                                        (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YouCanThisOnlyOnSelfControllingCreatures"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                        //MessageToCaster("You must cast this spell on a creature you are controlling.", eChatType.CT_System);
                                    }
                                return false;
                            }
                        }
                        //Now check distance for own pet
                        if (!m_caster.IsWithinRadius(target, CalculateSpellRange()))
                        {
                            if (!quiet)
                                if (m_caster is GamePlayer)
                                {
                                    (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YourTargetIsToFarAway"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                    // MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
                                }
                            return false;
                        }
                        break;
                }
            }

            if (m_caster.Mana <= 0 && Spell.Power != 0 && Spell.SpellType != "Archery")
            {
                if (!quiet)
                    if (m_caster is GamePlayer)
                    {
                        //MessageToCaster("You have exhausted all of your power and cannot cast spells!", eChatType.CT_SpellResisted);
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YouHaveExhaustedAllMana"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                return false;
            }
            if (Spell.Power != 0 && m_caster.Mana < PowerCost(target) && Spell.SpellType != "Archery")
            {
                if (!quiet)
                    //MessageToCaster("You don't have enough power to cast that!", eChatType.CT_SpellResisted);
                    if (m_caster is GamePlayer)
                    {
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YouDontHaveEnoughMana"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                return false;
            }

            if (m_caster is GamePlayer && m_spell.Concentration > 0 && m_caster.Concentration < m_spell.Concentration)
            {
                if (!quiet)
                    if (m_caster is GamePlayer)
                    {
                        //MessageToCaster("This spell requires " + m_spell.Concentration + " concentration points to cast!", eChatType.CT_SpellResisted);
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.ThisSpellRequiresConcentrationPoints", m_spell.Concentration.ToString()), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                return false;
            }

            if (m_caster is GamePlayer && m_spell.Concentration > 0 && m_caster.ConcentrationEffects.ConcSpellsCount >= 50)
            {
                if (!quiet)
                    if (m_caster is GamePlayer)
                    {
                        //MessageToCaster("You can only cast up to 50 simultaneous concentration spells!", eChatType.CT_SpellResisted);
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YouCanOnlyCastUpTo"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                return false;
            }

            return true;
        }

        public virtual bool CheckAfterCast(GameLiving target)
        {
            return CheckAfterCast(target, false);
        }


        public virtual bool CheckAfterCast(GameLiving target, bool quiet)
        {
            if (m_interrupted)
            {
                return false;
            }

            if (Stage < 2 && !m_spell.Uninterruptible && m_spell.CastTime > 0 && m_caster is GamePlayer &&
                m_caster.EffectList.GetOfType<QuickCastEffect>() == null && AttackerIsGreyConToCaster == false && m_caster.EffectList.GetOfType<MasteryofConcentrationEffect>() == null
                 || Stage < 2 && m_caster.EffectList.GetOfType<QuickCastEffect>() == null && AttackerIsGreyConToCaster == false && m_caster.EffectList.GetOfType<MasteryofConcentrationEffect>() == null && !m_spell.Uninterruptible && m_spell.CastTime > 0 && m_caster is GamePlayer)//Bodygard will break if hit by negative spells
            {
                if (Caster.InterruptTime > 0 && Caster.InterruptTime > m_started)
                {
                    if (!quiet)
                    {
                        if (Caster.LastInterruptMessage != "") MessageToCaster(Caster.LastInterruptMessage, eChatType.CT_SpellResisted);
                        else

                        if (Caster is GamePlayer)
                        {
                            (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YouAreInterruptedAndMustWait", ((Caster.InterruptTime - m_started) / 1000 + 1)), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        }

                        //MessageToCaster("You are interrupted and must wait " + ((Caster.InterruptTime - m_started) / 1000 + 1).ToString() + " seconds to cast a spell!", eChatType.CT_SpellResisted);
                    }
                    Caster.InterruptAction = Caster.CurrentRegion.Time - Caster.SpellInterruptRecastAgain;
                    return false;
                }
            }

            if (m_caster.ObjectState != GameLiving.eObjectState.Active)
            {
                return false;
            }

            if (!m_caster.IsAlive)
            {
                if (!quiet)
                    ((GamePlayer)m_caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client.Account.Language, "SpellHandler.YouAreDeatCantCast"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                //MessageToCaster("You are dead and can't cast!", eChatType.CT_System);
                return false;
            }

            if (m_spell.InstrumentRequirement != 0)
            {
                if (!CheckInstrument())
                {
                    if (!quiet)
                        ((GamePlayer)m_caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client.Account.Language, "SpellHandler.NotRightTypeOfInstrument"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    //MessageToCaster("You are not wielding the right type of instrument!", eChatType.CT_SpellResisted);
                    return false;
                }
            }
            else if (m_caster.IsSitting) // songs can be played if sitting
            {
                //Purge can be cast while sitting but only if player has negative effect that
                //don't allow standing up (like stun or mez)
                if (!quiet)
                    //MessageToCaster("You can't cast while sitting!", eChatType.CT_SpellResisted);
                    if (m_caster is GamePlayer)
                    {
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YouCantCastWhileSitting"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                return false;
            }

            if (m_spell.Target.ToLower() == "area")
            {
                if (!m_caster.IsWithinRadius(m_caster.GroundTarget, CalculateSpellRange()))
                {
                    if (!quiet)
                        if (m_caster is GamePlayer)
                        {
                            (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.SelectACloserGroundTarget"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            //MessageToCaster("Your area target is out of range.  Select a closer target.", eChatType.CT_SpellResisted);
                        }
                    return false;
                }

                if (ServerProperties.Properties.SPELL_GTAE_NEED_LOS)
                {
                    if (!Caster.GroundTargetInView)
                    {
                        if (Caster is GamePlayer)
                        {
                            // MessageToCaster("Your ground target is not in view!", eChatType.CT_SpellResisted);
                            (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourGroundTargetIsNotInView"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        }
                        return false;
                    }
                }
            }
            else if (m_spell.Target.ToLower() != "self" && m_spell.Target.ToLower() != "group" && m_spell.Target.ToLower() != "cone" && m_spell.Range > 0)
            {
                if (m_spell.Target.ToLower() != "pet")
                {
                    //all other spells that need a target
                    if (target == null || target.ObjectState != GameObject.eObjectState.Active)
                    {
                        if (Caster is GamePlayer && !quiet)
                        {

                           


                            (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.SelectATargetForThisSpell", Spell.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            // MessageToCaster("You must select a target for this spell!", eChatType.CT_SpellResisted);
                        }
                        return false;
                    }

                    if (Caster is GamePlayer && !m_caster.IsWithinRadius(target, CalculateSpellRange()))
                    {
                        
                        if (!quiet)
                            //MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
                            (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourTargetIsToFarAway"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        return false;
                    }
                }

                switch (m_spell.Target)
                {
                    case "Enemy":
                        //enemys have to be in front and in view for targeted spells
                        if (Caster is GamePlayer && ((Caster as GamePlayer).CharacterClass).ID == (int)eCharacterClass.Minstrel == false && !m_caster.IsObjectInFront(target, 180) && !Caster.IsWithinRadius(target, 50))
                        {
                            if (!quiet)

                                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.TargetNotInViewSpellFails"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            // MessageToCaster("Your target is not in view. The spell fails.", eChatType.CT_SpellResisted);
                            return false;
                        }
                        //log.ErrorFormat("SpellType: = {0}, pulse: {1}, instrument: {2}", Spell.SpellType, Spell.Pulse, Spell.InstrumentRequirement);
                        if (Caster is GamePlayer && Caster.IsCastingMesmerizeSpellWithPulse(Spell, (Caster as GamePlayer)) == false && ((Caster as GamePlayer).CharacterClass).ID == (int)eCharacterClass.Minstrel == true && !m_caster.IsObjectInFront(target, 180) && !Caster.IsWithinRadius(target, 50))
                        {
                            if (!quiet)
                                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.TargetNotInViewSpellFails"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            //MessageToCaster("Your target is not in view. The spell fails.", eChatType.CT_SpellResisted);
                            return false;
                        }

                        if (!GameServer.ServerRules.IsAllowedToAttack(Caster, target, quiet))
                        {
                            return false;
                        }
                        break;

                    case "Corpse":
                        if (target.IsAlive || !GameServer.ServerRules.IsSameRealm(Caster, target, quiet))
                        {
                            if (!quiet)
                                if (Caster is GamePlayer)
                                {
                                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.OnlyWorksOnDeadRealmMembers"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                    //MessageToCaster("This spell only works on dead members of your realm!", eChatType.CT_SpellResisted);
                                }
                            return false;
                        }
                        break;

                    case "Realm":
                        if (GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
                        {
                            return false;
                        }
                        break;

                    case "Pet":
                        /*
                         * [Ganrod] Nidel: Can cast pet spell on all Pet/Turret/Minion (our pet)
                         * -If caster target's isn't own pet.
                         *  -check if caster have controlled pet, select this automatically
                         *  -check if target isn't null
                         * -check if target isn't too far away
                         * If all checks isn't true, return false.
                         */
                        if (target == null || !Caster.IsControlledNPC(target as GameNPC))
                        {
                            if (Caster.ControlledBrain != null && Caster.ControlledBrain.Body != null)
                            {
                                target = Caster.ControlledBrain.Body;
                            }
                            else
                            {
                                if (!quiet)

                                    if (Caster is GamePlayer)
                                    {
                                        (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YouCanThisOnlyOnSelfControllingCreatures"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                        //MessageToCaster("You must cast this spell on a creature you are controlling.", eChatType.CT_System);
                                    }
                                return false;
                            }
                        }
                        //Now check distance for own pet
                        if (!m_caster.IsWithinRadius(target, CalculateSpellRange()))
                        {

                            if (!quiet)

                                if (m_caster is GamePlayer)
                                {
                                    (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YourTargetIsToFarAway"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                    // MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
                                }
                            return false;
                        }
                        break;
                }
            }

            if (m_caster.Mana <= 0 && Spell.Power != 0 && Spell.SpellType != "Archery")
            {
                if (!quiet)
                    if (m_caster is GamePlayer)
                    {
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YouHaveExhaustedAllMana"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        //MessageToCaster("You have exhausted all of your power and cannot cast spells!", eChatType.CT_SpellResisted);
                    }
                return false;
            }
            if (Spell.Power != 0 && m_caster.Mana < PowerCost(target) && Spell.SpellType != "Archery")
            {
                if (!quiet)
                    if (m_caster is GamePlayer)
                    {
                        //MessageToCaster("You don't have enough power to cast that!", eChatType.CT_SpellResisted);
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YouDontHaveEnoughMana"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                return false;
            }

            if (m_caster is GamePlayer && m_spell.Concentration > 0 && m_caster.Concentration < m_spell.Concentration)
            {
                if (!quiet)
                    if (m_caster is GamePlayer)
                    {
                        //MessageToCaster("This spell requires " + m_spell.Concentration + " concentration points to cast!", eChatType.CT_SpellResisted);
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.ThisSpellRequiresConcentrationPoints", m_spell.Concentration.ToString()), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                return false;
            }

            if (m_caster is GamePlayer && m_spell.Concentration > 0 && m_caster.ConcentrationEffects.ConcSpellsCount >= 50)
            {
                if (!quiet)
                    if (m_caster is GamePlayer)
                    {
                        //MessageToCaster("You can only cast up to 50 simultaneous concentration spells!", eChatType.CT_SpellResisted);
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YouCanOnlyCastUpTo"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                return false;
            }

            return true;
        }


        #endregion

        /// <summary>
        /// Calculates the power to cast the spell
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual int PowerCost(GameLiving target)
        {
            // warlock
            GameSpellEffect effect = SpellHandler.FindEffectOnTarget(m_caster, "Powerless");
            if (effect != null && !m_spell.IsPrimary)
                return 0;

            //1.108 - Valhallas Blessing now has a 75% chance to not use power.
            ValhallasBlessingEffect ValhallasBlessing = m_caster.EffectList.GetOfType<ValhallasBlessingEffect>();
            if (ValhallasBlessing != null && Util.Chance(75))
                return 0;

            //patch 1.108 increases the chance to not use power to 50%.
            FungalUnionEffect FungalUnion = m_caster.EffectList.GetOfType<FungalUnionEffect>();
            {
                if (FungalUnion != null && Util.Chance(50))
                    return 0;
            }

            // Arcane Syphon chance
            int syphon = Caster.GetModified(eProperty.ArcaneSyphon);
            if (syphon > 0)
            {
                if (Util.Chance(syphon))
                {
                    return 0;
                }
            }

            double basepower = m_spell.Power; //<== defined a basevar first then modified this base-var to tell %-costs from absolut-costs

            // percent of maxPower if less than zero
            if (basepower < 0)
            {
                if (Caster is GamePlayer && ((GamePlayer)Caster).CharacterClass.ManaStat != eStat.UNDEFINED)
                {
                    GamePlayer player = Caster as GamePlayer;
                    basepower = player.CalculateMaxMana(player.Level, player.GetBaseStat(player.CharacterClass.ManaStat)) * basepower * -0.01;
                }
                else
                {
                    basepower = Caster.MaxMana * basepower * -0.01;
                }
            }

            double power = basepower * 1.2; //<==NOW holding basepower*1.2 within 'power'

            eProperty focusProp = SkillBase.SpecToFocus(SpellLine.Spec);
            if (focusProp != eProperty.Undefined)
            {
                double focusBonus = Caster.GetModified(focusProp) * 0.4;
                if (Spell.Level > 0)
                    focusBonus /= Spell.Level;
                if (focusBonus > 0.4)
                    focusBonus = 0.4;
                else if (focusBonus < 0)
                    focusBonus = 0;
                power -= basepower * focusBonus; //<== So i can finally use 'basepower' for both calculations: % and absolut
            }
            else if (Caster is GamePlayer && ((GamePlayer)Caster).CharacterClass.ClassType == eClassType.Hybrid)
            {
                double specBonus = 0;
                if (Spell.Level != 0) specBonus = (((GamePlayer)Caster).GetBaseSpecLevel(SpellLine.Spec) * 0.4 / Spell.Level);

                if (specBonus > 0.4)
                    specBonus = 0.4;
                else if (specBonus < 0)
                    specBonus = 0;
                power -= basepower * specBonus;
            }
            // doubled power usage if quickcasting
            if (Caster.EffectList.GetOfType<QuickCastEffect>() != null && Spell.CastTime > 0)
                power *= 2;
            return (int)power;
        }

        /// <summary>
        /// Calculates the enduance cost of the spell
        /// </summary>
        /// <returns></returns>
        public virtual int CalculateEnduranceCost()
        {
            return 5;
        }



        /// <summary>
        /// Calculates the range to target needed to cast the spell
        /// </summary>
        /// <returns></returns>
        public virtual int CalculateSpellRange()
        {

            int allowedRange = 0;
            int _spellRange = -100;
            int _archeryrange = -100;
            int _gesRange = 0;

            if (Caster is GamePlayer && Spell != null && Spell.SpellType == "Archery")
            {


                if (Spell.SpellType == "Archery" && Caster is GamePlayer && Properties.Allow_New_Archery_Check_Arrows)
                {

                    if (Spell.LifeDrainReturn == 3 || Spell.Range == 2500) //Point blank and Long Shots
                    {
                        allowedRange = Spell.Range;
                    }
                    else
                        allowedRange = ArrowRange;

                    
                    //log.ErrorFormat("Arrow range: {0}", allowedRange);
                }
                else
                {
                    //int currentArrowRange = MaxArrowRange(null));
                    switch ((Caster as GamePlayer).CharacterClass.ID)
                    {
                        case 3:
                            {

                                //log.ErrorFormat("Longbow range: {0}", Spell.Range);
                                allowedRange = Spell.Range;
                                break;
                            }
                        case 25:
                            {
                                //log.ErrorFormat("CompositeBow range: {0}", Spell.Range);
                                allowedRange = Spell.Range;
                                break;
                            }
                        case 50:
                            {

                               //log.ErrorFormat("RecurvedBow range: {0}", Spell.Range);
                                allowedRange = Spell.Range;
                                break;
                            }
                        default: allowedRange = Spell.Range; break;
                    }
                }
            


               
                _archeryrange += Caster.GetModified(eProperty.ArcheryRange);
                _spellRange += Caster.GetModified(eProperty.SpellRange);
               // log.ErrorFormat("_archeryrange = {0}", _archeryrange);
               // log.ErrorFormat("_spellRange = {0}", _spellRange);


                _gesRange = _archeryrange + _spellRange;

                if (_gesRange < 0)
                {
                    _gesRange = 0;
                }

                if (_gesRange > 10)
                {
                    _gesRange = 10;
                }


                allowedRange += allowedRange * _gesRange / 100;
               

                //log.ErrorFormat("allowedMaxRange = {0}", allowedRange);


                return allowedRange;
            }
            else
            {
                int range = Math.Max(32, (int)(Spell.Range * Caster.GetModified(eProperty.SpellRange) * 0.01));
                return range;
            }
            //Dinberg: add for warlock range primer
        }

        public virtual void PauseCasting()
        {
            if (m_paused || !IsCasting)
                return;

            m_paused = true;

            if (IsCasting)
            {
                foreach (GamePlayer player in m_caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    player.Out.SendInterruptAnimation(m_caster);
                }
            }
        }

        /// <summary>
        /// Called whenever the casters casting sequence is to interrupt immediately
        /// </summary>
        public virtual void InterruptCasting()
        {
            if (m_interrupted || !IsCasting)
                return;

            m_interrupted = true;

            if (IsCasting)
            {
                foreach (GamePlayer player in m_caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    player.Out.SendInterruptAnimation(m_caster);
                }
            }

            if (m_castTimer != null)
            {
                m_castTimer.Stop();
                m_castTimer = null;

                if (m_caster is GamePlayer)
                {
                    ((GamePlayer)m_caster).ClearSpellQueue();
                }
            }

            m_startReuseTimer = false;
            OnAfterSpellCastSequence();
        }

        /// <summary>
        /// Casts a spell after the CastTime delay
        /// </summary>
        protected class DelayedCastTimer : GameTimer
        {
            /// <summary>
            /// The spellhandler instance with callbacks
            /// </summary>
            private readonly SpellHandler m_handler;
            /// <summary>
            /// The target object at the moment of CastSpell call
            /// </summary>
            private readonly GameLiving m_target;
            private readonly GameLiving m_caster;
            private byte m_stage;
            private readonly int m_delay1;
            private readonly int m_delay2;

            /// <summary>
            /// Constructs a new DelayedSpellTimer
            /// </summary>
            /// <param name="actionSource">The caster</param>
            /// <param name="handler">The spell handler</param>
            /// <param name="target">The target object</param>
            public DelayedCastTimer(GameLiving actionSource, SpellHandler handler, GameLiving target, int delay1, int delay2)
                : base(actionSource.CurrentRegion.TimeManager)
            {
                if (handler == null)
                    throw new ArgumentNullException("handler");

                if (actionSource == null)
                    throw new ArgumentNullException("actionSource");

                m_handler = handler;
                m_target = target;
                m_caster = actionSource;
                m_stage = 0;
                m_delay1 = delay1;
                m_delay2 = delay2;
            }

            /// <summary>
            /// Called on every timer tick
            /// </summary>
            protected override void OnTick()
            {
                try
                {
                    //GameNPC spell paused:
                    if (m_handler.m_paused)
                    {
                        if (m_caster.IsIncapacitated)
                        {
                            Interval = 100;
                            return;
                        }
                        m_handler.m_paused = false;

                        ushort remain = 0;
                        if (m_stage < 2) remain += (ushort)m_delay2;
                        if (m_stage < 1) remain += (ushort)m_delay1;
                        if (remain > 0)
                        {
                            foreach (GamePlayer player in m_caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                            {
                                if (player == null)
                                    continue;
                                player.Out.SendSpellCastAnimation(m_caster, m_handler.Spell.ClientEffect, remain);
                            }
                        }
                    }



                    if (m_stage == 0)
                    {
                        //log.ErrorFormat(" m_stage0 = {0}", m_stage);
                        if (!m_handler.CheckAfterCast(m_target))
                        {
                            Interval = 0;
                            m_handler.InterruptCasting();
                            m_handler.OnAfterSpellCastSequence();
                            return;
                        }
                        m_stage = 1;

                        m_handler.Stage = 1;
                        Interval = m_delay1;
                        //log.ErrorFormat("Interval1 = {0}", m_delay1);
                    }
                    else if (m_stage == 1)
                    {
                        //log.ErrorFormat(" m_stage1 = {0}", m_stage);
                        if (!m_handler.CheckDuringCast(m_target))
                        {
                            Interval = 0;
                            m_handler.InterruptCasting();
                            m_handler.OnAfterSpellCastSequence();
                            return;
                        }
                        m_stage = 2;

                        m_handler.Stage = 2;
                        Interval = m_delay2;
                        //log.ErrorFormat("Interva2 = {0}", m_delay2);
                    }
                    else if (m_stage == 2)
                    {
                        //log.ErrorFormat(" m_stage2 = {0}", m_stage);
                        m_stage = 3;

                        m_handler.Stage = 3;
                        Interval = 100;
                        //log.ErrorFormat("Interval3 = {0}", Interval);
                        if (m_handler.CheckEndCast(m_target))
                        {
                            m_handler.FinishSpellCast(m_target);
                        }
                    }
                    else
                    {
                        //log.ErrorFormat(" m_stage3 = {0}", m_stage);

                        m_stage = 4;
                        m_handler.Stage = 4;
                        Interval = 0;
                        //log.ErrorFormat("Interval4 = {0}", Interval);
                        m_handler.OnAfterSpellCastSequence();
                    }

                    if (m_caster is GamePlayer && ServerProperties.Properties.ENABLE_DEBUG && m_stage < 3)
                    {
                        (m_caster as GamePlayer).Out.SendMessage("[DEBUG] step = " + (m_handler.Stage + 1), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }

                    return;
                }
                catch (Exception e)
                {
                    if (log.IsErrorEnabled)
                        log.Error(ToString(), e);
                }

                m_handler.OnAfterSpellCastSequence();
                Interval = 0;
            }

            /// <summary>
            /// Returns short information about the timer
            /// </summary>
            /// <returns>Short info about the timer</returns>
            public override string ToString()
            {
                return new StringBuilder(base.ToString(), 128)
                    .Append(" spellhandler: (").Append(m_handler.ToString()).Append(')')
                    .ToString();
            }
        }

        /// <summary>
        /// Calculates the effective casting time
        /// </summary>
        /// <returns>effective casting time in milliseconds</returns>
        public virtual int CalculateCastingTime()
        {
            return m_caster.CalculateCastingTime(m_spellLine, m_spell);
        }


        #region animations

        /// <summary>
        /// Sends the cast animation
        /// </summary>
        public virtual void SendCastAnimation()
        {
            ushort castTime = (ushort)(CalculateCastingTime() / 100);
            SendCastAnimation(castTime);

        }

        /// <summary>
        /// Sends the cast animation
        /// </summary>
        /// <param name="castTime">The cast time</param>
        public virtual void SendCastAnimation(ushort castTime)
        {
            foreach (GamePlayer player in m_caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                if (player == null)
                    continue;
                player.Out.SendSpellCastAnimation(m_caster, m_spell.ClientEffect, castTime);

            }
        }

        /// <summary>
        /// Send the Effect Animation
        /// </summary>
        /// <param name="target">The target object</param>
        /// <param name="boltDuration">The duration of a bolt</param>
        /// <param name="noSound">sound?</param>
        /// <param name="success">spell success?</param>
        public virtual void SendEffectAnimation(GameObject target, ushort boltDuration, bool noSound, byte success)
        {
            if (target == null)
                target = m_caster;
           
            foreach (GamePlayer player in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                player.Out.SendSpellEffectAnimation(m_caster, target, m_spell.ClientEffect, boltDuration, noSound, success);

            }
        }

        /// <summary>
        /// Send the Interrupt Cast Animation
        /// </summary>
        public virtual void SendInterruptCastAnimation()
        {
            foreach (GamePlayer player in m_caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                player.Out.SendInterruptAnimation(m_caster);
            }
        }
        public virtual void SendEffectAnimation(GameObject target, ushort clientEffect, ushort boltDuration, bool noSound, byte success)
        {
            if (target == null)
                target = m_caster;
           

            foreach (GamePlayer player in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
               
                player.Out.SendSpellEffectAnimation(m_caster, target, clientEffect, boltDuration, noSound, success);
            }
        }
        #endregion

        /// <summary>
        /// called after normal spell cast is completed and effect has to be started
        /// </summary>
        public virtual void FinishSpellCast(GameLiving target)
        {

            if (Caster is GamePlayer && ((GamePlayer)Caster).IsOnHorse && !HasPositiveEffect)
                ((GamePlayer)Caster).IsOnHorse = false;

            //[Stryve]: Do not break stealth if spell never breaks stealth.
            if ((Caster is GamePlayer) && UnstealthCasterOnFinish)
                ((GamePlayer)Caster).Stealth(false);

            if (Caster is GamePlayer && !HasPositiveEffect)
            {
                if (Caster.AttackWeapon != null && Caster.AttackWeapon is GameInventoryItem)
                {
                    (Caster.AttackWeapon as GameInventoryItem).OnSpellCast(Caster, target, Spell);
                }
            }

            // messages
            if (Caster is GamePlayer && Spell.InstrumentRequirement == 0 && Spell.ClientEffect != 0 && Spell.CastTime > 0)
            {
                GamePlayer player = Caster as GamePlayer;
                {

                    if (Spell.SpellType == "Archery" && Caster is GamePlayer && Properties.Allow_New_Archery_Check_Arrows)
                    {
                        //Remove Arrows
                        InventoryItem ammo = RangeAttackAmmo;
                        Caster.Inventory.RemoveCountFromStack(ammo, 1);
                    }

                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "SpellHandler.SendCastMessage.YouCast", Spell.Name), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }
                //MessageToCaster("You cast a " + m_spell.Name + " spell!", eChatType.CT_Spell);
                foreach (GamePlayer players in m_caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
                {
                    if (players != m_caster)
                    {
                        ((GamePlayer)m_caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)m_caster).Client.Account.Language, "SpellHandler.SendCastMessage.OtherCast", m_caster.Name), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                        //MessageFromArea != "") (m_caster, m_caster.GetName(0, true) + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    }
                }
            }

           
            //Charmed pets with pulsing spell
            if (m_spell.Pulse != 0 && m_spell.Frequency > 0 && m_spell.SpellType == "Charm")
            {
                CancelPulsingSpell(Caster, m_spell.SpellType);
                //CancelPetCharmPulsingSpells(Caster);
                PulsingSpellEffect pulseeffect = new PulsingSpellEffect(this);
                pulseeffect.Start();
                // show animation on caster for positive spells, negative shows on every StartSpell
                if (m_spell.Target == "Self" || m_spell.Target == "Group")
                    SendEffectAnimation(Caster, 0, false, 1);
                if (m_spell.Target == "Pet")
                    SendEffectAnimation(target, 0, false, 1);
            }

            if (m_spell.Pulse != 0 && m_spell.InstrumentRequirement != 0 && !CheckInstrument())
            {
                if (Caster is GamePlayer)
                {
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CCSpellHandler.StopPlayingSong"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }
                SendEffectAnimation(target, 0, false, 0);
                target.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);

                // log.Error("caster hat kein instrument akztive");

                return;
            }



            if (m_spellTarget != null && Caster is GamePlayer && (FindImmunityOfSpellTypeOnTarget(m_spellTarget, Spell.SpellType) != null || target.HasAbility(Abilities.CCImmunity)) && m_spell.Pulse != 0 && Spell.InstrumentRequirement != 0)
            {
                if (Caster is GamePlayer)
                {
                    // MessageToCaster(target.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CCSpellHandler.TargetIsImmuneToThisEffect", target.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    //log.Error("hier 2");
                }
                SendEffectAnimation(target, 0, false, 0);
                target.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CCSpellHandler.StopPlayingSong"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);


                return;
            }

            if (m_spellTarget != null && Caster is GamePlayer && m_spell.Pulse != 0 && m_spell.Frequency > 0 && FindEffectOnTarget(m_spellTarget, Spell.SpellType) != null && m_spell.Pulse != 0 && Spell.InstrumentRequirement != 0)
            {
                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CCSpellHandler.YourTargetCantHaveThisEffectAgainYet", m_spellTarget.Name), eChatType.CT_SpellPulse, eChatLoc.CL_SystemWindow);


                SendEffectAnimation(target, 0, false, 0);
                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CCSpellHandler.StopPlayingSong"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                return;
            }

            //missing animation für enemys hinzugefügt =  m_spell.Target == "Enemy"
            if (m_spell.Pulse != 0 && m_spell.Frequency > 0)
            {
                CancelAllPulsingSpells(Caster);
                PulsingSpellEffect pulseeffect = new PulsingSpellEffect(this);
                pulseeffect.Start();
                // show animation on caster for positive spells, negative shows on every StartSpell
                if (m_spell.Target == "Self" || m_spell.Target == "Group")
                    SendEffectAnimation(Caster, 0, false, 1);
                if (m_spell.Target == "Pet")// || m_spell.Target == "Realm" || m_spell.Target == "Cone")
                    SendEffectAnimation(target, 0, false, 1);
            }

            /*
            //Minstrel  Mesmerize need a valid target at start of the Spell
            if (m_spell.Target == "Enemy" && m_spell.SpellType == "Mesmerize" && target == null || target != Caster)
            {
                log.Error("der zauber schägt fehl6");
                //MessageToCaster("Your target is not in view. The spell fails.", eChatType.CT_SpellResisted);
                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.TargetNotInViewSpellFails"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return;
            }
           */

            //Healreg are lower if we heal a target that is in combat.
            if (Caster is GamePlayer && target != null && (Spell.SpellType == "Heal" || Spell.SpellType == "AOEHeal" || Spell.SpellType == "HealOverTime"))
            {
                //log.Error("caster heals a target");
                Caster.IsHealingTarget = target; //Power Regeneration rate reduction

            }

            
            if (target != null && target is GuardLord && Caster is GamePlayer && Spell.Target.ToLower() == "enemy")
            {
                AttackData ad = CalculateDamageToTarget(target, Caster.Effectiveness);

                if (ad != null && (Caster as GamePlayer).IsWithinRadius(ad.Target, 127, true) == false || (Caster as GamePlayer).IsWithinRadius(ad.Attacker, 127, true) == false)
                {
                    //log.Error("Attack missed");
                    ad.AttackResult = GameLiving.eAttackResult.Missed;
                    ad.Damage = 0;
                    (Caster as GamePlayer).Out.SendMessage(ad.Target.Name + " is immune to damage from this range.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                    (Caster as GamePlayer).Out.SendMessage(" You has to be in melee range of the Lord to attack him.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                    
                    GameEventMgr.Notify(GameLivingEvent.CastFailed, m_caster, new CastingEventArgs(this, target, m_lastAttackData));
                    return;

                }
              
            }
            

            //Zusätzliche Los Abfrage vor Spellstart
            if (m_caster.TargetInView == false && Spell.Target.ToLower() == "enemy" && Spell.Radius == 0 && Spell.Range != 0)
            {

                AttackData ad = CalculateDamageToTarget(target, Caster.Effectiveness);

                if (ad != null)
                {
                    //log.Error("Attack missed");
                    ad.AttackResult = GameLiving.eAttackResult.Missed;
                    ad.Damage = 0;
                }
                GameEventMgr.Notify(GameLivingEvent.CastFailed, m_caster, new CastingEventArgs(this, target, m_lastAttackData));
                //log.Error("Cast Failed");
                return;
            }

            StartSpell(target); // and action


            //Dinberg: This is where I moved the warlock part (previously found in gameplayer) to prevent
            //cancelling before the spell was fired.
            if (m_spell.SpellType != "Powerless" && m_spell.SpellType != "Range" && m_spell.SpellType != "Uninterruptable")
            {
                GameSpellEffect effect = SpellHandler.FindEffectOnTarget(m_caster, "Powerless");
                if (effect == null)
                    effect = SpellHandler.FindEffectOnTarget(m_caster, "Range");
                if (effect == null)
                    effect = SpellHandler.FindEffectOnTarget(m_caster, "Uninterruptable");

                //if we found an effect, cancel it!
                if (effect != null)
                    effect.Cancel(false);
            }

            //the quick cast is unallowed whenever you miss the spell
            //set the time when casting to can not quickcast during a minimum time
            if (m_caster is GamePlayer)
            {
                QuickCastEffect quickcast = m_caster.EffectList.GetOfType<QuickCastEffect>();
                if (quickcast != null && Spell.CastTime > 0)
                {
                    m_caster.TempProperties.setProperty(GamePlayer.QUICK_CAST_CHANGE_TICK, m_caster.CurrentRegion.Time);
                    //((GamePlayer)m_caster).DisableSkill(SkillBase.GetAbility(Abilities.Quickcast), QuickCastAbilityHandler.DISABLE_DURATION);
                    quickcast.Cancel(false);
                }
            }


            if (m_ability != null)
                m_caster.DisableSkill(m_ability.Ability, (m_spell.RecastDelay == 0 ? 3 : m_spell.RecastDelay));


            // disable spells with recasttimer (Disables group of same type with same delay)
            if (m_spell.RecastDelay > 0 && m_startReuseTimer)
            {
                if (m_caster is GamePlayer)
                {
                    GamePlayer gp_caster = m_caster as GamePlayer;
                    foreach (SpellLine spellline in gp_caster.GetSpellLines())
                        foreach (Spell sp in SkillBase.GetSpellList(spellline.KeyName))
                            if (sp == m_spell || (sp.SharedTimerGroup != 0 && (sp.SharedTimerGroup == m_spell.SharedTimerGroup)))
                                m_caster.DisableSkill(sp, sp.RecastDelay);
                }
                else if (m_caster is GameNPC)
                {
                    m_caster.DisableSkill(m_spell, m_spell.RecastDelay);
                }


                    //Update Disabled icons
                    (m_caster as GamePlayer).UpdateDisabledSkills();
            }


            GameEventMgr.Notify(GameLivingEvent.CastFinished, m_caster, new CastingEventArgs(this, target, m_lastAttackData));
        }
        public virtual void ChancelPulsingSpell(GamePlayer player, string SpellType)
        {

            lock (player.EffectList)
            {
                foreach (GameSpellEffect effect in player.EffectList)
                {
                    if (effect.SpellHandler.HasPositiveEffect && effect.Spell.SpellType == SpellType)
                    {
                        effect.Cancel(false);
                    }
                }
            }
        }
        /// <summary>
        /// Select all targets for this spell
        /// </summary>
        /// <param name="castTarget"></param>
        /// <returns></returns>
        public virtual IList<GameLiving> SelectTargets(GameObject castTarget)
        {
            int castspellRange = CalculateSpellRange();
            var list = new List<GameLiving>(8);
            GameLiving target = castTarget as GameLiving;
            bool targetchanged = false;
            bool crocoringActive = false;
            string modifiedTarget = Spell.Target.ToLower();
            ushort modifiedRadius = (ushort)Spell.Radius;

            int newtarget = 0;
            GameSpellEffect TargetMod = SpellHandler.FindEffectOnTarget(m_caster, "TargetModifier");

            InventoryItem leftRing = null;
            InventoryItem rightRing = null;

            if (Caster is GamePlayer)
            {
                leftRing = Caster.Inventory.GetItem(eInventorySlot.LeftRing);
                rightRing = Caster.Inventory.GetItem(eInventorySlot.RightRing);
            }

            //Crocodile Tear Ring

            //Caster.EffectList.GetAllOfType<ProtectEffect>() == null Spell.SpellType == "Charm" == false //Caster.EffectList.GetAllOfType<ProtectEffect>() == null
            if (m_spell.SpellType == "Charm" == false && TargetMod != null && (leftRing != null && leftRing.Name == "Crocodile Tear Ring" || rightRing != null && rightRing.Name == "Crocodile Tear Ring"))
            {

                if (modifiedTarget == "group" && target is GameNPC == false) // || modifiedTarget == "group" && target is GameNPC && (target as GameNPC).Brain is IControlledBrain)
                {
                    if (m_spell.Target.ToLower() == "group"
                        && m_spell.Pulse != 0)
                    {
                        modifiedTarget = "realm";
                        modifiedRadius = (ushort)m_spell.Range;
                        crocoringActive = true;
                    }
                }

           
                if (crocoringActive == false && modifiedTarget == "enemy" || modifiedTarget == "realm" || modifiedTarget == "group")
                {
                    newtarget = (int)TargetMod.Spell.Value;

                    switch (newtarget)
                    {
                        case 0: // Apply on heal single
                            if (m_spell.SpellType.ToLower() == "heal" && modifiedTarget == "realm")
                            {
                                modifiedTarget = "group";
                                targetchanged = true;
                            }
                            break;
                        case 1: // Apply on heal group
                            if (m_spell.SpellType.ToLower() == "heal" && modifiedTarget == "group")
                            {
                                modifiedTarget = "realm";
                                modifiedRadius = (ushort)m_spell.Range;
                                targetchanged = true;
                            }
                            break;
                        case 2: // apply on enemy
                            if (modifiedTarget == "enemy")
                            {
                                if (m_spell.Radius == 0)
                                    modifiedRadius = 450;
                                if (m_spell.Radius != 0)
                                    modifiedRadius += 300;
                                targetchanged = true;
                            }
                            break;
                        case 3: // Apply on buff
                            if (m_spell.Target.ToLower() == "group"
                                && m_spell.Pulse != 0)
                            {
                                modifiedTarget = "realm";
                                modifiedRadius = (ushort)m_spell.Range;
                                targetchanged = true;
                            }
                            break;
                    }
                }
                if (targetchanged)
                {
                    if (TargetMod.Duration < 65535 && (ushort)m_spell.Pulse == 0) 
                        TargetMod.Cancel(false);

                  
                }
                               
            }
           

            //if (modifiedTarget == "pet" && Spell.Damage > 0 && Spell.Radius > 0)
            if (modifiedTarget == "pet" && !HasPositiveEffect)
            {
                modifiedTarget = "enemy";
                //[Ganrod] Nidel: can cast TurretPBAoE on selected Pet/Turret
                if (Spell.SpellType.ToLower() != "TurretPBAoE".ToLower())
                {
                    target = Caster.ControlledBrain.Body;
                }
            }

            #region Process the targets
            switch (modifiedTarget)
            {
                #region GTAoE
                // GTAoE
                case "area":
                    //Dinberg - fix for animists turrets, where before a radius of zero meant that no targets were ever
                    //selected!
                    if (Spell.SpellType == "SummonAnimistPet" || Spell.SpellType == "SummonAnimistFnF")// || Spell.SpellType == "SummonTitan")
                    {
                        list.Add(Caster);
                    }
                    else
                        if (modifiedRadius > 0)
                    {
                        foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot(Caster.CurrentRegionID, Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z, modifiedRadius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
                            {
                                // Apply Mentalist RA5L
                                SelectiveBlindnessEffect SelectiveBlindness = Caster.EffectList.GetOfType<SelectiveBlindnessEffect>();
                                if (SelectiveBlindness != null)
                                {
                                    GameLiving EffectOwner = SelectiveBlindness.EffectSource;
                                    if (EffectOwner == player)
                                    {
                                        if (Caster is GamePlayer) ((GamePlayer)Caster).Out.SendMessage(string.Format("{0} is invisible to you!", player.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                                    }
                                    else list.Add(player);
                                }
                                else list.Add(player);
                            }
                        }
                        foreach (GameNPC npc in WorldMgr.GetNPCsCloseToSpot(Caster.CurrentRegionID, Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z, modifiedRadius))
                        {
                            if (npc is GameStorm)
                                list.Add(npc);
                            else if (GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true))
                            {
                                if (!npc.HasAbility("DamageImmunity")) list.Add(npc);
                            }
                        }
                    }
                    break;
                #endregion
                #region Corpse
                case "corpse":
                    if (target != null && !target.IsAlive)
                        list.Add(target);
                    break;
                #endregion
                #region Pet
                case "pet":
                    {
                        //Start-- [Ganrod] Nidel: Can cast Pet spell on our Minion/Turret pet without ControlledNpc
                        // awesome, Pbaoe with target pet spell ?^_^
                        if (modifiedRadius > 0 && Spell.Range == 0)
                        {
                            foreach (GameNPC pet in Caster.GetNPCsInRadius(modifiedRadius))
                            {
                                if (Caster.IsControlledNPC(pet))
                                {
                                    list.Add(pet);
                                }
                            }
                            return list;
                        }
                        if (target == null)
                        {
                            break;
                        }

                        GameNPC petBody = target as GameNPC;
                        // check target
                        if (petBody != null && Caster.IsWithinRadius(petBody, Spell.Range))
                        {
                            if (Caster.IsControlledNPC(petBody))
                            {
                                list.Add(petBody);
                            }
                        }
                        //check controllednpc if target isn't pet (our pet)
                        if (list.Count < 1 && Caster.ControlledBrain != null)
                        {
                            petBody = Caster.ControlledBrain.Body;
                            if (petBody != null && Caster.IsWithinRadius(petBody, Spell.Range))
                            {
                                list.Add(petBody);
                            }
                        }

                        //Single spell buff/heal...
                        if (Spell.Radius == 0)
                        {
                            return list;
                        }
                        //Our buff affects every pet in the area of targetted pet (our pets)
                        if (Spell.Radius > 0 && petBody != null)
                        {
                            foreach (GameNPC pet in petBody.GetNPCsInRadius(modifiedRadius))
                            {
                                //ignore target or our main pet already added
                                if (pet == petBody || !Caster.IsControlledNPC(pet))
                                {
                                    continue;
                                }
                                list.Add(pet);
                            }
                        }
                    }
                    //End-- [Ganrod] Nidel: Can cast Pet spell on our Minion/Turret pet without ControlledNpc
                    break;
                #endregion
                #region Enemy
                case "enemy":
                    if (modifiedRadius > 0)
                    {
                        if (Spell.SpellType.ToLower() != "TurretPBAoE".ToLower() && (target == null || Spell.Range == 0))
                            target = Caster;
                        if (target == null) return null;
                        foreach (GamePlayer player in target.GetPlayersInRadius(modifiedRadius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
                            {
                                SelectiveBlindnessEffect SelectiveBlindness = Caster.EffectList.GetOfType<SelectiveBlindnessEffect>();
                                if (SelectiveBlindness != null)
                                {
                                    GameLiving EffectOwner = SelectiveBlindness.EffectSource;
                                    if (EffectOwner == player)
                                    {
                                        if (Caster is GamePlayer) ((GamePlayer)Caster).Out.SendMessage(string.Format("{0} is invisible to you!", player.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                                    }
                                    else list.Add(player);
                                }
                                else list.Add(player);
                            }
                        }
                        foreach (GameNPC npc in target.GetNPCsInRadius(modifiedRadius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true))
                            {
                                if (!npc.HasAbility("DamageImmunity")) list.Add(npc);
                            }
                        }
                    }
                    else
                    {
                        if (target != null && GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
                        {
                            // Apply Mentalist RA5L
                            if (Spell.Range > 0)
                            {
                                SelectiveBlindnessEffect SelectiveBlindness = Caster.EffectList.GetOfType<SelectiveBlindnessEffect>();
                                if (SelectiveBlindness != null)
                                {
                                    GameLiving EffectOwner = SelectiveBlindness.EffectSource;
                                    if (EffectOwner == target)
                                    {
                                        if (Caster is GamePlayer) ((GamePlayer)Caster).Out.SendMessage(string.Format("{0} is invisible to you!", target.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                                    }
                                    else if (!target.HasAbility("DamageImmunity")) list.Add(target);
                                }
                                else if (!target.HasAbility("DamageImmunity")) list.Add(target);
                            }
                            else if (!target.HasAbility("DamageImmunity")) list.Add(target);
                        }
                    }
                    break;
                #endregion
                #region Realm
                case "realm":

                    //Croco Tear Ring
                    if (modifiedRadius > 0 && TargetMod != null)
                    {
                        
                        if (target == null || Spell.Range == 0)
                            target = Caster;

                        foreach (GamePlayer player in target.GetPlayersInRadius(modifiedRadius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true) == false)
                            {
                             
                                list.Add(player);
                                if (player != Caster && player != null && !player.IsWithinRadius(Caster, CalculateSpellRange() -100))
                                {
                                                                      
                                    if (list.Contains(player))
                                    {
                                        //log.ErrorFormat("remove: {0}", player.Name);
                                        list.Remove(player);
                                    }

                                }
                            }
                        }
                        foreach (GameNPC npc in target.GetNPCsInRadius(modifiedRadius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true) == false)
                            {
                                list.Add(npc);

                                
                                if (npc != Caster && npc != null && !npc.IsWithinRadius(Caster, CalculateSpellRange() - 100))
                                {
                                                                       
                                    if (list.Contains(npc))
                                    {
                                        //log.ErrorFormat("remove: {0}", npc.Name);
                                        list.Remove(npc);
                                    }

                                }
                            }
                        }
                    }
                    else
                    {

                        if (target == null || Spell.Range == 0)
                            target = Caster;

                        if (Spell.Radius > 0)
                        {


                            foreach (GamePlayer player in target.GetPlayersInRadius((ushort)Spell.Radius))
                            {
                                if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true) == false)
                                {
                                  
                                    list.Add(player);
                                    
                                }
                            }

                            foreach (GameNPC npc in target.GetNPCsInRadius((ushort)Spell.Radius))
                            {
                                if (GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true) == false)
                                {
                                    list.Add(npc);


                                    
                                }
                            }
                        }
                        else

                        if (target != null && GameServer.ServerRules.IsAllowedToAttack(Caster, target, true) == false)
                        {
                            list.Add(target);
                        }

                    }
                    break;
                #endregion
                #region Self
                case "self":
                    {
                        if (modifiedRadius > 0)
                        {
                            if (target == null || Spell.Range == 0)
                                target = Caster;
                            foreach (GamePlayer player in target.GetPlayersInRadius(modifiedRadius))
                            {
                                if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true) == false)
                                {
                                    list.Add(player);
                                }
                            }
                            foreach (GameNPC npc in target.GetNPCsInRadius(modifiedRadius))
                            {
                                if (GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true) == false)
                                {
                                    list.Add(npc);
                                }
                            }
                        }
                        else
                        {

                            list.Add(Caster);
                        }
                        break;
                    }
                #endregion
                #region casterself
                case "casterself":
                    {

                        target = Caster;

                        {
                            list.Add(Caster);
                        }
                        break;
                    }
                #endregion
                #region Group
                case "group":
                    {
                        Group group = m_caster.Group;
                        int spellRange = CalculateSpellRange();
                        if (spellRange == 0)
                            spellRange = modifiedRadius;

                        //Just add ourself
                        if (group == null)
                        {
                            list.Add(m_caster);

                            IControlledBrain npc = m_caster.ControlledBrain;
                            if (npc != null)
                            {
                                //Add our first pet
                                GameNPC petBody2 = npc.Body;
                                if (m_caster.IsWithinRadius(petBody2, spellRange))
                                    list.Add(petBody2);

                                //Now lets add any subpets!
                                if (petBody2 != null && petBody2.ControlledNpcList != null)
                                {
                                    foreach (IControlledBrain icb in petBody2.ControlledNpcList)
                                    {
                                        if (icb != null && m_caster.IsWithinRadius(icb.Body, spellRange))
                                            list.Add(icb.Body);
                                    }
                                }
                            }

                        }
                        //We need to add the entire group
                        else
                        {
                            foreach (GameLiving living in group.GetMembersInTheGroup())
                            {
                                // only players in range
                                if (m_caster.IsWithinRadius(living, spellRange))
                                {
                                    list.Add(living);

                                    IControlledBrain npc = living.ControlledBrain;
                                    if (npc != null)
                                    {
                                        //Add our first pet
                                        GameNPC petBody2 = npc.Body;
                                        if (m_caster.IsWithinRadius(petBody2, spellRange))
                                            list.Add(petBody2);

                                        //Now lets add any subpets!
                                        if (petBody2 != null && petBody2.ControlledNpcList != null)
                                        {
                                            foreach (IControlledBrain icb in petBody2.ControlledNpcList)
                                            {
                                                if (icb != null && m_caster.IsWithinRadius(icb.Body, spellRange))
                                                    list.Add(icb.Body);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        break;
                    }
                #endregion
                #region Cone AoE
                case "cone":
                    {
                        target = Caster;
                        foreach (GamePlayer player in target.GetPlayersInRadius((ushort)Spell.Range))
                        {
                            if (player == Caster)
                                continue;

                            if (!m_caster.IsObjectInFront(player, (double)(Spell.Radius != 0 ? Spell.Radius : 100), false))
                                continue;

                            if (!GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
                                continue;

                            list.Add(player);
                        }

                        foreach (GameNPC npc in target.GetNPCsInRadius((ushort)Spell.Range))
                        {
                            if (npc == Caster)
                                continue;

                            if (!m_caster.IsObjectInFront(npc, (double)(Spell.Radius != 0 ? Spell.Radius : 100), false))
                                continue;

                            if (!GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true))
                                continue;

                            if (!npc.HasAbility("DamageImmunity")) list.Add(npc);

                        }
                        break;
                    }
                    #endregion
            }
            #endregion
            return list;
        }

        /// <summary>
        /// Cast all subspell recursively
        /// </summary>
        /// <param name="target"></param>
        public virtual void CastSubSpells(GameLiving target)
        {
            if (Spell.SubSpellID > 0 && Spell.SpellType != "Archery" && Spell.SpellType != "Bomber" && Spell.SpellType != "SummonAnimistFnF" && Spell.SpellType != "SummonAnimistPet" && Spell.SpellType != "Grapple")
            {
                Spell spell = SkillBase.GetSpellByID(m_spell.SubSpellID);
                //we need subspell ID to be 0, we don't want spells linking off the subspell
                if (target != null && spell != null && spell.SubSpellID == 0)
                {
                    ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(m_caster, spell, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
                    spellhandler.StartSpell(target);
                    /*
                    List<Spell> spells = SkillBase.GetSpellList(SpellLine.KeyName);
                    foreach (Spell subSpell in spells)
                    {
                        if (subSpell.ID == Spell.SubSpellID)
                        {
                            SpellHandler subSpellHandler = ScriptMgr.CreateSpellHandler(Caster, subSpell, line) as SpellHandler;
                            if (subSpellHandler != null)
                            {
                                subSpellHandler.StartSpell(target);
                                subSpellHandler.CastSubSpells(target, line);
                            }
                            break;
                        }
                     */
                }
            }
        }


        /// <summary>
        /// Tries to start a spell attached to an item (/use with at least 1 charge)
        /// Override this to do a CheckBeginCast if needed, otherwise spell will always cast and item will be used.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="item"></param>
        public virtual bool StartSpell(GameLiving target, InventoryItem item)
        {
            m_spellItem = item;
            return StartSpell(target);
        }


        /// <summary>
        /// Called when spell effect has to be started and applied to targets
        /// This is typically called after calling CheckBeginCast
        /// </summary>
        /// <param name="target">The current target object</param>
        public virtual bool StartSpell(GameLiving target)
        {
            // For PBAOE spells always set the target to the caster
            if (Spell.SpellType.ToLower() != "TurretPBAoE".ToLower() && (target == null || (Spell.Radius > 0 && Spell.Range == 0)))
            {
                target = Caster;
            }

            if (m_spellTarget == null)
                m_spellTarget = target;

            if (m_spellTarget == null) return false;

            var targets = SelectTargets(m_spellTarget);

            double effectiveness = Caster.Effectiveness;

            if (Caster.EffectList.GetOfType<MasteryofConcentrationEffect>() != null)
            {
                MasteryofConcentrationAbility ra = Caster.GetAbility<MasteryofConcentrationAbility>();
                if (ra != null && ra.Level > 0)
                {
                    effectiveness *= System.Math.Round((double)ra.GetAmountForLevel(ra.Level) / 100, 2);
                }
            }

            //[StephenxPimentel] Reduce Damage if necro is using MoC
            if (Caster is NecromancerPet)
            {
                if ((Caster as NecromancerPet).Owner.EffectList.GetOfType<MasteryofConcentrationEffect>() != null)
                {
                    MasteryofConcentrationAbility necroRA = (Caster as NecromancerPet).Owner.GetAbility<MasteryofConcentrationAbility>();
                    if (necroRA != null && necroRA.Level > 0)
                    {
                        effectiveness *= System.Math.Round((double)necroRA.GetAmountForLevel(necroRA.Level) / 100, 2);
                    }
                }
            }

            if (Caster is GamePlayer && (Caster as GamePlayer).IsCharcterClass(eCharacterClass.Warlock) && m_spell.IsSecondary)
            {
                Spell uninterruptibleSpell = Caster.TempProperties.getProperty<Spell>(UninterruptableSpellHandler.WARLOCK_UNINTERRUPTABLE_SPELL);

                if (uninterruptibleSpell != null && uninterruptibleSpell.Value > 0)
                {
                    double nerf = uninterruptibleSpell.Value;
                    effectiveness *= (1 - (nerf * 0.01));
                    Caster.TempProperties.removeProperty(UninterruptableSpellHandler.WARLOCK_UNINTERRUPTABLE_SPELL);
                }
            }

            foreach (GameLiving t in targets)
            {
                // Aggressive NPCs will aggro on every target they hit
                // with an AoE spell, whether it landed or was resisted.


                if (Spell.Radius > 0 && Spell.Target.ToLower() == "enemy"
                    && Caster is GameNPC && (Caster as GameNPC).Brain is IOldAggressiveBrain)
                    ((Caster as GameNPC).Brain as IOldAggressiveBrain).AddToAggroList(t, 1);

                if (Util.Chance(CalculateSpellResistChance(t)))
                {

                    //Additional resist checks
                    GamePlayer PetOwner = null;
                    GameLiving attacker = null;
                    double targetCon = 0;

                    //Level reference must be the owner of a pet
                    if (Caster is GameNPC && (Caster as GameNPC).Brain is IControlledBrain)
                    {
                        ControlledNpcBrain brain = ((GameNPC)Caster).Brain as ControlledNpcBrain;

                        if (brain != null)
                        {
                            PetOwner = brain.GetPlayerOwner();

                            //Is there a pet ?
                            if (PetOwner != null)
                            {
                                //We set the owner of the pet
                                attacker = PetOwner;
                            }
                        }
                    }


                    //We get the conlevel of the target
                    if (t != null && attacker != null)
                    {
                        targetCon = GameObject.GetConLevel(attacker.Level, t.Level); // < 0 = Grey, green, blue 0 = yellow 1 = ora/red 1+ = red/purble
                    }
                    else
                    {
                        targetCon = GameObject.GetConLevel(Caster.Level, t.Level); // < 0 = Grey, green, blue 0 = yellow 1 = ora/red 1+ = red/purble
                    }

                    //log.ErrorFormat("conlevel = {0}", (int)targetCon);



                    if (t != null && targetCon < 0) //grey - blue no resist
                    {
                        //no resist
                    }
                    else if (t != null && targetCon > -1 && targetCon < 1) //yello - ora 25% chance to resist
                    {
                        if (Util.Chance(25))
                        {
                            OnSpellResisted(t);
                            continue;
                        }
                        //no resist
                    }
                    else if (t != null && targetCon == 1) // 1 red - purple 70% chance to resist
                    {
                        if (Util.Chance(70))
                        {
                            OnSpellResisted(t);
                            continue;
                        }
                        //no ressist
                    }
                    else if (t != null && targetCon > 1) // 1+ red - purple highter con must be resist
                    {
                        OnSpellResisted(t);
                        continue;
                    }
                }

                if (Spell.SpellType != "DirectDamage" && Spell.SpellType != "Bomber" && Spell.Name != "OmniTap" && FindImmunityOfSpellTypeOnTarget(m_spellTarget, Spell.SpellType) == null && Caster.TempProperties.getProperty<bool>(AnimStatus) == false && m_spell.Target == "Enemy")
                {
                    if (target != Caster)
                    {
                       
                        SendEffectAnimation(target, 0, false, 1);
                    }

                }
                //log.Error("remove proteries4");
                Caster.TempProperties.removeProperty(AnimStatus);
                //if (Spell.Radius == 0)
                if (Spell.Radius == 0 || HasPositiveEffect)
                {
                    ApplyEffectOnTarget(t, effectiveness);
                }
                else if (Spell.Target.ToLower() == "area")
                {
                    int dist = t.GetDistanceTo(Caster.GroundTarget);
                    if (dist >= 0)

                        ApplyEffectOnTarget(t, (effectiveness - CalculateAreaVariance(t, dist, Spell.Radius)));

                }
                else if (Spell.Target.ToLower() == "cone")
                {
                    int dist = t.GetDistanceTo(Caster);
                    //Cone spells use the range for their variance!
                    if (dist >= 0)

                        ApplyEffectOnTarget(t, (effectiveness - CalculateAreaVariance(t, dist, Spell.Range)));

                }
                else
                {
                    int dist = t.GetDistanceTo(target);
                    if (dist >= 0)

                        ApplyEffectOnTarget(t, (effectiveness - CalculateAreaVariance(t, dist, Spell.Radius)));

                }
            }

            if (Spell.Target.ToLower() == "ground")
            {
                ApplyEffectOnTarget(null, 1);
            }

            CastSubSpells(target);
            return true;
        }
        /// <summary>
        /// Calculate the variance due to the radius of the spell
        /// </summary>
        /// <param name="distance">The distance away from center of the spell</param>
        /// <param name="radius">The radius of the spell</param>
        /// <returns></returns>
        protected virtual double CalculateAreaVariance(GameLiving target, int distance, int radius)
        {
            return ((double)distance / (double)radius);
        }

        /// <summary>
        /// Calculates the effect duration in milliseconds
        /// </summary>
        /// <param name="target">The effect target</param>
        /// <param name="effectiveness">The effect effectiveness</param>
        /// <returns>The effect duration in milliseconds</returns>
        protected virtual int CalculateEffectDuration(GameLiving target, double effectiveness)
        {
            double duration = Spell.Duration;
            duration *= (1.0 + m_caster.GetModified(eProperty.SpellDuration) * 0.01);
            if (Spell.InstrumentRequirement != 0)
            {
                InventoryItem instrument = Caster.AttackWeapon;
                if (instrument != null)
                {
                    duration *= 1.0 + Math.Min(1.0, instrument.Level / (double)Caster.Level); // up to 200% duration for songs
                    duration *= instrument.Condition / (double)instrument.MaxCondition * instrument.Quality / 100;
                }
            }

            duration *= effectiveness;
            if (duration < 1)
                duration = 1;
            else if (duration > (Spell.Duration * 4))
                duration = (Spell.Duration * 4);
            return (int)duration;
        }

        /// <summary>
        /// Creates the corresponding spell effect for the spell
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectiveness"></param>
        /// <returns></returns>
        protected virtual GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
        {
            int freq = Spell != null ? Spell.Frequency : 0;
            return new GameSpellEffect(this, CalculateEffectDuration(target, effectiveness), freq, effectiveness);
        }

        /// <summary>
        /// Apply effect on target or do spell action if non duration spell
        /// </summary>
        /// <param name="target">target that gets the effect</param>
        /// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
        public virtual void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            if (target.ObjectState != GameObject.eObjectState.Active)
                return;


            if (target is GamePlayer)
            {
                GameSpellEffect effect1;
                effect1 = SpellHandler.FindEffectOnTarget(target, "Phaseshift");
                if ((effect1 != null && (Spell.SpellType != "SpreadHeal" || Spell.SpellType != "Heal" || Spell.SpellType != "SpeedEnhancement")))
                {
                    if (m_caster is GamePlayer)
                    {
                        //MessageToCaster(target.Name + " is Phaseshifted and can't be effected by this Spell!", eChatType.CT_SpellResisted);
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YouArePhaseshiftedCantBeAffected", target.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                    return;
                }
            }
            if (SpellHandler.FindImmunityOfSpellTypeOnTarget(target, Spell.SpellType) != null)
            {
                if (Caster is GamePlayer)
                {
                    // MessageToCaster(target.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CCSpellHandler.TargetIsImmuneToThisEffect", target.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    //log.Error("hier 3");
                }


                SendEffectAnimation(target, 0, false, 0);
                return;
            }

            if ((target is Keeps.GameKeepDoor || target is Keeps.GameKeepComponent))
            {
                bool isAllowed = false;
                bool isSilent = false;

                if (Spell.Radius == 0)
                {
                    switch (Spell.SpellType.ToLower())
                    {
                        case "archery":
                        case "bolt":
                        case "bomber":
                        case "damagespeeddecrease":
                        case "directdamage":
                        case "magicalstrike":
                        case "siegearrow":
                        case "summontheurgistpet":
                        case "directdamagewithdebuff":
                            isAllowed = true;
                            break;
                    }
                }

                if (Spell.Radius > 0)
                {
                    // pbaoe is allowed, otherwise door is in range of a AOE so don't spam caster with a message
                    if (Spell.Range == 0)
                        isAllowed = true;
                    else
                        isSilent = true;
                }

                if (!isAllowed)
                {
                    if (!isSilent)
                    {
                        if (m_caster is GamePlayer)
                        {
                            (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.YourSpellHasNoEffectOn", target.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        }
                        //MessageToCaster(String.Format("Your spell has no effect on the {0}!", target.Name), eChatType.CT_SpellResisted);
                    }

                    return;
                }
            }

            ///Das hier muss noch gefixt werden!! // TODO player.PlayerEffectiveness
            if (Spell.IsItemSpell == false && (m_spellLine.KeyName == GlobalSpellsLines.Item_Effects || m_spellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect || m_spellLine.KeyName == GlobalSpellsLines.Potions_Effects || m_spellLine.KeyName == Specs.Savagery || m_spellLine.KeyName == GlobalSpellsLines.Character_Abilities || m_spellLine.KeyName == "OffensiveProc"))
                effectiveness = 1.0; // TODO player.PlayerEffectiveness
            //effectiveness = 1 - 0.3 * Caster.GetConLevel(target);
            //log.ErrorFormat("Target effectiveness = {0}", effectiveness.ToString());



            if (effectiveness <= 0)
                return; // no effect

            // Apply effect for Duration Spell.
            if (SpellHandler.FindImmunityOfSpellTypeOnTarget(target, Spell.SpellType) == null && (Spell.SpellType != "HereticDoTLostOnPulse" && Spell.SpellType != "HereticDamageSpeedDecreaseLOP" && Spell.SpellType != "HereticDoTLostOnPulse" && (Spell.Duration > 0 && Spell.Target.ToLower() != "area") || Spell.Concentration > 0))
            {

                OnDurationEffectApply(target, effectiveness);

            }
            else
            {
                /*
                GameSpellEffect eff = SpellHandler.FindEffectOnTarget(target, "Disease");




                if (eff != null && eff.Spell.SpellType != Spell.SpellType)
                {
                    if (SpellHandler.FindImmunityOfSpellTypeOnTarget(target, Spell.SpellType) == null)
                    {
                        // log.Error("starte OnDirectEffect");
                        OnDirectEffect(target, effectiveness);
                    }
                }
                */
                if (SpellHandler.FindImmunityOfSpellTypeOnTarget(target, Spell.SpellType) == null)
                {

                    //log.Error("starte OnDirectEffect2");
                    OnDirectEffect(target, effectiveness);

                }
            }

            if (!HasPositiveEffect && SpellHandler.FindImmunityOfSpellTypeOnTarget(target, Spell.SpellType) == null)
            {
                AttackData ad = new AttackData();
                ad.Attacker = Caster;
                ad.Target = target;
                ad.AttackType = AttackData.eAttackType.Spell;
                ad.SpellHandler = this;
                ad.AttackResult = GameLiving.eAttackResult.HitUnstyled;
                ad.IsSpellResisted = false;

                m_lastAttackData = ad;

                // Treat non-damaging effects as attacks to trigger an immediate response and BAF
                if (ad.Damage == 0 && ad.Target is GameNPC)
                {
                    IOldAggressiveBrain aggroBrain = ((GameNPC)ad.Target).Brain as IOldAggressiveBrain;
                    if (aggroBrain != null)
                        aggroBrain.AddToAggroList(Caster, 1);
                }
            }
        }

        /// <summary>
        /// Called when cast sequence is complete
        /// </summary>
        public virtual void OnAfterSpellCastSequence()
        {
            if (CastingCompleteEvent != null)
            {
                CastingCompleteEvent(this);

            }
        }

        /// <summary>
        /// Determines wether this spell is better than given one
        /// </summary>
        /// <param name="oldeffect"></param>
        /// <param name="neweffect"></param>
        /// <returns>true if this spell is better version than compare spell</returns>
        public virtual bool IsNewEffectBetter(GameSpellEffect oldeffect, GameSpellEffect neweffect)
        {
            Spell oldspell = oldeffect.Spell;
            Spell newspell = neweffect.Spell;

            //Blade Turn
            if (oldspell.Target == "Realm" && oldspell.SpellType == "Bladeturn" && newspell.Target == "Realm" && newspell.SpellType == "Bladeturn" && newspell.Pulse == 0)
            {
                return false;
            }
            if (newspell.SpellType == "VampiirArmorDebuff" && oldspell.SpellType == "VampiirArmorDebuff")
            {
                return false;
            }
            if (newspell.SpellType == "Disease" && oldspell.SpellType == "Disease")
            {
                return false;
            }
            if (newspell.SpellType == "Disarm" && oldspell.SpellType == "Disarm")
            {
                return false;
            }
            if (newspell.SpellType == "ArmorAbsorptionDebuff" && oldspell.SpellType == "ArmorAbsorptionDebuff")
            {
                return false;
            }
            if (newspell.SpellType == "CastingSpeedDebuff" && oldspell.SpellType == "CastingSpeedDebuff")
            {
                return false;
            }
            if (newspell.SpellType == "Nearsight" && oldspell.SpellType == "Nearsight")
            {
                return false;
            }
            if (newspell.SpellType == "SpeedDecrease" && oldspell.SpellType == "SpeedDecrease")
            {
                return false;
            }
            if (newspell.SpellType == "ToHitDebuff" && oldspell.SpellType == "ToHitDebuff")
            {
                return false;
            }
            if (newspell.SpellType == "UnbreakableSpeedDecrease" && oldspell.SpellType == "UnbreakableSpeedDecrease")
            {
                return false;
            }
            if (newspell.SpellType == "VampiirEffectivenessDeBuff" && oldspell.SpellType == "VampiirEffectivenessDeBuff")
            {
                return false;
            }
            if (newspell.SpellType == "VampiirSkillBonusDeBuff" && oldspell.SpellType == "VampiirSkillBonusDeBuff")
            {
                return false;
            }
            if (newspell.SpellType == "VampSpeedDecrease" && oldspell.SpellType == "VampSpeedDecrease")
            {
                return false;
            }
            if (newspell.SpellType == "VampSpeedDecrease" && oldspell.SpellType == "VampSpeedDecrease")
            {
                return false;
            }
            if (newspell.SpellType == "WarlockSpeedDecrease" && oldspell.SpellType == "WarlockSpeedDecrease")
            {
                return false;
            }





            if (oldspell.IsConcentration)
                return false;

            //log.Warn("ok");
            if (newspell.Damage < oldspell.Damage)
                return false;
            if (newspell.Value < oldspell.Value)
                return false;
            //makes problems for immunity effects
            if (!oldeffect.ImmunityState && !newspell.IsConcentration)
            {
                if (neweffect.Duration <= oldeffect.RemainingTime)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines wether this spell is compatible with given spell
        /// and therefore overwritable by better versions
        /// spells that are overwritable cannot stack
        /// </summary>
        /// <param name="compare"></param>
        /// <returns></returns>
        public virtual bool IsOverwritable(GameSpellEffect compare)
        {
            if (Spell.EffectGroup != 0)
                return Spell.EffectGroup == compare.Spell.EffectGroup;
            if (compare.Spell.SpellType != Spell.SpellType)
                return false;
            return true;
        }

        /// <summary>
        /// Determines wether this spell can be disabled
        /// by better versions spells that stacks without overwriting
        /// </summary>
        /// <param name="compare"></param>
        /// <returns></returns>
        public virtual bool IsCancellable(GameSpellEffect compare)
        {
            if (compare.SpellHandler != null)
            {
                if ((compare.SpellHandler.AllowCoexisting || AllowCoexisting)
                    && (!compare.SpellHandler.SpellLine.KeyName.Equals(SpellLine.KeyName, StringComparison.OrdinalIgnoreCase)
                        || compare.SpellHandler.Spell.IsInstantCast != Spell.IsInstantCast))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines wether new spell is better than old spell and should disable it
        /// </summary>
        /// <param name="oldeffect"></param>
        /// <param name="neweffect"></param>
        /// <returns></returns>
        public virtual bool IsCancellableEffectBetter(GameSpellEffect oldeffect, GameSpellEffect neweffect)
        {
            if (neweffect.SpellHandler.Spell.Value > oldeffect.SpellHandler.Spell.Value)
                return true;

            return false;
        }
       
        /// <summary>
        /// Execute Duration Spell Effect on Target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectiveness"></param>
        public virtual void OnDurationEffectApply(GameLiving target, double effectiveness)
        {

            if (!target.IsAlive || target.EffectList == null)
                return;


            eChatType noOverwrite = (Spell.Pulse == 0) ? eChatType.CT_SpellResisted : eChatType.CT_SpellPulse;
            GameSpellEffect neweffect = CreateSpellEffect(target, effectiveness);

            // Iterate through Overwritable Effect
            var overwritenEffects = target.EffectList.OfType<GameSpellEffect>().Where(effect => effect.SpellHandler != null && effect.SpellHandler.IsOverwritable(neweffect));

            // Store Overwritable or Cancellable
            var enable = true;
            var cancellableEffects = new List<GameSpellEffect>(1);
            GameSpellEffect overwriteEffect = null;

            foreach (var ovEffect in overwritenEffects)
            {
                // If we can cancel spell effect we don't need to overwrite it
                if (ovEffect.SpellHandler.IsCancellable(neweffect))
                {
                    // Spell is better than existing "Cancellable" or it should start disabled

                    if (IsCancellableEffectBetter(ovEffect, neweffect))
                        cancellableEffects.Add(ovEffect);
                    else
                        enable = false;
                }
                else
                {
                    // Check for Overwriting.
                    if (ovEffect.ImmunityState == false && IsNewEffectBetter(ovEffect, neweffect) && (ovEffect.Spell.Damage <= neweffect.Spell.Damage || ovEffect.Spell.Value <= neweffect.Spell.Value || neweffect.Spell.Target != "Enemy" || Caster is GamePlayer && neweffect.Spell.Pulse > 0 && neweffect.Spell.SpellType == "Charm" && ((Caster as GamePlayer).ResistCount <= 2)))
                    {
                        //log.Error("überschreibe Pulsing spell1");
                        // New Spell is overwriting this one.
                        overwriteEffect = ovEffect;
                    }
                    else
                    {
                        // Old Spell is Better than new one
                        SendSpellResistAnimation(target);

                        //Pulse af spell sould not owerride nonpulse af
                        if (ovEffect.Spell.Pulse == 0 && neweffect.Spell.Pulse > 0 && ovEffect.Spell.SpellType == "ArmorFactorBuff" && neweffect.Spell.SpellType == "ArmorFactorBuff")
                        {
                            //log.Error("konnte nicht überschreiben");
                        }
                        else

                        if (target == Caster)
                        {
                            if (ovEffect.ImmunityState)
                            {
                                if (Caster is GamePlayer)
                                {
                                    long UPDATETICK = Caster.TempProperties.getProperty<long>(SpamTime);
                                    long changeTime = Caster.CurrentRegion.Time - UPDATETICK;

                                    if (neweffect.Spell.Pulse > 0)
                                        CancelPulsingSpell(target, neweffect.Spell.SpellType);
                                    else
                                        neweffect.Cancel(false);


                                    if (changeTime > 15 * 1000 || UPDATETICK == 0)
                                    {
                                        Caster.TempProperties.setProperty(SpamTime, Caster.CurrentRegion.Time);



                                        if (Caster is GamePlayer && ovEffect.Owner != null)
                                        {

                                            SendEffectAnimation(ovEffect.Owner, 0, false, 0); // pulsing auras or songs resist animati
                                            (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CCSpellHandler.TargetIsImmuneToThisEffect", ovEffect.Owner.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);


                                            if (Spell.Pulse != 0)
                                            {
                                                CancelPulsingSpell(Caster, Spell.SpellType);
                                            }
                                            // log.Error("hier 4");
                                        }
                                    }
                                    else
                                    {
                                        //no message
                                    }
                                }
                            }
                            else
                            {
                                if (Caster is GamePlayer)
                                {
                                    long UPDATETICK = Caster.TempProperties.getProperty<long>(SpamTime);
                                    long changeTime = Caster.CurrentRegion.Time - UPDATETICK;

                                    //override busffs and effects with highter value if same EffectGroup(ovEffect.Spell.Pulse == 0 && neweffect.Spell.Pulse == 0 || ovEffect.Spell.Pulse > 0 && neweffect.Spell.Pulse > 0) 
                                    if (Caster.Realm == target.Realm && Caster is GamePlayer && ovEffect.Owner != null && target != null && ovEffect.Spell.Value > 0 && ovEffect.Spell.Value < Spell.Value && ovEffect.Spell.EffectGroup == Spell.EffectGroup)
                                    {

                                        if (ovEffect.Spell.Pulse > 0 && neweffect.Spell.Pulse > 0)
                                        {
                                            //log.Error("überschreibe Pulsing spell2");
                                            CancelPulsingSpell(target, ovEffect.Spell.SpellType);
                                            //log.Error("konnte spell überschreiben0");
                                            neweffect.Start(target);
                                        }
                                        else if (ovEffect.Spell.Pulse == 0)
                                        {
                                            // log.Error("konnte spell überschreiben1");
                                            ovEffect.Cancel(false);
                                            neweffect.Start(target);
                                        }

                                    }

                                    else if (changeTime > 30 * 1000 || UPDATETICK == 0)
                                    {
                                        Caster.TempProperties.setProperty(SpamTime, Caster.CurrentRegion.Time);
                                        if (ovEffect.Spell.SpellType != "StormEnergyTempest" && ovEffect.Spell.SpellType != "SpeedWrap" && ovEffect.Spell.SpellType != "Prescience" && ovEffect.Spell.SpellType != "PowerOverTime" && ovEffect.Spell.SpellType != "HealOverTime")
                                        {


                                            if (Caster is GamePlayer && ovEffect.Owner != null)
                                            {
                                                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.Youalreadyhavethateffect", ovEffect.Owner.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);

                                            }
                                        }
                                    }
                                    else
                                    {
                                        //no message
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (ovEffect.ImmunityState)
                            {
                                if (Caster is GamePlayer && ovEffect.Owner != null)
                                {

                                    SendEffectAnimation(ovEffect.Owner, 0, false, 0); // pulsing auras or songs resist animati
                                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "CCSpellHandler.TargetIsImmuneToThisEffect", ovEffect.Owner.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);

                                    //log.Error("hier 5");
                                    if (Spell.Pulse != 0)
                                    {
                                        CancelPulsingSpell(Caster, Spell.SpellType);
                                        //log.Error("konnte spell überschreiben2");
                                    }
                                }
                            }
                            else
                            {
                               
                                if (Caster is GamePlayer && ovEffect.Spell.Pulse > 0 && ovEffect.Spell.SpellType == "Charm" && ((Caster as GamePlayer).ResistCount > 0))
                                {
                                    //no message for ressisted charm spells
                                }
                                else
                                {


                                    if (Caster is GamePlayer)
                                    {
                                        GamePlayer player = Caster as GamePlayer;
                                        long UPDATETICK = Caster.TempProperties.getProperty<long>(SpamTime);
                                        long changeTime = Caster.CurrentRegion.Time - UPDATETICK;

                                        //override busffs and effects with highter value if same EffectGroup
                                        if (Caster.Realm == target.Realm && Caster is GamePlayer && ovEffect.Owner != null && target != null && ovEffect.Spell.Value > 0 && ovEffect.Spell.Value < Spell.Value && ovEffect.Spell.EffectGroup == Spell.EffectGroup)
                                        {
                                            if (ovEffect.Spell.Pulse > 0)
                                                CancelPulsingSpell(target, ovEffect.Spell.SpellType);
                                            else
                                                ovEffect.Cancel(false);

                                            neweffect.Start(target);
                                            //log.Error("konnte spell überschreiben3");

                                        }

                                        //Heretic AOE Dots
                                        if (ovEffect.Owner != null && target != null && ovEffect.Spell.EffectGroup != Spell.EffectGroup && ovEffect.Spell.SpellType == "HereticDamageSpeedDecreaseLOP" 
                                            && ovEffect.Spell.Radius > 0 && Spell.SpellType == "DamageOverTime" && Spell.Radius == 0)
                                        {
                                            if (ovEffect.Spell.Pulse > 0)
                                                CancelPulsingSpell(target, ovEffect.Spell.SpellType);
                                            else
                                                ovEffect.Cancel(false);
                                                                                        
                                            neweffect.Start(target);
                                        }
                                        //Heretic own focus dot vs own single Dots 
                                        else if (ovEffect.Owner != null && target != null && ovEffect.Spell.SpellType == "HereticDamageSpeedDecreaseLOP" && neweffect.Spell.SpellType == "DamageOverTime"
                                             && Spell.Radius == 0 && ovEffect.SpellHandler.Caster == neweffect.SpellHandler.Caster)
                                        {
                                            if (ovEffect.Spell.Pulse > 0)
                                                CancelPulsingSpell(target, ovEffect.Spell.SpellType);
                                            else
                                                ovEffect.Cancel(false);
                                            
                                            neweffect.Start(target);
                                        }



                                        else if (changeTime > 15 * 1000 || UPDATETICK == 0)
                                        {

                                            Caster.TempProperties.setProperty(SpamTime, Caster.CurrentRegion.Time);

                                            //speedWarp und PrescienceNode message unterdrückung!
                                            if (ovEffect.Spell.SpellType != "StormEnergyTempest" && ovEffect.Spell.SpellType != "SpeedWrap" && ovEffect.Spell.SpellType != "Prescience" && ovEffect.Spell.SpellType != "PowerOverTime" && ovEffect.Spell.SpellType != "HealOverTime")
                                            {




                                                if (Caster is GamePlayer && ovEffect.Owner != null && target != null)
                                                {

                                                    //this.MessageToCaster(noOverwrite, "{0} already has that effect.", target.GetName(0, true));
                                                    // MessageToCaster("Wait until it expires. Spell Failed.", noOverwrite);
                                                    SendEffectAnimation(ovEffect.Owner, 0, false, 0); // pulsing auras or songs resist animati
                                                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.TheTargetAlreadyHaveThatEffect", target.GetName(0, true)), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                                }

                                            }
                                            //if (Caster != null && Spell.Pulse != 0 && neweffect.Spell.Pulse > 0)
                                            if (Caster != null && ovEffect.Spell.Pulse > 0 && neweffect.Spell.Pulse > 0)
                                            {

                                                CancelPulsingSpell(Caster, Spell.SpellType);
                                                //log.Error("konnte spell überschreiben4");
                                            }

                                        }
                                        else
                                        {
                                            //no message
                                        }
                                    }
                                }
                            }
                        }
                        // Prevent Adding.
                        return;
                    }
                }

            }

            // Register Effect list Changes
            target.EffectList.BeginChanges();
            try
            {
                // Check for disabled effect
                foreach (var disableEffect in cancellableEffects)
                    disableEffect.DisableEffect(false);

                if (overwriteEffect != null)
                {
                    if (enable)
                        overwriteEffect.Overwrite(neweffect);
                    else
                        overwriteEffect.OverwriteDisabled(neweffect);
                }
                else
                {
                    if (enable)
                        neweffect.Start(target);
                    else
                        neweffect.StartDisabled(target);
                }
            }
            finally
            {
                target.EffectList.CommitChanges();
            }
        }

        /// <summary>
        /// Called when Effect is Added to target Effect List
        /// </summary>
        /// <param name="effect"></param>
        public virtual void OnEffectAdd(GameSpellEffect effect)
        {
        }

        /// <summary>
        /// Check for Spell Effect Removed to Enable Best Cancellable
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="overwrite"></param>
        public virtual void OnEffectRemove(GameSpellEffect effect, bool overwrite)
        {
            if (!overwrite)
            {
                /*
                if (Spell.IsFocus)
                {
                    FocusSpellAction(null, Caster, null);
                }
                */

                // Re-Enable Cancellable Effects.
                var enableEffect = effect.Owner.EffectList.OfType<GameSpellEffect>()
                    .Where(eff => eff != effect && eff.SpellHandler != null && eff.SpellHandler.IsOverwritable(effect) && eff.SpellHandler.IsCancellable(effect));

                // Find Best Remaining Effect
                GameSpellEffect best = null;
                foreach (var eff in enableEffect)
                {
                    if (best == null)
                        best = eff;
                    else if (best.SpellHandler.IsCancellableEffectBetter(best, eff))
                        best = eff;
                }

                if (best != null)
                {
                    effect.Owner.EffectList.BeginChanges();
                    try
                    {
                        // Enable Best Effect
                        best.EnableEffect();
                    }
                    finally
                    {
                        effect.Owner.EffectList.CommitChanges();
                    }
                }
            }
        }

        /// <summary>
        /// execute non duration spell effect on target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectiveness"></param>
        public virtual void OnDirectEffect(GameLiving target, double effectiveness)
        {
        }

        /// <summary>
        /// When an applied effect starts
        /// duration spells only
        /// </summary>
        /// <param name="effect"></param>
        public virtual void OnEffectStart(GameSpellEffect effect)
        {

            if (Spell.Pulse == 0 && FindImmunityOfSpellTypeOnTarget(m_spellTarget, Spell.SpellType) == null)

                SendEffectAnimation(effect.Owner, 0, false, 1);
            if (Spell.IsFocus) // Add Event handlers for focus spell
            {
                GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackFinished, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.RemoveHandler(Caster, GameLivingEvent.CastStarting, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
                //GameEventMgr.RemoveHandler(Caster, GameLivingEvent.TakeDamage, new DOLEventHandler(FocusSpellAction));

                Caster.TempProperties.setProperty(FOCUS_SPELL, effect);
                GameEventMgr.AddHandler(Caster, GameLivingEvent.AttackFinished, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.AddHandler(Caster, GameLivingEvent.CastStarting, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.AddHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.AddHandler(Caster, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.AddHandler(Caster, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.AddHandler(Caster, GameObjectEvent.TakeDamage, new DOLEventHandler(FocusSpellAction));
            }
            if (Spell.IsCastFocus) // Add Event handlers for focus spell
            {
                GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackFinished, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.RemoveHandler(Caster, GameLivingEvent.CastStarting, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));

                Caster.TempProperties.setProperty(FOCUS_SPELL, effect);
                GameEventMgr.AddHandler(Caster, GameLivingEvent.AttackFinished, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.AddHandler(Caster, GameLivingEvent.CastStarting, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.AddHandler(Caster, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.AddHandler(Caster, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
            }
            if (Spell.IsMoveFocus) // Add Event handlers for focus spell
            {
                GameEventMgr.AddHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));

                Caster.TempProperties.setProperty(FOCUS_SPELL, effect);
                GameEventMgr.AddHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.AddHandler(Caster, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
                GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
            }
        }

        /// <summary>
        /// When an applied effect pulses
        /// duration spells only
        /// </summary>
        /// <param name="effect"></param>
        public virtual void OnEffectPulse(GameSpellEffect effect)
        {
            if (effect.Owner.IsAlive == false)
            {
                effect.Cancel(false);
            }
        }

        /// <summary>
        /// When an applied effect expires.
        /// Duration spells only.
        /// </summary>
        /// <param name="effect">The expired effect</param>
        /// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
        /// <returns>immunity duration in milliseconds</returns>
        public virtual int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            return 0;
        }

        /// <summary>
        /// Calculates chance of spell getting resisted
        /// </summary>
        /// <param name="target">the target of the spell</param>
        /// <returns>chance that spell will be resisted for specific target</returns>
        public virtual int CalculateSpellResistChance(GameLiving target)
        {
            if (m_spellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect || HasPositiveEffect)
            {
                return 0;
            }

            if (m_spellLine.KeyName == GlobalSpellsLines.Item_Effects && m_spellItem != null)
            {
                GamePlayer playerCaster = Caster as GamePlayer;
                if (playerCaster != null)
                {
                    int itemSpellLevel = m_spellItem.Template.LevelRequirement > 0 ? m_spellItem.Template.LevelRequirement : Math.Min(playerCaster.MaxLevel, m_spellItem.Level);
                    return 100 - (85 + ((itemSpellLevel - target.Level) / 2));
                }
            }

            return 100 - CalculateToHitChance(target);
        }

        /// <summary>
        /// When spell was resisted
        /// </summary>
        /// <param name="target">the target that resisted the spell</param>
        protected virtual void OnSpellResisted(GameLiving target)
        {
            SendSpellResistAnimation(target);
            SendSpellResistMessages(target);
            SendSpellResistNotification(target);
            StartSpellResistInterruptTimer(target);
            StartSpellResistLastAttackTimer(target);
            WasResist = true;
        }

        /// <summary>
        /// Send Spell Resisted Animation
        /// </summary>
        /// <param name="target"></param>
        public virtual void SendSpellResistAnimation(GameLiving target)
        {
            if (Spell.Pulse == 0 || !HasPositiveEffect)
                SendEffectAnimation(target, 0, false, 0);
        }

        /// <summary>
        /// Send Spell Resist Messages to Caster and Target
        /// </summary>
        /// <param name="target"></param>
        public virtual void SendSpellResistMessages(GameLiving target)
        {
            // Deliver message to the target, if the target is a pet, to its
            // owner instead.
            if (target is GameNPC)
            {
                IControlledBrain brain = ((GameNPC)target).Brain as IControlledBrain;
                if (brain != null)
                {
                    GamePlayer owner = brain.GetPlayerOwner();
                    if (owner != null)
                    {
                        if (owner is GamePlayer)
                        {
                            //MessageToLiving(owner, eChatType.CT_SpellResisted, "Your {0} resists the effect!", target.Name);
                            //this.MessageToLiving(owner, "Your " + target.Name + " resists the effect!", eChatType.CT_SpellResisted);
                            (owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((owner as GamePlayer).Client.Account.Language, "SpellHandler.TargetNameRessistTheEffect", target.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        }
                    }
                }
            }
            else
            {
                if (target is GamePlayer)
                {
                    //    MessageToLiving(target, "You resist the effect!", eChatType.CT_SpellResisted);
                    (target as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((target as GamePlayer).Client.Account.Language, "SpellHandler.YouRessistTheEffect"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
            }

            // Deliver message to the caster as well.
            //this.MessageToCaster(eChatType.CT_SpellResisted, "{0} resists the effect!", target.GetName(0, true));
            //this.MessageToCaster(target.GetName(0, true) + " resists the effect!", eChatType.CT_SpellResisted);
            if (Caster is GamePlayer)
            {
                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.TargetRessistTheEffect", target.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
            }
        }

        /// <summary>
        /// Send Spell Attack Data Notification to Target when Spell is Resisted
        /// </summary>
        /// <param name="target"></param>
        public virtual void SendSpellResistNotification(GameLiving target)
        {
            // Report resisted spell attack data to any type of living object, no need
            // to decide here what to do. For example, NPCs will use their brain.
            // "Just the facts, ma'am, just the facts."
            AttackData ad = new AttackData();
            ad.Attacker = Caster;
            ad.Target = target;
            ad.AttackType = AttackData.eAttackType.Spell;
            ad.SpellHandler = this;
            ad.AttackResult = GameLiving.eAttackResult.Missed;
            ad.IsSpellResisted = true;
            target.OnAttackedByEnemy(ad);

        }

        /// <summary>
        /// Start Spell Interrupt Timer when Spell is Resisted
        /// </summary>
        /// <param name="target"></param>
        public virtual void StartSpellResistInterruptTimer(GameLiving target)
        {
            // Spells that would have caused damage or are not instant will still
            // interrupt a casting player.
            if (!(Spell.SpellType.IndexOf("debuff", StringComparison.OrdinalIgnoreCase) >= 0 && Spell.CastTime == 0))
                target.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
        }

        /// <summary>
        /// Start Last Attack Timer when Spell is Resisted
        /// </summary>
        /// <param name="target"></param>
        public virtual void StartSpellResistLastAttackTimer(GameLiving target)
        {
            if (target.Realm == 0 || Caster.Realm == 0)
            {
                target.LastAttackedByEnemyTickPvE = target.CurrentRegion.Time;
                Caster.LastAttackTickPvE = Caster.CurrentRegion.Time;
            }
            else
            {
                target.LastAttackedByEnemyTickPvP = target.CurrentRegion.Time;
                Caster.LastAttackTickPvP = Caster.CurrentRegion.Time;
            }
        }

        #region messages

        /// <summary>
        /// Sends a message to the caster, if the caster is a controlled
        /// creature, to the player instead (only spell hit and resisted
        /// messages).
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public void MessageToCaster(string message, eChatType type)
        {
            if (Caster is GamePlayer)
            {
                (Caster as GamePlayer).MessageToSelf(message, type);
            }
            else if (Caster is GameNPC && (Caster as GameNPC).Brain is IControlledBrain
                     && (type == eChatType.CT_YouHit || type == eChatType.CT_SpellResisted))
            {
                GamePlayer owner = ((Caster as GameNPC).Brain as IControlledBrain).GetPlayerOwner();
                if (owner != null)
                {
                    owner.MessageFromControlled(message, type);
                }
            }
        }

        /// <summary>
        /// sends a message to a living
        /// </summary>
        /// <param name="living"></param>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public void MessageToLiving(GameLiving living, string message, eChatType type)
        {
            if (message != null && message.Length > 0)
            {
                living.MessageToSelf(message, type);
            }
        }

        /// <summary>
        /// Hold events for focus spells
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void FocusSpellAction(DOLEvent e, object sender, EventArgs args)
        {
            GameLiving living = sender as GameLiving;
            if (living == null) return;

            GameSpellEffect currentEffect = (GameSpellEffect)living.TempProperties.getProperty<object>(FOCUS_SPELL, null);
            if (currentEffect == null)
                return;


            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackFinished, new DOLEventHandler(FocusSpellAction));
            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.CastStarting, new DOLEventHandler(FocusSpellAction));
            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(FocusSpellAction));
            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
            //GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(FocusSpellAction));

            GameEventMgr.RemoveHandler(currentEffect.Owner, GameLivingEvent.Dying, new DOLEventHandler(FocusSpellAction));
            Caster.TempProperties.removeProperty(FOCUS_SPELL);

            CancelPulsingSpell(Caster, currentEffect.Spell.SpellType);
            currentEffect.Cancel(false);

            MessageToCaster(String.Format("You lose your focus on your {0} spell.", currentEffect.Spell.Name), eChatType.CT_SpellExpires);

            if (e == GameLivingEvent.Moving)
                MessageToCaster("You move and interrupt your focus!", eChatType.CT_Important);
        }
        #endregion

        /// <summary>
        /// Ability to cast a spell
        /// </summary>
        public SpellCastingAbilityHandler Ability
        {
            get { return m_ability; }
            set { m_ability = value; }
        }
        /// <summary>
        /// The Spell
        /// </summary>
        public Spell Spell
        {
            get { return m_spell; }
        }

        /// <summary>
        /// The Spell Line
        /// </summary>
        public SpellLine SpellLine
        {
            get { return m_spellLine; }
        }

        /// <summary>
        /// The Caster
        /// </summary>
        public GameLiving Caster
        {
            get { return m_caster; }
        }

        /// <summary>
        /// Is the spell being cast?
        /// </summary>
        public bool IsCasting
        {
            get { return m_castTimer != null && m_castTimer.IsAlive; }
        }

        /// <summary>
        /// Does the spell have a positive effect?
        /// </summary>
        public virtual bool HasPositiveEffect
        {
            get
            {
                if (m_spell.Target.ToLower() != "enemy" && m_spell.Target.ToLower() != "cone" && m_spell.Target.ToLower() != "area")
                    return true;

                return false;
            }
        }

        /// <summary>
        /// Is this Spell purgeable
        /// </summary>
        public virtual bool IsUnPurgeAble
        {
            get { return false; }
        }

        /// <summary>
        /// Current depth of delve info
        /// </summary>
        public byte DelveInfoDepth
        {
            get { return m_delveInfoDepth; }
            set { m_delveInfoDepth = value; }
        }

        /// <summary>
        /// Delve Info
        /// </summary>
        public virtual IList<string> DelveInfo(GamePlayer player)
        {

            var list = new List<string>(32);




            list.Add(Spell.Description);
            list.Add(" "); //empty line
            if (Spell.InstrumentRequirement != 0)
                list.Add(player != null ? LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.InstrumentRequire", GlobalConstants.InstrumentTypeToName(Spell.InstrumentRequirement)) : LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.InstrumentRequire", GlobalConstants.InstrumentTypeToName(Spell.InstrumentRequirement)));
            if (Spell.Damage != 0)
                list.Add(player != null ? LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Damage", Spell.Damage.ToString("0.###;0.###'%'")) : LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Damage", Spell.Damage.ToString("0.###;0.###'%'")));
            if (Spell.LifeDrainReturn != 0 && Spell.SpellType == "Morph" == false && Spell.SpellType == "DreamMorph" == false && Spell.SpellType == "DreamGroupMorph" == false && Spell.SpellType == "AlvarusMorph" == false)
                list.Add(player != null ? LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.HealthReturned", Spell.LifeDrainReturn) : LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.HealthReturned", Spell.LifeDrainReturn.ToString()));
            else if (Spell.Value != 0)
                list.Add(player != null ? LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Value", Spell.Value.ToString("0.###;0.###'%'")) : LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Value", Spell.Value.ToString("0.###;0.###'%'")));
            list.Add(player != null ? LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Target", Spell.Target) : LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Target", Spell.Target));
            if (Spell.Range != 0)
                list.Add(player != null ? LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Range", Spell.Range) : LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Range", Spell.Range.ToString()));
            if (Spell.Duration >= ushort.MaxValue * 1000)
                list.Add(player != null ? LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + " Permanent." : LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + " Permanent.");
            else if (Spell.Duration > 60000)
                list.Add(player != null ? LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + " " + (Spell.Duration / 60000).ToString() + ":" + (Spell.Duration % 60000 / 1000).ToString("00") + " min" : LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + " " + (Spell.Duration / 60000).ToString() + ":" + (Spell.Duration % 60000 / 1000).ToString("00") + " min");
            else if (Spell.Duration != 0)
                list.Add(player != null ? LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + " " + (Spell.Duration / 1000).ToString("0' sec';'Permanent.';'Permanent.'") : LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + " " + (Spell.Duration / 1000).ToString("0' sec';'Permanent.';'Permanent.'"));
            if (Spell.Frequency != 0)
                list.Add(player != null ? LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Frequency", (Spell.Frequency * 0.001).ToString("0.0")) : LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Frequency", (Spell.Frequency * 0.001).ToString("0.0")));
            if (Spell.Power != 0)
                list.Add(player != null ? LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.PowerCost", Spell.Power.ToString("0;0'%'")) : LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.PowerCost", Spell.Power.ToString("0;0'%'")));
            list.Add(player != null ? LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.CastingTime", (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'")) : LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.CastingTime", (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'")));
            if (Spell.RecastDelay > 60000)
                list.Add(player != null ? LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.RecastTime") + " " + (Spell.RecastDelay / 60000).ToString() + ":" + (Spell.RecastDelay % 60000 / 1000).ToString("00") + " min" : LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.RecastTime") + " " + (Spell.RecastDelay / 60000).ToString() + ":" + (Spell.RecastDelay % 60000 / 1000).ToString("00") + " min");
            else if (Spell.RecastDelay > 0)
                list.Add(player != null ? LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.RecastTime") + " " + (Spell.RecastDelay / 1000).ToString() + " sec" : LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.RecastTime") + " " + (Spell.RecastDelay / 1000).ToString() + " sec");
            if (Spell.Concentration != 0)
                list.Add(player != null ? LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.ConcentrationCost", Spell.Concentration) : LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.ConcentrationCost", Spell.Concentration.ToString()));
            if (Spell.Radius != 0)
                list.Add(player != null ? LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Radius", Spell.Radius) : LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Radius", Spell.Radius.ToString()));
            if (Spell.DamageType != eDamageType.Natural)
                list.Add(player != null ? LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Damage", GlobalConstants.DamageTypeToName(Spell.DamageType)) : LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Damage", GlobalConstants.DamageTypeToName(Spell.DamageType)));
            if (Spell.IsFocus)
                list.Add(player != null ? LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Focus") : LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Focus"));

            return list;
        }

        // warlock add
        public static GameSpellEffect FindEffectOnTarget(GameLiving target, string spellType, string spellName)
        {
            lock (target.EffectList)
            {
                foreach (IGameEffect fx in target.EffectList)
                {
                    if (!(fx is GameSpellEffect))
                        continue;
                    GameSpellEffect effect = (GameSpellEffect)fx;
                    if (fx is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)fx).ImmunityState)
                        continue; // ignore immunity effects

                    if (effect.SpellHandler.Spell != null && (effect.SpellHandler.Spell.SpellType == spellType) && (effect.SpellHandler.Spell.Name == spellName))
                    {
                        return effect;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Find immunity by spell type
        /// </summary>
        /// <param name="target"></param>
        /// <param name="spellType"></param>
        /// <returns>first occurance of effect in target's effect list or null</returns>
        public static GameSpellEffect FindImmunityOfSpellTypeOnTarget(GameLiving target, string spellType)
        {
            if (target == null)
                return null;

            lock (target.EffectList)
            {
                foreach (IGameEffect fx in target.EffectList)
                {
                    if (!(fx is GameSpellEffect))
                        continue;
                    GameSpellEffect effect = (GameSpellEffect)fx;
                    if (fx is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)fx).ImmunityState)
                    {
                        // we dont ignore immunity effects
                        if (effect.SpellHandler.Spell != null && (effect.SpellHandler.Spell.SpellType == spellType))
                        {
                            return effect;

                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Find effect and immunity by spell type
        /// </summary>
        /// <param name="target"></param>
        /// <param name="spellType"></param>
        /// <returns>first occurance of effect in target's effect list or null</returns>
        public static GameSpellEffect FindEffectAndImmunityOnTarget(GameLiving target, string spellType)
        {
            if (target == null)
                return null;

            lock (target.EffectList)
            {
                foreach (IGameEffect fx in target.EffectList)
                {
                    if (!(fx is GameSpellEffect))
                        continue;
                    GameSpellEffect effect = (GameSpellEffect)fx;
                    if (fx is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)fx).ImmunityState)
                    {
                        // we dont ignore immunity effects
                        if (effect.SpellHandler.Spell != null && (effect.SpellHandler.Spell.SpellType == spellType))
                        {
                            return effect;

                        }
                    }

                    else if (effect.SpellHandler.Spell != null && (effect.SpellHandler.Spell.SpellType == spellType))
                    {
                        return effect;
                    }
                }
            }
            return null;
        }




        /// <summary>
        /// Find effect by spell type
        /// </summary>
        /// <param name="target"></param>
        /// <param name="spellType"></param>
        /// <returns>first occurance of effect in target's effect list or null</returns>
        public static GameSpellEffect FindEffectOnTarget(GameLiving target, string spellType)
        {
            if (target == null)
                return null;

            lock (target.EffectList)
            {
                foreach (IGameEffect fx in target.EffectList)
                {
                    if (!(fx is GameSpellEffect))
                        continue;
                    GameSpellEffect effect = (GameSpellEffect)fx;
                    if (fx is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)fx).ImmunityState)
                        continue; // ignore immunity effects
                    if (effect.SpellHandler.Spell != null && (effect.SpellHandler.Spell.SpellType == spellType))
                    {
                        return effect;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Find effect by spell handler
        /// </summary>
        /// <param name="target"></param>
        /// <param name="spellHandler"></param>
        /// <returns>first occurance of effect in target's effect list or null</returns>
        public static GameSpellEffect FindEffectOnTarget(GameLiving target, ISpellHandler spellHandler)
        {
            lock (target.EffectList)
            {
                foreach (IGameEffect effect in target.EffectList)
                {
                    GameSpellEffect gsp = effect as GameSpellEffect;
                    if (gsp == null)
                        continue;
                    if (gsp.SpellHandler != spellHandler)
                        continue;
                    if (gsp is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)gsp).ImmunityState)
                        continue; // ignore immunity effects
                    return gsp;
                }
            }
            return null;
        }

        public static List<GameSpellEffect> FindEffectsOnTarget(GameLiving target, string spellType)
        {
            List<GameSpellEffect> result = new List<GameSpellEffect>();


            if (target == null)
                return result;

            lock (target.EffectList)
            {
                foreach (IGameEffect effect in target.EffectList)
                {
                    GameSpellEffect gsp = effect as GameSpellEffect;

                    if (gsp == null)
                        continue;

                    if (gsp is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)gsp).ImmunityState)
                        continue; // ignore immunity effects

                    if (gsp.SpellHandler.Spell != null && (gsp.SpellHandler.Spell.SpellType == spellType))
                        result.Add(gsp);
                }
            }

            return result;
        }

        /// <summary>
        /// Find effect by spell handler
        /// </summary>
        /// <param name="target"></param>
        /// <param name="spellHandler"></param>
        /// <returns>first occurance of effect in target's effect list or null</returns>
        public static GameSpellEffect FindEffectOnTarget(GameLiving target, Type spellHandler)
        {
            if (spellHandler.IsInstanceOfType(typeof(SpellHandler)) == false)
                return null;

            lock (target.EffectList)
            {
                foreach (IGameEffect effect in target.EffectList)
                {
                    GameSpellEffect gsp = effect as GameSpellEffect;
                    if (gsp == null)
                        continue;
                    if (gsp.SpellHandler.GetType().IsInstanceOfType(spellHandler) == false)
                        continue;
                    if (gsp is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)gsp).ImmunityState)
                        continue; // ignore immunity effects
                    return gsp;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns true if the target has the given static effect, false
        /// otherwise.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectType"></param>
        /// <returns></returns>
        public static IGameEffect FindStaticEffectOnTarget(GameLiving target, Type effectType)
        {
            if (target == null)
                return null;

            lock (target.EffectList)
            {
                foreach (IGameEffect effect in target.EffectList)
                    if (effect.GetType() == effectType)
                        return effect;
            }
            return null;
        }

        public static GameSpellAndImmunityEffect FindImmunityEffectOnTarget(GameLiving target, Type effectType)
        {
            if (target == null)
                return null;

            lock (target.EffectList)
            {
                foreach (IGameEffect fx in target.EffectList)
                {
                    if (!(fx is GameSpellAndImmunityEffect))
                        continue;
                    GameSpellAndImmunityEffect effect = (GameSpellAndImmunityEffect)fx;
                    if (!effect.ImmunityState)
                        continue; // ignore non-immunity effects
                    if (effect.SpellHandler.GetType() == effectType)
                        return effect;
                }
            }
            return null;
        }

        /// <summary>
        /// Find pulsing spell by spell handler
        /// </summary>
        /// <param name="living"></param>
        /// <param name="handler"></param>
        /// <returns>first occurance of spellhandler in targets' conc list or null</returns>
        public static PulsingSpellEffect FindPulsingSpellOnTarget(GameLiving living, ISpellHandler handler)
        {
            lock (living.ConcentrationEffects)
            {
                foreach (IConcentrationEffect concEffect in living.ConcentrationEffects)
                {
                    PulsingSpellEffect pulsingSpell = concEffect as PulsingSpellEffect;
                    if (pulsingSpell == null) continue;
                    if (pulsingSpell.SpellHandler == handler)
                        return pulsingSpell;
                }
                return null;
            }
        }

        #region various helpers

        /// <summary>
        /// Level mod for effect between target and caster if there is any
        /// </summary>
        /// <returns></returns>
        public virtual double GetLevelModFactor()
        {
            return 0.02;  // Live testing done Summer 2009 by Bluraven, Tolakram  Levels 40, 45, 50, 55, 60, 65, 70
        }

        /// <summary>
        /// Calculates min damage variance %
        /// </summary>
        /// <param name="target">spell target</param>
        /// <param name="min">returns min variance</param>
        /// <param name="max">returns max variance</param>
        public virtual void CalculateDamageVariance(GameLiving target, out double min, out double max)
        {
            if (m_spellLine.KeyName == GlobalSpellsLines.Item_Effects)
            {
                min = 1.0;
                max = 1.25;
                return;
            }

            if (m_spellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect)
            {
                if (UseMinVariance)
                {
                    min = 1.50;
                }
                else
                {
                    min = 1.00;
                }

                max = 1.50;

                return;
            }

            if (m_spellLine.KeyName == GlobalSpellsLines.Reserved_Spells)
            {
                min = max = 1.0;
                return;
            }

            int speclevel = 1;

            if (m_caster is GamePet)
            {
                IControlledBrain brain = (m_caster as GameNPC).Brain as IControlledBrain;
                speclevel = brain.GetLivingOwner().Level;
            }
            else if (m_caster is GamePlayer)
            {
                speclevel = ((GamePlayer)m_caster).GetModifiedSpecLevel(m_spellLine.Spec);
                if (speclevel > 50)
                    speclevel = 50;
            }
            min = 1.25;
            max = 1.25;

            if (target.Level > 0)
            {
                min = 0.25 + (speclevel - 1) / (double)target.Level;
            }

            if (speclevel - 1 > target.Level)
            {
                double overspecBonus = (speclevel - 1 - target.Level) * 0.005;
                min += overspecBonus;
                max += overspecBonus;
            }

            // add level mod
            if (m_caster is GamePlayer)
            {
                min += GetLevelModFactor() * (m_caster.Level - target.Level);
                max += GetLevelModFactor() * (m_caster.Level - target.Level);
            }
            else if (m_caster is GameNPC && ((GameNPC)m_caster).Brain is IControlledBrain)
            {
                //Get the root owner
                GameLiving owner = ((IControlledBrain)((GameNPC)m_caster).Brain).GetLivingOwner();
                if (owner != null)
                {
                    min += GetLevelModFactor() * (owner.Level - target.Level);
                    max += GetLevelModFactor() * (owner.Level - target.Level);
                }
            }

            if (max < 0.25)
                max = 0.25;
            if (min > max)
                min = max;
            if (min < 0)
                min = 0;
        }

        /// <summary>
        /// Player pet damage cap
        /// This simulates a player casting a baseline nuke with the capped damage near (but not exactly) that of the equivilent spell of the players level.
        /// This cap is not applied if the player is level 50
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public virtual double CapPetSpellDamage(double damage, GamePlayer player)
        {
            double cappedDamage = damage;

            if (player.Level < 13)
            {
                cappedDamage = 4.1 * player.Level;
            }

            if (player.Level < 50)
            {
                cappedDamage = 3.8 * player.Level;
            }

            return Math.Min(damage, cappedDamage);
        }


        /// <summary>
        /// Put a calculated cap on NPC damage to solve a problem where an npc is given a high level spell but needs damage
        /// capped to the npc level.  This uses player spec nukes to calculate damage cap.
        /// NPC's level 50 and above are not capped
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public virtual double CapNPCSpellDamage(double damage, GameNPC npc)
        {
            if (npc.Level < 50)
            {
                return Math.Min(damage, 4.7 * npc.Level);
            }

            return damage;
        }
        const string DAMAGE_UPDATE_TICK = "damage_Update_Tick";
        /// <summary>
        /// Calculates the base 100% spell damage which is then modified by damage variance factors
        /// </summary>
        /// <returns></returns>
        public virtual double CalculateDamageBase(GameLiving target)
        {

            double spellDamage = 0;

            if (Caster != null && Caster.TempProperties.getProperty<int>(DAMAGE_UPDATE_TICK) > 0)
            {
                spellDamage = (double)Caster.TempProperties.getProperty<int>(DAMAGE_UPDATE_TICK);
            }
            else
                spellDamage = Spell.Damage;

            GamePlayer player = Caster as GamePlayer;

            // For pets the stats of the owner have to be taken into account.

            if (Caster is GameNPC && ((Caster as GameNPC).Brain) is IControlledBrain)
            {
                player = (((Caster as GameNPC).Brain) as IControlledBrain).Owner as GamePlayer;
            }

            if (player != null)
            {
                if (Caster is GamePet)
                {
                    // There is no reason to cap pet spell damage if it's being scaled anyway.
                    if (ServerProperties.Properties.PET_SCALE_SPELL_MAX_LEVEL == 0)
                        spellDamage = CapPetSpellDamage(spellDamage, player);
                    spellDamage *= (((Caster as GamePet).Intelligence + 200) / 275.0);
                }
                int StyleSpellLevelReduce = 0;
                if (SpellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect)
                {


                    // ///////test Spelldamage Reductin Ply vs Ply
                    if (Caster.Level < target.Level && Caster is GamePlayer && target is GameLiving)
                    {
                        StyleSpellLevelReduce = (target.Level - Caster.Level);

                        if (StyleSpellLevelReduce <= 0)
                            StyleSpellLevelReduce = 0;
                    }
                }
                double procDamge = 0;
                double WeaponSkill = player.GetWeaponSkill(player.AttackWeapon);
                WeaponSkill /= 5;
                procDamge = ((WeaponSkill * 0.1) - (StyleSpellLevelReduce));

                if (procDamge > 0)
                {
                    spellDamage += procDamge;
                }

                if (player.CharacterClass.ManaStat != eStat.UNDEFINED
                    && SpellLine.KeyName != GlobalSpellsLines.Combat_Styles_Effect
                    && m_spellLine.KeyName != GlobalSpellsLines.Mundane_Poisons
                    && SpellLine.KeyName != GlobalSpellsLines.Item_Effects
                    && player.IsCharcterClass(eCharacterClass.MaulerAlb) == false
                    && player.IsCharcterClass(eCharacterClass.MaulerMid) == false
                    && player.IsCharcterClass(eCharacterClass.MaulerHib) == false
                    && player.IsCharcterClass(eCharacterClass.Vampiir) == false
                      //procs
                      && Spell.SpellType != "ValkyrieOffensiveProc" && Spell.SpellType != "DefensiveProc"
                        && Spell.SpellType != "OffensiveProc" && Spell.SpellType != "ScarabProc"
                        && Spell.SpellType != "ScarabProc2" && Spell.SpellType != "TraitorsDaggerProc"
                        && Spell.SpellType != "UnyeldingProc" && Spell.SpellType != "UnyeldingProc")
                {

                    int manaStatValue = player.GetModified((eProperty)player.CharacterClass.ManaStat);


                    spellDamage *= (manaStatValue + 200) / 275.0;


                }

                else if (Caster is GameNPC)
                {
                    var npc = (GameNPC)Caster;
                    int manaStatValue = npc.GetModified(eProperty.Intelligence);
                    spellDamage = CapNPCSpellDamage(spellDamage, npc) * (manaStatValue + 200) / 275.0;
                }

                if (spellDamage < 0)
                    spellDamage = 0;


            }
            return spellDamage;
        }

        /// <summary>
        /// Calculates the chance that the spell lands on target
        /// can be negative or above 100%
        /// </summary>
        /// <param name="target">spell target</param>
        /// <returns>chance that the spell lands on target</returns>
        public virtual int CalculateToHitChance(GameLiving target)
        {
            int spellLevel = Spell.Level;

            GameLiving caster = null;
            if (m_caster is GameNPC && (m_caster as GameNPC).Brain is ControlledNpcBrain)
            {
                caster = ((ControlledNpcBrain)((GameNPC)m_caster).Brain).Owner;
            }
            else
            {
                caster = m_caster;
            }

            int spellbonus = caster.GetModified(eProperty.SpellLevel);
            spellLevel += spellbonus;

            GamePlayer playerCaster = caster as GamePlayer;

            if (playerCaster != null)
            {
                if (spellLevel > playerCaster.MaxLevel)
                {
                    spellLevel = playerCaster.MaxLevel;
                }
            }

            GameSpellEffect effect = FindEffectOnTarget(m_caster, "HereticPiercingMagic");
            if (effect != null)
            {
                spellLevel += (int)effect.Spell.Value;
            }

            if (playerCaster != null && (m_spellLine.KeyName == GlobalSpellsLines.Combat_Styles_Effect || m_spellLine.KeyName.StartsWith(GlobalSpellsLines.Champion_Spells)))
            {
                spellLevel = Math.Min(playerCaster.MaxLevel, target.Level);
            }

            int bonustohit = m_caster.GetModified(eProperty.ToHitBonus);

            //Piercing Magic affects to-hit bonus too
            GameSpellEffect resPierce = SpellHandler.FindEffectOnTarget(m_caster, "PenetrateResists");
            if (resPierce != null)
                bonustohit += (int)resPierce.Spell.Value;

            /*
            http://www.camelotherald.com/news/news_article.php?storyid=704

            Q: Spell resists. Can you give me more details as to how the system works?

            A: Here's the answer, straight from the desk of the spell designer:

            "Spells have a factor of (spell level / 2) added to their chance to hit. (Spell level defined as the level the spell is awarded, chance to hit defined as
            the chance of avoiding the "Your target resists the spell!" message.) Subtracted from the modified to-hit chance is the target's (level / 2).
            So a L50 caster casting a L30 spell at a L50 monster or player, they have a base chance of 85% to hit, plus 15%, minus 25% for a net chance to hit of 75%.
            If the chance to hit goes over 100% damage or duration is increased, and if it goes below 55%, you still have a 55% chance to hit but your damage
            or duration is penalized. If the chance to hit goes below 0, you cannot hit at all. Once the spell hits, damage and duration are further modified
            by resistances.

            Note:  The last section about maintaining a chance to hit of 55% has been proven incorrect with live testing.  The code below is very close to live like.
            - Tolakram
             */




            int hitchance = 85 + ((spellLevel - target.Level) / 2) + bonustohit;

            // ///////test Spelldamage Reductin Ply vs Ply
            //  if (caster.Level < (target.Level) && caster is GamePlayer && target is GamePlayer)
            //  {
            //      hitchance -= (target.Level - caster.Level);
            //  }

            if (!(caster is GamePlayer && target is GamePlayer))
            {
                hitchance -= (int)(m_caster.GetConLevel(target) * ServerProperties.Properties.PVE_SPELL_CONHITPERCENT);
                hitchance += Math.Max(0, target.Attackers.Count - 1) * ServerProperties.Properties.MISSRATE_REDUCTION_PER_ATTACKERS;
            }
            if (caster is GamePlayer && target is GamePlayer)
            {
                hitchance -= (int)(m_caster.GetConLevel(target) * ServerProperties.Properties.PVP_SPELL_CONHITPERCENT);
                hitchance += Math.Max(0, target.Attackers.Count - 1) * ServerProperties.Properties.PVP_MISSRATE_REDUCTION_PER_ATTACKERS;
            }
            // [Freya] Nidel: Harpy Cloak : They have less chance of landing melee attacks, and spells have a greater chance of affecting them.
            if ((target is GamePlayer))
            {
                GameSpellEffect harpyCloak = FindEffectOnTarget(target, "HarpyFeatherCloak");
                if (harpyCloak != null)
                {
                    hitchance += (int)((hitchance * harpyCloak.Spell.Value) * 0.01);
                }
            }

            return hitchance;
        }

        /// <summary>
        /// Calculates damage to target with resist chance and stores it in ad
        /// </summary>
        /// <param name="target">spell target</param>
        /// <returns>attack data</returns>
        public AttackData CalculateDamageToTarget(GameLiving target)
        {
            return CalculateDamageToTarget(target, 1);
        }


        /// <summary>
        /// Adjust damage based on chance to hit.
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="hitChance"></param>
        /// <returns></returns>
        public virtual int AdjustDamageForHitChance(int damage, int hitChance)
        {
            int adjustedDamage = damage;

            if (hitChance < 55)
            {
                adjustedDamage += (int)(adjustedDamage * (hitChance - 55) * ServerProperties.Properties.SPELL_HITCHANCE_DAMAGE_REDUCTION_MULTIPLIER * 0.01);
            }

            return Math.Max(adjustedDamage, 1);
        }

        /// <summary>
        /// Holds the AttackData object of last attack
        /// </summary>
        public const string LAST_ATTACK_DAMAGE = "LAST_ATTACK_DAMAGE";
        protected bool capForDamage = false;
        /// <summary>
        /// Calculates damage to target with resist chance and stores it in ad
        /// </summary>
        /// <param name="target">spell target</param>
        /// <param name="effectiveness">value from 0..1 to modify damage</param>
        /// <returns>attack data</returns>
        public virtual AttackData CalculateDamageToTarget(GameLiving target, double effectiveness)
        {
            AttackData ad = new AttackData();
            ad.Attacker = m_caster;
            ad.Target = target;
            ad.AttackType = AttackData.eAttackType.Spell;
            ad.SpellHandler = this;
            ad.AttackResult = GameLiving.eAttackResult.HitUnstyled;

            double minVariance;
            double maxVariance;

            CalculateDamageVariance(target, out minVariance, out maxVariance);
            double spellDamage = CalculateDamageBase(target);


            //Master of Magery Ability by Elcotek
            GamePlayer PetOwner = null;

            if (m_caster is GameNPC && ((m_caster as GameNPC).Brain) is IControlledBrain)
            {
                PetOwner = (((m_caster as GameNPC).Brain) as IControlledBrain).Owner as GamePlayer;
            }
            if (PetOwner != null)
            {
                MasteryOfMageryAbility mom = PetOwner.GetAbility<MasteryOfMageryAbility>();

                if (Caster is GamePet && mom != null && m_caster is NecromancerPet)
                {
                    //log.DebugFormat("pet mom = {0}", mom.Amount);
                    effectiveness += mom.Amount * 0.01;
                }
            }
            else if (m_caster is GamePlayer)
            {
                MasteryOfMageryAbility mom = m_caster.GetAbility<MasteryOfMageryAbility>();
                if (mom != null)
                {
                    //log.DebugFormat("player mom = {0}", mom.Amount);
                    effectiveness += mom.Amount * 0.001;
                }
            }


            if (m_caster is GamePlayer)
            {
                if (Spell.IsItemSpell == false)
                {
                    effectiveness += m_caster.GetModified(eProperty.SpellDamage) * 0.01;
                }
            }

            // Apply casters effectiveness
            spellDamage *= m_caster.Effectiveness;

            int finalDamage = Util.Random((int)(minVariance * spellDamage), (int)(maxVariance * spellDamage));

            // Live testing done Summer 2009 by Bluraven, Tolakram  Levels 40, 45, 50, 55, 60, 65, 70
            // Damage reduced by chance < 55, no extra damage increase noted with hitchance > 100
            int hitChance = CalculateToHitChance(ad.Target);
            finalDamage = AdjustDamageForHitChance(finalDamage, hitChance);

            // apply spell effectiveness
            finalDamage = (int)(finalDamage * effectiveness);


            double relicBonus = 0;


            if (Properties.ENABLE_RELIC_DAMAGE_BONUS)
            {
                relicBonus = RelicMgr.GetRelicBonusModifier(m_caster.Realm, eRelicType.Magic);
            }
            // log.ErrorFormat("Relic bonus {0}", relicBonus);

            if ((m_caster is GamePlayer || (m_caster is GameNPC && (m_caster as GameNPC).Brain is IControlledBrain && m_caster.Realm != 0)))
            {
                if (target is GamePlayer)
                    finalDamage = (int)((double)finalDamage * ((ServerProperties.Properties.PVP_SPELL_DAMAGE - 0.2) + relicBonus));
                else if (target is GameNPC)
                    finalDamage = (int)((double)finalDamage * ((ServerProperties.Properties.PVE_SPELL_DAMAGE - 0.2) + relicBonus));
            }

            //damage and conlevel calculation
            double conLevel;
            double damageMul = 0;

            if (ad.Target != null && ad.Attacker is GamePlayer && ad.Target is GameNPC && ad.Target is GameKeepDoor == false && ad.Target is GamePet == false
                && ad.Target is GameKeepComponent == false && ad.Target is GameFont == false && ad.Target is GameSiegeWeapon == false && ad.Target is GameStorm == false)
            {
                conLevel = GameObject.GetConLevel(ad.Attacker.Level, ad.Target.Level);


                // Console.WriteLine("conlevel2 = {0}", conLevel);

                if (conLevel < 0)
                {

                    if (conLevel == -1)
                        damageMul = 0.4;
                    if (conLevel == -2)
                        damageMul = 0.6;
                    if (conLevel <= -3)
                        damageMul = 0.8;
                    //log.ErrorFormat("conLevel1 = {0}", finalDamage);
                    finalDamage += Convert.ToInt16(damageMul * finalDamage);
                    // log.ErrorFormat("conLevel2 = {0}", finalDamage);
                }
            }


            // Well the PenetrateResistBuff is NOT ResistPierce
            GameSpellEffect penPierce = SpellHandler.FindEffectOnTarget(m_caster, "PenetrateResists");
            if (penPierce != null)
            {
                finalDamage = (int)(finalDamage * (1.0 + penPierce.Spell.Value / 100.0));
            }

            int cdamage = 0;

            //AttackDamageConversion spell
            if (finalDamage > 0)
            {
                GameSpellEffect AttackDamageConversion = SpellHandler.FindEffectOnTarget(m_caster, "AttackDamageConversion");

                if (AttackDamageConversion != null)
                    m_caster.TempProperties.setProperty(LAST_ATTACK_DAMAGE, finalDamage);
                //  else 
                //  if (Caster.TempProperties.getProperty<int>(SpellHandler.LAST_ATTACK_DAMAGE) > 0)
                //   m_caster.TempProperties.removeProperty(SpellHandler.LAST_ATTACK_DAMAGE);
            }

            if (finalDamage <= 0)
            {
                finalDamage = 0;
            }
            eDamageType damageType = DetermineSpellDamageType();
            //log.ErrorFormat("damagetype ab: {0}", damageType.ToString());
            #region Resists
            eProperty property = target.GetResistTypeForDamage(damageType);
            // The Daoc resistsystem is since 1.65 a 2category system.
            // - First category are Item/Race/Buff/RvrBanners resists that are displayed in the characteroverview.
            // - Second category are resists that are given through RAs like avoidance of magic, brilliance aura of deflection.
            //   Those resist affect ONLY the spelldamage. Not the duration, not the effectiveness of debuffs.
            // so calculation is (finaldamage * Category1Modification) * Category2Modification
            // -> Remark for the future: VampirResistBuff is Category2 too.
            // - avi

            #region Primary Resists
            int primaryResistModifier = ad.Target.GetResist(damageType);

            /* Resist Pierce
             * Resipierce is a special bonus which has been introduced with TrialsOfAtlantis.
             * At the calculation of SpellDamage, it reduces the resistance that the victim recives
             * through ITEMBONUSES for the specified percentage.
             * http://de.daocpedia.eu/index.php/Resistenz_durchdringen (translated)
             */
            int resiPierce = Caster.GetModified(eProperty.ResistPierce);
            GamePlayer ply = Caster as GamePlayer;
            if (resiPierce > 0) // && Spell.SpellType != "Archery")
            {
                //substract max ItemBonus of property of target, but atleast 0.
                primaryResistModifier -= Math.Max(0, Math.Min(ad.Target.ItemBonus[(int)property], resiPierce));
            }
            #endregion

            #region Secondary Resists
            //Using the resist BuffBonusCategory2 - its unused in ResistCalculator
            int secondaryResistModifier = target.SpecBuffBonusCategory[(int)property];

            /*Variance by Memories of War
             * - Memories of War: Upon reaching level 41, the Hero, Warrior and Armsman
             * will begin to gain more magic resistance (spell damage reduction only)
             * as they progress towards level 50. At each level beyond 41 they gain
             * 2%-3% extra resistance per level. At level 50, they will have the full 15% benefit.
             * from http://www.camelotherald.com/article.php?id=208
             *
             * - assume that "spell damage reduction only" indicates resistcategory 2
             */

            if (ad.Target is GamePlayer && (ad.Target as GamePlayer).HasAbility(Abilities.MemoriesOfWar) && ad.Target.Level >= 40)
            {
                int levelbonus = Math.Min(target.Level - 40, 10);
                secondaryResistModifier += (int)((levelbonus * 0.1 * 15));
            }

            if (secondaryResistModifier > 80)
                secondaryResistModifier = 80;
            #endregion

            int resistModifier = 0;


            #endregion

      
            //primary resists
            resistModifier += (int)(finalDamage * (double)primaryResistModifier * -0.01);
            //secondary resists
            resistModifier += (int)((finalDamage + (double)resistModifier) * (double)secondaryResistModifier * -0.01);
            //apply resists
            finalDamage += resistModifier;


            //Heretic Damage Caps
            if (Caster != null && Caster.TempProperties.getProperty<int>(DAMAGE_UPDATE_TICK) > 0)
            {

                int resistModifiers = resistModifier;
                if (HereticDamageCap(finalDamage, effectiveness))
                {
                  
                    finalDamage = (int)DamageCap(effectiveness);
                    resistModifier = resistModifiers;

                    capForDamage = true;
                }
                else
                    capForDamage = false;
            }





            // Apply damage cap (this can be raised by effectiveness)
            else if (finalDamage > DamageCap(effectiveness))
            {
                int resistModifiers = resistModifier;
                finalDamage = (int)DamageCap(effectiveness);
                resistModifier = resistModifiers;

            }

            if (finalDamage < 0)
                finalDamage = 0;

            int criticalchance = (m_caster.SpellCriticalChance);


            //Falcon Eye Ability Handling for new Archery by Elcotek
            //http://camelot.allakhazam.com/ability.html?cabil=34 Increases the chance of dealing a critical hit with archery by the listed percentage amount.
            int BaseChance = ServerProperties.Properties.BOW_BASE_CRITCHANCE; //Base chance is 10% on live 


            if (m_caster is GamePlayer)
            {
                RAPropertyEnhancer FalconEye = ((GamePlayer)m_caster).GetAbility<FalconsEyeAbility>();
                switch ((eCharacterClass)((GamePlayer)m_caster).CharacterClass.ID)
                {
                    case eCharacterClass.Hunter:
                    case eCharacterClass.Ranger:
                    case eCharacterClass.Scout:
                        {

                            if (FalconEye != null && this is ArrowSpellHandler && m_caster.TempProperties.getProperty<bool>(GameLiving.WasCriticalShot) == false)//no extra Chance for Citshots
                            {
                                // log.Error("FalconEye ok");
                                BaseChance += FalconEye.GetAmountForLevel(((GamePlayer)m_caster).CalculateSkillLevel(FalconEye));

                                criticalchance = BaseChance; //Base Chance + Falcon Eye Value
                                break;
                            }
                            else
                            {
                                //log.Error("FalconEye aus");
                                criticalchance = BaseChance; //Base chance is 10% on live 
                                break;
                            }
                        }
                }
            }



            if (Util.Chance(Math.Min(50, criticalchance)) && (finalDamage >= 1))
            {
                int critmax = (ad.Target is GamePlayer) ? finalDamage / 2 : finalDamage;
                cdamage = Util.Random(finalDamage / 10, critmax); //think min crit is 10% of damage
            }
            //Andraste
            if (ad.Target is GamePlayer && ad.Target.GetModified(eProperty.Conversion) > 0)
            {
                int manaconversion = (int)Math.Round(((double)ad.Damage + (double)ad.CriticalDamage) * (double)ad.Target.GetModified(eProperty.Conversion) / 200);
                //int enduconversion=(int)Math.Round((double)manaconversion*(double)ad.Target.MaxEndurance/(double)ad.Target.MaxMana);
                int enduconversion = (int)Math.Round(((double)ad.Damage + (double)ad.CriticalDamage) * (double)ad.Target.GetModified(eProperty.Conversion) / 200);
                if (ad.Target.Mana + manaconversion > ad.Target.MaxMana) manaconversion = ad.Target.MaxMana - ad.Target.Mana;
                if (ad.Target.Endurance + enduconversion > ad.Target.MaxEndurance) enduconversion = ad.Target.MaxEndurance - ad.Target.Endurance;
                if (manaconversion < 1) manaconversion = 0;
                if (enduconversion < 1) enduconversion = 0;
                if (manaconversion >= 1) (ad.Target as GamePlayer).Out.SendMessage("You gain " + manaconversion.ToString() + " power points", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                if (enduconversion >= 1) (ad.Target as GamePlayer).Out.SendMessage("You gain " + enduconversion.ToString() + " endurance points", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                ad.Target.Endurance += enduconversion; if (ad.Target.Endurance > ad.Target.MaxEndurance) ad.Target.Endurance = ad.Target.MaxEndurance;
                ad.Target.Mana += manaconversion; if (ad.Target.Mana > ad.Target.MaxMana) ad.Target.Mana = ad.Target.MaxMana;
            }

            ad.Damage = finalDamage;

            if (this is ArrowSpellHandler && Spell.LifeDrainReturn == 1)//no extra Chance for Citshots
            {
                ad.CriticalDamage = 0;
            }
            else
            {
                ad.CriticalDamage = cdamage;
            }
            ad.DamageType = damageType;
            ad.Modifier = resistModifier;

            // Attacked living may modify the attack data.  Primarily used for keep doors and components.
            ad.Target.ModifyAttack(ad);

            m_lastAttackData = ad;
            return ad;
        }

        protected virtual bool HereticDamageCap(int finalDamage, double effectiveness)
        {

            if (finalDamage > 0 && effectiveness > 0 && finalDamage >= ((int)DamageCap(effectiveness) * 2))
                return true;

            return false;
        }

        public virtual double DamageCap(double effectiveness)
        {
            return Spell.Damage * 3.0 * effectiveness;
        }

        /// <summary>
        /// What damage type to use.  Overriden by archery
        /// </summary>
        /// <returns></returns>
        public virtual eDamageType DetermineSpellDamageType()
        {
            if (Spell.SpellType == "SummonTheurgistPet")
            {
                //log.ErrorFormat("No resist found for damage type {0} on living {1}!", (int)damageType, Name);
                return 0;
            }
            if (Caster is GamePlayer && Properties.Allow_New_Archery_Check_Arrows && (Caster as GamePlayer).IsArcherClass())
            {
                if (Properties.Allow_New_Archery_Check_Arrows)
                {

                    if (Spell.LifeDrainReturn <= 4)
                    {
                        return ArrowDamageType;
                    }
                    else
                    {
                        return Spell.DamageType;
                    }
                }
                else
                    return Spell.DamageType;
            }
            else
           
            return Spell.DamageType;
        }


        /// <summary>
        /// Sends damage text messages but makes no damage
        /// </summary>
        /// <param name="ad"></param>
        public virtual void SendDamageMessages(AttackData ad)
        {
            GamePlayer this_necro_owner = null;
            GamePlayer this_npc_owner = null;
            GamePlayer this_pet_owner = null;

            string modmessage = String.Empty;
            if (ad.Modifier > 0)
                modmessage = " (+" + ad.Modifier.ToString() + ")";
            if (ad.Modifier < 0)
                modmessage = " (" + ad.Modifier.ToString() + ")";

            if (ad.Target.Name == "VolleyTarget")
            {
                return;
            }
           
            if (Caster is GamePlayer)
            {
                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YouHitTheForDamage", ad.Target.Name, ad.Damage.ToString(), modmessage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
               
            }
            if (ad.CriticalDamage > 0)

                if (Caster is GamePlayer)
                {
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YouCriticallyHitForAdditionalDamage", ad.CriticalDamage.ToString()), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                }

            if (Caster is NecromancerPet && (Caster as GameNPC).Brain is MobPetBrain == false && (Caster as GameNPC).Brain is IControlledBrain)
            {
                this_necro_owner = ((Caster as NecromancerPet).Brain as IControlledBrain).GetPlayerOwner();
                if (this_necro_owner != null)
                {
                    (this_necro_owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((this_necro_owner as GamePlayer).Client.Account.Language, "SpellHandler.YouHitTheForDamage", ad.Target.Name, ad.Damage.ToString(), modmessage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

                    if (ad.CriticalDamage > 0)
                        (this_necro_owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((this_necro_owner as GamePlayer).Client.Account.Language, "SpellHandler.YouCriticallyHitForAdditionalDamage", ad.CriticalDamage.ToString()), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                }
                else
                {
                    //else send nothing
                }
            }
            // MessageToCaster(string.Format("You hit {0} for {1}{2} damage!", ad.Target.GetName(0, false), ad.Damage, modmessage), eChatType.CT_YouHit);

            else if (Caster is GameNPC && (Caster as GameNPC).Brain is MobPetBrain == false && (Caster as GameNPC).Brain is IControlledBrain)
            {
                this_npc_owner = ((Caster as GameNPC).Brain as IControlledBrain).GetPlayerOwner();
                if (this_npc_owner != null && (Caster as GameNPC).Brain is IControlledBrain)
                {

                    (this_npc_owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((this_npc_owner as GamePlayer).Client.Account.Language, "SpellHandler.YourPetHitTheForDamage", ad.Attacker.Name, ad.Target.Name, ad.Damage.ToString(), modmessage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

                    if (ad.CriticalDamage > 0)
                        (this_npc_owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((this_npc_owner as GamePlayer).Client.Account.Language, "SpellHandler.YourPetHitTheForCriticalDamage", ad.Attacker.Name, ad.CriticalDamage.ToString()), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

                    //target is player pet
                    if (ad.Target != null && (ad.Target as GameNPC) != ad.Attacker && ad.Target is GameNPC && (ad.Target as GameNPC).Brain is MobPetBrain == false && (ad.Target as GameNPC).Brain is IControlledBrain)
                    {
                        this_pet_owner = ((ad.Target as GameNPC).Brain as IControlledBrain).GetPlayerOwner();

                        if (this_pet_owner != null && this_pet_owner != this_npc_owner && (ad.Target as GameNPC).Brain is IControlledBrain) //Pets attack target is Player pet
                        {

                            this_pet_owner.Out.SendMessage(LanguageMgr.GetTranslation((this_pet_owner as GamePlayer).Client.Account.Language, "GamePlayer.Attack.HitsYour",
                                    ad.Attacker.GetName(0, true, (this_pet_owner as GamePlayer).Client.Account.Language, (ad.Attacker as GameNPC)), ad.Target.Name, ad.Damage.ToString(), modmessage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);

                            if (ad.CriticalDamage > 0)
                                this_pet_owner.Out.SendMessage(LanguageMgr.GetTranslation((this_pet_owner as GamePlayer).Client.Account.Language, "GamePlayer.Attack.HitsYouCritical",
                                    ad.Attacker.GetName(0, true, (this_pet_owner as GamePlayer).Client.Account.Language, (ad.Attacker as GameNPC)), ad.CriticalDamage.ToString()), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                        }

                    }
                    else
                    {
                        //else send nothing
                    }
                }
            }
        }

        /// <summary>
        /// Make damage to target and send spell effect but no messages
        /// </summary>
        /// <param name="ad"></param>
        /// <param name="showEffectAnimation"></param>
        public virtual void DamageTarget(AttackData ad, bool showEffectAnimation, bool wasBlocked)
        {
            DamageTarget(ad, showEffectAnimation, 0x14, wasBlocked); //spell damage attack result
        }

        /// <summary>
        /// Make damage to target and send spell effect but no messages
        /// </summary>
        /// <param name="ad"></param>
        /// <param name="showEffectAnimation"></param>
        /// <param name="attackResult"></param>
        public virtual void DamageTarget(AttackData ad, bool showEffectAnimation, int attackResult, bool wasBlocked)
        {
            ad.AttackResult = GameLiving.eAttackResult.HitUnstyled;
            if (showEffectAnimation)
            {


                SendEffectAnimation(ad.Target, 0, false, 1);

            }

            if (ad.Damage > 0 && wasBlocked == false)
            {
                foreach (GamePlayer player in ad.Target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    player.Out.SendCombatAnimation(ad.Attacker, ad.Target, 0, 0, 0, 0, (byte)attackResult, ad.Target.HealthPercent);

                // send animation before dealing damage else dead livings show no animation
                ad.Target.OnAttackedByEnemy(ad);
                ad.Attacker.DealDamage(ad);

            }
            //block no damage
            else if (ad.Attacker is GamePlayer && ad.Target is GamePlayer && wasBlocked == true && ad.Damage > 0)
            {
                ad.Damage = 0;
                foreach (GamePlayer player in ad.Target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    player.Out.SendCombatAnimation(ad.Attacker, ad.Target, 0, 0, 0, 0, (byte)attackResult, ad.Target.HealthPercent);

                // send animation before dealing damage else dead livings show no animation
                ad.Target.OnAttackedByEnemy(ad);
            }
            // send animation before dealing damage else dead livings show no animation
            // ad.Target.OnAttackedByEnemy(ad);

            if (ad.Damage == 0 && wasBlocked == false && ad.Target is GameNPC)
            {
                IOldAggressiveBrain aggroBrain = ((GameNPC)ad.Target).Brain as IOldAggressiveBrain;
                if (aggroBrain != null)
                    aggroBrain.AddToAggroList(Caster, 1);
            }

            m_lastAttackData = ad;
        }

        #endregion

        #region saved effects
        public virtual PlayerXEffect GetSavedEffect(GameSpellEffect effect)
        {
            return null;
        }

        public virtual void OnEffectRestored(GameSpellEffect effect, int[] vars)
        { }

        public virtual int OnRestoredEffectExpires(GameSpellEffect effect, int[] vars, bool noMessages)
        {
            return 0;
        }
        #endregion

    }
}
