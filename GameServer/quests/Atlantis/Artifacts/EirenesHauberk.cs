﻿/*
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
using System;
using System.Collections.Generic;
using System.Text;
using DOL.Events;
using DOL.GS.Quests;
using DOL.Database;
using DOL.GS.PacketHandler;
using System.Collections;

namespace DOL.GS.Quests.Atlantis.Artifacts
{
	/// <summary>
	/// Quest for the Eirene's Hauberk artifact.
	/// </summary>
	/// <author>Aredhel</author>
	class EirenesHauberk : ArtifactQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


		public EirenesHauberk()
			: base() { }

		public EirenesHauberk(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		/// <summary>
		/// This constructor is needed to load quests from the DB.
		/// </summary>
		/// <param name="questingPlayer"></param>
		/// <param name="dbQuest"></param>
        public EirenesHauberk(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Quest initialisation.
		/// </summary>
		public static void Init()
		{
			ArtifactQuest.Init("Eirene's Chestpiece", typeof(EirenesHauberk));
		}

        /// <summary>
        /// Check if player is eligible for this quest.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool CheckQuestQualification(GamePlayer player)
        {
            if (!base.CheckQuestQualification(player))
                return false;

            return (player.Level >= 45);
        }


		/// <summary>
		/// Handle an item given to the scholar.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="item"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public override bool ReceiveItem(GameLiving source, GameLiving target, InventoryItem item)
		{
			if (base.ReceiveItem(source, target, item))
				return true;

			GamePlayer player = source as GamePlayer;
			Scholar scholar = target as Scholar;
			if (player == null || scholar == null)
				return false;

			if (Step == 2 && ArtifactMgr.GetArtifactID(item.Name) == ArtifactID)
			{
				scholar.TurnTo(player);
				if (RemoveItem(player, item))
				{
					Dictionary<String, ItemTemplate> versions = ArtifactMgr.GetArtifactVersions(ArtifactID, (eCharacterClass)player.CharacterClass.ID, (eRealm)player.Realm);

					String reply = String.Empty;

					if (versions.Count > 1)
					{
						reply = "Great! ";
						DisplayStep3(scholar, player, reply);
						Step = 3;
						return true;
					}
					else
					{
						reply = String.Format("The magic of Eirene's Chestpiece is unlocked {0} {1}. {2} {3} {4} {5}, {1}!",
							"and linked now to you,", player.CharacterClass.Name,
							"Please know that if you lose or destroy this Chestpiece, it will be gone",
							"from you forever. I hope it will help you succeed in the trials.",
							"Bring glory to", GlobalConstants.RealmToName(player.Realm));

						Dictionary<String, ItemTemplate>.Enumerator venum = versions.GetEnumerator();
						venum.MoveNext();

						if (GiveItem(scholar, player, ArtifactID, venum.Current.Value))
						{
							scholar.TurnTo(player);
							scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
							FinishQuest();
							return true;
						}
					}
				}
			}

			return false;
		}

		public override bool Interact(Scholar scholar, GamePlayer player)
		{
			if (Step == 3)
			{
				string reply = String.Empty;
				DisplayStep3(scholar, player, reply);
			}

			return false;
		}

		public void DisplayStep3(Scholar scholar, GamePlayer player, string reply)
		{
			Dictionary<String, ItemTemplate> versions = ArtifactMgr.GetArtifactVersions(ArtifactID, (eCharacterClass)player.CharacterClass.ID, (eRealm)player.Realm);

			reply += "Now you need to decide what version of " + ArtifactID + " you would like.  The following are available to you, please choose wisely. ";

			// versions will be in the format Name;  ... strip the ; when displaying, add it back when searching

			foreach (string version in versions.Keys)
			{
				reply += " [" + version.Replace(";", "") + "],";
			}

			reply = reply.TrimEnd(',');

			scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
		}

		/// <summary>
		/// Handle whispers to the scholar.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public override bool WhisperReceive(GameLiving source, GameLiving target, string text)
		{
			if (base.WhisperReceive(source, target, text))
				return true;

			GamePlayer player = source as GamePlayer;
			Scholar scholar = target as Scholar;
			if (player == null || scholar == null)
				return false;

			if (Step == 1 && text.ToLower() == ArtifactID.ToLower())
			{
				String reply = String.Format("There is powerful magic flowing through that artifact, {0}, {1} {2}",
                    player.CharacterClass.Name,
					"but I cannot release it without the spell hidden in the Journal. Please give me Eirene's",
					"Journal now. If you no longer have it, go and find the pages again. I will wait for you.");
				scholar.TurnTo(player);
				scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
				Step = 2;
				return true;
			}

			if (Step == 3)
			{
				Dictionary<String, ItemTemplate> versions = ArtifactMgr.GetArtifactVersions(ArtifactID, (eCharacterClass)player.CharacterClass.ID, (eRealm)player.Realm);
				string version = text + ";;";

				if (versions.ContainsKey(version))
				{
					if (GiveItem(scholar, player, ArtifactID, versions[version]))
					{
						String reply = String.Format("The magic of Eirene's Chestpiece is unlocked {0} {1}. {2} {3} {4} {5}, {1}!",
							"and linked now to you,", player.CharacterClass.Name,
							"Please know that if you lose or destroy this Chestpiece, it will be gone",
							"from you forever. I hope it will help you succeed in the trials.",
							"Bring glory to ", GlobalConstants.RealmToName(player.Realm));
						scholar.TurnTo(player);
						scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
						FinishQuest();
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Description for the current step.
		/// </summary>
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "Defeat Linos.";
					case 2:
						return "Turn in Eirene's Journal in the Hall of Heroes or the Oceanus Haven.";
					case 3:
						return "Choose the version of Eirene's Chestpiece you would like to have.";
					default:
						return base.Description;
				}
			}
		}

		/// <summary>
		/// The name of the quest (not necessarily the same as
		/// the name of the reward).
		/// </summary>
		public override string Name
		{
			get { return "Eirene's Hauberk"; }
		}

		/// <summary>
		/// The reward for this quest.
		/// </summary>
		public override String ArtifactID
		{
			get { return "Eirene's Chestpiece"; }
		}
	}
}

