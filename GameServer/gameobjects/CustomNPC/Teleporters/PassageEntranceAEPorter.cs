
/*
 * Elcotek: based on the work of ML10 Porter.
 * 
 * 
 * 
 */

using DOL.AI.Brain;
using DOL.GS;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
    public class PassageEntranceAEPorter : GameNPC
    {
        public PassageEntranceAEPorter()
            : base()
        {
            SetOwnBrain(new PassageAEPorterBrain());
        }
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    }
}

namespace DOL.AI.Brain
{
    public class PassageAEPorterBrain : StandardMobBrain
    {
        public PassageAEPorterBrain()
            : base()
        {
            AggroLevel = 0;
            AggroRange = 0;
            ThinkInterval = 29800;
        }
        public override void Think()
        {

            foreach (GamePlayer player in Body.GetPlayersInRadius(600))
            {
                GamePlayer t = (GamePlayer)player;

                if (player == null) continue;
                if (player.Client.Account.PrivLevel >= (uint)ePrivLevel.GM) continue;
                if (player.IsAttackable == false)
                {
                    SendReply(t, "You can't wait here, we will port you if you are attackable!");
                    return;
                }
                if (GameRelic.IsPlayerCarryingRelic(player))
                {
                    SendReply(t, "First drop your Relic to the ground!");
                    return;
                }
                if ((!Body.IsWithinRadius(player, (short)500))) continue;


                {
                    switch (t.Realm)
                    {
                        case eRealm.Albion:

                            t.MoveTo(244, 27458, 51886, 18048, 1941);
                            t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                            break;

                        case eRealm.Midgard:

                            t.MoveTo(244, 53096, 25092, 16492, 925);
                            t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                            break;

                        case eRealm.Hibernia:

                            t.MoveTo(244, 47182, 48442, 17345, 1024);
                            t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                            break;

                        default: break;
                    }
                    base.Think();
                }
            }
        }

        private void SendReply(GamePlayer target, string msg)
        {
            target.Client.Out.SendMessage(
                msg,
                eChatType.CT_Say, eChatLoc.CL_PopupWindow);
        }
    }
}


