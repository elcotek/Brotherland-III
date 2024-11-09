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
*Date           : 21 November 2004
*Quest Name     : The Desire of a God (Level 50)
*Quest Classes  : Healer, Shaman (Seer)
*Quest Version  : v1.2
*
*Changes:
*   The epic armour should now have the correct durability and condition
*   The armour will now be correctly rewarded with all peices
*   The items used in the quest cannot be traded or dropped
*   The items / itemtemplates / NPCs are created if they are not found
*   Add bonuses to epic items
*ToDo:
*   Add correct Text
*   Find Helm ModelID for epics..
*/

using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Midgard
{
	public class Seer_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "The Desire of a God";
		protected const int minimumLevel = 48;
		protected const int maximumLevel = 50;

		private static GameNPC Inaksha = null; // Start NPC
		private static GameNPC Loken = null; // Mob to kill
		private static GameNPC Miri = null; // Trainer for reward

		private static ItemTemplate ball_of_flame = null; //ball of flame
		private static ItemTemplate sealed_pouch = null; //sealed pouch
		private static ItemTemplate HealerEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate HealerEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate HealerEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate HealerEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate HealerEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate HealerEpicArms = null; //Valhalla Touched Sleeves 
		private static ItemTemplate ShamanEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate ShamanEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate ShamanEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate ShamanEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate ShamanEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate ShamanEpicArms = null; //Subterranean Sleeves         

		// Constructors
		public Seer_50() : base()
		{
		}

		public Seer_50(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public Seer_50(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Seer_50(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Inaksha", eRealm.Midgard);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Inaksha , creating it ...");
                Inaksha = new GameNPC
                {
                    Model = 193,
                    Name = "Inaksha",
                    GuildName = String.Empty,
                    Realm = eRealm.Midgard,
                    CurrentRegionID = 100,
                    Size = 50,
                    Level = 41,
                    X = 805929,
                    Y = 702449,
                    Z = 4960,
                    Heading = 2116
                };
                Inaksha.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Inaksha.SaveIntoDatabase();
				}

			}
			else
				Inaksha = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Loken", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Loken , creating it ...");
                Loken = new GameNPC
                {
                    Model = 212,
                    Name = "Loken",
                    GuildName = String.Empty,
                    Realm = eRealm.None,
                    CurrentRegionID = 100,
                    Size = 50,
                    Level = 65,
                    X = 636784,
                    Y = 762433,
                    Z = 4596,
                    Heading = 3777
                };
                Loken.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Loken.SaveIntoDatabase();
				}

			}
			else
				Loken = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Miri", eRealm.Midgard);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Miri , creating it ...");
                Miri = new GameNPC
                {
                    Model = 220,
                    Name = "Miri",
                    GuildName = String.Empty,
                    Realm = eRealm.Midgard,
                    CurrentRegionID = 101,
                    Size = 50,
                    Level = 43,
                    X = 30641,
                    Y = 32093,
                    Z = 8305,
                    Heading = 3037
                };
                Miri.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Miri.SaveIntoDatabase();
				}

			}
			else
				Miri = npcs[0];
			// end npc

			#endregion

			#region defineItems

			ball_of_flame = GameServer.Database.FindObjectByKey<ItemTemplate>("ball_of_flame");
			if (ball_of_flame == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find ball_of_flame , creating it ...");
                ball_of_flame = new ItemTemplate
                {
                    Id_nb = "ball_of_flame",
                    Name = "Ball of Flame",
                    Level = 8,
                    Item_Type = 29,
                    Model = 601,
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
					GameServer.Database.AddObject(ball_of_flame);
				}
			}

// end item
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

			//Valhalla Touched Boots
			HealerEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("HealerEpicBoots");
			if (HealerEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Healers Epic Boots , creating it ...");
                HealerEpicBoots = new ItemTemplate
                {
                    Id_nb = "HealerEpicBoots",
                    Name = "Valhalla Touched Boots",
                    Level = 50,
                    Item_Type = 23,
                    Model = 702,
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
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 12,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 12,
                    Bonus3Type = (int)eStat.QUI,

                    Bonus4 = 21,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(HealerEpicBoots);
				}
			}
