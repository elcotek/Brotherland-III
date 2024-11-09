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
* Author: Elkotek
* Date: 14.01.16	
*
* Notes: source: http://camelot.allakhazam.com/quests.html?cquest=325
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

namespace DOL.GS.Quests.Midgard
{

    /* The first thing we do, is to declare the class we create
    * as Quest. To do this, we derive from the abstract class
    * BaseQuest	  	 
    */
    //sveabonehiltsword
    public class CallingtheValeCouncil : BaseQuest
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

        protected const string questTitle = "Calling the Vale Council";

        protected const int minimumLevel = 3;
        protected const int maximumLevel = 6;

        //Gnup
        private static GameNPC Gnup = null; //Start NPC

        private static GameNPC MessengerGrendlmeir = null; //Messenger Grendlmeir

        private static ItemTemplate GnupsMessage = null; //Gnups Message
                
        

        // Custom Initialization Code Begin

        // Custom Initialization Code End

        /* 
        * Constructor
        */
        public CallingtheValeCouncil()
            : base()
        {
        }

        public CallingtheValeCouncil(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public CallingtheValeCouncil(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
        }

        public CallingtheValeCouncil(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest)
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

            npcs = WorldMgr.GetNPCsByName("Gnup", (eRealm)2);
            if (npcs.Length == 0)
            {
                if (!WorldMgr.GetRegion(100).IsDisabled)
                {
                    Gnup = new DOL.GS.GameNPC
                    {
                        Model = 188,
                        Name = "Gnup"
                    };
                    if (log.IsWarnEnabled)
                        log.Warn("Could not find " + Gnup.Name + ", creating ...");
                    Gnup.GuildName = "Part of " + questTitle + " Quest";
                    Gnup.Realm = eRealm.Midgard;
                    Gnup.CurrentRegionID = 100;
                    Gnup.Size = 54;
                    Gnup.Level = 20;
                    Gnup.MaxSpeedBase = 191;
                    Gnup.Faction = FactionMgr.GetFactionByID(0);
                    Gnup.X = 804885;
                    Gnup.Y = 727895;
                    Gnup.Z = 4714;
                    Gnup.Heading = 2155;
                    Gnup.RespawnInterval = -1;
                    Gnup.BodyType = 0;


                    StandardMobBrain brain = new StandardMobBrain
                    {
                        AggroLevel = 0,
                        AggroRange = 500
                    };
                    Gnup.SetOwnBrain(brain);

                    //You don't have to store the created mob in the db if you don't want,
                    //it will be recreated each time it is not found, just comment the following
                    //line if you rather not modify your database
                    if (SAVE_INTO_DATABASE)
                        Gnup.SaveIntoDatabase();

                    Gnup.AddToWorld();

                }
            }
            else
            {
                Gnup = npcs[0];
            }

            npcs = WorldMgr.GetNPCsByName("Messenger Grendlmeir", (eRealm)2);
            if (npcs.Length == 0)
            {
                if (!WorldMgr.GetRegion(100).IsDisabled)
                {
                    MessengerGrendlmeir = new DOL.GS.GameNPC
                    {
                        Model = 234,
                        Name = "Messenger Grendlmeir"
                    };
                    if (log.IsWarnEnabled)
                        log.Warn("Could not find " + Gnup.Name + ", creating ...");
                    MessengerGrendlmeir.GuildName = "Part of " + questTitle + " Quest";
                    MessengerGrendlmeir.Realm = eRealm.Midgard;
                    MessengerGrendlmeir.CurrentRegionID = 100;
                    MessengerGrendlmeir.Size = 48;
                    MessengerGrendlmeir.Level = 31;
                    MessengerGrendlmeir.MaxSpeedBase = 191;
                    MessengerGrendlmeir.Faction = FactionMgr.GetFactionByID(0);
                    MessengerGrendlmeir.X = 795679;
                    MessengerGrendlmeir.Y = 708263;
                    MessengerGrendlmeir.Z = 4759;
                    MessengerGrendlmeir.Heading = 3381;
                    MessengerGrendlmeir.RespawnInterval = -1;
                    MessengerGrendlmeir.BodyType = 0;


                    StandardMobBrain brain = new StandardMobBrain
                    {
                        AggroLevel = 0,
                        AggroRange = 500
                    };
                    MessengerGrendlmeir.SetOwnBrain(brain);

                    //You don't have to store the created mob in the db if you don't want,
                    //it will be recreated each time it is not found, just comment the following
                    //line if you rather not modify your database
                    if (SAVE_INTO_DATABASE)
                        Gnup.SaveIntoDatabase();

                    MessengerGrendlmeir.AddToWorld();

                }
            }
            else
            {
                MessengerGrendlmeir = npcs[0];
            }


            #endregion

            #region defineItems


            GnupsMessage = GameServer.Database.FindObjectByKey<ItemTemplate>("Gnups_Message");
            if (GnupsMessage == null)
            {
                GnupsMessage = new ItemTemplate
                {
                    Name = "Gnup's Message"
                };
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + GnupsMessage.Name + ", creating it ...");
                GnupsMessage.Level = 0;
                GnupsMessage.Weight = 1;
                GnupsMessage.Model = 498;
                GnupsMessage.Object_Type = 0;
                GnupsMessage.Item_Type = 40;
                GnupsMessage.Id_nb = "Gnups_Message";
                GnupsMessage.Hand = 0;
                GnupsMessage.Price = 0;
                GnupsMessage.IsPickable = true;
                GnupsMessage.IsDropable = true;
                GnupsMessage.IsTradable = false;
                GnupsMessage.CanDropAsLoot = false;
                GnupsMessage.Color = 0;
                GnupsMessage.Bonus = 0; // default bonus				
                GnupsMessage.Bonus1 = 0;
                GnupsMessage.Bonus1Type = (int)0;
                GnupsMessage.Bonus2 = 0;
                GnupsMessage.Bonus2Type = (int)0;
                GnupsMessage.Bonus3 = 0;
                GnupsMessage.Bonus3Type = (int)0;
                GnupsMessage.Bonus4 = 0;
                GnupsMessage.Bonus4Type = (int)0;
                GnupsMessage.Bonus5 = 0;
                GnupsMessage.Bonus5Type = (int)0;
                GnupsMessage.Bonus6 = 0;
                GnupsMessage.Bonus6Type = (int)0;
                GnupsMessage.Bonus7 = 0;
                GnupsMessage.Bonus7Type = (int)0;
                GnupsMessage.Bonus8 = 0;
                GnupsMessage.Bonus8Type = (int)0;
                GnupsMessage.Bonus9 = 0;
                GnupsMessage.Bonus9Type = (int)0;
                GnupsMessage.Bonus10 = 0;
                GnupsMessage.Bonus10Type = (int)0;
                GnupsMessage.ExtraBonus = 0;
                GnupsMessage.ExtraBonusType = (int)0;
                GnupsMessage.Effect = 0;
                GnupsMessage.Emblem = 0;
                GnupsMessage.Charges = 0;
                GnupsMessage.MaxCharges = 0;
                GnupsMessage.SpellID = 0;
                GnupsMessage.ProcSpellID = 0;
                GnupsMessage.Type_Damage = 0;
                GnupsMessage.Realm = 0;
                GnupsMessage.MaxCount = 1;
                GnupsMessage.PackSize = 1;
                GnupsMessage.Extension = 0;
                GnupsMessage.Quality = 0;
                GnupsMessage.Condition = 50000;
                GnupsMessage.MaxCondition = 50000;
                GnupsMessage.Durability = 50000;
                GnupsMessage.MaxDurability = 50000;
                GnupsMessage.PoisonCharges = 0;
                GnupsMessage.PoisonMaxCharges = 0;
                GnupsMessage.PoisonSpellID = 0;
                GnupsMessage.ProcSpellID1 = 0;
                GnupsMessage.SpellID1 = 0;
                GnupsMessage.MaxCharges1 = 0;
                GnupsMessage.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(GnupsMessage);
            }
          


