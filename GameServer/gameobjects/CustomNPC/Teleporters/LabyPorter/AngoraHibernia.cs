using System;
using DOL.GS;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
	public class AngoraHib: GameNPC
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
            player.Out.SendMessage("Stop!" + player.Name + "!Where do you want to travel? [Cruachan Gorge], [Angora]!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			return true;
		}
		public override bool WhisperReceive(GameLiving source, string str)
		{
			if(!base.WhisperReceive(source,str)) return false;
		  	if(!(source is GamePlayer)) return false;
			GamePlayer t = (GamePlayer) source;
            int Randomport = Util.Random(1, 4);
            //TurnTo(t.X,t.Y);
			switch(str)
			{
                case "Cruachan Gorge":
                    Say("I will port you now to the place of your choice!");
                     {
                        if (t.Group != null && t != null)

                            foreach (GamePlayer grpMate in t.Group.GetPlayersInTheGroup())
                                if (grpMate.IsWithinRadius(this, 500) && grpMate.IsAlive)

                                    grpMate.MoveTo(163, 396519, 618017, 9838, 2159);
                                else
                                    Say("your group is not in your range!");
                                    
                    }
                        if (t.Group == null && t != null)
                        {
                            t.MoveTo(163, 396519, 618017, 9838, 2159);
                        }
                      break;
               
                case "Angora":
                        Say("I will port you now to the place of your choice!");
                        if (Randomport == 1)
                       {
                        if (t.Group != null && t != null)

                            foreach (GamePlayer grpMate in t.Group.GetPlayersInTheGroup())
                                if (grpMate.IsWithinRadius(this, 500) && grpMate.IsAlive)

                                    grpMate.MoveTo(245, 49789, 29088, 30068, 997);
                                    break;
                                    
                    }
                        if (t.Group == null && t != null)
                        {
                            t.MoveTo(245, 49789, 29088, 30068, 997);
                        }
                        if (Randomport == 2)
                         {
                        if (t.Group != null && t != null)

                            foreach (GamePlayer grpMate in t.Group.GetPlayersInTheGroup())
                                if (grpMate.IsWithinRadius(this, 500) && grpMate.IsAlive)

                                    grpMate.MoveTo(245, 51271, 27467, 30068, 1957);
                                    break;
                                    
                    }
                        if (t.Group == null && t != null)
                        {
                            t.MoveTo(245, 51271, 27467, 30068, 1957);
                        }
                        if (Randomport == 3)
                          {
                        if (t.Group != null && t != null)

                            foreach (GamePlayer grpMate in t.Group.GetPlayersInTheGroup())
                                if (grpMate.IsWithinRadius(this, 500) && grpMate.IsAlive)

                                    grpMate.MoveTo(245, 52985, 29238, 30068, 2996);
                                    break;
                                    
                    }
                        if (t.Group == null && t != null)
                        {
                            t.MoveTo(245, 52985, 29238, 30068, 2996);
                        }
                        
                        if (Randomport == 4)
                        {
                        if (t.Group != null && t != null)

                            foreach (GamePlayer grpMate in t.Group.GetPlayersInTheGroup())
                                if (grpMate.IsWithinRadius(this, 500) && grpMate.IsAlive)

                                    grpMate.MoveTo(245, 51245, 30772, 30068, 3922);
                                    break;
                                    
                    }
                        if (t.Group == null && t != null)
                        {
                            t.MoveTo(245, 51245, 30772, 30068, 3922);
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