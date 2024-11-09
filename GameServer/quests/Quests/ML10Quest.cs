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
*Author         : Elcotek - ML10
*Source         : http://camelot.allakhazam.com
*Date           : 29 april 2015
*Quest Name     : Toa Quest ML10 (Mystery of Draco)
*Quest Classes  : All
*Quest Version  : v 1.2
*
*ToDo:
*   
*   Add correct Text
*   
*/

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Behaviour;
using log4net;

namespace DOL.GS.Quests.Hibernia
{
    public class ML10Quest : BaseQuest
    {

        protected const int addUtimeTime = 1850;// 1850; //Time in Sekunden = 30 minuten
        protected const int addDespwanTime = 70;// 40; //Time in Sekunden = 30 ekunden
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected const string questTitle = "Mystery of Celestius";
        protected const int minimumLevel = 50;
        protected const int maximumLevel = 50;
        public const int RitualSpell = 4914;
        public const int CallSpell = 321;
        //NPCs
        private static GameNPC npcsEstefa = null; // Start NPC  in Celestius

        protected const int templateScorpius = 3011; //npcTemplate Scorpius
        protected const int templateCentaurus = 3012; //npcTemplate Centaurus
        protected const int templateLeo = 3013; //npcTemplate Leo
        protected const int templateDraco = 3014; //npcTemplate Draco


        //Questitems
        private static ItemTemplate TailOfSagittarius = null; //blood drop Naxos
        private static ItemTemplate stingerOfScorpius = null; //blood drop Naxos
        private static ItemTemplate HeadOfCentaurus = null; //blood drop Naxos
        private static ItemTemplate SkinOfLeo = null; //blood drop Naxos

        //Rewards
        private static ItemTemplate ML10Token = null; //Mist Shrouded Boots 

        const string Scorpius_SPAWNED = "Scorpius_spawned";
        const string Centaurus_SPAWNED = "Centaurus_spawned";
        const string Leo_SPAWNED = "Leo_spawned";
        const string Draco_SPAWNED = "Draco_spawned";
        /// <summary>
        /// Retrieves how much time player can do the quest
        /// </summary>
        public override int MaxQuestCount
        {
            get { return int.MaxValue; }
        }

        // Constructors
        public ML10Quest()
            : base()
        {
        }

        public ML10Quest(GamePlayer questingPlayer)
            : base(questingPlayer)
        {
        }

        public ML10Quest(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
        }

        public ML10Quest(GamePlayer questingPlayer, DBQuest dbQuest)
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

            #region NPC Declarations

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Estefa of Atlantis", 0);

            if (npcs.Length == 0)
            {
                npcsEstefa = new GameNPC
                {
                    Model = 989,
                    Name = "Estefa of Atlantis"
                };
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Estefa of Atlantis, creating it ...");
                npcsEstefa.GuildName = "Masterlevel 10 Quest";
                npcsEstefa.Realm = 0;
                npcsEstefa.Flags |= GameNPC.eFlags.PEACE;
                npcsEstefa.CurrentRegionID = 91;
                npcsEstefa.Size = 35;
                npcsEstefa.Level = 70;
                npcsEstefa.X = 31883;
                npcsEstefa.Y = 31907;
                npcsEstefa.Z = 15844;
                npcsEstefa.Heading = 4085;
                npcsEstefa.AddToWorld();
                if (SAVE_INTO_DATABASE)
                {
                    npcsEstefa.SaveIntoDatabase();
                }

            }
            else
            {
                npcsEstefa = npcs[0];
            }

            // end npc

            #endregion

            #region Item Declarations

            TailOfSagittarius = GameServer.Database.FindObjectByKey<ItemTemplate>("Tail_Of_Sagittarius");
            if (TailOfSagittarius == null)
            {
                TailOfSagittarius = new ItemTemplate
                {
                    Id_nb = "Tail_Of_Sagittarius",
                    Name = "Tail Of Sagittarius"
                };
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Tail Of Sagittarius, creating it ...");
                TailOfSagittarius.Level = 0;
                TailOfSagittarius.Item_Type = 41;
                TailOfSagittarius.Model = 515;
                TailOfSagittarius.IsDropable = false;
                TailOfSagittarius.IsPickable = true;
                TailOfSagittarius.DPS_AF = 0;
                TailOfSagittarius.SPD_ABS = 0;
                TailOfSagittarius.Object_Type = 0;
                TailOfSagittarius.Hand = 0;
                TailOfSagittarius.Type_Damage = 0;
                TailOfSagittarius.Quality = 49;
                TailOfSagittarius.Weight = 1;
                if (SAVE_INTO_DATABASE)
                {
                    GameServer.Database.AddObject(TailOfSagittarius);
                }

            }

