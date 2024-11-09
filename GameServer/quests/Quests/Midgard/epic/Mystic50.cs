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
*               : http://daoc.warcry.com/quests/display_spoilquest.php?id=741
*Date           : 22 November 2004
*Quest Name     : Saving the Clan (Level 50)
*Quest Classes  : Runemaster, Bonedancer, Spiritmaster (Mystic)
*Quest Version  : v1.2
*
*Changes:
*   Fixed the texts to be like live.
*   The epic armor should now be described correctly
*   The epic armor should now fit into the correct slots
*   The epic armor should now have the correct durability and condition
*   The armour will now be correctly rewarded with all pieces
*   The items used in the quest cannot be traded or dropped
*   The items / itemtemplates / NPCs are created if they are not found
*
*ToDo:
*   Find Helm ModelID for epics..
*   checks for all other epics done, once they are implemented
*/

using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Midgard
{
	public class Mystic_50 : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "Saving the Clan";
		protected const int minimumLevel = 48;
		protected const int maximumLevel = 50;

		private static GameNPC Danica = null; // Start NPC
		private static GameNPC Kelic = null; // Mob to kill

		private static ItemTemplate kelics_totem = null;
		private static ItemTemplate SpiritmasterEpicBoots = null;
		private static ItemTemplate SpiritmasterEpicHelm = null;
		private static ItemTemplate SpiritmasterEpicGloves = null;
		private static ItemTemplate SpiritmasterEpicLegs = null;
		private static ItemTemplate SpiritmasterEpicArms = null;
		private static ItemTemplate SpiritmasterEpicVest = null;
		private static ItemTemplate RunemasterEpicBoots = null;
		private static ItemTemplate RunemasterEpicHelm = null;
		private static ItemTemplate RunemasterEpicGloves = null;
		private static ItemTemplate RunemasterEpicLegs = null;
		private static ItemTemplate RunemasterEpicArms = null;
		private static ItemTemplate RunemasterEpicVest = null;
		private static ItemTemplate BonedancerEpicBoots = null;
		private static ItemTemplate BonedancerEpicHelm = null;
		private static ItemTemplate BonedancerEpicGloves = null;
		private static ItemTemplate BonedancerEpicLegs = null;
		private static ItemTemplate BonedancerEpicArms = null;
		private static ItemTemplate BonedancerEpicVest = null;
		private static ItemTemplate WarlockEpicBoots = null;
		private static ItemTemplate WarlockEpicHelm = null;
		private static ItemTemplate WarlockEpicGloves = null;
		private static ItemTemplate WarlockEpicLegs = null;
		private static ItemTemplate WarlockEpicArms = null;
		private static ItemTemplate WarlockEpicVest = null;

		// Constructors
		public Mystic_50() : base()
		{
		}

		public Mystic_50(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public Mystic_50(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Mystic_50(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Danica", eRealm.Midgard);

			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Danica , creating it ...");
                Danica = new GameNPC
                {
                    Model = 227,
                    Name = "Danica",
                    GuildName = "",
                    Realm = eRealm.Midgard,
                    CurrentRegionID = 100,
                    Size = 51,
                    Level = 50,
                    X = 804440,
                    Y = 722267,
                    Z = 4719,
                    Heading = 2116
                };
                Danica.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Danica.SaveIntoDatabase();
				}
			}
			else
				Danica = npcs[0];
			// end npc

			npcs = WorldMgr.GetNPCsByName("Kelic", eRealm.None);
			if (npcs.Length == 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Kelic , creating it ...");
                Kelic = new GameNPC
                {
                    Model = 26,
                    Name = "Kelic",
                    GuildName = String.Empty,
                    Realm = eRealm.None,
                    CurrentRegionID = 100,
                    Size = 100,
                    Level = 65,
                    X = 621577,
                    Y = 745848,
                    Z = 4593,
                    Heading = 3538
                };
                Kelic.Flags ^= GameNPC.eFlags.GHOST;
				Kelic.MaxSpeedBase = 200;
				Kelic.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					Kelic.SaveIntoDatabase();
				}
			}
			else
				Kelic = npcs[0];
			// end npc

			#endregion

			#region defineItems

			kelics_totem = GameServer.Database.FindObjectByKey<ItemTemplate>("kelics_totem");
			if (kelics_totem == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Kelic's Totem , creating it ...");
                kelics_totem = new ItemTemplate
                {
                    Id_nb = "kelics_totem",
                    Name = "Kelic's Totem",
                    Level = 8,
                    Item_Type = 0,
                    Model = 488,
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
					GameServer.Database.AddObject(kelics_totem);
				}

			}

			SpiritmasterEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("SpiritmasterEpicBoots");
			if (SpiritmasterEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Spiritmaster Epic Boots , creating it ...");
                SpiritmasterEpicBoots = new ItemTemplate
                {
                    Id_nb = "SpiritmasterEpicBoots",
                    Name = "Spirit Touched Boots",
                    Level = 50,
                    Item_Type = 23,
                    Model = 803,
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

                    Bonus2 = 16,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Matter,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Heat
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(SpiritmasterEpicBoots);
				}

			}
//end item
			SpiritmasterEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("SpiritmasterEpicHelm");
			if (SpiritmasterEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Spiritmaster Epic Helm , creating it ...");
                SpiritmasterEpicHelm = new ItemTemplate
                {
                    Id_nb = "SpiritmasterEpicHelm",
                    Name = "Spirit Touched Cap",
                    Level = 50,
                    Item_Type = 21,
                    Model = 825, //NEED TO WORK ON..
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
                    Bonus1Type = (int)eProperty.Focus_Darkness,

                    Bonus2 = 4,
                    Bonus2Type = (int)eProperty.Focus_Suppression,

                    Bonus3 = 13,
                    Bonus3Type = (int)eStat.PIE,

                    Bonus4 = 4,
                    Bonus4Type = (int)eProperty.PowerRegenerationRate
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(SpiritmasterEpicHelm);
				}

			}
//end item
			SpiritmasterEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("SpiritmasterEpicGloves");
			if (SpiritmasterEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Spiritmaster Epic Gloves , creating it ...");
                SpiritmasterEpicGloves = new ItemTemplate
                {
                    Id_nb = "SpiritmasterEpicGloves",
                    Name = "Spirit Touched Gloves ",
                    Level = 50,
                    Item_Type = 22,
                    Model = 802,
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
                    Bonus1Type = (int)eProperty.Focus_Summoning,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 12,
                    Bonus3Type = (int)eStat.PIE,

                    Bonus4 = 4,
                    Bonus4Type = (int)eProperty.PowerRegenerationRate
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(SpiritmasterEpicGloves);
				}

			}

			SpiritmasterEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("SpiritmasterEpicVest");
			if (SpiritmasterEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Spiritmaster Epic Vest , creating it ...");
                SpiritmasterEpicVest = new ItemTemplate
                {
                    Id_nb = "SpiritmasterEpicVest",
                    Name = "Spirit Touched Vest",
                    Level = 50,
                    Item_Type = 25,
                    Model = 799,
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
                    Bonus1Type = (int)eStat.DEX,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.PIE,

                    Bonus3 = 12,
                    Bonus3Type = (int)eResist.Slash,

                    Bonus4 = 24,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(SpiritmasterEpicVest);
				}

			}

			SpiritmasterEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("SpiritmasterEpicLegs");
			if (SpiritmasterEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Spiritmaster Epic Legs , creating it ...");
                SpiritmasterEpicLegs = new ItemTemplate
                {
                    Id_nb = "SpiritmasterEpicLegs",
                    Name = "Spirit Touched Pants",
                    Level = 50,
                    Item_Type = 27,
                    Model = 800,
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

                    Bonus1 = 13,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 12,
                    Bonus3Type = (int)eResist.Crush,

                    Bonus4 = 24,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(SpiritmasterEpicLegs);
				}

			}

			SpiritmasterEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("SpiritmasterEpicArms");
			if (SpiritmasterEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Spiritmaster Epic Arms , creating it ...");
                SpiritmasterEpicArms = new ItemTemplate
                {
                    Id_nb = "SpiritmasterEpicArms",
                    Name = "Spirit Touched Sleeves",
                    Level = 50,
                    Item_Type = 28,
                    Model = 801,
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
                    Bonus1Type = (int)eStat.PIE,

                    Bonus2 = 6,
                    Bonus2Type = (int)eResist.Thrust,

                    Bonus3 = 12,
                    Bonus3Type = (int)eProperty.MaxHealth,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Heat
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(SpiritmasterEpicArms);
				}
			}

			RunemasterEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("RunemasterEpicBoots");
			if (RunemasterEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Runemaster Epic Boots , creating it ...");
                RunemasterEpicBoots = new ItemTemplate
                {
                    Id_nb = "RunemasterEpicBoots",
                    Name = "Raven-Rune Boots",
                    Level = 50,
                    Item_Type = 23,
                    Model = 707,
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

                    Bonus2 = 16,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Matter,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Heat
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(RunemasterEpicBoots);
				}
			}
//end item
			RunemasterEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("RunemasterEpicHelm");
			if (RunemasterEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Runemaster Epic Helm , creating it ...");
                RunemasterEpicHelm = new ItemTemplate
                {
                    Id_nb = "RunemasterEpicHelm",
                    Name = "Raven-Rune Cap",
                    Level = 50,
                    Item_Type = 21,
                    Model = 825, //NEED TO WORK ON..
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
                    Bonus1Type = (int)eProperty.Focus_Darkness,

                    Bonus2 = 4,
                    Bonus2Type = (int)eProperty.Focus_Suppression,

                    Bonus3 = 13,
                    Bonus3Type = (int)eStat.PIE,

                    Bonus4 = 4,
                    Bonus4Type = (int)eProperty.PowerRegenerationRate
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(RunemasterEpicHelm);
				}
			}
//end item
			RunemasterEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("RunemasterEpicGloves");
			if (RunemasterEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Runemaster Epic Gloves , creating it ...");
                RunemasterEpicGloves = new ItemTemplate
                {
                    Id_nb = "RunemasterEpicGloves",
                    Name = "Raven-Rune Gloves ",
                    Level = 50,
                    Item_Type = 22,
                    Model = 706,
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
                    Bonus1Type = (int)eProperty.Focus_Summoning,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 12,
                    Bonus3Type = (int)eStat.PIE,

                    Bonus4 = 6,
                    Bonus4Type = (int)eProperty.PowerRegenerationRate
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(RunemasterEpicGloves);
				}
			}

			RunemasterEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("RunemasterEpicVest");
			if (RunemasterEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Runemaster Epic Vest , creating it ...");
                RunemasterEpicVest = new ItemTemplate
                {
                    Id_nb = "RunemasterEpicVest",
                    Name = "Raven-Rune Vest",
                    Level = 50,
                    Item_Type = 25,
                    Model = 703,
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
                    Bonus1Type = (int)eStat.DEX,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.PIE,

                    Bonus3 = 12,
                    Bonus3Type = (int)eResist.Slash,

                    Bonus4 = 24,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(RunemasterEpicVest);
				}
			}

			RunemasterEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("RunemasterEpicLegs");
			if (RunemasterEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Runemaster Epic Legs , creating it ...");
                RunemasterEpicLegs = new ItemTemplate
                {
                    Id_nb = "RunemasterEpicLegs",
                    Name = "Raven-Rune Pants",
                    Level = 50,
                    Item_Type = 27,
                    Model = 704,
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

                    Bonus1 = 13,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 12,
                    Bonus3Type = (int)eResist.Crush,

                    Bonus4 = 24,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(RunemasterEpicLegs);
				}
			}

			RunemasterEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("RunemasterEpicArms");
			if (RunemasterEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Runemaster Epic Arms , creating it ...");
                RunemasterEpicArms = new ItemTemplate
                {
                    Id_nb = "RunemasterEpicArms",
                    Name = "Raven-Rune Sleeves",
                    Level = 50,
                    Item_Type = 28,
                    Model = 705,
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
                    Bonus1Type = (int)eStat.PIE,

                    Bonus2 = 6,
                    Bonus2Type = (int)eResist.Thrust,

                    Bonus3 = 12,
                    Bonus3Type = (int)eProperty.MaxHealth,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Heat
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(RunemasterEpicArms);
				}
			}

			BonedancerEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("BonedancerEpicBoots");
			if (BonedancerEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bonedancer Epic Boots , creating it ...");
                BonedancerEpicBoots = new ItemTemplate
                {
                    Id_nb = "BonedancerEpicBoots",
                    Name = "Raven-Boned Boots",
                    Level = 50,
                    Item_Type = 23,
                    Model = 1190,
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

                    Bonus2 = 16,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 8,
                    Bonus3Type = (int)eResist.Matter,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Heat
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(BonedancerEpicBoots);
				}

			}
//end item
			BonedancerEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("BonedancerEpicHelm");
			if (BonedancerEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bonedancer Epic Helm , creating it ...");
                BonedancerEpicHelm = new ItemTemplate
                {
                    Id_nb = "BonedancerEpicHelm",
                    Name = "Raven-Boned Cap",
                    Level = 50,
                    Item_Type = 21,
                    Model = 825, //NEED TO WORK ON..
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
                    Bonus1Type = (int)eProperty.Focus_Suppression,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.PIE,

                    Bonus3 = 4,
                    Bonus3Type = (int)eProperty.PowerRegenerationRate,

                    Bonus4 = 4,
                    Bonus4Type = (int)eProperty.Focus_BoneArmy
                };


                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(BonedancerEpicHelm);
				}

			}
