	
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
	public class DragonslayerFriarQuest : BaseQuest
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

		protected const string questTitle = "DragonslayerFriarQuest";

		protected const int minimumLevel = 49;
		protected const int maximumLevel = 50;
	
	
		private static GameNPC Elia = null;
		
		private static GameNPC Runicaath = null;
		
		private static ItemTemplate Runicaath_Head = null;
		
		private static ItemTemplate Dragonslayer_Blessed_Leather_Shoes = null;
		
		private static ItemTemplate Dragonslayer_Blessed_Leather_Tunic = null;
		
		private static ItemTemplate Dragonslayer_Blessed_Leather_Sleeves = null;
		
		private static ItemTemplate Dragonslayer_Hard_Leather_Full_Helm = null;
		
		private static ItemTemplate Dragonslayer_Hard_Leather_Pants = null;
		
		private static ItemTemplate Dragonslayer_Hard_Leather_Gloves = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public DragonslayerFriarQuest() : base()
		{
		}

		public DragonslayerFriarQuest(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public DragonslayerFriarQuest(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public DragonslayerFriarQuest(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
	{
	}

		
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			QuestBehaviour a;
			QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerFriarQuest));

			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(DragonslayerFriarQuest)) == null)
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
						a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerFriarQuest), null);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Blessed_Leather_Shoes, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Blessed_Leather_Tunic, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Blessed_Leather_Sleeves, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Hard_Leather_Full_Helm, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Hard_Leather_Pants, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Hard_Leather_Gloves, Elia);
						a.AddAction(eActionType.FinishQuest, typeof(DragonslayerFriarQuest), null);
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
			Dragonslayer_Blessed_Leather_Shoes = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Blessed_Leather_Shoes");
			if (Dragonslayer_Blessed_Leather_Shoes == null)
			{
				Dragonslayer_Blessed_Leather_Shoes = new ItemTemplate();
				Dragonslayer_Blessed_Leather_Shoes.Name = "Dragonslayer Blessed Leather Shoes";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Blessed_Leather_Shoes.Name + ", creating it ...");
				Dragonslayer_Blessed_Leather_Shoes.Level = 51;
				Dragonslayer_Blessed_Leather_Shoes.Weight = 22;
				Dragonslayer_Blessed_Leather_Shoes.Model = 3994;
				Dragonslayer_Blessed_Leather_Shoes.Object_Type = 33;
				Dragonslayer_Blessed_Leather_Shoes.Item_Type = 23;
				Dragonslayer_Blessed_Leather_Shoes.Id_nb = "Dragonslayer_Blessed_Leather_Shoes";
				Dragonslayer_Blessed_Leather_Shoes.Hand = 0;
				Dragonslayer_Blessed_Leather_Shoes.IsPickable = true;
				Dragonslayer_Blessed_Leather_Shoes.IsDropable = true;
				Dragonslayer_Blessed_Leather_Shoes.IsTradable = true;
				Dragonslayer_Blessed_Leather_Shoes.CanDropAsLoot = true;
				Dragonslayer_Blessed_Leather_Shoes.Color = 0;
				Dragonslayer_Blessed_Leather_Shoes.Bonus = 35; // default bonus				
				Dragonslayer_Blessed_Leather_Shoes.Bonus1 = 18;
				Dragonslayer_Blessed_Leather_Shoes.Bonus1Type = (int) 3;
				Dragonslayer_Blessed_Leather_Shoes.Bonus2 = 18;
				Dragonslayer_Blessed_Leather_Shoes.Bonus2Type = (int) 2;
				Dragonslayer_Blessed_Leather_Shoes.Bonus3 = 6;
				Dragonslayer_Blessed_Leather_Shoes.Bonus3Type = (int) 13;
				Dragonslayer_Blessed_Leather_Shoes.Bonus4 = 6;
				Dragonslayer_Blessed_Leather_Shoes.Bonus4Type = (int) 17;
				Dragonslayer_Blessed_Leather_Shoes.Bonus5 = 6;
				Dragonslayer_Blessed_Leather_Shoes.Bonus5Type = (int) 19;
				Dragonslayer_Blessed_Leather_Shoes.Bonus6 = 6;
				Dragonslayer_Blessed_Leather_Shoes.Bonus6Type = (int) 195;
				Dragonslayer_Blessed_Leather_Shoes.Bonus7 = 0;
				Dragonslayer_Blessed_Leather_Shoes.Bonus7Type = (int) 0;
				Dragonslayer_Blessed_Leather_Shoes.Bonus8 = 0;
				Dragonslayer_Blessed_Leather_Shoes.Bonus8Type = (int) 0;
				Dragonslayer_Blessed_Leather_Shoes.Bonus9 = 0;
				Dragonslayer_Blessed_Leather_Shoes.Bonus9Type = (int) 0;
				Dragonslayer_Blessed_Leather_Shoes.Bonus10 = 0;
				Dragonslayer_Blessed_Leather_Shoes.Bonus10Type = (int) 0;
				Dragonslayer_Blessed_Leather_Shoes.ExtraBonus = 0;
				Dragonslayer_Blessed_Leather_Shoes.ExtraBonusType = (int) 0;
				Dragonslayer_Blessed_Leather_Shoes.Effect = 0;
				Dragonslayer_Blessed_Leather_Shoes.Emblem = 0;
				Dragonslayer_Blessed_Leather_Shoes.Charges = 0;
				Dragonslayer_Blessed_Leather_Shoes.MaxCharges = 0;
				Dragonslayer_Blessed_Leather_Shoes.SpellID = 0;
				Dragonslayer_Blessed_Leather_Shoes.ProcSpellID = 31001;
				Dragonslayer_Blessed_Leather_Shoes.Type_Damage = 0;
				Dragonslayer_Blessed_Leather_Shoes.Realm = 0;
				Dragonslayer_Blessed_Leather_Shoes.MaxCount = 1;
				Dragonslayer_Blessed_Leather_Shoes.PackSize = 1;
				Dragonslayer_Blessed_Leather_Shoes.Extension = 5;
				Dragonslayer_Blessed_Leather_Shoes.Quality = 100;				
				Dragonslayer_Blessed_Leather_Shoes.Condition = 50000;
				Dragonslayer_Blessed_Leather_Shoes.MaxCondition = 50000;
				Dragonslayer_Blessed_Leather_Shoes.Durability = 50000;
				Dragonslayer_Blessed_Leather_Shoes.MaxDurability = 0;
				Dragonslayer_Blessed_Leather_Shoes.PoisonCharges = 0;
				Dragonslayer_Blessed_Leather_Shoes.PoisonMaxCharges = 0;
				Dragonslayer_Blessed_Leather_Shoes.PoisonSpellID = 0;
				Dragonslayer_Blessed_Leather_Shoes.ProcSpellID1 = 40006;
				Dragonslayer_Blessed_Leather_Shoes.SpellID1 = 0;
				Dragonslayer_Blessed_Leather_Shoes.MaxCharges1 = 0;
				Dragonslayer_Blessed_Leather_Shoes.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Blessed_Leather_Shoes);
				}
			Dragonslayer_Blessed_Leather_Tunic = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Blessed_Leather_Tunic");
			if (Dragonslayer_Blessed_Leather_Tunic == null)
			{
				Dragonslayer_Blessed_Leather_Tunic = new ItemTemplate();
				Dragonslayer_Blessed_Leather_Tunic.Name = "Dragonslayer Blessed Leather Tunic";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Blessed_Leather_Tunic.Name + ", creating it ...");
				Dragonslayer_Blessed_Leather_Tunic.Level = 51;
				Dragonslayer_Blessed_Leather_Tunic.Weight = 22;
				Dragonslayer_Blessed_Leather_Tunic.Model = 3990;
				Dragonslayer_Blessed_Leather_Tunic.Object_Type = 33;
				Dragonslayer_Blessed_Leather_Tunic.Item_Type = 25;
				Dragonslayer_Blessed_Leather_Tunic.Id_nb = "Dragonslayer_Blessed_Leather_Tunic";
				Dragonslayer_Blessed_Leather_Tunic.Hand = 0;
				Dragonslayer_Blessed_Leather_Tunic.IsPickable = true;
				Dragonslayer_Blessed_Leather_Tunic.IsDropable = true;
				Dragonslayer_Blessed_Leather_Tunic.IsTradable = true;
				Dragonslayer_Blessed_Leather_Tunic.CanDropAsLoot = true;
				Dragonslayer_Blessed_Leather_Tunic.Color = 0;
				Dragonslayer_Blessed_Leather_Tunic.Bonus = 35; // default bonus				
				Dragonslayer_Blessed_Leather_Tunic.Bonus1 = 22;
				Dragonslayer_Blessed_Leather_Tunic.Bonus1Type = (int) 2;
				Dragonslayer_Blessed_Leather_Tunic.Bonus2 = 22;
				Dragonslayer_Blessed_Leather_Tunic.Bonus2Type = (int) 4;
				Dragonslayer_Blessed_Leather_Tunic.Bonus3 = 5;
				Dragonslayer_Blessed_Leather_Tunic.Bonus3Type = (int) 12;
				Dragonslayer_Blessed_Leather_Tunic.Bonus4 = 5;
				Dragonslayer_Blessed_Leather_Tunic.Bonus4Type = (int) 18;
				Dragonslayer_Blessed_Leather_Tunic.Bonus5 = 5;
				Dragonslayer_Blessed_Leather_Tunic.Bonus5Type = (int) 15;
				Dragonslayer_Blessed_Leather_Tunic.Bonus6 = 5;
				Dragonslayer_Blessed_Leather_Tunic.Bonus6Type = (int) 202;
				Dragonslayer_Blessed_Leather_Tunic.Bonus7 = 5;
				Dragonslayer_Blessed_Leather_Tunic.Bonus7Type = (int) 204;
				Dragonslayer_Blessed_Leather_Tunic.Bonus8 = 2;
				Dragonslayer_Blessed_Leather_Tunic.Bonus8Type = (int) 155;
				Dragonslayer_Blessed_Leather_Tunic.Bonus9 = 0;
				Dragonslayer_Blessed_Leather_Tunic.Bonus9Type = (int) 0;
				Dragonslayer_Blessed_Leather_Tunic.Bonus10 = 0;
				Dragonslayer_Blessed_Leather_Tunic.Bonus10Type = (int) 0;
				Dragonslayer_Blessed_Leather_Tunic.ExtraBonus = 0;
				Dragonslayer_Blessed_Leather_Tunic.ExtraBonusType = (int) 0;
				Dragonslayer_Blessed_Leather_Tunic.Effect = 0;
				Dragonslayer_Blessed_Leather_Tunic.Emblem = 0;
				Dragonslayer_Blessed_Leather_Tunic.Charges = 0;
				Dragonslayer_Blessed_Leather_Tunic.MaxCharges = 0;
				Dragonslayer_Blessed_Leather_Tunic.SpellID = 0;
				Dragonslayer_Blessed_Leather_Tunic.ProcSpellID = 31001;
				Dragonslayer_Blessed_Leather_Tunic.Type_Damage = 0;
				Dragonslayer_Blessed_Leather_Tunic.Realm = 0;
				Dragonslayer_Blessed_Leather_Tunic.MaxCount = 1;
				Dragonslayer_Blessed_Leather_Tunic.PackSize = 1;
				Dragonslayer_Blessed_Leather_Tunic.Extension = 5;
				Dragonslayer_Blessed_Leather_Tunic.Quality = 100;				
				Dragonslayer_Blessed_Leather_Tunic.Condition = 50000;
				Dragonslayer_Blessed_Leather_Tunic.MaxCondition = 50000;
				Dragonslayer_Blessed_Leather_Tunic.Durability = 50000;
				Dragonslayer_Blessed_Leather_Tunic.MaxDurability = 0;
				Dragonslayer_Blessed_Leather_Tunic.PoisonCharges = 0;
				Dragonslayer_Blessed_Leather_Tunic.PoisonMaxCharges = 0;
				Dragonslayer_Blessed_Leather_Tunic.PoisonSpellID = 0;
				Dragonslayer_Blessed_Leather_Tunic.ProcSpellID1 = 40006;
				Dragonslayer_Blessed_Leather_Tunic.SpellID1 = 0;
				Dragonslayer_Blessed_Leather_Tunic.MaxCharges1 = 0;
				Dragonslayer_Blessed_Leather_Tunic.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Blessed_Leather_Tunic);
				}
			Dragonslayer_Blessed_Leather_Sleeves = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Blessed_Leather_Sleeves");
			if (Dragonslayer_Blessed_Leather_Sleeves == null)
			{
				Dragonslayer_Blessed_Leather_Sleeves = new ItemTemplate();
				Dragonslayer_Blessed_Leather_Sleeves.Name = "Dragonslayer Blessed Leather Sleeves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Blessed_Leather_Sleeves.Name + ", creating it ...");
				Dragonslayer_Blessed_Leather_Sleeves.Level = 51;
				Dragonslayer_Blessed_Leather_Sleeves.Weight = 22;
				Dragonslayer_Blessed_Leather_Sleeves.Model = 3992;
				Dragonslayer_Blessed_Leather_Sleeves.Object_Type = 33;
				Dragonslayer_Blessed_Leather_Sleeves.Item_Type = 28;
				Dragonslayer_Blessed_Leather_Sleeves.Id_nb = "Dragonslayer_Blessed_Leather_Sleeves";
				Dragonslayer_Blessed_Leather_Sleeves.Hand = 0;
				Dragonslayer_Blessed_Leather_Sleeves.IsPickable = true;
				Dragonslayer_Blessed_Leather_Sleeves.IsDropable = true;
				Dragonslayer_Blessed_Leather_Sleeves.IsTradable = true;
				Dragonslayer_Blessed_Leather_Sleeves.CanDropAsLoot = true;
				Dragonslayer_Blessed_Leather_Sleeves.Color = 0;
				Dragonslayer_Blessed_Leather_Sleeves.Bonus = 35; // default bonus				
				Dragonslayer_Blessed_Leather_Sleeves.Bonus1 = 22;
				Dragonslayer_Blessed_Leather_Sleeves.Bonus1Type = (int) 2;
				Dragonslayer_Blessed_Leather_Sleeves.Bonus2 = 5;
				Dragonslayer_Blessed_Leather_Sleeves.Bonus2Type = (int) 12;
				Dragonslayer_Blessed_Leather_Sleeves.Bonus3 = 5;
				Dragonslayer_Blessed_Leather_Sleeves.Bonus3Type = (int) 18;
				Dragonslayer_Blessed_Leather_Sleeves.Bonus4 = 40;
				Dragonslayer_Blessed_Leather_Sleeves.Bonus4Type = (int) 10;
				Dragonslayer_Blessed_Leather_Sleeves.Bonus5 = 5;
				Dragonslayer_Blessed_Leather_Sleeves.Bonus5Type = (int) 15;
				Dragonslayer_Blessed_Leather_Sleeves.Bonus6 = 1;
				Dragonslayer_Blessed_Leather_Sleeves.Bonus6Type = (int) 155;
				Dragonslayer_Blessed_Leather_Sleeves.Bonus7 = 5;
				Dragonslayer_Blessed_Leather_Sleeves.Bonus7Type = (int) 202;
				Dragonslayer_Blessed_Leather_Sleeves.Bonus8 = 0;
				Dragonslayer_Blessed_Leather_Sleeves.Bonus8Type = (int) 0;
				Dragonslayer_Blessed_Leather_Sleeves.Bonus9 = 0;
				Dragonslayer_Blessed_Leather_Sleeves.Bonus9Type = (int) 0;
				Dragonslayer_Blessed_Leather_Sleeves.Bonus10 = 0;
				Dragonslayer_Blessed_Leather_Sleeves.Bonus10Type = (int) 0;
				Dragonslayer_Blessed_Leather_Sleeves.ExtraBonus = 0;
				Dragonslayer_Blessed_Leather_Sleeves.ExtraBonusType = (int) 0;
				Dragonslayer_Blessed_Leather_Sleeves.Effect = 0;
				Dragonslayer_Blessed_Leather_Sleeves.Emblem = 0;
				Dragonslayer_Blessed_Leather_Sleeves.Charges = 0;
				Dragonslayer_Blessed_Leather_Sleeves.MaxCharges = 0;
				Dragonslayer_Blessed_Leather_Sleeves.SpellID = 0;
				Dragonslayer_Blessed_Leather_Sleeves.ProcSpellID = 31001;
				Dragonslayer_Blessed_Leather_Sleeves.Type_Damage = 0;
				Dragonslayer_Blessed_Leather_Sleeves.Realm = 0;
				Dragonslayer_Blessed_Leather_Sleeves.MaxCount = 1;
				Dragonslayer_Blessed_Leather_Sleeves.PackSize = 1;
				Dragonslayer_Blessed_Leather_Sleeves.Extension = 5;
				Dragonslayer_Blessed_Leather_Sleeves.Quality = 100;				
				Dragonslayer_Blessed_Leather_Sleeves.Condition = 50000;
				Dragonslayer_Blessed_Leather_Sleeves.MaxCondition = 50000;
				Dragonslayer_Blessed_Leather_Sleeves.Durability = 50000;
				Dragonslayer_Blessed_Leather_Sleeves.MaxDurability = 0;
				Dragonslayer_Blessed_Leather_Sleeves.PoisonCharges = 0;
				Dragonslayer_Blessed_Leather_Sleeves.PoisonMaxCharges = 0;
				Dragonslayer_Blessed_Leather_Sleeves.PoisonSpellID = 0;
				Dragonslayer_Blessed_Leather_Sleeves.ProcSpellID1 = 40006;
				Dragonslayer_Blessed_Leather_Sleeves.SpellID1 = 0;
				Dragonslayer_Blessed_Leather_Sleeves.MaxCharges1 = 0;
				Dragonslayer_Blessed_Leather_Sleeves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Blessed_Leather_Sleeves);
				}
			Dragonslayer_Hard_Leather_Full_Helm = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Hard_Leather_Full_Helm");
			if (Dragonslayer_Hard_Leather_Full_Helm == null)
			{
				Dragonslayer_Hard_Leather_Full_Helm = new ItemTemplate();
				Dragonslayer_Hard_Leather_Full_Helm.Name = "Dragonslayer Hard Leather Full Helm";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Hard_Leather_Full_Helm.Name + ", creating it ...");
				Dragonslayer_Hard_Leather_Full_Helm.Level = 51;
				Dragonslayer_Hard_Leather_Full_Helm.Weight = 22;
				Dragonslayer_Hard_Leather_Full_Helm.Model = 4054;
				Dragonslayer_Hard_Leather_Full_Helm.Object_Type = 33;
				Dragonslayer_Hard_Leather_Full_Helm.Item_Type = 21;
				Dragonslayer_Hard_Leather_Full_Helm.Id_nb = "Dragonslayer_Hard_Leather_Full_Helm";
				Dragonslayer_Hard_Leather_Full_Helm.Hand = 0;
				Dragonslayer_Hard_Leather_Full_Helm.IsPickable = true;
				Dragonslayer_Hard_Leather_Full_Helm.IsDropable = true;
				Dragonslayer_Hard_Leather_Full_Helm.IsTradable = true;
				Dragonslayer_Hard_Leather_Full_Helm.CanDropAsLoot = true;
				Dragonslayer_Hard_Leather_Full_Helm.Color = 0;
				Dragonslayer_Hard_Leather_Full_Helm.Bonus = 35; // default bonus				
				Dragonslayer_Hard_Leather_Full_Helm.Bonus1 = 22;
				Dragonslayer_Hard_Leather_Full_Helm.Bonus1Type = (int) 3;
				Dragonslayer_Hard_Leather_Full_Helm.Bonus2 = 40;
				Dragonslayer_Hard_Leather_Full_Helm.Bonus2Type = (int) 10;
				Dragonslayer_Hard_Leather_Full_Helm.Bonus3 = 6;
				Dragonslayer_Hard_Leather_Full_Helm.Bonus3Type = (int) 11;
				Dragonslayer_Hard_Leather_Full_Helm.Bonus4 = 6;
				Dragonslayer_Hard_Leather_Full_Helm.Bonus4Type = (int) 14;
				Dragonslayer_Hard_Leather_Full_Helm.Bonus5 = 6;
				Dragonslayer_Hard_Leather_Full_Helm.Bonus5Type = (int) 16;
				Dragonslayer_Hard_Leather_Full_Helm.Bonus6 = 40;
				Dragonslayer_Hard_Leather_Full_Helm.Bonus6Type = (int) 210;
				Dragonslayer_Hard_Leather_Full_Helm.Bonus7 = 5;
				Dragonslayer_Hard_Leather_Full_Helm.Bonus7Type = (int) 148;
				Dragonslayer_Hard_Leather_Full_Helm.Bonus8 = 0;
				Dragonslayer_Hard_Leather_Full_Helm.Bonus8Type = (int) 0;
				Dragonslayer_Hard_Leather_Full_Helm.Bonus9 = 0;
				Dragonslayer_Hard_Leather_Full_Helm.Bonus9Type = (int) 0;
				Dragonslayer_Hard_Leather_Full_Helm.Bonus10 = 0;
				Dragonslayer_Hard_Leather_Full_Helm.Bonus10Type = (int) 0;
				Dragonslayer_Hard_Leather_Full_Helm.ExtraBonus = 0;
				Dragonslayer_Hard_Leather_Full_Helm.ExtraBonusType = (int) 0;
				Dragonslayer_Hard_Leather_Full_Helm.Effect = 0;
				Dragonslayer_Hard_Leather_Full_Helm.Emblem = 0;
				Dragonslayer_Hard_Leather_Full_Helm.Charges = 0;
				Dragonslayer_Hard_Leather_Full_Helm.MaxCharges = 0;
				Dragonslayer_Hard_Leather_Full_Helm.SpellID = 0;
				Dragonslayer_Hard_Leather_Full_Helm.ProcSpellID = 31001;
				Dragonslayer_Hard_Leather_Full_Helm.Type_Damage = 0;
				Dragonslayer_Hard_Leather_Full_Helm.Realm = 0;
				Dragonslayer_Hard_Leather_Full_Helm.MaxCount = 1;
				Dragonslayer_Hard_Leather_Full_Helm.PackSize = 1;
				Dragonslayer_Hard_Leather_Full_Helm.Extension = 0;
				Dragonslayer_Hard_Leather_Full_Helm.Quality = 100;				
				Dragonslayer_Hard_Leather_Full_Helm.Condition = 50000;
				Dragonslayer_Hard_Leather_Full_Helm.MaxCondition = 50000;
				Dragonslayer_Hard_Leather_Full_Helm.Durability = 50000;
				Dragonslayer_Hard_Leather_Full_Helm.MaxDurability = 0;
				Dragonslayer_Hard_Leather_Full_Helm.PoisonCharges = 0;
				Dragonslayer_Hard_Leather_Full_Helm.PoisonMaxCharges = 0;
				Dragonslayer_Hard_Leather_Full_Helm.PoisonSpellID = 0;
				Dragonslayer_Hard_Leather_Full_Helm.ProcSpellID1 = 40006;
				Dragonslayer_Hard_Leather_Full_Helm.SpellID1 = 0;
				Dragonslayer_Hard_Leather_Full_Helm.MaxCharges1 = 0;
				Dragonslayer_Hard_Leather_Full_Helm.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Hard_Leather_Full_Helm);
				}
			Dragonslayer_Hard_Leather_Pants = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Hard_Leather_Pants");
			if (Dragonslayer_Hard_Leather_Pants == null)
			{
				Dragonslayer_Hard_Leather_Pants = new ItemTemplate();
				Dragonslayer_Hard_Leather_Pants.Name = "Dragonslayer Hard Leather Pants";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Hard_Leather_Pants.Name + ", creating it ...");
				Dragonslayer_Hard_Leather_Pants.Level = 51;
				Dragonslayer_Hard_Leather_Pants.Weight = 22;
				Dragonslayer_Hard_Leather_Pants.Model = 3991;
				Dragonslayer_Hard_Leather_Pants.Object_Type = 33;
				Dragonslayer_Hard_Leather_Pants.Item_Type = 27;
				Dragonslayer_Hard_Leather_Pants.Id_nb = "Dragonslayer_Hard_Leather_Pants";
				Dragonslayer_Hard_Leather_Pants.Hand = 0;
				Dragonslayer_Hard_Leather_Pants.IsPickable = true;
				Dragonslayer_Hard_Leather_Pants.IsDropable = true;
				Dragonslayer_Hard_Leather_Pants.IsTradable = true;
				Dragonslayer_Hard_Leather_Pants.CanDropAsLoot = true;
				Dragonslayer_Hard_Leather_Pants.Color = 0;
				Dragonslayer_Hard_Leather_Pants.Bonus = 35; // default bonus				
				Dragonslayer_Hard_Leather_Pants.Bonus1 = 22;
				Dragonslayer_Hard_Leather_Pants.Bonus1Type = (int) 3;
				Dragonslayer_Hard_Leather_Pants.Bonus2 = 5;
				Dragonslayer_Hard_Leather_Pants.Bonus2Type = (int) 12;
				Dragonslayer_Hard_Leather_Pants.Bonus3 = 5;
				Dragonslayer_Hard_Leather_Pants.Bonus3Type = (int) 18;
				Dragonslayer_Hard_Leather_Pants.Bonus4 = 40;
				Dragonslayer_Hard_Leather_Pants.Bonus4Type = (int) 10;
				Dragonslayer_Hard_Leather_Pants.Bonus5 = 5;
				Dragonslayer_Hard_Leather_Pants.Bonus5Type = (int) 15;
				Dragonslayer_Hard_Leather_Pants.Bonus6 = 5;
				Dragonslayer_Hard_Leather_Pants.Bonus6Type = (int) 203;
				Dragonslayer_Hard_Leather_Pants.Bonus7 = 1;
				Dragonslayer_Hard_Leather_Pants.Bonus7Type = (int) 155;
				Dragonslayer_Hard_Leather_Pants.Bonus8 = 0;
				Dragonslayer_Hard_Leather_Pants.Bonus8Type = (int) 0;
				Dragonslayer_Hard_Leather_Pants.Bonus9 = 0;
				Dragonslayer_Hard_Leather_Pants.Bonus9Type = (int) 0;
				Dragonslayer_Hard_Leather_Pants.Bonus10 = 0;
				Dragonslayer_Hard_Leather_Pants.Bonus10Type = (int) 0;
				Dragonslayer_Hard_Leather_Pants.ExtraBonus = 0;
				Dragonslayer_Hard_Leather_Pants.ExtraBonusType = (int) 0;
				Dragonslayer_Hard_Leather_Pants.Effect = 0;
				Dragonslayer_Hard_Leather_Pants.Emblem = 0;
				Dragonslayer_Hard_Leather_Pants.Charges = 0;
				Dragonslayer_Hard_Leather_Pants.MaxCharges = 0;
				Dragonslayer_Hard_Leather_Pants.SpellID = 0;
				Dragonslayer_Hard_Leather_Pants.ProcSpellID = 31001;
				Dragonslayer_Hard_Leather_Pants.Type_Damage = 0;
				Dragonslayer_Hard_Leather_Pants.Realm = 0;
				Dragonslayer_Hard_Leather_Pants.MaxCount = 1;
				Dragonslayer_Hard_Leather_Pants.PackSize = 1;
				Dragonslayer_Hard_Leather_Pants.Extension = 5;
				Dragonslayer_Hard_Leather_Pants.Quality = 100;				
				Dragonslayer_Hard_Leather_Pants.Condition = 50000;
				Dragonslayer_Hard_Leather_Pants.MaxCondition = 50000;
				Dragonslayer_Hard_Leather_Pants.Durability = 50000;
				Dragonslayer_Hard_Leather_Pants.MaxDurability = 0;
				Dragonslayer_Hard_Leather_Pants.PoisonCharges = 0;
				Dragonslayer_Hard_Leather_Pants.PoisonMaxCharges = 0;
				Dragonslayer_Hard_Leather_Pants.PoisonSpellID = 0;
				Dragonslayer_Hard_Leather_Pants.ProcSpellID1 = 40006;
				Dragonslayer_Hard_Leather_Pants.SpellID1 = 0;
				Dragonslayer_Hard_Leather_Pants.MaxCharges1 = 0;
				Dragonslayer_Hard_Leather_Pants.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Hard_Leather_Pants);
				}
			Dragonslayer_Hard_Leather_Gloves = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Hard_Leather_Gloves");
			if (Dragonslayer_Hard_Leather_Gloves == null)
			{
				Dragonslayer_Hard_Leather_Gloves = new ItemTemplate();
				Dragonslayer_Hard_Leather_Gloves.Name = "Dragonslayer Hard Leather Gloves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Hard_Leather_Gloves.Name + ", creating it ...");
				Dragonslayer_Hard_Leather_Gloves.Level = 51;
				Dragonslayer_Hard_Leather_Gloves.Weight = 22;
				Dragonslayer_Hard_Leather_Gloves.Model = 3993;
				Dragonslayer_Hard_Leather_Gloves.Object_Type = 33;
				Dragonslayer_Hard_Leather_Gloves.Item_Type = 22;
				Dragonslayer_Hard_Leather_Gloves.Id_nb = "Dragonslayer_Hard_Leather_Gloves";
				Dragonslayer_Hard_Leather_Gloves.Hand = 0;
				Dragonslayer_Hard_Leather_Gloves.IsPickable = true;
				Dragonslayer_Hard_Leather_Gloves.IsDropable = true;
				Dragonslayer_Hard_Leather_Gloves.IsTradable = true;
				Dragonslayer_Hard_Leather_Gloves.CanDropAsLoot = true;
				Dragonslayer_Hard_Leather_Gloves.Color = 0;
				Dragonslayer_Hard_Leather_Gloves.Bonus = 35; // default bonus				
				Dragonslayer_Hard_Leather_Gloves.Bonus1 = 5;
				Dragonslayer_Hard_Leather_Gloves.Bonus1Type = (int) 12;
				Dragonslayer_Hard_Leather_Gloves.Bonus2 = 5;
				Dragonslayer_Hard_Leather_Gloves.Bonus2Type = (int) 18;
				Dragonslayer_Hard_Leather_Gloves.Bonus3 = 45;
				Dragonslayer_Hard_Leather_Gloves.Bonus3Type = (int) 10;
				Dragonslayer_Hard_Leather_Gloves.Bonus4 = 5;
				Dragonslayer_Hard_Leather_Gloves.Bonus4Type = (int) 15;
				Dragonslayer_Hard_Leather_Gloves.Bonus5 = 3;
				Dragonslayer_Hard_Leather_Gloves.Bonus5Type = (int) 164;
				Dragonslayer_Hard_Leather_Gloves.Bonus6 = 40;
				Dragonslayer_Hard_Leather_Gloves.Bonus6Type = (int) 210;
				Dragonslayer_Hard_Leather_Gloves.Bonus7 = 10;
				Dragonslayer_Hard_Leather_Gloves.Bonus7Type = (int) 148;
				Dragonslayer_Hard_Leather_Gloves.Bonus8 = 0;
				Dragonslayer_Hard_Leather_Gloves.Bonus8Type = (int) 0;
				Dragonslayer_Hard_Leather_Gloves.Bonus9 = 0;
				Dragonslayer_Hard_Leather_Gloves.Bonus9Type = (int) 0;
				Dragonslayer_Hard_Leather_Gloves.Bonus10 = 0;
				Dragonslayer_Hard_Leather_Gloves.Bonus10Type = (int) 0;
				Dragonslayer_Hard_Leather_Gloves.ExtraBonus = 0;
				Dragonslayer_Hard_Leather_Gloves.ExtraBonusType = (int) 0;
				Dragonslayer_Hard_Leather_Gloves.Effect = 0;
				Dragonslayer_Hard_Leather_Gloves.Emblem = 0;
				Dragonslayer_Hard_Leather_Gloves.Charges = 0;
				Dragonslayer_Hard_Leather_Gloves.MaxCharges = 0;
				Dragonslayer_Hard_Leather_Gloves.SpellID = 0;
				Dragonslayer_Hard_Leather_Gloves.ProcSpellID = 31001;
				Dragonslayer_Hard_Leather_Gloves.Type_Damage = 0;
				Dragonslayer_Hard_Leather_Gloves.Realm = 0;
				Dragonslayer_Hard_Leather_Gloves.MaxCount = 1;
				Dragonslayer_Hard_Leather_Gloves.PackSize = 1;
				Dragonslayer_Hard_Leather_Gloves.Extension = 5;
				Dragonslayer_Hard_Leather_Gloves.Quality = 100;				
				Dragonslayer_Hard_Leather_Gloves.Condition = 50000;
				Dragonslayer_Hard_Leather_Gloves.MaxCondition = 50000;
				Dragonslayer_Hard_Leather_Gloves.Durability = 50000;
				Dragonslayer_Hard_Leather_Gloves.MaxDurability = 0;
				Dragonslayer_Hard_Leather_Gloves.PoisonCharges = 0;
				Dragonslayer_Hard_Leather_Gloves.PoisonMaxCharges = 0;
				Dragonslayer_Hard_Leather_Gloves.PoisonSpellID = 0;
				Dragonslayer_Hard_Leather_Gloves.ProcSpellID1 = 40006;
				Dragonslayer_Hard_Leather_Gloves.SpellID1 = 0;
				Dragonslayer_Hard_Leather_Gloves.MaxCharges1 = 0;
				Dragonslayer_Hard_Leather_Gloves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Hard_Leather_Gloves);
				}


            #endregion

            #region defineAreas

            #endregion

            #region defineQuestParts

            QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerFriarQuest));
            QuestBehaviour a;
            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.Interact, null, Elia);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerFriarQuest), Elia);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerFriarQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "I have lived in this village since I was a young girl. My father was a great dragon hunter you know.", Elia);
            a.AddAction(eActionType.Talk, "In my years I have learned to work with all kinds of problems, specially with [Dragons].", Elia);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.Whisper, "Dragons", Elia);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerFriarQuest), Elia);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerFriarQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "yes dragons, but there is a problem for me, i can't hunt dragons by self, [you know].", Elia);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.Whisper, "you know", Elia);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerFriarQuest), Elia);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerFriarQuest), null, (eComparator)5);
            a.AddAction(eActionType.OfferQuest, typeof(DragonslayerFriarQuest), "Helen has offered you the Dragonslayer quest.?Do you accept?");
            AddBehaviour(a);

            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.DeclineQuest, null, typeof(DragonslayerFriarQuest));
            a.AddAction(eActionType.Talk, "No problem. See you", Elia);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.AcceptQuest, null, typeof(DragonslayerFriarQuest));
            a.AddAction(eActionType.Talk, "If you bring me the head of Runicaath I shall pay you well!", Elia);
            a.AddAction(eActionType.GiveQuest, typeof(DragonslayerFriarQuest), Elia);
            AddBehaviour(a);

           						
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End
			if (Elia!=null) {
				Elia.AddQuestToGive(typeof (DragonslayerFriarQuest));
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
			Elia.RemoveQuestToGive(typeof (DragonslayerFriarQuest));
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
			if (player.IsDoingQuest(typeof (DragonslayerFriarQuest)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
			if (
		
			player.CharacterClass.ID != (byte) eCharacterClass.Friar && 
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
