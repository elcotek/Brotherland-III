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
using System;

namespace DOL.GS.Effects
{
    /// <summary>
    /// The helper class for the interacions
    /// </summary>
    public class ArtifactDoorEffect : TimedEffect, IGameEffect
    {

        /// <summary>
        /// Creates a new interact effect
        /// </summary>
        public ArtifactDoorEffect()
            : base(AEDoorOpener.openDuration)
        {
        }

        /// <summary>
        /// Start the interact on a living
        /// </summary>
        public override void Start(GameLiving living)
        {
            base.Start(living);

            GameEventMgr.AddHandler(living, GameLivingEvent.Interact, new DOLEventHandler(NPCInteractFinish));

            if (living is GameLiving)
            {
                // Console.Out.WriteLine("interaction is starting");

            }
        }

        public override void Stop()
        {
            base.Stop();


            // there is no animation on end of the effect
            if (m_owner is GameLiving)
                GameEventMgr.RemoveHandler(m_owner, GameLivingEvent.Interact, new DOLEventHandler(NPCInteractFinish));
            //(m_owner as GameNPC).OpenArtifactDoors(m_owner);


        }
        /// <summary>
		/// Handler fired on every interaction
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
        protected void NPCInteractFinish(DOLEvent e, object sender, EventArgs arguments)
        {
            GameLiving living = sender as GameLiving;
            if (living == null) return;


            InteractEventArgs atkArgs = arguments as InteractEventArgs;

            if (atkArgs != null)
            {
                //(living.AEDoorOpenerBrain).OPENDOOR();  //OPENDOOR();// Console.Out.WriteLine("Interaction löschen...");
            }

            return;
        }
        /*
        private void OPENDOOR()

        {

            int myid = 87159201;
            DBDoor DoorDBTEST = GameServer.Database.FindObjectByKey<DBDoor>(myid);
            if (DoorDBTEST != null)
            {
                Console.Out.WriteLine("DoorDBFound !");
            }
            List<IDoor> GameDoorListTest = DoorMgr.getDoorByID(myid);
            if (GameDoorListTest.Count > 0)
            {
                Console.Out.WriteLine("IDoorFound !");
                foreach (IDoor mydoor in GameDoorListTest)
                {
                    GameDoor door = mydoor as GameDoor;
                    if (door.State != eDoorState.Open)
                        mydoor.Open();
                    return;
                }
                */

    }
}


