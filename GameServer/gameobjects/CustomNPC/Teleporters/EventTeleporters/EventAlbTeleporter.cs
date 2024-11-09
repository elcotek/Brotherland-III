using System;
using DOL.GS;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;
using System.Collections;

namespace DOL.GS.Scripts
{
    public class EventAlbTeleporter: GameNPC
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		public override bool AddToWorld()
        {
			int color = Util.Random(0, 86);
            GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
            template.AddNPCEquipment(eInventorySlot.Cloak, 3801);
            template.AddNPCEquipment(eInventorySlot.TorsoArmor, 2928);
            template.AddNPCEquipment(eInventorySlot.LegsArmor, 2929);
            template.AddNPCEquipment(eInventorySlot.ArmsArmor, 2930);
            template.AddNPCEquipment(eInventorySlot.HandsArmor, 2931);
            template.AddNPCEquipment(eInventorySlot.FeetArmor, 2932);

            Inventory = template.CloseTemplate();
            Level = 50;
            Name = "Ludolf";
            GuildName = "Albion Teleporter";
            Model = 33;
            Size = 54;
            RespawnInterval = 2000;
            MaxSpeedBase = 0;
            Realm = 0;
            base.AddToWorld();
            return true;
        }
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player)) return false;
            TurnTo(player, 5000);
            if (player.Realm == eRealm.Albion)
            {
            }
            else
            {
            player.Out.SendMessage("No your Realm Telepoter please use another!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return false;
            }
            player.Out.SendMessage("Will you port to[Cotswold],[Camelot],[Housing]?", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
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
                case "Cotswold":
                    Say("I will port you to the place of your choice!");
                    t.MoveTo(1, 560138, 512037, 2282, 2629);
                    break;
                
                case "Camelot":
                    Say("I will port you to the place of your choice!");
                    t.MoveTo(10, 32189, 31372, 8000, 2450);
                    break;
               
                case "Housing":
                    Say("I will port you to the place of your choice!");
                    t.MoveTo(2, 585005, 561905, 3576, 920);
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