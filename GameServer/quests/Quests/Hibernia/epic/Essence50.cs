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
*Source         : http://translate.google.com/translate?hl=en&sl=ja&u=http://ina.kappe.co.jp/~shouji/cgi-bin/nquest/nquest.html&prev=/search%3Fq%3DThe%2BMoonstone%2BTwin%2B(level%2B50)%26hl%3Den%26lr%3D%26safe%3Doff%26sa%3DG
*http://camelot.allakhazam.com/quests.html?realm=Hibernia&cquest=299
*Date           : 22 November 2004
*Quest Name     : The Moonstone Twin (level 50)
*Quest Classes  : Enchanter, Bard, Champion, Nighthsade(Path of Essence)
*Quest Version  : v1
*
*Done: 
*
*Bonuses to epic items 
*
*ToDo:
*   
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
	public class Essence_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "The Moonstone Twin";
		protected const int minimumLevel = 48;
		protected const int maximumLevel = 50;

		private static GameNPC Brigit = null; // Start NPC        
		private static GameNPC Caithor = null; // Mob to kill

		private static ItemTemplate Moonstone = null; //ball of flame

		private static ItemTemplate ChampionEpicBoots = null; //Mist Shrouded Boots 
		private static ItemTemplate ChampionEpicHelm = null; //Mist Shrouded Coif 
		private static ItemTemplate ChampionEpicGloves = null; //Mist Shrouded Gloves 
		private static ItemTemplate ChampionEpicVest = null; //Mist Shrouded Hauberk 
		private static ItemTemplate ChampionEpicLegs = null; //Mist Shrouded Legs 
		private static ItemTemplate ChampionEpicArms = null; //Mist Shrouded Sleeves 
		private static ItemTemplate BardEpicBoots = null; //Shadow Shrouded Boots 
		private static ItemTemplate BardEpicHelm = null; //Shadow Shrouded Coif 
		private static ItemTemplate BardEpicGloves = null; //Shadow Shrouded Gloves 
		private static ItemTemplate BardEpicVest = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate BardEpicLegs = null; //Shadow Shrouded Legs 
		private static ItemTemplate BardEpicArms = null; //Shadow Shrouded Sleeves 
		private static ItemTemplate EnchanterEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate EnchanterEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate EnchanterEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate EnchanterEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate EnchanterEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate EnchanterEpicArms = null; //Valhalla Touched Sleeves 
		private static ItemTemplate NightshadeEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate NightshadeEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate NightshadeEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate NightshadeEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate NightshadeEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate NightshadeEpicArms = null; //Subterranean Sleeves         

		// Constructors
		public Essence_50() : base()
		{
		}

		public Essence_50(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public Essence_50(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Essence_50(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Brigit", eRealm.Hibernia);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Brigit , creating it ...");
                Brigit = new GameNPC
                {
                    Model = 384,
                    Name = "Brigit",
                    GuildName = "",
                    Realm = eRealm.Hibernia,
                    CurrentRegionID = 201,
                    Size = 51,
                    Level = 50,
                    X = 33131,
                    Y = 32922,
                    Z = 8008,
                    Heading = 3254
                };
                Brigit.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Brigit.SaveIntoDatabase();
				}

			}
			else
				Brigit = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Caithor", eRealm.None);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Caithor , creating it ...");
                Caithor = new GameNPC
                {
                    Model = 339,
                    Name = "Caithor",
                    GuildName = "",
                    Realm = eRealm.None,
                    CurrentRegionID = 200,
                    Size = 60,
                    Level = 65,
                    X = 470547,
                    Y = 531497,
                    Z = 4984,
                    Heading = 3319
                };
                Caithor.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Caithor.SaveIntoDatabase();
				}

			}
			else
				Caithor = npcs[0];
			// end npc

			#endregion

			#region Item Declarations

			Moonstone = GameServer.Database.FindObjectByKey<ItemTemplate>("Moonstone");
			if (Moonstone == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Moonstone , creating it ...");
                Moonstone = new ItemTemplate
                {
                    Id_nb = "Moonstone",
                    Name = "Moonstone",
                    Level = 8,
                    Item_Type = 29,
                    Model = 514,
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
					GameServer.Database.AddObject(Moonstone);
				}

			}
