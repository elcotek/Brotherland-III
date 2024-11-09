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


namespace DOL.GS
{
    /// <summary>
    /// Holds a GameSiegeBallista item in the world that will disappear after some interval
    /// </summary>
    public class GameSiegeTimedBallista : GameSiegeBallista
    {
        /// <summary>
        /// How long this object can stay in the world without being removed
        /// </summary>
        protected uint m_removeDelay = 120000 * 15; //Currently 30 mins
        /// <summary>
        /// The timer that will remove this object from the world after a delay
        /// </summary>
        protected RemoveItemAction m_removeItemAction;

        /// <summary>
        /// Creates a new GameSiegeBallista item that will disappear after X minutes
        /// </summary>
        public GameSiegeTimedBallista()
            : base()
        {
        }

        /// <summary>
        /// Creates a new GameSiegeBallista item that will disappear after the given
        /// tick-count
        /// </summary>
        /// <param name="vanishTicks">milliseconds after which the item will vanish</param>
        public GameSiegeTimedBallista(uint vanishTicks)
            : this()
        {
            if (vanishTicks > 0)
                m_removeDelay = vanishTicks;
        }

        /// <summary>
        /// Gets or Sets the delay in gameticks after which this object is removed
        /// </summary>
        public uint RemoveDelay
        {
            get
            {
                return m_removeDelay;
            }
            set
            {
                if (value > 0)
                    m_removeDelay = value;
                if (m_removeItemAction.IsAlive)
                    m_removeItemAction.Start((int)m_removeDelay);
            }
        }

        /// <summary>
        /// Removes this object from the world
        /// </summary>
        public override void Delete()
        {
            if (m_removeItemAction != null)
            {
                m_removeItemAction.Stop();
                m_removeItemAction = null;
            }
            base.Delete();
        }

        /// <summary>
        /// Adds this object to the world
        /// </summary>
        /// <returns>true if successfull</returns>
        public override bool AddToWorld()
        {
            if (!base.AddToWorld()) return false;
            if (m_removeItemAction == null)
                m_removeItemAction = new RemoveItemAction(this);
            m_removeItemAction.Start((int)m_removeDelay);
            return true;
        }

        /// <summary>
        /// The callback function that will remove this GameSiegeBallista after some time
        /// </summary>
        protected class RemoveItemAction : RegionAction
        {
            /// <summary>
            /// Constructs a new remove action
            /// </summary>
            /// <param name="item"></param>
            public RemoveItemAction(GameSiegeTimedBallista item)
                : base(item)
            {
            }

            /// <summary>
            /// The callback function that will remove this GameSiegeBallista after some time
            /// </summary>
            protected override void OnTick()
            {
                GameSiegeTimedBallista item = (GameSiegeTimedBallista)m_actionSource;
                //remove this object from the world after some time
                item.Delete();
            }
        }
    }
}
