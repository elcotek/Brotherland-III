/*
 * Elcotek: based on the work of my ML10 Porter.
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
    public class TreibtCalitePorter : GameNPC
    {
        public TreibtCalitePorter()
            : base()
        {
            SetOwnBrain(new TreibtCalitePorterBrain());
        }
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
       
        public override bool AddToWorld()
        {
			Level = 34;
            Size = 85;
            Model = 114;
            Name = "rocky golem";
                        
            base.AddToWorld();
            return true;
        }
               
    }
}

namespace DOL.AI.Brain
{
    public class TreibtCalitePorterBrain : StandardMobBrain
    {
        public TreibtCalitePorterBrain()
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
                // if (player.Client.Account.PrivLevel >= (uint)ePrivLevel.GM) continue;
                if ((!Body.IsWithinRadius(player, (short)350))) continue;
                {

                    switch (Util.Random(1, 3))
                    {

                        case 1:


                            if (Body != null && Body.IsAlive && t.IsAlive && t.Z != 14748 && Body.HealthPercent < 90)
                                t.MoveTo(224, 34173, 30356, 14748, 4);
                            break;

                        case 2:


                            if (Body != null && Body.IsAlive && t.IsAlive && t.Z != 14492 && Body.HealthPercent < 90)
                                t.MoveTo(224, 30894, 32132, 14492, 3044);
                            break;

                        case 3:


                            if (Body != null && Body.IsAlive && t.IsAlive && t.Z != 13980 && Body.HealthPercent < 90)
                                t.MoveTo(224, 30122, 32645, 13980, 2976);
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


