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
*Editor			: Gandulf
*Source         : http://camelot.allakhazam.com
*Date           : 22 November 2004
*Quest Name     : An End to the Daggers (level 50)
*Quest Classes  : Warrior, Berserker, Thane, Skald, Savage (Viking)
*Quest Version  : v1
*
*Done:
*Bonuses to epic items
*
*ToDo:   
*   Find Helm ModelID for epics..
*   checks for all other epics done
*/

using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Midgard
{
	public class Viking_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "An End to the Daggers";
		protected const int minimumLevel = 48;
		protected const int maximumLevel = 50;

		private static GameNPC Lynnleigh = null; // Start NPC
		private static GameNPC Ydenia = null; // Mob to kill
		private static GameNPC Elizabeth = null; // reward NPC

		private static ItemTemplate tome_enchantments = null;
		private static ItemTemplate sealed_pouch = null;
		private static ItemTemplate WarriorEpicBoots = null;
		private static ItemTemplate WarriorEpicHelm = null;
		private static ItemTemplate WarriorEpicGloves = null;
		private static ItemTemplate WarriorEpicLegs = null;
		private static ItemTemplate WarriorEpicArms = null;
		private static ItemTemplate WarriorEpicVest = null;
		private static ItemTemplate BerserkerEpicBoots = null;
		private static ItemTemplate BerserkerEpicHelm = null;
		private static ItemTemplate BerserkerEpicGloves = null;
		private static ItemTemplate BerserkerEpicLegs = null;
		private static ItemTemplate BerserkerEpicArms = null;
		private static ItemTemplate BerserkerEpicVest = null;
		private static ItemTemplate ThaneEpicBoots = null;
		private static ItemTemplate ThaneEpicHelm = null;
		private static ItemTemplate ThaneEpicGloves = null;
		private static ItemTemplate ThaneEpicLegs = null;
		private static ItemTemplate ThaneEpicArms = null;
		private static ItemTemplate ThaneEpicVest = null;
		private static ItemTemplate SkaldEpicBoots = null;
		private static ItemTemplate SkaldEpicHelm = null;
		private static ItemTemplate SkaldEpicGloves = null;
		private static ItemTemplate SkaldEpicVest = null;
		private static ItemTemplate SkaldEpicLegs = null;
		private static ItemTemplate SkaldEpicArms = null;
		private static ItemTemplate SavageEpicBoots = null;
		private static ItemTemplate SavageEpicHelm = null;
		private static ItemTemplate SavageEpicGloves = null;
		private static ItemTemplate SavageEpicVest = null;
		private static ItemTemplate SavageEpicLegs = null;
		private static ItemTemplate SavageEpicArms = null;
		private static ItemTemplate ValkyrieEpicBoots = null;
		private static ItemTemplate ValkyrieEpicHelm = null;
		private static ItemTemplate ValkyrieEpicGloves = null;
		private static ItemTemplate ValkyrieEpicVest = null;
		private static ItemTemplate ValkyrieEpicLegs = null;
		private static ItemTemplate ValkyrieEpicArms = null;
        private static ItemTemplate MaulerMidEpicBoots = null;
        private static ItemTemplate MaulerMidEpicHelm = null;
        private static ItemTemplate MaulerMidEpicGloves = null;
        private static ItemTemplate MaulerMidEpicVest = null;
        private static ItemTemplate MaulerMidEpicLegs = null;
        private static ItemTemplate MaulerMidEpicArms = null; 


		// Constructors
		public Viking_50() : base()
		{
		}

		public Viking_50(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public Viking_50(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Viking_50(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Lynnleigh", eRealm.Midgard);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Lynnleigh , creating it ...");
                Lynnleigh = new GameNPC
                {
                    Model = 217,
                    Name = "Lynnleigh",
                    GuildName = String.Empty,
                    Realm = eRealm.Midgard,
                    CurrentRegionID = 100,
                    Size = 51,
                    Level = 50,
                    X = 760085,
                    Y = 758453,
                    Z = 4736,
                    Heading = 2197
                };
                Lynnleigh.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Lynnleigh.SaveIntoDatabase();
				}
			}
			else
				Lynnleigh = npcs[0];
			// end npc
			npcs = WorldMgr.GetNPCsByName("Elizabeth", eRealm.Midgard);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Elizabeth , creating it ...");
                Elizabeth = new GameNPC
                {
                    Model = 217,
                    Name = "Elizabeth",
                    GuildName = "Enchanter",
                    Realm = eRealm.Midgard,
                    CurrentRegionID = 100,
                    Size = 51,
                    Level = 41,
                    X = 802849,
                    Y = 727081,
                    Z = 4681,
                    Heading = 2480
                };
                Elizabeth.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Elizabeth.SaveIntoDatabase();
				}

			}
			else
				Elizabeth = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Ydenia of Seithkona", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Ydenia , creating it ...");
                Ydenia = new GameNPC
                {
                    Model = 217,
                    Name = "Ydenia of Seithkona",
                    GuildName = String.Empty,
                    Realm = eRealm.None,
                    CurrentRegionID = 100,
                    Size = 100,
                    Level = 65,
                    X = 637680,
                    Y = 767189,
                    Z = 4480,
                    Heading = 2156
                };
                Ydenia.Flags ^= GameNPC.eFlags.GHOST;
				Ydenia.MaxSpeedBase = 200;
				Ydenia.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Ydenia.SaveIntoDatabase();
				}

			}
			else
				Ydenia = npcs[0];
			// end npc

			#endregion

			#region defineItems

			tome_enchantments = GameServer.Database.FindObjectByKey<ItemTemplate>("tome_enchantments");
			if (tome_enchantments == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Tome of Enchantments , creating it ...");
                tome_enchantments = new ItemTemplate
                {
                    Id_nb = "tome_enchantments",
                    Name = "Tome of Enchantments",
                    Level = 8,
                    Item_Type = 0,
                    Model = 500,
                    IsDropable = false,
                    IsPickable = false,
                    DPS_AF = 0,
                    SPD_ABS = 0,
                    Object_Type = 0,
                    Hand = 0,
                    Type_Damage = 0,
                    Quality = 100,
                    Weight = 12
                };
                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(tome_enchantments);
				}

			}

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

			WarriorEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("WarriorEpicBoots");
			if (WarriorEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warrior Epic Boots , creating it ...");
                WarriorEpicBoots = new ItemTemplate
                {
                    Id_nb = "WarriorEpicBoots",
                    Name = "Tyr's Might Boots",
                    Level = 50,
                    Item_Type = 23,
                    Model = 780,
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
                    Durability = 50000,
                    Condition = 50000,

                    Bonus1 = 16,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.QUI,

                    Bonus3 = 10,
                    Bonus3Type = (int)eResist.Heat,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Energy
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(WarriorEpicBoots);
				}

			}
