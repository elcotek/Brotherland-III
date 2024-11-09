	
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
	public class DragonslayerNightshadeQuest : BaseQuest
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

		protected const string questTitle = "DragonslayerNightshadeQuest";

		protected const int minimumLevel = 49;
		protected const int maximumLevel = 50;
	
	
		private static GameNPC Lirazal = null;
		
		private static GameNPC Asiintath = null;
		
		private static ItemTemplate Asiintath_Head = null;
		
		private static ItemTemplate dragonslayer_soft_cruaigh_tunic = null;
		
		private static ItemTemplate dragonslayer_cruaigh_sleeves = null;
		
		private static ItemTemplate dragonslayer_shadowy_cruaigh_pants = null;
		
		private static ItemTemplate dragonslayer_soft_cruaigh_shoes = null;
		
		private static ItemTemplate dragonslayer_cruaigh_gloves = null;
		
		private static ItemTemplate dragonslayer_cruaigh_full_helm = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public DragonslayerNightshadeQuest() : base()
		{
		}

		public DragonslayerNightshadeQuest(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public DragonslayerNightshadeQuest(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public DragonslayerNightshadeQuest(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
	{
	}
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			QuestBehaviour a;
			QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerNightshadeQuest));

			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(DragonslayerNightshadeQuest)) == null)
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
						a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerNightshadeQuest), null);
						a.AddAction(eActionType.GiveItem, dragonslayer_soft_cruaigh_tunic, Lirazal);
						a.AddAction(eActionType.GiveItem, dragonslayer_cruaigh_sleeves, Lirazal);
						a.AddAction(eActionType.GiveItem, dragonslayer_shadowy_cruaigh_pants, Lirazal);
						a.AddAction(eActionType.GiveItem, dragonslayer_soft_cruaigh_shoes, Lirazal);
						a.AddAction(eActionType.GiveItem, dragonslayer_cruaigh_gloves, Lirazal);
						a.AddAction(eActionType.GiveItem, dragonslayer_cruaigh_full_helm, Lirazal);
						a.AddAction(eActionType.FinishQuest, typeof(DragonslayerNightshadeQuest), null);
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
			dragonslayer_soft_cruaigh_tunic = GameServer.Database.FindObjectByKey<ItemTemplate>("dragonslayer_soft_cruaigh_tunic");
			if (dragonslayer_soft_cruaigh_tunic == null)
			{
				dragonslayer_soft_cruaigh_tunic = new ItemTemplate();
				dragonslayer_soft_cruaigh_tunic.Name = "Dragonslayer Soft Cruaigh Tunic";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dragonslayer_soft_cruaigh_tunic.Name + ", creating it ...");
				dragonslayer_soft_cruaigh_tunic.Level = 51;
				dragonslayer_soft_cruaigh_tunic.Weight = 40;
				dragonslayer_soft_cruaigh_tunic.Model = 4074;
				dragonslayer_soft_cruaigh_tunic.Object_Type = 33;
				dragonslayer_soft_cruaigh_tunic.Item_Type = 25;
				dragonslayer_soft_cruaigh_tunic.Id_nb = "dragonslayer_soft_cruaigh_tunic";
				dragonslayer_soft_cruaigh_tunic.Hand = 0;
				dragonslayer_soft_cruaigh_tunic.IsPickable = true;
				dragonslayer_soft_cruaigh_tunic.IsDropable = true;
				dragonslayer_soft_cruaigh_tunic.IsTradable = true;
				dragonslayer_soft_cruaigh_tunic.CanDropAsLoot = true;
				dragonslayer_soft_cruaigh_tunic.Color = 0;
				dragonslayer_soft_cruaigh_tunic.Bonus = 35; // default bonus				
				dragonslayer_soft_cruaigh_tunic.Bonus1 = 15;
				dragonslayer_soft_cruaigh_tunic.Bonus1Type = (int) 1;
				dragonslayer_soft_cruaigh_tunic.Bonus2 = 15;
				dragonslayer_soft_cruaigh_tunic.Bonus2Type = (int) 2;
				dragonslayer_soft_cruaigh_tunic.Bonus3 = 5;
				dragonslayer_soft_cruaigh_tunic.Bonus3Type = (int) 12;
				dragonslayer_soft_cruaigh_tunic.Bonus4 = 5;
				dragonslayer_soft_cruaigh_tunic.Bonus4Type = (int) 18;
				dragonslayer_soft_cruaigh_tunic.Bonus5 = 5;
				dragonslayer_soft_cruaigh_tunic.Bonus5Type = (int) 15;
				dragonslayer_soft_cruaigh_tunic.Bonus6 = 4;
				dragonslayer_soft_cruaigh_tunic.Bonus6Type = (int) 153;
				dragonslayer_soft_cruaigh_tunic.Bonus7 = 4;
				dragonslayer_soft_cruaigh_tunic.Bonus7Type = (int) 173;
				dragonslayer_soft_cruaigh_tunic.Bonus8 = 4;
				dragonslayer_soft_cruaigh_tunic.Bonus8Type = (int) 200;
				dragonslayer_soft_cruaigh_tunic.Bonus9 = 4;
				dragonslayer_soft_cruaigh_tunic.Bonus9Type = (int) 198;
				dragonslayer_soft_cruaigh_tunic.Bonus10 = 0;
				dragonslayer_soft_cruaigh_tunic.Bonus10Type = (int) 0;
				dragonslayer_soft_cruaigh_tunic.ExtraBonus = 0;
				dragonslayer_soft_cruaigh_tunic.ExtraBonusType = (int) 0;
				dragonslayer_soft_cruaigh_tunic.Effect = 0;
				dragonslayer_soft_cruaigh_tunic.Emblem = 0;
				dragonslayer_soft_cruaigh_tunic.Charges = 0;
				dragonslayer_soft_cruaigh_tunic.MaxCharges = 0;
				dragonslayer_soft_cruaigh_tunic.SpellID = 0;
				dragonslayer_soft_cruaigh_tunic.ProcSpellID = 31001;
				dragonslayer_soft_cruaigh_tunic.Type_Damage = 0;
				dragonslayer_soft_cruaigh_tunic.Realm = 0;
				dragonslayer_soft_cruaigh_tunic.MaxCount = 1;
				dragonslayer_soft_cruaigh_tunic.PackSize = 1;
				dragonslayer_soft_cruaigh_tunic.Extension = 5;
				dragonslayer_soft_cruaigh_tunic.Quality = 100;				
				dragonslayer_soft_cruaigh_tunic.Condition = 50000;
				dragonslayer_soft_cruaigh_tunic.MaxCondition = 50000;
				dragonslayer_soft_cruaigh_tunic.Durability = 50000;
				dragonslayer_soft_cruaigh_tunic.MaxDurability = 0;
				dragonslayer_soft_cruaigh_tunic.PoisonCharges = 0;
				dragonslayer_soft_cruaigh_tunic.PoisonMaxCharges = 0;
				dragonslayer_soft_cruaigh_tunic.PoisonSpellID = 0;
				dragonslayer_soft_cruaigh_tunic.ProcSpellID1 = 40006;
				dragonslayer_soft_cruaigh_tunic.SpellID1 = 0;
				dragonslayer_soft_cruaigh_tunic.MaxCharges1 = 0;
				dragonslayer_soft_cruaigh_tunic.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(dragonslayer_soft_cruaigh_tunic);
				}
			dragonslayer_cruaigh_sleeves = GameServer.Database.FindObjectByKey<ItemTemplate>("dragonslayer_cruaigh_sleeves");
			if (dragonslayer_cruaigh_sleeves == null)
			{
				dragonslayer_cruaigh_sleeves = new ItemTemplate();
				dragonslayer_cruaigh_sleeves.Name = "Dragonslayer Cruaigh Sleeves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dragonslayer_cruaigh_sleeves.Name + ", creating it ...");
				dragonslayer_cruaigh_sleeves.Level = 51;
				dragonslayer_cruaigh_sleeves.Weight = 24;
				dragonslayer_cruaigh_sleeves.Model = 4076;
				dragonslayer_cruaigh_sleeves.Object_Type = 33;
				dragonslayer_cruaigh_sleeves.Item_Type = 28;
				dragonslayer_cruaigh_sleeves.Id_nb = "dragonslayer_cruaigh_sleeves";
				dragonslayer_cruaigh_sleeves.Hand = 0;
				dragonslayer_cruaigh_sleeves.IsPickable = true;
				dragonslayer_cruaigh_sleeves.IsDropable = true;
				dragonslayer_cruaigh_sleeves.IsTradable = true;
				dragonslayer_cruaigh_sleeves.CanDropAsLoot = true;
				dragonslayer_cruaigh_sleeves.Color = 0;
				dragonslayer_cruaigh_sleeves.Bonus = 35; // default bonus				
				dragonslayer_cruaigh_sleeves.Bonus1 = 22;
				dragonslayer_cruaigh_sleeves.Bonus1Type = (int) 1;
				dragonslayer_cruaigh_sleeves.Bonus2 = 40;
				dragonslayer_cruaigh_sleeves.Bonus2Type = (int) 10;
				dragonslayer_cruaigh_sleeves.Bonus3 = 5;
				dragonslayer_cruaigh_sleeves.Bonus3Type = (int) 12;
				dragonslayer_cruaigh_sleeves.Bonus4 = 5;
				dragonslayer_cruaigh_sleeves.Bonus4Type = (int) 18;
				dragonslayer_cruaigh_sleeves.Bonus5 = 5;
				dragonslayer_cruaigh_sleeves.Bonus5Type = (int) 15;
				dragonslayer_cruaigh_sleeves.Bonus6 = 5;
				dragonslayer_cruaigh_sleeves.Bonus6Type = (int) 201;
				dragonslayer_cruaigh_sleeves.Bonus7 = 1;
				dragonslayer_cruaigh_sleeves.Bonus7Type = (int) 155;
				dragonslayer_cruaigh_sleeves.Bonus8 = 0;
				dragonslayer_cruaigh_sleeves.Bonus8Type = (int) 0;
				dragonslayer_cruaigh_sleeves.Bonus9 = 0;
				dragonslayer_cruaigh_sleeves.Bonus9Type = (int) 0;
				dragonslayer_cruaigh_sleeves.Bonus10 = 0;
				dragonslayer_cruaigh_sleeves.Bonus10Type = (int) 0;
				dragonslayer_cruaigh_sleeves.ExtraBonus = 0;
				dragonslayer_cruaigh_sleeves.ExtraBonusType = (int) 0;
				dragonslayer_cruaigh_sleeves.Effect = 0;
				dragonslayer_cruaigh_sleeves.Emblem = 0;
				dragonslayer_cruaigh_sleeves.Charges = 0;
				dragonslayer_cruaigh_sleeves.MaxCharges = 0;
				dragonslayer_cruaigh_sleeves.SpellID = 0;
				dragonslayer_cruaigh_sleeves.ProcSpellID = 31001;
				dragonslayer_cruaigh_sleeves.Type_Damage = 0;
				dragonslayer_cruaigh_sleeves.Realm = 0;
				dragonslayer_cruaigh_sleeves.MaxCount = 1;
				dragonslayer_cruaigh_sleeves.PackSize = 1;
				dragonslayer_cruaigh_sleeves.Extension = 5;
				dragonslayer_cruaigh_sleeves.Quality = 100;				
				dragonslayer_cruaigh_sleeves.Condition = 50000;
				dragonslayer_cruaigh_sleeves.MaxCondition = 50000;
				dragonslayer_cruaigh_sleeves.Durability = 50000;
				dragonslayer_cruaigh_sleeves.MaxDurability = 0;
				dragonslayer_cruaigh_sleeves.PoisonCharges = 0;
				dragonslayer_cruaigh_sleeves.PoisonMaxCharges = 0;
				dragonslayer_cruaigh_sleeves.PoisonSpellID = 0;
				dragonslayer_cruaigh_sleeves.ProcSpellID1 = 40006;
				dragonslayer_cruaigh_sleeves.SpellID1 = 0;
				dragonslayer_cruaigh_sleeves.MaxCharges1 = 0;
				dragonslayer_cruaigh_sleeves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(dragonslayer_cruaigh_sleeves);
				}
			dragonslayer_shadowy_cruaigh_pants = GameServer.Database.FindObjectByKey<ItemTemplate>("dragonslayer_shadowy_cruaigh_pants");
			if (dragonslayer_shadowy_cruaigh_pants == null)
			{
				dragonslayer_shadowy_cruaigh_pants = new ItemTemplate();
				dragonslayer_shadowy_cruaigh_pants.Name = "Dragonslayer Shadowy Cruaigh Pants";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dragonslayer_shadowy_cruaigh_pants.Name + ", creating it ...");
				dragonslayer_shadowy_cruaigh_pants.Level = 51;
				dragonslayer_shadowy_cruaigh_pants.Weight = 20;
				dragonslayer_shadowy_cruaigh_pants.Model = 4075;
				dragonslayer_shadowy_cruaigh_pants.Object_Type = 33;
				dragonslayer_shadowy_cruaigh_pants.Item_Type = 27;
				dragonslayer_shadowy_cruaigh_pants.Id_nb = "dragonslayer_shadowy_cruaigh_pants";
				dragonslayer_shadowy_cruaigh_pants.Hand = 0;
				dragonslayer_shadowy_cruaigh_pants.IsPickable = true;
				dragonslayer_shadowy_cruaigh_pants.IsDropable = true;
				dragonslayer_shadowy_cruaigh_pants.IsTradable = true;
				dragonslayer_shadowy_cruaigh_pants.CanDropAsLoot = true;
				dragonslayer_shadowy_cruaigh_pants.Color = 0;
				dragonslayer_shadowy_cruaigh_pants.Bonus = 35; // default bonus				
				dragonslayer_shadowy_cruaigh_pants.Bonus1 = 22;
				dragonslayer_shadowy_cruaigh_pants.Bonus1Type = (int) 2;
				dragonslayer_shadowy_cruaigh_pants.Bonus2 = 5;
				dragonslayer_shadowy_cruaigh_pants.Bonus2Type = (int) 12;
				dragonslayer_shadowy_cruaigh_pants.Bonus3 = 5;
				dragonslayer_shadowy_cruaigh_pants.Bonus3Type = (int) 18;
				dragonslayer_shadowy_cruaigh_pants.Bonus4 = 5;
				dragonslayer_shadowy_cruaigh_pants.Bonus4Type = (int) 15;
				dragonslayer_shadowy_cruaigh_pants.Bonus5 = 40;
				dragonslayer_shadowy_cruaigh_pants.Bonus5Type = (int) 10;
				dragonslayer_shadowy_cruaigh_pants.Bonus6 = 1;
				dragonslayer_shadowy_cruaigh_pants.Bonus6Type = (int) 145;
				dragonslayer_shadowy_cruaigh_pants.Bonus7 = 5;
				dragonslayer_shadowy_cruaigh_pants.Bonus7Type = (int) 202;
				dragonslayer_shadowy_cruaigh_pants.Bonus8 = 0;
				dragonslayer_shadowy_cruaigh_pants.Bonus8Type = (int) 0;
				dragonslayer_shadowy_cruaigh_pants.Bonus9 = 0;
				dragonslayer_shadowy_cruaigh_pants.Bonus9Type = (int) 0;
				dragonslayer_shadowy_cruaigh_pants.Bonus10 = 0;
				dragonslayer_shadowy_cruaigh_pants.Bonus10Type = (int) 0;
				dragonslayer_shadowy_cruaigh_pants.ExtraBonus = 0;
				dragonslayer_shadowy_cruaigh_pants.ExtraBonusType = (int) 0;
				dragonslayer_shadowy_cruaigh_pants.Effect = 0;
				dragonslayer_shadowy_cruaigh_pants.Emblem = 0;
				dragonslayer_shadowy_cruaigh_pants.Charges = 0;
				dragonslayer_shadowy_cruaigh_pants.MaxCharges = 0;
				dragonslayer_shadowy_cruaigh_pants.SpellID = 0;
				dragonslayer_shadowy_cruaigh_pants.ProcSpellID = 31001;
				dragonslayer_shadowy_cruaigh_pants.Type_Damage = 0;
				dragonslayer_shadowy_cruaigh_pants.Realm = 0;
				dragonslayer_shadowy_cruaigh_pants.MaxCount = 1;
				dragonslayer_shadowy_cruaigh_pants.PackSize = 1;
				dragonslayer_shadowy_cruaigh_pants.Extension = 5;
				dragonslayer_shadowy_cruaigh_pants.Quality = 100;				
				dragonslayer_shadowy_cruaigh_pants.Condition = 50000;
				dragonslayer_shadowy_cruaigh_pants.MaxCondition = 50000;
				dragonslayer_shadowy_cruaigh_pants.Durability = 50000;
				dragonslayer_shadowy_cruaigh_pants.MaxDurability = 0;
				dragonslayer_shadowy_cruaigh_pants.PoisonCharges = 0;
				dragonslayer_shadowy_cruaigh_pants.PoisonMaxCharges = 0;
				dragonslayer_shadowy_cruaigh_pants.PoisonSpellID = 0;
				dragonslayer_shadowy_cruaigh_pants.ProcSpellID1 = 40006;
				dragonslayer_shadowy_cruaigh_pants.SpellID1 = 0;
				dragonslayer_shadowy_cruaigh_pants.MaxCharges1 = 0;
				dragonslayer_shadowy_cruaigh_pants.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(dragonslayer_shadowy_cruaigh_pants);
				}
			dragonslayer_soft_cruaigh_shoes = GameServer.Database.FindObjectByKey<ItemTemplate>("dragonslayer_soft_cruaigh_shoes");
			if (dragonslayer_soft_cruaigh_shoes == null)
			{
				dragonslayer_soft_cruaigh_shoes = new ItemTemplate();
				dragonslayer_soft_cruaigh_shoes.Name = "Dragonslayer Soft Cruaigh Shoes";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dragonslayer_soft_cruaigh_shoes.Name + ", creating it ...");
				dragonslayer_soft_cruaigh_shoes.Level = 51;
				dragonslayer_soft_cruaigh_shoes.Weight = 16;
				dragonslayer_soft_cruaigh_shoes.Model = 4078;
				dragonslayer_soft_cruaigh_shoes.Object_Type = 33;
				dragonslayer_soft_cruaigh_shoes.Item_Type = 23;
				dragonslayer_soft_cruaigh_shoes.Id_nb = "dragonslayer_soft_cruaigh_shoes";
				dragonslayer_soft_cruaigh_shoes.Hand = 0;
				dragonslayer_soft_cruaigh_shoes.IsPickable = true;
				dragonslayer_soft_cruaigh_shoes.IsDropable = true;
				dragonslayer_soft_cruaigh_shoes.IsTradable = true;
				dragonslayer_soft_cruaigh_shoes.CanDropAsLoot = true;
				dragonslayer_soft_cruaigh_shoes.Color = 0;
				dragonslayer_soft_cruaigh_shoes.Bonus = 35; // default bonus				
				dragonslayer_soft_cruaigh_shoes.Bonus1 = 20;
				dragonslayer_soft_cruaigh_shoes.Bonus1Type = (int) 10;
				dragonslayer_soft_cruaigh_shoes.Bonus2 = 6;
				dragonslayer_soft_cruaigh_shoes.Bonus2Type = (int) 13;
				dragonslayer_soft_cruaigh_shoes.Bonus3 = 6;
				dragonslayer_soft_cruaigh_shoes.Bonus3Type = (int) 17;
				dragonslayer_soft_cruaigh_shoes.Bonus4 = 6;
				dragonslayer_soft_cruaigh_shoes.Bonus4Type = (int) 19;
				dragonslayer_soft_cruaigh_shoes.Bonus5 = 3;
				dragonslayer_soft_cruaigh_shoes.Bonus5Type = (int) 49;
				dragonslayer_soft_cruaigh_shoes.Bonus6 = 3;
				dragonslayer_soft_cruaigh_shoes.Bonus6Type = (int) 23;
				dragonslayer_soft_cruaigh_shoes.Bonus7 = 3;
				dragonslayer_soft_cruaigh_shoes.Bonus7Type = (int) 164;
				dragonslayer_soft_cruaigh_shoes.Bonus8 = 5;
				dragonslayer_soft_cruaigh_shoes.Bonus8Type = (int) 148;
				dragonslayer_soft_cruaigh_shoes.Bonus9 = 0;
				dragonslayer_soft_cruaigh_shoes.Bonus9Type = (int) 0;
				dragonslayer_soft_cruaigh_shoes.Bonus10 = 0;
				dragonslayer_soft_cruaigh_shoes.Bonus10Type = (int) 0;
				dragonslayer_soft_cruaigh_shoes.ExtraBonus = 0;
				dragonslayer_soft_cruaigh_shoes.ExtraBonusType = (int) 0;
				dragonslayer_soft_cruaigh_shoes.Effect = 0;
				dragonslayer_soft_cruaigh_shoes.Emblem = 0;
				dragonslayer_soft_cruaigh_shoes.Charges = 0;
				dragonslayer_soft_cruaigh_shoes.MaxCharges = 0;
				dragonslayer_soft_cruaigh_shoes.SpellID = 0;
				dragonslayer_soft_cruaigh_shoes.ProcSpellID = 31001;
				dragonslayer_soft_cruaigh_shoes.Type_Damage = 0;
				dragonslayer_soft_cruaigh_shoes.Realm = 0;
				dragonslayer_soft_cruaigh_shoes.MaxCount = 1;
				dragonslayer_soft_cruaigh_shoes.PackSize = 1;
				dragonslayer_soft_cruaigh_shoes.Extension = 5;
				dragonslayer_soft_cruaigh_shoes.Quality = 100;				
				dragonslayer_soft_cruaigh_shoes.Condition = 50000;
				dragonslayer_soft_cruaigh_shoes.MaxCondition = 50000;
				dragonslayer_soft_cruaigh_shoes.Durability = 50000;
				dragonslayer_soft_cruaigh_shoes.MaxDurability = 0;
				dragonslayer_soft_cruaigh_shoes.PoisonCharges = 0;
				dragonslayer_soft_cruaigh_shoes.PoisonMaxCharges = 0;
				dragonslayer_soft_cruaigh_shoes.PoisonSpellID = 0;
				dragonslayer_soft_cruaigh_shoes.ProcSpellID1 = 40006;
				dragonslayer_soft_cruaigh_shoes.SpellID1 = 0;
				dragonslayer_soft_cruaigh_shoes.MaxCharges1 = 0;
				dragonslayer_soft_cruaigh_shoes.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(dragonslayer_soft_cruaigh_shoes);
				}
			dragonslayer_cruaigh_gloves = GameServer.Database.FindObjectByKey<ItemTemplate>("dragonslayer_cruaigh_gloves");
			if (dragonslayer_cruaigh_gloves == null)
			{
				dragonslayer_cruaigh_gloves = new ItemTemplate();
				dragonslayer_cruaigh_gloves.Name = "Dragonslayer Cruaigh Gloves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dragonslayer_cruaigh_gloves.Name + ", creating it ...");
				dragonslayer_cruaigh_gloves.Level = 51;
				dragonslayer_cruaigh_gloves.Weight = 24;
				dragonslayer_cruaigh_gloves.Model = 4077;
				dragonslayer_cruaigh_gloves.Object_Type = 33;
				dragonslayer_cruaigh_gloves.Item_Type = 22;
				dragonslayer_cruaigh_gloves.Id_nb = "dragonslayer_cruaigh_gloves";
				dragonslayer_cruaigh_gloves.Hand = 0;
				dragonslayer_cruaigh_gloves.IsPickable = true;
				dragonslayer_cruaigh_gloves.IsDropable = true;
				dragonslayer_cruaigh_gloves.IsTradable = true;
				dragonslayer_cruaigh_gloves.CanDropAsLoot = true;
				dragonslayer_cruaigh_gloves.Color = 0;
				dragonslayer_cruaigh_gloves.Bonus = 35; // default bonus				
				dragonslayer_cruaigh_gloves.Bonus1 = 45;
				dragonslayer_cruaigh_gloves.Bonus1Type = (int) 10;
				dragonslayer_cruaigh_gloves.Bonus2 = 5;
				dragonslayer_cruaigh_gloves.Bonus2Type = (int) 12;
				dragonslayer_cruaigh_gloves.Bonus3 = 5;
				dragonslayer_cruaigh_gloves.Bonus3Type = (int) 18;
				dragonslayer_cruaigh_gloves.Bonus4 = 5;
				dragonslayer_cruaigh_gloves.Bonus4Type = (int) 15;
				dragonslayer_cruaigh_gloves.Bonus5 = 3;
				dragonslayer_cruaigh_gloves.Bonus5Type = (int) 164;
				dragonslayer_cruaigh_gloves.Bonus6 = 10;
				dragonslayer_cruaigh_gloves.Bonus6Type = (int) 148;
				dragonslayer_cruaigh_gloves.Bonus7 = 45;
				dragonslayer_cruaigh_gloves.Bonus7Type = (int) 210;
				dragonslayer_cruaigh_gloves.Bonus8 = 0;
				dragonslayer_cruaigh_gloves.Bonus8Type = (int) 0;
				dragonslayer_cruaigh_gloves.Bonus9 = 0;
				dragonslayer_cruaigh_gloves.Bonus9Type = (int) 0;
				dragonslayer_cruaigh_gloves.Bonus10 = 0;
				dragonslayer_cruaigh_gloves.Bonus10Type = (int) 0;
				dragonslayer_cruaigh_gloves.ExtraBonus = 0;
				dragonslayer_cruaigh_gloves.ExtraBonusType = (int) 0;
				dragonslayer_cruaigh_gloves.Effect = 0;
				dragonslayer_cruaigh_gloves.Emblem = 0;
				dragonslayer_cruaigh_gloves.Charges = 0;
				dragonslayer_cruaigh_gloves.MaxCharges = 0;
				dragonslayer_cruaigh_gloves.SpellID = 0;
				dragonslayer_cruaigh_gloves.ProcSpellID = 31001;
				dragonslayer_cruaigh_gloves.Type_Damage = 0;
				dragonslayer_cruaigh_gloves.Realm = 0;
				dragonslayer_cruaigh_gloves.MaxCount = 1;
				dragonslayer_cruaigh_gloves.PackSize = 1;
				dragonslayer_cruaigh_gloves.Extension = 5;
				dragonslayer_cruaigh_gloves.Quality = 100;				
				dragonslayer_cruaigh_gloves.Condition = 50000;
				dragonslayer_cruaigh_gloves.MaxCondition = 50000;
				dragonslayer_cruaigh_gloves.Durability = 50000;
				dragonslayer_cruaigh_gloves.MaxDurability = 0;
				dragonslayer_cruaigh_gloves.PoisonCharges = 0;
				dragonslayer_cruaigh_gloves.PoisonMaxCharges = 0;
				dragonslayer_cruaigh_gloves.PoisonSpellID = 0;
				dragonslayer_cruaigh_gloves.ProcSpellID1 = 40006;
				dragonslayer_cruaigh_gloves.SpellID1 = 0;
				dragonslayer_cruaigh_gloves.MaxCharges1 = 0;
				dragonslayer_cruaigh_gloves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(dragonslayer_cruaigh_gloves);
				}
			dragonslayer_cruaigh_full_helm = GameServer.Database.FindObjectByKey<ItemTemplate>("dragonslayer_cruaigh_full_helm");
			if (dragonslayer_cruaigh_full_helm == null)
			{
				dragonslayer_cruaigh_full_helm = new ItemTemplate();
				dragonslayer_cruaigh_full_helm.Name = "Dragonslayer Cruaigh Full Helm";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dragonslayer_cruaigh_full_helm.Name + ", creating it ...");
				dragonslayer_cruaigh_full_helm.Level = 51;
				dragonslayer_cruaigh_full_helm.Weight = 16;
				dragonslayer_cruaigh_full_helm.Model = 4061;
				dragonslayer_cruaigh_full_helm.Object_Type = 33;
				dragonslayer_cruaigh_full_helm.Item_Type = 21;
				dragonslayer_cruaigh_full_helm.Id_nb = "dragonslayer_cruaigh_full_helm";
				dragonslayer_cruaigh_full_helm.Hand = 0;
				dragonslayer_cruaigh_full_helm.IsPickable = true;
				dragonslayer_cruaigh_full_helm.IsDropable = true;
				dragonslayer_cruaigh_full_helm.IsTradable = true;
				dragonslayer_cruaigh_full_helm.CanDropAsLoot = true;
				dragonslayer_cruaigh_full_helm.Color = 0;
				dragonslayer_cruaigh_full_helm.Bonus = 35; // default bonus				
				dragonslayer_cruaigh_full_helm.Bonus1 = 22;
				dragonslayer_cruaigh_full_helm.Bonus1Type = (int) 3;
				dragonslayer_cruaigh_full_helm.Bonus2 = 48;
				dragonslayer_cruaigh_full_helm.Bonus2Type = (int) 10;
				dragonslayer_cruaigh_full_helm.Bonus3 = 6;
				dragonslayer_cruaigh_full_helm.Bonus3Type = (int) 11;
				dragonslayer_cruaigh_full_helm.Bonus4 = 6;
				dragonslayer_cruaigh_full_helm.Bonus4Type = (int) 14;
				dragonslayer_cruaigh_full_helm.Bonus5 = 6;
				dragonslayer_cruaigh_full_helm.Bonus5Type = (int) 16;
				dragonslayer_cruaigh_full_helm.Bonus6 = 5;
				dragonslayer_cruaigh_full_helm.Bonus6Type = (int) 148;
				dragonslayer_cruaigh_full_helm.Bonus7 = 40;
				dragonslayer_cruaigh_full_helm.Bonus7Type = (int) 210;
				dragonslayer_cruaigh_full_helm.Bonus8 = 0;
				dragonslayer_cruaigh_full_helm.Bonus8Type = (int) 0;
				dragonslayer_cruaigh_full_helm.Bonus9 = 0;
				dragonslayer_cruaigh_full_helm.Bonus9Type = (int) 0;
				dragonslayer_cruaigh_full_helm.Bonus10 = 0;
				dragonslayer_cruaigh_full_helm.Bonus10Type = (int) 0;
				dragonslayer_cruaigh_full_helm.ExtraBonus = 0;
				dragonslayer_cruaigh_full_helm.ExtraBonusType = (int) 0;
				dragonslayer_cruaigh_full_helm.Effect = 0;
				dragonslayer_cruaigh_full_helm.Emblem = 0;
				dragonslayer_cruaigh_full_helm.Charges = 0;
				dragonslayer_cruaigh_full_helm.MaxCharges = 0;
				dragonslayer_cruaigh_full_helm.SpellID = 0;
				dragonslayer_cruaigh_full_helm.ProcSpellID = 31001;
				dragonslayer_cruaigh_full_helm.Type_Damage = 0;
				dragonslayer_cruaigh_full_helm.Realm = 0;
				dragonslayer_cruaigh_full_helm.MaxCount = 1;
				dragonslayer_cruaigh_full_helm.PackSize = 1;
				dragonslayer_cruaigh_full_helm.Extension = 0;
				dragonslayer_cruaigh_full_helm.Quality = 100;				
				dragonslayer_cruaigh_full_helm.Condition = 50000;
				dragonslayer_cruaigh_full_helm.MaxCondition = 50000;
				dragonslayer_cruaigh_full_helm.Durability = 50000;
				dragonslayer_cruaigh_full_helm.MaxDurability = 0;
				dragonslayer_cruaigh_full_helm.PoisonCharges = 0;
				dragonslayer_cruaigh_full_helm.PoisonMaxCharges = 0;
				dragonslayer_cruaigh_full_helm.PoisonSpellID = 0;
				dragonslayer_cruaigh_full_helm.ProcSpellID1 = 40006;
				dragonslayer_cruaigh_full_helm.SpellID1 = 0;
				dragonslayer_cruaigh_full_helm.MaxCharges1 = 0;
				dragonslayer_cruaigh_full_helm.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(dragonslayer_cruaigh_full_helm);
				}


            #endregion

            #region defineAreas

            #endregion

            #region defineQuestParts

            QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerNightshadeQuest));
            QuestBehaviour a;
            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.Interact, null, Lirazal);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerNightshadeQuest), Lirazal);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerNightshadeQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "I have lived in this village since I was a young girl. My father was a great dragon hunter you know.", Lirazal);
            a.AddAction(eActionType.Talk, "In my years I have learned to work with all kinds of problems, specially with [Dragons].", Lirazal);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.Whisper, "Dragons", Lirazal);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerNightshadeQuest), Lirazal);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerNightshadeQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "yes dragons, but there is a problem for me, i can't hunt dragons by self, [you know].", Lirazal);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.Whisper, "you know", Lirazal);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerNightshadeQuest), Lirazal);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerNightshadeQuest), null, (eComparator)5);
            a.AddAction(eActionType.OfferQuest, typeof(DragonslayerNightshadeQuest), "Helen has offered you the Dragonslayer quest.?Do you accept?");
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.DeclineQuest, null, typeof(DragonslayerNightshadeQuest));
            a.AddAction(eActionType.Talk, "No problem. See you", Lirazal);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.AcceptQuest, null, typeof(DragonslayerNightshadeQuest));
            a.AddAction(eActionType.Talk, "If you bring me the head of Asiintath I shall pay you well!", Lirazal);
            a.AddAction(eActionType.GiveQuest, typeof(DragonslayerNightshadeQuest), Lirazal);
            AddBehaviour(a);

         			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End
			if (Lirazal!=null) {
				Lirazal.AddQuestToGive(typeof (DragonslayerNightshadeQuest));
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
			Lirazal.RemoveQuestToGive(typeof (DragonslayerNightshadeQuest));
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
			if (player.IsDoingQuest(typeof (DragonslayerNightshadeQuest)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
			if (
		
			player.CharacterClass.ID != (byte) eCharacterClass.Nightshade && 
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
