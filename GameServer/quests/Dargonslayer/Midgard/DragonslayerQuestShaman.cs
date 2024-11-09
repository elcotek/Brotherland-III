	
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
	public class DragonslayerShamanQuest : BaseQuest
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

		protected const string questTitle = "DragonslayerShamanQuest";

		protected const int minimumLevel = 49;
		protected const int maximumLevel = 50;
	
	
		private static GameNPC Jarkkonith = null;
		
		private static GameNPC AmarasavaDas = null;
		
		private static ItemTemplate Jarkkonith_Head = null;
		
		private static ItemTemplate Dragonslayer_Seer_Starkakedja_Arms = null;
		
		private static ItemTemplate Dragonslayer_Seer_Starkakedja_Boots = null;
		
		private static ItemTemplate Dragonslayer_Seer_Starkakedja_Hauberk = null;
		
		private static ItemTemplate Dragonslayer_Seer_Starkakedja_Gloves = null;
		
		private static ItemTemplate Dragonslayer_Seer_Starkakedja_Chausses = null;
		
		private static ItemTemplate Dragonslayer_Seer_Starkakedja_Full_Helm = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public DragonslayerShamanQuest() : base()
		{
		}

		public DragonslayerShamanQuest(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public DragonslayerShamanQuest(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public DragonslayerShamanQuest(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
	{
	}
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			QuestBehaviour a;
			QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerShamanQuest));

			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(DragonslayerShamanQuest)) == null)
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
						a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerShamanQuest), null);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Seer_Starkakedja_Arms, AmarasavaDas);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Seer_Starkakedja_Full_Helm, AmarasavaDas);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Seer_Starkakedja_Boots, AmarasavaDas);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Seer_Starkakedja_Hauberk, AmarasavaDas);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Seer_Starkakedja_Gloves, AmarasavaDas);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Seer_Starkakedja_Chausses, AmarasavaDas);
						a.AddAction(eActionType.FinishQuest, typeof(DragonslayerShamanQuest), null);
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
	
			npcs = WorldMgr.GetNPCsByName("Jarkkonith", eRealm.None);
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
			Dragonslayer_Seer_Starkakedja_Arms = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Seer_Starkakedja_Arms");
			if (Dragonslayer_Seer_Starkakedja_Arms == null)
			{
				Dragonslayer_Seer_Starkakedja_Arms = new ItemTemplate();
				Dragonslayer_Seer_Starkakedja_Arms.Name = "Dragonslayer Seer Starkakedja Arms";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Seer_Starkakedja_Arms.Name + ", creating it ...");
				Dragonslayer_Seer_Starkakedja_Arms.Level = 51;
				Dragonslayer_Seer_Starkakedja_Arms.Weight = 38;
				Dragonslayer_Seer_Starkakedja_Arms.Model = 4028;
				Dragonslayer_Seer_Starkakedja_Arms.Object_Type = 35;
				Dragonslayer_Seer_Starkakedja_Arms.Item_Type = 28;
				Dragonslayer_Seer_Starkakedja_Arms.Id_nb = "Dragonslayer_Seer_Starkakedja_Arms";
				Dragonslayer_Seer_Starkakedja_Arms.Hand = 0;
    			Dragonslayer_Seer_Starkakedja_Arms.IsPickable = true;
				Dragonslayer_Seer_Starkakedja_Arms.IsDropable = true;
				Dragonslayer_Seer_Starkakedja_Arms.IsTradable = true;
				Dragonslayer_Seer_Starkakedja_Arms.CanDropAsLoot = true;
				Dragonslayer_Seer_Starkakedja_Arms.Color = 0;
				Dragonslayer_Seer_Starkakedja_Arms.Bonus = 35; // default bonus				
				Dragonslayer_Seer_Starkakedja_Arms.Bonus1 = 5;
				Dragonslayer_Seer_Starkakedja_Arms.Bonus1Type = (int) 12;
				Dragonslayer_Seer_Starkakedja_Arms.Bonus2 = 5;
				Dragonslayer_Seer_Starkakedja_Arms.Bonus2Type = (int) 18;
				Dragonslayer_Seer_Starkakedja_Arms.Bonus3 = 40;
				Dragonslayer_Seer_Starkakedja_Arms.Bonus3Type = (int) 10;
				Dragonslayer_Seer_Starkakedja_Arms.Bonus4 = 5;
				Dragonslayer_Seer_Starkakedja_Arms.Bonus4Type = (int) 15;
				Dragonslayer_Seer_Starkakedja_Arms.Bonus5 = 22;
				Dragonslayer_Seer_Starkakedja_Arms.Bonus5Type = (int) 156;
				Dragonslayer_Seer_Starkakedja_Arms.Bonus6 = 5;
				Dragonslayer_Seer_Starkakedja_Arms.Bonus6Type = (int) 209;
				Dragonslayer_Seer_Starkakedja_Arms.Bonus7 = 1;
				Dragonslayer_Seer_Starkakedja_Arms.Bonus7Type = (int) 191;
				Dragonslayer_Seer_Starkakedja_Arms.Bonus8 = 0;
				Dragonslayer_Seer_Starkakedja_Arms.Bonus8Type = (int) 0;
				Dragonslayer_Seer_Starkakedja_Arms.Bonus9 = 0;
				Dragonslayer_Seer_Starkakedja_Arms.Bonus9Type = (int) 0;
				Dragonslayer_Seer_Starkakedja_Arms.Bonus10 = 0;
				Dragonslayer_Seer_Starkakedja_Arms.Bonus10Type = (int) 0;
				Dragonslayer_Seer_Starkakedja_Arms.ExtraBonus = 0;
				Dragonslayer_Seer_Starkakedja_Arms.ExtraBonusType = (int) 0;
				Dragonslayer_Seer_Starkakedja_Arms.Effect = 0;
				Dragonslayer_Seer_Starkakedja_Arms.Emblem = 0;
				Dragonslayer_Seer_Starkakedja_Arms.Charges = 0;
				Dragonslayer_Seer_Starkakedja_Arms.MaxCharges = 0;
				Dragonslayer_Seer_Starkakedja_Arms.SpellID = 0;
				Dragonslayer_Seer_Starkakedja_Arms.ProcSpellID = 31001;
				Dragonslayer_Seer_Starkakedja_Arms.Type_Damage = 0;
				Dragonslayer_Seer_Starkakedja_Arms.Realm = 0;
				Dragonslayer_Seer_Starkakedja_Arms.MaxCount = 1;
				Dragonslayer_Seer_Starkakedja_Arms.PackSize = 1;
				Dragonslayer_Seer_Starkakedja_Arms.Extension = 5;
				Dragonslayer_Seer_Starkakedja_Arms.Quality = 100;				
				Dragonslayer_Seer_Starkakedja_Arms.Condition = 50000;
				Dragonslayer_Seer_Starkakedja_Arms.MaxCondition = 50000;
				Dragonslayer_Seer_Starkakedja_Arms.Durability = 50000;
				Dragonslayer_Seer_Starkakedja_Arms.MaxDurability = 0;
				Dragonslayer_Seer_Starkakedja_Arms.PoisonCharges = 0;
				Dragonslayer_Seer_Starkakedja_Arms.PoisonMaxCharges = 0;
				Dragonslayer_Seer_Starkakedja_Arms.PoisonSpellID = 0;
				Dragonslayer_Seer_Starkakedja_Arms.ProcSpellID1 = 40006;
				Dragonslayer_Seer_Starkakedja_Arms.SpellID1 = 0;
				Dragonslayer_Seer_Starkakedja_Arms.MaxCharges1 = 0;
				Dragonslayer_Seer_Starkakedja_Arms.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Seer_Starkakedja_Arms);
				}
			Dragonslayer_Seer_Starkakedja_Boots = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Seer_Starkakedja_Boots");
			if (Dragonslayer_Seer_Starkakedja_Boots == null)
			{
				Dragonslayer_Seer_Starkakedja_Boots = new ItemTemplate();
				Dragonslayer_Seer_Starkakedja_Boots.Name = "Dragonslayer Seer Starkakedja Boots";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Seer_Starkakedja_Boots.Name + ", creating it ...");
				Dragonslayer_Seer_Starkakedja_Boots.Level = 51;
				Dragonslayer_Seer_Starkakedja_Boots.Weight = 22;
				Dragonslayer_Seer_Starkakedja_Boots.Model = 4030;
				Dragonslayer_Seer_Starkakedja_Boots.Object_Type = 35;
				Dragonslayer_Seer_Starkakedja_Boots.Item_Type = 23;
				Dragonslayer_Seer_Starkakedja_Boots.Id_nb = "Dragonslayer_Seer_Starkakedja_Boots";
				Dragonslayer_Seer_Starkakedja_Boots.Hand = 0;
				Dragonslayer_Seer_Starkakedja_Boots.IsPickable = true;
				Dragonslayer_Seer_Starkakedja_Boots.IsDropable = true;
				Dragonslayer_Seer_Starkakedja_Boots.IsTradable = true;
				Dragonslayer_Seer_Starkakedja_Boots.CanDropAsLoot = true;
				Dragonslayer_Seer_Starkakedja_Boots.Color = 0;
				Dragonslayer_Seer_Starkakedja_Boots.Bonus = 35; // default bonus				
				Dragonslayer_Seer_Starkakedja_Boots.Bonus1 = 18;
				Dragonslayer_Seer_Starkakedja_Boots.Bonus1Type = (int) 2;
				Dragonslayer_Seer_Starkakedja_Boots.Bonus2 = 6;
				Dragonslayer_Seer_Starkakedja_Boots.Bonus2Type = (int) 13;
				Dragonslayer_Seer_Starkakedja_Boots.Bonus3 = 6;
				Dragonslayer_Seer_Starkakedja_Boots.Bonus3Type = (int) 17;
				Dragonslayer_Seer_Starkakedja_Boots.Bonus4 = 6;
				Dragonslayer_Seer_Starkakedja_Boots.Bonus4Type = (int) 19;
				Dragonslayer_Seer_Starkakedja_Boots.Bonus5 = 18;
				Dragonslayer_Seer_Starkakedja_Boots.Bonus5Type = (int) 156;
				Dragonslayer_Seer_Starkakedja_Boots.Bonus6 = 6;
				Dragonslayer_Seer_Starkakedja_Boots.Bonus6Type = (int) 196;
				Dragonslayer_Seer_Starkakedja_Boots.Bonus7 = 0;
				Dragonslayer_Seer_Starkakedja_Boots.Bonus7Type = (int) 0;
				Dragonslayer_Seer_Starkakedja_Boots.Bonus8 = 0;
				Dragonslayer_Seer_Starkakedja_Boots.Bonus8Type = (int) 0;
				Dragonslayer_Seer_Starkakedja_Boots.Bonus9 = 0;
				Dragonslayer_Seer_Starkakedja_Boots.Bonus9Type = (int) 0;
				Dragonslayer_Seer_Starkakedja_Boots.Bonus10 = 0;
				Dragonslayer_Seer_Starkakedja_Boots.Bonus10Type = (int) 0;
				Dragonslayer_Seer_Starkakedja_Boots.ExtraBonus = 0;
				Dragonslayer_Seer_Starkakedja_Boots.ExtraBonusType = (int) 0;
				Dragonslayer_Seer_Starkakedja_Boots.Effect = 0;
				Dragonslayer_Seer_Starkakedja_Boots.Emblem = 0;
				Dragonslayer_Seer_Starkakedja_Boots.Charges = 0;
				Dragonslayer_Seer_Starkakedja_Boots.MaxCharges = 0;
				Dragonslayer_Seer_Starkakedja_Boots.SpellID = 0;
				Dragonslayer_Seer_Starkakedja_Boots.ProcSpellID = 31001;
				Dragonslayer_Seer_Starkakedja_Boots.Type_Damage = 0;
				Dragonslayer_Seer_Starkakedja_Boots.Realm = 0;
				Dragonslayer_Seer_Starkakedja_Boots.MaxCount = 1;
				Dragonslayer_Seer_Starkakedja_Boots.PackSize = 1;
				Dragonslayer_Seer_Starkakedja_Boots.Extension = 5;
				Dragonslayer_Seer_Starkakedja_Boots.Quality = 100;				
				Dragonslayer_Seer_Starkakedja_Boots.Condition = 50000;
				Dragonslayer_Seer_Starkakedja_Boots.MaxCondition = 50000;
				Dragonslayer_Seer_Starkakedja_Boots.Durability = 50000;
				Dragonslayer_Seer_Starkakedja_Boots.MaxDurability = 0;
				Dragonslayer_Seer_Starkakedja_Boots.PoisonCharges = 0;
				Dragonslayer_Seer_Starkakedja_Boots.PoisonMaxCharges = 0;
				Dragonslayer_Seer_Starkakedja_Boots.PoisonSpellID = 0;
				Dragonslayer_Seer_Starkakedja_Boots.ProcSpellID1 = 40006;
				Dragonslayer_Seer_Starkakedja_Boots.SpellID1 = 0;
				Dragonslayer_Seer_Starkakedja_Boots.MaxCharges1 = 0;
				Dragonslayer_Seer_Starkakedja_Boots.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Seer_Starkakedja_Boots);
				}
			Dragonslayer_Seer_Starkakedja_Hauberk = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Seer_Starkakedja_Hauberk");
			if (Dragonslayer_Seer_Starkakedja_Hauberk == null)
			{
				Dragonslayer_Seer_Starkakedja_Hauberk = new ItemTemplate();
				Dragonslayer_Seer_Starkakedja_Hauberk.Name = "Dragonslayer Seer Starkakedja Hauberk";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Seer_Starkakedja_Hauberk.Name + ", creating it ...");
				Dragonslayer_Seer_Starkakedja_Hauberk.Level = 51;
				Dragonslayer_Seer_Starkakedja_Hauberk.Weight = 70;
				Dragonslayer_Seer_Starkakedja_Hauberk.Model = 4026;
				Dragonslayer_Seer_Starkakedja_Hauberk.Object_Type = 35;
				Dragonslayer_Seer_Starkakedja_Hauberk.Item_Type = 25;
				Dragonslayer_Seer_Starkakedja_Hauberk.Id_nb = "Dragonslayer_Seer_Starkakedja_Hauberk";
				Dragonslayer_Seer_Starkakedja_Hauberk.Hand = 0;
				Dragonslayer_Seer_Starkakedja_Hauberk.IsPickable = true;
				Dragonslayer_Seer_Starkakedja_Hauberk.IsDropable = true;
				Dragonslayer_Seer_Starkakedja_Hauberk.IsTradable = true;
				Dragonslayer_Seer_Starkakedja_Hauberk.CanDropAsLoot = true;
				Dragonslayer_Seer_Starkakedja_Hauberk.Color = 0;
				Dragonslayer_Seer_Starkakedja_Hauberk.Bonus = 35; // default bonus				
				Dragonslayer_Seer_Starkakedja_Hauberk.Bonus1 = 5;
				Dragonslayer_Seer_Starkakedja_Hauberk.Bonus1Type = (int) 18;
				Dragonslayer_Seer_Starkakedja_Hauberk.Bonus2 = 22;
				Dragonslayer_Seer_Starkakedja_Hauberk.Bonus2Type = (int) 10;
				Dragonslayer_Seer_Starkakedja_Hauberk.Bonus3 = 5;
				Dragonslayer_Seer_Starkakedja_Hauberk.Bonus3Type = (int) 15;
				Dragonslayer_Seer_Starkakedja_Hauberk.Bonus4 = 22;
				Dragonslayer_Seer_Starkakedja_Hauberk.Bonus4Type = (int) 2;
				Dragonslayer_Seer_Starkakedja_Hauberk.Bonus5 = 5;
				Dragonslayer_Seer_Starkakedja_Hauberk.Bonus5Type = (int) 12;
				Dragonslayer_Seer_Starkakedja_Hauberk.Bonus6 = 2;
				Dragonslayer_Seer_Starkakedja_Hauberk.Bonus6Type = (int) 191;
				Dragonslayer_Seer_Starkakedja_Hauberk.Bonus7 = 5;
				Dragonslayer_Seer_Starkakedja_Hauberk.Bonus7Type = (int) 209;
				Dragonslayer_Seer_Starkakedja_Hauberk.Bonus8 = 5;
				Dragonslayer_Seer_Starkakedja_Hauberk.Bonus8Type = (int) 202;
				Dragonslayer_Seer_Starkakedja_Hauberk.Bonus9 = 0;
				Dragonslayer_Seer_Starkakedja_Hauberk.Bonus9Type = (int) 0;
				Dragonslayer_Seer_Starkakedja_Hauberk.Bonus10 = 0;
				Dragonslayer_Seer_Starkakedja_Hauberk.Bonus10Type = (int) 0;
				Dragonslayer_Seer_Starkakedja_Hauberk.ExtraBonus = 0;
				Dragonslayer_Seer_Starkakedja_Hauberk.ExtraBonusType = (int) 0;
				Dragonslayer_Seer_Starkakedja_Hauberk.Effect = 0;
				Dragonslayer_Seer_Starkakedja_Hauberk.Emblem = 0;
				Dragonslayer_Seer_Starkakedja_Hauberk.Charges = 0;
				Dragonslayer_Seer_Starkakedja_Hauberk.MaxCharges = 0;
				Dragonslayer_Seer_Starkakedja_Hauberk.SpellID = 0;
				Dragonslayer_Seer_Starkakedja_Hauberk.ProcSpellID = 31001;
				Dragonslayer_Seer_Starkakedja_Hauberk.Type_Damage = 0;
				Dragonslayer_Seer_Starkakedja_Hauberk.Realm = 0;
				Dragonslayer_Seer_Starkakedja_Hauberk.MaxCount = 1;
				Dragonslayer_Seer_Starkakedja_Hauberk.PackSize = 1;
				Dragonslayer_Seer_Starkakedja_Hauberk.Extension = 5;
				Dragonslayer_Seer_Starkakedja_Hauberk.Quality = 100;				
				Dragonslayer_Seer_Starkakedja_Hauberk.Condition = 50000;
				Dragonslayer_Seer_Starkakedja_Hauberk.MaxCondition = 50000;
				Dragonslayer_Seer_Starkakedja_Hauberk.Durability = 50000;
				Dragonslayer_Seer_Starkakedja_Hauberk.MaxDurability = 0;
				Dragonslayer_Seer_Starkakedja_Hauberk.PoisonCharges = 0;
				Dragonslayer_Seer_Starkakedja_Hauberk.PoisonMaxCharges = 0;
				Dragonslayer_Seer_Starkakedja_Hauberk.PoisonSpellID = 0;
				Dragonslayer_Seer_Starkakedja_Hauberk.ProcSpellID1 = 40006;
				Dragonslayer_Seer_Starkakedja_Hauberk.SpellID1 = 0;
				Dragonslayer_Seer_Starkakedja_Hauberk.MaxCharges1 = 0;
				Dragonslayer_Seer_Starkakedja_Hauberk.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Seer_Starkakedja_Hauberk);
				}
			Dragonslayer_Seer_Starkakedja_Gloves = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Seer_Starkakedja_Gloves");
			if (Dragonslayer_Seer_Starkakedja_Gloves == null)
			{
				Dragonslayer_Seer_Starkakedja_Gloves = new ItemTemplate();
				Dragonslayer_Seer_Starkakedja_Gloves.Name = "Dragonslayer Seer Starkakedja Gloves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Seer_Starkakedja_Gloves.Name + ", creating it ...");
				Dragonslayer_Seer_Starkakedja_Gloves.Level = 51;
				Dragonslayer_Seer_Starkakedja_Gloves.Weight = 22;
				Dragonslayer_Seer_Starkakedja_Gloves.Model = 4029;
				Dragonslayer_Seer_Starkakedja_Gloves.Object_Type = 35;
				Dragonslayer_Seer_Starkakedja_Gloves.Item_Type = 22;
				Dragonslayer_Seer_Starkakedja_Gloves.Id_nb = "Dragonslayer_Seer_Starkakedja_Gloves";
				Dragonslayer_Seer_Starkakedja_Gloves.Hand = 0;
				Dragonslayer_Seer_Starkakedja_Gloves.IsPickable = true;
				Dragonslayer_Seer_Starkakedja_Gloves.IsDropable = true;
				Dragonslayer_Seer_Starkakedja_Gloves.IsTradable = true;
				Dragonslayer_Seer_Starkakedja_Gloves.CanDropAsLoot = true;
				Dragonslayer_Seer_Starkakedja_Gloves.Color = 0;
				Dragonslayer_Seer_Starkakedja_Gloves.Bonus = 35; // default bonus				
				Dragonslayer_Seer_Starkakedja_Gloves.Bonus1 = 5;
				Dragonslayer_Seer_Starkakedja_Gloves.Bonus1Type = (int) 12;
				Dragonslayer_Seer_Starkakedja_Gloves.Bonus2 = 5;
				Dragonslayer_Seer_Starkakedja_Gloves.Bonus2Type = (int) 18;
				Dragonslayer_Seer_Starkakedja_Gloves.Bonus3 = 40;
				Dragonslayer_Seer_Starkakedja_Gloves.Bonus3Type = (int) 10;
				Dragonslayer_Seer_Starkakedja_Gloves.Bonus4 = 5;
				Dragonslayer_Seer_Starkakedja_Gloves.Bonus4Type = (int) 15;
				Dragonslayer_Seer_Starkakedja_Gloves.Bonus5 = 5;
				Dragonslayer_Seer_Starkakedja_Gloves.Bonus5Type = (int) 190;
				Dragonslayer_Seer_Starkakedja_Gloves.Bonus6 = 5;
				Dragonslayer_Seer_Starkakedja_Gloves.Bonus6Type = (int) 195;
				Dragonslayer_Seer_Starkakedja_Gloves.Bonus7 = 6;
				Dragonslayer_Seer_Starkakedja_Gloves.Bonus7Type = (int) 196;
				Dragonslayer_Seer_Starkakedja_Gloves.Bonus8 = 0;
				Dragonslayer_Seer_Starkakedja_Gloves.Bonus8Type = (int) 0;
				Dragonslayer_Seer_Starkakedja_Gloves.Bonus9 = 0;
				Dragonslayer_Seer_Starkakedja_Gloves.Bonus9Type = (int) 0;
				Dragonslayer_Seer_Starkakedja_Gloves.Bonus10 = 0;
				Dragonslayer_Seer_Starkakedja_Gloves.Bonus10Type = (int) 0;
				Dragonslayer_Seer_Starkakedja_Gloves.ExtraBonus = 0;
				Dragonslayer_Seer_Starkakedja_Gloves.ExtraBonusType = (int) 0;
				Dragonslayer_Seer_Starkakedja_Gloves.Effect = 0;
				Dragonslayer_Seer_Starkakedja_Gloves.Emblem = 0;
				Dragonslayer_Seer_Starkakedja_Gloves.Charges = 0;
				Dragonslayer_Seer_Starkakedja_Gloves.MaxCharges = 0;
				Dragonslayer_Seer_Starkakedja_Gloves.SpellID = 0;
				Dragonslayer_Seer_Starkakedja_Gloves.ProcSpellID = 31001;
				Dragonslayer_Seer_Starkakedja_Gloves.Type_Damage = 0;
				Dragonslayer_Seer_Starkakedja_Gloves.Realm = 0;
				Dragonslayer_Seer_Starkakedja_Gloves.MaxCount = 1;
				Dragonslayer_Seer_Starkakedja_Gloves.PackSize = 1;
				Dragonslayer_Seer_Starkakedja_Gloves.Extension = 5;
				Dragonslayer_Seer_Starkakedja_Gloves.Quality = 100;				
				Dragonslayer_Seer_Starkakedja_Gloves.Condition = 50000;
				Dragonslayer_Seer_Starkakedja_Gloves.MaxCondition = 50000;
				Dragonslayer_Seer_Starkakedja_Gloves.Durability = 50000;
				Dragonslayer_Seer_Starkakedja_Gloves.MaxDurability = 0;
				Dragonslayer_Seer_Starkakedja_Gloves.PoisonCharges = 0;
				Dragonslayer_Seer_Starkakedja_Gloves.PoisonMaxCharges = 0;
				Dragonslayer_Seer_Starkakedja_Gloves.PoisonSpellID = 0;
				Dragonslayer_Seer_Starkakedja_Gloves.ProcSpellID1 = 40006;
				Dragonslayer_Seer_Starkakedja_Gloves.SpellID1 = 0;
				Dragonslayer_Seer_Starkakedja_Gloves.MaxCharges1 = 0;
				Dragonslayer_Seer_Starkakedja_Gloves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Seer_Starkakedja_Gloves);
				}
			Dragonslayer_Seer_Starkakedja_Chausses = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Seer_Starkakedja_Chausses");
			if (Dragonslayer_Seer_Starkakedja_Chausses == null)
			{
				Dragonslayer_Seer_Starkakedja_Chausses = new ItemTemplate();
				Dragonslayer_Seer_Starkakedja_Chausses.Name = "Dragonslayer Seer Starkakedja Chausses";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Seer_Starkakedja_Chausses.Name + ", creating it ...");
				Dragonslayer_Seer_Starkakedja_Chausses.Level = 51;
				Dragonslayer_Seer_Starkakedja_Chausses.Weight = 46;
				Dragonslayer_Seer_Starkakedja_Chausses.Model = 4027;
				Dragonslayer_Seer_Starkakedja_Chausses.Object_Type = 35;
				Dragonslayer_Seer_Starkakedja_Chausses.Item_Type = 27;
				Dragonslayer_Seer_Starkakedja_Chausses.Id_nb = "Dragonslayer_Seer_Starkakedja_Chausses";
				Dragonslayer_Seer_Starkakedja_Chausses.Hand = 0;
				Dragonslayer_Seer_Starkakedja_Chausses.IsPickable = true;
				Dragonslayer_Seer_Starkakedja_Chausses.IsDropable = true;
				Dragonslayer_Seer_Starkakedja_Chausses.IsTradable = true;
				Dragonslayer_Seer_Starkakedja_Chausses.CanDropAsLoot = true;
				Dragonslayer_Seer_Starkakedja_Chausses.Color = 0;
				Dragonslayer_Seer_Starkakedja_Chausses.Bonus = 35; // default bonus				
				Dragonslayer_Seer_Starkakedja_Chausses.Bonus1 = 22;
				Dragonslayer_Seer_Starkakedja_Chausses.Bonus1Type = (int) 2;
				Dragonslayer_Seer_Starkakedja_Chausses.Bonus2 = 5;
				Dragonslayer_Seer_Starkakedja_Chausses.Bonus2Type = (int) 12;
				Dragonslayer_Seer_Starkakedja_Chausses.Bonus3 = 5;
				Dragonslayer_Seer_Starkakedja_Chausses.Bonus3Type = (int) 18;
				Dragonslayer_Seer_Starkakedja_Chausses.Bonus4 = 40;
				Dragonslayer_Seer_Starkakedja_Chausses.Bonus4Type = (int) 10;
				Dragonslayer_Seer_Starkakedja_Chausses.Bonus5 = 5;
				Dragonslayer_Seer_Starkakedja_Chausses.Bonus5Type = (int) 15;
				Dragonslayer_Seer_Starkakedja_Chausses.Bonus6 = 1;
				Dragonslayer_Seer_Starkakedja_Chausses.Bonus6Type = (int) 191;
				Dragonslayer_Seer_Starkakedja_Chausses.Bonus7 = 5;
				Dragonslayer_Seer_Starkakedja_Chausses.Bonus7Type = (int) 202;
				Dragonslayer_Seer_Starkakedja_Chausses.Bonus8 = 0;
				Dragonslayer_Seer_Starkakedja_Chausses.Bonus8Type = (int) 0;
				Dragonslayer_Seer_Starkakedja_Chausses.Bonus9 = 0;
				Dragonslayer_Seer_Starkakedja_Chausses.Bonus9Type = (int) 0;
				Dragonslayer_Seer_Starkakedja_Chausses.Bonus10 = 0;
				Dragonslayer_Seer_Starkakedja_Chausses.Bonus10Type = (int) 0;
				Dragonslayer_Seer_Starkakedja_Chausses.ExtraBonus = 0;
				Dragonslayer_Seer_Starkakedja_Chausses.ExtraBonusType = (int) 0;
				Dragonslayer_Seer_Starkakedja_Chausses.Effect = 0;
				Dragonslayer_Seer_Starkakedja_Chausses.Emblem = 0;
				Dragonslayer_Seer_Starkakedja_Chausses.Charges = 0;
				Dragonslayer_Seer_Starkakedja_Chausses.MaxCharges = 0;
				Dragonslayer_Seer_Starkakedja_Chausses.SpellID = 0;
				Dragonslayer_Seer_Starkakedja_Chausses.ProcSpellID = 31001;
				Dragonslayer_Seer_Starkakedja_Chausses.Type_Damage = 0;
				Dragonslayer_Seer_Starkakedja_Chausses.Realm = 0;
				Dragonslayer_Seer_Starkakedja_Chausses.MaxCount = 1;
				Dragonslayer_Seer_Starkakedja_Chausses.PackSize = 1;
				Dragonslayer_Seer_Starkakedja_Chausses.Extension = 5;
				Dragonslayer_Seer_Starkakedja_Chausses.Quality = 100;				
				Dragonslayer_Seer_Starkakedja_Chausses.Condition = 50000;
				Dragonslayer_Seer_Starkakedja_Chausses.MaxCondition = 50000;
				Dragonslayer_Seer_Starkakedja_Chausses.Durability = 50000;
				Dragonslayer_Seer_Starkakedja_Chausses.MaxDurability = 0;
				Dragonslayer_Seer_Starkakedja_Chausses.PoisonCharges = 0;
				Dragonslayer_Seer_Starkakedja_Chausses.PoisonMaxCharges = 0;
				Dragonslayer_Seer_Starkakedja_Chausses.PoisonSpellID = 0;
				Dragonslayer_Seer_Starkakedja_Chausses.ProcSpellID1 = 40006;
				Dragonslayer_Seer_Starkakedja_Chausses.SpellID1 = 0;
				Dragonslayer_Seer_Starkakedja_Chausses.MaxCharges1 = 0;
				Dragonslayer_Seer_Starkakedja_Chausses.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Seer_Starkakedja_Chausses);
				}
			Dragonslayer_Seer_Starkakedja_Full_Helm = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Seer_Starkakedja_Full_Helm");
			if (Dragonslayer_Seer_Starkakedja_Full_Helm == null)
			{
				Dragonslayer_Seer_Starkakedja_Full_Helm = new ItemTemplate();
				Dragonslayer_Seer_Starkakedja_Full_Helm.Name = "Dragonslayer Seer Starkakedja Full Helm";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Seer_Starkakedja_Full_Helm.Name + ", creating it ...");
				Dragonslayer_Seer_Starkakedja_Full_Helm.Level = 51;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Weight = 22;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Model = 4072;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Object_Type = 35;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Item_Type = 21;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Id_nb = "Dragonslayer_Seer_Starkakedja_Full_Helm";
				Dragonslayer_Seer_Starkakedja_Full_Helm.Hand = 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.IsPickable = true;
				Dragonslayer_Seer_Starkakedja_Full_Helm.IsDropable = true;
				Dragonslayer_Seer_Starkakedja_Full_Helm.IsTradable = true;
				Dragonslayer_Seer_Starkakedja_Full_Helm.CanDropAsLoot = true;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Color = 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Bonus = 35; // default bonus				
				Dragonslayer_Seer_Starkakedja_Full_Helm.Bonus1 = 22;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Bonus1Type = (int) 3;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Bonus2 = 6;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Bonus2Type = (int) 16;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Bonus3 = 6;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Bonus3Type = (int) 11;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Bonus4 = 40;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Bonus4Type = (int) 10;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Bonus5 = 6;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Bonus5Type = (int) 14;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Bonus6 = 5;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Bonus6Type = (int) 148;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Bonus7 = 40;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Bonus7Type = (int) 210;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Bonus8 = 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Bonus8Type = (int) 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Bonus9 = 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Bonus9Type = (int) 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Bonus10 = 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Bonus10Type = (int) 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.ExtraBonus = 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.ExtraBonusType = (int) 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Effect = 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Emblem = 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Charges = 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.MaxCharges = 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.SpellID = 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.ProcSpellID = 31001;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Type_Damage = 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Realm = 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.MaxCount = 1;
				Dragonslayer_Seer_Starkakedja_Full_Helm.PackSize = 1;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Extension = 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Quality = 100;				
				Dragonslayer_Seer_Starkakedja_Full_Helm.Condition = 50000;
				Dragonslayer_Seer_Starkakedja_Full_Helm.MaxCondition = 50000;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Durability = 50000;
				Dragonslayer_Seer_Starkakedja_Full_Helm.MaxDurability = 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.PoisonCharges = 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.PoisonMaxCharges = 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.PoisonSpellID = 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.ProcSpellID1 = 40006;
				Dragonslayer_Seer_Starkakedja_Full_Helm.SpellID1 = 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.MaxCharges1 = 0;
				Dragonslayer_Seer_Starkakedja_Full_Helm.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Seer_Starkakedja_Full_Helm);
				}


            #endregion

            #region defineAreas

            #endregion

            #region defineQuestParts


            QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerShamanQuest));
            QuestBehaviour a;
            a = builder.CreateBehaviour(AmarasavaDas, -1);
            a.AddTrigger(eTriggerType.Interact, null, AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerShamanQuest), AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerShamanQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "I have lived in this village since I was a young girl. My father was a great dragon hunter you know.", AmarasavaDas);
            a.AddAction(eActionType.Talk, "In my years I have learned to work with all kinds of problems, specially with [Dragons].", AmarasavaDas);
            AddBehaviour(a);

            a = builder.CreateBehaviour(AmarasavaDas, -1);
            a.AddTrigger(eTriggerType.Whisper, "Dragons", AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerShamanQuest), AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerShamanQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "yes dragons, but there is a problem for me, i can't hunt dragons by self, [you know].", AmarasavaDas);
            AddBehaviour(a);

            a = builder.CreateBehaviour(AmarasavaDas, -1);
            a.AddTrigger(eTriggerType.Whisper, "you know", AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerShamanQuest), AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerShamanQuest), null, (eComparator)5);
            a.AddAction(eActionType.OfferQuest, typeof(DragonslayerShamanQuest), "Helen has offered you the Dragonslayer quest.?Do you accept?");
            AddBehaviour(a);

            a = builder.CreateBehaviour(AmarasavaDas, -1);
            a.AddTrigger(eTriggerType.DeclineQuest, null, typeof(DragonslayerShamanQuest));
            a.AddAction(eActionType.Talk, "No problem. See you", AmarasavaDas);
            AddBehaviour(a);

            a = builder.CreateBehaviour(AmarasavaDas, -1);
            a.AddTrigger(eTriggerType.AcceptQuest, null, typeof(DragonslayerShamanQuest));
            a.AddAction(eActionType.Talk, "If you bring me the head of Jarkkonith I shall pay you well!", AmarasavaDas);
            a.AddAction(eActionType.GiveQuest, typeof(DragonslayerShamanQuest), AmarasavaDas);
            AddBehaviour(a);

			           			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End
			if (AmarasavaDas!=null) {
				AmarasavaDas.AddQuestToGive(typeof (DragonslayerShamanQuest));
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
			AmarasavaDas.RemoveQuestToGive(typeof (DragonslayerShamanQuest));
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
			if (player.IsDoingQuest(typeof (DragonslayerShamanQuest)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
			if (
		
			player.CharacterClass.ID != (byte) eCharacterClass.Shaman && 
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
