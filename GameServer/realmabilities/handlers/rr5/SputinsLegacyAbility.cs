using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.Effects;
using DOL.Language;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Sputins Legacy Realm Ability
    /// </summary>
    public class SputinsLegacyAbility : RR5RealmAbility
    {
        public SputinsLegacyAbility(DBAbility dba, int level) : base(dba, level) { }

        /// <summary>
        /// Action
        /// </summary>
        /// <param name="living"></param>
        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;



            GamePlayer player = living as GamePlayer;
            if (player != null)
            {
                SendCasterSpellEffectAndCastMessage(player, 7070, true);
                SputinsLegacyEffect effect = new SputinsLegacyEffect();
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
            list.Add("The Healer won't die for 30sec.");
            list.Add("");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Target", "Self"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + " 30 seconds");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.CastingTime", "instant"));
            
        }
    }
}