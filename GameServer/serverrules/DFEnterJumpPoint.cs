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

using DOL.Database;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.Events;

namespace DOL.GS.ServerRules
{
    /// <summary>
    /// Handles DF entrance jump point allowing only one realm to enter on Normal server type.
    /// </summary>
    public class DFEnterJumpPoint : IJumpPointHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Decides whether player can jump to the target point.
        /// All messages with reasons must be sent here.
        /// Can change destination too.
        /// </summary>
        /// <param name="targetPoint">The jump destination</param>
        /// <param name="player">The jumping player</param>
        /// <returns>True if allowed</returns>
        public bool IsAllowedToJump(ZonePoint targetPoint, GamePlayer player)
        {
            if (GameServer.Instance.Configuration.ServerType != eGameServerType.GST_Normal)
            {
                return true;
            }
            if (ServerProperties.Properties.ALLOW_ALL_REALMS_DF)
            {
                return true;
            }
            if (DFOpenForAll(DarknessFallOwner))
            {
                return true;
            }

            else
            {
                return (player.Realm == DarknessFallOwner);
            }

        }

        public static eRealm DarknessFallOwner = eRealm.None;

        /// <summary>
        /// initialize the darkness fall entrance system
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        [ScriptLoadedEvent]
        public static void OnScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            CheckDFOwner();
            GameEventMgr.AddHandler(KeepEvent.KeepTaken, new DOLEventHandler(OnKeepTaken));
        }

        /// <summary>
        /// check if a realm have more keep at start
        /// to know the DF owner
        /// </summary>

        public static void CheckDFOwner()
        {

            int midgardTowerCount = 0, hiberniaTowerCount = 0, albionTowerCount = 0;

            int albcountTowerCount1 = KeepMgr.GetTowerCountByRealm(eRealm.Albion);
            int midcountTowerCount2 = KeepMgr.GetTowerCountByRealm(eRealm.Midgard);
            int hibcountTowerCount3 = KeepMgr.GetTowerCountByRealm(eRealm.Hibernia);


            for (int i = 0; i < albcountTowerCount1; i++)
            {
                ++albionTowerCount;

            }
            for (int i = 0; i < midcountTowerCount2; i++)
            {
                ++midgardTowerCount;

            }
            for (int i = 0; i < hibcountTowerCount3; i++)
            {
                ++hiberniaTowerCount;

            }


            if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvP)
            {
                DarknessFallOwner = eRealm.None;
            }
            else if (albionTowerCount > midgardTowerCount && albionTowerCount > hiberniaTowerCount)
            {
                DarknessFallOwner = eRealm.Albion;
            }
            else if (midgardTowerCount > albionTowerCount && midgardTowerCount > hiberniaTowerCount)
            {
                DarknessFallOwner = eRealm.Midgard;
            }
            else if (hiberniaTowerCount > midgardTowerCount && hiberniaTowerCount > albionTowerCount)
            {
                DarknessFallOwner = eRealm.Hibernia;
            }
            else
            {
                DarknessFallOwner = eRealm.None;
              
            }

        }
        public static  bool DFOpenForAll(eRealm realm)
        {
            if  (realm == eRealm.None)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// when  keep is taken it check if the realm which take gain the control of DF
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        public static void OnKeepTaken(DOLEvent e, object sender, EventArgs arguments)
        {
            KeepEventArgs args = arguments as KeepEventArgs;
            eRealm realm = (eRealm)args.Keep.Realm;
            CheckDFOwner();


            if (DarknessFallOwner == eRealm.None)
            {
                DarknessFallOwner = eRealm.None;
            }
            else if (realm != DarknessFallOwner)
            {
                int currentDFOwnerTowerCount = KeepMgr.GetTowerCountByRealm(DarknessFallOwner);
                int challengerOwnerTowerCount = KeepMgr.GetTowerCountByRealm(realm);
                if (currentDFOwnerTowerCount < challengerOwnerTowerCount)
                    if (ServerProperties.Properties.ALLOW_ALL_REALMS_DF == true)
                    {
                        DarknessFallOwner = eRealm.None;
                    }
                if (ServerProperties.Properties.ALLOW_ALL_REALMS_DF == false)
                {
                    DarknessFallOwner = realm;
                }



            }
        }
    }
}
