using System;
using DOL.GS;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
    public class RunihuraAlb : GameNPC
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
            player.Out.SendMessage("Stop!" + player.Name + "Wish you travel to [Runihura]? ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
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
                case "Runihura":
                        Say("I will port you to the place of your choice!");
                        t.MoveTo(73, 334061, 466177, 10729, 3074);
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