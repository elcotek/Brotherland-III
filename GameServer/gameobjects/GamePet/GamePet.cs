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
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;
using System;

namespace DOL.GS
{
    public class GamePet : GamePets
   {
      private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

      /// <summary>
      /// Create a Underhill Pet
      /// </summary>
      /// <param name="npcTemplate"></param>
      /// <param name="owner"></param>
      public GamePet(INpcTemplate npcTemplate)
         : base(npcTemplate)
      {
            switch (Name.ToLower())
            {
                case "underhill companion" :
                case "underhill zealot" :
                    {
                        UnderHillType = 2;
                        break;
                    }
                case "underhill ally":
                case "underhill compatriot":
                    {
                        UnderHillType = 3;
                        break;
                    }
                default: break;
            }
            UpdateModelStats();
      }

        // 1 = basic, 2 = caster, 3 = tanker
        public int UnderHillType = 1;
        public eRace UnderhillRace = eRace.Unknown;
        private readonly string templatedagger = "Enchanter_Pet_Dagger";
        private readonly string templatestaff = "Enchanter_Pet_Staff";
        private readonly string templateshield = "Enchanter_Pet_Shield";

        public bool UpdateModelStats()
        {
            switch (Model)
            {
                case 318:
                case 319:
                case 320:
                case 321:
                case 322:
                case 323:
                case 324:
                case 325:
                    {
                        UnderhillRace = eRace.Lurikeen;
                        Gender = eGender.Male;
                        break;
                    }
                case 326:
                case 327:
                case 328:
                case 329:
                case 330:
                case 331:
                case 332:
                case 333:
                    {
                        UnderhillRace = eRace.Lurikeen;
                        Gender = eGender.Female;
                        break;
                    }
                case 334:
                case 335:
                case 336:
                case 337:
                case 338:
                case 339:
                case 340:
                case 341:
                    {
                        UnderhillRace = eRace.Elf;
                        Gender = eGender.Male;
                        break;
                    }
                case 342:
                case 343:
                case 344:
                case 345:
                case 346:
                case 347:
                case 348:
                case 349:
                    {
                        UnderhillRace = eRace.Elf;
                        Gender = eGender.Female;
                        break;
                    }

                default: break;
            }
            return true;

        }
      /// <summary>
      /// Called when owner sends a whisper to the pet
      /// </summary>
      /// <param name="e"></param>
      /// <param name="sender"></param>
      /// <param name="arguments"></param>
      public override bool WhisperReceive(GameLiving source, string str)
      {
         GamePlayer player = source as GamePlayer;

            // Überprüfen, ob der Spieler null ist oder ob der Brain null oder nicht vom Typ IControlledBrain ist.
            // Wenn ja, wird false zurückgegeben, um einen möglichen Absturz zu vermeiden.
            if (this.InCombat || player == null || Brain == null || !(Brain is IControlledBrain controlledBrain) || player != controlledBrain.Owner)
                return false;

            // Überprüfung, ob der Pet-Name in der Liste der gültigen Namen ist
            if ( this.Name == "Underhill Companion" == false &&
                    this.Name == "Underhill Zealot" == false &&
                     this.Name == "Underhill Friend" == false &&
                     this.Name == "Underhill Compatriot" == false &&
                     this.Name == "Underhill Ally" == false)
              return false;


            // Aufteilen des Eingabestrings in Kleinbuchstaben       
            string[] strargs = str.ToLower().Split(' ');

         for (int i = 0; i < strargs.Length; i++)
         {
            String curStr = strargs[i];


           
            if (curStr == "underhill")
            {
                   
            
                    string gendermessage = "[male]";
                    if (Gender == eGender.Male)
                    {
                        gendermessage = "[female]";
                    }
                    string racemessage = "an [Elf]";
                    if (UnderhillRace == eRace.Elf)
                    {
                        racemessage = "a [Lurikeen]";
                    }
                  //  string cloakmessage = "If you can't stand my hair. I can always put my [cloak] up!";
                   /* 
                if ((IsCloakHoodUp))
                    {
                        cloakmessage = "I can put my hood [down]";
                    }
                */
               

                    string playermessage = "Greetings, friend. As one of the Underhill, I also have the ability to take the appearance of " + racemessage +
                        ". If you wish I can also can become a " + gendermessage + ". " +
                        
                        ". I also have several [combat] tactics at your disposal.";

                    player.Out.SendMessage(playermessage, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    break;
                }
             /*
                if (curStr == "cloak")
                {
                    if (!(IsCloakHoodUp))
                    {
                        IsCloakHoodUp = true;
                        BroadcastLivingEquipmentUpdate();
                        player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                    }
                    break;
                }
                if (curStr == "down")
                {
                    if ((IsCloakHoodUp))
                    {
                        IsCloakHoodUp = false;
                        BroadcastLivingEquipmentUpdate();
                        player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                    }
                    break;
                }
              */
                if (curStr == "male")
                {
                    if (Gender == eGender.Neutral)
                        break;
                    if (Gender == eGender.Male)
                        break;
                    Model = (ushort)(Model - 8);
                    UpdateModelStats();
                    player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                    break;
                }
                if (curStr == "female")
                {
                    if (Gender == eGender.Neutral)
                        break;
                    if (Gender == eGender.Female)
                        break;
                    Model = (ushort)(Model + 8);
                    UpdateModelStats();
                    player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                    break;
                }
                if (curStr == "elf")
                {
                    if (UnderhillRace == eRace.Unknown)
                        break;
                    if (UnderhillRace == eRace.Elf)
                        break;
                    Model = (ushort)(Model + 16);
                    UpdateModelStats();
                    player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                    break;
                }
                if (curStr == "lurikeen")
                {
                    if (UnderhillRace == eRace.Unknown)
                        break;
                    if (UnderhillRace == eRace.Lurikeen)
                        break;
                    Model = (ushort)(Model - 16);
                    UpdateModelStats();
                    player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                    break;
                }

                if (curStr == "combat" && UnderHillType == 1)
                {
                    player.Out.SendMessage("You may bid me to [parry] with my weapon(s) or if you wish you may command me to [cease] parrying at any time."+
                        "If you wish it, I am able to [assist] all your attacks or [taunt] your enemies so that they will focus me instead of yourself.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                }
                if (curStr == "combat" && UnderHillType == 2)
                {
                    player.Out.SendMessage("If you wish it, I am able to [assist] all your attacks or [taunt] your enemies so that they will focus me instead of yourself." +
                        "I can also equip a [dagger] or a [staff] as you wish.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                }
                if (curStr == "combat" && UnderHillType == 3)
                {
                    player.Out.SendMessage("You may bid me to [parry] with my weapon(s) or if you wish you may command me to [cease] parrying at any time."+
                        "I can also equip a [shield] or [unequip] it just as easy. I am able to [assist] all your attacks or [taunt] your enemies so that they will focus me instead of yourself.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                }
                if (curStr == "parry")
                {
                    if (UnderHillType == 1 || UnderHillType == 3)
                    {
                        ParryChance = 10;
                        player.Out.SendMessage("As you wished. I am now parrying", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
                if (curStr == "assist")
                {
                    string name = String.Empty;
                    if(player.TargetObject != null)
                    name = player.TargetObject.Name;
                    //assist on/off
                    if (EnableAssist == false)
                    {
                        EnableAssist = true;
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.Assist.Text", name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        break;
                    }
                    else
                    {
                        EnableAssist = false;
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.AssistOff.Text", name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        break;
                    }
                }
                if (curStr == "cease")
                {
                    if (UnderHillType == 1 || UnderHillType == 3)
                    {
                        ParryChance = 0;
                        player.Out.SendMessage("As you wished. I can nolonger parry", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
                if (curStr == "shield")
                {
                    if (UnderHillType == 3)
                    {
                        if (ActiveWeaponSlot == eActiveWeaponSlot.TwoHanded)
                        {
                            UnderhillSwitchWeapon(eInventorySlot.LeftHandWeapon, eActiveWeaponSlot.Standard, templateshield);
                            BlockChance = 10;
                            player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                        }
                        else
                        {
                            UnderhillSwitchWeapon(eInventorySlot.LeftHandWeapon, eActiveWeaponSlot.Standard, templateshield);
                            BlockChance = 10;
                            player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                        }
                        break;
                    }
                    break;
                }
                if (curStr == "unequip")
                {
                    if (UnderHillType == 3)
                    {
                        InventoryItem item = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
                        if (item != null)
                        {
                            Inventory.RemoveItem(item);
                            BroadcastLivingEquipmentUpdate();
                            BlockChance = 0;
                            player.Out.SendMessage("As you wished. I will no longer block.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                        }
                    }
                    break;
                }
                if (curStr == "staff")
                {
                    if (UnderHillType == 2)
                    {
                        if (ActiveWeaponSlot == eActiveWeaponSlot.TwoHanded)
                        {
                            UnderhillSwitchWeapon(eInventorySlot.RightHandWeapon, eActiveWeaponSlot.Standard, templatestaff);
                            player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                        }
                        break;
                    }
                    break;
                }
                if (curStr == "dagger")
                {
                    if (UnderHillType == 2)
                    {
                        if (ActiveWeaponSlot == eActiveWeaponSlot.TwoHanded)
                        {
                            UnderhillSwitchWeapon(eInventorySlot.RightHandWeapon, eActiveWeaponSlot.Standard, templatedagger);
                            player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                        }
                        else
                        {
                            UnderhillSwitchWeapon(eInventorySlot.RightHandWeapon, eActiveWeaponSlot.Standard, templatedagger);
                            player.Out.SendMessage("As you wished.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                        }
                        break;
                    }
                }
            if (curStr == "taunt")
            {
               bool found = false;
               foreach (Spell spell in Spells)
               {
                  //If the taunt spell's ID is changed - this needs to be changed
                  if (spell.ID == 60127)
                  {
                     Spells.Remove(spell);
                     player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.CommNoTaunt"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                     found = true;
                     break;
                  }
               }
               if (found) break;
               //TODO: change this so it isn't hardcoded
               Spell tauntspell = SkillBase.GetSpellByID(60127);
               player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.CommStartTaunt"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
               if (tauntspell != null)
                  Spells.Add(tauntspell);
               else
                  Console.WriteLine("Couldn't find pet's taunt spell");
               break;
            }

         }
         return base.WhisperReceive(source, str);
      }

      /// <summary>
      /// Changes the commander's weapon to the specified type
      /// </summary>
      /// <param name="slot"></param>
      /// <param name="aSlot"></param>
      /// <param name="weaponType"></param>
      protected void UnderhillSwitchWeapon(eInventorySlot slot, eActiveWeaponSlot aSlot, string weapon)
      {
         if (Inventory == null)
            return;
         //all weapons removed before
            if (slot == eInventorySlot.LeftHandWeapon)
            {
                InventoryItem item = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
                if (item != null) Inventory.RemoveItem(item);
            }
            else
            {
                InventoryItem item = Inventory.GetItem(eInventorySlot.RightHandWeapon);
                if (item != null) Inventory.RemoveItem(item);
                item = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
                if (item != null) Inventory.RemoveItem(item);
                item = Inventory.GetItem(eInventorySlot.DistanceWeapon);
                if (item != null) Inventory.RemoveItem(item);
                item = Inventory.GetItem(eInventorySlot.TwoHandWeapon);
                if (item != null) Inventory.RemoveItem(item);
            }

         ItemTemplate temp = GameServer.Database.FindObjectByKey<ItemTemplate>(weapon);

         if (temp == null)
         {
            if (log.IsErrorEnabled)
               log.Error(string.Format("Unable to find Underhill item: {0}", weapon));
            return;
         }

         Inventory.AddItem(slot, GameInventoryItem.Create<ItemTemplate>(temp));
         SwitchWeapon(aSlot);
         AddStatsToWeapon();
         BroadcastLivingEquipmentUpdate();
      }
   }
}