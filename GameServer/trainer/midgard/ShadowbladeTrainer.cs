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
    /// Shadowblade Trainer
    /// </summary>
    [NPCGuildScript("Shadowblade Trainer", eRealm.Midgard)]		// this attribute instructs DOL to use this script for all "Shadowblade Trainer" NPC's in Albion (multiple guilds are possible for one script)
    public class ShadowbladeTrainer : GameTrainer
    {
        public override eCharacterClass TrainedClass
        {
            get { return eCharacterClass.Shadowblade; }
        }
        /// <summary>
        /// The slash sword item template ID
        /// </summary>
        public const string WEAPON_ID1 = "shadowblade_sword_item";
        /// <summary>
        /// The thrust sword item template ID
        /// </summary>
        public const string WEAPON_ID2 = "shadowblade_axe_item";

        public const string WEAPON_ID30 = "Shadowblade_item30";
        public const string WEAPON_ID40 = "Shadowblade_item40";
        public const string WEAPON_ID45 = "Shadowblade_item45";

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
                OfferTraining(player);

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
                    player.Out.SendMessage(this.Name + " says, \"Do you desire to [join the House of Loki] and defend our realm as a Shadowblade?\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
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
        /// checks wether a player can be promoted or not
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool CanPromotePlayer(GamePlayer player)
        {
            return (player.Level >= 5 && player.IsCharcterClass(eCharacterClass.MidgardRogue) && (player.Race == (int)eRace.Kobold || player.Race == (int)eRace.Norseman
                                                                                                          || player.Race == (int)eRace.Valkyn || player.Race == (int)eRace.Frostalf));
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
                    case "join the House of Loki":

                        player.Out.SendMessage(this.Name + " says, \"Very well. Welcome young Shadowblade! May your time in Midgard army be rewarding! It is not meant for everyone, but I think it will suit you," + source.GetName(0, false) + ". Here, from me, a small gift to aid you in your journeys. [sword] or [axe]?\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        break;

                    case "sword":

                        PromotePlayer(player, (int)eCharacterClass.Shadowblade, "Here is your Sword of the Initiate. Welcome to the Defenders of Midgard.", null);
                        player.ReceiveItem(this, WEAPON_ID1, 0);

                        break;

                    case "axe":

                        PromotePlayer(player, (int)eCharacterClass.Shadowblade, "Here is your Rapier of the Initiate. Welcome to the Defenders of Midgard.", null);
                        player.ReceiveItem(this, WEAPON_ID2, 0);

                        break;

                }
            }
            return true;
        }
    }
}
 
  