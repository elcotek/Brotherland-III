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

namespace DOL.GS.Trainer
{
    /// <summary>
    /// Nightshade Trainer
    /// </summary>	
    [NPCGuildScript("Nightshade Trainer", eRealm.Hibernia)]		// this attribute instructs DOL to use this script for all "Nightshade Trainer" NPC's in Albion (multiple guilds are possible for one script)
    public class NightshadeTrainer : GameTrainer
    {
        public override eCharacterClass TrainedClass
        {
            get { return eCharacterClass.Nightshade; }
        }

        public NightshadeTrainer()
            : base()
        {
        }
        /// <summary>
        /// The item template ID
        /// </summary>
        public const string WEAPON_ID1 = "nightshade_slash_sword_item";
        public const string WEAPON_ID2 = "nightshade_thrust_Rapier_item";
        public const string WEAPON_ID30 = "nightshade_item30";
        public const string WEAPON_ID40 = "nightshade_item40";
        public const string WEAPON_ID45 = "nightshade_item45";


        /// <summary>
        /// Interact with trainer
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player)) return false;

            // check if class matches.				
            if (player.CharacterClass.ID == (int)TrainedClass)
            {
                player.Out.SendMessage(this.Name + " says, \"Here for a bit of training, " + player.Name + "? Step up and get it!\"", eChatType.CT_System, eChatLoc.CL_PopupWindow); //popup window on live

                // Trainer Items for level 30 40 45
                if (player.Level >= 30 && player.Level < 40 && player.HasStartWeapon30 == false)
                {
                    player.ReceiveItem(this, WEAPON_ID30, 0);
                    player.HasStartWeapon30 = true;
                    player.Out.SendMessage("Good job " + player.Name + ", here is your personal reward for your way to be a hero!", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
                }
                if (player.Level >= 40 && player.Level < 45 && player.HasStartWeapon40 == false)
                {
                    player.ReceiveItem(this, WEAPON_ID40, 0);
                    player.HasStartWeapon40 = true;
                    player.Out.SendMessage("Good job " + player.Name + ", here is your personal reward for your way to be a hero!", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
                }
                if (player.Level >= 45 && player.HasStartWeapon45 == false)
                {
                    player.ReceiveItem(this, WEAPON_ID45, 0);
                    player.HasStartWeapon45 = true;
                    player.Out.SendMessage("Good job " + player.Name + ", here is my last spend for you, to be a hero!", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
                }
            }
            else
            {
                // perhaps player can be promoted
                if (CanPromotePlayer(player))
                {
                    player.Out.SendMessage(this.Name + " says, \"You have thought this through, I'm sure. Tell me now if you wish to train as a [Nightshade] and follow the Path of Essence.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    if (!player.IsLevelRespecUsed)
                    {
                        OfferRespecialize(player);
                    }
                }
                else
                {
                    CheckChampionTraining(player);
                }
            }
            return true;
        }

        /// <summary>
        /// checks whether a player can be promoted or not
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool CanPromotePlayer(GamePlayer player)
        {
            return (player.Level >= 5 && player.IsCharcterClass(eCharacterClass.Stalker) && (player.Race == (int)eRace.Celt || player.Race == (int)eRace.Elf || player.Race == (int)eRace.Lurikeen));
        }

        /// <summary>
        /// Talk to trainer
        /// </summary>
        /// <param name="source"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public override bool WhisperReceive(GameLiving source, string text)
        {
            if (!base.WhisperReceive(source, text)) return false;
            GamePlayer player = source as GamePlayer;

            if (CanPromotePlayer(player))
            {

                switch (text)
                {
                    case "Nightshade":

                        player.Out.SendMessage(this.Name + " says, \"Very well. Some would think you mad, choosing to walk through life as a Nightshade. It is not meant for everyone, but I think it will suit you," + source.GetName(0, false) + ". Here, from me, a small gift to aid you in your journeys. [slashing] or [thrusting]?\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        break;

                    case "slashing":

                        PromotePlayer(player, (int)eCharacterClass.Nightshade, "Here is your Sword of the Initiate. Welcome to the Defenders of Hibernia.", null);
                        player.ReceiveItem(this, WEAPON_ID1, 0);
                        player.ReceiveItem(this, WEAPON_ID1, 0);
                        break;

                    case "thrusting":

                        PromotePlayer(player, (int)eCharacterClass.Nightshade, "Here is your Rapier of the Initiate. Welcome to the Defenders of Hibernia.", null);
                        player.ReceiveItem(this, WEAPON_ID2, 0);
                        player.ReceiveItem(this, WEAPON_ID2, 0);

                        break;

                }
            }
            return true;
        }
    }
}
    
   	