// end item			
			BardEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("BardEpicBoots");
			if (BardEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bards Epic Boots , creating it ...");
                BardEpicBoots = new ItemTemplate
                {
                    Id_nb = "BardEpicBoots",
                    Name = "Moonsung Boots",
                    Level = 50,
                    Item_Type = 23,
                    Model = 738,
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

                    Bonus1 = 15,
                    Bonus1Type = (int)eStat.QUI,

                    Bonus2 = 10,
                    Bonus2Type = (int)eResist.Matter,

                    Bonus3 = 4,
                    Bonus3Type = (int)eProperty.PowerRegenerationRate,

                    Bonus4 = 33,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(BardEpicBoots);
				}

			}
//end item
			//Moonsung Coif 
			BardEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("BardEpicHelm");
			if (BardEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bards Epic Helm , creating it ...");
                BardEpicHelm = new ItemTemplate
                {
                    Id_nb = "BardEpicHelm",
                    Name = "Moonsung Coif",
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

                    Bonus1 = 18,
                    Bonus1Type = (int)eStat.CHR,

                    Bonus2 = 4,
                    Bonus2Type = (int)eProperty.PowerRegenerationRate,

                    Bonus3 = 3,
                    Bonus3Type = (int)eProperty.Skill_Regrowth,

                    Bonus4 = 21,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(BardEpicHelm);
				}


			}
//end item
			//Moonsung Gloves 
			BardEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("BardEpicGloves");
			if (BardEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bards Epic Gloves , creating it ...");
                BardEpicGloves = new ItemTemplate
                {
                    Id_nb = "BardEpicGloves",
                    Name = "Moonsung Gloves ",
                    Level = 50,
                    Item_Type = 22,
                    Model = 737,
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
                    Bonus1Type = (int)eProperty.Skill_Nurture,

                    Bonus2 = 3,
                    Bonus2Type = (int)eProperty.Skill_Music,

                    Bonus3 = 12,
                    Bonus3Type = (int)eStat.DEX,

                    Bonus4 = 33,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(BardEpicGloves);
				}

			}
			//Moonsung Hauberk 
			BardEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("BardEpicVest");
			if (BardEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bards Epic Vest , creating it ...");
                BardEpicVest = new ItemTemplate
                {
                    Id_nb = "BardEpicVest",
                    Name = "Moonsung Hauberk",
                    Level = 50,
                    Item_Type = 25,
                    Model = 734,
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
                    Bonus1Type = (int)eProperty.Skill_Regrowth,

                    Bonus2 = 3,
                    Bonus2Type = (int)eProperty.Skill_Nurture,

                    Bonus3 = 13,
                    Bonus3Type = (int)eStat.CON,

                    Bonus4 = 15,
                    Bonus4Type = (int)eStat.CHR
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(BardEpicVest);
				}

			}
			//Moonsung Legs 
			BardEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("BardEpicLegs");
			if (BardEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bards Epic Legs , creating it ...");
                BardEpicLegs = new ItemTemplate
                {
                    Id_nb = "BardEpicLegs",
                    Name = "Moonsung Legs",
                    Level = 50,
                    Item_Type = 27,
                    Model = 735,
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

                    Bonus1 = 16,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 10,
                    Bonus3Type = (int)eResist.Body,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Matter
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(BardEpicLegs);
				}

			}
			//Moonsung Sleeves 
			BardEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("BardEpicArms");
			if (BardEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bard Epic Arms , creating it ...");
                BardEpicArms = new ItemTemplate
                {
                    Id_nb = "BardEpicArms",
                    Name = "Moonsung Sleeves",
                    Level = 50,
                    Item_Type = 28,
                    Model = 736,
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

                    Bonus1 = 15,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 12,
                    Bonus2Type = (int)eStat.CHR,

                    Bonus3 = 10,
                    Bonus3Type = (int)eStat.CON,

                    Bonus4 = 12,
                    Bonus4Type = (int)eResist.Energy
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(BardEpicArms);
				}

			}
