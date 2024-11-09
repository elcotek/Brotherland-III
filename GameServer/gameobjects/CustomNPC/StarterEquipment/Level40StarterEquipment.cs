/*
 *  Script updated and corrected by Elcotek
 * 
 */

using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using System;
using System.Reflection;


namespace DOL.GS.Scripts
{
    public class Level40StarterEquipment : GameNPC
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        #region Declarations

        private static ItemTemplate free_chain_vest40 = null; //Free Chain Vest
        private static ItemTemplate free_chain_arms40 = null; //Free Chain Arms
        private static ItemTemplate free_chain_legs40 = null; //Free Chain Legs
        private static ItemTemplate free_chain_helm40 = null; //Free Chain Helm
        private static ItemTemplate free_chain_gloves40 = null; //Free Chain Gloves
        private static ItemTemplate free_chain_boots40 = null; //Free Chain Boots

        private static ItemTemplate free_cloth_vest40 = null; //Free Cloth Vest
        private static ItemTemplate free_cloth_arms40 = null; //Free Cloth Arms
        private static ItemTemplate free_cloth_legs40 = null; //Free Cloth Legs
        private static ItemTemplate free_cloth_helm40 = null; //Free Cloth Helm
        private static ItemTemplate free_cloth_gloves40 = null; //Free Cloth Gloves
        private static ItemTemplate free_cloth_boots40 = null; //Free Cloth Boots

        private static ItemTemplate free_leather_vest40 = null; //Free Leather Vest
        private static ItemTemplate free_leather_arms40 = null; //Free Leather Arms
        private static ItemTemplate free_leather_legs40 = null; //Free Leather Legs
        private static ItemTemplate free_leather_helm40 = null; //Free Leather Helm
        private static ItemTemplate free_leather_gloves40 = null; //Free Leather Gloves
        private static ItemTemplate free_leather_boots40 = null; //Free Leather Boots

        private static ItemTemplate free_studded_vest40 = null; //Free Studded Vest
        private static ItemTemplate free_studded_arms40 = null; //Free Studded Arms
        private static ItemTemplate free_studded_legs40 = null; //Free Studded Legs
        private static ItemTemplate free_studded_helm40 = null; //Free Studded Helm
        private static ItemTemplate free_studded_gloves40 = null; //Free Studded Gloves
        private static ItemTemplate free_studded_boots40 = null; //Free Studded Boots

        private static ItemTemplate free_plate_vest40 = null; //Free Plate Vest
        private static ItemTemplate free_plate_arms40 = null; //Free Plate Arms
        private static ItemTemplate free_plate_legs40 = null; //Free Plate Legs
        private static ItemTemplate free_plate_helm40 = null; //Free Plate Helm
        private static ItemTemplate free_plate_gloves40 = null; //Free Plate Gloves
        private static ItemTemplate free_plate_boots40 = null; //Free Plate Boots

        //weapons
        private static ItemTemplate free_staff40 = null; //Free Caster Staff
        private static ItemTemplate free_slash_alb40 = null; //Free Slash
        private static ItemTemplate free_slash_mid40 = null; //Free Slash
        private static ItemTemplate free_slash_hib40 = null; //Free Slash
        private static ItemTemplate free_thrust_alb40 = null; //Free Thrust
        private static ItemTemplate free_thrust_hib40 = null; //Free Thrust
        private static ItemTemplate free_crush_alb40 = null; //Free Crush
        private static ItemTemplate free_crush_mid40 = null; //Free Crush
        private static ItemTemplate free_crush_hib40 = null; //Free Crush
        private static ItemTemplate free_axe40 = null; //Free Axe
        private static ItemTemplate free_long_bow40 = null; //Free Long Bow
        private static ItemTemplate free_composite_bow40 = null; //Free Composite Bow
        private static ItemTemplate free_recurved_bow40 = null; //Free Recurved Bow
        private static ItemTemplate free_short_bow40 = null; //Free Short Bow
        private static ItemTemplate free_crossbow40 = null; //Free Crossbow
        private static ItemTemplate free_twohanded_slash_alb40 = null; //Free TwoHanded Slash
        private static ItemTemplate free_twohanded_slash_hib40 = null; //Free TwoHanded Slash
        private static ItemTemplate free_twohanded_crush_alb40 = null; //Free TwoHanded Crush
        private static ItemTemplate free_twohanded_crush_hib40 = null; //Free TwoHanded Crush
        private static ItemTemplate free_twohanded_axe40 = null; //Free TwoHanded Axe
        private static ItemTemplate free_twohanded_sword40 = null; //Free TwoHanded Sword
        private static ItemTemplate free_twohanded_hammer40 = null; //Free TwoHanded Hammer
        private static ItemTemplate free_small_shield40 = null; //Free Small Shield
        private static ItemTemplate free_medium_shield40 = null; //Free Medium Shield
        private static ItemTemplate free_large_shield40 = null; //Free Large Shield
        private static ItemTemplate free_slash_spear40 = null; //Free Slash Spear
        private static ItemTemplate free_thrust_spear40 = null; //Free Thrust Spear
        private static ItemTemplate free_slash_flail40 = null; //Free Slash Flail
        private static ItemTemplate free_thrust_flail40 = null; //Free Thrust Flail
        private static ItemTemplate free_crush_flail40 = null; //Free Crush Flail
        private static ItemTemplate free_slash_claw40 = null; //Free Slash Claw
        private static ItemTemplate free_thrust_claw40 = null; //Free Thrust Claw
        private static ItemTemplate free_harp40 = null; //Free Harp
        private static ItemTemplate free_mauler_staff40 = null; //Free Mauler Staff
        private static ItemTemplate free_mauler_right_wrap40 = null;      //Free Mauler Fist Wrap
        private static ItemTemplate free_mauler_left_wrap40 = null; 		//Free Mauler Fist Wrap
        private static ItemTemplate free_scythe40 = null; //Free Scythe
        private static ItemTemplate free_crush_polearm40 = null; //Free Crush Polearm
        private static ItemTemplate free_thrust_polearm40 = null; //Free Thrust Polearm
        private static ItemTemplate free_slash_polearm40 = null; //Free Slash Polearm



        #endregion Declarations

