	
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
	public class DragonslayerHereticQuest : BaseQuest
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

		protected const string questTitle = "DragonslayerHereticQuest";

		protected const int minimumLevel = 49;
		protected const int maximumLevel = 50;
	
	
		private static GameNPC Elia = null;
		
		private static GameNPC Runicaath = null;
		
		private static ItemTemplate Runicaath_Head = null;
		
		private static ItemTemplate Dragonslayer_Infernal_Cloth_Vest = null;
		
		private static ItemTemplate Dragonslayer_Infernal_Cloth_Breeches = null;
		
		private static ItemTemplate Dragonslayer_Infernal_Cloth_Gloves = null;
		
		private static ItemTemplate Dragonslayer_Cloth_Full_Helm = null;
		
		private static ItemTemplate Dragonslayer_Cloth_Slippers = null;
		
		private static ItemTemplate Dragonslayer_Cloth_Sleeves = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public DragonslayerHereticQuest() : base()
		{
		}

		public DragonslayerHereticQuest(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public DragonslayerHereticQuest(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public DragonslayerHereticQuest(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
	{
	}

		
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			QuestBehaviour a;
			QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerHereticQuest));

			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(DragonslayerHereticQuest)) == null)
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
						a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerHereticQuest), null);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Infernal_Cloth_Vest, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Infernal_Cloth_Breeches, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Infernal_Cloth_Gloves, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Cloth_Full_Helm, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Cloth_Slippers, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Cloth_Sleeves, Elia);
						a.AddAction(eActionType.FinishQuest, typeof(DragonslayerHereticQuest), null);
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
			Dragonslayer_Infernal_Cloth_Vest = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Infernal_Cloth_Vest");
			if (Dragonslayer_Infernal_Cloth_Vest == null)
			{
				Dragonslayer_Infernal_Cloth_Vest = new ItemTemplate();
				Dragonslayer_Infernal_Cloth_Vest.Name = "Dragonslayer Infernal Cloth Vest";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Infernal_Cloth_Vest.Name + ", creating it ...");
				Dragonslayer_Infernal_Cloth_Vest.Level = 51;
				Dragonslayer_Infernal_Cloth_Vest.Weight = 20;
				Dragonslayer_Infernal_Cloth_Vest.Model = 4020;
				Dragonslayer_Infernal_Cloth_Vest.Object_Type = 32;
				Dragonslayer_Infernal_Cloth_Vest.Item_Type = 25;
				Dragonslayer_Infernal_Cloth_Vest.Id_nb = "Dragonslayer_Infernal_Cloth_Vest";
				Dragonslayer_Infernal_Cloth_Vest.Hand = 0;
				Dragonslayer_Infernal_Cloth_Vest.IsPickable = true;
				Dragonslayer_Infernal_Cloth_Vest.IsDropable = true;
				Dragonslayer_Infernal_Cloth_Vest.IsTradable = true;
				Dragonslayer_Infernal_Cloth_Vest.CanDropAsLoot = true;
				Dragonslayer_Infernal_Cloth_Vest.Color = 0;
				Dragonslayer_Infernal_Cloth_Vest.Bonus = 35; // default bonus				
				Dragonslayer_Infernal_Cloth_Vest.Bonus1 = 15;
				Dragonslayer_Infernal_Cloth_Vest.Bonus1Type = (int) 1;
				Dragonslayer_Infernal_Cloth_Vest.Bonus2 = 15;
				Dragonslayer_Infernal_Cloth_Vest.Bonus2Type = (int) 2;
				Dragonslayer_Infernal_Cloth_Vest.Bonus3 = 5;
				Dragonslayer_Infernal_Cloth_Vest.Bonus3Type = (int) 12;
				Dragonslayer_Infernal_Cloth_Vest.Bonus4 = 5;
				Dragonslayer_Infernal_Cloth_Vest.Bonus4Type = (int) 18;
				Dragonslayer_Infernal_Cloth_Vest.Bonus5 = 5;
				Dragonslayer_Infernal_Cloth_Vest.Bonus5Type = (int) 15;
				Dragonslayer_Infernal_Cloth_Vest.Bonus6 = 4;
				Dragonslayer_Infernal_Cloth_Vest.Bonus6Type = (int) 200;
				Dragonslayer_Infernal_Cloth_Vest.Bonus7 = 4;
				Dragonslayer_Infernal_Cloth_Vest.Bonus7Type = (int) 198;
				Dragonslayer_Infernal_Cloth_Vest.Bonus8 = 4;
				Dragonslayer_Infernal_Cloth_Vest.Bonus8Type = (int) 153;
				Dragonslayer_Infernal_Cloth_Vest.Bonus9 = 4;
				Dragonslayer_Infernal_Cloth_Vest.Bonus9Type = (int) 173;
				Dragonslayer_Infernal_Cloth_Vest.Bonus10 = 0;
				Dragonslayer_Infernal_Cloth_Vest.Bonus10Type = (int) 0;
				Dragonslayer_Infernal_Cloth_Vest.ExtraBonus = 0;
				Dragonslayer_Infernal_Cloth_Vest.ExtraBonusType = (int) 0;
				Dragonslayer_Infernal_Cloth_Vest.Effect = 0;
				Dragonslayer_Infernal_Cloth_Vest.Emblem = 0;
				Dragonslayer_Infernal_Cloth_Vest.Charges = 0;
				Dragonslayer_Infernal_Cloth_Vest.MaxCharges = 0;
				Dragonslayer_Infernal_Cloth_Vest.SpellID = 0;
				Dragonslayer_Infernal_Cloth_Vest.ProcSpellID = 31001;
				Dragonslayer_Infernal_Cloth_Vest.Type_Damage = 0;
				Dragonslayer_Infernal_Cloth_Vest.Realm = 0;
				Dragonslayer_Infernal_Cloth_Vest.MaxCount = 1;
				Dragonslayer_Infernal_Cloth_Vest.PackSize = 1;
				Dragonslayer_Infernal_Cloth_Vest.Extension = 5;
				Dragonslayer_Infernal_Cloth_Vest.Quality = 100;				
				Dragonslayer_Infernal_Cloth_Vest.Condition = 50000;
				Dragonslayer_Infernal_Cloth_Vest.MaxCondition = 50000;
				Dragonslayer_Infernal_Cloth_Vest.Durability = 50000;
				Dragonslayer_Infernal_Cloth_Vest.MaxDurability = 0;
				Dragonslayer_Infernal_Cloth_Vest.PoisonCharges = 0;
				Dragonslayer_Infernal_Cloth_Vest.PoisonMaxCharges = 0;
				Dragonslayer_Infernal_Cloth_Vest.PoisonSpellID = 0;
				Dragonslayer_Infernal_Cloth_Vest.ProcSpellID1 = 40006;
				Dragonslayer_Infernal_Cloth_Vest.SpellID1 = 0;
				Dragonslayer_Infernal_Cloth_Vest.MaxCharges1 = 0;
				Dragonslayer_Infernal_Cloth_Vest.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Infernal_Cloth_Vest);
				}
			Dragonslayer_Infernal_Cloth_Breeches = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Infernal_Cloth_Breeches");
			if (Dragonslayer_Infernal_Cloth_Breeches == null)
			{
				Dragonslayer_Infernal_Cloth_Breeches = new ItemTemplate();
				Dragonslayer_Infernal_Cloth_Breeches.Name = "Dragonslayer Infernal Cloth Breeches";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Infernal_Cloth_Breeches.Name + ", creating it ...");
				Dragonslayer_Infernal_Cloth_Breeches.Level = 51;
				Dragonslayer_Infernal_Cloth_Breeches.Weight = 14;
				Dragonslayer_Infernal_Cloth_Breeches.Model = 4016;
				Dragonslayer_Infernal_Cloth_Breeches.Object_Type = 32;
				Dragonslayer_Infernal_Cloth_Breeches.Item_Type = 27;
				Dragonslayer_Infernal_Cloth_Breeches.Id_nb = "Dragonslayer_Infernal_Cloth_Breeches";
				Dragonslayer_Infernal_Cloth_Breeches.Hand = 0;
				Dragonslayer_Infernal_Cloth_Breeches.IsPickable = true;
				Dragonslayer_Infernal_Cloth_Breeches.IsDropable = true;
				Dragonslayer_Infernal_Cloth_Breeches.IsTradable = true;
				Dragonslayer_Infernal_Cloth_Breeches.CanDropAsLoot = true;
				Dragonslayer_Infernal_Cloth_Breeches.Color = 0;
				Dragonslayer_Infernal_Cloth_Breeches.Bonus = 35; // default bonus				
				Dragonslayer_Infernal_Cloth_Breeches.Bonus1 = 22;
				Dragonslayer_Infernal_Cloth_Breeches.Bonus1Type = (int) 2;
				Dragonslayer_Infernal_Cloth_Breeches.Bonus2 = 40;
				Dragonslayer_Infernal_Cloth_Breeches.Bonus2Type = (int) 10;
				Dragonslayer_Infernal_Cloth_Breeches.Bonus3 = 5;
				Dragonslayer_Infernal_Cloth_Breeches.Bonus3Type = (int) 12;
				Dragonslayer_Infernal_Cloth_Breeches.Bonus4 = 5;
				Dragonslayer_Infernal_Cloth_Breeches.Bonus4Type = (int) 15;
				Dragonslayer_Infernal_Cloth_Breeches.Bonus5 = 5;
				Dragonslayer_Infernal_Cloth_Breeches.Bonus5Type = (int) 18;
				Dragonslayer_Infernal_Cloth_Breeches.Bonus6 = 1;
				Dragonslayer_Infernal_Cloth_Breeches.Bonus6Type = (int) 155;
				Dragonslayer_Infernal_Cloth_Breeches.Bonus7 = 5;
				Dragonslayer_Infernal_Cloth_Breeches.Bonus7Type = (int) 202;
				Dragonslayer_Infernal_Cloth_Breeches.Bonus8 = 0;
				Dragonslayer_Infernal_Cloth_Breeches.Bonus8Type = (int) 0;
				Dragonslayer_Infernal_Cloth_Breeches.Bonus9 = 0;
				Dragonslayer_Infernal_Cloth_Breeches.Bonus9Type = (int) 0;
				Dragonslayer_Infernal_Cloth_Breeches.Bonus10 = 0;
				Dragonslayer_Infernal_Cloth_Breeches.Bonus10Type = (int) 0;
				Dragonslayer_Infernal_Cloth_Breeches.ExtraBonus = 0;
				Dragonslayer_Infernal_Cloth_Breeches.ExtraBonusType = (int) 0;
				Dragonslayer_Infernal_Cloth_Breeches.Effect = 0;
				Dragonslayer_Infernal_Cloth_Breeches.Emblem = 0;
				Dragonslayer_Infernal_Cloth_Breeches.Charges = 0;
				Dragonslayer_Infernal_Cloth_Breeches.MaxCharges = 0;
				Dragonslayer_Infernal_Cloth_Breeches.SpellID = 0;
				Dragonslayer_Infernal_Cloth_Breeches.ProcSpellID = 31001;
				Dragonslayer_Infernal_Cloth_Breeches.Type_Damage = 0;
				Dragonslayer_Infernal_Cloth_Breeches.Realm = 0;
				Dragonslayer_Infernal_Cloth_Breeches.MaxCount = 1;
				Dragonslayer_Infernal_Cloth_Breeches.PackSize = 1;
				Dragonslayer_Infernal_Cloth_Breeches.Extension = 5;
				Dragonslayer_Infernal_Cloth_Breeches.Quality = 100;				
				Dragonslayer_Infernal_Cloth_Breeches.Condition = 50000;
				Dragonslayer_Infernal_Cloth_Breeches.MaxCondition = 50000;
				Dragonslayer_Infernal_Cloth_Breeches.Durability = 50000;
				Dragonslayer_Infernal_Cloth_Breeches.MaxDurability = 0;
				Dragonslayer_Infernal_Cloth_Breeches.PoisonCharges = 0;
				Dragonslayer_Infernal_Cloth_Breeches.PoisonMaxCharges = 0;
				Dragonslayer_Infernal_Cloth_Breeches.PoisonSpellID = 0;
				Dragonslayer_Infernal_Cloth_Breeches.ProcSpellID1 = 40006;
				Dragonslayer_Infernal_Cloth_Breeches.SpellID1 = 0;
				Dragonslayer_Infernal_Cloth_Breeches.MaxCharges1 = 0;
				Dragonslayer_Infernal_Cloth_Breeches.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Infernal_Cloth_Breeches);
				}
			Dragonslayer_Infernal_Cloth_Gloves = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Infernal_Cloth_Gloves");
			if (Dragonslayer_Infernal_Cloth_Gloves == null)
			{
				Dragonslayer_Infernal_Cloth_Gloves = new ItemTemplate();
				Dragonslayer_Infernal_Cloth_Gloves.Name = "Dragonslayer Infernal Cloth Gloves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Infernal_Cloth_Gloves.Name + ", creating it ...");
				Dragonslayer_Infernal_Cloth_Gloves.Level = 51;
				Dragonslayer_Infernal_Cloth_Gloves.Weight = 8;
				Dragonslayer_Infernal_Cloth_Gloves.Model = 4018;
				Dragonslayer_Infernal_Cloth_Gloves.Object_Type = 32;
				Dragonslayer_Infernal_Cloth_Gloves.Item_Type = 22;
				Dragonslayer_Infernal_Cloth_Gloves.Id_nb = "Dragonslayer_Infernal_Cloth_Gloves";
				Dragonslayer_Infernal_Cloth_Gloves.Hand = 0;
				Dragonslayer_Infernal_Cloth_Gloves.IsPickable = true;
				Dragonslayer_Infernal_Cloth_Gloves.IsDropable = true;
				Dragonslayer_Infernal_Cloth_Gloves.IsTradable = true;
				Dragonslayer_Infernal_Cloth_Gloves.CanDropAsLoot = true;
				Dragonslayer_Infernal_Cloth_Gloves.Color = 0;
				Dragonslayer_Infernal_Cloth_Gloves.Bonus = 35; // default bonus				
				Dragonslayer_Infernal_Cloth_Gloves.Bonus1 = 22;
				Dragonslayer_Infernal_Cloth_Gloves.Bonus1Type = (int) 1;
				Dragonslayer_Infernal_Cloth_Gloves.Bonus2 = 5;
				Dragonslayer_Infernal_Cloth_Gloves.Bonus2Type = (int) 12;
				Dragonslayer_Infernal_Cloth_Gloves.Bonus3 = 5;
				Dragonslayer_Infernal_Cloth_Gloves.Bonus3Type = (int) 18;
				Dragonslayer_Infernal_Cloth_Gloves.Bonus4 = 45;
				Dragonslayer_Infernal_Cloth_Gloves.Bonus4Type = (int) 10;
				Dragonslayer_Infernal_Cloth_Gloves.Bonus5 = 5;
				Dragonslayer_Infernal_Cloth_Gloves.Bonus5Type = (int) 15;
				Dragonslayer_Infernal_Cloth_Gloves.Bonus6 = 1;
				Dragonslayer_Infernal_Cloth_Gloves.Bonus6Type = (int) 155;
				Dragonslayer_Infernal_Cloth_Gloves.Bonus7 = 5;
				Dragonslayer_Infernal_Cloth_Gloves.Bonus7Type = (int) 201;
				Dragonslayer_Infernal_Cloth_Gloves.Bonus8 = 0;
				Dragonslayer_Infernal_Cloth_Gloves.Bonus8Type = (int) 0;
				Dragonslayer_Infernal_Cloth_Gloves.Bonus9 = 0;
				Dragonslayer_Infernal_Cloth_Gloves.Bonus9Type = (int) 0;
				Dragonslayer_Infernal_Cloth_Gloves.Bonus10 = 0;
				Dragonslayer_Infernal_Cloth_Gloves.Bonus10Type = (int) 0;
				Dragonslayer_Infernal_Cloth_Gloves.ExtraBonus = 0;
				Dragonslayer_Infernal_Cloth_Gloves.ExtraBonusType = (int) 0;
				Dragonslayer_Infernal_Cloth_Gloves.Effect = 0;
				Dragonslayer_Infernal_Cloth_Gloves.Emblem = 0;
				Dragonslayer_Infernal_Cloth_Gloves.Charges = 0;
				Dragonslayer_Infernal_Cloth_Gloves.MaxCharges = 0;
				Dragonslayer_Infernal_Cloth_Gloves.SpellID = 0;
				Dragonslayer_Infernal_Cloth_Gloves.ProcSpellID = 31001;
				Dragonslayer_Infernal_Cloth_Gloves.Type_Damage = 0;
				Dragonslayer_Infernal_Cloth_Gloves.Realm = 0;
				Dragonslayer_Infernal_Cloth_Gloves.MaxCount = 1;
				Dragonslayer_Infernal_Cloth_Gloves.PackSize = 1;
				Dragonslayer_Infernal_Cloth_Gloves.Extension = 5;
				Dragonslayer_Infernal_Cloth_Gloves.Quality = 100;				
				Dragonslayer_Infernal_Cloth_Gloves.Condition = 50000;
				Dragonslayer_Infernal_Cloth_Gloves.MaxCondition = 50000;
				Dragonslayer_Infernal_Cloth_Gloves.Durability = 50000;
				Dragonslayer_Infernal_Cloth_Gloves.MaxDurability = 0;
				Dragonslayer_Infernal_Cloth_Gloves.PoisonCharges = 0;
				Dragonslayer_Infernal_Cloth_Gloves.PoisonMaxCharges = 0;
				Dragonslayer_Infernal_Cloth_Gloves.PoisonSpellID = 0;
				Dragonslayer_Infernal_Cloth_Gloves.ProcSpellID1 = 40006;
				Dragonslayer_Infernal_Cloth_Gloves.SpellID1 = 0;
				Dragonslayer_Infernal_Cloth_Gloves.MaxCharges1 = 0;
				Dragonslayer_Infernal_Cloth_Gloves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Infernal_Cloth_Gloves);
				}
			Dragonslayer_Cloth_Full_Helm = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Cloth_Full_Helm");
			if (Dragonslayer_Cloth_Full_Helm == null)
			{
				Dragonslayer_Cloth_Full_Helm = new ItemTemplate();
				Dragonslayer_Cloth_Full_Helm.Name = "Dragonslayer Cloth Full Helm";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Cloth_Full_Helm.Name + ", creating it ...");
				Dragonslayer_Cloth_Full_Helm.Level = 51;
				Dragonslayer_Cloth_Full_Helm.Weight = 8;
				Dragonslayer_Cloth_Full_Helm.Model = 4056;
				Dragonslayer_Cloth_Full_Helm.Object_Type = 32;
				Dragonslayer_Cloth_Full_Helm.Item_Type = 21;
				Dragonslayer_Cloth_Full_Helm.Id_nb = "Dragonslayer_Cloth_Full_Helm";
				Dragonslayer_Cloth_Full_Helm.Hand = 0;
				Dragonslayer_Cloth_Full_Helm.IsPickable = true;
				Dragonslayer_Cloth_Full_Helm.IsDropable = true;
				Dragonslayer_Cloth_Full_Helm.IsTradable = true;
				Dragonslayer_Cloth_Full_Helm.CanDropAsLoot = true;
				Dragonslayer_Cloth_Full_Helm.Color = 0;
				Dragonslayer_Cloth_Full_Helm.Bonus = 35; // default bonus				
				Dragonslayer_Cloth_Full_Helm.Bonus1 = 22;
				Dragonslayer_Cloth_Full_Helm.Bonus1Type = (int) 3;
				Dragonslayer_Cloth_Full_Helm.Bonus2 = 6;
				Dragonslayer_Cloth_Full_Helm.Bonus2Type = (int) 16;
				Dragonslayer_Cloth_Full_Helm.Bonus3 = 6;
				Dragonslayer_Cloth_Full_Helm.Bonus3Type = (int) 11;
				Dragonslayer_Cloth_Full_Helm.Bonus4 = 40;
				Dragonslayer_Cloth_Full_Helm.Bonus4Type = (int) 10;
				Dragonslayer_Cloth_Full_Helm.Bonus5 = 6;
				Dragonslayer_Cloth_Full_Helm.Bonus5Type = (int) 14;
				Dragonslayer_Cloth_Full_Helm.Bonus6 = 40;
				Dragonslayer_Cloth_Full_Helm.Bonus6Type = (int) 210;
				Dragonslayer_Cloth_Full_Helm.Bonus7 = 5;
				Dragonslayer_Cloth_Full_Helm.Bonus7Type = (int) 148;
				Dragonslayer_Cloth_Full_Helm.Bonus8 = 0;
				Dragonslayer_Cloth_Full_Helm.Bonus8Type = (int) 0;
				Dragonslayer_Cloth_Full_Helm.Bonus9 = 0;
				Dragonslayer_Cloth_Full_Helm.Bonus9Type = (int) 0;
				Dragonslayer_Cloth_Full_Helm.Bonus10 = 0;
				Dragonslayer_Cloth_Full_Helm.Bonus10Type = (int) 0;
				Dragonslayer_Cloth_Full_Helm.ExtraBonus = 0;
				Dragonslayer_Cloth_Full_Helm.ExtraBonusType = (int) 0;
				Dragonslayer_Cloth_Full_Helm.Effect = 0;
				Dragonslayer_Cloth_Full_Helm.Emblem = 0;
				Dragonslayer_Cloth_Full_Helm.Charges = 0;
				Dragonslayer_Cloth_Full_Helm.MaxCharges = 0;
				Dragonslayer_Cloth_Full_Helm.SpellID = 0;
				Dragonslayer_Cloth_Full_Helm.ProcSpellID = 31001;
				Dragonslayer_Cloth_Full_Helm.Type_Damage = 0;
				Dragonslayer_Cloth_Full_Helm.Realm = 0;
				Dragonslayer_Cloth_Full_Helm.MaxCount = 1;
				Dragonslayer_Cloth_Full_Helm.PackSize = 1;
				Dragonslayer_Cloth_Full_Helm.Extension = 0;
				Dragonslayer_Cloth_Full_Helm.Quality = 100;				
				Dragonslayer_Cloth_Full_Helm.Condition = 50000;
				Dragonslayer_Cloth_Full_Helm.MaxCondition = 50000;
				Dragonslayer_Cloth_Full_Helm.Durability = 50000;
				Dragonslayer_Cloth_Full_Helm.MaxDurability = 0;
				Dragonslayer_Cloth_Full_Helm.PoisonCharges = 0;
				Dragonslayer_Cloth_Full_Helm.PoisonMaxCharges = 0;
				Dragonslayer_Cloth_Full_Helm.PoisonSpellID = 0;
				Dragonslayer_Cloth_Full_Helm.ProcSpellID1 = 40006;
				Dragonslayer_Cloth_Full_Helm.SpellID1 = 0;
				Dragonslayer_Cloth_Full_Helm.MaxCharges1 = 0;
				Dragonslayer_Cloth_Full_Helm.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Cloth_Full_Helm);
				}
			Dragonslayer_Cloth_Slippers = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Cloth_Slippers");
			if (Dragonslayer_Cloth_Slippers == null)
			{
				Dragonslayer_Cloth_Slippers = new ItemTemplate();
				Dragonslayer_Cloth_Slippers.Name = "Dragonslayer Cloth Slippers";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Cloth_Slippers.Name + ", creating it ...");
				Dragonslayer_Cloth_Slippers.Level = 51;
				Dragonslayer_Cloth_Slippers.Weight = 8;
				Dragonslayer_Cloth_Slippers.Model = 4019;
				Dragonslayer_Cloth_Slippers.Object_Type = 32;
				Dragonslayer_Cloth_Slippers.Item_Type = 23;
				Dragonslayer_Cloth_Slippers.Id_nb = "Dragonslayer_Cloth_Slippers";
				Dragonslayer_Cloth_Slippers.Hand = 0;
				Dragonslayer_Cloth_Slippers.IsPickable = true;
				Dragonslayer_Cloth_Slippers.IsDropable = true;
				Dragonslayer_Cloth_Slippers.IsTradable = true;
				Dragonslayer_Cloth_Slippers.CanDropAsLoot = true;
				Dragonslayer_Cloth_Slippers.Color = 0;
				Dragonslayer_Cloth_Slippers.Bonus = 35; // default bonus				
				Dragonslayer_Cloth_Slippers.Bonus1 = 18;
				Dragonslayer_Cloth_Slippers.Bonus1Type = (int) 2;
				Dragonslayer_Cloth_Slippers.Bonus2 = 6;
				Dragonslayer_Cloth_Slippers.Bonus2Type = (int) 13;
				Dragonslayer_Cloth_Slippers.Bonus3 = 6;
				Dragonslayer_Cloth_Slippers.Bonus3Type = (int) 17;
				Dragonslayer_Cloth_Slippers.Bonus4 = 6;
				Dragonslayer_Cloth_Slippers.Bonus4Type = (int) 19;
				Dragonslayer_Cloth_Slippers.Bonus5 = 18;
				Dragonslayer_Cloth_Slippers.Bonus5Type = (int) 156;
				Dragonslayer_Cloth_Slippers.Bonus6 = 6;
				Dragonslayer_Cloth_Slippers.Bonus6Type = (int) 196;
				Dragonslayer_Cloth_Slippers.Bonus7 = 0;
				Dragonslayer_Cloth_Slippers.Bonus7Type = (int) 0;
				Dragonslayer_Cloth_Slippers.Bonus8 = 0;
				Dragonslayer_Cloth_Slippers.Bonus8Type = (int) 0;
				Dragonslayer_Cloth_Slippers.Bonus9 = 0;
				Dragonslayer_Cloth_Slippers.Bonus9Type = (int) 0;
				Dragonslayer_Cloth_Slippers.Bonus10 = 0;
				Dragonslayer_Cloth_Slippers.Bonus10Type = (int) 0;
				Dragonslayer_Cloth_Slippers.ExtraBonus = 0;
				Dragonslayer_Cloth_Slippers.ExtraBonusType = (int) 0;
				Dragonslayer_Cloth_Slippers.Effect = 0;
				Dragonslayer_Cloth_Slippers.Emblem = 0;
				Dragonslayer_Cloth_Slippers.Charges = 0;
				Dragonslayer_Cloth_Slippers.MaxCharges = 0;
				Dragonslayer_Cloth_Slippers.SpellID = 0;
				Dragonslayer_Cloth_Slippers.ProcSpellID = 31001;
				Dragonslayer_Cloth_Slippers.Type_Damage = 0;
				Dragonslayer_Cloth_Slippers.Realm = 0;
				Dragonslayer_Cloth_Slippers.MaxCount = 1;
				Dragonslayer_Cloth_Slippers.PackSize = 1;
				Dragonslayer_Cloth_Slippers.Extension = 5;
				Dragonslayer_Cloth_Slippers.Quality = 100;				
				Dragonslayer_Cloth_Slippers.Condition = 50000;
				Dragonslayer_Cloth_Slippers.MaxCondition = 50000;
				Dragonslayer_Cloth_Slippers.Durability = 50000;
				Dragonslayer_Cloth_Slippers.MaxDurability = 0;
				Dragonslayer_Cloth_Slippers.PoisonCharges = 0;
				Dragonslayer_Cloth_Slippers.PoisonMaxCharges = 0;
				Dragonslayer_Cloth_Slippers.PoisonSpellID = 0;
				Dragonslayer_Cloth_Slippers.ProcSpellID1 = 40006;
				Dragonslayer_Cloth_Slippers.SpellID1 = 0;
				Dragonslayer_Cloth_Slippers.MaxCharges1 = 0;
				Dragonslayer_Cloth_Slippers.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Cloth_Slippers);
				}
			Dragonslayer_Cloth_Sleeves = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Cloth_Sleeves");
			if (Dragonslayer_Cloth_Sleeves == null)
			{
				Dragonslayer_Cloth_Sleeves = new ItemTemplate();
				Dragonslayer_Cloth_Sleeves.Name = "Dragonslayer Cloth Sleeves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Cloth_Sleeves.Name + ", creating it ...");
				Dragonslayer_Cloth_Sleeves.Level = 51;
				Dragonslayer_Cloth_Sleeves.Weight = 12;
				Dragonslayer_Cloth_Sleeves.Model = 4017;
				Dragonslayer_Cloth_Sleeves.Object_Type = 32;
				Dragonslayer_Cloth_Sleeves.Item_Type = 28;
				Dragonslayer_Cloth_Sleeves.Id_nb = "Dragonslayer_Cloth_Sleeves";
				Dragonslayer_Cloth_Sleeves.Hand = 0;
				Dragonslayer_Cloth_Sleeves.IsPickable = true;
				Dragonslayer_Cloth_Sleeves.IsDropable = true;
				Dragonslayer_Cloth_Sleeves.IsTradable = true;
				Dragonslayer_Cloth_Sleeves.CanDropAsLoot = true;
				Dragonslayer_Cloth_Sleeves.Color = 0;
				Dragonslayer_Cloth_Sleeves.Bonus = 35; // default bonus				
				Dragonslayer_Cloth_Sleeves.Bonus1 = 5;
				Dragonslayer_Cloth_Sleeves.Bonus1Type = (int) 12;
				Dragonslayer_Cloth_Sleeves.Bonus2 = 5;
				Dragonslayer_Cloth_Sleeves.Bonus2Type = (int) 18;
				Dragonslayer_Cloth_Sleeves.Bonus3 = 40;
				Dragonslayer_Cloth_Sleeves.Bonus3Type = (int) 10;
				Dragonslayer_Cloth_Sleeves.Bonus4 = 5;
				Dragonslayer_Cloth_Sleeves.Bonus4Type = (int) 15;
				Dragonslayer_Cloth_Sleeves.Bonus5 = 22;
				Dragonslayer_Cloth_Sleeves.Bonus5Type = (int) 156;
				Dragonslayer_Cloth_Sleeves.Bonus6 = 1;
				Dragonslayer_Cloth_Sleeves.Bonus6Type = (int) 191;
				Dragonslayer_Cloth_Sleeves.Bonus7 = 5;
				Dragonslayer_Cloth_Sleeves.Bonus7Type = (int) 209;
				Dragonslayer_Cloth_Sleeves.Bonus8 = 0;
				Dragonslayer_Cloth_Sleeves.Bonus8Type = (int) 0;
				Dragonslayer_Cloth_Sleeves.Bonus9 = 0;
				Dragonslayer_Cloth_Sleeves.Bonus9Type = (int) 0;
				Dragonslayer_Cloth_Sleeves.Bonus10 = 0;
				Dragonslayer_Cloth_Sleeves.Bonus10Type = (int) 0;
				Dragonslayer_Cloth_Sleeves.ExtraBonus = 0;
				Dragonslayer_Cloth_Sleeves.ExtraBonusType = (int) 0;
				Dragonslayer_Cloth_Sleeves.Effect = 0;
				Dragonslayer_Cloth_Sleeves.Emblem = 0;
				Dragonslayer_Cloth_Sleeves.Charges = 0;
				Dragonslayer_Cloth_Sleeves.MaxCharges = 0;
				Dragonslayer_Cloth_Sleeves.SpellID = 0;
				Dragonslayer_Cloth_Sleeves.ProcSpellID = 31001;
				Dragonslayer_Cloth_Sleeves.Type_Damage = 0;
				Dragonslayer_Cloth_Sleeves.Realm = 0;
				Dragonslayer_Cloth_Sleeves.MaxCount = 1;
				Dragonslayer_Cloth_Sleeves.PackSize = 1;
				Dragonslayer_Cloth_Sleeves.Extension = 5;
				Dragonslayer_Cloth_Sleeves.Quality = 100;				
				Dragonslayer_Cloth_Sleeves.Condition = 50000;
				Dragonslayer_Cloth_Sleeves.MaxCondition = 50000;
				Dragonslayer_Cloth_Sleeves.Durability = 50000;
				Dragonslayer_Cloth_Sleeves.MaxDurability = 0;
				Dragonslayer_Cloth_Sleeves.PoisonCharges = 0;
				Dragonslayer_Cloth_Sleeves.PoisonMaxCharges = 0;
				Dragonslayer_Cloth_Sleeves.PoisonSpellID = 0;
				Dragonslayer_Cloth_Sleeves.ProcSpellID1 = 40006;
				Dragonslayer_Cloth_Sleeves.SpellID1 = 0;
				Dragonslayer_Cloth_Sleeves.MaxCharges1 = 0;
				Dragonslayer_Cloth_Sleeves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Cloth_Sleeves);
				}


            #endregion

            #region defineAreas

            #endregion

            #region defineQuestParts

            QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerHereticQuest));
            QuestBehaviour a;
            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.Interact, null, Elia);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerHereticQuest), Elia);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerHereticQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "I have lived in this village since I was a young girl. My father was a great dragon hunter you know.", Elia);
            a.AddAction(eActionType.Talk, "In my years I have learned to work with all kinds of problems, specially with [Dragons].", Elia);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.Whisper, "Dragons", Elia);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerHereticQuest), Elia);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerHereticQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "yes dragons, but there is a problem for me, i can't hunt dragons by self, [you know].", Elia);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.Whisper, "you know", Elia);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerHereticQuest), Elia);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerHereticQuest), null, (eComparator)5);
            a.AddAction(eActionType.OfferQuest, typeof(DragonslayerHereticQuest), "Helen has offered you the Dragonslayer quest.?Do you accept?");
            AddBehaviour(a);

            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.DeclineQuest, null, typeof(DragonslayerHereticQuest));
            a.AddAction(eActionType.Talk, "No problem. See you", Elia);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.AcceptQuest, null, typeof(DragonslayerHereticQuest));
            a.AddAction(eActionType.Talk, "If you bring me the head of Runicaath I shall pay you well!", Elia);
            a.AddAction(eActionType.GiveQuest, typeof(DragonslayerHereticQuest), Elia);
            AddBehaviour(a);

			         			
			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End
			if (Elia!=null) {
				Elia.AddQuestToGive(typeof (DragonslayerHereticQuest));
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
			Elia.RemoveQuestToGive(typeof (DragonslayerHereticQuest));
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
			if (player.IsDoingQuest(typeof (DragonslayerHereticQuest)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
			if (
		
			player.CharacterClass.ID != (byte) eCharacterClass.Heretic && 
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
