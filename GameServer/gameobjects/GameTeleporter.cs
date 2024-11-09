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
using DOL.GS.Housing;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DOL.GS
{
    /// <summary>
    /// Base class for all teleporter type NPCs.
    /// </summary>
    /// <author>Aredhel</author>
    public class GameTeleporter : GameNPC
    {
        public GameTeleporter()
            : base() { }


        /// <summary>
        /// The type of teleporter; this is used in order to be able to handle
        /// identical TeleportIDs differently, depending on the actual teleporter.
        /// </summary>
        protected virtual String Type
        {
            get { return ""; }
        }

        /// <summary>
        /// The destination realm. 
        /// </summary>
        protected virtual eRealm DestinationRealm
        {
            get { return Realm; }
        }

        public virtual bool GetPlayerHouse(GamePlayer player)
        {
            House house = HouseMgr.GetHouseByPlayer(player);

            if (house != null)
            {

                if (house.HouseNumber >= 1 && house.HouseNumber <= 1383 && player.Realm == eRealm.Albion)
                {
                    return true;
                }

                if (house.HouseNumber >= 1601 && house.HouseNumber <= 2574 && player.Realm == eRealm.Midgard)
                {
                    return true;
                }

                if (house.HouseNumber >= 3200 && house.HouseNumber <= 4399 && player.Realm == eRealm.Hibernia)
                {
                    return true;
                }
            }
            return false;
        }
        public virtual bool GetPlayerGuildHouse(GamePlayer player)
        {
            House house = HouseMgr.GetGuildHouseByPlayer(player);

            if (house != null)
            {

                if (house.HouseNumber >= 1 && house.HouseNumber <= 1383 && player.Realm == eRealm.Albion)
                {
                    return true;
                }

                if (house.HouseNumber >= 1601 && house.HouseNumber <= 2574 && player.Realm == eRealm.Midgard)
                {
                    return true;
                }

                if (house.HouseNumber >= 3200 && house.HouseNumber <= 4399 && player.Realm == eRealm.Hibernia)
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Turn the teleporter to face the player.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player) || GameRelic.IsPlayerCarryingRelic(player))
                return false;

            TurnTo(player, 10000);
            return true;
        }

        /// <summary>
        /// Talk to the teleporter.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public override bool WhisperReceive(GameLiving source, string text)
        {
            if (!base.WhisperReceive(source, text))
                return false;

            GamePlayer player = source as GamePlayer;
            if (player == null)
                return false;

            if (GameRelic.IsPlayerCarryingRelic(player))
                return false;

            return GetTeleportLocation(player, text);
        }

        protected virtual bool GetTeleportLocation(GamePlayer player, string text)
        {
            House BindHouse = null;
            House targetHouse = null;
            // Battlegrounds are specials, as the teleport location depends on
            // the level of the player, so let's deal with that first.
            if (text.ToLower() == "battlegrounds")
            {
                if (!ServerProperties.Properties.BG_ZONES_OPENED && player.Client.Account.PrivLevel == (uint)ePrivLevel.Player)
                {
                    SayTo(player, ServerProperties.Properties.BG_ZONES_CLOSED_MESSAGE);
                }
                else
                {
                    AbstractGameKeep portalKeep = KeepMgr.GetBGPK(player);
                    if (portalKeep != null)
                    {
                        Teleport teleport = new Teleport();
                        teleport.TeleportID = "battlegrounds";
                        teleport.Realm = (byte)portalKeep.Realm;
                        teleport.RegionID = portalKeep.Region;
                        teleport.X = portalKeep.X;
                        teleport.Y = portalKeep.Y;
                        teleport.Z = portalKeep.Z;
                        teleport.Heading = 0;
                        OnDestinationPicked(player, teleport);
                        return true;
                    }
                    else
                    {
                        if (player.Client.Account.PrivLevel > (uint)ePrivLevel.Player)
                        {
                            player.Out.SendMessage("No portal keep found.", eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
                        }
                        return true;
                    }
                }
            }

            // Another special case is personal house, as there is no location
            // that will work for every player.
            if (text.ToLower() == "personal")
            {

                House house = HouseMgr.GetHouseByPlayer(player);


                if (house == null || GetPlayerHouse(player) == false)
                {
                    text = "entrance";  // Fall through, port to housing entrance.
                }
                else if (house.Realm == player.Realm)
                {
                    IGameLocation location = house.OutdoorJumpPoint(player);
                    Teleport teleport = new Teleport();
                    teleport.TeleportID = "personal";
                    teleport.Realm = (int)DestinationRealm;
                    teleport.RegionID = location.RegionID;
                    teleport.X = location.X;
                    teleport.Y = location.Y;
                    teleport.Z = location.Z;
                    teleport.Heading = location.Heading;
                    OnDestinationPicked(player, teleport);
                    return true;
                }
                else
                    SayTo(player, "Sorry, you haven't set any house bind point yet.");
            }

            // Yet another special case the port to the 'hearth' what means
            // that the player will be ported to the defined house bindstone
            if (text.ToLower() == "hearth")
            {
                // Check if player has set a house bind
                if (!(player.BindHouseRegion > 0))
                {
                    SayTo(player, "Sorry, you haven't set any house bind point yet.");
                    return false;
                }

                //Get Player Bind Housenumber
                if (string.IsNullOrEmpty(player.BindHouseNumber.ToString()) == false)
                {

                    BindHouse = HouseMgr.GetHouse((ushort)player.BindHouseRegion, player.BindHouseNumber);
                }


                // Check if the house at the player's house bind location still exists
                ArrayList houses = (ArrayList)HouseMgr.GetHousesCloseToSpot((ushort)player.
                    BindHouseRegion, player.BindHouseXpos, player.
                    BindHouseYpos, 800);



                if (houses.Count == 0 && BindHouse == null)
                {
                    SayTo(player, "I'm afraid I can't teleport you to your hearth since the house at your " +
                        "house bind location has been torn down.");
                    return false;
                }

                /*
                // Check if the house at the player's house bind location still exists
                ArrayList houses = (ArrayList)HouseMgr.GetHousesCloseToSpot((ushort)player.
                    DBCharacter.BindHouseRegion, player.DBCharacter.BindHouseXpos, player.
                    DBCharacter.BindHouseYpos, 700);
                if (houses.Count == 0)
                {
                    SayTo(player, "I'm afraid I can't teleport you to your hearth since the house at your " +
                        "house bind location has been torn down.");
                    return false;
                }
                */
                if (BindHouse != null)
                    targetHouse = BindHouse;

                else if ((House)houses[0] != null)
                    targetHouse = (House)houses[0];

                else
                {
                    SayTo(player, "I'm afraid I can't teleport you to your hearth since the house at your " +
                        "house bind location has been torn down.");
                    return false;
                }


                // Check if the house at the player's house bind location contains a bind stone

                IDictionary<uint, DBHouseHookpointItem> hookpointItems = targetHouse.HousepointItems;
                Boolean hasBindstone = false;

                foreach (KeyValuePair<uint, DBHouseHookpointItem> targetHouseItem in hookpointItems)
                { //// Check if the house at the player's house bind location contains a bind sto
                    if (((GameObject)targetHouseItem.Value.GameObject).GetName(0, false).ToLower().EndsWith("bindstone"))
                    {
                        hasBindstone = true;
                        break;
                    }
                }

                if (!hasBindstone)
                {
                    SayTo(player, "I'm sorry to tell that the bindstone of your current house bind location " +
                        "has been removed, so I'm not able to teleport you there.");
                    return false;
                }

                // Check if the player has the permission to bind at the house bind stone
                if (!targetHouse.CanBindInHouse(player))
                {
                    SayTo(player, "You're no longer allowed to bind at the house bindstone you've previously " +
                        "chosen, hence I'm not allowed to teleport you there.");
                    return false;
                }

                Teleport teleport = new Teleport();
                teleport.TeleportID = "hearth";
                teleport.Realm = (int)DestinationRealm;
                teleport.RegionID = player.BindHouseRegion;
                teleport.X = player.BindHouseXpos;
                teleport.Y = player.BindHouseYpos;
                teleport.Z = player.BindHouseZpos;
                teleport.Heading = player.BindHouseHeading;
                OnDestinationPicked(player, teleport);
                return true;
            }

            if (text.ToLower() == "guild house")
            {
                House house = HouseMgr.GetGuildHouseByPlayer(player);

                if (house == null || GetPlayerGuildHouse(player) == false)
                {
                    player.Out.SendMessage("Sorry, there was no guild house found!", eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
                    return false;  // no teleport when guild house not found
                }
                else if (house.Realm == player.Realm)
                {
                    IGameLocation location = house.OutdoorJumpPoint(player);
                    Teleport teleport = new Teleport();
                    teleport.TeleportID = "guild house";
                    teleport.Realm = (int)DestinationRealm;
                    teleport.RegionID = location.RegionID;
                    teleport.X = location.X;
                    teleport.Y = location.Y;
                    teleport.Z = location.Z;
                    teleport.Heading = location.Heading;
                    OnDestinationPicked(player, teleport);
                    return true;
                }
                else
                    SayTo(player, "Sorry, you haven't set any guild house bind point yet.");
            }

            // Find the teleport location in the database.
            Teleport port = WorldMgr.GetTeleportLocation(DestinationRealm, String.Format("{0}:{1}", Type, text));
            if (port != null)
            {
                if (port.RegionID == 0 && port.X == 0 && port.Y == 0 && port.Z == 0)
                {
                    OnSubSelectionPicked(player, port);
                }
                else
                {
                    OnDestinationPicked(player, port);
                }
                return false;
            }

            return true;    // Needs further processing.
        }

        /// <summary>
        /// Player has picked a destination.
        /// Override if you need the teleporter to say something to the player
        /// before porting him.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="destination"></param>
        protected virtual void OnDestinationPicked(GamePlayer player, Teleport destination)
        {
            Region region = WorldMgr.GetRegion((ushort)destination.RegionID);

            if (region == null || region.IsDisabled)
            {
                player.Out.SendMessage("This destination is not available.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            //RvR enter timer after realm switch
            IList<Account> PlayerAccountName = GameServer.Database.SelectObjects<Account>("`Name` = @Name", new QueryParameter("@Name", player.Client.Account.Name));

            foreach (Account db in PlayerAccountName)
            {

                if (db.PrivLevel == 1 && db != null && db.IsOtherRealmAllowed == false && db.LastLoginRealm != 0 && (byte)player.Realm != db.LastLoginRealm && ServerProperties.Properties.LOGIN_OTHER_RELAM_SWITCH_TIMER_MINUTES > 0)
                {
                    if (region != null && db.LoginWaitTime > DateTime.Now)
                    {
                        string _realm = String.Empty;

                        if (region.IsRvR)
                        {
                            switch (db.LastLoginRealm)
                            {
                                case 1:
                                    _realm = "Albion";
                                    break;
                                case 2:
                                    _realm = "Midgard";
                                    break;
                                case 3:
                                    _realm = "Hibernia";
                                    break;
                            }
                            string message = string.Format("Destination region {0} is not available at this time - reason: You were recently in another realm." + "You can enter RvR regions again at: {1} EU time.", region.Description.ToString(), db.LoginWaitTime.ToString());

                            player.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            return;
                        }
                    }
                }
            }

            OnTeleport(player, destination);
        }

        /// <summary>
        /// Player has picked a subselection.
        /// Override to pass teleport options on to the player.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="subSelection"></param>
        protected virtual void OnSubSelectionPicked(GamePlayer player, Teleport subSelection)
        {
        }

        /// <summary>
        /// Teleport the player to the designated coordinates using the
        /// portal spell.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="destination"></param>
        protected virtual void OnTeleportSpell(GamePlayer player, Teleport destination)
        {
            SpellLine spellLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells);
            List<Spell> spellList = SkillBase.GetSpellList(GlobalSpellsLines.Mob_Spells);
            Spell spell = SkillBase.GetSpellByID(5999); // UniPortal spell.

            if (spell != null)
            {
                TargetObject = player;
                UniPortal portalHandler = new UniPortal(this, spell, spellLine, destination);
                m_runningSpellHandler = portalHandler;
                portalHandler.CastSpell();
                return;
            }

            // Spell not found in the database, fall back on default procedure.

            if (player.Client.Account.PrivLevel > 1)
                player.Out.SendMessage("Uni-Portal spell not found.",
                    eChatType.CT_Skill, eChatLoc.CL_SystemWindow);

            this.OnTeleport(player, destination);
        }

        /// <summary>
        /// Teleport the player to the designated coordinates. 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="destination"></param>
        protected virtual void OnTeleport(GamePlayer player, Teleport destination)
        {
            if (player.InCombat == false && GameRelic.IsPlayerCarryingRelic(player) == false)
            {
                player.LeaveHouse();
                GameLocation currentLocation = new GameLocation("TeleportStart", player.CurrentRegionID, player.X, player.Y, player.Z);
                player.MoveTo((ushort)destination.RegionID, destination.X, destination.Y, destination.Z, (ushort)destination.Heading);
                GameServer.ServerRules.OnPlayerTeleport(player, currentLocation, destination);
            }
        }
    }
}