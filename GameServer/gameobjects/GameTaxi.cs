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
    /// <summary>
    /// 
    /// </summary>
    public class GameTaxi : GameNPC
	{
		public GameTaxi() : base()
		{
			Model = 450;
			MaxSpeedBase = 500;
			Size = 63;
			Level = 55;
			Name = "horse";
            
            BlankBrain brain = new BlankBrain();
			SetOwnBrain(brain);
		}
		
		public GameTaxi(INpcTemplate templateid) : base(templateid)
		{
			BlankBrain brain = new BlankBrain();
			SetOwnBrain(brain);
		}
		
		public override int MAX_PASSENGERS
		{
			get
			{
				return 1;
			}
		}

		public override int SLOT_OFFSET
		{
			get
			{
				return 0;
			}
		}
	}
}
