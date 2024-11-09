using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.Effects;
using DOL.Language;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Testudo Realm Ability
    /// </summary>
    public class TestudoAbility : RR5RealmAbility
    {
        public TestudoAbility(DBAbility dba, int level) : base(dba, level) { }

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
            if (living.AttackWeapon.Hand == 1)
                return;

            GamePlayer player = living as GamePlayer;
            if (player != null)
            {
                SendCasterSpellEffectAndCastMessage(player, 7068, true);
                TestudoEffect effect = new TestudoEffect();
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
            list.Add("Warrior with shield equipped covers up and takes 90% less damage for all attacks for 45 seconds. Can only move at reduced speed (speed buffs have no effect) and cannot attack. Using a style will break testudo form. This ability is only effective versus realm enemies.");
            list.Add("");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Target", "Self"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + " 45 seconds");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.CastingTime", "instant"));
            
        }
    }
}