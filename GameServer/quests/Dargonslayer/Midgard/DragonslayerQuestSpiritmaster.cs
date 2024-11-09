	
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
	public class DragonslayerSpiritmasterQuest : BaseQuest
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

		protected const string questTitle = "DragonslayerSpiritmasterQuest";

		protected const int minimumLevel = 49;
		protected const int maximumLevel = 50;
	
	
		private static GameNPC Jarkkonith = null;
		
		private static GameNPC AmarasavaDas = null;
		
		private static ItemTemplate Jarkkonith_Head = null;
		
		private static ItemTemplate Dragonslayer_Padded_Robes = null;
		
		private static ItemTemplate Dragonslayer_Padded_Sleeves = null;
		
		private static ItemTemplate Dragonslayer_Padded_Gloves = null;
		
		private static ItemTemplate Dragonslayer_Padded_Slippers = null;
		
		private static ItemTemplate Dragonslayer_Padded_Full_Helm = null;
		
		private static ItemTemplate Dragonslayer_Padded_Breeches_Mid = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public DragonslayerSpiritmasterQuest() : base()
		{
		}

		public DragonslayerSpiritmasterQuest(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public DragonslayerSpiritmasterQuest(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public DragonslayerSpiritmasterQuest(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
	{
	}
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			QuestBehaviour a;
			QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerSpiritmasterQuest));

			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(DragonslayerSpiritmasterQuest)) == null)
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
						a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerSpiritmasterQuest), null);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Padded_Robes, AmarasavaDas);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Padded_Sleeves, AmarasavaDas);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Padded_Gloves, AmarasavaDas);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Padded_Slippers, AmarasavaDas);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Padded_Full_Helm, AmarasavaDas);
						a.AddAction(eActionType.GiveItem, Dragonslayer_Padded_Breeches_Mid, AmarasavaDas);
						a.AddAction(eActionType.FinishQuest, typeof(DragonslayerSpiritmasterQuest), null);
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
			Dragonslayer_Padded_Robes = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Padded_Robes");
			if (Dragonslayer_Padded_Robes == null)
			{
				Dragonslayer_Padded_Robes = new ItemTemplate();
				Dragonslayer_Padded_Robes.Name = "Dragonslayer Padded Robes";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Padded_Robes.Name + ", creating it ...");
				Dragonslayer_Padded_Robes.Level = 51;
				Dragonslayer_Padded_Robes.Weight = 15;
				Dragonslayer_Padded_Robes.Model = 4051;
				Dragonslayer_Padded_Robes.Object_Type = 32;
				Dragonslayer_Padded_Robes.Item_Type = 25;
				Dragonslayer_Padded_Robes.Id_nb = "Dragonslayer_Padded_Robes";
				Dragonslayer_Padded_Robes.Hand = 0;
				Dragonslayer_Padded_Robes.IsPickable = true;
				Dragonslayer_Padded_Robes.IsDropable = true;
				Dragonslayer_Padded_Robes.IsTradable = true;
				Dragonslayer_Padded_Robes.CanDropAsLoot = true;
				Dragonslayer_Padded_Robes.Color = 0;
				Dragonslayer_Padded_Robes.Bonus = 35; // default bonus				
				Dragonslayer_Padded_Robes.Bonus1 = 22;
				Dragonslayer_Padded_Robes.Bonus1Type = (int) 2;
				Dragonslayer_Padded_Robes.Bonus2 = 5;
				Dragonslayer_Padded_Robes.Bonus2Type = (int) 12;
				Dragonslayer_Padded_Robes.Bonus3 = 5;
				Dragonslayer_Padded_Robes.Bonus3Type = (int) 15;
				Dragonslayer_Padded_Robes.Bonus4 = 5;
				Dragonslayer_Padded_Robes.Bonus4Type = (int) 18;
				Dragonslayer_Padded_Robes.Bonus5 = 22;
				Dragonslayer_Padded_Robes.Bonus5Type = (int) 156;
				Dragonslayer_Padded_Robes.Bonus6 = 5;
				Dragonslayer_Padded_Robes.Bonus6Type = (int) 202;
				Dragonslayer_Padded_Robes.Bonus7 = 5;
				Dragonslayer_Padded_Robes.Bonus7Type = (int) 209;
				Dragonslayer_Padded_Robes.Bonus8 = 5;
				Dragonslayer_Padded_Robes.Bonus8Type = (int) 191;
				Dragonslayer_Padded_Robes.Bonus9 = 0;
				Dragonslayer_Padded_Robes.Bonus9Type = (int) 0;
				Dragonslayer_Padded_Robes.Bonus10 = 0;
				Dragonslayer_Padded_Robes.Bonus10Type = (int) 0;
				Dragonslayer_Padded_Robes.ExtraBonus = 0;
				Dragonslayer_Padded_Robes.ExtraBonusType = (int) 0;
				Dragonslayer_Padded_Robes.Effect = 0;
				Dragonslayer_Padded_Robes.Emblem = 0;
				Dragonslayer_Padded_Robes.Charges = 0;
				Dragonslayer_Padded_Robes.MaxCharges = 0;
				Dragonslayer_Padded_Robes.SpellID = 0;
				Dragonslayer_Padded_Robes.ProcSpellID = 31001;
				Dragonslayer_Padded_Robes.Type_Damage = 0;
				Dragonslayer_Padded_Robes.Realm = 0;
				Dragonslayer_Padded_Robes.MaxCount = 1;
				Dragonslayer_Padded_Robes.PackSize = 1;
				Dragonslayer_Padded_Robes.Extension = 5;
				Dragonslayer_Padded_Robes.Quality = 100;				
				Dragonslayer_Padded_Robes.Condition = 50000;
				Dragonslayer_Padded_Robes.MaxCondition = 50000;
				Dragonslayer_Padded_Robes.Durability = 50000;
				Dragonslayer_Padded_Robes.MaxDurability = 0;
				Dragonslayer_Padded_Robes.PoisonCharges = 0;
				Dragonslayer_Padded_Robes.PoisonMaxCharges = 0;
				Dragonslayer_Padded_Robes.PoisonSpellID = 0;
				Dragonslayer_Padded_Robes.ProcSpellID1 = 40006;
				Dragonslayer_Padded_Robes.SpellID1 = 0;
				Dragonslayer_Padded_Robes.MaxCharges1 = 0;
				Dragonslayer_Padded_Robes.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Padded_Robes);
				}
			Dragonslayer_Padded_Sleeves = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Padded_Sleeves");
			if (Dragonslayer_Padded_Sleeves == null)
			{
				Dragonslayer_Padded_Sleeves = new ItemTemplate();
				Dragonslayer_Padded_Sleeves.Name = "Dragonslayer Padded Sleeves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Padded_Sleeves.Name + ", creating it ...");
				Dragonslayer_Padded_Sleeves.Level = 51;
				Dragonslayer_Padded_Sleeves.Weight = 8;
				Dragonslayer_Padded_Sleeves.Model = 4048;
				Dragonslayer_Padded_Sleeves.Object_Type = 32;
				Dragonslayer_Padded_Sleeves.Item_Type = 28;
				Dragonslayer_Padded_Sleeves.Id_nb = "Dragonslayer_Padded_Sleeves";
				Dragonslayer_Padded_Sleeves.Hand = 0;
				Dragonslayer_Padded_Sleeves.IsPickable = true;
				Dragonslayer_Padded_Sleeves.IsDropable = true;
				Dragonslayer_Padded_Sleeves.IsTradable = true;
				Dragonslayer_Padded_Sleeves.CanDropAsLoot = true;
				Dragonslayer_Padded_Sleeves.Color = 0;
				Dragonslayer_Padded_Sleeves.Bonus = 35; // default bonus				
				Dragonslayer_Padded_Sleeves.Bonus1 = 5;
				Dragonslayer_Padded_Sleeves.Bonus1Type = (int) 12;
				Dragonslayer_Padded_Sleeves.Bonus2 = 5;
				Dragonslayer_Padded_Sleeves.Bonus2Type = (int) 18;
				Dragonslayer_Padded_Sleeves.Bonus3 = 40;
				Dragonslayer_Padded_Sleeves.Bonus3Type = (int) 10;
				Dragonslayer_Padded_Sleeves.Bonus4 = 5;
				Dragonslayer_Padded_Sleeves.Bonus4Type = (int) 15;
				Dragonslayer_Padded_Sleeves.Bonus5 = 22;
				Dragonslayer_Padded_Sleeves.Bonus5Type = (int) 156;
				Dragonslayer_Padded_Sleeves.Bonus6 = 1;
				Dragonslayer_Padded_Sleeves.Bonus6Type = (int) 191;
				Dragonslayer_Padded_Sleeves.Bonus7 = 5;
				Dragonslayer_Padded_Sleeves.Bonus7Type = (int) 209;
				Dragonslayer_Padded_Sleeves.Bonus8 = 0;
				Dragonslayer_Padded_Sleeves.Bonus8Type = (int) 0;
				Dragonslayer_Padded_Sleeves.Bonus9 = 0;
				Dragonslayer_Padded_Sleeves.Bonus9Type = (int) 0;
				Dragonslayer_Padded_Sleeves.Bonus10 = 0;
				Dragonslayer_Padded_Sleeves.Bonus10Type = (int) 0;
				Dragonslayer_Padded_Sleeves.ExtraBonus = 0;
				Dragonslayer_Padded_Sleeves.ExtraBonusType = (int) 0;
				Dragonslayer_Padded_Sleeves.Effect = 0;
				Dragonslayer_Padded_Sleeves.Emblem = 0;
				Dragonslayer_Padded_Sleeves.Charges = 0;
				Dragonslayer_Padded_Sleeves.MaxCharges = 0;
				Dragonslayer_Padded_Sleeves.SpellID = 0;
				Dragonslayer_Padded_Sleeves.ProcSpellID = 31001;
				Dragonslayer_Padded_Sleeves.Type_Damage = 0;
				Dragonslayer_Padded_Sleeves.Realm = 0;
				Dragonslayer_Padded_Sleeves.MaxCount = 1;
				Dragonslayer_Padded_Sleeves.PackSize = 1;
				Dragonslayer_Padded_Sleeves.Extension = 5;
				Dragonslayer_Padded_Sleeves.Quality = 100;				
				Dragonslayer_Padded_Sleeves.Condition = 50000;
				Dragonslayer_Padded_Sleeves.MaxCondition = 50000;
				Dragonslayer_Padded_Sleeves.Durability = 50000;
				Dragonslayer_Padded_Sleeves.MaxDurability = 0;
				Dragonslayer_Padded_Sleeves.PoisonCharges = 0;
				Dragonslayer_Padded_Sleeves.PoisonMaxCharges = 0;
				Dragonslayer_Padded_Sleeves.PoisonSpellID = 0;
				Dragonslayer_Padded_Sleeves.ProcSpellID1 = 40006;
				Dragonslayer_Padded_Sleeves.SpellID1 = 0;
				Dragonslayer_Padded_Sleeves.MaxCharges1 = 0;
				Dragonslayer_Padded_Sleeves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Padded_Sleeves);
				}
			Dragonslayer_Padded_Gloves = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Padded_Gloves");
			if (Dragonslayer_Padded_Gloves == null)
			{
				Dragonslayer_Padded_Gloves = new ItemTemplate();
				Dragonslayer_Padded_Gloves.Name = "Dragonslayer Padded Gloves";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Padded_Gloves.Name + ", creating it ...");
				Dragonslayer_Padded_Gloves.Level = 51;
				Dragonslayer_Padded_Gloves.Weight = 8;
				Dragonslayer_Padded_Gloves.Model = 4049;
				Dragonslayer_Padded_Gloves.Object_Type = 32;
				Dragonslayer_Padded_Gloves.Item_Type = 22;
				Dragonslayer_Padded_Gloves.Id_nb = "Dragonslayer_Padded_Gloves";
				Dragonslayer_Padded_Gloves.Hand = 0;
				Dragonslayer_Padded_Gloves.IsPickable = true;
				Dragonslayer_Padded_Gloves.IsDropable = true;
				Dragonslayer_Padded_Gloves.IsTradable = true;
				Dragonslayer_Padded_Gloves.CanDropAsLoot = true;
				Dragonslayer_Padded_Gloves.Color = 0;
				Dragonslayer_Padded_Gloves.Bonus = 35; // default bonus				
				Dragonslayer_Padded_Gloves.Bonus1 = 5;
				Dragonslayer_Padded_Gloves.Bonus1Type = (int) 12;
				Dragonslayer_Padded_Gloves.Bonus2 = 5;
				Dragonslayer_Padded_Gloves.Bonus2Type = (int) 18;
				Dragonslayer_Padded_Gloves.Bonus3 = 40;
				Dragonslayer_Padded_Gloves.Bonus3Type = (int) 10;
				Dragonslayer_Padded_Gloves.Bonus4 = 5;
				Dragonslayer_Padded_Gloves.Bonus4Type = (int) 15;
				Dragonslayer_Padded_Gloves.Bonus5 = 3;
				Dragonslayer_Padded_Gloves.Bonus5Type = (int) 163;
				Dragonslayer_Padded_Gloves.Bonus6 = 40;
				Dragonslayer_Padded_Gloves.Bonus6Type = (int) 210;
				Dragonslayer_Padded_Gloves.Bonus7 = 6;
				Dragonslayer_Padded_Gloves.Bonus7Type = (int) 196;
				Dragonslayer_Padded_Gloves.Bonus8 = 0;
				Dragonslayer_Padded_Gloves.Bonus8Type = (int) 0;
				Dragonslayer_Padded_Gloves.Bonus9 = 0;
				Dragonslayer_Padded_Gloves.Bonus9Type = (int) 0;
				Dragonslayer_Padded_Gloves.Bonus10 = 0;
				Dragonslayer_Padded_Gloves.Bonus10Type = (int) 0;
				Dragonslayer_Padded_Gloves.ExtraBonus = 0;
				Dragonslayer_Padded_Gloves.ExtraBonusType = (int) 0;
				Dragonslayer_Padded_Gloves.Effect = 0;
				Dragonslayer_Padded_Gloves.Emblem = 0;
				Dragonslayer_Padded_Gloves.Charges = 0;
				Dragonslayer_Padded_Gloves.MaxCharges = 0;
				Dragonslayer_Padded_Gloves.SpellID = 0;
				Dragonslayer_Padded_Gloves.ProcSpellID = 31001;
				Dragonslayer_Padded_Gloves.Type_Damage = 0;
				Dragonslayer_Padded_Gloves.Realm = 0;
				Dragonslayer_Padded_Gloves.MaxCount = 1;
				Dragonslayer_Padded_Gloves.PackSize = 1;
				Dragonslayer_Padded_Gloves.Extension = 5;
				Dragonslayer_Padded_Gloves.Quality = 100;				
				Dragonslayer_Padded_Gloves.Condition = 50000;
				Dragonslayer_Padded_Gloves.MaxCondition = 50000;
				Dragonslayer_Padded_Gloves.Durability = 50000;
				Dragonslayer_Padded_Gloves.MaxDurability = 0;
				Dragonslayer_Padded_Gloves.PoisonCharges = 0;
				Dragonslayer_Padded_Gloves.PoisonMaxCharges = 0;
				Dragonslayer_Padded_Gloves.PoisonSpellID = 0;
				Dragonslayer_Padded_Gloves.ProcSpellID1 = 40006;
				Dragonslayer_Padded_Gloves.SpellID1 = 0;
				Dragonslayer_Padded_Gloves.MaxCharges1 = 0;
				Dragonslayer_Padded_Gloves.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Padded_Gloves);
				}
			Dragonslayer_Padded_Slippers = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Padded_Slippers");
			if (Dragonslayer_Padded_Slippers == null)
			{
				Dragonslayer_Padded_Slippers = new ItemTemplate();
				Dragonslayer_Padded_Slippers.Name = "Dragonslayer Padded Slippers";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Padded_Slippers.Name + ", creating it ...");
				Dragonslayer_Padded_Slippers.Level = 51;
				Dragonslayer_Padded_Slippers.Weight = 8;
				Dragonslayer_Padded_Slippers.Model = 4050;
				Dragonslayer_Padded_Slippers.Object_Type = 32;
				Dragonslayer_Padded_Slippers.Item_Type = 23;
				Dragonslayer_Padded_Slippers.Id_nb = "Dragonslayer_Padded_Slippers";
				Dragonslayer_Padded_Slippers.Hand = 0;
				Dragonslayer_Padded_Slippers.IsPickable = true;
				Dragonslayer_Padded_Slippers.IsDropable = true;
				Dragonslayer_Padded_Slippers.IsTradable = true;
				Dragonslayer_Padded_Slippers.CanDropAsLoot = true;
				Dragonslayer_Padded_Slippers.Color = 0;
				Dragonslayer_Padded_Slippers.Bonus = 35; // default bonus				
				Dragonslayer_Padded_Slippers.Bonus1 = 18;
				Dragonslayer_Padded_Slippers.Bonus1Type = (int) 2;
				Dragonslayer_Padded_Slippers.Bonus2 = 6;
				Dragonslayer_Padded_Slippers.Bonus2Type = (int) 13;
				Dragonslayer_Padded_Slippers.Bonus3 = 6;
				Dragonslayer_Padded_Slippers.Bonus3Type = (int) 17;
				Dragonslayer_Padded_Slippers.Bonus4 = 6;
				Dragonslayer_Padded_Slippers.Bonus4Type = (int) 19;
				Dragonslayer_Padded_Slippers.Bonus5 = 18;
				Dragonslayer_Padded_Slippers.Bonus5Type = (int) 156;
				Dragonslayer_Padded_Slippers.Bonus6 = 6;
				Dragonslayer_Padded_Slippers.Bonus6Type = (int) 196;
				Dragonslayer_Padded_Slippers.Bonus7 = 0;
				Dragonslayer_Padded_Slippers.Bonus7Type = (int) 0;
				Dragonslayer_Padded_Slippers.Bonus8 = 0;
				Dragonslayer_Padded_Slippers.Bonus8Type = (int) 0;
				Dragonslayer_Padded_Slippers.Bonus9 = 0;
				Dragonslayer_Padded_Slippers.Bonus9Type = (int) 0;
				Dragonslayer_Padded_Slippers.Bonus10 = 0;
				Dragonslayer_Padded_Slippers.Bonus10Type = (int) 0;
				Dragonslayer_Padded_Slippers.ExtraBonus = 0;
				Dragonslayer_Padded_Slippers.ExtraBonusType = (int) 0;
				Dragonslayer_Padded_Slippers.Effect = 0;
				Dragonslayer_Padded_Slippers.Emblem = 0;
				Dragonslayer_Padded_Slippers.Charges = 0;
				Dragonslayer_Padded_Slippers.MaxCharges = 0;
				Dragonslayer_Padded_Slippers.SpellID = 0;
				Dragonslayer_Padded_Slippers.ProcSpellID = 31001;
				Dragonslayer_Padded_Slippers.Type_Damage = 0;
				Dragonslayer_Padded_Slippers.Realm = 0;
				Dragonslayer_Padded_Slippers.MaxCount = 1;
				Dragonslayer_Padded_Slippers.PackSize = 1;
				Dragonslayer_Padded_Slippers.Extension = 5;
				Dragonslayer_Padded_Slippers.Quality = 100;				
				Dragonslayer_Padded_Slippers.Condition = 50000;
				Dragonslayer_Padded_Slippers.MaxCondition = 50000;
				Dragonslayer_Padded_Slippers.Durability = 50000;
				Dragonslayer_Padded_Slippers.MaxDurability = 0;
				Dragonslayer_Padded_Slippers.PoisonCharges = 0;
				Dragonslayer_Padded_Slippers.PoisonMaxCharges = 0;
				Dragonslayer_Padded_Slippers.PoisonSpellID = 0;
				Dragonslayer_Padded_Slippers.ProcSpellID1 = 40006;
				Dragonslayer_Padded_Slippers.SpellID1 = 0;
				Dragonslayer_Padded_Slippers.MaxCharges1 = 0;
				Dragonslayer_Padded_Slippers.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Padded_Slippers);
				}
			Dragonslayer_Padded_Full_Helm = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Padded_Full_Helm");
			if (Dragonslayer_Padded_Full_Helm == null)
			{
				Dragonslayer_Padded_Full_Helm = new ItemTemplate();
				Dragonslayer_Padded_Full_Helm.Name = "Dragonslayer Padded Full Helm";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Padded_Full_Helm.Name + ", creating it ...");
				Dragonslayer_Padded_Full_Helm.Level = 51;
				Dragonslayer_Padded_Full_Helm.Weight = 8;
				Dragonslayer_Padded_Full_Helm.Model = 4070;
				Dragonslayer_Padded_Full_Helm.Object_Type = 32;
				Dragonslayer_Padded_Full_Helm.Item_Type = 21;
				Dragonslayer_Padded_Full_Helm.Id_nb = "Dragonslayer_Padded_Full_Helm";
				Dragonslayer_Padded_Full_Helm.Hand = 0;
				Dragonslayer_Padded_Full_Helm.IsPickable = true;
				Dragonslayer_Padded_Full_Helm.IsDropable = true;
				Dragonslayer_Padded_Full_Helm.IsTradable = true;
				Dragonslayer_Padded_Full_Helm.CanDropAsLoot = true;
				Dragonslayer_Padded_Full_Helm.Color = 0;
				Dragonslayer_Padded_Full_Helm.Bonus = 35; // default bonus				
				Dragonslayer_Padded_Full_Helm.Bonus1 = 22;
				Dragonslayer_Padded_Full_Helm.Bonus1Type = (int) 3;
				Dragonslayer_Padded_Full_Helm.Bonus2 = 6;
				Dragonslayer_Padded_Full_Helm.Bonus2Type = (int) 16;
				Dragonslayer_Padded_Full_Helm.Bonus3 = 6;
				Dragonslayer_Padded_Full_Helm.Bonus3Type = (int) 11;
				Dragonslayer_Padded_Full_Helm.Bonus4 = 40;
				Dragonslayer_Padded_Full_Helm.Bonus4Type = (int) 10;
				Dragonslayer_Padded_Full_Helm.Bonus5 = 6;
				Dragonslayer_Padded_Full_Helm.Bonus5Type = (int) 14;
				Dragonslayer_Padded_Full_Helm.Bonus6 = 40;
				Dragonslayer_Padded_Full_Helm.Bonus6Type = (int) 210;
				Dragonslayer_Padded_Full_Helm.Bonus7 = 5;
				Dragonslayer_Padded_Full_Helm.Bonus7Type = (int) 148;
				Dragonslayer_Padded_Full_Helm.Bonus8 = 0;
				Dragonslayer_Padded_Full_Helm.Bonus8Type = (int) 0;
				Dragonslayer_Padded_Full_Helm.Bonus9 = 0;
				Dragonslayer_Padded_Full_Helm.Bonus9Type = (int) 0;
				Dragonslayer_Padded_Full_Helm.Bonus10 = 0;
				Dragonslayer_Padded_Full_Helm.Bonus10Type = (int) 0;
				Dragonslayer_Padded_Full_Helm.ExtraBonus = 0;
				Dragonslayer_Padded_Full_Helm.ExtraBonusType = (int) 0;
				Dragonslayer_Padded_Full_Helm.Effect = 0;
				Dragonslayer_Padded_Full_Helm.Emblem = 0;
				Dragonslayer_Padded_Full_Helm.Charges = 0;
				Dragonslayer_Padded_Full_Helm.MaxCharges = 0;
				Dragonslayer_Padded_Full_Helm.SpellID = 0;
				Dragonslayer_Padded_Full_Helm.ProcSpellID = 31001;
				Dragonslayer_Padded_Full_Helm.Type_Damage = 0;
				Dragonslayer_Padded_Full_Helm.Realm = 0;
				Dragonslayer_Padded_Full_Helm.MaxCount = 1;
				Dragonslayer_Padded_Full_Helm.PackSize = 1;
				Dragonslayer_Padded_Full_Helm.Extension = 0;
				Dragonslayer_Padded_Full_Helm.Quality = 100;				
				Dragonslayer_Padded_Full_Helm.Condition = 50000;
				Dragonslayer_Padded_Full_Helm.MaxCondition = 50000;
				Dragonslayer_Padded_Full_Helm.Durability = 50000;
				Dragonslayer_Padded_Full_Helm.MaxDurability = 0;
				Dragonslayer_Padded_Full_Helm.PoisonCharges = 0;
				Dragonslayer_Padded_Full_Helm.PoisonMaxCharges = 0;
				Dragonslayer_Padded_Full_Helm.PoisonSpellID = 0;
				Dragonslayer_Padded_Full_Helm.ProcSpellID1 = 40006;
				Dragonslayer_Padded_Full_Helm.SpellID1 = 0;
				Dragonslayer_Padded_Full_Helm.MaxCharges1 = 0;
				Dragonslayer_Padded_Full_Helm.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Padded_Full_Helm);
				}
			Dragonslayer_Padded_Breeches_Mid = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Padded_Breeches_Mid");
			if (Dragonslayer_Padded_Breeches_Mid == null)
			{
				Dragonslayer_Padded_Breeches_Mid = new ItemTemplate();
				Dragonslayer_Padded_Breeches_Mid.Name = "Dragonslayer Padded Breeches";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Dragonslayer_Padded_Breeches_Mid.Name + ", creating it ...");
				Dragonslayer_Padded_Breeches_Mid.Level = 51;
				Dragonslayer_Padded_Breeches_Mid.Weight = 10;
				Dragonslayer_Padded_Breeches_Mid.Model = 4047;
				Dragonslayer_Padded_Breeches_Mid.Object_Type = 32;
				Dragonslayer_Padded_Breeches_Mid.Item_Type = 27;
				Dragonslayer_Padded_Breeches_Mid.Id_nb = "Dragonslayer_Padded_Breeches_Mid";
				Dragonslayer_Padded_Breeches_Mid.Hand = 0;
				Dragonslayer_Padded_Breeches_Mid.IsPickable = true;
				Dragonslayer_Padded_Breeches_Mid.IsDropable = true;
				Dragonslayer_Padded_Breeches_Mid.IsTradable = true;
				Dragonslayer_Padded_Breeches_Mid.CanDropAsLoot = true;
				Dragonslayer_Padded_Breeches_Mid.Color = 0;
				Dragonslayer_Padded_Breeches_Mid.Bonus = 35; // default bonus				
				Dragonslayer_Padded_Breeches_Mid.Bonus1 = 22;
				Dragonslayer_Padded_Breeches_Mid.Bonus1Type = (int) 2;
				Dragonslayer_Padded_Breeches_Mid.Bonus2 = 5;
				Dragonslayer_Padded_Breeches_Mid.Bonus2Type = (int) 12;
				Dragonslayer_Padded_Breeches_Mid.Bonus3 = 5;
				Dragonslayer_Padded_Breeches_Mid.Bonus3Type = (int) 18;
				Dragonslayer_Padded_Breeches_Mid.Bonus4 = 40;
				Dragonslayer_Padded_Breeches_Mid.Bonus4Type = (int) 10;
				Dragonslayer_Padded_Breeches_Mid.Bonus5 = 5;
				Dragonslayer_Padded_Breeches_Mid.Bonus5Type = (int) 15;
				Dragonslayer_Padded_Breeches_Mid.Bonus6 = 5;
				Dragonslayer_Padded_Breeches_Mid.Bonus6Type = (int) 202;
				Dragonslayer_Padded_Breeches_Mid.Bonus7 = 1;
				Dragonslayer_Padded_Breeches_Mid.Bonus7Type = (int) 191;
				Dragonslayer_Padded_Breeches_Mid.Bonus8 = 0;
				Dragonslayer_Padded_Breeches_Mid.Bonus8Type = (int) 0;
				Dragonslayer_Padded_Breeches_Mid.Bonus9 = 0;
				Dragonslayer_Padded_Breeches_Mid.Bonus9Type = (int) 0;
				Dragonslayer_Padded_Breeches_Mid.Bonus10 = 0;
				Dragonslayer_Padded_Breeches_Mid.Bonus10Type = (int) 0;
				Dragonslayer_Padded_Breeches_Mid.ExtraBonus = 0;
				Dragonslayer_Padded_Breeches_Mid.ExtraBonusType = (int) 0;
				Dragonslayer_Padded_Breeches_Mid.Effect = 0;
				Dragonslayer_Padded_Breeches_Mid.Emblem = 0;
				Dragonslayer_Padded_Breeches_Mid.Charges = 0;
				Dragonslayer_Padded_Breeches_Mid.MaxCharges = 0;
				Dragonslayer_Padded_Breeches_Mid.SpellID = 0;
				Dragonslayer_Padded_Breeches_Mid.ProcSpellID = 31001;
				Dragonslayer_Padded_Breeches_Mid.Type_Damage = 0;
				Dragonslayer_Padded_Breeches_Mid.Realm = 2;
				Dragonslayer_Padded_Breeches_Mid.MaxCount = 1;
				Dragonslayer_Padded_Breeches_Mid.PackSize = 1;
				Dragonslayer_Padded_Breeches_Mid.Extension = 5;
				Dragonslayer_Padded_Breeches_Mid.Quality = 100;				
				Dragonslayer_Padded_Breeches_Mid.Condition = 50000;
				Dragonslayer_Padded_Breeches_Mid.MaxCondition = 50000;
				Dragonslayer_Padded_Breeches_Mid.Durability = 50000;
				Dragonslayer_Padded_Breeches_Mid.MaxDurability = 0;
				Dragonslayer_Padded_Breeches_Mid.PoisonCharges = 0;
				Dragonslayer_Padded_Breeches_Mid.PoisonMaxCharges = 0;
				Dragonslayer_Padded_Breeches_Mid.PoisonSpellID = 0;
				Dragonslayer_Padded_Breeches_Mid.ProcSpellID1 = 40006;
				Dragonslayer_Padded_Breeches_Mid.SpellID1 = 0;
				Dragonslayer_Padded_Breeches_Mid.MaxCharges1 = 0;
				Dragonslayer_Padded_Breeches_Mid.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddObject(Dragonslayer_Padded_Breeches_Mid);
				}


            #endregion

            #region defineAreas

            #endregion

            #region defineQuestParts


            QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerSpiritmasterQuest));
            QuestBehaviour a;
            a = builder.CreateBehaviour(AmarasavaDas, -1);
            a.AddTrigger(eTriggerType.Interact, null, AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerSpiritmasterQuest), AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerSpiritmasterQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "I have lived in this village since I was a young girl. My father was a great dragon hunter you know.", AmarasavaDas);
            a.AddAction(eActionType.Talk, "In my years I have learned to work with all kinds of problems, specially with [Dragons].", AmarasavaDas);
            AddBehaviour(a);

            a = builder.CreateBehaviour(AmarasavaDas, -1);
            a.AddTrigger(eTriggerType.Whisper, "Dragons", AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerSpiritmasterQuest), AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerSpiritmasterQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "yes dragons, but there is a problem for me, i can't hunt dragons by self, [you know].", AmarasavaDas);
            AddBehaviour(a);

            a = builder.CreateBehaviour(AmarasavaDas, -1);
            a.AddTrigger(eTriggerType.Whisper, "you know", AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerSpiritmasterQuest), AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerSpiritmasterQuest), null, (eComparator)5);
            a.AddAction(eActionType.OfferQuest, typeof(DragonslayerSpiritmasterQuest), "Helen has offered you the Dragonslayer quest.?Do you accept?");
            AddBehaviour(a);

            a = builder.CreateBehaviour(AmarasavaDas, -1);
            a.AddTrigger(eTriggerType.DeclineQuest, null, typeof(DragonslayerSpiritmasterQuest));
            a.AddAction(eActionType.Talk, "No problem. See you", AmarasavaDas);
            AddBehaviour(a);

            a = builder.CreateBehaviour(AmarasavaDas, -1);
            a.AddTrigger(eTriggerType.AcceptQuest, null, typeof(DragonslayerSpiritmasterQuest));
            a.AddAction(eActionType.Talk, "If you bring me the head of Jarkkonith I shall pay you well!", AmarasavaDas);
            a.AddAction(eActionType.GiveQuest, typeof(DragonslayerSpiritmasterQuest), AmarasavaDas);
            AddBehaviour(a);


           
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End
			if (AmarasavaDas!=null) {
				AmarasavaDas.AddQuestToGive(typeof (DragonslayerSpiritmasterQuest));
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
			AmarasavaDas.RemoveQuestToGive(typeof (DragonslayerSpiritmasterQuest));
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
			if (player.IsDoingQuest(typeof (DragonslayerSpiritmasterQuest)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
			if (
		
			player.CharacterClass.ID != (byte) eCharacterClass.Spiritmaster && 
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