//end item
			WarriorEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("WarriorEpicHelm");
			if (WarriorEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warrior Epic Helm , creating it ...");
                WarriorEpicHelm = new ItemTemplate
                {
                    Id_nb = "WarriorEpicHelm",
                    Name = "Tyr's Might Coif",
                    Level = 50,
                    Item_Type = 21,
                    Model = 832, //NEED TO WORK ON..
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

                    Bonus1 = 12,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 12,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 12,
                    Bonus3Type = (int)eStat.DEX,

                    Bonus4 = 11,
                    Bonus4Type = (int)eResist.Crush
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(WarriorEpicHelm);
				}

			}
//end item
			WarriorEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("WarriorEpicGloves");
			if (WarriorEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warrior Epic Gloves , creating it ...");
                WarriorEpicGloves = new ItemTemplate
                {
                    Id_nb = "WarriorEpicGloves",
                    Name = "Tyr's Might Gloves",
                    Level = 50,
                    Item_Type = 22,
                    Model = 779,
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

                    Bonus1 = 3,
                    Bonus1Type = (int)eProperty.Skill_Shields,

                    Bonus2 = 3,
                    Bonus2Type = (int)eProperty.Skill_Parry,

                    Bonus3 = 15,
                    Bonus3Type = (int)eStat.STR,

                    Bonus4 = 13,
                    Bonus4Type = (int)eStat.DEX
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(WarriorEpicGloves);
				}

			}

			WarriorEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("WarriorEpicVest");
			if (WarriorEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warrior Epic Vest , creating it ...");
                WarriorEpicVest = new ItemTemplate
                {
                    Id_nb = "WarriorEpicVest",
                    Name = "Tyr's Might Hauberk",
                    Level = 50,
                    Item_Type = 25,
                    Model = 776,
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

                    Bonus1 = 13,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 6,
                    Bonus3Type = (int)eResist.Matter,

                    Bonus4 = 30,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(WarriorEpicVest);
				}

			}

			WarriorEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("WarriorEpicLegs");
			if (WarriorEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warrior Epic Legs , creating it ...");
                WarriorEpicLegs = new ItemTemplate
                {
                    Id_nb = "WarriorEpicLegs",
                    Name = "Tyr's Might Legs",
                    Level = 50,
                    Item_Type = 27,
                    Model = 777,
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

                    Bonus1 = 22,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.STR,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Cold,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Body
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(WarriorEpicLegs);
				}

			}

			WarriorEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("WarriorEpicArms");
			if (WarriorEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warrior Epic Arms , creating it ...");
                WarriorEpicArms = new ItemTemplate
                {
                    Id_nb = "WarriorEpicArms",
                    Name = "Tyr's Might Sleeves",
                    Level = 50,
                    Item_Type = 28,
                    Model = 778,
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

                    Bonus1 = 22,
                    Bonus1Type = (int)eStat.DEX,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.QUI,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Crush,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Slash
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(WarriorEpicArms);
				}

			}
			BerserkerEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("BerserkerEpicBoots");
			if (BerserkerEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Berserker Epic Boots , creating it ...");
                BerserkerEpicBoots = new ItemTemplate
                {
                    Id_nb = "BerserkerEpicBoots",
                    Name = "Courage Bound Boots",
                    Level = 50,
                    Item_Type = 23,
                    Model = 755,
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

                    Bonus1 = 19,
                    Bonus1Type = (int)eStat.DEX,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.QUI,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Spirit,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Energy
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(BerserkerEpicBoots);
				}

			}
