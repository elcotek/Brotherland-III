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
*Date           : 24 april 2015
*Quest Name     : Toa Quest Broken Tablet (level 45-48)
*Quest Classes  : All
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
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Hibernia
{
	public class BrokenTabletToAHIB   : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "Broken Tablet";
		protected const int minimumLevel = 45;
		protected const int maximumLevel = 50;
        public const int RitualSpell = 14375;
        //NPCs
        private static GameNPC npcsGlycon = null; // Start NPC  ravel to Mesothalassa loc=16.5k, 34.9k and speak to Glycon.  he send you to Alyciana
        private static GameNPC npcsAlyciana = null; //now speak with him in Mesothalassa 12.5k, 32.2k. 
        private static GameNPC MelosBladeWarrior = null; // Mob to kill
        private static GameNPC NaxosMarrowMage = null; // or Mob to kill
        private static GameNPC Khahet = null; // Mob to kill Khahet     Travel to Oceanus Notos loc=42.9k, 49.1k and kill Khahet. Upon his death he will drop half of a Broken Tablet.                  
        private static GameNPC npcsBryseia = null; // Mob to kill Bryseia (45k, 23.2k) in Mesothalassa.
        private static GameNPC npcsMikon = null; // Mob to kill  Mikon (41.7k, 46.8k) in Mesothalassa.

        //Questitems
        private static ItemTemplate VialofNaxosBlood = null; //blood drop Naxos
        private static ItemTemplate VialofMelosBlood = null; //blood drop melos
        private static ItemTemplate Broken_Tablet = null; //Broken Tablet of drop from Khahet
        private static ItemTemplate HeadsOfBryseiaAndMikon = null; // Heads Of Bryseia And Mikon 

        //Rewards
        private static ItemTemplate SkyrosEelSkinPants = null; //Mist Shrouded Boots 
		private static ItemTemplate SkyrosOctopusSkinSleeves = null; //Mist Shrouded Coif 
		private static ItemTemplate SkyrosSpinyUrchinBoots = null; //Mist Shrouded Gloves 
		private static ItemTemplate SkyrosStarfishStuddedVest = null; //Mist Shrouded Hauberk 
		private static ItemTemplate SkyrosLacedEelSkinPants = null; //Mist Shrouded Legs 
		private static ItemTemplate SkyrosSpinyUrchinHauberk = null; //Mist Shrouded Sleeves 
		private static ItemTemplate SkyrosBraidedEelSkinPants = null; //Shadow Shrouded Boots 
		private static ItemTemplate SkyrosOctopusTentacleSleeves = null; //Shadow Shrouded Coif 
		private static ItemTemplate SkyrosSeaUrchinBoots = null; //Shadow Shrouded Gloves 
		private static ItemTemplate SkyrosCrabshellBoots = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate SkyrosSeaUrchinHauberk = null; //Shadow Shrouded Legs 
		private static ItemTemplate SkyrosStarfishWrappedSleeves = null; //Shadow Shrouded Sleeves 
		private static ItemTemplate SkyrosWovenEelSkinPants = null; //Valhalla Touched Boots 
		private static ItemTemplate SkyrosSpikedUrchinBoots = null; //Valhalla Touched Coif 
		    

		// Constructors
		public BrokenTabletToAHIB() : base()
		{
		}

		public BrokenTabletToAHIB(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public BrokenTabletToAHIB(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

        public BrokenTabletToAHIB(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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

            GameNPC[] npcs = WorldMgr.GetNPCsByName("Glycon", eRealm.Hibernia);

			if (npcs.Length == 0)
			{
			    npcsGlycon = new GameNPC();
                npcsGlycon.Model = 33746;
                npcsGlycon.Name = "Glycon";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Glycon , creating it ...");
                npcsGlycon.GuildName = "Broken Tablet Quest";
                npcsGlycon.Realm = eRealm.Hibernia;
                npcsGlycon.CurrentRegionID = 130;
                npcsGlycon.Size = 50;
                npcsGlycon.Level = 46;
                npcsGlycon.X = 344202;
                npcsGlycon.Y = 559207;
                npcsGlycon.Z = 3341;
                npcsGlycon.Heading = 2184;
                npcsGlycon.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
                    npcsGlycon.SaveIntoDatabase();
				}

			}
			else
            {
                npcsGlycon = npcs[0];
        }
            npcs = WorldMgr.GetNPCsByName("Alyciana", eRealm.Hibernia);

            if (npcs.Length == 0)
            {
                npcsAlyciana = new GameNPC();
                npcsAlyciana.Model = 33749;
                npcsAlyciana.Name = "Alyciana";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Alyciana , creating it ...");
                npcsAlyciana.GuildName = "part of Broken Tablet Quest";
                npcsAlyciana.Realm = eRealm.Hibernia;
                npcsAlyciana.CurrentRegionID = 130;
                npcsAlyciana.Size = 50;
                npcsAlyciana.Level = 44;
                npcsAlyciana.X = 340120;
                npcsAlyciana.Y = 556523;
                npcsAlyciana.Z = 3471;
                npcsAlyciana.Heading = 3800;
                npcsAlyciana.AddToWorld();
                if (SAVE_INTO_DATABASE)
                {
                    npcsAlyciana.SaveIntoDatabase();
                }

            }
            else
            {
                npcsAlyciana = npcs[0];
            }


            npcs = WorldMgr.GetNPCsByName("Khahet", eRealm.None);

            if (npcs.Length == 0)
            {
                Khahet = new GameNPC();
                Khahet.Model = 33744;
                Khahet.Name = "Khahet";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Khahet, creating it ...");
                Khahet.GuildName = "";
                Khahet.Realm = eRealm.None;
                Khahet.CurrentRegionID = 130;
                Khahet.Size = 50;
                Khahet.Level = 65;
                Khahet.X = 368414;
                Khahet.Y = 640119;
                Khahet.Z = 6305;
                Khahet.Heading = 3478;
                Khahet.RoamingRange = 600;
                Khahet.AddToWorld();
                if (SAVE_INTO_DATABASE)
                {
                    Khahet.SaveIntoDatabase();
                }

            }
            else
            {
                Khahet = npcs[0];
            }



            npcs = WorldMgr.GetNPCsByName("MelosBladeWarrior", eRealm.None);

            if (npcs.Length == 0)
            {
                MelosBladeWarrior = new GameNPC();
                MelosBladeWarrior.Model = 33744;
                MelosBladeWarrior.Name = "Melos blade warrior";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Melos Blade Warrior , creating it ...");
                MelosBladeWarrior.GuildName = "";
                MelosBladeWarrior.Realm = eRealm.None;
                MelosBladeWarrior.CurrentRegionID = 130;
                MelosBladeWarrior.Size = 49;
                MelosBladeWarrior.Level = 50;
                MelosBladeWarrior.X = 367073;
                MelosBladeWarrior.Y = 568022;
                MelosBladeWarrior.Z = 5501;
                MelosBladeWarrior.Heading = 3353;
                MelosBladeWarrior.RoamingRange = 600;
                MelosBladeWarrior.AddToWorld();
                if (SAVE_INTO_DATABASE)
                {
                    MelosBladeWarrior.SaveIntoDatabase();
                }

            }
            else
            {
                MelosBladeWarrior = npcs[0];
            }




            npcs = WorldMgr.GetNPCsByName("NaxosMarrowMage", eRealm.None);

            if (npcs.Length == 0)
            {
                NaxosMarrowMage = new GameNPC();
                NaxosMarrowMage.Model = 33749;
                NaxosMarrowMage.Name = "Naxos marrow mage";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Naxos marrow mage , creating it ...");
                NaxosMarrowMage.GuildName = "";
                NaxosMarrowMage.Realm = eRealm.None;
                NaxosMarrowMage.CurrentRegionID = 130;
                NaxosMarrowMage.Size = 52;
                NaxosMarrowMage.Level = 50;
                NaxosMarrowMage.X = 362668;
                NaxosMarrowMage.Y = 543798;
                NaxosMarrowMage.Z = 3208;
                NaxosMarrowMage.Heading = 3801;
                NaxosMarrowMage.RoamingRange = 600;
                NaxosMarrowMage.AddToWorld();
                if (SAVE_INTO_DATABASE)
                {
                    NaxosMarrowMage.SaveIntoDatabase();
                }

            }
            else
            {
                NaxosMarrowMage = npcs[0];
            }


            
            npcs = WorldMgr.GetNPCsByName("Bryseia", eRealm.None);

            if (npcs.Length == 0)
            {
                npcsBryseia = new GameNPC();
                npcsBryseia.Model = 33744;
                npcsBryseia.Name = "Bryseia";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Bryseia, creating it ...");
                npcsBryseia.GuildName = "";
                npcsBryseia.Realm = eRealm.None;
                npcsBryseia.CurrentRegionID = 130;
                npcsBryseia.Size = 54;
                npcsBryseia.Level = 49;
                npcsBryseia.X = 372397;
                npcsBryseia.Y = 550406;
                npcsBryseia.Z = 3750;
                npcsBryseia.Heading = 1814;
                npcsBryseia.RoamingRange = 600;
                npcsBryseia.AddToWorld();
                if (SAVE_INTO_DATABASE)
                {
                    npcsBryseia.SaveIntoDatabase();
                }

            }
            else
            {
                npcsBryseia = npcs[0];
            }


  npcs = WorldMgr.GetNPCsByName("Mikon", eRealm.None);

  if (npcs.Length == 0)
  {
      npcsMikon = new GameNPC();
      npcsMikon.Model = 33744;
      npcsMikon.Name = "Mikon";
      if (log.IsWarnEnabled)
          log.Warn("Could not find Mikon, creating it ...");
      npcsMikon.GuildName = "";
      npcsMikon.Realm = eRealm.None;
      npcsMikon.CurrentRegionID = 130;
      npcsMikon.Size = 54;
      npcsMikon.Level = 49;
      npcsMikon.X = 369489;
      npcsMikon.Y = 571546;
      npcsMikon.Z = 5640;
      npcsMikon.Heading = 2082;
      npcsMikon.RoamingRange = 600;
      npcsMikon.AddToWorld();
      if (SAVE_INTO_DATABASE)
      {
          npcsMikon.SaveIntoDatabase();
      }

  }
  else
  {
      npcsMikon = npcs[0];
  }


			// end npc

			#endregion

			#region Item Declarations

			VialofNaxosBlood = GameServer.Database.FindObjectByKey<ItemTemplate>("vial_of_Naxos_blood");
            if (VialofNaxosBlood == null)
			{
                VialofNaxosBlood = new ItemTemplate();
                VialofNaxosBlood.Id_nb = "vial_of_Naxos_blood";
                VialofNaxosBlood.Name = "vial of Naxos blood";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Naxos marrow mage , creating it ...");
                VialofNaxosBlood.Level = 0;
                VialofNaxosBlood.Item_Type = 41;
                VialofNaxosBlood.Model = 529;
                VialofNaxosBlood.IsDropable = false;
                VialofNaxosBlood.IsPickable = true;
                VialofNaxosBlood.DPS_AF = 0;
                VialofNaxosBlood.SPD_ABS = 0;
                VialofNaxosBlood.Object_Type = 0;
                VialofNaxosBlood.Hand = 0;
                VialofNaxosBlood.Type_Damage = 0;
                VialofNaxosBlood.Quality = 49;
                VialofNaxosBlood.Weight = 1;
				if (SAVE_INTO_DATABASE)
				{
                    GameServer.Database.AddObject(VialofNaxosBlood);
				}

			}

            VialofMelosBlood = GameServer.Database.FindObjectByKey<ItemTemplate>("vial_of_melos_blood");
            if (VialofMelosBlood == null)
            {
                VialofMelosBlood = new ItemTemplate();
                VialofMelosBlood.Id_nb = "vial_of_melos_blood";
                VialofMelosBlood.Name = "vial of melos blood";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find melos blood , creating it ...");
                VialofMelosBlood.Level = 0;
                VialofMelosBlood.Item_Type = 41;
                VialofMelosBlood.Model = 529;
                VialofMelosBlood.IsDropable = false;
                VialofMelosBlood.IsPickable = true;
                VialofMelosBlood.DPS_AF = 0;
                VialofMelosBlood.SPD_ABS = 0;
                VialofMelosBlood.Object_Type = 0;
                VialofMelosBlood.Hand = 0;
                VialofMelosBlood.Type_Damage = 0;
                VialofMelosBlood.Quality = 49;
                VialofMelosBlood.Weight = 1;
                if (SAVE_INTO_DATABASE)
                {
                    GameServer.Database.AddObject(VialofMelosBlood);
                }

            }
            HeadsOfBryseiaAndMikon = GameServer.Database.FindObjectByKey<ItemTemplate>("Heads_Of_Bryseia_And_Mikon");
            if (HeadsOfBryseiaAndMikon == null)
            {
                HeadsOfBryseiaAndMikon = new ItemTemplate();
                HeadsOfBryseiaAndMikon.Id_nb = "Heads_Of_Bryseia_And_Mikon";
                HeadsOfBryseiaAndMikon.Name = "Heads of Bryseia and Mikon";
                if (log.IsWarnEnabled)
                    log.Warn("Could not findHeads of Bryseia and Mikon, creating it ...");
                HeadsOfBryseiaAndMikon.Level = 0;
                HeadsOfBryseiaAndMikon.Item_Type = 41;
                HeadsOfBryseiaAndMikon.Model = 488;
                HeadsOfBryseiaAndMikon.IsDropable = false;
                HeadsOfBryseiaAndMikon.IsPickable = true;
                HeadsOfBryseiaAndMikon.DPS_AF = 0;
                HeadsOfBryseiaAndMikon.SPD_ABS = 0;
                HeadsOfBryseiaAndMikon.Object_Type = 0;
                HeadsOfBryseiaAndMikon.Hand = 0;
                HeadsOfBryseiaAndMikon.Type_Damage = 0;
                HeadsOfBryseiaAndMikon.Quality = 49;
                HeadsOfBryseiaAndMikon.Weight = 1;
                if (SAVE_INTO_DATABASE)
                {
                    GameServer.Database.AddObject(HeadsOfBryseiaAndMikon);
                }

            }



            Broken_Tablet = GameServer.Database.FindObjectByKey<ItemTemplate>("broken_tablet");
            if (Broken_Tablet == null)
            {
                Broken_Tablet = new ItemTemplate();
                Broken_Tablet.Id_nb = "broken_tablet";
                Broken_Tablet.Name = "half of a broken tablet";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find broken tablet , creating it ...");
                Broken_Tablet.Level = 0;
                Broken_Tablet.Item_Type = 41;
                Broken_Tablet.Model = 505;
                Broken_Tablet.IsDropable = false;
                Broken_Tablet.IsPickable = true;
                Broken_Tablet.DPS_AF = 0;
                Broken_Tablet.SPD_ABS = 0;
                Broken_Tablet.Object_Type = 0;
                Broken_Tablet.Hand = 0;
                Broken_Tablet.Type_Damage = 0;
                Broken_Tablet.Quality = 0;
                Broken_Tablet.Weight = 12;
                if (SAVE_INTO_DATABASE)
                {
                    GameServer.Database.AddObject(Broken_Tablet);
                }

            }
            // start reward items
			
            SkyrosEelSkinPants = GameServer.Database.FindObjectByKey<ItemTemplate>("Skyros_Eel_Skin_Pants"); //midgard
			
            SkyrosOctopusSkinSleeves = GameServer.Database.FindObjectByKey<ItemTemplate>("Skyros_Octopus_Skin_Sleeves");
			
            SkyrosSpinyUrchinBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("Skyros_Spiny_Urchin_Boots");
		
            SkyrosStarfishStuddedVest = GameServer.Database.FindObjectByKey<ItemTemplate>("Skyros_Starfish_Studded_Vest");
			
            SkyrosLacedEelSkinPants = GameServer.Database.FindObjectByKey<ItemTemplate>("Skyros_Laced_Eel_Skin_Pants");
			 
            SkyrosSpinyUrchinHauberk = GameServer.Database.FindObjectByKey<ItemTemplate>("Skyros_Spiny_Urchin_Hauberk");
		
            SkyrosBraidedEelSkinPants = GameServer.Database.FindObjectByKey<ItemTemplate>("Skyros_Braided_Eel_Skin_Pants");
			
            SkyrosOctopusTentacleSleeves = GameServer.Database.FindObjectByKey<ItemTemplate>("Skyros_Octopus_Tentacle_Sleeves");
			
            SkyrosSeaUrchinBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("Skyros_Sea_Urchin_Boots");
	
            SkyrosCrabshellBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("Skyros_Crabshell_Boots");

            SkyrosSeaUrchinHauberk = GameServer.Database.FindObjectByKey<ItemTemplate>("Skyros_Sea_Urchin_Hauberk");
	
            SkyrosStarfishWrappedSleeves = GameServer.Database.FindObjectByKey<ItemTemplate>("Skyros_Starfish_Wrapped_Sleeves");

            SkyrosWovenEelSkinPants = GameServer.Database.FindObjectByKey<ItemTemplate>("Skyros_Woven_Eel_Skin_Pants");

            SkyrosSpikedUrchinBoots = GameServer.Database.FindObjectByKey<ItemTemplate>("Skyros_Spiked_Urchin_Boots");
			


            //Item Descriptions End

			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.AddHandler(npcsGlycon, GameObjectEvent.Interact, new DOLEventHandler(TalkToGlycon));
            GameEventMgr.AddHandler(npcsGlycon, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToGlycon));
            GameEventMgr.AddHandler(npcsAlyciana, GameObjectEvent.Interact, new DOLEventHandler(TalkToAlyciana));
            GameEventMgr.AddHandler(npcsAlyciana, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAlyciana));

            

			/* Now we bring to Ainrebh the possibility to give this quest to players */
            npcsGlycon.AddQuestToGive(typeof(BrokenTabletToAHIB));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
            if (npcsGlycon == null)
				return;
			// remove handlers
            GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.RemoveHandler(npcsGlycon, GameObjectEvent.Interact, new DOLEventHandler(TalkToGlycon));
            GameEventMgr.RemoveHandler(npcsGlycon, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToGlycon));
            GameEventMgr.RemoveHandler(npcsAlyciana, GameObjectEvent.Interact, new DOLEventHandler(TalkToAlyciana));
            GameEventMgr.RemoveHandler(npcsAlyciana, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAlyciana));
                    
			/* Now we remove to Ainrebh the possibility to give this quest to players */
            npcsGlycon.RemoveQuestToGive(typeof(BrokenTabletToAHIB));
		}

        protected static void TalkToGlycon(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (npcsGlycon.CanGiveQuest(typeof(BrokenTabletToAHIB), player) <= 0)
                return;

            //We also check if the player is already doing the quest
            BrokenTabletToAHIB quest = player.IsDoingQuest(typeof(BrokenTabletToAHIB)) as BrokenTabletToAHIB;
            npcsGlycon.TurnTo(player);
            if (e == GameObjectEvent.Interact)
            {
                if (quest != null)
                {
                    npcsGlycon.SayTo(player, "Check your Journal for instructions!");
                }
                else
                {
                    npcsGlycon.SayTo(player, "hello my friend please speak with my Sister [Alyciana] in Mesothalassa loc 12.5k, 32.2k.");
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
                        case "Alyciana":
                            player.Out.SendQuestSubscribeCommand(npcsGlycon, QuestMgr.GetIDForQuestType(typeof(BrokenTabletToAHIB)), "Will you help Alyciana [Quest Broken Tablet of Atlantis level 45-48]?");
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

        protected static void TalkToAlyciana(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (player.IsDoingQuest(typeof(BrokenTabletToAHIB)) == null)
                return;

            /*
             *
            if (Alyciana.CanGiveQuest(typeof(BrokenTablet), player) <= 0)
                return;
            */
            //We also check if the player is already doing the quest
            BrokenTabletToAHIB quest = player.IsDoingQuest(typeof(BrokenTabletToAHIB)) as BrokenTabletToAHIB;

            if (quest == null)
                return;

            if (e == GameObjectEvent.Interact)
            {
                if (quest.Step == 1)
                {
                    npcsAlyciana.SayTo(player, "Hello, have my brother [Glycon] you send ?");
                }
            }
            // The player whispered to the NPC
            else if (e == GameLivingEvent.WhisperReceive)
            {
                WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;
                //Check player is already doing quest
                if (quest != null)
                {
                    switch (wArgs.Text)
                    {
                        case "Glycon":
                            npcsAlyciana.SayTo(player, "Travel to the Naxos area and kill one Naxos Marrow Mage and then travel to the melos Area and kill one Melos Blade Warrior." + "after all bring me here blood");
                            //npcsAlyciana.SayTo(player, "Travel to Oceanus Notos loc=42.9k, 49.1k and kill Khahet. Upon his death he will drop half of a Broken Tablet." + "after all take it and come back to me");
                            //player.Out.SendQuestSubscribeCommand(npcsAlyciana, QuestMgr.GetIDForQuestType(typeof(BrokenTablet)), "Travel to Oceanus Notos loc=42.9k, 49.1k and kill Khahet. Upon his death he will drop half of a Broken Tablet." + "after all take the Tablet and come back to me");
                            quest.Step = 2;
                            break;
                    }
                }
                else
                {
                    switch (wArgs.Text)
                    {
                        case "no":
                            player.Out.SendCustomDialog("don't make me angry, do your job!", new CustomDialogResponse(CheckPlayerAbortQuest));
                            break;
                    }
                }
            }
        }
          
		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
            if (player.IsDoingQuest(typeof(BrokenTabletToAHIB)) != null)
				return true;

            /*
			if (player.CharacterClass.ID != (byte) eCharacterClass.Hero &&
				player.CharacterClass.ID != (byte) eCharacterClass.Ranger &&
                player.CharacterClass.ID != (byte) eCharacterClass.MaulerHib &&
                player.CharacterClass.ID != (byte) eCharacterClass.Warden &&
				player.CharacterClass.ID != (byte) eCharacterClass.Eldritch)
				return false;
            */
			// This checks below are only performed is player isn't doing quest already

			//if (player.HasFinishedQuest(typeof(Academy_47)) == 0) return false;

			//if (!CheckPartAccessible(player,typeof(CityOfCamelot)))
			//	return false;

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
            BrokenTabletToAHIB quest = player.IsDoingQuest(typeof(BrokenTabletToAHIB)) as BrokenTabletToAHIB;

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

            if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(BrokenTabletToAHIB)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

        private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
        {
            if (npcsGlycon.CanGiveQuest(typeof(BrokenTabletToAHIB), player) <= 0)
                return;


            if (player.IsDoingQuest(typeof(BrokenTabletToAHIB)) != null)
                return;

            if (response == 0x00)
            {
                player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            }
            else
            {
                /*
                //Check if we can add the quest!
                if (!npcsGlycon.GiveQuest(typeof(BrokenTabletToAHIB), player, 1))
                    return;
                player.Out.SendMessage("first we need poison to begin with the ritual, Search for a Naxos marrow mage and a Melos blade warrior and bring me here bood!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
               */
                //Check if we can add the quest!
                if (!npcsGlycon.GiveQuest(typeof(BrokenTabletToAHIB), player, 1))
                    return;
                //step 2
                player.Out.SendMessage("i have a important task for you my friend, please speak with my Sister Alyciana in Mesothalassa at loc 12.5k, 32.2k !", eChatType.CT_System, eChatLoc.CL_PopupWindow);


                if (npcsAlyciana.CanGiveQuest(typeof(BrokenTabletToAHIB), player) <= 0)
                  return;


                if (player.IsDoingQuest(typeof(BrokenTabletToAHIB)) == null)
                    return;

                if (response == 0x00)
                {
                    player.Out.SendMessage("Our God forgives your laziness, just look out for stray lightning bolts.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }
                else
                {
                    //Check if we can add the quest!
                    if (!npcsAlyciana.GiveQuest(typeof(BrokenTabletToAHIB), player, 2))
                        return;

                    player.Out.SendMessage("Take blood from a  Naxos marrow mage and then from a Melos blade warrior, all can be find in the Naxos and melos areas.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    //player.Out.SendMessage("Travel to Oceanus Notos loc=42.9k, 49.1k and kill Khahet. Upon his death he will drop half of a Broken Tablet.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                }
            }
        }

		//Set quest name
		public override string Name
		{
            get { return "the broken Tablet (Level 45-50 ToA Quest)"; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
                        return "[Step #1] Seek Alyciana in Mesothalassa 12.5k, 32.2k and speak with her, i think he need your help!";
                   
                    case 2:
                        return "[Step #2] collect blood from a  Naxos marrow mage.";

                    case 3:
                        return "[Step #3] now collect blood from a  Melos blade warrior.";

                    case 4:
                        return "[Step #4] Return to Alyciana and hand her the viol of blood from Naxos and melos " + "  first hand her the Naxos blood!."; 
                   
                    case 5:
                        return "[Step #5] now hand Alyciana the viol of Melos blood!."; 
                   
                    case 6:
                        return "[Step #6] now Travel to Oceanus Notos loc=42.9k, 49.1k and kill Khahet.";

                    case 7:
                        return "[Step #7] Return to Alyciana and give her the half Tablet!";

                    case 8:
                        return "[Step #8]  for now we are near the final stage, you must now find and sly Bryseia in Mesothalassa at (45k, 23.2k).";

                        case 9:
                        return "[Step #9]  at last sly Mikon in Mesothalassa at (41.7k, 46.8k) then you have the heads of all them.";

                        case 10:
                        return "[Step #10]  travel back to Alyciana give her the Heads and take your reward.";

                    
				}
				return base.Description;
			}
		}

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = sender as GamePlayer;

            if (player == null || player.IsDoingQuest(typeof(BrokenTabletToAHIB)) == null)
                return;

            if (Step == 2 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;

                if (gArgs.Target.Name == NaxosMarrowMage.Name)
                {
                    m_questPlayer.Out.SendMessage("You collect the bood from the Naxos", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    GiveItem(m_questPlayer, VialofNaxosBlood);
                    Step = 3;
                    return;
                }

            }
            if (Step == 3 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;

                if (gArgs.Target.Name == MelosBladeWarrior.Name)
                {
                    m_questPlayer.Out.SendMessage("You collect the blood from the Melos", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    GiveItem(m_questPlayer, VialofMelosBlood);
                    Step = 4;
                    return;
                }

            }
            if (Step == 4 && e == GamePlayerEvent.GiveItem)
            {
                GiveItemEventArgs gArgs1 = (GiveItemEventArgs)args;
                if (gArgs1.Target.Name == npcsAlyciana.Name && gArgs1.Item.Id_nb == VialofNaxosBlood.Id_nb)
                {

                    RemoveItem(npcsAlyciana, m_questPlayer, VialofNaxosBlood);

                    npcsAlyciana.SayTo(player, "give me now the Melos vial!");
                    Step = 5;
                    return;
                }
            }
            if (Step == 5 && e == GamePlayerEvent.GiveItem)
            {
                GiveItemEventArgs gArgs2 = (GiveItemEventArgs)args;
                if (gArgs2.Target.Name == npcsAlyciana.Name && gArgs2.Item.Id_nb == VialofMelosBlood.Id_nb)
                {

                    RemoveItem(npcsAlyciana, m_questPlayer, VialofMelosBlood);

                    npcsAlyciana.SayTo(player, "oki now we have all we need so i begin the ritual...!");

                    npcsAlyciana.CastSpell(SkillBase.GetSpellByID(RitualSpell), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    Step = 6;
                    return;
                }
            }
            if (Step == 6 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;

                if (gArgs.Target.Name == Khahet.Name)
                {
                    m_questPlayer.Out.SendMessage("You sly Kahet!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    GiveItem(m_questPlayer, Broken_Tablet);
                    Step = 7;
                    return;
                }

            }
            if (Step == 7 && e == GamePlayerEvent.GiveItem)
            {
                GiveItemEventArgs gArgs3 = (GiveItemEventArgs)args;
                if (gArgs3.Target.Name == npcsAlyciana.Name && gArgs3.Item.Id_nb == Broken_Tablet.Id_nb)
                {
                    //player.Out.SendMessage("Travel to Oceanus Notos loc=42.9k, 49.1k and kill Khahet. Upon his death he will drop half of a Broken Tablet.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    RemoveItem(npcsAlyciana, m_questPlayer, Broken_Tablet);
                    npcsAlyciana.SayTo(player, "after all, the tablet is back, i can repear this, but we have at last two more steps todo, we need the Heads of Bryseia and Mikon so we can living in freedom!");
                    //npcsAlyciana.SayTo(player, "after all, the tablet is back, i can repear this, but you will be my Hero, I will never forget you! thank you for all my friend!!");
                    Step = 8;
                    return;
                }
            }
            if (Step == 8 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs4 = (EnemyKilledEventArgs)args;

                if (gArgs4.Target.Name == npcsBryseia.Name)
                {
                    m_questPlayer.Out.SendMessage("You sly Bryseia and take the her Heed!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    Step = 9;
                    return;
                }

            }
            if (Step == 9 && e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs5 = (EnemyKilledEventArgs)args;

                
                if (gArgs5.Target.Name == npcsMikon.Name)
                {
                    m_questPlayer.Out.SendMessage("You sly Mikon and have now Heads of them all!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    GiveItem(m_questPlayer, HeadsOfBryseiaAndMikon);
                    Step = 10;
                    return;
                }

            }
           
             if (Step == 10 && e == GamePlayerEvent.GiveItem)
            {
                GiveItemEventArgs gArgs3 = (GiveItemEventArgs)args;
                if (gArgs3.Target.Name == npcsAlyciana.Name && gArgs3.Item.Id_nb == HeadsOfBryseiaAndMikon.Id_nb)
                {
                    npcsAlyciana.SayTo(player, "after all, my brother and i can be in freedom!... you will be my Hero, we will never forget you! thank you for all you help my friend!!");
                    npcsAlyciana.SayTo(player, "here please take this little Reward, we have not mutch, but it is as memory of all your glory actions you have do for me and my brother!");
                    FinishQuest();
                    return;
                }

            }
        }

              

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

            RemoveItem(m_questPlayer, Broken_Tablet, false);
            RemoveItem(m_questPlayer, VialofMelosBlood, false);
            RemoveItem(m_questPlayer, VialofNaxosBlood, false);
		}

		public override void FinishQuest()
		{
			if (m_questPlayer.Inventory.IsSlotsFree(6, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
			{
                RemoveItem(npcsAlyciana, m_questPlayer, HeadsOfBryseiaAndMikon);
				base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...


                //Rewards for Midgard
                if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Bonedancer ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Runemaster ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Spiritmaster ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Warlock)
				{
                    //Bonedancer, Runemaster, Spiritmaster, Warlock
                    GiveItem(m_questPlayer, SkyrosEelSkinPants);


                }
				else  if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.MaulerMid ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Shadowblade)
				{

                    //Mauler (Mid), Shadowblade
                    GiveItem(m_questPlayer, SkyrosOctopusTentacleSleeves);

                }  
                else  if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Skald ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Thane ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Valkyrie ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Warrior)
				{
					//Skald, Thane, Valkyrie, Warrior
					GiveItem(m_questPlayer, SkyrosSeaUrchinBoots);

				} 
                else  if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Healer ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Shaman)
				{
					//Healer, Shaman
					GiveItem(m_questPlayer, SkyrosSeaUrchinHauberk);
				} 



                //Rewards for Albion
                else  if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Friar ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Infiltrator ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.MaulerAlb)
				{
					//Friar, Infiltrator, Mauler (Alb)
                    GiveItem(m_questPlayer, SkyrosOctopusSkinSleeves);
				} 
                 else  if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Cabalist ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Necromancer ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Sorcerer ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Theurgist ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Wizard)
				{
					
                    //Cabalist, Necromancer, Sorcerer, Theurgist, Wizard
					GiveItem(m_questPlayer, SkyrosBraidedEelSkinPants);

				} 
                else  if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Scout)
				{
					//Scout
					GiveItem(m_questPlayer, SkyrosStarfishStuddedVest);
                   
				} 
                else  if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Armsman ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Paladin)
				{
					//Armsman, Paladin
					GiveItem(m_questPlayer, SkyrosCrabshellBoots);

				} 
                 else  if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Mercenary ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Minstrel ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Reaver)
				{
					//Mercenary, Minstrel, Reaver
                    GiveItem(m_questPlayer, SkyrosSpikedUrchinBoots);
				} 
                 else  if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Heretic)
				{
					//Heretic
					GiveItem(m_questPlayer, SkyrosWovenEelSkinPants);
				} 
                
                 //Rewards for Hibernia
                 else  if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Animist ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Bainshee ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Eldritch ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Enchanter ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Mentalist)
				{
				//Animist, Bainshee, Eldritch, Enchanter, Mentalist
					GiveItem(m_questPlayer, SkyrosLacedEelSkinPants);
	          	} 
                else  if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Champion ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Hero ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Warden)
				{
					 //Champion, Hero, Warden
					GiveItem(m_questPlayer, SkyrosSpinyUrchinBoots);
				} 
                 else  if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Bard ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Blademaster ||
                    m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Ranger)
				{
				    //Bard, Blademaster, Ranger
					GiveItem(m_questPlayer, SkyrosStarfishWrappedSleeves);
				}
                else if (m_questPlayer.CharacterClass.ID == (byte)eCharacterClass.Druid)
				{
	                //Druid
					GiveItem(m_questPlayer, SkyrosSpinyUrchinHauberk);
				} 
            
               
				m_questPlayer.GainExperience(GameLiving.eXPSource.Quest, 2132899648, true);
				m_questPlayer.AddMoney(Money.GetMoney(0,0,54,22,Util.Random(50)), "You recieve {0} as a reward.");		
			}
			else
			{
				m_questPlayer.Out.SendMessage("You do not have enough free space in your inventory!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}
		}

		#region Allakhazam Broken Tablet Source

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
