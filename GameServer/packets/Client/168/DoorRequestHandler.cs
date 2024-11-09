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
using DOL.GS.Keeps;
using DOL.GS.ServerProperties;
using DOL.Language;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DOL.GS.PacketHandler.Client.v168
{
    [PacketHandler(PacketHandlerType.TCP, eClientPackets.DoorRequest, eClientStatus.PlayerInGame)]
    public class DoorRequestHandler : IPacketHandler
    {
        public static int m_handlerDoorID;

        #region IPacketHandler Members




        /// <summary>
        /// door index which is unique
        /// </summary>
        public void HandlePacket(GameClient client, GSPacketIn packet)
        {
            var doorID = (int)packet.ReadInt();
            m_handlerDoorID = doorID;
            var doorState = (byte)packet.ReadByte();
            int doorType = doorID / 100000000;

            int radius = ServerProperties.Properties.WORLD_PICKUP_DISTANCE * 2;
            int zoneDoor = (int)(doorID / 1000000);

            string debugText = String.Empty;

            // For ToA the client always sends the same ID so we need to construct an id using the current zone
            if (client.Player.CurrentRegion.Expansion == (int)eClientExpansion.TrialsOfAtlantis)
            {
                debugText = "ToA DoorID: " + doorID + " ";

                doorID -= zoneDoor * 1000000;
                zoneDoor = client.Player.CurrentZone.ID;
                doorID += zoneDoor * 1000000;
                m_handlerDoorID = doorID;

                // experimental to handle a few odd TOA door issues
                if (client.Player.CurrentRegion.IsDungeon)
                {
                    radius *= 4;
                }
            }

            // debug text
            if (client.Account.PrivLevel > 1 || Properties.ENABLE_DEBUG)
            {
                if (doorType == 7)
                {
                    int ownerKeepId = (doorID / 100000) % 1000;
                    int towerNum = (doorID / 10000) % 10;
                    int keepID = ownerKeepId + towerNum * 256;
                    int componentID = (doorID / 100) % 100;
                    int doorIndex = doorID % 10;
                    client.Out.SendDebugMessage(
                        "Keep Door ID: {0} state:{1} (Owner Keep: {6} KeepID:{2} ComponentID:{3} DoorIndex:{4} TowerNumber:{5})", doorID,
                        doorState, keepID, componentID, doorIndex, towerNum, ownerKeepId);
                }
                else if (doorType == 9)
                {
                    int doorIndex = doorID - doorType * 10000000;
                    client.Out.SendDebugMessage("House DoorID:{0} state:{1} (doorType:{2} doorIndex:{3})", doorID, doorState, doorType,
                                                doorIndex);
                }
                else
                {
                    int fixture = (doorID - zoneDoor * 1000000);
                    int fixturePiece = fixture;
                    fixture /= 100;
                    fixturePiece -= fixture * 100;

                    client.Out.SendDebugMessage("{6}DoorID:{0} state:{1} zone:{2} fixture:{3} fixturePiece:{4} Type:{5}",
                                                doorID, doorState, zoneDoor, fixture, fixturePiece, doorType, debugText);
                }
            }

            var target = client.Player.TargetObject as GameDoor;



            if (client.Player.CurrentRegion.IsDungeon && target != null && !client.Player.IsWithinRadius(target, radius))
            {
                client.Player.Out.SendMessage("You are too far to open this door", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                return;
            }
            else if (target != null && !client.Player.IsWithinRadius(target, 400, true))
            {
                client.Player.Out.SendMessage("You are too far to open this door", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                return;
            }

            //var door = GameServer.Database.SelectObjects<DBDoor>("`InternalID` = @InternalID", new QueryParameter("@InternalID", doorID)).FirstOrDefault();
            var door = GameServer.Database.SelectObjects<DBDoor>("`InternalID` = @InternalID", new QueryParameter("@InternalID", doorID)).FirstOrDefault();

            if (door != null)
            {
                if (doorType == 7 || doorType == 9)
                {
                    new ChangeDoorAction(client.Player, doorID, doorState, radius).Start(1);
                    return;
                }

                if (client.Account.PrivLevel == 1)
                {

                    ////////////////////////////////////////////////////////////////////////////
                    if (door.Locked == 0)
                    {
                        if (door.Health == 0)
                        {
                            new ChangeDoorAction(client.Player, doorID, doorState, radius).Start(1);
                            return;
                        }

                        if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvP)
                        {
                            if (door.Realm != 0)
                            {
                                new ChangeDoorAction(client.Player, doorID, doorState, radius).Start(1);
                                return;
                            }
                        }

                        if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_Normal)
                        {
                            if (client.Player.Realm == (eRealm)door.Realm || door.Realm == 6)
                            {
                                new ChangeDoorAction(client.Player, doorID, doorState, radius).Start(1);
                                return;
                            }
                        }
                    }
                    if (IsRelicGate(doorID))
                    {
                        PortPlayerAtRelictGates(doorID, client.Player, target);
                    }
                }

                if (client.Account.PrivLevel > 1)
                {
                    client.Out.SendDebugMessage("GM: Forcing locked door open.");
                    new ChangeDoorAction(client.Player, doorID, doorState, radius).Start(1);
                    return;
                }
            }

            if (door == null)
            {
                if (doorType != 9 && client.Account.PrivLevel > 1 && client.Player.CurrentRegion.IsInstance == false)
                {
                    if (client.Player.TempProperties.getProperty(DoorMgr.WANT_TO_ADD_DOORS, false))
                    {
                        client.Player.Out.SendCustomDialog(
                            "This door is not in the database. Place yourself nearest to this door and click Accept to add it.", AddingDoor);
                    }
                    else
                    {
                        client.Player.Out.SendMessage(
                            "This door is not in the database. Use '/door show' to enable the add door dialog when targeting doors.",
                            eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    }
                }

                new ChangeDoorAction(client.Player, doorID, doorState, radius).Start(1);
                return;
            }
        }


        public virtual bool IsRelicGate(int doorId)
        {
            switch (doorId)
            {  //Relic Gates
                case 173000301:
                case 173000201:
                case 174061701:
                case 174149601:
                case 170128101:
                case 170127801:
                case 169000501:
                case 169000301:
                case 176233801:

                case 176233901:
                case 175014001:
                case 175013901:
                    {
                        return true;
                    }
            }
            return false;
        }

        public virtual void PortPlayerAtRelictGates(int doorID, GamePlayer player, GameDoor target)
        {

            //Melee Gates Dun Lamhota Relic Gate II
            if (doorID == 173000201)
            {

                if (player != null && player.Realm == eRealm.Hibernia)
                {
                    if (target != null && !player.IsWithinRadius(target, WorldMgr.INTERACT_DISTANCE))
                    {
                        player.Out.SendMessage("You are too far to open this door", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    // tor 201
                    //raus 2537 - 3685
                    //rein 477 - 1665



                    if (player.Heading < 1685)
                    {
                        //Rein
                        player.MoveTo(163, 383451, 574091, 8224, 1003);



                    }
                    else
                    {
                        //Raus
                        player.MoveTo(163, 383663, 574095, 8224, 3041);


                    }
                }

            }

            //Melee Gates Dun Lamhota Relic Gate I
            if (doorID == 173000301)
            {



                if (player != null && player.Realm == eRealm.Hibernia)
                {

                    if (target != null && !player.IsWithinRadius(target, WorldMgr.INTERACT_DISTANCE))
                    {
                        player.Out.SendMessage("You are too far to open this door", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        return;
                    }
                    //tor 301
                    //raus = 2315 - 3524
                    //rein = 294 - 1504

                    if (player.Heading < 1584)
                    {
                        //Rein
                        player.MoveTo(163, 383385, 597803, 8408, 843);



                    }
                    else
                    {
                        //Raus
                        player.MoveTo(163, 383577, 597742, 8408, 2974);


                    }
                }


            }

            //Magic Gates Dun Lamhota Relic Gate I
            if (doorID == 174061701)
            {



                if (player != null && player.Realm == eRealm.Hibernia)
                {
                    if (target != null && !player.IsWithinRadius(target, WorldMgr.INTERACT_DISTANCE))
                    {
                        player.Out.SendMessage("You are too far to open this door", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    //raus: 163, 383577, 597742, 8408, 2974 ok
                    // rein: 163, 383385, y597803, 8408, 843


                    // raus = 1526 - 2286
                    //rein  377 - 3407
                    if (player.Heading > 377 && player.Heading < 1500 || player.Heading > 3407)
                    {
                        //Rein
                        player.MoveTo(163, 459881, 667098, 8088, 3879);



                    }
                    else
                    {
                        //Raus
                        player.MoveTo(163, 459753, 666882, 8088, 1942);


                    }
                }


            }
            //Magic Gates Dun Lamhota Relic Gate II
            if (doorID == 174149601)
            {



                if (player != null && player.Realm == eRealm.Hibernia)
                {

                    if (target != null && !player.IsWithinRadius(target, WorldMgr.INTERACT_DISTANCE))
                    {
                        player.Out.SendMessage("You are too far to open this door", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    //raus: 163, 383577, 597742, 8408, 2974 ok
                    // rein: 163, 383385, y597803, 8408, 843

                    if (player.Heading > 3100)//  rein == false)
                    {
                        //Rein
                        player.MoveTo(163, 473507, 650875, 8088, 3479);



                    }
                    else
                    {
                        //Raus
                        player.MoveTo(163, 473283, 650727, 8088, 1424);


                    }
                }

            }

            //Midgard Gate Stärke Mjollner Relic Gate I
            if (doorID == 170128101)
            {



                if (player != null && player.Realm == eRealm.Midgard)
                {

                    if (target != null && !player.IsWithinRadius(target, WorldMgr.INTERACT_DISTANCE))
                    {
                        player.Out.SendMessage("You are too far to open this door", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    //raus: 163, 383577, 597742, 8408, 2974 ok
                    // rein: 163, 383385, y597803, 8408, 843

                    if (player.Heading > 1524 && player.Heading < 2483)
                    {
                        //Rein
                        player.MoveTo(163, 615396, 311861, 8088, 1997);



                    }
                    else
                    {
                        //Raus
                        player.MoveTo(163, 615401, 312052, 8088, 21);


                    }
                }
            }
            //Midgard Gate Stärke Mjollner Relic Gate II
            if (doorID == 170127801)
            {



                if (player != null && player.Realm == eRealm.Midgard)
                {
                    if (target != null && !player.IsWithinRadius(target, WorldMgr.INTERACT_DISTANCE))
                    {
                        player.Out.SendMessage("You are too far to open this door", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    //raus: 163, 383577, 597742, 8408, 2974 ok
                    // rein: 163, 383385, y597803, 8408, 843

                    if (player.Heading > 1595 && player.Heading < 2724)//  rein == false)
                    {
                        //Rein
                        player.MoveTo(163, 596650, 312067, 8088, 2097);



                    }
                    else
                    {
                        //Raus
                        player.MoveTo(163, 596651, 312239, 8088, 4094);


                    }
                }
            }

            //Midgard Gate power Grallarhorn Relic Gate I
            if (doorID == 169000501)
            {



                if (player != null && player.Realm == eRealm.Midgard)
                {
                    if (target != null && !player.IsWithinRadius(target, WorldMgr.INTERACT_DISTANCE))
                    {
                        player.Out.SendMessage("You are too far to open this door", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        return;
                    }


                    //raus: 163, 383577, 597742, 8408, 2974 ok
                    // rein: 163, 383385, y597803, 8408, 843

                    if (player.Heading > 2708)
                    {
                        //Rein
                        player.MoveTo(163, 704650, 394102, 8024, 3163);



                    }
                    else
                    {
                        //Raus
                        player.MoveTo(163, 704436, 394066, 8024, 1119);


                    }
                }
            }
            //Midgard Gate power Grallarhorn Relic Gate II
            if (doorID == 169000301)
            {



                if (player != null && player.Realm == eRealm.Midgard)
                {
                    if (target != null && !player.IsWithinRadius(target, WorldMgr.INTERACT_DISTANCE))
                    {
                        player.Out.SendMessage("You are too far to open this door", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    //raus: 163, 383577, 597742, 8408, 2974 ok
                    // rein: 163, 383385, y597803, 8408, 843

                    if (player.Heading < 1407 || player.Heading > 3451)
                    {
                        //Rein
                        player.MoveTo(163, 690651, 413370, 8088, 4054);



                    }
                    else
                    {
                        //Raus
                        player.MoveTo(163, 690566, 413210, 8088, 1928);


                    }
                }
            }
            //Albion Gate Strenght Castle Excalibur Relic Gate I
            if (doorID == 176233801)
            {



                if (player != null && player.Realm == eRealm.Albion)
                {
                    if (target != null && !player.IsWithinRadius(target, WorldMgr.INTERACT_DISTANCE))
                    {
                        player.Out.SendMessage("You are too far to open this door", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    //raus: 163, 383577, 597742, 8408, 2974 ok
                    // rein: 163, 383385, y597803, 8408, 843

                    if (player.Heading > 2606)
                    {
                        //Rein
                        player.MoveTo(163, 659112, 570501, 8088, 3055);



                    }
                    else
                    {
                        //Raus
                        player.MoveTo(163, 658942, 570505, 8088, 884);


                    }
                }
            }
            //Albion Gate Strenght Castle Excalibur Relic Gate II
            if (doorID == 176233901)
            {



                if (player != null && player.Realm == eRealm.Albion)
                {
                    if (target != null && !player.IsWithinRadius(target, WorldMgr.INTERACT_DISTANCE))
                    {
                        player.Out.SendMessage("You are too far to open this door", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    //raus: 163, 383577, 597742, 8408, 2974 ok
                    // rein: 163, 383385, y597803, 8408, 843

                    if (player.Heading > 2862)//  rein == false)
                    {
                        //Rein
                        player.MoveTo(163, 655159, 595207, 8088, 3162);



                    }
                    else
                    {
                        //Raus
                        player.MoveTo(163, 655005, 595208, 8088, 1070);


                    }
                }
            }

            //Albion power Castle Mirddin Relic Gate I
            if (doorID == 175014001)
            {



                if (player != null && player.Realm == eRealm.Albion)
                {
                    if (target != null && !player.IsWithinRadius(target, WorldMgr.INTERACT_DISTANCE))
                    {
                        player.Out.SendMessage("You are too far to open this door", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        return;
                    }


                    //raus: 163, 383577, 597742, 8408, 2974 ok
                    // rein: 163, 383385, y597803, 8408, 843

                    if (player.Heading < 883 || player.Heading > 3805)
                    {
                        //Rein
                        player.MoveTo(163, 590571, 666124, 8088, 358);



                    }
                    else
                    {
                        //Raus
                        player.MoveTo(163, 590613, 665891, 8088, 2306);


                    }
                }
            }
            //Albion power Castle Mirddin Relic Gate II
            if (doorID == 175013901)
            {



                if (player != null && player.Realm == eRealm.Albion)
                {
                    if (target != null && !player.IsWithinRadius(target, WorldMgr.INTERACT_DISTANCE))
                    {
                        player.Out.SendMessage("You are too far to open this door", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    //raus: 163, 383577, 597742, 8408, 2974 ok
                    // rein: 163, 383385, y597803, 8408, 843

                    if (player.Heading < 1227)//  rein == false)
                    {
                        //Rein
                        player.MoveTo(163, 574036, 651048, 8088, 584);



                    }
                    else
                    {
                        //Raus
                        player.MoveTo(163, 574157, 650943, 8088, 2544);


                    }
                }
            }
        }
              
                #endregion


            public void AddingDoor(GamePlayer player, byte response)
		    {
			if (response != 0x01)
            {
                return;
            }

            int doorType = m_handlerDoorID/100000000;
            if (doorType == 7)
            {
                PositionMgr.CreateDoor(m_handlerDoorID, player);
            }
            else
            {
                var door = new DBDoor
                {
                    ObjectId = null,
                    InternalID = m_handlerDoorID,
                    Name = "door",
                    Type = m_handlerDoorID / 100000000,
                    Level = 20,
                    Realm = 6,
                    MaxHealth = 2545,
                    Health = 2545,
                    Locked = 0,
                    X = player.X,
                    Y = player.Y,
                    Z = player.Z,
                    Heading = player.Heading
                };
                GameServer.Database.AddObject(door);

                player.Out.SendMessage("Added door " + m_handlerDoorID + " to the database!", eChatType.CT_Important,
                                       eChatLoc.CL_SystemWindow);
                DoorMgr.Init();
            }
		}

		#region Nested type: ChangeDoorAction

		/// <summary>
		/// Handles the door state change actions
		/// </summary>
		protected class ChangeDoorAction : RegionAction
		{
			/// <summary>
			/// The target door Id
			/// </summary>
			protected readonly int m_doorId;

			/// <summary>
			/// The door state
			/// </summary>
			protected readonly int m_doorState;

			/// <summary>
			/// allowed distance to door
			/// </summary>
			protected readonly int m_radius;

			/// <summary>
			/// Constructs a new ChangeDoorAction
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="doorId">The target door Id</param>
			/// <param name="doorState">The door state</param>
			public ChangeDoorAction(GamePlayer actionSource, int doorId, int doorState, int radius)
				: base(actionSource)
			{
				m_doorId = doorId;
				m_doorState = doorState;
				m_radius = radius;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				var player = (GamePlayer) m_actionSource;
				List<IDoor> doorList = DoorMgr.getDoorByID(m_doorId);

				if (doorList.Count > 0)
				{
					bool success = false;
                    for (int i = 0; i < doorList.Count; i++)
					{
                        IDoor mydoor = doorList[i];
                        if (success)
							break;
						if (mydoor is GameKeepDoor)
						{
                            if (player.IsWithinRadius(mydoor, 4000))
                            {
                                player.SendDoorUpdate(mydoor);
                            }

							var door = mydoor as GameKeepDoor;
							//portal keeps left click = right click
							if (door.Component.Keep is GameKeepTower && door.Component.Keep.KeepComponents.Count > 1)
								door.Interact(player);
							success = true;
						}
						else
						{
							if (player.IsWithinRadius(mydoor, m_radius))
							{
								if (m_doorState == 0x01)
                                {
                                    mydoor.Open();
                                }
                                else
                                {
                                    mydoor.Close();
                                }

                                success = true;
							}
						}
					}

					if (!success)
                    {
                        player.Out.SendMessage(
							LanguageMgr.GetTranslation(player.Client.Account.Language, "DoorRequestHandler.OnTick.TooFarAway", doorList[0].Name),
							eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                }
				else
				{
					//new frontiers we don't want this, i.e. relic gates etc
					if (player.CurrentRegionID == 163 && player.Client.Account.PrivLevel == 1)
                    {
                        return;
                    }
                    
                    player.Out.SendDebugMessage("Door {0} not found in door list, opening via GM door hack.", m_doorId);

                    //else basic quick hack
                    var door = new GameDoor
                    {
                        DoorID = m_doorId,
                        X = player.X,
                        Y = player.Y,
                        Z = player.Z,
                        Realm = eRealm.Door,
                        CurrentRegion = player.CurrentRegion
                    };
                    door.Open();
				}
			}
		}

		#endregion
	}
}