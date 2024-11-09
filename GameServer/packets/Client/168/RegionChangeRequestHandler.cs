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
using DOL.GS.Effects;
using DOL.GS.Keeps;
using DOL.GS.Quests;
using DOL.GS.ServerRules;
using DOL.GS.Spells;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;



namespace DOL.GS.PacketHandler.Client.v168
{
    [PacketHandler(PacketHandlerType.TCP, eClientPackets.PlayerRegionChangeRequest, eClientStatus.PlayerInGame)]
    public class PlayerRegionChangeRequestHandler : IPacketHandler
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Holds jump point types
        /// </summary>
        protected readonly Hashtable m_customJumpPointHandlers = new Hashtable();

        #region IPacketHandler Members


        public void HandlePacket(GameClient client, GSPacketIn packet)
        {
            ushort jumpSpotID = packet.ReadShort();

            eRealm targetRealm = client.Player.Realm;

            if (client.Player.CurrentRegion.Expansion == (int)eClientExpansion.TrialsOfAtlantis)
            {
                // if we are in TrialsOfAtlantis then base the target jump on the current region realm instead of the players realm
                targetRealm = client.Player.Realm; //targetRealm = client.Player.CurrentZone.GetRealm();
            }

            var zonePoint = GameServer.Database.SelectObjects<ZonePoint>("`Id` = @Id AND (`Realm` = @Realm OR `Realm` = @DefaultRealm OR `Realm` IS NULL)",
                                                                       new[] { new QueryParameter("@Id", jumpSpotID), new QueryParameter("@Realm", (byte)targetRealm), new QueryParameter("@DefaultRealm", 0) }).FirstOrDefault();


            if (zonePoint == null || zonePoint.TargetRegion == 0)
            {
                ChatUtil.SendDebugMessage(client, "Invalid Jump (ZonePoint table): [" + jumpSpotID + "]" + ((zonePoint == null) ? ". Entry missing!" : ". TargetRegion is 0!"));
                zonePoint = new ZonePoint
                {
                    Id = jumpSpotID
                };
            }

            if (client.Account.PrivLevel > 1)
            {
                client.Out.SendMessage("JumpSpotID = " + jumpSpotID, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("ZonePoint Target: Region = " + zonePoint.TargetRegion + ", ClassType = '" + zonePoint.ClassType + "'", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }


            //Dinberg: Fix - some jump points are handled code side, such as instances.
            //As such, region MAY be zero in the database, so this causes an issue.

            if (zonePoint.TargetRegion != 0)
            {
                Region reg = WorldMgr.GetRegion(zonePoint.TargetRegion);
                if (reg != null)
                {
                    // check for target region disabled if player is in a standard region
                    // otherwise the custom region should handle OnZonePoint for this check
                    if (client.Player.CurrentRegion.IsCustom == false && reg.IsDisabled)
                    {
                        if ((client.Player.Mission is TaskDungeonMission &&
                             (client.Player.Mission as TaskDungeonMission).TaskRegion.Skin == reg.Skin) == false)
                        {
                            client.Out.SendMessage("This region has been disabled!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            if (client.Account.PrivLevel == 1)
                            {
                                return;
                            }
                        }
                    }
                }
            }

            // Allow the region to either deny exit or handle the zonepoint in a custom way
            if (client.Player.CurrentRegion.OnZonePoint(client.Player, zonePoint) == false)
            {
                return;
            }

            //check caps for battleground
            Battleground bg = KeepMgr.GetBattleground(zonePoint.TargetRegion);
            if (bg != null)
            {
                if (client.Player.Level < bg.MinLevel && client.Player.Level > bg.MaxLevel &&
                    client.Player.RealmLevel >= bg.MaxRealmLevel)
                {
                    return;
                }
            }

            IJumpPointHandler customHandler = null;
            if (string.IsNullOrEmpty(zonePoint.ClassType) == false)
            {
                customHandler = (IJumpPointHandler)m_customJumpPointHandlers[zonePoint.ClassType];

                // check for db change to update cached handler
                if (customHandler != null && customHandler.GetType().FullName != zonePoint.ClassType)
                {
                    customHandler = null;
                }

                if (customHandler == null)
                {
                    //Dinberg - Instances need to use a special handler. This is because some instances will result
                    //in duplicated zonepoints, such as if Tir Na Nog were to be instanced for a quest.
                    string type = (client.Player.CurrentRegion.IsInstance)
                        ? "DOL.GS.ServerRules.InstanceDoorJumpPoint"
                        : zonePoint.ClassType;
                    Type t = ScriptMgr.GetType(type);

                    if (t == null)
                    {
                        Log.ErrorFormat("jump point {0}: class {1} not found!", zonePoint.Id, zonePoint.ClassType);
                    }
                    else if (!typeof(IJumpPointHandler).IsAssignableFrom(t))
                    {
                        Log.ErrorFormat("jump point {0}: class {1} must implement IJumpPointHandler interface!", zonePoint.Id, zonePoint.ClassType);
                    }
                    else
                    {
                        try
                        {
                            customHandler = (IJumpPointHandler)Activator.CreateInstance(t);
                        }
                        catch (Exception e)
                        {
                            customHandler = null;
                            Log.Error(
                                string.Format("jump point {0}: error creating a new instance of jump point handler {1}", zonePoint.Id,
                                              zonePoint.ClassType), e);
                        }
                    }
                }

                if (customHandler != null)
                {
                    m_customJumpPointHandlers[zonePoint.ClassType] = customHandler;
                }
            }

            new RegionChangeRequestHandler(client.Player, zonePoint, customHandler).Start(1);
        }

        #endregion

        #region Nested type: RegionChangeRequestHandler

        /// <summary>
        /// Handles player region change requests
        /// </summary>
        protected class RegionChangeRequestHandler : RegionAction
        {
            /// <summary>
            /// Checks whether player is allowed to jump
            /// </summary>
            protected readonly IJumpPointHandler m_checkHandler;

            /// <summary>
            /// The target zone point
            /// </summary>
            protected readonly ZonePoint m_zonePoint;

            /// <summary>
            /// Constructs a new RegionChangeRequestHandler
            /// </summary>
            /// <param name="actionSource">The action source</param>
            /// <param name="zonePoint">The target zone point</param>
            /// <param name="checker">The jump point checker instance</param>
            public RegionChangeRequestHandler(GamePlayer actionSource, ZonePoint zonePoint, IJumpPointHandler checkHandler)
                : base(actionSource)
            {
                m_zonePoint = zonePoint ?? throw new ArgumentNullException("zonePoint");
                m_checkHandler = checkHandler;
            }

            /// <summary>
            /// Called on every timer tick
            /// </summary>
            protected override void OnTick()
            {
                var player = (GamePlayer)m_actionSource;

                Region reg = WorldMgr.GetRegion(m_zonePoint.TargetRegion);


                if (reg != null && reg.Expansion > (int)player.Client.ClientType)
                {
                    player.Out.SendMessage("Destination region (" + reg.Description + ") is not supported by your client type.",
                                           eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }
                
                //RvR enter timer after realm switch
                IList<Account> PlayerAccountName = GameServer.Database.SelectObjects<Account>("`Name` = @Name", new QueryParameter("@Name", player.Client.Account.Name));

                foreach (Account db in PlayerAccountName)
                {

                    if (db.PrivLevel == 1 && db != null && db.LastLoginRealm != 0 && db.IsOtherRealmAllowed == false && (byte)player.Realm != db.LastLoginRealm && ServerProperties.Properties.LOGIN_OTHER_RELAM_SWITCH_TIMER_MINUTES > 0)
                    {
                        if (reg != null && db.LoginWaitTime > DateTime.Now)
                        {
                            string _realm = String.Empty;

                            if (reg.IsRvR)
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
                                
                                string message = string.Format("Destination region {0} is not available at this time - reason: You were recently in {2}." + "You can enter RvR regions again at: {1} EU time.", reg.Description.ToString(), db.LoginWaitTime.ToString(), _realm.ToString());

                                player.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                        }
                    }
                }

                if (m_checkHandler != null)
                {
                    try
                    {
                        if (!m_checkHandler.IsAllowedToJump(m_zonePoint, player))
                        {
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        if (Log.IsErrorEnabled)
                        {
                            Log.Error("Jump point handler (" + m_zonePoint.ClassType + ")", e);
                        }

                        player.Out.SendMessage("exception in jump point (" + m_zonePoint.Id + ") handler...", eChatType.CT_System,
                                               eChatLoc.CL_SystemWindow);
                        return;
                    }
                }
                switch (m_zonePoint.Id)
                {
                    case 87:
                        if (ServerProperties.Properties.ALLOW_ALL_REALMS_DF == false && GlobalConstants.RealmToName(DFEnterJumpPoint.DarknessFallOwner) != "Hibernia" && GlobalConstants.RealmToName(DFEnterJumpPoint.DarknessFallOwner) != "None")
                        {
                            player.Out.SendMessage("The portal is closed, Darkness Falls is in the hand of " + GlobalConstants.RealmToName(DFEnterJumpPoint.DarknessFallOwner) + "!", eChatType.CT_System,
                                               eChatLoc.CL_SystemWindow);
                            return;
                        }
                        break;

                    case 84:

                        if (ServerProperties.Properties.ALLOW_ALL_REALMS_DF == false && GlobalConstants.RealmToName(DFEnterJumpPoint.DarknessFallOwner) != "Midgard" && GlobalConstants.RealmToName(DFEnterJumpPoint.DarknessFallOwner) != "None")
                        {
                            player.Out.SendMessage("The portal is closed, Darkness Falls is in the hand of " + GlobalConstants.RealmToName(DFEnterJumpPoint.DarknessFallOwner) + "!", eChatType.CT_System,
                                               eChatLoc.CL_SystemWindow);
                            return;
                        }
                        break;

                    case 81:

                        if (ServerProperties.Properties.ALLOW_ALL_REALMS_DF == false && GlobalConstants.RealmToName(DFEnterJumpPoint.DarknessFallOwner) != "Albion" && GlobalConstants.RealmToName(DFEnterJumpPoint.DarknessFallOwner) != "None")
                        {
                            player.Out.SendMessage("The portal is closed, Darkness Falls is in the hand of " + GlobalConstants.RealmToName(DFEnterJumpPoint.DarknessFallOwner) + "! ", eChatType.CT_System,
                                               eChatLoc.CL_SystemWindow);
                            return;
                        }
                        break;

                    case 705:

                        if (player.Level > 10)
                        {
                            player.Out.SendMessage("The portal is closed for you, your level is to hight! ", eChatType.CT_System,
                                               eChatLoc.CL_SystemWindow);
                            return;
                        }
                        break;
                    case 707:

                        if (player.Level > 10)
                        {
                            player.Out.SendMessage("The portal is closed for you, your level is to hight! ", eChatType.CT_System,
                                               eChatLoc.CL_SystemWindow);
                            return;
                        }
                        break;

                    case 708:

                        if (player.Level > 10)
                        {
                            player.Out.SendMessage("The portal is closed for you, your level is to hight! ", eChatType.CT_System,
                                               eChatLoc.CL_SystemWindow);
                            return;
                        }
                        break;
                        
                }


                if (ServerProperties.Properties.BUFFBOTS_ONLY_RVR == false)
                {
                  player.MoveTo(m_zonePoint.TargetRegion, m_zonePoint.TargetX, m_zonePoint.TargetY, m_zonePoint.TargetZ, m_zonePoint.TargetHeading);

                }
                else
                {
                 
                    //Anti Buffbot for PVM Zones
                    GameSpellEffect effect1 = SpellHandler.FindEffectOnTarget((GamePlayer)player, "DexterityQuicknessBuff");
                    GameSpellEffect effect2 = SpellHandler.FindEffectOnTarget((GamePlayer)player, "StrengthConstitutionBuff");
                    GameSpellEffect effect3 = SpellHandler.FindEffectOnTarget((GamePlayer)player, "StrengthBuff");
                    GameSpellEffect effect4 = SpellHandler.FindEffectOnTarget((GamePlayer)player, "ConstitutionBuff");
                    GameSpellEffect effect5 = SpellHandler.FindEffectOnTarget((GamePlayer)player, "DexterityBuff");
                    GameSpellEffect effect6 = SpellHandler.FindEffectOnTarget((GamePlayer)player, "AcuityBuff");
                    GameSpellEffect effect7 = SpellHandler.FindEffectOnTarget((GamePlayer)player, "ArmorFactorBuff");
                    switch (m_zonePoint.TargetRegion)
                    {
                        case 1:
                            if (reg != null && player.CurrentRegionID == 163 && ServerProperties.Properties.BUFFBOTS_ONLY_RVR == true)
                            {
                                if (effect1 != null) effect1.Cancel(false);
                                if (effect2 != null) effect2.Cancel(false);
                                if (effect3 != null) effect3.Cancel(false);
                                if (effect4 != null) effect4.Cancel(false);
                                if (effect5 != null) effect5.Cancel(false);
                                if (effect6 != null) effect6.Cancel(false);
                                if (effect7 != null) effect7.Cancel(false);
                            }
                            if (reg != null && player.CurrentRegionID == 245 && ServerProperties.Properties.BUFFBOTS_ONLY_RVR == true)
                            {
                                if (effect1 != null) effect1.Cancel(false);
                                if (effect2 != null) effect2.Cancel(false);
                                if (effect3 != null) effect3.Cancel(false);
                                if (effect4 != null) effect4.Cancel(false);
                                if (effect5 != null) effect5.Cancel(false);
                                if (effect6 != null) effect6.Cancel(false);
                                if (effect7 != null) effect7.Cancel(false);
                            }
                            if (reg != null && player.CurrentRegionID == 241 && ServerProperties.Properties.BUFFBOTS_ONLY_RVR == true)
                            {
                                if (effect1 != null) effect1.Cancel(false);
                                if (effect2 != null) effect2.Cancel(false);
                                if (effect3 != null) effect3.Cancel(false);
                                if (effect4 != null) effect4.Cancel(false);
                                if (effect5 != null) effect5.Cancel(false);
                                if (effect6 != null) effect6.Cancel(false);
                                if (effect7 != null) effect7.Cancel(false);
                            }
                            break;

                        case 200:
                            if (reg != null && player.CurrentRegionID == 163 && ServerProperties.Properties.BUFFBOTS_ONLY_RVR == true)
                            {
                                if (effect1 != null) effect1.Cancel(false);
                                if (effect2 != null) effect2.Cancel(false);
                                if (effect3 != null) effect3.Cancel(false);
                                if (effect4 != null) effect4.Cancel(false);
                                if (effect5 != null) effect5.Cancel(false);
                                if (effect6 != null) effect6.Cancel(false);
                                if (effect7 != null) effect7.Cancel(false);
                            }
                            if (reg != null && player.CurrentRegionID == 245 && ServerProperties.Properties.BUFFBOTS_ONLY_RVR == true)
                            {
                                if (effect1 != null) effect1.Cancel(false);
                                if (effect2 != null) effect2.Cancel(false);
                                if (effect3 != null) effect3.Cancel(false);
                                if (effect4 != null) effect4.Cancel(false);
                                if (effect5 != null) effect5.Cancel(false);
                                if (effect6 != null) effect6.Cancel(false);
                                if (effect7 != null) effect7.Cancel(false);
                            }
                            if (reg != null && player.CurrentRegionID == 241 && ServerProperties.Properties.BUFFBOTS_ONLY_RVR == true)
                            {
                                if (effect1 != null) effect1.Cancel(false);
                                if (effect2 != null) effect2.Cancel(false);
                                if (effect3 != null) effect3.Cancel(false);
                                if (effect4 != null) effect4.Cancel(false);
                                if (effect5 != null) effect5.Cancel(false);
                                if (effect6 != null) effect6.Cancel(false);
                                if (effect7 != null) effect7.Cancel(false);
                            }
                            break;

                        case 100:
                            if (reg != null && player.CurrentRegionID == 163 && ServerProperties.Properties.BUFFBOTS_ONLY_RVR == true)
                            {
                                if (effect1 != null) effect1.Cancel(false);
                                if (effect2 != null) effect2.Cancel(false);
                                if (effect3 != null) effect3.Cancel(false);
                                if (effect4 != null) effect4.Cancel(false);
                                if (effect5 != null) effect5.Cancel(false);
                                if (effect6 != null) effect6.Cancel(false);
                                if (effect7 != null) effect7.Cancel(false);
                            }
                            if (reg != null && player.CurrentRegionID == 245 && ServerProperties.Properties.BUFFBOTS_ONLY_RVR == true)
                            {
                                if (effect1 != null) effect1.Cancel(false);
                                if (effect2 != null) effect2.Cancel(false);
                                if (effect3 != null) effect3.Cancel(false);
                                if (effect4 != null) effect4.Cancel(false);
                                if (effect5 != null) effect5.Cancel(false);
                                if (effect6 != null) effect6.Cancel(false);
                                if (effect7 != null) effect7.Cancel(false);
                            }
                            if (reg != null && player.CurrentRegionID == 241 && ServerProperties.Properties.BUFFBOTS_ONLY_RVR == true)
                            {
                                if (effect1 != null) effect1.Cancel(false);
                                if (effect2 != null) effect2.Cancel(false);
                                if (effect3 != null) effect3.Cancel(false);
                                if (effect4 != null) effect4.Cancel(false);
                                if (effect5 != null) effect5.Cancel(false);
                                if (effect6 != null) effect6.Cancel(false);
                                if (effect7 != null) effect7.Cancel(false);
                            }
                            break;


                    }
                    //move the player
                    player.MoveTo(m_zonePoint.TargetRegion, m_zonePoint.TargetX, m_zonePoint.TargetY, m_zonePoint.TargetZ, m_zonePoint.TargetHeading);
                }
            }

        #endregion
        }
    }
}