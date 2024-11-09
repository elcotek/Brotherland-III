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
using System.Threading.Tasks;

namespace DOL.GS.Commands
{
	[CmdAttribute("&stuck",
		ePrivLevel.Player, //minimum privelege level
		"Removes the player from the world and put it to a safe location", //command description
		"/stuck")] //usage
	public class StuckCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public void OnCommand(GameClient client, string[] args)
		{
			if (IsSpammingCommand(client.Player, "stuck"))
				return;

			

			
			client.Player.Stuck = true;
			Task<bool> quit = client.Player.Quit(false);
			if (!quit.Result)
			{
				//log.Error("Nicht stuck!");
				client.Player.Stuck = false;
			}
			
		}
	}
}