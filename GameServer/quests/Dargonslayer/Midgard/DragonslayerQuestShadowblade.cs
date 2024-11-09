	
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
	public class DragonslayerShadowbladeQuest : BaseQuest
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

		protected const string questTitle = "DragonslayerShadowbladeQuest";

		protected const int minimumLevel = 49;
		protected const int maximumLevel = 50;
	
	
		private static GameNPC Jarkkonith = null;
		
		private static GameNPC AmarasavaDas = null;
		
		private static ItemTemplate Jarkkonith_Head = null;
		
		private static ItemTemplate Dragonslayer_Soft_Starklaedar_Shoes = null;
		
		private static ItemTemplate Dragonslayer_Hard_Starklaedar_Tunic = null;
		
		private static ItemTemplate Dragonslayer_Hard_Starklaedar_Gloves = null;
		
		private static ItemTemplate Dragonslayer_Starklaedar_Full_Helm = null;
		
		private static ItemTemplate Dragonslayer_Hard_Starklaedar_Pants = null;
		
		private static ItemTemplate Dragonslayer_Hard_Starklaedar_Sleeves = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public DragonslayerShadowbladeQuest() : base()
		{
		}

		public DragonslayerShadowbladeQuest(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public DragonslayerShadowbladeQuest(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public DragonslayerShadowbladeQuest(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
	{
	}
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			QuestBehaviour a;
			QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerShadowbladeQuest));

			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(DragonslayerShadowbladeQuest)) == null)
				return;


			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;

				if (gArgs.Target.Name.IndexOf("Jarkkonith") >= 0)
				{

					GiveItem(gArgs.Target, player, Jarkkonith_Head);
					Step = 2;
					return;
				}
			}


			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs)args;

				if (gArgs.Target.Name == AmarasavaDas.Name && gArgs.Item.Id_nb == Jarkkonith_Head.Id_nb)
				{

					if (player.Inventory.IsSlotsFree(6, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
					{

						a = builder.CreateBehaviour(AmarasavaDas, -1);
						a.AddTrigger(eTriggerType.GiveItem, AmarasavaDas, Jarkkonith_Head);
						a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerShadowbladeQuest), null);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Soft_Starklaedar_Shoes, AmarasavaDas);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Hard_Starklaedar_Tunic, AmarasavaDas);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Hard_Starklaedar_Gloves, AmarasavaDas);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Starklaedar_Full_Helm, AmarasavaDas);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Hard_Starklaedar_Pants, AmarasavaDas);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Hard_Starklaedar_Sleeves, AmarasavaDas);
						a.AddAction(eActionType.FinishQuest, typeof(DragonslayerShadowbladeQuest), null);
						a.AddAction(eActionType.DestroyItem, Jarkkonith_Head, null);
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
	
			npcs = WorldMgr.GetNPCsByName("Jarkkonith",eRealm.None);
			if (npcs.Length == 0)
			{
				if (!WorldMgr.GetRegion(100).IsDisabled)
				{
				Jarkkonith = new DOL.GS.GameNPC();
					Jarkkonith.Model = 2309;
				Jarkkonith.Name = "Jarkkonith";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Jarkkonith.Name + ", creating ...");
				Jarkkonith.GuildName = "Part of " + questTitle + " Quest";
				Jarkkonith.Realm = eRealm.None;
				Jarkkonith.CurrentRegionID = 100;
				Jarkkonith.Size = 200;
				Jarkkonith.Level = 65;
				Jarkkonith.MaxSpeedBase = 200;
				Jarkkonith.Faction = FactionMgr.GetFactionByID(0);
				Jarkkonith.X = 731471;
				Jarkkonith.Y = 983685;
				Jarkkonith.Z = 5104;
				Jarkkonith.Heading = 3720;
				Jarkkonith.RespawnInterval = 0;
				Jarkkonith.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 30;
				brain.AggroRange = 500;
				Jarkkonith.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					Jarkkonith.SaveIntoDatabase();
					
				Jarkkonith.AddToWorld();
				
				}
			}
			else 
			{
				Jarkkonith = npcs[0];
			}
		
			npcs = WorldMgr.GetNPCsByName("Amarasava Das",eRealm.Midgard);
			if (npcs.Length == 0)
			{
				if (!WorldMgr.GetRegion(100).IsDisabled)
				{
				AmarasavaDas = new DOL.GS.GameNPC();
					AmarasavaDas.Model = 200;
				AmarasavaDas.Name = "Amarasava Das";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + AmarasavaDas.Name + ", creating ...");
				AmarasavaDas.GuildName = "Part of " + questTitle + " Quest";
				AmarasavaDas.Realm = eRealm.Midgard;
				AmarasavaDas.CurrentRegionID = 100;
				AmarasavaDas.Size = 50;
				AmarasavaDas.Level = 65;
				AmarasavaDas.MaxSpeedBase = 200;
				AmarasavaDas.Faction = FactionMgr.GetFactionByID(0);
				AmarasavaDas.X = 742466;
				AmarasavaDas.Y = 978181;
				AmarasavaDas.Z = 3920;
				AmarasavaDas.Heading = 2938;
				AmarasavaDas.RespawnInterval = 0;
				AmarasavaDas.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				AmarasavaDas.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					AmarasavaDas.SaveIntoDatabase();
					
				AmarasavaDas.AddToWorld();
				
				}
			}
			else 
			{
				AmarasavaDas = npcs[0];
			}
		

			#endregion

			#region defineItems

		Jarkkonith_Head = GameServer.Database.FindObjectByKey<ItemTemplate>("Jarkkonith_Head");
			if (Jarkkonith_Head == null)
			{
				Jarkkonith_Head = new ItemTemplate();
				Jarkkonith_Head.Name = "Jarkkonith Head";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Jarkkonith_Head.Name + ", creating it ...");
				Jarkkonith_Head.Level = 1;
				Jarkkonith_Head.Weight = 1;
				Jarkkonith_Head.Model = 488;
				Jarkkonith_Head.Object_Type = 0;
				Jarkkonith_Head.Item_Type = 0;
				Jarkkonith_Head.Id_nb = "Jarkkonith_Head";
				Jarkkonith_Head.Hand = 0;
				Jarkkonith_Head.IsPickable = true;
				Jarkkonith_Head.IsDropable = true;
				Jarkkonith_Head.IsTradable = true;
				Jarkkonith_Head.CanDropAsLoot = true;
				Jarkkonith_Head.Color = 0;
				Jarkkonith_Head.Bonus = 0; // default bonus				
				Jarkkonith_Head.Bonus1 = 0;
				Jarkkonith_Head.Bonus1Type = (int) 0;
				Jarkkonith_Head.Bonus2 = 0;
				Jarkkonith_Head.Bonus2Type = (int) 0;
				Jarkkonith_Head.Bonus3 = 0;
				Jarkkonith_Head.Bonus3Type = (int) 208;
				Jarkkonith_Head.Bonus4 = 0;
				Jarkkonith_Head.Bonus4Type = (int) 0;
				Jarkkonith_Head.Bonus5 = 0;
				Jarkkonith_Head.Bonus5Type = (int) 0;
				Jarkkonith_Head.Bonus6 = 0;
				Jarkkonith_Head.Bonus6Type = (int) 0;
				Jarkkonith_Head.Bonus7 = 0;
				Jarkkonith_Head.Bonus7Type = (int) 0;
				Jarkkonith_Head.Bonus8 = 0;
				Jarkkonith_Head.Bonus8Type = (int) 0;
				Jarkkonith_Head.Bonus9 = 0;
				Jarkkonith_Head.Bonus9Type = (int) 0;
				Jarkkonith_Head.Bonus10 = 0;
				Jarkkonith_Head.Bonus10Type = (int) 0;
				Jarkkonith_Head.ExtraBonus = 0;
				Jarkkonith_Head.ExtraBonusType = (int) 0;
				Jarkkonith_Head.Effect = 0;
				Jarkkonith_Head.Emblem = 0;
				Jarkkonith_Head.Charges = 0;
				Jarkkonith_Head.MaxCharges = 0;
				Jarkkonith_Head.SpellID = 0;
				Jarkkonith_Head.ProcSpellID = 0;
				Jarkkonith_Head.Type_Damage = 0;
				Jarkkonith_Head.Realm = 0;
				Jarkkonith_Head.MaxCount = 1;
				Jarkkonith_Head.PackSize = 1;
				Jarkkonith_Head.Extension = 0;
				Jarkkonith_Head.Quality = 100;				
				Jarkkonith_Head.Condition = 100;
				Jarkkonith_Head.MaxCondition = 100;
				Jarkkonith_Head.Durability = 100;
				Jarkkonith_Head.MaxDurability = 100;
				Jarkkonith_Head.PoisonCharges = 0;
				Jarkkonith_Head.PoisonMaxCharges = 0;
				Jarkkonith_Head.PoisonSpellID = 0;
				Jarkkonith_Head.ProcSpellID1 = 0;
				Jarkkonith_Head.SpellID1 = 0;
				Jarkkonith_Head.MaxCharges1 = 0;
				Jarkkonith_Head.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Jarkkonith_Head);
				}
			Dragonslayer_Soft_Starklaedar_Shoes = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Soft_Starklaedar_Shoes");
			if (Dragonslayer_Soft_Starklaedar_Shoes == null)
			{
				Dragonslayer_Soft_Starklaedar_Shoes = new ItemTemplate();
				Dragonslayer_Soft_Starklaedar_Shoes.Name = "Dragonslayer Soft Starklaedar Shoes";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Soft_Starklaedar_Shoes.Name + ", creating it ...");
				Dragonslayer_Soft_Starklaedar_Shoes.Level = 51;
				Dragonslayer_Soft_Starklaedar_Shoes.Weight = 22;
				Dragonslayer_Soft_Starklaedar_Shoes.Model = 4025;
				Dragonslayer_Soft_Starklaedar_Shoes.Object_Type = 33;
				Dragonslayer_Soft_Starklaedar_Shoes.Item_Type = 23;
				Dragonslayer_Soft_Starklaedar_Shoes.Id_nb = "Dragonslayer_Soft_Starklaedar_Shoes";
				Dragonslayer_Soft_Starklaedar_Shoes.Hand = 0;
				Dragonslayer_Soft_Starklaedar_Shoes.IsPickable = true;
				Dragonslayer_Soft_Starklaedar_Shoes.IsDropable = true;
				Dragonslayer_Soft_Starklaedar_Shoes.IsTradable = true;
				Dragonslayer_Soft_Starklaedar_Shoes.CanDropAsLoot = true;
				Dragonslayer_Soft_Starklaedar_Shoes.Color = 0;
				Dragonslayer_Soft_Starklaedar_Shoes.Bonus = 35; // default bonus				
				Dragonslayer_Soft_Starklaedar_Shoes.Bonus1 = 3;
				Dragonslayer_Soft_Starklaedar_Shoes.Bonus1Type = (int) 23;
				Dragonslayer_Soft_Starklaedar_Shoes.Bonus2 = 3;
				Dragonslayer_Soft_Starklaedar_Shoes.Bonus2Type = (int) 49;
				Dragonslayer_Soft_Starklaedar_Shoes.Bonus3 = 6;
				Dragonslayer_Soft_Starklaedar_Shoes.Bonus3Type = (int) 13;
				Dragonslayer_Soft_Starklaedar_Shoes.Bonus4 = 6;
				Dragonslayer_Soft_Starklaedar_Shoes.Bonus4Type = (int) 17;
				Dragonslayer_Soft_Starklaedar_Shoes.Bonus5 = 6;
				Dragonslayer_Soft_Starklaedar_Shoes.Bonus5Type = (int) 19;
				Dragonslayer_Soft_Starklaedar_Shoes.Bonus6 = 20;
				Dragonslayer_Soft_Starklaedar_Shoes.Bonus6Type = (int) 10;
				Dragonslayer_Soft_Starklaedar_Shoes.Bonus7 = 3;
				Dragonslayer_Soft_Starklaedar_Shoes.Bonus7Type = (int) 164;
				Dragonslayer_Soft_Starklaedar_Shoes.Bonus8 = 5;
				Dragonslayer_Soft_Starklaedar_Shoes.Bonus8Type = (int) 148;
				Dragonslayer_Soft_Starklaedar_Shoes.Bonus9 = 0;
				Dragonslayer_Soft_Starklaedar_Shoes.Bonus9Type = (int) 0;
				Dragonslayer_Soft_Starklaedar_Shoes.Bonus10 = 0;
				Dragonslayer_Soft_Starklaedar_Shoes.Bonus10Type = (int) 0;
				Dragonslayer_Soft_Starklaedar_Shoes.ExtraBonus = 0;
				Dragonslayer_Soft_Starklaedar_Shoes.ExtraBonusType = (int) 0;
				Dragonslayer_Soft_Starklaedar_Shoes.Effect = 0;
				Dragonslayer_Soft_Starklaedar_Shoes.Emblem = 0;
				Dragonslayer_Soft_Starklaedar_Shoes.Charges = 0;
				Dragonslayer_Soft_Starklaedar_Shoes.MaxCharges = 0;
				Dragonslayer_Soft_Starklaedar_Shoes.SpellID = 0;
				Dragonslayer_Soft_Starklaedar_Shoes.ProcSpellID = 31001;
				Dragonslayer_Soft_Starklaedar_Shoes.Type_Damage = 0;
				Dragonslayer_Soft_Starklaedar_Shoes.Realm = 0;
				Dragonslayer_Soft_Starklaedar_Shoes.MaxCount = 1;
				Dragonslayer_Soft_Starklaedar_Shoes.PackSize = 1;
				Dragonslayer_Soft_Starklaedar_Shoes.Extension = 5;
				Dragonslayer_Soft_Starklaedar_Shoes.Quality = 100;				
				Dragonslayer_Soft_Starklaedar_Shoes.Condition = 50000;
				Dragonslayer_Soft_Starklaedar_Shoes.MaxCondition = 50000;
				Dragonslayer_Soft_Starklaedar_Shoes.Durability = 50000;
				Dragonslayer_Soft_Starklaedar_Shoes.MaxDurability = 0;
				Dragonslayer_Soft_Starklaedar_Shoes.PoisonCharges = 0;
				Dragonslayer_Soft_Starklaedar_Shoes.PoisonMaxCharges = 0;
				Dragonslayer_Soft_Starklaedar_Shoes.PoisonSpellID = 0;
				Dragonslayer_Soft_Starklaedar_Shoes.ProcSpellID1 = 40006;
				Dragonslayer_Soft_Starklaedar_Shoes.SpellID1 = 0;
				Dragonslayer_Soft_Starklaedar_Shoes.MaxCharges1 = 0;
				Dragonslayer_Soft_Starklaedar_Shoes.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Soft_Starklaedar_Shoes);
				}
			Dragonslayer_Hard_Starklaedar_Tunic = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Hard_Starklaedar_Tunic");
			if (Dragonslayer_Hard_Starklaedar_Tunic == null)
			{
				Dragonslayer_Hard_Starklaedar_Tunic = new ItemTemplate();
				Dragonslayer_Hard_Starklaedar_Tunic.Name = "Dragonslayer Hard Starklaedar Tunic";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Hard_Starklaedar_Tunic.Name + ", creating it ...");
				Dragonslayer_Hard_Starklaedar_Tunic.Level = 51;
				Dragonslayer_Hard_Starklaedar_Tunic.Weight = 22;
				Dragonslayer_Hard_Starklaedar_Tunic.Model = 4021;
				Dragonslayer_Hard_Starklaedar_Tunic.Object_Type = 33;
				Dragonslayer_Hard_Starklaedar_Tunic.Item_Type = 25;
				Dragonslayer_Hard_Starklaedar_Tunic.Id_nb = "Dragonslayer_Hard_Starklaedar_Tunic";
				Dragonslayer_Hard_Starklaedar_Tunic.Hand = 0;
				Dragonslayer_Hard_Starklaedar_Tunic.IsPickable = true;
				Dragonslayer_Hard_Starklaedar_Tunic.IsDropable = true;
				Dragonslayer_Hard_Starklaedar_Tunic.IsTradable = true;
				Dragonslayer_Hard_Starklaedar_Tunic.CanDropAsLoot = true;
				Dragonslayer_Hard_Starklaedar_Tunic.Color = 0;
				Dragonslayer_Hard_Starklaedar_Tunic.Bonus = 35; // default bonus				
				Dragonslayer_Hard_Starklaedar_Tunic.Bonus1 = 22;
				Dragonslayer_Hard_Starklaedar_Tunic.Bonus1Type = (int) 1;
				Dragonslayer_Hard_Starklaedar_Tunic.Bonus2 = 22;
				Dragonslayer_Hard_Starklaedar_Tunic.Bonus2Type = (int) 4;
				Dragonslayer_Hard_Starklaedar_Tunic.Bonus3 = 5;
				Dragonslayer_Hard_Starklaedar_Tunic.Bonus3Type = (int) 12;
				Dragonslayer_Hard_Starklaedar_Tunic.Bonus4 = 5;
				Dragonslayer_Hard_Starklaedar_Tunic.Bonus4Type = (int) 18;
				Dragonslayer_Hard_Starklaedar_Tunic.Bonus5 = 5;
				Dragonslayer_Hard_Starklaedar_Tunic.Bonus5Type = (int) 15;
				Dragonslayer_Hard_Starklaedar_Tunic.Bonus6 = 5;
				Dragonslayer_Hard_Starklaedar_Tunic.Bonus6Type = (int) 201;
				Dragonslayer_Hard_Starklaedar_Tunic.Bonus7 = 5;
				Dragonslayer_Hard_Starklaedar_Tunic.Bonus7Type = (int) 203;
				Dragonslayer_Hard_Starklaedar_Tunic.Bonus8 = 2;
				Dragonslayer_Hard_Starklaedar_Tunic.Bonus8Type = (int) 155;
				Dragonslayer_Hard_Starklaedar_Tunic.Bonus9 = 0;
				Dragonslayer_Hard_Starklaedar_Tunic.Bonus9Type = (int) 0;
				Dragonslayer_Hard_Starklaedar_Tunic.Bonus10 = 0;
				Dragonslayer_Hard_Starklaedar_Tunic.Bonus10Type = (int) 0;
				Dragonslayer_Hard_Starklaedar_Tunic.ExtraBonus = 0;
				Dragonslayer_Hard_Starklaedar_Tunic.ExtraBonusType = (int) 0;
				Dragonslayer_Hard_Starklaedar_Tunic.Effect = 0;
				Dragonslayer_Hard_Starklaedar_Tunic.Emblem = 0;
				Dragonslayer_Hard_Starklaedar_Tunic.Charges = 0;
				Dragonslayer_Hard_Starklaedar_Tunic.MaxCharges = 0;
				Dragonslayer_Hard_Starklaedar_Tunic.SpellID = 0;
				Dragonslayer_Hard_Starklaedar_Tunic.ProcSpellID = 31001;
				Dragonslayer_Hard_Starklaedar_Tunic.Type_Damage = 0;
				Dragonslayer_Hard_Starklaedar_Tunic.Realm = 0;
				Dragonslayer_Hard_Starklaedar_Tunic.MaxCount = 1;
				Dragonslayer_Hard_Starklaedar_Tunic.PackSize = 1;
				Dragonslayer_Hard_Starklaedar_Tunic.Extension = 5;
				Dragonslayer_Hard_Starklaedar_Tunic.Quality = 100;				
				Dragonslayer_Hard_Starklaedar_Tunic.Condition = 50000;
				Dragonslayer_Hard_Starklaedar_Tunic.MaxCondition = 50000;
				Dragonslayer_Hard_Starklaedar_Tunic.Durability = 50000;
				Dragonslayer_Hard_Starklaedar_Tunic.MaxDurability = 0;
				Dragonslayer_Hard_Starklaedar_Tunic.PoisonCharges = 0;
				Dragonslayer_Hard_Starklaedar_Tunic.PoisonMaxCharges = 0;
				Dragonslayer_Hard_Starklaedar_Tunic.PoisonSpellID = 0;
				Dragonslayer_Hard_Starklaedar_Tunic.ProcSpellID1 = 40006;
				Dragonslayer_Hard_Starklaedar_Tunic.SpellID1 = 0;
				Dragonslayer_Hard_Starklaedar_Tunic.MaxCharges1 = 0;
				Dragonslayer_Hard_Starklaedar_Tunic.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Hard_Starklaedar_Tunic);
				}
			Dragonslayer_Hard_Starklaedar_Gloves = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Hard_Starklaedar_Gloves");
			if (Dragonslayer_Hard_Starklaedar_Gloves == null)
			{
				Dragonslayer_Hard_Starklaedar_Gloves = new ItemTemplate();
				Dragonslayer_Hard_Starklaedar_Gloves.Name = "Dragonslayer Hard Starklaedar Gloves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Hard_Starklaedar_Gloves.Name + ", creating it ...");
				Dragonslayer_Hard_Starklaedar_Gloves.Level = 51;
				Dragonslayer_Hard_Starklaedar_Gloves.Weight = 22;
				Dragonslayer_Hard_Starklaedar_Gloves.Model = 4024;
				Dragonslayer_Hard_Starklaedar_Gloves.Object_Type = 33;
				Dragonslayer_Hard_Starklaedar_Gloves.Item_Type = 22;
				Dragonslayer_Hard_Starklaedar_Gloves.Id_nb = "Dragonslayer_Hard_Starklaedar_Gloves";
				Dragonslayer_Hard_Starklaedar_Gloves.Hand = 0;
				Dragonslayer_Hard_Starklaedar_Gloves.IsPickable = true;
				Dragonslayer_Hard_Starklaedar_Gloves.IsDropable = true;
				Dragonslayer_Hard_Starklaedar_Gloves.IsTradable = true;
				Dragonslayer_Hard_Starklaedar_Gloves.CanDropAsLoot = true;
				Dragonslayer_Hard_Starklaedar_Gloves.Color = 0;
				Dragonslayer_Hard_Starklaedar_Gloves.Bonus = 35; // default bonus				
				Dragonslayer_Hard_Starklaedar_Gloves.Bonus1 = 5;
				Dragonslayer_Hard_Starklaedar_Gloves.Bonus1Type = (int) 12;
				Dragonslayer_Hard_Starklaedar_Gloves.Bonus2 = 5;
				Dragonslayer_Hard_Starklaedar_Gloves.Bonus2Type = (int) 18;
				Dragonslayer_Hard_Starklaedar_Gloves.Bonus3 = 45;
				Dragonslayer_Hard_Starklaedar_Gloves.Bonus3Type = (int) 10;
				Dragonslayer_Hard_Starklaedar_Gloves.Bonus4 = 5;
				Dragonslayer_Hard_Starklaedar_Gloves.Bonus4Type = (int) 15;
				Dragonslayer_Hard_Starklaedar_Gloves.Bonus5 = 3;
				Dragonslayer_Hard_Starklaedar_Gloves.Bonus5Type = (int) 164;
				Dragonslayer_Hard_Starklaedar_Gloves.Bonus6 = 45;
				Dragonslayer_Hard_Starklaedar_Gloves.Bonus6Type = (int) 210;
				Dragonslayer_Hard_Starklaedar_Gloves.Bonus7 = 10;
				Dragonslayer_Hard_Starklaedar_Gloves.Bonus7Type = (int) 148;
				Dragonslayer_Hard_Starklaedar_Gloves.Bonus8 = 0;
				Dragonslayer_Hard_Starklaedar_Gloves.Bonus8Type = (int) 0;
				Dragonslayer_Hard_Starklaedar_Gloves.Bonus9 = 0;
				Dragonslayer_Hard_Starklaedar_Gloves.Bonus9Type = (int) 0;
				Dragonslayer_Hard_Starklaedar_Gloves.Bonus10 = 0;
				Dragonslayer_Hard_Starklaedar_Gloves.Bonus10Type = (int) 0;
				Dragonslayer_Hard_Starklaedar_Gloves.ExtraBonus = 0;
				Dragonslayer_Hard_Starklaedar_Gloves.ExtraBonusType = (int) 0;
				Dragonslayer_Hard_Starklaedar_Gloves.Effect = 0;
				Dragonslayer_Hard_Starklaedar_Gloves.Emblem = 0;
				Dragonslayer_Hard_Starklaedar_Gloves.Charges = 0;
				Dragonslayer_Hard_Starklaedar_Gloves.MaxCharges = 0;
				Dragonslayer_Hard_Starklaedar_Gloves.SpellID = 0;
				Dragonslayer_Hard_Starklaedar_Gloves.ProcSpellID = 31001;
				Dragonslayer_Hard_Starklaedar_Gloves.Type_Damage = 0;
				Dragonslayer_Hard_Starklaedar_Gloves.Realm = 0;
				Dragonslayer_Hard_Starklaedar_Gloves.MaxCount = 1;
				Dragonslayer_Hard_Starklaedar_Gloves.PackSize = 1;
				Dragonslayer_Hard_Starklaedar_Gloves.Extension = 5;
				Dragonslayer_Hard_Starklaedar_Gloves.Quality = 100;				
				Dragonslayer_Hard_Starklaedar_Gloves.Condition = 50000;
				Dragonslayer_Hard_Starklaedar_Gloves.MaxCondition = 50000;
				Dragonslayer_Hard_Starklaedar_Gloves.Durability = 50000;
				Dragonslayer_Hard_Starklaedar_Gloves.MaxDurability = 0;
				Dragonslayer_Hard_Starklaedar_Gloves.PoisonCharges = 0;
				Dragonslayer_Hard_Starklaedar_Gloves.PoisonMaxCharges = 0;
				Dragonslayer_Hard_Starklaedar_Gloves.PoisonSpellID = 0;
				Dragonslayer_Hard_Starklaedar_Gloves.ProcSpellID1 = 40006;
				Dragonslayer_Hard_Starklaedar_Gloves.SpellID1 = 0;
				Dragonslayer_Hard_Starklaedar_Gloves.MaxCharges1 = 0;
				Dragonslayer_Hard_Starklaedar_Gloves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Hard_Starklaedar_Gloves);
				}
			Dragonslayer_Starklaedar_Full_Helm = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Starklaedar_Full_Helm");
			if (Dragonslayer_Starklaedar_Full_Helm == null)
			{
				Dragonslayer_Starklaedar_Full_Helm = new ItemTemplate();
				Dragonslayer_Starklaedar_Full_Helm.Name = "Dragonslayer Starklaedar Full Helm";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Starklaedar_Full_Helm.Name + ", creating it ...");
				Dragonslayer_Starklaedar_Full_Helm.Level = 51;
				Dragonslayer_Starklaedar_Full_Helm.Weight = 22;
				Dragonslayer_Starklaedar_Full_Helm.Model = 4068;
				Dragonslayer_Starklaedar_Full_Helm.Object_Type = 33;
				Dragonslayer_Starklaedar_Full_Helm.Item_Type = 21;
				Dragonslayer_Starklaedar_Full_Helm.Id_nb = "Dragonslayer_Starklaedar_Full_Helm";
				Dragonslayer_Starklaedar_Full_Helm.Hand = 0;
				Dragonslayer_Starklaedar_Full_Helm.IsPickable = true;
				Dragonslayer_Starklaedar_Full_Helm.IsDropable = true;
				Dragonslayer_Starklaedar_Full_Helm.IsTradable = true;
				Dragonslayer_Starklaedar_Full_Helm.CanDropAsLoot = true;
				Dragonslayer_Starklaedar_Full_Helm.Color = 0;
				Dragonslayer_Starklaedar_Full_Helm.Bonus = 35; // default bonus				
				Dragonslayer_Starklaedar_Full_Helm.Bonus1 = 22;
				Dragonslayer_Starklaedar_Full_Helm.Bonus1Type = (int) 3;
				Dragonslayer_Starklaedar_Full_Helm.Bonus2 = 6;
				Dragonslayer_Starklaedar_Full_Helm.Bonus2Type = (int) 16;
				Dragonslayer_Starklaedar_Full_Helm.Bonus3 = 6;
				Dragonslayer_Starklaedar_Full_Helm.Bonus3Type = (int) 11;
				Dragonslayer_Starklaedar_Full_Helm.Bonus4 = 40;
				Dragonslayer_Starklaedar_Full_Helm.Bonus4Type = (int) 10;
				Dragonslayer_Starklaedar_Full_Helm.Bonus5 = 6;
				Dragonslayer_Starklaedar_Full_Helm.Bonus5Type = (int) 14;
				Dragonslayer_Starklaedar_Full_Helm.Bonus6 = 5;
				Dragonslayer_Starklaedar_Full_Helm.Bonus6Type = (int) 148;
				Dragonslayer_Starklaedar_Full_Helm.Bonus7 = 40;
				Dragonslayer_Starklaedar_Full_Helm.Bonus7Type = (int) 210;
				Dragonslayer_Starklaedar_Full_Helm.Bonus8 = 0;
				Dragonslayer_Starklaedar_Full_Helm.Bonus8Type = (int) 0;
				Dragonslayer_Starklaedar_Full_Helm.Bonus9 = 0;
				Dragonslayer_Starklaedar_Full_Helm.Bonus9Type = (int) 0;
				Dragonslayer_Starklaedar_Full_Helm.Bonus10 = 0;
				Dragonslayer_Starklaedar_Full_Helm.Bonus10Type = (int) 0;
				Dragonslayer_Starklaedar_Full_Helm.ExtraBonus = 0;
				Dragonslayer_Starklaedar_Full_Helm.ExtraBonusType = (int) 0;
				Dragonslayer_Starklaedar_Full_Helm.Effect = 0;
				Dragonslayer_Starklaedar_Full_Helm.Emblem = 0;
				Dragonslayer_Starklaedar_Full_Helm.Charges = 0;
				Dragonslayer_Starklaedar_Full_Helm.MaxCharges = 0;
				Dragonslayer_Starklaedar_Full_Helm.SpellID = 0;
				Dragonslayer_Starklaedar_Full_Helm.ProcSpellID = 31001;
				Dragonslayer_Starklaedar_Full_Helm.Type_Damage = 0;
				Dragonslayer_Starklaedar_Full_Helm.Realm = 0;
				Dragonslayer_Starklaedar_Full_Helm.MaxCount = 1;
				Dragonslayer_Starklaedar_Full_Helm.PackSize = 1;
				Dragonslayer_Starklaedar_Full_Helm.Extension = 0;
				Dragonslayer_Starklaedar_Full_Helm.Quality = 100;				
				Dragonslayer_Starklaedar_Full_Helm.Condition = 50000;
				Dragonslayer_Starklaedar_Full_Helm.MaxCondition = 50000;
				Dragonslayer_Starklaedar_Full_Helm.Durability = 50000;
				Dragonslayer_Starklaedar_Full_Helm.MaxDurability = 0;
				Dragonslayer_Starklaedar_Full_Helm.PoisonCharges = 0;
				Dragonslayer_Starklaedar_Full_Helm.PoisonMaxCharges = 0;
				Dragonslayer_Starklaedar_Full_Helm.PoisonSpellID = 0;
				Dragonslayer_Starklaedar_Full_Helm.ProcSpellID1 = 40006;
				Dragonslayer_Starklaedar_Full_Helm.SpellID1 = 0;
				Dragonslayer_Starklaedar_Full_Helm.MaxCharges1 = 0;
				Dragonslayer_Starklaedar_Full_Helm.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Starklaedar_Full_Helm);
				}
			Dragonslayer_Hard_Starklaedar_Pants = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Hard_Starklaedar_Pants");
			if (Dragonslayer_Hard_Starklaedar_Pants == null)
			{
				Dragonslayer_Hard_Starklaedar_Pants = new ItemTemplate();
				Dragonslayer_Hard_Starklaedar_Pants.Name = "Dragonslayer Hard Starklaedar Pants";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Hard_Starklaedar_Pants.Name + ", creating it ...");
				Dragonslayer_Hard_Starklaedar_Pants.Level = 51;
				Dragonslayer_Hard_Starklaedar_Pants.Weight = 22;
				Dragonslayer_Hard_Starklaedar_Pants.Model = 4022;
				Dragonslayer_Hard_Starklaedar_Pants.Object_Type = 33;
				Dragonslayer_Hard_Starklaedar_Pants.Item_Type = 27;
				Dragonslayer_Hard_Starklaedar_Pants.Id_nb = "Dragonslayer_Hard_Starklaedar_Pants";
				Dragonslayer_Hard_Starklaedar_Pants.Hand = 0;
				Dragonslayer_Hard_Starklaedar_Pants.IsPickable = true;
				Dragonslayer_Hard_Starklaedar_Pants.IsDropable = true;
				Dragonslayer_Hard_Starklaedar_Pants.IsTradable = true;
				Dragonslayer_Hard_Starklaedar_Pants.CanDropAsLoot = true;
				Dragonslayer_Hard_Starklaedar_Pants.Color = 0;
				Dragonslayer_Hard_Starklaedar_Pants.Bonus = 35; // default bonus				
				Dragonslayer_Hard_Starklaedar_Pants.Bonus1 = 22;
				Dragonslayer_Hard_Starklaedar_Pants.Bonus1Type = (int) 3;
				Dragonslayer_Hard_Starklaedar_Pants.Bonus2 = 5;
				Dragonslayer_Hard_Starklaedar_Pants.Bonus2Type = (int) 12;
				Dragonslayer_Hard_Starklaedar_Pants.Bonus3 = 5;
				Dragonslayer_Hard_Starklaedar_Pants.Bonus3Type = (int) 18;
				Dragonslayer_Hard_Starklaedar_Pants.Bonus4 = 40;
				Dragonslayer_Hard_Starklaedar_Pants.Bonus4Type = (int) 10;
				Dragonslayer_Hard_Starklaedar_Pants.Bonus5 = 5;
				Dragonslayer_Hard_Starklaedar_Pants.Bonus5Type = (int) 15;
				Dragonslayer_Hard_Starklaedar_Pants.Bonus6 = 1;
				Dragonslayer_Hard_Starklaedar_Pants.Bonus6Type = (int) 155;
				Dragonslayer_Hard_Starklaedar_Pants.Bonus7 = 5;
				Dragonslayer_Hard_Starklaedar_Pants.Bonus7Type = (int) 203;
				Dragonslayer_Hard_Starklaedar_Pants.Bonus8 = 0;
				Dragonslayer_Hard_Starklaedar_Pants.Bonus8Type = (int) 0;
				Dragonslayer_Hard_Starklaedar_Pants.Bonus9 = 0;
				Dragonslayer_Hard_Starklaedar_Pants.Bonus9Type = (int) 0;
				Dragonslayer_Hard_Starklaedar_Pants.Bonus10 = 0;
				Dragonslayer_Hard_Starklaedar_Pants.Bonus10Type = (int) 0;
				Dragonslayer_Hard_Starklaedar_Pants.ExtraBonus = 0;
				Dragonslayer_Hard_Starklaedar_Pants.ExtraBonusType = (int) 0;
				Dragonslayer_Hard_Starklaedar_Pants.Effect = 0;
				Dragonslayer_Hard_Starklaedar_Pants.Emblem = 0;
				Dragonslayer_Hard_Starklaedar_Pants.Charges = 0;
				Dragonslayer_Hard_Starklaedar_Pants.MaxCharges = 0;
				Dragonslayer_Hard_Starklaedar_Pants.SpellID = 0;
				Dragonslayer_Hard_Starklaedar_Pants.ProcSpellID = 31001;
				Dragonslayer_Hard_Starklaedar_Pants.Type_Damage = 0;
				Dragonslayer_Hard_Starklaedar_Pants.Realm = 0;
				Dragonslayer_Hard_Starklaedar_Pants.MaxCount = 1;
				Dragonslayer_Hard_Starklaedar_Pants.PackSize = 1;
				Dragonslayer_Hard_Starklaedar_Pants.Extension = 5;
				Dragonslayer_Hard_Starklaedar_Pants.Quality = 100;				
				Dragonslayer_Hard_Starklaedar_Pants.Condition = 50000;
				Dragonslayer_Hard_Starklaedar_Pants.MaxCondition = 50000;
				Dragonslayer_Hard_Starklaedar_Pants.Durability = 50000;
				Dragonslayer_Hard_Starklaedar_Pants.MaxDurability = 0;
				Dragonslayer_Hard_Starklaedar_Pants.PoisonCharges = 0;
				Dragonslayer_Hard_Starklaedar_Pants.PoisonMaxCharges = 0;
				Dragonslayer_Hard_Starklaedar_Pants.PoisonSpellID = 0;
				Dragonslayer_Hard_Starklaedar_Pants.ProcSpellID1 = 40006;
				Dragonslayer_Hard_Starklaedar_Pants.SpellID1 = 0;
				Dragonslayer_Hard_Starklaedar_Pants.MaxCharges1 = 0;
				Dragonslayer_Hard_Starklaedar_Pants.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Hard_Starklaedar_Pants);
				}
			Dragonslayer_Hard_Starklaedar_Sleeves = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Hard_Starklaedar_Sleeves");
			if (Dragonslayer_Hard_Starklaedar_Sleeves == null)
			{
				Dragonslayer_Hard_Starklaedar_Sleeves = new ItemTemplate();
				Dragonslayer_Hard_Starklaedar_Sleeves.Name = "Dragonslayer Hard Starklaedar Sleeves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Hard_Starklaedar_Sleeves.Name + ", creating it ...");
				Dragonslayer_Hard_Starklaedar_Sleeves.Level = 51;
				Dragonslayer_Hard_Starklaedar_Sleeves.Weight = 22;
				Dragonslayer_Hard_Starklaedar_Sleeves.Model = 4023;
				Dragonslayer_Hard_Starklaedar_Sleeves.Object_Type = 33;
				Dragonslayer_Hard_Starklaedar_Sleeves.Item_Type = 28;
				Dragonslayer_Hard_Starklaedar_Sleeves.Id_nb = "Dragonslayer_Hard_Starklaedar_Sleeves";
				Dragonslayer_Hard_Starklaedar_Sleeves.Hand = 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.IsPickable = true;
				Dragonslayer_Hard_Starklaedar_Sleeves.IsDropable = true;
				Dragonslayer_Hard_Starklaedar_Sleeves.IsTradable = true;
				Dragonslayer_Hard_Starklaedar_Sleeves.CanDropAsLoot = true;
				Dragonslayer_Hard_Starklaedar_Sleeves.Color = 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.Bonus = 35; // default bonus				
				Dragonslayer_Hard_Starklaedar_Sleeves.Bonus1 = 22;
				Dragonslayer_Hard_Starklaedar_Sleeves.Bonus1Type = (int) 1;
				Dragonslayer_Hard_Starklaedar_Sleeves.Bonus2 = 5;
				Dragonslayer_Hard_Starklaedar_Sleeves.Bonus2Type = (int) 12;
				Dragonslayer_Hard_Starklaedar_Sleeves.Bonus3 = 5;
				Dragonslayer_Hard_Starklaedar_Sleeves.Bonus3Type = (int) 18;
				Dragonslayer_Hard_Starklaedar_Sleeves.Bonus4 = 40;
				Dragonslayer_Hard_Starklaedar_Sleeves.Bonus4Type = (int) 10;
				Dragonslayer_Hard_Starklaedar_Sleeves.Bonus5 = 5;
				Dragonslayer_Hard_Starklaedar_Sleeves.Bonus5Type = (int) 15;
				Dragonslayer_Hard_Starklaedar_Sleeves.Bonus6 = 1;
				Dragonslayer_Hard_Starklaedar_Sleeves.Bonus6Type = (int) 155;
				Dragonslayer_Hard_Starklaedar_Sleeves.Bonus7 = 5;
				Dragonslayer_Hard_Starklaedar_Sleeves.Bonus7Type = (int) 201;
				Dragonslayer_Hard_Starklaedar_Sleeves.Bonus8 = 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.Bonus8Type = (int) 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.Bonus9 = 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.Bonus9Type = (int) 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.Bonus10 = 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.Bonus10Type = (int) 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.ExtraBonus = 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.ExtraBonusType = (int) 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.Effect = 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.Emblem = 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.Charges = 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.MaxCharges = 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.SpellID = 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.ProcSpellID = 31001;
				Dragonslayer_Hard_Starklaedar_Sleeves.Type_Damage = 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.Realm = 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.MaxCount = 1;
				Dragonslayer_Hard_Starklaedar_Sleeves.PackSize = 1;
				Dragonslayer_Hard_Starklaedar_Sleeves.Extension = 5;
				Dragonslayer_Hard_Starklaedar_Sleeves.Quality = 100;				
				Dragonslayer_Hard_Starklaedar_Sleeves.Condition = 50000;
				Dragonslayer_Hard_Starklaedar_Sleeves.MaxCondition = 50000;
				Dragonslayer_Hard_Starklaedar_Sleeves.Durability = 50000;
				Dragonslayer_Hard_Starklaedar_Sleeves.MaxDurability = 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.PoisonCharges = 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.PoisonMaxCharges = 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.PoisonSpellID = 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.ProcSpellID1 = 40006;
				Dragonslayer_Hard_Starklaedar_Sleeves.SpellID1 = 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.MaxCharges1 = 0;
				Dragonslayer_Hard_Starklaedar_Sleeves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Hard_Starklaedar_Sleeves);
				}


            #endregion

            #region defineAreas

            #endregion

            #region defineQuestParts

            QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerShadowbladeQuest));
            QuestBehaviour a;
            a = builder.CreateBehaviour(AmarasavaDas, -1);
            a.AddTrigger(eTriggerType.Interact, null, AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerShadowbladeQuest), AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerShadowbladeQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "I have lived in this village since I was a young girl. My father was a great dragon hunter you know.", AmarasavaDas);
            a.AddAction(eActionType.Talk, "In my years I have learned to work with all kinds of problems, specially with [Dragons].", AmarasavaDas);
            AddBehaviour(a);

            a = builder.CreateBehaviour(AmarasavaDas, -1);
            a.AddTrigger(eTriggerType.Whisper, "Dragons", AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerShadowbladeQuest), AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerShadowbladeQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "yes dragons, but there is a problem for me, i can't hunt dragons by self, [you know].", AmarasavaDas);
            AddBehaviour(a);

            a = builder.CreateBehaviour(AmarasavaDas, -1);
            a.AddTrigger(eTriggerType.Whisper, "you know", AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerShadowbladeQuest), AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerShadowbladeQuest), null, (eComparator)5);
            a.AddAction(eActionType.OfferQuest, typeof(DragonslayerShadowbladeQuest), "Helen has offered you the Dragonslayer quest.?Do you accept?");
            AddBehaviour(a);

            a = builder.CreateBehaviour(AmarasavaDas, -1);
            a.AddTrigger(eTriggerType.DeclineQuest, null, typeof(DragonslayerShadowbladeQuest));
            a.AddAction(eActionType.Talk, "No problem. See you", AmarasavaDas);
            AddBehaviour(a);

            a = builder.CreateBehaviour(AmarasavaDas, -1);
            a.AddTrigger(eTriggerType.AcceptQuest, null, typeof(DragonslayerShadowbladeQuest));
            a.AddAction(eActionType.Talk, "If you bring me the head of Jarkkonith I shall pay you well!", AmarasavaDas);
            a.AddAction(eActionType.GiveQuest, typeof(DragonslayerShadowbladeQuest), AmarasavaDas);
            AddBehaviour(a);

			          
			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End
			if (AmarasavaDas!=null) {
				AmarasavaDas.AddQuestToGive(typeof (DragonslayerShadowbladeQuest));
			}
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{			
				
			// Custom Scriptunloaded Code Begin
			
			// Custom Scriptunloaded Code End

			

			/* If AmarasavaDas has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (AmarasavaDas == null)
				return;
			/* Now we remove the possibility to give this quest to players */			
			AmarasavaDas.RemoveQuestToGive(typeof (DragonslayerShadowbladeQuest));
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
						return "[Step #1] Kill the Minidragon Jarrkonith and prove your strenght.When you got it come back to me with his HEAD";
					case 2:
						return "[Step #2] Bring the head of Jarkkonith to Amarasava Das";
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
			if (player.IsDoingQuest(typeof (DragonslayerShadowbladeQuest)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
			if (
		
			player.CharacterClass.ID != (byte) eCharacterClass.Shadowblade && 
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