//end item
			BerserkerEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("BerserkerEpicHelm");
			if (BerserkerEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Berserker Epic Helm , creating it ...");
                BerserkerEpicHelm = new ItemTemplate
                {
                    Id_nb = "BerserkerEpicHelm",
                    Name = "Courage Bound Helm",
                    Level = 50,
                    Item_Type = 21,
                    Model = 829, //NEED TO WORK ON..
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

                    Bonus1 = 10,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 10,
                    Bonus3Type = (int)eStat.DEX,

                    Bonus4 = 10,
                    Bonus4Type = (int)eStat.QUI
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(BerserkerEpicHelm);
				}
			}
//end item
			BerserkerEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("BerserkerEpicGloves");
			if (BerserkerEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Berserker Epic Gloves , creating it ...");
                BerserkerEpicGloves = new ItemTemplate
                {
                    Id_nb = "BerserkerEpicGloves",
                    Name = "Courage Bound Gloves",
                    Level = 50,
                    Item_Type = 22,
                    Model = 754,
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

                    Bonus1 = 3,
                    Bonus1Type = (int)eProperty.Skill_Left_Axe,

                    Bonus2 = 3,
                    Bonus2Type = (int)eProperty.Skill_Parry,

                    Bonus3 = 12,
                    Bonus3Type = (int)eStat.STR,

                    Bonus4 = 33,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(BerserkerEpicGloves);
				}
			}

			BerserkerEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("BerserkerEpicVest");
			if (BerserkerEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Berserker Epic Vest , creating it ...");
                BerserkerEpicVest = new ItemTemplate
                {
                    Id_nb = "BerserkerEpicVest",
                    Name = "Courage Bound Jerkin",
                    Level = 50,
                    Item_Type = 25,
                    Model = 751,
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

                    Bonus1 = 13,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 6,
                    Bonus3Type = (int)eResist.Body,

                    Bonus4 = 30,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(BerserkerEpicVest);
				}
			}

			BerserkerEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("BerserkerEpicLegs");
			if (BerserkerEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Berserker Epic Legs , creating it ...");
                BerserkerEpicLegs = new ItemTemplate
                {
                    Id_nb = "BerserkerEpicLegs",
                    Name = "Courage Bound Leggings",
                    Level = 50,
                    Item_Type = 27,
                    Model = 752,
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

                    Bonus1 = 15,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 7,
                    Bonus3Type = (int)eStat.DEX,

                    Bonus4 = 12,
                    Bonus4Type = (int)eResist.Slash
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(BerserkerEpicLegs);
				}
			}

			BerserkerEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("BerserkerEpicArms");
			if (BerserkerEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Berserker Epic Arms , creating it ...");
                BerserkerEpicArms = new ItemTemplate
                {
                    Id_nb = "BerserkerEpicArms",
                    Name = "Courage Bound Sleeves",
                    Level = 50,
                    Item_Type = 28,
                    Model = 753,
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

                    Bonus1 = 19,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Thrust,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Heat
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(BerserkerEpicArms);
				}

			}
			ThaneEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("ThaneEpicBoots");
			if (ThaneEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Thane Epic Boots , creating it ...");
                ThaneEpicBoots = new ItemTemplate
                {
                    Id_nb = "ThaneEpicBoots",
                    Name = "Storm Touched Boots",
                    Level = 50,
                    Item_Type = 23,
                    Model = 791,
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

                    Bonus1 = 13,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 13,
                    Bonus3Type = (int)eStat.QUI,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Matter
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ThaneEpicBoots);
				}

			}
//end item
			ThaneEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("ThaneEpicHelm");
			if (ThaneEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Thane Epic Helm , creating it ...");
                ThaneEpicHelm = new ItemTemplate
                {
                    Id_nb = "ThaneEpicHelm",
                    Name = "Storm Touched Coif",
                    Level = 50,
                    Item_Type = 21,
                    Model = 834,
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

                    Bonus1 = 4,
                    Bonus1Type = (int)eProperty.Skill_Stormcalling,

                    Bonus2 = 18,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 4,
                    Bonus3Type = (int)eResist.Spirit,

                    Bonus4 = 6,
                    Bonus4Type = (int)eProperty.PowerRegenerationRate
                };


                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ThaneEpicHelm);
				}

			}
