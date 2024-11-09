using System;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;

using log4net;

namespace DOL.GS.Commands
{
    /// <summary>
    /// command handler for /boat command
    /// </summary>
    [Cmd(
        "&boat",
        new string[] { "&boatcommand" },
        ePrivLevel.Player,
        "Boat command (use /boat for options)",
        "/boat <option>")]
    public class BoatCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        bool boatFound = false;

        /// <summary>
        /// Names for deleting boats
        /// </summary>
        /// <returns>the Name of the boat</returns> 
        public string ShipName = null;
        /// <returns>the Name of the boat for the load command</returns> 
        public string LoadShipName = null;
        /// <summary>
        /// method to handle /boat commands from a client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public void OnCommand(GameClient client, string[] args)
        {
            if (IsSpammingCommand(client.Player, "boat"))
                return;

            if (client.Player.InCombat)
            {
                client.Player.Out.SendMessage("/boat commands don't work while in combat!.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                //client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.NotOwnBoat"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                return;
            }

            try
            {
                switch (args[1])
                {

                    case "load":
                        {
                            /*
                            if (!client.Player.IsSwimming)
                            {
                                // Not in water
                                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.NotInWater"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            */
                            // Check to see if player has boat
                            int boatFound = 0;
                            GameBoat curBoat = BoatMgr.GetBoatByOwner(client.Player.InternalID);
                            if (curBoat != null)
                            {
                                if (curBoat.BoatOwner == client.Player.InternalID)
                                {
                                    boatFound = 1;
                                    //Console.Out.WriteLine("boot gefunden {0}", boatFound);
                                }
                                else
                                    curBoat = null;


                                if (curBoat == null)
                                {
                                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.NotOwnBoat"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                    return;
                                }
                               
                            }
                            if (boatFound == 1)
                            {

                                //GameBoat playerBoat = BoatMgr.GetBoatByOwner(client.Player.InternalID);
                                if (curBoat.BoatOwner == client.Player.InternalID)
                                {
                                    switch (curBoat.Model)
                                    {
                                        case 2648:
                                            LoadShipName = "scout_boat";
                                            break;
                                        case 1616:
                                            LoadShipName = "Skiff";
                                            break;
                                        case 2647:
                                            LoadShipName = "Warship";
                                            break;
                                        case 2646:
                                            LoadShipName = "Galleon";
                                            break;
                                        case 1615:
                                            LoadShipName = "Viking_Longship";
                                            break;
                                        case 1614:
                                            LoadShipName = "British_Cog";
                                            break;
                                    }
                                }


                                if (client.Player.Inventory.GetFirstItemByID(LoadShipName, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) == null)
                                {

                                    client.Player.ReceiveItem(curBoat, LoadShipName, 0);

                                    client.Player.UpdatePlayerStatus();
                                }

                                curBoat.RemoveFromWorld();
                                BoatMgr.DeleteBoat(curBoat.Name);

                            }



                            else
                            {
                                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.NotOwnBoat"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            }
                            break;
                        }
                    case "buy":
                        {
                            GamePlayer t = (GamePlayer)client.Player;
                            GameBoat boat = new GameBoat();
                            GameBoat curBoat = BoatMgr.GetBoatByOwner(client.Player.InternalID);
                            InventoryItem item = client.Player.Inventory.GetFirstItemByID("scout_boat", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);


                            if (curBoat == null)
                            {
                                boat.BoatID = System.Guid.NewGuid().ToString();

                                boat.MaxSpeedBase = 270;
                                boat.Model = 1616;
                                boat.Name = t.Name + "'s skiff";

                                BoatMgr.CreateBoat(t, boat);
                                {
                                    //t.Out.SendMessage("use now /boat load for your new boat ! .", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                    t.Out.SendMessage(LanguageMgr.GetTranslation(t.Client.Account.Language, "Behaviour.DropItemAction.LoadForYourNewBoat"), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                }


                            }
                            else
                            {
                                t.Out.SendMessage(LanguageMgr.GetTranslation(t.Client.Account.Language, "Behaviour.DropItemAction.YouAlreadyOwnABoat"), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                                //t.Out.SendMessage("It seems that you already own a boat , use /boat load before you buy a new boat ! .", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            }



                            break;
                        }
                    case "summonDisabled":
                        {
                            if (GameRelic.IsPlayerCarryingRelic(client.Player))
                            {

                                //client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "you cant use a boat with a Relic!"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.YouCantUseWithRelics"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                break;
                            }
                            if (!client.Player.IsSwimming)
                            {
                                // Not in water
                                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.NotInWater"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            GamePlayer t = (GamePlayer)client.Player;
                            GameBoat boat = new GameBoat();
                            GameBoat curBoat = BoatMgr.GetBoatByOwner(client.Player.InternalID);



                            if (curBoat != null)
                            {
                                log.Warn("  Boote von =" + curBoat.BoatOwner + "  von owner vorhanden");

                                if (curBoat.BoatOwner == client.Player.InternalID)
                                {
                                    boatFound = true;

                                    log.Warn(" boot vorhanden =   " + boatFound + "  Boote von =" + curBoat + "  owner");
                                }

                                else
                                    curBoat = null;
                            }
                            else
                                curBoat = null;

                            if (curBoat == null && boatFound != true)
                            {
                                if (GameBoat.PlayerHasItem(client.Player, "scout_boat"))
                                {
                                    GameBoat playerBoat = new GameBoat();
                                    InventoryItem item = client.Player.Inventory.GetFirstItemByID("scout_boat", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                    playerBoat.BoatID = System.Guid.NewGuid().ToString();
                                    playerBoat.Name = client.Player.Name + "'s scout boat";
                                    playerBoat.X = client.Player.X;
                                    playerBoat.Y = client.Player.Y;
                                    playerBoat.Z = client.Player.Z;
                                    playerBoat.Model = 2648;
                                    playerBoat.Heading = client.Player.Heading;
                                    playerBoat.Realm = client.Player.Realm;
                                    playerBoat.CurrentRegionID = client.Player.CurrentRegionID;
                                    playerBoat.BoatOwner = client.Player.InternalID;
                                    playerBoat.MaxSpeedBase = 500;
                                    client.Player.Inventory.RemoveItem(item);
                                    //InventoryLogging.LogInventoryAction(client.Player, "(ground)", eInventoryActionType.Other, item.Template, item.Count);
                                    playerBoat.Riders = new GamePlayer[8];
                                    BlankBrain brain = new BlankBrain();
                                    playerBoat.SetOwnBrain(brain);
                                    playerBoat = BoatMgr.CreateBoat(client.Player, playerBoat);
                                    if (client.Player.Guild != null)
                                    {
                                        if (client.Player.Guild.Emblem != 0)
                                            playerBoat.Emblem = (ushort)client.Player.Guild.Emblem;

                                        playerBoat.GuildName = client.Player.Guild.Name;
                                    }
                                    playerBoat.AddToWorld();
                                    client.Player.MountSteed(playerBoat, true);
                                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.Summoned", playerBoat.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                }
                                else if (GameBoat.PlayerHasItem(client.Player, "warship"))
                                {
                                    GameBoat playerBoat = new GameBoat();
                                    InventoryItem item = client.Player.Inventory.GetFirstItemByID("warship", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                    playerBoat.BoatID = System.Guid.NewGuid().ToString();
                                    playerBoat.Name = client.Player.Name + "'s warship";
                                    playerBoat.X = client.Player.X;
                                    playerBoat.Y = client.Player.Y;
                                    playerBoat.Z = client.Player.Z;
                                    playerBoat.Model = 2647;
                                    playerBoat.Heading = client.Player.Heading;
                                    playerBoat.Realm = client.Player.Realm;
                                    playerBoat.CurrentRegionID = client.Player.CurrentRegionID;
                                    playerBoat.BoatOwner = client.Player.InternalID;
                                    playerBoat.MaxSpeedBase = 400;
                                    client.Player.Inventory.RemoveItem(item);
                                    //InventoryLogging.LogInventoryAction(client.Player, "(ground)", eInventoryActionType.Other, item.Template, item.Count);
                                    playerBoat.Riders = new GamePlayer[32];
                                    BlankBrain brain = new BlankBrain();
                                    playerBoat.SetOwnBrain(brain);
                                    playerBoat = BoatMgr.CreateBoat(client.Player, playerBoat);
                                    if (client.Player.Guild != null)
                                    {
                                        if (client.Player.Guild.Emblem != 0)
                                            playerBoat.Emblem = (ushort)client.Player.Guild.Emblem;

                                        playerBoat.GuildName = client.Player.Guild.Name;
                                    }
                                    playerBoat.AddToWorld();
                                    client.Player.MountSteed(playerBoat, true);
                                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.Summoned", playerBoat.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                }
                                else if (GameBoat.PlayerHasItem(client.Player, "galleon"))
                                {
                                    GameBoat playerBoat = new GameBoat();
                                    InventoryItem item = client.Player.Inventory.GetFirstItemByID("galleon", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                    playerBoat.BoatID = System.Guid.NewGuid().ToString();
                                    playerBoat.Name = client.Player.Name + "'s galleon";
                                    playerBoat.X = client.Player.X;
                                    playerBoat.Y = client.Player.Y;
                                    playerBoat.Z = client.Player.Z;
                                    playerBoat.Model = 2646;
                                    playerBoat.Heading = client.Player.Heading;
                                    playerBoat.Realm = client.Player.Realm;
                                    playerBoat.CurrentRegionID = client.Player.CurrentRegionID;
                                    playerBoat.BoatOwner = client.Player.InternalID;
                                    playerBoat.MaxSpeedBase = 300;
                                    client.Player.Inventory.RemoveItem(item);
                                    //InventoryLogging.LogInventoryAction(client.Player, "(ground)", eInventoryActionType.Other, item.Template, item.Count);
                                    playerBoat.Riders = new GamePlayer[16];
                                    BlankBrain brain = new BlankBrain();
                                    playerBoat.SetOwnBrain(brain);
                                    playerBoat = BoatMgr.CreateBoat(client.Player, playerBoat);
                                    if (client.Player.Guild != null)
                                    {
                                        if (client.Player.Guild.Emblem != 0)
                                            playerBoat.Emblem = (ushort)client.Player.Guild.Emblem;

                                        playerBoat.GuildName = client.Player.Guild.Name;
                                    }
                                    playerBoat.AddToWorld();
                                    client.Player.MountSteed(playerBoat, true);
                                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.Summoned", playerBoat.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                }
                                else if (GameBoat.PlayerHasItem(client.Player, "skiff"))
                                {
                                    GameBoat playerBoat = new GameBoat();
                                    InventoryItem item = client.Player.Inventory.GetFirstItemByID("skiff", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                    playerBoat.BoatID = System.Guid.NewGuid().ToString();
                                    playerBoat.Name = client.Player.Name + "'s skiff";
                                    playerBoat.X = client.Player.X;
                                    playerBoat.Y = client.Player.Y;
                                    playerBoat.Z = client.Player.Z;
                                    playerBoat.Model = 1616;
                                    playerBoat.Heading = client.Player.Heading;
                                    playerBoat.Realm = client.Player.Realm;
                                    playerBoat.CurrentRegionID = client.Player.CurrentRegionID;
                                    playerBoat.BoatOwner = client.Player.InternalID;
                                    playerBoat.MaxSpeedBase = 250;
                                    client.Player.Inventory.RemoveItem(item);
                                    //InventoryLogging.LogInventoryAction(client.Player, "(ground)", eInventoryActionType.Other, item.Template, item.Count);
                                    playerBoat.Riders = new GamePlayer[8];
                                    BlankBrain brain = new BlankBrain();
                                    playerBoat.SetOwnBrain(brain);
                                    playerBoat = BoatMgr.CreateBoat(client.Player, playerBoat);
                                    if (client.Player.Guild != null)
                                    {
                                        if (client.Player.Guild.Emblem != 0)
                                            playerBoat.Emblem = (ushort)client.Player.Guild.Emblem;

                                        playerBoat.GuildName = client.Player.Guild.Name;
                                    }
                                    playerBoat.AddToWorld();
                                    client.Player.MountSteed(playerBoat, true);
                                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.Summoned", playerBoat.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                }
                                else if (GameBoat.PlayerHasItem(client.Player, "Viking_Longship"))
                                {
                                    GameBoat playerBoat = new GameBoat();
                                    InventoryItem item = client.Player.Inventory.GetFirstItemByID("Viking_Longship", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                    playerBoat.BoatID = System.Guid.NewGuid().ToString();
                                    playerBoat.Name = client.Player.Name + "'s Viking longship";
                                    playerBoat.X = client.Player.X;
                                    playerBoat.Y = client.Player.Y;
                                    playerBoat.Z = client.Player.Z;
                                    playerBoat.Model = 1615;
                                    playerBoat.Heading = client.Player.Heading;
                                    playerBoat.Realm = client.Player.Realm;
                                    playerBoat.CurrentRegionID = client.Player.CurrentRegionID;
                                    playerBoat.BoatOwner = client.Player.InternalID;
                                    playerBoat.MaxSpeedBase = 500;
                                    client.Player.Inventory.RemoveItem(item);
                                    // InventoryLogging.LogInventoryAction(client.Player, "(ground)", eInventoryActionType.Other, item.Template, item.Count);
                                    playerBoat.Riders = new GamePlayer[32];
                                    BlankBrain brain = new BlankBrain();
                                    playerBoat.SetOwnBrain(brain);
                                    playerBoat = BoatMgr.CreateBoat(client.Player, playerBoat);
                                    if (client.Player.Guild != null)
                                    {
                                        if (client.Player.Guild.Emblem != 0)
                                            playerBoat.Emblem = (ushort)client.Player.Guild.Emblem;

                                        playerBoat.GuildName = client.Player.Guild.Name;
                                    }
                                    playerBoat.AddToWorld();
                                    client.Player.MountSteed(playerBoat, true);
                                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.Summoned", playerBoat.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                }
                                else if (GameBoat.PlayerHasItem(client.Player, "ps_longship"))
                                {
                                    GameBoat playerBoat = new GameBoat();
                                    InventoryItem item = client.Player.Inventory.GetFirstItemByID("ps_longship", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                    playerBoat.BoatID = System.Guid.NewGuid().ToString();
                                    playerBoat.Name = client.Player.Name + "'s Longship";
                                    playerBoat.X = client.Player.X;
                                    playerBoat.Y = client.Player.Y;
                                    playerBoat.Z = client.Player.Z;
                                    playerBoat.Model = 1595;
                                    playerBoat.Heading = client.Player.Heading;
                                    playerBoat.Realm = client.Player.Realm;
                                    playerBoat.CurrentRegionID = client.Player.CurrentRegionID;
                                    playerBoat.BoatOwner = client.Player.InternalID;
                                    playerBoat.MaxSpeedBase = 600;
                                    client.Player.Inventory.RemoveItem(item);
                                    // InventoryLogging.LogInventoryAction(client.Player, "(ground)", eInventoryActionType.Other, item.Template, item.Count);
                                    playerBoat.Riders = new GamePlayer[31];
                                    BlankBrain brain = new BlankBrain();
                                    playerBoat.SetOwnBrain(brain);
                                    playerBoat = BoatMgr.CreateBoat(client.Player, playerBoat);
                                    if (client.Player.Guild != null)
                                    {
                                        if (client.Player.Guild.Emblem != 0)
                                            playerBoat.Emblem = (ushort)client.Player.Guild.Emblem;

                                        playerBoat.GuildName = client.Player.Guild.Name;
                                    }
                                    playerBoat.AddToWorld();
                                    client.Player.MountSteed(playerBoat, true);
                                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.Summoned", playerBoat.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                }
                                else if (GameBoat.PlayerHasItem(client.Player, "stygian_ship"))
                                {
                                    GameBoat playerBoat = new GameBoat();
                                    InventoryItem item = client.Player.Inventory.GetFirstItemByID("stygian_ship", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                    playerBoat.BoatID = System.Guid.NewGuid().ToString();
                                    playerBoat.Name = client.Player.Name + "'s Stygian ship";
                                    playerBoat.X = client.Player.X;
                                    playerBoat.Y = client.Player.Y;
                                    playerBoat.Z = client.Player.Z;
                                    playerBoat.Model = 1612;
                                    playerBoat.Heading = client.Player.Heading;
                                    playerBoat.Realm = client.Player.Realm;
                                    playerBoat.CurrentRegionID = client.Player.CurrentRegionID;
                                    playerBoat.BoatOwner = client.Player.InternalID;
                                    playerBoat.MaxSpeedBase = 500;
                                    client.Player.Inventory.RemoveItem(item);
                                    //InventoryLogging.LogInventoryAction(client.Player, "(ground)", eInventoryActionType.Other, item.Template, item.Count);
                                    playerBoat.Riders = new GamePlayer[24];
                                    BlankBrain brain = new BlankBrain();
                                    playerBoat.SetOwnBrain(brain);
                                    playerBoat = BoatMgr.CreateBoat(client.Player, playerBoat);
                                    if (client.Player.Guild != null)
                                    {
                                        if (client.Player.Guild.Emblem != 0)
                                            playerBoat.Emblem = (ushort)client.Player.Guild.Emblem;

                                        playerBoat.GuildName = client.Player.Guild.Name;
                                    }
                                    playerBoat.AddToWorld();
                                    client.Player.MountSteed(playerBoat, true);
                                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.Summoned", playerBoat.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                }
                                else if (GameBoat.PlayerHasItem(client.Player, "atlantean_ship"))
                                {
                                    GameBoat playerBoat = new GameBoat();
                                    InventoryItem item = client.Player.Inventory.GetFirstItemByID("atlantean_ship", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                    playerBoat.BoatID = System.Guid.NewGuid().ToString();
                                    playerBoat.Name = client.Player.Name + "'s Atlantean ship";
                                    playerBoat.X = client.Player.X;
                                    playerBoat.Y = client.Player.Y;
                                    playerBoat.Z = client.Player.Z;
                                    playerBoat.Model = 1613;
                                    playerBoat.Heading = client.Player.Heading;
                                    playerBoat.Realm = client.Player.Realm;
                                    playerBoat.CurrentRegionID = client.Player.CurrentRegionID;
                                    playerBoat.BoatOwner = client.Player.InternalID;
                                    playerBoat.MaxSpeedBase = 800;
                                    client.Player.Inventory.RemoveItem(item);
                                    //InventoryLogging.LogInventoryAction(client.Player, "(ground)", eInventoryActionType.Other, item.Template, item.Count);
                                    playerBoat.Riders = new GamePlayer[64];
                                    BlankBrain brain = new BlankBrain();
                                    playerBoat.SetOwnBrain(brain);
                                    playerBoat = BoatMgr.CreateBoat(client.Player, playerBoat);
                                    if (client.Player.Guild != null)
                                    {
                                        if (client.Player.Guild.Emblem != 0)
                                            playerBoat.Emblem = (ushort)client.Player.Guild.Emblem;

                                        playerBoat.GuildName = client.Player.Guild.Name;
                                    }
                                    playerBoat.AddToWorld();
                                    client.Player.MountSteed(playerBoat, true);
                                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.Summoned", playerBoat.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                }
                                else if (GameBoat.PlayerHasItem(client.Player, "British_Cog"))
                                {
                                    GameBoat playerBoat = new GameBoat();
                                    InventoryItem item = client.Player.Inventory.GetFirstItemByID("British_Cog", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);
                                    playerBoat.BoatID = System.Guid.NewGuid().ToString();
                                    playerBoat.Name = client.Player.Name + "'s British Cog";
                                    playerBoat.X = client.Player.X;
                                    playerBoat.Y = client.Player.Y;
                                    playerBoat.Z = client.Player.Z;
                                    playerBoat.Model = 1614;
                                    playerBoat.Heading = client.Player.Heading;
                                    playerBoat.Realm = client.Player.Realm;
                                    playerBoat.CurrentRegionID = client.Player.CurrentRegionID;
                                    playerBoat.BoatOwner = client.Player.InternalID;
                                    playerBoat.MaxSpeedBase = 700;
                                    client.Player.Inventory.RemoveItem(item);
                                    //InventoryLogging.LogInventoryAction(client.Player, "(ground)", eInventoryActionType.Other, item.Template, item.Count);
                                    playerBoat.Riders = new GamePlayer[33];
                                    BlankBrain brain = new BlankBrain();
                                    playerBoat.SetOwnBrain(brain);
                                    playerBoat = BoatMgr.CreateBoat(client.Player, playerBoat);
                                    if (client.Player.Guild != null)
                                    {
                                        if (client.Player.Guild.Emblem != 0)
                                            playerBoat.Emblem = (ushort)client.Player.Guild.Emblem;

                                        playerBoat.GuildName = client.Player.Guild.Name;
                                    }
                                    playerBoat.AddToWorld();
                                    client.Player.MountSteed(playerBoat, true);
                                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.Summoned", playerBoat.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                }
                                else
                                {
                                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.NotOwnBoat"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                    return;
                                }
                                BoatMgr.SaveAllBoats();
                            }
                            else if (boatFound == true)
                            {
                                if (client.Player.Guild != null)
                                {
                                    if (client.Player.Guild.Emblem != 0)
                                        curBoat.Emblem = (ushort)client.Player.Guild.Emblem;

                                    curBoat.GuildName = client.Player.Guild.Name;
                                }

                                curBoat.X = client.Player.X;
                                curBoat.Y = client.Player.Y;
                                curBoat.Z = client.Player.Z;
                                curBoat.Heading = client.Player.Heading;
                                curBoat.Realm = client.Player.Realm;
                                curBoat.CurrentRegionID = client.Player.CurrentRegionID;
                                curBoat.Riders = new GamePlayer[32];
                                BlankBrain brain = new BlankBrain();
                                curBoat.SetOwnBrain(brain);
                                curBoat.AddToWorld();
                                client.Player.MountSteed(curBoat, true);
                                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.Summoned", curBoat.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            }
                            else
                            {
                                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.NotOwnBoat"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            }
                            break;
                        }

                    case "board":
                        {
                            GameBoat playerBoat = BoatMgr.GetBoatByName(client.Player.TargetObject.Name);
                            if (GameRelic.IsPlayerCarryingRelic(client.Player))
                            {
                                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.YouCantUseWithRelics"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                break;
                            }

                            if (client.Player.TargetObject == null)
                            {
                                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.NoBoatSelected"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                break;
                            }
                            if (client.Player.IsWithinRadius(playerBoat, 500) == false)
                            {
                                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.YouMustBeNearBoat", playerBoat.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                break;
                            }
                            if (playerBoat.MAX_PASSENGERS > 1)
                            {
                                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.YouBoard", playerBoat.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Player.MountSteed(playerBoat, true);
                            }
                            else
                            {
                                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.FullBoat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            break;
                        }
                    case "follow":
                        {
                            GameBoat targetBoat = BoatMgr.GetBoatByName(client.Player.TargetObject.Name);

                            if (client.Player.Steed.OwnerID == client.Player.InternalID)// needs to be player on own boat
                            {
                                if (client.Player.TargetObject == null)
                                {
                                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.NoBoatSelected"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                    break;
                                }

                                client.Player.Steed.Follow(targetBoat, 800, 5000);
                                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.MoveFollow", client.Player.TargetObject.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            }
                            else
                            {
                                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.NotOwnBoat"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            }
                            break;
                        }
                    case "stopfollow":
                        {
                            if (client.Player.Steed.OwnerID == client.Player.InternalID)// needs to be player on own boat
                            {
                                client.Player.Steed.StopFollowing();
                                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.StopFollow"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            }
                            else
                            {
                                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.NotOwnBoat"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            }
                            break;
                        }
                    case "invite":
                        {
                            break;
                        }
                    case "unsummon":
                        {
                            if (client.Player.TargetObject == null)
                            {
                                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.NoBoatSelected"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                break;
                            }
                            GameBoat playerBoat = BoatMgr.GetBoatByName(client.Player.TargetObject.Name);
                            switch (client.Player.TargetObject.Model)
                            {
                                case 2648:
                                    ShipName = "scout_boat";
                                    break;
                                case 1616:
                                    ShipName = "Skiff";
                                    break;
                                case 2647:
                                    ShipName = "Warship";
                                    break;
                                case 2646:
                                    ShipName = "Galleon";
                                    break;
                                case 1615:
                                    ShipName = "Viking_Longship";
                                    break;
                                case 1614:
                                    ShipName = "British_Cog";
                                    break;
                            }
                            if (client.Player.InternalID == playerBoat.BoatOwner)
                                client.Player.Out.SendCustomDialog(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.TakeBackConfirmation", playerBoat.Name), new CustomDialogResponse(TakeBackConfirmation));
                            else
                                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.NotOwnBoat"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);

                            break;
                        }
                    case "boot":
                        {
                            GameBoat playerBoat = BoatMgr.GetBoatByOwner(client.Player.InternalID);

                            if (client.Player.InternalID == playerBoat.BoatOwner)
                            {
                                if (client.Player.TargetObject == null)
                                {
                                    // no player selected
                                    break;
                                }

                                GamePlayer target = (client.Player.TargetObject as GamePlayer);
                                if (playerBoat.RiderSlot(target) != -1)
                                {
                                    target.DismountSteed(true);
                                    target.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.BootedBy", client.Player.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.BootedTarget", target.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                }
                                else
                                {
                                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.TargetNotInBoat", target.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                }
                            }
                            else
                            {
                                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.NotOwnBoat"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            }
                            break;
                        }
                    default:
                        {
                            client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.UnknownCommand"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            DisplayHelp(client);
                        }
                        break;
                }
            }
            catch (Exception)
            {
                DisplayHelp(client);
            }
        }
        public void DisplayHelp(GameClient client)
        {
            client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.Usage"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
            client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.Help.Load"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
            client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.Help.Unsummon"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
            client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.Help.Follow"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
            client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.Help.StopFollow"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
            client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.Help.Board"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
            client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Player.Boat.Help.Boot"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);

        }

        //Holt das Boot wieder ins Inventory
        protected void TakeBackConfirmation(GamePlayer player, byte response)
        {
            if (response != 0x01) return;

            GameBoat playerBoat = BoatMgr.GetBoatByOwner(player.InternalID);
            if (player.Inventory.GetFirstItemByID(ShipName, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) == null)
            {
                player.ReceiveItem(playerBoat, ShipName, 0);

                player.UpdatePlayerStatus();
            }

            playerBoat.RemoveFromWorld();
            BoatMgr.DeleteBoat(playerBoat.Name);
        }
    }
}
