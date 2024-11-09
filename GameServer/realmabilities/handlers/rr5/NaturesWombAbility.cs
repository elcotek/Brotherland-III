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
	public class NaturesWombAbility : RR5RealmAbility
	{
		public NaturesWombAbility(DBAbility dba, int level) : base(dba, level) { }

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
				SendCasterSpellEffectAndCastMessage(player, 7053, true);
				NaturesWombEffect effect = new NaturesWombEffect();
				effect.Start(player);
			}
			DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			return 600;
		}

		public override void AddEffectsInfo(IList<string> list, GamePlayer player)
		{
			list.Add("Stuns you for 5 seconds. During this time all damage is converted into healing!");
			list.Add("");
			list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Target", "Self"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + " 5 seconds");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.CastingTime", "instant"));
       }

	}
}