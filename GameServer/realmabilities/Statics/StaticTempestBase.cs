using System;
using System.Collections;
using DOL.Database;
using DOL.GS;
using DOL.GS.Spells;
using DOL.Events;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities.Statics 
{
    public class StaticTempestBase : GenericBase  
    {
		protected override string GetStaticName() {return "Static Tempest";}
		protected override ushort GetStaticModel() {return 2654;}
		protected override ushort GetStaticEffect() {return 7032;}
		private readonly DBSpell dbs;
		private readonly Spell   s;
		private readonly SpellLine sl;
		public StaticTempestBase (int stunDuration) 
        {
            dbs = new DBSpell
            {
                Name = GetStaticName(),
                Icon = GetStaticEffect(),
                ClientEffect = GetStaticEffect(),
                Damage = 0,
                DamageType = (int)eDamageType.Energy,
                Target = "Enemy",
                Radius = 0,
                Type = "UnresistableStun",
                Value = 0,
                Duration = stunDuration,
                Pulse = 0,
                PulsePower = 0,
                Power = 0,
                CastTime = 0,
                Range = WorldMgr.VISIBILITY_DISTANCE
            };
            s = new Spell(dbs,1);
			sl = new SpellLine("RAs","RealmAbilitys","RealmAbilitys",true);
		}
		protected override void CastSpell (GameLiving target) {
            if (!target.IsAlive) return;
			if (GameServer.ServerRules.IsAllowedToAttack(m_caster, target, true))
            {
				ISpellHandler stun = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
				stun.StartSpell(target);
			}
		}
	}
}