            stingerOfScorpius = GameServer.Database.FindObjectByKey<ItemTemplate>("stinger_Of_Scorpius");
            if (stingerOfScorpius == null)
            {
                stingerOfScorpius = new ItemTemplate
                {
                    Id_nb = "stinger_Of_Scorpius",
                    Name = "Stinger of Scorpius"
                };
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Stinger of Scorpius, creating it ...");
                stingerOfScorpius.Level = 0;
                stingerOfScorpius.Item_Type = 41;
                stingerOfScorpius.Model = 586;
                stingerOfScorpius.IsDropable = false;
                stingerOfScorpius.IsPickable = true;
                stingerOfScorpius.DPS_AF = 0;
                stingerOfScorpius.SPD_ABS = 0;
                stingerOfScorpius.Object_Type = 0;
                stingerOfScorpius.Hand = 0;
                stingerOfScorpius.Type_Damage = 0;
                stingerOfScorpius.Quality = 49;
                stingerOfScorpius.Weight = 1;
                if (SAVE_INTO_DATABASE)
                {
                    GameServer.Database.AddObject(stingerOfScorpius);
                }

            }
            HeadOfCentaurus = GameServer.Database.FindObjectByKey<ItemTemplate>("Head_Of_Centaurus");
            if (HeadOfCentaurus == null)
            {
                HeadOfCentaurus = new ItemTemplate
                {
                    Id_nb = "Head_Of_Centaurus",
                    Name = "Head Of Centaurus"
                };
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Head of Centaurus, creating it ...");
                HeadOfCentaurus.Level = 0;
                HeadOfCentaurus.Item_Type = 41;
                HeadOfCentaurus.Model = 503;
                HeadOfCentaurus.IsDropable = false;
                HeadOfCentaurus.IsPickable = true;
                HeadOfCentaurus.DPS_AF = 0;
                HeadOfCentaurus.SPD_ABS = 0;
                HeadOfCentaurus.Object_Type = 0;
                HeadOfCentaurus.Hand = 0;
                HeadOfCentaurus.Type_Damage = 0;
                HeadOfCentaurus.Quality = 49;
                HeadOfCentaurus.Weight = 1;
                if (SAVE_INTO_DATABASE)
                {
                    GameServer.Database.AddObject(HeadOfCentaurus);
                }

            }



            SkinOfLeo = GameServer.Database.FindObjectByKey<ItemTemplate>("Skin_Of_Leo");
            if (SkinOfLeo == null)
            {
                SkinOfLeo = new ItemTemplate
                {
                    Id_nb = "Skin_Of_Leo",
                    Name = "Skin of Leo"
                };
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Skin of Leo, creating it ...");
                SkinOfLeo.Level = 0;
                SkinOfLeo.Item_Type = 41;
                SkinOfLeo.Model = 506;
                SkinOfLeo.IsDropable = false;
                SkinOfLeo.IsPickable = true;
                SkinOfLeo.DPS_AF = 0;
                SkinOfLeo.SPD_ABS = 0;
                SkinOfLeo.Object_Type = 0;
                SkinOfLeo.Hand = 0;
                SkinOfLeo.Type_Damage = 0;
                SkinOfLeo.Quality = 0;
                SkinOfLeo.Weight = 12;
                if (SAVE_INTO_DATABASE)
                {
                    GameServer.Database.AddObject(SkinOfLeo);
                }

            }
            // start reward items

            ML10Token = GameServer.Database.FindObjectByKey<ItemTemplate>("ml10token"); //ML Book


            //Item Descriptions End

            #endregion

            GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));


            GameEventMgr.AddHandler(npcsEstefa, GameObjectEvent.Interact, new DOLEventHandler(TalkTonEstefa));
            GameEventMgr.AddHandler(npcsEstefa, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkTonEstefa));



            /* Now we bring to Ainrebh the possibility to give this quest to players */
            npcsEstefa.AddQuestToGive(typeof(ML10Quest));

            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initialized");
        }

        [ScriptUnloadedEvent]
        public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
        {
            //if not loaded, don't worry
            if (npcsEstefa == null)
                return;
            // remove handlers
            GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.RemoveHandler(npcsEstefa, GameObjectEvent.Interact, new DOLEventHandler(TalkTonEstefa));
            GameEventMgr.RemoveHandler(npcsEstefa, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkTonEstefa));

            /* Now we remove to Ainrebh the possibility to give this quest to players */
            npcsEstefa.RemoveQuestToGive(typeof(ML10Quest));
        }

        protected static void TalkTonEstefa(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (npcsEstefa.CanGiveQuest(typeof(ML10Quest), player) <= 0)
                return;

         
            if (IsBosesIncombatOrMoving())
            {
                npcsEstefa.SayTo(player, " " + player.Name + " I won't let you disrupt the current round. Come back to me once the fighters have completed their tasks.");
                return;
            }
          

            //We also check if the player is already doing the quest
            ML10Quest quest = player.IsDoingQuest(typeof(ML10Quest)) as ML10Quest;
            npcsEstefa.TurnTo(player);
            if (e == GameObjectEvent.Interact)
            {
                if (quest != null)
                {
                    npcsEstefa.SayTo(player, "Check your Journal for instructions!");
                }
                else
                {
                    npcsEstefa.SayTo(player, " " + player.Name + " you are on a historic place, to be a part of all them, you must conquer five opponents. [more]..");
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
                        case "more":
                            npcsEstefa.SayTo(player, " " + player.Name + " many heros have lost there lives at this place, but you have the chance to change something. - Do you [want] to take on this task " + player.Name + "?");
                            break;

                        case "want":
                            player.Out.SendQuestSubscribeCommand(npcsEstefa, QuestMgr.GetIDForQuestType(typeof(ML10Quest)), " " + npcsEstefa.Name + " says, may zeus be by your side " + player.Name + " - good luck!");
                            break;
                    }
                }
                else
                {
                    switch (wArgs.Text)
                    {
                        case "abort":
                            player.Out.SendCustomDialog("Do you really want to abort this quest, \n all items gained during quest will be lost?", new CustomDialogResponse(CheckPlayerAbortQuest));
                            break;
                    }
                }
            }
        }

        public override bool CheckQuestQualification(GamePlayer player)
        {
            // if the player is already doing the quest his level is no longer of relevance
            if (player.IsDoingQuest(typeof(ML10Quest)) != null)
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
            ML10Quest quest = player.IsDoingQuest(typeof(ML10Quest)) as ML10Quest;

            if (quest == null)
                return;

            if (response == 0x00)
            {
                SendSystemMessage(player, "Good, now go out there and finish your work!");

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

            if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(ML10Quest)))
                return;

            if (e == GamePlayerEvent.AcceptQuest)
                CheckPlayerAcceptQuest(qargs.Player, 0x01);
            else if (e == GamePlayerEvent.DeclineQuest)
                CheckPlayerAcceptQuest(qargs.Player, 0x00);
        }

        private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
        {
            if (npcsEstefa.CanGiveQuest(typeof(ML10Quest), player) <= 0)
                return;

           
            if (player.IsDoingQuest(typeof(ML10Quest)) != null)
                return;

            
            if (response == 0x00)
            {
                player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            }
            else
            {
                //Check if we can add the quest!
                if (!npcsEstefa.GiveQuest(typeof(ML10Quest), player, 1))
                    return;
                //step 2
                player.Out.SendMessage(" " + npcsEstefa.Name+ " says, Sly first Sagittarius for her Tail.", eChatType.CT_System, eChatLoc.CL_PopupWindow);

            }
        }


        //Set quest name
        public override string Name
        {
            get { return "Mystery of Celestius [Masterlevel 10 Quest]"; }
        }

        // Define Steps
        public override string Description
        {
            get
            {
                switch (Step)
                {
                    case 1:
                        {
                            if (m_questPlayer.CurrentRegionID == 91)
                            {
                                m_questPlayer.Out.SendSoundEffect(812, 0, 0, 0, 0, 0);
                            }
                        }
                        return "Sly first Sagittarius for her Tail.";

                    case 2:
                        {
                            if (m_questPlayer.CurrentRegionID == 91)
                            {
                                m_questPlayer.Out.SendSoundEffect(933, 0, 0, 0, 0, 0);
                                m_questPlayer.Out.SendSoundEffect(753, 0, 0, 0, 0, 0);
                            }
                        }
                        return "Now sly Scorpius.";

                    case 3:
                        {
                            if (m_questPlayer.CurrentRegionID == 91)
                            {
                                m_questPlayer.Out.SendSoundEffect(808, 0, 0, 0, 0, 0);
                            }
                        }
                        return "Now sly Centaurus.";
                    case 4:
                        {
                            if (m_questPlayer.CurrentRegionID == 91)
                            {
                                m_questPlayer.Out.SendSoundEffect(811, 0, 0, 0, 0, 0);
                            }
                        }
                        return "Now sly Leo.";
                    case 5:
                        {
                            if (m_questPlayer.CurrentRegionID == 91)
                            {
                                m_questPlayer.Out.SendSoundEffect(807, 0, 0, 0, 0, 0);
                            }

                        }
                        return "Now its time to sly Draco!";
                    case 6:
                        {
                            if (m_questPlayer.CurrentRegionID == 91)
                            {
                                m_questPlayer.Out.SendSoundEffect(689, 0, 0, 0, 0, 0);
                            }

                        }
                        return "Hand the Tail to Estefa.";
                    case 7:
                        {

                        }
                        return "Hand the Stinger to Estefa.";


                    case 8:
                        {
                            return "Hand the head to Estefa..";
                        }


                    case 9:
                        {
                            return "Hand the Skin to Estefa.";
                        }
                }
                return base.Description;
            }
        }





        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = sender as GamePlayer;

            if (player == null || player.IsDoingQuest(typeof(ML10Quest)) == null)
                return;



            //Sagittarius
            if (Step == 1 && e == GameLivingEvent.EnemyKilled)
            {

                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;


                if (gArgs.Target.Name.IndexOf("Sagittarius") >= 0)
                {

                    m_questPlayer.Out.SendMessage("You collect the Tail Of Sagittarius", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    GiveItem(m_questPlayer, TailOfSagittarius);



                    if (npcsEstefa != null && npcsEstefa.TempProperties.getProperty<bool>(Scorpius_SPAWNED) == false)
                    {
                        SpawnAdd(templateScorpius, Util.Random(65, 68), 31817, 30283, 15733, 3065, addDespwanTime);
                        npcsEstefa.TempProperties.setProperty(Scorpius_SPAWNED, true);
                        npcsEstefa.CastSpell(SkillBase.GetSpellByID(CallSpell), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    }


#pragma warning disable CS0618 // Typ oder Element ist veraltet
                    npcsEstefa.WalkTo(31817, 30283, 15733, 50);
#pragma warning restore CS0618 // Typ oder Element ist veraltet
                    m_questPlayer.Out.SendMessage("Wait a moment I summon now Scorpius!.", eChatType.CT_Broadcast, eChatLoc.CL_SystemWindow);
                    Step = 2;
                    return;
                }

            }

            //Scorpius
            if (Step == 2 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;


                if (gArgs.Target.Name.IndexOf("Scorpius") >= 0)
                {

                    m_questPlayer.Out.SendMessage("You collect the Stinger Of Scorpius", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    GiveItem(m_questPlayer, stingerOfScorpius);

                    if (npcsEstefa != null && npcsEstefa.TempProperties.getProperty<bool>(Centaurus_SPAWNED) == false)
                    {
                        SpawnAdd(templateCentaurus, Util.Random(65, 68), 31817, 30283, 15733, 3065, addDespwanTime);
                        npcsEstefa.TempProperties.setProperty(Centaurus_SPAWNED, true);
                        npcsEstefa.CastSpell(SkillBase.GetSpellByID(CallSpell), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));

                    }


#pragma warning disable CS0618 // Typ oder Element ist veraltet
                    npcsEstefa.WalkTo(31817, 30283, 15733, 50);
#pragma warning restore CS0618 // Typ oder Element ist veraltet
                    m_questPlayer.Out.SendMessage("Wait a moment I summon now Centaurus!.", eChatType.CT_Broadcast, eChatLoc.CL_SystemWindow);
                    Step = 3;
                    return;
                }

            }
            //Centaurus
            if (Step == 3 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;


                if (gArgs.Target.Name.IndexOf("Centaurus") >= 0)
                {
                    m_questPlayer.Out.SendMessage("You collect the head Of Centaurus", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    GiveItem(m_questPlayer, HeadOfCentaurus);


                    if (npcsEstefa != null && npcsEstefa.TempProperties.getProperty<bool>(Leo_SPAWNED) == false)
                    {
                        SpawnAdd(templateLeo, Util.Random(65, 68), 31817, 30283, 15733, 3065, addDespwanTime);
                        npcsEstefa.TempProperties.setProperty(Leo_SPAWNED, true);
                        npcsEstefa.CastSpell(SkillBase.GetSpellByID(CallSpell), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));

                    }

#pragma warning disable CS0618 // Typ oder Element ist veraltet
                    npcsEstefa.WalkTo(31817, 30283, 15733, 50);
#pragma warning restore CS0618 // Typ oder Element ist veraltet
                    m_questPlayer.Out.SendMessage("Wait a moment I summon now Leo!.", eChatType.CT_Broadcast, eChatLoc.CL_SystemWindow);
                    Step = 4;
                    return;
                }
            }
            //Leo
            if (Step == 4 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;


                if (gArgs.Target.Name.IndexOf("Leo") >= 0)
                {
                    m_questPlayer.Out.SendMessage("You collect the head Of Centaurus", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    GiveItem(m_questPlayer, SkinOfLeo);


                    if (npcsEstefa != null && npcsEstefa.TempProperties.getProperty<bool>(Draco_SPAWNED) == false)
                    {
                        SpawnAdd(templateDraco, Util.Random(65, 68), 31817, 30283, 15733, 3065, addDespwanTime);
                        npcsEstefa.TempProperties.setProperty(Draco_SPAWNED, true);
                        npcsEstefa.CastSpell(SkillBase.GetSpellByID(CallSpell), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));

                    }


#pragma warning disable CS0618 // Typ oder Element ist veraltet
                    npcsEstefa.WalkTo(31817, 30283, 15733, 50);
#pragma warning restore CS0618 // Typ oder Element ist veraltet
                    m_questPlayer.Out.SendMessage("Wait a moment I summon now Draco for the final!.", eChatType.CT_Broadcast, eChatLoc.CL_SystemWindow);
                    Step = 5;
                    return;
                }

            }
            if (Step == 5 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;


                if (gArgs.Target.Name.IndexOf("Draco") >= 0)
                {
                    npcsEstefa.CastSpell(SkillBase.GetSpellByID(RitualSpell), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    m_questPlayer.Out.SendMessage("Congratulations, you has sly Draco!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                    if (npcsEstefa != null)
                    {
                        npcsEstefa.TempProperties.removeProperty(Draco_SPAWNED);
                        npcsEstefa.TempProperties.removeProperty(Leo_SPAWNED);
                        npcsEstefa.TempProperties.removeProperty(Centaurus_SPAWNED);
                        npcsEstefa.TempProperties.removeProperty(Scorpius_SPAWNED);
                    }


                    Step = 6;
                    return;
                }

            }
            if (Step == 6 && e == GamePlayerEvent.GiveItem)
            {
                GiveItemEventArgs gArgs1 = (GiveItemEventArgs)args;
                if (gArgs1.Target.Name == npcsEstefa.Name && gArgs1.Item.Id_nb == TailOfSagittarius.Id_nb)
                {

                    RemoveItem(npcsEstefa, m_questPlayer, TailOfSagittarius);

                }
                npcsEstefa.SayTo(player, "Give me the stinger of Scorpius!");
                Step = 7;
                return;
            }

            if (Step == 7 && e == GamePlayerEvent.GiveItem)
            {
                GiveItemEventArgs gArgs1 = (GiveItemEventArgs)args;
                if (gArgs1.Target.Name == npcsEstefa.Name && gArgs1.Item.Id_nb == stingerOfScorpius.Id_nb)
                {

                    RemoveItem(npcsEstefa, m_questPlayer, stingerOfScorpius);

                    npcsEstefa.SayTo(player, "Give me the Head of Centaurus!");

                    Step = 8;
                    return;
                }
            }
            if (Step == 8 && e == GamePlayerEvent.GiveItem)
            {
                GiveItemEventArgs gArgs1 = (GiveItemEventArgs)args;
                if (gArgs1.Target.Name == npcsEstefa.Name && gArgs1.Item.Id_nb == HeadOfCentaurus.Id_nb)
                {

                    RemoveItem(npcsEstefa, m_questPlayer, HeadOfCentaurus);

                    npcsEstefa.SayTo(player, "Give me the Skin of Leo!");

                    Step = 9;
                    return;
                }
            }
            if (Step == 9 && e == GamePlayerEvent.GiveItem)
            {
                GiveItemEventArgs gArgs1 = (GiveItemEventArgs)args;
                if (gArgs1.Target.Name == npcsEstefa.Name && gArgs1.Item.Id_nb == SkinOfLeo.Id_nb)
                {

                    RemoveItem(npcsEstefa, m_questPlayer, SkinOfLeo);

                }


                if (m_questPlayer.HasML10 == false)
                {
                    npcsEstefa.SayTo(player, "Estefa yell, WOW, You are a true Hero, please take this as Reward!");
                    GiveItem(m_questPlayer, ML10Token);
                    m_questPlayer.Out.SendMessage("You has gained Masterlevel 10.", eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
                    m_questPlayer.HasML10 = true;
                    m_questPlayer.SaveIntoDatabase();
                }
                npcsEstefa.Emote(eEmote.Victory);
                FinishQuest();
                return;
            }

        }

        public static bool IsBosesIncombatOrMoving()
        {
            foreach (GameNPC bosses in WorldMgr.GetNPCsFromRegion(91))
            {
                if (bosses != null && bosses.IsAlive && bosses.ObjectState == GameObject.eObjectState.Active)
                {
                    if (bosses.Name == "Sagittarius" && bosses.InCombat)
                    {
                        return true;
                    }
                    if (bosses.Name == "Scorpius" && bosses.InCombat)
                    {
                        return true;
                    }
                    if (bosses.Name == "Centaurus" && bosses.InCombat)
                    {
                        return true;
                    }
                    if (bosses.Name == "Leo" && bosses.InCombat)
                    {
                        return true;
                    }
                    if (bosses.Name == "Draco" && bosses.InCombat)
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        public override void AbortQuest()
        {
            if (IsBosesIncombatOrMoving())
            {
                m_questPlayer.Out.SendMessage("One of the Bosses are currently in combat. You can't remove this quest yet - please try again later.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                return;
            }


            base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...
            npcsEstefa.TempProperties.removeAllProperties();

            RemoveItem(m_questPlayer, TailOfSagittarius, false);
            RemoveItem(m_questPlayer, stingerOfScorpius, false);
            RemoveItem(m_questPlayer, HeadOfCentaurus, false);
            RemoveItem(m_questPlayer, SkinOfLeo, false);
            //kill of sagi will allow reset all spawn to false

           
        }

        public override void FinishQuest()
        {
            npcsEstefa.TempProperties.removeAllProperties();

            if (m_questPlayer.Inventory.IsSlotsFree(1, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
            {

                m_questPlayer.Out.SendSoundEffect(662, 0, 0, 0, 0, 0);
                base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

                //Rewards for All
                m_questPlayer.GainExperience(GameLiving.eXPSource.Quest, 2132899648, true);
                m_questPlayer.AddMoney(Money.GetMoney(0, 0, 50, 72, Util.Random(99)), "You recieve {0} as a reward.");
                m_questPlayer.BountyPointsAndMassage(110);
            }
            else
            {

                m_questPlayer.Out.SendMessage("You do not have enough free space in your inventory!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                m_questPlayer.GainExperience(GameLiving.eXPSource.Quest, 2132899648, true);
                m_questPlayer.AddMoney(Money.GetMoney(0, 0, 50, 72, Util.Random(99)), "You recieve {0} as a reward.");
                m_questPlayer.BountyPointsAndMassage(110);
            }
        }

        #region ML10 NpcTemplate Spawn

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
        protected GameNPC SpawnAdd(int templateID, int level, int x, int y, int Z, ushort heading, int uptime)
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
                    add = new GameNPC(m_addTemplate)
                    {
                        CurrentRegion = m_questPlayer.CurrentRegion,
                        Heading = heading,
                        Realm = 0,
                        X = x,
                        Y = y,
                        Z = Z,
                        CurrentSpeed = 0,
                        Level = (byte)level,
                        RespawnInterval = -1
                    };

                    new SpawnTimer(add, add, uptime * 1000);

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
        protected class SpawnTimer : GameTimer
        {
            private readonly GameNPC m_npc;

            /// <summary>
            /// Constructs a new DespawnTimer.
            /// </summary>
            /// <param name="timerOwner">The owner of this timer.</param>
            /// <param name="npc">The GameNPC to despawn when the time is up.</param>
            /// <param name="delay">The time after which the add is supposed to despawn.</param>
            public SpawnTimer(GameObject timerOwner, GameNPC npc, int delay)
                : base(timerOwner.CurrentRegion.TimeManager)
            {
                if (timerOwner != null)
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

                    m_npc.AddToWorld();
                }
                new DespawnTimer(m_npc, m_npc, addUtimeTime * 1000);
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
                    if (timerOwner != null)
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

            #endregion
        }
    }
}