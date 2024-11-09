using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.GS.Spells;
using DOL.Language;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Concentration - refresh quickcast
    /// </summary>
    public class ConcentrationAbility : TimedRealmAbility
    {
        public ConcentrationAbility(DBAbility dba, int level) : base(dba, level) { }


        /// <summary>
        /// The ability disable duration in milliseconds
        /// </summary>
        private const int QickCastDISABLE_DURATION = 30000;


        /// <summary>
        /// Action
        /// </summary>
        /// <param name="living"></param>
        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

            SendCasterSpellEffectAndCastMessage(living, 7006, true);


            int newDisableduraion = QickCastDISABLE_DURATION;


            GamePlayer player = living as GamePlayer;
           
            if (player != null)
            {
                               
                ConcentrationAbility conAbility = player.GetAbility<ConcentrationAbility>();

                if (conAbility != null)
                {
                    switch (conAbility.Level)
                    {
                        case 1:
                        case 2:
                        case 3:
                            {
                                newDisableduraion = 0;// percent * newDisableduraion / 100;
                                player.TempProperties.setProperty(GamePlayer.ConcentrationTimeValue, newDisableduraion);
                                break;
                            }
                    }
                } 

            }
            if (SpellHandler.FindEffectOnTarget(player, "Amnesia") == null)
            {
                DisableSkill(living);
            }
            else
                return;
        }

        public override int GetReUseDelay(int level)
        {
            switch (level)
            {
                    case 1: return 15 * 60;
                    case 2: return 3 * 60;
                    case 3: return 30;
            }
            return 0;
        }

        public override void AddEffectsInfo(IList<string> list, GamePlayer player)
        {
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "ConcentrationAbility.AddEffectsInfo.Info1"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "ConcentrationAbility.AddEffectsInfo.Info2"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "ConcentrationAbility.AddEffectsInfo.Info3"));
            list.Add("");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "ConcentrationAbility.AddEffectsInfo.Info4"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "ConcentrationAbility.AddEffectsInfo.Info5"));

        }
    }
}