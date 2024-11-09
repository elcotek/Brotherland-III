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
using System.Collections;

namespace DOL.GS.Keeps
{
    /// <summary>
    /// GameKeep is the keep in New Frontiere
    /// </summary>
    public class GameKeep : AbstractGameKeep
    {
        public GameKeep()
            : base()
        {
        }

        /// <summary>
        /// time to upgrade from one level to another
        /// </summary>
        public static int[] UpgradeTime =
        {
            60*60*1000, // 0 60min 1h
            60*60*1000, // 1 60min 1h
			60*60*1000, // 2 60min 1h
			60*60*1000, // 3 60min 1h
			60*60*1000, // 4 60min 1h
			60*60*1000, // 5 60min 1h
			60*60*1000, // 6 60min 1h
			60*60*1000, // 7 60min 1h
			60*60*1000, // 8 60min 1h
			60*60*1000, // 9 60min 1h
			60*60*1000, // 10 60min 1h
        };
        private ArrayList m_towers = new ArrayList(4);
        /// <summary>
        /// The Keep Towers
        /// </summary>
        public ArrayList Towers
        {
            get { return m_towers; }
            set { m_towers = value; }
        }

        public bool OwnsAllTowers
        {
            get
            {
                foreach (GameKeepTower tower in this.Towers)
                {
                    if (tower.Realm != this.Realm)
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// The time to upgrade a keep
        /// </summary>
        /// <returns></returns>
        public override int CalculateTimeToUpgrade()
        {
            if (Level < 10)
                return UpgradeTime[this.Level];

            return 0;
        }

        /// <summary>
        /// The checks we need to run before allowing claim
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool CheckForClaim(GamePlayer player)
        {
            //let gms do everything
            if (player.Client.Account.PrivLevel > 1)
                return true;


            if (ServerProperties.Properties.CLAIM_NUM > 1)
            {

                if (player.Group == null)
                {

                    
                        player.Out.SendMessage("You must be in a group to claim.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return false;
                   
                }
            }

            if (player.Group != null && player.Group.MemberCount < ServerProperties.Properties.CLAIM_NUM)
            {
                player.Out.SendMessage("You need " + ServerProperties.Properties.CLAIM_NUM + " players to claim.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            return base.CheckForClaim(player);
        }

        private void BroadcastMessage(string v, eRealm realm)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The RP reward for claiming based on difficulty level
        /// </summary>
        /// <returns></returns>
        public override int CalculRP()
        {
            return ServerProperties.Properties.KEEP_RP_CLAIM_MULTIPLIER * DifficultyLevel;
        }

        /// <summary>
        /// Add a tower to the keep
        /// </summary>
        /// <param name="tower"></param>
        public void AddTower(GameKeepTower tower)
        {
            if (!m_towers.Contains(tower))
                m_towers.Add(tower);
        }

        public override void Reset(eRealm realm)
        {
            base.Reset(realm);
           
        }
    }
}
