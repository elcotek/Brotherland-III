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
using DOL.GS.PacketHandler;
using DOL.GS.ServerProperties;
using DOL.Language;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DOL.GS.Housing
{
    public class GameLotMarker : GameStaticItem
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private DBHouse m_dbitem;

        public GameLotMarker()
            : base()
        {
            SaveInDB = false;
        }

        public DBHouse DatabaseItem
        {
            get { return m_dbitem; }
            set { m_dbitem = value; }
        }


       


        public override IList GetExamineMessages(GamePlayer player)
        {
            string msg = "";
            string msg2 = "";
            int rentCheckInterval = Properties.RENT_CHECK_INTERVAL;
            if (String.IsNullOrEmpty(DatabaseItem.Name) == false)
            {
                 msg = " Remainig time before this lot will be clear and again aviable to buy: " + DatabaseItem.LastPaid.AddMinutes(Properties.RENT_CHECK_INTERVAL) + ".";
            }
            if (string.IsNullOrEmpty(DatabaseItem.OwnerID))
            {
                if (Properties.RENT_CHECK_INTERVAL <= 60)
                {
                    msg2 = " it will be only aviable to create a house for  " + Properties.RENT_CHECK_INTERVAL + " minutes." + " - After the remaining time, the Server will set this lot again for sell if no House was built!!";
                }
                else
                    msg2 = " it will be only aviable to create a house for  " + (Properties.RENT_CHECK_INTERVAL / 60) + "hours." + " - After the remaining time, the Server will set this lot again for sell if no House was built!!";
            
            }

                IList list = new ArrayList
            {
              
                "You target lot number " + DatabaseItem.HouseNumber + "." 
                + msg

            };

            if (string.IsNullOrEmpty(DatabaseItem.OwnerID))
            {
                list.Add(" It can be bought for " + Money.GetString(HouseTemplateMgr.GetLotPrice(DatabaseItem)) + "." + msg2);
                
            }
            else if (!string.IsNullOrEmpty(DatabaseItem.Name))
            {
                list.Add(" It is owned by " + DatabaseItem.Name + ".");
            }

            return list;
        }


       

        public static void AutomaticEmptyLotRemover()
        {

            foreach (DBHouse house in GameServer.Database.SelectAllObjects<DBHouse>())
            {
                if (string.IsNullOrEmpty(house.OwnerID) == false && house.Model == 0)
                {
                    log.InfoFormat("Remove empty lot from House nummer {0}", house.HouseNumber);


                    House ownedlot = HouseMgr.GetHouse(house.RegionID, house.HouseNumber);



                    HouseMgr.RemoveEmptyLotByTimer(ownedlot);
                  // RemoveHouse(ownedlot);

                   
                }
                
            }
        }
        public static int GetRealmByHousenumber(int houseNumber)
        {
            if (houseNumber >= 1 && houseNumber <= 1383)
            {
                return 1;
            }

            if (houseNumber >= 1601 && houseNumber <= 2574)
            {
                return 2;
            }

            if (houseNumber >= 3200 && houseNumber <= 4399)
            {
                return 3;
            }
            else
                return 0;
        }


        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
            {
                return false;
            }

            House house = HouseMgr.GetHouseByPlayer(player);

            if (house != null)
            {
                               
                //the player might be targeting a lot he already purchased that has no house on it yet
                if (house.HouseNumber != DatabaseItem.HouseNumber && player.Client.Account.PrivLevel != (int)ePrivLevel.Admin)
                {
                    string pRealm = "";

                    switch (player.Realm)
                    {
                        case eRealm.Albion:
                            {
                                pRealm = "Albion";
                                break;
                            }
                        case eRealm.Midgard:
                            {
                                pRealm = "Midgard";
                                break;
                            }
                        case eRealm.Hibernia:
                            {
                                pRealm = "Hibernia";
                                break;
                            }
                        case eRealm.None:
                            {
                                pRealm = "No Pleayer Realm Selected";
                                break;
                            }

                    }
                    ChatUtil.SendSystemMessage(player, "You already own a house or lot in " + pRealm + "!");
                    return false;
                }
            }

            if (string.IsNullOrEmpty(DatabaseItem.OwnerID))
            {

                switch (GetRealmByHousenumber(DatabaseItem.HouseNumber))
                {
                    case 1:
                        {
                            //IList<Account> lastAccountId = GameServer.Database.SelectObjects<Account>("AccountID = '" + playerAccount.AccountID + "'");
                            IList<Account> lastAccountId = GameServer.Database.SelectObjects<Account>("`Name` = @Name", new QueryParameter("@Name", player.DBCharacter.AccountName));
                            for (int i = 0; i < lastAccountId.Count; i++)
                            {
                                Account dba = lastAccountId[i];

                                if (dba != null && player.DBCharacter.AccountName == dba.Name && dba.HasHouseAlb == false)
                                {

                                    player.Out.SendCustomDialog("Do you want to buy this lot?\r\n It costs " + Money.GetString(HouseTemplateMgr.GetLotPrice(DatabaseItem)) + "!", BuyLot);
                                }
                                else
                                {
                                    ChatUtil.SendMerchantMessage(player, "Your Account already own another lot or house here in Albion!");
                                    return false;
                                }
                            }
                            break;
                        }

                    case 2:
                        {
                            //IList<Account> lastAccountId = GameServer.Database.SelectObjects<Account>("AccountID = '" + playerAccount.AccountID + "'");
                            IList<Account> lastAccountId = GameServer.Database.SelectObjects<Account>("`Name` = @Name", new QueryParameter("@Name", player.DBCharacter.AccountName));
                            for (int i = 0; i < lastAccountId.Count; i++)
                            {
                                Account dba = lastAccountId[i];

                                if (dba != null && player.DBCharacter.AccountName == dba.Name && dba.HasHouseMid == false)
                                {

                                    player.Out.SendCustomDialog("Do you want to buy this lot?\r\n It costs " + Money.GetString(HouseTemplateMgr.GetLotPrice(DatabaseItem)) + "!", BuyLot);
                                }
                                else
                                {
                                    ChatUtil.SendMerchantMessage(player, "Your Account already own another lot or house here in Midgard!");
                                    return false;
                                }
                            }
                            break;
                        }

                    case 3:
                        {
                            //IList<Account> lastAccountId = GameServer.Database.SelectObjects<Account>("AccountID = '" + playerAccount.AccountID + "'");
                            IList<Account> lastAccountId = GameServer.Database.SelectObjects<Account>("`Name` = @Name", new QueryParameter("@Name", player.DBCharacter.AccountName));
                            for (int i = 0; i < lastAccountId.Count; i++)
                            {
                                Account dba = lastAccountId[i];

                                if (dba != null && player.DBCharacter.AccountName == dba.Name && dba.HasHouseHib == false)
                                {

                                    player.Out.SendCustomDialog("Do you want to buy this lot?\r\n It costs " + Money.GetString(HouseTemplateMgr.GetLotPrice(DatabaseItem)) + "!", BuyLot);
                                }
                                else
                                {
                                    ChatUtil.SendMerchantMessage(player, "Your Account already own another lot or house here in Hibernia!");
                                    return false;
                                }
                            }
                            break;
                        }
                }

                
            }
            else
            {
                if (HouseMgr.IsOwner(DatabaseItem, player))
                {
                    player.Out.SendMerchantWindow(HouseTemplateMgr.GetLotMarkerItems(this), eMerchantWindowType.Normal);
                }
                else
                {
                    

                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Player.Housing.LotNotYours"), eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
                }
            }

            return true;
        }
      

        private void BuyLot(GamePlayer player, byte response)
        {
            if (response != 0x01)
            {
                return;
            }

            lock (DatabaseItem) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
            {
                if (!string.IsNullOrEmpty(DatabaseItem.OwnerID))
                    return;

                if (HouseMgr.GetHouseNumberByPlayer(player) != 0 && player.Client.Account.PrivLevel != (int)ePrivLevel.Admin)
                {
                    ChatUtil.SendMerchantMessage(player, "You already own another lot or house (Number " + HouseMgr.GetHouseNumberByPlayer(player) + ").");
                    return;
                }

                long totalCost = HouseTemplateMgr.GetLotPrice(DatabaseItem);
                if (player.RemoveMoney(totalCost, "You just bought this lot for {0}.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow))
                {

                    //IList<Account> lastAccountId = GameServer.Database.SelectObjects<Account>("AccountID = '" + playerAccount.AccountID + "'");
                    IList<Account> lastAccountId = GameServer.Database.SelectObjects<Account>("`Name` = @Name", new QueryParameter("@Name", player.DBCharacter.AccountName));
                    foreach (Account dba in lastAccountId)
                    {

                        switch (GetRealmByHousenumber(DatabaseItem.HouseNumber))
                        {
                            case 1:
                                {
                                    if (dba != null && player.DBCharacter.AccountName == dba.Name && dba.HasHouseAlb == false)
                                    {
                                        DatabaseItem.LastPaid = DateTime.Now;
                                        DatabaseItem.OwnerID = player.DBCharacter.ObjectId;
                                        CreateHouse(player, 0);

                                        {

                                            if (DatabaseItem.OwnerID == player.DBCharacter.ObjectId)
                                            {
                                                dba.HouseNumberAlb = DatabaseItem.HouseNumber;
                                                dba.HasHouseAlb = true;
                                                dba.AllowAdd = true;
                                            }
                                            GameServer.Database.SaveObject(dba);
                                        }
                                    }
                                }
                                break;
                            case 2:
                                {
                                    if (dba != null && player.DBCharacter.AccountName == dba.Name && dba.HasHouseMid == false)
                                    {
                                        DatabaseItem.LastPaid = DateTime.Now;
                                        DatabaseItem.OwnerID = player.DBCharacter.ObjectId;
                                        CreateHouse(player, 0);

                                        {

                                            if (DatabaseItem.OwnerID == player.DBCharacter.ObjectId)
                                            {
                                                dba.HouseNumberMid = DatabaseItem.HouseNumber;
                                                dba.HasHouseMid = true;
                                                dba.AllowAdd = true;
                                            }
                                            GameServer.Database.SaveObject(dba);
                                        }
                                    }
                                }
                                break;
                            case 3:
                                {
                                    if (dba != null && player.DBCharacter.AccountName == dba.Name && dba.HasHouseHib == false)
                                    {
                                        DatabaseItem.LastPaid = DateTime.Now;
                                        DatabaseItem.OwnerID = player.DBCharacter.ObjectId;
                                        CreateHouse(player, 0);

                                        {

                                            if (DatabaseItem.OwnerID == player.DBCharacter.ObjectId)
                                            {
                                                dba.HouseNumberHib = DatabaseItem.HouseNumber;
                                                dba.HasHouseHib = true;
                                                dba.AllowAdd = true;
                                            }
                                            GameServer.Database.SaveObject(dba);
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }

                else
                {
                    ChatUtil.SendMerchantMessage(player, "You dont have enough money!");
                }
            }
        } 
        

		public override bool ReceiveItem(GameLiving source, InventoryItem item)
		{
			if (source == null || item == null)
            {
                return false;
            }

            if (!(source is GamePlayer))
            {
                return false;
            }

            var player = (GamePlayer) source;

			if (HouseMgr.IsOwner(DatabaseItem, player))
			{
				switch (item.Id_nb)
				{
					case "housing_alb_cottage_deed":
						CreateHouse(player, 1);
						break;
					case "housing_alb_house_deed":
						CreateHouse(player, 2);
						break;
					case "housing_alb_villa_deed":
						CreateHouse(player, 3);
						break;
					case "housing_alb_mansion_deed":
						CreateHouse(player, 4);
						break;
					case "housing_mid_cottage_deed":
						CreateHouse(player, 5);
						break;
					case "housing_mid_house_deed":
						CreateHouse(player, 6);
						break;
					case "housing_mid_villa_deed":
						CreateHouse(player, 7);
						break;
					case "housing_mid_mansion_deed":
						CreateHouse(player, 8);
						break;
					case "housing_hib_cottage_deed":
						CreateHouse(player, 9);
						break;
					case "housing_hib_house_deed":
						CreateHouse(player, 10);
						break;
					case "housing_hib_villa_deed":
						CreateHouse(player, 11);
						break;
					case "housing_hib_mansion_deed":
						CreateHouse(player, 12);
						break;
                    default:
						return false;
				}

				player.Inventory.RemoveItem(item);

				// Tolakram:  Is this always null when purchasing a house?
				//InventoryLogging.LogInventoryAction(player, "(HOUSE;" + (CurrentHouse == null ? DatabaseItem.HouseNumber : CurrentHouse.HouseNumber) + ")", eInventoryActionType.Other, item.Template, item.Count);

				return true;
			}

			
            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Player.Housing.LotNotYours"), eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);

            return false;
		}

		private void CreateHouse(GamePlayer player, int model)
		{
			DatabaseItem.Model = model;
			DatabaseItem.Name = player.Name;

			if (player.Guild != null)
			{
				DatabaseItem.Emblem = player.Guild.Emblem;
			}

			var house = new House(DatabaseItem);
			HouseMgr.AddHouse(house);

			if (model != 0)
			{
				// move all players outside the mesh
				foreach (GamePlayer p in player.GetPlayersInRadius(500))
				{
					house.Exit(p, true);
				}

				RemoveFromWorld();
				Delete();
			}
		}

		public virtual bool OnPlayerBuy(GamePlayer player, int item_slot, int number)
		{
			GameMerchant.OnPlayerBuy(player, item_slot, number, HouseTemplateMgr.GetLotMarkerItems(this));
			return true;
		}

		public virtual bool OnPlayerSell(GamePlayer player, InventoryItem item)
		{
			if (!item.IsDropable)
			{
				ChatUtil.SendMerchantMessage(player, "This item can't be sold.");
				return false;
			}

			return true;
		}

		public long OnPlayerAppraise(GamePlayer player, InventoryItem item, bool silent)
		{
			if (item == null)
            {
                return 0;
            }

            int itemCount = Math.Max(1, item.Count);
			return item.Price * itemCount / 2;
		}

		public override void SaveIntoDatabase()
		{
			// do nothing !!!
		}

		public static void SpawnLotMarker(DBHouse house)
		{
            GameLotMarker obj = new GameLotMarker
			          	{
			          		X = house.X,
			          		Y = house.Y,
			          		Z = house.Z,
			          		CurrentRegionID = house.RegionID,
			          		Heading = (ushort) house.Heading,
			          		Name = "Lot Marker",
			          		Model = 1308,
			          		DatabaseItem = house
			          	};

			//No clue how we can check if a region
			//is in albion, midgard or hibernia instead
			//of checking the region id directly
			switch (obj.CurrentRegionID)
			{
				case 2:
					obj.Model = 1308;
					obj.Name = "Albion Lot";
					break; //ALB
				case 102:
					obj.Model = 1306;
					obj.Name = "Midgard Lot";
					break; //MID
				case 202:
					obj.Model = 1307;
					obj.Name = "Hibernia Lot";
					break; //HIB
			}

			obj.AddToWorld();
		}
	}
}