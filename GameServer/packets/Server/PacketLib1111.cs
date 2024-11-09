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
#define NOENCRYPTION
using DOL.Database;
using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;


namespace DOL.GS.PacketHandler
{
    [PacketLib(1111, GameClient.eClientVersion.Version1111)]
    public class PacketLib1111 : PacketLib1110
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructs a new PacketLib for Client Version 1.111
        /// </summary>
        /// <param name="client">the gameclient this lib is associated with</param>
        public PacketLib1111(GameClient client)
            : base(client)
        {

        }

        public override void SendLoginGranted(byte color)
        {
            using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.LoginGranted)))
            {
                pak.WritePascalString(m_gameClient.Account.Name);
                pak.WritePascalString(GameServer.Instance.Configuration.ServerNameShort); //server name
                pak.WriteByte(0x0C); //Server ID
                pak.WriteByte(color);
                pak.WriteByte(0x00);
                pak.WriteByte(0x00);
                SendTCP(pak);
            }
        }

        public override void SendLoginGranted(GameClient client)
        {
            //[Freya] Nidel: Can use realm button in character selection screen

            if (ServerProperties.Properties.ALLOW_ALL_REALMS)
            {

                IList<Account> PlayerAccountName = GameServer.Database.SelectObjects<Account>("`Name` = @Name", new QueryParameter("@Name", client.Account.Name));

                foreach (Account db in PlayerAccountName)
                {

                    if (db.LastLoginRealm != 0 && db.PrivLevel == 1 && db != null && db.IsOtherRealmAllowed == false && ServerProperties.Properties.LOGIN_OTHER_RELAM_SWITCH_TIMER_MINUTES > 0)
                    {
                        if (db.LoginWaitTime > DateTime.Now)
                        {

                            SendLoginGranted(GameServer.ServerRules.GetColorHandling(m_gameClient));

                        }
                        else
                        {
                            SendLoginGranted(1);
                        }
                    }
                    if (db.PrivLevel > 1 || db.LastLoginRealm == 0 && db.Realm == 0) //GMs and New Accounts only
                    {

                        SendLoginGranted(1);
                    }
                }
            }
            else
            {
                IList<Account> PlayerAccountName = GameServer.Database.SelectObjects<Account>("`Name` = @Name", new QueryParameter("@Name", client.Account.Name));

                foreach (Account db in PlayerAccountName)
                {

                    if (db.LoginWaitTime > DateTime.Now)
                    {

                        SendLoginGranted(GameServer.ServerRules.GetColorHandling(m_gameClient));

                    }
                    else
                    {
                        SendLoginGranted(1);
                    }
                }

                //SendLoginGranted(GameServer.ServerRules.GetColorHandling(m_gameClient));
            }
        }
    }
}
