using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.Effects;
using DOL.Language;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Arms Length Realm Ability
	/// </summary>
	public class DreamweaverAbility : RR5RealmAbility
	{
		public DreamweaverAbility(DBAbility dba, int level) : base(dba, level) { }

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
				SendCasterSpellEffectAndCastMessage(player, 7052, true);
				DreamweaverEffect effect = new DreamweaverEffect();
				effect.Start(player);
			}
			DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			return 420;
		}

		public override void AddEffectsInfo(IList<string> list, GamePlayer player)
		{
			list.Add("Dreamweaver.");
			list.Add("");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Target", "Self"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + " 5 min");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.CastingTime", "instant"));
           
        }

	}
}