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
 * Author:		k109
 * 
 * Date:		12/5/07
 * Directory: /scripts/quests/albion/
 *
 * Description:
 *  Brief Walkthrough:
 * 1) Talk with Lady Grynoch in Cotswold Village
 * 2) Find the Entrance to the Demon's Breach and then head back to her.
 * You will receive some xp, copper and the armor of your choice.
 */
using System;
using DOL.Database;
using DOL.Events;
using DOL.Language;

namespace DOL.GS.Quests.Albion
{
	public class ToReachTheBreach : RewardQuest
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		protected static string questTitle = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.ToReachTheBreach.QuestTitle");
		protected const int minimumLevel = 2;
		protected const int maximumLevel = 5;

		private static GameNPC LadyGrynoch = null;
		private QuestGoal FoundBreach;

		private static readonly GameLocation Demons_Breach = new GameLocation("Demon's Breach", 1, 562731, 514531, 2751);

		private static IArea Demons_Breach_Area = null;

		private static ItemTemplate RecruitsIntelligentBelt;
		private static ItemTemplate RecruitsMightyBelt;
		private static ItemTemplate RecruitsPiousBelt;

		public ToReachTheBreach()
			: base()
		{
			Init();
		}

		public ToReachTheBreach(GamePlayer questingPlayer)
			: this(questingPlayer, 1)
		{
		}

		public ToReachTheBreach(GamePlayer questingPlayer, int step)
			: base(questingPlayer, step)
		{
			Init();
		}

		public ToReachTheBreach(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest)
		{
			Init();
		}

