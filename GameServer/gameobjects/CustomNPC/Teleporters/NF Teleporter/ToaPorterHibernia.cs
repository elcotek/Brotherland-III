using System;
using DOL.GS;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
	public class NFHib: GameNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		public override bool AddToWorld()
        {
			Level=50;
            base.AddToWorld();
            return true;
        }
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player)) return false;
			//TurnTo(player.X,player.Y);
            player.Out.SendMessage("Stop!" + player.Name + "!Where do you want to travel? (no save Zone, use at your own risk!!)"
                + "[Midgard]or [Albion] or Homeland [Emain Macha] or [Labyrinth], [Passage]," 
                + "[Druim Cain Border Keep]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			return true;
		}
		public override bool WhisperReceive(GameLiving source, string str)
		{
			if(!base.WhisperReceive(source,str)) return false;
		  	if(!(source is GamePlayer)) return false;
			GamePlayer t = (GamePlayer) source;
			//TurnTo(t.X,t.Y);
			switch(str)
			{
                case "Midgard":
                        Say("I will port you to the place of your choice!");
                        t.MoveTo(163, 568693, 403146, 9698, 2368);
                        t.BroadcastUpdate();
                        break;

                case "Albion":
                        Say("I will port you to the place of your choice!");
                        t.MoveTo(163, 593846, 513038, 8533, 1434);
                        t.BroadcastUpdate();
                        break;

                case "Emain Macha":
                        Say("I will port you to the place of your choice!");
                        t.MoveTo(163, 458759, 516265, 7958, 2531);
                        t.BroadcastUpdate();
                        break;
                
                case "Labyrinth":
                        Say("I will port you to the place of your choice!");
                        t.MoveTo(245, 30129, 20054, 30273, 3439);
                        t.BroadcastUpdate();
                        break;
               
                case "Passage":
                        Say("I will port you to the place of your choice!");
                        t.MoveTo(244, 47257, 48400, 17402, 959);
                        t.BroadcastUpdate();
                        break;
              
                case "Thidranki":
                        Say("I will port you to the place of your choice!");
                        t.MoveTo(238, 533690, 533927, 5408, 3531);
                        t.BroadcastUpdate();
                        break;

                case "Druim Cain Border Keep":
                        Say("I will port you to the place of your choice!");
                        t.MoveTo(163, 432841, 680032, 9747, 2585);
                        t.BroadcastUpdate();
                        break;
                
                
                
                default: break;
			}
			return true;
		}
		private void SendReply(GamePlayer target, string msg)
			{
				target.Client.Out.SendMessage(
					msg,
					eChatType.CT_Say,eChatLoc.CL_PopupWindow);
			}
		[ScriptLoadedEvent]
        public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
        {
            log.Info("\tTeleporter initialized: true");
        }	
    }
	
}