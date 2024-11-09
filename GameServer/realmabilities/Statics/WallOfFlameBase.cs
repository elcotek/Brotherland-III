using System;
using System.Collections;
using DOL.Database;
using DOL.GS;
using DOL.GS.Spells;
using DOL.Events;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities.Statics
{
	public class WallOfFlameBase : GenericBase
	{
		protected override string GetStaticName() { return "Wall Of Flame"; }
		protected override ushort GetStaticModel() { return 2651; }
		protected override ushort GetStaticEffect() { return 7050; }
		private readonly DBSpell dbs;
		private readonly Spell s;
		private readonly SpellLine sl;
		public WallOfFlameBase(int damage)
		{
            dbs = new DBSpell
            {
                Name = GetStaticName(),
                Icon = GetStaticEffect(),
                ClientEffect = GetStaticEffect(),
                Damage = damage,
                DamageType = (int)eDamageType.Heat,
                Target = "Enemy",
                Radius = 0,
                Type = "DirectDamageNoVariance",
                Value = 0,
                Duration = 0,
                Pulse = 0,
                PulsePower = 0,
                Power = 0,
                CastTime = 0,
                Range = WorldMgr.VISIBILITY_DISTANCE
            };
            s = new Spell(dbs, 1);
			sl = new SpellLine("RAs", "RealmAbilitys", "RealmAbilitys", true);
		}
		protected override void CastSpell(GameLiving target)
		{
			if (!target.IsAlive) return;
			if (GameServer.ServerRules.IsAllowedToAttack(m_caster, target, true))
			{
				ISpellHandler damage = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
				damage.StartSpell(target);
			}
		}
	}
}