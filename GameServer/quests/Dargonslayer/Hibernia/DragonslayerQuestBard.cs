	
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
	public class DragonslayerBardQuest : BaseQuest
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

		protected const string questTitle = "DragonslayerBardQuest";

		protected const int minimumLevel = 49;
		protected const int maximumLevel = 50;
	
	
		private static GameNPC Lirazal = null;
		
		private static GameNPC Asiintath = null;
		
		private static ItemTemplate Asiintath_Head = null;
		
		private static ItemTemplate Dragonslayer_Dirge_Cailiocht_Jerkin = null;
		
		private static ItemTemplate Dragonslayer_Dirge_Cailiocht_Arms = null;
		
		private static ItemTemplate Dragonslayer_Dirge_Cailiocht_Full_Helm = null;
		
		private static ItemTemplate Dragonslayer_Dirge_Cailiocht_Gloves = null;
		
		private static ItemTemplate Dragonslayer_Dirge_Cailiocht_Boots = null;
		
		private static ItemTemplate Dragonslayer_Dirge_Cailiocht_Legs = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public DragonslayerBardQuest() : base()
		{
		}

		public DragonslayerBardQuest(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public DragonslayerBardQuest(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public DragonslayerBardQuest(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
	{
	}
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			QuestBehaviour a;
			QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerBardQuest));

			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(DragonslayerBardQuest)) == null)
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
						a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerBardQuest), null);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Dirge_Cailiocht_Jerkin, Lirazal);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Dirge_Cailiocht_Arms, Lirazal);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Dirge_Cailiocht_Full_Helm, Lirazal);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Dirge_Cailiocht_Gloves, Lirazal);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Dirge_Cailiocht_Boots, Lirazal);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Dirge_Cailiocht_Legs, Lirazal);
						a.AddAction(eActionType.FinishQuest, typeof(DragonslayerBardQuest), null);
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
			Dragonslayer_Dirge_Cailiocht_Jerkin = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Dirge_Cailiocht_Jerkin");
			if (Dragonslayer_Dirge_Cailiocht_Jerkin == null)
			{
				Dragonslayer_Dirge_Cailiocht_Jerkin = new ItemTemplate();
				Dragonslayer_Dirge_Cailiocht_Jerkin.Name = "Dragonslayer Dirge Cailiocht Jerkin";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Dirge_Cailiocht_Jerkin.Name + ", creating it ...");
				Dragonslayer_Dirge_Cailiocht_Jerkin.Level = 51;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Weight = 0;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Model = 4094;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Object_Type = 37;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Item_Type = 25;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Id_nb = "Dragonslayer_Dirge_Cailiocht_Jerkin";
				Dragonslayer_Dirge_Cailiocht_Jerkin.Hand = 0;
				Dragonslayer_Dirge_Cailiocht_Jerkin.IsPickable = true;
				Dragonslayer_Dirge_Cailiocht_Jerkin.IsDropable = true;
				Dragonslayer_Dirge_Cailiocht_Jerkin.IsTradable = true;
				Dragonslayer_Dirge_Cailiocht_Jerkin.CanDropAsLoot = true;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Color = 0;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Bonus = 35; // default bonus				
				Dragonslayer_Dirge_Cailiocht_Jerkin.Bonus1 = 22;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Bonus1Type = (int) 2;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Bonus2 = 5;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Bonus2Type = (int) 12;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Bonus3 = 5;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Bonus3Type = (int) 18;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Bonus4 = 5;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Bonus4Type = (int) 15;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Bonus5 = 22;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Bonus5Type = (int) 156;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Bonus6 = 5;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Bonus6Type = (int) 202;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Bonus7 = 2;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Bonus7Type = (int) 191;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Bonus8 = 5;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Bonus8Type = (int) 209;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Bonus9 = 0;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Bonus9Type = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Bonus10 = 0;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Bonus10Type = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Jerkin.ExtraBonus = 0;
				Dragonslayer_Dirge_Cailiocht_Jerkin.ExtraBonusType = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Effect = 0;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Emblem = 0;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Charges = 0;
				Dragonslayer_Dirge_Cailiocht_Jerkin.MaxCharges = 0;
				Dragonslayer_Dirge_Cailiocht_Jerkin.SpellID = 0;
				Dragonslayer_Dirge_Cailiocht_Jerkin.ProcSpellID = 31001;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Type_Damage = 0;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Realm = 0;
				Dragonslayer_Dirge_Cailiocht_Jerkin.MaxCount = 1;
				Dragonslayer_Dirge_Cailiocht_Jerkin.PackSize = 1;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Extension = 5;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Quality = 100;				
				Dragonslayer_Dirge_Cailiocht_Jerkin.Condition = 50000;
				Dragonslayer_Dirge_Cailiocht_Jerkin.MaxCondition = 50000;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Durability = 50000;
				Dragonslayer_Dirge_Cailiocht_Jerkin.MaxDurability = 0;
				Dragonslayer_Dirge_Cailiocht_Jerkin.PoisonCharges = 0;
				Dragonslayer_Dirge_Cailiocht_Jerkin.PoisonMaxCharges = 0;
				Dragonslayer_Dirge_Cailiocht_Jerkin.PoisonSpellID = 0;
				Dragonslayer_Dirge_Cailiocht_Jerkin.ProcSpellID1 = 40006;
				Dragonslayer_Dirge_Cailiocht_Jerkin.SpellID1 = 0;
				Dragonslayer_Dirge_Cailiocht_Jerkin.MaxCharges1 = 0;
				Dragonslayer_Dirge_Cailiocht_Jerkin.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Dirge_Cailiocht_Jerkin);
				}
			Dragonslayer_Dirge_Cailiocht_Arms = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Dirge_Cailiocht_Arms");
			if (Dragonslayer_Dirge_Cailiocht_Arms == null)
			{
				Dragonslayer_Dirge_Cailiocht_Arms = new ItemTemplate();
				Dragonslayer_Dirge_Cailiocht_Arms.Name = "Dragonslayer Dirge Cailiocht Arms";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Dirge_Cailiocht_Arms.Name + ", creating it ...");
				Dragonslayer_Dirge_Cailiocht_Arms.Level = 51;
				Dragonslayer_Dirge_Cailiocht_Arms.Weight = 0;
				Dragonslayer_Dirge_Cailiocht_Arms.Model = 4096;
				Dragonslayer_Dirge_Cailiocht_Arms.Object_Type = 37;
				Dragonslayer_Dirge_Cailiocht_Arms.Item_Type = 28;
				Dragonslayer_Dirge_Cailiocht_Arms.Id_nb = "Dragonslayer_Dirge_Cailiocht_Arms";
				Dragonslayer_Dirge_Cailiocht_Arms.Hand = 0;
				Dragonslayer_Dirge_Cailiocht_Arms.IsPickable = true;
				Dragonslayer_Dirge_Cailiocht_Arms.IsDropable = true;
				Dragonslayer_Dirge_Cailiocht_Arms.IsTradable = true;
				Dragonslayer_Dirge_Cailiocht_Arms.CanDropAsLoot = true;
				Dragonslayer_Dirge_Cailiocht_Arms.Color = 0;
				Dragonslayer_Dirge_Cailiocht_Arms.Bonus = 35; // default bonus				
				Dragonslayer_Dirge_Cailiocht_Arms.Bonus1 = 5;
				Dragonslayer_Dirge_Cailiocht_Arms.Bonus1Type = (int) 12;
				Dragonslayer_Dirge_Cailiocht_Arms.Bonus2 = 5;
				Dragonslayer_Dirge_Cailiocht_Arms.Bonus2Type = (int) 18;
				Dragonslayer_Dirge_Cailiocht_Arms.Bonus3 = 40;
				Dragonslayer_Dirge_Cailiocht_Arms.Bonus3Type = (int) 10;
				Dragonslayer_Dirge_Cailiocht_Arms.Bonus4 = 5;
				Dragonslayer_Dirge_Cailiocht_Arms.Bonus4Type = (int) 15;
				Dragonslayer_Dirge_Cailiocht_Arms.Bonus5 = 22;
				Dragonslayer_Dirge_Cailiocht_Arms.Bonus5Type = (int) 156;
				Dragonslayer_Dirge_Cailiocht_Arms.Bonus6 = 5;
				Dragonslayer_Dirge_Cailiocht_Arms.Bonus6Type = (int) 209;
				Dragonslayer_Dirge_Cailiocht_Arms.Bonus7 = 1;
				Dragonslayer_Dirge_Cailiocht_Arms.Bonus7Type = (int) 191;
				Dragonslayer_Dirge_Cailiocht_Arms.Bonus8 = 0;
				Dragonslayer_Dirge_Cailiocht_Arms.Bonus8Type = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Arms.Bonus9 = 0;
				Dragonslayer_Dirge_Cailiocht_Arms.Bonus9Type = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Arms.Bonus10 = 0;
				Dragonslayer_Dirge_Cailiocht_Arms.Bonus10Type = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Arms.ExtraBonus = 0;
				Dragonslayer_Dirge_Cailiocht_Arms.ExtraBonusType = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Arms.Effect = 0;
				Dragonslayer_Dirge_Cailiocht_Arms.Emblem = 0;
				Dragonslayer_Dirge_Cailiocht_Arms.Charges = 0;
				Dragonslayer_Dirge_Cailiocht_Arms.MaxCharges = 0;
				Dragonslayer_Dirge_Cailiocht_Arms.SpellID = 0;
				Dragonslayer_Dirge_Cailiocht_Arms.ProcSpellID = 31001;
				Dragonslayer_Dirge_Cailiocht_Arms.Type_Damage = 0;
				Dragonslayer_Dirge_Cailiocht_Arms.Realm = 0;
				Dragonslayer_Dirge_Cailiocht_Arms.MaxCount = 1;
				Dragonslayer_Dirge_Cailiocht_Arms.PackSize = 1;
				Dragonslayer_Dirge_Cailiocht_Arms.Extension = 5;
				Dragonslayer_Dirge_Cailiocht_Arms.Quality = 100;				
				Dragonslayer_Dirge_Cailiocht_Arms.Condition = 50000;
				Dragonslayer_Dirge_Cailiocht_Arms.MaxCondition = 50000;
				Dragonslayer_Dirge_Cailiocht_Arms.Durability = 50000;
				Dragonslayer_Dirge_Cailiocht_Arms.MaxDurability = 0;
				Dragonslayer_Dirge_Cailiocht_Arms.PoisonCharges = 0;
				Dragonslayer_Dirge_Cailiocht_Arms.PoisonMaxCharges = 0;
				Dragonslayer_Dirge_Cailiocht_Arms.PoisonSpellID = 0;
				Dragonslayer_Dirge_Cailiocht_Arms.ProcSpellID1 = 40006;
				Dragonslayer_Dirge_Cailiocht_Arms.SpellID1 = 0;
				Dragonslayer_Dirge_Cailiocht_Arms.MaxCharges1 = 0;
				Dragonslayer_Dirge_Cailiocht_Arms.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Dirge_Cailiocht_Arms);
				}
			Dragonslayer_Dirge_Cailiocht_Full_Helm = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Dirge_Cailiocht_Full_Helm");
			if (Dragonslayer_Dirge_Cailiocht_Full_Helm == null)
			{
				Dragonslayer_Dirge_Cailiocht_Full_Helm = new ItemTemplate();
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Name = "Dragonslayer Dirge Cailiocht Full Helm";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Dirge_Cailiocht_Full_Helm.Name + ", creating it ...");
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Level = 51;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Weight = 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Model = 4062;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Object_Type = 37;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Item_Type = 21;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Id_nb = "Dragonslayer_Dirge_Cailiocht_Full_Helm";
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Hand = 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.IsPickable = true;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.IsDropable = true;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.IsTradable = true;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.CanDropAsLoot = true;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Color = 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Bonus = 35; // default bonus				
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Bonus1 = 22;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Bonus1Type = (int) 3;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Bonus2 = 6;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Bonus2Type = (int) 16;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Bonus3 = 6;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Bonus3Type = (int) 11;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Bonus4 = 40;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Bonus4Type = (int) 10;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Bonus5 = 6;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Bonus5Type = (int) 14;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Bonus6 = 40;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Bonus6Type = (int) 210;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Bonus7 = 5;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Bonus7Type = (int) 148;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Bonus8 = 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Bonus8Type = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Bonus9 = 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Bonus9Type = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Bonus10 = 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Bonus10Type = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.ExtraBonus = 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.ExtraBonusType = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Effect = 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Emblem = 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Charges = 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.MaxCharges = 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.SpellID = 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.ProcSpellID = 31001;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Type_Damage = 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Realm = 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.MaxCount = 1;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.PackSize = 1;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Extension = 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Quality = 100;				
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Condition = 50000;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.MaxCondition = 50000;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Durability = 50000;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.MaxDurability = 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.PoisonCharges = 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.PoisonMaxCharges = 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.PoisonSpellID = 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.ProcSpellID1 = 40006;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.SpellID1 = 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.MaxCharges1 = 0;
				Dragonslayer_Dirge_Cailiocht_Full_Helm.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Dirge_Cailiocht_Full_Helm);
				}
			Dragonslayer_Dirge_Cailiocht_Gloves = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Dirge_Cailiocht_Gloves");
			if (Dragonslayer_Dirge_Cailiocht_Gloves == null)
			{
				Dragonslayer_Dirge_Cailiocht_Gloves = new ItemTemplate();
				Dragonslayer_Dirge_Cailiocht_Gloves.Name = "Dragonslayer Dirge Cailiocht Gloves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Dirge_Cailiocht_Gloves.Name + ", creating it ...");
				Dragonslayer_Dirge_Cailiocht_Gloves.Level = 51;
				Dragonslayer_Dirge_Cailiocht_Gloves.Weight = 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.Model = 4098;
				Dragonslayer_Dirge_Cailiocht_Gloves.Object_Type = 37;
				Dragonslayer_Dirge_Cailiocht_Gloves.Item_Type = 22;
				Dragonslayer_Dirge_Cailiocht_Gloves.Id_nb = "Dragonslayer_Dirge_Cailiocht_Gloves";
				Dragonslayer_Dirge_Cailiocht_Gloves.Hand = 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.IsPickable = true;
				Dragonslayer_Dirge_Cailiocht_Gloves.IsDropable = true;
				Dragonslayer_Dirge_Cailiocht_Gloves.IsTradable = true;
				Dragonslayer_Dirge_Cailiocht_Gloves.CanDropAsLoot = true;
				Dragonslayer_Dirge_Cailiocht_Gloves.Color = 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.Bonus = 35; // default bonus				
				Dragonslayer_Dirge_Cailiocht_Gloves.Bonus1 = 5;
				Dragonslayer_Dirge_Cailiocht_Gloves.Bonus1Type = (int) 12;
				Dragonslayer_Dirge_Cailiocht_Gloves.Bonus2 = 5;
				Dragonslayer_Dirge_Cailiocht_Gloves.Bonus2Type = (int) 18;
				Dragonslayer_Dirge_Cailiocht_Gloves.Bonus3 = 40;
				Dragonslayer_Dirge_Cailiocht_Gloves.Bonus3Type = (int) 10;
				Dragonslayer_Dirge_Cailiocht_Gloves.Bonus4 = 5;
				Dragonslayer_Dirge_Cailiocht_Gloves.Bonus4Type = (int) 15;
				Dragonslayer_Dirge_Cailiocht_Gloves.Bonus5 = 5;
				Dragonslayer_Dirge_Cailiocht_Gloves.Bonus5Type = (int) 195;
				Dragonslayer_Dirge_Cailiocht_Gloves.Bonus6 = 6;
				Dragonslayer_Dirge_Cailiocht_Gloves.Bonus6Type = (int) 196;
				Dragonslayer_Dirge_Cailiocht_Gloves.Bonus7 = 5;
				Dragonslayer_Dirge_Cailiocht_Gloves.Bonus7Type = (int) 190;
				Dragonslayer_Dirge_Cailiocht_Gloves.Bonus8 = 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.Bonus8Type = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.Bonus9 = 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.Bonus9Type = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.Bonus10 = 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.Bonus10Type = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.ExtraBonus = 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.ExtraBonusType = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.Effect = 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.Emblem = 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.Charges = 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.MaxCharges = 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.SpellID = 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.ProcSpellID = 31001;
				Dragonslayer_Dirge_Cailiocht_Gloves.Type_Damage = 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.Realm = 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.MaxCount = 1;
				Dragonslayer_Dirge_Cailiocht_Gloves.PackSize = 1;
				Dragonslayer_Dirge_Cailiocht_Gloves.Extension = 5;
				Dragonslayer_Dirge_Cailiocht_Gloves.Quality = 100;				
				Dragonslayer_Dirge_Cailiocht_Gloves.Condition = 50000;
				Dragonslayer_Dirge_Cailiocht_Gloves.MaxCondition = 50000;
				Dragonslayer_Dirge_Cailiocht_Gloves.Durability = 50000;
				Dragonslayer_Dirge_Cailiocht_Gloves.MaxDurability = 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.PoisonCharges = 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.PoisonMaxCharges = 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.PoisonSpellID = 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.ProcSpellID1 = 40006;
				Dragonslayer_Dirge_Cailiocht_Gloves.SpellID1 = 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.MaxCharges1 = 0;
				Dragonslayer_Dirge_Cailiocht_Gloves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Dirge_Cailiocht_Gloves);
				}
			Dragonslayer_Dirge_Cailiocht_Boots = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Dirge_Cailiocht_Boots");
			if (Dragonslayer_Dirge_Cailiocht_Boots == null)
			{
				Dragonslayer_Dirge_Cailiocht_Boots = new ItemTemplate();
				Dragonslayer_Dirge_Cailiocht_Boots.Name = "Dragonslayer Dirge Cailiocht Boots";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Dirge_Cailiocht_Boots.Name + ", creating it ...");
				Dragonslayer_Dirge_Cailiocht_Boots.Level = 51;
				Dragonslayer_Dirge_Cailiocht_Boots.Weight = 0;
				Dragonslayer_Dirge_Cailiocht_Boots.Model = 4097;
				Dragonslayer_Dirge_Cailiocht_Boots.Object_Type = 37;
				Dragonslayer_Dirge_Cailiocht_Boots.Item_Type = 23;
				Dragonslayer_Dirge_Cailiocht_Boots.Id_nb = "Dragonslayer_Dirge_Cailiocht_Boots";
				Dragonslayer_Dirge_Cailiocht_Boots.Hand = 0;
				Dragonslayer_Dirge_Cailiocht_Boots.IsPickable = true;
				Dragonslayer_Dirge_Cailiocht_Boots.IsDropable = true;
				Dragonslayer_Dirge_Cailiocht_Boots.IsTradable = true;
				Dragonslayer_Dirge_Cailiocht_Boots.CanDropAsLoot = true;
				Dragonslayer_Dirge_Cailiocht_Boots.Color = 0;
				Dragonslayer_Dirge_Cailiocht_Boots.Bonus = 35; // default bonus				
				Dragonslayer_Dirge_Cailiocht_Boots.Bonus1 = 18;
				Dragonslayer_Dirge_Cailiocht_Boots.Bonus1Type = (int) 2;
				Dragonslayer_Dirge_Cailiocht_Boots.Bonus2 = 6;
				Dragonslayer_Dirge_Cailiocht_Boots.Bonus2Type = (int) 13;
				Dragonslayer_Dirge_Cailiocht_Boots.Bonus3 = 6;
				Dragonslayer_Dirge_Cailiocht_Boots.Bonus3Type = (int) 17;
				Dragonslayer_Dirge_Cailiocht_Boots.Bonus4 = 6;
				Dragonslayer_Dirge_Cailiocht_Boots.Bonus4Type = (int) 19;
				Dragonslayer_Dirge_Cailiocht_Boots.Bonus5 = 18;
				Dragonslayer_Dirge_Cailiocht_Boots.Bonus5Type = (int) 156;
				Dragonslayer_Dirge_Cailiocht_Boots.Bonus6 = 6;
				Dragonslayer_Dirge_Cailiocht_Boots.Bonus6Type = (int) 196;
				Dragonslayer_Dirge_Cailiocht_Boots.Bonus7 = 0;
				Dragonslayer_Dirge_Cailiocht_Boots.Bonus7Type = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Boots.Bonus8 = 0;
				Dragonslayer_Dirge_Cailiocht_Boots.Bonus8Type = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Boots.Bonus9 = 0;
				Dragonslayer_Dirge_Cailiocht_Boots.Bonus9Type = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Boots.Bonus10 = 0;
				Dragonslayer_Dirge_Cailiocht_Boots.Bonus10Type = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Boots.ExtraBonus = 0;
				Dragonslayer_Dirge_Cailiocht_Boots.ExtraBonusType = (int) 0;
				Dragonslayer_Dirge_Cailiocht_Boots.Effect = 0;
				Dragonslayer_Dirge_Cailiocht_Boots.Emblem = 0;
				Dragonslayer_Dirge_Cailiocht_Boots.Charges = 0;
				Dragonslayer_Dirge_Cailiocht_Boots.MaxCharges = 0;
				Dragonslayer_Dirge_Cailiocht_Boots.SpellID = 0;
				Dragonslayer_Dirge_Cailiocht_Boots.ProcSpellID = 31001;
				Dragonslayer_Dirge_Cailiocht_Boots.Type_Damage = 0;
				Dragonslayer_Dirge_Cailiocht_Boots.Realm = 0;
				Dragonslayer_Dirge_Cailiocht_Boots.MaxCount = 1;
				Dragonslayer_Dirge_Cailiocht_Boots.PackSize = 1;
				Dragonslayer_Dirge_Cailiocht_Boots.Extension = 5;
				Dragonslayer_Dirge_Cailiocht_Boots.Quality = 100;				
				Dragonslayer_Dirge_Cailiocht_Boots.Condition = 50000;
				Dragonslayer_Dirge_Cailiocht_Boots.MaxCondition = 50000;
				Dragonslayer_Dirge_Cailiocht_Boots.Durability = 50000;
				Dragonslayer_Dirge_Cailiocht_Boots.MaxDurability = 0;
				Dragonslayer_Dirge_Cailiocht_Boots.PoisonCharges = 0;
				Dragonslayer_Dirge_Cailiocht_Boots.PoisonMaxCharges = 0;
				Dragonslayer_Dirge_Cailiocht_Boots.PoisonSpellID = 0;
				Dragonslayer_Dirge_Cailiocht_Boots.ProcSpellID1 = 40006;
				Dragonslayer_Dirge_Cailiocht_Boots.SpellID1 = 0;
				Dragonslayer_Dirge_Cailiocht_Boots.MaxCharges1 = 0;
				Dragonslayer_Dirge_Cailiocht_Boots.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Dirge_Cailiocht_Boots);
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


            #endregion

            #region defineAreas

            #endregion

            #region defineQuestParts

            QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerBardQuest));
            QuestBehaviour a;
            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.Interact, null, Lirazal);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerBardQuest), Lirazal);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerBardQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "I have lived in this village since I was a young girl. My father was a great dragon hunter you know.", Lirazal);
            a.AddAction(eActionType.Talk, "In my years I have learned to work with all kinds of problems, specially with [Dragons].", Lirazal);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.Whisper, "Dragons", Lirazal);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerBardQuest), Lirazal);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerBardQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "yes dragons, but there is a problem for me, i can't hunt dragons by self, [you know].", Lirazal);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.Whisper, "you know", Lirazal);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerBardQuest), Lirazal);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerBardQuest), null, (eComparator)5);
            a.AddAction(eActionType.OfferQuest, typeof(DragonslayerBardQuest), "Helen has offered you the Dragonslayer quest.?Do you accept?");
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.DeclineQuest, null, typeof(DragonslayerBardQuest));
            a.AddAction(eActionType.Talk, "No problem. See you", Lirazal);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.AcceptQuest, null, typeof(DragonslayerBardQuest));
            a.AddAction(eActionType.Talk, "If you bring me the head of Asiintath I shall pay you well!", Lirazal);
            a.AddAction(eActionType.GiveQuest, typeof(DragonslayerBardQuest), Lirazal);
            AddBehaviour(a);

			         			
			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End
			if (Lirazal!=null) {
				Lirazal.AddQuestToGive(typeof (DragonslayerBardQuest));
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
			Lirazal.RemoveQuestToGive(typeof (DragonslayerBardQuest));
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
			if (player.IsDoingQuest(typeof (DragonslayerBardQuest)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
			if (
		
			player.CharacterClass.ID != (byte) eCharacterClass.Bard && 
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
