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

using System.Collections;
using System.Reflection;
using DOL.Language;
using DOL.GS;
using DOL.GS.ServerProperties;
using DOL.GS.PacketHandler;


namespace DOL.GS.Commands
{
	[CmdAttribute(
		 "&lfg",
		 ePrivLevel.Player,
		 "Broadcast a LFG message to other players in the same region",
		 "/lfg <message>")]
	public class LFGCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			const string BROAD_TICK = "Broad_Tick";

			if (args.Length < 2)
			{
				DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Broadcast.NoText"));
				return;
			}
			if (client.Player.IsMuted)
			{
				client.Player.Out.SendMessage("You have been muted. You cannot broadcast.", eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
				return;
			}
			string message = string.Join(" ", args, 1, args.Length - 1);

			long BroadTick = client.Player.TempProperties.getProperty<long>(BROAD_TICK);
			if (BroadTick > 0 && BroadTick - client.Player.CurrentRegion.Time <= 0)
			{
				client.Player.TempProperties.removeProperty(BROAD_TICK);
			}
			long changeTime = client.Player.CurrentRegion.Time - BroadTick;
			if (changeTime < 800 && BroadTick > 0)
			{
				client.Player.Out.SendMessage("Slow down! Think before you say each word!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Player.TempProperties.setProperty(BROAD_TICK, client.Player.CurrentRegion.Time);
				return;
			}
			Broadcast(client.Player, message);

			client.Player.TempProperties.setProperty(BROAD_TICK, client.Player.CurrentRegion.Time);
		}

		private void Broadcast(GamePlayer player, string message)
		{
			string RealmName = "";
			string playerMessage = message;

			foreach (GameClient c1 in WorldMgr.GetAllPlayingClients())
			{

				{
					switch (player.Realm)
					{
						case eRealm.Albion:
							{

								RealmName = "Albion";
							}
							break;
						case eRealm.Midgard:
							{

								RealmName = "midgard";
							}
							break;
						case eRealm.Hibernia:
							{

								RealmName = "Hibernia";
							}
							break;
						case eRealm.None:
							{

								RealmName = "None";
							}
							break;

					}
					if (c1.Player.Client.Account.PrivLevel >= (uint)ePrivLevel.Helper)
					{
						c1.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Players.LFG.MessageGM", player.Name, playerMessage, RealmName), eChatType.CT_LFG, eChatLoc.CL_ChatWindow);
						return;
					}

					else
						foreach (GameClient c in WorldMgr.GetClientsOfRegion(c1.Player.CurrentRegionID))
						{
							if (GameServer.ServerRules.IsAllowedToUnderstand(c.Player, player))
							{
								c.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Players.LFG.Message", player.Name, message), eChatType.CT_LFG, eChatLoc.CL_ChatWindow);
								return;
							}
						}
				}
			}
		}
	}
}

