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
using log4net;
using System.Reflection;

namespace DOL.GS.PacketHandler.Client.v168
{
    [PacketHandler(PacketHandlerType.TCP, 0x11, "Handles player market search")]
    public class PlayerMarketSearchRequestHandler : IPacketHandler
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void HandlePacket(GameClient client, GSPacketIn packet)
        {
            if (client == null || client.Player == null)
            {
                return;
            }

            if ((client.Player.TargetObject is IGameInventoryObject) == false)
            {
                return;
            }

            MarketSearch.SearchData search = new MarketSearch.SearchData
            {
                name = packet.ReadString(64),
                slot = (int)packet.ReadInt(),
                skill = (int)packet.ReadInt(),
                resist = (int)packet.ReadInt(),
                bonus = (int)packet.ReadInt(),
                hp = (int)packet.ReadInt(),
                power = (int)packet.ReadInt(),
                proc = (int)packet.ReadInt(),
                qtyMin = (int)packet.ReadInt(),
                qtyMax = (int)packet.ReadInt(),
                levelMin = (int)packet.ReadInt(),
                levelMax = (int)packet.ReadInt(),
                priceMin = (int)packet.ReadInt(),
                priceMax = (int)packet.ReadInt(),
                visual = (int)packet.ReadInt(),
                page = (byte)packet.ReadByte()
            };
            byte unk1 = (byte)packet.ReadByte();
            short unk2 = (short)packet.ReadShort();
            byte unk3 = 0;
            byte unk4 = 0;
            byte unk5 = 0;
            byte unk6 = 0;
            byte unk7 = 0;
            byte unk8 = 0;

            if (client.Version >= GameClient.eClientVersion.Version198)
            {
                // Dunnerholl 2009-07-28 Version 1.98 introduced new options to Market search. 12 Bytes were added, but only 7 are in usage so far in my findings.
                // update this, when packets change and keep in mind, that this code reflects only the 1.98 changes
                search.armorType = search.page; // page is now used for the armorType (still has to be logged, i just checked that 2 means leather, 0 = standard
                search.damageType = (byte)packet.ReadByte(); // 1=crush, 2=slash, 3=thrust
                unk3 = (byte)packet.ReadByte();
                unk4 = (byte)packet.ReadByte();
                unk5 = (byte)packet.ReadByte();
                search.playerCrafted = (byte)packet.ReadByte(); // 1 = show only Player crafted, 0 = all
                // 3 bytes unused
                packet.Skip(3);
                search.page = (byte)packet.ReadByte(); // page is now sent here
                unk6 = (byte)packet.ReadByte();
                unk7 = (byte)packet.ReadByte();
                unk8 = (byte)packet.ReadByte();
            }
           

            search.clientVersion = client.Version.ToString();

            (client.Player.TargetObject as IGameInventoryObject).SearchInventory(client.Player, search);
        }
    }
}