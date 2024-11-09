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
/*
*Author         : Etaew - Fallen Realms
*Source         : http://camelot.allakhazam.com
*Date           : 22 November 2004
*Quest Name     : Unnatural Powers (level 50)
*Quest Classes  : Eldritch, Hero, Ranger, and Warden (Path of Focus)
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

namespace DOL.GS.Quests.Hibernia
{
	public class Focus_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "Unnatural Powers";
		protected const int minimumLevel = 48;
		protected const int maximumLevel = 50;

		private static GameNPC Ainrebh = null; // Start NPC
		private static GameNPC GreenMaw = null; // Mob to kill

		private static ItemTemplate GreenMaw_key = null; //ball of flame
		private static ItemTemplate RangerEpicBoots = null; //Mist Shrouded Boots 
		private static ItemTemplate RangerEpicHelm = null; //Mist Shrouded Coif 
		private static ItemTemplate RangerEpicGloves = null; //Mist Shrouded Gloves 
		private static ItemTemplate RangerEpicVest = null; //Mist Shrouded Hauberk 
		private static ItemTemplate RangerEpicLegs = null; //Mist Shrouded Legs 
		private static ItemTemplate RangerEpicArms = null; //Mist Shrouded Sleeves 
		private static ItemTemplate HeroEpicBoots = null; //Shadow Shrouded Boots 
		private static ItemTemplate HeroEpicHelm = null; //Shadow Shrouded Coif 
		private static ItemTemplate HeroEpicGloves = null; //Shadow Shrouded Gloves 
		private static ItemTemplate HeroEpicVest = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate HeroEpicLegs = null; //Shadow Shrouded Legs 
		private static ItemTemplate HeroEpicArms = null; //Shadow Shrouded Sleeves 
		private static ItemTemplate EldritchEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate EldritchEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate EldritchEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate EldritchEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate EldritchEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate EldritchEpicArms = null; //Valhalla Touched Sleeves 
		private static ItemTemplate WardenEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate WardenEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate WardenEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate WardenEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate WardenEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate WardenEpicArms = null; //Subterranean Sleeves    
        private static ItemTemplate MaulerHibEpicBoots = null;
        private static ItemTemplate MaulerHibEpicHelm = null;
        private static ItemTemplate MaulerHibEpicGloves = null;
        private static ItemTemplate MaulerHibEpicVest = null;
        private static ItemTemplate MaulerHibEpicLegs = null;
        private static ItemTemplate MaulerHibEpicArms = null;      

		// Constructors
		public Focus_50() : base()
		{
		}

		public Focus_50(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public Focus_50(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Focus_50(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Ainrebh", eRealm.Hibernia);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Ainrebh , creating it ...");
                Ainrebh = new GameNPC
                {
                    Model = 384,
                    Name = "Ainrebh",
                    GuildName = "Enchanter",
                    Realm = eRealm.Hibernia,
                    CurrentRegionID = 200,
                    Size = 48,
                    Level = 40,
                    X = 421281,
                    Y = 516273,
                    Z = 1877,
                    Heading = 3254
                };
                Ainrebh.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Ainrebh.SaveIntoDatabase();
				}

			}
			else
				Ainrebh = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Green Maw", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find GreenMaw , creating it ...");
                GreenMaw = new GameNPC
                {
                    Model = 146,
                    Name = "Green Maw",
                    GuildName = String.Empty,
                    Realm = eRealm.None,
                    CurrentRegionID = 200,
                    Size = 50,
                    Level = 65,
                    X = 488306,
                    Y = 521440,
                    Z = 6328,
                    Heading = 1162
                };
                GreenMaw.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					GreenMaw.SaveIntoDatabase();
				}

			}
			else
				GreenMaw = npcs[0];
			// end npc

			#endregion

			#region Item Declarations

			GreenMaw_key = GameServer.Database.FindObjectByKey<ItemTemplate>("GreenMaw_key");
			if (GreenMaw_key == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find GreenMaw's Key , creating it ...");
                GreenMaw_key = new ItemTemplate
                {
                    Id_nb = "GreenMaw_key",
                    Name = "GreenMaw's Key",
                    Level = 8,
                    Item_Type = 29,
                    Model = 583,
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
					GameServer.Database.AddObject(GreenMaw_key);
				}

			}
// end item			
			RangerEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("RangerEpicBoots");
			if (RangerEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Rangers Epic Boots , creating it ...");
                RangerEpicBoots = new ItemTemplate
                {
                    Id_nb = "RangerEpicBoots",
                    Name = "Mist Shrouded Boots",
                    Level = 50,
                    Item_Type = 23,
                    Model = 819,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 19,
                    Object_Type = 37,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    Bonus1 = 13,
                    Bonus1Type = (int)eStat.DEX,

                    Bonus2 = 12,
                    Bonus2Type = (int)eStat.QUI,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Thrust,

                    Bonus4 = 30,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(RangerEpicBoots);
				}

			}
//end item
			//Mist Shrouded Coif 
			RangerEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("RangerEpicHelm");
			if (RangerEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Rangers Epic Helm , creating it ...");
                RangerEpicHelm = new ItemTemplate
                {
                    Id_nb = "RangerEpicHelm",
                    Name = "Mist Shrouded Helm",
                    Level = 50,
                    Item_Type = 21,
                    Model = 1292, //NEED TO WORK ON..
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 19,
                    Object_Type = 37,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    Bonus1 = 19,
                    Bonus1Type = (int)eStat.DEX,

                    Bonus2 = 10,
                    Bonus2Type = (int)eResist.Spirit,

                    Bonus3 = 27,
                    Bonus3Type = (int)eProperty.MaxHealth,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Energy
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(RangerEpicHelm);
				}

			}
//end item
			//Mist Shrouded Gloves 
			RangerEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("RangerEpicGloves");
			if (RangerEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Rangers Epic Gloves , creating it ...");
                RangerEpicGloves = new ItemTemplate
                {
                    Id_nb = "RangerEpicGloves",
                    Name = "Mist Shrouded Gloves ",
                    Level = 50,
                    Item_Type = 22,
                    Model = 818,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 19,
                    Object_Type = 37,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    Bonus1 = 3,
                    Bonus1Type = (int)eProperty.Skill_RecurvedBow,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 15,
                    Bonus3Type = (int)eStat.QUI,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Crush
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(RangerEpicGloves);
				}

			}
			//Mist Shrouded Hauberk 
			RangerEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("RangerEpicVest");
			if (RangerEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Rangers Epic Vest , creating it ...");
                RangerEpicVest = new ItemTemplate
                {
                    Id_nb = "RangerEpicVest",
                    Name = "Mist Shrouded Hauberk",
                    Level = 50,
                    Item_Type = 25,
                    Model = 815,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 19,
                    Object_Type = 37,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    Bonus1 = 7,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 7,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 7,
                    Bonus3Type = (int)eStat.QUI,

                    Bonus4 = 48,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(RangerEpicVest);
				}

			}
			//Mist Shrouded Legs 
			RangerEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("RangerEpicLegs");
			if (RangerEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Rangers Epic Legs , creating it ...");
                RangerEpicLegs = new ItemTemplate
                {
                    Id_nb = "RangerEpicLegs",
                    Name = "Mist Shrouded Leggings",
                    Level = 50,
                    Item_Type = 27,
                    Model = 816,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 19,
                    Object_Type = 37,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    Bonus1 = 12,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 12,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 12,
                    Bonus3Type = (int)eResist.Body,

                    Bonus4 = 39,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(RangerEpicLegs);
				}
				;

			}
			//Mist Shrouded Sleeves 
			RangerEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("RangerEpicArms");
			if (RangerEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Ranger Epic Arms , creating it ...");
                RangerEpicArms = new ItemTemplate
                {
                    Id_nb = "RangerEpicArms",
                    Name = "Mist Shrouded Sleeves",
                    Level = 50,
                    Item_Type = 28,
                    Model = 817,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 19,
                    Object_Type = 37,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    Bonus1 = 12,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 12,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 10,
                    Bonus3Type = (int)eResist.Spirit,

                    Bonus4 = 30,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(RangerEpicArms);
				}

			}
//Hero Epic Sleeves End
			HeroEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("HeroEpicBoots");
			if (HeroEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Heros Epic Boots , creating it ...");
                HeroEpicBoots = new ItemTemplate
                {
                    Id_nb = "HeroEpicBoots",
                    Name = "Misted Boots",
                    Level = 50,
                    Item_Type = 23,
                    Model = 712,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 38,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    Bonus1 = 12,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 12,
                    Bonus2Type = (int)eStat.QUI,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Spirit,

                    Bonus4 = 33,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(HeroEpicBoots);
				}

			}
//end item
			//Misted Coif 
			HeroEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("HeroEpicHelm");
			if (HeroEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Heros Epic Helm , creating it ...");
                HeroEpicHelm = new ItemTemplate
                {
                    Id_nb = "HeroEpicHelm",
                    Name = "Misted Coif",
                    Level = 50,
                    Item_Type = 21,
                    Model = 1292, //NEED TO WORK ON..
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 38,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    Bonus1 = 15,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 8,
                    Bonus2Type = (int)eResist.Spirit,

                    Bonus3 = 48,
                    Bonus3Type = (int)eProperty.MaxHealth,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Heat
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(HeroEpicHelm);
				}

			}
//end item
			//Misted Gloves 
			HeroEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("HeroEpicGloves");
			if (HeroEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Heros Epic Gloves , creating it ...");
                HeroEpicGloves = new ItemTemplate
                {
                    Id_nb = "HeroEpicGloves",
                    Name = "Misted Gloves ",
                    Level = 50,
                    Item_Type = 22,
                    Model = 711,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 38,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    Bonus1 = 2,
                    Bonus1Type = (int)eProperty.Skill_Shields,

                    Bonus2 = 2,
                    Bonus2Type = (int)eProperty.Skill_Parry,

                    Bonus3 = 16,
                    Bonus3Type = (int)eStat.DEX,

                    Bonus4 = 18,
                    Bonus4Type = (int)eStat.QUI
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(HeroEpicGloves);
				}

			}
			//Misted Hauberk 
			HeroEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("HeroEpicVest");
			if (HeroEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Heros Epic Vest , creating it ...");
                HeroEpicVest = new ItemTemplate
                {
                    Id_nb = "HeroEpicVest",
                    Name = "Misted Hauberk",
                    Level = 50,
                    Item_Type = 25,
                    Model = 708,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 38,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    Bonus1 = 15,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 16,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 15,
                    Bonus3Type = (int)eStat.DEX
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(HeroEpicVest);
				}

			}
			//Misted Legs 
			HeroEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("HeroEpicLegs");
			if (HeroEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Heros Epic Legs , creating it ...");
                HeroEpicLegs = new ItemTemplate
                {
                    Id_nb = "HeroEpicLegs",
                    Name = "Misted Leggings",
                    Level = 50,
                    Item_Type = 27,
                    Model = 709,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 38,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    Bonus1 = 10,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 21,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 10,
                    Bonus3Type = (int)eResist.Thrust,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Heat
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(HeroEpicLegs);
				}

			}
			//Misted Sleeves 
			HeroEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("HeroEpicArms");
			if (HeroEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Hero Epic Arms , creating it ...");
                HeroEpicArms = new ItemTemplate
                {
                    Id_nb = "HeroEpicArms",
                    Name = "Misted Sleeves",
                    Level = 50,
                    Item_Type = 28,
                    Model = 710,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 38,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    Bonus1 = 24,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 10,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Cold,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Spirit
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(HeroEpicArms);
				}

			}
			WardenEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("WardenEpicBoots");
			if (WardenEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warden Epic Boots , creating it ...");
                WardenEpicBoots = new ItemTemplate
                {
                    Id_nb = "WardenEpicBoots",
                    Name = "Mystical Boots",
                    Level = 50,
                    Item_Type = 23,
                    Model = 809,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 38,
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

                    Bonus3 = 10,
                    Bonus3Type = (int)eResist.Crush,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Matter
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(WardenEpicBoots);
				}

			}
//end item
			//Mystical Coif 
			WardenEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("WardenEpicHelm");
			if (WardenEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warden Epic Helm , creating it ...");
                WardenEpicHelm = new ItemTemplate
                {
                    Id_nb = "WardenEpicHelm",
                    Name = "Mystical Coif",
                    Level = 50,
                    Item_Type = 21,
                    Model = 1292, //NEED TO WORK ON..
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 38,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    Bonus1 = 15,
                    Bonus1Type = (int)eStat.EMP,

                    Bonus2 = 2,
                    Bonus2Type = (int)eProperty.PowerRegenerationRate,

                    Bonus3 = 30,
                    Bonus3Type = (int)eProperty.MaxHealth,

                    Bonus4 = 4,
                    Bonus4Type = (int)eProperty.Skill_Regrowth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(WardenEpicHelm);
				}

			}
//end item
			//Mystical Gloves 
			WardenEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("WardenEpicGloves");
			if (WardenEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warden Epic Gloves , creating it ...");
                WardenEpicGloves = new ItemTemplate
                {
                    Id_nb = "WardenEpicGloves",
                    Name = "Mystical Gloves ",
                    Level = 50,
                    Item_Type = 22,
                    Model = 808,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 38,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    Bonus1 = 4,
                    Bonus1Type = (int)eProperty.Skill_Nurture,

                    Bonus2 = 12,
                    Bonus2Type = (int)eResist.Slash,

                    Bonus3 = 4,
                    Bonus3Type = (int)eProperty.PowerRegenerationRate,

                    Bonus4 = 33,
                    Bonus4Type = (int)eProperty.MaxHealth
                };


                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(WardenEpicGloves);
				}

			}
			//Mystical Hauberk 
			WardenEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("WardenEpicVest");
			if (WardenEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warden Epic Vest , creating it ...");
                WardenEpicVest = new ItemTemplate
                {
                    Id_nb = "WardenEpicVest",
                    Name = "Mystical Hauberk",
                    Level = 50,
                    Item_Type = 25,
                    Model = 805,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 38,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    Bonus1 = 9,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 9,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 9,
                    Bonus3Type = (int)eStat.EMP
                };

                WardenEpicVest.Bonus2 = 39;
				WardenEpicVest.Bonus2Type = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(WardenEpicVest);
				}

			}
			//Mystical Legs 
			WardenEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("WardenEpicLegs");
			if (WardenEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warden Epic Legs , creating it ...");
                WardenEpicLegs = new ItemTemplate
                {
                    Id_nb = "WardenEpicLegs",
                    Name = "Mystical Legs",
                    Level = 50,
                    Item_Type = 27,
                    Model = 806,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 38,
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
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 10,
                    Bonus3Type = (int)eStat.DEX,

                    Bonus4 = 30,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(WardenEpicLegs);
				}

			}
			//Mystical Sleeves 
			WardenEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("WardenEpicArms");
			if (WardenEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warden Epic Arms , creating it ...");
                WardenEpicArms = new ItemTemplate
                {
                    Id_nb = "WardenEpicArms",
                    Name = "Mystical Sleeves",
                    Level = 50,
                    Item_Type = 28,
                    Model = 807,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 38,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    Bonus1 = 12,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 8,
                    Bonus2Type = (int)eResist.Matter,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Spirit,

                    Bonus4 = 45,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(WardenEpicArms);
				}

			}
			EldritchEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("EldritchEpicBoots");
			if (EldritchEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Eldritch Epic Boots , creating it ...");
                EldritchEpicBoots = new ItemTemplate
                {
                    Id_nb = "EldritchEpicBoots",
                    Name = "Mistwoven Boots",
                    Level = 50,
                    Item_Type = 23,
                    Model = 382,
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

                    Bonus1 = 9,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 9,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 6,
                    Bonus3Type = (int)eProperty.PowerRegenerationRate,

                    Bonus4 = 21,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(EldritchEpicBoots);
				}

			}
//end item
			//Mist Woven Coif 
			EldritchEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("EldritchEpicHelm");
			if (EldritchEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Eldritch Epic Helm , creating it ...");
                EldritchEpicHelm = new ItemTemplate
                {
                    Id_nb = "EldritchEpicHelm",
                    Name = "Mistwoven Cap",
                    Level = 50,
                    Item_Type = 21,
                    Model = 1298, //NEED TO WORK ON..
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
                    Bonus1Type = (int)eResist.Heat,

                    Bonus2 = 10,
                    Bonus2Type = (int)eResist.Spirit,

                    Bonus3 = 4,
                    Bonus3Type = (int)eProperty.Focus_Void,

                    Bonus4 = 19,
                    Bonus4Type = (int)eStat.INT
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(EldritchEpicHelm);
				}

			}
//end item
			//Mist Woven Gloves 
			EldritchEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("EldritchEpicGloves");
			if (EldritchEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Eldritch Epic Gloves , creating it ...");
                EldritchEpicGloves = new ItemTemplate
                {
                    Id_nb = "EldritchEpicGloves",
                    Name = "Mistwoven Gloves ",
                    Level = 50,
                    Item_Type = 22,
                    Model = 381,
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

                    Bonus1 = 4,
                    Bonus1Type = (int)eProperty.Focus_Light,

                    Bonus2 = 9,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 4,
                    Bonus3Type = (int)eProperty.PowerRegenerationRate,

                    Bonus4 = 24,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(EldritchEpicGloves);
				}

			}
			//Mist Woven Hauberk 
			EldritchEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("EldritchEpicVest");
			if (EldritchEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Eldritch Epic Vest , creating it ...");
                EldritchEpicVest = new ItemTemplate
                {
                    Id_nb = "EldritchEpicVest",
                    Name = "Mistwoven Vest",
                    Level = 50,
                    Item_Type = 25,
                    Model = 744,
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

                    Bonus1 = 15,
                    Bonus1Type = (int)eStat.DEX,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.INT,

                    Bonus3 = 33
                };
                EldritchEpicVest.Bonus3 = (int) eProperty.MaxHealth;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(EldritchEpicVest);
				}

			}
			//Mist Woven Legs 
			EldritchEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("EldritchEpicLegs");
			if (EldritchEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Eldritch Epic Legs , creating it ...");
                EldritchEpicLegs = new ItemTemplate
                {
                    Id_nb = "EldritchEpicLegs",
                    Name = "Mistwoven Pants",
                    Level = 50,
                    Item_Type = 27,
                    Model = 379,
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
                    Bonus1Type = (int)eResist.Cold,

                    Bonus2 = 10,
                    Bonus2Type = (int)eResist.Body,

                    Bonus3 = 15,
                    Bonus3Type = (int)eStat.DEX,

                    Bonus4 = 16,
                    Bonus4Type = (int)eStat.CON
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(EldritchEpicLegs);
				}

			}
			//Mist Woven Sleeves 
			EldritchEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("EldritchEpicArms");
			if (EldritchEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Eldritch Epic Arms , creating it ...");
                EldritchEpicArms = new ItemTemplate
                {
                    Id_nb = "EldritchEpicArms",
                    Name = "Mistwoven Sleeves",
                    Level = 50,
                    Item_Type = 28,
                    Model = 380,
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

                    Bonus1 = 4,
                    Bonus1Type = (int)eProperty.Focus_Mana,

                    Bonus2 = 10,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 10,
                    Bonus3Type = (int)eStat.INT,

                    Bonus4 = 27,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(EldritchEpicArms);
				}

			}

//Hero Epic Sleeves End

            // Graveen: we assume items are existing in the DB
            // TODO: insert here creation of items if they do not exists
            MaulerHibEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("MaulerHibEpicBoots");
            MaulerHibEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("MaulerHibEpicHelm");
            MaulerHibEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("MaulerHibEpicGloves");
            MaulerHibEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("MaulerHibEpicVest");
            MaulerHibEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("MaulerHibEpicLegs");
            MaulerHibEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("MaulerHibEpicArms");

//Item Descriptions End

			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(Ainrebh, GameObjectEvent.Interact, new DOLEventHandler(TalkToAinrebh));
			GameEventMgr.AddHandler(Ainrebh, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAinrebh));

			/* Now we bring to Ainrebh the possibility to give this quest to players */
			Ainrebh.AddQuestToGive(typeof (Focus_50));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Ainrebh == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(Ainrebh, GameObjectEvent.Interact, new DOLEventHandler(TalkToAinrebh));
			GameEventMgr.RemoveHandler(Ainrebh, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAinrebh));

			/* Now we remove to Ainrebh the possibility to give this quest to players */
			Ainrebh.RemoveQuestToGive(typeof (Focus_50));
		}

		protected static void TalkToAinrebh(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Ainrebh.CanGiveQuest(typeof (Focus_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Focus_50 quest = player.IsDoingQuest(typeof (Focus_50)) as Focus_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					Ainrebh.SayTo(player, "Check your Journal for instructions!");
				}
				else
				{
					Ainrebh.SayTo(player, "Hibernia needs your [services]");
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
							player.Out.SendQuestSubscribeCommand(Ainrebh, QuestMgr.GetIDForQuestType(typeof(Focus_50)), "Will you help Ainrebh [Path of Focus Level 50 Epic]?");
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
			if (player.IsDoingQuest(typeof (Focus_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte) eCharacterClass.Hero &&
				player.CharacterClass.ID != (byte) eCharacterClass.Ranger &&
                player.CharacterClass.ID != (byte) eCharacterClass.MaulerHib &&
                player.CharacterClass.ID != (byte) eCharacterClass.Warden &&
				player.CharacterClass.ID != (byte) eCharacterClass.Eldritch)
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
			Focus_50 quest = player.IsDoingQuest(typeof (Focus_50)) as Focus_50;

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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Focus_50)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if(Ainrebh.CanGiveQuest(typeof (Focus_50), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (Focus_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!Ainrebh.GiveQuest(typeof (Focus_50), player, 1))
					return;
				player.Out.SendMessage("Kill Green Maw in Cursed Forest loc 37k, 38k!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "Unnatural Powers (Level 50 Path of Focus Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Seek out GreenMaw in Cursed Forest Loc 37k,38k kill it!";
					case 2:
						return "[Step #2] Return to Ainrebh and give her Green Maw's Key!";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Focus_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;

				//if (gArgs.Target.Name == GreenMaw.Name)
                    if (gArgs.Target.Name.IndexOf(GreenMaw.Name) >= 0)

                    {
					m_questPlayer.Out.SendMessage("You collect Green Maw's Key", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					GiveItem(m_questPlayer, GreenMaw_key);
					Step = 2;
					return;
				}

			}

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
            {
                // Graveen: if not existing maulerepic in DB
                // player is not allowed to finish this quest until we fix this problem
//                if (MaulerHibEpicArms == null || MaulerHibEpicBoots == null || MaulerHibEpicGloves == null ||
//                    MaulerHibEpicHelm == null || MaulerHibEpicLegs == null || MaulerHibEpicVest == null)
                if (MaulerHibEpicArms == null || MaulerHibEpicBoots == null || MaulerHibEpicGloves == null ||
                    MaulerHibEpicHelm == null || MaulerHibEpicLegs == null || MaulerHibEpicVest == null)
                    {
                    Ainrebh.SayTo(player, "Dark forces are still voiding this quest, your armor is not ready.");
                    return;
                }

				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Ainrebh.Name && gArgs.Item.Id_nb == GreenMaw_key.Id_nb)
				{
					Ainrebh.SayTo(player, "You have earned this Epic Armour!");
					FinishQuest();
					return;
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, GreenMaw_key, false);
		}

		public override void FinishQuest()
		{
			if (m_questPlayer.Inventory.IsSlotsFree(6, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
			{
				RemoveItem(Ainrebh, m_questPlayer, GreenMaw_key);

				base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

				if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Hero)
				{
					GiveItem(m_questPlayer, HeroEpicArms);
					GiveItem(m_questPlayer, HeroEpicBoots);
					GiveItem(m_questPlayer, HeroEpicGloves);
					GiveItem(m_questPlayer, HeroEpicHelm);
					GiveItem(m_questPlayer, HeroEpicLegs);
					GiveItem(m_questPlayer, HeroEpicVest);
				}
				else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Ranger)
				{
					GiveItem(m_questPlayer, RangerEpicArms);
					GiveItem(m_questPlayer, RangerEpicBoots);
					GiveItem(m_questPlayer, RangerEpicGloves);
					GiveItem(m_questPlayer, RangerEpicHelm);
					GiveItem(m_questPlayer, RangerEpicLegs);
					GiveItem(m_questPlayer, RangerEpicVest);
				}
				else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Eldritch)
				{
					GiveItem(m_questPlayer, EldritchEpicArms);
					GiveItem(m_questPlayer, EldritchEpicBoots);
					GiveItem(m_questPlayer, EldritchEpicGloves);
					GiveItem(m_questPlayer, EldritchEpicHelm);
					GiveItem(m_questPlayer, EldritchEpicLegs);
					GiveItem(m_questPlayer, EldritchEpicVest);
				}
				else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Warden)
				{
					GiveItem(m_questPlayer, WardenEpicArms);
					GiveItem(m_questPlayer, WardenEpicBoots);
					GiveItem(m_questPlayer, WardenEpicGloves);
					GiveItem(m_questPlayer, WardenEpicHelm);
					GiveItem(m_questPlayer, WardenEpicLegs);
					GiveItem(m_questPlayer, WardenEpicVest);
				}
				else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.MaulerHib)
				{
					GiveItem(m_questPlayer, MaulerHibEpicBoots);
					GiveItem(m_questPlayer, MaulerHibEpicArms);
					GiveItem(m_questPlayer, MaulerHibEpicGloves);
					GiveItem(m_questPlayer, MaulerHibEpicHelm);
					GiveItem(m_questPlayer, MaulerHibEpicVest);
					GiveItem(m_questPlayer, MaulerHibEpicLegs);
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
        *#25 talk to Ainrebh
        *#26 seek out Loken in Raumarik Loc 47k, 25k, 4k, and kill him purp and 2 blue adds 
        *#27 return to Ainrebh 
        *#28 give her the ball of flame
        *#29 talk with Ainrebh about Loken’s demise
        *#30 go to MorlinCaan in Jordheim 
        *#31 give her the sealed pouch
        *#32 you get your epic armor as a reward
        */

		/*
            *Mist Shrouded Boots 
            *Mist Shrouded Coif
            *Mist Shrouded Gloves
            *Mist Shrouded Hauberk
            *Mist Shrouded Legs
            *Mist Shrouded Sleeves
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
