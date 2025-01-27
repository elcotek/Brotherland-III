using System;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using DOL.Database;
using DOL.Language;
namespace DOL.GS.RealmAbilities
{
	public class NegativeMaelstromAbility : TimedRealmAbility
	{
        public NegativeMaelstromAbility(DBAbility dba, int level) : base(dba, level) { }
		private int dmgValue;
		private uint duration;
		private GamePlayer player;
        private const string IS_CASTING = "isCasting";
        private const string NM_CAST_SUCCESS = "NMCasting";

        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
            GamePlayer player = living as GamePlayer;
			if (player.IsMoving)
			{
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "SpellHandler.YouMustBeStandingStillToCast"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return;
			}
            if (player.HasGroundTarget() == false)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "SpellHandler.YouNeedAGroundTarget"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return;
            }

            if (player.IsWithinRadius(player.GroundTarget, 1500) == false)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "SpellHandler.YourGroundTargetIsNotInView"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return;
            }

            if (player.TempProperties.getProperty(IS_CASTING, false))
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.SendCastMessage.YouAlreadyCasting"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return;
            }
            this.player = player;
            if (player.AttackState) 
            {
                player.StopAttack();
            }
            player.StopCurrentSpellcast();
			switch (Level)
			{
				case 1: dmgValue = 120; break;
				case 2: dmgValue = 240; break;
				case 3: dmgValue = 360; break;
				default: return;
			}
			duration = 30;
			foreach (GamePlayer i_player in player.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
			{
				if (i_player == player) i_player.Out.SendMessage("You cast " + this.Name + "!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				else i_player.Out.SendMessage(player.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

				i_player.Out.SendSpellCastAnimation(player, 7027, 20);
			}
            player.TempProperties.setProperty(IS_CASTING, true);
            player.TempProperties.setProperty(NM_CAST_SUCCESS, true);
            GameEventMgr.AddHandler(player, GamePlayerEvent.Moving, new DOLEventHandler(CastInterrupted));
            GameEventMgr.AddHandler(player, GamePlayerEvent.AttackFinished, new DOLEventHandler(CastInterrupted));
            GameEventMgr.AddHandler(player, GamePlayerEvent.Dying, new DOLEventHandler(CastInterrupted));
            if (player != null)
            {
                new RegionTimer(player, new RegionTimerCallback(EndCast), 2000);
            }
		}
		protected virtual int EndCast(RegionTimer timer)
		{
            bool castWasSuccess = player.TempProperties.getProperty(NM_CAST_SUCCESS, false);
            player.TempProperties.removeProperty(IS_CASTING);
            GameEventMgr.RemoveHandler(player, GamePlayerEvent.Moving, new DOLEventHandler(CastInterrupted));
            GameEventMgr.RemoveHandler(player, GamePlayerEvent.AttackFinished, new DOLEventHandler(CastInterrupted));
            GameEventMgr.RemoveHandler(player, GamePlayerEvent.Dying, new DOLEventHandler(CastInterrupted));
            if (player.IsMezzed || player.IsStunned || player.IsSitting)
                return 0;
            if (!castWasSuccess)
                return 0;
			Statics.NegativeMaelstromBase nm = new Statics.NegativeMaelstromBase(dmgValue);
            nm.CreateStatic(player, player.GroundTarget, duration, 5, 350, 1);
            DisableSkill(player); 
			timer.Stop();
			timer = null;
			return 0;
		}
        private void CastInterrupted(DOLEvent e, object sender, EventArgs arguments) 
        {
            AttackFinishedEventArgs attackFinished = arguments as AttackFinishedEventArgs;
            if (attackFinished != null && attackFinished.AttackData.Attacker != sender)
                return;
            player.TempProperties.setProperty(NM_CAST_SUCCESS, false);
            foreach (GamePlayer i_player in player.GetPlayersInRadius(WorldMgr.INFO_DISTANCE)) {
                i_player.Out.SendInterruptAnimation(player);
            }
        }
        public override int GetReUseDelay(int level)
        {
            return 900;
        }
	}
}
