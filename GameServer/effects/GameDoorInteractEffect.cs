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
using DOL.Events;
using DOL.GS.Keeps;
using System;
using System.Collections.Generic;

namespace DOL.GS.Effects
{
    /// <summary>
    /// The helper class for the interactions
    /// </summary>
    public class GameDoorInteractEffect : TimedEffect, IGameEffect
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Creates a new interact effect
        /// </summary>
        public GameDoorInteractEffect()
              : base(GameDoor.OpenDuration)
        {
        }

        /// <summary>
        /// Returns true if the realm of the player is the same of the keep realm.
        /// </summary>
        /// <param name="player"></param>
        /// <returns>true/false</returns>
        static bool GetGemkeepRealm(GamePlayer player)
        {
            //Die eigenen Keeps darf man zurück erobern
            ICollection<AbstractGameKeep> keepList = KeepMgr.GetKeepsOfRegion(player.CurrentRegionID);
            foreach (AbstractGameKeep keep in keepList)
            {
                if (keep is GameKeep && keep.Region == player.CurrentRegionID)
                {
                   // log.DebugFormat("keep name =  {0} keep realm  {1}", keep.Name, keep.Realm);
                    
                    foreach (AbstractArea a in keep.CurrentRegion.GetAreasOfSpot(player.X, player.Y, player.Z))
                    {
                        if (a is GameKeepArea && a.Description == keep.Name && player.Realm == keep.Realm)
                        {
                            
                            return true;
                        }
                    }

                }

            }
           
            return false;
        }
        /// <summary>
        /// Start the interact on a living
        /// </summary>
        public override void Start(GameLiving living)
        {
            base.Start(living);

            GameEventMgr.AddHandler(living, GameLivingEvent.Interact, new DOLEventHandler(DoorInteractInteractFinish));

            if (living is GameDoor)
            {

               //Console.Out.WriteLine("interaction start");


                //Caledonia keep Doors special handling
                if ((living as GameDoor).DoorID == 250000305)
                {
                    foreach (GamePlayer player in living.GetPlayersInRadius(WorldMgr.INTERACT_DISTANCE))
                    {

                        if (player != null && GetGemkeepRealm(player))
                        {
                            player.MoveTo(166, 33603, 37435, 4106, 3621);
                            
                        }
                        break;

                    }

                }
                if ((living as GameDoor).DoorID == 250000306)
                {
                    foreach (GamePlayer player in living.GetPlayersInRadius(WorldMgr.INTERACT_DISTANCE))
                    {
                        if (player != null && GetGemkeepRealm(player))
                        {
                            player.MoveTo(166, 33485, 37282, 3930, 1576);
                            
                        }
                        break;

                    }
                }

            }
        }

        public override void Stop()
        {
            base.Stop();


            // there is no animation on end of the effect
            if (m_owner is GameDoor)
                GameEventMgr.RemoveHandler(m_owner, GameLivingEvent.Interact, new DOLEventHandler(DoorInteractInteractFinish));
            //(m_owner as GameDoor).GameDoorInteractActionEnd();

           // Console.Out.WriteLine("interaction ende");
        }
        /// <summary>
		/// Handler fired on every interaction
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
        protected void DoorInteractInteractFinish(DOLEvent e, object sender, EventArgs arguments)
        {
            GameLiving living = sender as GameLiving;
            if (living == null) return;


            InteractEventArgs atkArgs = arguments as InteractEventArgs;

            if (atkArgs != null)
            {
               // Console.Out.WriteLine("Interaction löschen...");
            }

            return;
        }
    }
}
