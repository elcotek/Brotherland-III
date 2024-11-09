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
namespace DOL.GS.Commands
{
    [CmdAttribute(
	"&siegeweapon",
	ePrivLevel.GM,
	"creates siege weapons",
	"/siegeweapon create miniram/lightram/mediumram/heavyram")]
	public class SiegeWeaponCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 3)
			{
				DisplaySyntax(client);
				return;
			}

			switch (args[1].ToLower())
			{
				case "create":
					{
						switch (args[2].ToLower())
						{
							case "miniram":
								{
                                    GameSiegeRam ram = new GameSiegeRam
                                    {
                                        X = client.Player.X,
                                        Y = client.Player.Y,
                                        Z = client.Player.Z,
                                        CurrentRegion = client.Player.CurrentRegion,
                                        Model = 2605,
                                        Level = 0,
                                        Name = "mini ram",
                                        Realm = client.Player.Realm
                                    };
                                    ram.AddToWorld();
									break;
								}
							case "lightram":
								{
                                    GameSiegeRam ram = new GameSiegeRam
                                    {
                                        X = client.Player.X,
                                        Y = client.Player.Y,
                                        Z = client.Player.Z,
                                        CurrentRegion = client.Player.CurrentRegion,
                                        Model = 2600,
                                        Level = 1,
                                        Name = "light ram",
                                        Realm = client.Player.Realm
                                    };
                                    ram.AddToWorld();
									break;
								}
							case "mediumram":
								{
                                    GameSiegeRam ram = new GameSiegeRam
                                    {
                                        X = client.Player.X,
                                        Y = client.Player.Y,
                                        Z = client.Player.Z,
                                        CurrentRegion = client.Player.CurrentRegion,
                                        Model = 2601,
                                        Level = 2,
                                        Name = "medium ram",
                                        Realm = client.Player.Realm
                                    };
                                    ram.AddToWorld();
									break;
								}
							case "heavyram":
								{
                                    GameSiegeRam ram = new GameSiegeRam
                                    {
                                        X = client.Player.X,
                                        Y = client.Player.Y,
                                        Z = client.Player.Z,
                                        CurrentRegion = client.Player.CurrentRegion,
                                        Model = 2602,
                                        Level = 3,
                                        Name = "heavy ram",
                                        Realm = client.Player.Realm
                                    };
                                    ram.AddToWorld();
									break;
								}
						}
						break;
					}
			}
		}
	}
}