//end item
			//Valhalla Touched Coif 
			HealerEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("HealerEpicHelm");
			if (HealerEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Healers Epic Helm , creating it ...");
                HealerEpicHelm = new ItemTemplate
                {
                    Id_nb = "HealerEpicHelm",
                    Name = "Valhalla Touched Coif",
                    Level = 50,
                    Item_Type = 21,
                    Model = 1291, //NEED TO WORK ON..
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
                    Bonus1Type = (int)eProperty.Skill_Augmentation,

                    Bonus2 = 18,
                    Bonus2Type = (int)eStat.PIE,

                    Bonus3 = 4,
                    Bonus3Type = (int)eResist.Slash,

                    Bonus4 = 6,
                    Bonus4Type = (int)eProperty.PowerRegenerationRate
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(HealerEpicHelm);
				}

			}
//end item
			//Valhalla Touched Gloves 
			HealerEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("HealerEpicGloves");
			if (HealerEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Healers Epic Gloves , creating it ...");
                HealerEpicGloves = new ItemTemplate
                {
                    Id_nb = "HealerEpicGloves",
                    Name = "Valhalla Touched Gloves ",
                    Level = 50,
                    Item_Type = 22,
                    Model = 701,
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
                    Bonus1Type = (int)eProperty.Skill_Mending,

                    Bonus2 = 16,
                    Bonus2Type = (int)eStat.PIE,

                    Bonus3 = 4,
                    Bonus3Type = (int)eResist.Crush,

                    Bonus4 = 6,
                    Bonus4Type = (int)eProperty.PowerRegenerationRate
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(HealerEpicGloves);
				}

			}
			//Valhalla Touched Hauberk 
			HealerEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("HealerEpicVest");
			if (HealerEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Healers Epic Vest , creating it ...");
                HealerEpicVest = new ItemTemplate
                {
                    Id_nb = "HealerEpicVest",
                    Name = "Valhalla Touched Haukberk",
                    Level = 50,
                    Item_Type = 25,
                    Model = 698,
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

                    Bonus2 = 16,
                    Bonus2Type = (int)eStat.PIE,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Cold,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Heat
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(HealerEpicVest);
				}

			}
			//Valhalla Touched Legs 
			HealerEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("HealerEpicLegs");
			if (HealerEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Healers Epic Legs , creating it ...");
                HealerEpicLegs = new ItemTemplate
                {
                    Id_nb = "HealerEpicLegs",
                    Name = "Valhalla Touched Legs",
                    Level = 50,
                    Item_Type = 27,
                    Model = 699,
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
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 16,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 10,
                    Bonus3Type = (int)eResist.Spirit,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Energy
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(HealerEpicLegs);
				}

			}
			//Valhalla Touched Sleeves 
			HealerEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("HealerEpicArms");
			if (HealerEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Healer Epic Arms , creating it ...");
                HealerEpicArms = new ItemTemplate
                {
                    Id_nb = "HealerEpicArms",
                    Name = "Valhalla Touched Sleeves",
                    Level = 50,
                    Item_Type = 28,
                    Model = 700,
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
                    Bonus1Type = (int)eProperty.Skill_Mending,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.STR,

                    Bonus3 = 15,
                    Bonus3Type = (int)eStat.PIE,

                    Bonus4 = 6,
                    Bonus4Type = (int)eResist.Matter
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(HealerEpicArms);
				}

			}
			//Subterranean Boots 
			ShamanEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("ShamanEpicBoots");
			if (ShamanEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shaman Epic Boots , creating it ...");
                ShamanEpicBoots = new ItemTemplate
                {
                    Id_nb = "ShamanEpicBoots",
                    Name = "Subterranean Boots",
                    Level = 50,
                    Item_Type = 23,
                    Model = 770,
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
                    Bonus1Type = (int)eStat.DEX,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.QUI,

                    Bonus3 = 39,
                    Bonus3Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ShamanEpicBoots);
				}

			}
			//Subterranean Coif 
			ShamanEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("ShamanEpicHelm");
			if (ShamanEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shaman Epic Helm , creating it ...");
                ShamanEpicHelm = new ItemTemplate
                {
                    Id_nb = "ShamanEpicHelm",
                    Name = "Subterranean Coif",
                    Level = 50,
                    Item_Type = 21,
                    Model = 63, //NEED TO WORK ON..
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
                    Bonus1Type = (int)eProperty.Skill_Mending,

                    Bonus2 = 18,
                    Bonus2Type = (int)eStat.PIE,

                    Bonus3 = 4,
                    Bonus3Type = (int)eResist.Thrust,

                    Bonus4 = 6,
                    Bonus4Type = (int)eProperty.PowerRegenerationRate
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ShamanEpicHelm);
				}

			}
			//Subterranean Gloves 
			ShamanEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("ShamanEpicGloves");
			if (ShamanEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shaman Epic Gloves , creating it ...");
                ShamanEpicGloves = new ItemTemplate
                {
                    Id_nb = "ShamanEpicGloves",
                    Name = "Subterranean Gloves",
                    Level = 50,
                    Item_Type = 22,
                    Model = 769,
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
                    Bonus1Type = (int)eProperty.Skill_Subterranean,

                    Bonus2 = 18,
                    Bonus2Type = (int)eStat.PIE,

                    Bonus3 = 4,
                    Bonus3Type = (int)eResist.Crush,

                    Bonus4 = 6,
                    Bonus4Type = (int)eProperty.PowerRegenerationRate
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ShamanEpicGloves);
				}

			}
			//Subterranean Hauberk 
			ShamanEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("ShamanEpicVest");
			if (ShamanEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shaman Epic Vest , creating it ...");
                ShamanEpicVest = new ItemTemplate
                {
                    Id_nb = "ShamanEpicVest",
                    Name = "Subterranean Hauberk",
                    Level = 50,
                    Item_Type = 25,
                    Model = 766,
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

                    Bonus2 = 16,
                    Bonus2Type = (int)eStat.PIE,

                    Bonus3 = 10,
                    Bonus3Type = (int)eResist.Matter,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Heat
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ShamanEpicVest);
				}

			}
			//Subterranean Legs 
			ShamanEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("ShamanEpicLegs");
			if (ShamanEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shaman Epic Legs , creating it ...");
                ShamanEpicLegs = new ItemTemplate
                {
                    Id_nb = "ShamanEpicLegs",
                    Name = "Subterranean Legs",
                    Level = 50,
                    Item_Type = 27,
                    Model = 767,
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

                    Bonus2 = 16,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Cold,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Spirit
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ShamanEpicLegs);
				}

			}
			//Subterranean Sleeves 
			ShamanEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("ShamanEpicArms");
			if (ShamanEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Shaman Epic Arms , creating it ...");
                ShamanEpicArms = new ItemTemplate
                {
                    Id_nb = "ShamanEpicArms",
                    Name = "Subterranean Sleeves",
                    Level = 50,
                    Item_Type = 28,
                    Model = 768,
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
                    Bonus1Type = (int)eProperty.Skill_Augmentation,

                    Bonus2 = 12,
                    Bonus2Type = (int)eStat.STR,

                    Bonus3 = 18,
                    Bonus3Type = (int)eStat.PIE,

                    Bonus4 = 6,
                    Bonus4Type = (int)eResist.Energy
                };


                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ShamanEpicArms);
				}

			}