//Champion Epic Sleeves End
			ChampionEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("ChampionEpicBoots");
			if (ChampionEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Champions Epic Boots , creating it ...");
                ChampionEpicBoots = new ItemTemplate
                {
                    Id_nb = "ChampionEpicBoots",
                    Name = "Moonglow Boots",
                    Level = 50,
                    Item_Type = 23,
                    Model = 814,
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

                    Bonus1 = 33,
                    Bonus1Type = (int)eProperty.MaxHealth,

                    Bonus2 = 10,
                    Bonus2Type = (int)eResist.Heat,

                    Bonus3 = 10,
                    Bonus3Type = (int)eResist.Matter,

                    Bonus4 = 15,
                    Bonus4Type = (int)eStat.DEX
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ChampionEpicBoots);
				}

			}
//end item
			//Moonglow Coif 
			ChampionEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("ChampionEpicHelm");
			if (ChampionEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Champions Epic Helm , creating it ...");
                ChampionEpicHelm = new ItemTemplate
                {
                    Id_nb = "ChampionEpicHelm",
                    Name = "Moonglow Coif",
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

                    Bonus1 = 3,
                    Bonus1Type = (int)eProperty.Skill_Valor,

                    Bonus2 = 12,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 12,
                    Bonus3Type = (int)eStat.QUI,

                    Bonus4 = 6,
                    Bonus4Type = (int)eProperty.PowerRegenerationRate
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ChampionEpicHelm);
				}

			}
//end item
			//Moonglow Gloves 
			ChampionEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("ChampionEpicGloves");
			if (ChampionEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Champions Epic Gloves , creating it ...");
                ChampionEpicGloves = new ItemTemplate
                {
                    Id_nb = "ChampionEpicGloves",
                    Name = "Moonglow Gloves ",
                    Level = 50,
                    Item_Type = 22,
                    Model = 813,
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

                    Bonus1 = 3,
                    Bonus1Type = (int)eProperty.Skill_Parry,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.STR,

                    Bonus3 = 15,
                    Bonus3Type = (int)eStat.QUI,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Crush
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ChampionEpicGloves);
				}

			}
			//Moonglow Hauberk 
			ChampionEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("ChampionEpicVest");
			if (ChampionEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Champions Epic Vest , creating it ...");
                ChampionEpicVest = new ItemTemplate
                {
                    Id_nb = "ChampionEpicVest",
                    Name = "Moonglow Brestplate",
                    Level = 50,
                    Item_Type = 25,
                    Model = 810,
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
                    Bonus1Type = (int)eProperty.Skill_Valor,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.STR,

                    Bonus3 = 13,
                    Bonus3Type = (int)eStat.QUI,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Energy
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ChampionEpicVest);
				}

			}
			//Moonglow Legs 
			ChampionEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("ChampionEpicLegs");
			if (ChampionEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Champions Epic Legs , creating it ...");
                ChampionEpicLegs = new ItemTemplate
                {
                    Id_nb = "ChampionEpicLegs",
                    Name = "Moonglow Legs",
                    Level = 50,
                    Item_Type = 27,
                    Model = 811,
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
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 10,
                    Bonus3Type = (int)eResist.Crush,

                    Bonus4 = 18,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ChampionEpicLegs);
				}

			}
			//Moonglow Sleeves 
			ChampionEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("ChampionEpicArms");
			if (ChampionEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Champion Epic Arms , creating it ...");
                ChampionEpicArms = new ItemTemplate
                {
                    Id_nb = "ChampionEpicArms",
                    Name = "Moonglow Sleeves",
                    Level = 50,
                    Item_Type = 28,
                    Model = 812,
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

                    Bonus1 = 3,
                    Bonus1Type = (int)eProperty.Skill_Large_Weapon,

                    Bonus2 = 10,
                    Bonus2Type = (int)eStat.STR,

                    Bonus3 = 10,
                    Bonus3Type = (int)eStat.QUI,

                    Bonus4 = 33,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(ChampionEpicArms);
				}

			}
			NightshadeEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("NightshadeEpicBoots");
			if (NightshadeEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Nightshade Epic Boots , creating it ...");
                NightshadeEpicBoots = new ItemTemplate
                {
                    Id_nb = "NightshadeEpicBoots",
                    Name = "Moonlit Boots",
                    Level = 50,
                    Item_Type = 23,
                    Model = 750,
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

                    Bonus1 = 12,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 10,
                    Bonus3Type = (int)eResist.Thrust,

                    Bonus4 = 24,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(NightshadeEpicBoots);
				}

			}
