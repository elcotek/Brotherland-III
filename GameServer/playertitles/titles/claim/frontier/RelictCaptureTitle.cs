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

/*
 * Suncheck: [19.06.2007]
 *   - Corrected
 *   - Sorted
 *   - Added missing (+language support)
 */

using System;
using DOL.Language;
using DOL.Events;

namespace DOL.GS.PlayerTitles
{
    public class RelictCaptureTitle : EventPlayerTitle
    {
        /// <summary>
        /// The title description, shown in "Titles" window.
        /// </summary>
        /// <param name="player">The title owner.</param>
        /// <returns>The title description.</returns>
        public override string GetDescription(GamePlayer player)
        {
            return LanguageMgr.GetTranslation(player.Client.Account.Language, "Titles.Claim.Frontier.RelictChallenger");
        }

        /// <summary>
        /// The title value, shown over player's head.
        /// </summary>
        /// <param name="player">The title owner.</param>
        /// <returns>The title value.</returns>
        public override string GetValue(GamePlayer player)
        {
            return LanguageMgr.GetTranslation(player.Client.Account.Language, "Titles.Claim.Frontier.RelictChallenger");
        }

        /// <summary>
        /// The event to hook.
        /// </summary>
        public override DOLEvent Event
        {
            get { return GamePlayerEvent.CapturedRelicsChanged; }
        }
        /// <summary>
        /// Verify whether the player is suitable for this title.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns>true if the player is suitable for this title.</returns>
        public override bool IsSuitable(GamePlayer player)
        {
            //2+
            return (player.CapturedRelics) >= 2;
        }

        /// <summary>
        /// The event callback.
        /// </summary>
        /// <param name="e">The event fired.</param>
        /// <param name="sender">The event sender.</param>
        /// <param name="arguments">The event arguments.</param>
        protected override void EventCallback(DOLEvent e, object sender, EventArgs arguments)
        {
            GamePlayer p = sender as GamePlayer;
            if (p != null && p.Titles.Contains(this))
            {
                p.UpdateCurrentTitle();
                return;
            }
            base.EventCallback(e, sender, arguments);
        }
    }
}
