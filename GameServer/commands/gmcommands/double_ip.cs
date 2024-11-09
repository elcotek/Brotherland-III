/*Created by FinalFury (Thaney) with an easy description on how to use the command
 
 */
using System;
using DOL.GS;
using DOL.Database;
using DOL.GS.PacketHandler;
using System.Net;
using System.Collections;

namespace DOL.GS.Commands

    ///doubleip displays all duplicate IP's on the server. Thats all ya need to know :)
{
	[CmdAttribute(
		 "&doubleip",
		 ePrivLevel.GM,
		 "Find the double logins",
		 "/doubleip")]
	public class DoubleIPCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			int i = 0;
			Hashtable ip = new Hashtable();
			string accip;
			foreach (GameClient plc in WorldMgr.GetAllPlayingClients())
			{
				accip = ((IPEndPoint)plc.Socket.RemoteEndPoint).Address.ToString();

				if (!ip.Contains(accip))
					ip.Add(accip, plc);
				else
				{
					GameClient cls = (GameClient)ip[accip];

					string name1 = plc.Player.Name;
					string ip1 = ((IPEndPoint)plc.Socket.RemoteEndPoint).Address.ToString();
					string name2 = cls.Player.Name;
					string ip2 = ((IPEndPoint)cls.Socket.RemoteEndPoint).Address.ToString();

					DisplayMessage(client, "IP: {0} AccountName: {1} (Player: {2}) ", ip1, plc.Account.Name, name1);
					DisplayMessage(client, "IP: {0} AccountName: {1} (Player: {2}) ", ip2, cls.Account.Name, name2);
					i++;
				}
			}
			DisplayMessage(client, "{0} double IP found.", i);
			
		}
	}
}