//end item
			BonedancerEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("BonedancerEpicGloves");
			if (BonedancerEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bonedancer Epic Gloves , creating it ...");
                BonedancerEpicGloves = new ItemTemplate
                {
                    Id_nb = "BonedancerEpicGloves",
                    Name = "Raven-Boned Gloves ",
                    Level = 50,
                    Item_Type = 22,
                    Model = 1191,
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
                    Bonus1Type = (int)eProperty.Focus_Darkness,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 12,
                    Bonus3Type = (int)eStat.PIE,

                    Bonus4 = 6,
                    Bonus4Type = (int)eProperty.PowerRegenerationRate
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(BonedancerEpicGloves);
				}
			}

			BonedancerEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("BonedancerEpicVest");
			if (BonedancerEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bonedancer Epic Vest , creating it ...");
                BonedancerEpicVest = new ItemTemplate
                {
                    Id_nb = "BonedancerEpicVest",
                    Name = "Raven-Boned Vest",
                    Level = 50,
                    Item_Type = 25,
                    Model = 1187,
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
                    Bonus1Type = (int)eStat.DEX,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.PIE,

                    Bonus3 = 12,
                    Bonus3Type = (int)eResist.Slash,

                    Bonus4 = 24,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(BonedancerEpicVest);
				}
			}

			BonedancerEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("BonedancerEpicLegs");
			if (BonedancerEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bonedancer Epic Legs , creating it ...");
                BonedancerEpicLegs = new ItemTemplate
                {
                    Id_nb = "BonedancerEpicLegs",
                    Name = "Raven-Boned Pants",
                    Level = 50,
                    Item_Type = 27,
                    Model = 1188,
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

                    Bonus1 = 13,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.DEX,

                    Bonus3 = 12,
                    Bonus3Type = (int)eResist.Crush,

                    Bonus4 = 24,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(BonedancerEpicLegs);
				}

			}

			BonedancerEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("BonedancerEpicArms");
			if (BonedancerEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bonedancer Epic Arms , creating it ...");
                BonedancerEpicArms = new ItemTemplate
                {
                    Id_nb = "BonedancerEpicArms",
                    Name = "Raven-Boned Sleeves",
                    Level = 50,
                    Item_Type = 28,
                    Model = 1189,
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
                    Bonus1Type = (int)eStat.PIE,

                    Bonus2 = 6,
                    Bonus2Type = (int)eResist.Thrust,

                    Bonus3 = 12,
                    Bonus3Type = (int)eProperty.MaxHealth,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Heat
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(BonedancerEpicArms);
				}

			}
			#region Warlock
			WarlockEpicBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("WarlockEpicBoots");
			if (WarlockEpicBoots == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warlock Epic Boots , creating it ...");
                WarlockEpicBoots = new ItemTemplate
                {
                    Id_nb = "WarlockEpicBoots",
                    Name = "Bewitched Soothsayer Boots",
                    Level = 50,
                    Item_Type = 23,
                    Model = 2937,
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
                     *   Constitution: 16 pts
                     *   Matter Resist: 8%
                     *   Hits: 48 pts
                     *   Heat Resist: 10%
                     */

                    Bonus1 = 16,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 8,
                    Bonus2Type = (int)eResist.Matter,

                    Bonus3 = 48,
                    Bonus3Type = (int)eProperty.MaxHealth,

                    Bonus4 = 10,
                    Bonus4Type = (int)eResist.Heat
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(WarlockEpicBoots);
				}

			}
			//end item
			WarlockEpicHelm = GameServer.Database.FindObjectByKey<ItemTemplate>("WarlockEpicHelm");
			if (WarlockEpicHelm == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warlock Epic Helm , creating it ...");
                WarlockEpicHelm = new ItemTemplate
                {
                    Id_nb = "WarlockEpicHelm",
                    Name = "Bewitched Soothsayer Cap",
                    Level = 50,
                    Item_Type = 21,
                    Model = 825, //NEED TO WORK ON..
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
                     *   Piety: 13 pts
                     *   Power: 4 pts
                     *   Cursing: +4 pts
                     *   Hexing: +4 pts
                     */

                    Bonus1 = 13,
                    Bonus1Type = (int)eStat.PIE,

                    Bonus2 = 4,
                    Bonus2Type = (int)eProperty.MaxMana,

                    Bonus3 = 4,
                    Bonus3Type = (int)eProperty.Skill_Cursing,

                    Bonus4 = 4,
                    Bonus4Type = (int)eProperty.Skill_Hexing
                };


                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(WarlockEpicHelm);
				}

			}
			//end item
			WarlockEpicGloves = GameServer.Database.FindObjectByKey<ItemTemplate>("WarlockEpicGloves");
			if (WarlockEpicGloves == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warlock Epic Gloves , creating it ...");
                WarlockEpicGloves = new ItemTemplate
                {
                    Id_nb = "WarlockEpicGloves",
                    Name = "Bewitched Soothsayer Gloves ",
                    Level = 50,
                    Item_Type = 22,
                    Model = 2936,
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
                     *   Constitution: 13 pts
                     *   Piety: 12 pts
                     *   Power: 4 pts
                     *   Hexing: +4 pts
                     */

                    Bonus1 = 13,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 12,
                    Bonus2Type = (int)eStat.PIE,

                    Bonus3 = 4,
                    Bonus3Type = (int)eProperty.MaxMana,

                    Bonus4 = 4,
                    Bonus4Type = (int)eProperty.Skill_Hexing
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(WarlockEpicGloves);
				}
			}

			WarlockEpicVest = GameServer.Database.FindObjectByKey<ItemTemplate>("WarlockEpicVest");
			if (WarlockEpicVest == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warlock Epic Vest , creating it ...");
                WarlockEpicVest = new ItemTemplate
                {
                    Id_nb = "WarlockEpicVest",
                    Name = "Bewitched Soothsayer Vest",
                    Level = 50,
                    Item_Type = 25,
                    Model = 2933,
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
                     *   Constitution: 12 pts
                     *   Piety: 13 pts
                     *   Slash Resist: 12%
                     *   Hits: 24 pts
                     */

                    Bonus1 = 12,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.PIE,

                    Bonus3 = 12,
                    Bonus3Type = (int)eResist.Slash,

                    Bonus4 = 24,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(WarlockEpicVest);
				}
			}

			WarlockEpicLegs = GameServer.Database.FindObjectByKey<ItemTemplate>("WarlockEpicLegs");
			if (WarlockEpicLegs == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warlock Epic Legs , creating it ...");
                WarlockEpicLegs = new ItemTemplate
                {
                    Id_nb = "WarlockEpicLegs",
                    Name = "Bewitched Soothsayer Pants",
                    Level = 50,
                    Item_Type = 27,
                    Model = 2934,
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
                     *   Constitution: 13 pts
                     *   Piety: 13 pts
                     *   Crush Resist: 12%
                     *   Hits: 24 pts
                     */

                    Bonus1 = 13,
                    Bonus1Type = (int)eStat.CON,

                    Bonus2 = 13,
                    Bonus2Type = (int)eStat.PIE,

                    Bonus3 = 12,
                    Bonus3Type = (int)eResist.Crush,

                    Bonus4 = 24,
                    Bonus4Type = (int)eProperty.MaxHealth
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(WarlockEpicLegs);
				}

			}

			WarlockEpicArms = GameServer.Database.FindObjectByKey<ItemTemplate>("WarlockEpicArms");
			if (WarlockEpicArms == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Warlock Epic Arms , creating it ...");
                WarlockEpicArms = new ItemTemplate
                {
                    Id_nb = "WarlockEpicArms",
                    Name = "Bewitched Soothsayer Sleeves",
                    Level = 50,
                    Item_Type = 28,
                    Model = 1189,
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
                     *   Piety: 9 pts
                     *   Thrust Resist: 6%
                     *   Power: 12 pts
                     *   Heat Resist: 8%
                     */

                    Bonus1 = 9,
                    Bonus1Type = (int)eStat.PIE,

                    Bonus2 = 6,
                    Bonus2Type = (int)eResist.Thrust,

                    Bonus3 = 12,
                    Bonus3Type = (int)eProperty.MaxMana,

                    Bonus4 = 8,
                    Bonus4Type = (int)eResist.Heat
                };

                if (SAVE_INTO_DATABASE)
				{
					GameServer.Database.AddObject(WarlockEpicArms);
				}

			}
			#endregion
			//Item Descriptions End

			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(Danica, GameObjectEvent.Interact, new DOLEventHandler(TalkToDanica));
			GameEventMgr.AddHandler(Danica, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToDanica));

			/* Now we bring to Danica the possibility to give this quest to players */
			Danica.AddQuestToGive(typeof (Mystic_50));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (Danica == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(Danica, GameObjectEvent.Interact, new DOLEventHandler(TalkToDanica));
			GameEventMgr.RemoveHandler(Danica, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToDanica));

			/* Now we remove to Danica the possibility to give this quest to players */
			Danica.RemoveQuestToGive(typeof (Mystic_50));
		}

		protected static void TalkToDanica(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(Danica.CanGiveQuest(typeof (Mystic_50), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			Mystic_50 quest = player.IsDoingQuest(typeof (Mystic_50)) as Mystic_50;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					switch (quest.Step)
					{
						case 1:
							Danica.SayTo(player, "Yes, you must face and defeat him! There is a note scrawled in the corner of the map that even in death Kelic is strong." +
								"He has gathered followers to protect him in his spirit state and they will come to his aid if he is attacked. Even though you have improved your skills quite a bit, " +
								"I would highley recommed taking some friends with you to face Kelic. It is imperative that you defeat him and obtain the totem he holds if I am to end the spell. " +
								"According to the map you can find Kelic in Raumarik. Head to the river in Raumarik and go north. When you reach the end of it, go northwest to the next river. " +
								"Cross the river and head west. Follow the snowline until you reach a group of trees. That is where you will find Kelic and his followers. " +
								"Return to me when you have the totem. May all the gods be with you.");
							break;
						case 2:
							Danica.SayTo(player, "It is good to see you were strong enough to survive Kelic. I can sense you have the controlling totem on you. Give me Kelic's totem now! Hurry!");
							quest.Step = 3;
							break;
						case 3:
							Danica.SayTo(player, "The curse is broken and the clan is safe. They are in your debt, but I think Arnfinn, has come up with a suitable reward for you. There are six parts to it, so make sure you have room for them. Just let me know when you are ready, and then you can [take them] with our thanks!");
							break;
					}
				}
				else
				{
					Danica.SayTo(player, "Ah, this reveals exactly where Jango and his deserters took Kelic to dispose of him. He also has a note here about how strong Kelic really was. That [worries me].");
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
							Danica.SayTo(player, "Yes, it worries me, but I think that you are ready to [face Kelic] and his minions.");
							break;
						case "face Kelic":
							player.Out.SendQuestSubscribeCommand(Danica, QuestMgr.GetIDForQuestType(typeof(Mystic_50)), "Will you face Kelic [Mystic Level 50 Epic]?");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "take them":
							if (quest.Step == 3)
								quest.FinishQuest();
							break;

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
			if (player.IsDoingQuest(typeof (Mystic_50)) != null)
				return true;

			if (player.CharacterClass.ID != (byte) eCharacterClass.Spiritmaster &&
				player.CharacterClass.ID != (byte) eCharacterClass.Runemaster &&
				player.CharacterClass.ID != (byte) eCharacterClass.Bonedancer &&
				player.CharacterClass.ID != (byte) eCharacterClass.Warlock)
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
			Mystic_50 quest = player.IsDoingQuest(typeof (Mystic_50)) as Mystic_50;

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

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(Mystic_50)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if(Danica.CanGiveQuest(typeof (Mystic_50), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (Mystic_50)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!Danica.GiveQuest(typeof (Mystic_50), player, 1))
					return;

				Danica.SayTo(player, "Yes, you must face and defeat him! There is a note scrawled in the corner of the map that even in death Kelic is strong." +
					"He has gathered followers to protect him in his spirit state and they will come to his aid if he is attacked. Even though you have improved your skills quite a bit, " +
					"I would highley recommed taking some friends with you to face Kelic. It is imperative that you defeat him and obtain the totem he holds if I am to end the spell. " +
					"According to the map you can find Kelic in Raumarik. Head to the river in Raumarik and go north. When you reach the end of it, go northwest to the next river. " +
					"Cross the river and head west. Follow the snowline until you reach a group of trees. That is where you will find Kelic and his followers. " +
					"Return to me when you have the totem. May all the gods be with you.");
			}
		}

		//Set quest name
		public override string Name
		{
			get { return "Saving the Clan (Level 50 Mystic Epic)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Find Kelic in Raumarik. Head to the river and go north. At the end go northwest to the next river, cross and head west. Follow the snowline until you reach a group of trees.";
					case 2:
						return "[Step #2] Return to Danica and give her the totem!";
					case 3:
						return "[Step #3] Tell Danica you can 'take them' for your rewards!";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (Mystic_50)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				//if (gArgs.Target.Name == Kelic.Name)
                    if (gArgs.Target.Name.IndexOf(Kelic.Name) >= 0)
                    {
					Step = 2;
					GiveItem(m_questPlayer, kelics_totem);
					m_questPlayer.Out.SendMessage("Kelic drops his Totem and you pick it up!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
			}

			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == Danica.Name && gArgs.Item.Id_nb == kelics_totem.Id_nb)
				{
					RemoveItem(Danica, player, kelics_totem);
					Danica.SayTo(player, "Ah, I can see how he wore the curse around the totem. I can now break the curse that is destroying the clan!");
					Danica.SayTo(player, "The curse is broken and the clan is safe. They are in your debt, but I think Arnfinn, has come up with a suitable reward for you. There are six parts to it, so make sure you have room for them. Just let me know when you are ready, and then you can [take them] with our thanks!");
					Step = 3;
				}
			}

		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, kelics_totem, false);
		}

		public override void FinishQuest()
		{
			if (m_questPlayer.Inventory.IsSlotsFree(6, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
			{
				base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

				switch ((eCharacterClass)m_questPlayer.CharacterClass.ID)
				{
					case eCharacterClass.Spiritmaster:
						{
							GiveItem(m_questPlayer, SpiritmasterEpicArms);
							GiveItem(m_questPlayer, SpiritmasterEpicBoots);
							GiveItem(m_questPlayer, SpiritmasterEpicGloves);
							GiveItem(m_questPlayer, SpiritmasterEpicHelm);
							GiveItem(m_questPlayer, SpiritmasterEpicLegs);
							GiveItem(m_questPlayer, SpiritmasterEpicVest);
							break;
						}
					case eCharacterClass.Runemaster:
						{
							GiveItem(m_questPlayer, RunemasterEpicArms);
							GiveItem(m_questPlayer, RunemasterEpicBoots);
							GiveItem(m_questPlayer, RunemasterEpicGloves);
							GiveItem(m_questPlayer, RunemasterEpicHelm);
							GiveItem(m_questPlayer, RunemasterEpicLegs);
							GiveItem(m_questPlayer, RunemasterEpicVest);
							break;
						}
					case eCharacterClass.Bonedancer:
						{
							GiveItem(m_questPlayer, BonedancerEpicArms);
							GiveItem(m_questPlayer, BonedancerEpicBoots);
							GiveItem(m_questPlayer, BonedancerEpicGloves);
							GiveItem(m_questPlayer, BonedancerEpicHelm);
							GiveItem(m_questPlayer, BonedancerEpicLegs);
							GiveItem(m_questPlayer, BonedancerEpicVest);
							break;
						}
					case eCharacterClass.Warlock:
						{
							GiveItem(m_questPlayer, WarlockEpicArms);
							GiveItem(m_questPlayer, WarlockEpicBoots);
							GiveItem(m_questPlayer, WarlockEpicGloves);
							GiveItem(m_questPlayer, WarlockEpicHelm);
							GiveItem(m_questPlayer, WarlockEpicLegs);
							GiveItem(m_questPlayer, WarlockEpicVest);
							break;
						}
				}
				Danica.SayTo(m_questPlayer, "May it serve you well, knowing that you have helped preserve the history of Midgard!");

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
Spirit Touched Boots 
Spirit Touched Cap 
Spirit Touched Gloves 
Spirit Touched Pants 
Spirit Touched Sleeves 
Spirit Touched Vest 
Raven-Rune Boots 
Raven-Rune Cap 
Raven-Rune Gloves 
Raven-Rune Pants 
Raven-Rune Sleeves 
Raven-Rune Vest 
Raven-boned Boots 
Raven-Boned Cap 
Raven-boned Gloves 
Raven-Boned Pants 
Raven-Boned Sleeves 
Bone-rune Vest 
        */

		#endregion
	}
}
