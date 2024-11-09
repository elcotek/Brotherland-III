using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Mastery of Concentration RA
	/// </summary>
	public class BlindingDustAbility : RR5RealmAbility
	{
		public BlindingDustAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			SendCasterSpellEffectAndCastMessage(living, 7040, true);

			bool deactivate = false;
			foreach (GamePlayer player in living.GetPlayersInRadius(false, 350))
			{
				if (GameServer.ServerRules.IsAllowedToAttack(living, player, true))
				{
					DamageTarget(player, living);
					deactivate = true;
				}
			}

			foreach (GameNPC npc in living.GetNPCsInRadius(false, 350))
			{
				if (GameServer.ServerRules.IsAllowedToAttack(living, npc, true))
				{
					DamageTarget(npc, living);
					deactivate = true;
				}
			}
			if (deactivate)
				DisableSkill(living);
		}

		private void DamageTarget(GameLiving target, GameLiving caster)
		{
			if (!target.IsAlive)
				return;
			if (target.EffectList.GetOfType<BlindingDustEffect>() == null)
			{
				BlindingDustEffect effect = new BlindingDustEffect();
				effect.Start(target);
			}

		}

		public override int GetReUseDelay(int level)
		{
			return 300;
		}

		public override void AddEffectsInfo(IList<string> list, GamePlayer player)
		{
			list.Add("Insta-cast PBAE Attack that causes the enemy to have a 25% chance to fumble melee/bow attacks for the next 15 seconds.");
			list.Add("");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Radius", "350"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Target", "Enemy"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + " 15 seconds");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.CastingTime", "instant"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.RecastTime") + " 5 minutes");
            
        }
	}
}