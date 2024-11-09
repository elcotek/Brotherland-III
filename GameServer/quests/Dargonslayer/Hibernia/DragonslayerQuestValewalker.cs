	
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
	public class DragonslayerValewalkerQuest : BaseQuest
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

		protected const string questTitle = "DragonslayerValewalkerQuest";

		protected const int minimumLevel = 49;
		protected const int maximumLevel = 50;
	
	
		private static GameNPC Lirazal = null;
		
		private static GameNPC Asiintath = null;
		
		private static ItemTemplate Asiintath_Head = null;
		
		private static ItemTemplate Dragonslayer_Woven_Sleeves = null;
		
		private static ItemTemplate Dragonslayer_Woven_Full_Helm = null;
		
		private static ItemTemplate Dragonslayer_Woven_Slippers = null;
		
		private static ItemTemplate dragonslayer_moss_woven_vest = null;
		
		private static ItemTemplate dragonslayer_moss_woven_gloves = null;
		
		private static ItemTemplate dragonslayer_moss_padded_pants = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public DragonslayerValewalkerQuest() : base()
		{
		}

		public DragonslayerValewalkerQuest(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public DragonslayerValewalkerQuest(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public DragonslayerValewalkerQuest(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
	{
	}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			QuestBehaviour a;
			QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerValewalkerQuest));

			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(DragonslayerValewalkerQuest)) == null)
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
						a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerValewalkerQuest), null);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Woven_Sleeves, Lirazal);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Woven_Full_Helm, Lirazal);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Woven_Slippers, Lirazal);
						a.AddAction(eActionType.GiveItem, dragonslayer_moss_woven_vest, Lirazal);
						a.AddAction(eActionType.GiveItem, dragonslayer_moss_woven_gloves, Lirazal);
						a.AddAction(eActionType.GiveItem, dragonslayer_moss_padded_pants, Lirazal);
						a.AddAction(eActionType.FinishQuest, typeof(DragonslayerValewalkerQuest), null);
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
			dragonslayer_moss_woven_vest = GameServer.Database.FindObjectByKey<ItemTemplate>("dragonslayer_moss_woven_vest");
			if (dragonslayer_moss_woven_vest == null)
			{
				dragonslayer_moss_woven_vest = new ItemTemplate();
				dragonslayer_moss_woven_vest.Name = "Dragonslayer Moss Woven Vest";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dragonslayer_moss_woven_vest.Name + ", creating it ...");
				dragonslayer_moss_woven_vest.Level = 51;
				dragonslayer_moss_woven_vest.Weight = 20;
				dragonslayer_moss_woven_vest.Model = 4104;
				dragonslayer_moss_woven_vest.Object_Type = 32;
				dragonslayer_moss_woven_vest.Item_Type = 25;
				dragonslayer_moss_woven_vest.Id_nb = "dragonslayer_moss_woven_vest";
				dragonslayer_moss_woven_vest.Hand = 0;
				dragonslayer_moss_woven_vest.IsPickable = true;
				dragonslayer_moss_woven_vest.IsDropable = true;
				dragonslayer_moss_woven_vest.IsTradable = true;
				dragonslayer_moss_woven_vest.CanDropAsLoot = true;
				dragonslayer_moss_woven_vest.Color = 0;
				dragonslayer_moss_woven_vest.Bonus = 35; // default bonus				
				dragonslayer_moss_woven_vest.Bonus1 = 15;
				dragonslayer_moss_woven_vest.Bonus1Type = (int) 1;
				dragonslayer_moss_woven_vest.Bonus2 = 15;
				dragonslayer_moss_woven_vest.Bonus2Type = (int) 2;
				dragonslayer_moss_woven_vest.Bonus3 = 5;
				dragonslayer_moss_woven_vest.Bonus3Type = (int) 12;
				dragonslayer_moss_woven_vest.Bonus4 = 5;
				dragonslayer_moss_woven_vest.Bonus4Type = (int) 18;
				dragonslayer_moss_woven_vest.Bonus5 = 5;
				dragonslayer_moss_woven_vest.Bonus5Type = (int) 15;
				dragonslayer_moss_woven_vest.Bonus6 = 4;
				dragonslayer_moss_woven_vest.Bonus6Type = (int) 173;
				dragonslayer_moss_woven_vest.Bonus7 = 4;
				dragonslayer_moss_woven_vest.Bonus7Type = (int) 153;
				dragonslayer_moss_woven_vest.Bonus8 = 4;
				dragonslayer_moss_woven_vest.Bonus8Type = (int) 198;
				dragonslayer_moss_woven_vest.Bonus9 = 4;
				dragonslayer_moss_woven_vest.Bonus9Type = (int) 200;
				dragonslayer_moss_woven_vest.Bonus10 = 0;
				dragonslayer_moss_woven_vest.Bonus10Type = (int) 0;
				dragonslayer_moss_woven_vest.ExtraBonus = 0;
				dragonslayer_moss_woven_vest.ExtraBonusType = (int) 0;
				dragonslayer_moss_woven_vest.Effect = 0;
				dragonslayer_moss_woven_vest.Emblem = 0;
				dragonslayer_moss_woven_vest.Charges = 0;
				dragonslayer_moss_woven_vest.MaxCharges = 0;
				dragonslayer_moss_woven_vest.SpellID = 0;
				dragonslayer_moss_woven_vest.ProcSpellID = 31001;
				dragonslayer_moss_woven_vest.Type_Damage = 0;
				dragonslayer_moss_woven_vest.Realm = 0;
				dragonslayer_moss_woven_vest.MaxCount = 1;
				dragonslayer_moss_woven_vest.PackSize = 1;
				dragonslayer_moss_woven_vest.Extension = 5;
				dragonslayer_moss_woven_vest.Quality = 100;				
				dragonslayer_moss_woven_vest.Condition = 50000;
				dragonslayer_moss_woven_vest.MaxCondition = 50000;
				dragonslayer_moss_woven_vest.Durability = 50000;
				dragonslayer_moss_woven_vest.MaxDurability = 0;
				dragonslayer_moss_woven_vest.PoisonCharges = 0;
				dragonslayer_moss_woven_vest.PoisonMaxCharges = 0;
				dragonslayer_moss_woven_vest.PoisonSpellID = 0;
				dragonslayer_moss_woven_vest.ProcSpellID1 = 40006;
				dragonslayer_moss_woven_vest.SpellID1 = 0;
				dragonslayer_moss_woven_vest.MaxCharges1 = 0;
				dragonslayer_moss_woven_vest.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(dragonslayer_moss_woven_vest);
				}
			dragonslayer_moss_woven_gloves = GameServer.Database.FindObjectByKey<ItemTemplate>("dragonslayer_moss_woven_gloves");
			if (dragonslayer_moss_woven_gloves == null)
			{
				dragonslayer_moss_woven_gloves = new ItemTemplate();
				dragonslayer_moss_woven_gloves.Name = "Dragonslayer Moss Woven Gloves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dragonslayer_moss_woven_gloves.Name + ", creating it ...");
				dragonslayer_moss_woven_gloves.Level = 51;
				dragonslayer_moss_woven_gloves.Weight = 20;
				dragonslayer_moss_woven_gloves.Model = 4102;
				dragonslayer_moss_woven_gloves.Object_Type = 32;
				dragonslayer_moss_woven_gloves.Item_Type = 22;
				dragonslayer_moss_woven_gloves.Id_nb = "dragonslayer_moss_woven_gloves";
				dragonslayer_moss_woven_gloves.Hand = 0;
				dragonslayer_moss_woven_gloves.IsPickable = true;
				dragonslayer_moss_woven_gloves.IsDropable = true;
				dragonslayer_moss_woven_gloves.IsTradable = true;
				dragonslayer_moss_woven_gloves.CanDropAsLoot = true;
				dragonslayer_moss_woven_gloves.Color = 0;
				dragonslayer_moss_woven_gloves.Bonus = 35; // default bonus				
				dragonslayer_moss_woven_gloves.Bonus1 = 22;
				dragonslayer_moss_woven_gloves.Bonus1Type = (int) 1;
				dragonslayer_moss_woven_gloves.Bonus2 = 40;
				dragonslayer_moss_woven_gloves.Bonus2Type = (int) 10;
				dragonslayer_moss_woven_gloves.Bonus3 = 5;
				dragonslayer_moss_woven_gloves.Bonus3Type = (int) 12;
				dragonslayer_moss_woven_gloves.Bonus4 = 5;
				dragonslayer_moss_woven_gloves.Bonus4Type = (int) 18;
				dragonslayer_moss_woven_gloves.Bonus5 = 5;
				dragonslayer_moss_woven_gloves.Bonus5Type = (int) 15;
				dragonslayer_moss_woven_gloves.Bonus6 = 1;
				dragonslayer_moss_woven_gloves.Bonus6Type = (int) 155;
				dragonslayer_moss_woven_gloves.Bonus7 = 5;
				dragonslayer_moss_woven_gloves.Bonus7Type = (int) 201;
				dragonslayer_moss_woven_gloves.Bonus8 = 0;
				dragonslayer_moss_woven_gloves.Bonus8Type = (int) 0;
				dragonslayer_moss_woven_gloves.Bonus9 = 0;
				dragonslayer_moss_woven_gloves.Bonus9Type = (int) 0;
				dragonslayer_moss_woven_gloves.Bonus10 = 0;
				dragonslayer_moss_woven_gloves.Bonus10Type = (int) 0;
				dragonslayer_moss_woven_gloves.ExtraBonus = 0;
				dragonslayer_moss_woven_gloves.ExtraBonusType = (int) 0;
				dragonslayer_moss_woven_gloves.Effect = 0;
				dragonslayer_moss_woven_gloves.Emblem = 0;
				dragonslayer_moss_woven_gloves.Charges = 0;
				dragonslayer_moss_woven_gloves.MaxCharges = 0;
				dragonslayer_moss_woven_gloves.SpellID = 0;
				dragonslayer_moss_woven_gloves.ProcSpellID = 31001;
				dragonslayer_moss_woven_gloves.Type_Damage = 0;
				dragonslayer_moss_woven_gloves.Realm = 0;
				dragonslayer_moss_woven_gloves.MaxCount = 1;
				dragonslayer_moss_woven_gloves.PackSize = 1;
				dragonslayer_moss_woven_gloves.Extension = 5;
				dragonslayer_moss_woven_gloves.Quality = 100;				
				dragonslayer_moss_woven_gloves.Condition = 50000;
				dragonslayer_moss_woven_gloves.MaxCondition = 50000;
				dragonslayer_moss_woven_gloves.Durability = 50000;
				dragonslayer_moss_woven_gloves.MaxDurability = 0;
				dragonslayer_moss_woven_gloves.PoisonCharges = 0;
				dragonslayer_moss_woven_gloves.PoisonMaxCharges = 0;
				dragonslayer_moss_woven_gloves.PoisonSpellID = 0;
				dragonslayer_moss_woven_gloves.ProcSpellID1 = 40006;
				dragonslayer_moss_woven_gloves.SpellID1 = 0;
				dragonslayer_moss_woven_gloves.MaxCharges1 = 0;
				dragonslayer_moss_woven_gloves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(dragonslayer_moss_woven_gloves);
				}
			dragonslayer_moss_padded_pants = GameServer.Database.FindObjectByKey<ItemTemplate>("dragonslayer_moss_padded_pants");
			if (dragonslayer_moss_padded_pants == null)
			{
				dragonslayer_moss_padded_pants = new ItemTemplate();
				dragonslayer_moss_padded_pants.Name = "Dragonslayer Moss Padded Pants";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dragonslayer_moss_padded_pants.Name + ", creating it ...");
				dragonslayer_moss_padded_pants.Level = 51;
				dragonslayer_moss_padded_pants.Weight = 14;
				dragonslayer_moss_padded_pants.Model = 4100;
				dragonslayer_moss_padded_pants.Object_Type = 32;
				dragonslayer_moss_padded_pants.Item_Type = 27;
				dragonslayer_moss_padded_pants.Id_nb = "dragonslayer_moss_padded_pants";
				dragonslayer_moss_padded_pants.Hand = 0;
				dragonslayer_moss_padded_pants.IsPickable = true;
				dragonslayer_moss_padded_pants.IsDropable = true;
				dragonslayer_moss_padded_pants.IsTradable = true;
				dragonslayer_moss_padded_pants.CanDropAsLoot = true;
				dragonslayer_moss_padded_pants.Color = 0;
				dragonslayer_moss_padded_pants.Bonus = 35; // default bonus				
				dragonslayer_moss_padded_pants.Bonus1 = 22;
				dragonslayer_moss_padded_pants.Bonus1Type = (int) 2;
				dragonslayer_moss_padded_pants.Bonus2 = 5;
				dragonslayer_moss_padded_pants.Bonus2Type = (int) 12;
				dragonslayer_moss_padded_pants.Bonus3 = 5;
				dragonslayer_moss_padded_pants.Bonus3Type = (int) 18;
				dragonslayer_moss_padded_pants.Bonus4 = 5;
				dragonslayer_moss_padded_pants.Bonus4Type = (int) 15;
				dragonslayer_moss_padded_pants.Bonus5 = 40;
				dragonslayer_moss_padded_pants.Bonus5Type = (int) 10;
				dragonslayer_moss_padded_pants.Bonus6 = 1;
				dragonslayer_moss_padded_pants.Bonus6Type = (int) 155;
				dragonslayer_moss_padded_pants.Bonus7 = 5;
				dragonslayer_moss_padded_pants.Bonus7Type = (int) 202;
				dragonslayer_moss_padded_pants.Bonus8 = 0;
				dragonslayer_moss_padded_pants.Bonus8Type = (int) 0;
				dragonslayer_moss_padded_pants.Bonus9 = 0;
				dragonslayer_moss_padded_pants.Bonus9Type = (int) 0;
				dragonslayer_moss_padded_pants.Bonus10 = 0;
				dragonslayer_moss_padded_pants.Bonus10Type = (int) 0;
				dragonslayer_moss_padded_pants.ExtraBonus = 0;
				dragonslayer_moss_padded_pants.ExtraBonusType = (int) 0;
				dragonslayer_moss_padded_pants.Effect = 0;
				dragonslayer_moss_padded_pants.Emblem = 0;
				dragonslayer_moss_padded_pants.Charges = 0;
				dragonslayer_moss_padded_pants.MaxCharges = 0;
				dragonslayer_moss_padded_pants.SpellID = 0;
				dragonslayer_moss_padded_pants.ProcSpellID = 31001;
				dragonslayer_moss_padded_pants.Type_Damage = 0;
				dragonslayer_moss_padded_pants.Realm = 0;
				dragonslayer_moss_padded_pants.MaxCount = 1;
				dragonslayer_moss_padded_pants.PackSize = 1;
				dragonslayer_moss_padded_pants.Extension = 5;
				dragonslayer_moss_padded_pants.Quality = 100;				
				dragonslayer_moss_padded_pants.Condition = 50000;
				dragonslayer_moss_padded_pants.MaxCondition = 50000;
				dragonslayer_moss_padded_pants.Durability = 50000;
				dragonslayer_moss_padded_pants.MaxDurability = 0;
				dragonslayer_moss_padded_pants.PoisonCharges = 0;
				dragonslayer_moss_padded_pants.PoisonMaxCharges = 0;
				dragonslayer_moss_padded_pants.PoisonSpellID = 0;
				dragonslayer_moss_padded_pants.ProcSpellID1 = 40006;
				dragonslayer_moss_padded_pants.SpellID1 = 0;
				dragonslayer_moss_padded_pants.MaxCharges1 = 0;
				dragonslayer_moss_padded_pants.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(dragonslayer_moss_padded_pants);
				}


            #endregion

            #region defineAreas

            #endregion

            #region defineQuestParts

            QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerValewalkerQuest));
            QuestBehaviour a;
            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.Interact, null, Lirazal);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerValewalkerQuest), Lirazal);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerValewalkerQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "I have lived in this village since I was a young girl. My father was a great dragon hunter you know.", Lirazal);
            a.AddAction(eActionType.Talk, "In my years I have learned to work with all kinds of problems, specially with [Dragons].", Lirazal);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.Whisper, "Dragons", Lirazal);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerValewalkerQuest), Lirazal);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerValewalkerQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "yes dragons, but there is a problem for me, i can't hunt dragons by self, [you know].", Lirazal);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.Whisper, "you know", Lirazal);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerValewalkerQuest), Lirazal);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerValewalkerQuest), null, (eComparator)5);
            a.AddAction(eActionType.OfferQuest, typeof(DragonslayerValewalkerQuest), "Helen has offered you the Dragonslayer quest.?Do you accept?");
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.DeclineQuest, null, typeof(DragonslayerValewalkerQuest));
            a.AddAction(eActionType.Talk, "No problem. See you", Lirazal);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.AcceptQuest, null, typeof(DragonslayerValewalkerQuest));
            a.AddAction(eActionType.Talk, "If you bring me the head of Asiintath I shall pay you well!", Lirazal);
            a.AddAction(eActionType.GiveQuest, typeof(DragonslayerValewalkerQuest), Lirazal);
            AddBehaviour(a);

			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End
			if (Lirazal!=null) {
				Lirazal.AddQuestToGive(typeof (DragonslayerValewalkerQuest));
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
			Lirazal.RemoveQuestToGive(typeof (DragonslayerValewalkerQuest));
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
			if (player.IsDoingQuest(typeof (DragonslayerValewalkerQuest)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
			if (
		
			player.CharacterClass.ID != (byte) eCharacterClass.Valewalker && 
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
