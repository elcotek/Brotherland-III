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
*Author         : Elcotek - Broken Tablet
*Source         : http://camelot.allakhazam.com
*Date           : 25 april 2015
*Quest Name     : Toa Wicoessa's Proposition (level 46-50)
*Quest Classes  : All no Maulers at the moment
*Quest Version  : beta v 0.2
*
*ToDo:
*   Add Bonuses to Epic Items
*   Add correct Text
*   Find Helm ModelID for epics..
*/




using System;
using System.Reflection;
using DOL.Database;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using DOL.Language;

namespace DOL.GS.Quests.Hibernia
{
    public class WicoessasPropositionToAALB : BaseQuest
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// /excavate command for Hib
        private static QuestSearchArea searchArea = new QuestSearchArea(typeof(WicoessasPropositionToAALB), 1, "Use /search to look near the loose stones.", 73, 274096, 527534, 3877);
        /// /excavate command for Hib
        //private static QuestSearchArea searchArea2 = new QuestSearchArea(typeof(WicoessasPropositionToAALB), 2, "Use /search to look near the loose stones.", 130, 268342, 526566, 3832);
        /// /excavate command for Hib
        // private static QuestSearchArea searchArea3 = new QuestSearchArea(typeof(WicoessasPropositionToAALB), 2, "Use /search to look near the loose stones.", 130, 290817, 537851, 3467);
        /// /excavate command for Hib
        // private static QuestSearchArea searchArea4 = new QuestSearchArea(typeof(WicoessasPropositionToAALB), 2, "Use /search to look near the loose stones.", 130, 291497, 527842, 4188);

        //Shark spawned?
        bool addShark1 = false;
        bool addShark2 = false;
        bool addShark3 = false;
        bool addShark4 = false;

        protected const string questTitle = "Wicoessa's Proposition";
        protected const int minimumLevel = 46;
        protected const int maximumLevel = 50;
        //public const int RitualSpell = 14375;
        protected const int addDespwanTime = 120; //Time in Sec
        protected const int templateID1 = 3001; //npcTemplate shrak1
        protected const int templateID2 = 3002; //npcTemplate shrak2


        //NPCs
        private static GameNPC npcsWicoessa = null; // Start NPC Wicoessa


        //Questitems
        private static ItemTemplate RuinStone = null; //RuinStone the stone
        private static ItemTemplate JunkStone = null; //JunkStone the junk

        //Rewards
        private static ItemTemplate AncientCarvedBreastplate = null; //Armsman, paladin
        private static ItemTemplate AncientTannedRobe = null; //Friar
        private static ItemTemplate AncientTannedJerkin = null; //Shadowblade
        private static ItemTemplate AncientWroughtHauberk = null; //Healer Shaman
        private static ItemTemplate AncientWroughtHauberkHib = null; //Druid
        private static ItemTemplate AncientForgedVest = null; //bard
        private static ItemTemplate AncientWroughtHauberkHib2 = null; //Champion, Hero, Warden
        private static ItemTemplate AncientWroughtHauberkAlb = null; //Mercenary, Minstrel, Reaver
        private static ItemTemplate AncientEmbroideredVest = null; //Valewalker
        private static ItemTemplate AncientEmbroideredVestHib2 = null; //Bainchi, Eldritch, Enchanter, Mentalist
        private static ItemTemplate AncientWroughtHauberkMid2 = null; //Skald, Thane, Valkrie, Warrior
        private static ItemTemplate AncientTannedJerkinAlb2 = null; //Infiltrator
        private static ItemTemplate AncientEmbroideredVestAlb2 = null; //Cabalist, Necromancer, Sorcerer, Theurg, Wizard
        private static ItemTemplate AncientForgedVestMid2 = null; //Berserker
        private static ItemTemplate AncientForgedVestMid3 = null; //Hunter
        private static ItemTemplate AncientForgedVestHib2 = null; //Blademaster, Ranger
        private static ItemTemplate AncientForgedVestAlb2 = null; //Scout
        private static ItemTemplate AncientForgedVestMid4 = null; //Savage
        private static ItemTemplate AncientTannedJerkinHib2 = null; //Nightshade
        private static ItemTemplate AncientHolyHauberk = null; //Cleric 
        private static ItemTemplate AncientEmbroideredTunic = null; //Bonedancer, Runemaster, Spiritmaster, Warlock
        private static ItemTemplate AncientVelveteenVest = null; //Heretic
        private static ItemTemplate AncientTannedBloodyJerkin = null; //Vampiir
        // Constructors
        public WicoessasPropositionToAALB()
            : base()
        {
            InitializeQuest(null);
        }

