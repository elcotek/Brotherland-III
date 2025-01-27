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
using DOL.GS.Spells;

namespace DOL.GS.Commands
{
    [CmdAttribute(
		"&morph", //command to handle
		ePrivLevel.GM, //minimum privelege level
		"Temporarily changes the target player's model", //command description
		"'/morph <modelID> [time]' to change into <modelID> for [time] minutes (default=10)")] //usage
	public class MorphCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				DisplaySyntax( client );
				return;
			}

			GamePlayer player = client.Player.TargetObject as GamePlayer;

			if ( player == null )
				player = client.Player;


            if (ushort.TryParse(args[1], out ushort model) == false)
            {
                DisplaySyntax(client);
                return;
            }

            int duration = 10;

			if ( args.Length > 2 )
			{
				if ( int.TryParse( args[2], out duration ) == false )
					duration = 10;
			}

            DBSpell dbSpell = new DBSpell
            {
                Name = "GM Morph",
                Description = "Target has been shapechanged.",
                ClientEffect = 8000,
                Icon = 805,
                Target = "Realm",
                Range = 4000,
                Power = 0,
                CastTime = 0,
                Type = "Morph",
                Duration = duration * 60,
                LifeDrainReturn = model
            };

            Spell morphSpell = new Spell( dbSpell, 0 );
			SpellLine gmLine = new SpellLine( "GMSpell", "GM Spell", "none", false );

			ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler( client.Player, morphSpell, gmLine );

			if ( spellHandler == null )
			{
				DisplayMessage( client, "Unable to create spell handler." );
			}
			else
			{
				spellHandler.StartSpell( player );
			}
		}
	}
}