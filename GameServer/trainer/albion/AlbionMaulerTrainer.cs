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


namespace DOL.GS.Trainer
{
    /// <summary>
    /// Mauler Trainer
    /// </summary>
    [NPCGuildScript("Mauler Trainer", eRealm.Albion)]
    public class AlbionMaulerTrainer : GameTrainer
    {
        public override eCharacterClass TrainedClass
        {
            get { return eCharacterClass.MaulerAlb; }
        }

        public const string WEAPON_ID1 = "mauleralb_item_staff";
        public const string WEAPON_ID2 = "mauleralb_item_fist";
        public const string WEAPON_ID30 = "mauleralb_item30";
        public const string WEAPON_ID40 = "mauleralb_item40";
        public const string WEAPON_ID45 = "mauleralb_item45";


        /// <summary>
        /// Interact with trainer
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player)) return false;

            // check if class matches
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
                    player.Out.SendMessage(this.Name + " says, \"Do you desire to [join the Temple of the Iron Fist] and fight for the glorious realm of Albion?\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
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
            return (player.Level >= 5 && player.IsCharcterClass(eCharacterClass.Fighter) && (player.Race == (int)eRace.Briton
                                                                                                      || player.Race == (int)eRace.AlbionMinotaur || player.Race == (int)eRace.Inconnu));
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

            switch (text)
            {
                case "join the Temple of the Iron Fist":
                    // promote player to other class
                    if (CanPromotePlayer(player))
                    {
                        // loose all spec lines
                        player.RemoveAllSkills();
                        player.RemoveAllStyles();

                        // Mauler_Alb = 60
                        PromotePlayer(player, (int)eCharacterClass.MaulerAlb, "Welcome young Mauler. May your time in Albion be rewarding.", null);

                        // drop any equiped-non usable item, in inventory or on the ground if full
                        CheckAbilityToUseItem(player);
                        if (player.Level == 5)
                            player.Out.SendMessage("Wish you a [Staff] or [Fist Wraps]?", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                    break;

                case "Staff":
                    {
                        if (player.Inventory.GetFirstItemByID(WEAPON_ID1, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) == null)
                           

                            player.ReceiveItem(this, WEAPON_ID1, 0);
                        else
                            (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client, "SpellHandler.YouAlreadyHaveThisItem"), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        //player.Out.SendMessage("you already have this Weapon!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }

                    break;


                case "Fist Wraps":
                    {
                        if (player.Inventory.GetFirstItemByID(WEAPON_ID2, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) == null)
                        {
                            player.ReceiveItem(this, WEAPON_ID2, 0);
                            player.ReceiveItem(this, WEAPON_ID2, 0);
                        }
                        else
                            //player.Out.SendMessage("you already have the Weapons!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client, "SpellHandler.YouAlreadyHaveThisItem"), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }

                        break;
                    }

                    return true;
            }
        }
    }

  