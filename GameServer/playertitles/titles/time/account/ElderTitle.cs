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

/*
 * Suncheck: [19.06.2007]
 *   - Corrected
 *   - Sorted
 *   - Added missing (+language support)
 */
using System;
using DOL.Language;

namespace DOL.GS.PlayerTitles
{
	/// <summary>
	/// "Elder" title granted to all chars on all accounts that created for at least one year.
	/// </summary>
	public class ElderTitle : SimplePlayerTitle
	{
		/// <summary>
		/// The title description, shown in "Titles" window.
		/// </summary>
		/// <param name="player">The title owner.</param>
		/// <returns>The title description.</returns>
		public override string GetDescription(GamePlayer player)
		{
			return LanguageMgr.GetTranslation(player.Client.Account.Language, "Titles.Time.Account.ElderTitle");
		}

		/// <summary>
		/// The title value, shown over player's head.
		/// </summary>
		/// <param name="player">The title owner.</param>
		/// <returns>The title value.</returns>
		public override string GetValue(GamePlayer player)
		{
			return LanguageMgr.GetTranslation(player.Client.Account.Language, "Titles.Time.Account.ElderTitle");
		}

		/// <summary>
		/// Verify whether the player is suitable for this title.
		/// </summary>
		/// <param name="player">The player to check.</param>
		/// <returns>true if the player is suitable for this title.</returns>
		public override bool IsSuitable(GamePlayer player)
		{
            TimeSpan t = DateTime.Now.Subtract(player.Client.Account.CreationDate);
			return t.TotalDays >= 365; // ~year
			//Why one year? At live you will get it after 4 years
		}
	}
}
