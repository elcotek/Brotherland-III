using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.RealmAbilities
{
	public class EpiphanyAbility : RR5RealmAbility
	{
		public EpiphanyAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED )) return;

			bool deactivate = false;

			GamePlayer player = living as GamePlayer;
			if (player != null)
			{
				if (player.Group != null)
				{
					SendCasterSpellEffectAndCastMessage(living, 7066, true);
					foreach (GamePlayer member in player.Group.GetPlayersInTheGroup())
					{
						if (!CheckPreconditions(member, DEAD) && living.IsWithinRadius(member, 2000))
						{
							if (restoreMana(member, player))
								deactivate = true;
						}
					}
				}
				else
				{
					if (!CheckPreconditions(player, DEAD))
					{
						if (restoreMana(player, player))
							deactivate = true;
					}
				}

				if (deactivate)
					DisableSkill(living);
			}
		}
		

		private bool restoreMana(GameLiving target, GamePlayer owner)
		{
			int mana = (int)(target.MaxMana * 0.25);
			int modheal = target.MaxMana - target.Mana;
			if (modheal < 1)
				return false;
			if (modheal > mana)
				modheal = mana;
			if (target is GamePlayer && target != owner)
				((GamePlayer)target).Out.SendMessage(owner.Name + " restores you " + modheal + " points of mana, and 50% of your endurance.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
			if (target != owner)
				owner.Out.SendMessage("You restore" + target.Name + " " + modheal + " points of mana, and 50% of their endurance.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
			if (target == owner)
				owner.Out.SendMessage("You restore yourself " + modheal + " points of mana, and 50% of your endurance.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
			
            target.Mana += modheal;

            //[StephenxPimentel]
            //1.108 - Now Heals 50% Endurance.
            target.Endurance += (target.MaxEndurance / 2);

			return true;

		}

		public override int GetReUseDelay(int level)
		{
			return 600;
		}


		public override void AddEffectsInfo(IList<string> list, GamePlayer player)
		{
			list.Add("25% Group power refresh.");
			list.Add("");
		    list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Target", "Group"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.CastingTime", "instant"));
    
        }

	}
}