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
using DOL.Network;
using log4net;
using System.Reflection;

namespace DOL.GS.PacketHandler.Client.v168
{
    [PacketHandlerAttribute(PacketHandlerType.TCP, 0x9F ^ 168, "Handles client crash packets")]
	public class ClientCrashPacketHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			lock (this)
			{
				string dllName = packet.ReadString(16);
				packet.Position = 0x50;
				uint upTime = packet.ReadInt();
				string text = string.Format("Client crash ({0}) dll:{1} clientUptime:{2}sec", client.ToString(), dllName, upTime);
				if (log.IsInfoEnabled)
                {
                    log.Info(text);
                }

                if (log.IsDebugEnabled)
				{
					log.Debug("Last client sent/received packets (from older to newer):");
					
					foreach (IPacket prevPak in client.PacketProcessor.GetLastPackets())
					{
						log.Info(prevPak.ToHumanReadable());
					}
				}
					
				//Eden
				if(client.Player!=null)
				{
					GamePlayer player = client.Player;
					client.Out.SendPlayerQuit(true);
					client.Player.SaveIntoDatabase();
					client.Player.Quit(true);
					client.Disconnect();
				}
			}
		}
	}
}
