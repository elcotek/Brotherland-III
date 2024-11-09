	
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
* Author:	Lamyuras
* Date:		
*
* Notes:
*  
*/

using System;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.Behaviour;
using DOL.GS.Behaviour.Attributes;
using DOL.GS.PacketHandler;
using DOL.GS.Quests;
using DOL.Language;
using log4net;

namespace DOL.GS.Quests.Midgard
{
     /* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * BaseQuest	  	 
	 */
	public class Abearybadproblem : BaseQuest
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

        protected static string questTitle = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.ABearyBadProblem.QuestTitle");

		protected const int minimumLevel = 1;
		protected const int maximumLevel = 50;
	
	
		private static GameNPC VikingKreimhilde = null;
		
		private static ItemTemplate silver_ring_of_health = null;
		
		private static ItemTemplate black_mauler_cub_pelt = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public Abearybadproblem() : base()
		{
		}

		public Abearybadproblem(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public Abearybadproblem(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Abearybadproblem(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
	{
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
	
            npcs = WorldMgr.GetNPCsByName(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.ABearyBadProblem.NPCKreimhilde"),(eRealm) 2);
			if (npcs.Length == 0)
			{
                VikingKreimhilde = new DOL.GS.GameNPC
                {
                    Model = 218,
                    Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.ABearyBadProblem.NPCKreimhilde")
                };
                if (log.IsWarnEnabled)
					log.Warn("Could not find " + VikingKreimhilde.Name + ", creating ...");
				VikingKreimhilde.GuildName = "Part of " + questTitle + " Quest";
				VikingKreimhilde.Realm = eRealm.Midgard;
				VikingKreimhilde.CurrentRegionID = 100;
				VikingKreimhilde.Size = 51;
				VikingKreimhilde.Level = 50;
				VikingKreimhilde.MaxSpeedBase = 191;
				VikingKreimhilde.Faction = FactionMgr.GetFactionByID(0);
				VikingKreimhilde.X = 803999;
				VikingKreimhilde.Y = 726551;
				VikingKreimhilde.Z = 4752;
				VikingKreimhilde.Heading = 2116;
				VikingKreimhilde.RespawnInterval = -1;
				VikingKreimhilde.BodyType = 0;

                StandardMobBrain brain = new StandardMobBrain
                {
                    AggroLevel = 0,
                    AggroRange = 500
                };
                VikingKreimhilde.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					VikingKreimhilde.SaveIntoDatabase();
					
				VikingKreimhilde.AddToWorld();
				
			}
			else 
			{
				VikingKreimhilde = npcs[0];
			}
		

			#endregion

			#region defineItems

            silver_ring_of_health = GameServer.Database.FindObjectByKey<ItemTemplate>("silver_ring_of_health");
			if (silver_ring_of_health == null)
			{
                silver_ring_of_health = new ItemTemplate
                {
                    Name = "Silver Ring of Health"
                };
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + silver_ring_of_health.Name + ", creating it ...");
                silver_ring_of_health.Level = 5;
                silver_ring_of_health.Weight = 5;
                silver_ring_of_health.Model = 103;
                silver_ring_of_health.Object_Type = 41;
                silver_ring_of_health.Item_Type = 35;
                silver_ring_of_health.Id_nb = "silver_ring_of_health";
                silver_ring_of_health.Hand = 0;
                silver_ring_of_health.Price = 0;
                silver_ring_of_health.IsPickable = true;
                silver_ring_of_health.IsDropable = true;
                silver_ring_of_health.IsTradable = true;
                silver_ring_of_health.CanDropAsLoot = false;
                silver_ring_of_health.Color = 0;
                silver_ring_of_health.Bonus = 5; // default bonus				
                silver_ring_of_health.Bonus1 = 12;
                silver_ring_of_health.Bonus1Type = (int)10;
                silver_ring_of_health.Bonus2 = 0;
                silver_ring_of_health.Bonus2Type = (int)0;
                silver_ring_of_health.Bonus3 = 0;
                silver_ring_of_health.Bonus3Type = (int)0;
                silver_ring_of_health.Bonus4 = 0;
                silver_ring_of_health.Bonus4Type = (int)0;
                silver_ring_of_health.Bonus5 = 0;
                silver_ring_of_health.Bonus5Type = (int)0;
                silver_ring_of_health.Bonus6 = 0;
                silver_ring_of_health.Bonus6Type = (int)0;
                silver_ring_of_health.Bonus7 = 0;
                silver_ring_of_health.Bonus7Type = (int)0;
                silver_ring_of_health.Bonus8 = 0;
                silver_ring_of_health.Bonus8Type = (int)0;
                silver_ring_of_health.Bonus9 = 0;
                silver_ring_of_health.Bonus9Type = (int)0;
                silver_ring_of_health.Bonus10 = 0;
                silver_ring_of_health.Bonus10Type = (int)0;
                silver_ring_of_health.ExtraBonus = 0;
                silver_ring_of_health.ExtraBonusType = (int)0;
                silver_ring_of_health.Effect = 0;
                silver_ring_of_health.Emblem = 0;
                silver_ring_of_health.Charges = 0;
                silver_ring_of_health.MaxCharges = 0;
                silver_ring_of_health.SpellID = 0;
                silver_ring_of_health.ProcSpellID = 0;
                silver_ring_of_health.Type_Damage = 0;
                silver_ring_of_health.Realm = 0;
                silver_ring_of_health.MaxCount = 1;
                silver_ring_of_health.PackSize = 1;
                silver_ring_of_health.Extension = 0;
                silver_ring_of_health.Quality = 100;
                silver_ring_of_health.Condition = 100;
                silver_ring_of_health.MaxCondition = 100;
                silver_ring_of_health.Durability = 100;
                silver_ring_of_health.MaxDurability = 100;
                silver_ring_of_health.PoisonCharges = 0;
                silver_ring_of_health.PoisonMaxCharges = 0;
                silver_ring_of_health.PoisonSpellID = 0;
                silver_ring_of_health.ProcSpellID1 = 0;
                silver_ring_of_health.SpellID1 = 0;
                silver_ring_of_health.MaxCharges1 = 0;
                silver_ring_of_health.Charges1 = 0;
                silver_ring_of_health.RepearCount = 200;
                GameServer.Database.AddObject(silver_ring_of_health);
				}
            black_mauler_cub_pelt = GameServer.Database.FindObjectByKey<ItemTemplate>("black_mauler_cub_pelt");
			if (black_mauler_cub_pelt == null)
			{
                black_mauler_cub_pelt = new ItemTemplate
                {
                    Name = "Black Mauler Cub Pelt"
                };
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + black_mauler_cub_pelt.Name + ", creating it ...");
                black_mauler_cub_pelt.Level = 1;
                black_mauler_cub_pelt.Weight = 5;
                black_mauler_cub_pelt.Model = 100;
                black_mauler_cub_pelt.Object_Type = 0;
                black_mauler_cub_pelt.Item_Type = 40;
                black_mauler_cub_pelt.Id_nb = "black_mauler_cub_pelt";
                black_mauler_cub_pelt.Hand = 0;
                black_mauler_cub_pelt.Price = 0;
                black_mauler_cub_pelt.IsPickable = true;
                black_mauler_cub_pelt.IsDropable = true;
                black_mauler_cub_pelt.IsTradable = true;
                black_mauler_cub_pelt.CanDropAsLoot = true;
                black_mauler_cub_pelt.Color = 0;
                black_mauler_cub_pelt.Bonus = 35; // default bonus				
                black_mauler_cub_pelt.Bonus1 = 0;
                black_mauler_cub_pelt.Bonus1Type = (int)0;
                black_mauler_cub_pelt.Bonus2 = 0;
                black_mauler_cub_pelt.Bonus2Type = (int)0;
                black_mauler_cub_pelt.Bonus3 = 0;
                black_mauler_cub_pelt.Bonus3Type = (int)0;
                black_mauler_cub_pelt.Bonus4 = 0;
                black_mauler_cub_pelt.Bonus4Type = (int)0;
                black_mauler_cub_pelt.Bonus5 = 0;
                black_mauler_cub_pelt.Bonus5Type = (int)0;
                black_mauler_cub_pelt.Bonus6 = 0;
                black_mauler_cub_pelt.Bonus6Type = (int)0;
                black_mauler_cub_pelt.Bonus7 = 0;
                black_mauler_cub_pelt.Bonus7Type = (int)0;
                black_mauler_cub_pelt.Bonus8 = 0;
                black_mauler_cub_pelt.Bonus8Type = (int)0;
                black_mauler_cub_pelt.Bonus9 = 0;
                black_mauler_cub_pelt.Bonus9Type = (int)0;
                black_mauler_cub_pelt.Bonus10 = 0;
                black_mauler_cub_pelt.Bonus10Type = (int)0;
                black_mauler_cub_pelt.ExtraBonus = 0;
                black_mauler_cub_pelt.ExtraBonusType = (int)0;
                black_mauler_cub_pelt.Effect = 0;
                black_mauler_cub_pelt.Emblem = 0;
                black_mauler_cub_pelt.Charges = 0;
                black_mauler_cub_pelt.MaxCharges = 0;
                black_mauler_cub_pelt.SpellID = 0;
                black_mauler_cub_pelt.ProcSpellID = 0;
                black_mauler_cub_pelt.Type_Damage = 0;
                black_mauler_cub_pelt.Realm = 0;
                black_mauler_cub_pelt.MaxCount = 1;
                black_mauler_cub_pelt.PackSize = 1;
                black_mauler_cub_pelt.Extension = 0;
                black_mauler_cub_pelt.Quality = 99;
                black_mauler_cub_pelt.Condition = 100;
                black_mauler_cub_pelt.MaxCondition = 100;
                black_mauler_cub_pelt.Durability = 100;
                black_mauler_cub_pelt.MaxDurability = 100;
                black_mauler_cub_pelt.PoisonCharges = 0;
                black_mauler_cub_pelt.PoisonMaxCharges = 0;
                black_mauler_cub_pelt.PoisonSpellID = 0;
                black_mauler_cub_pelt.ProcSpellID1 = 0;
                black_mauler_cub_pelt.SpellID1 = 0;
                black_mauler_cub_pelt.MaxCharges1 = 0;
                black_mauler_cub_pelt.Charges1 = 0;
                black_mauler_cub_pelt.RepearCount = 0;

                GameServer.Database.AddObject(black_mauler_cub_pelt);
				}
			

			#endregion

			#region defineAreas
			
		#endregion
		
		#region defineQuestParts

		QuestBuilder builder = QuestMgr.getBuilder(typeof(Abearybadproblem));
			QuestBehaviour a;
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
			    a.AddTrigger(eTriggerType.Interact,null,VikingKreimhilde);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.Abearybadproblem),VikingKreimhilde);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.Abearybadproblem),null,(eComparator)5);
            a.AddAction(eActionType.Talk, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.ABearyBadProblem.Talk1"), VikingKreimhilde);
            a.AddAction(eActionType.Talk, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.ABearyBadProblem.Talk2"), VikingKreimhilde);
            a.AddAction(eActionType.Talk, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.ABearyBadProblem.Talk3"), VikingKreimhilde);
            a.AddAction(eActionType.Talk, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.ABearyBadProblem.Talk4"), VikingKreimhilde);
            a.AddAction(eActionType.OfferQuest, typeof(DOL.GS.Quests.Midgard.Abearybadproblem), LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.ABearyBadProblem.OfferQuest"));
            AddBehaviour(a);
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
			    a.AddTrigger(eTriggerType.AcceptQuest,null,typeof(DOL.GS.Quests.Midgard.Abearybadproblem));
			a.AddAction(eActionType.GiveQuest,typeof(DOL.GS.Quests.Midgard.Abearybadproblem),VikingKreimhilde);
			a.AddAction(eActionType.SetQuestStep,typeof(DOL.GS.Quests.Midgard.Abearybadproblem),1);
		    AddBehaviour(a);
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
			    a.AddTrigger(eTriggerType.DeclineQuest,null,typeof(DOL.GS.Quests.Midgard.Abearybadproblem));
		    AddBehaviour(a);
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
                a.AddTrigger(eTriggerType.EnemyKilled, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.ABearyBadProblem.EnemyKilled"), null);
            a.AddRequirement(eRequirementType.QuestStep, typeof(DOL.GS.Quests.Midgard.Abearybadproblem), 1, (eComparator)3);
			a.AddAction(eActionType.GiveItem,black_mauler_cub_pelt,null);
			a.AddAction(eActionType.SetQuestStep,typeof(DOL.GS.Quests.Midgard.Abearybadproblem),2);
		    AddBehaviour(a);
			a = builder.CreateBehaviour(VikingKreimhilde,-1);
				a.AddTrigger(eTriggerType.Interact,null,VikingKreimhilde);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.Abearybadproblem),2,(eComparator)3);
            a.AddAction(eActionType.Talk, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.ABearyBadProblem.GoodJob"), VikingKreimhilde);
            a.AddAction(eActionType.GiveXP, 22, null);
			a.AddAction(eActionType.GiveGold,23,null);
			a.AddAction(eActionType.TakeItem,black_mauler_cub_pelt, null);
            a.AddAction(eActionType.GiveItem,silver_ring_of_health,VikingKreimhilde);
			a.AddAction(eActionType.FinishQuest,typeof(DOL.GS.Quests.Midgard.Abearybadproblem),null);
			AddBehaviour(a);
			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End

			VikingKreimhilde.AddQuestToGive(typeof (Abearybadproblem));
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{			
				
			// Custom Scriptunloaded Code Begin
			
			// Custom Scriptunloaded Code End

			/* If VikingKreimhilde has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (VikingKreimhilde == null)
				return;
			/* Now we remove the possibility to give this quest to players */			
			VikingKreimhilde.RemoveQuestToGive(typeof (Abearybadproblem));
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
                    return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.ABearyBadProblem.Step1");
				
					case 2:
                    return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Mid.ABearyBadProblem.Step2");
				
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
			if (player.IsDoingQuest(typeof (Abearybadproblem)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
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
