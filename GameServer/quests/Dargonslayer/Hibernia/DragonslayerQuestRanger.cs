	
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
	public class DragonslayerRangerQuest : BaseQuest
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

		protected const string questTitle = "DragonslayerRangeQuest";

		protected const int minimumLevel = 49;
		protected const int maximumLevel = 50;
	
	
		private static GameNPC Lirazal = null;
		
		private static GameNPC Asiintath = null;
		
		private static ItemTemplate Asiintath_Head = null;
		
		private static ItemTemplate Dragonslayer_Dirge_Cailiocht_Legs = null;
		
		private static ItemTemplate dragonslayer_nature_cailiocht_gloves = null;
		
		private static ItemTemplate dragonslayer_nature_cailiocht_arms = null;
		
		private static ItemTemplate dragonslayer_nature_cailiocht_boots = null;
		
		private static ItemTemplate dragonslayer_cailiocht_full_helm = null;
		
		private static ItemTemplate dragonslayer_nature_cailiocht_jerkin = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public DragonslayerRangerQuest() : base()
		{
		}

		public DragonslayerRangerQuest(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public DragonslayerRangerQuest(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public DragonslayerRangerQuest(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
	{
	}
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			QuestBehaviour a;
			QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerRangerQuest));

			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(DragonslayerRangerQuest)) == null)
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
						a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerRangerQuest), null);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Dirge_Cailiocht_Legs, Lirazal);
						a.AddAction(eActionType.GiveItem, dragonslayer_nature_cailiocht_gloves, Lirazal);
						a.AddAction(eActionType.GiveItem, dragonslayer_nature_cailiocht_arms, Lirazal);
						a.AddAction(eActionType.GiveItem, dragonslayer_nature_cailiocht_boots, Lirazal);
						a.AddAction(eActionType.GiveItem, dragonslayer_cailiocht_full_helm, Lirazal);
						a.AddAction(eActionType.GiveItem, dragonslayer_nature_cailiocht_jerkin, Lirazal);
						a.AddAction(eActionType.FinishQuest, typeof(DragonslayerRangerQuest), null);
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
			Dragonslayer_Dirge_Cailiocht_Legs = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Dirge_Cailiocht_Legs");
			if (Dragonslayer_Dirge_Cailiocht_Legs == null)
			{
				Dragonslayer_Dirge_Cailiocht_Legs = new ItemTemplate();
				Dragonslayer_Dirge_Cailiocht_Legs.Name = "Dragonslayer Dirge Cailiocht Legs";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Dirge_Cailiocht_Legs.Name + ", creating it ...");
				Dragonslayer_Dirge_Cailiocht_Legs.Level = 51;
				Dragonslayer_Dirge_Cailiocht_Legs.Weight = 0;
				Dragonslayer_Dirge_Cailiocht_Legs.Model = 4095;
				Dragonslayer_Dirge_Cailiocht_Legs.Object_Type = 37;
				Dragonslayer_Dirge_Cailiocht_Legs.Item_Type = 27;
				Dragonslayer_Dirge_Cailiocht_Legs.Id_nb = "Dragonslayer_Dirge_Cailiocht_Legs";
				Dragonslayer_Dirge_Cailiocht_Legs.Hand = 0;
				Dragonslayer_Dirge_Cailiocht_Legs.IsPickable = true;
				Dragonslayer_Dirge_Cailiocht_Legs.IsDropable = true;
				Dragonslayer_Dirge_Cailiocht_Legs.IsTradable = true;
				Dragonslayer_Dirge_Cailiocht_Legs.CanDropAsLoot = true;
				Dragonslayer_Dirge_Cailiocht_Legs.Color = 0;
				Dragonslayer_Dirge_Cailiocht_Legs.Bonus = 35; // default bonus				
				Dragonslayer_Dirge_Cailiocht_Legs.Bonus1 = 22;
				Dragonslayer_Dirge_Cailiocht_Legs.Bonus1Type = (int) 2;
				Dragonslayer_Dirge_Cailiocht_Legs.Bonus2 = 5;
				Dragonslayer_Dirge_Cailiocht_Legs.Bonus2Type = (int) 12;
				Dragonslayer_Dirge_Cailiocht_Legs.Bonus3 = 5;
				Dragonslayer_Dirge_Cailiocht_Legs.Bonus3Type = (int) 18;
				Dragonslayer_Dirge_Cailiocht_Legs.Bonus4 = 40;
				Dragonslayer_Dirge_Cailiocht_Legs.Bonus4Type = (int) 10;
				Dragonslayer_Dirge_Cailiocht_Legs.Bonus5 = 5;
				Dragonslayer_Dirge_Cailiocht_Legs.Bonus5Type = (int) 15;
				Dragonslayer_Dirge_Cailiocht_Legs.Bonus6 = 1;
				Dragonslayer_Dirge_Cailiocht_Legs.Bonus6Type = (int) 191;
				Dragonslayer_Dirge_Cailiocht_Legs.Bonus7 = 5;
				Dragonslayer_Dirge_Cailiocht_Legs.Bonus7Type = (int) 202;
				Dragonslayer_Dirge_Cailiocht_Legs.Bonus8 = 0;
				Dragonslayer_Dirge_Cailiocht_Legs.Bonus8Type = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Legs.Bonus9 = 0;
				Dragonslayer_Dirge_Cailiocht_Legs.Bonus9Type = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Legs.Bonus10 = 0;
				Dragonslayer_Dirge_Cailiocht_Legs.Bonus10Type = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Legs.ExtraBonus = 0;
				Dragonslayer_Dirge_Cailiocht_Legs.ExtraBonusType = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Legs.Effect = 0;
				Dragonslayer_Dirge_Cailiocht_Legs.Emblem = 0;
				Dragonslayer_Dirge_Cailiocht_Legs.Charges = 0;
				Dragonslayer_Dirge_Cailiocht_Legs.MaxCharges = 0;
				Dragonslayer_Dirge_Cailiocht_Legs.SpellID = 0;
				Dragonslayer_Dirge_Cailiocht_Legs.ProcSpellID = 31001;
				Dragonslayer_Dirge_Cailiocht_Legs.Type_Damage = 0;
				Dragonslayer_Dirge_Cailiocht_Legs.Realm = 0;
				Dragonslayer_Dirge_Cailiocht_Legs.MaxCount = 1;
				Dragonslayer_Dirge_Cailiocht_Legs.PackSize = 1;
				Dragonslayer_Dirge_Cailiocht_Legs.Extension = 5;
				Dragonslayer_Dirge_Cailiocht_Legs.Quality = 100;				
				Dragonslayer_Dirge_Cailiocht_Legs.Condition = 50000;
				Dragonslayer_Dirge_Cailiocht_Legs.MaxCondition = 50000;
				Dragonslayer_Dirge_Cailiocht_Legs.Durability = 50000;
				Dragonslayer_Dirge_Cailiocht_Legs.MaxDurability = 0;
				Dragonslayer_Dirge_Cailiocht_Legs.PoisonCharges = 0;
				Dragonslayer_Dirge_Cailiocht_Legs.PoisonMaxCharges = 0;
				Dragonslayer_Dirge_Cailiocht_Legs.PoisonSpellID = 0;
				Dragonslayer_Dirge_Cailiocht_Legs.ProcSpellID1 = 40006;
				Dragonslayer_Dirge_Cailiocht_Legs.SpellID1 = 0;
				Dragonslayer_Dirge_Cailiocht_Legs.MaxCharges1 = 0;
				Dragonslayer_Dirge_Cailiocht_Legs.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Dirge_Cailiocht_Legs);
				}
			dragonslayer_nature_cailiocht_gloves = GameServer.Database.FindObjectByKey<ItemTemplate>("dragonslayer_nature_cailiocht_gloves");
			if (dragonslayer_nature_cailiocht_gloves == null)
			{
				dragonslayer_nature_cailiocht_gloves = new ItemTemplate();
				dragonslayer_nature_cailiocht_gloves.Name = "Dragonslayer Nature Cailiocht Gloves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dragonslayer_nature_cailiocht_gloves.Name + ", creating it ...");
				dragonslayer_nature_cailiocht_gloves.Level = 51;
				dragonslayer_nature_cailiocht_gloves.Weight = 24;
				dragonslayer_nature_cailiocht_gloves.Model = 4098;
				dragonslayer_nature_cailiocht_gloves.Object_Type = 37;
				dragonslayer_nature_cailiocht_gloves.Item_Type = 22;
				dragonslayer_nature_cailiocht_gloves.Id_nb = "dragonslayer_nature_cailiocht_gloves";
				dragonslayer_nature_cailiocht_gloves.Hand = 0;
				dragonslayer_nature_cailiocht_gloves.IsPickable = true;
				dragonslayer_nature_cailiocht_gloves.IsDropable = true;
				dragonslayer_nature_cailiocht_gloves.IsTradable = true;
				dragonslayer_nature_cailiocht_gloves.CanDropAsLoot = true;
				dragonslayer_nature_cailiocht_gloves.Color = 0;
				dragonslayer_nature_cailiocht_gloves.Bonus = 35; // default bonus				
				dragonslayer_nature_cailiocht_gloves.Bonus1 = 5;
				dragonslayer_nature_cailiocht_gloves.Bonus1Type = (int) 12;
				dragonslayer_nature_cailiocht_gloves.Bonus2 = 5;
				dragonslayer_nature_cailiocht_gloves.Bonus2Type = (int) 18;
				dragonslayer_nature_cailiocht_gloves.Bonus3 = 5;
				dragonslayer_nature_cailiocht_gloves.Bonus3Type = (int) 15;
				dragonslayer_nature_cailiocht_gloves.Bonus4 = 45;
				dragonslayer_nature_cailiocht_gloves.Bonus4Type = (int) 10;
				dragonslayer_nature_cailiocht_gloves.Bonus5 = 3;
				dragonslayer_nature_cailiocht_gloves.Bonus5Type = (int) 168;
				dragonslayer_nature_cailiocht_gloves.Bonus6 = 10;
				dragonslayer_nature_cailiocht_gloves.Bonus6Type = (int) 148;
				dragonslayer_nature_cailiocht_gloves.Bonus7 = 45;
				dragonslayer_nature_cailiocht_gloves.Bonus7Type = (int) 210;
				dragonslayer_nature_cailiocht_gloves.Bonus8 = 0;
				dragonslayer_nature_cailiocht_gloves.Bonus8Type = (int) 0;
				dragonslayer_nature_cailiocht_gloves.Bonus9 = 0;
				dragonslayer_nature_cailiocht_gloves.Bonus9Type = (int) 0;
				dragonslayer_nature_cailiocht_gloves.Bonus10 = 0;
				dragonslayer_nature_cailiocht_gloves.Bonus10Type = (int) 0;
				dragonslayer_nature_cailiocht_gloves.ExtraBonus = 0;
				dragonslayer_nature_cailiocht_gloves.ExtraBonusType = (int) 0;
				dragonslayer_nature_cailiocht_gloves.Effect = 0;
				dragonslayer_nature_cailiocht_gloves.Emblem = 0;
				dragonslayer_nature_cailiocht_gloves.Charges = 0;
				dragonslayer_nature_cailiocht_gloves.MaxCharges = 0;
				dragonslayer_nature_cailiocht_gloves.SpellID = 0;
				dragonslayer_nature_cailiocht_gloves.ProcSpellID = 31001;
				dragonslayer_nature_cailiocht_gloves.Type_Damage = 0;
				dragonslayer_nature_cailiocht_gloves.Realm = 0;
				dragonslayer_nature_cailiocht_gloves.MaxCount = 1;
				dragonslayer_nature_cailiocht_gloves.PackSize = 1;
				dragonslayer_nature_cailiocht_gloves.Extension = 5;
				dragonslayer_nature_cailiocht_gloves.Quality = 100;				
				dragonslayer_nature_cailiocht_gloves.Condition = 50000;
				dragonslayer_nature_cailiocht_gloves.MaxCondition = 50000;
				dragonslayer_nature_cailiocht_gloves.Durability = 50000;
				dragonslayer_nature_cailiocht_gloves.MaxDurability = 0;
				dragonslayer_nature_cailiocht_gloves.PoisonCharges = 0;
				dragonslayer_nature_cailiocht_gloves.PoisonMaxCharges = 0;
				dragonslayer_nature_cailiocht_gloves.PoisonSpellID = 0;
				dragonslayer_nature_cailiocht_gloves.ProcSpellID1 = 40006;
				dragonslayer_nature_cailiocht_gloves.SpellID1 = 0;
				dragonslayer_nature_cailiocht_gloves.MaxCharges1 = 0;
				dragonslayer_nature_cailiocht_gloves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(dragonslayer_nature_cailiocht_gloves);
				}
			dragonslayer_nature_cailiocht_arms = GameServer.Database.FindObjectByKey<ItemTemplate>("dragonslayer_nature_cailiocht_arms");
			if (dragonslayer_nature_cailiocht_arms == null)
			{
				dragonslayer_nature_cailiocht_arms = new ItemTemplate();
				dragonslayer_nature_cailiocht_arms.Name = "Dragonslayer Nature Cailiocht Arms";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dragonslayer_nature_cailiocht_arms.Name + ", creating it ...");
				dragonslayer_nature_cailiocht_arms.Level = 51;
				dragonslayer_nature_cailiocht_arms.Weight = 36;
				dragonslayer_nature_cailiocht_arms.Model = 4096;
				dragonslayer_nature_cailiocht_arms.Object_Type = 37;
				dragonslayer_nature_cailiocht_arms.Item_Type = 28;
				dragonslayer_nature_cailiocht_arms.Id_nb = "dragonslayer_nature_cailiocht_arms";
				dragonslayer_nature_cailiocht_arms.Hand = 0;
				dragonslayer_nature_cailiocht_arms.IsPickable = true;
				dragonslayer_nature_cailiocht_arms.IsDropable = true;
				dragonslayer_nature_cailiocht_arms.IsTradable = true;
				dragonslayer_nature_cailiocht_arms.CanDropAsLoot = true;
				dragonslayer_nature_cailiocht_arms.Color = 0;
				dragonslayer_nature_cailiocht_arms.Bonus = 35; // default bonus				
				dragonslayer_nature_cailiocht_arms.Bonus1 = 22;
				dragonslayer_nature_cailiocht_arms.Bonus1Type = (int) 2;
				dragonslayer_nature_cailiocht_arms.Bonus2 = 40;
				dragonslayer_nature_cailiocht_arms.Bonus2Type = (int) 10;
				dragonslayer_nature_cailiocht_arms.Bonus3 = 5;
				dragonslayer_nature_cailiocht_arms.Bonus3Type = (int) 12;
				dragonslayer_nature_cailiocht_arms.Bonus4 = 5;
				dragonslayer_nature_cailiocht_arms.Bonus4Type = (int) 18;
				dragonslayer_nature_cailiocht_arms.Bonus5 = 5;
				dragonslayer_nature_cailiocht_arms.Bonus5Type = (int) 15;
				dragonslayer_nature_cailiocht_arms.Bonus6 = 1;
				dragonslayer_nature_cailiocht_arms.Bonus6Type = (int) 191;
				dragonslayer_nature_cailiocht_arms.Bonus7 = 5;
				dragonslayer_nature_cailiocht_arms.Bonus7Type = (int) 202;
				dragonslayer_nature_cailiocht_arms.Bonus8 = 0;
				dragonslayer_nature_cailiocht_arms.Bonus8Type = (int) 0;
				dragonslayer_nature_cailiocht_arms.Bonus9 = 0;
				dragonslayer_nature_cailiocht_arms.Bonus9Type = (int) 0;
				dragonslayer_nature_cailiocht_arms.Bonus10 = 0;
				dragonslayer_nature_cailiocht_arms.Bonus10Type = (int) 0;
				dragonslayer_nature_cailiocht_arms.ExtraBonus = 0;
				dragonslayer_nature_cailiocht_arms.ExtraBonusType = (int) 0;
				dragonslayer_nature_cailiocht_arms.Effect = 0;
				dragonslayer_nature_cailiocht_arms.Emblem = 0;
				dragonslayer_nature_cailiocht_arms.Charges = 0;
				dragonslayer_nature_cailiocht_arms.MaxCharges = 0;
				dragonslayer_nature_cailiocht_arms.SpellID = 0;
				dragonslayer_nature_cailiocht_arms.ProcSpellID = 31001;
				dragonslayer_nature_cailiocht_arms.Type_Damage = 0;
				dragonslayer_nature_cailiocht_arms.Realm = 0;
				dragonslayer_nature_cailiocht_arms.MaxCount = 1;
				dragonslayer_nature_cailiocht_arms.PackSize = 1;
				dragonslayer_nature_cailiocht_arms.Extension = 5;
				dragonslayer_nature_cailiocht_arms.Quality = 100;				
				dragonslayer_nature_cailiocht_arms.Condition = 50000;
				dragonslayer_nature_cailiocht_arms.MaxCondition = 50000;
				dragonslayer_nature_cailiocht_arms.Durability = 50000;
				dragonslayer_nature_cailiocht_arms.MaxDurability = 0;
				dragonslayer_nature_cailiocht_arms.PoisonCharges = 0;
				dragonslayer_nature_cailiocht_arms.PoisonMaxCharges = 0;
				dragonslayer_nature_cailiocht_arms.PoisonSpellID = 0;
				dragonslayer_nature_cailiocht_arms.ProcSpellID1 = 40006;
				dragonslayer_nature_cailiocht_arms.SpellID1 = 0;
				dragonslayer_nature_cailiocht_arms.MaxCharges1 = 0;
				dragonslayer_nature_cailiocht_arms.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(dragonslayer_nature_cailiocht_arms);
				}
			dragonslayer_nature_cailiocht_boots = GameServer.Database.FindObjectByKey<ItemTemplate>("dragonslayer_nature_cailiocht_boots");
			if (dragonslayer_nature_cailiocht_boots == null)
			{
				dragonslayer_nature_cailiocht_boots = new ItemTemplate();
				dragonslayer_nature_cailiocht_boots.Name = "Dragonslayer Nature Cailiocht Boots";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dragonslayer_nature_cailiocht_boots.Name + ", creating it ...");
				dragonslayer_nature_cailiocht_boots.Level = 51;
				dragonslayer_nature_cailiocht_boots.Weight = 24;
				dragonslayer_nature_cailiocht_boots.Model = 4097;
				dragonslayer_nature_cailiocht_boots.Object_Type = 37;
				dragonslayer_nature_cailiocht_boots.Item_Type = 23;
				dragonslayer_nature_cailiocht_boots.Id_nb = "dragonslayer_nature_cailiocht_boots";
				dragonslayer_nature_cailiocht_boots.Hand = 0;
				dragonslayer_nature_cailiocht_boots.IsPickable = true;
				dragonslayer_nature_cailiocht_boots.IsDropable = true;
				dragonslayer_nature_cailiocht_boots.IsTradable = true;
				dragonslayer_nature_cailiocht_boots.CanDropAsLoot = true;
				dragonslayer_nature_cailiocht_boots.Color = 0;
				dragonslayer_nature_cailiocht_boots.Bonus = 35; // default bonus				
				dragonslayer_nature_cailiocht_boots.Bonus1 = 4;
				dragonslayer_nature_cailiocht_boots.Bonus1Type = (int) 49;
				dragonslayer_nature_cailiocht_boots.Bonus2 = 20;
				dragonslayer_nature_cailiocht_boots.Bonus2Type = (int) 10;
				dragonslayer_nature_cailiocht_boots.Bonus3 = 4;
				dragonslayer_nature_cailiocht_boots.Bonus3Type = (int) 168;
				dragonslayer_nature_cailiocht_boots.Bonus4 = 6;
				dragonslayer_nature_cailiocht_boots.Bonus4Type = (int) 13;
				dragonslayer_nature_cailiocht_boots.Bonus5 = 6;
				dragonslayer_nature_cailiocht_boots.Bonus5Type = (int) 17;
				dragonslayer_nature_cailiocht_boots.Bonus6 = 6;
				dragonslayer_nature_cailiocht_boots.Bonus6Type = (int) 19;
				dragonslayer_nature_cailiocht_boots.Bonus7 = 0;
				dragonslayer_nature_cailiocht_boots.Bonus7Type = (int) 0;
				dragonslayer_nature_cailiocht_boots.Bonus8 = 0;
				dragonslayer_nature_cailiocht_boots.Bonus8Type = (int) 0;
				dragonslayer_nature_cailiocht_boots.Bonus9 = 0;
				dragonslayer_nature_cailiocht_boots.Bonus9Type = (int) 0;
				dragonslayer_nature_cailiocht_boots.Bonus10 = 0;
				dragonslayer_nature_cailiocht_boots.Bonus10Type = (int) 0;
				dragonslayer_nature_cailiocht_boots.ExtraBonus = 0;
				dragonslayer_nature_cailiocht_boots.ExtraBonusType = (int) 0;
				dragonslayer_nature_cailiocht_boots.Effect = 0;
				dragonslayer_nature_cailiocht_boots.Emblem = 0;
				dragonslayer_nature_cailiocht_boots.Charges = 0;
				dragonslayer_nature_cailiocht_boots.MaxCharges = 0;
				dragonslayer_nature_cailiocht_boots.SpellID = 0;
				dragonslayer_nature_cailiocht_boots.ProcSpellID = 31001;
				dragonslayer_nature_cailiocht_boots.Type_Damage = 0;
				dragonslayer_nature_cailiocht_boots.Realm = 0;
				dragonslayer_nature_cailiocht_boots.MaxCount = 1;
				dragonslayer_nature_cailiocht_boots.PackSize = 1;
				dragonslayer_nature_cailiocht_boots.Extension = 5;
				dragonslayer_nature_cailiocht_boots.Quality = 100;				
				dragonslayer_nature_cailiocht_boots.Condition = 50000;
				dragonslayer_nature_cailiocht_boots.MaxCondition = 50000;
				dragonslayer_nature_cailiocht_boots.Durability = 50000;
				dragonslayer_nature_cailiocht_boots.MaxDurability = 0;
				dragonslayer_nature_cailiocht_boots.PoisonCharges = 0;
				dragonslayer_nature_cailiocht_boots.PoisonMaxCharges = 0;
				dragonslayer_nature_cailiocht_boots.PoisonSpellID = 0;
				dragonslayer_nature_cailiocht_boots.ProcSpellID1 = 40006;
				dragonslayer_nature_cailiocht_boots.SpellID1 = 0;
				dragonslayer_nature_cailiocht_boots.MaxCharges1 = 0;
				dragonslayer_nature_cailiocht_boots.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(dragonslayer_nature_cailiocht_boots);
				}
			dragonslayer_cailiocht_full_helm = GameServer.Database.FindObjectByKey<ItemTemplate>("dragonslayer_cailiocht_full_helm");
			if (dragonslayer_cailiocht_full_helm == null)
			{
				dragonslayer_cailiocht_full_helm = new ItemTemplate();
				dragonslayer_cailiocht_full_helm.Name = "Dragonslayer Cailiocht Full Helm";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dragonslayer_cailiocht_full_helm.Name + ", creating it ...");
				dragonslayer_cailiocht_full_helm.Level = 51;
				dragonslayer_cailiocht_full_helm.Weight = 24;
				dragonslayer_cailiocht_full_helm.Model = 4062;
				dragonslayer_cailiocht_full_helm.Object_Type = 37;
				dragonslayer_cailiocht_full_helm.Item_Type = 21;
				dragonslayer_cailiocht_full_helm.Id_nb = "dragonslayer_cailiocht_full_helm";
				dragonslayer_cailiocht_full_helm.Hand = 0;
				dragonslayer_cailiocht_full_helm.IsPickable = true;
				dragonslayer_cailiocht_full_helm.IsDropable = true;
				dragonslayer_cailiocht_full_helm.IsTradable = true;
				dragonslayer_cailiocht_full_helm.CanDropAsLoot = true;
				dragonslayer_cailiocht_full_helm.Color = 0;
				dragonslayer_cailiocht_full_helm.Bonus = 35; // default bonus				
				dragonslayer_cailiocht_full_helm.Bonus1 = 22;
				dragonslayer_cailiocht_full_helm.Bonus1Type = (int) 3;
				dragonslayer_cailiocht_full_helm.Bonus2 = 6;
				dragonslayer_cailiocht_full_helm.Bonus2Type = (int) 11;
				dragonslayer_cailiocht_full_helm.Bonus3 = 6;
				dragonslayer_cailiocht_full_helm.Bonus3Type = (int) 14;
				dragonslayer_cailiocht_full_helm.Bonus4 = 6;
				dragonslayer_cailiocht_full_helm.Bonus4Type = (int) 16;
				dragonslayer_cailiocht_full_helm.Bonus5 = 40;
				dragonslayer_cailiocht_full_helm.Bonus5Type = (int) 10;
				dragonslayer_cailiocht_full_helm.Bonus6 = 5;
				dragonslayer_cailiocht_full_helm.Bonus6Type = (int) 148;
				dragonslayer_cailiocht_full_helm.Bonus7 = 40;
				dragonslayer_cailiocht_full_helm.Bonus7Type = (int) 210;
				dragonslayer_cailiocht_full_helm.Bonus8 = 0;
				dragonslayer_cailiocht_full_helm.Bonus8Type = (int) 0;
				dragonslayer_cailiocht_full_helm.Bonus9 = 0;
				dragonslayer_cailiocht_full_helm.Bonus9Type = (int) 0;
				dragonslayer_cailiocht_full_helm.Bonus10 = 0;
				dragonslayer_cailiocht_full_helm.Bonus10Type = (int) 0;
				dragonslayer_cailiocht_full_helm.ExtraBonus = 0;
				dragonslayer_cailiocht_full_helm.ExtraBonusType = (int) 0;
				dragonslayer_cailiocht_full_helm.Effect = 0;
				dragonslayer_cailiocht_full_helm.Emblem = 0;
				dragonslayer_cailiocht_full_helm.Charges = 0;
				dragonslayer_cailiocht_full_helm.MaxCharges = 0;
				dragonslayer_cailiocht_full_helm.SpellID = 0;
				dragonslayer_cailiocht_full_helm.ProcSpellID = 31001;
				dragonslayer_cailiocht_full_helm.Type_Damage = 0;
				dragonslayer_cailiocht_full_helm.Realm = 0;
				dragonslayer_cailiocht_full_helm.MaxCount = 1;
				dragonslayer_cailiocht_full_helm.PackSize = 1;
				dragonslayer_cailiocht_full_helm.Extension = 0;
				dragonslayer_cailiocht_full_helm.Quality = 100;				
				dragonslayer_cailiocht_full_helm.Condition = 50000;
				dragonslayer_cailiocht_full_helm.MaxCondition = 50000;
				dragonslayer_cailiocht_full_helm.Durability = 50000;
				dragonslayer_cailiocht_full_helm.MaxDurability = 0;
				dragonslayer_cailiocht_full_helm.PoisonCharges = 0;
				dragonslayer_cailiocht_full_helm.PoisonMaxCharges = 0;
				dragonslayer_cailiocht_full_helm.PoisonSpellID = 0;
				dragonslayer_cailiocht_full_helm.ProcSpellID1 = 40006;
				dragonslayer_cailiocht_full_helm.SpellID1 = 0;
				dragonslayer_cailiocht_full_helm.MaxCharges1 = 0;
				dragonslayer_cailiocht_full_helm.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(dragonslayer_cailiocht_full_helm);
				}
			dragonslayer_nature_cailiocht_jerkin = GameServer.Database.FindObjectByKey<ItemTemplate>("dragonslayer_nature_cailiocht_jerkin");
			if (dragonslayer_nature_cailiocht_jerkin == null)
			{
				dragonslayer_nature_cailiocht_jerkin = new ItemTemplate();
				dragonslayer_nature_cailiocht_jerkin.Name = "Dragonslayer Nature Cailiocht Jerkin";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dragonslayer_nature_cailiocht_jerkin.Name + ", creating it ...");
				dragonslayer_nature_cailiocht_jerkin.Level = 51;
				dragonslayer_nature_cailiocht_jerkin.Weight = 38;
				dragonslayer_nature_cailiocht_jerkin.Model = 4094;
				dragonslayer_nature_cailiocht_jerkin.Object_Type = 37;
				dragonslayer_nature_cailiocht_jerkin.Item_Type = 25;
				dragonslayer_nature_cailiocht_jerkin.Id_nb = "dragonslayer_nature_cailiocht_jerkin";
				dragonslayer_nature_cailiocht_jerkin.Hand = 0;
				dragonslayer_nature_cailiocht_jerkin.IsPickable = true;
				dragonslayer_nature_cailiocht_jerkin.IsDropable = true;
				dragonslayer_nature_cailiocht_jerkin.IsTradable = true;
				dragonslayer_nature_cailiocht_jerkin.CanDropAsLoot = true;
				dragonslayer_nature_cailiocht_jerkin.Color = 0;
				dragonslayer_nature_cailiocht_jerkin.Bonus = 35; // default bonus				
				dragonslayer_nature_cailiocht_jerkin.Bonus1 = 22;
				dragonslayer_nature_cailiocht_jerkin.Bonus1Type = (int) 1;
				dragonslayer_nature_cailiocht_jerkin.Bonus2 = 22;
				dragonslayer_nature_cailiocht_jerkin.Bonus2Type = (int) 2;
				dragonslayer_nature_cailiocht_jerkin.Bonus3 = 5;
				dragonslayer_nature_cailiocht_jerkin.Bonus3Type = (int) 12;
				dragonslayer_nature_cailiocht_jerkin.Bonus4 = 5;
				dragonslayer_nature_cailiocht_jerkin.Bonus4Type = (int) 18;
				dragonslayer_nature_cailiocht_jerkin.Bonus5 = 5;
				dragonslayer_nature_cailiocht_jerkin.Bonus5Type = (int) 15;
				dragonslayer_nature_cailiocht_jerkin.Bonus6 = 2;
				dragonslayer_nature_cailiocht_jerkin.Bonus6Type = (int) 188;
				dragonslayer_nature_cailiocht_jerkin.Bonus7 = 5;
				dragonslayer_nature_cailiocht_jerkin.Bonus7Type = (int) 201;
				dragonslayer_nature_cailiocht_jerkin.Bonus8 = 5;
				dragonslayer_nature_cailiocht_jerkin.Bonus8Type = (int) 202;
				dragonslayer_nature_cailiocht_jerkin.Bonus9 = 0;
				dragonslayer_nature_cailiocht_jerkin.Bonus9Type = (int) 0;
				dragonslayer_nature_cailiocht_jerkin.Bonus10 = 0;
				dragonslayer_nature_cailiocht_jerkin.Bonus10Type = (int) 0;
				dragonslayer_nature_cailiocht_jerkin.ExtraBonus = 0;
				dragonslayer_nature_cailiocht_jerkin.ExtraBonusType = (int) 0;
				dragonslayer_nature_cailiocht_jerkin.Effect = 0;
				dragonslayer_nature_cailiocht_jerkin.Emblem = 0;
				dragonslayer_nature_cailiocht_jerkin.Charges = 0;
				dragonslayer_nature_cailiocht_jerkin.MaxCharges = 0;
				dragonslayer_nature_cailiocht_jerkin.SpellID = 0;
				dragonslayer_nature_cailiocht_jerkin.ProcSpellID = 31001;
				dragonslayer_nature_cailiocht_jerkin.Type_Damage = 0;
				dragonslayer_nature_cailiocht_jerkin.Realm = 0;
				dragonslayer_nature_cailiocht_jerkin.MaxCount = 1;
				dragonslayer_nature_cailiocht_jerkin.PackSize = 1;
				dragonslayer_nature_cailiocht_jerkin.Extension = 5;
				dragonslayer_nature_cailiocht_jerkin.Quality = 100;				
				dragonslayer_nature_cailiocht_jerkin.Condition = 50000;
				dragonslayer_nature_cailiocht_jerkin.MaxCondition = 50000;
				dragonslayer_nature_cailiocht_jerkin.Durability = 50000;
				dragonslayer_nature_cailiocht_jerkin.MaxDurability = 0;
				dragonslayer_nature_cailiocht_jerkin.PoisonCharges = 0;
				dragonslayer_nature_cailiocht_jerkin.PoisonMaxCharges = 0;
				dragonslayer_nature_cailiocht_jerkin.PoisonSpellID = 0;
				dragonslayer_nature_cailiocht_jerkin.ProcSpellID1 = 40006;
				dragonslayer_nature_cailiocht_jerkin.SpellID1 = 0;
				dragonslayer_nature_cailiocht_jerkin.MaxCharges1 = 0;
				dragonslayer_nature_cailiocht_jerkin.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(dragonslayer_nature_cailiocht_jerkin);
				}


            #endregion

            #region defineAreas

            #endregion

            #region defineQuestParts

            QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerRangerQuest));
            QuestBehaviour a;
            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.Interact, null, Lirazal);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerRangerQuest), Lirazal);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerRangerQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "I have lived in this village since I was a young girl. My father was a great dragon hunter you know.", Lirazal);
            a.AddAction(eActionType.Talk, "In my years I have learned to work with all kinds of problems, specially with [Dragons].", Lirazal);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.Whisper, "Dragons", Lirazal);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerRangerQuest), Lirazal);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerRangerQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "yes dragons, but there is a problem for me, i can't hunt dragons by self, [you know].", Lirazal);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.Whisper, "you know", Lirazal);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerRangerQuest), Lirazal);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerRangerQuest), null, (eComparator)5);
            a.AddAction(eActionType.OfferQuest, typeof(DragonslayerRangerQuest), "Helen has offered you the Dragonslayer quest.?Do you accept?");
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.DeclineQuest, null, typeof(DragonslayerRangerQuest));
            a.AddAction(eActionType.Talk, "No problem. See you", Lirazal);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.AcceptQuest, null, typeof(DragonslayerRangerQuest));
            a.AddAction(eActionType.Talk, "If you bring me the head of Asiintath I shall pay you well!", Lirazal);
            a.AddAction(eActionType.GiveQuest, typeof(DragonslayerRangerQuest), Lirazal);
            AddBehaviour(a);

			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End
			if (Lirazal!=null) {
				Lirazal.AddQuestToGive(typeof (DragonslayerRangerQuest));
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
			Lirazal.RemoveQuestToGive(typeof (DragonslayerRangerQuest));
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
			if (player.IsDoingQuest(typeof (DragonslayerRangerQuest)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
			if (
		
			player.CharacterClass.ID != (byte) eCharacterClass.Ranger && 
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
