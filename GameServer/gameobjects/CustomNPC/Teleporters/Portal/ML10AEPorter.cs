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
    public class ML10Portal : GameNPC
    {
        public ML10Portal()
            : base()
        {
            SetOwnBrain(new ML10PortalBrain());
        }
   }
}

namespace DOL.AI.Brain
{
    public class ML10PortalBrain : StandardMobBrain
    {
        public ML10PortalBrain()
            : base()
        {
            AggroLevel = 0;
            AggroRange = 0;
            ThinkInterval = 1500;
        }
        public override void Think()
        {

            foreach (GamePlayer player in Body.GetPlayersInRadius(1000))
            {
                GamePlayer t = (GamePlayer)player;

                if (player == null) continue;
                if (player.Client.Account.PrivLevel >= (uint)ePrivLevel.GM) continue;
                //if ((!Body.IsWithinRadius(player, (short)800)) || (t.MLLevel < 9) || (t.Level < 50)) continue;
                {
                    switch (t.Realm)
                    {
                        case eRealm.Albion:
                            
                            
                            t.MoveTo(91, 31893, 34801, 15763, 1976);
                            break;

                        case eRealm.Midgard:
                                                       
                            t.MoveTo(91, 31893, 34801, 15763, 1976);
                            break;

                        case eRealm.Hibernia:
                            
                            t.MoveTo(91, 31893, 34801, 15763, 1976);
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


