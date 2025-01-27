
using DOL.GS.PacketHandler;
using System;

namespace DOL.GS.Keeps
{
    /// <summary>
    /// Class to manage all the dealings with Guilds
    /// </summary>
    public class KeepGuildMgr
    {
        /// <summary>
        /// Sends a message to the guild informing them that a door has been destroyed
        /// </summary>
        /// <param name="door">The door object</param>
        public static void SendDoorDestroyedMessage(GameKeepDoor door)
        {
            string message = door.Name + " in your area " + door.Component.Keep.Name + " has been destroyed";
            SendMessageToGuild(message, door.Component.Keep.Guild);
        }

        /// <summary>
        /// Send message to a guild
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="guild">The guild</param>
        public static void SendMessageToGuild(string message, Guild guild)
        {
            if (guild == null)
                return;

            message = "[Guild] [" + message + "]";
            guild.SendMessageToGuildMembers(message, eChatType.CT_Guild, eChatLoc.CL_ChatWindow);
        }
      
        public static void SendLevelChangeMessage(AbstractGameKeep keep)
        {

            string message = "Your guild's keep " + keep.Name + " is now level " + keep.Level;


            if (keep is GameKeep)
            {
                if (keep.Level != ServerProperties.Properties.MAX_KEEP_LEVEL)
                    message += ", it is on the way to level " + ServerProperties.Properties.MAX_KEEP_LEVEL.ToString();
                SendMessageToGuild(message, keep.Guild);

            }
            if (keep is GameKeepTower)
            {
                if (keep.Level != ServerProperties.Properties.MAX_KEEP_TOWER_LEVEL)
                    message += ", it is on the way to level " + ServerProperties.Properties.MAX_KEEP_TOWER_LEVEL.ToString();
                SendMessageToGuild(message, keep.Guild);
            }
        }


        public static void SendChangeLevelTimeMessage(AbstractGameKeep keep)
        {
            string message;
            string changeleveltext = "";
            int nextlevel;

            byte maxlevel = (byte)ServerProperties.Properties.MAX_KEEP_LEVEL;

            if (keep.Level < maxlevel)
            {
                changeleveltext = "upgrade";
                nextlevel = keep.Level + 1;
            }
            else if (keep.Level > maxlevel)
            {
                changeleveltext = "downgrade";
                nextlevel = keep.Level - 1;
            }
            else if (keep.Level == maxlevel)
            {
                return;
            }
            message = "Your guild is starting to " + changeleveltext + " its area " + keep.Name + " to level " + maxlevel + ".";
            TimeSpan time = keep.ChangeLevelTimeRemaining;
            message += " It will take ";
            if (time.Hours > 0)
                message += time.Hours + " hour(s) ";
            if (time.Minutes > 0)
                message += time.Minutes + " minute(s) ";
            else message += time.Seconds + " second(s)";
            message += " to reach the next level.";
            SendMessageToGuild(message, keep.Guild);
        }
    }
}