//end item
			//Moonlit Coif 
			NightshadeEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("NightshadeEpicHelm");
			if (NightshadeEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Nightshade Epic Helm , creating it ...");
                NightshadeEpicHelm = new ItemTemplate
                {
                    Id_nb = "NightshadeEpicHelm",
                    Name = "Moonlit Helm",
                    Level = 50,
                    Item_Type = 21,
                    Model = 1292, //NEED TO WORK ON..
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

                    Bonus1 = 9,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 9,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 9,
                    Bonus3Type = (int)eStat.QUI,

                    Bonus4 = 39,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(NightshadeEpicHelm);
				}

			}
//end item
			//Moonlit Gloves 
			NightshadeEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("NightshadeEpicGloves");
			if (NightshadeEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Nightshade Epic Gloves , creating it ...");
                NightshadeEpicGloves = new ItemTemplate
                {
                    Id_nb = "NightshadeEpicGloves",
                    Name = "Moonlit Gloves ",
                    Level = 50,
                    Item_Type = 22,
                    Model = 749,
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

                    Bonus1 = 2,
                    Bonus1Type = (int)eProperty.Skill_Critical_Strike,

                    Bonus2 = 12,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 13,
                    Bonus3Type = (int)eStat.QUI,

                    Bonus4 = 5,
                    Bonus4Type = (int)eProperty.Skill_Envenom
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(NightshadeEpicGloves);
				}

			}
			//Moonlit Hauberk 
			NightshadeEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("NightshadeEpicVest");
			if (NightshadeEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Nightshade Epic Vest , creating it ...");
                NightshadeEpicVest = new ItemTemplate
                {
                    Id_nb = "NightshadeEpicVest",
                    Name = "Moonlit Leather Jerking",
                    Level = 50,
                    Item_Type = 25,
                    Model = 746,
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

                    Bonus1 = 10,
                    Bonus1Type = (int)eStat.STR,

                    Bonus2 = 10,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 30,
                    Bonus3Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(NightshadeEpicVest);
				}

			}
			//Moonlit Legs 
			NightshadeEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("NightshadeEpicLegs");
			if (NightshadeEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Nightshade Epic Legs , creating it ...");
                NightshadeEpicLegs = new ItemTemplate
                {
                    Id_nb = "NightshadeEpicLegs",
                    Name = "Moonlit Leggings",
                    Level = 50,
                    Item_Type = 27,
                    Model = 747,
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

                    Bonus1 = 16,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 10,
                    Bonus3Type = (int)eResist.Crush,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Slash
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(NightshadeEpicLegs);
				}

			}
			//Moonlit Sleeves 
			NightshadeEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("NightshadeEpicArms");
			if (NightshadeEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Nightshade Epic Arms , creating it ...");
                NightshadeEpicArms = new ItemTemplate
                {
                    Id_nb = "NightshadeEpicArms",
                    Name = "Moonlit Sleeves",
                    Level = 50,
                    Item_Type = 28,
                    Model = 748,
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

                    Bonus1 = 4,
                    Bonus1Type = (int)eProperty.Skill_Celtic_Dual,

                    Bonus2 = 16,
                    Bonus2Type = (int)eStat.CON,

                    Bonus3 = 15,
                    Bonus3Type = (int)eStat.DEX,

                    Bonus4 = 6,
                    Bonus4Type = (int)eResist.Cold
                };

                if (SAVE_INTO_DATABASE)
				{
                    GameServer.Database.AddObject(NightshadeEpicArms);
				}

			}
			EnchanterEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("EnchanterEpicBoots");
			if (EnchanterEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Enchanter Epic Boots , creating it ...");
                EnchanterEpicBoots = new ItemTemplate
                {
                    Id_nb = "EnchanterEpicBoots",
                    Name = "Moonspun Boots",
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

                    Bonus1 = 12,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 12,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 12,
                    Bonus3Type = (int)eResist.Body,

                    Bonus4 = 39,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(EnchanterEpicBoots);
				}

			}