//Shaman Epic Sleeves End
//Item Descriptions End

			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(Inaksha, GameObjectEvent.Interact, new DOLEventHandler(TalkToInaksha));
			GameEventMgr.AddHandler(Inaksha, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToInaksha));
			GameEventMgr.AddHandler(Miri, GameObjectEvent.Interact, new DOLEventHandler(TalkToMiri));
			GameEventMgr.AddHandler(Miri, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMiri));

			/* Now we bring to Inaksha the possibility to give this quest to players */
			Inaksha.AddQuestToGive(typeof (Seer_50));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Inaksha == null || Miri == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(Inaksha, GameObjectEvent.Interact, new DOLEventHandler(TalkToInaksha));
			GameEventMgr.RemoveHandler(Inaksha, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToInaksha));
			GameEventMgr.RemoveHandler(Miri, GameObjectEvent.Interact, new DOLEventHandler(TalkToMiri));
			GameEventMgr.RemoveHandler(Miri, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMiri));

			/* Now we remove to Inaksha the possibility to give this quest to players */
			Inaksha.RemoveQuestToGive(typeof (Seer_50));
		}

		protected static void TalkToInaksha(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Inaksha.CanGiveQuest(typeof (Seer_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Seer_50 quest = player.IsDoingQuest(typeof (Seer_50)) as Seer_50;

			if (e == GameObjectEvent.Interact)
			{
				// Nag to finish quest
				if (quest != null)
				{
					Inaksha.SayTo(player, "Check your Journal for instructions!");
				}
				else
				{
					Inaksha.SayTo(player, "Midgard needs your [services]");
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
							player.Out.SendQuestSubscribeCommand(Inaksha, QuestMgr.GetIDForQuestType(typeof(Seer_50)), "Will you help Inaksha [Seer Level 50 Epic]?");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "dead":
							if (quest.Step == 3)
							{
								Inaksha.SayTo(player, "Take this sealed pouch to Miri in Jordheim for your reward!");
								GiveItem(Inaksha, player, sealed_pouch);
								quest.Step = 4;
							}
							break;
						case "abort":
							player.Out.SendCustomDialog("Do you really want to abort this quest, \nall items gained during quest will be lost?", new CustomDialogResponse(CheckPlayerAbortQuest));
							break;
					}
				}

			}

		}

		protected static void TalkToMiri(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Inaksha.CanGiveQuest(typeof (Seer_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Seer_50 quest = player.IsDoingQuest(typeof (Seer_50)) as Seer_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					Miri.SayTo(player, "Check your journal for instructions!");
				}
				else
				{
					Miri.SayTo(player, "I need your help to seek out loken in raumarik Loc 47k, 25k, 4k, and kill him ");
				}
			}

		}

		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (Seer_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte) eCharacterClass.Shaman &&
				player.CharacterClass.ID != (byte) eCharacterClass.Healer)
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
			Seer_50 quest = player.IsDoingQuest(typeof (Seer_50)) as Seer_50;

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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Seer_50)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if(Inaksha.CanGiveQuest(typeof (Seer_50), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (Seer_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!Inaksha.GiveQuest(typeof (Seer_50), player, 1))
					return;

				player.Out.SendMessage("Good now go kill him!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "The Desire of a God (Level 50 Seer Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Seek out Loken in Raumarik Loc 47k, 25k kill him!";
					case 2:
						return "[Step #2] Return to Inaksha and give her the Ball of Flame!";
					case 3:
						return "[Step #3] Talk with Inaksha about Loken’s demise!";
					case 4:
						return "[Step #4] Go to Miri in Jordheim and give her the Sealed Pouch for your reward!";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(Seer_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if (gArgs.Target.Name == Loken.Name)
				{
					m_questPlayer.Out.SendMessage("You get a ball of flame", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					GiveItem(m_questPlayer, ball_of_flame);
					Step = 2;
					return;
				}
			}

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Inaksha.Name && gArgs.Item.Id_nb == ball_of_flame.Id_nb)
				{
					RemoveItem(Inaksha, player, ball_of_flame);
					Inaksha.SayTo(player, "So it seems Logan's [dead]");
					Step = 3;
					return;
				}
			}

			if (Step == 4 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Miri.Name && gArgs.Item.Id_nb == sealed_pouch.Id_nb)
				{
					Miri.SayTo(player, "You have earned this Epic Armour!");
					FinishQuest();
					return;
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, sealed_pouch, false);
			RemoveItem(m_questPlayer, ball_of_flame, false);
		}

		public override void FinishQuest()
		{
			if (m_questPlayer.Inventory.IsSlotsFree(6, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
			{
				RemoveItem(Miri, m_questPlayer, sealed_pouch);

				base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

				if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Shaman)
				{
					GiveItem(m_questPlayer, ShamanEpicArms);
					GiveItem(m_questPlayer, ShamanEpicBoots);
					GiveItem(m_questPlayer, ShamanEpicGloves);
					GiveItem(m_questPlayer, ShamanEpicHelm);
					GiveItem(m_questPlayer, ShamanEpicLegs);
					GiveItem(m_questPlayer, ShamanEpicVest);
				}
				else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Healer)
				{
					GiveItem(m_questPlayer, HealerEpicArms);
					GiveItem(m_questPlayer, HealerEpicBoots);
					GiveItem(m_questPlayer, HealerEpicGloves);
					GiveItem(m_questPlayer, HealerEpicHelm);
					GiveItem(m_questPlayer, HealerEpicLegs);
					GiveItem(m_questPlayer, HealerEpicVest);
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
        *#25 talk to Inaksha
        *#26 seek out Loken in Raumarik Loc 47k, 25k, 4k, and kill him purp and 2 blue adds 
        *#27 return to Inaksha 
        *#28 give her the ball of flame
        *#29 talk with Inaksha about Loken’s demise
        *#30 go to Miri in Jordheim 
        *#31 give her the sealed pouch
        *#32 you get your epic armor as a reward
        */

		/*
            *Valhalla Touched Boots 
            *Valhalla Touched Coif
            *Valhalla Touched Gloves
            *Valhalla Touched Hauberk
            *Valhalla Touched Legs
            *Valhalla Touched Sleeves
            *Subterranean Boots
            *Subterranean Coif
            *Subterranean Gloves
            *Subterranean Hauberk
            *Subterranean Legs
            *Subterranean Sleeves
        */

		#endregion
	}
}
