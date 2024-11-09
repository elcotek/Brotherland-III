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
    /// Armsman Trainer
    /// </summary>
    [NPCGuildScript("Armsman Trainer", eRealm.Albion)]		// this attribute instructs DOL to use this script for all "Fighter Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class ArmsmanTrainer : GameTrainer
	{
		public override eCharacterClass TrainedClass
		{
			get { return eCharacterClass.Armsman; }
		}
		/// <summary>
		/// The slash sword item template ID
		/// </summary>
		public const string WEAPON_ID1 = "slash_sword_item";
		/// <summary>
		/// The crush sword item template ID
		/// </summary>
		public const string WEAPON_ID2 = "crush_sword_item";
		/// <summary>
		/// The thrust sword item template ID
		/// </summary>
		public const string WEAPON_ID3 = "thrust_sword_item";
		/// <summary>
		/// The pike polearm item template ID
		/// </summary>
		public const string WEAPON_ID4 = "pike_polearm_item";
        public const string WEAPON_ID30 = "armsman_item30";
        public const string WEAPON_ID40 = "armsman_item40";
        public const string WEAPON_ID45 = "armsman_item45";




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
					player.Out.SendMessage(this.Name + " says, \"Do you desire to [join the Defenders of Albion] and defend our realm as an Armsman?\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
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
			return (player.Level>=5 && player.CharacterClass.ID == (int) eCharacterClass.Fighter && (player.Race == (int) eRace.Briton || player.Race == (int) eRace.Avalonian
			                                                                                         || player.Race == (int) eRace.Highlander || player.Race == (int) eRace.Saracen || player.Race == (int) eRace.Inconnu || player.Race == (int) eRace.HalfOgre
			                                                                                         || player.Race == (int) eRace.AlbionMinotaur));
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
					case "join the Defenders of Albion":
						
						player.Out.SendMessage(this.Name + " says, \"Very well. Choose a weapon, and you shall become one of us. Which would you have, [slashing], [crushing], [thrusting] or [polearms]?\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
						
						break;
					case "slashing":
						
						PromotePlayer(player, (int)eCharacterClass.Armsman, "Here is your Sword of the Initiate. Welcome to the Defenders of Albion.", null);
						player.ReceiveItem(this,WEAPON_ID1, 0);
						
						break;
					case "crushing":
						
						PromotePlayer(player, (int)eCharacterClass.Armsman, "Here is your Mace of the Initiate. Welcome to the Defenders of Albion.", null);
						player.ReceiveItem(this,WEAPON_ID2, 0);
						
						break;
					case "thrusting":
						
						PromotePlayer(player, (int)eCharacterClass.Armsman, "Here is your Rapier of the Initiate. Welcome to the Defenders of Albion.", null);
						player.ReceiveItem(this,WEAPON_ID3, 0);
						
						break;
					case "polearms":
						
						PromotePlayer(player, (int)eCharacterClass.Armsman, "Here is your Pike of the Initiate. Welcome to the Defenders of Albion.", null);
						player.ReceiveItem(this,WEAPON_ID4, 0);
						
						break;
				}
			}
			return true;
		}
	}
}