        public WicoessasPropositionToAALB(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
            InitializeQuest(questingPlayer);
        }

        public WicoessasPropositionToAALB(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
            InitializeQuest(questingPlayer);
        }

        public WicoessasPropositionToAALB(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest)
        {
            InitializeQuest(questingPlayer);
        }
        /// <summary>
        /// Perform any initialization actions needed when quest is created
        /// </summary>
        protected void InitializeQuest(GamePlayer player)
        {
            AddSearchArea(searchArea);
        }



        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            if (!ServerProperties.Properties.LOAD_QUESTS)
                return;
            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initializing ...");

            #region NPC Declarations

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Wicoessa", eRealm.Albion);

            if (npcs.Length == 0)
            {
                npcsWicoessa = new GameNPC();
                npcsWicoessa.Model = 329;
                npcsWicoessa.Name = "Wicoessa";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Wicoessa , creating it ...");
                npcsWicoessa.GuildName = "Wicoessa's Proposition";
                npcsWicoessa.Realm = eRealm.Albion;
                npcsWicoessa.CurrentRegionID = 73;
                npcsWicoessa.Size = 48;
                npcsWicoessa.Level = 55;
                npcsWicoessa.X = 270309;
                npcsWicoessa.Y = 541013;
                npcsWicoessa.Z = 8344;
                npcsWicoessa.Heading = 2811;
                npcsWicoessa.AddToWorld();
                if (SAVE_INTO_DATABASE)
                {
                    npcsWicoessa.SaveIntoDatabase();
                }

            }
            else
            {
                npcsWicoessa = npcs[0];
            }

            // end npc

            #endregion

            #region Item Declarations

            RuinStone = GameServer.Database.FindObjectByKey<ItemTemplate>("ruin_stone");
            if (RuinStone == null)
            {
                RuinStone = new ItemTemplate();
                RuinStone.Id_nb = "ruin_stone";
                RuinStone.Name = "ruin stone";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find ruin stone marrow mage , creating it ...");
                RuinStone.Level = 0;
                RuinStone.Item_Type = 41;
                RuinStone.Model = 529;
                RuinStone.IsDropable = false;
                RuinStone.IsPickable = true;
                RuinStone.DPS_AF = 0;
                RuinStone.SPD_ABS = 0;
                RuinStone.Object_Type = 0;
                RuinStone.Hand = 0;
                RuinStone.Type_Damage = 0;
                RuinStone.Quality = 49;
                RuinStone.Weight = 1;
                if (SAVE_INTO_DATABASE)
                {
                    GameServer.Database.AddObject(RuinStone);
                }

            }

            JunkStone = GameServer.Database.FindObjectByKey<ItemTemplate>("junk");
            if (JunkStone == null)
            {
                JunkStone = new ItemTemplate();
                JunkStone.Id_nb = "junk";
                JunkStone.Name = "junk";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find junk stone, creating it ...");
                JunkStone.Level = 0;
                JunkStone.Item_Type = 41;
                JunkStone.Model = 529;
                JunkStone.IsDropable = false;
                JunkStone.IsPickable = true;
                JunkStone.DPS_AF = 0;
                JunkStone.SPD_ABS = 0;
                JunkStone.Object_Type = 0;
                JunkStone.Hand = 0;
                JunkStone.Type_Damage = 0;
                JunkStone.Quality = 49;
                JunkStone.Weight = 1;
                if (SAVE_INTO_DATABASE)
                {
                    GameServer.Database.AddObject(JunkStone);
                }

            }


            // start reward items