//end item
			ThaneEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("ThaneEpicGloves");
			if (ThaneEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Thane Epic Gloves , creating it ...");
                ThaneEpicGloves = new ItemTemplate
                {
                    Id_nb = "ThaneEpicGloves",
                    Name = "Storm Touched Gloves",
                    Level = 50,
                    Item_Type = 22,
                    Model = 790,
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

                    Bonus1 = 3,
                    Bonus1Type = (int)eProperty.Skill_Sword,

                    Bonus2 = 3,
                    Bonus2Type = (int)eProperty.Skill_Hammer,

                    Bonus3 = 3,
                    Bonus3Type = (int)eProperty.Skill_Axe,

                    Bonus4 = 19,
                    Bonus4Type = (int)eStat.STR
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ThaneEpicGloves);
				}

			}

			ThaneEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("ThaneEpicVest");
			if (ThaneEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Thane Epic Vest , creating it ...");
                ThaneEpicVest = new ItemTemplate
                {
                    Id_nb = "ThaneEpicVest",
                    Name = "Storm Touched Hauberk",
                    Level = 50,
                    Item_Type = 25,
                    Model = 787,
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

                    Bonus1 = 13,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 6,
                    Bonus3Type = (int)eResist.Slash,

                    Bonus4 = 30,
                    Bonus4Type = (int)eProperty.MaxHealth
                };


                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ThaneEpicVest);
				}
			}

			ThaneEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("ThaneEpicLegs");
			if (ThaneEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Thane Epic Legs , creating it ...");
                ThaneEpicLegs = new ItemTemplate
                {
                    Id_nb = "ThaneEpicLegs",
                    Name = "Storm Touched Legs",
                    Level = 50,
                    Item_Type = 27,
                    Model = 788,
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
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.PIE,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Crush,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Heat
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ThaneEpicLegs);
				}
			}

			ThaneEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("ThaneEpicArms");
			if (ThaneEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Thane Epic Arms , creating it ...");
                ThaneEpicArms = new ItemTemplate
                {
                    Id_nb = "ThaneEpicArms",
                    Name = "Storm Touched Sleeves",
                    Level = 50,
                    Item_Type = 28,
                    Model = 789,
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
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 16,
                    Bonus2Type = (int)eStat.QUI,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Thrust,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Body
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ThaneEpicArms);
				}
			}
			//Valhalla Touched Boots
			SkaldEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("SkaldEpicBoots");
			if (SkaldEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Skalds Epic Boots , creating it ...");
                SkaldEpicBoots = new ItemTemplate
                {
                    Id_nb = "SkaldEpicBoots",
                    Name = "Battlesung Boots",
                    Level = 50,
                    Item_Type = 23,
                    Model = 775,
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

                    Bonus1 = 13,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.QUI,

                    Bonus3 = 10,
                    Bonus3Type = (int)eResist.Cold,

                    Bonus4 = 24,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(SkaldEpicBoots);
				}
			}
//end item
			//Valhalla Touched Coif 
			SkaldEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("SkaldEpicHelm");
			if (SkaldEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Skalds Epic Helm , creating it ...");
                SkaldEpicHelm = new ItemTemplate
                {
                    Id_nb = "SkaldEpicHelm",
                    Name = "Battlesung Coif",
                    Level = 50,
                    Item_Type = 21,
                    Model = 832, //NEED TO WORK ON..
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

                    Bonus1 = 5,
                    Bonus1Type = (int)eProperty.Skill_Battlesongs,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.CHR,

                    Bonus3 = 33,
                    Bonus3Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(SkaldEpicHelm);
				}
			}
