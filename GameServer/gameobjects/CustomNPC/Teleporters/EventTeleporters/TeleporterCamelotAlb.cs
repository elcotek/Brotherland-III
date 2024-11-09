using System;
using DOL.GS;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
	public class MolvikAlb: GameNPC
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
            player.Out.SendMessage("Stop!" + player.Name + "!Where do you want to travel  ? [Camelot], [Cotswold] [Housing] !", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
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
                case "Camelot":
                        Say("I will port you to the place of your choice!");
                        t.MoveTo(10, 32475, 31710, 8000, 1855);
                        break;
                             
               case "Cotswold":
                        Say("I will port you to the place of your choice!");
                        t.MoveTo(1, 558762, 512185, 2263, 2992);
                        break;

               case "Housing":
                        Say("I will port you to the place of your choice!");
                        t.MoveTo(2, 584521, 561478, 3576, 1039);
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