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
using DOL.Language;
using System;

namespace DOL.GS.Trainer
{
    /// <summary>
    /// Valkyrie Trainer
    /// </summary>
    [NPCGuildScript("Valkyrie Trainer", eRealm.Midgard)]        // this attribute instructs DOL to use this script for all "Valkyrie Trainer" NPC's in Albion (multiple guilds are possible for one script)
    public class ValkyrieTrainer : GameTrainer
    {
        public override eCharacterClass TrainedClass
        {
            get { return eCharacterClass.Valkyrie; }
        }

        public const string WEAPON_ID0 = "valkyrie_item_sword_onehanded";
        public const string WEAPON_ID1 = "valkyrie_item_sword";
        public const string WEAPON_ID2 = "valkyrie_item_spear";
        public const string WEAPON_ID30 = "Valkyrie_item30";
        public const string WEAPON_ID40 = "Valkyrie_item40";
        public const string WEAPON_ID45 = "Valkyrie_item45";

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
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "ValkyrieTrainer.Interact.Text2", this.Name, player.Name), eChatType.CT_System, eChatLoc.CL_ChatWindow);

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
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "ValkyrieTrainer.Interact.Text1", this.Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
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
            return (player.Level >= 5 && player.DBCharacter.Gender == 1 && player.IsCharcterClass(eCharacterClass.Viking) && (player.Race == (int)eRace.Dwarf || player.Race == (int)eRace.Norseman
                                                                                                                                       || player.Race == (int)eRace.Frostalf));
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

            if (lowerCase == LanguageMgr.GetTranslation(player.Client.Account.Language, "ValkyrieTrainer.WhisperReceiveCase.Text1"))
            {
                // promote player to other class
                if (CanPromotePlayer(player))
                {
                    PromotePlayer(player, (int)eCharacterClass.Valkyrie, LanguageMgr.GetTranslation(player.Client.Account.Language, "ValkyrieTrainer.WhisperReceive.Text1"), null);

                    CheckAbilityToUseItem(player);
                }
            }
            else if ((player.Inventory.GetFirstItemByID(WEAPON_ID1, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) == null) &&
                     (player.Inventory.GetFirstItemByID(WEAPON_ID2, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) == null) &&
                    (player.Inventory.GetFirstItemByID(WEAPON_ID0, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) == null))
            {
                if (lowerCase == LanguageMgr.GetTranslation(player.Client.Account.Language, "ValkyrieTrainer.WhisperReceiveCase.Text2"))
                {
                    player.ReceiveItem(this, WEAPON_ID1, 0);
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "ValkyrieTrainer.WhisperReceive.Text2"), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }
                if (lowerCase == LanguageMgr.GetTranslation(player.Client.Account.Language, "ValkyrieTrainer.WhisperReceiveCase.Text4"))
                {
                    player.ReceiveItem(this, WEAPON_ID0, 0);
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "ValkyrieTrainer.WhisperReceive.Text2"), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }
                else if (lowerCase == LanguageMgr.GetTranslation(player.Client, "ValkyrieTrainer.WhisperReceiveCase.Text3"))
                {
                    player.ReceiveItem(this, WEAPON_ID2, 0);
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "ValkyrieTrainer.WhisperReceive.Text2"), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }
            }
            return true;
        }
    }
}
