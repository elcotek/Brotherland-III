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
using DOL.GS;
using System;
namespace DOL.Events
{
    /// <summary>
    /// Holds the arguments for the FollowLostTarget event of GameNpc
    /// </summary>
    public class FollowLostTargetEventArgs : EventArgs
	{
		private readonly GameObject m_lostTarget;

		/// <summary>
		/// Constructs new FollowLostTargetEventArgs
		/// </summary>
		/// <param name="lostTarget">The lost follow target</param>
		public FollowLostTargetEventArgs(GameObject lostTarget)
		{
			m_lostTarget = lostTarget;
		}

		/// <summary>
		/// Gets the lost follow target
		/// </summary>
		public GameObject LostTarget
		{
			get { return m_lostTarget; }
		}
	}
}
