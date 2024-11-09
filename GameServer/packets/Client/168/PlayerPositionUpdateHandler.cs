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

//#define OUTPUT_DEBUG_INFO

using DOL.Database;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.SkillHandler;
using DOL.GS.Spells;
using DOL.Language;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DOL.GS.PacketHandler.Client.v168
{
    [PacketHandlerAttribute(PacketHandlerType.TCP, 0x01 ^ 168, "Handles player position updates")]
    public class PlayerPositionUpdateHandler : IPacketHandler
    {

        /// <summary>
        /// Ability that casts a spell
        /// </summary>
        protected SpellCastingAbilityHandler m_ability = null;

        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public const string LASTMOVEMENTTICK = "PLAYERPOSITION_LASTMOVEMENTTICK";
        public const string LASTCPSTICK = "PLAYERPOSITION_LASTCPSTICK";
        //public const string PlayerOldRange = "PLAYEROLDRANGE";
        //public const string PlayerFall = "PLAYERFALL";
        //public const string PlayerZoneChange = "PLAYERZONECHANGE";
        public const string RvRTimerCheck = "RVRTIMERCheck";

        /// <summary>
        /// The ability disable duration in milliseconds
        /// </summary>
        public const int DISABLE_DURATION = 30000;

        /// <summary>
        /// Stores the count of times the player is above speedhack tolerance!
        /// If this value reaches 10 or more, a logfile entry is written.
        /// </summary>
        //public const string SPEEDHACKCOUNTER = "SPEEDHACKCOUNTER";
        public const string SHSPEEDCOUNTER = "MYSPEEDHACKCOUNTER";

               
        /// <summary>
        /// Ability to cast a spell
        /// </summary>
        public SpellCastingAbilityHandler Ability
        {
            get { return m_ability; }
            set { m_ability = value; }
        }

        //static int lastZ=int.MinValue;
        public void HandlePacket(GameClient client, GSPacketIn packet)
        {

            // Stellen sicher, dass `client` und `client.Player` nicht null sind
            if (client == null || client.Player == null || client.Player.ObjectState != GameObject.eObjectState.Active || client.ClientState != GameClient.eClientState.Playing)
            {
                return; // Return if client or player is null
            }

            var environmentTick = GameTimer.GetTickCount();
            int packetVersion;
            if (client.Version > GameClient.eClientVersion.Version171)
            {
                packetVersion = 172;
            }
            else
            {
                packetVersion = 168;
            }

            int oldSpeed = client.Player.CurrentSpeed;

            // Überprüfen, ob das Paket nicht null ist
            if (packet == null)
            {
                // Loggen oder Handhabung von Fehlern
                return; // Return if packet is null
            }

            //read the state of the player
            packet.Skip(2); //PID
            ushort data = packet.ReadShort();
            int speed = (data & 0x1FF);

            //			if(!GameServer.ServerRules.IsAllowedDebugMode(client)
            //				&& (speed > client.Player.MaxSpeed + SPEED_TOL))


            if ((data & 0x200) != 0)
            {
                speed = -speed;
            }

            // Prüfen ob Player-Objekt gültig ist, bevor wir Speed setzen
            if (client.Player.IsMezzed || client.Player.IsStunned)
            {
               client.Player.SetCurrentSpeed(0);
            }
            else
            {
                client.Player.SetCurrentSpeed((short)speed);
            }

            // `client.Player` sollte hier auch auf null überprüft werden
            if (client.Player != null)
            {
                client.Player.IsStrafing = ((data & 0xe000) != 0);
            }

            int realZ = packet.ReadShort();

            ushort xOffsetInZone = packet.ReadShort();
            ushort yOffsetInZone = packet.ReadShort();
            ushort currentZoneID;
            if (packetVersion == 168)
            {
                currentZoneID = (ushort)packet.ReadByte();
                packet.Skip(1); //0x00 padding for zoneID
            }
            else
            {
                currentZoneID = packet.ReadShort();
            }


            //Dinberg - Instance considerations.
            //Now this gets complicated, so listen up! We have told the client a lie when it comes to the zoneID.
            //As a result, every movement update, they are sending a lie back to us. Two liars could get confusing!

            //BUT, the lie we sent has a truth to it - the geometry and layout of the zone. As a result, the zones
            //x and y offsets will still actually be relevant to our current zone. And for the clones to have been
            //created, there must have been a real zone to begin with, of id == instanceZone.SkinID.

            //So, although our client is lying to us, and thinks its in another zone, that zone happens to coincide
            //exactly with the zone we are instancing - and so all the positions still ring true.

            //Philosophically speaking, its like looking in a mirror and saying 'Am I a reflected, or reflector?'
            //What it boils down to has no bearing whatsoever on the result of anything, so long as someone sitting
            //outside of the unvierse knows not to listen to whether you say which you are, and knows the truth to the
            //answer. Then, he need only know what you are doing ;)

            Zone newZone = WorldMgr.GetZone(currentZoneID);
            if (newZone == null)
            {
                if (client.Player == null) return;
                if (!client.Player.TempProperties.getProperty("isbeingbanned", false))
                {
                    if (log.IsErrorEnabled)
                        log.Error(client.Player.Name + "'s position in unknown zone! => " + currentZoneID);
                    GamePlayer player = client.Player;
                    player.TempProperties.setProperty("isbeingbanned", true);
                    player.MoveToBind();
                }

                return; // TODO: what should we do? player lost in space
            }

            // move to bind if player fell through the floor
            if (realZ == 0)
            {
                client.Player.MoveTo(
                    (ushort)client.Player.BindRegion,
                    client.Player.BindXpos,
                    client.Player.BindYpos,
                    (ushort)client.Player.BindZpos,
                    (ushort)client.Player.BindHeading
                );
                return;
            }

            int realX = newZone.XOffset + xOffsetInZone;
            int realY = newZone.YOffset + yOffsetInZone;

            bool zoneChange = newZone != client.Player.LastPositionUpdateZone;

            if (zoneChange)
            {
                //If the region changes -> make sure we don't take any falling damage
                if (client.Player.LastPositionUpdateZone != null && newZone.ZoneRegion.ID != client.Player.LastPositionUpdateZone.ZoneRegion.ID)
                {
                    client.Player.MaxLastZ = int.MinValue;
                }

                // Update water level and diving flag for the new zone
                client.Out.SendPlayerPositionAndObjectID();
                zoneChange = true;
                //client.Player.TempProperties.setProperty(PlayerZoneChange, client.Player);
                /*
                 * "You have entered Burial Tomb."
                 * "Burial Tomb"
                 * "Current area is adjusted for one level 1 player."
                 * "Current area has a 50% instance bonus."
                 */



                // Sicherstellen, dass die Client-Ausgabe nicht null ist
                if (client.Out != null)
                {
                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "PlayerPositionUpdateHandler.Entered", newZone.Description),
                    eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage(newZone.Description, eChatType.CT_ScreenCenterSmaller, eChatLoc.CL_SystemWindow);
                }

                //Artefact Passive Spells
                if (client != null && client.IsPlaying)
                {
                    lock (client.Player.Inventory)
                    {
                        //Start passive spells at login
                        foreach (InventoryItem items in client.Player.Inventory.EquippedItems)
                        {
                            if (items != null && items is IGameInventoryItem)
                            {
                                if (items.PassiveSpell != 0)
                                {
                                    Spell passiveSpell = SkillBase.GetSpellByID(items.PassiveSpell);

                                    //SkillBase.GetSpellByID(items.PassiveSpell);
                                    if (passiveSpell != null && SpellHandler.FindEffectOnTarget(client.Player, passiveSpell.SpellType) == null)
                                    {
                                        ISpellHandler passiveHandler = ScriptMgr.CreateSpellHandler(client.Player, passiveSpell, SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects));
                                        if (passiveHandler != null)
                                        {
                                            passiveHandler.StartSpell(client.Player);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                client.Player.LastPositionUpdateZone = newZone;
            }

            long coordsPerSec = 0;
            int jumpDetectX = 0;
            int jumpDetectY = 0;
            int jumpDetect = 0;
            int distance = 0;

            var currentTime = GameTimer.GetTickCount();
           
            
                // Optimierung
                var timediff = currentTime - client.Player.LastPositionUpdateTick;

            if (timediff > 0)
            {
                distance = client.Player.LastPositionUpdatePoint.GetDistanceTo(new Point3D(realX, realY, realZ));
                coordsPerSec = (distance * 1000) / timediff; // Verwenden der berechneten Zeit
            }
            client.Player.LastPositionUpdateTick = currentTime; // Update direkt nach Berechnung




            if (distance < 100 && client.Player.LastPositionUpdatePoint.Z > 0)
            {
                jumpDetect = realZ - client.Player.LastPositionUpdatePoint.Z;
            }
           
            if (distance > 100 && distance < 3000 && oldSpeed == 0 && client.Player.IsRiding == false && client.Player.TargetObject == null && client.Player.IsJumping == false && client.Player.LastPositionUpdatePoint.X > 0)
            {
                jumpDetectX = realX - client.Player.LastPositionUpdatePoint.X;
            }
            if (distance > 100 && distance < 3000 && oldSpeed == 0 && client.Player.IsRiding == false && client.Player.TargetObject == null && client.Player.IsJumping == false && client.Player.LastPositionUpdatePoint.Y > 0)
            {
                jumpDetectY = realX - client.Player.LastPositionUpdatePoint.Y;
            }


            #region DEBUG
#if OUTPUT_DEBUG_INFO
			if (client.Player.LastPositionUpdatePoint.X != 0 && client.Player.LastPositionUpdatePoint.Y != 0)
			{
				log.Debug(client.Player.Name + ": distance = " + distance + ", speed = " + oldSpeed + ",  coords/sec=" + coordsPerSec);
			}
			if (jumpDetect > 0)
			{
				log.Debug(client.Player.Name + ": jumpdetect = " + jumpDetect);
			}
            if (jumpDetectX > 0)
            {
                log.Debug(client.Player.Name + ": jumpdetectX = " + jumpDetectX);
            }
            if (jumpDetectY > 0)
            {
                log.Debug(client.Player.Name + ": jumpdetectY = " + jumpDetectY);
            }
#endif
            #endregion DEBUG
            //teleport detection
            GameLocation oldLocation = client.Player.TempProperties.getProperty<GameLocation>("HACK_DETECTION_LAST_LOCATION");
            GameLocation currentLocation = new GameLocation("Current Loc", client.Player.CurrentRegionID, client.Player.X, client.Player.Y, client.Player.Z);
            client.Player.TempProperties.setProperty("HACK_DETECTION_LAST_LOCATION", currentLocation);



            client.Player.LastPositionUpdateTick = GameTimer.GetTickCount();
            client.Player.LastPositionUpdatePoint.X = realX;
            client.Player.LastPositionUpdatePoint.Y = realY;
            client.Player.LastPositionUpdatePoint.Z = realZ;

            int tolerance = ServerProperties.Properties.CPS_TOLERANCE;

            if (client.Player.Steed != null && client.Player.Steed.MaxSpeed > 0)
            {
                tolerance += client.Player.Steed.MaxSpeed;
            }
            else if (client.Player.MaxSpeed > 0)
            {
                tolerance += client.Player.MaxSpeed;
            }


            if (client.Player.IsJumping)
            {
                coordsPerSec = 0;
                jumpDetect = 0;
                client.Player.IsJumping = false;
            }
            if (!client.Player.IsJumping && oldSpeed > 0)
            {
                coordsPerSec = 0;
                jumpDetectX = 0;
                jumpDetectY = 0;
                client.Player.IsJumping = false;
            }


            ushort headingflag = packet.ReadShort();
            client.Player.Heading = (ushort)(headingflag & 0xFFF);
            ushort flyingflag = packet.ReadShort();
            byte flags = (byte)packet.ReadByte();

            if (client.Player.X != realX || client.Player.Y != realY)
            {
                client.Player.TempProperties.setProperty(LASTMOVEMENTTICK, client.Player.CurrentRegion.Time);
            }
            client.Player.X = realX;
            client.Player.Y = realY;
            client.Player.Z = realZ;

            if (zoneChange)
            {
                // update client zone information for waterlevel and diving
                client.Out.SendPlayerPositionAndObjectID();
            }

            // used to predict current position, should be before
            // any calculation (like fall damage)
            client.Player.MovementStartTick = GameTimer.GetTickCount();

            // Begin ---------- New Area System -----------
            if (client.Player.CurrentRegion.Time > client.Player.AreaUpdateTick) // check if update is needed
            {
                IList oldAreas = client.Player.CurrentAreas;

                // Because we may be in an instance we need to do the area check from the current region 
                // rather than relying on the zone which is in the skinned region.  - Tolakram

                IList newAreas = client.Player.CurrentRegion.GetAreasOfZone(newZone, client.Player);

                // Check for left areas
                if (oldAreas != null)
                {
                    foreach (IArea area in oldAreas)
                    {
                        if (!newAreas.Contains(area))
                        {
                            area.OnPlayerLeave(client.Player);
                        }
                    }
                }
                // Check for entered areas
                foreach (IArea area in newAreas)
                {
                    if (oldAreas == null || !oldAreas.Contains(area))
                    {
                        area.OnPlayerEnter(client.Player);
                    }
                }
                // set current areas to new one...
                client.Player.CurrentAreas = newAreas;
                client.Player.AreaUpdateTick = client.Player.CurrentRegion.Time + 2000; // update every 2 seconds
            }
            // End ---------- New Area System -----------


            client.Player.TargetInView = ((flags & 0x10) != 0);
            client.Player.GroundTargetInView = ((flags & 0x08) != 0);
            client.Player.IsTorchLighted = ((flags & 0x80) != 0);
            //7  6  5  4  3  2  1 0
            //15 14 13 12 11 10 9 8
            //                1 1

            const string SHLASTUPDATETICK = "SHPLAYERPOSITION_LASTUPDATETICK";
            const string SHLASTFLY = "SHLASTFLY_STRING";
            const string SHLASTSTATUS = "SHLASTSTATUS_STRING";
            long SHlastTick = client.Player.TempProperties.getProperty<uint>(SHLASTUPDATETICK);
            int SHlastFly = client.Player.TempProperties.getProperty<int>(SHLASTFLY);
            int SHlastStatus = client.Player.TempProperties.getProperty<int>(SHLASTSTATUS);
            int SHcount = client.Player.TempProperties.getProperty<int>(SHSPEEDCOUNTER);
            int status = (data & 0x1FF ^ data) >> 8;
            int fly = (flyingflag & 0x1FF ^ flyingflag) >> 8;

            if (client.Player.IsJumping)
            {
                SHcount = 0;
            }

            if (SHlastTick != 0 && SHlastTick != environmentTick)
            {
                if (((SHlastStatus == status || (status & 0x8) == 0)) && ((fly & 0x80) != 0x80) && (SHlastFly == fly || (SHlastFly & 0x10) == (fly & 0x10) || !((((SHlastFly & 0x10) == 0x10) && ((fly & 0x10) == 0x0) && (flyingflag & 0x7FF) > 0))))
                {
                    if ((environmentTick - SHlastTick) < 400)
                    {
                        SHcount++;

                        if (SHcount > 1 && client.Account.PrivLevel > 1)
                        {
                            //Apo: ?? no idea how to name the first parameter for language translation: 1: ??, 2: {detected} ?, 3: {count} ?
                            client.Out.SendMessage(string.Format("SH: ({0}) detected: {1}, count {2}", 500 / (environmentTick - SHlastTick), environmentTick - SHlastTick, SHcount), eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
                        }

                        if (SHcount % 5 == 0)
                        {
                            StringBuilder builder = new StringBuilder();
                            builder.Append("TEST_SH_DETECT[");
                            builder.Append(SHcount);
                            builder.Append("] (");
                            builder.Append(environmentTick - SHlastTick);
                            builder.Append("): CharName=");
                            builder.Append(client.Player.Name);
                            builder.Append(" Account=");
                            builder.Append(client.Account.Name);
                            builder.Append(" IP=");
                            builder.Append(client.TcpEndpointAddress);
                            GameServer.Instance.LogCheatAction(builder.ToString());

                            if (client.Account.PrivLevel > 1)
                            {

                                client.Out.SendMessage("SH: Logging SH cheat.", eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);

                                if (SHcount >= ServerProperties.Properties.SPEEDHACK_TOLERANCE)
                                {

                                    client.Out.SendMessage("SH: Player would have been banned!", eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                                }
                            }

                            if ((client.Account.PrivLevel == 1) && SHcount >= ServerProperties.Properties.SPEEDHACK_TOLERANCE)
                            {
                                if (ServerProperties.Properties.BAN_HACKERS)
                                {
                                    DBBannedAccount b = new DBBannedAccount
                                    {
                                        Author = "SERVER",
                                        Ip = client.TcpEndpointAddress,
                                        Account = client.Account.Name,
                                        DateBan = DateTime.Now,
                                        Type = "B",
                                        Reason = string.Format("Autoban SH:({0},{1}) on player:{2}", SHcount, environmentTick - SHlastTick, client.Player.Name)
                                    };
                                    GameServer.Database.AddObject(b);
                                    GameServer.Database.SaveObject(b);

                                    string message = String.Empty;

                                    message = "You have been auto kicked and banned for speed hacking!";
                                    for (int i = 0; i < 8; i++)
                                    {
                                        client.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_SystemWindow);
                                        client.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_ChatWindow);
                                    }

                                    client.Out.SendPlayerQuit(true);
                                    client.Player.SaveIntoDatabase();
                                    client.Player.Quit(true);
                                }
                                else
                                {
                                    string message = String.Empty;

                                    message = "You have been auto kicked for speed hacking!";
                                    for (int i = 0; i < 8; i++)
                                    {
                                        client.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_SystemWindow);
                                        client.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_ChatWindow);
                                    }

                                    client.Out.SendPlayerQuit(true);
                                    client.Player.SaveIntoDatabase();
                                    client.Player.Quit(true);
                                }
                                client.Disconnect();
                                return;
                            }
                        }
                    }
                    else
                    {
                        SHcount = 0;
                    }

                    SHlastTick = environmentTick;
                }
            }
            else
            {
                SHlastTick = environmentTick;
            }

            int state = ((data >> 10) & 7);
            client.Player.IsClimbing = (state == 7);
            client.Player.IsSwimming = (state == 1);
            if (state == 3 && client.Player.TempProperties.getProperty<bool>(GamePlayer.DEBUG_MODE_PROPERTY, false) == false && client.Player.IsAllowedToFly == false) //debugFly on, but player not do /debug on (hack)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("HACK_FLY");
                builder.Append(": CharName=");
                builder.Append(client.Player.Name);
                builder.Append(" Account=");
                builder.Append(client.Account.Name);
                builder.Append(" IP=");
                builder.Append(client.TcpEndpointAddress);
                GameServer.Instance.LogCheatAction(builder.ToString());
                {
                    if (ServerProperties.Properties.BAN_HACKERS)
                    {
                        DBBannedAccount b = new DBBannedAccount
                        {
                            Author = "SERVER",
                            Ip = client.TcpEndpointAddress,
                            Account = client.Account.Name,
                            DateBan = DateTime.Now,
                            Type = "B",
                            Reason = string.Format("Autoban flying hack: on player:{0}", client.Player.Name)
                        };
                        GameServer.Database.AddObject(b);
                        GameServer.Database.SaveObject(b);
                    }
                    string message = "";

                    message = "Client Hack Detected!";
                    for (int i = 0; i < 6; i++)
                    {
                        client.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        client.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_ChatWindow);
                    }
                    client.Out.SendPlayerQuit(true);
                    client.Disconnect();
                    return;
                }
            }

            SHlastFly = fly;
            SHlastStatus = status;
            client.Player.TempProperties.setProperty(SHLASTUPDATETICK, SHlastTick);
            client.Player.TempProperties.setProperty(SHLASTFLY, SHlastFly);
            client.Player.TempProperties.setProperty(SHLASTSTATUS, SHlastStatus);
            client.Player.TempProperties.setProperty(SHSPEEDCOUNTER, SHcount);
            lock (client.Player.LastUniqueLocations)
            {
                GameLocation[] locations = client.Player.LastUniqueLocations;
                GameLocation loc = locations[0];
                if (loc.X != realX || loc.Y != realY || loc.Z != realZ || loc.RegionID != client.Player.CurrentRegionID)
                {
                    loc = locations[locations.Length - 1];
                    Array.Copy(locations, 0, locations, 1, locations.Length - 1);
                    locations[0] = loc;
                    loc.X = realX;
                    loc.Y = realY;
                    loc.Z = realZ;
                    loc.Heading = client.Player.Heading;
                    loc.RegionID = client.Player.CurrentRegionID;
                }
            }

            //**************//
            //FALLING DAMAGE//
            //**************//
            int fallSpeed = 0;
            double fallDamage = 0;

            if (GameServer.ServerRules.CanTakeFallDamage(client.Player) && client.Player.IsSwimming == false)
            {
                int maxLastZ = client.Player.MaxLastZ;
                /* Are we on the ground? */
                if ((flyingflag >> 15) != 0)
                {
                    //client.Player.TempProperties.setProperty(PlayerFall, client.Player);//kein hacking wegen Fall
                    //log.Error("Player fällt!");


                    int safeFallLevel = client.Player.GetAbilityLevel(Abilities.SafeFall);
                    fallSpeed = (flyingflag & 0xFFF) - 100 * safeFallLevel; // 0x7FF fall speed and 0x800 bit = fall speed overcaped
                    int fallMinSpeed = 400;
                    int fallDivide = 6;
                    if (client.Version >= GameClient.eClientVersion.Version188)
                    {
                        fallMinSpeed = 500;
                        fallDivide = 15;
                    }
                    //int fallPercent = Math.Min(99, (fallSpeed - (fallMinSpeed + 1)) / fallDivide);
                    int fallPercent = Math.Min(99, (fallSpeed - (fallMinSpeed + 1)) / fallDivide);

                    if (fallSpeed > fallMinSpeed)

                    {
                        client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "PlayerPositionUpdateHandler.FallingDamage"),
                         eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
                        fallDamage = client.Player.CalcFallDamage(fallPercent);
                    }


                    client.Player.MaxLastZ = client.Player.Z;
                    //client.Player.TempProperties.setProperty(PlayerFall, null);//kein hacking wegen Fall
                }
                else

                {
                    // always set Z if on the ground
                    if (flyingflag == 0)
                    {
                        client.Player.MaxLastZ = client.Player.Z;
                        //client.Player.TempProperties.setProperty(PlayerFall, null);//kein hacking wegen Fall
                    }
                    // set Z if in air and higher than old Z
                    else if (maxLastZ < client.Player.Z)
                        client.Player.MaxLastZ = client.Player.Z;
                    //client.Player.TempProperties.setProperty(PlayerFall, null);//kein hacking wegen Fall
                }
            }
            //**************//

            byte[] con168 = packet.ToArray();
            //Riding is set here!
            if (client.Player.Steed != null && client.Player.Steed.ObjectState == GameObject.eObjectState.Active)
            {
                client.Player.Heading = client.Player.Steed.Heading;

                con168[2] = 0x18; // Set ride flag 00011000
                con168[3] = 0; // player speed = 0 while ride
                con168[12] = (byte)(client.Player.Steed.ObjectID >> 8); //heading = steed ID
                con168[13] = (byte)(client.Player.Steed.ObjectID & 0xFF);
                con168[14] = (byte)0;
                con168[15] = (byte)(client.Player.Steed.RiderSlot(client.Player)); // there rider slot this player
            }
            else if (!client.Player.IsAlive)
            {
                con168[2] &= 0xE3; //11100011
                con168[2] |= 0x14; //Set dead flag 00010100
            }

            if (zoneChange)
                // Update water level and diving flag for the new zone
                client.Out.SendPlayerPositionAndObjectID();

            var outpak = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerPosition));
            outpak.WriteShort((ushort)client.SessionID);
            if (client.Player.Steed != null && client.Player.Steed.ObjectState == GameObject.eObjectState.Active)
                outpak.WriteShort(0x1800);
            else
            {
                var rSpeed = client.Player.IsIncapacitated ? 0 : client.Player.CurrentSpeed;
                ushort content;
                if (rSpeed < 0)
                    content = (ushort)((rSpeed < -511 ? 511 : -rSpeed) + 0x200);
                else
                    content = (ushort)(rSpeed > 511 ? 511 : rSpeed);
                if (!client.Player.IsAlive)
                    content |= 5 << 10;


                else
                {

                    ushort pState = 0;
                    if (client.Player.IsSwimming)
                        pState = 1;
                    if (client.Player.IsClimbing)
                        pState = 7;
                    if (client.Player.IsSitting)
                        pState = 4;
                    if (client.Player.IsStrafing)
                        pState |= 8;
                    content |= (ushort)(pState << 10);
                }
                outpak.WriteShort(content);
            }
            outpak.WriteShort((ushort)client.Player.Z);
            outpak.WriteShort((ushort)(client.Player.X - client.Player.CurrentZone.XOffset));
            outpak.WriteShort((ushort)(client.Player.Y - client.Player.CurrentZone.YOffset));
            // Write Zone
            outpak.WriteShort(client.Player.CurrentZone.ZoneSkinID);

            // Copy Heading && Falling or Write Steed
            if (client.Player.Steed != null && client.Player.Steed.ObjectState == GameObject.eObjectState.Active)
            {
                outpak.WriteShort((ushort)client.Player.Steed.ObjectID);
                outpak.WriteShort((ushort)client.Player.Steed.RiderSlot(client.Player));
            }
            else
            {
                // Set Player always on ground, this is an "anti lag" packet
                ushort contenthead = (ushort)(client.Player.Heading + (true ? 0x1000 : 0));
                outpak.WriteShort(contenthead);
                outpak.WriteShort(0); // No Fall Speed.
            }

            //diving is set here
            con168[16] &= 0xFB; //11 11 10 11
            if ((con168[16] & 0x02) != 0x00)
            {
                client.Player.IsDiving = true;
                con168[16] |= 0x04;
            }
            else
                client.Player.IsDiving = false;

            con168[16] &= 0xFC; //11 11 11 00 cleared Wireframe & Stealth bits
            if (client.Player.IsWireframe)
            {
                con168[16] |= 0x01;
            }
            //stealth is set here
            if (client.Player.IsStealthed)
            {
                con168[16] |= 0x02;
            }

            con168[17] = (byte)((con168[17] & 0x80) | client.Player.HealthPercent);
            // zone ID has changed in 1.72, fix bytes 11 and 12
            byte[] con172 = (byte[])con168.Clone();
            if (packetVersion == 168)
            {
                // client sent v168 pos update packet, fix 172 version
                con172[10] = 0;
                con172[11] = con168[10];
            }
            else
            {
                // client sent v172 pos update packet, fix 168 version
                con168[10] = con172[11];
                con168[11] = 0;
            }

            GSUDPPacketOut outpak168 = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerPosition));
            //Now copy the whole content of the packet
            outpak168.Write(con168, 0, 18/*con168.Length*/);
            outpak168.WritePacketLength();

            GSUDPPacketOut outpak172 = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerPosition));
            //Now copy the whole content of the packet
            outpak172.Write(con172, 0, 18/*con172.Length*/);
            outpak172.WritePacketLength();

            //			byte[] pak168 = outpak168.GetBuffer();
            //			byte[] pak172 = outpak172.GetBuffer();
            //			outpak168 = null;
            //			outpak172 = null;
            GSUDPPacketOut outpak190 = null;

            GSUDPPacketOut outpak1112 = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerPosition));
            outpak1112.Write(con172, 0, 18/*con172.Length*/);
            outpak1112.WriteByte(client.Player.ManaPercent);
            outpak1112.WriteByte(client.Player.EndurancePercent);
            outpak1112.WriteByte((byte)(client.Player.RPFlag ? 1 : 0));
            outpak1112.WriteByte(0); //outpak1112.WriteByte((con168.Length == 22) ? con168[21] : (byte)0);
            outpak1112.WritePacketLength();

            foreach (GamePlayer player in client.Player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                if (player == null)
                    continue;
                //No position updates for ourselves
                if (player == client.Player)
                {
                    // Update Player Cache (Client sending Packet is admitting he's already having it)
                    player.Client.GameObjectUpdateArray[new Tuple<ushort, ushort>(client.Player.CurrentRegionID, (ushort)client.Player.ObjectID)] = GameTimer.GetTickCount();
                    continue;
                }
                //no position updates in different houses
                if ((client.Player.InHouse || player.InHouse) && player.CurrentHouse != client.Player.CurrentHouse)
                    continue;

                if (client.Player.MinotaurRelic != null)
                {
                    MinotaurRelic relic = client.Player.MinotaurRelic;
                    if (!relic.Playerlist.Contains(player) && player != client.Player)
                    {
                        relic.Playerlist.Add(player);
                        player.Out.SendMinotaurRelicWindow(client.Player, client.Player.MinotaurRelic.Effect, true);
                    }
                }

                if (!client.Player.IsStealthed || player.CanDetect(client.Player))
                {
                    // Update Player Cache
                    player.Client.GameObjectUpdateArray[new Tuple<ushort, ushort>(client.Player.CurrentRegionID, (ushort)client.Player.ObjectID)] = GameTimer.GetTickCount();

                    //forward the position packet like normal!
                    if (player.Client.Version >= GameClient.eClientVersion.Version1112)
                    {
                        player.Out.SendUDPRaw(outpak1112);
                    }
                    else if (player.Client.Version >= GameClient.eClientVersion.Version190)
                    {
                        if (outpak190 == null)
                        {
                            outpak190 = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerPosition));
                            outpak190.Write(con172, 0, 18/*con172.Length*/);
                            outpak190.WriteByte(client.Player.ManaPercent);
                            outpak190.WriteByte(client.Player.EndurancePercent);
                            outpak190.FillString(client.Player.CharacterClass.Name, 32);
                            // roleplay flag, if == 1, show name (RP) with gray color
                            if (client.Player.RPFlag)
                                outpak190.WriteByte(1);
                            else outpak190.WriteByte(0);
                            outpak190.WriteByte((con168.Length == 54) ? con168[53] : (byte)0); // send last byte for 190+ packets
                            outpak190.WritePacketLength();
                        }
                        player.Out.SendUDPRaw(outpak190);
                    }
                    else if (player.Client.Version >= GameClient.eClientVersion.Version172)
                        player.Out.SendUDPRaw(outpak172);
                    else
                        player.Out.SendUDPRaw(outpak168);
                }
                else
                    player.Out.SendObjectDelete(client.Player); //remove the stealthed player from view
            }

            if (client.Player.IsCharcterClass(eCharacterClass.Warlock))
            {
                //Send Chamber effect
                client.Player.Out.SendWarlockChamberEffect(client.Player);
            }


            
            /* Disabe skill icon
        long quickcastChangeTick2 = client.Player.TempProperties.getProperty<long>(GamePlayer.QUICK_CAST_CHANGE_TICK);
            long changeTime2 = client.Player.CurrentRegion.Time - quickcastChangeTick2;
            if (changeTime2 < DISABLE_DURATION)
            {
                if (m_ability != null)
                {
                    client.Player.DisableSkill(m_ability.Ability, DISABLE_DURATION);


                    // disable spells with recasttimer (Disables group of same type with same delay)
                  
                        if (client.Player is GamePlayer)
                        {
                            GamePlayer gp_caster = client.Player as GamePlayer;
                            foreach (SpellLine spellline in gp_caster.GetSpellLines())
                                foreach (Spell sp in SkillBase.GetSpellList(spellline.KeyName))
                                      client.Player.DisableSkill(m_ability.Ability, DISABLE_DURATION);

                      
                    }



                            //Update Disabled icons
                            (client.Player as GamePlayer).UpdateDisabledSkills();
                  
                }
            }

            */

                //Remove Quickcast after 5 sec
                long quickcastChangeTick = client.Player.TempProperties.getProperty<long>(GamePlayer.Quick_Cast_Remove_TICK);
            long changeTime = client.Player.CurrentRegion.Time - quickcastChangeTick;


            if (changeTime > 5000)
            {

                if (client.Player.EffectList.GetOfType<QuickCastEffect>() != null)
                {
                    QuickCastEffect quickcast = client.Player.EffectList.GetOfType<QuickCastEffect>();
                    if (quickcast != null)
                    {
                        (client.Player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((client.Player as GamePlayer).Client.Account.Language, "SpellHandler.ThePulsingSpell.Ends", quickcast.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        if (m_ability != null)
                        {
                            client.Player.DisableSkill(m_ability.Ability, 3500);
                        }

                        //client.Player.DisableSkill(SkillBase.GetAbility(Abilities.Quickcast), 3500);
                        quickcast.Cancel(false);

                    }
                }
            }

            //handle closing of windows
            //trade window
            if (client.Player.TradeWindow != null)
            {
                if (client.Player.TradeWindow.Partner != null)
                {
                    if (!client.Player.IsWithinRadius(client.Player.TradeWindow.Partner, WorldMgr.GIVE_ITEM_DISTANCE))
                        client.Player.TradeWindow.CloseTrade();
                }
            }


            
            #region Antihack
            /*
            GameSpellEffect MoveSpeed = null;
            MoveSpeed = SpellHandler.FindEffectOnTarget(client.Player, "SpeedOfTheRealm");
            MoveSpeed = SpellHandler.FindEffectOnTarget(client.Player, "ArcherSpeedEnhancement");
            MoveSpeed = SpellHandler.FindEffectOnTarget(client.Player, "SpeedEnhancement");
            SpeedOfSoundEffect speedofsoundeffect = client.Player.EffectList.GetOfType<SpeedOfSoundEffect>();
            ChargeEffect chargeeffect = client.Player.EffectList.GetOfType<ChargeEffect>();


            int newRange = 0;
            int oldRange = 0;


            int entfernung = GetDistanceBetweenPoints(client.Player.LastPositionUpdatePoint.X, client.Player.LastPositionUpdatePoint.Y); //ermittle entfernung


            //Porter wurde nicht benutzt
            if (client.Player.TempProperties.getProperty<GamePlayer>(PlayerZoneChange) == null && client.Player.MaxLastZ == client.Player.Z && client.Player.TempProperties.getProperty<GamePlayer>(PlayerFall) == null && client.Player.TempProperties.getProperty<GamePlayer>(GamePlayer.PlayerPort) == null && client.Player.ObjectState == GameObject.eObjectState.Active && client.Player.IsRiding == false && client.Player.IsOnHorse == false &&
                client.Player.CurrentRegion.IsHousing == false && chargeeffect == null && speedofsoundeffect == null && (flyingflag >> 15) == 0)
            {

                newRange = entfernung;
                // log.ErrorFormat("Player jump entfernung ohne port {0}", entfernung);
            }
            else
            {

                //Es wurde geportet dann propertie löschen
                client.Player.TempProperties.removeProperty(GamePlayer.PlayerPort);
                //log.ErrorFormat("player {0} ist geportet ok!", client.Player.Name);
                client.Player.TempProperties.setProperty(PlayerOldRange, 0);
            }



            if (client.Player.CurrentSpeed < 192 && client.Player.TempProperties.getProperty<GamePlayer>(PlayerZoneChange) == null && client.Player.TempProperties.getProperty<GamePlayer>(PlayerFall) == null && client.Account.PrivLevel == 1 && client.Player.IsAlive
                && client.Player.ObjectState == GameObject.eObjectState.Active && client.Player.TempProperties.getProperty<int>(PlayerOldRange) != 0 && client.Player.TempProperties.getProperty<int>(PlayerOldRange) != newRange)
            {

                if (client.Player.TempProperties.getProperty<GamePlayer>(GamePlayer.PlayerPort) == null)
                {
                    //nur alte rangebenutzen  wenn sie nicht 0 ist
                    oldRange = client.Player.TempProperties.getProperty<int>(PlayerOldRange);
                    //log.ErrorFormat("oldRange {0} newRange {1} Player {2}", oldRange, newRange, client.Player.Name);
                    bool BanHacker = false;

                    if (newRange > oldRange)
                    {
                        if (newRange - 1000 > oldRange)
                        {
                            BanHacker = true;
                        }
                    }
                    if (newRange < oldRange)
                    {
                        if (newRange + 1000 < oldRange)
                        {
                            BanHacker = true;
                        }
                    }

                    if (BanHacker == true)
                    {
                        string message = String.Empty;


                        // message = "WARNING! DO NOT USE ANY FORM OF HACK ON THIS SERVER!!!!";
                        //message = "Your hacking has been logged! After next time you use a Hack, you will be banned from the server!";

                        if (ServerProperties.Properties.BAN_HACKERS_For_Porthack == false)
                        {
                            {
                                StringBuilder builder = new StringBuilder();
                                builder.Append("PORT_HACK_DETECT");
                                builder.Append(" DATE TIME: (");
                                builder.Append(DateTime.Now);
                                builder.Append(") ");
                                builder.Append(" : CharName=");
                                builder.Append(client.Player.Name);
                                builder.Append(" Account=");
                                builder.Append(client.Account.Name);
                                builder.Append(" IP=");
                                builder.Append(client.TcpEndpointAddress);
                                GameServer.Instance.LogCheatAction(builder.ToString());
                            }

                            //client.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_SystemWindow);
                            //client.Out.SendPlayerQuit(true);
                            //client.Player.SaveIntoDatabase();
                            // client.Player.Quit(true);
                        }
                        else
                        {

                            //string accountName = client.Account.Name;


                            // var CharcterNames = DOLDB<Account>.SelectObjects(DB.Column("Name").IsEqualTo(accountName));
                            // IList<Account> CharcterNames = GameServer.Database.SelectObjects<Account>("`Name` = @Name", new QueryParameter("@Name", accountName));
                            // var CharcterNames = DOLDB<Account>.SelectObjects(DB.Column("Name").IsEqualTo(accountName));
                            DateTime date;
                            date = DateTime.Now;
                            date = new DateTime(date.Year, date.Month, date.Day);
                            {


                                DBBannedAccount b = new DBBannedAccount
                                {
                                    Author = "SERVER",
                                    Ip = client.TcpEndpointAddress,
                                    Account = client.Account.Name,
                                    DateBan = DateTime.Now,
                                    Type = "C",
                                    Reason = string.Format("Port HACK: on player:{0}", client.Player.Name)
                                };
                                GameServer.Database.AddObject(b);
                                GameServer.Database.SaveObject(b);



                                message = "You have been auto kicked and banned after Sytem has Porthack detect!";
                                for (int i = 0; i < 8; i++)
                                {
                                    client.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_SystemWindow);
                                    client.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_ChatWindow);

                                }

                                client.Out.SendPlayerQuit(true);
                                client.Player.SaveIntoDatabase();
                                client.Player.Quit(true);

                            }
                        }
                    }
                    BanHacker = false;
                }
            }
            //setze zuerst die ranage
            client.Player.TempProperties.setProperty(PlayerOldRange, entfernung);//setze alte entfernung
            client.Player.TempProperties.removeProperty(GamePlayer.PlayerPort);//Lösche nach port
            client.Player.TempProperties.removeProperty(PlayerZoneChange);
            */
            #endregion
           

            //handle closing of windows
            //trade window
            if (client.Player.TradeWindow?.Partner != null && !client.Player.IsWithinRadius(client.Player.TradeWindow.Partner, WorldMgr.GIVE_ITEM_DISTANCE))
                client.Player.TradeWindow.CloseTrade();
        }
    }
}
