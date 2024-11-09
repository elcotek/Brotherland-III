using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.Effects;
using DOL.Language;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Blade Barrier
    /// </summary>
    public class BladeBarrierAbility : RR5RealmAbility
    {
        public BladeBarrierAbility(DBAbility dba, int level) : base(dba, level) { }

        /// <summary>
        /// Action
        /// </summary>
        /// <param></param>
        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

            GamePlayer player = living as GamePlayer;
            if (player != null)
            {
                SendCasterSpellEffectAndCastMessage(player, 7055, true);
                BladeBarrierEffect effect = new BladeBarrierEffect();
                effect.Start(player);
            }
            DisableSkill(living);
        }

        public override int GetReUseDelay(int level)
        {
            return 300;
        }

        public override void AddEffectsInfo(IList<string> list, GamePlayer player)
        {
            list.Add("Gives you a 90% 360° Parry buff which is broken if the Effect Owner attacks");
            list.Add("");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Target", "Self"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + " 30 seconds");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.CastingTime", "instant"));
        }
    }
}