            #endregion

            #region defineAreas

            #endregion

            #region defineQuestParts

            //Quest start
            QuestBuilder builder = QuestMgr.getBuilder(typeof(CallingtheValeCouncil));
            QuestBehaviour a;
            a = builder.CreateBehaviour(Gnup, -1);
            a.AddTrigger(eTriggerType.Interact, null, Gnup);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DOL.GS.Quests.Midgard.CallingtheValeCouncil), Gnup);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Midgard.CallingtheValeCouncil), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "They're saying the Vendo really have taken the western fort. Who'd have thought they'd be capable of it? What troubles me is how this sudden organization came about. They've always been a fractious lot, and that's worked to our advantage for some [time].", Gnup);
            AddBehaviour(a);


            //some text start
            a = builder.CreateBehaviour(Gnup, -1);
            a.AddTrigger(eTriggerType.Whisper, "time", Gnup);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DOL.GS.Quests.Midgard.CallingtheValeCouncil), Gnup);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Midgard.CallingtheValeCouncil), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "I almost wonder... no, it couldn't be. Could it? Forgive me. It's just that a recent visitor from Haggerfel passing through - one who told me the tale about the Vendo taking over the western fort - he mentioned that a sister living in Fort Veldon claims that members of their town guard have recently noticed a sudden increase in Vendo traffic into nearby Muspelheim, land of the Fire Giants. It seems odd, combined with their recent activities here in [Mularn]. ", Gnup);
            AddBehaviour(a);

            //next some text
            a = builder.CreateBehaviour(Gnup, -1);
            a.AddTrigger(eTriggerType.Whisper, "Mularn", Gnup);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DOL.GS.Quests.Midgard.CallingtheValeCouncil), Gnup);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Midgard.CallingtheValeCouncil), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "This warrents further discussion, involving members of Haggerfel and Fort Veldon's village councils. Could you help set up a meeting by [delivering a message] to Messenger Grendlmeir of Haggerfel, while I speak with Mularn's council? He can be found between our two villiages.", Gnup);
            AddBehaviour(a);

                      
            //Quest accept
            a = builder.CreateBehaviour(Gnup, -1);
            a.AddTrigger(eTriggerType.Whisper, "delivering a message", Gnup);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DOL.GS.Quests.Midgard.CallingtheValeCouncil), Gnup);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Midgard.CallingtheValeCouncil), null, (eComparator)5);
            a.AddAction(eActionType.OfferQuest, typeof(DOL.GS.Quests.Midgard.CallingtheValeCouncil), "You Deliver Gnup's message to Messenger Grendlmeir?");
            AddBehaviour(a);


            //Quest about?
            a = builder.CreateBehaviour(Gnup, -1);
            a.AddTrigger(eTriggerType.DeclineQuest, null, typeof(DOL.GS.Quests.Midgard.CallingtheValeCouncil));
            a.AddAction(eActionType.Talk, "No problem. See you", Gnup);
            AddBehaviour(a);

            //Step 1
            a = builder.CreateBehaviour(Gnup, -1);
            
            a.AddTrigger(eTriggerType.AcceptQuest, null, typeof(DOL.GS.Quests.Midgard.CallingtheValeCouncil));
            a.AddAction(eActionType.Talk, "Deliver Gnup's message to Messenger Grendlmeir, who traverses the road between Mularn and Haggerfel.", Gnup);
            a.AddRequirement(eRequirementType.InventoryItem, GnupsMessage, 1, (eComparator)1);
            a.AddAction(eActionType.GiveItem, GnupsMessage, null);
            a.AddAction(eActionType.GiveQuest, typeof(DOL.GS.Quests.Midgard.CallingtheValeCouncil), Gnup);
           
            AddBehaviour(a);

            

            //we give the Message to Gremlmeir
            //Step 1 now we give the Message to Gremlmeir and at the end, the Quest reward is the 'Choker of the Bear'
            a = builder.CreateBehaviour(MessengerGrendlmeir, -1);
            a.AddTrigger(eTriggerType.GiveItem, MessengerGrendlmeir, GnupsMessage);
            a.AddRequirement(eRequirementType.QuestStep, typeof(DOL.GS.Quests.Midgard.CallingtheValeCouncil), 1, (eComparator)3);
            a.AddAction(eActionType.Talk, "Ouu! Oh thank you friend! don't walk away. there is a Reward for you.", MessengerGrendlmeir);
            a.AddAction(eActionType.TakeItem, GnupsMessage, null);
            a.AddAction(eActionType.GiveXP, 270, null);
            a.AddAction(eActionType.GiveGold, 150, null);
            a.AddAction(eActionType.FinishQuest, typeof(DOL.GS.Quests.Midgard.CallingtheValeCouncil), null);
            AddBehaviour(a);


            #endregion

            // Custom Scriptloaded Code Begin

            // Custom Scriptloaded Code End
            if (Gnup != null)
            {
                Gnup.AddQuestToGive(typeof(CallingtheValeCouncil));
            }
            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initialized");
        }

        [ScriptUnloadedEvent]
        public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
        {

            // Custom Scriptunloaded Code Begin

            // Custom Scriptunloaded Code End



            /* If Bolli has not been initialized, then we don't have to remove any
             * hooks from him ;-)
             */
            if (Gnup == null)
                return;
            /* Now we remove the possibility to give this quest to players */
            Gnup.RemoveQuestToGive(typeof(CallingtheValeCouncil));
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
                        return "[Step #1] Deliver Gnup's message to Messenger Grendlmeir, who traverses the road between Mularn and Haggerfel. He often takes a break in his travels near the point in the road mid-way between the villages.";
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
            if (player.IsDoingQuest(typeof(CallingtheValeCouncil)) != null)
                return true;

            // Custom Code Begin

            // Custom  Code End


            if (player.Level > maximumLevel || player.Level < minimumLevel)
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
