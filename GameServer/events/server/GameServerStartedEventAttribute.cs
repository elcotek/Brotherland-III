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
using System;

namespace DOL.Events
{
    /// <summary>
    /// This attribute can be applied to static methods to automatically
    /// register them with the GameServer's global start event
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
	public class GameServerStartedEventAttribute : Attribute
	{
		/// <summary>
		/// Constructs a new GameServerStartedEventAttribute
		/// </summary>
		public GameServerStartedEventAttribute()
		{
		}
	}
}
