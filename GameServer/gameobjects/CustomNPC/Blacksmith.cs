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
using System.Collections;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS
{
    [NPCGuildScript("Smith")]
    public class Blacksmith : GameNPC
    {
        private const string REPAIR_ITEM_WEAK = "repair item";

        /// <summary>
        /// Can accept any item
        /// </summary>
        public override bool CanTradeAnyItem
        {
            get { return true; }
        }


        #region Examine/Interact Message

        /// <summary>
        /// Adds messages to ArrayList which are sent when object is targeted
        /// </summary>
        /// <param name="player">GamePlayer that is examining this object</param>
        /// <returns>list with string messages</returns>
        public override IList GetExamineMessages(GamePlayer player)
        {
            IList list = new ArrayList();
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Blacksmith.YouTarget", GetName(0, false)));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Blacksmith.YouExamine", GetName(0, false), GetPronoun(0, true), GetAggroLevelString(player, false)));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Blacksmith.GiveObject", GetPronoun(0, true)));
            return list;
        }

        /// <summary>
        /// This function is called from the ObjectInteractRequestHandler
        /// </summary>
        /// <param name="player">GamePlayer that interacts with this object</param>
        /// <returns>false if interaction is prevented</returns>
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;

            TurnTo(player, 1000);

            SayTo(player, eChatLoc.CL_PopupWindow, LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Blacksmith.Say"));
            return true;
        }

        #endregion Examine/Interact Message

        #region Receive item

        public override bool ReceiveItem(GameLiving source, InventoryItem item)
        {
            GamePlayer player = source as GamePlayer;
            if (player == null || item == null)
                return false;

            if (item.Count != 1)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language,
                                                                    "Scripts.Blacksmith.StackedObjets", GetName(0, false)),
                                         eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return false;
            }

            switch (item.Object_Type)
            {
                case (int)eObjectType.GenericItem:
                // case (int)eObjectType.Magical:
                // case (int)eObjectType.Instrument:
                case (int)eObjectType.Poison:
                    SayTo(player, LanguageMgr.GetTranslation(player.Client.Account.Language,
                                                             "Scripts.Blacksmith.CantRepairThat"));

                    return false;
            }

            if (item.Condition < item.MaxCondition)
            {
                if (item.Durability <= 0)
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language,
                                                                       "Scripts.Blacksmith.ObjectCantRepaired"), eChatType.CT_System,
                                            eChatLoc.CL_SystemWindow);

                    return false;
                }
                else
                {
                    player.TempProperties.setProperty(REPAIR_ITEM_WEAK, new WeakRef(item));
                    player.Client.Out.SendCustomDialog(LanguageMgr.GetTranslation(player.Client.Account.Language,
                                                                                  "Scripts.Blacksmith.RepairCostAccept",
                                                                                  Money.GetString(item.RepairCost), item.Name),
                                                       new CustomDialogResponse(BlacksmithDialogResponse));
                }
            }
            else
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language,
                                                                  "Scripts.Blacksmith.NoNeedRepair"), eChatType.CT_System,
                                       eChatLoc.CL_SystemWindow);
            }

            return false;

        }

        protected void BlacksmithDialogResponse(GamePlayer player, byte response)
        {
            WeakReference itemWeak =
                (WeakReference)player.TempProperties.getProperty<object>(
                    REPAIR_ITEM_WEAK,
                    new WeakRef(null)
                );
            player.TempProperties.removeProperty(REPAIR_ITEM_WEAK);
            InventoryItem item = (InventoryItem)itemWeak.Target;

            if (response != 0x01)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Blacksmith.AbortRepair", item.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return;
            }


            if (item == null || item.SlotPosition == (int)eInventorySlot.Ground
                  || item.OwnerID == null || item.OwnerID != player.InternalID)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Blacksmith.InvalidItem"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return;
            }



            if (!player.RemoveMoney(item.RepairCost))
            {
                //InventoryLogging.LogInventoryAction(player, this, eInventoryActionType.Merchant, item.RepairCost);
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language,
                                                                  "Scripts.Blacksmith.NotEnoughMoney"), eChatType.CT_System,
                                       eChatLoc.CL_SystemWindow);

                return;
            }

            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Blacksmith.YouPay",
                                                              GetName(0, false), Money.GetString(item.RepairCost)), eChatType.CT_System,
                                   eChatLoc.CL_SystemWindow);

            // Items with IsNotLosingDur are not....losing DUR.
            int ToRecoverCond = 0;
            int ToRecoverCond2 = 0;
            int Condition = 0;
            if (item.ConditionPercent <= 5)
                Condition = 5;
            else
                Condition = item.ConditionPercent;


            ToRecoverCond = ((100 / Condition) * (item.Quality / 10));
            ToRecoverCond2 = ((100 / Condition) * (item.Quality / 15));

            if (item.Quality > 98 && item.Durability > 550)
            {


                if (item.RepearCount > 0)
                    item.RepearCount -= ToRecoverCond2;
                if (item.RepearCount < 0)
                    item.RepearCount = 0;
                if (item.RepearCount == 0)
                    item.Durability -= (ToRecoverCond2);
                if (item.RepearCount < 0)
                    item.RepearCount = 0;

                item.Condition = item.MaxCondition;
                player.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                SayTo(player, LanguageMgr.GetTranslation(player.Client,
                                                     "Scripts.Blacksmith.ItsDone", item.Name));
            }
            if (item.Durability > 499 && item.Durability < 550)
            {

                item.Durability -= (ToRecoverCond);
                item.Condition = item.MaxCondition;
                player.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                SayTo(player, LanguageMgr.GetTranslation(player.Client,
                "Scripts.Blacksmith.ObjectRatherOld", item.Name));
            }
            if (item.Quality > 95 && item.Quality < 99 && item.Durability > 550)
            {

                if (item.RepearCount > 0)
                    item.RepearCount -= (ToRecoverCond);
                if (item.RepearCount < 0)
                    item.RepearCount = 0;
                if (item.RepearCount == 0)
                    item.Durability -= (ToRecoverCond);
                if (item.RepearCount < 0)
                    item.RepearCount = 0;

                item.Condition = item.MaxCondition;
                player.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                SayTo(player, LanguageMgr.GetTranslation(player.Client,
                                                     "Scripts.Blacksmith.ItsDone", item.Name));
            }
            if (item.Durability > 499 && item.Durability < 550)
            {

                item.Durability -= (ToRecoverCond * 5);
                item.Condition = item.MaxCondition;
                player.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                SayTo(player, LanguageMgr.GetTranslation(player.Client,
                "Scripts.Blacksmith.ObjectRatherOld", item.Name));
            }
            if (item.Quality < 96 && item.Durability > 550)
            {


                if (item.RepearCount > 0)
                    item.RepearCount -= (ToRecoverCond * 3);
                if (item.RepearCount < 0)
                    item.RepearCount = 0;
                if (item.RepearCount == 0)
                    item.Durability -= (ToRecoverCond * 10);



                item.Condition = item.MaxCondition;
                player.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                SayTo(player, LanguageMgr.GetTranslation(player.Client,
                                                     "Scripts.Blacksmith.ItsDone", item.Name));
            }
            if (item.Durability > 499 && item.Durability < 550)
            {
                if (item.Durability > 0)
                    item.Durability -= (ToRecoverCond * 10);
                item.Condition = item.MaxCondition;
                player.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
                SayTo(player, LanguageMgr.GetTranslation(player.Client,
                "Scripts.Blacksmith.ObjectRatherOld", item.Name));
            }
            else

                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language,
                "Scripts.Blacksmith.CantRepairThat"), eChatType.CT_System,
                                      eChatLoc.CL_SystemWindow);

            return;
        }

        #endregion
    }
}

