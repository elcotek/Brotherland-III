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
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;
using System;

namespace DOL.GS.Commands
{
    [CmdAttribute(
		"&ban",
		ePrivLevel.GM,
		"GMCommands.Ban.Description",
		"GMCommands.Ban.Usage.IP",
		"GMCommands.Ban.Usage.Account",
		"GMCommands.Ban.Usage.Both")]
	public class BanCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 3)
			{
				DisplaySyntax(client);
				return;
			}

			GameClient gc = WorldMgr.GetClientByPlayerName(args[2],false,false);
			if (gc == null)
			{
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Ban.UnableToFindPlayer"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			try
			{
				DBBannedAccount b = new DBBannedAccount();
				string accip = gc.TcpEndpointAddress;
				string accname = GameServer.Database.Escape(gc.Account.Name);
				string reason;

				if (args.Length >= 4)
					reason = String.Join(" ", args, 2, args.Length - 2);
				else reason = "No Reason.";

				switch (args[1].ToLower())
				{
						#region Account
					case "account":
						var acctBans = GameServer.Database.SelectObjects<DBBannedAccount>("(`Type` = @TypeA OR `Type` = @TypeB) AND `Account` = @Account", new[] { new QueryParameter("@TypeA", "A"), new QueryParameter("@TypeB", "B"), new QueryParameter("@Account", accname) });

                        if (acctBans.Count > 0)
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Ban.AAlreadyBanned"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							return;
						}

						b.Type = "A";
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Ban.ABanned", accname), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						break;
						#endregion Account
						#region IP
					case "ip":
						
                        var ipBans = GameServer.Database.SelectObjects<DBBannedAccount>("(`Type` = @TypeI OR `Type` = @TypeB) AND `Ip` = @Ip", new[] { new QueryParameter("@TypeI", "I"), new QueryParameter("@TypeB", "B"), new QueryParameter("@Ip", accip) });
                        if (ipBans.Count > 0)
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Ban.IAlreadyBanned"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							return;
						}

						b.Type = "I";
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Ban.IBanned", accip), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						break;
						#endregion IP
						#region Both
					case "both":
						
                        var acctIpBans = GameServer.Database.SelectObjects<DBBannedAccount>("`Type` = @Type AND `Account` = @Account AND `Ip` = @Ip", new[] { new QueryParameter("@Type", "B"), new QueryParameter("@Account", accname), new QueryParameter("@Ip", accip) });
                        if (acctIpBans.Count > 0)
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Ban.BAlreadyBanned"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							return;
						}

						b.Type = "B";
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Ban.BBanned", accname, accip), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						break;
						#endregion Both
						#region Default
					default:
						{
							DisplaySyntax(client);
							return;
						}
						#endregion Default
				}

				b.Author = client.Player.DBCharacter.Name;
				b.Ip = accip;
				b.Account = accname;
                b.DateBan = DateTime.Now;
				b.Reason = reason;
				GameServer.Database.AddObject(b);
				GameServer.Database.SaveObject(b);

				if (log.IsInfoEnabled)
					log.Info("Ban added [" + args[1].ToLower() + "]: " + accname + "(" + accip + ")");
				return;
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("/ban Exception", e);
			}

			// if not returned here, there is an error
			DisplaySyntax(client);
		}
	}
}