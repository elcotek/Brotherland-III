


namespace DOL.GS.Spells
{
    /// <summary>
    /// 
    /// </summary>
    [SpellHandlerAttribute("DirectDamagePoison")]
    public class DirectDamagePoisoneSpellHandler : DirectDamageSpellHandler
    {
        public override double CalculateDamageBase(GameLiving target)
        {
            return Spell.Damage;
        }
        public const int spell1 = 30040;
        int RandProc = Util.Random(1, 2);

        /// <summary>
        /// Calculates damage to target with resist chance and stores it in ad
        /// </summary>
        /// <param name="target">spell target</param>
        /// <param name="effectiveness">value from 0..1 to modify damage</param>
        /// <returns>attack data</returns>

        public override AttackData CalculateDamageToTarget(GameLiving target, double effectiveness)
        {
            AttackData ad = base.CalculateDamageToTarget(target, effectiveness);
            if (this.SpellLine.KeyName == GlobalSpellsLines.Mundane_Poisons)
            {
                RealmAbilities.L3RAPropertyEnhancer ra = Caster.GetAbility<RealmAbilities.ViperAbility>();
                if (ra != null)
                {
                    int additional = (int)((float)ad.Damage * ((float)ra.Amount / 100));
                    ad.Damage += additional;
                }
            }

            //ad.CriticalDamage = 0; - DoTs can crit.
            return ad;
        }

        public DirectDamagePoisoneSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}