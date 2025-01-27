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
using DOL.AI.Brain;

namespace DOL.GS
{
    public class BossSubPet : BossPet
    {
        /// <summary>
        /// Holds the different subpet ids
        /// </summary>
        public enum SubPetType : byte
        {
            Melee = 0,
            Healer = 1,
            Caster = 2,
            Debuffer = 3,
            Buffer = 4,
            Archer = 5
        }

        /// <summary>
        /// Create a commander.
        /// </summary>
        /// <param name="npcTemplate"></param>
        /// <param name="owner"></param>
        public BossSubPet(INpcTemplate npcTemplate) : base(npcTemplate) { }


        public override short MaxSpeed
        {
            get
            {
                if ((Brain as IControlledBrain).Owner.CurrentSpeed > 0)
                {
                    return (Brain as IControlledBrain).Owner.CurrentSpeed;
                }
                else
                    return (Brain as IControlledBrain).Owner.MaxSpeed;
            }
        }

    }
}