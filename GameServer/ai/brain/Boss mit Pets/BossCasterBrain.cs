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
using DOL.GS;
using log4net;
using System.Reflection;

namespace DOL.AI.Brain
{
    /// <summary>
    /// A brain that can be controlled
    /// </summary>
    public class BossCasterBrain : BossPetBrain
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructs new controlled npc brain
        /// </summary>
        /// <param name="owner"></param>
        public BossCasterBrain(GameLiving owner) : base(owner) { }

        #region AI

        /// <summary>
        /// Checks the Abilities
        /// </summary>
        public override void CheckAbilities() { }

        /// <summary>
        /// Checks the Positive Spells.  Handles buffs, heals, etc.
        /// </summary>
        protected override bool CheckDefensiveSpells(Spell spell) { return false; }

        #endregion
    }
}
