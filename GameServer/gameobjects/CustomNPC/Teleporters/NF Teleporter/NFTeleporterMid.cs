using System;
using DOL.GS;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
	public class NFMid: GameNPC
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
                + "You can choose the [Hibernia]or [Albion] or Homeland near [BledmeerFaste]or [Labyrinth], [Passage]," 
                + " [Svasud Border Keep]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
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
                case "Hibernia":
                        Say("I will port you now to the place of your choice!");
                        t.MoveTo(163, 429863, 517563, 8984, 2269);
                        t.BroadcastUpdate();
                        break;

                case "Albion":
                        Say("I will port you now to the place of your choice!");
                        t.MoveTo(163, 566470, 514811, 8406, 2714);
                        t.BroadcastUpdate();
                        break;

                case "BledmeerFaste":
                        Say("I will port you now to the place of your choice!");
                        t.MoveTo(163, 548978, 398533, 8018, 155);
                        t.BroadcastUpdate();
                        break;

                case "Labyrinth":
                        Say("I will port you now to the place of your choice!");
                        t.MoveTo(245, 66127, 14006, 30255, 464);
                        t.BroadcastUpdate();
                        break;

                case "Passage":
                        Say("I will port you now to the place of your choice!");
                        t.MoveTo(244, 52680, 25106, 16537, 677);
                        t.BroadcastUpdate();
                        break;

                case "Thidranki":
                        Say("I will port you now to the place of your choice!");
                        t.MoveTo(238, 570012, 540325, 5404, 4090);
                        t.BroadcastUpdate();
                        break;

                case "Svasud Border Keep":
                        Say("I will port you now to the place of your choice!");
                        t.MoveTo(163, 651460, 313758, 9432, 1004);
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