            AncientCarvedBreastplate = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Carved_Breastplate"); //midgard
            AncientTannedRobe = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Tanned_Robe");
            AncientTannedJerkin = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Tanned_Jerkin");
            AncientWroughtHauberk = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Wrought_Hauberk");
            AncientWroughtHauberkHib = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Wrought_Hauberk_Hib");
            AncientForgedVest = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Forged_Vest");
            AncientWroughtHauberkHib2 = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Wrought_Hauberk_Hib2");
            AncientWroughtHauberkAlb = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Wrought_Hauberk_Alb");
            AncientEmbroideredVest = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Embroidered_Vest");
            AncientEmbroideredVestHib2 = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Embroidered_Vest_Hib2");
            AncientWroughtHauberkMid2 = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Wrought_Hauberk_Mid2");
            AncientTannedJerkinAlb2 = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Tanned_Jerkin_Alb2");
            AncientEmbroideredVestAlb2 = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Embroidered_Vest_Albion2");
            AncientForgedVestMid2 = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Forged_Vest_Mid2");
            AncientForgedVestMid3 = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Forged_Vest_Mid3");
            AncientForgedVestHib2 = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Forged_Vest_Hib2");
            AncientForgedVestAlb2 = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Forged_Vest_Alb2");
            AncientForgedVestMid4 = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Forged_Vest_Mid4");
            AncientTannedJerkinHib2 = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Tanned_Jerkin_Hib2");
            AncientHolyHauberk = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Holy_Hauberk");
            AncientEmbroideredTunic = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Embroidered_Tunic");
            AncientVelveteenVest = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Velveteen_Vest");
            AncientTannedBloodyJerkin = GameServer.Database.FindObjectByKey<ItemTemplate>("Ancient_Tanned_Bloody_Jerkin");


            //Item Descriptions End

            #endregion

            GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.AddHandler(npcsWicoessa, GameObjectEvent.Interact, new DOLEventHandler(TalkToWicoessa));
            GameEventMgr.AddHandler(npcsWicoessa, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToWicoessa));


            /* Now we bring to Ainrebh the possibility to give this quest to players */
            npcsWicoessa.AddQuestToGive(typeof(WicoessasPropositionToAALB));

            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initialized");
        }

        [ScriptUnloadedEvent]
        public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
        {
            //if not loaded, don't worry
            if (npcsWicoessa == null)
                return;
            // remove handlers
            GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.RemoveHandler(npcsWicoessa, GameObjectEvent.Interact, new DOLEventHandler(TalkToWicoessa));
            GameEventMgr.RemoveHandler(npcsWicoessa, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToWicoessa));

            /* Now we remove to Ainrebh the possibility to give this quest to players */
            npcsWicoessa.RemoveQuestToGive(typeof(WicoessasPropositionToAALB));
        }

        protected static void TalkToWicoessa(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (npcsWicoessa.CanGiveQuest(typeof(WicoessasPropositionToAALB), player) <= 0)
                return;

            //We also check if the player is already doing the quest
            WicoessasPropositionToAALB quest = player.IsDoingQuest(typeof(WicoessasPropositionToAALB)) as WicoessasPropositionToAALB;

            npcsWicoessa.TurnTo(player);
            if (e == GameObjectEvent.Interact)
            {
                if (quest != null)
                {
                    npcsWicoessa.SayTo(player, "Check your Journal for instructions!");
                }
                else
                {
                    npcsWicoessa.SayTo(player, "You are here at the right time and place, nice to see you my friend, i need a unique stone at the ruins they are underwater!"
                        + " my strenthness is not geminite to search by self for this unique, but i need it to finish my jop here!" +
                        " if you do the jop so to be careful for the Sharks, near the Ruin's! you be sure to [Help] me ?");
                }

            }

            // The player whispered to the NPC
            else if (e == GameLivingEvent.WhisperReceive)
            {
                WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;
                //Check player is already doing quest
                if (quest == null)
                {
                    switch (wArgs.Text)
                    {
                        case "Help":
                            player.Out.SendQuestSubscribeCommand(npcsWicoessa, QuestMgr.GetIDForQuestType(typeof(WicoessasPropositionToAALB)), "Oh good! please use /search at the the Ruins not far from here.");
                            break;
                    }
                }
                else
                {
                    switch (wArgs.Text)
                    {
                        case "abort":
                            player.Out.SendCustomDialog("Do you really want to abort this quest, \nall items gained during quest will be lost?", new CustomDialogResponse(CheckPlayerAbortQuest));
                            break;
                    }
                }
            }
        }



        public override bool CheckQuestQualification(GamePlayer player)
        {
            // if the player is already doing the quest his level is no longer of relevance
            if (player.IsDoingQuest(typeof(WicoessasPropositionToAALB)) != null)
                return true;

            if (player.Level < minimumLevel || player.Level > maximumLevel)
                return false;

            return true;
        }

        /* This is our callback hook that will be called when the player clicks
         * on any button in the quest offer dialog. We check if he accepts or
         * declines here...
         */

        private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
        {
            WicoessasPropositionToAALB quest = player.IsDoingQuest(typeof(WicoessasPropositionToAALB)) as WicoessasPropositionToAALB;

            if (quest == null)
                return;

            if (response == 0x00)
            {
                SendSystemMessage(player, "Good look, now go out there and search the stone!");
            }
            else
            {
                SendSystemMessage(player, "Aborting Quest " + questTitle + ". You can start over again if you want.");
                quest.AbortQuest();
            }
        }

        protected static void SubscribeQuest(DOLEvent e, object sender, EventArgs args)
        {
            QuestEventArgs qargs = args as QuestEventArgs;
            if (qargs == null)
                return;

            if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(WicoessasPropositionToAALB)))
                return;

            if (e == GamePlayerEvent.AcceptQuest)
                CheckPlayerAcceptQuest(qargs.Player, 0x01);
            else if (e == GamePlayerEvent.DeclineQuest)
                CheckPlayerAcceptQuest(qargs.Player, 0x00);
        }

        private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
        {
            if (npcsWicoessa.CanGiveQuest(typeof(WicoessasPropositionToAALB), player) <= 0)
                return;


            if (player.IsDoingQuest(typeof(WicoessasPropositionToAALB)) != null)
                return;

            if (response == 0x00)
            {
                player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            }
            else
            {

                //Check if we can add the quest!
                if (!npcsWicoessa.GiveQuest(typeof(WicoessasPropositionToAALB), player, 1))
                    return;
                //step 2
                player.Out.SendMessage("you have to /search at the ruins at loc 11.6k, 3.2k! take care of the sharks!", eChatType.CT_System, eChatLoc.CL_PopupWindow);

            }
        }

        //Set quest name
        public override string Name
        {
            get { return "Wicoessa's Proposition (Level 46-50 ToA Quest)"; }
        }

        // Define Steps
        public override string Description
        {
            get
            {
                switch (Step)
                {
                    case 1:
                        return "[Step #1] Wicoessa is wanting you to /search one unique stone at the ruins, underwater in Oceanus Hesperos. at loc 11.6k, 3.2k";

                    case 2:
                        // return "[Step #2] you have to collect some stones near the ruins.";
                        return "[Step #2] go back to Wicoessa and give her the missing stone and take your reward.";

                    // case 3:
                    //     return "[Step #3]  travel back to Wicoessa give her the stones and take your reward.";


                }
                return base.Description;
            }
        }

        /// <summary>
        /// This is the critical method to override when using the /search command
        /// For this quest will will give the player the item and advance the quest step.
        /// </summary>
        /// <param name="command"></param>
        protected override void QuestCommandCompleted(AbstractQuest.eQuestCommand command, GamePlayer player)
        {

            if (command == eQuestCommand.Search)
            {

                Console.WriteLine("Quest step = {0}", Step);

                {

                    if (Step == 1)
                    {
                        int rand = Util.Random(1, 7);

                        if (rand == 1 && addShark1 == false)
                        {
                            SpawnAdd(templateID1, Util.Random(46, 55), 274503, 527773, 4120, addDespwanTime);
                            m_questPlayer.Out.SendMessage("You find nothing", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            addShark1 = true;
                            return;
                        }

                        else if (rand == 2 && addShark2 == false)
                        {
                            SpawnAdd(templateID1, Util.Random(46, 55), 274431, 527199, 4120, addDespwanTime);
                            m_questPlayer.Out.SendMessage("You find nothing", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            addShark2 = true;
                            return;
                        }
                        else if (rand == 3 && addShark3 == false)
                        {
                            SpawnAdd(templateID2, Util.Random(46, 55), 273850, 527576, 3996, addDespwanTime);
                            m_questPlayer.Out.SendMessage("You find nothing", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            addShark3 = true;
                            return;
                        }
                        else if (rand == 4 && addShark4 == false)
                        {
                            SpawnAdd(templateID2, Util.Random(46, 55), 273718, 527576, 3996, addDespwanTime);
                            m_questPlayer.Out.SendMessage("You find nothing", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            addShark4 = true;
                            return;
                        }
                        else if (rand == 5 && TryGiveItem(QuestPlayer, JunkStone))
                        {
                            m_questPlayer.Out.SendMessage("You find junk", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }
                        else if (rand == 6 && TryGiveItem(QuestPlayer, RuinStone))
                        {
                            m_questPlayer.Out.SendMessage("Congratulations! You find the missing stone from the Ruin!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            Step = 2;
                            return;
                        }
                        else
                        {
                            m_questPlayer.Out.SendMessage("You find nothing, you have to search more!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }
                    }
                }
            }
        }
        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = sender as GamePlayer;

            if (player == null || player.IsDoingQuest(typeof(WicoessasPropositionToAALB)) == null)
                return;
            /*
            if (Step == 2 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;

                if (gArgs.Target.Name == RuinStone.Name)
                {
                    m_questPlayer.Out.SendMessage("You collect one stone", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    GiveItem(m_questPlayer, RuinStone);
                    Step = 3;
                    return;
                }

            }
           */
            if (Step == 2 && e == GamePlayerEvent.GiveItem)
            {
                GiveItemEventArgs gArgs1 = (GiveItemEventArgs)args;
                if (gArgs1.Target.Name == npcsWicoessa.Name && gArgs1.Item.Id_nb == RuinStone.Id_nb)
                {



                    npcsWicoessa.SayTo(player, "Oh great, now i can do my jop, thank you! take is a little reward for your good jop!");
                    FinishQuest();

                    return;
                }
            }
        }



        public override void AbortQuest()
        {
            base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

            RemoveItem(m_questPlayer, RuinStone, false);
            RemoveItem(m_questPlayer, JunkStone, false);

        }

        public override void FinishQuest()
        {
            // Here we try to place the item in the players backpack.  If their inventory is full it fails and quest does not advance.
            if (m_questPlayer.Inventory.IsSlotsFree(3, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
            {
                addShark1 = false;
                addShark2 = false;
                addShark3 = false;
                addShark4 = false;
                RemoveItem(npcsWicoessa, m_questPlayer, RuinStone);
                base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...





                //Rewards for Midgard
                //Bonedancer, Runemaster, Spiritmaster, Warlock
                if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Bonedancer ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Runemaster ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Spiritmaster ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Warlock)
                {


                    GiveItem(m_questPlayer, AncientEmbroideredTunic);

                }
                //Shadowblade
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.MaulerMid ||
                   m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Shadowblade)
                {
                    GiveItem(m_questPlayer, AncientTannedJerkin);

                }
                //Skald, Thane, Valkrie, Warrior
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Skald ||
                   m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Thane ||
                   m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Valkyrie ||
                   m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Warrior)
                {
                    GiveItem(m_questPlayer, AncientWroughtHauberkMid2);
                }
                //Healer Shaman
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Healer ||
                   m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Shaman)
                {
                    GiveItem(m_questPlayer, AncientWroughtHauberk);
                }
                //Berserker
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Berserker)
                {
                    GiveItem(m_questPlayer, AncientForgedVestMid2);
                }
                //Savage
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Savage)
                {
                    GiveItem(m_questPlayer, AncientForgedVestMid4);
                }
                //Hunter
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Hunter)
                {
                    GiveItem(m_questPlayer, AncientForgedVestMid3);
                }

                //Rewards for Albion
                //Friar
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Friar ||
                      m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.MaulerAlb)
                {
                    GiveItem(m_questPlayer, AncientTannedRobe);
                }
                //Cabalist, Necromancer, Sorcerer, Theurg, Wizard
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Cabalist ||
                  m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Necromancer ||
                  m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Sorcerer ||
                  m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Theurgist ||
                  m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Wizard)
                {

                    GiveItem(m_questPlayer, AncientEmbroideredVestAlb2);

                }
                //Infiltrator
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Infiltrator)
                {

                    GiveItem(m_questPlayer, AncientTannedJerkinAlb2);

                }
                //Scout
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Scout)
                {
                    GiveItem(m_questPlayer, AncientForgedVestAlb2);

                }
                //Armsman, paladin
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Armsman ||
                   m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Paladin)
                {
                    GiveItem(m_questPlayer, AncientCarvedBreastplate);

                }
                //Mercenary, Minstrel, Reaver
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Mercenary ||
                  m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Minstrel ||
                  m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Reaver)
                {
                    GiveItem(m_questPlayer, AncientWroughtHauberkAlb);
                }
                //Heretic
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Heretic)
                {
                    GiveItem(m_questPlayer, AncientVelveteenVest);
                }
                //Cleric 
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Cleric)
                {
                    GiveItem(m_questPlayer, AncientHolyHauberk);
                }

                //Rewards for Hibernia
                //Bainchi, Eldritch, Enchanter, Mentalist
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Animist ||
                  m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Bainshee ||
                  m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Eldritch ||
                  m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Enchanter ||
                  m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Mentalist)
                {
                    GiveItem(m_questPlayer, AncientEmbroideredVestHib2);
                }
                //Champion, Hero, Warden
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Champion ||
                   m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Hero ||
                   m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Warden)
                {
                    GiveItem(m_questPlayer, AncientWroughtHauberkHib2);
                }
                //bard
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Bard)
                {
                    GiveItem(m_questPlayer, AncientForgedVest);
                }
                //Blademaster, Ranger
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Blademaster
                         || m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Ranger)
                {
                    GiveItem(m_questPlayer, AncientForgedVestHib2);
                }
                //Druid
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Druid)
                {
                    GiveItem(m_questPlayer, AncientWroughtHauberkHib);
                }
                //Valewalker
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Valewalker)
                {
                    GiveItem(m_questPlayer, AncientEmbroideredVest);
                }
                //Nightshade
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Nightshade)
                {
                    GiveItem(m_questPlayer, AncientTannedJerkinHib2);
                }
                //Vampiir
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Vampiir)
                {
                    GiveItem(m_questPlayer, AncientTannedBloodyJerkin);
                }



                m_questPlayer.GainExperience(GameLiving.eXPSource.Quest, 132899648, true);
                m_questPlayer.AddMoney(Money.GetMoney(0, 0, 36, 22, Util.Random(50)), "You recieve {0} as a reward.");
            }
            else
            {
                m_questPlayer.Out.SendMessage("You do not have enough free space in your inventory!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
            }
        }

        #region Allakhazam Broken Tablet Source


        private INpcTemplate m_addTemplate;

        /// <summary>
        /// Create an add from the specified template.
        /// </summary>
        /// <param name="templateID"></param>
        /// <param name="level"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="uptime"></param>
        /// <returns></returns>
        protected GameNPC SpawnAdd(int templateID, int level, int x, int y, int Z, int uptime)
        {
            GameNPC add = null;

            try
            {
                if (m_addTemplate == null || m_addTemplate.TemplateId != templateID)
                {
                    m_addTemplate = NpcTemplateMgr.GetTemplate(templateID);
                }

                // Create add from template.


                if (m_addTemplate != null)
                {
                    add = new GameNPC(m_addTemplate);
                    add.CurrentRegion = m_questPlayer.CurrentRegion;
                    add.Heading = (ushort)(Util.Random(0, 4095));
                    add.Realm = 0;
                    add.X = x;
                    add.Y = y;
                    add.Z = Z;
                    add.CurrentSpeed = 0;
                    add.Level = (byte)level;
                    add.RespawnInterval = -1;
                    add.AddToWorld();
                    //  foreach (GameNPC npc in QuestPlayer.GetNPCsInRadius(1500))
                    // {
                    //   if (npc != null && npc.NPCTemplate != null)
                    new DespawnTimer(add, add, uptime * 1000);
                }
            }

            catch
            {
                log.Warn(String.Format("Unable to get template for {0}", Name));
            }
            return add;
        }
        /// <summary>
        /// Provides a timer to remove an NPC from the world after some
        /// time has passed.
        /// </summary>
        protected class DespawnTimer : GameTimer
        {
            private GameNPC m_npc;

            /// <summary>
            /// Constructs a new DespawnTimer.
            /// </summary>
            /// <param name="timerOwner">The owner of this timer.</param>
            /// <param name="npc">The GameNPC to despawn when the time is up.</param>
            /// <param name="delay">The time after which the add is supposed to despawn.</param>
            public DespawnTimer(GameObject timerOwner, GameNPC npc, int delay)
                : base(timerOwner.CurrentRegion.TimeManager)
            {
                m_npc = npc;
                Start(delay);
            }

            /// <summary>
            /// Called on every timer tick.
            /// </summary>
            protected override void OnTick()
            {
                // Remove the NPC from the world.

                if (m_npc != null)
                {
                    m_npc.Delete();
                    m_npc = null;
                }
            }
        }
        /*
        You need to have good Skyros faction to do this quest.

1) Travel to Mesothalassa loc=16.5k, 34.9k and speak to Glycon.

2) Glycon will send you to go speak with Alyciana in Mesothalassa 12.5k, 32.2k.

3) Alyciana will send you to kill one Melos and Naxos triton. They will drop blood.

4) Take the blood and combine it ('E' key on toolbar) and it will create a potion.

5) Place the potion on your hotbar and use it to update your journal to see what you need to do next.

6) Travel to Oceanus Notos loc=42.9k, 49.1k and kill Khahet. Upon his death he will drop half of a Broken Tablet.

7) Return to Alyciana in Mesothalassa and hand her the tablet. She will give it back to you to read and then request it back along with the potion you created.

8) Alyciana will then send you to kill Bryseia (45k, 23.2k) and Mikon (41.7k, 46.8k) in Mesothalassa.

9) Return to Alyciana for your reward.

lvl 45 received 1,407,188,992 exp and 15 gold.
lvl 48 received 2,132,899,648 exp and 15 gold.

Rewards Classes:
Skyros Octopus Skin Sleeves -- Infiltrator and Friar
Skyros Eel Skin Pants -- Bonedancer, Runemaster, and Spiritmaster
Skyros Spiny Urchin Boots -- Champion, Hero, and Warden
Skyros Starfish Studded Vest -- Scout
Skyros Laced Eel Skin Pants -- Animist, Eldritch, Enchanter, Mentalist
Skyros Spiny Urchin Hauberk -- Druid
Skyros Octopus Tentacle Sleeves -- Shadowblade
Skyros Sea Urchin Boots -- Skald, Thane, Warrior and Valkyrie
Skyros Crabshell Boots -- Armsman, Paladin
Skyros Sea Urchin Hauberk -- Healer, Shaman
Skyros Starfish Wrapped Sleeves -- Bard, Blademaster, Ranger
Skyros Woven Eel Skin Pants -- Heretic


  Rewards:
  Skyros Eel Skin Pants
  Skyros Octopus Skin Sleeves
  Skyros Spiny Urchin Boots
  Skyros Starfish Studded Vest
  Skyros Laced Eel Skin Pants
  Skyros Spiny Urchin Hauberk
  Skyros Braided Eel Skin Pants
  Skyros Octopus Tentacle Sleeves
  Skyros Sea Urchin Boots
  Skyros Crabshell Boots
  Skyros Sea Urchin Hauberk
  Skyros Starfish Wrapped Sleeves
  Skyros Woven Eel Skin Pants
  Skyros Spiked Urchin Boots

        */

        #endregion
    }
}
