	
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
	public class DragonslayerDruidQuest : BaseQuest
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

		protected const string questTitle = "DragonslayerDruidQuest";

		protected const int minimumLevel = 49;
		protected const int maximumLevel = 50;
	
	
		private static GameNPC Lirazal = null;
		
		private static GameNPC Asiintath = null;
		
		private static ItemTemplate Asiintath_Head = null;
		
		private static ItemTemplate dragonslayer_holy_osnadurtha_arms = null;
		
		private static ItemTemplate dragonslayer_holy_osnadurtha_hauberk = null;
		
		private static ItemTemplate dragonslayer_holy_osnadurtha_boots = null;
		
		private static ItemTemplate dragonslayer_holy_osnadurtha_gloves = null;
		
		private static ItemTemplate dragonslayer_holy_osnadurtha_full_helm = null;
		
		private static ItemTemplate dragonslayer_holy_osnadurtha_legs = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public DragonslayerDruidQuest() : base()
		{
		}

		public DragonslayerDruidQuest(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public DragonslayerDruidQuest(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public DragonslayerDruidQuest(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
	{
	}
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			QuestBehaviour a;
			QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerDruidQuest));

			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(DragonslayerDruidQuest)) == null)
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
						a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerDruidQuest), null);
						a.AddAction(eActionType.GiveItem, dragonslayer_holy_osnadurtha_arms, Lirazal);
						a.AddAction(eActionType.GiveItem, dragonslayer_holy_osnadurtha_hauberk, Lirazal);
						a.AddAction(eActionType.GiveItem, dragonslayer_holy_osnadurtha_boots, Lirazal);
						a.AddAction(eActionType.GiveItem, dragonslayer_holy_osnadurtha_gloves, Lirazal);
						a.AddAction(eActionType.GiveItem, dragonslayer_holy_osnadurtha_full_helm, Lirazal);
						a.AddAction(eActionType.GiveItem, dragonslayer_holy_osnadurtha_legs, Lirazal);
						a.AddAction(eActionType.FinishQuest, typeof(DragonslayerDruidQuest), null);
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
			dragonslayer_holy_osnadurtha_arms = GameServer.Database.FindObjectByKey<ItemTemplate>("dragonslayer_holy_osnadurtha_arms");
			if (dragonslayer_holy_osnadurtha_arms == null)
			{
				dragonslayer_holy_osnadurtha_arms = new ItemTemplate();
				dragonslayer_holy_osnadurtha_arms.Name = "Dragonslayer Holy Osnadurtha Arms";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dragonslayer_holy_osnadurtha_arms.Name + ", creating it ...");
				dragonslayer_holy_osnadurtha_arms.Level = 51;
				dragonslayer_holy_osnadurtha_arms.Weight = 48;
				dragonslayer_holy_osnadurtha_arms.Model = 4091;
				dragonslayer_holy_osnadurtha_arms.Object_Type = 38;
				dragonslayer_holy_osnadurtha_arms.Item_Type = 28;
				dragonslayer_holy_osnadurtha_arms.Id_nb = "dragonslayer_holy_osnadurtha_arms";
				dragonslayer_holy_osnadurtha_arms.Hand = 0;
				dragonslayer_holy_osnadurtha_arms.IsPickable = true;
				dragonslayer_holy_osnadurtha_arms.IsDropable = true;
				dragonslayer_holy_osnadurtha_arms.IsTradable = true;
				dragonslayer_holy_osnadurtha_arms.CanDropAsLoot = true;
				dragonslayer_holy_osnadurtha_arms.Color = 0;
				dragonslayer_holy_osnadurtha_arms.Bonus = 35; // default bonus				
				dragonslayer_holy_osnadurtha_arms.Bonus1 = 5;
				dragonslayer_holy_osnadurtha_arms.Bonus1Type = (int) 12;
				dragonslayer_holy_osnadurtha_arms.Bonus2 = 5;
				dragonslayer_holy_osnadurtha_arms.Bonus2Type = (int) 18;
				dragonslayer_holy_osnadurtha_arms.Bonus3 = 5;
				dragonslayer_holy_osnadurtha_arms.Bonus3Type = (int) 15;
				dragonslayer_holy_osnadurtha_arms.Bonus4 = 22;
				dragonslayer_holy_osnadurtha_arms.Bonus4Type = (int) 156;
				dragonslayer_holy_osnadurtha_arms.Bonus5 = 40;
				dragonslayer_holy_osnadurtha_arms.Bonus5Type = (int) 10;
				dragonslayer_holy_osnadurtha_arms.Bonus6 = 5;
				dragonslayer_holy_osnadurtha_arms.Bonus6Type = (int) 209;
				dragonslayer_holy_osnadurtha_arms.Bonus7 = 1;
				dragonslayer_holy_osnadurtha_arms.Bonus7Type = (int) 191;
				dragonslayer_holy_osnadurtha_arms.Bonus8 = 0;
				dragonslayer_holy_osnadurtha_arms.Bonus8Type = (int) 0;
				dragonslayer_holy_osnadurtha_arms.Bonus9 = 0;
				dragonslayer_holy_osnadurtha_arms.Bonus9Type = (int) 0;
				dragonslayer_holy_osnadurtha_arms.Bonus10 = 0;
				dragonslayer_holy_osnadurtha_arms.Bonus10Type = (int) 0;
				dragonslayer_holy_osnadurtha_arms.ExtraBonus = 0;
				dragonslayer_holy_osnadurtha_arms.ExtraBonusType = (int) 0;
				dragonslayer_holy_osnadurtha_arms.Effect = 0;
				dragonslayer_holy_osnadurtha_arms.Emblem = 0;
				dragonslayer_holy_osnadurtha_arms.Charges = 0;
				dragonslayer_holy_osnadurtha_arms.MaxCharges = 0;
				dragonslayer_holy_osnadurtha_arms.SpellID = 0;
				dragonslayer_holy_osnadurtha_arms.ProcSpellID = 31001;
				dragonslayer_holy_osnadurtha_arms.Type_Damage = 0;
				dragonslayer_holy_osnadurtha_arms.Realm = 0;
				dragonslayer_holy_osnadurtha_arms.MaxCount = 1;
				dragonslayer_holy_osnadurtha_arms.PackSize = 1;
				dragonslayer_holy_osnadurtha_arms.Extension = 5;
				dragonslayer_holy_osnadurtha_arms.Quality = 100;				
				dragonslayer_holy_osnadurtha_arms.Condition = 50000;
				dragonslayer_holy_osnadurtha_arms.MaxCondition = 50000;
				dragonslayer_holy_osnadurtha_arms.Durability = 50000;
				dragonslayer_holy_osnadurtha_arms.MaxDurability = 0;
				dragonslayer_holy_osnadurtha_arms.PoisonCharges = 0;
				dragonslayer_holy_osnadurtha_arms.PoisonMaxCharges = 0;
				dragonslayer_holy_osnadurtha_arms.PoisonSpellID = 0;
				dragonslayer_holy_osnadurtha_arms.ProcSpellID1 = 40006;
				dragonslayer_holy_osnadurtha_arms.SpellID1 = 0;
				dragonslayer_holy_osnadurtha_arms.MaxCharges1 = 0;
				dragonslayer_holy_osnadurtha_arms.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(dragonslayer_holy_osnadurtha_arms);
				}
			dragonslayer_holy_osnadurtha_hauberk = GameServer.Database.FindObjectByKey<ItemTemplate>("dragonslayer_holy_osnadurtha_hauberk");
			if (dragonslayer_holy_osnadurtha_hauberk == null)
			{
				dragonslayer_holy_osnadurtha_hauberk = new ItemTemplate();
				dragonslayer_holy_osnadurtha_hauberk.Name = "Dragonslayer Holy Osnadurtha Hauberk";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dragonslayer_holy_osnadurtha_hauberk.Name + ", creating it ...");
				dragonslayer_holy_osnadurtha_hauberk.Level = 51;
				dragonslayer_holy_osnadurtha_hauberk.Weight = 80;
				dragonslayer_holy_osnadurtha_hauberk.Model = 4089;
				dragonslayer_holy_osnadurtha_hauberk.Object_Type = 38;
				dragonslayer_holy_osnadurtha_hauberk.Item_Type = 25;
				dragonslayer_holy_osnadurtha_hauberk.Id_nb = "dragonslayer_holy_osnadurtha_hauberk";
				dragonslayer_holy_osnadurtha_hauberk.Hand = 0;
				dragonslayer_holy_osnadurtha_hauberk.IsPickable = true;
				dragonslayer_holy_osnadurtha_hauberk.IsDropable = true;
				dragonslayer_holy_osnadurtha_hauberk.IsTradable = true;
				dragonslayer_holy_osnadurtha_hauberk.CanDropAsLoot = true;
				dragonslayer_holy_osnadurtha_hauberk.Color = 0;
				dragonslayer_holy_osnadurtha_hauberk.Bonus = 35; // default bonus				
				dragonslayer_holy_osnadurtha_hauberk.Bonus1 = 22;
				dragonslayer_holy_osnadurtha_hauberk.Bonus1Type = (int) 2;
				dragonslayer_holy_osnadurtha_hauberk.Bonus2 = 5;
				dragonslayer_holy_osnadurtha_hauberk.Bonus2Type = (int) 12;
				dragonslayer_holy_osnadurtha_hauberk.Bonus3 = 5;
				dragonslayer_holy_osnadurtha_hauberk.Bonus3Type = (int) 18;
				dragonslayer_holy_osnadurtha_hauberk.Bonus4 = 5;
				dragonslayer_holy_osnadurtha_hauberk.Bonus4Type = (int) 15;
				dragonslayer_holy_osnadurtha_hauberk.Bonus5 = 22;
				dragonslayer_holy_osnadurtha_hauberk.Bonus5Type = (int) 156;
				dragonslayer_holy_osnadurtha_hauberk.Bonus6 = 2;
				dragonslayer_holy_osnadurtha_hauberk.Bonus6Type = (int) 191;
				dragonslayer_holy_osnadurtha_hauberk.Bonus7 = 5;
				dragonslayer_holy_osnadurtha_hauberk.Bonus7Type = (int) 209;
				dragonslayer_holy_osnadurtha_hauberk.Bonus8 = 5;
				dragonslayer_holy_osnadurtha_hauberk.Bonus8Type = (int) 202;
				dragonslayer_holy_osnadurtha_hauberk.Bonus9 = 0;
				dragonslayer_holy_osnadurtha_hauberk.Bonus9Type = (int) 0;
				dragonslayer_holy_osnadurtha_hauberk.Bonus10 = 0;
				dragonslayer_holy_osnadurtha_hauberk.Bonus10Type = (int) 0;
				dragonslayer_holy_osnadurtha_hauberk.ExtraBonus = 0;
				dragonslayer_holy_osnadurtha_hauberk.ExtraBonusType = (int) 0;
				dragonslayer_holy_osnadurtha_hauberk.Effect = 0;
				dragonslayer_holy_osnadurtha_hauberk.Emblem = 0;
				dragonslayer_holy_osnadurtha_hauberk.Charges = 0;
				dragonslayer_holy_osnadurtha_hauberk.MaxCharges = 0;
				dragonslayer_holy_osnadurtha_hauberk.SpellID = 0;
				dragonslayer_holy_osnadurtha_hauberk.ProcSpellID = 31001;
				dragonslayer_holy_osnadurtha_hauberk.Type_Damage = 0;
				dragonslayer_holy_osnadurtha_hauberk.Realm = 0;
				dragonslayer_holy_osnadurtha_hauberk.MaxCount = 1;
				dragonslayer_holy_osnadurtha_hauberk.PackSize = 1;
				dragonslayer_holy_osnadurtha_hauberk.Extension = 5;
				dragonslayer_holy_osnadurtha_hauberk.Quality = 100;				
				dragonslayer_holy_osnadurtha_hauberk.Condition = 50000;
				dragonslayer_holy_osnadurtha_hauberk.MaxCondition = 50000;
				dragonslayer_holy_osnadurtha_hauberk.Durability = 50000;
				dragonslayer_holy_osnadurtha_hauberk.MaxDurability = 0;
				dragonslayer_holy_osnadurtha_hauberk.PoisonCharges = 0;
				dragonslayer_holy_osnadurtha_hauberk.PoisonMaxCharges = 0;
				dragonslayer_holy_osnadurtha_hauberk.PoisonSpellID = 0;
				dragonslayer_holy_osnadurtha_hauberk.ProcSpellID1 = 40006;
				dragonslayer_holy_osnadurtha_hauberk.SpellID1 = 0;
				dragonslayer_holy_osnadurtha_hauberk.MaxCharges1 = 0;
				dragonslayer_holy_osnadurtha_hauberk.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(dragonslayer_holy_osnadurtha_hauberk);
				}
			dragonslayer_holy_osnadurtha_boots = GameServer.Database.FindObjectByKey<ItemTemplate>("dragonslayer_holy_osnadurtha_boots");
			if (dragonslayer_holy_osnadurtha_boots == null)
			{
				dragonslayer_holy_osnadurtha_boots = new ItemTemplate();
				dragonslayer_holy_osnadurtha_boots.Name = "Dragonslayer Holy Osnadurtha Boots";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dragonslayer_holy_osnadurtha_boots.Name + ", creating it ...");
				dragonslayer_holy_osnadurtha_boots.Level = 51;
				dragonslayer_holy_osnadurtha_boots.Weight = 32;
				dragonslayer_holy_osnadurtha_boots.Model = 4093;
				dragonslayer_holy_osnadurtha_boots.Object_Type = 38;
				dragonslayer_holy_osnadurtha_boots.Item_Type = 23;
				dragonslayer_holy_osnadurtha_boots.Id_nb = "dragonslayer_holy_osnadurtha_boots";
				dragonslayer_holy_osnadurtha_boots.Hand = 0;
				dragonslayer_holy_osnadurtha_boots.IsPickable = true;
				dragonslayer_holy_osnadurtha_boots.IsDropable = true;
				dragonslayer_holy_osnadurtha_boots.IsTradable = true;
				dragonslayer_holy_osnadurtha_boots.CanDropAsLoot = true;
				dragonslayer_holy_osnadurtha_boots.Color = 0;
				dragonslayer_holy_osnadurtha_boots.Bonus = 35; // default bonus				
				dragonslayer_holy_osnadurtha_boots.Bonus1 = 18;
				dragonslayer_holy_osnadurtha_boots.Bonus1Type = (int) 1;
				dragonslayer_holy_osnadurtha_boots.Bonus2 = 18;
				dragonslayer_holy_osnadurtha_boots.Bonus2Type = (int) 2;
				dragonslayer_holy_osnadurtha_boots.Bonus3 = 6;
				dragonslayer_holy_osnadurtha_boots.Bonus3Type = (int) 13;
				dragonslayer_holy_osnadurtha_boots.Bonus4 = 6;
				dragonslayer_holy_osnadurtha_boots.Bonus4Type = (int) 17;
				dragonslayer_holy_osnadurtha_boots.Bonus5 = 6;
				dragonslayer_holy_osnadurtha_boots.Bonus5Type = (int) 19;
				dragonslayer_holy_osnadurtha_boots.Bonus6 = 6;
				dragonslayer_holy_osnadurtha_boots.Bonus6Type = (int) 196;
				dragonslayer_holy_osnadurtha_boots.Bonus7 = 0;
				dragonslayer_holy_osnadurtha_boots.Bonus7Type = (int) 0;
				dragonslayer_holy_osnadurtha_boots.Bonus8 = 0;
				dragonslayer_holy_osnadurtha_boots.Bonus8Type = (int) 0;
				dragonslayer_holy_osnadurtha_boots.Bonus9 = 0;
				dragonslayer_holy_osnadurtha_boots.Bonus9Type = (int) 0;
				dragonslayer_holy_osnadurtha_boots.Bonus10 = 0;
				dragonslayer_holy_osnadurtha_boots.Bonus10Type = (int) 0;
				dragonslayer_holy_osnadurtha_boots.ExtraBonus = 0;
				dragonslayer_holy_osnadurtha_boots.ExtraBonusType = (int) 0;
				dragonslayer_holy_osnadurtha_boots.Effect = 0;
				dragonslayer_holy_osnadurtha_boots.Emblem = 0;
				dragonslayer_holy_osnadurtha_boots.Charges = 0;
				dragonslayer_holy_osnadurtha_boots.MaxCharges = 0;
				dragonslayer_holy_osnadurtha_boots.SpellID = 0;
				dragonslayer_holy_osnadurtha_boots.ProcSpellID = 31001;
				dragonslayer_holy_osnadurtha_boots.Type_Damage = 0;
				dragonslayer_holy_osnadurtha_boots.Realm = 0;
				dragonslayer_holy_osnadurtha_boots.MaxCount = 1;
				dragonslayer_holy_osnadurtha_boots.PackSize = 1;
				dragonslayer_holy_osnadurtha_boots.Extension = 5;
				dragonslayer_holy_osnadurtha_boots.Quality = 100;				
				dragonslayer_holy_osnadurtha_boots.Condition = 50000;
				dragonslayer_holy_osnadurtha_boots.MaxCondition = 50000;
				dragonslayer_holy_osnadurtha_boots.Durability = 50000;
				dragonslayer_holy_osnadurtha_boots.MaxDurability = 0;
				dragonslayer_holy_osnadurtha_boots.PoisonCharges = 0;
				dragonslayer_holy_osnadurtha_boots.PoisonMaxCharges = 0;
				dragonslayer_holy_osnadurtha_boots.PoisonSpellID = 0;
				dragonslayer_holy_osnadurtha_boots.ProcSpellID1 = 40006;
				dragonslayer_holy_osnadurtha_boots.SpellID1 = 0;
				dragonslayer_holy_osnadurtha_boots.MaxCharges1 = 0;
				dragonslayer_holy_osnadurtha_boots.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(dragonslayer_holy_osnadurtha_boots);
				}
			dragonslayer_holy_osnadurtha_gloves = GameServer.Database.FindObjectByKey<ItemTemplate>("dragonslayer_holy_osnadurtha_gloves");
			if (dragonslayer_holy_osnadurtha_gloves == null)
			{
				dragonslayer_holy_osnadurtha_gloves = new ItemTemplate();
				dragonslayer_holy_osnadurtha_gloves.Name = "Dragonslayer Holy Osnadurtha Gloves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dragonslayer_holy_osnadurtha_gloves.Name + ", creating it ...");
				dragonslayer_holy_osnadurtha_gloves.Level = 51;
				dragonslayer_holy_osnadurtha_gloves.Weight = 32;
				dragonslayer_holy_osnadurtha_gloves.Model = 4092;
				dragonslayer_holy_osnadurtha_gloves.Object_Type = 38;
				dragonslayer_holy_osnadurtha_gloves.Item_Type = 22;
				dragonslayer_holy_osnadurtha_gloves.Id_nb = "dragonslayer_holy_osnadurtha_gloves";
				dragonslayer_holy_osnadurtha_gloves.Hand = 0;
				dragonslayer_holy_osnadurtha_gloves.IsPickable = true;
				dragonslayer_holy_osnadurtha_gloves.IsDropable = true;
				dragonslayer_holy_osnadurtha_gloves.IsTradable = true;
				dragonslayer_holy_osnadurtha_gloves.CanDropAsLoot = true;
				dragonslayer_holy_osnadurtha_gloves.Color = 0;
				dragonslayer_holy_osnadurtha_gloves.Bonus = 35; // default bonus				
				dragonslayer_holy_osnadurtha_gloves.Bonus1 = 40;
				dragonslayer_holy_osnadurtha_gloves.Bonus1Type = (int) 10;
				dragonslayer_holy_osnadurtha_gloves.Bonus2 = 5;
				dragonslayer_holy_osnadurtha_gloves.Bonus2Type = (int) 12;
				dragonslayer_holy_osnadurtha_gloves.Bonus3 = 5;
				dragonslayer_holy_osnadurtha_gloves.Bonus3Type = (int) 18;
				dragonslayer_holy_osnadurtha_gloves.Bonus4 = 5;
				dragonslayer_holy_osnadurtha_gloves.Bonus4Type = (int) 15;
				dragonslayer_holy_osnadurtha_gloves.Bonus5 = 5;
				dragonslayer_holy_osnadurtha_gloves.Bonus5Type = (int) 190;
				dragonslayer_holy_osnadurtha_gloves.Bonus6 = 5;
				dragonslayer_holy_osnadurtha_gloves.Bonus6Type = (int) 195;
				dragonslayer_holy_osnadurtha_gloves.Bonus7 = 6;
				dragonslayer_holy_osnadurtha_gloves.Bonus7Type = (int) 196;
				dragonslayer_holy_osnadurtha_gloves.Bonus8 = 0;
				dragonslayer_holy_osnadurtha_gloves.Bonus8Type = (int) 0;
				dragonslayer_holy_osnadurtha_gloves.Bonus9 = 0;
				dragonslayer_holy_osnadurtha_gloves.Bonus9Type = (int) 0;
				dragonslayer_holy_osnadurtha_gloves.Bonus10 = 0;
				dragonslayer_holy_osnadurtha_gloves.Bonus10Type = (int) 0;
				dragonslayer_holy_osnadurtha_gloves.ExtraBonus = 0;
				dragonslayer_holy_osnadurtha_gloves.ExtraBonusType = (int) 0;
				dragonslayer_holy_osnadurtha_gloves.Effect = 0;
				dragonslayer_holy_osnadurtha_gloves.Emblem = 0;
				dragonslayer_holy_osnadurtha_gloves.Charges = 0;
				dragonslayer_holy_osnadurtha_gloves.MaxCharges = 0;
				dragonslayer_holy_osnadurtha_gloves.SpellID = 0;
				dragonslayer_holy_osnadurtha_gloves.ProcSpellID = 31001;
				dragonslayer_holy_osnadurtha_gloves.Type_Damage = 0;
				dragonslayer_holy_osnadurtha_gloves.Realm = 0;
				dragonslayer_holy_osnadurtha_gloves.MaxCount = 1;
				dragonslayer_holy_osnadurtha_gloves.PackSize = 1;
				dragonslayer_holy_osnadurtha_gloves.Extension = 5;
				dragonslayer_holy_osnadurtha_gloves.Quality = 100;				
				dragonslayer_holy_osnadurtha_gloves.Condition = 50000;
				dragonslayer_holy_osnadurtha_gloves.MaxCondition = 50000;
				dragonslayer_holy_osnadurtha_gloves.Durability = 50000;
				dragonslayer_holy_osnadurtha_gloves.MaxDurability = 0;
				dragonslayer_holy_osnadurtha_gloves.PoisonCharges = 0;
				dragonslayer_holy_osnadurtha_gloves.PoisonMaxCharges = 0;
				dragonslayer_holy_osnadurtha_gloves.PoisonSpellID = 0;
				dragonslayer_holy_osnadurtha_gloves.ProcSpellID1 = 40006;
				dragonslayer_holy_osnadurtha_gloves.SpellID1 = 0;
				dragonslayer_holy_osnadurtha_gloves.MaxCharges1 = 0;
				dragonslayer_holy_osnadurtha_gloves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(dragonslayer_holy_osnadurtha_gloves);
				}
			dragonslayer_holy_osnadurtha_full_helm = GameServer.Database.FindObjectByKey<ItemTemplate>("dragonslayer_holy_osnadurtha_full_helm");
			if (dragonslayer_holy_osnadurtha_full_helm == null)
			{
				dragonslayer_holy_osnadurtha_full_helm = new ItemTemplate();
				dragonslayer_holy_osnadurtha_full_helm.Name = "Dragonslayer Holy Osnadurtha Full Helm";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dragonslayer_holy_osnadurtha_full_helm.Name + ", creating it ...");
				dragonslayer_holy_osnadurtha_full_helm.Level = 51;
				dragonslayer_holy_osnadurtha_full_helm.Weight = 32;
				dragonslayer_holy_osnadurtha_full_helm.Model = 4066;
				dragonslayer_holy_osnadurtha_full_helm.Object_Type = 38;
				dragonslayer_holy_osnadurtha_full_helm.Item_Type = 21;
				dragonslayer_holy_osnadurtha_full_helm.Id_nb = "dragonslayer_holy_osnadurtha_full_helm";
				dragonslayer_holy_osnadurtha_full_helm.Hand = 0;
				dragonslayer_holy_osnadurtha_full_helm.IsPickable = true;
				dragonslayer_holy_osnadurtha_full_helm.IsDropable = true;
				dragonslayer_holy_osnadurtha_full_helm.IsTradable = true;
				dragonslayer_holy_osnadurtha_full_helm.CanDropAsLoot = true;
				dragonslayer_holy_osnadurtha_full_helm.Color = 0;
				dragonslayer_holy_osnadurtha_full_helm.Bonus = 35; // default bonus				
				dragonslayer_holy_osnadurtha_full_helm.Bonus1 = 22;
				dragonslayer_holy_osnadurtha_full_helm.Bonus1Type = (int) 3;
				dragonslayer_holy_osnadurtha_full_helm.Bonus2 = 40;
				dragonslayer_holy_osnadurtha_full_helm.Bonus2Type = (int) 10;
				dragonslayer_holy_osnadurtha_full_helm.Bonus3 = 6;
				dragonslayer_holy_osnadurtha_full_helm.Bonus3Type = (int) 16;
				dragonslayer_holy_osnadurtha_full_helm.Bonus4 = 6;
				dragonslayer_holy_osnadurtha_full_helm.Bonus4Type = (int) 11;
				dragonslayer_holy_osnadurtha_full_helm.Bonus5 = 6;
				dragonslayer_holy_osnadurtha_full_helm.Bonus5Type = (int) 14;
				dragonslayer_holy_osnadurtha_full_helm.Bonus6 = 5;
				dragonslayer_holy_osnadurtha_full_helm.Bonus6Type = (int) 148;
				dragonslayer_holy_osnadurtha_full_helm.Bonus7 = 40;
				dragonslayer_holy_osnadurtha_full_helm.Bonus7Type = (int) 210;
				dragonslayer_holy_osnadurtha_full_helm.Bonus8 = 0;
				dragonslayer_holy_osnadurtha_full_helm.Bonus8Type = (int) 0;
				dragonslayer_holy_osnadurtha_full_helm.Bonus9 = 0;
				dragonslayer_holy_osnadurtha_full_helm.Bonus9Type = (int) 0;
				dragonslayer_holy_osnadurtha_full_helm.Bonus10 = 0;
				dragonslayer_holy_osnadurtha_full_helm.Bonus10Type = (int) 0;
				dragonslayer_holy_osnadurtha_full_helm.ExtraBonus = 0;
				dragonslayer_holy_osnadurtha_full_helm.ExtraBonusType = (int) 0;
				dragonslayer_holy_osnadurtha_full_helm.Effect = 0;
				dragonslayer_holy_osnadurtha_full_helm.Emblem = 0;
				dragonslayer_holy_osnadurtha_full_helm.Charges = 0;
				dragonslayer_holy_osnadurtha_full_helm.MaxCharges = 0;
				dragonslayer_holy_osnadurtha_full_helm.SpellID = 0;
				dragonslayer_holy_osnadurtha_full_helm.ProcSpellID = 31001;
				dragonslayer_holy_osnadurtha_full_helm.Type_Damage = 0;
				dragonslayer_holy_osnadurtha_full_helm.Realm = 0;
				dragonslayer_holy_osnadurtha_full_helm.MaxCount = 1;
				dragonslayer_holy_osnadurtha_full_helm.PackSize = 1;
				dragonslayer_holy_osnadurtha_full_helm.Extension = 0;
				dragonslayer_holy_osnadurtha_full_helm.Quality = 100;				
				dragonslayer_holy_osnadurtha_full_helm.Condition = 50000;
				dragonslayer_holy_osnadurtha_full_helm.MaxCondition = 50000;
				dragonslayer_holy_osnadurtha_full_helm.Durability = 50000;
				dragonslayer_holy_osnadurtha_full_helm.MaxDurability = 0;
				dragonslayer_holy_osnadurtha_full_helm.PoisonCharges = 0;
				dragonslayer_holy_osnadurtha_full_helm.PoisonMaxCharges = 0;
				dragonslayer_holy_osnadurtha_full_helm.PoisonSpellID = 0;
				dragonslayer_holy_osnadurtha_full_helm.ProcSpellID1 = 40006;
				dragonslayer_holy_osnadurtha_full_helm.SpellID1 = 0;
				dragonslayer_holy_osnadurtha_full_helm.MaxCharges1 = 0;
				dragonslayer_holy_osnadurtha_full_helm.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(dragonslayer_holy_osnadurtha_full_helm);
				}
			dragonslayer_holy_osnadurtha_legs = GameServer.Database.FindObjectByKey<ItemTemplate>("dragonslayer_holy_osnadurtha_legs");
			if (dragonslayer_holy_osnadurtha_legs == null)
			{
				dragonslayer_holy_osnadurtha_legs = new ItemTemplate();
				dragonslayer_holy_osnadurtha_legs.Name = "Dragonslayer Holy Osnadurtha Legs";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dragonslayer_holy_osnadurtha_legs.Name + ", creating it ...");
				dragonslayer_holy_osnadurtha_legs.Level = 51;
				dragonslayer_holy_osnadurtha_legs.Weight = 56;
				dragonslayer_holy_osnadurtha_legs.Model = 4090;
				dragonslayer_holy_osnadurtha_legs.Object_Type = 38;
				dragonslayer_holy_osnadurtha_legs.Item_Type = 27;
				dragonslayer_holy_osnadurtha_legs.Id_nb = "dragonslayer_holy_osnadurtha_legs";
				dragonslayer_holy_osnadurtha_legs.Hand = 0;
				dragonslayer_holy_osnadurtha_legs.IsPickable = true;
				dragonslayer_holy_osnadurtha_legs.IsDropable = true;
				dragonslayer_holy_osnadurtha_legs.IsTradable = true;
				dragonslayer_holy_osnadurtha_legs.CanDropAsLoot = true;
				dragonslayer_holy_osnadurtha_legs.Color = 0;
				dragonslayer_holy_osnadurtha_legs.Bonus = 35; // default bonus				
				dragonslayer_holy_osnadurtha_legs.Bonus1 = 22;
				dragonslayer_holy_osnadurtha_legs.Bonus1Type = (int) 2;
				dragonslayer_holy_osnadurtha_legs.Bonus2 = 40;
				dragonslayer_holy_osnadurtha_legs.Bonus2Type = (int) 10;
				dragonslayer_holy_osnadurtha_legs.Bonus3 = 5;
				dragonslayer_holy_osnadurtha_legs.Bonus3Type = (int) 12;
				dragonslayer_holy_osnadurtha_legs.Bonus4 = 5;
				dragonslayer_holy_osnadurtha_legs.Bonus4Type = (int) 18;
				dragonslayer_holy_osnadurtha_legs.Bonus5 = 5;
				dragonslayer_holy_osnadurtha_legs.Bonus5Type = (int) 15;
				dragonslayer_holy_osnadurtha_legs.Bonus6 = 1;
				dragonslayer_holy_osnadurtha_legs.Bonus6Type = (int) 191;
				dragonslayer_holy_osnadurtha_legs.Bonus7 = 5;
				dragonslayer_holy_osnadurtha_legs.Bonus7Type = (int) 202;
				dragonslayer_holy_osnadurtha_legs.Bonus8 = 0;
				dragonslayer_holy_osnadurtha_legs.Bonus8Type = (int) 0;
				dragonslayer_holy_osnadurtha_legs.Bonus9 = 0;
				dragonslayer_holy_osnadurtha_legs.Bonus9Type = (int) 0;
				dragonslayer_holy_osnadurtha_legs.Bonus10 = 0;
				dragonslayer_holy_osnadurtha_legs.Bonus10Type = (int) 0;
				dragonslayer_holy_osnadurtha_legs.ExtraBonus = 0;
				dragonslayer_holy_osnadurtha_legs.ExtraBonusType = (int) 0;
				dragonslayer_holy_osnadurtha_legs.Effect = 0;
				dragonslayer_holy_osnadurtha_legs.Emblem = 0;
				dragonslayer_holy_osnadurtha_legs.Charges = 0;
				dragonslayer_holy_osnadurtha_legs.MaxCharges = 0;
				dragonslayer_holy_osnadurtha_legs.SpellID = 0;
				dragonslayer_holy_osnadurtha_legs.ProcSpellID = 31001;
				dragonslayer_holy_osnadurtha_legs.Type_Damage = 0;
				dragonslayer_holy_osnadurtha_legs.Realm = 0;
				dragonslayer_holy_osnadurtha_legs.MaxCount = 1;
				dragonslayer_holy_osnadurtha_legs.PackSize = 1;
				dragonslayer_holy_osnadurtha_legs.Extension = 5;
				dragonslayer_holy_osnadurtha_legs.Quality = 100;				
				dragonslayer_holy_osnadurtha_legs.Condition = 50000;
				dragonslayer_holy_osnadurtha_legs.MaxCondition = 50000;
				dragonslayer_holy_osnadurtha_legs.Durability = 50000;
				dragonslayer_holy_osnadurtha_legs.MaxDurability = 0;
				dragonslayer_holy_osnadurtha_legs.PoisonCharges = 0;
				dragonslayer_holy_osnadurtha_legs.PoisonMaxCharges = 0;
				dragonslayer_holy_osnadurtha_legs.PoisonSpellID = 0;
				dragonslayer_holy_osnadurtha_legs.ProcSpellID1 = 40006;
				dragonslayer_holy_osnadurtha_legs.SpellID1 = 0;
				dragonslayer_holy_osnadurtha_legs.MaxCharges1 = 0;
				dragonslayer_holy_osnadurtha_legs.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(dragonslayer_holy_osnadurtha_legs);
				}


            #endregion

            #region defineAreas

            #endregion

            #region defineQuestParts

            QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerDruidQuest));
            QuestBehaviour a;
            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.Interact, null, Lirazal);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerDruidQuest), Lirazal);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerDruidQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "I have lived in this village since I was a young girl. My father was a great dragon hunter you know.", Lirazal);
            a.AddAction(eActionType.Talk, "In my years I have learned to work with all kinds of problems, specially with [Dragons].", Lirazal);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.Whisper, "Dragons", Lirazal);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerDruidQuest), Lirazal);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerDruidQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "yes dragons, but there is a problem for me, i can't hunt dragons by self, [you know].", Lirazal);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.Whisper, "you know", Lirazal);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerDruidQuest), Lirazal);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerDruidQuest), null, (eComparator)5);
            a.AddAction(eActionType.OfferQuest, typeof(DragonslayerDruidQuest), "Helen has offered you the Dragonslayer quest.?Do you accept?");
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.DeclineQuest, null, typeof(DragonslayerDruidQuest));
            a.AddAction(eActionType.Talk, "No problem. See you", Lirazal);
            AddBehaviour(a);

            a = builder.CreateBehaviour(Lirazal, -1);
            a.AddTrigger(eTriggerType.AcceptQuest, null, typeof(DragonslayerDruidQuest));
            a.AddAction(eActionType.Talk, "If you bring me the head of Asiintath I shall pay you well!", Lirazal);
            a.AddAction(eActionType.GiveQuest, typeof(DragonslayerDruidQuest), Lirazal);
            AddBehaviour(a);

			          						
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End
			if (Lirazal!=null) {
				Lirazal.AddQuestToGive(typeof (DragonslayerDruidQuest));
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
			Lirazal.RemoveQuestToGive(typeof (DragonslayerDruidQuest));
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
			if (player.IsDoingQuest(typeof (DragonslayerDruidQuest)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
			if (
		
			player.CharacterClass.ID != (byte) eCharacterClass.Druid && 
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
