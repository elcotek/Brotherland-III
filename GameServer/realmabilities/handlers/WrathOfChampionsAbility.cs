using System;
using System.Collections;
using System.Reflection;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using DOL.Database;
using DOL.GS.Spells;
using DOL.Language;


namespace DOL.GS.RealmAbilities
{
	public class WrathofChampionsAbility : TimedRealmAbility
	{
        public WrathofChampionsAbility(DBAbility dba, int level) : base(dba, level) { }

		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			GamePlayer player = living as GamePlayer;
			if (player == null)
				return;

			Int32 dmgValue = 0;
			switch (Level)
			{
				case 1: dmgValue = 200; break;
				case 2: dmgValue = 500; break;
				case 3: dmgValue = 750; break;
			}

			//send cast messages
			foreach (GamePlayer i_player in player.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
			{
				if (i_player == player)
                    //i_player.Out.SendMessage("You cast a Wrath of Champions Spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                (i_player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((i_player as GamePlayer).Client.Account.Language, "ArrowSpellHandler.YouCastAWrathOfChampionsSpell"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				else
                    (i_player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((i_player as GamePlayer).Client.Account.Language, "ArrowSpellHandler.CastAWrathOfChampionsSpell", player.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                //i_player.Out.SendMessage(player.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
			}

			//deal damage to npcs
			foreach (GameNPC mob in player.GetNPCsInRadius(200))
			{
				if (GameServer.ServerRules.IsAllowedToAttack(player, mob, true) == false) continue;

				mob.TakeDamage(player, eDamageType.Spirit, dmgValue, 0);
				//player.Out.SendMessage("You hit the " + mob.Name + " for " + dmgValue + " damage.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client.Account.Language, "ArrowSpellHandler.YouHitTheForDamage", mob.Name, dmgValue), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                foreach (GamePlayer player2 in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					player2.Out.SendSpellCastAnimation(player, 4468, 0);
					player2.Out.SendSpellEffectAnimation(player, mob, 4468, 0, false, 1);
				}
			}

			//deal damage to players
			foreach (GamePlayer t_player in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				if (GameServer.ServerRules.IsAllowedToAttack(player, t_player, true) == false)
					continue;
					
                //Check to see if the player is phaseshifted
                GameSpellEffect phaseshift;
                phaseshift = SpellHandler.FindEffectOnTarget(t_player, "Phaseshift");
                if (phaseshift != null)
                {
                    ///player.Out.SendMessage(t_player.Name + " is Phaseshifted and can't be effected by this Spell!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client.Account.Language, "ArrowSpellHandler.YouArePhaseshiftedCantBeAffected", t_player.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    continue;
                }

				if (!player.IsWithinRadius( t_player, 200 ))
					continue;
				t_player.TakeDamage(player, eDamageType.Spirit, dmgValue, 0);

				// send a message
				//player.Out.SendMessage("You hit " + t_player.Name + " for " + dmgValue + " damage.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				//t_player.Out.SendMessage(player.Name + " hits you for " + dmgValue + " damage.", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
                (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client.Account.Language, "ArrowSpellHandler.YouHitTheForDamage", t_player.Name, dmgValue), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

                (t_player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((t_player as GamePlayer).Client.Account.Language, "ArrowSpellHandler.HitYouForDamage", player.Name, dmgValue), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                foreach (GamePlayer n_player in t_player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					n_player.Out.SendSpellCastAnimation(player, 4468, 0);
					n_player.Out.SendSpellEffectAnimation(player, t_player, 4468, 0, false, 1);
				}
			}
			DisableSkill(living);
			player.LastAttackTickPvP = player.CurrentRegion.Time;
		}

        public override int GetReUseDelay(int level)
        {
            return 600;
        }
	}
}
