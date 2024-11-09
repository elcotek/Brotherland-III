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
using DOL.GS.PacketHandler;
using DOL.GS.ServerProperties;
using DOL.Language;

namespace DOL.GS.Commands
{
    [CmdAttribute(
        "&dailys",
        ePrivLevel.Player,
        "daily Rewards on/off",
        "/dailys <on/off>")]
    public class DailyRewardCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (Properties.Allow_Daily_Rewards == false)
            {
                DisplayMessage(client, LanguageMgr.GetTranslation(client, "This Feature is currently disabled on the Server!"));
                return;
            }
            if (args.Length < 2)
            {
                DisplaySyntax(client);
                return;
            }

            if (args[1].ToLower().Equals("on"))
            {
                client.Player.DailyReward = true;
                DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.DailyReward.On"));
            }
            else if (args[1].ToLower().Equals("off"))
            {
                client.Player.DailyReward = false;
                DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.DailyReward.Off"));
            }
        }
    }
}