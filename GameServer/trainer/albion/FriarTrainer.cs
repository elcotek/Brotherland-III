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

namespace DOL.GS.Trainer
{
    /// <summary>
    /// Friar Trainer
    /// </summary>
    [NPCGuildScript("Friar Trainer", eRealm.Albion)]        // this attribute instructs DOL to use this script for all "Friar Trainer" NPC's in Albion (multiple guilds are possible for one script)
    public class FriarTrainer : GameTrainer
    {
        public override eCharacterClass TrainedClass
        {
            get { return eCharacterClass.Friar; }
        }

        /// <summary>
        /// The free starter armor from trainer
        /// </summary>
        public const string Staff_ID1 = "friar_Staff_item";
        public const string ARMOR_ID1 = "friar_item";
        public const string ARMOR_ID2 = "chaplains_robe";
        public const string ARMOR_ID3 = "robes_of_the_neophyte";
        public const string WEAPON_ID30 = "friar_item30";
        public const string WEAPON_ID40 = "friar_item40";
        public const string WEAPON_ID45 = "friar_item45";

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
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "FriarTrainer.Interact.Text2", this.Name, player.CharacterClass.Name), eChatType.CT_System, eChatLoc.CL_ChatWindow);

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
                if (player.Level >= 10 && player.Level < 15)
                {
                    if (player.Inventory.GetFirstItemByID(ARMOR_ID3, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) == null)
                    {
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "FriarTrainer.Interact.Text4", this.Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        addGift(ARMOR_ID3, player);
                    }
                    if (player.Inventory.GetFirstItemByID(ARMOR_ID1, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) == null)
                    { }
                    else
                    {
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "FriarTrainer.Interact.Text3", this.Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                }
            }
            else
            {
                // perhaps player can be promoted
                if (CanPromotePlayer(player))
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "FriarTrainer.Interact.Text1", this.Name, player.CharacterClass.Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
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
            return (player.Level >= 5 && player.IsCharcterClass(eCharacterClass.Acolyte) && (player.Race == (int)eRace.Briton || player.Race == (int)eRace.Highlander));
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
            String lowerCase = text.ToLower();

            if (lowerCase == LanguageMgr.GetTranslation(player.Client.Account.Language, "FriarTrainer.WhisperReceiveCase.Text1"))
            {
                // promote player to other class
                if (CanPromotePlayer(player))
                {
                    PromotePlayer(player, (int)eCharacterClass.Friar, LanguageMgr.GetTranslation(player.Client.Account.Language, "FriarTrainer.WhisperReceive.Text1"), null);
                    addGift(ARMOR_ID1, player);
                    addGift(Staff_ID1, player);
                }
            }
            return true;
        }

        /// <summary>
        /// For Recieving Friar Item.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool ReceiveItem(GameLiving source, InventoryItem item)
        {
            if (source == null || item == null) return false;

            GamePlayer player = source as GamePlayer;

            if (player.Level >= 10 && player.Level < 15 && item.Id_nb == ARMOR_ID1)
            {
                player.Inventory.RemoveCountFromStack(item, 1);
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "FriarTrainer.ReceiveItem.Text1", this.Name, player.Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                addGift(ARMOR_ID2, player);
            }
            return base.ReceiveItem(source, item);
        }
    }
}
