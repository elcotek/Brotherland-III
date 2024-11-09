using System;
using DOL.GS;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
	public class NFAlb: GameNPC
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
            player.Out.SendMessage("Stop!" + player.Name + "!Where do you want to travel? (no save Zone, use at your own risk!!) "
            + "  [Midgard Odins Tor], [Hibernia Emain Macha] or Homeland near [Caer Benowyc] or [Labyrinth], [Passage]," 
            + " [Castle Sauvage Border Keep]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
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
                case "Midgard Odins Tor":
                        Say("I will port you to the place of your choice!");
                        t.MoveTo(163, 541215, 416818, 8214, 2192);
                        t.BroadcastUpdate();
                        break;

                case "Hibernia Emain Macha":
                        Say("I will port you to the place of your choice!");
                        t.MoveTo(163, 432000, 500143, 8560, 3622);
                        t.BroadcastUpdate();
                        break;

                case "Caer Benowyc":
                        Say("I will port you to the place of your choice!");
                        t.MoveTo(163, 579938, 512584, 8042, 1248);
                        t.BroadcastUpdate();
                        break;

                case "Labyrinth":
                        Say("I will port you to the place of your choice!");
                        t.MoveTo(245, 65173, 42681, 30257, 1397);
                        t.BroadcastUpdate();
                        break;
               
                case "Passage":
                        Say("I will port you to the place of your choice!");
                        t.MoveTo(244, 27172, 50996, 17882, 1696);
                        t.BroadcastUpdate();
                        break;
              
                case "Thidranki":
                        Say("I will port you to the place of your choice!");
                        t.MoveTo(238, 563505, 574362, 5408, 2051);
                        t.BroadcastUpdate();
                        break;


                case "Castle Sauvage Border Keep":
                        Say("I will port you to the place of your choice!");
                        t.MoveTo(163, 653811, 616998, 9560, 2040);
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