//end item
			//Moonspun Coif 
			EnchanterEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("EnchanterEpicHelm");
			if (EnchanterEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Enchanter Epic Helm , creating it ...");
                EnchanterEpicHelm = new ItemTemplate
                {
                    Id_nb = "EnchanterEpicHelm",
                    Name = "Moonspun Cap",
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

                    Bonus1 = 21,
                    Bonus1Type = (int)eProperty.MaxHealth,

                    Bonus2 = 8,
                    Bonus2Type = (int)eResist.Energy,

                    Bonus3 = 4,
                    Bonus3Type = (int)eProperty.Skill_Enchantments,

                    Bonus4 = 18,
                    Bonus4Type = (int)eStat.INT
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(EnchanterEpicHelm);
				}

			}
//end item
			//Moonspun Gloves 
			EnchanterEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("EnchanterEpicGloves");
			if (EnchanterEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Enchanter Epic Gloves , creating it ...");
                EnchanterEpicGloves = new ItemTemplate
                {
                    Id_nb = "EnchanterEpicGloves",
                    Name = "Moonspun Gloves ",
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

                    Bonus1 = 30,
                    Bonus1Type = (int)eProperty.MaxHealth,

                    Bonus2 = 4,
                    Bonus2Type = (int)eProperty.Skill_Mana,

                    Bonus3 = 6,
                    Bonus3Type = (int)eStat.INT,

                    Bonus4 = 13,
                    Bonus4Type = (int)eStat.DEX
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(EnchanterEpicGloves);
				}

			}
			//Moonspun Hauberk 
			EnchanterEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("EnchanterEpicVest");
			if (EnchanterEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Enchanter Epic Vest , creating it ...");
                EnchanterEpicVest = new ItemTemplate
                {
                    Id_nb = "EnchanterEpicVest",
                    Name = "Moonspun Vest",
                    Level = 50,
                    Item_Type = 25,
                    Model = 781,
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

                    Bonus1 = 30,
                    Bonus1Type = (int)eProperty.MaxHealth,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.INT,

                    Bonus3 = 15,
                    Bonus3Type = (int)eStat.DEX
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(EnchanterEpicVest);
				}

			}
			//Moonspun Legs 
			EnchanterEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("EnchanterEpicLegs");
			if (EnchanterEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Enchanter Epic Legs , creating it ...");
                EnchanterEpicLegs = new ItemTemplate
                {
                    Id_nb = "EnchanterEpicLegs",
                    Name = "Moonspun Pants",
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

                    Bonus1 = 16,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 15,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 10,
                    Bonus3Type = (int)eResist.Heat,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Cold
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(EnchanterEpicLegs);
				}

			}
			//Moonspun Sleeves 
			EnchanterEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("EnchanterEpicArms");
			if (EnchanterEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Enchanter Epic Arms , creating it ...");
                EnchanterEpicArms = new ItemTemplate
                {
                    Id_nb = "EnchanterEpicArms",
                    Name = "Moonspun Sleeves",
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

                    Bonus1 = 27,
                    Bonus1Type = (int)eProperty.MaxHealth,

                    Bonus2 = 10,
                    Bonus2Type = (int)eStat.INT,

                    Bonus3 = 5,
                    Bonus3Type = (int)eProperty.Skill_Light,

                    Bonus4 = 10,
                    Bonus4Type = (int)eStat.DEX
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(EnchanterEpicArms);
				}

			}

