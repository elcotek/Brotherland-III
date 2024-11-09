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

/*
  * New system by Niko jan 2009
  */

using DOL.Database;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.PacketHandler.Client.v168;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DOL.GS.Commands
{
    [Cmd(
		"&door",
		ePrivLevel.GM,
		"GMCommands.door.Description",
		"GMCommands.door.Add",
		"GMCommands.door.Update",
		"GMCommands.door.Delete",
		"GMCommands.door.Name",
		"GMCommands.door.Level",
		"GMCommands.door.Realm",
		"GMCommands.door.Guild",
        "GMCommands.Door.Sound",
		"GMCommands.door.Info",
		"GMCommands.door.Heal",
		"GMCommands.door.Locked",
		"GMCommands.door.Unlocked")]
	public class DoorCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		private int DoorID;
		private int doorType;
		private string Realmname;
		private string statut;

		#region ICommandHandler Members

		public void OnCommand(GameClient client, string[] args)
		{
			

			if (args.Length > 1 && args[1] == "show" && client.Player != null)
			{
				if (client.Player.TempProperties.getProperty(DoorMgr.WANT_TO_ADD_DOORS, false))
				{
					client.Player.TempProperties.removeProperty(DoorMgr.WANT_TO_ADD_DOORS);
					client.Out.SendMessage("You will no longer be shown the add door dialog.", eChatType.CT_System,
					                       eChatLoc.CL_SystemWindow);
				}
				else
				{
					client.Player.TempProperties.setProperty(DoorMgr.WANT_TO_ADD_DOORS, true);
					client.Out.SendMessage("You will now be shown the add door dialog if door is not found in the DB.",
					                       eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}

				return;
			}

			if (client.Player.TargetObject == null)
			{
				client.Out.SendMessage("You must target a door", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (client.Player.TargetObject != null &&
			    (client.Player.TargetObject is GameNPC || client.Player.TargetObject is GamePlayer))
			{
				client.Out.SendMessage("You must target a door", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			GameDoor targetDoor = null;
			if (client.Player.TargetObject != null && (client.Player.TargetObject is GameDoor || client.Player.TargetObject is GameKeepDoor))
			{
				

				if (client.Player.TargetObject is GameDoor)
				{
				

					targetDoor = client.Player.TargetObject as GameDoor;
				}
				


				targetDoor = (GameDoor)client.Player.TargetObject;
				DoorID = targetDoor.DoorID;
				doorType = targetDoor.DoorID/100000000;
			}
			
			if (targetDoor == null)
            {
				return;
            }

			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return;
			}

			switch (args[1])
			{
				case "name":
					name(client, targetDoor, args);
					break;
				case "guild":
					guild(client, targetDoor, args);
					break;
				case "level":
					level(client, targetDoor, args);
					break;
				case "realm":
					realm(client, targetDoor, args);
					break;
				case "info":
					info(client, targetDoor);
					break;
				case "heal":
					heal(client, targetDoor);
					break;
				case "locked":
					locked(client, targetDoor);
					break;
				case "unlocked":
					unlocked(client, targetDoor);
					break;
				case "kill":
					kill(client, targetDoor, args);
					break;
				case "delete":
					delete(client, targetDoor);
					break;
				case "add":
					Add(client, targetDoor);
					break;
				case "update":
					Update(client, targetDoor);
					break;
				case "sound":
					sound(client, targetDoor, args);
					break;

				default:
					DisplaySyntax(client);
					return;
			}
		}

		#endregion

		private void Add(GameClient client, GameDoor targetDoor)
		{
            var DOOR = GameServer.Database.SelectObjects<DBDoor>("`InternalID` = @InternalID", new QueryParameter("@InternalID", DoorID)).FirstOrDefault();


            if (DOOR != null)
			{
				client.Out.SendMessage("The door is already in the database", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (DOOR == null)
			{
				if (doorType != 7 && doorType != 9)
				{
                    var door = new DBDoor
                    {
                        ObjectId = null,
                        InternalID = DoorID,
                        Name = "door",
                        Type = DoorID / 100000000,
                        Level = 20,
                        Realm = 6,
                        X = targetDoor.X,
                        Y = targetDoor.Y,
                        Z = targetDoor.Z,
                        Heading = targetDoor.Heading,
                        Health = 2545
                    };
                    GameServer.Database.AddObject(door);
					(targetDoor).AddToWorld();
					client.Player.Out.SendMessage("Added door ID:" + DoorID + "to the database", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					
					
					return;
				}
			}
		}

		private void Update(GameClient client, GameDoor targetDoor)
		{
			delete(client, targetDoor);

			if (targetDoor != null)
			{
				if (doorType != 7 && doorType != 9)
				{
                    var door = new DBDoor
                    {
                        ObjectId = null,
                        InternalID = DoorID,
                        Name = "door",
                        Type = DoorID / 100000000,
                        Level = targetDoor.Level,
                        Realm = (byte)targetDoor.Realm,
                        Health = targetDoor.Health,
                        Locked = targetDoor.Locked,
                        X = client.Player.X,
                        Y = client.Player.Y,
                        Z = client.Player.Z,
                        Heading = client.Player.Heading
                    };
                    GameServer.Database.AddObject(door);
					(targetDoor).AddToWorld();
					
					client.Player.Out.SendMessage("Added door " + DoorID + " to the database", eChatType.CT_Important,
					                              eChatLoc.CL_SystemWindow);
					return;
				}
			}
		}

		private void delete(GameClient client, GameDoor targetDoor)
		{
            var DOOR = GameServer.Database.SelectObjects<DBDoor>("`InternalID` = @InternalID", new QueryParameter("@InternalID", DoorID)).FirstOrDefault();


            if (DOOR != null)
			{
				GameServer.Database.DeleteObject(DOOR);
				client.Out.SendMessage("Door removed (ID:" + DOOR.InternalID + ")", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				
				return;
			}
			if (DOOR == null)
			{
				client.Out.SendMessage("This door doesn't exist in the database", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
		}


		private void name(GameClient client, GameDoor targetDoor, string[] args)
		{
			string doorName = String.Empty;

			if (args.Length > 2)
				doorName = String.Join(" ", args, 2, args.Length - 2);

			if (doorName != String.Empty)
			{
				targetDoor.Name = CheckName(doorName, client);
				targetDoor.SaveIntoDatabase();
				client.Out.SendMessage("You changed the door name to " + targetDoor.Name, eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);
			}
			else
			{
				DisplaySyntax(client, args[1]);
			}
		}

		private void sound(GameClient client, GameDoor targetDoor, string[] args)
		{
			uint doorSound = 0;

			try
			{
				if (args.Length > 2)
				{
					doorSound = Convert.ToUInt16(args[2]);
					targetDoor.Flag = doorSound;
					targetDoor.SaveIntoDatabase();
					client.Out.SendMessage("You set the door sound to " + doorSound, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					DisplaySyntax(client, args[1]);
				}
			}
			catch
			{
				DisplaySyntax(client, args[1]);
			}
		}

		private void guild(GameClient client, GameDoor targetDoor, string[] args)
		{
			string guildName = String.Empty;

			if (args.Length > 2)
				guildName = String.Join(" ", args, 2, args.Length - 2);

			if (guildName != String.Empty)
			{
				targetDoor.GuildName = CheckGuildName(guildName, client);
				targetDoor.SaveIntoDatabase();
				client.Out.SendMessage("You changed the door guild to " + targetDoor.GuildName, eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);
			}
			else
			{
				if (targetDoor.GuildName != "")
				{
					targetDoor.GuildName = "";
					targetDoor.SaveIntoDatabase();
					client.Out.SendMessage("Door guild removed", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
					DisplaySyntax(client, args[1]);
			}
		}

		private void level(GameClient client, GameDoor targetDoor, string[] args)
		{
			byte level;

			try
			{
				level = Convert.ToByte(args[2]);
				targetDoor.Level = level;
				targetDoor.Health = targetDoor.MaxHealth;
				targetDoor.SaveIntoDatabase();
				client.Out.SendMessage("You changed the door level to " + targetDoor.Level, eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
			}
		}

		private void realm(GameClient client, GameDoor targetDoor, string[] args)
		{
			byte realm;

			try
			{
				realm = Convert.ToByte(args[2]);
				targetDoor.Realm = (eRealm) realm;
				targetDoor.SaveIntoDatabase();
				client.Out.SendMessage("You changed the door realm to " + targetDoor.Realm, eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
			}
		}

		private void info(GameClient client, GameDoor targetDoor)
		{

			if (targetDoor != null)
			{
				if (targetDoor.Realm == eRealm.None)
					Realmname = "None";

				if (targetDoor.Realm == eRealm.Albion)
					Realmname = "Albion";

				if (targetDoor.Realm == eRealm.Midgard)
					Realmname = "Midgard";

				if (targetDoor.Realm == eRealm.Hibernia)
					Realmname = "Hibernia";

				if (targetDoor.Realm == eRealm.Door)
					Realmname = "All";

				if (targetDoor.Locked == 1)
					statut = " Locked";

				if (targetDoor.Locked == 0)
					statut = " Unlocked";

				int doorType = DoorRequestHandler.m_handlerDoorID / 100000000;

				var info = new List<string>
			{
				" + Info sur la porte :  " + targetDoor.Name,
				"  ",
				" + Name : " + targetDoor.Name,
				" + ID : " + DoorID,
				" + Realm : " + (int)targetDoor.Realm + " : " + Realmname,
				" + Level : " + targetDoor.Level,
				" + Guild : " + targetDoor.GuildName,
				" + Health : " + targetDoor.Health + " / " + targetDoor.MaxHealth,
				" + Statut : " + statut,
				" + Type : " + doorType,
				" + X : " + targetDoor.X,
				" + Y : " + targetDoor.Y,
				" + Z : " + targetDoor.Z,
				" + Heading : " + targetDoor.Heading
			};

				client.Out.SendCustomTextWindow("Door Information", info);

			}
		}

		private void heal(GameClient client, GameDoor targetDoor)
		{
			targetDoor.Health = targetDoor.MaxHealth;
			targetDoor.SaveIntoDatabase();
			client.Out.SendMessage("You change the door health to " + targetDoor.Health, eChatType.CT_System,
			                       eChatLoc.CL_SystemWindow);
		}

		private void locked(GameClient client, GameDoor targetDoor)
		{
			targetDoor.Locked = 1;
			targetDoor.SaveIntoDatabase();
			client.Out.SendMessage("Door " + targetDoor.Name + " is locked", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		private void unlocked(GameClient client, GameDoor targetDoor)
		{
			targetDoor.Locked = 0;
			targetDoor.SaveIntoDatabase();
			client.Out.SendMessage("Door " + targetDoor.Name + " is unlocked", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}


		private void kill(GameClient client, GameDoor targetDoor, string[] args)
		{
			try
			{
				targetDoor.AddAttacker(client.Player);
				targetDoor.AddXPGainer(client.Player, targetDoor.Health);
				targetDoor.Die(client.Player);
				targetDoor.XPGainers.Clear();
				client.Out.SendMessage("Door " + targetDoor.Name + " health reaches 0", eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);
			}
			catch (Exception e)
			{
				client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		private string CheckName(string name, GameClient client)
		{
			if (name.Length > 47)
				client.Out.SendMessage("The door name must not be longer than 47 bytes", eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);
			return name;
		}

		private string CheckGuildName(string name, GameClient client)
		{
			if (name.Length > 47)
				client.Out.SendMessage("The guild name is " + name.Length + ", but only 47 bytes 'll be displayed",
				                       eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return name;
		}
	}
}