using System;
using DOL.GS;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
	public class DarkspireHib: GameNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		public override bool AddToWorld()
        {
           // int color = Util.Random(0, 86);
           // GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
            
           // Inventory = template.CloseTemplate();
            Level = 50;
            Name = "Teleporter";
            
            Model = 411;
            Level = 50;
            Size = 70;
            RespawnInterval = 2000;
            MaxSpeedBase = 0;
            Realm = eRealm.Hibernia;
            base.AddToWorld();
            return true;
        }
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player)) return false;
			TurnTo(player.X,player.Y);
            player.Out.SendMessage("Stop " + player.Name + "! Where do you want to travel  ? I can port you to the [Dub]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			return true;
		}
		public override bool WhisperReceive(GameLiving source, string str)
		{
			if(!base.WhisperReceive(source,str)) return false;
		  	if(!(source is GamePlayer)) return false;
			GamePlayer t = (GamePlayer) source;
			TurnTo(t.X,t.Y);
			switch(str)
			{
                case "Dub":
                    Say("I'm now teleporting you to Dub");
                     {
                         if ((t != null) && (t.Group != null))
                             foreach (GamePlayer pl in t.Group.GetPlayersInTheGroup())

                             pl.MoveTo(98, 29838, 38921, 18989, 1024);
                         else
                             t.MoveTo(98, 29838, 38921, 18989, 1024);
                     }  
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