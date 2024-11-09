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


namespace DOL.GS.Scripts
{
    public class AEBackHomePorter : GameNPC
    {
        public AEBackHomePorter()
            : base()
        {
            SetOwnBrain(new AEBackHomePorterBrain());
        }
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    }
}

namespace DOL.AI.Brain
{
    public class AEBackHomePorterBrain : StandardMobBrain
    {
        public AEBackHomePorterBrain()
            : base()
        {
            AggroLevel = 0;
            AggroRange = 0;
            ThinkInterval = 1500;
        }
        public override void Think()
        {

            foreach (GamePlayer player in Body.GetPlayersInRadius(600))
            {
                GamePlayer t = (GamePlayer)player;

                if (player == null) continue;
                if (player.InCombat)
                {
                    Body.Say(player.Name + " Sorry, you are recently in combat. The port fails!");
                    continue;
                }
                //if (player.Client.Account.PrivLevel >= (uint)ePrivLevel.GM) continue;
                if ((!Body.IsWithinRadius(player, (short)150))) continue;
                {
                    switch (t.Realm)
                    {
                        case eRealm.Albion:
                            t.MoveTo(166, 38027, 53524, 4154, 2527);
                            t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                            break;

                        case eRealm.Midgard:
                            t.MoveTo(166, 54809, 24284, 4320, 958);
                            t.StartInvulnerabilityTimer(GS.ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                            break;

                        case eRealm.Hibernia:
                            t.MoveTo(166, 18211, 17653, 4320, 3924);
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