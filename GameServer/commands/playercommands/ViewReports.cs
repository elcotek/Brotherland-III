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
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&viewreports",
		ePrivLevel.Player,
		"Allows you to view submitted bug reports.",
		"/viewreports")]
	public class ViewReportsCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (IsSpammingCommand(client.Player, "viewreports"))
				return;

			try
			{
				// We recieved args, and must be admin
				switch (args[1])
				{
					case "close":
						{
							if (client.Account.PrivLevel < 2)
							{
								client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.ViewReport.NoPriv"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return;
							}
							if (args[2] == String.Empty)
							{
								client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.ViewReport.Help.Close"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								break;
							}

							int repor = int.Parse(args[2]);
							BugReport report = GameServer.Database.FindObjectByKey<BugReport>(repor);
							if (report == null)
							{
								client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.ViewReport.InvalidReport"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								break;
							}
							report.ClosedBy = client.Player.Name;
                            report.DateClosed = DateTime.Now;
							GameServer.Database.SaveObject(report);
							break;
						}
					case "delete":
						{
							if (client.Account.PrivLevel < 2)
							{
								client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.ViewReport.NoPriv"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return;
							}
							if (args[2] == String.Empty)
							{
								client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.ViewReport.Help.Delete"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								break;
							}
							int repor = int.Parse(args[2]);
							BugReport report = GameServer.Database.FindObjectByKey<BugReport>(repor);
							if (report == null)
							{
								client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.ViewReport.InvalidReport"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								break;
							}
							// Create a counter to keep track of our BugReport ID
							int count = 1;
							GameServer.Database.DeleteObject(report);
							// Get all Database'd Bug Reports since we have deleted one
							var bugReports = GameServer.Database.SelectAllObjects<BugReport>();
							foreach (BugReport curReport in bugReports)
							{
								// Create new DB for bugreports without the one we deleted
								curReport.ID = count;
								GameServer.Database.SaveObject(curReport);
								count++;
							}
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.ViewReport.ReportDeleted", report.ID), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							break;
						}
					default:
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.ViewReport.UnknownCommand"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							DisplayHelp(client);
						}
						break;
				} //switch
				return;
			}
			catch (Exception e)
			{
				e = new Exception();
				// Display bug reports to player
				string Reports = "---------- BUG REPORTS ------------\n";
				var dbo = GameServer.Database.SelectAllObjects<BugReport>();
				if (dbo.Count < 1)
				{
					Reports += "  - No Reports On File -\n";
					return;
				}

				foreach (BugReport repo in dbo)
				{
					Reports += repo.ID + ")";
					if (client.Account.PrivLevel > 2)
						Reports += repo.Submitter + "\n";
					Reports += "Submitted: " + repo.DateSubmitted + "\n";
					Reports += "Report: " + repo.Message + "\n";
					Reports += "Closed By: " + repo.ClosedBy + "\n";
					Reports += "Date Closed: " + repo.DateClosed + "\n\n";
					client.Out.SendMessage(Reports, eChatType.CT_Important, eChatLoc.CL_PopupWindow);
					Reports = String.Empty;
				}
			}
		}

		public void DisplayHelp(GameClient client)
		{
			client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.ViewReport.Usage"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.ViewReport.Help.Close"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.ViewReport.Help.Delete"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

	}
}
