/*
 * Originally written by ?
 * updated by BluRaven on 10-25-08.
 * Added the respec and granting free champion levels.  Just comment those out if you don't want them to be available.
 * Also added code from the king NPC so the player dosn't have to use both NPC's, just this one.
 * 
 */

using DOL.Database;
using DOL.GS.PacketHandler;
using System;

namespace DOL.GS.Scripts
{
    public class ChampionNPC : GameNPC
    {
        public ChampionNPC()
            : base()
        {
        }
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;
            TurnTo(player, 5000);

            //check if player needs to be promoted
            CheckPromoteChampion(player);

            if (player.BountyPoints > 499)
            {
            }
            else
            {
                player.Out.SendMessage("You need 500 bounty points to use this service!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                return false;
            }
            if (!player.Champion && player.Level == 50)
            {
                player.Out.SendMessage("Would you like to embrace in the life of [champions]?.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
            }
            else if (player.Champion)
            {
                switch (player.Realm)
                {
                    case eRealm.Albion:
                        player.Out.SendMessage("Which line would you like to train?\n[Acolyte], [Elementalist], [Disciple], or [Mage]?", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        break;
                    case eRealm.Hibernia:
                        player.Out.SendMessage("Which line would you like to train?\n[Way of Nature], [Way of Magic]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        break;
                    case eRealm.Midgard:
                        player.Out.SendMessage("Which line would you like to train?\n[Seer] or [Mystic]?", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        break;
                }
                player.Out.SendMessage("Sorry, healing and magic are all that I can offer to every realm, stealth and melee training is not available at this time.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                if (player.ChampionLevel < player.ChampionMaxLevel)
                {
                    player.Out.SendMessage("For testing purposes I can [grant] you a free champion level.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                }
                if (player.Champion && player.ChampionLevel >= 5)
                {
                    player.Out.SendMessage("For testing purposes I can [respec] your champion abilities as well.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                }
            }
            else
            {
                player.Out.SendMessage("Come back when you are level 50.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
            }
            return true;
        }
        public override bool WhisperReceive(GameLiving source, string str)
        {

            if (!base.WhisperReceive(source, str))
                return false;
            GamePlayer player = source as GamePlayer;
            if (player == null) return false;
            if (str == "dostuff")
            {
                DoSomeStuff(15);
                return true;
            }
            if (player.Level != 50)
            {
                player.Out.SendMessage("Your not strong enough to embrace the life of champions!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                return false;
            }
            if (str == "champions")
            {
                if (player.Champion)
                {
                    player.Out.SendMessage("You are already a champion!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    return false;
                }
                player.Champion = true;
                player.SaveIntoDatabase();
                player.Out.SendMessage("You have just embraced on the life of the champions!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                Interact(player);
                return true;
            }
            if (str == "grant" && (player.Champion))
            {
                if (player.ChampionLevel == player.ChampionMaxLevel)
                {
                    player.Out.SendMessage("You are already at the maximum champion level!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    return true;
                }
                player.ChampionExperience = +player.ChampionExperienceForNextLevel;
                CheckPromoteChampion(player);
                return true;
            }

            //level respec for players
            if (str == "respec")
                player.Out.SendMessage("This will you cost 200 bps, [yes].", eChatType.CT_System, eChatLoc.CL_PopupWindow);
            if (str == "yes")
            {
                if (player.Champion && player.ChampionLevel >= 5)
                    player.RemoveBountyPoints(200);
                {
                    player.RemoveSpellLine(GlobalSpellsLines.Champion_Spells + player.Name);
                    SkillBase.UnRegisterSpellLine(GlobalSpellsLines.Champion_Spells + player.Name);
                    player.ChampionSpells = String.Empty;
                    player.ChampionSpecialtyPoints = player.ChampionLevel;
                    player.UpdateSpellLineLevels(true);
                    player.RefreshSpecDependantSkills(true);
                    player.Out.SendUpdatePlayer();
                    player.UpdatePlayerStatus();
                    player.Out.SendUpdatePlayerSkills();
                    player.SaveIntoDatabase();
                    //There is a bug here that old spells will come back once buying new ones untill relog of character.  you may want
                    //to force the player to relog after they respec champion spells.
                }
            }


            int ctype = 0;
            switch (player.Realm)
            {
                case eRealm.Albion:
                    {
                        switch (str)
                        {
                            //only Elementalist, Acolyte, Disciple, and Mage actually work, due to missing database entries, thats why they are the only choices offered by the NPC.

                            ///////////neue dazu

                            case "Fighter":
                               {
                                   ctype = 14;
                                }
                                break;
                            case "Albion Rogue":
                                {
                                    ctype = 2;
                                }
                                break;

                                ///////////neue dazu
                            case "Acolyte":
                                {
                                    ctype = 4;
                                }
                                break;
                            case "Elementalist":
                                {
                                    ctype = 5;
                                }
                                break;
                            case "Mage":
                                {
                                    ctype = 6;
                                }
                                break;
                            case "Disciple":
                                {
                                    ctype = 7;
                                }
                                break;
                        }
                    }
                    break;
                case eRealm.Hibernia:
                    {
                        switch (str)
                        {
                            //only Way of Nature and Way of Magic, and Grove actually work, due to missing database entries, thats why they are the only choices offered by the NPC.
                            case "Way of Arms":
                                {
                                    ctype = 1;//für druiden
                                }
                                break;
                            case "Way of Stealth":
                                {
                                    ctype = 2;
                                }
                                break;
                            case "Way of Nature":
                                {
                                    ctype = 10;
                                }
                                break;
                            case "Way of Magic":
                                {
                                    ctype = 11;
                                }
                                break;


                                ///////////neue dazu
                               
                                case "Way of the Grove":
                                {
                                    ctype = 12;
                                }
                                    break;
                               ///////////neue dazu
                        }
                    }
                    break;
                case eRealm.Midgard:
                    {
                        switch (str)
                        {
                            //only Seer and Mystic actually work, due to missing database entries, thats why they are the only choices offered by the NPC.
                            case "Viking":
                                {
                                    ctype = 13;
                                }
                                break;
                            ///////////neue dazu
                            case "Midgard Rogue":
                                {
                                    ctype = 3;
                                }
                                break;
                            ///////////neue dazu
                            case "Seer":
                                {
                                    ctype = 8;
                                }
                                break;
                            case "Mystic":
                                {
                                    ctype = 9;
                                }
                                break;
                        }
                    }
                    break;
            }
            if (ctype != 0)
            {
                player.TempProperties.setProperty("championtraining", ctype);
                player.Out.SendChampionTrainerWindow(ctype);
            }

            return true;
        }

        public void DoSomeStuff(int times)
        {
            int idlinecount = 1;
            for (int i = 0; i < times; i++)
            {
                int indexcount = 1;
                int skillindexcount = 1;
                for (int o = 0; o < 5; o++)
                {
                    DBChampSpecs s1 = new DBChampSpecs();
                    s1.ObjectId = idlinecount.ToString() + "-" + skillindexcount.ToString() + "-" + indexcount.ToString();
                    s1.IdLine = idlinecount;
                    s1.SkillIndex = skillindexcount;
                    s1.Index = indexcount;
                    s1.Cost = 1;
                    GameServer.Database.AddObject(s1);
                    indexcount++;
                }
                indexcount = 1;
                for (int o = 0; o < 5; o++)
                {
                    DBChampSpecs s1 = new DBChampSpecs();
                    s1.ObjectId = idlinecount.ToString() + "-" + (skillindexcount + 1).ToString() + "-" + indexcount.ToString();
                    s1.IdLine = idlinecount;
                    s1.SkillIndex = skillindexcount + 1;
                    s1.Index = indexcount;
                    s1.Cost = 1;
                    GameServer.Database.AddObject(s1);
                    indexcount++;
                }
                indexcount = 1;
                for (int o = 0; o < 2; o++)
                {
                    DBChampSpecs s1 = new DBChampSpecs();
                    s1.ObjectId = idlinecount.ToString() + "-" + (skillindexcount + 2).ToString() + "-" + indexcount.ToString();
                    s1.IdLine = idlinecount;
                    s1.SkillIndex = skillindexcount + 2;
                    s1.Index = indexcount;
                    s1.Cost = 1;
                    GameServer.Database.AddObject(s1);
                    indexcount++;
                }
                indexcount = 1;
                for (int o = 0; o < 3; o++)
                {
                    DBChampSpecs s1 = new DBChampSpecs();
                    s1.ObjectId = idlinecount.ToString() + "-" + (skillindexcount + 3).ToString() + "-" + indexcount.ToString();
                    s1.IdLine = idlinecount;
                    s1.SkillIndex = skillindexcount + 3;
                    s1.Index = indexcount;
                    s1.Cost = 1;
                    GameServer.Database.AddObject(s1);
                    indexcount++;
                }
                indexcount = 1;
                for (int o = 0; o < 2; o++)
                {
                    DBChampSpecs s1 = new DBChampSpecs();
                    s1.ObjectId = idlinecount.ToString() + "-" + (skillindexcount + 4).ToString() + "-" + indexcount.ToString();
                    s1.IdLine = idlinecount;
                    s1.SkillIndex = skillindexcount + 4;
                    s1.Index = indexcount;
                    s1.Cost = 1;
                    GameServer.Database.AddObject(s1);
                    indexcount++;
                }
                idlinecount++;

            }
        }


        protected void CheckPromoteChampion(GamePlayer player)
        {
            if (player.Champion)
            {
                bool cllevel = false;
                while (player.ChampionLevel < player.ChampionMaxLevel && player.ChampionExperience >= player.ChampionExperienceForNextLevel)
                {
                    player.ChampionLevelUp();
                    player.RemoveBountyPoints(500);
                    cllevel = true;
                }
                if (cllevel) //TODO: Out.Message (MLXP)
                    player.Out.SendMessage("You reached champion level " + player.ChampionLevel + "!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                //player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "KingNPC.WhisperReceive.NewLevelMessage"), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                return;
            }
        }

    }
}