        public override bool AddToWorld()
        {

            Level = 55;
            //Name = "Retho Tohmson";
            GuildName = "Free Level 40 Armor and Weapons";
            //Model = 1903;
            base.AddToWorld();
            this.Flags = eFlags.PEACE;
            return true;
        }
        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            if (log.IsInfoEnabled)
                log.Info("Free Level 40 Stuff NPC Initializing...");



            #region Armor
            ItemTemplate item = null;
            item = GameServer.Database.FindObjectByKey<ItemTemplate>("free_chain_vest40");
            if (item == null)
                #region Free Chain Armor

                //Free Chain Vest
                item = new ItemTemplate();
            item.Id_nb = "free_chain_vest40";
            item.Name = "Chain Vest";
            item.Level = 40;
            item.Durability = 50000;
            item.MaxDurability = 50000;
            item.Condition = 50000;
            item.MaxCondition = 50000;
            item.Quality = 95;
            item.DPS_AF = 80;
            item.SPD_ABS = 27;
            item.Object_Type = 35;
            item.Item_Type = 25;
            item.Weight = 50;
            item.Model = 776;
            item.IsPickable = true;
            item.CraftPriceSet = false;
            item.IsDropable = false;
            item.CanDropAsLoot = false;
            item.IsTradable = false;
            item.IsIndestructible = false;
            item.MaxCount = 1;
            item.PackSize = 1;
            item.AllowAdd = true;
            free_chain_vest40 = item;

