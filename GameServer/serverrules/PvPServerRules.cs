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
using System;
using System.Collections;
using DOL;
using DOL.Language;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using System.Collections.Generic;

namespace DOL.GS.ServerRules
{
	/// <summary>
	/// Set of rules for "PvP" server type.
	/// </summary>
	[ServerRules(eGameServerType.GST_PvP)]
	public class PvPServerRules : AbstractServerRules
	{
		public override string RulesDescription()
		{
			return "standard PvP server rules";
		}

		//release city
		//alb=26315, 21177, 8256, dir=0
		//mid=24664, 21402, 8759, dir=0
		//hib=15780, 22727, 7060, dir=0
		//0,"You will now release automatically to your home city in 8 more seconds!"

		//TODO: 2min immunity after release if killed by player

		/// <summary>
		/// Constructor
		/// </summary>
		public PvPServerRules()
		{
			GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(OnGameEntered));
			GameEventMgr.AddHandler(GamePlayerEvent.RegionChanged, new DOLEventHandler(OnRegionChanged));
			GameEventMgr.AddHandler(GamePlayerEvent.Released, new DOLEventHandler(OnReleased));
			m_invExpiredCallback = new GamePlayer.InvulnerabilityExpiredCallback(ImmunityOverCallback);
		}

		#region PvP Immunity

		/// <summary>
		/// TempProperty set if killed by player
		/// </summary>
		protected const string KILLED_BY_PLAYER_PROP = "PvP killed by player";

		/// <summary>
		/// Level at which players safety flag has no effect
		/// </summary>
		protected int m_safetyLevel = 10;

        /// <summary>
        /// Called when player enters the game for first time
        /// </summary>
        /// <param name="e">event</param>
        /// <param name="sender">GamePlayer object that has entered the game</param>
        /// <param name="args"></param>
        public virtual void OnGameEntered(DOLEvent e, object sender, EventArgs args)
        {
            SetImmunity((GamePlayer)sender, (ServerProperties.Properties.TIMER_REGION_CHANGED / 3)*1000); //Timer when a player enters game.
        }

        /// <summary>
        /// Called when player has changed the region
        /// </summary>
        /// <param name="e">event</param>
        /// <param name="sender">GamePlayer object that has changed the region</param>
        /// <param name="args"></param>
        public virtual void OnRegionChanged(DOLEvent e, object sender, EventArgs args)
        {
            SetImmunity((GamePlayer)sender, ServerProperties.Properties.TIMER_REGION_CHANGED*1000);//When a player changes Regions
        }

        /// <summary>
        /// Called after player has released
        /// </summary>
        /// <param name="e">event</param>
        /// <param name="sender">GamePlayer that has released</param>
        /// <param name="args"></param>
        public virtual void OnReleased(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = (GamePlayer)sender;
            if (player.TempProperties.getProperty<object>(KILLED_BY_PLAYER_PROP, null) != null)
            {
                player.TempProperties.removeProperty(KILLED_BY_PLAYER_PROP);
                SetImmunity(player, ServerProperties.Properties.TIMER_KILLED_BY_PLAYER*1000);//When Killed by a Player
            }
            else
            {
                SetImmunity(player, ServerProperties.Properties.TIMER_KILLED_BY_MOB*1000);//When Killed by a Mob
            }
        }

		/// <summary>
		/// Sets PvP immunity for a player and starts the timer if needed
		/// </summary>
		/// <param name="player">player that gets immunity</param>
		/// <param name="duration">amount of milliseconds when immunity ends</param>
		public virtual void SetImmunity(GamePlayer player, int duration)
		{
			// left for compatibility
			player.StartInvulnerabilityTimer(duration, m_invExpiredCallback);
		}

		/// <summary>
		/// Holds the delegate called when PvP invulnerability is expired
		/// </summary>
		protected GamePlayer.InvulnerabilityExpiredCallback m_invExpiredCallback;

