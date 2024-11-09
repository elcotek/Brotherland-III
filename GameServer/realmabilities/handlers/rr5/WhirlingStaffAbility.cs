using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Whirling Staff Ability
    /// </summary>
    public class WhirlingStaffAbility : RR5RealmAbility
    {
        public WhirlingStaffAbility(DBAbility dba, int level) : base(dba, level) { }

        /// <summary>
        /// Action
        /// </summary>
        /// <param name="living"></param>
        public override void Execute(GameLiving living)
        {
            if (!living.IsAlive || living.IsSitting || living.IsMezzed || living.IsStunned)
                return;


            bool deactivate = false;
            foreach (GamePlayer player in living.GetPlayersInRadius(false, 350))
            {
                if (GameServer.ServerRules.IsAllowedToAttack(living, player, true))
                {

                    DamageTarget(player, living);

                    deactivate = true;
                }
                if (deactivate)
                    DisableSkill(living);
            }
        }

        private void DamageTarget(GameLiving target, GameLiving caster)
        {
            int resist = 251 * target.GetResist(eDamageType.Crush) / -100;
            int damage = 251 + resist;
            GameNPC npc = target as GameNPC;
            GamePlayer player = caster as GamePlayer;

            if (player != null)
                player.Out.SendMessage("You hit " + target.Name + " for " + damage + "(" + resist + ") points of damage!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

            GamePlayer targetPlayer = target as GamePlayer;
            if (npc != null)
                return;
            {
                if (targetPlayer != null)
                    SendCasterSpellEffectAndCastMessage(player, 7043, true);
                {
                    if (targetPlayer.IsStealthed)
                        targetPlayer.Stealth(false);
                }

                foreach (GamePlayer p in target.GetPlayersInRadius(false, WorldMgr.VISIBILITY_DISTANCE))
                {
                    p.Out.SendSpellEffectAnimation(caster, target, 7043, 0, false, 1);
                    p.Out.SendCombatAnimation(caster, target, 0, 0, 0, 0, 0x14, target.HealthPercent);
                }

                //target.TakeDamage(caster, eDamageType.Spirit, damage, 0);
                AttackData ad = new AttackData();
                ad.AttackResult = GameLiving.eAttackResult.HitUnstyled;
                ad.Attacker = caster;
                ad.Target = target;
                ad.DamageType = eDamageType.Crush;
                ad.Damage = damage;
                target.OnAttackedByEnemy(ad);
                caster.DealDamage(ad);

                if ((WhirlingStaffEffect)target.EffectList.GetOfType(typeof(WhirlingStaffEffect)) == null)
                {
                    WhirlingStaffEffect effect = new WhirlingStaffEffect();
                    effect.Start((GameLiving)(target));
                }
            }
        }


        public override int GetReUseDelay(int level)
        {
            return 600;
        }

        public override void AddEffectsInfo(IList<string> list, GamePlayer player)
        {
            list.Add("A 350 radius PBAE attack that deals medium crushing damage and disarms your opponents for 6 seconds");
            list.Add("");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Damage", " 251"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Radius", " 350"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Target", "Enemy"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + " 6 seconds");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.CastingTime", "instant"));
            
        }
    }
}