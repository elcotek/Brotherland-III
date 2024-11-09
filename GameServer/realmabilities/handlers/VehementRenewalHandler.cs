using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;


namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Vehement Renewal, healing all but caster in 2000 range
	/// </summary>
	public class VehementRenewalAbility : TimedRealmAbility
	{
		public VehementRenewalAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED | NOTINGROUP)) return;

			int heal = 0;
			switch (Level)
			{
				case 1: heal = 375; break;
				case 2: heal = 750; break;
				case 3: heal = 1500; break;
			}

			bool used = false;

			GamePlayer player = living as GamePlayer;
			if (player != null)
			{
				if (player.Group == null)
				{
					player.Out.SendMessage("You are not in a group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}

				SendCasterSpellEffectAndCastMessage(living, 7017, true);

				foreach (GamePlayer member in player.Group.GetPlayersInTheGroup())
				{
					if (member == player) continue;
					if (!player.IsWithinRadius(member, 2000 )) continue;
					if (!member.IsAlive) continue;
					int healed = member.ChangeHealth(living, GameLiving.eHealthChangeType.Spell, heal);
					if (healed > 0)
						used = true;

					if (healed > 0) member.Out.SendMessage(player.Name + " heals your for " + healed + " hit points.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					if (heal > healed)
					{
						//p.Out.SendMessage("You are fully healed.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                        ((GamePlayer)member).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)member).Client.Account.Language, "SpellHandler.SendCastMessage.FullyHealed"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

                    }
                }
			}
			if (used) DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			return 600;
		}

		public override void AddEffectsInfo(IList<string> list, GamePlayer player)
		{
			list.Add("Level 1: Value: 375");
			list.Add("Level 2: Value: 750");
			list.Add("Level 3: Value: 1500");
			list.Add("");
		    list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Target", "Group"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.CastingTime", "instant"));
        }
	}
}