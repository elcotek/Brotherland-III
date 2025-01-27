using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.Effects;
using DOL.Language;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Mastery of Concentration RA
    /// </summary>
    public class ShieldTripAbility : RR5RealmAbility
    {
        public ShieldTripAbility(DBAbility dba, int level) : base(dba, level) { }

        /// <summary>
        /// Action
        /// </summary>
        /// <param name="living"></param>
        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
            InventoryItem shield = living.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
            if (shield == null)
                return;
            if (shield.Object_Type != (int)eObjectType.Shield)
                return;
            if (living.TargetObject == null)
                return;
            if (living.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Distance)
                return;
            if (living.AttackWeapon == null)
                return;
            if (living.AttackWeapon.Hand == 1)
                return;
            GameLiving target = (GameLiving)living.TargetObject;
            if (target == null) return;
            if (!GameServer.ServerRules.IsAllowedToAttack(living, target, false))
                return;
            if (!living.IsWithinRadius(target, 1000))
                return;
            new ShieldTripRootEffect().Start(target);

            GamePlayer player = living as GamePlayer;
            if (player != null)
            {
                SendCasterSpellEffectAndCastMessage(player, 7046, true);
                ShieldTripDisarmEffect effect = new ShieldTripDisarmEffect();
                effect.Start(player);
            }
            DisableSkill(living);
        }

        public override int GetReUseDelay(int level)
        {
            return 900;
        }

        public override void AddEffectsInfo(IList<string> list, GamePlayer player)
        {
            list.Add("Roots your target for 10 seconds but disarms you for 15 seconds!");
            list.Add("");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Range", " 1000"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Target", "Enemy"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + " 10 seconds");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.CastingTime", "instant"));
                     
        }
    }
}