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
using DOL.Language;
using log4net;
using System.Reflection;

namespace DOL.GS.PacketHandler.Client.v168
{
    [PacketHandler(PacketHandlerType.TCP, eClientPackets.PlayerMoveItem, eClientStatus.PlayerInGame)]
    public class PlayerMoveItemRequestHandler : IPacketHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void HandlePacket(GameClient client, GSPacketIn packet)
        {
            if (client.Player == null || packet == null)
            {
                return;
            }

            ushort id = packet.ReadShort();
            ushort toClientSlot = packet.ReadShort();
            ushort fromClientSlot = packet.ReadShort();
            ushort itemCount = packet.ReadShort();

            /// ChatUtil.SendDebugMessage(client, "GM: MoveItem; id=" + id.ToString() + " client fromSlot=" + fromClientSlot.ToString() + " client toSlot=" + toClientSlot.ToString() + " itemCount=" + itemCount.ToString());

            // If our toSlot is > 1000 then target is a game object (not a window) with an ObjectID of toSlot - 1000

            if (toClientSlot > 1000)
            {
                ushort objectID = (ushort)(toClientSlot - 1000);
                GameObject obj = WorldMgr.GetObjectByIDFromRegion(client.Player.CurrentRegionID, objectID);
                if (obj == null || obj.ObjectState != GameObject.eObjectState.Active)
                {
                    client.Out.SendInventorySlotsUpdate(new int[] { fromClientSlot });
                    client.Out.SendMessage("Invalid trade target. (" + objectID + ")", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }

                GamePlayer tradeTarget = obj as GamePlayer;
                // If our target is another player we set the tradetarget
                // trade permissions are done in GamePlayer
                if (tradeTarget != null)
                {
                    if (tradeTarget.Client.ClientState != GameClient.eClientState.Playing)
                    {
                        client.Out.SendInventorySlotsUpdate(new int[] { fromClientSlot });
                        client.Out.SendMessage("Can't trade with inactive players.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
                    if (tradeTarget == client.Player)
                    {
                        client.Out.SendInventorySlotsUpdate(new int[] { fromClientSlot });
                        client.Out.SendMessage("You can't trade with yourself, silly!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
                    if (!GameServer.ServerRules.IsAllowedToTrade(client.Player, tradeTarget, false))
                    {
                        client.Out.SendInventorySlotsUpdate(new int[] { fromClientSlot });
                        return;
                    }
                }

                // Is the item we want to move in our backpack?
                // we also allow drag'n drop from equipped to blacksmith
                if ((fromClientSlot >= (ushort)eInventorySlot.FirstBackpack &&
                     fromClientSlot <= (ushort)eInventorySlot.LastBackpack) ||
                    (obj is Blacksmith &&
                     fromClientSlot >= (ushort)eInventorySlot.MinEquipable &&
                     fromClientSlot <= (ushort)eInventorySlot.MaxEquipable))
                {
                    if (!obj.IsWithinRadius(client.Player, WorldMgr.GIVE_ITEM_DISTANCE))
                    {
                        // show too far away message
                        if (obj is GamePlayer)
                        {
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "PlayerMoveItemRequestHandler.TooFarAway", client.Player.GetName((GamePlayer)obj)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        else
                        {
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "PlayerMoveItemRequestHandler.TooFarAway", obj.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }

                        client.Out.SendInventorySlotsUpdate(new int[] { fromClientSlot });
                        return;
                    }

                    InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)fromClientSlot);
                    if (item == null)
                    {
                        client.Out.SendInventorySlotsUpdate(new int[] { fromClientSlot });
                        client.Out.SendMessage("Null item (client slot# " + fromClientSlot + ").", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
                    //Neu f�r NPCs und h�heren count als 1
                    if (obj is GameNPC && item.Count > 1)
                    {

                        // see if any event handlers will handle this move
                        client.Player.Notify(GamePlayerEvent.GiveItem, client.Player, new GiveItemEventArgs(client.Player, obj, item));
                    }

                    if (obj is GameNPC == false || item.Count == 1)
                    {
                       
                        // see if any event handlers will handle this move
                        client.Player.Notify(GamePlayerEvent.GiveItem, client.Player, new GiveItemEventArgs(client.Player, obj, item));
                    }

                    //If the item has been removed by the event handlers, return;
                    if (item == null || item.OwnerID == null)
                    {
                        client.Out.SendInventorySlotsUpdate(new int[] { fromClientSlot });
                        return;
                    }

                    // if a player to a GM and item is not dropable then don't allow trade???? This seems wrong.
                    if (client.Account.PrivLevel == (uint)ePrivLevel.Player && tradeTarget != null && tradeTarget.Client.Account.PrivLevel != (uint)ePrivLevel.Player)
                    {
                        if (!item.IsDropable && !(obj is GameNPC && (obj is Blacksmith || obj is Recharger || (obj as GameNPC).CanTradeAnyItem)))
                        {
                            client.Out.SendInventorySlotsUpdate(new int[] { fromClientSlot });
                            client.Out.SendMessage("You can not remove this item!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }
                    }

                    if (tradeTarget != null)
                    {
                        // This is a player trade, let trade code handle
                        tradeTarget.ReceiveTradeItem(client.Player, item);
                        client.Out.SendInventorySlotsUpdate(new int[] { fromClientSlot });
                        return;
                    }

                    if (obj.ReceiveItem(client.Player, item))
                    {
                        // this object was expecting an item and handled it
                        client.Out.SendInventorySlotsUpdate(new int[] { fromClientSlot });
                        return;
                    }

                    client.Out.SendInventorySlotsUpdate(new int[] { fromClientSlot });
                    return;
                }

                //Is the "item" we want to move money? For Version 1.78+
                if (client.Version >= GameClient.eClientVersion.Version178 &&
                    fromClientSlot >= (int)eInventorySlot.Mithril178 &&
                    fromClientSlot <= (int)eInventorySlot.Copper178)
                {
                    fromClientSlot -= eInventorySlot.Mithril178 - eInventorySlot.Mithril;
                }

                //Is the "item" we want to move money?
                if (fromClientSlot >= (ushort)eInventorySlot.Mithril && fromClientSlot <= (ushort)eInventorySlot.Copper)
                {
                    int[] money = new int[5];
                    money[fromClientSlot - (ushort)eInventorySlot.Mithril] = itemCount;
                    long flatMoney = Money.GetMoney(money[0], money[1], money[2], money[3], money[4]);

                    if (client.Version >= GameClient.eClientVersion.Version178) // add it back for proper slot update...
                    {
                        fromClientSlot += eInventorySlot.Mithril178 - eInventorySlot.Mithril;
                    }

                    if (!obj.IsWithinRadius(client.Player, WorldMgr.GIVE_ITEM_DISTANCE))
                    {
                        // show too far away message
                        if (obj is GamePlayer)
                        {
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "PlayerMoveItemRequestHandler.TooFarAway", client.Player.GetName((GamePlayer)obj)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        else
                        {
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "PlayerMoveItemRequestHandler.TooFarAway", obj.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }

                        client.Out.SendInventorySlotsUpdate(new int[] { fromClientSlot });
                        return;
                    }

                    if (flatMoney > client.Player.GetCurrentMoney())
                    {
                        client.Out.SendInventorySlotsUpdate(new int[] { fromClientSlot });
                        return;
                    }

                    client.Player.Notify(GamePlayerEvent.GiveMoney, client.Player, new GiveMoneyEventArgs(client.Player, obj, flatMoney));

                    if (tradeTarget != null)
                    {
                        tradeTarget.ReceiveTradeMoney(client.Player, flatMoney);
                        client.Out.SendInventorySlotsUpdate(new int[] { fromClientSlot });
                        return;
                    }

                    if (obj.ReceiveMoney(client.Player, flatMoney))
                    {
                        client.Out.SendInventorySlotsUpdate(new int[] { fromClientSlot });
                        return;
                    }

                    client.Out.SendInventorySlotsUpdate(new int[] { fromClientSlot });
                    return;
                }

                client.Out.SendInventoryItemsUpdate(null);
                return;
            }

            // We did not drop an item on a game object, which means we should have valid from and to slots 
            // since we are moving an item from one window to another.

            // First check for activated InventoryObjects even if the player does not have one targeted

            if (client.Player.ActiveInventoryObject != null && client.Player.ActiveInventoryObject.MoveItem(client.Player, fromClientSlot, toClientSlot))
            {
                // Object handled the move so stop processing
                return;
            }

            // Check to see if the player has a target object of type IGameInventoryObject and
            // if they do let the object check and handle the move if it can
            if (client.Player.TargetObject is IGameInventoryObject && (client.Player.TargetObject as IGameInventoryObject).CanHandleMove(client.Player, fromClientSlot, toClientSlot))
            {
                if ((client.Player.TargetObject as IGameInventoryObject).MoveItem(client.Player, fromClientSlot, toClientSlot))
                {
                    // Object handled the move so stop processing
                    return;
                }
            }
            //Do we want to move an item from immediate inventory to immediate inventory or drop on the ground
            if (((fromClientSlot >= (ushort)eInventorySlot.Ground && fromClientSlot <= (ushort)eInventorySlot.LastBackpack)
                || (fromClientSlot >= (ushort)eInventorySlot.FirstVault && fromClientSlot <= (ushort)eInventorySlot.LastVault)
                || (fromClientSlot >= (ushort)eInventorySlot.FirstBagHorse && fromClientSlot <= (ushort)eInventorySlot.LastBagHorse))
                && ((toClientSlot >= (ushort)eInventorySlot.Ground && toClientSlot <= (ushort)eInventorySlot.LastBackpack)
                || (toClientSlot >= (ushort)eInventorySlot.FirstVault && toClientSlot <= (ushort)eInventorySlot.LastVault)
                || (toClientSlot >= (ushort)eInventorySlot.FirstBagHorse && toClientSlot <= (ushort)eInventorySlot.LastBagHorse)))
            {
                InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)fromClientSlot);
                //We want to drop the item
                if (toClientSlot == (ushort)eInventorySlot.Ground)
                {
                   



                    if (item == null)
                    {
                        client.Out.SendInventorySlotsUpdate(new int[] { fromClientSlot });
                        client.Out.SendMessage("Invalid item (slot# " + fromClientSlot + ").", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
                    if (fromClientSlot < (ushort)eInventorySlot.FirstBackpack)
                    {
                        client.Out.SendInventorySlotsUpdate(new int[] { fromClientSlot });
                        return;
                    }
                    if (!item.IsDropable)
                    {
                        client.Out.SendInventorySlotsUpdate(new int[] { fromClientSlot });
                        client.Out.SendMessage("You can not drop this item!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    if (client.Player.DropItem((eInventorySlot)fromClientSlot))
                    {
                        client.Out.SendMessage("You drop " + item.GetName(0, false) + " on the ground!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
                    client.Out.SendInventoryItemsUpdate(null);
                    return;
                }


                if (item == null)
                {
                    client.Out.SendMessage("Invalid item (slot# " + fromClientSlot + ").", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendInventoryItemsUpdate(null);
                    return;
                }

                //Relics are no move allowed in Inventorys
                if (item != null && item.Id_nb == "GameRelic")
                {
                    client.Out.SendMessage("You move " + item.GetName(0, false) + " can't move in your inventory!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendInventoryItemsUpdate(null);
                    return;
                }

                /*
                if (item.Id_nb == "GameRelic" && (toClientSlot == (ushort)eInventorySlot.RightRearSaddleBag || toClientSlot == (ushort)eInventorySlot.RightFrontSaddleBag))
                {
                    client.Out.SendMessage("You move " + item.GetName(0, false) + " can't move in your sattle bag!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendInventoryItemsUpdate(null);
                }
                */



                    //We want to move the item in inventory
                    client.Player.Inventory.MoveItem((eInventorySlot)fromClientSlot, (eInventorySlot)toClientSlot, itemCount);
                return;
            }

            if (((fromClientSlot >= (ushort)eInventorySlot.Ground && fromClientSlot <= (ushort)eInventorySlot.LastBackpack)
                || (fromClientSlot >= (ushort)eInventorySlot.FirstVault && fromClientSlot <= (ushort)eInventorySlot.LastVault)
                || (fromClientSlot >= (ushort)eInventorySlot.FirstBagHorse && fromClientSlot <= (ushort)eInventorySlot.LastBagHorse))
                && ((toClientSlot == (ushort)eInventorySlot.PlayerPaperDoll || toClientSlot == (ushort)eInventorySlot.NewPlayerPaperDoll)
                || (toClientSlot >= (ushort)eInventorySlot.Ground && toClientSlot <= (ushort)eInventorySlot.LastBackpack)
                || (toClientSlot >= (ushort)eInventorySlot.FirstVault && toClientSlot <= (ushort)eInventorySlot.LastVault)
                || (toClientSlot >= (ushort)eInventorySlot.FirstBagHorse && toClientSlot <= (ushort)eInventorySlot.LastBagHorse)))
            {
                InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)fromClientSlot);
                if (item == null)
                {
                    return;
                }

                toClientSlot = 0;
                if (item.Item_Type >= (int)eInventorySlot.MinEquipable && item.Item_Type <= (int)eInventorySlot.MaxEquipable)
                {
                    toClientSlot = (ushort)item.Item_Type;
                }

                //Artifact Bracer
                if (client.Player.Inventory.GetItem(eInventorySlot.LeftBracer) != null && item.PackageID == "Artifact"
                        && item.Name == client.Player.Inventory.GetItem(eInventorySlot.LeftBracer).Name)
                {
                    client.Out.SendMessage("You can not use same type of Aritifacts!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    toClientSlot = 0;
                }
                if (client.Player.Inventory.GetItem(eInventorySlot.RightBracer) != null && item.PackageID == "Artifact"
                    && item.Name == client.Player.Inventory.GetItem(eInventorySlot.RightBracer).Name)
                {
                    client.Out.SendMessage("You can not use same type of Aritifacts!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    toClientSlot = 0;
                }
                //Artifact Ring
                if (client.Player.Inventory.GetItem(eInventorySlot.LeftRing) != null && item.PackageID == "Artifact"
                        && item.Name == client.Player.Inventory.GetItem(eInventorySlot.LeftRing).Name)
                {
                    client.Out.SendMessage("You can not use same type of Aritifacts!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    toClientSlot = 0;
                }
                if (client.Player.Inventory.GetItem(eInventorySlot.RightRing) != null && item.PackageID == "Artifact"
                    && item.Name == client.Player.Inventory.GetItem(eInventorySlot.RightRing).Name)
                {
                    client.Out.SendMessage("You can not use same type of Aritifacts!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    toClientSlot = 0;
                }
                //Artifact Weapon
                if (client.Player.Inventory.GetItem(eInventorySlot.LeftHandWeapon) != null && item.PackageID == "Artifact"
                       && item.Name == client.Player.Inventory.GetItem(eInventorySlot.LeftHandWeapon).Name)
                {
                    client.Out.SendMessage("You can not use same type of Aritifacts!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    toClientSlot = 0;
                }
                if (client.Player.Inventory.GetItem(eInventorySlot.RightHandWeapon) != null && item.PackageID == "Artifact"
                    && item.Name == client.Player.Inventory.GetItem(eInventorySlot.RightHandWeapon).Name)
                {
                    client.Out.SendMessage("You can not use same type of Aritifacts!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    toClientSlot = 0;
                }
                if (client.Player.Inventory.GetItem(eInventorySlot.TwoHandWeapon) != null && item.PackageID == "Artifact"
                    && item.Name == client.Player.Inventory.GetItem(eInventorySlot.TwoHandWeapon).Name)
                {
                    client.Out.SendMessage("You can not use same type of Aritifacts!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    toClientSlot = 0;
                }
                
                
                if (toClientSlot == 0)
                {
                    client.Out.SendInventorySlotsUpdate(new int[] { fromClientSlot });
                    return;
                }
                if (toClientSlot == (int)eInventorySlot.LeftBracer || toClientSlot == (int)eInventorySlot.RightBracer)
                {




                    if (client.Player.Inventory.GetItem(eInventorySlot.LeftBracer) == null)
                    {
                        toClientSlot = (int)eInventorySlot.LeftBracer;
                    }
                    else
                    {
                        toClientSlot = (int)eInventorySlot.RightBracer;
                    }
                }

                if (toClientSlot == (int)eInventorySlot.LeftRing || toClientSlot == (int)eInventorySlot.RightRing)
                {
                    if (client.Player.Inventory.GetItem(eInventorySlot.LeftRing) == null)
                    {
                        toClientSlot = (int)eInventorySlot.LeftRing;
                    }
                    else
                    {
                        toClientSlot = (int)eInventorySlot.RightRing;
                    }
                }

                client.Player.Inventory.MoveItem((eInventorySlot)fromClientSlot, (eInventorySlot)toClientSlot, itemCount);
                return;
            }

            client.Out.SendInventoryItemsUpdate(null);
        }
    }
}
    