//Champion Epic Sleeves End
//Item Descriptions End

			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(Brigit, GameObjectEvent.Interact, new DOLEventHandler(TalkToBrigit));
			GameEventMgr.AddHandler(Brigit, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBrigit));

			/* Now we bring to Brigit the possibility to give this quest to players */
			Brigit.AddQuestToGive(typeof (Essence_50));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Brigit == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(Brigit, GameObjectEvent.Interact, new DOLEventHandler(TalkToBrigit));
			GameEventMgr.RemoveHandler(Brigit, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBrigit));

			/* Now we remove to Brigit the possibility to give this quest to players */
			Brigit.RemoveQuestToGive(typeof (Essence_50));
		}

		protected static void TalkToBrigit(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Brigit.CanGiveQuest(typeof (Essence_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Essence_50 quest = player.IsDoingQuest(typeof (Essence_50)) as Essence_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					Brigit.SayTo(player, "Check your Journal for instructions!");
				}
				else
				{
					Brigit.SayTo(player, "Hibernia needs your [services]");
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
							player.Out.SendQuestSubscribeCommand(Brigit, QuestMgr.GetIDForQuestType(typeof(Essence_50)), "Will you help Brigit [Path of Essence Level 50 Epic]?");
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
			if (player.IsDoingQuest(typeof (Essence_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte) eCharacterClass.Champion &&
				player.CharacterClass.ID != (byte) eCharacterClass.Bard &&
				player.CharacterClass.ID != (byte) eCharacterClass.Nightshade &&
				player.CharacterClass.ID != (byte) eCharacterClass.Enchanter)
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
			Essence_50 quest = player.IsDoingQuest(typeof (Essence_50)) as Essence_50;

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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Essence_50)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if(Brigit.CanGiveQuest(typeof (Essence_50), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (Essence_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!Brigit.GiveQuest(typeof (Essence_50), player, 1))
					return;
				player.Out.SendMessage("Kill Caithor in Cursed Forest loc 28k 48k ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "The Moonstone Twin (Level 50 Path of Essence Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Seek out Caithor in Cursed Forest Loc 20k,48k kill him!";
					case 2:
						return "[Step #2] Return to Brigit and give the Moonstone!";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Essence_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				//if (gArgs.Target.Name == Caithor.Name)
                    if (gArgs.Target.Name.IndexOf(Caithor.Name) >= 0)
                    {
					m_questPlayer.Out.SendMessage("You collect the Moonstone from Caithor", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					GiveItem(m_questPlayer, Moonstone);
					Step = 2;
					return;
				}

			}

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Brigit.Name && gArgs.Item.Id_nb == Moonstone.Id_nb)
				{
					Brigit.SayTo(player, "You have earned this Epic Armour!");
					FinishQuest();
					return;
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, Moonstone, false);
		}

		public override void FinishQuest()
		{
			if (m_questPlayer.Inventory.IsSlotsFree(6, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
			{
				RemoveItem(Brigit, m_questPlayer, Moonstone);

				base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

				if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Champion)
				{
					GiveItem(m_questPlayer, ChampionEpicArms);
					GiveItem(m_questPlayer, ChampionEpicBoots);
					GiveItem(m_questPlayer, ChampionEpicGloves);
					GiveItem(m_questPlayer, ChampionEpicHelm);
					GiveItem(m_questPlayer, ChampionEpicLegs);
					GiveItem(m_questPlayer, ChampionEpicVest);
				}
				else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Bard)
				{
					GiveItem(m_questPlayer, BardEpicArms);
					GiveItem(m_questPlayer, BardEpicBoots);
					GiveItem(m_questPlayer, BardEpicGloves);
					GiveItem(m_questPlayer, BardEpicHelm);
					GiveItem(m_questPlayer, BardEpicLegs);
					GiveItem(m_questPlayer, BardEpicVest);
				}
				else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Enchanter)
				{
					GiveItem(m_questPlayer, EnchanterEpicArms);
					GiveItem(m_questPlayer, EnchanterEpicBoots);
					GiveItem(m_questPlayer, EnchanterEpicGloves);
					GiveItem(m_questPlayer, EnchanterEpicHelm);
					GiveItem(m_questPlayer, EnchanterEpicLegs);
					GiveItem(m_questPlayer, EnchanterEpicVest);
				}
				else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Nightshade)
				{
					GiveItem(m_questPlayer, NightshadeEpicArms);
					GiveItem(m_questPlayer, NightshadeEpicBoots);
					GiveItem(m_questPlayer, NightshadeEpicGloves);
					GiveItem(m_questPlayer, NightshadeEpicHelm);
					GiveItem(m_questPlayer, NightshadeEpicLegs);
					GiveItem(m_questPlayer, NightshadeEpicVest);
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
        *#25 talk to Brigit
        *#26 seek out Loken in Raumarik Loc 47k, 25k, 4k, and kill him purp and 2 blue adds 
        *#27 return to Brigit 
        *#28 give her the ball of flame
        *#29 talk with Brigit about Loken’s demise
        *#30 go to MorlinCaan in Jordheim 
        *#31 give her the sealed pouch
        *#32 you get your epic armor as a reward
        */

		/*
            *Moonsung Boots 
            *Moonsung Coif
            *Moonsung Gloves
            *Moonsung Hauberk
            *Moonsung Legs
            *Moonsung Sleeves
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