		private void Init()
		{
			#region defineItems
			RecruitsIntelligentBelt = GameServer.Database.FindObjectByKey<ItemTemplate>("RecruitsIntelligentBelt");
			if(RecruitsIntelligentBelt == null)
			{
                RecruitsIntelligentBelt = new ItemTemplate
                {
                    Name = LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Alb.ToReachTheBreach.Init.Text1"),
                    Id_nb = "RecruitsIntelligentBelt",
                    Level = 4,
                    Weight = 3,
                    Model = 597,
                    Color = 0,
                    Object_Type = (int)eObjectType.Magical,
                    Item_Type = (int)eEquipmentItems.WAIST,
                    Price = Money.GetMoney(0, 0, 0, 0, 10),
                    Quality = 100,
                    Condition = 50000,
                    MaxCondition = 50000,
                    Durability = 50000,
                    MaxDurability = 50000,
                    Bonus1 = 3,
                    Bonus1Type = (int)eProperty.Acuity,
                    Bonus2 = 3,
                    Bonus2Type = (int)eProperty.Dexterity,
                    Bonus3 = 1,
                    Bonus3Type = (int)eProperty.Constitution,
                    Bonus4 = 8,
                    Bonus4Type = (int)eProperty.MaxHealth,
                    IsDropable = false
                };
                GameServer.Database.AddObject(RecruitsIntelligentBelt);
			}
			RecruitsMightyBelt = GameServer.Database.FindObjectByKey<ItemTemplate>("RecruitsMightyBelt");
			if(RecruitsMightyBelt == null)
			{
                RecruitsMightyBelt = new ItemTemplate
                {
                    Name = LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Alb.ToReachTheBreach.Init.Text2"),
                    Id_nb = "RecruitsMightyBelt",
                    Level = 4,
                    Weight = 3,
                    Model = 597,
                    Color = 0,
                    Object_Type = (int)eObjectType.Magical,
                    Item_Type = (int)eEquipmentItems.WAIST,
                    Price = Money.GetMoney(0, 0, 0, 0, 10),
                    Quality = 100,
                    Condition = 50000,
                    MaxCondition = 50000,
                    Durability = 50000,
                    MaxDurability = 50000,
                    Bonus1 = 3,
                    Bonus1Type = (int)eProperty.Strength,
                    Bonus2 = 3,
                    Bonus2Type = (int)eProperty.Dexterity,
                    Bonus3 = 1,
                    Bonus3Type = (int)eProperty.Quickness,
                    Bonus4 = 8,
                    Bonus4Type = (int)eProperty.MaxHealth,
                    IsDropable = false
                };
                GameServer.Database.AddObject(RecruitsMightyBelt);
			}
			RecruitsPiousBelt = GameServer.Database.FindObjectByKey<ItemTemplate>("RecruitsPiousBelt");
			if(RecruitsPiousBelt == null)
			{
                RecruitsPiousBelt = new ItemTemplate
                {
                    Name = LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Alb.ToReachTheBreach.Init.Text3"),
                    Id_nb = "RecruitsPiousBelt",
                    Level = 4,
                    Weight = 3,
                    Model = 597,
                    Color = 0,
                    Object_Type = (int)eObjectType.Magical,
                    Item_Type = (int)eEquipmentItems.WAIST,
                    Price = Money.GetMoney(0, 0, 0, 0, 10),
                    Quality = 100,
                    Condition = 50000,
                    MaxCondition = 50000,
                    Durability = 50000,
                    MaxDurability = 50000,
                    Bonus1 = 3,
                    Bonus1Type = (int)eProperty.Acuity,
                    Bonus2 = 1,
                    Bonus2Type = (int)eProperty.AllMagicSkills,
                    Bonus3 = 3,
                    Bonus3Type = (int)eProperty.Dexterity,
                    Bonus4 = 8,
                    Bonus4Type = (int)eProperty.MaxHealth,
                    IsDropable = false
                };
                GameServer.Database.AddObject(RecruitsPiousBelt);
			}
			#endregion

			Level = 3;
			QuestGiver = LadyGrynoch;
			Rewards.Experience = 90;
			Rewards.MoneyPercent = 100;
			Rewards.AddOptionalItem(RecruitsIntelligentBelt);
			Rewards.AddOptionalItem(RecruitsMightyBelt);
			Rewards.AddOptionalItem(RecruitsPiousBelt);
			Rewards.ChoiceOf = 1;

			FoundBreach = AddGoal(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.ToReachTheBreach.Init.Text4"), QuestGoal.GoalType.ScoutMission, 1, null);
		}

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");


			#region defineNPCS

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Lady Grynoch", eRealm.Albion);

			if (npcs.Length == 0)
			{
                LadyGrynoch = new GameNPC
                {
                    Model = 5,
                    Name = "Lady Grynoch"
                };
                if (log.IsWarnEnabled)
					log.Warn("Could not find " + LadyGrynoch.Name + ", creating her ...");
				//k109: My preference, no guildname for quest NPCs.  Uncomment if you like that...
				//LadyGrynoch.GuildName = "Part of " + questTitle + " Quest";
				LadyGrynoch.Realm = eRealm.Albion;
				LadyGrynoch.CurrentRegionID = 1;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 58);    //Slot 25
				LadyGrynoch.Inventory = template.CloseTemplate();
				LadyGrynoch.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				LadyGrynoch.Size = 51;
				LadyGrynoch.Level = 38;
				LadyGrynoch.X = 559698;
				LadyGrynoch.Y = 513578;
				LadyGrynoch.Z = 2428;
				LadyGrynoch.Heading = 2742;

				if (SAVE_INTO_DATABASE)
					LadyGrynoch.SaveIntoDatabase();

				LadyGrynoch.AddToWorld();
			}
			else
				LadyGrynoch = npcs[0];