		/// <summary>
		/// Removes PvP immunity from the players
		/// </summary>
		/// <player></player>
		public virtual void ImmunityOverCallback(GamePlayer player)
		{
			if (player.ObjectState != GameObject.eObjectState.Active) return;
			if (player.Client.IsPlaying == false) return;

			if (player.Level < m_safetyLevel && player.SafetyFlag)
				player.Out.SendMessage("Your temporary pvp invulnerability timer has expired, but your /safety flag is still on.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			else
				player.Out.SendMessage("Your temporary pvp invulnerability timer has expired.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

			return;
		}

		/// <summary>
		/// Invoked on Player death and deals out
		/// experience/realm points if needed
		/// </summary>
		/// <param name="killedPlayer">player that died</param>
		/// <param name="killer">killer</param>
		public override void OnPlayerKilled(GamePlayer killedPlayer, GameObject killer)
		{
			base.OnPlayerKilled(killedPlayer, killer);
			if (killer == null || killer is GamePlayer)
				killedPlayer.TempProperties.setProperty(KILLED_BY_PLAYER_PROP, KILLED_BY_PLAYER_PROP);
			else
				killedPlayer.TempProperties.removeProperty(KILLED_BY_PLAYER_PROP);
		}

		#endregion

		/// <summary>
		/// Regions where players can't be attacked
		/// </summary>
		protected int[] m_safeRegions =
		{
			10,  //City of Camelot
			101, //Jordheim
			201, //Tir Na Nog

			2,   //Albion Housing
			102, //Midgard Housing
			202, //Hibernia Housing

			//No PVP Dungeons: http://support.darkageofcamelot.com/cgi-bin/support.cfg/php/enduser/std_adp.php?p_sid=frxnPUjg&p_lva=&p_refno=020709-000000&p_created=1026248996&p_sp=cF9ncmlkc29ydD0mcF9yb3dfY250PTE0JnBfc2VhcmNoX3RleHQ9JnBfc2VhcmNoX3R5cGU9MyZwX2NhdF9sdmwxPTI2JnBfY2F0X2x2bDI9fmFueX4mcF9zb3J0X2J5PWRmbHQmcF9wYWdlPTE*&p_li
			21,  //Tomb of Mithra
			129, //Nisse�s Lair (Nisee's Lair in regions.ini)
			221, //Muire Tomb (Undead in regions.ini)

		};

		/// <summary>
		/// Regions unsafe for players with safety flag
		/// </summary>
		protected int[] m_unsafeRegions =
		{
			163, // new frontiers
		};

		public override bool IsAllowedToAttack(GameLiving attacker, GameLiving defender, bool quiet)
		{
			if (!base.IsAllowedToAttack(attacker, defender, quiet))
				return false;

			// if controlled NPC - do checks for owner instead
			if (attacker is GameNPC)
			{
				IControlledBrain controlled = ((GameNPC)attacker).Brain as IControlledBrain;
				if (controlled != null)
				{
                    attacker = controlled.GetLivingOwner();
					quiet = true; // silence all attacks by controlled npc
				}
			}
			if (defender is GameNPC)
			{
				IControlledBrain controlled = ((GameNPC)defender).Brain as IControlledBrain;
				if (controlled != null)
                    defender = controlled.GetLivingOwner();
			}

			// can't attack self
			if (attacker == defender)
			{
				if (quiet == false)
                    (attacker as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((attacker as GamePlayer).Client.Account.Language, "SpellHandler.YouCantAttackYourself"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                //MessageToLiving(attacker, "You can't attack yourself!");
                return false;
			}

			//ogre: sometimes other players shouldn't be attackable
			GamePlayer playerAttacker = attacker as GamePlayer;
			GamePlayer playerDefender = defender as GamePlayer;
			if (playerAttacker != null && playerDefender != null)
			{
				//check group
				if (playerAttacker.Group != null && playerAttacker.Group.IsInTheGroup(playerDefender))
				{
					if (!quiet) MessageToLiving(playerAttacker, "You can't attack your group members.");
					return false;
				}

				if (playerAttacker.DuelTarget != defender)
				{
					//check guild
					if (playerAttacker.Guild != null && playerAttacker.Guild == playerDefender.Guild)
					{
						if (!quiet) MessageToLiving(playerAttacker, "You can't attack your guild members.");
						return false;
					}

				    // Player can't hit other members of the same BattleGroup
				    BattleGroup mybattlegroup = (BattleGroup)playerAttacker.TempProperties.getProperty<object>(BattleGroup.BATTLEGROUP_PROPERTY, null);

				    if (mybattlegroup != null && mybattlegroup.IsInTheBattleGroup(playerDefender))
				    {
				       if (!quiet) MessageToLiving(playerAttacker, "You can't attack a member of your battlegroup.");
				       return false;
				    }

					// Safe regions
					if (m_safeRegions != null)
					{
						foreach (int reg in m_safeRegions)
							if (playerAttacker.CurrentRegionID == reg)
							{
								if (quiet == false) MessageToLiving(playerAttacker, "You're currently in a safe zone, you can't attack other players here.");
								return false;
							}
					}


					// Players with safety flag can not attack other players
					if (playerAttacker.Level < m_safetyLevel && playerAttacker.SafetyFlag)
					{
						if (quiet == false) MessageToLiving(attacker, "Your PvP safety flag is ON.");
						return false;
					}

					// Players with safety flag can not be attacked in safe regions
					if (playerDefender.Level < m_safetyLevel && playerDefender.SafetyFlag)
					{
						bool unsafeRegion = false;
						foreach (int regionID in m_unsafeRegions)
						{
							if (regionID == playerDefender.CurrentRegionID)
							{
								unsafeRegion = true;
								break;
							}
						}
						if (unsafeRegion == false)
						{
							//"PLAYER has his safety flag on and is in a safe area, you can't attack him here."
							if (quiet == false) MessageToLiving(attacker, playerDefender.Name + " has " + playerDefender.GetPronoun(1, false) + " safety flag on and is in a safe area, you can't attack " + playerDefender.GetPronoun(2, false) + " here.");
							return false;
						}
					}
				}
			}

			if (attacker.Realm == 0 && defender.Realm == 0)
			{
				return FactionMgr.CanLivingAttack(attacker, defender);
			}

			//allow confused mobs to attack same realm
			if (attacker is GameNPC && (attacker as GameNPC).IsConfused && attacker.Realm == defender.Realm)
				return true;

			// "friendly" NPCs can't attack "friendly" players
			if (defender is GameNPC && defender.Realm != 0 && attacker.Realm != 0 && defender is GameKeepGuard == false && defender is GameFont == false)
			{
				if (quiet == false) MessageToLiving(attacker, "You can't attack a friendly NPC!");
				return false;
			}
			// "friendly" NPCs can't be attacked by "friendly" players
			if (attacker is GameNPC && attacker.Realm != 0 && defender.Realm != 0 && attacker is GameKeepGuard == false)
			{
				return false;
			}

			#region Keep Guards
			//guard vs guard / npc
			if (attacker is GameKeepGuard)
			{
				if (defender is GameKeepGuard)
					return false;

				if (defender is GameNPC && (defender as GameNPC).Brain is IControlledBrain == false)
					return false;
			}

			//player vs guard
			if (defender is GameKeepGuard && attacker is GamePlayer
				&& KeepMgr.IsEnemy(defender as GameKeepGuard, attacker as GamePlayer) == false)
			{
				if (quiet == false) MessageToLiving(attacker, "You can't attack a friendly NPC!");
				return false;
			}

			//guard vs player
			if (attacker is GameKeepGuard && defender is GamePlayer
				&& KeepMgr.IsEnemy(attacker as GameKeepGuard, defender as GamePlayer) == false)
			{
				return false;
			}
			#endregion

			return true;
		}

		/// <summary>
		/// Is caster allowed to cast a spell
		/// </summary>
		/// <param name="caster"></param>
		/// <param name="target"></param>
		/// <param name="spell"></param>
		/// <param name="spellLine"></param>
		/// <returns>true if allowed</returns>
		public override bool IsAllowedToCastSpell(GameLiving caster, GameLiving target, Spell spell, SpellLine spellLine)
		{
			if (!base.IsAllowedToCastSpell(caster, target, spell, spellLine)) return false;

			GamePlayer casterPlayer = caster as GamePlayer;
			if (casterPlayer != null)
			{
				if (casterPlayer.IsInvulnerableToAttack)
				{
					// always allow selftargeted spells
					if (spell.Target == "Self") return true;

					// only caster can be the target, can't buff/heal other players
					// PBAE/GTAE doesn't need a target so we check spell type as well
					if (caster != target || spell.Target == "Area" || spell.Target == "Enemy" || (spell.Target == "Group" && spell.SpellType != "SpeedEnhancement"))
					{
						MessageToLiving(caster, "You can only cast spells on yourself until your PvP invulnerability timer wears off!", eChatType.CT_Important);
						return false;
					}
				}

			}
			return true;
		}

		public override bool IsSameRealm(GameObject source, GameObject target, bool quiet)
		{
			if (source == null || target == null) 
				return false;

			// if controlled NPC - do checks for owner instead
			if (source is GameNPC)
			{
				IControlledBrain controlled = ((GameNPC)source).Brain as IControlledBrain;
				if (controlled != null)
				{
					source = controlled.GetPlayerOwner();
					quiet = true; // silence all attacks by controlled npc
				}
			}
			if (target is GameNPC)
			{
				IControlledBrain controlled = ((GameNPC)target).Brain as IControlledBrain;
				if (controlled != null)
                    target = controlled.GetLivingOwner();
			}

			if (source == target)
				return true;

			// clients with priv level > 1 are considered friendly by anyone
			if (target is GamePlayer && ((GamePlayer)target).Client.Account.PrivLevel > 1) return true;
			// checking as a gm, targets are considered friendly
			if (source is GamePlayer && ((GamePlayer)source).Client.Account.PrivLevel > 1) return true;

			// mobs can heal mobs, players heal players/NPC
			if (source.Realm == 0 && target.Realm == 0) return true;

			//keep guards
			if (source is GameKeepGuard && target is GamePlayer)
			{
				if (!KeepMgr.IsEnemy(source as GameKeepGuard, target as GamePlayer))
					return true;
			}

			if (target is GameKeepGuard && source is GamePlayer)
			{
				if (!KeepMgr.IsEnemy(target as GameKeepGuard, source as GamePlayer))
					return true;
			}

			//doors need special handling
			if (target is GameKeepDoor && source is GamePlayer)
				return KeepMgr.IsEnemy(target as GameKeepDoor, source as GamePlayer);

			if (source is GameKeepDoor && target is GamePlayer)
				return KeepMgr.IsEnemy(source as GameKeepDoor, target as GamePlayer);

			//components need special handling
			if (target is GameKeepComponent && source is GamePlayer)
				return KeepMgr.IsEnemy(target as GameKeepComponent, source as GamePlayer);

			//Peace flag NPCs are same realm
			if (target is GameNPC)
				if ((((GameNPC)target).Flags & GameNPC.eFlags.PEACE) != 0)
					return true;

			if (source is GameNPC)
				if ((((GameNPC)source).Flags & GameNPC.eFlags.PEACE) != 0)
					return true;

			if (source is GamePlayer && target is GamePlayer)
				return true;

			if (source is GamePlayer && target is GameNPC && target.Realm != 0)
				return true;

			if (quiet == false) MessageToLiving(source, target.GetName(0, true) + " is not a member of your realm!");
			return false;
		}

		public override bool IsAllowedCharsInAllRealms(GameClient client)
		{
			return true;
		}

		public override bool IsAllowedToGroup(GamePlayer source, GamePlayer target, bool quiet)
		{
			return true;
		}

		public override bool IsAllowedToJoinGuild(GamePlayer source, Guild guild)
		{
			return true;
		}

		public override bool IsAllowedToTrade(GameLiving source, GameLiving target, bool quiet)
		{
			return true;
		}

		public override bool IsAllowedToUnderstand(GameLiving source, GamePlayer target)
		{
			return true;
		}

		/// <summary>
		/// Gets the server type color handling scheme
		/// 
		/// ColorHandling: this byte tells the client how to handle color for PC and NPC names (over the head) 
		/// 0: standard way, other realm PC appear red, our realm NPC appear light green 
		/// 1: standard PvP way, all PC appear red, all NPC appear with their level color 
		/// 2: Same realm livings are friendly, other realm livings are enemy; nearest friend/enemy buttons work
		/// 3: standard PvE way, all PC friendly, realm 0 NPC enemy rest NPC appear light green 
		/// 4: All NPC are enemy, all players are friendly; nearest friend button selects self, nearest enemy don't work at all
		/// </summary>
		/// <param name="client">The client asking for color handling</param>
		/// <returns>The color handling</returns>
		public override byte GetColorHandling(GameClient client)
		{
			return 1;
		}

		/// <summary>
		/// Formats player statistics.
		/// </summary>
		/// <param name="player">The player to read statistics from.</param>
		/// <returns>List of strings.</returns>
		public override IList<string> FormatPlayerStatistics(GamePlayer player)
		{
			var stat = new List<string>();

			int total = 0;
			#region Players Killed
			//only show if there is a kill [by Suncheck]
			if ((player.KillsAlbionPlayers + player.KillsMidgardPlayers + player.KillsHiberniaPlayers) > 0)
			{
				stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Kill.Title"));
				if (player.KillsAlbionPlayers > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Kill.AlbionPlayer") + ": " + player.KillsAlbionPlayers.ToString("N0"));
				if (player.KillsMidgardPlayers > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Kill.MidgardPlayer") + ": " + player.KillsMidgardPlayers.ToString("N0"));
				if (player.KillsHiberniaPlayers > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Kill.HiberniaPlayer") + ": " + player.KillsHiberniaPlayers.ToString("N0"));
				total = player.KillsMidgardPlayers + player.KillsAlbionPlayers + player.KillsHiberniaPlayers;
				stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Kill.TotalPlayers") + ": " + total.ToString("N0"));
			}
			#endregion
			stat.Add(" ");
			#region Players Deathblows
			//only show if there is a kill [by Suncheck]
			if ((player.KillsAlbionDeathBlows + player.KillsMidgardDeathBlows + player.KillsHiberniaDeathBlows) > 0)
			{
				total = 0;
				if (player.KillsAlbionDeathBlows > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Deathblows.AlbionPlayer") + ": " + player.KillsAlbionDeathBlows.ToString("N0"));
				if (player.KillsMidgardDeathBlows > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Deathblows.MidgardPlayer") + ": " + player.KillsMidgardDeathBlows.ToString("N0"));
				if (player.KillsHiberniaDeathBlows > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Deathblows.HiberniaPlayer") + ": " + player.KillsHiberniaDeathBlows.ToString("N0"));
				total = player.KillsMidgardDeathBlows + player.KillsAlbionDeathBlows + player.KillsHiberniaDeathBlows;
				stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Deathblows.TotalPlayers") + ": " + total.ToString("N0"));
			}
			#endregion
			stat.Add(" ");
			#region Players Solo Kills
			//only show if there is a kill [by Suncheck]
			if ((player.KillsAlbionSolo + player.KillsMidgardSolo + player.KillsHiberniaSolo) > 0)
			{
				total = 0;
				if (player.KillsAlbionSolo > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Solo.AlbionPlayer") + ": " + player.KillsAlbionSolo.ToString("N0"));
				if (player.KillsMidgardSolo > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Solo.MidgardPlayer") + ": " + player.KillsMidgardSolo.ToString("N0"));
				if (player.KillsHiberniaSolo > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Solo.HiberniaPlayer") + ": " + player.KillsHiberniaSolo.ToString("N0"));
				total = player.KillsMidgardSolo + player.KillsAlbionSolo + player.KillsHiberniaSolo;
				stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Solo.TotalPlayers") + ": " + total.ToString("N0"));
			}
			#endregion
			stat.Add(" ");
			#region Keeps
			//only show if there is a capture [by Suncheck]
			if ((player.CapturedKeeps + player.CapturedTowers) > 0)
			{
				stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Capture.Title"));
				//stat.Add("Relics Taken: " + player.RelicsTaken.ToString("N0"));
				//stat.Add("Albion Keeps Captured: " + player.CapturedAlbionKeeps.ToString("N0"));
				//stat.Add("Midgard Keeps Captured: " + player.CapturedMidgardKeeps.ToString("N0"));
				//stat.Add("Hibernia Keeps Captured: " + player.CapturedHiberniaKeeps.ToString("N0"));
				if (player.CapturedKeeps > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Capture.Keeps") + ": " + player.CapturedKeeps.ToString("N0"));
				//stat.Add("Keep Lords Slain: " + player.KeepLordsSlain.ToString("N0"));
				//stat.Add("Albion Towers Captured: " + player.CapturedAlbionTowers.ToString("N0"));
				//stat.Add("Midgard Towers Captured: " + player.CapturedMidgardTowers.ToString("N0"));
				//stat.Add("Hibernia Towers Captured: " + player.CapturedHiberniaTowers.ToString("N0"));
				if (player.CapturedTowers > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Capture.Towers") + ": " + player.CapturedTowers.ToString("N0"));
				//stat.Add("Tower Captains Slain: " + player.TowerCaptainsSlain.ToString("N0"));
				//stat.Add("Realm Guard Kills Albion: " + player.RealmGuardTotalKills.ToString("N0"));
				//stat.Add("Realm Guard Kills Midgard: " + player.RealmGuardTotalKills.ToString("N0"));
				//stat.Add("Realm Guard Kills Hibernia: " + player.RealmGuardTotalKills.ToString("N0"));
				//stat.Add("Total Realm Guard Kills: " + player.RealmGuardTotalKills.ToString("N0"));
			}
			#endregion
			stat.Add(" ");
			#region PvE
			//only show if there is a kill [by Suncheck]
			if ((player.KillsDragon + player.KillsEpicBoss + player.KillsLegion) > 0)
			{
				stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.PvE.Title"));
				if (player.KillsDragon > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.PvE.KillsDragon") + ": " + player.KillsDragon.ToString("N0"));
				if (player.KillsEpicBoss > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.PvE.KillsEpic") + ": " + player.KillsEpicBoss.ToString("N0"));
				if (player.KillsLegion > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.PvE.KillsLegion") + ": " + player.KillsLegion.ToString("N0"));
			}
			#endregion

			return stat;
		}

		/// <summary>
		/// Reset the keep with special server rules handling
		/// </summary>
		/// <param name="lord">The lord that was killed</param>
		/// <param name="killer">The lord's killer</param>
		public override void ResetKeep(GuardLord lord, GameObject killer)
		{
			base.ResetKeep(lord, killer);
			eRealm realm = eRealm.None;

			//pvp servers, the realm changes to the group leaders realm
			if (killer is GamePlayer)
			{
				Group group = ((killer as GamePlayer).Group);
				if (group != null)
					realm = (eRealm)group.Leader.Realm;
				else realm = (eRealm)killer.Realm;
			}
			else if (killer is GameNPC && (killer as GameNPC).Brain is IControlledBrain)
			{
				GamePlayer player = ((killer as GameNPC).Brain as IControlledBrain).GetPlayerOwner();
				Group group = null;
				if (player != null)
					group = player.Group;
				if (group != null)
					realm = (eRealm)group.Leader.Realm;
				else realm = (eRealm)killer.Realm;
			}
			lord.Component.Keep.Reset(realm);
		}
	}
}
