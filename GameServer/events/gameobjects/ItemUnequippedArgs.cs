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
using DOL.GS;
using System;

namespace DOL.Events
{
    /// <summary>
    /// Holds the arguments for the ItemUnEquipped event of PlayerInventory
    /// </summary>
    public class ItemUnequippedArgs : EventArgs
	{
		private readonly InventoryItem m_item;
		private readonly eInventorySlot m_previousSlotPos;

		/// <summary>
		/// Constructs a new ItemEquippedArgs
		/// </summary>
		/// <param name="item">The unequipped item</param>
		/// <param name="previousSlotPos">The slot position item had before it was equipped</param>
		public ItemUnequippedArgs(InventoryItem item, eInventorySlot previousSlotPos)
		{
			m_item = item;
			m_previousSlotPos = previousSlotPos;
		}

		/// <summary>
		/// Constructs a new ItemEquippedArgs
		/// </summary>
		/// <param name="item">The unequipped item</param>
		/// <param name="previousSlotPos">The slot position item had before it was equipped</param>
		public ItemUnequippedArgs(InventoryItem item, int previousSlotPos)
		{
			m_item = item;
			m_previousSlotPos = (eInventorySlot)previousSlotPos;
		}

		/// <summary>
		/// Gets the unequipped item
		/// </summary>
		public InventoryItem Item
		{
			get { return m_item; }
		}

		/// <summary>
		/// Gets the previous slot position
		/// </summary>
		public eInventorySlot PreviousSlotPosition
		{
			get { return m_previousSlotPos; }
		}
	}
}
