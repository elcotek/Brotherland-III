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
using DOL.Events;
using DOL.GS.Behaviour.Attributes;
using System;
using System.Runtime.InteropServices;

namespace DOL.GS.Behaviour.Actions
{
    [ComVisible(false)]
    [ActionAttribute(ActionType = eActionType.ReplaceItem)]
    public class ReplaceItemAction : AbstractAction<ItemTemplate, ItemTemplate>
    {

        public ReplaceItemAction(GameNPC defaultNPC, Object p, Object q)
            : base(defaultNPC, eActionType.ReplaceItem, p, q)
        {
        }


        public ReplaceItemAction(GameNPC defaultNPC, ItemTemplate oldItemTemplate, ItemTemplate newItemTemplate)
            : this(defaultNPC, (object)oldItemTemplate, (object)newItemTemplate) { }



        public override void Perform(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = BehaviourUtils.GuessGamePlayerFromNotify(e, sender, args);

            ItemTemplate oldItem = P;
            ItemTemplate newItem = Q;

            //TODO: what about stacked items???
            if (player.Inventory.RemoveTemplate(oldItem.Id_nb, 1, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
            {
                //InventoryLogging.LogInventoryAction(player, NPC, eInventoryActionType.Quest, oldItem, 1);
                InventoryItem inventoryItem = GameInventoryItem.Create<ItemTemplate>(newItem);
                // if (player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, inventoryItem))
                // InventoryLogging.LogInventoryAction(NPC, player, eInventoryActionType.Quest, newItem, 1);
            }
        }
    }
}
