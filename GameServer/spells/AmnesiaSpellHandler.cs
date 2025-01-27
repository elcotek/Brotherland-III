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
using DOL.Language;





namespace DOL.GS.Spells
{
    /// <summary>
    /// 
    /// </summary>
    [SpellHandlerAttribute("Amnesia")]
    public class AmnesiaSpellHandler : SpellHandler
    {
        /// <summary>
        /// Execute direct damage spell
        /// </summary>
        /// <param name="target"></param>
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        /// <summary>
        /// execute non duration spell effect on target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectiveness"></param>
        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            base.OnDirectEffect(target, effectiveness);
            if (target == null || !target.IsAlive)
                return;
            
            if (Caster.EffectList.GetOfType<MasteryofConcentrationEffect>() != null || Caster.EffectList.GetOfType<ChargeEffect>() != null)
                return;


            if (target is GamePlayer && target.EffectList.GetOfType<MasteryofConcentrationEffect>() != null || target.EffectList.GetOfType<QuickCastEffect>() != null || target.EffectList.GetOfType<ChargeEffect>() != null)
                return;


            //have to do it here because OnAttackedByEnemy is not called to not get aggro
            if (target.Realm == 0 || Caster.Realm == 0)
                target.LastAttackedByEnemyTickPvE = target.CurrentRegion.Time;
            else target.LastAttackedByEnemyTickPvP = target.CurrentRegion.Time;
            SendEffectAnimation(target, 0, false, 1);

            if (target is GamePlayer)
            {
                ((GamePlayer)target).NextCombatStyle = null;
                ((GamePlayer)target).NextCombatBackupStyle = null;
            }
            target.StopCurrentSpellcast(); //stop even if MoC or QC

            if (target is GamePlayer)
                MessageToLiving(target, LanguageMgr.GetTranslation((target as GamePlayer).Client, "Amnesia.MessageToTarget"), eChatType.CT_Spell);

            GameSpellEffect effect;

            GameSpellEffect aoemez = SpellHandler.FindEffectOnTarget(target, "AOEMesmerize");
            if (aoemez != null)
                aoemez.Cancel(false);

            effect = SpellHandler.FindEffectOnTarget(target, "Mesmerize");
            if (effect != null)
            {
                effect.Cancel(false);
                return;
            }

            if (target is GameNPC)
            {
                GameNPC npc = (GameNPC)target;
                IOldAggressiveBrain aggroBrain = npc.Brain as IOldAggressiveBrain;
                if (aggroBrain != null)
                {
                    if (Util.Chance(Spell.AmnesiaChance))
                        aggroBrain.ClearAggroList();
                }
            }
        }

        /// <summary>
        /// When spell was resisted
        /// </summary>
        /// <param name="target">the target that resisted the spell</param>
        protected override void OnSpellResisted(GameLiving target)
        {
            base.OnSpellResisted(target);
            if (Spell.CastTime == 0)
            {
                // start interrupt even for resisted instant amnesia
                target.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
            }
        }

        // constructor
        public AmnesiaSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
