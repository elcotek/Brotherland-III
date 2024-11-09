using System;
using System.Collections;
using DOL.Database;
using DOL.GS;
using DOL.GS.Spells;
using DOL.Events;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities.Statics 
{
    public class ThornweedFieldBase : GenericBase 
    {
		protected override string GetStaticName() {return "Thornwood Field";}
		protected override ushort GetStaticModel() {return 2653;}
		protected override ushort GetStaticEffect() {return 7028;}
		private readonly DBSpell dbs;
		private readonly Spell   s;
		private readonly SpellLine sl;
		public ThornweedFieldBase (int damage) 
        {
            dbs = new DBSpell
            {
                Name = GetStaticName(),
                Icon = GetStaticEffect(),
                ClientEffect = GetStaticEffect(),
                Damage = damage,
                DamageType = (int)eDamageType.Natural,
                Target = "Enemy",
                Radius = 0,
                Type = "DamageSpeedDecreaseNoVariance",
                Value = 50,
                Duration = 5,
                Pulse = 0,
                PulsePower = 0,
                Power = 0,
                CastTime = 0,
                Range = WorldMgr.VISIBILITY_DISTANCE
            };
            s = new Spell(dbs,1);
			sl = new SpellLine("RAs","RealmAbilitys","RealmAbilitys",true);
		}
		protected override void CastSpell (GameLiving target)
        {
            if (!target.IsAlive) return;
			if (GameServer.ServerRules.IsAllowedToAttack(m_caster, target, true))
            {
				ISpellHandler snare = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
				snare.StartSpell(target);
			}
		}
	}
}
