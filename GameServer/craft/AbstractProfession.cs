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
using DOL.Language;
using System;

namespace DOL.GS
{
    /// <summary>
    /// Tradeskills that earn the crafter a title.
    /// <author>Aredhel</author>
    /// </summary>
    public abstract class AbstractProfession : AbstractCraftingSkill
    {
        #region Title

        protected abstract String Profession { get; }

        public static String GetTitleFormat(int skillLevel, GamePlayer player)
        {
            if (skillLevel < 0)
                throw new ArgumentOutOfRangeException("crafter skill level must be >= 0");

            //String language = ServerProperties.Properties.SERV_LANGUAGE;

            switch (skillLevel / 100)
            {
                case 0: return LanguageMgr.GetTranslation(player.Client.Account.Language, "CraftersTitle.Helper");
                case 1: return LanguageMgr.GetTranslation(player.Client.Account.Language, "CraftersTitle.JuniorApprentice");
                case 2: return LanguageMgr.GetTranslation(player.Client.Account.Language, "CraftersTitle.Apprentice");
                case 3: return LanguageMgr.GetTranslation(player.Client.Account.Language, "CraftersTitle.Neophyte");
                case 4: return LanguageMgr.GetTranslation(player.Client.Account.Language, "CraftersTitle.Assistant");
                case 5: return LanguageMgr.GetTranslation(player.Client.Account.Language, "CraftersTitle.Junior");
                case 6: return LanguageMgr.GetTranslation(player.Client.Account.Language, "CraftersTitle.Journeyman");
                case 7: return LanguageMgr.GetTranslation(player.Client.Account.Language, "CraftersTitle.Senior");
                case 8: return LanguageMgr.GetTranslation(player.Client.Account.Language, "CraftersTitle.Master");
                case 9: return LanguageMgr.GetTranslation(player.Client.Account.Language, "CraftersTitle.Grandmaster");
                case 10: return LanguageMgr.GetTranslation(player.Client.Account.Language, "CraftersTitle.Legendary");
                default: return LanguageMgr.GetTranslation(player.Client.Account.Language, "CraftersTitle.LegendaryGrandmaster");
            }
        }

        public String GetTitle(int skillLevel, GamePlayer player)
        {
            try
            {
                return String.Format(GetTitleFormat(skillLevel, player), Profession);
            }
            catch
            {
                return "<you may want to check your Crafting.txt language file>";
            }
        }

        #endregion
    }
}
