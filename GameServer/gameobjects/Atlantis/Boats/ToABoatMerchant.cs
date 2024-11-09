//---------------------------------------------------------
//-------------------Toa Boat Merchant --------------------
//-------------------Author : Hibernos---------------------
//---------------------Fixed and Updated by Elcotek------------------------------------

using DOL.GS.PacketHandler;


namespace DOL.GS.ToaMgr.CustomNpc
{

    //Toa Boat merchant
    public class ToaBoatMerchant : GameNPC
    {
        //Log - Debug
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static bool debug = true;
        public override bool AddToWorld()
        {
            Level = 50;
            base.AddToWorld();
            return true;
        }
        //Realm Regions 
        public static int albregion = 73;
        public static int midregion = 30;
        public static int hibregion = 130;

        //Realm Available
        public static bool Albion = true;
        public static bool Midgard = true;
        public static bool Hibernia = true;

        //Override
      //  public override void SaveIntoDatabase()
      //  {
     //   }
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player)) return false;
            //TurnTo(player.X,player.Y);
            player.Out.SendMessage("Hello " + player.Name + "! Would you like a[Scout Boat][Warship] [Galleon] [Skiff] [Stygian Ship] [Atlantean Ship], or a [Viking Longship]?" +
            " [Atlantean Ship], [British Cog]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            return true;
        }
        public override bool WhisperReceive(GameLiving player, string str)
        {
            GamePlayer t = (GamePlayer)player;
            GameBoat boat = new GameBoat();
            GameBoat curBoat = BoatMgr.GetBoatByOwner(player.InternalID);


            switch (str)
            {
                case "Scout Boat":
                    {
                        #region Scout Boat

                        if (curBoat == null)
                        {

                            boat.BoatID = System.Guid.NewGuid().ToString();
                            boat.MaxSpeedBase = 270;
                            boat.Model = 2648;
                            boat.Name = t.Name + "'s scout boat";
                            //boat.OwnerID = t.InternalID;////////////////////////teste1!!!!!!!!!!!!!!!
                            if (BoatMgr.DoesBoatExist(boat.Name) == true)
                            {
                                t.Out.SendMessage("It seems that you already own a boat , use /boat delete before you buy a new boat ! .", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            }
                            if (BoatMgr.DoesBoatExist(boat.Name) == false)
                            {

                                BoatMgr.CreateBoat(t, boat);
                                //  if (ToaConfig.BoatMerchantMoney == 1) t.RemoveMoney(ToaConfig.BoatMerchantSmallBoatPrice * 100 * 100);
                                //  if (ToaConfig.BoatMerchantMoney == 2) t.BountyPoints = t.BountyPoints - ToaConfig.BoatMerchantSmallBoatPrice;
                                t.Out.SendMessage("Congratulations, now you have a Skiff! Use /boat to using it!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                if (debug == true) log.Warn("NPC - BoatMerchant -¨" + t.Name + " bought a skiff !");
                            }
                        }
                        else
                        {
                            t.Out.SendMessage("It seems that you already own a boat , use /boat summon before you buy a new boat ! .", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        }

                        #endregion Scout Boat
                    }
                    break;

                case "Skiff":
                    {
                        #region Skiff

                        if (curBoat == null)
                        {
                            boat.BoatID = System.Guid.NewGuid().ToString();
                            boat.MaxSpeedBase = 270;
                            boat.Model = 1616;
                            boat.Name = t.Name + "'s skiff";

                            if (BoatMgr.DoesBoatExist(boat.Name) == true)
                            {
                                t.Out.SendMessage("It seems that you already own a boat , use /boat delete before you buy a new boat ! .", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            }
                            if (BoatMgr.DoesBoatExist(boat.Name) == false)
                            {
                                BoatMgr.CreateBoat(t, boat);
                                //  if (ToaConfig.BoatMerchantMoney == 1) t.RemoveMoney(ToaConfig.BoatMerchantSmallBoatPrice * 100 * 100);
                                //  if (ToaConfig.BoatMerchantMoney == 2) t.BountyPoints = t.BountyPoints - ToaConfig.BoatMerchantSmallBoatPrice;
                                t.Out.SendMessage("Congratulations, now you have a Skiff! Use /boat to using it!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                if (debug == true) log.Warn("NPC - BoatMerchant -¨" + t.Name + " bought a skiff !");

                            }
                            else
                            {
                                t.Out.SendMessage("It seems that you already own a boat , use /boat summon before you buy a new boat ! .", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            }
                        }

                        #endregion Skiff
                    }
                    break;


                case "Warship":
                    {
                        #region Warship

                        if (curBoat == null)
                        {
                            boat.BoatID = System.Guid.NewGuid().ToString();
                            boat.MaxSpeedBase = 270;
                            boat.Model = 2647;
                            boat.Name = "Boat of " + t.Name;

                            if (BoatMgr.DoesBoatExist(boat.Name) == true)
                            {
                                t.Out.SendMessage("It seems that you already own a boat , use /boat delete before you buy a new boat ! .", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            }
                            if (BoatMgr.DoesBoatExist(boat.Name) == false)
                            {
                                BoatMgr.CreateBoat(t, boat);
                                //  if (ToaConfig.BoatMerchantMoney == 1) t.RemoveMoney(ToaConfig.BoatMerchantSmallBoatPrice * 100 * 100);
                                //  if (ToaConfig.BoatMerchantMoney == 2) t.BountyPoints = t.BountyPoints - ToaConfig.BoatMerchantSmallBoatPrice;
                                t.Out.SendMessage("Congratulations, now you have a Skiff! Use /boat to using it!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                if (debug == true) log.Warn("NPC - BoatMerchant -¨" + t.Name + " bought a skiff !");
                            }
                            else
                            {
                                t.Out.SendMessage("It seems that you already own a boat , use /boat summon before you buy a new boat ! .", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            }
                        }
                        #endregion Warship
                    }
                    break;

                case "Galleon":
                    {
                        #region Galeon

                        if (curBoat == null)
                        {
                            boat.BoatID = System.Guid.NewGuid().ToString();
                            boat.MaxSpeedBase = 270;
                            boat.Model = 2646;
                            boat.Name = t.Name + "'s galleon";

                            if (BoatMgr.DoesBoatExist(boat.Name) == true)
                            {
                                t.Out.SendMessage("It seems that you already own a boat , use /boat delete before you buy a new boat ! .", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            }
                            if (BoatMgr.DoesBoatExist(boat.Name) == false)
                            {
                                BoatMgr.CreateBoat(t, boat);
                                //  if (ToaConfig.BoatMerchantMoney == 1) t.RemoveMoney(ToaConfig.BoatMerchantSmallBoatPrice * 100 * 100);
                                //  if (ToaConfig.BoatMerchantMoney == 2) t.BountyPoints = t.BountyPoints - ToaConfig.BoatMerchantSmallBoatPrice;
                                t.Out.SendMessage("Congratulations, now you have a Skiff! Use /boat to using it!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                if (debug == true) log.Warn("NPC - BoatMerchant -¨" + t.Name + " bought a skiff !");
                            }
                            else
                            {
                                t.Out.SendMessage("It seems that you already own a boat , use /boat summon before you buy a new boat ! .", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            }
                        }
                        #endregion Galeon
                    }
                    break;

                case "Viking Longship":
                    {
                        #region Viking Longship

                        if (curBoat == null)
                        {
                            boat.BoatID = System.Guid.NewGuid().ToString();
                            boat.MaxSpeedBase = 400;
                            boat.Model = 1615;
                            boat.Name = t.Name + "'s Viking longship";

                            if (BoatMgr.DoesBoatExist(boat.Name) == true)
                            {
                                t.Out.SendMessage("It seems that you already own a boat , use /boat delete before  you buy a new boat ! .", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            }
                            if (BoatMgr.DoesBoatExist(boat.Name) == false)
                            {
                                BoatMgr.CreateBoat(t, boat);
                                // if (ToaConfig.BoatMerchantMoney == 1) t.RemoveMoney(ToaConfig.BoatMerchantBigBoatPrice * 100 * 100);
                                // if (ToaConfig.BoatMerchantMoney == 2) t.BountyPoints = t.BountyPoints - ToaConfig.BoatMerchantBigBoatPrice;
                                t.Out.SendMessage("Congratulations, now you have a Frigate! Use /boat to using it!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                if (debug == true) log.Warn("NPC - BoatMerchant -¨" + t.Name + " bought a frigate !");
                            }
                            else
                            {
                                t.Out.SendMessage("It seems that you already own a boat , use /boat summon before you buy a new boat ! .", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            }
                        }

                        #endregion Viking Longship
                    }
                    break;


                case "Stygian Ship":
                    {
                        #region Stygian Ship

                        if (curBoat == null)
                        {
                            boat.BoatID = System.Guid.NewGuid().ToString();
                            boat.MaxSpeedBase = 400;
                            boat.Model = 1612;
                            boat.Name = "Boat of " + t.Name;

                            if (BoatMgr.DoesBoatExist(boat.Name) == true)
                            {
                                t.Out.SendMessage("It seems that you already own a boat , use /boat delete before you buy a new boat ! .", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            }
                            if (BoatMgr.DoesBoatExist(boat.Name) == false)
                            {
                                BoatMgr.CreateBoat(t, boat);
                                // if (ToaConfig.BoatMerchantMoney == 1) t.RemoveMoney(ToaConfig.BoatMerchantBigBoatPrice * 100 * 100);
                                // if (ToaConfig.BoatMerchantMoney == 2) t.BountyPoints = t.BountyPoints - ToaConfig.BoatMerchantBigBoatPrice;
                                t.Out.SendMessage("Congratulations, now you have a Frigate! Use /boat to using it!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                if (debug == true) log.Warn("NPC - BoatMerchant -¨" + t.Name + " bought a frigate !");
                            }
                            else
                            {
                                t.Out.SendMessage("It seems that you already own a boat , use /boat summon before you buy a new boat ! .", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            }
                        }


                        #endregion Stygian Ship
                    }
                    break;

                case "Atlantean Ship":
                    {
                        #region Atlantean Ship

                        if (curBoat == null)
                        {
                            boat.BoatID = System.Guid.NewGuid().ToString();
                            boat.MaxSpeedBase = 400;
                            boat.Model = 1613;
                            boat.Name = "Boat of " + t.Name;

                            if (BoatMgr.DoesBoatExist(boat.Name) == true)
                            {
                                t.Out.SendMessage("It seems that you already own a boat , use /boat delete before you buy a new boat ! .", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            }
                            if (BoatMgr.DoesBoatExist(boat.Name) == false)
                            {
                                BoatMgr.CreateBoat(t, boat);
                                // if (ToaConfig.BoatMerchantMoney == 1) t.RemoveMoney(ToaConfig.BoatMerchantBigBoatPrice * 100 * 100);
                                // if (ToaConfig.BoatMerchantMoney == 2) t.BountyPoints = t.BountyPoints - ToaConfig.BoatMerchantBigBoatPrice;
                                t.Out.SendMessage("Congratulations, now you have a Frigate! Use /boat to using it!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                if (debug == true) log.Warn("NPC - BoatMerchant -¨" + t.Name + " bought a frigate !");
                            }
                            else
                            {
                                t.Out.SendMessage("It seems that you already own a boat , use /boat summon before you buy a new boat ! .", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            }
                        }

                        #endregion Atlantean Ship
                    }
                    break;
                case "British Cog":
                    {
                        #region British Cog

                        if (curBoat == null)
                        {
                            boat.BoatID = System.Guid.NewGuid().ToString();
                            boat.MaxSpeedBase = 400;
                            boat.Model = 1614;
                            boat.Name = t.Name + "'s British Cog";

                            if (BoatMgr.DoesBoatExist(boat.Name) == true)
                            {
                                t.Out.SendMessage("It seems that you already own a boat , use /boat delete before you buy a new boat ! .", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            }
                            if (BoatMgr.DoesBoatExist(boat.Name) == false)
                            {
                                BoatMgr.CreateBoat(t, boat);
                                // if (ToaConfig.BoatMerchantMoney == 1) t.RemoveMoney(ToaConfig.BoatMerchantBigBoatPrice * 100 * 100);
                                // if (ToaConfig.BoatMerchantMoney == 2) t.BountyPoints = t.BountyPoints - ToaConfig.BoatMerchantBigBoatPrice;
                                t.Out.SendMessage("Congratulations, now you have a Frigate! Use /boat to using it!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                if (debug == true) log.Warn("NPC - BoatMerchant -¨" + t.Name + " bought a frigate !");
                            }
                            else
                            {
                                t.Out.SendMessage("It seems that you already own a boat , use /boat summon before you buy a new boat ! .", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            }
                        }

                        #endregion British Cog
                    }
                    break;


            }
            return true;
        }

    }
}

