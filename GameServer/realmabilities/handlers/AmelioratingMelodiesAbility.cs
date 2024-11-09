using System;
using System.Collections;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.PropertyCalc;
using DOL.GS.Spells;
using System.Collections.Generic;
using DOL.Language;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Ameliorating Melodies realm ability
	/// </summary>
	public class AmelioratingMelodiesAbility : TimedRealmAbility
	{
		/// <summary>
		/// Constructs the Ameliorating Melodies handler
		/// </summary>
		public AmelioratingMelodiesAbility(DBAbility dba, int level) : base(dba, level) { }

        /// <summary>
        /// Action
        /// </summary>
        /// <param name="living"></param>
        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
            GamePlayer player = living as GamePlayer;


            Group playerGroup = player.Group;

            if (playerGroup == null)
            {
                player.Out.SendMessage("You must be in a group to use this ability!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return;
            }


            int heal = 0;

            switch (Level)
            {
                case 1:
                    heal = 100;
                    break;
                case 2:
                    heal = 250;
                    break;
                case 3:
                    heal = 400;
                    break;
            }


            foreach (GamePlayer groupMember in playerGroup.GetPlayersInTheGroup())
            {
                AmelioratingMelodiesEffect ameffect = groupMember.EffectList.GetOfType<AmelioratingMelodiesEffect>();
                if (ameffect != null)
                {
                    ameffect.Cancel(false);
                    //player.Out.SendMessage("You are already protected by a pool of healing", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    return;
                }
                DisableSkill(living);

            

                if (player == groupMember)
                {
                    SendCasterSpellEffectAndCastMessage(player, 3021, true);
                    new AmelioratingMelodiesEffect(heal).Start(groupMember);
                }
                
            }
        }

		/// <summary>
		/// Returns the re-use delay of the ability
		/// </summary>
		/// <param name="level">Level of the ability</param>
		/// <returns>Delay in seconds</returns>
		public override int GetReUseDelay(int level)
		{
            //900
			return 900;
		}

		/// <summary>
		/// Delve information
		/// </summary>
		public override void AddEffectsInfo(IList<string> list, GamePlayer player)
		{
			list.Add("Level 1: Heals 100 / tick");
			list.Add("Level 2: Heals 250 / tick");
			list.Add("Level 3: Heals 400 / tick");
			list.Add("");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Target", "Group except the user"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + " 30 seconds");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.CastingTime", "instant"));
            
        }
	}
}