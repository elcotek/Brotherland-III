/*
*Author         : Etaew - Fallen Realms
*Editor         : Gandulf
*Source         : http://camelot.allakhazam.com
*Date           : 8 December 2004
*Quest Name     : Feast of the Decadent (level 50)
*Quest Classes  : Theurgist, Armsman, Scout, and Friar (Defenders of Albion)
*Quest Version  : v1
*
*ToDo:
*   Add Bonuses to Epic Items
*   Add correct Text
*   Find Helm ModelID for epics..
*/

using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Albion
{
	public class Defenders_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "Feast of the Decadent";
		protected const int minimumLevel = 48;
		protected const int maximumLevel = 50;

		private static GameNPC Lidmann = null; // Start NPC
		private static GameNPC Uragaig = null; // Mob to kill

		private static ItemTemplate sealed_pouch = null; //sealed pouch
		private static ItemTemplate ScoutEpicBoots = null; //Brigandine of Vigilant Defense  Boots 
		private static ItemTemplate ScoutEpicHelm = null; //Brigandine of Vigilant Defense  Coif 
		private static ItemTemplate ScoutEpicGloves = null; //Brigandine of Vigilant Defense  Gloves 
		private static ItemTemplate ScoutEpicVest = null; //Brigandine of Vigilant Defense  Hauberk 
		private static ItemTemplate ScoutEpicLegs = null; //Brigandine of Vigilant Defense  Legs 
		private static ItemTemplate ScoutEpicArms = null; //Brigandine of Vigilant Defense  Sleeves 
		private static ItemTemplate ArmsmanEpicBoots = null; //Shadow Shrouded Boots 
		private static ItemTemplate ArmsmanEpicHelm = null; //Shadow Shrouded Coif 
		private static ItemTemplate ArmsmanEpicGloves = null; //Shadow Shrouded Gloves 
		private static ItemTemplate ArmsmanEpicVest = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate ArmsmanEpicLegs = null; //Shadow Shrouded Legs 
		private static ItemTemplate ArmsmanEpicArms = null; //Shadow Shrouded Sleeves 
		private static ItemTemplate TheurgistEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate TheurgistEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate TheurgistEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate TheurgistEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate TheurgistEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate TheurgistEpicArms = null; //Valhalla Touched Sleeves 
		private static ItemTemplate FriarEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate FriarEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate FriarEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate FriarEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate FriarEpicLegs = null; //Subterranean Legs
		private static ItemTemplate FriarEpicArms = null; //Subterranean Sleeves
		private static ItemTemplate MaulerAlbEpicBoots = null;
		private static ItemTemplate MaulerAlbEpicHelm = null;
		private static ItemTemplate MaulerAlbEpicGloves = null;
		private static ItemTemplate MaulerAlbEpicVest = null;
		private static ItemTemplate MaulerAlbEpicLegs = null;
		private static ItemTemplate MaulerAlbEpicArms = null;

		// Constructors
		public Defenders_50() : base()
		{
		}

		public Defenders_50(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public Defenders_50(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Defenders_50(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
		{
		}

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");

			#region defineNPCs

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Lidmann Halsey", eRealm.Albion);

			if (npcs.Length == 0)
			{

                Lidmann = new GameNPC
                {
                    Model = 64,
                    Name = "Lidmann Halsey"
                };

                if (log.IsWarnEnabled)
					log.Warn("Could not find " + Lidmann.Name + ", creating it ...");

				Lidmann.GuildName = String.Empty;
				Lidmann.Realm = eRealm.Albion;
				Lidmann.CurrentRegionID = 1;
				Lidmann.Size = 50;
				Lidmann.Level = 50;
				Lidmann.X = 466464;
				Lidmann.Y = 634554;
				Lidmann.Z = 1954;
				Lidmann.Heading = 1809;
				Lidmann.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Lidmann.SaveIntoDatabase();
				}

			}
			else
				Lidmann = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Cailleach Uragaig", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Uragaig , creating it ...");
                Uragaig = new GameNPC
                {
                    Model = 349,
                    Name = "Cailleach Uragaig",
                    GuildName = String.Empty,
                    Realm = eRealm.None,
                    CurrentRegionID = 1,
                    Size = 55,
                    Level = 70,
                    X = 316218,
                    Y = 664484,
                    Z = 2736,
                    Heading = 3072
                };
                Uragaig.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Uragaig.SaveIntoDatabase();
				}

			}
			else
				Uragaig = npcs[0];
			// end npc

			#endregion

			#region defineItems

			sealed_pouch = GameServer.Database.FindObjectByKey<ItemTemplate>("sealed_pouch");
			if (sealed_pouch == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Sealed Pouch , creating it ...");
                sealed_pouch = new ItemTemplate
                {
                    Id_nb = "sealed_pouch",
                    Name = "Sealed Pouch",
                    Level = 8,
                    Item_Type = 29,
                    Model = 488,
                    IsDropable = false,
                    IsPickable = false,
                    DPS_AF = 0,
                    SPD_ABS = 0,
                    Object_Type = 41,
                    Hand = 0,
                    Type_Damage = 0,
                    Quality = 100,
                    Weight = 12
                };
                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(sealed_pouch);
				}

			}
			// end item
			ItemTemplate i = null;
			ScoutEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("ScoutEpicBoots");
			if (ScoutEpicBoots == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "ScoutEpicBoots",
                    Name = "Brigandine Boots of Vigilant Defense",
                    Level = 50,
                    Item_Type = 23,
                    Model = 731,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 19,
                    Object_Type = 34,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: Con +10, Dex +18, Qui +15, Spirit +8%
                    Bonus1 = 10,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 18,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 15,
                    Bonus3Type = (int)eStat.QUI,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Spirit
                };
                {
					GameServer.Database.AddObject(i);
				}
				ScoutEpicBoots = i;

			}
			//end item
			//Brigandine of Vigilant Defense  Coif
			ScoutEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("ScoutEpicHelm");
			if (ScoutEpicHelm == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "ScoutEpicHelm",
                    Name = "Brigandine Coif of Vigilant Defense",
                    Level = 50,
                    Item_Type = 21,
                    Model = 1290, //NEED TO WORK ON..
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 19,
                    Object_Type = 34,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: Dex +12, Qui +22, Crush +8%, Heat +8%
                    Bonus1 = 12,
                    Bonus1Type = (int)eStat.DEX,

                    Bonus2 = 22,
                    Bonus2Type = (int)eStat.QUI,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Crush,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Heat
                };
                {
					GameServer.Database.AddObject(i);
				}

				ScoutEpicHelm = i;

			}
			//end item
			//Brigandine of Vigilant Defense  Gloves
			ScoutEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("ScoutEpicGloves");
            if (ScoutEpicGloves == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "ScoutEpicGloves",
                    Name = "Brigandine Gloves of Vigilant Defense",
                    Level = 50,
                    Item_Type = 22,
                    Model = 732,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 19,
                    Object_Type = 34,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: Dex +21, Longbow +5, Body +8%, Slash +8%
                    Bonus1 = 21,
                    Bonus1Type = (int)eStat.DEX,

                    Bonus2 = 5,
                    Bonus2Type = (int)eProperty.Skill_Long_bows,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Body,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Slash
                };

                {
					GameServer.Database.AddObject(i);
				}

				ScoutEpicGloves = i;

			}
			//Brigandine of Vigilant Defense  Hauberk
			ScoutEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("ScoutEpicVest");
			if (ScoutEpicVest == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "ScoutEpicVest",
                    Name = "Brigandine Jerkin of Vigilant Defense",
                    Level = 50,
                    Item_Type = 25,
                    Model = 728,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 19,
                    Object_Type = 34,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: Str +18, HP +45, Spirit +4%, Thrust +4%
                    Bonus1 = 18,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 4,
                    Bonus2Type = (int)eResist.Thrust,

                    Bonus3 = 4,
                    Bonus3Type = (int)eResist.Spirit,

                    Bonus4 = 45,
                    Bonus4Type = (int)eProperty.MaxHealth
                };
                {
					GameServer.Database.AddObject(i);
				}

				ScoutEpicVest = i;

			}
			//Brigandine of Vigilant Defense  Legs
			ScoutEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("ScoutEpicLegs");
			if (ScoutEpicLegs == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "ScoutEpicLegs",
                    Name = "Brigandine Legs of Vigilant Defense",
                    Level = 50,
                    Item_Type = 27,
                    Model = 729,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 19,
                    Object_Type = 34,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: Con +22, Dex +15, Qui +7, Spirit +6%
                    Bonus1 = 22,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 7,
                    Bonus3Type = (int)eStat.QUI,

                    Bonus4 = 6,
                    Bonus4Type = (int)eResist.Spirit
                };
                {
					GameServer.Database.AddObject(i);
				}
				ScoutEpicLegs = i;

			}
			//Brigandine of Vigilant Defense  Sleeves
			ScoutEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("ScoutEpicArms");
			if (ScoutEpicArms == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "ScoutEpicArms",
                    Name = "Brigandine Sleeves of Vigilant Defense",
                    Level = 50,
                    Item_Type = 28,
                    Model = 730,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 19,
                    Object_Type = 34,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: Con +22, Str +18, Energy +8%, Slash +4%
                    Bonus1 = 22,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 18,
                    Bonus2Type = (int)eStat.STR,

                    Bonus3 = 4,
                    Bonus3Type = (int)eResist.Energy,

                    Bonus4 = 4,
                    Bonus4Type = (int)eResist.Slash
                };
                {
					GameServer.Database.AddObject(i);
				}

				ScoutEpicArms = i;

			}
			//Scout Epic Sleeves End

			//Armsman Epic Boots Start
			ArmsmanEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("ArmsmanEpicBoots");
			if (ArmsmanEpicBoots == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "ArmsmanEpicBoots",
                    Name = "Sabaton of the Stalwart Arm",
                    Level = 50,
                    Item_Type = 23,
                    Model = 692,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 34,
                    Object_Type = 36,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: Str +15, Qui +15, Spirit +8%
                    Bonus1 = 15,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.QUI,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Spirit
                };


                {
					GameServer.Database.AddObject(i);
				}
				ArmsmanEpicBoots = i;

			}
			//end item
			//of the Stalwart Arm Coif
			ArmsmanEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("ArmsmanEpicHelm");
			if (ArmsmanEpicHelm == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "ArmsmanEpicHelm",
                    Name = "Coif of the Stalwart Arm",
                    Level = 50,
                    Item_Type = 21,
                    Model = 1290, //NEED TO WORK ON..
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 34,
                    Object_Type = 36,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: Con +19, Qui +18, Body +6%, Crush +6%
                    Bonus1 = 19,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 18,
                    Bonus2Type = (int)eStat.QUI,

                    Bonus3 = 6,
                    Bonus3Type = (int)eResist.Body,

                    Bonus4 = 6,
                    Bonus4Type = (int)eResist.Crush
                };
                {
					GameServer.Database.AddObject(i);
				}

				ArmsmanEpicHelm = i;

			}
			//end item
			//of the Stalwart Arm Gloves
			ArmsmanEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("ArmsmanEpicGloves");
			if (ArmsmanEpicGloves == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "ArmsmanEpicGloves",
                    Name = "Gloves of the Stalwart Arm",
                    Level = 50,
                    Item_Type = 22,
                    Model = 691,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 34,
                    Object_Type = 36,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: Str +22, Dex +15, Cold +6%, Slash +6
                    Bonus1 = 22,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 6,
                    Bonus3Type = (int)eResist.Cold,

                    Bonus4 = 6,
                    Bonus4Type = (int)eResist.Slash
                };
                {
					GameServer.Database.AddObject(i);
				}

				ArmsmanEpicGloves = i;

			}
			//of the Stalwart Arm Hauberk
			ArmsmanEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("ArmsmanEpicVest");
			if (ArmsmanEpicVest == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "ArmsmanEpicVest",
                    Name = "Jerkin of the Stalwart Arm",
                    Level = 50,
                    Item_Type = 25,
                    Model = 688,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 34,
                    Object_Type = 36,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: HP +45, Str +18, Energy +4%, Slash +4%
                    // there is an additional bonus here I couldn't figure out how to add
                    // 3 charges of 75 point shield ???
                    Bonus1 = 18,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 4,
                    Bonus2Type = (int)eResist.Slash,

                    Bonus3 = 4,
                    Bonus3Type = (int)eResist.Energy,

                    Bonus4 = 45,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                {
					GameServer.Database.AddObject(i);
				}

				ArmsmanEpicVest = i;

			}
			//of the Stalwart Arm Legs
			ArmsmanEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("ArmsmanEpicLegs");
			if (ArmsmanEpicLegs == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "ArmsmanEpicLegs",
                    Name = "Legs of the Stalwart Arm",
                    Level = 50,
                    Item_Type = 27,
                    Model = 689,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 34,
                    Object_Type = 36,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: Con +24, Str +10, Matter +8%, Crush +8%
                    Bonus1 = 24,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 10,
                    Bonus2Type = (int)eStat.STR,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Matter,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Crush
                };
                {
					GameServer.Database.AddObject(i);
				}

				ArmsmanEpicLegs = i;

			}
			//of the Stalwart Arm Sleeves
			ArmsmanEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("ArmsmanEpicArms");
			if (ArmsmanEpicArms == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "ArmsmanEpicArms",
                    Name = "Sleeves of the Stalwart Arm",
                    Level = 50,
                    Item_Type = 28,
                    Model = 690,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 34,
                    Object_Type = 36,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: Con +19, Dex +18, Heat +6%, Thrust +6%
                    Bonus1 = 19,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 18,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 6,
                    Bonus3Type = (int)eResist.Heat,

                    Bonus4 = 6,
                    Bonus4Type = (int)eResist.Thrust
                };
                {
					GameServer.Database.AddObject(i);
				}

				ArmsmanEpicArms = i;

			}
			FriarEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("FriarEpicBoots");
			if (FriarEpicBoots == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "FriarEpicBoots",
                    Name = "Prayer-bound Boots",
                    Level = 50,
                    Item_Type = 23,
                    Model = 40,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 10,
                    Object_Type = 33,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: Qui +18, Dex +15, Spirit +10%, Con +12
                    Bonus1 = 18,
                    Bonus1Type = (int)eStat.QUI,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 12,
                    Bonus3Type = (int)eStat.CON,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Spirit
                };
                {
					GameServer.Database.AddObject(i);
				}

				FriarEpicBoots = i;

			}
			//end item
			//Prayer-bound Coif
			FriarEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("FriarEpicHelm");
			if (FriarEpicHelm == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "FriarEpicHelm",
                    Name = "Prayer-bound Coif",
                    Level = 50,
                    Item_Type = 21,
                    Model = 1290, //NEED TO WORK ON..
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 10,
                    Object_Type = 33,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: Dex +15, Pie +12, Con +10, Enchantment +4
                    Bonus1 = 15,
                    Bonus1Type = (int)eStat.DEX,

                    Bonus2 = 12,
                    Bonus2Type = (int)eStat.PIE,

                    Bonus3 = 10,
                    Bonus3Type = (int)eStat.CON,

                    Bonus4 = 4,
                    Bonus4Type = (int)eProperty.Skill_Enhancement //guessing here
                };
                {
					GameServer.Database.AddObject(i);
				}

				FriarEpicHelm = i;

			}
			//end item
			//Prayer-bound Gloves
			FriarEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("FriarEpicGloves");
			if (FriarEpicGloves == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "FriarEpicGloves",
                    Name = "Prayer-bound Gloves",
                    Level = 50,
                    Item_Type = 22,
                    Model = 39,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 10,
                    Object_Type = 33,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: Pie +15, Rejuvination +4, Qui +15, Crush +6%
                    Bonus1 = 15,
                    Bonus1Type = (int)eStat.PIE,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.QUI,

                    Bonus3 = 6,
                    Bonus3Type = (int)eResist.Crush,

                    Bonus4 = 6,
                    Bonus4Type = (int)eProperty.Skill_Rejuvenation //guessing here
                };
                {
					GameServer.Database.AddObject(i);
				}

				FriarEpicGloves = i;

			}
			//Prayer-bound Hauberk
			FriarEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("FriarEpicVest");
			if (FriarEpicVest == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "FriarEpicVest",
                    Name = "Prayer-bound Jerkin",
                    Level = 50,
                    Item_Type = 25,
                    Model = 797,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 10,
                    Object_Type = 33,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: HP +33, Pwr +10, Spirit 4%, Crush 6%
                    // Charged (3 Max) Self-Only Shield -- 75 AF, Duration 10 mins (no clue how to add this)
                    Bonus1 = 10,
                    Bonus1Type = (int)eProperty.MaxMana,

                    Bonus2 = 6,
                    Bonus2Type = (int)eResist.Crush,

                    Bonus3 = 4,
                    Bonus3Type = (int)eResist.Spirit,

                    Bonus4 = 33,
                    Bonus4Type = (int)eProperty.MaxHealth
                };
                {
					GameServer.Database.AddObject(i);
				}

				FriarEpicVest = i;

			}
			//Prayer-bound Legs
			FriarEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("FriarEpicLegs");
			if (FriarEpicLegs == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "FriarEpicLegs",
                    Name = "Prayer-bound Legs",
                    Level = 50,
                    Item_Type = 27,
                    Model = 37,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 10,
                    Object_Type = 33,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: Con +22, Str +15, Heat +6%, Slash +6%
                    Bonus1 = 15,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 22,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 6,
                    Bonus3Type = (int)eResist.Heat,

                    Bonus4 = 6,
                    Bonus4Type = (int)eResist.Slash
                };
                {
					GameServer.Database.AddObject(i);
				}

				FriarEpicLegs = i;

			}
			//Prayer-bound Sleeves
			FriarEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("FriarEpicArms");
			if (FriarEpicArms == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "FriarEpicArms",
                    Name = "Prayer-bound Sleeves",
                    Level = 50,
                    Item_Type = 28,
                    Model = 38,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 10,
                    Object_Type = 33,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: Pie +18, Dex +16, Cold +8%, Thrust +8%
                    Bonus1 = 18,
                    Bonus1Type = (int)eStat.PIE,

                    Bonus2 = 16,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Cold,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Thrust
                };
                {
					GameServer.Database.AddObject(i);
				}

				FriarEpicArms = i;

			}
			TheurgistEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("TheurgistEpicBoots");
			if (TheurgistEpicBoots == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "TheurgistEpicBoots",
                    Name = "Boots of Shielding Power",
                    Level = 50,
                    Item_Type = 23,
                    Model = 143,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 50,
                    SPD_ABS = 0,
                    Object_Type = 32,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: Dex +16, Cold +6%, Body +8%, Energy +8%
                    Bonus1 = 16,
                    Bonus1Type = (int)eStat.DEX,

                    Bonus2 = 6,
                    Bonus2Type = (int)eResist.Cold,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Body,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Energy
                };
                {
					GameServer.Database.AddObject(i);
				}

				TheurgistEpicBoots = i;

			}
			//end item
			//of Shielding Power Coif
			TheurgistEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("TheurgistEpicHelm");
			if (TheurgistEpicHelm == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "TheurgistEpicHelm",
                    Name = "Coif of Shielding Power",
                    Level = 50,
                    Item_Type = 21,
                    Model = 1290, //NEED TO WORK ON..
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 50,
                    SPD_ABS = 0,
                    Object_Type = 32,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: Int +21, Dex +13, Spirit +8%, Crush +8%
                    Bonus1 = 21,
                    Bonus1Type = (int)eStat.INT,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Spirit,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Crush
                };
                {
					GameServer.Database.AddObject(i);
				}

				TheurgistEpicHelm = i;

			}
			//end item
			//of Shielding Power Gloves
			TheurgistEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("TheurgistEpicGloves");
			if (TheurgistEpicGloves == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "TheurgistEpicGloves",
                    Name = "Gloves of Shielding Power",
                    Level = 50,
                    Item_Type = 22,
                    Model = 142,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 50,
                    SPD_ABS = 0,
                    Object_Type = 32,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: Dex +16, Int +18, Heat +8%, Matter +8%
                    Bonus1 = 16,
                    Bonus1Type = (int)eStat.DEX,

                    Bonus2 = 18,
                    Bonus2Type = (int)eStat.INT,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Heat,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Matter
                };
                {
					GameServer.Database.AddObject(i);
				}

				TheurgistEpicGloves = i;

			}
			//of Shielding Power Hauberk
			TheurgistEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("TheurgistEpicVest");
			if (TheurgistEpicVest == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "TheurgistEpicVest",
                    Name = "Jerkin of Shielding Power",
                    Level = 50,
                    Item_Type = 25,
                    Model = 733,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 50,
                    SPD_ABS = 0,
                    Object_Type = 32,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: HP +24, Power +14, Cold +4%,
                    //triggered effect: Shield (3 charges max) duration 10 mins  (no clue how to implement)
                    Bonus1 = 24,
                    Bonus1Type = (int)eProperty.MaxHealth,

                    Bonus2 = 14,
                    Bonus2Type = (int)eProperty.MaxMana,

                    Bonus3 = 4,
                    Bonus3Type = (int)eResist.Cold
                };
                {
					GameServer.Database.AddObject(i);
				}

				TheurgistEpicVest = i;

			}
			//of Shielding Power Legs
			TheurgistEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("TheurgistEpicLegs");
			if (TheurgistEpicLegs == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "TheurgistEpicLegs",
                    Name = "Legs of Shielding Power",
                    Level = 50,
                    Item_Type = 27,
                    Model = 140,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 50,
                    SPD_ABS = 0,
                    Object_Type = 32,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: Con +19, Wind +4, Energy +10%, Cold +10%
                    Bonus1 = 19,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 4,
                    Bonus2Type = (int)eProperty.Skill_Wind,

                    Bonus3 = 10,
                    Bonus3Type = (int)eResist.Energy,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Cold
                };
                {
					GameServer.Database.AddObject(i);
				}

				TheurgistEpicLegs = i;

			}
			//of Shielding Power Sleeves
			TheurgistEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("TheurgistEpicArms");
			if (TheurgistEpicArms == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "TheurgistEpicArms",
                    Name = "Sleeves of Shielding Power",
                    Level = 50,
                    Item_Type = 28,
                    Model = 141,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 50,
                    SPD_ABS = 0,
                    Object_Type = 32,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    //bonuses: Int +18, Earth +4, Dex +16
                    Bonus1 = 18,
                    Bonus1Type = (int)eStat.INT,

                    Bonus2 = 4,
                    Bonus2Type = (int)eProperty.Skill_Earth,

                    Bonus3 = 16,
                    Bonus3Type = (int)eStat.DEX
                };

                GameServer.Database.AddObject(i);

				TheurgistEpicArms = i;

			}

			MaulerAlbEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("MaulerAlbEpicBoots");
			MaulerAlbEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("MaulerAlbEpicHelm");
			MaulerAlbEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("MaulerAlbEpicGloves");
			MaulerAlbEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("MaulerAlbEpicVest");
			MaulerAlbEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("MaulerAlbEpicLegs");
			MaulerAlbEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("MaulerAlbEpicArms");
			//Item Descriptions End

			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(Lidmann, GameObjectEvent.Interact, new DOLEventHandler(TalkToLidmann));
			GameEventMgr.AddHandler(Lidmann, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLidmann));

			/* Now we bring to masterFrederick the possibility to give this quest to players */
			Lidmann.AddQuestToGive(typeof(Defenders_50));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
			//if not loaded, don't worry
			if (Lidmann == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(Lidmann, GameObjectEvent.Interact, new DOLEventHandler(TalkToLidmann));
			GameEventMgr.RemoveHandler(Lidmann, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLidmann));

			/* Now we remove to masterFrederick the possibility to give this quest to players */
			Lidmann.RemoveQuestToGive(typeof (Defenders_50));
		}

		protected static void TalkToLidmann(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Lidmann.CanGiveQuest(typeof (Defenders_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Defenders_50 quest = player.IsDoingQuest(typeof (Defenders_50)) as Defenders_50;

			if (e == GameObjectEvent.Interact)
			{
				// Nag to finish quest
				if (quest != null)
				{
					Lidmann.SayTo(player, "Check your Journal for instructions!");
					return;
				}
				else
				{
					// Check if player is qualifed for quest                
					Lidmann.SayTo(player, "Albion needs your [services]");
					return;
				}
			}
				// The player whispered to the NPC
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				//Check player is already doing quest
				if (quest == null)
				{
					switch (wArgs.Text)
					{
						case "services":
							player.Out.SendQuestSubscribeCommand(Lidmann, QuestMgr.GetIDForQuestType(typeof(Defenders_50)), "Will you help Lidmann [Defenders of Albion Level 50 Epic]?");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "abort":
							player.Out.SendCustomDialog("Do you really want to abort this quest, \nall items gained during quest will be lost?", new CustomDialogResponse(CheckPlayerAbortQuest));
							break;
					}
				}

			}

		}

		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (Defenders_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte) eCharacterClass.Armsman &&
				player.CharacterClass.ID != (byte) eCharacterClass.Scout &&
				player.CharacterClass.ID != (byte) eCharacterClass.Theurgist &&
				player.CharacterClass.ID != (byte) eCharacterClass.Friar &&
				player.CharacterClass.ID != (byte) eCharacterClass.MaulerAlb)
				return false;

			// This checks below are only performed is player isn't doing quest already

			//if (player.HasFinishedQuest(typeof(Academy_47)) == 0) return false;

			//if (!CheckPartAccessible(player,typeof(CityOfCamelot)))
			//	return false;

			if (player.Level < minimumLevel || player.Level > maximumLevel)
				return false;

			return true;
		}

		/* This is our callback hook that will be called when the player clicks
				 * on any button in the quest offer dialog. We check if he accepts or
				 * declines here...
				 */

		private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
		{
			Defenders_50 quest = player.IsDoingQuest(typeof (Defenders_50)) as Defenders_50;

			if (quest == null)
				return;

			if (response == 0x00)
			{
				SendSystemMessage(player, "Good, no go out there and finish your work!");
			}
			else
			{
				SendSystemMessage(player, "Aborting Quest " + questTitle + ". You can start over again if you want.");
				quest.AbortQuest();
			}
		}

		protected static void SubscribeQuest(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs qargs = args as QuestEventArgs;
			if (qargs == null)
				return;

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Defenders_50)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if(Lidmann.CanGiveQuest(typeof (Defenders_50), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (Defenders_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				// Check to see if we can add quest
				if (!Lidmann.GiveQuest(typeof (Defenders_50), player, 1))
					return;

				player.Out.SendMessage("Kill Cailleach Uragaig in Lyonesse loc 29k, 33k!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "Feast of the Decadent (Level 50 Defenders of Albion Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Seek out Cailleach Uragaig in Lyonesse Loc 29k,33k kill her!";
					case 2:
						return "[Step #2] Give the sealed pouch to Lidmann Halsey.";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Defenders_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if (gArgs != null && gArgs.Target != null && Uragaig != null)
				{
					//if (gArgs.Target.Name == Uragaig.Name)
                        if (gArgs.Target.Name.IndexOf(Uragaig.Name) >= 0)

                        {
						m_questPlayer.Out.SendMessage("Take the pouch to Lidmann Halsey", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						GiveItem(m_questPlayer, sealed_pouch);

						Step = 2;
						return;
					}
				}
			}

            if (Step == 2 && e == GamePlayerEvent.GiveItem)
            {
                // Graveen: if not existing maulerepic in DB
                // player is not allowed to finish this quest until we fix this problem
                if (MaulerAlbEpicArms == null || MaulerAlbEpicBoots == null || MaulerAlbEpicGloves == null ||
                    MaulerAlbEpicHelm == null || MaulerAlbEpicLegs == null || MaulerAlbEpicVest == null)
                    {
                    Lidmann.SayTo(player, "Dark forces are still voiding this quest, your armor is not ready.");
                    return;
                }

                GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
				if (gArgs.Target.Name == Lidmann.Name && gArgs.Item.Id_nb == sealed_pouch.Id_nb)
				{
					Lidmann.SayTo(player, "You have earned this Epic Armor, wear it with honor!");
					FinishQuest();
					return;
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, sealed_pouch, false);
		}

		public override void FinishQuest()
		{
			if (m_questPlayer.Inventory.IsSlotsFree(6, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
			{
				RemoveItem(Lidmann, m_questPlayer, sealed_pouch);

				base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

				if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Armsman)
				{
					GiveItem(m_questPlayer, ArmsmanEpicBoots);
					GiveItem(m_questPlayer, ArmsmanEpicArms);
					GiveItem(m_questPlayer, ArmsmanEpicGloves);
					GiveItem(m_questPlayer, ArmsmanEpicHelm);
					GiveItem(m_questPlayer, ArmsmanEpicLegs);
					GiveItem(m_questPlayer, ArmsmanEpicVest);
				}
				else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Scout)
				{
					GiveItem(m_questPlayer, ScoutEpicArms);
					GiveItem(m_questPlayer, ScoutEpicBoots);
					GiveItem(m_questPlayer, ScoutEpicGloves);
					GiveItem(m_questPlayer, ScoutEpicHelm);
					GiveItem(m_questPlayer, ScoutEpicLegs);
					GiveItem(m_questPlayer, ScoutEpicVest);
				}
				else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Theurgist)
				{
					GiveItem(m_questPlayer, TheurgistEpicArms);
					GiveItem(m_questPlayer, TheurgistEpicBoots);
					GiveItem(m_questPlayer, TheurgistEpicGloves);
					GiveItem(m_questPlayer, TheurgistEpicHelm);
					GiveItem(m_questPlayer, TheurgistEpicLegs);
					GiveItem(m_questPlayer, TheurgistEpicVest);
				}
				else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Friar)
				{
					GiveItem(m_questPlayer, FriarEpicArms);
					GiveItem(m_questPlayer, FriarEpicBoots);
					GiveItem(m_questPlayer, FriarEpicGloves);
					GiveItem(m_questPlayer, FriarEpicHelm);
					GiveItem(m_questPlayer, FriarEpicLegs);
					GiveItem(m_questPlayer, FriarEpicVest);
				}
				else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.MaulerAlb)
				{
					GiveItem(m_questPlayer, MaulerAlbEpicArms);
					GiveItem(m_questPlayer, MaulerAlbEpicBoots);
					GiveItem(m_questPlayer, MaulerAlbEpicGloves);
					GiveItem(m_questPlayer, MaulerAlbEpicHelm);
					GiveItem(m_questPlayer, MaulerAlbEpicLegs);
					GiveItem(m_questPlayer, MaulerAlbEpicVest);
				}

				m_questPlayer.GainExperience(GameLiving.eXPSource.Quest, 1937768448, true);
				//m_questPlayer.AddMoney(Money.GetMoney(0,0,0,2,Util.Random(50)), "You recieve {0} as a reward.");		
			}
			else
			{
				m_questPlayer.Out.SendMessage("You do not have enough free space in your inventory!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}
		}

		#region Allakhazam Epic Source

		/*
        *#25 talk to Lidmann
        *#26 seek out Loken in Raumarik Loc 47k, 25k, 4k, and kill him purp and 2 blue adds 
        *#27 return to Lidmann 
        *#28 give her the ball of flame
        *#29 talk with Lidmann about Loken’s demise
        *#30 go to MorlinCaan in Jordheim 
        *#31 give her the sealed pouch
        *#32 you get your epic armor as a reward
        */

		/*
            *Brigandine of Vigilant Defense  Boots 
            *Brigandine of Vigilant Defense  Coif
            *Brigandine of Vigilant Defense  Gloves
            *Brigandine of Vigilant Defense  Hauberk
            *Brigandine of Vigilant Defense  Legs
            *Brigandine of Vigilant Defense  Sleeves
            *Shadow Shrouded Boots
            *Shadow Shrouded Coif
            *Shadow Shrouded Gloves
            *Shadow Shrouded Hauberk
            *Shadow Shrouded Legs
            *Shadow Shrouded Sleeves
        */

		#endregion
	}
}
