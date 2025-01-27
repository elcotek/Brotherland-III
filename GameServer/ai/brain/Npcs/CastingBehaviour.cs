﻿using DOL.Events;
using DOL.GS;

namespace DOL.AI.Brain
{
    public class CastingBehaviour : IAttackBehaviour
    {
        public CastingBehaviour(GameNPC body)
        {
            Body = body;
        }

        protected GameNPC Body { get; set; }

        protected SpellLine SpellLine
        {
            get
            {
                return SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells);
            }
        }

        #region IAttackBehaviour Members

        public void Attack(GameObject target)
        {
            foreach (Spell spell in Body.HarmfulSpells)
            {
                if (target.IsAttackable && !target.HasEffect(spell))
                {
                    Body.StopMoving();
                    Body.TargetObject = target;
                    Body.TurnTo(target);
                    Body.CastSpell(spell, SpellLine, true);
                    return;
                }
            }

            Body.Notify(GameLivingEvent.CastFailed, Body);
        }

        public void Retreat()
        {
        }

        #endregion
    }
}
