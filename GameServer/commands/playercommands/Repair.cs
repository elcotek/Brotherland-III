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
using DOL.Database;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
    [CmdAttribute(
        "&repair",
        ePrivLevel.Player,
        "You can repair an item when you are a crafter",
        "/repair")]
    public class RepairCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

      
        public void OnCommand(GameClient client, string[] args)
        {
            if (IsSpammingCommand(client.Player, "repair"))
                return;

            WorldInventoryItem item = client.Player.TargetObject as WorldInventoryItem;
            if (item != null)
            {
                client.Player.RepairItem(item.Item);
                return;
            }
            GameKeepDoor door = client.Player.TargetObject as GameKeepDoor;
            if (door != null)
            {
                if (!PreFireChecks(client.Player, door)) return;
                StartRepair(client.Player, door);
            }
            GameKeepComponent component = client.Player.TargetObject as GameKeepComponent;
            if (component != null)
            {

                if (!PreFireChecks(client.Player, component)) return;
                                
                StartRepair(client.Player, component);
            }
            GameSiegeWeapon weapon = client.Player.TargetObject as GameSiegeWeapon;
            if (weapon != null)
            {
                if (!PreFireChecks(client.Player, weapon)) return;
                StartRepair(client.Player, weapon);
            }
        }
        protected static int woodneed = 0;
        protected static string woodNemeneed = "wood";
        public bool PreFireChecks(GamePlayer player, GameLiving obj)
        {
            if (obj == null)
                return false;
            if (player.Realm != obj.Realm)
                return false;

            if (player.Client.Account.PrivLevel > (int)ePrivLevel.Player)
                return true;

            if ((obj as GameLiving).InCombat)
            {
                DisplayMessage(player, "You can't repair object while it is under attack!");
                return false;
            }

            if (obj is GameKeepComponent && obj as GameKeepComponent != null && (obj as GameKeepComponent).Keep.InCombat && ((obj as GameKeepComponent).Keep is GameKeepTower || (obj as GameKeepComponent).Keep is GameKeep))
            {
                GameKeepComponent keep = obj as GameKeepComponent;

                DisplayMessage(player, "You can't repair " + keep.Name + " while it is under attack!");
                return false;

            }

            if (obj is GameKeepDoor)
            {
                GameKeepDoor doorcomponent = obj as GameKeepDoor;
                if (doorcomponent.Component.Keep.InCombat)
                {
                    DisplayMessage(player, "You can't repair the keep door while keep is under attack!");
                    return false;
                }
            }

            if (obj is IKeepItem)
            {
                if (obj.CurrentRegion.Time - obj.LastAttackedByEnemyTick <= 60 * 1000)
                {
                    DisplayMessage(player, "You can't repair the keep component while it is under attack!");
                    return false;
                }
            }

            if ((obj as GameLiving).HealthPercent == 100)
            {
                DisplayMessage(player, "The component is already at full health!");
                return false;
            }
            /*
            if (obj is GameKeepComponent)
            {
                GameKeepComponent component = obj as GameKeepComponent;
               
                if (component.IsRaized)
                {
                    DisplayMessage(player, "You cannot repair a raized tower!");
                    return false;
                }
            }
            */

            if (obj is GameKeepComponent)
            {
                GameKeepComponent component = obj as GameKeepComponent;
                if (!player.IsWithinRadius(component, 360, true))
                {
                    DisplayMessage(player, "You are too far away to repair this component.");
                    return false;
                }
            }
            if (player.IsCrafting)
            {
                DisplayMessage(player, "You must end your current action before you repair anything!");
                return false;
            }

            if (player.IsMoving)
            {
                DisplayMessage(player, "You can't repair while moving");
                return false;
            }

            if (!player.IsAlive)
            {
                DisplayMessage(player, "You can't repair while dead.");
                return false;
            }

            if (player.IsSitting)
            {
                DisplayMessage(player, "You can't repair while sitting.");
                return false;
            }

            if (player.InCombat)
            {
                DisplayMessage(player, "You can't repair while in combat.");
                return false;
            }
            //log.Error("repariere!!");
            
            int playerswood = 0;

            if (obj is GameKeepComponent && obj as GameKeepComponent != null && ((obj as GameKeepComponent).Keep is GameKeepTower || (obj as GameKeepComponent).Keep is GameKeep))
            {
                GameKeepComponent keepcomp = obj as GameKeepComponent;
                AbstractGameKeep keep = keepcomp.Keep as AbstractGameKeep;
                woodneed = GetTotalWoodForLevel(keep.Level);
                //log.ErrorFormat("holz gebraucht: {0} keep level {1}", GetTotalWoodForLevel(keep.Level), keep.Level);
                player.TempProperties.setProperty("wood_needed", woodneed);
                playerswood = CalculatePlayersWoodForKeepRepair(player, 0);
            }
            else
            {
                //Doors and Siege Weapons
                woodneed = GetTotalWoodForLevel(obj.Level);
                //log.ErrorFormat("holz für tor!! gebraucht: {0} door level {1}", woodneed, obj.Level);

                player.TempProperties.setProperty("wood_needed", woodneed);
                playerswood = CalculatePlayersWoodForKeepRepair(player, 0);
            }


            if (playerswood < woodneed)
            {
                DisplayMessage(player, "You need another " + (woodneed - playerswood) + " units of wood!");
                return false;
            }

            player.TempProperties.setProperty("wood_name", woodNemeneed);

            DisplayMessage(player, "You use "+ woodneed + " units of " + woodNemeneed +" for this repair");
           
           

            if (player.GetCraftingSkillValue(eCraftingSkill.WoodWorking) < 1)
            {
                DisplayMessage(player, "You need woodworking skill to repair.");
                return false;
            }

            player.Stealth(false);

            return true;
        }


        /// <summary>
        /// Craft Timer
        /// </summary>
        static int workDuration = 20;
        public void StartRepair(GamePlayer player, GameLiving obj)
        {
            player.Out.SendTimerWindow("Repairing: " + obj.Name, workDuration);
            player.CraftTimer = new RegionTimer(player);
            player.CraftTimer.Callback = new RegionTimerCallback(Proceed);
            player.CraftTimer.Properties.setProperty("repair_player", player);
            player.CraftTimer.Properties.setProperty("repair_target", obj);
            player.CraftTimer.Start(workDuration * 1000);
        }
        protected static double healvalue = 0.15;
        protected static int healValueFromWoodLevel = 0;
        protected int Proceed(RegionTimer timer)
        {
            GamePlayer player = (GamePlayer)timer.Properties.getProperty<object>("repair_player", null);
            GameLiving obj = (GameLiving)timer.Properties.getProperty<object>("repair_target", null);
            
            if (player == null || obj == null)
            {
                if (log.IsWarnEnabled)
                    log.Warn("There was a problem getting back the target or player in door/component repair!");
                return 0;
            }

            player.CraftTimer.Stop();
            player.Out.SendCloseTimerWindow();

            if (!PreFireChecks(player, obj))
                return 0;

            if (Util.ChanceDouble(CalculateRepairChance(player, obj)))
            {
                int start = obj.HealthPercent;
                       
                //Keeep Walls
                if (obj is GameKeepComponent && obj as GameKeepComponent != null && ((obj as GameKeepComponent).Keep is GameKeepTower || (obj as GameKeepComponent).Keep is GameKeep))
                {
                    GameKeepComponent keepcomp = obj as GameKeepComponent;
                    AbstractGameKeep keep = keepcomp.Keep;

                    int woodneed = GetTotalWoodForLevel(keep.Level);
                   
                    CalculatePlayersWoodForKeepRepair(player, woodneed);
                    healvalue = (double)(healValueFromWoodLevel * 0.01);
                    keepcomp.Repair((int)(keepcomp.MaxHealth * (healvalue + 0.04)));

                    DisplayMessage(player, "You successfully repair the component by " + (healvalue + 0.04) * 100 + "%!");
                }
                else
                {
                    //Keep Doors
                    if (obj is GameKeepDoor)
                    {
                        GameKeepDoor door = obj as GameKeepDoor;
                        string woodNametoremove = player.TempProperties.getProperty<string>("wood_name");

                        healvalue = GetWoodValueForKeepRepair(woodNametoremove);

                        CalculatePlayersWoodForKeepRepair(player, (GetTotalWoodForLevel(obj.Level)));
                        healvalue = (double)(healValueFromWoodLevel * 0.01);
                        door.Repair((int)(door.MaxHealth * (healvalue + 0.04)));
                                           
                        DisplayMessage(player, "You successfully repair the component by " + (healvalue + 0.04) * 100 + "%!");
                    }
                    else
                    {

                        if (obj is GameSiegeWeapon)
                        {
                            GameSiegeWeapon weapon = obj as GameSiegeWeapon;
                            //weapon.Repair();

                            if (player.GetCraftingSkillValue(eCraftingSkill.WoodWorking) < 301)
                            {
                                player.Out.SendMessage("You must have woodworking skill 300+ to repair the "+weapon.Name+".", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                                return 0;
                            }
                            string woodNametoremove = player.TempProperties.getProperty<string>("wood_name");

                            healvalue = GetWoodValueForKeepRepair(woodNametoremove);

                            CalculatePlayersWoodForKeepRepair(player, (GetTotalWoodForLevel(obj.Level)));
                            healvalue = (double)(healValueFromWoodLevel * 0.01);
                            //log.ErrorFormat("Ramme: healvalue:{0}", healvalue);

                           
                            weapon.Health += (int)(weapon.MaxHealth * (healvalue + 0.04));
                            DisplayMessage(player, "You successfully repair the component by " + (healvalue + 0.04) * 100 + "%!");
                        }
                        else
                        {
                            if (player.GetCraftingSkillValue(eCraftingSkill.WoodWorking) < 301)
                            {
                                player.Out.SendMessage("You cannot repair " + obj.Name + ".", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                                return 0;
                            }
                        }
                    }
                }
                int finish = obj.HealthPercent;

                /*
				 * - Realm points will now be awarded for successfully repairing a door or outpost piece.
				 * Players will receive approximately 10% of the amount repaired in realm points.
				 * (Note that realm points for repairing a door or outpost piece will not work in the battlegrounds.)
				 */
                // tolakram - we have no idea how many hit points a live door has so this code is not accurate
                int amount = (finish - start) * obj.Level;  // level of non claimed keep is 4
                //log.ErrorFormat("rps amount2: {0}", amount);
                player.GainRealmPoints(Math.Min(150, amount));
            }
            else
            {
                DisplayMessage(player, "You fail to repair the component!");
            }

            return 0;
        }
       

        public static double CalculateRepairChance(GamePlayer player, GameObject obj)
        {
            if (player.Client.Account.PrivLevel > (int)ePrivLevel.Player)
                return 100;

            double skill = player.GetCraftingSkillValue(eCraftingSkill.WoodWorking);
            int skillneeded = (obj.Level + 1) * 50;
            double chance = skill / skillneeded;
            return chance;
        }

        public static int GetTotalWoodForLevel(int level)
        {
            switch (level)
            {
                case 0: return 1;
                case 1: return 2;
                case 2: return 20;
                case 3: return 30;
                case 4: return 60;
                case 5: return 70;
                case 6: return 90;
                case 7: return 120;
                case 8: return 180;
                case 9: return 220;
                case 10: return 300;
                default: return 0;
            }
        }
        static string[] WoodNames = { "rowan", "elm", "oak", "oaken", "ironwood", "heartwood", "runewood", "stonewood", "ebonwood", "dyrwood", "duskwood", "mysticwood" };


       
        /*
        public static int CalculatePlayersWood(GamePlayer player, int removeamount)
        {
            int amount = 0;
            foreach (InventoryItem item in player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
            {
                foreach (string name in WoodNames)
                {
                    if (item.Name.Replace(" wooden boards", "").ToLower() == name)
                    {
                        int woodvalue = GetWoodValue(item.Name.ToLower());
                        

                        amount += item.Count * woodvalue;
                       

                        if (removeamount > 0)
                        {
                           
                            if (item.Count * woodvalue < removeamount)
                            {
                                
                                int removecount = Math.Min(1, removeamount / woodvalue);
                                removeamount -= removecount * woodvalue;
                                player.Inventory.RemoveCountFromStack(item, removecount);
                            }
                            else
                            {
                                removeamount -= item.Count * woodvalue;
                                player.Inventory.RemoveItem(item);
                            }
                        }
                        break;
                    }
                }
            }
            return amount;
        }
        */

        public static int CalculatePlayersWoodForKeepRepair(GamePlayer player, int removeamount)
        {
            int amount = 0;
            foreach (InventoryItem item in player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
            {
                foreach (string name in WoodNames)
                {
                   
                    if (item.Name.Replace(" wooden boards", "").ToLower() == name)
                    {
                        
                        int woodvalue = GetWoodValueForKeepRepair(name);
                        string woodname = GetWoodNameForKeepLevel(woodvalue);
                        
                        if (item.Count < woodneed)
                        {
                            if (item.Count < woodneed)
                            {
                                
                                log.ErrorFormat("You need more {0} !!!!", woodname);
                            }
                                
                        }
                        else
                        {
                            if (item.Count >= woodneed)
                            {
                                log.ErrorFormat("Holz ist genug vorhanden {0}, menge: {1}", woodname, item.Count);
                                woodNemeneed = item.Name;
                            }
                        }
                        if (item.Name == woodNemeneed && item.Count >= woodneed)
                        {
                            string woodNametoremove = "";
                               // log.ErrorFormat("benutze: {0}", item.Name);

                            woodNametoremove = player.TempProperties.getProperty<string>("wood_name");
                                                       
                            healValueFromWoodLevel = woodvalue;
                            amount += item.Count;// * woodvalue;

                            if (removeamount > 0)
                            {

                                if ((item.Count * woodvalue) > removeamount)
                                {
                                    int woodtoremove = player.TempProperties.getProperty<int>("wood_needed");


                                    int removecount = Math.Min(1, removeamount / woodvalue);
                                    removeamount -= removecount * woodvalue;

                                    if (item.Name == woodNametoremove)
                                    {
                                        player.Inventory.RemoveCountFromStack(item, woodtoremove);
                                        player.TempProperties.removeProperty("wood_needed");
                                        player.TempProperties.removeProperty("wood_name");
                                    }
                                }
                                else
                                {
                                    if (item.Name == woodNametoremove)
                                    {
                                        removeamount -= item.Count * woodvalue;
                                        player.Inventory.RemoveItem(item);
                                        player.TempProperties.removeProperty("wood_needed");
                                        player.TempProperties.removeProperty("wood_name");
                                    }
                                }
                                woodNemeneed = "wooden";
                            }
                        }
                        break;
                    }
                }
            }
            return amount;
        }
        public static int GetWoodValue(string name)
        {
            switch (name.Replace(" wooden boards", ""))
            {
                case "rowan": return 1;
                case "elm": return 4;
                case "oak": return 8;
                case "ironwood": return 16;
                case "heartwood": return 32;
                case "runewood": return 48;
                case "stonewood": return 60;
                case "ebonwood": return 80;
                case "dyrwood": return 104;
                case "duskwood": return 136;
                case "mysticwood": return 100;
                default: return 0;
            }
        }
        public static int GetWoodValueForKeepRepair(string name)
        {
            switch (name.Replace(" wooden boards", ""))
            {
                case "rowan": return 1;
                case "elm": return 2;
                case "oak": return 3;
                case "ironwood": return 4;
                case "heartwood": return 5;
                case "runewood": return 6;
                case "stonewood": return 7;
                case "ebonwood": return 8;
                case "dyrwood": return 9;
                case "duskwood": return 10;
                case "mysticwood": return 11;
                default: return 0;
            }
        }
        public static string GetWoodNameForKeepLevel(int value)
        {
            switch (value)
            {
                case 1: return "rowan wooden boards";
                case 2: return "elm wooden boards";
                case 3: return "oak wooden boards";
                case 4: return "ironwood wooden boards";
                case 5: return "heartwood wooden boards";
                case 6: return "runewood wooden boards";
                case 7: return "stonewood wooden boards";
                case 8: return "ebonwood wooden boards";
                case 9: return "dyrwood wooden boards";
                case 10: return "duskwood wooden boards";
                case 11: return "mysticwood wooden boards";
                default: return "";
            }
        }
    }
}