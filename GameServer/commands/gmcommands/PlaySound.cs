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
using DOL.GS.PacketHandler;
using DOL.Language;
using System;

namespace DOL.GS.Commands
{
    [CmdAttribute(
        "&playsound", //command to handle
        ePrivLevel.GM, //minimum privelege level
        "playsound id [ID] ", //command description
         "playsound inc [1]", //command description
         "playsound dec [1] ", //command description
        "'/PlaySound id <SoundID> usable code for play sound are: player.Out.SendSoundEffect(ID, 0, 0, 0, 0, 0); ",
        "'/PlaySound inc <1> usable code for play sound are: player.Out.SendSoundEffect(ID, 0, 0, 0, 0, 0); ",
        "'/PlaySound dec <1> usable code for play sound are: player.Out.SendSoundEffect(ID, 0, 0, 0, 0, 0); ")] //usage
    public class PlaySoundIDCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private byte m_id;

        /// <summary>
        /// The Ork Spawn
        /// </summary>
        private byte ID
        {
            get { return m_id; }
            set { m_id = value; }
        }

        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length == 1)
            {
                log.ErrorFormat("args lenght fehler! = {0}", args.Length);
                DisplaySyntax(client);
                return;
            }
            switch (args[1].ToLower())
            {
                case "inc":
                    {
                        log.ErrorFormat("args lenght1 = {0}", args.Length);
                        GamePlayer plyer = client.Player.TargetObject as GamePlayer;

                        if (plyer == null)
                            plyer = client.Player;


                       
                        ID += Convert.ToByte(args[2]);

                        byte soundmodel = ID;
                       
                       
                        try
                        {
                            if (soundmodel <= 255)
                            {
                               
                                ID = soundmodel;
                                plyer.Out.SendSoundEffect(ID, 0,0,0,0,0);
                                DisplayMessage(client, "Sound id to: " + ID);
                            }
                            else
                            {
                                DisplayMessage(client, "Highest sound id reached!");
                            }
                        }
                        catch (Exception)
                        {
                            DisplayMessage(client, "Type /item for command overview");
                            return;
                        }
                        break;
                    }
                case "dec":
                    {
                        log.ErrorFormat("args lenght1 = {0}", args.Length);
                        GamePlayer plyer = client.Player.TargetObject as GamePlayer;

                        if (plyer == null)
                            plyer = client.Player;


                        /*
                        if (ushort.TryParse(args[2], out ushort IDs) == false)
                        {

                            log.ErrorFormat("args lenght1 = {0}", args.Length);
                            DisplaySyntax(client);
                            return;
                        }
                        */
                        ID -= Convert.ToByte(args[2]);

                        byte soundmodel = ID;


                        try
                        {
                            if (ID > 0 && ID <= 255)
                            {

                                ID = soundmodel;
                                plyer.Out.SendSoundEffect(ID, 0, 0, 0, 0, 0);
                                DisplayMessage(client, "Increase Sound id to: " + ID);
                            }
                            else
                            {
                                DisplayMessage(client, "Highest sound id reached!");
                            }
                        }
                        catch (Exception)
                        {
                            DisplayMessage(client, "Type /item for command overview");
                            return;
                        }
                        break;
                    }
                case "id":
                    {
                        
                        GamePlayer plyer = client.Player.TargetObject as GamePlayer;

                        if (plyer == null)
                            plyer = client.Player;


                        
                        if (ushort.TryParse(args[2], out ushort IDs) == false)
                        {

                            log.ErrorFormat("args lenght1 = {0}", args.Length);
                            DisplaySyntax(client);
                            return;
                        }
                        
                        ID = Convert.ToByte(args[2]);

                        byte soundmodel = ID;


                        try
                        {
                            if (soundmodel < 255)
                            {

                                ID = soundmodel;
                                plyer.Out.SendRegionEnterSound(ID);
                                DisplayMessage(client, "Sound id to: " + ID);
                            }
                            else
                            {
                                DisplayMessage(client, "Highest sound id reached!");
                            }
                        }
                        catch (Exception)
                        {
                            DisplayMessage(client, "Type /item for command overview");
                            return;
                        }
                        break;
                    }

            }
        }
    }
}
            /*
            GamePlayer player = client.Player.TargetObject as GamePlayer;

            if (player == null)
                player = client.Player;

           
            if (ushort.TryParse(args[1], out ushort ID) == false)
            {
                DisplaySyntax(client);
                return;
            }

            player.Out.SendSoundEffect(ID, 0, 0, 0, 0, 0);
        }*/
        