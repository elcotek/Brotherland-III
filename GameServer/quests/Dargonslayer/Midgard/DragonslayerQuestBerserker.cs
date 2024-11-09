
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

namespace DOL.GS.Quests
{

    /* The first thing we do, is to declare the class we create
    * as Quest. To do this, we derive from the abstract class
    * BaseQuest	  	 
    */
    public class DragonslayerBerserkerQuest : BaseQuest
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

        protected const string questTitle = "DragonslayerBerserkerQuest";

        protected const int minimumLevel = 49;
        protected const int maximumLevel = 50;


        private static GameNPC Jarkkonith = null;

        private static GameNPC AmarasavaDas = null;

        private static ItemTemplate Jarkkonith_Head = null;

        private static ItemTemplate Dragonslayer_Starkaskodd_Jerkin = null;

        private static ItemTemplate Dragonslayer_Starkaskodd_Leggings = null;

        private static ItemTemplate Dragonslayer_Starkaskodd_Armguards = null;

        private static ItemTemplate Dragonslayer_Starkaskodd_Boots = null;

        private static ItemTemplate Dragonslayer_Starkaskodd_Gauntlets = null;

        private static ItemTemplate Dragonslayer_Starkaskodd_Full_Helm = null;


        // Custom Initialization Code Begin

        // Custom Initialization Code End

        /* 
        * Constructor
        */
        public DragonslayerBerserkerQuest()
            : base()
        {
        }

