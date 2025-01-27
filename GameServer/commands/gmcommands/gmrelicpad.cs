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

//Emblem = emblem,
namespace DOL.GS.Commands
{
    [CmdAttribute(
		"&gmrelicpad",
		ePrivLevel.GM,
		"GMCommands.GMRelicPad.Description",
		"GMCommands.GMRelicPad.Usage")]
	public class GMRelicPadCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length != 4 || (args[1] != "magic" && args[1] != "strength"))
			{
				DisplaySyntax(client);
				return;
			}

			ushort emblem = ushort.Parse(args[3]);
			emblem += (ushort)((args[1] == "magic") ? 10 : 0);

            GameRelicPad pad = new GameRelicPad
            {
                Name = args[2],
                Realm = (eRealm)byte.Parse(args[3]),
                CurrentRegionID = client.Player.CurrentRegionID,
                X = client.Player.X,
                Y = client.Player.Y,
                Z = client.Player.Z,
                Heading = client.Player.Heading
            };
            pad.AddToWorld();
			pad.SaveIntoDatabase();
		}
	}
}