			#endregion
			#region defineAreas
			Demons_Breach_Area = WorldMgr.GetRegion(Demons_Breach.RegionID).AddArea(new Area.Circle("", Demons_Breach.X, Demons_Breach.Y, Demons_Breach.Z, 200));
			Demons_Breach_Area.RegisterPlayerEnter(new DOLEventHandler(PlayerEnterDemonBreachArea));
			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(LadyGrynoch, GameLivingEvent.Interact, new DOLEventHandler(TalkToLadyGrynoch));
			GameEventMgr.AddHandler(LadyGrynoch, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLadyGrynoch));

			LadyGrynoch.AddQuestToGive(typeof(ToReachTheBreach));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			if (LadyGrynoch == null)
				return;

			Demons_Breach_Area.UnRegisterPlayerEnter(new DOLEventHandler(PlayerEnterDemonBreachArea));
			WorldMgr.GetRegion(Demons_Breach.RegionID).RemoveArea(Demons_Breach_Area);

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(LadyGrynoch, GameObjectEvent.Interact, new DOLEventHandler(TalkToLadyGrynoch));
			GameEventMgr.RemoveHandler(LadyGrynoch, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLadyGrynoch));

			LadyGrynoch.RemoveQuestToGive(typeof(ToReachTheBreach));
		}

		protected static void TalkToLadyGrynoch(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (LadyGrynoch.CanGiveQuest(typeof(ToReachTheBreach), player) <= 0)
				return;


			ToReachTheBreach quest = player.IsDoingQuest(typeof(ToReachTheBreach)) as ToReachTheBreach;
			LadyGrynoch.TurnTo(player);

			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
                    quest = new ToReachTheBreach
                    {
                        QuestGiver = LadyGrynoch
                    };
                    quest.OfferQuest(player);
				}
				else
				{
					if (quest.Step == 1 && quest.FoundBreach.IsAchieved)
					{
						quest.ChooseRewards(player);
					}
				}
			}
		}

		/// <summary>
		/// Callback for player accept/decline action.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected static void SubscribeQuest(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs qargs = args as QuestEventArgs;
			if (qargs == null)
				return;

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(ToReachTheBreach)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		/// <summary>
		/// This method checks if a player qualifies for this quest
		/// </summary>
		/// <returns>true if qualified, false if not</returns>
		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof(ToReachTheBreach)) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already

			if (player.Level < minimumLevel || player.Level > maximumLevel)
				return false;

			return true;
		}

		private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
		{
			ToReachTheBreach quest = player.IsDoingQuest(typeof(ToReachTheBreach)) as ToReachTheBreach;

			if (quest == null)
				return;

			if (response == 0x00)
			{
				SendSystemMessage(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.ToReachTheBreach.CheckPlayerAbortQuest.Text1"));
			}
			else
			{
				SendSystemMessage(player, LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.ToReachTheBreach.CheckPlayerAbortQuest.Text2", questTitle));
				quest.AbortQuest();
			}
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			// We recheck the qualification, because we don't talk to players
			// who are not doing the quest.

			if (LadyGrynoch.CanGiveQuest(typeof(ToReachTheBreach), player) <= 0)
				return;

			if (player.IsDoingQuest(typeof(ToReachTheBreach)) != null)
				return;

			if (response == 0x00)
			{
				// Player declined, don't do anything.
			}
			else
			{
				// Player accepted, let's try to give him the quest.

				if (!LadyGrynoch.GiveQuest(typeof(ToReachTheBreach), player, 1))
					return;

			}
		}

		/// <summary>
		/// The quest title.
		/// </summary>
		public override string Name
		{
			get { return questTitle; }
		}

		/// <summary>
		/// The text for individual quest steps as shown in the journal.
		/// </summary>

		public override string Description
		{
			get
			{
				switch (Step)
				{

					case 1:
						return Summary;
					default:
						return "No Queststep Description available.";
				}
			}
		}

		/// <summary>
		/// The fully-fledged story to the quest.
		/// </summary>
		public override string Story
		{
			get
			{
				String desc = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.ToReachTheBreach.Story");
				return desc;
			}
		}

		/// <summary>
		/// A summary of the quest's story.
		/// </summary>
		public override string Summary
		{
			get
			{
				return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.ToReachTheBreach.Summary");
			}
		}

		/// <summary>
		/// Text showing upon finishing the quest.
		/// </summary>
		public override String Conclusion
		{
			get
			{
				String text = String.Format(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Alb.ToReachTheBreach.Conclusion.Text1", QuestPlayer.CharacterClass.Name));
				return text;
			}
		}

		/// <summary>
		/// The level of the quest as it shows in the journal.
		/// </summary>
		public override int Level
		{
			get
			{
				return 1;
			}
		}
		protected static void PlayerEnterDemonBreachArea(DOLEvent e, object sender, EventArgs args)
		{
			AreaEventArgs aargs = args as AreaEventArgs;
			GamePlayer player = aargs.GameObject as GamePlayer;

			if (player == null)
				return;
			if (player.IsDoingQuest(typeof(ToReachTheBreach)) == null)
				return;

			ToReachTheBreach quest = player.IsDoingQuest(typeof(ToReachTheBreach)) as ToReachTheBreach;

			if (quest.Step == 1 && quest.FoundBreach.IsAchieved == false)
			{
				quest.FoundBreach.Advance();
				return;
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}

		public override void FinishQuest()
		{
			base.FinishQuest();
		}
	}
}