//end item
			//Valhalla Touched Gloves 
			SkaldEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("SkaldEpicGloves");
			if (SkaldEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Skalds Epic Gloves , creating it ...");
                SkaldEpicGloves = new ItemTemplate
                {
                    Id_nb = "SkaldEpicGloves",
                    Name = "Battlesung Gloves",
                    Level = 50,
                    Item_Type = 22,
                    Model = 774,
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
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Body,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Energy
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(SkaldEpicGloves);
				}

			}
			//Valhalla Touched Hauberk 
			SkaldEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("SkaldEpicVest");
			if (SkaldEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Skalds Epic Vest , creating it ...");
                SkaldEpicVest = new ItemTemplate
                {
                    Id_nb = "SkaldEpicVest",
                    Name = "Battlesung Hauberk",
                    Level = 50,
                    Item_Type = 25,
                    Model = 771,
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

                    Bonus1 = 13,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 13,
                    Bonus3Type = (int)eStat.CHR,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Matter
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(SkaldEpicVest);
				}
			}
			//Valhalla Touched Legs 
			SkaldEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("SkaldEpicLegs");
			if (SkaldEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Skalds Epic Legs , creating it ...");
                SkaldEpicLegs = new ItemTemplate
                {
                    Id_nb = "SkaldEpicLegs",
                    Name = "Battlesung Legs",
                    Level = 50,
                    Item_Type = 27,
                    Model = 772,
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

                    Bonus1 = 13,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Spirit,

                    Bonus4 = 27,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(SkaldEpicLegs);
				}
			}
			//Valhalla Touched Sleeves 
			SkaldEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("SkaldEpicArms");
			if (SkaldEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Skald Epic Arms , creating it ...");
                SkaldEpicArms = new ItemTemplate
                {
                    Id_nb = "SkaldEpicArms",
                    Name = "Battlesung Sleeves",
                    Level = 50,
                    Item_Type = 28,
                    Model = 773,
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
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 10,
                    Bonus3Type = (int)eResist.Thrust,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Cold
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(SkaldEpicArms);
				}
			}
			//Subterranean Boots 
			SavageEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("SavageEpicBoots");
			if (SavageEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Savage Epic Boots , creating it ...");
                SavageEpicBoots = new ItemTemplate
                {
                    Id_nb = "SavageEpicBoots",
                    Name = "Kelgor's Battle Boots",
                    Level = 50,
                    Item_Type = 23,
                    Model = 1196,
                    IsDropable = true,
                    IsPickable = true,
                    DPS_AF = 100,
                    SPD_ABS = 27,
                    Object_Type = 34,
                    Quality = 100,
                    Weight = 22,
                    Bonus = 35,
                    MaxCondition = 50000,
                    MaxDurability = 50000,
                    Condition = 50000,
                    Durability = 50000,

                    Bonus1 = 15,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 19,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Matter,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Energy
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(SavageEpicBoots);
				}
			}
			//Subterranean Coif 
			SavageEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("SavageEpicHelm");
			if (SavageEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Savage Epic Helm , creating it ...");
                SavageEpicHelm = new ItemTemplate
                {
                    Id_nb = "SavageEpicHelm",
                    Name = "Kelgor's Battle Helm",
                    Level = 50,
                    Item_Type = 21,
                    Model = 831, //NEED TO WORK ON..
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

                    Bonus1 = 15,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 10,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 10,
                    Bonus3Type = (int)eStat.DEX,

                    Bonus4 = 10,
                    Bonus4Type = (int)eStat.QUI
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(SavageEpicHelm);
				}
			}
			//Subterranean Gloves 
			SavageEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("SavageEpicGloves");
			if (SavageEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Savage Epic Gloves , creating it ...");
                SavageEpicGloves = new ItemTemplate
                {
                    Id_nb = "SavageEpicGloves",
                    Name = "Kelgor's Battle Gauntlets",
                    Level = 50,
                    Item_Type = 22,
                    Model = 1195,
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

                    Bonus1 = 3,
                    Bonus1Type = (int)eProperty.Skill_Parry,

                    Bonus2 = 12,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 33,
                    Bonus3Type = (int)eProperty.MaxHealth,

                    Bonus4 = 3,
                    Bonus4Type = (int)eProperty.Skill_HandToHand
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(SavageEpicGloves);
				}
			}
			//Subterranean Hauberk 
			SavageEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("SavageEpicVest");
			if (SavageEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Savage Epic Vest , creating it ...");
                SavageEpicVest = new ItemTemplate
                {
                    Id_nb = "SavageEpicVest",
                    Name = "Kelgor's Battle Vest",
                    Level = 50,
                    Item_Type = 25,
                    Model = 1192,
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

                    Bonus1 = 13,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.QUI,

                    Bonus3 = 6,
                    Bonus3Type = (int)eResist.Slash,

                    Bonus4 = 30,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(SavageEpicVest);
				}
			}
			//Subterranean Legs 
			SavageEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("SavageEpicLegs");
			if (SavageEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Savage Epic Legs , creating it ...");
                SavageEpicLegs = new ItemTemplate
                {
                    Id_nb = "SavageEpicLegs",
                    Name = "Kelgor's Battle Leggings",
                    Level = 50,
                    Item_Type = 27,
                    Model = 1193,
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

                    Bonus1 = 12,
                    Bonus1Type = (int)eResist.Heat,

                    Bonus2 = 7,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 15,
                    Bonus3Type = (int)eStat.DEX,

                    Bonus4 = 15,
                    Bonus4Type = (int)eStat.QUI
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(SavageEpicLegs);
				}
			}
			//Subterranean Sleeves 
			SavageEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("SavageEpicArms");
			if (SavageEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Savage Epic Arms , creating it ...");
                SavageEpicArms = new ItemTemplate
                {
                    Id_nb = "SavageEpicArms",
                    Name = "Kelgor's Battle Sleeves",
                    Level = 50,
                    Item_Type = 28,
                    Model = 1194,
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

                    Bonus1 = 19,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.QUI,

                    Bonus3 = 8
                };
                SavageEpicArms.Bonus3 = (int) eResist.Cold;

				SavageEpicArms.Bonus4 = 8;
				SavageEpicArms.Bonus4Type = (int) eResist.Heat;

				if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(SavageEpicArms);
				}

			}
			#region Valkyrie
			ValkyrieEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("ValkyrieEpicBoots");
			if (ValkyrieEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Valkyrie Epic Boots , creating it ...");
                ValkyrieEpicBoots = new ItemTemplate
                {
                    Id_nb = "ValkyrieEpicBoots",
                    Name = "Battle Maiden's Boots",
                    Level = 50,
                    Item_Type = 23,
                    Model = 2932,
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
                    Durability = 50000,
                    Condition = 50000,

                    /*
                     *   Constitution: 7 pts
                     *   Dexterity: 13 pts
                     *   Quickness: 13 pts
                     *   Body Resist: 8%
                     */

                    Bonus1 = 7,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 13,
                    Bonus3Type = (int)eStat.QUI,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Body
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ValkyrieEpicBoots);
				}

			}
			//end item
			ValkyrieEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("ValkyrieEpicHelm");
			if (ValkyrieEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Valkyrie Epic Helm , creating it ...");
                ValkyrieEpicHelm = new ItemTemplate
                {
                    Id_nb = "ValkyrieEpicHelm",
                    Name = "Battle Maiden's Coif",
                    Level = 50,
                    Item_Type = 21,
                    Model = 2951, //NEED TO WORK ON..
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

                    /*
                     *   Sword: +4 pts
                     *   Constitution: 18 pts
                     *   Cold Resist: 4%
                     *   Energy Resist: 6%
                     */

                    Bonus1 = 4,
                    Bonus1Type = (int)eProperty.Skill_Sword,

                    Bonus2 = 18,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 4,
                    Bonus3Type = (int)eResist.Cold,

                    Bonus4 = 6,
                    Bonus4Type = (int)eResist.Energy
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ValkyrieEpicHelm);
				}

			}
			//end item
			ValkyrieEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("ValkyrieEpicGloves");
			if (ValkyrieEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Valkyrie Epic Gloves , creating it ...");
                ValkyrieEpicGloves = new ItemTemplate
                {
                    Id_nb = "ValkyrieEpicGloves",
                    Name = "Battle Maiden's Gloves",
                    Level = 50,
                    Item_Type = 22,
                    Model = 2931,
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

                    /*
                     *   Spear: +3 pts
                     *   Parry: +3 pts
                     *   Strength: 19 pts
                     *   Odin's Will: +3 pts
                     */

                    Bonus1 = 3,
                    Bonus1Type = (int)eProperty.Skill_Spear,

                    Bonus2 = 3,
                    Bonus2Type = (int)eProperty.Skill_Parry,

                    Bonus3 = 19,
                    Bonus3Type = (int)eStat.STR,

                    Bonus4 = 3,
                    Bonus4Type = (int)eProperty.Skill_OdinsWill
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ValkyrieEpicGloves);
				}

			}

			ValkyrieEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("ValkyrieEpicVest");
			if (ValkyrieEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Valkyrie Epic Vest , creating it ...");
                ValkyrieEpicVest = new ItemTemplate
                {
                    Id_nb = "ValkyrieEpicVest",
                    Name = "Battle Maiden's Hauberk",
                    Level = 50,
                    Item_Type = 25,
                    Model = 2928,
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

                    /*
                     *   Strength: 13 pts
                     *   Constitution: 13 pts
                     *   Slash Resist: 6%
                     *   Hits: 30 pts
                     */

                    Bonus1 = 13,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 6,
                    Bonus3Type = (int)eResist.Matter,

                    Bonus4 = 30,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ValkyrieEpicVest);
				}

			}

			ValkyrieEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("ValkyrieEpicLegs");
			if (ValkyrieEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Valkyrie Epic Legs , creating it ...");
                ValkyrieEpicLegs = new ItemTemplate
                {
                    Id_nb = "ValkyrieEpicLegs",
                    Name = "Battle Maiden's Legs",
                    Level = 50,
                    Item_Type = 27,
                    Model = 2929,
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

                    /*
                     *   Constitution: 19 pts
                     *   Piety: 15 pts
                     *   Crush Resist: 8%
                     *   Heat Resist: 8%
                     */

                    Bonus1 = 19,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.PIE,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Crush,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Heat
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ValkyrieEpicLegs);
				}

			}

			ValkyrieEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("ValkyrieEpicArms");
			if (ValkyrieEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Valkyrie Epic Arms , creating it ...");
                ValkyrieEpicArms = new ItemTemplate
                {
                    Id_nb = "ValkyrieEpicArms",
                    Name = "Battle Maiden's Sleeves",
                    Level = 50,
                    Item_Type = 28,
                    Model = 2930,
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

                    /*
                     *   Strength: 18 pts
                     *   Quickness: 16 pts
                     *   Thrust Resist: 8%
                     *   Spirit Resist: 8%
                     */

                    Bonus1 = 18,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 16,
                    Bonus2Type = (int)eStat.QUI,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Thrust,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Spirit
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ValkyrieEpicArms);
				}

			}
			#endregion

            // Graveen: we assume items are existing in the DB
            // TODO: insert here creation of items if they do not exists
            MaulerMidEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("MaulerMidEpicBoots");
            MaulerMidEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("MaulerMidEpicHelm");
            MaulerMidEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("MaulerMidEpicGloves");
            MaulerMidEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("MaulerMidEpicVest");
            MaulerMidEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("MaulerMidEpicLegs");
            MaulerMidEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("MaulerMidEpicArms");

