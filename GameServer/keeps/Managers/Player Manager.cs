
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;
using System;

namespace DOL.GS.Keeps
{
	/// <summary>
	/// The type of interaction we check for to handle lord permission checks
	/// </summary>
	public enum eInteractType
	{
		/// <summary>
		/// Claim the Area
		/// </summary>
		Claim,
		/// <summary>
		/// Release the Area
		/// </summary>
		Release,
		/// <summary>
		/// Change the level of the Area
		/// </summary>
		ChangeLevel,
	}

	/// <summary>
	/// Class to manage all the dealings with Players
	/// </summary>
	public class PlayerMgr
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Sends a message to all players to notify them of the keep capture
		/// </summary>
		/// <param name="keep">The keep object</param>
		public static void BroadcastCapture(AbstractGameKeep keep)
		{
			string message = String.Empty;
			if (keep.Realm != eRealm.None)
			{
				message = string.Format(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerManager.BroadcastCapture.Captured", GlobalConstants.RealmToName((eRealm)keep.Realm), keep.Name));
			}
			else
			{
				message = string.Format("{0} has been captured!", keep.Name);
			}

			/*
			switch (GameServer.Instance.Configuration.ServerType)
			{
				case eGameServerType.GST_Normal:
					{
						message = string.Format("The forces of {0} have captured {1}!", GlobalConstants.RealmToName((eRealm)keep.Realm), keep.Name);
						break;
					}
				case eGameServerType.GST_PvP:
					{
						string defeatersStr = "";
						message = string.Format("The forces of {0} have defeated the defenders of {1}!", defeatersStr, keep.Name);
						break;
					}
			}*/

			BroadcastMessage(message, eRealm.None);
			NewsMgr.CreateNews(message, keep.Realm, eNewsType.RvRGlobal, false);
		}

		/// <summary>
		/// Sends a message to all players to notify them of the raize
		/// </summary>
		/// <param name="keep">The keep object</param>
		/// <param name="realm">The raizing realm</param>
		public static void BroadcastRaize(AbstractGameKeep keep, eRealm realm)
		{
			string message = string.Format(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerManager.BroadcastRaize.Razed", keep.Name, GlobalConstants.RealmToName(realm)));
			BroadcastMessage(message, eRealm.None);
			NewsMgr.CreateNews(message, keep.Realm, eNewsType.RvRGlobal, false);
		}

		/// <summary>
		/// Sends a message to all players of a realm, to notify them of a claim
		/// </summary>
		/// <param name="keep">The keep object</param>
		public static void BroadcastClaim(AbstractGameKeep keep)
		{
			BroadcastMessage(string.Format(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerManager.BroadcastClaim.Claimed", keep.Guild.Name, keep.Name)), (eRealm)keep.Realm);
		}

		/// <summary>
		/// Sends a message to all players of a realm, to notify them of a release
		/// </summary>
		/// <param name="keep">The keep object</param>
		public static void BroadcastRelease(AbstractGameKeep keep)
		{
			BroadcastMessage(string.Format(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerManager.BroadcastRelease.LostControl", keep.Guild.Name, keep.Name)), (eRealm)keep.Realm);
		}

		/// <summary>
		/// Method to broadcast messages, if eRealm.None all can see,
		/// else only the right realm can see
		/// </summary>
		/// <param name="message">The message</param>
		/// <param name="realm">The realm</param>
		public static void BroadcastMessage(string message, eRealm realm)
		{
			foreach (GameClient client in WorldMgr.GetAllClients())
			{
				if (client.Player == null)
					continue;
				if ((client.Account.PrivLevel != 1 || realm == eRealm.None) || client.Player.Realm == realm)
				{
					client.Out.SendMessage(message, eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				}
			}
		}

		/// <summary>
		/// Method to popup message on area enter
		/// </summary>
		/// <param name="player">The target of the message</param>
		/// <param name="message">The message</param>
		public static void PopupAreaEnter(GamePlayer player, string message)
		{
			/*
			 * Blood of the Realm has claimed this outpost.
			 */
			player.Out.SendMessage(message + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			player.Out.SendMessage(message, eChatType.CT_ScreenCenterSmaller, eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// Method to tell us if a player can interact with the lord to do certain tasks
		/// </summary>
		/// <param name="player">The player object</param>
		/// <param name="keep">The area object</param>
		/// <param name="type">The type of interaction</param>
		/// <returns></returns>
		public static bool IsAllowedToInteract(GamePlayer player, AbstractGameKeep keep, eInteractType type)
		{
			if (player.Client.Account.PrivLevel > 1)
				return true;
			if (player.Realm != keep.Realm)
			{

				return false;
			}
			if (player.Guild == null)
				return false;


			if (keep.InCombat)
			{
				log.DebugFormat("KEEPWARNING: {0} attempted to {1} {2} while in combat.", player.Name, type, keep.Name);
				return false;
			}

			switch (type)
			{
				case eInteractType.Claim:
					{

						if (keep.Guild != null)
						{
							player.Out.SendMessage("This Keep is alredy claimed.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return false;
						}


						if (ServerProperties.Properties.CLAIM_NUM > 1)
						{
							if (player.Group == null)
							{


								player.Out.SendMessage("You must be in a group to claim.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return false;

							}
						}

						if (!player.GuildRank.Claim)
						{
							player.Out.SendMessage("You do not have high enough privilage in your Guild to claim.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return false;
						}

						break;
					}
				case eInteractType.Release:
					{

						if (keep.Guild == null)
							return false;
						if (keep.Guild != player.Guild)
							return false;
						if (!player.GuildRank.Claim)
							return false;
						break;
					}
				case eInteractType.ChangeLevel:
					{
						if (keep.Guild == null)
							return false;
						if (keep.Guild != player.Guild)
							return false;
						if (!player.GuildRank.Claim)
							return false;
						break;
					}
			}
			return true;
		}

		/// <summary>
		/// Method to update stats for all players who helped kill lord
		/// </summary>
		/// <param name="lord">The lord object</param>
		public static void UpdateStats(GuardLord lord)
		{
			foreach (var de in lord.XPGainers)
			{
				var player = de.Key as GamePlayer;
				if (de.Key is GameNPC npc && npc.Brain is IControlledBrain brain)
					player = brain.GetPlayerOwner();
				if (player != null)
				{
					if (lord.Component.Keep is GameKeep)
						player.CapturedKeeps++;
					else
						player.CapturedTowers++;
				}
			}
		}
	}
}