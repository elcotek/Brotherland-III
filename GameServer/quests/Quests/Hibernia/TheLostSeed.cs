





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
* Date: 17.01.16	
*
* Notes: source: http://camelot.allakhazam.com/quests.html?cquest=734
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

namespace DOL.GS.Quests.Hibernia
{

    /* The first thing we do, is to declare the class we create
    * as Quest. To do this, we derive from the abstract class
    * BaseQuest	  	 
    */
    //sveabonehiltsword
    public class TheLostSeed : BaseQuest
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

        protected const string questTitle = "The Lost Seed";

        protected const int minimumLevel = 48;
        protected const int maximumLevel = 50;

        //Terod
        private static GameNPC Terod = null; //Start NPC

        private static GameNPC Kredril = null; //NPC Kredil

        private static GameNPC Jandros = null; //NPC Jandros

        private static GameNPC Syalia = null; //NPC Syalia


        private static GameNPC deadtree = null; //NPC dead tree



        private static ItemTemplate ticketToAalidFeie = null;
        //private static ItemTemplate ticketToCaliteGarren = null;

        private static ItemTemplate deadtreebranch = null; // dead_tree_branch
        private static ItemTemplate NoteforTerod = null; // Note_for_Terod
        private static ItemTemplate NoteforKredril = null; // Note_for_Kredril
        private static ItemTemplate NoteforSyalia = null; // Note_for_Syalia

        private static ItemTemplate PaidreanNecklace = null;
        private static ItemTemplate AigleanEarring = null;
        private static ItemTemplate FaineyArgidRing = null;


        /// <summary>
        /// Retrieves how much time player can do the quest
        /// </summary>
        public override int MaxQuestCount
        {
            get { return 1; }
        }
        // Custom Initialization Code Begin

        // Custom Initialization Code End

        /* 
        * Constructor
        */
        public TheLostSeed()
            : base()
        {
        }