        public DragonslayerBerserkerQuest(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public DragonslayerBerserkerQuest(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
        }

        public DragonslayerBerserkerQuest(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest)
        {
        }
        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            QuestBehaviour a;
            QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerBerserkerQuest));

            GamePlayer player = sender as GamePlayer;

            if (player == null || player.IsDoingQuest(typeof(DragonslayerBerserkerQuest)) == null)
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
                        a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerBerserkerQuest), null);
                        a.AddAction(eActionType.GiveItem, Dragonslayer_Starkaskodd_Jerkin, AmarasavaDas);
                        a.AddAction(eActionType.GiveItem, Dragonslayer_Starkaskodd_Leggings, AmarasavaDas);
                        a.AddAction(eActionType.GiveItem, Dragonslayer_Starkaskodd_Armguards, AmarasavaDas);
                        a.AddAction(eActionType.GiveItem, Dragonslayer_Starkaskodd_Boots, AmarasavaDas);
                        a.AddAction(eActionType.GiveItem, Dragonslayer_Starkaskodd_Gauntlets, AmarasavaDas);
                        a.AddAction(eActionType.GiveItem, Dragonslayer_Starkaskodd_Full_Helm, AmarasavaDas);
                        a.AddAction(eActionType.FinishQuest, typeof(DragonslayerBerserkerQuest), null);
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

            npcs = WorldMgr.GetNPCsByName("Amarasava Das", eRealm.Midgard);
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
                Jarkkonith_Head.Bonus1Type = (int)0;
                Jarkkonith_Head.Bonus2 = 0;
                Jarkkonith_Head.Bonus2Type = (int)0;
                Jarkkonith_Head.Bonus3 = 0;
                Jarkkonith_Head.Bonus3Type = (int)208;
                Jarkkonith_Head.Bonus4 = 0;
                Jarkkonith_Head.Bonus4Type = (int)0;
                Jarkkonith_Head.Bonus5 = 0;
                Jarkkonith_Head.Bonus5Type = (int)0;
                Jarkkonith_Head.Bonus6 = 0;
                Jarkkonith_Head.Bonus6Type = (int)0;
                Jarkkonith_Head.Bonus7 = 0;
                Jarkkonith_Head.Bonus7Type = (int)0;
                Jarkkonith_Head.Bonus8 = 0;
                Jarkkonith_Head.Bonus8Type = (int)0;
                Jarkkonith_Head.Bonus9 = 0;
                Jarkkonith_Head.Bonus9Type = (int)0;
                Jarkkonith_Head.Bonus10 = 0;
                Jarkkonith_Head.Bonus10Type = (int)0;
                Jarkkonith_Head.ExtraBonus = 0;
                Jarkkonith_Head.ExtraBonusType = (int)0;
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
            Dragonslayer_Starkaskodd_Jerkin = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Starkaskodd_Jerkin");
            if (Dragonslayer_Starkaskodd_Jerkin == null)
            {
                Dragonslayer_Starkaskodd_Jerkin = new ItemTemplate();
                Dragonslayer_Starkaskodd_Jerkin.Name = "Dragonslayer Savage Starkaskodd Jerkin";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + Dragonslayer_Starkaskodd_Jerkin.Name + ", creating it ...");
                Dragonslayer_Starkaskodd_Jerkin.Level = 51;
                Dragonslayer_Starkaskodd_Jerkin.Weight = 22;
                Dragonslayer_Starkaskodd_Jerkin.Model = 4041;
                Dragonslayer_Starkaskodd_Jerkin.Object_Type = 34;
                Dragonslayer_Starkaskodd_Jerkin.Item_Type = 25;
                Dragonslayer_Starkaskodd_Jerkin.Id_nb = "Dragonslayer_Starkaskodd_Jerkin";
                Dragonslayer_Starkaskodd_Jerkin.Hand = 0;
                Dragonslayer_Starkaskodd_Jerkin.IsPickable = true;
                Dragonslayer_Starkaskodd_Jerkin.IsDropable = true;
                Dragonslayer_Starkaskodd_Jerkin.IsTradable = true;
                Dragonslayer_Starkaskodd_Jerkin.CanDropAsLoot = true;
                Dragonslayer_Starkaskodd_Jerkin.Color = 0;
                Dragonslayer_Starkaskodd_Jerkin.Bonus = 35; // default bonus				
                Dragonslayer_Starkaskodd_Jerkin.Bonus1 = 22;
                Dragonslayer_Starkaskodd_Jerkin.Bonus1Type = (int)1;
                Dragonslayer_Starkaskodd_Jerkin.Bonus2 = 22;
                Dragonslayer_Starkaskodd_Jerkin.Bonus2Type = (int)2;
                Dragonslayer_Starkaskodd_Jerkin.Bonus3 = 5;
                Dragonslayer_Starkaskodd_Jerkin.Bonus3Type = (int)12;
                Dragonslayer_Starkaskodd_Jerkin.Bonus4 = 5;
                Dragonslayer_Starkaskodd_Jerkin.Bonus4Type = (int)18;
                Dragonslayer_Starkaskodd_Jerkin.Bonus5 = 5;
                Dragonslayer_Starkaskodd_Jerkin.Bonus5Type = (int)15;
                Dragonslayer_Starkaskodd_Jerkin.Bonus6 = 4;
                Dragonslayer_Starkaskodd_Jerkin.Bonus6Type = (int)173;
                Dragonslayer_Starkaskodd_Jerkin.Bonus7 = 4;
                Dragonslayer_Starkaskodd_Jerkin.Bonus7Type = (int)200;
                Dragonslayer_Starkaskodd_Jerkin.Bonus8 = 0;
                Dragonslayer_Starkaskodd_Jerkin.Bonus8Type = (int)0;
                Dragonslayer_Starkaskodd_Jerkin.Bonus9 = 0;
                Dragonslayer_Starkaskodd_Jerkin.Bonus9Type = (int)0;
                Dragonslayer_Starkaskodd_Jerkin.Bonus10 = 0;
                Dragonslayer_Starkaskodd_Jerkin.Bonus10Type = (int)0;
                Dragonslayer_Starkaskodd_Jerkin.ExtraBonus = 0;
                Dragonslayer_Starkaskodd_Jerkin.ExtraBonusType = (int)0;
                Dragonslayer_Starkaskodd_Jerkin.Effect = 0;
                Dragonslayer_Starkaskodd_Jerkin.Emblem = 0;
                Dragonslayer_Starkaskodd_Jerkin.Charges = 0;
                Dragonslayer_Starkaskodd_Jerkin.MaxCharges = 0;
                Dragonslayer_Starkaskodd_Jerkin.SpellID = 0;
                Dragonslayer_Starkaskodd_Jerkin.ProcSpellID = 31001;
                Dragonslayer_Starkaskodd_Jerkin.Type_Damage = 0;
                Dragonslayer_Starkaskodd_Jerkin.Realm = 0;
                Dragonslayer_Starkaskodd_Jerkin.MaxCount = 1;
                Dragonslayer_Starkaskodd_Jerkin.PackSize = 1;
                Dragonslayer_Starkaskodd_Jerkin.Extension = 5;
                Dragonslayer_Starkaskodd_Jerkin.Quality = 100;
                Dragonslayer_Starkaskodd_Jerkin.Condition = 50000;
                Dragonslayer_Starkaskodd_Jerkin.MaxCondition = 50000;
                Dragonslayer_Starkaskodd_Jerkin.Durability = 50000;
                Dragonslayer_Starkaskodd_Jerkin.MaxDurability = 0;
                Dragonslayer_Starkaskodd_Jerkin.PoisonCharges = 0;
                Dragonslayer_Starkaskodd_Jerkin.PoisonMaxCharges = 0;
                Dragonslayer_Starkaskodd_Jerkin.PoisonSpellID = 0;
                Dragonslayer_Starkaskodd_Jerkin.ProcSpellID1 = 40006;
                Dragonslayer_Starkaskodd_Jerkin.SpellID1 = 0;
                Dragonslayer_Starkaskodd_Jerkin.MaxCharges1 = 0;
                Dragonslayer_Starkaskodd_Jerkin.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(Dragonslayer_Starkaskodd_Jerkin);
            }
            Dragonslayer_Starkaskodd_Leggings = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Starkaskodd_Leggings");
            if (Dragonslayer_Starkaskodd_Leggings == null)
            {
                Dragonslayer_Starkaskodd_Leggings = new ItemTemplate();
                Dragonslayer_Starkaskodd_Leggings.Name = "Dragonslayer Starkaskodd Leggings";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + Dragonslayer_Starkaskodd_Leggings.Name + ", creating it ...");
                Dragonslayer_Starkaskodd_Leggings.Level = 51;
                Dragonslayer_Starkaskodd_Leggings.Weight = 22;
                Dragonslayer_Starkaskodd_Leggings.Model = 4042;
                Dragonslayer_Starkaskodd_Leggings.Object_Type = 34;
                Dragonslayer_Starkaskodd_Leggings.Item_Type = 27;
                Dragonslayer_Starkaskodd_Leggings.Id_nb = "Dragonslayer_Starkaskodd_Leggings";
                Dragonslayer_Starkaskodd_Leggings.Hand = 0;
                Dragonslayer_Starkaskodd_Leggings.IsPickable = true;
                Dragonslayer_Starkaskodd_Leggings.IsDropable = true;
                Dragonslayer_Starkaskodd_Leggings.IsTradable = true;
                Dragonslayer_Starkaskodd_Leggings.CanDropAsLoot = true;
                Dragonslayer_Starkaskodd_Leggings.Color = 0;
                Dragonslayer_Starkaskodd_Leggings.Bonus = 35; // default bonus				
                Dragonslayer_Starkaskodd_Leggings.Bonus1 = 22;
                Dragonslayer_Starkaskodd_Leggings.Bonus1Type = (int)3;
                Dragonslayer_Starkaskodd_Leggings.Bonus2 = 5;
                Dragonslayer_Starkaskodd_Leggings.Bonus2Type = (int)12;
                Dragonslayer_Starkaskodd_Leggings.Bonus3 = 5;
                Dragonslayer_Starkaskodd_Leggings.Bonus3Type = (int)18;
                Dragonslayer_Starkaskodd_Leggings.Bonus4 = 40;
                Dragonslayer_Starkaskodd_Leggings.Bonus4Type = (int)10;
                Dragonslayer_Starkaskodd_Leggings.Bonus5 = 5;
                Dragonslayer_Starkaskodd_Leggings.Bonus5Type = (int)15;
                Dragonslayer_Starkaskodd_Leggings.Bonus6 = 5;
                Dragonslayer_Starkaskodd_Leggings.Bonus6Type = (int)203;
                Dragonslayer_Starkaskodd_Leggings.Bonus7 = 1;
                Dragonslayer_Starkaskodd_Leggings.Bonus7Type = (int)155;
                Dragonslayer_Starkaskodd_Leggings.Bonus8 = 0;
                Dragonslayer_Starkaskodd_Leggings.Bonus8Type = (int)0;
                Dragonslayer_Starkaskodd_Leggings.Bonus9 = 0;
                Dragonslayer_Starkaskodd_Leggings.Bonus9Type = (int)0;
                Dragonslayer_Starkaskodd_Leggings.Bonus10 = 0;
                Dragonslayer_Starkaskodd_Leggings.Bonus10Type = (int)0;
                Dragonslayer_Starkaskodd_Leggings.ExtraBonus = 0;
                Dragonslayer_Starkaskodd_Leggings.ExtraBonusType = (int)0;
                Dragonslayer_Starkaskodd_Leggings.Effect = 0;
                Dragonslayer_Starkaskodd_Leggings.Emblem = 0;
                Dragonslayer_Starkaskodd_Leggings.Charges = 0;
                Dragonslayer_Starkaskodd_Leggings.MaxCharges = 0;
                Dragonslayer_Starkaskodd_Leggings.SpellID = 0;
                Dragonslayer_Starkaskodd_Leggings.ProcSpellID = 31001;
                Dragonslayer_Starkaskodd_Leggings.Type_Damage = 0;
                Dragonslayer_Starkaskodd_Leggings.Realm = 0;
                Dragonslayer_Starkaskodd_Leggings.MaxCount = 1;
                Dragonslayer_Starkaskodd_Leggings.PackSize = 1;
                Dragonslayer_Starkaskodd_Leggings.Extension = 5;
                Dragonslayer_Starkaskodd_Leggings.Quality = 100;
                Dragonslayer_Starkaskodd_Leggings.Condition = 50000;
                Dragonslayer_Starkaskodd_Leggings.MaxCondition = 50000;
                Dragonslayer_Starkaskodd_Leggings.Durability = 50000;
                Dragonslayer_Starkaskodd_Leggings.MaxDurability = 0;
                Dragonslayer_Starkaskodd_Leggings.PoisonCharges = 0;
                Dragonslayer_Starkaskodd_Leggings.PoisonMaxCharges = 0;
                Dragonslayer_Starkaskodd_Leggings.PoisonSpellID = 0;
                Dragonslayer_Starkaskodd_Leggings.ProcSpellID1 = 40006;
                Dragonslayer_Starkaskodd_Leggings.SpellID1 = 0;
                Dragonslayer_Starkaskodd_Leggings.MaxCharges1 = 0;
                Dragonslayer_Starkaskodd_Leggings.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(Dragonslayer_Starkaskodd_Leggings);
            }
            Dragonslayer_Starkaskodd_Armguards = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Starkaskodd_Armguards");
            if (Dragonslayer_Starkaskodd_Armguards == null)
            {
                Dragonslayer_Starkaskodd_Armguards = new ItemTemplate();
                Dragonslayer_Starkaskodd_Armguards.Name = "Dragonslayer Starkaskodd Armguards";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + Dragonslayer_Starkaskodd_Armguards.Name + ", creating it ...");
                Dragonslayer_Starkaskodd_Armguards.Level = 51;
                Dragonslayer_Starkaskodd_Armguards.Weight = 22;
                Dragonslayer_Starkaskodd_Armguards.Model = 4043;
                Dragonslayer_Starkaskodd_Armguards.Object_Type = 34;
                Dragonslayer_Starkaskodd_Armguards.Item_Type = 28;
                Dragonslayer_Starkaskodd_Armguards.Id_nb = "Dragonslayer_Starkaskodd_Armguards";
                Dragonslayer_Starkaskodd_Armguards.Hand = 0;
                Dragonslayer_Starkaskodd_Armguards.IsPickable = true;
                Dragonslayer_Starkaskodd_Armguards.IsDropable = true;
                Dragonslayer_Starkaskodd_Armguards.IsTradable = true;
                Dragonslayer_Starkaskodd_Armguards.CanDropAsLoot = true;
                Dragonslayer_Starkaskodd_Armguards.Color = 0;
                Dragonslayer_Starkaskodd_Armguards.Bonus = 35; // default bonus				
                Dragonslayer_Starkaskodd_Armguards.Bonus1 = 22;
                Dragonslayer_Starkaskodd_Armguards.Bonus1Type = (int)1;
                Dragonslayer_Starkaskodd_Armguards.Bonus2 = 5;
                Dragonslayer_Starkaskodd_Armguards.Bonus2Type = (int)12;
                Dragonslayer_Starkaskodd_Armguards.Bonus3 = 5;
                Dragonslayer_Starkaskodd_Armguards.Bonus3Type = (int)18;
                Dragonslayer_Starkaskodd_Armguards.Bonus4 = 40;
                Dragonslayer_Starkaskodd_Armguards.Bonus4Type = (int)10;
                Dragonslayer_Starkaskodd_Armguards.Bonus5 = 5;
                Dragonslayer_Starkaskodd_Armguards.Bonus5Type = (int)15;
                Dragonslayer_Starkaskodd_Armguards.Bonus6 = 5;
                Dragonslayer_Starkaskodd_Armguards.Bonus6Type = (int)201;
                Dragonslayer_Starkaskodd_Armguards.Bonus7 = 1;
                Dragonslayer_Starkaskodd_Armguards.Bonus7Type = (int)155;
                Dragonslayer_Starkaskodd_Armguards.Bonus8 = 0;
                Dragonslayer_Starkaskodd_Armguards.Bonus8Type = (int)0;
                Dragonslayer_Starkaskodd_Armguards.Bonus9 = 0;
                Dragonslayer_Starkaskodd_Armguards.Bonus9Type = (int)0;
                Dragonslayer_Starkaskodd_Armguards.Bonus10 = 0;
                Dragonslayer_Starkaskodd_Armguards.Bonus10Type = (int)0;
                Dragonslayer_Starkaskodd_Armguards.ExtraBonus = 0;
                Dragonslayer_Starkaskodd_Armguards.ExtraBonusType = (int)0;
                Dragonslayer_Starkaskodd_Armguards.Effect = 0;
                Dragonslayer_Starkaskodd_Armguards.Emblem = 0;
                Dragonslayer_Starkaskodd_Armguards.Charges = 0;
                Dragonslayer_Starkaskodd_Armguards.MaxCharges = 0;
                Dragonslayer_Starkaskodd_Armguards.SpellID = 0;
                Dragonslayer_Starkaskodd_Armguards.ProcSpellID = 31001;
                Dragonslayer_Starkaskodd_Armguards.Type_Damage = 0;
                Dragonslayer_Starkaskodd_Armguards.Realm = 0;
                Dragonslayer_Starkaskodd_Armguards.MaxCount = 1;
                Dragonslayer_Starkaskodd_Armguards.PackSize = 1;
                Dragonslayer_Starkaskodd_Armguards.Extension = 5;
                Dragonslayer_Starkaskodd_Armguards.Quality = 100;
                Dragonslayer_Starkaskodd_Armguards.Condition = 50000;
                Dragonslayer_Starkaskodd_Armguards.MaxCondition = 50000;
                Dragonslayer_Starkaskodd_Armguards.Durability = 50000;
                Dragonslayer_Starkaskodd_Armguards.MaxDurability = 0;
                Dragonslayer_Starkaskodd_Armguards.PoisonCharges = 0;
                Dragonslayer_Starkaskodd_Armguards.PoisonMaxCharges = 0;
                Dragonslayer_Starkaskodd_Armguards.PoisonSpellID = 0;
                Dragonslayer_Starkaskodd_Armguards.ProcSpellID1 = 40006;
                Dragonslayer_Starkaskodd_Armguards.SpellID1 = 0;
                Dragonslayer_Starkaskodd_Armguards.MaxCharges1 = 0;
                Dragonslayer_Starkaskodd_Armguards.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(Dragonslayer_Starkaskodd_Armguards);
            }
            Dragonslayer_Starkaskodd_Boots = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Starkaskodd_Boots");
            if (Dragonslayer_Starkaskodd_Boots == null)
            {
                Dragonslayer_Starkaskodd_Boots = new ItemTemplate();
                Dragonslayer_Starkaskodd_Boots.Name = "Dragonslayer Starkaskodd Boots";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + Dragonslayer_Starkaskodd_Boots.Name + ", creating it ...");
                Dragonslayer_Starkaskodd_Boots.Level = 51;
                Dragonslayer_Starkaskodd_Boots.Weight = 22;
                Dragonslayer_Starkaskodd_Boots.Model = 4044;
                Dragonslayer_Starkaskodd_Boots.Object_Type = 34;
                Dragonslayer_Starkaskodd_Boots.Item_Type = 23;
                Dragonslayer_Starkaskodd_Boots.Id_nb = "Dragonslayer_Starkaskodd_Boots";
                Dragonslayer_Starkaskodd_Boots.Hand = 0;
                Dragonslayer_Starkaskodd_Boots.IsPickable = true;
                Dragonslayer_Starkaskodd_Boots.IsDropable = true;
                Dragonslayer_Starkaskodd_Boots.IsTradable = true;
                Dragonslayer_Starkaskodd_Boots.CanDropAsLoot = true;
                Dragonslayer_Starkaskodd_Boots.Color = 0;
                Dragonslayer_Starkaskodd_Boots.Bonus = 35; // default bonus				
                Dragonslayer_Starkaskodd_Boots.Bonus1 = 18;
                Dragonslayer_Starkaskodd_Boots.Bonus1Type = (int)1;
                Dragonslayer_Starkaskodd_Boots.Bonus2 = 18;
                Dragonslayer_Starkaskodd_Boots.Bonus2Type = (int)3;
                Dragonslayer_Starkaskodd_Boots.Bonus3 = 6;
                Dragonslayer_Starkaskodd_Boots.Bonus3Type = (int)13;
                Dragonslayer_Starkaskodd_Boots.Bonus4 = 6;
                Dragonslayer_Starkaskodd_Boots.Bonus4Type = (int)17;
                Dragonslayer_Starkaskodd_Boots.Bonus5 = 6;
                Dragonslayer_Starkaskodd_Boots.Bonus5Type = (int)19;
                Dragonslayer_Starkaskodd_Boots.Bonus6 = 6;
                Dragonslayer_Starkaskodd_Boots.Bonus6Type = (int)194;
                Dragonslayer_Starkaskodd_Boots.Bonus7 = 0;
                Dragonslayer_Starkaskodd_Boots.Bonus7Type = (int)0;
                Dragonslayer_Starkaskodd_Boots.Bonus8 = 0;
                Dragonslayer_Starkaskodd_Boots.Bonus8Type = (int)0;
                Dragonslayer_Starkaskodd_Boots.Bonus9 = 0;
                Dragonslayer_Starkaskodd_Boots.Bonus9Type = (int)0;
                Dragonslayer_Starkaskodd_Boots.Bonus10 = 0;
                Dragonslayer_Starkaskodd_Boots.Bonus10Type = (int)0;
                Dragonslayer_Starkaskodd_Boots.ExtraBonus = 0;
                Dragonslayer_Starkaskodd_Boots.ExtraBonusType = (int)0;
                Dragonslayer_Starkaskodd_Boots.Effect = 0;
                Dragonslayer_Starkaskodd_Boots.Emblem = 0;
                Dragonslayer_Starkaskodd_Boots.Charges = 0;
                Dragonslayer_Starkaskodd_Boots.MaxCharges = 0;
                Dragonslayer_Starkaskodd_Boots.SpellID = 0;
                Dragonslayer_Starkaskodd_Boots.ProcSpellID = 31001;
                Dragonslayer_Starkaskodd_Boots.Type_Damage = 0;
                Dragonslayer_Starkaskodd_Boots.Realm = 0;
                Dragonslayer_Starkaskodd_Boots.MaxCount = 1;
                Dragonslayer_Starkaskodd_Boots.PackSize = 1;
                Dragonslayer_Starkaskodd_Boots.Extension = 5;
                Dragonslayer_Starkaskodd_Boots.Quality = 100;
                Dragonslayer_Starkaskodd_Boots.Condition = 50000;
                Dragonslayer_Starkaskodd_Boots.MaxCondition = 50000;
                Dragonslayer_Starkaskodd_Boots.Durability = 50000;
                Dragonslayer_Starkaskodd_Boots.MaxDurability = 0;
                Dragonslayer_Starkaskodd_Boots.PoisonCharges = 0;
                Dragonslayer_Starkaskodd_Boots.PoisonMaxCharges = 0;
                Dragonslayer_Starkaskodd_Boots.PoisonSpellID = 0;
                Dragonslayer_Starkaskodd_Boots.ProcSpellID1 = 40006;
                Dragonslayer_Starkaskodd_Boots.SpellID1 = 0;
                Dragonslayer_Starkaskodd_Boots.MaxCharges1 = 0;
                Dragonslayer_Starkaskodd_Boots.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(Dragonslayer_Starkaskodd_Boots);
            }
            Dragonslayer_Starkaskodd_Gauntlets = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Starkaskodd_Gauntlets");
            if (Dragonslayer_Starkaskodd_Gauntlets == null)
            {
                Dragonslayer_Starkaskodd_Gauntlets = new ItemTemplate();
                Dragonslayer_Starkaskodd_Gauntlets.Name = "Dragonslayer Starkaskodd Gauntlets";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + Dragonslayer_Starkaskodd_Gauntlets.Name + ", creating it ...");
                Dragonslayer_Starkaskodd_Gauntlets.Level = 51;
                Dragonslayer_Starkaskodd_Gauntlets.Weight = 22;
                Dragonslayer_Starkaskodd_Gauntlets.Model = 4045;
                Dragonslayer_Starkaskodd_Gauntlets.Object_Type = 34;
                Dragonslayer_Starkaskodd_Gauntlets.Item_Type = 22;
                Dragonslayer_Starkaskodd_Gauntlets.Id_nb = "Dragonslayer_Starkaskodd_Gauntlets";
                Dragonslayer_Starkaskodd_Gauntlets.Hand = 0;
                Dragonslayer_Starkaskodd_Gauntlets.IsPickable = true;
                Dragonslayer_Starkaskodd_Gauntlets.IsDropable = true;
                Dragonslayer_Starkaskodd_Gauntlets.IsTradable = true;
                Dragonslayer_Starkaskodd_Gauntlets.CanDropAsLoot = true;
                Dragonslayer_Starkaskodd_Gauntlets.Color = 0;
                Dragonslayer_Starkaskodd_Gauntlets.Bonus = 35; // default bonus				
                Dragonslayer_Starkaskodd_Gauntlets.Bonus1 = 5;
                Dragonslayer_Starkaskodd_Gauntlets.Bonus1Type = (int)12;
                Dragonslayer_Starkaskodd_Gauntlets.Bonus2 = 5;
                Dragonslayer_Starkaskodd_Gauntlets.Bonus2Type = (int)18;
                Dragonslayer_Starkaskodd_Gauntlets.Bonus3 = 45;
                Dragonslayer_Starkaskodd_Gauntlets.Bonus3Type = (int)10;
                Dragonslayer_Starkaskodd_Gauntlets.Bonus4 = 5;
                Dragonslayer_Starkaskodd_Gauntlets.Bonus4Type = (int)15;
                Dragonslayer_Starkaskodd_Gauntlets.Bonus5 = 3;
                Dragonslayer_Starkaskodd_Gauntlets.Bonus5Type = (int)164;
                Dragonslayer_Starkaskodd_Gauntlets.Bonus6 = 45;
                Dragonslayer_Starkaskodd_Gauntlets.Bonus6Type = (int)210;
                Dragonslayer_Starkaskodd_Gauntlets.Bonus7 = 10;
                Dragonslayer_Starkaskodd_Gauntlets.Bonus7Type = (int)148;
                Dragonslayer_Starkaskodd_Gauntlets.Bonus8 = 0;
                Dragonslayer_Starkaskodd_Gauntlets.Bonus8Type = (int)0;
                Dragonslayer_Starkaskodd_Gauntlets.Bonus9 = 0;
                Dragonslayer_Starkaskodd_Gauntlets.Bonus9Type = (int)0;
                Dragonslayer_Starkaskodd_Gauntlets.Bonus10 = 0;
                Dragonslayer_Starkaskodd_Gauntlets.Bonus10Type = (int)0;
                Dragonslayer_Starkaskodd_Gauntlets.ExtraBonus = 0;
                Dragonslayer_Starkaskodd_Gauntlets.ExtraBonusType = (int)0;
                Dragonslayer_Starkaskodd_Gauntlets.Effect = 0;
                Dragonslayer_Starkaskodd_Gauntlets.Emblem = 0;
                Dragonslayer_Starkaskodd_Gauntlets.Charges = 0;
                Dragonslayer_Starkaskodd_Gauntlets.MaxCharges = 0;
                Dragonslayer_Starkaskodd_Gauntlets.SpellID = 0;
                Dragonslayer_Starkaskodd_Gauntlets.ProcSpellID = 31001;
                Dragonslayer_Starkaskodd_Gauntlets.Type_Damage = 0;
                Dragonslayer_Starkaskodd_Gauntlets.Realm = 0;
                Dragonslayer_Starkaskodd_Gauntlets.MaxCount = 1;
                Dragonslayer_Starkaskodd_Gauntlets.PackSize = 1;
                Dragonslayer_Starkaskodd_Gauntlets.Extension = 5;
                Dragonslayer_Starkaskodd_Gauntlets.Quality = 100;
                Dragonslayer_Starkaskodd_Gauntlets.Condition = 50000;
                Dragonslayer_Starkaskodd_Gauntlets.MaxCondition = 50000;
                Dragonslayer_Starkaskodd_Gauntlets.Durability = 50000;
                Dragonslayer_Starkaskodd_Gauntlets.MaxDurability = 0;
                Dragonslayer_Starkaskodd_Gauntlets.PoisonCharges = 0;
                Dragonslayer_Starkaskodd_Gauntlets.PoisonMaxCharges = 0;
                Dragonslayer_Starkaskodd_Gauntlets.PoisonSpellID = 0;
                Dragonslayer_Starkaskodd_Gauntlets.ProcSpellID1 = 40006;
                Dragonslayer_Starkaskodd_Gauntlets.SpellID1 = 0;
                Dragonslayer_Starkaskodd_Gauntlets.MaxCharges1 = 0;
                Dragonslayer_Starkaskodd_Gauntlets.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(Dragonslayer_Starkaskodd_Gauntlets);
            }
            Dragonslayer_Starkaskodd_Full_Helm = GameServer.Database.FindObjectByKey<ItemTemplate>("Dragonslayer_Starkaskodd_Full_Helm");
            if (Dragonslayer_Starkaskodd_Full_Helm == null)
            {
                Dragonslayer_Starkaskodd_Full_Helm = new ItemTemplate();
                Dragonslayer_Starkaskodd_Full_Helm.Name = "Dragonslayer Starkaskodd Full Helm";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + Dragonslayer_Starkaskodd_Full_Helm.Name + ", creating it ...");
                Dragonslayer_Starkaskodd_Full_Helm.Level = 51;
                Dragonslayer_Starkaskodd_Full_Helm.Weight = 22;
                Dragonslayer_Starkaskodd_Full_Helm.Model = 4069;
                Dragonslayer_Starkaskodd_Full_Helm.Object_Type = 34;
                Dragonslayer_Starkaskodd_Full_Helm.Item_Type = 21;
                Dragonslayer_Starkaskodd_Full_Helm.Id_nb = "Dragonslayer_Starkaskodd_Full_Helm";
                Dragonslayer_Starkaskodd_Full_Helm.Hand = 0;
                Dragonslayer_Starkaskodd_Full_Helm.IsPickable = true;
                Dragonslayer_Starkaskodd_Full_Helm.IsDropable = true;
                Dragonslayer_Starkaskodd_Full_Helm.IsTradable = true;
                Dragonslayer_Starkaskodd_Full_Helm.CanDropAsLoot = true;
                Dragonslayer_Starkaskodd_Full_Helm.Color = 0;
                Dragonslayer_Starkaskodd_Full_Helm.Bonus = 35; // default bonus				
                Dragonslayer_Starkaskodd_Full_Helm.Bonus1 = 22;
                Dragonslayer_Starkaskodd_Full_Helm.Bonus1Type = (int)3;
                Dragonslayer_Starkaskodd_Full_Helm.Bonus2 = 6;
                Dragonslayer_Starkaskodd_Full_Helm.Bonus2Type = (int)16;
                Dragonslayer_Starkaskodd_Full_Helm.Bonus3 = 6;
                Dragonslayer_Starkaskodd_Full_Helm.Bonus3Type = (int)11;
                Dragonslayer_Starkaskodd_Full_Helm.Bonus4 = 40;
                Dragonslayer_Starkaskodd_Full_Helm.Bonus4Type = (int)10;
                Dragonslayer_Starkaskodd_Full_Helm.Bonus5 = 6;
                Dragonslayer_Starkaskodd_Full_Helm.Bonus5Type = (int)14;
                Dragonslayer_Starkaskodd_Full_Helm.Bonus6 = 40;
                Dragonslayer_Starkaskodd_Full_Helm.Bonus6Type = (int)210;
                Dragonslayer_Starkaskodd_Full_Helm.Bonus7 = 5;
                Dragonslayer_Starkaskodd_Full_Helm.Bonus7Type = (int)148;
                Dragonslayer_Starkaskodd_Full_Helm.Bonus8 = 0;
                Dragonslayer_Starkaskodd_Full_Helm.Bonus8Type = (int)0;
                Dragonslayer_Starkaskodd_Full_Helm.Bonus9 = 0;
                Dragonslayer_Starkaskodd_Full_Helm.Bonus9Type = (int)0;
                Dragonslayer_Starkaskodd_Full_Helm.Bonus10 = 0;
                Dragonslayer_Starkaskodd_Full_Helm.Bonus10Type = (int)0;
                Dragonslayer_Starkaskodd_Full_Helm.ExtraBonus = 0;
                Dragonslayer_Starkaskodd_Full_Helm.ExtraBonusType = (int)0;
                Dragonslayer_Starkaskodd_Full_Helm.Effect = 0;
                Dragonslayer_Starkaskodd_Full_Helm.Emblem = 0;
                Dragonslayer_Starkaskodd_Full_Helm.Charges = 0;
                Dragonslayer_Starkaskodd_Full_Helm.MaxCharges = 0;
                Dragonslayer_Starkaskodd_Full_Helm.SpellID = 0;
                Dragonslayer_Starkaskodd_Full_Helm.ProcSpellID = 31001;
                Dragonslayer_Starkaskodd_Full_Helm.Type_Damage = 0;
                Dragonslayer_Starkaskodd_Full_Helm.Realm = 0;
                Dragonslayer_Starkaskodd_Full_Helm.MaxCount = 1;
                Dragonslayer_Starkaskodd_Full_Helm.PackSize = 1;
                Dragonslayer_Starkaskodd_Full_Helm.Extension = 0;
                Dragonslayer_Starkaskodd_Full_Helm.Quality = 100;
                Dragonslayer_Starkaskodd_Full_Helm.Condition = 50000;
                Dragonslayer_Starkaskodd_Full_Helm.MaxCondition = 50000;
                Dragonslayer_Starkaskodd_Full_Helm.Durability = 50000;
                Dragonslayer_Starkaskodd_Full_Helm.MaxDurability = 0;
                Dragonslayer_Starkaskodd_Full_Helm.PoisonCharges = 0;
                Dragonslayer_Starkaskodd_Full_Helm.PoisonMaxCharges = 0;
                Dragonslayer_Starkaskodd_Full_Helm.PoisonSpellID = 0;
                Dragonslayer_Starkaskodd_Full_Helm.ProcSpellID1 = 40006;
                Dragonslayer_Starkaskodd_Full_Helm.SpellID1 = 0;
                Dragonslayer_Starkaskodd_Full_Helm.MaxCharges1 = 0;
                Dragonslayer_Starkaskodd_Full_Helm.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(Dragonslayer_Starkaskodd_Full_Helm);
            }


            #endregion

            #region defineAreas

            #endregion

            #region defineQuestParts


            QuestBuilder builder = QuestMgr.getBuilder(typeof(DragonslayerBerserkerQuest));
            QuestBehaviour a;
            a = builder.CreateBehaviour(AmarasavaDas, -1);
            a.AddTrigger(eTriggerType.Interact, null, AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerBerserkerQuest), AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerBerserkerQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "I have lived in this village since I was a young girl. My father was a great dragon hunter you know.", AmarasavaDas);
            a.AddAction(eActionType.Talk, "In my years I have learned to work with all kinds of problems, specially with [Dragons].", AmarasavaDas);
            AddBehaviour(a);

            a = builder.CreateBehaviour(AmarasavaDas, -1);
            a.AddTrigger(eTriggerType.Whisper, "Dragons", AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerBerserkerQuest), AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerBerserkerQuest), null, (eComparator)5);
            a.AddAction(eActionType.Talk, "yes dragons, but there is a problem for me, i can't hunt dragons by self, [you know].", AmarasavaDas);
            AddBehaviour(a);

            a = builder.CreateBehaviour(AmarasavaDas, -1);
            a.AddTrigger(eTriggerType.Whisper, "you know", AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestGivable, typeof(DragonslayerBerserkerQuest), AmarasavaDas);
            a.AddRequirement(eRequirementType.QuestPending, typeof(DragonslayerBerserkerQuest), null, (eComparator)5);
            a.AddAction(eActionType.OfferQuest, typeof(DragonslayerBerserkerQuest), "Helen has offered you the Dragonslayer quest.?Do you accept?");
            AddBehaviour(a);

            a = builder.CreateBehaviour(AmarasavaDas, -1);
            a.AddTrigger(eTriggerType.DeclineQuest, null, typeof(DragonslayerBerserkerQuest));
            a.AddAction(eActionType.Talk, "No problem. See you", AmarasavaDas);
            AddBehaviour(a);

            a = builder.CreateBehaviour(AmarasavaDas, -1);
            a.AddTrigger(eTriggerType.AcceptQuest, null, typeof(DragonslayerBerserkerQuest));
            a.AddAction(eActionType.Talk, "If you bring me the head of Jarkkonith I shall pay you well!", AmarasavaDas);
            a.AddAction(eActionType.GiveQuest, typeof(DragonslayerBerserkerQuest), AmarasavaDas);
            AddBehaviour(a);
                      
           
            #endregion

            // Custom Scriptloaded Code Begin

            // Custom Scriptloaded Code End
            if (AmarasavaDas != null)
            {
                AmarasavaDas.AddQuestToGive(typeof(DragonslayerBerserkerQuest));
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
            AmarasavaDas.RemoveQuestToGive(typeof(DragonslayerBerserkerQuest));
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
            if (player.IsDoingQuest(typeof(DragonslayerBerserkerQuest)) != null)
                return true;

            // Custom Code Begin

            // Custom  Code End


            if (player.Level > maximumLevel || player.Level < minimumLevel)
                return false;

            if (

            player.CharacterClass.ID != (byte)eCharacterClass.Berserker &&
                true)
            {
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
