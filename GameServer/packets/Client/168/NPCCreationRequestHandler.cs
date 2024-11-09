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

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP,0x16^168,"Handles requests for npcs(0x72) in game")]
	public class NPCCreationRequestHandler : IPacketHandler
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			if (client.Player == null)
				return;
			Region region = client.Player.CurrentRegion;
			if (region == null)
				return;

			ushort id;
			if (client.Version >= GameClient.eClientVersion.Version1126)
				id = packet.ReadShortLowEndian(); // Dre: disassembled game.dll show a write of uint, is it a wip in the game.dll?
			else
				id = packet.ReadShort();
			GameObject anpc = region.GetObject(id) as GameObject;

			if (anpc == null)
				return;
			if (!client.Player.IsWithinRadius(anpc, WorldMgr.VISIBILITY_DISTANCE + 1000))
			//if (WorldMgr.GetClientsOfRegion(anpc.CurrentRegionID) == null) //| lient.Player.IsWithinRadius(anpc, WorldMgr.OBJ_UPDATE_DISTANCE
			{
				client.Out.SendObjectDelete(id);
				return;
			}

			GameNPC npc = anpc as GameNPC;

			if (npc != null)
			{
				Tuple<ushort, ushort> key = new Tuple<ushort, ushort>(npc.CurrentRegionID, (ushort)npc.ObjectID);

				long updatetime;
				if (!client.GameObjectUpdateArray.TryGetValue(key, out updatetime))
				{
					updatetime = 0;
				}

				client.Out.SendNPCCreate(npc);
				// override update from npc create as this is a client request !
				if (updatetime > 0)
					client.GameObjectUpdateArray[key] = updatetime;

				if (npc.Inventory != null)
					client.Out.SendLivingEquipmentUpdate(npc);

				//DO NOT SEND A NPC UPDATE, it is done in Create anyway
				//Sending a Update causes a UDP packet to be sent and
				//the client will get the UDP packet before the TCP Create packet
				//Causing the client to issue another NPC CREATION REQUEST!
				//client.Out.SendNPCUpdate(npc); <-- BIG NO NO
			}
			/*
			public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			ushort id = packet.ReadShort();
//			GameNPC npc = (GameNPC)WorldMgr.GetObjectTypeByIDFromRegion(client.Player.CurrentRegionID, id, typeof(GameNPC));
			if(client.Player==null)
            {
                return;
            }

            Region region = client.Player.CurrentRegion;
			if (region == null)
            {
                return;
            }

            GameNPC npc = region.GetObject(id) as GameNPC;

			if(npc != null)
			{
				client.Out.SendNPCCreate(npc);
				if(npc.Inventory != null)
                {
                    client.Out.SendLivingEquipmentUpdate(npc);
                }

                //DO NOT SEND A NPC UPDATE, it is done in Create anyway
                //Sending a Update causes a UDP packet to be sent and
                //the client will get the UDP packet before the TCP Create packet
                //Causing the client to issue another NPC CREATION REQUEST!
                //client.Out.SendNPCUpdate(npc); <-- BIG NO NO
            }
			*/

		}
	}
}
