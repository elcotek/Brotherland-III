using System;
using DOL.GS;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;
using System.Collections;

namespace DOL.GS.Scripts
{
    public class EventMidTeleporter: GameNPC
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
            Name = "Mendo";
            GuildName = "Teleporter";
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
            if (player.Realm == eRealm.Midgard)
            {
            }
            else
            {
            player.Out.SendMessage("No your Realm Telepoter please use another!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return false;
            }
            player.Out.SendMessage("Will you port to [Mularn],[Jordheim],[Housing]?", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
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
                case "Mularn":
                    Say("Ok now we bring you to Mularn!");
                    t.MoveTo(100, 804182, 725490, 4680, 232);
                    break;
                
                case "Jordheim":
                    Say("I will port you now to the place of your choice!");
                    t.MoveTo(101, 31733, 35617, 8004, 2403);
                    break;
                
                case "Housing":
                    Say("I will port you now to the place of your choice!");
                    t.MoveTo(102, 526503, 561313, 3637, 3032);
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