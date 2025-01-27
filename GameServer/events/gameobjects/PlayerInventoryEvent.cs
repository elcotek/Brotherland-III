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
namespace DOL.Events
{
    /// <summary>
    /// This class holds all possible player inventory events.
    /// Only constants defined here!
    /// </summary>
    public class PlayerInventoryEvent : DOLEvent
	{
		/// <summary>
		/// Constructs a new PlayerInventory event
		/// </summary>
		/// <param name="name"></param>
		public PlayerInventoryEvent(string name) : base(name)
		{
		}

		/// <summary>
		/// The item was just equipped
		/// </summary>
		public static readonly PlayerInventoryEvent ItemEquipped = new PlayerInventoryEvent("PlayerInventory.ItemEquipped");
		/// <summary>
		/// The item was just unequipped
		/// </summary>
		public static readonly PlayerInventoryEvent ItemUnequipped = new PlayerInventoryEvent("PlayerInventory.ItemUnequipped");
		/// <summary>
		/// The item was just dropped
		/// </summary>
		public static readonly PlayerInventoryEvent ItemDropped = new PlayerInventoryEvent("PlayerInventory.ItemDropped");
		/// <summary>
		/// A bonus on an item changed.
		/// </summary>
		public static readonly PlayerInventoryEvent ItemBonusChanged = new PlayerInventoryEvent("PlayerInventory.ItemBonusChanged");
	}
}
