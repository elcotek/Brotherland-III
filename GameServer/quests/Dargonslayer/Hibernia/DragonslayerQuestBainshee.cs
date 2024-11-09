	
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
	public class DragonslayerBainsheeQuest : BaseQuest
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

		protected const string questTitle = "DragonslayerBainsheeQuest";

		protected const int minimumLevel = 49;
		protected const int maximumLevel = 50;
	
	
		private static GameNPC Lirazal = null;
		
		private static GameNPC Asiintath = null;
		
		private static ItemTemplate Asiintath_Head = null;
		
		private static ItemTemplate Dragonslayer_Woven_Robe = null;
		
		private static ItemTemplate Dragonslayer_Woven_Sleeves = null;
		
		private static ItemTemplate Dragonslayer_Padded_Breeches_Hib = null;
		
		private static ItemTemplate Dragonslayer_Woven_Full_Helm = null;
		
		private static ItemTemplate Dragonslayer_Woven_Slippers = null;
		
		private static ItemTemplate Dragonslayer_Woven_Gloves = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public DragonslayerBainsheeQuest() : base()
		{
		}

		public DragonslayerBainsheeQuest(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public DragonslayerBainsheeQuest(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public DragonslayerBainsheeQuest(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
	{
	}
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			QuestBehaviour a;
			QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerBainsheeQuest));

			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(DragonslayerBainsheeQuest)) == null)
				return;


			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;

				if (gArgs.Target.Name.IndexOf("Asiintath") >= 0)
				{

					GiveItem(gArgs.Target, player, Asiintath_Head);
					Step = 2;
					return;
				}
			}


			if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs)args;

				if (gArgs.Target.Name == Lirazal.Name && gArgs.Item.Id_nb == Asiintath_Head.Id_nb)
				{

					if (player.Inventory.IsSlotsFree(6, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
					{

						a = builder.CreateBehaviour(Lirazal, -1);
						a.AddTrigger(eTriggerType.GiveItem, Lirazal, Asiintath_Head);
						a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerBainsheeQuest), null);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Woven_Robe, Lirazal);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Woven_Sleeves, Lirazal);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Padded_Breeches_Hib, Lirazal);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Woven_Full_Helm, Lirazal);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Woven_Slippers, Lirazal);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Woven_Gloves, Lirazal);
						a.AddAction(eActionType.FinishQuest, typeof(DragonslayerBainsheeQuest), null);
						a.AddAction(eActionType.DestroyItem, Asiintath_Head, null);
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
	
			npcs = WorldMgr.GetNPCsByName("Lirazal",eRealm.Hibernia);
			if (npcs.Length == 0)
			{
				if (!WorldMgr.GetRegion(200).IsDisabled)
				{
				Lirazal = new DOL.GS.GameNPC();
					Lirazal.Model = 200;
				Lirazal.Name = "Lirazal";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Lirazal.Name + ", creating ...");
				Lirazal.GuildName = "Part of " + questTitle + " Quest";
				Lirazal.Realm = eRealm.Hibernia;
				Lirazal.CurrentRegionID = 200;
				Lirazal.Size = 50;
				Lirazal.Level = 65;
				Lirazal.MaxSpeedBase = 200;
				Lirazal.Faction = FactionMgr.GetFactionByID(0);
				Lirazal.X = 372948;
				Lirazal.Y = 652117;
				Lirazal.Z = 4719;
				Lirazal.Heading = 3153;
				Lirazal.RespawnInterval = 0;
				Lirazal.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				Lirazal.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					Lirazal.SaveIntoDatabase();
					
				Lirazal.AddToWorld();
				
				}
			}
			else 
			{
				Lirazal = npcs[0];
			}
		
			npcs = WorldMgr.GetNPCsByName("Asiintath",eRealm.None);
			if (npcs.Length == 0)
			{
				if (!WorldMgr.GetRegion(200).IsDisabled)
				{
				Asiintath = new DOL.GS.GameNPC();
					Asiintath.Model = 2307;
				Asiintath.Name = "Asiintath";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Asiintath.Name + ", creating ...");
				Asiintath.GuildName = "Part of " + questTitle + " Quest";
				Asiintath.Realm = eRealm.None;
				Asiintath.CurrentRegionID = 200;
				Asiintath.Size = 150;
				Asiintath.Level = 65;
				Asiintath.MaxSpeedBase = 200;
				Asiintath.Faction = FactionMgr.GetFactionByID(0);
				Asiintath.X = 381689;
				Asiintath.Y = 702700;
				Asiintath.Z = 6290;
				Asiintath.Heading = 3331;
				Asiintath.RespawnInterval = 0;
				Asiintath.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 100;
				brain.AggroRange = 1000;
				Asiintath.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					Asiintath.SaveIntoDatabase();
					
				Asiintath.AddToWorld();
				
				}
			}
			else 
			{
				Asiintath = npcs[0];
			}
		

			#endregion

			#region defineItems

		Asiintath_Head = GameServer.Database.FindObjectByKey<ItemTemplate>("Asiintath_Head");
			if (Asiintath_Head == null)
			{
				Asiintath_Head = new ItemTemplate();
				Asiintath_Head.Name = "Asiintath Head";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Asiintath_Head.Name + ", creating it ...");
				Asiintath_Head.Level = 1;
				Asiintath_Head.Weight = 1;
				Asiintath_Head.Model = 503;
				Asiintath_Head.Object_Type = 0;
				Asiintath_Head.Item_Type = 0;
				Asiintath_Head.Id_nb = "Asiintath_Head";
				Asiintath_Head.Hand = 0;
				Asiintath_Head.IsPickable = true;
				Asiintath_Head.IsDropable = true;
				Asiintath_Head.IsTradable = true;
				Asiintath_Head.CanDropAsLoot = true;
				Asiintath_Head.Color = 0;
				Asiintath_Head.Bonus = 0; // default bonus				
				Asiintath_Head.Bonus1 = 0;
				Asiintath_Head.Bonus1Type = (int) 0;
				Asiintath_Head.Bonus2 = 0;
				Asiintath_Head.Bonus2Type = (int) 0;
				Asiintath_Head.Bonus3 = 0;
				Asiintath_Head.Bonus3Type = (int) 208;
				Asiintath_Head.Bonus4 = 0;
				Asiintath_Head.Bonus4Type = (int) 0;
				Asiintath_Head.Bonus5 = 0;
				Asiintath_Head.Bonus5Type = (int) 0;
				Asiintath_Head.Bonus6 = 0;
				Asiintath_Head.Bonus6Type = (int) 0;
				Asiintath_Head.Bonus7 = 0;
				Asiintath_Head.Bonus7Type = (int) 0;
				Asiintath_Head.Bonus8 = 0;
				Asiintath_Head.Bonus8Type = (int) 0;
				Asiintath_Head.Bonus9 = 0;
				Asiintath_Head.Bonus9Type = (int) 0;
				Asiintath_Head.Bonus10 = 0;
				Asiintath_Head.Bonus10Type = (int) 0;
				Asiintath_Head.ExtraBonus = 0;
				Asiintath_Head.ExtraBonusType = (int) 0;
				Asiintath_Head.Effect = 0;
				Asiintath_Head.Emblem = 0;
				Asiintath_Head.Charges = 0;
				Asiintath_Head.MaxCharges = 0;
				Asiintath_Head.SpellID = 0;
				Asiintath_Head.ProcSpellID = 0;
				Asiintath_Head.Type_Damage = 0;
				Asiintath_Head.Realm = 0;
				Asiintath_Head.MaxCount = 1;
				Asiintath_Head.PackSize = 1;
				Asiintath_Head.Extension = 0;
				Asiintath_Head.Quality = 100;				
				Asiintath_Head.Condition = 100;
				Asiintath_Head.MaxCondition = 100;
				Asiintath_Head.Durability = 100;
				Asiintath_Head.MaxDurability = 100;
				Asiintath_Head.PoisonCharges = 0;
				Asiintath_Head.PoisonMaxCharges = 0;
				Asiintath_Head.PoisonSpellID = 0;
				Asiintath_Head.ProcSpellID1 = 0;
				Asiintath_Head.SpellID1 = 0;
				Asiintath_Head.MaxCharges1 = 0;
				Asiintath_Head.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Asiintath_Head);
				}
			Dragonslayer_Woven_Robe = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Woven_Robe");
			if (Dragonslayer_Woven_Robe == null)
			{
				Dragonslayer_Woven_Robe = new ItemTemplate();
				Dragonslayer_Woven_Robe.Name = "Dragonslayer Woven Robe";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Woven_Robe.Name + ", creating it ...");
				Dragonslayer_Woven_Robe.Level = 51;
				Dragonslayer_Woven_Robe.Weight = 15;
				Dragonslayer_Woven_Robe.Model = 4104;
				Dragonslayer_Woven_Robe.Object_Type = 32;
				Dragonslayer_Woven_Robe.Item_Type = 25;
				Dragonslayer_Woven_Robe.Id_nb = "Dragonslayer_Woven_Robe";
				Dragonslayer_Woven_Robe.Hand = 0;
				Dragonslayer_Woven_Robe.IsPickable = true;
				Dragonslayer_Woven_Robe.IsDropable = true;
				Dragonslayer_Woven_Robe.IsTradable = true;
				Dragonslayer_Woven_Robe.CanDropAsLoot = true;
				Dragonslayer_Woven_Robe.Color = 0;
				Dragonslayer_Woven_Robe.Bonus = 35; // default bonus				
				Dragonslayer_Woven_Robe.Bonus1 = 22;
				Dragonslayer_Woven_Robe.Bonus1Type = (int) 2;
				Dragonslayer_Woven_Robe.Bonus2 = 5;
				Dragonslayer_Woven_Robe.Bonus2Type = (int) 12;
				Dragonslayer_Woven_Robe.Bonus3 = 5;
				Dragonslayer_Woven_Robe.Bonus3Type = (int) 18;
				Dragonslayer_Woven_Robe.Bonus4 = 5;
				Dragonslayer_Woven_Robe.Bonus4Type = (int) 15;
				Dragonslayer_Woven_Robe.Bonus5 = 22;
				Dragonslayer_Woven_Robe.Bonus5Type = (int) 156;
				Dragonslayer_Woven_Robe.Bonus6 = 5;
				Dragonslayer_Woven_Robe.Bonus6Type = (int) 202;
				Dragonslayer_Woven_Robe.Bonus7 = 2;
				Dragonslayer_Woven_Robe.Bonus7Type = (int) 191;
				Dragonslayer_Woven_Robe.Bonus8 = 5;
				Dragonslayer_Woven_Robe.Bonus8Type = (int) 209;
				Dragonslayer_Woven_Robe.Bonus9 = 0;
				Dragonslayer_Woven_Robe.Bonus9Type = (int) 0;
				Dragonslayer_Woven_Robe.Bonus10 = 0;
				Dragonslayer_Woven_Robe.Bonus10Type = (int) 0;
				Dragonslayer_Woven_Robe.ExtraBonus = 0;
				Dragonslayer_Woven_Robe.ExtraBonusType = (int) 0;
				Dragonslayer_Woven_Robe.Effect = 0;
				Dragonslayer_Woven_Robe.Emblem = 0;
				Dragonslayer_Woven_Robe.Charges = 0;
				Dragonslayer_Woven_Robe.MaxCharges = 0;
				Dragonslayer_Woven_Robe.SpellID = 0;
				Dragonslayer_Woven_Robe.ProcSpellID = 31001;
				Dragonslayer_Woven_Robe.Type_Damage = 0;
				Dragonslayer_Woven_Robe.Realm = 0;
				Dragonslayer_Woven_Robe.MaxCount = 1;
				Dragonslayer_Woven_Robe.PackSize = 1;
				Dragonslayer_Woven_Robe.Extension = 5;
				Dragonslayer_Woven_Robe.Quality = 100;				
				Dragonslayer_Woven_Robe.Condition = 50000;
				Dragonslayer_Woven_Robe.MaxCondition = 50000;
				Dragonslayer_Woven_Robe.Durability = 50000;
				Dragonslayer_Woven_Robe.MaxDurability = 0;
				Dragonslayer_Woven_Robe.PoisonCharges = 0;
				Dragonslayer_Woven_Robe.PoisonMaxCharges = 0;
				Dragonslayer_Woven_Robe.PoisonSpellID = 0;
				Dragonslayer_Woven_Robe.ProcSpellID1 = 40006;
				Dragonslayer_Woven_Robe.SpellID1 = 0;
				Dragonslayer_Woven_Robe.MaxCharges1 = 0;
				Dragonslayer_Woven_Robe.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Woven_Robe);
				}
			Dragonslayer_Woven_Sleeves = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Woven_Sleeves");
			if (Dragonslayer_Woven_Sleeves == null)
			{
				Dragonslayer_Woven_Sleeves = new ItemTemplate();
				Dragonslayer_Woven_Sleeves.Name = "Dragonslayer Woven Sleeves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Woven_Sleeves.Name + ", creating it ...");
				Dragonslayer_Woven_Sleeves.Level = 51;
				Dragonslayer_Woven_Sleeves.Weight = 10;
				Dragonslayer_Woven_Sleeves.Model = 4101;
				Dragonslayer_Woven_Sleeves.Object_Type = 32;
				Dragonslayer_Woven_Sleeves.Item_Type = 28;
				Dragonslayer_Woven_Sleeves.Id_nb = "Dragonslayer_Woven_Sleeves";
				Dragonslayer_Woven_Sleeves.Hand = 0;
				Dragonslayer_Woven_Sleeves.IsPickable = true;
				Dragonslayer_Woven_Sleeves.IsDropable = true;
				Dragonslayer_Woven_Sleeves.IsTradable = true;
				Dragonslayer_Woven_Sleeves.CanDropAsLoot = true;
				Dragonslayer_Woven_Sleeves.Color = 0;
				Dragonslayer_Woven_Sleeves.Bonus = 35; // default bonus				
				Dragonslayer_Woven_Sleeves.Bonus1 = 5;
				Dragonslayer_Woven_Sleeves.Bonus1Type = (int) 12;
				Dragonslayer_Woven_Sleeves.Bonus2 = 5;
				Dragonslayer_Woven_Sleeves.Bonus2Type = (int) 18;
				Dragonslayer_Woven_Sleeves.Bonus3 = 40;
				Dragonslayer_Woven_Sleeves.Bonus3Type = (int) 10;
				Dragonslayer_Woven_Sleeves.Bonus4 = 5;
				Dragonslayer_Woven_Sleeves.Bonus4Type = (int) 15;
				Dragonslayer_Woven_Sleeves.Bonus5 = 22;
				Dragonslayer_Woven_Sleeves.Bonus5Type = (int) 156;
				Dragonslayer_Woven_Sleeves.Bonus6 = 1;
				Dragonslayer_Woven_Sleeves.Bonus6Type = (int) 191;
				Dragonslayer_Woven_Sleeves.Bonus7 = 5;
				Dragonslayer_Woven_Sleeves.Bonus7Type = (int) 209;
				Dragonslayer_Woven_Sleeves.Bonus8 = 0;
				Dragonslayer_Woven_Sleeves.Bonus8Type = (int) 0;
				Dragonslayer_Woven_Sleeves.Bonus9 = 0;
				Dragonslayer_Woven_Sleeves.Bonus9Type = (int) 0;
				Dragonslayer_Woven_Sleeves.Bonus10 = 0;
				Dragonslayer_Woven_Sleeves.Bonus10Type = (int) 0;
				Dragonslayer_Woven_Sleeves.ExtraBonus = 0;
				Dragonslayer_Woven_Sleeves.ExtraBonusType = (int) 0;
				Dragonslayer_Woven_Sleeves.Effect = 0;
				Dragonslayer_Woven_Sleeves.Emblem = 0;
				Dragonslayer_Woven_Sleeves.Charges = 0;
				Dragonslayer_Woven_Sleeves.MaxCharges = 0;
				Dragonslayer_Woven_Sleeves.SpellID = 0;
				Dragonslayer_Woven_Sleeves.ProcSpellID = 31001;
				Dragonslayer_Woven_Sleeves.Type_Damage = 0;
				Dragonslayer_Woven_Sleeves.Realm = 0;
				Dragonslayer_Woven_Sleeves.MaxCount = 1;
				Dragonslayer_Woven_Sleeves.PackSize = 1;
				Dragonslayer_Woven_Sleeves.Extension = 5;
				Dragonslayer_Woven_Sleeves.Quality = 100;				
				Dragonslayer_Woven_Sleeves.Condition = 50000;
				Dragonslayer_Woven_Sleeves.MaxCondition = 50000;
				Dragonslayer_Woven_Sleeves.Durability = 50000;
				Dragonslayer_Woven_Sleeves.MaxDurability = 0;
				Dragonslayer_Woven_Sleeves.PoisonCharges = 0;
				Dragonslayer_Woven_Sleeves.PoisonMaxCharges = 0;
				Dragonslayer_Woven_Sleeves.PoisonSpellID = 0;
				Dragonslayer_Woven_Sleeves.ProcSpellID1 = 40006;
				Dragonslayer_Woven_Sleeves.SpellID1 = 0;
				Dragonslayer_Woven_Sleeves.MaxCharges1 = 0;
				Dragonslayer_Woven_Sleeves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Woven_Sleeves);
				}
			Dragonslayer_Padded_Breeches_Hib = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Padded_Breeches_Hib");
			if (Dragonslayer_Padded_Breeches_Hib == null)
			{
				Dragonslayer_Padded_Breeches_Hib = new ItemTemplate();
				Dragonslayer_Padded_Breeches_Hib.Name = "Dragonslayer Padded Breeches";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Padded_Breeches_Hib.Name + ", creating it ...");
				Dragonslayer_Padded_Breeches_Hib.Level = 51;
				Dragonslayer_Padded_Breeches_Hib.Weight = 10;
				Dragonslayer_Padded_Breeches_Hib.Model = 4047;
				Dragonslayer_Padded_Breeches_Hib.Object_Type = 32;
				Dragonslayer_Padded_Breeches_Hib.Item_Type = 27;
				Dragonslayer_Padded_Breeches_Hib.Id_nb = "Dragonslayer_Padded_Breeches_Hib";
				Dragonslayer_Padded_Breeches_Hib.Hand = 0;
				Dragonslayer_Padded_Breeches_Hib.IsPickable = true;
				Dragonslayer_Padded_Breeches_Hib.IsDropable = true;
				Dragonslayer_Padded_Breeches_Hib.IsTradable = true;
				Dragonslayer_Padded_Breeches_Hib.CanDropAsLoot = true;
				Dragonslayer_Padded_Breeches_Hib.Color = 0;
				Dragonslayer_Padded_Breeches_Hib.Bonus = 35; // default bonus
				Dragonslayer_Padded_Breeches_Hib.Bonus1 = 22;
				Dragonslayer_Padded_Breeches_Hib.Bonus1Type = (int) 2;
				Dragonslayer_Padded_Breeches_Hib.Bonus2 = 5;
				Dragonslayer_Padded_Breeches_Hib.Bonus2Type = (int) 12;
				Dragonslayer_Padded_Breeches_Hib.Bonus3 = 5;
				Dragonslayer_Padded_Breeches_Hib.Bonus3Type = (int) 18;
				Dragonslayer_Padded_Breeches_Hib.Bonus4 = 40;
				Dragonslayer_Padded_Breeches_Hib.Bonus4Type = (int) 10;
				Dragonslayer_Padded_Breeches_Hib.Bonus5 = 5;
				Dragonslayer_Padded_Breeches_Hib.Bonus5Type = (int) 15;
				Dragonslayer_Padded_Breeches_Hib.Bonus6 = 5;
				Dragonslayer_Padded_Breeches_Hib.Bonus6Type = (int) 202;
				Dragonslayer_Padded_Breeches_Hib.Bonus7 = 1;
				Dragonslayer_Padded_Breeches_Hib.Bonus7Type = (int) 191;
				Dragonslayer_Padded_Breeches_Hib.Bonus8 = 0;
				Dragonslayer_Padded_Breeches_Hib.Bonus8Type = (int) 0;
				Dragonslayer_Padded_Breeches_Hib.Bonus9 = 0;
				Dragonslayer_Padded_Breeches_Hib.Bonus9Type = (int) 0;
				Dragonslayer_Padded_Breeches_Hib.Bonus10 = 0;
				Dragonslayer_Padded_Breeches_Hib.Bonus10Type = (int) 0;
				Dragonslayer_Padded_Breeches_Hib.ExtraBonus = 0;
				Dragonslayer_Padded_Breeches_Hib.ExtraBonusType = (int) 0;
				Dragonslayer_Padded_Breeches_Hib.Effect = 0;
				Dragonslayer_Padded_Breeches_Hib.Emblem = 0;
				Dragonslayer_Padded_Breeches_Hib.Charges = 0;
				Dragonslayer_Padded_Breeches_Hib.MaxCharges = 0;
				Dragonslayer_Padded_Breeches_Hib.SpellID = 0;
				Dragonslayer_Padded_Breeches_Hib.ProcSpellID = 31001;
				Dragonslayer_Padded_Breeches_Hib.Type_Damage = 0;
				Dragonslayer_Padded_Breeches_Hib.Realm = 2;
				Dragonslayer_Padded_Breeches_Hib.MaxCount = 1;
				Dragonslayer_Padded_Breeches_Hib.PackSize = 1;
				Dragonslayer_Padded_Breeches_Hib.Extension = 5;
				Dragonslayer_Padded_Breeches_Hib.Quality = 100;
				Dragonslayer_Padded_Breeches_Hib.Condition = 50000;
				Dragonslayer_Padded_Breeches_Hib.MaxCondition = 50000;
				Dragonslayer_Padded_Breeches_Hib.Durability = 50000;
				Dragonslayer_Padded_Breeches_Hib.MaxDurability = 0;
				Dragonslayer_Padded_Breeches_Hib.PoisonCharges = 0;
				Dragonslayer_Padded_Breeches_Hib.PoisonMaxCharges = 0;
				Dragonslayer_Padded_Breeches_Hib.PoisonSpellID = 0;
				Dragonslayer_Padded_Breeches_Hib.ProcSpellID1 = 40006;
				Dragonslayer_Padded_Breeches_Hib.SpellID1 = 0;
				Dragonslayer_Padded_Breeches_Hib.MaxCharges1 = 0;
				Dragonslayer_Padded_Breeches_Hib.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Padded_Breeches_Hib);
				}
			Dragonslayer_Woven_Full_Helm = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Woven_Full_Helm");
			if (Dragonslayer_Woven_Full_Helm == null)
			{
				Dragonslayer_Woven_Full_Helm = new ItemTemplate();
				Dragonslayer_Woven_Full_Helm.Name = "Dragonslayer Woven Full Helm";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Woven_Full_Helm.Name + ", creating it ...");
				Dragonslayer_Woven_Full_Helm.Level = 51;
				Dragonslayer_Woven_Full_Helm.Weight = 6;
				Dragonslayer_Woven_Full_Helm.Model = 4063;
				Dragonslayer_Woven_Full_Helm.Object_Type = 32;
				Dragonslayer_Woven_Full_Helm.Item_Type = 21;
				Dragonslayer_Woven_Full_Helm.Id_nb = "Dragonslayer_Woven_Full_Helm";
				Dragonslayer_Woven_Full_Helm.Hand = 0;
				Dragonslayer_Woven_Full_Helm.IsPickable = true;
				Dragonslayer_Woven_Full_Helm.IsDropable = true;
				Dragonslayer_Woven_Full_Helm.IsTradable = true;
				Dragonslayer_Woven_Full_Helm.CanDropAsLoot = true;
				Dragonslayer_Woven_Full_Helm.Color = 0;
				Dragonslayer_Woven_Full_Helm.Bonus = 35; // default bonus				
				Dragonslayer_Woven_Full_Helm.Bonus1 = 22;
				Dragonslayer_Woven_Full_Helm.Bonus1Type = (int) 3;
				Dragonslayer_Woven_Full_Helm.Bonus2 = 40;
				Dragonslayer_Woven_Full_Helm.Bonus2Type = (int) 10;
				Dragonslayer_Woven_Full_Helm.Bonus3 = 6;
				Dragonslayer_Woven_Full_Helm.Bonus3Type = (int) 11;
				Dragonslayer_Woven_Full_Helm.Bonus4 = 6;
				Dragonslayer_Woven_Full_Helm.Bonus4Type = (int) 14;
				Dragonslayer_Woven_Full_Helm.Bonus5 = 6;
				Dragonslayer_Woven_Full_Helm.Bonus5Type = (int) 16;
				Dragonslayer_Woven_Full_Helm.Bonus6 = 40;
				Dragonslayer_Woven_Full_Helm.Bonus6Type = (int) 210;
				Dragonslayer_Woven_Full_Helm.Bonus7 = 5;
				Dragonslayer_Woven_Full_Helm.Bonus7Type = (int) 148;
				Dragonslayer_Woven_Full_Helm.Bonus8 = 0;
				Dragonslayer_Woven_Full_Helm.Bonus8Type = (int) 0;
				Dragonslayer_Woven_Full_Helm.Bonus9 = 0;
				Dragonslayer_Woven_Full_Helm.Bonus9Type = (int) 0;
				Dragonslayer_Woven_Full_Helm.Bonus10 = 0;
				Dragonslayer_Woven_Full_Helm.Bonus10Type = (int) 0;
				Dragonslayer_Woven_Full_Helm.ExtraBonus = 0;
				Dragonslayer_Woven_Full_Helm.ExtraBonusType = (int) 0;
				Dragonslayer_Woven_Full_Helm.Effect = 0;
				Dragonslayer_Woven_Full_Helm.Emblem = 0;
				Dragonslayer_Woven_Full_Helm.Charges = 0;
				Dragonslayer_Woven_Full_Helm.MaxCharges = 0;
				Dragonslayer_Woven_Full_Helm.SpellID = 0;
				Dragonslayer_Woven_Full_Helm.ProcSpellID = 31001;
				Dragonslayer_Woven_Full_Helm.Type_Damage = 0;
				Dragonslayer_Woven_Full_Helm.Realm = 0;
				Dragonslayer_Woven_Full_Helm.MaxCount = 1;
				Dragonslayer_Woven_Full_Helm.PackSize = 1;
				Dragonslayer_Woven_Full_Helm.Extension = 0;
				Dragonslayer_Woven_Full_Helm.Quality = 100;				
				Dragonslayer_Woven_Full_Helm.Condition = 50000;
				Dragonslayer_Woven_Full_Helm.MaxCondition = 50000;
				Dragonslayer_Woven_Full_Helm.Durability = 50000;
				Dragonslayer_Woven_Full_Helm.MaxDurability = 0;
				Dragonslayer_Woven_Full_Helm.PoisonCharges = 0;
				Dragonslayer_Woven_Full_Helm.PoisonMaxCharges = 0;
				Dragonslayer_Woven_Full_Helm.PoisonSpellID = 0;
				Dragonslayer_Woven_Full_Helm.ProcSpellID1 = 40006;
				Dragonslayer_Woven_Full_Helm.SpellID1 = 0;
				Dragonslayer_Woven_Full_Helm.MaxCharges1 = 0;
				Dragonslayer_Woven_Full_Helm.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Woven_Full_Helm);
				}
			Dragonslayer_Woven_Slippers = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Woven_Slippers");
			if (Dragonslayer_Woven_Slippers == null)
			{
				Dragonslayer_Woven_Slippers = new ItemTemplate();
				Dragonslayer_Woven_Slippers.Name = "Dragonslayer Woven Slippers";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Woven_Slippers.Name + ", creating it ...");
				Dragonslayer_Woven_Slippers.Level = 51;
				Dragonslayer_Woven_Slippers.Weight = 6;
				Dragonslayer_Woven_Slippers.Model = 4103;
				Dragonslayer_Woven_Slippers.Object_Type = 32;
				Dragonslayer_Woven_Slippers.Item_Type = 23;
				Dragonslayer_Woven_Slippers.Id_nb = "Dragonslayer_Woven_Slippers";
				Dragonslayer_Woven_Slippers.Hand = 0;
				Dragonslayer_Woven_Slippers.IsPickable = true;
				Dragonslayer_Woven_Slippers.IsDropable = true;
				Dragonslayer_Woven_Slippers.IsTradable = true;
				Dragonslayer_Woven_Slippers.CanDropAsLoot = true;
				Dragonslayer_Woven_Slippers.Color = 0;
				Dragonslayer_Woven_Slippers.Bonus = 35; // default bonus				
				Dragonslayer_Woven_Slippers.Bonus1 = 18;
				Dragonslayer_Woven_Slippers.Bonus1Type = (int) 2;
				Dragonslayer_Woven_Slippers.Bonus2 = 6;
				Dragonslayer_Woven_Slippers.Bonus2Type = (int) 13;
				Dragonslayer_Woven_Slippers.Bonus3 = 6;
				Dragonslayer_Woven_Slippers.Bonus3Type = (int) 17;
				Dragonslayer_Woven_Slippers.Bonus4 = 6;
				Dragonslayer_Woven_Slippers.Bonus4Type = (int) 19;
				Dragonslayer_Woven_Slippers.Bonus5 = 18;
				Dragonslayer_Woven_Slippers.Bonus5Type = (int) 156;
				Dragonslayer_Woven_Slippers.Bonus6 = 6;
				Dragonslayer_Woven_Slippers.Bonus6Type = (int) 196;
				Dragonslayer_Woven_Slippers.Bonus7 = 0;
				Dragonslayer_Woven_Slippers.Bonus7Type = (int) 0;
				Dragonslayer_Woven_Slippers.Bonus8 = 0;
				Dragonslayer_Woven_Slippers.Bonus8Type = (int) 0;
				Dragonslayer_Woven_Slippers.Bonus9 = 0;
				Dragonslayer_Woven_Slippers.Bonus9Type = (int) 0;
				Dragonslayer_Woven_Slippers.Bonus10 = 0;
				Dragonslayer_Woven_Slippers.Bonus10Type = (int) 0;
				Dragonslayer_Woven_Slippers.ExtraBonus = 0;
				Dragonslayer_Woven_Slippers.ExtraBonusType = (int) 0;
				Dragonslayer_Woven_Slippers.Effect = 0;
				Dragonslayer_Woven_Slippers.Emblem = 0;
				Dragonslayer_Woven_Slippers.Charges = 0;
				Dragonslayer_Woven_Slippers.MaxCharges = 0;
				Dragonslayer_Woven_Slippers.SpellID = 0;
				Dragonslayer_Woven_Slippers.ProcSpellID = 31001;
				Dragonslayer_Woven_Slippers.Type_Damage = 0;
				Dragonslayer_Woven_Slippers.Realm = 0;
				Dragonslayer_Woven_Slippers.MaxCount = 1;
				Dragonslayer_Woven_Slippers.PackSize = 1;
				Dragonslayer_Woven_Slippers.Extension = 5;
				Dragonslayer_Woven_Slippers.Quality = 100;				
				Dragonslayer_Woven_Slippers.Condition = 50000;
				Dragonslayer_Woven_Slippers.MaxCondition = 50000;
				Dragonslayer_Woven_Slippers.Durability = 50000;
				Dragonslayer_Woven_Slippers.MaxDurability = 0;
				Dragonslayer_Woven_Slippers.PoisonCharges = 0;
				Dragonslayer_Woven_Slippers.PoisonMaxCharges = 0;
				Dragonslayer_Woven_Slippers.PoisonSpellID = 0;
				Dragonslayer_Woven_Slippers.ProcSpellID1 = 40006;
				Dragonslayer_Woven_Slippers.SpellID1 = 0;
				Dragonslayer_Woven_Slippers.MaxCharges1 = 0;
				Dragonslayer_Woven_Slippers.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Woven_Slippers);
				}
			Dragonslayer_Woven_Gloves = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Woven_Gloves");
			if (Dragonslayer_Woven_Gloves == null)
			{
				Dragonslayer_Woven_Gloves = new ItemTemplate();
				Dragonslayer_Woven_Gloves.Name = "Dragonslayer Woven Gloves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Woven_Gloves.Name + ", creating it ...");
				Dragonslayer_Woven_Gloves.Level = 51;
				Dragonslayer_Woven_Gloves.Weight = 6;
				Dragonslayer_Woven_Gloves.Model = 4102;
				Dragonslayer_Woven_Gloves.Object_Type = 32;
				Dragonslayer_Woven_Gloves.Item_Type = 22;
				Dragonslayer_Woven_Gloves.Id_nb = "Dragonslayer_Woven_Gloves";
				Dragonslayer_Woven_Gloves.Hand = 0;
				Dragonslayer_Woven_Gloves.IsPickable = true;
				Dragonslayer_Woven_Gloves.IsDropable = true;
				Dragonslayer_Woven_Gloves.IsTradable = true;
				Dragonslayer_Woven_Gloves.CanDropAsLoot = true;
				Dragonslayer_Woven_Gloves.Color = 0;
				Dragonslayer_Woven_Gloves.Bonus = 35; // default bonus				
				Dragonslayer_Woven_Gloves.Bonus1 = 5;
				Dragonslayer_Woven_Gloves.Bonus1Type = (int) 12;
				Dragonslayer_Woven_Gloves.Bonus2 = 5;
				Dragonslayer_Woven_Gloves.Bonus2Type = (int) 18;
				Dragonslayer_Woven_Gloves.Bonus3 = 40;
				Dragonslayer_Woven_Gloves.Bonus3Type = (int) 10;
				Dragonslayer_Woven_Gloves.Bonus4 = 5;
				Dragonslayer_Woven_Gloves.Bonus4Type = (int) 15;
				Dragonslayer_Woven_Gloves.Bonus5 = 3;
				Dragonslayer_Woven_Gloves.Bonus5Type = (int) 163;
				Dragonslayer_Woven_Gloves.Bonus6 = 6;
				Dragonslayer_Woven_Gloves.Bonus6Type = (int) 196;
				Dragonslayer_Woven_Gloves.Bonus7 = 40;
				Dragonslayer_Woven_Gloves.Bonus7Type = (int) 210;
				Dragonslayer_Woven_Gloves.Bonus8 = 0;
				Dragonslayer_Woven_Gloves.Bonus8Type = (int) 0;
				Dragonslayer_Woven_Gloves.Bonus9 = 0;
				Dragonslayer_Woven_Gloves.Bonus9Type = (int) 0;
				Dragonslayer_Woven_Gloves.Bonus10 = 0;
				Dragonslayer_Woven_Gloves.Bonus10Type = (int) 0;
				Dragonslayer_Woven_Gloves.ExtraBonus = 0;
				Dragonslayer_Woven_Gloves.ExtraBonusType = (int) 0;
				Dragonslayer_Woven_Gloves.Effect = 0;
				Dragonslayer_Woven_Gloves.Emblem = 0;
				Dragonslayer_Woven_Gloves.Charges = 0;
				Dragonslayer_Woven_Gloves.MaxCharges = 0;
				Dragonslayer_Woven_Gloves.SpellID = 0;
				Dragonslayer_Woven_Gloves.ProcSpellID = 31001;
				Dragonslayer_Woven_Gloves.Type_Damage = 0;
				Dragonslayer_Woven_Gloves.Realm = 0;
				Dragonslayer_Woven_Gloves.MaxCount = 1;
				Dragonslayer_Woven_Gloves.PackSize = 1;
				Dragonslayer_Woven_Gloves.Extension = 5;
				Dragonslayer_Woven_Gloves.Quality = 100;				
				Dragonslayer_Woven_Gloves.Condition = 50000;
				Dragonslayer_Woven_Gloves.MaxCondition = 50000;
				Dragonslayer_Woven_Gloves.Durability = 50000;
				Dragonslayer_Woven_Gloves.MaxDurability = 0;
				Dragonslayer_Woven_Gloves.PoisonCharges = 0;
				Dragonslayer_Woven_Gloves.PoisonMaxCharges = 0;
				Dragonslayer_Woven_Gloves.PoisonSpellID = 0;
				Dragonslayer_Woven_Gloves.ProcSpellID1 = 40006;
				Dragonslayer_Woven_Gloves.SpellID1 = 0;
				Dragonslayer_Woven_Gloves.MaxCharges1 = 0;
				Dragonslayer_Woven_Gloves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Woven_Gloves);
				}


            #endregion

            #region defineAreas

            #endregion

            #region defineQuestParts

            QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerBainsheeQuest));
            QuestBehaviour a;
            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.Interact, null, Lirazal);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerBainsheeQuest), Lirazal);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerBainsheeQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "I have lived in this village since I was a young girl. My father was a great dragon hunter you know.", Lirazal);
            a.AddAction(eActionType.Talk, "In my years I have learned to work with all kinds of problems, specially with [Dragons].", Lirazal);
            AddBehaviour(a);
         
            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.Whisper, "Dragons", Lirazal);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerBainsheeQuest), Lirazal);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerBainsheeQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "yes dragons, but there is a problem for me, i can't hunt dragons by self, [you know].", Lirazal);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.Whisper, "you know", Lirazal);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerBainsheeQuest), Lirazal);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerBainsheeQuest), null, (eComparator)5);
            a.AddAction(eActionType.OfferQuest, typeof(DragonslayerBainsheeQuest), "Helen has offered you the Dragonslayer quest.?Do you accept?");
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.DeclineQuest, null, typeof(DragonslayerBainsheeQuest));
            a.AddAction(eActionType.Talk, "No problem. See you", Lirazal);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.AcceptQuest, null, typeof(DragonslayerBainsheeQuest));
            a.AddAction(eActionType.Talk, "If you bring me the head of Asiintath I shall pay you well!", Lirazal);
            a.AddAction(eActionType.GiveQuest, typeof(DragonslayerBainsheeQuest), Lirazal);
            AddBehaviour(a);

            
						
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End
			if (Lirazal!=null) {
				Lirazal.AddQuestToGive(typeof (DragonslayerBainsheeQuest));
			}
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{			
				
			// Custom Scriptunloaded Code Begin
			
			// Custom Scriptunloaded Code End

			

			/* If Lirazal has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (Lirazal == null)
				return;
			/* Now we remove the possibility to give this quest to players */			
			Lirazal.RemoveQuestToGive(typeof (DragonslayerBainsheeQuest));
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
						return "[Step #1] Kill the Minidragon Asiintath and prove your strenght.When you got it come back to me with his HEAD";
					case 2:
						return "[Step #2] Bring the head of Asiintath to Lirazal";
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
			if (player.IsDoingQuest(typeof (DragonslayerBainsheeQuest)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
			if (
		
			player.CharacterClass.ID != (byte) eCharacterClass.Bainshee && 
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