//Item Descriptions End

			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(Lynnleigh, GameObjectEvent.Interact, new DOLEventHandler(TalkToLynnleigh));
			GameEventMgr.AddHandler(Lynnleigh, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLynnleigh));

			GameEventMgr.AddHandler(Elizabeth, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToElizabeth));
			GameEventMgr.AddHandler(Elizabeth, GameLivingEvent.Interact, new DOLEventHandler(TalkToElizabeth));

			/* Now we bring to Lynnleigh the possibility to give this quest to players */
			Lynnleigh.AddQuestToGive(typeof (Viking_50));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Lynnleigh == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(Lynnleigh, GameObjectEvent.Interact, new DOLEventHandler(TalkToLynnleigh));
			GameEventMgr.RemoveHandler(Lynnleigh, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLynnleigh));

			GameEventMgr.RemoveHandler(Elizabeth, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToElizabeth));
			GameEventMgr.RemoveHandler(Elizabeth, GameLivingEvent.Interact, new DOLEventHandler(TalkToElizabeth));
		
			/* Now we remove to Lynnleigh the possibility to give this quest to players */
			Lynnleigh.RemoveQuestToGive(typeof (Viking_50));
		}

		protected static void TalkToLynnleigh(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Lynnleigh.CanGiveQuest(typeof (Viking_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Viking_50 quest = player.IsDoingQuest(typeof (Viking_50)) as Viking_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					Lynnleigh.SayTo(player, "Check your Journal for information about what to do!");
				}
				else
				{
					Lynnleigh.SayTo(player, "Ah, this reveals exactly where Jango and his deserters took Ydenia to dispose of him. He also has a note here about how strong Ydenia really was. That [worries me].");
				}
			}
				// The player whispered to the NPC
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;

				if (quest == null)
				{
					switch (wArgs.Text)
					{
						case "worries me":
							Lynnleigh.SayTo(player, "Yes, it worries me, but I think that you are ready to [face Ydenia] and his minions.");
							break;
						case "face Ydenia":
							player.Out.SendQuestSubscribeCommand(Lynnleigh, QuestMgr.GetIDForQuestType(typeof(Viking_50)), "Will you face Ydenia [Viking Level 50 Epic]?");
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

		protected static void TalkToElizabeth(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Lynnleigh.CanGiveQuest(typeof (Viking_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Viking_50 quest = player.IsDoingQuest(typeof (Viking_50)) as Viking_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					switch (quest.Step)
					{
                        case 4:
                            {
                                // Graveen: if not existing maulerepic in DB
                                // player is not allowed to finish this quest until we fix this problem
//                                if (MaulerMidEpicArms == null || MaulerMidEpicBoots == null || MaulerMidEpicGloves == null ||
//                                    MaulerMidEpicHelm == null || MaulerMidEpicLegs == null || MaulerMidEpicVest == null)
                                if (MaulerMidEpicArms == null || MaulerMidEpicBoots == null || MaulerMidEpicGloves == null ||
                                    MaulerMidEpicHelm == null || MaulerMidEpicLegs == null || MaulerMidEpicVest == null)
                                {
                                    Elizabeth.SayTo(player, "Dark forces are still voiding this quest, your armor is not ready.");
                                    return;
                                }

                                Elizabeth.SayTo(player, "There are six parts to your reward, so make sure you have room for them. Just let me know when you are ready, and then you can [take them] with our thanks!");
                                break;
                            }

					}
				}
			}
				// The player whispered to the NPC
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;

				if (quest != null)
				{
					switch (wArgs.Text)
					{
						case "take them":
							if (quest.Step == 4)
								quest.FinishQuest();
							break;
					}
				}
			}
		}

		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (Viking_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte) eCharacterClass.Warrior &&
				player.CharacterClass.ID != (byte) eCharacterClass.Berserker &&
				player.CharacterClass.ID != (byte) eCharacterClass.Thane &&
				player.CharacterClass.ID != (byte) eCharacterClass.Skald &&
				player.CharacterClass.ID != (byte) eCharacterClass.Savage &&
                player.CharacterClass.ID != (byte) eCharacterClass.MaulerMid &&
				player.CharacterClass.ID != (byte) eCharacterClass.Valkyrie)
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
			Viking_50 quest = player.IsDoingQuest(typeof (Viking_50)) as Viking_50;

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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Viking_50)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if(Lynnleigh.CanGiveQuest(typeof (Viking_50), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (Viking_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!Lynnleigh.GiveQuest(typeof (Viking_50), player, 1))
					return;

				Lynnleigh.SayTo(player, "Yes, you must face and defeat him! There is a note scrawled in the corner of the map that even in death Ydenia is strong. He has gathered followers to protect him in his spirit state and they will come to his aid if he is attacked. Even though you have improved your skills quite a bit, I would highley recommed taking some friends with you to face Ydenia. It is imperative that you defeat him and obtain the totem he holds if I am to end the spell. According to the map you can find Ydenia in Raumarik. Head to the river in Raumarik and go north. When you reach the end of it, go northwest to the next river. Cross the river and head west. Follow the snowline until you reach a group of trees. That is where you will find Ydenia and his followers. Return to me when you have the totem. May all the gods be with you.");
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "An End to the Daggers (level 50 Viking epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Seek out Ydenia in Raumarik Loc 48k, 30k kill her!";
					case 2:
						return "[Step #2] Return to Lynnleigh and give her tome of Enchantments!";
					case 3:
						return "[Step #3] Take the Sealed Pouch to Elizabeth in Mularn";
					case 4:
						return "[Step #4] Tell Elizabeth you can 'take them' for your rewards!";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Viking_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;

				//if (gArgs.Target.Name == Ydenia.Name)
                    if (gArgs.Target.Name.IndexOf(Ydenia.Name) >= 0)
                    {
					Step = 2;
					GiveItem(m_questPlayer, tome_enchantments);
					m_questPlayer.Out.SendMessage("Ydenia drops the Tome of Enchantments and you pick it up!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
			}

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
        		GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Lynnleigh.Name && gArgs.Item.Id_nb == tome_enchantments.Id_nb)
				{
					RemoveItem(Lynnleigh, player, tome_enchantments);
					Lynnleigh.SayTo(player, "Take this sealed pouch to Elizabeth in Mularn for your reward!");
					GiveItem(Lynnleigh, player, sealed_pouch);
					Step = 3;
				}
			}

			if (Step == 3 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Elizabeth.Name && gArgs.Item.Id_nb == sealed_pouch.Id_nb)
				{
					RemoveItem(Elizabeth, player, sealed_pouch);
					Elizabeth.SayTo(player, "There are six parts to your reward, so make sure you have room for them. Just let me know when you are ready, and then you can [take them] with our thanks!");
					Step = 4;
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, sealed_pouch, false);
			RemoveItem(m_questPlayer, tome_enchantments, false);
		}

		public override void FinishQuest()
		{
			if (m_questPlayer.Inventory.IsSlotsFree(6, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
			{
				base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

				switch ((eCharacterClass)m_questPlayer.CharacterClass.ID)
				{
					case eCharacterClass.Warrior:
						{
							GiveItem(m_questPlayer, WarriorEpicArms);
							GiveItem(m_questPlayer, WarriorEpicBoots);
							GiveItem(m_questPlayer, WarriorEpicGloves);
							GiveItem(m_questPlayer, WarriorEpicHelm);
							GiveItem(m_questPlayer, WarriorEpicLegs);
							GiveItem(m_questPlayer, WarriorEpicVest);
							break;
						}
					case eCharacterClass.Berserker:
						{
							GiveItem(m_questPlayer, BerserkerEpicArms);
							GiveItem(m_questPlayer, BerserkerEpicBoots);
							GiveItem(m_questPlayer, BerserkerEpicGloves);
							GiveItem(m_questPlayer, BerserkerEpicHelm);
							GiveItem(m_questPlayer, BerserkerEpicLegs);
							GiveItem(m_questPlayer, BerserkerEpicVest);
							break;
						}
					case eCharacterClass.Thane:
						{
							GiveItem(m_questPlayer, ThaneEpicArms);
							GiveItem(m_questPlayer, ThaneEpicBoots);
							GiveItem(m_questPlayer, ThaneEpicGloves);
							GiveItem(m_questPlayer, ThaneEpicHelm);
							GiveItem(m_questPlayer, ThaneEpicLegs);
							GiveItem(m_questPlayer, ThaneEpicVest);
							break;
						}
					case eCharacterClass.Skald:
						{
							GiveItem(m_questPlayer, SkaldEpicArms);
							GiveItem(m_questPlayer, SkaldEpicBoots);
							GiveItem(m_questPlayer, SkaldEpicGloves);
							GiveItem(m_questPlayer, SkaldEpicHelm);
							GiveItem(m_questPlayer, SkaldEpicLegs);
							GiveItem(m_questPlayer, SkaldEpicVest);
							break;
						}
					case eCharacterClass.Savage:
						{
							GiveItem(m_questPlayer, SavageEpicArms);
							GiveItem(m_questPlayer, SavageEpicBoots);
							GiveItem(m_questPlayer, SavageEpicGloves);
							GiveItem(m_questPlayer, SavageEpicHelm);
							GiveItem(m_questPlayer, SavageEpicLegs);
							GiveItem(m_questPlayer, SavageEpicVest);
							break;
						}
					case eCharacterClass.Valkyrie:
						{
							GiveItem(m_questPlayer, ValkyrieEpicArms);
							GiveItem(m_questPlayer, ValkyrieEpicBoots);
							GiveItem(m_questPlayer, ValkyrieEpicGloves);
							GiveItem(m_questPlayer, ValkyrieEpicHelm);
							GiveItem(m_questPlayer, ValkyrieEpicLegs);
							GiveItem(m_questPlayer, ValkyrieEpicVest);
							break;
						}
					case eCharacterClass.MaulerMid:
						{
							GiveItem(m_questPlayer, MaulerMidEpicArms);
							GiveItem(m_questPlayer, MaulerMidEpicBoots);
							GiveItem(m_questPlayer, MaulerMidEpicGloves);
							GiveItem(m_questPlayer, MaulerMidEpicHelm);
							GiveItem(m_questPlayer, MaulerMidEpicLegs);
							GiveItem(m_questPlayer, MaulerMidEpicVest);
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
	}
}
