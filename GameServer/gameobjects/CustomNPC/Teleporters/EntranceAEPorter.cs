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
    public class EntranceAEPorter : GameNPC
    {
        public EntranceAEPorter()
            : base()
        {
            SetOwnBrain(new EntranceAEPorterBrain());
        }
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Gets/sets the body of this brain
        /// </summary>
       


    }
}

namespace DOL.AI.Brain
{
    public class EntranceAEPorterBrain : StandardMobBrain
    {
        public EntranceAEPorterBrain()
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
                if ((!Body.IsWithinRadius(player, (short)1000))) continue;
                if (GameRelic.IsPlayerCarryingRelic(t))
                {
                    SendReply(t, "Remove your Relic from backpack first!");
                    continue;
                }
                {
                    switch (t.Realm)
                    {
                        case eRealm.Albion:

                            // if ((t != null) && (t.Group != null))
                            //     foreach (GamePlayer pl in t.Group.GetPlayersInTheGroup())
                            //        pl.MoveTo(163, 616149, 679041, 9560, 1611);
                            // else

                            t.MoveTo(91, 31836, 32542, 15588, 2041);
                            break;

                        case eRealm.Midgard:
                            //  Say("I will port you to the place of your choice!");
                            // if ((t != null) && (t.Group != null))
                            //     foreach (GamePlayer pl in t.Group.GetPlayersInTheGroup())

                            //        pl.MoveTo(163, 651460, 313758, 9432, 1100);
                            // else

                            t.MoveTo(91, 31836, 32542, 15588, 2041);
                            break;

                        case eRealm.Hibernia:
                            //   Say("I will port you to the place of your choice!");
                            //   if ((t != null) && (t.Group != null))
                            //      foreach (GamePlayer pl in t.Group.GetPlayersInTheGroup())

                            //         pl.MoveTo(163, 396519, 618017, 9838, 2159);
                            // else


                            t.MoveTo(91, 31836, 32542, 15588, 2041);
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


