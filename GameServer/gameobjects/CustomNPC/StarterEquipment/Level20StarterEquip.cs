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
    public class Level20StarterEquipment : GameNPC
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        #region Declarations

        private static ItemTemplate free_chain_vest = null; //Free Chain Vest
        private static ItemTemplate free_chain_arms = null; //Free Chain Arms
        private static ItemTemplate free_chain_legs = null; //Free Chain Legs
        private static ItemTemplate free_chain_helm = null; //Free Chain Helm
        private static ItemTemplate free_chain_gloves = null; //Free Chain Gloves
        private static ItemTemplate free_chain_boots = null; //Free Chain Boots

        private static ItemTemplate free_cloth_vest = null; //Free Cloth Vest
        private static ItemTemplate free_cloth_arms = null; //Free Cloth Arms
        private static ItemTemplate free_cloth_legs = null; //Free Cloth Legs
        private static ItemTemplate free_cloth_helm = null; //Free Cloth Helm
        private static ItemTemplate free_cloth_gloves = null; //Free Cloth Gloves
        private static ItemTemplate free_cloth_boots = null; //Free Cloth Boots

        private static ItemTemplate free_leather_vest = null; //Free Leather Vest
        private static ItemTemplate free_leather_arms = null; //Free Leather Arms
        private static ItemTemplate free_leather_legs = null; //Free Leather Legs
        private static ItemTemplate free_leather_helm = null; //Free Leather Helm
        private static ItemTemplate free_leather_gloves = null; //Free Leather Gloves
        private static ItemTemplate free_leather_boots = null; //Free Leather Boots

        private static ItemTemplate free_studded_vest = null; //Free Studded Vest
        private static ItemTemplate free_studded_arms = null; //Free Studded Arms
        private static ItemTemplate free_studded_legs = null; //Free Studded Legs
        private static ItemTemplate free_studded_helm = null; //Free Studded Helm
        private static ItemTemplate free_studded_gloves = null; //Free Studded Gloves
        private static ItemTemplate free_studded_boots = null; //Free Studded Boots

        private static ItemTemplate free_plate_vest = null; //Free Plate Vest
        private static ItemTemplate free_plate_arms = null; //Free Plate Arms
        private static ItemTemplate free_plate_legs = null; //Free Plate Legs
        private static ItemTemplate free_plate_helm = null; //Free Plate Helm
        private static ItemTemplate free_plate_gloves = null; //Free Plate Gloves
        private static ItemTemplate free_plate_boots = null; //Free Plate Boots

        //weapons
        private static ItemTemplate free_staff = null; //Free Caster Staff
        private static ItemTemplate free_slash_alb = null; //Free Slash
        private static ItemTemplate free_slash_mid = null; //Free Slash
        private static ItemTemplate free_slash_hib = null; //Free Slash
        private static ItemTemplate free_thrust_alb = null; //Free Thrust
        private static ItemTemplate free_thrust_hib = null; //Free Thrust
        private static ItemTemplate free_crush_alb = null; //Free Crush
        private static ItemTemplate free_crush_mid = null; //Free Crush
        private static ItemTemplate free_crush_hib = null; //Free Crush
        private static ItemTemplate free_axe = null; //Free Axe
        private static ItemTemplate free_long_bow = null; //Free Long Bow
        private static ItemTemplate free_composite_bow = null; //Free Composite Bow
        private static ItemTemplate free_recurved_bow = null; //Free Recurved Bow
        private static ItemTemplate free_short_bow = null; //Free Short Bow
        private static ItemTemplate free_crossbow = null; //Free Crossbow
        private static ItemTemplate free_twohanded_slash_alb = null; //Free TwoHanded Slash
        private static ItemTemplate free_twohanded_slash_hib = null; //Free TwoHanded Slash
        private static ItemTemplate free_twohanded_crush_alb = null; //Free TwoHanded Crush
        private static ItemTemplate free_twohanded_crush_hib = null; //Free TwoHanded Crush
        private static ItemTemplate free_twohanded_axe = null; //Free TwoHanded Axe
        private static ItemTemplate free_twohanded_sword = null; //Free TwoHanded Sword
        private static ItemTemplate free_twohanded_hammer = null; //Free TwoHanded Hammer
        private static ItemTemplate free_small_shield = null; //Free Small Shield
        private static ItemTemplate free_medium_shield = null; //Free Medium Shield
        private static ItemTemplate free_large_shield = null; //Free Large Shield
        private static ItemTemplate free_slash_spear = null; //Free Slash Spear
        private static ItemTemplate free_thrust_spear = null; //Free Thrust Spear
        private static ItemTemplate free_slash_flail = null; //Free Slash Flail
        private static ItemTemplate free_thrust_flail = null; //Free Thrust Flail
        private static ItemTemplate free_crush_flail = null; //Free Crush Flail
        private static ItemTemplate free_slash_claw = null; //Free Slash Claw
        private static ItemTemplate free_thrust_claw = null; //Free Thrust Claw
        private static ItemTemplate free_harp = null; //Free Harp
        private static ItemTemplate free_mauler_staff = null; //Free Mauler Staff
        private static ItemTemplate free_mauler_right_wrap = null;      //Free Mauler Fist Wrap
        private static ItemTemplate free_mauler_left_wrap = null; 		//Free Mauler Fist Wrap
        private static ItemTemplate free_scythe = null; //Free Scythe
        private static ItemTemplate free_crush_polearm = null; //Free Crush Polearm
        private static ItemTemplate free_thrust_polearm = null; //Free Thrust Polearm
        private static ItemTemplate free_slash_polearm = null; //Free Slash Polearm



        #endregion Declarations

        public override bool AddToWorld()
        {

            Level = 55;
            //Name = "Retho Tohmson";
            GuildName = "Free Level 20 Armor and Weapons";
            //Model = 1903;
            base.AddToWorld();
            this.Flags = eFlags.PEACE;
            return true;
        }
        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            if (log.IsInfoEnabled)
                log.Info("Free Level 20 Stuff NPC Initializing...");



            #region Armor
            ItemTemplate item = null;
            item = GameServer.Database.FindObjectByKey<ItemTemplate>("free_chain_vest");
            if (item == null)
                #region Free Chain Armor

                //Free Chain Vest
                item = new ItemTemplate();
            item.Id_nb = "free_chain_vest";
            item.Name = "Chain Vest";
            item.Level = 20;
            item.Durability = 50000;
            item.MaxDurability = 50000;
            item.Condition = 50000;
            item.MaxCondition = 50000;
            item.Quality = 95;
            item.DPS_AF = 40;
            item.SPD_ABS = 27;
            item.Object_Type = 35;
            item.Item_Type = 25;
            item.Weight = 50;
            item.Model = 776;
            item.IsPickable = true;
            item.IsDropable = false;
            item.CanDropAsLoot = false;
            item.IsTradable = false;
            item.IsIndestructible = false;
            item.MaxCount = 1;
            item.PackSize = 1;
            item.AllowAdd = true;
            free_chain_vest = item;

            //Free Chain Arms
            item = new ItemTemplate
            {
                Id_nb = "free_chain_arms",
                Name = "Chain Arms",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_chain_arms = item;

            //Free Chain Legs
            item = new ItemTemplate
            {
                Id_nb = "free_chain_legs",
                Name = "Chain Legs",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_chain_legs = item;


            //Free Chain Boots
            item = new ItemTemplate
            {
                Id_nb = "free_chain_boots",
                Name = "Chain Boots",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_chain_boots = item;


            //Free Chain Gloves
            item = new ItemTemplate
            {
                Id_nb = "free_chain_gloves",
                Name = "Chain Gloves",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_chain_gloves = item;


            //Free Chain Helm
            item = new ItemTemplate
            {
                Id_nb = "free_chain_helm",
                Name = "Chain Helm",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_chain_helm = item;

            #endregion Free Chain Armor
            #region Free Cloth Armor

            //Free Cloth Vest
            item = new ItemTemplate
            {
                Id_nb = "free_cloth_vest",
                Name = "Cloth Vest",
                Level = 20,
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
            free_cloth_vest = item;

            //Free Cloth Arms
            item = new ItemTemplate
            {
                Id_nb = "free_cloth_arms",
                Name = "Free Cloth Arms",
                Level = 20,
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
            free_cloth_arms = item;

            //Free Cloth Legs
            item = new ItemTemplate
            {
                Id_nb = "free_cloth_legs",
                Name = "Cloth Legs",
                Level = 20,
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
            free_cloth_legs = item;


            //Free Cloth Boots
            item = new ItemTemplate
            {
                Id_nb = "free_cloth_boots",
                Name = "Cloth Boots",
                Level = 20,
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
            free_cloth_boots = item;


            //Free Cloth Gloves
            item = new ItemTemplate
            {
                Id_nb = "free_cloth_gloves",
                Name = "Cloth Gloves",
                Level = 20,
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
            free_cloth_gloves = item;


            //Free Cloth Helm
            item = new ItemTemplate
            {
                Id_nb = "free_cloth_helm",
                Name = "Cloth Helm",
                Level = 20,
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
            free_cloth_helm = item;

            #endregion Free Cloth Armor
            #region Free Leather Armor

            //Free Leather Vest
            item = new ItemTemplate
            {
                Id_nb = "free_leather_vest",
                Name = "Leather Vest",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_leather_vest = item;

            //Free Leather Arms
            item = new ItemTemplate
            {
                Id_nb = "free_leather_arms",
                Name = "Leather Arms",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_leather_arms = item;

            //Free Leather Legs
            item = new ItemTemplate
            {
                Id_nb = "free_leather_legs",
                Name = "Leather Legs",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_leather_legs = item;


            //Free Leather Boots
            item = new ItemTemplate
            {
                Id_nb = "free_leather_boots",
                Name = "Leather Boots",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_leather_boots = item;


            //Free Leather Gloves
            item = new ItemTemplate
            {
                Id_nb = "free_leather_gloves",
                Name = "Leather Gloves",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_leather_gloves = item;


            //Free Leather Helm
            item = new ItemTemplate
            {
                Id_nb = "free_leather_helm",
                Name = "Leather Helm",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_leather_helm = item;

            #endregion Free Leather Armor
            #region Free Studded Armor

            //Free Studded Vest
            item = new ItemTemplate
            {
                Id_nb = "free_studded_vest",
                Name = "Studded Vest",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_studded_vest = item;

            //Free Studded Arms
            item = new ItemTemplate
            {
                Id_nb = "free_studded_arms",
                Name = "Studded Arms",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_studded_arms = item;

            //Free Studded Legs
            item = new ItemTemplate
            {
                Id_nb = "free_studded_legs",
                Name = "Studded Legs",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_studded_legs = item;


            //Free Studded Boots
            item = new ItemTemplate
            {
                Id_nb = "free_studded_boots",
                Name = "Studded Boots",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_studded_boots = item;


            //Free Studded Gloves
            item = new ItemTemplate
            {
                Id_nb = "free_studded_gloves",
                Name = "Studded Gloves",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_studded_gloves = item;


            //Free Studded Helm
            item = new ItemTemplate
            {
                Id_nb = "free_studded_helm",
                Name = "Studded Helm",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_studded_helm = item;

            #endregion Free Studded Armor
            #region Free Plate Armor

            //Free Plate Vest
            item = new ItemTemplate
            {
                Id_nb = "free_plate_vest",
                Name = "Plate Vest",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_plate_vest = item;

            //Free Plate Arms
            item = new ItemTemplate
            {
                Id_nb = "free_plate_arms",
                Name = "Plate Arms",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_plate_arms = item;

            //Free Plate Legs
            item = new ItemTemplate
            {
                Id_nb = "free_plate_legs",
                Name = "Plate Legs",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_plate_legs = item;


            //Free Plate Boots
            item = new ItemTemplate
            {
                Id_nb = "free_plate_boots",
                Name = "Plate Boots",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_plate_boots = item;


            //Free Plate Gloves
            item = new ItemTemplate
            {
                Id_nb = "free_plate_gloves",
                Name = "Plate Gloves",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_plate_gloves = item;


            //Free Plate Helm
            item = new ItemTemplate
            {
                Id_nb = "free_plate_helm",
                Name = "Plate Helm",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 40,
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
            free_plate_helm = item;

            #endregion Free Plate Armor

            #endregion Armor
            #region Weapons

            #region Free Staff (Caster)
            //free caster staff
            item = new ItemTemplate
            {
                Id_nb = "free_staff",
                Name = "Staff",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
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
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_staff = item;
            #endregion Free Staff (Caster)
            #region Free Slash Alb (sword) Left hand
            //free slash (sword)
            item = new ItemTemplate
            {
                Id_nb = "free_slash_alb",
                Name = "Slash Alb",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 28,
                Hand = 0,
                Type_Damage = 2,
                Object_Type = 3,
                Item_Type = 11,
                Weight = 35,
                Model = 1017,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_slash_alb = item;
            #endregion Free Slash Alb (sword) Left hand
            #region Free Slash Mid (sword) Left hand
            //free slash (sword)
            item = new ItemTemplate
            {
                Id_nb = "free_slash_mid",
                Name = "Slash Mid",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 28,
                Hand = 0,
                Type_Damage = 2,
                Object_Type = 11,
                Item_Type = 11,
                Weight = 35,
                Model = 1017,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_slash_mid = item;
            #endregion Free Slash Mid (sword) Left hand
            #region Free Slash Hib (sword) Left hand
            //free slash (sword)
            item = new ItemTemplate
            {
                Id_nb = "free_slash_hib",
                Name = "Slash hib",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 28,
                Hand = 0,
                Type_Damage = 2,
                Object_Type = 19,
                Item_Type = 11,
                Weight = 35,
                Model = 1017,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_slash_hib = item;
            #endregion Free Slash Hib (sword) Left hand
            #region Free Thrust Alb (Pierce) Left hand
            item = new ItemTemplate
            {
                Id_nb = "free_thrust_alb",
                Name = "Thrust Alb",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 28,
                Hand = 0,
                Type_Damage = 3,
                Object_Type = 4,
                Item_Type = 11,
                Weight = 35,
                Model = 2684,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_thrust_alb = item;
            #endregion Free Thrust Alb (Pierce) Left hand
            #region Free Thrust Hib (Pierce) Left hand
            item = new ItemTemplate
            {
                Id_nb = "free_thrust_hib",
                Name = "Thrust Hib",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 28,
                Hand = 0,
                Type_Damage = 3,
                Object_Type = 21,
                Item_Type = 11,
                Weight = 35,
                Model = 2684,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_thrust_hib = item;
            #endregion Free Thrust Hib (Pierce) Left hand
            #region Free Crush Alb (blunt) Left hand
            item = new ItemTemplate
            {
                Id_nb = "free_crush_alb",
                Name = "Crush Alb",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 28,
                Hand = 0,
                Type_Damage = 1,
                Object_Type = 2,
                Item_Type = 11,
                Weight = 35,
                Model = 1009,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_crush_alb = item;
            #endregion Free Crush Alb (blunt) Left hand
            #region Free Crush Mid (blunt) Left hand
            item = new ItemTemplate
            {
                Id_nb = "free_crush_mid",
                Name = "Crush Mid",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 28,
                Hand = 0,
                Type_Damage = 1,
                Object_Type = 12,
                Item_Type = 11,
                Weight = 35,
                Model = 1009,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_crush_mid = item;
            #endregion Free Crush Mid (blunt) Left hand
            #region Free Crush Hib (blunt) Left hand
            item = new ItemTemplate
            {
                Id_nb = "free_crush_hib",
                Name = "Crush Hib",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 28,
                Hand = 0,
                Type_Damage = 1,
                Object_Type = 20,
                Item_Type = 11,
                Weight = 35,
                Model = 1009,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_crush_hib = item;
            #endregion Free Crush Hib (blunt) Left hand
            #region Free Axe Left hand
            item = new ItemTemplate
            {
                Id_nb = "free_axe",
                Name = "Axe",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 35,
                Hand = 0,
                Type_Damage = 2,
                Object_Type = 17,
                Item_Type = 11,
                Weight = 35,
                Model = 1010,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_axe = item;
            #endregion Free Axe Left hand
            #region Free Long Bow (Scout)
            item = new ItemTemplate
            {
                Id_nb = "free_long_bow",
                Name = "Long Bow",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 54,
                Hand = 1,
                Type_Damage = 3,
                Object_Type = 9,
                Item_Type = 13,
                Weight = 40,
                Model = 849,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true,
                AllowedClasses = "3" //scout
            };
            free_long_bow = item;
            #endregion Free Long Bow (Scout)
            #region Free Composite Bow (Hunter)
            item = new ItemTemplate
            {
                Id_nb = "free_composite_bow",
                Name = "Composite Bow",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
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
                CanDropAsLoot = true,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true,
                AllowedClasses = "25" //hunter
            };
            free_composite_bow = item;
            #endregion Free Composite Bow (Hunter)
            #region Free Recurved Bow (Ranger)
            item = new ItemTemplate
            {
                Id_nb = "free_recurved_bow",
                Name = "Recurved Bow",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 54,
                Hand = 1,
                Type_Damage = 3,
                Object_Type = 18,
                Item_Type = 13,
                Weight = 40,
                Model = 925,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true,
                AllowedClasses = "50"//ranger
            };
            free_recurved_bow = item;
            #endregion Free Recurved Bow (Ranger)
            #region Free Short Bow
            item = new ItemTemplate
            {
                Id_nb = "free_short_bow",
                Name = "Short Bow",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 40,
                Hand = 1,
                Type_Damage = 3,
                Object_Type = 5,
                Item_Type = 13,
                Weight = 40,
                Model = 922,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_short_bow = item;
            #endregion Free Short Bow
            #region Free Crossbow
            item = new ItemTemplate
            {
                Id_nb = "free_crossbow",
                Name = "Crossbow",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 40,
                Type_Damage = 3,
                Object_Type = 10,
                Item_Type = 13,
                Weight = 40,
                Model = 226,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_crossbow = item;
            #endregion Free Crossbow
            #region Free TwoHanded Slash Alb
            item = new ItemTemplate
            {
                Id_nb = "free_twohanded_slash_alb",
                Name = "TwoHanded Slash Alb",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 51,
                Hand = 1,
                Type_Damage = 2,
                Object_Type = 6,
                Item_Type = 12,
                Weight = 50,
                Model = 907,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_twohanded_slash_alb = item;
            #endregion Free Twohanded Slash Alb
            #region Free TwoHanded Slash Hib
            item = new ItemTemplate
            {
                Id_nb = "free_twohanded_slash_hib",
                Name = "TwoHanded Slash Hib",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 51,
                Hand = 1,
                Type_Damage = 2,
                Object_Type = 22,
                Item_Type = 12,
                Weight = 50,
                Model = 907,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_twohanded_slash_hib = item;
            #endregion Free Twohanded Slash Hib
            #region Free TwoHanded Crush Alb
            item = new ItemTemplate
            {
                Id_nb = "free_twohanded_crush_alb",
                Name = "TwoHanded Crush Alb",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 48,
                Hand = 1,
                Type_Damage = 1,
                Object_Type = 6,
                Item_Type = 12,
                Weight = 50,
                Model = 574,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_twohanded_crush_alb = item;
            #endregion Free Twohanded Crush Alb
            #region Free TwoHanded Crush Hib
            item = new ItemTemplate
            {
                Id_nb = "free_twohanded_crush_hib",
                Name = "TwoHanded Crush Hib",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 48,
                Hand = 1,
                Type_Damage = 1,
                Object_Type = 22,
                Item_Type = 12,
                Weight = 50,
                Model = 574,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_twohanded_crush_hib = item;
            #endregion Free Twohanded Crush Hib
            #region Free TwoHanded Axe
            item = new ItemTemplate
            {
                Id_nb = "free_twohanded_axe",
                Name = "TwoHanded Axe",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 51,
                Hand = 1,
                Type_Damage = 2,
                Object_Type = 13,
                Item_Type = 12,
                Weight = 50,
                Model = 577,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_twohanded_axe = item;
            #endregion Free TwoHanded Axe
            #region Free TwoHanded Sword
            item = new ItemTemplate
            {
                Id_nb = "free_twohanded_sword",
                Name = "TwoHanded Sword",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 56,
                Hand = 1,
                Type_Damage = 2,
                Object_Type = 11,
                Item_Type = 12,
                Weight = 50,
                Model = 658,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_twohanded_sword = item;
            #endregion Free TwoHanded Sword
            #region Free TwoHanded Hammer
            item = new ItemTemplate
            {
                Id_nb = "free_twohanded_hammer",
                Name = "TwoHanded Hammer",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 48,
                Hand = 1,
                Type_Damage = 1,
                Object_Type = 12,
                Item_Type = 12,
                Weight = 50,
                Model = 574,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_twohanded_hammer = item;
            #endregion Free TwoHanded Hammer
            #region Free Small Shield
            item = new ItemTemplate
            {
                Id_nb = "free_small_shield",
                Name = "Small Shield",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 28,
                Hand = 2,
                Type_Damage = 1,
                Object_Type = 42,
                Item_Type = 11,
                Weight = 35,
                Model = 2218,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_small_shield = item;
            #endregion Free Small Shield
            #region Free Medium Shield
            item = new ItemTemplate
            {
                Id_nb = "free_medium_shield",
                Name = "Medium Shield",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 32,
                Hand = 2,
                Type_Damage = 2,
                Object_Type = 42,
                Item_Type = 11,
                Weight = 35,
                Model = 2219,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_medium_shield = item;
            #endregion Free Medium Shield
            #region Free Large Shield
            item = new ItemTemplate
            {
                Id_nb = "free_large_shield",
                Name = "Large Shield",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 40,
                Hand = 2,
                Type_Damage = 3,
                Object_Type = 42,
                Item_Type = 11,
                Weight = 35,
                Model = 2220,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_large_shield = item;
            #endregion Free Large Shield
            #region Free Slash Spear
            item = new ItemTemplate
            {
                Id_nb = "free_slash_spear",
                Name = "Slash Spear",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 40,
                Hand = 1,
                Type_Damage = 2,
                Object_Type = 14,
                Item_Type = 12,
                Weight = 50,
                Model = 657,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_slash_spear = item;
            #endregion Free Slash Spear
            #region Free Thrust Spear
            item = new ItemTemplate
            {
                Id_nb = "free_thrust_spear",
                Name = "Thrust Spear",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
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
            free_thrust_spear = item;
            #endregion Free Thrust Spear
            #region Free Slash Flail
            item = new ItemTemplate
            {
                Id_nb = "free_slash_flail",
                Name = "Slash Flail",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
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
            free_slash_flail = item;
            #endregion Free Slash Flail
            #region Free Thrust Flail
            item = new ItemTemplate
            {
                Id_nb = "free_thrust_flail",
                Name = "Thrust Flail",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
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
            free_thrust_flail = item;
            #endregion Free Thrust Flail
            #region Free Crush Flail
            item = new ItemTemplate
            {
                Id_nb = "free_crush_flail",
                Name = "Crush Flail",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 35,
                Type_Damage = 1,
                Object_Type = 24,
                Item_Type = 10,
                Weight = 35,
                Model = 861,
                Bonus = 15,
                IsPickable = true,
                CraftPriceSet = false,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_crush_flail = item;
            #endregion Free Crush Flail
            #region Free Slash Claw
            item = new ItemTemplate
            {
                Id_nb = "free_slash_claw",
                Name = "Slash Claw",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 30,
                Hand = 2,
                Type_Damage = 2,
                Object_Type = 25,
                Item_Type = 11,
                Weight = 35,
                Model = 967,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_slash_claw = item;
            #endregion Free Slash Claw
            #region Free Thrust Claw
            item = new ItemTemplate
            {
                Id_nb = "free_thrust_claw",
                Name = "Thrust Claw",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 30,
                Hand = 2,
                Type_Damage = 3,
                Object_Type = 25,
                Item_Type = 11,
                Weight = 35,
                Model = 967,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_thrust_claw = item;
            #endregion Free Thrust Claw
            #region Free Harp
            item = new ItemTemplate
            {
                Id_nb = "free_harp",
                Name = "Harp",
                Level = 20,
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
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_harp = item;
            #endregion Free Harp

            #region Free Mauler Staff
            item = new ItemTemplate
            {
                Id_nb = "free_mauler_staff",
                Name = "Mauler Staff",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 30,
                Hand = 1,
                Type_Damage = 1,
                Object_Type = 28,
                Item_Type = 12,
                Weight = 50,
                Model = 1950,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_mauler_staff = item;
            #endregion Free Mauler Staff

            #region Free Mauler Right Wrap
            item = new ItemTemplate
            {
                Id_nb = "free_mauler_right_wrap",
                Name = "Mauler Right Wrap",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 33,
                Hand = 0,
                Type_Damage = 1,
                Object_Type = 27,
                Item_Type = 10,
                Weight = 35,
                Model = 3550,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_mauler_right_wrap = item;
            #endregion Free Mauler Right Wrap

            #region Free Mauler Left Wrap
            item = new ItemTemplate
            {
                Id_nb = "free_mauler_left_wrap",
                Name = "Mauler Left Wrap",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 33,
                Hand = 0,
                Type_Damage = 1,
                Object_Type = 27,
                Item_Type = 11,
                Weight = 35,
                Model = 3550,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_mauler_left_wrap = item;
            #endregion Free Mauler Left Wrap

            #region Free Scythe
            item = new ItemTemplate
            {
                Id_nb = "free_scythe",
                Name = "Scythe",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 37,
                Hand = 1,
                Type_Damage = 2,
                Object_Type = 26,
                Item_Type = 12,
                Weight = 50,
                Model = 929,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_scythe = item;
            #endregion Free Scythe
            #region Free Crush Polearm
            item = new ItemTemplate
            {
                Id_nb = "free_crush_polearm",
                Name = "Crush Polearm",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 56,
                Hand = 1,
                Type_Damage = 1,
                Object_Type = 7,
                Item_Type = 12,
                Weight = 50,
                Model = 2664,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_crush_polearm = item;
            #endregion Free Crush Polearm
            #region Free Thrust Polearm
            item = new ItemTemplate
            {
                Id_nb = "free_thrust_polearm",
                Name = "Thrust Polearm",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 43,
                Hand = 1,
                Type_Damage = 3,
                Object_Type = 7,
                Item_Type = 12,
                Weight = 50,
                Model = 2665,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_thrust_polearm = item;
            #endregion Free Thrust Polearm
            #region Free Slash Polearm
            item = new ItemTemplate
            {
                Id_nb = "free_slash_polearm",
                Name = "Slash Polearm",
                Level = 20,
                Durability = 50000,
                MaxDurability = 50000,
                Condition = 50000,
                MaxCondition = 50000,
                Quality = 95,
                DPS_AF = 72,
                SPD_ABS = 48,
                Hand = 1,
                Type_Damage = 2,
                Object_Type = 7,
                Item_Type = 12,
                Weight = 50,
                Model = 2663,
                Bonus = 15,
                IsPickable = true,
                IsDropable = true,
                CanDropAsLoot = false,
                IsTradable = false,
                IsIndestructible = true,
                MaxCount = 1,
                PackSize = 1,
                AllowAdd = true
            };
            free_slash_polearm = item;
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
                        GiveItem(player, free_chain_vest);
                        GiveItem(player, free_chain_legs);
                        GiveItem(player, free_chain_boots);
                        GiveItem(player, free_chain_arms);
                        GiveItem(player, free_chain_gloves);
                        free_chain_helm.Model = realmHelmModel;
                        GiveItem(player, free_chain_helm);
                        break;
                    }
                case "cloth":
                    {
                        GiveItem(player, free_cloth_vest);
                        GiveItem(player, free_cloth_legs);
                        GiveItem(player, free_cloth_boots);
                        GiveItem(player, free_cloth_arms);
                        GiveItem(player, free_cloth_gloves);
                        free_cloth_helm.Model = realmHelmModel;
                        GiveItem(player, free_cloth_helm);
                        break;
                    }
                case "leather":
                    {
                        GiveItem(player, free_leather_vest);
                        GiveItem(player, free_leather_legs);
                        GiveItem(player, free_leather_boots);
                        GiveItem(player, free_leather_arms);
                        GiveItem(player, free_leather_gloves);
                        free_leather_helm.Model = realmHelmModel;
                        GiveItem(player, free_leather_helm);
                        break;
                    }
                case "reinforced": //reinforced gets studded armor instead of reinforced
                case "studded":
                    {
                        GiveItem(player, free_studded_vest);
                        GiveItem(player, free_studded_legs);
                        GiveItem(player, free_studded_boots);
                        GiveItem(player, free_studded_arms);
                        GiveItem(player, free_studded_gloves);
                        free_studded_helm.Model = realmHelmModel;
                        GiveItem(player, free_studded_helm);
                        break;
                    }
                case "plate":
                    {
                        GiveItem(player, free_plate_vest);
                        GiveItem(player, free_plate_legs);
                        GiveItem(player, free_plate_boots);
                        GiveItem(player, free_plate_arms);
                        GiveItem(player, free_plate_gloves);
                        free_plate_helm.Model = realmHelmModel;
                        GiveItem(player, free_plate_helm);
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
                        EquipGiveItem(player, free_chain_vest, eInventorySlot.TorsoArmor);
                        EquipGiveItem(player, free_chain_legs, eInventorySlot.LegsArmor);
                        EquipGiveItem(player, free_chain_boots, eInventorySlot.FeetArmor);
                        EquipGiveItem(player, free_chain_arms, eInventorySlot.ArmsArmor);
                        EquipGiveItem(player, free_chain_gloves, eInventorySlot.HandsArmor);
                        free_chain_helm.Model = realmHelmModel;
                        EquipGiveItem(player, free_chain_helm, eInventorySlot.HeadArmor);
                        break;
                    }
                case "cloth":
                    {
                        EquipGiveItem(player, free_cloth_vest, eInventorySlot.TorsoArmor);
                        EquipGiveItem(player, free_cloth_legs, eInventorySlot.LegsArmor);
                        EquipGiveItem(player, free_cloth_boots, eInventorySlot.FeetArmor);
                        EquipGiveItem(player, free_cloth_arms, eInventorySlot.ArmsArmor);
                        EquipGiveItem(player, free_cloth_gloves, eInventorySlot.HandsArmor);
                        free_cloth_helm.Model = realmHelmModel;
                        EquipGiveItem(player, free_cloth_helm, eInventorySlot.HeadArmor);
                        break;
                    }
                case "leather":
                    {
                        EquipGiveItem(player, free_leather_vest, eInventorySlot.TorsoArmor);
                        EquipGiveItem(player, free_leather_legs, eInventorySlot.LegsArmor);
                        EquipGiveItem(player, free_leather_boots, eInventorySlot.FeetArmor);
                        EquipGiveItem(player, free_leather_arms, eInventorySlot.ArmsArmor);
                        EquipGiveItem(player, free_leather_gloves, eInventorySlot.HandsArmor);
                        free_leather_helm.Model = realmHelmModel;
                        EquipGiveItem(player, free_leather_helm, eInventorySlot.HeadArmor);
                        break;
                    }
                case "reinforced": //reinforced gets studded armor instead of reinforced
                case "studded":
                    {
                        EquipGiveItem(player, free_studded_vest, eInventorySlot.TorsoArmor);
                        EquipGiveItem(player, free_studded_legs, eInventorySlot.LegsArmor);
                        EquipGiveItem(player, free_studded_boots, eInventorySlot.FeetArmor);
                        EquipGiveItem(player, free_studded_arms, eInventorySlot.ArmsArmor);
                        EquipGiveItem(player, free_studded_gloves, eInventorySlot.HandsArmor);
                        free_studded_helm.Model = realmHelmModel;
                        EquipGiveItem(player, free_studded_helm, eInventorySlot.HeadArmor);
                        break;
                    }
                case "plate":
                    {
                        EquipGiveItem(player, free_plate_vest, eInventorySlot.TorsoArmor);
                        EquipGiveItem(player, free_plate_legs, eInventorySlot.LegsArmor);
                        EquipGiveItem(player, free_plate_boots, eInventorySlot.FeetArmor);
                        EquipGiveItem(player, free_plate_arms, eInventorySlot.ArmsArmor);
                        EquipGiveItem(player, free_plate_gloves, eInventorySlot.HandsArmor);
                        free_plate_helm.Model = realmHelmModel;
                        EquipGiveItem(player, free_plate_helm, eInventorySlot.HeadArmor);
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
            if (player.HasAbilityToUseItem(free_staff)) { GiveItem(player, free_staff); }
            if (player.HasAbilityToUseItem(free_slash_alb)) { GiveItem(player, free_slash_alb); }
            if (player.HasAbilityToUseItem(free_slash_mid)) { GiveItem(player, free_slash_mid); }
            if (player.HasAbilityToUseItem(free_slash_hib)) { GiveItem(player, free_slash_hib); }
            if (player.HasAbilityToUseItem(free_thrust_alb)) { GiveItem(player, free_thrust_alb); }
            if (player.HasAbilityToUseItem(free_thrust_hib)) { GiveItem(player, free_thrust_hib); }
            if (player.HasAbilityToUseItem(free_crush_alb)) { GiveItem(player, free_crush_alb); }
            if (player.HasAbilityToUseItem(free_crush_mid)) { GiveItem(player, free_crush_mid); }
            if (player.HasAbilityToUseItem(free_crush_hib)) { GiveItem(player, free_crush_hib); }
            if (player.HasAbilityToUseItem(free_axe)) { GiveItem(player, free_axe); }
            if (player.HasAbilityToUseItem(free_long_bow)) { GiveItem(player, free_long_bow); }
            if (player.HasAbilityToUseItem(free_composite_bow)) { GiveItem(player, free_composite_bow); }
            if (player.HasAbilityToUseItem(free_recurved_bow)) { GiveItem(player, free_recurved_bow); }
            if (player.HasAbilityToUseItem(free_short_bow)) { GiveItem(player, free_short_bow); }
            if (player.HasAbilityToUseItem(free_crossbow)) { GiveItem(player, free_crossbow); }
            if (player.HasAbilityToUseItem(free_twohanded_slash_alb)) { GiveItem(player, free_twohanded_slash_alb); }
            if (player.HasAbilityToUseItem(free_twohanded_slash_hib)) { GiveItem(player, free_twohanded_slash_hib); }
            if (player.HasAbilityToUseItem(free_twohanded_crush_alb)) { GiveItem(player, free_twohanded_crush_alb); }
            if (player.HasAbilityToUseItem(free_twohanded_crush_hib)) { GiveItem(player, free_twohanded_crush_hib); }
            if (player.HasAbilityToUseItem(free_twohanded_axe)) { GiveItem(player, free_twohanded_axe); }
            if (player.HasAbilityToUseItem(free_twohanded_sword)) { GiveItem(player, free_twohanded_sword); }
            if (player.HasAbilityToUseItem(free_twohanded_hammer)) { GiveItem(player, free_twohanded_hammer); }
            if (player.HasAbilityToUseItem(free_large_shield)) { GiveItem(player, free_large_shield); }
            else if (player.HasAbilityToUseItem(free_medium_shield)) { GiveItem(player, free_medium_shield); }
            else if (player.HasAbilityToUseItem(free_small_shield)) { GiveItem(player, free_small_shield); }
            if (player.HasAbilityToUseItem(free_slash_spear)) { GiveItem(player, free_slash_spear); }
            if (player.HasAbilityToUseItem(free_thrust_spear)) { GiveItem(player, free_thrust_spear); }
            if (player.HasAbilityToUseItem(free_slash_flail)) { GiveItem(player, free_slash_flail); }
            if (player.HasAbilityToUseItem(free_thrust_flail)) { GiveItem(player, free_thrust_flail); }
            if (player.HasAbilityToUseItem(free_crush_flail)) { GiveItem(player, free_crush_flail); }
            if (player.HasAbilityToUseItem(free_slash_claw)) { GiveItem(player, free_slash_claw); }
            if (player.HasAbilityToUseItem(free_thrust_claw)) { GiveItem(player, free_thrust_claw); }
            if (player.HasAbilityToUseItem(free_harp)) { GiveItem(player, free_harp); }
            if (player.HasAbilityToUseItem(free_mauler_staff)) { GiveItem(player, free_mauler_staff); }
            if (player.HasAbilityToUseItem(free_mauler_right_wrap)) { GiveItem(player, free_mauler_right_wrap); }
            if (player.HasAbilityToUseItem(free_mauler_left_wrap)) { GiveItem(player, free_mauler_left_wrap); }
            if (player.HasAbilityToUseItem(free_scythe)) { GiveItem(player, free_scythe); }
            if (player.HasAbilityToUseItem(free_crush_polearm)) { GiveItem(player, free_crush_polearm); }
            if (player.HasAbilityToUseItem(free_thrust_polearm)) { GiveItem(player, free_thrust_polearm); }
            if (player.HasAbilityToUseItem(free_slash_polearm)) { GiveItem(player, free_slash_polearm); }

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
            //InventoryItem item = GameInventoryItem.Create<ItemTemplate>(itemTemplate);
            ItemTemplate generic = GameServer.Database.FindObjectByKey<ItemTemplate>(itemTemplate.Id_nb);
            InventoryItem temp = new GameInventoryItem(generic);
            // ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("alvarus_leggings_leather");

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


            if (player.Level < 20)
            {
                player.Out.SendMessage("Hello i have first stuff for level 20 beginners only " + player.Name + ", if you have reached level 20 come back to me my young recrut!.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return false;
            }
            if (player.HasFreeStartArmor && player.HasFreeStartWeapons || player.HasFreeStartArmor40 && player.HasFreeStartWeapons40)
            {
                player.Out.SendMessage("hm, You already have all Items! ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return false;
            }

            player.Out.SendMessage("Hello " + player.Name + ", I am able to give out free [Armor] and [Weapons].", eChatType.CT_Say, eChatLoc.CL_PopupWindow);



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
                    if (player.HasFreeStartArmor || player.HasFreeStartArmor40)
                    {
                        player.Out.SendMessage("Sorry, You already have this items! ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        return false;
                    }
                    else
                    {
                        EquipFreeArmor(player);
                        player.HasFreeStartArmor = true;
                    }
                }
                catch (Exception ex)
                {
                    player.Out.SendMessage(ex.Message, eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    GiveFreeArmor(player);
                    player.HasFreeStartArmor = true;
                }
                return true;
            }
            if (str == "Weapons")
            {
                if (player.HasFreeStartWeapons || player.HasFreeStartArmor40)
                {
                    player.Out.SendMessage("Sorry, You already have this items! ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    return false;
                }
                else
                {
                    GiveFreeWeapons(player);
                    player.HasFreeStartWeapons = true;
                    return true;
                }
            }
            return true;
        }
    }
}

	
