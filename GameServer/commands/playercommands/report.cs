/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */

using System;

using DOL.GS.PacketHandler;
using DOL.GS.GameEvents;
using DOL.Database;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&report",
		ePrivLevel.Player,
		"Reports a bug",
		"Usage: /report <message>")]
	public class ReportCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		private const ushort MAX_REPORTS = 100;
		
		public void OnCommand(GameClient client, string[] args)
		{
			if (ServerProperties.Properties.DISABLE_BUG_REPORTS)
			{
				DisplayMessage(client, "Bug reporting has been disabled for this server!");
				return;
			}

			if (IsSpammingCommand(client.Player, "report"))
				return;

			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return;
			}

			if (client.Player.IsMuted)
			{
				client.Player.Out.SendMessage("You have been muted and are not allowed to submit bug reports.", eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
				return;
			}

			string message = string.Join(" ", args, 1, args.Length - 1);
			BugReport report = new BugReport();
			
			//Andraste
			var reports = GameServer.Database.SelectAllObjects<BugReport>();
            bool found = false; int i = 0;
            for(i=0;i<MAX_REPORTS;i++)
			{
				found=false;
				foreach(BugReport rep in reports) if(rep.ID==i) found=true;
				if(!found) break;
			}
			if(found)
			{
				client.Player.Out.SendMessage("There are too many reports, please contact a GM or wait until they are cleaned.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			
			//report.ID = GameServer.Database.GetObjectCount<BugReport>() + 1;
			report.ID = i;
			report.Message = message;
			report.Submitter = client.Player.Name + " [" + client.Account.Name + "]";
			GameServer.Database.AddObject(report);
			client.Player.Out.SendMessage("Report submitted, if this is not a bug report it will be ignored!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

			if (String.IsNullOrEmpty(ServerProperties.Properties.BUG_REPORT_EMAIL_ADDRESSES) == false)
			{
				if (client.Account.Mail == String.Empty)
					client.Player.Out.SendMessage("If you enter your email address for your account with /email command, your bug reports will send an email to the staff!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				else
				{
					Mail.MailMgr.SendMail(ServerProperties.Properties.BUG_REPORT_EMAIL_ADDRESSES, GameServer.Instance.Configuration.ServerName + " bug report " + report.ID, report.Message, report.Submitter, client.Account.Mail);
				}
			}
		}
	}
}