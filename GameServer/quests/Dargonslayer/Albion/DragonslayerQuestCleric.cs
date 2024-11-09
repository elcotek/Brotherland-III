	
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
	public class DragonslayerClericQuest : BaseQuest
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

		protected const string questTitle = "DragonslayerClericQuest";

		protected const int minimumLevel = 49;
		protected const int maximumLevel = 50;
	
	
		private static GameNPC Elia = null;
		
		private static GameNPC Runicaath = null;
		
		private static ItemTemplate Runicaath_Head = null;
		
		private static ItemTemplate Dragonslayer_Holy_Mail_Hauberk = null;
		
		private static ItemTemplate Dragonslayer_Mail_Full_Helm = null;
		
		private static ItemTemplate Dragonslayer_Holy_Mail_Gauntlets = null;
		
		private static ItemTemplate Dragonslayer_Holy_Mail_Armguards = null;
		
		private static ItemTemplate Dragonslayer_Holy_Mail_Chausses = null;
		
		private static ItemTemplate Dragonslayer_Holy_Mail_Boots = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public DragonslayerClericQuest() : base()
		{
		}

		public DragonslayerClericQuest(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public DragonslayerClericQuest(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public DragonslayerClericQuest(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
	{
	}
		
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			QuestBehaviour a;
			QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerClericQuest));

			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(DragonslayerClericQuest)) == null)
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
						a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerClericQuest), null);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Holy_Mail_Hauberk, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Mail_Full_Helm, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Holy_Mail_Gauntlets, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Holy_Mail_Armguards, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Holy_Mail_Chausses, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Holy_Mail_Boots, Elia);
						a.AddAction(eActionType.FinishQuest, typeof(DragonslayerClericQuest), null);
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
			Dragonslayer_Holy_Mail_Hauberk = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Holy_Mail_Hauberk");
			if (Dragonslayer_Holy_Mail_Hauberk == null)
			{
				Dragonslayer_Holy_Mail_Hauberk = new ItemTemplate();
				Dragonslayer_Holy_Mail_Hauberk.Name = "Dragonslayer Holy Mail Hauberk";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Holy_Mail_Hauberk.Name + ", creating it ...");
				Dragonslayer_Holy_Mail_Hauberk.Level = 51;
				Dragonslayer_Holy_Mail_Hauberk.Weight = 80;
				Dragonslayer_Holy_Mail_Hauberk.Model = 3995;
				Dragonslayer_Holy_Mail_Hauberk.Object_Type = 35;
				Dragonslayer_Holy_Mail_Hauberk.Item_Type = 25;
				Dragonslayer_Holy_Mail_Hauberk.Id_nb = "Dragonslayer_Holy_Mail_Hauberk";
				Dragonslayer_Holy_Mail_Hauberk.Hand = 0;
				Dragonslayer_Holy_Mail_Hauberk.IsPickable = true;
				Dragonslayer_Holy_Mail_Hauberk.IsDropable = true;
				Dragonslayer_Holy_Mail_Hauberk.IsTradable = true;
				Dragonslayer_Holy_Mail_Hauberk.CanDropAsLoot = true;
				Dragonslayer_Holy_Mail_Hauberk.Color = 0;
				Dragonslayer_Holy_Mail_Hauberk.Bonus = 35; // default bonus				
				Dragonslayer_Holy_Mail_Hauberk.Bonus1 = 22;
				Dragonslayer_Holy_Mail_Hauberk.Bonus1Type = (int) 2;
				Dragonslayer_Holy_Mail_Hauberk.Bonus2 = 5;
				Dragonslayer_Holy_Mail_Hauberk.Bonus2Type = (int) 12;
				Dragonslayer_Holy_Mail_Hauberk.Bonus3 = 5;
				Dragonslayer_Holy_Mail_Hauberk.Bonus3Type = (int) 18;
				Dragonslayer_Holy_Mail_Hauberk.Bonus4 = 5;
				Dragonslayer_Holy_Mail_Hauberk.Bonus4Type = (int) 15;
				Dragonslayer_Holy_Mail_Hauberk.Bonus5 = 22;
				Dragonslayer_Holy_Mail_Hauberk.Bonus5Type = (int) 156;
				Dragonslayer_Holy_Mail_Hauberk.Bonus6 = 4;
				Dragonslayer_Holy_Mail_Hauberk.Bonus6Type = (int) 153;
				Dragonslayer_Holy_Mail_Hauberk.Bonus7 = 4;
				Dragonslayer_Holy_Mail_Hauberk.Bonus7Type = (int) 191;
				Dragonslayer_Holy_Mail_Hauberk.Bonus8 = 0;
				Dragonslayer_Holy_Mail_Hauberk.Bonus8Type = (int) 0;
				Dragonslayer_Holy_Mail_Hauberk.Bonus9 = 0;
				Dragonslayer_Holy_Mail_Hauberk.Bonus9Type = (int) 0;
				Dragonslayer_Holy_Mail_Hauberk.Bonus10 = 0;
				Dragonslayer_Holy_Mail_Hauberk.Bonus10Type = (int) 0;
				Dragonslayer_Holy_Mail_Hauberk.ExtraBonus = 0;
				Dragonslayer_Holy_Mail_Hauberk.ExtraBonusType = (int) 0;
				Dragonslayer_Holy_Mail_Hauberk.Effect = 0;
				Dragonslayer_Holy_Mail_Hauberk.Emblem = 0;
				Dragonslayer_Holy_Mail_Hauberk.Charges = 0;
				Dragonslayer_Holy_Mail_Hauberk.MaxCharges = 0;
				Dragonslayer_Holy_Mail_Hauberk.SpellID = 0;
				Dragonslayer_Holy_Mail_Hauberk.ProcSpellID = 31001;
				Dragonslayer_Holy_Mail_Hauberk.Type_Damage = 0;
				Dragonslayer_Holy_Mail_Hauberk.Realm = 0;
				Dragonslayer_Holy_Mail_Hauberk.MaxCount = 1;
				Dragonslayer_Holy_Mail_Hauberk.PackSize = 1;
				Dragonslayer_Holy_Mail_Hauberk.Extension = 5;
				Dragonslayer_Holy_Mail_Hauberk.Quality = 100;				
				Dragonslayer_Holy_Mail_Hauberk.Condition = 50000;
				Dragonslayer_Holy_Mail_Hauberk.MaxCondition = 50000;
				Dragonslayer_Holy_Mail_Hauberk.Durability = 50000;
				Dragonslayer_Holy_Mail_Hauberk.MaxDurability = 0;
				Dragonslayer_Holy_Mail_Hauberk.PoisonCharges = 0;
				Dragonslayer_Holy_Mail_Hauberk.PoisonMaxCharges = 0;
				Dragonslayer_Holy_Mail_Hauberk.PoisonSpellID = 0;
				Dragonslayer_Holy_Mail_Hauberk.ProcSpellID1 = 40006;
				Dragonslayer_Holy_Mail_Hauberk.SpellID1 = 0;
				Dragonslayer_Holy_Mail_Hauberk.MaxCharges1 = 0;
				Dragonslayer_Holy_Mail_Hauberk.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Holy_Mail_Hauberk);
				}
			Dragonslayer_Mail_Full_Helm = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Mail_Full_Helm");
			if (Dragonslayer_Mail_Full_Helm == null)
			{
				Dragonslayer_Mail_Full_Helm = new ItemTemplate();
				Dragonslayer_Mail_Full_Helm.Name = "Dragonslayer Mail Full Helm";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Mail_Full_Helm.Name + ", creating it ...");
				Dragonslayer_Mail_Full_Helm.Level = 51;
				Dragonslayer_Mail_Full_Helm.Weight = 8;
				Dragonslayer_Mail_Full_Helm.Model = 4057;
				Dragonslayer_Mail_Full_Helm.Object_Type = 35;
				Dragonslayer_Mail_Full_Helm.Item_Type = 21;
				Dragonslayer_Mail_Full_Helm.Id_nb = "Dragonslayer_Mail_Full_Helm";
				Dragonslayer_Mail_Full_Helm.Hand = 0;
				Dragonslayer_Mail_Full_Helm.IsPickable = true;
				Dragonslayer_Mail_Full_Helm.IsDropable = true;
				Dragonslayer_Mail_Full_Helm.IsTradable = true;
				Dragonslayer_Mail_Full_Helm.CanDropAsLoot = true;
				Dragonslayer_Mail_Full_Helm.Color = 0;
				Dragonslayer_Mail_Full_Helm.Bonus = 35; // default bonus				
				Dragonslayer_Mail_Full_Helm.Bonus1 = 22;
				Dragonslayer_Mail_Full_Helm.Bonus1Type = (int) 3;
				Dragonslayer_Mail_Full_Helm.Bonus2 = 6;
				Dragonslayer_Mail_Full_Helm.Bonus2Type = (int) 16;
				Dragonslayer_Mail_Full_Helm.Bonus3 = 6;
				Dragonslayer_Mail_Full_Helm.Bonus3Type = (int) 11;
				Dragonslayer_Mail_Full_Helm.Bonus4 = 40;
				Dragonslayer_Mail_Full_Helm.Bonus4Type = (int) 10;
				Dragonslayer_Mail_Full_Helm.Bonus5 = 6;
				Dragonslayer_Mail_Full_Helm.Bonus5Type = (int) 14;
				Dragonslayer_Mail_Full_Helm.Bonus6 = 40;
				Dragonslayer_Mail_Full_Helm.Bonus6Type = (int) 210;
				Dragonslayer_Mail_Full_Helm.Bonus7 = 5;
				Dragonslayer_Mail_Full_Helm.Bonus7Type = (int) 148;
				Dragonslayer_Mail_Full_Helm.Bonus8 = 0;
				Dragonslayer_Mail_Full_Helm.Bonus8Type = (int) 0;
				Dragonslayer_Mail_Full_Helm.Bonus9 = 0;
				Dragonslayer_Mail_Full_Helm.Bonus9Type = (int) 0;
				Dragonslayer_Mail_Full_Helm.Bonus10 = 0;
				Dragonslayer_Mail_Full_Helm.Bonus10Type = (int) 0;
				Dragonslayer_Mail_Full_Helm.ExtraBonus = 0;
				Dragonslayer_Mail_Full_Helm.ExtraBonusType = (int) 0;
				Dragonslayer_Mail_Full_Helm.Effect = 0;
				Dragonslayer_Mail_Full_Helm.Emblem = 0;
				Dragonslayer_Mail_Full_Helm.Charges = 0;
				Dragonslayer_Mail_Full_Helm.MaxCharges = 0;
				Dragonslayer_Mail_Full_Helm.SpellID = 0;
				Dragonslayer_Mail_Full_Helm.ProcSpellID = 31001;
				Dragonslayer_Mail_Full_Helm.Type_Damage = 0;
				Dragonslayer_Mail_Full_Helm.Realm = 0;
				Dragonslayer_Mail_Full_Helm.MaxCount = 1;
				Dragonslayer_Mail_Full_Helm.PackSize = 1;
				Dragonslayer_Mail_Full_Helm.Extension = 0;
				Dragonslayer_Mail_Full_Helm.Quality = 100;				
				Dragonslayer_Mail_Full_Helm.Condition = 50000;
				Dragonslayer_Mail_Full_Helm.MaxCondition = 50000;
				Dragonslayer_Mail_Full_Helm.Durability = 50000;
				Dragonslayer_Mail_Full_Helm.MaxDurability = 0;
				Dragonslayer_Mail_Full_Helm.PoisonCharges = 0;
				Dragonslayer_Mail_Full_Helm.PoisonMaxCharges = 0;
				Dragonslayer_Mail_Full_Helm.PoisonSpellID = 0;
				Dragonslayer_Mail_Full_Helm.ProcSpellID1 = 40006;
				Dragonslayer_Mail_Full_Helm.SpellID1 = 0;
				Dragonslayer_Mail_Full_Helm.MaxCharges1 = 0;
				Dragonslayer_Mail_Full_Helm.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Mail_Full_Helm);
				}
			Dragonslayer_Holy_Mail_Gauntlets = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Holy_Mail_Gauntlets");
			if (Dragonslayer_Holy_Mail_Gauntlets == null)
			{
				Dragonslayer_Holy_Mail_Gauntlets = new ItemTemplate();
				Dragonslayer_Holy_Mail_Gauntlets.Name = "Dragonslayer Holy Mail Gauntlets";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Holy_Mail_Gauntlets.Name + ", creating it ...");
				Dragonslayer_Holy_Mail_Gauntlets.Level = 51;
				Dragonslayer_Holy_Mail_Gauntlets.Weight = 32;
				Dragonslayer_Holy_Mail_Gauntlets.Model = 3998;
				Dragonslayer_Holy_Mail_Gauntlets.Object_Type = 35;
				Dragonslayer_Holy_Mail_Gauntlets.Item_Type = 22;
				Dragonslayer_Holy_Mail_Gauntlets.Id_nb = "Dragonslayer_Holy_Mail_Gauntlets";
				Dragonslayer_Holy_Mail_Gauntlets.Hand = 0;
				Dragonslayer_Holy_Mail_Gauntlets.IsPickable = true;
				Dragonslayer_Holy_Mail_Gauntlets.IsDropable = true;
				Dragonslayer_Holy_Mail_Gauntlets.IsTradable = true;
				Dragonslayer_Holy_Mail_Gauntlets.CanDropAsLoot = true;
				Dragonslayer_Holy_Mail_Gauntlets.Color = 0;
				Dragonslayer_Holy_Mail_Gauntlets.Bonus = 35; // default bonus				
				Dragonslayer_Holy_Mail_Gauntlets.Bonus1 = 40;
				Dragonslayer_Holy_Mail_Gauntlets.Bonus1Type = (int) 10;
				Dragonslayer_Holy_Mail_Gauntlets.Bonus2 = 5;
				Dragonslayer_Holy_Mail_Gauntlets.Bonus2Type = (int) 12;
				Dragonslayer_Holy_Mail_Gauntlets.Bonus3 = 5;
				Dragonslayer_Holy_Mail_Gauntlets.Bonus3Type = (int) 15;
				Dragonslayer_Holy_Mail_Gauntlets.Bonus4 = 5;
				Dragonslayer_Holy_Mail_Gauntlets.Bonus4Type = (int) 18;
				Dragonslayer_Holy_Mail_Gauntlets.Bonus5 = 5;
				Dragonslayer_Holy_Mail_Gauntlets.Bonus5Type = (int) 195;
				Dragonslayer_Holy_Mail_Gauntlets.Bonus6 = 5;
				Dragonslayer_Holy_Mail_Gauntlets.Bonus6Type = (int) 190;
				Dragonslayer_Holy_Mail_Gauntlets.Bonus7 = 6;
				Dragonslayer_Holy_Mail_Gauntlets.Bonus7Type = (int) 196;
				Dragonslayer_Holy_Mail_Gauntlets.Bonus8 = 0;
				Dragonslayer_Holy_Mail_Gauntlets.Bonus8Type = (int) 0;
				Dragonslayer_Holy_Mail_Gauntlets.Bonus9 = 0;
				Dragonslayer_Holy_Mail_Gauntlets.Bonus9Type = (int) 0;
				Dragonslayer_Holy_Mail_Gauntlets.Bonus10 = 0;
				Dragonslayer_Holy_Mail_Gauntlets.Bonus10Type = (int) 0;
				Dragonslayer_Holy_Mail_Gauntlets.ExtraBonus = 0;
				Dragonslayer_Holy_Mail_Gauntlets.ExtraBonusType = (int) 0;
				Dragonslayer_Holy_Mail_Gauntlets.Effect = 0;
				Dragonslayer_Holy_Mail_Gauntlets.Emblem = 0;
				Dragonslayer_Holy_Mail_Gauntlets.Charges = 0;
				Dragonslayer_Holy_Mail_Gauntlets.MaxCharges = 0;
				Dragonslayer_Holy_Mail_Gauntlets.SpellID = 0;
				Dragonslayer_Holy_Mail_Gauntlets.ProcSpellID = 31001;
				Dragonslayer_Holy_Mail_Gauntlets.Type_Damage = 0;
				Dragonslayer_Holy_Mail_Gauntlets.Realm = 0;
				Dragonslayer_Holy_Mail_Gauntlets.MaxCount = 1;
				Dragonslayer_Holy_Mail_Gauntlets.PackSize = 1;
				Dragonslayer_Holy_Mail_Gauntlets.Extension = 5;
				Dragonslayer_Holy_Mail_Gauntlets.Quality = 100;				
				Dragonslayer_Holy_Mail_Gauntlets.Condition = 50000;
				Dragonslayer_Holy_Mail_Gauntlets.MaxCondition = 50000;
				Dragonslayer_Holy_Mail_Gauntlets.Durability = 50000;
				Dragonslayer_Holy_Mail_Gauntlets.MaxDurability = 0;
				Dragonslayer_Holy_Mail_Gauntlets.PoisonCharges = 0;
				Dragonslayer_Holy_Mail_Gauntlets.PoisonMaxCharges = 0;
				Dragonslayer_Holy_Mail_Gauntlets.PoisonSpellID = 0;
				Dragonslayer_Holy_Mail_Gauntlets.ProcSpellID1 = 40006;
				Dragonslayer_Holy_Mail_Gauntlets.SpellID1 = 0;
				Dragonslayer_Holy_Mail_Gauntlets.MaxCharges1 = 0;
				Dragonslayer_Holy_Mail_Gauntlets.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Holy_Mail_Gauntlets);
				}
			Dragonslayer_Holy_Mail_Armguards = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Holy_Mail_Armguards");
			if (Dragonslayer_Holy_Mail_Armguards == null)
			{
				Dragonslayer_Holy_Mail_Armguards = new ItemTemplate();
				Dragonslayer_Holy_Mail_Armguards.Name = "Dragonslayer Holy Mail Armguards";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Holy_Mail_Armguards.Name + ", creating it ...");
				Dragonslayer_Holy_Mail_Armguards.Level = 51;
				Dragonslayer_Holy_Mail_Armguards.Weight = 48;
				Dragonslayer_Holy_Mail_Armguards.Model = 3997;
				Dragonslayer_Holy_Mail_Armguards.Object_Type = 35;
				Dragonslayer_Holy_Mail_Armguards.Item_Type = 28;
				Dragonslayer_Holy_Mail_Armguards.Id_nb = "Dragonslayer_Holy_Mail_Armguards";
				Dragonslayer_Holy_Mail_Armguards.Hand = 0;
				Dragonslayer_Holy_Mail_Armguards.IsPickable = true;
				Dragonslayer_Holy_Mail_Armguards.IsDropable = true;
				Dragonslayer_Holy_Mail_Armguards.IsTradable = true;
				Dragonslayer_Holy_Mail_Armguards.CanDropAsLoot = true;
				Dragonslayer_Holy_Mail_Armguards.Color = 0;
				Dragonslayer_Holy_Mail_Armguards.Bonus = 35; // default bonus				
				Dragonslayer_Holy_Mail_Armguards.Bonus1 = 22;
				Dragonslayer_Holy_Mail_Armguards.Bonus1Type = (int) 1;
				Dragonslayer_Holy_Mail_Armguards.Bonus2 = 40;
				Dragonslayer_Holy_Mail_Armguards.Bonus2Type = (int) 10;
				Dragonslayer_Holy_Mail_Armguards.Bonus3 = 5;
				Dragonslayer_Holy_Mail_Armguards.Bonus3Type = (int) 12;
				Dragonslayer_Holy_Mail_Armguards.Bonus4 = 5;
				Dragonslayer_Holy_Mail_Armguards.Bonus4Type = (int) 15;
				Dragonslayer_Holy_Mail_Armguards.Bonus5 = 5;
				Dragonslayer_Holy_Mail_Armguards.Bonus5Type = (int) 18;
				Dragonslayer_Holy_Mail_Armguards.Bonus6 = 1;
				Dragonslayer_Holy_Mail_Armguards.Bonus6Type = (int) 191;
				Dragonslayer_Holy_Mail_Armguards.Bonus7 = 5;
				Dragonslayer_Holy_Mail_Armguards.Bonus7Type = (int) 209;
				Dragonslayer_Holy_Mail_Armguards.Bonus8 = 0;
				Dragonslayer_Holy_Mail_Armguards.Bonus8Type = (int) 0;
				Dragonslayer_Holy_Mail_Armguards.Bonus9 = 0;
				Dragonslayer_Holy_Mail_Armguards.Bonus9Type = (int) 0;
				Dragonslayer_Holy_Mail_Armguards.Bonus10 = 0;
				Dragonslayer_Holy_Mail_Armguards.Bonus10Type = (int) 0;
				Dragonslayer_Holy_Mail_Armguards.ExtraBonus = 0;
				Dragonslayer_Holy_Mail_Armguards.ExtraBonusType = (int) 0;
				Dragonslayer_Holy_Mail_Armguards.Effect = 0;
				Dragonslayer_Holy_Mail_Armguards.Emblem = 0;
				Dragonslayer_Holy_Mail_Armguards.Charges = 0;
				Dragonslayer_Holy_Mail_Armguards.MaxCharges = 0;
				Dragonslayer_Holy_Mail_Armguards.SpellID = 0;
				Dragonslayer_Holy_Mail_Armguards.ProcSpellID = 31001;
				Dragonslayer_Holy_Mail_Armguards.Type_Damage = 0;
				Dragonslayer_Holy_Mail_Armguards.Realm = 0;
				Dragonslayer_Holy_Mail_Armguards.MaxCount = 1;
				Dragonslayer_Holy_Mail_Armguards.PackSize = 1;
				Dragonslayer_Holy_Mail_Armguards.Extension = 5;
				Dragonslayer_Holy_Mail_Armguards.Quality = 100;				
				Dragonslayer_Holy_Mail_Armguards.Condition = 50000;
				Dragonslayer_Holy_Mail_Armguards.MaxCondition = 50000;
				Dragonslayer_Holy_Mail_Armguards.Durability = 50000;
				Dragonslayer_Holy_Mail_Armguards.MaxDurability = 0;
				Dragonslayer_Holy_Mail_Armguards.PoisonCharges = 0;
				Dragonslayer_Holy_Mail_Armguards.PoisonMaxCharges = 0;
				Dragonslayer_Holy_Mail_Armguards.PoisonSpellID = 0;
				Dragonslayer_Holy_Mail_Armguards.ProcSpellID1 = 40006;
				Dragonslayer_Holy_Mail_Armguards.SpellID1 = 0;
				Dragonslayer_Holy_Mail_Armguards.MaxCharges1 = 0;
				Dragonslayer_Holy_Mail_Armguards.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Holy_Mail_Armguards);
				}
			Dragonslayer_Holy_Mail_Chausses = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Holy_Mail_Chausses");
			if (Dragonslayer_Holy_Mail_Chausses == null)
			{
				Dragonslayer_Holy_Mail_Chausses = new ItemTemplate();
				Dragonslayer_Holy_Mail_Chausses.Name = "Dragonslayer Holy Mail Chausses";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Holy_Mail_Chausses.Name + ", creating it ...");
				Dragonslayer_Holy_Mail_Chausses.Level = 51;
				Dragonslayer_Holy_Mail_Chausses.Weight = 56;
				Dragonslayer_Holy_Mail_Chausses.Model = 3996;
				Dragonslayer_Holy_Mail_Chausses.Object_Type = 35;
				Dragonslayer_Holy_Mail_Chausses.Item_Type = 27;
				Dragonslayer_Holy_Mail_Chausses.Id_nb = "Dragonslayer_Holy_Mail_Chausses";
				Dragonslayer_Holy_Mail_Chausses.Hand = 0;
				Dragonslayer_Holy_Mail_Chausses.IsPickable = true;
				Dragonslayer_Holy_Mail_Chausses.IsDropable = true;
				Dragonslayer_Holy_Mail_Chausses.IsTradable = true;
				Dragonslayer_Holy_Mail_Chausses.CanDropAsLoot = true;
				Dragonslayer_Holy_Mail_Chausses.Color = 0;
				Dragonslayer_Holy_Mail_Chausses.Bonus = 35; // default bonus				
				Dragonslayer_Holy_Mail_Chausses.Bonus1 = 22;
				Dragonslayer_Holy_Mail_Chausses.Bonus1Type = (int) 3;
				Dragonslayer_Holy_Mail_Chausses.Bonus2 = 5;
				Dragonslayer_Holy_Mail_Chausses.Bonus2Type = (int) 12;
				Dragonslayer_Holy_Mail_Chausses.Bonus3 = 5;
				Dragonslayer_Holy_Mail_Chausses.Bonus3Type = (int) 18;
				Dragonslayer_Holy_Mail_Chausses.Bonus4 = 40;
				Dragonslayer_Holy_Mail_Chausses.Bonus4Type = (int) 10;
				Dragonslayer_Holy_Mail_Chausses.Bonus5 = 5;
				Dragonslayer_Holy_Mail_Chausses.Bonus5Type = (int) 15;
				Dragonslayer_Holy_Mail_Chausses.Bonus6 = 5;
				Dragonslayer_Holy_Mail_Chausses.Bonus6Type = (int) 202;
				Dragonslayer_Holy_Mail_Chausses.Bonus7 = 1;
				Dragonslayer_Holy_Mail_Chausses.Bonus7Type = (int) 191;
				Dragonslayer_Holy_Mail_Chausses.Bonus8 = 0;
				Dragonslayer_Holy_Mail_Chausses.Bonus8Type = (int) 0;
				Dragonslayer_Holy_Mail_Chausses.Bonus9 = 0;
				Dragonslayer_Holy_Mail_Chausses.Bonus9Type = (int) 0;
				Dragonslayer_Holy_Mail_Chausses.Bonus10 = 0;
				Dragonslayer_Holy_Mail_Chausses.Bonus10Type = (int) 0;
				Dragonslayer_Holy_Mail_Chausses.ExtraBonus = 0;
				Dragonslayer_Holy_Mail_Chausses.ExtraBonusType = (int) 0;
				Dragonslayer_Holy_Mail_Chausses.Effect = 0;
				Dragonslayer_Holy_Mail_Chausses.Emblem = 0;
				Dragonslayer_Holy_Mail_Chausses.Charges = 0;
				Dragonslayer_Holy_Mail_Chausses.MaxCharges = 0;
				Dragonslayer_Holy_Mail_Chausses.SpellID = 0;
				Dragonslayer_Holy_Mail_Chausses.ProcSpellID = 31001;
				Dragonslayer_Holy_Mail_Chausses.Type_Damage = 0;
				Dragonslayer_Holy_Mail_Chausses.Realm = 0;
				Dragonslayer_Holy_Mail_Chausses.MaxCount = 1;
				Dragonslayer_Holy_Mail_Chausses.PackSize = 1;
				Dragonslayer_Holy_Mail_Chausses.Extension = 5;
				Dragonslayer_Holy_Mail_Chausses.Quality = 100;				
				Dragonslayer_Holy_Mail_Chausses.Condition = 50000;
				Dragonslayer_Holy_Mail_Chausses.MaxCondition = 50000;
				Dragonslayer_Holy_Mail_Chausses.Durability = 50000;
				Dragonslayer_Holy_Mail_Chausses.MaxDurability = 0;
				Dragonslayer_Holy_Mail_Chausses.PoisonCharges = 0;
				Dragonslayer_Holy_Mail_Chausses.PoisonMaxCharges = 0;
				Dragonslayer_Holy_Mail_Chausses.PoisonSpellID = 0;
				Dragonslayer_Holy_Mail_Chausses.ProcSpellID1 = 40006;
				Dragonslayer_Holy_Mail_Chausses.SpellID1 = 0;
				Dragonslayer_Holy_Mail_Chausses.MaxCharges1 = 0;
				Dragonslayer_Holy_Mail_Chausses.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Holy_Mail_Chausses);
				}
			Dragonslayer_Holy_Mail_Boots = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Holy_Mail_Boots");
			if (Dragonslayer_Holy_Mail_Boots == null)
			{
				Dragonslayer_Holy_Mail_Boots = new ItemTemplate();
				Dragonslayer_Holy_Mail_Boots.Name = "Dragonslayer Holy Mail Boots";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Holy_Mail_Boots.Name + ", creating it ...");
				Dragonslayer_Holy_Mail_Boots.Level = 51;
				Dragonslayer_Holy_Mail_Boots.Weight = 32;
				Dragonslayer_Holy_Mail_Boots.Model = 3999;
				Dragonslayer_Holy_Mail_Boots.Object_Type = 35;
				Dragonslayer_Holy_Mail_Boots.Item_Type = 23;
				Dragonslayer_Holy_Mail_Boots.Id_nb = "Dragonslayer_Holy_Mail_Boots";
				Dragonslayer_Holy_Mail_Boots.Hand = 0;
				Dragonslayer_Holy_Mail_Boots.IsPickable = true;
				Dragonslayer_Holy_Mail_Boots.IsDropable = true;
				Dragonslayer_Holy_Mail_Boots.IsTradable = true;
				Dragonslayer_Holy_Mail_Boots.CanDropAsLoot = true;
				Dragonslayer_Holy_Mail_Boots.Color = 0;
				Dragonslayer_Holy_Mail_Boots.Bonus = 35; // default bonus				
				Dragonslayer_Holy_Mail_Boots.Bonus1 = 18;
				Dragonslayer_Holy_Mail_Boots.Bonus1Type = (int) 2;
				Dragonslayer_Holy_Mail_Boots.Bonus2 = 6;
				Dragonslayer_Holy_Mail_Boots.Bonus2Type = (int) 13;
				Dragonslayer_Holy_Mail_Boots.Bonus3 = 6;
				Dragonslayer_Holy_Mail_Boots.Bonus3Type = (int) 17;
				Dragonslayer_Holy_Mail_Boots.Bonus4 = 6;
				Dragonslayer_Holy_Mail_Boots.Bonus4Type = (int) 19;
				Dragonslayer_Holy_Mail_Boots.Bonus5 = 18;
				Dragonslayer_Holy_Mail_Boots.Bonus5Type = (int) 156;
				Dragonslayer_Holy_Mail_Boots.Bonus6 = 6;
				Dragonslayer_Holy_Mail_Boots.Bonus6Type = (int) 196;
				Dragonslayer_Holy_Mail_Boots.Bonus7 = 0;
				Dragonslayer_Holy_Mail_Boots.Bonus7Type = (int) 0;
				Dragonslayer_Holy_Mail_Boots.Bonus8 = 0;
				Dragonslayer_Holy_Mail_Boots.Bonus8Type = (int) 0;
				Dragonslayer_Holy_Mail_Boots.Bonus9 = 0;
				Dragonslayer_Holy_Mail_Boots.Bonus9Type = (int) 0;
				Dragonslayer_Holy_Mail_Boots.Bonus10 = 0;
				Dragonslayer_Holy_Mail_Boots.Bonus10Type = (int) 0;
				Dragonslayer_Holy_Mail_Boots.ExtraBonus = 0;
				Dragonslayer_Holy_Mail_Boots.ExtraBonusType = (int) 0;
				Dragonslayer_Holy_Mail_Boots.Effect = 0;
				Dragonslayer_Holy_Mail_Boots.Emblem = 0;
				Dragonslayer_Holy_Mail_Boots.Charges = 0;
				Dragonslayer_Holy_Mail_Boots.MaxCharges = 0;
				Dragonslayer_Holy_Mail_Boots.SpellID = 0;
				Dragonslayer_Holy_Mail_Boots.ProcSpellID = 31001;
				Dragonslayer_Holy_Mail_Boots.Type_Damage = 0;
				Dragonslayer_Holy_Mail_Boots.Realm = 0;
				Dragonslayer_Holy_Mail_Boots.MaxCount = 1;
				Dragonslayer_Holy_Mail_Boots.PackSize = 1;
				Dragonslayer_Holy_Mail_Boots.Extension = 5;
				Dragonslayer_Holy_Mail_Boots.Quality = 100;				
				Dragonslayer_Holy_Mail_Boots.Condition = 50000;
				Dragonslayer_Holy_Mail_Boots.MaxCondition = 50000;
				Dragonslayer_Holy_Mail_Boots.Durability = 50000;
				Dragonslayer_Holy_Mail_Boots.MaxDurability = 0;
				Dragonslayer_Holy_Mail_Boots.PoisonCharges = 0;
				Dragonslayer_Holy_Mail_Boots.PoisonMaxCharges = 0;
				Dragonslayer_Holy_Mail_Boots.PoisonSpellID = 0;
				Dragonslayer_Holy_Mail_Boots.ProcSpellID1 = 40006;
				Dragonslayer_Holy_Mail_Boots.SpellID1 = 0;
				Dragonslayer_Holy_Mail_Boots.MaxCharges1 = 0;
				Dragonslayer_Holy_Mail_Boots.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Holy_Mail_Boots);
				}


            #endregion

            #region defineAreas

            #endregion

            #region defineQuestParts
            QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerClericQuest));
            QuestBehaviour a;
            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.Interact, null, Elia);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerClericQuest), Elia);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerClericQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "I have lived in this village since I was a young girl. My father was a great dragon hunter you know.", Elia);
            a.AddAction(eActionType.Talk, "In my years I have learned to work with all kinds of problems, specially with [Dragons].", Elia);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.Whisper, "Dragons", Elia);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerClericQuest), Elia);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerClericQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "yes dragons, but there is a problem for me, i can't hunt dragons by self, [you know].", Elia);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.Whisper, "you know", Elia);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerClericQuest), Elia);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerClericQuest), null, (eComparator)5);
            a.AddAction(eActionType.OfferQuest, typeof(DragonslayerClericQuest), "Helen has offered you the Dragonslayer quest.?Do you accept?");
            AddBehaviour(a);

            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.DeclineQuest, null, typeof(DragonslayerClericQuest));
            a.AddAction(eActionType.Talk, "No problem. See you", Elia);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.AcceptQuest, null, typeof(DragonslayerClericQuest));
            a.AddAction(eActionType.Talk, "If you bring me the head of Runicaath I shall pay you well!", Elia);
            a.AddAction(eActionType.GiveQuest, typeof(DragonslayerClericQuest), Elia);
            AddBehaviour(a);

			        			
			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End
			if (Elia!=null) {
				Elia.AddQuestToGive(typeof (DragonslayerClericQuest));
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
			Elia.RemoveQuestToGive(typeof (DragonslayerClericQuest));
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
						return "[Step #2] bring the head of Runicaath to Elia";
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
			if (player.IsDoingQuest(typeof (DragonslayerClericQuest)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
			if (
		
			player.CharacterClass.ID != (byte) eCharacterClass.Cleric && 
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
