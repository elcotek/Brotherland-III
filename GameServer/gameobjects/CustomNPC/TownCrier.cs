﻿/*
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
using DOL.Database.Attributes;
using DOL.GS.PacketHandler;
//This script demonstrates how to create a data aware custom NPC

namespace DOL.GS.Scripts
{
    public class TownCrier : GameNPC
    {
        /// <summary>
        /// The data object that represents a town crier message
        /// </summary>
        [DataTable(TableName = "TownCrierMessages")]
        public class TownCrierMessage : DataObject
        {
            private string m_msg;
            private string m_id;

            public TownCrierMessage()
            {
                m_msg = "Dawn of Light r0x my world!";
            }

            [PrimaryKey]
            public string TownCrierID
            {
                get { return m_id; }
                set
                {
                    Dirty = true;
                    m_id = value;
                }
            }

            [DataElement(AllowDbNull = true)]
            public string Message
            {
                get { return m_msg; }
                set
                {
                    Dirty = true;
                    m_msg = value;
                }
            }
        } //end db relation

        private string m_msg;
        private static bool m_init = false;

        public TownCrier()
        {
            m_msg = "Dawn of Light r0x my world!";
            InitTC();
        }

        public TownCrier(string msg)
        {
            InitTC();
            m_msg = msg;
        }

        private static void InitTC()
        {
            if (!m_init && GameServer.Database != null)
            {
                //Obsolete with GSS Table Registering in SVN : 3337
                //GameServer.Database.RegisterDataObject(typeof(TownCrierMessage));
                // 2008-01-29 Kakuri - Obsolete
                //GameServer.Database.LoadDatabaseTable( typeof( TownCrierMessage ) );
                m_init = true;
            }
        }

        public override bool Interact(GamePlayer player)
        {
            bool res = false;

            if (res = base.Interact(player))
            {
                TurnTo(player, 5000);

                player.Out.SendMessage(this.Name + " says, \"" + m_msg + "\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
            }

            return res;
        }

        public override void LoadFromDatabase(DataObject mob)
        {
            base.LoadFromDatabase(mob);
            if (GameServer.Database != null)
            {
                var tcmsg = GameServer.Database.FindObjectByKey<TownCrierMessage>(InternalID);

                if (tcmsg != null)
                {
                    m_msg = tcmsg.Message;
                }
            }
        }

        public override void SaveIntoDatabase()
        {
            base.SaveIntoDatabase();

            if (GameServer.Database != null)
            {
                TownCrierMessage tcmsg = null;

                tcmsg = GameServer.Database.FindObjectByKey<TownCrierMessage>(InternalID);

                if (tcmsg == null)
                {
                    tcmsg = new TownCrierMessage { TownCrierID = InternalID, Message = m_msg };

                    GameServer.Database.AddObject(tcmsg);
                    GameServer.Database.SaveObject(tcmsg);
                }
                else
                {
                    tcmsg.Message = m_msg;
                    GameServer.Database.SaveObject(tcmsg);
                }
            }
        }

        public override void DeleteFromDatabase()
        {
            if (InternalID != null && GameServer.Database != null)
            {
                var tcmsg = GameServer.Database.FindObjectByKey<TownCrierMessage>(InternalID);

                if (tcmsg != null)
                {
                    GameServer.Database.DeleteObject(tcmsg);
                }
            }

            base.DeleteFromDatabase();
        }
    }
}