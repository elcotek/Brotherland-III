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
using DOL.GS.Effects;
using DOL.Language;



namespace DOL.GS.Commands
{
    [CmdAttribute(
        "&playeffect", //command to handle
        ePrivLevel.GM, //minimum privelege level
        "playeffect [ID]", //command description
        "'/Playeffcect ID <effectID> usable code for play effect are: player.Out.SendSpellEffectAnimation(client.Player, target, (ushort)ID, 0, false, 1); ")] //usage
    public class PlayEffectIDCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length == 1)
            {
                DisplaySyntax(client);
                return;
            }

            GameLiving target = client.Player.TargetObject as GameLiving;
            if (target == null)
                target = client.Player as GameLiving;

           

            if (ushort.TryParse(args[1], out ushort ID) == false)
            {
                DisplaySyntax(client);
                return;
            }

            DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Cast.EffectExecuted", ID.ToString()));
            DummyEffect effect = new DummyEffect((ushort)ID);

            effect.Start(client.Player);
            foreach (GamePlayer player in client.Player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                player.Out.SendSpellEffectAnimation(client.Player, target, (ushort)ID, 0, false, 1);
        }
    }
}