        public TheLostSeed(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public TheLostSeed(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
        }

        public TheLostSeed(GamePlayer questingPlayer, DBQuest dbQuest)
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

            npcs = WorldMgr.GetNPCsByName("Terod", (eRealm)3);
            if (npcs.Length == 0)
            {
                if (!WorldMgr.GetRegion(181).IsDisabled)
                {
                    Terod = new DOL.GS.GameNPC
                    {
                        Model = 334,
                        Name = "Terod"
                    };
                    if (log.IsWarnEnabled)
                        log.Warn("Could not find " + Terod.Name + ", creating ...");
                    Terod.GuildName = "Part of " + questTitle + " Quest";
                    Terod.Realm = eRealm.Hibernia;
                    Terod.CurrentRegionID = 181;
                    Terod.Size = 53;
                    Terod.Level = 52;
                    Terod.MaxSpeedBase = 191;
                    Terod.Faction = FactionMgr.GetFactionByID(0);
                    Terod.X = 382876;
                    Terod.Y = 421059;
                    Terod.Z = 5613;
                    Terod.Heading = 1171;
                    Terod.RespawnInterval = -1;
                    Terod.BodyType = 0;


                    StandardMobBrain brain = new StandardMobBrain
                    {
                        AggroLevel = 0,
                        AggroRange = 500
                    };
                    Terod.SetOwnBrain(brain);

                    //You don't have to store the created mob in the db if you don't want,
                    //it will be recreated each time it is not found, just comment the following
                    //line if you rather not modify your database
                    if (SAVE_INTO_DATABASE)
                        Terod.SaveIntoDatabase();

                    Terod.AddToWorld();

                }
            }
            else
            {
                Terod = npcs[0];
            }

            npcs = WorldMgr.GetNPCsByName("Kredril", (eRealm)3);
            if (npcs.Length == 0)
            {
                if (!WorldMgr.GetRegion(181).IsDisabled)
                {
                    Kredril = new DOL.GS.GameNPC
                    {
                        Model = 334,
                        Name = "Kredril"
                    };
                    if (log.IsWarnEnabled)
                        log.Warn("Could not find " + Terod.Name + ", creating ...");
                    Kredril.GuildName = "Part of " + questTitle + " Quest";
                    Kredril.Realm = eRealm.Hibernia;
                    Kredril.CurrentRegionID = 181;
                    Kredril.Size = 53;
                    Kredril.Level = 52;
                    Kredril.MaxSpeedBase = 191;
                    Kredril.Faction = FactionMgr.GetFactionByID(0);
                    Kredril.X = 380510;
                    Kredril.Y = 421414;
                    Kredril.Z = 5528;
                    Kredril.Heading = 1501;
                    Kredril.RespawnInterval = -1;
                    Kredril.BodyType = 0;


                    StandardMobBrain brain = new StandardMobBrain
                    {
                        AggroLevel = 0,
                        AggroRange = 500
                    };
                    Kredril.SetOwnBrain(brain);

                    //You don't have to store the created mob in the db if you don't want,
                    //it will be recreated each time it is not found, just comment the following
                    //line if you rather not modify your database
                    if (SAVE_INTO_DATABASE)
                        Kredril.SaveIntoDatabase();

                    Kredril.AddToWorld();

                }
            }
            else
            {
                Kredril = npcs[0];
            }

            npcs = WorldMgr.GetNPCsByName("Jandros", (eRealm)3);
            if (npcs.Length == 0)
            {
                if (!WorldMgr.GetRegion(181).IsDisabled)
                {
                    Jandros = new DOL.GS.GameNPC
                    {
                        Model = 734,
                        Name = "Jandros"
                    };
                    if (log.IsWarnEnabled)
                        log.Warn("Could not find " + Terod.Name + ", creating ...");
                    Jandros.GuildName = "Part of " + questTitle + " Quest";
                    Jandros.Realm = eRealm.Hibernia;
                    Jandros.CurrentRegionID = 181;
                    Jandros.Size = 53;
                    Jandros.Level = 54;
                    Jandros.MaxSpeedBase = 191;
                    Jandros.Faction = FactionMgr.GetFactionByID(0);
                    Jandros.X = 380897;
                    Jandros.Y = 349963;
                    Jandros.Z = 3571;
                    Jandros.Heading = 1399;
                    Jandros.RespawnInterval = -1;
                    Jandros.BodyType = 0;


                    StandardMobBrain brain = new StandardMobBrain
                    {
                        AggroLevel = 0,
                        AggroRange = 500
                    };
                    Jandros.SetOwnBrain(brain);

                    //You don't have to store the created mob in the db if you don't want,
                    //it will be recreated each time it is not found, just comment the following
                    //line if you rather not modify your database
                    if (SAVE_INTO_DATABASE)
                        Jandros.SaveIntoDatabase();

                    Jandros.AddToWorld();

                }
            }
            else
            {
                Jandros = npcs[0];
            }

            npcs = WorldMgr.GetNPCsByName("Syalia", (eRealm)3);
            if (npcs.Length == 0)
            {
                if (!WorldMgr.GetRegion(181).IsDisabled)
                {
                    Syalia = new DOL.GS.GameNPC
                    {
                        Model = 741,
                        Name = "Syalia"
                    };
                    if (log.IsWarnEnabled)
                        log.Warn("Could not find " + Terod.Name + ", creating ...");
                    Syalia.GuildName = "Part of " + questTitle + " Quest";
                    Syalia.Realm = eRealm.Hibernia;
                    Syalia.CurrentRegionID = 181;
                    Syalia.Size = 48;
                    Syalia.Level = 50;
                    Syalia.MaxSpeedBase = 191;
                    Syalia.Faction = FactionMgr.GetFactionByID(0);
                    Syalia.X = 377555;
                    Syalia.Y = 420729;
                    Syalia.Z = 5965;
                    Syalia.Heading = 1968;
                    Syalia.RespawnInterval = -1;
                    Syalia.BodyType = 0;


                    StandardMobBrain brain = new StandardMobBrain
                    {
                        AggroLevel = 0,
                        AggroRange = 500
                    };
                    Syalia.SetOwnBrain(brain);

                    //You don't have to store the created mob in the db if you don't want,
                    //it will be recreated each time it is not found, just comment the following
                    //line if you rather not modify your database
                    if (SAVE_INTO_DATABASE)
                        Syalia.SaveIntoDatabase();

                    Syalia.AddToWorld();

                }
            }
            else
            {
                Syalia = npcs[0];
            }


            npcs = WorldMgr.GetNPCsByName("dead tree", (eRealm)0);
            if (npcs.Length == 0)
            {
                if (!WorldMgr.GetRegion(181).IsDisabled)
                {
                    deadtree = new DOL.GS.GameNPC
                    {
                        Model = 570,
                        Name = "dead tree"
                    };
                    if (log.IsWarnEnabled)
                        log.Warn("Could not find " + Terod.Name + ", creating ...");
                    deadtree.GuildName = "Part of " + questTitle + " Quest";
                    deadtree.Realm = eRealm.None;
                    deadtree.CurrentRegionID = 181;
                    deadtree.Size = 80;
                    deadtree.Level = 65;
                    deadtree.MaxSpeedBase = 191;
                    deadtree.Faction = FactionMgr.GetFactionByID(0);
                    deadtree.X = 304942;
                    deadtree.Y = 324528;
                    deadtree.Z = 4297;
                    deadtree.Heading = 1939;
                    deadtree.RespawnInterval = 600000;
                    deadtree.BodyType = 0;


                    StandardMobBrain brain = new StandardMobBrain
                    {
                        AggroLevel = 0,
                        AggroRange = 500
                    };
                    deadtree.SetOwnBrain(brain);

                    //You don't have to store the created mob in the db if you don't want,
                    //it will be recreated each time it is not found, just comment the following
                    //line if you rather not modify your database
                    if (SAVE_INTO_DATABASE)
                        Terod.SaveIntoDatabase();

                    deadtree.AddToWorld();

                }
            }
            else
            {
                deadtree = npcs[0];
            }

            #endregion

            #region defineItems





            ticketToAalidFeie = GameServer.Database.FindObjectByKey<ItemTemplate>("HS_Droighaid_GroveofAalidFeie");
            if (ticketToAalidFeie == null)
                ticketToAalidFeie = CreateTicketTo("Ticket to Grove of Aalid Feie", "HS_Droighaid_GroveofAalidFeie");

           
            deadtreebranch = GameServer.Database.FindObjectByKey<ItemTemplate>("dead_tree_branch");
            if (deadtreebranch == null)
                deadtreebranch = CreateTicketTo("dead tree branch", "dead_tree_branch");

            NoteforTerod = GameServer.Database.FindObjectByKey<ItemTemplate>("Note_for_Terod");
            if (NoteforTerod == null)
                NoteforTerod = CreateTicketTo("Note for Terod", "Note_for_Terod");

            NoteforKredril = GameServer.Database.FindObjectByKey<ItemTemplate>("Note_for_Kredril");
            if (NoteforKredril == null)
                NoteforKredril = CreateTicketTo("Note for Kredril", "Note_for_Kredril");

            NoteforSyalia = GameServer.Database.FindObjectByKey<ItemTemplate>("Note_for_Syalia");
            if (NoteforSyalia == null)
                NoteforSyalia = CreateTicketTo("Note for Syalia", "Note_for_Syalia");


            PaidreanNecklace = GameServer.Database.FindObjectByKey<ItemTemplate>("Paidrean_Necklace");
            if (PaidreanNecklace == null)
                PaidreanNecklace = CreateTicketTo("Paidrean Necklace", "Paidrean_Necklace");

            AigleanEarring = GameServer.Database.FindObjectByKey<ItemTemplate>("Aiglean_Earring");
            if (AigleanEarring == null)
                AigleanEarring = CreateTicketTo("Aiglean Earring", "Aiglean_Earring");

            FaineyArgidRing = GameServer.Database.FindObjectByKey<ItemTemplate>("Fainey_Argid_Ring");
            if (FaineyArgidRing == null)
                FaineyArgidRing = CreateTicketTo("Fainey Argid Ring", "Fainey_Argid_Ring");





            #endregion

            #region defineAreas

            #endregion

            #region defineQuestParts

            //Quest start
            QuestBuilder builder = QuestMgr.getBuilder(typeof(TheLostSeed));
            QuestBehaviour a;
            a = builder.CreateBehaviour(Terod, -1);
            a.AddTrigger(eTriggerType.Interact, null, Terod);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DOL.GS.Quests.Hibernia.TheLostSeed), Terod);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Hibernia.TheLostSeed), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "Greetings to you. you are here to help the [Clan]? ", Terod);
            AddBehaviour(a);


            //some text start
            a = builder.CreateBehaviour(Terod, -1);
            a.AddTrigger(eTriggerType.Whisper, "Clan", Terod);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DOL.GS.Quests.Hibernia.TheLostSeed), Terod);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Hibernia.TheLostSeed), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "Ah, you are [speak] with Kredril and help the Clan ? ", Terod);
            AddBehaviour(a);

            //Quest accept
            a = builder.CreateBehaviour(Terod, -1);
            a.AddTrigger(eTriggerType.Whisper, "speak", Terod);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DOL.GS.Quests.Hibernia.TheLostSeed), Terod);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Hibernia.TheLostSeed), null, (eComparator)5);
            a.AddAction(eActionType.OfferQuest, typeof(DOL.GS.Quests.Hibernia.TheLostSeed), "You find Kredril on the same side where stablemaster Hyath stay.");
            AddBehaviour(a);

            //Quest about?
            a = builder.CreateBehaviour(Terod, -1);
            a.AddTrigger(eTriggerType.DeclineQuest, null, typeof(DOL.GS.Quests.Hibernia.TheLostSeed));
            a.AddAction(eActionType.Talk, "No problem. See you", Terod);
            AddBehaviour(a);

            //Step 1
            a = builder.CreateBehaviour(Terod, -1);
            a.AddTrigger(eTriggerType.AcceptQuest, null, typeof(DOL.GS.Quests.Hibernia.TheLostSeed));
            a.AddAction(eActionType.Talk, "Now go and speak with Kredril.", Terod);
            a.AddAction(eActionType.GiveQuest, typeof(DOL.GS.Quests.Hibernia.TheLostSeed), Terod);
            AddBehaviour(a);

            //Step 2 take the ticket from kredril
            a = builder.CreateBehaviour(Kredril, -1);
            a.AddTrigger(eTriggerType.Interact, null, Kredril);
            a.AddRequirement(eRequirementType.QuestStep, typeof(DOL.GS.Quests.Hibernia.TheLostSeed), 1, (eComparator)3);
            a.AddAction(eActionType.Talk, "Ah., you help the clan!?, take this ticket and if you in Aalid Feie speak with Jandros in the Tavern if you there.", Kredril);
            a.AddAction(eActionType.GiveItem, ticketToAalidFeie, null);
            a.AddAction(eActionType.IncQuestStep, typeof(DOL.GS.Quests.Hibernia.TheLostSeed), null);
            AddBehaviour(a);



            //Step 3 take the dead tree
            a = builder.CreateBehaviour(Jandros, -1);
            a.AddTrigger(eTriggerType.Interact, null, Jandros);
            a.AddRequirement(eRequirementType.QuestStep, typeof(DOL.GS.Quests.Hibernia.TheLostSeed), 2, (eComparator)3);
            a.AddAction(eActionType.Talk, "The trees can be a sulotion, take a branch from a tree, go out and kill a dead tree in Cothrom Gorge.", Jandros);
            a.AddAction(eActionType.Talk, "If you have a branche of him come back to me.", Jandros);
            a.AddAction(eActionType.IncQuestStep, typeof(DOL.GS.Quests.Hibernia.TheLostSeed), null);
            AddBehaviour(a);


            //Step 4 kill the deadtree"
            a = builder.CreateBehaviour(Jandros, -1);
            a.AddTrigger(eTriggerType.EnemyKilled, "dead tree", null);
            a.AddRequirement(eRequirementType.QuestStep, typeof(DOL.GS.Quests.Hibernia.TheLostSeed), 3, (eComparator)3);
            a.AddAction(eActionType.GiveItem, deadtreebranch, null);
            a.AddAction(eActionType.IncQuestStep, typeof(DOL.GS.Quests.Hibernia.TheLostSeed), null);
            AddBehaviour(a);


            //Step 5 return to Jandros"
            a = builder.CreateBehaviour(Jandros, -1);
            a.AddTrigger(eTriggerType.GiveItem, Jandros, deadtreebranch);
            a.AddRequirement(eRequirementType.QuestStep, typeof(DOL.GS.Quests.Hibernia.TheLostSeed), 4, (eComparator)3);
            a.AddAction(eActionType.TakeItem, deadtreebranch, null);
            a.AddAction(eActionType.GiveItem, NoteforTerod, null);
            a.AddAction(eActionType.GiveItem, NoteforKredril, null);
            a.AddAction(eActionType.GiveItem, NoteforSyalia, null);
            a.AddAction(eActionType.Talk, "Oh thank you! Give one of this Notes first to Terod, who scowls at you. Second note to Kredril. Third note to Syalia (on top of the tower in the east half of Droighaid).", Jandros);
            a.AddAction(eActionType.GiveXP, 5000000, null);
            a.AddAction(eActionType.GiveGold, 202500, null);
            a.AddAction(eActionType.IncQuestStep, typeof(TheLostSeed), null);
            AddBehaviour(a);



            //Step 6 speak with Terod
            a = builder.CreateBehaviour(Terod, -1);
            ///a.AddTrigger(eTriggerType.EnemyKilled, "huldu lurker", null);
            a.AddTrigger(eTriggerType.GiveItem, Terod, NoteforTerod);
            a.AddRequirement(eRequirementType.QuestStep, typeof(DOL.GS.Quests.Hibernia.TheLostSeed), 5, (eComparator)3);
            a.AddAction(eActionType.TakeItem, NoteforTerod, null);
            a.AddAction(eActionType.Talk, "Good work, take this Reward for all you have do for the Clan.", Terod);
            a.AddAction(eActionType.Talk, "Speak now with Kredril here in callite garren.", Terod);
            a.AddAction(eActionType.GiveItem, PaidreanNecklace, null);
            a.AddAction(eActionType.IncQuestStep, typeof(DOL.GS.Quests.Hibernia.TheLostSeed), null);
            AddBehaviour(a);
            

            //Step 7 give NoteforKredril to Kredil
            a = builder.CreateBehaviour(Terod, -1);
            a.AddTrigger(eTriggerType.GiveItem, Kredril, NoteforKredril);
            a.AddRequirement(eRequirementType.QuestStep, typeof(DOL.GS.Quests.Hibernia.TheLostSeed), 6, (eComparator)3);
            a.AddAction(eActionType.TakeItem, NoteforKredril, null);
            a.AddAction(eActionType.Talk, "Good work, take this Reward for all you have do for the Clan.", Kredril);
            a.AddAction(eActionType.Talk, "As next speak with Sylia she is on the Tower.", Terod);
            a.AddAction(eActionType.GiveItem, AigleanEarring, null);
            a.AddAction(eActionType.IncQuestStep, typeof(DOL.GS.Quests.Hibernia.TheLostSeed), null);
            AddBehaviour(a);


            //Step 8 give NoteforKredril to Syalia END
            a = builder.CreateBehaviour(Terod, -1);
            a.AddTrigger(eTriggerType.GiveItem, Syalia, NoteforSyalia);
            a.AddRequirement(eRequirementType.QuestStep, typeof(DOL.GS.Quests.Hibernia.TheLostSeed), 7, (eComparator)3);
            a.AddAction(eActionType.TakeItem, NoteforSyalia, null);
            a.AddAction(eActionType.Talk, "Wonderfull, take this for all you have do for us", Syalia);
            a.AddAction(eActionType.GiveItem, FaineyArgidRing, null);
            a.AddAction(eActionType.GiveXP, 170000000, null);
            a.AddAction(eActionType.GiveGold, 300000, null);
            a.AddAction(eActionType.FinishQuest, typeof(DOL.GS.Quests.Hibernia.TheLostSeed), null);
            AddBehaviour(a);

          
            #endregion

            // Custom Scriptloaded Code Begin

            // Custom Scriptloaded Code End
            if (Terod != null)
            {
                Terod.AddQuestToGive(typeof(TheLostSeed));
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
            if (Terod == null)
                return;
            /* Now we remove the possibility to give this quest to players */
            Terod.RemoveQuestToGive(typeof(TheLostSeed));
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
                        return "[Step #1] speak with Kredril, you find Kredril on the same side where stablemaster Hyath stay.";

                    case 2:
                        return "[Step #2] take the Ticket and ride to Aalid Feie if you there speak with Jandros in the Tavern.";

                    case 3:
                        return "[Step #3]  Follow the north road into Cothrom until the first drop around the mirages, and turn west, following the zone wall until it turns north, at which point there is a dree named dead tree, take a branch of him";

                    case 4:
                        return "[Step #4]  return to Jandros and give him the Branch.";

                    case 5:
                        return "[Step #5]  Give the first Note to Terod in Callite Garren.";

                    case 6:
                        return "[Step #6]   Give the Second note to Kredril.";
                        
                    case 7:
                        return "[Step #7]   Give the Second note to Kredril in Callite Garren.";

                    case 8:
                        return "[Step #8]   Give the Third note to Syalia on top of the tower in the east half of Droighaid.";
                        

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
            if (player.IsDoingQuest(typeof(TheLostSeed)) != null)
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
    


