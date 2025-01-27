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
using DOL.Language;
using log4net;
using System;
using System.Collections.Generic;



namespace DOL.GS.Spells
{
    /// <summary>
    /// Base class for proc spell handler
    /// </summary>
    public abstract class BaseProcSpellHandler : SpellHandler
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructs new proc spell handler
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="spell"></param>
        /// <param name="spellLine"></param>
        protected BaseProcSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine)
            : base(caster, spell, spellLine)
        {
            m_procSpellLine = SkillBase.GetSpellLine(SubSpellLineName);
            m_procSpell = SkillBase.GetSpellByID((int)spell.Value);
        }

        public const string LAST_FIRED_PROC_TICK = "LastFiredProcTick";

        /// <summary>
        /// Returns the list of attackers
        /// </summary>
        public List<GameObject> Attackers
        {
            get { return m_attackers; }
        }
        /// <summary>
        /// The objects currently attacking this living
        /// To be more exact, the objects that are in combat
        /// and have this living as target.
        /// </summary>
        protected readonly List<GameObject> m_attackers;
        /// <summary>
        /// The event type to hook on
        /// </summary>
        protected abstract DOLEvent EventType { get; }

        /// <summary>
        /// The spell line name of the proc spell
        /// </summary>
        protected abstract string SubSpellLineName { get; }


        /// <summary>
        /// The event handler of given event type
        /// </summary>
        protected abstract void EventHandler(DOLEvent e, object sender, EventArgs arguments);

        /// <summary>
        /// Proc Spelltype for items
        /// </summary>
        protected Spell m_subSpell = null;

        /// <summary>
        /// Proc Spelltype for items
        /// </summary>
        protected Spell m_SpellValue = null;

        private string subSpellName = "";

        /// <summary>
        /// Holds the proc spell
        /// </summary>
        protected Spell m_procSpell;

        /// <summary>
        /// Holds the proc spell line
        /// </summary>
        protected SpellLine m_procSpellLine;

        /// <summary>
        /// called after normal spell cast is completed and effect has to be started
        /// </summary>
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


        /// <summary>
        /// When an applied effect starts
        /// duration spells only
        /// </summary>
        /// <param name="effect"></param>
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
            GameEventMgr.AddHandler(effect.Owner, EventType, new DOLEventHandler(EventHandler));
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
            if (!noMessages)
            {
                MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, true)), eChatType.CT_SpellExpires, effect.Owner);
            }
            GameEventMgr.RemoveHandler(effect.Owner, EventType, new DOLEventHandler(EventHandler));
            return 0;
        }

        /// <summary>
        /// Determines wether this spell is better than given one
        /// </summary>
        /// <returns>true if this spell is better version than compare spell</returns>
        public override bool IsNewEffectBetter(GameSpellEffect oldeffect, GameSpellEffect neweffect)
        {
            Spell oldProcSpell = SkillBase.GetSpellByID((int)oldeffect.Spell.Value);
            Spell newProcSpell = SkillBase.GetSpellByID((int)neweffect.Spell.Value);

            if (oldProcSpell == null || newProcSpell == null)
                return true;

            // do not replace active proc with different type proc
            if (oldProcSpell.SpellType != newProcSpell.SpellType) return false;

            if (oldProcSpell.Concentration > 0) return false;

            // if the new spell does less damage return false
            if (oldProcSpell.Damage > newProcSpell.Damage) return false;

            // if the new spell is lower than the old one return false
            if (oldProcSpell.Value > newProcSpell.Value) return false;

            //makes problems for immunity effects
            if (oldeffect is GameSpellAndImmunityEffect == false || ((GameSpellAndImmunityEffect)oldeffect).ImmunityState == false)
            {
                if (neweffect.Duration <= oldeffect.RemainingTime) return false;
            }

            return true;
        }
        public override bool IsOverwritable(GameSpellEffect compare)
        {
            if (Spell.Group != 0)
                return Spell.Group == compare.Spell.Group;
            if (compare.Spell.SpellType != Spell.SpellType)
                return false;
            Spell oldProcSpell = SkillBase.GetSpellByID((int)Spell.Value);
            Spell newProcSpell = SkillBase.GetSpellByID((int)compare.Spell.Value);
            if (oldProcSpell == null || newProcSpell == null)
                return true;
            if (oldProcSpell.SpellType != newProcSpell.SpellType)
                return false;
            return true;
        }
       
        /// <summary>
        /// Delve Info
        /// </summary>
        public override IList<string> DelveInfo(GamePlayer player)
        {
           
            {
                string nameofpellType;
                switch (Spell.SpellType)
                {
                    case "OffensiveProc":
                        {
                            nameofpellType = "OffensiveProc";
                            break;
                        }
                    case "DeffensiveProc":
                        {
                            nameofpellType = "DeffensiveProc";
                            break;
                        }
                    default:
                        nameofpellType = Spell.SpellType;
                        break;
                }

                var list = new List<string>
                {

                     

                
                     



                    //                list.Add("Function: " + (string)(Spell.SpellType == "" ? "(not implemented)" : Spell.SpellType));
                        LanguageMgr.GetTranslation(player.Client.Account.Language, "ProcSpellHandler.DelveInfo.Function", (string)(nameofpellType == "" ? "(not implemented)" : nameofpellType)),

                    //                list.Add("Target: " + Spell.Target);
                    LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Target", Spell.Target)
                };

                //                if (Spell.Range != 0) list.Add("Range: " + Spell.Range);
                if (Spell.Range != 0)
                    list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Range", Spell.Range.ToString()));

                //                if (Spell.Duration >= ushort.MaxValue * 1000) list.Add("Duration: Permanent.");
                if (Spell.Duration >= ushort.MaxValue * 1000)
                    list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + " Permanent.");

                //                else if (Spell.Duration > 60000) list.Add(string.Format("Duration: {0}:{1} min", Spell.Duration / 60000, (Spell.Duration % 60000 / 1000).ToString("00")));
                else if (Spell.Duration > 60000)
                    list.Add(string.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + Spell.Duration / 60000 + ":" + (Spell.Duration % 60000 / 1000).ToString("00") + "min"));

                else if (Spell.Duration != 0) list.Add("Duration: " + (Spell.Duration / 1000).ToString("0' sec';'Permanent.';'Permanent.'"));
                if (Spell.Power != 0) list.Add("Power cost: " + Spell.Power.ToString("0;0'%'"));
                list.Add("Casting time: " + (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
                if (Spell.RecastDelay > 60000) list.Add("Recast time: " + (Spell.RecastDelay / 60000).ToString() + ":" + (Spell.RecastDelay % 60000 / 1000).ToString("00") + " min");
                else if (Spell.RecastDelay > 0) list.Add("Recast time: " + (Spell.RecastDelay / 1000).ToString() + " sec");
                if (Spell.Concentration != 0) list.Add("Concentration cost: " + Spell.Concentration.ToString());
                if (Spell.Radius != 0) list.Add("Radius: " + Spell.Radius.ToString());

                // Recursion check
                byte nextDelveDepth = (byte)(DelveInfoDepth + 1);
                if (nextDelveDepth > MAX_DELVE_RECURSION)
                {
                    list.Add("(recursion - see server logs)");
                    log.ErrorFormat("Spell delve info recursion limit reached. Source spell ID: {0}, Sub-spell ID: {1}", m_spell.ID.ToString(), m_procSpell.ID.ToString());
                }
                else
                {
                    // add subspell specific informations
                    list.Add(" "); //empty line
                    list.Add("Sub-spell informations: ");
                    list.Add(" "); //empty line
                    ISpellHandler subSpellHandler = ScriptMgr.CreateSpellHandler(player, m_procSpell, m_procSpellLine);
                    if (subSpellHandler == null)
                    {
                        return list;
                    }

                    SpellLine ProcspellLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
                    if (ProcspellLine != null)
                    {
                        Spell spell = SkillBase.FindSpell((int)Spell.Value, ProcspellLine);

                        if (spell != null)
                        {

                            m_SpellValue = spell;

                            m_subSpell = SkillBase.FindSpell(spell.SubSpellID, ProcspellLine);

                            if (m_subSpell != null)
                            {
                                subSpellName = m_subSpell.Name;

                                subSpellHandler.DelveInfoDepth = nextDelveDepth;
                                //Sub Spell of Sub Spell info
                                ISpellHandler subSpellHandler1 = ScriptMgr.CreateSpellHandler(player, m_subSpell, m_procSpellLine);
                                if (subSpellHandler1 == null)
                                {
                                    list.Add("unable to create subspell1 handler for sub Spell: '" + SubSpellLineName + "', " + spell.Value.ToString());
                                    return list;
                                }

                                subSpellHandler1.DelveInfoDepth = nextDelveDepth;
                                // Get delve info of sub-spell
                                IList<string> subSpellDelve1 = subSpellHandler1.DelveInfo(player);
                                if (subSpellDelve1.Count > 0)
                                {
                                    subSpellDelve1.RemoveAt(0);
                                    list.Add("Function Name: " + subSpellName);  //m_spell.Name
                                    list.AddRange(subSpellDelve1);
                                    list.Add(" ");  //m_spell.Name
                                }



                            }
                        }
                    }



                    // Get delve info of sub-spell
                    IList<string> subSpellDelve = subSpellHandler.DelveInfo(player);
                    if (subSpellDelve.Count > 0)
                    {
                        subSpellDelve.RemoveAt(0);
                        if (m_SpellValue != null)
                        {
                            list.Add("Funtion Name: " + m_SpellValue.Name);  //m_spell.Name
                        }
                        list.AddRange(subSpellDelve);
                    }

                }

                return list;
            }
        }
    }

    /// <summary>
    /// This class contains data for OffensiveProc spells
    /// </summary>
    [SpellHandler("OffensiveProc")]
    public class OffensiveProcSpellHandler : BaseProcSpellHandler
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The event type to hook on
        /// </summary>
        protected override DOLEvent EventType
        {
            get { return GameLivingEvent.AttackFinished; }
        }

        /// <summary>
        /// The spell line name of the proc spell
        /// </summary>
        protected override string SubSpellLineName
        {
            get { return "OffensiveProc"; }
        }

        /// <summary>
        /// Handler fired whenever effect target attacks
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        protected override void EventHandler(DOLEvent e, object sender, EventArgs arguments)
        {
            AttackFinishedEventArgs args = arguments as AttackFinishedEventArgs;

            if (args == null || args.AttackData == null || args.AttackData.AttackType == AttackData.eAttackType.Spell)
                return;

            AttackData ad = args.AttackData;

            //Hit styled and unstyled proc
            if (Spell.AmnesiaChance == 0 && ad.AttackResult != GameLiving.eAttackResult.HitUnstyled && ad.AttackResult != GameLiving.eAttackResult.HitStyle)
                return;

            //Hit style proc
            if (Spell.AmnesiaChance > 0 && ad.AttackResult != GameLiving.eAttackResult.HitStyle)
                return;


            int baseChance = Spell.Frequency / 100;

            if (ad.AttackType == AttackData.eAttackType.MeleeDualWield)
                baseChance /= 2;

            if (baseChance < 1)
                baseChance = 1;


            if (Util.Chance(baseChance))
            {
                ISpellHandler handler = ScriptMgr.CreateSpellHandler((GameLiving)sender, m_procSpell, m_procSpellLine);
                if (handler != null)
                {
                    switch (m_procSpell.Target.ToLower())
                    {
                        case "enemy":
                            handler.StartSpell(ad.Target);
                            break;
                        default:
                            handler.StartSpell(ad.Attacker);
                            break;
                    }
                }
            }
        }

        // constructor
        public OffensiveProcSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// This class contains data for DefensiveProc spells
    /// </summary>
    [SpellHandler("DefensiveProc")]
    public class DefensiveProcSpellHandler : BaseProcSpellHandler
    {
        /// <summary>
        /// The event type to hook on
        /// </summary>
        protected override DOLEvent EventType
        {
            get { return GameLivingEvent.AttackedByEnemy; }
        }

        /// <summary>
        /// The spell line name of the proc spell
        /// </summary>
        protected override string SubSpellLineName
        {
            get { return "DefensiveProc"; }
        }

        /// <summary>
        /// Handler fired whenever effect target is attacked
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        protected override void EventHandler(DOLEvent e, object sender, EventArgs arguments)
        {
            AttackedByEnemyEventArgs args = arguments as AttackedByEnemyEventArgs;
            if (args == null || args.AttackData == null || args.AttackData.AttackType == AttackData.eAttackType.Spell)
                return;

            AttackData ad = args.AttackData;
            if (ad.AttackResult != GameLiving.eAttackResult.HitUnstyled && ad.AttackResult != GameLiving.eAttackResult.HitStyle)
                return;

            int baseChance = Spell.Frequency / 100;

            if (ad.AttackType == AttackData.eAttackType.MeleeDualWield)
                baseChance /= 2;



            long changeTime;
            long LastFiredProcTic;

            //Proc spam prevent
            if (ad.Attacker != null && ad.Target is GameLiving)
            {

                LastFiredProcTic = ad.Target.TempProperties.getProperty<long>(LAST_FIRED_PROC_TICK);

                changeTime = ad.Target.CurrentRegion.Time - LastFiredProcTic;

                // 1.5 sec anti double proc time
                if (changeTime < 1500 && LastFiredProcTic > 0)
                {
                    //Console.WriteLine("last Proc time was to small");

                    return;
                }
            }

            if (baseChance < 1)
                baseChance = 1;

            if (Util.Chance(baseChance))
            {
                ISpellHandler handler = ScriptMgr.CreateSpellHandler((GameLiving)sender, m_procSpell, m_procSpellLine);
                if (handler != null)
                {
                    switch (m_procSpell.Target.ToLower())
                    {
                        case "enemy":

                            //Proc spam pevent
                            //remove old propertie proc fire time
                            ad.Target.TempProperties.removeProperty(LAST_FIRED_PROC_TICK);
                            // add new propertie proc fire time
                            ad.Target.TempProperties.setProperty(LAST_FIRED_PROC_TICK, ad.Target.CurrentRegion.Time);

                            handler.StartSpell(ad.Attacker);
                            break;
                        default:

                            //Proc spam pevent
                            //remove old propertie proc fire time
                            ad.Target.TempProperties.removeProperty(LAST_FIRED_PROC_TICK);
                            // add new propertie proc fire time
                            ad.Target.TempProperties.setProperty(LAST_FIRED_PROC_TICK, ad.Target.CurrentRegion.Time);

                            handler.StartSpell(ad.Target);
                            break;
                    }
                }
            }
        }

        // constructor
        public DefensiveProcSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    [SpellHandler("OffensiveProcPvE")]
    public class OffensiveProcPvESpellHandler : OffensiveProcSpellHandler
    {
        /// <summary>
        /// Handler fired whenever effect target is attacked
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        protected override void EventHandler(DOLEvent e, object sender, EventArgs arguments)
        {
            AttackFinishedEventArgs args = arguments as AttackFinishedEventArgs;
            if (args == null || args.AttackData == null)
                return;

            GameNPC target = args.AttackData.Target as GameNPC;

            if (target != null && !(target.Brain is IControlledBrain && ((IControlledBrain)target.Brain).GetPlayerOwner() != null))
                base.EventHandler(e, sender, arguments);
        }

        public OffensiveProcPvESpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }









    /// <summary>
    /// This class contains data for OffensiveProc spells
    /// </summary>
    [SpellHandler("ValkyrieOffensiveProc")]
    public class ValkyrieOffensiveProcSpellHandler : BaseProcSpellHandler
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The event type to hook on
        /// </summary>
        protected override DOLEvent EventType
        {
            get { return GameLivingEvent.AttackFinished; }
        }

        /// <summary>
        /// The spell line name of the proc spell
        /// </summary>
        protected override string SubSpellLineName
        {
            get { return "OffensiveProc"; }
        }

        /// <summary>
        /// Handler fired whenever effect target attacks
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        protected override void EventHandler(DOLEvent e, object sender, EventArgs arguments)
        {
            AttackFinishedEventArgs args = arguments as AttackFinishedEventArgs;

            if (args == null || args.AttackData == null || args.AttackData.AttackType == AttackData.eAttackType.Spell)
                return;

            AttackData ad = args.AttackData;

            //Hit styled and unstyled proc
            if (Spell.AmnesiaChance == 0 && ad.AttackResult != GameLiving.eAttackResult.HitUnstyled && ad.AttackResult != GameLiving.eAttackResult.HitStyle)
                return;

            //Hit style proc
            if (Spell.AmnesiaChance > 0 && ad.AttackResult != GameLiving.eAttackResult.HitStyle)
                return;


            int baseChance = Spell.Frequency / 100;

            if (ad.AttackType == AttackData.eAttackType.MeleeDualWield)
                baseChance /= 2;

            if (baseChance < 1)
                baseChance = 1;


            if (Util.Chance(baseChance))
            {
                ISpellHandler handler = ScriptMgr.CreateSpellHandler((GameLiving)sender, m_procSpell, m_procSpellLine);
                if (handler != null)
                {
                    switch (m_procSpell.Target.ToLower())
                    {
                        case "enemy":
                            handler.StartSpell(ad.Target);
                            break;
                        default:
                            handler.StartSpell(ad.Attacker);
                            break;
                    }
                }
            }
        }

        // constructor
        public ValkyrieOffensiveProcSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}










