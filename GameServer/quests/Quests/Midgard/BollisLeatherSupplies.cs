
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
    public class BollisLeatherSupplies : BaseQuest
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

        protected const string questTitle = "Bolli's Leather Supplies";

        protected const int minimumLevel = 3;
        protected const int maximumLevel = 13;

        //Bolli
        private static GameNPC Bolli = null; //Start NPC

        private static ItemTemplate Bollis_Skinning_knife = null; //first Item

        private static ItemTemplate shinbone = null; //seccond Item needed

        private static ItemTemplate Choker_of_the_Bear = null; //this item is given at the end of quest 


        // Custom Initialization Code Begin

        // Custom Initialization Code End

        /* 
        * Constructor
        */
        public BollisLeatherSupplies()
            : base()
        {
        }

        public BollisLeatherSupplies(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public BollisLeatherSupplies(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
        }

        public BollisLeatherSupplies(GamePlayer questingPlayer, DBQuest dbQuest)
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

            npcs = WorldMgr.GetNPCsByName("Bolli", (eRealm)2);
            if (npcs.Length == 0)
            {
                if (!WorldMgr.GetRegion(100).IsDisabled)
                {
                    Bolli = new DOL.GS.GameNPC
                    {
                        Model = 231,
                        Name = "Bolli"
                    };
                    if (log.IsWarnEnabled)
                        log.Warn("Could not find " + Bolli.Name + ", creating ...");
                    Bolli.GuildName = "Part of " + questTitle + " Quest";
                    Bolli.Realm = eRealm.Midgard;
                    Bolli.CurrentRegionID = 100;
                    Bolli.Size = 51;
                    Bolli.Level = 31;
                    Bolli.MaxSpeedBase = 191;
                    Bolli.Faction = FactionMgr.GetFactionByID(0);
                    Bolli.X = 805101;
                    Bolli.Y = 726511;
                    Bolli.Z = 4688;
                    Bolli.Heading = 917;
                    Bolli.RespawnInterval = -1;
                    Bolli.BodyType = 0;


                    StandardMobBrain brain = new StandardMobBrain
                    {
                        AggroLevel = 0,
                        AggroRange = 500
                    };
                    Bolli.SetOwnBrain(brain);

                    //You don't have to store the created mob in the db if you don't want,
                    //it will be recreated each time it is not found, just comment the following
                    //line if you rather not modify your database
                    if (SAVE_INTO_DATABASE)
                        Bolli.SaveIntoDatabase();

                    Bolli.AddToWorld();

                }
            }
            else
            {
                Bolli = npcs[0];
            }


            #endregion

            #region defineItems

            Bollis_Skinning_knife = GameServer.Database.FindObjectByKey<ItemTemplate>("Bollis_Skinning_knife");
            if (Bollis_Skinning_knife == null)
            {
                Bollis_Skinning_knife = new ItemTemplate
                {
                    Name = "Bolli's Skinning knife"
                };
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + Bollis_Skinning_knife.Name + ", creating it ...");
                Bollis_Skinning_knife.Level = 3;
                Bollis_Skinning_knife.Weight = 6;
                Bollis_Skinning_knife.Model = 1;
                Bollis_Skinning_knife.Object_Type = 11;
                Bollis_Skinning_knife.Item_Type = 9;
                Bollis_Skinning_knife.Id_nb = "Bollis_Skinning_knife";
                Bollis_Skinning_knife.Hand = 0;
                Bollis_Skinning_knife.Price = Money.GetMoney(0, 0, 0, 1, 50);
                Bollis_Skinning_knife.IsPickable = true;
                Bollis_Skinning_knife.IsDropable = true;
                Bollis_Skinning_knife.IsTradable = false;
                Bollis_Skinning_knife.CanDropAsLoot = true;
                Bollis_Skinning_knife.Color = 0;
                Bollis_Skinning_knife.Bonus = 0; // default bonus				
                Bollis_Skinning_knife.Bonus1 = 0;
                Bollis_Skinning_knife.Bonus1Type = (int)0;
                Bollis_Skinning_knife.Bonus2 = 0;
                Bollis_Skinning_knife.Bonus2Type = (int)0;
                Bollis_Skinning_knife.Bonus3 = 0;
                Bollis_Skinning_knife.Bonus3Type = (int)0;
                Bollis_Skinning_knife.Bonus4 = 0;
                Bollis_Skinning_knife.Bonus4Type = (int)0;
                Bollis_Skinning_knife.Bonus5 = 0;
                Bollis_Skinning_knife.Bonus5Type = (int)0;
                Bollis_Skinning_knife.Bonus6 = 0;
                Bollis_Skinning_knife.Bonus6Type = (int)0;
                Bollis_Skinning_knife.Bonus7 = 0;
                Bollis_Skinning_knife.Bonus7Type = (int)0;
                Bollis_Skinning_knife.Bonus8 = 0;
                Bollis_Skinning_knife.Bonus8Type = (int)0;
                Bollis_Skinning_knife.Bonus9 = 0;
                Bollis_Skinning_knife.Bonus9Type = (int)0;
                Bollis_Skinning_knife.Bonus10 = 0;
                Bollis_Skinning_knife.Bonus10Type = (int)0;
                Bollis_Skinning_knife.ExtraBonus = 0;
                Bollis_Skinning_knife.ExtraBonusType = (int)0;
                Bollis_Skinning_knife.Effect = 0;
                Bollis_Skinning_knife.Emblem = 0;
                Bollis_Skinning_knife.Charges = 0;
                Bollis_Skinning_knife.MaxCharges = 0;
                Bollis_Skinning_knife.SpellID = 0;
                Bollis_Skinning_knife.ProcSpellID = 0;
                Bollis_Skinning_knife.Type_Damage = 2;
                Bollis_Skinning_knife.Realm = 2;
                Bollis_Skinning_knife.MaxCount = 1;
                Bollis_Skinning_knife.PackSize = 1;
                Bollis_Skinning_knife.Extension = 0;
                Bollis_Skinning_knife.Quality = 100;
                Bollis_Skinning_knife.Condition = 50000;
                Bollis_Skinning_knife.MaxCondition = 50000;
                Bollis_Skinning_knife.Durability = 100;
                Bollis_Skinning_knife.MaxDurability = 100;
                Bollis_Skinning_knife.PoisonCharges = 0;
                Bollis_Skinning_knife.PoisonMaxCharges = 0;
                Bollis_Skinning_knife.PoisonSpellID = 0;
                Bollis_Skinning_knife.ProcSpellID1 = 0;
                Bollis_Skinning_knife.SpellID1 = 0;
                Bollis_Skinning_knife.MaxCharges1 = 0;
                Bollis_Skinning_knife.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(Bollis_Skinning_knife);
            }
            shinbone = GameServer.Database.FindObjectByKey<ItemTemplate>("shinbone");
            if (shinbone == null)
            {
                shinbone = new ItemTemplate
                {
                    Name = "Shin Bone"
                };
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + shinbone.Name + ", creating it ...");
                shinbone.Level = 3;
                shinbone.Weight = 1;
                shinbone.Model = 497;
                shinbone.Object_Type = 0;
                shinbone.Item_Type = 40;
                shinbone.Id_nb = "shinbone";
                shinbone.Hand = 0;
                shinbone.Price = 0;
                shinbone.IsPickable = true;
                shinbone.IsDropable = true;
                shinbone.IsTradable = false;
                shinbone.CanDropAsLoot = false;
                shinbone.Color = 0;
                shinbone.Bonus = 0; // default bonus				
                shinbone.Bonus1 = 0;
                shinbone.Bonus1Type = (int)0;
                shinbone.Bonus2 = 0;
                shinbone.Bonus2Type = (int)0;
                shinbone.Bonus3 = 0;
                shinbone.Bonus3Type = (int)0;
                shinbone.Bonus4 = 0;
                shinbone.Bonus4Type = (int)0;
                shinbone.Bonus5 = 0;
                shinbone.Bonus5Type = (int)0;
                shinbone.Bonus6 = 0;
                shinbone.Bonus6Type = (int)0;
                shinbone.Bonus7 = 0;
                shinbone.Bonus7Type = (int)0;
                shinbone.Bonus8 = 0;
                shinbone.Bonus8Type = (int)0;
                shinbone.Bonus9 = 0;
                shinbone.Bonus9Type = (int)0;
                shinbone.Bonus10 = 0;
                shinbone.Bonus10Type = (int)0;
                shinbone.ExtraBonus = 0;
                shinbone.ExtraBonusType = (int)0;
                shinbone.Effect = 0;
                shinbone.Emblem = 0;
                shinbone.Charges = 0;
                shinbone.MaxCharges = 0;
                shinbone.SpellID = 0;
                shinbone.ProcSpellID = 0;
                shinbone.Type_Damage = 0;
                shinbone.Realm = 0;
                shinbone.MaxCount = 1;
                shinbone.PackSize = 1;
                shinbone.Extension = 0;
                shinbone.Quality = 0;
                shinbone.Condition = 50000;
                shinbone.MaxCondition = 50000;
                shinbone.Durability = 50000;
                shinbone.MaxDurability = 50000;
                shinbone.PoisonCharges = 0;
                shinbone.PoisonMaxCharges = 0;
                shinbone.PoisonSpellID = 0;
                shinbone.ProcSpellID1 = 0;
                shinbone.SpellID1 = 0;
                shinbone.MaxCharges1 = 0;
                shinbone.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(shinbone);
            }
            Choker_of_the_Bear = GameServer.Database.FindObjectByKey<ItemTemplate>("Choker_of_the_Bear");
            if (Choker_of_the_Bear == null)
            {
                Choker_of_the_Bear = new ItemTemplate
                {
                    Name = "Choker of the Bear"
                };
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + Choker_of_the_Bear.Name + ", creating it ...");
                Choker_of_the_Bear.Level = 5;
                Choker_of_the_Bear.Weight = 6;
                Choker_of_the_Bear.Model = 101;
                Choker_of_the_Bear.Object_Type = 41;
                Choker_of_the_Bear.Item_Type = 29;
                Choker_of_the_Bear.Id_nb = "Choker_of_the_Bear";
                Choker_of_the_Bear.DPS_AF = 18;
                Choker_of_the_Bear.SPD_ABS = 30;
                Choker_of_the_Bear.Hand = 0;
                Choker_of_the_Bear.Price = 0;
                Choker_of_the_Bear.IsPickable = true;
                Choker_of_the_Bear.IsDropable = true;
                Choker_of_the_Bear.IsTradable = false;
                Choker_of_the_Bear.CanDropAsLoot = false;
                Choker_of_the_Bear.Color = 0;
                Choker_of_the_Bear.Bonus = 1; // default bonus				
                Choker_of_the_Bear.Bonus1 = 4;
                Choker_of_the_Bear.Bonus1Type = (int)1;
                Choker_of_the_Bear.Bonus2 = 7;
                Choker_of_the_Bear.Bonus2Type = (int)3;
                Choker_of_the_Bear.Bonus3 = 1;
                Choker_of_the_Bear.Bonus3Type = (int)19;
                Choker_of_the_Bear.Bonus4 = 0;
                Choker_of_the_Bear.Bonus4Type = (int)0;
                Choker_of_the_Bear.Bonus5 = 0;
                Choker_of_the_Bear.Bonus5Type = (int)0;
                Choker_of_the_Bear.Bonus6 = 0;
                Choker_of_the_Bear.Bonus6Type = (int)0;
                Choker_of_the_Bear.Bonus7 = 0;
                Choker_of_the_Bear.Bonus7Type = (int)0;
                Choker_of_the_Bear.Bonus8 = 0;
                Choker_of_the_Bear.Bonus8Type = (int)0;
                Choker_of_the_Bear.Bonus9 = 0;
                Choker_of_the_Bear.Bonus9Type = (int)0;
                Choker_of_the_Bear.Bonus10 = 0;
                Choker_of_the_Bear.Bonus10Type = (int)0;
                Choker_of_the_Bear.ExtraBonus = 0;
                Choker_of_the_Bear.ExtraBonusType = (int)0;
                Choker_of_the_Bear.Effect = 0;
                Choker_of_the_Bear.Emblem = 0;
                Choker_of_the_Bear.Charges = 0;
                Choker_of_the_Bear.MaxCharges = 0;
                Choker_of_the_Bear.SpellID = 0;
                Choker_of_the_Bear.ProcSpellID = 0;
                Choker_of_the_Bear.Type_Damage = 2;
                Choker_of_the_Bear.Realm = 0;
                Choker_of_the_Bear.MaxCount = 1;
                Choker_of_the_Bear.PackSize = 1;
                Choker_of_the_Bear.Extension = 0;
                Choker_of_the_Bear.Quality = 70;
                Choker_of_the_Bear.Condition = 100;
                Choker_of_the_Bear.MaxCondition = 100;
                Choker_of_the_Bear.Durability = 100;
                Choker_of_the_Bear.MaxDurability = 100;
                Choker_of_the_Bear.PoisonCharges = 0;
                Choker_of_the_Bear.PoisonMaxCharges = 0;
                Choker_of_the_Bear.PoisonSpellID = 0;
                Choker_of_the_Bear.ProcSpellID1 = 0;
                Choker_of_the_Bear.SpellID1 = 0;
                Choker_of_the_Bear.MaxCharges1 = 0;
                Choker_of_the_Bear.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(Choker_of_the_Bear);
            }


            #endregion

            #region defineAreas

            #endregion

            #region defineQuestParts

            //Quest start
            QuestBuilder builder = QuestMgr.getBuilder(typeof(BollisLeatherSupplies));
            QuestBehaviour a;
            a = builder.CreateBehaviour(Bolli, -1);
            a.AddTrigger(eTriggerType.Interact, null, Bolli);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), Bolli);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "Greetings to you. I'm sure I've seen you before, but my memory isn't as great as it used to be, so you'll have to forgive me. Hehe [Mularn] sure is a nice place isn't it? ", Bolli);
            AddBehaviour(a);


            //some text start
            a = builder.CreateBehaviour(Bolli, -1);
            a.AddTrigger(eTriggerType.Whisper, "Mularn", Bolli);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), Bolli);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "Ah, I've lived here nearly my entire life, well, most of it anyhow. Since my accident, though, I've had to take up another hobby besides adventuring. Did you know I do [leatherworking]?", Bolli);
            AddBehaviour(a);

            //next some text
            a = builder.CreateBehaviour(Bolli, -1);
            a.AddTrigger(eTriggerType.Whisper, "leatherworking", Bolli);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), Bolli);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "Oh aye, I have become quite handy with a leather working tool. But alas, I seem to have [misplaced] it recently.", Bolli);
            AddBehaviour(a);

            //next some text
            a = builder.CreateBehaviour(Bolli, -1);
            a.AddTrigger(eTriggerType.Whisper, "misplaced", Bolli);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), Bolli);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "I know...I know...I should have put it and my leather pieces away before heading to bed, but I just plain forgot! I don't suppose you could help a crippled old man [look] for it, can you?", Bolli);
            AddBehaviour(a);

            //next some text
            a = builder.CreateBehaviour(Bolli, -1);
            a.AddTrigger(eTriggerType.Whisper, "look", Bolli);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), Bolli);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "Oh wonder of wonders! I shall indeed make it worth your time and effort. Now, the first item I am missing is my [skinning knife].", Bolli);
            AddBehaviour(a);

            //next some text
            a = builder.CreateBehaviour(Bolli, -1);
            a.AddTrigger(eTriggerType.Whisper, "skinning knife", Bolli);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), Bolli);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "It is a wonderful knife, give to me as a present from a very old and sweet friend. I would be very appreciative if you could find it for me. I know there are creatures aroudn that like [shiny objects].", Bolli);
            AddBehaviour(a);


            //Quest accept
            a = builder.CreateBehaviour(Bolli, -1);
            a.AddTrigger(eTriggerType.Whisper, "shiny objects", Bolli);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), Bolli);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), null, (eComparator)5);
            a.AddAction(eActionType.OfferQuest, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), "Will you fetch the items for Bolli?");
            AddBehaviour(a);


            //Quest about?
            a = builder.CreateBehaviour(Bolli, -1);
            a.AddTrigger(eTriggerType.DeclineQuest, null, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies));
            a.AddAction(eActionType.Talk, "No problem. See you", Bolli);
            AddBehaviour(a);

            //Step 1
            a = builder.CreateBehaviour(Bolli, -1);
            a.AddTrigger(eTriggerType.AcceptQuest, null, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies));
            a.AddAction(eActionType.Talk, "Hrm...Huldus come to mind. There are a bunch of them on a hill to the west, right outside of town. See if one of them has my knife.", Bolli);
            a.AddAction(eActionType.GiveQuest, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), Bolli);
            AddBehaviour(a);

            //kill the lurker
            a = builder.CreateBehaviour(Bolli, -1);
            a.AddTrigger(eTriggerType.EnemyKilled, "huldu lurker", null);
            a.AddRequirement(eRequirementType.QuestStep, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), 1, (eComparator)3);
            a.AddAction(eActionType.GiveItem, Bollis_Skinning_knife, null);
            a.AddAction(eActionType.IncQuestStep, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), null);
            AddBehaviour(a);


            //we give the Knife to Bolli
            a = builder.CreateBehaviour(Bolli, -1);
            a.AddTrigger(eTriggerType.GiveItem, Bolli, Bollis_Skinning_knife);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), null);
            a.AddAction(eActionType.TakeItem, Bollis_Skinning_knife, null);
            a.AddAction(eActionType.Talk, "My knife! Oh thank you! Now, I am in need of just one more thing, if you are [willing to get it].", Bolli);
            a.AddAction(eActionType.GiveXP, 50, null);
            a.AddAction(eActionType.GiveGold, 500, null);
            a.AddAction(eActionType.IncQuestStep, typeof(BollisLeatherSupplies), null); //step 2
            AddBehaviour(a);


            //Step 2
            a = builder.CreateBehaviour(Bolli, -1);
            a.AddTrigger(eTriggerType.Whisper, "willing to get it", Bolli);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), null);
            a.AddAction(eActionType.Talk, "I knew you would. I was using the shinbone of a rattling skeleton to smooth out my skins so I could start making my leather. THat is now gone also. I think the skeleton I took if from [took it back].", Bolli);
            AddBehaviour(a);


            //Step 3
            a = builder.CreateBehaviour(Bolli, -1);
            a.AddTrigger(eTriggerType.Whisper, "took it back", Bolli);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), null);
            a.AddAction(eActionType.Talk, "Honestly, I didn't know they would be so sensitive, but it was the perfect bone for the job. Please go out and get me another shinbone!", Bolli);
            a.AddAction(eActionType.IncQuestStep, typeof(BollisLeatherSupplies), null);
            AddBehaviour(a);


            //Step 4 kill the skeleton"
            a = builder.CreateBehaviour(Bolli, -1);
            a.AddTrigger(eTriggerType.EnemyKilled, "rattling skeleton", null);
            a.AddRequirement(eRequirementType.QuestStep, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), 4, (eComparator)3);
            a.AddAction(eActionType.GiveItem, shinbone, null);
            a.AddAction(eActionType.IncQuestStep, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), null);
            AddBehaviour(a);


            //Step 5 now we give the bone to Bolli and at the end, the Quest reward is the 'Choker of the Bear'
            a = builder.CreateBehaviour(Bolli, -1);
            a.AddTrigger(eTriggerType.GiveItem, Bolli, shinbone);
            a.AddRequirement(eRequirementType.QuestStep, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), 5, (eComparator)3);
            a.AddAction(eActionType.Talk, "Ah! This will do nicely! I just hope its former owner doesn't come looking for it. hehe Anyhow, I thank you for your help in this matter, and maybe, in the future, I can show you some of my wares. Good luck in your travels!", Bolli);
            a.AddAction(eActionType.TakeItem, shinbone, null);
            a.AddAction(eActionType.GiveItem, Choker_of_the_Bear, Bolli);
            a.AddAction(eActionType.GiveXP, 80, null);
            a.AddAction(eActionType.GiveGold, 1000, null);
            a.AddAction(eActionType.FinishQuest, typeof(DOL.GS.Quests.Midgard.BollisLeatherSupplies), null);
            AddBehaviour(a);

            #endregion

            // Custom Scriptloaded Code Begin

            // Custom Scriptloaded Code End
            if (Bolli != null)
            {
                Bolli.AddQuestToGive(typeof(BollisLeatherSupplies));
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
            if (Bolli == null)
                return;
            /* Now we remove the possibility to give this quest to players */
            Bolli.RemoveQuestToGive(typeof(BollisLeatherSupplies));
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
                        return "[Step #1] Search the hills to the west of Mularn for a Huldu Lurker. See if the creature has Bolli's skinning knife.";

                    case 2:
                        return "[Step #2] Return Bolli's skinning knife to him in Mularn.";

                    case 3:
                        return "[Step #3]  listen to Bolli for your next step";

                    case 4:
                        return "[Step #4]  Find a rattling skeleton outside of Mularn. Retrieve one of its shinbones for Bolli.";

                    case 5:
                        return "[Step #5]   Return the Shin Bone to Bolli in Mularn.";



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
            if (player.IsDoingQuest(typeof(BollisLeatherSupplies)) != null)
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