            //Free Chain Arms
            item = new ItemTemplate
            {
                Id_nb = "free_chain_arms40",
                Name = "Chain Arms",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 27,
                Object_Type = 35,
                Item_Type = 28,
                Weight = 25,
                Model = 778,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_chain_arms40 = item;

            //Free Chain Legs
            item = new ItemTemplate
            {
                Id_nb = "free_chain_legs40",
                Name = "Chain Legs",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 27,
                Object_Type = 35,
                Item_Type = 27,
                Weight = 50,
                Model = 777,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_chain_legs40 = item;


            //Free Chain Boots
            item = new ItemTemplate
            {
                Id_nb = "free_chain_boots40",
                Name = "Chain Boots",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 27,
                Object_Type = 35,
                Item_Type = 23,
                Weight = 30,
                Model = 780,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_chain_boots40 = item;


            //Free Chain Gloves
            item = new ItemTemplate
            {
                Id_nb = "free_chain_gloves40",
                Name = "Chain Gloves",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 27,
                Object_Type = 35,
                Item_Type = 22,
                Weight = 32,
                Model = 779,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_chain_gloves40 = item;


            //Free Chain Helm
            item = new ItemTemplate
            {
                Id_nb = "free_chain_helm40",
                Name = "Chain Helm",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 27,
                Object_Type = 35,
                Item_Type = 21,
                Weight = 32,
                Model = 1291,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_chain_helm40 = item;

            #endregion Free Chain Armor
            #region Free Cloth Armor

            //Free Cloth Vest
            item = new ItemTemplate
            {
                Id_nb = "free_cloth_vest40",
                Name = "Cloth Vest",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 51,
                SPD_ABS = 0,
                Object_Type = 32,
                Item_Type = 25,
                Weight = 20,
                Model = 2171,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_cloth_vest40 = item;

            //Free Cloth Arms
            item = new ItemTemplate
            {
                Id_nb = "free_cloth_arms40",
                Name = "Free Cloth Arms",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 51,
                SPD_ABS = 0,
                Object_Type = 32,
                Item_Type = 28,
                Weight = 10,
                Model = 2161,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_cloth_arms40 = item;

            //Free Cloth Legs
            item = new ItemTemplate
            {
                Id_nb = "free_cloth_legs40",
                Name = "Cloth Legs",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 51,
                SPD_ABS = 0,
                Object_Type = 32,
                Item_Type = 27,
                Weight = 14,
                Model = 2167,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_cloth_legs40 = item;


            //Free Cloth Boots
            item = new ItemTemplate
            {
                Id_nb = "free_cloth_boots40",
                Name = "Cloth Boots",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 51,
                SPD_ABS = 0,
                Object_Type = 32,
                Item_Type = 23,
                Weight = 8,
                Model = 2166,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_cloth_boots40 = item;


            //Free Cloth Gloves
            item = new ItemTemplate
            {
                Id_nb = "free_cloth_gloves40",
                Name = "Cloth Gloves",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 51,
                SPD_ABS = 0,
                Object_Type = 32,
                Item_Type = 22,
                Weight = 6,
                Model = 2235,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_cloth_gloves40 = item;


            //Free Cloth Helm
            item = new ItemTemplate
            {
                Id_nb = "free_cloth_helm40",
                Name = "Cloth Helm",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 51,
                SPD_ABS = 0,
                Object_Type = 32,
                Item_Type = 21,
                Weight = 8,
                Model = 1279,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_cloth_helm40 = item;

            #endregion Free Cloth Armor
            #region Free Leather Armor

            //Free Leather Vest
            item = new ItemTemplate
            {
                Id_nb = "free_leather_vest40",
                Name = "Leather Vest",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 10,
                Object_Type = 33,
                Item_Type = 25,
                Weight = 30,
                Model = 1267,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true,
                AllowedClasses = "3;9;10;23;25;43;48;49;50;58;60;61;62"
            };
            free_leather_vest40 = item;

            //Free Leather Arms
            item = new ItemTemplate
            {
                Id_nb = "free_leather_arms40",
                Name = "Leather Arms",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 10,
                Object_Type = 33,
                Item_Type = 28,
                Weight = 24,
                Model = 1269,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true,
                AllowedClasses = "3;9;10;23;25;43;48;49;50;58;60;61;62"
            };
            free_leather_arms40 = item;

            //Free Leather Legs
            item = new ItemTemplate
            {
                Id_nb = "free_leather_legs40",
                Name = "Leather Legs",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 10,
                Object_Type = 33,
                Item_Type = 27,
                Weight = 30,
                Model = 1268,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true,
                AllowedClasses = "3;9;10;23;25;43;48;49;50;58;60;61;62"
            };
            free_leather_legs40 = item;


            //Free Leather Boots
            item = new ItemTemplate
            {
                Id_nb = "free_leather_boots40",
                Name = "Leather Boots",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 10,
                Object_Type = 33,
                Item_Type = 23,
                Weight = 16,
                Model = 1270,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true,
                AllowedClasses = "3;9;10;23;25;43;48;49;50;58;60;61;62"
            };
            free_leather_boots40 = item;


            //Free Leather Gloves
            item = new ItemTemplate
            {
                Id_nb = "free_leather_gloves40",
                Name = "Leather Gloves",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 10,
                Object_Type = 33,
                Item_Type = 22,
                Weight = 6,
                Model = 1271,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true,
                AllowedClasses = "3;9;10;23;25;43;48;49;50;58;60;61;62"
            };
            free_leather_gloves40 = item;


            //Free Leather Helm
            item = new ItemTemplate
            {
                Id_nb = "free_leather_helm40",
                Name = "Leather Helm",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 10,
                Object_Type = 33,
                Item_Type = 21,
                Weight = 8,
                Model = 1291,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true,
                AllowedClasses = "3;9;10;23;25;43;48;49;50;58;60;61;62"
            };
            free_leather_helm40 = item;

            #endregion Free Leather Armor
            #region Free Studded Armor

            //Free Studded Vest
            item = new ItemTemplate
            {
                Id_nb = "free_studded_vest40",
                Name = "Studded Vest",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 19,
                Object_Type = 34,
                Item_Type = 25,
                Weight = 40,
                Model = 1192,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true,
                AllowedClasses = "3;48;43;50;31;25;32"
            };
            free_studded_vest40 = item;

            //Free Studded Arms
            item = new ItemTemplate
            {
                Id_nb = "free_studded_arms40",
                Name = "Studded Arms",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 19,
                Object_Type = 34,
                Item_Type = 28,
                Weight = 36,
                Model = 1194,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true,
                AllowedClasses = "3;48;43;50;31;25;32"
            };
            free_studded_arms40 = item;

            //Free Studded Legs
            item = new ItemTemplate
            {
                Id_nb = "free_studded_legs40",
                Name = "Studded Legs",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 19,
                Object_Type = 34,
                Item_Type = 27,
                Weight = 42,
                Model = 1193,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true,
                AllowedClasses = "3;48;43;50;31;25;32"
            };
            free_studded_legs40 = item;


            //Free Studded Boots
            item = new ItemTemplate
            {
                Id_nb = "free_studded_boots40",
                Name = "Studded Boots",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 19,
                Object_Type = 34,
                Item_Type = 23,
                Weight = 10,
                Model = 1196,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true,
                AllowedClasses = "3;48;43;50;31;25;32"
            };
            free_studded_boots40 = item;


            //Free Studded Gloves
            item = new ItemTemplate
            {
                Id_nb = "free_studded_gloves40",
                Name = "Studded Gloves",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 19,
                Object_Type = 34,
                Item_Type = 22,
                Weight = 10,
                Model = 1195,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true,
                AllowedClasses = "3;48;43;50;31;25;32"
            };
            free_studded_gloves40 = item;


            //Free Studded Helm
            item = new ItemTemplate
            {
                Id_nb = "free_studded_helm40",
                Name = "Studded Helm",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 19,
                Object_Type = 34,
                Item_Type = 21,
                Weight = 24,
                Model = 1291,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true,
                AllowedClasses = "3;48;43;50;31;25;32"
            };
            free_studded_helm40 = item;

            #endregion Free Studded Armor
            #region Free Plate Armor

            //Free Plate Vest
            item = new ItemTemplate
            {
                Id_nb = "free_plate_vest40",
                Name = "Plate Vest",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 34,
                Object_Type = 36,
                Item_Type = 25,
                Weight = 60,
                Model = 810,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_plate_vest40 = item;

            //Free Plate Arms
            item = new ItemTemplate
            {
                Id_nb = "free_plate_arms40",
                Name = "Plate Arms",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 34,
                Object_Type = 36,
                Item_Type = 28,
                Weight = 18,
                Model = 812,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_plate_arms40 = item;

            //Free Plate Legs
            item = new ItemTemplate
            {
                Id_nb = "free_plate_legs40",
                Name = "Plate Legs",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 34,
                Object_Type = 36,
                Item_Type = 27,
                Weight = 60,
                Model = 811,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_plate_legs40 = item;


            //Free Plate Boots
            item = new ItemTemplate
            {
                Id_nb = "free_plate_boots40",
                Name = "Plate Boots",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 34,
                Object_Type = 36,
                Item_Type = 23,
                Weight = 40,
                Model = 814,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_plate_boots40 = item;


            //Free Plate Gloves
            item = new ItemTemplate
            {
                Id_nb = "free_plate_gloves40",
                Name = "Plate Gloves",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 34,
                Object_Type = 36,
                Item_Type = 22,
                Weight = 12,
                Model = 813,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_plate_gloves40 = item;


            //Free Plate Helm
            item = new ItemTemplate
            {
                Id_nb = "free_plate_helm40",
                Name = "Plate Helm",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 80,
                SPD_ABS = 34,
                Object_Type = 36,
                Item_Type = 21,
                Weight = 40,
                Model = 1291,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_plate_helm40 = item;

            #endregion Free Plate Armor

            #endregion Armor
            #region Weapons

            #region Free Staff (Caster)
            //free caster staff
            item = new ItemTemplate
            {
                Id_nb = "free_staff40",
                Name = "Staff",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 44,
                Hand = 1,
                Type_Damage = 1,
                Object_Type = 8,
                Item_Type = 12,
                Weight = 50,
                Model = 19,
                Bonus = 15,
                Bonus1 = 50,
                Bonus1Type = 165,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_staff40 = item;
            #endregion Free Staff (Caster)
            #region Free Slash Alb (sword) Left hand
            //free slash (sword)
            item = new ItemTemplate
            {
                Id_nb = "free_slash_alb40",
                Name = "Slash Alb",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 28,
                Hand = 0,
                Type_Damage = 2,
                Object_Type = 3,
                Item_Type = 11,
                Weight = 35,
                Model = 1017,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_slash_alb40 = item;
            #endregion Free Slash Alb (sword) Left hand
            #region Free Slash Mid (sword) Left hand
            //free slash (sword)
            item = new ItemTemplate
            {
                Id_nb = "free_slash_mid40",
                Name = "Slash Mid",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 28,
                Hand = 0,
                Type_Damage = 2,
                Object_Type = 11,
                Item_Type = 11,
                Weight = 35,
                Model = 1017,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_slash_mid40 = item;
            #endregion Free Slash Mid (sword) Left hand
            #region Free Slash Hib (sword) Left hand
            //free slash (sword)
            item = new ItemTemplate
            {
                Id_nb = "free_slash_hib40",
                Name = "Slash hib",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 28,
                Hand = 0,
                Type_Damage = 2,
                Object_Type = 19,
                Item_Type = 11,
                Weight = 35,
                Model = 1017,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_slash_hib40 = item;
            #endregion Free Slash Hib (sword) Left hand
            #region Free Thrust Alb (Pierce) Left hand
            item = new ItemTemplate
            {
                Id_nb = "free_thrust_alb40",
                Name = "Thrust Alb",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 28,
                Hand = 0,
                Type_Damage = 3,
                Object_Type = 4,
                Item_Type = 11,
                Weight = 35,
                Model = 2684,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_thrust_alb40 = item;
            #endregion Free Thrust Alb (Pierce) Left hand
            #region Free Thrust Hib (Pierce) Left hand
            item = new ItemTemplate
            {
                Id_nb = "free_thrust_hib40",
                Name = "Thrust Hib",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 28,
                Hand = 0,
                Type_Damage = 3,
                Object_Type = 21,
                Item_Type = 11,
                Weight = 35,
                Model = 2684,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_thrust_hib40 = item;
            #endregion Free Thrust Hib (Pierce) Left hand
            #region Free Crush Alb (blunt) Left hand
            item = new ItemTemplate
            {
                Id_nb = "free_crush_alb40",
                Name = "Crush Alb",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 28,
                Hand = 0,
                Type_Damage = 1,
                Object_Type = 2,
                Item_Type = 11,
                Weight = 35,
                Model = 1009,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_crush_alb40 = item;
            #endregion Free Crush Alb (blunt) Left hand
            #region Free Crush Mid (blunt) Left hand
            item = new ItemTemplate
            {
                Id_nb = "free_crush_mid40",
                Name = "Crush Mid",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 28,
                Hand = 0,
                Type_Damage = 1,
                Object_Type = 12,
                Item_Type = 11,
                Weight = 35,
                Model = 1009,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_crush_mid40 = item;
            #endregion Free Crush Mid (blunt) Left hand
            #region Free Crush Hib (blunt) Left hand
            item = new ItemTemplate
            {
                Id_nb = "free_crush_hib40",
                Name = "Crush Hib",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 28,
                Hand = 0,
                Type_Damage = 1,
                Object_Type = 20,
                Item_Type = 11,
                Weight = 35,
                Model = 1009,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_crush_hib40 = item;
            #endregion Free Crush Hib (blunt) Left hand
            #region Free Axe Left hand
            item = new ItemTemplate
            {
                Id_nb = "free_axe40",
                Name = "Axe",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 35,
                Hand = 0,
                Type_Damage = 2,
                Object_Type = 17,
                Item_Type = 11,
                Weight = 35,
                Model = 1010,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_axe40 = item;
            #endregion Free Axe Left hand
            #region Free Long Bow (Scout)
            item = new ItemTemplate
            {
                Id_nb = "free_long_bow40",
                Name = "Long Bow",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 54,
                Hand = 1,
                Type_Damage = 3,
                Object_Type = 9,
                Item_Type = 13,
                Weight = 40,
                Model = 849,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true,
                AllowedClasses = "3" //scout
            };
            free_long_bow40 = item;
            #endregion Free Long Bow (Scout)
            #region Free Composite Bow (Hunter)
            item = new ItemTemplate
            {
                Id_nb = "free_composite_bow40",
                Name = "Composite Bow",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 44,
                Hand = 1,
                Type_Damage = 3,
                Object_Type = 15,
                Item_Type = 13,
                Weight = 40,
                Model = 564,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true,
                AllowedClasses = "25" //hunter
            };
            free_composite_bow40 = item;
            #endregion Free Composite Bow (Hunter)
            #region Free Recurved Bow (Ranger)
            item = new ItemTemplate
            {
                Id_nb = "free_recurved_bow40",
                Name = "Recurved Bow",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 54,
                Hand = 1,
                Type_Damage = 3,
                Object_Type = 18,
                Item_Type = 13,
                Weight = 40,
                Model = 925,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true,
                AllowedClasses = "50"//ranger
            };
            free_recurved_bow40 = item;
            #endregion Free Recurved Bow (Ranger)
            #region Free Short Bow
            item = new ItemTemplate
            {
                Id_nb = "free_short_bow40",
                Name = "Short Bow",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 40,
                Hand = 1,
                Type_Damage = 3,
                Object_Type = 5,
                Item_Type = 13,
                Weight = 40,
                Model = 922,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_short_bow40 = item;
            #endregion Free Short Bow
            #region Free Crossbow
            item = new ItemTemplate
            {
                Id_nb = "free_crossbow40",
                Name = "Crossbow",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 40,
                Type_Damage = 3,
                Object_Type = 10,
                Item_Type = 13,
                Weight = 40,
                Model = 226,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_crossbow40 = item;
            #endregion Free Crossbow
            #region Free TwoHanded Slash Alb
            item = new ItemTemplate
            {
                Id_nb = "free_twohanded_slash_alb40",
                Name = "TwoHanded Slash Alb",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 51,
                Hand = 1,
                Type_Damage = 2,
                Object_Type = 6,
                Item_Type = 12,
                Weight = 50,
                Model = 907,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_twohanded_slash_alb40 = item;
            #endregion Free Twohanded Slash Alb
            #region Free TwoHanded Slash Hib
            item = new ItemTemplate
            {
                Id_nb = "free_twohanded_slash_hib40",
                Name = "TwoHanded Slash Hib",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 51,
                Hand = 1,
                Type_Damage = 2,
                Object_Type = 22,
                Item_Type = 12,
                Weight = 50,
                Model = 907,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_twohanded_slash_hib40 = item;
            #endregion Free Twohanded Slash Hib
            #region Free TwoHanded Crush Alb
            item = new ItemTemplate
            {
                Id_nb = "free_twohanded_crush_alb40",
                Name = "TwoHanded Crush Alb",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 48,
                Hand = 1,
                Type_Damage = 1,
                Object_Type = 6,
                Item_Type = 12,
                Weight = 50,
                Model = 574,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_twohanded_crush_alb40 = item;
            #endregion Free Twohanded Crush Alb
            #region Free TwoHanded Crush Hib
            item = new ItemTemplate
            {
                Id_nb = "free_twohanded_crush_hib40",
                Name = "TwoHanded Crush Hib",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 48,
                Hand = 1,
                Type_Damage = 1,
                Object_Type = 22,
                Item_Type = 12,
                Weight = 50,
                Model = 574,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_twohanded_crush_hib40 = item;
            #endregion Free Twohanded Crush Hib
            #region Free TwoHanded Axe
            item = new ItemTemplate
            {
                Id_nb = "free_twohanded_axe40",
                Name = "TwoHanded Axe",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 51,
                Hand = 1,
                Type_Damage = 2,
                Object_Type = 13,
                Item_Type = 12,
                Weight = 50,
                Model = 577,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_twohanded_axe40 = item;
            #endregion Free TwoHanded Axe
            #region Free TwoHanded Sword
            item = new ItemTemplate
            {
                Id_nb = "free_twohanded_sword40",
                Name = "TwoHanded Sword",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 56,
                Hand = 1,
                Type_Damage = 2,
                Object_Type = 11,
                Item_Type = 12,
                Weight = 50,
                Model = 658,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_twohanded_sword40 = item;
            #endregion Free TwoHanded Sword
            #region Free TwoHanded Hammer
            item = new ItemTemplate
            {
                Id_nb = "free_twohanded_hammer40",
                Name = "TwoHanded Hammer",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 48,
                Hand = 1,
                Type_Damage = 1,
                Object_Type = 12,
                Item_Type = 12,
                Weight = 50,
                Model = 574,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_twohanded_hammer40 = item;
            #endregion Free TwoHanded Hammer
            #region Free Small Shield
            item = new ItemTemplate
            {
                Id_nb = "free_small_shield40",
                Name = "Small Shield",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 28,
                Hand = 2,
                Type_Damage = 1,
                Object_Type = 42,
                Item_Type = 11,
                Weight = 35,
                Model = 2218,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_small_shield40 = item;
            #endregion Free Small Shield
            #region Free Medium Shield
            item = new ItemTemplate
            {
                Id_nb = "free_medium_shield40",
                Name = "Medium Shield",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 32,
                Hand = 2,
                Type_Damage = 2,
                Object_Type = 42,
                Item_Type = 11,
                Weight = 35,
                Model = 2219,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_medium_shield40 = item;
            #endregion Free Medium Shield
            #region Free Large Shield
            item = new ItemTemplate
            {
                Id_nb = "free_large_shield40",
                Name = "Large Shield",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 40,
                Hand = 2,
                Type_Damage = 3,
                Object_Type = 42,
                Item_Type = 11,
                Weight = 35,
                Model = 2220,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_large_shield40 = item;
            #endregion Free Large Shield
            #region Free Slash Spear
            item = new ItemTemplate
            {
                Id_nb = "free_slash_spear40",
                Name = "Slash Spear",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 40,
                Hand = 1,
                Type_Damage = 2,
                Object_Type = 14,
                Item_Type = 12,
                Weight = 50,
                Model = 657,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_slash_spear40 = item;
            #endregion Free Slash Spear
            #region Free Thrust Spear
            item = new ItemTemplate
            {
                Id_nb = "free_thrust_spear40",
                Name = "Thrust Spear",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 40,
                Hand = 1,
                Type_Damage = 3,
                Object_Type = 14,
                Item_Type = 12,
                Weight = 50,
                Model = 657,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_thrust_spear40 = item;
            #endregion Free Thrust Spear
            #region Free Slash Flail
            item = new ItemTemplate
            {
                Id_nb = "free_slash_flail40",
                Name = "Slash Flail",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 35,
                Type_Damage = 2,
                Object_Type = 24,
                Item_Type = 10,
                Weight = 35,
                Model = 861,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_slash_flail40 = item;
            #endregion Free Slash Flail
            #region Free Thrust Flail
            item = new ItemTemplate
            {
                Id_nb = "free_thrust_flail40",
                Name = "Thrust Flail",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 35,
                Type_Damage = 3,
                Object_Type = 24,
                Item_Type = 10,
                Weight = 35,
                Model = 861,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_thrust_flail40 = item;
            #endregion Free Thrust Flail
            #region Free Crush Flail
            item = new ItemTemplate
            {
                Id_nb = "free_crush_flail40",
                Name = "Crush Flail",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 35,
                Type_Damage = 1,
                Object_Type = 24,
                Item_Type = 10,
                Weight = 35,
                Model = 861,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_crush_flail40 = item;
            #endregion Free Crush Flail
            #region Free Slash Claw
            item = new ItemTemplate
            {
                Id_nb = "free_slash_claw40",
                Name = "Slash Claw",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 30,
                Hand = 2,
                Type_Damage = 2,
                Object_Type = 25,
                Item_Type = 11,
                Weight = 35,
                Model = 967,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_slash_claw40 = item;
            #endregion Free Slash Claw
            #region Free Thrust Claw
            item = new ItemTemplate
            {
                Id_nb = "free_thrust_claw40",
                Name = "Thrust Claw",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 30,
                Hand = 2,
                Type_Damage = 3,
                Object_Type = 25,
                Item_Type = 11,
                Weight = 35,
                Model = 967,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_thrust_claw40 = item;
            #endregion Free Thrust Claw
            #region Free Harp
            item = new ItemTemplate
            {
                Id_nb = "free_harp40",
                Name = "Harp",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 4,
                SPD_ABS = 40,
                Object_Type = 45,
                Hand = 1,
                Item_Type = 12,
                Weight = 50,
                Model = 3985,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_harp40 = item;
            #endregion Free Harp

            #region Free Mauler Staff
            item = new ItemTemplate
            {
                Id_nb = "free_mauler_staff40",
                Name = "Mauler Staff",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 30,
                Hand = 1,
                Type_Damage = 1,
                Object_Type = 28,
                Item_Type = 12,
                Weight = 50,
                Model = 1950,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_mauler_staff40 = item;
            #endregion Free Mauler Staff

            #region Free Mauler Right Wrap
            item = new ItemTemplate
            {
                Id_nb = "free_mauler_right_wrap40",
                Name = "Mauler Right Wrap",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 33,
                Hand = 0,
                Type_Damage = 1,
                Object_Type = 27,
                Item_Type = 10,
                Weight = 35,
                Model = 3550,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_mauler_right_wrap40 = item;
            #endregion Free Mauler Right Wrap

            #region Free Mauler Left Wrap
            item = new ItemTemplate
            {
                Id_nb = "free_mauler_left_wrap40",
                Name = "Mauler Left Wrap",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 33,
                Hand = 0,
                Type_Damage = 1,
                Object_Type = 27,
                Item_Type = 11,
                Weight = 35,
                Model = 3550,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_mauler_left_wrap40 = item;
            #endregion Free Mauler Left Wrap

            #region Free Scythe
            item = new ItemTemplate
            {
                Id_nb = "free_scythe40",
                Name = "Scythe",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 37,
                Hand = 1,
                Type_Damage = 2,
                Object_Type = 26,
                Item_Type = 12,
                Weight = 50,
                Model = 929,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_scythe40 = item;
            #endregion Free Scythe
            #region Free Crush Polearm
            item = new ItemTemplate
            {
                Id_nb = "free_crush_polearm40",
                Name = "Crush Polearm",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 56,
                Hand = 1,
                Type_Damage = 1,
                Object_Type = 7,
                Item_Type = 12,
                Weight = 50,
                Model = 2664,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_crush_polearm40 = item;
            #endregion Free Crush Polearm
            #region Free Thrust Polearm
            item = new ItemTemplate
            {
                Id_nb = "free_thrust_polearm40",
                Name = "Thrust Polearm",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 43,
                Hand = 1,
                Type_Damage = 3,
                Object_Type = 7,
                Item_Type = 12,
                Weight = 50,
                Model = 2665,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_thrust_polearm40 = item;
            #endregion Free Thrust Polearm
            #region Free Slash Polearm
            item = new ItemTemplate
            {
                Id_nb = "free_slash_polearm40",
                Name = "Slash Polearm",
                Level = 40,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 144,
                SPD_ABS = 48,
                Hand = 1,
                Type_Damage = 2,
                Object_Type = 7,
                Item_Type = 12,
                Weight = 50,
                Model = 2663,
                Bonus = 15,
                IsPickable = true,
                IsDropable = false,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = false,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_slash_polearm40 = item;
            #endregion Free Slash Polearm



        }

        public void GiveFreeArmor(GamePlayer player)
        {
            if (player == null)
                return;


            //Find out the best armor type for this player and give him a full set of it.
            String armorType = GlobalConstants.ArmorLevelToName(player.BestArmorLevel, (eRealm)player.Realm);
            //change helm model to fit players head depending on their realm.
            int realmHelmModel = 0;
            if ((eRealm)player.Realm == eRealm.Hibernia) { realmHelmModel = 1292; }
            if ((eRealm)player.Realm == eRealm.Albion) { realmHelmModel = 1294; }
            if ((eRealm)player.Realm == eRealm.Midgard) { realmHelmModel = 1291; }

            switch (armorType)
            {
                case "scale": //scale gets chain armor instead of scale
                case "chain":
                    {
                        GiveItem(player, free_chain_vest40);
                        GiveItem(player, free_chain_legs40);
                        GiveItem(player, free_chain_boots40);
                        GiveItem(player, free_chain_arms40);
                        GiveItem(player, free_chain_gloves40);
                        free_chain_helm40.Model = realmHelmModel;
                        GiveItem(player, free_chain_helm40);
                        break;
                    }
                case "cloth":
                    {
                        GiveItem(player, free_cloth_vest40);
                        GiveItem(player, free_cloth_legs40);
                        GiveItem(player, free_cloth_boots40);
                        GiveItem(player, free_cloth_arms40);
                        GiveItem(player, free_cloth_gloves40);
                        free_cloth_helm40.Model = realmHelmModel;
                        GiveItem(player, free_cloth_helm40);
                        break;
                    }
                case "leather":
                    {
                        GiveItem(player, free_leather_vest40);
                        GiveItem(player, free_leather_legs40);
                        GiveItem(player, free_leather_boots40);
                        GiveItem(player, free_leather_arms40);
                        GiveItem(player, free_leather_gloves40);
                        free_leather_helm40.Model = realmHelmModel;
                        GiveItem(player, free_leather_helm40);
                        break;
                    }
                case "reinforced": //reinforced gets studded armor instead of reinforced
                case "studded":
                    {
                        GiveItem(player, free_studded_vest40);
                        GiveItem(player, free_studded_legs40);
                        GiveItem(player, free_studded_boots40);
                        GiveItem(player, free_studded_arms40);
                        GiveItem(player, free_studded_gloves40);
                        free_studded_helm40.Model = realmHelmModel;
                        GiveItem(player, free_studded_helm40);
                        break;
                    }
                case "plate":
                    {
                        GiveItem(player, free_plate_vest40);
                        GiveItem(player, free_plate_legs40);
                        GiveItem(player, free_plate_boots40);
                        GiveItem(player, free_plate_arms40);
                        GiveItem(player, free_plate_gloves40);
                        free_plate_helm40.Model = realmHelmModel;
                        GiveItem(player, free_plate_helm40);
                        break;
                    }
            }
            //everyone gets the epic cloak, necklace, bracers, rings, and jewel
            return;
        }

        public void EquipFreeArmor(GamePlayer player)
        {
            if (player == null)
                return;


            //Find out the best armor type for this player and give him a full set of it.
            String armorType = GlobalConstants.ArmorLevelToName(player.BestArmorLevel, (eRealm)player.Realm);
            //change helm model to fit players head depending on their realm.
            int realmHelmModel = 0;
            if ((eRealm)player.Realm == eRealm.Hibernia) { realmHelmModel = 1292; }
            if ((eRealm)player.Realm == eRealm.Albion) { realmHelmModel = 1294; }
            if ((eRealm)player.Realm == eRealm.Midgard) { realmHelmModel = 1291; }

            //player.Inventory.GetItem
            switch (armorType)
            {
                case "scale": //scale gets chain armor instead of scale
                case "chain":
                    {
                        EquipGiveItem(player, free_chain_vest40, eInventorySlot.TorsoArmor);
                        EquipGiveItem(player, free_chain_legs40, eInventorySlot.LegsArmor);
                        EquipGiveItem(player, free_chain_boots40, eInventorySlot.FeetArmor);
                        EquipGiveItem(player, free_chain_arms40, eInventorySlot.ArmsArmor);
                        EquipGiveItem(player, free_chain_gloves40, eInventorySlot.HandsArmor);
                        free_chain_helm40.Model = realmHelmModel;
                        EquipGiveItem(player, free_chain_helm40, eInventorySlot.HeadArmor);
                        break;
                    }
                case "cloth":
                    {
                        EquipGiveItem(player, free_cloth_vest40, eInventorySlot.TorsoArmor);
                        EquipGiveItem(player, free_cloth_legs40, eInventorySlot.LegsArmor);
                        EquipGiveItem(player, free_cloth_boots40, eInventorySlot.FeetArmor);
                        EquipGiveItem(player, free_cloth_arms40, eInventorySlot.ArmsArmor);
                        EquipGiveItem(player, free_cloth_gloves40, eInventorySlot.HandsArmor);
                        free_cloth_helm40.Model = realmHelmModel;
                        EquipGiveItem(player, free_cloth_helm40, eInventorySlot.HeadArmor);
                        break;
                    }
                case "leather":
                    {
                        EquipGiveItem(player, free_leather_vest40, eInventorySlot.TorsoArmor);
                        EquipGiveItem(player, free_leather_legs40, eInventorySlot.LegsArmor);
                        EquipGiveItem(player, free_leather_boots40, eInventorySlot.FeetArmor);
                        EquipGiveItem(player, free_leather_arms40, eInventorySlot.ArmsArmor);
                        EquipGiveItem(player, free_leather_gloves40, eInventorySlot.HandsArmor);
                        free_leather_helm40.Model = realmHelmModel;
                        EquipGiveItem(player, free_leather_helm40, eInventorySlot.HeadArmor);
                        break;
                    }
                case "reinforced": //reinforced gets studded armor instead of reinforced
                case "studded":
                    {
                        EquipGiveItem(player, free_studded_vest40, eInventorySlot.TorsoArmor);
                        EquipGiveItem(player, free_studded_legs40, eInventorySlot.LegsArmor);
                        EquipGiveItem(player, free_studded_boots40, eInventorySlot.FeetArmor);
                        EquipGiveItem(player, free_studded_arms40, eInventorySlot.ArmsArmor);
                        EquipGiveItem(player, free_studded_gloves40, eInventorySlot.HandsArmor);
                        free_studded_helm40.Model = realmHelmModel;
                        EquipGiveItem(player, free_studded_helm40, eInventorySlot.HeadArmor);
                        break;
                    }
                case "plate":
                    {
                        EquipGiveItem(player, free_plate_vest40, eInventorySlot.TorsoArmor);
                        EquipGiveItem(player, free_plate_legs40, eInventorySlot.LegsArmor);
                        EquipGiveItem(player, free_plate_boots40, eInventorySlot.FeetArmor);
                        EquipGiveItem(player, free_plate_arms40, eInventorySlot.ArmsArmor);
                        EquipGiveItem(player, free_plate_gloves40, eInventorySlot.HandsArmor);
                        free_plate_helm40.Model = realmHelmModel;
                        EquipGiveItem(player, free_plate_helm40, eInventorySlot.HeadArmor);
                        break;
                    }

            }

            return;
        }




        public void GiveFreeWeapons(GamePlayer player)
        {
            if (player == null)
                return;


            //Check if the player can use the weapon, if yes then give him one of that kind.
            if (player.HasAbilityToUseItem(free_staff40)) { GiveItem(player, free_staff40); }
            if (player.HasAbilityToUseItem(free_slash_alb40)) { GiveItem(player, free_slash_alb40); }
            if (player.HasAbilityToUseItem(free_slash_mid40)) { GiveItem(player, free_slash_mid40); }
            if (player.HasAbilityToUseItem(free_slash_hib40)) { GiveItem(player, free_slash_hib40); }
            if (player.HasAbilityToUseItem(free_thrust_alb40)) { GiveItem(player, free_thrust_alb40); }
            if (player.HasAbilityToUseItem(free_thrust_hib40)) { GiveItem(player, free_thrust_hib40); }
            if (player.HasAbilityToUseItem(free_crush_alb40)) { GiveItem(player, free_crush_alb40); }
            if (player.HasAbilityToUseItem(free_crush_mid40)) { GiveItem(player, free_crush_mid40); }
            if (player.HasAbilityToUseItem(free_crush_hib40)) { GiveItem(player, free_crush_hib40); }
            if (player.HasAbilityToUseItem(free_axe40)) { GiveItem(player, free_axe40); }
            if (player.HasAbilityToUseItem(free_long_bow40)) { GiveItem(player, free_long_bow40); }
            if (player.HasAbilityToUseItem(free_composite_bow40)) { GiveItem(player, free_composite_bow40); }
            if (player.HasAbilityToUseItem(free_recurved_bow40)) { GiveItem(player, free_recurved_bow40); }
            if (player.HasAbilityToUseItem(free_short_bow40)) { GiveItem(player, free_short_bow40); }
            if (player.HasAbilityToUseItem(free_crossbow40)) { GiveItem(player, free_crossbow40); }
            if (player.HasAbilityToUseItem(free_twohanded_slash_alb40)) { GiveItem(player, free_twohanded_slash_alb40); }
            if (player.HasAbilityToUseItem(free_twohanded_slash_hib40)) { GiveItem(player, free_twohanded_slash_hib40); }
            if (player.HasAbilityToUseItem(free_twohanded_crush_alb40)) { GiveItem(player, free_twohanded_crush_alb40); }
            if (player.HasAbilityToUseItem(free_twohanded_crush_hib40)) { GiveItem(player, free_twohanded_crush_hib40); }
            if (player.HasAbilityToUseItem(free_twohanded_axe40)) { GiveItem(player, free_twohanded_axe40); }
            if (player.HasAbilityToUseItem(free_twohanded_sword40)) { GiveItem(player, free_twohanded_sword40); }
            if (player.HasAbilityToUseItem(free_twohanded_hammer40)) { GiveItem(player, free_twohanded_hammer40); }
            if (player.HasAbilityToUseItem(free_large_shield40)) { GiveItem(player, free_large_shield40); }
            else if (player.HasAbilityToUseItem(free_medium_shield40)) { GiveItem(player, free_medium_shield40); }
            else if (player.HasAbilityToUseItem(free_small_shield40)) { GiveItem(player, free_small_shield40); }
            if (player.HasAbilityToUseItem(free_slash_spear40)) { GiveItem(player, free_slash_spear40); }
            if (player.HasAbilityToUseItem(free_thrust_spear40)) { GiveItem(player, free_thrust_spear40); }
            if (player.HasAbilityToUseItem(free_slash_flail40)) { GiveItem(player, free_slash_flail40); }
            if (player.HasAbilityToUseItem(free_thrust_flail40)) { GiveItem(player, free_thrust_flail40); }
            if (player.HasAbilityToUseItem(free_crush_flail40)) { GiveItem(player, free_crush_flail40); }
            if (player.HasAbilityToUseItem(free_slash_claw40)) { GiveItem(player, free_slash_claw40); }
            if (player.HasAbilityToUseItem(free_thrust_claw40)) { GiveItem(player, free_thrust_claw40); }
            if (player.HasAbilityToUseItem(free_harp40)) { GiveItem(player, free_harp40); }
            if (player.HasAbilityToUseItem(free_mauler_staff40)) { GiveItem(player, free_mauler_staff40); }
            if (player.HasAbilityToUseItem(free_mauler_right_wrap40)) { GiveItem(player, free_mauler_right_wrap40); }
            if (player.HasAbilityToUseItem(free_mauler_left_wrap40)) { GiveItem(player, free_mauler_left_wrap40); }
            if (player.HasAbilityToUseItem(free_scythe40)) { GiveItem(player, free_scythe40); }
            if (player.HasAbilityToUseItem(free_crush_polearm40)) { GiveItem(player, free_crush_polearm40); }
            if (player.HasAbilityToUseItem(free_thrust_polearm40)) { GiveItem(player, free_thrust_polearm40); }
            if (player.HasAbilityToUseItem(free_slash_polearm40)) { GiveItem(player, free_slash_polearm40); }

        }
        #endregion Give Free Weapons

        #region GiveItem

        protected static void GiveItem(GamePlayer player, ItemTemplate itemTemplate)
        {
            GiveItem(null, player, itemTemplate);
        }

        protected static void GiveItem(GameLiving source, GamePlayer player, ItemTemplate itemTemplate)
        {

            ItemTemplate generic = GameServer.Database.FindObjectByKey<ItemTemplate>(itemTemplate.Id_nb);
            InventoryItem temp = new GameInventoryItem(generic);




            if (temp == null)
            {

                GameServer.Database.AddObject(itemTemplate);
            }


            if (player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, temp))
            {

                if (source == null)
                {
                    player.Out.SendMessage("You receive the " + itemTemplate.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                else
                {
                    player.Out.SendMessage("You receive " + itemTemplate.GetName(0, false) + " from " + source.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
            }
            else
            {
                player.CreateItemOnTheGround(temp);
                player.Out.SendMessage("Your Inventory is full. You couldn't recieve the " + itemTemplate.Name + ", so it's been placed on the ground. Pick it up as soon as possible or it will vanish in a few minutes.", eChatType.CT_Important, eChatLoc.CL_PopupWindow);
            }
        }
        #endregion GiveItem



        protected static void EquipGiveItem(GamePlayer player, ItemTemplate itemTemplate, eInventorySlot slot)
        {
            if (player.Inventory.GetItem(slot) != null)
            {
                GiveItem(player, player, itemTemplate);
                return;
            }
            EquipGiveItem(null, player, itemTemplate, slot);
        }

        protected static void EquipGiveItem(GameLiving source, GamePlayer player, ItemTemplate itemTemplate, eInventorySlot slot)
        {
            ItemTemplate generic = GameServer.Database.FindObjectByKey<ItemTemplate>(itemTemplate.Id_nb);
            InventoryItem temp = new GameInventoryItem(generic);
            

            if (temp == null)
            {
                GameServer.Database.AddObject(itemTemplate);

            }


            if (player.Inventory.AddItem(slot, temp))
            {
                if (source == null)
                {
                    player.Out.SendMessage("You receive the " + itemTemplate.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                else
                {
                    player.Out.SendMessage("You receive " + itemTemplate.GetName(0, false) + " from " + source.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
            }
            else
            {
                player.CreateItemOnTheGround(temp);
                player.Out.SendMessage("Your Inventory is full. You couldn't recieve the " + itemTemplate.Name + ", so it's been placed on the ground. Pick it up as soon as possible or it will vanish in a few minutes.", eChatType.CT_Important, eChatLoc.CL_PopupWindow);
            }
        }


        public void SendReply(GamePlayer player, string msg)
        {
            player.Out.SendMessage(msg, eChatType.CT_System, eChatLoc.CL_PopupWindow);
        }
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;

            
            if (player.Level < 40)
            {
                player.Out.SendMessage("Hello i have stuff for level 40 " + player.Name + ", if you have reached level 40 come back to me!.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return false;
            }
            if (player.HasFreeStartArmor && player.HasFreeStartWeapons40)
            {
                player.Out.SendMessage("hm, You already have all that Items! ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return false;
            }

            player.Out.SendMessage("Hello " + player.Name + ", I am able to give out free level 40 [Armor] and [Weapons].", eChatType.CT_Say, eChatLoc.CL_PopupWindow);



            return true;
        }

        public override bool WhisperReceive(GameLiving source, string str)
        {
            if (!base.WhisperReceive(source, str))
                return false;

            GamePlayer player = source as GamePlayer;

            if (player == null)
                return false;


            if (str == "Armor")
            {
                try
                {
                    if (player.HasFreeStartArmor40)
                    {
                        player.Out.SendMessage("Sorry, You already have this items! ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        return false;
                    }
                    else
                    {
                        EquipFreeArmor(player);
                        player.HasFreeStartArmor40 = true;
                    }
                }
                catch (Exception ex)
                {
                    player.Out.SendMessage(ex.Message, eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    GiveFreeArmor(player);
                    player.HasFreeStartArmor40 = true;
                }
                return true;
            }
            if (str == "Weapons")
            {
                if (player.HasFreeStartWeapons40)
                {
                    player.Out.SendMessage("Sorry, You already have this items! ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    return false;
                }
                else
                {
                    GiveFreeWeapons(player);
                    player.HasFreeStartWeapons40 = true;
                    return true;
                }
            }
            return true;
        }






    }


}



