	
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
	public class DragonslayerScoutQuest : BaseQuest
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

		protected const string questTitle = "DragonslayerScoutQuest";

		protected const int minimumLevel = 49;
		protected const int maximumLevel = 50;
	
	
		private static GameNPC Elia = null;
		
		private static GameNPC Runicaath = null;
		
		private static ItemTemplate Runicaath_Head = null;
		
		private static ItemTemplate Dragonslayer_Lamellar_Jerkin = null;
		
		private static ItemTemplate Dragonslayer_Lamellar_Leggings = null;
		
		private static ItemTemplate Dragonslayer_Lamellar_Boots = null;
		
		private static ItemTemplate Dragonslayer_Lamellar_Gauntlets = null;
		
		private static ItemTemplate Dragonslayer_Lamellar_Full_Helm = null;
		
		private static ItemTemplate Dragonslayer_Lamellar_Armguards = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public DragonslayerScoutQuest() : base()
		{
		}

		public DragonslayerScoutQuest(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public DragonslayerScoutQuest(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public DragonslayerScoutQuest(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
	{
	}
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			QuestBehaviour a;
			QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerScoutQuest));

			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(DragonslayerScoutQuest)) == null)
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
						a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerScoutQuest), null);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Lamellar_Jerkin, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Lamellar_Leggings, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Lamellar_Boots, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Lamellar_Gauntlets, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Lamellar_Full_Helm, Elia);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Lamellar_Armguards, Elia);
						a.AddAction(eActionType.FinishQuest, typeof(DragonslayerScoutQuest), null);
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
			Dragonslayer_Lamellar_Jerkin = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Lamellar_Jerkin");
			if (Dragonslayer_Lamellar_Jerkin == null)
			{
				Dragonslayer_Lamellar_Jerkin = new ItemTemplate();
				Dragonslayer_Lamellar_Jerkin.Name = "Dragonslayer Lamellar Jerkin";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Lamellar_Jerkin.Name + ", creating it ...");
				Dragonslayer_Lamellar_Jerkin.Level = 51;
				Dragonslayer_Lamellar_Jerkin.Weight = 22;
				Dragonslayer_Lamellar_Jerkin.Model = 4010;
				Dragonslayer_Lamellar_Jerkin.Object_Type = 34;
				Dragonslayer_Lamellar_Jerkin.Item_Type = 25;
				Dragonslayer_Lamellar_Jerkin.Id_nb = "Dragonslayer_Lamellar_Jerkin";
				Dragonslayer_Lamellar_Jerkin.Hand = 0;
				Dragonslayer_Lamellar_Jerkin.IsPickable = true;
				Dragonslayer_Lamellar_Jerkin.IsDropable = true;
				Dragonslayer_Lamellar_Jerkin.IsTradable = true;
				Dragonslayer_Lamellar_Jerkin.CanDropAsLoot = true;
				Dragonslayer_Lamellar_Jerkin.Color = 0;
				Dragonslayer_Lamellar_Jerkin.Bonus = 35; // default bonus				
				Dragonslayer_Lamellar_Jerkin.Bonus1 = 22;
				Dragonslayer_Lamellar_Jerkin.Bonus1Type = (int) 1;
				Dragonslayer_Lamellar_Jerkin.Bonus2 = 22;
				Dragonslayer_Lamellar_Jerkin.Bonus2Type = (int) 2;
				Dragonslayer_Lamellar_Jerkin.Bonus3 = 5;
				Dragonslayer_Lamellar_Jerkin.Bonus3Type = (int) 12;
				Dragonslayer_Lamellar_Jerkin.Bonus4 = 5;
				Dragonslayer_Lamellar_Jerkin.Bonus4Type = (int) 18;
				Dragonslayer_Lamellar_Jerkin.Bonus5 = 5;
				Dragonslayer_Lamellar_Jerkin.Bonus5Type = (int) 15;
				Dragonslayer_Lamellar_Jerkin.Bonus6 = 5;
				Dragonslayer_Lamellar_Jerkin.Bonus6Type = (int) 202;
				Dragonslayer_Lamellar_Jerkin.Bonus7 = 5;
				Dragonslayer_Lamellar_Jerkin.Bonus7Type = (int) 201;
				Dragonslayer_Lamellar_Jerkin.Bonus8 = 2;
				Dragonslayer_Lamellar_Jerkin.Bonus8Type = (int) 191;
				Dragonslayer_Lamellar_Jerkin.Bonus9 = 0;
				Dragonslayer_Lamellar_Jerkin.Bonus9Type = (int) 0;
				Dragonslayer_Lamellar_Jerkin.Bonus10 = 0;
				Dragonslayer_Lamellar_Jerkin.Bonus10Type = (int) 0;
				Dragonslayer_Lamellar_Jerkin.ExtraBonus = 0;
				Dragonslayer_Lamellar_Jerkin.ExtraBonusType = (int) 0;
				Dragonslayer_Lamellar_Jerkin.Effect = 0;
				Dragonslayer_Lamellar_Jerkin.Emblem = 0;
				Dragonslayer_Lamellar_Jerkin.Charges = 0;
				Dragonslayer_Lamellar_Jerkin.MaxCharges = 0;
				Dragonslayer_Lamellar_Jerkin.SpellID = 0;
				Dragonslayer_Lamellar_Jerkin.ProcSpellID = 31001;
				Dragonslayer_Lamellar_Jerkin.Type_Damage = 0;
				Dragonslayer_Lamellar_Jerkin.Realm = 0;
				Dragonslayer_Lamellar_Jerkin.MaxCount = 1;
				Dragonslayer_Lamellar_Jerkin.PackSize = 1;
				Dragonslayer_Lamellar_Jerkin.Extension = 5;
				Dragonslayer_Lamellar_Jerkin.Quality = 100;				
				Dragonslayer_Lamellar_Jerkin.Condition = 50000;
				Dragonslayer_Lamellar_Jerkin.MaxCondition = 50000;
				Dragonslayer_Lamellar_Jerkin.Durability = 50000;
				Dragonslayer_Lamellar_Jerkin.MaxDurability = 0;
				Dragonslayer_Lamellar_Jerkin.PoisonCharges = 0;
				Dragonslayer_Lamellar_Jerkin.PoisonMaxCharges = 0;
				Dragonslayer_Lamellar_Jerkin.PoisonSpellID = 0;
				Dragonslayer_Lamellar_Jerkin.ProcSpellID1 = 40006;
				Dragonslayer_Lamellar_Jerkin.SpellID1 = 0;
				Dragonslayer_Lamellar_Jerkin.MaxCharges1 = 0;
				Dragonslayer_Lamellar_Jerkin.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Lamellar_Jerkin);
				}
			Dragonslayer_Lamellar_Leggings = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Lamellar_Leggings");
			if (Dragonslayer_Lamellar_Leggings == null)
			{
				Dragonslayer_Lamellar_Leggings = new ItemTemplate();
				Dragonslayer_Lamellar_Leggings.Name = "Dragonslayer Lamellar Leggings";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Lamellar_Leggings.Name + ", creating it ...");
				Dragonslayer_Lamellar_Leggings.Level = 51;
				Dragonslayer_Lamellar_Leggings.Weight = 22;
				Dragonslayer_Lamellar_Leggings.Model = 4011;
				Dragonslayer_Lamellar_Leggings.Object_Type = 34;
				Dragonslayer_Lamellar_Leggings.Item_Type = 27;
				Dragonslayer_Lamellar_Leggings.Id_nb = "Dragonslayer_Lamellar_Leggings";
				Dragonslayer_Lamellar_Leggings.Hand = 0;
				Dragonslayer_Lamellar_Leggings.IsPickable = true;
				Dragonslayer_Lamellar_Leggings.IsDropable = true;
				Dragonslayer_Lamellar_Leggings.IsTradable = true;
				Dragonslayer_Lamellar_Leggings.CanDropAsLoot = true;
				Dragonslayer_Lamellar_Leggings.Color = 0;
				Dragonslayer_Lamellar_Leggings.Bonus = 35; // default bonus				
				Dragonslayer_Lamellar_Leggings.Bonus1 = 22;
				Dragonslayer_Lamellar_Leggings.Bonus1Type = (int) 2;
				Dragonslayer_Lamellar_Leggings.Bonus2 = 5;
				Dragonslayer_Lamellar_Leggings.Bonus2Type = (int) 12;
				Dragonslayer_Lamellar_Leggings.Bonus3 = 5;
				Dragonslayer_Lamellar_Leggings.Bonus3Type = (int) 18;
				Dragonslayer_Lamellar_Leggings.Bonus4 = 40;
				Dragonslayer_Lamellar_Leggings.Bonus4Type = (int) 10;
				Dragonslayer_Lamellar_Leggings.Bonus5 = 5;
				Dragonslayer_Lamellar_Leggings.Bonus5Type = (int) 15;
				Dragonslayer_Lamellar_Leggings.Bonus6 = 5;
				Dragonslayer_Lamellar_Leggings.Bonus6Type = (int) 202;
				Dragonslayer_Lamellar_Leggings.Bonus7 = 1;
				Dragonslayer_Lamellar_Leggings.Bonus7Type = (int) 191;
				Dragonslayer_Lamellar_Leggings.Bonus8 = 0;
				Dragonslayer_Lamellar_Leggings.Bonus8Type = (int) 0;
				Dragonslayer_Lamellar_Leggings.Bonus9 = 0;
				Dragonslayer_Lamellar_Leggings.Bonus9Type = (int) 0;
				Dragonslayer_Lamellar_Leggings.Bonus10 = 0;
				Dragonslayer_Lamellar_Leggings.Bonus10Type = (int) 0;
				Dragonslayer_Lamellar_Leggings.ExtraBonus = 0;
				Dragonslayer_Lamellar_Leggings.ExtraBonusType = (int) 0;
				Dragonslayer_Lamellar_Leggings.Effect = 0;
				Dragonslayer_Lamellar_Leggings.Emblem = 0;
				Dragonslayer_Lamellar_Leggings.Charges = 0;
				Dragonslayer_Lamellar_Leggings.MaxCharges = 0;
				Dragonslayer_Lamellar_Leggings.SpellID = 0;
				Dragonslayer_Lamellar_Leggings.ProcSpellID = 31001;
				Dragonslayer_Lamellar_Leggings.Type_Damage = 0;
				Dragonslayer_Lamellar_Leggings.Realm = 0;
				Dragonslayer_Lamellar_Leggings.MaxCount = 1;
				Dragonslayer_Lamellar_Leggings.PackSize = 1;
				Dragonslayer_Lamellar_Leggings.Extension = 5;
				Dragonslayer_Lamellar_Leggings.Quality = 100;				
				Dragonslayer_Lamellar_Leggings.Condition = 50000;
				Dragonslayer_Lamellar_Leggings.MaxCondition = 50000;
				Dragonslayer_Lamellar_Leggings.Durability = 50000;
				Dragonslayer_Lamellar_Leggings.MaxDurability = 0;
				Dragonslayer_Lamellar_Leggings.PoisonCharges = 0;
				Dragonslayer_Lamellar_Leggings.PoisonMaxCharges = 0;
				Dragonslayer_Lamellar_Leggings.PoisonSpellID = 0;
				Dragonslayer_Lamellar_Leggings.ProcSpellID1 = 40006;
				Dragonslayer_Lamellar_Leggings.SpellID1 = 0;
				Dragonslayer_Lamellar_Leggings.MaxCharges1 = 0;
				Dragonslayer_Lamellar_Leggings.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Lamellar_Leggings);
				}
			Dragonslayer_Lamellar_Boots = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Lamellar_Boots");
			if (Dragonslayer_Lamellar_Boots == null)
			{
				Dragonslayer_Lamellar_Boots = new ItemTemplate();
				Dragonslayer_Lamellar_Boots.Name = "Dragonslayer Lamellar Boots";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Lamellar_Boots.Name + ", creating it ...");
				Dragonslayer_Lamellar_Boots.Level = 51;
				Dragonslayer_Lamellar_Boots.Weight = 22;
				Dragonslayer_Lamellar_Boots.Model = 4013;
				Dragonslayer_Lamellar_Boots.Object_Type = 34;
				Dragonslayer_Lamellar_Boots.Item_Type = 23;
				Dragonslayer_Lamellar_Boots.Id_nb = "Dragonslayer_Lamellar_Boots";
				Dragonslayer_Lamellar_Boots.Hand = 0;
				Dragonslayer_Lamellar_Boots.IsPickable = true;
				Dragonslayer_Lamellar_Boots.IsDropable = true;
				Dragonslayer_Lamellar_Boots.IsTradable = true;
				Dragonslayer_Lamellar_Boots.CanDropAsLoot = true;
				Dragonslayer_Lamellar_Boots.Color = 0;
				Dragonslayer_Lamellar_Boots.Bonus = 35; // default bonus				
				Dragonslayer_Lamellar_Boots.Bonus1 = 4;
				Dragonslayer_Lamellar_Boots.Bonus1Type = (int) 49;
				Dragonslayer_Lamellar_Boots.Bonus2 = 6;
				Dragonslayer_Lamellar_Boots.Bonus2Type = (int) 13;
				Dragonslayer_Lamellar_Boots.Bonus3 = 6;
				Dragonslayer_Lamellar_Boots.Bonus3Type = (int) 17;
				Dragonslayer_Lamellar_Boots.Bonus4 = 6;
				Dragonslayer_Lamellar_Boots.Bonus4Type = (int) 19;
				Dragonslayer_Lamellar_Boots.Bonus5 = 20;
				Dragonslayer_Lamellar_Boots.Bonus5Type = (int) 10;
				Dragonslayer_Lamellar_Boots.Bonus6 = 4;
				Dragonslayer_Lamellar_Boots.Bonus6Type = (int) 168;
				Dragonslayer_Lamellar_Boots.Bonus7 = 0;
				Dragonslayer_Lamellar_Boots.Bonus7Type = (int) 0;
				Dragonslayer_Lamellar_Boots.Bonus8 = 0;
				Dragonslayer_Lamellar_Boots.Bonus8Type = (int) 0;
				Dragonslayer_Lamellar_Boots.Bonus9 = 0;
				Dragonslayer_Lamellar_Boots.Bonus9Type = (int) 0;
				Dragonslayer_Lamellar_Boots.Bonus10 = 0;
				Dragonslayer_Lamellar_Boots.Bonus10Type = (int) 0;
				Dragonslayer_Lamellar_Boots.ExtraBonus = 0;
				Dragonslayer_Lamellar_Boots.ExtraBonusType = (int) 0;
				Dragonslayer_Lamellar_Boots.Effect = 0;
				Dragonslayer_Lamellar_Boots.Emblem = 0;
				Dragonslayer_Lamellar_Boots.Charges = 0;
				Dragonslayer_Lamellar_Boots.MaxCharges = 0;
				Dragonslayer_Lamellar_Boots.SpellID = 0;
				Dragonslayer_Lamellar_Boots.ProcSpellID = 31001;
				Dragonslayer_Lamellar_Boots.Type_Damage = 0;
				Dragonslayer_Lamellar_Boots.Realm = 0;
				Dragonslayer_Lamellar_Boots.MaxCount = 1;
				Dragonslayer_Lamellar_Boots.PackSize = 1;
				Dragonslayer_Lamellar_Boots.Extension = 5;
				Dragonslayer_Lamellar_Boots.Quality = 100;				
				Dragonslayer_Lamellar_Boots.Condition = 50000;
				Dragonslayer_Lamellar_Boots.MaxCondition = 50000;
				Dragonslayer_Lamellar_Boots.Durability = 50000;
				Dragonslayer_Lamellar_Boots.MaxDurability = 0;
				Dragonslayer_Lamellar_Boots.PoisonCharges = 0;
				Dragonslayer_Lamellar_Boots.PoisonMaxCharges = 0;
				Dragonslayer_Lamellar_Boots.PoisonSpellID = 0;
				Dragonslayer_Lamellar_Boots.ProcSpellID1 = 40006;
				Dragonslayer_Lamellar_Boots.SpellID1 = 0;
				Dragonslayer_Lamellar_Boots.MaxCharges1 = 0;
				Dragonslayer_Lamellar_Boots.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Lamellar_Boots);
				}
			Dragonslayer_Lamellar_Gauntlets = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Lamellar_Gauntlets");
			if (Dragonslayer_Lamellar_Gauntlets == null)
			{
				Dragonslayer_Lamellar_Gauntlets = new ItemTemplate();
				Dragonslayer_Lamellar_Gauntlets.Name = "Dragonslayer Lamellar Gauntlets";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Lamellar_Gauntlets.Name + ", creating it ...");
				Dragonslayer_Lamellar_Gauntlets.Level = 51;
				Dragonslayer_Lamellar_Gauntlets.Weight = 22;
				Dragonslayer_Lamellar_Gauntlets.Model = 4014;
				Dragonslayer_Lamellar_Gauntlets.Object_Type = 34;
				Dragonslayer_Lamellar_Gauntlets.Item_Type = 22;
				Dragonslayer_Lamellar_Gauntlets.Id_nb = "Dragonslayer_Lamellar_Gauntlets";
				Dragonslayer_Lamellar_Gauntlets.Hand = 0;
				Dragonslayer_Lamellar_Gauntlets.IsPickable = true;
				Dragonslayer_Lamellar_Gauntlets.IsDropable = true;
				Dragonslayer_Lamellar_Gauntlets.IsTradable = true;
				Dragonslayer_Lamellar_Gauntlets.CanDropAsLoot = true;
				Dragonslayer_Lamellar_Gauntlets.Color = 0;
				Dragonslayer_Lamellar_Gauntlets.Bonus = 35; // default bonus				
				Dragonslayer_Lamellar_Gauntlets.Bonus1 = 5;
				Dragonslayer_Lamellar_Gauntlets.Bonus1Type = (int) 12;
				Dragonslayer_Lamellar_Gauntlets.Bonus2 = 5;
				Dragonslayer_Lamellar_Gauntlets.Bonus2Type = (int) 18;
				Dragonslayer_Lamellar_Gauntlets.Bonus3 = 45;
				Dragonslayer_Lamellar_Gauntlets.Bonus3Type = (int) 10;
				Dragonslayer_Lamellar_Gauntlets.Bonus4 = 5;
				Dragonslayer_Lamellar_Gauntlets.Bonus4Type = (int) 15;
				Dragonslayer_Lamellar_Gauntlets.Bonus5 = 3;
				Dragonslayer_Lamellar_Gauntlets.Bonus5Type = (int) 168;
				Dragonslayer_Lamellar_Gauntlets.Bonus6 = 45;
				Dragonslayer_Lamellar_Gauntlets.Bonus6Type = (int) 210;
				Dragonslayer_Lamellar_Gauntlets.Bonus7 = 10;
				Dragonslayer_Lamellar_Gauntlets.Bonus7Type = (int) 148;
				Dragonslayer_Lamellar_Gauntlets.Bonus8 = 0;
				Dragonslayer_Lamellar_Gauntlets.Bonus8Type = (int) 0;
				Dragonslayer_Lamellar_Gauntlets.Bonus9 = 0;
				Dragonslayer_Lamellar_Gauntlets.Bonus9Type = (int) 0;
				Dragonslayer_Lamellar_Gauntlets.Bonus10 = 0;
				Dragonslayer_Lamellar_Gauntlets.Bonus10Type = (int) 0;
				Dragonslayer_Lamellar_Gauntlets.ExtraBonus = 0;
				Dragonslayer_Lamellar_Gauntlets.ExtraBonusType = (int) 0;
				Dragonslayer_Lamellar_Gauntlets.Effect = 0;
				Dragonslayer_Lamellar_Gauntlets.Emblem = 0;
				Dragonslayer_Lamellar_Gauntlets.Charges = 0;
				Dragonslayer_Lamellar_Gauntlets.MaxCharges = 0;
				Dragonslayer_Lamellar_Gauntlets.SpellID = 0;
				Dragonslayer_Lamellar_Gauntlets.ProcSpellID = 31001;
				Dragonslayer_Lamellar_Gauntlets.Type_Damage = 0;
				Dragonslayer_Lamellar_Gauntlets.Realm = 0;
				Dragonslayer_Lamellar_Gauntlets.MaxCount = 1;
				Dragonslayer_Lamellar_Gauntlets.PackSize = 1;
				Dragonslayer_Lamellar_Gauntlets.Extension = 5;
				Dragonslayer_Lamellar_Gauntlets.Quality = 100;				
				Dragonslayer_Lamellar_Gauntlets.Condition = 50000;
				Dragonslayer_Lamellar_Gauntlets.MaxCondition = 50000;
				Dragonslayer_Lamellar_Gauntlets.Durability = 50000;
				Dragonslayer_Lamellar_Gauntlets.MaxDurability = 0;
				Dragonslayer_Lamellar_Gauntlets.PoisonCharges = 0;
				Dragonslayer_Lamellar_Gauntlets.PoisonMaxCharges = 0;
				Dragonslayer_Lamellar_Gauntlets.PoisonSpellID = 0;
				Dragonslayer_Lamellar_Gauntlets.ProcSpellID1 = 40006;
				Dragonslayer_Lamellar_Gauntlets.SpellID1 = 0;
				Dragonslayer_Lamellar_Gauntlets.MaxCharges1 = 0;
				Dragonslayer_Lamellar_Gauntlets.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Lamellar_Gauntlets);
				}
			Dragonslayer_Lamellar_Full_Helm = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Lamellar_Full_Helm");
			if (Dragonslayer_Lamellar_Full_Helm == null)
			{
				Dragonslayer_Lamellar_Full_Helm = new ItemTemplate();
				Dragonslayer_Lamellar_Full_Helm.Name = "Dragonslayer Lamellar Full Helm";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Lamellar_Full_Helm.Name + ", creating it ...");
				Dragonslayer_Lamellar_Full_Helm.Level = 51;
				Dragonslayer_Lamellar_Full_Helm.Weight = 22;
				Dragonslayer_Lamellar_Full_Helm.Model = 4055;
				Dragonslayer_Lamellar_Full_Helm.Object_Type = 34;
				Dragonslayer_Lamellar_Full_Helm.Item_Type = 21;
				Dragonslayer_Lamellar_Full_Helm.Id_nb = "Dragonslayer_Lamellar_Full_Helm";
				Dragonslayer_Lamellar_Full_Helm.Hand = 0;
				Dragonslayer_Lamellar_Full_Helm.IsPickable = true;
				Dragonslayer_Lamellar_Full_Helm.IsDropable = true;
				Dragonslayer_Lamellar_Full_Helm.IsTradable = true;
				Dragonslayer_Lamellar_Full_Helm.CanDropAsLoot = true;
				Dragonslayer_Lamellar_Full_Helm.Color = 0;
				Dragonslayer_Lamellar_Full_Helm.Bonus = 35; // default bonus				
				Dragonslayer_Lamellar_Full_Helm.Bonus1 = 22;
				Dragonslayer_Lamellar_Full_Helm.Bonus1Type = (int) 3;
				Dragonslayer_Lamellar_Full_Helm.Bonus2 = 6;
				Dragonslayer_Lamellar_Full_Helm.Bonus2Type = (int) 16;
				Dragonslayer_Lamellar_Full_Helm.Bonus3 = 6;
				Dragonslayer_Lamellar_Full_Helm.Bonus3Type = (int) 11;
				Dragonslayer_Lamellar_Full_Helm.Bonus4 = 40;
				Dragonslayer_Lamellar_Full_Helm.Bonus4Type = (int) 10;
				Dragonslayer_Lamellar_Full_Helm.Bonus5 = 6;
				Dragonslayer_Lamellar_Full_Helm.Bonus5Type = (int) 14;
				Dragonslayer_Lamellar_Full_Helm.Bonus6 = 40;
				Dragonslayer_Lamellar_Full_Helm.Bonus6Type = (int) 210;
				Dragonslayer_Lamellar_Full_Helm.Bonus7 = 5;
				Dragonslayer_Lamellar_Full_Helm.Bonus7Type = (int) 148;
				Dragonslayer_Lamellar_Full_Helm.Bonus8 = 0;
				Dragonslayer_Lamellar_Full_Helm.Bonus8Type = (int) 0;
				Dragonslayer_Lamellar_Full_Helm.Bonus9 = 0;
				Dragonslayer_Lamellar_Full_Helm.Bonus9Type = (int) 0;
				Dragonslayer_Lamellar_Full_Helm.Bonus10 = 0;
				Dragonslayer_Lamellar_Full_Helm.Bonus10Type = (int) 0;
				Dragonslayer_Lamellar_Full_Helm.ExtraBonus = 0;
				Dragonslayer_Lamellar_Full_Helm.ExtraBonusType = (int) 0;
				Dragonslayer_Lamellar_Full_Helm.Effect = 0;
				Dragonslayer_Lamellar_Full_Helm.Emblem = 0;
				Dragonslayer_Lamellar_Full_Helm.Charges = 0;
				Dragonslayer_Lamellar_Full_Helm.MaxCharges = 0;
				Dragonslayer_Lamellar_Full_Helm.SpellID = 0;
				Dragonslayer_Lamellar_Full_Helm.ProcSpellID = 31001;
				Dragonslayer_Lamellar_Full_Helm.Type_Damage = 0;
				Dragonslayer_Lamellar_Full_Helm.Realm = 0;
				Dragonslayer_Lamellar_Full_Helm.MaxCount = 1;
				Dragonslayer_Lamellar_Full_Helm.PackSize = 1;
				Dragonslayer_Lamellar_Full_Helm.Extension = 0;
				Dragonslayer_Lamellar_Full_Helm.Quality = 100;				
				Dragonslayer_Lamellar_Full_Helm.Condition = 50000;
				Dragonslayer_Lamellar_Full_Helm.MaxCondition = 50000;
				Dragonslayer_Lamellar_Full_Helm.Durability = 50000;
				Dragonslayer_Lamellar_Full_Helm.MaxDurability = 0;
				Dragonslayer_Lamellar_Full_Helm.PoisonCharges = 0;
				Dragonslayer_Lamellar_Full_Helm.PoisonMaxCharges = 0;
				Dragonslayer_Lamellar_Full_Helm.PoisonSpellID = 0;
				Dragonslayer_Lamellar_Full_Helm.ProcSpellID1 = 40006;
				Dragonslayer_Lamellar_Full_Helm.SpellID1 = 0;
				Dragonslayer_Lamellar_Full_Helm.MaxCharges1 = 0;
				Dragonslayer_Lamellar_Full_Helm.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Lamellar_Full_Helm);
				}
			Dragonslayer_Lamellar_Armguards = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Lamellar_Armguards");
			if (Dragonslayer_Lamellar_Armguards == null)
			{
				Dragonslayer_Lamellar_Armguards = new ItemTemplate();
				Dragonslayer_Lamellar_Armguards.Name = "Dragonslayer Lamellar Armguards";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Lamellar_Armguards.Name + ", creating it ...");
				Dragonslayer_Lamellar_Armguards.Level = 51;
				Dragonslayer_Lamellar_Armguards.Weight = 22;
				Dragonslayer_Lamellar_Armguards.Model = 4012;
				Dragonslayer_Lamellar_Armguards.Object_Type = 34;
				Dragonslayer_Lamellar_Armguards.Item_Type = 28;
				Dragonslayer_Lamellar_Armguards.Id_nb = "Dragonslayer_Lamellar_Armguards";
				Dragonslayer_Lamellar_Armguards.Hand = 0;
				Dragonslayer_Lamellar_Armguards.IsPickable = true;
				Dragonslayer_Lamellar_Armguards.IsDropable = true;
				Dragonslayer_Lamellar_Armguards.IsTradable = true;
				Dragonslayer_Lamellar_Armguards.CanDropAsLoot = true;
				Dragonslayer_Lamellar_Armguards.Color = 0;
				Dragonslayer_Lamellar_Armguards.Bonus = 35; // default bonus				
				Dragonslayer_Lamellar_Armguards.Bonus1 = 22;
				Dragonslayer_Lamellar_Armguards.Bonus1Type = (int) 2;
				Dragonslayer_Lamellar_Armguards.Bonus2 = 40;
				Dragonslayer_Lamellar_Armguards.Bonus2Type = (int) 10;
				Dragonslayer_Lamellar_Armguards.Bonus3 = 5;
				Dragonslayer_Lamellar_Armguards.Bonus3Type = (int) 12;
				Dragonslayer_Lamellar_Armguards.Bonus4 = 5;
				Dragonslayer_Lamellar_Armguards.Bonus4Type = (int) 15;
				Dragonslayer_Lamellar_Armguards.Bonus5 = 5;
				Dragonslayer_Lamellar_Armguards.Bonus5Type = (int) 18;
				Dragonslayer_Lamellar_Armguards.Bonus6 = 5;
				Dragonslayer_Lamellar_Armguards.Bonus6Type = (int) 202;
				Dragonslayer_Lamellar_Armguards.Bonus7 = 1;
				Dragonslayer_Lamellar_Armguards.Bonus7Type = (int) 191;
				Dragonslayer_Lamellar_Armguards.Bonus8 = 0;
				Dragonslayer_Lamellar_Armguards.Bonus8Type = (int) 0;
				Dragonslayer_Lamellar_Armguards.Bonus9 = 0;
				Dragonslayer_Lamellar_Armguards.Bonus9Type = (int) 0;
				Dragonslayer_Lamellar_Armguards.Bonus10 = 0;
				Dragonslayer_Lamellar_Armguards.Bonus10Type = (int) 0;
				Dragonslayer_Lamellar_Armguards.ExtraBonus = 0;
				Dragonslayer_Lamellar_Armguards.ExtraBonusType = (int) 0;
				Dragonslayer_Lamellar_Armguards.Effect = 0;
				Dragonslayer_Lamellar_Armguards.Emblem = 0;
				Dragonslayer_Lamellar_Armguards.Charges = 0;
				Dragonslayer_Lamellar_Armguards.MaxCharges = 0;
				Dragonslayer_Lamellar_Armguards.SpellID = 0;
				Dragonslayer_Lamellar_Armguards.ProcSpellID = 31001;
				Dragonslayer_Lamellar_Armguards.Type_Damage = 0;
				Dragonslayer_Lamellar_Armguards.Realm = 0;
				Dragonslayer_Lamellar_Armguards.MaxCount = 1;
				Dragonslayer_Lamellar_Armguards.PackSize = 1;
				Dragonslayer_Lamellar_Armguards.Extension = 5;
				Dragonslayer_Lamellar_Armguards.Quality = 100;				
				Dragonslayer_Lamellar_Armguards.Condition = 50000;
				Dragonslayer_Lamellar_Armguards.MaxCondition = 50000;
				Dragonslayer_Lamellar_Armguards.Durability = 50000;
				Dragonslayer_Lamellar_Armguards.MaxDurability = 0;
				Dragonslayer_Lamellar_Armguards.PoisonCharges = 0;
				Dragonslayer_Lamellar_Armguards.PoisonMaxCharges = 0;
				Dragonslayer_Lamellar_Armguards.PoisonSpellID = 0;
				Dragonslayer_Lamellar_Armguards.ProcSpellID1 = 40006;
				Dragonslayer_Lamellar_Armguards.SpellID1 = 0;
				Dragonslayer_Lamellar_Armguards.MaxCharges1 = 0;
				Dragonslayer_Lamellar_Armguards.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Lamellar_Armguards);
				}


            #endregion

            #region defineAreas

            #endregion

            #region defineQuestParts

            QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerScoutQuest));
            QuestBehaviour a;
            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.Interact, null, Elia);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerScoutQuest), Elia);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerScoutQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "I have lived in this village since I was a young girl. My father was a great dragon hunter you know.", Elia);
            a.AddAction(eActionType.Talk, "In my years I have learned to work with all kinds of problems, specially with [Dragons].", Elia);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.Whisper, "Dragons", Elia);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerScoutQuest), Elia);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerScoutQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "yes dragons, but there is a problem for me, i can't hunt dragons by self, [you know].", Elia);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.Whisper, "you know", Elia);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerScoutQuest), Elia);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerScoutQuest), null, (eComparator)5);
            a.AddAction(eActionType.OfferQuest, typeof(DragonslayerScoutQuest), "Helen has offered you the Dragonslayer quest.?Do you accept?");
            AddBehaviour(a);

            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.DeclineQuest, null, typeof(DragonslayerScoutQuest));
            a.AddAction(eActionType.Talk, "No problem. See you", Elia);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Elia, -1);
            a.AddTrigger(eTriggerType.AcceptQuest, null, typeof(DragonslayerScoutQuest));
            a.AddAction(eActionType.Talk, "If you bring me the head of Runicaath I shall pay you well!", Elia);
            a.AddAction(eActionType.GiveQuest, typeof(DragonslayerScoutQuest), Elia);
            AddBehaviour(a);


            						
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End
			if (Elia!=null) {
				Elia.AddQuestToGive(typeof (DragonslayerScoutQuest));
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
			Elia.RemoveQuestToGive(typeof (DragonslayerScoutQuest));
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
			if (player.IsDoingQuest(typeof (DragonslayerScoutQuest)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
			if (
		
			player.CharacterClass.ID != (byte) eCharacterClass.Scout && 
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
