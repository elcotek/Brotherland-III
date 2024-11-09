/*
 * Elcotek: based on the work of ML10 Porter.
 * 
 * 
 * 
 */

using DOL.AI.Brain;
using DOL.GS;
using DOL.GS.PacketHandler;


namespace DOL.GS.Scripts
{
    public class NFAEPorter : GameNPC
    {
        public NFAEPorter()
            : base()
        {
            SetOwnBrain(new NFAEPorterBrain());
        }
    }
}

namespace DOL.AI.Brain
{
    public class NFAEPorterBrain : StandardMobBrain
    {
        public NFAEPorterBrain()
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
                if (player.Client.Account.PrivLevel >= (uint)ePrivLevel.GM) continue;
                if ((!Body.IsWithinRadius(player, (short)350))) continue;
                {
                    switch (t.Realm)
                    {
                        case eRealm.Albion:
                           
                            t.MoveTo(163, 616149, 679041, 9560, 1611);
                            break;

                        case eRealm.Midgard:
                                                        
                            t.MoveTo(163, 651460, 313758, 9432, 1100);
                            break;

                        case eRealm.Hibernia:
                                                     

                            t.MoveTo(163, 396519, 618017, 9838, 2159);
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


