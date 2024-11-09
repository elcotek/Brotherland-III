	
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
* Author:	black - promise GM
* Date:		
*
* Notes:
*  
*/

using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using DOL.GS.Quests;
using DOL.GS.Behaviour;
using DOL.GS.Behaviour.Attributes;
using DOL.AI.Brain;

	namespace DOL.GS.Quests {
	
     /* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * BaseQuest	  	 
	 */
	public class DragonslayerPaladinQuest : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		///
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/* Declare the variables we need inside our quest.
		* You can declare static variables here, which will be available in
		* ALL instance of your quest and should be initialized ONLY ONCE inside
		* the OnScriptLoaded method.
		*
		* Or declare nonstatic variables here which can be unique for each Player
		* and change through the quest journey...
		*
		*/

		protected const string questTitle = "DragonslayerPaladinQuest";

		protected const int minimumLevel = 49;
		protected const int maximumLevel = 50;
	
	
		private static GameNPC Elia = null;
		
		private static GameNPC Runicaath = null;
		
		private static ItemTemplate Runicaath_Head = null;
		
		private static ItemTemplate Dragonslayer_Plate_Gauntlets = null;
		
		private static ItemTemplate Dragonslayer_Plate_Boots = null;
		
		private static ItemTemplate Dragonslayer_Plate_Greaves = null;
		
		private static ItemTemplate Dragonslayer_Plate_Vambraces = null;
		
		private static ItemTemplate Dragonslayer_Plate_Full_Helm = null;
		
		private static ItemTemplate Dragonslayer_Plate_Breastplate = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public DragonslayerPaladinQuest() : base()
		{
		}

		public DragonslayerPaladinQuest(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public DragonslayerPaladinQuest(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public DragonslayerPaladinQuest(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
	{
	}
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			QuestBehaviour a;
			QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerPaladinQuest));

			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(DragonslayerPaladinQuest)) == null)
				return;


			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;

				if (gArgs.Target.Name.IndexOf("Runicaath") >= 0)
				{
					
					GiveItem(gArgs.Target, player, Runicaath_Head);
					Step = 2;
					return;
				}
			}


			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs)args;

				if (gArgs.Target.Name == Elia.Name && gArgs.Item.Id_nb == Runicaath_Head.Id_nb)
				{

					if (player.Inventory.IsSlotsFree(6, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
					{

						a = builder.CreateBehaviour(Elia, -1);
						a.AddTrigger(eTriggerType.GiveItem, Elia, Runicaath_Head);
						a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerPaladinQuest), null);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Plate_Gauntlets, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Plate_Boots, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Plate_Greaves, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Plate_Vambraces, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Plate_Breastplate, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Plate_Full_Helm, Elia);
						a.AddAction(eActionType.FinishQuest, typeof(DragonslayerPaladinQuest), null);
						a.AddAction(eActionType.DestroyItem, Runicaath_Head, null);
						AddBehaviour(a);

					}
					else
						player.Out.SendMessage("You don't have enough inventory space to advance this quest.  You need " + 6 + " free slot(s)!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
			}
		}
		

		[ScriptLoadedEvent]
	public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
	{
	if (!ServerProperties.Properties.LOAD_QUESTS)
		return;
	if (log.IsInfoEnabled)
		log.Info("Quest \"" + questTitle + "\" initializing ...");

	#region defineNPCs
	GameNPC[] npcs;
	
			npcs = WorldMgr.GetNPCsByName("Elia",eRealm.Albion);
			if (npcs.Length == 0)
			{
				if (!WorldMgr.GetRegion(1).IsDisabled)
				{
				Elia = new DOL.GS.GameNPC();
					Elia.Model = 200;
				Elia.Name = "Elia";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Elia.Name + ", creating ...");
				Elia.GuildName = "Part of " + questTitle + " Quest";
				Elia.Realm = eRealm.Albion;
				Elia.CurrentRegionID = 1;
				Elia.Size = 50;
				Elia.Level = 65;
				Elia.MaxSpeedBase = 200;
				Elia.Faction = FactionMgr.GetFactionByID(0);
				Elia.X = 362113;
				Elia.Y = 715121;
				Elia.Z = 304;
				Elia.Heading = 2040;
				Elia.RespawnInterval = 0;
				Elia.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				Elia.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					Elia.SaveIntoDatabase();
					
				Elia.AddToWorld();
				
				}
			}
			else 
			{
				Elia = npcs[0];
			}
		
			npcs = WorldMgr.GetNPCsByName("Runicaath",eRealm.None);
			if (npcs.Length == 0)
			{
				if (!WorldMgr.GetRegion(1).IsDisabled)
				{
				Runicaath = new DOL.GS.GameNPC();
					Runicaath.Model = 2303;
				Runicaath.Name = "Runicaath";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Runicaath.Name + ", creating ...");
				Runicaath.GuildName = "Part of " + questTitle + " Quest";
				Runicaath.Realm = eRealm.None;
				Runicaath.CurrentRegionID = 1;
				Runicaath.Size = 230;
				Runicaath.Level = 65;
				Runicaath.MaxSpeedBase = 200;
				Runicaath.Faction = FactionMgr.GetFactionByID(0);
				Runicaath.X = 372155;
				Runicaath.Y = 735302;
				Runicaath.Z = 4302;
				Runicaath.Heading = 1608;
				Runicaath.RespawnInterval = 0;
				Runicaath.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 100;
				brain.AggroRange = 1000;
				Runicaath.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					Runicaath.SaveIntoDatabase();
					
				Runicaath.AddToWorld();
				
				}
			}
			else 
			{
				Runicaath = npcs[0];
			}
		

			#endregion

			#region defineItems

		Runicaath_Head = GameServer.Database.FindObjectByKey<ItemTemplate>("Runicaath_Head");
			if (Runicaath_Head == null)
			{
				Runicaath_Head = new ItemTemplate();
				Runicaath_Head.Name = "Runicaath Head";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Runicaath_Head.Name + ", creating it ...");
				Runicaath_Head.Level = 1;
				Runicaath_Head.Weight = 1;
				Runicaath_Head.Model = 503;
				Runicaath_Head.Object_Type = 0;
				Runicaath_Head.Item_Type = 0;
				Runicaath_Head.Id_nb = "Runicaath_Head";
				Runicaath_Head.Hand = 0;
				Runicaath_Head.IsPickable = true;
				Runicaath_Head.IsDropable = true;
				Runicaath_Head.IsTradable = true;
				Runicaath_Head.CanDropAsLoot = true;
				Runicaath_Head.Color = 0;
				Runicaath_Head.Bonus = 0; // default bonus				
				Runicaath_Head.Bonus1 = 0;
				Runicaath_Head.Bonus1Type = (int) 0;
				Runicaath_Head.Bonus2 = 0;
				Runicaath_Head.Bonus2Type = (int) 0;
				Runicaath_Head.Bonus3 = 0;
				Runicaath_Head.Bonus3Type = (int) 208;
				Runicaath_Head.Bonus4 = 0;
				Runicaath_Head.Bonus4Type = (int) 0;
				Runicaath_Head.Bonus5 = 0;
				Runicaath_Head.Bonus5Type = (int) 0;
				Runicaath_Head.Bonus6 = 0;
				Runicaath_Head.Bonus6Type = (int) 0;
				Runicaath_Head.Bonus7 = 0;
				Runicaath_Head.Bonus7Type = (int) 0;
				Runicaath_Head.Bonus8 = 0;
				Runicaath_Head.Bonus8Type = (int) 0;
				Runicaath_Head.Bonus9 = 0;
				Runicaath_Head.Bonus9Type = (int) 0;
				Runicaath_Head.Bonus10 = 0;
				Runicaath_Head.Bonus10Type = (int) 0;
				Runicaath_Head.ExtraBonus = 0;
				Runicaath_Head.ExtraBonusType = (int) 0;
				Runicaath_Head.Effect = 0;
				Runicaath_Head.Emblem = 0;
				Runicaath_Head.Charges = 0;
				Runicaath_Head.MaxCharges = 0;
				Runicaath_Head.SpellID = 0;
				Runicaath_Head.ProcSpellID = 0;
				Runicaath_Head.Type_Damage = 0;
				Runicaath_Head.Realm = 0;
				Runicaath_Head.MaxCount = 1;
				Runicaath_Head.PackSize = 1;
				Runicaath_Head.Extension = 0;
				Runicaath_Head.Quality = 100;				
				Runicaath_Head.Condition = 100;
				Runicaath_Head.MaxCondition = 100;
				Runicaath_Head.Durability = 100;
				Runicaath_Head.MaxDurability = 100;
				Runicaath_Head.PoisonCharges = 0;
				Runicaath_Head.PoisonMaxCharges = 0;
				Runicaath_Head.PoisonSpellID = 0;
				Runicaath_Head.ProcSpellID1 = 0;
				Runicaath_Head.SpellID1 = 0;
				Runicaath_Head.MaxCharges1 = 0;
				Runicaath_Head.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Runicaath_Head);
				}
			Dragonslayer_Plate_Gauntlets = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Plate_Gauntlets");
			if (Dragonslayer_Plate_Gauntlets == null)
			{
				Dragonslayer_Plate_Gauntlets = new ItemTemplate();
				Dragonslayer_Plate_Gauntlets.Name = "Dragonslayer Plate Gauntlets";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Plate_Gauntlets.Name + ", creating it ...");
				Dragonslayer_Plate_Gauntlets.Level = 51;
				Dragonslayer_Plate_Gauntlets.Weight = 40;
				Dragonslayer_Plate_Gauntlets.Model = 4003;
				Dragonslayer_Plate_Gauntlets.Object_Type = 36;
				Dragonslayer_Plate_Gauntlets.Item_Type = 22;
				Dragonslayer_Plate_Gauntlets.Id_nb = "Dragonslayer_Plate_Gauntlets";
				Dragonslayer_Plate_Gauntlets.Hand = 0;
				Dragonslayer_Plate_Gauntlets.IsPickable = true;
				Dragonslayer_Plate_Gauntlets.IsDropable = true;
				Dragonslayer_Plate_Gauntlets.IsTradable = true;
				Dragonslayer_Plate_Gauntlets.CanDropAsLoot = true;
				Dragonslayer_Plate_Gauntlets.Color = 0;
				Dragonslayer_Plate_Gauntlets.Bonus = 35; // default bonus				
				Dragonslayer_Plate_Gauntlets.Bonus1 = 5;
				Dragonslayer_Plate_Gauntlets.Bonus1Type = (int) 12;
				Dragonslayer_Plate_Gauntlets.Bonus2 = 5;
				Dragonslayer_Plate_Gauntlets.Bonus2Type = (int) 18;
				Dragonslayer_Plate_Gauntlets.Bonus3 = 45;
				Dragonslayer_Plate_Gauntlets.Bonus3Type = (int) 10;
				Dragonslayer_Plate_Gauntlets.Bonus4 = 5;
				Dragonslayer_Plate_Gauntlets.Bonus4Type = (int) 15;
				Dragonslayer_Plate_Gauntlets.Bonus5 = 3;
				Dragonslayer_Plate_Gauntlets.Bonus5Type = (int) 164;
				Dragonslayer_Plate_Gauntlets.Bonus6 = 45;
				Dragonslayer_Plate_Gauntlets.Bonus6Type = (int) 210;
				Dragonslayer_Plate_Gauntlets.Bonus7 = 10;
				Dragonslayer_Plate_Gauntlets.Bonus7Type = (int) 148;
				Dragonslayer_Plate_Gauntlets.Bonus8 = 0;
				Dragonslayer_Plate_Gauntlets.Bonus8Type = (int) 0;
				Dragonslayer_Plate_Gauntlets.Bonus9 = 0;
				Dragonslayer_Plate_Gauntlets.Bonus9Type = (int) 0;
				Dragonslayer_Plate_Gauntlets.Bonus10 = 0;
				Dragonslayer_Plate_Gauntlets.Bonus10Type = (int) 0;
				Dragonslayer_Plate_Gauntlets.ExtraBonus = 0;
				Dragonslayer_Plate_Gauntlets.ExtraBonusType = (int) 0;
				Dragonslayer_Plate_Gauntlets.Effect = 0;
				Dragonslayer_Plate_Gauntlets.Emblem = 0;
				Dragonslayer_Plate_Gauntlets.Charges = 0;
				Dragonslayer_Plate_Gauntlets.MaxCharges = 0;
				Dragonslayer_Plate_Gauntlets.SpellID = 0;
				Dragonslayer_Plate_Gauntlets.ProcSpellID = 31001;
				Dragonslayer_Plate_Gauntlets.Type_Damage = 0;
				Dragonslayer_Plate_Gauntlets.Realm = 0;
				Dragonslayer_Plate_Gauntlets.MaxCount = 1;
				Dragonslayer_Plate_Gauntlets.PackSize = 1;
				Dragonslayer_Plate_Gauntlets.Extension = 5;
				Dragonslayer_Plate_Gauntlets.Quality = 100;				
				Dragonslayer_Plate_Gauntlets.Condition = 50000;
				Dragonslayer_Plate_Gauntlets.MaxCondition = 50000;
				Dragonslayer_Plate_Gauntlets.Durability = 50000;
				Dragonslayer_Plate_Gauntlets.MaxDurability = 0;
				Dragonslayer_Plate_Gauntlets.PoisonCharges = 0;
				Dragonslayer_Plate_Gauntlets.PoisonMaxCharges = 0;
				Dragonslayer_Plate_Gauntlets.PoisonSpellID = 0;
				Dragonslayer_Plate_Gauntlets.ProcSpellID1 = 40006;
				Dragonslayer_Plate_Gauntlets.SpellID1 = 0;
				Dragonslayer_Plate_Gauntlets.MaxCharges1 = 0;
				Dragonslayer_Plate_Gauntlets.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Plate_Gauntlets);
				}
			Dragonslayer_Plate_Boots = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Plate_Boots");
			if (Dragonslayer_Plate_Boots == null)
			{
				Dragonslayer_Plate_Boots = new ItemTemplate();
				Dragonslayer_Plate_Boots.Name = "Dragonslayer Plate Boots";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Plate_Boots.Name + ", creating it ...");
				Dragonslayer_Plate_Boots.Level = 51;
				Dragonslayer_Plate_Boots.Weight = 40;
				Dragonslayer_Plate_Boots.Model = 4004;
				Dragonslayer_Plate_Boots.Object_Type = 36;
				Dragonslayer_Plate_Boots.Item_Type = 23;
				Dragonslayer_Plate_Boots.Id_nb = "Dragonslayer_Plate_Boots";
				Dragonslayer_Plate_Boots.Hand = 0;
				Dragonslayer_Plate_Boots.IsPickable = true;
				Dragonslayer_Plate_Boots.IsDropable = true;
				Dragonslayer_Plate_Boots.IsTradable = true;
				Dragonslayer_Plate_Boots.CanDropAsLoot = true;
				Dragonslayer_Plate_Boots.Color = 0;
				Dragonslayer_Plate_Boots.Bonus = 35; // default bonus				
				Dragonslayer_Plate_Boots.Bonus1 = 18;
				Dragonslayer_Plate_Boots.Bonus1Type = (int) 1;
				Dragonslayer_Plate_Boots.Bonus2 = 18;
				Dragonslayer_Plate_Boots.Bonus2Type = (int) 3;
				Dragonslayer_Plate_Boots.Bonus3 = 6;
				Dragonslayer_Plate_Boots.Bonus3Type = (int) 13;
				Dragonslayer_Plate_Boots.Bonus4 = 6;
				Dragonslayer_Plate_Boots.Bonus4Type = (int) 17;
				Dragonslayer_Plate_Boots.Bonus5 = 6;
				Dragonslayer_Plate_Boots.Bonus5Type = (int) 19;
				Dragonslayer_Plate_Boots.Bonus6 = 6;
				Dragonslayer_Plate_Boots.Bonus6Type = (int) 194;
				Dragonslayer_Plate_Boots.Bonus7 = 0;
				Dragonslayer_Plate_Boots.Bonus7Type = (int) 0;
				Dragonslayer_Plate_Boots.Bonus8 = 0;
				Dragonslayer_Plate_Boots.Bonus8Type = (int) 0;
				Dragonslayer_Plate_Boots.Bonus9 = 0;
				Dragonslayer_Plate_Boots.Bonus9Type = (int) 0;
				Dragonslayer_Plate_Boots.Bonus10 = 0;
				Dragonslayer_Plate_Boots.Bonus10Type = (int) 0;
				Dragonslayer_Plate_Boots.ExtraBonus = 0;
				Dragonslayer_Plate_Boots.ExtraBonusType = (int) 0;
				Dragonslayer_Plate_Boots.Effect = 0;
				Dragonslayer_Plate_Boots.Emblem = 0;
				Dragonslayer_Plate_Boots.Charges = 0;
				Dragonslayer_Plate_Boots.MaxCharges = 0;
				Dragonslayer_Plate_Boots.SpellID = 0;
				Dragonslayer_Plate_Boots.ProcSpellID = 31001;
				Dragonslayer_Plate_Boots.Type_Damage = 0;
				Dragonslayer_Plate_Boots.Realm = 0;
				Dragonslayer_Plate_Boots.MaxCount = 1;
				Dragonslayer_Plate_Boots.PackSize = 1;
				Dragonslayer_Plate_Boots.Extension = 5;
				Dragonslayer_Plate_Boots.Quality = 100;				
				Dragonslayer_Plate_Boots.Condition = 50000;
				Dragonslayer_Plate_Boots.MaxCondition = 50000;
				Dragonslayer_Plate_Boots.Durability = 50000;
				Dragonslayer_Plate_Boots.MaxDurability = 0;
				Dragonslayer_Plate_Boots.PoisonCharges = 0;
				Dragonslayer_Plate_Boots.PoisonMaxCharges = 0;
				Dragonslayer_Plate_Boots.PoisonSpellID = 0;
				Dragonslayer_Plate_Boots.ProcSpellID1 = 40006;
				Dragonslayer_Plate_Boots.SpellID1 = 0;
				Dragonslayer_Plate_Boots.MaxCharges1 = 0;
				Dragonslayer_Plate_Boots.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Plate_Boots);
				}
			Dragonslayer_Plate_Greaves = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Plate_Greaves");
			if (Dragonslayer_Plate_Greaves == null)
			{
				Dragonslayer_Plate_Greaves = new ItemTemplate();
				Dragonslayer_Plate_Greaves.Name = "Dragonslayer Plate Greaves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Plate_Greaves.Name + ", creating it ...");
				Dragonslayer_Plate_Greaves.Level = 51;
				Dragonslayer_Plate_Greaves.Weight = 70;
				Dragonslayer_Plate_Greaves.Model = 4001;
				Dragonslayer_Plate_Greaves.Object_Type = 36;
				Dragonslayer_Plate_Greaves.Item_Type = 27;
				Dragonslayer_Plate_Greaves.Id_nb = "Dragonslayer_Plate_Greaves";
				Dragonslayer_Plate_Greaves.Hand = 0;
				Dragonslayer_Plate_Greaves.IsPickable = true;
				Dragonslayer_Plate_Greaves.IsDropable = true;
				Dragonslayer_Plate_Greaves.IsTradable = true;
				Dragonslayer_Plate_Greaves.CanDropAsLoot = true;
				Dragonslayer_Plate_Greaves.Color = 0;
				Dragonslayer_Plate_Greaves.Bonus = 35; // default bonus				
				Dragonslayer_Plate_Greaves.Bonus1 = 22;
				Dragonslayer_Plate_Greaves.Bonus1Type = (int) 3;
				Dragonslayer_Plate_Greaves.Bonus2 = 5;
				Dragonslayer_Plate_Greaves.Bonus2Type = (int) 12;
				Dragonslayer_Plate_Greaves.Bonus3 = 5;
				Dragonslayer_Plate_Greaves.Bonus3Type = (int) 18;
				Dragonslayer_Plate_Greaves.Bonus4 = 40;
				Dragonslayer_Plate_Greaves.Bonus4Type = (int) 10;
				Dragonslayer_Plate_Greaves.Bonus5 = 5;
				Dragonslayer_Plate_Greaves.Bonus5Type = (int) 15;
				Dragonslayer_Plate_Greaves.Bonus6 = 5;
				Dragonslayer_Plate_Greaves.Bonus6Type = (int) 203;
				Dragonslayer_Plate_Greaves.Bonus7 = 1;
				Dragonslayer_Plate_Greaves.Bonus7Type = (int) 155;
				Dragonslayer_Plate_Greaves.Bonus8 = 0;
				Dragonslayer_Plate_Greaves.Bonus8Type = (int) 0;
				Dragonslayer_Plate_Greaves.Bonus9 = 0;
				Dragonslayer_Plate_Greaves.Bonus9Type = (int) 0;
				Dragonslayer_Plate_Greaves.Bonus10 = 0;
				Dragonslayer_Plate_Greaves.Bonus10Type = (int) 0;
				Dragonslayer_Plate_Greaves.ExtraBonus = 0;
				Dragonslayer_Plate_Greaves.ExtraBonusType = (int) 0;
				Dragonslayer_Plate_Greaves.Effect = 0;
				Dragonslayer_Plate_Greaves.Emblem = 0;
				Dragonslayer_Plate_Greaves.Charges = 0;
				Dragonslayer_Plate_Greaves.MaxCharges = 0;
				Dragonslayer_Plate_Greaves.SpellID = 0;
				Dragonslayer_Plate_Greaves.ProcSpellID = 31001;
				Dragonslayer_Plate_Greaves.Type_Damage = 0;
				Dragonslayer_Plate_Greaves.Realm = 0;
				Dragonslayer_Plate_Greaves.MaxCount = 1;
				Dragonslayer_Plate_Greaves.PackSize = 1;
				Dragonslayer_Plate_Greaves.Extension = 5;
				Dragonslayer_Plate_Greaves.Quality = 100;				
				Dragonslayer_Plate_Greaves.Condition = 50000;
				Dragonslayer_Plate_Greaves.MaxCondition = 50000;
				Dragonslayer_Plate_Greaves.Durability = 50000;
				Dragonslayer_Plate_Greaves.MaxDurability = 0;
				Dragonslayer_Plate_Greaves.PoisonCharges = 0;
				Dragonslayer_Plate_Greaves.PoisonMaxCharges = 0;
				Dragonslayer_Plate_Greaves.PoisonSpellID = 0;
				Dragonslayer_Plate_Greaves.ProcSpellID1 = 40006;
				Dragonslayer_Plate_Greaves.SpellID1 = 0;
				Dragonslayer_Plate_Greaves.MaxCharges1 = 0;
				Dragonslayer_Plate_Greaves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Plate_Greaves);
				}
			Dragonslayer_Plate_Vambraces = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Plate_Vambraces");
			if (Dragonslayer_Plate_Vambraces == null)
			{
				Dragonslayer_Plate_Vambraces = new ItemTemplate();
				Dragonslayer_Plate_Vambraces.Name = "Dragonslayer Plate Vambraces";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Plate_Vambraces.Name + ", creating it ...");
				Dragonslayer_Plate_Vambraces.Level = 51;
				Dragonslayer_Plate_Vambraces.Weight = 60;
				Dragonslayer_Plate_Vambraces.Model = 4002;
				Dragonslayer_Plate_Vambraces.Object_Type = 36;
				Dragonslayer_Plate_Vambraces.Item_Type = 28;
				Dragonslayer_Plate_Vambraces.Id_nb = "Dragonslayer_Plate_Vambraces";
				Dragonslayer_Plate_Vambraces.Hand = 0;
				Dragonslayer_Plate_Vambraces.IsPickable = true;
				Dragonslayer_Plate_Vambraces.IsDropable = true;
				Dragonslayer_Plate_Vambraces.IsTradable = true;
				Dragonslayer_Plate_Vambraces.CanDropAsLoot = true;
				Dragonslayer_Plate_Vambraces.Color = 0;
				Dragonslayer_Plate_Vambraces.Bonus = 35; // default bonus				
				Dragonslayer_Plate_Vambraces.Bonus1 = 22;
				Dragonslayer_Plate_Vambraces.Bonus1Type = (int) 1;
				Dragonslayer_Plate_Vambraces.Bonus2 = 40;
				Dragonslayer_Plate_Vambraces.Bonus2Type = (int) 10;
				Dragonslayer_Plate_Vambraces.Bonus3 = 5;
				Dragonslayer_Plate_Vambraces.Bonus3Type = (int) 12;
				Dragonslayer_Plate_Vambraces.Bonus4 = 5;
				Dragonslayer_Plate_Vambraces.Bonus4Type = (int) 15;
				Dragonslayer_Plate_Vambraces.Bonus5 = 5;
				Dragonslayer_Plate_Vambraces.Bonus5Type = (int) 18;
				Dragonslayer_Plate_Vambraces.Bonus6 = 1;
				Dragonslayer_Plate_Vambraces.Bonus6Type = (int) 155;
				Dragonslayer_Plate_Vambraces.Bonus7 = 5;
				Dragonslayer_Plate_Vambraces.Bonus7Type = (int) 201;
				Dragonslayer_Plate_Vambraces.Bonus8 = 0;
				Dragonslayer_Plate_Vambraces.Bonus8Type = (int) 0;
				Dragonslayer_Plate_Vambraces.Bonus9 = 0;
				Dragonslayer_Plate_Vambraces.Bonus9Type = (int) 0;
				Dragonslayer_Plate_Vambraces.Bonus10 = 0;
				Dragonslayer_Plate_Vambraces.Bonus10Type = (int) 0;
				Dragonslayer_Plate_Vambraces.ExtraBonus = 0;
				Dragonslayer_Plate_Vambraces.ExtraBonusType = (int) 0;
				Dragonslayer_Plate_Vambraces.Effect = 0;
				Dragonslayer_Plate_Vambraces.Emblem = 0;
				Dragonslayer_Plate_Vambraces.Charges = 0;
				Dragonslayer_Plate_Vambraces.MaxCharges = 0;
				Dragonslayer_Plate_Vambraces.SpellID = 0;
				Dragonslayer_Plate_Vambraces.ProcSpellID = 31001;
				Dragonslayer_Plate_Vambraces.Type_Damage = 0;
				Dragonslayer_Plate_Vambraces.Realm = 0;
				Dragonslayer_Plate_Vambraces.MaxCount = 1;
				Dragonslayer_Plate_Vambraces.PackSize = 1;
				Dragonslayer_Plate_Vambraces.Extension = 5;
				Dragonslayer_Plate_Vambraces.Quality = 100;				
				Dragonslayer_Plate_Vambraces.Condition = 50000;
				Dragonslayer_Plate_Vambraces.MaxCondition = 50000;
				Dragonslayer_Plate_Vambraces.Durability = 50000;
				Dragonslayer_Plate_Vambraces.MaxDurability = 0;
				Dragonslayer_Plate_Vambraces.PoisonCharges = 0;
				Dragonslayer_Plate_Vambraces.PoisonMaxCharges = 0;
				Dragonslayer_Plate_Vambraces.PoisonSpellID = 0;
				Dragonslayer_Plate_Vambraces.ProcSpellID1 = 40006;
				Dragonslayer_Plate_Vambraces.SpellID1 = 0;
				Dragonslayer_Plate_Vambraces.MaxCharges1 = 0;
				Dragonslayer_Plate_Vambraces.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Plate_Vambraces);
				}
			Dragonslayer_Plate_Full_Helm = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Plate_Full_Helm");
			if (Dragonslayer_Plate_Full_Helm == null)
			{
				Dragonslayer_Plate_Full_Helm = new ItemTemplate();
				Dragonslayer_Plate_Full_Helm.Name = "Dragonslayer Plate Full Helm";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Plate_Full_Helm.Name + ", creating it ...");
				Dragonslayer_Plate_Full_Helm.Level = 51;
				Dragonslayer_Plate_Full_Helm.Weight = 40;
				Dragonslayer_Plate_Full_Helm.Model = 4053;
				Dragonslayer_Plate_Full_Helm.Object_Type = 36;
				Dragonslayer_Plate_Full_Helm.Item_Type = 21;
				Dragonslayer_Plate_Full_Helm.Id_nb = "Dragonslayer_Plate_Full_Helm";
				Dragonslayer_Plate_Full_Helm.Hand = 0;
				Dragonslayer_Plate_Full_Helm.IsPickable = true;
				Dragonslayer_Plate_Full_Helm.IsDropable = true;
				Dragonslayer_Plate_Full_Helm.IsTradable = true;
				Dragonslayer_Plate_Full_Helm.CanDropAsLoot = true;
				Dragonslayer_Plate_Full_Helm.Color = 0;
				Dragonslayer_Plate_Full_Helm.Bonus = 35; // default bonus				
				Dragonslayer_Plate_Full_Helm.Bonus1 = 22;
				Dragonslayer_Plate_Full_Helm.Bonus1Type = (int) 3;
				Dragonslayer_Plate_Full_Helm.Bonus2 = 6;
				Dragonslayer_Plate_Full_Helm.Bonus2Type = (int) 16;
				Dragonslayer_Plate_Full_Helm.Bonus3 = 6;
				Dragonslayer_Plate_Full_Helm.Bonus3Type = (int) 11;
				Dragonslayer_Plate_Full_Helm.Bonus4 = 40;
				Dragonslayer_Plate_Full_Helm.Bonus4Type = (int) 10;
				Dragonslayer_Plate_Full_Helm.Bonus5 = 6;
				Dragonslayer_Plate_Full_Helm.Bonus5Type = (int) 14;
				Dragonslayer_Plate_Full_Helm.Bonus6 = 40;
				Dragonslayer_Plate_Full_Helm.Bonus6Type = (int) 210;
				Dragonslayer_Plate_Full_Helm.Bonus7 = 5;
				Dragonslayer_Plate_Full_Helm.Bonus7Type = (int) 148;
				Dragonslayer_Plate_Full_Helm.Bonus8 = 0;
				Dragonslayer_Plate_Full_Helm.Bonus8Type = (int) 0;
				Dragonslayer_Plate_Full_Helm.Bonus9 = 0;
				Dragonslayer_Plate_Full_Helm.Bonus9Type = (int) 0;
				Dragonslayer_Plate_Full_Helm.Bonus10 = 0;
				Dragonslayer_Plate_Full_Helm.Bonus10Type = (int) 0;
				Dragonslayer_Plate_Full_Helm.ExtraBonus = 0;
				Dragonslayer_Plate_Full_Helm.ExtraBonusType = (int) 0;
				Dragonslayer_Plate_Full_Helm.Effect = 0;
				Dragonslayer_Plate_Full_Helm.Emblem = 0;
				Dragonslayer_Plate_Full_Helm.Charges = 0;
				Dragonslayer_Plate_Full_Helm.MaxCharges = 0;
				Dragonslayer_Plate_Full_Helm.SpellID = 0;
				Dragonslayer_Plate_Full_Helm.ProcSpellID = 31001;
				Dragonslayer_Plate_Full_Helm.Type_Damage = 0;
				Dragonslayer_Plate_Full_Helm.Realm = 0;
				Dragonslayer_Plate_Full_Helm.MaxCount = 1;
				Dragonslayer_Plate_Full_Helm.PackSize = 1;
				Dragonslayer_Plate_Full_Helm.Extension = 0;
				Dragonslayer_Plate_Full_Helm.Quality = 100;				
				Dragonslayer_Plate_Full_Helm.Condition = 50000;
				Dragonslayer_Plate_Full_Helm.MaxCondition = 50000;
				Dragonslayer_Plate_Full_Helm.Durability = 50000;
				Dragonslayer_Plate_Full_Helm.MaxDurability = 0;
				Dragonslayer_Plate_Full_Helm.PoisonCharges = 0;
				Dragonslayer_Plate_Full_Helm.PoisonMaxCharges = 0;
				Dragonslayer_Plate_Full_Helm.PoisonSpellID = 0;
				Dragonslayer_Plate_Full_Helm.ProcSpellID1 = 40006;
				Dragonslayer_Plate_Full_Helm.SpellID1 = 0;
				Dragonslayer_Plate_Full_Helm.MaxCharges1 = 0;
				Dragonslayer_Plate_Full_Helm.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Plate_Full_Helm);
				}
			Dragonslayer_Plate_Breastplate = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Plate_Breastplate");
			if (Dragonslayer_Plate_Breastplate == null)
			{
				Dragonslayer_Plate_Breastplate = new ItemTemplate();
				Dragonslayer_Plate_Breastplate.Name = "Dragonslayer Plate Breastplate";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Plate_Breastplate.Name + ", creating it ...");
				Dragonslayer_Plate_Breastplate.Level = 51;
				Dragonslayer_Plate_Breastplate.Weight = 100;
				Dragonslayer_Plate_Breastplate.Model = 4000;
				Dragonslayer_Plate_Breastplate.Object_Type = 36;
				Dragonslayer_Plate_Breastplate.Item_Type = 25;
				Dragonslayer_Plate_Breastplate.Id_nb = "Dragonslayer_Plate_Breastplate";
				Dragonslayer_Plate_Breastplate.Hand = 0;
				Dragonslayer_Plate_Breastplate.IsPickable = true;
				Dragonslayer_Plate_Breastplate.IsDropable = true;
				Dragonslayer_Plate_Breastplate.IsTradable = true;
				Dragonslayer_Plate_Breastplate.CanDropAsLoot = true;
				Dragonslayer_Plate_Breastplate.Color = 0;
				Dragonslayer_Plate_Breastplate.Bonus = 35; // default bonus				
				Dragonslayer_Plate_Breastplate.Bonus1 = 22;
				Dragonslayer_Plate_Breastplate.Bonus1Type = (int) 4;
				Dragonslayer_Plate_Breastplate.Bonus2 = 22;
				Dragonslayer_Plate_Breastplate.Bonus2Type = (int) 1;
				Dragonslayer_Plate_Breastplate.Bonus3 = 5;
				Dragonslayer_Plate_Breastplate.Bonus3Type = (int) 12;
				Dragonslayer_Plate_Breastplate.Bonus4 = 5;
				Dragonslayer_Plate_Breastplate.Bonus4Type = (int) 15;
				Dragonslayer_Plate_Breastplate.Bonus5 = 5;
				Dragonslayer_Plate_Breastplate.Bonus5Type = (int) 18;
				Dragonslayer_Plate_Breastplate.Bonus6 = 4;
				Dragonslayer_Plate_Breastplate.Bonus6Type = (int) 200;
				Dragonslayer_Plate_Breastplate.Bonus7 = 4;
				Dragonslayer_Plate_Breastplate.Bonus7Type = (int) 173;
				Dragonslayer_Plate_Breastplate.Bonus8 = 0;
				Dragonslayer_Plate_Breastplate.Bonus8Type = (int) 0;
				Dragonslayer_Plate_Breastplate.Bonus9 = 0;
				Dragonslayer_Plate_Breastplate.Bonus9Type = (int) 0;
				Dragonslayer_Plate_Breastplate.Bonus10 = 0;
				Dragonslayer_Plate_Breastplate.Bonus10Type = (int) 0;
				Dragonslayer_Plate_Breastplate.ExtraBonus = 0;
				Dragonslayer_Plate_Breastplate.ExtraBonusType = (int) 0;
				Dragonslayer_Plate_Breastplate.Effect = 0;
				Dragonslayer_Plate_Breastplate.Emblem = 0;
				Dragonslayer_Plate_Breastplate.Charges = 0;
				Dragonslayer_Plate_Breastplate.MaxCharges = 0;
				Dragonslayer_Plate_Breastplate.SpellID = 0;
				Dragonslayer_Plate_Breastplate.ProcSpellID = 31001;
				Dragonslayer_Plate_Breastplate.Type_Damage = 0;
				Dragonslayer_Plate_Breastplate.Realm = 0;
				Dragonslayer_Plate_Breastplate.MaxCount = 1;
				Dragonslayer_Plate_Breastplate.PackSize = 1;
				Dragonslayer_Plate_Breastplate.Extension = 5;
				Dragonslayer_Plate_Breastplate.Quality = 100;				
				Dragonslayer_Plate_Breastplate.Condition = 50000;
				Dragonslayer_Plate_Breastplate.MaxCondition = 50000;
				Dragonslayer_Plate_Breastplate.Durability = 50000;
				Dragonslayer_Plate_Breastplate.MaxDurability = 0;
				Dragonslayer_Plate_Breastplate.PoisonCharges = 0;
				Dragonslayer_Plate_Breastplate.PoisonMaxCharges = 0;
				Dragonslayer_Plate_Breastplate.PoisonSpellID = 0;
				Dragonslayer_Plate_Breastplate.ProcSpellID1 = 40006;
				Dragonslayer_Plate_Breastplate.SpellID1 = 0;
				Dragonslayer_Plate_Breastplate.MaxCharges1 = 0;
				Dragonslayer_Plate_Breastplate.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Plate_Breastplate);
				}


            #endregion

            #region defineAreas

            #endregion

            #region defineQuestParts

            QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerPaladinQuest));
            QuestBehaviour a;
            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.Interact, null, Elia);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerPaladinQuest), Elia);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerPaladinQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "I have lived in this village since I was a young girl. My father was a great dragon hunter you know.", Elia);
            a.AddAction(eActionType.Talk, "In my years I have learned to work with all kinds of problems, specially with [Dragons].", Elia);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.Whisper, "Dragons", Elia);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerPaladinQuest), Elia);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerPaladinQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "yes dragons, but there is a problem for me, i can't hunt dragons by self, [you know].", Elia);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.Whisper, "you know", Elia);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerPaladinQuest), Elia);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerPaladinQuest), null, (eComparator)5);
            a.AddAction(eActionType.OfferQuest, typeof(DragonslayerPaladinQuest), "Helen has offered you the Dragonslayer quest.?Do you accept?");
            AddBehaviour(a);

            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.DeclineQuest, null, typeof(DragonslayerPaladinQuest));
            a.AddAction(eActionType.Talk, "No problem. See you", Elia);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.AcceptQuest, null, typeof(DragonslayerPaladinQuest));
            a.AddAction(eActionType.Talk, "If you bring me the head of Runicaath I shall pay you well!", Elia);
            a.AddAction(eActionType.GiveQuest, typeof(DragonslayerPaladinQuest), Elia);
            AddBehaviour(a);

			          

			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End
			if (Elia!=null) {
				Elia.AddQuestToGive(typeof (DragonslayerPaladinQuest));
			}
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{			
				
			// Custom Scriptunloaded Code Begin
			
			// Custom Scriptunloaded Code End

			

			/* If Elia has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (Elia == null)
				return;
			/* Now we remove the possibility to give this quest to players */			
			Elia.RemoveQuestToGive(typeof (DragonslayerPaladinQuest));
		}

		/* Now we set the quest name.
		* If we don't override the base method, then the quest
		* will have the name "UNDEFINED QUEST NAME" and we don't
		* want that, do we? ;-)
		*/

		public override string Name
		{
			get { return questTitle; }
		}

		/* Now we set the quest step descriptions.
		* If we don't override the base method, then the quest
		* description for ALL steps will be "UNDEFINDED QUEST DESCRIPTION"
		* and this isn't something nice either ;-)
		*/
		public override string Description
		{
			get
			{
			switch (Step)
			{
				
					case 1:
						return "[Step #1] Kill the Minidragon Runicaath and prove your strenght.When you got it come back to me with his HEAD";
					case 2:
						return "[Step #2] Bring the head of Runicaath to Elia";
					default:
						return " No Queststep Description available.";
				}				
			}
		}
		
		/// <summary>
		/// This method checks if a player is qualified for this quest
		/// </summary>
		/// <returns>true if qualified, false if not</returns>
		public override bool CheckQuestQualification(GamePlayer player)
		{		
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (DragonslayerPaladinQuest)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
			if (
		
			player.CharacterClass.ID != (byte) eCharacterClass.Paladin && 
				true) {
				return false;			
			}
		
			return true;
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}
	}
}
