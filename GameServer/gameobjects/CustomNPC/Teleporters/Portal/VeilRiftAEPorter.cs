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
    public class VeilRiftEntranceAEPorter : GameNPC
    {
        public VeilRiftEntranceAEPorter()
            : base()
        {
            SetOwnBrain(new VeilRiftEntranceAEPorterBrain());
        }
    }
}

namespace DOL.AI.Brain
{
    public class VeilRiftEntranceAEPorterBrain : StandardMobBrain
    {
        public VeilRiftEntranceAEPorterBrain()
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
                {
                    switch (t.Realm)
                    {
                        case eRealm.Albion:
                           
                            // if ((t != null) && (t.Group != null))
                            //     foreach (GamePlayer pl in t.Group.GetPlayersInTheGroup())
                            //        pl.MoveTo(163, 616149, 679041, 9560, 1611);
                            // else
                            
                            t.MoveTo(93, 24283, 27740, 17549, 3855);
                            break;

                        case eRealm.Midgard:
                            //  Say("I will port you to the place of your choice!");
                            // if ((t != null) && (t.Group != null))
                            //     foreach (GamePlayer pl in t.Group.GetPlayersInTheGroup())

                            //        pl.MoveTo(163, 651460, 313758, 9432, 1100);
                            // else

                            t.MoveTo(93, 24283, 27740, 17549, 3855);
                            break;

                        case eRealm.Hibernia:
                            //   Say("I will port you to the place of your choice!");
                            //   if ((t != null) && (t.Group != null))
                            //      foreach (GamePlayer pl in t.Group.GetPlayersInTheGroup())

                            //         pl.MoveTo(163, 396519, 618017, 9838, 2159);
                            // else


                            t.MoveTo(93, 24283, 27740, 17549, 3855);
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
        // [ScriptLoadedEvent]
        //public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
        //  {
        //    log.Info("\tTeleporter initialized: true");


    }
}


