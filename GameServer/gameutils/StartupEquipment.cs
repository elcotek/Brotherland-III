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
using DOL.Database;
using System.Collections;
using System.Collections.Generic;

namespace DOL.GS
{
    /// <summary>
    /// Equips new created Characters with standard equipment
    /// </summary>
    public class StartupEquipment
    {
        /// <summary>
        /// Add starter equipment to the character
        /// </summary>
        /// <param name="c">The character</param>
        public static void AddEquipment(DOLCharacters c)
        {
            Hashtable usedSlots = new Hashtable();

            // 0 = for all classes, then quickcheck if it contains the classid
            //System.Collections.Generic.IList<DBStarterEquipment> items = GameServer.Database.SelectObjects<DBStarterEquipment>("`Class` = '0' OR `Class` LIKE '%" + c.Class + "%'");
            IList<DBStarterEquipment> items = GameServer.Database.SelectObjects<DBStarterEquipment>("`Class` = '0' OR `Class` LIKE @Class", new[] { new QueryParameter("@Class", "%" + c.Class + "%") });
                

                foreach (DBStarterEquipment item in items)
            {
                if (item.Template == null)
                {
                    GameServer.Instance.Logger.Error("StartupEquipment.cs error adding starter equipment for class " + c.Class + " cannot find itemtemplate for " + item.TemplateID);
                    continue;
                }

                // deeper check if item is suitable to classid
                if (!Util.IsEmpty(item.Class, true))
                {
                    bool isFind = false;
                    foreach (string currentItem in item.Class.SplitCSV(true))
                    {
                        int.TryParse(currentItem, out int charClass);
                        if (charClass == c.Class)
                        {
                            isFind = true;
                            break;
                        }
                    }
                    if (!isFind)
                        continue;
                }

                InventoryItem inventoryItem = GameInventoryItem.Create<ItemTemplate>(item.Template);
                inventoryItem.OwnerID = c.ObjectId;
                inventoryItem.Realm = c.Realm;

                //if equipable item, equip
                foreach (eInventorySlot slot in GameLivingInventory.EQUIP_SLOTS)
                {
                    if (slot == (eInventorySlot)inventoryItem.Item_Type)
                    {
                        eInventorySlot chosenSlot = eInventorySlot.FirstEmptyBackpack;

                        if (slot == eInventorySlot.LeftHandWeapon && (eObjectType)inventoryItem.Object_Type != eObjectType.Shield && usedSlots.ContainsKey(eInventorySlot.RightHandWeapon) == false)
                        {
                            chosenSlot = eInventorySlot.RightHandWeapon;
                        }
                        else
                        {
                            chosenSlot = slot;
                        }

                        if (usedSlots.ContainsKey(chosenSlot))
                        {
                            GameServer.Instance.Logger.Error("Cannot add item " + item.TemplateID + " to class " + item.Class + " already an item for that slot assigned!");
                            continue;
                        }

                        inventoryItem.SlotPosition = (int)chosenSlot;
                        usedSlots[chosenSlot] = true;
                        if (c.ActiveWeaponSlot == 0)
                        {
                            switch (inventoryItem.SlotPosition)
                            {
                                case Slot.RIGHTHAND:
                                    c.ActiveWeaponSlot = (byte)GamePlayer.eActiveWeaponSlot.Standard;
                                    break;
                                case Slot.TWOHAND:
                                    c.ActiveWeaponSlot = (byte)GamePlayer.eActiveWeaponSlot.TwoHanded;
                                    break;
                                case Slot.RANGED:
                                    c.ActiveWeaponSlot = (byte)GamePlayer.eActiveWeaponSlot.Distance;
                                    break;
                            }
                        }
                    }
                }
                if (inventoryItem.SlotPosition == 0)
                {
                    //otherwise stick the item in the backpack
                    for (int i = (int)eInventorySlot.FirstBackpack; i < (int)eInventorySlot.LastBackpack; i++)
                    {
                        if (usedSlots[i] == null)
                        {
                            inventoryItem.SlotPosition = i;
                            usedSlots[i] = true;
                            break;
                        }
                    }
                }
                GameServer.Database.AddObject(inventoryItem);
            }
        }
    }
}