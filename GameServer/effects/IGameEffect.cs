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
using System.Collections.Generic;

namespace DOL.GS.Effects
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGameEffect
	{
		/// <summary>
		/// Effect must be canceled
		/// </summary>
		/// <param name="playerCanceled">true if player decided to cancel that effect by shift + rightclick</param>
		void Cancel(bool playerCanceled);
	
		/// <summary>
		/// Name of the effect
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Remaining Time of the effect in seconds
		/// </summary>
		int RemainingTime { get; }

		/// <summary>
		/// Icon to show on players, can be id
		/// </summary>
		ushort Icon { get; }

		/// <summary>
		/// Unique ID, will be set by effect list on add
		/// </summary>
		ushort InternalID { get; set; }

		
        /// <summary>
        /// The DelveInfo
        /// </summary>
        IList<string> DelveInfo(GamePlayer player);
        /// <summary>
        /// Get the save effect
        /// </summary>
        /// <returns></returns>
        PlayerXEffect GetSavedEffect();
	}
}
