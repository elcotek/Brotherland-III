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
using DOL.AI.Brain;



namespace DOL.GS
{
    /*
	 * These boats are very fast, and can carry up to sixteen passengers.
	 * You have thirty seconds to board the boat before it sets sail.
	 * You can board the boat by double clicking on it,
	 * typing /vboard or using your `Get key' with the boat targeted. 
	 * You will automatically leave the boat when it reaches its destination,
	 * but if you wish to leave before then, just type `/disembark'.
	 * or press the jump key
	 */
    public class GameTaxiBoat : GameMovingObject
	{
		public GameTaxiBoat()
			: base()
		{
			Model = 2650;
			Level = 0;
			Flags = eFlags.PEACE;
			Name = "boat";
			MaxSpeedBase = 4000;
			BlankBrain brain = new BlankBrain();
			SetOwnBrain(brain);
		}

        /// <summary>
        /// The distance this object can be interacted with - we need this to be larger for the boat
        /// </summary>
        public override int InteractDistance
        {
            get
            {
                return 666;
            }
        }

		public override int MAX_PASSENGERS
		{
			get
			{
				return 16;
			}
		}

		public override int SLOT_OFFSET
		{
			get
			{
				return 1;
			}
		}

		public override short MaxSpeed
		{
			get
			{
				return 4000;
			}
		}
	}
}
