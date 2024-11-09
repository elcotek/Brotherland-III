/*
*Author         : Etaew - Fallen Realms
*Editor         : Gandulf
*Source         : http://camelot.allakhazam.com
*Date           : 8 December 2004
*Quest Name     : Feast of the Decadent (level 50)
*Quest Classes  : Cabalist, Reaver, Mercenary, Necromancer and Infiltrator (Guild of Shadows), Heretic
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
	public class Shadows_50 : BaseQuest
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
		private static ItemTemplate MercenaryEpicBoots = null; // of the Shadowy Embers  Boots 
		private static ItemTemplate MercenaryEpicHelm = null; // of the Shadowy Embers  Coif 
		private static ItemTemplate MercenaryEpicGloves = null; // of the Shadowy Embers  Gloves 
		private static ItemTemplate MercenaryEpicVest = null; // of the Shadowy Embers  Hauberk 
		private static ItemTemplate MercenaryEpicLegs = null; // of the Shadowy Embers  Legs 
		private static ItemTemplate MercenaryEpicArms = null; // of the Shadowy Embers  Sleeves 
		private static ItemTemplate ReaverEpicBoots = null; //Shadow Shrouded Boots 
		private static ItemTemplate ReaverEpicHelm = null; //Shadow Shrouded Coif 
		private static ItemTemplate ReaverEpicGloves = null; //Shadow Shrouded Gloves 
		private static ItemTemplate ReaverEpicVest = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate ReaverEpicLegs = null; //Shadow Shrouded Legs 
		private static ItemTemplate ReaverEpicArms = null; //Shadow Shrouded Sleeves 
		private static ItemTemplate CabalistEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate CabalistEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate CabalistEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate CabalistEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate CabalistEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate CabalistEpicArms = null; //Valhalla Touched Sleeves 
		private static ItemTemplate InfiltratorEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate InfiltratorEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate InfiltratorEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate InfiltratorEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate InfiltratorEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate InfiltratorEpicArms = null; //Subterranean Sleeves		
		private static ItemTemplate NecromancerEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate NecromancerEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate NecromancerEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate NecromancerEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate NecromancerEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate NecromancerEpicArms = null; //Subterranean Sleeves
		private static ItemTemplate HereticEpicBoots = null;
		private static ItemTemplate HereticEpicHelm = null;
		private static ItemTemplate HereticEpicGloves = null;
		private static ItemTemplate HereticEpicVest = null;
		private static ItemTemplate HereticEpicLegs = null;
		private static ItemTemplate HereticEpicArms = null;

		// Constructors
		public Shadows_50()
			: base()
		{
		}
		public Shadows_50(GamePlayer questingPlayer)
			: base(questingPlayer)
		{
		}

		public Shadows_50(GamePlayer questingPlayer, int step)
			: base(questingPlayer, step)
		{
		}

		public Shadows_50(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest)
		{
		}

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");

			#region NPC Declarations

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Lidmann Halsey", eRealm.Albion);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Lidmann Halsey, creating it ...");
                Lidmann = new GameNPC
                {
                    Model = 64,
                    Name = "Lidmann Halsey",
                    GuildName = String.Empty,
                    Realm = eRealm.Albion,
                    CurrentRegionID = 1,
                    Size = 50,
                    Level = 50,
                    X = 466464,
                    Y = 634554,
                    Z = 1954,
                    Heading = 1809
                };
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

			#region Item Declarations

			#region misc
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
			#endregion
			// end item
			ItemTemplate i = null;
			#region Mercenary
			MercenaryEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("MercenaryEpicBoots");
			if (MercenaryEpicBoots == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "MercenaryEpicBoots",
                    Name = "Boots of the Shadowy Embers",
                    Level = 50,
                    Item_Type = 23,
                    Model = 722,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 35,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,


                    Bonus1 = 15,
                    Bonus1Type = (int)eStat.DEX,

                    Bonus2 = 16,
                    Bonus2Type = (int)eStat.QUI,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Cold,

                    Bonus4 = 9,
                    Bonus4Type = (int)eStat.STR
                };
                {
					GameServer.Database.AddObject(i);
				}

				MercenaryEpicBoots = i;

			}
			//end item
			// of the Shadowy Embers  Coif
			MercenaryEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("MercenaryEpicHelm");
			if (MercenaryEpicHelm == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "MercenaryEpicHelm",
                    Name = "Coif of the Shadowy Embers",
                    Level = 50,
                    Item_Type = 21,
                    Model = 1290, //NEED TO WORK ON..
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 35,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,


                    Bonus1 = 16,
                    Bonus1Type = (int)eStat.DEX,

                    Bonus2 = 18,
                    Bonus2Type = (int)eStat.STR,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Body,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Thrust
                };
                {
					GameServer.Database.AddObject(i);
				}

				MercenaryEpicHelm = i;

			}
			//end item
			// of the Shadowy Embers  Gloves
			MercenaryEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("MercenaryEpicGloves");
			if (MercenaryEpicGloves == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "MercenaryEpicGloves",
                    Name = "Gauntlets of the Shadowy Embers",
                    Level = 50,
                    Item_Type = 22,
                    Model = 721,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 35,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,


                    Bonus1 = 19,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Crush,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Matter
                };
                {
					GameServer.Database.AddObject(i);
				}

				MercenaryEpicGloves = i;

			}
			// of the Shadowy Embers  Hauberk
			MercenaryEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("MercenaryEpicVest");
			if (MercenaryEpicVest == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "MercenaryEpicVest",
                    Name = "Haurberk of the Shadowy Embers",
                    Level = 50,
                    Item_Type = 25,
                    Model = 718,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 35,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,


                    Bonus1 = 15,
                    Bonus1Type = (int)eStat.DEX,

                    Bonus2 = 48,
                    Bonus2Type = (int)eProperty.MaxHealth,

                    Bonus3 = 4,
                    Bonus3Type = (int)eResist.Cold,

                    Bonus4 = 6,
                    Bonus4Type = (int)eResist.Thrust
                };
                {
					GameServer.Database.AddObject(i);
				}

				MercenaryEpicVest = i;

			}
			// of the Shadowy Embers  Legs
			MercenaryEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("MercenaryEpicLegs");
			if (MercenaryEpicLegs == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "MercenaryEpicLegs",
                    Name = "Chausses of the Shadowy Embers",
                    Level = 50,
                    Item_Type = 27,
                    Model = 719,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 35,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,


                    Bonus1 = 18,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 16,
                    Bonus2Type = (int)eStat.STR,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Heat,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Slash
                };
                {
					GameServer.Database.AddObject(i);
				}

				MercenaryEpicLegs = i;

			}
			// of the Shadowy Embers  Sleeves
			MercenaryEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("MercenaryEpicArms");
			if (MercenaryEpicArms == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "MercenaryEpicArms",
                    Name = "Sleeves of the Shadowy Embers",
                    Level = 50,
                    Item_Type = 28,
                    Model = 720,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 35,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,


                    Bonus1 = 15,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 16,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Cold,

                    Bonus4 = 12,
                    Bonus4Type = (int)eStat.QUI
                };
                {
					GameServer.Database.AddObject(i);
				}

				MercenaryEpicArms = i;
			}
			#endregion
			#region Reaver
			//Reaver Epic Sleeves End
			ReaverEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("ReaverEpicBoots");
			if (ReaverEpicBoots == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "ReaverEpicBoots",
                    Name = "Boots of Murky Secrets",
                    Level = 50,
                    Item_Type = 23,
                    Model = 1270,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 35,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,


                    Bonus1 = 14,
                    Bonus1Type = (int)eProperty.MaxMana,

                    Bonus2 = 9,
                    Bonus2Type = (int)eStat.STR,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Cold
                };

                //                    i.Bonus4 = 10;
                //                    i.Bonus4Type = (int)eResist.Energy;
                {
					GameServer.Database.AddObject(i);
				}

				ReaverEpicBoots = i;

			}
			//end item
			//of Murky Secrets Coif
			ReaverEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("ReaverEpicHelm");
			if (ReaverEpicHelm == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "ReaverEpicHelm",
                    Name = "Coif of Murky Secrets",
                    Level = 50,
                    Item_Type = 21,
                    Model = 1290, //NEED TO WORK ON..
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 35,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,


                    Bonus1 = 16,
                    Bonus1Type = (int)eStat.PIE,

                    Bonus2 = 6,
                    Bonus2Type = (int)eProperty.Skill_Flexible_Weapon,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Body,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Thrust
                };
                {
					GameServer.Database.AddObject(i);
				}

				ReaverEpicHelm = i;

			}
			//end item
			//of Murky Secrets Gloves
			ReaverEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("ReaverEpicGloves");
			if (ReaverEpicGloves == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "ReaverEpicGloves",
                    Name = "Gauntlets of Murky Secrets",
                    Level = 50,
                    Item_Type = 22,
                    Model = 1271,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 35,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,


                    Bonus1 = 19,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Matter,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Crush
                };
                {
					GameServer.Database.AddObject(i);
				}

				ReaverEpicGloves = i;

			}
			//of Murky Secrets Hauberk
			ReaverEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("ReaverEpicVest");
			if (ReaverEpicVest == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "ReaverEpicVest",
                    Name = "Hauberk of Murky Secrets",
                    Level = 50,
                    Item_Type = 25,
                    Model = 1267,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 35,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,


                    Bonus1 = 48,
                    Bonus1Type = (int)eProperty.MaxHealth,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.PIE,

                    Bonus3 = 4,
                    Bonus3Type = (int)eResist.Cold,

                    Bonus4 = 6,
                    Bonus4Type = (int)eResist.Thrust
                };
                {
					GameServer.Database.AddObject(i);
				}

				ReaverEpicVest = i;

			}
			//of Murky Secrets Legs
			ReaverEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("ReaverEpicLegs");
			if (ReaverEpicLegs == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "ReaverEpicLegs",
                    Name = "Chausses of Murky Secrets",
                    Level = 50,
                    Item_Type = 27,
                    Model = 1268,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 35,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,


                    Bonus1 = 18,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 16,
                    Bonus2Type = (int)eStat.STR,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Heat,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Slash
                };
                {
					GameServer.Database.AddObject(i);
				}

				ReaverEpicLegs = i;

			}
			//of Murky Secrets Sleeves
			ReaverEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("ReaverEpicArms");
			if (ReaverEpicArms == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "ReaverEpicArms",
                    Name = "Sleeves of Murky Secrets",
                    Level = 50,
                    Item_Type = 28,
                    Model = 1269,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 35,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,


                    Bonus1 = 16,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Cold,

                    Bonus4 = 4,
                    Bonus4Type = (int)eProperty.Skill_Slashing
                };
                {
					GameServer.Database.AddObject(i);
				}

				ReaverEpicArms = i;
			}
			#endregion
			#region Infiltrator
			InfiltratorEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("InfiltratorEpicBoots");
			if (InfiltratorEpicBoots == null)
			{
                InfiltratorEpicBoots = new ItemTemplate
                {
                    Id_nb = "InfiltratorEpicBoots",
                    Name = "Shadow-Woven Boots",
                    Level = 50,
                    Item_Type = 23,
                    Model = 796,
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

                    Bonus1 = 13,
                    Bonus1Type = (int)eStat.QUI,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Cold,

                    Bonus4 = 13,
                    Bonus4Type = (int)eStat.CON
                };
                {
					GameServer.Database.AddObject(InfiltratorEpicBoots);
				}

			}
			//end item
			//Shadow-Woven Coif
			InfiltratorEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("InfiltratorEpicHelm");
			if (InfiltratorEpicHelm == null)
			{
                InfiltratorEpicHelm = new ItemTemplate
                {
                    Id_nb = "InfiltratorEpicHelm",
                    Name = "Shadow-Woven Coif",
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

                    Bonus1 = 13,
                    Bonus1Type = (int)eStat.DEX,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.QUI,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Spirit,

                    Bonus4 = 13,
                    Bonus4Type = (int)eStat.STR
                };
                {
					GameServer.Database.AddObject(InfiltratorEpicHelm);
				}

			}
			//end item
			//Shadow-Woven Gloves
			InfiltratorEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("InfiltratorEpicGloves");
			if (InfiltratorEpicGloves == null)
			{
                InfiltratorEpicGloves = new ItemTemplate
                {
                    Id_nb = "InfiltratorEpicGloves",
                    Name = "Shadow-Woven Gloves",
                    Level = 50,
                    Item_Type = 22,
                    Model = 795,
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


                    Bonus1 = 18,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 21,
                    Bonus2Type = (int)eProperty.MaxHealth,

                    Bonus3 = 3,
                    Bonus3Type = (int)eProperty.Skill_Envenom,

                    Bonus4 = 3,
                    Bonus4Type = (int)eProperty.Skill_Critical_Strike
                };
                {
					GameServer.Database.AddObject(InfiltratorEpicGloves);
				}

			}
			//Shadow-Woven Hauberk
			InfiltratorEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("InfiltratorEpicVest");
			if (InfiltratorEpicVest == null)
			{
                InfiltratorEpicVest = new ItemTemplate
                {
                    Id_nb = "InfiltratorEpicVest",
                    Name = "Shadow-Woven Jerkin",
                    Level = 50,
                    Item_Type = 25,
                    Model = 792,
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

                    Bonus1 = 36,
                    Bonus1Type = (int)eProperty.MaxHealth,

                    Bonus2 = 16,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Cold,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Body
                };
                {
					GameServer.Database.AddObject(InfiltratorEpicVest);
				}

			}
			//Shadow-Woven Legs
			InfiltratorEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("InfiltratorEpicLegs");
			if (InfiltratorEpicLegs == null)
			{
                InfiltratorEpicLegs = new ItemTemplate
                {
                    Id_nb = "InfiltratorEpicLegs",
                    Name = "Shadow-Woven Leggings",
                    Level = 50,
                    Item_Type = 27,
                    Model = 793,
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

                    Bonus1 = 21,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 16,
                    Bonus2Type = (int)eStat.QUI,

                    Bonus3 = 6,
                    Bonus3Type = (int)eResist.Heat,

                    Bonus4 = 6,
                    Bonus4Type = (int)eResist.Crush
                };
                {
					GameServer.Database.AddObject(InfiltratorEpicLegs);
				}

			}
			//Shadow-Woven Sleeves
			InfiltratorEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("InfiltratorEpicArms");
			if (InfiltratorEpicArms == null)
			{
                InfiltratorEpicArms = new ItemTemplate
                {
                    Id_nb = "InfiltratorEpicArms",
                    Name = "Shadow-Woven Sleeves",
                    Level = 50,
                    Item_Type = 28,
                    Model = 794,
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

                    Bonus1 = 21,
                    Bonus1Type = (int)eStat.DEX,

                    Bonus2 = 18,
                    Bonus2Type = (int)eStat.STR,

                    Bonus3 = 6,
                    Bonus3Type = (int)eResist.Matter,

                    Bonus4 = 4,
                    Bonus4Type = (int)eResist.Slash
                };
                {
					GameServer.Database.AddObject(InfiltratorEpicArms);
				}

			}
			#endregion
			#region Cabalist
			CabalistEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("CabalistEpicBoots");
			if (CabalistEpicBoots == null)
			{
                CabalistEpicBoots = new ItemTemplate
                {
                    Id_nb = "CabalistEpicBoots",
                    Name = "Warm Boots of the Construct",
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

                    Bonus1 = 22,
                    Bonus1Type = (int)eStat.DEX,

                    Bonus2 = 3,
                    Bonus2Type = (int)eProperty.Skill_Matter,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Slash,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Thrust
                };
                {
					GameServer.Database.AddObject(CabalistEpicBoots);
				}

			}
			//end item
			//Warm of the Construct Coif
			CabalistEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("CabalistEpicHelm");
			if (CabalistEpicHelm == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "CabalistEpicHelm",
                    Name = "Warm Coif of the Construct",
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

                    Bonus1 = 21,
                    Bonus1Type = (int)eStat.INT,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Heat,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Matter
                };
                {
					GameServer.Database.AddObject(i);
				}
				CabalistEpicHelm = i;

			}
			//end item
			//Warm of the Construct Gloves
			CabalistEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("CabalistEpicGloves");
			if (CabalistEpicGloves == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "CabalistEpicGloves",
                    Name = "Warm Gloves of the Construct",
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

                    Bonus1 = 10,
                    Bonus1Type = (int)eStat.DEX,

                    Bonus2 = 10,
                    Bonus2Type = (int)eStat.INT,

                    Bonus3 = 8,
                    Bonus3Type = (int)eProperty.MaxMana,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Energy
                };
                {
					GameServer.Database.AddObject(i);
				}

				CabalistEpicGloves = i;

			}
			//Warm of the Construct Hauberk
			CabalistEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("CabalistEpicVest");
			if (CabalistEpicVest == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "CabalistEpicVest",
                    Name = "Warm Robe of the Construct",
                    Level = 50,
                    Item_Type = 25,
                    Model = 682,
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

                    Bonus1 = 24,
                    Bonus1Type = (int)eProperty.MaxHealth,

                    Bonus2 = 14,
                    Bonus2Type = (int)eProperty.MaxMana,

                    Bonus3 = 4,
                    Bonus3Type = (int)eResist.Crush
                };

                //                    i.Bonus4 = 10;
                //                    i.Bonus4Type = (int)eResist.Energy;
                {
					GameServer.Database.AddObject(i);
				}

				CabalistEpicVest = i;

			}
			//Warm of the Construct Legs
			CabalistEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("CabalistEpicLegs");
			if (CabalistEpicLegs == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "CabalistEpicLegs",
                    Name = "Warm Leggings of the Construct",
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


                    Bonus1 = 22,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 4,
                    Bonus2Type = (int)eProperty.Skill_Spirit,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Cold,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Matter
                };
                {
					GameServer.Database.AddObject(i);
				}

				CabalistEpicLegs = i;

			}
			//Warm of the Construct Sleeves
			CabalistEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("CabalistEpicArms");
			if (CabalistEpicArms == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "CabalistEpicArms",
                    Name = "Warm Sleeves of the Construct",
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


                    Bonus1 = 18,
                    Bonus1Type = (int)eStat.INT,

                    Bonus2 = 4,
                    Bonus2Type = (int)eProperty.Skill_Body,

                    Bonus3 = 16,
                    Bonus3Type = (int)eStat.DEX
                };

                //                    i.Bonus4 = 10;
                //                    i.Bonus4Type = (int)eResist.Energy;
                {
					GameServer.Database.AddObject(i);
				}
				CabalistEpicArms = i;

			}
			#endregion
			#region Necromancer
			NecromancerEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("NecromancerEpicBoots");
			if (NecromancerEpicBoots == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "NecromancerEpicBoots",
                    Name = "Boots of Forbidden Rites",
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


                    Bonus1 = 22,
                    Bonus1Type = (int)eStat.INT,

                    Bonus2 = 4,
                    Bonus2Type = (int)eProperty.Skill_Pain_working,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Slash,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Thrust
                };
                {
					GameServer.Database.AddObject(i);
				}

				NecromancerEpicBoots = i;
			}
			//end item
			//of Forbidden Rites Coif
			NecromancerEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("NecromancerEpicHelm");
			if (NecromancerEpicHelm == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "NecromancerEpicHelm",
                    Name = "Cap of Forbidden Rites",
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


                    Bonus1 = 21,
                    Bonus1Type = (int)eStat.INT,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.QUI,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Heat,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Matter
                };
                {
					GameServer.Database.AddObject(i);
				}
				NecromancerEpicHelm = i;

			}
			//end item
			//of Forbidden Rites Gloves
			NecromancerEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("NecromancerEpicGloves");
            if (NecromancerEpicGloves == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "NecromancerEpicGloves",
                    Name = "Gloves of Forbidden Rites",
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


                    Bonus1 = 10,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 10,
                    Bonus2Type = (int)eStat.INT,

                    Bonus3 = 8,
                    Bonus3Type = (int)eProperty.MaxMana,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Energy
                };
                {
					GameServer.Database.AddObject(i);
				}
				NecromancerEpicGloves = i;

			}
			//of Forbidden Rites Hauberk
			NecromancerEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("NecromancerEpicVest");
			if (NecromancerEpicVest == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "NecromancerEpicVest",
                    Name = "Robe of Forbidden Rites",
                    Level = 50,
                    Item_Type = 25,
                    Model = 1266,
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


                    Bonus1 = 24,
                    Bonus1Type = (int)eProperty.MaxHealth,

                    Bonus2 = 14,
                    Bonus2Type = (int)eProperty.MaxMana,

                    Bonus3 = 4,
                    Bonus3Type = (int)eResist.Crush
                };

                //                    i.Bonus4 = 10;
                //                    i.Bonus4Type = (int)eResist.Energy;
                {
					GameServer.Database.AddObject(i);
				}

				NecromancerEpicVest = i;

			}
			//of Forbidden Rites Legs
			NecromancerEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("NecromancerEpicLegs");
			if (NecromancerEpicLegs == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "NecromancerEpicLegs",
                    Name = "Leggings of Forbidden Rites",
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


                    Bonus1 = 22,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 4,
                    Bonus2Type = (int)eProperty.Skill_Death_Servant,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Cold,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Matter
                };
                {
					GameServer.Database.AddObject(i);
				}
				NecromancerEpicLegs = i;

			}
			//of Forbidden Rites Sleeves
			NecromancerEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("NecromancerEpicArms");
			if (NecromancerEpicArms == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "NecromancerEpicArms",
                    Name = "Sleeves of Forbidden Rites",
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


                    Bonus1 = 18,
                    Bonus1Type = (int)eStat.INT,

                    Bonus2 = 4,
                    Bonus2Type = (int)eProperty.Skill_DeathSight,

                    Bonus3 = 16,
                    Bonus3Type = (int)eStat.DEX
                };

                //                    i.Bonus4 = 10;
                //                    i.Bonus4Type = (int)eResist.Energy;
                {
					GameServer.Database.AddObject(i);
				}
				NecromancerEpicArms = i;
				//Item Descriptions End
			}
			#endregion
			#region Heretic
			HereticEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("HereticEpicBoots");
			if (HereticEpicBoots == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "HereticEpicBoots",
                    Name = "Boots of the Zealous Renegade",
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

                    /*
                     *   Strength: 16 pts
                     *   Constitution: 18 pts
                     *   Slash Resist: 8%
                     *   Heat Resist: 8%
                     */

                    Bonus1 = 16,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 18,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Slash,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Heat
                };
                {
					GameServer.Database.AddObject(i);
				}

				HereticEpicBoots = i;
			}
			//end item
			//of Forbidden Rites Coif
			HereticEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("HereticEpicHelm");
			if (HereticEpicHelm == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "HereticEpicHelm",
                    Name = "Cap of the Zealous Renegade",
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

                    /*
                     *   Piety: 15 pts
                     *   Thrust Resist: 6%
                     *   Cold Resist: 4%
                     *   Hits: 48 pts
                     */

                    Bonus1 = 15,
                    Bonus1Type = (int)eStat.PIE,

                    Bonus2 = 6,
                    Bonus2Type = (int)eResist.Thrust,

                    Bonus3 = 4,
                    Bonus3Type = (int)eResist.Cold,

                    Bonus4 = 48,
                    Bonus4Type = (int)eProperty.MaxHealth
                };
                {
					GameServer.Database.AddObject(i);
				}
				HereticEpicHelm = i;

			}
			//end item
			//of Forbidden Rites Gloves
			HereticEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("HereticEpicGloves");
			if (HereticEpicGloves == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "HereticEpicGloves",
                    Name = "Gloves of the Zealous Renegade",
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

                    /*
                     *   Strength: 9 pts
                     *   Power: 14 pts
                     *   Cold Resist: 8%
                     */
                    Bonus1 = 9,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 14,
                    Bonus2Type = (int)eProperty.MaxMana,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Cold
                };

                {
					GameServer.Database.AddObject(i);
				}
				HereticEpicGloves = i;

			}
			//of Forbidden Rites Hauberk
			HereticEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("HereticEpicVest");
			if (HereticEpicVest == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "HereticEpicVest",
                    Name = "Robe of the Zealous Renegade",
                    Level = 50,
                    Item_Type = 25,
                    Model = 2921,
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

                    /*
                     *   Crush: +4 pts
                     *   Constitution: 16 pts
                     *   Dexterity: 15 pts
                     *   Cold Resist: 8%
                     */

                    Bonus1 = 4,
                    Bonus1Type = (int)eProperty.Skill_Crushing,

                    Bonus2 = 16,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 15,
                    Bonus3Type = (int)eStat.DEX,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Cold
                };
                {
					GameServer.Database.AddObject(i);
				}

				HereticEpicVest = i;

			}
			//of Forbidden Rites Legs
			HereticEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("HereticEpicLegs");
			if (HereticEpicLegs == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "HereticEpicLegs",
                    Name = "Pants of the Zealous Renegade",
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

                    /*
                     *   Strength: 19 pts
                     *   Constitution: 15 pts
                     *   Crush Resist: 8%
                     *   Matter Resist: 8%
                     */

                    Bonus1 = 19,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Crush,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Matter
                };
                {
					GameServer.Database.AddObject(i);
				}
				HereticEpicLegs = i;

			}
			//of Forbidden Rites Sleeves
			HereticEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("HereticEpicArms");
			if (HereticEpicArms == null)
			{
                i = new ItemTemplate
                {
                    Id_nb = "HereticEpicArms",
                    Name = "Sleeves of the Zealous Renegade",
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

                    /*
                     *   Piety: 16 pts
                     *   Thrust Resist: 8%
                     *   Body Resist: 8%
                     *   Flexible: 6 pts
                     */

                    Bonus1 = 16,
                    Bonus1Type = (int)eStat.PIE,

                    Bonus2 = 8,
                    Bonus2Type = (int)eResist.Thrust,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Body,

                    Bonus4 = 6,
                    Bonus4Type = (int)eProperty.Skill_Flexible_Weapon
                };
                {
					GameServer.Database.AddObject(i);
				}
				HereticEpicArms = i;
				//Item Descriptions End
			}
			#endregion

			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(Lidmann, GameObjectEvent.Interact, new DOLEventHandler(TalkToLidmann));
			GameEventMgr.AddHandler(Lidmann, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLidmann));

			/* Now we bring to Lidmann the possibility to give this quest to players */
			Lidmann.AddQuestToGive(typeof(Shadows_50));

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

			/* Now we remove to Lidmann the possibility to give this quest to players */
			Lidmann.RemoveQuestToGive(typeof(Shadows_50));
		}

		protected static void TalkToLidmann(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (Lidmann.CanGiveQuest(typeof(Shadows_50), player) <= 0)
				return;

			//We also check if the player is already doing the quest
			Shadows_50 quest = player.IsDoingQuest(typeof(Shadows_50)) as Shadows_50;

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
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;
				//Check player is already doing quest
				if (quest == null)
				{
					switch (wArgs.Text)
					{
						case "services":
							player.Out.SendQuestSubscribeCommand(Lidmann, QuestMgr.GetIDForQuestType(typeof(Shadows_50)), "Will you help Lidmann [Defenders of Albion Level 50 Epic]?");
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
			if (player.IsDoingQuest(typeof(Shadows_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte)eCharacterClass.Reaver &&
				player.CharacterClass.ID != (byte)eCharacterClass.Mercenary &&
				player.CharacterClass.ID != (byte)eCharacterClass.Cabalist &&
				player.CharacterClass.ID != (byte)eCharacterClass.Necromancer &&
				player.CharacterClass.ID != (byte)eCharacterClass.Infiltrator &&
				player.CharacterClass.ID != (byte)eCharacterClass.Heretic)
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
			Shadows_50 quest = player.IsDoingQuest(typeof(Shadows_50)) as Shadows_50;

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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Shadows_50)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if (Lidmann.CanGiveQuest(typeof(Shadows_50), player) <= 0)
				return;

			if (player.IsDoingQuest(typeof(Shadows_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				// Check to see if we can add quest
				if (!Lidmann.GiveQuest(typeof(Shadows_50), player, 1))
					return;

				player.Out.SendMessage("Kill Cailleach Uragaig in Lyonesse loc 29k, 33k!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "Feast of the Decadent (Level 50 Guild of Shadows Epic)"; }
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
						return "[Step #2] Return to Lidmann Halsey for your reward!";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(Shadows_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
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

				switch ((eCharacterClass)m_questPlayer.CharacterClass.ID)
				{
					case eCharacterClass.Reaver:
						{
							GiveItem(m_questPlayer, ReaverEpicArms);
							GiveItem(m_questPlayer, ReaverEpicBoots);
							GiveItem(m_questPlayer, ReaverEpicGloves);
							GiveItem(m_questPlayer, ReaverEpicHelm);
							GiveItem(m_questPlayer, ReaverEpicLegs);
							GiveItem(m_questPlayer, ReaverEpicVest);
							break;
						}
					case eCharacterClass.Mercenary:
						{
							GiveItem(m_questPlayer, MercenaryEpicArms);
							GiveItem(m_questPlayer, MercenaryEpicBoots);
							GiveItem(m_questPlayer, MercenaryEpicGloves);
							GiveItem(m_questPlayer, MercenaryEpicHelm);
							GiveItem(m_questPlayer, MercenaryEpicLegs);
							GiveItem(m_questPlayer, MercenaryEpicVest);
							break;
						}
					case eCharacterClass.Cabalist:
						{
							GiveItem(m_questPlayer, CabalistEpicArms);
							GiveItem(m_questPlayer, CabalistEpicBoots);
							GiveItem(m_questPlayer, CabalistEpicGloves);
							GiveItem(m_questPlayer, CabalistEpicHelm);
							GiveItem(m_questPlayer, CabalistEpicLegs);
							GiveItem(m_questPlayer, CabalistEpicVest);
							break;
						}
					case eCharacterClass.Infiltrator:
						{
							GiveItem(m_questPlayer, InfiltratorEpicArms);
							GiveItem(m_questPlayer, InfiltratorEpicBoots);
							GiveItem(m_questPlayer, InfiltratorEpicGloves);
							GiveItem(m_questPlayer, InfiltratorEpicHelm);
							GiveItem(m_questPlayer, InfiltratorEpicLegs);
							GiveItem(m_questPlayer, InfiltratorEpicVest);
							break;
						}
					case eCharacterClass.Necromancer:
						{
							GiveItem(m_questPlayer, NecromancerEpicArms);
							GiveItem(m_questPlayer, NecromancerEpicBoots);
							GiveItem(m_questPlayer, NecromancerEpicGloves);
							GiveItem(m_questPlayer, NecromancerEpicHelm);
							GiveItem(m_questPlayer, NecromancerEpicLegs);
							GiveItem(m_questPlayer, NecromancerEpicVest);
							break;
						}
					case eCharacterClass.Heretic:
						{
							GiveItem(m_questPlayer, HereticEpicArms);
							GiveItem(m_questPlayer, HereticEpicBoots);
							GiveItem(m_questPlayer, HereticEpicGloves);
							GiveItem(m_questPlayer, HereticEpicHelm);
							GiveItem(m_questPlayer, HereticEpicLegs);
							GiveItem(m_questPlayer, HereticEpicVest);
							break;
						}
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
            * of the Shadowy Embers  Boots 
            * of the Shadowy Embers  Coif
            * of the Shadowy Embers  Gloves
            * of the Shadowy Embers  Hauberk
            * of the Shadowy Embers  Legs
            * of the Shadowy Embers  Sleeves
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
